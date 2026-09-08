using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class RestNewsChannel : RestTextChannel, INewsChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, IIntegrationChannel
{
	public override int SlowModeInterval
	{
		get
		{
			throw new NotSupportedException("News channels do not support Slow Mode.");
		}
	}

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, News)";

	internal RestNewsChannel(BaseDiscordClient discord, IGuild guild, ulong id, ulong guildId)
		: base(discord, guild, id, guildId)
	{
	}

	internal new static RestNewsChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestNewsChannel restNewsChannel = new RestNewsChannel(discord, guild, model.Id, guild?.Id ?? model.GuildId.Value);
		restNewsChannel.Update(model);
		return restNewsChannel;
	}

	public Task<ulong> FollowAnnouncementChannelAsync(ulong channelId, RequestOptions options = null)
	{
		return ChannelHelper.FollowAnnouncementChannelAsync(this, channelId, base.Discord, options);
	}
}
