using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAwaitExpression : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public BoundExpression Expression { get; }

	public BoundAwaitableInfo AwaitableInfo { get; }

	public BoundAwaitExpressionDebugInfo DebugInfo { get; }

	public BoundAwaitExpression(SyntaxNode syntax, BoundExpression expression, BoundAwaitableInfo awaitableInfo, BoundAwaitExpressionDebugInfo debugInfo, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.AwaitExpression, syntax, type, hasErrors || expression.HasErrors() || awaitableInfo.HasErrors())
	{
		Expression = expression;
		AwaitableInfo = awaitableInfo;
		DebugInfo = debugInfo;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAwaitExpression(this);
	}

	public BoundAwaitExpression Update(BoundExpression expression, BoundAwaitableInfo awaitableInfo, BoundAwaitExpressionDebugInfo debugInfo, TypeSymbol type)
	{
		if (expression != Expression || awaitableInfo != AwaitableInfo || debugInfo != DebugInfo || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundAwaitExpression boundAwaitExpression = new BoundAwaitExpression(Syntax, expression, awaitableInfo, debugInfo, type, base.HasErrors);
			boundAwaitExpression.CopyAttributes(this);
			return boundAwaitExpression;
		}
		return this;
	}
}
