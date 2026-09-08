namespace System.Reactive.Linq.ObservableImpl;

internal sealed class FirstBlocking<T> : BaseBlocking<T>
{
	public override void OnNext(T value)
	{
		if (!_hasValue)
		{
			_value = value;
			_hasValue = true;
			Set();
		}
	}
}
