using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

public static class AsyncEnumerable
{
	private sealed class EmptyAsyncEnumerable<TResult> : IAsyncEnumerable<TResult>, IAsyncEnumerator<TResult>, IAsyncDisposable, IOrderedAsyncEnumerable<TResult>
	{
		public static readonly EmptyAsyncEnumerable<TResult> Instance = new EmptyAsyncEnumerable<TResult>();

		public TResult Current => default(TResult);

		public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
		{
			return this;
		}

		public ValueTask<bool> MoveNextAsync()
		{
			return default(ValueTask<bool>);
		}

		public ValueTask DisposeAsync()
		{
			return default(ValueTask);
		}

		public IOrderedAsyncEnumerable<TResult> CreateOrderedAsyncEnumerable<TKey>(Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool descending)
		{
			System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
			return this;
		}

		public IOrderedAsyncEnumerable<TResult> CreateOrderedAsyncEnumerable<TKey>(Func<TResult, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, bool descending)
		{
			System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
			return this;
		}
	}

	internal sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement>, IEnumerable<TElement>, IEnumerable, IList<TElement>, ICollection<TElement>
	{
		internal readonly TKey _key;

		internal readonly int _hashCode;

		internal TElement[] _elements;

		internal int _count;

		internal Grouping<TKey, TElement> _hashNext;

		internal Grouping<TKey, TElement> _next;

		public TKey Key => _key;

		int ICollection<TElement>.Count => _count;

		bool ICollection<TElement>.IsReadOnly => true;

		TElement IList<TElement>.this[int index]
		{
			get
			{
				if ((uint)index >= (uint)_count)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException("index");
				}
				return _elements[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		internal Grouping(TKey key, int hashCode)
		{
			_key = key;
			_hashCode = hashCode;
			_elements = new TElement[1];
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

		public IEnumerator<TElement> GetEnumerator()
		{
			for (int i = 0; i < _count; i++)
			{
				yield return _elements[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void ICollection<TElement>.Add(TElement item)
		{
			throw new NotSupportedException();
		}

		void ICollection<TElement>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<TElement>.Contains(TElement item)
		{
			return Array.IndexOf(_elements, item, 0, _count) >= 0;
		}

		void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
		{
			Array.Copy(_elements, 0, array, arrayIndex, _count);
		}

		bool ICollection<TElement>.Remove(TElement item)
		{
			throw new NotSupportedException();
		}

		int IList<TElement>.IndexOf(TElement item)
		{
			return Array.IndexOf(_elements, item, 0, _count);
		}

		void IList<TElement>.Insert(int index, TElement item)
		{
			throw new NotSupportedException();
		}

		void IList<TElement>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	private abstract class OrderedIterator<TElement> : IOrderedAsyncEnumerable<TElement>, IAsyncEnumerable<TElement>
	{
		internal readonly IAsyncEnumerable<TElement> _source;

		protected OrderedIterator(IAsyncEnumerable<TElement> source)
		{
			_source = source;
		}

		private protected ValueTask<int[]> CreateSortedMapAsync(TElement[] buffer, CancellationToken cancellationToken)
		{
			return GetEnumerableSorter().SortAsync(buffer, buffer.Length, cancellationToken);
		}

		internal abstract EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next = null);

		public IOrderedAsyncEnumerable<TElement> CreateOrderedAsyncEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
		{
			return new OrderedIterator<TElement, TKey>(_source, keySelector, comparer, descending, this);
		}

		public IOrderedAsyncEnumerable<TElement> CreateOrderedAsyncEnumerable<TKey>(Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, bool descending)
		{
			return new OrderedIterator<TElement, TKey>(_source, keySelector, comparer, descending, this);
		}

		public abstract IAsyncEnumerator<TElement> GetAsyncEnumerator(CancellationToken cancellationToken);
	}

	private sealed class OrderedIterator<TElement, TKey> : OrderedIterator<TElement>
	{
		private readonly OrderedIterator<TElement> _parent;

		private readonly object _keySelector;

		private readonly IComparer<TKey> _comparer;

		private readonly bool _descending;

		internal OrderedIterator(IAsyncEnumerable<TElement> source, object keySelector, IComparer<TKey> comparer, bool descending, OrderedIterator<TElement> parent)
			: base(source)
		{
			_parent = parent;
			_keySelector = keySelector;
			_comparer = comparer ?? Comparer<TKey>.Default;
			_descending = descending;
		}

		internal override EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next)
		{
			IComparer<TKey> comparer = _comparer;
			if (typeof(TKey) == typeof(string) && comparer == Comparer<string>.Default)
			{
				comparer = (IComparer<TKey>)StringComparer.CurrentCulture;
			}
			EnumerableSorter<TElement> enumerableSorter = new EnumerableSorter<TElement, TKey>(_keySelector, comparer, _descending, next);
			if (_parent != null)
			{
				enumerableSorter = _parent.GetEnumerableSorter(enumerableSorter);
			}
			return enumerableSorter;
		}

		public override async IAsyncEnumerator<TElement> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			TElement[] buffer = await _source.ToArrayAsync(cancellationToken);
			if (buffer.Length != 0)
			{
				int[] map = await CreateSortedMapAsync(buffer, cancellationToken);
				for (int i = 0; i < map.Length; i++)
				{
					yield return buffer[map[i]];
				}
			}
		}
	}

	private abstract class EnumerableSorter<TElement> : IComparer<int>
	{
		internal static readonly Func<TElement, TElement> IdentityFunc = (TElement e) => e;

		internal abstract Task ComputeKeysAsync(TElement[] elements, int count, CancellationToken cancellationToken);

		public abstract int Compare(int index1, int index2);

		internal async ValueTask<int[]> SortAsync(TElement[] elements, int count, CancellationToken cancellationToken)
		{
			await ComputeKeysAsync(elements, count, cancellationToken);
			int[] array = new int[count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = i;
			}
			QuickSort(array, 0, count - 1);
			return array;
		}

		protected abstract void QuickSort(int[] map, int left, int right);
	}

	private sealed class EnumerableSorter<TElement, TKey> : EnumerableSorter<TElement>, IComparer<int>
	{
		private readonly object _keySelector;

		private readonly IComparer<TKey> _comparer;

		private readonly bool _descending;

		private readonly EnumerableSorter<TElement> _next;

		private TKey[] _keys;

		internal EnumerableSorter(object keySelector, IComparer<TKey> comparer, bool descending, EnumerableSorter<TElement> next)
		{
			_keySelector = keySelector;
			_comparer = comparer;
			_descending = descending;
			_next = next;
		}

		internal override async Task ComputeKeysAsync(TElement[] elements, int count, CancellationToken cancellationToken)
		{
			object keySelector = _keySelector;
			if (keySelector == EnumerableSorter<TElement>.IdentityFunc)
			{
				_keys = (TKey[])(object)elements;
			}
			else
			{
				TKey[] keys = new TKey[count];
				if (keySelector is Func<TElement, TKey> func)
				{
					for (int i = 0; i < keys.Length; i++)
					{
						keys[i] = func(elements[i]);
					}
				}
				else
				{
					Func<TElement, CancellationToken, ValueTask<TKey>> asyncSelector = (Func<TElement, CancellationToken, ValueTask<TKey>>)keySelector;
					for (int j = 0; j < keys.Length; j++)
					{
						TKey[] array = keys;
						int num = j;
						array[num] = await asyncSelector(elements[j], cancellationToken);
					}
				}
				_keys = keys;
			}
			_next?.ComputeKeysAsync(elements, count, cancellationToken);
		}

		public override int Compare(int index1, int index2)
		{
			TKey[] keys = _keys;
			int num = _comparer.Compare(keys[index1], keys[index2]);
			if (num == 0)
			{
				if (_next == null)
				{
					return index1 - index2;
				}
				return _next.Compare(index1, index2);
			}
			if (_descending == num > 0)
			{
				return -1;
			}
			return 1;
		}

		protected override void QuickSort(int[] keys, int lo, int hi)
		{
			Array.Sort(keys, lo, hi - lo + 1, this);
		}
	}

	[DebuggerDisplay("Count = 0")]
	private sealed class EmptyLookup<TKey, TElement> : ILookup<TKey, TElement>, IEnumerable<IGrouping<TKey, TElement>>, IEnumerable, IList<IGrouping<TKey, TElement>>, ICollection<IGrouping<TKey, TElement>>, IReadOnlyCollection<IGrouping<TKey, TElement>>
	{
		public static readonly EmptyLookup<TKey, TElement> Instance = new EmptyLookup<TKey, TElement>();

		public bool IsReadOnly => true;

		public int Count => 0;

		public IEnumerable<TElement> this[TKey key] => Array.Empty<TElement>();

		public IGrouping<TKey, TElement> this[int index]
		{
			get
			{
				throw new ArgumentOutOfRangeException("index");
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
		{
			return Enumerable.Empty<IGrouping<TKey, TElement>>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Contains(TKey key)
		{
			return false;
		}

		public bool Contains(IGrouping<TKey, TElement> item)
		{
			return false;
		}

		public void CopyTo(IGrouping<TKey, TElement>[] array, int arrayIndex)
		{
			System.ExceptionPolyfills.ThrowIfNull(array, "array");
			if ((uint)arrayIndex > (uint)array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException("arrayIndex");
			}
		}

		public int IndexOf(IGrouping<TKey, TElement> item)
		{
			return -1;
		}

		public void Add(IGrouping<TKey, TElement> item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, IGrouping<TKey, TElement> item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(IGrouping<TKey, TElement> item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	[DebuggerDisplay("Count = {Count}")]
	private sealed class AsyncLookup<TKey, TElement> : ILookup<TKey, TElement>, IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
	{
		private readonly IEqualityComparer<TKey> _comparer;

		private Grouping<TKey, TElement>[] _groupings;

		internal Grouping<TKey, TElement> _lastGrouping;

		private int _count;

		public int Count => _count;

		public IEnumerable<TElement> this[TKey key]
		{
			get
			{
				IEnumerable<TElement> grouping = GetGrouping(key, create: false);
				return grouping ?? Enumerable.Empty<TElement>();
			}
		}

		internal AsyncLookup(IEqualityComparer<TKey> comparer)
		{
			_comparer = comparer ?? EqualityComparer<TKey>.Default;
			_groupings = new Grouping<TKey, TElement>[7];
		}

		internal static async ValueTask<AsyncLookup<TKey, TElement>> CreateForJoinAsync(IAsyncEnumerable<TElement> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
		{
			AsyncLookup<TKey, TElement> lookup = new AsyncLookup<TKey, TElement>(comparer);
			ConfiguredCancelableAsyncEnumerable<TElement>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TElement current = asyncEnumerator.Current;
					TKey val = keySelector(current);
					if (val != null)
					{
						lookup.GetGrouping(val, create: true).Add(current);
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return lookup;
		}

		internal static async ValueTask<AsyncLookup<TKey, TElement>> CreateForJoinAsync(IAsyncEnumerable<TElement> source, Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
		{
			AsyncLookup<TKey, TElement> lookup = new AsyncLookup<TKey, TElement>(comparer);
			ConfiguredCancelableAsyncEnumerable<TElement>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TElement item = asyncEnumerator.Current;
					TKey val = await keySelector(item, cancellationToken);
					if (val != null)
					{
						lookup.GetGrouping(val, create: true).Add(item);
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return lookup;
		}

		public bool Contains(TKey key)
		{
			return GetGrouping(key, create: false) != null;
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
		{
			Grouping<TKey, TElement> g = _lastGrouping;
			if (g != null)
			{
				do
				{
					g = g._next;
					yield return g;
				}
				while (g != _lastGrouping);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal Grouping<TKey, TElement> GetGrouping(TKey key, bool create)
		{
			int num = ((key != null) ? (_comparer.GetHashCode(key) & 0x7FFFFFFF) : 0);
			for (Grouping<TKey, TElement> grouping = _groupings[(uint)num % _groupings.Length]; grouping != null; grouping = grouping._hashNext)
			{
				if (grouping._hashCode == num && _comparer.Equals(grouping._key, key))
				{
					return grouping;
				}
			}
			if (create)
			{
				if (_count == _groupings.Length)
				{
					Resize();
				}
				int num2 = num % _groupings.Length;
				Grouping<TKey, TElement> grouping2 = new Grouping<TKey, TElement>(key, num)
				{
					_hashNext = _groupings[num2]
				};
				_groupings[num2] = grouping2;
				if (_lastGrouping == null)
				{
					grouping2._next = grouping2;
				}
				else
				{
					grouping2._next = _lastGrouping._next;
					_lastGrouping._next = grouping2;
				}
				_lastGrouping = grouping2;
				_count++;
				return grouping2;
			}
			return null;
		}

		private void Resize()
		{
			int num = checked(_count * 2 + 1);
			Grouping<TKey, TElement>[] array = new Grouping<TKey, TElement>[num];
			Grouping<TKey, TElement> grouping = _lastGrouping;
			do
			{
				grouping = grouping._next;
				int num2 = grouping._hashCode % num;
				grouping._hashNext = array[num2];
				array[num2] = grouping;
			}
			while (grouping != _lastGrouping);
			_groupings = array;
		}

		internal IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
		{
			Grouping<TKey, TElement> g = _lastGrouping;
			if (g != null)
			{
				do
				{
					g = g._next;
					g.Trim();
					yield return resultSelector(g._key, g._elements);
				}
				while (g != _lastGrouping);
			}
		}

		internal async IAsyncEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			Grouping<TKey, TElement> g = _lastGrouping;
			if (g != null)
			{
				do
				{
					g = g._next;
					g.Trim();
					yield return await resultSelector(g._key, g._elements, cancellationToken);
				}
				while (g != _lastGrouping);
			}
		}
	}

	[ThreadStatic]
	private static Random t_random;

	public static ValueTask<TSource> AggregateAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, TSource> func, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		return Impl(source, func, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, TSource, TSource> func2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result2 = default(TSource);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					ThrowHelper.ThrowNoElementsException();
				}
				TSource result = e.Current;
				while (await e.MoveNextAsync())
				{
					result = func2(result, e.Current);
				}
				result2 = result;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result2;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> AggregateAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, CancellationToken, ValueTask<TSource>> func, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		return Impl(source, func, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, TSource, CancellationToken, ValueTask<TSource>> func2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result2 = default(TSource);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					ThrowHelper.ThrowNoElementsException();
				}
				TSource result = e.Current;
				while (await e.MoveNextAsync())
				{
					result = await func2(result, e.Current, cancellationToken2);
				}
				result2 = result;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result2;
			}
			throw null;
		}
	}

	public static ValueTask<TAccumulate> AggregateAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		return Impl(source.WithCancellation(cancellationToken), seed, func);
		static async ValueTask<TAccumulate> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, TAccumulate val, Func<TAccumulate, TSource, TAccumulate> func2)
		{
			TAccumulate result = val;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					result = func2(result, current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return result;
		}
	}

	public static ValueTask<TAccumulate> AggregateAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		return Impl(source, seed, func, cancellationToken);
		static async ValueTask<TAccumulate> Impl(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func2, CancellationToken cancellationToken2 = default(CancellationToken))
		{
			TAccumulate result = val;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					result = await func2(result, current, cancellationToken2);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return result;
		}
	}

	public static ValueTask<TResult> AggregateAsync<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		return Impl(source.WithCancellation(cancellationToken), seed, func, resultSelector);
		static async ValueTask<TResult> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, TAccumulate val, Func<TAccumulate, TSource, TAccumulate> func2, Func<TAccumulate, TResult> func3)
		{
			TAccumulate result = val;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					result = func2(result, current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return func3(result);
		}
	}

	public static ValueTask<TResult> AggregateAsync<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func, Func<TAccumulate, CancellationToken, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		return Impl(source, seed, func, resultSelector, cancellationToken);
		static async ValueTask<TResult> Impl(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func2, Func<TAccumulate, CancellationToken, ValueTask<TResult>> func3, CancellationToken cancellationToken2)
		{
			TAccumulate result = val;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					result = await func2(result, current, cancellationToken2);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return await func3(result, cancellationToken2);
		}
	}

	public static IAsyncEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TSource, TKey, TAccumulate>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey>? keyComparer = null) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, seed, func, keyComparer, default(CancellationToken));
		}
		return Empty<KeyValuePair<TKey, TAccumulate>>();
		static async IAsyncEnumerable<KeyValuePair<TKey, TAccumulate>> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, TKey> func2, TAccumulate val, Func<TAccumulate, TSource, TAccumulate> func3, IEqualityComparer<TKey> comparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					Dictionary<TKey, TAccumulate> dict = new Dictionary<TKey, TAccumulate>(comparer);
					do
					{
						TSource current = e.Current;
						TKey key = func2(current);
						dict[key] = func3(dict.TryGetValue(key, out var value) ? value : val, current);
					}
					while (await e.MoveNextAsync());
					foreach (KeyValuePair<TKey, TAccumulate> item in dict)
					{
						yield return item;
					}
					num = 1;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TSource, TKey, TAccumulate>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func, IEqualityComparer<TKey>? keyComparer = null) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, seed, func, keyComparer, default(CancellationToken));
		}
		return Empty<KeyValuePair<TKey, TAccumulate>>();
		static async IAsyncEnumerable<KeyValuePair<TKey, TAccumulate>> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func2, TAccumulate val2, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func3, IEqualityComparer<TKey> comparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					Dictionary<TKey, TAccumulate> dict = new Dictionary<TKey, TAccumulate>(comparer);
					do
					{
						TSource value = e.Current;
						TKey val = await func2(value, cancellationToken);
						Dictionary<TKey, TAccumulate> dictionary = dict;
						TKey key = val;
						dictionary[key] = await func3(dict.TryGetValue(val, out var value2) ? value2 : val2, value, cancellationToken);
					}
					while (await e.MoveNextAsync());
					foreach (KeyValuePair<TKey, TAccumulate> item in dict)
					{
						yield return item;
					}
					num = 1;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TSource, TKey, TAccumulate>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey>? keyComparer = null) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(seedSelector, "seedSelector");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, seedSelector, func, keyComparer, default(CancellationToken));
		}
		return Empty<KeyValuePair<TKey, TAccumulate>>();
		static async IAsyncEnumerable<KeyValuePair<TKey, TAccumulate>> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, TKey> func2, Func<TKey, TAccumulate> func4, Func<TAccumulate, TSource, TAccumulate> func3, IEqualityComparer<TKey> comparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					Dictionary<TKey, TAccumulate> dict = new Dictionary<TKey, TAccumulate>(comparer);
					do
					{
						TSource current = e.Current;
						TKey val = func2(current);
						dict[val] = func3(dict.TryGetValue(val, out var value) ? value : func4(val), current);
					}
					while (await e.MoveNextAsync());
					foreach (KeyValuePair<TKey, TAccumulate> item in dict)
					{
						yield return item;
					}
					num = 1;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TSource, TKey, TAccumulate>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, CancellationToken, ValueTask<TAccumulate>> seedSelector, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func, IEqualityComparer<TKey>? keyComparer = null) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(seedSelector, "seedSelector");
		System.ExceptionPolyfills.ThrowIfNull(func, "func");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, seedSelector, func, keyComparer, default(CancellationToken));
		}
		return Empty<KeyValuePair<TKey, TAccumulate>>();
		static async IAsyncEnumerable<KeyValuePair<TKey, TAccumulate>> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func2, Func<TKey, CancellationToken, ValueTask<TAccumulate>> func3, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func4, IEqualityComparer<TKey> comparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					Dictionary<TKey, TAccumulate> dict = new Dictionary<TKey, TAccumulate>(comparer);
					do
					{
						TSource value = e.Current;
						TKey val = await func2(value, cancellationToken);
						Dictionary<TKey, TAccumulate> dictionary = dict;
						TKey key = val;
						TAccumulate arg = ((!dict.TryGetValue(val, out var value2)) ? (await func3(val, cancellationToken)) : value2);
						dictionary[key] = await func4(arg, value, cancellationToken);
					}
					while (await e.MoveNextAsync());
					foreach (KeyValuePair<TKey, TAccumulate> item in dict)
					{
						yield return item;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static ValueTask<bool> AllAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source.WithCancellation(cancellationToken), predicate);
		static async ValueTask<bool> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, bool> func)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			bool result = default(bool);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (!func(current))
					{
						result = false;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			return true;
		}
	}

	public static ValueTask<bool> AllAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<bool> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			bool result = default(bool);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (!(await func(current, cancellationToken2)))
					{
						result = false;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			return true;
		}
	}

	public static ValueTask<bool> AnyAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, cancellationToken);
		static async ValueTask<bool> Impl(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			bool result = default(bool);
			try
			{
				result = await e.MoveNextAsync();
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<bool> AnyAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source.WithCancellation(cancellationToken), predicate);
		static async ValueTask<bool> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, bool> func)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			bool result = default(bool);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (func(current))
					{
						result = true;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			return false;
		}
	}

	public static ValueTask<bool> AnyAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<bool> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			bool result = default(bool);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (await func(current, cancellationToken2))
					{
						result = true;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			return false;
		}
	}

	public static IAsyncEnumerable<TSource> Append<TSource>(this IAsyncEnumerable<TSource> source, TSource element)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, element, default(CancellationToken));
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, TSource val, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					yield return asyncEnumerator.Current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			yield return val;
		}
	}

	public static ValueTask<double> AverageAsync(this IAsyncEnumerable<int> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<double> Impl(ConfiguredCancelableAsyncEnumerable<int> configuredCancelableAsyncEnumerable)
		{
			long sum = 0L;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<int>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					int current = asyncEnumerator.Current;
					sum = checked(sum + current);
					count++;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (count == 0L)
			{
				ThrowHelper.ThrowNoElementsException();
			}
			return (double)sum / (double)count;
		}
	}

	public static ValueTask<double> AverageAsync(this IAsyncEnumerable<long> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<double> Impl(ConfiguredCancelableAsyncEnumerable<long> configuredCancelableAsyncEnumerable)
		{
			long sum = 0L;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<long>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					long current = asyncEnumerator.Current;
					sum = checked(sum + current);
					count++;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (count == 0L)
			{
				ThrowHelper.ThrowNoElementsException();
			}
			return (double)sum / (double)count;
		}
	}

	public static ValueTask<float> AverageAsync(this IAsyncEnumerable<float> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<float> Impl(ConfiguredCancelableAsyncEnumerable<float> configuredCancelableAsyncEnumerable)
		{
			double sum = 0.0;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<float>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					double num = asyncEnumerator.Current;
					sum += num;
					count++;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (count == 0L)
			{
				ThrowHelper.ThrowNoElementsException();
			}
			return (float)(sum / (double)count);
		}
	}

	public static ValueTask<double> AverageAsync(this IAsyncEnumerable<double> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<double> Impl(ConfiguredCancelableAsyncEnumerable<double> configuredCancelableAsyncEnumerable)
		{
			double sum = 0.0;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<double>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					double current = asyncEnumerator.Current;
					sum += current;
					count++;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (count == 0L)
			{
				ThrowHelper.ThrowNoElementsException();
			}
			return sum / (double)count;
		}
	}

	public static ValueTask<decimal> AverageAsync(this IAsyncEnumerable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<decimal> Impl(ConfiguredCancelableAsyncEnumerable<decimal> configuredCancelableAsyncEnumerable)
		{
			decimal sum = 0m;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<decimal>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					decimal current = asyncEnumerator.Current;
					sum += current;
					count++;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (count == 0L)
			{
				ThrowHelper.ThrowNoElementsException();
			}
			return sum / (decimal)count;
		}
	}

	public static ValueTask<double?> AverageAsync(this IAsyncEnumerable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<double?> Impl(ConfiguredCancelableAsyncEnumerable<int?> configuredCancelableAsyncEnumerable)
		{
			long sum = 0L;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<int?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					int? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						int valueOrDefault = current.GetValueOrDefault();
						sum = checked(sum + valueOrDefault);
						count++;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return (count != 0L) ? new double?((double)sum / (double)count) : ((double?)null);
		}
	}

	public static ValueTask<double?> AverageAsync(this IAsyncEnumerable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<double?> Impl(ConfiguredCancelableAsyncEnumerable<long?> configuredCancelableAsyncEnumerable)
		{
			long sum = 0L;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<long?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					long? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						long valueOrDefault = current.GetValueOrDefault();
						sum = checked(sum + valueOrDefault);
						count++;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return (count != 0L) ? new double?((double)sum / (double)count) : ((double?)null);
		}
	}

	public static ValueTask<float?> AverageAsync(this IAsyncEnumerable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<float?> Impl(ConfiguredCancelableAsyncEnumerable<float?> configuredCancelableAsyncEnumerable)
		{
			double sum = 0.0;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<float?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					float? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						float valueOrDefault = current.GetValueOrDefault();
						sum += (double)valueOrDefault;
						count++;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return (count != 0L) ? new float?((float)(sum / (double)count)) : ((float?)null);
		}
	}

	public static ValueTask<double?> AverageAsync(this IAsyncEnumerable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<double?> Impl(ConfiguredCancelableAsyncEnumerable<double?> configuredCancelableAsyncEnumerable)
		{
			double sum = 0.0;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<double?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					double? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						double valueOrDefault = current.GetValueOrDefault();
						sum += valueOrDefault;
						count++;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return (count != 0L) ? new double?(sum / (double)count) : ((double?)null);
		}
	}

	public static ValueTask<decimal?> AverageAsync(this IAsyncEnumerable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<decimal?> Impl(ConfiguredCancelableAsyncEnumerable<decimal?> configuredCancelableAsyncEnumerable)
		{
			decimal sum = 0m;
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<decimal?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					decimal? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						decimal valueOrDefault = current.GetValueOrDefault();
						sum += valueOrDefault;
						count++;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return (count != 0L) ? new decimal?(sum / (decimal)count) : ((decimal?)null);
		}
	}

	public static IAsyncEnumerable<TResult> Cast<TResult>(this IAsyncEnumerable<object?> source)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		object obj;
		if (!source.IsKnownEmpty())
		{
			obj = source as IAsyncEnumerable<TResult>;
			if (obj == null)
			{
				return Impl(source, default(CancellationToken));
			}
		}
		else
		{
			obj = Empty<TResult>();
		}
		return (IAsyncEnumerable<TResult>)obj;
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<object> source2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<object>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj2 = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					object current = asyncEnumerator.Current;
					yield return (TResult)current;
				}
			}
			catch (object obj3)
			{
				obj2 = obj3;
			}
			await asyncEnumerator.DisposeAsync();
			object obj4 = obj2;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource[]> Chunk<TSource>(this IAsyncEnumerable<TSource> source, int size)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		ThrowHelper.ThrowIfNegativeOrZero(size, "size");
		if (!source.IsKnownEmpty())
		{
			return Chunk(source, size, default(CancellationToken));
		}
		return Empty<TSource[]>();
		static async IAsyncEnumerable<TSource[]> Chunk(IAsyncEnumerable<TSource> asyncEnumerable, int num2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					int arraySize = Math.Min(num2, 4);
					bool flag;
					do
					{
						TSource[] array = new TSource[arraySize];
						array[0] = e.Current;
						int i = 1;
						if (num2 != array.Length)
						{
							while (true)
							{
								flag = i < num2;
								if (flag)
								{
									flag = await e.MoveNextAsync();
								}
								if (!flag)
								{
									break;
								}
								if (i >= array.Length)
								{
									arraySize = (int)Math.Min((uint)num2, (uint)(2 * array.Length));
									Array.Resize(ref array, arraySize);
								}
								array[i] = e.Current;
								i++;
							}
						}
						else
						{
							TSource[] local = array;
							while (true)
							{
								flag = (uint)i < (uint)local.Length;
								if (flag)
								{
									flag = await e.MoveNextAsync();
								}
								if (!flag)
								{
									break;
								}
								local[i] = e.Current;
								i++;
							}
						}
						if (i != array.Length)
						{
							Array.Resize(ref array, i);
						}
						yield return array;
						flag = i >= num2;
						if (flag)
						{
							flag = await e.MoveNextAsync();
						}
					}
					while (flag);
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> Concat<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		if (!first.IsKnownEmpty())
		{
			if (!second.IsKnownEmpty())
			{
				return Impl(first, second, default(CancellationToken));
			}
			return first;
		}
		return second;
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source, IAsyncEnumerable<TSource> source2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					yield return asyncEnumerator.Current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			obj = null;
			num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					yield return asyncEnumerator.Current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static ValueTask<bool> ContainsAsync<TSource>(this IAsyncEnumerable<TSource> source, TSource value, IEqualityComparer<TSource>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken), value, comparer ?? EqualityComparer<TSource>.Default);
		static async ValueTask<bool> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, TSource y, IEqualityComparer<TSource> equalityComparer)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			bool result = default(bool);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (equalityComparer.Equals(current, y))
					{
						result = true;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			return false;
		}
	}

	public static ValueTask<int> CountAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, cancellationToken);
		static async ValueTask<int> Impl(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken2 = default(CancellationToken))
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			int result = default(int);
			try
			{
				int count = 0;
				while (await e.MoveNextAsync())
				{
					count = checked(count + 1);
				}
				result = count;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<int> CountAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source.WithCancellation(cancellationToken), predicate);
		static async ValueTask<int> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, bool> func)
		{
			int count = 0;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (func(current))
					{
						count = checked(count + 1);
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return count;
		}
	}

	public static ValueTask<int> CountAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<int> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2 = default(CancellationToken))
		{
			int count = 0;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (await func(current, cancellationToken2))
					{
						count = checked(count + 1);
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return count;
		}
	}

	public static ValueTask<long> LongCountAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, cancellationToken);
		static async ValueTask<long> Impl(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken2 = default(CancellationToken))
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			long result = default(long);
			try
			{
				long count = 0L;
				while (await e.MoveNextAsync())
				{
					count++;
				}
				result = count;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<long> LongCountAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source.WithCancellation(cancellationToken), predicate);
		static async ValueTask<long> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, bool> func)
		{
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (func(current))
					{
						count++;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return count;
		}
	}

	public static ValueTask<long> LongCountAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<long> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2 = default(CancellationToken))
		{
			long count = 0L;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (await func(current, cancellationToken2))
					{
						count++;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return count;
		}
	}

	public static IAsyncEnumerable<KeyValuePair<TKey, int>> CountBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, keyComparer, default(CancellationToken));
		}
		return Empty<KeyValuePair<TKey, int>>();
		static async IAsyncEnumerable<KeyValuePair<TKey, int>> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, TKey> func, IEqualityComparer<TKey> comparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					Dictionary<TKey, int> countsBy = new Dictionary<TKey, int>(comparer);
					do
					{
						TSource current = e.Current;
						TKey key = func(current);
						countsBy[key] = ((!countsBy.TryGetValue(key, out var value)) ? 1 : checked(value + 1));
					}
					while (await e.MoveNextAsync());
					foreach (KeyValuePair<TKey, int> item in countsBy)
					{
						yield return item;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<KeyValuePair<TKey, int>> CountBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? keyComparer = null) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, keyComparer, default(CancellationToken));
		}
		return Empty<KeyValuePair<TKey, int>>();
		static async IAsyncEnumerable<KeyValuePair<TKey, int>> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey> comparer, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					Dictionary<TKey, int> countsBy = new Dictionary<TKey, int>(comparer);
					do
					{
						TSource current = e.Current;
						TKey key = await func(current, cancellationToken);
						countsBy[key] = ((!countsBy.TryGetValue(key, out var value)) ? 1 : checked(value + 1));
					}
					while (await e.MoveNextAsync());
					foreach (KeyValuePair<TKey, int> item in countsBy)
					{
						yield return item;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource?> DefaultIfEmpty<TSource>(this IAsyncEnumerable<TSource> source)
	{
		return source.DefaultIfEmpty(default(TSource));
	}

	public static IAsyncEnumerable<TSource> DefaultIfEmpty<TSource>(this IAsyncEnumerable<TSource> source, TSource defaultValue)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, defaultValue, default(CancellationToken));
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, TSource val, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					yield return val;
				}
				else
				{
					do
					{
						yield return e.Current;
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> Distinct<TSource>(this IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, IEqualityComparer<TSource> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					HashSet<TSource> set = new HashSet<TSource>(comparer2);
					do
					{
						TSource current = e.Current;
						if (set.Add(current))
						{
							yield return current;
						}
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> DistinctBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, TKey> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					HashSet<TKey> set = new HashSet<TKey>(comparer2);
					do
					{
						TSource current = e.Current;
						if (set.Add(func(current)))
						{
							yield return current;
						}
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> DistinctBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					HashSet<TKey> set = new HashSet<TKey>(comparer2);
					do
					{
						TSource element = e.Current;
						HashSet<TKey> hashSet = set;
						if (hashSet.Add(await func(element, cancellationToken)))
						{
							yield return element;
						}
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> ElementAtAsync<TSource>(this IAsyncEnumerable<TSource> source, int index, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return ElementAtOrDefaultAsync(source, index, throwIfNotFound: true, cancellationToken);
	}

	public static ValueTask<TSource?> ElementAtOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, int index, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return ElementAtOrDefaultAsync(source, index, throwIfNotFound: false, cancellationToken);
	}

	public static ValueTask<TSource> ElementAtAsync<TSource>(this IAsyncEnumerable<TSource> source, Index index, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!index.IsFromEnd)
		{
			return source.ElementAtAsync(index.Value, cancellationToken);
		}
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return ElementAtFromEndOrDefault(source, index.Value, throwIfNotFound: true, cancellationToken);
	}

	public static ValueTask<TSource?> ElementAtOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Index index, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!index.IsFromEnd)
		{
			return source.ElementAtOrDefaultAsync(index.Value, cancellationToken);
		}
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return ElementAtFromEndOrDefault(source, index.Value, throwIfNotFound: false, cancellationToken);
	}

	private static async ValueTask<TSource> ElementAtOrDefaultAsync<TSource>(IAsyncEnumerable<TSource> source, int index, bool throwIfNotFound, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (index >= 0)
		{
			IAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			TSource current = default(TSource);
			try
			{
				while (await e.MoveNextAsync())
				{
					if (index == 0)
					{
						current = e.Current;
						num = 1;
						break;
					}
					index--;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return current;
			}
		}
		if (throwIfNotFound)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException("index");
		}
		return default(TSource);
	}

	private static async ValueTask<TSource> ElementAtFromEndOrDefault<TSource>(IAsyncEnumerable<TSource> source, int indexFromEnd, bool throwIfNotFound, CancellationToken cancellationToken)
	{
		if (indexFromEnd > 0)
		{
			IAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				if (await e.MoveNextAsync())
				{
					Queue<TSource> queue = new Queue<TSource>();
					queue.Enqueue(e.Current);
					while (await e.MoveNextAsync())
					{
						if (queue.Count == indexFromEnd)
						{
							queue.Dequeue();
						}
						queue.Enqueue(e.Current);
					}
					if (queue.Count == indexFromEnd)
					{
						result = queue.Dequeue();
						num = 1;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
		}
		if (throwIfNotFound)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException("index");
		}
		return default(TSource);
	}

	public static IAsyncEnumerable<TResult> Empty<TResult>()
	{
		return EmptyAsyncEnumerable<TResult>.Instance;
	}

	private static bool IsKnownEmpty<TResult>(this IAsyncEnumerable<TResult> source)
	{
		return source == EmptyAsyncEnumerable<TResult>.Instance;
	}

	public static IAsyncEnumerable<TSource> Except<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		if (!first.IsKnownEmpty())
		{
			return Impl(first, second, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, IAsyncEnumerable<TSource> source, IEqualityComparer<TSource> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> firstEnumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				if (await firstEnumerator.MoveNextAsync())
				{
					HashSet<TSource> set = new HashSet<TSource>(comparer2);
					ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
					object obj2 = null;
					int num2 = 0;
					try
					{
						while (await asyncEnumerator.MoveNextAsync())
						{
							TSource current = asyncEnumerator.Current;
							set.Add(current);
						}
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await asyncEnumerator.DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
					_ = num2;
					do
					{
						TSource current2 = firstEnumerator.Current;
						if (set.Add(current2))
						{
							yield return current2;
						}
					}
					while (await firstEnumerator.MoveNextAsync());
					num = 1;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			if (firstEnumerator != null)
			{
				await firstEnumerator.DisposeAsync();
			}
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!first.IsKnownEmpty())
		{
			return Impl(first, second, keySelector, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, IAsyncEnumerable<TKey> source, Func<TSource, TKey> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> firstEnumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				if (await firstEnumerator.MoveNextAsync())
				{
					HashSet<TKey> set = new HashSet<TKey>(comparer2);
					ConfiguredCancelableAsyncEnumerable<TKey>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
					object obj2 = null;
					int num2 = 0;
					try
					{
						while (await asyncEnumerator.MoveNextAsync())
						{
							TKey current = asyncEnumerator.Current;
							set.Add(current);
						}
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await asyncEnumerator.DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
					_ = num2;
					do
					{
						TSource current2 = firstEnumerator.Current;
						if (set.Add(func(current2)))
						{
							yield return current2;
						}
					}
					while (await firstEnumerator.MoveNextAsync());
					num = 1;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			if (firstEnumerator != null)
			{
				await firstEnumerator.DisposeAsync();
			}
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TKey> second, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!first.IsKnownEmpty())
		{
			return Impl(first, second, keySelector, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, IAsyncEnumerable<TKey> source, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> firstEnumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				if (await firstEnumerator.MoveNextAsync())
				{
					HashSet<TKey> set = new HashSet<TKey>(comparer2);
					ConfiguredCancelableAsyncEnumerable<TKey>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
					object obj2 = null;
					int num2 = 0;
					try
					{
						while (await asyncEnumerator.MoveNextAsync())
						{
							TKey current = asyncEnumerator.Current;
							set.Add(current);
						}
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await asyncEnumerator.DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
					_ = num2;
					do
					{
						TSource firstElement = firstEnumerator.Current;
						HashSet<TKey> hashSet = set;
						if (hashSet.Add(await func(firstElement, cancellationToken)))
						{
							yield return firstElement;
						}
					}
					while (await firstEnumerator.MoveNextAsync());
					num = 1;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			if (firstEnumerator != null)
			{
				await firstEnumerator.DisposeAsync();
			}
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource current = default(TSource);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					ThrowHelper.ThrowNoElementsException();
				}
				current = e.Current;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return current;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source.WithCancellation(cancellationToken), predicate);
		static async ValueTask<TSource> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, bool> func)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (func(current))
					{
						result = current;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			ThrowHelper.ThrowNoElementsException();
			return default(TSource);
		}
	}

	public static ValueTask<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource item = asyncEnumerator.Current;
					if (await func(item, cancellationToken2))
					{
						result = item;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			ThrowHelper.ThrowNoElementsException();
			return default(TSource);
		}
	}

	public static ValueTask<TSource?> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstOrDefaultAsync(default(TSource), cancellationToken);
	}

	public static ValueTask<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, defaultValue, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, TSource val, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				result = ((await e.MoveNextAsync()) ? e.Current : val);
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstOrDefaultAsync(predicate, default(TSource), cancellationToken);
	}

	public static ValueTask<TSource?> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstOrDefaultAsync(predicate, default(TSource), cancellationToken);
	}

	public static ValueTask<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source.WithCancellation(cancellationToken), predicate, defaultValue);
		static async ValueTask<TSource> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, bool> func, TSource result2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (func(current))
					{
						result = current;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			return result2;
		}
	}

	public static ValueTask<TSource> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, defaultValue, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, TSource result2, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource item = asyncEnumerator.Current;
					if (await func(item, cancellationToken2))
					{
						result = item;
						num = 1;
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			return result2;
		}
	}

	public static IAsyncEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, comparer, default(CancellationToken));
		}
		return Empty<IGrouping<TKey, TSource>>();
		static async IAsyncEnumerable<IGrouping<TKey, TSource>> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, TKey> keySelector2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			foreach (IGrouping<TKey, TSource> item in await source2.ToLookupAsync(keySelector2, comparer2, cancellationToken))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, comparer, default(CancellationToken));
		}
		return Empty<IGrouping<TKey, TSource>>();
		static async IAsyncEnumerable<IGrouping<TKey, TSource>> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			foreach (IGrouping<TKey, TSource> item in await source2.ToLookupAsync(keySelector2, comparer2, cancellationToken))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(elementSelector, "elementSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, elementSelector, comparer, default(CancellationToken));
		}
		return Empty<IGrouping<TKey, TElement>>();
		static async IAsyncEnumerable<IGrouping<TKey, TElement>> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, TKey> keySelector2, Func<TSource, TElement> elementSelector2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			foreach (IGrouping<TKey, TElement> item in await source2.ToLookupAsync(keySelector2, elementSelector2, comparer2, cancellationToken))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(elementSelector, "elementSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, elementSelector, comparer, default(CancellationToken));
		}
		return Empty<IGrouping<TKey, TElement>>();
		static async IAsyncEnumerable<IGrouping<TKey, TElement>> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector2, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			foreach (IGrouping<TKey, TElement> item in await source2.ToLookupAsync(keySelector2, elementSelector2, comparer2, cancellationToken))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, TKey> keySelector2, Func<TKey, IEnumerable<TSource>, TResult> resultSelector2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			if (await source2.ToLookupAsync(keySelector2, comparer2, cancellationToken) is AsyncLookup<TKey, TSource> asyncLookup)
			{
				foreach (TResult item in asyncLookup.ApplyResultSelector(resultSelector2))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, IEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector2, Func<TKey, IEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			if (await source2.ToLookupAsync(keySelector2, comparer2, cancellationToken) is AsyncLookup<TKey, TSource> asyncLookup)
			{
				IAsyncEnumerator<TResult> asyncEnumerator = asyncLookup.ApplyResultSelector(resultSelector2, cancellationToken).GetAsyncEnumerator();
				object obj = null;
				int num = 0;
				try
				{
					while (await asyncEnumerator.MoveNextAsync())
					{
						yield return asyncEnumerator.Current;
					}
				}
				catch (object obj2)
				{
					obj = obj2;
				}
				if (asyncEnumerator != null)
				{
					await asyncEnumerator.DisposeAsync();
				}
				object obj3 = obj;
				if (obj3 != null)
				{
					ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
				}
				_ = num;
			}
		}
	}

	public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(elementSelector, "elementSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, elementSelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, TKey> keySelector2, Func<TSource, TElement> elementSelector2, Func<TKey, IEnumerable<TElement>, TResult> resultSelector2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			if (await source2.ToLookupAsync(keySelector2, elementSelector2, comparer2, cancellationToken) is AsyncLookup<TKey, TElement> asyncLookup)
			{
				foreach (TResult item in asyncLookup.ApplyResultSelector(resultSelector2))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, Func<TKey, IEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(elementSelector, "elementSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, keySelector, elementSelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector2, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector2, Func<TKey, IEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			if (await source2.ToLookupAsync(keySelector2, elementSelector2, comparer2, cancellationToken) is AsyncLookup<TKey, TElement> asyncLookup)
			{
				IAsyncEnumerator<TResult> asyncEnumerator = asyncLookup.ApplyResultSelector(resultSelector2, cancellationToken).GetAsyncEnumerator();
				object obj = null;
				int num = 0;
				try
				{
					while (await asyncEnumerator.MoveNextAsync())
					{
						yield return asyncEnumerator.Current;
					}
				}
				catch (object obj2)
				{
					obj = obj2;
				}
				if (asyncEnumerator != null)
				{
					await asyncEnumerator.DisposeAsync();
				}
				object obj3 = obj;
				if (obj3 != null)
				{
					ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
				}
				_ = num;
			}
		}
	}

	public static IAsyncEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(outer, "outer");
		System.ExceptionPolyfills.ThrowIfNull(inner, "inner");
		System.ExceptionPolyfills.ThrowIfNull(outerKeySelector, "outerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(innerKeySelector, "innerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!outer.IsKnownEmpty())
		{
			return Impl(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TOuter> asyncEnumerable, IAsyncEnumerable<TInner> source, Func<TOuter, TKey> func2, Func<TInner, TKey> keySelector, Func<TOuter, IEnumerable<TInner>, TResult> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TOuter> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					AsyncLookup<TKey, TInner> lookup = await AsyncLookup<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken);
					do
					{
						TOuter current = e.Current;
						yield return func(current, lookup[func2(current)]);
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, IEnumerable<TInner>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(outer, "outer");
		System.ExceptionPolyfills.ThrowIfNull(inner, "inner");
		System.ExceptionPolyfills.ThrowIfNull(outerKeySelector, "outerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(innerKeySelector, "innerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!outer.IsKnownEmpty())
		{
			return Impl(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TOuter> asyncEnumerable, IAsyncEnumerable<TInner> source, Func<TOuter, CancellationToken, ValueTask<TKey>> func2, Func<TInner, CancellationToken, ValueTask<TKey>> keySelector, Func<TOuter, IEnumerable<TInner>, CancellationToken, ValueTask<TResult>> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TOuter> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					AsyncLookup<TKey, TInner> lookup = await AsyncLookup<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken);
					do
					{
						TOuter current = e.Current;
						TOuter arg = current;
						AsyncLookup<TKey, TInner> asyncLookup = lookup;
						yield return await func(arg, asyncLookup[await func2(current, cancellationToken)], cancellationToken);
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<(int Index, TSource Item)> Index<TSource>(this IAsyncEnumerable<TSource> source)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, default(CancellationToken));
		}
		return Empty<(int, TSource)>();
		static async IAsyncEnumerable<(int Index, TSource Item)> Impl(IAsyncEnumerable<TSource> source2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					yield return (Index: num2, Item: current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> Intersect<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		if (!first.IsKnownEmpty() && !second.IsKnownEmpty())
		{
			return Impl(first, second, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source, IAsyncEnumerable<TSource> asyncEnumerable, IEqualityComparer<TSource> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			HashSet<TSource> set = default(HashSet<TSource>);
			try
			{
				if (await e.MoveNextAsync())
				{
					set = new HashSet<TSource>(comparer2);
					do
					{
						set.Add(e.Current);
					}
					while (await e.MoveNextAsync());
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			obj = null;
			num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (set.Remove(current))
					{
						yield return current;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!first.IsKnownEmpty() && !second.IsKnownEmpty())
		{
			return Impl(first, second, keySelector, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source, IAsyncEnumerable<TKey> asyncEnumerable, Func<TSource, TKey> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TKey> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			HashSet<TKey> set = default(HashSet<TKey>);
			try
			{
				if (await e.MoveNextAsync())
				{
					set = new HashSet<TKey>(comparer2);
					do
					{
						set.Add(e.Current);
					}
					while (await e.MoveNextAsync());
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			obj = null;
			num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (set.Remove(func(current)))
					{
						yield return current;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TKey> second, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!first.IsKnownEmpty() && !second.IsKnownEmpty())
		{
			return Impl(first, second, keySelector, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source, IAsyncEnumerable<TKey> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TKey> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			HashSet<TKey> set = default(HashSet<TKey>);
			try
			{
				if (await e.MoveNextAsync())
				{
					set = new HashSet<TKey>(comparer2);
					do
					{
						set.Add(e.Current);
					}
					while (await e.MoveNextAsync());
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			obj = null;
			num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					HashSet<TKey> hashSet = set;
					if (hashSet.Remove(await func(element, cancellationToken)))
					{
						yield return element;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(outer, "outer");
		System.ExceptionPolyfills.ThrowIfNull(inner, "inner");
		System.ExceptionPolyfills.ThrowIfNull(outerKeySelector, "outerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(innerKeySelector, "innerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!outer.IsKnownEmpty() && !inner.IsKnownEmpty())
		{
			return Impl(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TOuter> asyncEnumerable, IAsyncEnumerable<TInner> source, Func<TOuter, TKey> func, Func<TInner, TKey> keySelector, Func<TOuter, TInner, TResult> func2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TOuter> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					AsyncLookup<TKey, TInner> lookup = await AsyncLookup<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken);
					if (lookup.Count != 0)
					{
						do
						{
							TOuter item = e.Current;
							Grouping<TKey, TInner> grouping = lookup.GetGrouping(func(item), create: false);
							if (grouping != null)
							{
								int count = grouping._count;
								TInner[] elements = grouping._elements;
								int i = 0;
								while (i != count)
								{
									yield return func2(item, elements[i]);
									int num2 = i + 1;
									i = num2;
								}
							}
						}
						while (await e.MoveNextAsync());
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(outer, "outer");
		System.ExceptionPolyfills.ThrowIfNull(inner, "inner");
		System.ExceptionPolyfills.ThrowIfNull(outerKeySelector, "outerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(innerKeySelector, "innerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!outer.IsKnownEmpty() && !inner.IsKnownEmpty())
		{
			return Impl(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TOuter> asyncEnumerable, IAsyncEnumerable<TInner> source, Func<TOuter, CancellationToken, ValueTask<TKey>> func, Func<TInner, CancellationToken, ValueTask<TKey>> keySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> func2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TOuter> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					AsyncLookup<TKey, TInner> lookup = await AsyncLookup<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken);
					if (lookup.Count != 0)
					{
						do
						{
							TOuter item = e.Current;
							AsyncLookup<TKey, TInner> asyncLookup = lookup;
							Grouping<TKey, TInner> grouping = asyncLookup.GetGrouping(await func(item, cancellationToken), create: false);
							if (grouping != null)
							{
								int count = grouping._count;
								TInner[] elements = grouping._elements;
								int i = 0;
								while (i != count)
								{
									yield return await func2(item, elements[i], cancellationToken);
									int num2 = i + 1;
									i = num2;
								}
							}
						}
						while (await e.MoveNextAsync());
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> LastAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result2 = default(TSource);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					ThrowHelper.ThrowNoElementsException();
				}
				TSource result;
				do
				{
					result = e.Current;
				}
				while (await e.MoveNextAsync());
				result2 = result;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result2;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> LastAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, bool> func, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						ThrowHelper.ThrowNoMatchException();
						result = default(TSource);
						break;
					}
					TSource current = e.Current;
					if (func(current))
					{
						TSource result2 = current;
						while (await e.MoveNextAsync())
						{
							current = e.Current;
							if (func(current))
							{
								result2 = current;
							}
						}
						result = result2;
						break;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> LastAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						ThrowHelper.ThrowNoMatchException();
						result = default(TSource);
						break;
					}
					TSource element = e.Current;
					if (await func(element, cancellationToken2))
					{
						TSource result2 = element;
						while (await e.MoveNextAsync())
						{
							element = e.Current;
							if (await func(element, cancellationToken2))
							{
								result2 = element;
							}
						}
						result = result2;
						break;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastOrDefaultAsync(default(TSource), cancellationToken);
	}

	public static ValueTask<TSource> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, defaultValue, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, TSource val, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result2 = default(TSource);
			try
			{
				TSource result = val;
				if (await e.MoveNextAsync())
				{
					do
					{
						result = e.Current;
					}
					while (await e.MoveNextAsync());
				}
				result2 = result;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result2;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastOrDefaultAsync(predicate, default(TSource), cancellationToken);
	}

	public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastOrDefaultAsync(predicate, default(TSource), cancellationToken);
	}

	public static ValueTask<TSource> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, defaultValue, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, bool> func, TSource val, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result2 = default(TSource);
			try
			{
				TSource result = val;
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (func(current))
					{
						result = current;
						while (await e.MoveNextAsync())
						{
							current = e.Current;
							if (func(current))
							{
								result = current;
							}
						}
						break;
					}
				}
				result2 = result;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result2;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, defaultValue, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<bool>> func, TSource val, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result2 = default(TSource);
			try
			{
				TSource result = val;
				while (await e.MoveNextAsync())
				{
					TSource element = e.Current;
					if (await func(element, cancellationToken2))
					{
						result = element;
						while (await e.MoveNextAsync())
						{
							element = e.Current;
							if (await func(element, cancellationToken2))
							{
								result = element;
							}
						}
						break;
					}
				}
				result2 = result;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result2;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner?, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(outer, "outer");
		System.ExceptionPolyfills.ThrowIfNull(inner, "inner");
		System.ExceptionPolyfills.ThrowIfNull(outerKeySelector, "outerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(innerKeySelector, "innerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!outer.IsKnownEmpty())
		{
			return Impl(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TOuter> asyncEnumerable, IAsyncEnumerable<TInner> source, Func<TOuter, TKey> func, Func<TInner, TKey> keySelector, Func<TOuter, TInner, TResult> func2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TOuter> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					AsyncLookup<TKey, TInner> innerLookup = await AsyncLookup<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken);
					do
					{
						TOuter item = e.Current;
						Grouping<TKey, TInner> grouping = innerLookup.GetGrouping(func(item), create: false);
						if (grouping == null)
						{
							yield return func2(item, default(TInner));
						}
						else
						{
							int count = grouping._count;
							TInner[] elements = grouping._elements;
							int i = 0;
							while (i != count)
							{
								yield return func2(item, elements[i]);
								int num2 = i + 1;
								i = num2;
							}
						}
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner?, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(outer, "outer");
		System.ExceptionPolyfills.ThrowIfNull(inner, "inner");
		System.ExceptionPolyfills.ThrowIfNull(outerKeySelector, "outerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(innerKeySelector, "innerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!outer.IsKnownEmpty())
		{
			return Impl(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TOuter> asyncEnumerable, IAsyncEnumerable<TInner> source, Func<TOuter, CancellationToken, ValueTask<TKey>> func, Func<TInner, CancellationToken, ValueTask<TKey>> keySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> func2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TOuter> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					AsyncLookup<TKey, TInner> innerLookup = await AsyncLookup<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken);
					do
					{
						TOuter item = e.Current;
						AsyncLookup<TKey, TInner> asyncLookup = innerLookup;
						Grouping<TKey, TInner> grouping = asyncLookup.GetGrouping(await func(item, cancellationToken), create: false);
						if (grouping == null)
						{
							yield return await func2(item, default(TInner), cancellationToken);
						}
						else
						{
							int count = grouping._count;
							TInner[] elements = grouping._elements;
							int i = 0;
							while (i != count)
							{
								yield return await func2(item, elements[i], cancellationToken);
								int num2 = i + 1;
								i = num2;
							}
						}
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, IComparer<TSource>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (comparer == null)
		{
			comparer = Comparer<TSource>.Default;
		}
		if (typeof(TSource) == typeof(float) && comparer == Comparer<TSource>.Default)
		{
			return (ValueTask<TSource>)(object)((IAsyncEnumerable<float>)source).MaxAsync(cancellationToken);
		}
		if (typeof(TSource) == typeof(double) && comparer == Comparer<TSource>.Default)
		{
			return (ValueTask<TSource>)(object)((IAsyncEnumerable<double>)source).MaxAsync(cancellationToken);
		}
		if (typeof(TSource) == typeof(float?) && comparer == Comparer<TSource>.Default)
		{
			return (ValueTask<TSource>)(object)MaxAsync((IAsyncEnumerable<float?>)source, cancellationToken);
		}
		if (typeof(TSource) == typeof(double?) && comparer == Comparer<TSource>.Default)
		{
			return (ValueTask<TSource>)(object)MaxAsync((IAsyncEnumerable<double?>)source, cancellationToken);
		}
		return Impl(source, comparer, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, IComparer<TSource> comparer2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				TSource value = default(TSource);
				if (default(TSource) != null)
				{
					if (!(await e.MoveNextAsync()))
					{
						ThrowHelper.ThrowNoElementsException();
					}
					value = e.Current;
					if (comparer2 == Comparer<TSource>.Default)
					{
						while (await e.MoveNextAsync())
						{
							TSource current = e.Current;
							if (Comparer<TSource>.Default.Compare(current, value) > 0)
							{
								value = current;
							}
						}
					}
					else
					{
						while (await e.MoveNextAsync())
						{
							TSource current2 = e.Current;
							if (comparer2.Compare(current2, value) > 0)
							{
								value = current2;
							}
						}
					}
					goto IL_0367;
				}
				while (await e.MoveNextAsync())
				{
					value = e.Current;
					if (value == null)
					{
						continue;
					}
					goto IL_013f;
				}
				result = value;
				goto IL_0373;
				IL_0373:
				num = 1;
				goto end_IL_0052;
				IL_0367:
				result = value;
				goto IL_0373;
				IL_013f:
				while (await e.MoveNextAsync())
				{
					TSource current3 = e.Current;
					if (current3 != null && comparer2.Compare(current3, value) > 0)
					{
						value = current3;
					}
				}
				goto IL_0367;
				end_IL_0052:;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	private static async ValueTask<float> MaxAsync(this IAsyncEnumerable<float> source, CancellationToken cancellationToken)
	{
		IAsyncEnumerator<float> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		float result = default(float);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				ThrowHelper.ThrowNoElementsException();
			}
			float value = e.Current;
			while (true)
			{
				if (float.IsNaN(value))
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = e.Current;
					continue;
				}
				while (await e.MoveNextAsync())
				{
					float current = e.Current;
					if (current > value)
					{
						value = current;
					}
				}
				result = value;
				break;
			}
			num = 1;
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if (e != null)
		{
			await e.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		throw null;
	}

	private static async ValueTask<double> MaxAsync(this IAsyncEnumerable<double> source, CancellationToken cancellationToken)
	{
		IAsyncEnumerator<double> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		double result = default(double);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				ThrowHelper.ThrowNoElementsException();
			}
			double value = e.Current;
			while (true)
			{
				if (double.IsNaN(value))
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = e.Current;
					continue;
				}
				while (await e.MoveNextAsync())
				{
					double current = e.Current;
					if (current > value)
					{
						value = current;
					}
				}
				result = value;
				break;
			}
			num = 1;
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if (e != null)
		{
			await e.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		throw null;
	}

	private static async ValueTask<float?> MaxAsync(IAsyncEnumerable<float?> source, CancellationToken cancellationToken)
	{
		float? value = null;
		ConfiguredCancelableAsyncEnumerable<float?>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
		object obj = null;
		try
		{
			while (await asyncEnumerator.MoveNextAsync())
			{
				float? current = asyncEnumerator.Current;
				if (current.HasValue && (!value.HasValue || current > value || float.IsNaN(value.Value)))
				{
					value = current;
				}
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		await asyncEnumerator.DisposeAsync();
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		return value;
	}

	private static async ValueTask<double?> MaxAsync(IAsyncEnumerable<double?> source, CancellationToken cancellationToken)
	{
		double? value = null;
		ConfiguredCancelableAsyncEnumerable<double?>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
		object obj = null;
		try
		{
			while (await asyncEnumerator.MoveNextAsync())
			{
				double? current = asyncEnumerator.Current;
				if (current.HasValue && (!value.HasValue || current > value || double.IsNaN(value.Value)))
				{
					value = current;
				}
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		await asyncEnumerator.DisposeAsync();
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		return value;
	}

	public static ValueTask<TSource?> MaxByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		return Impl(source, keySelector, comparer ?? Comparer<TKey>.Default, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, TKey> func, IComparer<TKey> comparer2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				TSource value;
				TKey key;
				if (!(await e.MoveNextAsync()))
				{
					if (default(TSource) != null)
					{
						ThrowHelper.ThrowNoElementsException();
					}
					result = default(TSource);
				}
				else
				{
					value = e.Current;
					key = func(value);
					if (default(TKey) != null)
					{
						if (comparer2 == Comparer<TKey>.Default)
						{
							while (await e.MoveNextAsync())
							{
								TSource current = e.Current;
								TKey val = func(current);
								if (Comparer<TKey>.Default.Compare(val, key) > 0)
								{
									key = val;
									value = current;
								}
							}
						}
						else
						{
							while (await e.MoveNextAsync())
							{
								TSource current2 = e.Current;
								TKey val2 = func(current2);
								if (comparer2.Compare(val2, key) > 0)
								{
									key = val2;
									value = current2;
								}
							}
						}
						goto IL_0414;
					}
					if (key != null)
					{
						goto IL_023d;
					}
					TSource firstValue = value;
					while (await e.MoveNextAsync())
					{
						value = e.Current;
						key = func(value);
						if (key == null)
						{
							continue;
						}
						goto IL_023d;
					}
					result = firstValue;
				}
				goto IL_0420;
				IL_023d:
				while (await e.MoveNextAsync())
				{
					TSource current3 = e.Current;
					TKey val3 = func(current3);
					if (val3 != null && comparer2.Compare(val3, key) > 0)
					{
						key = val3;
						value = current3;
					}
				}
				goto IL_0414;
				IL_0420:
				num = 1;
				goto end_IL_0052;
				IL_0414:
				result = value;
				goto IL_0420;
				end_IL_0052:;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> MaxByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		return Impl(source, keySelector, comparer ?? Comparer<TKey>.Default, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func, IComparer<TKey> comparer2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				TSource value;
				TKey key;
				if (!(await e.MoveNextAsync()))
				{
					if (default(TSource) != null)
					{
						ThrowHelper.ThrowNoElementsException();
					}
					result = default(TSource);
				}
				else
				{
					value = e.Current;
					key = await func(value, cancellationToken2);
					TSource firstValue;
					if (default(TKey) != null)
					{
						if (comparer2 == Comparer<TKey>.Default)
						{
							while (await e.MoveNextAsync())
							{
								firstValue = e.Current;
								TKey val = await func(firstValue, cancellationToken2);
								if (Comparer<TKey>.Default.Compare(val, key) > 0)
								{
									key = val;
									value = firstValue;
								}
							}
						}
						else
						{
							while (await e.MoveNextAsync())
							{
								firstValue = e.Current;
								TKey val2 = await func(firstValue, cancellationToken2);
								if (comparer2.Compare(val2, key) > 0)
								{
									key = val2;
									value = firstValue;
								}
							}
						}
						goto IL_066b;
					}
					if (key != null)
					{
						goto IL_039c;
					}
					firstValue = value;
					while (await e.MoveNextAsync())
					{
						value = e.Current;
						key = await func(value, cancellationToken2);
						if (key == null)
						{
							continue;
						}
						goto IL_039c;
					}
					result = firstValue;
				}
				goto IL_0677;
				IL_039c:
				while (await e.MoveNextAsync())
				{
					TSource firstValue = e.Current;
					TKey val3 = await func(firstValue, cancellationToken2);
					if (val3 != null && comparer2.Compare(val3, key) > 0)
					{
						key = val3;
						value = firstValue;
					}
				}
				goto IL_066b;
				IL_0677:
				num = 1;
				goto end_IL_0068;
				IL_066b:
				result = value;
				goto IL_0677;
				end_IL_0068:;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, IComparer<TSource>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (comparer == null)
		{
			comparer = Comparer<TSource>.Default;
		}
		if (typeof(TSource) == typeof(float) && comparer == Comparer<TSource>.Default)
		{
			return (ValueTask<TSource>)(object)MinAsync((IAsyncEnumerable<float>)source, cancellationToken);
		}
		if (typeof(TSource) == typeof(double) && comparer == Comparer<TSource>.Default)
		{
			return (ValueTask<TSource>)(object)MinAsync((IAsyncEnumerable<double>)source, cancellationToken);
		}
		if (typeof(TSource) == typeof(float?) && comparer == Comparer<TSource>.Default)
		{
			return (ValueTask<TSource>)(object)MinAsync((IAsyncEnumerable<float?>)source, cancellationToken);
		}
		if (typeof(TSource) == typeof(double?) && comparer == Comparer<TSource>.Default)
		{
			return (ValueTask<TSource>)(object)MinAsync((IAsyncEnumerable<double?>)source, cancellationToken);
		}
		return Impl(source, comparer, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, IComparer<TSource> comparer2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				TSource value = default(TSource);
				if (default(TSource) != null)
				{
					if (!(await e.MoveNextAsync()))
					{
						ThrowHelper.ThrowNoElementsException();
					}
					value = e.Current;
					if (comparer2 == Comparer<TSource>.Default)
					{
						while (await e.MoveNextAsync())
						{
							TSource current = e.Current;
							if (Comparer<TSource>.Default.Compare(current, value) < 0)
							{
								value = current;
							}
						}
					}
					else
					{
						while (await e.MoveNextAsync())
						{
							TSource current2 = e.Current;
							if (comparer2.Compare(current2, value) < 0)
							{
								value = current2;
							}
						}
					}
					goto IL_0367;
				}
				while (await e.MoveNextAsync())
				{
					value = e.Current;
					if (value == null)
					{
						continue;
					}
					goto IL_013f;
				}
				result = value;
				goto IL_0373;
				IL_0373:
				num = 1;
				goto end_IL_0052;
				IL_0367:
				result = value;
				goto IL_0373;
				IL_013f:
				while (await e.MoveNextAsync())
				{
					TSource current3 = e.Current;
					if (current3 != null && comparer2.Compare(current3, value) < 0)
					{
						value = current3;
					}
				}
				goto IL_0367;
				end_IL_0052:;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	private static async ValueTask<float> MinAsync(IAsyncEnumerable<float> source, CancellationToken cancellationToken)
	{
		IAsyncEnumerator<float> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		float result = default(float);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				ThrowHelper.ThrowNoElementsException();
			}
			float value = e.Current;
			if (float.IsNaN(value))
			{
				result = value;
			}
			else
			{
				while (true)
				{
					if (await e.MoveNextAsync())
					{
						float current = e.Current;
						if (current < value)
						{
							value = current;
						}
						else if (float.IsNaN(current))
						{
							result = current;
							break;
						}
						continue;
					}
					result = value;
					break;
				}
			}
			num = 1;
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if (e != null)
		{
			await e.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		throw null;
	}

	private static async ValueTask<double> MinAsync(IAsyncEnumerable<double> source, CancellationToken cancellationToken)
	{
		IAsyncEnumerator<double> e = source.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		double result = default(double);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				ThrowHelper.ThrowNoElementsException();
			}
			double value = e.Current;
			if (double.IsNaN(value))
			{
				result = value;
			}
			else
			{
				while (true)
				{
					if (await e.MoveNextAsync())
					{
						double current = e.Current;
						if (current < value)
						{
							value = current;
						}
						else if (double.IsNaN(current))
						{
							result = current;
							break;
						}
						continue;
					}
					result = value;
					break;
				}
			}
			num = 1;
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if (e != null)
		{
			await e.DisposeAsync();
		}
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		throw null;
	}

	private static async ValueTask<float?> MinAsync(IAsyncEnumerable<float?> source, CancellationToken cancellationToken)
	{
		float? value = null;
		ConfiguredCancelableAsyncEnumerable<float?>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
		object obj = null;
		try
		{
			while (await asyncEnumerator.MoveNextAsync())
			{
				float? current = asyncEnumerator.Current;
				if (current.HasValue && (!value.HasValue || current < value || float.IsNaN(current.GetValueOrDefault())))
				{
					value = current;
				}
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		await asyncEnumerator.DisposeAsync();
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		return value;
	}

	private static async ValueTask<double?> MinAsync(IAsyncEnumerable<double?> source, CancellationToken cancellationToken)
	{
		double? value = null;
		ConfiguredCancelableAsyncEnumerable<double?>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
		object obj = null;
		try
		{
			while (await asyncEnumerator.MoveNextAsync())
			{
				double? current = asyncEnumerator.Current;
				if (current.HasValue && (!value.HasValue || current < value || double.IsNaN(current.GetValueOrDefault())))
				{
					value = current;
				}
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		await asyncEnumerator.DisposeAsync();
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		return value;
	}

	public static ValueTask<TSource?> MinByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		return Impl(source, keySelector, comparer ?? Comparer<TKey>.Default, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, TKey> func, IComparer<TKey> comparer2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				TSource value;
				TKey key;
				if (!(await e.MoveNextAsync()))
				{
					if (default(TSource) != null)
					{
						ThrowHelper.ThrowNoElementsException();
					}
					result = default(TSource);
				}
				else
				{
					value = e.Current;
					key = func(value);
					if (default(TKey) != null)
					{
						if (comparer2 == Comparer<TKey>.Default)
						{
							while (await e.MoveNextAsync())
							{
								TSource current = e.Current;
								TKey val = func(current);
								if (Comparer<TKey>.Default.Compare(val, key) < 0)
								{
									key = val;
									value = current;
								}
							}
						}
						else
						{
							while (await e.MoveNextAsync())
							{
								TSource current2 = e.Current;
								TKey val2 = func(current2);
								if (comparer2.Compare(val2, key) < 0)
								{
									key = val2;
									value = current2;
								}
							}
						}
						goto IL_0414;
					}
					if (key != null)
					{
						goto IL_023d;
					}
					TSource firstValue = value;
					while (await e.MoveNextAsync())
					{
						value = e.Current;
						key = func(value);
						if (key == null)
						{
							continue;
						}
						goto IL_023d;
					}
					result = firstValue;
				}
				goto IL_0420;
				IL_023d:
				while (await e.MoveNextAsync())
				{
					TSource current3 = e.Current;
					TKey val3 = func(current3);
					if (val3 != null && comparer2.Compare(val3, key) < 0)
					{
						key = val3;
						value = current3;
					}
				}
				goto IL_0414;
				IL_0420:
				num = 1;
				goto end_IL_0052;
				IL_0414:
				result = value;
				goto IL_0420;
				end_IL_0052:;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> MinByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		return Impl(source, keySelector, comparer ?? Comparer<TKey>.Default, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func, IComparer<TKey> comparer2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				TSource value;
				TKey key;
				if (!(await e.MoveNextAsync()))
				{
					if (default(TSource) != null)
					{
						ThrowHelper.ThrowNoElementsException();
					}
					result = default(TSource);
				}
				else
				{
					value = e.Current;
					key = await func(value, cancellationToken2);
					TSource firstValue;
					if (default(TKey) != null)
					{
						if (comparer2 == Comparer<TKey>.Default)
						{
							while (await e.MoveNextAsync())
							{
								firstValue = e.Current;
								TKey val = await func(firstValue, cancellationToken2);
								if (Comparer<TKey>.Default.Compare(val, key) < 0)
								{
									key = val;
									value = firstValue;
								}
							}
						}
						else
						{
							while (await e.MoveNextAsync())
							{
								firstValue = e.Current;
								TKey val2 = await func(firstValue, cancellationToken2);
								if (comparer2.Compare(val2, key) < 0)
								{
									key = val2;
									value = firstValue;
								}
							}
						}
						goto IL_066b;
					}
					if (key != null)
					{
						goto IL_039c;
					}
					firstValue = value;
					while (await e.MoveNextAsync())
					{
						value = e.Current;
						key = await func(value, cancellationToken2);
						if (key == null)
						{
							continue;
						}
						goto IL_039c;
					}
					result = firstValue;
				}
				goto IL_0677;
				IL_039c:
				while (await e.MoveNextAsync())
				{
					TSource firstValue = e.Current;
					TKey val3 = await func(firstValue, cancellationToken2);
					if (val3 != null && comparer2.Compare(val3, key) < 0)
					{
						key = val3;
						value = firstValue;
					}
				}
				goto IL_066b;
				IL_0677:
				num = 1;
				goto end_IL_0068;
				IL_066b:
				result = value;
				goto IL_0677;
				end_IL_0068:;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TResult> OfType<TResult>(this IAsyncEnumerable<object?> source)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<object> source2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<object>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					object current = asyncEnumerator.Current;
					if (current is TResult)
					{
						yield return (TResult)current;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IOrderedAsyncEnumerable<T> Order<T>(this IAsyncEnumerable<T> source, IComparer<T>? comparer = null)
	{
		return source.OrderBy(EnumerableSorter<T>.IdentityFunc, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return new OrderedIterator<TSource, TKey>(source, keySelector, comparer, descending: false, null);
		}
		return EmptyAsyncEnumerable<TSource>.Instance;
	}

	public static IOrderedAsyncEnumerable<TSource> OrderBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return new OrderedIterator<TSource, TKey>(source, keySelector, comparer, descending: false, null);
		}
		return EmptyAsyncEnumerable<TSource>.Instance;
	}

	public static IOrderedAsyncEnumerable<T> OrderDescending<T>(this IAsyncEnumerable<T> source, IComparer<T>? comparer = null)
	{
		return source.OrderByDescending(EnumerableSorter<T>.IdentityFunc, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByDescending<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return new OrderedIterator<TSource, TKey>(source, keySelector, comparer, descending: true, null);
		}
		return EmptyAsyncEnumerable<TSource>.Instance;
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByDescending<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!source.IsKnownEmpty())
		{
			return new OrderedIterator<TSource, TKey>(source, keySelector, comparer, descending: true, null);
		}
		return EmptyAsyncEnumerable<TSource>.Instance;
	}

	public static IOrderedAsyncEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return source.CreateOrderedAsyncEnumerable(keySelector, comparer, descending: false);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return source.CreateOrderedAsyncEnumerable(keySelector, comparer, descending: false);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return source.CreateOrderedAsyncEnumerable(keySelector, comparer, descending: true);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return source.CreateOrderedAsyncEnumerable(keySelector, comparer, descending: true);
	}

	public static IAsyncEnumerable<TSource> Prepend<TSource>(this IAsyncEnumerable<TSource> source, TSource element)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, element, default(CancellationToken));
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, TSource val, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			yield return val;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					yield return asyncEnumerator.Current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<int> Range(int start, int count)
	{
		if (count == 0)
		{
			return Empty<int>();
		}
		if (count < 0 || (long)start + (long)count - 1 > int.MaxValue)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException("count");
		}
		return Impl(start, count);
		static async IAsyncEnumerable<int> Impl(int num2, int num)
		{
			for (int i = 0; i < num; i++)
			{
				yield return num2 + i;
			}
		}
	}

	public static IAsyncEnumerable<TResult> Repeat<TResult>(TResult element, int count)
	{
		if (count == 0)
		{
			return Empty<TResult>();
		}
		ThrowHelper.ThrowIfNegative(count, "count");
		return Impl(element, count);
		static async IAsyncEnumerable<TResult> Impl(TResult val, int num)
		{
			while (num-- != 0)
			{
				yield return val;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Reverse<TSource>(this IAsyncEnumerable<TSource> source)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			TSource[] array = await source2.ToArrayAsync(cancellationToken);
			for (int i = array.Length - 1; i >= 0; i--)
			{
				yield return array[i];
			}
		}
	}

	public static IAsyncEnumerable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter?, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(outer, "outer");
		System.ExceptionPolyfills.ThrowIfNull(inner, "inner");
		System.ExceptionPolyfills.ThrowIfNull(outerKeySelector, "outerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(innerKeySelector, "innerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!inner.IsKnownEmpty())
		{
			return Impl(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TOuter> source, IAsyncEnumerable<TInner> asyncEnumerable, Func<TOuter, TKey> keySelector, Func<TInner, TKey> func, Func<TOuter, TInner, TResult> func2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TInner> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					AsyncLookup<TKey, TOuter> outerLookup = await AsyncLookup<TKey, TOuter>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken);
					do
					{
						TInner item = e.Current;
						Grouping<TKey, TOuter> grouping = outerLookup.GetGrouping(func(item), create: false);
						if (grouping == null)
						{
							yield return func2(default(TOuter), item);
						}
						else
						{
							int count = grouping._count;
							TOuter[] elements = grouping._elements;
							int i = 0;
							while (i != count)
							{
								yield return func2(elements[i], item);
								int num2 = i + 1;
								i = num2;
							}
						}
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter?, TInner, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(outer, "outer");
		System.ExceptionPolyfills.ThrowIfNull(inner, "inner");
		System.ExceptionPolyfills.ThrowIfNull(outerKeySelector, "outerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(innerKeySelector, "innerKeySelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!inner.IsKnownEmpty())
		{
			return Impl(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TOuter> source, IAsyncEnumerable<TInner> asyncEnumerable, Func<TOuter, CancellationToken, ValueTask<TKey>> keySelector, Func<TInner, CancellationToken, ValueTask<TKey>> func, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> func2, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TInner> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				if (await e.MoveNextAsync())
				{
					AsyncLookup<TKey, TOuter> outerLookup = await AsyncLookup<TKey, TOuter>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken);
					do
					{
						TInner item = e.Current;
						AsyncLookup<TKey, TOuter> asyncLookup = outerLookup;
						Grouping<TKey, TOuter> grouping = asyncLookup.GetGrouping(await func(item, cancellationToken), create: false);
						if (grouping == null)
						{
							yield return await func2(default(TOuter), item, cancellationToken);
						}
						else
						{
							int count = grouping._count;
							TOuter[] elements = grouping._elements;
							int i = 0;
							while (i != count)
							{
								yield return await func2(elements[i], item, cancellationToken);
								int num2 = i + 1;
								i = num2;
							}
						}
					}
					while (await e.MoveNextAsync());
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, TResult> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					yield return func(current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					yield return await func(current, cancellationToken);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, TResult> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, TResult> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					yield return func(current, num2);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<TResult>> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					yield return await func(current, num2, cancellationToken);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, IEnumerable<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					foreach (TResult item in func(current))
					{
						yield return item;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IEnumerable<TResult>>> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<IEnumerable<TResult>>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					foreach (TResult item in await func(current, cancellationToken))
					{
						yield return item;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TResult>> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, IAsyncEnumerable<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					ConfiguredCancelableAsyncEnumerable<TResult>.Enumerator asyncEnumerator2 = func(current).WithCancellation(cancellationToken).GetAsyncEnumerator();
					object obj2 = null;
					int num2 = 0;
					try
					{
						while (await asyncEnumerator2.MoveNextAsync())
						{
							yield return asyncEnumerator2.Current;
						}
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await asyncEnumerator2.DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
					_ = num2;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			await asyncEnumerator.DisposeAsync();
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, IEnumerable<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					foreach (TResult item in func(current, num2))
					{
						yield return item;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<IEnumerable<TResult>>> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<IEnumerable<TResult>>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					foreach (TResult item in await func(current, num2, cancellationToken))
					{
						yield return item;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, IAsyncEnumerable<TResult>> selector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(selector, "selector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, selector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, IAsyncEnumerable<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					ConfiguredCancelableAsyncEnumerable<TResult>.Enumerator asyncEnumerator2 = func(current, num2).WithCancellation(cancellationToken).GetAsyncEnumerator();
					object obj2 = null;
					int num3 = 0;
					try
					{
						while (await asyncEnumerator2.MoveNextAsync())
						{
							yield return asyncEnumerator2.Current;
						}
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await asyncEnumerator2.DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
					_ = num3;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			await asyncEnumerator.DisposeAsync();
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(collectionSelector, "collectionSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, collectionSelector, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, IEnumerable<TCollection>> func, Func<TSource, TCollection, TResult> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					foreach (TCollection item in func(element))
					{
						yield return func2(element, item);
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(collectionSelector, "collectionSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, collectionSelector, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<IEnumerable<TCollection>>> func, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					foreach (TCollection item in await func(element, cancellationToken))
					{
						yield return await func2(element, item, cancellationToken);
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(collectionSelector, "collectionSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, collectionSelector, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, IAsyncEnumerable<TCollection>> func, Func<TSource, TCollection, TResult> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					ConfiguredCancelableAsyncEnumerable<TCollection>.Enumerator asyncEnumerator2 = func(element).WithCancellation(cancellationToken).GetAsyncEnumerator();
					object obj2 = null;
					int num2 = 0;
					try
					{
						while (await asyncEnumerator2.MoveNextAsync())
						{
							TCollection current = asyncEnumerator2.Current;
							yield return func2(element, current);
						}
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await asyncEnumerator2.DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
					_ = num2;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			await asyncEnumerator.DisposeAsync();
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(collectionSelector, "collectionSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, collectionSelector, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, IAsyncEnumerable<TCollection>> func, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					ConfiguredCancelableAsyncEnumerable<TCollection>.Enumerator asyncEnumerator2 = func(element).WithCancellation(cancellationToken).GetAsyncEnumerator();
					object obj2 = null;
					int num2 = 0;
					try
					{
						while (await asyncEnumerator2.MoveNextAsync())
						{
							TCollection current = asyncEnumerator2.Current;
							yield return await func2(element, current, cancellationToken);
						}
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await asyncEnumerator2.DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
					_ = num2;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			await asyncEnumerator.DisposeAsync();
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(collectionSelector, "collectionSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, collectionSelector, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, IEnumerable<TCollection>> func, Func<TSource, TCollection, TResult> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					foreach (TCollection item in func(element, num2))
					{
						yield return func2(element, item);
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<IEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(collectionSelector, "collectionSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, collectionSelector, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<IEnumerable<TCollection>>> func, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					foreach (TCollection item in await func(element, num2, cancellationToken))
					{
						yield return await func2(element, item, cancellationToken);
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, IAsyncEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(collectionSelector, "collectionSelector");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, collectionSelector, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, IAsyncEnumerable<TCollection>> func, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> func2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					ConfiguredCancelableAsyncEnumerable<TCollection>.Enumerator asyncEnumerator2 = func(element, num2).WithCancellation(cancellationToken).GetAsyncEnumerator();
					object obj2 = null;
					int num3 = 0;
					try
					{
						while (await asyncEnumerator2.MoveNextAsync())
						{
							TCollection current = asyncEnumerator2.Current;
							yield return await func2(element, current, cancellationToken);
						}
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await asyncEnumerator2.DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
					_ = num3;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			await asyncEnumerator.DisposeAsync();
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			_ = num;
		}
	}

	public static ValueTask<bool> SequenceEqualAsync<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		return Impl(first, second, comparer ?? EqualityComparer<TSource>.Default, cancellationToken);
		static async ValueTask<bool> Impl(IAsyncEnumerable<TSource> asyncEnumerable, IAsyncEnumerable<TSource> asyncEnumerable2, IEqualityComparer<TSource> equalityComparer, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e1 = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			bool result = default(bool);
			object obj4;
			try
			{
				IAsyncEnumerator<TSource> e2 = asyncEnumerable2.GetAsyncEnumerator(cancellationToken2);
				object obj2 = null;
				int num2 = 0;
				bool flag = default(bool);
				try
				{
					while (true)
					{
						if (!(await e1.MoveNextAsync()))
						{
							flag = !(await e2.MoveNextAsync());
							break;
						}
						if (!(await e2.MoveNextAsync()) || !equalityComparer.Equals(e1.Current, e2.Current))
						{
							flag = false;
							break;
						}
					}
					num2 = 1;
				}
				catch (object obj3)
				{
					obj2 = obj3;
				}
				if (e2 != null)
				{
					await e2.DisposeAsync();
				}
				obj4 = obj2;
				if (obj4 != null)
				{
					ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
				}
				if (num2 == 1)
				{
					result = flag;
					num = 1;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			if (e1 != null)
			{
				await e1.DisposeAsync();
			}
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> Shuffle<TSource>(this IAsyncEnumerable<TSource> source)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			TSource[] array = await source2.ToArrayAsync(cancellationToken);
			Random random = t_random ?? (t_random = new Random(Environment.TickCount ^ Environment.CurrentManagedThreadId));
			int num = array.Length;
			for (int i = 0; i < num - 1; i++)
			{
				int num2 = random.Next(i, num);
				if (num2 != i)
				{
					TSource val = array[i];
					array[i] = array[num2];
					array[num2] = val;
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				yield return array[j];
			}
		}
	}

	public static ValueTask<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result2 = default(TSource);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					ThrowHelper.ThrowNoElementsException();
				}
				TSource result = e.Current;
				if (await e.MoveNextAsync())
				{
					ThrowHelper.ThrowMoreThanOneElementException();
				}
				result2 = result;
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result2;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, bool> func, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						ThrowHelper.ThrowNoElementsException();
						result = default(TSource);
						break;
					}
					TSource result2 = e.Current;
					if (func(result2))
					{
						while (await e.MoveNextAsync())
						{
							if (func(e.Current))
							{
								ThrowHelper.ThrowMoreThanOneMatchException();
							}
						}
						result = result2;
						break;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						ThrowHelper.ThrowNoElementsException();
						result = default(TSource);
						break;
					}
					TSource result2 = e.Current;
					if (await func(result2, cancellationToken2))
					{
						while (await e.MoveNextAsync())
						{
							if (await func(e.Current, cancellationToken2))
							{
								ThrowHelper.ThrowMoreThanOneMatchException();
							}
						}
						result = result2;
						break;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleOrDefaultAsync(default(TSource), cancellationToken);
	}

	public static ValueTask<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source, defaultValue, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, TSource val, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					result = val;
				}
				else
				{
					TSource result2 = e.Current;
					if (await e.MoveNextAsync())
					{
						ThrowHelper.ThrowMoreThanOneElementException();
					}
					result = result2;
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource?> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleOrDefaultAsync(predicate, default(TSource), cancellationToken);
	}

	public static ValueTask<TSource?> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleOrDefaultAsync(predicate, default(TSource), cancellationToken);
	}

	public static ValueTask<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, defaultValue, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, bool> func, TSource val, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = val;
						break;
					}
					TSource result2 = e.Current;
					if (func(result2))
					{
						while (await e.MoveNextAsync())
						{
							if (func(e.Current))
							{
								ThrowHelper.ThrowMoreThanOneMatchException();
							}
						}
						result = result2;
						break;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<TSource> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, TSource defaultValue, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		return Impl(source, predicate, defaultValue, cancellationToken);
		static async ValueTask<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<bool>> func, TSource val, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			TSource result = default(TSource);
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = val;
						break;
					}
					TSource result2 = e.Current;
					if (await func(result2, cancellationToken2))
					{
						while (await e.MoveNextAsync())
						{
							if (await func(e.Current, cancellationToken2))
							{
								ThrowHelper.ThrowMoreThanOneMatchException();
							}
						}
						result = result2;
						break;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> Skip<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty())
		{
			if (count > 0)
			{
				return Impl(source, count, default(CancellationToken));
			}
			return source;
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, int num2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				while (true)
				{
					bool flag = num2 > 0;
					if (flag)
					{
						flag = await e.MoveNextAsync();
					}
					if (!flag)
					{
						break;
					}
					num2--;
				}
				if (num2 <= 0)
				{
					while (await e.MoveNextAsync())
					{
						yield return e.Current;
					}
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> SkipLast<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty())
		{
			if (count > 0)
			{
				return TakeRangeFromEndIterator(source, isStartIndexFromEnd: false, 0, isEndIndexFromEnd: true, count, default(CancellationToken));
			}
			return source;
		}
		return Empty<TSource>();
	}

	public static IAsyncEnumerable<TSource> SkipWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						num = 1;
						break;
					}
					TSource current = e.Current;
					if (!func(current))
					{
						yield return current;
						while (await e.MoveNextAsync())
						{
							yield return e.Current;
						}
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> SkipWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						num = 1;
						break;
					}
					TSource element = e.Current;
					if (!(await func(element, cancellationToken)))
					{
						yield return element;
						while (await e.MoveNextAsync())
						{
							yield return e.Current;
						}
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> SkipWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, int, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				int index = -1;
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						num = 1;
						break;
					}
					TSource current = e.Current;
					int num2 = checked(index + 1);
					index = num2;
					if (!func(current, num2))
					{
						yield return current;
						while (await e.MoveNextAsync())
						{
							yield return e.Current;
						}
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> SkipWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, int, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				int index = -1;
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						num = 1;
						break;
					}
					TSource element = e.Current;
					int num2 = checked(index + 1);
					index = num2;
					if (!(await func(element, num2, cancellationToken)))
					{
						yield return element;
						while (await e.MoveNextAsync())
						{
							yield return e.Current;
						}
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static ValueTask<int> SumAsync(this IAsyncEnumerable<int> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<int> Impl(ConfiguredCancelableAsyncEnumerable<int> configuredCancelableAsyncEnumerable)
		{
			int sum = 0;
			ConfiguredCancelableAsyncEnumerable<int>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					int current = asyncEnumerator.Current;
					sum = checked(sum + current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return sum;
		}
	}

	public static ValueTask<long> SumAsync(this IAsyncEnumerable<long> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<long> Impl(ConfiguredCancelableAsyncEnumerable<long> configuredCancelableAsyncEnumerable)
		{
			long sum = 0L;
			ConfiguredCancelableAsyncEnumerable<long>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					long current = asyncEnumerator.Current;
					sum = checked(sum + current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return sum;
		}
	}

	public static ValueTask<float> SumAsync(this IAsyncEnumerable<float> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<float> Impl(ConfiguredCancelableAsyncEnumerable<float> configuredCancelableAsyncEnumerable)
		{
			double sum = 0.0;
			ConfiguredCancelableAsyncEnumerable<float>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					float current = asyncEnumerator.Current;
					sum += (double)current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return (float)sum;
		}
	}

	public static ValueTask<double> SumAsync(this IAsyncEnumerable<double> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<double> Impl(ConfiguredCancelableAsyncEnumerable<double> configuredCancelableAsyncEnumerable)
		{
			double sum = 0.0;
			ConfiguredCancelableAsyncEnumerable<double>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					double current = asyncEnumerator.Current;
					sum += current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return sum;
		}
	}

	public static ValueTask<decimal> SumAsync(this IAsyncEnumerable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<decimal> Impl(ConfiguredCancelableAsyncEnumerable<decimal> configuredCancelableAsyncEnumerable)
		{
			decimal sum = 0m;
			ConfiguredCancelableAsyncEnumerable<decimal>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					decimal current = asyncEnumerator.Current;
					sum += current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return sum;
		}
	}

	public static ValueTask<int?> SumAsync(this IAsyncEnumerable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<int?> Impl(ConfiguredCancelableAsyncEnumerable<int?> configuredCancelableAsyncEnumerable)
		{
			int sum = 0;
			ConfiguredCancelableAsyncEnumerable<int?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					int? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						sum = checked(sum + current.GetValueOrDefault());
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return sum;
		}
	}

	public static ValueTask<long?> SumAsync(this IAsyncEnumerable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<long?> Impl(ConfiguredCancelableAsyncEnumerable<long?> configuredCancelableAsyncEnumerable)
		{
			long sum = 0L;
			ConfiguredCancelableAsyncEnumerable<long?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					long? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						sum = checked(sum + current.GetValueOrDefault());
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return sum;
		}
	}

	public static ValueTask<float?> SumAsync(this IAsyncEnumerable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<float?> Impl(ConfiguredCancelableAsyncEnumerable<float?> configuredCancelableAsyncEnumerable)
		{
			double sum = 0.0;
			ConfiguredCancelableAsyncEnumerable<float?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					float? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						sum += (double)current.GetValueOrDefault();
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return (float)sum;
		}
	}

	public static ValueTask<double?> SumAsync(this IAsyncEnumerable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<double?> Impl(ConfiguredCancelableAsyncEnumerable<double?> configuredCancelableAsyncEnumerable)
		{
			double sum = 0.0;
			ConfiguredCancelableAsyncEnumerable<double?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					double? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						sum += current.GetValueOrDefault();
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return sum;
		}
	}

	public static ValueTask<decimal?> SumAsync(this IAsyncEnumerable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<decimal?> Impl(ConfiguredCancelableAsyncEnumerable<decimal?> configuredCancelableAsyncEnumerable)
		{
			decimal sum = 0m;
			ConfiguredCancelableAsyncEnumerable<decimal?>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					decimal? current = asyncEnumerator.Current;
					if (current.HasValue)
					{
						sum += current.GetValueOrDefault();
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return sum;
		}
	}

	public static IAsyncEnumerable<TSource> Take<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty() && count > 0)
		{
			return Impl(source, count, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, int num3, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					yield return asyncEnumerator.Current;
					int num2 = num3 - 1;
					num3 = num2;
					if (num2 == 0)
					{
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> Take<TSource>(this IAsyncEnumerable<TSource> source, Range range)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (source.IsKnownEmpty())
		{
			return Empty<TSource>();
		}
		Index start = range.Start;
		Index end = range.End;
		bool isFromEnd = start.IsFromEnd;
		bool isFromEnd2 = end.IsFromEnd;
		int value = start.Value;
		int value2 = end.Value;
		if (isFromEnd)
		{
			if (value == 0 || (isFromEnd2 && value2 >= value))
			{
				return Empty<TSource>();
			}
		}
		else if (!isFromEnd2)
		{
			if (value < value2)
			{
				return Impl(source, value, value2, default(CancellationToken));
			}
			return Empty<TSource>();
		}
		return TakeRangeFromEndIterator(source, isFromEnd, value, isFromEnd2, value2, default(CancellationToken));
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> asyncEnumerable, int startIndex, int endIndex, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				int index = 0;
				while (true)
				{
					bool flag = index < startIndex;
					if (flag)
					{
						flag = await e.MoveNextAsync();
					}
					if (!flag)
					{
						break;
					}
					int num2 = index + 1;
					index = num2;
				}
				if (index >= startIndex)
				{
					while (true)
					{
						bool flag = index < endIndex;
						if (flag)
						{
							flag = await e.MoveNextAsync();
						}
						if (!flag)
						{
							break;
						}
						yield return e.Current;
						int num2 = index + 1;
						index = num2;
					}
					num = 1;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	private static async IAsyncEnumerable<TSource> TakeRangeFromEndIterator<TSource>(IAsyncEnumerable<TSource> source, bool isStartIndexFromEnd, int startIndex, bool isEndIndexFromEnd, int endIndex, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		IAsyncEnumerator<TSource> e;
		object obj;
		int num;
		object obj3;
		if (isStartIndexFromEnd)
		{
			e = source.GetAsyncEnumerator(cancellationToken);
			obj = null;
			num = 0;
			Queue<TSource> queue = default(Queue<TSource>);
			int count = default(int);
			try
			{
				if (await e.MoveNextAsync())
				{
					queue = new Queue<TSource>();
					queue.Enqueue(e.Current);
					count = 1;
					while (await e.MoveNextAsync())
					{
						if (count < startIndex)
						{
							queue.Enqueue(e.Current);
							int num2 = count + 1;
							count = num2;
							continue;
						}
						do
						{
							queue.Dequeue();
							queue.Enqueue(e.Current);
							int num2 = checked(count + 1);
							count = num2;
						}
						while (await e.MoveNextAsync());
						break;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			startIndex = CalculateStartIndexFromEnd(startIndex, count);
			endIndex = CalculateEndIndex(isEndIndexFromEnd, endIndex, count);
			for (num = startIndex; num < endIndex; num++)
			{
				yield return queue.Dequeue();
			}
			yield break;
		}
		e = source.GetAsyncEnumerator(cancellationToken);
		obj = null;
		num = 0;
		try
		{
			int count = 0;
			while (true)
			{
				bool flag = count < startIndex;
				if (flag)
				{
					flag = await e.MoveNextAsync();
				}
				if (!flag)
				{
					break;
				}
				int num2 = count + 1;
				count = num2;
			}
			if (count == startIndex)
			{
				Queue<TSource> queue = new Queue<TSource>();
				while (await e.MoveNextAsync())
				{
					if (queue.Count == endIndex)
					{
						do
						{
							queue.Enqueue(e.Current);
							yield return queue.Dequeue();
						}
						while (await e.MoveNextAsync());
						break;
					}
					queue.Enqueue(e.Current);
				}
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		if (e != null)
		{
			await e.DisposeAsync();
		}
		obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		_ = num;
		static int CalculateEndIndex(bool flag2, int num4, int num3)
		{
			return Math.Min(num3, flag2 ? (num3 - num4) : num4);
		}
		static int CalculateStartIndexFromEnd(int num4, int num3)
		{
			return Math.Max(0, num3 - num4);
		}
	}

	public static IAsyncEnumerable<TSource> TakeLast<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!source.IsKnownEmpty() && count > 0)
		{
			return TakeRangeFromEndIterator(source, isStartIndexFromEnd: true, count, isEndIndexFromEnd: true, 0, default(CancellationToken));
		}
		return Empty<TSource>();
	}

	public static IAsyncEnumerable<TSource> TakeWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (!func(current))
					{
						break;
					}
					yield return current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> TakeWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					if (!(await func(element, cancellationToken)))
					{
						break;
					}
					yield return element;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> TakeWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					if (!func(current, num2))
					{
						break;
					}
					yield return current;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> TakeWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					if (!(await func(element, num2, cancellationToken)))
					{
						break;
					}
					yield return element;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static ValueTask<TSource[]> ToArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<TSource[]> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			TSource[] result = default(TSource[]);
			try
			{
				if (await e.MoveNextAsync())
				{
					List<TSource> list = new List<TSource>();
					do
					{
						list.Add(e.Current);
					}
					while (await e.MoveNextAsync());
					result = list.ToArray();
				}
				else
				{
					result = Array.Empty<TSource>();
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await e.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IEnumerable<TSource> source)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		if (!(source is TSource[] array))
		{
			if (!(source is List<TSource> source2))
			{
				if (source is IList<TSource> source3)
				{
					return FromIList(source3);
				}
				if (source == Enumerable.Empty<TSource>())
				{
					return Empty<TSource>();
				}
				return FromIterator(source);
			}
			return FromList(source2);
		}
		return (array.Length == 0) ? Empty<TSource>() : FromArray(array);
		static async IAsyncEnumerable<TSource> FromArray(TSource[] array2)
		{
			int i = 0;
			while (true)
			{
				int num = i;
				if ((uint)num >= (uint)array2.Length)
				{
					break;
				}
				yield return array2[num];
				i++;
			}
		}
		static async IAsyncEnumerable<TSource> FromIList(IList<TSource> list)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				yield return list[i];
			}
		}
		static async IAsyncEnumerable<TSource> FromIterator(IEnumerable<TSource> enumerable)
		{
			foreach (TSource item in enumerable)
			{
				yield return item;
			}
		}
		static async IAsyncEnumerable<TSource> FromList(List<TSource> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				yield return list[i];
			}
		}
	}

	public static ValueTask<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IAsyncEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken), comparer);
		static async ValueTask<Dictionary<TKey, TValue>> Impl(ConfiguredCancelableAsyncEnumerable<KeyValuePair<TKey, TValue>> configuredCancelableAsyncEnumerable, IEqualityComparer<TKey> comparer2)
		{
			Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>(comparer2);
			ConfiguredCancelableAsyncEnumerable<KeyValuePair<TKey, TValue>>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					KeyValuePair<TKey, TValue> current = asyncEnumerator.Current;
					d.Add(current.Key, current.Value);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return d;
		}
	}

	public static ValueTask<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IAsyncEnumerable<(TKey Key, TValue Value)> source, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAsync<(TKey, TValue), TKey, TValue>(((TKey Key, TValue Value) vt) => vt.Key, ((TKey Key, TValue Value) vt) => vt.Value, comparer, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		return Impl(source.WithCancellation(cancellationToken), keySelector, comparer);
		static async ValueTask<Dictionary<TKey, TSource>> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, TKey> func, IEqualityComparer<TKey> comparer2)
		{
			Dictionary<TKey, TSource> d = new Dictionary<TKey, TSource>(comparer2);
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					d.Add(func(current), current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return d;
		}
	}

	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		return Impl(source, keySelector, comparer, cancellationToken);
		static async ValueTask<Dictionary<TKey, TSource>> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey> comparer2, CancellationToken cancellationToken2)
		{
			Dictionary<TKey, TSource> d = new Dictionary<TKey, TSource>(comparer2);
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					Dictionary<TKey, TSource> dictionary = d;
					dictionary.Add(await func(element, cancellationToken2), element);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return d;
		}
	}

	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(elementSelector, "elementSelector");
		return Impl(source.WithCancellation(cancellationToken), keySelector, elementSelector, comparer);
		static async ValueTask<Dictionary<TKey, TElement>> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, TKey> func, Func<TSource, TElement> func2, IEqualityComparer<TKey> comparer2)
		{
			Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer2);
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					d.Add(func(current), func2(current));
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return d;
		}
	}

	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(elementSelector, "elementSelector");
		return Impl(source, keySelector, elementSelector, comparer, cancellationToken);
		static async ValueTask<Dictionary<TKey, TElement>> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> func, Func<TSource, CancellationToken, ValueTask<TElement>> func2, IEqualityComparer<TKey> comparer2, CancellationToken cancellationToken2)
		{
			Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer2);
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken2).GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					Dictionary<TKey, TElement> dictionary = d;
					dictionary.Add(await func(element, cancellationToken2), await func2(element, cancellationToken2));
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return d;
		}
	}

	public static ValueTask<HashSet<TSource>> ToHashSetAsync<TSource>(this IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken), comparer);
		static async ValueTask<HashSet<TSource>> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, IEqualityComparer<TSource> comparer2)
		{
			HashSet<TSource> set = new HashSet<TSource>(comparer2);
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					set.Add(current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return set;
		}
	}

	public static ValueTask<List<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		return Impl(source.WithCancellation(cancellationToken));
		static async ValueTask<List<TSource>> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable)
		{
			List<TSource> list = new List<TSource>();
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					list.Add(current);
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return list;
		}
	}

	public static ValueTask<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		return Impl(source.WithCancellation(cancellationToken), keySelector, comparer);
		static async ValueTask<ILookup<TKey, TSource>> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, TKey> func, IEqualityComparer<TKey> comparer2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			ILookup<TKey, TSource> result = default(ILookup<TKey, TSource>);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					result = EmptyLookup<TKey, TSource>.Instance;
				}
				else
				{
					AsyncLookup<TKey, TSource> lookup = new AsyncLookup<TKey, TSource>(comparer2);
					do
					{
						TSource current = e.Current;
						lookup.GetGrouping(func(current), create: true).Add(current);
					}
					while (await e.MoveNextAsync());
					result = lookup;
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await e.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		return Impl(source, keySelector, comparer, cancellationToken);
		static async ValueTask<ILookup<TKey, TSource>> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey> comparer2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			ILookup<TKey, TSource> result = default(ILookup<TKey, TSource>);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					result = EmptyLookup<TKey, TSource>.Instance;
				}
				else
				{
					AsyncLookup<TKey, TSource> lookup = new AsyncLookup<TKey, TSource>(comparer2);
					do
					{
						TSource item = e.Current;
						AsyncLookup<TKey, TSource> asyncLookup = lookup;
						asyncLookup.GetGrouping(await func(item, cancellationToken2), create: true).Add(item);
					}
					while (await e.MoveNextAsync());
					result = lookup;
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(elementSelector, "elementSelector");
		return Impl(source.WithCancellation(cancellationToken), keySelector, elementSelector, comparer);
		static async ValueTask<ILookup<TKey, TElement>> Impl(ConfiguredCancelableAsyncEnumerable<TSource> configuredCancelableAsyncEnumerable, Func<TSource, TKey> func, Func<TSource, TElement> func2, IEqualityComparer<TKey> comparer2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = configuredCancelableAsyncEnumerable.GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			ILookup<TKey, TElement> result = default(ILookup<TKey, TElement>);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					result = EmptyLookup<TKey, TElement>.Instance;
				}
				else
				{
					AsyncLookup<TKey, TElement> lookup = new AsyncLookup<TKey, TElement>(comparer2);
					do
					{
						TSource current = e.Current;
						lookup.GetGrouping(func(current), create: true).Add(func2(current));
					}
					while (await e.MoveNextAsync());
					result = lookup;
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await e.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static ValueTask<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		System.ExceptionPolyfills.ThrowIfNull(elementSelector, "elementSelector");
		return Impl(source, keySelector, elementSelector, comparer, cancellationToken);
		static async ValueTask<ILookup<TKey, TElement>> Impl(IAsyncEnumerable<TSource> asyncEnumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func, Func<TSource, CancellationToken, ValueTask<TElement>> func2, IEqualityComparer<TKey> comparer2, CancellationToken cancellationToken2)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator(cancellationToken2);
			object obj = null;
			int num = 0;
			ILookup<TKey, TElement> result = default(ILookup<TKey, TElement>);
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					result = EmptyLookup<TKey, TElement>.Instance;
				}
				else
				{
					AsyncLookup<TKey, TElement> lookup = new AsyncLookup<TKey, TElement>(comparer2);
					do
					{
						TSource item = e.Current;
						AsyncLookup<TKey, TElement> asyncLookup = lookup;
						Grouping<TKey, TElement> grouping = asyncLookup.GetGrouping(await func(item, cancellationToken2), create: true);
						grouping.Add(await func2(item, cancellationToken2));
					}
					while (await e.MoveNextAsync());
					result = lookup;
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num == 1)
			{
				return result;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> Union<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		if (!first.IsKnownEmpty() || !second.IsKnownEmpty())
		{
			return Impl(first, second, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source, IAsyncEnumerable<TSource> source2, IEqualityComparer<TSource> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			HashSet<TSource> set = new HashSet<TSource>(comparer2);
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (set.Add(current))
					{
						yield return current;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			obj = null;
			num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current2 = asyncEnumerator.Current;
					if (set.Add(current2))
					{
						yield return current2;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> UnionBy<TSource, TKey>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!first.IsKnownEmpty() || !second.IsKnownEmpty())
		{
			return Impl(first, second, keySelector, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source, IAsyncEnumerable<TSource> source2, Func<TSource, TKey> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			HashSet<TKey> set = new HashSet<TKey>(comparer2);
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (set.Add(func(current)))
					{
						yield return current;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			obj = null;
			num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current2 = asyncEnumerator.Current;
					if (set.Add(func(current2)))
					{
						yield return current2;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> UnionBy<TSource, TKey>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer = null)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(keySelector, "keySelector");
		if (!first.IsKnownEmpty() || !second.IsKnownEmpty())
		{
			return Impl(first, second, keySelector, comparer, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source, IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey> comparer2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			HashSet<TKey> set = new HashSet<TKey>(comparer2);
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					HashSet<TKey> hashSet = set;
					if (hashSet.Add(await func(element, cancellationToken)))
					{
						yield return element;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
			asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			obj = null;
			num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					HashSet<TKey> hashSet = set;
					if (hashSet.Add(await func(element, cancellationToken)))
					{
						yield return element;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					if (func(current))
					{
						yield return current;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					if (await func(element, cancellationToken))
					{
						yield return element;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource current = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					if (func(current, num2))
					{
						yield return current;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		System.ExceptionPolyfills.ThrowIfNull(source, "source");
		System.ExceptionPolyfills.ThrowIfNull(predicate, "predicate");
		if (!source.IsKnownEmpty())
		{
			return Impl(source, predicate, default(CancellationToken));
		}
		return Empty<TSource>();
		static async IAsyncEnumerable<TSource> Impl(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			int index = -1;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator asyncEnumerator = source2.WithCancellation(cancellationToken).GetAsyncEnumerator();
			object obj = null;
			int num = 0;
			try
			{
				while (await asyncEnumerator.MoveNextAsync())
				{
					TSource element = asyncEnumerator.Current;
					int num2 = checked(index + 1);
					index = num2;
					if (await func(element, num2, cancellationToken))
					{
						yield return element;
					}
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			await asyncEnumerator.DisposeAsync();
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			_ = num;
		}
	}

	public static IAsyncEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!first.IsKnownEmpty() && !second.IsKnownEmpty())
		{
			return Impl(first, second, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TFirst> asyncEnumerable, IAsyncEnumerable<TSecond> asyncEnumerable2, Func<TFirst, TSecond, TResult> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TFirst> e1 = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				IAsyncEnumerator<TSecond> e2 = asyncEnumerable2.GetAsyncEnumerator(cancellationToken);
				object obj2 = null;
				int num2 = 0;
				try
				{
					while (true)
					{
						bool flag = await e1.MoveNextAsync();
						if (flag)
						{
							flag = await e2.MoveNextAsync();
						}
						if (!flag)
						{
							break;
						}
						yield return func(e1.Current, e2.Current);
					}
					num2 = 1;
				}
				catch (object obj3)
				{
					obj2 = obj3;
				}
				if (e2 != null)
				{
					await e2.DisposeAsync();
				}
				obj4 = obj2;
				if (obj4 != null)
				{
					ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
				}
				if (num2 == 1)
				{
					num = 1;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			if (e1 != null)
			{
				await e1.DisposeAsync();
			}
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(resultSelector, "resultSelector");
		if (!first.IsKnownEmpty() && !second.IsKnownEmpty())
		{
			return Impl(first, second, resultSelector, default(CancellationToken));
		}
		return Empty<TResult>();
		static async IAsyncEnumerable<TResult> Impl(IAsyncEnumerable<TFirst> asyncEnumerable, IAsyncEnumerable<TSecond> asyncEnumerable2, Func<TFirst, TSecond, CancellationToken, ValueTask<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TFirst> e1 = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				IAsyncEnumerator<TSecond> e2 = asyncEnumerable2.GetAsyncEnumerator(cancellationToken);
				object obj2 = null;
				int num2 = 0;
				try
				{
					while (true)
					{
						bool flag = await e1.MoveNextAsync();
						if (flag)
						{
							flag = await e2.MoveNextAsync();
						}
						if (!flag)
						{
							break;
						}
						yield return await func(e1.Current, e2.Current, cancellationToken);
					}
					num2 = 1;
				}
				catch (object obj3)
				{
					obj2 = obj3;
				}
				if (e2 != null)
				{
					await e2.DisposeAsync();
				}
				obj4 = obj2;
				if (obj4 != null)
				{
					ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
				}
				if (num2 == 1)
				{
					num = 1;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			if (e1 != null)
			{
				await e1.DisposeAsync();
			}
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		if (!first.IsKnownEmpty() && !second.IsKnownEmpty())
		{
			return Impl(first, second, default(CancellationToken));
		}
		return Empty<(TFirst, TSecond)>();
		static async IAsyncEnumerable<(TFirst First, TSecond Second)> Impl(IAsyncEnumerable<TFirst> asyncEnumerable, IAsyncEnumerable<TSecond> asyncEnumerable2, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TFirst> e1 = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			object obj4;
			try
			{
				IAsyncEnumerator<TSecond> e2 = asyncEnumerable2.GetAsyncEnumerator(cancellationToken);
				object obj2 = null;
				int num2 = 0;
				try
				{
					while (true)
					{
						bool flag = await e1.MoveNextAsync();
						if (flag)
						{
							flag = await e2.MoveNextAsync();
						}
						if (!flag)
						{
							break;
						}
						yield return (First: e1.Current, Second: e2.Current);
					}
					num2 = 1;
				}
				catch (object obj3)
				{
					obj2 = obj3;
				}
				if (e2 != null)
				{
					await e2.DisposeAsync();
				}
				obj4 = obj2;
				if (obj4 != null)
				{
					ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
				}
				if (num2 == 1)
				{
					num = 1;
				}
			}
			catch (object obj3)
			{
				obj = obj3;
			}
			if (e1 != null)
			{
				await e1.DisposeAsync();
			}
			obj4 = obj;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<(TFirst First, TSecond Second, TThird Third)> Zip<TFirst, TSecond, TThird>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, IAsyncEnumerable<TThird> third)
	{
		System.ExceptionPolyfills.ThrowIfNull(first, "first");
		System.ExceptionPolyfills.ThrowIfNull(second, "second");
		System.ExceptionPolyfills.ThrowIfNull(third, "third");
		if (!first.IsKnownEmpty() && !second.IsKnownEmpty() && !third.IsKnownEmpty())
		{
			return Impl(first, second, third, default(CancellationToken));
		}
		return Empty<(TFirst, TSecond, TThird)>();
		static async IAsyncEnumerable<(TFirst First, TSecond Second, TThird)> Impl(IAsyncEnumerable<TFirst> asyncEnumerable, IAsyncEnumerable<TSecond> asyncEnumerable2, IAsyncEnumerable<TThird> asyncEnumerable3, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			IAsyncEnumerator<TFirst> e1 = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			object obj5;
			try
			{
				IAsyncEnumerator<TSecond> e2 = asyncEnumerable2.GetAsyncEnumerator(cancellationToken);
				object obj2 = null;
				int num2 = 0;
				try
				{
					IAsyncEnumerator<TThird> e3 = asyncEnumerable3.GetAsyncEnumerator(cancellationToken);
					object obj3 = null;
					int num3 = 0;
					try
					{
						while (true)
						{
							bool flag = await e1.MoveNextAsync();
							if (flag)
							{
								flag = await e2.MoveNextAsync();
							}
							bool flag2 = flag;
							if (flag2)
							{
								flag2 = await e3.MoveNextAsync();
							}
							if (!flag2)
							{
								break;
							}
							yield return (First: e1.Current, Second: e2.Current, e3.Current);
						}
						num3 = 1;
					}
					catch (object obj4)
					{
						obj3 = obj4;
					}
					if (e3 != null)
					{
						await e3.DisposeAsync();
					}
					obj5 = obj3;
					if (obj5 != null)
					{
						ExceptionDispatchInfo.Capture((obj5 as Exception) ?? throw obj5).Throw();
					}
					if (num3 == 1)
					{
						num2 = 1;
					}
				}
				catch (object obj4)
				{
					obj2 = obj4;
				}
				if (e2 != null)
				{
					await e2.DisposeAsync();
				}
				obj5 = obj2;
				if (obj5 != null)
				{
					ExceptionDispatchInfo.Capture((obj5 as Exception) ?? throw obj5).Throw();
				}
				if (num2 == 1)
				{
					num = 1;
				}
			}
			catch (object obj4)
			{
				obj = obj4;
			}
			if (e1 != null)
			{
				await e1.DisposeAsync();
			}
			obj5 = obj;
			if (obj5 != null)
			{
				ExceptionDispatchInfo.Capture((obj5 as Exception) ?? throw obj5).Throw();
			}
			if (num == 1)
			{
				yield break;
			}
			throw null;
		}
	}
}
