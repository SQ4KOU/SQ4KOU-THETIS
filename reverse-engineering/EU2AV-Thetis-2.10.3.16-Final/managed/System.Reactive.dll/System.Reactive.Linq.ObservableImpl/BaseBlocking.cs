using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal abstract class BaseBlocking<T> : ManualResetEventSlim, IObserver<T>
{
	internal T? _value;

	internal bool _hasValue;

	internal Exception? _error;

	internal BaseBlocking()
	{
	}

	public void OnCompleted()
	{
		Set();
	}

	public void OnError(Exception error)
	{
		_value = default(T);
		_error = error;
		Set();
	}

	public abstract void OnNext(T value);
}
