using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal static class ImmutableHashSetExtensions
{
	public static bool SetEqualsWithoutIntermediateHashSet<T>(this ImmutableHashSet<T> set, ImmutableHashSet<T> other)
	{
		if (set == null)
		{
			throw new ArgumentNullException("set");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (set == other)
		{
			return true;
		}
		ImmutableHashSet<T> immutableHashSet = other.WithComparer(set.KeyComparer);
		if (set.Count != immutableHashSet.Count)
		{
			return false;
		}
		foreach (T item in other)
		{
			if (!set.Contains(item))
			{
				return false;
			}
		}
		return true;
	}
}
