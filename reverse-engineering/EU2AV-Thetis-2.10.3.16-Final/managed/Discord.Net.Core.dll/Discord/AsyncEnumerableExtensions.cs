using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord;

public static class AsyncEnumerableExtensions
{
	public static async Task<IEnumerable<T>> FlattenAsync<T>(this IAsyncEnumerable<IEnumerable<T>> source)
	{
		return await source.Flatten().ToArrayAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public static IAsyncEnumerable<T> Flatten<T>(this IAsyncEnumerable<IEnumerable<T>> source)
	{
		return source.SelectMany((IEnumerable<T> enumerable) => enumerable.ToAsyncEnumerable());
	}
}
