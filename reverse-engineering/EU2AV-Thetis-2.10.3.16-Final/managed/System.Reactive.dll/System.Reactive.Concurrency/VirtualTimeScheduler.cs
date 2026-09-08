using System.Collections.Generic;

namespace System.Reactive.Concurrency;

public abstract class VirtualTimeScheduler<TAbsolute, TRelative> : VirtualTimeSchedulerBase<TAbsolute, TRelative> where TAbsolute : IComparable<TAbsolute>
{
	private readonly SchedulerQueue<TAbsolute> _queue = new SchedulerQueue<TAbsolute>();

	protected VirtualTimeScheduler()
	{
	}

	protected VirtualTimeScheduler(TAbsolute initialClock, IComparer<TAbsolute> comparer)
		: base(initialClock, comparer)
	{
	}

	protected override IScheduledItem<TAbsolute>? GetNext()
	{
		lock (_queue)
		{
			while (_queue.Count > 0)
			{
				ScheduledItem<TAbsolute> scheduledItem = _queue.Peek();
				if (scheduledItem.IsCanceled)
				{
					_queue.Dequeue();
					continue;
				}
				return scheduledItem;
			}
		}
		return null;
	}

	public override IDisposable ScheduleAbsolute<TState>(TState state, TAbsolute dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		ScheduledItem<TAbsolute, TState> si = null;
		Func<IScheduler, TState, IDisposable> action2 = delegate(IScheduler scheduler, TState arg)
		{
			lock (_queue)
			{
				_queue.Remove(si);
			}
			return action(scheduler, arg);
		};
		si = new ScheduledItem<TAbsolute, TState>(this, state, action2, dueTime, base.Comparer);
		lock (_queue)
		{
			_queue.Enqueue(si);
		}
		return si;
	}
}
