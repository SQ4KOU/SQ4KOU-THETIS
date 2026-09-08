namespace Microsoft.CodeAnalysis.Operations;

internal sealed class PatternCaseClauseOperation : BaseCaseClauseOperation, IPatternCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public new ILabelSymbol Label => base.Label;

	public IPatternOperation Pattern { get; }

	public IOperation? Guard { get; }

	internal override int ChildOperationsCount => ((Pattern != null) ? 1 : 0) + ((Guard != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.CaseClause;

	public override CaseKind CaseKind => CaseKind.Pattern;

	internal PatternCaseClauseOperation(ILabelSymbol label, IPatternOperation pattern, IOperation? guard, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(label, semanticModel, syntax, isImplicit)
	{
		Pattern = Operation.SetParentOperation(pattern, this);
		Guard = Operation.SetParentOperation(guard, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Pattern != null)
			{
				return Pattern;
			}
			break;
		case 1:
			if (Guard != null)
			{
				return Guard;
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
			if (Pattern != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Guard != null)
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
				if (Guard != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (Pattern != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitPatternCaseClause(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitPatternCaseClause(this, argument);
	}
}
