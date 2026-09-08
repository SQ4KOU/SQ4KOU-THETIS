namespace Microsoft.CodeAnalysis;

internal static class NodeExtensions
{
	public static void LogTables<TSelf, TInput>(this IIncrementalGeneratorNode<TSelf> self, string? name, string? tableType, NodeStateTable<TSelf>? previousTable, NodeStateTable<TSelf> newTable, NodeStateTable<TInput> inputTable)
	{
		self.LogTables<TSelf, TInput, TInput>(name, tableType, previousTable, newTable, inputTable, null);
	}

	public static void LogTables<TSelf, TInput1, TInput2>(this IIncrementalGeneratorNode<TSelf> self, string? name, string? tableType, NodeStateTable<TSelf>? previousTable, NodeStateTable<TSelf> newTable, NodeStateTable<TInput1> inputNode1, NodeStateTable<TInput2>? inputNode2)
	{
		if (CodeAnalysisEventSource.Log.IsEnabled())
		{
			NodeStateTable<TSelf> nodeStateTable = ((newTable != previousTable) ? newTable : null);
			CodeAnalysisEventSource.Log.NodeTransform(self.GetHashCode(), name ?? "<anonymous>", tableType ?? "<unknown>", previousTable?.GetHashCode() ?? (-1), previousTable?.GetPackedStates() ?? "", nodeStateTable?.GetHashCode() ?? (-1), nodeStateTable?.GetPackedStates() ?? "", inputNode1.GetHashCode(), inputNode2?.GetHashCode() ?? (-1));
		}
	}
}
