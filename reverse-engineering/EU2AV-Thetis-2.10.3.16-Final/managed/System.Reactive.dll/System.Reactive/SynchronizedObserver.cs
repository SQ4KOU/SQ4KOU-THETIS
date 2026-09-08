namespace System.Reactive;

internal sealed class SynchronizedObserver<T> : ObserverBase<T>
{
	private readonly object _gate;

	private readonly IObserver<T> _observer;

	public SynchronizedObserver(IObserver<T> observer, object gate)
	{
		_gate = gate;
		_observer = observer;
	}

	protected override void OnNextCore(T value)
	{
		lock (_gate)
		{
			_observer.OnNext(value);
		}
	}

	protected override void OnErrorCore(Exception exception)
	{
		lock (_gate)
		{
			_observer.OnError(exception);
		}
	}

	protected override void OnCompletedCore()
	{
		lock (_gate)
		{
			_observer.OnCompleted();
		}
	}
}
