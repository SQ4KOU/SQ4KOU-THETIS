using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerOperationBlockStartAnalysisContext : OperationBlockStartAnalysisContext
{
	private readonly HostOperationBlockStartAnalysisScope _scope;

	internal AnalyzerOperationBlockStartAnalysisContext(HostOperationBlockStartAnalysisScope scope, ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, Func<IOperation, ControlFlowGraph> getControlFlowGraph, SyntaxTree filterTree, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
		: base(operationBlocks, owningSymbol, compilation, options, getControlFlowGraph, filterTree, filterSpan, isGeneratedCode, cancellationToken)
	{
		_scope = scope;
	}

	public override void RegisterOperationBlockEndAction(Action<OperationBlockAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterOperationBlockEndAction(action);
	}

	public override void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, operationKinds);
		_scope.RegisterOperationAction(action, operationKinds);
	}
}
