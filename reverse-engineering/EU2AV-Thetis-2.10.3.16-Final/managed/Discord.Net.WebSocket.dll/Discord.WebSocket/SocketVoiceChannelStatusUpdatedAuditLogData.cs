using Discord.API;

namespace Discord.WebSocket;

public class SocketVoiceChannelStatusUpdatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public string Status { get; }

	public ulong ChannelId { get; }

	private SocketVoiceChannelStatusUpdatedAuditLogData(string status, ulong channelId)
	{
		Status = status;
		ChannelId = channelId;
	}

	internal static SocketVoiceChannelStatusUpdatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketVoiceChannelStatusUpdatedAuditLogData(entry.Options.Status, entry.TargetId.Value);
	}
}
