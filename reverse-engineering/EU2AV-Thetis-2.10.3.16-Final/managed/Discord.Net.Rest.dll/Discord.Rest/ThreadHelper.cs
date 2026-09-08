using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class ThreadHelper
{
	public static Task<Channel> CreateThreadAsync(BaseDiscordClient client, ITextChannel channel, string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
	{
		if (channel is INewsChannel && type != ThreadType.NewsThread)
		{
			throw new ArgumentException(string.Format("{0} must be a {1} in News channels", "type", ThreadType.NewsThread));
		}
		StartThreadParams args = new StartThreadParams
		{
			Name = name,
			Duration = autoArchiveDuration,
			Type = type,
			Invitable = (invitable.HasValue ? ((Optional<bool>)invitable.Value) : Optional<bool>.Unspecified),
			Ratelimit = (slowmode.HasValue ? ((Optional<int?>)slowmode.Value) : Optional<int?>.Unspecified)
		};
		if (message != null)
		{
			return client.ApiClient.StartThreadAsync(channel.Id, message.Id, args, options);
		}
		return client.ApiClient.StartThreadAsync(channel.Id, args, options);
	}

	public static Task<Channel> ModifyAsync(IThreadChannel channel, BaseDiscordClient client, Action<ThreadChannelProperties> func, RequestOptions options)
	{
		ThreadChannelProperties threadChannelProperties = new ThreadChannelProperties();
		func(threadChannelProperties);
		Preconditions.AtMost(threadChannelProperties.AppliedTags.IsSpecified ? threadChannelProperties.AppliedTags.Value.Count() : 0, 5, "AppliedTags", "Forum post can have max 5 applied tags.");
		ModifyThreadParams args = new ModifyThreadParams
		{
			Name = threadChannelProperties.Name,
			Archived = threadChannelProperties.Archived,
			AutoArchiveDuration = threadChannelProperties.AutoArchiveDuration,
			Locked = threadChannelProperties.Locked,
			Slowmode = threadChannelProperties.SlowModeInterval,
			AppliedTags = threadChannelProperties.AppliedTags,
			Flags = threadChannelProperties.Flags
		};
		return client.ApiClient.ModifyThreadAsync(channel.Id, args, options);
	}

	public static async Task<IReadOnlyCollection<RestThreadChannel>> GetActiveThreadsAsync(IGuild guild, ulong channelId, BaseDiscordClient client, RequestOptions options)
	{
		return (from x in (await client.ApiClient.GetActiveThreadsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Threads
			where x.CategoryId == channelId
			select RestThreadChannel.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestThreadChannel>> GetPublicArchivedThreadsAsync(IGuildChannel channel, BaseDiscordClient client, int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return (await client.ApiClient.GetPublicArchivedThreadsAsync(channel.Id, before, limit, options)).Threads.Select((Channel x) => RestThreadChannel.Create(client, channel.Guild, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestThreadChannel>> GetPrivateArchivedThreadsAsync(IGuildChannel channel, BaseDiscordClient client, int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return (await client.ApiClient.GetPrivateArchivedThreadsAsync(channel.Id, before, limit, options)).Threads.Select((Channel x) => RestThreadChannel.Create(client, channel.Guild, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestThreadChannel>> GetJoinedPrivateArchivedThreadsAsync(IGuildChannel channel, BaseDiscordClient client, int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return (await client.ApiClient.GetJoinedPrivateArchivedThreadsAsync(channel.Id, before, limit, options)).Threads.Select((Channel x) => RestThreadChannel.Create(client, channel.Guild, x)).ToImmutableArray();
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestThreadUser>> GetUsersAsync(IThreadChannel channel, BaseDiscordClient client, int limit = 100, ulong? afterId = null, RequestOptions options = null)
	{
		return new PagedAsyncEnumerable<RestThreadUser>(limit, async delegate(PageInfo info, CancellationToken ct)
		{
			if (info.Position.HasValue)
			{
				afterId = info.Position.Value;
			}
			return (await client.ApiClient.ListThreadMembersAsync(channel.Id, afterId, limit, options)).Select((ThreadMember x) => RestThreadUser.Create(client, channel.Guild, x, channel)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestThreadUser> lastPage)
		{
			if (lastPage.Count != limit)
			{
				return false;
			}
			info.Position = lastPage.Max((RestThreadUser x) => x.Id);
			return true;
		}, afterId, limit);
	}

	public static async Task<RestThreadUser> GetUserAsync(ulong userId, IThreadChannel channel, BaseDiscordClient client, RequestOptions options = null)
	{
		ThreadMember model = await client.ApiClient.GetThreadMemberAsync(channel.Id, userId, options).ConfigureAwait(continueOnCapturedContext: false);
		return RestThreadUser.Create(client, channel.Guild, model, channel);
	}

	public static async Task<RestThreadChannel> CreatePostAsync(IForumChannel channel, BaseDiscordClient client, string title, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, ulong[] tagIds = null)
	{
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		Preconditions.AtMost((tagIds != null) ? tagIds.Length : 0, 5, "tagIds", "Forum post can have max 5 applied tags.");
		if (allowedMentions != null && allowedMentions.AllowedTypes.HasValue)
		{
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
			}
		}
		if (stickers != null)
		{
			Preconditions.AtMost(stickers.Length, 3, "stickers", "A max of 3 stickers are allowed.");
		}
		if (components?.Components?.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow) == true)
		{
			flags |= MessageFlags.ComponentsV2;
		}
		Preconditions.ValidateMessageFlags(flags);
		if (channel.Flags.HasFlag(ChannelFlags.RequireTag))
		{
			Preconditions.AtLeast((tagIds != null) ? tagIds.Length : 0, 1, "tagIds", "The channel " + channel.Name + " requires posts to have at least one tag.");
		}
		CreatePostParams args = new CreatePostParams
		{
			Title = title,
			ArchiveDuration = archiveDuration,
			Slowmode = slowmode,
			Message = new ForumThreadMessage
			{
				AllowedMentions = allowedMentions.ToModel(),
				Content = text,
				Embeds = (embeds.Any() ? ((Optional<Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<Discord.API.Embed[]>.Unspecified),
				Flags = flags,
				Components = ((components?.Components?.Any() == true) ? ((Optional<IMessageComponent[]>)components.Components.Select((IMessageComponent x) => x.ToModel()).ToArray()) : Optional<IMessageComponent[]>.Unspecified),
				Stickers = ((stickers?.Any() ?? false) ? ((Optional<ulong[]>)stickers.Select((ISticker x) => x.Id).ToArray()) : Optional<ulong[]>.Unspecified)
			},
			Tags = tagIds
		};
		Channel model = await client.ApiClient.CreatePostAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false);
		return RestThreadChannel.Create(client, channel.Guild, model);
	}

	public static async Task<RestThreadChannel> CreatePostAsync(IForumChannel channel, BaseDiscordClient client, string title, IEnumerable<FileAttachment> attachments, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags, ulong[] tagIds = null)
	{
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		Preconditions.AtMost((tagIds != null) ? tagIds.Length : 0, 5, "tagIds", "Forum post can have max 5 applied tags.");
		if (allowedMentions != null && allowedMentions.AllowedTypes.HasValue)
		{
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
			}
		}
		if (stickers != null)
		{
			Preconditions.AtMost(stickers.Length, 3, "stickers", "A max of 3 stickers are allowed.");
		}
		if (flags != MessageFlags.None && flags != MessageFlags.SuppressEmbeds)
		{
			throw new ArgumentException("The only valid MessageFlags are SuppressEmbeds and none.", "flags");
		}
		if (channel.Flags.HasFlag(ChannelFlags.RequireTag))
		{
			Preconditions.AtLeast((tagIds != null) ? tagIds.Length : 0, 1, "tagIds", "The channel " + channel.Name + " requires posts to have at least one tag.");
		}
		CreateMultipartPostAsync args = new CreateMultipartPostAsync(attachments.ToArray())
		{
			AllowedMentions = allowedMentions.ToModel(),
			ArchiveDuration = archiveDuration,
			Content = text,
			Embeds = (embeds.Any() ? ((Optional<Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<Discord.API.Embed[]>.Unspecified),
			Flags = flags,
			MessageComponent = ((components?.Components?.Any() == true) ? ((Optional<IMessageComponent[]>)components.Components.Select((IMessageComponent x) => x.ToModel()).ToArray()) : Optional<IMessageComponent[]>.Unspecified),
			Slowmode = slowmode,
			Stickers = ((stickers?.Any() ?? false) ? ((Optional<ulong[]>)stickers.Select((ISticker x) => x.Id).ToArray()) : Optional<ulong[]>.Unspecified),
			Title = title,
			TagIds = tagIds
		};
		Channel model = await client.ApiClient.CreatePostAsync(channel.Id, args, options);
		return RestThreadChannel.Create(client, channel.Guild, model);
	}
}
