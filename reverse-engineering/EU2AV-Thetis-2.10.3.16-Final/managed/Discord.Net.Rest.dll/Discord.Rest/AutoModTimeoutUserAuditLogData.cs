using Discord.API;

namespace Discord.Rest;

public class AutoModTimeoutUserAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; set; }

	public string AutoModRuleName { get; set; }

	public AutoModTriggerType AutoModRuleTriggerType { get; set; }

	internal AutoModTimeoutUserAuditLogData(ulong channelId, string autoModRuleName, AutoModTriggerType autoModRuleTriggerType)
	{
		ChannelId = channelId;
		AutoModRuleName = autoModRuleName;
		AutoModRuleTriggerType = autoModRuleTriggerType;
	}

	internal static AutoModTimeoutUserAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		return new AutoModTimeoutUserAuditLogData(entry.Options.ChannelId.Value, entry.Options.AutoModRuleName, entry.Options.AutoModRuleTriggerType.Value);
	}
}
