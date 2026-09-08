using Discord.API;

namespace Discord.WebSocket;

public class SocketMediaChannel : SocketForumChannel, IMediaChannel, IForumChannel, IMentionable, INestedChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable, IIntegrationChannel
{
	internal SocketMediaChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
	}

	internal new static SocketMediaChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketMediaChannel socketMediaChannel = new SocketMediaChannel(guild?.Discord, model.Id, guild);
		socketMediaChannel.Update(state, model);
		return socketMediaChannel;
	}

	internal override void Update(ClientState state, Channel model)
	{
		base.Update(state, model);
	}
}
