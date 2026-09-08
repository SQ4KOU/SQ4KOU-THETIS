using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Collections.Internal;

internal static class ICollectionCalls
{
	public static bool IsSynchronized<TCollection>(ref TCollection collection) where TCollection : ICollection
	{
		return collection.IsSynchronized;
	}

	public static void CopyTo<TCollection>(ref TCollection collection, Array array, int index) where TCollection : ICollection
	{
		collection.CopyTo(array, index);
	}
}
internal static class ICollectionCalls<T>
{
	public static bool IsReadOnly<TCollection>(ref TCollection collection) where TCollection : ICollection<T>
	{
		return collection.IsReadOnly;
	}

	public static void Add<TCollection>(ref TCollection collection, T item) where TCollection : ICollection<T>
	{
		collection.Add(item);
	}

	public static void CopyTo<TCollection>(ref TCollection collection, T[] array, int arrayIndex) where TCollection : ICollection<T>
	{
		collection.CopyTo(array, arrayIndex);
	}
}
