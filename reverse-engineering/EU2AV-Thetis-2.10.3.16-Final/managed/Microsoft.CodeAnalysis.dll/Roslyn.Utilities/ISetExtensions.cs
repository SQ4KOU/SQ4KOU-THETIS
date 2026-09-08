using System.Collections.Generic;
using System.Collections.Immutable;

namespace Roslyn.Utilities;

internal static class ISetExtensions
{
	public static bool AddAll<T>(this ISet<T> set, IEnumerable<T> values)
	{
		bool flag = false;
		foreach (T value in values)
		{
			flag |= set.Add(value);
		}
		return flag;
	}

	public static bool AddAll<T>(this ISet<T> set, ImmutableArray<T> values)
	{
		bool flag = false;
		foreach (T item in values)
		{
			flag |= set.Add(item);
		}
		return flag;
	}

	public static bool RemoveAll<T>(this ISet<T> set, IEnumerable<T> values)
	{
		bool flag = false;
		foreach (T value in values)
		{
			flag |= set.Remove(value);
		}
		return flag;
	}

	public static bool RemoveAll<T>(this ISet<T> set, ImmutableArray<T> values)
	{
		bool flag = false;
		foreach (T item in values)
		{
			flag |= set.Remove(item);
		}
		return flag;
	}
}
