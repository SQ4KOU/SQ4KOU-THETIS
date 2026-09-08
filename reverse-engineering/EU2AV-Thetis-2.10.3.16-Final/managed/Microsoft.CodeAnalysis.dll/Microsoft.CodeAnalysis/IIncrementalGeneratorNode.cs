using System.Collections.Generic;
using System.Threading;

namespace Microsoft.CodeAnalysis;

internal interface IIncrementalGeneratorNode<T>
{
	NodeStateTable<T> UpdateStateTable(DriverStateTable.Builder graphState, NodeStateTable<T>? previousTable, CancellationToken cancellationToken);

	IIncrementalGeneratorNode<T> WithComparer(IEqualityComparer<T> comparer);

	IIncrementalGeneratorNode<T> WithTrackingName(string name);

	void RegisterOutput(IIncrementalGeneratorOutputNode output);
}
