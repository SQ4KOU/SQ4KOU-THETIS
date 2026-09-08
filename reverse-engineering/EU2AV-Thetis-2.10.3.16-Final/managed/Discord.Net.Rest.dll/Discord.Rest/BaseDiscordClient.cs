using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.Logging;
using Discord.Net;

namespace Discord.Rest;

public abstract class BaseDiscordClient : IDiscordClient, IDisposable, IAsyncDisposable
{
	internal readonly AsyncEvent<Func<LogMessage, Task>> _logEvent = new AsyncEvent<Func<LogMessage, Task>>();

	private readonly AsyncEvent<Func<Task>> _loggedInEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<Task>> _loggedOutEvent = new AsyncEvent<Func<Task>>();

	internal readonly AsyncEvent<Func<string, string, double, Task>> _sentRequest = new AsyncEvent<Func<string, string, double, Task>>();

	internal readonly Logger _restLogger;

	private readonly SemaphoreSlim _stateLock;

	private bool _isFirstLogin;

	private bool _isDisposed;

	internal DiscordRestApiClient ApiClient { get; }

	internal LogManager LogManager { get; }

	public LoginState LoginState { get; private set; }

	public ISelfUser CurrentUser { get; protected set; }

	public TokenType TokenType => ApiClient.AuthTokenType;

	internal bool UseInteractionSnowflakeDate { get; private set; }

	internal bool FormatUsersInBidirectionalUnicode { get; private set; }

	internal bool ResponseInternalTimeCheck { get; private set; }

	public virtual ConnectionState ConnectionState => ConnectionState.Disconnected;

	ISelfUser IDiscordClient.CurrentUser => CurrentUser;

	public event Func<LogMessage, Task> Log
	{
		add
		{
			_logEvent.Add(value);
		}
		remove
		{
			_logEvent.Remove(value);
		}
	}

	public event Func<Task> LoggedIn
	{
		add
		{
			_loggedInEvent.Add(value);
		}
		remove
		{
			_loggedInEvent.Remove(value);
		}
	}

	public event Func<Task> LoggedOut
	{
		add
		{
			_loggedOutEvent.Add(value);
		}
		remove
		{
			_loggedOutEvent.Remove(value);
		}
	}

	public event Func<string, string, double, Task> SentRequest
	{
		add
		{
			_sentRequest.Add(value);
		}
		remove
		{
			_sentRequest.Remove(value);
		}
	}

	internal BaseDiscordClient(DiscordRestConfig config, DiscordRestApiClient client)
	{
		ApiClient = client;
		LogManager = new LogManager(config.LogLevel);
		LogManager.Message += async delegate(LogMessage msg)
		{
			await _logEvent.InvokeAsync(msg).ConfigureAwait(continueOnCapturedContext: false);
		};
		_stateLock = new SemaphoreSlim(1, 1);
		_restLogger = LogManager.CreateLogger("Rest");
		_isFirstLogin = config.DisplayInitialLog;
		UseInteractionSnowflakeDate = config.UseInteractionSnowflakeDate;
		FormatUsersInBidirectionalUnicode = config.FormatUsersInBidirectionalUnicode;
		ResponseInternalTimeCheck = config.ResponseInternalTimeCheck;
		ApiClient.RequestQueue.RateLimitTriggered += async delegate(BucketId id, RateLimitInfo? info, string endpoint)
		{
			if (!info.HasValue)
			{
				await _restLogger.VerboseAsync("Preemptive Rate limit triggered: " + endpoint + " " + (id.IsHashBucket ? ("(Bucket: " + id.BucketHash + ")") : "")).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await _restLogger.WarningAsync(string.Format("Rate limit triggered: {0} Remaining: {1}s {2}", endpoint, info.Value.RetryAfter, id.IsHashBucket ? ("(Bucket: " + id.BucketHash + ")") : "")).ConfigureAwait(continueOnCapturedContext: false);
			}
		};
		ApiClient.SentRequest += async delegate(string method, string endpoint, double millis)
		{
			await _restLogger.VerboseAsync($"{method} {endpoint}: {millis} ms").ConfigureAwait(continueOnCapturedContext: false);
		};
		ApiClient.SentRequest += (string method, string endpoint, double millis) => _sentRequest.InvokeAsync(method, endpoint, millis);
	}

	public async Task LoginAsync(TokenType tokenType, string token, bool validateToken = true)
	{
		await _stateLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await LoginInternalAsync(tokenType, token, validateToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_stateLock.Release();
		}
	}

