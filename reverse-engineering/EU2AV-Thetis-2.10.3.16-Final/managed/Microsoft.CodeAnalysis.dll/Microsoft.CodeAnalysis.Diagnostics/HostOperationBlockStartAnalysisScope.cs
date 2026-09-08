using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class HostOperationBlockStartAnalysisScope(DiagnosticAnalyzer analyzer)
{
	private ImmutableArray<OperationBlockAnalyzerAction> _operationBlockEndActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationAnalyzerAction> _operationActions = ImmutableArray<OperationAnalyzerAction>.Empty;

	private DiagnosticAnalyzer Analyzer { get; } = analyzer;

	public ImmutableArray<OperationBlockAnalyzerAction> OperationBlockEndActions => _operationBlockEndActions;

	public ImmutableArray<OperationAnalyzerAction> OperationActions => _operationActions;

	public void RegisterOperationBlockEndAction(Action<OperationBlockAnalysisContext> action)
	{
		_operationBlockEndActions = _operationBlockEndActions.Add(new OperationBlockAnalyzerAction(action, Analyzer));
	}

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		_operationActions = _operationActions.Add(new OperationAnalyzerAction(action, operationKinds, Analyzer));
	}
}
