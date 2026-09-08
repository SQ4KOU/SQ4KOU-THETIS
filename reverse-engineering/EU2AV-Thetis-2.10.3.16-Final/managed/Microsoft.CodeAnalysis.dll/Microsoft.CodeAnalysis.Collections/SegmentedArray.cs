using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Collections.Internal;

namespace Microsoft.CodeAnalysis.Collections;

internal static class SegmentedArray
{
	private readonly struct AlignedSegmentEnumerable<T>
	{
		private readonly SegmentedArray<T> _first;

		private readonly int _firstOffset;

		private readonly SegmentedArray<T> _second;

		private readonly int _secondOffset;

		private readonly int _length;

		public AlignedSegmentEnumerable(SegmentedArray<T> first, SegmentedArray<T> second, int length)
			: this(first, 0, second, 0, length)
		{
		}

		public AlignedSegmentEnumerable(SegmentedArray<T> first, int firstOffset, SegmentedArray<T> second, int secondOffset, int length)
		{
			_first = first;
			_firstOffset = firstOffset;
			_second = second;
			_secondOffset = secondOffset;
			_length = length;
		}

		public AlignedSegmentEnumerator<T> GetEnumerator()
		{
			return new AlignedSegmentEnumerator<T>(SegmentedCollectionsMarshal.AsSegments(_first), _firstOffset, SegmentedCollectionsMarshal.AsSegments(_second), _secondOffset, _length);
		}
	}

	private struct AlignedSegmentEnumerator<T>(T[][] firstSegments, int firstOffset, T[][] secondSegments, int secondOffset, int length)
	{
		private readonly T[][] _firstSegments = firstSegments;

		private readonly int _firstOffset = firstOffset;

		private readonly T[][] _secondSegments = secondSegments;

		private readonly int _secondOffset = secondOffset;

		private readonly int _length = length;

		private int _completed = 0;

		private (Memory<T> first, Memory<T> second) _current = (first: Memory<T>.Empty, second: Memory<T>.Empty);

		public readonly (Memory<T> first, Memory<T> second) Current => _current;

		public bool MoveNext()
		{
			if (_completed == _length)
			{
				_current = (first: Memory<T>.Empty, second: Memory<T>.Empty);
				return false;
			}
			if (_completed == 0)
			{
				int num = _firstOffset >> SegmentedArrayHelper.GetSegmentShift<T>();
				int num2 = _secondOffset >> SegmentedArrayHelper.GetSegmentShift<T>();
				int num3 = _firstOffset & SegmentedArrayHelper.GetOffsetMask<T>();
				T[] array = _firstSegments[num];
				T[] array2 = _secondSegments[num2];
				int num4 = Math.Min(array.Length - num3, _length);
				_current = (first: System.MemoryExtensions.AsMemory(array).Slice(num3, num4), second: System.MemoryExtensions.AsMemory(array2).Slice(num3, num4));
				_completed = num4;
				return true;
			}
			T[] array3 = _firstSegments[_completed + _firstOffset >> SegmentedArrayHelper.GetSegmentShift<T>()];
			T[] array4 = _secondSegments[_completed + _secondOffset >> SegmentedArrayHelper.GetSegmentShift<T>()];
			int num5 = Math.Min(SegmentedArrayHelper.GetSegmentSize<T>(), _length - _completed);
			_current = (first: System.MemoryExtensions.AsMemory(array3).Slice(0, num5), second: System.MemoryExtensions.AsMemory(array4).Slice(0, num5));
			_completed += num5;
			return true;
		}
	}

