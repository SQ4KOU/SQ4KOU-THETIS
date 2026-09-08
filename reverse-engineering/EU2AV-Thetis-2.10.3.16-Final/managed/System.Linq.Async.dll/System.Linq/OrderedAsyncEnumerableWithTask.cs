using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal sealed class OrderedAsyncEnumerableWithTask<TElement, TKey> : OrderedAsyncEnumerable<TElement>
{
	private readonly IComparer<TKey> _comparer;

	private readonly bool _descending;

	private readonly Func<TElement, ValueTask<TKey>> _keySelector;

	private readonly OrderedAsyncEnumerable<TElement>? _parent;

	public OrderedAsyncEnumerableWithTask(IAsyncEnumerable<TElement> source, Func<TElement, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, bool descending, OrderedAsyncEnumerable<TElement>? parent)
		: base(source)
	{
		_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
		_comparer = comparer ?? Comparer<TKey>.Default;
		_descending = descending;
		_parent = parent;
	}

	public override System.Linq.AsyncIteratorBase<TElement> Clone()
	{
		return new OrderedAsyncEnumerableWithTask<TElement, TKey>(_source, _keySelector, _comparer, _descending, _parent);
	}

	internal override AsyncEnumerableSorter<TElement> GetAsyncEnumerableSorter(AsyncEnumerableSorter<TElement>? next, CancellationToken cancellationToken)
	{
		AsyncKeySelectorAsyncEnumerableSorter<TElement, TKey> asyncKeySelectorAsyncEnumerableSorter = new AsyncKeySelectorAsyncEnumerableSorter<TElement, TKey>(_keySelector, _comparer, _descending, next);
		if (_parent != null)
		{
			return _parent.GetAsyncEnumerableSorter(asyncKeySelectorAsyncEnumerableSorter, cancellationToken);
		}
		return asyncKeySelectorAsyncEnumerableSorter;
	}

	internal override AsyncCachingComparer<TElement> GetComparer(AsyncCachingComparer<TElement>? childComparer)
	{
		AsyncCachingComparer<TElement> asyncCachingComparer = ((childComparer == null) ? new AsyncCachingComparerWithTask<TElement, TKey>(_keySelector, _comparer, _descending) : new AsyncCachingComparerWithTaskAndChild<TElement, TKey>(_keySelector, _comparer, _descending, childComparer));
		if (_parent == null)
		{
			return asyncCachingComparer;
		}
		return _parent.GetComparer(asyncCachingComparer);
	}
}
