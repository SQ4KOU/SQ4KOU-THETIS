using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class EmptyDirect<TResult> : BasicProducer<TResult>
{
	internal static readonly IObservable<TResult> Instance = new EmptyDirect<TResult>();

	private EmptyDirect()
	{
	}

	protected override IDisposable Run(IObserver<TResult> observer)
	{
		observer.OnCompleted();
		return Disposable.Empty;
	}
}