	private readonly struct UnalignedSegmentEnumerable<T>
	{
		public readonly struct ReverseEnumerable(UnalignedSegmentEnumerable<T> enumerable)
		{
			private readonly UnalignedSegmentEnumerable<T> _enumerable = enumerable;

			public UnalignedSegmentEnumerator<T>.Reverse GetEnumerator()
			{
				return new UnalignedSegmentEnumerator<T>.Reverse(SegmentedCollectionsMarshal.AsSegments(_enumerable._first), _enumerable._firstOffset, SegmentedCollectionsMarshal.AsSegments(_enumerable._second), _enumerable._secondOffset, _enumerable._length);
			}

			public UnalignedSegmentEnumerable<T> Reverse()
			{
				return _enumerable;
			}
		}

		private readonly SegmentedArray<T> _first;

		private readonly int _firstOffset;

		private readonly SegmentedArray<T> _second;

		private readonly int _secondOffset;

		private readonly int _length;

		public UnalignedSegmentEnumerable(SegmentedArray<T> first, SegmentedArray<T> second, int length)
			: this(first, 0, second, 0, length)
		{
		}

		public UnalignedSegmentEnumerable(SegmentedArray<T> first, int firstOffset, SegmentedArray<T> second, int secondOffset, int length)
		{
			_first = first;
			_firstOffset = firstOffset;
			_second = second;
			_secondOffset = secondOffset;
			_length = length;
		}

		public UnalignedSegmentEnumerator<T> GetEnumerator()
		{
			return new UnalignedSegmentEnumerator<T>(SegmentedCollectionsMarshal.AsSegments(_first), _firstOffset, SegmentedCollectionsMarshal.AsSegments(_second), _secondOffset, _length);
		}

		public ReverseEnumerable Reverse()
		{
			return new ReverseEnumerable(this);
		}
	}

	private struct UnalignedSegmentEnumerator<T>(T[][] firstSegments, int firstOffset, T[][] secondSegments, int secondOffset, int length)
	{
		public struct Reverse(T[][] firstSegments, int firstOffset, T[][] secondSegments, int secondOffset, int length)
		{
			private readonly T[][] _firstSegments = firstSegments;

			private readonly int _firstOffset = firstOffset;

			private readonly T[][] _secondSegments = secondSegments;

			private readonly int _secondOffset = secondOffset;

			private readonly int _length = length;

			private int _completed = 0;

			private (Memory<T> first, Memory<T> second) _current = (first: Memory<T>.Empty, second: Memory<T>.Empty);

			public readonly (Memory<T> first, Memory<T> second) Current => _current;

			public bool MoveNext()
			{
				if (_completed == _length)
				{
					_current = (first: Memory<T>.Empty, second: Memory<T>.Empty);
					return false;
				}
				int num = _firstOffset + _length - _completed - 1 >> SegmentedArrayHelper.GetSegmentShift<T>();
				int num2 = _secondOffset + _length - _completed - 1 >> SegmentedArrayHelper.GetSegmentShift<T>();
				int num3 = (_firstOffset + _length - _completed - 1) & SegmentedArrayHelper.GetOffsetMask<T>();
				int num4 = (_secondOffset + _length - _completed - 1) & SegmentedArrayHelper.GetOffsetMask<T>();
				T[] array = _firstSegments[num];
				T[] array2 = _secondSegments[num2];
				int val = num3 + 1;
				int val2 = num4 + 1;
				int num5 = Math.Min(Math.Min(val, val2), _length - _completed);
				_current = (first: System.MemoryExtensions.AsMemory(array).Slice(num3 - num5 + 1, num5), second: System.MemoryExtensions.AsMemory(array2).Slice(num4 - num5 + 1, num5));
				_completed += num5;
				return true;
			}
		}

		private readonly T[][] _firstSegments = firstSegments;

		private readonly int _firstOffset = firstOffset;

		private readonly T[][] _secondSegments = secondSegments;

		private readonly int _secondOffset = secondOffset;

		private readonly int _length = length;

		private int _completed = 0;

		private (Memory<T> first, Memory<T> second) _current = (first: Memory<T>.Empty, second: Memory<T>.Empty);

		public readonly (Memory<T> first, Memory<T> second) Current => _current;

		public bool MoveNext()
		{
			if (_completed == _length)
			{
				_current = (first: Memory<T>.Empty, second: Memory<T>.Empty);
				return false;
			}
			int num = _completed + _firstOffset >> SegmentedArrayHelper.GetSegmentShift<T>();
			int num2 = _completed + _secondOffset >> SegmentedArrayHelper.GetSegmentShift<T>();
			int num3 = (_completed + _firstOffset) & SegmentedArrayHelper.GetOffsetMask<T>();
			int num4 = (_completed + _secondOffset) & SegmentedArrayHelper.GetOffsetMask<T>();
			T[] array = _firstSegments[num];
			T[] array2 = _secondSegments[num2];
			int val = array.Length - num3;
			int val2 = array2.Length - num4;
			int num5 = Math.Min(Math.Min(val, val2), _length - _completed);
			_current = (first: System.MemoryExtensions.AsMemory(array).Slice(num3, num5), second: System.MemoryExtensions.AsMemory(array2).Slice(num4, num5));
			_completed += num5;
			return true;
		}
	}

