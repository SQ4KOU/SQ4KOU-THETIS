using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolatedStringOperation : Operation, IInterpolatedStringOperation, IOperation
{
	public ImmutableArray<IInterpolatedStringContentOperation> Parts { get; }

	internal override int ChildOperationsCount => Parts.Length;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue { get; }

	public override OperationKind Kind => OperationKind.InterpolatedString;

	internal InterpolatedStringOperation(ImmutableArray<IInterpolatedStringContentOperation> parts, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Parts = Operation.SetParentOperation(parts, this);
		OperationConstantValue = constantValue;
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && index < Parts.Length)
		{
			return Parts[index];
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (!Parts.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 1;
		case 0:
			if (previousIndex + 1 < Parts.Length)
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
				if (!Parts.IsEmpty)
				{
					return (hasNext: true, nextSlot: 0, nextIndex: Parts.Length - 1);
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
		visitor.VisitInterpolatedString(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitInterpolatedString(this, argument);
	}
}
