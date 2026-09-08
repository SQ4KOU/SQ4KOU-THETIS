using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

public class GuildScheduledEventRecurrenceRuleProperties
{
	public DateTimeOffset StartsAt { get; set; }

	public RecurrenceFrequency Frequency { get; set; }

	public int Interval { get; set; }

	public HashSet<RecurrenceRuleWeekday> ByWeekday { get; set; }

	public List<RecurrenceRuleByNWeekday> ByNWeekday { get; set; }

	public HashSet<RecurrenceRuleMonth> ByMonth { get; set; }

	public HashSet<int> ByMonthDay { get; set; }

	public GuildScheduledEventRecurrenceRuleProperties()
	{
	}

	public GuildScheduledEventRecurrenceRuleProperties(DateTimeOffset startsAt, RecurrenceFrequency frequency, int interval, HashSet<RecurrenceRuleWeekday> byWeekday, IEnumerable<RecurrenceRuleByNWeekday> byNWeekday, HashSet<RecurrenceRuleMonth> byMonth, HashSet<int> byMonthDay)
	{
		StartsAt = startsAt;
		Frequency = frequency;
		Interval = interval;
		ByWeekday = byWeekday;
		ByNWeekday = byNWeekday?.ToList();
		ByMonth = byMonth;
		ByMonthDay = byMonthDay;
	}
}
