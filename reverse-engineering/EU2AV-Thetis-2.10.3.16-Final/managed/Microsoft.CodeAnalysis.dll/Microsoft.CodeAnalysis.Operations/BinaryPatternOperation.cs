namespace Microsoft.CodeAnalysis.Operations;

internal sealed class BinaryPatternOperation : BasePatternOperation, IBinaryPatternOperation, IPatternOperation, IOperation
{
	public BinaryOperatorKind OperatorKind { get; }

	public IPatternOperation LeftPattern { get; }

	public IPatternOperation RightPattern { get; }

	internal override int ChildOperationsCount => ((LeftPattern != null) ? 1 : 0) + ((RightPattern != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.BinaryPattern;

	internal BinaryPatternOperation(BinaryOperatorKind operatorKind, IPatternOperation leftPattern, IPatternOperation rightPattern, ITypeSymbol inputType, ITypeSymbol narrowedType, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(inputType, narrowedType, semanticModel, syntax, isImplicit)
	{
		OperatorKind = operatorKind;
		LeftPattern = Operation.SetParentOperation(leftPattern, this);
		RightPattern = Operation.SetParentOperation(rightPattern, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (LeftPattern != null)
			{
				return LeftPattern;
			}
			break;
		case 1:
			if (RightPattern != null)
			{
				return RightPattern;
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
			if (LeftPattern != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (RightPattern != null)
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
				if (RightPattern != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (LeftPattern != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitBinaryPattern(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitBinaryPattern(this, argument);
	}
}
