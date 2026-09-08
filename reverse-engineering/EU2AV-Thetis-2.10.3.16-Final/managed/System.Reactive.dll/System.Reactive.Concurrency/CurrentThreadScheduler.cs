using System.ComponentModel;

namespace System.Reactive.Concurrency;

public sealed class CurrentThreadScheduler : LocalScheduler
{
	private static class Trampoline
	{
		public static void Run(SchedulerQueue<TimeSpan> queue)
		{
			while (queue.Count > 0)
			{
				ScheduledItem<TimeSpan> scheduledItem = queue.Dequeue();
				if (!scheduledItem.IsCanceled)
				{
					TimeSpan timeout = scheduledItem.DueTime - Time;
					if (timeout.Ticks > 0)
					{
						ConcurrencyAbstractionLayer.Current.Sleep(timeout);
					}
					if (!scheduledItem.IsCanceled)
					{
						scheduledItem.Invoke();
					}
				}
			}
		}
	}

	private static readonly Lazy<CurrentThreadScheduler> StaticInstance = new Lazy<CurrentThreadScheduler>(() => new CurrentThreadScheduler());

	[ThreadStatic]
	private static SchedulerQueue<TimeSpan>? _threadLocalQueue;

	[ThreadStatic]
	private static IStopwatch? _clock;

	[ThreadStatic]
	private static bool _running;

	public static CurrentThreadScheduler Instance => StaticInstance.Value;

	private static TimeSpan Time
	{
		get
		{
			if (_clock == null)
			{
				_clock = ConcurrencyAbstractionLayer.Current.StartStopwatch();
			}
			return _clock.Elapsed;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This instance property is no longer supported. Use CurrentThreadScheduler.IsScheduleRequired instead.")]
	public bool ScheduleRequired => IsScheduleRequired;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static bool IsScheduleRequired => !_running;

	private CurrentThreadScheduler()
	{
	}

	private static SchedulerQueue<TimeSpan>? GetQueue()
	{
		return _threadLocalQueue;
	}

	private static void SetQueue(SchedulerQueue<TimeSpan>? newQueue)
	{
		_threadLocalQueue = newQueue;
	}

	public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		SchedulerQueue<TimeSpan> queue;
		if (!_running)
		{
			_running = true;
			if (dueTime > TimeSpan.Zero)
			{
				ConcurrencyAbstractionLayer.Current.Sleep(dueTime);
			}
			IDisposable result;
			try
			{
				result = action(this, state);
			}
			catch
			{
				SetQueue(null);
				_running = false;
				throw;
			}
			queue = GetQueue();
			if (queue != null)
			{
				try
				{
					Trampoline.Run(queue);
				}
				finally
				{
					SetQueue(null);
					_running = false;
				}
			}
			else
			{
				_running = false;
			}
			return result;
		}
		queue = GetQueue();
		if (queue == null)
		{
			queue = new SchedulerQueue<TimeSpan>(4);
			SetQueue(queue);
		}
		TimeSpan dueTime2 = Time + Scheduler.Normalize(dueTime);
		ScheduledItem<TimeSpan, TState> scheduledItem = new ScheduledItem<TimeSpan, TState>(this, state, action, dueTime2);
		queue.Enqueue(scheduledItem);
		return scheduledItem;
	}
}
