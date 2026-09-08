using Discord.API;

namespace Discord.WebSocket;

public class SocketVoiceChannelStatusDeleteAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ChannelId { get; }

	private SocketVoiceChannelStatusDeleteAuditLogData(ulong channelId)
	{
		ChannelId = channelId;
	}

	internal static SocketVoiceChannelStatusDeleteAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketVoiceChannelStatusDeleteAuditLogData(entry.TargetId.Value);
	}
}
