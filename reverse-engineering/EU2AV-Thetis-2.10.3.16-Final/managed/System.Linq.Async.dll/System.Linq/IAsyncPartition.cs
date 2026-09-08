using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal interface IAsyncPartition<TElement> : IAsyncIListProvider<TElement>, IAsyncEnumerable<TElement>
{
	IAsyncPartition<TElement> Skip(int count);

	IAsyncPartition<TElement> Take(int count);

	ValueTask<Maybe<TElement>> TryGetElementAtAsync(int index, CancellationToken cancellationToken);

	ValueTask<Maybe<TElement>> TryGetFirstAsync(CancellationToken cancellationToken);

	ValueTask<Maybe<TElement>> TryGetLastAsync(CancellationToken cancellationToken);
}
