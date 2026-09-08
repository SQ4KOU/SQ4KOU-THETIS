namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ConditionalOperation : Operation, IConditionalOperation, IOperation
{
	public IOperation Condition { get; }

	public IOperation WhenTrue { get; }

	public IOperation? WhenFalse { get; }

	public bool IsRef { get; }

	internal override int ChildOperationsCount => ((Condition != null) ? 1 : 0) + ((WhenTrue != null) ? 1 : 0) + ((WhenFalse != null) ? 1 : 0);

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue { get; }

	public override OperationKind Kind => OperationKind.Conditional;

	internal ConditionalOperation(IOperation condition, IOperation whenTrue, IOperation? whenFalse, bool isRef, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Condition = Operation.SetParentOperation(condition, this);
		WhenTrue = Operation.SetParentOperation(whenTrue, this);
		WhenFalse = Operation.SetParentOperation(whenFalse, this);
		IsRef = isRef;
		OperationConstantValue = constantValue;
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Condition != null)
			{
				return Condition;
			}
			break;
		case 1:
			if (WhenTrue != null)
			{
				return WhenTrue;
			}
			break;
		case 2:
			if (WhenFalse != null)
			{
				return WhenFalse;
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
			if (Condition != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (WhenTrue != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (WhenFalse != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
		case 3:
			return (hasNext: false, nextSlot: 3, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (WhenFalse != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (WhenTrue != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (Condition != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case -1;
		case -1:
		case 0:
			return (hasNext: false, nextSlot: -1, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitConditional(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitConditional(this, argument);
	}
}
