using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Concurrency;

public sealed class EventLoopScheduler : LocalScheduler, ISchedulerPeriodic, IDisposable
{
	private sealed class PeriodicallyScheduledWorkItem<TState> : IDisposable
	{
		private readonly TimeSpan _period;

		private readonly Func<TState, TState> _action;

		private readonly EventLoopScheduler _scheduler;

		private readonly AsyncLock _gate = new AsyncLock();

		private TState _state;

		private TimeSpan _next;

		private MultipleAssignmentDisposableValue _task;

		public PeriodicallyScheduledWorkItem(EventLoopScheduler scheduler, TState state, TimeSpan period, Func<TState, TState> action)
		{
			_state = state;
			_period = period;
			_action = action;
			_scheduler = scheduler;
			_next = scheduler._stopwatch.Elapsed + period;
			_task.TrySetFirst(scheduler.Schedule(this, _next - scheduler._stopwatch.Elapsed, (IScheduler _, PeriodicallyScheduledWorkItem<TState> s) => s.Tick(_)));
		}

		private IDisposable Tick(IScheduler self)
		{
			_next += _period;
			_task.Disposable = self.Schedule(this, _next - _scheduler._stopwatch.Elapsed, (IScheduler _, PeriodicallyScheduledWorkItem<TState> s) => s.Tick(_));
			_gate.Wait(this, delegate(PeriodicallyScheduledWorkItem<TState> closureWorkItem)
			{
				closureWorkItem._state = closureWorkItem._action(closureWorkItem._state);
			});
			return Disposable.Empty;
		}

		public void Dispose()
		{
			_task.Dispose();
			_gate.Dispose();
		}
	}

	private static int _counter;

	private readonly Func<ThreadStart, Thread> _threadFactory;

	private readonly IStopwatch _stopwatch;

	private Thread? _thread;

	private readonly object _gate;

	private readonly SemaphoreSlim _evt;

	private readonly SchedulerQueue<TimeSpan> _queue;

	private readonly Queue<ScheduledItem<TimeSpan>> _readyList;

	private ScheduledItem<TimeSpan>? _nextItem;

	private SerialDisposableValue _nextTimer;

	private bool _disposed;

	internal bool ExitIfEmpty { get; set; }

	public EventLoopScheduler()
		: this((ThreadStart a) => new Thread(a)
		{
			Name = "Event Loop " + Interlocked.Increment(ref _counter),
			IsBackground = true
		})
	{
	}

	public EventLoopScheduler(Func<ThreadStart, Thread> threadFactory)
	{
		_threadFactory = threadFactory ?? throw new ArgumentNullException("threadFactory");
		_stopwatch = ConcurrencyAbstractionLayer.Current.StartStopwatch();
		_gate = new object();
		_evt = new SemaphoreSlim(0);
		_queue = new SchedulerQueue<TimeSpan>();
		_readyList = new Queue<ScheduledItem<TimeSpan>>();
		ExitIfEmpty = false;
	}

	public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		TimeSpan dueTime2 = _stopwatch.Elapsed + dueTime;
		ScheduledItem<TimeSpan, TState> scheduledItem = new ScheduledItem<TimeSpan, TState>(this, state, action, dueTime2);
		lock (_gate)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("EventLoopScheduler");
			}
			if (dueTime <= TimeSpan.Zero)
			{
				_readyList.Enqueue(scheduledItem);
				_evt.Release();
			}
			else
			{
				_queue.Enqueue(scheduledItem);
				_evt.Release();
			}
			EnsureThread();
			return scheduledItem;
		}
	}

	public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
	{
		if (period < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return new PeriodicallyScheduledWorkItem<TState>(this, state, period, action);
	}

	public override IStopwatch StartStopwatch()
	{
		return new StopwatchImpl();
	}

	public void Dispose()
	{
		lock (_gate)
		{
			if (!_disposed)
			{
				_disposed = true;
				_nextTimer.Dispose();
				_evt.Release();
			}
		}
	}

	private void EnsureThread()
	{
		if (_thread == null)
		{
			_thread = _threadFactory(Run);
			_thread.Start();
		}
	}

	private void Run()
	{
		while (true)
		{
			_evt.Wait();
			ScheduledItem<TimeSpan>[] array = null;
			lock (_gate)
			{
				while (_evt.CurrentCount > 0)
				{
					_evt.Wait();
				}
				if (_disposed)
				{
					_evt.Dispose();
					break;
				}
				while (_queue.Count > 0 && _queue.Peek().DueTime <= _stopwatch.Elapsed)
				{
					ScheduledItem<TimeSpan> item = _queue.Dequeue();
					_readyList.Enqueue(item);
				}
				if (_queue.Count > 0)
				{
					ScheduledItem<TimeSpan> scheduledItem = _queue.Peek();
					if (scheduledItem != _nextItem)
					{
						_nextItem = scheduledItem;
						TimeSpan dueTime = scheduledItem.DueTime - _stopwatch.Elapsed;
						_nextTimer.Disposable = ConcurrencyAbstractionLayer.Current.StartTimer(Tick, scheduledItem, dueTime);
					}
				}
				if (_readyList.Count > 0)
				{
					array = _readyList.ToArray();
					_readyList.Clear();
				}
			}
			if (array != null)
			{
				ScheduledItem<TimeSpan>[] array2 = array;
				foreach (ScheduledItem<TimeSpan> scheduledItem2 in array2)
				{
					if (!scheduledItem2.IsCanceled)
					{
						try
						{
							scheduledItem2.Invoke();
						}
						catch (ObjectDisposedException ex) when (ex.ObjectName == "EventLoopScheduler")
						{
						}
					}
				}
			}
			if (!ExitIfEmpty)
			{
				continue;
			}
			lock (_gate)
			{
				if (_readyList.Count == 0 && _queue.Count == 0)
				{
					_thread = null;
					break;
				}
			}
		}
	}

	private void Tick(object? state)
	{
		lock (_gate)
		{
			if (!_disposed)
			{
				ScheduledItem<TimeSpan> scheduledItem = (ScheduledItem<TimeSpan>)state;
				if (scheduledItem == _nextItem)
				{
					_nextItem = null;
				}
				if (_queue.Remove(scheduledItem))
				{
					_readyList.Enqueue(scheduledItem);
				}
				_evt.Release();
			}
		}
	}
}
