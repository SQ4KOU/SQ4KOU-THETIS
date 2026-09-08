using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Collections;

internal sealed class OrderedSet<T> : IOrderedReadOnlySet<T>, IReadOnlySet<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>
{
	private readonly HashSet<T> _set;

	private readonly ArrayBuilder<T> _list;

	public int Count => _list.Count;

	public T this[int index] => _list[index];

	public OrderedSet()
	{
		_set = new HashSet<T>();
		_list = new ArrayBuilder<T>();
	}

	public OrderedSet(IEnumerable<T> items)
		: this()
	{
		AddRange(items);
	}

	public void AddRange(IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			Add(item);
		}
	}

	public bool Add(T item)
	{
		if (_set.Add(item))
		{
			_list.Add(item);
			return true;
		}
		return false;
	}

	public bool Contains(T item)
	{
		return _set.Contains(item);
	}

	public ArrayBuilder<T>.Enumerator GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)_list).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_list).GetEnumerator();
	}

	public void Clear()
	{
		_set.Clear();
		_list.Clear();
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		return _set.IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		return _set.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		return _set.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		return _set.IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		return _set.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		return _set.SetEquals(other);
	}
}
