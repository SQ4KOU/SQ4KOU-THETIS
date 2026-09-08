using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SocketNewsChannel : SocketTextChannel, INewsChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, IIntegrationChannel
{
	public override int SlowModeInterval
	{
		get
		{
			throw new NotSupportedException("News channels do not support Slow Mode.");
		}
	}

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, News)";

	internal SocketNewsChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
	}

	internal new static SocketNewsChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketNewsChannel socketNewsChannel = new SocketNewsChannel(guild?.Discord, model.Id, guild);
		socketNewsChannel.Update(state, model);
		return socketNewsChannel;
	}

	public Task<ulong> FollowAnnouncementChannelAsync(ITextChannel channel, RequestOptions options = null)
	{
		return FollowAnnouncementChannelAsync(channel.Id, options);
	}

	public Task<ulong> FollowAnnouncementChannelAsync(ulong channelId, RequestOptions options = null)
	{
		return ChannelHelper.FollowAnnouncementChannelAsync(this, channelId, base.Discord, options);
	}
}
