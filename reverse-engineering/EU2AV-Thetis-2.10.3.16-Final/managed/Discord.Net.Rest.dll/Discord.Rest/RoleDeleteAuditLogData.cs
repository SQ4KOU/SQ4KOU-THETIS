using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class RoleDeleteAuditLogData : IAuditLogData
{
	public ulong RoleId { get; }

	public RoleEditInfo Properties { get; }

	private RoleDeleteAuditLogData(ulong id, RoleEditInfo props)
	{
		RoleId = id;
		Properties = props;
	}

	internal static RoleDeleteAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		RoleInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<RoleInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new RoleDeleteAuditLogData(entry.TargetId.Value, new RoleEditInfo(item));
	}
}
