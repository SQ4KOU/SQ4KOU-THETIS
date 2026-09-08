using Discord.API;

namespace Discord.WebSocket;

public class SocketAutoModTimeoutUserAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ChannelId { get; set; }

	public string AutoModRuleName { get; set; }

	public AutoModTriggerType AutoModRuleTriggerType { get; set; }

	internal SocketAutoModTimeoutUserAuditLogData(ulong channelId, string autoModRuleName, AutoModTriggerType autoModRuleTriggerType)
	{
		ChannelId = channelId;
		AutoModRuleName = autoModRuleName;
		AutoModRuleTriggerType = autoModRuleTriggerType;
	}

	internal static SocketAutoModTimeoutUserAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketAutoModTimeoutUserAuditLogData(entry.Options.ChannelId.Value, entry.Options.AutoModRuleName, entry.Options.AutoModRuleTriggerType.Value);
	}
}
