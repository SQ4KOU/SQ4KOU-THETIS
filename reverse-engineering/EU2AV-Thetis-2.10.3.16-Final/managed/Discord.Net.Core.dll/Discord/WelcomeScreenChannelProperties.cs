using System;

namespace Discord;

public class WelcomeScreenChannelProperties : ISnowflakeEntity, IEntity<ulong>
{
	public ulong Id { get; set; }

	public string Description { get; set; }

	public IEmote Emoji { get; set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	public WelcomeScreenChannelProperties(ulong id, string description, IEmote emoji = null)
	{
		Id = id;
		Description = description;
		Emoji = emoji;
	}

	public WelcomeScreenChannelProperties()
	{
	}

	public static WelcomeScreenChannelProperties FromWelcomeScreenChannel(WelcomeScreenChannel channel)
	{
		return new WelcomeScreenChannelProperties(channel.Id, channel.Description, channel.Emoji);
	}
}
