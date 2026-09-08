namespace Microsoft.CodeAnalysis.Operations;

internal sealed class TypePatternOperation : BasePatternOperation, ITypePatternOperation, IPatternOperation, IOperation
{
	public ITypeSymbol MatchedType { get; }

	internal override int ChildOperationsCount => 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.TypePattern;

	internal TypePatternOperation(ITypeSymbol matchedType, ITypeSymbol inputType, ITypeSymbol narrowedType, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(inputType, narrowedType, semanticModel, syntax, isImplicit)
	{
		MatchedType = matchedType;
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
		visitor.VisitTypePattern(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitTypePattern(this, argument);
	}
}
