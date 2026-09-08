using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class RoleUpdateAuditLogData : IAuditLogData
{
	public ulong RoleId { get; }

	public RoleEditInfo Before { get; }

	public RoleEditInfo After { get; }

	private RoleUpdateAuditLogData(ulong id, RoleEditInfo oldProps, RoleEditInfo newProps)
	{
		RoleId = id;
		Before = oldProps;
		After = newProps;
	}

	internal static RoleUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<RoleInfoAuditLogModel>(entry.Changes, discord);
		return new RoleUpdateAuditLogData(entry.TargetId.Value, new RoleEditInfo(model), new RoleEditInfo(model2));
	}
}
