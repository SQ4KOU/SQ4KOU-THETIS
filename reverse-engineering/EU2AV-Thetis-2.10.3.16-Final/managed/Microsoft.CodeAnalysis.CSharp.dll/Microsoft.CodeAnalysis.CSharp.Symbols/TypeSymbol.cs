using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class TypeSymbol : NamespaceOrTypeSymbol, ITypeSymbolInternal, INamespaceOrTypeSymbolInternal, ISymbolInternal
{
	private class InterfaceInfo
	{
		internal ImmutableArray<NamedTypeSymbol> allInterfaces;

		internal MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> interfacesAndTheirBaseInterfaces;

		internal static readonly MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> EmptyInterfacesAndTheirBaseInterfaces = new MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>(0, SymbolEqualityComparer.CLRSignature);

		private ConcurrentDictionary<Symbol, SymbolAndDiagnostics> _implementationForInterfaceMemberMap;

		internal MultiDictionary<Symbol, Symbol> explicitInterfaceImplementationMap;

		internal ImmutableDictionary<MethodSymbol, MethodSymbol>? synthesizedMethodImplMap;

		public ConcurrentDictionary<Symbol, SymbolAndDiagnostics> ImplementationForInterfaceMemberMap
		{
			get
			{
				ConcurrentDictionary<Symbol, SymbolAndDiagnostics> implementationForInterfaceMemberMap = _implementationForInterfaceMemberMap;
				if (implementationForInterfaceMemberMap != null)
				{
					return implementationForInterfaceMemberMap;
				}
				implementationForInterfaceMemberMap = new ConcurrentDictionary<Symbol, SymbolAndDiagnostics>(1, 1, SymbolEqualityComparer.ConsiderEverything);
				return Interlocked.CompareExchange(ref _implementationForInterfaceMemberMap, implementationForInterfaceMemberMap, null) ?? implementationForInterfaceMemberMap;
			}
		}

		internal bool IsDefaultValue()
		{
			if (allInterfaces.IsDefault && interfacesAndTheirBaseInterfaces == null && _implementationForInterfaceMemberMap == null && explicitInterfaceImplementationMap == null)
			{
				return synthesizedMethodImplMap == null;
			}
			return false;
		}
	}

	protected class ExplicitInterfaceImplementationTargetMemberEqualityComparer : IEqualityComparer<Symbol>
	{
		public static readonly ExplicitInterfaceImplementationTargetMemberEqualityComparer Instance = new ExplicitInterfaceImplementationTargetMemberEqualityComparer();

		private ExplicitInterfaceImplementationTargetMemberEqualityComparer()
		{
		}

		public bool Equals(Symbol x, Symbol y)
		{
			if (x.OriginalDefinition == y.OriginalDefinition)
			{
				return x.ContainingType.Equals(y.ContainingType, TypeCompareKind.CLRSignatureCompareOptions);
			}
			return false;
		}

		public int GetHashCode(Symbol obj)
		{
			return obj.OriginalDefinition.GetHashCode();
		}
	}

	internal class SymbolAndDiagnostics
	{
		public static readonly SymbolAndDiagnostics Empty = new SymbolAndDiagnostics(null, ReadOnlyBindingDiagnostic<AssemblySymbol>.Empty);

		public readonly Symbol Symbol;

		public readonly ReadOnlyBindingDiagnostic<AssemblySymbol> Diagnostics;

		public SymbolAndDiagnostics(Symbol symbol, ReadOnlyBindingDiagnostic<AssemblySymbol> diagnostics)
		{
			Symbol = symbol;
			Diagnostics = diagnostics;
		}
	}

	internal const string ImplicitTypeName = "<invalid-global-code>";

	private static readonly InterfaceInfo s_noInterfaces = new InterfaceInfo();

	private ImmutableHashSet<Symbol> _lazyAbstractMembers;

	private InterfaceInfo _lazyInterfaceInfo;

	private static readonly Func<TypeWithAnnotations, TypeWithAnnotations> s_setUnknownNullability = (TypeWithAnnotations type) => type.SetUnknownNullabilityForReferenceTypes();

	public new TypeSymbol OriginalDefinition => OriginalTypeSymbolDefinition;

	protected virtual TypeSymbol OriginalTypeSymbolDefinition => this;

	protected sealed override Symbol OriginalSymbolDefinition => OriginalTypeSymbolDefinition;

	internal abstract NamedTypeSymbol BaseTypeNoUseSiteDiagnostics { get; }

	internal ImmutableArray<NamedTypeSymbol> AllInterfacesNoUseSiteDiagnostics => GetAllInterfaces();

	internal TypeSymbol EffectiveTypeNoUseSiteDiagnostics
	{
		get
		{
			if (!this.IsTypeParameter())
			{
				return this;
			}
			return ((TypeParameterSymbol)this).EffectiveBaseClassNoUseSiteDiagnostics;
		}
	}

	internal MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics
	{
		get
		{
			InterfaceInfo interfaceInfo = GetInterfaceInfo();
			if (interfaceInfo == s_noInterfaces)
			{
				return InterfaceInfo.EmptyInterfacesAndTheirBaseInterfaces;
			}
			if (interfaceInfo.interfacesAndTheirBaseInterfaces == null)
			{
				Interlocked.CompareExchange(ref interfaceInfo.interfacesAndTheirBaseInterfaces, MakeInterfacesAndTheirBaseInterfaces(InterfacesNoUseSiteDiagnostics()), null);
			}
			return interfaceInfo.interfacesAndTheirBaseInterfaces;
		}
	}

	public abstract bool IsReferenceType { get; }

	public abstract bool IsValueType { get; }

	public abstract TypeKind TypeKind { get; }

	public virtual ExtendedSpecialType ExtendedSpecialType => default(ExtendedSpecialType);

	public SpecialType SpecialType => (SpecialType)ExtendedSpecialType;

	internal PrimitiveTypeCode PrimitiveTypeCode => TypeKind switch
	{
		TypeKind.Pointer => PrimitiveTypeCode.Pointer, 
		TypeKind.FunctionPointer => PrimitiveTypeCode.FunctionPointer, 
		_ => SpecialTypes.GetTypeCode(SpecialType), 
	};

	public override bool HasUnsupportedMetadata
	{
		get
		{
			DiagnosticInfo diagnosticInfo = GetUseSiteInfo().DiagnosticInfo;
			bool flag = diagnosticInfo != null;
			if (flag)
			{
				int code = diagnosticInfo.Code;
				bool flag2 = ((code == 648 || code == 9041) ? true : false);
				flag = flag2;
			}
			return flag;
		}
	}

	public virtual bool IsAnonymousType => false;

	public virtual bool IsTupleType => false;

	internal virtual bool IsNativeIntegerWrapperType => false;

	internal bool IsNativeIntegerType
	{
		get
		{
			bool flag = IsNativeIntegerWrapperType;
			if (!flag)
			{
				SpecialType specialType = SpecialType;
				bool flag2 = (uint)(specialType - 21) <= 1u;
				flag = flag2 && ContainingAssembly.RuntimeSupportsNumericIntPtr;
			}
			return flag;
		}
	}

	public virtual ImmutableArray<TypeWithAnnotations> TupleElementTypesWithAnnotations => default(ImmutableArray<TypeWithAnnotations>);

	public virtual ImmutableArray<string> TupleElementNames => default(ImmutableArray<string>);

	public virtual ImmutableArray<FieldSymbol> TupleElements => default(ImmutableArray<FieldSymbol>);

	internal bool IsManagedTypeNoUseSiteDiagnostics
	{
		get
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			return IsManagedType(ref useSiteInfo);
		}
	}

	internal ManagedKind ManagedKindNoUseSiteDiagnostics
	{
		get
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			return GetManagedKind(ref useSiteInfo);
		}
	}

	public abstract bool IsRefLikeType { get; }

	public abstract bool IsReadOnly { get; }

	internal ImmutableHashSet<Symbol> AbstractMembers
	{
		get
		{
			if (_lazyAbstractMembers == null)
			{
				Interlocked.CompareExchange(ref _lazyAbstractMembers, ComputeAbstractMembers(), null);
			}
			return _lazyAbstractMembers;
		}
	}

	internal Microsoft.CodeAnalysis.NullableAnnotation DefaultNullableAnnotation => NullableAnnotationExtensions.ToPublicAnnotation(this, NullableAnnotation.Oblivious);

	TypeKind ITypeSymbolInternal.TypeKind => TypeKind;

	SpecialType ITypeSymbolInternal.SpecialType => SpecialType;

	ExtendedSpecialType ITypeSymbolInternal.ExtendedSpecialType => ExtendedSpecialType;

	bool ITypeSymbolInternal.IsReferenceType => IsReferenceType;

	bool ITypeSymbolInternal.IsValueType => IsValueType;

	internal abstract bool IsRecord { get; }

	internal abstract bool IsRecordStruct { get; }

	private InterfaceInfo GetInterfaceInfo()
	{
		InterfaceInfo lazyInterfaceInfo = _lazyInterfaceInfo;
		if (lazyInterfaceInfo != null)
		{
			return lazyInterfaceInfo;
		}
		TypeSymbol typeSymbol = this;
		while ((object)typeSymbol != null)
		{
			if (!((typeSymbol.TypeKind == TypeKind.TypeParameter) ? ((TypeParameterSymbol)typeSymbol).EffectiveInterfacesNoUseSiteDiagnostics : typeSymbol.InterfacesNoUseSiteDiagnostics()).IsEmpty)
			{
				lazyInterfaceInfo = new InterfaceInfo();
				return Interlocked.CompareExchange(ref _lazyInterfaceInfo, lazyInterfaceInfo, null) ?? lazyInterfaceInfo;
			}
			typeSymbol = typeSymbol.BaseTypeNoUseSiteDiagnostics;
		}
		return _lazyInterfaceInfo = s_noInterfaces;
	}

	internal NamedTypeSymbol BaseTypeWithDefinitionUseSiteDiagnostics(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		baseTypeNoUseSiteDiagnostics?.OriginalDefinition.AddUseSiteInfo(ref useSiteInfo);
		return baseTypeNoUseSiteDiagnostics;
	}

	internal NamedTypeSymbol BaseTypeOriginalDefinition(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol namedTypeSymbol = BaseTypeNoUseSiteDiagnostics;
		if ((object)namedTypeSymbol != null)
		{
			namedTypeSymbol = namedTypeSymbol.OriginalDefinition;
			namedTypeSymbol.AddUseSiteInfo(ref useSiteInfo);
		}
		return namedTypeSymbol;
	}

	internal abstract ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved = null);

	internal ImmutableArray<NamedTypeSymbol> AllInterfacesWithDefinitionUseSiteDiagnostics(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ImmutableArray<NamedTypeSymbol> allInterfacesNoUseSiteDiagnostics = AllInterfacesNoUseSiteDiagnostics;
		TypeSymbol typeSymbol = this;
		do
		{
			typeSymbol = typeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		while ((object)typeSymbol != null);
		foreach (NamedTypeSymbol item in allInterfacesNoUseSiteDiagnostics)
		{
			item.OriginalDefinition.AddUseSiteInfo(ref useSiteInfo);
		}
		return allInterfacesNoUseSiteDiagnostics;
	}

	internal TypeSymbol EffectiveType(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!this.IsTypeParameter())
		{
			return this;
		}
		return ((TypeParameterSymbol)this).EffectiveBaseClass(ref useSiteInfo);
	}

	internal bool IsDerivedFrom(TypeSymbol type, TypeCompareKind comparison, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((object)this == type)
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		while ((object)namedTypeSymbol != null)
		{
			if (type.Equals(namedTypeSymbol, comparison))
			{
				return true;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		return false;
	}

	internal bool IsEqualToOrDerivedFrom(TypeSymbol type, TypeCompareKind comparison, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!Equals(type, comparison))
		{
			return IsDerivedFrom(type, comparison, ref useSiteInfo);
		}
		return true;
	}

	internal virtual bool Equals(TypeSymbol t2, TypeCompareKind compareKind)
	{
		return (object)this == t2;
	}

	public sealed override bool Equals(Symbol other, TypeCompareKind compareKind)
	{
		if (!(other is TypeSymbol t))
		{
			return false;
		}
		return Equals(t, compareKind);
	}

	public override int GetHashCode()
	{
		return RuntimeHelpers.GetHashCode(this);
	}

	protected virtual ImmutableArray<NamedTypeSymbol> GetAllInterfaces()
	{
		InterfaceInfo interfaceInfo = GetInterfaceInfo();
		if (interfaceInfo == s_noInterfaces)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		if (interfaceInfo.allInterfaces.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref interfaceInfo.allInterfaces, MakeAllInterfaces());
		}
		return interfaceInfo.allInterfaces;
	}

	protected virtual ImmutableArray<NamedTypeSymbol> MakeAllInterfaces()
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		HashSet<NamedTypeSymbol> visited = new HashSet<NamedTypeSymbol>(SymbolEqualityComparer.ConsiderEverything);
		TypeSymbol typeSymbol = this;
		while ((object)typeSymbol != null)
		{
			ImmutableArray<NamedTypeSymbol> immutableArray = ((typeSymbol.TypeKind == TypeKind.TypeParameter) ? ((TypeParameterSymbol)typeSymbol).EffectiveInterfacesNoUseSiteDiagnostics : typeSymbol.InterfacesNoUseSiteDiagnostics());
			for (int num = immutableArray.Length - 1; num >= 0; num--)
			{
				addAllInterfaces(immutableArray[num], visited, instance);
			}
			typeSymbol = typeSymbol.BaseTypeNoUseSiteDiagnostics;
		}
		instance.ReverseContents();
		return instance.ToImmutableAndFree();
		static void addAllInterfaces(NamedTypeSymbol @interface, HashSet<NamedTypeSymbol> hashSet, ArrayBuilder<NamedTypeSymbol> result)
		{
			if (hashSet.Add(@interface))
			{
				ImmutableArray<NamedTypeSymbol> immutableArray2 = @interface.InterfacesNoUseSiteDiagnostics();
				for (int num2 = immutableArray2.Length - 1; num2 >= 0; num2--)
				{
					addAllInterfaces(immutableArray2[num2], hashSet, result);
				}
				result.Add(@interface);
			}
		}
	}

	internal MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> InterfacesAndTheirBaseInterfacesWithDefinitionUseSiteDiagnostics(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics = InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics;
		foreach (NamedTypeSymbol key in interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Keys)
		{
			key.OriginalDefinition.AddUseSiteInfo(ref useSiteInfo);
		}
		return interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics;
	}

	private static MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> MakeInterfacesAndTheirBaseInterfaces(ImmutableArray<NamedTypeSymbol> declaredInterfaces)
	{
		MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> multiDictionary = new MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>(declaredInterfaces.Length, SymbolEqualityComparer.CLRSignature, SymbolEqualityComparer.ConsiderEverything);
		foreach (NamedTypeSymbol item in declaredInterfaces)
		{
			if (multiDictionary.Add(item, item))
			{
				foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in item.AllInterfacesNoUseSiteDiagnostics)
				{
					multiDictionary.Add(allInterfacesNoUseSiteDiagnostic, allInterfacesNoUseSiteDiagnostic);
				}
			}
		}
		return multiDictionary;
	}

	public Symbol FindImplementationForInterfaceMember(Symbol interfaceMember)
	{
		if ((object)interfaceMember == null)
		{
			throw new ArgumentNullException("interfaceMember");
		}
		if (!interfaceMember.IsImplementableInterfaceMember())
		{
			return null;
		}
		if (this.IsInterfaceType())
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			return FindMostSpecificImplementation(interfaceMember, (NamedTypeSymbol)this, ref useSiteInfo);
		}
		return FindImplementationForInterfaceMemberInNonInterface(interfaceMember);
	}

	internal TypeSymbol()
	{
	}

	protected sealed override bool IsHighestPriorityUseSiteErrorCode(int code)
	{
		if (code == 648 || code == 9041)
		{
			return true;
		}
		return false;
	}

	internal abstract bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes);

	internal bool IsTupleTypeOfCardinality(int targetCardinality)
	{
		if (IsTupleType)
		{
			return TupleElementTypesWithAnnotations.Length == targetCardinality;
		}
		return false;
	}

	internal bool IsManagedType(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return GetManagedKind(ref useSiteInfo) == ManagedKind.Managed;
	}

	internal abstract ManagedKind GetManagedKind(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo);

	internal bool NeedsNullableAttribute()
	{
		return TypeWithAnnotations.NeedsNullableAttribute(default(TypeWithAnnotations), this);
	}

	internal abstract void AddNullableTransforms(ArrayBuilder<byte> transforms);

	internal abstract bool ApplyNullableTransforms(byte defaultTransformFlag, ImmutableArray<byte> transforms, ref int position, out TypeSymbol result);

	internal abstract TypeSymbol SetNullabilityForReferenceTypes(Func<TypeWithAnnotations, TypeWithAnnotations> transform);

	internal TypeSymbol SetUnknownNullabilityForReferenceTypes()
	{
		return SetNullabilityForReferenceTypes(s_setUnknownNullability);
	}

	internal abstract TypeSymbol MergeEquivalentTypes(TypeSymbol other, VarianceKind variance);

	public string ToDisplayString(Microsoft.CodeAnalysis.NullableFlowState topLevelNullability, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToDisplayString((ITypeSymbol)base.ISymbol, topLevelNullability, format);
	}

	public ImmutableArray<SymbolDisplayPart> ToDisplayParts(Microsoft.CodeAnalysis.NullableFlowState topLevelNullability, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToDisplayParts((ITypeSymbol)base.ISymbol, topLevelNullability, format);
	}

	public string ToMinimalDisplayString(SemanticModel semanticModel, Microsoft.CodeAnalysis.NullableFlowState topLevelNullability, int position, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToMinimalDisplayString((ITypeSymbol)base.ISymbol, topLevelNullability, semanticModel, position, format);
	}

	public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, Microsoft.CodeAnalysis.NullableFlowState topLevelNullability, int position, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToMinimalDisplayParts((ITypeSymbol)base.ISymbol, topLevelNullability, semanticModel, position, format);
	}

	internal SymbolAndDiagnostics FindImplementationForInterfaceMemberInNonInterfaceWithDiagnostics(Symbol interfaceMember, bool ignoreImplementationInInterfacesIfResultIsNotReady = false)
	{
		if (this.IsInterfaceType())
		{
			return SymbolAndDiagnostics.Empty;
		}
		NamedTypeSymbol containingType = interfaceMember.ContainingType;
		if ((object)containingType == null || !containingType.IsInterface)
		{
			return SymbolAndDiagnostics.Empty;
		}
		SymbolKind kind = interfaceMember.Kind;
		if (kind == SymbolKind.Event || kind == SymbolKind.Method || kind == SymbolKind.Property)
		{
			InterfaceInfo interfaceInfo = GetInterfaceInfo();
			if (interfaceInfo == s_noInterfaces)
			{
				return SymbolAndDiagnostics.Empty;
			}
			ConcurrentDictionary<Symbol, SymbolAndDiagnostics> implementationForInterfaceMemberMap = interfaceInfo.ImplementationForInterfaceMemberMap;
			if (implementationForInterfaceMemberMap.TryGetValue(interfaceMember, out var value))
			{
				return value;
			}
			value = ComputeImplementationAndDiagnosticsForInterfaceMember(interfaceMember, ignoreImplementationInInterfacesIfResultIsNotReady, out var implementationInInterfacesMightChangeResult);
			if (!implementationInInterfacesMightChangeResult)
			{
				implementationForInterfaceMemberMap.TryAdd(interfaceMember, value);
			}
			return value;
		}
		return SymbolAndDiagnostics.Empty;
	}

	internal Symbol FindImplementationForInterfaceMemberInNonInterface(Symbol interfaceMember, bool ignoreImplementationInInterfacesIfResultIsNotReady = false)
	{
		return FindImplementationForInterfaceMemberInNonInterfaceWithDiagnostics(interfaceMember, ignoreImplementationInInterfacesIfResultIsNotReady).Symbol;
	}

	private SymbolAndDiagnostics ComputeImplementationAndDiagnosticsForInterfaceMember(Symbol interfaceMember, bool ignoreImplementationInInterfaces, out bool implementationInInterfacesMightChangeResult)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, DeclaringCompilation != null);
		return new SymbolAndDiagnostics(ComputeImplementationForInterfaceMember(interfaceMember, this, instance, ignoreImplementationInInterfaces, out implementationInInterfacesMightChangeResult), instance.ToReadOnlyAndFree());
	}

	private static Symbol ComputeImplementationForInterfaceMember(Symbol interfaceMember, TypeSymbol implementingType, BindingDiagnosticBag diagnostics, bool ignoreImplementationInInterfaces, out bool implementationInInterfacesMightChangeResult)
	{
		NamedTypeSymbol containingType = interfaceMember.ContainingType;
		bool flag = false;
		bool flag2 = false;
		Symbol implicitImpl = null;
		Symbol symbol = null;
		bool flag3 = interfaceMember.DeclaredAccessibility == Accessibility.Public && !interfaceMember.IsEventOrPropertyWithImplementableNonPublicAccessor();
		TypeSymbol typeSymbol = null;
		bool flag4 = false;
		CSharpCompilation declaringCompilation = implementingType.DeclaringCompilation;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = ((declaringCompilation != null) ? new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, declaringCompilation.Assembly) : CompoundUseSiteInfo<AssemblySymbol>.DiscardedDependencies);
		TypeSymbol typeSymbol2 = implementingType;
		while ((object)typeSymbol2 != null)
		{
			MultiDictionary<Symbol, Symbol>.ValueSet explicitImplementationForInterfaceMember = typeSymbol2.GetExplicitImplementationForInterfaceMember(interfaceMember);
			if (explicitImplementationForInterfaceMember.Count == 1)
			{
				implementationInInterfacesMightChangeResult = false;
				return explicitImplementationForInterfaceMember.Single();
			}
			if (explicitImplementationForInterfaceMember.Count > 1)
			{
				if (((object)typeSymbol2 == implementingType) | flag4)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateExplicitImpl, implementingType.GetFirstLocation(), interfaceMember);
				}
				implementationInInterfacesMightChangeResult = false;
				return null;
			}
			bool flag5 = (object)typeSymbol2 != implementingType || !typeSymbol2.IsDefinition;
			if (flag5 && interfaceMember is MethodSymbol interfaceMethod && typeSymbol2.InterfacesAndTheirBaseInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo).ContainsKey(containingType))
			{
				MethodSymbol bodyOfSynthesizedInterfaceMethodImpl = typeSymbol2.GetBodyOfSynthesizedInterfaceMethodImpl(interfaceMethod);
				if ((object)bodyOfSynthesizedInterfaceMethodImpl != null)
				{
					implementationInInterfacesMightChangeResult = false;
					return bodyOfSynthesizedInterfaceMethodImpl;
				}
			}
			if (IsExplicitlyImplementedViaAccessors(flag5, interfaceMember, typeSymbol2, ref useSiteInfo, out var implementingMember))
			{
				implementationInInterfacesMightChangeResult = false;
				return implementingMember;
			}
			if ((!flag || (!flag3 && (object)typeSymbol == null)) && typeSymbol2.InterfacesAndTheirBaseInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo).ContainsKey(containingType))
			{
				if (!flag)
				{
					flag2 = !(typeSymbol2.OriginalDefinition.ContainingModule is PEModuleSymbol);
					flag = true;
				}
				if ((object)typeSymbol2 == implementingType)
				{
					flag4 = true;
				}
				else if (!flag3 && (object)typeSymbol == null)
				{
					typeSymbol = typeSymbol2;
				}
			}
			if (flag && (!interfaceMember.IsStatic | flag2))
			{
				FindPotentialImplicitImplementationMemberDeclaredInType(interfaceMember, flag2, typeSymbol2, out var implicitImpl2, out var closeMismatch);
				if ((object)implicitImpl2 != null)
				{
					implicitImpl = implicitImpl2;
					break;
				}
				if ((object)symbol == null)
				{
					symbol = closeMismatch;
				}
			}
			typeSymbol2 = typeSymbol2.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		bool flag6 = true;
		if (flag2 && implicitImpl is MethodSymbol methodSymbol && methodSymbol.IsOperator() != ((MethodSymbol)interfaceMember).IsOperator())
		{
			symbol = implicitImpl;
			implicitImpl = null;
			flag6 = false;
		}
		else if (interfaceMember.IsAccessor())
		{
			Symbol symbol2 = implicitImpl;
			CheckForImplementationOfCorrespondingPropertyOrEvent((MethodSymbol)interfaceMember, implementingType, flag2, ref implicitImpl);
			if ((object)symbol2 != null && (object)implicitImpl == null)
			{
				flag6 = false;
			}
		}
		Symbol symbol3 = null;
		if (((object)implicitImpl == null) & flag & flag6)
		{
			if (ignoreImplementationInInterfaces)
			{
				implementationInInterfacesMightChangeResult = true;
			}
			else
			{
				symbol3 = FindMostSpecificImplementationInInterfaces(interfaceMember, implementingType, ref useSiteInfo, diagnostics);
				implementationInInterfacesMightChangeResult = false;
			}
		}
		else
		{
			implementationInInterfacesMightChangeResult = false;
		}
		diagnostics.Add((useSiteInfo.Diagnostics == null || !flag4) ? Location.None : GetInterfaceLocation(interfaceMember, implementingType), useSiteInfo);
		if ((object)symbol3 != null)
		{
			if (flag4)
			{
				ReportDefaultInterfaceImplementationMatchDiagnostics(interfaceMember, implementingType, symbol3, diagnostics);
			}
			return symbol3;
		}
		if (flag4)
		{
			if ((object)implicitImpl != null)
			{
				bool flag7 = false;
				if (!flag3 && interfaceMember.Kind == SymbolKind.Method && (object)typeSymbol == null)
				{
					CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2 = ((declaringCompilation != null) ? new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, declaringCompilation.Assembly) : CompoundUseSiteInfo<AssemblySymbol>.DiscardedDependencies);
					if (implementingType is NamedTypeSymbol within && !AccessCheck.IsSymbolAccessible(interfaceMember, within, ref useSiteInfo2))
					{
						diagnostics.Add(ErrorCode.ERR_ImplicitImplementationOfInaccessibleInterfaceMember, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implementingType, interfaceMember, implicitImpl);
						flag7 = true;
					}
					else if (!interfaceMember.IsStatic)
					{
						LanguageVersion languageVersion = MessageID.IDS_FeatureImplicitImplementationOfNonPublicMembers.RequiredVersion();
						LanguageVersion? languageVersion2 = implementingType.DeclaringCompilation?.LanguageVersion;
						if (languageVersion > languageVersion2)
						{
							diagnostics.Add(ErrorCode.ERR_ImplicitImplementationOfNonPublicInterfaceMember, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implementingType, interfaceMember, implicitImpl, languageVersion2.GetValueOrDefault().ToDisplayString(), new CSharpRequiredLanguageVersion(languageVersion));
						}
					}
					diagnostics.Add((useSiteInfo2.Diagnostics == null) ? Location.None : GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), useSiteInfo2);
				}
				if (!flag7)
				{
					ReportImplicitImplementationMatchDiagnostics(interfaceMember, implementingType, implicitImpl, diagnostics);
				}
			}
			else if ((object)symbol != null)
			{
				ReportImplicitImplementationMismatchDiagnostics(interfaceMember, implementingType, symbol, diagnostics);
			}
		}
		return implicitImpl;
	}

	private static Symbol FindMostSpecificImplementationInInterfaces(Symbol interfaceMember, TypeSymbol implementingType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, BindingDiagnosticBag diagnostics)
	{
		var (interfaceAccessor, interfaceAccessor2) = GetImplementableAccessors(interfaceMember);
		if (stopLookup(interfaceAccessor, implementingType) || stopLookup(interfaceAccessor2, implementingType))
		{
			return null;
		}
		Symbol result = FindMostSpecificImplementationInBases(interfaceMember, implementingType, ref useSiteInfo, out var conflictingImplementation, out var conflictingImplementation2);
		if ((object)conflictingImplementation != null)
		{
			diagnostics.Add(ErrorCode.ERR_MostSpecificImplementationIsNotFound, GetInterfaceLocation(interfaceMember, implementingType), interfaceMember, conflictingImplementation, conflictingImplementation2);
		}
		return result;
		static bool stopLookup(MethodSymbol methodSymbol, TypeSymbol typeSymbol)
		{
			if ((object)methodSymbol == null)
			{
				return false;
			}
			SymbolAndDiagnostics symbolAndDiagnostics = typeSymbol.FindImplementationForInterfaceMemberInNonInterfaceWithDiagnostics(methodSymbol);
			if ((object)symbolAndDiagnostics.Symbol != null)
			{
				return !symbolAndDiagnostics.Symbol.ContainingType.IsInterface;
			}
			return !symbolAndDiagnostics.Diagnostics.Diagnostics.Any((Diagnostic d) => d.Code == 8705);
		}
	}

	private static Symbol FindMostSpecificImplementation(Symbol interfaceMember, NamedTypeSymbol implementingInterface, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		MultiDictionary<Symbol, Symbol>.ValueSet valueSet = FindImplementationInInterface(interfaceMember, implementingInterface);
		switch (valueSet.Count)
		{
		case 0:
		{
			var (methodSymbol, methodSymbol2) = GetImplementableAccessors(interfaceMember);
			if (((object)methodSymbol != null && FindImplementationInInterface(methodSymbol, implementingInterface).Count != 0) || ((object)methodSymbol2 != null && FindImplementationInInterface(methodSymbol2, implementingInterface).Count != 0))
			{
				return null;
			}
			Symbol conflictingImplementation;
			Symbol conflictingImplementation2;
			return FindMostSpecificImplementationInBases(interfaceMember, implementingInterface, ref useSiteInfo, out conflictingImplementation, out conflictingImplementation2);
		}
		case 1:
		{
			Symbol symbol = valueSet.Single();
			if (symbol.IsAbstract)
			{
				return null;
			}
			return symbol;
		}
		default:
			return null;
		}
	}

	private static Symbol FindMostSpecificImplementationInBases(Symbol interfaceMember, TypeSymbol implementingType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out Symbol conflictingImplementation1, out Symbol conflictingImplementation2)
	{
		ImmutableArray<NamedTypeSymbol> allInterfaces = implementingType.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		if (allInterfaces.IsEmpty)
		{
			conflictingImplementation1 = null;
			conflictingImplementation2 = null;
			return null;
		}
		var (methodSymbol, methodSymbol2) = GetImplementableAccessors(interfaceMember);
		if ((object)methodSymbol == null && (object)methodSymbol2 == null)
		{
			return findMostSpecificImplementationInBases(interfaceMember, allInterfaces, ref useSiteInfo, out conflictingImplementation1, out conflictingImplementation2);
		}
		Symbol symbol = findMostSpecificImplementationInBases(methodSymbol ?? methodSymbol2, allInterfaces, ref useSiteInfo, out var conflictingImplementation3, out var conflictingImplementation4);
		if ((object)symbol == null && (object)conflictingImplementation3 == null)
		{
			conflictingImplementation1 = null;
			conflictingImplementation2 = null;
			return null;
		}
		if ((object)methodSymbol == null || (object)methodSymbol2 == null)
		{
			if ((object)symbol != null)
			{
				conflictingImplementation1 = null;
				conflictingImplementation2 = null;
				return findImplementationInInterface(interfaceMember, symbol);
			}
			conflictingImplementation1 = findImplementationInInterface(interfaceMember, conflictingImplementation3);
			conflictingImplementation2 = findImplementationInInterface(interfaceMember, conflictingImplementation4);
			if ((object)conflictingImplementation1 == null != ((object)conflictingImplementation2 == null))
			{
				conflictingImplementation1 = null;
				conflictingImplementation2 = null;
			}
			return null;
		}
		Symbol symbol2 = findMostSpecificImplementationInBases(methodSymbol2, allInterfaces, ref useSiteInfo, out var conflictingImplementation5, out var conflictingImplementation6);
		if (((object)symbol2 == null && (object)conflictingImplementation5 == null) || (object)symbol == null != ((object)symbol2 == null))
		{
			conflictingImplementation1 = null;
			conflictingImplementation2 = null;
			return null;
		}
		if ((object)symbol != null)
		{
			conflictingImplementation1 = null;
			conflictingImplementation2 = null;
			return findImplementationInInterface(interfaceMember, symbol, symbol2);
		}
		conflictingImplementation1 = findImplementationInInterface(interfaceMember, conflictingImplementation3, conflictingImplementation5);
		conflictingImplementation2 = findImplementationInInterface(interfaceMember, conflictingImplementation4, conflictingImplementation6);
		if ((object)conflictingImplementation1 == null != ((object)conflictingImplementation2 == null))
		{
			conflictingImplementation1 = null;
			conflictingImplementation2 = null;
		}
		return null;
		static Symbol findImplementationInInterface(Symbol interfaceMember2, Symbol inplementingAccessor1, Symbol implementingAccessor2 = null)
		{
			NamedTypeSymbol containingType = inplementingAccessor1.ContainingType;
			if ((object)implementingAccessor2 != null && !containingType.Equals(implementingAccessor2.ContainingType, TypeCompareKind.ConsiderEverything))
			{
				return null;
			}
			MultiDictionary<Symbol, Symbol>.ValueSet valueSet = FindImplementationInInterface(interfaceMember2, containingType);
			if (valueSet.Count == 1)
			{
				return valueSet.Single();
			}
			return null;
		}
		static Symbol findMostSpecificImplementationInBases(Symbol interfaceMember2, ImmutableArray<NamedTypeSymbol> immutableArray, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2, out Symbol reference, out Symbol reference2)
		{
			ArrayBuilder<(MultiDictionary<Symbol, Symbol>.ValueSet, MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>)> instance = ArrayBuilder<(MultiDictionary<Symbol, Symbol>.ValueSet, MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>)>.GetInstance();
			foreach (NamedTypeSymbol item4 in immutableArray)
			{
				if (item4.IsInterface)
				{
					MultiDictionary<Symbol, Symbol>.ValueSet item = FindImplementationInInterface(interfaceMember2, item4);
					if (item.Count != 0)
					{
						for (int i = 0; i < instance.Count; i++)
						{
							(MultiDictionary<Symbol, Symbol>.ValueSet, MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>) tuple2 = instance[i];
							MultiDictionary<Symbol, Symbol>.ValueSet item2 = tuple2.Item1;
							MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> multiDictionary = tuple2.Item2;
							NamedTypeSymbol containingType = item2.First().ContainingType;
							if (containingType.Equals(item4, TypeCompareKind.CLRSignatureCompareOptions))
							{
								instance[i] = (item, multiDictionary);
								item = default(MultiDictionary<Symbol, Symbol>.ValueSet);
								break;
							}
							if (multiDictionary == null)
							{
								multiDictionary = containingType.InterfacesAndTheirBaseInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo2);
								instance[i] = (item2, multiDictionary);
							}
							if (multiDictionary.ContainsKey(item4))
							{
								item = default(MultiDictionary<Symbol, Symbol>.ValueSet);
								break;
							}
						}
						if (item.Count != 0)
						{
							if (instance.Count != 0)
							{
								MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> multiDictionary2 = item4.InterfacesAndTheirBaseInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo2);
								for (int num = instance.Count - 1; num >= 0; num--)
								{
									if (multiDictionary2.ContainsKey(instance[num].Item1.First().ContainingType))
									{
										instance.RemoveAt(num);
									}
								}
								instance.Add((item, multiDictionary2));
							}
							else
							{
								instance.Add((item, null));
							}
						}
					}
				}
			}
			Symbol symbol3;
			switch (instance.Count)
			{
			case 0:
				symbol3 = null;
				reference = null;
				reference2 = null;
				break;
			case 1:
			{
				MultiDictionary<Symbol, Symbol>.ValueSet item3 = instance[0].Item1;
				if (item3.Count == 1)
				{
					symbol3 = item3.Single();
					if (symbol3.IsAbstract)
					{
						symbol3 = null;
					}
				}
				else
				{
					symbol3 = null;
				}
				reference = null;
				reference2 = null;
				break;
			}
			default:
				symbol3 = null;
				reference = instance[0].Item1.First();
				reference2 = instance[1].Item1.First();
				break;
			}
			instance.Free();
			return symbol3;
		}
	}

	internal static MultiDictionary<Symbol, Symbol>.ValueSet FindImplementationInInterface(Symbol interfaceMember, NamedTypeSymbol interfaceType)
	{
		NamedTypeSymbol containingType = interfaceMember.ContainingType;
		if (containingType.Equals(interfaceType, TypeCompareKind.CLRSignatureCompareOptions))
		{
			if (!interfaceMember.IsAbstract)
			{
				if (!containingType.Equals(interfaceType, TypeCompareKind.ConsiderEverything))
				{
					interfaceMember = interfaceMember.OriginalDefinition.SymbolAsMember(interfaceType);
				}
				return new MultiDictionary<Symbol, Symbol>.ValueSet(interfaceMember);
			}
			return default(MultiDictionary<Symbol, Symbol>.ValueSet);
		}
		return interfaceType.GetExplicitImplementationForInterfaceMember(interfaceMember);
	}

	private static (MethodSymbol interfaceAccessor1, MethodSymbol interfaceAccessor2) GetImplementableAccessors(Symbol interfaceMember)
	{
		MethodSymbol methodSymbol;
		MethodSymbol methodSymbol2;
		switch (interfaceMember.Kind)
		{
		case SymbolKind.Property:
		{
			PropertySymbol obj2 = (PropertySymbol)interfaceMember;
			methodSymbol = obj2.GetMethod;
			methodSymbol2 = obj2.SetMethod;
			break;
		}
		case SymbolKind.Event:
		{
			EventSymbol obj = (EventSymbol)interfaceMember;
			methodSymbol = obj.AddMethod;
			methodSymbol2 = obj.RemoveMethod;
			break;
		}
		default:
			methodSymbol = null;
			methodSymbol2 = null;
			break;
		}
		if (!methodSymbol.IsImplementable())
		{
			methodSymbol = null;
		}
		if (!methodSymbol2.IsImplementable())
		{
			methodSymbol2 = null;
		}
		return (interfaceAccessor1: methodSymbol, interfaceAccessor2: methodSymbol2);
	}

	private static bool IsExplicitlyImplementedViaAccessors(bool checkPendingExplicitImplementations, Symbol interfaceMember, TypeSymbol currType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out Symbol implementingMember)
	{
		var (interfaceAccessor, interfaceAccessor2) = GetImplementableAccessors(interfaceMember);
		if (TryGetExplicitImplementationAssociatedPropertyOrEvent(checkPendingExplicitImplementations, interfaceAccessor, currType, ref useSiteInfo, out var associated) | TryGetExplicitImplementationAssociatedPropertyOrEvent(checkPendingExplicitImplementations, interfaceAccessor2, currType, ref useSiteInfo, out var associated2))
		{
			if ((object)associated == null || (object)associated2 == null || associated == associated2)
			{
				implementingMember = associated ?? associated2;
				if ((object)implementingMember != null && !(implementingMember.OriginalDefinition.ContainingModule is PEModuleSymbol) && implementingMember.IsExplicitInterfaceImplementation())
				{
					implementingMember = null;
				}
			}
			else
			{
				implementingMember = null;
			}
			return true;
		}
		implementingMember = null;
		return false;
	}

	private static bool TryGetExplicitImplementationAssociatedPropertyOrEvent(bool checkPendingExplicitImplementations, MethodSymbol interfaceAccessor, TypeSymbol currType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out Symbol associated)
	{
		if ((object)interfaceAccessor != null)
		{
			MultiDictionary<Symbol, Symbol>.ValueSet explicitImplementationForInterfaceMember = currType.GetExplicitImplementationForInterfaceMember(interfaceAccessor);
			if (explicitImplementationForInterfaceMember.Count == 1)
			{
				Symbol symbol = explicitImplementationForInterfaceMember.Single();
				associated = ((symbol.Kind == SymbolKind.Method) ? ((MethodSymbol)symbol).AssociatedSymbol : null);
				return true;
			}
			if (checkPendingExplicitImplementations && currType.InterfacesAndTheirBaseInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo).ContainsKey(interfaceAccessor.ContainingType))
			{
				MethodSymbol bodyOfSynthesizedInterfaceMethodImpl = currType.GetBodyOfSynthesizedInterfaceMethodImpl(interfaceAccessor);
				if ((object)bodyOfSynthesizedInterfaceMethodImpl != null)
				{
					associated = bodyOfSynthesizedInterfaceMethodImpl.AssociatedSymbol;
					return true;
				}
			}
		}
		associated = null;
		return false;
	}

	private static void CheckForImplementationOfCorrespondingPropertyOrEvent(MethodSymbol interfaceMethod, TypeSymbol implementingType, bool implementingTypeIsFromSomeCompilation, ref Symbol implicitImpl)
	{
		Symbol associatedSymbol = interfaceMethod.AssociatedSymbol;
		Symbol symbol = implementingType.FindImplementationForInterfaceMemberInNonInterface(associatedSymbol, ignoreImplementationInInterfacesIfResultIsNotReady: true);
		MethodSymbol methodSymbol = null;
		if ((object)symbol != null && !symbol.ContainingType.IsInterface)
		{
			methodSymbol = interfaceMethod.MethodKind switch
			{
				MethodKind.PropertyGet => ((PropertySymbol)symbol).GetOwnOrInheritedGetMethod(), 
				MethodKind.PropertySet => ((PropertySymbol)symbol).GetOwnOrInheritedSetMethod(), 
				MethodKind.EventAdd => ((EventSymbol)symbol).GetOwnOrInheritedAddMethod(), 
				MethodKind.EventRemove => ((EventSymbol)symbol).GetOwnOrInheritedRemoveMethod(), 
				_ => throw ExceptionUtilities.UnexpectedValue(interfaceMethod.MethodKind), 
			};
		}
		if (methodSymbol == implicitImpl)
		{
			return;
		}
		if ((object)methodSymbol == null && (object)implicitImpl != null && implicitImpl.IsAccessor())
		{
			implicitImpl = null;
		}
		else if ((object)methodSymbol != null && ((object)implicitImpl == null || Equals(methodSymbol.ContainingType, implicitImpl.ContainingType, TypeCompareKind.ConsiderEverything)))
		{
			MethodSymbol interfaceMember = new SignatureOnlyMethodSymbol(methodSymbol.Name, interfaceMethod.ContainingType, interfaceMethod.MethodKind, interfaceMethod.CallingConvention, interfaceMethod.TypeParameters, interfaceMethod.Parameters, interfaceMethod.RefKind, interfaceMethod.IsInitOnly, interfaceMethod.IsStatic, interfaceMethod.ReturnTypeWithAnnotations, interfaceMethod.RefCustomModifiers, interfaceMethod.ExplicitInterfaceImplementations);
			if (IsInterfaceMemberImplementation(methodSymbol, interfaceMember, implementingTypeIsFromSomeCompilation))
			{
				implicitImpl = methodSymbol;
			}
		}
	}

	private static void ReportDefaultInterfaceImplementationMatchDiagnostics(Symbol interfaceMember, TypeSymbol implementingType, Symbol implicitImpl, BindingDiagnosticBag diagnostics)
	{
		if (interfaceMember.Kind != SymbolKind.Method)
		{
			return;
		}
		bool isStatic = implicitImpl.IsStatic;
		if (!isStatic && implementingType.IsRefLikeType)
		{
			diagnostics.Add(ErrorCode.ERR_RefStructDoesNotSupportDefaultInterfaceImplementationForMember, GetInterfaceLocation(interfaceMember, implementingType), implicitImpl, interfaceMember, implementingType);
		}
		else if (implementingType.ContainingModule != implicitImpl.ContainingModule)
		{
			MessageID messageID = (isStatic ? MessageID.IDS_FeatureStaticAbstractMembersInInterfaces : MessageID.IDS_DefaultInterfaceImplementation);
			LanguageVersion languageVersion = messageID.RequiredVersion();
			LanguageVersion? languageVersion2 = implementingType.DeclaringCompilation?.LanguageVersion;
			if (languageVersion > languageVersion2)
			{
				diagnostics.Add(ErrorCode.ERR_LanguageVersionDoesNotSupportInterfaceImplementationForMember, GetInterfaceLocation(interfaceMember, implementingType), implicitImpl, interfaceMember, implementingType, messageID.Localize(), languageVersion2.GetValueOrDefault().ToDisplayString(), new CSharpRequiredLanguageVersion(languageVersion));
			}
			if (!(isStatic ? implementingType.ContainingAssembly.RuntimeSupportsStaticAbstractMembersInInterfaces : implementingType.ContainingAssembly.RuntimeSupportsDefaultInterfaceImplementation))
			{
				diagnostics.Add(isStatic ? ErrorCode.ERR_RuntimeDoesNotSupportStaticAbstractMembersInInterfacesForMember : ErrorCode.ERR_RuntimeDoesNotSupportDefaultInterfaceImplementationForMember, GetInterfaceLocation(interfaceMember, implementingType), implicitImpl, interfaceMember, implementingType);
			}
		}
	}

	private static void ReportImplicitImplementationMatchDiagnostics(Symbol interfaceMember, TypeSymbol implementingType, Symbol implicitImpl, BindingDiagnosticBag diagnostics)
	{
		bool flag = false;
		if (interfaceMember.Kind == SymbolKind.Method)
		{
			MethodSymbol methodSymbol = (MethodSymbol)interfaceMember;
			bool flag2 = implicitImpl.IsAccessor();
			bool flag3 = methodSymbol.IsAccessor();
			if (flag3 && !flag2 && !methodSymbol.IsIndexedPropertyAccessor())
			{
				diagnostics.Add(ErrorCode.ERR_MethodImplementingAccessor, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, methodSymbol, implementingType);
			}
			else if (!flag3 & flag2)
			{
				diagnostics.Add(ErrorCode.ERR_AccessorImplementingMethod, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, methodSymbol, implementingType);
			}
			else
			{
				MethodSymbol methodSymbol2 = (MethodSymbol)implicitImpl;
				if (methodSymbol2.IsConditional)
				{
					diagnostics.Add(ErrorCode.ERR_InterfaceImplementedByConditional, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, methodSymbol, implementingType);
				}
				else if (methodSymbol2.IsStatic && methodSymbol2.MethodKind == MethodKind.Ordinary && methodSymbol2.GetUnmanagedCallersOnlyAttributeData(forceComplete: true) != null)
				{
					diagnostics.Add(ErrorCode.ERR_InterfaceImplementedByUnmanagedCallersOnlyMethod, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, methodSymbol, implementingType);
				}
				else if (ReportAnyMismatchedConstraints(methodSymbol, implementingType, methodSymbol2, diagnostics))
				{
					flag = true;
				}
			}
		}
		if (implicitImpl.ContainsTupleNames() && MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(implicitImpl, interfaceMember))
		{
			diagnostics.Add(ErrorCode.ERR_ImplBadTupleNames, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, interfaceMember);
			flag = true;
		}
		if (!flag && implementingType.DeclaringCompilation != null)
		{
			CheckModifierMismatchOnImplementingMember(implementingType, implicitImpl, interfaceMember, isExplicit: false, diagnostics);
		}
		if (!implicitImpl.ContainingType.IsDefinition)
		{
			foreach (Symbol member in implicitImpl.ContainingType.GetMembers(implicitImpl.Name))
			{
				if (member.DeclaredAccessibility == Accessibility.Public && !(member == implicitImpl) && MemberSignatureComparer.RuntimeImplicitImplementationComparer.Equals(interfaceMember, member) && !member.IsAccessor())
				{
					diagnostics.Add(ErrorCode.WRN_MultipleRuntimeImplementationMatches, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, member), member, interfaceMember, implementingType);
				}
			}
		}
		if (implicitImpl.IsStatic && interfaceMember.ContainingModule != implementingType.ContainingModule)
		{
			LanguageVersion languageVersion = MessageID.IDS_FeatureStaticAbstractMembersInInterfaces.RequiredVersion();
			LanguageVersion? languageVersion2 = implementingType.DeclaringCompilation?.LanguageVersion;
			if (languageVersion > languageVersion2)
			{
				diagnostics.Add(ErrorCode.ERR_LanguageVersionDoesNotSupportInterfaceImplementationForMember, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, interfaceMember, implementingType, MessageID.IDS_FeatureStaticAbstractMembersInInterfaces.Localize(), languageVersion2.GetValueOrDefault().ToDisplayString(), new CSharpRequiredLanguageVersion(languageVersion));
			}
			if (!implementingType.ContainingAssembly.RuntimeSupportsStaticAbstractMembersInInterfaces)
			{
				diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportStaticAbstractMembersInInterfacesForMember, GetImplicitImplementationDiagnosticLocation(interfaceMember, implementingType, implicitImpl), implicitImpl, interfaceMember, implementingType);
			}
		}
	}

	internal static void CheckModifierMismatchOnImplementingMember(TypeSymbol implementingType, Symbol implementingMember, Symbol interfaceMember, bool isExplicit, BindingDiagnosticBag diagnostics)
	{
		if (implementingMember.IsImplicitlyDeclared || implementingMember.IsAccessor())
		{
			return;
		}
		if (interfaceMember.Kind == SymbolKind.Event)
		{
			CSharpCompilation declaringCompilation = implementingType.DeclaringCompilation;
			EventSymbol overridingEvent = (EventSymbol)implementingMember;
			EventSymbol overriddenEvent = (EventSymbol)interfaceMember;
			SourceMemberContainerTypeSymbol.CheckValidNullableEventOverride(declaringCompilation, overriddenEvent, overridingEvent, diagnostics, delegate(BindingDiagnosticBag bindingDiagnosticBag, EventSymbol implementedEvent, EventSymbol implementingEvent, (TypeSymbol implementingType, bool isExplicit) arg)
			{
				if (arg.isExplicit)
				{
					bindingDiagnosticBag.Add(ErrorCode.WRN_NullabilityMismatchInTypeOnExplicitImplementation, implementingEvent.GetFirstLocation(), new FormattedSymbol(implementedEvent, SymbolDisplayFormat.MinimallyQualifiedFormat));
				}
				else
				{
					bindingDiagnosticBag.Add(ErrorCode.WRN_NullabilityMismatchInTypeOnImplicitImplementation, GetImplicitImplementationDiagnosticLocation(implementedEvent, arg.implementingType, implementingEvent), new FormattedSymbol(implementingEvent, SymbolDisplayFormat.MinimallyQualifiedFormat), new FormattedSymbol(implementedEvent, SymbolDisplayFormat.MinimallyQualifiedFormat));
				}
			}, (implementingType, isExplicit));
			return;
		}
		switch (interfaceMember.Kind)
		{
		case SymbolKind.Property:
		{
			PropertySymbol property = (PropertySymbol)implementingMember;
			PropertySymbol propertySymbol = (PropertySymbol)interfaceMember;
			MethodSymbol methodSymbol3 = (propertySymbol.GetMethod.IsImplementable() ? property.GetOwnOrInheritedGetMethod() : null);
			MethodSymbol methodSymbol4 = (propertySymbol.SetMethod.IsImplementable() ? property.GetOwnOrInheritedSetMethod() : null);
			if ((object)methodSymbol3 != null)
			{
				checkMethodOverride(implementingType, propertySymbol.GetMethod, methodSymbol3, isExplicit, diagnostics);
			}
			if ((object)methodSymbol4 != null)
			{
				checkMethodOverride(implementingType, propertySymbol.SetMethod, methodSymbol4, isExplicit, diagnostics);
			}
			break;
		}
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)implementingMember;
			MethodSymbol methodSymbol2 = (MethodSymbol)interfaceMember;
			if (methodSymbol2.IsGenericMethod)
			{
				methodSymbol2 = methodSymbol2.Construct(TypeMap.TypeParametersAsTypeSymbolsWithIgnoredAnnotations(methodSymbol.TypeParameters));
			}
			checkMethodOverride(implementingType, methodSymbol2, methodSymbol, isExplicit, diagnostics);
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(interfaceMember.Kind);
		}
		static void checkMethodOverride(TypeSymbol typeSymbol, MethodSymbol implementedMethod, MethodSymbol implementingMethod, bool item, BindingDiagnosticBag bindingDiagnosticBag)
		{
			ReportMismatchInReturnType<(TypeSymbol, bool)> reportMismatchInReturnType = delegate(BindingDiagnosticBag bindingDiagnosticBag2, MethodSymbol methodSymbol6, MethodSymbol methodSymbol5, bool topLevel, (TypeSymbol implementingType, bool isExplicit) arg)
			{
				if (arg.isExplicit)
				{
					bindingDiagnosticBag2.Add(topLevel ? ErrorCode.WRN_TopLevelNullabilityMismatchInReturnTypeOnExplicitImplementation : ErrorCode.WRN_NullabilityMismatchInReturnTypeOnExplicitImplementation, methodSymbol5.GetFirstLocation(), new FormattedSymbol(methodSymbol6.ConstructedFrom, SymbolDisplayFormat.MinimallyQualifiedFormat));
				}
				else
				{
					bindingDiagnosticBag2.Add(topLevel ? ErrorCode.WRN_TopLevelNullabilityMismatchInReturnTypeOnImplicitImplementation : ErrorCode.WRN_NullabilityMismatchInReturnTypeOnImplicitImplementation, GetImplicitImplementationDiagnosticLocation(methodSymbol6, arg.implementingType, methodSymbol5), new FormattedSymbol(methodSymbol5, SymbolDisplayFormat.MinimallyQualifiedFormat), new FormattedSymbol(methodSymbol6.ConstructedFrom, SymbolDisplayFormat.MinimallyQualifiedFormat));
				}
			};
			ReportMismatchInParameterType<(TypeSymbol, bool)> reportMismatchInParameterType = delegate(BindingDiagnosticBag bindingDiagnosticBag2, MethodSymbol methodSymbol6, MethodSymbol methodSymbol5, ParameterSymbol implementingParameter, bool topLevel, (TypeSymbol implementingType, bool isExplicit) arg)
			{
				if (arg.isExplicit)
				{
					bindingDiagnosticBag2.Add(topLevel ? ErrorCode.WRN_TopLevelNullabilityMismatchInParameterTypeOnExplicitImplementation : ErrorCode.WRN_NullabilityMismatchInParameterTypeOnExplicitImplementation, methodSymbol5.GetFirstLocation(), new FormattedSymbol(implementingParameter, SymbolDisplayFormat.ShortFormat), new FormattedSymbol(methodSymbol6.ConstructedFrom, SymbolDisplayFormat.MinimallyQualifiedFormat));
				}
				else
				{
					bindingDiagnosticBag2.Add(topLevel ? ErrorCode.WRN_TopLevelNullabilityMismatchInParameterTypeOnImplicitImplementation : ErrorCode.WRN_NullabilityMismatchInParameterTypeOnImplicitImplementation, GetImplicitImplementationDiagnosticLocation(methodSymbol6, arg.implementingType, methodSymbol5), new FormattedSymbol(implementingParameter, SymbolDisplayFormat.ShortFormat), new FormattedSymbol(methodSymbol5, SymbolDisplayFormat.MinimallyQualifiedFormat), new FormattedSymbol(methodSymbol6.ConstructedFrom, SymbolDisplayFormat.MinimallyQualifiedFormat));
				}
			};
			SourceMemberContainerTypeSymbol.CheckValidNullableMethodOverride(typeSymbol.DeclaringCompilation, implementedMethod, implementingMethod, bindingDiagnosticBag, reportMismatchInReturnType, reportMismatchInParameterType, (typeSymbol, item));
			if (SourceMemberContainerTypeSymbol.RequiresValidScopedOverrideForRefSafety(implementedMethod, implementingMethod.TryGetThisParameter(out ParameterSymbol thisParameter) ? thisParameter : null))
			{
				SourceMemberContainerTypeSymbol.CheckValidScopedOverride(implementedMethod, implementingMethod, bindingDiagnosticBag, delegate(BindingDiagnosticBag bindingDiagnosticBag2, MethodSymbol methodSymbol5, MethodSymbol methodSymbol6, ParameterSymbol implementingParameter, bool _, (TypeSymbol implementingType, bool isExplicit) arg)
				{
					bindingDiagnosticBag2.Add(SourceMemberContainerTypeSymbol.ReportInvalidScopedOverrideAsError(methodSymbol5, methodSymbol6) ? ErrorCode.ERR_ScopedMismatchInParameterOfOverrideOrImplementation : ErrorCode.WRN_ScopedMismatchInParameterOfOverrideOrImplementation, GetImplicitImplementationDiagnosticLocation(methodSymbol5, arg.implementingType, methodSymbol6), new FormattedSymbol(implementingParameter, SymbolDisplayFormat.ShortFormat));
				}, (typeSymbol, item), allowVariance: true, invokedAsExtensionMethod: false);
			}
			SourceMemberContainerTypeSymbol.CheckRefReadonlyInMismatch(implementedMethod, implementingMethod, bindingDiagnosticBag, delegate(BindingDiagnosticBag bindingDiagnosticBag2, MethodSymbol interfaceMember2, MethodSymbol member, ParameterSymbol implementingParameter, bool _, (ParameterSymbol BaseParameter, TypeSymbol Arg) arg)
			{
				(ParameterSymbol BaseParameter, TypeSymbol Arg) tuple = arg;
				ParameterSymbol item2 = tuple.BaseParameter;
				TypeSymbol item3 = tuple.Arg;
				Location implicitImplementationDiagnosticLocation = GetImplicitImplementationDiagnosticLocation(interfaceMember2, item3, member);
				bindingDiagnosticBag2.Add(ErrorCode.WRN_OverridingDifferentRefness, implicitImplementationDiagnosticLocation, implementingParameter, item2);
			}, typeSymbol, invokedAsExtensionMethod: false);
			if (implementingMethod.HasUnscopedRefAttributeOnMethodOrProperty())
			{
				if (implementedMethod.HasUnscopedRefAttributeOnMethodOrProperty())
				{
					if (!implementingMethod.IsExplicitInterfaceImplementation && implementingMethod is SourceMethodSymbol && implementedMethod.ContainingModule != implementingMethod.ContainingModule)
					{
						checkRefStructInterfacesFeatureAvailabilityOnUnscopedRefAttribute(implementingMethod.HasUnscopedRefAttribute ? implementingMethod : implementingMethod.AssociatedSymbol, bindingDiagnosticBag);
					}
				}
				else
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_UnscopedRefAttributeInterfaceImplementation, GetImplicitImplementationDiagnosticLocation(implementedMethod, typeSymbol, implementingMethod), implementedMethod);
				}
			}
		}
		static void checkRefStructInterfacesFeatureAvailabilityOnUnscopedRefAttribute(Symbol implementingSymbol, BindingDiagnosticBag diagnostics2)
		{
			foreach (CSharpAttributeData attribute in implementingSymbol.GetAttributes())
			{
				if (attribute is SourceAttributeData sourceAttributeData)
				{
					SyntaxReference applicationSyntaxReference = sourceAttributeData.ApplicationSyntaxReference;
					if (applicationSyntaxReference != null && attribute.IsTargetAttribute(AttributeDescription.UnscopedRefAttribute))
					{
						MessageID.IDS_FeatureRefStructInterfaces.CheckFeatureAvailability(diagnostics2, implementingSymbol.DeclaringCompilation, applicationSyntaxReference.GetLocation());
						break;
					}
				}
			}
		}
	}

	private static void ReportImplicitImplementationMismatchDiagnostics(Symbol interfaceMember, TypeSymbol implementingType, Symbol closestMismatch, BindingDiagnosticBag diagnostics)
	{
		Location interfaceLocation = GetInterfaceLocation(interfaceMember, implementingType);
		if (closestMismatch.IsStatic != interfaceMember.IsStatic)
		{
			diagnostics.Add(closestMismatch.IsStatic ? ErrorCode.ERR_CloseUnimplementedInterfaceMemberStatic : ErrorCode.ERR_CloseUnimplementedInterfaceMemberNotStatic, interfaceLocation, implementingType, interfaceMember, closestMismatch);
			return;
		}
		if (closestMismatch.DeclaredAccessibility != Accessibility.Public)
		{
			ErrorCode code = (interfaceMember.IsAccessor() ? ErrorCode.ERR_UnimplementedInterfaceAccessor : ErrorCode.ERR_CloseUnimplementedInterfaceMemberNotPublic);
			diagnostics.Add(code, interfaceLocation, implementingType, interfaceMember, closestMismatch);
			return;
		}
		if (HaveInitOnlyMismatch(interfaceMember, closestMismatch))
		{
			diagnostics.Add(ErrorCode.ERR_CloseUnimplementedInterfaceMemberWrongInitOnly, interfaceLocation, implementingType, interfaceMember, closestMismatch);
			return;
		}
		RefKind refKind = RefKind.None;
		TypeSymbol typeSymbol;
		switch (interfaceMember.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol obj2 = (MethodSymbol)interfaceMember;
			refKind = obj2.RefKind;
			typeSymbol = obj2.ReturnType;
			break;
		}
		case SymbolKind.Property:
		{
			PropertySymbol obj = (PropertySymbol)interfaceMember;
			refKind = obj.RefKind;
			typeSymbol = obj.Type;
			break;
		}
		case SymbolKind.Event:
			typeSymbol = ((EventSymbol)interfaceMember).Type;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(interfaceMember.Kind);
		}
		bool flag = false;
		switch (closestMismatch.Kind)
		{
		case SymbolKind.Method:
			flag = ((MethodSymbol)closestMismatch).RefKind != refKind;
			break;
		case SymbolKind.Property:
			flag = ((PropertySymbol)closestMismatch).RefKind != refKind;
			break;
		}
		DiagnosticInfo diagnosticInfo;
		if ((object)typeSymbol != null && (diagnosticInfo = typeSymbol.GetUseSiteInfo().DiagnosticInfo) != null && diagnosticInfo.DefaultSeverity == DiagnosticSeverity.Error)
		{
			diagnostics.Add(diagnosticInfo, interfaceLocation);
		}
		else if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_CloseUnimplementedInterfaceMemberWrongRefReturn, interfaceLocation, implementingType, interfaceMember, closestMismatch);
		}
		else if (interfaceMember is MethodSymbol methodSymbol && methodSymbol.IsOperator() != ((MethodSymbol)closestMismatch).IsOperator())
		{
			diagnostics.Add(ErrorCode.ERR_CloseUnimplementedInterfaceMemberOperatorMismatch, interfaceLocation, implementingType, interfaceMember, closestMismatch);
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_CloseUnimplementedInterfaceMemberWrongReturnType, interfaceLocation, implementingType, interfaceMember, closestMismatch, typeSymbol);
		}
	}

	internal static bool HaveInitOnlyMismatch(Symbol one, Symbol other)
	{
		if (!(one is MethodSymbol methodSymbol))
		{
			return false;
		}
		if (!(other is MethodSymbol methodSymbol2))
		{
			return false;
		}
		return methodSymbol.IsInitOnly != methodSymbol2.IsInitOnly;
	}

	private static Location GetInterfaceLocation(Symbol interfaceMember, TypeSymbol implementingType)
	{
		NamedTypeSymbol containingType = interfaceMember.ContainingType;
		SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol = null;
		if (implementingType.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics[containingType].Contains(containingType))
		{
			sourceMemberContainerTypeSymbol = implementingType as SourceMemberContainerTypeSymbol;
		}
		return sourceMemberContainerTypeSymbol?.GetImplementsLocation(containingType) ?? implementingType.GetFirstLocationOrNone();
	}

	private static bool ReportAnyMismatchedConstraints(MethodSymbol interfaceMethod, TypeSymbol implementingType, MethodSymbol implicitImpl, BindingDiagnosticBag diagnostics)
	{
		bool result = false;
		int arity = interfaceMethod.Arity;
		if (arity > 0)
		{
			ImmutableArray<TypeParameterSymbol> typeParameters = interfaceMethod.TypeParameters;
			ImmutableArray<TypeParameterSymbol> typeParameters2 = implicitImpl.TypeParameters;
			ImmutableArray<TypeWithAnnotations> to = IndexedTypeParameterSymbol.Take(arity);
			TypeMap typeMap = new TypeMap(typeParameters, to, allowAlpha: true);
			TypeMap typeMap2 = new TypeMap(typeParameters2, to, allowAlpha: true);
			for (int i = 0; i < arity; i++)
			{
				TypeParameterSymbol typeParameterSymbol = typeParameters[i];
				TypeParameterSymbol typeParameterSymbol2 = typeParameters2[i];
				if (!MemberSignatureComparer.HaveSameConstraints(typeParameterSymbol, typeMap, typeParameterSymbol2, typeMap2, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
				{
					diagnostics.Add(ErrorCode.ERR_ImplBadConstraints, GetImplicitImplementationDiagnosticLocation(interfaceMethod, implementingType, implicitImpl), typeParameterSymbol2.Name, implicitImpl, typeParameterSymbol.Name, interfaceMethod);
				}
				else if (!MemberSignatureComparer.HaveSameNullabilityInConstraints(typeParameterSymbol, typeMap, typeParameterSymbol2, typeMap2))
				{
					diagnostics.Add(ErrorCode.WRN_NullabilityMismatchInConstraintsOnImplicitImplementation, GetImplicitImplementationDiagnosticLocation(interfaceMethod, implementingType, implicitImpl), typeParameterSymbol2.Name, implicitImpl, typeParameterSymbol.Name, interfaceMethod);
				}
			}
		}
		return result;
	}

	internal static Location GetImplicitImplementationDiagnosticLocation(Symbol interfaceMember, TypeSymbol implementingType, Symbol member)
	{
		if (Equals(member.ContainingType, implementingType, TypeCompareKind.ConsiderEverything))
		{
			return member.GetFirstLocation();
		}
		NamedTypeSymbol containingType = interfaceMember.ContainingType;
		return (implementingType as SourceMemberContainerTypeSymbol)?.GetImplementsLocation(containingType) ?? implementingType.GetFirstLocation();
	}

	private static void FindPotentialImplicitImplementationMemberDeclaredInType(Symbol interfaceMember, bool implementingTypeIsFromSomeCompilation, TypeSymbol currType, out Symbol implicitImpl, out Symbol closeMismatch)
	{
		implicitImpl = null;
		closeMismatch = null;
		bool? flag = null;
		if (interfaceMember is MethodSymbol methodSymbol && interfaceMember.IsStatic)
		{
			MethodKind methodKind = methodSymbol.MethodKind;
			bool value = ((methodKind == MethodKind.Conversion || methodKind == MethodKind.UserDefinedOperator) ? true : false);
			flag = value;
		}
		foreach (Symbol member in currType.GetMembers(interfaceMember.Name))
		{
			if (member.Kind != interfaceMember.Kind)
			{
				continue;
			}
			bool value = flag.HasValue;
			if (value)
			{
				MethodKind methodKind = ((MethodSymbol)member).MethodKind;
				bool flag2 = ((methodKind == MethodKind.Conversion || methodKind == MethodKind.UserDefinedOperator) ? true : false);
				value = flag2 != (flag == true);
			}
			if (!value)
			{
				if (IsInterfaceMemberImplementation(member, interfaceMember, implementingTypeIsFromSomeCompilation))
				{
					implicitImpl = member;
					break;
				}
				if ((((object)closeMismatch == null) & implementingTypeIsFromSomeCompilation) && MemberSignatureComparer.CSharpCloseImplicitImplementationComparer.Equals(interfaceMember, member))
				{
					closeMismatch = member;
				}
			}
		}
	}

	private static bool IsInterfaceMemberImplementation(Symbol candidateMember, Symbol interfaceMember, bool implementingTypeIsFromSomeCompilation)
	{
		if (candidateMember.DeclaredAccessibility != Accessibility.Public || candidateMember.IsStatic != interfaceMember.IsStatic)
		{
			return false;
		}
		if (HaveInitOnlyMismatch(candidateMember, interfaceMember))
		{
			return false;
		}
		if (implementingTypeIsFromSomeCompilation)
		{
			return MemberSignatureComparer.CSharpImplicitImplementationComparer.Equals(interfaceMember, candidateMember);
		}
		return MemberSignatureComparer.RuntimeImplicitImplementationComparer.Equals(interfaceMember, candidateMember);
	}

	protected MultiDictionary<Symbol, Symbol>.ValueSet GetExplicitImplementationForInterfaceMember(Symbol interfaceMember)
	{
		InterfaceInfo interfaceInfo = GetInterfaceInfo();
		if (interfaceInfo == s_noInterfaces)
		{
			return default(MultiDictionary<Symbol, Symbol>.ValueSet);
		}
		if (interfaceInfo.explicitInterfaceImplementationMap == null)
		{
			Interlocked.CompareExchange(ref interfaceInfo.explicitInterfaceImplementationMap, MakeExplicitInterfaceImplementationMap(), null);
		}
		return interfaceInfo.explicitInterfaceImplementationMap[interfaceMember];
	}

	private MultiDictionary<Symbol, Symbol> MakeExplicitInterfaceImplementationMap()
	{
		MultiDictionary<Symbol, Symbol> multiDictionary = new MultiDictionary<Symbol, Symbol>(ExplicitInterfaceImplementationTargetMemberEqualityComparer.Instance);
		foreach (Symbol item in GetMembersUnordered())
		{
			foreach (Symbol explicitInterfaceImplementation in item.GetExplicitInterfaceImplementations())
			{
				multiDictionary.Add(explicitInterfaceImplementation, item);
			}
		}
		return multiDictionary;
	}

	protected MethodSymbol? GetBodyOfSynthesizedInterfaceMethodImpl(MethodSymbol interfaceMethod)
	{
		InterfaceInfo interfaceInfo = GetInterfaceInfo();
		if (interfaceInfo == s_noInterfaces)
		{
			return null;
		}
		if (interfaceInfo.synthesizedMethodImplMap == null)
		{
			Interlocked.CompareExchange(ref interfaceInfo.synthesizedMethodImplMap, makeSynthesizedMethodImplMap(), null);
		}
		if (interfaceInfo.synthesizedMethodImplMap.TryGetValue(interfaceMethod, out MethodSymbol value))
		{
			return value;
		}
		return null;
		ImmutableDictionary<MethodSymbol, MethodSymbol> makeSynthesizedMethodImplMap()
		{
			ImmutableDictionary<MethodSymbol, MethodSymbol>.Builder builder = ImmutableDictionary.CreateBuilder<MethodSymbol, MethodSymbol>(ExplicitInterfaceImplementationTargetMemberEqualityComparer.Instance);
			foreach (var (value2, key) in SynthesizedInterfaceMethodImpls())
			{
				builder.Add(key, value2);
			}
			return builder.ToImmutable();
		}
	}

	internal abstract IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls();

	private ImmutableHashSet<Symbol> ComputeAbstractMembers()
	{
		ImmutableHashSet<Symbol> immutableHashSet = ImmutableHashSet.Create<Symbol>();
		ImmutableHashSet<Symbol> immutableHashSet2 = ImmutableHashSet.Create<Symbol>();
		foreach (Symbol item in GetMembersUnordered())
		{
			if (IsAbstract && item.IsAbstract && item.Kind != SymbolKind.NamedType)
			{
				immutableHashSet = immutableHashSet.Add(item);
			}
			Symbol symbol = null;
			switch (item.Kind)
			{
			case SymbolKind.Method:
				symbol = ((MethodSymbol)item).OverriddenMethod;
				break;
			case SymbolKind.Property:
				symbol = ((PropertySymbol)item).OverriddenProperty;
				break;
			case SymbolKind.Event:
				symbol = ((EventSymbol)item).OverriddenEvent;
				break;
			}
			if ((object)symbol != null)
			{
				immutableHashSet2 = immutableHashSet2.Add(symbol);
			}
		}
		if ((object)BaseTypeNoUseSiteDiagnostics != null && BaseTypeNoUseSiteDiagnostics.IsAbstract)
		{
			foreach (Symbol abstractMember in BaseTypeNoUseSiteDiagnostics.AbstractMembers)
			{
				if (!immutableHashSet2.Contains(abstractMember))
				{
					immutableHashSet = immutableHashSet.Add(abstractMember);
				}
			}
		}
		return immutableHashSet;
	}

	[Obsolete("Use TypeWithAnnotations.Is method.", true)]
	internal bool Equals(TypeWithAnnotations other)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/TypeSymbol.cs", 2454);
	}

	public static bool Equals(TypeSymbol? left, TypeSymbol? right, TypeCompareKind comparison)
	{
		return left?.Equals(right, comparison) ?? ((object)right == null);
	}

	[Obsolete("Use 'TypeSymbol.Equals(TypeSymbol, TypeSymbol, TypeCompareKind)' method.", true)]
	public static bool operator ==(TypeSymbol left, TypeSymbol right)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/TypeSymbol.cs", 2471);
	}

	[Obsolete("Use 'TypeSymbol.Equals(TypeSymbol, TypeSymbol, TypeCompareKind)' method.", true)]
	public static bool operator !=(TypeSymbol left, TypeSymbol right)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/TypeSymbol.cs", 2475);
	}

	[Obsolete("Use 'TypeSymbol.Equals(TypeSymbol, TypeSymbol, TypeCompareKind)' method.", true)]
	public static bool operator ==(Symbol left, TypeSymbol right)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/TypeSymbol.cs", 2479);
	}

	[Obsolete("Use 'TypeSymbol.Equals(TypeSymbol, TypeSymbol, TypeCompareKind)' method.", true)]
	public static bool operator !=(Symbol left, TypeSymbol right)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/TypeSymbol.cs", 2483);
	}

	[Obsolete("Use 'TypeSymbol.Equals(TypeSymbol, TypeSymbol, TypeCompareKind)' method.", true)]
	public static bool operator ==(TypeSymbol left, Symbol right)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/TypeSymbol.cs", 2487);
	}

	[Obsolete("Use 'TypeSymbol.Equals(TypeSymbol, TypeSymbol, TypeCompareKind)' method.", true)]
	public static bool operator !=(TypeSymbol left, Symbol right)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/TypeSymbol.cs", 2491);
	}

	internal ITypeSymbol GetITypeSymbol(Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation)
	{
		if (nullableAnnotation == DefaultNullableAnnotation)
		{
			return (ITypeSymbol)base.ISymbol;
		}
		return CreateITypeSymbol(nullableAnnotation);
	}

	protected abstract ITypeSymbol CreateITypeSymbol(Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation);

	ITypeSymbol ITypeSymbolInternal.GetITypeSymbol()
	{
		return GetITypeSymbol(DefaultNullableAnnotation);
	}

	internal abstract bool HasInlineArrayAttribute(out int length);

	internal FieldSymbol? TryGetPossiblyUnsupportedByLanguageInlineArrayElementField()
	{
		FieldSymbol fieldSymbol = null;
		if (TypeKind == TypeKind.Struct)
		{
			foreach (FieldSymbol item in ((NamedTypeSymbol)this).OriginalDefinition.GetFieldsToEmit())
			{
				if (!item.IsStatic)
				{
					if ((object)fieldSymbol != null)
					{
						return null;
					}
					fieldSymbol = item;
				}
			}
		}
		if ((object)fieldSymbol != null && fieldSymbol.ContainingType.IsGenericType)
		{
			fieldSymbol = fieldSymbol.AsMember((NamedTypeSymbol)this);
		}
		return fieldSymbol;
	}

	internal FieldSymbol? TryGetInlineArrayElementField()
	{
		FieldSymbol fieldSymbol = TryGetPossiblyUnsupportedByLanguageInlineArrayElementField();
		if ((object)fieldSymbol == null || !IsInlineArrayElementFieldSupported(fieldSymbol))
		{
			return null;
		}
		return fieldSymbol;
	}

	internal static bool IsInlineArrayElementFieldSupported(FieldSymbol elementField)
	{
		if ((object)elementField != null && elementField.RefKind == RefKind.None)
		{
			return !elementField.IsFixedSizeBuffer;
		}
		return false;
	}
}
