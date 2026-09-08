using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

internal static class AsyncEnumerableExt
{
	public static ConfiguredCancelableAsyncEnumerable<T>.Enumerator GetConfiguredAsyncEnumerator<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken, bool continueOnCapturedContext)
	{
		return enumerable.ConfigureAwait(continueOnCapturedContext).WithCancellation(cancellationToken).GetAsyncEnumerator();
	}
}
