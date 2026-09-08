namespace Microsoft.CodeAnalysis.Operations;

internal sealed class RelationalCaseClauseOperation : BaseCaseClauseOperation, IRelationalCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public IOperation Value { get; }

	public BinaryOperatorKind Relation { get; }

	internal override int ChildOperationsCount => (Value != null) ? 1 : 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.CaseClause;

	public override CaseKind CaseKind => CaseKind.Relational;

	internal RelationalCaseClauseOperation(IOperation value, BinaryOperatorKind relation, ILabelSymbol? label, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(label, semanticModel, syntax, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
		Relation = relation;
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
		visitor.VisitRelationalCaseClause(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitRelationalCaseClause(this, argument);
	}
}
