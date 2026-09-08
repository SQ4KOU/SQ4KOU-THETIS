using System;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public readonly struct CodeBlockAnalysisContext
{
	private readonly SyntaxNode _codeBlock;

	private readonly ISymbol _owningSymbol;

	private readonly SemanticModel _semanticModel;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, CancellationToken, bool> _isSupportedDiagnostic;

	private readonly CancellationToken _cancellationToken;

	public SyntaxNode CodeBlock => _codeBlock;

	public ISymbol OwningSymbol => _owningSymbol;

	public SemanticModel SemanticModel => _semanticModel;

	public AnalyzerOptions Options => _options;

	public SyntaxTree FilterTree { get; }

	public TextSpan? FilterSpan { get; }

	public bool IsGeneratedCode { get; }

	public CancellationToken CancellationToken => _cancellationToken;

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	public CodeBlockAnalysisContext(SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
		: this(codeBlock, owningSymbol, semanticModel, options, reportDiagnostic, (Diagnostic d, CancellationToken _) => isSupportedDiagnostic(d), null, isGeneratedCode: false, cancellationToken)
	{
	}

	internal CodeBlockAnalysisContext(SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		_codeBlock = codeBlock;
		_owningSymbol = owningSymbol;
		_semanticModel = semanticModel;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		FilterTree = codeBlock.SyntaxTree;
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
