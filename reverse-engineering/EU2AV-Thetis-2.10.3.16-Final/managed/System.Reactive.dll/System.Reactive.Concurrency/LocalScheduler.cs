using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.PlatformServices;
using System.Threading;

namespace System.Reactive.Concurrency;

public abstract class LocalScheduler : IScheduler, IStopwatchProvider, IServiceProvider
{
	private abstract class WorkItem : IComparable<WorkItem>, IDisposable
	{
		public readonly LocalScheduler Scheduler;

		public readonly DateTimeOffset DueTime;

		private SingleAssignmentDisposableValue _disposable;

		private int _hasRun;

		protected WorkItem(LocalScheduler scheduler, DateTimeOffset dueTime)
		{
			Scheduler = scheduler;
			DueTime = dueTime;
			_hasRun = 0;
		}

		public void Invoke(IScheduler scheduler)
		{
			if (Interlocked.Exchange(ref _hasRun, 1) != 0)
			{
				return;
			}
			try
			{
				if (!_disposable.IsDisposed)
				{
					_disposable.Disposable = InvokeCore(scheduler);
				}
			}
			finally
			{
				SystemClock.Release();
			}
		}

		protected abstract IDisposable InvokeCore(IScheduler scheduler);

		public int CompareTo(WorkItem? other)
		{
			return DueTime.CompareTo(other.DueTime);
		}

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}

	private sealed class WorkItem<TState> : WorkItem
	{
		private readonly TState _state;

		private readonly Func<IScheduler, TState, IDisposable> _action;

		public WorkItem(LocalScheduler scheduler, TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
			: base(scheduler, dueTime)
		{
			_state = state;
			_action = action;
		}

		protected override IDisposable InvokeCore(IScheduler scheduler)
		{
			return _action(scheduler, _state);
		}
	}

	private static readonly object Gate = new object();

	private static readonly object StaticGate = new object();

	private static readonly PriorityQueue<WorkItem> LongTerm = new PriorityQueue<WorkItem>();

	private static readonly SerialDisposable NextLongTermTimer = new SerialDisposable();

	private static WorkItem? _nextLongTermWorkItem;

	private readonly PriorityQueue<WorkItem> _shortTerm = new PriorityQueue<WorkItem>();

	private readonly HashSet<IDisposable> _shortTermWork = new HashSet<IDisposable>();

	private static readonly TimeSpan ShortTerm = TimeSpan.FromSeconds(10.0);

	private const int MaxErrorRatio = 1000;

	private static readonly TimeSpan LongToShort = TimeSpan.FromSeconds(5.0);

	private static readonly TimeSpan RetryShort = TimeSpan.FromMilliseconds(50.0);

	private static readonly TimeSpan MaxSupportedTimer = TimeSpan.FromMilliseconds(4294967294.0);

	public virtual DateTimeOffset Now => Scheduler.Now;

	public virtual IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return Schedule(state, TimeSpan.Zero, action);
	}

