using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ArrayInitializerOperation : Operation, IArrayInitializerOperation, IOperation
{
	public ImmutableArray<IOperation> ElementValues { get; }

	internal override int ChildOperationsCount => ElementValues.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.ArrayInitializer;

	internal ArrayInitializerOperation(ImmutableArray<IOperation> elementValues, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		ElementValues = Operation.SetParentOperation(elementValues, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && index < ElementValues.Length)
		{
			return ElementValues[index];
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (!ElementValues.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 1;
		case 0:
			if (previousIndex + 1 < ElementValues.Length)
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
				if (!ElementValues.IsEmpty)
				{
					return (hasNext: true, nextSlot: 0, nextIndex: ElementValues.Length - 1);
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
		visitor.VisitArrayInitializer(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitArrayInitializer(this, argument);
	}
}
