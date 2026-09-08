using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Gateway;
using Discord.Audio;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SocketGuild : SocketEntity<ulong>, IGuild, IDeletable, ISnowflakeEntity, IEntity<ulong>, IDisposable
{
	private readonly SemaphoreSlim _audioLock;

	private TaskCompletionSource<bool> _syncPromise;

	private TaskCompletionSource<bool> _downloaderPromise;

	private TaskCompletionSource<AudioClient> _audioConnectPromise;

	private ConcurrentDictionary<ulong, SocketGuildChannel> _channels;

	private ConcurrentDictionary<ulong, SocketGuildUser> _members;

	private ConcurrentDictionary<ulong, SocketRole> _roles;

	private ConcurrentDictionary<ulong, SocketVoiceState> _voiceStates;

	private ConcurrentDictionary<ulong, SocketCustomSticker> _stickers;

	private ConcurrentDictionary<ulong, SocketGuildEvent> _events;

	private ConcurrentDictionary<ulong, SocketAutoModRule> _automodRules;

	private ImmutableArray<GuildEmote> _emotes;

	private readonly AuditLogCache _auditLogs;

	private AudioClient _audioClient;

	private VoiceStateUpdateParams _voiceStateUpdateParams;

	public string Name { get; private set; }

	public int AFKTimeout { get; private set; }

	public bool IsWidgetEnabled { get; private set; }

	public VerificationLevel VerificationLevel { get; private set; }

	public MfaLevel MfaLevel { get; private set; }

	public DefaultMessageNotifications DefaultMessageNotifications { get; private set; }

	public ExplicitContentFilterLevel ExplicitContentFilter { get; private set; }

	public int MemberCount { get; internal set; }

	public int DownloadedMemberCount { get; private set; }

	internal bool IsAvailable { get; private set; }

	public bool IsConnected { get; internal set; }

	public ulong? ApplicationId { get; internal set; }

	internal ulong? AFKChannelId { get; private set; }

	internal ulong? WidgetChannelId { get; private set; }

	internal ulong? SafetyAlertsChannelId { get; private set; }

	internal ulong? SystemChannelId { get; private set; }

	internal ulong? RulesChannelId { get; private set; }

	internal ulong? PublicUpdatesChannelId { get; private set; }

	public ulong OwnerId { get; private set; }

	public SocketGuildUser Owner => GetUser(OwnerId);

	public string VoiceRegionId { get; private set; }

	public string IconId { get; private set; }

	public string SplashId { get; private set; }

	public string DiscoverySplashId { get; private set; }

	public PremiumTier PremiumTier { get; private set; }

	public string BannerId { get; private set; }

	public string VanityURLCode { get; private set; }

	public SystemChannelMessageDeny SystemChannelFlags { get; private set; }

	public string Description { get; private set; }

	public int PremiumSubscriptionCount { get; private set; }

	public string PreferredLocale { get; private set; }

	public int? MaxPresences { get; private set; }

	public int? MaxMembers { get; private set; }

	public int? MaxVideoChannelUsers { get; private set; }

	public int? MaxStageVideoChannelUsers { get; private set; }

	public NsfwLevel NsfwLevel { get; private set; }

	public CultureInfo PreferredCulture { get; private set; }

	public bool IsBoostProgressBarEnabled { get; private set; }

	public GuildFeatures Features { get; private set; }

	public GuildIncidentsData IncidentsData { get; private set; }

	public GuildInventorySettings? InventorySettings { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public string IconUrl => CDN.GetGuildIconUrl(base.Id, IconId, 2048);

	public string SplashUrl => CDN.GetGuildSplashUrl(base.Id, SplashId, 2048);

	public string DiscoverySplashUrl => CDN.GetGuildDiscoverySplashUrl(base.Id, DiscoverySplashId);

	public string BannerUrl => CDN.GetGuildBannerUrl(base.Id, BannerId, ImageFormat.Auto);

	public bool HasAllMembers => MemberCount <= DownloadedMemberCount;

	public bool IsSynced => _syncPromise.Task.IsCompleted;

	public Task SyncPromise => _syncPromise.Task;

	public Task DownloaderPromise => _downloaderPromise.Task;

	public IAudioClient AudioClient => _audioClient;

	public SocketTextChannel DefaultChannel => (from c in TextChannels
		where CurrentUser.GetPermissions(c).ViewChannel && !(c is IThreadChannel)
		orderby c.Position
		select c).FirstOrDefault();

	public SocketVoiceChannel AFKChannel
	{
		get
		{
			ulong? aFKChannelId = AFKChannelId;
			if (!aFKChannelId.HasValue)
			{
				return null;
			}
			return GetVoiceChannel(aFKChannelId.Value);
		}
	}

	public int MaxBitrate => GuildHelper.GetMaxBitrate(PremiumTier);

	public ulong MaxUploadLimit => GuildHelper.GetUploadLimit(PremiumTier);

	public SocketGuildChannel WidgetChannel
	{
		get
		{
			ulong? widgetChannelId = WidgetChannelId;
			if (!widgetChannelId.HasValue)
			{
				return null;
			}
			return GetChannel(widgetChannelId.Value);
		}
	}

	public SocketGuildChannel SafetyAlertsChannel
	{
		get
		{
			ulong? safetyAlertsChannelId = SafetyAlertsChannelId;
			if (!safetyAlertsChannelId.HasValue)
			{
				return null;
			}
			return GetChannel(safetyAlertsChannelId.Value);
		}
	}

	public SocketTextChannel SystemChannel
	{
		get
		{
			ulong? systemChannelId = SystemChannelId;
			if (!systemChannelId.HasValue)
			{
				return null;
			}
			return GetTextChannel(systemChannelId.Value);
		}
	}

	public SocketTextChannel RulesChannel
	{
		get
		{
			ulong? rulesChannelId = RulesChannelId;
			if (!rulesChannelId.HasValue)
			{
				return null;
			}
			return GetTextChannel(rulesChannelId.Value);
		}
	}

	public SocketTextChannel PublicUpdatesChannel
	{
		get
		{
			ulong? publicUpdatesChannelId = PublicUpdatesChannelId;
			if (!publicUpdatesChannelId.HasValue)
			{
				return null;
			}
			return GetTextChannel(publicUpdatesChannelId.Value);
		}
	}

	public IReadOnlyCollection<SocketTextChannel> TextChannels => Channels.OfType<SocketTextChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketVoiceChannel> VoiceChannels => Channels.OfType<SocketVoiceChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketStageChannel> StageChannels => Channels.OfType<SocketStageChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketCategoryChannel> CategoryChannels => Channels.OfType<SocketCategoryChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketThreadChannel> ThreadChannels => Channels.OfType<SocketThreadChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketForumChannel> ForumChannels => Channels.OfType<SocketForumChannel>().ToImmutableArray();

	public IReadOnlyCollection<SocketMediaChannel> MediaChannels => Channels.OfType<SocketMediaChannel>().ToImmutableArray();

	public SocketGuildUser CurrentUser
	{
		get
		{
			if (!_members.TryGetValue(base.Discord.CurrentUser.Id, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public SocketRole EveryoneRole => GetRole(base.Id);

	public IReadOnlyCollection<SocketGuildChannel> Channels
	{
		get
		{
			ConcurrentDictionary<ulong, SocketGuildChannel> channels = _channels;
			_ = base.Discord.State;
			return (from x in channels
				select x.Value into x
				where x != null
				select x).ToReadOnlyCollection(channels);
		}
	}

	public IReadOnlyCollection<GuildEmote> Emotes => _emotes;

	public IReadOnlyCollection<SocketCustomSticker> Stickers => _stickers.Select((KeyValuePair<ulong, SocketCustomSticker> x) => x.Value).ToImmutableArray();

	public IReadOnlyCollection<SocketGuildUser> Users => _members.ToReadOnlyCollection();

	public IReadOnlyCollection<SocketRole> Roles => _roles.ToReadOnlyCollection();

	public IReadOnlyCollection<SocketGuildEvent> Events => _events.ToReadOnlyCollection();

	public IReadOnlyCollection<SocketAuditLogEntry> CachedAuditLogs => (IReadOnlyCollection<SocketAuditLogEntry>)(_auditLogs?.AuditLogs ?? ((object)ImmutableArray.Create<SocketAuditLogEntry>()));

	private string DebuggerDisplay => $"{Name} ({base.Id})";

	public IReadOnlyCollection<SocketAutoModRule> AutoModRules => _automodRules.ToReadOnlyCollection();

	ulong? IGuild.AFKChannelId => AFKChannelId;

	IAudioClient IGuild.AudioClient => AudioClient;

	bool IGuild.Available => true;

	ulong? IGuild.WidgetChannelId => WidgetChannelId;

	ulong? IGuild.SafetyAlertsChannelId => SafetyAlertsChannelId;

	ulong? IGuild.SystemChannelId => SystemChannelId;

	ulong? IGuild.RulesChannelId => RulesChannelId;

	ulong? IGuild.PublicUpdatesChannelId => PublicUpdatesChannelId;

	IRole IGuild.EveryoneRole => EveryoneRole;

	IReadOnlyCollection<IRole> IGuild.Roles => Roles;

	int? IGuild.ApproximateMemberCount => null;

	int? IGuild.ApproximatePresenceCount => null;

	IReadOnlyCollection<ICustomSticker> IGuild.Stickers => Stickers;

	internal SocketGuild(DiscordSocketClient client, ulong id)
		: base(client, id)
	{
		_audioLock = new SemaphoreSlim(1, 1);
		_emotes = ImmutableArray.Create<GuildEmote>();
		_automodRules = new ConcurrentDictionary<ulong, SocketAutoModRule>();
		_auditLogs = new AuditLogCache(client);
	}

	internal static SocketGuild Create(DiscordSocketClient discord, ClientState state, ExtendedGuild model)
	{
		SocketGuild socketGuild = new SocketGuild(discord, model.Id);
		socketGuild.Update(state, model);
		return socketGuild;
	}

	internal void Update(ClientState state, ExtendedGuild model)
	{
		IsAvailable = model.Unavailable != true;
		if (!IsAvailable)
		{
			if (_events == null)
			{
				_events = new ConcurrentDictionary<ulong, SocketGuildEvent>();
			}
			if (_channels == null)
			{
				_channels = new ConcurrentDictionary<ulong, SocketGuildChannel>();
			}
			if (_members == null)
			{
				_members = new ConcurrentDictionary<ulong, SocketGuildUser>();
			}
			if (_roles == null)
			{
				_roles = new ConcurrentDictionary<ulong, SocketRole>();
			}
			_syncPromise = new TaskCompletionSource<bool>();
			_downloaderPromise = new TaskCompletionSource<bool>();
			return;
		}
		Update(state, (Guild)model);
		ConcurrentDictionary<ulong, SocketGuildChannel> concurrentDictionary = new ConcurrentDictionary<ulong, SocketGuildChannel>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)model.Channels.Length * 1.05));
		for (int i = 0; i < model.Channels.Length; i++)
		{
			SocketGuildChannel socketGuildChannel = SocketGuildChannel.Create(this, state, model.Channels[i]);
			state.AddChannel(socketGuildChannel);
			concurrentDictionary.TryAdd(socketGuildChannel.Id, socketGuildChannel);
		}
		for (int j = 0; j < model.Threads.Length; j++)
		{
			SocketThreadChannel socketThreadChannel = SocketThreadChannel.Create(this, state, model.Threads[j]);
			state.AddChannel(socketThreadChannel);
			concurrentDictionary.TryAdd(socketThreadChannel.Id, socketThreadChannel);
		}
		_channels = concurrentDictionary;
		ConcurrentDictionary<ulong, SocketGuildUser> concurrentDictionary2 = new ConcurrentDictionary<ulong, SocketGuildUser>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)model.Members.Length * 1.05));
		for (int k = 0; k < model.Members.Length; k++)
		{
			SocketGuildUser socketGuildUser = SocketGuildUser.Create(this, state, model.Members[k]);
			if (concurrentDictionary2.TryAdd(socketGuildUser.Id, socketGuildUser))
			{
				socketGuildUser.GlobalUser.AddRef();
			}
		}
		DownloadedMemberCount = concurrentDictionary2.Count;
		for (int l = 0; l < model.Presences.Length; l++)
		{
			if (concurrentDictionary2.TryGetValue(model.Presences[l].User.Id, out var value))
			{
				value.Update(state, model.Presences[l], updatePresence: true);
			}
		}
		_members = concurrentDictionary2;
		MemberCount = model.MemberCount;
		ConcurrentDictionary<ulong, SocketVoiceState> concurrentDictionary3 = new ConcurrentDictionary<ulong, SocketVoiceState>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)model.VoiceStates.Length * 1.05));
		for (int m = 0; m < model.VoiceStates.Length; m++)
		{
			SocketVoiceChannel voiceChannel = null;
			if (model.VoiceStates[m].ChannelId.HasValue)
			{
				voiceChannel = state.GetChannel(model.VoiceStates[m].ChannelId.Value) as SocketVoiceChannel;
			}
			SocketVoiceState value2 = SocketVoiceState.Create(voiceChannel, model.VoiceStates[m]);
			concurrentDictionary3.TryAdd(model.VoiceStates[m].UserId, value2);
		}
		_voiceStates = concurrentDictionary3;
		ConcurrentDictionary<ulong, SocketGuildEvent> concurrentDictionary4 = new ConcurrentDictionary<ulong, SocketGuildEvent>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)model.GuildScheduledEvents.Length * 1.05));
		for (int n = 0; n < model.GuildScheduledEvents.Length; n++)
		{
			SocketGuildEvent socketGuildEvent = SocketGuildEvent.Create(base.Discord, this, model.GuildScheduledEvents[n]);
			concurrentDictionary4.TryAdd(socketGuildEvent.Id, socketGuildEvent);
		}
		_events = concurrentDictionary4;
		_syncPromise = new TaskCompletionSource<bool>();
		_downloaderPromise = new TaskCompletionSource<bool>();
		_syncPromise.TrySetResultAsync(result: true);
	}

	internal void Update(ClientState state, Guild model)
	{
		AFKChannelId = model.AFKChannelId;
		if (model.WidgetChannelId.IsSpecified)
		{
			WidgetChannelId = model.WidgetChannelId.Value;
		}
		if (model.SafetyAlertsChannelId.IsSpecified)
		{
			SafetyAlertsChannelId = model.SafetyAlertsChannelId.Value;
		}
		SystemChannelId = model.SystemChannelId;
		RulesChannelId = model.RulesChannelId;
		PublicUpdatesChannelId = model.PublicUpdatesChannelId;
		AFKTimeout = model.AFKTimeout;
		if (model.WidgetEnabled.IsSpecified)
		{
			IsWidgetEnabled = model.WidgetEnabled.Value;
		}
		IconId = model.Icon;
		Name = model.Name;
		OwnerId = model.OwnerId;
		VoiceRegionId = model.Region;
		SplashId = model.Splash;
		DiscoverySplashId = model.DiscoverySplash;
		VerificationLevel = model.VerificationLevel;
		MfaLevel = model.MfaLevel;
		DefaultMessageNotifications = model.DefaultMessageNotifications;
		ExplicitContentFilter = model.ExplicitContentFilter;
		ApplicationId = model.ApplicationId;
		PremiumTier = model.PremiumTier;
		VanityURLCode = model.VanityURLCode;
		BannerId = model.Banner;
		SystemChannelFlags = model.SystemChannelFlags;
		Description = model.Description;
		PremiumSubscriptionCount = model.PremiumSubscriptionCount.GetValueOrDefault();
		NsfwLevel = model.NsfwLevel;
		if (model.MaxPresences.IsSpecified)
		{
			MaxPresences = model.MaxPresences.Value ?? 25000;
		}
		if (model.MaxMembers.IsSpecified)
		{
			MaxMembers = model.MaxMembers.Value;
		}
		if (model.MaxVideoChannelUsers.IsSpecified)
		{
			MaxVideoChannelUsers = model.MaxVideoChannelUsers.Value;
		}
		if (model.MaxStageVideoChannelUsers.IsSpecified)
		{
			MaxStageVideoChannelUsers = model.MaxStageVideoChannelUsers.Value;
		}
		PreferredLocale = model.PreferredLocale;
		PreferredCulture = ((PreferredLocale == null) ? null : new CultureInfo(PreferredLocale));
		if (model.IsBoostProgressBarEnabled.IsSpecified)
		{
			IsBoostProgressBarEnabled = model.IsBoostProgressBarEnabled.Value;
		}
		if (model.InventorySettings.IsSpecified)
		{
			InventorySettings = ((model.InventorySettings.Value == null) ? ((GuildInventorySettings?)null) : new GuildInventorySettings?(new GuildInventorySettings(model.InventorySettings.Value.IsEmojiPackCollectible.GetValueOrDefault(defaultValue: false))));
		}
		IncidentsData = ((model.IncidentsData != null) ? new GuildIncidentsData
		{
			DmsDisabledUntil = model.IncidentsData.DmsDisabledUntil,
			InvitesDisabledUntil = model.IncidentsData.InvitesDisabledUntil
		} : new GuildIncidentsData());
		if (model.Emojis != null)
		{
			ImmutableArray<GuildEmote>.Builder builder = ImmutableArray.CreateBuilder<GuildEmote>(model.Emojis.Length);
			for (int i = 0; i < model.Emojis.Length; i++)
			{
				builder.Add(model.Emojis[i].ToEntity());
			}
			_emotes = builder.ToImmutable();
		}
		else
		{
			_emotes = ImmutableArray.Create<GuildEmote>();
		}
		Features = model.Features;
		ConcurrentDictionary<ulong, SocketRole> concurrentDictionary = new ConcurrentDictionary<ulong, SocketRole>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)model.Roles.Length * 1.05));
		for (int j = 0; j < model.Roles.Length; j++)
		{
			SocketRole socketRole = SocketRole.Create(this, state, model.Roles[j]);
			concurrentDictionary.TryAdd(socketRole.Id, socketRole);
		}
		_roles = concurrentDictionary;
		if (model.Stickers != null)
		{
			ConcurrentDictionary<ulong, SocketCustomSticker> concurrentDictionary2 = new ConcurrentDictionary<ulong, SocketCustomSticker>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)model.Stickers.Length * 1.05));
			for (int k = 0; k < model.Stickers.Length; k++)
			{
				global::Discord.API.Sticker sticker = model.Stickers[k];
				if (sticker.User.IsSpecified)
				{
					AddOrUpdateUser(sticker.User.Value);
				}
				SocketCustomSticker value = SocketCustomSticker.Create(base.Discord, sticker, this, sticker.User.IsSpecified ? new ulong?(sticker.User.Value.Id) : ((ulong?)null));
				concurrentDictionary2.TryAdd(sticker.Id, value);
			}
			_stickers = concurrentDictionary2;
		}
		else
		{
			_stickers = new ConcurrentDictionary<ulong, SocketCustomSticker>(ConcurrentHashSet.DefaultConcurrencyLevel, 7);
		}
	}

	internal void Update(ClientState state, GuildEmojiUpdateEvent model)
	{
		ImmutableArray<GuildEmote>.Builder builder = ImmutableArray.CreateBuilder<GuildEmote>(model.Emojis.Length);
		for (int i = 0; i < model.Emojis.Length; i++)
		{
			builder.Add(model.Emojis[i].ToEntity());
		}
		_emotes = builder.ToImmutable();
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return GuildHelper.DeleteAsync(this, base.Discord, options);
	}

	public Task ModifyAsync(Action<GuildProperties> func, RequestOptions options = null)
	{
		return GuildHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public Task ModifyWidgetAsync(Action<GuildWidgetProperties> func, RequestOptions options = null)
	{
		return GuildHelper.ModifyWidgetAsync(this, base.Discord, func, options);
	}

	public Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions options = null)
	{
		return GuildHelper.ReorderChannelsAsync(this, base.Discord, args, options);
	}

	public Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions options = null)
	{
		return GuildHelper.ReorderRolesAsync(this, base.Discord, args, options);
	}

	public Task LeaveAsync(RequestOptions options = null)
	{
		return GuildHelper.LeaveAsync(this, base.Discord, options);
	}

	public Task<GuildIncidentsData> ModifyIncidentActionsAsync(Action<GuildIncidentsDataProperties> props, RequestOptions options = null)
	{
		return GuildHelper.ModifyGuildIncidentActionsAsync(this, base.Discord, props, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestBan>> GetBansAsync(int limit = 1000, RequestOptions options = null)
	{
		return GuildHelper.GetBansAsync(this, base.Discord, null, Direction.Before, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestBan>> GetBansAsync(ulong fromUserId, Direction dir, int limit = 1000, RequestOptions options = null)
	{
		return GuildHelper.GetBansAsync(this, base.Discord, fromUserId, dir, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestBan>> GetBansAsync(IUser fromUser, Direction dir, int limit = 1000, RequestOptions options = null)
	{
		return GuildHelper.GetBansAsync(this, base.Discord, fromUser.Id, dir, limit, options);
	}

	public Task<RestBan> GetBanAsync(IUser user, RequestOptions options = null)
	{
		return GuildHelper.GetBanAsync(this, base.Discord, user.Id, options);
	}

	public Task<RestBan> GetBanAsync(ulong userId, RequestOptions options = null)
	{
		return GuildHelper.GetBanAsync(this, base.Discord, userId, options);
	}

	public Task AddBanAsync(IUser user, int pruneDays = 0, string reason = null, RequestOptions options = null)
	{
		return GuildHelper.AddBanAsync(this, base.Discord, user.Id, pruneDays, reason, options);
	}

	public Task AddBanAsync(ulong userId, int pruneDays = 0, string reason = null, RequestOptions options = null)
	{
		return GuildHelper.AddBanAsync(this, base.Discord, userId, pruneDays, reason, options);
	}

	public Task BanUserAsync(IUser user, uint pruneSeconds = 0u, RequestOptions options = null)
	{
		return GuildHelper.AddBanAsync(this, base.Discord, user.Id, pruneSeconds, options);
	}

	public Task BanUserAsync(ulong userId, uint pruneSeconds = 0u, RequestOptions options = null)
	{
		return GuildHelper.AddBanAsync(this, base.Discord, userId, pruneSeconds, options);
	}

	public Task RemoveBanAsync(IUser user, RequestOptions options = null)
	{
		return GuildHelper.RemoveBanAsync(this, base.Discord, user.Id, options);
	}

	public Task RemoveBanAsync(ulong userId, RequestOptions options = null)
	{
		return GuildHelper.RemoveBanAsync(this, base.Discord, userId, options);
	}

	public Task<BulkBanResult> BulkBanAsync(IEnumerable<ulong> userIds, int? deleteMessageSeconds = null, RequestOptions options = null)
	{
		return GuildHelper.BulkBanAsync(this, base.Discord, userIds.ToArray(), deleteMessageSeconds, options);
	}

	public SocketGuildChannel GetChannel(ulong id)
	{
		SocketGuildChannel socketGuildChannel = base.Discord.State.GetChannel(id) as SocketGuildChannel;
		if (socketGuildChannel?.Guild.Id == base.Id)
		{
			return socketGuildChannel;
		}
		return null;
	}

	public SocketTextChannel GetTextChannel(ulong id)
	{
		return GetChannel(id) as SocketTextChannel;
	}

	public SocketThreadChannel GetThreadChannel(ulong id)
	{
		return GetChannel(id) as SocketThreadChannel;
	}

	public SocketForumChannel GetForumChannel(ulong id)
	{
		return GetChannel(id) as SocketForumChannel;
	}

	public SocketVoiceChannel GetVoiceChannel(ulong id)
	{
		return GetChannel(id) as SocketVoiceChannel;
	}

	public SocketStageChannel GetStageChannel(ulong id)
	{
		return GetChannel(id) as SocketStageChannel;
	}

	public SocketCategoryChannel GetCategoryChannel(ulong id)
	{
		return GetChannel(id) as SocketCategoryChannel;
	}

	public SocketMediaChannel GetMediaChannel(ulong id)
	{
		return GetChannel(id) as SocketMediaChannel;
	}

	public Task<RestTextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateTextChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestNewsChannel> CreateNewsChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateNewsChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateVoiceChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestStageChannel> CreateStageChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateStageChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestCategoryChannel> CreateCategoryChannelAsync(string name, Action<GuildChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateCategoryChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestForumChannel> CreateForumChannelAsync(string name, Action<ForumChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateForumChannelAsync(this, base.Discord, name, options, func);
	}

	public Task<RestMediaChannel> CreateMediaChannelAsync(string name, Action<ForumChannelProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.CreateMediaChannelAsync(this, base.Discord, name, options, func);
	}

	internal SocketGuildChannel AddChannel(ClientState state, Channel model)
	{
		SocketGuildChannel socketGuildChannel = SocketGuildChannel.Create(this, state, model);
		_channels.TryAdd(model.Id, socketGuildChannel);
		state.AddChannel(socketGuildChannel);
		return socketGuildChannel;
	}

	internal SocketGuildChannel AddOrUpdateChannel(ClientState state, Channel model)
	{
		if (_channels.TryGetValue(model.Id, out var value))
		{
			value.Update(base.Discord.State, model);
		}
		else
		{
			value = SocketGuildChannel.Create(this, base.Discord.State, model);
			_channels[value.Id] = value;
			state.AddChannel(value);
		}
		return value;
	}

	internal SocketGuildChannel RemoveChannel(ClientState state, ulong id)
	{
		if (_channels.TryRemove(id, out var _))
		{
			return state.RemoveChannel(id) as SocketGuildChannel;
		}
		return null;
	}

	internal void PurgeChannelCache(ClientState state)
	{
		foreach (KeyValuePair<ulong, SocketGuildChannel> channel in _channels)
		{
			state.RemoveChannel(channel.Key);
		}
		_channels.Clear();
	}

	public Task<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
	{
		return GuildHelper.GetVoiceRegionsAsync(this, base.Discord, options);
	}

	public Task<IReadOnlyCollection<RestIntegration>> GetIntegrationsAsync(RequestOptions options = null)
	{
		return GuildHelper.GetIntegrationsAsync(this, base.Discord, options);
	}

	public Task DeleteIntegrationAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.DeleteIntegrationAsync(this, base.Discord, id, options);
	}

	public Task DeleteApplicationCommandsAsync(RequestOptions options = null)
	{
		return InteractionHelper.DeleteAllGuildCommandsAsync(base.Discord, base.Id, options);
	}

	public async Task<IReadOnlyCollection<SocketApplicationCommand>> GetApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		IEnumerable<SocketApplicationCommand> enumerable = (await base.Discord.ApiClient.GetGuildApplicationCommandsAsync(base.Id, withLocalizations, locale, options)).Select((ApplicationCommand x) => SocketApplicationCommand.Create(base.Discord, x, base.Id));
		foreach (SocketApplicationCommand item in enumerable)
		{
			base.Discord.State.AddCommand(item);
		}
		return enumerable.ToImmutableArray();
	}

	public async ValueTask<SocketApplicationCommand> GetApplicationCommandAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
	{
		SocketApplicationCommand command = base.Discord.State.GetCommand(id);
		if (command != null)
		{
			return command;
		}
		if (mode == CacheMode.CacheOnly)
		{
			return null;
		}
		ApplicationCommand applicationCommand = await base.Discord.ApiClient.GetGuildApplicationCommandAsync(base.Id, id, options);
		if (applicationCommand == null)
		{
			return null;
		}
		command = SocketApplicationCommand.Create(base.Discord, applicationCommand, base.Id);
		base.Discord.State.AddCommand(command);
		return command;
	}

	public async Task<SocketApplicationCommand> CreateApplicationCommandAsync(ApplicationCommandProperties properties, RequestOptions options = null)
	{
		ApplicationCommand model = await InteractionHelper.CreateGuildCommandAsync(base.Discord, base.Id, properties, options);
		SocketApplicationCommand orAddCommand = base.Discord.State.GetOrAddCommand(model.Id, (ulong id) => SocketApplicationCommand.Create(base.Discord, model, base.Id));
		orAddCommand.Update(model);
		return orAddCommand;
	}

	public async Task<IReadOnlyCollection<SocketApplicationCommand>> BulkOverwriteApplicationCommandAsync(ApplicationCommandProperties[] properties, RequestOptions options = null)
	{
		IEnumerable<SocketApplicationCommand> enumerable = (await InteractionHelper.BulkOverwriteGuildCommandsAsync(base.Discord, base.Id, properties, options)).Select((ApplicationCommand x) => SocketApplicationCommand.Create(base.Discord, x, base.Id));
		base.Discord.State.PurgeCommands((SocketApplicationCommand x) => !x.IsGlobalCommand && x.Guild.Id == base.Id);
		foreach (SocketApplicationCommand item in enumerable)
		{
			base.Discord.State.AddCommand(item);
		}
		return enumerable.ToImmutableArray();
	}

	public Task<IReadOnlyCollection<RestInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
	{
		return GuildHelper.GetInvitesAsync(this, base.Discord, options);
	}

	public Task<RestInviteMetadata> GetVanityInviteAsync(RequestOptions options = null)
	{
		return GuildHelper.GetVanityInviteAsync(this, base.Discord, options);
	}

	public SocketRole GetRole(ulong id)
	{
		if (_roles.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public Task<RestRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, bool isMentionable = false, RequestOptions options = null, Image? icon = null, Emoji emoji = null)
	{
		return GuildHelper.CreateRoleAsync(this, base.Discord, name, permissions, color, isHoisted, isMentionable, options, icon, emoji);
	}

	internal SocketRole AddRole(Role model)
	{
		SocketRole socketRole = SocketRole.Create(this, base.Discord.State, model);
		_roles[model.Id] = socketRole;
		return socketRole;
	}

	internal SocketRole RemoveRole(ulong id)
	{
		if (_roles.TryRemove(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal SocketRole AddOrUpdateRole(Role model)
	{
		if (_roles.TryGetValue(model.Id, out var value))
		{
			_roles[model.Id].Update(base.Discord.State, model);
			return value;
		}
		return AddRole(model);
	}

	internal SocketCustomSticker AddSticker(global::Discord.API.Sticker model)
	{
		if (model.User.IsSpecified)
		{
			AddOrUpdateUser(model.User.Value);
		}
		SocketCustomSticker socketCustomSticker = SocketCustomSticker.Create(base.Discord, model, this, model.User.IsSpecified ? new ulong?(model.User.Value.Id) : ((ulong?)null));
		_stickers[model.Id] = socketCustomSticker;
		return socketCustomSticker;
	}

	internal SocketCustomSticker AddOrUpdateSticker(global::Discord.API.Sticker model)
	{
		if (_stickers.TryGetValue(model.Id, out var value))
		{
			_stickers[model.Id].Update(model);
			return value;
		}
		return AddSticker(model);
	}

	internal SocketCustomSticker RemoveSticker(ulong id)
	{
		if (_stickers.TryRemove(id, out var value))
		{
			return value;
		}
		return null;
	}

	public Task<RestGuildUser> AddGuildUserAsync(ulong id, string accessToken, Action<AddGuildUserProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.AddGuildUserAsync(this, base.Discord, id, accessToken, func, options);
	}

	public SocketGuildUser GetUser(ulong id)
	{
		if (_members.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions options = null, IEnumerable<ulong> includeRoleIds = null)
	{
		return GuildHelper.PruneUsersAsync(this, base.Discord, days, simulate, options, includeRoleIds);
	}

	internal SocketGuildUser AddOrUpdateUser(User model)
	{
		if (_members.TryGetValue(model.Id, out var value))
		{
			value.GlobalUser?.Update(base.Discord.State, model);
		}
		else
		{
			value = SocketGuildUser.Create(this, base.Discord.State, model);
			value.GlobalUser.AddRef();
			_members[value.Id] = value;
			DownloadedMemberCount++;
		}
		return value;
	}

	internal SocketGuildUser AddOrUpdateUser(GuildMember model)
	{
		if (_members.TryGetValue(model.User.Id, out var value))
		{
			value.Update(base.Discord.State, model);
		}
		else
		{
			value = SocketGuildUser.Create(this, base.Discord.State, model);
			value.GlobalUser.AddRef();
			_members[value.Id] = value;
			DownloadedMemberCount++;
		}
		return value;
	}

	internal SocketGuildUser AddOrUpdateUser(Presence model)
	{
		if (_members.TryGetValue(model.User.Id, out var value))
		{
			value.Update(base.Discord.State, model, updatePresence: false);
		}
		else
		{
			value = SocketGuildUser.Create(this, base.Discord.State, model);
			value.GlobalUser.AddRef();
			_members[value.Id] = value;
			DownloadedMemberCount++;
		}
		return value;
	}

	internal SocketGuildUser RemoveUser(ulong id)
	{
		if (_members.TryRemove(id, out var value))
		{
			DownloadedMemberCount--;
			value.GlobalUser.RemoveRef(base.Discord);
			return value;
		}
		return null;
	}

	public void PurgeUserCache()
	{
		PurgeUserCache((SocketGuildUser _) => true);
	}

	public void PurgeUserCache(Func<SocketGuildUser, bool> predicate)
	{
		IEnumerable<SocketGuildUser> enumerable = Users.Where((SocketGuildUser x) => predicate(x) && x?.Id != base.Discord.CurrentUser.Id);
		IEnumerable<SocketGuildUser> enumerable2 = Users.Where((SocketGuildUser x) => !predicate(x) || x?.Id == base.Discord.CurrentUser.Id);
		foreach (SocketGuildUser item in enumerable)
		{
			if (_members.TryRemove(item.Id, out var _))
			{
				item.GlobalUser.RemoveRef(base.Discord);
			}
		}
		foreach (SocketGuildUser item2 in enumerable2)
		{
			_members.TryAdd(item2.Id, item2);
		}
		_downloaderPromise = new TaskCompletionSource<bool>();
		DownloadedMemberCount = _members.Count;
	}

	public IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> GetUsersAsync(RequestOptions options = null)
	{
		if (HasAllMembers)
		{
			return ((IEnumerable<IReadOnlyCollection<IGuildUser>>)ImmutableArray.Create(Users)).ToAsyncEnumerable();
		}
		return GuildHelper.GetUsersAsync(this, base.Discord, null, null, options);
	}

	public Task DownloadUsersAsync()
	{
		return base.Discord.DownloadUsersAsync(new SocketGuild[1] { this });
	}

	internal void CompleteDownloadUsers()
	{
		_downloaderPromise.TrySetResultAsync(result: true);
	}

	public Task<IReadOnlyCollection<RestGuildUser>> SearchUsersAsync(string query, int limit = 1000, RequestOptions options = null)
	{
		return GuildHelper.SearchUsersAsync(this, base.Discord, query, limit, options);
	}

	public Task<MemberSearchResult> SearchUsersAsyncV2(int limit = 1000, MemberSearchPropertiesV2 args = null, RequestOptions options = null)
	{
		return GuildHelper.SearchUsersAsyncV2(this, base.Discord, limit, args, options);
	}

	public SocketGuildEvent GetEvent(ulong id)
	{
		if (_events.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal SocketGuildEvent RemoveEvent(ulong id)
	{
		if (_events.TryRemove(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal SocketGuildEvent AddOrUpdateEvent(GuildScheduledEvent model)
	{
		if (_events.TryGetValue(model.Id, out var value))
		{
			value.Update(model);
		}
		else
		{
			value = SocketGuildEvent.Create(base.Discord, this, model);
			_events[model.Id] = value;
		}
		return value;
	}

	public Task<RestGuildEvent> GetEventAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetGuildEventAsync(base.Discord, id, this, options);
	}

	public Task<IReadOnlyCollection<RestGuildEvent>> GetEventsAsync(RequestOptions options = null)
	{
		return GuildHelper.GetGuildEventsAsync(base.Discord, this, options);
	}

	public Task<RestGuildEvent> CreateEventAsync(string name, DateTimeOffset startTime, GuildScheduledEventType type, GuildScheduledEventPrivacyLevel privacyLevel = GuildScheduledEventPrivacyLevel.Private, string description = null, DateTimeOffset? endTime = null, ulong? channelId = null, string location = null, Image? coverImage = null, RequestOptions options = null, GuildScheduledEventRecurrenceRuleProperties recurrenceRule = null)
	{
		switch (type)
		{
		case GuildScheduledEventType.Stage:
			CurrentUser.GuildPermissions.Ensure(GuildPermission.ManageChannels | GuildPermission.MuteMembers | GuildPermission.MoveMembers | GuildPermission.ManageEvents);
			break;
		case GuildScheduledEventType.Voice:
			CurrentUser.GuildPermissions.Ensure(GuildPermission.ViewChannel | GuildPermission.Connect | GuildPermission.ManageEvents);
			break;
		case GuildScheduledEventType.External:
			CurrentUser.GuildPermissions.Ensure(GuildPermission.ManageEvents);
			break;
		}
		return GuildHelper.CreateGuildEventAsync(base.Discord, this, name, privacyLevel, startTime, type, description, endTime, channelId, location, coverImage, options, recurrenceRule);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestAuditLogEntry>> GetAuditLogsAsync(int limit, RequestOptions options = null, ulong? beforeId = null, ulong? userId = null, ActionType? actionType = null, ulong? afterId = null)
	{
		return GuildHelper.GetAuditLogsAsync(this, base.Discord, beforeId, limit, options, userId, actionType, afterId);
	}

	public SocketAuditLogEntry GetCachedAuditLog(ulong id)
	{
		return _auditLogs.Get(id);
	}

	public IReadOnlyCollection<SocketAuditLogEntry> GetCachedAuditLogs(int limit = 100, ActionType? action = null, ulong? fromEntryId = null, Direction direction = Direction.Before)
	{
		return _auditLogs.GetMany(fromEntryId, direction, limit, action);
	}

	internal void AddAuditLog(SocketAuditLogEntry entry)
	{
		_auditLogs.Add(entry);
	}

	public Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetWebhookAsync(this, base.Discord, id, options);
	}

	public Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(RequestOptions options = null)
	{
		return GuildHelper.GetWebhooksAsync(this, base.Discord, options);
	}

	public Task<IReadOnlyCollection<GuildEmote>> GetEmotesAsync(RequestOptions options = null)
	{
		return GuildHelper.GetEmotesAsync(this, base.Discord, options);
	}

	public Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetEmoteAsync(this, base.Discord, id, options);
	}

	public Task<GuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default(Optional<IEnumerable<IRole>>), RequestOptions options = null)
	{
		return GuildHelper.CreateEmoteAsync(this, base.Discord, name, image, roles, options);
	}

	public Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func, RequestOptions options = null)
	{
		return GuildHelper.ModifyEmoteAsync(this, base.Discord, emote.Id, func, options);
	}

	public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions options = null)
	{
		return GuildHelper.DeleteEmoteAsync(this, base.Discord, emote.Id, options);
	}

	public Task MoveAsync(IGuildUser user, IVoiceChannel targetChannel)
	{
		return user.ModifyAsync(delegate(GuildUserProperties x)
		{
			x.Channel = new Optional<IVoiceChannel>(targetChannel);
		});
	}

	Task IGuild.DisconnectAsync(IGuildUser user)
	{
		return user.ModifyAsync(delegate(GuildUserProperties x)
		{
			x.Channel = null;
		});
	}

	public async ValueTask<SocketCustomSticker> GetStickerAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
	{
		KeyValuePair<ulong, SocketCustomSticker>? keyValuePair = _stickers?.FirstOrDefault((KeyValuePair<ulong, SocketCustomSticker> x) => x.Key == id);
		if (keyValuePair?.Value != null)
		{
			return keyValuePair?.Value;
		}
		if (mode == CacheMode.CacheOnly)
		{
			return null;
		}
		global::Discord.API.Sticker sticker = await base.Discord.ApiClient.GetGuildStickerAsync(base.Id, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (sticker == null)
		{
			return null;
		}
		return AddOrUpdateSticker(sticker);
	}

	public SocketCustomSticker GetSticker(ulong id)
	{
		return GetStickerAsync(id, CacheMode.CacheOnly).GetAwaiter().GetResult();
	}

	public async ValueTask<IReadOnlyCollection<SocketCustomSticker>> GetStickersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
	{
		if (Stickers.Count > 0)
		{
			return Stickers;
		}
		if (mode == CacheMode.CacheOnly)
		{
			return ImmutableArray.Create<SocketCustomSticker>();
		}
		global::Discord.API.Sticker[] obj = await base.Discord.ApiClient.ListGuildStickersAsync(base.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		List<SocketCustomSticker> list = new List<SocketCustomSticker>();
		global::Discord.API.Sticker[] array = obj;
		foreach (global::Discord.API.Sticker model in array)
		{
			list.Add(AddOrUpdateSticker(model));
		}
		return list;
	}

	public async Task<SocketCustomSticker> CreateStickerAsync(string name, Image image, IEnumerable<string> tags, string description = null, RequestOptions options = null)
	{
		return AddOrUpdateSticker(await GuildHelper.CreateStickerAsync(base.Discord, this, name, image, tags, description, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public async Task<SocketCustomSticker> CreateStickerAsync(string name, string path, IEnumerable<string> tags, string description = null, RequestOptions options = null)
	{
		using FileStream fs = File.OpenRead(path);
		return await CreateStickerAsync(name, fs, Path.GetFileName(fs.Name), tags, description, options);
	}

	public async Task<SocketCustomSticker> CreateStickerAsync(string name, Stream stream, string filename, IEnumerable<string> tags, string description = null, RequestOptions options = null)
	{
		return AddOrUpdateSticker(await GuildHelper.CreateStickerAsync(base.Discord, this, name, stream, filename, tags, description, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task DeleteStickerAsync(SocketCustomSticker sticker, RequestOptions options = null)
	{
		return sticker.DeleteAsync(options);
	}

	internal async Task<SocketVoiceState> AddOrUpdateVoiceStateAsync(ClientState state, VoiceState model)
	{
		SocketVoiceChannel voiceChannel = state.GetChannel(model.ChannelId.Value) as SocketVoiceChannel;
		SocketVoiceState socketVoiceState = GetVoiceState(model.UserId) ?? SocketVoiceState.Default;
		SocketVoiceState after = SocketVoiceState.Create(voiceChannel, model);
		_voiceStates[model.UserId] = after;
		if (_audioClient != null && socketVoiceState.VoiceChannel?.Id != after.VoiceChannel?.Id)
		{
			if (model.UserId == CurrentUser.Id)
			{
				if (after.VoiceChannel != null && _audioClient.ChannelId != after.VoiceChannel?.Id)
				{
					_audioClient.ChannelId = after.VoiceChannel.Id;
					await _audioClient.StopAsync(global::Discord.Audio.AudioClient.StopReason.Moved);
				}
			}
			else
			{
				await _audioClient.RemoveInputStreamAsync(model.UserId).ConfigureAwait(continueOnCapturedContext: false);
				if (CurrentUser.VoiceChannel != null && after.VoiceChannel?.Id == CurrentUser.VoiceChannel?.Id)
				{
					await _audioClient.CreateInputStreamAsync(model.UserId).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
		return after;
	}

	internal SocketVoiceState? GetVoiceState(ulong id)
	{
		if (_voiceStates.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal async Task<SocketVoiceState?> RemoveVoiceStateAsync(ulong id)
	{
		if (_voiceStates.TryRemove(id, out var voiceState))
		{
			if (_audioClient != null)
			{
				await _audioClient.RemoveInputStreamAsync(id).ConfigureAwait(continueOnCapturedContext: false);
				if (id == CurrentUser.Id)
				{
					await _audioClient.StopAsync(global::Discord.Audio.AudioClient.StopReason.Disconnected);
				}
			}
			return voiceState;
		}
		return null;
	}

	internal AudioInStream GetAudioStream(ulong userId)
	{
		return _audioClient?.GetInputStream(userId);
	}

	internal async Task<IAudioClient> ConnectAudioAsync(ulong channelId, bool selfDeaf, bool selfMute, bool external, bool disconnect = true)
	{
		base.Discord.EnsureGatewayIntent(GatewayIntents.GuildVoiceStates);
		await _audioLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		TaskCompletionSource<AudioClient> promise = default(TaskCompletionSource<AudioClient>);
		IAudioClient result = default(IAudioClient);
		try
		{
			if (disconnect || !external)
			{
				await DisconnectAudioInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			promise = new TaskCompletionSource<AudioClient>();
			_audioConnectPromise = promise;
			_voiceStateUpdateParams = new VoiceStateUpdateParams
			{
				GuildId = base.Id,
				ChannelId = channelId,
				SelfDeaf = selfDeaf,
				SelfMute = selfMute
			};
			if (external)
			{
				promise.TrySetResultAsync(null);
				await base.Discord.ApiClient.SendVoiceStateUpdateAsync(_voiceStateUpdateParams).ConfigureAwait(continueOnCapturedContext: false);
				result = null;
				return result;
			}
			if (_audioClient == null)
			{
				AudioClient audioClient = new AudioClient(this, base.Discord.GetAudioId(), channelId);
				audioClient.Disconnected += async delegate(Exception ex)
				{
					if (promise.Task.IsCompleted && audioClient.IsFinished)
					{
						try
						{
							audioClient.Dispose();
						}
						catch
						{
						}
						_audioClient = null;
						if (ex != null)
						{
							await promise.TrySetExceptionAsync(ex);
						}
						else
						{
							await promise.TrySetCanceledAsync();
						}
					}
				};
				audioClient.Connected += delegate
				{
					promise.TrySetResultAsync(_audioClient);
					return Task.CompletedTask;
				};
				_audioClient = audioClient;
			}
			await base.Discord.ApiClient.SendVoiceStateUpdateAsync(_voiceStateUpdateParams).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (object obj)
		{
			await DisconnectAudioInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			ExceptionDispatchInfo.Capture((obj as Exception) ?? throw obj).Throw();
		}
		finally
		{
			_audioLock.Release();
		}
		try
		{
			Task timeoutTask = Task.Delay(15000);
			if (await Task.WhenAny(promise.Task, timeoutTask).ConfigureAwait(continueOnCapturedContext: false) == timeoutTask)
			{
				throw new TimeoutException();
			}
			result = await promise.Task.ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
		catch (object obj2)
		{
			await DisconnectAudioAsync().ConfigureAwait(continueOnCapturedContext: false);
			ExceptionDispatchInfo.Capture((obj2 as Exception) ?? throw obj2).Throw();
		}
		return result;
	}

	internal async Task DisconnectAudioAsync()
	{
		await _audioLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await DisconnectAudioInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_audioLock.Release();
		}
	}

	private async Task DisconnectAudioInternalAsync()
	{
		_audioConnectPromise?.TrySetCanceledAsync();
		_audioConnectPromise = null;
		if (_audioClient != null)
		{
			await _audioClient.StopAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		await base.Discord.ApiClient.SendVoiceStateUpdateAsync(base.Id, null, selfDeaf: false, selfMute: false).ConfigureAwait(continueOnCapturedContext: false);
		_audioClient?.Dispose();
		_audioClient = null;
		_voiceStateUpdateParams = null;
	}

	internal async Task ModifyAudioAsync(ulong channelId, Action<AudioChannelProperties> func, RequestOptions options)
	{
		await _audioLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await ModifyAudioInternalAsync(channelId, func, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_audioLock.Release();
		}
	}

	private Task ModifyAudioInternalAsync(ulong channelId, Action<AudioChannelProperties> func, RequestOptions options)
	{
		if (_voiceStateUpdateParams == null || _voiceStateUpdateParams.ChannelId != channelId)
		{
			throw new InvalidOperationException("Cannot modify properties of not connected audio channel");
		}
		AudioChannelProperties audioChannelProperties = new AudioChannelProperties();
		func(audioChannelProperties);
		if (audioChannelProperties.SelfDeaf.IsSpecified)
		{
			_voiceStateUpdateParams.SelfDeaf = audioChannelProperties.SelfDeaf.Value;
		}
		if (audioChannelProperties.SelfMute.IsSpecified)
		{
			_voiceStateUpdateParams.SelfMute = audioChannelProperties.SelfMute.Value;
		}
		return base.Discord.ApiClient.SendVoiceStateUpdateAsync(_voiceStateUpdateParams, options);
	}

	internal async Task FinishConnectAudio(string url, string token)
	{
		SocketVoiceState voiceState = GetVoiceState(base.Discord.CurrentUser.Id).Value;
		await _audioLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			if (_audioClient == null)
			{
				return;
			}
			await RepopulateAudioStreamsAsync().ConfigureAwait(continueOnCapturedContext: false);
			if (_audioClient.ConnectionState != ConnectionState.Disconnected)
			{
				try
				{
					await _audioClient.WaitForDisconnectAsync(TimeSpan.FromSeconds(5.0)).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (TimeoutException)
				{
					await base.Discord.LogManager.WarningAsync("Failed to wait for disconnect audio client in time", (Exception)null).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			await Task.Delay(TimeSpan.FromMilliseconds(5.0)).ConfigureAwait(continueOnCapturedContext: false);
			await _audioClient.StartAsync(url, base.Discord.CurrentUser.Id, voiceState.VoiceSessionId, token).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			await DisconnectAudioInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex3)
		{
			await _audioConnectPromise.SetExceptionAsync(ex3).ConfigureAwait(continueOnCapturedContext: false);
			await DisconnectAudioInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_audioLock.Release();
		}
	}

	internal async Task RepopulateAudioStreamsAsync()
	{
		await _audioClient.ClearInputStreamsAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (CurrentUser.VoiceChannel == null)
		{
			return;
		}
		foreach (KeyValuePair<ulong, SocketVoiceState> voiceState in _voiceStates)
		{
			if (voiceState.Value.VoiceChannel?.Id == CurrentUser.VoiceChannel?.Id && voiceState.Key != CurrentUser.Id)
			{
				await _audioClient.CreateInputStreamAsync(voiceState.Key).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public override string ToString()
	{
		return Name;
	}

	internal SocketGuild Clone()
	{
		return MemberwiseClone() as SocketGuild;
	}

	internal SocketAutoModRule AddOrUpdateAutoModRule(AutoModerationRule model)
	{
		if (_automodRules.TryGetValue(model.Id, out var value))
		{
			value.Update(model);
			return value;
		}
		SocketAutoModRule socketAutoModRule = SocketAutoModRule.Create(base.Discord, this, model);
		_automodRules.TryAdd(model.Id, socketAutoModRule);
		return socketAutoModRule;
	}

	public SocketAutoModRule GetAutoModRule(ulong id)
	{
		if (!_automodRules.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	internal SocketAutoModRule RemoveAutoModRule(ulong id)
	{
		if (!_automodRules.TryRemove(id, out var value))
		{
			return null;
		}
		return value;
	}

	internal SocketAutoModRule RemoveAutoModRule(AutoModerationRule model)
	{
		if (_automodRules.TryRemove(model.Id, out var value))
		{
			value.Update(model);
		}
		return value ?? SocketAutoModRule.Create(base.Discord, this, model);
	}

	public async Task<SocketAutoModRule> GetAutoModRuleAsync(ulong ruleId, RequestOptions options = null)
	{
		return AddOrUpdateAutoModRule(await GuildHelper.GetAutoModRuleAsync(ruleId, this, base.Discord, options));
	}

	public async Task<SocketAutoModRule[]> GetAutoModRulesAsync(RequestOptions options = null)
	{
		return (await GuildHelper.GetAutoModRulesAsync(this, base.Discord, options)).Select(AddOrUpdateAutoModRule).ToArray();
	}

	public async Task<SocketAutoModRule> CreateAutoModRuleAsync(Action<AutoModRuleProperties> props, RequestOptions options = null)
	{
		return AddOrUpdateAutoModRule(await GuildHelper.CreateAutoModRuleAsync(this, props, base.Discord, options));
	}

	public async Task<SocketGuildOnboarding> GetOnboardingAsync(RequestOptions options = null)
	{
		GuildOnboarding model = await GuildHelper.GetGuildOnboardingAsync(this, base.Discord, options);
		return new SocketGuildOnboarding(base.Discord, model, this);
	}

	public async Task<SocketGuildOnboarding> ModifyOnboardingAsync(Action<GuildOnboardingProperties> props, RequestOptions options = null)
	{
		GuildOnboarding model = await GuildHelper.ModifyGuildOnboardingAsync(this, props, base.Discord, options);
		return new SocketGuildOnboarding(base.Discord, model, this);
	}

	async Task<IGuildScheduledEvent> IGuild.CreateEventAsync(string name, DateTimeOffset startTime, GuildScheduledEventType type, GuildScheduledEventPrivacyLevel privacyLevel, string description, DateTimeOffset? endTime, ulong? channelId, string location, Image? coverImage, RequestOptions options)
	{
		return await CreateEventAsync(name, startTime, type, privacyLevel, description, endTime, channelId, location, coverImage, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IGuildScheduledEvent> IGuild.GetEventAsync(ulong id, RequestOptions options)
	{
		return await GetEventAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IGuildScheduledEvent>> IGuild.GetEventsAsync(RequestOptions options)
	{
		return await GetEventsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	IAsyncEnumerable<IReadOnlyCollection<IBan>> IGuild.GetBansAsync(int limit, RequestOptions options)
	{
		return GetBansAsync(limit, options);
	}

	IAsyncEnumerable<IReadOnlyCollection<IBan>> IGuild.GetBansAsync(ulong fromUserId, Direction dir, int limit, RequestOptions options)
	{
		return GetBansAsync(fromUserId, dir, limit, options);
	}

	IAsyncEnumerable<IReadOnlyCollection<IBan>> IGuild.GetBansAsync(IUser fromUser, Direction dir, int limit, RequestOptions options)
	{
		return GetBansAsync(fromUser, dir, limit, options);
	}

	async Task<IBan> IGuild.GetBanAsync(IUser user, RequestOptions options)
	{
		return await GetBanAsync(user, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IBan> IGuild.GetBanAsync(ulong userId, RequestOptions options)
	{
		return await GetBanAsync(userId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IReadOnlyCollection<IGuildChannel>> IGuild.GetChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGuildChannel>)Channels);
	}

	Task<IGuildChannel> IGuild.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuildChannel)GetChannel(id));
	}

	Task<IReadOnlyCollection<ITextChannel>> IGuild.GetTextChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<ITextChannel>)TextChannels);
	}

	Task<ITextChannel> IGuild.GetTextChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((ITextChannel)GetTextChannel(id));
	}

	Task<IThreadChannel> IGuild.GetThreadChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IThreadChannel)GetThreadChannel(id));
	}

	Task<IReadOnlyCollection<IThreadChannel>> IGuild.GetThreadChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IThreadChannel>)ThreadChannels);
	}

	Task<IReadOnlyCollection<IVoiceChannel>> IGuild.GetVoiceChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IVoiceChannel>)VoiceChannels);
	}

	Task<IReadOnlyCollection<ICategoryChannel>> IGuild.GetCategoriesAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<ICategoryChannel>)CategoryChannels);
	}

	Task<IVoiceChannel> IGuild.GetVoiceChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IVoiceChannel)GetVoiceChannel(id));
	}

	Task<IStageChannel> IGuild.GetStageChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IStageChannel)GetStageChannel(id));
	}

	Task<IReadOnlyCollection<IStageChannel>> IGuild.GetStageChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IStageChannel>)StageChannels);
	}

	Task<IVoiceChannel> IGuild.GetAFKChannelAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IVoiceChannel)AFKChannel);
	}

	Task<ITextChannel> IGuild.GetDefaultChannelAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((ITextChannel)DefaultChannel);
	}

	Task<IGuildChannel> IGuild.GetWidgetChannelAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuildChannel)WidgetChannel);
	}

	Task<ITextChannel> IGuild.GetSystemChannelAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((ITextChannel)SystemChannel);
	}

	Task<ITextChannel> IGuild.GetRulesChannelAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((ITextChannel)RulesChannel);
	}

	Task<ITextChannel> IGuild.GetPublicUpdatesChannelAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((ITextChannel)PublicUpdatesChannel);
	}

	Task<IForumChannel> IGuild.GetForumChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IForumChannel)GetForumChannel(id));
	}

	Task<IReadOnlyCollection<IForumChannel>> IGuild.GetForumChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IForumChannel>)ForumChannels);
	}

	Task<IMediaChannel> IGuild.GetMediaChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IMediaChannel)GetMediaChannel(id));
	}

	Task<IReadOnlyCollection<IMediaChannel>> IGuild.GetMediaChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IMediaChannel>)MediaChannels);
	}

	async Task<ITextChannel> IGuild.CreateTextChannelAsync(string name, Action<TextChannelProperties> func, RequestOptions options)
	{
		return await CreateTextChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<INewsChannel> IGuild.CreateNewsChannelAsync(string name, Action<TextChannelProperties> func, RequestOptions options)
	{
		return await CreateNewsChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IVoiceChannel> IGuild.CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func, RequestOptions options)
	{
		return await CreateVoiceChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IStageChannel> IGuild.CreateStageChannelAsync(string name, Action<VoiceChannelProperties> func, RequestOptions options)
	{
		return await CreateStageChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<ICategoryChannel> IGuild.CreateCategoryAsync(string name, Action<GuildChannelProperties> func, RequestOptions options)
	{
		return await CreateCategoryChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IForumChannel> IGuild.CreateForumChannelAsync(string name, Action<ForumChannelProperties> func, RequestOptions options)
	{
		return await CreateForumChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IMediaChannel> IGuild.CreateMediaChannelAsync(string name, Action<ForumChannelProperties> func, RequestOptions options)
	{
		return await CreateMediaChannelAsync(name, func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IVoiceRegion>> IGuild.GetVoiceRegionsAsync(RequestOptions options)
	{
		return await GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IIntegration>> IGuild.GetIntegrationsAsync(RequestOptions options)
	{
		return await GetIntegrationsAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task IGuild.DeleteIntegrationAsync(ulong id, RequestOptions options)
	{
		await DeleteIntegrationAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IInviteMetadata>> IGuild.GetInvitesAsync(RequestOptions options)
	{
		return await GetInvitesAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IInviteMetadata> IGuild.GetVanityInviteAsync(RequestOptions options)
	{
		return await GetVanityInviteAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	IRole IGuild.GetRole(ulong id)
	{
		return GetRole(id);
	}

	public Task<RestRole> GetRoleAsync(ulong id, RequestOptions options = null)
	{
		return GuildHelper.GetRoleAsync(this, base.Discord, id, options);
	}

	async Task<IRole> IGuild.GetRoleAsync(ulong id, RequestOptions options)
	{
		return await GetRoleAsync(id);
	}

	async Task<IRole> IGuild.CreateRoleAsync(string name, GuildPermissions? permissions, Color? color, bool isHoisted, RequestOptions options)
	{
		return await CreateRoleAsync(name, permissions, color, isHoisted, isMentionable: false, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IRole> IGuild.CreateRoleAsync(string name, GuildPermissions? permissions, Color? color, bool isHoisted, bool isMentionable, RequestOptions options, Image? icon, Emoji emoji)
	{
		return await CreateRoleAsync(name, permissions, color, isHoisted, isMentionable, options, icon, emoji).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IGuildUser>> IGuild.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload && !HasAllMembers)
		{
			return (await GetUsersAsync(options).FlattenAsync().ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArray();
		}
		return Users;
	}

	async Task<IGuildUser> IGuild.AddGuildUserAsync(ulong userId, string accessToken, Action<AddGuildUserProperties> func, RequestOptions options)
	{
		return await AddGuildUserAsync(userId, accessToken, func, options);
	}

	async Task<IGuildUser> IGuild.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		SocketGuildUser user = GetUser(id);
		if (user != null || mode == CacheMode.CacheOnly)
		{
			return user;
		}
		return await GuildHelper.GetUserAsync(this, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task<IGuildUser> IGuild.GetCurrentUserAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuildUser)CurrentUser);
	}

	Task<IGuildUser> IGuild.GetOwnerAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuildUser)Owner);
	}

	async Task<IReadOnlyCollection<IGuildUser>> IGuild.SearchUsersAsync(string query, int limit, CacheMode mode, RequestOptions options)
	{
		if (mode == CacheMode.AllowDownload)
		{
			return await SearchUsersAsync(query, limit, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		return ImmutableArray.Create<IGuildUser>();
	}

	async Task<IReadOnlyCollection<IAuditLogEntry>> IGuild.GetAuditLogsAsync(int limit, CacheMode cacheMode, RequestOptions options, ulong? beforeId, ulong? userId, ActionType? actionType, ulong? afterId)
	{
		if (cacheMode == CacheMode.AllowDownload)
		{
			return (await GetAuditLogsAsync(limit, options, beforeId, userId, actionType, afterId).FlattenAsync().ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArray();
		}
		return ImmutableArray.Create<IAuditLogEntry>();
	}

	async Task<IWebhook> IGuild.GetWebhookAsync(ulong id, RequestOptions options)
	{
		return await GetWebhookAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IWebhook>> IGuild.GetWebhooksAsync(RequestOptions options)
	{
		return await GetWebhooksAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IReadOnlyCollection<IApplicationCommand>> IGuild.GetApplicationCommandsAsync(bool withLocalizations, string locale, RequestOptions options)
	{
		return await GetApplicationCommandsAsync(withLocalizations, locale, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<ICustomSticker> IGuild.CreateStickerAsync(string name, Image image, IEnumerable<string> tags, string description, RequestOptions options)
	{
		return await CreateStickerAsync(name, image, tags, description, options);
	}

	async Task<ICustomSticker> IGuild.CreateStickerAsync(string name, Stream stream, string filename, IEnumerable<string> tags, string description, RequestOptions options)
	{
		return await CreateStickerAsync(name, stream, filename, tags, description, options);
	}

	async Task<ICustomSticker> IGuild.CreateStickerAsync(string name, string path, IEnumerable<string> tags, string description, RequestOptions options)
	{
		return await CreateStickerAsync(name, path, tags, description, options);
	}

	async Task<ICustomSticker> IGuild.GetStickerAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return await GetStickerAsync(id, mode, options);
	}

	async Task<IReadOnlyCollection<ICustomSticker>> IGuild.GetStickersAsync(CacheMode mode, RequestOptions options)
	{
		return await GetStickersAsync(mode, options);
	}

	Task IGuild.DeleteStickerAsync(ICustomSticker sticker, RequestOptions options)
	{
		return DeleteStickerAsync(_stickers[sticker.Id], options);
	}

	async Task<IApplicationCommand> IGuild.GetApplicationCommandAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return await GetApplicationCommandAsync(id, mode, options);
	}

	async Task<IApplicationCommand> IGuild.CreateApplicationCommandAsync(ApplicationCommandProperties properties, RequestOptions options)
	{
		return await CreateApplicationCommandAsync(properties, options);
	}

	async Task<IReadOnlyCollection<IApplicationCommand>> IGuild.BulkOverwriteApplicationCommandsAsync(ApplicationCommandProperties[] properties, RequestOptions options)
	{
		return await BulkOverwriteApplicationCommandAsync(properties, options);
	}

	public Task<WelcomeScreen> GetWelcomeScreenAsync(RequestOptions options = null)
	{
		return GuildHelper.GetWelcomeScreenAsync(this, base.Discord, options);
	}

	public Task<WelcomeScreen> ModifyWelcomeScreenAsync(bool enabled, WelcomeScreenChannelProperties[] channels, string description = null, RequestOptions options = null)
	{
		return GuildHelper.ModifyWelcomeScreenAsync(enabled, description, channels, this, base.Discord, options);
	}

	void IDisposable.Dispose()
	{
		DisconnectAudioAsync().GetAwaiter().GetResult();
		_audioLock?.Dispose();
		_audioClient?.Dispose();
	}

	async Task<IAutoModRule> IGuild.GetAutoModRuleAsync(ulong ruleId, RequestOptions options)
	{
		return await GetAutoModRuleAsync(ruleId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IAutoModRule[]> IGuild.GetAutoModRulesAsync(RequestOptions options)
	{
		return await GetAutoModRulesAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IAutoModRule> IGuild.CreateAutoModRuleAsync(Action<AutoModRuleProperties> props, RequestOptions options)
	{
		return await CreateAutoModRuleAsync(props, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IGuildOnboarding> IGuild.GetOnboardingAsync(RequestOptions options)
	{
		return await GetOnboardingAsync(options);
	}

	async Task<IGuildOnboarding> IGuild.ModifyOnboardingAsync(Action<GuildOnboardingProperties> props, RequestOptions options)
	{
		return await ModifyOnboardingAsync(props, options);
	}
}
