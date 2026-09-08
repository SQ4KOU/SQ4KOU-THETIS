using System.Collections.Generic;

namespace System.Collections.Frozen;

internal static class FrozenSetExtensions
{
	public static FrozenSet<T> ToFrozenSet<T>(this HashSet<T> set, IEqualityComparer<T> comparer)
	{
		return new FrozenSet<T>(set, comparer);
	}
}
