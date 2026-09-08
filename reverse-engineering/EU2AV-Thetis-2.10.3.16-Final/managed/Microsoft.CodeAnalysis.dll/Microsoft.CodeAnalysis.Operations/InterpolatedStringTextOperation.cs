namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolatedStringTextOperation : BaseInterpolatedStringContentOperation, IInterpolatedStringTextOperation, IInterpolatedStringContentOperation, IOperation
{
	public IOperation Text { get; }

	internal override int ChildOperationsCount => (Text != null) ? 1 : 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.InterpolatedStringText;

	internal InterpolatedStringTextOperation(IOperation text, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Text = Operation.SetParentOperation(text, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && Text != null)
		{
			return Text;
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		if (previousSlot != -1)
		{
			if ((uint)previousSlot > 1u)
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
		}
		else if (Text != null)
		{
			return (hasNext: true, nextSlot: 0, nextIndex: 0);
		}
		return (hasNext: false, nextSlot: 1, nextIndex: 0);
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		if ((uint)(previousSlot - -1) > 1u)
		{
			if (previousSlot != int.MaxValue)
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			if (Text != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInterpolatedStringText(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitInterpolatedStringText(this, argument);
	}
}
