namespace Discord;

public interface IIntegrationApplication
{
	ulong Id { get; }

	string Name { get; }

	string Icon { get; }

	string Description { get; }

	string Summary { get; }

	IUser Bot { get; }
}
