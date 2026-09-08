using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class HostSessionStartAnalysisScope(DiagnosticAnalyzer analyzer) : HostAnalysisScope(analyzer)
{
	private bool _isConcurrent;

	private GeneratedCodeAnalysisFlags _generatedCodeConfiguration = GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics;

	public bool IsConcurrentAnalyzer()
	{
		return _isConcurrent;
	}

	public GeneratedCodeAnalysisFlags GetGeneratedCodeAnalysisFlags()
	{
		return _generatedCodeConfiguration;
	}

	public void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action)
	{
		CompilationStartAnalyzerAction action2 = new CompilationStartAnalyzerAction(action, base.Analyzer);
		GetOrCreateAnalyzerActions().Value.AddCompilationStartAction(action2);
	}

	public void EnableConcurrentExecution()
	{
		_isConcurrent = true;
		GetOrCreateAnalyzerActions().Value.EnableConcurrentExecution();
	}

	public void ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags mode)
	{
		_generatedCodeConfiguration = mode;
	}
}
