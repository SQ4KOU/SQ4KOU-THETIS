using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ForToLoopOperation : BaseLoopOperation, IForToLoopOperation, ILoopOperation, IOperation
{
	public IOperation LoopControlVariable { get; }

	public IOperation InitialValue { get; }

	public IOperation LimitValue { get; }

	public IOperation StepValue { get; }

	public bool IsChecked { get; }

	public ImmutableArray<IOperation> NextVariables { get; }

	public (ILocalSymbol LoopObject, ForToLoopOperationUserDefinedInfo UserDefinedInfo) Info { get; }

	internal override int ChildOperationsCount => ((LoopControlVariable != null) ? 1 : 0) + ((InitialValue != null) ? 1 : 0) + ((LimitValue != null) ? 1 : 0) + ((StepValue != null) ? 1 : 0) + NextVariables.Length + ((base.Body != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Loop;

	public override LoopKind LoopKind => LoopKind.ForTo;

	internal ForToLoopOperation(IOperation loopControlVariable, IOperation initialValue, IOperation limitValue, IOperation stepValue, bool isChecked, ImmutableArray<IOperation> nextVariables, (ILocalSymbol LoopObject, ForToLoopOperationUserDefinedInfo UserDefinedInfo) info, IOperation body, ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(body, locals, continueLabel, exitLabel, semanticModel, syntax, isImplicit)
	{
		LoopControlVariable = Operation.SetParentOperation(loopControlVariable, this);
		InitialValue = Operation.SetParentOperation(initialValue, this);
		LimitValue = Operation.SetParentOperation(limitValue, this);
		StepValue = Operation.SetParentOperation(stepValue, this);
		IsChecked = isChecked;
		NextVariables = Operation.SetParentOperation(nextVariables, this);
		Info = info;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (LoopControlVariable != null)
			{
				return LoopControlVariable;
			}
			break;
		case 1:
			if (InitialValue != null)
			{
				return InitialValue;
			}
			break;
		case 2:
			if (LimitValue != null)
			{
				return LimitValue;
			}
			break;
		case 3:
			if (StepValue != null)
			{
				return StepValue;
			}
			break;
		case 4:
			if (base.Body != null)
			{
				return base.Body;
			}
			break;
		case 5:
			if (index < NextVariables.Length)
			{
				return NextVariables[index];
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
			if (LoopControlVariable != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (InitialValue != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (LimitValue != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (StepValue != null)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: 0);
			}
			goto case 3;
		case 3:
			if (base.Body != null)
			{
				return (hasNext: true, nextSlot: 4, nextIndex: 0);
			}
			goto case 4;
		case 4:
			if (!NextVariables.IsEmpty)
			{
				return (hasNext: true, nextSlot: 5, nextIndex: 0);
			}
			goto case 6;
		case 5:
			if (previousIndex + 1 < NextVariables.Length)
			{
				return (hasNext: true, nextSlot: 5, nextIndex: previousIndex + 1);
			}
			goto case 6;
		case 6:
			return (hasNext: false, nextSlot: 6, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (!NextVariables.IsEmpty)
			{
				return (hasNext: true, nextSlot: 5, nextIndex: NextVariables.Length - 1);
			}
			goto IL_0068;
		case 5:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 5, nextIndex: previousIndex - 1);
			}
			goto IL_0068;
		case 4:
			if (StepValue != null)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: 0);
			}
			goto case 3;
		case 3:
			if (LimitValue != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (InitialValue != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (LoopControlVariable != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case -1;
		case -1:
		case 0:
			return (hasNext: false, nextSlot: -1, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_0068:
			if (base.Body != null)
			{
				return (hasNext: true, nextSlot: 4, nextIndex: 0);
			}
			goto case 4;
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitForToLoop(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitForToLoop(this, argument);
	}
}
