namespace Microsoft.CodeAnalysis.Emit;

internal interface IPEDeltaAssemblyBuilder
{
	void OnCreatedIndices(DiagnosticBag diagnostics);
}
