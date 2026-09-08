using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class IListExtensions
{
	public static bool HasDuplicates<T>(this IReadOnlyList<T> list)
	{
		return list.HasDuplicates(EqualityComparer<T>.Default);
	}

	public static bool HasDuplicates<T>(this IReadOnlyList<T> list, IEqualityComparer<T> comparer)
	{
		return list.HasDuplicates((T x) => x, comparer);
	}

	public static bool HasDuplicates<TItem, TValue>(this IReadOnlyList<TItem> list, Func<TItem, TValue> selector)
	{
		return list.HasDuplicates(selector, EqualityComparer<TValue>.Default);
	}

	internal static bool HasDuplicates<TItem, TValue>(this IReadOnlyList<TItem> list, Func<TItem, TValue> selector, IEqualityComparer<TValue> comparer)
	{
		switch (list.Count)
		{
		case 0:
		case 1:
			return false;
		case 2:
			return comparer.Equals(selector(list[0]), selector(list[1]));
		default:
		{
			HashSet<TValue> hashSet = ((comparer == EqualityComparer<TValue>.Default) ? PooledHashSet<TValue>.GetInstance() : new HashSet<TValue>(comparer));
			bool result = false;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (!hashSet.Add(selector(list[i])))
				{
					result = true;
					break;
				}
			}
			(hashSet as PooledHashSet<TValue>)?.Free();
			return result;
		}
		}
	}
}
