using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal interface IAsyncIListProvider<TElement> : IAsyncEnumerable<TElement>
{
	ValueTask<TElement[]> ToArrayAsync(CancellationToken cancellationToken);

	ValueTask<List<TElement>> ToListAsync(CancellationToken cancellationToken);

	ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken);
}
