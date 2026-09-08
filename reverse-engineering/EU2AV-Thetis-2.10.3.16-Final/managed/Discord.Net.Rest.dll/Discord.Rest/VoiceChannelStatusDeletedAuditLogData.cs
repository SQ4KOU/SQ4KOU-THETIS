using Discord.API;

namespace Discord.Rest;

public class VoiceChannelStatusDeletedAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	private VoiceChannelStatusDeletedAuditLogData(ulong channelId)
	{
		ChannelId = channelId;
	}

	internal static VoiceChannelStatusDeletedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		return new VoiceChannelStatusDeletedAuditLogData(entry.TargetId.Value);
	}
}
