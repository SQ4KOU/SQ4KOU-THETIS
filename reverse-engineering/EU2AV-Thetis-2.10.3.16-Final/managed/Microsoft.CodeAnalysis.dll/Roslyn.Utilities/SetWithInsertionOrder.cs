using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal sealed class SetWithInsertionOrder<T> : IOrderedReadOnlySet<T>, IReadOnlySet<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>
{
	private HashSet<T>? _set;

	private ArrayBuilder<T>? _elements;

	public int Count => _elements?.Count ?? 0;

	public T this[int i] => _elements[i];

	private IReadOnlySet<T> Set => ((IReadOnlySet<T>)_set) ?? SpecializedCollections.EmptyReadOnlySet<T>();

	public bool Add(T value)
	{
		if (_set == null)
		{
			_set = new HashSet<T>();
			_elements = new ArrayBuilder<T>();
		}
		if (!_set.Add(value))
		{
			return false;
		}
		_elements.Add(value);
		return true;
	}

	public bool Insert(int index, T value)
	{
		if (_set == null)
		{
			if (index > 0)
			{
				throw new IndexOutOfRangeException();
			}
			Add(value);
		}
		else
		{
			if (!_set.Add(value))
			{
				return false;
			}
			try
			{
				_elements.Insert(index, value);
			}
			catch
			{
				_set.Remove(value);
				throw;
			}
		}
		return true;
	}

	public bool Remove(T value)
	{
		if (_set == null)
		{
			return false;
		}
		if (!_set.Remove(value))
		{
			return false;
		}
		_elements.RemoveAt(_elements.IndexOf(value));
		return true;
	}

	public bool Contains(T value)
	{
		return _set?.Contains(value) ?? false;
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (_elements != null)
		{
			return ((IEnumerable<T>)_elements).GetEnumerator();
		}
		return SpecializedCollections.EmptyEnumerator<T>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public ImmutableArray<T> AsImmutable()
	{
		return _elements.ToImmutableArrayOrEmpty();
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		return Set.IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		return Set.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		return Set.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		return Set.IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		return Set.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		return Set.SetEquals(other);
	}
}
