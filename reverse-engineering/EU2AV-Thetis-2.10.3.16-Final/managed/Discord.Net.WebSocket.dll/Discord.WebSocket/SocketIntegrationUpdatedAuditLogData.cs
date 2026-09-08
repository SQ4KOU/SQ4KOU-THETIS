using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketIntegrationUpdatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketIntegrationInfo Before { get; }

	public SocketIntegrationInfo After { get; }

	internal SocketIntegrationUpdatedAuditLogData(SocketIntegrationInfo before, SocketIntegrationInfo after)
	{
		Before = before;
		After = after;
	}

	internal static SocketIntegrationUpdatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<IntegrationInfoAuditLogModel>(entry.Changes, discord);
		return new SocketIntegrationUpdatedAuditLogData(new SocketIntegrationInfo(model), new SocketIntegrationInfo(model2));
	}
}
