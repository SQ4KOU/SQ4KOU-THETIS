using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

public abstract class RestMessage : RestEntity<ulong>, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable, IUpdateable
{
	private long _timestampTicks;

	private ImmutableArray<RestReaction> _reactions = ImmutableArray.Create<RestReaction>();

	private ImmutableArray<RestUser> _userMentions = ImmutableArray.Create<RestUser>();

	public IMessageChannel Channel { get; }

	public IUser Author { get; }

	public MessageSource Source { get; }

	public string Content { get; private set; }

	public string CleanContent => MessageHelper.SanitizeMessage(this);

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public virtual bool IsTTS => false;

	public virtual bool IsPinned => false;

	public virtual bool IsSuppressed => false;

	public virtual DateTimeOffset? EditedTimestamp => null;

	public virtual bool MentionedEveryone => false;

	public RestThreadChannel Thread { get; private set; }

	IThreadChannel IMessage.Thread => Thread;

	public virtual IReadOnlyCollection<Attachment> Attachments => ImmutableArray.Create<Attachment>();

	public virtual IReadOnlyCollection<Embed> Embeds => ImmutableArray.Create<Embed>();

	public virtual IReadOnlyCollection<ulong> MentionedChannelIds => ImmutableArray.Create<ulong>();

	public virtual IReadOnlyCollection<ulong> MentionedRoleIds => ImmutableArray.Create<ulong>();

	public virtual IReadOnlyCollection<ITag> Tags => ImmutableArray.Create<ITag>();

	public virtual IReadOnlyCollection<StickerItem> Stickers => ImmutableArray.Create<StickerItem>();

	public DateTimeOffset Timestamp => DateTimeUtils.FromTicks(_timestampTicks);

	public MessageActivity Activity { get; private set; }

	public MessageApplication Application { get; private set; }

	public MessageReference Reference { get; private set; }

	public MessageInteraction<RestUser> Interaction { get; private set; }

	public MessageFlags? Flags { get; private set; }

	public MessageType Type { get; private set; }

	public MessageRoleSubscriptionData RoleSubscriptionData { get; private set; }

	public PurchaseNotification PurchaseNotification { get; private set; }

	public MessageCallData? CallData { get; private set; }

	public IReadOnlyCollection<IMessageComponent> Components { get; private set; }

	public IReadOnlyCollection<RestUser> MentionedUsers => _userMentions;

	IUser IMessage.Author => Author;

	IReadOnlyCollection<IAttachment> IMessage.Attachments => Attachments;

	IReadOnlyCollection<IEmbed> IMessage.Embeds => Embeds;

	IReadOnlyCollection<ulong> IMessage.MentionedUserIds => MentionedUsers.Select((RestUser x) => x.Id).ToImmutableArray();

	IReadOnlyCollection<IMessageComponent> IMessage.Components => Components;

	[Obsolete("This property will be deprecated soon. Use IUserMessage.InteractionMetadata instead.")]
	IMessageInteraction IMessage.Interaction => Interaction;

	IReadOnlyCollection<IStickerItem> IMessage.Stickers => Stickers;

	public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => _reactions.ToDictionary((RestReaction x) => x.Emote, (RestReaction x) => new ReactionMetadata
	{
		ReactionCount = x.Count,
		IsMe = x.Me,
		BurstColors = x.BurstColors,
		BurstCount = x.BurstCount,
		NormalCount = x.NormalCount
	});

	internal RestMessage(BaseDiscordClient discord, ulong id, IMessageChannel channel, IUser author, MessageSource source)
		: base(discord, id)
	{
		Channel = channel;
		Author = author;
		Source = source;
	}

	internal static RestMessage Create(BaseDiscordClient discord, IMessageChannel channel, IUser author, Message model)
	{
		if (model.Type == MessageType.Default || model.Type == MessageType.Reply || model.Type == MessageType.ApplicationCommand || model.Type == MessageType.ContextMenuCommand || model.Type == MessageType.ThreadStarterMessage)
		{
			return RestUserMessage.Create(discord, channel, author, model);
		}
		return RestSystemMessage.Create(discord, channel, author, model);
	}

	internal virtual void Update(Message model)
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
		if (model.Flags.IsSpecified)
		{
			Flags = model.Flags.Value;
		}
		if (model.Reactions.IsSpecified)
		{
			Reaction[] value = model.Reactions.Value;
			if (value.Length != 0)
			{
				ImmutableArray<RestReaction>.Builder builder = ImmutableArray.CreateBuilder<RestReaction>(value.Length);
				Reaction[] array = value;
				foreach (Reaction model2 in array)
				{
					builder.Add(RestReaction.Create(model2));
				}
				_reactions = builder.ToImmutable();
			}
			else
			{
				_reactions = ImmutableArray<RestReaction>.Empty;
			}
		}
		else
		{
			_reactions = ImmutableArray<RestReaction>.Empty;
		}
		if (model.Interaction.IsSpecified)
		{
			Interaction = new MessageInteraction<RestUser>(model.Interaction.Value.Id, model.Interaction.Value.Type, model.Interaction.Value.Name, RestUser.Create(base.Discord, model.Interaction.Value.User));
		}
		if (model.UserMentions.IsSpecified)
		{
			User[] value2 = model.UserMentions.Value;
			if (value2.Length != 0)
			{
				ImmutableArray<RestUser>.Builder builder2 = ImmutableArray.CreateBuilder<RestUser>(value2.Length);
				foreach (User user in value2)
				{
					if (user != null)
					{
						builder2.Add(RestUser.Create(base.Discord, user));
					}
				}
				_userMentions = builder2.ToImmutable();
			}
		}
		if (model.RoleSubscriptionData.IsSpecified)
		{
			RoleSubscriptionData = new MessageRoleSubscriptionData(model.RoleSubscriptionData.Value.SubscriptionListingId, model.RoleSubscriptionData.Value.TierName, model.RoleSubscriptionData.Value.MonthsSubscribed, model.RoleSubscriptionData.Value.IsRenewal);
		}
		if (model.Thread.IsSpecified)
		{
			Thread = RestThreadChannel.Create(base.Discord, new RestGuild(base.Discord, model.Thread.Value.GuildId.Value), model.Thread.Value);
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

	public async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetChannelMessageAsync(Channel.Id, base.Id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return MessageHelper.DeleteAsync(this, base.Discord, options);
	}

	public override string ToString()
	{
		return Content;
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
