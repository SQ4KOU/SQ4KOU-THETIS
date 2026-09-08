using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

[Obsolete("This interface was always unsupported, and the IAsyncEnumerable<T> LINQ implementation in System.Linq.AsyncEnumerable does not recognize it, so this no longer serves a purpose")]
public interface IAsyncIListProvider<TElement> : IAsyncEnumerable<TElement>
{
	ValueTask<TElement[]> ToArrayAsync(CancellationToken cancellationToken);

	ValueTask<List<TElement>> ToListAsync(CancellationToken cancellationToken);

	ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken);
}
