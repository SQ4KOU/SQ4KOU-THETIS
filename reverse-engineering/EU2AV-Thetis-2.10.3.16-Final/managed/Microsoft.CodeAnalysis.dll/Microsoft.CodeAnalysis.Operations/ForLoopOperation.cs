using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ForLoopOperation : BaseLoopOperation, IForLoopOperation, ILoopOperation, IOperation
{
	public ImmutableArray<IOperation> Before { get; }

	public ImmutableArray<ILocalSymbol> ConditionLocals { get; }

	public IOperation? Condition { get; }

	public ImmutableArray<IOperation> AtLoopBottom { get; }

	internal override int ChildOperationsCount => Before.Length + ((Condition != null) ? 1 : 0) + AtLoopBottom.Length + ((base.Body != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Loop;

	public override LoopKind LoopKind => LoopKind.For;

	internal ForLoopOperation(ImmutableArray<IOperation> before, ImmutableArray<ILocalSymbol> conditionLocals, IOperation? condition, ImmutableArray<IOperation> atLoopBottom, IOperation body, ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(body, locals, continueLabel, exitLabel, semanticModel, syntax, isImplicit)
	{
		Before = Operation.SetParentOperation(before, this);
		ConditionLocals = conditionLocals;
		Condition = Operation.SetParentOperation(condition, this);
		AtLoopBottom = Operation.SetParentOperation(atLoopBottom, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (index < Before.Length)
			{
				return Before[index];
			}
			break;
		case 1:
			if (Condition != null)
			{
				return Condition;
			}
			break;
		case 2:
			if (base.Body != null)
			{
				return base.Body;
			}
			break;
		case 3:
			if (index < AtLoopBottom.Length)
			{
				return AtLoopBottom[index];
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
			if (!Before.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto IL_005e;
		case 0:
			if (previousIndex + 1 < Before.Length)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex + 1);
			}
			goto IL_005e;
		case 1:
			if (base.Body != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (!AtLoopBottom.IsEmpty)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: 0);
			}
			goto case 4;
		case 3:
			if (previousIndex + 1 < AtLoopBottom.Length)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: previousIndex + 1);
			}
			goto case 4;
		case 4:
			return (hasNext: false, nextSlot: 4, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_005e:
			if (Condition != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (!AtLoopBottom.IsEmpty)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: AtLoopBottom.Length - 1);
			}
			goto IL_0060;
		case 3:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: previousIndex - 1);
			}
			goto IL_0060;
		case 2:
			if (Condition != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (!Before.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: Before.Length - 1);
			}
			goto case -1;
		case 0:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex - 1);
			}
			goto case -1;
		case -1:
			return (hasNext: false, nextSlot: -1, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_0060:
			if (base.Body != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitForLoop(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitForLoop(this, argument);
	}
}
