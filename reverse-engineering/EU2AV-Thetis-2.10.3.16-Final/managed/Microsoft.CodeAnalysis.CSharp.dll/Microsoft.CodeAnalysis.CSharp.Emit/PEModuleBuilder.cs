using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit.NoPia;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal abstract class PEModuleBuilder : PEModuleBuilder<CSharpCompilation, SourceModuleSymbol, AssemblySymbol, TypeSymbol, NamedTypeSymbol, MethodSymbol, SyntaxNode, EmbeddedTypesManager, ModuleCompilationState>
{
	protected readonly ConcurrentDictionary<Symbol, IModuleReference> AssemblyOrModuleSymbolToModuleRefMap = new ConcurrentDictionary<Symbol, IModuleReference>();

	private readonly ConcurrentDictionary<Symbol, object> _genericInstanceMap = new ConcurrentDictionary<Symbol, object>(Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything);

	private readonly ConcurrentDictionary<ImportChain, ImmutableArray<UsedNamespaceOrType>> _translatedImportsMap = new ConcurrentDictionary<ImportChain, ImmutableArray<UsedNamespaceOrType>>(ReferenceEqualityComparer.Instance);

	private readonly ConcurrentSet<TypeSymbol> _reportedErrorTypesMap = new ConcurrentSet<TypeSymbol>();

	private readonly EmbeddedTypesManager _embeddedTypesManagerOpt;

	private readonly string _metadataName;

	private ImmutableArray<ExportedType> _lazyExportedTypes;

	private Dictionary<FieldSymbol, NamedTypeSymbol> _fixedImplementationTypes;

	private SynthesizedPrivateImplementationDetailsType _lazyPrivateImplementationDetailsClass;

	private readonly ConcurrentDictionary<int, NamedTypeSymbol> _inlineArrayTypes = new ConcurrentDictionary<int, NamedTypeSymbol>();

	private readonly NamedTypeSymbol[] _readOnlyListTypes = new NamedTypeSymbol[3];

	private int _needsGeneratedAttributes;

	private bool _needsGeneratedAttributes_IsFrozen;

	public override EmbeddedTypesManager EmbeddedTypesManagerOpt => _embeddedTypesManagerOpt;

	public override string Name => _metadataName;

	internal sealed override string ModuleName => _metadataName;

	internal sealed override AssemblySymbol CorLibrary => SourceModule.ContainingSourceAssembly.CorLibrary;

	public sealed override bool GenerateVisualBasicStylePdb => false;

	public sealed override IEnumerable<string> LinkedAssembliesDebugInfo => SpecializedCollections.EmptyEnumerable<string>();

	public sealed override string DefaultNamespace => null;

	internal virtual bool IgnoreAccessibility => false;

	internal EmbeddableAttributes GetNeedsGeneratedAttributes()
	{
		_needsGeneratedAttributes_IsFrozen = true;
		return GetNeedsGeneratedAttributesInternal();
	}

	private EmbeddableAttributes GetNeedsGeneratedAttributesInternal()
	{
		return (EmbeddableAttributes)(_needsGeneratedAttributes | (int)Compilation.GetNeedsGeneratedAttributes(!(this is PEDeltaAssemblyBuilder)));
	}

	private void SetNeedsGeneratedAttributes(EmbeddableAttributes attributes)
	{
		ThreadSafeFlagOperations.Set(ref _needsGeneratedAttributes, (int)attributes);
	}

	protected PEModuleBuilder(SourceModuleSymbol sourceModule, EmitOptions emitOptions, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources)
		: base(sourceModule.ContainingSourceAssembly.DeclaringCompilation, sourceModule, serializationProperties, manifestResources, outputKind, emitOptions, new ModuleCompilationState())
	{
		string metadataName = sourceModule.MetadataName;
		_metadataName = ((metadataName != "?") ? metadataName : (emitOptions.OutputNameOverride ?? metadataName));
		AssemblyOrModuleSymbolToModuleRefMap.Add(sourceModule, this);
		if (sourceModule.AnyReferencedAssembliesAreLinked)
		{
			_embeddedTypesManagerOpt = new EmbeddedTypesManager(this);
		}
	}

	internal sealed override ICustomAttribute? SynthesizeAttribute(WellKnownMember attributeConstructor)
	{
		return Compilation.TrySynthesizeAttribute(attributeConstructor);
	}

	public sealed override IEnumerable<ICustomAttribute> GetSourceAssemblyAttributes(bool isRefAssembly)
	{
		return SourceModule.ContainingSourceAssembly.GetCustomAttributesToEmit(this, isRefAssembly, OutputKind.IsNetModule());
	}

	public sealed override IEnumerable<SecurityAttribute> GetSourceAssemblySecurityAttributes()
	{
		return SourceModule.ContainingSourceAssembly.GetSecurityAttributes();
	}

	public sealed override IEnumerable<ICustomAttribute> GetSourceModuleAttributes()
	{
		return SourceModule.GetCustomAttributesToEmit(this);
	}

	public sealed override ImmutableArray<UsedNamespaceOrType> GetImports()
	{
		return ImmutableArray<UsedNamespaceOrType>.Empty;
	}

	protected sealed override IEnumerable<IAssemblyReference> GetAssemblyReferencesFromAddedModules(DiagnosticBag diagnostics)
	{
		ImmutableArray<ModuleSymbol> modules = SourceModule.ContainingAssembly.Modules;
		for (int i = 1; i < modules.Length; i++)
		{
			foreach (AssemblySymbol referencedAssemblySymbol in modules[i].GetReferencedAssemblySymbols())
			{
				yield return Translate(referencedAssemblySymbol, diagnostics);
			}
		}
	}

	private void ValidateReferencedAssembly(AssemblySymbol assembly, AssemblyReference asmRef, DiagnosticBag diagnostics)
	{
		AssemblyIdentity identity = SourceModule.ContainingAssembly.Identity;
		AssemblyIdentity identity2 = asmRef.Identity;
		if (identity.IsStrongName && !identity2.IsStrongName && asmRef.Identity.ContentType != AssemblyContentType.WindowsRuntime)
		{
			diagnostics.Add(new CSDiagnosticInfo(ErrorCode.WRN_ReferencedAssemblyDoesNotHaveStrongName, assembly), NoLocation.Singleton);
		}
		if (OutputKind != OutputKind.NetModule && !string.IsNullOrEmpty(identity2.CultureName) && !string.Equals(identity2.CultureName, identity.CultureName, StringComparison.OrdinalIgnoreCase))
		{
			diagnostics.Add(new CSDiagnosticInfo(ErrorCode.WRN_RefCultureMismatch, assembly, identity2.CultureName), NoLocation.Singleton);
		}
		Machine machine = assembly.Machine;
		if ((object)assembly != assembly.CorLibrary && (machine != Machine.I386 || assembly.Bit32Required))
		{
			Machine machine2 = SourceModule.Machine;
			if ((machine2 != Machine.I386 || SourceModule.Bit32Required) && machine2 != machine)
			{
				diagnostics.Add(new CSDiagnosticInfo(ErrorCode.WRN_ConflictingMachineAssembly, assembly), NoLocation.Singleton);
			}
		}
		if (_embeddedTypesManagerOpt != null && _embeddedTypesManagerOpt.IsFrozen)
		{
			_embeddedTypesManagerOpt.ReportIndirectReferencesToLinkedAssemblies(assembly, diagnostics);
		}
	}

	internal sealed override IEnumerable<INestedTypeDefinition> GetSynthesizedNestedTypes(NamedTypeSymbol container)
	{
		return null;
	}

	public sealed override IEnumerable<(ITypeDefinition, ImmutableArray<DebugSourceDocument>)> GetTypeToDebugDocumentMap(EmitContext context)
	{
		ArrayBuilder<ITypeDefinition> typesToProcess = ArrayBuilder<ITypeDefinition>.GetInstance();
		ArrayBuilder<DebugSourceDocument> debugDocuments = ArrayBuilder<DebugSourceDocument>.GetInstance();
		PooledHashSet<DebugSourceDocument> methodDocumentList = PooledHashSet<DebugSourceDocument>.GetInstance();
		ArrayBuilder<NamespaceOrTypeSymbol> namespacesAndTopLevelTypesToProcess = ArrayBuilder<NamespaceOrTypeSymbol>.GetInstance();
		namespacesAndTopLevelTypesToProcess.Push(SourceModule.GlobalNamespace);
		while (namespacesAndTopLevelTypesToProcess.Count > 0)
		{
			NamespaceOrTypeSymbol namespaceOrTypeSymbol = namespacesAndTopLevelTypesToProcess.Pop();
			switch (namespaceOrTypeSymbol.Kind)
			{
			case SymbolKind.Namespace:
				if (!(GetSmallestSourceLocationOrNull(namespaceOrTypeSymbol) != null))
				{
					break;
				}
				foreach (Symbol member in namespaceOrTypeSymbol.GetMembers())
				{
					SymbolKind kind = member.Kind;
					if ((uint)(kind - 11) <= 1u)
					{
						namespacesAndTopLevelTypesToProcess.Push((NamespaceOrTypeSymbol)member);
						continue;
					}
					throw ExceptionUtilities.UnexpectedValue(member.Kind);
				}
				break;
			case SymbolKind.NamedType:
			{
				ITypeDefinition typeDefinition = (ITypeDefinition)namespaceOrTypeSymbol.GetCciAdapter();
				typesToProcess.Push(typeDefinition);
				GetDocumentsForMethodsAndNestedTypes(methodDocumentList, typesToProcess, context);
				foreach (Location location in namespaceOrTypeSymbol.Locations)
				{
					if (location.IsInSource)
					{
						FileLinePositionSpan lineSpan = location.GetLineSpan();
						DebugSourceDocument debugSourceDocument = DebugDocumentsBuilder.TryGetDebugDocument(lineSpan.Path, null);
						if (debugSourceDocument != null && !methodDocumentList.Contains(debugSourceDocument))
						{
							debugDocuments.Add(debugSourceDocument);
						}
					}
				}
				if (debugDocuments.Count > 0)
				{
					yield return (typeDefinition, debugDocuments.ToImmutable());
				}
				debugDocuments.Clear();
				methodDocumentList.Clear();
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(namespaceOrTypeSymbol.Kind);
			}
		}
		namespacesAndTopLevelTypesToProcess.Free();
		debugDocuments.Free();
		methodDocumentList.Free();
		typesToProcess.Free();
	}

	private static void GetDocumentsForMethodsAndNestedTypes(PooledHashSet<DebugSourceDocument> documentList, ArrayBuilder<ITypeDefinition> typesToProcess, EmitContext context)
	{
		while (typesToProcess.Count > 0)
		{
			ITypeDefinition typeDefinition = typesToProcess.Pop();
			foreach (IMethodDefinition method in typeDefinition.GetMethods(context))
			{
				IMethodBody body = method.GetBody(context);
				if (body != null)
				{
					foreach (SequencePoint sequencePoint in body.SequencePoints)
					{
						documentList.Add(sequencePoint.Document);
					}
				}
			}
			foreach (INestedTypeDefinition nestedType in typeDefinition.GetNestedTypes(context))
			{
				typesToProcess.Push(nestedType);
			}
		}
	}

	public sealed override MultiDictionary<DebugSourceDocument, DefinitionWithLocation> GetSymbolToLocationMap()
	{
		MultiDictionary<DebugSourceDocument, DefinitionWithLocation> result = new MultiDictionary<DebugSourceDocument, DefinitionWithLocation>();
		Stack<NamespaceOrTypeSymbol> stack = new Stack<NamespaceOrTypeSymbol>();
		stack.Push(SourceModule.GlobalNamespace);
		Location location = null;
		while (stack.Count > 0)
		{
			NamespaceOrTypeSymbol namespaceOrTypeSymbol = stack.Pop();
			switch (namespaceOrTypeSymbol.Kind)
			{
			case SymbolKind.Namespace:
				location = GetSmallestSourceLocationOrNull(namespaceOrTypeSymbol);
				if (!(location != null))
				{
					break;
				}
				foreach (Symbol member in namespaceOrTypeSymbol.GetMembers())
				{
					SymbolKind kind = member.Kind;
					if ((uint)(kind - 11) <= 1u)
					{
						stack.Push((NamespaceOrTypeSymbol)member);
						continue;
					}
					throw ExceptionUtilities.UnexpectedValue(member.Kind);
				}
				break;
			case SymbolKind.NamedType:
				location = GetSmallestSourceLocationOrNull(namespaceOrTypeSymbol);
				if (!(location != null))
				{
					break;
				}
				AddSymbolLocation(result, location, (IDefinition)namespaceOrTypeSymbol.GetCciAdapter());
				foreach (Symbol member2 in namespaceOrTypeSymbol.GetMembers())
				{
					switch (member2.Kind)
					{
					case SymbolKind.NamedType:
						if (!((NamedTypeSymbol)member2).IsExtension)
						{
							stack.Push((NamespaceOrTypeSymbol)member2);
						}
						break;
					case SymbolKind.Method:
						if (((MethodSymbol)member2).ShouldEmit())
						{
							AddSymbolLocation(result, member2);
						}
						break;
					case SymbolKind.Property:
						AddSymbolLocation(result, member2);
						break;
					case SymbolKind.Field:
						if (!(member2 is TupleErrorFieldSymbol))
						{
							AddSymbolLocation(result, member2);
						}
						break;
					case SymbolKind.Event:
					{
						AddSymbolLocation(result, member2);
						FieldSymbol associatedField = ((EventSymbol)member2).AssociatedField;
						if ((object)associatedField != null)
						{
							AddSymbolLocation(result, associatedField);
						}
						break;
					}
					default:
						throw ExceptionUtilities.UnexpectedValue(member2.Kind);
					}
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(namespaceOrTypeSymbol.Kind);
			}
		}
		return result;
	}

	private void AddSymbolLocation(MultiDictionary<DebugSourceDocument, DefinitionWithLocation> result, Symbol symbol)
	{
		Location smallestSourceLocationOrNull = GetSmallestSourceLocationOrNull(symbol);
		if (smallestSourceLocationOrNull != null)
		{
			AddSymbolLocation(result, smallestSourceLocationOrNull, (IDefinition)symbol.GetCciAdapter());
		}
	}

	private void AddSymbolLocation(MultiDictionary<DebugSourceDocument, DefinitionWithLocation> result, Location location, IDefinition definition)
	{
		FileLinePositionSpan lineSpan = location.GetLineSpan();
		DebugSourceDocument debugSourceDocument = DebugDocumentsBuilder.TryGetDebugDocument(lineSpan.Path, location.SourceTree.FilePath);
		if (debugSourceDocument != null)
		{
			result.Add(debugSourceDocument, new DefinitionWithLocation(definition, lineSpan.StartLinePosition.Line, lineSpan.StartLinePosition.Character, lineSpan.EndLinePosition.Line, lineSpan.EndLinePosition.Character));
		}
	}

	private Location GetSmallestSourceLocationOrNull(Symbol symbol)
	{
		CSharpCompilation declaringCompilation = symbol.DeclaringCompilation;
		Location location = null;
		foreach (Location location2 in symbol.Locations)
		{
			if (location2.IsInSource && (location == null || declaringCompilation.CompareSourceLocations(location, location2) > 0))
			{
				location = location2;
			}
		}
		return location;
	}

	internal virtual NamedTypeSymbol GetDynamicOperationContextType(NamedTypeSymbol contextType)
	{
		return contextType;
	}

	internal virtual VariableSlotAllocator TryCreateVariableSlotAllocator(MethodSymbol method, MethodSymbol topLevelMethod, DiagnosticBag diagnostics)
	{
		return null;
	}

	internal virtual MethodInstrumentation GetMethodBodyInstrumentations(MethodSymbol method)
	{
		return new MethodInstrumentation
		{
			Kinds = base.EmitOptions.InstrumentationKinds
		};
	}

	internal virtual int GetNextAnonymousTypeIndex()
	{
		return 0;
	}

	internal virtual int GetNextAnonymousDelegateIndex()
	{
		return 0;
	}

	internal virtual bool TryGetPreviousAnonymousTypeValue(AnonymousTypeManager.AnonymousTypeOrDelegateTemplateSymbol template, out AnonymousTypeValue typeValue)
	{
		typeValue = default(AnonymousTypeValue);
		return false;
	}

	internal virtual bool TryGetAnonymousDelegateValue(AnonymousTypeManager.AnonymousDelegateTemplateSymbol template, out SynthesizedDelegateValue delegateValue)
	{
		delegateValue = default(SynthesizedDelegateValue);
		return false;
	}

	public sealed override IEnumerable<INamespaceTypeDefinition> GetAnonymousTypeDefinitions(EmitContext context)
	{
		if (context.MetadataOnly)
		{
			return SpecializedCollections.EmptyEnumerable<INamespaceTypeDefinition>();
		}
		return Compilation.AnonymousTypeManager.GetAllCreatedTemplates();
	}

	public override IEnumerable<INamespaceTypeDefinition> GetTopLevelSourceTypeDefinitions(EmitContext context)
	{
		Stack<NamespaceSymbol> namespacesToProcess = new Stack<NamespaceSymbol>();
		namespacesToProcess.Push(SourceModule.GlobalNamespace);
		while (namespacesToProcess.Count > 0)
		{
			NamespaceSymbol namespaceSymbol = namespacesToProcess.Pop();
			foreach (Symbol member in namespaceSymbol.GetMembers())
			{
				if (member.Kind == SymbolKind.Namespace)
				{
					namespacesToProcess.Push((NamespaceSymbol)member);
				}
				else
				{
					yield return ((NamedTypeSymbol)member).GetCciAdapter();
				}
			}
		}
	}

	private static void GetExportedTypes(NamespaceOrTypeSymbol symbol, int parentIndex, ArrayBuilder<ExportedType> builder)
	{
		int parentIndex2;
		if (symbol.Kind == SymbolKind.NamedType)
		{
			if (symbol.DeclaredAccessibility != Accessibility.Public)
			{
				return;
			}
			parentIndex2 = builder.Count;
			builder.Add(new ExportedType((ITypeReference)symbol.GetCciAdapter(), parentIndex, isForwarder: false));
		}
		else
		{
			parentIndex2 = -1;
		}
		bool flag = false;
		foreach (Symbol item in symbol.IsNamespace ? symbol.GetMembers() : symbol.GetTypeMembers().Cast<NamedTypeSymbol, Symbol>())
		{
			if (item is NamespaceOrTypeSymbol namespaceOrTypeSymbol)
			{
				if (namespaceOrTypeSymbol is NamedTypeSymbol { IsExtension: not false })
				{
					flag = true;
				}
				else
				{
					GetExportedTypes(namespaceOrTypeSymbol, parentIndex2, builder);
				}
			}
		}
		if (flag)
		{
			ArrayBuilder<PENamedTypeSymbol> instance = ArrayBuilder<PENamedTypeSymbol>.GetInstance();
			GetNestedExtensionGroupingTypes((PENamedTypeSymbol)symbol, instance);
			foreach (PENamedTypeSymbol item2 in instance)
			{
				GetExportedTypes(item2, parentIndex2, builder);
			}
			instance.Free();
		}
	}

	private static ArrayBuilder<PENamedTypeSymbol> GetNestedExtensionGroupingTypes(PENamedTypeSymbol symbol, ArrayBuilder<PENamedTypeSymbol> groupingTypes)
	{
		PooledHashSet<PENamedTypeSymbol> instance = PooledHashSet<PENamedTypeSymbol>.GetInstance();
		foreach (NamedTypeSymbol typeMember in symbol.GetTypeMembers(""))
		{
			if (typeMember.IsExtension)
			{
				PENamedTypeSymbol extensionGroupingType = ((PENamedTypeSymbol)typeMember).ExtensionGroupingType;
				if (instance.Add(extensionGroupingType))
				{
					groupingTypes.Add(extensionGroupingType);
				}
			}
		}
		instance.Free();
		groupingTypes.Sort((PENamedTypeSymbol x, PENamedTypeSymbol y) => x.MetadataToken.CompareTo(y.MetadataToken));
		return groupingTypes;
	}

	public sealed override ImmutableArray<ExportedType> GetExportedTypes(EmitContext context)
	{
		if (_lazyExportedTypes.IsDefault && ImmutableInterlocked.InterlockedInitialize(ref _lazyExportedTypes, CalculateExportedTypes(context)) && _lazyExportedTypes.Length > 0)
		{
			ReportExportedTypeNameCollisions(_lazyExportedTypes, context.Diagnostics);
		}
		return _lazyExportedTypes;
	}

	private ImmutableArray<ExportedType> CalculateExportedTypes(EmitContext context)
	{
		SourceAssemblySymbol containingSourceAssembly = SourceModule.ContainingSourceAssembly;
		ArrayBuilder<ExportedType> instance = ArrayBuilder<ExportedType>.GetInstance();
		if (!OutputKind.IsNetModule())
		{
			ImmutableArray<ModuleSymbol> modules = containingSourceAssembly.Modules;
			for (int i = 1; i < modules.Length; i++)
			{
				GetExportedTypes(modules[i].GlobalNamespace, -1, instance);
			}
		}
		GetForwardedTypes(containingSourceAssembly, instance, context);
		return instance.ToImmutableAndFree();
	}

	internal static HashSet<NamedTypeSymbol> GetForwardedTypes(SourceAssemblySymbol sourceAssembly, ArrayBuilder<ExportedType>? builder, EmitContext? context)
	{
		HashSet<NamedTypeSymbol> hashSet = new HashSet<NamedTypeSymbol>();
		GetForwardedTypes(hashSet, sourceAssembly.GetSourceDecodedWellKnownAttributeData(), builder, context);
		if (!sourceAssembly.DeclaringCompilation.Options.OutputKind.IsNetModule())
		{
			GetForwardedTypes(hashSet, sourceAssembly.GetNetModuleDecodedWellKnownAttributeData(), builder, context);
		}
		return hashSet;
	}

	private void ReportExportedTypeNameCollisions(ImmutableArray<ExportedType> exportedTypes, DiagnosticBag diagnostics)
	{
		SourceAssemblySymbol containingSourceAssembly = SourceModule.ContainingSourceAssembly;
		Dictionary<string, NamedTypeSymbol> dictionary = new Dictionary<string, NamedTypeSymbol>(StringOrdinalComparer.Instance);
		foreach (ExportedType item in exportedTypes)
		{
			if (item.Type.AsNestedTypeReference != null)
			{
				continue;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)item.Type.GetInternalSymbol();
			string text = MetadataHelpers.BuildQualifiedName(((INamespaceTypeReference)namedTypeSymbol.GetCciAdapter()).NamespaceName, MetadataWriter.GetMetadataName(namedTypeSymbol.GetCciAdapter(), 0));
			NamedTypeSymbol value;
			if (ContainsTopLevelType(text))
			{
				if ((object)namedTypeSymbol.ContainingAssembly == containingSourceAssembly)
				{
					diagnostics.Add(ErrorCode.ERR_ExportedTypeConflictsWithDeclaration, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingModule);
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_ForwardedTypeConflictsWithDeclaration, NoLocation.Singleton, namedTypeSymbol);
				}
			}
			else if (dictionary.TryGetValue(text, out value))
			{
				if ((object)namedTypeSymbol.ContainingAssembly == containingSourceAssembly)
				{
					diagnostics.Add(ErrorCode.ERR_ExportedTypesConflict, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingModule, value, value.ContainingModule);
				}
				else if ((object)value.ContainingAssembly == containingSourceAssembly)
				{
					diagnostics.Add(ErrorCode.ERR_ForwardedTypeConflictsWithExportedType, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingAssembly, value, value.ContainingModule);
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_ForwardedTypesConflict, NoLocation.Singleton, namedTypeSymbol, namedTypeSymbol.ContainingAssembly, value, value.ContainingAssembly);
				}
			}
			else
			{
				dictionary.Add(text, namedTypeSymbol);
			}
		}
	}

	private static void GetForwardedTypes(HashSet<NamedTypeSymbol> seenTopLevelTypes, CommonAssemblyWellKnownAttributeData<NamedTypeSymbol> wellKnownAttributeData, ArrayBuilder<ExportedType>? builder, EmitContext? contextOpt)
	{
		if (wellKnownAttributeData == null || !(wellKnownAttributeData.ForwardedTypes?.Count > 0))
		{
			return;
		}
		ArrayBuilder<(NamedTypeSymbol, int)> instance = ArrayBuilder<(NamedTypeSymbol, int)>.GetInstance();
		IEnumerable<NamedTypeSymbol> enumerable = wellKnownAttributeData.ForwardedTypes;
		if (builder != null)
		{
			enumerable = enumerable.OrderBy((NamedTypeSymbol t) => t.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat));
		}
		foreach (NamedTypeSymbol item in enumerable)
		{
			NamedTypeSymbol originalDefinition = item.OriginalDefinition;
			if (seenTopLevelTypes.Add(originalDefinition) && builder != null)
			{
				EmitContext valueOrDefault = contextOpt.GetValueOrDefault();
				instance.Push((originalDefinition, -1));
				while (instance.Count > 0)
				{
					processTopItemFromStack(instance, valueOrDefault, builder);
				}
			}
		}
		instance.Free();
		static void processTopItemFromStack(ArrayBuilder<(NamedTypeSymbol type, int parentIndex)> stack, EmitContext context, ArrayBuilder<ExportedType> arrayBuilder)
		{
			var (namedTypeSymbol, parentIndex) = stack.Pop();
			if (namedTypeSymbol.DeclaredAccessibility != Accessibility.Private)
			{
				int count = arrayBuilder.Count;
				arrayBuilder.Add(new ExportedType(namedTypeSymbol.GetCciAdapter(), parentIndex, isForwarder: true));
				ImmutableArray<NamedTypeSymbol> typeMembers = namedTypeSymbol.GetTypeMembers();
				if (typeMembers.Any((NamedTypeSymbol n) => n.IsExtension))
				{
					if (!(namedTypeSymbol is PENamedTypeSymbol symbol))
					{
						if (!(namedTypeSymbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol))
						{
							if (!(namedTypeSymbol is RetargetingNamedTypeSymbol retargetingNamedTypeSymbol))
							{
								throw ExceptionUtilities.UnexpectedValue(namedTypeSymbol);
							}
							pushAndProcessNestedTypes(stack, context, count, typeMembers, arrayBuilder);
							foreach (var item2 in retargetingNamedTypeSymbol.GetExtensionGroupingAndMarkerTypesForTypeForwarding(context))
							{
								int count2 = arrayBuilder.Count;
								arrayBuilder.Add(new ExportedType(item2.GroupingType, count, isForwarder: true));
								foreach (INestedTypeReference item3 in item2.MarkerTypes)
								{
									arrayBuilder.Add(new ExportedType(item3, count2, isForwarder: true));
								}
							}
						}
						else
						{
							pushAndProcessNestedTypes(stack, context, count, typeMembers, arrayBuilder);
							foreach (INestedTypeDefinition groupingType in sourceMemberContainerTypeSymbol.GetExtensionGroupingInfo().GetGroupingTypes())
							{
								int count3 = arrayBuilder.Count;
								arrayBuilder.Add(new ExportedType(groupingType, count, isForwarder: true));
								foreach (INestedTypeDefinition nestedType in groupingType.GetNestedTypes(context))
								{
									arrayBuilder.Add(new ExportedType(nestedType, count3, isForwarder: true));
								}
							}
						}
					}
					else
					{
						ArrayBuilder<PENamedTypeSymbol> instance2 = ArrayBuilder<PENamedTypeSymbol>.GetInstance();
						GetNestedExtensionGroupingTypes(symbol, instance2);
						for (int num = instance2.Count - 1; num >= 0; num--)
						{
							stack.Push((instance2[num], count));
						}
						instance2.Free();
						pushNestedTypes(stack, count, typeMembers);
					}
				}
				else
				{
					pushNestedTypes(stack, count, typeMembers);
				}
			}
		}
		static void pushAndProcessNestedTypes(ArrayBuilder<(NamedTypeSymbol type, int parentIndex)> stack, EmitContext context, int index, ImmutableArray<NamedTypeSymbol> nested, ArrayBuilder<ExportedType> builder2)
		{
			int count = stack.Count;
			pushNestedTypes(stack, index, nested);
			while (stack.Count > count)
			{
				processTopItemFromStack(stack, context, builder2);
			}
		}
		static void pushNestedTypes(ArrayBuilder<(NamedTypeSymbol type, int parentIndex)> stack, int index, ImmutableArray<NamedTypeSymbol> nested)
		{
			for (int num = nested.Length - 1; num >= 0; num--)
			{
				if (!nested[num].IsExtension)
				{
					stack.Push((nested[num], index));
				}
			}
		}
	}

	internal IEnumerable<AssemblySymbol> GetReferencedAssembliesUsedSoFar()
	{
		foreach (AssemblySymbol referencedAssemblySymbol in SourceModule.GetReferencedAssemblySymbols())
		{
			if (!referencedAssemblySymbol.IsLinked && !referencedAssemblySymbol.IsMissing && AssemblyOrModuleSymbolToModuleRefMap.ContainsKey(referencedAssemblySymbol))
			{
				yield return referencedAssemblySymbol;
			}
		}
	}

	private NamedTypeSymbol GetUntranslatedSpecialType(SpecialType specialType, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol specialType2 = SourceModule.ContainingAssembly.GetSpecialType(specialType);
		DiagnosticInfo diagnosticInfo = specialType2.GetUseSiteInfo().DiagnosticInfo;
		if (diagnosticInfo != null)
		{
			Symbol.ReportUseSiteDiagnostic(diagnosticInfo, diagnostics, (syntaxNodeOpt != null) ? syntaxNodeOpt.Location : NoLocation.Singleton);
		}
		return specialType2;
	}

	internal sealed override INamedTypeReference GetSpecialType(SpecialType specialType, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return Translate(GetUntranslatedSpecialType(specialType, syntaxNodeOpt, diagnostics), syntaxNodeOpt, diagnostics, fromImplements: false, needDeclaration: true);
	}

	public sealed override IMethodReference GetInitArrayHelper()
	{
		return ((MethodSymbol)Compilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_RuntimeHelpers__InitializeArrayArrayRuntimeFieldHandle))?.GetCciAdapter();
	}

	public sealed override bool IsPlatformType(ITypeReference typeRef, PlatformType platformType)
	{
		if (typeRef.GetInternalSymbol() is NamedTypeSymbol namedTypeSymbol)
		{
			if (platformType == PlatformType.SystemType)
			{
				return (object)namedTypeSymbol == Compilation.GetWellKnownType(WellKnownType.System_Type);
			}
			return (int)namedTypeSymbol.SpecialType == (sbyte)platformType;
		}
		return false;
	}

	protected sealed override IAssemblyReference GetCorLibraryReferenceToEmit(EmitContext context)
	{
		AssemblySymbol corLibrary = CorLibrary;
		if (!corLibrary.IsMissing && !corLibrary.IsLinked && (object)corLibrary != SourceModule.ContainingAssembly)
		{
			return Translate(corLibrary, context.Diagnostics);
		}
		return null;
	}

	internal sealed override IAssemblyReference Translate(AssemblySymbol assembly, DiagnosticBag diagnostics)
	{
		if ((object)SourceModule.ContainingAssembly == assembly)
		{
			return (IAssemblyReference)this;
		}
		if (AssemblyOrModuleSymbolToModuleRefMap.TryGetValue(assembly, out var value))
		{
			return (IAssemblyReference)value;
		}
		AssemblyReference assemblyReference = new AssemblyReference(assembly);
		AssemblyReference assemblyReference2 = (AssemblyReference)AssemblyOrModuleSymbolToModuleRefMap.GetOrAdd(assembly, assemblyReference);
		if (assemblyReference2 == assemblyReference)
		{
			ValidateReferencedAssembly(assembly, assemblyReference2, diagnostics);
		}
		AssemblyOrModuleSymbolToModuleRefMap.TryAdd(assembly.Modules[0], assemblyReference2);
		return assemblyReference2;
	}

	internal IModuleReference Translate(ModuleSymbol module, DiagnosticBag diagnostics)
	{
		if ((object)SourceModule == module)
		{
			return this;
		}
		if ((object)module == null)
		{
			return null;
		}
		if (AssemblyOrModuleSymbolToModuleRefMap.TryGetValue(module, out var value))
		{
			return value;
		}
		value = TranslateModule(module, diagnostics);
		return AssemblyOrModuleSymbolToModuleRefMap.GetOrAdd(module, value);
	}

	protected virtual IModuleReference TranslateModule(ModuleSymbol module, DiagnosticBag diagnostics)
	{
		AssemblySymbol containingAssembly = module.ContainingAssembly;
		if ((object)containingAssembly != null && (object)containingAssembly.Modules[0] == module)
		{
			IModuleReference moduleReference = new AssemblyReference(containingAssembly);
			IModuleReference orAdd = AssemblyOrModuleSymbolToModuleRefMap.GetOrAdd(containingAssembly, moduleReference);
			if (orAdd == moduleReference)
			{
				ValidateReferencedAssembly(containingAssembly, (AssemblyReference)moduleReference, diagnostics);
			}
			else
			{
				moduleReference = orAdd;
			}
			return moduleReference;
		}
		return new ModuleReference(this, module);
	}

	internal INamedTypeReference Translate(NamedTypeSymbol namedTypeSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool fromImplements = false, bool needDeclaration = false)
	{
		if (namedTypeSymbol.IsAnonymousType)
		{
			namedTypeSymbol = AnonymousTypeManager.TranslateAnonymousTypeSymbol(namedTypeSymbol);
		}
		else if (namedTypeSymbol.IsTupleType)
		{
			CheckTupleUnderlyingType(namedTypeSymbol, syntaxNodeOpt, diagnostics);
		}
		if (namedTypeSymbol.OriginalDefinition.Kind == SymbolKind.ErrorType)
		{
			ErrorTypeSymbol errorTypeSymbol = (ErrorTypeSymbol)namedTypeSymbol.OriginalDefinition;
			DiagnosticInfo diagnosticInfo = errorTypeSymbol.GetUseSiteInfo().DiagnosticInfo ?? errorTypeSymbol.ErrorInfo;
			if (diagnosticInfo == null && namedTypeSymbol.Kind == SymbolKind.ErrorType)
			{
				errorTypeSymbol = (ErrorTypeSymbol)namedTypeSymbol;
				diagnosticInfo = errorTypeSymbol.GetUseSiteInfo().DiagnosticInfo ?? errorTypeSymbol.ErrorInfo;
			}
			if (_reportedErrorTypesMap.Add(errorTypeSymbol))
			{
				diagnostics.Add(new CSDiagnostic(diagnosticInfo ?? new CSDiagnosticInfo(ErrorCode.ERR_BogusType, string.Empty), (syntaxNodeOpt == null) ? NoLocation.Singleton : syntaxNodeOpt.Location));
			}
			return ErrorType.Singleton;
		}
		if (!namedTypeSymbol.IsDefinition)
		{
			if (!namedTypeSymbol.IsUnboundGenericType)
			{
				return (INamedTypeReference)GetCciAdapter(namedTypeSymbol);
			}
			namedTypeSymbol = namedTypeSymbol.OriginalDefinition;
		}
		else if (!needDeclaration)
		{
			NamedTypeSymbol containingType = namedTypeSymbol.ContainingType;
			object value;
			if (namedTypeSymbol.Arity > 0)
			{
				if (_genericInstanceMap.TryGetValue(namedTypeSymbol, out value))
				{
					return (INamedTypeReference)value;
				}
				INamedTypeReference value2 = (((object)containingType == null) ? new GenericNamespaceTypeInstanceReference(namedTypeSymbol) : ((!IsGenericType(containingType)) ? ((INamedTypeReference)new GenericNestedTypeInstanceReference(namedTypeSymbol)) : ((INamedTypeReference)new SpecializedGenericNestedTypeInstanceReference(namedTypeSymbol))));
				return (INamedTypeReference)_genericInstanceMap.GetOrAdd(namedTypeSymbol, value2);
			}
			if (IsGenericType(containingType))
			{
				if (_genericInstanceMap.TryGetValue(namedTypeSymbol, out value))
				{
					return (INamedTypeReference)value;
				}
				INamedTypeReference value2 = new SpecializedNestedTypeReference(namedTypeSymbol);
				return (INamedTypeReference)_genericInstanceMap.GetOrAdd(namedTypeSymbol, value2);
			}
			NamedTypeSymbol nativeIntegerUnderlyingType = namedTypeSymbol.NativeIntegerUnderlyingType;
			if ((object)nativeIntegerUnderlyingType != null)
			{
				namedTypeSymbol = nativeIntegerUnderlyingType;
			}
		}
		return _embeddedTypesManagerOpt?.EmbedTypeIfNeedTo(namedTypeSymbol, fromImplements, syntaxNodeOpt, diagnostics) ?? namedTypeSymbol.GetCciAdapter();
	}

	private object GetCciAdapter(Symbol symbol)
	{
		return _genericInstanceMap.GetOrAdd(symbol, (Symbol s) => s.GetCciAdapter());
	}

	private void CheckTupleUnderlyingType(NamedTypeSymbol namedTypeSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = namedTypeSymbol.BaseTypeNoUseSiteDiagnostics;
		if (((object)baseTypeNoUseSiteDiagnostics != null && baseTypeNoUseSiteDiagnostics.SpecialType == SpecialType.System_ValueType) || !_reportedErrorTypesMap.Add(namedTypeSymbol))
		{
			return;
		}
		Location location = ((syntaxNodeOpt == null) ? NoLocation.Singleton : syntaxNodeOpt.Location);
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			DiagnosticInfo diagnosticInfo = baseTypeNoUseSiteDiagnostics.GetUseSiteInfo().DiagnosticInfo;
			if (diagnosticInfo != null && diagnosticInfo.Severity == DiagnosticSeverity.Error)
			{
				diagnostics.Add(diagnosticInfo, location);
				return;
			}
		}
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_PredefinedValueTupleTypeMustBeStruct, namedTypeSymbol.MetadataName), location));
	}

	public static bool IsGenericType(NamedTypeSymbol toCheck)
	{
		while ((object)toCheck != null)
		{
			if (toCheck.Arity > 0)
			{
				return true;
			}
			toCheck = toCheck.ContainingType;
		}
		return false;
	}

	internal static IGenericParameterReference Translate(TypeParameterSymbol param)
	{
		if (!param.IsDefinition)
		{
			throw new InvalidOperationException(string.Format(CSharpResources.GenericParameterDefinition, param.Name));
		}
		return param.GetCciAdapter();
	}

	internal sealed override ITypeReference Translate(TypeSymbol typeSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		switch (typeSymbol.Kind)
		{
		case SymbolKind.DynamicType:
			return Translate(syntaxNodeOpt, diagnostics);
		case SymbolKind.ArrayType:
			return Translate((ArrayTypeSymbol)typeSymbol);
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return Translate((NamedTypeSymbol)typeSymbol, syntaxNodeOpt, diagnostics);
		case SymbolKind.PointerType:
			return Translate((PointerTypeSymbol)typeSymbol);
		case SymbolKind.TypeParameter:
			return Translate((TypeParameterSymbol)typeSymbol);
		case SymbolKind.FunctionPointerType:
			return Translate((FunctionPointerTypeSymbol)typeSymbol);
		default:
			throw ExceptionUtilities.UnexpectedValue(typeSymbol.Kind);
		}
	}

	internal IFieldReference Translate(FieldSymbol fieldSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool needDeclaration = false)
	{
		if (!fieldSymbol.IsDefinition)
		{
			return (IFieldReference)GetCciAdapter(fieldSymbol);
		}
		if (needDeclaration || !IsGenericType(fieldSymbol.ContainingType))
		{
			return _embeddedTypesManagerOpt?.EmbedFieldIfNeedTo(fieldSymbol.GetCciAdapter(), syntaxNodeOpt, diagnostics) ?? fieldSymbol.GetCciAdapter();
		}
		if (_genericInstanceMap.TryGetValue(fieldSymbol, out var value))
		{
			return (IFieldReference)value;
		}
		IFieldReference value2 = new SpecializedFieldReference(fieldSymbol);
		return (IFieldReference)_genericInstanceMap.GetOrAdd(fieldSymbol, value2);
	}

	internal sealed override IMethodReference Translate(MethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration)
	{
		return Translate(symbol, null, diagnostics, null, needDeclaration);
	}

	internal IMethodReference Translate(MethodSymbol methodSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, BoundArgListOperator optArgList = null, bool needDeclaration = false)
	{
		IMethodReference methodReference = Translate(methodSymbol, syntaxNodeOpt, diagnostics, needDeclaration);
		if (optArgList != null && optArgList.Arguments.Length > 0)
		{
			IParameterTypeInformation[] array = new IParameterTypeInformation[optArgList.Arguments.Length];
			int num = methodSymbol.ParameterCount;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ArgListParameterTypeInformation(num, !optArgList.ArgumentRefKindsOpt.IsDefaultOrEmpty && optArgList.ArgumentRefKindsOpt[i] != RefKind.None, Translate(optArgList.Arguments[i].Type, syntaxNodeOpt, diagnostics));
				num++;
			}
			return new ExpandedVarargsMethodReference(methodReference, array.AsImmutableOrNull());
		}
		return methodReference;
	}

	private IMethodReference Translate(MethodSymbol methodSymbol, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool needDeclaration)
	{
		NamedTypeSymbol containingType = methodSymbol.ContainingType;
		if ((object)containingType != null && containingType.IsAnonymousType)
		{
			methodSymbol = AnonymousTypeManager.TranslateAnonymousTypeMethodSymbol(methodSymbol);
		}
		if (!methodSymbol.IsDefinition)
		{
			return (IMethodReference)GetCciAdapter(methodSymbol);
		}
		if (!needDeclaration)
		{
			bool isGenericMethod = methodSymbol.IsGenericMethod;
			bool flag = IsGenericType(containingType);
			if (isGenericMethod | flag)
			{
				if (_genericInstanceMap.TryGetValue(methodSymbol, out var value))
				{
					return (IMethodReference)value;
				}
				IMethodReference value2 = ((!isGenericMethod) ? new SpecializedMethodReference(methodSymbol) : ((!flag) ? ((IMethodReference)new GenericMethodInstanceReference(methodSymbol)) : ((IMethodReference)new SpecializedGenericMethodInstanceReference(methodSymbol))));
				return (IMethodReference)_genericInstanceMap.GetOrAdd(methodSymbol, value2);
			}
			if (methodSymbol is NativeIntegerMethodSymbol { UnderlyingMethod: { } underlyingMethod })
			{
				methodSymbol = underlyingMethod;
			}
		}
		if (_embeddedTypesManagerOpt != null)
		{
			return _embeddedTypesManagerOpt.EmbedMethodIfNeedTo(methodSymbol.GetCciAdapter(), syntaxNodeOpt, diagnostics);
		}
		return methodSymbol.GetCciAdapter();
	}

	internal IMethodReference TranslateOverriddenMethodReference(MethodSymbol methodSymbol, CSharpSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		if (IsGenericType(methodSymbol.ContainingType))
		{
			if (methodSymbol.IsDefinition)
			{
				if (_genericInstanceMap.TryGetValue(methodSymbol, out var value))
				{
					return (IMethodReference)value;
				}
				IMethodReference value2 = new SpecializedMethodReference(methodSymbol);
				return (IMethodReference)_genericInstanceMap.GetOrAdd(methodSymbol, value2);
			}
			return new SpecializedMethodReference(methodSymbol);
		}
		if (_embeddedTypesManagerOpt != null)
		{
			return _embeddedTypesManagerOpt.EmbedMethodIfNeedTo(methodSymbol.GetCciAdapter(), syntaxNodeOpt, diagnostics);
		}
		return methodSymbol.GetCciAdapter();
	}

	internal ImmutableArray<IParameterTypeInformation> Translate(ImmutableArray<ParameterSymbol> @params)
	{
		if (!@params.Any() || !MustBeWrapped(@params.First()))
		{
			return StaticCast<IParameterTypeInformation>.From(@params);
		}
		return TranslateAll(@params);
	}

	private static bool MustBeWrapped(ParameterSymbol param)
	{
		if (param.IsDefinition && ContainerIsGeneric(param.ContainingSymbol))
		{
			return true;
		}
		return false;
	}

	private ImmutableArray<IParameterTypeInformation> TranslateAll(ImmutableArray<ParameterSymbol> @params)
	{
		ArrayBuilder<IParameterTypeInformation> instance = ArrayBuilder<IParameterTypeInformation>.GetInstance();
		foreach (ParameterSymbol item in @params)
		{
			instance.Add(CreateParameterTypeInformationWrapper(item));
		}
		return instance.ToImmutableAndFree();
	}

	private IParameterTypeInformation CreateParameterTypeInformationWrapper(ParameterSymbol param)
	{
		if (_genericInstanceMap.TryGetValue(param, out var value))
		{
			return (IParameterTypeInformation)value;
		}
		IParameterTypeInformation value2 = new ParameterTypeInformation(param);
		return (IParameterTypeInformation)_genericInstanceMap.GetOrAdd(param, value2);
	}

	private static bool ContainerIsGeneric(Symbol container)
	{
		if (container.Kind != SymbolKind.Method || !((MethodSymbol)container).IsGenericMethod)
		{
			return IsGenericType(container.ContainingType);
		}
		return true;
	}

	internal ITypeReference Translate(SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return GetSpecialType(SpecialType.System_Object, syntaxNodeOpt, diagnostics);
	}

	internal IArrayTypeReference Translate(ArrayTypeSymbol symbol)
	{
		return (IArrayTypeReference)GetCciAdapter(symbol);
	}

	internal IPointerTypeReference Translate(PointerTypeSymbol symbol)
	{
		return (IPointerTypeReference)GetCciAdapter(symbol);
	}

	internal IFunctionPointerTypeReference Translate(FunctionPointerTypeSymbol symbol)
	{
		return (IFunctionPointerTypeReference)GetCciAdapter(symbol);
	}

	public NamedTypeSymbol SetFixedImplementationType(SourceMemberFieldSymbol field)
	{
		if (_fixedImplementationTypes == null)
		{
			Interlocked.CompareExchange(ref _fixedImplementationTypes, new Dictionary<FieldSymbol, NamedTypeSymbol>(), null);
		}
		lock (_fixedImplementationTypes)
		{
			if (_fixedImplementationTypes.TryGetValue(field, out var value))
			{
				return value;
			}
			value = new FixedFieldImplementationType(field);
			_fixedImplementationTypes.Add(field, value);
			AddSynthesizedDefinition(value.ContainingType, value.GetCciAdapter());
			return value;
		}
	}

	protected override IMethodDefinition CreatePrivateImplementationDetailsStaticConstructor(SyntaxNode syntaxOpt, DiagnosticBag diagnostics)
	{
		return new SynthesizedPrivateImplementationDetailsStaticConstructor(GetPrivateImplClass(syntaxOpt, diagnostics), GetUntranslatedSpecialType(SpecialType.System_Void, syntaxOpt, diagnostics)).GetCciAdapter();
	}

	internal abstract SynthesizedAttributeData SynthesizeEmbeddedAttribute();

	internal SynthesizedAttributeData SynthesizeIsReadOnlyAttribute(Symbol symbol)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeIsReadOnlyAttribute();
	}

	internal SynthesizedAttributeData SynthesizeRequiresLocationAttribute(ParameterSymbol symbol)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeRequiresLocationAttribute();
	}

	internal SynthesizedAttributeData SynthesizeParamCollectionAttribute(ParameterSymbol symbol)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeParamCollectionAttribute();
	}

	internal SynthesizedAttributeData SynthesizeExtensionMarkerAttribute(Symbol symbol, string markerName)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeExtensionMarkerAttribute(markerName);
	}

	internal SynthesizedAttributeData SynthesizeIsUnmanagedAttribute(Symbol symbol)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeIsUnmanagedAttribute();
	}

	internal SynthesizedAttributeData SynthesizeIsByRefLikeAttribute(Symbol symbol)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return TrySynthesizeIsByRefLikeAttribute();
	}

	internal SynthesizedAttributeData SynthesizeNullableAttributeIfNecessary(Symbol symbol, byte? nullableContextValue, TypeWithAnnotations type)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance();
		type.AddNullableTransforms(instance);
		SynthesizedAttributeData result;
		if (!instance.Any())
		{
			result = null;
		}
		else
		{
			byte? commonValue = MostCommonNullableValueBuilder.GetCommonValue(instance);
			if (commonValue.HasValue)
			{
				result = SynthesizeNullableAttributeIfNecessary(nullableContextValue, commonValue.GetValueOrDefault());
			}
			else
			{
				NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Byte);
				ArrayTypeSymbol type2 = ArrayTypeSymbol.CreateSZArray(specialType.ContainingAssembly, TypeWithAnnotations.Create(specialType));
				ImmutableArray<TypedConstant> array = instance.SelectAsArray((byte flag, NamedTypeSymbol byteType) => new TypedConstant(byteType, TypedConstantKind.Primitive, flag), specialType);
				result = SynthesizeNullableAttribute(WellKnownMember.System_Runtime_CompilerServices_NullableAttribute__ctorTransformFlags, ImmutableArray.Create(new TypedConstant(type2, array)));
			}
		}
		instance.Free();
		return result;
	}

	internal SynthesizedAttributeData SynthesizeNullableAttributeIfNecessary(byte? nullableContextValue, byte nullableValue)
	{
		if (nullableValue == nullableContextValue || (!nullableContextValue.HasValue && nullableValue == 0))
		{
			return null;
		}
		NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Byte);
		return SynthesizeNullableAttribute(WellKnownMember.System_Runtime_CompilerServices_NullableAttribute__ctorByte, ImmutableArray.Create(new TypedConstant(specialType, TypedConstantKind.Primitive, nullableValue)));
	}

	internal virtual SynthesizedAttributeData SynthesizeNullableAttribute(WellKnownMember member, ImmutableArray<TypedConstant> arguments)
	{
		return Compilation.TrySynthesizeAttribute(member, arguments, default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), isOptionalUse: true);
	}

	internal SynthesizedAttributeData SynthesizeNullableContextAttribute(Symbol symbol, byte value)
	{
		ModuleSymbol sourceModule = Compilation.SourceModule;
		if ((object)sourceModule != symbol && (object)sourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return SynthesizeNullableContextAttribute(ImmutableArray.Create(new TypedConstant(Compilation.GetSpecialType(SpecialType.System_Byte), TypedConstantKind.Primitive, value)));
	}

	internal virtual SynthesizedAttributeData SynthesizeNullableContextAttribute(ImmutableArray<TypedConstant> arguments)
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_NullableContextAttribute__ctor, arguments, default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), isOptionalUse: true);
	}

	internal SynthesizedAttributeData SynthesizePreserveBaseOverridesAttribute()
	{
		return Compilation.TrySynthesizeAttribute(SpecialMember.System_Runtime_CompilerServices_PreserveBaseOverridesAttribute__ctor, isOptionalUse: true);
	}

	internal SynthesizedAttributeData SynthesizeNativeIntegerAttribute(Symbol symbol, TypeSymbol type)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance();
		CSharpCompilation.NativeIntegerTransformsEncoder.Encode(instance, type);
		SynthesizedAttributeData result;
		if (instance.Count == 1 && instance[0])
		{
			result = SynthesizeNativeIntegerAttribute(WellKnownMember.System_Runtime_CompilerServices_NativeIntegerAttribute__ctor, ImmutableArray<TypedConstant>.Empty);
		}
		else
		{
			NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Boolean);
			ImmutableArray<TypedConstant> array = instance.SelectAsArray((bool flag, NamedTypeSymbol constantType) => new TypedConstant(constantType, TypedConstantKind.Primitive, flag), specialType);
			ImmutableArray<TypedConstant> arguments = ImmutableArray.Create(new TypedConstant(ArrayTypeSymbol.CreateSZArray(specialType.ContainingAssembly, TypeWithAnnotations.Create(specialType)), array));
			result = SynthesizeNativeIntegerAttribute(WellKnownMember.System_Runtime_CompilerServices_NativeIntegerAttribute__ctorTransformFlags, arguments);
		}
		instance.Free();
		return result;
	}

	internal virtual SynthesizedAttributeData SynthesizeNativeIntegerAttribute(WellKnownMember member, ImmutableArray<TypedConstant> arguments)
	{
		return Compilation.TrySynthesizeAttribute(member, arguments, default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), isOptionalUse: true);
	}

	internal SynthesizedAttributeData SynthesizeScopedRefAttribute(ParameterSymbol symbol, ScopedKind scope)
	{
		if ((object)Compilation.SourceModule != symbol.ContainingModule)
		{
			return null;
		}
		return SynthesizeScopedRefAttribute(WellKnownMember.System_Runtime_CompilerServices_ScopedRefAttribute__ctor);
	}

	internal virtual SynthesizedAttributeData SynthesizeScopedRefAttribute(WellKnownMember member)
	{
		return Compilation.TrySynthesizeAttribute(member, default(ImmutableArray<TypedConstant>), default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), isOptionalUse: true);
	}

	internal virtual SynthesizedAttributeData SynthesizeRefSafetyRulesAttribute(ImmutableArray<TypedConstant> arguments)
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_RefSafetyRulesAttribute__ctor, arguments, default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), isOptionalUse: true);
	}

	internal bool ShouldEmitNullablePublicOnlyAttribute()
	{
		if (Compilation.GetUsesNullableAttributes())
		{
			return Compilation.EmitNullablePublicOnly;
		}
		return false;
	}

	internal virtual SynthesizedAttributeData SynthesizeNullablePublicOnlyAttribute(ImmutableArray<TypedConstant> arguments)
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_NullablePublicOnlyAttribute__ctor, arguments);
	}

	protected virtual SynthesizedAttributeData TrySynthesizeIsReadOnlyAttribute()
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_IsReadOnlyAttribute__ctor);
	}

	protected virtual SynthesizedAttributeData TrySynthesizeRequiresLocationAttribute()
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_RequiresLocationAttribute__ctor);
	}

	protected virtual SynthesizedAttributeData TrySynthesizeParamCollectionAttribute()
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_ParamCollectionAttribute__ctor);
	}

	protected virtual SynthesizedAttributeData TrySynthesizeExtensionMarkerAttribute(string markerName)
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_ExtensionMarkerAttribute__ctor, ImmutableCollectionsMarshal.AsImmutableArray(new TypedConstant[1]
		{
			new TypedConstant(Compilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, markerName)
		}));
	}

	internal virtual SynthesizedEmbeddedAttributeSymbol TryGetSynthesizedIsUnmanagedAttribute()
	{
		return null;
	}

	protected virtual SynthesizedAttributeData TrySynthesizeIsUnmanagedAttribute()
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_IsUnmanagedAttribute__ctor);
	}

	protected virtual SynthesizedAttributeData TrySynthesizeIsByRefLikeAttribute()
	{
		return Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_IsByRefLikeAttribute__ctor);
	}

	private void EnsureEmbeddableAttributeExists(EmbeddableAttributes attribute, BindingDiagnosticBag diagnosticsOpt = null, Location locationOpt = null)
	{
		if ((GetNeedsGeneratedAttributesInternal() & attribute) == 0 && Compilation.CheckIfAttributeShouldBeEmbedded(attribute, diagnosticsOpt, locationOpt))
		{
			SetNeedsGeneratedAttributes(attribute);
		}
	}

	internal void EnsureIsReadOnlyAttributeExists()
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.IsReadOnlyAttribute);
	}

	internal void EnsureRequiresLocationAttributeExists()
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.RequiresLocationAttribute);
	}

	internal void EnsureParamCollectionAttributeExists(BindingDiagnosticBag diagnostics, Location location)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.ParamCollectionAttribute, diagnostics, location);
	}

	internal void EnsureIsUnmanagedAttributeExists()
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.IsUnmanagedAttribute);
	}

	internal void EnsureNullableAttributeExists()
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.NullableAttribute);
	}

	internal void EnsureNullableContextAttributeExists()
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.NullableContextAttribute);
	}

	internal void EnsureNativeIntegerAttributeExists()
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.NativeIntegerAttribute);
	}

	internal void EnsureScopedRefAttributeExists()
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.ScopedRefAttribute);
	}

	internal MethodSymbol EnsureThrowSwitchExpressionExceptionExists(SyntaxNode syntaxNode, SyntheticBoundNodeFactory factory, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "ThrowSwitchExpressionException", delegate(SynthesizedPrivateImplementationDetailsType privateImplClass, SyntheticBoundNodeFactory syntheticBoundNodeFactory)
		{
			TypeSymbol returnType = syntheticBoundNodeFactory.SpecialType(SpecialType.System_Void);
			TypeSymbol paramType = syntheticBoundNodeFactory.SpecialType(SpecialType.System_Object);
			return new SynthesizedThrowSwitchExpressionExceptionMethod(privateImplClass, returnType, paramType);
		}, factory, diagnostics);
	}

	private MethodSymbol EnsurePrivateImplClassMethodExists<TArg>(SyntaxNode syntaxNode, string methodName, Func<SynthesizedPrivateImplementationDetailsType, TArg, MethodSymbol> createMethodSymbol, TArg arg, DiagnosticBag diagnostics)
	{
		SynthesizedPrivateImplementationDetailsType privateImplClass = GetPrivateImplClass(syntaxNode, diagnostics);
		IMethodDefinition method = privateImplClass.PrivateImplementationDetails.GetMethod(methodName);
		if (method != null)
		{
			return (MethodSymbol)method.GetInternalSymbol();
		}
		MethodSymbol methodSymbol = createMethodSymbol(privateImplClass, arg);
		privateImplClass.PrivateImplementationDetails.TryAddSynthesizedMethod(methodSymbol.GetCciAdapter());
		return (MethodSymbol)privateImplClass.PrivateImplementationDetails.GetMethod(methodName).GetInternalSymbol();
	}

	internal new SynthesizedPrivateImplementationDetailsType GetPrivateImplClass(SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		if ((object)_lazyPrivateImplementationDetailsClass == null)
		{
			Interlocked.CompareExchange(ref _lazyPrivateImplementationDetailsClass, new SynthesizedPrivateImplementationDetailsType(base.GetPrivateImplClass(syntaxNodeOpt, diagnostics), SourceModule.GlobalNamespace, Compilation.ObjectType), null);
		}
		return _lazyPrivateImplementationDetailsClass;
	}

	internal MethodSymbol EnsureThrowSwitchExpressionExceptionParameterlessExists(SyntaxNode syntaxNode, SyntheticBoundNodeFactory factory, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "ThrowSwitchExpressionExceptionParameterless", delegate(SynthesizedPrivateImplementationDetailsType privateImplClass, SyntheticBoundNodeFactory syntheticBoundNodeFactory)
		{
			TypeSymbol returnType = syntheticBoundNodeFactory.SpecialType(SpecialType.System_Void);
			return new SynthesizedParameterlessThrowMethod(privateImplClass, returnType, "ThrowSwitchExpressionExceptionParameterless", syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_SwitchExpressionException__ctor));
		}, factory, diagnostics);
	}

	internal MethodSymbol EnsureThrowInvalidOperationExceptionExists(SyntaxNode syntaxNode, SyntheticBoundNodeFactory factory, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "ThrowInvalidOperationException", delegate(SynthesizedPrivateImplementationDetailsType privateImplClass, SyntheticBoundNodeFactory syntheticBoundNodeFactory)
		{
			TypeSymbol returnType = syntheticBoundNodeFactory.SpecialType(SpecialType.System_Void);
			return new SynthesizedParameterlessThrowMethod(privateImplClass, returnType, "ThrowInvalidOperationException", syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_InvalidOperationException__ctor));
		}, factory, diagnostics);
	}

	internal MethodSymbol EnsureInlineArrayAsSpanExists(SyntaxNode syntaxNode, NamedTypeSymbol spanType, NamedTypeSymbol intType, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "InlineArrayAsSpan", (SynthesizedPrivateImplementationDetailsType privateImplClass, (NamedTypeSymbol spanType, NamedTypeSymbol intType) arg) => new SynthesizedInlineArrayAsSpanMethod(privateImplClass, "InlineArrayAsSpan", arg.spanType, arg.intType), (spanType, intType), diagnostics);
	}

	internal NamedTypeSymbol EnsureInlineArrayTypeExists(SyntaxNode syntaxNode, SyntheticBoundNodeFactory factory, int arrayLength, BindingDiagnosticBag diagnostics)
	{
		if (arrayLength >= 2 && arrayLength <= 16)
		{
			WellKnownType type = (WellKnownType)(350 + (arrayLength - 2));
			if (Binder.TryGetOptionalWellKnownType(Compilation, type, diagnostics, syntaxNode.Location, out NamedTypeSymbol typeSymbol))
			{
				return typeSymbol;
			}
		}
		return ConcurrentDictionaryExtensions.GetOrAdd(_inlineArrayTypes, arrayLength, delegate(int arrayLength2, (PEModuleBuilder @this, SyntheticBoundNodeFactory factory) arg)
		{
			MethodSymbol inlineArrayAttributeConstructor = (MethodSymbol)arg.factory.SpecialMember(SpecialMember.System_Runtime_CompilerServices_InlineArrayAttribute__ctor);
			string name = GeneratedNames.MakeSynthesizedInlineArrayName(arrayLength2, arg.@this.CurrentGenerationOrdinal);
			return new SynthesizedInlineArrayTypeSymbol(arg.@this.SourceModule, name, arrayLength2, inlineArrayAttributeConstructor);
		}, (this, factory));
	}

	internal NamedTypeSymbol EnsureReadOnlyListTypeExists(SyntaxNode syntaxNode, SynthesizedReadOnlyListKind kind, DiagnosticBag diagnostics)
	{
		ref NamedTypeSymbol reference = ref _readOnlyListTypes[(int)kind];
		NamedTypeSymbol value;
		if ((object)reference == null)
		{
			string name = GeneratedNames.MakeSynthesizedReadOnlyListName(kind, base.CurrentGenerationOrdinal);
			value = SynthesizedReadOnlyListTypeSymbol.Create(SourceModule, name, kind);
			Interlocked.CompareExchange(ref reference, value, null);
		}
		value = reference;
		DiagnosticInfo diagnosticInfo = value.GetUseSiteInfo().DiagnosticInfo;
		if (diagnosticInfo != null)
		{
			Symbol.ReportUseSiteDiagnostic(diagnosticInfo, diagnostics, syntaxNode.Location);
		}
		return value;
	}

	internal MethodSymbol EnsureInlineArrayAsReadOnlySpanExists(SyntaxNode syntaxNode, NamedTypeSymbol spanType, NamedTypeSymbol intType, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "InlineArrayAsReadOnlySpan", (SynthesizedPrivateImplementationDetailsType privateImplClass, (NamedTypeSymbol spanType, NamedTypeSymbol intType) arg) => new SynthesizedInlineArrayAsReadOnlySpanMethod(privateImplClass, "InlineArrayAsReadOnlySpan", arg.spanType, arg.intType), (spanType, intType), diagnostics);
	}

	internal MethodSymbol EnsureInlineArrayElementRefExists(SyntaxNode syntaxNode, NamedTypeSymbol intType, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "InlineArrayElementRef", (SynthesizedPrivateImplementationDetailsType privateImplClass, NamedTypeSymbol intType2) => new SynthesizedInlineArrayElementRefMethod(privateImplClass, "InlineArrayElementRef", intType2), intType, diagnostics);
	}

	internal MethodSymbol EnsureInlineArrayElementRefReadOnlyExists(SyntaxNode syntaxNode, NamedTypeSymbol intType, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "InlineArrayElementRefReadOnly", (SynthesizedPrivateImplementationDetailsType privateImplClass, NamedTypeSymbol intType2) => new SynthesizedInlineArrayElementRefReadOnlyMethod(privateImplClass, "InlineArrayElementRefReadOnly", intType2), intType, diagnostics);
	}

	internal MethodSymbol EnsureInlineArrayFirstElementRefExists(SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "InlineArrayFirstElementRef", (SynthesizedPrivateImplementationDetailsType privateImplClass, object _) => new SynthesizedInlineArrayFirstElementRefMethod(privateImplClass, "InlineArrayFirstElementRef"), null, diagnostics);
	}

	internal MethodSymbol EnsureInlineArrayFirstElementRefReadOnlyExists(SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		return EnsurePrivateImplClassMethodExists(syntaxNode, "InlineArrayFirstElementRefReadOnly", (SynthesizedPrivateImplementationDetailsType privateImplClass, object _) => new SynthesizedInlineArrayFirstElementRefReadOnlyMethod(privateImplClass, "InlineArrayFirstElementRefReadOnly"), null, diagnostics);
	}

	public override IEnumerable<INamespaceTypeDefinition> GetAdditionalTopLevelTypeDefinitions(EmitContext context)
	{
		return GetAdditionalTopLevelTypes();
	}

	public override ImmutableArray<NamedTypeSymbol> GetAdditionalTopLevelTypes()
	{
		ImmutableArray<NamedTypeSymbol> first = ImmutableArray<NamedTypeSymbol>.Empty;
		if (_inlineArrayTypes.Count != 0 || _readOnlyListTypes.Any((NamedTypeSymbol t) => (object)t != null))
		{
			ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance(_inlineArrayTypes.Count + _readOnlyListTypes.Length);
			instance.AddRange(_inlineArrayTypes.Values);
			NamedTypeSymbol[] readOnlyListTypes = _readOnlyListTypes;
			foreach (NamedTypeSymbol namedTypeSymbol in readOnlyListTypes)
			{
				if ((object)namedTypeSymbol != null)
				{
					instance.Add(namedTypeSymbol);
				}
			}
			instance.Sort((NamedTypeSymbol a, NamedTypeSymbol b) => StringComparer.Ordinal.Compare(a.MetadataName, b.MetadataName));
			first = instance.ToImmutableAndFree();
		}
		return first.Concat(base.GetAdditionalTopLevelTypes());
	}

	public override IEnumerable<INamespaceTypeDefinition> GetEmbeddedTypeDefinitions(EmitContext context)
	{
		return GetEmbeddedTypes(context.Diagnostics);
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetEmbeddedTypes(DiagnosticBag diagnostics)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
		ImmutableArray<NamedTypeSymbol> embeddedTypes = GetEmbeddedTypes(instance);
		diagnostics.AddRange(instance.DiagnosticBag);
		instance.Free();
		return embeddedTypes;
	}

	internal virtual ImmutableArray<NamedTypeSymbol> GetEmbeddedTypes(BindingDiagnosticBag diagnostics)
	{
		return base.GetEmbeddedTypes(diagnostics.DiagnosticBag);
	}

	internal bool TryGetTranslatedImports(ImportChain chain, out ImmutableArray<UsedNamespaceOrType> imports)
	{
		return _translatedImportsMap.TryGetValue(chain, out imports);
	}

	internal ImmutableArray<UsedNamespaceOrType> GetOrAddTranslatedImports(ImportChain chain, ImmutableArray<UsedNamespaceOrType> imports)
	{
		return _translatedImportsMap.GetOrAdd(chain, imports);
	}

	public override void AddSynthesizedDefinition(NamedTypeSymbol container, INestedTypeDefinition nestedType)
	{
		base.AddSynthesizedDefinition(container, nestedType);
	}

	public override void AddSynthesizedDefinition(NamedTypeSymbol container, IFieldDefinition field)
	{
		base.AddSynthesizedDefinition(container, field);
	}

	public override void AddSynthesizedDefinition(NamedTypeSymbol container, IMethodDefinition method)
	{
		base.AddSynthesizedDefinition(container, method);
	}

	public override void AddSynthesizedDefinition(NamedTypeSymbol container, IPropertyDefinition property)
	{
		base.AddSynthesizedDefinition(container, property);
	}
}
