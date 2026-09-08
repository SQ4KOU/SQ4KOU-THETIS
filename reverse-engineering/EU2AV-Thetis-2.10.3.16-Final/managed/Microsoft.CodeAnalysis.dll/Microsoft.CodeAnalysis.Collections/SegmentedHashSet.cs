using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.Collections.Internal;

namespace Microsoft.CodeAnalysis.Collections;

[DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
internal class SegmentedHashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ISet<T>, IReadOnlyCollection<T>
{
	private struct Entry
	{
		public int _hashCode;

		public int _next;

		public T _value;
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly SegmentedHashSet<T> _hashSet;

		private readonly int _version;

		private int _index;

		private T _current;

		public readonly T Current => _current;

		readonly object? IEnumerator.Current
		{
			get
			{
				if (_index == 0 || _index == _hashSet._count + 1)
				{
					ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
				}
				return _current;
			}
		}

		internal Enumerator(SegmentedHashSet<T> hashSet)
		{
			_hashSet = hashSet;
			_version = hashSet._version;
			_index = 0;
			_current = default(T);
		}

		public bool MoveNext()
		{
			if (_version != _hashSet._version)
			{
				ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
			}
			while ((uint)_index < (uint)_hashSet._count)
			{
				ref Entry reference = ref _hashSet._entries[_index++];
				if (reference._next >= -1)
				{
					_current = reference._value;
					return true;
				}
			}
			_index = _hashSet._count + 1;
			_current = default(T);
			return false;
		}

		public readonly void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			if (_version != _hashSet._version)
			{
				ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
			}
			_index = 0;
			_current = default(T);
		}
	}

	private const bool SupportsComparerDevirtualization = false;

	private const int StackAllocThreshold = 100;

	private const int ShrinkThreshold = 3;

	private const int StartOfFreeList = -3;

	private static IEnumerator<T>? s_emptyEnumerator;

	private SegmentedArray<int> _buckets;

	private SegmentedArray<Entry> _entries;

	private ulong _fastModMultiplier;

	private int _count;

	private int _freeList;

	private int _freeCount;

	private int _version;

	private readonly IEqualityComparer<T> _comparer;

	public int Count => _count - _freeCount;

	bool ICollection<T>.IsReadOnly => false;

	public IEqualityComparer<T> Comparer => _comparer ?? EqualityComparer<T>.Default;

	public SegmentedHashSet()
		: this((IEqualityComparer<T>?)null)
	{
	}

	public SegmentedHashSet(IEqualityComparer<T>? comparer)
	{
		if (!typeof(T).IsValueType)
		{
			_comparer = comparer ?? EqualityComparer<T>.Default;
		}
		else if (comparer != null && comparer != EqualityComparer<T>.Default)
		{
			_comparer = comparer;
		}
		if (_comparer == null)
		{
			_comparer = EqualityComparer<T>.Default;
		}
	}

	public SegmentedHashSet(int capacity)
		: this(capacity, (IEqualityComparer<T>?)null)
	{
	}

	public SegmentedHashSet(IEnumerable<T> collection)
		: this(collection, (IEqualityComparer<T>?)null)
	{
	}

	public SegmentedHashSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer)
		: this(comparer)
	{
		if (collection == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
		}
		if (collection is SegmentedHashSet<T> segmentedHashSet && EqualityComparersAreEqual(this, segmentedHashSet))
		{
			ConstructFrom(segmentedHashSet);
			return;
		}
		if (collection is ICollection<T> { Count: var count } && count > 0)
		{
			Initialize(count);
		}
		UnionWith(collection);
		if (_count > 0 && _entries.Length / _count > 3)
		{
			TrimExcess();
		}
	}

	public SegmentedHashSet(int capacity, IEqualityComparer<T>? comparer)
		: this(comparer)
	{
		if (capacity < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
		}
		if (capacity > 0)
		{
			Initialize(capacity);
		}
	}

	private void ConstructFrom(SegmentedHashSet<T> source)
	{
		if (source.Count == 0)
		{
			return;
		}
		int length = source._buckets.Length;
		if (HashHelpers.ExpandPrime(source.Count + 1) >= length)
		{
			_buckets = (SegmentedArray<int>)source._buckets.Clone();
			_entries = (SegmentedArray<Entry>)source._entries.Clone();
			_freeList = source._freeList;
			_freeCount = source._freeCount;
			_count = source._count;
			_fastModMultiplier = source._fastModMultiplier;
			return;
		}
		Initialize(source.Count);
		SegmentedArray<Entry> entries = source._entries;
		for (int i = 0; i < source._count; i++)
		{
			ref Entry reference = ref entries[i];
			if (reference._next >= -1)
			{
				AddIfNotPresent(reference._value, out var _);
			}
		}
	}

	void ICollection<T>.Add(T item)
	{
		AddIfNotPresent(item, out var _);
	}

	public void Clear()
	{
		int count = _count;
		if (count > 0)
		{
			SegmentedArray.Clear(_buckets);
			_count = 0;
			_freeList = -1;
			_freeCount = 0;
			SegmentedArray.Clear(_entries, 0, count);
		}
	}

	public bool Contains(T item)
	{
		return FindItemIndex(item) >= 0;
	}

	private int FindItemIndex(T item)
	{
		SegmentedArray<int> buckets = _buckets;
		if (buckets.Length > 0)
		{
			SegmentedArray<Entry> entries = _entries;
			uint num = 0u;
			IEqualityComparer<T> comparer = _comparer;
			int num2 = ((item != null) ? comparer.GetHashCode(item) : 0);
			int num3 = GetBucketRef(num2) - 1;
			while (num3 >= 0)
			{
				ref Entry reference = ref entries[num3];
				if (reference._hashCode == num2 && comparer.Equals(reference._value, item))
				{
					return num3;
				}
				num3 = reference._next;
				num++;
				if (num > (uint)entries.Length)
				{
					ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
				}
			}
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ref int GetBucketRef(int hashCode)
	{
		SegmentedArray<int> buckets = _buckets;
		return ref buckets[(int)HashHelpers.FastMod((uint)hashCode, (uint)buckets.Length, _fastModMultiplier)];
	}

	public bool Remove(T item)
	{
		if (_buckets.Length > 0)
		{
			SegmentedArray<Entry> entries = _entries;
			uint num = 0u;
			int num2 = -1;
			IEqualityComparer<T> comparer = _comparer;
			int num3 = ((item != null) ? comparer.GetHashCode(item) : 0);
			ref int bucketRef = ref GetBucketRef(num3);
			int num4 = bucketRef - 1;
			while (num4 >= 0)
			{
				ref Entry reference = ref entries[num4];
				if (reference._hashCode == num3 && (comparer?.Equals(reference._value, item) ?? EqualityComparer<T>.Default.Equals(reference._value, item)))
				{
					if (num2 < 0)
					{
						bucketRef = reference._next + 1;
					}
					else
					{
						entries[num2]._next = reference._next;
					}
					reference._next = -3 - _freeList;
					reference._value = default(T);
					_freeList = num4;
					_freeCount++;
					return true;
				}
				num2 = num4;
				num4 = reference._next;
				num++;
				if (num > (uint)entries.Length)
				{
					ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
				}
			}
		}
		return false;
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
		return LazyInitializer.EnsureInitialized(ref s_emptyEnumerator, () => new Enumerator(new SegmentedHashSet<T>()));
	}

	public bool Add(T item)
	{
		int location;
		return AddIfNotPresent(item, out location);
	}

	public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
	{
		if (_buckets.Length > 0)
		{
			int num = FindItemIndex(equalValue);
			if (num >= 0)
			{
				actualValue = _entries[num]._value;
				return true;
			}
		}
		actualValue = default(T);
		return false;
	}

	public void UnionWith(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		foreach (T item in other)
		{
			AddIfNotPresent(item, out var _);
		}
	}

	public void IntersectWith(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (Count == 0 || other == this)
		{
			return;
		}
		if (other is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				Clear();
				return;
			}
			if (other is SegmentedHashSet<T> segmentedHashSet && EqualityComparersAreEqual(this, segmentedHashSet))
			{
				IntersectWithHashSetWithSameComparer(segmentedHashSet);
				return;
			}
		}
		IntersectWithEnumerable(other);
	}

	public void ExceptWith(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (Count == 0)
		{
			return;
		}
		if (other == this)
		{
			Clear();
			return;
		}
		foreach (T item in other)
		{
			Remove(item);
		}
	}

	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (Count == 0)
		{
			UnionWith(other);
		}
		else if (other == this)
		{
			Clear();
		}
		else if (other is SegmentedHashSet<T> segmentedHashSet && EqualityComparersAreEqual(this, segmentedHashSet))
		{
			SymmetricExceptWithUniqueHashSet(segmentedHashSet);
		}
		else
		{
			SymmetricExceptWithEnumerable(other);
		}
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (Count == 0 || other == this)
		{
			return true;
		}
		if (other is SegmentedHashSet<T> segmentedHashSet && EqualityComparersAreEqual(this, segmentedHashSet))
		{
			if (Count > segmentedHashSet.Count)
			{
				return false;
			}
			return IsSubsetOfHashSetWithSameComparer(segmentedHashSet);
		}
		var (num, num2) = CheckUniqueAndUnfoundElements(other, returnIfUnfound: false);
		if (num == Count)
		{
			return num2 >= 0;
		}
		return false;
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (other == this)
		{
			return false;
		}
		if (other is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				return false;
			}
			if (Count == 0)
			{
				return collection.Count > 0;
			}
			if (other is SegmentedHashSet<T> segmentedHashSet && EqualityComparersAreEqual(this, segmentedHashSet))
			{
				if (Count >= segmentedHashSet.Count)
				{
					return false;
				}
				return IsSubsetOfHashSetWithSameComparer(segmentedHashSet);
			}
		}
		var (num, num2) = CheckUniqueAndUnfoundElements(other, returnIfUnfound: false);
		if (num == Count)
		{
			return num2 > 0;
		}
		return false;
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (other == this)
		{
			return true;
		}
		if (other is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				return true;
			}
			if (other is SegmentedHashSet<T> segmentedHashSet && EqualityComparersAreEqual(this, segmentedHashSet) && segmentedHashSet.Count > Count)
			{
				return false;
			}
		}
		foreach (T item in other)
		{
			if (!Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (Count == 0 || other == this)
		{
			return false;
		}
		if (other is ICollection<T> collection)
		{
			if (collection.Count == 0)
			{
				return true;
			}
			if (other is SegmentedHashSet<T> segmentedHashSet && EqualityComparersAreEqual(this, segmentedHashSet))
			{
				if (segmentedHashSet.Count >= Count)
				{
					return false;
				}
				return segmentedHashSet.IsSubsetOfHashSetWithSameComparer(this);
			}
		}
		var (num, num2) = CheckUniqueAndUnfoundElements(other, returnIfUnfound: true);
		if (num < Count)
		{
			return num2 == 0;
		}
		return false;
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (Count == 0)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		foreach (T item in other)
		{
			if (Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		if (other == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
		}
		if (other == this)
		{
			return true;
		}
		if (other is SegmentedHashSet<T> segmentedHashSet && EqualityComparersAreEqual(this, segmentedHashSet))
		{
			if (Count != segmentedHashSet.Count)
			{
				return false;
			}
			return IsSubsetOfHashSetWithSameComparer(segmentedHashSet);
		}
		if (Count == 0 && other is ICollection<T> { Count: >0 })
		{
			return false;
		}
		var (num, num2) = CheckUniqueAndUnfoundElements(other, returnIfUnfound: true);
		if (num == Count)
		{
			return num2 == 0;
		}
		return false;
	}

	public void CopyTo(T[] array)
	{
		CopyTo(array, 0, Count);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		CopyTo(array, arrayIndex, Count);
	}

	public void CopyTo(T[] array, int arrayIndex, int count)
	{
		if (array == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "Non-negative number required.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", count, "Non-negative number required.");
		}
		if (arrayIndex > array.Length || count > array.Length - arrayIndex)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		SegmentedArray<Entry> entries = _entries;
		for (int i = 0; i < _count; i++)
		{
			if (count == 0)
			{
				break;
			}
			ref Entry reference = ref entries[i];
			if (reference._next >= -1)
			{
				array[arrayIndex++] = reference._value;
				count--;
			}
		}
	}

	public int RemoveWhere(Predicate<T> match)
	{
		if (match == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
		}
		SegmentedArray<Entry> entries = _entries;
		int num = 0;
		for (int i = 0; i < _count; i++)
		{
			ref Entry reference = ref entries[i];
			if (reference._next >= -1)
			{
				T value = reference._value;
				if (match(value) && Remove(value))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int EnsureCapacity(int capacity)
	{
		if (capacity < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
		}
		int length = _entries.Length;
		if (length >= capacity)
		{
			return length;
		}
		if (_buckets.Length == 0)
		{
			return Initialize(capacity);
		}
		int prime = HashHelpers.GetPrime(capacity);
		Resize(prime);
		return prime;
	}

	private void Resize()
	{
		Resize(HashHelpers.ExpandPrime(_count));
	}

	private void Resize(int newSize)
	{
		int count = _count;
		SegmentedArray<Entry> entries = CreateNewSegmentedArrayReusingOldSegments(_entries, newSize);
		_buckets = new SegmentedArray<int>(newSize);
		_fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newSize);
		for (int i = 0; i < count; i++)
		{
			ref Entry reference = ref entries[i];
			if (reference._next >= -1)
			{
				ref int bucketRef = ref GetBucketRef(reference._hashCode);
				reference._next = bucketRef - 1;
				bucketRef = i + 1;
			}
		}
		_entries = entries;
	}

	private static SegmentedArray<Entry> CreateNewSegmentedArrayReusingOldSegments(SegmentedArray<Entry> oldArray, int newSize)
	{
		Entry[][] array = SegmentedCollectionsMarshal.AsSegments(oldArray);
		int num = array.Length;
		int num2 = newSize + SegmentedArrayHelper.GetSegmentSize<Entry>() - 1 >> SegmentedArrayHelper.GetSegmentShift<Entry>();
		Array.Resize(ref array, num2);
		for (int i = num - 1; i < num2 - 1; i++)
		{
			Array.Resize(ref array[i], SegmentedArrayHelper.GetSegmentSize<Entry>());
		}
		int newSize2 = newSize - (num2 - 1 << SegmentedArrayHelper.GetSegmentShift<Entry>());
		Array.Resize(ref array[num2 - 1], newSize2);
		return SegmentedCollectionsMarshal.AsSegmentedArray(newSize, array);
	}

	public void TrimExcess()
	{
		int count = Count;
		int prime = HashHelpers.GetPrime(count);
		SegmentedArray<Entry> entries = _entries;
		int length = entries.Length;
		if (prime >= length)
		{
			return;
		}
		int count2 = _count;
		_version++;
		Initialize(prime);
		SegmentedArray<Entry> entries2 = _entries;
		int num = 0;
		for (int i = 0; i < count2; i++)
		{
			int hashCode = entries[i]._hashCode;
			if (entries[i]._next >= -1)
			{
				ref Entry reference = ref entries2[num];
				reference = entries[i];
				ref int bucketRef = ref GetBucketRef(hashCode);
				reference._next = bucketRef - 1;
				bucketRef = num + 1;
				num++;
			}
		}
		_count = count;
		_freeCount = 0;
	}

	public static IEqualityComparer<SegmentedHashSet<T>> CreateSetComparer()
	{
		return new SegmentedHashSetEqualityComparer<T>();
	}

	private int Initialize(int capacity)
	{
		int prime = HashHelpers.GetPrime(capacity);
		SegmentedArray<int> buckets = new SegmentedArray<int>(prime);
		SegmentedArray<Entry> entries = new SegmentedArray<Entry>(prime);
		_freeList = -1;
		_buckets = buckets;
		_entries = entries;
		_fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)prime);
		return prime;
	}

	private bool AddIfNotPresent(T value, out int location)
	{
		if (_buckets.Length == 0)
		{
			Initialize(0);
		}
		SegmentedArray<Entry> entries = _entries;
		IEqualityComparer<T> comparer = _comparer;
		uint num = 0u;
		ref int reference = ref RoslynUnsafe.NullRef<int>();
		int num2 = ((value != null) ? comparer.GetHashCode(value) : 0);
		reference = ref GetBucketRef(num2);
		int num3 = reference - 1;
		while (num3 >= 0)
		{
			ref Entry reference2 = ref entries[num3];
			if (reference2._hashCode == num2 && comparer.Equals(reference2._value, value))
			{
				location = num3;
				return false;
			}
			num3 = reference2._next;
			num++;
			if (num > (uint)entries.Length)
			{
				ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
			}
		}
		int num4;
		if (_freeCount > 0)
		{
			num4 = _freeList;
			_freeCount--;
			_freeList = -3 - entries[_freeList]._next;
		}
		else
		{
			int count = _count;
			if (count == entries.Length)
			{
				Resize();
				reference = ref GetBucketRef(num2);
			}
			num4 = count;
			_count = count + 1;
			entries = _entries;
		}
		ref Entry reference3 = ref entries[num4];
		reference3._hashCode = num2;
		reference3._next = reference - 1;
		reference3._value = value;
		reference = num4 + 1;
		_version++;
		location = num4;
		return true;
	}

	internal bool IsSubsetOfHashSetWithSameComparer(SegmentedHashSet<T> other)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				if (!other.Contains(current))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void IntersectWithHashSetWithSameComparer(SegmentedHashSet<T> other)
	{
		SegmentedArray<Entry> entries = _entries;
		for (int i = 0; i < _count; i++)
		{
			ref Entry reference = ref entries[i];
			if (reference._next >= -1)
			{
				T value = reference._value;
				if (!other.Contains(value))
				{
					Remove(value);
				}
			}
		}
	}

	private void IntersectWithEnumerable(IEnumerable<T> other)
	{
		int count = _count;
		int num = BitHelper.ToIntArrayLength(count);
		Span<int> span = stackalloc int[100];
		BitHelper bitHelper = ((num <= 100) ? new BitHelper(span.Slice(0, num), clear: true) : new BitHelper(new int[num], clear: false));
		foreach (T item in other)
		{
			int num2 = FindItemIndex(item);
			if (num2 >= 0)
			{
				bitHelper.MarkBit(num2);
			}
		}
		for (int i = 0; i < count; i++)
		{
			ref Entry reference = ref _entries[i];
			if (reference._next >= -1 && !bitHelper.IsMarked(i))
			{
				Remove(reference._value);
			}
		}
	}

	private void SymmetricExceptWithUniqueHashSet(SegmentedHashSet<T> other)
	{
		foreach (T item in other)
		{
			if (!Remove(item))
			{
				AddIfNotPresent(item, out var _);
			}
		}
	}

	private void SymmetricExceptWithEnumerable(IEnumerable<T> other)
	{
		int count = _count;
		int num = BitHelper.ToIntArrayLength(count);
		Span<int> span = stackalloc int[50];
		BitHelper bitHelper = ((num <= 50) ? new BitHelper(span.Slice(0, num), clear: true) : new BitHelper(new int[num], clear: false));
		Span<int> span2 = stackalloc int[50];
		BitHelper bitHelper2 = ((num <= 50) ? new BitHelper(span2.Slice(0, num), clear: true) : new BitHelper(new int[num], clear: false));
		foreach (T item in other)
		{
			if (AddIfNotPresent(item, out var location))
			{
				bitHelper2.MarkBit(location);
			}
			else if (location < count && !bitHelper2.IsMarked(location))
			{
				bitHelper.MarkBit(location);
			}
		}
		for (int i = 0; i < count; i++)
		{
			if (bitHelper.IsMarked(i))
			{
				Remove(_entries[i]._value);
			}
		}
	}

	private (int UniqueCount, int UnfoundCount) CheckUniqueAndUnfoundElements(IEnumerable<T> other, bool returnIfUnfound)
	{
		if (_count == 0)
		{
			int num = 0;
			using (IEnumerator<T> enumerator = other.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					_ = enumerator.Current;
					num++;
				}
			}
			return (UniqueCount: 0, UnfoundCount: num);
		}
		int num2 = BitHelper.ToIntArrayLength(_count);
		Span<int> span = stackalloc int[100];
		BitHelper bitHelper = ((num2 <= 100) ? new BitHelper(span.Slice(0, num2), clear: true) : new BitHelper(new int[num2], clear: false));
		int num3 = 0;
		int num4 = 0;
		foreach (T item in other)
		{
			int num5 = FindItemIndex(item);
			if (num5 >= 0)
			{
				if (!bitHelper.IsMarked(num5))
				{
					bitHelper.MarkBit(num5);
					num4++;
				}
			}
			else
			{
				num3++;
				if (returnIfUnfound)
				{
					break;
				}
			}
		}
		return (UniqueCount: num4, UnfoundCount: num3);
	}

	internal static bool EqualityComparersAreEqual(SegmentedHashSet<T> set1, SegmentedHashSet<T> set2)
	{
		return set1.Comparer.Equals(set2.Comparer);
	}
}
