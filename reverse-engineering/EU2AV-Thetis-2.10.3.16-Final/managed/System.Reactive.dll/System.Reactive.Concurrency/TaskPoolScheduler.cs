using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Concurrency;

public sealed class TaskPoolScheduler : LocalScheduler, ISchedulerLongRunning, ISchedulerPeriodic
{
	private sealed class ScheduledWorkItem<TState> : IDisposable
	{
		private readonly TState _state;

		private readonly TaskPoolScheduler _scheduler;

		private readonly Func<IScheduler, TState, IDisposable> _action;

		private SerialDisposableValue _cancel;

		public ScheduledWorkItem(TaskPoolScheduler scheduler, TState state, Func<IScheduler, TState, IDisposable> action)
		{
			_state = state;
			_action = action;
			_scheduler = scheduler;
			CancellationDisposable cancellationDisposable = new CancellationDisposable();
			_cancel.Disposable = cancellationDisposable;
			scheduler._taskFactory.StartNew(delegate(object thisObject)
			{
				ScheduledWorkItem<TState> scheduledWorkItem = (ScheduledWorkItem<TState>)thisObject;
				scheduledWorkItem._cancel.Disposable = scheduledWorkItem._action(scheduledWorkItem._scheduler, scheduledWorkItem._state);
			}, this, cancellationDisposable.Token);
		}

		public void Dispose()
		{
			_cancel.Dispose();
		}
	}

	private sealed class SlowlyScheduledWorkItem<TState> : IDisposable
	{
		private readonly TState _state;

		private readonly TaskPoolScheduler _scheduler;

		private readonly Func<IScheduler, TState, IDisposable> _action;

		private MultipleAssignmentDisposableValue _cancel;

		public SlowlyScheduledWorkItem(TaskPoolScheduler scheduler, TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
		{
			_state = state;
			_action = action;
			_scheduler = scheduler;
			CancellationDisposable cancellationDisposable = new CancellationDisposable();
			_cancel.Disposable = cancellationDisposable;
			TaskHelpers.Delay(dueTime, cancellationDisposable.Token).ContinueWith(delegate(Task _, object args)
			{
				var (slowlyScheduledWorkItem, scheduler2, num) = ((SlowlyScheduledWorkItem<TState>, TaskScheduler, int))args;
				if (Environment.CurrentManagedThreadId == num)
				{
					new Task(RunWork, slowlyScheduledWorkItem).Start(scheduler2);
				}
				else
				{
					RunWork(slowlyScheduledWorkItem);
				}
				static void RunWork(object? thisObject)
				{
					SlowlyScheduledWorkItem<TState> slowlyScheduledWorkItem2 = (SlowlyScheduledWorkItem<TState>)thisObject;
					if (!slowlyScheduledWorkItem2._cancel.IsDisposed)
					{
						slowlyScheduledWorkItem2._cancel.Disposable = slowlyScheduledWorkItem2._action(slowlyScheduledWorkItem2._scheduler, slowlyScheduledWorkItem2._state);
					}
				}
			}, (this, scheduler._taskFactory.Scheduler ?? TaskScheduler.Default, Environment.CurrentManagedThreadId), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, scheduler._taskFactory.Scheduler ?? TaskScheduler.Default);
		}

		public void Dispose()
		{
			_cancel.Dispose();
		}
	}

	private sealed class LongScheduledWorkItem<TState> : ICancelable, IDisposable
	{
		private readonly TState _state;

		private readonly Action<TState, ICancelable> _action;

		private SingleAssignmentDisposableValue _cancel;

		public bool IsDisposed => _cancel.IsDisposed;

		public LongScheduledWorkItem(TaskPoolScheduler scheduler, TState state, Action<TState, ICancelable> action)
		{
			_state = state;
			_action = action;
			scheduler._taskFactory.StartNew(delegate(object thisObject)
			{
				LongScheduledWorkItem<TState> longScheduledWorkItem = (LongScheduledWorkItem<TState>)thisObject;
				longScheduledWorkItem._action(longScheduledWorkItem._state, longScheduledWorkItem);
			}, this, TaskCreationOptions.LongRunning);
		}

		public void Dispose()
		{
			_cancel.Dispose();
		}
	}

	private sealed class PeriodicallyScheduledWorkItem<TState> : IDisposable
	{
		private TState _state;

		private readonly TimeSpan _period;

		private readonly TaskFactory _taskFactory;

		private readonly Func<TState, TState> _action;

		private readonly AsyncLock _gate = new AsyncLock();

		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		public PeriodicallyScheduledWorkItem(TState state, TimeSpan period, Func<TState, TState> action, TaskFactory taskFactory)
		{
			_state = state;
			_period = period;
			_action = action;
			_taskFactory = taskFactory;
			MoveNext();
		}

		public void Dispose()
		{
			_cts.Cancel();
			_gate.Dispose();
		}

		private void MoveNext()
		{
			TaskHelpers.Delay(_period, _cts.Token).ContinueWith(delegate(Task _, object thisObject)
			{
				PeriodicallyScheduledWorkItem<TState> periodicallyScheduledWorkItem = (PeriodicallyScheduledWorkItem<TState>)thisObject;
				periodicallyScheduledWorkItem.MoveNext();
				periodicallyScheduledWorkItem._gate.Wait(periodicallyScheduledWorkItem, delegate(PeriodicallyScheduledWorkItem<TState> closureThis)
				{
					closureThis._state = closureThis._action(closureThis._state);
				});
			}, this, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, _taskFactory.Scheduler ?? TaskScheduler.Default);
		}
	}

	private static readonly Lazy<TaskPoolScheduler> LazyInstance = new Lazy<TaskPoolScheduler>(() => new TaskPoolScheduler(new TaskFactory(TaskScheduler.Default)));

	private readonly TaskFactory _taskFactory;

	public static TaskPoolScheduler Default => LazyInstance.Value;

	public TaskPoolScheduler(TaskFactory taskFactory)
	{
		_taskFactory = taskFactory ?? throw new ArgumentNullException("taskFactory");
	}

	public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return new ScheduledWorkItem<TState>(this, state, action);
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
		return ScheduleSlow(state, dueTime2, action);
	}

	private IDisposable ScheduleSlow<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		return new SlowlyScheduledWorkItem<TState>(this, state, dueTime, action);
	}

	public IDisposable ScheduleLongRunning<TState>(TState state, Action<TState, ICancelable> action)
	{
		return new LongScheduledWorkItem<TState>(this, state, action);
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
		return new PeriodicallyScheduledWorkItem<TState>(state, period, action, _taskFactory);
	}
}
