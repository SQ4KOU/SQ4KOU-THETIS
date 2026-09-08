namespace System.Reactive.Linq.ObservableImpl;

internal sealed class SingleBlocking<T> : BaseBlocking<T>
{
	internal bool _hasMoreThanOneElement;

	public override void OnNext(T value)
	{
		if (_hasValue)
		{
			_hasMoreThanOneElement = true;
			Set();
		}
		_value = value;
		_hasValue = true;
	}
}
