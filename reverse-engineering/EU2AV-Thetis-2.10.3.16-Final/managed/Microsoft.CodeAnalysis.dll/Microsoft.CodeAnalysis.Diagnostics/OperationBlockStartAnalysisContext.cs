using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class OperationBlockStartAnalysisContext
{
	private readonly ImmutableArray<IOperation> _operationBlocks;

	private readonly ISymbol _owningSymbol;

	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly Func<IOperation, ControlFlowGraph>? _getControlFlowGraph;

	private readonly CancellationToken _cancellationToken;

	public ImmutableArray<IOperation> OperationBlocks => _operationBlocks;

	public ISymbol OwningSymbol => _owningSymbol;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public SyntaxTree FilterTree { get; }

	public TextSpan? FilterSpan { get; }

	public bool IsGeneratedCode { get; }

	public CancellationToken CancellationToken => _cancellationToken;

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	protected OperationBlockStartAnalysisContext(ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken)
		: this(operationBlocks, owningSymbol, compilation, options, null, operationBlocks[0].Syntax.SyntaxTree, null, isGeneratedCode: false, cancellationToken)
	{
	}

	internal OperationBlockStartAnalysisContext(ImmutableArray<IOperation> operationBlocks, ISymbol owningSymbol, Compilation compilation, AnalyzerOptions options, Func<IOperation, ControlFlowGraph>? getControlFlowGraph, SyntaxTree filterTree, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		_operationBlocks = operationBlocks;
		_owningSymbol = owningSymbol;
		_compilation = compilation;
		_options = options;
		_getControlFlowGraph = getControlFlowGraph;
		FilterTree = filterTree;
		FilterSpan = filterSpan;
		IsGeneratedCode = isGeneratedCode;
		_cancellationToken = cancellationToken;
	}

	public abstract void RegisterOperationBlockEndAction(Action<OperationBlockAnalysisContext> action);

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
	{
		RegisterOperationAction(action, operationKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds);

	public ControlFlowGraph GetControlFlowGraph(IOperation operationBlock)
	{
		if (operationBlock == null)
		{
			throw new ArgumentNullException("operationBlock");
		}
		if (!OperationBlocks.Contains(operationBlock))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidOperationBlockForAnalysisContext, "operationBlock");
		}
		return DiagnosticAnalysisContextHelpers.GetControlFlowGraph(operationBlock, _getControlFlowGraph, _cancellationToken);
	}
}
