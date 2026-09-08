using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading;

namespace Microsoft.CodeAnalysis.Diagnostics;

public static class DiagnosticAnalyzerExtensions
{
	[Obsolete("Use WithAnalyzers overload without a cancellation token", false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static CompilationWithAnalyzers WithAnalyzers(this Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions? options, CancellationToken cancellationToken)
	{
		return new CompilationWithAnalyzers(compilation, analyzers, options, cancellationToken);
	}

	public static CompilationWithAnalyzers WithAnalyzers(this Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions? options = null)
	{
		return new CompilationWithAnalyzers(compilation, analyzers, options);
	}

	public static CompilationWithAnalyzers WithAnalyzers(this Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions)
	{
		return new CompilationWithAnalyzers(compilation, analyzers, analysisOptions);
	}
}
