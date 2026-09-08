namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolatedStringHandlerArgumentPlaceholderOperation : Operation, IInterpolatedStringHandlerArgumentPlaceholderOperation, IOperation
{
	public int ArgumentIndex { get; }

	public InterpolatedStringArgumentPlaceholderKind PlaceholderKind { get; }

	internal override int ChildOperationsCount => 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.InterpolatedStringHandlerArgumentPlaceholder;

	internal InterpolatedStringHandlerArgumentPlaceholderOperation(int argumentIndex, InterpolatedStringArgumentPlaceholderKind placeholderKind, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		ArgumentIndex = argumentIndex;
		PlaceholderKind = placeholderKind;
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
		visitor.VisitInterpolatedStringHandlerArgumentPlaceholder(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitInterpolatedStringHandlerArgumentPlaceholder(this, argument);
	}
}
