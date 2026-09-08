using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SwitchExpressionArmOperation : Operation, ISwitchExpressionArmOperation, IOperation
{
	public IPatternOperation Pattern { get; }

	public IOperation? Guard { get; }

	public IOperation Value { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	internal override int ChildOperationsCount => ((Pattern != null) ? 1 : 0) + ((Guard != null) ? 1 : 0) + ((Value != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.SwitchExpressionArm;

	internal SwitchExpressionArmOperation(IPatternOperation pattern, IOperation? guard, IOperation value, ImmutableArray<ILocalSymbol> locals, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Pattern = Operation.SetParentOperation(pattern, this);
		Guard = Operation.SetParentOperation(guard, this);
		Value = Operation.SetParentOperation(value, this);
		Locals = locals;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Pattern != null)
			{
				return Pattern;
			}
			break;
		case 1:
			if (Guard != null)
			{
				return Guard;
			}
			break;
		case 2:
			if (Value != null)
			{
				return Value;
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
			if (Pattern != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Guard != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (Value != null)
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
			if (Value != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (Guard != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (Pattern != null)
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
		visitor.VisitSwitchExpressionArm(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitSwitchExpressionArm(this, argument);
	}
}
