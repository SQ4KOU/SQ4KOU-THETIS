using System;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public readonly struct SyntaxNodeAnalysisContext
{
	private readonly SyntaxNode _node;

	private readonly ISymbol? _containingSymbol;

	private readonly SemanticModel _semanticModel;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, CancellationToken, bool> _isSupportedDiagnostic;

	private readonly CancellationToken _cancellationToken;

	public SyntaxNode Node => _node;

	public ISymbol? ContainingSymbol => _containingSymbol;

	public SemanticModel SemanticModel => _semanticModel;

	public Compilation Compilation => _semanticModel?.Compilation ?? throw new InvalidOperationException();

	public AnalyzerOptions Options => _options;

	public SyntaxTree FilterTree { get; }

	public TextSpan? FilterSpan { get; }

	public bool IsGeneratedCode { get; }

	public CancellationToken CancellationToken => _cancellationToken;

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	public SyntaxNodeAnalysisContext(SyntaxNode node, ISymbol? containingSymbol, SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
		: this(node, containingSymbol, semanticModel, options, reportDiagnostic, (Diagnostic d, CancellationToken _) => isSupportedDiagnostic(d), null, isGeneratedCode: false, cancellationToken)
	{
	}

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	public SyntaxNodeAnalysisContext(SyntaxNode node, SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
		: this(node, null, semanticModel, options, reportDiagnostic, (Diagnostic d, CancellationToken _) => isSupportedDiagnostic(d), null, isGeneratedCode: false, cancellationToken)
	{
	}

	internal SyntaxNodeAnalysisContext(SyntaxNode node, ISymbol? containingSymbol, SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		_node = node;
		_containingSymbol = containingSymbol;
		_semanticModel = semanticModel;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		FilterTree = node.SyntaxTree;
		FilterSpan = filterSpan;
		IsGeneratedCode = isGeneratedCode;
		_cancellationToken = cancellationToken;
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, _semanticModel.Compilation, _isSupportedDiagnostic, _cancellationToken);
		lock (_reportDiagnostic)
		{
			_reportDiagnostic(diagnostic);
		}
	}
}
