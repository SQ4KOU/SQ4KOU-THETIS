using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketModal : SocketInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>, IModalInteraction
{
	private object _lock = new object();

	public new SocketModalData Data { get; set; }

	public SocketUserMessage Message { get; private set; }

	IUserMessage IModalInteraction.Message => Message;

	public override bool HasResponded { get; internal set; }

	IModalInteractionData IModalInteraction.Data => Data;

	internal SocketModal(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
		: base(client, model.Id, channel, user)
	{
		SocketModal socketModal = this;
		ModalInteractionData model2 = (model.Data.IsSpecified ? ((ModalInteractionData)model.Data.Value) : null);
		if (model.Message.IsSpecified)
		{
			SocketUser socketUser = null;
			if (base.Channel is SocketGuildChannel socketGuildChannel)
			{
				if (model.Message.Value.WebhookId.IsSpecified)
				{
					socketUser = SocketWebhookUser.Create(socketGuildChannel.Guild, base.Discord.State, model.Message.Value.Author.Value, model.Message.Value.WebhookId.Value);
				}
				else if (model.Message.Value.Author.IsSpecified)
				{
					socketUser = socketGuildChannel.Guild.GetUser(model.Message.Value.Author.Value.Id);
				}
			}
			else if (model.Message.Value.Author.IsSpecified)
			{
				socketUser = (base.Channel as SocketChannel)?.GetUser(model.Message.Value.Author.Value.Id);
			}
			if (socketUser == null)
			{
				socketUser = base.Discord.State.GetOrAddUser(model.Message.Value.Author.Value.Id, (ulong _) => SocketGlobalUser.Create(socketModal.Discord, socketModal.Discord.State, model.Message.Value.Author.Value));
			}
			Message = SocketUserMessage.Create(base.Discord, base.Discord.State, socketUser, base.Channel, model.Message.Value);
		}
		Data = new SocketModalData(model2, client, client.State, client.State.GetGuild(model.GuildId.GetValueOrDefault()), model.User.GetValueOrDefault());
	}

	internal new static SocketModal Create(DiscordSocketClient client, Interaction model, ISocketMessageChannel channel, SocketUser user)
	{
		SocketModal socketModal = new SocketModal(client, model, channel, user);
		socketModal.Update(model);
		return socketModal;
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
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.MessageComponents = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		obj.Flags = flags;
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
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users))
			{
				List<ulong> userIds = allowedMentions.UserIds;
				if (userIds != null && userIds.Count > 0)
				{
					throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
				}
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles))
			{
				List<ulong> userIds = allowedMentions.RoleIds;
				if (userIds != null && userIds.Count > 0)
				{
					throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
				}
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
		interactionCallbackData.AllowedMentions = allowedMentions?.ToModel();
		interactionCallbackData.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		interactionCallbackData.TTS = isTTS;
		interactionCallbackData.Flags = flags;
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		interactionCallbackData.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		CreatePollParams createPollParams = poll?.ToModel();
		interactionCallbackData.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		obj.Data = interactionCallbackData;
		InteractionResponse response = obj;
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond, update, or defer twice to the same interaction");
			}
		}
		await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		HasResponded = true;
	}

	public async Task UpdateAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		MessageProperties messageProperties = new MessageProperties();
		func(messageProperties);
		if (!base.IsValidToken)
		{
			throw new InvalidOperationException("Interaction token is no longer valid");
		}
		if (!InteractionHelper.CanSendResponse(this) && base.Discord.ResponseInternalTimeCheck)
		{
			throw new TimeoutException($"Cannot respond to an interaction after {3.0} seconds!");
		}
		if (messageProperties.AllowedMentions.IsSpecified)
		{
			AllowedMentions value = messageProperties.AllowedMentions.Value;
			Preconditions.AtMost((value?.RoleIds?.Count).GetValueOrDefault(), 100, "allowedMentions", "A max of 100 role Ids are allowed.");
			Preconditions.AtMost((value?.UserIds?.Count).GetValueOrDefault(), 100, "allowedMentions", "A max of 100 user Ids are allowed.");
		}
		Optional<Embed> embed = messageProperties.Embed;
		Optional<Embed[]> embeds = messageProperties.Embeds;
		List<global::Discord.API.Embed> list = ((embed.IsSpecified || embeds.IsSpecified) ? new List<global::Discord.API.Embed>() : null);
		if (embed.IsSpecified && embed.Value != null)
		{
			list.Add(embed.Value.ToModel());
		}
		if (embeds.IsSpecified && embeds.Value != null)
		{
			list.AddRange(embeds.Value.Select((Embed x) => x.ToModel()));
		}
		Preconditions.AtMost(list?.Count ?? 0, 10, "Embeds", "A max of 10 embeds are allowed.");
		if (messageProperties.AllowedMentions.IsSpecified && messageProperties.AllowedMentions.Value != null && messageProperties.AllowedMentions.Value.AllowedTypes.HasValue)
		{
			AllowedMentions value2 = messageProperties.AllowedMentions.Value;
			if (value2.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && value2.UserIds != null && value2.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "AllowedMentions");
			}
			if (value2.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && value2.RoleIds != null && value2.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "AllowedMentions");
			}
		}
		if (!messageProperties.Attachments.IsSpecified)
		{
			InteractionResponse obj = new InteractionResponse
			{
				Type = InteractionResponseType.UpdateMessage
			};
			InteractionCallbackData obj2 = new InteractionCallbackData
			{
				Content = messageProperties.Content,
				AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<global::Discord.API.AllowedMentions>)(messageProperties.AllowedMentions.Value?.ToModel())) : Optional<global::Discord.API.AllowedMentions>.Unspecified)
			};
			global::Discord.API.Embed[] array = list?.ToArray();
			obj2.Embeds = ((array != null) ? ((Optional<global::Discord.API.Embed[]>)array) : Optional<global::Discord.API.Embed[]>.Unspecified);
			obj2.Components = (messageProperties.Components.IsSpecified ? ((Optional<IMessageComponent[]>)(messageProperties.Components.Value?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray() ?? Array.Empty<IMessageComponent>())) : Optional<IMessageComponent[]>.Unspecified);
			obj2.Flags = ((!messageProperties.Flags.IsSpecified) ? Optional<MessageFlags>.Unspecified : (((Optional<MessageFlags>?)messageProperties.Flags.Value) ?? Optional<MessageFlags>.Unspecified));
			obj.Data = obj2;
			InteractionResponse response = obj;
			await InteractionHelper.SendInteractionResponseAsync(base.Discord, response, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			UploadInteractionFileParams obj3 = new UploadInteractionFileParams(messageProperties.Attachments.Value?.ToArray() ?? Array.Empty<FileAttachment>())
			{
				Type = InteractionResponseType.UpdateMessage,
				Content = messageProperties.Content,
				AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<global::Discord.API.AllowedMentions>)(messageProperties.AllowedMentions.Value?.ToModel())) : Optional<global::Discord.API.AllowedMentions>.Unspecified)
			};
			global::Discord.API.Embed[] array = list?.ToArray();
			obj3.Embeds = ((array != null) ? ((Optional<global::Discord.API.Embed[]>)array) : Optional<global::Discord.API.Embed[]>.Unspecified);
			obj3.MessageComponents = (messageProperties.Components.IsSpecified ? ((Optional<IMessageComponent[]>)(messageProperties.Components.Value?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray() ?? Array.Empty<IMessageComponent>())) : Optional<IMessageComponent[]>.Unspecified);
			obj3.Flags = ((!messageProperties.Flags.IsSpecified) ? Optional<MessageFlags>.Unspecified : (((Optional<MessageFlags>?)messageProperties.Flags.Value) ?? Optional<MessageFlags>.Unspecified));
			UploadInteractionFileParams response2 = obj3;
			await InteractionHelper.SendInteractionResponseAsync(base.Discord, response2, this, base.Channel, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond, update, or defer twice to the same interaction");
			}
		}
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
		CreateWebhookMessageParams obj = new CreateWebhookMessageParams
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.IsTTS = isTTS;
		obj.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		obj.Flags = flags;
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		CreateWebhookMessageParams args = obj;
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
			throw new TimeoutException($"Cannot defer an interaction after {3.0} seconds of no response/acknowledgement");
		}
		InteractionResponse response = new InteractionResponse
		{
			Type = ((Message != null) ? InteractionResponseType.DeferredUpdateMessage : InteractionResponseType.DeferredChannelMessageWithSource),
			Data = (ephemeral ? ((Optional<InteractionCallbackData>)new InteractionCallbackData
			{
				Flags = MessageFlags.Ephemeral
			}) : Optional<InteractionCallbackData>.Unspecified)
		};
		lock (_lock)
		{
			if (HasResponded)
			{
				throw new InvalidOperationException("Cannot respond or defer twice to the same interaction");
			}
		}
		await base.Discord.Rest.ApiClient.CreateInteractionResponseAsync(response, base.Id, base.Token, options).ConfigureAwait(continueOnCapturedContext: false);
		lock (_lock)
		{
			HasResponded = true;
		}
	}

	public async Task DeferLoadingAsync(bool ephemeral = false, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this) && base.Discord.ResponseInternalTimeCheck)
		{
			throw new TimeoutException($"Cannot defer an interaction after {3.0} seconds of no response/acknowledgement");
		}
		InteractionResponse response = new InteractionResponse
		{
			Type = InteractionResponseType.DeferredChannelMessageWithSource,
			Data = (ephemeral ? ((Optional<InteractionCallbackData>)new InteractionCallbackData
			{
				Flags = MessageFlags.Ephemeral
			}) : Optional<InteractionCallbackData>.Unspecified)
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

	public override Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
	{
		throw new NotSupportedException("You cannot respond to a modal with a modal!");
	}
}
