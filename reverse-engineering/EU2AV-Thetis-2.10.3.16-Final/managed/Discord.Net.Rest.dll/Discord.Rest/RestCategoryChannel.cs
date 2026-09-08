using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class RestCategoryChannel : RestGuildChannel, ICategoryChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Category)";

	internal RestCategoryChannel(BaseDiscordClient discord, IGuild guild, ulong id, ulong guildId)
		: base(discord, guild, id, guildId)
	{
	}

	internal new static RestCategoryChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestCategoryChannel restCategoryChannel = new RestCategoryChannel(discord, guild, model.Id, guild?.Id ?? model.GuildId.Value);
		restCategoryChannel.Update(model);
		return restCategoryChannel;
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		throw new NotSupportedException();
	}

	Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		throw new NotSupportedException();
	}
}
