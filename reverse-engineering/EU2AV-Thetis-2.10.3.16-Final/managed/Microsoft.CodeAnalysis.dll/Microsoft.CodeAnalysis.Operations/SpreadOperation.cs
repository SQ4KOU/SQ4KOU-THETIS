namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SpreadOperation : Operation, ISpreadOperation, IOperation
{
	public IOperation Operand { get; }

	public ITypeSymbol? ElementType { get; }

	internal IConvertibleConversion ElementConversionConvertible { get; }

	public CommonConversion ElementConversion => ElementConversionConvertible.ToCommonConversion();

	internal override int ChildOperationsCount => (Operand != null) ? 1 : 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Spread;

	internal SpreadOperation(IOperation operand, ITypeSymbol? elementType, IConvertibleConversion elementConversion, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Operand = Operation.SetParentOperation(operand, this);
		ElementType = elementType;
		ElementConversionConvertible = elementConversion;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && Operand != null)
		{
			return Operand;
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
		else if (Operand != null)
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
			if (Operand != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSpread(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitSpread(this, argument);
	}
}
