using System;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public readonly struct SemanticModelAnalysisContext
{
	private readonly SemanticModel _semanticModel;

	private readonly AnalyzerOptions _options;

	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, CancellationToken, bool> _isSupportedDiagnostic;

	private readonly CancellationToken _cancellationToken;

	public SemanticModel SemanticModel => _semanticModel;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	public SyntaxTree FilterTree { get; }

	public TextSpan? FilterSpan { get; }

	public bool IsGeneratedCode { get; }

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	public SemanticModelAnalysisContext(SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
		: this(semanticModel, options, reportDiagnostic, (Diagnostic d, CancellationToken _) => isSupportedDiagnostic(d), null, isGeneratedCode: false, cancellationToken)
	{
	}

	internal SemanticModelAnalysisContext(SemanticModel semanticModel, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		_semanticModel = semanticModel;
		_options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		FilterTree = semanticModel.SyntaxTree;
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
