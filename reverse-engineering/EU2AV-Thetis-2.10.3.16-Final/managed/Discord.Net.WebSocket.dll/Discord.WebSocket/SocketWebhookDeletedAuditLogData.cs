using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketWebhookDeletedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong WebhookId { get; }

	public ulong ChannelId { get; }

	public WebhookType Type { get; }

	public string Name { get; }

	public string Avatar { get; }

	private SocketWebhookDeletedAuditLogData(ulong id, WebhookInfoAuditLogModel model)
	{
		WebhookId = id;
		ChannelId = model.ChannelId.Value;
		Name = model.Name;
		Type = model.Type.Value;
		Avatar = model.AvatarHash;
	}

	internal static SocketWebhookDeletedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		WebhookInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<WebhookInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new SocketWebhookDeletedAuditLogData(entry.TargetId.Value, item);
	}
}
