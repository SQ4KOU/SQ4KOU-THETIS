namespace Microsoft.CodeAnalysis.Operations;

internal sealed class RangeOperation : Operation, IRangeOperation, IOperation
{
	public IOperation? LeftOperand { get; }

	public IOperation? RightOperand { get; }

	public bool IsLifted { get; }

	public IMethodSymbol? Method { get; }

	internal override int ChildOperationsCount => ((LeftOperand != null) ? 1 : 0) + ((RightOperand != null) ? 1 : 0);

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Range;

	internal RangeOperation(IOperation? leftOperand, IOperation? rightOperand, bool isLifted, IMethodSymbol? method, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		LeftOperand = Operation.SetParentOperation(leftOperand, this);
		RightOperand = Operation.SetParentOperation(rightOperand, this);
		IsLifted = isLifted;
		Method = method;
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (LeftOperand != null)
			{
				return LeftOperand;
			}
			break;
		case 1:
			if (RightOperand != null)
			{
				return RightOperand;
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
			if (LeftOperand != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (RightOperand != null)
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
				if (RightOperand != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (LeftOperand != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitRangeOperation(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitRangeOperation(this, argument);
	}
}
