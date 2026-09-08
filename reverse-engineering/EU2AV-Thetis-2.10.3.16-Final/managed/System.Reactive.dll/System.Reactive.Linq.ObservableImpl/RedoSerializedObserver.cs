using System.Collections.Concurrent;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class RedoSerializedObserver<X> : IObserver<X>
{
	private static readonly Exception SignaledIndicator = new Exception();

	private readonly IObserver<X> _downstream;

	private readonly ConcurrentQueue<X> _queue;

	private int _wip;

	private Exception? _terminalException;

	internal RedoSerializedObserver(IObserver<X> downstream)
	{
		_downstream = downstream;
		_queue = new ConcurrentQueue<X>();
	}

	public void OnCompleted()
	{
		if (Interlocked.CompareExchange(ref _terminalException, ExceptionHelper.Terminated, null) == null)
		{
			Drain();
		}
	}

	public void OnError(Exception error)
	{
		if (Interlocked.CompareExchange(ref _terminalException, error, null) == null)
		{
			Drain();
		}
	}

	public void OnNext(X value)
	{
		_queue.Enqueue(value);
		Drain();
	}

	private void Clear()
	{
		X result;
		while (_queue.TryDequeue(out result))
		{
		}
	}

	private void Drain()
	{
		if (Interlocked.Increment(ref _wip) != 1)
		{
			return;
		}
		int num = 1;
		do
		{
			Exception ex = Volatile.Read(ref _terminalException);
			if (ex != null)
			{
				if (ex != SignaledIndicator)
				{
					Interlocked.Exchange(ref _terminalException, SignaledIndicator);
					if (ex != ExceptionHelper.Terminated)
					{
						_downstream.OnError(ex);
					}
					else
					{
						_downstream.OnCompleted();
					}
				}
				Clear();
			}
			else
			{
				X result;
				while (_queue.TryDequeue(out result))
				{
					_downstream.OnNext(result);
				}
			}
			num = Interlocked.Add(ref _wip, -num);
		}
		while (num != 0);
	}
}
