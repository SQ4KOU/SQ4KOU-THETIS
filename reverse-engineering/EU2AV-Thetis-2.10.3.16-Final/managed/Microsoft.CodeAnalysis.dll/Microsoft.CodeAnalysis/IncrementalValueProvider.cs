namespace Microsoft.CodeAnalysis;

public readonly struct IncrementalValueProvider<TValue>
{
	internal readonly IIncrementalGeneratorNode<TValue> Node;

	internal readonly bool CatchAnalyzerExceptions;

	internal IncrementalValueProvider(IIncrementalGeneratorNode<TValue> node, bool catchAnalyzerExceptions)
	{
		Node = node;
		CatchAnalyzerExceptions = catchAnalyzerExceptions;
	}
}
