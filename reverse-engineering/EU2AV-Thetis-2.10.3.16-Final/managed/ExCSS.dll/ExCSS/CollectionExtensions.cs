using System;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal static class CollectionExtensions
{
	public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T element)
	{
		foreach (T item in items)
		{
			yield return item;
		}
		yield return element;
	}

	public static T GetItemByIndex<T>(this IEnumerable<T> items, int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int num = 0;
		foreach (T item in items)
		{
			if (num++ == index)
			{
				return item;
			}
		}
		throw new ArgumentOutOfRangeException("index");
	}

	public static IEnumerable<object[]> ToObjectArray<T>(this IEnumerable<T> items)
	{
		return items.Select((T i) => new object[1] { i });
	}
}
