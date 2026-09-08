using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketAutoModRuleDeletedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketAutoModRuleInfo Data { get; }

	private SocketAutoModRuleDeletedAuditLogData(SocketAutoModRuleInfo data)
	{
		Data = data;
	}

	internal static SocketAutoModRuleDeletedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketAutoModRuleDeletedAuditLogData(new SocketAutoModRuleInfo(AuditLogHelper.CreateAuditLogEntityInfo<AutoModRuleInfoAuditLogModel>(entry.Changes, discord).Item1));
	}
}
