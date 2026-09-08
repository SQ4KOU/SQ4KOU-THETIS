using Discord.API;

namespace Discord.Rest;

public class PruneAuditLogData : IAuditLogData
{
	public int PruneDays { get; }

	public int MembersRemoved { get; }

	private PruneAuditLogData(int pruneDays, int membersRemoved)
	{
		PruneDays = pruneDays;
		MembersRemoved = membersRemoved;
	}

	internal static PruneAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		return new PruneAuditLogData(entry.Options.PruneDeleteMemberDays.Value, entry.Options.PruneMembersRemoved.Value);
	}
}
