using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class AutoModRuleCreatedAuditLogData : IAuditLogData
{
	public AutoModRuleInfo Data { get; }

	private AutoModRuleCreatedAuditLogData(AutoModRuleInfo data)
	{
		Data = data;
	}

	internal static AutoModRuleCreatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		return new AutoModRuleCreatedAuditLogData(new AutoModRuleInfo(AuditLogHelper.CreateAuditLogEntityInfo<AutoModRuleInfoAuditLogModel>(entry.Changes, discord).Item2));
	}
}
