using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CapturedToExpressionSymbolReplacement<THoistedSymbolType> : CapturedSymbolReplacement where THoistedSymbolType : Symbol
{
	private readonly BoundExpression _replacement;

	public readonly ImmutableArray<THoistedSymbolType> HoistedSymbols;

	public CapturedToExpressionSymbolReplacement(BoundExpression replacement, ImmutableArray<THoistedSymbolType> hoistedSymbols, bool isReusable)
		: base(isReusable)
	{
		_replacement = replacement;
		HoistedSymbols = hoistedSymbols;
	}

	public override BoundExpression Replacement<TArg>(SyntaxNode node, Func<NamedTypeSymbol, TArg, BoundExpression> makeFrame, TArg arg)
	{
		return _replacement;
	}
}
