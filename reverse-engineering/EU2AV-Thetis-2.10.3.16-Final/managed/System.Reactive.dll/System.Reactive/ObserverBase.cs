using System.Threading;

namespace System.Reactive;

public abstract class ObserverBase<T> : IObserver<T>, IDisposable
{
	private int _isStopped;

	protected ObserverBase()
	{
		_isStopped = 0;
	}

	public void OnNext(T value)
	{
		if (Volatile.Read(ref _isStopped) == 0)
		{
			OnNextCore(value);
		}
	}

	protected abstract void OnNextCore(T value);

	public void OnError(Exception error)
	{
		if (error == null)
		{
			throw new ArgumentNullException("error");
		}
		if (Interlocked.Exchange(ref _isStopped, 1) == 0)
		{
			OnErrorCore(error);
		}
	}

	protected abstract void OnErrorCore(Exception error);

	public void OnCompleted()
	{
		if (Interlocked.Exchange(ref _isStopped, 1) == 0)
		{
			OnCompletedCore();
		}
	}

	protected abstract void OnCompletedCore();

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Volatile.Write(ref _isStopped, 1);
		}
	}

	internal bool Fail(Exception error)
	{
		if (Interlocked.Exchange(ref _isStopped, 1) == 0)
		{
			OnErrorCore(error);
			return true;
		}
		return false;
	}
}
