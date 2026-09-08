using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Roslyn.Utilities;

internal sealed class ImmutableSetWithInsertionOrder<T> : IEnumerable<T>, IEnumerable where T : notnull
{
	public static readonly ImmutableSetWithInsertionOrder<T> Empty = new ImmutableSetWithInsertionOrder<T>(ImmutableDictionary.Create<T, uint>(), 0u);

	private readonly ImmutableDictionary<T, uint> _map;

	private readonly uint _nextElementValue;

	public int Count => _map.Count;

	public IEnumerable<T> InInsertionOrder => from kv in _map
		orderby kv.Value
		select kv.Key;

	private ImmutableSetWithInsertionOrder(ImmutableDictionary<T, uint> map, uint nextElementValue)
	{
		_map = map;
		_nextElementValue = nextElementValue;
	}

	public bool Contains(T value)
	{
		return _map.ContainsKey(value);
	}

	public ImmutableSetWithInsertionOrder<T> Add(T value)
	{
		if (_map.ContainsKey(value))
		{
			return this;
		}
		return new ImmutableSetWithInsertionOrder<T>(_map.Add(value, _nextElementValue), _nextElementValue + 1);
	}

	public ImmutableSetWithInsertionOrder<T> AddRange(List<T> values)
	{
		ImmutableDictionary<T, uint>.Builder builder = null;
		uint num = _nextElementValue;
		foreach (T value in values)
		{
			if (builder == null)
			{
				if (_map.ContainsKey(value))
				{
					continue;
				}
				builder = _map.ToBuilder();
			}
			else if (builder.ContainsKey(value))
			{
				continue;
			}
			builder.Add(value, num);
			num++;
		}
		if (builder == null)
		{
			return this;
		}
		return new ImmutableSetWithInsertionOrder<T>(builder.ToImmutable(), num);
	}

	public ImmutableSetWithInsertionOrder<T> Remove(T value)
	{
		ImmutableDictionary<T, uint> immutableDictionary = _map.Remove(value);
		if (immutableDictionary == _map)
		{
			return this;
		}
		if (Count != 1)
		{
			return new ImmutableSetWithInsertionOrder<T>(immutableDictionary, _nextElementValue);
		}
		return Empty;
	}

	public ImmutableSetWithInsertionOrder<T> RemoveRange(List<T> values)
	{
		ImmutableDictionary<T, uint>.Builder builder = null;
		foreach (T value in values)
		{
			if (builder == null)
			{
				if (!_map.ContainsKey(value))
				{
					continue;
				}
				builder = _map.ToBuilder();
			}
			builder.Remove(value);
		}
		if (builder == null)
		{
			return this;
		}
		return new ImmutableSetWithInsertionOrder<T>(builder.ToImmutable(), _nextElementValue);
	}

	public override string ToString()
	{
		return "{" + string.Join(", ", this) + "}";
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _map.Keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _map.Keys.GetEnumerator();
	}
}
