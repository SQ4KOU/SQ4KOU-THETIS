using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketIntegrationCreatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketIntegrationInfo Data { get; }

	internal SocketIntegrationCreatedAuditLogData(SocketIntegrationInfo info)
	{
		Data = info;
	}

	internal static SocketIntegrationCreatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketIntegrationCreatedAuditLogData(new SocketIntegrationInfo(AuditLogHelper.CreateAuditLogEntityInfo<IntegrationInfoAuditLogModel>(entry.Changes, discord).Item2));
	}
}
