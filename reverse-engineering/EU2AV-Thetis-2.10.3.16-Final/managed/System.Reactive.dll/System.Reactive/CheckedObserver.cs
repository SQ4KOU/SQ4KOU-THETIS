using System.Threading;

namespace System.Reactive;

internal sealed class CheckedObserver<T> : IObserver<T>
{
	private readonly IObserver<T> _observer;

	private int _state;

	private const int Idle = 0;

	private const int Busy = 1;

	private const int Done = 2;

	public CheckedObserver(IObserver<T> observer)
	{
		_observer = observer;
	}

	public void OnNext(T value)
	{
		CheckAccess();
		try
		{
			_observer.OnNext(value);
		}
		finally
		{
			Interlocked.Exchange(ref _state, 0);
		}
	}

	public void OnError(Exception error)
	{
		CheckAccess();
		try
		{
			_observer.OnError(error);
		}
		finally
		{
			Interlocked.Exchange(ref _state, 2);
		}
	}

	public void OnCompleted()
	{
		CheckAccess();
		try
		{
			_observer.OnCompleted();
		}
		finally
		{
			Interlocked.Exchange(ref _state, 2);
		}
	}

	private void CheckAccess()
	{
		switch (Interlocked.CompareExchange(ref _state, 1, 0))
		{
		case 1:
			throw new InvalidOperationException(Strings_Core.REENTRANCY_DETECTED);
		case 2:
			throw new InvalidOperationException(Strings_Core.OBSERVER_TERMINATED);
		}
	}
}
