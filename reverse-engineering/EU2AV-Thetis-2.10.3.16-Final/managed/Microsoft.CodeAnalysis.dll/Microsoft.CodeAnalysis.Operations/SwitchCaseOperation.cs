using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SwitchCaseOperation : Operation, ISwitchCaseOperation, IOperation
{
	public ImmutableArray<ICaseClauseOperation> Clauses { get; }

	public ImmutableArray<IOperation> Body { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	public IOperation? Condition { get; }

	internal override int ChildOperationsCount => Clauses.Length + Body.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.SwitchCase;

	internal SwitchCaseOperation(ImmutableArray<ICaseClauseOperation> clauses, ImmutableArray<IOperation> body, ImmutableArray<ILocalSymbol> locals, IOperation? condition, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Clauses = Operation.SetParentOperation(clauses, this);
		Body = Operation.SetParentOperation(body, this);
		Locals = locals;
		Condition = Operation.SetParentOperation(condition, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (index < Clauses.Length)
			{
				return Clauses[index];
			}
			break;
		case 1:
			if (index < Body.Length)
			{
				return Body[index];
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
			if (!Clauses.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto IL_0053;
		case 0:
			if (previousIndex + 1 < Clauses.Length)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex + 1);
			}
			goto IL_0053;
		case 1:
			if (previousIndex + 1 < Body.Length)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex + 1);
			}
			goto case 2;
		case 2:
			return (hasNext: false, nextSlot: 2, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_0053:
			if (!Body.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 2;
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (!Body.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: Body.Length - 1);
			}
			goto IL_0055;
		case 1:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex - 1);
			}
			goto IL_0055;
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
			IL_0055:
			if (!Clauses.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: Clauses.Length - 1);
			}
			goto case -1;
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSwitchCase(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitSwitchCase(this, argument);
	}
}
