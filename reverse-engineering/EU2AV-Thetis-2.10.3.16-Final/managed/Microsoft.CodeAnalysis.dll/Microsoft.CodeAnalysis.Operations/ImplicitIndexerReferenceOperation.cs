namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ImplicitIndexerReferenceOperation : Operation, IImplicitIndexerReferenceOperation, IOperation
{
	public IOperation Instance { get; }

	public IOperation Argument { get; }

	public ISymbol LengthSymbol { get; }

	public ISymbol IndexerSymbol { get; }

	internal override int ChildOperationsCount => ((Instance != null) ? 1 : 0) + ((Argument != null) ? 1 : 0);

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.ImplicitIndexerReference;

	internal ImplicitIndexerReferenceOperation(IOperation instance, IOperation argument, ISymbol lengthSymbol, ISymbol indexerSymbol, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Instance = Operation.SetParentOperation(instance, this);
		Argument = Operation.SetParentOperation(argument, this);
		LengthSymbol = lengthSymbol;
		IndexerSymbol = indexerSymbol;
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Instance != null)
			{
				return Instance;
			}
			break;
		case 1:
			if (Argument != null)
			{
				return Argument;
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
			if (Instance != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Argument != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
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
				if (Argument != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (Instance != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitImplicitIndexerReference(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitImplicitIndexerReference(this, argument);
	}
}
