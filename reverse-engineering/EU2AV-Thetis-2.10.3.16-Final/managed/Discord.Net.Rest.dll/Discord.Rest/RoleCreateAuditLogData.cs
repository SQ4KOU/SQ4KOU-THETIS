using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class RoleCreateAuditLogData : IAuditLogData
{
	public ulong RoleId { get; }

	public RoleEditInfo Properties { get; }

	private RoleCreateAuditLogData(ulong id, RoleEditInfo props)
	{
		RoleId = id;
		Properties = props;
	}

	internal static RoleCreateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		RoleInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<RoleInfoAuditLogModel>(entry.Changes, discord).Item2;
		return new RoleCreateAuditLogData(entry.TargetId.Value, new RoleEditInfo(item));
	}
}