	internal virtual async Task LoginInternalAsync(TokenType tokenType, string token, bool validateToken)
	{
		if (_isFirstLogin)
		{
			_isFirstLogin = false;
			await LogManager.WriteInitialLog().ConfigureAwait(continueOnCapturedContext: false);
		}
		if (LoginState != LoginState.LoggedOut)
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		LoginState = LoginState.LoggingIn;
		try
		{
			if (validateToken)
			{
				try
				{
					TokenUtils.ValidateToken(tokenType, token);
				}
				catch (ArgumentException ex)
				{
					await LogManager.WarningAsync("Discord", "A supplied token was invalid.", ex).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			await ApiClient.LoginAsync(tokenType, token).ConfigureAwait(continueOnCapturedContext: false);
			await OnLoginAsync(tokenType, token).ConfigureAwait(continueOnCapturedContext: false);
			LoginState = LoginState.LoggedIn;
		}
		catch (object obj)
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			ExceptionDispatchInfo.Capture((obj as Exception) ?? throw obj).Throw();
		}
		await _loggedInEvent.InvokeAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	internal virtual Task OnLoginAsync(TokenType tokenType, string token)
	{
		return Task.CompletedTask;
	}

	public async Task LogoutAsync()
	{
		await _stateLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_stateLock.Release();
		}
	}

	internal virtual async Task LogoutInternalAsync()
	{
		if (LoginState != LoginState.LoggedOut)
		{
			LoginState = LoginState.LoggingOut;
			await ApiClient.LogoutAsync().ConfigureAwait(continueOnCapturedContext: false);
			await OnLogoutAsync().ConfigureAwait(continueOnCapturedContext: false);
			CurrentUser = null;
			LoginState = LoginState.LoggedOut;
			await _loggedOutEvent.InvokeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal virtual Task OnLogoutAsync()
	{
		return Task.CompletedTask;
	}

	internal virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			ApiClient.Dispose();
			_stateLock?.Dispose();
			_isDisposed = true;
		}
	}

	internal virtual async ValueTask DisposeAsync(bool disposing)
	{
		if (!_isDisposed)
		{
			await ApiClient.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			_stateLock?.Dispose();
			_isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public ValueTask DisposeAsync()
	{
		return DisposeAsync(disposing: true);
	}

	public Task<int> GetRecommendedShardCountAsync(RequestOptions options = null)
	{
		return ClientHelper.GetRecommendShardCountAsync(this, options);
	}

	public Task<BotGateway> GetBotGatewayAsync(RequestOptions options = null)
	{
		return ClientHelper.GetBotGatewayAsync(this, options);
	}

	Task<IApplication> IDiscordClient.GetApplicationInfoAsync(RequestOptions options)
	{
		throw new NotSupportedException();
	}

	Task<IChannel> IDiscordClient.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IChannel>(null);
	}

	Task<IReadOnlyCollection<IPrivateChannel>> IDiscordClient.GetPrivateChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IPrivateChannel>)ImmutableArray.Create<IPrivateChannel>());
	}

	Task<IReadOnlyCollection<IDMChannel>> IDiscordClient.GetDMChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IDMChannel>)ImmutableArray.Create<IDMChannel>());
	}

	Task<IReadOnlyCollection<IGroupChannel>> IDiscordClient.GetGroupChannelsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGroupChannel>)ImmutableArray.Create<IGroupChannel>());
	}

	Task<IReadOnlyCollection<IConnection>> IDiscordClient.GetConnectionsAsync(RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IConnection>)ImmutableArray.Create<IConnection>());
	}

	Task<IInvite> IDiscordClient.GetInviteAsync(string inviteId, RequestOptions options)
	{
		return Task.FromResult<IInvite>(null);
	}

	Task<IGuild> IDiscordClient.GetGuildAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IGuild>(null);
	}

	Task<IReadOnlyCollection<IGuild>> IDiscordClient.GetGuildsAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IGuild>)ImmutableArray.Create<IGuild>());
	}

	Task<IGuild> IDiscordClient.CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
	{
		throw new NotSupportedException();
	}

	Task<IUser> IDiscordClient.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IUser>(null);
	}

	Task<IUser> IDiscordClient.GetUserAsync(string username, string discriminator, RequestOptions options)
	{
		return Task.FromResult<IUser>(null);
	}

	Task<IReadOnlyCollection<IVoiceRegion>> IDiscordClient.GetVoiceRegionsAsync(RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IVoiceRegion>)ImmutableArray.Create<IVoiceRegion>());
	}

	Task<IVoiceRegion> IDiscordClient.GetVoiceRegionAsync(string id, RequestOptions options)
	{
		return Task.FromResult<IVoiceRegion>(null);
	}

	Task<IWebhook> IDiscordClient.GetWebhookAsync(ulong id, RequestOptions options)
	{
		return Task.FromResult<IWebhook>(null);
	}

	Task<IApplicationCommand> IDiscordClient.GetGlobalApplicationCommandAsync(ulong id, RequestOptions options)
	{
		return Task.FromResult<IApplicationCommand>(null);
	}

	Task<IReadOnlyCollection<IApplicationCommand>> IDiscordClient.GetGlobalApplicationCommandsAsync(bool withLocalizations, string locale, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IApplicationCommand>)ImmutableArray.Create<IApplicationCommand>());
	}

	Task<IApplicationCommand> IDiscordClient.CreateGlobalApplicationCommand(ApplicationCommandProperties properties, RequestOptions options)
	{
		return Task.FromResult<IApplicationCommand>(null);
	}

	Task<IReadOnlyCollection<IApplicationCommand>> IDiscordClient.BulkOverwriteGlobalApplicationCommand(ApplicationCommandProperties[] properties, RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<IApplicationCommand>)ImmutableArray.Create<IApplicationCommand>());
	}

	Task IDiscordClient.StartAsync()
	{
		return Task.CompletedTask;
	}

	Task IDiscordClient.StopAsync()
	{
		return Task.CompletedTask;
	}

	Task<IEntitlement> IDiscordClient.CreateTestEntitlementAsync(ulong skuId, ulong ownerId, SubscriptionOwnerType ownerType, RequestOptions options)
	{
		return Task.FromResult<IEntitlement>(null);
	}

	Task IDiscordClient.DeleteTestEntitlementAsync(ulong entitlementId, RequestOptions options)
	{
		return Task.CompletedTask;
	}

	IAsyncEnumerable<IReadOnlyCollection<IEntitlement>> IDiscordClient.GetEntitlementsAsync(int limit, ulong? afterId, ulong? beforeId, bool excludeEnded, ulong? guildId, ulong? userId, ulong[] skuIds, RequestOptions options, bool? excludeDeleted)
	{
		return AsyncEnumerable.Empty<IReadOnlyCollection<IEntitlement>>();
	}

	Task<IReadOnlyCollection<SKU>> IDiscordClient.GetSKUsAsync(RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<SKU>)Array.Empty<SKU>());
	}

	Task IDiscordClient.ConsumeEntitlementAsync(ulong entitlementId, RequestOptions options)
	{
		return Task.CompletedTask;
	}

	IAsyncEnumerable<IReadOnlyCollection<ISubscription>> IDiscordClient.GetSKUSubscriptionsAsync(ulong skuId, int limit, ulong? afterId, ulong? beforeId, ulong? userId, RequestOptions options)
	{
		return AsyncEnumerable.Empty<IReadOnlyCollection<ISubscription>>();
	}

	Task<ISubscription> IDiscordClient.GetSKUSubscriptionAsync(ulong skuId, ulong subscriptionId, RequestOptions options)
	{
		return Task.FromResult<ISubscription>(null);
	}

	Task<Emote> IDiscordClient.GetApplicationEmoteAsync(ulong emoteId, RequestOptions options)
	{
		return Task.FromResult<Emote>(null);
	}

	Task<IReadOnlyCollection<Emote>> IDiscordClient.GetApplicationEmotesAsync(RequestOptions options)
	{
		return Task.FromResult((IReadOnlyCollection<Emote>)ImmutableArray.Create<Emote>());
	}

	Task<Emote> IDiscordClient.ModifyApplicationEmoteAsync(ulong emoteId, Action<ApplicationEmoteProperties> args, RequestOptions options)
	{
		return Task.FromResult<Emote>(null);
	}

	Task<Emote> IDiscordClient.CreateApplicationEmoteAsync(string name, Image image, RequestOptions options)
	{
		return Task.FromResult<Emote>(null);
	}

	Task IDiscordClient.DeleteApplicationEmoteAsync(ulong emoteId, RequestOptions options)
	{
		return Task.CompletedTask;
	}
}
