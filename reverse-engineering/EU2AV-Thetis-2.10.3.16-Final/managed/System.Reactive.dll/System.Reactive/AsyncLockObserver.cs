using System.Reactive.Concurrency;

namespace System.Reactive;

internal sealed class AsyncLockObserver<T> : ObserverBase<T>
{
	private readonly AsyncLock _gate;

	private readonly IObserver<T> _observer;

	public AsyncLockObserver(IObserver<T> observer, AsyncLock gate)
	{
		_gate = gate;
		_observer = observer;
	}

	protected override void OnNextCore(T value)
	{
		_gate.Wait((_observer, value), delegate((IObserver<T> _observer, T value) tuple)
		{
			tuple._observer.OnNext(tuple.value);
		});
	}

	protected override void OnErrorCore(Exception exception)
	{
		_gate.Wait((_observer, exception), delegate((IObserver<T> _observer, Exception exception) tuple)
		{
			tuple._observer.OnError(tuple.exception);
		});
	}

	protected override void OnCompletedCore()
	{
		_gate.Wait(_observer, delegate(IObserver<T> closureObserver)
		{
			closureObserver.OnCompleted();
		});
	}
}
