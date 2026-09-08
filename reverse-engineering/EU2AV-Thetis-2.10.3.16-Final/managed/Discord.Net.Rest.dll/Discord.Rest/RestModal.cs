using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;
using Discord.Net.Rest;

namespace Discord.Rest;

public class RestModal : RestInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>, IModalInteraction
{
	private object _lock = new object();

	public new RestModalData Data { get; set; }

	public RestUserMessage Message { get; private set; }

	IUserMessage IModalInteraction.Message => Message;

	IModalInteractionData IModalInteraction.Data => Data;

	internal RestModal(DiscordRestClient client, Interaction model)
		: base(client, model.Id)
	{
		ModalInteractionData model2 = (model.Data.IsSpecified ? ((ModalInteractionData)model.Data.Value) : null);
		if (model.Message.IsSpecified && model.ChannelId.IsSpecified)
		{
			Message = RestUserMessage.Create(base.Discord, base.Channel, base.User, model.Message.Value);
		}
		Data = new RestModalData(model2, client, base.Guild);
	}

	internal new static async Task<RestModal> CreateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		RestModal entity = new RestModal(client, model);
		await entity.UpdateAsync(client, model, doApiCall);
		return entity;
	}

	public override string Defer(bool ephemeral = false, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this) && base.Discord.ResponseInternalTimeCheck)
		{
			throw new TimeoutException($"Cannot defer an interaction after {3.0} seconds!");
		}
		InteractionResponse payload = new InteractionResponse
		{
			Type = ((Message != null) ? InteractionResponseType.DeferredUpdateMessage : InteractionResponseType.DeferredChannelMessageWithSource),
			Data = new InteractionCallbackData
			{
				Flags = (ephemeral ? ((Optional<MessageFlags>)MessageFlags.Ephemeral) : Optional<MessageFlags>.Unspecified)
			}
		};
		lock (_lock)
		{
			if (base.HasResponded)
			{
				throw new InvalidOperationException("Cannot respond or defer twice to the same interaction");
			}
		}
		lock (_lock)
		{
			base.HasResponded = true;
		}
		return SerializePayload(payload);
	}

	public string DeferLoading(bool ephemeral = false, RequestOptions options = null)
	{
		if (!InteractionHelper.CanSendResponse(this) && base.Discord.ResponseInternalTimeCheck)
		{
			throw new TimeoutException($"Cannot defer an interaction after {3.0} seconds of no response/acknowledgement");
		}
		InteractionResponse payload = new InteractionResponse
		{
			Type = InteractionResponseType.DeferredChannelMessageWithSource,
			Data = (ephemeral ? ((Optional<InteractionCallbackData>)new InteractionCallbackData
			{
				Flags = MessageFlags.Ephemeral
			}) : Optional<InteractionCallbackData>.Unspecified)
		};
		lock (_lock)
		{
			if (base.HasResponded)
			{
				throw new InvalidOperationException("Cannot respond or defer twice to the same interaction");
			}
			base.HasResponded = true;
		}
		return SerializePayload(payload);
	}

	public override Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
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
		CreateWebhookMessageParams obj = new CreateWebhookMessageParams
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.IsTTS = isTTS;
		obj.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		IMessageComponent[] array = component?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		obj.Flags = (ephemeral ? ((Optional<MessageFlags>)(flags | MessageFlags.Ephemeral)) : ((flags == MessageFlags.None) ? Optional<MessageFlags>.Unspecified : ((Optional<MessageFlags>)flags)));
		CreateWebhookMessageParams args = obj;
		return InteractionHelper.SendFollowupAsync(base.Discord, args, base.Token, base.Channel, options);
	}

	public override Task<RestFollowupMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
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
		Preconditions.NotNull(fileStream, "fileStream", "File Stream must have data");
		Preconditions.NotNullOrEmpty(fileName, "fileName", "File Name must not be empty or null");
		Preconditions.ValidatePoll(poll);
		CreateWebhookMessageParams obj = new CreateWebhookMessageParams
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.IsTTS = isTTS;
		obj.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		IMessageComponent[] array = component?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		obj.File = ((fileStream != null) ? ((Optional<MultipartFile>)new MultipartFile(fileStream, fileName)) : Optional<MultipartFile>.Unspecified);
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		obj.Flags = (ephemeral ? ((Optional<MessageFlags>)(flags | MessageFlags.Ephemeral)) : ((flags == MessageFlags.None) ? Optional<MessageFlags>.Unspecified : ((Optional<MessageFlags>)flags)));
		CreateWebhookMessageParams args = obj;
		return InteractionHelper.SendFollowupAsync(base.Discord, args, base.Token, base.Channel, options);
	}

	public override async Task<RestFollowupMessage> FollowupWithFileAsync(string filePath, string text = null, string fileName = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
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
		Preconditions.NotNullOrEmpty(filePath, "filePath", "Path must exist");
		Preconditions.ValidatePoll(poll);
		if (fileName == null)
		{
			fileName = Path.GetFileName(filePath);
		}
		Preconditions.NotNullOrEmpty(fileName, "fileName", "File Name must not be empty or null");
		using MemoryStream fileStream = ((!string.IsNullOrEmpty(filePath)) ? new MemoryStream(File.ReadAllBytes(filePath), writable: false) : null);
		CreateWebhookMessageParams obj = new CreateWebhookMessageParams
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj.IsTTS = isTTS;
		obj.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		IMessageComponent[] array = component?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		obj.File = ((fileStream != null) ? ((Optional<MultipartFile>)new MultipartFile(fileStream, fileName)) : Optional<MultipartFile>.Unspecified);
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		obj.Flags = (ephemeral ? ((Optional<MessageFlags>)(flags | MessageFlags.Ephemeral)) : ((flags == MessageFlags.None) ? Optional<MessageFlags>.Unspecified : ((Optional<MessageFlags>)flags)));
		CreateWebhookMessageParams args = obj;
		return await InteractionHelper.SendFollowupAsync(base.Discord, args, base.Token, base.Channel, options);
	}

	public override string Respond(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent component = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
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
		InteractionResponse obj = new InteractionResponse
		{
			Type = InteractionResponseType.ChannelMessageWithSource
		};
		InteractionCallbackData obj2 = new InteractionCallbackData
		{
			Content = text
		};
		global::Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj2.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<global::Discord.API.AllowedMentions>)allowedMentions2) : Optional<global::Discord.API.AllowedMentions>.Unspecified);
		obj2.Embeds = embeds.Select((Embed x) => x.ToModel()).ToArray();
		obj2.TTS = isTTS;
		IMessageComponent[] array = component?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj2.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		obj2.Flags = (ephemeral ? ((Optional<MessageFlags>)(flags | MessageFlags.Ephemeral)) : ((flags == MessageFlags.None) ? Optional<MessageFlags>.Unspecified : ((Optional<MessageFlags>)flags)));
		CreatePollParams createPollParams = poll?.ToModel();
		obj2.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		obj.Data = obj2;
		InteractionResponse payload = obj;
		lock (_lock)
		{
			if (base.HasResponded)
			{
				throw new InvalidOperationException("Cannot respond twice to the same interaction");
			}
		}
		lock (_lock)
		{
			base.HasResponded = true;
		}
		return SerializePayload(payload);
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
		UploadWebhookFileParams obj = new UploadWebhookFileParams(attachments.ToArray())
		{
			Flags = (ephemeral ? ((Optional<MessageFlags>)(flags | MessageFlags.Ephemeral)) : ((flags == MessageFlags.None) ? Optional<MessageFlags>.Unspecified : ((Optional<MessageFlags>)flags))),
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

	public override Task<RestFollowupMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None)
	{
		return FollowupWithFilesAsync(new _003C_003Ez__ReadOnlySingleElementList<FileAttachment>(attachment), text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags);
	}

	public override string RespondWithModal(Modal modal, RequestOptions requestOptions = null)
	{
		throw new NotSupportedException("Modal interactions cannot have modal responces!");
	}

	Task IModalInteraction.DeferLoadingAsync(bool ephemeral, RequestOptions options)
	{
		return Task.FromResult(DeferLoading(ephemeral, options));
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
			if (base.HasResponded)
			{
				throw new InvalidOperationException("Cannot respond, update, or defer twice to the same interaction");
			}
		}
		base.HasResponded = true;
	}
}
