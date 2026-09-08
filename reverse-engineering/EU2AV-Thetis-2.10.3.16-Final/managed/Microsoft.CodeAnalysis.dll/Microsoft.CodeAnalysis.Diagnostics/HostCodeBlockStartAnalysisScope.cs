using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class HostCodeBlockStartAnalysisScope<TLanguageKindEnum>(DiagnosticAnalyzer analyzer) where TLanguageKindEnum : struct
{
	private ImmutableArray<CodeBlockAnalyzerAction> _codeBlockEndActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> _syntaxNodeActions = ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>.Empty;

	private DiagnosticAnalyzer Analyzer { get; } = analyzer;

	public ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => _codeBlockEndActions;

	public ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> SyntaxNodeActions => _syntaxNodeActions;

	public void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action)
	{
		_codeBlockEndActions = _codeBlockEndActions.Add(new CodeBlockAnalyzerAction(action, Analyzer));
	}

	public void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
	{
		_syntaxNodeActions = _syntaxNodeActions.Add(new SyntaxNodeAnalyzerAction<TLanguageKindEnum>(action, syntaxKinds, Analyzer));
	}
}
