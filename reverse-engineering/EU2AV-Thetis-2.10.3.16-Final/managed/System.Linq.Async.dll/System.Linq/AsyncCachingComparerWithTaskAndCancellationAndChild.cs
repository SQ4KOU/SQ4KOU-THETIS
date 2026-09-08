using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal sealed class AsyncCachingComparerWithTaskAndCancellationAndChild<TElement, TKey> : AsyncCachingComparerWithTaskAndCancellation<TElement, TKey>
{
	private readonly AsyncCachingComparer<TElement> _child;

	public AsyncCachingComparerWithTaskAndCancellationAndChild(Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, bool descending, AsyncCachingComparer<TElement> child)
		: base(keySelector, comparer, descending)
	{
		_child = child;
	}

	internal override async ValueTask<int> Compare(TElement element, bool cacheLower, CancellationToken cancellationToken)
	{
		TKey val = await _keySelector(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int cmp = (_descending ? _comparer.Compare(_lastKey, val) : _comparer.Compare(val, _lastKey));
		if (cmp == 0)
		{
			return await _child.Compare(element, cacheLower, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (cacheLower == cmp < 0)
		{
			_lastKey = val;
			await _child.SetElement(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return cmp;
	}

	internal override async ValueTask SetElement(TElement element, CancellationToken cancellationToken)
	{
		await base.SetElement(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await _child.SetElement(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}
}
