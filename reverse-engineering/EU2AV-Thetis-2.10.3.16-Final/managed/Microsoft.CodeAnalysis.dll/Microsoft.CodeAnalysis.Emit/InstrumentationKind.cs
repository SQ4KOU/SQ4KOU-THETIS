namespace Microsoft.CodeAnalysis.Emit;

public enum InstrumentationKind
{
	None,
	TestCoverage,
	StackOverflowProbing,
	ModuleCancellation
}
