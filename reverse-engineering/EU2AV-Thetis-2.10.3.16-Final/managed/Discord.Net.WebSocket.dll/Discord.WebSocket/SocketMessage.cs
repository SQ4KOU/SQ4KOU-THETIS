using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public abstract class SocketMessage : SocketEntity<ulong>, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private long _timestampTicks;

	private readonly List<SocketReaction> _reactions = new List<SocketReaction>();

	private ImmutableArray<SocketUser> _userMentions = ImmutableArray<SocketUser>.Empty;

	public SocketUser Author { get; }

	public ISocketMessageChannel Channel { get; }

	public MessageSource Source { get; }

	public string Content { get; private set; }

	public string CleanContent => MessageHelper.SanitizeMessage(this);

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public virtual bool IsTTS => false;

	public virtual bool IsPinned => false;

	public virtual bool IsSuppressed => false;

	public virtual DateTimeOffset? EditedTimestamp => null;

	public virtual bool MentionedEveryone => false;

	public MessageActivity Activity { get; private set; }

	public MessageApplication Application { get; private set; }

	public MessageReference Reference { get; private set; }

	public IReadOnlyCollection<IMessageComponent> Components { get; private set; }

	public MessageInteraction<SocketUser> Interaction { get; private set; }

	public MessageFlags? Flags { get; private set; }

	public MessageType Type { get; private set; }

	public MessageRoleSubscriptionData RoleSubscriptionData { get; private set; }

	public PurchaseNotification PurchaseNotification { get; private set; }

	public SocketThreadChannel Thread { get; private set; }

	IThreadChannel IMessage.Thread => Thread;

	public MessageCallData? CallData { get; private set; }

	public virtual IReadOnlyCollection<Attachment> Attachments => ImmutableArray.Create<Attachment>();

	public virtual IReadOnlyCollection<Embed> Embeds => ImmutableArray.Create<Embed>();

	public virtual IReadOnlyCollection<SocketGuildChannel> MentionedChannels => Array.Empty<SocketGuildChannel>();

	public virtual IReadOnlyCollection<SocketRole> MentionedRoles => Array.Empty<SocketRole>();

	public virtual IReadOnlyCollection<ulong> MentionedRoleIds => Array.Empty<ulong>();

	public virtual IReadOnlyCollection<ITag> Tags => Array.Empty<ITag>();

	public virtual IReadOnlyCollection<SocketSticker> Stickers => Array.Empty<SocketSticker>();

	public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => (from r in _reactions
		group r by r.Emote).ToDictionary((IGrouping<IEmote, SocketReaction> x) => x.Key, (IGrouping<IEmote, SocketReaction> x) => new ReactionMetadata
	{
		ReactionCount = x.Count(),
		IsMe = x.Any((SocketReaction y) => y.UserId == base.Discord.CurrentUser.Id)
	});

	public IReadOnlyCollection<SocketUser> MentionedUsers => _userMentions;

	public IReadOnlyCollection<ulong> MentionedUserIds { get; private set; }

	public DateTimeOffset Timestamp => DateTimeUtils.FromTicks(_timestampTicks);

	IUser IMessage.Author => Author;

	IMessageChannel IMessage.Channel => Channel;

	IReadOnlyCollection<IAttachment> IMessage.Attachments => Attachments;

	IReadOnlyCollection<IEmbed> IMessage.Embeds => Embeds;

	IReadOnlyCollection<ulong> IMessage.MentionedChannelIds => MentionedChannels.Select((SocketGuildChannel x) => x.Id).ToImmutableArray();

	IReadOnlyCollection<ulong> IMessage.MentionedUserIds => MentionedUserIds;

	IReadOnlyCollection<IMessageComponent> IMessage.Components => Components;

	[Obsolete("This property will be deprecated soon. Use IUserMessage.InteractionMetadata instead.")]
	IMessageInteraction IMessage.Interaction => Interaction;

	IReadOnlyCollection<IStickerItem> IMessage.Stickers => Stickers;

	internal SocketMessage(DiscordSocketClient discord, ulong id, ISocketMessageChannel channel, SocketUser author, MessageSource source)
		: base(discord, id)
	{
		Channel = channel;
		Author = author;
		Source = source;
	}

	internal static SocketMessage Create(DiscordSocketClient discord, ClientState state, SocketUser author, ISocketMessageChannel channel, Message model)
	{
		if (model.Type == MessageType.Default || model.Type == MessageType.Reply || model.Type == MessageType.ApplicationCommand || model.Type == MessageType.ThreadStarterMessage || model.Type == MessageType.ContextMenuCommand)
		{
			return SocketUserMessage.Create(discord, state, author, channel, model);
		}
		return SocketSystemMessage.Create(discord, state, author, channel, model);
	}

	internal virtual void Update(ClientState state, Message model)
	{
		Type = model.Type;
		if (model.Timestamp.IsSpecified)
		{
			_timestampTicks = model.Timestamp.Value.UtcTicks;
		}
		if (model.Content.IsSpecified)
		{
			Content = model.Content.Value;
		}
		if (model.Application.IsSpecified)
		{
			Application = new MessageApplication
			{
				Id = model.Application.Value.Id,
				CoverImage = model.Application.Value.CoverImage,
				Description = model.Application.Value.Description,
				Icon = model.Application.Value.Icon,
				Name = model.Application.Value.Name
			};
		}
		if (model.Activity.IsSpecified)
		{
			Activity = new MessageActivity
			{
				Type = model.Activity.Value.Type.Value,
				PartyId = model.Activity.Value.PartyId.GetValueOrDefault()
			};
		}
		if (model.Reference.IsSpecified)
		{
			Reference = new MessageReference
			{
				GuildId = model.Reference.Value.GuildId,
				InternalChannelId = model.Reference.Value.ChannelId,
				MessageId = model.Reference.Value.MessageId,
				FailIfNotExists = model.Reference.Value.FailIfNotExists,
				ReferenceType = model.Reference.Value.Type
			};
		}
		Components = (model.Components.IsSpecified ? model.Components.Value.Select((IMessageComponent x) => x.ToEntity()).ToImmutableArray() : ImmutableArray<IMessageComponent>.Empty);
		if (model.UserMentions.IsSpecified)
		{
			if (model.UserMentions.Value.Length == 0)
			{
				_userMentions = ImmutableArray<SocketUser>.Empty;
				MentionedUserIds = ImmutableArray<ulong>.Empty;
			}
			else
			{
				MentionedUserIds = model.UserMentions.Value.Select((User x) => x.Id).ToImmutableArray();
				ImmutableArray<SocketUser>.Builder builder = ImmutableArray.CreateBuilder<SocketUser>(model.UserMentions.Value.Length);
				User[] value = model.UserMentions.Value;
				foreach (User user in value)
				{
					if (user != null)
					{
						SocketUser socketUser = null;
						if (Channel is SocketChannel socketChannel)
						{
							socketUser = socketChannel.GetUser(user.Id);
						}
						builder.Add(socketUser ?? SocketUnknownUser.Create(base.Discord, state, user));
					}
				}
				_userMentions = builder.ToImmutable();
			}
		}
		if (model.Interaction.IsSpecified)
		{
			Interaction = new MessageInteraction<SocketUser>(model.Interaction.Value.Id, model.Interaction.Value.Type, model.Interaction.Value.Name, SocketGlobalUser.Create(base.Discord, state, model.Interaction.Value.User));
		}
		if (model.Flags.IsSpecified)
		{
			Flags = model.Flags.Value;
		}
		if (model.RoleSubscriptionData.IsSpecified)
		{
			RoleSubscriptionData = new MessageRoleSubscriptionData(model.RoleSubscriptionData.Value.SubscriptionListingId, model.RoleSubscriptionData.Value.TierName, model.RoleSubscriptionData.Value.MonthsSubscribed, model.RoleSubscriptionData.Value.IsRenewal);
		}
		if (model.Thread.IsSpecified)
		{
			Thread = ((Channel as SocketGuildChannel)?.Guild)?.AddOrUpdateChannel(state, model.Thread.Value) as SocketThreadChannel;
		}
		if (model.PurchaseNotification.IsSpecified)
		{
			PurchaseNotification = new PurchaseNotification(model.PurchaseNotification.Value.Type, model.PurchaseNotification.Value.ProductPurchase.IsSpecified ? new GuildProductPurchase?(new GuildProductPurchase(model.PurchaseNotification.Value.ProductPurchase.Value.ListingId, model.PurchaseNotification.Value.ProductPurchase.Value.ProductName)) : ((GuildProductPurchase?)null));
		}
		if (model.Call.IsSpecified)
		{
			CallData = new MessageCallData(model.Call.Value.Participants, model.Call.Value.EndedTimestamp.ToNullable());
		}
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return MessageHelper.DeleteAsync(this, base.Discord, options);
	}

	public override string ToString()
	{
		return Content;
	}

	internal SocketMessage Clone()
	{
		return MemberwiseClone() as SocketMessage;
	}

	internal void AddReaction(SocketReaction reaction)
	{
		_reactions.Add(reaction);
	}

	internal void RemoveReaction(SocketReaction reaction)
	{
		if (_reactions.Contains(reaction))
		{
			_reactions.Remove(reaction);
		}
	}

	internal void ClearReactions()
	{
		_reactions.Clear();
	}

	internal void RemoveReactionsForEmote(IEmote emote)
	{
		_reactions.RemoveAll((SocketReaction x) => x.Emote.Equals(emote));
	}

	public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
	{
		return MessageHelper.AddReactionAsync(this, emote, base.Discord, options);
	}

	public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
	{
		return MessageHelper.RemoveReactionAsync(this, user.Id, emote, base.Discord, options);
	}

	public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null)
	{
		return MessageHelper.RemoveReactionAsync(this, userId, emote, base.Discord, options);
	}

	public Task RemoveAllReactionsAsync(RequestOptions options = null)
	{
		return MessageHelper.RemoveAllReactionsAsync(this, base.Discord, options);
	}

	public Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions options = null)
	{
		return MessageHelper.RemoveAllReactionsForEmoteAsync(this, emote, base.Discord, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emote, int limit, RequestOptions options = null, ReactionType type = ReactionType.Normal)
	{
		return MessageHelper.GetReactionUsersAsync(this, emote, limit, base.Discord, type, options);
	}
}
