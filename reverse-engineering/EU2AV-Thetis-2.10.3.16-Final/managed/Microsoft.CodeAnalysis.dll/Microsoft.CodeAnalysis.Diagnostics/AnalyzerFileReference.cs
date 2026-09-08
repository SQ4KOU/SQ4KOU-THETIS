using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class AnalyzerFileReference : AnalyzerReference, IEquatable<AnalyzerReference>
{
	private delegate ImmutableArray<string> AttributeLanguagesFunc(PEModule module, CustomAttributeHandle attribute);

	private sealed class Extensions<TExtension> where TExtension : class
	{
		private class ExtTypeComparer : IEqualityComparer<TExtension>
		{
			public static readonly ExtTypeComparer Instance = new ExtTypeComparer();

			public bool Equals(TExtension? x, TExtension? y)
			{
				return object.Equals(x?.GetType(), y?.GetType());
			}

			public int GetHashCode(TExtension obj)
			{
				return obj.GetType().GetHashCode();
			}
		}

		private readonly AnalyzerFileReference _reference;

		private readonly Type _attributeType;

		private readonly AttributeLanguagesFunc _languagesFunc;

		private readonly bool _allowNetFramework;

		private readonly Func<object?, TExtension?>? _coerceFunction;

		private ImmutableArray<TExtension> _lazyAllExtensions;

		private ImmutableDictionary<string, ImmutableArray<TExtension>> _lazyExtensionsPerLanguage;

		private ImmutableSortedDictionary<string, ImmutableHashSet<string>>? _lazyExtensionTypeNameMap;

		internal Extensions(AnalyzerFileReference reference, Type attributeType, AttributeLanguagesFunc languagesFunc, bool allowNetFramework, Func<object?, TExtension?>? coerceFunction = null)
		{
			_reference = reference;
			_attributeType = attributeType;
			_languagesFunc = languagesFunc;
			_allowNetFramework = allowNetFramework;
			_coerceFunction = coerceFunction;
			_lazyAllExtensions = default(ImmutableArray<TExtension>);
			_lazyExtensionsPerLanguage = ImmutableDictionary<string, ImmutableArray<TExtension>>.Empty;
		}

		internal ImmutableArray<TExtension> GetExtensionsForAllLanguages(bool includeDuplicates)
		{
			if (_lazyAllExtensions.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyAllExtensions, CreateExtensionsForAllLanguages(this, includeDuplicates));
			}
			return _lazyAllExtensions;
		}

		private static ImmutableArray<TExtension> CreateExtensionsForAllLanguages(Extensions<TExtension> extensions, bool includeDuplicates)
		{
			ImmutableSortedDictionary<string, ImmutableArray<TExtension>>.Builder builder = ImmutableSortedDictionary.CreateBuilder<string, ImmutableArray<TExtension>>(StringComparer.OrdinalIgnoreCase);
			extensions.AddExtensions(builder);
			ImmutableArray<TExtension>.Builder builder2 = ImmutableArray.CreateBuilder<TExtension>();
			foreach (ImmutableArray<TExtension> value in builder.Values)
			{
				foreach (TExtension item in value)
				{
					builder2.Add(item);
				}
			}
			if (includeDuplicates)
			{
				return builder2.ToImmutable();
			}
			return builder2.Distinct(ExtTypeComparer.Instance).ToImmutableArray();
		}

		internal ImmutableArray<TExtension> GetExtensions(string language)
		{
			if (string.IsNullOrEmpty(language))
			{
				throw new ArgumentException("language");
			}
			return ImmutableInterlocked.GetOrAdd(ref _lazyExtensionsPerLanguage, language, CreateLanguageSpecificExtensions, this);
		}

		private static ImmutableArray<TExtension> CreateLanguageSpecificExtensions(string language, Extensions<TExtension> extensions)
		{
			ImmutableArray<TExtension>.Builder builder = ImmutableArray.CreateBuilder<TExtension>();
			extensions.AddExtensions(builder, language);
			return builder.ToImmutable();
		}

		internal ImmutableSortedDictionary<string, ImmutableHashSet<string>> GetExtensionTypeNameMap()
		{
			if (_lazyExtensionTypeNameMap == null)
			{
				ImmutableSortedDictionary<string, ImmutableHashSet<string>> analyzerTypeNameMap = GetAnalyzerTypeNameMap(_reference.FullPath, _attributeType, _languagesFunc);
				Interlocked.CompareExchange(ref _lazyExtensionTypeNameMap, analyzerTypeNameMap, null);
			}
			return _lazyExtensionTypeNameMap;
		}

		internal void AddExtensions(ImmutableSortedDictionary<string, ImmutableArray<TExtension>>.Builder builder)
		{
			ImmutableSortedDictionary<string, ImmutableHashSet<string>> extensionTypeNameMap;
			Assembly assembly;
			try
			{
				extensionTypeNameMap = GetExtensionTypeNameMap();
				if (extensionTypeNameMap.Count == 0)
				{
					return;
				}
				assembly = _reference.GetAssembly();
				if (CheckAssemblyReferencesNewerCompiler(assembly))
				{
					return;
				}
			}
			catch (Exception e)
			{
				_reference.AnalyzerLoadFailed?.Invoke(_reference, CreateAnalyzerFailedArgs(e));
				return;
			}
			int count = builder.Count;
			bool reportedError = false;
			foreach (var (text2, _) in extensionTypeNameMap)
			{
				if (text2 != null)
				{
					ImmutableArray<TExtension> languageSpecificAnalyzers = GetLanguageSpecificAnalyzers(assembly, extensionTypeNameMap, text2, ref reportedError);
					builder.Add(text2, languageSpecificAnalyzers);
				}
			}
			if (builder.Count == count && !reportedError)
			{
				_reference.AnalyzerLoadFailed?.Invoke(_reference, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers, CodeAnalysisResources.NoAnalyzersFound));
			}
		}

		internal void AddExtensions(ImmutableArray<TExtension>.Builder builder, string language, Func<TExtension, bool>? shouldInclude = null)
		{
			ImmutableSortedDictionary<string, ImmutableHashSet<string>> extensionTypeNameMap;
			Assembly assembly;
			try
			{
				extensionTypeNameMap = GetExtensionTypeNameMap();
				if (!extensionTypeNameMap.ContainsKey(language))
				{
					return;
				}
				assembly = _reference.GetAssembly();
				if (assembly == null || CheckAssemblyReferencesNewerCompiler(assembly))
				{
					return;
				}
			}
			catch (Exception e)
			{
				_reference.AnalyzerLoadFailed?.Invoke(_reference, CreateAnalyzerFailedArgs(e));
				return;
			}
			bool reportedError = false;
			ImmutableArray<TExtension> immutableArray = GetLanguageSpecificAnalyzers(assembly, extensionTypeNameMap, language, ref reportedError);
			bool num = !immutableArray.IsEmpty;
			if (shouldInclude != null)
			{
				immutableArray = immutableArray.WhereAsArray(shouldInclude);
			}
			builder.AddRange(immutableArray);
			if (!num && !reportedError)
			{
				_reference.AnalyzerLoadFailed?.Invoke(_reference, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers, CodeAnalysisResources.NoAnalyzersFound));
			}
		}

		private bool CheckAssemblyReferencesNewerCompiler(Assembly analyzerAssembly)
		{
			AssemblyName name = typeof(AnalyzerFileReference).Assembly.GetName();
			AssemblyName[] referencedAssemblies = analyzerAssembly.GetReferencedAssemblies();
			foreach (AssemblyName assemblyName in referencedAssemblies)
			{
				if (string.Equals(assemblyName.Name, name.Name, StringComparison.OrdinalIgnoreCase) && assemblyName.Version > name.Version)
				{
					_reference.AnalyzerLoadFailed?.Invoke(_reference, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.ReferencesNewerCompiler, "")
					{
						ReferencedCompilerVersion = assemblyName.Version
					});
					return true;
				}
			}
			return false;
		}

		private ImmutableArray<TExtension> GetLanguageSpecificAnalyzers(Assembly analyzerAssembly, ImmutableSortedDictionary<string, ImmutableHashSet<string>> analyzerTypeNameMap, string language, ref bool reportedError)
		{
			if (!analyzerTypeNameMap.TryGetValue(language, out ImmutableHashSet<string> value))
			{
				return ImmutableArray<TExtension>.Empty;
			}
			return GetAnalyzersForTypeNames(analyzerAssembly, value, ref reportedError);
		}

		private ImmutableArray<TExtension> GetAnalyzersForTypeNames(Assembly analyzerAssembly, ImmutableHashSet<string> analyzerTypeNames, ref bool reportedError)
		{
			ArrayBuilder<(string, TExtension)> instance = ArrayBuilder<(string, TExtension)>.GetInstance();
			foreach (string item in shuffle(analyzerTypeNames))
			{
				Type type;
				try
				{
					type = analyzerAssembly.GetType(item, throwOnError: true, ignoreCase: false);
				}
				catch (Exception e)
				{
					_reference.AnalyzerLoadFailed?.Invoke(_reference, CreateAnalyzerFailedArgs(e, item));
					reportedError = true;
					continue;
				}
				if (!_allowNetFramework)
				{
					TargetFrameworkAttribute customAttribute = analyzerAssembly.GetCustomAttribute<TargetFrameworkAttribute>();
					if (customAttribute != null && customAttribute.FrameworkName.StartsWith(".NETFramework", StringComparison.OrdinalIgnoreCase))
					{
						_reference.AnalyzerLoadFailed?.Invoke(_reference, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.ReferencesFramework, string.Format(CodeAnalysisResources.AssemblyReferencesNetFramework, item), null, item));
						continue;
					}
				}
				object obj;
				try
				{
					obj = Activator.CreateInstance(type);
				}
				catch (Exception e2)
				{
					_reference.AnalyzerLoadFailed?.Invoke(_reference, CreateAnalyzerFailedArgs(e2, item));
					reportedError = true;
					continue;
				}
				object obj2 = obj as TExtension;
				if (obj2 == null)
				{
					Func<object?, TExtension?>? coerceFunction = _coerceFunction;
					obj2 = ((coerceFunction != null) ? coerceFunction(obj) : null);
				}
				TExtension val = (TExtension)obj2;
				if (val != null)
				{
					instance.Add((item, val));
				}
			}
			instance.Sort(((string typeName, TExtension analyzer) x, (string typeName, TExtension analyzer) y) => string.Compare(x.typeName, y.typeName, StringComparison.OrdinalIgnoreCase));
			ImmutableArray<TExtension> result = instance.SelectAsArray(((string typeName, TExtension analyzer) x) => x.analyzer);
			instance.Free();
			return result;
			static IEnumerable<string> shuffle(ImmutableHashSet<string> source)
			{
				Random random = new Random();
				ArrayBuilder<string> builder = ArrayBuilder<string>.GetInstance(source.Count);
				builder.AddRange(source);
				for (int i = builder.Count - 1; i >= 0; i--)
				{
					int swapIndex = random.Next(i + 1);
					yield return builder[swapIndex];
					builder[swapIndex] = builder[i];
				}
				builder.Free();
			}
		}
	}

	private readonly IAnalyzerAssemblyLoader _assemblyLoader;

	private readonly Extensions<DiagnosticAnalyzer> _diagnosticAnalyzers;

	private readonly Extensions<ISourceGenerator> _generators;

	private string? _lazyDisplay;

	private object? _lazyIdentity;

	private Assembly? _lazyAssembly;

	public override string FullPath { get; }

	public IAnalyzerAssemblyLoader AssemblyLoader => _assemblyLoader;

	public override string Display
	{
		get
		{
			if (_lazyDisplay == null)
			{
				InitializeDisplayAndId();
			}
			return _lazyDisplay;
		}
	}

	public override object Id
	{
		get
		{
			if (_lazyIdentity == null)
			{
				InitializeDisplayAndId();
			}
			return _lazyIdentity;
		}
	}

	public event EventHandler<AnalyzerLoadFailureEventArgs>? AnalyzerLoadFailed;

	public AnalyzerFileReference(string fullPath, IAnalyzerAssemblyLoader assemblyLoader)
	{
		CompilerPathUtilities.RequireAbsolutePath(fullPath, "fullPath");
		FullPath = fullPath;
		_assemblyLoader = assemblyLoader ?? throw new ArgumentNullException("assemblyLoader");
		_diagnosticAnalyzers = new Extensions<DiagnosticAnalyzer>(this, typeof(DiagnosticAnalyzerAttribute), GetDiagnosticsAnalyzerSupportedLanguages, allowNetFramework: true);
		_generators = new Extensions<ISourceGenerator>(this, typeof(GeneratorAttribute), GetGeneratorSupportedLanguages, allowNetFramework: false, CoerceGeneratorType);
		assemblyLoader.AddDependencyLocation(fullPath);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as AnalyzerFileReference);
	}

	public bool Equals(AnalyzerFileReference? other)
	{
		if (this == other)
		{
			return true;
		}
		if (other != null && _assemblyLoader == other._assemblyLoader)
		{
			return FullPath == other.FullPath;
		}
		return false;
	}

	public bool Equals(AnalyzerReference? other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		if (other is AnalyzerFileReference other2)
		{
			return Equals(other2);
		}
		return FullPath == other.FullPath;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(RuntimeHelpers.GetHashCode(_assemblyLoader), FullPath.GetHashCode());
	}

	public override string ToString()
	{
		return "AnalyzerFileReference(FullPath = " + FullPath + ")";
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzersForAllLanguages()
	{
		return _diagnosticAnalyzers.GetExtensionsForAllLanguages(includeDuplicates: true);
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzers(string language)
	{
		return _diagnosticAnalyzers.GetExtensions(language);
	}

	public override ImmutableArray<ISourceGenerator> GetGeneratorsForAllLanguages()
	{
		return _generators.GetExtensionsForAllLanguages(includeDuplicates: false);
	}

	[Obsolete("Use GetGenerators(string language) or GetGeneratorsForAllLanguages()")]
	public override ImmutableArray<ISourceGenerator> GetGenerators()
	{
		return _generators.GetExtensions("C#");
	}

	public override ImmutableArray<ISourceGenerator> GetGenerators(string language)
	{
		return _generators.GetExtensions(language);
	}

	[MemberNotNull(new string[] { "_lazyIdentity", "_lazyDisplay" })]
	private void InitializeDisplayAndId()
	{
		try
		{
			using PEReader peReader = new PEReader(FileUtilities.OpenRead(FullPath));
			AssemblyIdentity assemblyIdentity = peReader.GetMetadataReader().ReadAssemblyIdentityOrThrow();
			_lazyDisplay = assemblyIdentity.Name;
			_lazyIdentity = assemblyIdentity;
		}
		catch
		{
			_lazyDisplay = FileNameUtilities.GetFileName(FullPath, includeExtension: false);
			_lazyIdentity = _lazyDisplay;
		}
	}

	internal void AddAnalyzers(ImmutableArray<DiagnosticAnalyzer>.Builder builder, string language, Func<DiagnosticAnalyzer, bool>? shouldInclude = null)
	{
		_diagnosticAnalyzers.AddExtensions(builder, language, shouldInclude);
	}

	internal void AddGenerators(ImmutableArray<ISourceGenerator>.Builder builder, string language)
	{
		_generators.AddExtensions(builder, language);
	}

	private static AnalyzerLoadFailureEventArgs CreateAnalyzerFailedArgs(Exception e, string? typeName = null)
	{
		string message = e.Message.Replace("\r", "").Replace("\n", "");
		return new AnalyzerLoadFailureEventArgs((typeName == null) ? AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToLoadAnalyzer : AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToCreateAnalyzer, message, e, typeName);
	}

	internal ImmutableSortedDictionary<string, ImmutableHashSet<string>> GetAnalyzerTypeNameMap()
	{
		return _diagnosticAnalyzers.GetExtensionTypeNameMap();
	}

	private static ImmutableSortedDictionary<string, ImmutableHashSet<string>> GetAnalyzerTypeNameMap(string fullPath, Type attributeType, AttributeLanguagesFunc languagesFunc)
	{
		using AssemblyMetadata assemblyMetadata = AssemblyMetadata.CreateFromFile(fullPath);
		Dictionary<string, ImmutableHashSet<string>.Builder> dictionary = new Dictionary<string, ImmutableHashSet<string>.Builder>(StringComparer.OrdinalIgnoreCase);
		foreach (ModuleMetadata module in assemblyMetadata.GetModules())
		{
			foreach (TypeDefinitionHandle typeDefinition2 in module.MetadataReader.TypeDefinitions)
			{
				TypeDefinition typeDefinition = module.MetadataReader.GetTypeDefinition(typeDefinition2);
				ImmutableArray<string> supportedLanguages = GetSupportedLanguages(typeDefinition, module.Module, attributeType, languagesFunc);
				if (supportedLanguages.Length <= 0)
				{
					continue;
				}
				string fullyQualifiedTypeName = GetFullyQualifiedTypeName(typeDefinition, module.Module);
				foreach (string item in supportedLanguages)
				{
					if (!dictionary.TryGetValue(item, out var value))
					{
						value = ImmutableHashSet.CreateBuilder<string>();
						dictionary.Add(item, value);
					}
					value.Add(fullyQualifiedTypeName);
				}
			}
		}
		return dictionary.ToImmutableSortedDictionary<KeyValuePair<string, ImmutableHashSet<string>.Builder>, string, ImmutableHashSet<string>>((KeyValuePair<string, ImmutableHashSet<string>.Builder> g) => g.Key, (KeyValuePair<string, ImmutableHashSet<string>.Builder> g) => g.Value.ToImmutable(), StringComparer.OrdinalIgnoreCase);
	}

	private static ImmutableArray<string> GetSupportedLanguages(TypeDefinition typeDef, PEModule peModule, Type attributeType, AttributeLanguagesFunc languagesFunc)
	{
		ImmutableArray<string> result = ImmutableArray<string>.Empty;
		foreach (CustomAttributeHandle customAttribute in typeDef.GetCustomAttributes())
		{
			if (peModule.IsTargetAttribute(customAttribute, attributeType.Namespace, attributeType.Name, out var _))
			{
				ImmutableArray<string> immutableArray = languagesFunc(peModule, customAttribute);
				result = ((!result.IsDefaultOrEmpty) ? result.AddRange(immutableArray) : immutableArray);
			}
		}
		return result;
	}

	private static ImmutableArray<string> GetDiagnosticsAnalyzerSupportedLanguages(PEModule peModule, CustomAttributeHandle customAttrHandle)
	{
		BlobReader argsReader = peModule.GetMemoryReaderOrThrow(peModule.GetCustomAttributeValueOrThrow(customAttrHandle));
		return ReadLanguagesFromAttribute(ref argsReader);
	}

	private static ImmutableArray<string> GetGeneratorSupportedLanguages(PEModule peModule, CustomAttributeHandle customAttrHandle)
	{
		BlobReader argsReader = peModule.GetMemoryReaderOrThrow(peModule.GetCustomAttributeValueOrThrow(customAttrHandle));
		if (argsReader.Length == 4)
		{
			return ImmutableArray.Create("C#");
		}
		return ReadLanguagesFromAttribute(ref argsReader);
	}

	private static ImmutableArray<string> ReadLanguagesFromAttribute(ref BlobReader argsReader)
	{
		if (argsReader.Length > 4 && argsReader.ReadByte() == 1 && argsReader.ReadByte() == 0)
		{
			if (!PEModule.CrackStringInAttributeValue(out string value, ref argsReader))
			{
				return ImmutableArray<string>.Empty;
			}
			if (PEModule.CrackStringArrayInAttributeValue(out ImmutableArray<string> value2, ref argsReader))
			{
				if (value2.Length == 0)
				{
					return ImmutableCollectionsMarshal.AsImmutableArray(new string[1] { value });
				}
				return value2.Insert(0, value);
			}
		}
		return ImmutableArray<string>.Empty;
	}

	private static ISourceGenerator? CoerceGeneratorType(object? generator)
	{
		if (generator is IIncrementalGenerator generator2)
		{
			return new IncrementalGeneratorWrapper(generator2);
		}
		return null;
	}

	private static string GetFullyQualifiedTypeName(TypeDefinition typeDef, PEModule peModule)
	{
		TypeDefinitionHandle declaringType = typeDef.GetDeclaringType();
		if (declaringType.IsNil)
		{
			return peModule.GetFullNameOrThrow(typeDef.Namespace, typeDef.Name);
		}
		return GetFullyQualifiedTypeName(peModule.MetadataReader.GetTypeDefinition(declaringType), peModule) + "+" + peModule.MetadataReader.GetString(typeDef.Name);
	}

	public Assembly GetAssembly()
	{
		if (_lazyAssembly == null)
		{
			_lazyAssembly = _assemblyLoader.LoadFromPath(FullPath);
		}
		return _lazyAssembly;
	}
}
