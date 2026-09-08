using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundDynamicInvocableBase : BoundExpression
{
	public BoundExpression Expression { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	protected BoundDynamicInvocableBase(BoundKind kind, SyntaxNode syntax, BoundExpression expression, ImmutableArray<BoundExpression> arguments, TypeSymbol? type, bool hasErrors = false)
		: base(kind, syntax, type, hasErrors)
	{
		Expression = expression;
		Arguments = arguments;
	}
}
