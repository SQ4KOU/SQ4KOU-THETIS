using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ReturnImmediate<TSource> : BasicProducer<TSource>
{
	private readonly TSource _value;

	public ReturnImmediate(TSource value)
	{
		_value = value;
	}

	protected override IDisposable Run(IObserver<TSource> observer)
	{
		observer.OnNext(_value);
		observer.OnCompleted();
		return Disposable.Empty;
	}
}
