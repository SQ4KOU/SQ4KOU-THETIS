using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.API.Rest;
using Discord.Net;
using Discord.Net.Converters;
using Discord.Net.Queue;
using Discord.Net.Rest;
using Newtonsoft.Json;

namespace Discord.API;

internal class DiscordRestApiClient : IDisposable, IAsyncDisposable
{
	internal class BucketIds
	{
		public ulong GuildId { get; internal set; }

		public ulong ChannelId { get; internal set; }

		public ulong WebhookId { get; internal set; }

		public string HttpMethod { get; internal set; }

		internal BucketIds(ulong guildId = 0uL, ulong channelId = 0uL, ulong webhookId = 0uL)
		{
			GuildId = guildId;
			ChannelId = channelId;
			WebhookId = webhookId;
		}

		internal object[] ToArray()
		{
			return new object[4] { HttpMethod, GuildId, ChannelId, WebhookId };
		}

		internal Dictionary<string, string> ToMajorParametersDictionary()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (GuildId != 0L)
			{
				dictionary["GuildId"] = GuildId.ToString();
			}
			if (ChannelId != 0L)
			{
				dictionary["ChannelId"] = ChannelId.ToString();
			}
			if (WebhookId != 0L)
			{
				dictionary["WebhookId"] = WebhookId.ToString();
			}
			return dictionary;
		}

