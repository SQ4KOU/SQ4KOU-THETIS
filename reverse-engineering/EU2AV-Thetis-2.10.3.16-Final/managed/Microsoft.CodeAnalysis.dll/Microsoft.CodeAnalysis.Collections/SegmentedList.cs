using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.Collections.Internal;

namespace Microsoft.CodeAnalysis.Collections;

[DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
internal class SegmentedList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly SegmentedList<T> _list;

		private int _index;

		private readonly int _version;

		private T? _current;

		public readonly T Current => _current;

		readonly object? IEnumerator.Current
		{
			get
			{
				if (_index == 0 || _index == _list._size + 1)
				{
					ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
				}
				return Current;
			}
		}

		internal Enumerator(SegmentedList<T> list)
		{
			_list = list;
			_index = 0;
			_version = list._version;
			_current = default(T);
		}

		public readonly void Dispose()
		{
		}

		public bool MoveNext()
		{
			SegmentedList<T> list = _list;
			if (_version == list._version && (uint)_index < (uint)list._size)
			{
				_current = list._items[_index];
				_index++;
				return true;
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (_version != _list._version)
			{
				ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
			}
			_index = _list._size + 1;
			_current = default(T);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _list._version)
			{
				ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
			}
			_index = 0;
			_current = default(T);
		}
	}

	internal readonly struct TestAccessor(SegmentedList<T> instance)
	{
		public ref SegmentedArray<T> Items => ref instance._items;
	}

	private const int DefaultCapacity = 4;

	private const int MaxLength = 2147483591;

	internal SegmentedArray<T> _items;

	internal int _size;

	internal int _version;

	private static readonly SegmentedArray<T> s_emptyArray = new SegmentedArray<T>(0);

	private static IEnumerator<T>? s_emptyEnumerator;

	public int Capacity
	{
		get
		{
			return _items.Length;
		}
		set
		{
			if (value < _size)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
			}
			if (value != _items.Length)
			{
				if (value <= 0)
				{
					_items = s_emptyArray;
				}
				else if (_items.Length == 0)
				{
					_items = new SegmentedArray<T>(value);
				}
				else
				{
					_items = CreateNewSegmentedArrayReusingOldSegments(_items, value);
				}
			}
		}
	}

	public int Count => _size;

	bool IList.IsFixedSize => false;

	bool ICollection<T>.IsReadOnly => false;

	bool IList.IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	public T this[int index]
	{
		get
		{
			if ((uint)index >= (uint)_size)
			{
				ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
			}
			return _items[index];
		}
		set
		{
			if ((uint)index >= (uint)_size)
			{
				ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
			}
			_items[index] = value;
			_version++;
		}
	}

	object? IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
			try
			{
				this[index] = (T)value;
			}
			catch (InvalidCastException)
			{
				ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
			}
		}
	}

	public SegmentedList()
	{
		_items = s_emptyArray;
	}

	public SegmentedList(int capacity)
	{
		if (capacity < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (capacity == 0)
		{
			_items = s_emptyArray;
		}
		else
		{
			_items = new SegmentedArray<T>(capacity);
		}
	}

	public SegmentedList(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
		}
		if (collection is SegmentedList<T> segmentedList)
		{
			_items = (SegmentedArray<T>)segmentedList._items.Clone();
			_size = segmentedList._size;
			return;
		}
		if (collection is ICollection<T> { Count: var count } collection2)
		{
			if (count == 0)
			{
				_items = s_emptyArray;
				return;
			}
			_items = new SegmentedArray<T>(count);
			T[][] array = SegmentedCollectionsMarshal.AsSegments(_items);
			if (array != null && array.Length == 1)
			{
				collection2.CopyTo(array[0], 0);
				_size = count;
				return;
			}
		}
		else
		{
			_items = s_emptyArray;
		}
		foreach (T item in collection)
		{
			Add(item);
		}
	}

	private static SegmentedArray<T> CreateNewSegmentedArrayReusingOldSegments(SegmentedArray<T> oldArray, int newSize)
	{
		T[][] array = SegmentedCollectionsMarshal.AsSegments(oldArray);
		int num = array.Length;
		int num2 = newSize + SegmentedArrayHelper.GetSegmentSize<T>() - 1 >> SegmentedArrayHelper.GetSegmentShift<T>();
		Array.Resize(ref array, num2);
		for (int i = num - 1; i < num2 - 1; i++)
		{
			Array.Resize(ref array[i], SegmentedArrayHelper.GetSegmentSize<T>());
		}
		int newSize2 = newSize - (num2 - 1 << SegmentedArrayHelper.GetSegmentShift<T>());
		Array.Resize(ref array[num2 - 1], newSize2);
		return SegmentedCollectionsMarshal.AsSegmentedArray(newSize, array);
	}

	private static bool IsCompatibleObject(object? value)
	{
		if (!(value is T))
		{
			if (value == null)
			{
				return default(T) == null;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T item)
	{
		_version++;
		SegmentedArray<T> items = _items;
		int size = _size;
		if ((uint)size < (uint)items.Length)
		{
			_size = size + 1;
			items[size] = item;
		}
		else
		{
			AddWithResize(item);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddWithResize(T item)
	{
		int size = _size;
		Grow(size + 1);
		_size = size + 1;
		_items[size] = item;
	}

	int IList.Add(object? item)
	{
		ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
		try
		{
			Add((T)item);
		}
		catch (InvalidCastException)
		{
			ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
		}
		return Count - 1;
	}

	public void AddRange(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
		}
		if (collection is ICollection<T> { Count: var count } collection2)
		{
			if (count <= 0)
			{
				return;
			}
			if (_items.Length - _size < count)
			{
				Grow(checked(_size + count));
			}
			if (collection2 is SegmentedList<T> segmentedList)
			{
				SegmentedArray.Copy(segmentedList._items, 0, _items, _size, segmentedList.Count);
			}
			else if (collection2 is SegmentedArray<T> sourceArray)
			{
				SegmentedArray.Copy(sourceArray, 0, _items, _size, sourceArray.Length);
			}
			else
			{
				int size = _size;
				foreach (T item in collection2)
				{
					_items[size++] = item;
				}
			}
			_size += count;
			_version++;
			return;
		}
		foreach (T item2 in collection)
		{
			Add(item2);
		}
	}

	public ReadOnlyCollection<T> AsReadOnly()
	{
		return new ReadOnlyCollection<T>(this);
	}

	public int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		return SegmentedArray.BinarySearch(_items, index, count, item, comparer);
	}

	public int BinarySearch(T item)
	{
		return BinarySearch(0, Count, item, null);
	}

	public int BinarySearch(T item, IComparer<T>? comparer)
	{
		return BinarySearch(0, Count, item, comparer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		_version++;
		int size = _size;
		_size = 0;
		if (size > 0)
		{
			SegmentedArray.Clear(_items, 0, size);
		}
	}

	public bool Contains(T item)
	{
		if (_size != 0)
		{
			return IndexOf(item) >= 0;
		}
		return false;
	}

	bool IList.Contains(object? item)
	{
		if (IsCompatibleObject(item))
		{
			return Contains((T)item);
		}
		return false;
	}

	public SegmentedList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
	{
		if (converter == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
		}
		SegmentedList<TOutput> segmentedList = new SegmentedList<TOutput>(_size);
		for (int i = 0; i < _size; i++)
		{
			segmentedList._items[i] = converter(_items[i]);
		}
		segmentedList._size = _size;
		return segmentedList;
	}

	public void CopyTo(T[] array)
	{
		CopyTo(array, 0);
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		if (array != null && array.Rank != 1)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
		}
		try
		{
			SegmentedArray.Copy(_items, 0, array, arrayIndex, _size);
		}
		catch (ArrayTypeMismatchException)
		{
			ThrowHelper.ThrowArgumentException_Argument_IncompatibleArrayType();
		}
	}

	public void CopyTo(int index, T[] array, int arrayIndex, int count)
	{
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		SegmentedArray.Copy(_items, index, array, arrayIndex, count);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		SegmentedArray.Copy(_items, 0, array, arrayIndex, _size);
	}

	public int EnsureCapacity(int capacity)
	{
		if (capacity < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_items.Length < capacity)
		{
			Grow(capacity);
		}
		return _items.Length;
	}

	internal void Grow(int capacity)
	{
		int num = 0;
		if (_items.Length < SegmentedArrayHelper.GetSegmentSize<T>() / 2)
		{
			num = ((_items.Length == 0) ? 4 : (_items.Length * 2));
		}
		else if (_items.Length < SegmentedArrayHelper.GetSegmentSize<T>())
		{
			num = SegmentedArrayHelper.GetSegmentSize<T>();
		}
		else if ((_items.Length & SegmentedArrayHelper.GetOffsetMask<T>()) == 0)
		{
			int num2 = _items.Length + SegmentedArrayHelper.GetSegmentSize<T>() - 1 >> SegmentedArrayHelper.GetSegmentShift<T>();
			int num3 = num2 + Math.Max(1, num2 >> 3);
			num = SegmentedArrayHelper.GetSegmentSize<T>() * num3;
		}
		if (num < capacity)
		{
			num = capacity;
		}
		if (num > SegmentedArrayHelper.GetSegmentSize<T>())
		{
			int num4 = num & SegmentedArrayHelper.GetOffsetMask<T>();
			if (num4 > 0)
			{
				num = num - num4 + SegmentedArrayHelper.GetSegmentSize<T>();
			}
			if ((uint)num > 2147483591u)
			{
				num = 2147483591;
			}
		}
		Capacity = num;
	}

	public bool Exists(Predicate<T> match)
	{
		return FindIndex(match) != -1;
	}

	public T? Find(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		for (int i = 0; i < _size; i++)
		{
			if (match(_items[i]))
			{
				return _items[i];
			}
		}
		return default(T);
	}

	public SegmentedList<T> FindAll(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		SegmentedList<T> segmentedList = new SegmentedList<T>();
		for (int i = 0; i < _size; i++)
		{
			if (match(_items[i]))
			{
				segmentedList.Add(_items[i]);
			}
		}
		return segmentedList;
	}

	public int FindIndex(Predicate<T> match)
	{
		return FindIndex(0, _size, match);
	}

	public int FindIndex(int startIndex, Predicate<T> match)
	{
		return FindIndex(startIndex, _size - startIndex, match);
	}

	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		if ((uint)startIndex > (uint)_size)
		{
			ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLessOrEqual();
		}
		if (count < 0 || startIndex > _size - count)
		{
			ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
		}
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		int num = startIndex + count;
		for (int i = startIndex; i < num; i++)
		{
			if (match(_items[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public T? FindLast(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		for (int num = _size - 1; num >= 0; num--)
		{
			if (match(_items[num]))
			{
				return _items[num];
			}
		}
		return default(T);
	}

	public int FindLastIndex(Predicate<T> match)
	{
		return FindLastIndex(_size - 1, _size, match);
	}

	public int FindLastIndex(int startIndex, Predicate<T> match)
	{
		return FindLastIndex(startIndex, startIndex + 1, match);
	}

	public int FindLastIndex(int startIndex, int count, Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		if (_size == 0)
		{
			if (startIndex != -1)
			{
				ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess();
			}
		}
		else if ((uint)startIndex >= (uint)_size)
		{
			ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess();
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
		}
		int num = startIndex - count;
		for (int num2 = startIndex; num2 > num; num2--)
		{
			if (match(_items[num2]))
			{
				return num2;
			}
		}
		return -1;
	}

	public void ForEach(Action<T> action)
	{
		if (action == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);
		}
		int version = _version;
		for (int i = 0; i < _size; i++)
		{
			if (version != _version)
			{
				break;
			}
			action(_items[i]);
		}
		if (version != _version)
		{
			ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (Count != 0)
		{
			return GetEnumerator();
		}
		return GetEmptyEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}

	private static IEnumerator<T> GetEmptyEnumerator()
	{
		return LazyInitializer.EnsureInitialized(ref s_emptyEnumerator, () => new Enumerator(new SegmentedList<T>(0)));
	}

	public SegmentedList<T> GetRange(int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		SegmentedList<T> segmentedList = new SegmentedList<T>(count);
		SegmentedArray.Copy(_items, index, segmentedList._items, 0, count);
		segmentedList._size = count;
		return segmentedList;
	}

	public SegmentedList<T> Slice(int start, int length)
	{
		return GetRange(start, length);
	}

	public int IndexOf(T item)
	{
		return SegmentedArray.IndexOf(_items, item, 0, _size);
	}

	int IList.IndexOf(object? item)
	{
		if (IsCompatibleObject(item))
		{
			return IndexOf((T)item);
		}
		return -1;
	}

	public int IndexOf(T item, int index)
	{
		if (index > _size)
		{
			ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
		}
		return SegmentedArray.IndexOf(_items, item, index, _size - index);
	}

	public int IndexOf(T item, int index, int count)
	{
		if (index > _size)
		{
			ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
		}
		if (count < 0 || index > _size - count)
		{
			ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
		}
		return SegmentedArray.IndexOf(_items, item, index, count);
	}

	public int IndexOf(T item, int index, int count, IEqualityComparer<T>? comparer)
	{
		if (index > _size)
		{
			ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
		}
		if (count < 0 || index > _size - count)
		{
			ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
		}
		return SegmentedArray.IndexOf(_items, item, index, count, comparer);
	}

	public void Insert(int index, T item)
	{
		if ((uint)index > (uint)_size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
		}
		if (_size == _items.Length)
		{
			Grow(_size + 1);
		}
		if (index < _size)
		{
			SegmentedArray.Copy(_items, index, _items, index + 1, _size - index);
		}
		_items[index] = item;
		_size++;
		_version++;
	}

	void IList.Insert(int index, object? item)
	{
		ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
		try
		{
			Insert(index, (T)item);
		}
		catch (InvalidCastException)
		{
			ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
		}
	}

	public void InsertRange(int index, IEnumerable<T> collection)
	{
		if (collection == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
		}
		if ((uint)index > (uint)_size)
		{
			ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
		}
		if (collection is ICollection<T> { Count: var count } collection2)
		{
			if (count <= 0)
			{
				return;
			}
			if (_items.Length - _size < count)
			{
				Grow(checked(_size + count));
			}
			if (index < _size)
			{
				SegmentedArray.Copy(_items, index, _items, index + count, _size - index);
			}
			if (this == collection2)
			{
				SegmentedArray.Copy(_items, 0, _items, index, index);
				SegmentedArray.Copy(_items, index + count, _items, index * 2, _size - index);
			}
			else if (collection2 is SegmentedList<T> segmentedList)
			{
				SegmentedArray.Copy(segmentedList._items, 0, _items, index, segmentedList.Count);
			}
			else if (collection2 is SegmentedArray<T> sourceArray)
			{
				SegmentedArray.Copy(sourceArray, 0, _items, index, sourceArray.Length);
			}
			else
			{
				int num = index;
				foreach (T item in collection2)
				{
					_items[num++] = item;
				}
			}
			_size += count;
			_version++;
			return;
		}
		using IEnumerator<T> enumerator2 = collection.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			Insert(index++, enumerator2.Current);
		}
	}

	public int LastIndexOf(T item)
	{
		if (_size == 0)
		{
			return -1;
		}
		return LastIndexOf(item, _size - 1, _size);
	}

	public int LastIndexOf(T item, int index)
	{
		if (index >= _size)
		{
			ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
		}
		return LastIndexOf(item, index, index + 1);
	}

	public int LastIndexOf(T item, int index, int count)
	{
		if (_size == 0)
		{
			return -1;
		}
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (index >= _size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
		}
		if (count > index + 1)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
		}
		return SegmentedArray.LastIndexOf(_items, item, index, count);
	}

	public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? comparer)
	{
		if (_size == 0)
		{
			return -1;
		}
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (index >= _size)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
		}
		if (count > index + 1)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
		}
		return SegmentedArray.LastIndexOf(_items, item, index, count, comparer);
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	void IList.Remove(object? item)
	{
		if (IsCompatibleObject(item))
		{
			Remove((T)item);
		}
	}

	public int RemoveAll(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		int i;
		for (i = 0; i < _size && !match(_items[i]); i++)
		{
		}
		if (i >= _size)
		{
			return 0;
		}
		int j = i + 1;
		while (j < _size)
		{
			for (; j < _size && match(_items[j]); j++)
			{
			}
			if (j < _size)
			{
				_items[i++] = _items[j++];
			}
		}
		SegmentedArray.Clear(_items, i, _size - i);
		int result = _size - i;
		_size = i;
		_version++;
		return result;
	}

	public void RemoveAt(int index)
	{
		if ((uint)index >= (uint)_size)
		{
			ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
		}
		_size--;
		if (index < _size)
		{
			SegmentedArray.Copy(_items, index + 1, _items, index, _size - index);
		}
		_items[_size] = default(T);
		_version++;
	}

	public void RemoveRange(int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		if (count > 0)
		{
			_size -= count;
			if (index < _size)
			{
				SegmentedArray.Copy(_items, index + count, _items, index, _size - index);
			}
			_version++;
			SegmentedArray.Clear(_items, _size, count);
		}
	}

	public void Reverse()
	{
		Reverse(0, Count);
	}

	public void Reverse(int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		if (count > 1)
		{
			SegmentedArray.Reverse(_items, index, count);
		}
		_version++;
	}

	public void Sort()
	{
		Sort(0, Count, null);
	}

	public void Sort(IComparer<T>? comparer)
	{
		Sort(0, Count, comparer);
	}

	public void Sort(int index, int count, IComparer<T>? comparer)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (_size - index < count)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		if (count > 1)
		{
			SegmentedArray.Sort(_items, index, count, comparer);
		}
		_version++;
	}

	public void Sort(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
		}
		if (_size > 1)
		{
			SegmentedArraySortHelper<T>.Sort(new SegmentedArraySegment<T>(_items, 0, _size), comparison);
		}
		_version++;
	}

	public T[] ToArray()
	{
		if (_size == 0)
		{
			return Array.Empty<T>();
		}
		T[] array = new T[_size];
		SegmentedArray.Copy(_items, array, _size);
		return array;
	}

	public void TrimExcess()
	{
		int num = (int)((double)_items.Length * 0.9);
		if (_size < num)
		{
			Capacity = _size;
		}
	}

	public bool TrueForAll(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		for (int i = 0; i < _size; i++)
		{
			if (!match(_items[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal TestAccessor GetTestAccessor()
	{
		return new TestAccessor(this);
	}
}
