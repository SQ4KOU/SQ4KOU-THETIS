namespace System.Collections.Generic;

internal static class DictionaryExtensions
{
	public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
	{
		if (!dictionary.ContainsKey(key))
		{
			dictionary[key] = value;
			return true;
		}
		return false;
	}
}
