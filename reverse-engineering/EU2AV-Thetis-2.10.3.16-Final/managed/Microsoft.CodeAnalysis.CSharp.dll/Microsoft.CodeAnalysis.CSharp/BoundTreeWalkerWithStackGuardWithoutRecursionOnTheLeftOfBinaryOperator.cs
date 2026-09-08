using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator : BoundTreeWalkerWithStackGuard
{
	protected BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator()
	{
	}

	protected BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator(int recursionDepth)
		: base(recursionDepth)
	{
	}

	public sealed override BoundNode? VisitBinaryOperator(BoundBinaryOperator node)
	{
		if (!(node.Left is BoundBinaryOperator boundBinaryOperator))
		{
			return base.VisitBinaryOperator(node);
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		instance.Push(node.Right);
		BeforeVisitingSkippedBoundBinaryOperatorChildren(boundBinaryOperator);
		instance.Push(boundBinaryOperator.Right);
		BoundExpression left = boundBinaryOperator.Left;
		while (left.Kind == BoundKind.BinaryOperator)
		{
			BoundBinaryOperator boundBinaryOperator2 = (BoundBinaryOperator)left;
			BeforeVisitingSkippedBoundBinaryOperatorChildren(boundBinaryOperator2);
			instance.Push(boundBinaryOperator2.Right);
			left = boundBinaryOperator2.Left;
		}
		Visit(left);
		left = instance.Pop();
		do
		{
			Visit(left);
		}
		while (instance.TryPop(out left));
		instance.Free();
		return null;
	}

	protected virtual void BeforeVisitingSkippedBoundBinaryOperatorChildren(BoundBinaryOperator node)
	{
	}

	public sealed override BoundNode? VisitBinaryPattern(BoundBinaryPattern node)
	{
		if (!(node.Left is BoundBinaryPattern boundBinaryPattern))
		{
			return base.VisitBinaryPattern(node);
		}
		ArrayBuilder<BoundPattern> instance = ArrayBuilder<BoundPattern>.GetInstance();
		instance.Push(node.Right);
		instance.Push(boundBinaryPattern.Right);
		BoundPattern left = boundBinaryPattern.Left;
		while (left.Kind == BoundKind.BinaryPattern)
		{
			BoundBinaryPattern boundBinaryPattern2 = (BoundBinaryPattern)left;
			instance.Push(boundBinaryPattern2.Right);
			left = boundBinaryPattern2.Left;
		}
		Visit(left);
		left = instance.Pop();
		do
		{
			Visit(left);
		}
		while (instance.TryPop(out left));
		instance.Free();
		return null;
	}

	public override BoundNode? VisitCall(BoundCall node)
	{
		if (node.ReceiverOpt is BoundCall boundCall)
		{
			ArrayBuilder<BoundCall> instance = ArrayBuilder<BoundCall>.GetInstance();
			instance.Push(node);
			node = boundCall;
			while (node.ReceiverOpt is BoundCall boundCall2)
			{
				BeforeVisitingSkippedBoundCallChildren(node);
				instance.Push(node);
				node = boundCall2;
			}
			BeforeVisitingSkippedBoundCallChildren(node);
			VisitReceiver(node);
			do
			{
				VisitArguments(node);
			}
			while (instance.TryPop(out node));
			instance.Free();
		}
		else
		{
			VisitReceiver(node);
			VisitArguments(node);
		}
		return null;
	}

	protected virtual void BeforeVisitingSkippedBoundCallChildren(BoundCall node)
	{
	}

	protected virtual void VisitReceiver(BoundCall node)
	{
		Visit(node.ReceiverOpt);
	}

	protected virtual void VisitArguments(BoundCall node)
	{
		VisitList(node.Arguments);
	}

	public sealed override BoundNode? VisitIfStatement(BoundIfStatement node)
	{
		while (true)
		{
			Visit(node.Condition);
			Visit(node.Consequence);
			BoundStatement alternativeOpt = node.AlternativeOpt;
			if (alternativeOpt == null)
			{
				break;
			}
			if (alternativeOpt is BoundIfStatement boundIfStatement)
			{
				node = boundIfStatement;
				continue;
			}
			Visit(alternativeOpt);
			break;
		}
		return null;
	}
}
