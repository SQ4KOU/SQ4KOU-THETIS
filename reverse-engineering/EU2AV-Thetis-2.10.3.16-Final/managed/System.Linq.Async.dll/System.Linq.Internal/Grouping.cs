using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace System.Linq.Internal;

internal class Grouping<TKey, TElement> : IGrouping<TKey, TElement>, IEnumerable<TElement>, IEnumerable, IList<TElement>, ICollection<TElement>, IAsyncGrouping<TKey, TElement>, IAsyncEnumerable<TElement>
{
	internal int _count;

	internal TElement[] _elements;

	internal int _hashCode;

	internal Grouping<TKey, TElement> _hashNext;

	internal TKey _key;

	internal Grouping<TKey, TElement>? _next;

	public TKey Key => _key;

	int ICollection<TElement>.Count => _count;

	bool ICollection<TElement>.IsReadOnly => true;

	TElement IList<TElement>.this[int index]
	{
		get
		{
			if (index < 0 || index >= _count)
			{
				throw System.Error.ArgumentOutOfRange("index");
			}
			return _elements[index];
		}
		set
		{
			throw System.Error.NotSupported();
		}
	}

	public Grouping(TKey key, int hashCode, TElement[] elements, Grouping<TKey, TElement> hashNext)
	{
		_key = key;
		_hashCode = hashCode;
		_elements = elements;
		_hashNext = hashNext;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<TElement> GetEnumerator()
	{
		for (int i = 0; i < _count; i++)
		{
			yield return _elements[i];
		}
	}

	void ICollection<TElement>.Add(TElement item)
	{
		throw System.Error.NotSupported();
	}

	void ICollection<TElement>.Clear()
	{
		throw System.Error.NotSupported();
	}

	bool ICollection<TElement>.Contains(TElement item)
	{
		return Array.IndexOf<TElement>(_elements, item, 0, _count) >= 0;
	}

	void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
	{
		Array.Copy(_elements, 0, array, arrayIndex, _count);
	}

	bool ICollection<TElement>.Remove(TElement item)
	{
		throw System.Error.NotSupported();
	}

	int IList<TElement>.IndexOf(TElement item)
	{
		return Array.IndexOf<TElement>(_elements, item, 0, _count);
	}

	void IList<TElement>.Insert(int index, TElement item)
	{
		throw System.Error.NotSupported();
	}

	void IList<TElement>.RemoveAt(int index)
	{
		throw System.Error.NotSupported();
	}

	internal void Add(TElement element)
	{
		if (_elements.Length == _count)
		{
			Array.Resize(ref _elements, checked(_count * 2));
		}
		_elements[_count] = element;
		_count++;
	}

	internal void Trim()
	{
		if (_elements.Length != _count)
		{
			Array.Resize(ref _elements, _count);
		}
	}

	IAsyncEnumerator<TElement> IAsyncEnumerable<TElement>.GetAsyncEnumerator(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return AsyncEnumerable.ToAsyncEnumerable(this).GetAsyncEnumerator(cancellationToken);
	}
}
