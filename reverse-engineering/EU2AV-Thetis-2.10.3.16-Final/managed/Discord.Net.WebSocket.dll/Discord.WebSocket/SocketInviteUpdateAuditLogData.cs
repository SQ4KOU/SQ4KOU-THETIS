using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketInviteUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketInviteInfo Before { get; }

	public SocketInviteInfo After { get; }

	private SocketInviteUpdateAuditLogData(SocketInviteInfo before, SocketInviteInfo after)
	{
		Before = before;
		After = after;
	}

	internal static SocketInviteUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<InviteInfoAuditLogModel>(entry.Changes, discord);
		return new SocketInviteUpdateAuditLogData(new SocketInviteInfo(model), new SocketInviteInfo(model2));
	}
}
