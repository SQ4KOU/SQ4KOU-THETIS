using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive;

internal sealed class ObserveOnObserverNew<T> : IdentitySink<T>
{
	private readonly IScheduler _scheduler;

	private readonly ConcurrentQueue<T> _queue;

	private IDisposable? _task;

	private int _wip;

	private bool _done;

	private Exception? _error;

	private bool _disposed;

	private static readonly Func<IScheduler, ObserveOnObserverNew<T>, IDisposable> DrainShortRunningFunc = (IScheduler scheduler, ObserveOnObserverNew<T> self) => self.DrainShortRunning(scheduler);

	public ObserveOnObserverNew(IScheduler scheduler, IObserver<T> downstream)
		: base(downstream)
	{
		_scheduler = scheduler;
		_queue = new ConcurrentQueue<T>();
	}

	protected override void Dispose(bool disposing)
	{
		Volatile.Write(ref _disposed, value: true);
		base.Dispose(disposing);
		if (disposing)
		{
			Disposable.Dispose(ref _task);
			Clear(_queue);
		}
	}

	private static void Clear(ConcurrentQueue<T> q)
	{
		T result;
		while (q.TryDequeue(out result))
		{
		}
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

	public override void OnNext(T value)
	{
		_queue.Enqueue(value);
		Schedule();
	}

	private void Schedule()
	{
		if (Interlocked.Increment(ref _wip) == 1)
		{
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			if (Disposable.TrySetMultiple(ref _task, singleAssignmentDisposable))
			{
				singleAssignmentDisposable.Disposable = _scheduler.Schedule(this, DrainShortRunningFunc);
			}
			if (Volatile.Read(ref _disposed))
			{
				Clear(_queue);
			}
		}
	}

	private IDisposable DrainShortRunning(IScheduler recursiveScheduler)
	{
		DrainStep(_queue);
		if (Interlocked.Decrement(ref _wip) != 0)
		{
			IDisposable value = recursiveScheduler.Schedule(this, DrainShortRunningFunc);
			Disposable.TrySetMultiple(ref _task, value);
		}
		return Disposable.Empty;
	}

	private void DrainStep(ConcurrentQueue<T> q)
	{
		if (Volatile.Read(ref _disposed))
		{
			Clear(q);
			return;
		}
		bool flag = Volatile.Read(ref _done);
		if (flag)
		{
			Exception error = _error;
			if (error != null)
			{
				Volatile.Write(ref _disposed, value: true);
				ForwardOnError(error);
				return;
			}
		}
		if (q.TryDequeue(out T result))
		{
			ForwardOnNext(result);
		}
		else if (flag)
		{
			Volatile.Write(ref _disposed, value: true);
			ForwardOnCompleted();
		}
	}
}
