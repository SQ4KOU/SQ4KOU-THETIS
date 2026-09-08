using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ZipObserver<T> : SafeObserver<T>
{
	private readonly object _gate;

	private readonly IZip _parent;

	private readonly int _index;

	private readonly Queue<T> _values;

	public Queue<T> Values => _values;

	public ZipObserver(object gate, IZip parent, int index)
	{
		_gate = gate;
		_parent = parent;
		_index = index;
		_values = new Queue<T>();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			lock (_gate)
			{
				_values.Clear();
			}
		}
	}

	public override void OnNext(T value)
	{
		lock (_gate)
		{
			_values.Enqueue(value);
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
		base.Dispose(disposing: true);
		lock (_gate)
		{
			_parent.Done(_index);
		}
	}
}
