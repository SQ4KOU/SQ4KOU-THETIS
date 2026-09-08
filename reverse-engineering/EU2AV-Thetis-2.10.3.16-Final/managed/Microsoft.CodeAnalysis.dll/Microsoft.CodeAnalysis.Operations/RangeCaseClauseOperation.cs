namespace Microsoft.CodeAnalysis.Operations;

internal sealed class RangeCaseClauseOperation : BaseCaseClauseOperation, IRangeCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public IOperation MinimumValue { get; }

	public IOperation MaximumValue { get; }

	internal override int ChildOperationsCount => ((MinimumValue != null) ? 1 : 0) + ((MaximumValue != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.CaseClause;

	public override CaseKind CaseKind => CaseKind.Range;

	internal RangeCaseClauseOperation(IOperation minimumValue, IOperation maximumValue, ILabelSymbol? label, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(label, semanticModel, syntax, isImplicit)
	{
		MinimumValue = Operation.SetParentOperation(minimumValue, this);
		MaximumValue = Operation.SetParentOperation(maximumValue, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (MinimumValue != null)
			{
				return MinimumValue;
			}
			break;
		case 1:
			if (MaximumValue != null)
			{
				return MaximumValue;
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
			if (MinimumValue != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (MaximumValue != null)
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
				if (MaximumValue != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (MinimumValue != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitRangeCaseClause(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitRangeCaseClause(this, argument);
	}
}
