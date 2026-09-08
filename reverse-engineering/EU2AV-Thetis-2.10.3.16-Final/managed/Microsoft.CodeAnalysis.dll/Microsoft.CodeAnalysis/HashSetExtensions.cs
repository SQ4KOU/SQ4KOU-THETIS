using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

internal static class HashSetExtensions
{
	internal static bool IsNullOrEmpty<T>([NotNullWhen(false)] this HashSet<T>? hashSet)
	{
		if (hashSet != null)
		{
			return hashSet.Count == 0;
		}
		return true;
	}

	internal static bool InitializeAndAdd<T>([NotNullIfNotNull("item")][NotNullWhen(true)] ref HashSet<T>? hashSet, [NotNullWhen(true)] T? item) where T : class
	{
		if (item == null)
		{
			return false;
		}
		if (hashSet == null)
		{
			hashSet = new HashSet<T>();
		}
		return hashSet.Add(item);
	}

	internal static bool Any<T>(this HashSet<T> hashSet, Func<T, bool> predicate)
	{
		foreach (T item in hashSet)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}
}
