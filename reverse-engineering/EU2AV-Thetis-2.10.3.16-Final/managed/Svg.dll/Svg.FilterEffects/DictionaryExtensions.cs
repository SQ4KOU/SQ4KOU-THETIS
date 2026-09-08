using System;
using System.Collections.Generic;

namespace Svg.FilterEffects;

internal static class DictionaryExtensions
{
	public static void Clear<TKey, TValue>(this Dictionary<TKey, TValue> self, Action<TValue> action)
	{
		foreach (KeyValuePair<TKey, TValue> item in self)
		{
			action(item.Value);
		}
		self.Clear();
	}

	public static bool Remove<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, Action<TValue> action)
	{
		if (!self.ContainsKey(key))
		{
			return false;
		}
		action(self[key]);
		return self.Remove(key);
	}
}
