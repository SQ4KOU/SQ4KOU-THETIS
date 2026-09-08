namespace System.Reactive.Linq.ObservableImpl;

internal sealed class LastBlocking<T> : BaseBlocking<T>
{
	public override void OnNext(T value)
	{
		_value = value;
		_hasValue = true;
	}
}
