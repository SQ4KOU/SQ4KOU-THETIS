namespace Microsoft.CodeAnalysis;

internal enum StateMachineState
{
	FirstResumableAsyncIteratorState = -4,
	InitialAsyncIteratorState = -3,
	FirstIteratorFinalizeState = InitialAsyncIteratorState,
	FinishedState = -2,
	NotStartedOrRunningState = -1,
	FirstUnusedState = 0,
	FirstResumableAsyncState = FirstUnusedState,
	InitialIteratorState = FirstUnusedState,
	FirstResumableIteratorState = 1
}
