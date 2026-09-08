using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

public readonly struct GeneratorRunResult
{
	public ISourceGenerator Generator { get; }

	public ImmutableArray<GeneratedSourceResult> GeneratedSources { get; }

	public ImmutableArray<Diagnostic> Diagnostics { get; }

	[Experimental("RSEXPERIMENTAL004", UrlFormat = "https://github.com/dotnet/roslyn/issues/74753")]
	public ImmutableDictionary<string, object> HostOutputs { get; }

	public Exception? Exception { get; }

	internal TimeSpan ElapsedTime { get; }

	public ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> TrackedSteps { get; }

	public ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> TrackedOutputSteps { get; }

	internal GeneratorRunResult(ISourceGenerator generator, ImmutableArray<GeneratedSourceResult> generatedSources, ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> namedSteps, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> outputSteps, ImmutableDictionary<string, object> hostOutputs, Exception? exception, TimeSpan elapsedTime)
	{
		Generator = generator;
		GeneratedSources = generatedSources;
		Diagnostics = diagnostics;
		TrackedSteps = namedSteps;
		TrackedOutputSteps = outputSteps;
		HostOutputs = hostOutputs;
		Exception = exception;
		ElapsedTime = elapsedTime;
	}
}
