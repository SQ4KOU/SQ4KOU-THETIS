using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

public interface IMessage : ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	MessageType Type { get; }

	MessageSource Source { get; }

	bool IsTTS { get; }

	bool IsPinned { get; }

	bool IsSuppressed { get; }

	bool MentionedEveryone { get; }

	string Content { get; }

	string CleanContent { get; }

	DateTimeOffset Timestamp { get; }

	DateTimeOffset? EditedTimestamp { get; }

	IMessageChannel Channel { get; }

	IUser Author { get; }

	IThreadChannel Thread { get; }

	IReadOnlyCollection<IAttachment> Attachments { get; }

	IReadOnlyCollection<IEmbed> Embeds { get; }

	IReadOnlyCollection<ITag> Tags { get; }

	IReadOnlyCollection<ulong> MentionedChannelIds { get; }

	IReadOnlyCollection<ulong> MentionedRoleIds { get; }

	IReadOnlyCollection<ulong> MentionedUserIds { get; }

	MessageActivity Activity { get; }

	MessageApplication Application { get; }

	MessageReference Reference { get; }

	IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions { get; }

	IReadOnlyCollection<IMessageComponent> Components { get; }

	IReadOnlyCollection<IStickerItem> Stickers { get; }

	MessageFlags? Flags { get; }

	[Obsolete("This property will be deprecated soon. Use IUserMessage.InteractionMetadata instead.")]
	IMessageInteraction Interaction { get; }

	MessageRoleSubscriptionData RoleSubscriptionData { get; }

	PurchaseNotification PurchaseNotification { get; }

	MessageCallData? CallData { get; }

	Task AddReactionAsync(IEmote emote, RequestOptions options = null);

	Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null);

	Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null);

	Task RemoveAllReactionsAsync(RequestOptions options = null);

	Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null, ReactionType type = ReactionType.Normal);
}
