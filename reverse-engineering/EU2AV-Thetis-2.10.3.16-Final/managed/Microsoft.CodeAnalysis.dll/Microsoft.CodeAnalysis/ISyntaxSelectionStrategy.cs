using System.Collections.Generic;

namespace Microsoft.CodeAnalysis;

internal interface ISyntaxSelectionStrategy<T>
{
	ISyntaxInputBuilder GetBuilder(StateTableStore tableStore, object key, bool trackIncrementalSteps, string? name, IEqualityComparer<T> comparer);
}
