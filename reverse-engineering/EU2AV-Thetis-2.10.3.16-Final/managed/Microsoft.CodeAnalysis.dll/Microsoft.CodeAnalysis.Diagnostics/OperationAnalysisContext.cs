using System;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public readonly struct OperationAnalysisContext
{
	private readonly IOperation _operation;

	private readonly ISymbol _containingSymbol;

	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, CancellationToken, bool> _isSupportedDiagnostic;

	private readonly Func<IOperation, ControlFlowGraph>? _getControlFlowGraph;

	private readonly CancellationToken _cancellationToken;

	public IOperation Operation => _operation;

	public ISymbol ContainingSymbol => _containingSymbol;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public SyntaxTree FilterTree { get; }

	public TextSpan? FilterSpan { get; }

	public bool IsGeneratedCode { get; }

	public CancellationToken CancellationToken => _cancellationToken;

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	public OperationAnalysisContext(IOperation operation, ISymbol containingSymbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
		: this(operation, containingSymbol, compilation, options, reportDiagnostic, (Diagnostic d, CancellationToken _) => isSupportedDiagnostic(d), null, null, isGeneratedCode: false, cancellationToken)
	{
	}

	internal OperationAnalysisContext(IOperation operation, ISymbol containingSymbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, Func<IOperation, ControlFlowGraph>? getControlFlowGraph, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		_operation = operation;
		_containingSymbol = containingSymbol;
		_compilation = compilation;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		_getControlFlowGraph = getControlFlowGraph;
		FilterTree = operation.Syntax.SyntaxTree;
		FilterSpan = filterSpan;
		IsGeneratedCode = isGeneratedCode;
		_cancellationToken = cancellationToken;
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, _compilation, _isSupportedDiagnostic, _cancellationToken);
		lock (_reportDiagnostic)
		{
			_reportDiagnostic(diagnostic);
		}
	}

	public ControlFlowGraph GetControlFlowGraph()
	{
		return DiagnosticAnalysisContextHelpers.GetControlFlowGraph(Operation, _getControlFlowGraph, _cancellationToken);
	}
}
