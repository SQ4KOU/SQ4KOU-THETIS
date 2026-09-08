namespace Discord;

public class RecurrenceRuleByNWeekdayProperties
{
	public int Week { get; set; }

	public RecurrenceRuleWeekday Day { get; set; }

	public RecurrenceRuleByNWeekdayProperties()
	{
	}

	public RecurrenceRuleByNWeekdayProperties(int week, RecurrenceRuleWeekday day)
	{
		Week = week;
		Day = day;
	}
}
