using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class ChannelHelper
{
	public static Task DeleteAsync(IChannel channel, BaseDiscordClient client, RequestOptions options)
	{
		return client.ApiClient.DeleteChannelAsync(channel.Id, options);
	}

	public static Task<Channel> ModifyAsync(IGuildChannel channel, BaseDiscordClient client, Action<GuildChannelProperties> func, RequestOptions options)
	{
		GuildChannelProperties guildChannelProperties = new GuildChannelProperties();
		func(guildChannelProperties);
		ModifyGuildChannelParams args = new ModifyGuildChannelParams
		{
			Name = guildChannelProperties.Name,
			Position = guildChannelProperties.Position,
			CategoryId = guildChannelProperties.CategoryId,
			Overwrites = (guildChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)guildChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			Flags = guildChannelProperties.Flags.GetValueOrDefault()
		};
		return client.ApiClient.ModifyGuildChannelAsync(channel.Id, args, options);
	}

	public static Task<Channel> ModifyAsync(ITextChannel channel, BaseDiscordClient client, Action<TextChannelProperties> func, RequestOptions options)
	{
		TextChannelProperties textChannelProperties = new TextChannelProperties();
		func(textChannelProperties);
		ModifyTextChannelParams args = new ModifyTextChannelParams
		{
			Type = textChannelProperties.ChannelType,
			Name = textChannelProperties.Name,
			Position = textChannelProperties.Position,
			CategoryId = textChannelProperties.CategoryId,
			Topic = textChannelProperties.Topic,
			IsNsfw = textChannelProperties.IsNsfw,
			SlowModeInterval = textChannelProperties.SlowModeInterval,
			Overwrites = (textChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)textChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			DefaultSlowModeInterval = textChannelProperties.DefaultSlowModeInterval
		};
		return client.ApiClient.ModifyGuildChannelAsync(channel.Id, args, options);
	}

	public static Task<Channel> ModifyAsync(IVoiceChannel channel, BaseDiscordClient client, Action<VoiceChannelProperties> func, RequestOptions options)
	{
		VoiceChannelProperties voiceChannelProperties = new VoiceChannelProperties();
		func(voiceChannelProperties);
		ModifyVoiceChannelParams args = new ModifyVoiceChannelParams
		{
			Bitrate = voiceChannelProperties.Bitrate,
			Name = voiceChannelProperties.Name,
			RTCRegion = voiceChannelProperties.RTCRegion,
			Position = voiceChannelProperties.Position,
			CategoryId = voiceChannelProperties.CategoryId,
			UserLimit = (voiceChannelProperties.UserLimit.IsSpecified ? ((Optional<int>)voiceChannelProperties.UserLimit.Value.GetValueOrDefault()) : Optional.Create<int>()),
			Overwrites = (voiceChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)voiceChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			SlowModeInterval = voiceChannelProperties.SlowModeInterval,
			IsNsfw = voiceChannelProperties.IsNsfw
		};
		return client.ApiClient.ModifyGuildChannelAsync(channel.Id, args, options);
	}

	public static Task<StageInstance> ModifyAsync(IStageChannel channel, BaseDiscordClient client, Action<StageInstanceProperties> func, RequestOptions options = null)
	{
		StageInstanceProperties stageInstanceProperties = new StageInstanceProperties();
		func(stageInstanceProperties);
		ModifyStageInstanceParams args = new ModifyStageInstanceParams
		{
			PrivacyLevel = stageInstanceProperties.PrivacyLevel,
			Topic = stageInstanceProperties.Topic
		};
		return client.ApiClient.ModifyStageInstanceAsync(channel.Id, args, options);
	}

	public static async Task<IReadOnlyCollection<RestInviteMetadata>> GetInvitesAsync(IGuildChannel channel, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetChannelInvitesAsync(channel.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((InviteMetadata x) => RestInviteMetadata.Create(client, null, channel, x)).ToImmutableArray();
	}

	public static async Task<RestInviteMetadata> CreateInviteAsync(IGuildChannel channel, BaseDiscordClient client, int? maxAge, int? maxUses, bool isTemporary, bool isUnique, RequestOptions options)
	{
		CreateChannelInviteParams args = new CreateChannelInviteParams
		{
			IsTemporary = isTemporary,
			IsUnique = isUnique,
			MaxAge = maxAge.GetValueOrDefault(),
			MaxUses = maxUses.GetValueOrDefault()
		};
		return RestInviteMetadata.Create(client, null, channel, await client.ApiClient.CreateChannelInviteAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestInviteMetadata> CreateInviteToStreamAsync(IGuildChannel channel, BaseDiscordClient client, int? maxAge, int? maxUses, bool isTemporary, bool isUnique, IUser user, RequestOptions options)
	{
		CreateChannelInviteParams args = new CreateChannelInviteParams
		{
			IsTemporary = isTemporary,
			IsUnique = isUnique,
			MaxAge = maxAge.GetValueOrDefault(),
			MaxUses = maxUses.GetValueOrDefault(),
			TargetType = TargetUserType.Stream,
			TargetUserId = user.Id
		};
		return RestInviteMetadata.Create(client, null, channel, await client.ApiClient.CreateChannelInviteAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestInviteMetadata> CreateInviteToApplicationAsync(IGuildChannel channel, BaseDiscordClient client, int? maxAge, int? maxUses, bool isTemporary, bool isUnique, ulong applicationId, RequestOptions options)
	{
		CreateChannelInviteParams args = new CreateChannelInviteParams
		{
			IsTemporary = isTemporary,
			IsUnique = isUnique,
			MaxAge = maxAge.GetValueOrDefault(),
			MaxUses = maxUses.GetValueOrDefault(),
			TargetType = TargetUserType.EmbeddedApplication,
			TargetApplicationId = applicationId
		};
		return RestInviteMetadata.Create(client, null, channel, await client.ApiClient.CreateChannelInviteAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestMessage> GetMessageAsync(IMessageChannel channel, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		ulong? num = (channel as IGuildChannel)?.GuildId;
		IGuild guild = ((!num.HasValue) ? null : (await ((IDiscordClient)client).GetGuildAsync(num.Value, CacheMode.CacheOnly, (RequestOptions)null).ConfigureAwait(continueOnCapturedContext: false)));
		IGuild guild2 = guild;
		Message message = await client.ApiClient.GetChannelMessageAsync(channel.Id, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (message == null)
		{
			return null;
		}
		IUser author = MessageHelper.GetAuthor(client, guild2, message.Author.Value, message.WebhookId.ToNullable());
		return RestMessage.Create(client, channel, author, message);
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(IMessageChannel channel, BaseDiscordClient client, ulong? fromMessageId, Direction dir, int limit, RequestOptions options)
	{
		ulong? num = (channel as IGuildChannel)?.GuildId;
		IGuild guild = (num.HasValue ? ((IDiscordClient)client).GetGuildAsync(num.Value, CacheMode.CacheOnly, (RequestOptions)null).Result : null);
		if (dir == Direction.Around && limit > 100)
		{
			int num2 = limit / 2;
			if (fromMessageId.HasValue)
			{
				return GetMessagesAsync(channel, client, fromMessageId.Value + 1, Direction.Before, num2 + 1, options).Concat(GetMessagesAsync(channel, client, fromMessageId, Direction.After, num2, options));
			}
			return GetMessagesAsync(channel, client, null, Direction.Before, num2 + 1, options);
		}
		return new PagedAsyncEnumerable<RestMessage>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetChannelMessagesParams getChannelMessagesParams = new GetChannelMessagesParams
			{
				RelativeDirection = dir,
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getChannelMessagesParams.RelativeMessageId = info.Position.Value;
			}
			IReadOnlyCollection<Message> obj = await client.ApiClient.GetChannelMessagesAsync(channel.Id, getChannelMessagesParams, options).ConfigureAwait(continueOnCapturedContext: false);
			ImmutableArray<RestMessage>.Builder builder = ImmutableArray.CreateBuilder<RestMessage>();
			foreach (Message item in obj)
			{
				IUser author = MessageHelper.GetAuthor(client, guild, item.Author.Value, item.WebhookId.ToNullable());
				builder.Add(RestMessage.Create(client, channel, author, item));
			}
			return builder.ToImmutable();
		}, delegate(PageInfo info, IReadOnlyCollection<RestMessage> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			if (dir == Direction.Before)
			{
				info.Position = lastPage.Min((RestMessage x) => x.Id);
			}
			else
			{
				info.Position = lastPage.Max((RestMessage x) => x.Id);
			}
			return true;
		}, fromMessageId, limit);
	}

	public static async Task<IReadOnlyCollection<RestMessage>> GetPinnedMessagesAsync(IMessageChannel channel, BaseDiscordClient client, RequestOptions options)
	{
		ulong? num = (channel as IGuildChannel)?.GuildId;
		IGuild guild = ((!num.HasValue) ? null : (await ((IDiscordClient)client).GetGuildAsync(num.Value, CacheMode.CacheOnly, (RequestOptions)null).ConfigureAwait(continueOnCapturedContext: false)));
		IGuild guild2 = guild;
		IReadOnlyCollection<Message> obj = await client.ApiClient.GetPinsAsync(channel.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<RestMessage>.Builder builder = ImmutableArray.CreateBuilder<RestMessage>();
		foreach (Message item in obj)
		{
			IUser author = MessageHelper.GetAuthor(client, guild2, item.Author.Value, item.WebhookId.ToNullable());
			builder.Add(RestMessage.Create(client, channel, author, item));
		}
		return builder.ToImmutable();
	}

	public static async Task<RestUserMessage> SendMessageAsync(IMessageChannel channel, BaseDiscordClient client, string text, bool isTTS, Embed embed, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, RequestOptions options, Embed[] embeds, MessageFlags flags, PollProperties poll)
	{
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
		Preconditions.AtMost(embeds.Length, 10, "embeds", $"A max of {10} Embeds are allowed.");
		Preconditions.MessageAtLeastOneOf(text, components, embeds, stickers, null, poll, messageReference);
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
		if (stickers != null)
		{
			Preconditions.AtMost(stickers.Length, 3, "stickers", "A max of 3 stickers are allowed.");
		}
		if (components?.Components?.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow) == true)
		{
			flags |= MessageFlags.ComponentsV2;
		}
		Preconditions.ValidateMessageFlags(flags);
		CreateMessageParams obj = new CreateMessageParams
		{
			Content = text,
			IsTTS = isTTS,
			Embeds = (embeds.Any() ? ((Optional<Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<Discord.API.Embed[]>.Unspecified),
			AllowedMentions = allowedMentions?.ToModel(),
			MessageReference = messageReference?.ToModel()
		};
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.Components = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		obj.Stickers = ((stickers?.Any() ?? false) ? ((Optional<ulong[]>)stickers.Select((ISticker x) => x.Id).ToArray()) : Optional<ulong[]>.Unspecified);
		obj.Flags = flags;
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		CreateMessageParams args = obj;
		Message model = await client.ApiClient.CreateMessageAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false);
		return RestUserMessage.Create(client, channel, client.CurrentUser, model);
	}

	public static async Task<RestUserMessage> SendFileAsync(IMessageChannel channel, BaseDiscordClient client, string filePath, string text, bool isTTS, Embed embed, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, RequestOptions options, bool isSpoiler, Embed[] embeds, MessageFlags flags = MessageFlags.None, PollProperties poll = null)
	{
		string fileName = Path.GetFileName(filePath);
		using FileStream file = File.OpenRead(filePath);
		return await SendFileAsync(channel, client, file, fileName, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, isSpoiler, embeds, flags, poll).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<RestUserMessage> SendFileAsync(IMessageChannel channel, BaseDiscordClient client, Stream stream, string filename, string text, bool isTTS, Embed embed, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, RequestOptions options, bool isSpoiler, Embed[] embeds, MessageFlags flags = MessageFlags.None, PollProperties poll = null)
	{
		using FileAttachment file = new FileAttachment(stream, filename, null, isSpoiler);
		return await SendFileAsync(channel, client, file, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, embeds, flags, poll).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task<RestUserMessage> SendFileAsync(IMessageChannel channel, BaseDiscordClient client, FileAttachment attachment, string text, bool isTTS, Embed embed, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, RequestOptions options, Embed[] embeds, MessageFlags flags = MessageFlags.None, PollProperties poll = null)
	{
		return SendFilesAsync(channel, client, new FileAttachment[1] { attachment }, text, isTTS, embed, allowedMentions, messageReference, components, stickers, options, embeds, flags, poll);
	}

	public static async Task<RestUserMessage> SendFilesAsync(IMessageChannel channel, BaseDiscordClient client, IEnumerable<FileAttachment> attachments, string text, bool isTTS, Embed embed, AllowedMentions allowedMentions, MessageReference messageReference, MessageComponent components, ISticker[] stickers, RequestOptions options, Embed[] embeds, MessageFlags flags, PollProperties poll = null)
	{
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
		Preconditions.AtMost(embeds.Length, 10, "embeds", $"A max of {10} Embeds are allowed.");
		Preconditions.MessageAtLeastOneOf(text, components, embeds, stickers, attachments, poll, messageReference);
		Preconditions.ValidatePoll(poll);
		foreach (FileAttachment attachment in attachments)
		{
			Preconditions.NotNullOrEmpty(attachment.FileName, "FileName", "File Name must not be empty or null");
			if (channel is ITextChannel textChannel && attachment.Stream.CanSeek && (ulong)attachment.Stream.Length > textChannel.Guild.MaxUploadLimit)
			{
				throw new ArgumentOutOfRangeException("attachments", $"File size exceeds the max file size of {textChannel.Guild.MaxUploadLimit} bytes in that guild!");
			}
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
		Preconditions.ValidateMessageFlags(flags);
		if (stickers != null)
		{
			Preconditions.AtMost(stickers.Length, 3, "stickers", "A max of 3 stickers are allowed.");
		}
		UploadFileParams obj = new UploadFileParams(attachments.ToArray())
		{
			Content = text,
			IsTTS = isTTS,
			Embeds = (embeds.Any() ? ((Optional<Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<Discord.API.Embed[]>.Unspecified)
		};
		Discord.API.AllowedMentions allowedMentions2 = allowedMentions?.ToModel();
		obj.AllowedMentions = ((allowedMentions2 != null) ? ((Optional<Discord.API.AllowedMentions>)allowedMentions2) : Optional<Discord.API.AllowedMentions>.Unspecified);
		Discord.API.MessageReference messageReference2 = messageReference?.ToModel();
		obj.MessageReference = ((messageReference2 != null) ? ((Optional<Discord.API.MessageReference>)messageReference2) : Optional<Discord.API.MessageReference>.Unspecified);
		IMessageComponent[] array = components?.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		obj.MessageComponent = ((array != null) ? ((Optional<IMessageComponent[]>)array) : Optional<IMessageComponent[]>.Unspecified);
		obj.Stickers = ((stickers?.Any() ?? false) ? ((Optional<ulong[]>)stickers.Select((ISticker x) => x.Id).ToArray()) : Optional<ulong[]>.Unspecified);
		obj.Flags = flags;
		CreatePollParams createPollParams = poll?.ToModel();
		obj.Poll = ((createPollParams != null) ? ((Optional<CreatePollParams>)createPollParams) : Optional<CreatePollParams>.Unspecified);
		UploadFileParams args = obj;
		Message model = await client.ApiClient.UploadFileAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false);
		return RestUserMessage.Create(client, channel, client.CurrentUser, model);
	}

	public static async Task<RestUserMessage> ModifyMessageAsync(IMessageChannel channel, ulong messageId, Action<MessageProperties> func, BaseDiscordClient client, RequestOptions options)
	{
		Message message = await MessageHelper.ModifyAsync(channel.Id, messageId, client, func, options).ConfigureAwait(continueOnCapturedContext: false);
		IUser author;
		if (!message.Author.IsSpecified)
		{
			IUser currentUser = client.CurrentUser;
			author = currentUser;
		}
		else
		{
			IUser currentUser = RestUser.Create(client, message.Author.Value);
			author = currentUser;
		}
		return RestUserMessage.Create(client, channel, author, message);
	}

	public static Task DeleteMessageAsync(IMessageChannel channel, ulong messageId, BaseDiscordClient client, RequestOptions options)
	{
		return MessageHelper.DeleteAsync(channel.Id, messageId, client, options);
	}

	public static async Task DeleteMessagesAsync(ITextChannel channel, BaseDiscordClient client, IEnumerable<ulong> messageIds, RequestOptions options)
	{
		ulong[] msgs = messageIds.ToArray();
		int batches = msgs.Length / 100;
		for (int i = 0; i <= batches; i++)
		{
			ArraySegment<ulong> arraySegment;
			if (i < batches)
			{
				arraySegment = new ArraySegment<ulong>(msgs, i * 100, 100);
			}
			else
			{
				arraySegment = new ArraySegment<ulong>(msgs, i * 100, msgs.Length - batches * 100);
				if (arraySegment.Count == 0)
				{
					break;
				}
			}
			DeleteMessagesParams args = new DeleteMessagesParams(arraySegment.ToArray());
			await client.ApiClient.DeleteMessagesAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static Task AddPermissionOverwriteAsync(IGuildChannel channel, BaseDiscordClient client, IUser user, OverwritePermissions perms, RequestOptions options)
	{
		ModifyChannelPermissionsParams args = new ModifyChannelPermissionsParams(1, perms.AllowValue.ToString(), perms.DenyValue.ToString());
		return client.ApiClient.ModifyChannelPermissionsAsync(channel.Id, user.Id, args, options);
	}

	public static Task AddPermissionOverwriteAsync(IGuildChannel channel, BaseDiscordClient client, IRole role, OverwritePermissions perms, RequestOptions options)
	{
		ModifyChannelPermissionsParams args = new ModifyChannelPermissionsParams(0, perms.AllowValue.ToString(), perms.DenyValue.ToString());
		return client.ApiClient.ModifyChannelPermissionsAsync(channel.Id, role.Id, args, options);
	}

	public static Task RemovePermissionOverwriteAsync(IGuildChannel channel, BaseDiscordClient client, IUser user, RequestOptions options)
	{
		return client.ApiClient.DeleteChannelPermissionAsync(channel.Id, user.Id, options);
	}

	public static Task RemovePermissionOverwriteAsync(IGuildChannel channel, BaseDiscordClient client, IRole role, RequestOptions options)
	{
		return client.ApiClient.DeleteChannelPermissionAsync(channel.Id, role.Id, options);
	}

	public static async Task<RestGuildUser> GetUserAsync(IGuildChannel channel, IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		GuildMember guildMember = await client.ApiClient.GetGuildMemberAsync(channel.GuildId, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildMember == null)
		{
			return null;
		}
		RestGuildUser restGuildUser = RestGuildUser.Create(client, guild, guildMember);
		if (!restGuildUser.GetPermissions(channel).ViewChannel)
		{
			return null;
		}
		return restGuildUser;
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> GetUsersAsync(IGuildChannel channel, IGuild guild, BaseDiscordClient client, ulong? fromUserId, int? limit, RequestOptions options)
	{
		return new PagedAsyncEnumerable<RestGuildUser>(1000, async delegate(PageInfo info, CancellationToken ct)
		{
			GetGuildMembersParams getGuildMembersParams = new GetGuildMembersParams
			{
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getGuildMembersParams.AfterUserId = info.Position.Value;
			}
			return (from x in await client.ApiClient.GetGuildMembersAsync(guild.Id, getGuildMembersParams, options).ConfigureAwait(continueOnCapturedContext: false)
				select RestGuildUser.Create(client, guild, x) into x
				where x.GetPermissions(channel).ViewChannel
				select x).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestGuildUser> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Max((RestGuildUser x) => x.Id);
			return true;
		}, fromUserId, limit);
	}

	public static Task TriggerTypingAsync(IMessageChannel channel, BaseDiscordClient client, RequestOptions options = null)
	{
		return client.ApiClient.TriggerTypingIndicatorAsync(channel.Id, options);
	}

	public static IDisposable EnterTypingState(IMessageChannel channel, BaseDiscordClient client, RequestOptions options)
	{
		return new TypingNotifier(channel, options);
	}

	public static async Task<RestWebhook> CreateWebhookAsync(IIntegrationChannel channel, BaseDiscordClient client, string name, Stream avatar, RequestOptions options)
	{
		CreateWebhookParams createWebhookParams = new CreateWebhookParams
		{
			Name = name
		};
		if (avatar != null)
		{
			createWebhookParams.Avatar = new Discord.API.Image(avatar);
		}
		return RestWebhook.Create(client, channel, await client.ApiClient.CreateWebhookAsync(channel.Id, createWebhookParams, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestWebhook> GetWebhookAsync(IIntegrationChannel channel, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Webhook webhook = await client.ApiClient.GetWebhookAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (webhook == null)
		{
			return null;
		}
		return RestWebhook.Create(client, channel, webhook);
	}

	public static async Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(IIntegrationChannel channel, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetChannelWebhooksAsync(channel.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Webhook x) => RestWebhook.Create(client, channel, x)).ToImmutableArray();
	}

	public static async Task<ulong> FollowAnnouncementChannelAsync(INewsChannel newsChannel, ulong channelId, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.FollowChannelAsync(newsChannel.Id, channelId, options)).WebhookId;
	}

	public static async Task<ICategoryChannel> GetCategoryAsync(INestedChannel channel, BaseDiscordClient client, RequestOptions options)
	{
		if (!channel.CategoryId.HasValue)
		{
			return null;
		}
		return RestChannel.Create(client, await client.ApiClient.GetChannelAsync(channel.CategoryId.Value, options).ConfigureAwait(continueOnCapturedContext: false)) as ICategoryChannel;
	}

	public static async Task SyncPermissionsAsync(INestedChannel channel, BaseDiscordClient client, RequestOptions options)
	{
		ICategoryChannel categoryChannel = await GetCategoryAsync(channel, client, options).ConfigureAwait(continueOnCapturedContext: false);
		if (categoryChannel == null)
		{
			throw new InvalidOperationException("This channel does not have a parent channel.");
		}
		ModifyGuildChannelParams args = new ModifyGuildChannelParams
		{
			Overwrites = categoryChannel.PermissionOverwrites.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()
		};
		await client.ApiClient.ModifyGuildChannelAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task ModifyVoiceChannelStatusAsync(IVoiceChannel channel, string status, BaseDiscordClient client, RequestOptions options)
	{
		Preconditions.AtMost(status.Length, 500, $"Voice channel status length must be less than {500}.");
		await client.ApiClient.ModifyVoiceChannelStatusAsync(channel.Id, status, options).ConfigureAwait(continueOnCapturedContext: false);
	}
}
