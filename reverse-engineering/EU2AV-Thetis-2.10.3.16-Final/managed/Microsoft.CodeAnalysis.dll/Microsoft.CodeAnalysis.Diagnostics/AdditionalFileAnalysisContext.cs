using System;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public readonly struct AdditionalFileAnalysisContext
{
	private readonly Action<Diagnostic> _reportDiagnostic;

	private readonly Func<Diagnostic, CancellationToken, bool> _isSupportedDiagnostic;

	public AdditionalText AdditionalFile { get; }

	public AnalyzerOptions Options { get; }

	public TextSpan? FilterSpan { get; }

	public CancellationToken CancellationToken { get; }

	public Compilation Compilation { get; }

	internal AdditionalFileAnalysisContext(AdditionalText additionalFile, AnalyzerOptions options, Action<Diagnostic> reportDiagnostic, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, Compilation compilation, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		AdditionalFile = additionalFile;
		Options = options;
		_reportDiagnostic = reportDiagnostic;
		_isSupportedDiagnostic = isSupportedDiagnostic;
		Compilation = compilation;
		FilterSpan = filterSpan;
		CancellationToken = cancellationToken;
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, Compilation, _isSupportedDiagnostic, CancellationToken);
		lock (_reportDiagnostic)
		{
			_reportDiagnostic(diagnostic);
		}
	}
}
