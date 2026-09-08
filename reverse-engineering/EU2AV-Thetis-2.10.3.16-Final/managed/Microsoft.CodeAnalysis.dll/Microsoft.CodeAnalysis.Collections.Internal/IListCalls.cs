using System.Collections;

namespace Microsoft.CodeAnalysis.Collections.Internal;

internal static class IListCalls
{
	public static object? GetItem<TList>(ref TList list, int index) where TList : IList
	{
		return list[index];
	}

	public static void SetItem<TList>(ref TList list, int index, object? value) where TList : IList
	{
		list[index] = value;
	}

	public static bool IsFixedSize<TList>(ref TList list) where TList : IList
	{
		return list.IsFixedSize;
	}

	public static bool IsReadOnly<TList>(ref TList list) where TList : IList
	{
		return list.IsReadOnly;
	}

	public static int Add<TList>(ref TList list, object? value) where TList : IList
	{
		return list.Add(value);
	}

	public static bool Contains<TList>(ref TList list, object? value) where TList : IList
	{
		return list.Contains(value);
	}

	public static int IndexOf<TList>(ref TList list, object? value) where TList : IList
	{
		return list.IndexOf(value);
	}

	public static void Insert<TList>(ref TList list, int index, object? value) where TList : IList
	{
		list.Insert(index, value);
	}

	public static void Remove<TList>(ref TList list, object? value) where TList : IList
	{
		list.Remove(value);
	}
}
