using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerSymbolStartAnalysisContext : SymbolStartAnalysisContext
{
	private readonly HostSymbolStartAnalysisScope _scope;

	internal AnalyzerSymbolStartAnalysisContext(HostSymbolStartAnalysisScope scope, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, bool isGeneratedCode, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken)
		: base(owningSymbol, compilation, options, isGeneratedCode, filterTree, filterSpan, cancellationToken)
	{
		_scope = scope;
	}

	public override void RegisterSymbolEndAction(Action<SymbolAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterSymbolEndAction(action);
	}

	public override void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCodeBlockStartAction(action);
	}

	public override void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCodeBlockAction(action);
	}

	public override void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, syntaxKinds);
		_scope.RegisterSyntaxNodeAction(action, syntaxKinds);
	}

	public override void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterOperationBlockStartAction(action);
	}

	public override void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterOperationBlockAction(action);
	}

	public override void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, operationKinds);
		_scope.RegisterOperationAction(action, operationKinds);
	}

	public AnalyzerSymbolStartAnalysisContext WithOptions(AnalyzerOptions analyzerOptions)
	{
		if (base.Options != analyzerOptions)
		{
			return new AnalyzerSymbolStartAnalysisContext(_scope, base.Symbol, base.Compilation, analyzerOptions, base.IsGeneratedCode, base.FilterTree, base.FilterSpan, base.CancellationToken);
		}
		return this;
	}
}
