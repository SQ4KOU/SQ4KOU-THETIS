using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

public class RestChannel : RestEntity<ulong>, IChannel, ISnowflakeEntity, IEntity<ulong>, IUpdateable
{
	public ChannelType ChannelType { get; internal set; }

	public virtual DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	string IChannel.Name => null;

	internal RestChannel(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestChannel Create(BaseDiscordClient discord, Channel model)
	{
		switch (model.Type)
		{
		case ChannelType.Text:
		case ChannelType.Voice:
		case ChannelType.News:
		case ChannelType.NewsThread:
		case ChannelType.PublicThread:
		case ChannelType.PrivateThread:
		case ChannelType.Stage:
		case ChannelType.Forum:
			return RestGuildChannel.Create(discord, new RestGuild(discord, model.GuildId.Value), model);
		case ChannelType.DM:
		case ChannelType.Group:
			return CreatePrivate(discord, model) as RestChannel;
		case ChannelType.Category:
			return RestCategoryChannel.Create(discord, new RestGuild(discord, model.GuildId.Value), model);
		default:
			return new RestChannel(discord, model.Id);
		}
	}

	internal static RestChannel Create(BaseDiscordClient discord, Channel model, IGuild guild)
	{
		switch (model.Type)
		{
		case ChannelType.Text:
		case ChannelType.Voice:
		case ChannelType.News:
		case ChannelType.NewsThread:
		case ChannelType.PublicThread:
		case ChannelType.PrivateThread:
		case ChannelType.Stage:
		case ChannelType.Forum:
		case ChannelType.Media:
			return RestGuildChannel.Create(discord, guild, model);
		case ChannelType.DM:
		case ChannelType.Group:
			return CreatePrivate(discord, model) as RestChannel;
		case ChannelType.Category:
			return RestCategoryChannel.Create(discord, guild, model);
		default:
			return new RestChannel(discord, model.Id);
		}
	}

	internal static IRestPrivateChannel CreatePrivate(BaseDiscordClient discord, Channel model)
	{
		return model.Type switch
		{
			ChannelType.DM => RestDMChannel.Create(discord, model), 
			ChannelType.Group => RestGroupChannel.Create(discord, model), 
			_ => throw new InvalidOperationException($"Unexpected channel type: {model.Type}"), 
		};
	}

	internal virtual void Update(Channel model)
	{
		ChannelType = model.Type;
	}

	public virtual Task UpdateAsync(RequestOptions options = null)
	{
		return Task.CompletedTask;
	}

	Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IUser>(null);
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return AsyncEnumerable.Empty<IReadOnlyCollection<IUser>>();
	}
}
