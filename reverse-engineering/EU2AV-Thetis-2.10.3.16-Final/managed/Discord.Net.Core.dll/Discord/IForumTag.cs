namespace Discord;

public interface IForumTag
{
	ulong? Id { get; }

	string Name { get; }

	IEmote? Emoji { get; }

	bool IsModerated { get; }
}
