using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class OverriddenOrHiddenMembersHelpers
{
	internal static OverriddenOrHiddenMembersResult MakeOverriddenOrHiddenMembers(this MethodSymbol member)
	{
		return MakeOverriddenOrHiddenMembersWorker(member);
	}

	internal static OverriddenOrHiddenMembersResult MakeOverriddenOrHiddenMembers(this PropertySymbol member)
	{
		return MakeOverriddenOrHiddenMembersWorker(member);
	}

	internal static OverriddenOrHiddenMembersResult MakeOverriddenOrHiddenMembers(this EventSymbol member)
	{
		return MakeOverriddenOrHiddenMembersWorker(member);
	}

	private static OverriddenOrHiddenMembersResult MakeOverriddenOrHiddenMembersWorker(Symbol member)
	{
		if (!CanOverrideOrHide(member))
		{
			return OverriddenOrHiddenMembersResult.Empty;
		}
		if (member.IsAccessor())
		{
			MethodSymbol methodSymbol = member as MethodSymbol;
			Symbol associatedSymbol = methodSymbol.AssociatedSymbol;
			if ((object)associatedSymbol != null)
			{
				if (associatedSymbol.Kind == SymbolKind.Property)
				{
					return MakePropertyAccessorOverriddenOrHiddenMembers(methodSymbol, (PropertySymbol)associatedSymbol);
				}
				return MakeEventAccessorOverriddenOrHiddenMembers(methodSymbol, (EventSymbol)associatedSymbol);
			}
		}
		NamedTypeSymbol containingType = member.ContainingType;
		bool dangerous_IsFromSomeCompilation = member.Dangerous_IsFromSomeCompilation;
		if (containingType.IsInterface)
		{
			return MakeInterfaceOverriddenOrHiddenMembers(member, dangerous_IsFromSomeCompilation);
		}
		FindOverriddenOrHiddenMembers(member, containingType, dangerous_IsFromSomeCompilation, out var hiddenBuilder, out var overriddenMembers);
		ImmutableArray<Symbol> hiddenMembers = hiddenBuilder?.ToImmutableAndFree() ?? ImmutableArray<Symbol>.Empty;
		return OverriddenOrHiddenMembersResult.Create(overriddenMembers, hiddenMembers);
	}

	private static void FindOverriddenOrHiddenMembers(Symbol member, NamedTypeSymbol containingType, bool memberIsFromSomeCompilation, out ArrayBuilder<Symbol> hiddenBuilder, out ImmutableArray<Symbol> overriddenMembers)
	{
		Symbol currTypeBestMatch = null;
		hiddenBuilder = null;
		Symbol symbol;
		if (!(member is MethodSymbol method))
		{
			if (member is PEPropertySymbol pEPropertySymbol)
			{
				if (!(pEPropertySymbol.GetMethod is PEMethodSymbol { ExplicitlyOverriddenClassMethod: { AssociatedSymbol: PropertySymbol associatedSymbol } }))
				{
					goto IL_00a0;
				}
				symbol = associatedSymbol;
			}
			else
			{
				if (!(member is RetargetingPropertySymbol { GetMethod: RetargetingMethodSymbol { ExplicitlyOverriddenClassMethod: { AssociatedSymbol: PropertySymbol associatedSymbol2 } } }))
				{
					goto IL_00a0;
				}
				symbol = associatedSymbol2;
			}
		}
		else
		{
			symbol = KnownOverriddenClassMethod(method);
		}
		goto IL_00a3;
		IL_00a0:
		symbol = null;
		goto IL_00a3;
		IL_00a3:
		Symbol knownOverriddenMember = symbol;
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = containingType.BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null && (object)currTypeBestMatch == null && hiddenBuilder == null)
		{
			FindOverriddenOrHiddenMembersInType(member, memberIsFromSomeCompilation, containingType, knownOverriddenMember, baseTypeNoUseSiteDiagnostics, out currTypeBestMatch, out var _, out hiddenBuilder);
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		FindRelatedMembers(member.IsOverride, memberIsFromSomeCompilation, member, currTypeBestMatch, out overriddenMembers, ref hiddenBuilder);
	}

	public static Symbol FindFirstHiddenMemberIfAny(Symbol member, bool memberIsFromSomeCompilation)
	{
		FindOverriddenOrHiddenMembers(member, member.ContainingType, memberIsFromSomeCompilation, out var hiddenBuilder, out var _);
		Symbol? result = hiddenBuilder?.FirstOrDefault();
		hiddenBuilder?.Free();
		return result;
	}

	private static MethodSymbol KnownOverriddenClassMethod(MethodSymbol method)
	{
		if (!(method is PEMethodSymbol { ExplicitlyOverriddenClassMethod: var explicitlyOverriddenClassMethod }))
		{
			if (!(method is RetargetingMethodSymbol { ExplicitlyOverriddenClassMethod: var explicitlyOverriddenClassMethod2 }))
			{
				return null;
			}
			return explicitlyOverriddenClassMethod2;
		}
		return explicitlyOverriddenClassMethod;
	}

	private static OverriddenOrHiddenMembersResult MakePropertyAccessorOverriddenOrHiddenMembers(MethodSymbol accessor, PropertySymbol associatedProperty)
	{
		bool flag = accessor.MethodKind == MethodKind.PropertyGet;
		MethodSymbol methodSymbol = null;
		ArrayBuilder<Symbol> builder = null;
		OverriddenOrHiddenMembersResult overriddenOrHiddenMembers = associatedProperty.OverriddenOrHiddenMembers;
		foreach (Symbol hiddenMember in overriddenOrHiddenMembers.HiddenMembers)
		{
			if (hiddenMember.Kind == SymbolKind.Property)
			{
				PropertySymbol propertySymbol = (PropertySymbol)hiddenMember;
				MethodSymbol methodSymbol2 = (flag ? propertySymbol.GetMethod : propertySymbol.SetMethod);
				if ((object)methodSymbol2 != null)
				{
					AccessOrGetInstance(ref builder).Add(methodSymbol2);
				}
			}
		}
		if (overriddenOrHiddenMembers.OverriddenMembers.Any())
		{
			PropertySymbol property = (PropertySymbol)overriddenOrHiddenMembers.OverriddenMembers[0];
			MethodSymbol methodSymbol3 = (flag ? property.GetOwnOrInheritedGetMethod() : property.GetOwnOrInheritedSetMethod());
			if ((object)methodSymbol3 != null)
			{
				methodSymbol = methodSymbol3;
			}
		}
		bool accessorIsFromSomeCompilation = accessor.Dangerous_IsFromSomeCompilation;
		ImmutableArray<Symbol> overriddenMembers = ImmutableArray<Symbol>.Empty;
		if ((object)methodSymbol != null && IsOverriddenSymbolAccessible(methodSymbol, accessor.ContainingType) && isAccessorOverride(accessor, methodSymbol))
		{
			FindRelatedMembers(accessor.IsOverride, accessorIsFromSomeCompilation, accessor, methodSymbol, out overriddenMembers, ref builder);
		}
		ImmutableArray<Symbol> hiddenMembers = builder?.ToImmutableAndFree() ?? ImmutableArray<Symbol>.Empty;
		return OverriddenOrHiddenMembersResult.Create(overriddenMembers, hiddenMembers);
		bool isAccessorOverride(MethodSymbol methodSymbol4, MethodSymbol overriddenAccessor)
		{
			if (accessorIsFromSomeCompilation)
			{
				return MemberSignatureComparer.CSharpAccessorOverrideComparer.Equals(methodSymbol4, overriddenAccessor);
			}
			if (overriddenAccessor.Equals(KnownOverriddenClassMethod(methodSymbol4), TypeCompareKind.AllIgnoreOptions))
			{
				return true;
			}
			return MemberSignatureComparer.RuntimeSignatureComparer.Equals(methodSymbol4, overriddenAccessor);
		}
	}

	private static OverriddenOrHiddenMembersResult MakeEventAccessorOverriddenOrHiddenMembers(MethodSymbol accessor, EventSymbol associatedEvent)
	{
		bool flag = accessor.MethodKind == MethodKind.EventAdd;
		MethodSymbol methodSymbol = null;
		ArrayBuilder<Symbol> builder = null;
		OverriddenOrHiddenMembersResult overriddenOrHiddenMembers = associatedEvent.OverriddenOrHiddenMembers;
		foreach (Symbol hiddenMember in overriddenOrHiddenMembers.HiddenMembers)
		{
			if (hiddenMember.Kind == SymbolKind.Event)
			{
				EventSymbol eventSymbol = (EventSymbol)hiddenMember;
				MethodSymbol methodSymbol2 = (flag ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
				if ((object)methodSymbol2 != null)
				{
					AccessOrGetInstance(ref builder).Add(methodSymbol2);
				}
			}
		}
		if (overriddenOrHiddenMembers.OverriddenMembers.Any())
		{
			MethodSymbol ownOrInheritedAccessor = ((EventSymbol)overriddenOrHiddenMembers.OverriddenMembers[0]).GetOwnOrInheritedAccessor(flag);
			if ((object)ownOrInheritedAccessor != null)
			{
				methodSymbol = ownOrInheritedAccessor;
			}
		}
		bool dangerous_IsFromSomeCompilation = accessor.Dangerous_IsFromSomeCompilation;
		ImmutableArray<Symbol> overriddenMembers = ImmutableArray<Symbol>.Empty;
		if ((object)methodSymbol != null && IsOverriddenSymbolAccessible(methodSymbol, accessor.ContainingType) && (dangerous_IsFromSomeCompilation ? MemberSignatureComparer.CSharpAccessorOverrideComparer.Equals(accessor, methodSymbol) : MemberSignatureComparer.RuntimeSignatureComparer.Equals(accessor, methodSymbol)))
		{
			FindRelatedMembers(accessor.IsOverride, dangerous_IsFromSomeCompilation, accessor, methodSymbol, out overriddenMembers, ref builder);
		}
		ImmutableArray<Symbol> hiddenMembers = builder?.ToImmutableAndFree() ?? ImmutableArray<Symbol>.Empty;
		return OverriddenOrHiddenMembersResult.Create(overriddenMembers, hiddenMembers);
	}

	internal static OverriddenOrHiddenMembersResult MakeInterfaceOverriddenOrHiddenMembers(Symbol member, bool memberIsFromSomeCompilation)
	{
		NamedTypeSymbol containingType = member.ContainingType;
		PooledHashSet<NamedTypeSymbol> instance = PooledHashSet<NamedTypeSymbol>.GetInstance();
		PooledHashSet<NamedTypeSymbol> instance2 = PooledHashSet<NamedTypeSymbol>.GetInstance();
		ArrayBuilder<Symbol> builder = null;
		foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in containingType.AllInterfacesNoUseSiteDiagnostics)
		{
			if (instance2.Contains(allInterfacesNoUseSiteDiagnostic))
			{
				continue;
			}
			FindOverriddenOrHiddenMembersInType(member, memberIsFromSomeCompilation, containingType, null, allInterfacesNoUseSiteDiagnostic, out var currTypeBestMatch, out var currTypeHasSameKindNonMatch, out var hiddenBuilder);
			bool flag = (object)currTypeBestMatch != null;
			if (flag)
			{
				foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic2 in allInterfacesNoUseSiteDiagnostic.AllInterfacesNoUseSiteDiagnostics)
				{
					instance2.Add(allInterfacesNoUseSiteDiagnostic2);
				}
				AccessOrGetInstance(ref builder).Add(currTypeBestMatch);
			}
			if (hiddenBuilder != null)
			{
				if (!instance.Contains(allInterfacesNoUseSiteDiagnostic))
				{
					if (!flag)
					{
						foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic3 in allInterfacesNoUseSiteDiagnostic.AllInterfacesNoUseSiteDiagnostics)
						{
							instance2.Add(allInterfacesNoUseSiteDiagnostic3);
						}
					}
					AccessOrGetInstance(ref builder).AddRange(hiddenBuilder);
				}
				hiddenBuilder.Free();
			}
			else if (currTypeHasSameKindNonMatch && !flag)
			{
				foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic4 in allInterfacesNoUseSiteDiagnostic.AllInterfacesNoUseSiteDiagnostics)
				{
					instance.Add(allInterfacesNoUseSiteDiagnostic4);
				}
			}
		}
		instance.Free();
		instance2.Free();
		ImmutableArray<Symbol> overriddenMembers = ImmutableArray<Symbol>.Empty;
		if (builder != null)
		{
			ArrayBuilder<Symbol> hiddenBuilder2 = null;
			foreach (Symbol item in builder)
			{
				FindRelatedMembers(member.IsOverride, memberIsFromSomeCompilation, member, item, out overriddenMembers, ref hiddenBuilder2);
			}
			builder.Free();
			builder = hiddenBuilder2;
		}
		ImmutableArray<Symbol> hiddenMembers = builder?.ToImmutableAndFree() ?? ImmutableArray<Symbol>.Empty;
		return OverriddenOrHiddenMembersResult.Create(overriddenMembers, hiddenMembers);
	}

	private static void FindOverriddenOrHiddenMembersInType(Symbol member, bool memberIsFromSomeCompilation, NamedTypeSymbol memberContainingType, Symbol knownOverriddenMember, NamedTypeSymbol currType, out Symbol currTypeBestMatch, out bool currTypeHasSameKindNonMatch, out ArrayBuilder<Symbol> hiddenBuilder)
	{
		currTypeBestMatch = null;
		currTypeHasSameKindNonMatch = false;
		hiddenBuilder = null;
		bool flag = false;
		int num = int.MaxValue;
		IEqualityComparer<Symbol> equalityComparer = (memberIsFromSomeCompilation ? MemberSignatureComparer.CSharpCustomModifierOverrideComparer : MemberSignatureComparer.RuntimePlusRefOutSignatureComparer);
		IEqualityComparer<Symbol> equalityComparer2 = (memberIsFromSomeCompilation ? MemberSignatureComparer.CSharpOverrideComparer : MemberSignatureComparer.RuntimeSignatureComparer);
		SymbolKind kind = member.Kind;
		int memberArity = member.GetMemberArity();
		foreach (Symbol member2 in currType.GetMembers(member.Name))
		{
			if (!IsOverriddenSymbolAccessible(member2, memberContainingType) || (member2.IsAccessor() && !((MethodSymbol)member2).IsIndexedPropertyAccessor()))
			{
				continue;
			}
			if (member2.Kind != kind)
			{
				int memberArity2 = member2.GetMemberArity();
				if (memberArity2 == memberArity || (kind == SymbolKind.Method && memberArity2 == 0))
				{
					AddHiddenMemberIfApplicable(ref hiddenBuilder, member, member2);
				}
			}
			else
			{
				if (flag)
				{
					continue;
				}
				switch (kind)
				{
				case SymbolKind.Field:
					flag = true;
					currTypeBestMatch = member2;
					continue;
				case SymbolKind.NamedType:
					if (member2.GetMemberArity() == memberArity)
					{
						flag = true;
						currTypeBestMatch = member2;
					}
					continue;
				}
				if (member2.Equals(knownOverriddenMember, TypeCompareKind.AllIgnoreOptions))
				{
					flag = true;
					currTypeBestMatch = member2;
					continue;
				}
				if (!(knownOverriddenMember == null))
				{
					continue;
				}
				if (equalityComparer.Equals(member, member2))
				{
					flag = true;
					currTypeBestMatch = member2;
				}
				else if (equalityComparer2.Equals(member, member2))
				{
					int num2 = CustomModifierCount(member2);
					if (num2 < num)
					{
						num = num2;
						currTypeBestMatch = member2;
					}
				}
				else
				{
					currTypeHasSameKindNonMatch = true;
				}
			}
		}
		if (kind == SymbolKind.Field || kind == SymbolKind.NamedType || !(flag & memberIsFromSomeCompilation) || !member.IsDefinition || !TypeOrReturnTypeHasCustomModifiers(currTypeBestMatch))
		{
			return;
		}
		Symbol symbol = currTypeBestMatch;
		foreach (Symbol member3 in currType.GetMembers(member.Name))
		{
			if (member3.Kind == currTypeBestMatch.Kind && (object)member3 != currTypeBestMatch && MemberSignatureComparer.CSharpOverrideComparer.Equals(member3, currTypeBestMatch))
			{
				int num3 = CustomModifierCount(member3);
				if (num3 < num)
				{
					num = num3;
					symbol = member3;
				}
			}
		}
		currTypeBestMatch = symbol;
	}

	private static void FindRelatedMembers(bool isOverride, bool overridingMemberIsFromSomeCompilation, Symbol overridingMember, Symbol representativeMember, out ImmutableArray<Symbol> overriddenMembers, ref ArrayBuilder<Symbol> hiddenBuilder)
	{
		overriddenMembers = ImmutableArray<Symbol>.Empty;
		if ((object)representativeMember == null)
		{
			return;
		}
		bool flag = representativeMember.Kind != SymbolKind.Field && representativeMember.Kind != SymbolKind.NamedType && (!representativeMember.ContainingType.IsDefinition || representativeMember.IsIndexer());
		if (isOverride)
		{
			if (flag)
			{
				ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
				instance.Add(representativeMember);
				FindOtherOverriddenMethodsInContainingType(representativeMember, overridingMemberIsFromSomeCompilation, instance);
				overriddenMembers = instance.ToImmutableAndFree();
			}
			else
			{
				overriddenMembers = ImmutableArray.Create(representativeMember);
			}
		}
		else
		{
			AddHiddenMemberIfApplicable(ref hiddenBuilder, overridingMember, representativeMember);
			if (flag)
			{
				FindOtherHiddenMembersInContainingType(overridingMember, representativeMember, ref hiddenBuilder);
			}
		}
	}

	private static void AddHiddenMemberIfApplicable(ref ArrayBuilder<Symbol> hiddenBuilder, Symbol hidingMember, Symbol hiddenMember)
	{
		if (hiddenMember.Kind != SymbolKind.Method || ((MethodSymbol)hiddenMember).CanBeHiddenByMember(hidingMember))
		{
			AccessOrGetInstance(ref hiddenBuilder).Add(hiddenMember);
		}
	}

	private static ArrayBuilder<T> AccessOrGetInstance<T>(ref ArrayBuilder<T> builder)
	{
		if (builder == null)
		{
			builder = ArrayBuilder<T>.GetInstance();
		}
		return builder;
	}

	private static void FindOtherOverriddenMethodsInContainingType(Symbol representativeMember, bool overridingMemberIsFromSomeCompilation, ArrayBuilder<Symbol> overriddenBuilder)
	{
		int num = -1;
		foreach (Symbol member in representativeMember.ContainingType.GetMembers(representativeMember.Name))
		{
			if (member.Kind != representativeMember.Kind || !(member != representativeMember))
			{
				continue;
			}
			if (overridingMemberIsFromSomeCompilation)
			{
				if (num < 0)
				{
					num = representativeMember.CustomModifierCount();
				}
				if (MemberSignatureComparer.CSharpOverrideComparer.Equals(member, representativeMember) && member.CustomModifierCount() == num)
				{
					overriddenBuilder.Add(member);
				}
			}
			else if (MemberSignatureComparer.CSharpCustomModifierOverrideComparer.Equals(member, representativeMember))
			{
				overriddenBuilder.Add(member);
			}
		}
	}

	private static void FindOtherHiddenMembersInContainingType(Symbol hidingMember, Symbol representativeMember, ref ArrayBuilder<Symbol> hiddenBuilder)
	{
		IEqualityComparer<Symbol> cSharpCustomModifierOverrideComparer = MemberSignatureComparer.CSharpCustomModifierOverrideComparer;
		foreach (Symbol member in representativeMember.ContainingType.GetMembers(representativeMember.Name))
		{
			if (member.Kind == representativeMember.Kind && member != representativeMember && cSharpCustomModifierOverrideComparer.Equals(member, representativeMember))
			{
				AddHiddenMemberIfApplicable(ref hiddenBuilder, hidingMember, member);
			}
		}
	}

	private static bool CanOverrideOrHide(Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Property:
			return !member.IsExplicitInterfaceImplementation();
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			if (MethodSymbol.CanOverrideOrHide(methodSymbol.MethodKind))
			{
				return (object)methodSymbol == methodSymbol.ConstructedFrom;
			}
			return false;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	private static bool TypeOrReturnTypeHasCustomModifiers(Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			TypeWithAnnotations returnTypeWithAnnotations = methodSymbol.ReturnTypeWithAnnotations;
			if (!returnTypeWithAnnotations.CustomModifiers.Any() && !methodSymbol.RefCustomModifiers.Any())
			{
				return returnTypeWithAnnotations.Type.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds: false);
			}
			return true;
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)member;
			TypeWithAnnotations typeWithAnnotations = propertySymbol.TypeWithAnnotations;
			if (!typeWithAnnotations.CustomModifiers.Any() && !propertySymbol.RefCustomModifiers.Any())
			{
				return typeWithAnnotations.Type.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds: false);
			}
			return true;
		}
		case SymbolKind.Event:
			return ((EventSymbol)member).Type.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds: false);
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	private static int CustomModifierCount(Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).CustomModifierCount(), 
			SymbolKind.Property => ((PropertySymbol)member).CustomModifierCount(), 
			SymbolKind.Event => ((EventSymbol)member).Type.CustomModifierCount(), 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static bool RequiresExplicitOverride(this MethodSymbol method, out bool warnAmbiguous)
	{
		warnAmbiguous = false;
		if (!method.IsOverride)
		{
			return false;
		}
		MethodSymbol overriddenMethod = method.OverriddenMethod;
		if ((object)overriddenMethod == null)
		{
			return false;
		}
		MethodSymbol firstRuntimeOverriddenMethodIgnoringNewSlot = method.GetFirstRuntimeOverriddenMethodIgnoringNewSlot(out var wasAmbiguous);
		if (overriddenMethod == firstRuntimeOverriddenMethodIgnoringNewSlot && !wasAmbiguous)
		{
			return false;
		}
		if (method.ContainingAssembly.RuntimeSupportsCovariantReturnsOfClasses)
		{
			return true;
		}
		if (!method.ReturnType.Equals(overriddenMethod.ReturnType, TypeCompareKind.AllIgnoreOptions))
		{
			return true;
		}
		if (!overriddenMethod.MethodHasRuntimeCollision())
		{
			return true;
		}
		bool flag = overriddenMethod.IsDefinition || overriddenMethod.OriginalDefinition.MethodHasRuntimeCollision();
		warnAmbiguous = !flag;
		if (!overriddenMethod.ContainingType.Equals(firstRuntimeOverriddenMethodIgnoringNewSlot.ContainingType, TypeCompareKind.CLRSignatureCompareOptions))
		{
			return true;
		}
		if (overriddenMethod != firstRuntimeOverriddenMethodIgnoringNewSlot)
		{
			return method.IsAccessor() != firstRuntimeOverriddenMethodIgnoringNewSlot.IsAccessor();
		}
		return false;
	}

	internal static bool MethodHasRuntimeCollision(this MethodSymbol method)
	{
		foreach (Symbol member in method.ContainingType.GetMembers(method.Name))
		{
			if (member != method && MemberSignatureComparer.RuntimeSignatureComparer.Equals(member, method))
			{
				return true;
			}
		}
		return false;
	}

	internal static MethodSymbol GetFirstRuntimeOverriddenMethodIgnoringNewSlot(this MethodSymbol method, out bool wasAmbiguous)
	{
		wasAmbiguous = false;
		if (!method.IsMetadataVirtual(MethodSymbol.IsMetadataVirtualOption.IgnoreInterfaceImplementationChanges) || method.IsStatic)
		{
			return null;
		}
		NamedTypeSymbol containingType = method.ContainingType;
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = containingType.BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			MethodSymbol methodSymbol = null;
			foreach (Symbol member in baseTypeNoUseSiteDiagnostics.GetMembers(method.Name))
			{
				if (member.Kind != SymbolKind.Method || !IsOverriddenSymbolAccessible(member, containingType) || !MemberSignatureComparer.RuntimeSignatureComparer.Equals(method, member))
				{
					continue;
				}
				MethodSymbol methodSymbol2 = (MethodSymbol)member;
				if (methodSymbol2.IsMetadataVirtual(MethodSymbol.IsMetadataVirtualOption.IgnoreInterfaceImplementationChanges))
				{
					if ((object)methodSymbol != null)
					{
						wasAmbiguous = true;
						return methodSymbol;
					}
					methodSymbol = methodSymbol2;
				}
			}
			if ((object)methodSymbol != null)
			{
				return methodSymbol;
			}
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		return null;
	}

	private static bool IsOverriddenSymbolAccessible(Symbol overridden, NamedTypeSymbol overridingContainingType)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		return AccessCheck.IsSymbolAccessible(overridden.OriginalDefinition, overridingContainingType.OriginalDefinition, ref useSiteInfo);
	}
}
