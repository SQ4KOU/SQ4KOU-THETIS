using Discord.API;

namespace Discord.WebSocket;

public class SocketAutoModBlockedMessageAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ChannelId { get; set; }

	public string AutoModRuleName { get; set; }

	public AutoModTriggerType AutoModRuleTriggerType { get; set; }

	internal SocketAutoModBlockedMessageAuditLogData(ulong channelId, string autoModRuleName, AutoModTriggerType autoModRuleTriggerType)
	{
		ChannelId = channelId;
		AutoModRuleName = autoModRuleName;
		AutoModRuleTriggerType = autoModRuleTriggerType;
	}

	internal static SocketAutoModBlockedMessageAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketAutoModBlockedMessageAuditLogData(entry.Options.ChannelId.Value, entry.Options.AutoModRuleName, entry.Options.AutoModRuleTriggerType.Value);
	}
}
