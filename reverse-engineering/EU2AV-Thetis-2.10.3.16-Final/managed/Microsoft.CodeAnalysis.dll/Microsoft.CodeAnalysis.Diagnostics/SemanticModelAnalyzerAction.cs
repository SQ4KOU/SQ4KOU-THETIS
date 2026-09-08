using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class SemanticModelAnalyzerAction : AnalyzerAction
{
	public Action<SemanticModelAnalysisContext> Action { get; }

	public SemanticModelAnalyzerAction(Action<SemanticModelAnalysisContext> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
	}
}
