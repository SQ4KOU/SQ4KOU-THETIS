namespace Discord;

public readonly struct RecurrenceRuleByNWeekday
{
	public int Week { get; }

	public RecurrenceRuleWeekday Day { get; }

	internal RecurrenceRuleByNWeekday(int week, RecurrenceRuleWeekday day)
	{
		Week = week;
		Day = day;
	}
}
