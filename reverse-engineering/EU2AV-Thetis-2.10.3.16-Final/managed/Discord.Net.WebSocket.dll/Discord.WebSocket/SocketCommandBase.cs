using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketCommandBase : SocketInteraction
{
	private object _lock = new object();

	public string CommandName => Data.Name;

	public ulong CommandId => Data.Id;

	internal new SocketCommandBaseData Data { get; }

	public override bool HasResponded { get; internal set; }

	internal SocketCommandBase(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
		: base(client, model.Id, channel, user)
	{
		ApplicationCommandInteractionData model2 = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
		ulong? guildId = model.GuildId.ToNullable();
		Data = SocketCommandBaseData<IApplicationCommandInteractionDataOption>.Create(client, model2, model.Id, guildId);
	}

	internal new static SocketInteraction Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		SocketCommandBase socketCommandBase = new SocketCommandBase(client, model, channel, user);
		socketCommandBase.Update(model);
		return socketCommandBase;
	}

	internal override void Update(Interaction model)
	{
		ApplicationCommandInteractionData model2 = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
		Data.Update(model2);
		base.Update(model);
	}

	public override async Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (!InteractionHelper.CanSendResponse(this) && base.Discord.ResponseInternalTimeCheck)
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		Preconditions.ValidatePoll(poll);
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
		if (components?.Components?.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow) == true)
		{
			flags |= MessageFlags.ComponentsV2;
		}
		if (ephemeral)
		{
			flags |= MessageFlags.Ephemeral;
		}
		Preconditions.ValidateMessageFlags(flags);
		InteractionResponse obj = new InteractionResponse
		{
			Type = InteractionResponseType.ChannelMessageWithSource
		};
		InteractionCallbackData interactionCallbackData = new InteractionCallbackData();
		interactionCallbackData.Content = ((text != null) ? ((Optional<string>)text) : Optional<string>.Unspecified);
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		interactionCallbackData.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		interactionCallbackData.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		interactionCallbackData.TTS = isTTS;
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		interactionCallbackData.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		interactionCallbackData.Flags = flags;
		CreatePollParams createPollParams = poll?.ToModel();
		interactionCallbackData.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		obj.Data = interactionCallbackData;
		InteractionResponse response = obj;
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond twice to the same interaction");
			}
		}
		await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		HasResponded = true;
	}

	public override async Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (!InteractionHelper.CanSendResponse(this) && base.Discord.ResponseInternalTimeCheck)
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		InteractionResponse obj = new InteractionResponse
		{
			Type = InteractionResponseType.Modal
		};
		InteractionCallbackData obj2 = new InteractionCallbackData
		{
			CustomId = modal.CustomId,
			Title = modal.Title
		};
		IMessageComponent[] array = modal.Component.Components.Select((ActionRowComponent x) => new global::Discord.API.ActionRowComponent(x)).ToArray();
		obj2.Components = array;
		obj.Data = obj2;
		InteractionResponse response = obj;
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond twice to the same interaction");
			}
		}
		await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		lock (_lock)
		{
			HasResponded = true;
		}
	}

	public override async Task RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (!InteractionHelper.CanSendResponse(this) && base.Discord.ResponseInternalTimeCheck)
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		Preconditions.ValidatePoll(poll);
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
		if (components?.Components?.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow) == true)
		{
			flags |= MessageFlags.ComponentsV2;
		}
		if (ephemeral)
		{
			flags |= MessageFlags.Ephemeral;
		}
		Preconditions.ValidateMessageFlags(flags);
		UploadInteractionFileParams obj = new UploadInteractionFileParams(attachments?.ToArray())
		{
			Type = InteractionResponseType.ChannelMessageWithSource
		};
		obj.Content = ((text != null) ? ((Optional<string>)text) : Optional<string>.Unspecified);
		obj.AllowedMentions = ((allowedMentions != null) ? ((Optional<global::Discord.API.AllowedMentions>)(allowedMentions?.ToModel())) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.Embeds = (embeds.Any() ? ((Optional<global::Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<global::Discord.API.Embed[]>.Unspecified);
		obj.IsTTS = isTTS;
		obj.Flags = flags;
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.MessageComponents = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		UploadInteractionFileParams response = obj;
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond, update, or defer the same interaction twice");
			}
		}
		await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		HasResponded = true;
	}

	public override Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		Preconditions.ValidatePoll(poll);
		if (components?.Components?.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow) == true)
		{
			flags |= MessageFlags.ComponentsV2;
		}
		if (ephemeral)
		{
			flags |= MessageFlags.Ephemeral;
		}
		Preconditions.ValidateMessageFlags(flags);
		CreateWebhookMessageParams createWebhookMessageParams = new CreateWebhookMessageParams();
		createWebhookMessageParams.Content = ((text != null) ? ((Optional<string>)text) : Optional<string>.Unspecified);
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		createWebhookMessageParams.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		createWebhookMessageParams.IsTTS = isTTS;
		createWebhookMessageParams.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		createWebhookMessageParams.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		CreatePollParams createPollParams = poll?.ToModel();
		createWebhookMessageParams.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		createWebhookMessageParams.Flags = flags;
		CreateWebhookMessageParams args = createWebhookMessageParams;
		return InteractionHelper.SendFollowupAsync(base.Discord.Rest, args, base.Token, base.Channel, options);
	}

	public override Task<RestFollowupMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		Preconditions.ValidatePoll(poll);
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
		if (components?.Components?.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow) == true)
		{
			flags |= MessageFlags.ComponentsV2;
		}
		if (ephemeral)
		{
			flags |= MessageFlags.Ephemeral;
		}
		Preconditions.ValidateMessageFlags(flags);
		UploadWebhookFileParams obj = new UploadWebhookFileParams(attachments.ToArray())
		{
			Flags = flags,
			Content = text,
			IsTTS = isTTS,
			Embeds = (embeds.Any() ? ((Optional<global::Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<global::Discord.API.Embed[]>.Unspecified)
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.MessageComponents = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		UploadWebhookFileParams args = obj;
		return InteractionHelper.SendFollowupAsync(base.Discord, args, base.Token, base.Channel, options);
	}

	public override async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this) && base.Discord.ResponseInternalTimeCheck)
		{
			throw new TimeoutException($"Cannot defer an interaction after {3.0} seconds!");
		}
		InteractionResponse response = new InteractionResponse
		{
			Type = InteractionResponseType.DeferredChannelMessageWithSource,
			Data = new InteractionCallbackData
			{
				Flags = (ephemeral ? ((Optional<MessageFlags>)MessageFlags.Ephemeral) : Optional<MessageFlags>.Unspecified)
			}
		};
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond or defer twice to the same interaction");
			}
		}
		await base.Discord.Rest.ApiClient.CreateInteractionResponseAsync(response, base.Id, base.Token, options).ConfigureAwait(continueOnCapturedContext: false);
		HasResponded = true;
	}
}
