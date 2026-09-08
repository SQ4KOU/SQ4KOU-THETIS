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

internal static class ClientHelper
{
	public static async Task<RestApplication> GetApplicationInfoAsync(BaseDiscordClient client, RequestOptions options)
	{
		return RestApplication.Create(client, await client.ApiClient.GetMyApplicationAsync(options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestApplication> GetCurrentBotApplicationAsync(BaseDiscordClient client, RequestOptions options)
	{
		return RestApplication.Create(client, await client.ApiClient.GetCurrentBotApplicationAsync(options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static Task<Application> ModifyCurrentBotApplicationAsync(BaseDiscordClient client, Action<ModifyApplicationProperties> func, RequestOptions options)
	{
		ModifyApplicationProperties modifyApplicationProperties = new ModifyApplicationProperties();
		func(modifyApplicationProperties);
		if (modifyApplicationProperties.Tags.IsSpecified)
		{
			Preconditions.AtMost(modifyApplicationProperties.Tags.Value.Length, 5, "Tags", $"An application can have a maximum of {5} applied.");
			string[] value = modifyApplicationProperties.Tags.Value;
			for (int i = 0; i < value.Length; i++)
			{
				Preconditions.AtMost(value[i].Length, 20, "Tags", $"An application tag must have length less or equal to {20}");
			}
		}
		if (modifyApplicationProperties.Description.IsSpecified)
		{
			Preconditions.AtMost(modifyApplicationProperties.Description.Value.Length, 400, "Description", $"An application description tag mus have length less or equal to {400}");
		}
		return client.ApiClient.ModifyCurrentBotApplicationAsync(new ModifyCurrentApplicationBotParams
		{
			Description = modifyApplicationProperties.Description,
			Tags = modifyApplicationProperties.Tags,
			Icon = (modifyApplicationProperties.Icon.IsSpecified ? ((Optional<Discord.API.Image?>)(modifyApplicationProperties.Icon.Value?.ToModel())) : Optional<Discord.API.Image?>.Unspecified),
			InteractionsEndpointUrl = modifyApplicationProperties.InteractionsEndpointUrl,
			RoleConnectionsEndpointUrl = modifyApplicationProperties.RoleConnectionsEndpointUrl,
			Flags = modifyApplicationProperties.Flags,
			CoverImage = (modifyApplicationProperties.CoverImage.IsSpecified ? ((Optional<Discord.API.Image?>)(modifyApplicationProperties.CoverImage.Value?.ToModel())) : Optional<Discord.API.Image?>.Unspecified),
			CustomInstallUrl = modifyApplicationProperties.CustomInstallUrl,
			InstallParams = (modifyApplicationProperties.InstallParams.IsSpecified ? ((Optional<InstallParams>)((modifyApplicationProperties.InstallParams.Value == null) ? null : new InstallParams
			{
				Permission = modifyApplicationProperties.InstallParams.Value.Permission,
				Scopes = modifyApplicationProperties.InstallParams.Value.Scopes.ToArray()
			})) : Optional<InstallParams>.Unspecified),
			IntegrationTypesConfig = (modifyApplicationProperties.IntegrationTypesConfig.IsSpecified ? ((Optional<Dictionary<ApplicationIntegrationType, InstallParams>>)(modifyApplicationProperties.IntegrationTypesConfig.Value?.ToDictionary((KeyValuePair<ApplicationIntegrationType, ApplicationInstallParams> x) => x.Key, (KeyValuePair<ApplicationIntegrationType, ApplicationInstallParams> x) => new InstallParams
			{
				Permission = x.Value.Permission,
				Scopes = x.Value.Scopes.ToArray()
			}))) : Optional<Dictionary<ApplicationIntegrationType, InstallParams>>.Unspecified)
		}, options);
	}

	public static async Task<RestChannel> GetChannelAsync(BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Channel channel = await client.ApiClient.GetChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (channel != null)
		{
			return RestChannel.Create(client, channel);
		}
		return null;
	}

	public static async Task<IReadOnlyCollection<IRestPrivateChannel>> GetPrivateChannelsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetMyPrivateChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((Channel x) => RestChannel.CreatePrivate(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestDMChannel>> GetDMChannelsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (from x in await client.ApiClient.GetMyPrivateChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false)
			where x.Type == ChannelType.DM
			select RestDMChannel.Create(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestGroupChannel>> GetGroupChannelsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (from x in await client.ApiClient.GetMyPrivateChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false)
			where x.Type == ChannelType.Group
			select RestGroupChannel.Create(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestConnection>> GetConnectionsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetMyConnectionsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((Connection model) => RestConnection.Create(client, model)).ToImmutableArray();
	}

	public static async Task<RestInviteMetadata> GetInviteAsync(BaseDiscordClient client, string inviteId, RequestOptions options, ulong? scheduledEventId = null)
	{
		InviteMetadata inviteMetadata = await client.ApiClient.GetInviteAsync(inviteId, options, scheduledEventId).ConfigureAwait(continueOnCapturedContext: false);
		if (inviteMetadata != null)
		{
			return RestInviteMetadata.Create(client, null, null, inviteMetadata);
		}
		return null;
	}

	public static async Task<RestGuild> GetGuildAsync(BaseDiscordClient client, ulong id, bool withCounts, RequestOptions options)
	{
		Guild guild = await client.ApiClient.GetGuildAsync(id, withCounts, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guild != null)
		{
			return RestGuild.Create(client, guild);
		}
		return null;
	}

	public static async Task<RestGuildWidget?> GetGuildWidgetAsync(BaseDiscordClient client, ulong id, RequestOptions options)
	{
		GuildWidget guildWidget = await client.ApiClient.GetGuildWidgetAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildWidget != null)
		{
			return RestGuildWidget.Create(guildWidget);
		}
		return null;
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestUserGuild>> GetGuildSummariesAsync(BaseDiscordClient client, ulong? fromGuildId, int? limit, RequestOptions options)
	{
		return new PagedAsyncEnumerable<RestUserGuild>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetGuildSummariesParams getGuildSummariesParams = new GetGuildSummariesParams
			{
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getGuildSummariesParams.AfterGuildId = info.Position.Value;
			}
			return (await client.ApiClient.GetMyGuildsAsync(getGuildSummariesParams, options).ConfigureAwait(continueOnCapturedContext: false)).Select((UserGuild x) => RestUserGuild.Create(client, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestUserGuild> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Max((RestUserGuild x) => x.Id);
			return true;
		}, fromGuildId, limit);
	}

	public static async Task<IReadOnlyCollection<RestGuild>> GetGuildsAsync(BaseDiscordClient client, bool withCounts, RequestOptions options)
	{
		IEnumerable<RestUserGuild> enumerable = await GetGuildSummariesAsync(client, null, null, options).FlattenAsync().ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<RestGuild>.Builder guilds = ImmutableArray.CreateBuilder<RestGuild>();
		foreach (RestUserGuild item in enumerable)
		{
			Guild guild = await client.ApiClient.GetGuildAsync(item.Id, withCounts).ConfigureAwait(continueOnCapturedContext: false);
			if (guild != null)
			{
				guilds.Add(RestGuild.Create(client, guild));
			}
		}
		return guilds.ToImmutable();
	}

	public static async Task<RestGuild> CreateGuildAsync(BaseDiscordClient client, string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
	{
		CreateGuildParams createGuildParams = new CreateGuildParams(name, region.Id);
		if (jpegIcon != null)
		{
			createGuildParams.Icon = new Discord.API.Image(jpegIcon);
		}
		return RestGuild.Create(client, await client.ApiClient.CreateGuildAsync(createGuildParams, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestUser> GetUserAsync(BaseDiscordClient client, ulong id, RequestOptions options)
	{
		User user = await client.ApiClient.GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (user != null)
		{
			return RestUser.Create(client, user);
		}
		return null;
	}

	public static async Task<RestGuildUser> GetGuildUserAsync(BaseDiscordClient client, ulong guildId, ulong id, RequestOptions options)
	{
		RestGuild guild = await GetGuildAsync(client, guildId, withCounts: false, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guild == null)
		{
			return null;
		}
		GuildMember guildMember = await client.ApiClient.GetGuildMemberAsync(guildId, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildMember != null)
		{
			return RestGuildUser.Create(client, guild, guildMember);
		}
		return null;
	}

	public static async Task<RestWebhook> GetWebhookAsync(BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Webhook webhook = await client.ApiClient.GetWebhookAsync(id).ConfigureAwait(continueOnCapturedContext: false);
		if (webhook != null)
		{
			return RestWebhook.Create(client, (IGuild)null, webhook);
		}
		return null;
	}

	public static async Task<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((VoiceRegion x) => RestVoiceRegion.Create(client, x)).ToImmutableArray();
	}

	public static async Task<RestVoiceRegion> GetVoiceRegionAsync(BaseDiscordClient client, string id, RequestOptions options)
	{
		return (await client.ApiClient.GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((VoiceRegion x) => RestVoiceRegion.Create(client, x)).FirstOrDefault((RestVoiceRegion x) => x.Id == id);
	}

	public static async Task<int> GetRecommendShardCountAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetBotGatewayAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Shards;
	}

	public static async Task<BotGateway> GetBotGatewayAsync(BaseDiscordClient client, RequestOptions options)
	{
		GetBotGatewayResponse getBotGatewayResponse = await client.ApiClient.GetBotGatewayAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		return new BotGateway
		{
			Url = getBotGatewayResponse.Url,
			Shards = getBotGatewayResponse.Shards,
			SessionStartLimit = new SessionStartLimit
			{
				Total = getBotGatewayResponse.SessionStartLimit.Total,
				Remaining = getBotGatewayResponse.SessionStartLimit.Remaining,
				ResetAfter = getBotGatewayResponse.SessionStartLimit.ResetAfter,
				MaxConcurrency = getBotGatewayResponse.SessionStartLimit.MaxConcurrency
			}
		};
	}

	public static async Task<IReadOnlyCollection<RestGlobalCommand>> GetGlobalApplicationCommandsAsync(BaseDiscordClient client, bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		ApplicationCommand[] source = await client.ApiClient.GetGlobalApplicationCommandsAsync(withLocalizations, locale, options).ConfigureAwait(continueOnCapturedContext: false);
		if (!source.Any())
		{
			return Array.Empty<RestGlobalCommand>();
		}
		return source.Select((ApplicationCommand x) => RestGlobalCommand.Create(client, x)).ToArray();
	}

	public static async Task<RestGlobalCommand> GetGlobalApplicationCommandAsync(BaseDiscordClient client, ulong id, RequestOptions options = null)
	{
		ApplicationCommand applicationCommand = await client.ApiClient.GetGlobalApplicationCommandAsync(id, options);
		return (applicationCommand != null) ? RestGlobalCommand.Create(client, applicationCommand) : null;
	}

	public static async Task<IReadOnlyCollection<RestGuildCommand>> GetGuildApplicationCommandsAsync(BaseDiscordClient client, ulong guildId, bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		ApplicationCommand[] source = await client.ApiClient.GetGuildApplicationCommandsAsync(guildId, withLocalizations, locale, options).ConfigureAwait(continueOnCapturedContext: false);
		if (!source.Any())
		{
			return ImmutableArray.Create<RestGuildCommand>();
		}
		return source.Select((ApplicationCommand x) => RestGuildCommand.Create(client, x, guildId)).ToImmutableArray();
	}

	public static async Task<RestGuildCommand> GetGuildApplicationCommandAsync(BaseDiscordClient client, ulong id, ulong guildId, RequestOptions options = null)
	{
		ApplicationCommand applicationCommand = await client.ApiClient.GetGuildApplicationCommandAsync(guildId, id, options);
		return (applicationCommand != null) ? RestGuildCommand.Create(client, applicationCommand, guildId) : null;
	}

	public static async Task<RestGuildCommand> CreateGuildApplicationCommandAsync(BaseDiscordClient client, ulong guildId, ApplicationCommandProperties properties, RequestOptions options = null)
	{
		return RestGuildCommand.Create(client, await InteractionHelper.CreateGuildCommandAsync(client, guildId, properties, options), guildId);
	}

	public static async Task<RestGlobalCommand> CreateGlobalApplicationCommandAsync(BaseDiscordClient client, ApplicationCommandProperties properties, RequestOptions options = null)
	{
		return RestGlobalCommand.Create(client, await InteractionHelper.CreateGlobalCommandAsync(client, properties, options));
	}

	public static async Task<IReadOnlyCollection<RestGlobalCommand>> BulkOverwriteGlobalApplicationCommandAsync(BaseDiscordClient client, ApplicationCommandProperties[] properties, RequestOptions options = null)
	{
		return (await InteractionHelper.BulkOverwriteGlobalCommandsAsync(client, properties, options)).Select((ApplicationCommand x) => RestGlobalCommand.Create(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestGuildCommand>> BulkOverwriteGuildApplicationCommandAsync(BaseDiscordClient client, ulong guildId, ApplicationCommandProperties[] properties, RequestOptions options = null)
	{
		return (await InteractionHelper.BulkOverwriteGuildCommandsAsync(client, guildId, properties, options)).Select((ApplicationCommand x) => RestGuildCommand.Create(client, x, guildId)).ToImmutableArray();
	}

	public static Task AddRoleAsync(BaseDiscordClient client, ulong guildId, ulong userId, ulong roleId, RequestOptions options = null)
	{
		return client.ApiClient.AddRoleAsync(guildId, userId, roleId, options);
	}

	public static Task RemoveRoleAsync(BaseDiscordClient client, ulong guildId, ulong userId, ulong roleId, RequestOptions options = null)
	{
		return client.ApiClient.RemoveRoleAsync(guildId, userId, roleId, options);
	}

	public static async Task<IReadOnlyCollection<RoleConnectionMetadata>> GetRoleConnectionMetadataRecordsAsync(BaseDiscordClient client, RequestOptions options = null)
	{
		return (await client.ApiClient.GetApplicationRoleConnectionMetadataRecordsAsync(options)).Select((Discord.API.RoleConnectionMetadata model) => new RoleConnectionMetadata(model.Type, model.Key, model.Name, model.Description, (!model.NameLocalizations.IsSpecified) ? null : model.NameLocalizations.Value?.ToImmutableDictionary(), (!model.DescriptionLocalizations.IsSpecified) ? null : model.DescriptionLocalizations.Value?.ToImmutableDictionary())).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RoleConnectionMetadata>> ModifyRoleConnectionMetadataRecordsAsync(ICollection<RoleConnectionMetadataProperties> metadata, BaseDiscordClient client, RequestOptions options = null)
	{
		return (await client.ApiClient.UpdateApplicationRoleConnectionMetadataRecordsAsync(metadata.Select((RoleConnectionMetadataProperties x) => new Discord.API.RoleConnectionMetadata
		{
			Name = x.Name,
			Description = x.Description,
			Key = x.Key,
			Type = x.Type,
			NameLocalizations = x.NameLocalizations?.ToDictionary((KeyValuePair<string, string> keyValuePair) => keyValuePair.Key, (KeyValuePair<string, string> y) => y.Value),
			DescriptionLocalizations = x.DescriptionLocalizations?.ToDictionary((KeyValuePair<string, string> keyValuePair) => keyValuePair.Key, (KeyValuePair<string, string> y) => y.Value)
		}).ToArray())).Select((Discord.API.RoleConnectionMetadata model) => new RoleConnectionMetadata(model.Type, model.Key, model.Name, model.Description, (!model.NameLocalizations.IsSpecified) ? null : model.NameLocalizations.Value?.ToImmutableDictionary(), (!model.DescriptionLocalizations.IsSpecified) ? null : model.DescriptionLocalizations.Value?.ToImmutableDictionary())).ToImmutableArray();
	}

	public static async Task<RoleConnection> GetUserRoleConnectionAsync(ulong applicationId, BaseDiscordClient client, RequestOptions options = null)
	{
		Discord.API.RoleConnection roleConnection = await client.ApiClient.GetUserApplicationRoleConnectionAsync(applicationId, options);
		return new RoleConnection(roleConnection.PlatformName.GetValueOrDefault(null), roleConnection.PlatformUsername.GetValueOrDefault(null), roleConnection.Metadata.GetValueOrDefault());
	}

	public static async Task<RoleConnection> ModifyUserRoleConnectionAsync(ulong applicationId, RoleConnectionProperties roleConnection, BaseDiscordClient client, RequestOptions options = null)
	{
		Discord.API.RoleConnection roleConnection2 = await client.ApiClient.ModifyUserApplicationRoleConnectionAsync(applicationId, new Discord.API.RoleConnection
		{
			PlatformName = roleConnection.PlatformName,
			PlatformUsername = roleConnection.PlatformUsername,
			Metadata = roleConnection.Metadata
		}, options);
		return new RoleConnection(roleConnection2.PlatformName.GetValueOrDefault(null), roleConnection2.PlatformUsername.GetValueOrDefault(null), roleConnection2.Metadata.GetValueOrDefault()?.ToImmutableDictionary());
	}

	public static async Task<RestEntitlement> CreateTestEntitlementAsync(BaseDiscordClient client, ulong skuId, ulong ownerId, SubscriptionOwnerType ownerType, RequestOptions options = null)
	{
		return RestEntitlement.Create(client, await client.ApiClient.CreateEntitlementAsync(new CreateEntitlementParams
		{
			Type = ownerType,
			OwnerId = ownerId,
			SkuId = skuId
		}, options));
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestEntitlement>> ListEntitlementsAsync(BaseDiscordClient client, int? limit = 100, ulong? afterId = null, ulong? beforeId = null, bool excludeEnded = false, ulong? guildId = null, ulong? userId = null, ulong[] skuIds = null, bool? excludeDeleted = null, RequestOptions options = null)
	{
		return new PagedAsyncEnumerable<RestEntitlement>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			ListEntitlementsParams obj = new ListEntitlementsParams
			{
				Limit = info.PageSize,
				BeforeId = (((Optional<ulong>?)beforeId) ?? Optional<ulong>.Unspecified),
				ExcludeEnded = excludeEnded,
				GuildId = (((Optional<ulong>?)guildId) ?? Optional<ulong>.Unspecified),
				UserId = (((Optional<ulong>?)userId) ?? Optional<ulong>.Unspecified)
			};
			ulong[] array = skuIds;
			obj.SkuIds = ((array != null) ? ((Optional<ulong[]>)array) : Optional<ulong[]>.Unspecified);
			obj.ExcludeDeleted = ((Optional<bool>?)excludeDeleted) ?? Optional<bool>.Unspecified;
			ListEntitlementsParams listEntitlementsParams = obj;
			if (info.Position.HasValue)
			{
				listEntitlementsParams.AfterId = info.Position.Value;
			}
			return (await client.ApiClient.ListEntitlementAsync(listEntitlementsParams, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Entitlement x) => RestEntitlement.Create(client, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestEntitlement> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Max((RestEntitlement x) => x.Id);
			return true;
		}, afterId, limit);
	}

	public static async Task<IReadOnlyCollection<SKU>> ListSKUsAsync(BaseDiscordClient client, RequestOptions options = null)
	{
		return (await client.ApiClient.ListSKUsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((Discord.API.SKU x) => new SKU(x.Id, x.Type, x.ApplicationId, x.Name, x.Slug, x.Flags)).ToImmutableArray();
	}

	public static Task ConsumeEntitlementAsync(BaseDiscordClient client, ulong entitlementId, RequestOptions options = null)
	{
		return client.ApiClient.ConsumeEntitlementAsync(entitlementId, options);
	}

	public static async Task<RestSubscription> GetSKUSubscriptionAsync(BaseDiscordClient client, ulong skuId, ulong subscriptionId, RequestOptions options = null)
	{
		return RestSubscription.Create(client, await client.ApiClient.GetSKUSubscriptionAsync(skuId, subscriptionId, options));
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestSubscription>> ListSubscriptionsAsync(BaseDiscordClient client, ulong skuId, int limit = 100, ulong? afterId = null, ulong? beforeId = null, ulong? userId = null, RequestOptions options = null)
	{
		return new PagedAsyncEnumerable<RestSubscription>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			ulong? after = afterId;
			if (info.Position.HasValue)
			{
				after = info.Position.Value;
			}
			return (await client.ApiClient.ListSKUSubscriptionsAsync(skuId, beforeId, after, limit, userId, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Subscription x) => RestSubscription.Create(client, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestSubscription> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Max((RestSubscription x) => x.Id);
			return true;
		}, afterId, limit);
	}

	public static async Task<IReadOnlyCollection<Emote>> GetApplicationEmojisAsync(BaseDiscordClient client, RequestOptions options = null)
	{
		return (await client.ApiClient.GetApplicationEmotesAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Items.Select((Discord.API.Emoji x) => x.ToEmote(client)).ToImmutableArray();
	}

	public static async Task<Emote> GetApplicationEmojiAsync(BaseDiscordClient client, ulong emojiId, RequestOptions options = null)
	{
		return (await client.ApiClient.GetApplicationEmoteAsync(emojiId, options).ConfigureAwait(continueOnCapturedContext: false)).ToEmote(client);
	}

	public static async Task<Emote> CreateApplicationEmojiAsync(BaseDiscordClient client, string name, Image image, RequestOptions options = null)
	{
		return (await client.ApiClient.CreateApplicationEmoteAsync(new CreateApplicationEmoteParams
		{
			Name = name,
			Image = image.ToModel()
		}, options).ConfigureAwait(continueOnCapturedContext: false)).ToEmote(client);
	}

	public static async Task<Emote> ModifyApplicationEmojiAsync(BaseDiscordClient client, ulong emojiId, Action<ApplicationEmoteProperties> func, RequestOptions options = null)
	{
		ApplicationEmoteProperties applicationEmoteProperties = new ApplicationEmoteProperties();
		func(applicationEmoteProperties);
		return (await client.ApiClient.ModifyApplicationEmoteAsync(emojiId, new ModifyApplicationEmoteParams
		{
			Name = applicationEmoteProperties.Name
		}, options).ConfigureAwait(continueOnCapturedContext: false)).ToEmote(client);
	}

	public static Task DeleteApplicationEmojiAsync(BaseDiscordClient client, ulong emojiId, RequestOptions options = null)
	{
		return client.ApiClient.DeleteApplicationEmoteAsync(emojiId, options);
	}
}
