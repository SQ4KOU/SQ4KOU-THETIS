using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Collections;

internal class CachingDictionary<TKey, TElement> where TKey : notnull
{
	private readonly Func<TKey, ImmutableArray<TElement>> _getElementsOfKey;

	private readonly Func<IEqualityComparer<TKey>, SegmentedHashSet<TKey>> _getKeys;

	private readonly IEqualityComparer<TKey> _comparer;

	private IDictionary<TKey, ImmutableArray<TElement>>? _map;

	private static readonly ImmutableArray<TElement> s_emptySentinel = ImmutableArray<TElement>.Empty;

	public ImmutableArray<TElement> this[TKey key] => GetOrCreateValue(key);

	public int Count => EnsureFullyPopulated().Count;

	public IEnumerable<TKey> Keys => EnsureFullyPopulated().Keys;

	public CachingDictionary(Func<TKey, ImmutableArray<TElement>> getElementsOfKey, Func<IEqualityComparer<TKey>, SegmentedHashSet<TKey>> getKeys, IEqualityComparer<TKey> comparer)
	{
		_getElementsOfKey = getElementsOfKey;
		_getKeys = getKeys;
		_comparer = comparer;
	}

	public bool Contains(TKey key)
	{
		return this[key].Length != 0;
	}

	public void AddValues(ArrayBuilder<TElement> array)
	{
		foreach (KeyValuePair<TKey, ImmutableArray<TElement>> item in EnsureFullyPopulated())
		{
			array.AddRange(item.Value);
		}
	}

	private ConcurrentDictionary<TKey, ImmutableArray<TElement>> CreateConcurrentDictionary()
	{
		return new ConcurrentDictionary<TKey, ImmutableArray<TElement>>(2, 0, _comparer);
	}

	private IDictionary<TKey, ImmutableArray<TElement>> CreateDictionaryForFullyPopulatedMap(int capacity)
	{
		return new Dictionary<TKey, ImmutableArray<TElement>>(capacity, _comparer);
	}

	private ImmutableArray<TElement> GetOrCreateValue(TKey key)
	{
		IDictionary<TKey, ImmutableArray<TElement>> dictionary = _map;
		if (dictionary == null)
		{
			ConcurrentDictionary<TKey, ImmutableArray<TElement>> concurrentDictionary = CreateConcurrentDictionary();
			dictionary = Interlocked.CompareExchange(ref _map, concurrentDictionary, null);
			if (dictionary == null)
			{
				return AddToConcurrentMap(concurrentDictionary, key);
			}
		}
		if (dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		if (dictionary is ConcurrentDictionary<TKey, ImmutableArray<TElement>> map)
		{
			return AddToConcurrentMap(map, key);
		}
		return s_emptySentinel;
	}

	private ImmutableArray<TElement> AddToConcurrentMap(ConcurrentDictionary<TKey, ImmutableArray<TElement>> map, TKey key)
	{
		ImmutableArray<TElement> value = _getElementsOfKey(key);
		if (value.IsDefaultOrEmpty)
		{
			value = s_emptySentinel;
		}
		return map.GetOrAdd(key, value);
	}

	private static bool IsNotFullyPopulatedMap([NotNullWhen(false)] IDictionary<TKey, ImmutableArray<TElement>>? existingMap)
	{
		if (existingMap != null)
		{
			return existingMap is ConcurrentDictionary<TKey, ImmutableArray<TElement>>;
		}
		return true;
	}

	private IDictionary<TKey, ImmutableArray<TElement>> CreateFullyPopulatedMap(ConcurrentDictionary<TKey, ImmutableArray<TElement>>? existingMap)
	{
		SegmentedHashSet<TKey> segmentedHashSet = _getKeys(_comparer);
		IDictionary<TKey, ImmutableArray<TElement>> dictionary = CreateDictionaryForFullyPopulatedMap(segmentedHashSet.Count);
		if (existingMap == null)
		{
			foreach (TKey item in segmentedHashSet)
			{
				dictionary.Add(item, _getElementsOfKey(item));
			}
		}
		else
		{
			foreach (TKey item2 in segmentedHashSet)
			{
				ImmutableArray<TElement> orAdd = existingMap.GetOrAdd(item2, _getElementsOfKey);
				dictionary.Add(item2, orAdd);
			}
		}
		return dictionary;
	}

	private IDictionary<TKey, ImmutableArray<TElement>> EnsureFullyPopulated()
	{
		IDictionary<TKey, ImmutableArray<TElement>> dictionary = _map;
		while (IsNotFullyPopulatedMap(dictionary))
		{
			IDictionary<TKey, ImmutableArray<TElement>> dictionary2 = CreateFullyPopulatedMap((ConcurrentDictionary<TKey, ImmutableArray<TElement>>)dictionary);
			IDictionary<TKey, ImmutableArray<TElement>> dictionary3 = Interlocked.CompareExchange(ref _map, dictionary2, dictionary);
			if (dictionary3 == dictionary)
			{
				return dictionary2;
			}
			dictionary = dictionary3;
		}
		return dictionary;
	}
}
