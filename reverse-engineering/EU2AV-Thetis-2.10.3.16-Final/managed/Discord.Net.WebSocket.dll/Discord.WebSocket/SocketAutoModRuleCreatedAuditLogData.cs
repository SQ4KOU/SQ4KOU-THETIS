using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketAutoModRuleCreatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketAutoModRuleInfo Data { get; }

	private SocketAutoModRuleCreatedAuditLogData(SocketAutoModRuleInfo data)
	{
		Data = data;
	}

	internal static SocketAutoModRuleCreatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketAutoModRuleCreatedAuditLogData(new SocketAutoModRuleInfo(AuditLogHelper.CreateAuditLogEntityInfo<AutoModRuleInfoAuditLogModel>(entry.Changes, discord).Item2));
	}
}
