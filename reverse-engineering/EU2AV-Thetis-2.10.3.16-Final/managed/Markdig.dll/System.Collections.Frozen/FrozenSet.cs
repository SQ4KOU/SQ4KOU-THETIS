using System.Collections.Generic;

namespace System.Collections.Frozen;

internal sealed class FrozenSet<T> : HashSet<T>
{
	public FrozenSet(HashSet<T> set, IEqualityComparer<T> comparer)
		: base((IEnumerable<T>)set, comparer)
	{
	}
}
