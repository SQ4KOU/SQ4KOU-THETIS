using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal readonly struct IncrementalExecutionContext(DriverStateTable.Builder? tableBuilder, GeneratorRunStateTable.Builder generatorRunStateBuilder, AdditionalSourcesCollection sources)
{
	internal readonly DiagnosticBag Diagnostics = DiagnosticBag.GetInstance();

	internal readonly AdditionalSourcesCollection Sources = sources;

	internal readonly DriverStateTable.Builder? TableBuilder = tableBuilder;

	internal readonly GeneratorRunStateTable.Builder GeneratorRunStateBuilder = generatorRunStateBuilder;

	internal readonly ImmutableDictionary<string, object>.Builder HostOutputBuilder = ImmutableDictionary.CreateBuilder<string, object>();

	internal (ImmutableArray<GeneratedSourceText> sources, ImmutableArray<Diagnostic> diagnostics, GeneratorRunStateTable executedSteps, ImmutableDictionary<string, object> hostOutputs) ToImmutableAndFree()
	{
		return (sources: Sources.ToImmutableAndFree(), diagnostics: Diagnostics.ToReadOnlyAndFree(), executedSteps: GeneratorRunStateBuilder.ToImmutableAndFree(), hostOutputs: HostOutputBuilder.ToImmutable());
	}

	internal void Free()
	{
		Sources.Free();
		Diagnostics.Free();
	}
}
