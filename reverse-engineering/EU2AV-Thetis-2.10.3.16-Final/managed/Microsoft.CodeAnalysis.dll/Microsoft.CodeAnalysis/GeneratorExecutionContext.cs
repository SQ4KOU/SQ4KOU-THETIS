using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorExecutionContext
{
	private readonly DiagnosticBag _diagnostics;

	private readonly AdditionalSourcesCollection _additionalSources;

	private readonly SourceHashAlgorithm _checksumAlgorithm;

	public Compilation Compilation { get; }

	public ParseOptions ParseOptions { get; }

	public ImmutableArray<AdditionalText> AdditionalFiles { get; }

	public AnalyzerConfigOptionsProvider AnalyzerConfigOptions { get; }

	public ISyntaxReceiver? SyntaxReceiver { get; }

	public ISyntaxContextReceiver? SyntaxContextReceiver { get; }

	public CancellationToken CancellationToken { get; }

	internal GeneratorExecutionContext(Compilation compilation, ParseOptions parseOptions, ImmutableArray<AdditionalText> additionalTexts, AnalyzerConfigOptionsProvider optionsProvider, ISyntaxContextReceiver? syntaxReceiver, string sourceExtension, SourceHashAlgorithm checksumAlgorithm, CancellationToken cancellationToken = default(CancellationToken))
	{
		Compilation = compilation;
		ParseOptions = parseOptions;
		AdditionalFiles = additionalTexts;
		AnalyzerConfigOptions = optionsProvider;
		SyntaxReceiver = (syntaxReceiver as SyntaxContextReceiverAdaptor)?.Receiver;
		SyntaxContextReceiver = ((syntaxReceiver is SyntaxContextReceiverAdaptor) ? null : syntaxReceiver);
		CancellationToken = cancellationToken;
		_additionalSources = new AdditionalSourcesCollection(sourceExtension);
		_checksumAlgorithm = checksumAlgorithm;
		_diagnostics = new DiagnosticBag();
	}

	public void AddSource(string hintName, string source)
	{
		AddSource(hintName, SourceText.From(source, Encoding.UTF8, (_checksumAlgorithm == SourceHashAlgorithm.None) ? SourceHashAlgorithm.Sha256 : _checksumAlgorithm));
	}

	public void AddSource(string hintName, SourceText sourceText)
	{
		_additionalSources.Add(hintName, sourceText.WithChecksumAlgorithm(_checksumAlgorithm));
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, Compilation, (Diagnostic _, CancellationToken _) => true, CancellationToken);
		_diagnostics.Add(diagnostic);
	}

	internal (ImmutableArray<GeneratedSourceText> sources, ImmutableArray<Diagnostic> diagnostics) ToImmutableAndFree()
	{
		return (sources: _additionalSources.ToImmutableAndFree(), diagnostics: _diagnostics.ToReadOnlyAndFree());
	}

	internal void Free()
	{
		_additionalSources.Free();
		_diagnostics.Free();
	}

	internal void CopyToProductionContext(SourceProductionContext ctx)
	{
		_additionalSources.CopyTo(ctx.Sources);
		ctx.Diagnostics.AddRange(_diagnostics);
	}
}
