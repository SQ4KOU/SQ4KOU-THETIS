using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal sealed class OrderedAsyncEnumerableWithTaskAndCancellation<TElement, TKey> : OrderedAsyncEnumerable<TElement>
{
	private readonly IComparer<TKey> _comparer;

	private readonly bool _descending;

	private readonly Func<TElement, CancellationToken, ValueTask<TKey>> _keySelector;

	private readonly OrderedAsyncEnumerable<TElement>? _parent;

	public OrderedAsyncEnumerableWithTaskAndCancellation(IAsyncEnumerable<TElement> source, Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, bool descending, OrderedAsyncEnumerable<TElement>? parent)
		: base(source)
	{
		_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
		_comparer = comparer ?? Comparer<TKey>.Default;
		_descending = descending;
		_parent = parent;
	}

	public override System.Linq.AsyncIteratorBase<TElement> Clone()
	{
		return new OrderedAsyncEnumerableWithTaskAndCancellation<TElement, TKey>(_source, _keySelector, _comparer, _descending, _parent);
	}

	internal override AsyncEnumerableSorter<TElement> GetAsyncEnumerableSorter(AsyncEnumerableSorter<TElement>? next, CancellationToken cancellationToken)
	{
		AsyncKeySelectorAsyncEnumerableSorterWithCancellation<TElement, TKey> asyncKeySelectorAsyncEnumerableSorterWithCancellation = new AsyncKeySelectorAsyncEnumerableSorterWithCancellation<TElement, TKey>(_keySelector, _comparer, _descending, next, cancellationToken);
		if (_parent != null)
		{
			return _parent.GetAsyncEnumerableSorter(asyncKeySelectorAsyncEnumerableSorterWithCancellation, cancellationToken);
		}
		return asyncKeySelectorAsyncEnumerableSorterWithCancellation;
	}

	internal override AsyncCachingComparer<TElement> GetComparer(AsyncCachingComparer<TElement>? childComparer)
	{
		AsyncCachingComparer<TElement> asyncCachingComparer = ((childComparer == null) ? new AsyncCachingComparerWithTaskAndCancellation<TElement, TKey>(_keySelector, _comparer, _descending) : new AsyncCachingComparerWithTaskAndCancellationAndChild<TElement, TKey>(_keySelector, _comparer, _descending, childComparer));
		if (_parent == null)
		{
			return asyncCachingComparer;
		}
		return _parent.GetComparer(asyncCachingComparer);
	}
}
