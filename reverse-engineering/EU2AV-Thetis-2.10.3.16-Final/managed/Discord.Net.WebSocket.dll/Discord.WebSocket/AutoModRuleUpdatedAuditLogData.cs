using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class AutoModRuleUpdatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketAutoModRuleInfo Before { get; }

	public SocketAutoModRuleInfo After { get; }

	private AutoModRuleUpdatedAuditLogData(SocketAutoModRuleInfo before, SocketAutoModRuleInfo after)
	{
		Before = before;
		After = after;
	}

	internal static AutoModRuleUpdatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<AutoModRuleInfoAuditLogModel>(entry.Changes, discord);
		return new AutoModRuleUpdatedAuditLogData(new SocketAutoModRuleInfo(model), new SocketAutoModRuleInfo(model2));
	}
}
