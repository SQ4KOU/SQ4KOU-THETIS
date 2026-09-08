using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Rest;

namespace Discord.Webhook;

internal static class WebhookClientHelper
{
	public static async Task<RestInternalWebhook> GetWebhookAsync(DiscordWebhookClient client, ulong webhookId)
	{
		Discord.API.Webhook webhook = await client.ApiClient.GetWebhookAsync(webhookId).ConfigureAwait(continueOnCapturedContext: false);
		if (webhook == null)
		{
			throw new InvalidOperationException("Could not find a webhook with the supplied credentials.");
		}
		return RestInternalWebhook.Create(client, webhook);
	}

	public static async Task<ulong> SendMessageAsync(DiscordWebhookClient client, string text, bool isTTS, IEnumerable<Embed> embeds, string username, string avatarUrl, AllowedMentions allowedMentions, RequestOptions options, MessageComponent components, MessageFlags flags, ulong? threadId = null, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		if (components?.Components.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow) ?? false)
		{
			flags |= MessageFlags.ComponentsV2;
		}
		Preconditions.ValidateMessageFlags(flags);
		CreateWebhookMessageParams createWebhookMessageParams = new CreateWebhookMessageParams
		{
			Content = text,
			IsTTS = isTTS,
			Flags = flags
		};
		Preconditions.WebhookMessageAtLeastOneOf(text, components, embeds?.ToArray(), null, poll);
		Preconditions.ValidatePoll(poll);
		if (embeds != null)
		{
			createWebhookMessageParams.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		}
		if (username != null)
		{
			createWebhookMessageParams.Username = username;
		}
		if (avatarUrl != null)
		{
			createWebhookMessageParams.AvatarUrl = avatarUrl;
		}
		if (allowedMentions != null)
		{
			createWebhookMessageParams.AllowedMentions = allowedMentions.ToModel();
		}
		if (components != null)
		{
			createWebhookMessageParams.Components = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		}
		if (threadName != null)
		{
			createWebhookMessageParams.ThreadName = threadName;
		}
		if (appliedTags != null)
		{
			createWebhookMessageParams.AppliedTags = appliedTags;
		}
		if (poll != null)
		{
			createWebhookMessageParams.Poll = poll.ToModel();
		}
		return (await client.ApiClient.CreateWebhookMessageAsync(client.Webhook.Id, createWebhookMessageParams, options, threadId).ConfigureAwait(continueOnCapturedContext: false)).Id;
	}

	public static Task ModifyMessageAsync(DiscordWebhookClient client, ulong messageId, Action<WebhookMessageProperties> func, RequestOptions options, ulong? threadId)
	{
		WebhookMessageProperties webhookMessageProperties = new WebhookMessageProperties();
		func(webhookMessageProperties);
		if (webhookMessageProperties.AllowedMentions.IsSpecified)
		{
			AllowedMentions value = webhookMessageProperties.AllowedMentions.Value;
			Preconditions.AtMost((value?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
			Preconditions.AtMost((value?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
			if (value != null && value.AllowedTypes.HasValue)
			{
				if (value.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && value.UserIds != null && value.UserIds.Count > 0)
				{
					throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
				}
				if (value.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && value.RoleIds != null && value.RoleIds.Count > 0)
				{
					throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
				}
			}
		}
		if (!webhookMessageProperties.Attachments.IsSpecified)
		{
			ModifyWebhookMessageParams args = new ModifyWebhookMessageParams
			{
				Content = (webhookMessageProperties.Content.IsSpecified ? ((Optional<string>)webhookMessageProperties.Content.Value) : Optional.Create<string>()),
				Embeds = (webhookMessageProperties.Embeds.IsSpecified ? ((Optional<Discord.API.Embed[]>)webhookMessageProperties.Embeds.Value.Select((Embed embed) => embed.ToModel()).ToArray()) : Optional.Create<Discord.API.Embed[]>()),
				AllowedMentions = (webhookMessageProperties.AllowedMentions.IsSpecified ? ((Optional<Discord.API.AllowedMentions>)webhookMessageProperties.AllowedMentions.Value.ToModel()) : Optional.Create<Discord.API.AllowedMentions>()),
				Components = (webhookMessageProperties.Components.IsSpecified ? ((Optional<IMessageComponent[]>)(webhookMessageProperties.Components.Value?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray())) : Optional<IMessageComponent[]>.Unspecified)
			};
			return client.ApiClient.ModifyWebhookMessageAsync(client.Webhook.Id, messageId, args, options, threadId);
		}
		UploadWebhookFileParams args2 = new UploadWebhookFileParams(webhookMessageProperties.Attachments.Value?.ToArray() ?? Array.Empty<FileAttachment>())
		{
			Content = (webhookMessageProperties.Content.IsSpecified ? ((Optional<string>)webhookMessageProperties.Content.Value) : Optional.Create<string>()),
			Embeds = (webhookMessageProperties.Embeds.IsSpecified ? ((Optional<Discord.API.Embed[]>)webhookMessageProperties.Embeds.Value.Select((Embed embed) => embed.ToModel()).ToArray()) : Optional.Create<Discord.API.Embed[]>()),
			AllowedMentions = (webhookMessageProperties.AllowedMentions.IsSpecified ? ((Optional<Discord.API.AllowedMentions>)webhookMessageProperties.AllowedMentions.Value.ToModel()) : Optional.Create<Discord.API.AllowedMentions>()),
			MessageComponents = (webhookMessageProperties.Components.IsSpecified ? ((Optional<IMessageComponent[]>)(webhookMessageProperties.Components.Value?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray())) : Optional<IMessageComponent[]>.Unspecified)
		};
		return client.ApiClient.ModifyWebhookMessageAsync(client.Webhook.Id, messageId, args2, options, threadId);
	}

	public static Task DeleteMessageAsync(DiscordWebhookClient client, ulong messageId, RequestOptions options, ulong? threadId)
	{
		return client.ApiClient.DeleteWebhookMessageAsync(client.Webhook.Id, messageId, options, threadId);
	}

	public static async Task<ulong> SendFileAsync(DiscordWebhookClient client, string filePath, string text, bool isTTS, IEnumerable<Embed> embeds, string username, string avatarUrl, AllowedMentions allowedMentions, RequestOptions options, bool isSpoiler, MessageComponent components, MessageFlags flags = MessageFlags.None, ulong? threadId = null, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		string fileName = Path.GetFileName(filePath);
		using FileStream file = File.OpenRead(filePath);
		return await SendFileAsync(client, file, fileName, text, isTTS, embeds, username, avatarUrl, allowedMentions, options, isSpoiler, components, flags, threadId, threadName, appliedTags, poll).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task<ulong> SendFileAsync(DiscordWebhookClient client, Stream stream, string filename, string text, bool isTTS, IEnumerable<Embed> embeds, string username, string avatarUrl, AllowedMentions allowedMentions, RequestOptions options, bool isSpoiler, MessageComponent components, MessageFlags flags, ulong? threadId, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		return SendFileAsync(client, new FileAttachment(stream, filename, null, isSpoiler), text, isTTS, embeds, username, avatarUrl, allowedMentions, components, options, flags, threadId, threadName, appliedTags, poll);
	}

	public static Task<ulong> SendFileAsync(DiscordWebhookClient client, FileAttachment attachment, string text, bool isTTS, IEnumerable<Embed> embeds, string username, string avatarUrl, AllowedMentions allowedMentions, MessageComponent components, RequestOptions options, MessageFlags flags, ulong? threadId, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		return SendFilesAsync(client, new FileAttachment[1] { attachment }, text, isTTS, embeds, username, avatarUrl, allowedMentions, components, options, flags, threadId, threadName, appliedTags, poll);
	}

	public static async Task<ulong> SendFilesAsync(DiscordWebhookClient client, IEnumerable<FileAttachment> attachments, string text, bool isTTS, IEnumerable<Embed> embeds, string username, string avatarUrl, AllowedMentions allowedMentions, MessageComponent components, RequestOptions options, MessageFlags flags, ulong? threadId, string threadName = null, ulong[] appliedTags = null, PollProperties poll = null)
	{
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Count(), 10, "embeds", $"A max of {10} Embeds are allowed.");
		Preconditions.ValidatePoll(poll);
		Preconditions.WebhookMessageAtLeastOneOf(text, components, embeds.ToArray(), attachments, poll);
		foreach (FileAttachment attachment in attachments)
		{
			Preconditions.NotNullOrEmpty(attachment.FileName, "FileName", "File Name must not be empty or null");
		}
		if (allowedMentions != null && allowedMentions.AllowedTypes.HasValue)
		{
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
			}
		}
		if (components?.Components.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow) ?? false)
		{
			flags |= MessageFlags.ComponentsV2;
		}
		Preconditions.ValidateMessageFlags(flags);
		UploadWebhookFileParams obj = new UploadWebhookFileParams(attachments.ToArray())
		{
			AvatarUrl = avatarUrl,
			Username = username,
			Content = text,
			IsTTS = isTTS,
			Embeds = (embeds.Any() ? ((Optional<Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<Discord.API.Embed[]>.Unspecified)
		};
		Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<Discord.API.AllowedMentions>)allowedMentions2) : Optional<Discord.API.AllowedMentions>.Unspecified);
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.MessageComponents = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		obj.Flags = flags;
		obj.ThreadName = threadName;
		obj.AppliedTags = appliedTags;
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		UploadWebhookFileParams args = obj;
		return (await client.ApiClient.UploadWebhookFileAsync(client.Webhook.Id, args, options, threadId).ConfigureAwait(continueOnCapturedContext: false)).Id;
	}

	public static Task<Discord.API.Webhook> ModifyAsync(DiscordWebhookClient client, Action<WebhookProperties> func, RequestOptions options)
	{
		WebhookProperties webhookProperties = new WebhookProperties();
		func(webhookProperties);
		ModifyWebhookParams modifyWebhookParams = new ModifyWebhookParams
		{
			Avatar = (webhookProperties.Image.IsSpecified ? ((Optional<Discord.API.Image?>)(webhookProperties.Image.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			Name = webhookProperties.Name
		};
		if (!modifyWebhookParams.Avatar.IsSpecified && client.Webhook.AvatarId != null)
		{
			modifyWebhookParams.Avatar = new Discord.API.Image(client.Webhook.AvatarId);
		}
		return client.ApiClient.ModifyWebhookAsync(client.Webhook.Id, modifyWebhookParams, options);
	}

	public static Task DeleteAsync(DiscordWebhookClient client, RequestOptions options)
	{
		return client.ApiClient.DeleteWebhookAsync(client.Webhook.Id, options);
	}
}
