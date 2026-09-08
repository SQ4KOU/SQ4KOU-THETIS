using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class MessageReference
{
	internal Optional<ulong> InternalChannelId;

	public Optional<ulong> MessageId { get; internal set; }

	public ulong ChannelId => InternalChannelId.GetValueOrDefault();

	public Optional<ulong> GuildId { get; internal set; }

	public Optional<bool> FailIfNotExists { get; internal set; }

	public Optional<MessageReferenceType> ReferenceType { get; internal set; }

	private string DebuggerDisplay => string.Format("Channel ID: ({0}){1}", ChannelId, GuildId.IsSpecified ? $", Guild ID: ({GuildId.Value})" : "") + (MessageId.IsSpecified ? $", Message ID: ({MessageId.Value})" : "") + (FailIfNotExists.IsSpecified ? $", FailIfNotExists: ({FailIfNotExists.Value})" : "");

	public MessageReference(ulong? messageId = null, ulong? channelId = null, ulong? guildId = null, bool? failIfNotExists = null, MessageReferenceType referenceType = MessageReferenceType.Default)
	{
		MessageId = ((Optional<ulong>?)messageId) ?? Optional.Create<ulong>();
		InternalChannelId = ((Optional<ulong>?)channelId) ?? Optional.Create<ulong>();
		GuildId = ((Optional<ulong>?)guildId) ?? Optional.Create<ulong>();
		FailIfNotExists = ((Optional<bool>?)failIfNotExists) ?? Optional.Create<bool>();
		ReferenceType = referenceType;
	}

	public override string ToString()
	{
		return DebuggerDisplay;
	}
}
