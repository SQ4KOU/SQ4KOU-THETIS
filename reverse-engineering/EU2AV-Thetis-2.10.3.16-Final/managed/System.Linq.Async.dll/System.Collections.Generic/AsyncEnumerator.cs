using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic;

public static class AsyncEnumerator
{
	private sealed class AnonymousAsyncIterator<T> : System.Linq.AsyncIterator<T>
	{
		private readonly Func<T> _currentFunc;

		private readonly Func<ValueTask<bool>> _moveNext;

		private Func<ValueTask>? _dispose;

		public AnonymousAsyncIterator(Func<ValueTask<bool>> moveNext, Func<T> currentFunc, Func<ValueTask> dispose)
		{
			_moveNext = moveNext;
			_currentFunc = currentFunc;
			_dispose = dispose;
			GetAsyncEnumerator(default(CancellationToken));
		}

		public override System.Linq.AsyncIteratorBase<T> Clone()
		{
			throw new NotSupportedException("AnonymousAsyncIterator cannot be cloned. It is only intended for use as an iterator.");
		}

		public override async ValueTask DisposeAsync()
		{
			Func<ValueTask> func = Interlocked.Exchange(ref _dispose, null);
			if (func != null)
			{
				await func().ConfigureAwait(continueOnCapturedContext: false);
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0127;
				}
			}
			else
			{
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (await _moveNext().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _currentFunc();
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0127;
			IL_0127:
			return false;
		}
	}

	private sealed class WithCancellationAsyncEnumerator<T> : IAsyncEnumerator<T>, IAsyncDisposable
	{
		private readonly IAsyncEnumerator<T> _source;

		private readonly CancellationToken _cancellationToken;

		public T Current => _source.Current;

		public WithCancellationAsyncEnumerator(IAsyncEnumerator<T> source, CancellationToken cancellationToken)
		{
			_source = source;
			_cancellationToken = cancellationToken;
		}

		public ValueTask DisposeAsync()
		{
			return _source.DisposeAsync();
		}

		public ValueTask<bool> MoveNextAsync()
		{
			_cancellationToken.ThrowIfCancellationRequested();
			return _source.MoveNextAsync();
		}
	}

	public static IAsyncEnumerator<T> Create<T>(Func<ValueTask<bool>> moveNextAsync, Func<T> getCurrent, Func<ValueTask> disposeAsync)
	{
		if (moveNextAsync == null)
		{
			throw System.Error.ArgumentNull("moveNextAsync");
		}
		return new AnonymousAsyncIterator<T>(moveNextAsync, getCurrent, disposeAsync);
	}

	public static ValueTask<bool> MoveNextAsync<T>(this IAsyncEnumerator<T> source, CancellationToken cancellationToken)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		cancellationToken.ThrowIfCancellationRequested();
		return source.MoveNextAsync();
	}

	public static IAsyncEnumerator<T> WithCancellation<T>(this IAsyncEnumerator<T> source, CancellationToken cancellationToken)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (cancellationToken == default(CancellationToken))
		{
			return source;
		}
		return new WithCancellationAsyncEnumerator<T>(source, cancellationToken);
	}
}
