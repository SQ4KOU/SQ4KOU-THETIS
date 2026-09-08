using System;

namespace Roslyn.Utilities;

internal sealed class WeakList<T> where T : class
{
	public struct Enumerator(WeakList<T> weakList)
	{
		private readonly WeakList<T> _weakList = weakList;

		private readonly int _count = weakList._size;

		private int _nextIndex = 0;

		private int _alive = weakList._size;

		private int _firstDead = -1;

		private T? _current = null;

		public T Current => _current;

		public bool MoveNext()
		{
			while (_nextIndex < _count)
			{
				int nextIndex = _nextIndex;
				_nextIndex++;
				if (_weakList._items[nextIndex].TryGetTarget(out var target))
				{
					_current = target;
					return true;
				}
				if (_firstDead < 0)
				{
					_firstDead = nextIndex;
				}
				_alive--;
			}
			if (_alive == 0)
			{
				_weakList._items = Array.Empty<WeakReference<T>>();
				_weakList._size = 0;
			}
			else if (_alive < _weakList._items.Length / 4)
			{
				_weakList.Shrink(_firstDead, _alive);
			}
			return false;
		}
	}

	private WeakReference<T>[] _items;

	private int _size;

	private const int MinimalNonEmptySize = 4;

	public int WeakCount => _size;

	internal WeakReference<T>[] TestOnly_UnderlyingArray => _items;

	public WeakList()
	{
		_items = Array.Empty<WeakReference<T>>();
	}

	private void Resize()
	{
		int num = _items.Length;
		int num2 = -1;
		for (int i = 0; i < _items.Length; i++)
		{
			if (!_items[i].TryGetTarget(out var _))
			{
				if (num2 == -1)
				{
					num2 = i;
				}
				num--;
			}
		}
		if (num < _items.Length / 4)
		{
			Shrink(num2, num);
		}
		else if (num >= 3 * _items.Length / 4)
		{
			WeakReference<T>[] array = new WeakReference<T>[GetExpandedSize(_items.Length)];
			if (num2 >= 0)
			{
				Compact(num2, array);
			}
			else
			{
				Array.Copy(_items, 0, array, 0, _items.Length);
			}
			_items = array;
		}
		else
		{
			Compact(num2, _items);
		}
	}

	private void Shrink(int firstDead, int alive)
	{
		int expandedSize = GetExpandedSize(alive);
		WeakReference<T>[] array = ((expandedSize == _items.Length) ? _items : new WeakReference<T>[expandedSize]);
		Compact(firstDead, array);
		_items = array;
	}

	private static int GetExpandedSize(int baseSize)
	{
		return Math.Max(baseSize * 2 + 1, 4);
	}

	private void Compact(int firstDead, WeakReference<T>[] result)
	{
		if (_items != result)
		{
			Array.Copy(_items, 0, result, 0, firstDead);
		}
		int size = _size;
		int num = firstDead;
		for (int i = firstDead + 1; i < size; i++)
		{
			WeakReference<T> weakReference = _items[i];
			if (weakReference.TryGetTarget(out var _))
			{
				result[num++] = weakReference;
			}
		}
		_size = num;
		if (_items == result)
		{
			while (num < size)
			{
				_items[num++] = null;
			}
		}
	}

	public WeakReference<T> GetWeakReference(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return _items[index];
	}

	public void Add(T item)
	{
		if (_size == _items.Length)
		{
			Resize();
		}
		_items[_size++] = new WeakReference<T>(item);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
