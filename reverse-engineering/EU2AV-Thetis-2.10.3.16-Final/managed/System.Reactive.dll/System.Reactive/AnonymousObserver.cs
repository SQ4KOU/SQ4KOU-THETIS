namespace System.Reactive;

public sealed class AnonymousObserver<T> : ObserverBase<T>
{
	private readonly Action<T> _onNext;

	private readonly Action<Exception> _onError;

	private readonly Action _onCompleted;

	public AnonymousObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
	{
		_onNext = onNext ?? throw new ArgumentNullException("onNext");
		_onError = onError ?? throw new ArgumentNullException("onError");
		_onCompleted = onCompleted ?? throw new ArgumentNullException("onCompleted");
	}

	public AnonymousObserver(Action<T> onNext)
		: this(onNext, Stubs.Throw, Stubs.Nop)
	{
	}

	public AnonymousObserver(Action<T> onNext, Action<Exception> onError)
		: this(onNext, onError, Stubs.Nop)
	{
	}

	public AnonymousObserver(Action<T> onNext, Action onCompleted)
		: this(onNext, Stubs.Throw, onCompleted)
	{
	}

	protected override void OnNextCore(T value)
	{
		_onNext(value);
	}

	protected override void OnErrorCore(Exception error)
	{
		_onError(error);
	}

	protected override void OnCompletedCore()
	{
		_onCompleted();
	}

	internal ISafeObserver<T> MakeSafe()
	{
		return new AnonymousSafeObserver<T>(_onNext, _onError, _onCompleted);
	}
}
