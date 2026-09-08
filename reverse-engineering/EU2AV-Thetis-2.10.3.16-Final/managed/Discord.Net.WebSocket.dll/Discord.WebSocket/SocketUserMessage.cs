using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SocketUserMessage : SocketMessage, IUserMessage, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private bool _isMentioningEveryone;

	private bool _isTTS;

	private bool _isPinned;

	private long? _editedTimestampTicks;

	private IUserMessage _referencedMessage;

	private ImmutableArray<Attachment> _attachments = ImmutableArray<Attachment>.Empty;

	private ImmutableArray<Embed> _embeds = ImmutableArray<Embed>.Empty;

	private ImmutableArray<ITag> _tags = ImmutableArray<ITag>.Empty;

	private ImmutableArray<SocketRole> _roleMentions;

	private ImmutableArray<ulong> _roleMentionsIds = ImmutableArray<ulong>.Empty;

	private ImmutableArray<SocketSticker> _stickers = ImmutableArray<SocketSticker>.Empty;

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

	public override IReadOnlyCollection<ITag> Tags => _tags;

	public override IReadOnlyCollection<SocketGuildChannel> MentionedChannels => MessageHelper.FilterTagsByValue<SocketGuildChannel>(TagType.ChannelMention, _tags);

	public override IReadOnlyCollection<SocketRole> MentionedRoles => _roleMentions;

	public override IReadOnlyCollection<ulong> MentionedRoleIds => _roleMentionsIds;

	public override IReadOnlyCollection<SocketSticker> Stickers => _stickers;

	public IUserMessage ReferencedMessage => _referencedMessage;

	public IMessageInteractionMetadata InteractionMetadata { get; internal set; }

	public Poll? Poll { get; internal set; }

	public MessageResolvedData ResolvedData { get; internal set; }

	public IReadOnlyCollection<MessageSnapshot> ForwardedMessages { get; internal set; }

	private string DebuggerDisplay => string.Format("{0}: {1} ({2}{3})", base.Author, base.Content, base.Id, (Attachments.Count > 0) ? $", {Attachments.Count} Attachments" : "");

	internal SocketUserMessage(DiscordSocketClient discord, ulong id, ISocketMessageChannel channel, SocketUser author, MessageSource source)
		: base(discord, id, channel, author, source)
	{
		_roleMentions = ImmutableArray.Create<SocketRole>();
	}

	internal new static SocketUserMessage Create(DiscordSocketClient discord, ClientState state, SocketUser author, ISocketMessageChannel channel, Message model)
	{
		SocketUserMessage socketUserMessage = new SocketUserMessage(discord, model.Id, channel, author, MessageHelper.GetSource(model));
		socketUserMessage.Update(state, model);
		return socketUserMessage;
	}

	internal override void Update(ClientState state, Message model)
	{
		base.Update(state, model);
		SocketGuild guild = (base.Channel as SocketGuildChannel)?.Guild;
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
			if (guild != null)
			{
				_roleMentions = model.RoleMentions.Value.Select((ulong x) => guild.GetRole(x)).ToImmutableArray();
			}
			_roleMentionsIds = ((IEnumerable<ulong>)model.RoleMentions.Value).ToImmutableArray();
		}
		if (model.Attachments.IsSpecified)
		{
			global::Discord.API.Attachment[] value = model.Attachments.Value;
			if (value.Length != 0)
			{
				ImmutableArray<Attachment>.Builder builder = ImmutableArray.CreateBuilder<Attachment>(value.Length);
				for (int num = 0; num < value.Length; num++)
				{
					builder.Add(Attachment.Create(value[num], base.Discord));
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
				for (int num2 = 0; num2 < value2.Length; num2++)
				{
					builder2.Add(value2[num2].ToEntity());
				}
				_embeds = builder2.ToImmutable();
			}
			else
			{
				_embeds = ImmutableArray<Embed>.Empty;
			}
		}
		if (model.Content.IsSpecified)
		{
			string value3 = model.Content.Value;
			_tags = MessageHelper.ParseTags(value3, base.Channel, guild, base.MentionedUsers);
			model.Content = value3;
		}
		if (model.ReferencedMessage.IsSpecified && model.ReferencedMessage.Value != null)
		{
			Message value4 = model.ReferencedMessage.Value;
			ulong? num3 = value4.WebhookId.ToNullable();
			SocketUser socketUser = null;
			if (value4.Author.IsSpecified)
			{
				socketUser = ((guild == null) ? (base.Channel as SocketChannel)?.GetUser(value4.Author.Value.Id) : ((!num3.HasValue) ? ((SocketUser)guild.GetUser(value4.Author.Value.Id)) : ((SocketUser)SocketWebhookUser.Create(guild, state, value4.Author.Value, num3.Value))));
				if (socketUser == null)
				{
					socketUser = SocketUnknownUser.Create(base.Discord, state, value4.Author.Value);
				}
			}
			else
			{
				socketUser = new SocketUnknownUser(base.Discord, 0uL);
			}
			_referencedMessage = Create(base.Discord, state, socketUser, base.Channel, value4);
		}
		if (model.StickerItems.IsSpecified)
		{
			global::Discord.API.StickerItem[] value5 = model.StickerItems.Value;
			if (value5.Length != 0)
			{
				ImmutableArray<SocketSticker>.Builder builder3 = ImmutableArray.CreateBuilder<SocketSticker>(value5.Length);
				global::Discord.API.StickerItem[] array = value5;
				foreach (global::Discord.API.StickerItem stickerItem in array)
				{
					SocketSticker socketSticker = null;
					if (guild != null)
					{
						socketSticker = guild.GetSticker(stickerItem.Id);
					}
					if (socketSticker == null)
					{
						socketSticker = base.Discord.GetSticker(stickerItem.Id);
					}
					if (base.Discord.AlwaysResolveStickers)
					{
						global::Discord.API.StickerItem item = stickerItem;
						socketSticker = Task.Run(async () => await base.Discord.GetStickerAsync(item.Id).ConfigureAwait(continueOnCapturedContext: false)).GetAwaiter().GetResult();
					}
					if (socketSticker == null)
					{
						socketSticker = SocketUnknownSticker.Create(base.Discord, stickerItem);
					}
					builder3.Add(socketSticker);
				}
				_stickers = builder3.ToImmutable();
			}
			else
			{
				_stickers = ImmutableArray<SocketSticker>.Empty;
			}
		}
		if (model.Resolved.IsSpecified)
		{
			ImmutableArray<RestUser> immutableArray = (model.Resolved.Value.Users.IsSpecified ? model.Resolved.Value.Users.Value.Select((KeyValuePair<string, User> x) => RestUser.Create(base.Discord, x.Value)).ToImmutableArray() : ImmutableArray<RestUser>.Empty);
			ImmutableArray<RestGuildUser> immutableArray2 = (model.Resolved.Value.Members.IsSpecified ? model.Resolved.Value.Members.Value.Select(delegate(KeyValuePair<string, GuildMember> x)
			{
				x.Value.User = (model.Resolved.Value.Users.Value.TryGetValue(x.Key, out var value6) ? value6 : null);
				return RestGuildUser.Create(base.Discord, guild, x.Value);
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

	public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		return MessageHelper.ModifyAsync(this, base.Discord, func, options);
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

	internal new SocketUserMessage Clone()
	{
		return MemberwiseClone() as SocketUserMessage;
	}
}
