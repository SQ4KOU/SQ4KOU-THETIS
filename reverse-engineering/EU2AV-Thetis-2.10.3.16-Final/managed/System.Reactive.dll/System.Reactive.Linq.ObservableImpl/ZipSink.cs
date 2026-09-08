using System.Collections;

namespace System.Reactive.Linq.ObservableImpl;

internal abstract class ZipSink<TResult> : IdentitySink<TResult>, IZip
{
	protected readonly object _gate;

	private readonly ICollection[] _queues;

	private readonly bool[] _isDone;

	public ICollection[] Queues => _queues;

	protected ZipSink(int arity, IObserver<TResult> observer)
		: base(observer)
	{
		_gate = new object();
		_isDone = new bool[arity];
		_queues = new ICollection[arity];
	}

	public void Next(int index)
	{
		bool flag = true;
		ICollection[] queues = _queues;
		for (int i = 0; i < queues.Length; i++)
		{
			if (queues[i].Count == 0)
			{
				flag = false;
				break;
			}
		}
		if (flag)
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
		bool flag2 = true;
		for (int j = 0; j < _isDone.Length; j++)
		{
			if (j != index && !_isDone[j])
			{
				flag2 = false;
				break;
			}
		}
		if (flag2)
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
