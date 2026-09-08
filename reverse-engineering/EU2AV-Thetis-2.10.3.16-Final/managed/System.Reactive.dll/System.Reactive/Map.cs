using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace System.Reactive;

internal sealed class Map<TKey, TValue>
{
	private const int DefaultConcurrencyMultiplier = 4;

	private readonly ConcurrentDictionary<TKey, TValue> _map;

	private static int DefaultConcurrencyLevel => 4 * Environment.ProcessorCount;

	public IEnumerable<TValue> Values => _map.Values.ToArray();

	public Map(int? capacity, IEqualityComparer<TKey> comparer)
	{
		if (capacity.HasValue)
		{
			_map = new ConcurrentDictionary<TKey, TValue>(DefaultConcurrencyLevel, capacity.Value, comparer);
		}
		else
		{
			_map = new ConcurrentDictionary<TKey, TValue>(comparer);
		}
	}

	public TValue GetOrAdd(TKey key, Func<TValue> valueFactory, out bool added)
	{
		added = false;
		TValue val = default(TValue);
		bool flag = false;
		TValue value;
		while (!_map.TryGetValue(key, out value))
		{
			if (!flag)
			{
				val = valueFactory();
				flag = true;
			}
			if (_map.TryAdd(key, val))
			{
				added = true;
				return val;
			}
		}
		return value;
	}

	public bool Remove(TKey key)
	{
		TValue value;
		return _map.TryRemove(key, out value);
	}
}
