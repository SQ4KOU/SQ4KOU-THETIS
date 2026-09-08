using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator : BoundTreeRewriterWithStackGuard
{
	protected BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator()
	{
	}

	protected BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator(int recursionDepth)
		: base(recursionDepth)
	{
	}

	public sealed override BoundNode? VisitBinaryOperator(BoundBinaryOperator node)
	{
		BoundExpression left = node.Left;
		if (left.Kind != BoundKind.BinaryOperator)
		{
			return node.Update(node.OperatorKind, VisitBinaryOperatorData(node), node.ResultKind, (BoundExpression)Visit(node.Left), (BoundExpression)Visit(node.Right), VisitType(node.Type));
		}
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		instance.Push(node);
		BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)left;
		while (true)
		{
			instance.Push(boundBinaryOperator);
			left = boundBinaryOperator.Left;
			if (left.Kind != BoundKind.BinaryOperator)
			{
				break;
			}
			boundBinaryOperator = (BoundBinaryOperator)left;
		}
		BoundExpression boundExpression = (BoundExpression)Visit(left);
		do
		{
			boundBinaryOperator = instance.Pop();
			BoundExpression right = (BoundExpression)Visit(boundBinaryOperator.Right);
			TypeSymbol type = VisitType(boundBinaryOperator.Type);
			boundExpression = boundBinaryOperator.Update(boundBinaryOperator.OperatorKind, VisitBinaryOperatorData(boundBinaryOperator), boundBinaryOperator.ResultKind, boundExpression, right, type);
		}
		while (instance.Count > 0);
		instance.Free();
		return boundExpression;
	}

	protected virtual BoundBinaryOperator.UncommonData? VisitBinaryOperatorData(BoundBinaryOperator node)
	{
		return node.Data;
	}

	public sealed override BoundNode? VisitIfStatement(BoundIfStatement node)
	{
		BoundIfStatement boundIfStatement = node.AlternativeOpt as BoundIfStatement;
		if (boundIfStatement == null)
		{
			return base.VisitIfStatement(node);
		}
		ArrayBuilder<BoundIfStatement> instance = ArrayBuilder<BoundIfStatement>.GetInstance();
		instance.Push(node);
		BoundStatement alternativeOpt;
		while (true)
		{
			instance.Push(boundIfStatement);
			alternativeOpt = boundIfStatement.AlternativeOpt;
			if (!(alternativeOpt is BoundIfStatement boundIfStatement2))
			{
				break;
			}
			boundIfStatement = boundIfStatement2;
		}
		alternativeOpt = (BoundStatement)Visit(alternativeOpt);
		do
		{
			boundIfStatement = instance.Pop();
			BoundExpression condition = (BoundExpression)Visit(boundIfStatement.Condition);
			BoundStatement consequence = (BoundStatement)Visit(boundIfStatement.Consequence);
			alternativeOpt = boundIfStatement.Update(condition, consequence, alternativeOpt);
		}
		while (instance.Count > 0);
		instance.Free();
		return alternativeOpt;
	}

	public sealed override BoundNode? VisitBinaryPattern(BoundBinaryPattern node)
	{
		BoundPattern left = node.Left;
		if (left.Kind != BoundKind.BinaryPattern)
		{
			return base.VisitBinaryPattern(node);
		}
		ArrayBuilder<BoundBinaryPattern> instance = ArrayBuilder<BoundBinaryPattern>.GetInstance();
		instance.Push(node);
		BoundBinaryPattern boundBinaryPattern = (BoundBinaryPattern)left;
		while (true)
		{
			instance.Push(boundBinaryPattern);
			left = boundBinaryPattern.Left;
			if (left.Kind != BoundKind.BinaryPattern)
			{
				break;
			}
			boundBinaryPattern = (BoundBinaryPattern)left;
		}
		BoundPattern boundPattern = (BoundPattern)Visit(left);
		do
		{
			boundBinaryPattern = instance.Pop();
			BoundPattern right = (BoundPattern)Visit(boundBinaryPattern.Right);
			boundPattern = boundBinaryPattern.Update(boundBinaryPattern.Disjunction, boundPattern, right, VisitType(boundBinaryPattern.InputType), VisitType(boundBinaryPattern.NarrowedType));
		}
		while (instance.Count > 0);
		instance.Free();
		return boundPattern;
	}
}
