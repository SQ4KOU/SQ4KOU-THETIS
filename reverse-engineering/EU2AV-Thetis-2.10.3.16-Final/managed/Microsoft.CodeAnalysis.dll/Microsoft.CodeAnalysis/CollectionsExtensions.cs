using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

internal static class CollectionsExtensions
{
	internal static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ICollection<T>? collection)
	{
		if (collection != null)
		{
			return collection.Count == 0;
		}
		return true;
	}

	internal static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IReadOnlyCollection<T>? collection)
	{
		if (collection != null)
		{
			return collection.Count == 0;
		}
		return true;
	}

	internal static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ImmutableHashSet<T>? hashSet)
	{
		if (hashSet != null)
		{
			return hashSet.Count == 0;
		}
		return true;
	}
}
