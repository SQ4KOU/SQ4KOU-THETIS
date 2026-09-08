namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class HostCompilationStartAnalysisScope : HostAnalysisScope
{
	private readonly HostSessionStartAnalysisScope _sessionScope;

	public HostCompilationStartAnalysisScope(HostSessionStartAnalysisScope sessionScope)
		: base(sessionScope.Analyzer)
	{
		_sessionScope = sessionScope;
	}

	public override AnalyzerActions GetAnalyzerActions()
	{
		AnalyzerActions analyzerActions = base.GetAnalyzerActions();
		AnalyzerActions otherActions = _sessionScope.GetAnalyzerActions();
		if (otherActions.IsEmpty)
		{
			return analyzerActions;
		}
		if (analyzerActions.IsEmpty)
		{
			return otherActions;
		}
		return analyzerActions.Append(in otherActions);
	}
}
