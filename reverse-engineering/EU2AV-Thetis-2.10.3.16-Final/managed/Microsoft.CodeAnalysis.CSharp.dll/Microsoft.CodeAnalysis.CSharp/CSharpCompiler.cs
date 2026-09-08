using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class CSharpCompiler : CommonCompiler
{
	internal const string ResponseFileName = "csc.rsp";

	private readonly CommandLineDiagnosticFormatter _diagnosticFormatter;

	private readonly string? _tempDirectory;

	public override DiagnosticFormatter DiagnosticFormatter => _diagnosticFormatter;

	protected internal new CSharpCommandLineArguments Arguments => (CSharpCommandLineArguments)base.Arguments;

	internal override Type Type => typeof(CSharpCompiler);

	protected CSharpCompiler(CSharpCommandLineParser parser, string? responseFile, string[] args, BuildPaths buildPaths, string? additionalReferenceDirectories, IAnalyzerAssemblyLoader assemblyLoader, GeneratorDriverCache? driverCache = null, ICommonCompilerFileSystem? fileSystem = null)
		: base(parser, responseFile, args, buildPaths, additionalReferenceDirectories, assemblyLoader, driverCache, fileSystem)
	{
		_diagnosticFormatter = new CommandLineDiagnosticFormatter(buildPaths.WorkingDirectory, Arguments.PrintFullPaths, Arguments.ShouldIncludeErrorEndLocation);
		_tempDirectory = buildPaths.TempDirectory;
	}

	public override Compilation? CreateCompilation(TextWriter consoleOutput, TouchedFileLogger? touchedFilesLogger, ErrorLogger? errorLogger, ImmutableArray<AnalyzerConfigOptionsResult> analyzerConfigOptions, AnalyzerConfigOptionsResult globalConfigOptions)
	{
		CSharpParseOptions parseOptions = Arguments.ParseOptions;
		CSharpParseOptions scriptParseOptions = parseOptions.WithKind(SourceCodeKind.Script);
		bool hadErrors = false;
		ImmutableArray<CommandLineSourceFile> sourceFiles = Arguments.SourceFiles;
		SyntaxTree?[] trees = new SyntaxTree[sourceFiles.Length];
		string?[] normalizedFilePaths = new string[sourceFiles.Length];
		DiagnosticBag diagnosticBag = DiagnosticBag.GetInstance();
		if (Arguments.CompilationOptions.ConcurrentBuild)
		{
			RoslynParallel.For(0, sourceFiles.Length, UICultureUtilities.WithCurrentUICulture(delegate(int i)
			{
				trees[i] = ParseFile(parseOptions, scriptParseOptions, ref hadErrors, sourceFiles[i], diagnosticBag, out normalizedFilePaths[i]);
			}), CancellationToken.None);
		}
		else
		{
			for (int num = 0; num < sourceFiles.Length; num++)
			{
				trees[num] = ParseFile(parseOptions, scriptParseOptions, ref hadErrors, sourceFiles[num], diagnosticBag, out normalizedFilePaths[num]);
			}
		}
		if (ReportDiagnostics(diagnosticBag.ToReadOnlyAndFree(), consoleOutput, errorLogger, null))
		{
			return null;
		}
		List<DiagnosticInfo> list = new List<DiagnosticInfo>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		for (int num2 = 0; num2 < sourceFiles.Length; num2++)
		{
			string text = normalizedFilePaths[num2];
			if (!hashSet.Add(text))
			{
				list.Add(new DiagnosticInfo(base.MessageProvider, 2002, Arguments.PrintFullPaths ? text : _diagnosticFormatter.RelativizeNormalizedPath(text)));
				trees[num2] = null;
			}
		}
		if (Arguments.TouchedFilesPath != null)
		{
			foreach (string item in hashSet)
			{
				touchedFilesLogger.AddRead(item);
			}
		}
		DesktopAssemblyIdentityComparer comparer = DesktopAssemblyIdentityComparer.Default;
		string appConfigPath = Arguments.AppConfigPath;
		if (appConfigPath != null)
		{
			try
			{
				using (FileStream input = new FileStream(appConfigPath, FileMode.Open, FileAccess.Read))
				{
					comparer = DesktopAssemblyIdentityComparer.LoadFromXml(input);
				}
				touchedFilesLogger?.AddRead(appConfigPath);
			}
			catch (Exception ex)
			{
				list.Add(new DiagnosticInfo(base.MessageProvider, 7093, appConfigPath, ex.Message));
			}
		}
		LoggingXmlFileResolver resolver = new LoggingXmlFileResolver(Arguments.BaseDirectory, touchedFilesLogger);
		LoggingSourceFileResolver resolver2 = new LoggingSourceFileResolver(ImmutableArray<string>.Empty, Arguments.BaseDirectory, Arguments.PathMap, touchedFilesLogger);
		List<MetadataReference> references = ResolveMetadataReferences(list, touchedFilesLogger, out MetadataReferenceResolver referenceDirectiveResolver);
		if (ReportDiagnostics(list, consoleOutput, errorLogger, null))
		{
			return null;
		}
		LoggingStrongNameFileSystem fileSystem = new LoggingStrongNameFileSystem(touchedFilesLogger, _tempDirectory);
		CompilerSyntaxTreeOptionsProvider provider = new CompilerSyntaxTreeOptionsProvider(trees, analyzerConfigOptions, globalConfigOptions);
		return CSharpCompilation.Create(Arguments.CompilationName, trees.WhereNotNull(), references, Arguments.CompilationOptions.WithMetadataReferenceResolver(referenceDirectiveResolver).WithAssemblyIdentityComparer(comparer).WithXmlReferenceResolver(resolver)
			.WithStrongNameProvider(Arguments.GetStrongNameProvider(fileSystem))
			.WithSourceReferenceResolver(resolver2)
			.WithSyntaxTreeOptionsProvider(provider));
	}

	private SyntaxTree? ParseFile(CSharpParseOptions parseOptions, CSharpParseOptions scriptParseOptions, ref bool addedDiagnostics, CommandLineSourceFile file, DiagnosticBag diagnostics, out string? normalizedFilePath)
	{
		List<DiagnosticInfo> list = new List<DiagnosticInfo>();
		SourceText sourceText = TryReadFileContent(file, list, out normalizedFilePath);
		if (sourceText == null)
		{
			foreach (DiagnosticInfo item in list)
			{
				diagnostics.Add(base.MessageProvider.CreateDiagnostic(item));
			}
			list.Clear();
			addedDiagnostics = true;
			return null;
		}
		return ParseFile(parseOptions, scriptParseOptions, sourceText, file);
	}

	private static SyntaxTree ParseFile(CSharpParseOptions parseOptions, CSharpParseOptions scriptParseOptions, SourceText content, CommandLineSourceFile file)
	{
		SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(content, file.IsScript ? scriptParseOptions : parseOptions, file.Path);
		syntaxTree.GetMappedLineSpanAndVisibility(default(TextSpan), out var _);
		return syntaxTree;
	}

	protected override string GetOutputFileName(Compilation compilation, CancellationToken cancellationToken)
	{
		if (Arguments.OutputFileName != null)
		{
			return Arguments.OutputFileName;
		}
		CSharpCompilation cSharpCompilation = (CSharpCompilation)compilation;
		Symbol symbol = cSharpCompilation.ScriptClass;
		if ((object)symbol == null)
		{
			MethodSymbol entryPoint = cSharpCompilation.GetEntryPoint(cancellationToken);
			if ((object)entryPoint == null)
			{
				return "error";
			}
			symbol = entryPoint.PartialImplementationPart ?? entryPoint;
		}
		return Path.ChangeExtension(PathUtilities.GetFileName(symbol.GetFirstLocation().SourceTree.FilePath), ".exe");
	}

	internal override bool SuppressDefaultResponseFile(IEnumerable<string> args)
	{
		return args.Any((string arg) => IReadOnlyListExtensions.Contains(new string[2] { "/noconfig", "-noconfig" }, arg.ToLowerInvariant()));
	}

	public override void PrintLogo(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LogoLine1, Culture), GetToolName(), GetCompilerVersion());
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LogoLine2, Culture));
		consoleOutput.WriteLine();
	}

	public override void PrintLangVersions(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LangVersions, Culture));
		LanguageVersion languageVersion = LanguageVersion.Default.MapSpecifiedToEffectiveVersion();
		LanguageVersion languageVersion2 = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion();
		LanguageVersion[] array = (LanguageVersion[])Enum.GetValues(typeof(LanguageVersion));
		foreach (LanguageVersion languageVersion3 in array)
		{
			if (languageVersion3 == languageVersion)
			{
				consoleOutput.WriteLine(languageVersion3.ToDisplayString() + " (default)");
			}
			else if (languageVersion3 == languageVersion2)
			{
				consoleOutput.WriteLine(languageVersion3.ToDisplayString() + " (latest)");
			}
			else
			{
				consoleOutput.WriteLine(languageVersion3.ToDisplayString());
			}
		}
		consoleOutput.WriteLine();
	}

	internal override string GetToolName()
	{
		return ErrorFacts.GetMessage(MessageID.IDS_ToolName, Culture);
	}

	public override void PrintHelp(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_CSCHelp, Culture));
	}

	protected override bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code)
	{
		return CommonCompiler.TryGetCompilerDiagnosticCode(diagnosticId, "CS", out code);
	}

	protected override void ResolveAnalyzersFromArguments(List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider, CompilationOptions compilationOptions, bool skipAnalyzers, out ImmutableArray<DiagnosticAnalyzer> analyzers, out ImmutableArray<ISourceGenerator> generators)
	{
		Arguments.ResolveAnalyzersFromArguments("C#", diagnostics, messageProvider, base.AssemblyLoader, compilationOptions, skipAnalyzers, out analyzers, out generators);
	}

	protected override void ResolveEmbeddedFilesFromExternalSourceDirectives(SyntaxTree tree, SourceReferenceResolver resolver, OrderedSet<string> embeddedFiles, DiagnosticBag diagnostics)
	{
		foreach (LineDirectiveTriviaSyntax directive in tree.GetRoot().GetDirectives((DirectiveTriviaSyntax d) => d.IsActive && !d.HasErrors && d.Kind() == SyntaxKind.LineDirectiveTrivia))
		{
			string text = (string)directive.File.Value;
			if (text != null)
			{
				string text2 = resolver.ResolveReference(text, tree.FilePath);
				if (text2 == null)
				{
					diagnostics.Add(base.MessageProvider.CreateDiagnostic(1504, directive.File.GetLocation(), text, CSharpResources.CouldNotFindFile));
				}
				else
				{
					embeddedFiles.Add(text2);
				}
			}
		}
	}

	private protected override GeneratorDriver CreateGeneratorDriver(string baseDirectory, ParseOptions parseOptions, ImmutableArray<ISourceGenerator> generators, AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider, ImmutableArray<AdditionalText> additionalTexts, SourceHashAlgorithm checksumAlgorithm)
	{
		return CSharpGeneratorDriver.Create(generators, additionalTexts, (CSharpParseOptions)parseOptions, analyzerConfigOptionsProvider, new GeneratorDriverOptions(IncrementalGeneratorOutputKind.Host, trackIncrementalGeneratorSteps: false, baseDirectory)
		{
			ChecksumAlgorithm = checksumAlgorithm
		});
	}

	private protected override void DiagnoseBadAccesses(TextWriter consoleOutput, ErrorLogger? errorLogger, Compilation compilation, ImmutableArray<Diagnostic> diagnostics)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		foreach (Diagnostic item in diagnostics)
		{
			Symbol symbol2;
			if (item != null)
			{
				int code = item.Code;
				if (code <= 271)
				{
					if (code != 122)
					{
						if (code == 271)
						{
							IReadOnlyList<object> arguments = item.Arguments;
							if (arguments != null && arguments.Count == 1 && arguments[0] is Symbol symbol)
							{
								symbol2 = symbol;
								goto IL_0139;
							}
						}
					}
					else
					{
						IReadOnlyList<object> arguments = item.Arguments;
						if (arguments != null && arguments.Count == 1 && arguments[0] is Symbol symbol3)
						{
							symbol2 = symbol3;
							goto IL_0139;
						}
					}
				}
				else if (code != 272)
				{
					if (code == 9044)
					{
						IReadOnlyList<object> arguments = item.Arguments;
						if (arguments != null && arguments.Count == 3 && arguments[1] is Symbol symbol4)
						{
							symbol2 = symbol4;
							goto IL_0139;
						}
					}
				}
				else
				{
					IReadOnlyList<object> arguments = item.Arguments;
					if (arguments != null && arguments.Count == 1 && arguments[0] is Symbol symbol5)
					{
						symbol2 = symbol5;
						goto IL_0139;
					}
				}
			}
			symbol2 = null;
			goto IL_0139;
			IL_0139:
			Symbol symbol6 = symbol2;
			if ((object)symbol6 != null && compilation.Assembly != symbol6.ContainingAssembly)
			{
				instance.Add(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_SymbolDefinedInAssembly, symbol6, symbol6.ContainingAssembly), item.Location));
			}
		}
		ReportDiagnostics(instance.ToReadOnlyAndFree(), consoleOutput, errorLogger, compilation);
	}
}
