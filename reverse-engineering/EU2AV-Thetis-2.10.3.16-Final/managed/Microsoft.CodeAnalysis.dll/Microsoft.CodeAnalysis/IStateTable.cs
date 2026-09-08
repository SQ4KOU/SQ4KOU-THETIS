using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal interface IStateTable
{
	bool HasTrackedSteps { get; }

	ImmutableArray<IncrementalGeneratorRunStep> Steps { get; }

	IStateTable AsCached();
}
