using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class WebhookDeleteAuditLogData : IAuditLogData
{
	public ulong WebhookId { get; }

	public ulong ChannelId { get; }

	public WebhookType Type { get; }

	public string Name { get; }

	public string Avatar { get; }

	private WebhookDeleteAuditLogData(ulong id, WebhookInfoAuditLogModel model)
	{
		WebhookId = id;
		ChannelId = model.ChannelId.Value;
		Name = model.Name;
		Type = model.Type.Value;
		Avatar = model.AvatarHash;
	}

	internal static WebhookDeleteAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		WebhookInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<WebhookInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new WebhookDeleteAuditLogData(entry.TargetId.Value, item);
	}
}
