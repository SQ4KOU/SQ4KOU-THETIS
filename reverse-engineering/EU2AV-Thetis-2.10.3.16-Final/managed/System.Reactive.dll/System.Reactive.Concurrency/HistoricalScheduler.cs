using System.Collections.Generic;
using System.Diagnostics;

namespace System.Reactive.Concurrency;

[DebuggerDisplay("\\{ Clock = {Clock} Now = {Now.ToString(\"O\")} \\}")]
public class HistoricalScheduler : HistoricalSchedulerBase
{
	private readonly SchedulerQueue<DateTimeOffset> _queue = new SchedulerQueue<DateTimeOffset>();

	public HistoricalScheduler()
	{
	}

	public HistoricalScheduler(DateTimeOffset initialClock)
		: base(initialClock)
	{
	}

	public HistoricalScheduler(DateTimeOffset initialClock, IComparer<DateTimeOffset> comparer)
		: base(initialClock, comparer)
	{
	}

	protected override IScheduledItem<DateTimeOffset>? GetNext()
	{
		while (_queue.Count > 0)
		{
			ScheduledItem<DateTimeOffset> scheduledItem = _queue.Peek();
			if (scheduledItem.IsCanceled)
			{
				_queue.Dequeue();
				continue;
			}
			return scheduledItem;
		}
		return null;
	}

	public override IDisposable ScheduleAbsolute<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		ScheduledItem<DateTimeOffset, TState> si = null;
		Func<IScheduler, TState, IDisposable> action2 = delegate(IScheduler scheduler, TState arg)
		{
			_queue.Remove(si);
			return action(scheduler, arg);
		};
		si = new ScheduledItem<DateTimeOffset, TState>(this, state, action2, dueTime, base.Comparer);
		_queue.Enqueue(si);
		return si;
	}
}
