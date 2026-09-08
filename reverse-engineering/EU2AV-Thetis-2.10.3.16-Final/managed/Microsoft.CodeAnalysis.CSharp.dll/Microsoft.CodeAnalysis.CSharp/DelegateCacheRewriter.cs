using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DelegateCacheRewriter
{
	private readonly SyntheticBoundNodeFactory _factory;

	private readonly int _topLevelMethodOrdinal;

	private Dictionary<Symbol, DelegateCacheContainer>? _genericCacheContainers;

	private static readonly Func<TypeSymbol, HashSet<TypeParameterSymbol>, bool, bool> s_typeParameterSymbolCollector = delegate(TypeSymbol typeSymbol, HashSet<TypeParameterSymbol> result, bool _)
	{
		if (typeSymbol is TypeParameterSymbol item)
		{
			result.Add(item);
		}
		return false;
	};

	internal DelegateCacheRewriter(SyntheticBoundNodeFactory factory, int topLevelMethodOrdinal)
	{
		_factory = factory;
		_topLevelMethodOrdinal = topLevelMethodOrdinal;
	}

	internal static bool CanRewrite(BoundDelegateCreationExpression boundDelegateCreation)
	{
		if (boundDelegateCreation.MethodOpt.IsStatic)
		{
			return !boundDelegateCreation.IsExtensionMethod;
		}
		return false;
	}

	internal BoundExpression Rewrite(BoundDelegateCreationExpression boundDelegateCreation)
	{
		SyntaxNode syntax = _factory.Syntax;
		_factory.Syntax = boundDelegateCreation.Syntax;
		FieldSymbol orAddCacheField = GetOrAddCacheContainer(boundDelegateCreation).GetOrAddCacheField(_factory, boundDelegateCreation);
		BoundFieldAccess left = _factory.Field(null, orAddCacheField);
		BoundExpression result = _factory.Coalesce(left, _factory.AssignmentExpression(left, boundDelegateCreation));
		_factory.Syntax = syntax;
		return result;
	}

	private DelegateCacheContainer GetOrAddCacheContainer(BoundDelegateCreationExpression boundDelegateCreation)
	{
		int currentGenerationOrdinal = _factory.ModuleBuilderOpt.CurrentGenerationOrdinal;
		DelegateCacheContainer concreteDelegateCacheContainer;
		if (!TryGetOwnerFunctionOrExtensionType(_factory.CurrentFunction, boundDelegateCreation, out Symbol owner))
		{
			TypeCompilationState compilationState = _factory.CompilationState;
			concreteDelegateCacheContainer = compilationState.ConcreteDelegateCacheContainer;
			if ((object)concreteDelegateCacheContainer != null)
			{
				return concreteDelegateCacheContainer;
			}
			concreteDelegateCacheContainer = (compilationState.ConcreteDelegateCacheContainer = new DelegateCacheContainer(compilationState.Type, currentGenerationOrdinal));
		}
		else
		{
			Dictionary<Symbol, DelegateCacheContainer> dictionary = _genericCacheContainers ?? (_genericCacheContainers = new Dictionary<Symbol, DelegateCacheContainer>(ReferenceEqualityComparer.Instance));
			if (dictionary.TryGetValue(owner, out concreteDelegateCacheContainer))
			{
				return concreteDelegateCacheContainer;
			}
			concreteDelegateCacheContainer = new DelegateCacheContainer(_factory.CompilationState.Type, owner, _topLevelMethodOrdinal, dictionary.Count, currentGenerationOrdinal);
			dictionary.Add(owner, concreteDelegateCacheContainer);
		}
		_factory.AddNestedType(concreteDelegateCacheContainer);
		return concreteDelegateCacheContainer;
	}

	private static bool TryGetOwnerFunctionOrExtensionType(MethodSymbol currentFunction, BoundDelegateCreationExpression boundDelegateCreation, [NotNullWhen(true)] out Symbol? owner)
	{
		MethodSymbol methodOpt = boundDelegateCreation.MethodOpt;
		if (methodOpt.MethodKind == MethodKind.LocalFunction)
		{
			Symbol symbol;
			for (symbol = currentFunction; symbol is MethodSymbol methodSymbol; symbol = symbol.ContainingSymbol)
			{
				if (methodSymbol.Arity > 0)
				{
					owner = methodSymbol;
					return true;
				}
			}
			if (symbol is NamedTypeSymbol { IsExtension: not false, Arity: >0 })
			{
				owner = symbol;
				return true;
			}
			owner = null;
			return false;
		}
		PooledHashSet<TypeParameterSymbol> instance = PooledHashSet<TypeParameterSymbol>.GetInstance();
		try
		{
			if ((methodOpt.IsAbstract || methodOpt.IsVirtual) && boundDelegateCreation.Argument is BoundTypeExpression boundTypeExpression)
			{
				FindTypeParameters(boundTypeExpression.Type, instance);
			}
			FindTypeParameters(boundDelegateCreation.Type, instance);
			FindTypeParameters(methodOpt, instance);
			Symbol symbol2;
			for (symbol2 = currentFunction; symbol2 is MethodSymbol methodSymbol2; symbol2 = symbol2.ContainingSymbol)
			{
				if (usedTypeParametersContains(instance, methodSymbol2.TypeParameters))
				{
					owner = methodSymbol2;
					return true;
				}
			}
			if (symbol2 is NamedTypeSymbol { IsExtension: not false, Arity: >0 } namedTypeSymbol2 && usedTypeParametersContains(instance, namedTypeSymbol2.TypeParameters))
			{
				owner = symbol2;
				return true;
			}
			owner = null;
			return false;
		}
		finally
		{
			instance.Free();
		}
		static bool usedTypeParametersContains(HashSet<TypeParameterSymbol> used, ImmutableArray<TypeParameterSymbol> typeParameters)
		{
			foreach (TypeParameterSymbol item in typeParameters)
			{
				if (used.Contains(item))
				{
					return true;
				}
			}
			return false;
		}
	}

	private static void FindTypeParameters(TypeSymbol type, HashSet<TypeParameterSymbol> result)
	{
		type.VisitType<HashSet<TypeParameterSymbol>>(s_typeParameterSymbolCollector, result, canDigThroughNullable: false, visitCustomModifiers: true);
	}

	private static void FindTypeParameters(MethodSymbol method, HashSet<TypeParameterSymbol> result)
	{
		FindTypeParameters(method.ContainingType, result);
		foreach (TypeWithAnnotations typeArgumentsWithAnnotation in method.TypeArgumentsWithAnnotations)
		{
			typeArgumentsWithAnnotation.VisitType<HashSet<TypeParameterSymbol>>(null, null, s_typeParameterSymbolCollector, result, canDigThroughNullable: false, useDefaultType: false, visitCustomModifiers: true);
		}
	}
}
