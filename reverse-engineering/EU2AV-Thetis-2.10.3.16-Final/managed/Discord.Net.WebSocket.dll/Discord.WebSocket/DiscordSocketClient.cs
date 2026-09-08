using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Gateway;
using Discord.Logging;
using Discord.Net.Converters;
using Discord.Net.Udp;
using Discord.Net.WebSockets;
using Discord.Rest;
using Discord.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Discord.WebSocket;

public class DiscordSocketClient : BaseSocketClient, IDiscordClient, IDisposable, IAsyncDisposable
{
	private readonly ConcurrentQueue<ulong> _largeGuilds;

	internal readonly JsonSerializer _serializer;

	private readonly DiscordShardedClient _shardedClient;

	private readonly DiscordSocketClient _parentClient;

	private readonly ConcurrentQueue<long> _heartbeatTimes;

	private readonly ConnectionManager _connection;

	private readonly Logger _gatewayLogger;

	private readonly SemaphoreSlim _stateLock;

	private string _sessionId;

	private int _lastSeq;

	private ImmutableDictionary<string, RestVoiceRegion> _voiceRegions;

	private Task _heartbeatTask;

	private Task _guildDownloadTask;

	private int _unavailableGuildCount;

	private long _lastGuildAvailableTime;

	private long _lastMessageTime;

	private int _nextAudioId;

	private DateTimeOffset? _statusSince;

	private RestApplication _applicationInfo;

	private bool _isDisposed;

	private GatewayIntents _gatewayIntents;

	private ImmutableArray<StickerPack<SocketSticker>> _defaultStickers;

	private SocketSelfUser _previousSessionUser;

	private UserStatus? _status;

	private Optional<IActivity> _activity;

	private readonly AsyncEvent<Func<Task>> _connectedEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<Exception, Task>> _disconnectedEvent = new AsyncEvent<Func<Exception, Task>>();

	private readonly AsyncEvent<Func<Task>> _readyEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<int, int, Task>> _latencyUpdatedEvent = new AsyncEvent<Func<int, int, Task>>();

	public override DiscordSocketRestClient Rest { get; }

	public int ShardId { get; }

	public override ConnectionState ConnectionState => _connection.State;

	public override int Latency { get; protected set; }

	public override UserStatus Status
	{
		get
		{
			return _status ?? UserStatus.Online;
		}
		protected set
		{
			_status = value;
		}
	}

	public override IActivity Activity
	{
		get
		{
			return _activity.GetValueOrDefault();
		}
		protected set
		{
			_activity = Optional.Create(value);
		}
	}

	internal int TotalShards { get; private set; }

	internal int MessageCacheSize { get; private set; }

	internal int LargeThreshold { get; private set; }

	internal ClientState State { get; private set; }

	internal UdpSocketProvider UdpSocketProvider { get; private set; }

	internal WebSocketProvider WebSocketProvider { get; private set; }

	internal bool AlwaysDownloadUsers { get; private set; }

	internal int? HandlerTimeout { get; private set; }

	internal bool AlwaysDownloadDefaultStickers { get; private set; }

	internal bool AlwaysResolveStickers { get; private set; }

	internal bool LogGatewayIntentWarnings { get; private set; }

	internal bool SuppressUnknownDispatchWarnings { get; private set; }

	internal bool IncludeRawPayloadOnGatewayErrors { get; private set; }

	internal int AuditLogCacheSize { get; private set; }

	internal new DiscordSocketApiClient ApiClient => base.ApiClient;

	public override IReadOnlyCollection<SocketGuild> Guilds => State.Guilds;

	public override IReadOnlyCollection<StickerPack<SocketSticker>> DefaultStickerPacks
	{
		get
		{
			if (_shardedClient != null)
			{
				return _shardedClient.DefaultStickerPacks;
			}
			return _defaultStickers.ToReadOnlyCollection();
		}
	}

	public override IReadOnlyCollection<ISocketPrivateChannel> PrivateChannels => State.PrivateChannels;

	public IReadOnlyCollection<SocketDMChannel> DMChannels => State.PrivateChannels.OfType<SocketDMChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketGroupChannel> GroupChannels => State.PrivateChannels.OfType<SocketGroupChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketEntitlement> Entitlements => State.Entitlements;

	public IReadOnlyCollection<SocketSubscription> Subscription => State.Subscriptions;

	public event Func<Task> Connected
	{
		add
		{
			_connectedEvent.Add(value);
		}
		remove
		{
			_connectedEvent.Remove(value);
		}
	}

	public event Func<Exception, Task> Disconnected
	{
		add
		{
			_disconnectedEvent.Add(value);
		}
		remove
		{
			_disconnectedEvent.Remove(value);
		}
	}

	public event Func<Task> Ready
	{
		add
		{
			_readyEvent.Add(value);
		}
		remove
		{
			_readyEvent.Remove(value);
		}
	}

	public event Func<int, int, Task> LatencyUpdated
	{
		add
		{
			_latencyUpdatedEvent.Add(value);
		}
		remove
		{
			_latencyUpdatedEvent.Remove(value);
		}
	}

	public DiscordSocketClient()
		: this(new DiscordSocketConfig())
	{
	}

	public DiscordSocketClient(DiscordSocketConfig config)
		: this(config, CreateApiClient(config), null, null)
	{
	}

	internal DiscordSocketClient(DiscordSocketConfig config, DiscordShardedClient shardedClient, DiscordSocketClient parentClient)
		: this(config, CreateApiClient(config), shardedClient, parentClient)
	{
	}

	private DiscordSocketClient(DiscordSocketConfig config, DiscordSocketApiClient client, DiscordShardedClient shardedClient, DiscordSocketClient parentClient)
		: base(config, client)
	{
		ShardId = config.ShardId.GetValueOrDefault();
		TotalShards = config.TotalShards ?? 1;
		MessageCacheSize = config.MessageCacheSize;
		LargeThreshold = config.LargeThreshold;
		UdpSocketProvider = config.UdpSocketProvider;
		WebSocketProvider = config.WebSocketProvider;
		AlwaysDownloadUsers = config.AlwaysDownloadUsers;
		AlwaysDownloadDefaultStickers = config.AlwaysDownloadDefaultStickers;
		AlwaysResolveStickers = config.AlwaysResolveStickers;
		LogGatewayIntentWarnings = config.LogGatewayIntentWarnings;
		SuppressUnknownDispatchWarnings = config.SuppressUnknownDispatchWarnings;
		IncludeRawPayloadOnGatewayErrors = config.IncludeRawPayloadOnGatewayErrors;
		HandlerTimeout = config.HandlerTimeout;
		State = new ClientState(0, 0);
		Rest = new DiscordSocketRestClient(config, ApiClient);
		_heartbeatTimes = new ConcurrentQueue<long>();
		_gatewayIntents = config.GatewayIntents;
		_defaultStickers = ImmutableArray.Create<StickerPack<SocketSticker>>();
		_stateLock = new SemaphoreSlim(1, 1);
		_gatewayLogger = base.LogManager.CreateLogger((ShardId == 0 && TotalShards == 1) ? "Gateway" : $"Shard #{ShardId}");
		_connection = new ConnectionManager(_stateLock, _gatewayLogger, config.ConnectionTimeout, OnConnectingAsync, OnDisconnectingAsync, delegate(Func<Exception, Task> x)
		{
			ApiClient.Disconnected += x;
		});
		_connection.Connected += () => TimedInvokeAsync(_connectedEvent, "Connected");
		_connection.Disconnected += (Exception ex, bool recon) => TimedInvokeAsync(_disconnectedEvent, "Disconnected", ex);
		_nextAudioId = 1;
		_shardedClient = shardedClient;
		_parentClient = parentClient;
		_serializer = new JsonSerializer
		{
			ContractResolver = new DiscordContractResolver()
		};
		_serializer.Error += delegate(object s, Newtonsoft.Json.Serialization.ErrorEventArgs e)
		{
			_gatewayLogger.WarningAsync("Serializer Error", e.ErrorContext.Error).GetAwaiter().GetResult();
			e.ErrorContext.Handled = true;
		};
		ApiClient.SentGatewayMessage += async delegate(GatewayOpCode opCode)
		{
			await _gatewayLogger.DebugAsync($"Sent {opCode}").ConfigureAwait(continueOnCapturedContext: false);
		};
		ApiClient.ReceivedGatewayEvent += ProcessMessageAsync;
		base.LeftGuild += async delegate(SocketGuild g)
		{
			await _gatewayLogger.InfoAsync("Left " + g.Name).ConfigureAwait(continueOnCapturedContext: false);
		};
		base.JoinedGuild += async delegate(SocketGuild g)
		{
			await _gatewayLogger.InfoAsync("Joined " + g.Name).ConfigureAwait(continueOnCapturedContext: false);
		};
		base.GuildAvailable += async delegate(SocketGuild g)
		{
			await _gatewayLogger.VerboseAsync("Connected to " + g.Name).ConfigureAwait(continueOnCapturedContext: false);
		};
		base.GuildUnavailable += async delegate(SocketGuild g)
		{
			await _gatewayLogger.VerboseAsync("Disconnected from " + g.Name).ConfigureAwait(continueOnCapturedContext: false);
		};
		LatencyUpdated += async delegate(int old, int val)
		{
			await _gatewayLogger.DebugAsync($"Latency = {val} ms").ConfigureAwait(continueOnCapturedContext: false);
		};
		base.GuildAvailable += delegate(SocketGuild g)
		{
			Task guildDownloadTask = _guildDownloadTask;
			if (guildDownloadTask != null && guildDownloadTask.IsCompleted && ConnectionState == ConnectionState.Connected && AlwaysDownloadUsers && !g.HasAllMembers)
			{
				g.DownloadUsersAsync();
			}
			return Task.CompletedTask;
		};
		_largeGuilds = new ConcurrentQueue<ulong>();
		AuditLogCacheSize = config.AuditLogCacheSize;
	}

	private static DiscordSocketApiClient CreateApiClient(DiscordSocketConfig config)
	{
		return new DiscordSocketApiClient(config.RestClientProvider, config.WebSocketProvider, DiscordConfig.UserAgent, config.GatewayHost, RetryMode.AlwaysRetry, null, config.UseSystemClock, config.DefaultRatelimitCallback);
	}

