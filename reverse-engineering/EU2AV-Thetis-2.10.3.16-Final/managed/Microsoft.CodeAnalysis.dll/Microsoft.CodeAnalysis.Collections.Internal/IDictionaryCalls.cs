using System;
using System.Collections;

namespace Microsoft.CodeAnalysis.Collections.Internal;

internal static class IDictionaryCalls
{
	public static bool IsFixedSize<TDictionary>(ref TDictionary dictionary) where TDictionary : IDictionary
	{
		return dictionary.IsFixedSize;
	}

	public static bool IsReadOnly<TDictionary>(ref TDictionary dictionary) where TDictionary : IDictionary
	{
		return dictionary.IsReadOnly;
	}

	public static object? GetItem<TDictionary>(ref TDictionary dictionary, object key) where TDictionary : IDictionary
	{
		return dictionary[key];
	}

	public static void SetItem<TDictionary>(ref TDictionary dictionary, object key, object? value) where TDictionary : IDictionary
	{
		dictionary[key] = value;
	}

	public static void Add<TDictionary>(ref TDictionary dictionary, object key, object? value) where TDictionary : IDictionary
	{
		dictionary.Add(key, value);
	}

	public static bool Contains<TDictionary>(ref TDictionary dictionary, object key) where TDictionary : IDictionary
	{
		return dictionary.Contains(key);
	}

	public static void CopyTo<TDictionary>(ref TDictionary dictionary, Array array, int index) where TDictionary : IDictionary
	{
		dictionary.CopyTo(array, index);
	}

	public static IDictionaryEnumerator GetEnumerator<TDictionary>(ref TDictionary dictionary) where TDictionary : IDictionary
	{
		return dictionary.GetEnumerator();
	}

	public static void Remove<TDictionary>(ref TDictionary dictionary, object key) where TDictionary : IDictionary
	{
		dictionary.Remove(key);
	}
}
