using System;
using System.Threading.Tasks;

namespace Discord;

public interface IWebhook : IDeletable, ISnowflakeEntity, IEntity<ulong>
{
	string Token { get; }

	string Name { get; }

	string AvatarId { get; }

	IIntegrationChannel Channel { get; }

	ulong? ChannelId { get; }

	IGuild Guild { get; }

	ulong? GuildId { get; }

	IUser Creator { get; }

	ulong? ApplicationId { get; }

	WebhookType Type { get; }

	string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128);

	Task ModifyAsync(Action<WebhookProperties> func, RequestOptions options = null);
}