	public abstract IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action);

	public virtual IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return Enqueue(state, dueTime, action);
	}

	public virtual IStopwatch StartStopwatch()
	{
		return ConcurrencyAbstractionLayer.Current.StartStopwatch();
	}

	object? IServiceProvider.GetService(Type serviceType)
	{
		return GetService(serviceType);
	}

	protected virtual object? GetService(Type serviceType)
	{
		if (serviceType == typeof(IStopwatchProvider))
		{
			return this;
		}
		if (serviceType == typeof(ISchedulerLongRunning))
		{
			return this as ISchedulerLongRunning;
		}
		if (serviceType == typeof(ISchedulerPeriodic))
		{
			return this as ISchedulerPeriodic;
		}
		return null;
	}

	protected LocalScheduler()
	{
		SystemClock.Register(this);
	}

	private IDisposable Enqueue<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		TimeSpan timeSpan = Scheduler.Normalize(dueTime - Now);
		if (timeSpan == TimeSpan.Zero)
		{
			return Schedule(state, TimeSpan.Zero, action);
		}
		SystemClock.AddRef();
		WorkItem<TState> workItem = new WorkItem<TState>(this, state, dueTime, action);
		if (timeSpan <= ShortTerm)
		{
			ScheduleShortTermWork(workItem);
		}
		else
		{
			ScheduleLongTermWork(workItem);
		}
		return workItem;
	}

	private void ScheduleShortTermWork(WorkItem item)
	{
		lock (Gate)
		{
			_shortTerm.Enqueue(item);
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			_shortTermWork.Add(singleAssignmentDisposable);
			TimeSpan dueTime = Scheduler.Normalize(item.DueTime - item.Scheduler.Now);
			singleAssignmentDisposable.Disposable = item.Scheduler.Schedule((this, singleAssignmentDisposable), dueTime, (IScheduler self, (LocalScheduler @this, SingleAssignmentDisposable d) tuple) => tuple.@this.ExecuteNextShortTermWorkItem(self, tuple.d));
		}
	}

	private IDisposable ExecuteNextShortTermWorkItem(IScheduler scheduler, IDisposable cancel)
	{
		WorkItem workItem = null;
		lock (Gate)
		{
			if (_shortTermWork.Remove(cancel) && _shortTerm.Count > 0)
			{
				workItem = _shortTerm.Dequeue();
			}
		}
		if (workItem != null)
		{
			if (workItem.DueTime - workItem.Scheduler.Now >= RetryShort)
			{
				ScheduleShortTermWork(workItem);
			}
			else
			{
				workItem.Invoke(scheduler);
			}
		}
		return Disposable.Empty;
	}

	private static void ScheduleLongTermWork(WorkItem item)
	{
		lock (StaticGate)
		{
			LongTerm.Enqueue(item);
			UpdateLongTermProcessingTimer();
		}
	}

	private static void UpdateLongTermProcessingTimer()
	{
		if (LongTerm.Count == 0)
		{
			return;
		}
		WorkItem workItem = LongTerm.Peek();
		if (workItem != _nextLongTermWorkItem)
		{
			TimeSpan timeSpan = Scheduler.Normalize(workItem.DueTime - workItem.Scheduler.Now);
			TimeSpan timeSpan2 = TimeSpan.FromTicks(Math.Max(timeSpan.Ticks / 1000, LongToShort.Ticks));
			TimeSpan dueTime = TimeSpan.FromTicks(Math.Min((timeSpan - timeSpan2).Ticks, MaxSupportedTimer.Ticks));
			_nextLongTermWorkItem = workItem;
			NextLongTermTimer.Disposable = ConcurrencyAbstractionLayer.Current.StartTimer(delegate
			{
				EvaluateLongTermQueue();
			}, null, dueTime);
		}
	}

	private static void EvaluateLongTermQueue()
	{
		lock (StaticGate)
		{
			while (LongTerm.Count > 0)
			{
				WorkItem workItem = LongTerm.Peek();
				if (Scheduler.Normalize(workItem.DueTime - workItem.Scheduler.Now) >= ShortTerm)
				{
					break;
				}
				WorkItem workItem2 = LongTerm.Dequeue();
				workItem2.Scheduler.ScheduleShortTermWork(workItem2);
			}
			_nextLongTermWorkItem = null;
			UpdateLongTermProcessingTimer();
		}
	}

	internal virtual void SystemClockChanged(object? sender, SystemClockChangedEventArgs args)
	{
		lock (StaticGate)
		{
			lock (Gate)
			{
				foreach (IDisposable item2 in _shortTermWork)
				{
					item2.Dispose();
				}
				_shortTermWork.Clear();
				while (_shortTerm.Count > 0)
				{
					WorkItem item = _shortTerm.Dequeue();
					LongTerm.Enqueue(item);
				}
				_nextLongTermWorkItem = null;
				EvaluateLongTermQueue();
			}
		}
	}
}
