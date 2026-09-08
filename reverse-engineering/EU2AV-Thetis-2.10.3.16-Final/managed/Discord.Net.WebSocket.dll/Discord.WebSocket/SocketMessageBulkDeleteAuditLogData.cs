using Discord.API;

namespace Discord.WebSocket;

public class SocketMessageBulkDeleteAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ChannelId { get; }

	public int MessageCount { get; }

	private SocketMessageBulkDeleteAuditLogData(ulong channelId, int count)
	{
		ChannelId = channelId;
		MessageCount = count;
	}

	internal static SocketMessageBulkDeleteAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketMessageBulkDeleteAuditLogData(entry.TargetId.Value, entry.Options.Count.Value);
	}
}
