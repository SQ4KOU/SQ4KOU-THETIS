using System;
using Newtonsoft.Json;

namespace Discord.API;

internal class Message
{
	[JsonProperty("id")]
	public ulong Id { get; set; }

	[JsonProperty("type")]
	public MessageType Type { get; set; }

	[JsonProperty("channel_id")]
	public ulong ChannelId { get; set; }

	[JsonProperty("guild_id")]
	public Optional<ulong> GuildId { get; set; }

	[JsonProperty("webhook_id")]
	public Optional<ulong> WebhookId { get; set; }

	[JsonProperty("author")]
	public Optional<User> Author { get; set; }

	[JsonProperty("member")]
	public Optional<GuildMember> Member { get; set; }

	[JsonProperty("content")]
	public Optional<string> Content { get; set; }

	[JsonProperty("timestamp")]
	public Optional<DateTimeOffset> Timestamp { get; set; }

	[JsonProperty("edited_timestamp")]
	public Optional<DateTimeOffset?> EditedTimestamp { get; set; }

	[JsonProperty("tts")]
	public Optional<bool> IsTextToSpeech { get; set; }

	[JsonProperty("mention_everyone")]
	public Optional<bool> MentionEveryone { get; set; }

	[JsonProperty("mentions")]
	public Optional<User[]> UserMentions { get; set; }

	[JsonProperty("mention_roles")]
	public Optional<ulong[]> RoleMentions { get; set; }

	[JsonProperty("attachments")]
	public Optional<Attachment[]> Attachments { get; set; }

	[JsonProperty("embeds")]
	public Optional<Embed[]> Embeds { get; set; }

	[JsonProperty("pinned")]
	public Optional<bool> Pinned { get; set; }

	[JsonProperty("reactions")]
	public Optional<Reaction[]> Reactions { get; set; }

	[JsonProperty("activity")]
	public Optional<MessageActivity> Activity { get; set; }

	[JsonProperty("application")]
	public Optional<MessageApplication> Application { get; set; }

	[JsonProperty("message_reference")]
	public Optional<MessageReference> Reference { get; set; }

	[JsonProperty("flags")]
	public Optional<MessageFlags> Flags { get; set; }

	[JsonProperty("allowed_mentions")]
	public Optional<AllowedMentions> AllowedMentions { get; set; }

	[JsonProperty("referenced_message")]
	public Optional<Message> ReferencedMessage { get; set; }

	[JsonProperty("components")]
	public Optional<IMessageComponent[]> Components { get; set; }

	[JsonProperty("interaction")]
	public Optional<MessageInteraction> Interaction { get; set; }

	[JsonProperty("sticker_items")]
	public Optional<StickerItem[]> StickerItems { get; set; }

	[JsonProperty("role_subscription_data")]
	public Optional<MessageRoleSubscriptionData> RoleSubscriptionData { get; set; }

	[JsonProperty("thread")]
	public Optional<Channel> Thread { get; set; }

	[JsonProperty("resolved")]
	public Optional<MessageComponentInteractionDataResolved> Resolved { get; set; }

	[JsonProperty("interaction_metadata")]
	public Optional<MessageInteractionMetadata> InteractionMetadata { get; set; }

	[JsonProperty("message_snapshots")]
	public Optional<MessageSnapshot[]> MessageSnapshots { get; set; }

	[JsonProperty("poll")]
	public Optional<Poll> Poll { get; set; }

	[JsonProperty("purchase_notification")]
	public Optional<MessagePurchaseNotification> PurchaseNotification { get; set; }

	[JsonProperty("call")]
	public Optional<MessageCallData> Call { get; set; }
}
