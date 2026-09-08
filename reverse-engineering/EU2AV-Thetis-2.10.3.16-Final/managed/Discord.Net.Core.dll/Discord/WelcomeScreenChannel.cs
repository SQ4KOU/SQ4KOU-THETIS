using System;

namespace Discord;

public class WelcomeScreenChannel : ISnowflakeEntity, IEntity<ulong>
{
	public ulong Id { get; }

	public string Description { get; }

	public IEmote Emoji { get; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	internal WelcomeScreenChannel(ulong id, string description, string emojiName = null, ulong? emoteId = null)
	{
		Id = id;
		Description = description;
		if (emoteId.HasValue && emoteId.Value != 0L)
		{
			Emoji = new Emote(emoteId.Value, emojiName, false);
		}
		else if (emojiName != null)
		{
			Emoji = new Emoji(emojiName);
		}
		else
		{
			Emoji = null;
		}
	}
}
