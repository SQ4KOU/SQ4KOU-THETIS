using System.Reactive.Disposables;

namespace System.Reactive;

public sealed class AnonymousObservable<T> : ObservableBase<T>
{
	private readonly Func<IObserver<T>, IDisposable> _subscribe;

	public AnonymousObservable(Func<IObserver<T>, IDisposable> subscribe)
	{
		_subscribe = subscribe ?? throw new ArgumentNullException("subscribe");
	}

	protected override IDisposable SubscribeCore(IObserver<T> observer)
	{
		return _subscribe(observer) ?? Disposable.Empty;
	}
}
