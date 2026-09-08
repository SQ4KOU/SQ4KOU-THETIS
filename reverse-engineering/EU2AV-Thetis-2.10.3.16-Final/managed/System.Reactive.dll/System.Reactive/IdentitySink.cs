namespace System.Reactive;

internal abstract class IdentitySink<T> : Sink<T, T>
{
	protected IdentitySink(IObserver<T> observer)
		: base(observer)
	{
	}

	public override void OnNext(T value)
	{
		ForwardOnNext(value);
	}
}
