using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq.Internal;

internal class LookupWithTask<TKey, TElement> : ILookup<TKey, TElement>, IEnumerable<IGrouping<TKey, TElement>>, IEnumerable, IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>, IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>
{
	private readonly IEqualityComparer<TKey> _comparer;

	private Grouping<TKey, TElement>[] _groupings;

	private Grouping<TKey, TElement>? _lastGrouping;

	public int Count { get; private set; }

	public IEnumerable<TElement> this[TKey key]
	{
		get
		{
			Grouping<TKey, TElement> grouping = GetGrouping(key);
			if (grouping != null)
			{
				return grouping;
			}
			return Array.Empty<TElement>();
		}
	}

	private LookupWithTask(IEqualityComparer<TKey>? comparer)
	{
		_comparer = comparer ?? EqualityComparer<TKey>.Default;
		_groupings = new Grouping<TKey, TElement>[7];
	}

	public bool Contains(TKey key)
	{
		return GetGrouping(key) != null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
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

	internal static async Task<LookupWithTask<TKey, TElement>> CreateAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		LookupWithTask<TKey, TElement> lookup = new LookupWithTask<TKey, TElement>(comparer);
		await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			Grouping<TKey, TElement> group = lookup.GetOrCreateGrouping(await keySelector(item).ConfigureAwait(continueOnCapturedContext: false));
			group.Add(await elementSelector(item).ConfigureAwait(continueOnCapturedContext: false));
		}
		return lookup;
	}

	internal static async Task<LookupWithTask<TKey, TElement>> CreateAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		LookupWithTask<TKey, TElement> lookup = new LookupWithTask<TKey, TElement>(comparer);
		await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			Grouping<TKey, TElement> group = lookup.GetOrCreateGrouping(await keySelector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			group.Add(await elementSelector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return lookup;
	}

	internal static async Task<LookupWithTask<TKey, TElement>> CreateAsync(IAsyncEnumerable<TElement> source, Func<TElement, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		LookupWithTask<TKey, TElement> lookup = new LookupWithTask<TKey, TElement>(comparer);
		await foreach (TElement item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			lookup.GetOrCreateGrouping(await keySelector(item).ConfigureAwait(continueOnCapturedContext: false)).Add(item);
		}
		return lookup;
	}

	internal static async Task<LookupWithTask<TKey, TElement>> CreateAsync(IAsyncEnumerable<TElement> source, Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		LookupWithTask<TKey, TElement> lookup = new LookupWithTask<TKey, TElement>(comparer);
		await foreach (TElement item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			lookup.GetOrCreateGrouping(await keySelector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Add(item);
		}
		return lookup;
	}

	internal static async Task<LookupWithTask<TKey, TElement>> CreateForJoinAsync(IAsyncEnumerable<TElement> source, Func<TElement, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		LookupWithTask<TKey, TElement> lookup = new LookupWithTask<TKey, TElement>(comparer);
		await foreach (TElement item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			TKey val = await keySelector(item).ConfigureAwait(continueOnCapturedContext: false);
			if (val != null)
			{
				lookup.GetOrCreateGrouping(val).Add(item);
			}
		}
		return lookup;
	}

	internal static async Task<LookupWithTask<TKey, TElement>> CreateForJoinAsync(IAsyncEnumerable<TElement> source, Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		LookupWithTask<TKey, TElement> lookup = new LookupWithTask<TKey, TElement>(comparer);
		await foreach (TElement item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			TKey val = await keySelector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (val != null)
			{
				lookup.GetOrCreateGrouping(val).Add(item);
			}
		}
		return lookup;
	}

	internal Grouping<TKey, TElement>? GetGrouping(TKey key)
	{
		int hashCode = InternalGetHashCode(key);
		return GetGrouping(key, hashCode);
	}

	internal Grouping<TKey, TElement>? GetGrouping(TKey key, int hashCode)
	{
		for (Grouping<TKey, TElement> grouping = _groupings[hashCode % _groupings.Length]; grouping != null; grouping = grouping._hashNext)
		{
			if (grouping._hashCode == hashCode && _comparer.Equals(grouping._key, key))
			{
				return grouping;
			}
		}
		return null;
	}

	internal Grouping<TKey, TElement> GetOrCreateGrouping(TKey key)
	{
		int num = InternalGetHashCode(key);
		Grouping<TKey, TElement> grouping = GetGrouping(key, num);
		if (grouping != null)
		{
			return grouping;
		}
		if (Count == _groupings.Length)
		{
			Resize();
		}
		int num2 = num % _groupings.Length;
		Grouping<TKey, TElement> grouping2 = new Grouping<TKey, TElement>(key, num, new TElement[1], _groupings[num2]);
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
		Count++;
		return grouping2;
	}

	internal int InternalGetHashCode(TKey key)
	{
		if (key != null)
		{
			return _comparer.GetHashCode(key) & 0x7FFFFFFF;
		}
		return 0;
	}

	internal async Task<TResult[]> ToArray<TResult>(Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector)
	{
		TResult[] array = new TResult[Count];
		int index = 0;
		Grouping<TKey, TElement> g = _lastGrouping;
		if (g != null)
		{
			do
			{
				g = g._next;
				g.Trim();
				TResult[] array2 = array;
				int num = index;
				array2[num] = await resultSelector(g._key, AsyncEnumerable.ToAsyncEnumerable(g._elements)).ConfigureAwait(continueOnCapturedContext: false);
				int num2 = index + 1;
				index = num2;
			}
			while (g != _lastGrouping);
		}
		return array;
	}

	internal async Task<TResult[]> ToArray<TResult>(Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		TResult[] array = new TResult[Count];
		int index = 0;
		Grouping<TKey, TElement> g = _lastGrouping;
		if (g != null)
		{
			do
			{
				g = g._next;
				g.Trim();
				TResult[] array2 = array;
				int num = index;
				array2[num] = await resultSelector(g._key, AsyncEnumerable.ToAsyncEnumerable(g._elements), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				int num2 = index + 1;
				index = num2;
			}
			while (g != _lastGrouping);
		}
		return array;
	}

	internal async Task<List<TResult>> ToList<TResult>(Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector)
	{
		List<TResult> list = new List<TResult>(Count);
		Grouping<TKey, TElement> g = _lastGrouping;
		if (g != null)
		{
			do
			{
				g = g._next;
				g.Trim();
				list.Add(await resultSelector(g._key, AsyncEnumerable.ToAsyncEnumerable(g._elements)).ConfigureAwait(continueOnCapturedContext: false));
			}
			while (g != _lastGrouping);
		}
		return list;
	}

	internal async Task<List<TResult>> ToList<TResult>(Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		List<TResult> list = new List<TResult>(Count);
		Grouping<TKey, TElement> g = _lastGrouping;
		if (g != null)
		{
			do
			{
				g = g._next;
				g.Trim();
				list.Add(await resultSelector(g._key, AsyncEnumerable.ToAsyncEnumerable(g._elements), cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
			while (g != _lastGrouping);
		}
		return list;
	}

	private void Resize()
	{
		int num = checked(Count * 2 + 1);
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

	public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
	{
		return new ValueTask<int>(Count);
	}

	IAsyncEnumerator<IAsyncGrouping<TKey, TElement>> IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>.GetAsyncEnumerator(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return AsyncEnumerable.ToAsyncEnumerable(Enumerable.Cast<IAsyncGrouping<TKey, TElement>>(this)).GetAsyncEnumerator(cancellationToken);
	}

	ValueTask<List<IAsyncGrouping<TKey, TElement>>> IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>.ToListAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		List<IAsyncGrouping<TKey, TElement>> list = new List<IAsyncGrouping<TKey, TElement>>(Count);
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = grouping._next;
				list.Add(grouping);
			}
			while (grouping != _lastGrouping);
		}
		return new ValueTask<List<IAsyncGrouping<TKey, TElement>>>(list);
	}

	ValueTask<IAsyncGrouping<TKey, TElement>[]> IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>.ToArrayAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		IAsyncGrouping<TKey, TElement>[] array = new IAsyncGrouping<TKey, TElement>[Count];
		int num = 0;
		Grouping<TKey, TElement> grouping = _lastGrouping;
		if (grouping != null)
		{
			do
			{
				grouping = (Grouping<TKey, TElement>)(array[num] = grouping._next);
				num++;
			}
			while (grouping != _lastGrouping);
		}
		return new ValueTask<IAsyncGrouping<TKey, TElement>[]>(array);
	}
}
