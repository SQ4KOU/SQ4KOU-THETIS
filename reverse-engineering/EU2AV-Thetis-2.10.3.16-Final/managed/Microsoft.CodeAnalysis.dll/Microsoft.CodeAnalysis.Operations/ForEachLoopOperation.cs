using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ForEachLoopOperation : BaseLoopOperation, IForEachLoopOperation, ILoopOperation, IOperation
{
	public IOperation LoopControlVariable { get; }

	public IOperation Collection { get; }

	public ImmutableArray<IOperation> NextVariables { get; }

	public ForEachLoopOperationInfo? Info { get; }

	public bool IsAsynchronous { get; }

	internal override int ChildOperationsCount => ((LoopControlVariable != null) ? 1 : 0) + ((Collection != null) ? 1 : 0) + NextVariables.Length + ((base.Body != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Loop;

	public override LoopKind LoopKind => LoopKind.ForEach;

	internal ForEachLoopOperation(IOperation loopControlVariable, IOperation collection, ImmutableArray<IOperation> nextVariables, ForEachLoopOperationInfo? info, bool isAsynchronous, IOperation body, ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(body, locals, continueLabel, exitLabel, semanticModel, syntax, isImplicit)
	{
		LoopControlVariable = Operation.SetParentOperation(loopControlVariable, this);
		Collection = Operation.SetParentOperation(collection, this);
		NextVariables = Operation.SetParentOperation(nextVariables, this);
		Info = info;
		IsAsynchronous = isAsynchronous;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Collection != null)
			{
				return Collection;
			}
			break;
		case 1:
			if (LoopControlVariable != null)
			{
				return LoopControlVariable;
			}
			break;
		case 2:
			if (base.Body != null)
			{
				return base.Body;
			}
			break;
		case 3:
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
			if (Collection != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (LoopControlVariable != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (base.Body != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (!NextVariables.IsEmpty)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: 0);
			}
			goto case 4;
		case 3:
			if (previousIndex + 1 < NextVariables.Length)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: previousIndex + 1);
			}
			goto case 4;
		case 4:
			return (hasNext: false, nextSlot: 4, nextIndex: 0);
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
				return (hasNext: true, nextSlot: 3, nextIndex: NextVariables.Length - 1);
			}
			goto IL_005d;
		case 3:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 3, nextIndex: previousIndex - 1);
			}
			goto IL_005d;
		case 2:
			if (LoopControlVariable != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (Collection != null)
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
			IL_005d:
			if (base.Body != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitForEachLoop(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitForEachLoop(this, argument);
	}
}