		internal static int? GetIndex(string name)
		{
			return name switch
			{
				"httpMethod" => 0, 
				"guildId" => 1, 
				"channelId" => 2, 
				"webhookId" => 3, 
				_ => null, 
			};
		}
	}

	private static readonly ConcurrentDictionary<string, Func<BucketIds, BucketId>> _bucketIdGenerators = new ConcurrentDictionary<string, Func<BucketIds, BucketId>>();

	private readonly AsyncEvent<Func<string, string, double, Task>> _sentRequestEvent = new AsyncEvent<Func<string, string, double, Task>>();

	protected readonly JsonSerializer _serializer;

	protected readonly SemaphoreSlim _stateLock;

	private readonly RestClientProvider _restClientProvider;

	protected bool _isDisposed;

	private CancellationTokenSource _loginCancelToken;

	public RetryMode DefaultRetryMode { get; }

	public string UserAgent { get; }

	internal RequestQueue RequestQueue { get; }

	public LoginState LoginState { get; private set; }

	public TokenType AuthTokenType { get; private set; }

	internal string AuthToken { get; private set; }

	internal IRestClient RestClient { get; private set; }

	internal ulong? CurrentUserId { get; set; }

	internal ulong? CurrentApplicationId { get; set; }

	internal bool UseSystemClock { get; set; }

	internal Func<IRateLimitInfo, Task> DefaultRatelimitCallback { get; set; }

	internal JsonSerializer Serializer => _serializer;

	public event Func<string, string, double, Task> SentRequest
	{
		add
		{
			_sentRequestEvent.Add(value);
		}
		remove
		{
			_sentRequestEvent.Remove(value);
		}
	}

	public DiscordRestApiClient(RestClientProvider restClientProvider, string userAgent, RetryMode defaultRetryMode = RetryMode.AlwaysRetry, JsonSerializer serializer = null, bool useSystemClock = true, Func<IRateLimitInfo, Task> defaultRatelimitCallback = null)
	{
		_restClientProvider = restClientProvider;
		UserAgent = userAgent;
		DefaultRetryMode = defaultRetryMode;
		_serializer = serializer ?? new JsonSerializer
		{
			ContractResolver = new DiscordContractResolver()
		};
		UseSystemClock = useSystemClock;
		DefaultRatelimitCallback = defaultRatelimitCallback;
		RequestQueue = new RequestQueue();
		_stateLock = new SemaphoreSlim(1, 1);
		SetBaseUrl(DiscordConfig.APIUrl);
	}

	internal void SetBaseUrl(string baseUrl)
	{
		RestClient?.Dispose();
		RestClient = _restClientProvider(baseUrl);
		RestClient.SetHeader("accept", "*/*");
		RestClient.SetHeader("user-agent", UserAgent);
		RestClient.SetHeader("authorization", GetPrefixedToken(AuthTokenType, AuthToken));
	}

	internal static string GetPrefixedToken(TokenType tokenType, string token)
	{
		return tokenType switch
		{
			TokenType.Bot => "Bot " + token, 
			TokenType.Bearer => "Bearer " + token, 
			_ => throw new ArgumentException("Unknown OAuth token type.", "tokenType"), 
		};
	}

	internal virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_loginCancelToken?.Dispose();
				RestClient?.Dispose();
				RequestQueue?.Dispose();
				_stateLock?.Dispose();
			}
			_isDisposed = true;
		}
	}

	internal virtual async ValueTask DisposeAsync(bool disposing)
	{
		if (_isDisposed)
		{
			return;
		}
		if (disposing)
		{
			_loginCancelToken?.Dispose();
			RestClient?.Dispose();
			if (RequestQueue != null)
			{
				await RequestQueue.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			_stateLock?.Dispose();
		}
		_isDisposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public ValueTask DisposeAsync()
	{
		return DisposeAsync(disposing: true);
	}

	public async Task LoginAsync(TokenType tokenType, string token, RequestOptions options = null)
	{
		await _stateLock.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await LoginInternalAsync(tokenType, token, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_stateLock.Release();
		}
	}

	private async Task LoginInternalAsync(TokenType tokenType, string token, RequestOptions options = null)
	{
		if (LoginState != LoginState.LoggedOut)
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		LoginState = LoginState.LoggingIn;
		try
		{
			_loginCancelToken?.Dispose();
			_loginCancelToken = new CancellationTokenSource();
			AuthToken = null;
			await RequestQueue.SetCancelTokenAsync(_loginCancelToken.Token).ConfigureAwait(continueOnCapturedContext: false);
			RestClient.SetCancelToken(_loginCancelToken.Token);
			AuthTokenType = tokenType;
			AuthToken = token?.TrimEnd();
			if (tokenType != TokenType.Webhook)
			{
				RestClient.SetHeader("authorization", GetPrefixedToken(AuthTokenType, AuthToken));
			}
			LoginState = LoginState.LoggedIn;
		}
		catch (object obj)
		{
			await LogoutInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			ExceptionDispatchInfo.Capture((obj as Exception) ?? throw obj).Throw();
		}
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

	private async Task LogoutInternalAsync()
	{
		if (LoginState != LoginState.LoggedOut)
		{
			LoginState = LoginState.LoggingOut;
			try
			{
				_loginCancelToken?.Cancel(throwOnFirstException: false);
			}
			catch
			{
			}
			await DisconnectInternalAsync().ConfigureAwait(continueOnCapturedContext: false);
			await RequestQueue.ClearAsync().ConfigureAwait(continueOnCapturedContext: false);
			await RequestQueue.SetCancelTokenAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
			RestClient.SetCancelToken(CancellationToken.None);
			CurrentUserId = null;
			LoginState = LoginState.LoggedOut;
		}
	}

	internal virtual Task ConnectInternalAsync()
	{
		return Task.CompletedTask;
	}

	internal virtual Task DisconnectInternalAsync(Exception ex = null)
	{
		return Task.CompletedTask;
	}

	internal Task SendAsync(string method, Expression<Func<string>> endpointExpr, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null)
	{
		return SendAsync(method, GetEndpoint(endpointExpr), GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public Task SendAsync(string method, string endpoint, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null)
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.HeaderOnly = true;
		options.BucketId = bucketId;
		RestRequest request = new RestRequest(RestClient, method, endpoint, options);
		return SendInternalAsync(method, endpoint, request);
	}

	internal Task SendJsonAsync(string method, Expression<Func<string>> endpointExpr, object payload, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null)
	{
		return SendJsonAsync(method, GetEndpoint(endpointExpr), payload, GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public Task SendJsonAsync(string method, string endpoint, object payload, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null)
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.HeaderOnly = true;
		options.BucketId = bucketId;
		string json = ((payload != null) ? SerializeJson(payload) : null);
		JsonRestRequest request = new JsonRestRequest(RestClient, method, endpoint, json, options);
		return SendInternalAsync(method, endpoint, request);
	}

	internal Task SendMultipartAsync(string method, Expression<Func<string>> endpointExpr, IReadOnlyDictionary<string, object> multipartArgs, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null)
	{
		return SendMultipartAsync(method, GetEndpoint(endpointExpr), multipartArgs, GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public Task SendMultipartAsync(string method, string endpoint, IReadOnlyDictionary<string, object> multipartArgs, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null)
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.HeaderOnly = true;
		options.BucketId = bucketId;
		MultipartRestRequest request = new MultipartRestRequest(RestClient, method, endpoint, multipartArgs, options);
		return SendInternalAsync(method, endpoint, request);
	}

	internal Task<TResponse> SendAsync<TResponse>(string method, Expression<Func<string>> endpointExpr, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null) where TResponse : class
	{
		return SendAsync<TResponse>(method, GetEndpoint(endpointExpr), GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task<TResponse> SendAsync<TResponse>(string method, string endpoint, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null) where TResponse : class
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.BucketId = bucketId;
		RestRequest request = new RestRequest(RestClient, method, endpoint, options);
		return DeserializeJson<TResponse>(await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal Task<TResponse> SendJsonAsync<TResponse>(string method, Expression<Func<string>> endpointExpr, object payload, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null) where TResponse : class
	{
		return SendJsonAsync<TResponse>(method, GetEndpoint(endpointExpr), payload, GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task<TResponse> SendJsonAsync<TResponse>(string method, string endpoint, object payload, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null) where TResponse : class
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.BucketId = bucketId;
		string json = ((payload != null) ? SerializeJson(payload) : null);
		JsonRestRequest request = new JsonRestRequest(RestClient, method, endpoint, json, options);
		return DeserializeJson<TResponse>(await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal Task<TResponse> SendMultipartAsync<TResponse>(string method, Expression<Func<string>> endpointExpr, IReadOnlyDictionary<string, object> multipartArgs, BucketIds ids, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null, [CallerMemberName] string funcName = null)
	{
		return SendMultipartAsync<TResponse>(method, GetEndpoint(endpointExpr), multipartArgs, GetBucketId(method, ids, endpointExpr, funcName), clientBucket, options);
	}

	public async Task<TResponse> SendMultipartAsync<TResponse>(string method, string endpoint, IReadOnlyDictionary<string, object> multipartArgs, BucketId bucketId = null, ClientBucketType clientBucket = ClientBucketType.Unbucketed, RequestOptions options = null)
	{
		if (options == null)
		{
			options = new RequestOptions();
		}
		options.BucketId = bucketId;
		MultipartRestRequest request = new MultipartRestRequest(RestClient, method, endpoint, multipartArgs, options);
		return DeserializeJson<TResponse>(await SendInternalAsync(method, endpoint, request).ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task<Stream> SendInternalAsync(string method, string endpoint, RestRequest request)
	{
		if (!request.Options.IgnoreState)
		{
			CheckState();
		}
		RequestOptions options = request.Options;
		RetryMode? retryMode = options.RetryMode;
		retryMode.GetValueOrDefault();
		if (!retryMode.HasValue)
		{
			RetryMode defaultRetryMode = DefaultRetryMode;
			options.RetryMode = defaultRetryMode;
		}
		options = request.Options;
		bool? useSystemClock = options.UseSystemClock;
		_ = useSystemClock == true;
		if (!useSystemClock.HasValue)
		{
			bool useSystemClock2 = UseSystemClock;
			options.UseSystemClock = useSystemClock2;
		}
		options = request.Options;
		if (options.RatelimitCallback == null)
		{
			options.RatelimitCallback = DefaultRatelimitCallback;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		Stream responseStream = await RequestQueue.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
		stopwatch.Stop();
		double arg = ToMilliseconds(stopwatch);
		await _sentRequestEvent.InvokeAsync(method, endpoint, arg).ConfigureAwait(continueOnCapturedContext: false);
		return responseStream;
	}

	public Task ValidateTokenAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync("GET", () => "auth/login", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ValidateTokenAsync");
	}

	public Task<GetGatewayResponse> GetGatewayAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<GetGatewayResponse>("GET", () => "gateway", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGatewayAsync");
	}

	public Task<GetBotGatewayResponse> GetBotGatewayAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<GetBotGatewayResponse>("GET", () => "gateway/bot", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetBotGatewayAsync");
	}

	public async Task<Channel> GetChannelAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(0uL, channelId, 0uL);
			return await SendAsync<Channel>("GET", () => $"channels/{channelId}", ids, ClientBucketType.Unbucketed, options, "GetChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<Channel> GetChannelAsync(ulong guildId, ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(0uL, channelId, 0uL);
			Channel channel = await SendAsync<Channel>("GET", () => $"channels/{channelId}", ids, ClientBucketType.Unbucketed, options, "GetChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
			if (!channel.GuildId.IsSpecified || channel.GuildId.Value != guildId)
			{
				return null;
			}
			return channel;
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<IReadOnlyCollection<Channel>> GetGuildChannelsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<IReadOnlyCollection<Channel>>("GET", () => $"guilds/{guildId}/channels", ids, ClientBucketType.Unbucketed, options, "GetGuildChannelsAsync");
	}

	public Task<Channel> CreateGuildChannelAsync(ulong guildId, CreateGuildChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Bitrate, 0, "Bitrate");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		Preconditions.AtMost(args.Name.Length, 100, "Name");
		Optional<string> topic = args.Topic;
		if (topic.IsSpecified && topic.Value != null)
		{
			Preconditions.AtMost(args.Topic.Value.Length, 1024, "Name");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<Channel>("POST", () => $"guilds/{guildId}/channels", args, ids, ClientBucketType.Unbucketed, options, "CreateGuildChannelAsync");
	}

	public Task<Channel> DeleteChannelAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync<Channel>("DELETE", () => $"channels/{channelId}", ids, ClientBucketType.Unbucketed, options, "DeleteChannelAsync");
	}

	public Task<Channel> ModifyGuildChannelAsync(ulong channelId, ModifyGuildChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Position, 0, "Position");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		if (args.Name.IsSpecified)
		{
			Preconditions.AtMost(args.Name.Value.Length, 100, "Name");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Channel>("PATCH", () => $"channels/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildChannelAsync");
	}

	public async Task<Channel> ModifyGuildChannelAsync(ulong channelId, ModifyTextChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Position, 0, "Position");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		if (args.Name.IsSpecified)
		{
			Preconditions.AtMost(args.Name.Value.Length, 100, "Name");
		}
		if (args.Topic.IsSpecified)
		{
			Preconditions.AtMost(args.Topic.Value.Length, 1024, "Name");
		}
		Preconditions.AtLeast(args.SlowModeInterval, 0, "SlowModeInterval");
		Preconditions.AtMost(args.SlowModeInterval, 21600, "SlowModeInterval");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendJsonAsync<Channel>("PATCH", () => $"channels/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildChannelAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<Channel> ModifyGuildChannelAsync(ulong channelId, ModifyVoiceChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Bitrate, 8000, "Bitrate");
		Preconditions.AtLeast(args.UserLimit, 0, "UserLimit");
		Preconditions.AtLeast(args.Position, 0, "Position");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Channel>("PATCH", () => $"channels/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildChannelAsync");
	}

	public Task ModifyGuildChannelsAsync(ulong guildId, IEnumerable<ModifyGuildChannelsParams> args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		ModifyGuildChannelsParams[] array = args.ToArray();
		switch (array.Length)
		{
		case 0:
			return Task.CompletedTask;
		case 1:
			return ModifyGuildChannelAsync(array[0].Id, new ModifyGuildChannelParams
			{
				Position = array[0].Position
			});
		default:
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return SendJsonAsync("PATCH", () => $"guilds/{guildId}/channels", array, ids, ClientBucketType.Unbucketed, options, "ModifyGuildChannelsAsync");
		}
		}
	}

	public async Task ModifyVoiceChannelStatusAsync(ulong channelId, string status, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		ModifyVoiceStatusParams payload = new ModifyVoiceStatusParams
		{
			Status = status
		};
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		await SendJsonAsync("PUT", () => $"channels/{channelId}/voice-status", payload, ids, ClientBucketType.Unbucketed, options, "ModifyVoiceChannelStatusAsync");
	}

	public Task<Channel> CreatePostAsync(ulong channelId, CreatePostParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Channel>("POST", () => $"channels/{channelId}/threads", args, ids, ClientBucketType.Unbucketed, options, "CreatePostAsync");
	}

	public Task<Channel> CreatePostAsync(ulong channelId, CreateMultipartPostAsync args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendMultipartAsync<Channel>("POST", () => $"channels/{channelId}/threads", args.ToDictionary(), ids, ClientBucketType.Unbucketed, options, "CreatePostAsync");
	}

	public Task<Channel> ModifyThreadAsync(ulong channelId, ModifyThreadParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Channel>("PATCH", () => $"channels/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyThreadAsync");
	}

	public Task<Channel> StartThreadAsync(ulong channelId, ulong messageId, StartThreadParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Channel>("POST", () => $"channels/{channelId}/messages/{messageId}/threads", args, ids, ClientBucketType.Unbucketed, options, "StartThreadAsync");
	}

	public Task<Channel> StartThreadAsync(ulong channelId, StartThreadParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Channel>("POST", () => $"channels/{channelId}/threads", args, ids, ClientBucketType.Unbucketed, options, "StartThreadAsync");
	}

	public Task JoinThreadAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("PUT", () => $"channels/{channelId}/thread-members/@me", ids, ClientBucketType.Unbucketed, options, "JoinThreadAsync");
	}

	public Task AddThreadMemberAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(userId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("PUT", () => $"channels/{channelId}/thread-members/{userId}", ids, ClientBucketType.Unbucketed, options, "AddThreadMemberAsync");
	}

	public Task LeaveThreadAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("DELETE", () => $"channels/{channelId}/thread-members/@me", ids, ClientBucketType.Unbucketed, options, "LeaveThreadAsync");
	}

	public Task RemoveThreadMemberAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(userId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("DELETE", () => $"channels/{channelId}/thread-members/{userId}", ids, ClientBucketType.Unbucketed, options, "RemoveThreadMemberAsync");
	}

	public async Task<ThreadMember[]> ListThreadMembersAsync(ulong channelId, ulong? after = null, int? limit = null, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		string query = "?with_member=true";
		if (limit.HasValue)
		{
			query += $"&limit={limit}";
		}
		if (after.HasValue)
		{
			query += $"&after={after}";
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return await SendAsync<ThreadMember[]>("GET", () => $"channels/{channelId}/thread-members{query}", ids, ClientBucketType.Unbucketed, options, "ListThreadMembersAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<ThreadMember> GetThreadMemberAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string query = "?with_member=true";
		return SendAsync<ThreadMember>("GET", () => $"channels/{channelId}/thread-members/{userId}{query}", ids, ClientBucketType.Unbucketed, options, "GetThreadMemberAsync");
	}

	public Task<ChannelThreads> GetActiveThreadsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<ChannelThreads>("GET", () => $"guilds/{guildId}/threads/active", ids, ClientBucketType.Unbucketed, options, "GetActiveThreadsAsync");
	}

	public Task<ChannelThreads> GetPublicArchivedThreadsAsync(ulong channelId, DateTimeOffset? before = null, int? limit = null, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string query = "";
		if (limit.HasValue)
		{
			string arg = WebUtility.UrlEncode(before.GetValueOrDefault(DateTimeOffset.UtcNow).ToString("O"));
			query = $"?before={arg}&limit={limit.Value}";
		}
		else if (before.HasValue)
		{
			string text = WebUtility.UrlEncode(before.Value.ToString("O"));
			query = "?before=" + text;
		}
		return SendAsync<ChannelThreads>("GET", () => $"channels/{channelId}/threads/archived/public{query}", ids, ClientBucketType.Unbucketed, options, "GetPublicArchivedThreadsAsync");
	}

	public Task<ChannelThreads> GetPrivateArchivedThreadsAsync(ulong channelId, DateTimeOffset? before = null, int? limit = null, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string query = "";
		if (limit.HasValue)
		{
			string arg = WebUtility.UrlEncode(before.GetValueOrDefault(DateTimeOffset.UtcNow).ToString("O"));
			query = $"?before={arg}&limit={limit.Value}";
		}
		else if (before.HasValue)
		{
			string text = WebUtility.UrlEncode(before.Value.ToString("O"));
			query = "?before=" + text;
		}
		return SendAsync<ChannelThreads>("GET", () => $"channels/{channelId}/threads/archived/private{query}", ids, ClientBucketType.Unbucketed, options, "GetPrivateArchivedThreadsAsync");
	}

	public Task<ChannelThreads> GetJoinedPrivateArchivedThreadsAsync(ulong channelId, DateTimeOffset? before = null, int? limit = null, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string query = "";
		if (limit.HasValue)
		{
			query = $"?before={SnowflakeUtils.ToSnowflake(before.GetValueOrDefault(DateTimeOffset.UtcNow))}&limit={limit.Value}";
		}
		else if (before.HasValue)
		{
			query = "?before=" + before.Value.ToString("O");
		}
		return SendAsync<ChannelThreads>("GET", () => $"channels/{channelId}/users/@me/threads/archived/private{query}", ids, ClientBucketType.Unbucketed, options, "GetJoinedPrivateArchivedThreadsAsync");
	}

	public Task<StageInstance> CreateStageInstanceAsync(CreateStageInstanceParams args, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendJsonAsync<StageInstance>("POST", () => "stage-instances", args, ids, ClientBucketType.Unbucketed, options, "CreateStageInstanceAsync");
	}

	public Task<StageInstance> ModifyStageInstanceAsync(ulong channelId, ModifyStageInstanceParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<StageInstance>("PATCH", () => $"stage-instances/{channelId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyStageInstanceAsync");
	}

	public async Task DeleteStageInstanceAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		try
		{
			await SendAsync("DELETE", () => $"stage-instances/{channelId}", ids, ClientBucketType.Unbucketed, options, "DeleteStageInstanceAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
		}
	}

	public async Task<StageInstance> GetStageInstanceAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		try
		{
			return await SendAsync<StageInstance>("POST", () => $"stage-instances/{channelId}", ids, ClientBucketType.Unbucketed, options, "GetStageInstanceAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task ModifyMyVoiceState(ulong guildId, ModifyVoiceStateParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendJsonAsync("PATCH", () => $"guilds/{guildId}/voice-states/@me", args, ids, ClientBucketType.Unbucketed, options, "ModifyMyVoiceState");
	}

	public Task ModifyUserVoiceState(ulong guildId, ulong userId, ModifyVoiceStateParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendJsonAsync("PATCH", () => $"guilds/{guildId}/voice-states/{userId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyUserVoiceState");
	}

	public Task AddRoleAsync(ulong guildId, ulong userId, ulong roleId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		Preconditions.NotEqual(roleId, guildId, "roleId", "The Everyone role cannot be added to a user.");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync("PUT", () => $"guilds/{guildId}/members/{userId}/roles/{roleId}", ids, ClientBucketType.Unbucketed, options, "AddRoleAsync");
	}

	public Task RemoveRoleAsync(ulong guildId, ulong userId, ulong roleId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		Preconditions.NotEqual(roleId, guildId, "roleId", "The Everyone role cannot be removed from a user.");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync("DELETE", () => $"guilds/{guildId}/members/{userId}/roles/{roleId}", ids, ClientBucketType.Unbucketed, options, "RemoveRoleAsync");
	}

	public Task<Role> GetRoleAsync(ulong guildId, ulong roleId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<Role>("GET", () => $"guilds/{guildId}/roles/{roleId}", ids, ClientBucketType.Unbucketed, options, "GetRoleAsync");
	}

	public async Task<Message> GetChannelMessageAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(0uL, channelId, 0uL);
			return await SendAsync<Message>("GET", () => $"channels/{channelId}/messages/{messageId}", ids, ClientBucketType.Unbucketed, options, "GetChannelMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<IReadOnlyCollection<Message>> GetChannelMessagesAsync(ulong channelId, GetChannelMessagesParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 100, "Limit");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(100);
		ulong? relativeId = (args.RelativeMessageId.IsSpecified ? new ulong?(args.RelativeMessageId.Value) : ((ulong?)null));
		string relativeDir = args.RelativeDirection.GetValueOrDefault(Direction.Before) switch
		{
			Direction.After => "after", 
			Direction.Around => "around", 
			_ => "before", 
		};
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		Expression<Func<string>> endpointExpr = ((!relativeId.HasValue) ? ((Expression<Func<string>>)(() => $"channels/{channelId}/messages?limit={limit}")) : ((Expression<Func<string>>)(() => $"channels/{channelId}/messages?limit={limit}&{relativeDir}={relativeId}")));
		return SendAsync<IReadOnlyCollection<Message>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetChannelMessagesAsync");
	}

	public Task<Message> CreateMessageAsync(ulong channelId, CreateMessageParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		if (args.Content.IsSpecified && args.Content.Value != null)
		{
			Preconditions.AtMost(args.Content.Value.Length, 2000, "Content", $"Message content is too long, length must be less or equal to {2000}.");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Message>("POST", () => $"channels/{channelId}/messages", args, ids, ClientBucketType.SendEdit, options, "CreateMessageAsync");
	}

	public Task<Message> CreateWebhookMessageAsync(ulong webhookId, CreateWebhookMessageParams args, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		if (args.Embeds.IsSpecified)
		{
			Preconditions.AtMost(args.Embeds.Value.Length, 10, "Embeds", "A max of 10 Embeds are allowed.");
		}
		if (args.Content.IsSpecified && args.Content.Value != null)
		{
			Preconditions.AtMost(args.Content.Value.Length, 2000, "Content", $"Message content is too long, length must be less or equal to {2000}.");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		return SendJsonAsync<Message>("POST", () => $"webhooks/{webhookId}/{AuthToken}?{WebhookQuery(wait: true, threadId)}", args, ids, ClientBucketType.SendEdit, options, "CreateWebhookMessageAsync");
	}

	public async Task ModifyWebhookMessageAsync(ulong webhookId, ulong messageId, ModifyWebhookMessageParams args, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		if (args.Embeds.IsSpecified)
		{
			Preconditions.AtMost(args.Embeds.Value.Length, 10, "Embeds", $"A max of {10} Embeds are allowed.");
		}
		if (args.Content.IsSpecified && args.Content.Value != null)
		{
			Preconditions.AtMost(args.Content.Value.Length, 2000, "Content", $"Message content is too long, length must be less or equal to {2000}.");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		await SendJsonAsync<Message>("PATCH", () => $"webhooks/{webhookId}/{AuthToken}/messages/{messageId}?{WebhookQuery(wait: false, threadId)}", args, ids, ClientBucketType.SendEdit, options, "ModifyWebhookMessageAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task ModifyWebhookMessageAsync(ulong webhookId, ulong messageId, UploadWebhookFileParams args, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		if (args.Embeds.IsSpecified)
		{
			Preconditions.AtMost(args.Embeds.Value.Length, 10, "Embeds", $"A max of {10} Embeds are allowed.");
		}
		if (args.Content.IsSpecified && args.Content.Value != null)
		{
			Preconditions.AtMost(args.Content.Value.Length, 2000, "Content", $"Message content is too long, length must be less or equal to {2000}.");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		return SendMultipartAsync<Message>("PATCH", () => $"webhooks/{webhookId}/{AuthToken}/messages/{messageId}?{WebhookQuery(wait: false, threadId)}", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "ModifyWebhookMessageAsync");
	}

	public Task DeleteWebhookMessageAsync(ulong webhookId, ulong messageId, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		return SendAsync("DELETE", () => $"webhooks/{webhookId}/{AuthToken}/messages/{messageId}?{WebhookQuery(wait: false, threadId)}", ids, ClientBucketType.Unbucketed, options, "DeleteWebhookMessageAsync");
	}

	public Task<Message> UploadFileAsync(ulong channelId, UploadFileParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		if (args.Embeds.IsSpecified)
		{
			Preconditions.AtMost(args.Embeds.Value.Length, 10, "Embeds", $"A max of {10} Embeds are allowed.");
		}
		if (args.Content.IsSpecified && args.Content.Value != null)
		{
			Preconditions.AtMost(args.Content.Value.Length, 2000, "Content", $"Message content is too long, length must be less or equal to {2000}.");
		}
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendMultipartAsync<Message>("POST", () => $"channels/{channelId}/messages", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "UploadFileAsync");
	}

	public Task<Message> UploadWebhookFileAsync(ulong webhookId, UploadWebhookFileParams args, RequestOptions options = null, ulong? threadId = null)
	{
		if (AuthTokenType != TokenType.Webhook)
		{
			throw new InvalidOperationException("This operation may only be called with a Webhook token.");
		}
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		options = RequestOptions.CreateOrClone(options);
		if (args.Embeds.IsSpecified)
		{
			Preconditions.AtMost(args.Embeds.Value.Length, 10, "Embeds", $"A max of {10} Embeds are allowed.");
		}
		if (args.Content.IsSpecified && args.Content.Value != null)
		{
			Preconditions.AtMost(args.Content.Value.Length, 2000, "Content", $"Message content is too long, length must be less or equal to {2000}.");
		}
		BucketIds ids = new BucketIds(0uL, 0uL, webhookId);
		return SendMultipartAsync<Message>("POST", () => $"webhooks/{webhookId}/{AuthToken}?{WebhookQuery(wait: true, threadId)}", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "UploadWebhookFileAsync");
	}

	public Task DeleteMessageAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("DELETE", () => $"channels/{channelId}/messages/{messageId}", ids, ClientBucketType.Unbucketed, options, "DeleteMessageAsync");
	}

	public Task DeleteMessagesAsync(ulong channelId, DeleteMessagesParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNull(args.MessageIds, "MessageIds");
		Preconditions.AtMost(args.MessageIds.Length, 100, "Length");
		Preconditions.YoungerThanTwoWeeks(args.MessageIds, "MessageIds");
		options = RequestOptions.CreateOrClone(options);
		switch (args.MessageIds.Length)
		{
		case 0:
			return Task.CompletedTask;
		case 1:
			return DeleteMessageAsync(channelId, args.MessageIds[0]);
		default:
		{
			BucketIds ids = new BucketIds(0uL, channelId, 0uL);
			return SendJsonAsync("POST", () => $"channels/{channelId}/messages/bulk-delete", args, ids, ClientBucketType.Unbucketed, options, "DeleteMessagesAsync");
		}
		}
	}

	public Task<Message> ModifyMessageAsync(ulong channelId, ulong messageId, ModifyMessageParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNull(args, "args");
		if (args.Embeds.IsSpecified)
		{
			Preconditions.AtMost(args.Embeds.Value.Length, 10, "Embeds", "A max of 10 Embeds are allowed.");
		}
		if (args.Content.IsSpecified && args.Content.Value != null)
		{
			Preconditions.AtMost(args.Content.Value.Length, 2000, "Content", $"Message content is too long, length must be less or equal to {2000}.");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Message>("PATCH", () => $"channels/{channelId}/messages/{messageId}", args, ids, ClientBucketType.SendEdit, options, "ModifyMessageAsync");
	}

	public Task<Message> ModifyMessageAsync(ulong channelId, ulong messageId, UploadFileParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNull(args, "args");
		if (args.Embeds.IsSpecified)
		{
			Preconditions.AtMost(args.Embeds.Value.Length, 10, "Embeds", "A max of 10 Embeds are allowed.");
		}
		if (args.Content.IsSpecified && args.Content.Value != null)
		{
			Preconditions.AtMost(args.Content.Value.Length, 2000, "Content", $"Message content is too long, length must be less or equal to {2000}.");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendMultipartAsync<Message>("PATCH", () => $"channels/{channelId}/messages/{messageId}", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "ModifyMessageAsync");
	}

	public Task<Sticker> GetStickerAsync(ulong id, RequestOptions options = null)
	{
		Preconditions.NotEqual(id, 0uL, "id");
		options = RequestOptions.CreateOrClone(options);
		return NullifyNotFound(SendAsync<Sticker>("GET", () => $"stickers/{id}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetStickerAsync"));
	}

	public Task<Sticker> GetGuildStickerAsync(ulong guildId, ulong id, RequestOptions options = null)
	{
		Preconditions.NotEqual(id, 0uL, "id");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return NullifyNotFound(SendAsync<Sticker>("GET", () => $"guilds/{guildId}/stickers/{id}", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildStickerAsync"));
	}

	public Task<Sticker[]> ListGuildStickersAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<Sticker[]>("GET", () => $"guilds/{guildId}/stickers", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ListGuildStickersAsync");
	}

	public Task<NitroStickerPacks> ListNitroStickerPacksAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<NitroStickerPacks>("GET", () => "sticker-packs", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ListNitroStickerPacksAsync");
	}

	public Task<Sticker> CreateGuildStickerAsync(CreateStickerParams args, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return SendMultipartAsync<Sticker>("POST", () => $"guilds/{guildId}/stickers", args.ToDictionary(), new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateGuildStickerAsync");
	}

	public Task<Sticker> ModifyStickerAsync(ModifyStickerParams args, ulong guildId, ulong stickerId, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(stickerId, 0uL, "stickerId");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<Sticker>("PATCH", () => $"guilds/{guildId}/stickers/{stickerId}", args, new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyStickerAsync");
	}

	public Task DeleteStickerAsync(ulong guildId, ulong stickerId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(stickerId, 0uL, "stickerId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync("DELETE", () => $"guilds/{guildId}/stickers/{stickerId}", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteStickerAsync");
	}

	public Task AddReactionAsync(ulong channelId, ulong messageId, string emoji, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNullOrWhitespace(emoji, "emoji");
		options = RequestOptions.CreateOrClone(options);
		options.IsReactionBucket = true;
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string me = "@me";
		return SendAsync("PUT", () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{me}", ids, ClientBucketType.Unbucketed, options, "AddReactionAsync");
	}

	public Task RemoveReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNullOrWhitespace(emoji, "emoji");
		options = RequestOptions.CreateOrClone(options);
		options.IsReactionBucket = true;
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		string user = ((!CurrentUserId.HasValue) ? userId.ToString() : ((userId == CurrentUserId.Value) ? "@me" : userId.ToString()));
		return SendAsync("DELETE", () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{user}", ids, ClientBucketType.Unbucketed, options, "RemoveReactionAsync");
	}

	public Task RemoveAllReactionsAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("DELETE", () => $"channels/{channelId}/messages/{messageId}/reactions", ids, ClientBucketType.Unbucketed, options, "RemoveAllReactionsAsync");
	}

	public Task RemoveAllReactionsForEmoteAsync(ulong channelId, ulong messageId, string emoji, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNullOrWhitespace(emoji, "emoji");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("DELETE", () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}", ids, ClientBucketType.Unbucketed, options, "RemoveAllReactionsForEmoteAsync");
	}

	public Task<IReadOnlyCollection<User>> GetReactionUsersAsync(ulong channelId, ulong messageId, string emoji, GetReactionUsersParams args, ReactionType reactionType, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		Preconditions.NotNullOrWhitespace(emoji, "emoji");
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 100, "Limit");
		Preconditions.GreaterThan(args.AfterUserId, 0uL, "AfterUserId");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(100);
		ulong afterUserId = args.AfterUserId.GetValueOrDefault(0uL);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		Expression<Func<string>> endpointExpr = () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}?limit={limit}&after={afterUserId}&type={(int)reactionType}";
		return SendAsync<IReadOnlyCollection<User>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetReactionUsersAsync");
	}

	public Task AckMessageAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("POST", () => $"channels/{channelId}/messages/{messageId}/ack", ids, ClientBucketType.Unbucketed, options, "AckMessageAsync");
	}

	public Task TriggerTypingIndicatorAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("POST", () => $"channels/{channelId}/typing", ids, ClientBucketType.Unbucketed, options, "TriggerTypingIndicatorAsync");
	}

	public Task CrosspostAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("POST", () => $"channels/{channelId}/messages/{messageId}/crosspost", ids, ClientBucketType.Unbucketed, options, "CrosspostAsync");
	}

	public Task<FollowedChannel> FollowChannelAsync(ulong newsChannelId, ulong followingChannelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(newsChannelId, 0uL, "newsChannelId");
		Preconditions.NotEqual(followingChannelId, 0uL, "followingChannelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, newsChannelId, 0uL);
		return SendJsonAsync<FollowedChannel>("POST", () => $"channels/{newsChannelId}/followers", new
		{
			webhook_channel_id = followingChannelId
		}, ids, ClientBucketType.Unbucketed, options, "FollowChannelAsync");
	}

	public Task ModifyChannelPermissionsAsync(ulong channelId, ulong targetId, ModifyChannelPermissionsParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(targetId, 0uL, "targetId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync("PUT", () => $"channels/{channelId}/permissions/{targetId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyChannelPermissionsAsync");
	}

	public Task DeleteChannelPermissionAsync(ulong channelId, ulong targetId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(targetId, 0uL, "targetId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("DELETE", () => $"channels/{channelId}/permissions/{targetId}", ids, ClientBucketType.Unbucketed, options, "DeleteChannelPermissionAsync");
	}

	public Task AddPinAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.GreaterThan(channelId, 0uL, "channelId");
		Preconditions.GreaterThan(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("PUT", () => $"channels/{channelId}/pins/{messageId}", ids, ClientBucketType.Unbucketed, options, "AddPinAsync");
	}

	public Task RemovePinAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(messageId, 0uL, "messageId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("DELETE", () => $"channels/{channelId}/pins/{messageId}", ids, ClientBucketType.Unbucketed, options, "RemovePinAsync");
	}

	public Task<IReadOnlyCollection<Message>> GetPinsAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync<IReadOnlyCollection<Message>>("GET", () => $"channels/{channelId}/pins", ids, ClientBucketType.Unbucketed, options, "GetPinsAsync");
	}

	public Task AddGroupRecipientAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.GreaterThan(channelId, 0uL, "channelId");
		Preconditions.GreaterThan(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("PUT", () => $"channels/{channelId}/recipients/{userId}", ids, ClientBucketType.Unbucketed, options, "AddGroupRecipientAsync");
	}

	public Task RemoveGroupRecipientAsync(ulong channelId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync("DELETE", () => $"channels/{channelId}/recipients/{userId}", ids, ClientBucketType.Unbucketed, options, "RemoveGroupRecipientAsync");
	}

	public Task<ApplicationCommand[]> GetGlobalApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		if (locale != null)
		{
			if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
			{
				throw new ArgumentException(locale + " is not a valid locale.", "locale");
			}
			options.RequestHeaders["X-Discord-Locale"] = new string[1] { locale };
		}
		string query = (withLocalizations ? "?with_localizations=true" : string.Empty);
		return SendAsync<ApplicationCommand[]>("GET", () => $"applications/{CurrentApplicationId}/commands{query}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGlobalApplicationCommandsAsync");
	}

	public async Task<ApplicationCommand> GetGlobalApplicationCommandAsync(ulong id, RequestOptions options = null)
	{
		Preconditions.NotEqual(id, 0uL, "id");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			return await SendAsync<ApplicationCommand>("GET", () => $"applications/{CurrentApplicationId}/commands/{id}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGlobalApplicationCommandAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<ApplicationCommand> CreateGlobalApplicationCommandAsync(CreateApplicationCommandParams command, RequestOptions options = null)
	{
		Preconditions.NotNull(command, "command");
		Preconditions.AtMost(command.Name.Length, 32, "Name");
		Preconditions.AtLeast(command.Name.Length, 1, "Name");
		if (command.Type == ApplicationCommandType.Slash)
		{
			Preconditions.NotNullOrEmpty(command.Description, "Description");
			Preconditions.AtMost(command.Description.Length, 100, "Description");
			Preconditions.AtLeast(command.Description.Length, 1, "Description");
		}
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<ApplicationCommand>("POST", () => $"applications/{CurrentApplicationId}/commands", command, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateGlobalApplicationCommandAsync");
	}

	public Task<ApplicationCommand> ModifyGlobalApplicationCommandAsync(ModifyApplicationCommandParams command, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<ApplicationCommand>("PATCH", () => $"applications/{CurrentApplicationId}/commands/{commandId}", command, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyGlobalApplicationCommandAsync");
	}

	public Task<ApplicationCommand> ModifyGlobalApplicationUserCommandAsync(ModifyApplicationCommandParams command, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<ApplicationCommand>("PATCH", () => $"applications/{CurrentApplicationId}/commands/{commandId}", command, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyGlobalApplicationUserCommandAsync");
	}

	public Task<ApplicationCommand> ModifyGlobalApplicationMessageCommandAsync(ModifyApplicationCommandParams command, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<ApplicationCommand>("PATCH", () => $"applications/{CurrentApplicationId}/commands/{commandId}", command, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyGlobalApplicationMessageCommandAsync");
	}

	public Task DeleteGlobalApplicationCommandAsync(ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync("DELETE", () => $"applications/{CurrentApplicationId}/commands/{commandId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteGlobalApplicationCommandAsync");
	}

	public Task<ApplicationCommand[]> BulkOverwriteGlobalApplicationCommandsAsync(CreateApplicationCommandParams[] commands, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<ApplicationCommand[]>("PUT", () => $"applications/{CurrentApplicationId}/commands", commands, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "BulkOverwriteGlobalApplicationCommandsAsync");
	}

	public Task<ApplicationCommand[]> GetGuildApplicationCommandsAsync(ulong guildId, bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		if (locale != null)
		{
			if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
			{
				throw new ArgumentException(locale + " is not a valid locale.", "locale");
			}
			options.RequestHeaders["X-Discord-Locale"] = new string[1] { locale };
		}
		string query = (withLocalizations ? "?with_localizations=true" : string.Empty);
		return SendAsync<ApplicationCommand[]>("GET", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands{query}", ids, ClientBucketType.Unbucketed, options, "GetGuildApplicationCommandsAsync");
	}

	public async Task<ApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		try
		{
			return await SendAsync<ApplicationCommand>("GET", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}", ids, ClientBucketType.Unbucketed, options, "GetGuildApplicationCommandAsync");
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<ApplicationCommand> CreateGuildApplicationCommandAsync(CreateApplicationCommandParams command, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotNull(command, "command");
		Preconditions.AtMost(command.Name.Length, 32, "Name");
		Preconditions.AtLeast(command.Name.Length, 1, "Name");
		if (command.Type == ApplicationCommandType.Slash)
		{
			Preconditions.NotNullOrEmpty(command.Description, "Description");
			Preconditions.AtMost(command.Description.Length, 100, "Description");
			Preconditions.AtLeast(command.Description.Length, 1, "Description");
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<ApplicationCommand>("POST", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands", command, ids, ClientBucketType.Unbucketed, options, "CreateGuildApplicationCommandAsync");
	}

	public Task<ApplicationCommand> ModifyGuildApplicationCommandAsync(ModifyApplicationCommandParams command, ulong guildId, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<ApplicationCommand>("PATCH", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}", command, ids, ClientBucketType.Unbucketed, options, "ModifyGuildApplicationCommandAsync");
	}

	public Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<ApplicationCommand>("DELETE", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildApplicationCommandAsync");
	}

	public Task<ApplicationCommand[]> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, CreateApplicationCommandParams[] commands, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<ApplicationCommand[]>("PUT", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands", commands, ids, ClientBucketType.Unbucketed, options, "BulkOverwriteGuildApplicationCommandsAsync");
	}

	public Task CreateInteractionResponseAsync(InteractionResponse response, ulong interactionId, string interactionToken, RequestOptions options = null)
	{
		if (response.Data.IsSpecified && response.Data.Value.Content.IsSpecified)
		{
			Preconditions.AtMost(response.Data.Value.Content.Value?.Length ?? 0, 2000, "Content");
		}
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync("POST", () => $"interactions/{interactionId}/{interactionToken}/callback", response, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateInteractionResponseAsync");
	}

	public Task CreateInteractionResponseAsync(UploadInteractionFileParams response, ulong interactionId, string interactionToken, RequestOptions options = null)
	{
		if ((!response.Embeds.IsSpecified || response.Embeds.Value == null || response.Embeds.Value.Length == 0) && (!response.Content.IsSpecified || response.Content.Value == null || string.IsNullOrWhiteSpace(response.Content.Value)) && (!response.MessageComponents.IsSpecified || response.MessageComponents.Value == null || response.MessageComponents.Value.Length == 0) && (response.Files == null || response.Files.Length == 0))
		{
			throw new ArgumentException("At least one of 'Content', 'Embeds', 'Files' or 'MessageComponents' must be specified.", "response");
		}
		if (response.Content.IsSpecified)
		{
			string value = response.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendMultipartAsync("POST", () => $"interactions/{interactionId}/{interactionToken}/callback", response.ToDictionary(), ids, ClientBucketType.SendEdit, options, "CreateInteractionResponseAsync");
	}

	public Task<Message> GetInteractionResponseAsync(string interactionToken, RequestOptions options = null)
	{
		Preconditions.NotNullOrEmpty(interactionToken, "interactionToken");
		options = RequestOptions.CreateOrClone(options);
		return NullifyNotFound(SendAsync<Message>("GET", () => $"webhooks/{CurrentApplicationId}/{interactionToken}/messages/@original", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetInteractionResponseAsync"));
	}

	public Task<Message> ModifyInteractionResponseAsync(ModifyInteractionResponseParams args, string interactionToken, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<Message>("PATCH", () => $"webhooks/{CurrentApplicationId}/{interactionToken}/messages/@original", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyInteractionResponseAsync");
	}

	public Task<Message> ModifyInteractionResponseAsync(UploadWebhookFileParams args, string interactionToken, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendMultipartAsync<Message>("PATCH", () => $"webhooks/{CurrentApplicationId}/{interactionToken}/messages/@original", args.ToDictionary(), new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyInteractionResponseAsync");
	}

	public Task DeleteInteractionResponseAsync(string interactionToken, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync("DELETE", () => $"webhooks/{CurrentApplicationId}/{interactionToken}/messages/@original", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteInteractionResponseAsync");
	}

	public Task<Message> CreateInteractionFollowupMessageAsync(CreateWebhookMessageParams args, string token, RequestOptions options = null)
	{
		if ((!args.Embeds.IsSpecified || args.Embeds.Value == null || args.Embeds.Value.Length == 0) && (!args.Content.IsSpecified || args.Content.Value == null || string.IsNullOrWhiteSpace(args.Content.Value)) && (!args.Components.IsSpecified || args.Components.Value == null || args.Components.Value.Length == 0) && !args.File.IsSpecified)
		{
			throw new ArgumentException("At least one of 'Content', 'Embeds', 'File' or 'Components' must be specified.", "args");
		}
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		if (!args.File.IsSpecified)
		{
			return SendJsonAsync<Message>("POST", () => $"webhooks/{CurrentApplicationId}/{token}?wait=true", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateInteractionFollowupMessageAsync");
		}
		return SendMultipartAsync<Message>("POST", () => $"webhooks/{CurrentApplicationId}/{token}?wait=true", args.ToDictionary(), new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateInteractionFollowupMessageAsync");
	}

	public Task<Message> CreateInteractionFollowupMessageAsync(UploadWebhookFileParams args, string token, RequestOptions options = null)
	{
		if ((!args.Embeds.IsSpecified || args.Embeds.Value == null || args.Embeds.Value.Length == 0) && (!args.Content.IsSpecified || args.Content.Value == null || string.IsNullOrWhiteSpace(args.Content.Value)) && (!args.MessageComponents.IsSpecified || args.MessageComponents.Value == null || args.MessageComponents.Value.Length == 0) && args.Files.Length == 0)
		{
			throw new ArgumentException("At least one of 'Content', 'Embeds', 'Files' or 'Components' must be specified.", "args");
		}
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendMultipartAsync<Message>("POST", () => $"webhooks/{CurrentApplicationId}/{token}?wait=true", args.ToDictionary(), ids, ClientBucketType.SendEdit, options, "CreateInteractionFollowupMessageAsync");
	}

	public Task<Message> ModifyInteractionFollowupMessageAsync(ModifyInteractionResponseParams args, ulong id, string token, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(id, 0uL, "id");
		if (args.Content.IsSpecified)
		{
			string value = args.Content.Value;
			if (value != null && value.Length > 2000)
			{
				throw new ArgumentException($"Message content is too long, length must be less or equal to {2000}.", "Content");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<Message>("PATCH", () => $"webhooks/{CurrentApplicationId}/{token}/messages/{id}", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyInteractionFollowupMessageAsync");
	}

	public Task DeleteInteractionFollowupMessageAsync(ulong id, string token, RequestOptions options = null)
	{
		Preconditions.NotEqual(id, 0uL, "id");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync("DELETE", () => $"webhooks/{CurrentApplicationId}/{token}/messages/{id}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteInteractionFollowupMessageAsync");
	}

	public Task<GuildApplicationCommandPermission[]> GetGuildApplicationCommandPermissionsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<GuildApplicationCommandPermission[]>("GET", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/permissions", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildApplicationCommandPermissionsAsync");
	}

	public Task<GuildApplicationCommandPermission> GetGuildApplicationCommandPermissionAsync(ulong guildId, ulong commandId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(commandId, 0uL, "commandId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<GuildApplicationCommandPermission>("GET", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}/permissions", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildApplicationCommandPermissionAsync");
	}

	public Task<GuildApplicationCommandPermission> ModifyApplicationCommandPermissionsAsync(ModifyGuildApplicationCommandPermissionsParams permissions, ulong guildId, ulong commandId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(commandId, 0uL, "commandId");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<GuildApplicationCommandPermission>("PUT", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/{commandId}/permissions", permissions, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyApplicationCommandPermissionsAsync");
	}

	public async Task<IReadOnlyCollection<GuildApplicationCommandPermission>> BatchModifyApplicationCommandPermissionsAsync(ModifyGuildApplicationCommandPermissions[] permissions, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(permissions, "permissions");
		options = RequestOptions.CreateOrClone(options);
		return await SendJsonAsync<GuildApplicationCommandPermission[]>("PUT", () => $"applications/{CurrentApplicationId}/guilds/{guildId}/commands/permissions", permissions, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "BatchModifyApplicationCommandPermissionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Guild> GetGuildAsync(ulong guildId, bool withCounts, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<Guild>("GET", () => string.Format("guilds/{0}?with_counts={1}", guildId, withCounts ? "true" : "false"), ids, ClientBucketType.Unbucketed, options, "GetGuildAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<Guild> CreateGuildAsync(CreateGuildParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		Preconditions.NotNullOrWhitespace(args.RegionId, "RegionId");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<Guild>("POST", () => "guilds", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateGuildAsync");
	}

	public Task<Guild> DeleteGuildAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<Guild>("DELETE", () => $"guilds/{guildId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildAsync");
	}

	public Task<Guild> LeaveGuildAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<Guild>("DELETE", () => $"users/@me/guilds/{guildId}", ids, ClientBucketType.Unbucketed, options, "LeaveGuildAsync");
	}

	public Task<Guild> ModifyGuildAsync(ulong guildId, ModifyGuildParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(args.AfkChannelId, 0uL, "AfkChannelId");
		Preconditions.AtLeast(args.AfkTimeout, 0, "AfkTimeout");
		Preconditions.NotNullOrEmpty(args.Name, "Name");
		Preconditions.GreaterThan(args.OwnerId, 0uL, "OwnerId");
		Preconditions.NotNull(args.RegionId, "RegionId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<Guild>("PATCH", () => $"guilds/{guildId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildAsync");
	}

	public Task<GetGuildPruneCountResponse> BeginGuildPruneAsync(ulong guildId, GuildPruneParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Days, 1, "Days");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<GetGuildPruneCountResponse>("POST", () => $"guilds/{guildId}/prune", args, ids, ClientBucketType.Unbucketed, options, "BeginGuildPruneAsync");
	}

	public Task<GetGuildPruneCountResponse> GetGuildPruneCountAsync(ulong guildId, GuildPruneParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Days, 1, "Days");
		ulong[] includeRoleIds = args.IncludeRoleIds;
		string endpointRoleIds = ((includeRoleIds != null && includeRoleIds.Length != 0) ? ("&include_roles=" + string.Join(",", args.IncludeRoleIds)) : "");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<GetGuildPruneCountResponse>("GET", () => $"guilds/{guildId}/prune?days={args.Days}{endpointRoleIds}", ids, ClientBucketType.Unbucketed, options, "GetGuildPruneCountAsync");
	}

	public async Task<GuildIncidentsData> ModifyGuildIncidentActionsAsync(ulong guildId, ModifyGuildIncidentsDataParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<GuildIncidentsData>("PUT", () => $"guilds/{guildId}/incident-actions", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildIncidentActionsAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<IReadOnlyCollection<Ban>> GetGuildBansAsync(ulong guildId, GetGuildBansParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 1000, "Limit");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(1000);
		ulong? relativeId = (args.RelativeUserId.IsSpecified ? new ulong?(args.RelativeUserId.Value) : ((ulong?)null));
		string relativeDir = args.RelativeDirection.GetValueOrDefault(Direction.Before) switch
		{
			Direction.After => "after", 
			Direction.Around => "around", 
			_ => "before", 
		};
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		Expression<Func<string>> endpointExpr = ((!relativeId.HasValue) ? ((Expression<Func<string>>)(() => $"guilds/{guildId}/bans?limit={limit}")) : ((Expression<Func<string>>)(() => $"guilds/{guildId}/bans?limit={limit}&{relativeDir}={relativeId}")));
		return SendAsync<IReadOnlyCollection<Ban>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetGuildBansAsync");
	}

	public async Task<Ban> GetGuildBanAsync(ulong guildId, ulong userId, RequestOptions options)
	{
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<Ban>("GET", () => $"guilds/{guildId}/bans/{userId}", ids, ClientBucketType.Unbucketed, options, "GetGuildBanAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task CreateGuildBanAsync(ulong guildId, ulong userId, uint deleteMessageSeconds, string reason, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.AtMost(deleteMessageSeconds, 604800u, "deleteMessageSeconds", "Prune length must be within [0, 604800]");
		CreateGuildBanParams payload = new CreateGuildBanParams
		{
			DeleteMessageSeconds = deleteMessageSeconds
		};
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		if (!string.IsNullOrWhiteSpace(reason))
		{
			options.AuditLogReason = reason;
		}
		return SendJsonAsync("PUT", () => $"guilds/{guildId}/bans/{userId}", payload, ids, ClientBucketType.Unbucketed, options, "CreateGuildBanAsync");
	}

	public Task RemoveGuildBanAsync(ulong guildId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync("DELETE", () => $"guilds/{guildId}/bans/{userId}", ids, ClientBucketType.Unbucketed, options, "RemoveGuildBanAsync");
	}

	public Task<BulkBanResult> BulkBanAsync(ulong guildId, ulong[] userIds, int? deleteMessagesSeconds = null, RequestOptions options = null)
	{
		Preconditions.NotEqual(userIds.Length, 0, "userIds");
		Preconditions.AtMost(userIds.Length, 200, "userIds");
		Preconditions.AtMost(deleteMessagesSeconds.GetValueOrDefault(), 604800, "deleteMessagesSeconds");
		options = RequestOptions.CreateOrClone(options);
		BulkBanParams payload = new BulkBanParams
		{
			DeleteMessageSeconds = (((Optional<int>?)deleteMessagesSeconds) ?? Optional<int>.Unspecified),
			UserIds = userIds
		};
		return SendJsonAsync<BulkBanResult>("POST", () => $"guilds/{guildId}/bulk-ban", payload, new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "BulkBanAsync");
	}

	public async Task<GuildWidget> GetGuildWidgetAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<GuildWidget>("GET", () => $"guilds/{guildId}/widget", ids, ClientBucketType.Unbucketed, options, "GetGuildWidgetAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<GuildWidget> ModifyGuildWidgetAsync(ulong guildId, ModifyGuildWidgetParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<GuildWidget>("PATCH", () => $"guilds/{guildId}/widget", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildWidgetAsync");
	}

	public Task<IReadOnlyCollection<Integration>> GetIntegrationsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<IReadOnlyCollection<Integration>>("GET", () => $"guilds/{guildId}/integrations", ids, ClientBucketType.Unbucketed, options, "GetIntegrationsAsync");
	}

	public Task DeleteIntegrationAsync(ulong guildId, ulong integrationId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(integrationId, 0uL, "integrationId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync("DELETE", () => $"guilds/{guildId}/integrations/{integrationId}", ids, ClientBucketType.Unbucketed, options, "DeleteIntegrationAsync");
	}

	public async Task<InviteMetadata> GetInviteAsync(string inviteId, RequestOptions options = null, ulong? scheduledEventId = null)
	{
		Preconditions.NotNullOrEmpty(inviteId, "inviteId");
		options = RequestOptions.CreateOrClone(options);
		if (inviteId[inviteId.Length - 1] == '/')
		{
			inviteId = inviteId.Substring(0, inviteId.Length - 1);
		}
		int num = inviteId.LastIndexOf('/');
		if (num >= 0)
		{
			inviteId = inviteId.Substring(num + 1);
		}
		string scheduledEventQuery = (scheduledEventId.HasValue ? $"&guild_scheduled_event_id={scheduledEventId}" : string.Empty);
		try
		{
			return await SendAsync<InviteMetadata>("GET", () => $"invites/{inviteId}?with_counts=true&with_expiration=true{scheduledEventQuery}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetInviteAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<InviteVanity> GetVanityInviteAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<InviteVanity>("GET", () => $"guilds/{guildId}/vanity-url", ids, ClientBucketType.Unbucketed, options, "GetVanityInviteAsync");
	}

	public Task<IReadOnlyCollection<InviteMetadata>> GetGuildInvitesAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<IReadOnlyCollection<InviteMetadata>>("GET", () => $"guilds/{guildId}/invites", ids, ClientBucketType.Unbucketed, options, "GetGuildInvitesAsync");
	}

	public Task<IReadOnlyCollection<InviteMetadata>> GetChannelInvitesAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync<IReadOnlyCollection<InviteMetadata>>("GET", () => $"channels/{channelId}/invites", ids, ClientBucketType.Unbucketed, options, "GetChannelInvitesAsync");
	}

	public Task<InviteMetadata> CreateChannelInviteAsync(ulong channelId, CreateChannelInviteParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.MaxAge, 0, "MaxAge");
		Preconditions.AtLeast(args.MaxUses, 0, "MaxUses");
		Preconditions.AtMost(args.MaxAge, 86400, "MaxAge", "The maximum age of an invite must be less than or equal to a day (86400 seconds).");
		if (args.TargetType.IsSpecified)
		{
			Preconditions.NotEqual((int)args.TargetType.Value, 0, "TargetType");
			if (args.TargetType.Value == TargetUserType.Stream)
			{
				Preconditions.GreaterThan(args.TargetUserId, 0uL, "TargetUserId");
			}
			if (args.TargetType.Value == TargetUserType.EmbeddedApplication)
			{
				Preconditions.GreaterThan(args.TargetApplicationId, 0uL, "TargetApplicationId");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<InviteMetadata>("POST", () => $"channels/{channelId}/invites", args, ids, ClientBucketType.Unbucketed, options, "CreateChannelInviteAsync");
	}

	public Task<Invite> DeleteInviteAsync(string inviteId, RequestOptions options = null)
	{
		Preconditions.NotNullOrEmpty(inviteId, "inviteId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<Invite>("DELETE", () => $"invites/{inviteId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteInviteAsync");
	}

	public Task<GuildMember> AddGuildMemberAsync(ulong guildId, ulong userId, AddGuildMemberParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrWhitespace(args.AccessToken, "AccessToken");
		if (args.RoleIds.IsSpecified)
		{
			ulong[] value = args.RoleIds.Value;
			for (int i = 0; i < value.Length; i++)
			{
				Preconditions.NotEqual(value[i], 0uL, "roleId");
			}
		}
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<GuildMember>("PUT", () => $"guilds/{guildId}/members/{userId}", args, ids, ClientBucketType.Unbucketed, options, "AddGuildMemberAsync");
	}

	public async Task<GuildMember> GetGuildMemberAsync(ulong guildId, ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<GuildMember>("GET", () => $"guilds/{guildId}/members/{userId}", ids, ClientBucketType.Unbucketed, options, "GetGuildMemberAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<IReadOnlyCollection<GuildMember>> GetGuildMembersAsync(ulong guildId, GetGuildMembersParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 1000, "Limit");
		Preconditions.GreaterThan(args.AfterUserId, 0uL, "AfterUserId");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(int.MaxValue);
		ulong afterUserId = args.AfterUserId.GetValueOrDefault(0uL);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		Expression<Func<string>> endpointExpr = () => $"guilds/{guildId}/members?limit={limit}&after={afterUserId}";
		return SendAsync<IReadOnlyCollection<GuildMember>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetGuildMembersAsync");
	}

	public Task RemoveGuildMemberAsync(ulong guildId, ulong userId, string reason, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		if (!string.IsNullOrWhiteSpace(reason))
		{
			options.AuditLogReason = reason;
		}
		return SendAsync("DELETE", () => $"guilds/{guildId}/members/{userId}", ids, ClientBucketType.Unbucketed, options, "RemoveGuildMemberAsync");
	}

	public async Task ModifyGuildMemberAsync(ulong guildId, ulong userId, ModifyGuildMemberParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(userId, 0uL, "userId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		bool isCurrentUser = userId == CurrentUserId;
		if (isCurrentUser && args.Nickname.IsSpecified)
		{
			ModifyCurrentUserNickParams args2 = new ModifyCurrentUserNickParams(args.Nickname.Value ?? "");
			await ModifyMyNickAsync(guildId, args2).ConfigureAwait(continueOnCapturedContext: false);
			args.Nickname = Optional.Create<string>();
		}
		if (!isCurrentUser || args.Deaf.IsSpecified || args.Mute.IsSpecified || args.RoleIds.IsSpecified)
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			await SendJsonAsync("PATCH", () => $"guilds/{guildId}/members/{userId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildMemberAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public Task<IReadOnlyCollection<GuildMember>> SearchGuildMembersAsync(ulong guildId, SearchGuildMembersParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 1000, "Limit");
		Preconditions.NotNullOrEmpty(args.Query, "Query");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(1000);
		string query = args.Query;
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		Expression<Func<string>> endpointExpr = () => $"guilds/{guildId}/members/search?limit={limit}&query={query}";
		return SendAsync<IReadOnlyCollection<GuildMember>>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "SearchGuildMembersAsync");
	}

	public async Task<GuildMemberSearchResponse> SearchGuildMembersAsyncV2(ulong guildId, SearchGuildMembersParamsV2 args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<GuildMemberSearchResponse>("POST", () => $"guilds/{guildId}/members-search", args, ids, ClientBucketType.Unbucketed, options, "SearchGuildMembersAsyncV2");
	}

	public Task<IReadOnlyCollection<Role>> GetGuildRolesAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<IReadOnlyCollection<Role>>("GET", () => $"guilds/{guildId}/roles", ids, ClientBucketType.Unbucketed, options, "GetGuildRolesAsync");
	}

	public async Task<Role> CreateGuildRoleAsync(ulong guildId, ModifyGuildRoleParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return await SendJsonAsync<Role>("POST", () => $"guilds/{guildId}/roles", args, ids, ClientBucketType.Unbucketed, options, "CreateGuildRoleAsync").ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task DeleteGuildRoleAsync(ulong guildId, ulong roleId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync("DELETE", () => $"guilds/{guildId}/roles/{roleId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildRoleAsync");
	}

	public Task<Role> ModifyGuildRoleAsync(ulong guildId, ulong roleId, ModifyGuildRoleParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(roleId, 0uL, "roleId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Color, 0u, "Color");
		Preconditions.NotNullOrEmpty(args.Name, "Name");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<Role>("PATCH", () => $"guilds/{guildId}/roles/{roleId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildRoleAsync");
	}

	public Task<IReadOnlyCollection<Role>> ModifyGuildRolesAsync(ulong guildId, IEnumerable<ModifyGuildRolesParams> args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<IReadOnlyCollection<Role>>("PATCH", () => $"guilds/{guildId}/roles", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildRolesAsync");
	}

	public Task<IReadOnlyCollection<Emoji>> GetGuildEmotesAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<IReadOnlyCollection<Emoji>>("GET", () => $"guilds/{guildId}/emojis", ids, ClientBucketType.Unbucketed, options, "GetGuildEmotesAsync");
	}

	public Task<Emoji> GetGuildEmoteAsync(ulong guildId, ulong emoteId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<Emoji>("GET", () => $"guilds/{guildId}/emojis/{emoteId}", ids, ClientBucketType.Unbucketed, options, "GetGuildEmoteAsync");
	}

	public Task<Emoji> CreateGuildEmoteAsync(ulong guildId, CreateGuildEmoteParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		Preconditions.NotNull(args.Image.Stream, "Image");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<Emoji>("POST", () => $"guilds/{guildId}/emojis", args, ids, ClientBucketType.Unbucketed, options, "CreateGuildEmoteAsync");
	}

	public Task<Emoji> ModifyGuildEmoteAsync(ulong guildId, ulong emoteId, ModifyGuildEmoteParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<Emoji>("PATCH", () => $"guilds/{guildId}/emojis/{emoteId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildEmoteAsync");
	}

	public Task DeleteGuildEmoteAsync(ulong guildId, ulong emoteId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync("DELETE", () => $"guilds/{guildId}/emojis/{emoteId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildEmoteAsync");
	}

	public Task<GuildScheduledEvent[]> ListGuildScheduledEventsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<GuildScheduledEvent[]>("GET", () => $"guilds/{guildId}/scheduled-events?with_user_count=true", ids, ClientBucketType.Unbucketed, options, "ListGuildScheduledEventsAsync");
	}

	public Task<GuildScheduledEvent> GetGuildScheduledEventAsync(ulong eventId, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return NullifyNotFound(SendAsync<GuildScheduledEvent>("GET", () => $"guilds/{guildId}/scheduled-events/{eventId}?with_user_count=true", ids, ClientBucketType.Unbucketed, options, "GetGuildScheduledEventAsync"));
	}

	public Task<GuildScheduledEvent> CreateGuildScheduledEventAsync(CreateGuildScheduledEventParams args, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<GuildScheduledEvent>("POST", () => $"guilds/{guildId}/scheduled-events", args, ids, ClientBucketType.Unbucketed, options, "CreateGuildScheduledEventAsync");
	}

	public Task<GuildScheduledEvent> ModifyGuildScheduledEventAsync(ModifyGuildScheduledEventParams args, ulong eventId, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<GuildScheduledEvent>("PATCH", () => $"guilds/{guildId}/scheduled-events/{eventId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildScheduledEventAsync");
	}

	public Task DeleteGuildScheduledEventAsync(ulong eventId, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync("DELETE", () => $"guilds/{guildId}/scheduled-events/{eventId}", ids, ClientBucketType.Unbucketed, options, "DeleteGuildScheduledEventAsync");
	}

	public Task<GuildScheduledEventUser[]> GetGuildScheduledEventUsersAsync(ulong eventId, ulong guildId, int limit = 100, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<GuildScheduledEventUser[]>("GET", () => $"guilds/{guildId}/scheduled-events/{eventId}/users?limit={limit}&with_member=true", ids, ClientBucketType.Unbucketed, options, "GetGuildScheduledEventUsersAsync");
	}

	public Task<GuildScheduledEventUser[]> GetGuildScheduledEventUsersAsync(ulong eventId, ulong guildId, GetEventUsersParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(eventId, 0uL, "eventId");
		Preconditions.NotNull(args, "args");
		Preconditions.AtLeast(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 100, "Limit");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(100);
		ulong? relativeId = (args.RelativeUserId.IsSpecified ? new ulong?(args.RelativeUserId.Value) : ((ulong?)null));
		string relativeDir = args.RelativeDirection.GetValueOrDefault(Direction.Before) switch
		{
			Direction.After => "after", 
			Direction.Around => "around", 
			_ => "before", 
		};
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		Expression<Func<string>> endpointExpr = ((!relativeId.HasValue) ? ((Expression<Func<string>>)(() => $"guilds/{guildId}/scheduled-events/{eventId}/users?with_member=true&limit={limit}")) : ((Expression<Func<string>>)(() => $"guilds/{guildId}/scheduled-events/{eventId}/users?with_member=true&limit={limit}&{relativeDir}={relativeId}")));
		return SendAsync<GuildScheduledEventUser[]>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetGuildScheduledEventUsersAsync");
	}

	public Task<AutoModerationRule[]> GetGuildAutoModRulesAsync(ulong guildId, RequestOptions options)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<AutoModerationRule[]>("GET", () => $"guilds/{guildId}/auto-moderation/rules", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildAutoModRulesAsync");
	}

	public Task<AutoModerationRule> GetGuildAutoModRuleAsync(ulong guildId, ulong ruleId, RequestOptions options)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(ruleId, 0uL, "ruleId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<AutoModerationRule>("GET", () => $"guilds/{guildId}/auto-moderation/rules/{ruleId}", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildAutoModRuleAsync");
	}

	public Task<AutoModerationRule> CreateGuildAutoModRuleAsync(ulong guildId, CreateAutoModRuleParams args, RequestOptions options)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<AutoModerationRule>("POST", () => $"guilds/{guildId}/auto-moderation/rules", args, new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateGuildAutoModRuleAsync");
	}

	public Task<AutoModerationRule> ModifyGuildAutoModRuleAsync(ulong guildId, ulong ruleId, ModifyAutoModRuleParams args, RequestOptions options)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(ruleId, 0uL, "ruleId");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<AutoModerationRule>("PATCH", () => $"guilds/{guildId}/auto-moderation/rules/{ruleId}", args, new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyGuildAutoModRuleAsync");
	}

	public Task DeleteGuildAutoModRuleAsync(ulong guildId, ulong ruleId, RequestOptions options)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotEqual(ruleId, 0uL, "ruleId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync("DELETE", () => $"guilds/{guildId}/auto-moderation/rules/{ruleId}", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteGuildAutoModRuleAsync");
	}

	public async Task<WelcomeScreen> GetGuildWelcomeScreenAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
			return await SendAsync<WelcomeScreen>("GET", () => $"guilds/{guildId}/welcome-screen", ids, ClientBucketType.Unbucketed, options, "GetGuildWelcomeScreenAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<WelcomeScreen> ModifyGuildWelcomeScreenAsync(ModifyGuildWelcomeScreenParams args, ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync<WelcomeScreen>("PATCH", () => $"guilds/{guildId}/welcome-screen", args, ids, ClientBucketType.Unbucketed, options, "ModifyGuildWelcomeScreenAsync");
	}

	public Task<GuildOnboarding> GetGuildOnboardingAsync(ulong guildId, RequestOptions options)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<GuildOnboarding>("GET", () => $"guilds/{guildId}/onboarding", new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetGuildOnboardingAsync");
	}

	public Task<GuildOnboarding> ModifyGuildOnboardingAsync(ulong guildId, ModifyGuildOnboardingParams args, RequestOptions options)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<GuildOnboarding>("PUT", () => $"guilds/{guildId}/onboarding", args, new BucketIds(guildId, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyGuildOnboardingAsync");
	}

	public async Task<User> GetUserAsync(ulong userId, RequestOptions options = null)
	{
		Preconditions.NotEqual(userId, 0uL, "userId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			return await SendAsync<User>("GET", () => $"users/{userId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetUserAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<User> GetMyUserAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<User>("GET", () => "users/@me", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyUserAsync");
	}

	public Task<IReadOnlyCollection<Connection>> GetMyConnectionsAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<IReadOnlyCollection<Connection>>("GET", () => "users/@me/connections", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyConnectionsAsync");
	}

	public Task<IReadOnlyCollection<Channel>> GetMyPrivateChannelsAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<IReadOnlyCollection<Channel>>("GET", () => "users/@me/channels", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyPrivateChannelsAsync");
	}

	public Task<IReadOnlyCollection<UserGuild>> GetMyGuildsAsync(GetGuildSummariesParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.Limit, 0, "Limit");
		Preconditions.AtMost(args.Limit, 100, "Limit");
		Preconditions.GreaterThan(args.AfterGuildId, 0uL, "AfterGuildId");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(int.MaxValue);
		ulong afterGuildId = args.AfterGuildId.GetValueOrDefault(0uL);
		return SendAsync<IReadOnlyCollection<UserGuild>>("GET", () => $"users/@me/guilds?limit={limit}&after={afterGuildId}&with_counts=true", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyGuildsAsync");
	}

	public Task<Application> GetMyApplicationAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<Application>("GET", () => "oauth2/applications/@me", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetMyApplicationAsync");
	}

	public Task<Application> GetCurrentBotApplicationAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<Application>("GET", () => "applications/@me", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetCurrentBotApplicationAsync");
	}

	public Task<Application> ModifyCurrentBotApplicationAsync(ModifyCurrentApplicationBotParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<Application>("PATCH", () => "applications/@me", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyCurrentBotApplicationAsync");
	}

	public Task<User> ModifySelfAsync(ModifyCurrentUserParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrEmpty(args.Username, "Username");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<User>("PATCH", () => "users/@me", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifySelfAsync");
	}

	public Task ModifyMyNickAsync(ulong guildId, ModifyCurrentUserNickParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotNull(args.Nickname, "Nickname");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendJsonAsync("PATCH", () => $"guilds/{guildId}/members/@me/nick", args, ids, ClientBucketType.Unbucketed, options, "ModifyMyNickAsync");
	}

	public Task<Channel> CreateDMChannelAsync(CreateDMChannelParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.GreaterThan(args.RecipientId, 0uL, "RecipientId");
		options = RequestOptions.CreateOrClone(options);
		return SendJsonAsync<Channel>("POST", () => "users/@me/channels", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateDMChannelAsync");
	}

	public Task<GuildMember> GetCurrentUserGuildMember(ulong guildId, RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendAsync<GuildMember>("GET", () => $"users/@me/guilds/{guildId}/member", ids, ClientBucketType.Unbucketed, options, "GetCurrentUserGuildMember");
	}

	public Task<IReadOnlyCollection<VoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		return SendAsync<IReadOnlyCollection<VoiceRegion>>("GET", () => "voice/regions", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetVoiceRegionsAsync");
	}

	public Task<IReadOnlyCollection<VoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<IReadOnlyCollection<VoiceRegion>>("GET", () => $"guilds/{guildId}/regions", ids, ClientBucketType.Unbucketed, options, "GetGuildVoiceRegionsAsync");
	}

	public Task<AuditLog> GetAuditLogsAsync(ulong guildId, GetAuditLogsParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		int limit = args.Limit.GetValueOrDefault(int.MaxValue);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		StringBuilder queryArgs = new StringBuilder();
		if (args.BeforeEntryId.IsSpecified)
		{
			queryArgs.Append("&before=").Append(args.BeforeEntryId);
		}
		if (args.UserId.IsSpecified)
		{
			queryArgs.Append("&user_id=").Append(args.UserId.Value);
		}
		if (args.ActionType.IsSpecified)
		{
			queryArgs.Append("&action_type=").Append(args.ActionType.Value);
		}
		if (args.AfterEntryId.IsSpecified)
		{
			queryArgs.Append("&after=").Append(args.AfterEntryId);
		}
		Expression<Func<string>> endpointExpr = () => $"guilds/{guildId}/audit-logs?limit={limit}{queryArgs.ToString()}";
		return SendAsync<AuditLog>("GET", endpointExpr, ids, ClientBucketType.Unbucketed, options, "GetAuditLogsAsync");
	}

	public Task<Webhook> CreateWebhookAsync(ulong channelId, CreateWebhookParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNull(args.Name, "Name");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendJsonAsync<Webhook>("POST", () => $"channels/{channelId}/webhooks", args, ids, ClientBucketType.Unbucketed, options, "CreateWebhookAsync");
	}

	public async Task<Webhook> GetWebhookAsync(ulong webhookId, RequestOptions options = null)
	{
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		options = RequestOptions.CreateOrClone(options);
		try
		{
			if (AuthTokenType == TokenType.Webhook)
			{
				return await SendAsync<Webhook>("GET", () => $"webhooks/{webhookId}/{AuthToken}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
			}
			return await SendAsync<Webhook>("GET", () => $"webhooks/{webhookId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetWebhookAsync").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public Task<Webhook> ModifyWebhookAsync(ulong webhookId, ModifyWebhookParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrEmpty(args.Name, "Name");
		options = RequestOptions.CreateOrClone(options);
		if (AuthTokenType == TokenType.Webhook)
		{
			return SendJsonAsync<Webhook>("PATCH", () => $"webhooks/{webhookId}/{AuthToken}", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyWebhookAsync");
		}
		return SendJsonAsync<Webhook>("PATCH", () => $"webhooks/{webhookId}", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyWebhookAsync");
	}

	public Task DeleteWebhookAsync(ulong webhookId, RequestOptions options = null)
	{
		Preconditions.NotEqual(webhookId, 0uL, "webhookId");
		options = RequestOptions.CreateOrClone(options);
		if (AuthTokenType == TokenType.Webhook)
		{
			return SendAsync("DELETE", () => $"webhooks/{webhookId}/{AuthToken}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteWebhookAsync");
		}
		return SendAsync("DELETE", () => $"webhooks/{webhookId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteWebhookAsync");
	}

	public Task<IReadOnlyCollection<Webhook>> GetGuildWebhooksAsync(ulong guildId, RequestOptions options = null)
	{
		Preconditions.NotEqual(guildId, 0uL, "guildId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(guildId, 0uL, 0uL);
		return SendAsync<IReadOnlyCollection<Webhook>>("GET", () => $"guilds/{guildId}/webhooks", ids, ClientBucketType.Unbucketed, options, "GetGuildWebhooksAsync");
	}

	public Task<IReadOnlyCollection<Webhook>> GetChannelWebhooksAsync(ulong channelId, RequestOptions options = null)
	{
		Preconditions.NotEqual(channelId, 0uL, "channelId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, channelId, 0uL);
		return SendAsync<IReadOnlyCollection<Webhook>>("GET", () => $"channels/{channelId}/webhooks", ids, ClientBucketType.Unbucketed, options, "GetChannelWebhooksAsync");
	}

	protected void CheckState()
	{
		if (LoginState != LoginState.LoggedIn)
		{
			throw new InvalidOperationException("Client is not logged in.");
		}
	}

	protected static double ToMilliseconds(Stopwatch stopwatch)
	{
		return Math.Round((double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0, 2);
	}

	protected string SerializeJson(object value)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		using (TextWriter textWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
		{
			using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
			_serializer.Serialize(jsonWriter, value);
		}
		return stringBuilder.ToString();
	}

	protected T DeserializeJson<T>(Stream jsonStream)
	{
		using TextReader reader = new StreamReader(jsonStream);
		using JsonReader reader2 = new JsonTextReader(reader);
		return _serializer.Deserialize<T>(reader2);
	}

	protected async Task<T> NullifyNotFound<T>(Task<T> sendTask) where T : class
	{
		try
		{
			T result = await sendTask.ConfigureAwait(continueOnCapturedContext: false);
			if (sendTask.Exception != null)
			{
				if (sendTask.Exception.InnerException is HttpException { HttpCode: HttpStatusCode.NotFound })
				{
					return null;
				}
				throw sendTask.Exception;
			}
			return result;
		}
		catch (HttpException ex2) when (ex2.HttpCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	private static string GetEndpoint(Expression<Func<string>> endpointExpr)
	{
		return endpointExpr.Compile()();
	}

	private static BucketId GetBucketId(string httpMethod, BucketIds ids, Expression<Func<string>> endpointExpr, string callingMethod)
	{
		if (ids.HttpMethod == null)
		{
			string text = (ids.HttpMethod = httpMethod);
		}
		return _bucketIdGenerators.GetOrAdd(callingMethod, (string x) => CreateBucketId(endpointExpr))(ids);
	}

	private static Func<BucketIds, BucketId> CreateBucketId(Expression<Func<string>> endpoint)
	{
		try
		{
			if (endpoint.Body.NodeType == ExpressionType.Constant)
			{
				return (BucketIds x) => BucketId.Create(x.HttpMethod, (endpoint.Body as ConstantExpression).Value.ToString(), x.ToMajorParametersDictionary());
			}
			StringBuilder stringBuilder = new StringBuilder();
			Expression[] array = (endpoint.Body as MethodCallExpression).Arguments.ToArray();
			string format = (array[0] as ConstantExpression).Value as string;
			if (array.Length > 1 && array[1].NodeType == ExpressionType.NewArrayInit)
			{
				Expression[] array2 = (array[1] as NewArrayExpression).Expressions.ToArray();
				Array.Resize(ref array, array2.Length + 1);
				Array.Copy(array2, 0, array, 1, array2.Length);
			}
			int num = format.IndexOf('?');
			if (num == -1)
			{
				num = format.Length;
			}
			int num2 = 0;
			while (true)
			{
				int num3 = format.IndexOf('{', num2);
				if (num3 == -1 || num3 > num)
				{
					break;
				}
				stringBuilder.Append(format, num2, num3 - num2);
				int num4 = format.IndexOf('}', num3);
				int num5 = int.Parse(format.Substring(num3 + 1, num4 - num3 - 1), NumberStyles.None, CultureInfo.InvariantCulture);
				int? index = BucketIds.GetIndex(GetFieldName(array[num5 + 1]));
				if (!index.HasValue && num4 != num && format.Length > num4 + 1 && format[num4 + 1] == '/')
				{
					num4++;
				}
				if (index.HasValue)
				{
					stringBuilder.Append($"{{{index.Value}}}");
				}
				num2 = num4 + 1;
			}
			stringBuilder.Append(format, num2, num - num2);
			if (stringBuilder[stringBuilder.Length - 1] == '/')
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			format = stringBuilder.ToString();
			return (BucketIds x) => BucketId.Create(x.HttpMethod, string.Format(format, x.ToArray()), x.ToMajorParametersDictionary());
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Failed to generate the bucket id for this operation.", innerException);
		}
	}

	private static string GetFieldName(Expression expr)
	{
		if (expr.NodeType == ExpressionType.Convert)
		{
			expr = (expr as UnaryExpression).Operand;
		}
		if (expr.NodeType != ExpressionType.MemberAccess)
		{
			throw new InvalidOperationException("Unsupported expression");
		}
		return (expr as MemberExpression).Member.Name;
	}

	private static string WebhookQuery(bool wait = false, ulong? threadId = null)
	{
		List<string> list = new List<string>();
		if (wait)
		{
			list.Add("wait=true");
		}
		if (threadId.HasValue)
		{
			list.Add($"thread_id={threadId}");
		}
		list.Add("with_components=true");
		return string.Join("&", list) ?? "";
	}

	public Task<RoleConnectionMetadata[]> GetApplicationRoleConnectionMetadataRecordsAsync(RequestOptions options = null)
	{
		return SendAsync<RoleConnectionMetadata[]>("GET", () => $"applications/{CurrentApplicationId}/role-connections/metadata", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetApplicationRoleConnectionMetadataRecordsAsync");
	}

	public Task<RoleConnectionMetadata[]> UpdateApplicationRoleConnectionMetadataRecordsAsync(RoleConnectionMetadata[] roleConnections, RequestOptions options = null)
	{
		return SendJsonAsync<RoleConnectionMetadata[]>("PUT", () => $"applications/{CurrentApplicationId}/role-connections/metadata", roleConnections, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "UpdateApplicationRoleConnectionMetadataRecordsAsync");
	}

	public Task<RoleConnection> GetUserApplicationRoleConnectionAsync(ulong applicationId, RequestOptions options = null)
	{
		return SendAsync<RoleConnection>("GET", () => $"users/@me/applications/{applicationId}/role-connection", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetUserApplicationRoleConnectionAsync");
	}

	public Task<RoleConnection> ModifyUserApplicationRoleConnectionAsync(ulong applicationId, RoleConnection connection, RequestOptions options = null)
	{
		return SendJsonAsync<RoleConnection>("PUT", () => $"users/@me/applications/{applicationId}/role-connection", connection, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ModifyUserApplicationRoleConnectionAsync");
	}

	public Task<Entitlement> CreateEntitlementAsync(CreateEntitlementParams args, RequestOptions options = null)
	{
		return SendJsonAsync<Entitlement>("POST", () => $"applications/{CurrentApplicationId}/entitlements", args, new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "CreateEntitlementAsync");
	}

	public Task DeleteEntitlementAsync(ulong entitlementId, RequestOptions options = null)
	{
		return SendAsync("DELETE", () => $"applications/{CurrentApplicationId}/entitlements/{entitlementId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "DeleteEntitlementAsync");
	}

	public Task<Entitlement[]> ListEntitlementAsync(ListEntitlementsParams args, RequestOptions options = null)
	{
		string query = $"?limit={args.Limit.GetValueOrDefault(100)}";
		if (args.UserId.IsSpecified)
		{
			query += $"&user_id={args.UserId.Value}";
		}
		if (args.SkuIds.IsSpecified)
		{
			query = query + "&sku_ids=" + WebUtility.UrlEncode(string.Join(",", args.SkuIds.Value));
		}
		if (args.BeforeId.IsSpecified)
		{
			query += $"&before={args.BeforeId.Value}";
		}
		if (args.AfterId.IsSpecified)
		{
			query += $"&after={args.AfterId.Value}";
		}
		if (args.GuildId.IsSpecified)
		{
			query += $"&guild_id={args.GuildId.Value}";
		}
		if (args.ExcludeEnded.IsSpecified)
		{
			query += $"&exclude_ended={args.ExcludeEnded.Value}";
		}
		if (args.ExcludeDeleted.IsSpecified)
		{
			query += $"&exclude_deleted={args.ExcludeDeleted.Value}";
		}
		return SendAsync<Entitlement[]>("GET", () => $"applications/{CurrentApplicationId}/entitlements{query}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ListEntitlementAsync");
	}

	public Task<SKU[]> ListSKUsAsync(RequestOptions options = null)
	{
		return SendAsync<SKU[]>("GET", () => $"applications/{CurrentApplicationId}/skus", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ListSKUsAsync");
	}

	public Task ConsumeEntitlementAsync(ulong entitlementId, RequestOptions options = null)
	{
		return SendAsync("POST", () => $"applications/{CurrentApplicationId}/entitlements/{entitlementId}/consume", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ConsumeEntitlementAsync");
	}

	public Task<Subscription> GetSKUSubscriptionAsync(ulong skuId, ulong subscriptionId, RequestOptions options = null)
	{
		return SendAsync<Subscription>("GET", () => $"skus/{skuId}/subscriptions/{subscriptionId}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "GetSKUSubscriptionAsync");
	}

	public Task<Subscription[]> ListSKUSubscriptionsAsync(ulong skuId, ulong? before = null, ulong? after = null, int limit = 100, ulong? userId = null, RequestOptions options = null)
	{
		Preconditions.AtMost(100, limit, "Limit must be less or equal to 100.");
		Preconditions.AtLeast(1, limit, "Limit must be greater or equal to 1.");
		string args = $"?limit={limit}";
		if (before.HasValue)
		{
			args += $"&before={before}";
		}
		if (after.HasValue)
		{
			args += $"&after={after}";
		}
		if (userId.HasValue)
		{
			args += $"&user_id={userId}";
		}
		return SendAsync<Subscription[]>("GET", () => $"skus/{skuId}/subscriptions{args}", new BucketIds(0uL, 0uL, 0uL), ClientBucketType.Unbucketed, options, "ListSKUSubscriptionsAsync");
	}

	public Task<PollAnswerVoters> GetPollAnswerVotersAsync(ulong channelId, ulong messageId, uint answerId, int limit = 100, ulong? afterId = null, RequestOptions options = null)
	{
		string urlParams = $"?limit={limit}{(afterId.HasValue ? $"&after={afterId}" : string.Empty)}";
		return SendAsync<PollAnswerVoters>("GET", () => $"channels/{channelId}/polls/{messageId}/answers/{answerId}{urlParams}", new BucketIds(0uL, channelId, 0uL), ClientBucketType.Unbucketed, options, "GetPollAnswerVotersAsync");
	}

	public Task<Message> ExpirePollAsync(ulong channelId, ulong messageId, RequestOptions options = null)
	{
		return SendAsync<Message>("POST", () => $"channels/{channelId}/polls/{messageId}/expire", new BucketIds(0uL, channelId, 0uL), ClientBucketType.Unbucketed, options, "ExpirePollAsync");
	}

	public Task<Emoji> CreateApplicationEmoteAsync(CreateApplicationEmoteParams args, RequestOptions options = null)
	{
		Preconditions.NotNull(args, "args");
		Preconditions.NotNullOrWhitespace(args.Name, "Name");
		Preconditions.NotNull(args.Image.Stream, "Image");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendJsonAsync<Emoji>("POST", () => $"applications/{CurrentApplicationId}/emojis", args, ids, ClientBucketType.Unbucketed, options, "CreateApplicationEmoteAsync");
	}

	public Task<Emoji> ModifyApplicationEmoteAsync(ulong emoteId, ModifyApplicationEmoteParams args, RequestOptions options = null)
	{
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		Preconditions.NotNull(args, "args");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendJsonAsync<Emoji>("PATCH", () => $"applications/{CurrentApplicationId}/emojis/{emoteId}", args, ids, ClientBucketType.Unbucketed, options, "ModifyApplicationEmoteAsync");
	}

	public Task DeleteApplicationEmoteAsync(ulong emoteId, RequestOptions options = null)
	{
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendAsync("DELETE", () => $"applications/{CurrentApplicationId}/emojis/{emoteId}", ids, ClientBucketType.Unbucketed, options, "DeleteApplicationEmoteAsync");
	}

	public Task<Emoji> GetApplicationEmoteAsync(ulong emoteId, RequestOptions options = null)
	{
		Preconditions.NotEqual(emoteId, 0uL, "emoteId");
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendAsync<Emoji>("GET", () => $"applications/{CurrentApplicationId}/emojis/{emoteId}", ids, ClientBucketType.Unbucketed, options, "GetApplicationEmoteAsync");
	}

	public Task<ListApplicationEmojisResponse> GetApplicationEmotesAsync(RequestOptions options = null)
	{
		options = RequestOptions.CreateOrClone(options);
		BucketIds ids = new BucketIds(0uL, 0uL, 0uL);
		return SendAsync<ListApplicationEmojisResponse>("GET", () => $"applications/{CurrentApplicationId}/emojis", ids, ClientBucketType.Unbucketed, options, "GetApplicationEmotesAsync");
	}
}
