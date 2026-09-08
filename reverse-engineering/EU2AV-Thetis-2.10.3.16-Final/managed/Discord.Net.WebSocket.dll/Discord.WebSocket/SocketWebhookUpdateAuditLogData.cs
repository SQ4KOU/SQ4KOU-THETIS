using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketWebhookUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketWebhookInfo Before { get; }

	public SocketWebhookInfo After { get; }

	private SocketWebhookUpdateAuditLogData(SocketWebhookInfo before, SocketWebhookInfo after)
	{
		Before = before;
		After = after;
	}

	internal static SocketWebhookUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<WebhookInfoAuditLogModel>(entry.Changes, discord);
		return new SocketWebhookUpdateAuditLogData(new SocketWebhookInfo(model), new SocketWebhookInfo(model2));
	}
}
