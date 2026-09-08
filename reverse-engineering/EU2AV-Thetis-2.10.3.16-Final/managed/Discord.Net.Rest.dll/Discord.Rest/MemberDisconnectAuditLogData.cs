using Discord.API;

namespace Discord.Rest;

public class MemberDisconnectAuditLogData : IAuditLogData
{
	public int MemberCount { get; }

	private MemberDisconnectAuditLogData(int count)
	{
		MemberCount = count;
	}

	internal static MemberDisconnectAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		return new MemberDisconnectAuditLogData(entry.Options.Count.Value);
	}
}
