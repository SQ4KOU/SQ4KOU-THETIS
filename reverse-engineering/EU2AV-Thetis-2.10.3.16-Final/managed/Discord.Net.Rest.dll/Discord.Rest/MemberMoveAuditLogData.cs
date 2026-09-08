using Discord.API;

namespace Discord.Rest;

public class MemberMoveAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public int MemberCount { get; }

	private MemberMoveAuditLogData(ulong channelId, int count)
	{
		ChannelId = channelId;
		MemberCount = count;
	}

	internal static MemberMoveAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		return new MemberMoveAuditLogData(entry.Options.ChannelId.Value, entry.Options.Count.Value);
	}
}
