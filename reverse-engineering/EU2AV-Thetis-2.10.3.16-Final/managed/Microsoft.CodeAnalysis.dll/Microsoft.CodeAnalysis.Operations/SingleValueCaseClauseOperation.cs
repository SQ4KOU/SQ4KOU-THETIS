namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SingleValueCaseClauseOperation : BaseCaseClauseOperation, ISingleValueCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public IOperation Value { get; }

	internal override int ChildOperationsCount => (Value != null) ? 1 : 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.CaseClause;

	public override CaseKind CaseKind => CaseKind.SingleValue;

	internal SingleValueCaseClauseOperation(IOperation value, ILabelSymbol? label, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(label, semanticModel, syntax, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && Value != null)
		{
			return Value;
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		if (previousSlot != -1)
		{
			if ((uint)previousSlot > 1u)
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
		}
		else if (Value != null)
		{
			return (hasNext: true, nextSlot: 0, nextIndex: 0);
		}
		return (hasNext: false, nextSlot: 1, nextIndex: 0);
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		if ((uint)(previousSlot - -1) > 1u)
		{
			if (previousSlot != int.MaxValue)
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			if (Value != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSingleValueCaseClause(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitSingleValueCaseClause(this, argument);
	}
}
