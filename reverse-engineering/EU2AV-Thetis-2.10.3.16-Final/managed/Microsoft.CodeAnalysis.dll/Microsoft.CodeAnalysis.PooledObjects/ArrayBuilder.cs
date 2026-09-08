using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis.PooledObjects;

[DebuggerDisplay("Count = {Count,nq}")]
[DebuggerTypeProxy(typeof(ArrayBuilder<>.DebuggerProxy))]
internal sealed class ArrayBuilder<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, ICollection<T>
{
	private sealed class DebuggerProxy
	{
		private readonly ArrayBuilder<T> _builder;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] A
		{
			get
			{
				T[] array = new T[_builder.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = _builder[i];
				}
				return array;
			}
		}

		public DebuggerProxy(ArrayBuilder<T> builder)
		{
			_builder = builder;
		}
	}

	internal struct Enumerator(ArrayBuilder<T> builder)
	{
		private readonly ArrayBuilder<T> _builder = builder;

		private int _index = -1;

		public readonly T Current => _builder[_index];

		public bool MoveNext()
		{
			_index++;
			return _index < _builder.Count;
		}
	}

	public const int PooledArrayLengthLimitExclusive = 128;

	private readonly ImmutableArray<T>.Builder _builder;

	private readonly ObjectPool<ArrayBuilder<T>>? _pool;

	private static readonly ObjectPool<ArrayBuilder<T>> s_poolInstance = CreatePool();

	public int Count
	{
		get
		{
			return _builder.Count;
		}
		set
		{
			_builder.Count = value;
		}
	}

	public int Capacity
	{
		get
		{
			return _builder.Capacity;
		}
		set
		{
			_builder.Capacity = value;
		}
	}

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

	public bool IsReadOnly => false;

	public bool IsEmpty => Count == 0;

	public ArrayBuilder(int size)
	{
		_builder = ImmutableArray.CreateBuilder<T>(size);
	}

	public ArrayBuilder()
		: this(8)
	{
	}

	private ArrayBuilder(ObjectPool<ArrayBuilder<T>> pool)
		: this()
	{
		_pool = pool;
	}

	public ImmutableArray<T> ToImmutable()
	{
		return _builder.ToImmutable();
	}

	public ImmutableArray<T> ToImmutableAndClear()
	{
		ImmutableArray<T> result;
		if (Count == 0)
		{
			result = ImmutableArray<T>.Empty;
		}
		else if (_builder.Capacity == Count)
		{
			result = _builder.MoveToImmutable();
		}
		else
		{
			result = ToImmutable();
			Clear();
		}
		return result;
	}

	public void SetItem(int index, T value)
	{
		while (index > _builder.Count)
		{
			_builder.Add(default(T));
		}
		if (index == _builder.Count)
		{
			_builder.Add(value);
		}
		else
		{
			_builder[index] = value;
		}
	}

	public void Add(T item)
	{
		_builder.Add(item);
	}

	public void Insert(int index, T item)
	{
		_builder.Insert(index, item);
	}

	public void EnsureCapacity(int capacity)
	{
		if (_builder.Capacity < capacity)
		{
			_builder.Capacity = capacity;
		}
	}

	public void Clear()
	{
		_builder.Clear();
	}

	public bool Contains(T item)
	{
		return _builder.Contains(item);
	}

	public int IndexOf(T item)
	{
		return _builder.IndexOf(item);
	}

	public int IndexOf(T item, IEqualityComparer<T> equalityComparer)
	{
		return _builder.IndexOf(item, 0, _builder.Count, equalityComparer);
	}

	public int IndexOf(T item, int startIndex, int count)
	{
		return _builder.IndexOf(item, startIndex, count);
	}

	public int FindIndex(Predicate<T> match)
	{
		return FindIndex(0, Count, match);
	}

	public int FindIndex(int startIndex, Predicate<T> match)
	{
		return FindIndex(startIndex, Count - startIndex, match);
	}

	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		int num = startIndex + count;
		for (int i = startIndex; i < num; i++)
		{
			if (match(_builder[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public int FindIndex<TArg>(Func<T, TArg, bool> match, TArg arg)
	{
		return FindIndex(0, Count, match, arg);
	}

	public int FindIndex<TArg>(int startIndex, Func<T, TArg, bool> match, TArg arg)
	{
		return FindIndex(startIndex, Count - startIndex, match, arg);
	}

	public int FindIndex<TArg>(int startIndex, int count, Func<T, TArg, bool> match, TArg arg)
	{
		int num = startIndex + count;
		for (int i = startIndex; i < num; i++)
		{
			if (match(_builder[i], arg))
			{
				return i;
			}
		}
		return -1;
	}

	public bool Remove(T element)
	{
		return _builder.Remove(element);
	}

	public void RemoveAt(int index)
	{
		_builder.RemoveAt(index);
	}

	public void RemoveRange(int index, int length)
	{
		_builder.RemoveRange(index, length);
	}

	public void RemoveLast()
	{
		_builder.RemoveAt(_builder.Count - 1);
	}

	public void RemoveAll(Predicate<T> match)
	{
		int num = 0;
		for (int i = 0; i < _builder.Count; i++)
		{
			if (!match(_builder[i]))
			{
				if (num != i)
				{
					_builder[num] = _builder[i];
				}
				num++;
			}
		}
		Clip(num);
	}

	public void RemoveAll<TArg>(Func<T, TArg, bool> match, TArg arg)
	{
		int num = 0;
		for (int i = 0; i < _builder.Count; i++)
		{
			if (!match(_builder[i], arg))
			{
				if (num != i)
				{
					_builder[num] = _builder[i];
				}
				num++;
			}
		}
		Clip(num);
	}

	public void RemoveAll<TArg>(Func<T, int, TArg, bool> match, TArg arg)
	{
		int num = 0;
		for (int i = 0; i < _builder.Count; i++)
		{
			if (!match(_builder[i], num, arg))
			{
				if (num != i)
				{
					_builder[num] = _builder[i];
				}
				num++;
			}
		}
		Clip(num);
	}

	public void ReverseContents()
	{
		_builder.Reverse();
	}

	public void Sort()
	{
		_builder.Sort();
	}

	public void Sort(IComparer<T>? comparer)
	{
		_builder.Sort(comparer);
	}

	public void Sort(Comparison<T> compare)
	{
		if (Count > 1)
		{
			Sort(Comparer<T>.Create(compare));
		}
	}

	public void Sort(int startIndex, IComparer<T> comparer)
	{
		_builder.Sort(startIndex, _builder.Count - startIndex, comparer);
	}

	public T[] ToArray()
	{
		return _builder.ToArray();
	}

	public void CopyTo(T[] array, int start)
	{
		_builder.CopyTo(array, start);
	}

	public T Last()
	{
		return _builder[_builder.Count - 1];
	}

	internal T? LastOrDefault()
	{
		if (Count != 0)
		{
			return Last();
		}
		return default(T);
	}

	public T First()
	{
		return _builder[0];
	}

	public bool Any()
	{
		return _builder.Count > 0;
	}

	public ImmutableArray<T> ToImmutableOrNull()
	{
		if (Count == 0)
		{
			return default(ImmutableArray<T>);
		}
		return ToImmutable();
	}

	public ImmutableArray<U> ToDowncastedImmutable<U>() where U : T
	{
		if (Count == 0)
		{
			return ImmutableArray<U>.Empty;
		}
		ArrayBuilder<U> instance = ArrayBuilder<U>.GetInstance(Count);
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			instance.Add((U)(object)current);
		}
		return instance.ToImmutableAndFree();
	}

	public ImmutableArray<U> ToDowncastedImmutableAndFree<U>() where U : T
	{
		ImmutableArray<U> result = this.ToDowncastedImmutable<U>();
		Free();
		return result;
	}

	public ImmutableArray<T> ToImmutableAndFree()
	{
		ImmutableArray<T> result = ((Count == 0) ? ImmutableArray<T>.Empty : ((_builder.Capacity != Count) ? ToImmutable() : _builder.MoveToImmutable()));
		Free();
		return result;
	}

	public T[] ToArrayAndFree()
	{
		T[] result = ToArray();
		Free();
		return result;
	}

	public void FreeAll(Func<T, ArrayBuilder<T>?> getNested)
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			getNested(current)?.FreeAll(getNested);
		}
		Free();
	}

	public void Free()
	{
		ObjectPool<ArrayBuilder<T>> pool = _pool;
		if (pool != null && _builder.Capacity < 128)
		{
			if (Count != 0)
			{
				Clear();
			}
			pool.Free(this);
		}
	}

	public static ArrayBuilder<T> GetInstance()
	{
		return s_poolInstance.Allocate();
	}

	public static ArrayBuilder<T> GetInstance(int capacity)
	{
		ArrayBuilder<T> instance = GetInstance();
		instance.EnsureCapacity(capacity);
		return instance;
	}

	public static ArrayBuilder<T> GetInstance(int capacity, T fillWithValue)
	{
		ArrayBuilder<T> instance = GetInstance();
		instance.EnsureCapacity(capacity);
		for (int i = 0; i < capacity; i++)
		{
			instance.Add(fillWithValue);
		}
		return instance;
	}

	public static ObjectPool<ArrayBuilder<T>> CreatePool()
	{
		return CreatePool(128);
	}

	public static ObjectPool<ArrayBuilder<T>> CreatePool(int size)
	{
		ObjectPool<ArrayBuilder<T>> pool = null;
		pool = new ObjectPool<ArrayBuilder<T>>(() => new ArrayBuilder<T>(pool), size);
		return pool;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return _builder.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _builder.GetEnumerator();
	}

	internal Dictionary<K, ImmutableArray<T>> ToDictionary<K>(Func<T, K> keySelector, IEqualityComparer<K>? comparer = null) where K : notnull
	{
		if (Count == 1)
		{
			Dictionary<K, ImmutableArray<T>> dictionary = new Dictionary<K, ImmutableArray<T>>(1, comparer);
			T val = this[0];
			dictionary.Add(keySelector(val), ImmutableArray.Create(val));
			return dictionary;
		}
		if (Count == 0)
		{
			return new Dictionary<K, ImmutableArray<T>>(comparer);
		}
		Dictionary<K, ArrayBuilder<T>> dictionary2 = new Dictionary<K, ArrayBuilder<T>>(Count, comparer);
		for (int i = 0; i < Count; i++)
		{
			T val2 = this[i];
			K key = keySelector(val2);
			if (!dictionary2.TryGetValue(key, out var value))
			{
				value = GetInstance();
				dictionary2.Add(key, value);
			}
			value.Add(val2);
		}
		Dictionary<K, ImmutableArray<T>> dictionary3 = new Dictionary<K, ImmutableArray<T>>(dictionary2.Count, comparer);
		foreach (KeyValuePair<K, ArrayBuilder<T>> item in dictionary2)
		{
			dictionary3.Add(item.Key, item.Value.ToImmutableAndFree());
		}
		return dictionary3;
	}

	public void AddRange(ArrayBuilder<T> items)
	{
		_builder.AddRange(items._builder);
	}

	public void AddRange<U>(ArrayBuilder<U> items, Func<U, T> selector)
	{
		foreach (U item in items)
		{
			_builder.Add(selector(item));
		}
	}

	public void AddRange<U>(ArrayBuilder<U> items) where U : T
	{
		_builder.AddRange<U>(items._builder);
	}

	public void AddRange<U>(ArrayBuilder<U> items, int start, int length) where U : T
	{
		int i = start;
		for (int num = start + length; i < num; i++)
		{
			Add((T)(object)items[i]);
		}
	}

	public void AddRange(ImmutableArray<T> items)
	{
		_builder.AddRange(items);
	}

	public void AddRange(ImmutableArray<T> items, int length)
	{
		_builder.AddRange(items, length);
	}

	public void AddRange(ImmutableArray<T> items, int start, int length)
	{
		int i = start;
		for (int num = start + length; i < num; i++)
		{
			Add(items[i]);
		}
	}

	public void AddRange<S>(ImmutableArray<S> items) where S : class, T
	{
		AddRange(ImmutableArray<T>.CastUp<S>(items));
	}

	public void AddRange(T[] items, int start, int length)
	{
		int i = start;
		for (int num = start + length; i < num; i++)
		{
			Add(items[i]);
		}
	}

	public void AddRange(IEnumerable<T> items)
	{
		_builder.AddRange(items);
	}

	public void AddRange(params T[] items)
	{
		_builder.AddRange(items);
	}

	public void AddRange(T[] items, int length)
	{
		_builder.AddRange(items, length);
	}

	public void Clip(int limit)
	{
		_builder.Count = limit;
	}

	public void ZeroInit(int count)
	{
		_builder.Clear();
		_builder.Count = count;
	}

	public void AddMany(T item, int count)
	{
		EnsureCapacity(Count + count);
		for (int i = 0; i < count; i++)
		{
			Add(item);
		}
	}

	public void RemoveDuplicates()
	{
		PooledHashSet<T> instance = PooledHashSet<T>.GetInstance();
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			if (instance.Add(this[i]))
			{
				this[num] = this[i];
				num++;
			}
		}
		Clip(num);
		instance.Free();
	}

	public void SortAndRemoveDuplicates(IComparer<T>? comparer = null)
	{
		if (Count <= 1)
		{
			return;
		}
		if (comparer == null)
		{
			comparer = Comparer<T>.Default;
		}
		Sort(comparer);
		int num = 0;
		for (int i = 1; i < Count; i++)
		{
			if (comparer.Compare(this[num], this[i]) < 0)
			{
				num++;
				this[num] = this[i];
			}
		}
		Clip(num + 1);
	}

	public ImmutableArray<S> SelectDistinct<S>(Func<T, S> selector)
	{
		ArrayBuilder<S> instance = ArrayBuilder<S>.GetInstance(Count);
		PooledHashSet<S> instance2 = PooledHashSet<S>.GetInstance();
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			S item = selector(current);
			if (instance2.Add(item))
			{
				instance.Add(item);
			}
		}
		instance2.Free();
		return instance.ToImmutableAndFree();
	}

	public bool Any(Func<T, bool> predicate)
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (predicate(current))
			{
				return true;
			}
		}
		return false;
	}

	public bool Any<A>(Func<T, A, bool> predicate, A arg)
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (predicate(current, arg))
			{
				return true;
			}
		}
		return false;
	}

	public bool All(Func<T, bool> predicate)
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (!predicate(current))
			{
				return false;
			}
		}
		return true;
	}

	public bool All<A>(Func<T, A, bool> predicate, A arg)
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (!predicate(current, arg))
			{
				return false;
			}
		}
		return true;
	}

	public ImmutableArray<TResult> SelectAsArray<TResult>(Func<T, TResult> map)
	{
		switch (Count)
		{
		case 0:
			return ImmutableArray<TResult>.Empty;
		case 1:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[1] { map(this[0]) });
		case 2:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[2]
			{
				map(this[0]),
				map(this[1])
			});
		case 3:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[3]
			{
				map(this[0]),
				map(this[1]),
				map(this[2])
			});
		case 4:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[4]
			{
				map(this[0]),
				map(this[1]),
				map(this[2]),
				map(this[3])
			});
		default:
		{
			ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(Count);
			Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				instance.Add(map(current));
			}
			return instance.ToImmutableAndFree();
		}
		}
	}

	public ImmutableArray<TResult> SelectAsArray<TArg, TResult>(Func<T, TArg, TResult> map, TArg arg)
	{
		switch (Count)
		{
		case 0:
			return ImmutableArray<TResult>.Empty;
		case 1:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[1] { map(this[0], arg) });
		case 2:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[2]
			{
				map(this[0], arg),
				map(this[1], arg)
			});
		case 3:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[3]
			{
				map(this[0], arg),
				map(this[1], arg),
				map(this[2], arg)
			});
		case 4:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[4]
			{
				map(this[0], arg),
				map(this[1], arg),
				map(this[2], arg),
				map(this[3], arg)
			});
		default:
		{
			ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(Count);
			Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				instance.Add(map(current, arg));
			}
			return instance.ToImmutableAndFree();
		}
		}
	}

	public ImmutableArray<TResult> SelectAsArrayWithIndex<TArg, TResult>(Func<T, int, TArg, TResult> map, TArg arg)
	{
		switch (Count)
		{
		case 0:
			return ImmutableArray<TResult>.Empty;
		case 1:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[1] { map(this[0], 0, arg) });
		case 2:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[2]
			{
				map(this[0], 0, arg),
				map(this[1], 1, arg)
			});
		case 3:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[3]
			{
				map(this[0], 0, arg),
				map(this[1], 1, arg),
				map(this[2], 2, arg)
			});
		case 4:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[4]
			{
				map(this[0], 0, arg),
				map(this[1], 1, arg),
				map(this[2], 2, arg),
				map(this[3], 3, arg)
			});
		default:
		{
			ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(Count);
			Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				instance.Add(map(current, instance.Count, arg));
			}
			return instance.ToImmutableAndFree();
		}
		}
	}

	public void Push(T e)
	{
		Add(e);
	}

	public T Pop()
	{
		T result = Peek();
		RemoveAt(Count - 1);
		return result;
	}

	public bool TryPop([MaybeNullWhen(false)] out T result)
	{
		if (Count > 0)
		{
			result = Pop();
			return true;
		}
		result = default(T);
		return false;
	}

	public T Peek()
	{
		return this[Count - 1];
	}
}
