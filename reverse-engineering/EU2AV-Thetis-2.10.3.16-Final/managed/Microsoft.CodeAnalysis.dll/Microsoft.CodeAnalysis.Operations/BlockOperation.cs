using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class BlockOperation : Operation, IBlockOperation, IOperation
{
	public ImmutableArray<IOperation> Operations { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	internal override int ChildOperationsCount => Operations.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Block;

	internal BlockOperation(ImmutableArray<IOperation> operations, ImmutableArray<ILocalSymbol> locals, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Operations = Operation.SetParentOperation(operations, this);
		Locals = locals;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && index < Operations.Length)
		{
			return Operations[index];
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (!Operations.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 1;
		case 0:
			if (previousIndex + 1 < Operations.Length)
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
				if (!Operations.IsEmpty)
				{
					return (hasNext: true, nextSlot: 0, nextIndex: Operations.Length - 1);
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
		visitor.VisitBlock(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitBlock(this, argument);
	}

	public static BlockOperation CreateTemporaryBlock(ImmutableArray<IOperation> statements, SemanticModel semanticModel, SyntaxNode syntax)
	{
		return new BlockOperation(statements, semanticModel, syntax);
	}

	private BlockOperation(ImmutableArray<IOperation> statements, SemanticModel semanticModel, SyntaxNode syntax)
		: base(semanticModel, syntax, isImplicit: true)
	{
		Operations = statements;
		Locals = ImmutableArray<ILocalSymbol>.Empty;
	}
}
