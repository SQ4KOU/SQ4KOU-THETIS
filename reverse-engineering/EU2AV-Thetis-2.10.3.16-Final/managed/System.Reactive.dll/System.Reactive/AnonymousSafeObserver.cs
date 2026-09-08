using System.Threading;

namespace System.Reactive;

internal sealed class AnonymousSafeObserver<T> : SafeObserver<T>
{
	private readonly Action<T> _onNext;

	private readonly Action<Exception> _onError;

	private readonly Action _onCompleted;

	private int _isStopped;

	public AnonymousSafeObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
	{
		_onNext = onNext;
		_onError = onError;
		_onCompleted = onCompleted;
	}

	public override void OnNext(T value)
	{
		if (_isStopped != 0)
		{
			return;
		}
		bool flag = false;
		try
		{
			_onNext(value);
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

	public override void OnError(Exception error)
	{
		if (Interlocked.Exchange(ref _isStopped, 1) == 0)
		{
			using (this)
			{
				_onError(error);
			}
		}
	}

	public override void OnCompleted()
	{
		if (Interlocked.Exchange(ref _isStopped, 1) == 0)
		{
			using (this)
			{
				_onCompleted();
			}
		}
	}
}
