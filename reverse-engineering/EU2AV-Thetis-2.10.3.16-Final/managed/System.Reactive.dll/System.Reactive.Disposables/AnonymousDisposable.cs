using System.Threading;

namespace System.Reactive.Disposables;

internal sealed class AnonymousDisposable : ICancelable, IDisposable
{
	private volatile Action? _dispose;

	public bool IsDisposed => _dispose == null;

	public AnonymousDisposable(Action dispose)
	{
		_dispose = dispose;
	}

	public void Dispose()
	{
		Interlocked.Exchange(ref _dispose, null)?.Invoke();
	}
}
internal sealed class AnonymousDisposable<TState> : ICancelable, IDisposable
{
	private TState _state;

	private volatile Action<TState>? _dispose;

	public bool IsDisposed => _dispose == null;

	public AnonymousDisposable(TState state, Action<TState> dispose)
	{
		_state = state;
		_dispose = dispose;
	}

	public void Dispose()
	{
		Interlocked.Exchange(ref _dispose, null)?.Invoke(_state);
		_state = default(TState);
	}
}
