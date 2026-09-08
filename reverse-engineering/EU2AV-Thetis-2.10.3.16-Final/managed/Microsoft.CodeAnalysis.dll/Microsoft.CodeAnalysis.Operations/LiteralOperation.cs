namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LiteralOperation : Operation, ILiteralOperation, IOperation
{
	internal override int ChildOperationsCount => 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue { get; }

	public override OperationKind Kind => OperationKind.Literal;

	internal LiteralOperation(SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
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
		visitor.VisitLiteral(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitLiteral(this, argument);
	}
}
