using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketRoleDeleteAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong RoleId { get; }

	public SocketRoleEditInfo Properties { get; }

	private SocketRoleDeleteAuditLogData(ulong id, SocketRoleEditInfo props)
	{
		RoleId = id;
		Properties = props;
	}

	internal static SocketRoleDeleteAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		RoleInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<RoleInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new SocketRoleDeleteAuditLogData(entry.TargetId.Value, new SocketRoleEditInfo(item));
	}
}
