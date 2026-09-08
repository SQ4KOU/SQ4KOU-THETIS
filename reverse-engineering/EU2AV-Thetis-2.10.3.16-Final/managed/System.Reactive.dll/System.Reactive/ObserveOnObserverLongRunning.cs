using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive;

internal sealed class ObserveOnObserverLongRunning<TSource> : IdentitySink<TSource>
{
	private readonly ISchedulerLongRunning _scheduler;

	private readonly ConcurrentQueue<TSource> _queue;

	private readonly object _suspendGuard;

	private long _wip;

	private bool _done;

	private Exception? _error;

	private bool _disposed;

	private int _runDrainOnce;

	private SingleAssignmentDisposableValue _drainTask;

	private static readonly Action<ObserveOnObserverLongRunning<TSource>, ICancelable> DrainLongRunning = delegate(ObserveOnObserverLongRunning<TSource> self, ICancelable cancelable)
	{
		self.Drain();
	};

	public ObserveOnObserverLongRunning(ISchedulerLongRunning scheduler, IObserver<TSource> observer)
		: base(observer)
	{
		_scheduler = scheduler;
		_queue = new ConcurrentQueue<TSource>();
		_suspendGuard = new object();
	}

	public override void OnCompleted()
	{
		Volatile.Write(ref _done, value: true);
		Schedule();
	}

	public override void OnError(Exception error)
	{
		_error = error;
		Volatile.Write(ref _done, value: true);
		Schedule();
	}

	public override void OnNext(TSource value)
	{
		_queue.Enqueue(value);
		Schedule();
	}

	private void Schedule()
	{
		if (Volatile.Read(ref _runDrainOnce) == 0 && Interlocked.CompareExchange(ref _runDrainOnce, 1, 0) == 0)
		{
			_drainTask.Disposable = _scheduler.ScheduleLongRunning(this, DrainLongRunning);
		}
		if (Interlocked.Increment(ref _wip) == 1)
		{
			lock (_suspendGuard)
			{
				Monitor.Pulse(_suspendGuard);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		Volatile.Write(ref _disposed, value: true);
		lock (_suspendGuard)
		{
			Monitor.Pulse(_suspendGuard);
		}
		_drainTask.Dispose();
		base.Dispose(disposing);
	}

	private void Drain()
	{
		ConcurrentQueue<TSource> queue = _queue;
		while (true)
		{
			if (Volatile.Read(ref _disposed))
			{
				TSource result;
				while (queue.TryDequeue(out result))
				{
				}
				return;
			}
			bool num = Volatile.Read(ref _done);
			bool flag = queue.TryDequeue(out var result2);
			if (num && !flag)
			{
				break;
			}
			if (flag)
			{
				ForwardOnNext(result2);
				if (Interlocked.Decrement(ref _wip) != 0L)
				{
					continue;
				}
			}
			if (Volatile.Read(ref _wip) != 0L || Volatile.Read(ref _disposed))
			{
				continue;
			}
			object suspendGuard = _suspendGuard;
			if (Monitor.TryEnter(suspendGuard))
			{
				if (Volatile.Read(ref _wip) == 0L && !Volatile.Read(ref _disposed))
				{
					Monitor.Wait(suspendGuard);
				}
				Monitor.Exit(suspendGuard);
			}
		}
		Exception error = _error;
		if (error != null)
		{
			ForwardOnError(error);
		}
		else
		{
			ForwardOnCompleted();
		}
	}
}
