using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class WebhookCreateAuditLogData : IAuditLogData
{
	public IWebhook Webhook { get; }

	public ulong WebhookId { get; }

	public WebhookType Type { get; }

	public string Name { get; }

	public ulong ChannelId { get; }

	public string Avatar { get; }

	private WebhookCreateAuditLogData(IWebhook webhook, ulong webhookId, WebhookInfoAuditLogModel model)
	{
		Webhook = webhook;
		WebhookId = webhookId;
		Name = model.Name;
		Type = model.Type.Value;
		ChannelId = model.ChannelId.Value;
		Avatar = model.AvatarHash;
	}

	internal static WebhookCreateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		WebhookInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<WebhookInfoAuditLogModel>(entry.Changes, discord).Item2;
		Webhook webhook = log.Webhooks?.FirstOrDefault((Webhook x) => x.Id == entry.TargetId);
		return new WebhookCreateAuditLogData((webhook == null) ? null : RestWebhook.Create(discord, (IGuild)null, webhook), entry.TargetId.Value, item);
	}
}
