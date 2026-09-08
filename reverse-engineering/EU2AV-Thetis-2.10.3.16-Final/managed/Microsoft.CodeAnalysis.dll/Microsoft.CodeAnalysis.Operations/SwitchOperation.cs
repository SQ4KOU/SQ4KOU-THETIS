using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SwitchOperation : Operation, ISwitchOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public IOperation Value { get; }

	public ImmutableArray<ISwitchCaseOperation> Cases { get; }

	public ILabelSymbol ExitLabel { get; }

	internal override int ChildOperationsCount => ((Value != null) ? 1 : 0) + Cases.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Switch;

	internal SwitchOperation(ImmutableArray<ILocalSymbol> locals, IOperation value, ImmutableArray<ISwitchCaseOperation> cases, ILabelSymbol exitLabel, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Locals = locals;
		Value = Operation.SetParentOperation(value, this);
		Cases = Operation.SetParentOperation(cases, this);
		ExitLabel = exitLabel;
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
			if (index < Cases.Length)
			{
				return Cases[index];
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
			if (!Cases.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 2;
		case 1:
			if (previousIndex + 1 < Cases.Length)
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
				if (!Cases.IsEmpty)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: Cases.Length - 1);
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
		visitor.VisitSwitch(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitSwitch(this, argument);
	}
}
