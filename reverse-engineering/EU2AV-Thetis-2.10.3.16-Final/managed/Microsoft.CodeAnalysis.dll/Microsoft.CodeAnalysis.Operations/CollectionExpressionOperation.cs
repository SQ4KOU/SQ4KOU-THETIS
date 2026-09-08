using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class CollectionExpressionOperation : Operation, ICollectionExpressionOperation, IOperation
{
	public IMethodSymbol? ConstructMethod { get; }

	public ImmutableArray<IOperation> Elements { get; }

	internal override int ChildOperationsCount => Elements.Length;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.CollectionExpression;

	internal CollectionExpressionOperation(IMethodSymbol? constructMethod, ImmutableArray<IOperation> elements, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		ConstructMethod = constructMethod;
		Elements = Operation.SetParentOperation(elements, this);
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && index < Elements.Length)
		{
			return Elements[index];
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (!Elements.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 1;
		case 0:
			if (previousIndex + 1 < Elements.Length)
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
				if (!Elements.IsEmpty)
				{
					return (hasNext: true, nextSlot: 0, nextIndex: Elements.Length - 1);
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
		visitor.VisitCollectionExpression(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitCollectionExpression(this, argument);
	}
}
