using Discord.API;

namespace Discord.Rest;

public class RestMediaChannel : RestForumChannel, IMediaChannel, IForumChannel, IMentionable, INestedChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable, IIntegrationChannel
{
	internal RestMediaChannel(BaseDiscordClient client, IGuild guild, ulong id, ulong guildId)
		: base(client, guild, id, guildId)
	{
	}

	internal new static RestMediaChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestMediaChannel restMediaChannel = new RestMediaChannel(discord, guild, model.Id, guild?.Id ?? model.GuildId.Value);
		restMediaChannel.Update(model);
		return restMediaChannel;
	}

	internal override void Update(Channel model)
	{
		base.Update(model);
	}
}
