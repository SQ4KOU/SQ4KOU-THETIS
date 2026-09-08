namespace Microsoft.CodeAnalysis.Operations;

internal sealed class UnaryOperation : Operation, IUnaryOperation, IOperation
{
	public UnaryOperatorKind OperatorKind { get; }

	public IOperation Operand { get; }

	public bool IsLifted { get; }

	public bool IsChecked { get; }

	public IMethodSymbol? OperatorMethod { get; }

	public ITypeSymbol? ConstrainedToType { get; }

	internal override int ChildOperationsCount => (Operand != null) ? 1 : 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue { get; }

	public override OperationKind Kind => OperationKind.Unary;

	internal UnaryOperation(UnaryOperatorKind operatorKind, IOperation operand, bool isLifted, bool isChecked, IMethodSymbol? operatorMethod, ITypeSymbol? constrainedToType, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		OperatorKind = operatorKind;
		Operand = Operation.SetParentOperation(operand, this);
		IsLifted = isLifted;
		IsChecked = isChecked;
		OperatorMethod = operatorMethod;
		ConstrainedToType = constrainedToType;
		OperationConstantValue = constantValue;
		Type = type;
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
		visitor.VisitUnaryOperator(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitUnaryOperator(this, argument);
	}
}
