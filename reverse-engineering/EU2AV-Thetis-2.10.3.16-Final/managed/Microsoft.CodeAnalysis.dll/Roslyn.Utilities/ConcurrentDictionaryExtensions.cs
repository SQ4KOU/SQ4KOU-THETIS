using System;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal static class ConcurrentDictionaryExtensions
{
	public static void Add<K, V>(this ConcurrentDictionary<K, V> dict, K key, V value) where K : notnull
	{
		if (!dict.TryAdd(key, value))
		{
			throw new ArgumentException("adding a duplicate", "key");
		}
	}

	public static TValue GetOrAdd<TKey, TArg, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument) where TKey : notnull
	{
		if (dictionary.TryGetValue(key, out TValue value))
		{
			return value;
		}
		Func<TKey, TValue> boundFunction;
		using (PooledDelegates.GetPooledFunction(valueFactory, factoryArgument, out boundFunction))
		{
			return dictionary.GetOrAdd(key, boundFunction);
		}
	}

	public static TValue AddOrUpdate<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument) where TKey : notnull
	{
		Func<TKey, TValue> boundFunction;
		using (PooledDelegates.GetPooledFunction(addValueFactory, factoryArgument, out boundFunction))
		{
			Func<TKey, TValue, TValue> boundFunction2;
			using (PooledDelegates.GetPooledFunction(updateValueFactory, factoryArgument, out boundFunction2))
			{
				return dictionary.AddOrUpdate(key, boundFunction, boundFunction2);
			}
		}
	}
}
