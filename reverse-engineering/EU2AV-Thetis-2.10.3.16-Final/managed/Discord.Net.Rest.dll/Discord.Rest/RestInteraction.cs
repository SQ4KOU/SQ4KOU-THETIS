using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.API;
using Discord.Net;
using Newtonsoft.Json;

namespace Discord.Rest;

public abstract class RestInteraction : RestEntity<ulong>, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	private Func<RequestOptions, ulong, Task<IRestMessageChannel>> _getChannel;

	private Func<RequestOptions, ulong, Task<RestGuild>> _getGuild;

	public InteractionType Type { get; private set; }

	public IDiscordInteractionData Data { get; private set; }

	public string Token { get; private set; }

	public int Version { get; private set; }

	public RestUser User { get; private set; }

	public string UserLocale { get; private set; }

	public string GuildLocale { get; private set; }

	public DateTimeOffset CreatedAt { get; private set; }

	public bool IsValidToken => InteractionHelper.CanRespondOrFollowup(this);

	public IRestMessageChannel Channel { get; private set; }

	public ulong? ChannelId { get; private set; }

	public RestGuild Guild { get; private set; }

	public ulong? GuildId { get; private set; }

	public bool HasResponded { get; protected set; }

	public bool IsDMInteraction { get; private set; }

	public ulong ApplicationId { get; private set; }

	public InteractionContextType? ContextType { get; private set; }

	public GuildPermissions Permissions { get; private set; }

	public IReadOnlyCollection<RestEntitlement> Entitlements { get; private set; }

	public IReadOnlyDictionary<ApplicationIntegrationType, ulong> IntegrationOwners { get; private set; }

	public ulong AttachmentSizeLimit { get; private set; }

	IReadOnlyCollection<IEntitlement> IDiscordInteraction.Entitlements => Entitlements;

	IUser IDiscordInteraction.User => User;

	internal RestInteraction(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
		CreatedAt = (discord.UseInteractionSnowflakeDate ? SnowflakeUtils.FromSnowflake(base.Id) : ((DateTimeOffset)DateTime.UtcNow));
	}

	internal static async Task<RestInteraction> CreateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		if (model.Type == InteractionType.Ping)
		{
			return await RestPingInteraction.CreateAsync(client, model, doApiCall);
		}
		if (model.Type == InteractionType.ApplicationCommand)
		{
			ApplicationCommandInteractionData applicationCommandInteractionData = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
			if (applicationCommandInteractionData == null)
			{
				return null;
			}
			return applicationCommandInteractionData.Type switch
			{
				ApplicationCommandType.Slash => await RestSlashCommand.CreateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false), 
				ApplicationCommandType.Message => await RestMessageCommand.CreateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false), 
				ApplicationCommandType.User => await RestUserCommand.CreateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false), 
				_ => null, 
			};
		}
		if (model.Type == InteractionType.MessageComponent)
		{
			return await RestMessageComponent.CreateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (model.Type == InteractionType.ApplicationCommandAutocomplete)
		{
			return await RestAutocompleteInteraction.CreateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (model.Type == InteractionType.ModalSubmit)
		{
			return await RestModal.CreateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		}
		return null;
	}

	internal virtual async Task UpdateAsync(DiscordRestClient discord, Interaction model, bool doApiCall)
	{
		ChannelId = (model.Channel.IsSpecified ? new ulong?(model.Channel.Value.Id) : ((ulong?)null));
		GuildId = (model.GuildId.IsSpecified ? new ulong?(model.GuildId.Value) : ((ulong?)null));
		IsDMInteraction = !GuildId.HasValue;
		Data = (model.Data.IsSpecified ? model.Data.Value : null);
		Token = model.Token;
		Version = model.Version;
		Type = model.Type;
		ApplicationId = model.ApplicationId;
		if (Guild == null && GuildId.HasValue)
		{
			if (doApiCall)
			{
				Guild = await discord.GetGuildAsync(GuildId.Value);
			}
			else
			{
				Guild = null;
				_getGuild = async (RequestOptions opt, ulong ul) => await discord.GetGuildAsync(ul, opt);
			}
		}
		if (User == null)
		{
			if (model.Member.IsSpecified && GuildId.HasValue)
			{
				User = RestGuildUser.Create(base.Discord, Guild, model.Member.Value, GuildId);
			}
			else
			{
				User = RestUser.Create(base.Discord, model.User.Value);
			}
		}
		if (Channel == null && ChannelId.HasValue)
		{
			try
			{
				if (doApiCall)
				{
					Channel = (IRestMessageChannel)(await discord.GetChannelAsync(ChannelId.Value));
				}
				else
				{
					if (model.Channel.IsSpecified)
					{
						IRestMessageChannel channel;
						switch (model.Channel.Value.Type)
						{
						case ChannelType.Text:
						case ChannelType.Voice:
						case ChannelType.News:
						case ChannelType.NewsThread:
						case ChannelType.PublicThread:
						case ChannelType.PrivateThread:
						case ChannelType.Stage:
						case ChannelType.Forum:
						case ChannelType.Media:
							channel = RestGuildChannel.Create(discord, Guild, model.Channel.Value) as IRestMessageChannel;
							break;
						case ChannelType.DM:
							channel = RestDMChannel.Create(discord, model.Channel.Value);
							break;
						case ChannelType.Group:
							channel = RestGroupChannel.Create(discord, model.Channel.Value);
							break;
						default:
							channel = null;
							break;
						}
						Channel = channel;
					}
					_getChannel = async (RequestOptions opt, ulong ul) => (Guild == null) ? ((IRestMessageChannel)(await discord.GetChannelAsync(ul, opt))) : ((IRestMessageChannel)(await Guild.GetChannelAsync(ul, opt)));
				}
			}
			catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingPermissions)
			{
			}
		}
		UserLocale = (model.UserLocale.IsSpecified ? model.UserLocale.Value : null);
		GuildLocale = (model.GuildLocale.IsSpecified ? model.GuildLocale.Value : null);
		Entitlements = model.Entitlements.Select((Entitlement x) => RestEntitlement.Create(discord, x)).ToImmutableArray();
		IntegrationOwners = model.IntegrationOwners;
		ContextType = (model.ContextType.IsSpecified ? new InteractionContextType?(model.ContextType.Value) : ((InteractionContextType?)null));
		Permissions = new GuildPermissions((ulong)model.ApplicationPermissions);
		AttachmentSizeLimit = model.AttachmentSizeLimit;
	}

	internal string SerializePayload(object payload)
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (StringWriter textWriter = new StringWriter(stringBuilder))
		{
			using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
			DiscordRestClient.Serializer.Serialize(jsonWriter, payload);
		}
		return stringBuilder.ToString();
	}

	public async Task<IRestMessageChannel> GetChannelAsync(RequestOptions options = null)
	{
		if (Channel != null)
		{
			return Channel;
		}
		if (IsDMInteraction)
		{
			Channel = await User.CreateDMChannelAsync(options);
		}
		else if (ChannelId.HasValue)
		{
			Channel = (await _getChannel(options, ChannelId.Value)) ?? throw new InvalidOperationException("The interaction channel was not able to be retrieved.");
			_getChannel = null;
		}
		return Channel;
	}

	public async Task<RestGuild> GetGuildAsync(RequestOptions options)
	{
		if (!GuildId.HasValue)
		{
			return null;
		}
		if (Guild == null)
		{
			Guild = await _getGuild(options, GuildId.Value);
		}
		_getGuild = null;
		return Guild;
	}

	public abstract string Defer(bool ephemeral = false, RequestOptions options = null);

	public Task<RestInteractionMessage> GetOriginalResponseAsync(RequestOptions options = null)
	{
		return InteractionHelper.GetOriginalResponseAsync(base.Discord, Channel, this, options);
	}

	public async Task<RestInteractionMessage> ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		Message model = await InteractionHelper.ModifyInteractionResponseAsync(base.Discord, Token, func, options);
		return RestInteractionMessage.Create(base.Discord, model, Token, Channel);
	}

	public abstract string RespondWithModal(Modal modal, RequestOptions options = null);

	public abstract string Respond(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None);

	public abstract Task<RestFollowupMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None);

	public abstract Task<RestFollowupMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None);

	public abstract Task<RestFollowupMessage> FollowupWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None);

	public abstract Task<RestFollowupMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None);

	public abstract Task<RestFollowupMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null, PollProperties poll = null, MessageFlags flags = MessageFlags.None);

	public Task DeleteOriginalResponseAsync(RequestOptions options = null)
	{
		return InteractionHelper.DeleteInteractionResponseAsync(base.Discord, this, options);
	}

	public Task RespondWithPremiumRequiredAsync(RequestOptions options = null)
	{
		return InteractionHelper.RespondWithPremiumRequiredAsync(base.Discord, base.Id, Token, options);
	}

	Task IDiscordInteraction.RespondAsync(string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		return Task.FromResult(Respond(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll));
	}

	Task IDiscordInteraction.DeferAsync(bool ephemeral, RequestOptions options)
	{
		return Task.FromResult(Defer(ephemeral, options));
	}

	Task IDiscordInteraction.RespondWithModalAsync(Modal modal, RequestOptions options)
	{
		return Task.FromResult(RespondWithModal(modal, options));
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupAsync(string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		return await FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.GetOriginalResponseAsync(RequestOptions options)
	{
		return await GetOriginalResponseAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions options)
	{
		return await ModifyOriginalResponseAsync(func, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupWithFileAsync(Stream fileStream, string fileName, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		return await FollowupWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupWithFileAsync(string filePath, string fileName, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		return await FollowupWithFileAsync(filePath, text, fileName, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupWithFileAsync(FileAttachment attachment, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		return await FollowupWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task<IUserMessage> IDiscordInteraction.FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		return await FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options, poll, flags).ConfigureAwait(continueOnCapturedContext: false);
	}

	Task IDiscordInteraction.RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		throw new NotSupportedException("REST-Based interactions don't support files.");
	}

	Task IDiscordInteraction.RespondWithFileAsync(Stream fileStream, string fileName, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		throw new NotSupportedException("REST-Based interactions don't support files.");
	}

	Task IDiscordInteraction.RespondWithFileAsync(string filePath, string fileName, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		throw new NotSupportedException("REST-Based interactions don't support files.");
	}

	Task IDiscordInteraction.RespondWithFileAsync(FileAttachment attachment, string text, Embed[] embeds, bool isTTS, bool ephemeral, AllowedMentions allowedMentions, MessageComponent components, Embed embed, RequestOptions options, PollProperties poll, MessageFlags flags)
	{
		throw new NotSupportedException("REST-Based interactions don't support files.");
	}
}
