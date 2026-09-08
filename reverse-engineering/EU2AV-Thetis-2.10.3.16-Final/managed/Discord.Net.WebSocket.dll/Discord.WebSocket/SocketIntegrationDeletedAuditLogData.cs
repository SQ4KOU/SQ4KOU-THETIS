using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketIntegrationDeletedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketIntegrationInfo Data { get; }

	internal SocketIntegrationDeletedAuditLogData(SocketIntegrationInfo info)
	{
		Data = info;
	}

	internal static SocketIntegrationDeletedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketIntegrationDeletedAuditLogData(new SocketIntegrationInfo(AuditLogHelper.CreateAuditLogEntityInfo<IntegrationInfoAuditLogModel>(entry.Changes, discord).Item1));
	}
}
