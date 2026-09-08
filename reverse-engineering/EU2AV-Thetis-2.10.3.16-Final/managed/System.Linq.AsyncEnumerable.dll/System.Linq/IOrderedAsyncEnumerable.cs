using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

public interface IOrderedAsyncEnumerable<out TElement> : IAsyncEnumerable<TElement>
{
	IOrderedAsyncEnumerable<TElement> CreateOrderedAsyncEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey>? comparer, bool descending);

	IOrderedAsyncEnumerable<TElement> CreateOrderedAsyncEnumerable<TKey>(Func<TElement, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, bool descending);
}
