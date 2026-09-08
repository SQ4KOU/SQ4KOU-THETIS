using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq;

internal sealed class SyncKeySelectorAsyncEnumerableSorter<TElement, TKey> : AsyncEnumerableSorterBase<TElement, TKey>
{
	private readonly Func<TElement, TKey> _keySelector;

	public SyncKeySelectorAsyncEnumerableSorter(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, AsyncEnumerableSorter<TElement>? next)
		: base(comparer, descending, next)
	{
		_keySelector = keySelector;
	}

	internal override async ValueTask ComputeKeys(TElement[] elements, int count)
	{
		_keys = new TKey[count];
		for (int i = 0; i < count; i++)
		{
			_keys[i] = _keySelector(elements[i]);
		}
		if (_next != null)
		{
			await _next.ComputeKeys(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
