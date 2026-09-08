using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal abstract class AsyncIteratorBase<TSource> : IAsyncEnumerable<TSource>, IAsyncEnumerator<TSource>, IAsyncDisposable
{
	private readonly int _threadId;

	protected AsyncIteratorState _state;

	protected CancellationToken _cancellationToken;

	public abstract TSource Current { get; }

	protected AsyncIteratorBase()
	{
		_threadId = Environment.CurrentManagedThreadId;
	}

	public IAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		AsyncIteratorBase<TSource> obj = ((_state == AsyncIteratorState.New && _threadId == Environment.CurrentManagedThreadId) ? this : Clone());
		obj._state = AsyncIteratorState.Allocated;
		obj._cancellationToken = cancellationToken;
		return obj;
	}

	public virtual ValueTask DisposeAsync()
	{
		_state = AsyncIteratorState.Disposed;
		return default(ValueTask);
	}

	public async ValueTask<bool> MoveNextAsync()
	{
		if (_state == AsyncIteratorState.Disposed)
		{
			return false;
		}
		try
		{
			return await MoveNextCore().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch
		{
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
	}

	public abstract AsyncIteratorBase<TSource> Clone();

	protected abstract ValueTask<bool> MoveNextCore();
}
