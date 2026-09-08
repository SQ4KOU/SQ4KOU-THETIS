using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class WhileLoopOperation : BaseLoopOperation, IWhileLoopOperation, ILoopOperation, IOperation
{
	public IOperation? Condition { get; }

	public bool ConditionIsTop { get; }

	public bool ConditionIsUntil { get; }

	public IOperation? IgnoredCondition { get; }

	internal override int ChildOperationsCount => ((Condition != null) ? 1 : 0) + ((IgnoredCondition != null) ? 1 : 0) + ((base.Body != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Loop;

	public override LoopKind LoopKind => LoopKind.While;

	internal WhileLoopOperation(IOperation? condition, bool conditionIsTop, bool conditionIsUntil, IOperation? ignoredCondition, IOperation body, ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(body, locals, continueLabel, exitLabel, semanticModel, syntax, isImplicit)
	{
		Condition = Operation.SetParentOperation(condition, this);
		ConditionIsTop = conditionIsTop;
		ConditionIsUntil = conditionIsUntil;
		IgnoredCondition = Operation.SetParentOperation(ignoredCondition, this);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitWhileLoop(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitWhileLoop(this, argument);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (!ConditionIsTop)
		{
			return getCurrentSwitchBottom();
		}
		return getCurrentSwitchTop();
		IOperation getCurrentSwitchBottom()
		{
			switch (slot)
			{
			case 0:
				if (base.Body != null)
				{
					return base.Body;
				}
				break;
			case 1:
				if (Condition != null)
				{
					return Condition;
				}
				break;
			case 2:
				if (IgnoredCondition != null)
				{
					return IgnoredCondition;
				}
				break;
			}
			throw ExceptionUtilities.UnexpectedValue((slot, index));
		}
		IOperation getCurrentSwitchTop()
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
				if (base.Body != null)
				{
					return base.Body;
				}
				break;
			case 2:
				if (IgnoredCondition != null)
				{
					return IgnoredCondition;
				}
				break;
			}
			throw ExceptionUtilities.UnexpectedValue((slot, index));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		if (!ConditionIsTop)
		{
			return moveNextConditionIsBottom();
		}
		return moveNextConditionIsTop();
		(bool hasNext, int nextSlot, int nextIndex) moveNextConditionIsBottom()
		{
			switch (previousSlot)
			{
			case -1:
				if (base.Body != null)
				{
					return (hasNext: true, nextSlot: 0, nextIndex: 0);
				}
				goto case 0;
			case 0:
				if (Condition != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
				goto case 1;
			case 1:
				if (IgnoredCondition != null)
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
		(bool hasNext, int nextSlot, int nextIndex) moveNextConditionIsTop()
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
				if (base.Body != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
				goto case 1;
			case 1:
				if (IgnoredCondition != null)
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
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		if (!ConditionIsTop)
		{
			return moveNextConditionIsBottom();
		}
		return moveNextConditionIsTop();
		(bool hasNext, int nextSlot, int nextIndex) moveNextConditionIsBottom()
		{
			switch (previousSlot)
			{
			case int.MaxValue:
				if (IgnoredCondition != null)
				{
					return (hasNext: true, nextSlot: 2, nextIndex: 0);
				}
				goto case 2;
			case 2:
				if (Condition != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
				goto case 1;
			case 1:
				if (base.Body != null)
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
		(bool hasNext, int nextSlot, int nextIndex) moveNextConditionIsTop()
		{
			switch (previousSlot)
			{
			case int.MaxValue:
				if (IgnoredCondition != null)
				{
					return (hasNext: true, nextSlot: 2, nextIndex: 0);
				}
				goto case 2;
			case 2:
				if (base.Body != null)
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
	}
}
