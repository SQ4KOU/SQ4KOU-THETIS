using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.API;
using Discord.Logging;
using Discord.Net;
using Discord.Rest;

namespace Discord.Webhook;

public class DiscordWebhookClient : IDisposable
{
	internal readonly AsyncEvent<Func<LogMessage, Task>> _logEvent = new AsyncEvent<Func<LogMessage, Task>>();

	private readonly ulong _webhookId;

	internal IWebhook Webhook;

	internal readonly Logger _restLogger;

	private static Regex WebhookUrlRegex = new Regex("^.*(discord|discordapp)\\.com\\/api\\/webhooks\\/([\\d]+)\\/([a-z0-9_-]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

	internal DiscordRestApiClient ApiClient { get; }

	internal LogManager LogManager { get; }

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

	public DiscordWebhookClient(IWebhook webhook)
		: this(webhook.Id, webhook.Token, new DiscordRestConfig())
	{
	}

	public DiscordWebhookClient(ulong webhookId, string webhookToken)
		: this(webhookId, webhookToken, new DiscordRestConfig())
	{
	}

	public DiscordWebhookClient(string webhookUrl)
		: this(webhookUrl, new DiscordRestConfig())
	{
	}

	public DiscordWebhookClient(ulong webhookId, string webhookToken, DiscordRestConfig config)
		: this(config)
	{
		_webhookId = webhookId;
		ApiClient.LoginAsync(TokenType.Webhook, webhookToken).GetAwaiter().GetResult();
		Webhook = WebhookClientHelper.GetWebhookAsync(this, webhookId).GetAwaiter().GetResult();
	}

	public DiscordWebhookClient(IWebhook webhook, DiscordRestConfig config)
		: this(config)
	{
		Webhook = webhook;
		_webhookId = Webhook.Id;
		ApiClient.LoginAsync(TokenType.Webhook, webhook.Token).GetAwaiter().GetResult();
	}

	public DiscordWebhookClient(string webhookUrl, DiscordRestConfig config)
		: this(config)
	{
		ParseWebhookUrl(webhookUrl, out _webhookId, out var webhookToken);
		ApiClient.LoginAsync(TokenType.Webhook, webhookToken).GetAwaiter().GetResult();
		Webhook = WebhookClientHelper.GetWebhookAsync(this, _webhookId).GetAwaiter().GetResult();
	}

	private DiscordWebhookClient(DiscordRestConfig config)
	{
		ApiClient = CreateApiClient(config);
		LogManager = new LogManager(config.LogLevel);
		LogManager.Message += async delegate(LogMessage msg)
		{
			await _logEvent.InvokeAsync(msg).ConfigureAwait(continueOnCapturedContext: false);
		};
		_restLogger = LogManager.CreateLogger("Rest");
		ApiClient.RequestQueue.RateLimitTriggered += async delegate(BucketId id, RateLimitInfo? info, string endpoint)
		{
			if (!info.HasValue)
			{
				await _restLogger.VerboseAsync("Preemptive Rate limit triggered: " + endpoint + " " + (id.IsHashBucket ? ("(Bucket: " + id.BucketHash + ")") : "")).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await _restLogger.WarningAsync("Rate limit triggered: " + endpoint + " " + (id.IsHashBucket ? ("(Bucket: " + id.BucketHash + ")") : "")).ConfigureAwait(continueOnCapturedContext: false);
			}
		};
		ApiClient.SentRequest += async delegate(string method, string endpoint, double millis)
		{
			await _restLogger.VerboseAsync($"{method} {endpoint}: {millis} ms").ConfigureAwait(continueOnCapturedContext: false);
		};
	}

	private static DiscordRestApiClient CreateApiClient(DiscordRestConfig config)
	{
		return new DiscordRestApiClient(config.RestClientProvider, DiscordConfig.UserAgent, RetryMode.AlwaysRetry, null, config.UseSystemClock, config.DefaultRatelimitCallback);
	}

	public Task<ulong> SendMessageAsync(string text = null, bool isTTS = false, IEnumerable<Embed> embeds = null, string username = null, string avatarUrl = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, MessageFlags flags = MessageFlags.None, ulong? threadId = null, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		return WebhookClientHelper.SendMessageAsync(this, text, isTTS, embeds, username, avatarUrl, allowedMentions, options, components, flags, threadId, threadName, appliedTags, poll);
	}

	public Task ModifyMessageAsync(ulong messageId, Action<WebhookMessageProperties> func, RequestOptions options = null, ulong? threadId = null)
	{
		return WebhookClientHelper.ModifyMessageAsync(this, messageId, func, options, threadId);
	}

	public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null, ulong? threadId = null)
	{
		return WebhookClientHelper.DeleteMessageAsync(this, messageId, options, threadId);
	}

