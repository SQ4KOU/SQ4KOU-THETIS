using System.Threading.Tasks;

namespace Discord;

public interface IUser : ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence
{
	string AvatarId { get; }

	string Discriminator { get; }

	ushort DiscriminatorValue { get; }

	bool IsBot { get; }

	bool IsWebhook { get; }

	string Username { get; }

	UserProperties? PublicFlags { get; }

	string GlobalName { get; }

	string AvatarDecorationHash { get; }

	ulong? AvatarDecorationSkuId { get; }

	PrimaryGuild? PrimaryGuild { get; }

	string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128);

	string GetDefaultAvatarUrl();

	string GetDisplayAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128);

	Task<IDMChannel> CreateDMChannelAsync(RequestOptions options = null);

	string GetAvatarDecorationUrl();
}
