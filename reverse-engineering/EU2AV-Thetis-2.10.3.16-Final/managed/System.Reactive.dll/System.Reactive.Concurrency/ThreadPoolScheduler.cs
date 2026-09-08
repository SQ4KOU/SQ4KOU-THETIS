using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Concurrency;

public sealed class ThreadPoolScheduler : LocalScheduler, ISchedulerLongRunning, ISchedulerPeriodic
{
	private sealed class FastPeriodicTimer<TState> : IDisposable
	{
		private TState _state;

		private Func<TState, TState> _action;

		private volatile bool _disposed;

		public FastPeriodicTimer(TState state, Func<TState, TState> action)
		{
			_state = state;
			_action = action;
			ThreadPool.QueueUserWorkItem(delegate(object @this)
			{
				Tick(@this);
			}, this);
		}

		private static void Tick(object state)
		{
			FastPeriodicTimer<TState> fastPeriodicTimer = (FastPeriodicTimer<TState>)state;
			if (!fastPeriodicTimer._disposed)
			{
				fastPeriodicTimer._state = fastPeriodicTimer._action(fastPeriodicTimer._state);
				ThreadPool.QueueUserWorkItem(delegate(object t)
				{
					Tick(t);
				}, fastPeriodicTimer);
			}
		}

		public void Dispose()
		{
			_disposed = true;
			_action = Stubs<TState>.I;
		}
	}

	private sealed class PeriodicTimer<TState> : IDisposable
	{
		private TState _state;

		private Func<TState, TState> _action;

		private readonly AsyncLock _gate;

		private volatile Timer? _timer;

		public PeriodicTimer(TState state, TimeSpan period, Func<TState, TState> action)
		{
			_state = state;
			_action = action;
			_gate = new AsyncLock();
			_timer = new Timer(delegate(object @this)
			{
				((PeriodicTimer<TState>)@this).Tick();
			}, this, period, period);
		}

		private void Tick()
		{
			_gate.Wait(this, delegate(PeriodicTimer<TState> @this)
			{
				@this._state = @this._action(@this._state);
			});
		}

		public void Dispose()
		{
			Timer timer = _timer;
			if (timer != null)
			{
				_action = Stubs<TState>.I;
				_timer = null;
				timer.Dispose();
				_gate.Dispose();
			}
		}
	}

	private static readonly Lazy<ThreadPoolScheduler> LazyInstance = new Lazy<ThreadPoolScheduler>(() => new ThreadPoolScheduler());

	private static readonly Lazy<NewThreadScheduler> LazyNewBackgroundThread = new Lazy<NewThreadScheduler>(() => new NewThreadScheduler((ThreadStart action) => new Thread(action)
	{
		IsBackground = true
	}));

	public static ThreadPoolScheduler Instance => LazyInstance.Value;

	private ThreadPoolScheduler()
	{
	}

	public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		UserWorkItem<TState> userWorkItem = new UserWorkItem<TState>(this, state, action);
		ThreadPool.QueueUserWorkItem(delegate(object closureWorkItem)
		{
			((UserWorkItem<TState>)closureWorkItem).Run();
		}, userWorkItem);
		return userWorkItem;
	}

	public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		TimeSpan dueTime2 = Scheduler.Normalize(dueTime);
		if (dueTime2.Ticks == 0L)
		{
			return Schedule(state, action);
		}
		UserWorkItem<TState> userWorkItem = new UserWorkItem<TState>(this, state, action);
		userWorkItem.CancelQueueDisposable = new Timer(delegate(object closureWorkItem)
		{
			((UserWorkItem<TState>)closureWorkItem).Run();
		}, userWorkItem, dueTime2, Timeout.InfiniteTimeSpan);
		return userWorkItem;
	}

	public IDisposable ScheduleLongRunning<TState>(TState state, Action<TState, ICancelable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return LazyNewBackgroundThread.Value.ScheduleLongRunning(state, action);
	}

	public override IStopwatch StartStopwatch()
	{
		return new StopwatchImpl();
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
		if (period == TimeSpan.Zero)
		{
			return new FastPeriodicTimer<TState>(state, action);
		}
		return new PeriodicTimer<TState>(state, period, action);
	}
}
