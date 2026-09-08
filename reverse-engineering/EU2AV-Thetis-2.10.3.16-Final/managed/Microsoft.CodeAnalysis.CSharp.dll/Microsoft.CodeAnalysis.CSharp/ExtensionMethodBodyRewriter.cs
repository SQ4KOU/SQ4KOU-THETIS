using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ExtensionMethodBodyRewriter : BoundTreeToDifferentEnclosingContextRewriter
{
	private ImmutableDictionary<Symbol, Symbol> _symbolMap;

	private RewrittenMethodSymbol _rewrittenContainingMethod;

	protected override bool EnforceAccurateContainerForLocals => true;

	protected override MethodSymbol CurrentMethod => _rewrittenContainingMethod;

	protected override TypeMap TypeMap => _rewrittenContainingMethod.TypeMap;

	public ExtensionMethodBodyRewriter(MethodSymbol sourceMethod, SourceExtensionImplementationMethodSymbol implementationMethod)
	{
		_symbolMap = ImmutableDictionary<Symbol, Symbol>.Empty.WithComparers(ReferenceEqualityComparer.Instance, ReferenceEqualityComparer.Instance);
		bool flag = sourceMethod.ParameterCount != implementationMethod.ParameterCount;
		ImmutableArray<ParameterSymbol> parameters;
		if (flag)
		{
			parameters = implementationMethod.Parameters;
			WrappedParameterSymbol wrappedParameterSymbol = (WrappedParameterSymbol)parameters[0];
			_symbolMap = _symbolMap.Add(wrappedParameterSymbol.UnderlyingParameter, wrappedParameterSymbol);
		}
		parameters = implementationMethod.Parameters;
		ReadOnlySpan<ParameterSymbol> readOnlySpan = parameters.AsSpan();
		int num = (flag ? 1 : 0);
		EnterMethod(sourceMethod, implementationMethod, readOnlySpan.Slice(num, readOnlySpan.Length - num));
	}

	private (RewrittenMethodSymbol, ImmutableDictionary<Symbol, Symbol>) EnterMethod(MethodSymbol symbol, RewrittenMethodSymbol rewritten, ReadOnlySpan<ParameterSymbol> rewrittenParameters)
	{
		ImmutableDictionary<Symbol, Symbol> symbolMap = _symbolMap;
		RewrittenMethodSymbol rewrittenContainingMethod = _rewrittenContainingMethod;
		if (!rewrittenParameters.IsEmpty)
		{
			ImmutableDictionary<Symbol, Symbol>.Builder builder = _symbolMap.ToBuilder();
			foreach (ParameterSymbol parameter in symbol.Parameters)
			{
				builder.Add(parameter, rewrittenParameters[parameter.Ordinal]);
			}
			_symbolMap = builder.ToImmutable();
		}
		_rewrittenContainingMethod = rewritten;
		return (rewrittenContainingMethod, symbolMap);
	}

	private (RewrittenMethodSymbol, ImmutableDictionary<Symbol, Symbol>) EnterMethod(MethodSymbol symbol, RewrittenLambdaOrLocalFunctionSymbol rewritten)
	{
		return EnterMethod(symbol, rewritten, rewritten.Parameters.AsSpan());
	}

	public override BoundNode? VisitThisReference(BoundThisReference node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/ExtensionMethodBodyRewriter.cs", 84);
	}

	public override ParameterSymbol VisitParameterSymbol(ParameterSymbol symbol)
	{
		return (ParameterSymbol)_symbolMap[symbol];
	}

	public override BoundNode? VisitLambda(BoundLambda node)
	{
		RewrittenLambdaOrLocalFunctionSymbol rewrittenLambdaOrLocalFunctionSymbol = new RewrittenLambdaOrLocalFunctionSymbol(node.Symbol, _rewrittenContainingMethod);
		(RewrittenMethodSymbol, ImmutableDictionary<Symbol, Symbol>) tuple = EnterMethod(node.Symbol, rewrittenLambdaOrLocalFunctionSymbol);
		_symbolMap = _symbolMap.Add(node.Symbol, rewrittenLambdaOrLocalFunctionSymbol);
		BoundBlock body = (BoundBlock)Visit(node.Body);
		(RewrittenMethodSymbol, ImmutableDictionary<Symbol, Symbol>) tuple2 = tuple;
		_rewrittenContainingMethod = tuple2.Item1;
		_symbolMap = tuple2.Item2;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.UnboundLambda, rewrittenLambdaOrLocalFunctionSymbol, body, node.Diagnostics, node.Binder, type);
	}

	public override BoundNode? VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		MethodSymbol methodSymbol = VisitMethodSymbol(node.Symbol);
		(RewrittenMethodSymbol, ImmutableDictionary<Symbol, Symbol>) tuple = EnterMethod(node.Symbol, (RewrittenLambdaOrLocalFunctionSymbol)methodSymbol);
		BoundBlock blockBody = (BoundBlock)Visit(node.BlockBody);
		BoundBlock expressionBody = (BoundBlock)Visit(node.ExpressionBody);
		(_rewrittenContainingMethod, _symbolMap) = tuple;
		return node.Update(methodSymbol, blockBody, expressionBody);
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		ImmutableDictionary<Symbol, Symbol> symbolMap = _symbolMap;
		if (!node.LocalFunctions.IsEmpty)
		{
			ImmutableDictionary<Symbol, Symbol>.Builder builder = _symbolMap.ToBuilder();
			foreach (MethodSymbol localFunction in node.LocalFunctions)
			{
				builder.Add(localFunction, new RewrittenLambdaOrLocalFunctionSymbol(localFunction, _rewrittenContainingMethod));
			}
			_symbolMap = builder.ToImmutable();
		}
		BoundNode result = base.VisitBlock(node);
		_symbolMap = symbolMap;
		return result;
	}

	protected override ImmutableArray<MethodSymbol> VisitDeclaredLocalFunctions(ImmutableArray<MethodSymbol> localFunctions)
	{
		return localFunctions.SelectAsArray((MethodSymbol l, ImmutableDictionary<Symbol, Symbol> map) => (MethodSymbol)map[l], _symbolMap);
	}

	[return: NotNullIfNotNull("symbol")]
	public override MethodSymbol? VisitMethodSymbol(MethodSymbol? symbol)
	{
		switch (symbol?.MethodKind)
		{
		case MethodKind.AnonymousFunction:
			return (MethodSymbol)_symbolMap[symbol];
		case MethodKind.LocalFunction:
			if (symbol.IsDefinition)
			{
				return (MethodSymbol)_symbolMap[symbol];
			}
			return ((MethodSymbol)_symbolMap[symbol.OriginalDefinition]).ConstructIfGeneric(TypeMap.SubstituteTypes(symbol.TypeArgumentsWithAnnotations));
		default:
			return base.VisitMethodSymbol(symbol);
		}
	}

	public override BoundNode? VisitCall(BoundCall node)
	{
		return ExtensionMethodReferenceRewriter.VisitCall(this, node);
	}

	public override BoundNode? VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		return ExtensionMethodReferenceRewriter.VisitDelegateCreationExpression(this, node);
	}

	public override BoundNode VisitFunctionPointerLoad(BoundFunctionPointerLoad node)
	{
		return ExtensionMethodReferenceRewriter.VisitFunctionPointerLoad(this, node);
	}

	[return: NotNullIfNotNull("symbol")]
	public override PropertySymbol? VisitPropertySymbol(PropertySymbol? symbol)
	{
		return base.VisitPropertySymbol(symbol);
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		return ExtensionMethodReferenceRewriter.VisitUnaryOperator(this, node);
	}

	protected override BoundBinaryOperator.UncommonData? VisitBinaryOperatorData(BoundBinaryOperator node)
	{
		return ExtensionMethodReferenceRewriter.VisitBinaryOperatorData(this, node);
	}

	public override BoundNode? VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		return ExtensionMethodReferenceRewriter.VisitMethodDefIndex(this, node);
	}
}
