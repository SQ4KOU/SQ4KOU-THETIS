namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolatedStringHandlerCreationOperation : Operation, IInterpolatedStringHandlerCreationOperation, IOperation
{
	public IOperation HandlerCreation { get; }

	public bool HandlerCreationHasSuccessParameter { get; }

	public bool HandlerAppendCallsReturnBool { get; }

	public IOperation Content { get; }

	internal override int ChildOperationsCount => ((HandlerCreation != null) ? 1 : 0) + ((Content != null) ? 1 : 0);

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.InterpolatedStringHandlerCreation;

	internal InterpolatedStringHandlerCreationOperation(IOperation handlerCreation, bool handlerCreationHasSuccessParameter, bool handlerAppendCallsReturnBool, IOperation content, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		HandlerCreation = Operation.SetParentOperation(handlerCreation, this);
		HandlerCreationHasSuccessParameter = handlerCreationHasSuccessParameter;
		HandlerAppendCallsReturnBool = handlerAppendCallsReturnBool;
		Content = Operation.SetParentOperation(content, this);
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (HandlerCreation != null)
			{
				return HandlerCreation;
			}
			break;
		case 1:
			if (Content != null)
			{
				return Content;
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
			if (HandlerCreation != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Content != null)
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
				if (Content != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (HandlerCreation != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInterpolatedStringHandlerCreation(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitInterpolatedStringHandlerCreation(this, argument);
	}
}
