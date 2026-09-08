using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class DictionaryExtensions
{
	public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
	{
		if (dictionary.TryGetValue(key, out TValue value2))
		{
			return value2;
		}
		dictionary.Add(key, value);
		return value;
	}

	public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> getValue) where TKey : notnull
	{
		if (dictionary.TryGetValue(key, out TValue value))
		{
			return value;
		}
		TValue val = getValue();
		dictionary.Add(key, val);
		return val;
	}

	public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
	{
		if (dictionary.TryGetValue(key, out TValue _))
		{
			return false;
		}
		dictionary.Add(key, value);
		return true;
	}

	public static void AddPooled<K, V>(this Dictionary<K, ArrayBuilder<V>> dictionary, K key, V value) where K : notnull
	{
		if (!dictionary.TryGetValue(key, out ArrayBuilder<V> value2))
		{
			value2 = (dictionary[key] = ArrayBuilder<V>.GetInstance());
		}
		value2.Add(value);
	}

	public static ImmutableSegmentedDictionary<K, ImmutableArray<V>> ToImmutableSegmentedDictionaryAndFree<K, V>(this PooledDictionary<K, ArrayBuilder<V>> dictionary) where K : notnull
	{
		ImmutableSegmentedDictionary<K, ImmutableArray<V>>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<K, ImmutableArray<V>>();
		foreach (var (key, arrayBuilder2) in dictionary)
		{
			builder.Add(key, arrayBuilder2.ToImmutableAndFree());
		}
		dictionary.Free();
		return builder.ToImmutable();
	}
}
