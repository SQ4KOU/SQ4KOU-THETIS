using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketForumChannel : SocketGuildChannel, IForumChannel, IMentionable, INestedChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable, IIntegrationChannel
{
	public bool IsNsfw { get; private set; }

	public string Topic { get; private set; }

	public ThreadArchiveDuration DefaultAutoArchiveDuration { get; private set; }

	public IReadOnlyCollection<ForumTag> Tags { get; private set; }

	public int ThreadCreationInterval { get; private set; }

	public int DefaultSlowModeInterval { get; private set; }

	public string Mention => MentionUtils.MentionChannel(base.Id);

	public ulong? CategoryId { get; private set; }

	public IEmote DefaultReactionEmoji { get; private set; }

	public ForumSortOrder? DefaultSortOrder { get; private set; }

	public ForumLayout DefaultLayout { get; private set; }

	public ICategoryChannel Category
	{
		get
		{
			if (!CategoryId.HasValue)
			{
				return null;
			}
			return base.Guild.GetChannel(CategoryId.Value) as ICategoryChannel;
		}
	}

	internal SocketForumChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
	}

	internal new static SocketForumChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketForumChannel socketForumChannel = new SocketForumChannel(guild?.Discord, model.Id, guild);
		socketForumChannel.Update(state, model);
		return socketForumChannel;
	}

	internal override void Update(ClientState state, Channel model)
	{
		base.Update(state, model);
		IsNsfw = model.Nsfw.GetValueOrDefault(defaultValue: false);
		Topic = model.Topic.GetValueOrDefault();
		DefaultAutoArchiveDuration = model.AutoArchiveDuration.GetValueOrDefault(ThreadArchiveDuration.OneDay);
		if (model.ThreadRateLimitPerUser.IsSpecified)
		{
			DefaultSlowModeInterval = model.ThreadRateLimitPerUser.Value;
		}
		if (model.SlowMode.IsSpecified)
		{
			ThreadCreationInterval = model.SlowMode.Value;
		}
		DefaultSortOrder = model.DefaultSortOrder.GetValueOrDefault();
		Tags = (from x in model.ForumTags.GetValueOrDefault(Array.Empty<global::Discord.API.ForumTag>())
			select new ForumTag(x.Id, x.Name, x.EmojiId.GetValueOrDefault(null), x.EmojiName.GetValueOrDefault(), x.Moderated)).ToImmutableArray();
		if (model.DefaultReactionEmoji.IsSpecified && model.DefaultReactionEmoji.Value != null)
		{
			if (model.DefaultReactionEmoji.Value.EmojiId.HasValue && model.DefaultReactionEmoji.Value.EmojiId.Value != 0L)
			{
				DefaultReactionEmoji = new Emote(model.DefaultReactionEmoji.Value.EmojiId.GetValueOrDefault(), null, false);
			}
			else if (model.DefaultReactionEmoji.Value.EmojiName.IsSpecified)
			{
				DefaultReactionEmoji = new Emoji(model.DefaultReactionEmoji.Value.EmojiName.Value);
			}
			else
			{
				DefaultReactionEmoji = null;
			}
		}
		CategoryId = model.CategoryId.GetValueOrDefault();
		DefaultLayout = model.DefaultForumLayout.GetValueOrDefault();
	}

	public virtual Task ModifyAsync(Action<ForumChannelProperties> func, RequestOptions options = null)
	{
		return ForumHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public Task<RestThreadChannel> CreatePostAsync(string title, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, ForumTag[] tags = null)
	{
		return ThreadHelper.CreatePostAsync(this, base.Discord, title, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags, tags?.Select((ForumTag tag) => tag.Id).ToArray());
	}

	public async Task<RestThreadChannel> CreatePostWithFileAsync(string title, string filePath, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, ForumTag[] tags = null)
	{
		using FileAttachment file = new FileAttachment(filePath, null, null, isSpoiler);
		return await ThreadHelper.CreatePostAsync(this, base.Discord, title, new FileAttachment[1] { file }, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags, tags?.Select((ForumTag tag) => tag.Id).ToArray()).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<RestThreadChannel> CreatePostWithFileAsync(string title, Stream stream, string filename, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, ForumTag[] tags = null)
	{
		using FileAttachment file = new FileAttachment(stream, filename, null, isSpoiler);
		return await ThreadHelper.CreatePostAsync(this, base.Discord, title, new FileAttachment[1] { file }, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags, tags?.Select((ForumTag tag) => tag.Id).ToArray()).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<RestThreadChannel> CreatePostWithFileAsync(string title, FileAttachment attachment, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, ForumTag[] tags = null)
	{
		return ThreadHelper.CreatePostAsync(this, base.Discord, title, new FileAttachment[1] { attachment }, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags, tags?.Select((ForumTag tag) => tag.Id).ToArray());
	}

	public Task<RestThreadChannel> CreatePostWithFilesAsync(string title, IEnumerable<FileAttachment> attachments, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, ForumTag[] tags = null)
	{
		return ThreadHelper.CreatePostAsync(this, base.Discord, title, attachments, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags, tags?.Select((ForumTag tag) => tag.Id).ToArray());
	}

	public Task<IReadOnlyCollection<RestThreadChannel>> GetActiveThreadsAsync(RequestOptions options = null)
	{
		return ThreadHelper.GetActiveThreadsAsync(base.Guild, base.Id, base.Discord, options);
	}

	public Task<IReadOnlyCollection<RestThreadChannel>> GetJoinedPrivateArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return ThreadHelper.GetJoinedPrivateArchivedThreadsAsync(this, base.Discord, limit, before, options);
	}

	public Task<IReadOnlyCollection<RestThreadChannel>> GetPrivateArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return ThreadHelper.GetPrivateArchivedThreadsAsync(this, base.Discord, limit, before, options);
	}

	public Task<IReadOnlyCollection<RestThreadChannel>> GetPublicArchivedThreadsAsync(int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return ThreadHelper.GetPublicArchivedThreadsAsync(this, base.Discord, limit, before, options);
	}

	public Task<RestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
	{
		return ChannelHelper.CreateWebhookAsync(this, base.Discord, name, avatar, options);
	}

	public Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		return ChannelHelper.GetWebhookAsync(this, base.Discord, id, options);
	}

	public Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(RequestOptions options = null)
	{
		return ChannelHelper.GetWebhooksAsync(this, base.Discord, options);
	}

	async Task<IWebhook> IIntegrationChannel.CreateWebhookAsync(string name, Stream avatar, RequestOptions options)
	{
		return await CreateWebhookAsync(name, avatar, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IWebhook> IIntegrationChannel.GetWebhookAsync(ulong id, RequestOptions options)
	{
		return await GetWebhookAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IWebhook>> IIntegrationChannel.GetWebhooksAsync(RequestOptions options)
	{
		return await GetWebhooksAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IForumChannel.GetActiveThreadsAsync(RequestOptions options)
	{
		return await GetActiveThreadsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IForumChannel.GetPublicArchivedThreadsAsync(int? limit, DateTimeOffset? before, RequestOptions options)
	{
		return await GetPublicArchivedThreadsAsync(limit, before, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IForumChannel.GetPrivateArchivedThreadsAsync(int? limit, DateTimeOffset? before, RequestOptions options)
	{
		return await GetPrivateArchivedThreadsAsync(limit, before, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IThreadChannel>> IForumChannel.GetJoinedPrivateArchivedThreadsAsync(int? limit, DateTimeOffset? before, RequestOptions options)
	{
		return await GetJoinedPrivateArchivedThreadsAsync(limit, before, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostAsync(string title, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags, ForumTag[] tags)
	{
		return await CreatePostAsync(title, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags, tags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostWithFileAsync(string title, string filePath, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags, ForumTag[] tags)
	{
		return await CreatePostWithFileAsync(title, filePath, archiveDuration, slowmode, text, embed, options, isSpoiler, allowedMentions, components, stickers, embeds, flags, tags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostWithFileAsync(string title, Stream stream, string filename, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags, ForumTag[] tags)
	{
		return await CreatePostWithFileAsync(title, stream, filename, archiveDuration, slowmode, text, embed, options, isSpoiler, allowedMentions, components, stickers, embeds, flags, tags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostWithFileAsync(string title, FileAttachment attachment, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags, ForumTag[] tags)
	{
		return await CreatePostWithFileAsync(title, attachment, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags, tags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IThreadChannel> IForumChannel.CreatePostWithFilesAsync(string title, IEnumerable<FileAttachment> attachments, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags, ForumTag[] tags)
	{
		return await CreatePostWithFilesAsync(title, attachments, archiveDuration, slowmode, text, embed, options, allowedMentions, components, stickers, embeds, flags, tags);
	}

	public virtual async Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual async Task<IInviteMetadata> CreateInviteToApplicationAsync(ulong applicationId, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteToApplicationAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, applicationId, options);
	}

	public virtual async Task<IInviteMetadata> CreateInviteToApplicationAsync(DefaultApplications application, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		return await ChannelHelper.CreateInviteToApplicationAsync(this, base.Discord, maxAge, maxUses, isTemporary, isUnique, (ulong)application, options);
	}

	public virtual Task<IInviteMetadata> CreateInviteToStreamAsync(IUser user, int? maxAge, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		throw new NotImplementedException();
	}

	public virtual async Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
	{
		return await ChannelHelper.GetInvitesAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult(Category);
	}

	public virtual Task SyncPermissionsAsync(RequestOptions options = null)
	{
		return ChannelHelper.SyncPermissionsAsync(this, base.Discord, options);
	}
}
