using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal abstract class OrderedAsyncEnumerable<TElement> : System.Linq.AsyncIterator<TElement>, IOrderedAsyncEnumerable<TElement>, IAsyncEnumerable<TElement>, IAsyncPartition<TElement>, IAsyncIListProvider<TElement>
{
	protected readonly IAsyncEnumerable<TElement> _source;

	private TElement[]? _buffer;

	private int[]? _indexes;

	private int _index;

	protected OrderedAsyncEnumerable(IAsyncEnumerable<TElement> source)
	{
		_source = source ?? throw System.Error.ArgumentNull("source");
	}

	IOrderedAsyncEnumerable<TElement> IOrderedAsyncEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey>? comparer, bool descending)
	{
		return new OrderedAsyncEnumerable<TElement, TKey>(_source, keySelector, comparer, descending, this);
	}

	IOrderedAsyncEnumerable<TElement> IOrderedAsyncEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, bool descending)
	{
		return new OrderedAsyncEnumerableWithTask<TElement, TKey>(_source, keySelector, comparer, descending, this);
	}

	IOrderedAsyncEnumerable<TElement> IOrderedAsyncEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, bool descending)
	{
		return new OrderedAsyncEnumerableWithTaskAndCancellation<TElement, TKey>(_source, keySelector, comparer, descending, this);
	}

	protected override async ValueTask<bool> MoveNextCore()
	{
		System.Linq.AsyncIteratorState state = _state;
		if (state != System.Linq.AsyncIteratorState.Allocated)
		{
			if (state != System.Linq.AsyncIteratorState.Iterating)
			{
				goto IL_0201;
			}
		}
		else
		{
			_buffer = await AsyncEnumerable.ToArrayAsync(_source, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			_indexes = await GetAsyncEnumerableSorter(_cancellationToken).Sort(_buffer, _buffer.Length).ConfigureAwait(continueOnCapturedContext: false);
			_index = 0;
			_state = System.Linq.AsyncIteratorState.Iterating;
		}
		if (_index < _buffer.Length)
		{
			_current = _buffer[_indexes[_index++]];
			return true;
		}
		await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		goto IL_0201;
		IL_0201:
		return false;
	}

	public override async ValueTask DisposeAsync()
	{
		_buffer = null;
		_indexes = null;
		await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	internal abstract AsyncEnumerableSorter<TElement> GetAsyncEnumerableSorter(AsyncEnumerableSorter<TElement>? next, CancellationToken cancellationToken);

	internal AsyncEnumerableSorter<TElement> GetAsyncEnumerableSorter(CancellationToken cancellationToken)
	{
		return GetAsyncEnumerableSorter(null, cancellationToken);
	}

	public async ValueTask<TElement[]> ToArrayAsync(CancellationToken cancellationToken)
	{
		AsyncEnumerableHelpers.ArrayWithLength<TElement> arrayWithLength = await AsyncEnumerableHelpers.ToArrayWithLength(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int count = arrayWithLength.Length;
		if (count == 0)
		{
			return Array.Empty<TElement>();
		}
		TElement[] array = arrayWithLength.Array;
		int[] array2 = await SortedMap(array, count, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		TElement[] array3 = new TElement[count];
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i] = array[array2[i]];
		}
		return array3;
	}

	internal async ValueTask<TElement[]> ToArrayAsync(int minIndexInclusive, int maxIndexInclusive, CancellationToken cancellationToken)
	{
		AsyncEnumerableHelpers.ArrayWithLength<TElement> arrayWithLength = await AsyncEnumerableHelpers.ToArrayWithLength(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int length = arrayWithLength.Length;
		if (length <= minIndexInclusive)
		{
			return Array.Empty<TElement>();
		}
		if (length <= maxIndexInclusive)
		{
			maxIndexInclusive = length - 1;
		}
		TElement[] array = arrayWithLength.Array;
		if (minIndexInclusive == maxIndexInclusive)
		{
			TElement val = await GetAsyncEnumerableSorter(cancellationToken).ElementAt(array, length, minIndexInclusive).ConfigureAwait(continueOnCapturedContext: false);
			return new TElement[1] { val };
		}
		int[] array2 = await SortedMap(array, length, minIndexInclusive, maxIndexInclusive, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		TElement[] array3 = new TElement[maxIndexInclusive - minIndexInclusive + 1];
		int num = 0;
		while (minIndexInclusive <= maxIndexInclusive)
		{
			array3[num] = array[array2[minIndexInclusive++]];
			num++;
		}
		return array3;
	}

	public async ValueTask<List<TElement>> ToListAsync(CancellationToken cancellationToken)
	{
		AsyncEnumerableHelpers.ArrayWithLength<TElement> arrayWithLength = await AsyncEnumerableHelpers.ToArrayWithLength(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int count = arrayWithLength.Length;
		if (count == 0)
		{
			return new List<TElement>();
		}
		TElement[] array = arrayWithLength.Array;
		int[] array2 = await SortedMap(array, count, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		List<TElement> list = new List<TElement>(count);
		for (int i = 0; i < count; i++)
		{
			list.Add(array[array2[i]]);
		}
		return list;
	}

	internal async ValueTask<List<TElement>> ToListAsync(int minIndexInclusive, int maxIndexInclusive, CancellationToken cancellationToken)
	{
		AsyncEnumerableHelpers.ArrayWithLength<TElement> arrayWithLength = await AsyncEnumerableHelpers.ToArrayWithLength(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int length = arrayWithLength.Length;
		if (length <= minIndexInclusive)
		{
			return new List<TElement>();
		}
		if (length <= maxIndexInclusive)
		{
			maxIndexInclusive = length - 1;
		}
		TElement[] array = arrayWithLength.Array;
		if (minIndexInclusive == maxIndexInclusive)
		{
			TElement item = await GetAsyncEnumerableSorter(cancellationToken).ElementAt(array, length, minIndexInclusive).ConfigureAwait(continueOnCapturedContext: false);
			return new List<TElement>(1) { item };
		}
		int[] array2 = await SortedMap(array, length, minIndexInclusive, maxIndexInclusive, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		List<TElement> list = new List<TElement>(maxIndexInclusive - minIndexInclusive + 1);
		while (minIndexInclusive <= maxIndexInclusive)
		{
			list.Add(array[array2[minIndexInclusive++]]);
		}
		return list;
	}

	public async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
	{
		if (_source is IAsyncIListProvider<TElement> asyncIListProvider)
		{
			return await asyncIListProvider.GetCountAsync(onlyIfCheap, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return (onlyIfCheap && !(_source is ICollection<TElement>) && !(_source is ICollection)) ? (-1) : (await AsyncEnumerable.CountAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal async ValueTask<int> GetCountAsync(int minIndexInclusive, int maxIndexInclusive, bool onlyIfCheap, CancellationToken cancellationToken)
	{
		int num = await GetCountAsync(onlyIfCheap, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (num <= 0)
		{
			return num;
		}
		if (num <= minIndexInclusive)
		{
			return 0;
		}
		return ((num <= maxIndexInclusive) ? num : (maxIndexInclusive + 1)) - minIndexInclusive;
	}

	private ValueTask<int[]> SortedMap(TElement[] elements, int count, CancellationToken cancellationToken)
	{
		return GetAsyncEnumerableSorter(cancellationToken).Sort(elements, count);
	}

	private ValueTask<int[]> SortedMap(TElement[] elements, int count, int minIndexInclusive, int maxIndexInclusive, CancellationToken cancellationToken)
	{
		return GetAsyncEnumerableSorter(cancellationToken).Sort(elements, count, minIndexInclusive, maxIndexInclusive);
	}

	private AsyncCachingComparer<TElement> GetComparer()
	{
		return GetComparer(null);
	}

	internal abstract AsyncCachingComparer<TElement> GetComparer(AsyncCachingComparer<TElement>? childComparer);

	public async ValueTask<Maybe<TElement>> TryGetFirstAsync(CancellationToken cancellationToken)
	{
		ConfiguredCancelableAsyncEnumerable<TElement>.Enumerator e = _source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		Maybe<TElement> result;
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				result = default(Maybe<TElement>);
			}
			else
			{
				TElement value = e.Current;
				AsyncCachingComparer<TElement> comparer = GetComparer();
				await comparer.SetElement(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					TElement x = e.Current;
					if (await comparer.Compare(x, cacheLower: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) < 0)
					{
						value = x;
					}
				}
				result = new Maybe<TElement>(value);
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		return result;
	}

	public async ValueTask<Maybe<TElement>> TryGetLastAsync(CancellationToken cancellationToken)
	{
		ConfiguredCancelableAsyncEnumerable<TElement>.Enumerator e = _source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		Maybe<TElement> result;
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				result = default(Maybe<TElement>);
			}
			else
			{
				TElement value = e.Current;
				AsyncCachingComparer<TElement> comparer = GetComparer();
				await comparer.SetElement(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					TElement current = e.Current;
					if (await comparer.Compare(current, cacheLower: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) >= 0)
					{
						value = current;
					}
				}
				result = new Maybe<TElement>(value);
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		return result;
	}

	internal async ValueTask<Maybe<TElement>> TryGetLastAsync(int minIndexInclusive, int maxIndexInclusive, CancellationToken cancellationToken)
	{
		AsyncEnumerableHelpers.ArrayWithLength<TElement> arrayWithLength = await AsyncEnumerableHelpers.ToArrayWithLength(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int length = arrayWithLength.Length;
		if (minIndexInclusive >= length)
		{
			return default(Maybe<TElement>);
		}
		TElement[] array = arrayWithLength.Array;
		TElement value = ((maxIndexInclusive >= length - 1) ? (await Last(array, length, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) : (await GetAsyncEnumerableSorter(cancellationToken).ElementAt(array, length, maxIndexInclusive).ConfigureAwait(continueOnCapturedContext: false)));
		return new Maybe<TElement>(value);
	}

	private async ValueTask<TElement> Last(TElement[] items, int count, CancellationToken cancellationToken)
	{
		TElement value = items[0];
		AsyncCachingComparer<TElement> comparer = GetComparer();
		await comparer.SetElement(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int i = 1;
		while (i != count)
		{
			TElement x = items[i];
			if (await comparer.Compare(x, cacheLower: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) >= 0)
			{
				value = x;
			}
			int num = i + 1;
			i = num;
		}
		return value;
	}

	public IAsyncPartition<TElement> Skip(int count)
	{
		return new OrderedAsyncPartition<TElement>(this, count, int.MaxValue);
	}

	public IAsyncPartition<TElement> Take(int count)
	{
		return new OrderedAsyncPartition<TElement>(this, 0, count - 1);
	}

	public ValueTask<Maybe<TElement>> TryGetElementAtAsync(int index, CancellationToken cancellationToken)
	{
		if (index == 0)
		{
			return TryGetFirstAsync(cancellationToken);
		}
		if (index > 0)
		{
			return Core();
		}
		return new ValueTask<Maybe<TElement>>(default(Maybe<TElement>));
		async ValueTask<Maybe<TElement>> Core()
		{
			AsyncEnumerableHelpers.ArrayWithLength<TElement> arrayWithLength = await AsyncEnumerableHelpers.ToArrayWithLength(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			int length = arrayWithLength.Length;
			if (index < length)
			{
				return new Maybe<TElement>(await GetAsyncEnumerableSorter(cancellationToken).ElementAt(arrayWithLength.Array, length, index).ConfigureAwait(continueOnCapturedContext: false));
			}
			return default(Maybe<TElement>);
		}
	}
}
internal sealed class OrderedAsyncEnumerable<TElement, TKey> : OrderedAsyncEnumerable<TElement>
{
	private readonly IComparer<TKey> _comparer;

	private readonly bool _descending;

	private readonly Func<TElement, TKey> _keySelector;

	private readonly OrderedAsyncEnumerable<TElement>? _parent;

	public OrderedAsyncEnumerable(IAsyncEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey>? comparer, bool descending, OrderedAsyncEnumerable<TElement>? parent)
		: base(source)
	{
		_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
		_comparer = comparer ?? Comparer<TKey>.Default;
		_descending = descending;
		_parent = parent;
	}

	public override System.Linq.AsyncIteratorBase<TElement> Clone()
	{
		return new OrderedAsyncEnumerable<TElement, TKey>(_source, _keySelector, _comparer, _descending, _parent);
	}

	internal override AsyncEnumerableSorter<TElement> GetAsyncEnumerableSorter(AsyncEnumerableSorter<TElement>? next, CancellationToken cancellationToken)
	{
		SyncKeySelectorAsyncEnumerableSorter<TElement, TKey> syncKeySelectorAsyncEnumerableSorter = new SyncKeySelectorAsyncEnumerableSorter<TElement, TKey>(_keySelector, _comparer, _descending, next);
		if (_parent != null)
		{
			return _parent.GetAsyncEnumerableSorter(syncKeySelectorAsyncEnumerableSorter, cancellationToken);
		}
		return syncKeySelectorAsyncEnumerableSorter;
	}

	internal override AsyncCachingComparer<TElement> GetComparer(AsyncCachingComparer<TElement>? childComparer)
	{
		AsyncCachingComparer<TElement> asyncCachingComparer = ((childComparer == null) ? new AsyncCachingComparer<TElement, TKey>(_keySelector, _comparer, _descending) : new AsyncCachingComparerWithChild<TElement, TKey>(_keySelector, _comparer, _descending, childComparer));
		if (_parent == null)
		{
			return asyncCachingComparer;
		}
		return _parent.GetComparer(asyncCachingComparer);
	}
}
