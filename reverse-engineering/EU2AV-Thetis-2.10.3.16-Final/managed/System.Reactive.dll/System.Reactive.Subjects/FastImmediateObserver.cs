using System.Collections.Generic;
using System.Threading;

namespace System.Reactive.Subjects;

internal sealed class FastImmediateObserver<T> : IScheduledObserver<T>, IObserver<T>, IDisposable
{
	private readonly object _gate = new object();

	private volatile IObserver<T> _observer;

	private Queue<T> _queue = new Queue<T>();

	private Queue<T>? _queue2;

	private Exception? _error;

	private bool _done;

	private bool _busy;

	private bool _hasFaulted;

	public FastImmediateObserver(IObserver<T> observer)
	{
		_observer = observer;
	}

	public void Dispose()
	{
		Done();
	}

	public void EnsureActive()
	{
		EnsureActive(1);
	}

	public void EnsureActive(int count)
	{
		bool flag = false;
		lock (_gate)
		{
			if (!_hasFaulted && !_busy)
			{
				flag = true;
				_busy = true;
			}
		}
		if (!flag)
		{
			return;
		}
		while (true)
		{
			Queue<T> queue = null;
			Exception ex = null;
			bool flag2 = false;
			lock (_gate)
			{
				if (_queue.Count > 0)
				{
					if (_queue2 == null)
					{
						_queue2 = new Queue<T>();
					}
					queue = _queue;
					_queue = _queue2;
					_queue2 = null;
				}
				if (_error != null)
				{
					ex = _error;
				}
				else if (_done)
				{
					flag2 = true;
				}
				else if (queue == null)
				{
					_busy = false;
					break;
				}
			}
			try
			{
				if (queue != null)
				{
					while (queue.Count > 0)
					{
						_observer.OnNext(queue.Dequeue());
					}
					lock (_gate)
					{
						_queue2 = queue;
					}
				}
				if (ex != null)
				{
					Done().OnError(ex);
					break;
				}
				if (flag2)
				{
					Done().OnCompleted();
					break;
				}
			}
			catch
			{
				lock (_gate)
				{
					_hasFaulted = true;
					_queue.Clear();
				}
				throw;
			}
		}
	}

	public void OnCompleted()
	{
		lock (_gate)
		{
			if (!_hasFaulted)
			{
				_done = true;
			}
		}
	}

	public void OnError(Exception error)
	{
		lock (_gate)
		{
			if (!_hasFaulted)
			{
				_error = error;
			}
		}
	}

	public void OnNext(T value)
	{
		lock (_gate)
		{
			if (!_hasFaulted)
			{
				_queue.Enqueue(value);
			}
		}
	}

	private IObserver<T> Done()
	{
		return Interlocked.Exchange(ref _observer, NopObserver<T>.Instance);
	}
}
