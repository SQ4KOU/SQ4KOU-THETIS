using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class SymbolEndAnalyzerAction : AnalyzerAction
{
	public Action<SymbolAnalysisContext> Action { get; }

	public SymbolEndAnalyzerAction(Action<SymbolAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
