using Discord.API;

namespace Discord.Rest;

public class MessageBulkDeleteAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public int MessageCount { get; }

	private MessageBulkDeleteAuditLogData(ulong channelId, int count)
	{
		ChannelId = channelId;
		MessageCount = count;
	}

	internal static MessageBulkDeleteAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		return new MessageBulkDeleteAuditLogData(entry.TargetId.Value, entry.Options.Count.Value);
	}
}