	internal override void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				StopAsync().GetAwaiter().GetResult();
				ApiClient?.Dispose();
				_stateLock?.Dispose();
			}
			_isDisposed = true;
		}
		base.Dispose(disposing);
	}

	internal override async ValueTask DisposeAsync(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				await StopAsync().ConfigureAwait(continueOnCapturedContext: false);
				if (ApiClient != null)
				{
					await ApiClient.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				_stateLock?.Dispose();
			}
			_isDisposed = true;
		}
		await base.DisposeAsync(disposing).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal override async Task OnLoginAsync(TokenType tokenType, string token)
	{
		if (_shardedClient != null || _defaultStickers.Length != 0 || !AlwaysDownloadDefaultStickers)
		{
			return;
		}
		NitroStickerPacks obj = await ApiClient.ListNitroStickerPacksAsync().ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<StickerPack<SocketSticker>>.Builder builder = ImmutableArray.CreateBuilder<StickerPack<SocketSticker>>();
		foreach (StickerPack stickerPack in obj.StickerPacks)
		{
			IEnumerable<SocketSticker> stickers = stickerPack.Stickers.Select((Discord.API.Sticker x) => SocketSticker.Create(this, x));
			StickerPack<SocketSticker> item = new StickerPack<SocketSticker>(stickerPack.Name, stickerPack.Id, stickerPack.SkuId, stickerPack.CoverStickerId.ToNullable(), stickerPack.Description, stickerPack.BannerAssetId, stickers);
			builder.Add(item);
		}
		_defaultStickers = builder.ToImmutable();
	}

	internal override async Task OnLogoutAsync()
	{
		await StopAsync().ConfigureAwait(continueOnCapturedContext: false);
		_applicationInfo = null;
		_voiceRegions = null;
		await Rest.OnLogoutAsync();
	}

	public override Task StartAsync()
	{
		return _connection.StartAsync();
	}

	public override Task StopAsync()
	{
		return _connection.StopAsync();
	}

	private async Task OnConnectingAsync()
	{
		bool locked = false;
		if (_shardedClient != null && _sessionId == null)
		{
			await _shardedClient.AcquireIdentifyLockAsync(ShardId, _connection.CancelToken).ConfigureAwait(continueOnCapturedContext: false);
			locked = true;
		}
		try
		{
			await _gatewayLogger.DebugAsync("Connecting ApiClient").ConfigureAwait(continueOnCapturedContext: false);
			await ApiClient.ConnectAsync().ConfigureAwait(continueOnCapturedContext: false);
			if (_sessionId == null)
			{
				await _gatewayLogger.DebugAsync("Identifying").ConfigureAwait(continueOnCapturedContext: false);
				await ApiClient.SendIdentifyAsync(100, ShardId, TotalShards, _gatewayIntents, BuildCurrentStatus()).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await _gatewayLogger.DebugAsync("Resuming").ConfigureAwait(continueOnCapturedContext: false);
				await ApiClient.SendResumeAsync(_sessionId, _lastSeq).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			if (locked)
			{
				_shardedClient.ReleaseIdentifyLock();
			}
		}
		await _connection.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (LogGatewayIntentWarnings)
		{
			await LogGatewayIntentsWarning().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task OnDisconnectingAsync(Exception ex)
	{
		await _gatewayLogger.DebugAsync("Disconnecting ApiClient").ConfigureAwait(continueOnCapturedContext: false);
		await ApiClient.DisconnectAsync(ex).ConfigureAwait(continueOnCapturedContext: false);
		await _gatewayLogger.DebugAsync("Waiting for heartbeater").ConfigureAwait(continueOnCapturedContext: false);
		Task heartbeatTask = _heartbeatTask;
		if (heartbeatTask != null)
		{
			await heartbeatTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		_heartbeatTask = null;
		long result;
		while (_heartbeatTimes.TryDequeue(out result))
		{
		}
		_lastMessageTime = 0L;
		await _gatewayLogger.DebugAsync("Waiting for guild downloader").ConfigureAwait(continueOnCapturedContext: false);
		Task guildDownloadTask = _guildDownloadTask;
		if (guildDownloadTask != null)
		{
			await guildDownloadTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		_guildDownloadTask = null;
		await _gatewayLogger.DebugAsync("Clearing large guild queue").ConfigureAwait(continueOnCapturedContext: false);
		ulong result2;
		while (_largeGuilds.TryDequeue(out result2))
		{
		}
		await _gatewayLogger.DebugAsync("Raising virtual GuildUnavailables").ConfigureAwait(continueOnCapturedContext: false);
		foreach (SocketGuild guild in State.Guilds)
		{
			if (guild.IsAvailable)
			{
				await GuildUnavailableAsync(guild).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public override async Task<RestApplication> GetApplicationInfoAsync(RequestOptions options = null)
	{
		RestApplication restApplication = _applicationInfo;
		if (restApplication == null)
		{
			restApplication = (_applicationInfo = await ClientHelper.GetApplicationInfoAsync(this, options ?? RequestOptions.Default).ConfigureAwait(continueOnCapturedContext: false));
		}
		return restApplication;
	}

	public override SocketGuild GetGuild(ulong id)
	{
		return State.GetGuild(id);
	}

	public override SocketChannel GetChannel(ulong id)
	{
		return State.GetChannel(id);
	}

	public async ValueTask<IChannel> GetChannelAsync(ulong id, RequestOptions options = null)
	{
		IChannel channel = GetChannel(id);
		if (channel == null)
		{
			channel = await ClientHelper.GetChannelAsync(this, id, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return channel;
	}

	public async ValueTask<IUser> GetUserAsync(ulong id, RequestOptions options = null)
	{
		return await ((IDiscordClient)this).GetUserAsync(id, CacheMode.AllowDownload, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public void PurgeChannelCache()
	{
		State.PurgeAllChannels();
	}

	public void PurgeDMChannelCache()
	{
		RemoveDMChannels();
	}

	public override SocketUser GetUser(ulong id)
	{
		return State.GetUser(id);
	}

	public override SocketUser GetUser(string username, string discriminator = null)
	{
		return State.Users.FirstOrDefault((SocketGlobalUser x) => (discriminator == null || x.Discriminator == discriminator) && x.Username == username);
	}

	public Task<RestEntitlement> CreateTestEntitlementAsync(ulong skuId, ulong ownerId, SubscriptionOwnerType ownerType, RequestOptions options = null)
	{
		return ClientHelper.CreateTestEntitlementAsync(this, skuId, ownerId, ownerType, options);
	}

	public Task DeleteTestEntitlementAsync(ulong entitlementId, RequestOptions options = null)
	{
		return ApiClient.DeleteEntitlementAsync(entitlementId, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<IEntitlement>> GetEntitlementsAsync(int? limit = 100, ulong? afterId = null, ulong? beforeId = null, bool excludeEnded = false, ulong? guildId = null, ulong? userId = null, ulong[] skuIds = null, RequestOptions options = null, bool? excludeDeleted = null)
	{
		return ClientHelper.ListEntitlementsAsync(this, limit, afterId, beforeId, excludeEnded, guildId, userId, skuIds, excludeDeleted, options);
	}

	public Task<IReadOnlyCollection<SKU>> GetSKUsAsync(RequestOptions options = null)
	{
		return ClientHelper.ListSKUsAsync(this, options);
	}

	public Task ConsumeEntitlementAsync(ulong entitlementId, RequestOptions options = null)
	{
		return ClientHelper.ConsumeEntitlementAsync(this, entitlementId, options);
	}

	public Task<RestSubscription> GetSKUSubscriptionAsync(ulong skuId, ulong subscriptionId, RequestOptions options = null)
	{
		return ClientHelper.GetSKUSubscriptionAsync(this, skuId, subscriptionId, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestSubscription>> GetSKUSubscriptionsAsync(ulong skuId, int limit = 100, ulong? afterId = null, ulong? beforeId = null, ulong? userId = null, RequestOptions options = null)
	{
		return ClientHelper.ListSubscriptionsAsync(this, skuId, limit, afterId, beforeId, userId, options);
	}

	public Task<Emote> GetApplicationEmoteAsync(ulong emoteId, RequestOptions options = null)
	{
		return ClientHelper.GetApplicationEmojiAsync(this, emoteId, options);
	}

	public Task<IReadOnlyCollection<Emote>> GetApplicationEmotesAsync(RequestOptions options = null)
	{
		return ClientHelper.GetApplicationEmojisAsync(this, options);
	}

	public Task<Emote> ModifyApplicationEmoteAsync(ulong emoteId, Action<ApplicationEmoteProperties> args, RequestOptions options = null)
	{
		return ClientHelper.ModifyApplicationEmojiAsync(this, emoteId, args, options);
	}

	public Task<Emote> CreateApplicationEmoteAsync(string name, Image image, RequestOptions options = null)
	{
		return ClientHelper.CreateApplicationEmojiAsync(this, name, image, options);
	}

	public Task DeleteApplicationEmoteAsync(ulong emoteId, RequestOptions options = null)
	{
		return ClientHelper.DeleteApplicationEmojiAsync(this, emoteId, options);
	}

	public SocketEntitlement GetEntitlement(ulong id)
	{
		return State.GetEntitlement(id);
	}

	public SocketSubscription GetSubscription(ulong id)
	{
		return State.GetSubscription(id);
	}

	public async ValueTask<SocketApplicationCommand> GetGlobalApplicationCommandAsync(ulong id, RequestOptions options = null)
	{
		SocketApplicationCommand command = State.GetCommand(id);
		if (command != null)
		{
			return command;
		}
		ApplicationCommand applicationCommand = await ApiClient.GetGlobalApplicationCommandAsync(id, options);
		if (applicationCommand == null)
		{
			return null;
		}
		command = SocketApplicationCommand.Create(this, applicationCommand);
		State.AddCommand(command);
		return command;
	}

	public async Task<IReadOnlyCollection<SocketApplicationCommand>> GetGlobalApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		IEnumerable<SocketApplicationCommand> enumerable = (await ApiClient.GetGlobalApplicationCommandsAsync(withLocalizations, locale, options)).Select((ApplicationCommand x) => SocketApplicationCommand.Create(this, x));
		foreach (SocketApplicationCommand item in enumerable)
		{
			State.AddCommand(item);
		}
		return enumerable.ToImmutableArray();
	}

	public async Task<SocketApplicationCommand> CreateGlobalApplicationCommandAsync(ApplicationCommandProperties properties, RequestOptions options = null)
	{
		ApplicationCommand model = await InteractionHelper.CreateGlobalCommandAsync(this, properties, options).ConfigureAwait(continueOnCapturedContext: false);
		SocketApplicationCommand orAddCommand = State.GetOrAddCommand(model.Id, (ulong id) => SocketApplicationCommand.Create(this, model));
		orAddCommand.Update(model);
		return orAddCommand;
	}

	public async Task<IReadOnlyCollection<SocketApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ApplicationCommandProperties[] properties, RequestOptions options = null)
	{
		IEnumerable<SocketApplicationCommand> enumerable = (await InteractionHelper.BulkOverwriteGlobalCommandsAsync(this, properties, options)).Select((ApplicationCommand x) => SocketApplicationCommand.Create(this, x));
		State.PurgeCommands((SocketApplicationCommand x) => x.IsGlobalCommand);
		foreach (SocketApplicationCommand item in enumerable)
		{
			State.AddCommand(item);
		}
		return enumerable.ToImmutableArray();
	}

	public void PurgeUserCache()
	{
		State.PurgeUsers();
	}

	internal SocketGlobalUser GetOrCreateUser(ClientState state, User model)
	{
		return state.GetOrAddUser(model.Id, (ulong x) => SocketGlobalUser.Create(this, state, model));
	}

	internal SocketUser GetOrCreateTemporaryUser(ClientState state, User model)
	{
		return (SocketUser)(((object)state.GetUser(model.Id)) ?? ((object)SocketUnknownUser.Create(this, state, model)));
	}

	internal SocketGlobalUser GetOrCreateSelfUser(ClientState state, User model)
	{
		return state.GetOrAddUser(model.Id, delegate
		{
			SocketGlobalUser socketGlobalUser = SocketGlobalUser.Create(this, state, model);
			socketGlobalUser.GlobalUser.AddRef();
			socketGlobalUser.Presence = new SocketPresence(UserStatus.Online, null, null);
			return socketGlobalUser;
		});
	}

	internal void RemoveUser(ulong id)
	{
		State.RemoveUser(id);
	}

	public override async Task<SocketSticker> GetStickerAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
	{
		SocketSticker socketSticker = _defaultStickers.FirstOrDefault((StickerPack<SocketSticker> x) => x.Stickers.Any((SocketSticker y) => y.Id == id))?.Stickers.FirstOrDefault((SocketSticker x) => x.Id == id);
		if (socketSticker != null)
		{
			return socketSticker;
		}
		foreach (SocketGuild guild2 in Guilds)
		{
			socketSticker = await guild2.GetStickerAsync(id, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
			if (socketSticker != null)
			{
				return socketSticker;
			}
		}
		if (mode == CacheMode.CacheOnly)
		{
			return null;
		}
		Discord.API.Sticker sticker = await ApiClient.GetStickerAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (sticker == null)
		{
			return null;
		}
		if (sticker.GuildId.IsSpecified)
		{
			SocketGuild guild = State.GetGuild(sticker.GuildId.Value);
			return (guild == null) ? SocketSticker.Create(this, sticker) : guild.AddOrUpdateSticker(sticker);
		}
		return SocketSticker.Create(this, sticker);
	}

	public SocketSticker GetSticker(ulong id)
	{
		return GetStickerAsync(id, CacheMode.CacheOnly).GetAwaiter().GetResult();
	}

	public override async ValueTask<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
	{
		if (_parentClient == null)
		{
			if (_voiceRegions == null)
			{
				options = RequestOptions.CreateOrClone(options);
				options.IgnoreState = true;
				_voiceRegions = (await ApiClient.GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((VoiceRegion x) => RestVoiceRegion.Create(this, x)).ToImmutableDictionary((RestVoiceRegion x) => x.Id);
			}
			return _voiceRegions.ToReadOnlyCollection();
		}
		return await _parentClient.GetVoiceRegionsAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async ValueTask<RestVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
	{
		if (_parentClient == null)
		{
			if (_voiceRegions == null)
			{
				await GetVoiceRegionsAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			if (_voiceRegions.TryGetValue(id, out var value))
			{
				return value;
			}
			return null;
		}
		return await _parentClient.GetVoiceRegionAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task DownloadUsersAsync(IEnumerable<IGuild> guilds)
	{
		if (ConnectionState == ConnectionState.Connected)
		{
			EnsureGatewayIntent(GatewayIntents.GuildMembers);
			return ProcessUserDownloadsAsync(from x in guilds
				select GetGuild(x.Id) into x
				where x != null
				select x);
		}
		return Task.CompletedTask;
	}

	private async Task ProcessUserDownloadsAsync(IEnumerable<SocketGuild> guilds)
	{
		ImmutableArray<SocketGuild> cachedGuilds = guilds.ToImmutableArray();
		ulong[] batchIds = new ulong[Math.Min(1, cachedGuilds.Length)];
		Task[] batchTasks = new Task[batchIds.Length];
		int batchCount = cachedGuilds.Length / 1;
		int i = 0;
		int k = 0;
		for (; i < batchCount; i++)
		{
			bool isLast = i == batchCount - 1;
			int count = ((!isLast) ? 1 : (cachedGuilds.Length - (batchCount - 1)));
			int num = 0;
			while (num < count)
			{
				SocketGuild socketGuild = cachedGuilds[k];
				batchIds[num] = socketGuild.Id;
				batchTasks[num] = socketGuild.DownloaderPromise;
				num++;
				k++;
			}
			await ApiClient.SendRequestMembersAsync(batchIds).ConfigureAwait(continueOnCapturedContext: false);
			if (!isLast || batchCount <= 1)
			{
				await Task.WhenAll(batchTasks).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await Task.WhenAll(batchTasks.Take(count)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public override Task SetStatusAsync(UserStatus status)
	{
		Status = status;
		if (status == UserStatus.AFK)
		{
			_statusSince = DateTimeOffset.UtcNow;
		}
		else
		{
			_statusSince = null;
		}
		return SendStatusAsync();
	}

	public override Task SetGameAsync(string name, string streamUrl = null, ActivityType type = ActivityType.Playing)
	{
		if (!string.IsNullOrEmpty(streamUrl))
		{
			Activity = new StreamingGame(name, streamUrl);
		}
		else if (!string.IsNullOrEmpty(name))
		{
			if (type == ActivityType.CustomStatus)
			{
				Activity = new CustomStatusGame(name);
			}
			else
			{
				Activity = new Game(name, type);
			}
		}
		else
		{
			Activity = null;
		}
		return SendStatusAsync();
	}

	public override Task SetActivityAsync(IActivity activity)
	{
		Activity = activity;
		return SendStatusAsync();
	}

	public override Task SetCustomStatusAsync(string status)
	{
		CustomStatusGame activityAsync = new CustomStatusGame(status);
		return SetActivityAsync(activityAsync);
	}

	private Task SendStatusAsync()
	{
		if (CurrentUser == null)
		{
			return Task.CompletedTask;
		}
		ImmutableList<IActivity> activities = (_activity.IsSpecified ? ImmutableList.Create(_activity.Value) : null);
		CurrentUser.Presence = new SocketPresence(Status, null, activities);
		(UserStatus, bool, long?, Discord.API.Game) tuple = BuildCurrentStatus() ?? (UserStatus.Online, false, null, null);
		return ApiClient.SendPresenceUpdateAsync(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
	}

	private (UserStatus, bool, long?, Discord.API.Game)? BuildCurrentStatus()
	{
		UserStatus? status = _status;
		DateTimeOffset? statusSince = _statusSince;
		Optional<IActivity> activity = _activity;
		if (!status.HasValue && !activity.IsSpecified)
		{
			return null;
		}
		Discord.API.Game item = null;
		if (activity.GetValueOrDefault() != null)
		{
			Discord.API.Game game = new Discord.API.Game();
			if (activity.Value is RichGame)
			{
				throw new NotSupportedException("Outgoing Rich Presences are not supported via WebSocket.");
			}
			game.Name = Activity.Name;
			game.Type = Activity.Type;
			if (Activity is StreamingGame streamingGame)
			{
				game.StreamUrl = streamingGame.Url;
			}
			if (Activity is CustomStatusGame customStatusGame)
			{
				game.State = customStatusGame.State;
			}
			item = game;
		}
		else if (activity.IsSpecified)
		{
			item = null;
		}
		return (status ?? UserStatus.Online, status == UserStatus.AFK, statusSince.HasValue ? new long?(_statusSince.Value.ToUnixTimeMilliseconds()) : ((long?)null), item);
	}

	private async Task LogGatewayIntentsWarning()
	{
		if (_gatewayIntents.HasFlag(GatewayIntents.GuildPresences) && ((_shardedClient == null && !_presenceUpdated.HasSubscribers) || (_shardedClient != null && !_shardedClient._presenceUpdated.HasSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using the GuildPresences intent without listening to the PresenceUpdate event, consider removing the intent from your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!_gatewayIntents.HasFlag(GatewayIntents.GuildPresences) && ((_shardedClient == null && _presenceUpdated.HasSubscribers) || (_shardedClient != null && _shardedClient._presenceUpdated.HasSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using the PresenceUpdate event without specifying the GuildPresences intent. Discord wont send this event to your client without the intent set in your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		bool hasGuildScheduledEventsSubscribers = _guildScheduledEventCancelled.HasSubscribers || _guildScheduledEventUserRemove.HasSubscribers || _guildScheduledEventCompleted.HasSubscribers || _guildScheduledEventCreated.HasSubscribers || _guildScheduledEventStarted.HasSubscribers || _guildScheduledEventUpdated.HasSubscribers || _guildScheduledEventUserAdd.HasSubscribers;
		bool shardedClientHasGuildScheduledEventsSubscribers = _shardedClient != null && (_shardedClient._guildScheduledEventCancelled.HasSubscribers || _shardedClient._guildScheduledEventUserRemove.HasSubscribers || _shardedClient._guildScheduledEventCompleted.HasSubscribers || _shardedClient._guildScheduledEventCreated.HasSubscribers || _shardedClient._guildScheduledEventStarted.HasSubscribers || _shardedClient._guildScheduledEventUpdated.HasSubscribers || _shardedClient._guildScheduledEventUserAdd.HasSubscribers);
		if (_gatewayIntents.HasFlag(GatewayIntents.GuildScheduledEvents) && ((_shardedClient == null && !hasGuildScheduledEventsSubscribers) || (_shardedClient != null && !shardedClientHasGuildScheduledEventsSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using the GuildScheduledEvents gateway intent without listening to any events related to that intent, consider removing the intent from your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!_gatewayIntents.HasFlag(GatewayIntents.GuildScheduledEvents) && (((_shardedClient == null) & hasGuildScheduledEventsSubscribers) || ((_shardedClient != null) & shardedClientHasGuildScheduledEventsSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using events related to the GuildScheduledEvents gateway intent without specifying the intent. Discord wont send this event to your client without the intent set in your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		bool hasInviteEventSubscribers = _inviteCreatedEvent.HasSubscribers || _inviteDeletedEvent.HasSubscribers;
		bool shardedClientHasInviteEventSubscribers = _shardedClient != null && (_shardedClient._inviteCreatedEvent.HasSubscribers || _shardedClient._inviteDeletedEvent.HasSubscribers);
		if (_gatewayIntents.HasFlag(GatewayIntents.GuildInvites) && ((_shardedClient == null && !hasInviteEventSubscribers) || (_shardedClient != null && !shardedClientHasInviteEventSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using the GuildInvites gateway intent without listening to any events related to that intent, consider removing the intent from your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!_gatewayIntents.HasFlag(GatewayIntents.GuildInvites) && (((_shardedClient == null) & hasInviteEventSubscribers) || ((_shardedClient != null) & shardedClientHasInviteEventSubscribers)))
		{
			await _gatewayLogger.WarningAsync("You're using events related to the GuildInvites gateway intent without specifying the intent. Discord wont send this event to your client without the intent set in your config.").ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task RunHeartbeatAsync(int intervalMillis, CancellationToken cancelToken)
	{
		int delayInterval = (int)((double)intervalMillis * 0.9);
		try
		{
			await _gatewayLogger.DebugAsync("Heartbeat Started").ConfigureAwait(continueOnCapturedContext: false);
			while (!cancelToken.IsCancellationRequested)
			{
				int tickCount = Environment.TickCount;
				if (_heartbeatTimes.Count != 0 && tickCount - _lastMessageTime > intervalMillis && ConnectionState == ConnectionState.Connected && (_guildDownloadTask?.IsCompleted ?? true))
				{
					_connection.Error(new GatewayReconnectException("Server missed last heartbeat"));
					return;
				}
				_heartbeatTimes.Enqueue(tickCount);
				try
				{
					await ApiClient.SendHeartbeatAsync(_lastSeq).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (Exception exception)
				{
					await _gatewayLogger.WarningAsync("Heartbeat Errored", exception).ConfigureAwait(continueOnCapturedContext: false);
				}
				await Task.Delay(Math.Max(0, delayInterval - Latency), cancelToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			await _gatewayLogger.DebugAsync("Heartbeat Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			await _gatewayLogger.DebugAsync("Heartbeat Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception2)
		{
			await _gatewayLogger.ErrorAsync("Heartbeat Errored", exception2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task WaitForGuildsAsync(CancellationToken cancelToken, Logger logger)
	{
		try
		{
			await logger.DebugAsync("GuildDownloader Started").ConfigureAwait(continueOnCapturedContext: false);
			while (_unavailableGuildCount != 0 && Environment.TickCount - _lastGuildAvailableTime < BaseConfig.MaxWaitBetweenGuildAvailablesBeforeReady)
			{
				await Task.Delay(500, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			await logger.DebugAsync("GuildDownloader Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			await logger.DebugAsync("GuildDownloader Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception)
		{
			await logger.ErrorAsync("GuildDownloader Errored", exception).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private Task SyncGuildsAsync()
	{
		ImmutableArray<ulong> immutableArray = (from x in Guilds
			where !x.IsSynced
			select x.Id).ToImmutableArray();
		if (immutableArray.Length > 0)
		{
			return ApiClient.SendGuildSyncAsync(immutableArray);
		}
		return Task.CompletedTask;
	}

	internal SocketGuild AddGuild(ExtendedGuild model, ClientState state)
	{
		SocketGuild socketGuild = SocketGuild.Create(this, state, model);
		state.AddGuild(socketGuild);
		if (model.Large)
		{
			_largeGuilds.Enqueue(model.Id);
		}
		return socketGuild;
	}

	internal SocketGuild RemoveGuild(ulong id)
	{
		return State.RemoveGuild(id);
	}

	internal ISocketPrivateChannel AddPrivateChannel(Channel model, ClientState state)
	{
		ISocketPrivateChannel socketPrivateChannel = SocketChannel.CreatePrivate(this, state, model);
		state.AddChannel(socketPrivateChannel as SocketChannel);
		return socketPrivateChannel;
	}

	internal SocketDMChannel CreateDMChannel(ulong channelId, User model, ClientState state)
	{
		return SocketDMChannel.Create(this, state, channelId, model);
	}

	internal SocketDMChannel CreateDMChannel(ulong channelId, SocketUser user, ClientState state)
	{
		return new SocketDMChannel(this, channelId, user);
	}

	internal ISocketPrivateChannel RemovePrivateChannel(ulong id)
	{
		ISocketPrivateChannel socketPrivateChannel = State.RemoveChannel(id) as ISocketPrivateChannel;
		if (socketPrivateChannel != null)
		{
			foreach (SocketUser recipient in socketPrivateChannel.Recipients)
			{
				recipient.GlobalUser.RemoveRef(this);
			}
		}
		return socketPrivateChannel;
	}

	internal void RemoveDMChannels()
	{
		IReadOnlyCollection<SocketDMChannel> dMChannels = State.DMChannels;
		State.PurgeDMChannels();
		foreach (SocketDMChannel item in dMChannels)
		{
			item.Recipient.GlobalUser.RemoveRef(this);
		}
	}

	internal void EnsureGatewayIntent(GatewayIntents intents)
	{
		if (!_gatewayIntents.HasFlag(intents))
		{
			IEnumerable<GatewayIntents> source = from GatewayIntents x in Enum.GetValues(typeof(GatewayIntents))
				where intents.HasFlag(x) && !_gatewayIntents.HasFlag(x)
				select x;
			throw new InvalidOperationException("Missing required gateway intent" + ((source.Count() > 1) ? "s" : "") + " " + string.Join(", ", source.Select((GatewayIntents x) => x.ToString())) + " in order to execute this operation.");
		}
	}

	internal bool HasGatewayIntent(GatewayIntents intents)
	{
		return _gatewayIntents.HasFlag(intents);
	}

	private Task GuildAvailableAsync(SocketGuild guild)
	{
		if (!guild.IsConnected)
		{
			guild.IsConnected = true;
			return TimedInvokeAsync(_guildAvailableEvent, "GuildAvailable", guild);
		}
		return Task.CompletedTask;
	}

	private Task GuildUnavailableAsync(SocketGuild guild)
	{
		if (guild.IsConnected)
		{
			guild.IsConnected = false;
			return TimedInvokeAsync(_guildUnavailableEvent, "GuildUnavailable", guild);
		}
		return Task.CompletedTask;
	}

	private Task TimedInvokeAsync(AsyncEvent<Func<Task>> eventHandler, string name)
	{
		if (eventHandler.HasSubscribers)
		{
			if (HandlerTimeout.HasValue)
			{
				return TimeoutWrap(name, eventHandler.InvokeAsync);
			}
			return eventHandler.InvokeAsync();
		}
		return Task.CompletedTask;
	}

	private Task TimedInvokeAsync<T>(AsyncEvent<Func<T, Task>> eventHandler, string name, T arg)
	{
		if (eventHandler.HasSubscribers)
		{
			if (HandlerTimeout.HasValue)
			{
				return TimeoutWrap(name, () => eventHandler.InvokeAsync(arg));
			}
			return eventHandler.InvokeAsync(arg);
		}
		return Task.CompletedTask;
	}

	private Task TimedInvokeAsync<T1, T2>(AsyncEvent<Func<T1, T2, Task>> eventHandler, string name, T1 arg1, T2 arg2)
	{
		if (eventHandler.HasSubscribers)
		{
			if (HandlerTimeout.HasValue)
			{
				return TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2));
			}
			return eventHandler.InvokeAsync(arg1, arg2);
		}
		return Task.CompletedTask;
	}

	private Task TimedInvokeAsync<T1, T2, T3>(AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3)
	{
		if (eventHandler.HasSubscribers)
		{
			if (HandlerTimeout.HasValue)
			{
				return TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3));
			}
			return eventHandler.InvokeAsync(arg1, arg2, arg3);
		}
		return Task.CompletedTask;
	}

	private Task TimedInvokeAsync<T1, T2, T3, T4>(AsyncEvent<Func<T1, T2, T3, T4, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (eventHandler.HasSubscribers)
		{
			if (HandlerTimeout.HasValue)
			{
				return TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3, arg4));
			}
			return eventHandler.InvokeAsync(arg1, arg2, arg3, arg4);
		}
		return Task.CompletedTask;
	}

	private Task TimedInvokeAsync<T1, T2, T3, T4, T5>(AsyncEvent<Func<T1, T2, T3, T4, T5, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (eventHandler.HasSubscribers)
		{
			if (HandlerTimeout.HasValue)
			{
				return TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3, arg4, arg5));
			}
			return eventHandler.InvokeAsync(arg1, arg2, arg3, arg4, arg5);
		}
		return Task.CompletedTask;
	}

	private async Task TimeoutWrap(string name, Func<Task> action)
	{
		try
		{
			Task timeoutTask = Task.Delay(HandlerTimeout.Value);
			Task handlersTask = action();
			if (await Task.WhenAny(timeoutTask, handlersTask).ConfigureAwait(continueOnCapturedContext: false) == timeoutTask)
			{
				await _gatewayLogger.WarningAsync("A " + name + " handler is blocking the gateway task.").ConfigureAwait(continueOnCapturedContext: false);
			}
			await handlersTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception)
		{
			await _gatewayLogger.WarningAsync("A " + name + " handler has thrown an unhandled exception.", exception).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private Task UnknownGlobalUserAsync(string evnt, ulong userId)
	{
		string text = $"{evnt} User={userId}";
		return _gatewayLogger.WarningAsync("Unknown User (" + text + ").");
	}

	private Task UnknownChannelUserAsync(string evnt, ulong userId, ulong channelId)
	{
		string text = $"{evnt} User={userId} Channel={channelId}";
		return _gatewayLogger.WarningAsync("Unknown User (" + text + ").");
	}

	private Task UnknownGuildUserAsync(string evnt, ulong userId, ulong guildId)
	{
		string text = $"{evnt} User={userId} Guild={guildId}";
		return _gatewayLogger.WarningAsync("Unknown User (" + text + ").");
	}

	private Task IncompleteGuildUserAsync(string evnt, ulong userId, ulong guildId)
	{
		string text = $"{evnt} User={userId} Guild={guildId}";
		return _gatewayLogger.DebugAsync("User has not been downloaded (" + text + ").");
	}

	private Task UnknownChannelAsync(string evnt, ulong channelId)
	{
		string text = $"{evnt} Channel={channelId}";
		return _gatewayLogger.WarningAsync("Unknown Channel (" + text + ").");
	}

	private Task UnknownChannelAsync(string evnt, ulong channelId, ulong guildId)
	{
		if (guildId == 0L)
		{
			return UnknownChannelAsync(evnt, channelId);
		}
		string text = $"{evnt} Channel={channelId} Guild={guildId}";
		return _gatewayLogger.WarningAsync("Unknown Channel (" + text + ").");
	}

	private Task UnknownRoleAsync(string evnt, ulong roleId, ulong guildId)
	{
		string text = $"{evnt} Role={roleId} Guild={guildId}";
		return _gatewayLogger.WarningAsync("Unknown Role (" + text + ").");
	}

	private Task UnknownGuildAsync(string evnt, ulong guildId)
	{
		string text = $"{evnt} Guild={guildId}";
		return _gatewayLogger.WarningAsync("Unknown Guild (" + text + ").");
	}

	private Task UnknownGuildEventAsync(string evnt, ulong eventId, ulong guildId)
	{
		string text = $"{evnt} Event={eventId} Guild={guildId}";
		return _gatewayLogger.WarningAsync("Unknown Guild Event (" + text + ").");
	}

	private Task UnsyncedGuildAsync(string evnt, ulong guildId)
	{
		string text = $"{evnt} Guild={guildId}";
		return _gatewayLogger.DebugAsync("Unsynced Guild (" + text + ").");
	}

	internal int GetAudioId()
	{
		return _nextAudioId++;
	}

	async Task<ISubscription> IDiscordClient.GetSKUSubscriptionAsync(ulong skuId, ulong subscriptionId, RequestOptions options)
	{
		return await GetSKUSubscriptionAsync(skuId, subscriptionId, options);
	}

	IAsyncEnumerable<IReadOnlyCollection<ISubscription>> IDiscordClient.GetSKUSubscriptionsAsync(ulong skuId, int limit, ulong? afterId, ulong? beforeId, ulong? userId, RequestOptions options)
	{
		return GetSKUSubscriptionsAsync(skuId, limit, afterId, beforeId, userId, options);
	}

	async Task<IEntitlement> IDiscordClient.CreateTestEntitlementAsync(ulong skuId, ulong ownerId, SubscriptionOwnerType ownerType, RequestOptions options)
	{
		return await CreateTestEntitlementAsync(skuId, ownerId, ownerType, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IChannel> IDiscordClient.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return (mode != CacheMode.AllowDownload) ? GetChannel(id) : (await GetChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	Task<IReadOnlyCollection<IPrivateChannel>> IDiscordClient.GetPrivateChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IPrivateChannel>)PrivateChannels);
	}

	Task<IReadOnlyCollection<IDMChannel>> IDiscordClient.GetDMChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IDMChannel>)DMChannels);
	}

	Task<IReadOnlyCollection<IGroupChannel>> IDiscordClient.GetGroupChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGroupChannel>)GroupChannels);
	}

	async Task<IReadOnlyCollection<IConnection>> IDiscordClient.GetConnectionsAsync(RequestOptions options)
	{
		return await GetConnectionsAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IInvite> IDiscordClient.GetInviteAsync(string inviteId, RequestOptions options)
	{
		return await GetInviteAsync(inviteId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IGuild> IDiscordClient.GetGuildAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuild)GetGuild(id));
	}

	Task<IReadOnlyCollection<IGuild>> IDiscordClient.GetGuildsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGuild>)Guilds);
	}

	async Task<IGuild> IDiscordClient.CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
	{
		return await CreateGuildAsync(name, region, jpegIcon).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUser> IDiscordClient.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		SocketUser user = GetUser(id);
		if (user != null || mode == CacheMode.CacheOnly)
		{
			return user;
		}
		return await Rest.GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IUser> IDiscordClient.GetUserAsync(string username, string discriminator, RequestOptions options)
	{
		return Task.FromResult((IUser)GetUser(username, discriminator));
	}

	async Task<IReadOnlyCollection<IVoiceRegion>> IDiscordClient.GetVoiceRegionsAsync(RequestOptions options)
	{
		return await GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IVoiceRegion> IDiscordClient.GetVoiceRegionAsync(string id, RequestOptions options)
	{
		return await GetVoiceRegionAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IApplicationCommand> IDiscordClient.GetGlobalApplicationCommandAsync(ulong id, RequestOptions options)
	{
		return await GetGlobalApplicationCommandAsync(id, options);
	}

	async Task<IReadOnlyCollection<IApplicationCommand>> IDiscordClient.GetGlobalApplicationCommandsAsync(bool withLocalizations, string locale, RequestOptions options)
	{
		return await GetGlobalApplicationCommandsAsync(withLocalizations, locale, options);
	}

	async Task<IApplicationCommand> IDiscordClient.CreateGlobalApplicationCommand(ApplicationCommandProperties properties, RequestOptions options)
	{
		return await CreateGlobalApplicationCommandAsync(properties, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IApplicationCommand>> IDiscordClient.BulkOverwriteGlobalApplicationCommand(ApplicationCommandProperties[] properties, RequestOptions options)
	{
		return await BulkOverwriteGlobalApplicationCommandsAsync(properties, options);
	}

	Task IDiscordClient.StartAsync()
	{
		return StartAsync();
	}

	Task IDiscordClient.StopAsync()
	{
		return StopAsync();
	}

	private async Task ProcessMessageAsync(GatewayOpCode opCode, int? seq, string type, object payload)
	{
		if (seq.HasValue)
		{
			_lastSeq = seq.Value;
		}
		_lastMessageTime = Environment.TickCount;
		try
		{
			switch (opCode)
			{
			case GatewayOpCode.Hello:
			{
				await _gatewayLogger.DebugAsync("Received Hello").ConfigureAwait(continueOnCapturedContext: false);
				HelloEvent helloEvent = (payload as JToken).ToObject<HelloEvent>(_serializer);
				_heartbeatTask = RunHeartbeatAsync(helloEvent.HeartbeatInterval, _connection.CancelToken);
				break;
			}
			case GatewayOpCode.Heartbeat:
				await _gatewayLogger.DebugAsync("Received Heartbeat").ConfigureAwait(continueOnCapturedContext: false);
				await ApiClient.SendHeartbeatAsync(_lastSeq).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case GatewayOpCode.HeartbeatAck:
			{
				await _gatewayLogger.DebugAsync("Received HeartbeatAck").ConfigureAwait(continueOnCapturedContext: false);
				if (_heartbeatTimes.TryDequeue(out var result))
				{
					int num4 = (int)(Environment.TickCount - result);
					int latency = Latency;
					Latency = num4;
					await TimedInvokeAsync(_latencyUpdatedEvent, "LatencyUpdated", latency, num4).ConfigureAwait(continueOnCapturedContext: false);
				}
				break;
			}
			case GatewayOpCode.InvalidSession:
				await _gatewayLogger.DebugAsync("Received InvalidSession").ConfigureAwait(continueOnCapturedContext: false);
				await _gatewayLogger.WarningAsync("Failed to resume previous session").ConfigureAwait(continueOnCapturedContext: false);
				_sessionId = null;
				_lastSeq = 0;
				ApiClient.ResumeGatewayUrl = null;
				if (_shardedClient != null)
				{
					await _shardedClient.AcquireIdentifyLockAsync(ShardId, _connection.CancelToken).ConfigureAwait(continueOnCapturedContext: false);
					try
					{
						await ApiClient.SendIdentifyAsync(100, ShardId, TotalShards, _gatewayIntents, BuildCurrentStatus()).ConfigureAwait(continueOnCapturedContext: false);
					}
					finally
					{
						_shardedClient.ReleaseIdentifyLock();
					}
				}
				else
				{
					await ApiClient.SendIdentifyAsync(100, ShardId, TotalShards, _gatewayIntents, BuildCurrentStatus()).ConfigureAwait(continueOnCapturedContext: false);
				}
				break;
			case GatewayOpCode.Reconnect:
				await _gatewayLogger.DebugAsync("Received Reconnect").ConfigureAwait(continueOnCapturedContext: false);
				_connection.Error(new GatewayReconnectException("Server requested a reconnect"));
				break;
			case GatewayOpCode.Dispatch:
				switch (type)
				{
				case "READY":
					try
					{
						await _gatewayLogger.DebugAsync("Received Dispatch (READY)").ConfigureAwait(continueOnCapturedContext: false);
						ReadyEvent data8 = (payload as JToken).ToObject<ReadyEvent>(_serializer);
						ClientState state = new ClientState(data8.Guilds.Length, data8.PrivateChannels.Length);
						SocketSelfUser currentUser = SocketSelfUser.Create(this, state, data8.User);
						Rest.CreateRestSelfUser(data8.User);
						ImmutableList<IActivity> activities = (_activity.IsSpecified ? ImmutableList.Create(_activity.Value) : null);
						currentUser.Presence = new SocketPresence(Status, null, activities);
						ApiClient.CurrentUserId = currentUser.Id;
						ApiClient.CurrentApplicationId = data8.Application?.Id;
						Rest.CurrentUser = RestSelfUser.Create(this, data8.User);
						int unavailableGuilds = 0;
						for (int i = 0; i < data8.Guilds.Length; i++)
						{
							ExtendedGuild model = data8.Guilds[i];
							SocketGuild socketGuild3 = AddGuild(model, state);
							if (!socketGuild3.IsAvailable)
							{
								unavailableGuilds++;
							}
							else
							{
								await GuildAvailableAsync(socketGuild3).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
						for (int num2 = 0; num2 < data8.PrivateChannels.Length; num2++)
						{
							AddPrivateChannel(data8.PrivateChannels[num2], state);
						}
						_sessionId = data8.SessionId;
						ApiClient.ResumeGatewayUrl = data8.ResumeGatewayUrl;
						_unavailableGuildCount = unavailableGuilds;
						CurrentUser = currentUser;
						_previousSessionUser = CurrentUser;
						State = state;
					}
					catch (Exception innerException)
					{
						_connection.CriticalError(new Exception("Processing READY failed", innerException));
						return;
					}
					_lastGuildAvailableTime = Environment.TickCount;
					_guildDownloadTask = WaitForGuildsAsync(_connection.CancelToken, _gatewayLogger).ContinueWith((Func<Task, Task>)async delegate(Task x)
					{
						if (x.IsFaulted)
						{
							_connection.Error(x.Exception);
						}
						else if (!_connection.CancelToken.IsCancellationRequested)
						{
							if (BaseConfig.AlwaysDownloadUsers)
							{
								try
								{
									DownloadUsersAsync(Guilds.Where((SocketGuild socketGuild9) => socketGuild9.IsAvailable && !socketGuild9.HasAllMembers));
								}
								catch (Exception exception)
								{
									await _gatewayLogger.WarningAsync(exception);
								}
							}
							await TimedInvokeAsync(_readyEvent, "Ready").ConfigureAwait(continueOnCapturedContext: false);
							await _gatewayLogger.InfoAsync("Ready").ConfigureAwait(continueOnCapturedContext: false);
						}
					});
					_connection.CompleteAsync();
					break;
				case "RESUMED":
					await _gatewayLogger.DebugAsync("Received Dispatch (RESUMED)").ConfigureAwait(continueOnCapturedContext: false);
					_connection.CompleteAsync();
					foreach (SocketGuild guild40 in State.Guilds)
					{
						if (guild40.IsAvailable)
						{
							await GuildAvailableAsync(guild40).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					CurrentUser = _previousSessionUser;
					await _gatewayLogger.InfoAsync("Resumed previous session").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "GUILD_CREATE":
				{
					ExtendedGuild data15 = (payload as JToken).ToObject<ExtendedGuild>(_serializer);
					if (data15.Unavailable == false)
					{
						type = "GUILD_AVAILABLE";
						_lastGuildAvailableTime = Environment.TickCount;
						await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_AVAILABLE)").ConfigureAwait(continueOnCapturedContext: false);
						SocketGuild guild3 = State.GetGuild(data15.Id);
						if (guild3 == null)
						{
							await UnknownGuildAsync(type, data15.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						guild3.Update(State, data15);
						if (_unavailableGuildCount != 0)
						{
							_unavailableGuildCount--;
						}
						await GuildAvailableAsync(guild3).ConfigureAwait(continueOnCapturedContext: false);
						if (guild3.DownloadedMemberCount >= guild3.MemberCount && !guild3.DownloaderPromise.IsCompleted)
						{
							guild3.CompleteDownloadUsers();
							await TimedInvokeAsync(_guildMembersDownloadedEvent, "GuildMembersDownloaded", guild3).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					else
					{
						await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
						SocketGuild guild3 = AddGuild(data15, State);
						if (guild3 == null)
						{
							await UnknownGuildAsync(type, data15.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_joinedGuildEvent, "JoinedGuild", guild3).ConfigureAwait(continueOnCapturedContext: false);
						await GuildAvailableAsync(guild3).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "GUILD_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Guild guild15 = (payload as JToken).ToObject<Guild>(_serializer);
					SocketGuild guild16 = State.GetGuild(guild15.Id);
					if (guild16 != null)
					{
						SocketGuild arg23 = guild16.Clone();
						guild16.Update(State, guild15);
						await TimedInvokeAsync(_guildUpdatedEvent, "GuildUpdated", arg23, guild16).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guild15.Id).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_EMOJIS_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_EMOJIS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildEmojiUpdateEvent guildEmojiUpdateEvent = (payload as JToken).ToObject<GuildEmojiUpdateEvent>(_serializer);
					SocketGuild guild33 = State.GetGuild(guildEmojiUpdateEvent.GuildId);
					if (guild33 != null)
					{
						SocketGuild arg44 = guild33.Clone();
						guild33.Update(State, guildEmojiUpdateEvent);
						await TimedInvokeAsync(_guildUpdatedEvent, "GuildUpdated", arg44, guild33).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildEmojiUpdateEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_SYNC":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (GUILD_SYNC)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "GUILD_DELETE":
				{
					ExtendedGuild data15 = (payload as JToken).ToObject<ExtendedGuild>(_serializer);
					if (data15.Unavailable == true)
					{
						type = "GUILD_UNAVAILABLE";
						await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_UNAVAILABLE)").ConfigureAwait(continueOnCapturedContext: false);
						SocketGuild guild38 = State.GetGuild(data15.Id);
						if (guild38 == null)
						{
							await UnknownGuildAsync(type, data15.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await GuildUnavailableAsync(guild38).ConfigureAwait(continueOnCapturedContext: false);
						_unavailableGuildCount++;
					}
					else
					{
						await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
						SocketGuild guild3 = RemoveGuild(data15.Id);
						if (guild3 == null)
						{
							await UnknownGuildAsync(type, data15.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await GuildUnavailableAsync(guild3).ConfigureAwait(continueOnCapturedContext: false);
						await TimedInvokeAsync(_leftGuildEvent, "LeftGuild", guild3).ConfigureAwait(continueOnCapturedContext: false);
						((IDisposable)guild3).Dispose();
					}
					break;
				}
				case "GUILD_STICKERS_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_STICKERS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildStickerUpdateEvent data12 = (payload as JToken).ToObject<GuildStickerUpdateEvent>(_serializer);
					SocketGuild guild17 = State.GetGuild(data12.GuildId);
					if (guild17 == null)
					{
						await UnknownGuildAsync(type, data12.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					IEnumerable<Discord.API.Sticker> enumerable = data12.Stickers.Where((Discord.API.Sticker x) => !guild17.Stickers.Any((SocketCustomSticker y) => y.Id == x.Id));
					IEnumerable<SocketCustomSticker> deletedStickers = guild17.Stickers.Where((SocketCustomSticker x) => !data12.Stickers.Any((Discord.API.Sticker y) => y.Id == x.Id));
					(SocketCustomSticker Entity, Discord.API.Sticker Model)[] updatedStickers = (from x in data12.Stickers.Select(delegate(Discord.API.Sticker x)
						{
							SocketCustomSticker socketCustomSticker = guild17.Stickers.FirstOrDefault((SocketCustomSticker y) => y.Id == x.Id);
							if (socketCustomSticker == null)
							{
								return ((SocketCustomSticker Entity, Discord.API.Sticker Model)?)null;
							}
							return (!socketCustomSticker.Equals(x)) ? new(SocketCustomSticker, Discord.API.Sticker)?((socketCustomSticker, x)) : (((SocketCustomSticker, Discord.API.Sticker)?)null);
						})
						where x.HasValue
						select x.Value).ToArray();
					foreach (Discord.API.Sticker item2 in enumerable)
					{
						SocketCustomSticker arg26 = guild17.AddSticker(item2);
						await TimedInvokeAsync(_guildStickerCreated, "GuildStickerCreated", arg26);
					}
					foreach (SocketCustomSticker item3 in deletedStickers)
					{
						SocketCustomSticker arg27 = guild17.RemoveSticker(item3.Id);
						await TimedInvokeAsync(_guildStickerDeleted, "GuildStickerDeleted", arg27);
					}
					(SocketCustomSticker Entity, Discord.API.Sticker Model)[] array = updatedStickers;
					for (int unavailableGuilds = 0; unavailableGuilds < array.Length; unavailableGuilds++)
					{
						(SocketCustomSticker, Discord.API.Sticker) tuple = array[unavailableGuilds];
						SocketCustomSticker arg28 = tuple.Item1.Clone();
						tuple.Item1.Update(tuple.Item2);
						await TimedInvokeAsync(_guildStickerUpdated, "GuildStickerUpdated", arg28, tuple.Item1);
					}
					break;
				}
				case "CHANNEL_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel channel13 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketChannel socketChannel3;
					if (channel13.GuildId.IsSpecified)
					{
						SocketGuild guild30 = State.GetGuild(channel13.GuildId.Value);
						if (guild30 == null)
						{
							await UnknownGuildAsync(type, channel13.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						socketChannel3 = guild30.AddChannel(State, channel13);
						if (!guild30.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild30.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					else
					{
						socketChannel3 = State.GetChannel(channel13.Id);
						if (socketChannel3 != null)
						{
							return;
						}
						socketChannel3 = AddPrivateChannel(channel13, State) as SocketChannel;
					}
					if (socketChannel3 != null)
					{
						await TimedInvokeAsync(_channelCreatedEvent, "ChannelCreated", socketChannel3).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "CHANNEL_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel channel7 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketChannel channel8 = State.GetChannel(channel7.Id);
					if (channel8 != null)
					{
						SocketChannel arg25 = channel8.Clone();
						channel8.Update(State, channel7);
						SocketGuild socketGuild5 = (channel8 as SocketGuildChannel)?.Guild;
						if (!(socketGuild5?.IsSynced ?? true))
						{
							await UnsyncedGuildAsync(type, socketGuild5.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_channelUpdatedEvent, "ChannelUpdated", arg25, channel8).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, channel7.Id).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "CHANNEL_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel channel6 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketChannel socketChannel2;
					if (channel6.GuildId.IsSpecified)
					{
						SocketGuild guild10 = State.GetGuild(channel6.GuildId.Value);
						if (guild10 == null)
						{
							await UnknownGuildAsync(type, channel6.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						socketChannel2 = guild10.RemoveChannel(State, channel6.Id);
						if (!guild10.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild10.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					else
					{
						socketChannel2 = RemovePrivateChannel(channel6.Id) as SocketChannel;
					}
					if (socketChannel2 == null)
					{
						await UnknownChannelAsync(type, channel6.Id, channel6.GuildId.GetValueOrDefault(0uL)).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					await TimedInvokeAsync(_channelDestroyedEvent, "ChannelDestroyed", socketChannel2).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "GUILD_MEMBER_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_MEMBER_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					GuildMemberAddEvent guildMemberAddEvent = (payload as JToken).ToObject<GuildMemberAddEvent>(_serializer);
					SocketGuild guild11 = State.GetGuild(guildMemberAddEvent.GuildId);
					if (guild11 != null)
					{
						SocketGuildUser arg19 = guild11.AddOrUpdateUser(guildMemberAddEvent);
						guild11.MemberCount++;
						if (!guild11.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild11.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_userJoinedEvent, "UserJoined", arg19).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildMemberAddEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_MEMBER_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_MEMBER_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildMemberUpdateEvent data23 = (payload as JToken).ToObject<GuildMemberUpdateEvent>(_serializer);
					SocketGuild guild31 = State.GetGuild(data23.GuildId);
					if (guild31 != null)
					{
						SocketGuildUser user15 = guild31.GetUser(data23.User.Id);
						if (!guild31.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild31.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (user15 != null)
						{
							SocketGuildUser before2 = user15.Clone();
							if (user15.GlobalUser.Update(State, data23.User))
							{
								await TimedInvokeAsync(_userUpdatedEvent, "UserUpdated", before2.GlobalUser, user15).ConfigureAwait(continueOnCapturedContext: false);
							}
							user15.Update(State, data23);
							await TimedInvokeAsync<Cacheable<SocketGuildUser, ulong>, SocketGuildUser>(arg1: new Cacheable<SocketGuildUser, ulong>(before2, user15.Id, hasValue: true, () => Task.FromResult<SocketGuildUser>(null)), eventHandler: _guildMemberUpdatedEvent, name: "GuildMemberUpdated", arg2: user15).ConfigureAwait(continueOnCapturedContext: false);
						}
						else
						{
							user15 = guild31.AddOrUpdateUser(data23);
							await TimedInvokeAsync<Cacheable<SocketGuildUser, ulong>, SocketGuildUser>(arg1: new Cacheable<SocketGuildUser, ulong>(null, user15.Id, hasValue: false, () => Task.FromResult<SocketGuildUser>(null)), eventHandler: _guildMemberUpdatedEvent, name: "GuildMemberUpdated", arg2: user15).ConfigureAwait(continueOnCapturedContext: false);
						}
						break;
					}
					await UnknownGuildAsync(type, data23.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_MEMBER_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_MEMBER_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildMemberRemoveEvent data16 = (payload as JToken).ToObject<GuildMemberRemoveEvent>(_serializer);
					SocketGuild guild23 = State.GetGuild(data16.GuildId);
					if (guild23 != null)
					{
						SocketUser socketUser5 = guild23.RemoveUser(data16.User.Id);
						guild23.MemberCount--;
						if (!guild23.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild23.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (socketUser5 == null)
						{
							socketUser5 = State.GetUser(data16.User.Id);
						}
						if (socketUser5 != null)
						{
							socketUser5.Update(State, data16.User);
						}
						else
						{
							socketUser5 = State.GetOrAddUser(data16.User.Id, (ulong x) => SocketGlobalUser.Create(this, State, data16.User));
						}
						await TimedInvokeAsync(_userLeftEvent, "UserLeft", guild23, socketUser5).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, data16.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_MEMBERS_CHUNK":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_MEMBERS_CHUNK)").ConfigureAwait(continueOnCapturedContext: false);
					GuildMembersChunkEvent guildMembersChunkEvent = (payload as JToken).ToObject<GuildMembersChunkEvent>(_serializer);
					SocketGuild guild39 = State.GetGuild(guildMembersChunkEvent.GuildId);
					if (guild39 != null)
					{
						GuildMember[] members = guildMembersChunkEvent.Members;
						foreach (GuildMember model2 in members)
						{
							guild39.AddOrUpdateUser(model2);
						}
						if (guild39.DownloadedMemberCount >= guild39.MemberCount && !guild39.DownloaderPromise.IsCompleted)
						{
							guild39.CompleteDownloadUsers();
							await TimedInvokeAsync(_guildMembersDownloadedEvent, "GuildMembersDownloaded", guild39).ConfigureAwait(continueOnCapturedContext: false);
						}
						break;
					}
					await UnknownGuildAsync(type, guildMembersChunkEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_JOIN_REQUEST_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_JOIN_REQUEST_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildJoinRequestDeleteEvent guildJoinRequestDeleteEvent = (payload as JToken).ToObject<GuildJoinRequestDeleteEvent>(_serializer);
					SocketGuild guild32 = State.GetGuild(guildJoinRequestDeleteEvent.GuildId);
					if (guild32 == null)
					{
						await UnknownGuildAsync(type, guildJoinRequestDeleteEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildUser socketGuildUser2 = guild32.RemoveUser(guildJoinRequestDeleteEvent.UserId);
					guild32.MemberCount--;
					await TimedInvokeAsync<Cacheable<SocketGuildUser, ulong>, SocketGuild>(arg1: new Cacheable<SocketGuildUser, ulong>(socketGuildUser2, guildJoinRequestDeleteEvent.UserId, socketGuildUser2 != null, () => Task.FromResult<SocketGuildUser>(null)), eventHandler: _guildJoinRequestDeletedEvent, name: "GuildJoinRequestDeleted", arg2: guild32).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "CHANNEL_RECIPIENT_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_RECIPIENT_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					RecipientEvent recipientEvent2 = (payload as JToken).ToObject<RecipientEvent>(_serializer);
					if (State.GetChannel(recipientEvent2.ChannelId) is SocketGroupChannel socketGroupChannel4)
					{
						SocketGroupUser orAddUser = socketGroupChannel4.GetOrAddUser(recipientEvent2.User);
						await TimedInvokeAsync(_recipientAddedEvent, "RecipientAdded", orAddUser).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, recipientEvent2.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "CHANNEL_RECIPIENT_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (CHANNEL_RECIPIENT_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					RecipientEvent recipientEvent = (payload as JToken).ToObject<RecipientEvent>(_serializer);
					if (State.GetChannel(recipientEvent.ChannelId) is SocketGroupChannel socketGroupChannel3)
					{
						SocketGroupUser socketGroupUser = socketGroupChannel3.RemoveUser(recipientEvent.User.Id);
						if (socketGroupUser == null)
						{
							await UnknownChannelUserAsync(type, recipientEvent.User.Id, recipientEvent.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_recipientRemovedEvent, "RecipientRemoved", socketGroupUser).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, recipientEvent.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_ROLE_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_ROLE_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildRoleCreateEvent guildRoleCreateEvent = (payload as JToken).ToObject<GuildRoleCreateEvent>(_serializer);
					SocketGuild guild13 = State.GetGuild(guildRoleCreateEvent.GuildId);
					if (guild13 != null)
					{
						SocketRole arg22 = guild13.AddRole(guildRoleCreateEvent.Role);
						if (!guild13.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild13.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_roleCreatedEvent, "RoleCreated", arg22).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildRoleCreateEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_ROLE_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_ROLE_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildRoleUpdateEvent guildRoleUpdateEvent = (payload as JToken).ToObject<GuildRoleUpdateEvent>(_serializer);
					SocketGuild guild21 = State.GetGuild(guildRoleUpdateEvent.GuildId);
					if (guild21 != null)
					{
						SocketRole role = guild21.GetRole(guildRoleUpdateEvent.Role.Id);
						if (role != null)
						{
							SocketRole arg32 = role.Clone();
							role.Update(State, guildRoleUpdateEvent.Role);
							if (!guild21.IsSynced)
							{
								await UnsyncedGuildAsync(type, guild21.Id).ConfigureAwait(continueOnCapturedContext: false);
								return;
							}
							await TimedInvokeAsync(_roleUpdatedEvent, "RoleUpdated", arg32, role).ConfigureAwait(continueOnCapturedContext: false);
							break;
						}
						await UnknownRoleAsync(type, guildRoleUpdateEvent.Role.Id, guild21.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					await UnknownGuildAsync(type, guildRoleUpdateEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_ROLE_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_ROLE_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildRoleDeleteEvent guildRoleDeleteEvent = (payload as JToken).ToObject<GuildRoleDeleteEvent>(_serializer);
					SocketGuild guild14 = State.GetGuild(guildRoleDeleteEvent.GuildId);
					if (guild14 != null)
					{
						SocketRole socketRole = guild14.RemoveRole(guildRoleDeleteEvent.RoleId);
						if (socketRole != null)
						{
							if (!guild14.IsSynced)
							{
								await UnsyncedGuildAsync(type, guild14.Id).ConfigureAwait(continueOnCapturedContext: false);
								return;
							}
							await TimedInvokeAsync(_roleDeletedEvent, "RoleDeleted", socketRole).ConfigureAwait(continueOnCapturedContext: false);
							break;
						}
						await UnknownRoleAsync(type, guildRoleDeleteEvent.RoleId, guild14.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					await UnknownGuildAsync(type, guildRoleDeleteEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_BAN_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_BAN_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					GuildBanEvent guildBanEvent = (payload as JToken).ToObject<GuildBanEvent>(_serializer);
					SocketGuild guild18 = State.GetGuild(guildBanEvent.GuildId);
					if (guild18 != null)
					{
						if (!guild18.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild18.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						SocketUser socketUser3 = guild18.GetUser(guildBanEvent.User.Id);
						if (socketUser3 == null)
						{
							socketUser3 = SocketUnknownUser.Create(this, State, guildBanEvent.User);
						}
						await TimedInvokeAsync(_userBannedEvent, "UserBanned", socketUser3, guild18).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildBanEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "GUILD_BAN_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_BAN_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					GuildBanEvent guildBanEvent2 = (payload as JToken).ToObject<GuildBanEvent>(_serializer);
					SocketGuild guild22 = State.GetGuild(guildBanEvent2.GuildId);
					if (guild22 != null)
					{
						if (!guild22.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild22.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						SocketUser socketUser4 = State.GetUser(guildBanEvent2.User.Id);
						if (socketUser4 == null)
						{
							socketUser4 = SocketUnknownUser.Create(this, State, guildBanEvent2.User);
						}
						await TimedInvokeAsync(_userUnbannedEvent, "UserUnbanned", socketUser4, guild22).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, guildBanEvent2.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "MESSAGE_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Message message2 = (payload as JToken).ToObject<Message>(_serializer);
					ISocketMessageChannel socketMessageChannel2 = GetChannel(message2.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild4 = (socketMessageChannel2 as SocketGuildChannel)?.Guild;
					if (socketGuild4 != null && !socketGuild4.IsSynced)
					{
						await UnsyncedGuildAsync(type, socketGuild4.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					if (socketMessageChannel2 == null)
					{
						if (message2.GuildId.IsSpecified)
						{
							await UnknownChannelAsync(type, message2.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						socketMessageChannel2 = CreateDMChannel(message2.ChannelId, message2.Author.Value, State);
					}
					SocketUser socketUser2 = ((socketGuild4 == null) ? (socketMessageChannel2 as SocketChannel).GetUser(message2.Author.Value.Id) : ((!message2.WebhookId.IsSpecified) ? ((SocketUser)socketGuild4.GetUser(message2.Author.Value.Id)) : ((SocketUser)SocketWebhookUser.Create(socketGuild4, State, message2.Author.Value, message2.WebhookId.Value))));
					if (socketUser2 == null)
					{
						if (socketGuild4 != null)
						{
							if (message2.Member.IsSpecified)
							{
								message2.Member.Value.User = message2.Author.Value;
								socketUser2 = socketGuild4.AddOrUpdateUser(message2.Member.Value);
							}
							else
							{
								socketUser2 = socketGuild4.AddOrUpdateUser(message2.Author.Value);
							}
						}
						else
						{
							if (!(socketMessageChannel2 is SocketGroupChannel socketGroupChannel))
							{
								await UnknownChannelUserAsync(type, message2.Author.Value.Id, socketMessageChannel2.Id).ConfigureAwait(continueOnCapturedContext: false);
								return;
							}
							socketUser2 = socketGroupChannel.GetOrAddUser(message2.Author.Value);
						}
					}
					SocketMessage socketMessage2 = SocketMessage.Create(this, State, socketUser2, socketMessageChannel2, message2);
					SocketChannelHelper.AddMessage(socketMessageChannel2, this, socketMessage2);
					await TimedInvokeAsync(_messageReceivedEvent, "MessageReceived", socketMessage2).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Message data24 = (payload as JToken).ToObject<Message>(_serializer);
					ISocketMessageChannel channel14 = GetChannel(data24.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild7 = (channel14 as SocketGuildChannel)?.Guild;
					if (socketGuild7 != null && !socketGuild7.IsSynced)
					{
						await UnsyncedGuildAsync(type, socketGuild7.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketMessage value3 = null;
					SocketMessage socketMessage4 = channel14?.GetCachedMessage(data24.Id);
					bool flag2 = socketMessage4 != null;
					SocketMessage arg46;
					if (flag2)
					{
						value3 = socketMessage4.Clone();
						socketMessage4.Update(State, data24);
						arg46 = socketMessage4;
					}
					else
					{
						SocketUser socketUser6;
						if (data24.Author.IsSpecified)
						{
							socketUser6 = ((socketGuild7 == null) ? (channel14 as SocketChannel)?.GetUser(data24.Author.Value.Id) : ((!data24.WebhookId.IsSpecified) ? ((SocketUser)socketGuild7.GetUser(data24.Author.Value.Id)) : ((SocketUser)SocketWebhookUser.Create(socketGuild7, State, data24.Author.Value, data24.WebhookId.Value))));
							if (socketUser6 == null)
							{
								if (socketGuild7 != null)
								{
									if (data24.Member.IsSpecified)
									{
										data24.Member.Value.User = data24.Author.Value;
										socketUser6 = socketGuild7.AddOrUpdateUser(data24.Member.Value);
									}
									else
									{
										socketUser6 = socketGuild7.AddOrUpdateUser(data24.Author.Value);
									}
								}
								else if (channel14 is SocketGroupChannel socketGroupChannel5)
								{
									socketUser6 = socketGroupChannel5.GetOrAddUser(data24.Author.Value);
								}
							}
						}
						else
						{
							socketUser6 = new SocketUnknownUser(this, 0uL);
						}
						if (channel14 == null)
						{
							if (data24.GuildId.IsSpecified)
							{
								await UnknownChannelAsync(type, data24.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
								return;
							}
							if (data24.Author.IsSpecified)
							{
								socketUser6 = ((SocketDMChannel)(channel14 = CreateDMChannel(data24.ChannelId, data24.Author.Value, State))).Recipient;
							}
							else
							{
								channel14 = CreateDMChannel(data24.ChannelId, socketUser6, State);
							}
						}
						arg46 = SocketMessage.Create(this, State, socketUser6, channel14, data24);
					}
					await TimedInvokeAsync<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel>(arg1: new Cacheable<IMessage, ulong>(value3, data24.Id, flag2, async () => await channel14.GetMessageAsync(data24.Id).ConfigureAwait(continueOnCapturedContext: false)), eventHandler: _messageUpdatedEvent, name: "MessageUpdated", arg2: arg46, arg3: channel14).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					Message data6 = (payload as JToken).ToObject<Message>(_serializer);
					ISocketMessageChannel socketMessageChannel = GetChannel(data6.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild2 = (socketMessageChannel as SocketGuildChannel)?.Guild;
					if (!(socketGuild2?.IsSynced ?? true))
					{
						await UnsyncedGuildAsync(type, socketGuild2.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketMessage socketMessage = null;
					if (socketMessageChannel != null)
					{
						socketMessage = SocketChannelHelper.RemoveMessage(socketMessageChannel, this, data6.Id);
					}
					await TimedInvokeAsync(arg1: new Cacheable<IMessage, ulong>(socketMessage, data6.Id, socketMessage != null, () => Task.FromResult<IMessage>(null)), arg2: new Cacheable<IMessageChannel, ulong>(socketMessageChannel, data6.ChannelId, socketMessageChannel != null, async () => (await GetChannelAsync(data6.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel), eventHandler: _messageDeletedEvent, name: "MessageDeleted").ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_REACTION_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_REACTION_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					Discord.API.Gateway.Reaction data5 = (payload as JToken).ToObject<Discord.API.Gateway.Reaction>(_serializer);
					ISocketMessageChannel channel2 = GetChannel(data5.ChannelId) as ISocketMessageChannel;
					SocketUserMessage cachedMsg = channel2?.GetCachedMessage(data5.MessageId) as SocketUserMessage;
					bool isMsgCached = cachedMsg != null;
					IUser user2 = null;
					if (channel2 != null)
					{
						user2 = await channel2.GetUserAsync(data5.UserId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
					}
					Optional<SocketUserMessage> message = ((!isMsgCached) ? Optional.Create<SocketUserMessage>() : Optional.Create(cachedMsg));
					if (data5.Member.IsSpecified)
					{
						SocketGuild socketGuild = (channel2 as SocketGuildChannel)?.Guild;
						if (socketGuild != null)
						{
							user2 = socketGuild.AddOrUpdateUser(data5.Member.Value);
						}
					}
					else
					{
						user2 = GetUser(data5.UserId);
					}
					Optional<IUser> user3 = ((user2 == null) ? Optional.Create<IUser>() : Optional.Create(user2));
					Cacheable<IMessageChannel, ulong> cacheableChannel = new Cacheable<IMessageChannel, ulong>(channel2, data5.ChannelId, channel2 != null, async () => (await GetChannelAsync(data5.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					Cacheable<IUserMessage, ulong> arg8 = new Cacheable<IUserMessage, ulong>(cachedMsg, data5.MessageId, isMsgCached, async () => (await (await cacheableChannel.GetOrDownloadAsync().ConfigureAwait(continueOnCapturedContext: false)).GetMessageAsync(data5.MessageId).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage);
					SocketReaction socketReaction = SocketReaction.Create(data5, channel2, message, user3);
					cachedMsg?.AddReaction(socketReaction);
					await TimedInvokeAsync(_reactionAddedEvent, "ReactionAdded", arg8, cacheableChannel, socketReaction).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_REACTION_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_REACTION_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					Discord.API.Gateway.Reaction data11 = (payload as JToken).ToObject<Discord.API.Gateway.Reaction>(_serializer);
					ISocketMessageChannel channel2 = GetChannel(data11.ChannelId) as ISocketMessageChannel;
					SocketUserMessage cachedMsg = channel2?.GetCachedMessage(data11.MessageId) as SocketUserMessage;
					bool isMsgCached = cachedMsg != null;
					IUser user7 = null;
					if (channel2 != null)
					{
						user7 = await channel2.GetUserAsync(data11.UserId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
					}
					else if (!data11.GuildId.IsSpecified)
					{
						user7 = GetUser(data11.UserId);
					}
					Optional<SocketUserMessage> message3 = ((!isMsgCached) ? Optional.Create<SocketUserMessage>() : Optional.Create(cachedMsg));
					Optional<IUser> user8 = ((user7 == null) ? Optional.Create<IUser>() : Optional.Create(user7));
					Cacheable<IMessageChannel, ulong> cacheableChannel3 = new Cacheable<IMessageChannel, ulong>(channel2, data11.ChannelId, channel2 != null, async () => (await GetChannelAsync(data11.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					Cacheable<IUserMessage, ulong> arg24 = new Cacheable<IUserMessage, ulong>(cachedMsg, data11.MessageId, isMsgCached, async () => (await (await cacheableChannel3.GetOrDownloadAsync().ConfigureAwait(continueOnCapturedContext: false)).GetMessageAsync(data11.MessageId).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage);
					SocketReaction socketReaction2 = SocketReaction.Create(data11, channel2, message3, user8);
					cachedMsg?.RemoveReaction(socketReaction2);
					await TimedInvokeAsync(_reactionRemovedEvent, "ReactionRemoved", arg24, cacheableChannel3, socketReaction2).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_REACTION_REMOVE_ALL":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_REACTION_REMOVE_ALL)").ConfigureAwait(continueOnCapturedContext: false);
					RemoveAllReactionsEvent data19 = (payload as JToken).ToObject<RemoveAllReactionsEvent>(_serializer);
					ISocketMessageChannel socketMessageChannel4 = GetChannel(data19.ChannelId) as ISocketMessageChannel;
					Cacheable<IMessageChannel, ulong> cacheableChannel4 = new Cacheable<IMessageChannel, ulong>(socketMessageChannel4, data19.ChannelId, socketMessageChannel4 != null, async () => (await GetChannelAsync(data19.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					SocketUserMessage socketUserMessage2 = socketMessageChannel4?.GetCachedMessage(data19.MessageId) as SocketUserMessage;
					bool hasValue = socketUserMessage2 != null;
					Cacheable<IUserMessage, ulong> arg37 = new Cacheable<IUserMessage, ulong>(socketUserMessage2, data19.MessageId, hasValue, async () => (await (await cacheableChannel4.GetOrDownloadAsync().ConfigureAwait(continueOnCapturedContext: false)).GetMessageAsync(data19.MessageId).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage);
					socketUserMessage2?.ClearReactions();
					await TimedInvokeAsync(_reactionsClearedEvent, "ReactionsCleared", arg37, cacheableChannel4).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_REACTION_REMOVE_EMOJI":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_REACTION_REMOVE_EMOJI)").ConfigureAwait(continueOnCapturedContext: false);
					RemoveAllReactionsForEmoteEvent data10 = (payload as JToken).ToObject<RemoveAllReactionsForEmoteEvent>(_serializer);
					ISocketMessageChannel socketMessageChannel3 = GetChannel(data10.ChannelId) as ISocketMessageChannel;
					SocketUserMessage socketUserMessage = socketMessageChannel3?.GetCachedMessage(data10.MessageId) as SocketUserMessage;
					bool flag = socketUserMessage != null;
					if (flag)
					{
						Optional.Create(socketUserMessage);
					}
					else
					{
						Optional.Create<SocketUserMessage>();
					}
					Cacheable<IMessageChannel, ulong> cacheableChannel2 = new Cacheable<IMessageChannel, ulong>(socketMessageChannel3, data10.ChannelId, socketMessageChannel3 != null, async () => (await GetChannelAsync(data10.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					Cacheable<IUserMessage, ulong> arg18 = new Cacheable<IUserMessage, ulong>(socketUserMessage, data10.MessageId, flag, async () => (await (await cacheableChannel2.GetOrDownloadAsync().ConfigureAwait(continueOnCapturedContext: false)).GetMessageAsync(data10.MessageId).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage);
					IEmote emote = data10.Emoji.ToIEmote();
					socketUserMessage?.RemoveReactionsForEmote(emote);
					await TimedInvokeAsync(_reactionsRemovedForEmoteEvent, "ReactionsRemovedForEmote", arg18, cacheableChannel2, emote).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_DELETE_BULK":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_DELETE_BULK)").ConfigureAwait(continueOnCapturedContext: false);
					MessageDeleteBulkEvent data22 = (payload as JToken).ToObject<MessageDeleteBulkEvent>(_serializer);
					ISocketMessageChannel socketMessageChannel5 = GetChannel(data22.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild6 = (socketMessageChannel5 as SocketGuildChannel)?.Guild;
					if (!(socketGuild6?.IsSynced ?? true))
					{
						await UnsyncedGuildAsync(type, socketGuild6.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					Cacheable<IMessageChannel, ulong> arg41 = new Cacheable<IMessageChannel, ulong>(socketMessageChannel5, data22.ChannelId, socketMessageChannel5 != null, async () => (await GetChannelAsync(data22.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					List<Cacheable<IMessage, ulong>> list = new List<Cacheable<IMessage, ulong>>(data22.Ids.Length);
					ulong[] ids = data22.Ids;
					foreach (ulong id in ids)
					{
						SocketMessage socketMessage3 = null;
						if (socketMessageChannel5 != null)
						{
							socketMessage3 = SocketChannelHelper.RemoveMessage(socketMessageChannel5, this, id);
						}
						bool hasValue2 = socketMessage3 != null;
						Cacheable<IMessage, ulong> item = new Cacheable<IMessage, ulong>(socketMessage3, id, hasValue2, () => Task.FromResult<IMessage>(null));
						list.Add(item);
					}
					await TimedInvokeAsync(_messagesBulkDeletedEvent, "MessagesBulkDeleted", list, arg41).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "MESSAGE_POLL_VOTE_ADD":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_POLL_VOTE_ADD)").ConfigureAwait(continueOnCapturedContext: false);
					PollVote data17 = (payload as JToken).ToObject<PollVote>(_serializer);
					Cacheable<SocketGuild, RestGuild, IGuild, ulong>? arg33 = null;
					Cacheable<IUser, ulong> arg34;
					Cacheable<ISocketMessageChannel, IRestMessageChannel, IMessageChannel, ulong> arg35;
					Cacheable<IUserMessage, ulong> arg36;
					if (data17.GuildId.IsSpecified)
					{
						SocketGuild guild24 = State.GetGuild(data17.GuildId.Value);
						arg33 = new Cacheable<SocketGuild, RestGuild, IGuild, ulong>(guild24, data17.GuildId.Value, guild24 != null, () => Rest.GetGuildAsync(data17.GuildId.Value));
						if (guild24 != null)
						{
							SocketGuildUser user11 = guild24.GetUser(data17.UserId);
							arg34 = new Cacheable<IUser, ulong>(user11, data17.UserId, user11 != null, async () => await Rest.GetGuildUserAsync(data17.GuildId.Value, data17.UserId));
							SocketTextChannel channel11 = guild24.GetTextChannel(data17.ChannelId);
							arg35 = new Cacheable<ISocketMessageChannel, IRestMessageChannel, IMessageChannel, ulong>(channel11, data17.ChannelId, channel11 != null, async () => (RestTextChannel)(await Rest.GetChannelAsync(data17.ChannelId)));
							IUserMessage userMessage4 = channel11?.GetCachedMessage(data17.MessageId) as IUserMessage;
							arg36 = new Cacheable<IUserMessage, ulong>(userMessage4, data17.MessageId, userMessage4 != null, async delegate
							{
								ITextChannel textChannel = channel11;
								if (textChannel == null)
								{
									textChannel = (ITextChannel)(await Rest.GetChannelAsync(data17.ChannelId));
								}
								return textChannel.GetMessageAsync(data17.MessageId) as IUserMessage;
							});
						}
						else
						{
							arg34 = new Cacheable<IUser, ulong>(null, data17.UserId, hasValue: false, async () => await Rest.GetGuildUserAsync(data17.GuildId.Value, data17.UserId));
							arg35 = new Cacheable<ISocketMessageChannel, IRestMessageChannel, IMessageChannel, ulong>(null, data17.ChannelId, hasValue: false, async () => (RestTextChannel)(await Rest.GetChannelAsync(data17.ChannelId)));
							arg36 = new Cacheable<IUserMessage, ulong>(null, data17.MessageId, hasValue: false, async () => (await ((ITextChannel)(await Rest.GetChannelAsync(data17.ChannelId))).GetMessageAsync(data17.MessageId)) as IUserMessage);
						}
					}
					else
					{
						SocketGlobalUser user12 = State.GetUser(data17.UserId);
						arg34 = new Cacheable<IUser, ulong>(user12, data17.UserId, user12 != null, async () => await GetUserAsync(data17.UserId));
						ISocketMessageChannel channel12 = State.GetChannel(data17.ChannelId) as ISocketMessageChannel;
						arg35 = new Cacheable<ISocketMessageChannel, IRestMessageChannel, IMessageChannel, ulong>(channel12, data17.ChannelId, channel12 != null, async () => (await Rest.GetDMChannelAsync(data17.ChannelId)) as IRestMessageChannel);
						IUserMessage userMessage5 = channel12?.GetCachedMessage(data17.MessageId) as IUserMessage;
						arg36 = new Cacheable<IUserMessage, ulong>(userMessage5, data17.MessageId, userMessage5 != null, async delegate
						{
							IMessageChannel messageChannel = channel12;
							if (messageChannel == null)
							{
								messageChannel = await Rest.GetDMChannelAsync(data17.ChannelId);
							}
							return (await messageChannel.GetMessageAsync(data17.MessageId)) as IUserMessage;
						});
					}
					await TimedInvokeAsync(_pollVoteAdded, "PollVoteAdded", arg34, arg35, arg36, arg33, data17.AnswerId);
					break;
				}
				case "MESSAGE_POLL_VOTE_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (MESSAGE_POLL_VOTE_REMOVE)").ConfigureAwait(continueOnCapturedContext: false);
					PollVote data7 = (payload as JToken).ToObject<PollVote>(_serializer);
					Cacheable<SocketGuild, RestGuild, IGuild, ulong>? arg10 = null;
					Cacheable<IUser, ulong> arg11;
					Cacheable<ISocketMessageChannel, IRestMessageChannel, IMessageChannel, ulong> arg12;
					Cacheable<IUserMessage, ulong> arg13;
					if (data7.GuildId.IsSpecified)
					{
						SocketGuild guild6 = State.GetGuild(data7.GuildId.Value);
						arg10 = new Cacheable<SocketGuild, RestGuild, IGuild, ulong>(guild6, data7.GuildId.Value, guild6 != null, () => Rest.GetGuildAsync(data7.GuildId.Value));
						if (guild6 != null)
						{
							SocketGuildUser user5 = guild6.GetUser(data7.UserId);
							arg11 = new Cacheable<IUser, ulong>(user5, data7.UserId, user5 != null, async () => await Rest.GetGuildUserAsync(data7.GuildId.Value, data7.UserId));
							SocketTextChannel channel3 = guild6.GetTextChannel(data7.ChannelId);
							arg12 = new Cacheable<ISocketMessageChannel, IRestMessageChannel, IMessageChannel, ulong>(channel3, data7.ChannelId, channel3 != null, async () => (RestTextChannel)(await Rest.GetChannelAsync(data7.ChannelId)));
							IUserMessage userMessage = channel3?.GetCachedMessage(data7.MessageId) as IUserMessage;
							arg13 = new Cacheable<IUserMessage, ulong>(userMessage, data7.MessageId, userMessage != null, async delegate
							{
								ITextChannel textChannel = channel3;
								if (textChannel == null)
								{
									textChannel = (ITextChannel)(await Rest.GetChannelAsync(data7.ChannelId));
								}
								return textChannel.GetMessageAsync(data7.MessageId) as IUserMessage;
							});
						}
						else
						{
							arg11 = new Cacheable<IUser, ulong>(null, data7.UserId, hasValue: false, async () => await Rest.GetGuildUserAsync(data7.GuildId.Value, data7.UserId));
							arg12 = new Cacheable<ISocketMessageChannel, IRestMessageChannel, IMessageChannel, ulong>(null, data7.ChannelId, hasValue: false, async () => (RestTextChannel)(await Rest.GetChannelAsync(data7.ChannelId)));
							arg13 = new Cacheable<IUserMessage, ulong>(null, data7.MessageId, hasValue: false, async () => (await ((ITextChannel)(await Rest.GetChannelAsync(data7.ChannelId))).GetMessageAsync(data7.MessageId)) as IUserMessage);
						}
					}
					else
					{
						SocketGlobalUser user6 = State.GetUser(data7.UserId);
						arg11 = new Cacheable<IUser, ulong>(user6, data7.UserId, user6 != null, async () => await GetUserAsync(data7.UserId));
						ISocketMessageChannel channel4 = State.GetChannel(data7.ChannelId) as ISocketMessageChannel;
						arg12 = new Cacheable<ISocketMessageChannel, IRestMessageChannel, IMessageChannel, ulong>(channel4, data7.ChannelId, channel4 != null, async () => (await Rest.GetDMChannelAsync(data7.ChannelId)) as IRestMessageChannel);
						IUserMessage userMessage2 = channel4?.GetCachedMessage(data7.MessageId) as IUserMessage;
						arg13 = new Cacheable<IUserMessage, ulong>(userMessage2, data7.MessageId, userMessage2 != null, async delegate
						{
							IMessageChannel messageChannel = channel4;
							if (messageChannel == null)
							{
								messageChannel = await Rest.GetDMChannelAsync(data7.ChannelId);
							}
							return (await messageChannel.GetMessageAsync(data7.MessageId)) as IUserMessage;
						});
					}
					await TimedInvokeAsync(_pollVoteRemoved, "PollVoteRemoved", arg11, arg12, arg13, arg10, data7.AnswerId);
					break;
				}
				case "PRESENCE_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (PRESENCE_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Presence data20 = (payload as JToken).ToObject<Presence>(_serializer);
					SocketUser user13;
					if (data20.GuildId.IsSpecified)
					{
						SocketGuild guild25 = State.GetGuild(data20.GuildId.Value);
						if (guild25 == null)
						{
							await UnknownGuildAsync(type, data20.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (!guild25.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild25.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						user13 = guild25.GetUser(data20.User.Id);
						if (user13 == null)
						{
							if (data20.Status == UserStatus.Offline)
							{
								return;
							}
							user13 = guild25.AddOrUpdateUser(data20);
						}
						else
						{
							SocketGlobalUser arg39 = user13.GlobalUser.Clone();
							if (user13.GlobalUser.Update(State, data20.User))
							{
								await TimedInvokeAsync(_userUpdatedEvent, "UserUpdated", arg39, user13).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
					}
					else
					{
						user13 = State.GetUser(data20.User.Id);
						if (user13 == null)
						{
							await UnknownGlobalUserAsync(type, data20.User.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					SocketPresence arg40 = user13.Presence?.Clone();
					user13.Update(State, data20.User);
					user13.Update(data20);
					await TimedInvokeAsync(_presenceUpdated, "PresenceUpdated", user13, arg40, user13.Presence).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "TYPING_START":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (TYPING_START)").ConfigureAwait(continueOnCapturedContext: false);
					TypingStartEvent data26 = (payload as JToken).ToObject<TypingStartEvent>(_serializer);
					ISocketMessageChannel socketMessageChannel6 = GetChannel(data26.ChannelId) as ISocketMessageChannel;
					SocketGuild socketGuild8 = (socketMessageChannel6 as SocketGuildChannel)?.Guild;
					if (!(socketGuild8?.IsSynced ?? true))
					{
						await UnsyncedGuildAsync(type, socketGuild8.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					Cacheable<IMessageChannel, ulong> arg48 = new Cacheable<IMessageChannel, ulong>(socketMessageChannel6, data26.ChannelId, socketMessageChannel6 != null, async () => (await GetChannelAsync(data26.ChannelId).ConfigureAwait(continueOnCapturedContext: false)) as IMessageChannel);
					SocketUser socketUser7 = (socketMessageChannel6 as SocketChannel)?.GetUser(data26.UserId);
					if (socketUser7 == null && socketGuild8 != null && data26.Member.IsSpecified)
					{
						socketUser7 = socketGuild8.AddOrUpdateUser(data26.Member.Value);
					}
					await TimedInvokeAsync<Cacheable<IUser, ulong>, Cacheable<IMessageChannel, ulong>>(arg1: new Cacheable<IUser, ulong>(socketUser7, data26.UserId, socketUser7 != null, async () => await GetUserAsync(data26.UserId).ConfigureAwait(continueOnCapturedContext: false)), eventHandler: _userIsTypingEvent, name: "UserIsTyping", arg2: arg48).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "INTEGRATION_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INTEGRATION_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Integration integration2 = (payload as JToken).ToObject<Integration>(_serializer);
					if (!integration2.GuildId.IsSpecified)
					{
						return;
					}
					SocketGuild guild36 = State.GetGuild(integration2.GuildId.Value);
					if (guild36 != null)
					{
						if (!guild36.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild36.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_integrationCreated, "IntegrationCreated", RestIntegration.Create(this, guild36, integration2)).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, integration2.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "INTEGRATION_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INTEGRATION_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Integration integration = (payload as JToken).ToObject<Integration>(_serializer);
					if (!integration.GuildId.IsSpecified)
					{
						return;
					}
					SocketGuild guild35 = State.GetGuild(integration.GuildId.Value);
					if (guild35 != null)
					{
						if (!guild35.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild35.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_integrationUpdated, "IntegrationUpdated", RestIntegration.Create(this, guild35, integration)).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, integration.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "INTEGRATION_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INTEGRATION_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					IntegrationDeletedEvent integrationDeletedEvent = (payload as JToken).ToObject<IntegrationDeletedEvent>(_serializer);
					SocketGuild guild20 = State.GetGuild(integrationDeletedEvent.GuildId);
					if (guild20 != null)
					{
						if (!guild20.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild20.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_integrationDeleted, "IntegrationDeleted", guild20, integrationDeletedEvent.Id, integrationDeletedEvent.ApplicationID).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownGuildAsync(type, integrationDeletedEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "USER_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (USER_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					User user14 = (payload as JToken).ToObject<User>(_serializer);
					if (user14.Id == CurrentUser.Id)
					{
						SocketSelfUser arg38 = CurrentUser.Clone();
						CurrentUser.Update(State, user14);
						await TimedInvokeAsync(_selfUpdatedEvent, "CurrentUserUpdated", arg38, CurrentUser).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await _gatewayLogger.WarningAsync("Received USER_UPDATE for wrong user.").ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "VOICE_STATE_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (VOICE_STATE_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					VoiceState data18 = (payload as JToken).ToObject<VoiceState>(_serializer);
					SocketVoiceState before;
					SocketVoiceState after;
					SocketUser user13;
					if (data18.GuildId.HasValue)
					{
						SocketGuild guild3 = State.GetGuild(data18.GuildId.Value);
						if (guild3 == null)
						{
							await UnknownGuildAsync(type, data18.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (!guild3.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild3.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (data18.ChannelId.HasValue)
						{
							before = guild3.GetVoiceState(data18.UserId)?.Clone() ?? SocketVoiceState.Default;
							after = await guild3.AddOrUpdateVoiceStateAsync(State, data18).ConfigureAwait(continueOnCapturedContext: false);
						}
						else
						{
							before = (await guild3.RemoveVoiceStateAsync(data18.UserId).ConfigureAwait(continueOnCapturedContext: false)) ?? SocketVoiceState.Default;
							after = SocketVoiceState.Create(null, data18);
						}
						user13 = guild3.GetUser(data18.UserId) ?? (data18.Member.IsSpecified ? guild3.AddOrUpdateUser(data18.Member.Value) : null);
						if (user13 == null)
						{
							await UnknownGuildUserAsync(type, data18.UserId, guild3.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					else
					{
						if (!(GetChannel(data18.ChannelId.Value) is SocketGroupChannel socketGroupChannel2))
						{
							await UnknownChannelAsync(type, data18.ChannelId.Value).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						if (data18.ChannelId.HasValue)
						{
							before = socketGroupChannel2.GetVoiceState(data18.UserId)?.Clone() ?? SocketVoiceState.Default;
							after = socketGroupChannel2.AddOrUpdateVoiceState(State, data18);
						}
						else
						{
							before = socketGroupChannel2.RemoveVoiceState(data18.UserId) ?? SocketVoiceState.Default;
							after = SocketVoiceState.Create(null, data18);
						}
						user13 = socketGroupChannel2.GetUser(data18.UserId);
						if (user13 == null)
						{
							await UnknownChannelUserAsync(type, data18.UserId, socketGroupChannel2.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
					}
					if (user13 is SocketGuildUser socketGuildUser && data18.ChannelId.HasValue)
					{
						SocketStageChannel stageChannel = socketGuildUser.Guild.GetStageChannel(data18.ChannelId.Value);
						if (stageChannel != null && before.VoiceChannel != null && after.VoiceChannel != null)
						{
							if (!before.RequestToSpeakTimestamp.HasValue && after.RequestToSpeakTimestamp.HasValue)
							{
								await TimedInvokeAsync(_requestToSpeak, "RequestToSpeak", stageChannel, socketGuildUser);
								return;
							}
							if (before.IsSuppressed && !after.IsSuppressed)
							{
								await TimedInvokeAsync(_speakerAdded, "SpeakerAdded", stageChannel, socketGuildUser);
								return;
							}
							if (!before.IsSuppressed && after.IsSuppressed)
							{
								await TimedInvokeAsync(_speakerRemoved, "SpeakerRemoved", stageChannel, socketGuildUser);
							}
						}
					}
					await TimedInvokeAsync(_userVoiceStateUpdatedEvent, "UserVoiceStateUpdated", user13, before, after).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "VOICE_SERVER_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (VOICE_SERVER_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					VoiceServerUpdateEvent data13 = (payload as JToken).ToObject<VoiceServerUpdateEvent>(_serializer);
					SocketGuild guild3 = State.GetGuild(data13.GuildId);
					bool isMsgCached = guild3 != null;
					SocketVoiceServer arg29 = new SocketVoiceServer(new Cacheable<IGuild, ulong>(guild3, data13.GuildId, isMsgCached, () => Task.FromResult((IGuild)State.GetGuild(data13.GuildId))), data13.Endpoint, data13.Token);
					await TimedInvokeAsync(_voiceServerUpdatedEvent, "UserVoiceStateUpdated", arg29).ConfigureAwait(continueOnCapturedContext: false);
					if (isMsgCached)
					{
						string text2 = data13.Endpoint;
						int num3 = text2.LastIndexOf(':');
						if (num3 > 0)
						{
							text2 = text2.Substring(0, num3);
						}
						guild3.FinishConnectAudio(text2, data13.Token).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						await UnknownGuildAsync(type, data13.GuildId).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "VOICE_CHANNEL_STATUS_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (VOICE_CHANNEL_STATUS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					VoiceChannelStatusUpdateEvent voiceChannelStatusUpdateEvent = (payload as JToken).ToObject<VoiceChannelStatusUpdateEvent>(_serializer);
					State.GetGuild(voiceChannelStatusUpdateEvent.GuildId);
					SocketVoiceChannel socketVoiceChannel = State.GetChannel(voiceChannelStatusUpdateEvent.Id) as SocketVoiceChannel;
					Cacheable<SocketVoiceChannel, ulong> arg14 = new Cacheable<SocketVoiceChannel, ulong>(socketVoiceChannel, voiceChannelStatusUpdateEvent.Id, socketVoiceChannel != null, () => (Task<SocketVoiceChannel>)null);
					string arg15 = (string)socketVoiceChannel?.Status?.Clone();
					string status = voiceChannelStatusUpdateEvent.Status;
					socketVoiceChannel?.UpdateVoiceStatus(voiceChannelStatusUpdateEvent.Status);
					await TimedInvokeAsync(_voiceChannelStatusUpdated, "VoiceChannelStatusUpdated", arg14, arg15, status);
					break;
				}
				case "INVITE_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INVITE_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					InviteCreateEvent inviteCreateEvent = (payload as JToken).ToObject<InviteCreateEvent>(_serializer);
					if (State.GetChannel(inviteCreateEvent.ChannelId) is SocketGuildChannel { Guild: var guild34 } socketGuildChannel2)
					{
						if (!guild34.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild34.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						SocketGuildUser inviter = (inviteCreateEvent.Inviter.IsSpecified ? (guild34.GetUser(inviteCreateEvent.Inviter.Value.Id) ?? guild34.AddOrUpdateUser(inviteCreateEvent.Inviter.Value)) : null);
						SocketUser target = (SocketUser)(inviteCreateEvent.TargetUser.IsSpecified ? (((object)guild34.GetUser(inviteCreateEvent.TargetUser.Value.Id)) ?? ((object)SocketUnknownUser.Create(this, State, inviteCreateEvent.TargetUser.Value))) : null);
						SocketInvite arg45 = SocketInvite.Create(this, guild34, socketGuildChannel2, inviter, target, inviteCreateEvent);
						await TimedInvokeAsync(_inviteCreatedEvent, "InviteCreated", arg45).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, inviteCreateEvent.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "INVITE_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INVITE_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					InviteDeleteEvent inviteDeleteEvent = (payload as JToken).ToObject<InviteDeleteEvent>(_serializer);
					if (State.GetChannel(inviteDeleteEvent.ChannelId) is SocketGuildChannel { Guild: var guild29 } socketGuildChannel)
					{
						if (!guild29.IsSynced)
						{
							await UnsyncedGuildAsync(type, guild29.Id).ConfigureAwait(continueOnCapturedContext: false);
							return;
						}
						await TimedInvokeAsync(_inviteDeletedEvent, "InviteDeleted", socketGuildChannel, inviteDeleteEvent.Code).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					await UnknownChannelAsync(type, inviteDeleteEvent.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				case "INTERACTION_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (INTERACTION_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Interaction data3 = (payload as JToken).ToObject<Interaction>(_serializer);
					SocketGuild guild3 = (data3.GuildId.IsSpecified ? GetGuild(data3.GuildId.Value) : null);
					if (guild3 != null && !guild3.IsSynced)
					{
						await UnsyncedGuildAsync(type, guild3.Id).ConfigureAwait(continueOnCapturedContext: false);
					}
					SocketUser user = (data3.User.IsSpecified ? State.GetOrAddUser(data3.User.Value.Id, (ulong _) => SocketGlobalUser.Create(this, State, data3.User.Value)) : ((guild3 != null) ? ((SocketUser)guild3.AddOrUpdateUser(data3.Member.Value)) : ((SocketUser)State.GetOrAddUser(data3.Member.Value.User.Id, (ulong _) => SocketGlobalUser.Create(this, State, data3.Member.Value.User)))));
					SocketChannel socketChannel = null;
					if (data3.ChannelId.IsSpecified)
					{
						socketChannel = State.GetChannel(data3.ChannelId.Value);
						if (socketChannel == null && !data3.GuildId.IsSpecified)
						{
							socketChannel = CreateDMChannel(data3.ChannelId.Value, user, State);
						}
					}
					else if (data3.User.IsSpecified)
					{
						socketChannel = State.GetDMChannel(data3.User.Value.Id);
					}
					SocketInteraction interaction = SocketInteraction.Create(this, data3, socketChannel as ISocketMessageChannel, user);
					await TimedInvokeAsync(_interactionCreatedEvent, "InteractionCreated", interaction).ConfigureAwait(continueOnCapturedContext: false);
					if (!(interaction is SocketSlashCommand arg3))
					{
						if (!(interaction is SocketMessageComponent messageComponent))
						{
							if (!(interaction is SocketUserCommand arg4))
							{
								if (!(interaction is SocketMessageCommand arg5))
								{
									if (!(interaction is SocketAutocompleteInteraction arg6))
									{
										if (interaction is SocketModal arg7)
										{
											await TimedInvokeAsync(_modalSubmitted, "ModalSubmitted", arg7).ConfigureAwait(continueOnCapturedContext: false);
										}
									}
									else
									{
										await TimedInvokeAsync(_autocompleteExecuted, "AutocompleteExecuted", arg6).ConfigureAwait(continueOnCapturedContext: false);
									}
								}
								else
								{
									await TimedInvokeAsync(_messageCommandExecuted, "MessageCommandExecuted", arg5).ConfigureAwait(continueOnCapturedContext: false);
								}
							}
							else
							{
								await TimedInvokeAsync(_userCommandExecuted, "UserCommandExecuted", arg4).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
						else
						{
							if (messageComponent.Data.Type.IsSelectType())
							{
								await TimedInvokeAsync(_selectMenuExecuted, "SelectMenuExecuted", messageComponent).ConfigureAwait(continueOnCapturedContext: false);
							}
							if (messageComponent.Data.Type == ComponentType.Button)
							{
								await TimedInvokeAsync(_buttonExecuted, "ButtonExecuted", messageComponent).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
					}
					else
					{
						await TimedInvokeAsync(_slashCommandExecuted, "SlashCommandExecuted", arg3).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "APPLICATION_COMMAND_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (APPLICATION_COMMAND_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					ApplicationCommandCreatedUpdatedEvent applicationCommandCreatedUpdatedEvent3 = (payload as JToken).ToObject<ApplicationCommandCreatedUpdatedEvent>(_serializer);
					if (applicationCommandCreatedUpdatedEvent3.GuildId.IsSpecified && State.GetGuild(applicationCommandCreatedUpdatedEvent3.GuildId.Value) == null)
					{
						await UnknownGuildAsync(type, applicationCommandCreatedUpdatedEvent3.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketApplicationCommand socketApplicationCommand3 = SocketApplicationCommand.Create(this, applicationCommandCreatedUpdatedEvent3);
					State.AddCommand(socketApplicationCommand3);
					await TimedInvokeAsync(_applicationCommandCreated, "ApplicationCommandCreated", socketApplicationCommand3).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "APPLICATION_COMMAND_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (APPLICATION_COMMAND_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					ApplicationCommandCreatedUpdatedEvent applicationCommandCreatedUpdatedEvent2 = (payload as JToken).ToObject<ApplicationCommandCreatedUpdatedEvent>(_serializer);
					if (applicationCommandCreatedUpdatedEvent2.GuildId.IsSpecified && State.GetGuild(applicationCommandCreatedUpdatedEvent2.GuildId.Value) == null)
					{
						await UnknownGuildAsync(type, applicationCommandCreatedUpdatedEvent2.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketApplicationCommand socketApplicationCommand2 = SocketApplicationCommand.Create(this, applicationCommandCreatedUpdatedEvent2);
					State.AddCommand(socketApplicationCommand2);
					await TimedInvokeAsync(_applicationCommandUpdated, "ApplicationCommandUpdated", socketApplicationCommand2).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "APPLICATION_COMMAND_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (APPLICATION_COMMAND_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					ApplicationCommandCreatedUpdatedEvent applicationCommandCreatedUpdatedEvent = (payload as JToken).ToObject<ApplicationCommandCreatedUpdatedEvent>(_serializer);
					if (applicationCommandCreatedUpdatedEvent.GuildId.IsSpecified && State.GetGuild(applicationCommandCreatedUpdatedEvent.GuildId.Value) == null)
					{
						await UnknownGuildAsync(type, applicationCommandCreatedUpdatedEvent.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketApplicationCommand socketApplicationCommand = SocketApplicationCommand.Create(this, applicationCommandCreatedUpdatedEvent);
					State.RemoveCommand(socketApplicationCommand.Id);
					await TimedInvokeAsync(_applicationCommandDeleted, "ApplicationCommandDeleted", socketApplicationCommand).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "THREAD_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel data21 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketGuild guild26 = State.GetGuild(data21.GuildId.Value);
					if (guild26 == null)
					{
						await UnknownGuildAsync(type, data21.GuildId.Value);
						return;
					}
					SocketThreadChannel socketThreadChannel4;
					if ((socketThreadChannel4 = guild26.ThreadChannels.FirstOrDefault((SocketThreadChannel x) => x.Id == data21.Id)) != null)
					{
						socketThreadChannel4.Update(State, data21);
						if (data21.ThreadMember.IsSpecified)
						{
							socketThreadChannel4.AddOrUpdateThreadMember(data21.ThreadMember.Value, guild26.CurrentUser);
						}
					}
					else
					{
						socketThreadChannel4 = (SocketThreadChannel)guild26.AddChannel(State, data21);
						if (data21.ThreadMember.IsSpecified)
						{
							socketThreadChannel4.AddOrUpdateThreadMember(data21.ThreadMember.Value, guild26.CurrentUser);
						}
					}
					await TimedInvokeAsync(_threadCreated, "ThreadCreated", socketThreadChannel4).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "THREAD_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel data9 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketGuild guild9 = State.GetGuild(data9.GuildId.Value);
					if (guild9 == null)
					{
						await UnknownGuildAsync(type, data9.GuildId.Value);
						return;
					}
					SocketThreadChannel socketThreadChannel3 = guild9.ThreadChannels.FirstOrDefault((SocketThreadChannel x) => x.Id == data9.Id);
					Cacheable<SocketThreadChannel, ulong> arg17 = ((socketThreadChannel3 != null) ? new Cacheable<SocketThreadChannel, ulong>(socketThreadChannel3.Clone(), data9.Id, hasValue: true, () => Task.FromResult<SocketThreadChannel>(null)) : new Cacheable<SocketThreadChannel, ulong>(null, data9.Id, hasValue: false, () => Task.FromResult<SocketThreadChannel>(null)));
					if (socketThreadChannel3 != null)
					{
						socketThreadChannel3.Update(State, data9);
						if (data9.ThreadMember.IsSpecified)
						{
							socketThreadChannel3.AddOrUpdateThreadMember(data9.ThreadMember.Value, guild9.CurrentUser);
						}
					}
					else
					{
						socketThreadChannel3 = (SocketThreadChannel)guild9.AddChannel(State, data9);
						if (data9.ThreadMember.IsSpecified)
						{
							socketThreadChannel3.AddOrUpdateThreadMember(data9.ThreadMember.Value, guild9.CurrentUser);
						}
					}
					if (!(guild9?.IsSynced ?? true))
					{
						await UnsyncedGuildAsync(type, guild9.Id).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					await TimedInvokeAsync(_threadUpdated, "ThreadUpdated", arg17, socketThreadChannel3).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "THREAD_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					Channel channel5 = (payload as JToken).ToObject<Channel>(_serializer);
					SocketGuild guild8 = State.GetGuild(channel5.GuildId.Value);
					if (guild8 == null)
					{
						await UnknownGuildAsync(type, channel5.GuildId.Value).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketThreadChannel socketThreadChannel2 = (SocketThreadChannel)guild8.RemoveChannel(State, channel5.Id);
					await TimedInvokeAsync(arg: new Cacheable<SocketThreadChannel, ulong>(socketThreadChannel2, channel5.Id, socketThreadChannel2 != null, null), eventHandler: _threadDeleted, name: "ThreadDeleted").ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "THREAD_LIST_SYNC":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_LIST_SYNC)").ConfigureAwait(continueOnCapturedContext: false);
					ThreadListSyncEvent threadListSyncEvent = (payload as JToken).ToObject<ThreadListSyncEvent>(_serializer);
					SocketGuild guild5 = State.GetGuild(threadListSyncEvent.GuildId);
					if (guild5 == null)
					{
						await UnknownGuildAsync(type, threadListSyncEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					Channel[] threads = threadListSyncEvent.Threads;
					foreach (Channel thread in threads)
					{
						SocketThreadChannel entity = guild5.ThreadChannels.FirstOrDefault((SocketThreadChannel x) => x.Id == thread.Id);
						if (entity == null)
						{
							entity = (SocketThreadChannel)guild5.AddChannel(State, thread);
						}
						else
						{
							entity.Update(State, thread);
						}
						foreach (ThreadMember item4 in threadListSyncEvent.Members.Where((ThreadMember x) => x.Id.Value == entity.Id))
						{
							SocketGuildUser user4 = guild5.GetUser(item4.Id.Value);
							entity.AddOrUpdateThreadMember(item4, user4);
						}
					}
					break;
				}
				case "THREAD_MEMBER_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_MEMBER_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					ThreadMember threadMember = (payload as JToken).ToObject<ThreadMember>(_serializer);
					SocketThreadChannel socketThreadChannel = (SocketThreadChannel)State.GetChannel(threadMember.Id.Value);
					if (socketThreadChannel == null)
					{
						await UnknownChannelAsync(type, threadMember.Id.Value);
						return;
					}
					socketThreadChannel.AddOrUpdateThreadMember(threadMember, socketThreadChannel.Guild.CurrentUser);
					break;
				}
				case "THREAD_MEMBERS_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (THREAD_MEMBERS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					ThreadMembersUpdated threadMembersUpdated = (payload as JToken).ToObject<ThreadMembersUpdated>(_serializer);
					SocketGuild guild3 = State.GetGuild(threadMembersUpdated.GuildId);
					if (guild3 == null)
					{
						await UnknownGuildAsync(type, threadMembersUpdated.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketThreadChannel thread2 = (SocketThreadChannel)guild3.GetChannel(threadMembersUpdated.Id);
					if (thread2 == null)
					{
						await UnknownChannelAsync(type, threadMembersUpdated.Id);
						return;
					}
					IReadOnlyCollection<SocketThreadUser> leftUsers = null;
					IReadOnlyCollection<SocketThreadUser> joinUsers = null;
					if (threadMembersUpdated.RemovedMemberIds.IsSpecified)
					{
						leftUsers = thread2.RemoveUsers(threadMembersUpdated.RemovedMemberIds.Value);
					}
					if (threadMembersUpdated.AddedMembers.IsSpecified)
					{
						List<SocketThreadUser> newThreadMembers = new List<SocketThreadUser>();
						ThreadMember[] value2 = threadMembersUpdated.AddedMembers.Value;
						foreach (ThreadMember threadMember2 in value2)
						{
							SocketGuildUser user16 = guild3.GetUser(threadMember2.UserId.Value);
							if (user16 == null)
							{
								await UnknownGuildUserAsync("THREAD_MEMBERS_UPDATE", threadMember2.UserId.Value, guild3.Id);
							}
							else
							{
								newThreadMembers.Add(thread2.AddOrUpdateThreadMember(threadMember2, user16));
							}
						}
						if (newThreadMembers.Any())
						{
							joinUsers = newThreadMembers.ToImmutableArray();
						}
					}
					if (leftUsers != null)
					{
						foreach (SocketThreadUser item5 in leftUsers)
						{
							await TimedInvokeAsync(_threadMemberLeft, "ThreadMemberLeft", item5).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					if (joinUsers == null)
					{
						break;
					}
					foreach (SocketThreadUser item6 in joinUsers)
					{
						await TimedInvokeAsync(_threadMemberJoined, "ThreadMemberJoined", item6).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "STAGE_INSTANCE_CREATE":
				case "STAGE_INSTANCE_UPDATE":
				case "STAGE_INSTANCE_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					StageInstance stageInstance = (payload as JToken).ToObject<StageInstance>(_serializer);
					SocketGuild guild27 = State.GetGuild(stageInstance.GuildId);
					if (guild27 == null)
					{
						await UnknownGuildAsync(type, stageInstance.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketStageChannel stageChannel2 = guild27.GetStageChannel(stageInstance.ChannelId);
					if (stageChannel2 == null)
					{
						await UnknownChannelAsync(type, stageInstance.ChannelId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketStageChannel arg42 = ((type == "STAGE_INSTANCE_UPDATE") ? stageChannel2.Clone() : null);
					stageChannel2.Update(stageInstance, type == "STAGE_INSTANCE_CREATE");
					switch (type)
					{
					case "STAGE_INSTANCE_CREATE":
						await TimedInvokeAsync(_stageStarted, "StageStarted", stageChannel2).ConfigureAwait(continueOnCapturedContext: false);
						return;
					case "STAGE_INSTANCE_DELETE":
						await TimedInvokeAsync(_stageEnded, "StageEnded", stageChannel2).ConfigureAwait(continueOnCapturedContext: false);
						return;
					case "STAGE_INSTANCE_UPDATE":
						await TimedInvokeAsync(_stageUpdated, "StageUpdated", arg42, stageChannel2).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					break;
				}
				case "GUILD_SCHEDULED_EVENT_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					GuildScheduledEvent guildScheduledEvent3 = (payload as JToken).ToObject<GuildScheduledEvent>(_serializer);
					SocketGuild guild28 = State.GetGuild(guildScheduledEvent3.GuildId);
					if (guild28 == null)
					{
						await UnknownGuildAsync(type, guildScheduledEvent3.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildEvent arg43 = guild28.AddOrUpdateEvent(guildScheduledEvent3);
					await TimedInvokeAsync(_guildScheduledEventCreated, "GuildScheduledEventCreated", arg43).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "GUILD_SCHEDULED_EVENT_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					GuildScheduledEvent guildScheduledEvent2 = (payload as JToken).ToObject<GuildScheduledEvent>(_serializer);
					SocketGuild guild12 = State.GetGuild(guildScheduledEvent2.GuildId);
					if (guild12 == null)
					{
						await UnknownGuildAsync(type, guildScheduledEvent2.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildEvent socketGuildEvent2 = guild12.GetEvent(guildScheduledEvent2.Id)?.Clone();
					Cacheable<SocketGuildEvent, ulong> arg20 = new Cacheable<SocketGuildEvent, ulong>(socketGuildEvent2, guildScheduledEvent2.Id, socketGuildEvent2 != null, () => Task.FromResult<SocketGuildEvent>(null));
					SocketGuildEvent socketGuildEvent3 = guild12.AddOrUpdateEvent(guildScheduledEvent2);
					if ((socketGuildEvent2 == null || socketGuildEvent2.Status != GuildScheduledEventStatus.Completed) && guildScheduledEvent2.Status == GuildScheduledEventStatus.Completed)
					{
						await TimedInvokeAsync(_guildScheduledEventCompleted, "GuildScheduledEventCompleted", socketGuildEvent3).ConfigureAwait(continueOnCapturedContext: false);
					}
					else if (socketGuildEvent2 != null && socketGuildEvent2.Status != GuildScheduledEventStatus.Active && guildScheduledEvent2.Status == GuildScheduledEventStatus.Active)
					{
						await TimedInvokeAsync(_guildScheduledEventStarted, "GuildScheduledEventStarted", socketGuildEvent3).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						await TimedInvokeAsync(_guildScheduledEventUpdated, "GuildScheduledEventUpdated", arg20, socketGuildEvent3).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "GUILD_SCHEDULED_EVENT_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					GuildScheduledEvent guildScheduledEvent = (payload as JToken).ToObject<GuildScheduledEvent>(_serializer);
					SocketGuild guild7 = State.GetGuild(guildScheduledEvent.GuildId);
					if (guild7 == null)
					{
						await UnknownGuildAsync(type, guildScheduledEvent.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildEvent arg16 = guild7.RemoveEvent(guildScheduledEvent.Id) ?? SocketGuildEvent.Create(this, guild7, guildScheduledEvent);
					await TimedInvokeAsync(_guildScheduledEventCancelled, "GuildScheduledEventCancelled", arg16).ConfigureAwait(continueOnCapturedContext: false);
					break;
				}
				case "GUILD_SCHEDULED_EVENT_USER_ADD":
				case "GUILD_SCHEDULED_EVENT_USER_REMOVE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					GuildScheduledEventUserAddRemoveEvent data2 = (payload as JToken).ToObject<GuildScheduledEventUserAddRemoveEvent>(_serializer);
					SocketGuild guild2 = State.GetGuild(data2.GuildId);
					if (guild2 == null)
					{
						await UnknownGuildAsync(type, data2.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketGuildEvent socketGuildEvent = guild2.GetEvent(data2.EventId);
					if (socketGuildEvent == null)
					{
						await UnknownGuildEventAsync(type, data2.EventId, data2.GuildId).ConfigureAwait(continueOnCapturedContext: false);
						return;
					}
					SocketUser socketUser = (SocketUser)(((object)guild2.GetUser(data2.UserId)) ?? ((object)State.GetUser(data2.UserId)));
					Cacheable<SocketUser, RestUser, IUser, ulong> arg2 = new Cacheable<SocketUser, RestUser, IUser, ulong>(socketUser, data2.UserId, socketUser != null, () => Rest.GetUserAsync(data2.UserId));
					string text = type;
					if (!(text == "GUILD_SCHEDULED_EVENT_USER_ADD"))
					{
						if (text == "GUILD_SCHEDULED_EVENT_USER_REMOVE")
						{
							await TimedInvokeAsync(_guildScheduledEventUserRemove, "GuildScheduledEventUserRemove", arg2, socketGuildEvent).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					else
					{
						await TimedInvokeAsync(_guildScheduledEventUserAdd, "GuildScheduledEventUserAdd", arg2, socketGuildEvent).ConfigureAwait(continueOnCapturedContext: false);
					}
					break;
				}
				case "WEBHOOKS_UPDATE":
				{
					WebhooksUpdatedEvent data4 = (payload as JToken).ToObject<WebhooksUpdatedEvent>(_serializer);
					type = "WEBHOOKS_UPDATE";
					await _gatewayLogger.DebugAsync("Received Dispatch (WEBHOOKS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					SocketGuild guild4 = State.GetGuild(data4.GuildId);
					SocketChannel channel = State.GetChannel(data4.ChannelId);
					await TimedInvokeAsync(_webhooksUpdated, "WebhooksUpdated", guild4, channel);
					break;
				}
				case "GUILD_AUDIT_LOG_ENTRY_CREATE":
				{
					AuditLogCreatedEvent data = (payload as JToken).ToObject<AuditLogCreatedEvent>(_serializer);
					type = "GUILD_AUDIT_LOG_ENTRY_CREATE";
					await _gatewayLogger.DebugAsync("Received Dispatch (GUILD_AUDIT_LOG_ENTRY_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					SocketGuild guild = State.GetGuild(data.GuildId);
					SocketAuditLogEntry socketAuditLogEntry = SocketAuditLogEntry.Create(this, data);
					guild.AddAuditLog(socketAuditLogEntry);
					await TimedInvokeAsync(_auditLogCreated, "AuditLogCreated", socketAuditLogEntry, guild);
					break;
				}
				case "AUTO_MODERATION_RULE_CREATE":
				{
					AutoModerationRule autoModerationRule = (payload as JToken).ToObject<AutoModerationRule>(_serializer);
					SocketAutoModRule arg = State.GetGuild(autoModerationRule.GuildId).AddOrUpdateAutoModRule(autoModerationRule);
					await TimedInvokeAsync(_autoModRuleCreated, "AutoModRuleCreated", arg);
					break;
				}
				case "AUTO_MODERATION_RULE_UPDATE":
				{
					AutoModerationRule data25 = (payload as JToken).ToObject<AutoModerationRule>(_serializer);
					SocketGuild guild37 = State.GetGuild(data25.GuildId);
					SocketAutoModRule autoModRule2 = guild37.GetAutoModRule(data25.Id);
					await TimedInvokeAsync<Cacheable<SocketAutoModRule, ulong>, SocketAutoModRule>(arg1: new Cacheable<SocketAutoModRule, ulong>(autoModRule2?.Clone(), data25.Id, autoModRule2 != null, async () => await guild37.GetAutoModRuleAsync(data25.Id)), eventHandler: _autoModRuleUpdated, name: "AutoModRuleUpdated", arg2: guild37.AddOrUpdateAutoModRule(data25));
					break;
				}
				case "AUTO_MODERATION_RULE_DELETE":
				{
					AutoModerationRule autoModerationRule2 = (payload as JToken).ToObject<AutoModerationRule>(_serializer);
					SocketAutoModRule arg47 = State.GetGuild(autoModerationRule2.GuildId).RemoveAutoModRule(autoModerationRule2);
					await TimedInvokeAsync(_autoModRuleDeleted, "AutoModRuleDeleted", arg47);
					break;
				}
				case "AUTO_MODERATION_ACTION_EXECUTION":
				{
					AutoModActionExecutedEvent data14 = (payload as JToken).ToObject<AutoModActionExecutedEvent>(_serializer);
					SocketGuild guild19 = State.GetGuild(data14.GuildId);
					AutoModRuleAction arg30 = new AutoModRuleAction(data14.Action.Type, (!data14.Action.Metadata.IsSpecified) ? ((ulong?)null) : (data14.Action.Metadata.Value.ChannelId.IsSpecified ? new ulong?(data14.Action.Metadata.Value.ChannelId.Value) : ((ulong?)null)), (!data14.Action.Metadata.IsSpecified) ? ((int?)null) : (data14.Action.Metadata.Value.DurationSeconds.IsSpecified ? new int?(data14.Action.Metadata.Value.DurationSeconds.Value) : ((int?)null)), (!data14.Action.Metadata.IsSpecified) ? null : (data14.Action.Metadata.Value.CustomMessage.IsSpecified ? data14.Action.Metadata.Value.CustomMessage.Value : null));
					SocketGuildUser user9 = guild19.GetUser(data14.UserId);
					Cacheable<SocketGuildUser, ulong> user10 = new Cacheable<SocketGuildUser, ulong>(user9, data14.UserId, user9 != null, async delegate
					{
						GuildMember model3 = await ApiClient.GetGuildMemberAsync(data14.GuildId, data14.UserId);
						return guild19.AddOrUpdateUser(model3);
					});
					ISocketMessageChannel channel9 = null;
					if (data14.ChannelId.IsSpecified)
					{
						channel9 = GetChannel(data14.ChannelId.Value) as ISocketMessageChannel;
					}
					Cacheable<ISocketMessageChannel, ulong> channel10 = new Cacheable<ISocketMessageChannel, ulong>(channel9, data14.ChannelId.GetValueOrDefault(0uL), channel9 != null, async () => data14.ChannelId.IsSpecified ? ((await GetChannelAsync(data14.ChannelId.Value).ConfigureAwait(continueOnCapturedContext: false)) as ISocketMessageChannel) : null);
					IUserMessage userMessage3 = null;
					if (data14.MessageId.IsSpecified)
					{
						userMessage3 = channel9?.GetCachedMessage(data14.MessageId.GetValueOrDefault(0uL)) as IUserMessage;
					}
					Cacheable<IUserMessage, ulong> value = new Cacheable<IUserMessage, ulong>(userMessage3, data14.MessageId.GetValueOrDefault(0uL), userMessage3 != null, async () => data14.MessageId.IsSpecified ? ((await channel9.GetMessageAsync(data14.MessageId.Value).ConfigureAwait(continueOnCapturedContext: false)) as IUserMessage) : null);
					SocketAutoModRule autoModRule = guild19.GetAutoModRule(data14.RuleId);
					AutoModActionExecutedData arg31 = new AutoModActionExecutedData(new Cacheable<IAutoModRule, ulong>(autoModRule, data14.RuleId, autoModRule != null, async () => await guild19.GetAutoModRuleAsync(data14.RuleId)), data14.TriggerType, user10, channel10, data14.MessageId.IsSpecified ? new Cacheable<IUserMessage, ulong>?(value) : ((Cacheable<IUserMessage, ulong>?)null), data14.AlertSystemMessageId.GetValueOrDefault(0uL), data14.Content, data14.MatchedContent.IsSpecified ? data14.MatchedContent.Value : null, data14.MatchedKeyword.IsSpecified ? data14.MatchedKeyword.Value : null);
					await TimedInvokeAsync(_autoModActionExecuted, "AutoModActionExecuted", guild19, arg30, arg31);
					break;
				}
				case "ENTITLEMENT_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (ENTITLEMENT_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Entitlement entitlement3 = (payload as JToken).ToObject<Entitlement>(_serializer);
					SocketEntitlement socketEntitlement3 = SocketEntitlement.Create(this, entitlement3);
					State.AddEntitlement(entitlement3.Id, socketEntitlement3);
					await TimedInvokeAsync(_entitlementCreated, "EntitlementCreated", socketEntitlement3);
					break;
				}
				case "ENTITLEMENT_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (ENTITLEMENT_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Entitlement entitlement2 = (payload as JToken).ToObject<Entitlement>(_serializer);
					SocketEntitlement socketEntitlement2 = State.GetEntitlement(entitlement2.Id);
					Cacheable<SocketEntitlement, ulong> arg21 = new Cacheable<SocketEntitlement, ulong>(socketEntitlement2?.Clone(), entitlement2.Id, socketEntitlement2 != null, () => (Task<SocketEntitlement>)null);
					if (socketEntitlement2 == null)
					{
						socketEntitlement2 = SocketEntitlement.Create(this, entitlement2);
						State.AddEntitlement(entitlement2.Id, socketEntitlement2);
					}
					else
					{
						socketEntitlement2.Update(entitlement2);
					}
					await TimedInvokeAsync(_entitlementUpdated, "EntitlementUpdated", arg21, socketEntitlement2);
					break;
				}
				case "ENTITLEMENT_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (ENTITLEMENT_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					Entitlement entitlement = (payload as JToken).ToObject<Entitlement>(_serializer);
					SocketEntitlement socketEntitlement = State.RemoveEntitlement(entitlement.Id);
					if (socketEntitlement == null)
					{
						socketEntitlement = SocketEntitlement.Create(this, entitlement);
					}
					else
					{
						socketEntitlement.Update(entitlement);
					}
					await TimedInvokeAsync(arg: new Cacheable<SocketEntitlement, ulong>(socketEntitlement, entitlement.Id, socketEntitlement != null, () => (Task<SocketEntitlement>)null), eventHandler: _entitlementDeleted, name: "EntitlementDeleted");
					break;
				}
				case "SUBSCRIPTION_CREATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (SUBSCRIPTION_CREATE)").ConfigureAwait(continueOnCapturedContext: false);
					Subscription subscription3 = (payload as JToken).ToObject<Subscription>(_serializer);
					SocketSubscription socketSubscription3 = SocketSubscription.Create(this, subscription3);
					State.AddSubscription(subscription3.Id, socketSubscription3);
					await TimedInvokeAsync(_subscriptionCreated, "SubscriptionCreated", socketSubscription3);
					break;
				}
				case "SUBSCRIPTION_UPDATE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (SUBSCRIPTION_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					Subscription subscription2 = (payload as JToken).ToObject<Subscription>(_serializer);
					SocketSubscription socketSubscription2 = State.GetSubscription(subscription2.Id);
					Cacheable<SocketSubscription, ulong> arg9 = new Cacheable<SocketSubscription, ulong>(socketSubscription2?.Clone(), subscription2.Id, socketSubscription2 != null, () => (Task<SocketSubscription>)null);
					if (socketSubscription2 == null)
					{
						socketSubscription2 = SocketSubscription.Create(this, subscription2);
						State.AddSubscription(subscription2.Id, socketSubscription2);
					}
					else
					{
						socketSubscription2.Update(subscription2);
					}
					await TimedInvokeAsync(_subscriptionUpdated, "SubscriptionUpdated", arg9, socketSubscription2);
					break;
				}
				case "SUBSCRIPTION_DELETE":
				{
					await _gatewayLogger.DebugAsync("Received Dispatch (SUBSCRIPTION_DELETE)").ConfigureAwait(continueOnCapturedContext: false);
					Subscription subscription = (payload as JToken).ToObject<Subscription>(_serializer);
					SocketSubscription socketSubscription = State.RemoveSubscription(subscription.Id);
					if (socketSubscription == null)
					{
						socketSubscription = SocketSubscription.Create(this, subscription);
					}
					else
					{
						socketSubscription.Update(subscription);
					}
					await TimedInvokeAsync(arg: new Cacheable<SocketSubscription, ulong>(socketSubscription, subscription.Id, socketSubscription != null, () => (Task<SocketSubscription>)null), eventHandler: _subscriptionDeleted, name: "SubscriptionDeleted");
					break;
				}
				case "CHANNEL_PINS_ACK":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (CHANNEL_PINS_ACK)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "CHANNEL_PINS_UPDATE":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (CHANNEL_PINS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "GUILD_INTEGRATIONS_UPDATE":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (GUILD_INTEGRATIONS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "MESSAGE_ACK":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (MESSAGE_ACK)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "PRESENCES_REPLACE":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (PRESENCES_REPLACE)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				case "USER_SETTINGS_UPDATE":
					await _gatewayLogger.DebugAsync("Ignored Dispatch (USER_SETTINGS_UPDATE)").ConfigureAwait(continueOnCapturedContext: false);
					break;
				default:
					if (!SuppressUnknownDispatchWarnings)
					{
						await _gatewayLogger.WarningAsync("Unknown Dispatch (" + type + ")").ConfigureAwait(continueOnCapturedContext: false);
					}
					await TimedInvokeAsync(_unknownDispatchReceived, "UnknownDispatchReceived", type, payload as JToken);
					break;
				}
				break;
			default:
				await _gatewayLogger.WarningAsync($"Unknown OpCode ({opCode})").ConfigureAwait(continueOnCapturedContext: false);
				break;
			}
		}
		catch (Exception ex)
		{
			if (IncludeRawPayloadOnGatewayErrors)
			{
				ex.Data["opcode"] = opCode;
				ex.Data["type"] = type;
				ex.Data["payload_data"] = (payload as JToken).ToString();
			}
			await _gatewayLogger.ErrorAsync(string.Format("Error handling {0}{1}", opCode, (type != null) ? (" (" + type + ")") : ""), ex).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal DiscordSocketClient(DiscordSocketConfig config, DiscordRestApiClient client)
		: base(config, client)
	{
	}
}
