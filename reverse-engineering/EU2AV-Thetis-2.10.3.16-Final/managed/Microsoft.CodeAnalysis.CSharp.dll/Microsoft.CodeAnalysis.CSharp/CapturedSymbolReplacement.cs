using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class CapturedSymbolReplacement
{
	public readonly bool IsReusable;

	public CapturedSymbolReplacement(bool isReusable)
	{
		IsReusable = isReusable;
	}

	public abstract BoundExpression Replacement<TArg>(SyntaxNode node, Func<NamedTypeSymbol, TArg, BoundExpression> makeFrame, TArg arg);
}
