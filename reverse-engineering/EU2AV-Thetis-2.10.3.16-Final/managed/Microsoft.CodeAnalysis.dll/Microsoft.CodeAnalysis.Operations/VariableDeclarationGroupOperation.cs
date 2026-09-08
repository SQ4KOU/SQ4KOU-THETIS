using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class VariableDeclarationGroupOperation : Operation, IVariableDeclarationGroupOperation, IOperation
{
	public ImmutableArray<IVariableDeclarationOperation> Declarations { get; }

	internal override int ChildOperationsCount => Declarations.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.VariableDeclarationGroup;

	internal VariableDeclarationGroupOperation(ImmutableArray<IVariableDeclarationOperation> declarations, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Declarations = Operation.SetParentOperation(declarations, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && index < Declarations.Length)
		{
			return Declarations[index];
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (!Declarations.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 1;
		case 0:
			if (previousIndex + 1 < Declarations.Length)
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
				if (!Declarations.IsEmpty)
				{
					return (hasNext: true, nextSlot: 0, nextIndex: Declarations.Length - 1);
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
		visitor.VisitVariableDeclarationGroup(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitVariableDeclarationGroup(this, argument);
	}
}
