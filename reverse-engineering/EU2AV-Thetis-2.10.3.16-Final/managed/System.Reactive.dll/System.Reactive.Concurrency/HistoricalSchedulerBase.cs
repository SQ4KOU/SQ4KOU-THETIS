using System.Collections.Generic;

namespace System.Reactive.Concurrency;

public abstract class HistoricalSchedulerBase : VirtualTimeSchedulerBase<DateTimeOffset, TimeSpan>
{
	protected HistoricalSchedulerBase()
		: base(DateTimeOffset.MinValue, (IComparer<DateTimeOffset>)Comparer<DateTimeOffset>.Default)
	{
	}

	protected HistoricalSchedulerBase(DateTimeOffset initialClock)
		: base(initialClock, (IComparer<DateTimeOffset>)Comparer<DateTimeOffset>.Default)
	{
	}

	protected HistoricalSchedulerBase(DateTimeOffset initialClock, IComparer<DateTimeOffset> comparer)
		: base(initialClock, comparer)
	{
	}

	protected override DateTimeOffset Add(DateTimeOffset absolute, TimeSpan relative)
	{
		return absolute.Add(relative);
	}

	protected override DateTimeOffset ToDateTimeOffset(DateTimeOffset absolute)
	{
		return absolute;
	}

	protected override TimeSpan ToRelative(TimeSpan timeSpan)
	{
		return timeSpan;
	}
}
