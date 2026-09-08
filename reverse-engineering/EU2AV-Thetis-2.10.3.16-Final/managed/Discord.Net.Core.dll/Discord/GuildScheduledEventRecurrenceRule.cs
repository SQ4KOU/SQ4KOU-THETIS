using System;
using System.Collections.Generic;

namespace Discord;

public readonly struct GuildScheduledEventRecurrenceRule
{
	public DateTimeOffset StartsAt { get; }

	public DateTimeOffset? EndsAt { get; }

	public RecurrenceFrequency Frequency { get; }

	public int Interval { get; }

	public IReadOnlyCollection<RecurrenceRuleWeekday> ByWeekday { get; }

	public IReadOnlyCollection<RecurrenceRuleByNWeekday> ByNWeekday { get; }

	public IReadOnlyCollection<RecurrenceRuleMonth> ByMonth { get; }

	public IReadOnlyCollection<int> ByMonthDay { get; }

	public IReadOnlyCollection<int> ByYearDay { get; }

	public int? Count { get; }

	internal GuildScheduledEventRecurrenceRule(DateTimeOffset startsAt, DateTimeOffset? endsAt, RecurrenceFrequency frequency, int interval, IReadOnlyCollection<RecurrenceRuleWeekday> byWeekday, IReadOnlyCollection<RecurrenceRuleByNWeekday> byNWeekday, IReadOnlyCollection<RecurrenceRuleMonth> byMonth, IReadOnlyCollection<int> byMonthDay, IReadOnlyCollection<int> byYearDay, int? count)
	{
		StartsAt = startsAt;
		EndsAt = endsAt;
		Frequency = frequency;
		Interval = interval;
		ByWeekday = byWeekday;
		ByNWeekday = byNWeekday;
		ByMonth = byMonth;
		ByMonthDay = byMonthDay;
		ByYearDay = byYearDay;
		Count = count;
	}
}
