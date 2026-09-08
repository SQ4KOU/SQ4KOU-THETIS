using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.Collections.Internal;

namespace Microsoft.CodeAnalysis.Collections;

internal static class ImmutableSegmentedList
{
	public static ImmutableSegmentedList<T> Create<T>()
	{
		return ImmutableSegmentedList<T>.Empty;
	}

	public static ImmutableSegmentedList<T> Create<T>(T item)
	{
		return ImmutableSegmentedList<T>.Empty.Add(item);
	}

	public static ImmutableSegmentedList<T> Create<T>(params T[] items)
	{
		return ImmutableSegmentedList<T>.Empty.AddRange(items);
	}

	public static ImmutableSegmentedList<T>.Builder CreateBuilder<T>()
	{
		return ImmutableSegmentedList<T>.Empty.ToBuilder();
	}

	public static ImmutableSegmentedList<T> CreateRange<T>(IEnumerable<T> items)
	{
		return ImmutableSegmentedList<T>.Empty.AddRange(items);
	}

	public static ImmutableSegmentedList<T> ToImmutableSegmentedList<T>(this IEnumerable<T> source)
	{
		if (source is ImmutableSegmentedList<T>)
		{
			return (ImmutableSegmentedList<T>)(object)source;
		}
		return ImmutableSegmentedList<T>.Empty.AddRange(source);
	}

