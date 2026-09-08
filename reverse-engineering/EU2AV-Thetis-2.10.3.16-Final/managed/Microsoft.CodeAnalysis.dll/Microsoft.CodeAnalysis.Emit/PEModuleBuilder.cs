using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit.NoPia;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class PEModuleBuilder<TCompilation, TSourceModuleSymbol, TAssemblySymbol, TTypeSymbol, TNamedTypeSymbol, TMethodSymbol, TSyntaxNode, TEmbeddedTypesManager, TModuleCompilationState> : CommonPEModuleBuilder where TCompilation : Compilation where TSourceModuleSymbol : class, IModuleSymbolInternal where TAssemblySymbol : class, IAssemblySymbolInternal where TTypeSymbol : class, ITypeSymbolInternal where TNamedTypeSymbol : class, TTypeSymbol, INamedTypeSymbolInternal where TMethodSymbol : class, IMethodSymbolInternal where TSyntaxNode : SyntaxNode where TEmbeddedTypesManager : CommonEmbeddedTypesManager where TModuleCompilationState : ModuleCompilationState<TNamedTypeSymbol, TMethodSymbol>
{
	private sealed class SynthesizedDefinitions
	{
		private ConcurrentQueue<INestedTypeDefinition> NestedTypes;

		public ConcurrentQueue<IMethodDefinition> Methods;

		public ConcurrentQueue<IPropertyDefinition> Properties;

		public ConcurrentQueue<IFieldDefinition> Fields;

		internal IEnumerable<INestedTypeDefinition> OrderedNestedTypes => NestedTypes?.OrderBy<INestedTypeDefinition, string>((INestedTypeDefinition t) => t.Name, StringComparer.Ordinal);

		internal void AddNestedType(INestedTypeDefinition nestedType)
		{
			if (NestedTypes == null)
			{
				Interlocked.CompareExchange(ref NestedTypes, new ConcurrentQueue<INestedTypeDefinition>(), null);
			}
			NestedTypes.Enqueue(nestedType);
		}

		public ImmutableArray<ISymbolInternal> GetAllMembers()
		{
			ArrayBuilder<ISymbolInternal> instance = ArrayBuilder<ISymbolInternal>.GetInstance();
			if (Fields != null)
			{
				foreach (IFieldDefinition field in Fields)
				{
					instance.Add(field.GetInternalSymbol());
				}
			}
			if (Methods != null)
			{
				foreach (IMethodDefinition method in Methods)
				{
					instance.Add(method.GetInternalSymbol());
				}
			}
			if (Properties != null)
			{
				foreach (IPropertyDefinition property in Properties)
				{
					instance.Add(property.GetInternalSymbol());
				}
			}
			if (NestedTypes != null)
			{
				foreach (INestedTypeDefinition orderedNestedType in OrderedNestedTypes)
				{
					instance.Add(orderedNestedType.GetInternalSymbol());
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	internal readonly TSourceModuleSymbol SourceModule;

	internal readonly TCompilation Compilation;

	private PrivateImplementationDetails _lazyPrivateImplementationDetails;

	private HashSet<string> _namesOfTopLevelTypes;

	internal readonly TModuleCompilationState CompilationState;

	private readonly RootModuleType _rootModuleType;

	private readonly ConcurrentDictionary<TNamedTypeSymbol, SynthesizedDefinitions> _synthesizedTypeMembers = new ConcurrentDictionary<TNamedTypeSymbol, SynthesizedDefinitions>(ReferenceEqualityComparer.Instance);

	private ConcurrentDictionary<INamespaceSymbolInternal, ConcurrentQueue<INamespaceOrTypeSymbolInternal>> _lazySynthesizedNamespaceMembers;

	public abstract TEmbeddedTypesManager EmbeddedTypesManagerOpt { get; }

	public RootModuleType RootModuleType => _rootModuleType;

	internal override IAssemblySymbolInternal CommonCorLibrary => CorLibrary;

	internal abstract TAssemblySymbol CorLibrary { get; }

	protected bool HaveDeterminedTopLevelTypes => _namesOfTopLevelTypes != null;

	internal sealed override IModuleSymbolInternal CommonSourceModule => SourceModule;

	internal sealed override Compilation CommonCompilation => Compilation;

	internal sealed override CommonModuleCompilationState CommonModuleCompilationState => CompilationState;

	internal sealed override CommonEmbeddedTypesManager CommonEmbeddedTypesManagerOpt => EmbeddedTypesManagerOpt;

	protected PEModuleBuilder(TCompilation compilation, TSourceModuleSymbol sourceModule, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources, OutputKind outputKind, EmitOptions emitOptions, TModuleCompilationState compilationState)
		: base(manifestResources, emitOptions, outputKind, serializationProperties, compilation)
	{
		Compilation = compilation;
		SourceModule = sourceModule;
		CompilationState = compilationState;
		_rootModuleType = new RootModuleType(this);
	}

	internal sealed override void CompilationFinished()
	{
		CompilationState.Freeze();
	}

	internal abstract INamedTypeReference GetSpecialType(SpecialType specialType, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal sealed override ITypeReference EncTranslateType(ITypeSymbolInternal type, DiagnosticBag diagnostics)
	{
		return EncTranslateLocalVariableType((TTypeSymbol)type, diagnostics);
	}

	internal virtual ITypeReference EncTranslateLocalVariableType(TTypeSymbol type, DiagnosticBag diagnostics)
	{
		return Translate(type, null, diagnostics);
	}

	protected bool ContainsTopLevelType(string fullEmittedName)
	{
		return _namesOfTopLevelTypes.Contains(fullEmittedName);
	}

	public override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypeDefinitions(EmitContext context)
	{
		TypeReferenceIndexer typeReferenceIndexer = null;
		HashSet<string> names = ((_namesOfTopLevelTypes != null) ? null : new HashSet<string>());
		if (EmbeddedTypesManagerOpt != null && !EmbeddedTypesManagerOpt.IsFrozen)
		{
			typeReferenceIndexer = new TypeReferenceIndexer(context);
			Dispatch(typeReferenceIndexer);
		}
		AddTopLevelType(names, RootModuleType);
		VisitTopLevelType(typeReferenceIndexer, RootModuleType);
		yield return RootModuleType;
		foreach (INamespaceTypeDefinition item in GetTopLevelTypeDefinitionsExcludingNoPiaAndRootModule(context, includePrivateImplementationDetails: true))
		{
			AddTopLevelType(names, item);
			VisitTopLevelType(typeReferenceIndexer, item);
			yield return item;
		}
		if (EmbeddedTypesManagerOpt != null)
		{
			foreach (INamespaceTypeDefinition type in EmbeddedTypesManagerOpt.GetTypes(context.Diagnostics, names))
			{
				AddTopLevelType(names, type);
				yield return type;
			}
		}
		if (names != null)
		{
			_namesOfTopLevelTypes = names;
		}
		static void AddTopLevelType(HashSet<string> hashSet, INamespaceTypeDefinition type)
		{
			hashSet?.Add(MetadataHelpers.BuildQualifiedName(type.NamespaceName, MetadataWriter.GetMetadataName(type, 0)));
		}
	}

	public virtual ImmutableArray<TNamedTypeSymbol> GetAdditionalTopLevelTypes()
	{
		return ImmutableArray<TNamedTypeSymbol>.Empty;
	}

	public virtual ImmutableArray<TNamedTypeSymbol> GetEmbeddedTypes(DiagnosticBag diagnostics)
	{
		return ImmutableArray<TNamedTypeSymbol>.Empty;
	}

	internal abstract IAssemblyReference Translate(TAssemblySymbol symbol, DiagnosticBag diagnostics);

	internal abstract ITypeReference Translate(TTypeSymbol symbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal abstract IMethodReference Translate(TMethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration);

	internal sealed override IAssemblyReference Translate(IAssemblySymbolInternal symbol, DiagnosticBag diagnostics)
	{
		return Translate((TAssemblySymbol)symbol, diagnostics);
	}

	internal sealed override ITypeReference Translate(ITypeSymbolInternal symbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return Translate((TTypeSymbol)symbol, (TSyntaxNode)syntaxNodeOpt, diagnostics);
	}

	internal sealed override IMethodReference Translate(IMethodSymbolInternal symbol, DiagnosticBag diagnostics, bool needDeclaration)
	{
		return Translate((TMethodSymbol)symbol, diagnostics, needDeclaration);
	}

	internal MetadataConstant CreateConstant(TTypeSymbol type, object value, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return new MetadataConstant(Translate(type, syntaxNodeOpt, diagnostics), value);
	}

	private static void VisitTopLevelType(TypeReferenceIndexer noPiaIndexer, INamespaceTypeDefinition type)
	{
		noPiaIndexer?.Visit((ITypeDefinition)type);
	}

	internal IFieldReference GetModuleVersionId(ITypeReference mvidType, TSyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		PrivateImplementationDetails privateImplClass = GetPrivateImplClass(syntaxOpt, diagnostics);
		EnsurePrivateImplementationDetailsStaticConstructor(privateImplClass, syntaxOpt, diagnostics);
		return privateImplClass.GetModuleVersionId(mvidType);
	}

	internal IFieldReference GetModuleCancellationToken(ITypeReference cancellationTokenType, TSyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		return GetPrivateImplClass(syntaxOpt, diagnostics).GetModuleCancellationToken(cancellationTokenType);
	}

	internal IFieldReference GetInstrumentationPayloadRoot(int analysisKind, ITypeReference payloadType, TSyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		PrivateImplementationDetails privateImplClass = GetPrivateImplClass(syntaxOpt, diagnostics);
		EnsurePrivateImplementationDetailsStaticConstructor(privateImplClass, syntaxOpt, diagnostics);
		return privateImplClass.GetOrAddInstrumentationPayloadRoot(analysisKind, payloadType);
	}

	private void EnsurePrivateImplementationDetailsStaticConstructor(PrivateImplementationDetails details, TSyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		if (details.GetMethod(".cctor") == null)
		{
			IMethodDefinition method = CreatePrivateImplementationDetailsStaticConstructor(syntaxOpt, diagnostics);
			details.TryAddSynthesizedMethod(method);
		}
	}

	protected abstract IMethodDefinition CreatePrivateImplementationDetailsStaticConstructor(TSyntaxNode syntaxOpt, DiagnosticBag diagnostics);

	internal abstract IEnumerable<INestedTypeDefinition> GetSynthesizedNestedTypes(TNamedTypeSymbol container);

	public IEnumerable<INestedTypeDefinition> GetSynthesizedTypes(TNamedTypeSymbol container)
	{
		IEnumerable<INestedTypeDefinition> synthesizedNestedTypes = GetSynthesizedNestedTypes(container);
		IEnumerable<INestedTypeDefinition> enumerable = null;
		if (_synthesizedTypeMembers.TryGetValue(container, out var value))
		{
			enumerable = value.OrderedNestedTypes;
		}
		if (synthesizedNestedTypes == null)
		{
			return enumerable;
		}
		if (enumerable == null)
		{
			return synthesizedNestedTypes;
		}
		return synthesizedNestedTypes.Concat(enumerable);
	}

	private SynthesizedDefinitions GetOrAddSynthesizedDefinitions(TNamedTypeSymbol container)
	{
		return _synthesizedTypeMembers.GetOrAdd(container, (TNamedTypeSymbol _) => new SynthesizedDefinitions());
	}

	public virtual void AddSynthesizedDefinition(TNamedTypeSymbol container, IMethodDefinition method)
	{
		SynthesizedDefinitions orAddSynthesizedDefinitions = GetOrAddSynthesizedDefinitions(container);
		if (orAddSynthesizedDefinitions.Methods == null)
		{
			Interlocked.CompareExchange(ref orAddSynthesizedDefinitions.Methods, new ConcurrentQueue<IMethodDefinition>(), null);
		}
		orAddSynthesizedDefinitions.Methods.Enqueue(method);
	}

	public virtual void AddSynthesizedDefinition(TNamedTypeSymbol container, IPropertyDefinition property)
	{
		SynthesizedDefinitions orAddSynthesizedDefinitions = GetOrAddSynthesizedDefinitions(container);
		if (orAddSynthesizedDefinitions.Properties == null)
		{
			Interlocked.CompareExchange(ref orAddSynthesizedDefinitions.Properties, new ConcurrentQueue<IPropertyDefinition>(), null);
		}
		orAddSynthesizedDefinitions.Properties.Enqueue(property);
	}

	public virtual void AddSynthesizedDefinition(TNamedTypeSymbol container, IFieldDefinition field)
	{
		SynthesizedDefinitions orAddSynthesizedDefinitions = GetOrAddSynthesizedDefinitions(container);
		if (orAddSynthesizedDefinitions.Fields == null)
		{
			Interlocked.CompareExchange(ref orAddSynthesizedDefinitions.Fields, new ConcurrentQueue<IFieldDefinition>(), null);
		}
		orAddSynthesizedDefinitions.Fields.Enqueue(field);
	}

	public virtual void AddSynthesizedDefinition(TNamedTypeSymbol container, INestedTypeDefinition nestedType)
	{
		GetOrAddSynthesizedDefinitions(container).AddNestedType(nestedType);
	}

	public void AddSynthesizedDefinition(INamespaceSymbolInternal container, INamespaceOrTypeSymbolInternal typeOrNamespace)
	{
		if (_lazySynthesizedNamespaceMembers == null)
		{
			Interlocked.CompareExchange(ref _lazySynthesizedNamespaceMembers, new ConcurrentDictionary<INamespaceSymbolInternal, ConcurrentQueue<INamespaceOrTypeSymbolInternal>>(), null);
		}
		_lazySynthesizedNamespaceMembers.GetOrAdd(container, (INamespaceSymbolInternal _) => new ConcurrentQueue<INamespaceOrTypeSymbolInternal>()).Enqueue(typeOrNamespace);
	}

	public IEnumerable<IFieldDefinition> GetSynthesizedFields(TNamedTypeSymbol container)
	{
		if (!_synthesizedTypeMembers.TryGetValue(container, out var value))
		{
			return null;
		}
		return value.Fields;
	}

	public IEnumerable<IPropertyDefinition> GetSynthesizedProperties(TNamedTypeSymbol container)
	{
		if (!_synthesizedTypeMembers.TryGetValue(container, out var value))
		{
			return null;
		}
		return value.Properties;
	}

	public IEnumerable<IMethodDefinition> GetSynthesizedMethods(TNamedTypeSymbol container)
	{
		if (!_synthesizedTypeMembers.TryGetValue(container, out var value))
		{
			return null;
		}
		return value.Methods;
	}

	internal override ImmutableDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> GetAllSynthesizedMembers()
	{
		ImmutableDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>>.Builder builder = ImmutableDictionary.CreateBuilder<ISymbolInternal, ImmutableArray<ISymbolInternal>>();
		foreach (KeyValuePair<TNamedTypeSymbol, SynthesizedDefinitions> synthesizedTypeMember in _synthesizedTypeMembers)
		{
			builder.Add(synthesizedTypeMember.Key, synthesizedTypeMember.Value.GetAllMembers());
		}
		ConcurrentDictionary<INamespaceSymbolInternal, ConcurrentQueue<INamespaceOrTypeSymbolInternal>> lazySynthesizedNamespaceMembers = _lazySynthesizedNamespaceMembers;
		if (lazySynthesizedNamespaceMembers != null)
		{
			foreach (KeyValuePair<INamespaceSymbolInternal, ConcurrentQueue<INamespaceOrTypeSymbolInternal>> item in lazySynthesizedNamespaceMembers)
			{
				builder.Add(item.Key, ((IEnumerable<ISymbolInternal>)item.Value).ToImmutableArray());
			}
		}
		INamedTypeSymbolInternal usedSynthesizedHotReloadExceptionType = GetUsedSynthesizedHotReloadExceptionType();
		if (usedSynthesizedHotReloadExceptionType != null)
		{
			if (!builder.TryGetValue(usedSynthesizedHotReloadExceptionType.ContainingNamespace, out var value))
			{
				value = ImmutableArray<ISymbolInternal>.Empty;
			}
			builder[usedSynthesizedHotReloadExceptionType.ContainingNamespace] = value.Add(usedSynthesizedHotReloadExceptionType);
		}
		return builder.ToImmutable();
	}

	internal sealed override SynthesizedTypeMaps GetAllSynthesizedTypes()
	{
		return Compilation.CommonAnonymousTypeManager.GetSynthesizedTypeMaps();
	}

	public sealed override IFieldReference GetFieldForData(ImmutableArray<byte> data, ushort alignment, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		return GetPrivateImplClass((TSyntaxNode)syntaxNode, diagnostics).GetOrAddDataField(data, alignment);
	}

	public sealed override IFieldReference GetArrayCachingFieldForData(ImmutableArray<byte> data, IArrayTypeReference arrayType, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		PrivateImplementationDetails privateImplClass = GetPrivateImplClass((TSyntaxNode)syntaxNode, diagnostics);
		EmitContext emitContext = new EmitContext(this, syntaxNode, diagnostics, metadataOnly: false, includePrivateMembers: true);
		return privateImplClass.CreateArrayCachingField(data, arrayType, emitContext);
	}

	public sealed override IFieldReference GetArrayCachingFieldForConstants(ImmutableArray<ConstantValue> constants, IArrayTypeReference arrayType, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		PrivateImplementationDetails privateImplClass = GetPrivateImplClass((TSyntaxNode)syntaxNode, diagnostics);
		EmitContext emitContext = new EmitContext(this, syntaxNode, diagnostics, metadataOnly: false, includePrivateMembers: true);
		return privateImplClass.CreateArrayCachingField(constants, arrayType, emitContext);
	}

	internal PrivateImplementationDetails GetPrivateImplClass(TSyntaxNode? syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		PrivateImplementationDetails privateImplementationDetails = _lazyPrivateImplementationDetails;
		if (privateImplementationDetails == null)
		{
			privateImplementationDetails = new PrivateImplementationDetails(this, SourceModule.Name, Compilation.GetSubmissionSlotIndex(), GetSpecialType(SpecialType.System_Object, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_ValueType, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_Byte, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_Int16, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_Int32, syntaxNodeOpt, diagnostics), GetSpecialType(SpecialType.System_Int64, syntaxNodeOpt, diagnostics), SynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
			if (Interlocked.CompareExchange(ref _lazyPrivateImplementationDetails, privateImplementationDetails, null) != null)
			{
				privateImplementationDetails = _lazyPrivateImplementationDetails;
			}
		}
		return privateImplementationDetails;
	}

	internal override PrivateImplementationDetails GetPrivateImplClass(SyntaxNode? syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return GetPrivateImplClass((TSyntaxNode)syntaxNodeOpt, diagnostics);
	}

	public PrivateImplementationDetails? FreezePrivateImplementationDetails()
	{
		_lazyPrivateImplementationDetails?.Freeze();
		return _lazyPrivateImplementationDetails;
	}

	public override PrivateImplementationDetails? GetFrozenPrivateImplementationDetails()
	{
		return _lazyPrivateImplementationDetails;
	}

	public sealed override ITypeReference GetPlatformType(PlatformType platformType, EmitContext context)
	{
		if (platformType == PlatformType.SystemType)
		{
			throw ExceptionUtilities.UnexpectedValue(platformType);
		}
		return GetSpecialType((SpecialType)platformType, (TSyntaxNode)context.SyntaxNode, context.Diagnostics);
	}
}
