using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class AutoModRuleDeletedAuditLogData : IAuditLogData
{
	public AutoModRuleInfo Data { get; }

	private AutoModRuleDeletedAuditLogData(AutoModRuleInfo data)
	{
		Data = data;
	}

	internal static AutoModRuleDeletedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		return new AutoModRuleDeletedAuditLogData(new AutoModRuleInfo(AuditLogHelper.CreateAuditLogEntityInfo<AutoModRuleInfoAuditLogModel>(entry.Changes, discord).Item1));
	}
}
