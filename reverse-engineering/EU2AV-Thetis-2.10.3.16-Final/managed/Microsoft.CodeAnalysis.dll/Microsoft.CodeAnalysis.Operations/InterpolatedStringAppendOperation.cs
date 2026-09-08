namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolatedStringAppendOperation : BaseInterpolatedStringContentOperation, IInterpolatedStringAppendOperation, IInterpolatedStringContentOperation, IOperation
{
	public IOperation AppendCall { get; }

	internal override int ChildOperationsCount => (AppendCall != null) ? 1 : 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind { get; }

	internal InterpolatedStringAppendOperation(IOperation appendCall, OperationKind kind, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		AppendCall = Operation.SetParentOperation(appendCall, this);
		Kind = kind;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && AppendCall != null)
		{
			return AppendCall;
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
		else if (AppendCall != null)
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
			if (AppendCall != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInterpolatedStringAppend(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitInterpolatedStringAppend(this, argument);
	}
}
