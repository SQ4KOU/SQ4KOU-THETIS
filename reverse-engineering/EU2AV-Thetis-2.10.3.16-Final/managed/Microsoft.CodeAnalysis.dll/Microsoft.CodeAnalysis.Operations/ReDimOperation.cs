using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ReDimOperation : Operation, IReDimOperation, IOperation
{
	public ImmutableArray<IReDimClauseOperation> Clauses { get; }

	public bool Preserve { get; }

	internal override int ChildOperationsCount => Clauses.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.ReDim;

	internal ReDimOperation(ImmutableArray<IReDimClauseOperation> clauses, bool preserve, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Clauses = Operation.SetParentOperation(clauses, this);
		Preserve = preserve;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && index < Clauses.Length)
		{
			return Clauses[index];
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
			goto case 1;
		case 0:
			if (previousIndex + 1 < Clauses.Length)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex + 1);
			}
			goto case 1;
		case 1:
			return (hasNext: false, nextSlot: 1, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		if (previousSlot != -1)
		{
			if (previousSlot != 0)
			{
				if (previousSlot != int.MaxValue)
				{
					throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
				}
				if (!Clauses.IsEmpty)
				{
					return (hasNext: true, nextSlot: 0, nextIndex: Clauses.Length - 1);
				}
			}
			else if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex - 1);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitReDim(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitReDim(this, argument);
	}
}
