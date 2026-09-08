using Discord.API.AuditLogs;

namespace Discord.WebSocket;

public struct SocketWebhookInfo
{
	public string Name { get; }

	public ulong? ChannelId { get; }

	public string Avatar { get; }

	internal SocketWebhookInfo(WebhookInfoAuditLogModel model)
	{
		Name = model.Name;
		ChannelId = model.ChannelId;
		Avatar = model.AvatarHash;
	}
}
