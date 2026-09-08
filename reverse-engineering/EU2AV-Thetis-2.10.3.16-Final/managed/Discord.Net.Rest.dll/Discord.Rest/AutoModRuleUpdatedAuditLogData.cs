using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class AutoModRuleUpdatedAuditLogData : IAuditLogData
{
	public IAutoModRule Rule { get; }

	public AutoModRuleInfo Before { get; }

	public AutoModRuleInfo After { get; }

	private AutoModRuleUpdatedAuditLogData(AutoModRuleInfo before, AutoModRuleInfo after, IAutoModRule rule)
	{
		Before = before;
		After = after;
		Rule = rule;
	}

	internal static AutoModRuleUpdatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		(AutoModRuleInfoAuditLogModel, AutoModRuleInfoAuditLogModel) tuple = AuditLogHelper.CreateAuditLogEntityInfo<AutoModRuleInfoAuditLogModel>(entry.Changes, discord);
		AutoModRuleInfoAuditLogModel item = tuple.Item1;
		AutoModRuleInfoAuditLogModel item2 = tuple.Item2;
		RestAutoModRule rule = RestAutoModRule.Create(discord, log.AutoModerationRules.FirstOrDefault((AutoModerationRule x) => x.Id == entry.TargetId));
		return new AutoModRuleUpdatedAuditLogData(new AutoModRuleInfo(item), new AutoModRuleInfo(item2), rule);
	}
}
