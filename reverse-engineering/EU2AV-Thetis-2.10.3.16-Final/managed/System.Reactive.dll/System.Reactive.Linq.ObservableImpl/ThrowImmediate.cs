using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ThrowImmediate<TSource> : BasicProducer<TSource>
{
	private readonly Exception _exception;

	public ThrowImmediate(Exception exception)
	{
		_exception = exception;
	}

	protected override IDisposable Run(IObserver<TSource> observer)
	{
		observer.OnError(_exception);
		return Disposable.Empty;
	}
}
