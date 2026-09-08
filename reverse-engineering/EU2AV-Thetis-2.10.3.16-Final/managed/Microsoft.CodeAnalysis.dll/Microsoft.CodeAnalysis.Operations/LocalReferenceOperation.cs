namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LocalReferenceOperation : Operation, ILocalReferenceOperation, IOperation
{
	public ILocalSymbol Local { get; }

	public bool IsDeclaration { get; }

	internal override int ChildOperationsCount => 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue { get; }

	public override OperationKind Kind => OperationKind.LocalReference;

	internal LocalReferenceOperation(ILocalSymbol local, bool isDeclaration, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Local = local;
		IsDeclaration = isDeclaration;
		OperationConstantValue = constantValue;
		Type = type;
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
		visitor.VisitLocalReference(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitLocalReference(this, argument);
	}
}