	public static ImmutableSegmentedList<T> ToImmutableSegmentedList<T>(this ImmutableSegmentedList<T>.Builder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		return builder.ToImmutable();
	}
}
internal readonly struct ImmutableSegmentedList<T> : IImmutableList<T>, IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, IList<T>, ICollection<T>, IList, ICollection, IEquatable<ImmutableSegmentedList<T>>
{
	public sealed class Builder : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IList, ICollection
	{
		private ValueBuilder _builder;

		public int Count => _builder.Count;

		bool ICollection<T>.IsReadOnly => ICollectionCalls<T>.IsReadOnly(ref _builder);

		bool IList.IsFixedSize => IListCalls.IsFixedSize(ref _builder);

		bool IList.IsReadOnly => IListCalls.IsReadOnly(ref _builder);

		bool ICollection.IsSynchronized => ICollectionCalls.IsSynchronized(ref _builder);

		object ICollection.SyncRoot => this;

		public T this[int index]
		{
			get
			{
				return _builder[index];
			}
			set
			{
				_builder[index] = value;
			}
		}

		object? IList.this[int index]
		{
			get
			{
				return IListCalls.GetItem(ref _builder, index);
			}
			set
			{
				IListCalls.SetItem(ref _builder, index, value);
			}
		}

		internal Builder(ImmutableSegmentedList<T> list)
		{
			_builder = new ValueBuilder(list);
		}

		public ref readonly T ItemRef(int index)
		{
			return ref _builder.ItemRef(index);
		}

		public void Add(T item)
		{
			_builder.Add(item);
		}

		public void AddRange(IEnumerable<T> items)
		{
			_builder.AddRange(items);
		}

		public int BinarySearch(T item)
		{
			return _builder.BinarySearch(item);
		}

		public int BinarySearch(T item, IComparer<T>? comparer)
		{
			return _builder.BinarySearch(item, comparer);
		}

		public int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
		{
			return _builder.BinarySearch(index, count, item, comparer);
		}

		public void Clear()
		{
			_builder.Clear();
		}

		public bool Contains(T item)
		{
			return _builder.Contains(item);
		}

		public ImmutableSegmentedList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		{
			return _builder.ConvertAll(converter);
		}

		public void CopyTo(T[] array)
		{
			_builder.CopyTo(array);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_builder.CopyTo(array, arrayIndex);
		}

		public void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			_builder.CopyTo(index, array, arrayIndex, count);
		}

		public bool Exists(Predicate<T> match)
		{
			return _builder.Exists(match);
		}

		public T? Find(Predicate<T> match)
		{
			return _builder.Find(match);
		}

		public ImmutableSegmentedList<T> FindAll(Predicate<T> match)
		{
			return _builder.FindAll(match);
		}

		public int FindIndex(Predicate<T> match)
		{
			return _builder.FindIndex(match);
		}

		public int FindIndex(int startIndex, Predicate<T> match)
		{
			return _builder.FindIndex(startIndex, match);
		}

		public int FindIndex(int startIndex, int count, Predicate<T> match)
		{
			return _builder.FindIndex(startIndex, count, match);
		}

		public T? FindLast(Predicate<T> match)
		{
			return _builder.FindLast(match);
		}

		public int FindLastIndex(Predicate<T> match)
		{
			return _builder.FindLastIndex(match);
		}

		public int FindLastIndex(int startIndex, Predicate<T> match)
		{
			return _builder.FindLastIndex(startIndex, match);
		}

		public int FindLastIndex(int startIndex, int count, Predicate<T> match)
		{
			return _builder.FindLastIndex(startIndex, count, match);
		}

		public void ForEach(Action<T> action)
		{
			_builder.ForEach(action);
		}

		public Enumerator GetEnumerator()
		{
			return _builder.GetEnumerator();
		}

		public ImmutableSegmentedList<T> GetRange(int index, int count)
		{
			return _builder.GetRange(index, count);
		}

		public int IndexOf(T item)
		{
			return _builder.IndexOf(item);
		}

		public int IndexOf(T item, int index)
		{
			return _builder.IndexOf(item, index);
		}

		public int IndexOf(T item, int index, int count)
		{
			return _builder.IndexOf(item, index, count);
		}

		public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
		{
			return _builder.IndexOf(item, index, count, equalityComparer);
		}

		public void Insert(int index, T item)
		{
			_builder.Insert(index, item);
		}

		public void InsertRange(int index, IEnumerable<T> items)
		{
			_builder.InsertRange(index, items);
		}

		public int LastIndexOf(T item)
		{
			return _builder.LastIndexOf(item);
		}

		public int LastIndexOf(T item, int startIndex)
		{
			return _builder.LastIndexOf(item, startIndex);
		}

		public int LastIndexOf(T item, int startIndex, int count)
		{
			return _builder.LastIndexOf(item, startIndex, count);
		}

		public int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer)
		{
			return _builder.LastIndexOf(item, startIndex, count, equalityComparer);
		}

		public bool Remove(T item)
		{
			return _builder.Remove(item);
		}

		public int RemoveAll(Predicate<T> match)
		{
			return _builder.RemoveAll(match);
		}

		public void RemoveAt(int index)
		{
			_builder.RemoveAt(index);
		}

		public void Reverse()
		{
			_builder.Reverse();
		}

		public void Reverse(int index, int count)
		{
			_builder.Reverse(index, count);
		}

		public void Sort()
		{
			_builder.Sort();
		}

		public void Sort(IComparer<T>? comparer)
		{
			_builder.Sort(comparer);
		}

		public void Sort(Comparison<T> comparison)
		{
			_builder.Sort(comparison);
		}

		public void Sort(int index, int count, IComparer<T>? comparer)
		{
			_builder.Sort(index, count, comparer);
		}

		public ImmutableSegmentedList<T> ToImmutable()
		{
			return _builder.ToImmutable();
		}

		public bool TrueForAll(Predicate<T> match)
		{
			return _builder.TrueForAll(match);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return IEnumerableCalls<T>.GetEnumerator(ref _builder);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return IEnumerableCalls.GetEnumerator(ref _builder);
		}

		int IList.Add(object? value)
		{
			return IListCalls.Add(ref _builder, value);
		}

		bool IList.Contains(object? value)
		{
			return IListCalls.Contains(ref _builder, value);
		}

		int IList.IndexOf(object? value)
		{
			return IListCalls.IndexOf(ref _builder, value);
		}

		void IList.Insert(int index, object? value)
		{
			IListCalls.Insert(ref _builder, index, value);
		}

		void IList.Remove(object? value)
		{
			IListCalls.Remove(ref _builder, value);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			ICollectionCalls.CopyTo(ref _builder, array, index);
		}
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly SegmentedList<T> _list;

		private SegmentedList<T>.Enumerator _enumerator;

		public readonly T Current => _enumerator.Current;

		readonly object? IEnumerator.Current => ((IEnumerator)_enumerator).Current;

		internal Enumerator(SegmentedList<T> list)
		{
			_list = list;
			_enumerator = list.GetEnumerator();
		}

		public readonly void Dispose()
		{
			_enumerator.Dispose();
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator = _list.GetEnumerator();
		}
	}

	internal static class PrivateMarshal
	{
		internal static ImmutableSegmentedList<T> VolatileRead(in ImmutableSegmentedList<T> location)
		{
			SegmentedList<T> segmentedList = Volatile.Read(in Unsafe.AsRef(in location._list));
			if (segmentedList == null)
			{
				return default(ImmutableSegmentedList<T>);
			}
			return new ImmutableSegmentedList<T>(segmentedList);
		}

		internal static ImmutableSegmentedList<T> InterlockedExchange(ref ImmutableSegmentedList<T> location, ImmutableSegmentedList<T> value)
		{
			SegmentedList<T> segmentedList = Interlocked.Exchange(ref Unsafe.AsRef(in location._list), value._list);
			if (segmentedList == null)
			{
				return default(ImmutableSegmentedList<T>);
			}
			return new ImmutableSegmentedList<T>(segmentedList);
		}

		internal static ImmutableSegmentedList<T> InterlockedCompareExchange(ref ImmutableSegmentedList<T> location, ImmutableSegmentedList<T> value, ImmutableSegmentedList<T> comparand)
		{
			SegmentedList<T> segmentedList = Interlocked.CompareExchange(ref Unsafe.AsRef(in location._list), value._list, comparand._list);
			if (segmentedList == null)
			{
				return default(ImmutableSegmentedList<T>);
			}
			return new ImmutableSegmentedList<T>(segmentedList);
		}

		internal static ImmutableSegmentedList<T> AsImmutableSegmentedList(SegmentedList<T>? list)
		{
			if (list == null)
			{
				return default(ImmutableSegmentedList<T>);
			}
			return new ImmutableSegmentedList<T>(list);
		}

		internal static SegmentedList<T>? AsSegmentedList(ImmutableSegmentedList<T> list)
		{
			return list._list;
		}
	}

	private struct ValueBuilder : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IList, ICollection
	{
		private ImmutableSegmentedList<T> _list;

		private SegmentedList<T>? _mutableList;

		public readonly int Count => ReadOnlyList.Count;

		private readonly SegmentedList<T> ReadOnlyList => _mutableList ?? _list._list;

		readonly bool ICollection<T>.IsReadOnly => false;

		readonly bool IList.IsFixedSize => false;

		readonly bool IList.IsReadOnly => false;

		readonly bool ICollection.IsSynchronized => false;

		readonly object ICollection.SyncRoot
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public T this[int index]
		{
			readonly get
			{
				return ReadOnlyList[index];
			}
			set
			{
				GetOrCreateMutableList()[index] = value;
			}
		}

		object? IList.this[int index]
		{
			readonly get
			{
				return ((IList)ReadOnlyList)[index];
			}
			set
			{
				((IList)GetOrCreateMutableList())[index] = value;
			}
		}

		internal ValueBuilder(ImmutableSegmentedList<T> list)
		{
			_list = list;
			_mutableList = null;
		}

		public readonly ref readonly T ItemRef(int index)
		{
			if ((uint)index >= (uint)ReadOnlyList.Count)
			{
				ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
			}
			return ref ReadOnlyList._items[index];
		}

		private SegmentedList<T> GetOrCreateMutableList()
		{
			if (_mutableList == null)
			{
				ImmutableSegmentedList<T> immutableSegmentedList = RoslynImmutableInterlocked.InterlockedExchange(ref _list, default(ImmutableSegmentedList<T>));
				if (immutableSegmentedList.IsDefault)
				{
					throw new InvalidOperationException($"Unexpected concurrent access to {GetType()}");
				}
				_mutableList = new SegmentedList<T>(immutableSegmentedList._list);
			}
			return _mutableList;
		}

		public void Add(T item)
		{
			GetOrCreateMutableList().Add(item);
		}

		public void AddRange(IEnumerable<T> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			GetOrCreateMutableList().AddRange(items);
		}

		public readonly int BinarySearch(T item)
		{
			return ReadOnlyList.BinarySearch(item);
		}

		public readonly int BinarySearch(T item, IComparer<T>? comparer)
		{
			return ReadOnlyList.BinarySearch(item, comparer);
		}

		public readonly int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
		{
			return ReadOnlyList.BinarySearch(index, count, item, comparer);
		}

		public void Clear()
		{
			if (ReadOnlyList.Count != 0)
			{
				if (_mutableList == null)
				{
					_mutableList = new SegmentedList<T>();
					_list = default(ImmutableSegmentedList<T>);
				}
				else
				{
					_mutableList.Clear();
				}
			}
		}

		public readonly bool Contains(T item)
		{
			return ReadOnlyList.Contains(item);
		}

		public readonly ImmutableSegmentedList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		{
			return new ImmutableSegmentedList<TOutput>(ReadOnlyList.ConvertAll(converter));
		}

		public readonly void CopyTo(T[] array)
		{
			ReadOnlyList.CopyTo(array);
		}

		public readonly void CopyTo(T[] array, int arrayIndex)
		{
			ReadOnlyList.CopyTo(array, arrayIndex);
		}

		public readonly void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			ReadOnlyList.CopyTo(index, array, arrayIndex, count);
		}

		public readonly bool Exists(Predicate<T> match)
		{
			return ReadOnlyList.Exists(match);
		}

		public readonly T? Find(Predicate<T> match)
		{
			return ReadOnlyList.Find(match);
		}

		public readonly ImmutableSegmentedList<T> FindAll(Predicate<T> match)
		{
			return new ImmutableSegmentedList<T>(ReadOnlyList.FindAll(match));
		}

		public readonly int FindIndex(Predicate<T> match)
		{
			return ReadOnlyList.FindIndex(match);
		}

		public readonly int FindIndex(int startIndex, Predicate<T> match)
		{
			return ReadOnlyList.FindIndex(startIndex, match);
		}

		public readonly int FindIndex(int startIndex, int count, Predicate<T> match)
		{
			return ReadOnlyList.FindIndex(startIndex, count, match);
		}

		public readonly T? FindLast(Predicate<T> match)
		{
			return ReadOnlyList.FindLast(match);
		}

		public readonly int FindLastIndex(Predicate<T> match)
		{
			return ReadOnlyList.FindLastIndex(match);
		}

		public readonly int FindLastIndex(int startIndex, Predicate<T> match)
		{
			if (startIndex == 0 && Count == 0)
			{
				return -1;
			}
			return ReadOnlyList.FindLastIndex(startIndex, match);
		}

		public readonly int FindLastIndex(int startIndex, int count, Predicate<T> match)
		{
			if (count == 0 && startIndex == 0 && Count == 0)
			{
				return -1;
			}
			return ReadOnlyList.FindLastIndex(startIndex, count, match);
		}

		public readonly void ForEach(Action<T> action)
		{
			ReadOnlyList.ForEach(action);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(GetOrCreateMutableList());
		}

		public ImmutableSegmentedList<T> GetRange(int index, int count)
		{
			if (index == 0 && count == Count)
			{
				return ToImmutable();
			}
			return new ImmutableSegmentedList<T>(ReadOnlyList.GetRange(index, count));
		}

		public readonly int IndexOf(T item)
		{
			return ReadOnlyList.IndexOf(item);
		}

		public readonly int IndexOf(T item, int index)
		{
			return ReadOnlyList.IndexOf(item, index);
		}

		public readonly int IndexOf(T item, int index, int count)
		{
			return ReadOnlyList.IndexOf(item, index, count);
		}

		public readonly int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
		{
			return ReadOnlyList.IndexOf(item, index, count, equalityComparer);
		}

		public void Insert(int index, T item)
		{
			GetOrCreateMutableList().Insert(index, item);
		}

		public void InsertRange(int index, IEnumerable<T> items)
		{
			GetOrCreateMutableList().InsertRange(index, items);
		}

		public readonly int LastIndexOf(T item)
		{
			return ReadOnlyList.LastIndexOf(item);
		}

		public readonly int LastIndexOf(T item, int startIndex)
		{
			if (startIndex == 0 && Count == 0)
			{
				return -1;
			}
			return ReadOnlyList.LastIndexOf(item, startIndex);
		}

		public readonly int LastIndexOf(T item, int startIndex, int count)
		{
			if (count == 0 && startIndex == 0 && Count == 0)
			{
				return -1;
			}
			return ReadOnlyList.LastIndexOf(item, startIndex, count);
		}

		public readonly int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer)
		{
			if (startIndex < 0)
			{
				ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
			}
			if (count < 0 || count > Count)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);
			}
			if (startIndex - count + 1 < 0)
			{
				throw new ArgumentException();
			}
			return ReadOnlyList.LastIndexOf(item, startIndex, count, equalityComparer);
		}

		public bool Remove(T item)
		{
			if (_mutableList == null)
			{
				int num = IndexOf(item);
				if (num < 0)
				{
					return false;
				}
				RemoveAt(num);
				return true;
			}
			return _mutableList.Remove(item);
		}

		public int RemoveAll(Predicate<T> match)
		{
			return GetOrCreateMutableList().RemoveAll(match);
		}

		public void RemoveAt(int index)
		{
			GetOrCreateMutableList().RemoveAt(index);
		}

		public void RemoveRange(int index, int count)
		{
			GetOrCreateMutableList().RemoveRange(index, count);
		}

		public void Reverse()
		{
			if (Count >= 2)
			{
				GetOrCreateMutableList().Reverse();
			}
		}

		public void Reverse(int index, int count)
		{
			GetOrCreateMutableList().Reverse(index, count);
		}

		public void Sort()
		{
			if (Count >= 2)
			{
				GetOrCreateMutableList().Sort();
			}
		}

		public void Sort(IComparer<T>? comparer)
		{
			if (Count >= 2)
			{
				GetOrCreateMutableList().Sort(comparer);
			}
		}

		public void Sort(Comparison<T> comparison)
		{
			if (comparison == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
			}
			if (Count >= 2)
			{
				GetOrCreateMutableList().Sort(comparison);
			}
		}

		public void Sort(int index, int count, IComparer<T>? comparer)
		{
			GetOrCreateMutableList().Sort(index, count, comparer);
		}

		public ImmutableSegmentedList<T> ToImmutable()
		{
			_list = new ImmutableSegmentedList<T>(ReadOnlyList);
			_mutableList = null;
			return _list;
		}

		public readonly bool TrueForAll(Predicate<T> match)
		{
			return ReadOnlyList.TrueForAll(match);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		int IList.Add(object? value)
		{
			return ((IList)GetOrCreateMutableList()).Add(value);
		}

		readonly bool IList.Contains(object? value)
		{
			return ((IList)ReadOnlyList).Contains(value);
		}

		readonly int IList.IndexOf(object? value)
		{
			return ((IList)ReadOnlyList).IndexOf(value);
		}

		void IList.Insert(int index, object? value)
		{
			((IList)GetOrCreateMutableList()).Insert(index, value);
		}

		void IList.Remove(object? value)
		{
			((IList)GetOrCreateMutableList()).Remove(value);
		}

		readonly void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)ReadOnlyList).CopyTo(array, index);
		}
	}

	public static readonly ImmutableSegmentedList<T> Empty = new ImmutableSegmentedList<T>(new SegmentedList<T>());

	private readonly SegmentedList<T> _list;

	public int Count => _list.Count;

	public bool IsDefault => _list == null;

	public bool IsEmpty => _list.Count == 0;

	bool ICollection<T>.IsReadOnly => true;

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	bool ICollection.IsSynchronized => true;

	object ICollection.SyncRoot => _list;

	public T this[int index] => _list[index];

	T IList<T>.this[int index]
	{
		get
		{
			return _list[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	object? IList.this[int index]
	{
		get
		{
			return _list[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	private ImmutableSegmentedList(SegmentedList<T> list)
	{
		_list = list;
	}

	public static bool operator ==(ImmutableSegmentedList<T> left, ImmutableSegmentedList<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ImmutableSegmentedList<T> left, ImmutableSegmentedList<T> right)
	{
		return !left.Equals(right);
	}

	public static bool operator ==(ImmutableSegmentedList<T>? left, ImmutableSegmentedList<T>? right)
	{
		return left.GetValueOrDefault().Equals(right.GetValueOrDefault());
	}

	public static bool operator !=(ImmutableSegmentedList<T>? left, ImmutableSegmentedList<T>? right)
	{
		return !left.GetValueOrDefault().Equals(right.GetValueOrDefault());
	}

	public ref readonly T ItemRef(int index)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		if ((uint)index >= (uint)immutableSegmentedList.Count)
		{
			ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
		}
		return ref immutableSegmentedList._list._items[index];
	}

	public ImmutableSegmentedList<T> Add(T value)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		if (immutableSegmentedList.IsEmpty)
		{
			return new ImmutableSegmentedList<T>(new SegmentedList<T> { value });
		}
		ValueBuilder valueBuilder = immutableSegmentedList.ToValueBuilder();
		valueBuilder.Add(value);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> AddRange(IEnumerable<T> items)
	{
		ImmutableSegmentedList<T> result = this;
		if (items is ICollection<T> { Count: 0 })
		{
			return result;
		}
		if (result.IsEmpty)
		{
			if (items is ImmutableSegmentedList<T>)
			{
				return (ImmutableSegmentedList<T>)(object)items;
			}
			if (items is Builder builder)
			{
				return builder.ToImmutable();
			}
			return new ImmutableSegmentedList<T>(new SegmentedList<T>(items));
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.AddRange(items);
		return valueBuilder.ToImmutable();
	}

	public int BinarySearch(T item)
	{
		return _list.BinarySearch(item);
	}

	public int BinarySearch(T item, IComparer<T>? comparer)
	{
		return _list.BinarySearch(item, comparer);
	}

	public int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
	{
		return _list.BinarySearch(index, count, item, comparer);
	}

	public ImmutableSegmentedList<T> Clear()
	{
		return Empty;
	}

	public bool Contains(T value)
	{
		return _list.Contains(value);
	}

	public ImmutableSegmentedList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
	{
		return new ImmutableSegmentedList<TOutput>(_list.ConvertAll(converter));
	}

	public void CopyTo(T[] array)
	{
		_list.CopyTo(array);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_list.CopyTo(array, arrayIndex);
	}

	public void CopyTo(int index, T[] array, int arrayIndex, int count)
	{
		_list.CopyTo(index, array, arrayIndex, count);
	}

	public bool Exists(Predicate<T> match)
	{
		return _list.Exists(match);
	}

	public T? Find(Predicate<T> match)
	{
		return _list.Find(match);
	}

	public ImmutableSegmentedList<T> FindAll(Predicate<T> match)
	{
		return new ImmutableSegmentedList<T>(_list.FindAll(match));
	}

	public int FindIndex(Predicate<T> match)
	{
		return _list.FindIndex(match);
	}

	public int FindIndex(int startIndex, Predicate<T> match)
	{
		return _list.FindIndex(startIndex, match);
	}

	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		return _list.FindIndex(startIndex, count, match);
	}

	public T? FindLast(Predicate<T> match)
	{
		return _list.FindLast(match);
	}

	public int FindLastIndex(Predicate<T> match)
	{
		return _list.FindLastIndex(match);
	}

	public int FindLastIndex(int startIndex, Predicate<T> match)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		if (startIndex == 0 && immutableSegmentedList.IsEmpty)
		{
			return -1;
		}
		return immutableSegmentedList._list.FindLastIndex(startIndex, match);
	}

	public int FindLastIndex(int startIndex, int count, Predicate<T> match)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		if (count == 0 && startIndex == 0 && immutableSegmentedList.IsEmpty)
		{
			return -1;
		}
		return immutableSegmentedList._list.FindLastIndex(startIndex, count, match);
	}

	public void ForEach(Action<T> action)
	{
		_list.ForEach(action);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_list);
	}

	public ImmutableSegmentedList<T> GetRange(int index, int count)
	{
		ImmutableSegmentedList<T> result = this;
		if (index == 0 && count == result.Count)
		{
			return result;
		}
		return new ImmutableSegmentedList<T>(result._list.GetRange(index, count));
	}

	public int IndexOf(T value)
	{
		return _list.IndexOf(value);
	}

	public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
	{
		return _list.IndexOf(item, index, count, equalityComparer);
	}

	public ImmutableSegmentedList<T> Insert(int index, T item)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		if (index == immutableSegmentedList.Count)
		{
			return immutableSegmentedList.Add(item);
		}
		ValueBuilder valueBuilder = immutableSegmentedList.ToValueBuilder();
		valueBuilder.Insert(index, item);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> InsertRange(int index, IEnumerable<T> items)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		if (index == immutableSegmentedList.Count)
		{
			return immutableSegmentedList.AddRange(items);
		}
		ValueBuilder valueBuilder = immutableSegmentedList.ToValueBuilder();
		valueBuilder.InsertRange(index, items);
		return valueBuilder.ToImmutable();
	}

	public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (count < 0 || count > immutableSegmentedList.Count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);
		}
		if (index - count + 1 < 0)
		{
			throw new ArgumentException();
		}
		if (count == 0 && index == 0 && immutableSegmentedList.IsEmpty)
		{
			return -1;
		}
		return immutableSegmentedList._list.LastIndexOf(item, index, count, equalityComparer);
	}

	public ImmutableSegmentedList<T> Remove(T value)
	{
		ImmutableSegmentedList<T> result = this;
		int num = result.IndexOf(value);
		if (num < 0)
		{
			return result;
		}
		return result.RemoveAt(num);
	}

	public ImmutableSegmentedList<T> Remove(T value, IEqualityComparer<T>? equalityComparer)
	{
		ImmutableSegmentedList<T> result = this;
		int num = result.IndexOf(value, 0, Count, equalityComparer);
		if (num < 0)
		{
			return result;
		}
		return result.RemoveAt(num);
	}

	public ImmutableSegmentedList<T> RemoveAll(Predicate<T> match)
	{
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder.RemoveAll(match);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> RemoveAt(int index)
	{
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder.RemoveAt(index);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> RemoveRange(IEnumerable<T> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		ImmutableSegmentedList<T> result = this;
		if (result.IsEmpty)
		{
			return result;
		}
		ValueBuilder valueBuilder = ToValueBuilder();
		foreach (T item in items)
		{
			int num = valueBuilder.IndexOf(item);
			if (num >= 0)
			{
				valueBuilder.RemoveAt(num);
			}
		}
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		ImmutableSegmentedList<T> result = this;
		if (result.IsEmpty)
		{
			return result;
		}
		ValueBuilder valueBuilder = ToValueBuilder();
		foreach (T item in items)
		{
			int num = valueBuilder.IndexOf(item, 0, valueBuilder.Count, equalityComparer);
			if (num >= 0)
			{
				valueBuilder.RemoveAt(num);
			}
		}
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> RemoveRange(int index, int count)
	{
		ImmutableSegmentedList<T> result = this;
		if (count == 0 && index >= 0 && index <= result.Count)
		{
			return result;
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.RemoveRange(index, count);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> Replace(T oldValue, T newValue)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		int num = immutableSegmentedList.IndexOf(oldValue);
		if (num < 0)
		{
			throw new ArgumentException("Cannot find the old value", "oldValue");
		}
		return immutableSegmentedList.SetItem(num, newValue);
	}

	public ImmutableSegmentedList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
	{
		ImmutableSegmentedList<T> immutableSegmentedList = this;
		int num = immutableSegmentedList.IndexOf(oldValue, equalityComparer);
		if (num < 0)
		{
			throw new ArgumentException("Cannot find the old value", "oldValue");
		}
		return immutableSegmentedList.SetItem(num, newValue);
	}

	public ImmutableSegmentedList<T> Reverse()
	{
		ImmutableSegmentedList<T> result = this;
		if (result.Count < 2)
		{
			return result;
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.Reverse();
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> Reverse(int index, int count)
	{
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder.Reverse(index, count);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> SetItem(int index, T value)
	{
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder[index] = value;
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> Sort()
	{
		ImmutableSegmentedList<T> result = this;
		if (result.Count < 2)
		{
			return result;
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.Sort();
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> Sort(IComparer<T>? comparer)
	{
		ImmutableSegmentedList<T> result = this;
		if (result.Count < 2)
		{
			return result;
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.Sort(comparer);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> Sort(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
		}
		ImmutableSegmentedList<T> result = this;
		if (result.Count < 2)
		{
			return result;
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.Sort(comparison);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedList<T> Sort(int index, int count, IComparer<T>? comparer)
	{
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder.Sort(index, count, comparer);
		return valueBuilder.ToImmutable();
	}

	public Builder ToBuilder()
	{
		return new Builder(this);
	}

	private ValueBuilder ToValueBuilder()
	{
		return new ValueBuilder(this);
	}

	public override int GetHashCode()
	{
		return _list?.GetHashCode() ?? 0;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ImmutableSegmentedList<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(ImmutableSegmentedList<T> other)
	{
		return _list == other._list;
	}

	public bool TrueForAll(Predicate<T> match)
	{
		return _list.TrueForAll(match);
	}

	IImmutableList<T> IImmutableList<T>.Clear()
	{
		return Clear();
	}

	IImmutableList<T> IImmutableList<T>.Add(T value)
	{
		return Add(value);
	}

	IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items)
	{
		return AddRange(items);
	}

	IImmutableList<T> IImmutableList<T>.Insert(int index, T element)
	{
		return Insert(index, element);
	}

	IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items)
	{
		return InsertRange(index, items);
	}

	IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T>? equalityComparer)
	{
		return Remove(value, equalityComparer);
	}

	IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match)
	{
		return RemoveAll(match);
	}

	IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
	{
		return RemoveRange(items, equalityComparer);
	}

	IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count)
	{
		return RemoveRange(index, count);
	}

	IImmutableList<T> IImmutableList<T>.RemoveAt(int index)
	{
		return RemoveAt(index);
	}

	IImmutableList<T> IImmutableList<T>.SetItem(int index, T value)
	{
		return SetItem(index, value);
	}

	IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
	{
		return Replace(oldValue, newValue, equalityComparer);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (!IsEmpty)
		{
			return GetEnumerator();
		}
		return Enumerable.Empty<T>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	int IList.Add(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object? value)
	{
		return ((IList)_list).Contains(value);
	}

	int IList.IndexOf(object? value)
	{
		return ((IList)_list).IndexOf(value);
	}

	void IList.Insert(int index, object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Remove(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_list).CopyTo(array, index);
	}
}
