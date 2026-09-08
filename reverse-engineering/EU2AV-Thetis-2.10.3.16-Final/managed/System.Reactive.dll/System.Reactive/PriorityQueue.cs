using System.Collections.Generic;

namespace System.Reactive;

internal sealed class PriorityQueue<T> where T : IComparable<T>
{
	private struct IndexedItem : IComparable<IndexedItem>
	{
		public T Value;

		public long Id;

		public int CompareTo(IndexedItem other)
		{
			ref T value = ref Value;
			T value2 = other.Value;
			int num = value.CompareTo(value2);
			if (num == 0)
			{
				num = Id.CompareTo(other.Id);
			}
			return num;
		}
	}

	private long _count = long.MinValue;

	private IndexedItem[] _items;

	private int _size;

	public int Count => _size;

	public PriorityQueue()
		: this(16)
	{
	}

	public PriorityQueue(int capacity)
	{
		_items = new IndexedItem[capacity];
		_size = 0;
	}

	private bool IsHigherPriority(int left, int right)
	{
		return _items[left].CompareTo(_items[right]) < 0;
	}

	private int Percolate(int index)
	{
		if (index >= _size || index < 0)
		{
			return index;
		}
		int num = (index - 1) / 2;
		while (num >= 0 && num != index && IsHigherPriority(index, num))
		{
			ref IndexedItem reference = ref _items[num];
			ref IndexedItem reference2 = ref _items[index];
			IndexedItem indexedItem = _items[index];
			IndexedItem indexedItem2 = _items[num];
			reference = indexedItem;
			reference2 = indexedItem2;
			index = num;
			num = (index - 1) / 2;
		}
		return index;
	}

	private void Heapify(int index)
	{
		if (index >= _size || index < 0)
		{
			return;
		}
		while (true)
		{
			int num = 2 * index + 1;
			int num2 = 2 * index + 2;
			int num3 = index;
			if (num < _size && IsHigherPriority(num, num3))
			{
				num3 = num;
			}
			if (num2 < _size && IsHigherPriority(num2, num3))
			{
				num3 = num2;
			}
			if (num3 != index)
			{
				ref IndexedItem reference = ref _items[num3];
				ref IndexedItem reference2 = ref _items[index];
				IndexedItem indexedItem = _items[index];
				IndexedItem indexedItem2 = _items[num3];
				reference = indexedItem;
				reference2 = indexedItem2;
				index = num3;
				continue;
			}
			break;
		}
	}

	public T Peek()
	{
		if (_size == 0)
		{
			throw new InvalidOperationException(Strings_Core.HEAP_EMPTY);
		}
		return _items[0].Value;
	}

	private void RemoveAt(int index)
	{
		_items[index] = _items[--_size];
		_items[_size] = default(IndexedItem);
		if (Percolate(index) == index)
		{
			Heapify(index);
		}
		if (_size < _items.Length / 4)
		{
			IndexedItem[] items = _items;
			_items = new IndexedItem[_items.Length / 2];
			Array.Copy(items, 0, _items, 0, _size);
		}
	}

	public T Dequeue()
	{
		T result = Peek();
		RemoveAt(0);
		return result;
	}

	public void Enqueue(T item)
	{
		if (_size >= _items.Length)
		{
			IndexedItem[] items = _items;
			_items = new IndexedItem[_items.Length * 2];
			Array.Copy(items, _items, items.Length);
		}
		int num = _size++;
		_items[num] = new IndexedItem
		{
			Value = item,
			Id = ++_count
		};
		Percolate(num);
	}

	public bool Remove(T item)
	{
		for (int i = 0; i < _size; i++)
		{
			if (EqualityComparer<T>.Default.Equals(_items[i].Value, item))
			{
				RemoveAt(i);
				return true;
			}
		}
		return false;
	}
}
