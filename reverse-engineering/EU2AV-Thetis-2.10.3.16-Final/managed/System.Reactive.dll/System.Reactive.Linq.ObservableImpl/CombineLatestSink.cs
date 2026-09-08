namespace System.Reactive.Linq.ObservableImpl;

internal abstract class CombineLatestSink<TResult> : IdentitySink<TResult>, ICombineLatest
{
	protected readonly object _gate;

	private bool _hasValueAll;

	private readonly bool[] _hasValue;

	private readonly bool[] _isDone;

	protected CombineLatestSink(int arity, IObserver<TResult> observer)
		: base(observer)
	{
		_gate = new object();
		_hasValue = new bool[arity];
		_isDone = new bool[arity];
	}

	public void Next(int index)
	{
		if (!_hasValueAll)
		{
			_hasValue[index] = true;
			bool hasValueAll = true;
			bool[] hasValue = _hasValue;
			for (int i = 0; i < hasValue.Length; i++)
			{
				if (!hasValue[i])
				{
					hasValueAll = false;
					break;
				}
			}
			_hasValueAll = hasValueAll;
		}
		if (_hasValueAll)
		{
			TResult result;
			try
			{
				result = GetResult();
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			ForwardOnNext(result);
			return;
		}
		bool flag = true;
		for (int j = 0; j < _isDone.Length; j++)
		{
			if (j != index && !_isDone[j])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			ForwardOnCompleted();
		}
	}

	protected abstract TResult GetResult();

	public void Fail(Exception error)
	{
		ForwardOnError(error);
	}

	public void Done(int index)
	{
		_isDone[index] = true;
		bool flag = true;
		bool[] isDone = _isDone;
		for (int i = 0; i < isDone.Length; i++)
		{
			if (!isDone[i])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			ForwardOnCompleted();
		}
	}
}
