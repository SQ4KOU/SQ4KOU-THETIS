namespace Microsoft.CodeAnalysis.Operations;

internal sealed class OmittedArgumentOperation : Operation, IOmittedArgumentOperation, IOperation
{
	internal override int ChildOperationsCount => 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.OmittedArgument;

	internal OmittedArgumentOperation(SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
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
		visitor.VisitOmittedArgument(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitOmittedArgument(this, argument);
	}
}
