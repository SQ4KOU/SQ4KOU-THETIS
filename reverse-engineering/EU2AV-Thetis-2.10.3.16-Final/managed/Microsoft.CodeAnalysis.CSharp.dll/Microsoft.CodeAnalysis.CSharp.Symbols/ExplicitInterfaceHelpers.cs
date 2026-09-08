using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ExplicitInterfaceHelpers
{
	public static string GetMemberName(Binder binder, SyntaxTokenList modifiers, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierOpt, string name)
	{
		TypeSymbol explicitInterfaceTypeOpt;
		string aliasQualifierOpt;
		return GetMemberNameAndInterfaceSymbol(binder, modifiers, explicitInterfaceSpecifierOpt, name, BindingDiagnosticBag.Discarded, out explicitInterfaceTypeOpt, out aliasQualifierOpt);
	}

	public static string GetMemberNameAndInterfaceSymbol(Binder binder, SyntaxTokenList modifiers, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierOpt, string name, BindingDiagnosticBag diagnostics, out TypeSymbol explicitInterfaceTypeOpt, out string aliasQualifierOpt)
	{
		if (explicitInterfaceSpecifierOpt == null)
		{
			explicitInterfaceTypeOpt = null;
			aliasQualifierOpt = null;
			return name;
		}
		binder = binder.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.SuppressObsoleteChecks);
		binder = binder.SetOrClearUnsafeRegionIfNecessary(modifiers);
		NameSyntax name2 = explicitInterfaceSpecifierOpt.Name;
		explicitInterfaceTypeOpt = binder.BindType(name2, diagnostics).Type;
		aliasQualifierOpt = name2.GetAliasQualifierOpt();
		return GetMemberName(name, explicitInterfaceTypeOpt, aliasQualifierOpt);
	}

	public static string GetMemberName(string name, TypeSymbol explicitInterfaceTypeOpt, string aliasQualifierOpt)
	{
		if ((object)explicitInterfaceTypeOpt == null)
		{
			return name;
		}
		string text = explicitInterfaceTypeOpt.ToDisplayString(SymbolDisplayFormat.ExplicitInterfaceImplementationFormat);
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		if (!string.IsNullOrEmpty(aliasQualifierOpt))
		{
			builder.Append(aliasQualifierOpt);
			builder.Append("::");
		}
		string text2 = text;
		foreach (char c in text2)
		{
			if (c != ' ')
			{
				builder.Append(c);
			}
		}
		builder.Append('.');
		builder.Append(name);
		return instance.ToStringAndFree();
	}

	public static string GetMethodNameWithoutInterfaceName(this MethodSymbol method)
	{
		if (method.MethodKind != MethodKind.ExplicitInterfaceImplementation)
		{
			return method.Name;
		}
		return GetMemberNameWithoutInterfaceName(method.Name);
	}

	public static string GetMemberNameWithoutInterfaceName(string fullName)
	{
		int num = fullName.LastIndexOf('.');
		if (num <= 0)
		{
			return fullName;
		}
		return fullName.Substring(num + 1);
	}

	public static ImmutableArray<T> SubstituteExplicitInterfaceImplementations<T>(ImmutableArray<T> unsubstitutedExplicitInterfaceImplementations, TypeMap map) where T : Symbol
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		foreach (T item in unsubstitutedExplicitInterfaceImplementations)
		{
			instance.Add(SubstituteExplicitInterfaceImplementation(item, map));
		}
		return instance.ToImmutableAndFree();
	}

	public static T SubstituteExplicitInterfaceImplementation<T>(T unsubstitutedPropertyImplemented, TypeMap map) where T : Symbol
	{
		NamedTypeSymbol containingType = unsubstitutedPropertyImplemented.ContainingType;
		NamedTypeSymbol namedTypeSymbol = map.SubstituteNamedType(containingType);
		string name = unsubstitutedPropertyImplemented.Name;
		foreach (Symbol member in namedTypeSymbol.GetMembers(name))
		{
			if (member.OriginalDefinition == unsubstitutedPropertyImplemented.OriginalDefinition)
			{
				return (T)member;
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/ExplicitInterfaceHelpers.cs", 144);
	}

	internal static MethodSymbol FindExplicitlyImplementedMethod(this MethodSymbol implementingMethod, bool isOperator, TypeSymbol explicitInterfaceType, string interfaceMethodName, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax, BindingDiagnosticBag diagnostics)
	{
		return (MethodSymbol)FindExplicitlyImplementedMember(implementingMethod, isOperator, explicitInterfaceType, interfaceMethodName, explicitInterfaceSpecifierSyntax, diagnostics);
	}

	internal static PropertySymbol FindExplicitlyImplementedProperty(this PropertySymbol implementingProperty, TypeSymbol explicitInterfaceType, string interfacePropertyName, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax, BindingDiagnosticBag diagnostics)
	{
		return (PropertySymbol)FindExplicitlyImplementedMember(implementingProperty, isOperator: false, explicitInterfaceType, interfacePropertyName, explicitInterfaceSpecifierSyntax, diagnostics);
	}

	internal static EventSymbol FindExplicitlyImplementedEvent(this EventSymbol implementingEvent, TypeSymbol explicitInterfaceType, string interfaceEventName, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax, BindingDiagnosticBag diagnostics)
	{
		return (EventSymbol)FindExplicitlyImplementedMember(implementingEvent, isOperator: false, explicitInterfaceType, interfaceEventName, explicitInterfaceSpecifierSyntax, diagnostics);
	}

	private static Symbol FindExplicitlyImplementedMember(Symbol implementingMember, bool isOperator, TypeSymbol explicitInterfaceType, string interfaceMemberName, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax, BindingDiagnosticBag diagnostics)
	{
		if ((object)explicitInterfaceType == null)
		{
			return null;
		}
		Location memberLocation = implementingMember.GetFirstLocation();
		NamedTypeSymbol containingType = implementingMember.ContainingType;
		TypeKind typeKind = containingType.TypeKind;
		if (typeKind != TypeKind.Class && typeKind != TypeKind.Interface && typeKind != TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitInterfaceImplementationInNonClassOrStruct, memberLocation, implementingMember);
			return null;
		}
		if (!explicitInterfaceType.IsInterfaceType())
		{
			SourceLocation location = new SourceLocation(explicitInterfaceSpecifierSyntax.Name);
			diagnostics.Add(ErrorCode.ERR_ExplicitInterfaceImplementationNotInterface, location, explicitInterfaceType);
			return null;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)explicitInterfaceType;
		MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>.ValueSet valueSet = containingType.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics[namedTypeSymbol];
		int count = valueSet.Count;
		if (count == 0 || !valueSet.Contains(namedTypeSymbol, SymbolEqualityComparer.ObliviousNullableModifierMatchesAny))
		{
			SourceLocation location2 = new SourceLocation(explicitInterfaceSpecifierSyntax.Name);
			if (count > 0 && valueSet.Contains(namedTypeSymbol, SymbolEqualityComparer.IgnoringNullable))
			{
				diagnostics.Add(ErrorCode.WRN_NullabilityMismatchInExplicitlyImplementedInterface, location2);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_ClassDoesntImplementInterface, location2, implementingMember, namedTypeSymbol);
			}
		}
		bool flag = false;
		bool flag2 = false;
		Symbol symbol = null;
		Symbol symbol2 = null;
		if ((object)containingType == namedTypeSymbol.OriginalDefinition)
		{
			return null;
		}
		bool flag3 = implementingMember.HasParamsParameter();
		foreach (Symbol member in namedTypeSymbol.GetMembers(interfaceMemberName))
		{
			if (member.Kind != implementingMember.Kind || !member.IsImplementableInterfaceMember())
			{
				continue;
			}
			MethodSymbol methodSymbol = member as MethodSymbol;
			bool flag4 = (object)methodSymbol != null;
			if (flag4)
			{
				MethodKind methodKind = methodSymbol.MethodKind;
				bool flag5 = ((methodKind == MethodKind.Conversion || methodKind == MethodKind.UserDefinedOperator) ? true : false);
				flag4 = flag5 != isOperator;
			}
			if (flag4 || !MemberSignatureComparer.ExplicitImplementationWithoutReturnTypeComparer.Equals(implementingMember, member))
			{
				continue;
			}
			TypeMap typeMap = MemberSignatureComparer.GetTypeMap(implementingMember);
			TypeMap typeMap2 = MemberSignatureComparer.GetTypeMap(member);
			if (MemberSignatureComparer.HaveSameReturnTypes(implementingMember, typeMap, member, typeMap2, TypeCompareKind.AllIgnoreOptions))
			{
				flag2 = true;
				if (!member.IsAccessor() || ((MethodSymbol)member).IsIndexedPropertyAccessor())
				{
					if (member.MustCallMethodsDirectly())
					{
						diagnostics.Add(ErrorCode.ERR_BogusExplicitImpl, memberLocation, implementingMember, member);
					}
					else if (flag3 && !member.HasParamsParameter())
					{
						diagnostics.Add(ErrorCode.ERR_ExplicitImplParams, memberLocation, implementingMember, member);
					}
					symbol2 = member;
					break;
				}
				diagnostics.Add(ErrorCode.ERR_ExplicitMethodImplAccessor, memberLocation, implementingMember, member);
			}
			else
			{
				flag = true;
				symbol = member;
			}
		}
		if (!flag2)
		{
			if (flag)
			{
				ErrorCode code = ((implementingMember.Kind == SymbolKind.Method) ? ErrorCode.ERR_ExplicitInterfaceMemberReturnTypeMismatch : ErrorCode.ERR_ExplicitInterfaceMemberTypeMismatch);
				TypeWithAnnotations typeOrReturnType = symbol.GetTypeOrReturnType();
				diagnostics.Add(code, memberLocation, implementingMember, typeOrReturnType, symbol);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_InterfaceMemberNotFound, memberLocation, implementingMember);
			}
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo;
		if ((object)symbol2 != null)
		{
			useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, implementingMember.ContainingAssembly);
			if (!AccessCheck.IsSymbolAccessible(symbol2, implementingMember.ContainingType, ref useSiteInfo))
			{
				diagnostics.Add(ErrorCode.ERR_BadAccess, memberLocation, symbol2);
			}
			else
			{
				switch (symbol2.Kind)
				{
				case SymbolKind.Property:
				{
					PropertySymbol obj2 = (PropertySymbol)symbol2;
					checkAccessorIsAccessibleIfImplementable(obj2.GetMethod);
					checkAccessorIsAccessibleIfImplementable(obj2.SetMethod);
					break;
				}
				case SymbolKind.Event:
				{
					EventSymbol obj = (EventSymbol)symbol2;
					checkAccessorIsAccessibleIfImplementable(obj.AddMethod);
					checkAccessorIsAccessibleIfImplementable(obj.RemoveMethod);
					break;
				}
				}
			}
			diagnostics.Add(memberLocation, useSiteInfo);
		}
		return symbol2;
		void checkAccessorIsAccessibleIfImplementable(MethodSymbol accessor)
		{
			if (accessor.IsImplementable() && !AccessCheck.IsSymbolAccessible(accessor, implementingMember.ContainingType, ref useSiteInfo))
			{
				diagnostics.Add(ErrorCode.ERR_BadAccess, memberLocation, accessor);
			}
		}
	}

	internal static void FindExplicitlyImplementedMemberVerification(this Symbol implementingMember, Symbol implementedMember, BindingDiagnosticBag diagnostics)
	{
		if ((object)implementedMember != null)
		{
			if (implementingMember.ContainsTupleNames() && MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(implementingMember, implementedMember))
			{
				Location firstLocation = implementingMember.GetFirstLocation();
				diagnostics.Add(ErrorCode.ERR_ImplBadTupleNames, firstLocation, implementingMember, implementedMember);
			}
			FindExplicitImplementationCollisions(implementingMember, implementedMember, diagnostics);
			if (implementedMember.IsStatic && !implementingMember.ContainingAssembly.RuntimeSupportsStaticAbstractMembersInInterfaces)
			{
				diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportStaticAbstractMembersInInterfaces, implementingMember.GetFirstLocation());
			}
		}
	}

	private static void FindExplicitImplementationCollisions(Symbol implementingMember, Symbol implementedMember, BindingDiagnosticBag diagnostics)
	{
		if ((object)implementedMember == null)
		{
			return;
		}
		NamedTypeSymbol containingType = implementedMember.ContainingType;
		bool isDefinition = containingType.IsDefinition;
		foreach (Symbol member in containingType.GetMembers(implementedMember.Name))
		{
			if (member.Kind != implementingMember.Kind || !(implementedMember != member))
			{
				continue;
			}
			if (!isDefinition && MemberSignatureComparer.RuntimeSignatureComparer.Equals(implementedMember, member))
			{
				bool flag = false;
				ImmutableArray<ParameterSymbol> parameters = implementedMember.GetParameters();
				ImmutableArray<ParameterSymbol> parameters2 = member.GetParameters();
				int length = parameters.Length;
				for (int i = 0; i < length; i++)
				{
					if (parameters[i].RefKind != parameters2[i].RefKind)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					diagnostics.Add(ErrorCode.ERR_ExplicitImplCollisionOnRefOut, containingType.GetFirstLocation(), containingType, implementedMember);
				}
				else
				{
					diagnostics.Add(ErrorCode.WRN_ExplicitImplCollision, implementingMember.GetFirstLocation(), implementingMember);
				}
				break;
			}
			if (MemberSignatureComparer.ExplicitImplementationComparer.Equals(implementedMember, member))
			{
				diagnostics.Add(ErrorCode.WRN_ExplicitImplCollision, implementingMember.GetFirstLocation(), implementingMember);
			}
		}
	}
}
