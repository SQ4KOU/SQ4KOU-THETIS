using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class GeneratorRunStateTable
{
	public sealed class Builder
	{
		private readonly Dictionary<string, HashSet<IncrementalGeneratorRunStep>>? _namedSteps;

		private readonly Dictionary<string, HashSet<IncrementalGeneratorRunStep>>? _outputSteps;

		[MemberNotNullWhen(true, new string[] { "_namedSteps", "_outputSteps" })]
		public bool RecordingExecutedSteps
		{
			[MemberNotNullWhen(true, new string[] { "_namedSteps", "_outputSteps" })]
			get
			{
				return _namedSteps != null;
			}
		}

		public Builder(bool recordingExecutedSteps)
		{
			if (recordingExecutedSteps)
			{
				_namedSteps = new Dictionary<string, HashSet<IncrementalGeneratorRunStep>>();
				_outputSteps = new Dictionary<string, HashSet<IncrementalGeneratorRunStep>>();
			}
		}

		public void RecordStepsFromOutputNodeUpdate(IStateTable table)
		{
			foreach (IncrementalGeneratorRunStep step in table.Steps)
			{
				RecordStepTree(step, addToOutputSteps: true);
			}
		}

		public GeneratorRunStateTable ToImmutableAndFree()
		{
			return new GeneratorRunStateTable(StepCollectionToImmutable(_namedSteps), StepCollectionToImmutable(_outputSteps));
		}

		private static ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> StepCollectionToImmutable(Dictionary<string, HashSet<IncrementalGeneratorRunStep>>? builder)
		{
			if (builder == null)
			{
				return ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>.Empty;
			}
			ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>.Builder builder2 = ImmutableDictionary.CreateBuilder<string, ImmutableArray<IncrementalGeneratorRunStep>>();
			foreach (KeyValuePair<string, HashSet<IncrementalGeneratorRunStep>> item in builder)
			{
				builder2.Add(item.Key, item.Value.ToImmutableArrayOrEmpty());
			}
			return builder2.ToImmutable();
		}

		private void RecordStepTree(IncrementalGeneratorRunStep step, bool addToOutputSteps)
		{
			foreach (var input in step.Inputs)
			{
				IncrementalGeneratorRunStep item = input.Source;
				RecordStepTree(item, addToOutputSteps: false);
			}
			if (step.Name != null)
			{
				addToNamedStepCollection(_namedSteps, step);
				if (addToOutputSteps)
				{
					addToNamedStepCollection(_outputSteps, step);
				}
			}
			static void addToNamedStepCollection(Dictionary<string, HashSet<IncrementalGeneratorRunStep>> stepCollectionBuilder, IncrementalGeneratorRunStep incrementalGeneratorRunStep)
			{
				if (!stepCollectionBuilder.TryGetValue(incrementalGeneratorRunStep.Name, out HashSet<IncrementalGeneratorRunStep> value))
				{
					value = new HashSet<IncrementalGeneratorRunStep>();
					stepCollectionBuilder.Add(incrementalGeneratorRunStep.Name, value);
				}
				value.Add(incrementalGeneratorRunStep);
			}
		}
	}

	public ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> ExecutedSteps { get; }

	public ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> OutputSteps { get; }

	private GeneratorRunStateTable(ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> executedSteps, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> outputSteps)
	{
		ExecutedSteps = executedSteps;
		OutputSteps = outputSteps;
	}
}
