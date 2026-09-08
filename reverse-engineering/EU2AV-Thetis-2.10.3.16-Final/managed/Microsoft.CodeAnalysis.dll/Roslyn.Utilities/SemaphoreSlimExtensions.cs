using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities;

internal static class SemaphoreSlimExtensions
{
	[NonCopyable]
	internal struct SemaphoreDisposer(SemaphoreSlim semaphore) : IDisposable
	{
		private SemaphoreSlim? _semaphore = semaphore;

		public void Dispose()
		{
			(Interlocked.Exchange(ref _semaphore, null) ?? throw new ObjectDisposedException("Somehow a SemaphoreDisposer is being disposed twice.")).Release();
		}
	}

	public static SemaphoreDisposer DisposableWait(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default(CancellationToken))
	{
		semaphore.Wait(cancellationToken);
		return new SemaphoreDisposer(semaphore);
	}

	public static async ValueTask<SemaphoreDisposer> DisposableWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default(CancellationToken))
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new SemaphoreDisposer(semaphore);
	}
}
