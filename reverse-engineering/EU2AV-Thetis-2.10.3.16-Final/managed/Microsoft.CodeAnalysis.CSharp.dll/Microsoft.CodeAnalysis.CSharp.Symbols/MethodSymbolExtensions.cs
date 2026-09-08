using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class MethodSymbolExtensions
{
	public static bool IsParams(this MethodSymbol method)
	{
		if (method.ParameterCount != 0)
		{
			return method.Parameters[method.ParameterCount - 1].IsParams;
		}
		return false;
	}

	internal static bool IsSynthesizedLambda(this MethodSymbol method)
	{
		if (method.IsImplicitlyDeclared)
		{
			return method.MethodKind == MethodKind.AnonymousFunction;
		}
		return false;
	}

	public static bool IsRuntimeFinalizer(this MethodSymbol method, bool skipFirstMethodKindCheck = false)
	{
		if ((object)method == null || method.Name != "Finalize" || method.ParameterCount != 0 || method.Arity != 0 || !method.IsMetadataVirtual(MethodSymbol.IsMetadataVirtualOption.IgnoreInterfaceImplementationChanges))
		{
			return false;
		}
		while ((object)method != null)
		{
			if (!skipFirstMethodKindCheck && method.MethodKind == MethodKind.Destructor)
			{
				return true;
			}
			if (method.ContainingType.SpecialType == SpecialType.System_Object)
			{
				return true;
			}
			if (method.IsMetadataNewSlot(ignoreInterfaceImplementationChanges: true))
			{
				return false;
			}
			method = method.GetFirstRuntimeOverriddenMethodIgnoringNewSlot(out var _);
			skipFirstMethodKindCheck = false;
		}
		return false;
	}

	public static MethodSymbol ConstructIfGeneric(this MethodSymbol method, ImmutableArray<TypeWithAnnotations> typeArguments)
	{
		if (!method.IsGenericMethod)
		{
			return method;
		}
		return method.Construct(typeArguments);
	}

	public static bool CanBeHiddenByMember(this MethodSymbol hiddenMethod, Symbol hidingMember)
	{
		if (hiddenMethod.MethodKind == MethodKind.Destructor)
		{
			return false;
		}
		switch (hidingMember.Kind)
		{
		case SymbolKind.ErrorType:
		case SymbolKind.Method:
		case SymbolKind.NamedType:
		case SymbolKind.Property:
			return CanBeHiddenByMethodPropertyOrType(hiddenMethod, hidingMember);
		case SymbolKind.Event:
		case SymbolKind.Field:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(hidingMember.Kind);
		}
	}

	private static bool CanBeHiddenByMethodPropertyOrType(MethodSymbol method, Symbol hidingMember)
	{
		switch (method.MethodKind)
		{
		case MethodKind.Constructor:
		case MethodKind.Conversion:
		case MethodKind.Destructor:
		case MethodKind.StaticConstructor:
			return false;
		case MethodKind.UserDefinedOperator:
			if (!method.IsStatic)
			{
				if (hidingMember is MethodSymbol methodSymbol && !hidingMember.IsStatic)
				{
					return methodSymbol.MethodKind == MethodKind.UserDefinedOperator;
				}
				return false;
			}
			return false;
		case MethodKind.EventAdd:
		case MethodKind.EventRemove:
		case MethodKind.PropertyGet:
		case MethodKind.PropertySet:
			return method.IsIndexedPropertyAccessor();
		default:
			return true;
		}
	}

	public static bool IsAsyncReturningVoid(this MethodSymbol method)
	{
		if (method.IsAsync)
		{
			return method.ReturnsVoid;
		}
		return false;
	}

	public static bool IsAsyncEffectivelyReturningTask(this MethodSymbol method, CSharpCompilation compilation)
	{
		if (method.IsAsync && method.ReturnType is NamedTypeSymbol { Arity: 0 })
		{
			if (!method.HasAsyncMethodBuilderAttribute(out var _))
			{
				return method.ReturnType.IsNonGenericTaskType(compilation);
			}
			return true;
		}
		return false;
	}

	public static bool IsAsyncEffectivelyReturningGenericTask(this MethodSymbol method, CSharpCompilation compilation)
	{
		if (method.IsAsync && method.ReturnType is NamedTypeSymbol { Arity: 1 })
		{
			if (!method.HasAsyncMethodBuilderAttribute(out var _))
			{
				return method.ReturnType.IsGenericTaskType(compilation);
			}
			return true;
		}
		return false;
	}

	public static bool IsAsyncReturningIAsyncEnumerable(this MethodSymbol method, CSharpCompilation compilation)
	{
		if (method.IsAsync)
		{
			return method.ReturnType.IsIAsyncEnumerableType(compilation);
		}
		return false;
	}

	public static bool IsAsyncReturningIAsyncEnumerator(this MethodSymbol method, CSharpCompilation compilation)
	{
		if (method.IsAsync)
		{
			return method.ReturnType.IsIAsyncEnumeratorType(compilation);
		}
		return false;
	}

	internal static CSharpSyntaxNode ExtractReturnTypeSyntax(this MethodSymbol method)
	{
		if (method is SynthesizedSimpleProgramEntryPointSymbol synthesizedSimpleProgramEntryPointSymbol)
		{
			return (CSharpSyntaxNode)synthesizedSimpleProgramEntryPointSymbol.ReturnTypeSyntax;
		}
		method = method.PartialDefinitionPart ?? method;
		foreach (SyntaxReference declaringSyntaxReference in method.DeclaringSyntaxReferences)
		{
			SyntaxNode syntax = declaringSyntaxReference.GetSyntax();
			if (syntax is MethodDeclarationSyntax methodDeclarationSyntax)
			{
				return methodDeclarationSyntax.ReturnType;
			}
			if (syntax is LocalFunctionStatementSyntax localFunctionStatementSyntax)
			{
				return localFunctionStatementSyntax.ReturnType;
			}
		}
		return (CSharpSyntaxNode)CSharpSyntaxTree.Dummy.GetRoot();
	}

	internal static bool IsValidUnscopedRefAttributeTarget(this MethodSymbol method)
	{
		int num;
		if (!method.IsStatic)
		{
			NamedTypeSymbol containingType = method.ContainingType;
			if ((object)containingType != null)
			{
				num = ((containingType.IsStructType() || (containingType.IsInterface && method.IsImplementable())) ? 1 : 0);
				goto IL_0031;
			}
		}
		num = 0;
		goto IL_0031;
		IL_0031:
		bool flag = (byte)num != 0;
		if (flag)
		{
			MethodKind methodKind = method.MethodKind;
			bool flag2 = (uint)(methodKind - 8) <= 4u;
			flag = flag2;
		}
		if (flag)
		{
			return !method.IsInitOnly;
		}
		return false;
	}

	internal static bool HasUnscopedRefAttributeOnMethodOrProperty(this MethodSymbol? method)
	{
		if ((object)method == null)
		{
			return false;
		}
		if (!method.HasUnscopedRefAttribute)
		{
			if (method.AssociatedSymbol is PropertySymbol propertySymbol)
			{
				return propertySymbol.HasUnscopedRefAttribute;
			}
			return false;
		}
		return true;
	}
}
