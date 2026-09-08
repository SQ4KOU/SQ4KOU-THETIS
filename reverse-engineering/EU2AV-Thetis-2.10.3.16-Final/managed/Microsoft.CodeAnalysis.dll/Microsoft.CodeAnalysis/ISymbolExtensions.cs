using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis;

public static class ISymbolExtensions
{
	public static IMethodSymbol? GetConstructedReducedFrom(this IMethodSymbol method)
	{
		if (method.MethodKind != MethodKind.ReducedExtension)
		{
			return null;
		}
		IMethodSymbol reducedFrom = method.ReducedFrom;
		if (!reducedFrom.IsGenericMethod)
		{
			return reducedFrom;
		}
		ITypeSymbol[] array = new ITypeSymbol[reducedFrom.TypeParameters.Length];
		int i = 0;
		for (int length = method.TypeParameters.Length; i < length; i++)
		{
			ITypeSymbol typeSymbol = method.TypeArguments[i];
			ITypeParameterSymbol typeParameterSymbol = method.TypeParameters[i];
			if (typeSymbol.Equals(typeParameterSymbol))
			{
				typeSymbol = typeParameterSymbol.ReducedFrom;
			}
			array[typeParameterSymbol.ReducedFrom.Ordinal] = typeSymbol;
		}
		int j = 0;
		for (int length2 = reducedFrom.TypeParameters.Length; j < length2; j++)
		{
			ITypeSymbol typeInferredDuringReduction = method.GetTypeInferredDuringReduction(reducedFrom.TypeParameters[j]);
			if (typeInferredDuringReduction != null)
			{
				array[j] = typeInferredDuringReduction;
			}
		}
		return reducedFrom.Construct(array);
	}

	internal static bool IsDefaultTupleElement(this IFieldSymbol field)
	{
		return field == field.CorrespondingTupleField;
	}

	internal static bool IsTupleElement(this IFieldSymbol field)
	{
		return field.CorrespondingTupleField != null;
	}

	internal static string? ProvidedTupleElementNameOrNull(this IFieldSymbol field)
	{
		if (!field.IsTupleElement() || field.IsImplicitlyDeclared)
		{
			return null;
		}
		return field.Name;
	}

	internal static INamespaceSymbol? GetNestedNamespace(this INamespaceSymbol container, string name)
	{
		foreach (INamespaceOrTypeSymbol member in container.GetMembers(name))
		{
			if (member.Kind == SymbolKind.Namespace)
			{
				return (INamespaceSymbol)member;
			}
		}
		return null;
	}

	internal static bool IsNetModule(this IAssemblySymbol assembly)
	{
		if (assembly is ISourceAssemblySymbol sourceAssemblySymbol)
		{
			return sourceAssemblySymbol.Compilation.Options.OutputKind.IsNetModule();
		}
		return false;
	}

	internal static bool IsInSource(this ISymbol symbol)
	{
		foreach (Location location in symbol.Locations)
		{
			if (location.IsInSource)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsWellKnownTypeLock(this ITypeSymbol type)
	{
		if (type is INamedTypeSymbol namedTypeSymbol && type.Name == "Lock" && namedTypeSymbol.Arity == 0 && type.ContainingType == null)
		{
			INamespaceSymbol containingNamespace = type.ContainingNamespace;
			if (containingNamespace != null && containingNamespace.Name == "Threading")
			{
				INamespaceSymbol containingNamespace2 = containingNamespace.ContainingNamespace;
				if (containingNamespace2 != null && containingNamespace2.Name == "System")
				{
					INamespaceSymbol containingNamespace3 = containingNamespace2.ContainingNamespace;
					if (containingNamespace3 != null)
					{
						return containingNamespace3.IsGlobalNamespace;
					}
				}
			}
		}
		return false;
	}

	internal static (IMethodSymbol EnterScopeMethod, IMethodSymbol ScopeDisposeMethod)? TryFindLockTypeInfo(this ITypeSymbol lockType)
	{
		IMethodSymbol methodSymbol = TryFindPublicVoidParameterlessMethod(lockType, "EnterScope");
		if (methodSymbol == null || methodSymbol.ReturnsVoid || methodSymbol.RefKind != RefKind.None)
		{
			return null;
		}
		ITypeSymbol returnType = methodSymbol.ReturnType;
		if (!(returnType is INamedTypeSymbol namedTypeSymbol) || !(returnType.Name == "Scope") || namedTypeSymbol.Arity != 0 || !returnType.IsValueType || !returnType.IsRefLikeType || returnType.DeclaredAccessibility != Accessibility.Public || !lockType.Equals(returnType.ContainingType, SymbolEqualityComparer.ConsiderEverything))
		{
			return null;
		}
		IMethodSymbol methodSymbol2 = TryFindPublicVoidParameterlessMethod(returnType, "Dispose");
		if (methodSymbol2 == null || !methodSymbol2.ReturnsVoid)
		{
			return null;
		}
		return new(IMethodSymbol, IMethodSymbol)
		{
			Item1 = methodSymbol,
			Item2 = methodSymbol2
		};
	}

	private static IMethodSymbol? TryFindPublicVoidParameterlessMethod(ITypeSymbol type, string name)
	{
		ImmutableArray<ISymbol> members = type.GetMembers(name);
		IMethodSymbol methodSymbol = null;
		foreach (ISymbol item in members)
		{
			if (item is IMethodSymbol methodSymbol2 && methodSymbol2.Parameters.Length == 0 && methodSymbol2.Arity == 0 && !item.IsStatic && item.DeclaredAccessibility == Accessibility.Public && methodSymbol2.MethodKind == MethodKind.Ordinary)
			{
				if (methodSymbol != null)
				{
					return null;
				}
				methodSymbol = methodSymbol2;
			}
		}
		return methodSymbol;
	}

	internal static IVTConclusion PerformIVTCheck(this AssemblyIdentity assemblyGrantingAccessIdentity, ImmutableArray<byte> assemblyWantingAccessKey, ImmutableArray<byte> grantedToPublicKey)
	{
		bool isStrongName = assemblyGrantingAccessIdentity.IsStrongName;
		bool num = !grantedToPublicKey.IsDefaultOrEmpty;
		bool flag = !assemblyWantingAccessKey.IsDefaultOrEmpty;
		bool flag2 = (num & flag) && ByteSequenceComparer.Equals(grantedToPublicKey, assemblyWantingAccessKey);
		if (num && !flag2)
		{
			return IVTConclusion.PublicKeyDoesntMatch;
		}
		if (!isStrongName & flag)
		{
			return IVTConclusion.OneSignedOneNot;
		}
		return IVTConclusion.Match;
	}
}