	public Task<ulong> SendFileAsync(string filePath, string text, bool isTTS = false, IEnumerable<Embed> embeds = null, string username = null, string avatarUrl = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageComponent components = null, MessageFlags flags = MessageFlags.None, ulong? threadId = null, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		return WebhookClientHelper.SendFileAsync(this, filePath, text, isTTS, embeds, username, avatarUrl, allowedMentions, options, isSpoiler, components, flags, threadId, threadName, appliedTags, poll);
	}

	public Task<ulong> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, IEnumerable<Embed> embeds = null, string username = null, string avatarUrl = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageComponent components = null, MessageFlags flags = MessageFlags.None, ulong? threadId = null, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		return WebhookClientHelper.SendFileAsync(this, stream, filename, text, isTTS, embeds, username, avatarUrl, allowedMentions, options, isSpoiler, components, flags, threadId, threadName, appliedTags, poll);
	}

	public Task<ulong> SendFileAsync(FileAttachment attachment, string text, bool isTTS = false, IEnumerable<Embed> embeds = null, string username = null, string avatarUrl = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, MessageFlags flags = MessageFlags.None, ulong? threadId = null, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		return WebhookClientHelper.SendFileAsync(this, attachment, text, isTTS, embeds, username, avatarUrl, allowedMentions, components, options, flags, threadId, threadName, appliedTags, poll);
	}

	public Task<ulong> SendFilesAsync(IEnumerable<FileAttachment> attachments, string text, bool isTTS = false, IEnumerable<Embed> embeds = null, string username = null, string avatarUrl = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, MessageFlags flags = MessageFlags.None, ulong? threadId = null, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		return WebhookClientHelper.SendFilesAsync(this, attachments, text, isTTS, embeds, username, avatarUrl, allowedMentions, components, options, flags, threadId, threadName, appliedTags, poll);
	}

	public Task ModifyWebhookAsync(Action<WebhookProperties> func, RequestOptions options = null)
	{
		return Webhook.ModifyAsync(func, options);
	}

	public async Task DeleteWebhookAsync(RequestOptions options = null)
	{
		await Webhook.DeleteAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		Dispose();
	}

	public void Dispose()
	{
		ApiClient?.Dispose();
	}

	internal static void ParseWebhookUrl(string webhookUrl, out ulong webhookId, out string webhookToken)
	{
		if (string.IsNullOrWhiteSpace(webhookUrl))
		{
			throw new ArgumentNullException("webhookUrl", "The given webhook Url cannot be null or whitespace.");
		}
		Match match = WebhookUrlRegex.Match(webhookUrl);
		if (match != null)
		{
			if (!match.Groups[2].Success || !ulong.TryParse(match.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out webhookId))
			{
				throw ex("The webhook Id could not be parsed.");
			}
			if (!match.Groups[3].Success)
			{
				throw ex("The webhook token could not be parsed.");
			}
			webhookToken = match.Groups[3].Value;
			return;
		}
		throw ex();
		static ArgumentException ex(string reason = null)
		{
			return new ArgumentException("The given webhook Url was not in a valid format. " + reason, "webhookUrl");
		}
	}
}
