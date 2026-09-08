using System;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public readonly struct SymbolAnalysisContext
{
	private readonly ISymbol _symbol;

	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, CancellationToken, bool> _isSupportedDiagnostic;

	private readonly CancellationToken _cancellationToken;

	public ISymbol Symbol => _symbol;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public SyntaxTree? FilterTree { get; }

	public TextSpan? FilterSpan { get; }

	public CancellationToken CancellationToken => _cancellationToken;

	internal Func<Diagnostic, CancellationToken, bool> IsSupportedDiagnostic => _isSupportedDiagnostic;

	public bool IsGeneratedCode { get; }

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	public SymbolAnalysisContext(ISymbol symbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
		: this(symbol, compilation, options, reportDiagnostic, (Diagnostic d, CancellationToken _) => isSupportedDiagnostic(d), isGeneratedCode: false, null, null, cancellationToken)
	{
	}

	internal SymbolAnalysisContext(ISymbol symbol, Compilation compilation, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, bool isGeneratedCode, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		_symbol = symbol;
		_compilation = compilation;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		IsGeneratedCode = isGeneratedCode;
		FilterTree = filterTree;
		FilterSpan = filterSpan;
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
}
