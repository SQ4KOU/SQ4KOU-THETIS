namespace Microsoft.CodeAnalysis.CodeGen;

internal readonly struct StateMachineStateDebugInfo(int syntaxOffset, AwaitDebugId awaitId, StateMachineState stateNumber)
{
	public readonly int SyntaxOffset = syntaxOffset;

	public readonly AwaitDebugId AwaitId = awaitId;

	public readonly StateMachineState StateNumber = stateNumber;
}
