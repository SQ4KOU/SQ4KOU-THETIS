namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DefaultCaseClauseOperation : BaseCaseClauseOperation, IDefaultCaseClauseOperation, ICaseClauseOperation, IOperation
{
	internal override int ChildOperationsCount => 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.CaseClause;

	public override CaseKind CaseKind => CaseKind.Default;

	internal DefaultCaseClauseOperation(ILabelSymbol? label, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(label, semanticModel, syntax, isImplicit)
	{
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		return (hasNext: false, nextSlot: int.MinValue, nextIndex: int.MinValue);
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		return (hasNext: false, nextSlot: int.MinValue, nextIndex: int.MinValue);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDefaultCaseClause(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitDefaultCaseClause(this, argument);
	}
}
