using System;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis;

public class GeneratorDriverRunResult
{
	private ImmutableArray<Diagnostic> _lazyDiagnostics;

	private ImmutableArray<SyntaxTree> _lazyGeneratedTrees;

	public ImmutableArray<GeneratorRunResult> Results { get; }

	internal TimeSpan ElapsedTime { get; }

	public ImmutableArray<Diagnostic> Diagnostics
	{
		get
		{
			if (_lazyDiagnostics.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyDiagnostics, Results.Where((GeneratorRunResult r) => !r.Diagnostics.IsDefaultOrEmpty).SelectMany((GeneratorRunResult r) => r.Diagnostics).ToImmutableArray());
			}
			return _lazyDiagnostics;
		}
	}

	public ImmutableArray<SyntaxTree> GeneratedTrees
	{
		get
		{
			if (_lazyGeneratedTrees.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyGeneratedTrees, Results.Where((GeneratorRunResult r) => !r.GeneratedSources.IsDefaultOrEmpty).SelectMany((GeneratorRunResult r) => r.GeneratedSources.Select((GeneratedSourceResult g) => g.SyntaxTree)).ToImmutableArray());
			}
			return _lazyGeneratedTrees;
		}
	}

	internal GeneratorDriverRunResult(ImmutableArray<GeneratorRunResult> results, TimeSpan elapsedTime)
	{
		Results = results;
		ElapsedTime = elapsedTime;
	}
}
