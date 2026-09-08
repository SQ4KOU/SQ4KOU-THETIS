using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Emit.NoPia;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class NamedTypeSymbol : TypeSymbol, ITypeReference, IReference, ITypeDefinition, IDefinition, INamedTypeReference, INamedEntity, INamedTypeDefinition, INamespaceTypeReference, INamespaceTypeDefinition, INestedTypeReference, ITypeMemberReference, INestedTypeDefinition, ITypeDefinitionMember, IGenericTypeInstanceReference, ISpecializedNestedTypeReference, INamedTypeSymbolInternal, ITypeSymbolInternal, INamespaceOrTypeSymbolInternal, ISymbolInternal
{
	internal sealed class TupleExtraData
	{
		private ImmutableArray<TypeWithAnnotations> _lazyElementTypes;

		private ImmutableArray<FieldSymbol> _lazyDefaultElementFields;

		private SmallDictionary<Symbol, Symbol>? _lazyUnderlyingDefinitionToMemberMap;

		internal ImmutableArray<string?> ElementNames { get; }

		internal ImmutableArray<Location?> ElementLocations { get; }

		internal ImmutableArray<bool> ErrorPositions { get; }

		internal ImmutableArray<Location> Locations { get; }

		internal NamedTypeSymbol TupleUnderlyingType { get; }

		internal SmallDictionary<Symbol, Symbol> UnderlyingDefinitionToMemberMap
		{
			get
			{
				return _lazyUnderlyingDefinitionToMemberMap ?? (_lazyUnderlyingDefinitionToMemberMap = computeDefinitionToMemberMap());
				SmallDictionary<Symbol, Symbol> computeDefinitionToMemberMap()
				{
					SmallDictionary<Symbol, Symbol> smallDictionary = new SmallDictionary<Symbol, Symbol>(ReferenceEqualityComparer.Instance);
					ImmutableArray<Symbol> members = TupleUnderlyingType.GetMembers();
					for (int num = members.Length - 1; num >= 0; num--)
					{
						Symbol symbol = members[num];
						switch (symbol.Kind)
						{
						case SymbolKind.Method:
						case SymbolKind.NamedType:
						case SymbolKind.Property:
							smallDictionary.Add(symbol.OriginalDefinition, symbol);
							break;
						case SymbolKind.Field:
						{
							FieldSymbol tupleUnderlyingField = ((FieldSymbol)symbol).TupleUnderlyingField;
							if ((object)tupleUnderlyingField != null)
							{
								smallDictionary[tupleUnderlyingField.OriginalDefinition] = symbol;
							}
							break;
						}
						case SymbolKind.Event:
						{
							EventSymbol eventSymbol = (EventSymbol)symbol;
							FieldSymbol associatedField = eventSymbol.AssociatedField;
							if ((object)associatedField != null)
							{
								smallDictionary.Add(associatedField.OriginalDefinition, associatedField);
							}
							smallDictionary.Add(eventSymbol.OriginalDefinition, symbol);
							break;
						}
						default:
							throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
						}
					}
					return smallDictionary;
				}
			}
		}

		internal TupleExtraData(NamedTypeSymbol underlyingType)
		{
			TupleUnderlyingType = underlyingType;
			Locations = ImmutableArray<Location>.Empty;
		}

		internal TupleExtraData(NamedTypeSymbol underlyingType, ImmutableArray<string?> elementNames, ImmutableArray<Location?> elementLocations, ImmutableArray<bool> errorPositions, ImmutableArray<Location> locations)
			: this(underlyingType)
		{
			ElementNames = elementNames;
			ElementLocations = elementLocations;
			ErrorPositions = errorPositions;
			Locations = locations.NullToEmpty();
		}

		internal bool EqualsIgnoringTupleUnderlyingType(TupleExtraData? other)
		{
			if (other == null && ElementNames.IsDefault && ElementLocations.IsDefault && ErrorPositions.IsDefault)
			{
				return true;
			}
			if (other != null && areEqual<string>(ElementNames, other.ElementNames) && areEqual<Location>(ElementLocations, other.ElementLocations))
			{
				return areEqual<bool>(ErrorPositions, other.ErrorPositions);
			}
			return false;
			static bool areEqual<T>(ImmutableArray<T> one, ImmutableArray<T> items)
			{
				if (one.IsDefault && items.IsDefault)
				{
					return true;
				}
				if (one.IsDefault != items.IsDefault)
				{
					return false;
				}
				return one.SequenceEqual(items);
			}
		}

		public ImmutableArray<TypeWithAnnotations> TupleElementTypesWithAnnotations(NamedTypeSymbol tuple)
		{
			if (_lazyElementTypes.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyElementTypes, collectTupleElementTypesWithAnnotations(tuple));
			}
			return _lazyElementTypes;
			static ImmutableArray<TypeWithAnnotations> collectTupleElementTypesWithAnnotations(NamedTypeSymbol namedTypeSymbol)
			{
				if (namedTypeSymbol.Arity == 8)
				{
					ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[7].Type.TupleElementTypesWithAnnotations;
					ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(7 + tupleElementTypesWithAnnotations.Length);
					instance.AddRange(namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics, 7);
					instance.AddRange(tupleElementTypesWithAnnotations);
					return instance.ToImmutableAndFree();
				}
				return namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
			}
		}

		public ImmutableArray<FieldSymbol> TupleElements(NamedTypeSymbol tuple)
		{
			if (_lazyDefaultElementFields.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyDefaultElementFields, collectTupleElementFields(tuple));
			}
			return _lazyDefaultElementFields;
			ImmutableArray<FieldSymbol> collectTupleElementFields(NamedTypeSymbol namedTypeSymbol)
			{
				ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance(TupleElementTypesWithAnnotations(namedTypeSymbol).Length, null);
				foreach (Symbol member in namedTypeSymbol.GetMembers())
				{
					if (member.Kind == SymbolKind.Field)
					{
						FieldSymbol fieldSymbol = (FieldSymbol)member;
						int tupleElementIndex = fieldSymbol.TupleElementIndex;
						if (tupleElementIndex >= 0)
						{
							FieldSymbol fieldSymbol2 = instance[tupleElementIndex];
							if ((object)fieldSymbol2 == null || fieldSymbol2.IsDefaultTupleElement)
							{
								instance[tupleElementIndex] = fieldSymbol;
							}
						}
					}
				}
				return instance.ToImmutableAndFree();
			}
		}

		public TMember? GetTupleMemberSymbolForUnderlyingMember<TMember>(TMember? underlyingMemberOpt) where TMember : Symbol
		{
			if ((object)underlyingMemberOpt == null)
			{
				return null;
			}
			Symbol symbol = underlyingMemberOpt.OriginalDefinition;
			if (symbol is TupleElementFieldSymbol tupleElementFieldSymbol)
			{
				symbol = tupleElementFieldSymbol.UnderlyingField;
			}
			if (TypeSymbol.Equals(symbol.ContainingType, TupleUnderlyingType.OriginalDefinition, TypeCompareKind.ConsiderEverything) && UnderlyingDefinitionToMemberMap.TryGetValue(symbol, out Symbol value))
			{
				return (TMember)value;
			}
			return null;
		}
	}

	private bool _hasNoBaseCycles;

	private static readonly ImmutableSegmentedDictionary<string, Symbol> RequiredMembersErrorSentinel = ImmutableSegmentedDictionary<string, Symbol>.Empty.Add("<error sentinel>", null);

	private ImmutableSegmentedDictionary<string, Symbol> _lazyRequiredMembers;

	protected static Func<Symbol, bool> IsInstanceFieldOrEvent = delegate(Symbol symbol)
	{
		if (!symbol.IsStatic)
		{
			SymbolKind kind = symbol.Kind;
			if ((uint)(kind - 5) <= 1u)
			{
				return true;
			}
		}
		return false;
	};

	internal static readonly Func<TypeWithAnnotations, bool> TypeWithAnnotationsIsNullFunction = (TypeWithAnnotations type) => !type.HasType;

	internal static readonly Func<TypeWithAnnotations, bool> TypeWithAnnotationsIsErrorType = (TypeWithAnnotations type) => type.HasType && type.Type.IsErrorType();

	internal const int ValueTupleRestPosition = 8;

	internal const int ValueTupleRestIndex = 7;

	internal const string ValueTupleTypeName = "ValueTuple";

	internal const string ValueTupleRestFieldName = "Rest";

	private TupleExtraData? _lazyTupleData;

	private static readonly WellKnownType[] tupleTypes = new WellKnownType[8]
	{
		WellKnownType.System_ValueTuple_T1,
		WellKnownType.System_ValueTuple_T2,
		WellKnownType.System_ValueTuple_T3,
		WellKnownType.System_ValueTuple_T4,
		WellKnownType.System_ValueTuple_T5,
		WellKnownType.System_ValueTuple_T6,
		WellKnownType.System_ValueTuple_T7,
		WellKnownType.System_ValueTuple_TRest
	};

	private static readonly WellKnownMember[] tupleCtors = new WellKnownMember[8]
	{
		WellKnownMember.System_ValueTuple_T1__ctor,
		WellKnownMember.System_ValueTuple_T2__ctor,
		WellKnownMember.System_ValueTuple_T3__ctor,
		WellKnownMember.System_ValueTuple_T4__ctor,
		WellKnownMember.System_ValueTuple_T5__ctor,
		WellKnownMember.System_ValueTuple_T6__ctor,
		WellKnownMember.System_ValueTuple_T7__ctor,
		WellKnownMember.System_ValueTuple_TRest__ctor
	};

	private static readonly WellKnownMember[][] tupleMembers = new WellKnownMember[8][]
	{
		new WellKnownMember[1] { WellKnownMember.System_ValueTuple_T1__Item1 },
		new WellKnownMember[2]
		{
			WellKnownMember.System_ValueTuple_T2__Item1,
			WellKnownMember.System_ValueTuple_T2__Item2
		},
		new WellKnownMember[3]
		{
			WellKnownMember.System_ValueTuple_T3__Item1,
			WellKnownMember.System_ValueTuple_T3__Item2,
			WellKnownMember.System_ValueTuple_T3__Item3
		},
		new WellKnownMember[4]
		{
			WellKnownMember.System_ValueTuple_T4__Item1,
			WellKnownMember.System_ValueTuple_T4__Item2,
			WellKnownMember.System_ValueTuple_T4__Item3,
			WellKnownMember.System_ValueTuple_T4__Item4
		},
		new WellKnownMember[5]
		{
			WellKnownMember.System_ValueTuple_T5__Item1,
			WellKnownMember.System_ValueTuple_T5__Item2,
			WellKnownMember.System_ValueTuple_T5__Item3,
			WellKnownMember.System_ValueTuple_T5__Item4,
			WellKnownMember.System_ValueTuple_T5__Item5
		},
		new WellKnownMember[6]
		{
			WellKnownMember.System_ValueTuple_T6__Item1,
			WellKnownMember.System_ValueTuple_T6__Item2,
			WellKnownMember.System_ValueTuple_T6__Item3,
			WellKnownMember.System_ValueTuple_T6__Item4,
			WellKnownMember.System_ValueTuple_T6__Item5,
			WellKnownMember.System_ValueTuple_T6__Item6
		},
		new WellKnownMember[7]
		{
			WellKnownMember.System_ValueTuple_T7__Item1,
			WellKnownMember.System_ValueTuple_T7__Item2,
			WellKnownMember.System_ValueTuple_T7__Item3,
			WellKnownMember.System_ValueTuple_T7__Item4,
			WellKnownMember.System_ValueTuple_T7__Item5,
			WellKnownMember.System_ValueTuple_T7__Item6,
			WellKnownMember.System_ValueTuple_T7__Item7
		},
		new WellKnownMember[8]
		{
			WellKnownMember.System_ValueTuple_TRest__Item1,
			WellKnownMember.System_ValueTuple_TRest__Item2,
			WellKnownMember.System_ValueTuple_TRest__Item3,
			WellKnownMember.System_ValueTuple_TRest__Item4,
			WellKnownMember.System_ValueTuple_TRest__Item5,
			WellKnownMember.System_ValueTuple_TRest__Item6,
			WellKnownMember.System_ValueTuple_TRest__Item7,
			WellKnownMember.System_ValueTuple_TRest__Rest
		}
	};

	bool IDefinition.IsEncDeleted => false;

	bool ITypeReference.IsEnum => AdaptedNamedTypeSymbol.TypeKind == TypeKind.Enum;

	bool ITypeReference.IsValueType => AdaptedNamedTypeSymbol.IsValueType;

	Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode
	{
		get
		{
			if (AdaptedNamedTypeSymbol.IsDefinition)
			{
				return AdaptedNamedTypeSymbol.PrimitiveTypeCode;
			}
			return Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;
		}
	}

	TypeDefinitionHandle ITypeReference.TypeDef
	{
		get
		{
			if (AdaptedNamedTypeSymbol is PENamedTypeSymbol pENamedTypeSymbol)
			{
				return pENamedTypeSymbol.Handle;
			}
			return default(TypeDefinitionHandle);
		}
	}

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

	IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference
	{
		get
		{
			if (!AdaptedNamedTypeSymbol.IsDefinition && AdaptedNamedTypeSymbol.Arity > 0)
			{
				return this;
			}
			return null;
		}
	}

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference
	{
		get
		{
			if (AdaptedNamedTypeSymbol.IsDefinition && (object)AdaptedNamedTypeSymbol.ContainingType == null)
			{
				return this;
			}
			return null;
		}
	}

	INestedTypeReference ITypeReference.AsNestedTypeReference
	{
		get
		{
			if ((object)AdaptedNamedTypeSymbol.ContainingType != null)
			{
				return this;
			}
			return null;
		}
	}

	bool INestedTypeReference.InheritsEnclosingTypeTypeParameters => true;

	ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference
	{
		get
		{
			if (!AdaptedNamedTypeSymbol.IsDefinition && (AdaptedNamedTypeSymbol.Arity == 0 || PEModuleBuilder.IsGenericType(AdaptedNamedTypeSymbol.ContainingType)))
			{
				return this;
			}
			return null;
		}
	}

	IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters
	{
		get
		{
			foreach (TypeParameterSymbol typeParameter in AdaptedNamedTypeSymbol.TypeParameters)
			{
				yield return typeParameter.GetCciAdapter();
			}
		}
	}

	ushort ITypeDefinition.GenericParameterCount => GenericParameterCountImpl;

	private ushort GenericParameterCountImpl => (ushort)AdaptedNamedTypeSymbol.Arity;

	bool ITypeDefinition.IsAbstract => AdaptedNamedTypeSymbol.IsMetadataAbstract;

	bool ITypeDefinition.IsBeforeFieldInit
	{
		get
		{
			TypeKind typeKind = AdaptedNamedTypeSymbol.TypeKind;
			if (typeKind == TypeKind.Delegate || typeKind == TypeKind.Enum)
			{
				return false;
			}
			foreach (Symbol member in AdaptedNamedTypeSymbol.GetMembers(".cctor"))
			{
				if (!member.IsImplicitlyDeclared)
				{
					return false;
				}
			}
			return true;
		}
	}

	bool ITypeDefinition.IsComObject => AdaptedNamedTypeSymbol.IsComImport;

	bool ITypeDefinition.IsGeneric => AdaptedNamedTypeSymbol.Arity != 0;

	bool ITypeDefinition.IsInterface => AdaptedNamedTypeSymbol.IsInterface;

	bool ITypeDefinition.IsDelegate => AdaptedNamedTypeSymbol.IsDelegateType();

	bool ITypeDefinition.IsRuntimeSpecial => false;

	bool ITypeDefinition.IsSerializable => AdaptedNamedTypeSymbol.IsSerializable;

	bool ITypeDefinition.IsSpecialName => AdaptedNamedTypeSymbol.HasSpecialName;

	bool ITypeDefinition.IsWindowsRuntimeImport => AdaptedNamedTypeSymbol.IsWindowsRuntimeImport;

	bool ITypeDefinition.IsSealed => AdaptedNamedTypeSymbol.IsMetadataSealed;

	bool ITypeDefinition.HasDeclarativeSecurity => AdaptedNamedTypeSymbol.HasDeclarativeSecurity;

	IEnumerable<SecurityAttribute> ITypeDefinition.SecurityAttributes => AdaptedNamedTypeSymbol.GetSecurityInformation() ?? SpecializedCollections.EmptyEnumerable<SecurityAttribute>();

	ushort ITypeDefinition.Alignment => (ushort)AdaptedNamedTypeSymbol.Layout.Alignment;

	LayoutKind ITypeDefinition.Layout => AdaptedNamedTypeSymbol.Layout.Kind;

	uint ITypeDefinition.SizeOf => (uint)AdaptedNamedTypeSymbol.Layout.Size;

	CharSet ITypeDefinition.StringFormat => AdaptedNamedTypeSymbol.MarshallingCharSet;

	ushort INamedTypeReference.GenericParameterCount => GenericParameterCountImpl;

	bool INamedTypeReference.MangleName => AdaptedNamedTypeSymbol.MangleName;

	string? INamedTypeReference.AssociatedFileIdentifier => AdaptedNamedTypeSymbol.GetFileLocalTypeMetadataNamePrefix();

	string INamedEntity.Name
	{
		get
		{
			if (AdaptedNamedTypeSymbol.IsExtension)
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/NamedTypeSymbolAdapter.cs", 800);
			}
			return AdaptedNamedTypeSymbol.Name;
		}
	}

	string INamespaceTypeReference.NamespaceName => AdaptedNamedTypeSymbol.ContainingNamespace.QualifiedName;

	bool INamespaceTypeDefinition.IsPublic => AdaptedNamedTypeSymbol.MetadataVisibility == TypeMemberVisibility.Public;

	ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => AdaptedNamedTypeSymbol.ContainingType.GetCciAdapter();

	TypeMemberVisibility ITypeDefinitionMember.Visibility => AdaptedNamedTypeSymbol.MetadataVisibility;

	internal NamedTypeSymbol AdaptedNamedTypeSymbol => this;

	internal virtual bool IsMetadataAbstract
	{
		get
		{
			if (!IsAbstract)
			{
				return IsStatic;
			}
			return true;
		}
	}

	internal virtual bool IsMetadataSealed
	{
		get
		{
			if (!IsSealed)
			{
				return IsStatic;
			}
			return true;
		}
	}

	public abstract int Arity { get; }

	public abstract ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

	internal abstract ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics { get; }

	public abstract NamedTypeSymbol ConstructedFrom { get; }

	public virtual NamedTypeSymbol EnumUnderlyingType => null;

	public override NamedTypeSymbol ContainingType => ContainingSymbol as NamedTypeSymbol;

	internal virtual bool KnownCircularStruct => false;

	internal bool KnownToHaveNoDeclaredBaseCycles => _hasNoBaseCycles;

	internal virtual bool IsExplicitDefinitionOfNoPiaLocalType => false;

	public MethodSymbol? DelegateInvokeMethod
	{
		get
		{
			if (TypeKind != TypeKind.Delegate)
			{
				return null;
			}
			ImmutableArray<Symbol> members = GetMembers("Invoke");
			if (members.Length != 1)
			{
				return null;
			}
			return members[0] as MethodSymbol;
		}
	}

	public ImmutableArray<MethodSymbol> InstanceConstructors => GetConstructors(includeInstance: true, includeStatic: false);

	public ImmutableArray<MethodSymbol> StaticConstructors => GetConstructors(includeInstance: false, includeStatic: true);

	public ImmutableArray<MethodSymbol> Constructors => GetConstructors(includeInstance: true, includeStatic: true);

	public ImmutableArray<PropertySymbol> Indexers
	{
		get
		{
			ImmutableArray<Symbol> simpleNonTypeMembers = GetSimpleNonTypeMembers("this[]");
			if (simpleNonTypeMembers.IsEmpty)
			{
				return ImmutableArray<PropertySymbol>.Empty;
			}
			ArrayBuilder<PropertySymbol> instance = ArrayBuilder<PropertySymbol>.GetInstance();
			foreach (Symbol item in simpleNonTypeMembers)
			{
				if (item.Kind == SymbolKind.Property)
				{
					instance.Add((PropertySymbol)item);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	public abstract bool MightContainExtensionMethods { get; }

	public override bool IsReferenceType
	{
		get
		{
			TypeKind typeKind = TypeKind;
			if (typeKind != TypeKind.Enum && typeKind != TypeKind.Struct)
			{
				return typeKind != TypeKind.Error;
			}
			return false;
		}
	}

	public override bool IsValueType
	{
		get
		{
			TypeKind typeKind = TypeKind;
			if (typeKind != TypeKind.Struct)
			{
				return typeKind == TypeKind.Enum;
			}
			return true;
		}
	}

	public virtual bool IsScriptClass => false;

	internal bool IsSubmissionClass => TypeKind == TypeKind.Submission;

	public virtual bool IsImplicitClass => false;

	public abstract override string Name { get; }

	public override string MetadataName
	{
		get
		{
			string fileLocalTypeMetadataNamePrefix = this.GetFileLocalTypeMetadataNamePrefix();
			if (fileLocalTypeMetadataNamePrefix == null && !MangleName)
			{
				return Name;
			}
			return MetadataHelpers.ComposeAritySuffixedMetadataName(Name, Arity, fileLocalTypeMetadataNamePrefix);
		}
	}

	internal abstract bool IsFileLocal { get; }

	internal abstract FileIdentifier? AssociatedFileIdentifier { get; }

	[MemberNotNullWhen(true, new string[] { "ExtensionGroupingName", "ExtensionMarkerName" })]
	public virtual bool IsExtension
	{
		[MemberNotNullWhen(true, new string[] { "ExtensionGroupingName", "ExtensionMarkerName" })]
		get
		{
			return TypeKind == TypeKind.Extension;
		}
	}

	internal abstract ParameterSymbol? ExtensionParameter { get; }

	internal abstract string? ExtensionGroupingName { get; }

	internal abstract string? ExtensionMarkerName { get; }

	internal abstract bool MangleName { get; }

	public abstract IEnumerable<string> MemberNames { get; }

	internal abstract bool HasDeclaredRequiredMembers { get; }

	internal bool HasRequiredMembersError
	{
		get
		{
			EnsureRequiredMembersCalculated();
			return _lazyRequiredMembers == RequiredMembersErrorSentinel;
		}
	}

	internal bool HasAnyRequiredMembers
	{
		get
		{
			if (!HasDeclaredRequiredMembers)
			{
				return !AllRequiredMembers.IsEmpty;
			}
			return true;
		}
	}

	internal ImmutableSegmentedDictionary<string, Symbol> AllRequiredMembers
	{
		get
		{
			EnsureRequiredMembersCalculated();
			if (_lazyRequiredMembers == RequiredMembersErrorSentinel)
			{
				return ImmutableSegmentedDictionary<string, Symbol>.Empty;
			}
			return _lazyRequiredMembers;
		}
	}

	public abstract override Accessibility DeclaredAccessibility { get; }

	public override SymbolKind Kind => SymbolKind.NamedType;

	internal abstract bool HasCodeAnalysisEmbeddedAttribute { get; }

	internal abstract bool HasCompilerLoweringPreserveAttribute { get; }

	internal abstract bool IsInterpolatedStringHandlerType { get; }

	public bool IsGenericType
	{
		get
		{
			NamedTypeSymbol namedTypeSymbol = this;
			while ((object)namedTypeSymbol != null)
			{
				if (namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length != 0)
				{
					return true;
				}
				namedTypeSymbol = namedTypeSymbol.ContainingType;
			}
			return false;
		}
	}

	public virtual bool IsUnboundGenericType => false;

	public new virtual NamedTypeSymbol OriginalDefinition => this;

	protected sealed override TypeSymbol OriginalTypeSymbolDefinition => OriginalDefinition;

	internal virtual TypeMap TypeSubstitution => null;

	internal virtual bool IsDirectlyExcludedFromCodeCoverage => false;

	internal abstract bool HasSpecialName { get; }

	internal abstract bool IsComImport { get; }

	internal abstract bool IsWindowsRuntimeImport { get; }

	internal abstract bool ShouldAddWinRTMembers { get; }

	internal bool IsConditional
	{
		get
		{
			if (GetAppliedConditionalSymbols().Any())
			{
				return true;
			}
			return BaseTypeNoUseSiteDiagnostics?.IsConditional ?? false;
		}
	}

	public abstract bool IsSerializable { get; }

	public abstract bool AreLocalsZeroed { get; }

	internal abstract TypeLayout Layout { get; }

	protected CharSet DefaultMarshallingCharSet => GetEffectiveDefaultMarshallingCharSet() ?? CharSet.Ansi;

	internal abstract CharSet MarshallingCharSet { get; }

	internal abstract bool HasDeclarativeSecurity { get; }

	internal virtual NamedTypeSymbol ComImportCoClass => null;

	internal virtual FieldSymbol FixedElementField => null;

	internal abstract bool IsInterface { get; }

	internal abstract NamedTypeSymbol NativeIntegerUnderlyingType { get; }

	INamedTypeSymbolInternal INamedTypeSymbolInternal.EnumUnderlyingType => EnumUnderlyingType;

	internal NamedTypeSymbol? TupleUnderlyingType
	{
		get
		{
			if (_lazyTupleData == null)
			{
				if (!IsTupleType)
				{
					return null;
				}
				return this;
			}
			return TupleData.TupleUnderlyingType;
		}
	}

	public sealed override bool IsTupleType
	{
		get
		{
			int tupleCardinality;
			return IsTupleTypeOfCardinality(out tupleCardinality);
		}
	}

	internal TupleExtraData? TupleData
	{
		get
		{
			if (!IsTupleType)
			{
				return null;
			}
			if (_lazyTupleData == null)
			{
				Interlocked.CompareExchange(ref _lazyTupleData, new TupleExtraData(this), null);
			}
			return _lazyTupleData;
		}
	}

	public sealed override ImmutableArray<string?> TupleElementNames
	{
		get
		{
			if (_lazyTupleData != null)
			{
				return _lazyTupleData.ElementNames;
			}
			return default(ImmutableArray<string>);
		}
	}

	private ImmutableArray<bool> TupleErrorPositions
	{
		get
		{
			if (_lazyTupleData != null)
			{
				return _lazyTupleData.ErrorPositions;
			}
			return default(ImmutableArray<bool>);
		}
	}

	private ImmutableArray<Location?> TupleElementLocations
	{
		get
		{
			if (_lazyTupleData != null)
			{
				return _lazyTupleData.ElementLocations;
			}
			return default(ImmutableArray<Location>);
		}
	}

	public sealed override ImmutableArray<TypeWithAnnotations> TupleElementTypesWithAnnotations
	{
		get
		{
			if (!IsTupleType)
			{
				return default(ImmutableArray<TypeWithAnnotations>);
			}
			return TupleData.TupleElementTypesWithAnnotations(this);
		}
	}

	public sealed override ImmutableArray<FieldSymbol> TupleElements
	{
		get
		{
			if (!IsTupleType)
			{
				return default(ImmutableArray<FieldSymbol>);
			}
			return TupleData.TupleElements(this);
		}
	}

	ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return AsTypeDefinitionImpl(moduleBeingBuilt);
	}

	INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		if ((object)AdaptedNamedTypeSymbol.ContainingType == null && AdaptedNamedTypeSymbol.IsDefinition && AdaptedNamedTypeSymbol.ContainingModule == pEModuleBuilder.SourceModule)
		{
			return this;
		}
		return null;
	}

	INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return AsNestedTypeDefinitionImpl(moduleBeingBuilt);
	}

	private INestedTypeDefinition AsNestedTypeDefinitionImpl(PEModuleBuilder moduleBeingBuilt)
	{
		if ((object)AdaptedNamedTypeSymbol.ContainingType != null && AdaptedNamedTypeSymbol.IsDefinition && AdaptedNamedTypeSymbol.ContainingModule == moduleBeingBuilt.SourceModule)
		{
			return this;
		}
		return null;
	}

	ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return AsTypeDefinitionImpl(moduleBeingBuilt);
	}

	private ITypeDefinition AsTypeDefinitionImpl(PEModuleBuilder moduleBeingBuilt)
	{
		if (AdaptedNamedTypeSymbol.IsDefinition && AdaptedNamedTypeSymbol.ContainingModule == moduleBeingBuilt.SourceModule)
		{
			return this;
		}
		return null;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/NamedTypeSymbolAdapter.cs", 223);
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return AsTypeDefinitionImpl(moduleBeingBuilt);
	}

	ITypeReference ITypeDefinition.GetBaseClass(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		NamedTypeSymbol namedTypeSymbol = AdaptedNamedTypeSymbol.BaseTypeNoUseSiteDiagnostics;
		if (AdaptedNamedTypeSymbol.IsScriptClass || AdaptedNamedTypeSymbol.IsExtension)
		{
			namedTypeSymbol = AdaptedNamedTypeSymbol.ContainingAssembly.GetSpecialType(SpecialType.System_Object);
		}
		if ((object)namedTypeSymbol == null)
		{
			return null;
		}
		return pEModuleBuilder.Translate(namedTypeSymbol, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics);
	}

	IEnumerable<IEventDefinition> ITypeDefinition.GetEvents(EmitContext context)
	{
		foreach (EventSymbol item in AdaptedNamedTypeSymbol.GetEventsToEmit())
		{
			IEventDefinition cciAdapter = item.GetCciAdapter();
			if (cciAdapter.ShouldInclude(context) || !cciAdapter.GetAccessors(context).IsEmpty())
			{
				yield return cciAdapter;
			}
		}
	}

	IEnumerable<Microsoft.Cci.MethodImplementation> ITypeDefinition.GetExplicitImplementationOverrides(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		foreach (Symbol member in AdaptedNamedTypeSymbol.GetMembers())
		{
			if (member.Kind != SymbolKind.Method)
			{
				continue;
			}
			MethodSymbol method = (MethodSymbol)member;
			if (method.ExplicitInterfaceImplementations.Length != 0)
			{
				MethodSymbol adapter = method.GetCciAdapter();
				foreach (MethodSymbol explicitInterfaceImplementation in method.ExplicitInterfaceImplementations)
				{
					yield return new Microsoft.Cci.MethodImplementation(adapter, moduleBeingBuilt.TranslateOverriddenMethodReference(explicitInterfaceImplementation, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics));
				}
			}
			if (AdaptedNamedTypeSymbol.IsInterface)
			{
				continue;
			}
			if (method.RequiresExplicitOverride(out var _))
			{
				yield return new Microsoft.Cci.MethodImplementation(method.GetCciAdapter(), moduleBeingBuilt.TranslateOverriddenMethodReference(method.OverriddenMethod, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics));
			}
			else
			{
				if (method.MethodKind != MethodKind.Destructor || AdaptedNamedTypeSymbol.SpecialType == SpecialType.System_Object)
				{
					continue;
				}
				TypeSymbol specialType = AdaptedNamedTypeSymbol.DeclaringCompilation.GetSpecialType(SpecialType.System_Object);
				foreach (Symbol member2 in specialType.GetMembers("Finalize"))
				{
					if (member2 is MethodSymbol { MethodKind: MethodKind.Destructor } methodSymbol)
					{
						yield return new Microsoft.Cci.MethodImplementation(method.GetCciAdapter(), moduleBeingBuilt.TranslateOverriddenMethodReference(methodSymbol, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics));
					}
				}
			}
		}
		if (AdaptedNamedTypeSymbol.IsInterface)
		{
			yield break;
		}
		if (AdaptedNamedTypeSymbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
		{
			ImmutableArray<NamedTypeSymbol> interfaces = sourceMemberContainerTypeSymbol.GetInterfacesToEmit();
			foreach (var (methodSymbol2, methodSymbol3) in sourceMemberContainerTypeSymbol.GetSynthesizedExplicitImplementations(default(CancellationToken)).MethodImpls)
			{
				if (interfaces.Contains(methodSymbol3.ContainingType, SymbolEqualityComparer.ConsiderEverything))
				{
					yield return new Microsoft.Cci.MethodImplementation(methodSymbol2.GetCciAdapter(), moduleBeingBuilt.TranslateOverriddenMethodReference(methodSymbol3, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics));
				}
			}
		}
		IEnumerable<IMethodDefinition> synthesizedMethods = moduleBeingBuilt.GetSynthesizedMethods(AdaptedNamedTypeSymbol);
		if (synthesizedMethods == null)
		{
			yield break;
		}
		foreach (IMethodDefinition m in synthesizedMethods)
		{
			if (m.GetInternalSymbol() is MethodSymbol { ExplicitInterfaceImplementations: var explicitInterfaceImplementations })
			{
				foreach (MethodSymbol item in explicitInterfaceImplementations)
				{
					yield return new Microsoft.Cci.MethodImplementation(m, moduleBeingBuilt.TranslateOverriddenMethodReference(item, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics));
				}
			}
		}
	}

	IEnumerable<IFieldDefinition> ITypeDefinition.GetFields(EmitContext context)
	{
		bool isStruct = AdaptedNamedTypeSymbol.IsStructType();
		foreach (FieldSymbol item in AdaptedNamedTypeSymbol.GetFieldsToEmit())
		{
			if (isStruct || item.GetCciAdapter().ShouldInclude(context))
			{
				yield return item.GetCciAdapter();
			}
		}
		IEnumerable<IFieldDefinition> synthesizedFields = ((PEModuleBuilder)context.Module).GetSynthesizedFields(AdaptedNamedTypeSymbol);
		if (synthesizedFields == null)
		{
			yield break;
		}
		foreach (IFieldDefinition item2 in synthesizedFields)
		{
			if (isStruct || item2.ShouldInclude(context))
			{
				yield return item2;
			}
		}
	}

	IEnumerable<TypeReferenceWithAttributes> ITypeDefinition.Interfaces(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		foreach (NamedTypeSymbol item in AdaptedNamedTypeSymbol.GetInterfacesToEmit())
		{
			INamedTypeReference typeRef = moduleBeingBuilt.Translate(item, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics, fromImplements: true);
			TypeWithAnnotations type = TypeWithAnnotations.Create(item);
			yield return type.GetTypeRefWithAttributes(moduleBeingBuilt, AdaptedNamedTypeSymbol, typeRef);
		}
	}

	IEnumerable<IMethodDefinition> ITypeDefinition.GetMethods(EmitContext context)
	{
		bool alwaysIncludeConstructors = context.IncludePrivateMembers || AdaptedNamedTypeSymbol.DeclaringCompilation.IsAttributeType(AdaptedNamedTypeSymbol);
		foreach (MethodSymbol item in AdaptedNamedTypeSymbol.GetMethodsToEmit())
		{
			if ((alwaysIncludeConstructors && item.MethodKind == MethodKind.Constructor) || item.GetCciAdapter().ShouldInclude(context))
			{
				yield return item.GetCciAdapter();
			}
		}
		IEnumerable<IMethodDefinition> synthesizedMethods = ((PEModuleBuilder)context.Module).GetSynthesizedMethods(AdaptedNamedTypeSymbol);
		if (synthesizedMethods == null)
		{
			yield break;
		}
		foreach (IMethodDefinition item2 in synthesizedMethods)
		{
			if ((alwaysIncludeConstructors && item2.IsConstructor) || item2.ShouldInclude(context))
			{
				yield return item2;
			}
		}
	}

	IEnumerable<INestedTypeDefinition> ITypeDefinition.GetNestedTypes(EmitContext context)
	{
		foreach (NamedTypeSymbol typeMember in AdaptedNamedTypeSymbol.GetTypeMembers())
		{
			if (!typeMember.IsExtension)
			{
				yield return typeMember.GetCciAdapter();
			}
		}
		if (AdaptedNamedTypeSymbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
		{
			MergedTypeDeclaration mergedDeclaration = sourceMemberContainerTypeSymbol.MergedDeclaration;
			if (mergedDeclaration != null && mergedDeclaration.ContainsExtensionDeclarations)
			{
				foreach (INestedTypeDefinition groupingType in sourceMemberContainerTypeSymbol.GetExtensionGroupingInfo().GetGroupingTypes())
				{
					yield return groupingType;
				}
			}
		}
		IEnumerable<INestedTypeDefinition> synthesizedTypes = ((PEModuleBuilder)context.Module).GetSynthesizedTypes(AdaptedNamedTypeSymbol);
		if (synthesizedTypes == null)
		{
			yield break;
		}
		foreach (INestedTypeDefinition item in synthesizedTypes)
		{
			yield return item;
		}
	}

	IEnumerable<IPropertyDefinition> ITypeDefinition.GetProperties(EmitContext context)
	{
		foreach (PropertySymbol item in AdaptedNamedTypeSymbol.GetPropertiesToEmit())
		{
			IPropertyDefinition cciAdapter = item.GetCciAdapter();
			if (cciAdapter.ShouldInclude(context) || !cciAdapter.GetAccessors(context).IsEmpty())
			{
				yield return cciAdapter;
			}
		}
		IEnumerable<IPropertyDefinition> synthesizedProperties = ((PEModuleBuilder)context.Module).GetSynthesizedProperties(AdaptedNamedTypeSymbol);
		if (synthesizedProperties == null)
		{
			yield break;
		}
		foreach (IPropertyDefinition item2 in synthesizedProperties)
		{
			if (item2.ShouldInclude(context) || !item2.GetAccessors(context).IsEmpty())
			{
				yield return item2;
			}
		}
	}

	IUnitReference INamespaceTypeReference.GetUnit(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(AdaptedNamedTypeSymbol.ContainingModule, context.Diagnostics);
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(AdaptedNamedTypeSymbol.ContainingType, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics, fromImplements: false, AdaptedNamedTypeSymbol.IsDefinition);
	}

	ImmutableArray<ITypeReference> IGenericTypeInstanceReference.GetGenericArguments(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		ArrayBuilder<ITypeReference> instance = ArrayBuilder<ITypeReference>.GetInstance();
		ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = AdaptedNamedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
		for (int i = 0; i < typeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length; i++)
		{
			ITypeReference typeReference = pEModuleBuilder.Translate(typeArgumentsWithAnnotationsNoUseSiteDiagnostics[i].Type, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics);
			ImmutableArray<CustomModifier> customModifiers = typeArgumentsWithAnnotationsNoUseSiteDiagnostics[i].CustomModifiers;
			if (!customModifiers.IsDefaultOrEmpty)
			{
				typeReference = new ModifiedTypeReference(typeReference, ImmutableArray<ICustomModifier>.CastUp(customModifiers));
			}
			instance.Add(typeReference);
		}
		return instance.ToImmutableAndFree();
	}

	INamedTypeReference IGenericTypeInstanceReference.GetGenericType(EmitContext context)
	{
		return GenericTypeImpl(context);
	}

	private INamedTypeReference GenericTypeImpl(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(AdaptedNamedTypeSymbol.OriginalDefinition, (CSharpSyntaxNode)context.SyntaxNode, context.Diagnostics, fromImplements: false, needDeclaration: true);
	}

	INestedTypeReference ISpecializedNestedTypeReference.GetUnspecializedVersion(EmitContext context)
	{
		return GenericTypeImpl(context).AsNestedTypeReference;
	}

	internal new NamedTypeSymbol GetCciAdapter()
	{
		return this;
	}

	internal virtual IEnumerable<EventSymbol> GetEventsToEmit()
	{
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Event)
			{
				yield return (EventSymbol)member;
			}
		}
	}

	internal abstract IEnumerable<FieldSymbol> GetFieldsToEmit();

	internal abstract ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit();

	protected ImmutableArray<NamedTypeSymbol> CalculateInterfacesToEmit()
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		HashSet<NamedTypeSymbol> seen = null;
		InterfacesVisit(this, instance, ref seen);
		return instance.ToImmutableAndFree();
	}

	private static void InterfacesVisit(NamedTypeSymbol namedType, ArrayBuilder<NamedTypeSymbol> builder, ref HashSet<NamedTypeSymbol> seen)
	{
		foreach (NamedTypeSymbol item in namedType.InterfacesNoUseSiteDiagnostics())
		{
			if (seen == null)
			{
				seen = new HashSet<NamedTypeSymbol>(SymbolEqualityComparer.CLRSignature);
			}
			if (seen.Add(item))
			{
				builder.Add(item);
				InterfacesVisit(item, builder, ref seen);
			}
		}
	}

	internal virtual IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)member;
				if (methodSymbol.ShouldEmit())
				{
					yield return methodSymbol;
				}
			}
		}
	}

	internal virtual IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Property)
			{
				yield return (PropertySymbol)member;
			}
		}
	}

	internal NamedTypeSymbol(TupleExtraData tupleData = null)
	{
		_lazyTupleData = tupleData;
	}

	internal ImmutableArray<TypeWithAnnotations> TypeArgumentsWithDefinitionUseSiteDiagnostics(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
		foreach (TypeWithAnnotations item in typeArgumentsWithAnnotationsNoUseSiteDiagnostics)
		{
			item.Type.OriginalDefinition.AddUseSiteInfo(ref useSiteInfo);
		}
		return typeArgumentsWithAnnotationsNoUseSiteDiagnostics;
	}

	internal TypeWithAnnotations TypeArgumentWithDefinitionUseSiteDiagnostics(int index, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeWithAnnotations result = TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[index];
		result.Type.OriginalDefinition.AddUseSiteInfo(ref useSiteInfo);
		return result;
	}

	internal void SetKnownToHaveNoDeclaredBaseCycles()
	{
		_hasNoBaseCycles = true;
	}

	internal abstract bool GetGuidString(out string guidString);

	internal void AddOperators(string name, ArrayBuilder<MethodSymbol> operators)
	{
		ImmutableArray<Symbol> simpleNonTypeMembers = GetSimpleNonTypeMembers(name);
		if (!simpleNonTypeMembers.IsEmpty)
		{
			AddOperators(operators, simpleNonTypeMembers);
		}
	}

	internal static void AddOperators(ArrayBuilder<MethodSymbol> operators, ImmutableArray<Symbol> candidates)
	{
		foreach (Symbol item in candidates)
		{
			MethodSymbol methodSymbol = item as MethodSymbol;
			bool flag;
			if ((object)methodSymbol != null)
			{
				MethodKind methodKind = methodSymbol.MethodKind;
				if (methodKind == MethodKind.Conversion || methodKind == MethodKind.UserDefinedOperator)
				{
					flag = true;
					goto IL_0030;
				}
			}
			flag = false;
			goto IL_0030;
			IL_0030:
			if (flag)
			{
				operators.Add(methodSymbol);
			}
		}
	}

	internal static void AddOperators(ArrayBuilder<MethodSymbol> operators, ArrayBuilder<Symbol> candidates)
	{
		foreach (Symbol candidate in candidates)
		{
			MethodSymbol methodSymbol = candidate as MethodSymbol;
			bool flag;
			if ((object)methodSymbol != null)
			{
				MethodKind methodKind = methodSymbol.MethodKind;
				if (methodKind == MethodKind.Conversion || methodKind == MethodKind.UserDefinedOperator)
				{
					flag = true;
					goto IL_002f;
				}
			}
			flag = false;
			goto IL_002f;
			IL_002f:
			if (flag)
			{
				operators.Add(methodSymbol);
			}
		}
	}

	private ImmutableArray<MethodSymbol> GetConstructors(bool includeInstance, bool includeStatic)
	{
		ImmutableArray<Symbol> immutableArray = (includeInstance ? GetMembers(".ctor") : ImmutableArray<Symbol>.Empty);
		ImmutableArray<Symbol> immutableArray2 = (includeStatic ? GetMembers(".cctor") : ImmutableArray<Symbol>.Empty);
		if (immutableArray.IsEmpty && immutableArray2.IsEmpty)
		{
			return ImmutableArray<MethodSymbol>.Empty;
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		foreach (Symbol item3 in immutableArray)
		{
			if (item3 is MethodSymbol item)
			{
				instance.Add(item);
			}
		}
		foreach (Symbol item4 in immutableArray2)
		{
			if (item4 is MethodSymbol item2)
			{
				instance.Add(item2);
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal void GetExtensionMethods(ArrayBuilder<MethodSymbol> methods, string nameOpt, int arity, LookupOptions options)
	{
		if (MightContainExtensionMethods)
		{
			DoGetExtensionMethods(methods, nameOpt, arity, options);
		}
	}

	internal void DoGetExtensionMethods(ArrayBuilder<MethodSymbol> methods, string nameOpt, int arity, LookupOptions options)
	{
		foreach (Symbol item in (nameOpt == null) ? GetMembersUnordered() : GetSimpleNonTypeMembers(nameOpt))
		{
			if (item.Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)item;
				if (methodSymbol.IsExtensionMethod && ((options & LookupOptions.AllMethodsOnArityZero) != LookupOptions.Default || arity == methodSymbol.Arity) && IsValidExtensionReceiverParameter(methodSymbol.Parameters.First()))
				{
					methods.Add(methodSymbol);
				}
			}
		}
	}

	private static bool IsValidExtensionReceiverParameter(ParameterSymbol thisParam)
	{
		if (!thisParam.Type.IsValidExtensionParameterType())
		{
			return false;
		}
		if (thisParam.RefKind == RefKind.Ref && !thisParam.Type.IsValueType)
		{
			return false;
		}
		RefKind refKind = thisParam.RefKind;
		bool flag = refKind - 3 <= RefKind.Ref;
		if (flag && !thisParam.Type.IsValidInOrRefReadonlyExtensionParameterType())
		{
			return false;
		}
		return true;
	}

	internal void GetExtensionMembers(ArrayBuilder<Symbol> members, string? name, string? alternativeName, int arity, LookupOptions options, ConsList<FieldSymbol> fieldsBeingBound)
	{
		if (!this.IsClassType() || !IsStatic || IsGenericType || !MightContainExtensionMethods)
		{
			return;
		}
		foreach (NamedTypeSymbol typeMember in GetTypeMembers(""))
		{
			if ((object)typeMember == null || !typeMember.IsExtension)
			{
				continue;
			}
			ParameterSymbol extensionParameter = typeMember.ExtensionParameter;
			if ((object)extensionParameter == null || !IsValidExtensionReceiverParameter(extensionParameter))
			{
				continue;
			}
			foreach (Symbol item in (name == null || alternativeName != null) ? typeMember.GetMembersUnordered() : typeMember.GetMembers(name))
			{
				if (SourceMemberContainerTypeSymbol.IsAllowedExtensionMember(item) && extensionMemberMatches(item, name, alternativeName, arity, options, fieldsBeingBound))
				{
					members.Add(item);
				}
			}
		}
		static bool extensionMemberMatches(Symbol member, string? text, string? text2, int num, LookupOptions lookupOptions, ConsList<FieldSymbol> fieldsBeingBound2)
		{
			if ((lookupOptions & LookupOptions.MustBeInstance) != LookupOptions.Default && member.IsStatic)
			{
				return false;
			}
			if ((lookupOptions & LookupOptions.MustNotBeInstance) != LookupOptions.Default && !member.IsStatic)
			{
				return false;
			}
			if ((lookupOptions & LookupOptions.MustBeOperator) != LookupOptions.Default && !(member is MethodSymbol { MethodKind: MethodKind.UserDefinedOperator }))
			{
				return false;
			}
			if ((lookupOptions & LookupOptions.AllMethodsOnArityZero) == 0 && num != member.GetMemberArityIncludingExtension())
			{
				return false;
			}
			string name2 = member.Name;
			if (text != null && !(name2 == text) && (text2 == null || !(name2 == text2)))
			{
				return false;
			}
			if ((lookupOptions & LookupOptions.MustBeInvocableIfMember) != LookupOptions.Default && !Binder.IsInvocableMember(member, fieldsBeingBound2))
			{
				return false;
			}
			return true;
		}
	}

	public virtual MethodSymbol? TryGetCorrespondingExtensionImplementationMethod(MethodSymbol method)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NamedTypeSymbol.cs", 497);
	}

	internal override ManagedKind GetManagedKind(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return BaseTypeAnalysis.GetManagedKind(this, ref useSiteInfo);
	}

	internal abstract AttributeUsageInfo GetAttributeUsageInfo();

	internal SynthesizedInstanceConstructor GetScriptConstructor()
	{
		return (SynthesizedInstanceConstructor)InstanceConstructors.Single();
	}

	internal SynthesizedInteractiveInitializerMethod GetScriptInitializer()
	{
		return (SynthesizedInteractiveInitializerMethod)GetMembers("<Initialize>").Single();
	}

	internal SynthesizedEntryPointSymbol GetScriptEntryPoint()
	{
		string name = ((TypeKind == TypeKind.Submission) ? "<Factory>" : "<Main>");
		return (SynthesizedEntryPointSymbol)GetMembers(name).Single();
	}

	private void EnsureRequiredMembersCalculated()
	{
		if (_lazyRequiredMembers.IsDefault)
		{
			ImmutableSegmentedDictionary<string, Symbol> value = ((!tryCalculateRequiredMembers(out var requiredMembersBuilder)) ? RequiredMembersErrorSentinel : (requiredMembersBuilder?.ToImmutable() ?? BaseTypeNoUseSiteDiagnostics?.AllRequiredMembers ?? ImmutableSegmentedDictionary<string, Symbol>.Empty));
			RoslynImmutableInterlocked.InterlockedInitialize(ref _lazyRequiredMembers, value);
		}
		bool tryCalculateRequiredMembers(out ImmutableSegmentedDictionary<string, Symbol>.Builder? reference)
		{
			reference = null;
			NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
			if ((object)baseTypeNoUseSiteDiagnostics != null && baseTypeNoUseSiteDiagnostics.HasRequiredMembersError)
			{
				return false;
			}
			ImmutableSegmentedDictionary<string, Symbol> immutableSegmentedDictionary = BaseTypeNoUseSiteDiagnostics?.AllRequiredMembers ?? ImmutableSegmentedDictionary<string, Symbol>.Empty;
			bool hasDeclaredRequiredMembers = HasDeclaredRequiredMembers;
			foreach (Symbol item in GetMembersUnordered())
			{
				if (item is PropertySymbol { ParameterCount: >0 } propertySymbol)
				{
					if (propertySymbol.IsRequired)
					{
						return false;
					}
					continue;
				}
				if (immutableSegmentedDictionary.TryGetValue(item.Name, out var value2))
				{
					if (item.IsRequired())
					{
						Symbol overriddenMember = item.GetOverriddenMember();
						if ((object)overriddenMember != null && overriddenMember.Equals(value2, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.AllNullableIgnoreOptions))
						{
							goto IL_00ad;
						}
					}
					return false;
				}
				goto IL_00ad;
				IL_00ad:
				if (item.IsRequired())
				{
					if (!hasDeclaredRequiredMembers)
					{
						return false;
					}
					if (reference == null)
					{
						reference = immutableSegmentedDictionary.ToBuilder();
					}
					reference[item.Name] = item;
				}
			}
			return true;
		}
	}

	public abstract override ImmutableArray<Symbol> GetMembers();

	public abstract override ImmutableArray<Symbol> GetMembers(string name);

	internal abstract bool HasPossibleWellKnownCloneMethod();

	internal virtual ImmutableArray<Symbol> GetSimpleNonTypeMembers(string name)
	{
		return GetMembers(name);
	}

	public abstract override ImmutableArray<NamedTypeSymbol> GetTypeMembers();

	public abstract override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity);

	internal virtual IEnumerable<Symbol> GetInstanceFieldsAndEvents()
	{
		return GetMembersUnordered().Where(IsInstanceFieldOrEvent);
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNamedType(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitNamedType(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitNamedType(this);
	}

	internal abstract ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers();

	internal abstract ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name);

	internal abstract NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved);

	internal abstract ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved);

	public override int GetHashCode()
	{
		if (base.SpecialType == SpecialType.System_Object)
		{
			return 1;
		}
		return RuntimeHelpers.GetHashCode(OriginalDefinition);
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		if ((object)t2 == this)
		{
			return true;
		}
		if ((object)t2 == null)
		{
			return false;
		}
		if ((comparison & TypeCompareKind.IgnoreDynamic) != TypeCompareKind.ConsiderEverything && t2.TypeKind == TypeKind.Dynamic && base.SpecialType == SpecialType.System_Object)
		{
			return true;
		}
		if (!(t2 is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		NamedTypeSymbol originalDefinition = OriginalDefinition;
		NamedTypeSymbol originalDefinition2 = namedTypeSymbol.OriginalDefinition;
		bool flag = (object)this == originalDefinition;
		bool flag2 = (object)namedTypeSymbol == originalDefinition2;
		if (flag & flag2)
		{
			return false;
		}
		if ((flag | flag2) && (comparison & (TypeCompareKind.AllNullableIgnoreOptions | TypeCompareKind.AllIgnoreOptionsForVB)) == 0)
		{
			return false;
		}
		if (!TypeSymbol.Equals(originalDefinition, originalDefinition2, comparison))
		{
			return false;
		}
		return EqualsComplicatedCases(namedTypeSymbol, comparison);
	}

	private bool EqualsComplicatedCases(NamedTypeSymbol other, TypeCompareKind comparison)
	{
		if ((object)ContainingType != null && !ContainingType.Equals(other.ContainingType, comparison))
		{
			return false;
		}
		bool flag = (object)ConstructedFrom == this;
		bool flag2 = (object)other.ConstructedFrom == other;
		if (flag & flag2)
		{
			return true;
		}
		if (IsUnboundGenericType != other.IsUnboundGenericType)
		{
			return false;
		}
		if ((flag | flag2) && (comparison & (TypeCompareKind.AllNullableIgnoreOptions | TypeCompareKind.AllIgnoreOptionsForVB)) == 0)
		{
			return false;
		}
		ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
		ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics2 = other.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
		int length = typeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length;
		for (int i = 0; i < length; i++)
		{
			TypeWithAnnotations typeWithAnnotations = typeArgumentsWithAnnotationsNoUseSiteDiagnostics[i];
			TypeWithAnnotations other2 = typeArgumentsWithAnnotationsNoUseSiteDiagnostics2[i];
			if (!typeWithAnnotations.Equals(other2, comparison))
			{
				return false;
			}
		}
		if (IsTupleType && !tupleNamesEquals(other, comparison))
		{
			return false;
		}
		return true;
		bool tupleNamesEquals(NamedTypeSymbol namedTypeSymbol, TypeCompareKind typeCompareKind)
		{
			if ((typeCompareKind & TypeCompareKind.IgnoreTupleNames) == 0)
			{
				ImmutableArray<string> tupleElementNames = TupleElementNames;
				ImmutableArray<string> tupleElementNames2 = namedTypeSymbol.TupleElementNames;
				if (!tupleElementNames.IsDefault)
				{
					if (!tupleElementNames2.IsDefault)
					{
						return tupleElementNames.SequenceEqual(tupleElementNames2);
					}
					return false;
				}
				return tupleElementNames2.IsDefault;
			}
			return true;
		}
	}

	internal override void AddNullableTransforms(ArrayBuilder<byte> transforms)
	{
		ContainingType?.AddNullableTransforms(transforms);
		foreach (TypeWithAnnotations typeArgumentsWithAnnotationsNoUseSiteDiagnostic in TypeArgumentsWithAnnotationsNoUseSiteDiagnostics)
		{
			typeArgumentsWithAnnotationsNoUseSiteDiagnostic.AddNullableTransforms(transforms);
		}
	}

	internal override bool ApplyNullableTransforms(byte defaultTransformFlag, ImmutableArray<byte> transforms, ref int position, out TypeSymbol result)
	{
		if (!IsGenericType)
		{
			result = this;
			return true;
		}
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		GetAllTypeArgumentsNoUseSiteDiagnostics(instance);
		bool flag = false;
		for (int i = 0; i < instance.Count; i++)
		{
			TypeWithAnnotations typeWithAnnotations = instance[i];
			if (!typeWithAnnotations.ApplyNullableTransforms(defaultTransformFlag, transforms, ref position, out var result2))
			{
				instance.Free();
				result = this;
				return false;
			}
			if (!typeWithAnnotations.IsSameAs(result2))
			{
				instance[i] = result2;
				flag = true;
			}
		}
		result = (flag ? WithTypeArguments(instance.ToImmutable()) : this);
		instance.Free();
		return true;
	}

	internal override TypeSymbol SetNullabilityForReferenceTypes(Func<TypeWithAnnotations, TypeWithAnnotations> transform)
	{
		if (!IsGenericType)
		{
			return this;
		}
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		GetAllTypeArgumentsNoUseSiteDiagnostics(instance);
		bool flag = false;
		for (int i = 0; i < instance.Count; i++)
		{
			TypeWithAnnotations arg = instance[i];
			TypeWithAnnotations typeWithAnnotations = transform(arg);
			if (!arg.IsSameAs(typeWithAnnotations))
			{
				instance[i] = typeWithAnnotations;
				flag = true;
			}
		}
		NamedTypeSymbol result = (flag ? WithTypeArguments(instance.ToImmutable()) : this);
		instance.Free();
		return result;
	}

	internal NamedTypeSymbol WithTypeArguments(ImmutableArray<TypeWithAnnotations> allTypeArguments)
	{
		NamedTypeSymbol originalDefinition = OriginalDefinition;
		return new TypeMap(originalDefinition.GetAllTypeParameters(), allTypeArguments).SubstituteNamedType(originalDefinition).WithTupleDataFrom(this);
	}

	internal override TypeSymbol MergeEquivalentTypes(TypeSymbol other, VarianceKind variance)
	{
		if (!IsGenericType)
		{
			if (!other.IsDynamic())
			{
				return this;
			}
			return other;
		}
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		ArrayBuilder<TypeWithAnnotations> instance2 = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		NamedTypeSymbol namedTypeSymbol = ((!MergeEquivalentTypeArguments(this, (NamedTypeSymbol)other, variance, instance, instance2)) ? this : new TypeMap(instance.ToImmutable(), instance2.ToImmutable()).SubstituteNamedType(OriginalDefinition));
		instance2.Free();
		instance.Free();
		if (!IsTupleType)
		{
			return namedTypeSymbol;
		}
		return MergeTupleNames((NamedTypeSymbol)other, namedTypeSymbol);
	}

	private static bool MergeEquivalentTypeArguments(NamedTypeSymbol typeA, NamedTypeSymbol typeB, VarianceKind variance, ArrayBuilder<TypeParameterSymbol> allTypeParameters, ArrayBuilder<TypeWithAnnotations> allTypeArguments)
	{
		bool isTupleType = typeA.IsTupleType;
		NamedTypeSymbol namedTypeSymbol = typeA.OriginalDefinition;
		bool result = false;
		while (true)
		{
			ImmutableArray<TypeParameterSymbol> typeParameters = namedTypeSymbol.TypeParameters;
			if (typeParameters.Length > 0)
			{
				ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = typeA.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
				ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics2 = typeB.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
				allTypeParameters.AddRange(typeParameters);
				for (int i = 0; i < typeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length; i++)
				{
					TypeWithAnnotations typeWithAnnotations = typeArgumentsWithAnnotationsNoUseSiteDiagnostics[i];
					TypeWithAnnotations other = typeArgumentsWithAnnotationsNoUseSiteDiagnostics2[i];
					VarianceKind typeArgumentVariance = GetTypeArgumentVariance(variance, isTupleType ? VarianceKind.Out : typeParameters[i].Variance);
					TypeWithAnnotations typeWithAnnotations2 = typeWithAnnotations.MergeEquivalentTypes(other, typeArgumentVariance);
					allTypeArguments.Add(typeWithAnnotations2);
					if (!typeWithAnnotations.IsSameAs(typeWithAnnotations2))
					{
						result = true;
					}
				}
			}
			namedTypeSymbol = namedTypeSymbol.ContainingType;
			if ((object)namedTypeSymbol == null)
			{
				break;
			}
			typeA = typeA.ContainingType;
			typeB = typeB.ContainingType;
			variance = VarianceKind.None;
		}
		return result;
	}

	private static VarianceKind GetTypeArgumentVariance(VarianceKind typeVariance, VarianceKind typeParameterVariance)
	{
		return typeVariance switch
		{
			VarianceKind.In => typeParameterVariance switch
			{
				VarianceKind.In => VarianceKind.Out, 
				VarianceKind.Out => VarianceKind.In, 
				_ => VarianceKind.None, 
			}, 
			VarianceKind.Out => typeParameterVariance, 
			_ => VarianceKind.None, 
		};
	}

	public NamedTypeSymbol Construct(params TypeSymbol[] typeArguments)
	{
		return ConstructWithoutModifiers(typeArguments.AsImmutableOrNull(), unbound: false);
	}

	public NamedTypeSymbol Construct(ImmutableArray<TypeSymbol> typeArguments)
	{
		return ConstructWithoutModifiers(typeArguments, unbound: false);
	}

	public NamedTypeSymbol Construct(IEnumerable<TypeSymbol> typeArguments)
	{
		return ConstructWithoutModifiers(typeArguments.AsImmutableOrNull(), unbound: false);
	}

	public NamedTypeSymbol ConstructUnboundGenericType()
	{
		return OriginalDefinition.AsUnboundGenericType();
	}

	internal NamedTypeSymbol GetUnboundGenericTypeOrSelf()
	{
		if (!IsGenericType)
		{
			return this;
		}
		return ConstructUnboundGenericType();
	}

	internal abstract bool HasAsyncMethodBuilderAttribute(out TypeSymbol builderArgument);

	private NamedTypeSymbol ConstructWithoutModifiers(ImmutableArray<TypeSymbol> typeArguments, bool unbound)
	{
		ImmutableArray<TypeWithAnnotations> typeArguments2 = ((!typeArguments.IsDefault) ? typeArguments.SelectAsArray((TypeSymbol t) => TypeWithAnnotations.Create(t)) : default(ImmutableArray<TypeWithAnnotations>));
		return Construct(typeArguments2, unbound);
	}

	internal NamedTypeSymbol Construct(ImmutableArray<TypeWithAnnotations> typeArguments)
	{
		return Construct(typeArguments, unbound: false);
	}

	internal NamedTypeSymbol Construct(ImmutableArray<TypeWithAnnotations> typeArguments, bool unbound)
	{
		if ((object)this != ConstructedFrom)
		{
			throw new InvalidOperationException(CSharpResources.CannotCreateConstructedFromConstructed);
		}
		if (Arity == 0)
		{
			throw new InvalidOperationException(CSharpResources.CannotCreateConstructedFromNongeneric);
		}
		if (typeArguments.IsDefault)
		{
			throw new ArgumentNullException("typeArguments");
		}
		if (typeArguments.Any(TypeWithAnnotationsIsNullFunction))
		{
			throw new ArgumentException(CSharpResources.TypeArgumentCannotBeNull, "typeArguments");
		}
		if (typeArguments.Length != Arity)
		{
			throw new ArgumentException(CSharpResources.WrongNumberOfTypeArguments, "typeArguments");
		}
		if (ConstructedNamedTypeSymbol.TypeParametersMatchTypeArguments(TypeParameters, typeArguments))
		{
			return this;
		}
		return ConstructCore(typeArguments, unbound);
	}

	protected virtual NamedTypeSymbol ConstructCore(ImmutableArray<TypeWithAnnotations> typeArguments, bool unbound)
	{
		return new ConstructedNamedTypeSymbol(this, typeArguments, unbound);
	}

	internal void GetAllTypeArguments(ref TemporaryArray<TypeSymbol> builder, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ContainingType?.GetAllTypeArguments(ref builder, ref useSiteInfo);
		foreach (TypeWithAnnotations item in TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo))
		{
			builder.Add(item.Type);
		}
	}

	internal ImmutableArray<TypeWithAnnotations> GetAllTypeArguments(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		GetAllTypeArguments(instance, ref useSiteInfo);
		return instance.ToImmutableAndFree();
	}

	internal void GetAllTypeArguments(ArrayBuilder<TypeWithAnnotations> builder, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ContainingType?.GetAllTypeArguments(builder, ref useSiteInfo);
		builder.AddRange(TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo));
	}

	internal void GetAllTypeArgumentsNoUseSiteDiagnostics(ArrayBuilder<TypeWithAnnotations> builder)
	{
		ContainingType?.GetAllTypeArgumentsNoUseSiteDiagnostics(builder);
		builder.AddRange(TypeArgumentsWithAnnotationsNoUseSiteDiagnostics);
	}

	internal int AllTypeArgumentCount()
	{
		int num = TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length;
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType != null)
		{
			num += containingType.AllTypeArgumentCount();
		}
		return num;
	}

	internal ImmutableArray<TypeWithAnnotations> GetTypeParametersAsTypeArguments()
	{
		return TypeMap.TypeParametersAsTypeSymbolsWithAnnotations(TypeParameters);
	}

	internal virtual NamedTypeSymbol AsMember(NamedTypeSymbol newOwner)
	{
		if (!newOwner.IsDefinition)
		{
			return new SubstitutedNestedTypeSymbol((SubstitutedNamedTypeSymbol)newOwner, this);
		}
		return this;
	}

	internal override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		UseSiteInfo<AssemblySymbol> result = new UseSiteInfo<AssemblySymbol>(base.PrimaryDependency);
		if (base.IsDefinition)
		{
			return result;
		}
		if (!DeriveUseSiteInfoFromType(ref result, OriginalDefinition))
		{
			DeriveUseSiteDiagnosticFromTypeArguments(ref result);
		}
		return result;
	}

	private bool DeriveUseSiteDiagnosticFromTypeArguments(ref UseSiteInfo<AssemblySymbol> result)
	{
		NamedTypeSymbol namedTypeSymbol = this;
		do
		{
			foreach (TypeWithAnnotations typeArgumentsWithAnnotationsNoUseSiteDiagnostic in namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics)
			{
				if (DeriveUseSiteInfoFromType(ref result, typeArgumentsWithAnnotationsNoUseSiteDiagnostic, AllowedRequiredModifierType.None))
				{
					return true;
				}
			}
			namedTypeSymbol = namedTypeSymbol.ContainingType;
		}
		while ((object)namedTypeSymbol != null && !namedTypeSymbol.IsDefinition);
		return false;
	}

	internal DiagnosticInfo CalculateUseSiteDiagnostic()
	{
		DiagnosticInfo result = null;
		if (MergeUseSiteDiagnostics(ref result, DeriveUseSiteDiagnosticFromBase()))
		{
			return result;
		}
		if (ContainingModule.HasUnifiedReferences)
		{
			HashSet<TypeSymbol> checkedTypes = null;
			GetUnificationUseSiteDiagnosticRecursive(ref result, this, ref checkedTypes);
			return result;
		}
		return result;
	}

	private DiagnosticInfo DeriveUseSiteDiagnosticFromBase()
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			if (baseTypeNoUseSiteDiagnostics.IsErrorType() && baseTypeNoUseSiteDiagnostics is NoPiaIllegalGenericInstantiationSymbol)
			{
				return baseTypeNoUseSiteDiagnostics.GetUseSiteInfo().DiagnosticInfo;
			}
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		return null;
	}

	internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		if (!this.MarkCheckedIfNecessary(ref checkedTypes))
		{
			return false;
		}
		if (owner.ContainingModule.GetUnificationUseSiteDiagnostic(ref result, this))
		{
			return true;
		}
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null && baseTypeNoUseSiteDiagnostics.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes))
		{
			return true;
		}
		if (!Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, InterfacesNoUseSiteDiagnostics(), owner, ref checkedTypes))
		{
			return Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, TypeParameters, owner, ref checkedTypes);
		}
		return true;
	}

	internal abstract IEnumerable<SecurityAttribute> GetSecurityInformation();

	internal abstract ImmutableArray<string> GetAppliedConditionalSymbols();

	internal abstract bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName);

	internal bool IsTupleTypeOfCardinality(out int tupleCardinality)
	{
		if (!IsUnboundGenericType)
		{
			Symbol containingSymbol = ContainingSymbol;
			if ((object)containingSymbol != null && containingSymbol.Kind == SymbolKind.Namespace)
			{
				NamespaceSymbol containingNamespace = ContainingNamespace.ContainingNamespace;
				if ((object)containingNamespace != null && containingNamespace.IsGlobalNamespace && Name == "ValueTuple" && ContainingNamespace.Name == "System")
				{
					int arity = Arity;
					if (arity >= 0 && arity < 8)
					{
						tupleCardinality = arity;
						return true;
					}
					if (arity == 8 && !base.IsDefinition)
					{
						TypeSymbol typeSymbol = this;
						int num = 0;
						do
						{
							num++;
							typeSymbol = ((NamedTypeSymbol)typeSymbol).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[7].Type;
						}
						while (TypeSymbol.Equals(typeSymbol.OriginalDefinition, OriginalDefinition, TypeCompareKind.ConsiderEverything) && !typeSymbol.IsDefinition);
						arity = (typeSymbol as NamedTypeSymbol)?.Arity ?? 0;
						if (arity > 0 && arity < 8 && ((NamedTypeSymbol)typeSymbol).IsTupleTypeOfCardinality(out tupleCardinality))
						{
							tupleCardinality += 7 * num;
							return true;
						}
					}
				}
			}
		}
		tupleCardinality = 0;
		return false;
	}

	internal abstract NamedTypeSymbol AsNativeInteger();

	protected override ISymbol CreateISymbol()
	{
		return new NonErrorNamedTypeSymbol(this, base.DefaultNullableAnnotation);
	}

	protected override ITypeSymbol CreateITypeSymbol(Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation)
	{
		return new NonErrorNamedTypeSymbol(this, nullableAnnotation);
	}

	ImmutableArray<ISymbolInternal> INamedTypeSymbolInternal.GetMembers()
	{
		return GetMembers().CastArray<ISymbolInternal>();
	}

	ImmutableArray<ISymbolInternal> INamedTypeSymbolInternal.GetMembers(string name)
	{
		return GetMembers(name).CastArray<ISymbolInternal>();
	}

	internal static NamedTypeSymbol CreateTuple(Location? locationOpt, ImmutableArray<TypeWithAnnotations> elementTypesWithAnnotations, ImmutableArray<Location?> elementLocations, ImmutableArray<string?> elementNames, CSharpCompilation compilation, bool shouldCheckConstraints, bool includeNullability, ImmutableArray<bool> errorPositions, CSharpSyntaxNode? syntax = null, BindingDiagnosticBag? diagnostics = null)
	{
		if (elementTypesWithAnnotations.Length <= 1)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Tuples/TupleTypeSymbol.cs", 49);
		}
		NamedTypeSymbol namedTypeSymbol = getTupleUnderlyingType(elementTypesWithAnnotations, syntax, compilation, diagnostics);
		if (diagnostics != null && diagnostics.DiagnosticBag != null && ((SourceModuleSymbol)compilation.SourceModule).AnyReferencedAssembliesAreLinked)
		{
			EmbeddedTypesManager.IsValidEmbeddableType(namedTypeSymbol, syntax, diagnostics.DiagnosticBag);
		}
		ImmutableArray<Location> locations = (((object)locationOpt == null) ? ImmutableArray<Location>.Empty : ImmutableArray.Create(locationOpt));
		NamedTypeSymbol namedTypeSymbol2 = CreateTuple(namedTypeSymbol, elementNames, errorPositions, elementLocations, locations);
		if (shouldCheckConstraints && diagnostics != null)
		{
			namedTypeSymbol2.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(compilation, compilation.Conversions, includeNullability, syntax.Location, diagnostics), syntax, elementLocations, includeNullability ? diagnostics : null);
		}
		return namedTypeSymbol2;
		static NamedTypeSymbol getTupleUnderlyingType(ImmutableArray<TypeWithAnnotations> elementTypes, CSharpSyntaxNode? cSharpSyntaxNode, CSharpCompilation cSharpCompilation, BindingDiagnosticBag? bindingDiagnosticBag)
		{
			int num = NumberOfValueTuples(elementTypes.Length, out var remainder);
			NamedTypeSymbol wellKnownType = cSharpCompilation.GetWellKnownType(GetTupleType(remainder));
			if (bindingDiagnosticBag != null && cSharpSyntaxNode != null)
			{
				ReportUseSiteAndObsoleteDiagnostics(cSharpSyntaxNode, bindingDiagnosticBag, wellKnownType);
			}
			NamedTypeSymbol namedTypeSymbol3 = null;
			if (num > 1)
			{
				namedTypeSymbol3 = cSharpCompilation.GetWellKnownType(GetTupleType(8));
				if (bindingDiagnosticBag != null && cSharpSyntaxNode != null)
				{
					ReportUseSiteAndObsoleteDiagnostics(cSharpSyntaxNode, bindingDiagnosticBag, namedTypeSymbol3);
				}
			}
			return ConstructTupleUnderlyingType(wellKnownType, namedTypeSymbol3, elementTypes);
		}
	}

	public static NamedTypeSymbol CreateTuple(NamedTypeSymbol tupleCompatibleType, ImmutableArray<string?> elementNames = default(ImmutableArray<string?>), ImmutableArray<bool> errorPositions = default(ImmutableArray<bool>), ImmutableArray<Location?> elementLocations = default(ImmutableArray<Location?>), ImmutableArray<Location> locations = default(ImmutableArray<Location>))
	{
		return tupleCompatibleType.WithElementNames(elementNames, elementLocations, errorPositions, locations);
	}

	internal NamedTypeSymbol WithTupleDataFrom(NamedTypeSymbol original)
	{
		if (!IsTupleType || (original._lazyTupleData == null && _lazyTupleData == null) || TupleData.EqualsIgnoringTupleUnderlyingType(original.TupleData))
		{
			return this;
		}
		return WithElementNames(original.TupleElementNames, original.TupleElementLocations, original.TupleErrorPositions, original.Locations);
	}

	internal NamedTypeSymbol WithElementTypes(ImmutableArray<TypeWithAnnotations> newElementTypes)
	{
		NamedTypeSymbol originalDefinition;
		NamedTypeSymbol chainedTupleTypeOpt;
		if (Arity < 8)
		{
			originalDefinition = OriginalDefinition;
			chainedTupleTypeOpt = null;
		}
		else
		{
			chainedTupleTypeOpt = OriginalDefinition;
			NamedTypeSymbol namedTypeSymbol = this;
			do
			{
				namedTypeSymbol = (NamedTypeSymbol)namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[7].Type;
			}
			while (namedTypeSymbol.Arity >= 8);
			originalDefinition = namedTypeSymbol.OriginalDefinition;
		}
		return CreateTuple(ConstructTupleUnderlyingType(originalDefinition, chainedTupleTypeOpt, newElementTypes), TupleElementNames, elementLocations: TupleElementLocations, errorPositions: TupleErrorPositions, locations: Locations);
	}

	internal NamedTypeSymbol WithElementNames(ImmutableArray<string?> newElementNames, ImmutableArray<Location?> newElementLocations, ImmutableArray<bool> errorPositions, ImmutableArray<Location> locations)
	{
		return WithTupleData(new TupleExtraData(TupleUnderlyingType, newElementNames, newElementLocations, errorPositions, locations));
	}

	private NamedTypeSymbol WithTupleData(TupleExtraData newData)
	{
		if (newData.EqualsIgnoringTupleUnderlyingType(TupleData))
		{
			return this;
		}
		if (base.IsDefinition)
		{
			if (newData.ElementNames.IsDefault)
			{
				return this;
			}
			return ConstructCore(GetTypeParametersAsTypeArguments(), unbound: false).WithTupleData(newData);
		}
		return WithTupleDataCore(newData);
	}

	protected abstract NamedTypeSymbol WithTupleDataCore(TupleExtraData newData);

	internal static void GetUnderlyingTypeChain(NamedTypeSymbol underlyingTupleType, ArrayBuilder<NamedTypeSymbol> underlyingTupleTypeChain)
	{
		NamedTypeSymbol namedTypeSymbol = underlyingTupleType;
		while (true)
		{
			underlyingTupleTypeChain.Add(namedTypeSymbol);
			if (namedTypeSymbol.Arity == 8)
			{
				namedTypeSymbol = (NamedTypeSymbol)namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[7].Type;
				continue;
			}
			break;
		}
	}

	private static int NumberOfValueTuples(int numElements, out int remainder)
	{
		remainder = (numElements - 1) % 7 + 1;
		return (numElements - 1) / 7 + 1;
	}

	private static NamedTypeSymbol ConstructTupleUnderlyingType(NamedTypeSymbol firstTupleType, NamedTypeSymbol? chainedTupleTypeOpt, ImmutableArray<TypeWithAnnotations> elementTypes)
	{
		int num = NumberOfValueTuples(elementTypes.Length, out var remainder);
		NamedTypeSymbol namedTypeSymbol = firstTupleType.Construct(ImmutableArray.Create(elementTypes, (num - 1) * 7, remainder));
		for (int num2 = num - 1; num2 > 0; num2--)
		{
			ImmutableArray<TypeWithAnnotations> typeArguments = ImmutableArray.Create(elementTypes, (num2 - 1) * 7, 7).Add(TypeWithAnnotations.Create(namedTypeSymbol));
			namedTypeSymbol = chainedTupleTypeOpt.Construct(typeArguments);
		}
		return namedTypeSymbol;
	}

	private static void ReportUseSiteAndObsoleteDiagnostics(CSharpSyntaxNode? syntax, BindingDiagnosticBag diagnostics, NamedTypeSymbol firstTupleType)
	{
		Binder.ReportUseSite(firstTupleType, diagnostics, syntax);
		Binder.ReportDiagnosticsIfObsoleteInternal(diagnostics, firstTupleType, syntax, firstTupleType.ContainingType, BinderFlags.None);
	}

	internal static void VerifyTupleTypePresent(int cardinality, CSharpSyntaxNode? syntax, CSharpCompilation compilation, BindingDiagnosticBag diagnostics)
	{
		int num = NumberOfValueTuples(cardinality, out var remainder);
		NamedTypeSymbol wellKnownType = compilation.GetWellKnownType(GetTupleType(remainder));
		ReportUseSiteAndObsoleteDiagnostics(syntax, diagnostics, wellKnownType);
		if (num > 1)
		{
			NamedTypeSymbol wellKnownType2 = compilation.GetWellKnownType(GetTupleType(8));
			ReportUseSiteAndObsoleteDiagnostics(syntax, diagnostics, wellKnownType2);
		}
	}

	internal static void ReportTupleNamesMismatchesIfAny(TypeSymbol destination, BoundTupleLiteral literal, BindingDiagnosticBag diagnostics)
	{
		ImmutableArray<string> argumentNamesOpt = literal.ArgumentNamesOpt;
		if (argumentNamesOpt.IsDefault)
		{
			return;
		}
		ImmutableArray<bool> inferredNamesOpt = literal.InferredNamesOpt;
		bool isDefault = inferredNamesOpt.IsDefault;
		ImmutableArray<string> tupleElementNames = destination.TupleElementNames;
		int length = argumentNamesOpt.Length;
		bool isDefault2 = tupleElementNames.IsDefault;
		for (int i = 0; i < length; i++)
		{
			string text = argumentNamesOpt[i];
			bool flag = !isDefault && inferredNamesOpt[i];
			if (text != null && !flag && (isDefault2 || string.CompareOrdinal(tupleElementNames[i], text) != 0))
			{
				diagnostics.Add(ErrorCode.WRN_TupleLiteralNameMismatch, literal.Arguments[i].Syntax.Parent.Location, text, destination);
			}
		}
	}

	private static WellKnownType GetTupleType(int arity)
	{
		if (arity > 8)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Tuples/TupleTypeSymbol.cs", 313);
		}
		return tupleTypes[arity - 1];
	}

	internal static WellKnownMember GetTupleCtor(int arity)
	{
		if (arity > 8)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Tuples/TupleTypeSymbol.cs", 341);
		}
		return tupleCtors[arity - 1];
	}

	internal static WellKnownMember GetTupleTypeMember(int arity, int position)
	{
		return tupleMembers[arity - 1][position - 1];
	}

	internal static string TupleMemberName(int position)
	{
		return "Item" + position;
	}

	internal static int IsTupleElementNameReserved(string name)
	{
		if (isElementNameForbidden(name))
		{
			return 0;
		}
		return MatchesCanonicalTupleElementName(name);
		static bool isElementNameForbidden(string text)
		{
			switch (text)
			{
			case "CompareTo":
			case "Deconstruct":
			case "Equals":
			case "GetHashCode":
			case "Rest":
			case "ToString":
				return true;
			default:
				return false;
			}
		}
	}

	internal static int MatchesCanonicalTupleElementName(string name)
	{
		if (name.StartsWith("Item", StringComparison.Ordinal) && int.TryParse(name.Substring("Item".Length), out var result) && result > 0 && string.Equals(name, TupleMemberName(result), StringComparison.Ordinal))
		{
			return result;
		}
		return -1;
	}

	internal static Symbol? GetWellKnownMemberInType(NamedTypeSymbol type, WellKnownMember relativeMember, BindingDiagnosticBag diagnostics, SyntaxNode? syntax)
	{
		Symbol symbol = GetWellKnownMemberInType(type, relativeMember);
		if ((object)symbol == null)
		{
			MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(relativeMember);
			Binder.Error(diagnostics, ErrorCode.ERR_PredefinedTypeMemberNotFoundInAssembly, syntax, descriptor.Name, type, type.ContainingAssembly);
		}
		else
		{
			UseSiteInfo<AssemblySymbol> info = symbol.GetUseSiteInfo();
			DiagnosticInfo? diagnosticInfo = info.DiagnosticInfo;
			if (diagnosticInfo == null || diagnosticInfo.Severity != DiagnosticSeverity.Error)
			{
				info = info.AdjustDiagnosticInfo(null);
			}
			diagnostics.Add(info, (SyntaxNode syntaxNode) => syntaxNode?.Location ?? Location.None, syntax);
		}
		return symbol;
		static Symbol? GetWellKnownMemberInType(NamedTypeSymbol namedTypeSymbol, WellKnownMember member)
		{
			MemberDescriptor descriptor2 = WellKnownMembers.GetDescriptor(member);
			return CSharpCompilation.GetRuntimeMember(namedTypeSymbol.GetMembers(descriptor2.Name), in descriptor2, CSharpCompilation.SpecialMembersSignatureComparer.Instance, null);
		}
	}

	public TMember? GetTupleMemberSymbolForUnderlyingMember<TMember>(TMember? underlyingMemberOpt) where TMember : Symbol
	{
		if (!IsTupleType)
		{
			return null;
		}
		return TupleData.GetTupleMemberSymbolForUnderlyingMember(underlyingMemberOpt);
	}

	protected ArrayBuilder<Symbol> MakeSynthesizedTupleMembers(ImmutableArray<Symbol> currentMembers, HashSet<Symbol>? replacedFields = null)
	{
		ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = TupleElementTypesWithAnnotations;
		ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance(tupleElementTypesWithAnnotations.Length, fillWithValue: false);
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance(currentMembers.Length);
		NamedTypeSymbol namedTypeSymbol = this;
		int num = 0;
		ArrayBuilder<FieldSymbol> instance3 = ArrayBuilder<FieldSymbol>.GetInstance(namedTypeSymbol.Arity);
		collectTargetTupleFields(namedTypeSymbol.Arity, getOriginalFields(currentMembers), instance3);
		ImmutableArray<string> tupleElementNames = TupleElementNames;
		ImmutableArray<Location> elementLocations = TupleData.ElementLocations;
		while (true)
		{
			foreach (Symbol item in currentMembers)
			{
				switch (item.Kind)
				{
				case SymbolKind.Field:
				{
					FieldSymbol fieldSymbol = (FieldSymbol)item;
					if (fieldSymbol is TupleVirtualElementFieldSymbol)
					{
						replacedFields?.Add(fieldSymbol);
						break;
					}
					FieldSymbol fieldSymbol2 = ((fieldSymbol is TupleElementFieldSymbol tupleElementFieldSymbol) ? tupleElementFieldSymbol.UnderlyingField.OriginalDefinition : fieldSymbol.OriginalDefinition);
					int num2 = instance3.IndexOf(fieldSymbol2, ReferenceEqualityComparer.Instance);
					if (fieldSymbol2 is TupleErrorFieldSymbol)
					{
						replacedFields?.Add(fieldSymbol);
					}
					else if (num2 >= 0)
					{
						if (num != 0)
						{
							num2 += 7 * num;
						}
						else
						{
							replacedFields?.Add(fieldSymbol);
						}
						string text = (tupleElementNames.IsDefault ? null : tupleElementNames[num2]);
						ImmutableArray<Location> locations = getElementLocations(in elementLocations, num2);
						string text2 = TupleMemberName(num2 + 1);
						bool flag = text != text2;
						FieldSymbol underlyingField = fieldSymbol2.AsMember(namedTypeSymbol);
						FieldSymbol fieldSymbol3;
						if (num != 0)
						{
							fieldSymbol3 = new TupleVirtualElementFieldSymbol(this, underlyingField, text2, num2, locations, cannotUse: false, flag, null);
							instance2.Add(fieldSymbol3);
						}
						else if (base.IsDefinition)
						{
							fieldSymbol3 = fieldSymbol;
						}
						else
						{
							fieldSymbol3 = new TupleElementFieldSymbol(this, underlyingField, num2, locations, flag);
							instance2.Add(fieldSymbol3);
						}
						if (flag && !string.IsNullOrEmpty(text))
						{
							ImmutableArray<bool> tupleErrorPositions = TupleErrorPositions;
							bool cannotUse = !tupleErrorPositions.IsDefault && tupleErrorPositions[num2];
							instance2.Add(new TupleVirtualElementFieldSymbol(this, underlyingField, text, num2, locations, cannotUse, isImplicitlyDeclared: false, fieldSymbol3));
						}
						instance[num2] = true;
					}
					break;
				}
				default:
					if (num == 0)
					{
						throw ExceptionUtilities.UnexpectedValue(item.Kind);
					}
					break;
				case SymbolKind.Event:
				case SymbolKind.Method:
				case SymbolKind.NamedType:
				case SymbolKind.Property:
					break;
				}
			}
			if (namedTypeSymbol.Arity != 8)
			{
				break;
			}
			namedTypeSymbol = (NamedTypeSymbol)namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[7].Type;
			num++;
			if (namedTypeSymbol.Arity != 8)
			{
				currentMembers = namedTypeSymbol.GetMembers();
				instance3.Clear();
				collectTargetTupleFields(namedTypeSymbol.Arity, getOriginalFields(currentMembers), instance3);
			}
		}
		instance3.Free();
		for (int i = 0; i < instance.Count; i++)
		{
			if (!instance[i])
			{
				int num3 = NumberOfValueTuples(i + 1, out var remainder);
				NamedTypeSymbol originalDefinition = getNestedTupleUnderlyingType(this, num3 - 1).OriginalDefinition;
				CSDiagnosticInfo useSiteDiagnosticInfo = (originalDefinition.IsErrorType() ? null : new CSDiagnosticInfo(ErrorCode.ERR_PredefinedTypeMemberNotFoundInAssembly, TupleMemberName(remainder), originalDefinition, originalDefinition.ContainingAssembly));
				string text3 = (tupleElementNames.IsDefault ? null : tupleElementNames[i]);
				Location location = (elementLocations.IsDefault ? null : elementLocations[i]);
				string text4 = TupleMemberName(i + 1);
				bool flag2 = text3 != text4;
				TupleErrorFieldSymbol tupleErrorFieldSymbol = new TupleErrorFieldSymbol(this, text4, i, flag2 ? null : location, tupleElementTypesWithAnnotations[i], useSiteDiagnosticInfo, flag2, null);
				instance2.Add(tupleErrorFieldSymbol);
				if (flag2 && !string.IsNullOrEmpty(text3))
				{
					instance2.Add(new TupleErrorFieldSymbol(this, text3, i, location, tupleElementTypesWithAnnotations[i], useSiteDiagnosticInfo, isImplicitlyDeclared: false, tupleErrorFieldSymbol));
				}
			}
		}
		instance.Free();
		return instance2;
		static void collectTargetTupleFields(int arity, ImmutableArray<Symbol> members, ArrayBuilder<FieldSymbol?> fieldsForElements)
		{
			int num4 = Math.Min(arity, 7);
			for (int j = 0; j < num4; j++)
			{
				WellKnownMember tupleTypeMember = GetTupleTypeMember(arity, j + 1);
				fieldsForElements.Add((FieldSymbol)getWellKnownMemberInType(members, tupleTypeMember));
			}
		}
		static ImmutableArray<Location> getElementLocations(in ImmutableArray<Location?> reference, int tupleFieldIndex)
		{
			if (reference.IsDefault)
			{
				return ImmutableArray<Location>.Empty;
			}
			Location location2 = reference[tupleFieldIndex];
			if (!(location2 == null))
			{
				return ImmutableArray.Create(location2);
			}
			return ImmutableArray<Location>.Empty;
		}
		static NamedTypeSymbol getNestedTupleUnderlyingType(NamedTypeSymbol topLevelUnderlyingType, int depth)
		{
			NamedTypeSymbol namedTypeSymbol2 = topLevelUnderlyingType;
			for (int j = 0; j < depth; j++)
			{
				namedTypeSymbol2 = (NamedTypeSymbol)namedTypeSymbol2.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[7].Type;
			}
			return namedTypeSymbol2;
		}
		static ImmutableArray<Symbol> getOriginalFields(ImmutableArray<Symbol> members)
		{
			ArrayBuilder<Symbol> instance4 = ArrayBuilder<Symbol>.GetInstance();
			foreach (Symbol item2 in members)
			{
				if (!(item2 is TupleVirtualElementFieldSymbol))
				{
					if (item2 is TupleElementFieldSymbol tupleElementFieldSymbol2)
					{
						instance4.Add(tupleElementFieldSymbol2.UnderlyingField.OriginalDefinition);
					}
					else if (item2 is FieldSymbol fieldSymbol4)
					{
						instance4.Add(fieldSymbol4.OriginalDefinition);
					}
				}
			}
			return instance4.ToImmutableAndFree();
		}
		static Symbol? getWellKnownMemberInType(ImmutableArray<Symbol> members, WellKnownMember relativeMember)
		{
			return CSharpCompilation.GetRuntimeMember(members, WellKnownMembers.GetDescriptor(relativeMember), CSharpCompilation.SpecialMembersSignatureComparer.Instance, null);
		}
	}

	private TypeSymbol MergeTupleNames(NamedTypeSymbol other, NamedTypeSymbol mergedType)
	{
		ImmutableArray<string> tupleElementNames = TupleElementNames;
		ImmutableArray<string> tupleElementNames2 = other.TupleElementNames;
		ImmutableArray<string> immutableArray;
		if (tupleElementNames.IsDefault || tupleElementNames2.IsDefault)
		{
			immutableArray = default(ImmutableArray<string>);
		}
		else
		{
			immutableArray = tupleElementNames.ZipAsArray(tupleElementNames2, (string n1, string n2) => (string.CompareOrdinal(n1, n2) != 0) ? null : n1);
			if (immutableArray.All((string n) => n == null))
			{
				immutableArray = default(ImmutableArray<string>);
			}
		}
		if (!(immutableArray.IsDefault ? TupleElementNames.IsDefault : immutableArray.SequenceEqual(TupleElementNames)) || !Equals(mergedType, TypeCompareKind.ConsiderEverything))
		{
			return CreateTuple(mergedType, immutableArray, TupleErrorPositions, TupleElementLocations, Locations);
		}
		return this;
	}
}
