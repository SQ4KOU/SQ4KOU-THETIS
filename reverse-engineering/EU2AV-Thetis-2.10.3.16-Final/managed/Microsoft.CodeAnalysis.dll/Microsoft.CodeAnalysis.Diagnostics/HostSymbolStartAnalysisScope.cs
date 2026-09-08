namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class HostSymbolStartAnalysisScope : HostAnalysisScope
{
	public HostSymbolStartAnalysisScope(DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
	}
}
