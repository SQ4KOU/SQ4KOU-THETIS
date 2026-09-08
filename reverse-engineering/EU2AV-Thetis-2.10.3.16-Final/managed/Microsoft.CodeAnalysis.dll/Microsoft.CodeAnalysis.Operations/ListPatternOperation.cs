using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ListPatternOperation : BasePatternOperation, IListPatternOperation, IPatternOperation, IOperation
{
	public ISymbol? LengthSymbol { get; }

	public ISymbol? IndexerSymbol { get; }

	public ImmutableArray<IPatternOperation> Patterns { get; }

	public ISymbol? DeclaredSymbol { get; }

	internal override int ChildOperationsCount => Patterns.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.ListPattern;

	internal ListPatternOperation(ISymbol? lengthSymbol, ISymbol? indexerSymbol, ImmutableArray<IPatternOperation> patterns, ISymbol? declaredSymbol, ITypeSymbol inputType, ITypeSymbol narrowedType, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(inputType, narrowedType, semanticModel, syntax, isImplicit)
	{
		LengthSymbol = lengthSymbol;
		IndexerSymbol = indexerSymbol;
		Patterns = Operation.SetParentOperation(patterns, this);
		DeclaredSymbol = declaredSymbol;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && index < Patterns.Length)
		{
			return Patterns[index];
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (!Patterns.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 1;
		case 0:
			if (previousIndex + 1 < Patterns.Length)
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
				if (!Patterns.IsEmpty)
				{
					return (hasNext: true, nextSlot: 0, nextIndex: Patterns.Length - 1);
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
		visitor.VisitListPattern(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitListPattern(this, argument);
	}
}
