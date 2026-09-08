using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Scripting;

internal static class IListExtensions
{
	public static void AddRange<T>(this IList<T> list, ImmutableArray<T> items)
	{
		foreach (T item in items)
		{
			list.Add(item);
		}
	}

	public static void AddRange<T>(this IList<T> list, T[] items)
	{
		foreach (T item in items)
		{
			list.Add(item);
		}
	}
}
