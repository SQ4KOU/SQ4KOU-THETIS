namespace Microsoft.CodeAnalysis.CSharp;

internal enum NullableFlowState : byte
{
	NotNull = 0,
	MaybeNull = 1,
	MaybeDefault = 3
}
