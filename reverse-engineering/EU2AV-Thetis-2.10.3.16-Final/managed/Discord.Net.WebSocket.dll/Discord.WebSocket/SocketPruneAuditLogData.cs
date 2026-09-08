using Discord.API;

namespace Discord.WebSocket;

public class SocketPruneAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public int PruneDays { get; }

	public int MembersRemoved { get; }

	private SocketPruneAuditLogData(int pruneDays, int membersRemoved)
	{
		PruneDays = pruneDays;
		MembersRemoved = membersRemoved;
	}

	internal static SocketPruneAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketPruneAuditLogData(entry.Options.PruneDeleteMemberDays.Value, entry.Options.PruneMembersRemoved.Value);
	}
}
