namespace Discord;

public interface IActivity
{
	string Name { get; }

	ActivityType Type { get; }

	ActivityProperties Flags { get; }

	string Details { get; }
}
