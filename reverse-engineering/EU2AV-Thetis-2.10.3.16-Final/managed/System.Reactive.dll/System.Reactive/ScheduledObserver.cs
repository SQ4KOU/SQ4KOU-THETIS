using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive;

internal class ScheduledObserver<T> : ObserverBase<T>, IScheduledObserver<T>, IObserver<T>, IDisposable
{
	private sealed class SemaphoreSlimRelease : IDisposable
	{
		private volatile SemaphoreSlim? _dispatcherEvent;

		public SemaphoreSlimRelease(SemaphoreSlim dispatcherEvent)
		{
			_dispatcherEvent = dispatcherEvent;
		}

		public void Dispose()
		{
			Interlocked.Exchange(ref _dispatcherEvent, null)?.Release();
		}
	}

	private int _state;

	private const int Stopped = 0;

	private const int Running = 1;

	private const int Pending = 2;

	private const int Faulted = 9;

	private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

	private bool _failed;

	private Exception? _error;

	private bool _completed;

	private readonly IObserver<T> _observer;

	private readonly IScheduler _scheduler;

	private readonly ISchedulerLongRunning? _longRunning;

	private SerialDisposableValue _disposable;

	private readonly object _dispatcherInitGate = new object();

	private readonly SemaphoreSlim? _dispatcherEvent;

	private readonly IDisposable? _dispatcherEventRelease;

	private IDisposable? _dispatcherJob;

	public ScheduledObserver(IScheduler scheduler, IObserver<T> observer)
	{
		_scheduler = scheduler;
		_observer = observer;
		_longRunning = _scheduler.AsLongRunning();
		if (_longRunning != null)
		{
			_dispatcherEvent = new SemaphoreSlim(0);
			_dispatcherEventRelease = new SemaphoreSlimRelease(_dispatcherEvent);
		}
	}

	private void EnsureDispatcher()
	{
		if (_dispatcherJob != null)
		{
			return;
		}
		lock (_dispatcherInitGate)
		{
			if (_dispatcherJob == null)
			{
				_dispatcherJob = _longRunning.ScheduleLongRunning(Dispatch);
				_disposable.Disposable = StableCompositeDisposable.Create(_dispatcherJob, _dispatcherEventRelease);
			}
		}
	}

	private void Dispatch(ICancelable cancel)
	{
		do
		{
			_dispatcherEvent.Wait();
			if (cancel.IsDisposed)
			{
				return;
			}
			T result;
			while (_queue.TryDequeue(out result))
			{
				try
				{
					_observer.OnNext(result);
				}
				catch
				{
					T result2;
					while (_queue.TryDequeue(out result2))
					{
					}
					throw;
				}
				_dispatcherEvent.Wait();
				if (cancel.IsDisposed)
				{
					return;
				}
			}
			if (_failed)
			{
				_observer.OnError(_error);
				Dispose();
				return;
			}
		}
		while (!_completed);
		_observer.OnCompleted();
		Dispose();
	}

	public void EnsureActive()
	{
		EnsureActive(1);
	}

	public void EnsureActive(int n)
	{
		if (_longRunning != null)
		{
			if (n > 0)
			{
				_dispatcherEvent.Release(n);
			}
			EnsureDispatcher();
		}
		else
		{
			EnsureActiveSlow();
		}
	}

	private void EnsureActiveSlow()
	{
		bool flag = false;
		do
		{
			IL_0002:
			switch (Interlocked.CompareExchange(ref _state, 1, 0))
			{
			default:
				goto IL_0002;
			case 0:
				flag = true;
				break;
			case 9:
				return;
			case 1:
				continue;
			case 2:
				break;
			}
			break;
		}
		while (Interlocked.CompareExchange(ref _state, 2, 1) != 1);
		if (flag)
		{
			_disposable.Disposable = _scheduler.Schedule<object>(null, Run);
		}
	}

	private void Run(object? state, Action<object?> recurse)
	{
		T result;
		while (!_queue.TryDequeue(out result))
		{
			if (_failed)
			{
				if (_queue.IsEmpty)
				{
					Interlocked.Exchange(ref _state, 0);
					_observer.OnError(_error);
					Dispose();
					return;
				}
			}
			else if (_completed)
			{
				if (_queue.IsEmpty)
				{
					Interlocked.Exchange(ref _state, 0);
					_observer.OnCompleted();
					Dispose();
					return;
				}
			}
			else
			{
				int num = Interlocked.CompareExchange(ref _state, 0, 1);
				if (num == 1 || num == 9)
				{
					return;
				}
				_state = 1;
			}
		}
		Interlocked.Exchange(ref _state, 1);
		try
		{
			_observer.OnNext(result);
		}
		catch
		{
			Interlocked.Exchange(ref _state, 9);
			T result2;
			while (_queue.TryDequeue(out result2))
			{
			}
			throw;
		}
		recurse(state);
	}

	protected override void OnNextCore(T value)
	{
		_queue.Enqueue(value);
	}

	protected override void OnErrorCore(Exception exception)
	{
		_error = exception;
		_failed = true;
	}

	protected override void OnCompletedCore()
	{
		_completed = true;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			_disposable.Dispose();
		}
	}
}
