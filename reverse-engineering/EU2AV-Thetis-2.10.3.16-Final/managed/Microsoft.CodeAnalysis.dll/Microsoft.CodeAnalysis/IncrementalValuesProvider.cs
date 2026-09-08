namespace Microsoft.CodeAnalysis;

public readonly struct IncrementalValuesProvider<TValues>
{
	internal readonly IIncrementalGeneratorNode<TValues> Node;

	internal readonly bool CatchAnalyzerExceptions;

	internal IncrementalValuesProvider(IIncrementalGeneratorNode<TValues> node, bool catchAnalyzerExceptions)
	{
		Node = node;
		CatchAnalyzerExceptions = catchAnalyzerExceptions;
	}
}
