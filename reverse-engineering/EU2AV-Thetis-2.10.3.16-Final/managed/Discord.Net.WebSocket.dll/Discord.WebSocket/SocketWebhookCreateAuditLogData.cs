using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketWebhookCreateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong WebhookId { get; }

	public WebhookType Type { get; }

	public string Name { get; }

	public ulong ChannelId { get; }

	public string Avatar { get; }

	private SocketWebhookCreateAuditLogData(ulong webhookId, WebhookInfoAuditLogModel model)
	{
		WebhookId = webhookId;
		Name = model.Name;
		Type = model.Type.Value;
		ChannelId = model.ChannelId.Value;
		Avatar = model.AvatarHash;
	}

	internal static SocketWebhookCreateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		WebhookInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<WebhookInfoAuditLogModel>(entry.Changes, discord).Item2;
		return new SocketWebhookCreateAuditLogData(entry.TargetId.Value, item);
	}
}
