using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit.NoPia;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class CommonPEModuleBuilder : IUnit, IUnitReference, IReference, INamedEntity, IDefinition, IModuleReference
{
	internal readonly DebugDocumentsBuilder DebugDocumentsBuilder;

	internal readonly IEnumerable<ResourceDescription> ManifestResources;

	internal readonly ModulePropertiesForSerialization SerializationProperties;

	internal readonly OutputKind OutputKind;

	internal Stream? RawWin32Resources;

	internal IEnumerable<IWin32Resource>? Win32Resources;

	internal ResourceSection? Win32ResourceSection;

	internal Stream? SourceLinkStreamOpt;

	internal IMethodReference? PEEntryPoint;

	internal IMethodReference? DebugEntryPoint;

	private readonly ConcurrentDictionary<IMethodSymbolInternal, IMethodBody> _methodBodyMap;

	private readonly TokenMap _referencesInILMap = new TokenMap();

	private readonly Lazy<StringTokenMap> _stringsInILMap;

	private readonly ItemTokenMap<DebugSourceDocument> _sourceDocumentsInILMap = new ItemTokenMap<DebugSourceDocument>();

	private ImmutableArray<AssemblyReferenceAlias> _lazyAssemblyReferenceAliases;

	private ImmutableArray<ManagedResource> _lazyManagedResources;

	private IEnumerable<EmbeddedText> _embeddedTexts = SpecializedCollections.EmptyEnumerable<EmbeddedText>();

	private ArrayMethods? _lazyArrayMethods;

	private IReadOnlyDictionary<ITypeDefinition, ArrayBuilder<ITypeDefinitionMember>>? _encDeletedMemberDefinitions;

	internal CompilationTestData? TestData { get; private set; }

	internal EmitOptions EmitOptions { get; }

	internal DebugInformationFormat DebugInformationFormat => EmitOptions.DebugInformationFormat;

	internal HashAlgorithmName PdbChecksumAlgorithm => EmitOptions.PdbChecksumAlgorithm;

	public abstract SymbolChanges? EncSymbolChanges { get; }

	public abstract bool FieldRvaSupported { get; }

	public abstract bool MethodImplSupported { get; }

	public abstract EmitBaseline? PreviousGeneration { get; }

	public bool IsEncDelta => PreviousGeneration != null;

	public int CurrentGenerationOrdinal
	{
		get
		{
			EmitBaseline? previousGeneration = PreviousGeneration;
			if (previousGeneration == null)
			{
				return 0;
			}
			return previousGeneration.Ordinal + 1;
		}
	}

	public abstract string Name { get; }

	internal abstract string ModuleName { get; }

	internal abstract Compilation CommonCompilation { get; }

	internal abstract IModuleSymbolInternal CommonSourceModule { get; }

	internal abstract IAssemblySymbolInternal CommonCorLibrary { get; }

	internal abstract CommonModuleCompilationState CommonModuleCompilationState { get; }

	internal abstract CommonEmbeddedTypesManager CommonEmbeddedTypesManagerOpt { get; }

	public abstract bool GenerateVisualBasicStylePdb { get; }

	public abstract IEnumerable<string> LinkedAssembliesDebugInfo { get; }

	public abstract string DefaultNamespace { get; }

	public ArrayMethods ArrayMethods
	{
		get
		{
			ArrayMethods arrayMethods = _lazyArrayMethods;
			if (arrayMethods == null)
			{
				arrayMethods = new ArrayMethods();
				if (Interlocked.CompareExchange(ref _lazyArrayMethods, arrayMethods, null) != null)
				{
					arrayMethods = _lazyArrayMethods;
				}
			}
			return arrayMethods;
		}
	}

	bool IDefinition.IsEncDeleted => false;

	public int DebugDocumentCount => DebugDocumentsBuilder.DebugDocumentCount;

	public abstract ISourceAssemblySymbolInternal SourceAssemblyOpt { get; }

	public int HintNumberOfMethodDefinitions => (int)((double)_methodBodyMap.Count * 1.5);

	public IEnumerable<EmbeddedText> EmbeddedTexts
	{
		get
		{
			return _embeddedTexts;
		}
		set
		{
			_embeddedTexts = value;
		}
	}

	protected CommonPEModuleBuilder(IEnumerable<ResourceDescription> manifestResources, EmitOptions emitOptions, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, Compilation compilation)
	{
		ManifestResources = manifestResources;
		DebugDocumentsBuilder = new DebugDocumentsBuilder(compilation.Options.SourceReferenceResolver, compilation.IsCaseSensitive);
		OutputKind = outputKind;
		SerializationProperties = serializationProperties;
		_methodBodyMap = new ConcurrentDictionary<IMethodSymbolInternal, IMethodBody>(ReferenceEqualityComparer.Instance);
		_stringsInILMap = new Lazy<StringTokenMap>(() => new StringTokenMap(PreviousGeneration?.UserStringStreamLength ?? 0));
		EmitOptions = emitOptions;
	}

	public abstract IMethodSymbolInternal GetOrCreateHotReloadExceptionConstructorDefinition();

	public abstract INamedTypeSymbolInternal? TryGetOrCreateSynthesizedHotReloadExceptionType();

	public abstract INamedTypeSymbolInternal? GetUsedSynthesizedHotReloadExceptionType();

	public void CreateDeletedMemberDefinitions(DiagnosticBag diagnosticBag)
	{
		if (EncSymbolChanges != null)
		{
			EmitContext context = new EmitContext(this, diagnosticBag, metadataOnly: false, includePrivateMembers: true);
			_encDeletedMemberDefinitions = DeltaMetadataWriter.CreateDeletedMemberDefs(context, EncSymbolChanges);
		}
	}

	public IReadOnlyDictionary<ITypeDefinition, ArrayBuilder<ITypeDefinitionMember>> GetDeletedMemberDefinitions()
	{
		return _encDeletedMemberDefinitions;
	}

	internal abstract IAssemblyReference Translate(IAssemblySymbolInternal symbol, DiagnosticBag diagnostics);

	internal abstract ITypeReference Translate(ITypeSymbolInternal symbol, SyntaxNode syntaxOpt, DiagnosticBag diagnostics);

	internal abstract IMethodReference Translate(IMethodSymbolInternal symbol, DiagnosticBag diagnostics, bool needDeclaration);

	internal abstract void CompilationFinished();

	internal abstract ImmutableDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> GetAllSynthesizedMembers();

	internal abstract SynthesizedTypeMaps GetAllSynthesizedTypes();

	internal abstract ITypeReference EncTranslateType(ITypeSymbolInternal type, DiagnosticBag diagnostics);

	public abstract IEnumerable<ICustomAttribute> GetSourceAssemblyAttributes(bool isRefAssembly);

	public abstract IEnumerable<SecurityAttribute> GetSourceAssemblySecurityAttributes();

	public abstract IEnumerable<ICustomAttribute> GetSourceModuleAttributes();

	internal abstract ICustomAttribute? SynthesizeAttribute(WellKnownMember attributeConstructor);

	public abstract IMethodReference GetInitArrayHelper();

	public abstract IFieldReference GetFieldForData(ImmutableArray<byte> data, ushort alignment, SyntaxNode syntaxNode, DiagnosticBag diagnostics);

	public abstract IFieldReference GetArrayCachingFieldForData(ImmutableArray<byte> data, IArrayTypeReference arrayType, SyntaxNode syntaxNode, DiagnosticBag diagnostics);

	public abstract IFieldReference GetArrayCachingFieldForConstants(ImmutableArray<ConstantValue> constants, IArrayTypeReference arrayType, SyntaxNode syntaxNode, DiagnosticBag diagnostics);

	public abstract ImmutableArray<ExportedType> GetExportedTypes(EmitContext context);

	public abstract ImmutableArray<UsedNamespaceOrType> GetImports();

	protected abstract IAssemblyReference GetCorLibraryReferenceToEmit(EmitContext context);

	protected abstract IEnumerable<IAssemblyReference> GetAssemblyReferencesFromAddedModules(DiagnosticBag diagnostics);

	protected abstract void AddEmbeddedResourcesFromAddedModules(ArrayBuilder<ManagedResource> builder, DiagnosticBag diagnostics);

	public abstract ITypeReference GetPlatformType(PlatformType platformType, EmitContext context);

	public abstract bool IsPlatformType(ITypeReference typeRef, PlatformType platformType);

	public IFieldReference? TryGetOrCreateFieldForStringValue(string text, SyntaxNode? syntaxNode, DiagnosticBag diagnostics)
	{
		return PrivateImplementationDetails.TryGetOrCreateFieldForStringValue(text, this, syntaxNode, diagnostics);
	}

	public abstract IEnumerable<INamespaceTypeDefinition> GetTopLevelTypeDefinitions(EmitContext context);

	public IEnumerable<INamespaceTypeDefinition> GetTopLevelTypeDefinitionsExcludingNoPiaAndRootModule(EmitContext context, bool includePrivateImplementationDetails)
	{
		foreach (INamespaceTypeDefinition anonymousTypeDefinition in GetAnonymousTypeDefinitions(context))
		{
			yield return anonymousTypeDefinition;
		}
		foreach (INamespaceTypeDefinition additionalTopLevelTypeDefinition in GetAdditionalTopLevelTypeDefinitions(context))
		{
			yield return additionalTopLevelTypeDefinition;
		}
		foreach (INamespaceTypeDefinition embeddedTypeDefinition in GetEmbeddedTypeDefinitions(context))
		{
			yield return embeddedTypeDefinition;
		}
		foreach (INamespaceTypeDefinition topLevelSourceTypeDefinition in GetTopLevelSourceTypeDefinitions(context))
		{
			yield return topLevelSourceTypeDefinition;
		}
		if (includePrivateImplementationDetails)
		{
			PrivateImplementationDetails frozenPrivateImplementationDetails = GetFrozenPrivateImplementationDetails();
			if (frozenPrivateImplementationDetails != null)
			{
				yield return frozenPrivateImplementationDetails;
			}
		}
	}

	public abstract PrivateImplementationDetails? GetFrozenPrivateImplementationDetails();

	internal abstract PrivateImplementationDetails GetPrivateImplClass(SyntaxNode? syntaxNode, DiagnosticBag diagnostics);

	public abstract IEnumerable<INamespaceTypeDefinition> GetAdditionalTopLevelTypeDefinitions(EmitContext context);

	public abstract IEnumerable<INamespaceTypeDefinition> GetAnonymousTypeDefinitions(EmitContext context);

	public abstract IEnumerable<INamespaceTypeDefinition> GetEmbeddedTypeDefinitions(EmitContext context);

	public abstract IEnumerable<INamespaceTypeDefinition> GetTopLevelSourceTypeDefinitions(EmitContext context);

	public abstract IEnumerable<IFileReference> GetFiles(EmitContext context);

	public abstract MultiDictionary<DebugSourceDocument, DefinitionWithLocation> GetSymbolToLocationMap();

	public abstract IEnumerable<(ITypeDefinition, ImmutableArray<DebugSourceDocument>)> GetTypeToDebugDocumentMap(EmitContext context);

	public void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return this;
	}

	ISymbolInternal IReference.GetInternalSymbol()
	{
		return null;
	}

	internal IMethodBody? GetMethodBody(IMethodSymbolInternal methodSymbol)
	{
		if (_methodBodyMap.TryGetValue(methodSymbol, out IMethodBody value))
		{
			return value;
		}
		return null;
	}

	public void SetMethodBody(IMethodSymbolInternal methodSymbol, IMethodBody body)
	{
		_methodBodyMap.Add(methodSymbol, body);
	}

	internal void SetPEEntryPoint(IMethodSymbolInternal method, DiagnosticBag diagnostics)
	{
		PEEntryPoint = Translate(method, diagnostics, needDeclaration: true);
	}

	internal void SetDebugEntryPoint(IMethodSymbolInternal method, DiagnosticBag diagnostics)
	{
		DebugEntryPoint = Translate(method, diagnostics, needDeclaration: true);
	}

	private bool IsSourceDefinition(IMethodSymbolInternal method)
	{
		if (method.ContainingModule == CommonSourceModule)
		{
			return method.IsDefinition;
		}
		return false;
	}

	public IAssemblyReference GetCorLibrary(EmitContext context)
	{
		return Translate(CommonCorLibrary, context.Diagnostics);
	}

	public IAssemblyReference GetContainingAssembly(EmitContext context)
	{
		if (OutputKind != OutputKind.NetModule)
		{
			return (IAssemblyReference)this;
		}
		return null;
	}

	public string[] CopyStrings()
	{
		return _stringsInILMap.Value.CopyValues();
	}

	public uint GetFakeSymbolTokenForIL(IReference symbol, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		uint orAddTokenFor = _referencesInILMap.GetOrAddTokenFor(symbol, out var referenceAdded);
		if (referenceAdded)
		{
			ReferenceDependencyWalker.VisitReference(symbol, new EmitContext(this, syntaxNode, diagnostics, metadataOnly: false, includePrivateMembers: true));
		}
		return orAddTokenFor;
	}

	public uint GetFakeSymbolTokenForIL(ISignature symbol, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		uint orAddTokenFor = _referencesInILMap.GetOrAddTokenFor(symbol, out var referenceAdded);
		if (referenceAdded)
		{
			ReferenceDependencyWalker.VisitSignature(symbol, new EmitContext(this, syntaxNode, diagnostics, metadataOnly: false, includePrivateMembers: true));
		}
		return orAddTokenFor;
	}

	public uint GetSourceDocumentIndexForIL(DebugSourceDocument document)
	{
		return _sourceDocumentsInILMap.GetOrAddTokenFor(document);
	}

	internal DebugSourceDocument GetSourceDocumentFromIndex(uint token)
	{
		return _sourceDocumentsInILMap.GetItem(token);
	}

	public object GetReferenceFromToken(uint token)
	{
		return _referencesInILMap.GetItem(token);
	}

	public bool TryGetFakeStringTokenForIL(string str, out uint token)
	{
		return _stringsInILMap.Value.TryGetOrAddToken(str, out token);
	}

	public string GetStringFromToken(uint token)
	{
		return _stringsInILMap.Value.GetValue(token);
	}

	public ReadOnlySpan<object> ReferencesInIL()
	{
		return _referencesInILMap.GetAllItems();
	}

	public ImmutableArray<AssemblyReferenceAlias> GetAssemblyReferenceAliases(EmitContext context)
	{
		if (_lazyAssemblyReferenceAliases.IsDefault)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyAssemblyReferenceAliases, CalculateAssemblyReferenceAliases(context), default(ImmutableArray<AssemblyReferenceAlias>));
		}
		return _lazyAssemblyReferenceAliases;
	}

	private ImmutableArray<AssemblyReferenceAlias> CalculateAssemblyReferenceAliases(EmitContext context)
	{
		ArrayBuilder<AssemblyReferenceAlias> instance = ArrayBuilder<AssemblyReferenceAlias>.GetInstance();
		foreach (var referencedAssemblyAlias in CommonCompilation.GetBoundReferenceManager().GetReferencedAssemblyAliases())
		{
			IAssemblySymbolInternal item = referencedAssemblyAlias.AssemblySymbol;
			ImmutableArray<string> item2 = referencedAssemblyAlias.Aliases;
			for (int i = 0; i < item2.Length; i++)
			{
				string text = item2[i];
				if (text != MetadataReferenceProperties.GlobalAlias && item2.IndexOf(text, 0, i) < 0)
				{
					instance.Add(new AssemblyReferenceAlias(text, Translate(item, context.Diagnostics)));
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	public IEnumerable<IAssemblyReference> GetAssemblyReferences(EmitContext context)
	{
		IAssemblyReference corLibraryReferenceToEmit = GetCorLibraryReferenceToEmit(context);
		if (corLibraryReferenceToEmit != null)
		{
			yield return corLibraryReferenceToEmit;
		}
		if (OutputKind == OutputKind.NetModule)
		{
			yield break;
		}
		foreach (IAssemblyReference assemblyReferencesFromAddedModule in GetAssemblyReferencesFromAddedModules(context.Diagnostics))
		{
			yield return assemblyReferencesFromAddedModule;
		}
	}

	public ImmutableArray<ManagedResource> GetResources(EmitContext context)
	{
		if (context.IsRefAssembly)
		{
			return ImmutableArray<ManagedResource>.Empty;
		}
		if (_lazyManagedResources.IsDefault)
		{
			ArrayBuilder<ManagedResource> instance = ArrayBuilder<ManagedResource>.GetInstance();
			foreach (ResourceDescription manifestResource in ManifestResources)
			{
				instance.Add(manifestResource.ToManagedResource());
			}
			if (OutputKind != OutputKind.NetModule)
			{
				AddEmbeddedResourcesFromAddedModules(instance, context.Diagnostics);
			}
			_lazyManagedResources = instance.ToImmutableAndFree();
		}
		return _lazyManagedResources;
	}

	internal void SetTestData(CompilationTestData testData)
	{
		TestData = testData;
		testData.Module = this;
	}

	public int GetTypeDefinitionGeneration(INamedTypeDefinition typeDef)
	{
		if (PreviousGeneration != null)
		{
			SymbolChanges encSymbolChanges = EncSymbolChanges;
			if (encSymbolChanges.IsReplacedDef(typeDef))
			{
				return CurrentGenerationOrdinal;
			}
			IDefinition definition = encSymbolChanges.DefinitionMap.MapDefinition(typeDef);
			if (definition != null && PreviousGeneration.GenerationOrdinals.TryGetValue(definition, out var value))
			{
				return value;
			}
		}
		return 0;
	}
}
