namespace Microsoft.CodeAnalysis.Operations;

internal sealed class IncrementOrDecrementOperation : Operation, IIncrementOrDecrementOperation, IOperation
{
	public bool IsPostfix { get; }

	public bool IsLifted { get; }

	public bool IsChecked { get; }

	public IOperation Target { get; }

	public IMethodSymbol? OperatorMethod { get; }

	public ITypeSymbol? ConstrainedToType { get; }

	internal override int ChildOperationsCount => (Target != null) ? 1 : 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind { get; }

	internal IncrementOrDecrementOperation(bool isPostfix, bool isLifted, bool isChecked, IOperation target, IMethodSymbol? operatorMethod, ITypeSymbol? constrainedToType, OperationKind kind, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		IsPostfix = isPostfix;
		IsLifted = isLifted;
		IsChecked = isChecked;
		Target = Operation.SetParentOperation(target, this);
		OperatorMethod = operatorMethod;
		ConstrainedToType = constrainedToType;
		Type = type;
		Kind = kind;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && Target != null)
		{
			return Target;
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
		else if (Target != null)
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
			if (Target != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitIncrementOrDecrement(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitIncrementOrDecrement(this, argument);
	}
}
