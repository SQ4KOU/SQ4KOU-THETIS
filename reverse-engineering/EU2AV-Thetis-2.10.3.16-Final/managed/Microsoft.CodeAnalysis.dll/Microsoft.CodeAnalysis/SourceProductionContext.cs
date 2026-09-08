using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

public readonly struct SourceProductionContext
{
	internal readonly AdditionalSourcesCollection Sources;

	internal readonly DiagnosticBag Diagnostics;

	internal readonly Compilation Compilation;

	internal readonly SourceHashAlgorithm ChecksumAlgorithm;

	public CancellationToken CancellationToken { get; }

	internal SourceProductionContext(AdditionalSourcesCollection sources, DiagnosticBag diagnostics, Compilation compilation, SourceHashAlgorithm checksumAlgorithm, CancellationToken cancellationToken)
	{
		CancellationToken = cancellationToken;
		Sources = sources;
		Diagnostics = diagnostics;
		Compilation = compilation;
		ChecksumAlgorithm = checksumAlgorithm;
	}

	public void AddSource(string hintName, string source)
	{
		AddSource(hintName, SourceText.From(source, Encoding.UTF8, (ChecksumAlgorithm == SourceHashAlgorithm.None) ? SourceHashAlgorithm.Sha256 : ChecksumAlgorithm));
	}

	public void AddSource(string hintName, SourceText sourceText)
	{
		Sources.Add(hintName, sourceText.WithChecksumAlgorithm(ChecksumAlgorithm));
	}

	public void ReportDiagnostic(Diagnostic diagnostic)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(diagnostic, Compilation, (Diagnostic _, CancellationToken _) => true, CancellationToken);
		Diagnostics.Add(diagnostic);
	}
}
