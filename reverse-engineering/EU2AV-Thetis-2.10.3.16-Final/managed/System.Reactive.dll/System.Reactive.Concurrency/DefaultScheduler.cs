using System.Reactive.Disposables;

namespace System.Reactive.Concurrency;

public sealed class DefaultScheduler : LocalScheduler, ISchedulerPeriodic
{
	private sealed class PeriodicallyScheduledWorkItem<TState> : IDisposable
	{
		private TState _state;

		private Func<TState, TState> _action;

		private readonly IDisposable _cancel;

		private readonly AsyncLock _gate = new AsyncLock();

		public PeriodicallyScheduledWorkItem(TState state, TimeSpan period, Func<TState, TState> action)
		{
			_state = state;
			_action = action;
			_cancel = Cal.StartPeriodicTimer(Tick, period);
		}

		private void Tick()
		{
			_gate.Wait(this, delegate(PeriodicallyScheduledWorkItem<TState> closureWorkItem)
			{
				closureWorkItem._state = closureWorkItem._action(closureWorkItem._state);
			});
		}

		public void Dispose()
		{
			_cancel.Dispose();
			_gate.Dispose();
			_action = Stubs<TState>.I;
		}
	}

	private sealed class LongRunning : ISchedulerLongRunning
	{
		private sealed class LongScheduledWorkItem<TState> : ICancelable, IDisposable
		{
			private readonly TState _state;

			private readonly Action<TState, ICancelable> _action;

			private SingleAssignmentDisposableValue _cancel;

			public bool IsDisposed => _cancel.IsDisposed;

			public LongScheduledWorkItem(TState state, Action<TState, ICancelable> action)
			{
				_state = state;
				_action = action;
				Cal.StartThread(delegate(object? thisObject)
				{
					LongScheduledWorkItem<TState> longScheduledWorkItem = (LongScheduledWorkItem<TState>)thisObject;
					longScheduledWorkItem._action(longScheduledWorkItem._state, longScheduledWorkItem);
				}, this);
			}

			public void Dispose()
			{
				_cancel.Dispose();
			}
		}

		public static readonly ISchedulerLongRunning Instance = new LongRunning();

		public IDisposable ScheduleLongRunning<TState>(TState state, Action<TState, ICancelable> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			return new LongScheduledWorkItem<TState>(state, action);
		}
	}

	private static readonly Lazy<DefaultScheduler> DefaultInstance = new Lazy<DefaultScheduler>(() => new DefaultScheduler());

	private static readonly IConcurrencyAbstractionLayer Cal = ConcurrencyAbstractionLayer.Current;

	public static DefaultScheduler Instance => DefaultInstance.Value;

	private DefaultScheduler()
	{
	}

	public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		UserWorkItem<TState> userWorkItem = new UserWorkItem<TState>(this, state, action);
		userWorkItem.CancelQueueDisposable = Cal.QueueUserWorkItem(delegate(object? closureWorkItem)
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
		userWorkItem.CancelQueueDisposable = Cal.StartTimer(delegate(object? closureWorkItem)
		{
			((UserWorkItem<TState>)closureWorkItem).Run();
		}, userWorkItem, dueTime2);
		return userWorkItem;
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
		return new PeriodicallyScheduledWorkItem<TState>(state, period, action);
	}

	protected override object? GetService(Type serviceType)
	{
		if (serviceType == typeof(ISchedulerLongRunning) && Cal.SupportsLongRunning)
		{
			return LongRunning.Instance;
		}
		return base.GetService(serviceType);
	}
}
