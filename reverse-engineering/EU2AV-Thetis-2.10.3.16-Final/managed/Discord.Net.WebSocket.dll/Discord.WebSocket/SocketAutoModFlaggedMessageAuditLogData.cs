using Discord.API;

namespace Discord.WebSocket;

public class SocketAutoModFlaggedMessageAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ChannelId { get; set; }

	public string AutoModRuleName { get; set; }

	public AutoModTriggerType AutoModRuleTriggerType { get; set; }

	internal SocketAutoModFlaggedMessageAuditLogData(ulong? channelId, string autoModRuleName, AutoModTriggerType autoModRuleTriggerType)
	{
		ChannelId = channelId.GetValueOrDefault();
		AutoModRuleName = autoModRuleName;
		AutoModRuleTriggerType = autoModRuleTriggerType;
	}

	internal static SocketAutoModFlaggedMessageAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketAutoModFlaggedMessageAuditLogData(entry.Options.ChannelId, entry.Options.AutoModRuleName, entry.Options.AutoModRuleTriggerType.Value);
	}
}
