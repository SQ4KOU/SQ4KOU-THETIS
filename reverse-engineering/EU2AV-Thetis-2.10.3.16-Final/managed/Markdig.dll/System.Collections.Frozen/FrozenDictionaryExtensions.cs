using System.Collections.Generic;

namespace System.Collections.Frozen;

internal static class FrozenDictionaryExtensions
{
	public static FrozenDictionary<TKey, TValue> ToFrozenDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
	{
		return new FrozenDictionary<TKey, TValue>(dictionary);
	}
}
