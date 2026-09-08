using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketRoleUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong RoleId { get; }

	public SocketRoleEditInfo Before { get; }

	public SocketRoleEditInfo After { get; }

	private SocketRoleUpdateAuditLogData(ulong id, SocketRoleEditInfo oldProps, SocketRoleEditInfo newProps)
	{
		RoleId = id;
		Before = oldProps;
		After = newProps;
	}

	internal static SocketRoleUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<RoleInfoAuditLogModel>(entry.Changes, discord);
		return new SocketRoleUpdateAuditLogData(entry.TargetId.Value, new SocketRoleEditInfo(model), new SocketRoleEditInfo(model2));
	}
}
