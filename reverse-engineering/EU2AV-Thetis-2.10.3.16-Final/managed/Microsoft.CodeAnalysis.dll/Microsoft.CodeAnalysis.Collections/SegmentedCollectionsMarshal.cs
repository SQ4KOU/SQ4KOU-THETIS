namespace Microsoft.CodeAnalysis.Collections;

internal static class SegmentedCollectionsMarshal
{
	public static T[][] AsSegments<T>(SegmentedArray<T> array)
	{
		return SegmentedArray<T>.PrivateMarshal.AsSegments(array);
	}

	public static SegmentedArray<T> AsSegmentedArray<T>(int length, T[][] segments)
	{
		return SegmentedArray<T>.PrivateMarshal.AsSegmentedArray(length, segments);
	}

	public static ref TValue GetValueRefOrNullRef<TKey, TValue>(SegmentedDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
	{
		return ref SegmentedDictionary<TKey, TValue>.PrivateMarshal.FindValue(dictionary, key);
	}

	public static ref readonly TValue GetValueRefOrNullRef<TKey, TValue>(ImmutableSegmentedDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
	{
		return ref ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.FindValue(dictionary, key);
	}

	public static ref TValue GetValueRefOrNullRef<TKey, TValue>(ImmutableSegmentedDictionary<TKey, TValue>.Builder dictionary, TKey key) where TKey : notnull
	{
		return ref ImmutableSegmentedDictionary<TKey, TValue>.Builder.PrivateMarshal.FindValue(dictionary, key);
	}

	public static ImmutableSegmentedList<T> AsImmutableSegmentedList<T>(SegmentedList<T>? list)
	{
		return ImmutableSegmentedList<T>.PrivateMarshal.AsImmutableSegmentedList(list);
	}

	public static SegmentedList<T>? AsSegmentedList<T>(ImmutableSegmentedList<T> list)
	{
		return ImmutableSegmentedList<T>.PrivateMarshal.AsSegmentedList(list);
	}

	public static ImmutableSegmentedHashSet<T> AsImmutableSegmentedHashSet<T>(SegmentedHashSet<T>? set)
	{
		return ImmutableSegmentedHashSet<T>.PrivateMarshal.AsImmutableSegmentedHashSet(set);
	}

	public static SegmentedHashSet<T>? AsSegmentedHashSet<T>(ImmutableSegmentedHashSet<T> set)
	{
		return ImmutableSegmentedHashSet<T>.PrivateMarshal.AsSegmentedHashSet(set);
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> AsImmutableSegmentedDictionary<TKey, TValue>(SegmentedDictionary<TKey, TValue>? dictionary) where TKey : notnull
	{
		return ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.AsImmutableSegmentedDictionary(dictionary);
	}

	public static SegmentedDictionary<TKey, TValue>? AsSegmentedDictionary<TKey, TValue>(ImmutableSegmentedDictionary<TKey, TValue> dictionary) where TKey : notnull
	{
		return ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.AsSegmentedDictionary(dictionary);
	}
}
