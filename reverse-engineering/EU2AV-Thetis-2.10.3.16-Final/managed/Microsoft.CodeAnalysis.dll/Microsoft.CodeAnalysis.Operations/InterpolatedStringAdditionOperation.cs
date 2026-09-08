namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolatedStringAdditionOperation : Operation, IInterpolatedStringAdditionOperation, IOperation
{
	public IOperation Left { get; }

	public IOperation Right { get; }

	internal override int ChildOperationsCount => ((Left != null) ? 1 : 0) + ((Right != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.InterpolatedStringAddition;

	internal InterpolatedStringAdditionOperation(IOperation left, IOperation right, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Left = Operation.SetParentOperation(left, this);
		Right = Operation.SetParentOperation(right, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Left != null)
			{
				return Left;
			}
			break;
		case 1:
			if (Right != null)
			{
				return Right;
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
			if (Left != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Right != null)
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
				if (Right != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (Left != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInterpolatedStringAddition(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitInterpolatedStringAddition(this, argument);
	}
}
