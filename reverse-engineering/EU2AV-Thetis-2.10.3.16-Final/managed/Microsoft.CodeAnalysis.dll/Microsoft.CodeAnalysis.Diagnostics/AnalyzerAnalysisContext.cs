using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerAnalysisContext : AnalysisContext
{
	private readonly HostSessionStartAnalysisScope _scope;

	public override DiagnosticSeverity MinimumReportedSeverity { get; }

	public AnalyzerAnalysisContext(HostSessionStartAnalysisScope scope, SeverityFilter severityFilter)
	{
		_scope = scope;
		MinimumReportedSeverity = severityFilter.GetMinimumUnfilteredSeverity();
	}

	public override void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCompilationStartAction(action);
	}

	public override void RegisterCompilationAction(Action<CompilationAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCompilationAction(action);
	}

	public override void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterSyntaxTreeAction(action);
	}

	public override void RegisterAdditionalFileAction(Action<AdditionalFileAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterAdditionalFileAction(action);
	}

	public override void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterSemanticModelAction(action);
	}

	public override void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, symbolKinds);
		_scope.RegisterSymbolAction(action, symbolKinds);
	}

	public override void RegisterSymbolStartAction(Action<SymbolStartAnalysisContext> action, SymbolKind symbolKind)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterSymbolStartAction(action, symbolKind);
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

	public override void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, operationKinds);
		_scope.RegisterOperationAction(action, operationKinds);
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

	public override void EnableConcurrentExecution()
	{
		_scope.EnableConcurrentExecution();
	}

	public override void ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags mode)
	{
		_scope.ConfigureGeneratedCodeAnalysis(mode);
	}
}
