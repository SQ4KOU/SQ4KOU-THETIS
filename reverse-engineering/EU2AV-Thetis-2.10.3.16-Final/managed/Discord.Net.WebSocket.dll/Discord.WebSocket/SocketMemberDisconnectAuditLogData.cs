using Discord.API;

namespace Discord.WebSocket;

public class SocketMemberDisconnectAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public int MemberCount { get; }

	private SocketMemberDisconnectAuditLogData(int count)
	{
		MemberCount = count;
	}

	internal static SocketMemberDisconnectAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketMemberDisconnectAuditLogData(entry.Options.Count.Value);
	}
}
