namespace Discord;

public interface IMessageInteraction
{
	ulong Id { get; }

	InteractionType Type { get; }

	string Name { get; }

	IUser User { get; }
}
