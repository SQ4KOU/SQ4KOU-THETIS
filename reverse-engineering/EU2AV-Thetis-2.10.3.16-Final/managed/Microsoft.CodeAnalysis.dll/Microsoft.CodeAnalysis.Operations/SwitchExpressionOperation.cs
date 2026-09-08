using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SwitchExpressionOperation : Operation, ISwitchExpressionOperation, IOperation
{
	public IOperation Value { get; }

	public ImmutableArray<ISwitchExpressionArmOperation> Arms { get; }

	public bool IsExhaustive { get; }

	internal override int ChildOperationsCount => ((Value != null) ? 1 : 0) + Arms.Length;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.SwitchExpression;

	internal SwitchExpressionOperation(IOperation value, ImmutableArray<ISwitchExpressionArmOperation> arms, bool isExhaustive, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
		Arms = Operation.SetParentOperation(arms, this);
		IsExhaustive = isExhaustive;
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Value != null)
			{
				return Value;
			}
			break;
		case 1:
			if (index < Arms.Length)
			{
				return Arms[index];
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
			if (Value != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (!Arms.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 2;
		case 1:
			if (previousIndex + 1 < Arms.Length)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex + 1);
			}
			goto case 2;
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
				if (!Arms.IsEmpty)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: Arms.Length - 1);
				}
			}
			else if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex - 1);
			}
			if (Value != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSwitchExpression(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitSwitchExpression(this, argument);
	}
}