	private readonly struct SegmentEnumerable<T>
	{
		public readonly struct ReverseEnumerable(SegmentEnumerable<T> enumerable)
		{
			private readonly SegmentEnumerable<T> _enumerable = enumerable;

			public SegmentEnumerator<T>.Reverse GetEnumerator()
			{
				return new SegmentEnumerator<T>.Reverse(SegmentedCollectionsMarshal.AsSegments(_enumerable._array), _enumerable._offset, _enumerable._length);
			}

			public SegmentEnumerable<T> Reverse()
			{
				return _enumerable;
			}
		}

		private readonly SegmentedArray<T> _array;

		private readonly int _offset;

		private readonly int _length;

		public SegmentEnumerable(SegmentedArray<T> array)
		{
			_array = array;
			_offset = 0;
			_length = array.Length;
		}

		public SegmentEnumerable(SegmentedArray<T> array, int offset, int length)
		{
			if (offset < 0 || length < 0 || (uint)(offset + length) > (uint)array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException();
			}
			_array = array;
			_offset = offset;
			_length = length;
		}

		public SegmentEnumerator<T> GetEnumerator()
		{
			return new SegmentEnumerator<T>(SegmentedCollectionsMarshal.AsSegments(_array), _offset, _length);
		}

		public ReverseEnumerable Reverse()
		{
			return new ReverseEnumerable(this);
		}
	}

	private struct SegmentEnumerator<T>(T[][] segments, int offset, int length)
	{
		public struct Reverse(T[][] segments, int offset, int length)
		{
			private readonly T[][] _segments = segments;

			private readonly int _offset = offset;

			private readonly int _length = length;

			private int _completed = 0;

			private Memory<T> _current = Memory<T>.Empty;

			public readonly Memory<T> Current => _current;

			public bool MoveNext()
			{
				if (_completed == _length)
				{
					_current = Memory<T>.Empty;
					return false;
				}
				if (_completed == 0)
				{
					int num = _offset >> SegmentedArrayHelper.GetSegmentShift<T>();
					int num2 = _offset & SegmentedArrayHelper.GetOffsetMask<T>();
					T[] array = _segments[num];
					int val = array.Length - num2;
					_current = System.MemoryExtensions.AsMemory(array).Slice(num2, Math.Min(val, _length));
					_completed = _current.Length;
					return true;
				}
				T[] array2 = _segments[_completed + _offset >> SegmentedArrayHelper.GetSegmentShift<T>()];
				_current = System.MemoryExtensions.AsMemory(array2).Slice(0, Math.Min(SegmentedArrayHelper.GetSegmentSize<T>(), _length - _completed));
				_completed += _current.Length;
				return true;
			}
		}

		private readonly T[][] _segments = segments;

		private readonly int _offset = offset;

		private readonly int _length = length;

		private int _completed = 0;

		private Memory<T> _current = Memory<T>.Empty;

		public readonly Memory<T> Current => _current;

		public bool MoveNext()
		{
			if (_completed == _length)
			{
				_current = Memory<T>.Empty;
				return false;
			}
			if (_completed == 0)
			{
				int num = _offset >> SegmentedArrayHelper.GetSegmentShift<T>();
				int num2 = _offset & SegmentedArrayHelper.GetOffsetMask<T>();
				T[] array = _segments[num];
				int val = array.Length - num2;
				_current = System.MemoryExtensions.AsMemory(array).Slice(num2, Math.Min(val, _length));
				_completed = _current.Length;
				return true;
			}
			T[] array2 = _segments[_completed + _offset >> SegmentedArrayHelper.GetSegmentShift<T>()];
			_current = System.MemoryExtensions.AsMemory(array2).Slice(0, Math.Min(SegmentedArrayHelper.GetSegmentSize<T>(), _length - _completed));
			_completed += _current.Length;
			return true;
		}
	}

	internal static void Clear<T>(SegmentedArray<T> array)
	{
		Clear(array, 0, array.Length);
	}

	internal static void Clear<T>(SegmentedArray<T> array, int index, int length)
	{
		foreach (Memory<T> segment in array.GetSegments(index, length))
		{
			segment.Span.Clear();
		}
	}

