using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal class AsyncCachingComparerWithTask<TElement, TKey> : AsyncCachingComparer<TElement>
{
	protected readonly Func<TElement, ValueTask<TKey>> _keySelector;

	protected readonly IComparer<TKey> _comparer;

	protected readonly bool _descending;

	protected TKey _lastKey;

	public AsyncCachingComparerWithTask(Func<TElement, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, bool descending)
	{
		_keySelector = keySelector;
		_comparer = comparer;
		_descending = descending;
		_lastKey = default(TKey);
	}

	internal override async ValueTask<int> Compare(TElement element, bool cacheLower, CancellationToken cancellationToken)
	{
		TKey val = await _keySelector(element).ConfigureAwait(continueOnCapturedContext: false);
		int num = (_descending ? _comparer.Compare(_lastKey, val) : _comparer.Compare(val, _lastKey));
		if (cacheLower == num < 0)
		{
			_lastKey = val;
		}
		return num;
	}

	internal override async ValueTask SetElement(TElement element, CancellationToken cancellationToken)
	{
		_lastKey = await _keySelector(element).ConfigureAwait(continueOnCapturedContext: false);
	}
}
