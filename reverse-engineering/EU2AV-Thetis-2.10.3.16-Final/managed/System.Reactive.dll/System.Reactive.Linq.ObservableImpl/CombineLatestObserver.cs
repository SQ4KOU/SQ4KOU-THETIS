namespace System.Reactive.Linq.ObservableImpl;

internal sealed class CombineLatestObserver<T> : SafeObserver<T>
{
	private readonly object _gate;

	private readonly ICombineLatest _parent;

	private readonly int _index;

	private T? _value;

	public T? Value => _value;

	public CombineLatestObserver(object gate, ICombineLatest parent, int index)
	{
		_gate = gate;
		_parent = parent;
		_index = index;
	}

	public override void OnNext(T value)
	{
		lock (_gate)
		{
			_value = value;
			_parent.Next(_index);
		}
	}

	public override void OnError(Exception error)
	{
		Dispose();
		lock (_gate)
		{
			_parent.Fail(error);
		}
	}

	public override void OnCompleted()
	{
		Dispose();
		lock (_gate)
		{
			_parent.Done(_index);
		}
	}
}
