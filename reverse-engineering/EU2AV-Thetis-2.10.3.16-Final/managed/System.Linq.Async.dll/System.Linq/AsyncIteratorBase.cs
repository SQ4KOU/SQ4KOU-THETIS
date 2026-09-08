using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal abstract class AsyncIteratorBase<TSource> : IAsyncEnumerable<TSource>, IAsyncEnumerator<TSource>, IAsyncDisposable
{
	private readonly int _threadId;

	protected System.Linq.AsyncIteratorState _state;

	protected CancellationToken _cancellationToken;

	public abstract TSource Current { get; }

	protected AsyncIteratorBase()
	{
		_threadId = Environment.CurrentManagedThreadId;
	}

	public IAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		System.Linq.AsyncIteratorBase<TSource> obj = ((_state == System.Linq.AsyncIteratorState.New && _threadId == Environment.CurrentManagedThreadId) ? this : Clone());
		obj._state = System.Linq.AsyncIteratorState.Allocated;
		obj._cancellationToken = cancellationToken;
		return obj;
	}

	public virtual ValueTask DisposeAsync()
	{
		_state = System.Linq.AsyncIteratorState.Disposed;
		return default(ValueTask);
	}

	public async ValueTask<bool> MoveNextAsync()
	{
		if (_state == System.Linq.AsyncIteratorState.Disposed)
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

	public abstract System.Linq.AsyncIteratorBase<TSource> Clone();

	protected abstract ValueTask<bool> MoveNextCore();

	public virtual IAsyncEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
	{
		return new AsyncEnumerable.SelectEnumerableAsyncIterator<TSource, TResult>(this, selector);
	}

	public virtual IAsyncEnumerable<TResult> Select<TResult>(Func<TSource, ValueTask<TResult>> selector)
	{
		return new AsyncEnumerable.SelectEnumerableAsyncIteratorWithTask<TSource, TResult>(this, selector);
	}

	public virtual IAsyncEnumerable<TResult> Select<TResult>(Func<TSource, CancellationToken, ValueTask<TResult>> selector)
	{
		return new AsyncEnumerable.SelectEnumerableAsyncIteratorWithTaskAndCancellation<TSource, TResult>(this, selector);
	}

	public virtual IAsyncEnumerable<TSource> Where(Func<TSource, bool> predicate)
	{
		return new AsyncEnumerable.WhereEnumerableAsyncIterator<TSource>(this, predicate);
	}

	public virtual IAsyncEnumerable<TSource> Where(Func<TSource, ValueTask<bool>> predicate)
	{
		return new AsyncEnumerable.WhereEnumerableAsyncIteratorWithTask<TSource>(this, predicate);
	}

	public virtual IAsyncEnumerable<TSource> Where(Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		return new AsyncEnumerable.WhereEnumerableAsyncIteratorWithTaskAndCancellation<TSource>(this, predicate);
	}
}
