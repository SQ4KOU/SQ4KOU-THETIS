using System;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis;

internal readonly struct GeneratorState
{
	public static readonly GeneratorState Empty = new GeneratorState(ImmutableArray<GeneratedSyntaxTree>.Empty, ImmutableArray<SyntaxInputNode>.Empty, ImmutableArray<IIncrementalGeneratorOutputNode>.Empty, ImmutableArray<GeneratedSyntaxTree>.Empty, ImmutableArray<Diagnostic>.Empty, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>.Empty, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>.Empty, ImmutableDictionary<string, object>.Empty, null, TimeSpan.Zero);

	internal bool Initialized { get; }

	internal ImmutableArray<GeneratedSyntaxTree> PostInitTrees { get; }

	internal ImmutableArray<SyntaxInputNode> InputNodes { get; }

	internal ImmutableArray<IIncrementalGeneratorOutputNode> OutputNodes { get; }

	internal ImmutableArray<GeneratedSyntaxTree> GeneratedTrees { get; }

	internal Exception? Exception { get; }

	internal TimeSpan ElapsedTime { get; }

	internal ImmutableArray<Diagnostic> Diagnostics { get; }

	internal ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> ExecutedSteps { get; }

	internal ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> OutputSteps { get; }

	internal ImmutableDictionary<string, object> HostOutputs { get; }

	public GeneratorState(ImmutableArray<GeneratedSyntaxTree> postInitTrees, ImmutableArray<SyntaxInputNode> inputNodes, ImmutableArray<IIncrementalGeneratorOutputNode> outputNodes)
		: this(postInitTrees, inputNodes, outputNodes, ImmutableArray<GeneratedSyntaxTree>.Empty, ImmutableArray<Diagnostic>.Empty, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>.Empty, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>.Empty, ImmutableDictionary<string, object>.Empty, null, TimeSpan.Zero)
	{
	}

	private GeneratorState(ImmutableArray<GeneratedSyntaxTree> postInitTrees, ImmutableArray<SyntaxInputNode> inputNodes, ImmutableArray<IIncrementalGeneratorOutputNode> outputNodes, ImmutableArray<GeneratedSyntaxTree> generatedTrees, ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> executedSteps, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> outputSteps, ImmutableDictionary<string, object> hostOutputs, Exception? exception, TimeSpan elapsedTime)
	{
		Initialized = true;
		PostInitTrees = postInitTrees;
		InputNodes = inputNodes;
		OutputNodes = outputNodes;
		GeneratedTrees = generatedTrees;
		Diagnostics = diagnostics;
		ExecutedSteps = executedSteps;
		OutputSteps = outputSteps;
		HostOutputs = hostOutputs;
		Exception = exception;
		ElapsedTime = elapsedTime;
	}

	public GeneratorState WithResults(ImmutableArray<GeneratedSyntaxTree> generatedTrees, ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> executedSteps, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> outputSteps, ImmutableDictionary<string, object> hostOutputs, TimeSpan elapsedTime)
	{
		return new GeneratorState(PostInitTrees, InputNodes, OutputNodes, generatedTrees, diagnostics, executedSteps, outputSteps, hostOutputs, null, elapsedTime);
	}

	public GeneratorState WithError(Exception exception, Diagnostic error, TimeSpan elapsedTime)
	{
		return new GeneratorState(PostInitTrees, InputNodes, OutputNodes, ImmutableArray<GeneratedSyntaxTree>.Empty, ImmutableArray.Create(error), ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>.Empty, ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>.Empty, ImmutableDictionary<string, object>.Empty, exception, elapsedTime);
	}

	internal bool RequiresPostInitReparse(ParseOptions parseOptions)
	{
		return PostInitTrees.Any((GeneratedSyntaxTree t, ParseOptions parseOptions2) => t.Tree.Options != parseOptions2, parseOptions);
	}
}
