using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Collections;

[NonCopyable]
internal struct TemporaryArray<T> : IDisposable
{
	[NonCopyable]
	public struct Enumerator(in TemporaryArray<T> array)
	{
		private readonly TemporaryArray<T> _array = new TemporaryArray<T>(in array);

		private T _current = default(T);

		private int _nextIndex = 0;

		public T Current => _current;

		public bool MoveNext()
		{
			if (_nextIndex >= _array.Count)
			{
				return false;
			}
			_current = _array[_nextIndex];
			_nextIndex++;
			return true;
		}
	}

	internal static class TestAccessor
	{
		public static int InlineCapacity => 4;

		public static bool HasDynamicStorage(in TemporaryArray<T> array)
		{
			return array._builder != null;
		}

		public static int InlineCount(in TemporaryArray<T> array)
		{
			return array._count;
		}
	}

	private const int InlineCapacity = 4;

	private T _item0;

	private T _item1;

	private T _item2;

	private T _item3;

	private int _count;

	private ArrayBuilder<T>? _builder;

	public static TemporaryArray<T> Empty => default(TemporaryArray<T>);

	public readonly int Count => _builder?.Count ?? _count;

	public T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			if (_builder != null)
			{
				return _builder[index];
			}
			if ((uint)index >= _count)
			{
				ThrowIndexOutOfRangeException();
			}
			return index switch
			{
				0 => _item0, 
				1 => _item1, 
				2 => _item2, 
				_ => _item3, 
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (_builder != null)
			{
				_builder[index] = value;
				return;
			}
			if ((uint)index >= _count)
			{
				ThrowIndexOutOfRangeException();
			}
			switch (index)
			{
			case 0:
			{
				T val = (_item0 = value);
				break;
			}
			case 1:
			{
				T val = (_item1 = value);
				break;
			}
			case 2:
			{
				T val = (_item2 = value);
				break;
			}
			default:
			{
				T val = (_item3 = value);
				break;
			}
			}
		}
	}

	private TemporaryArray(in TemporaryArray<T> array)
	{
		this = array;
	}

	public static TemporaryArray<T> GetInstance(int capacity)
	{
		if (capacity <= 4)
		{
			return Empty;
		}
		return new TemporaryArray<T>
		{
			_builder = ArrayBuilder<T>.GetInstance(capacity)
		};
	}

	public void Dispose()
	{
		Interlocked.Exchange(ref _builder, null)?.Free();
	}

	public void Add(T item)
	{
		if (_builder != null)
		{
			_builder.Add(item);
		}
		else if (_count < 4)
		{
			_count++;
			this[_count - 1] = item;
		}
		else
		{
			MoveInlineToBuilder();
			_builder.Add(item);
		}
	}

	public void AddRange(ImmutableArray<T> items)
	{
		if (_builder != null)
		{
			_builder.AddRange(items);
		}
		else if (_count + items.Length <= 4)
		{
			foreach (T item in items)
			{
				_count++;
				this[_count - 1] = item;
			}
		}
		else
		{
			MoveInlineToBuilder();
			_builder.AddRange(items);
		}
	}

	public void AddRange(in TemporaryArray<T> items)
	{
		if (_count + items.Count <= 4)
		{
			foreach (T item in items)
			{
				_count++;
				this[_count - 1] = item;
			}
		}
		else
		{
			MoveInlineToBuilder();
			foreach (T item2 in items)
			{
				_builder.Add(item2);
			}
		}
	}

	public void Clear()
	{
		if (_builder != null)
		{
			_builder.Clear();
		}
		else
		{
			this = Empty;
		}
	}

	public T RemoveLast()
	{
		int count = Count;
		T result = this[count - 1];
		this[count - 1] = default(T);
		if (_builder != null)
		{
			_builder.Count--;
			return result;
		}
		_count--;
		return result;
	}

	public readonly bool Contains(T value, IEqualityComparer<T>? equalityComparer = null)
	{
		return IndexOf(value, equalityComparer) >= 0;
	}

	public readonly int IndexOf(T value, IEqualityComparer<T>? equalityComparer = null)
	{
		if (equalityComparer == null)
		{
			equalityComparer = EqualityComparer<T>.Default;
		}
		if (_builder != null)
		{
			return _builder.IndexOf(value, equalityComparer);
		}
		int num = 0;
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (equalityComparer.Equals(current, value))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public readonly Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public OneOrMany<T> ToOneOrManyAndClear()
	{
		switch (Count)
		{
		case 0:
			return OneOrMany<T>.Empty;
		case 1:
		{
			OneOrMany<T> result = OneOrMany.Create(this[0]);
			Clear();
			return result;
		}
		default:
			return new OneOrMany<T>(ToImmutableAndClear());
		}
	}

	public ImmutableArray<T> ToImmutableAndClear()
	{
		if (_builder != null)
		{
			return _builder.ToImmutableAndClear();
		}
		object result = _count switch
		{
			0 => ImmutableArray<T>.Empty, 
			1 => ImmutableArray.Create(_item0), 
			2 => ImmutableArray.Create(_item0, _item1), 
			3 => ImmutableArray.Create(_item0, _item1, _item2), 
			4 => ImmutableArray.Create(_item0, _item1, _item2, _item3), 
			_ => throw ExceptionUtilities.Unreachable("/_/src/Dependencies/Collections/TemporaryArray`1.cs", 317), 
		};
		this = Empty;
		return (ImmutableArray<T>)result;
	}

	[MemberNotNull("_builder")]
	private void MoveInlineToBuilder()
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		for (int i = 0; i < _count; i++)
		{
			instance.Add(this[i]);
			this[i] = default(T);
		}
		_count = 0;
		_builder = instance;
	}

	public void ReverseContents()
	{
		if (_builder != null)
		{
			_builder.ReverseContents();
			return;
		}
		int count = _count;
		if (count > 1)
		{
			switch (count)
			{
			case 2:
			{
				T item = _item1;
				T item2 = _item0;
				_item0 = item;
				_item1 = item2;
				break;
			}
			case 3:
			{
				T item2 = _item2;
				T item = _item0;
				_item0 = item2;
				_item2 = item;
				break;
			}
			case 4:
			{
				T item = _item3;
				T item2 = _item2;
				T item3 = _item1;
				T item4 = _item0;
				_item0 = item;
				_item1 = item2;
				_item2 = item3;
				_item3 = item4;
				break;
			}
			default:
				throw ExceptionUtilities.Unreachable("/_/src/Dependencies/Collections/TemporaryArray`1.cs", 379);
			}
		}
	}

	public void Sort(Comparison<T> compare)
	{
		if (_builder != null)
		{
			_builder.Sort(compare);
			return;
		}
		int count = _count;
		if (count <= 1)
		{
			return;
		}
		switch (count)
		{
		case 2:
			if (compare(_item0, _item1) > 0)
			{
				T item = _item1;
				T item2 = _item0;
				_item0 = item;
				_item1 = item2;
			}
			break;
		case 3:
			if (compare(_item0, _item1) > 0)
			{
				T item2 = _item1;
				T item = _item0;
				_item0 = item2;
				_item1 = item;
			}
			if (compare(_item1, _item2) > 0)
			{
				T item = _item2;
				T item2 = _item1;
				_item1 = item;
				_item2 = item2;
				if (compare(_item0, _item1) > 0)
				{
					item2 = _item1;
					item = _item0;
					_item0 = item2;
					_item1 = item;
				}
			}
			break;
		case 4:
			if (compare(_item0, _item1) > 0)
			{
				T item = _item1;
				T item2 = _item0;
				_item0 = item;
				_item1 = item2;
			}
			if (compare(_item2, _item3) > 0)
			{
				T item2 = _item3;
				T item = _item2;
				_item2 = item2;
				_item3 = item;
			}
			if (compare(_item0, _item2) > 0)
			{
				T item = _item2;
				T item2 = _item0;
				_item0 = item;
				_item2 = item2;
			}
			if (compare(_item1, _item3) > 0)
			{
				T item2 = _item3;
				T item = _item1;
				_item1 = item2;
				_item3 = item;
			}
			if (compare(_item1, _item2) > 0)
			{
				T item = _item2;
				T item2 = _item1;
				_item1 = item;
				_item2 = item2;
			}
			break;
		default:
			throw ExceptionUtilities.Unreachable("/_/src/Dependencies/Collections/TemporaryArray`1.cs", 432);
		}
	}

	private static void ThrowIndexOutOfRangeException()
	{
		throw new IndexOutOfRangeException();
	}
}
