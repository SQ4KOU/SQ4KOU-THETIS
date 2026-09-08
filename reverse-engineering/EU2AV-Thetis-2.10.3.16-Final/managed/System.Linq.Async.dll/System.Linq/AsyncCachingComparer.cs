using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal abstract class AsyncCachingComparer<TElement>
{
	internal abstract ValueTask<int> Compare(TElement element, bool cacheLower, CancellationToken cancellationToken);

	internal abstract ValueTask SetElement(TElement element, CancellationToken cancellationToken);
}
internal class AsyncCachingComparer<TElement, TKey> : AsyncCachingComparer<TElement>
{
	protected readonly Func<TElement, TKey> _keySelector;

	protected readonly IComparer<TKey> _comparer;

	protected readonly bool _descending;

	protected TKey _lastKey;

	public AsyncCachingComparer(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
	{
		_keySelector = keySelector;
		_comparer = comparer;
		_descending = descending;
		_lastKey = default(TKey);
	}

	internal override ValueTask<int> Compare(TElement element, bool cacheLower, CancellationToken cancellationToken)
	{
		TKey val = _keySelector(element);
		int num = (_descending ? _comparer.Compare(_lastKey, val) : _comparer.Compare(val, _lastKey));
		if (cacheLower == num < 0)
		{
			_lastKey = val;
		}
		return new ValueTask<int>(num);
	}

	internal override ValueTask SetElement(TElement element, CancellationToken cancellationToken)
	{
		_lastKey = _keySelector(element);
		return default(ValueTask);
	}
}
