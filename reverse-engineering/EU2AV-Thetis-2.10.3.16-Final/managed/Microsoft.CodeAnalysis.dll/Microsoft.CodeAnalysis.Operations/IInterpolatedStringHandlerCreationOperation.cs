namespace Microsoft.CodeAnalysis.Operations;

public interface IInterpolatedStringHandlerCreationOperation : IOperation
{
	IOperation HandlerCreation { get; }

	bool HandlerCreationHasSuccessParameter { get; }

	bool HandlerAppendCallsReturnBool { get; }

	IOperation Content { get; }
}
