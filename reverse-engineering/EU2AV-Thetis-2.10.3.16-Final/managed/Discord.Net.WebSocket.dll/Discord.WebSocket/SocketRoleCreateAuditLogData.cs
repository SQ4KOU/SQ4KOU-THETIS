using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketRoleCreateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong RoleId { get; }

	public SocketRoleEditInfo Properties { get; }

	private SocketRoleCreateAuditLogData(ulong id, SocketRoleEditInfo props)
	{
		RoleId = id;
		Properties = props;
	}

	internal static SocketRoleCreateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		RoleInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<RoleInfoAuditLogModel>(entry.Changes, discord).Item2;
		return new SocketRoleCreateAuditLogData(entry.TargetId.Value, new SocketRoleEditInfo(item));
	}
}
