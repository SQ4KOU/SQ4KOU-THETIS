using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq;

internal sealed class AsyncKeySelectorAsyncEnumerableSorter<TElement, TKey> : AsyncEnumerableSorterBase<TElement, TKey>
{
	private readonly Func<TElement, ValueTask<TKey>> _keySelector;

	public AsyncKeySelectorAsyncEnumerableSorter(Func<TElement, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, bool descending, AsyncEnumerableSorter<TElement>? next)
		: base(comparer, descending, next)
	{
		_keySelector = keySelector;
	}

	internal override async ValueTask ComputeKeys(TElement[] elements, int count)
	{
		_keys = new TKey[count];
		for (int i = 0; i < count; i++)
		{
			TKey[] keys = _keys;
			int num = i;
			keys[num] = await _keySelector(elements[i]).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (_next != null)
		{
			await _next.ComputeKeys(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
