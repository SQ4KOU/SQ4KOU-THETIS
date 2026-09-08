using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ArrayElementReferenceOperation : Operation, IArrayElementReferenceOperation, IOperation
{
	public IOperation ArrayReference { get; }

	public ImmutableArray<IOperation> Indices { get; }

	internal override int ChildOperationsCount => ((ArrayReference != null) ? 1 : 0) + Indices.Length;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.ArrayElementReference;

	internal ArrayElementReferenceOperation(IOperation arrayReference, ImmutableArray<IOperation> indices, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		ArrayReference = Operation.SetParentOperation(arrayReference, this);
		Indices = Operation.SetParentOperation(indices, this);
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (ArrayReference != null)
			{
				return ArrayReference;
			}
			break;
		case 1:
			if (index < Indices.Length)
			{
				return Indices[index];
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
			if (ArrayReference != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (!Indices.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 2;
		case 1:
			if (previousIndex + 1 < Indices.Length)
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
				if (!Indices.IsEmpty)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: Indices.Length - 1);
				}
			}
			else if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex - 1);
			}
			if (ArrayReference != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitArrayElementReference(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitArrayElementReference(this, argument);
	}
}
