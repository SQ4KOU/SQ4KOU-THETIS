using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class CommonCompiler
{
	private sealed class CompilerEmitStreamProvider : Compilation.EmitStreamProvider
	{
		private readonly CommonCompiler _compiler;

		private readonly string _filePath;

		private Stream? _streamToDispose;

		internal CompilerEmitStreamProvider(CommonCompiler compiler, string filePath)
		{
			_compiler = compiler;
			_filePath = filePath;
		}

		public void Close(DiagnosticBag diagnostics)
		{
			try
			{
				_streamToDispose?.Dispose();
			}
			catch (Exception ex)
			{
				CommonMessageProvider messageProvider = _compiler.MessageProvider;
				DiagnosticInfo info = new DiagnosticInfo(messageProvider, messageProvider.ERR_OutputWriteFailed, _filePath, ex.Message);
				diagnostics.Add(messageProvider.CreateDiagnostic(info));
			}
		}

		protected override Stream? CreateStream(DiagnosticBag diagnostics)
		{
			try
			{
				try
				{
					return OpenFileStream();
				}
				catch (IOException ex)
				{
					try
					{
						if (PathUtilities.IsUnixLikePlatform)
						{
							File.Delete(_filePath);
						}
						else if (ex.HResult == -2147024864)
						{
							string text = Path.Combine(Path.GetDirectoryName(_filePath), Guid.NewGuid().ToString() + "_" + Path.GetFileName(_filePath));
							File.Move(_filePath, text);
							File.SetAttributes(text, FileAttributes.Hidden);
							File.Delete(text);
						}
					}
					catch
					{
						ReportOpenFileDiagnostic(diagnostics, ex);
						return null;
					}
					return OpenFileStream();
				}
			}
			catch (Exception e)
			{
				ReportOpenFileDiagnostic(diagnostics, e);
				return null;
			}
		}

		private Stream OpenFileStream()
		{
			return _streamToDispose = _compiler.FileSystem.OpenFile(_filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		}

		private void ReportOpenFileDiagnostic(DiagnosticBag diagnostics, Exception e)
		{
			CommonMessageProvider messageProvider = _compiler.MessageProvider;
			ImmutableArray<(int, string)> immutableArray = FileLockCheck.TryGetLockingProcessInfos(_filePath);
			object obj = (immutableArray.IsEmpty ? ((object)e.Message) : ((object)new LocalizableResourceString("ExceptionMessage_FileMayBeLockedBy", CodeAnalysisResources.ResourceManager, typeof(CodeAnalysisResources), e.Message, string.Join(", ", immutableArray.Select<(int, string), string>(((int processId, string applicationName) info) => $"'{info.applicationName}' ({info.processId})")))));
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_CantOpenFileWrite, Location.None, _filePath, obj));
		}
	}

	internal sealed class CompilerRelativePathResolver : RelativePathResolver
	{
		internal ICommonCompilerFileSystem FileSystem { get; }

		internal CompilerRelativePathResolver(ICommonCompilerFileSystem fileSystem, ImmutableArray<string> searchPaths, string? baseDirectory)
			: base(searchPaths, baseDirectory)
		{
			FileSystem = fileSystem;
		}

		protected override bool FileExists(string fullPath)
		{
			return FileSystem.FileExists(fullPath);
		}
	}

	internal sealed class ExistingReferencesResolver : MetadataReferenceResolver, IEquatable<ExistingReferencesResolver>
	{
		private readonly MetadataReferenceResolver _resolver;

		private readonly ImmutableArray<MetadataReference> _availableReferences;

		private readonly Lazy<HashSet<AssemblyIdentity>> _lazyAvailableReferences;

		public ExistingReferencesResolver(MetadataReferenceResolver resolver, ImmutableArray<MetadataReference> availableReferences)
		{
			_resolver = resolver;
			_availableReferences = availableReferences;
			_lazyAvailableReferences = new Lazy<HashSet<AssemblyIdentity>>(() => new HashSet<AssemblyIdentity>(from reference in _availableReferences
				let identity = TryGetIdentity(reference)
				where identity != null
				select identity));
		}

		public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties)
		{
			return _resolver.ResolveReference(reference, baseFilePath, properties).WhereAsArray((PortableExecutableReference r) => _lazyAvailableReferences.Value.Contains(TryGetIdentity(r)));
		}

		private static AssemblyIdentity? TryGetIdentity(MetadataReference metadataReference)
		{
			if (!(metadataReference is PortableExecutableReference { Properties: { Kind: MetadataImageKind.Assembly } } portableExecutableReference))
			{
				return null;
			}
			try
			{
				return ((AssemblyMetadata)portableExecutableReference.GetMetadataNoCopy()).GetAssembly().Identity;
			}
			catch (Exception ex) when (ex is BadImageFormatException || ex is IOException)
			{
				return null;
			}
		}

		public override int GetHashCode()
		{
			return _resolver.GetHashCode();
		}

		public bool Equals(ExistingReferencesResolver? other)
		{
			if (other != null && _resolver.Equals(other._resolver))
			{
				return _availableReferences.SequenceEqual(other._availableReferences);
			}
			return false;
		}

		public override bool Equals(object? other)
		{
			if (other is ExistingReferencesResolver other2)
			{
				return Equals(other2);
			}
			return false;
		}
	}

	internal sealed class LoggingMetadataFileReferenceResolver : MetadataReferenceResolver, IEquatable<LoggingMetadataFileReferenceResolver>
	{
		private readonly TouchedFileLogger? _logger;

		private readonly RelativePathResolver _pathResolver;

		private readonly Func<string, MetadataReferenceProperties, PortableExecutableReference> _provider;

		public LoggingMetadataFileReferenceResolver(RelativePathResolver pathResolver, Func<string, MetadataReferenceProperties, PortableExecutableReference> provider, TouchedFileLogger? logger)
		{
			_pathResolver = pathResolver;
			_provider = provider;
			_logger = logger;
		}

		public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties)
		{
			string text = _pathResolver.ResolvePath(reference, baseFilePath);
			if (text != null)
			{
				_logger?.AddRead(text);
				return ImmutableArray.Create(_provider(text, properties));
			}
			return ImmutableArray<PortableExecutableReference>.Empty;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public bool Equals(LoggingMetadataFileReferenceResolver? other)
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object? obj)
		{
			if (obj is LoggingMetadataFileReferenceResolver other)
			{
				return Equals(other);
			}
			return false;
		}
	}

	internal sealed class LoggingSourceFileResolver : SourceFileResolver
	{
		private readonly TouchedFileLogger? _logger;

		public LoggingSourceFileResolver(ImmutableArray<string> searchPaths, string? baseDirectory, ImmutableArray<KeyValuePair<string, string>> pathMap, TouchedFileLogger? logger)
			: base(searchPaths, baseDirectory, pathMap)
		{
			_logger = logger;
		}

		protected override bool FileExists(string? fullPath)
		{
			if (fullPath != null)
			{
				_logger?.AddRead(fullPath);
			}
			return base.FileExists(fullPath);
		}

		public LoggingSourceFileResolver WithBaseDirectory(string value)
		{
			if (!(base.BaseDirectory == value))
			{
				return new LoggingSourceFileResolver(base.SearchPaths, value, base.PathMap, _logger);
			}
			return this;
		}

		public LoggingSourceFileResolver WithSearchPaths(ImmutableArray<string> value)
		{
			if (!(base.SearchPaths == value))
			{
				return new LoggingSourceFileResolver(value, base.BaseDirectory, base.PathMap, _logger);
			}
			return this;
		}
	}

	internal sealed class LoggingStrongNameFileSystem : StrongNameFileSystem
	{
		private readonly TouchedFileLogger? _loggerOpt;

		public LoggingStrongNameFileSystem(TouchedFileLogger? logger, string? customTempPath)
			: base(customTempPath)
		{
			_loggerOpt = logger;
		}

		internal override bool FileExists(string? fullPath)
		{
			if (fullPath != null)
			{
				_loggerOpt?.AddRead(fullPath);
			}
			return base.FileExists(fullPath);
		}

		internal override byte[] ReadAllBytes(string fullPath)
		{
			_loggerOpt?.AddRead(fullPath);
			return base.ReadAllBytes(fullPath);
		}
	}

	internal sealed class LoggingXmlFileResolver : XmlFileResolver
	{
		private readonly TouchedFileLogger? _logger;

		public LoggingXmlFileResolver(string? baseDirectory, TouchedFileLogger? logger)
			: base(baseDirectory)
		{
			_logger = logger;
		}

		protected override bool FileExists(string? fullPath)
		{
			if (fullPath != null)
			{
				_logger?.AddRead(fullPath);
			}
			return base.FileExists(fullPath);
		}
	}

	private sealed class SuppressionDiagnostic : Diagnostic
	{
		private static readonly DiagnosticDescriptor s_suppressionDiagnosticDescriptor = new DiagnosticDescriptor("SP0001", CodeAnalysisResources.SuppressionDiagnosticDescriptorTitle, CodeAnalysisResources.SuppressionDiagnosticDescriptorMessage, "ProgrammaticSuppression", DiagnosticSeverity.Info, true, null, null);

		private readonly Diagnostic _originalDiagnostic;

		private readonly string _suppressionId;

		private readonly LocalizableString _suppressionJustification;

		public override DiagnosticDescriptor Descriptor => s_suppressionDiagnosticDescriptor;

		public override string Id => Descriptor.Id;

		public override DiagnosticSeverity Severity => DiagnosticSeverity.Info;

		public override bool IsSuppressed => false;

		public override int WarningLevel => Diagnostic.GetDefaultWarningLevel(DiagnosticSeverity.Info);

		public override Location Location => _originalDiagnostic.Location;

		public override IReadOnlyList<Location> AdditionalLocations => _originalDiagnostic.AdditionalLocations;

		public override ImmutableDictionary<string, string?> Properties => ImmutableDictionary<string, string>.Empty;

		public SuppressionDiagnostic(Diagnostic originalDiagnostic, string suppressionId, LocalizableString suppressionJustification)
		{
			_originalDiagnostic = originalDiagnostic;
			_suppressionId = suppressionId;
			_suppressionJustification = suppressionJustification;
		}

		public override string GetMessage(IFormatProvider? formatProvider = null)
		{
			string format = s_suppressionDiagnosticDescriptor.MessageFormat.ToString(formatProvider);
			return string.Format(formatProvider, format, _originalDiagnostic.Id, _originalDiagnostic.GetMessage(formatProvider), _suppressionId, _suppressionJustification.ToString(formatProvider));
		}

		public override bool Equals(Diagnostic? obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (!(obj is SuppressionDiagnostic suppressionDiagnostic))
			{
				return false;
			}
			if (object.Equals(_originalDiagnostic, suppressionDiagnostic._originalDiagnostic) && object.Equals(_suppressionId, suppressionDiagnostic._suppressionId))
			{
				return object.Equals(_suppressionJustification, suppressionDiagnostic._suppressionJustification);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(_originalDiagnostic.GetHashCode(), Hash.Combine(_suppressionId.GetHashCode(), _suppressionJustification.GetHashCode()));
		}

		internal override Diagnostic WithLocation(Location location)
		{
			throw new NotSupportedException();
		}

		internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
		{
			throw new NotSupportedException();
		}

		internal override Diagnostic WithIsSuppressed(bool isSuppressed)
		{
			throw new NotSupportedException();
		}
	}

	internal const int Failed = 1;

	internal const int Succeeded = 0;

	private readonly Lazy<Encoding> _fallbackEncoding = new Lazy<Encoding>(EncodedStringText.CreateFallbackEncoding);

	private readonly HashSet<Diagnostic> _reportedDiagnostics = new HashSet<Diagnostic>();

	public CommonMessageProvider MessageProvider { get; }

	public CommandLineArguments Arguments { get; }

	public IAnalyzerAssemblyLoader AssemblyLoader { get; private set; }

	public GeneratorDriverCache? GeneratorDriverCache { get; }

	public abstract DiagnosticFormatter DiagnosticFormatter { get; }

	public IReadOnlySet<string> EmbeddedSourcePaths { get; }

	internal ICommonCompilerFileSystem FileSystem { get; set; }

	internal abstract Type Type { get; }

	protected virtual CultureInfo Culture => Arguments.PreferredUILang ?? CultureInfo.CurrentUICulture;

	public abstract Compilation? CreateCompilation(TextWriter consoleOutput, TouchedFileLogger? touchedFilesLogger, ErrorLogger? errorLoggerOpt, ImmutableArray<AnalyzerConfigOptionsResult> analyzerConfigOptions, AnalyzerConfigOptionsResult globalConfigOptions);

	public abstract void PrintLogo(TextWriter consoleOutput);

	public abstract void PrintHelp(TextWriter consoleOutput);

	public abstract void PrintLangVersions(TextWriter consoleOutput);

	public virtual void PrintVersion(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(GetCompilerVersion());
	}

	protected abstract bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code);

	protected abstract void ResolveAnalyzersFromArguments(List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider, CompilationOptions compilationOptions, bool skipAnalyzers, out ImmutableArray<DiagnosticAnalyzer> analyzers, out ImmutableArray<ISourceGenerator> generators);

	public CommonCompiler(CommandLineParser parser, string? responseFile, string[] args, BuildPaths buildPaths, string? additionalReferenceDirectories, IAnalyzerAssemblyLoader assemblyLoader, GeneratorDriverCache? driverCache, ICommonCompilerFileSystem? fileSystem)
	{
		IEnumerable<string> enumerable = args;
		if (!SuppressDefaultResponseFile(args) && File.Exists(responseFile))
		{
			enumerable = new string[1] { "@" + responseFile }.Concat(enumerable);
		}
		Arguments = parser.Parse(enumerable, buildPaths.WorkingDirectory, buildPaths.SdkDirectory, additionalReferenceDirectories);
		MessageProvider = parser.MessageProvider;
		AssemblyLoader = assemblyLoader;
		GeneratorDriverCache = driverCache;
		EmbeddedSourcePaths = GetEmbeddedSourcePaths(Arguments);
		FileSystem = fileSystem ?? StandardFileSystem.Instance;
	}

	internal abstract bool SuppressDefaultResponseFile(IEnumerable<string> args);

	internal string GetCompilerVersion()
	{
		return GetProductVersion(Type);
	}

	internal static string GetProductVersion(Type type)
	{
		string? informationalVersionWithoutHash = GetInformationalVersionWithoutHash(type);
		string text = type.Assembly.GetCustomAttribute<CommitHashAttribute>()?.Hash;
		return informationalVersionWithoutHash + " (" + text + ")";
	}

	private static string? GetInformationalVersionWithoutHash(Type type)
	{
		AssemblyInformationalVersionAttribute? customAttribute = type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (customAttribute == null)
		{
			return null;
		}
		return customAttribute.InformationalVersion.Split(new char[1] { '+' })[0];
	}

	internal static string GetAssemblyLocation(Type type)
	{
		string location = type.Assembly.Location;
		if (!string.IsNullOrEmpty(location))
		{
			return location;
		}
		return "<unknown>";
	}

	internal abstract string GetToolName();

	internal Version? GetAssemblyVersion()
	{
		return Type.GetTypeInfo().Assembly.GetName().Version;
	}

	internal string GetCultureName()
	{
		return Culture.Name;
	}

	internal virtual Func<string, MetadataReferenceProperties, PortableExecutableReference> GetMetadataProvider()
	{
		return (string path, MetadataReferenceProperties properties) => MetadataReference.CreateFromFile(FileSystem.OpenFileWithNormalizedException(path, FileMode.Open, FileAccess.Read, FileShare.Read), path, PEStreamOptions.PrefetchEntireImage, properties);
	}

	internal virtual MetadataReferenceResolver GetCommandLineMetadataReferenceResolver(TouchedFileLogger? loggerOpt)
	{
		return new LoggingMetadataFileReferenceResolver(new CompilerRelativePathResolver(FileSystem, Arguments.ReferencePaths, Arguments.BaseDirectory), GetMetadataProvider(), loggerOpt);
	}

	internal List<MetadataReference> ResolveMetadataReferences(List<DiagnosticInfo> diagnostics, TouchedFileLogger? touchedFiles, out MetadataReferenceResolver referenceDirectiveResolver)
	{
		MetadataReferenceResolver commandLineMetadataReferenceResolver = GetCommandLineMetadataReferenceResolver(touchedFiles);
		List<MetadataReference> list = new List<MetadataReference>();
		Arguments.ResolveMetadataReferences(commandLineMetadataReferenceResolver, diagnostics, MessageProvider, list);
		if (Arguments.IsScriptRunner)
		{
			referenceDirectiveResolver = commandLineMetadataReferenceResolver;
		}
		else
		{
			referenceDirectiveResolver = new ExistingReferencesResolver(commandLineMetadataReferenceResolver, list.ToImmutableArray());
		}
		return list;
	}

	internal SourceText? TryReadFileContent(CommandLineSourceFile file, IList<DiagnosticInfo> diagnostics)
	{
		string normalizedFilePath;
		return TryReadFileContent(file, diagnostics, out normalizedFilePath);
	}

	internal SourceText? TryReadFileContent(CommandLineSourceFile file, IList<DiagnosticInfo> diagnostics, out string? normalizedFilePath)
	{
		string path = file.Path;
		try
		{
			if (file.IsInputRedirected)
			{
				using (Stream stream = Console.OpenStandardInput())
				{
					normalizedFilePath = path;
					return EncodedStringText.Create(stream, _fallbackEncoding, Arguments.Encoding, Arguments.ChecksumAlgorithm, EmbeddedSourcePaths.Contains(file.Path));
				}
			}
			using Stream stream2 = OpenFileForReadWithSmallBufferOptimization(path, out normalizedFilePath);
			return EncodedStringText.Create(stream2, _fallbackEncoding, Arguments.Encoding, Arguments.ChecksumAlgorithm, EmbeddedSourcePaths.Contains(file.Path));
		}
		catch (Exception e)
		{
			diagnostics.Add(ToFileReadDiagnostics(MessageProvider, e, path));
			normalizedFilePath = null;
			return null;
		}
	}

	internal bool TryGetAnalyzerConfigSet(ImmutableArray<string> analyzerConfigPaths, DiagnosticBag diagnostics, [NotNullWhen(true)] out AnalyzerConfigSet? analyzerConfigSet)
	{
		ArrayBuilder<AnalyzerConfig> instance = ArrayBuilder<AnalyzerConfig>.GetInstance(analyzerConfigPaths.Length);
		PooledHashSet<string> instance2 = PooledHashSet<string>.GetInstance();
		foreach (string item in analyzerConfigPaths)
		{
			string text = TryReadFileContent(item, diagnostics, out string normalizedPath);
			if (text == null)
			{
				break;
			}
			string text2 = Path.GetDirectoryName(normalizedPath) ?? normalizedPath;
			AnalyzerConfig analyzerConfig = AnalyzerConfig.Parse(text, normalizedPath);
			if (!analyzerConfig.IsGlobal)
			{
				if (instance2.Contains(text2))
				{
					diagnostics.Add(Diagnostic.Create(MessageProvider, MessageProvider.ERR_MultipleAnalyzerConfigsInSameDir, text2));
					break;
				}
				instance2.Add(text2);
			}
			instance.Add(analyzerConfig);
		}
		instance2.Free();
		if (diagnostics.HasAnyErrors())
		{
			instance.Free();
			analyzerConfigSet = null;
			return false;
		}
		analyzerConfigSet = AnalyzerConfigSet.Create(instance, out ImmutableArray<Diagnostic> diagnostics2);
		diagnostics.AddRange(diagnostics2);
		return true;
	}

	internal Encoding? GetFallbackEncoding()
	{
		if (_fallbackEncoding.IsValueCreated)
		{
			return _fallbackEncoding.Value;
		}
		return null;
	}

	private string? TryReadFileContent(string filePath, DiagnosticBag diagnostics, out string? normalizedPath)
	{
		try
		{
			using StreamReader streamReader = new StreamReader(OpenFileForReadWithSmallBufferOptimization(filePath, out normalizedPath), Encoding.UTF8);
			return streamReader.ReadToEnd();
		}
		catch (Exception e)
		{
			diagnostics.Add(Diagnostic.Create(ToFileReadDiagnostics(MessageProvider, e, filePath)));
			normalizedPath = null;
			return null;
		}
	}

	private Stream OpenFileForReadWithSmallBufferOptimization(string filePath, out string normalizedFilePath)
	{
		return FileSystem.OpenFileEx(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, FileOptions.None, out normalizedFilePath);
	}

	internal EmbeddedText? TryReadEmbeddedFileContent(string filePath, DiagnosticBag diagnostics)
	{
		try
		{
			string normalizedFilePath;
			using Stream stream = OpenFileForReadWithSmallBufferOptimization(filePath, out normalizedFilePath);
			if (stream.Length < 81920 && EncodedStringText.TryGetBytesFromStream(stream, out var bytes))
			{
				return EmbeddedText.FromBytes(filePath, bytes, Arguments.ChecksumAlgorithm);
			}
			return EmbeddedText.FromStream(filePath, stream, Arguments.ChecksumAlgorithm);
		}
		catch (Exception e)
		{
			diagnostics.Add(MessageProvider.CreateDiagnostic(ToFileReadDiagnostics(MessageProvider, e, filePath)));
			return null;
		}
	}

	private ImmutableArray<EmbeddedText?> AcquireEmbeddedTexts(Compilation compilation, DiagnosticBag diagnostics)
	{
		if (Arguments.EmbeddedFiles.IsEmpty)
		{
			return ImmutableArray<EmbeddedText>.Empty;
		}
		Dictionary<string, SyntaxTree> dictionary = new Dictionary<string, SyntaxTree>(Arguments.EmbeddedFiles.Length);
		OrderedSet<string> orderedSet = new OrderedSet<string>(Arguments.EmbeddedFiles.Select((CommandLineSourceFile e) => e.Path));
		foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
		{
			if (EmbeddedSourcePaths.Contains(syntaxTree.FilePath) && !dictionary.ContainsKey(syntaxTree.FilePath))
			{
				dictionary.Add(syntaxTree.FilePath, syntaxTree);
				ResolveEmbeddedFilesFromExternalSourceDirectives(syntaxTree, compilation.Options.SourceReferenceResolver, orderedSet, diagnostics);
			}
		}
		ImmutableArray<EmbeddedText>.Builder builder = ImmutableArray.CreateBuilder<EmbeddedText>(orderedSet.Count);
		foreach (string item2 in orderedSet)
		{
			EmbeddedText item = ((!dictionary.TryGetValue(item2, out var value)) ? TryReadEmbeddedFileContent(item2, diagnostics) : EmbeddedText.FromSource(item2, value.GetText()));
			builder.Add(item);
		}
		return builder.MoveToImmutable();
	}

	protected abstract void ResolveEmbeddedFilesFromExternalSourceDirectives(SyntaxTree tree, SourceReferenceResolver resolver, OrderedSet<string> embeddedFiles, DiagnosticBag diagnostics);

	private static IReadOnlySet<string> GetEmbeddedSourcePaths(CommandLineArguments arguments)
	{
		if (arguments.EmbeddedFiles.IsEmpty)
		{
			return SpecializedCollections.EmptyReadOnlySet<string>();
		}
		HashSet<string> hashSet = new HashSet<string>(arguments.EmbeddedFiles.Select((CommandLineSourceFile f) => f.Path));
		hashSet.IntersectWith(arguments.SourceFiles.Select((CommandLineSourceFile f) => f.Path));
		return SpecializedCollections.StronglyTypedReadOnlySet(hashSet);
	}

	internal static DiagnosticInfo ToFileReadDiagnostics(CommonMessageProvider messageProvider, Exception e, string filePath)
	{
		if (e is FileNotFoundException || e is DirectoryNotFoundException)
		{
			return new DiagnosticInfo(messageProvider, messageProvider.ERR_FileNotFound, filePath);
		}
		if (e is InvalidDataException)
		{
			return new DiagnosticInfo(messageProvider, messageProvider.ERR_BinaryFile, filePath);
		}
		return new DiagnosticInfo(messageProvider, messageProvider.ERR_NoSourceFile, filePath, e.Message);
	}

	internal bool ReportDiagnostics(IEnumerable<Diagnostic> diagnostics, TextWriter consoleOutput, ErrorLogger? errorLoggerOpt, Compilation? compilation)
	{
		bool hasErrors = false;
		foreach (Diagnostic diagnostic in diagnostics)
		{
			reportDiagnostic(diagnostic, (compilation == null) ? null : diagnostic.GetSuppressionInfo(compilation));
		}
		return hasErrors;
		void reportDiagnostic(Diagnostic diag, SuppressionInfo? suppressionInfo)
		{
			if (!_reportedDiagnostics.Contains(diag) && diag.Severity != DiagnosticSeverity.Hidden)
			{
				errorLoggerOpt?.LogDiagnostic(diag, suppressionInfo);
				if (diag.ProgrammaticSuppressionInfo != null)
				{
					foreach (Suppression suppression in diag.ProgrammaticSuppressionInfo.Suppressions)
					{
						SuppressionDiagnostic suppressionDiagnostic = new SuppressionDiagnostic(diag, suppression.Descriptor.Id, suppression.Descriptor.Justification);
						if (_reportedDiagnostics.Add(suppressionDiagnostic))
						{
							PrintError(suppressionDiagnostic, consoleOutput);
						}
					}
					_reportedDiagnostics.Add(diag);
				}
				else if (!diag.IsSuppressed)
				{
					if (diag.Severity == DiagnosticSeverity.Error)
					{
						hasErrors = true;
					}
					PrintError(diag, consoleOutput);
					_reportedDiagnostics.Add(diag);
				}
			}
		}
	}

	private bool ReportDiagnostics(DiagnosticBag diagnostics, TextWriter consoleOutput, ErrorLogger? errorLoggerOpt, Compilation? compilation)
	{
		return ReportDiagnostics(diagnostics.ToReadOnly(), consoleOutput, errorLoggerOpt, compilation);
	}

	internal bool ReportDiagnostics(IEnumerable<DiagnosticInfo> diagnostics, TextWriter consoleOutput, ErrorLogger? errorLoggerOpt, Compilation? compilation)
	{
		return ReportDiagnostics(diagnostics.Select((DiagnosticInfo info) => Diagnostic.Create(info)), consoleOutput, errorLoggerOpt, compilation);
	}

	private void ReportIVTInfos(TextWriter consoleOutput, ErrorLogger? errorLogger, Compilation compilation, ImmutableArray<Diagnostic> diagnostics)
	{
		DiagnoseBadAccesses(consoleOutput, errorLogger, compilation, diagnostics);
		consoleOutput.WriteLine();
		consoleOutput.WriteLine(CodeAnalysisResources.InternalsVisibleToHeaderSummary);
		IAssemblySymbol assembly = compilation.Assembly;
		IAssemblySymbolInternal symbolInternal = compilation.GetSymbolInternal<IAssemblySymbolInternal>(assembly);
		consoleOutput.WriteLine(string.Format(CodeAnalysisResources.InternalsVisibleToCurrentAssembly, assembly.Identity.GetDisplayName(fullKey: true)));
		consoleOutput.WriteLine();
		foreach (IAssemblySymbol item in assembly.Modules.First().ReferencedAssemblySymbols.OrderBy((IAssemblySymbol a) => a.Name))
		{
			IAssemblySymbolInternal symbolInternal2 = compilation.GetSymbolInternal<IAssemblySymbolInternal>(item);
			bool flag = symbolInternal.AreInternalsVisibleToThisAssembly(symbolInternal2);
			consoleOutput.WriteLine(string.Format(CodeAnalysisResources.InternalsVisibleToReferencedAssembly, item.Identity.GetDisplayName(fullKey: true), flag));
			IEnumerable<string> internalsVisibleToAssemblyNames = symbolInternal2.GetInternalsVisibleToAssemblyNames();
			if (internalsVisibleToAssemblyNames.Any())
			{
				foreach (string item2 in internalsVisibleToAssemblyNames.OrderBy((string n) => n))
				{
					consoleOutput.WriteLine(string.Format(CodeAnalysisResources.InternalsVisibleToReferencedAssemblyDetails, item2));
					foreach (string item3 in from k in symbolInternal2.GetInternalsVisibleToPublicKeys(item2)
						select AssemblyIdentity.PublicKeyToString(k) into k
						orderby k
						select k)
					{
						consoleOutput.Write("      ");
						consoleOutput.WriteLine(item3);
					}
				}
			}
			else
			{
				consoleOutput.Write("    ");
				consoleOutput.WriteLine(CodeAnalysisResources.Nothing);
			}
			consoleOutput.WriteLine();
		}
	}

	private protected abstract void DiagnoseBadAccesses(TextWriter consoleOutput, ErrorLogger? errorLogger, Compilation compilation, ImmutableArray<Diagnostic> diagnostics);

	internal static bool HasUnsuppressableErrors(DiagnosticBag diagnostics)
	{
		foreach (Diagnostic item in diagnostics.AsEnumerable())
		{
			if (item.IsUnsuppressableError())
			{
				return true;
			}
		}
		return false;
	}

	internal static bool HasSuppressableWarningsOrErrors(DiagnosticBag diagnostics)
	{
		foreach (Diagnostic item in diagnostics.AsEnumerable())
		{
			if (!item.IsUnsuppressableError())
			{
				return true;
			}
		}
		return false;
	}

	internal static bool HasUnsuppressedErrors(DiagnosticBag diagnostics)
	{
		foreach (Diagnostic item in diagnostics.AsEnumerable())
		{
			if (item.IsUnsuppressedError)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void PrintError(Diagnostic diagnostic, TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(DiagnosticFormatter.Format(diagnostic, Culture));
	}

	public SarifErrorLogger? GetErrorLogger(TextWriter consoleOutput)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		Stream stream = OpenFile(Arguments.ErrorLogOptions.Path, instance, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
		SarifErrorLogger sarifErrorLogger;
		if (stream == null)
		{
			sarifErrorLogger = null;
		}
		else
		{
			string toolName = GetToolName();
			string compilerVersion = GetCompilerVersion();
			Version toolAssemblyVersion = GetAssemblyVersion() ?? new Version();
			sarifErrorLogger = ((Arguments.ErrorLogOptions.SarifVersion != SarifVersion.Sarif1) ? ((SarifErrorLogger)new SarifV2ErrorLogger(stream, toolName, compilerVersion, toolAssemblyVersion, Culture)) : ((SarifErrorLogger)new SarifV1ErrorLogger(stream, toolName, compilerVersion, toolAssemblyVersion, Culture)));
		}
		ReportDiagnostics(instance.ToReadOnlyAndFree(), consoleOutput, sarifErrorLogger, null);
		return sarifErrorLogger;
	}

	public virtual int Run(TextWriter consoleOutput, CancellationToken cancellationToken = default(CancellationToken))
	{
		CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
		SarifErrorLogger sarifErrorLogger = null;
		try
		{
			CultureInfo culture = Culture;
			if (culture != null)
			{
				CultureInfo.CurrentUICulture = culture;
			}
			if (Arguments.ErrorLogOptions?.Path != null)
			{
				sarifErrorLogger = GetErrorLogger(consoleOutput);
				if (sarifErrorLogger == null)
				{
					return 1;
				}
			}
			return RunCore(consoleOutput, sarifErrorLogger, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			int eRR_CompileCancelled = MessageProvider.ERR_CompileCancelled;
			if (eRR_CompileCancelled > 0)
			{
				DiagnosticInfo diagnosticInfo = new DiagnosticInfo(MessageProvider, eRR_CompileCancelled);
				ReportDiagnostics(new DiagnosticInfo[1] { diagnosticInfo }, consoleOutput, sarifErrorLogger, null);
			}
			return 1;
		}
		finally
		{
			CultureInfo.CurrentUICulture = currentUICulture;
			sarifErrorLogger?.Dispose();
		}
	}

	private protected (Compilation Compilation, GeneratorDriverTimingInfo DriverTimingInfo) RunGenerators(Compilation input, string generatedFilesBaseDirectory, ParseOptions parseOptions, ImmutableArray<ISourceGenerator> generators, AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider, ImmutableArray<AdditionalText> additionalTexts, DiagnosticBag generatorDiagnostics)
	{
		GeneratorDriver generatorDriver = null;
		string cacheKey = string.Empty;
		bool flag = !Arguments.ParseOptions.HasFeature("enable-generator-cache") || string.IsNullOrWhiteSpace(Arguments.OutputFileName);
		if (GeneratorDriverCache != null && !flag)
		{
			cacheKey = deriveCacheKey();
			generatorDriver = GeneratorDriverCache.TryGetDriver(cacheKey)?.WithUpdatedParseOptions(parseOptions).WithUpdatedAnalyzerConfigOptions(analyzerConfigOptionsProvider).ReplaceAdditionalTexts(additionalTexts);
		}
		if (generatorDriver == null)
		{
			generatorDriver = CreateGeneratorDriver(generatedFilesBaseDirectory, parseOptions, generators, analyzerConfigOptionsProvider, additionalTexts, Arguments.ChecksumAlgorithm);
		}
		generatorDriver = generatorDriver.RunGeneratorsAndUpdateCompilation(input, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);
		generatorDiagnostics.AddRange(diagnostics);
		if (!flag && generatorDriver.GetRunResult().GeneratedTrees.Any())
		{
			GeneratorDriverCache?.CacheGenerator(cacheKey, generatorDriver);
		}
		return (Compilation: outputCompilation, DriverTimingInfo: generatorDriver.GetTimingInfo());
		string deriveCacheKey()
		{
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			instance.Builder.Append(Arguments.GetOutputFilePath(Arguments.OutputFileName));
			foreach (ISourceGenerator item in generators)
			{
				Type generatorType = item.GetGeneratorType();
				instance.Builder.Append(generatorType.AssemblyQualifiedName);
				instance.Builder.Append(generatorType.Assembly.ManifestModule.ModuleVersionId.ToString());
			}
			return instance.ToStringAndFree();
		}
	}

	private protected abstract GeneratorDriver CreateGeneratorDriver(string baseDirectory, ParseOptions parseOptions, ImmutableArray<ISourceGenerator> generators, AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider, ImmutableArray<AdditionalText> additionalTexts, SourceHashAlgorithm checksumAlgorithm);

	private int RunCore(TextWriter consoleOutput, ErrorLogger? errorLogger, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (Arguments.DisplayVersion)
		{
			PrintVersion(consoleOutput);
			return 0;
		}
		if (Arguments.DisplayLangVersions)
		{
			PrintLangVersions(consoleOutput);
			return 0;
		}
		if (Arguments.DisplayLogo)
		{
			PrintLogo(consoleOutput);
		}
		if (Arguments.DisplayHelp)
		{
			PrintHelp(consoleOutput);
			return 0;
		}
		if (ReportDiagnostics(Arguments.Errors, consoleOutput, errorLogger, null))
		{
			return 1;
		}
		TouchedFileLogger touchedFilesLogger = ((Arguments.TouchedFilesPath != null) ? new TouchedFileLogger() : null);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		AnalyzerConfigSet analyzerConfigSet = null;
		ImmutableArray<AnalyzerConfigOptionsResult> immutableArray = default(ImmutableArray<AnalyzerConfigOptionsResult>);
		AnalyzerConfigOptionsResult globalConfigOptions = default(AnalyzerConfigOptionsResult);
		if (Arguments.AnalyzerConfigPaths.Length > 0)
		{
			if (!TryGetAnalyzerConfigSet(Arguments.AnalyzerConfigPaths, instance, out analyzerConfigSet))
			{
				ReportDiagnostics(instance, consoleOutput, errorLogger, null);
				return 1;
			}
			globalConfigOptions = analyzerConfigSet.GlobalConfigOptions;
			immutableArray = Arguments.SourceFiles.SelectAsArray((CommandLineSourceFile f) => analyzerConfigSet.GetOptionsForSourcePath(f.Path));
			foreach (AnalyzerConfigOptionsResult item in immutableArray)
			{
				instance.AddRange(item.Diagnostics);
			}
		}
		Compilation compilation = CreateCompilation(consoleOutput, touchedFilesLogger, errorLogger, immutableArray, globalConfigOptions);
		if (compilation == null)
		{
			return 1;
		}
		List<DiagnosticInfo> diagnostics = new List<DiagnosticInfo>();
		ResolveAnalyzersFromArguments(diagnostics, MessageProvider, compilation.Options, Arguments.SkipAnalyzers, out ImmutableArray<DiagnosticAnalyzer> analyzers, out ImmutableArray<ISourceGenerator> generators);
		ImmutableArray<AdditionalTextFile> items = ResolveAdditionalFilesFromArguments(diagnostics, MessageProvider, touchedFilesLogger);
		if (ReportDiagnostics(diagnostics, consoleOutput, errorLogger, compilation))
		{
			return 1;
		}
		ImmutableArray<EmbeddedText> embeddedTexts = AcquireEmbeddedTexts(compilation, instance);
		if (ReportDiagnostics(instance, consoleOutput, errorLogger, compilation))
		{
			return 1;
		}
		ImmutableArray<AdditionalText> additionalTextFiles = ImmutableArray<AdditionalText>.CastUp(items);
		CompileAndEmit(touchedFilesLogger, ref compilation, analyzers, generators, additionalTextFiles, analyzerConfigSet, immutableArray, embeddedTexts, instance, errorLogger, cancellationToken, out CancellationTokenSource analyzerCts, out AnalyzerDriver analyzerDriver, out GeneratorDriverTimingInfo? generatorTimingInfo);
		analyzerCts?.Cancel();
		int result = (ReportDiagnostics(instance, consoleOutput, errorLogger, compilation) ? 1 : 0);
		foreach (AdditionalTextFile item2 in items)
		{
			if (ReportDiagnostics(item2.Diagnostics, consoleOutput, errorLogger, compilation))
			{
				result = 1;
			}
		}
		if (Arguments.ReportAnalyzer)
		{
			ReportAnalyzerUtil.Report(consoleOutput, analyzerDriver, generatorTimingInfo, Culture, compilation.Options.ConcurrentBuild);
		}
		if (Arguments.ReportInternalsVisibleToAttributes)
		{
			ReportIVTInfos(consoleOutput, errorLogger, compilation, instance.ToReadOnly());
		}
		instance.Free();
		return result;
	}

	private static CompilerAnalyzerConfigOptionsProvider UpdateAnalyzerConfigOptionsProvider(CompilerAnalyzerConfigOptionsProvider existing, IEnumerable<SyntaxTree> syntaxTrees, ImmutableArray<AnalyzerConfigOptionsResult> sourceFileAnalyzerConfigOptions, ImmutableArray<AdditionalText> additionalFiles = default(ImmutableArray<AdditionalText>), ImmutableArray<AnalyzerConfigOptionsResult> additionalFileOptions = default(ImmutableArray<AnalyzerConfigOptionsResult>))
	{
		ImmutableDictionary<object, AnalyzerConfigOptions>.Builder builder = ImmutableDictionary.CreateBuilder<object, AnalyzerConfigOptions>();
		int num = 0;
		foreach (SyntaxTree syntaxTree in syntaxTrees)
		{
			ImmutableDictionary<string, string> analyzerOptions = sourceFileAnalyzerConfigOptions[num].AnalyzerOptions;
			if (analyzerOptions.Count > 0)
			{
				builder.Add(syntaxTree, new DictionaryAnalyzerConfigOptions(analyzerOptions));
			}
			num++;
		}
		if (!additionalFiles.IsDefault)
		{
			for (num = 0; num < additionalFiles.Length; num++)
			{
				ImmutableDictionary<string, string> analyzerOptions2 = additionalFileOptions[num].AnalyzerOptions;
				if (analyzerOptions2.Count > 0)
				{
					builder.Add(additionalFiles[num], new DictionaryAnalyzerConfigOptions(analyzerOptions2));
				}
			}
		}
		return existing.WithAdditionalTreeOptions(builder.ToImmutable());
	}

	private CompilerAnalyzerConfigOptionsProvider GetCompilerAnalyzerConfigOptionsProvider(AnalyzerConfigSet? analyzerConfigSet, ImmutableArray<AdditionalText> additionalTextFiles, DiagnosticBag diagnostics, Compilation compilation, ImmutableArray<AnalyzerConfigOptionsResult> sourceFileAnalyzerConfigOptions)
	{
		CompilerAnalyzerConfigOptionsProvider compilerAnalyzerConfigOptionsProvider = CompilerAnalyzerConfigOptionsProvider.Empty;
		if (Arguments.AnalyzerConfigPaths.Length > 0)
		{
			compilerAnalyzerConfigOptionsProvider = compilerAnalyzerConfigOptionsProvider.WithGlobalOptions(new DictionaryAnalyzerConfigOptions(analyzerConfigSet.GetOptionsForSourcePath(string.Empty).AnalyzerOptions));
			ImmutableArray<AnalyzerConfigOptionsResult> additionalFileOptions = additionalTextFiles.SelectAsArray((AdditionalText f) => analyzerConfigSet.GetOptionsForSourcePath(f.Path));
			foreach (AnalyzerConfigOptionsResult item in additionalFileOptions)
			{
				diagnostics.AddRange(item.Diagnostics);
			}
			compilerAnalyzerConfigOptionsProvider = UpdateAnalyzerConfigOptionsProvider(compilerAnalyzerConfigOptionsProvider, compilation.SyntaxTrees, sourceFileAnalyzerConfigOptions, additionalTextFiles, additionalFileOptions);
		}
		return compilerAnalyzerConfigOptionsProvider;
	}

	private void CompileAndEmit(TouchedFileLogger? touchedFilesLogger, ref Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableArray<ISourceGenerator> generators, ImmutableArray<AdditionalText> additionalTextFiles, AnalyzerConfigSet? analyzerConfigSet, ImmutableArray<AnalyzerConfigOptionsResult> sourceFileAnalyzerConfigOptions, ImmutableArray<EmbeddedText?> embeddedTexts, DiagnosticBag diagnostics, ErrorLogger? errorLogger, CancellationToken cancellationToken, out CancellationTokenSource? analyzerCts, out AnalyzerDriver? analyzerDriver, out GeneratorDriverTimingInfo? generatorTimingInfo)
	{
		analyzerCts = null;
		analyzerDriver = null;
		generatorTimingInfo = null;
		compilation.GetDiagnostics(CompilationStage.Parse, includeEarlierStages: false, diagnostics, cancellationToken);
		DiagnosticBag diagnosticBag = null;
		if (HasUnsuppressableErrors(diagnostics))
		{
			if (HasSuppressableWarningsOrErrors(diagnostics) && analyzers.Any((DiagnosticAnalyzer a) => a is DiagnosticSuppressor))
			{
				CompilerAnalyzerConfigOptionsProvider compilerAnalyzerConfigOptionsProvider = GetCompilerAnalyzerConfigOptionsProvider(analyzerConfigSet, additionalTextFiles, diagnostics, compilation, sourceFileAnalyzerConfigOptions);
				AnalyzerOptions analyzerOptions = CreateAnalyzerOptions(additionalTextFiles, compilerAnalyzerConfigOptionsProvider);
				(analyzerCts, diagnosticBag, analyzerDriver) = initializeAnalyzerDriver(analyzerOptions, ref compilation);
				analyzerDriver.ApplyProgrammaticSuppressions(diagnostics, compilation, analyzerCts.Token);
			}
			return;
		}
		if (!analyzers.IsEmpty || !generators.IsEmpty)
		{
			CompilerAnalyzerConfigOptionsProvider compilerAnalyzerConfigOptionsProvider2 = GetCompilerAnalyzerConfigOptionsProvider(analyzerConfigSet, additionalTextFiles, diagnostics, compilation, sourceFileAnalyzerConfigOptions);
			if (!generators.IsEmpty)
			{
				string generatedFilesOutputDirectory = Arguments.GeneratedFilesOutputDirectory;
				bool flag = !string.IsNullOrWhiteSpace(generatedFilesOutputDirectory);
				string generatedFilesBaseDirectory = (flag ? generatedFilesOutputDirectory : Arguments.OutputDirectory);
				(Compilation, GeneratorDriverTimingInfo) tuple2 = RunGenerators(compilation, generatedFilesBaseDirectory, Arguments.ParseOptions, generators, compilerAnalyzerConfigOptionsProvider2, additionalTextFiles, diagnostics);
				GeneratorDriverTimingInfo? generatorDriverTimingInfo = tuple2.Item2;
				compilation = tuple2.Item1;
				generatorTimingInfo = generatorDriverTimingInfo;
				bool num = !Arguments.AnalyzerConfigPaths.IsEmpty;
				List<SyntaxTree> list = compilation.SyntaxTrees.Skip(Arguments.SourceFiles.Length).ToList();
				ArrayBuilder<AnalyzerConfigOptionsResult> arrayBuilder = (num ? ArrayBuilder<AnalyzerConfigOptionsResult>.GetInstance(list.Count) : null);
				ArrayBuilder<EmbeddedText> instance = ArrayBuilder<EmbeddedText>.GetInstance(list.Count);
				try
				{
					foreach (SyntaxTree item2 in list)
					{
						cancellationToken.ThrowIfCancellationRequested();
						SourceText text = item2.GetText(cancellationToken);
						instance.Add(EmbeddedText.FromSource(item2.FilePath, text));
						arrayBuilder?.Add(analyzerConfigSet.GetOptionsForSourcePath(item2.FilePath));
						if (!flag)
						{
							continue;
						}
						string filePath = item2.FilePath;
						if (Directory.Exists(generatedFilesOutputDirectory))
						{
							Directory.CreateDirectory(Path.GetDirectoryName(filePath));
						}
						Stream stream = OpenFile(filePath, diagnostics, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
						if (stream == null)
						{
							continue;
						}
						using (new NoThrowStreamDisposer(stream, filePath, diagnostics, MessageProvider))
						{
							using StreamWriter textWriter = new StreamWriter(stream, item2.Encoding);
							text.Write(textWriter, cancellationToken);
							touchedFilesLogger?.AddWritten(filePath);
						}
					}
					embeddedTexts = embeddedTexts.AddRange(instance);
					if (arrayBuilder != null)
					{
						compilerAnalyzerConfigOptionsProvider2 = UpdateAnalyzerConfigOptionsProvider(compilerAnalyzerConfigOptionsProvider2, list, arrayBuilder.ToImmutable());
					}
				}
				finally
				{
					arrayBuilder?.Free();
					instance.Free();
				}
			}
			AnalyzerOptions analyzerOptions2 = CreateAnalyzerOptions(additionalTextFiles, compilerAnalyzerConfigOptionsProvider2);
			if (!analyzers.IsEmpty)
			{
				(analyzerCts, diagnosticBag, analyzerDriver) = initializeAnalyzerDriver(analyzerOptions2, ref compilation);
			}
		}
		compilation.GetDiagnostics(CompilationStage.Declare, includeEarlierStages: false, diagnostics, cancellationToken);
		if (HasUnsuppressableErrors(diagnostics))
		{
			if (analyzerDriver != null && analyzerDriver.HasDiagnosticSuppressors && HasSuppressableWarningsOrErrors(diagnostics))
			{
				analyzerDriver.ApplyProgrammaticSuppressions(diagnostics, compilation, cancellationToken);
			}
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
		string outputFileName = GetOutputFileName(compilation, cancellationToken);
		string outputFilePath = Arguments.GetOutputFilePath(outputFileName);
		string pdbFilePath = Arguments.GetPdbFilePath(outputFileName);
		string documentationPath = Arguments.DocumentationPath;
		NoThrowStreamDisposer noThrowStreamDisposer = null;
		try
		{
			EmitOptions emitOptions = Arguments.EmitOptions.WithOutputNameOverride(outputFileName).WithPdbFilePath(PathUtilities.NormalizePathPrefix(pdbFilePath, Arguments.PathMap));
			if (Arguments.ParseOptions.HasFeature("pdb-path-determinism") && !string.IsNullOrEmpty(emitOptions.PdbFilePath))
			{
				emitOptions = emitOptions.WithPdbFilePath(Path.GetFileName(emitOptions.PdbFilePath));
			}
			if (Arguments.ParseOptions.HasFeature("debug-determinism"))
			{
				EmitDeterminismKey(compilation, FileSystem, additionalTextFiles, analyzers, generators, Arguments.PathMap, emitOptions);
			}
			if (Arguments.SourceLink != null)
			{
				Stream stream2 = OpenFile(Arguments.SourceLink, diagnostics, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (stream2 != null)
				{
					noThrowStreamDisposer = new NoThrowStreamDisposer(stream2, Arguments.SourceLink, diagnostics, MessageProvider);
				}
			}
			if (!PathUtilities.IsValidFilePath(pdbFilePath))
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.FTL_InvalidInputFileName, Location.None, pdbFilePath));
			}
			CommonPEModuleBuilder commonPEModuleBuilder = compilation.CheckOptionsAndCreateModuleBuilder(diagnostics, Arguments.ManifestResources, emitOptions, null, noThrowStreamDisposer?.Stream, embeddedTexts, null, cancellationToken);
			if (commonPEModuleBuilder != null)
			{
				bool flag2;
				try
				{
					flag2 = compilation.CompileMethods(commonPEModuleBuilder, Arguments.EmitPdb, diagnostics, null, cancellationToken);
					if (analyzerDriver != null && !diagnostics.IsEmptyWithoutResolution)
					{
						analyzerDriver.ApplyProgrammaticSuppressions(diagnostics, compilation, cancellationToken);
					}
					if (HasUnsuppressedErrors(diagnostics))
					{
						flag2 = false;
					}
					if (flag2)
					{
						NoThrowStreamDisposer noThrowStreamDisposer2 = null;
						if (documentationPath != null)
						{
							Stream stream3 = OpenFile(documentationPath, diagnostics, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
							if (stream3 == null)
							{
								return;
							}
							try
							{
								stream3.SetLength(0L);
							}
							catch (Exception e)
							{
								MessageProvider.ReportStreamWriteException(e, documentationPath, diagnostics);
								return;
							}
							noThrowStreamDisposer2 = new NoThrowStreamDisposer(stream3, documentationPath, diagnostics, MessageProvider);
						}
						using (noThrowStreamDisposer2)
						{
							using Stream win32Resources = GetWin32Resources(FileSystem, MessageProvider, Arguments, compilation, diagnostics);
							if (HasUnsuppressableErrors(diagnostics))
							{
								return;
							}
							flag2 = compilation.GenerateResources(commonPEModuleBuilder, win32Resources, useRawWin32Resources: false, diagnostics, cancellationToken) && compilation.GenerateDocumentationComments(noThrowStreamDisposer2?.Stream, emitOptions.OutputNameOverride, diagnostics, cancellationToken);
						}
						if (noThrowStreamDisposer2 != null && noThrowStreamDisposer2.HasFailedToDispose)
						{
							return;
						}
						if (flag2)
						{
							compilation.ReportUnusedImports(diagnostics, cancellationToken);
						}
					}
					compilation.CompleteTrees(null);
					if (analyzerDriver != null)
					{
						ImmutableArray<Diagnostic> result = analyzerDriver.GetDiagnosticsAsync(compilation, cancellationToken).Result;
						diagnostics.AddRange(result);
						if (!diagnostics.IsEmptyWithoutResolution)
						{
							analyzerDriver.ApplyProgrammaticSuppressions(diagnostics, compilation, cancellationToken);
						}
						if (errorLogger != null)
						{
							ImmutableArray<(DiagnosticDescriptor, DiagnosticDescriptorErrorLoggerInfo)> allDiagnosticDescriptorsWithInfo = analyzerDriver.GetAllDiagnosticDescriptorsWithInfo(cancellationToken, out var totalAnalyzerExecutionTime);
							AddAnalyzerDescriptorsAndExecutionTime(errorLogger, allDiagnosticDescriptorsWithInfo, totalAnalyzerExecutionTime);
						}
					}
				}
				finally
				{
					commonPEModuleBuilder.CompilationFinished();
				}
				if (HasUnsuppressedErrors(diagnostics))
				{
					flag2 = false;
				}
				if (flag2)
				{
					CompilerEmitStreamProvider compilerEmitStreamProvider = new CompilerEmitStreamProvider(this, outputFilePath);
					CompilerEmitStreamProvider compilerEmitStreamProvider2 = (Arguments.EmitPdbFile ? new CompilerEmitStreamProvider(this, pdbFilePath) : null);
					string outputRefFilePath = Arguments.OutputRefFilePath;
					CompilerEmitStreamProvider compilerEmitStreamProvider3 = ((outputRefFilePath != null) ? new CompilerEmitStreamProvider(this, outputRefFilePath) : null);
					RSAParameters? privateKeyOpt = null;
					if (compilation.Options.StrongNameProvider != null && compilation.SignUsingBuilder && !compilation.Options.PublicSign)
					{
						privateKeyOpt = compilation.StrongNameKeys.PrivateKey;
					}
					emitOptions = emitOptions.WithFallbackSourceFileEncoding(GetFallbackEncoding());
					flag2 = compilation.SerializeToPeStream(commonPEModuleBuilder, compilerEmitStreamProvider, compilerEmitStreamProvider3, compilerEmitStreamProvider2, null, null, diagnostics, emitOptions, privateKeyOpt, cancellationToken);
					compilerEmitStreamProvider.Close(diagnostics);
					compilerEmitStreamProvider3?.Close(diagnostics);
					compilerEmitStreamProvider2?.Close(diagnostics);
					if (flag2 && touchedFilesLogger != null)
					{
						if (compilerEmitStreamProvider2 != null)
						{
							touchedFilesLogger.AddWritten(pdbFilePath);
						}
						if (compilerEmitStreamProvider3 != null)
						{
							touchedFilesLogger.AddWritten(outputRefFilePath);
						}
						touchedFilesLogger.AddWritten(outputFilePath);
					}
				}
			}
			if (HasUnsuppressableErrors(diagnostics))
			{
				return;
			}
		}
		finally
		{
			noThrowStreamDisposer?.Dispose();
		}
		if (noThrowStreamDisposer != null && noThrowStreamDisposer.HasFailedToDispose)
		{
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (diagnosticBag != null)
		{
			diagnostics.AddRange(diagnosticBag);
			if (HasUnsuppressableErrors(diagnosticBag))
			{
				return;
			}
		}
		cancellationToken.ThrowIfCancellationRequested();
		WriteTouchedFiles(diagnostics, touchedFilesLogger, documentationPath);
		(CancellationTokenSource, DiagnosticBag, AnalyzerDriver) initializeAnalyzerDriver(AnalyzerOptions options, ref Compilation reference)
		{
			CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { cancellationToken });
			DiagnosticBag diagnosticBag2 = new DiagnosticBag();
			SeverityFilter severityFilter = SeverityFilter.Hidden;
			if (Arguments.ErrorLogPath == null)
			{
				severityFilter |= SeverityFilter.Info;
			}
			AnalyzerDriver item = AnalyzerDriver.CreateAndAttachToCompilation(reference, analyzers, options, new AnalyzerManager(analyzers), diagnosticBag2.Add, Arguments.ReportAnalyzer || errorLogger != null, severityFilter, errorLogger != null, out reference, cancellationTokenSource.Token);
			return (cancellationTokenSource, diagnosticBag2, item);
		}
	}

	protected virtual AnalyzerOptions CreateAnalyzerOptions(ImmutableArray<AdditionalText> additionalTextFiles, AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider)
	{
		return new AnalyzerOptions(additionalTextFiles, analyzerConfigOptionsProvider);
	}

	protected virtual void AddAnalyzerDescriptorsAndExecutionTime(ErrorLogger errorLogger, ImmutableArray<(DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info)> descriptorsWithInfo, double totalAnalyzerExecutionTime)
	{
		errorLogger.AddAnalyzerDescriptorsAndExecutionTime(descriptorsWithInfo, totalAnalyzerExecutionTime);
	}

	private bool WriteTouchedFiles(DiagnosticBag diagnostics, TouchedFileLogger? touchedFilesLogger, string? finalXmlFilePath)
	{
		if (Arguments.TouchedFilesPath != null)
		{
			if (finalXmlFilePath != null)
			{
				touchedFilesLogger.AddWritten(finalXmlFilePath);
			}
			string text = Arguments.TouchedFilesPath + ".read";
			string text2 = Arguments.TouchedFilesPath + ".write";
			Stream stream = OpenFile(text, diagnostics, FileMode.OpenOrCreate);
			Stream stream2 = OpenFile(text2, diagnostics, FileMode.OpenOrCreate);
			if (stream == null || stream2 == null)
			{
				return false;
			}
			string filePath = null;
			try
			{
				filePath = text;
				using (StreamWriter s = new StreamWriter(stream))
				{
					touchedFilesLogger.WriteReadPaths(s);
				}
				filePath = text2;
				using StreamWriter s2 = new StreamWriter(stream2);
				touchedFilesLogger.WriteWrittenPaths(s2);
			}
			catch (Exception e)
			{
				MessageProvider.ReportStreamWriteException(e, filePath, diagnostics);
				return false;
			}
		}
		return true;
	}

	protected virtual ImmutableArray<AdditionalTextFile> ResolveAdditionalFilesFromArguments(List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider, TouchedFileLogger? touchedFilesLogger)
	{
		ArrayBuilder<AdditionalTextFile> instance = ArrayBuilder<AdditionalTextFile>.GetInstance();
		HashSet<string> hashSet = new HashSet<string>(PathUtilities.Comparer);
		foreach (CommandLineSourceFile additionalFile in Arguments.AdditionalFiles)
		{
			if (hashSet.Add(PathUtilities.ExpandAbsolutePathWithRelativeParts(additionalFile.Path)))
			{
				instance.Add(new AdditionalTextFile(additionalFile, this));
			}
		}
		return instance.ToImmutableAndFree();
	}

	protected abstract string GetOutputFileName(Compilation compilation, CancellationToken cancellationToken);

	private Stream? OpenFile(string filePath, DiagnosticBag diagnostics, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
	{
		try
		{
			return FileSystem.OpenFile(filePath, mode, access, share);
		}
		catch (Exception e)
		{
			MessageProvider.ReportStreamWriteException(e, filePath, diagnostics);
			return null;
		}
	}

	internal static Stream? GetWin32ResourcesInternal(ICommonCompilerFileSystem fileSystem, CommonMessageProvider messageProvider, CommandLineArguments arguments, Compilation compilation, out IEnumerable<DiagnosticInfo> errors)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		Stream? win32Resources = GetWin32Resources(fileSystem, messageProvider, arguments, compilation, instance);
		errors = instance.ToReadOnlyAndFree().SelectAsArray((Diagnostic diag) => new DiagnosticInfo(messageProvider, diag.IsWarningAsError, diag.Code, (object[])diag.Arguments));
		return win32Resources;
	}

	private static Stream? GetWin32Resources(ICommonCompilerFileSystem fileSystem, CommonMessageProvider messageProvider, CommandLineArguments arguments, Compilation compilation, DiagnosticBag diagnostics)
	{
		if (arguments.Win32ResourceFile != null)
		{
			return OpenStream(fileSystem, messageProvider, arguments.Win32ResourceFile, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Resource, diagnostics);
		}
		using (Stream manifestContents = OpenManifestStream(fileSystem, messageProvider, compilation.Options.OutputKind, arguments, diagnostics))
		{
			using Stream iconInIcoFormat = OpenStream(fileSystem, messageProvider, arguments.Win32Icon, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Icon, diagnostics);
			try
			{
				return compilation.CreateDefaultWin32Resources(versionResource: true, arguments.NoWin32Manifest, manifestContents, iconInIcoFormat);
			}
			catch (Exception ex)
			{
				diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_ErrorBuildingWin32Resource, Location.None, ex.Message));
			}
		}
		return null;
	}

	private static Stream? OpenManifestStream(ICommonCompilerFileSystem fileSystem, CommonMessageProvider messageProvider, OutputKind outputKind, CommandLineArguments arguments, DiagnosticBag diagnostics)
	{
		if (!outputKind.IsNetModule())
		{
			return OpenStream(fileSystem, messageProvider, arguments.Win32Manifest, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Manifest, diagnostics);
		}
		return null;
	}

	private static Stream? OpenStream(ICommonCompilerFileSystem fileSystem, CommonMessageProvider messageProvider, string? path, string? baseDirectory, int errorCode, DiagnosticBag diagnostics)
	{
		if (path == null)
		{
			return null;
		}
		string text = ResolveRelativePath(messageProvider, path, baseDirectory, diagnostics);
		if (text == null)
		{
			return null;
		}
		try
		{
			return fileSystem.OpenFile(text, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		catch (Exception ex)
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(errorCode, Location.None, text, ex.Message));
		}
		return null;
	}

	private static string? ResolveRelativePath(CommonMessageProvider messageProvider, string path, string? baseDirectory, DiagnosticBag diagnostics)
	{
		string text = FileUtilities.ResolveRelativePath(path, baseDirectory);
		if (text == null)
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.FTL_InvalidInputFileName, Location.None, path ?? ""));
		}
		return text;
	}

	internal static bool TryGetCompilerDiagnosticCode(string diagnosticId, string expectedPrefix, out uint code)
	{
		code = 0u;
		if (diagnosticId.StartsWith(expectedPrefix, StringComparison.Ordinal))
		{
			return uint.TryParse(diagnosticId.Substring(expectedPrefix.Length), out code);
		}
		return false;
	}

	private void EmitDeterminismKey(Compilation compilation, ICommonCompilerFileSystem fileSystem, ImmutableArray<AdditionalText> additionalTexts, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableArray<ISourceGenerator> generators, ImmutableArray<KeyValuePair<string, string>> pathMap, EmitOptions? emitOptions)
	{
		string deterministicKey = compilation.GetDeterministicKey(additionalTexts, analyzers, generators, pathMap, emitOptions);
		string filePath = Path.Combine(Arguments.OutputDirectory, Arguments.OutputFileName + ".key");
		using Stream stream = fileSystem.OpenFile(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		byte[] bytes = Encoding.UTF8.GetBytes(deterministicKey);
		stream.Write(bytes, 0, bytes.Length);
	}
}
