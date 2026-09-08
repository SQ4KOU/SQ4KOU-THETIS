using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal static class Utilities
{
	public static async ValueTask AddRangeAsync<T>(this List<T> list, IAsyncEnumerable<T> collection, CancellationToken cancellationToken)
	{
		if (collection is IEnumerable<T> collection2)
		{
			list.AddRange(collection2);
			return;
		}
		if (collection is IAsyncIListProvider<T> asyncIListProvider)
		{
			int num = await asyncIListProvider.GetCountAsync(onlyIfCheap: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (num == 0)
			{
				return;
			}
			if (num > 0)
			{
				int num2 = list.Count + num;
				if (list.Capacity < num2)
				{
					list.Capacity = num2;
				}
			}
		}
		await foreach (T item in collection.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			list.Add(item);
		}
	}
}
