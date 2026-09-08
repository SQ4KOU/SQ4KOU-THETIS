namespace Microsoft.CodeAnalysis.Operations;

internal sealed class MethodBodyOperation : BaseMethodBodyBaseOperation, IMethodBodyOperation, IMethodBodyBaseOperation, IOperation
{
	internal override int ChildOperationsCount => ((base.BlockBody != null) ? 1 : 0) + ((base.ExpressionBody != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.MethodBody;

	internal MethodBodyOperation(IBlockOperation? blockBody, IBlockOperation? expressionBody, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(blockBody, expressionBody, semanticModel, syntax, isImplicit)
	{
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (base.BlockBody != null)
			{
				return base.BlockBody;
			}
			break;
		case 1:
			if (base.ExpressionBody != null)
			{
				return base.ExpressionBody;
			}
			break;
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (base.BlockBody != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (base.ExpressionBody != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
		case 2:
			return (hasNext: false, nextSlot: 2, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		if ((uint)(previousSlot - -1) > 1u)
		{
			if (previousSlot != 1)
			{
				if (previousSlot != int.MaxValue)
				{
					throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
				}
				if (base.ExpressionBody != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (base.BlockBody != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitMethodBodyOperation(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitMethodBodyOperation(this, argument);
	}
}
