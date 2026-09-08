using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class TryOperation : Operation, ITryOperation, IOperation
{
	public IBlockOperation Body { get; }

	public ImmutableArray<ICatchClauseOperation> Catches { get; }

	public IBlockOperation? Finally { get; }

	public ILabelSymbol? ExitLabel { get; }

	internal override int ChildOperationsCount => ((Body != null) ? 1 : 0) + Catches.Length + ((Finally != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Try;

	internal TryOperation(IBlockOperation body, ImmutableArray<ICatchClauseOperation> catches, IBlockOperation? @finally, ILabelSymbol? exitLabel, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Body = Operation.SetParentOperation(body, this);
		Catches = Operation.SetParentOperation(catches, this);
		Finally = Operation.SetParentOperation(@finally, this);
		ExitLabel = exitLabel;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Body != null)
			{
				return Body;
			}
			break;
		case 1:
			if (index < Catches.Length)
			{
				return Catches[index];
			}
			break;
		case 2:
			if (Finally != null)
			{
				return Finally;
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
			if (Body != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (!Catches.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto IL_0068;
		case 1:
			if (previousIndex + 1 < Catches.Length)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex + 1);
			}
			goto IL_0068;
		case 2:
		case 3:
			return (hasNext: false, nextSlot: 3, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_0068:
			if (Finally != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (Finally != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (!Catches.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: Catches.Length - 1);
			}
			goto IL_006a;
		case 1:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex - 1);
			}
			goto IL_006a;
		case -1:
		case 0:
			return (hasNext: false, nextSlot: -1, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_006a:
			if (Body != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case -1;
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTry(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitTry(this, argument);
	}
}
