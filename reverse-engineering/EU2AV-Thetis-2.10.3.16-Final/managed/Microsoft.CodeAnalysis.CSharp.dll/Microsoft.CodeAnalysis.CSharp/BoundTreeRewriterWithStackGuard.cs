using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeRewriterWithStackGuard : BoundTreeRewriter
{
	private int _recursionDepth;

	protected int RecursionDepth => _recursionDepth;

	protected BoundTreeRewriterWithStackGuard()
	{
	}

	protected BoundTreeRewriterWithStackGuard(int recursionDepth)
	{
		_recursionDepth = recursionDepth;
	}

	[return: NotNullIfNotNull("node")]
	public override BoundNode? Visit(BoundNode? node)
	{
		if ((node is BoundExpression || node is BoundPattern) ? true : false)
		{
			return VisitExpressionOrPatternWithStackGuard(ref _recursionDepth, node);
		}
		return base.Visit(node);
	}

	protected BoundNode VisitExpressionOrPatternWithStackGuard(BoundNode node)
	{
		return VisitExpressionOrPatternWithStackGuard(ref _recursionDepth, node);
	}

	protected sealed override BoundNode VisitExpressionOrPatternWithoutStackGuard(BoundNode node)
	{
		return base.Visit(node);
	}
}
