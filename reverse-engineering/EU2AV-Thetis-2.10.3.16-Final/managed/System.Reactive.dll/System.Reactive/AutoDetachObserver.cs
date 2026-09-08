using System.Reactive.Disposables;

namespace System.Reactive;

internal sealed class AutoDetachObserver<T> : ObserverBase<T>, ISafeObserver<T>, IObserver<T>, IDisposable
{
	private readonly IObserver<T> _observer;

	private SingleAssignmentDisposableValue _disposable;

	public AutoDetachObserver(IObserver<T> observer)
	{
		_observer = observer;
	}

	public void SetResource(IDisposable resource)
	{
		_disposable.Disposable = resource;
	}

	protected override void OnNextCore(T value)
	{
		bool flag = false;
		try
		{
			_observer.OnNext(value);
			flag = true;
		}
		finally
		{
			if (!flag)
			{
				Dispose();
			}
		}
	}

	protected override void OnErrorCore(Exception exception)
	{
		try
		{
			_observer.OnError(exception);
		}
		finally
		{
			Dispose();
		}
	}

	protected override void OnCompletedCore()
	{
		try
		{
			_observer.OnCompleted();
		}
		finally
		{
			Dispose();
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			_disposable.Dispose();
		}
	}
}
