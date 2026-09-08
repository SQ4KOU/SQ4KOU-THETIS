using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class RestUserMessage : RestMessage, IUserMessage, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private bool _isMentioningEveryone;

	private bool _isTTS;

	private bool _isPinned;

	private long? _editedTimestampTicks;

	private IUserMessage _referencedMessage;

	private ImmutableArray<Attachment> _attachments = ImmutableArray.Create<Attachment>();

	private ImmutableArray<Embed> _embeds = ImmutableArray.Create<Embed>();

	private ImmutableArray<ITag> _tags = ImmutableArray.Create<ITag>();

	private ImmutableArray<ulong> _roleMentionIds = ImmutableArray.Create<ulong>();

	private ImmutableArray<StickerItem> _stickers = ImmutableArray.Create<StickerItem>();

	public override bool IsTTS => _isTTS;

	public override bool IsPinned => _isPinned;

	public override bool IsSuppressed
	{
		get
		{
			if (base.Flags.HasValue)
			{
				return base.Flags.Value.HasFlag(MessageFlags.SuppressEmbeds);
			}
			return false;
		}
	}

	public override DateTimeOffset? EditedTimestamp => DateTimeUtils.FromTicks(_editedTimestampTicks);

	public override bool MentionedEveryone => _isMentioningEveryone;

	public override IReadOnlyCollection<Attachment> Attachments => _attachments;

	public override IReadOnlyCollection<Embed> Embeds => _embeds;

	public override IReadOnlyCollection<ulong> MentionedChannelIds => MessageHelper.FilterTagsByKey(TagType.ChannelMention, _tags);

	public override IReadOnlyCollection<ulong> MentionedRoleIds => _roleMentionIds;

	public override IReadOnlyCollection<ITag> Tags => _tags;

	public override IReadOnlyCollection<StickerItem> Stickers => _stickers;

	public IUserMessage ReferencedMessage => _referencedMessage;

	public IMessageInteractionMetadata InteractionMetadata { get; internal set; }

	public Poll? Poll { get; internal set; }

	public MessageResolvedData ResolvedData { get; internal set; }

	public IReadOnlyCollection<MessageSnapshot> ForwardedMessages { get; internal set; }

	private string DebuggerDisplay => string.Format("{0}: {1} ({2}{3})", base.Author, base.Content, base.Id, (Attachments.Count > 0) ? $", {Attachments.Count} Attachments" : "");

	internal RestUserMessage(BaseDiscordClient discord, ulong id, IMessageChannel channel, IUser author, MessageSource source)
		: base(discord, id, channel, author, source)
	{
	}

	internal new static RestUserMessage Create(BaseDiscordClient discord, IMessageChannel channel, IUser author, Message model)
	{
		RestUserMessage restUserMessage = new RestUserMessage(discord, model.Id, channel, author, MessageHelper.GetSource(model));
		restUserMessage.Update(model);
		return restUserMessage;
	}

	internal override void Update(Message model)
	{
		base.Update(model);
		if (model.IsTextToSpeech.IsSpecified)
		{
			_isTTS = model.IsTextToSpeech.Value;
		}
		if (model.Pinned.IsSpecified)
		{
			_isPinned = model.Pinned.Value;
		}
		if (model.EditedTimestamp.IsSpecified)
		{
			_editedTimestampTicks = model.EditedTimestamp.Value?.UtcTicks;
		}
		if (model.MentionEveryone.IsSpecified)
		{
			_isMentioningEveryone = model.MentionEveryone.Value;
		}
		if (model.RoleMentions.IsSpecified)
		{
			_roleMentionIds = ImmutableCollectionsMarshal.AsImmutableArray(model.RoleMentions.Value.ToArray());
		}
		if (model.Attachments.IsSpecified)
		{
			global::Discord.API.Attachment[] value = model.Attachments.Value;
			if (value.Length != 0)
			{
				ImmutableArray<Attachment>.Builder builder = ImmutableArray.CreateBuilder<Attachment>(value.Length);
				global::Discord.API.Attachment[] array = value;
				foreach (global::Discord.API.Attachment model2 in array)
				{
					builder.Add(Attachment.Create(model2, base.Discord));
				}
				_attachments = builder.ToImmutable();
			}
			else
			{
				_attachments = ImmutableArray<Attachment>.Empty;
			}
		}
		if (model.Embeds.IsSpecified)
		{
			global::Discord.API.Embed[] value2 = model.Embeds.Value;
			if (value2.Length != 0)
			{
				ImmutableArray<Embed>.Builder builder2 = ImmutableArray.CreateBuilder<Embed>(value2.Length);
				global::Discord.API.Embed[] array2 = value2;
				foreach (global::Discord.API.Embed model3 in array2)
				{
					builder2.Add(model3.ToEntity());
				}
				_embeds = builder2.ToImmutable();
			}
			else
			{
				_embeds = ImmutableArray<Embed>.Empty;
			}
		}
		ulong? guildId = (base.Channel as IGuildChannel)?.GuildId;
		IGuild guild = (guildId.HasValue ? ((IDiscordClient)base.Discord).GetGuildAsync(guildId.Value, CacheMode.CacheOnly, (RequestOptions)null).Result : null);
		if (model.Content.IsSpecified)
		{
			string value3 = model.Content.Value;
			_tags = MessageHelper.ParseTags(value3, null, guild, base.MentionedUsers);
			model.Content = value3;
		}
		Optional<Message> referencedMessage = model.ReferencedMessage;
		if (referencedMessage.IsSpecified && referencedMessage.Value != null)
		{
			Message value4 = model.ReferencedMessage.Value;
			IUser author = MessageHelper.GetAuthor(base.Discord, guild, value4.Author.Value, value4.WebhookId.ToNullable());
			_referencedMessage = Create(base.Discord, base.Channel, author, value4);
		}
		if (model.StickerItems.IsSpecified)
		{
			global::Discord.API.StickerItem[] value5 = model.StickerItems.Value;
			if (value5.Length != 0)
			{
				ImmutableArray<StickerItem>.Builder builder3 = ImmutableArray.CreateBuilder<StickerItem>(value5.Length);
				for (int j = 0; j < value5.Length; j++)
				{
					builder3.Add(new StickerItem(base.Discord, value5[j]));
				}
				_stickers = builder3.ToImmutable();
			}
			else
			{
				_stickers = ImmutableArray<StickerItem>.Empty;
			}
		}
		if (model.Resolved.IsSpecified)
		{
			ImmutableArray<RestUser> immutableArray = (model.Resolved.Value.Users.IsSpecified ? model.Resolved.Value.Users.Value.Select((KeyValuePair<string, User> x) => RestUser.Create(base.Discord, x.Value)).ToImmutableArray() : ImmutableArray<RestUser>.Empty);
			ImmutableArray<RestGuildUser> immutableArray2 = (model.Resolved.Value.Members.IsSpecified ? model.Resolved.Value.Members.Value.Select(delegate(KeyValuePair<string, GuildMember> x)
			{
				x.Value.User = (model.Resolved.Value.Users.Value.TryGetValue(x.Key, out var value6) ? value6 : null);
				return RestGuildUser.Create(base.Discord, guild, x.Value, guildId);
			}).ToImmutableArray() : ImmutableArray<RestGuildUser>.Empty);
			ImmutableArray<RestRole> immutableArray3 = (model.Resolved.Value.Roles.IsSpecified ? model.Resolved.Value.Roles.Value.Select((KeyValuePair<string, Role> x) => RestRole.Create(base.Discord, guild, x.Value)).ToImmutableArray() : ImmutableArray<RestRole>.Empty);
			ImmutableArray<RestChannel> immutableArray4 = (model.Resolved.Value.Channels.IsSpecified ? model.Resolved.Value.Channels.Value.Select((KeyValuePair<string, Channel> x) => RestChannel.Create(base.Discord, x.Value, guild)).ToImmutableArray() : ImmutableArray<RestChannel>.Empty);
			ResolvedData = new MessageResolvedData(immutableArray, immutableArray2, immutableArray3, immutableArray4);
		}
		if (model.InteractionMetadata.IsSpecified)
		{
			InteractionMetadata = model.InteractionMetadata.Value.ToInteractionMetadata(base.Discord);
		}
		if (model.MessageSnapshots.IsSpecified)
		{
			ForwardedMessages = model.MessageSnapshots.Value.Select((global::Discord.API.MessageSnapshot x) => new MessageSnapshot(RestMessage.Create(base.Discord, null, null, x.Message))).ToImmutableArray();
		}
		else
		{
			ForwardedMessages = ImmutableArray<MessageSnapshot>.Empty;
		}
		if (model.Poll.IsSpecified)
		{
			Poll = model.Poll.Value.ToEntity();
		}
	}

	public async Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		Update(await MessageHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task PinAsync(RequestOptions options = null)
	{
		return MessageHelper.PinAsync(this, base.Discord, options);
	}

	public Task UnpinAsync(RequestOptions options = null)
	{
		return MessageHelper.UnpinAsync(this, base.Discord, options);
	}

	public string Resolve(int startIndex, TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
	{
		return MentionUtils.Resolve(this, startIndex, userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);
	}

	public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
	{
		return MentionUtils.Resolve(this, 0, userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);
	}

	public Task CrosspostAsync(RequestOptions options = null)
	{
		if (!(base.Channel is INewsChannel))
		{
			throw new InvalidOperationException("Publishing (crossposting) is only valid in news channels.");
		}
		return MessageHelper.CrosspostAsync(this, base.Discord, options);
	}

	public Task EndPollAsync(RequestOptions options = null)
	{
		return MessageHelper.EndPollAsync(base.Channel.Id, base.Id, base.Discord, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetPollAnswerVotersAsync(uint answerId, int? limit = null, ulong? afterId = null, RequestOptions options = null)
	{
		return MessageHelper.GetPollAnswerVotersAsync(base.Channel.Id, base.Id, afterId, answerId, limit, base.Discord, options);
	}
}
