using Discord.API;

namespace Discord.Rest;

public class AutoModFlaggedMessageAuditLogData : IAuditLogData
{
	public ulong? ChannelId { get; set; }

	public string AutoModRuleName { get; set; }

	public AutoModTriggerType AutoModRuleTriggerType { get; set; }

	internal AutoModFlaggedMessageAuditLogData(ulong? channelId, string autoModRuleName, AutoModTriggerType autoModRuleTriggerType)
	{
		ChannelId = channelId.GetValueOrDefault();
		AutoModRuleName = autoModRuleName;
		AutoModRuleTriggerType = autoModRuleTriggerType;
	}

	internal static AutoModFlaggedMessageAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		return new AutoModFlaggedMessageAuditLogData(entry.Options.ChannelId, entry.Options.AutoModRuleName, entry.Options.AutoModRuleTriggerType.Value);
	}
}
