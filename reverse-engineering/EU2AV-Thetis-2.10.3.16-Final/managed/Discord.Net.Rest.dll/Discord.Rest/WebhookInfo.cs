using Discord.API.AuditLogs;

namespace Discord.Rest;

public struct WebhookInfo
{
	public string Name { get; }

	public ulong? ChannelId { get; }

	public string Avatar { get; }

	internal WebhookInfo(WebhookInfoAuditLogModel model)
	{
		Name = model.Name;
		ChannelId = model.ChannelId;
		Avatar = model.AvatarHash;
	}
}
