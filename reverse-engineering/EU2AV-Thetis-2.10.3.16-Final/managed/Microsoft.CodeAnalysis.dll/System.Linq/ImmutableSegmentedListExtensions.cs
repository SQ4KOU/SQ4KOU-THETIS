using System.Collections.Generic;
using Microsoft.CodeAnalysis.Collections;

namespace System.Linq;

internal static class ImmutableSegmentedListExtensions
{
	public static bool All<T>(this ImmutableSegmentedList<T> immutableList, Func<T, bool> predicate)
	{
		if (immutableList.IsDefault)
		{
			throw new ArgumentNullException("immutableList");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in immutableList)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	public static bool Any<T>(this ImmutableSegmentedList<T> immutableList)
	{
		if (immutableList.IsDefault)
		{
			throw new ArgumentNullException("immutableList");
		}
		return !immutableList.IsEmpty;
	}

	public static bool Any<T>(this ImmutableSegmentedList<T>.Builder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		return builder.Count > 0;
	}

	public static bool Any<T>(this ImmutableSegmentedList<T> immutableList, Func<T, bool> predicate)
	{
		if (immutableList.IsDefault)
		{
			throw new ArgumentNullException("immutableList");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in immutableList)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static T Last<T>(this ImmutableSegmentedList<T> immutableList)
	{
		if (immutableList.Count <= 0)
		{
			return Enumerable.Last(immutableList);
		}
		return immutableList[immutableList.Count - 1];
	}

	public static T Last<T>(this ImmutableSegmentedList<T>.Builder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		if (builder.Count <= 0)
		{
			return Enumerable.Last(builder);
		}
		return builder[builder.Count - 1];
	}

	public static T Last<T>(this ImmutableSegmentedList<T> immutableList, Func<T, bool> predicate)
	{
		if (immutableList.IsDefault)
		{
			throw new ArgumentNullException("immutableList");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		for (int num = immutableList.Count - 1; num >= 0; num--)
		{
			if (predicate(immutableList[num]))
			{
				return immutableList[num];
			}
		}
		return Enumerable.Empty<T>().Last();
	}

	public static IEnumerable<TResult> Select<T, TResult>(this ImmutableSegmentedList<T> immutableList, Func<T, TResult> selector)
	{
		if (immutableList.IsDefault)
		{
			throw new ArgumentNullException("immutableList");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (immutableList.IsEmpty)
		{
			return Enumerable.Empty<TResult>();
		}
		return Enumerable.Select(immutableList, selector);
	}

	public static int BinarySearch<TElement, TValue>(this ImmutableSegmentedList<TElement> array, TValue value, Func<TElement, TValue, int> comparer)
	{
		int num = 0;
		int num2 = array.Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			int num4 = comparer(array[num3], value);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 > 0)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return ~num;
	}
}
