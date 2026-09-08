using Discord.API;

namespace Discord.Rest;

public class VoiceChannelStatusUpdateAuditLogData : IAuditLogData
{
	public string Status { get; }

	public ulong ChannelId { get; }

	private VoiceChannelStatusUpdateAuditLogData(string status, ulong channelId)
	{
		Status = status;
		ChannelId = channelId;
	}

	internal static VoiceChannelStatusUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		return new VoiceChannelStatusUpdateAuditLogData(entry.Options.Status, entry.TargetId.Value);
	}
}
