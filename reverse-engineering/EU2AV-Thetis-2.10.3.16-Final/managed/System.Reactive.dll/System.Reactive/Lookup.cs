using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Reactive;

internal sealed class Lookup<K, E> : ILookup<K, E>, IEnumerable<IGrouping<K, E>>, IEnumerable
{
	private sealed class Grouping : IGrouping<K, E>, IEnumerable<E>, IEnumerable
	{
		private readonly KeyValuePair<K, List<E>> _keyValuePair;

		public K Key => _keyValuePair.Key;

		public Grouping(KeyValuePair<K, List<E>> keyValuePair)
		{
			_keyValuePair = keyValuePair;
		}

		public IEnumerator<E> GetEnumerator()
		{
			return _keyValuePair.Value.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private readonly Dictionary<K, List<E>> _dictionary;

	public int Count => _dictionary.Count;

	public IEnumerable<E> this[K key]
	{
		get
		{
			if (!_dictionary.TryGetValue(key, out var value))
			{
				return Array.Empty<E>();
			}
			return Hide(value);
		}
	}

	public Lookup(IEqualityComparer<K> comparer)
	{
		_dictionary = new Dictionary<K, List<E>>(comparer);
	}

	public void Add(K key, E element)
	{
		if (!_dictionary.TryGetValue(key, out var value))
		{
			value = (_dictionary[key] = new List<E>());
		}
		value.Add(element);
	}

	public bool Contains(K key)
	{
		return _dictionary.ContainsKey(key);
	}

	private static IEnumerable<E> Hide(List<E> elements)
	{
		return elements.Skip(0);
	}

	public IEnumerator<IGrouping<K, E>> GetEnumerator()
	{
		foreach (KeyValuePair<K, List<E>> item in _dictionary)
		{
			yield return new Grouping(item);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
