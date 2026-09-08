using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerCodeBlockStartAnalysisContext<TLanguageKindEnum> : CodeBlockStartAnalysisContext<TLanguageKindEnum> where TLanguageKindEnum : struct
{
	private readonly HostCodeBlockStartAnalysisScope<TLanguageKindEnum> _scope;

	internal AnalyzerCodeBlockStartAnalysisContext(HostCodeBlockStartAnalysisScope<TLanguageKindEnum> scope, SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
		: base(codeBlock, owningSymbol, semanticModel, options, filterSpan, isGeneratedCode, cancellationToken)
	{
		_scope = scope;
	}

	public override void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCodeBlockEndAction(action);
	}

	public override void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, syntaxKinds);
		_scope.RegisterSyntaxNodeAction(action, syntaxKinds);
	}
}
