using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public sealed class IncrementalGeneratorRunStep
{
	public string? Name { get; }

	public ImmutableArray<(IncrementalGeneratorRunStep Source, int OutputIndex)> Inputs { get; }

	public ImmutableArray<(object Value, IncrementalStepRunReason Reason)> Outputs { get; }

	public TimeSpan ElapsedTime { get; }

	internal IncrementalGeneratorRunStep(string? stepName, ImmutableArray<(IncrementalGeneratorRunStep Source, int OutputIndex)> inputs, ImmutableArray<(object Value, IncrementalStepRunReason OutputState)> outputs, TimeSpan elapsedTime)
	{
		Name = stepName;
		Inputs = inputs;
		Outputs = outputs;
		ElapsedTime = elapsedTime;
	}
}
