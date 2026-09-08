using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord;

public interface IDiscordClient : IDisposable, IAsyncDisposable
{
	ConnectionState ConnectionState { get; }

	ISelfUser CurrentUser { get; }

	TokenType TokenType { get; }

	Task StartAsync();

	Task StopAsync();

	Task<IApplication> GetApplicationInfoAsync(RequestOptions options = null);

	Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null);

	Task<IApplicationCommand> GetGlobalApplicationCommandAsync(ulong id, RequestOptions options = null);

	Task<IReadOnlyCollection<IApplicationCommand>> GetGlobalApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null);

	Task<IApplicationCommand> CreateGlobalApplicationCommand(ApplicationCommandProperties properties, RequestOptions options = null);

	Task<IReadOnlyCollection<IApplicationCommand>> BulkOverwriteGlobalApplicationCommand(ApplicationCommandProperties[] properties, RequestOptions options = null);

	Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null);

	Task<IInvite> GetInviteAsync(string inviteId, RequestOptions options = null);

	Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null);

	Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null);

	Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null);

	Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null);

	Task<int> GetRecommendedShardCountAsync(RequestOptions options = null);

	Task<BotGateway> GetBotGatewayAsync(RequestOptions options = null);

	Task<IEntitlement> CreateTestEntitlementAsync(ulong skuId, ulong ownerId, SubscriptionOwnerType ownerType, RequestOptions options = null);

	Task DeleteTestEntitlementAsync(ulong entitlementId, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IEntitlement>> GetEntitlementsAsync(int limit = 100, ulong? afterId = null, ulong? beforeId = null, bool excludeEnded = false, ulong? guildId = null, ulong? userId = null, ulong[] skuIds = null, RequestOptions options = null, bool? excludeDeleted = null);

	Task<IReadOnlyCollection<SKU>> GetSKUsAsync(RequestOptions options = null);

	Task ConsumeEntitlementAsync(ulong entitlementId, RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<ISubscription>> GetSKUSubscriptionsAsync(ulong skuId, int limit = 100, ulong? afterId = null, ulong? beforeId = null, ulong? userId = null, RequestOptions options = null);

	Task<ISubscription> GetSKUSubscriptionAsync(ulong skuId, ulong subscriptionId, RequestOptions options = null);

	Task<Emote> GetApplicationEmoteAsync(ulong emoteId, RequestOptions options = null);

	Task<IReadOnlyCollection<Emote>> GetApplicationEmotesAsync(RequestOptions options = null);

	Task<Emote> ModifyApplicationEmoteAsync(ulong emoteId, Action<ApplicationEmoteProperties> args, RequestOptions options = null);

	Task<Emote> CreateApplicationEmoteAsync(string name, Image image, RequestOptions options = null);

	Task DeleteApplicationEmoteAsync(ulong emoteId, RequestOptions options = null);
}
