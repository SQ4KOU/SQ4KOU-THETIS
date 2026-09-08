using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class WebhookUpdateAuditLogData : IAuditLogData
{
	public IWebhook Webhook { get; }

	public WebhookInfo Before { get; }

	public WebhookInfo After { get; }

	private WebhookUpdateAuditLogData(IWebhook webhook, WebhookInfo before, WebhookInfo after)
	{
		Webhook = webhook;
		Before = before;
		After = after;
	}

	internal static WebhookUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		(WebhookInfoAuditLogModel, WebhookInfoAuditLogModel) tuple = AuditLogHelper.CreateAuditLogEntityInfo<WebhookInfoAuditLogModel>(entry.Changes, discord);
		WebhookInfoAuditLogModel item = tuple.Item1;
		WebhookInfoAuditLogModel item2 = tuple.Item2;
		Webhook webhook = log.Webhooks?.FirstOrDefault((Webhook x) => x.Id == entry.TargetId);
		return new WebhookUpdateAuditLogData((webhook != null) ? RestWebhook.Create(discord, (IGuild)null, webhook) : null, new WebhookInfo(item), new WebhookInfo(item2));
	}
}
