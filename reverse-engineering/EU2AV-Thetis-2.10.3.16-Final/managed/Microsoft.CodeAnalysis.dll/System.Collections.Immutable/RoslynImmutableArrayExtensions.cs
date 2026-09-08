using System.Linq;

namespace System.Collections.Immutable;

internal static class RoslynImmutableArrayExtensions
{
	public static bool Contains<T>(this ImmutableArray<T> array, Func<T, bool> predicate)
	{
		return array.Any(predicate);
	}

	public static int BinarySearch<TElement, TValue>(this ImmutableArray<TElement> array, TValue value, Func<TElement, TValue, int> comparer)
	{
		return array.AsSpan().BinarySearch(value, comparer);
	}
}
