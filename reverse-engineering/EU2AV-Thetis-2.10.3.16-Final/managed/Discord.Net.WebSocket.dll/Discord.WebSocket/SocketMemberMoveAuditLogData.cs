using Discord.API;

namespace Discord.WebSocket;

public class SocketMemberMoveAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ChannelId { get; }

	public int MemberCount { get; }

	private SocketMemberMoveAuditLogData(ulong channelId, int count)
	{
		ChannelId = channelId;
		MemberCount = count;
	}

	internal static SocketMemberMoveAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketMemberMoveAuditLogData(entry.Options.ChannelId.Value, entry.Options.Count.Value);
	}
}
