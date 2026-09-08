using Discord.API;

namespace Discord.Rest;

public class AutoModBlockedMessageAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; set; }

	public string AutoModRuleName { get; set; }

	public AutoModTriggerType AutoModRuleTriggerType { get; set; }

	internal AutoModBlockedMessageAuditLogData(ulong channelId, string autoModRuleName, AutoModTriggerType autoModRuleTriggerType)
	{
		ChannelId = channelId;
		AutoModRuleName = autoModRuleName;
		AutoModRuleTriggerType = autoModRuleTriggerType;
	}

	internal static AutoModBlockedMessageAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		return new AutoModBlockedMessageAuditLogData(entry.Options.ChannelId.Value, entry.Options.AutoModRuleName, entry.Options.AutoModRuleTriggerType.Value);
	}
}