	internal static void Copy<T>(SegmentedArray<T> sourceArray, SegmentedArray<T> destinationArray, int length)
	{
		if (length != 0)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			if (length > sourceArray.Length)
			{
				throw new ArgumentException("Source array was not long enough. Check the source index, length, and the array's lower bounds.", "sourceArray");
			}
			if (length > destinationArray.Length)
			{
				throw new ArgumentException("Destination array was not long enough. Check the destination index, length, and the array's lower bounds.", "destinationArray");
			}
			foreach (var (memory, destination) in GetSegments(sourceArray, destinationArray, length))
			{
				memory.CopyTo(destination);
			}
		}
	}

	public static void Copy<T>(SegmentedArray<T> sourceArray, Array destinationArray, int length)
	{
		if (destinationArray == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.destinationArray);
		}
		if (length == 0)
		{
			return;
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (length > sourceArray.Length)
		{
			throw new ArgumentException("Source array was not long enough. Check the source index, length, and the array's lower bounds.", "sourceArray");
		}
		if (length > destinationArray.Length)
		{
			throw new ArgumentException("Destination array was not long enough. Check the destination index, length, and the array's lower bounds.", "destinationArray");
		}
		int num = 0;
		foreach (Memory<T> segment2 in sourceArray.GetSegments(0, length))
		{
			if (!MemoryMarshal.TryGetArray((ReadOnlyMemory<T>)segment2, out ArraySegment<T> segment))
			{
				throw new NotSupportedException();
			}
			Array.Copy(segment.Array, segment.Offset, destinationArray, num, segment.Count);
			num += segment.Count;
		}
	}

	public static void Copy<T>(SegmentedArray<T> sourceArray, int sourceIndex, SegmentedArray<T> destinationArray, int destinationIndex, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Non-negative number required.");
		}
		if (sourceIndex < 0)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", "Number was less than the array's lower bound in the first dimension.");
		}
		if (destinationIndex < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex", "Number was less than the array's lower bound in the first dimension.");
		}
		if ((uint)(sourceIndex + length) > sourceArray.Length)
		{
			throw new ArgumentException("Source array was not long enough. Check the source index, length, and the array's lower bounds.", "sourceArray");
		}
		if ((uint)(destinationIndex + length) > destinationArray.Length)
		{
			throw new ArgumentException("Destination array was not long enough. Check the destination index, length, and the array's lower bounds.", "destinationArray");
		}
		if (length == 0)
		{
			return;
		}
		if (SegmentedCollectionsMarshal.AsSegments(sourceArray) == SegmentedCollectionsMarshal.AsSegments(destinationArray) && sourceIndex + length > destinationIndex)
		{
			CopyOverlapped(sourceArray, sourceIndex, destinationIndex, length);
			return;
		}
		foreach (var (memory, destination) in GetSegmentsUnaligned(sourceArray, sourceIndex, destinationArray, destinationIndex, length))
		{
			memory.CopyTo(destination);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CopyOverlapped<T>(SegmentedArray<T> array, int sourceIndex, int destinationIndex, int length)
	{
		UnalignedSegmentEnumerable<T> segmentsUnaligned = GetSegmentsUnaligned(array, sourceIndex, array, destinationIndex, length);
		if (sourceIndex < destinationIndex)
		{
			foreach (var (memory, destination) in segmentsUnaligned.Reverse())
			{
				memory.CopyTo(destination);
			}
		}
		else
		{
			foreach (var (memory2, destination2) in segmentsUnaligned)
			{
				memory2.CopyTo(destination2);
			}
		}
	}

	public static void Copy<T>(SegmentedArray<T> sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
	{
		if (destinationArray == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.destinationArray);
		}
		if (typeof(T[]) != destinationArray.GetType() && destinationArray.Rank != 1)
		{
			throw new RankException("The specified arrays must have the same number of dimensions.");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Non-negative number required.");
		}
		if (sourceIndex < 0)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", "Number was less than the array's lower bound in the first dimension.");
		}
		int lowerBound = destinationArray.GetLowerBound(0);
		if (destinationIndex < lowerBound || destinationIndex - lowerBound < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex", "Number was less than the array's lower bound in the first dimension.");
		}
		destinationIndex -= lowerBound;
		if ((uint)(sourceIndex + length) > sourceArray.Length)
		{
			throw new ArgumentException("Source array was not long enough. Check the source index, length, and the array's lower bounds.", "sourceArray");
		}
		if ((uint)(destinationIndex + length) > (nuint)destinationArray.LongLength)
		{
			throw new ArgumentException("Destination array was not long enough. Check the destination index, length, and the array's lower bounds.", "destinationArray");
		}
		int num = 0;
		foreach (Memory<T> segment2 in sourceArray.GetSegments(sourceIndex, length))
		{
			if (!MemoryMarshal.TryGetArray((ReadOnlyMemory<T>)segment2, out ArraySegment<T> segment))
			{
				throw new NotSupportedException();
			}
			Array.Copy(segment.Array, segment.Offset, destinationArray, destinationIndex + num, segment.Count);
			num += segment.Count;
		}
	}

	public static int BinarySearch<T>(SegmentedArray<T> array, T value)
	{
		return BinarySearch(array, 0, array.Length, value, null);
	}

	public static int BinarySearch<T>(SegmentedArray<T> array, T value, IComparer<T>? comparer)
	{
		return BinarySearch(array, 0, array.Length, value, comparer);
	}

	public static int BinarySearch<T>(SegmentedArray<T> array, int index, int length, T value)
	{
		return BinarySearch(array, index, length, value, null);
	}

	public static int BinarySearch<T>(SegmentedArray<T> array, int index, int length, T value, IComparer<T>? comparer)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (length < 0)
		{
			ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
		}
		if (array.Length - index < length)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		return SegmentedArraySortHelper<T>.BinarySearch(array, index, length, value, comparer);
	}

	public static int IndexOf<T>(SegmentedArray<T> array, T value)
	{
		return IndexOf(array, value, 0, array.Length, null);
	}

	public static int IndexOf<T>(SegmentedArray<T> array, T value, int startIndex)
	{
		return IndexOf(array, value, startIndex, array.Length - startIndex, null);
	}

	public static int IndexOf<T>(SegmentedArray<T> array, T value, int startIndex, int count)
	{
		return IndexOf(array, value, startIndex, count, null);
	}

	public static int IndexOf<T>(SegmentedArray<T> array, T value, int startIndex, int count, IEqualityComparer<T>? comparer)
	{
		if ((uint)startIndex > (uint)array.Length)
		{
			ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLessOrEqual();
		}
		if ((uint)count > (uint)(array.Length - startIndex))
		{
			ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
		}
		int num = startIndex;
		foreach (Memory<T> segment2 in array.GetSegments(startIndex, count))
		{
			if (!MemoryMarshal.TryGetArray((ReadOnlyMemory<T>)segment2, out ArraySegment<T> segment))
			{
				throw new NotSupportedException();
			}
			int num2;
			if (comparer == null || comparer == EqualityComparer<T>.Default)
			{
				num2 = Array.IndexOf(segment.Array, value, segment.Offset, segment.Count);
			}
			else
			{
				num2 = -1;
				int num3 = segment.Offset + segment.Count;
				for (int i = segment.Offset; i < num3; i++)
				{
					if (comparer.Equals(array[i], value))
					{
						num2 = i;
						break;
					}
				}
			}
			if (num2 >= 0)
			{
				return num2 + num - segment.Offset;
			}
			num += segment.Count;
		}
		return -1;
	}

	public static int LastIndexOf<T>(SegmentedArray<T> array, T value)
	{
		return LastIndexOf(array, value, array.Length - 1, array.Length, null);
	}

	public static int LastIndexOf<T>(SegmentedArray<T> array, T value, int startIndex)
	{
		return LastIndexOf(array, value, startIndex, (array.Length != 0) ? (startIndex + 1) : 0, null);
	}

	public static int LastIndexOf<T>(SegmentedArray<T> array, T value, int startIndex, int count)
	{
		return LastIndexOf(array, value, startIndex, count, null);
	}

	public static int LastIndexOf<T>(SegmentedArray<T> array, T value, int startIndex, int count, IEqualityComparer<T>? comparer)
	{
		if (array.Length == 0)
		{
			if (startIndex != -1 && startIndex != 0)
			{
				ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess();
			}
			if (count != 0)
			{
				ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
			}
			return -1;
		}
		if ((uint)startIndex >= (uint)array.Length)
		{
			ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess();
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
		}
		if (comparer == null || comparer == EqualityComparer<T>.Default)
		{
			int num = startIndex - count + 1;
			for (int num2 = startIndex; num2 >= num; num2--)
			{
				if (EqualityComparer<T>.Default.Equals(array[num2], value))
				{
					return num2;
				}
			}
		}
		else
		{
			int num3 = startIndex - count + 1;
			for (int num4 = startIndex; num4 >= num3; num4--)
			{
				if (comparer.Equals(array[num4], value))
				{
					return num4;
				}
			}
		}
		return -1;
	}

	public static void Reverse<T>(SegmentedArray<T> array)
	{
		Reverse(array, 0, array.Length);
	}

	public static void Reverse<T>(SegmentedArray<T> array, int index, int length)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (length < 0)
		{
			ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
		}
		if (array.Length - index < length)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		if (length > 1)
		{
			int num = index;
			int num2 = index + length - 1;
			do
			{
				T val = array[num];
				array[num] = array[num2];
				array[num2] = val;
				num++;
				num2--;
			}
			while (num < num2);
		}
	}

	public static void Sort<T>(SegmentedArray<T> array)
	{
		if (array.Length > 1)
		{
			SegmentedArraySortHelper<T>.Sort(new SegmentedArraySegment<T>(array, 0, array.Length), (IComparer<T>?)null);
		}
	}

	public static void Sort<T>(SegmentedArray<T> array, int index, int length)
	{
		Sort(array, index, length, null);
	}

	public static void Sort<T>(SegmentedArray<T> array, IComparer<T>? comparer)
	{
		Sort(array, 0, array.Length, comparer);
	}

	public static void Sort<T>(SegmentedArray<T> array, int index, int length, IComparer<T>? comparer)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (length < 0)
		{
			ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
		}
		if (array.Length - index < length)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
		}
		if (length > 1)
		{
			SegmentedArraySortHelper<T>.Sort(new SegmentedArraySegment<T>(array, index, length), comparer);
		}
	}

	public static void Sort<T>(SegmentedArray<T> array, Comparison<T> comparison)
	{
		if (comparison == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
		}
		if (array.Length > 1)
		{
			SegmentedArraySortHelper<T>.Sort(new SegmentedArraySegment<T>(array, 0, array.Length), comparison);
		}
	}

	private static SegmentEnumerable<T> GetSegments<T>(this SegmentedArray<T> array, int offset, int length)
	{
		return new SegmentEnumerable<T>(array, offset, length);
	}

	private static AlignedSegmentEnumerable<T> GetSegments<T>(SegmentedArray<T> first, SegmentedArray<T> second, int length)
	{
		return new AlignedSegmentEnumerable<T>(first, second, length);
	}

	private static AlignedSegmentEnumerable<T> GetSegmentsAligned<T>(SegmentedArray<T> first, int firstOffset, SegmentedArray<T> second, int secondOffset, int length)
	{
		return new AlignedSegmentEnumerable<T>(first, firstOffset, second, secondOffset, length);
	}

	private static UnalignedSegmentEnumerable<T> GetSegmentsUnaligned<T>(SegmentedArray<T> first, int firstOffset, SegmentedArray<T> second, int secondOffset, int length)
	{
		return new UnalignedSegmentEnumerable<T>(first, firstOffset, second, secondOffset, length);
	}
}
internal readonly struct SegmentedArray<T> : ICloneable, IList, ICollection, IEnumerable, IStructuralComparable, IStructuralEquatable, IList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEquatable<SegmentedArray<T>>
{
	internal static class PrivateMarshal
	{
		public static T[][] AsSegments(SegmentedArray<T> array)
		{
			return array._items;
		}

		public static SegmentedArray<T> AsSegmentedArray(int length, T[][] segments)
		{
			return new SegmentedArray<T>(length, segments);
		}
	}

	public struct Enumerator(SegmentedArray<T> array) : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly T[][] _items = array._items;

		private int _nextItemSegment = 0;

		private int _nextItemIndex = 0;

		private T _current = default(T);

		public readonly T Current => _current;

		readonly object? IEnumerator.Current => Current;

		public readonly void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_items.Length == 0)
			{
				return false;
			}
			if (_nextItemIndex == _items[_nextItemSegment].Length)
			{
				if (_nextItemSegment == _items.Length - 1)
				{
					return false;
				}
				_nextItemSegment++;
				_nextItemIndex = 0;
			}
			_current = _items[_nextItemSegment][_nextItemIndex];
			_nextItemIndex++;
			return true;
		}

		public void Reset()
		{
			_nextItemSegment = 0;
			_nextItemIndex = 0;
			_current = default(T);
		}
	}

	internal static class TestAccessor
	{
		public static int SegmentSize => SegmentedArray<T>.SegmentSize;
	}

	private readonly int _length;

	private readonly T[][] _items;

	private static int SegmentSize => SegmentedArrayHelper.GetSegmentSize<T>();

	private static int SegmentShift => SegmentedArrayHelper.GetSegmentShift<T>();

	private static int OffsetMask => SegmentedArrayHelper.GetOffsetMask<T>();

	public bool IsFixedSize => true;

	public bool IsReadOnly => true;

	public bool IsSynchronized => false;

	public int Length => _length;

	public object SyncRoot => _items;

	public ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref _items[index >> SegmentShift][index & OffsetMask];
		}
	}

	int ICollection.Count => Length;

	int ICollection<T>.Count => Length;

	int IReadOnlyCollection<T>.Count => Length;

	T IReadOnlyList<T>.this[int index] => this[index];

	T IList<T>.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = value;
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
			this[index] = (T)value;
		}
	}

	public SegmentedArray(int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (length == 0)
		{
			_items = Array.Empty<T[]>();
			_length = 0;
			return;
		}
		_items = new T[length + SegmentSize - 1 >> SegmentShift][];
		for (int i = 0; i < _items.Length - 1; i++)
		{
			_items[i] = new T[SegmentSize];
		}
		int num = length - (_items.Length - 1 << SegmentShift);
		_items[_items.Length - 1] = new T[num];
		_length = length;
	}

	private SegmentedArray(int length, T[][] items)
	{
		_length = length;
		_items = items;
	}

	public object Clone()
	{
		T[][] array = (T[][])_items.Clone();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (T[])array[i].Clone();
		}
		return new SegmentedArray<T>(Length, array);
	}

	public void CopyTo(Array array, int index)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			_items[i].CopyTo(array, index + i * SegmentSize);
		}
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			((ICollection<T>)_items[i]).CopyTo(array, arrayIndex + i * SegmentSize);
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public override bool Equals(object? obj)
	{
		if (obj is SegmentedArray<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _items.GetHashCode();
	}

	public bool Equals(SegmentedArray<T> other)
	{
		return _items == other._items;
	}

	int IList.Add(object? value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	void ICollection<T>.Add(T value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	void IList.Clear()
	{
		T[][] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			((IList)items[i]).Clear();
		}
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	bool IList.Contains(object? value)
	{
		T[][] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			if (((IList)items[i]).Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	bool ICollection<T>.Contains(T value)
	{
		T[][] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			if (((ICollection<T>)items[i]).Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	int IList.IndexOf(object? value)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			int num = ((IList)_items[i]).IndexOf(value);
			if (num >= 0)
			{
				return num + i * SegmentSize;
			}
		}
		return -1;
	}

	int IList<T>.IndexOf(T value)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			int num = ((IList<T>)_items[i]).IndexOf(value);
			if (num >= 0)
			{
				return num + i * SegmentSize;
			}
		}
		return -1;
	}

	void IList.Insert(int index, object? value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	void IList<T>.Insert(int index, T value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	void IList.Remove(object? value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	bool ICollection<T>.Remove(T value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	int IStructuralComparable.CompareTo(object? other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is SegmentedArray<T> segmentedArray) || Length != segmentedArray.Length)
		{
			throw new ArgumentException("Object is not a array with the same number of elements as the array to compare it to.", "other");
		}
		for (int i = 0; i < Length; i++)
		{
			int num = comparer.Compare(this[i], segmentedArray[i]);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	bool IStructuralEquatable.Equals(object? other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is SegmentedArray<T> segmentedArray))
		{
			return false;
		}
		if (_items == segmentedArray._items)
		{
			return true;
		}
		if (Length != segmentedArray.Length)
		{
			return false;
		}
		for (int i = 0; i < Length; i++)
		{
			if (!comparer.Equals(this[i], segmentedArray[i]))
			{
				return false;
			}
		}
		return true;
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		int num = 0;
		for (int i = ((Length >= 8) ? (Length - 8) : 0); i < Length; i++)
		{
			num = num * -1521134295 + comparer.GetHashCode(this[i]);
		}
		return num;
	}
}
