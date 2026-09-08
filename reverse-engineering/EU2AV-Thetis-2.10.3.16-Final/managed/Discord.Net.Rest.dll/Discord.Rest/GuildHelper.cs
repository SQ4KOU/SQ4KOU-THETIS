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

internal static class GuildHelper
{
	public static Task<Guild> ModifyAsync(IGuild guild, BaseDiscordClient client, Action<GuildProperties> func, RequestOptions options)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		GuildProperties guildProperties = new GuildProperties();
		func(guildProperties);
		ModifyGuildParams modifyGuildParams = new ModifyGuildParams
		{
			AfkChannelId = guildProperties.AfkChannelId,
			AfkTimeout = guildProperties.AfkTimeout,
			SystemChannelId = guildProperties.SystemChannelId,
			DefaultMessageNotifications = guildProperties.DefaultMessageNotifications,
			Icon = (guildProperties.Icon.IsSpecified ? ((Optional<Discord.API.Image?>)(guildProperties.Icon.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			Name = guildProperties.Name,
			Splash = (guildProperties.Splash.IsSpecified ? ((Optional<Discord.API.Image?>)(guildProperties.Splash.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			Banner = (guildProperties.Banner.IsSpecified ? ((Optional<Discord.API.Image?>)(guildProperties.Banner.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			VerificationLevel = guildProperties.VerificationLevel,
			ExplicitContentFilter = guildProperties.ExplicitContentFilter,
			SystemChannelFlags = guildProperties.SystemChannelFlags,
			IsBoostProgressBarEnabled = guildProperties.IsBoostProgressBarEnabled,
			GuildFeatures = (guildProperties.Features.IsSpecified ? ((Optional<GuildFeatures>)new GuildFeatures(guildProperties.Features.Value, Array.Empty<string>())) : Optional.Create<GuildFeatures>())
		};
		if (modifyGuildParams.Banner.IsSpecified)
		{
			guild.Features.EnsureFeature(GuildFeature.Banner);
		}
		if (modifyGuildParams.Splash.IsSpecified)
		{
			guild.Features.EnsureFeature(GuildFeature.InviteSplash);
		}
		if (guildProperties.AfkChannel.IsSpecified)
		{
			modifyGuildParams.AfkChannelId = guildProperties.AfkChannel.Value.Id;
		}
		else if (guildProperties.AfkChannelId.IsSpecified)
		{
			modifyGuildParams.AfkChannelId = guildProperties.AfkChannelId.Value;
		}
		if (guildProperties.SystemChannel.IsSpecified)
		{
			modifyGuildParams.SystemChannelId = guildProperties.SystemChannel.Value.Id;
		}
		else if (guildProperties.SystemChannelId.IsSpecified)
		{
			modifyGuildParams.SystemChannelId = guildProperties.SystemChannelId.Value;
		}
		if (guildProperties.Owner.IsSpecified)
		{
			modifyGuildParams.OwnerId = guildProperties.Owner.Value.Id;
		}
		else if (guildProperties.OwnerId.IsSpecified)
		{
			modifyGuildParams.OwnerId = guildProperties.OwnerId.Value;
		}
		if (guildProperties.Region.IsSpecified)
		{
			modifyGuildParams.RegionId = guildProperties.Region.Value.Id;
		}
		else if (guildProperties.RegionId.IsSpecified)
		{
			modifyGuildParams.RegionId = guildProperties.RegionId.Value;
		}
		if (!modifyGuildParams.Banner.IsSpecified && guild.BannerId != null)
		{
			modifyGuildParams.Banner = new Discord.API.Image(guild.BannerId);
		}
		if (!modifyGuildParams.Splash.IsSpecified && guild.SplashId != null)
		{
			modifyGuildParams.Splash = new Discord.API.Image(guild.SplashId);
		}
		if (!modifyGuildParams.Icon.IsSpecified && guild.IconId != null)
		{
			modifyGuildParams.Icon = new Discord.API.Image(guild.IconId);
		}
		if (guildProperties.ExplicitContentFilter.IsSpecified)
		{
			modifyGuildParams.ExplicitContentFilter = guildProperties.ExplicitContentFilter.Value;
		}
		if (guildProperties.SystemChannelFlags.IsSpecified)
		{
			modifyGuildParams.SystemChannelFlags = guildProperties.SystemChannelFlags.Value;
		}
		if (guildProperties.PreferredLocale.IsSpecified)
		{
			modifyGuildParams.PreferredLocale = guildProperties.PreferredLocale.Value;
		}
		else if (guildProperties.PreferredCulture.IsSpecified)
		{
			modifyGuildParams.PreferredLocale = guildProperties.PreferredCulture.Value.Name;
		}
		return client.ApiClient.ModifyGuildAsync(guild.Id, modifyGuildParams, options);
	}

	public static Task<GuildWidget> ModifyWidgetAsync(IGuild guild, BaseDiscordClient client, Action<GuildWidgetProperties> func, RequestOptions options)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		GuildWidgetProperties guildWidgetProperties = new GuildWidgetProperties();
		func(guildWidgetProperties);
		ModifyGuildWidgetParams modifyGuildWidgetParams = new ModifyGuildWidgetParams
		{
			Enabled = guildWidgetProperties.Enabled
		};
		if (guildWidgetProperties.Channel.IsSpecified)
		{
			modifyGuildWidgetParams.ChannelId = guildWidgetProperties.Channel.Value?.Id;
		}
		else if (guildWidgetProperties.ChannelId.IsSpecified)
		{
			modifyGuildWidgetParams.ChannelId = guildWidgetProperties.ChannelId.Value;
		}
		return client.ApiClient.ModifyGuildWidgetAsync(guild.Id, modifyGuildWidgetParams, options);
	}

	public static Task ReorderChannelsAsync(IGuild guild, BaseDiscordClient client, IEnumerable<ReorderChannelProperties> args, RequestOptions options)
	{
		IEnumerable<ModifyGuildChannelsParams> args2 = args.Select((ReorderChannelProperties x) => new ModifyGuildChannelsParams(x.Id, x.Position));
		return client.ApiClient.ModifyGuildChannelsAsync(guild.Id, args2, options);
	}

	public static Task<IReadOnlyCollection<Role>> ReorderRolesAsync(IGuild guild, BaseDiscordClient client, IEnumerable<ReorderRoleProperties> args, RequestOptions options)
	{
		IEnumerable<ModifyGuildRolesParams> args2 = args.Select((ReorderRoleProperties x) => new ModifyGuildRolesParams(x.Id, x.Position));
		return client.ApiClient.ModifyGuildRolesAsync(guild.Id, args2, options);
	}

	public static Task LeaveAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return client.ApiClient.LeaveGuildAsync(guild.Id, options);
	}

	public static Task DeleteAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return client.ApiClient.DeleteGuildAsync(guild.Id, options);
	}

	public static int GetMaxBitrate(PremiumTier premiumTier)
	{
		return premiumTier switch
		{
			PremiumTier.Tier1 => 128000, 
			PremiumTier.Tier2 => 256000, 
			PremiumTier.Tier3 => 384000, 
			_ => 96000, 
		};
	}

	public static ulong GetUploadLimit(PremiumTier premiumTier)
	{
		long num = premiumTier switch
		{
			PremiumTier.Tier2 => 50L, 
			PremiumTier.Tier3 => 100L, 
			_ => 25L, 
		};
		ulong num2 = 1048576uL;
		return (ulong)num * num2;
	}

	public static async Task<GuildIncidentsData> ModifyGuildIncidentActionsAsync(IGuild guild, BaseDiscordClient client, Action<GuildIncidentsDataProperties> func, RequestOptions options = null)
	{
		GuildIncidentsDataProperties guildIncidentsDataProperties = new GuildIncidentsDataProperties();
		func(guildIncidentsDataProperties);
		ModifyGuildIncidentsDataParams args = ((guildIncidentsDataProperties.DmsDisabledUntil.IsSpecified || guildIncidentsDataProperties.InvitesDisabledUntil.IsSpecified) ? new ModifyGuildIncidentsDataParams
		{
			DmsDisabledUntil = guildIncidentsDataProperties.DmsDisabledUntil,
			InvitesDisabledUntil = guildIncidentsDataProperties.InvitesDisabledUntil
		} : null);
		Discord.API.GuildIncidentsData guildIncidentsData = await client.ApiClient.ModifyGuildIncidentActionsAsync(guild.Id, args, options);
		return new GuildIncidentsData
		{
			DmsDisabledUntil = guildIncidentsData.DmsDisabledUntil,
			InvitesDisabledUntil = guildIncidentsData.InvitesDisabledUntil
		};
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestBan>> GetBansAsync(IGuild guild, BaseDiscordClient client, ulong? fromUserId, Direction dir, int limit, RequestOptions options)
	{
		if (dir == Direction.Around && limit > 1000)
		{
			int num = limit / 2;
			if (fromUserId.HasValue)
			{
				return GetBansAsync(guild, client, fromUserId.Value + 1, Direction.Before, num + 1, options).Concat(GetBansAsync(guild, client, fromUserId.Value, Direction.After, num, options));
			}
			return GetBansAsync(guild, client, null, Direction.Before, num + 1, options);
		}
		return new PagedAsyncEnumerable<RestBan>(1000, async delegate(PageInfo info, CancellationToken ct)
		{
			GetGuildBansParams getGuildBansParams = new GetGuildBansParams
			{
				RelativeDirection = dir,
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getGuildBansParams.RelativeUserId = info.Position.Value;
			}
			IReadOnlyCollection<Ban> obj = await client.ApiClient.GetGuildBansAsync(guild.Id, getGuildBansParams, options).ConfigureAwait(continueOnCapturedContext: false);
			ImmutableArray<RestBan>.Builder builder = ImmutableArray.CreateBuilder<RestBan>();
			foreach (Ban item in obj)
			{
				builder.Add(RestBan.Create(client, item));
			}
			return builder.ToImmutable();
		}, delegate(PageInfo info, IReadOnlyCollection<RestBan> lastPage)
		{
			if (lastPage.Count != 1000)
			{
				return false;
			}
			if (dir == Direction.Before)
			{
				info.Position = lastPage.Min((RestBan x) => x.User.Id);
			}
			else
			{
				info.Position = lastPage.Max((RestBan x) => x.User.Id);
			}
			return true;
		}, fromUserId, limit);
	}

	public static async Task<RestBan> GetBanAsync(IGuild guild, BaseDiscordClient client, ulong userId, RequestOptions options)
	{
		Ban ban = await client.ApiClient.GetGuildBanAsync(guild.Id, userId, options).ConfigureAwait(continueOnCapturedContext: false);
		return (ban == null) ? null : RestBan.Create(client, ban);
	}

	public static Task AddBanAsync(IGuild guild, BaseDiscordClient client, ulong userId, int pruneDays, string reason, RequestOptions options)
	{
		Preconditions.AtLeast(pruneDays, 0, "pruneDays", "Prune length must be within [0, 7]");
		return client.ApiClient.CreateGuildBanAsync(guild.Id, userId, (uint)(pruneDays * 86400), reason, options);
	}

	public static Task AddBanAsync(IGuild guild, BaseDiscordClient client, ulong userId, uint pruneSeconds, RequestOptions options)
	{
		return client.ApiClient.CreateGuildBanAsync(guild.Id, userId, pruneSeconds, null, options);
	}

	public static Task RemoveBanAsync(IGuild guild, BaseDiscordClient client, ulong userId, RequestOptions options)
	{
		return client.ApiClient.RemoveGuildBanAsync(guild.Id, userId, options);
	}

	public static async Task<BulkBanResult> BulkBanAsync(IGuild guild, BaseDiscordClient client, ulong[] userIds, int? deleteMessageSeconds, RequestOptions options)
	{
		int pos = 0;
		List<ulong> banned = new List<ulong>(userIds.Length);
		List<ulong> failed = new List<ulong>();
		while (pos * 200 < userIds.Length)
		{
			IEnumerable<ulong> source = userIds.Skip(pos * 200).Take(200);
			pos++;
			Discord.API.BulkBanResult bulkBanResult = await client.ApiClient.BulkBanAsync(guild.Id, source.ToArray(), deleteMessageSeconds, options);
			banned.AddRange(bulkBanResult.BannedUsers ?? Array.Empty<ulong>());
			failed.AddRange(bulkBanResult.FailedUsers ?? Array.Empty<ulong>());
		}
		return new BulkBanResult(banned.ToImmutableArray(), failed.ToImmutableArray());
	}

	public static async Task<RestGuildChannel> GetChannelAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Channel channel = await client.ApiClient.GetChannelAsync(guild.Id, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (channel != null)
		{
			return RestGuildChannel.Create(client, guild, channel);
		}
		return null;
	}

	public static async Task<IReadOnlyCollection<RestGuildChannel>> GetChannelsAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildChannelsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Channel x) => RestGuildChannel.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<RestTextChannel> CreateTextChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<TextChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		TextChannelProperties textChannelProperties = new TextChannelProperties();
		func?.Invoke(textChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Text)
		{
			CategoryId = textChannelProperties.CategoryId,
			Topic = textChannelProperties.Topic,
			IsNsfw = textChannelProperties.IsNsfw,
			Position = textChannelProperties.Position,
			SlowModeInterval = textChannelProperties.SlowModeInterval,
			Overwrites = (textChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)textChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			DefaultAutoArchiveDuration = textChannelProperties.AutoArchiveDuration
		};
		return RestTextChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestNewsChannel> CreateNewsChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<TextChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		TextChannelProperties textChannelProperties = new TextChannelProperties();
		func?.Invoke(textChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.News)
		{
			CategoryId = textChannelProperties.CategoryId,
			Topic = textChannelProperties.Topic,
			IsNsfw = textChannelProperties.IsNsfw,
			Position = textChannelProperties.Position,
			SlowModeInterval = textChannelProperties.SlowModeInterval,
			Overwrites = (textChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)textChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			DefaultAutoArchiveDuration = textChannelProperties.AutoArchiveDuration
		};
		return RestNewsChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestVoiceChannel> CreateVoiceChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<VoiceChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		VoiceChannelProperties voiceChannelProperties = new VoiceChannelProperties();
		func?.Invoke(voiceChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Voice)
		{
			CategoryId = voiceChannelProperties.CategoryId,
			Bitrate = voiceChannelProperties.Bitrate,
			UserLimit = voiceChannelProperties.UserLimit,
			Position = voiceChannelProperties.Position,
			Overwrites = (voiceChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)voiceChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			VideoQuality = voiceChannelProperties.VideoQualityMode,
			RtcRegion = voiceChannelProperties.RTCRegion
		};
		return RestVoiceChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestStageChannel> CreateStageChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<VoiceChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		VoiceChannelProperties voiceChannelProperties = new VoiceChannelProperties();
		func?.Invoke(voiceChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Stage)
		{
			CategoryId = voiceChannelProperties.CategoryId,
			Bitrate = voiceChannelProperties.Bitrate,
			UserLimit = voiceChannelProperties.UserLimit,
			Position = voiceChannelProperties.Position,
			Overwrites = (voiceChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)voiceChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>())
		};
		return RestStageChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestCategoryChannel> CreateCategoryChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<GuildChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		GuildChannelProperties guildChannelProperties = new GuildChannelProperties();
		func?.Invoke(guildChannelProperties);
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Category)
		{
			Position = guildChannelProperties.Position,
			Overwrites = (guildChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)guildChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>())
		};
		return RestCategoryChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestForumChannel> CreateForumChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<ForumChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		ForumChannelProperties forumChannelProperties = new ForumChannelProperties();
		func?.Invoke(forumChannelProperties);
		Preconditions.AtMost(forumChannelProperties.Tags.IsSpecified ? forumChannelProperties.Tags.Value.Count() : 0, 5, "Tags", "Forum channel can have max 20 tags.");
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Forum)
		{
			Position = forumChannelProperties.Position,
			Overwrites = (forumChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)forumChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			SlowModeInterval = forumChannelProperties.ThreadCreationInterval,
			AvailableTags = (from x in forumChannelProperties.Tags.GetValueOrDefault(Array.Empty<ForumTagProperties>())
				select new ModifyForumTagParams
				{
					Id = (((Optional<ulong>?)x.Id) ?? Optional<ulong>.Unspecified),
					Name = x.Name,
					EmojiId = ((x.Emoji is Emote emote2) ? ((Optional<ulong?>)emote2.Id) : Optional<ulong?>.Unspecified),
					EmojiName = ((x.Emoji is Emoji emoji2) ? ((Optional<string>)emoji2.Name) : Optional<string>.Unspecified),
					Moderated = x.IsModerated
				}).ToArray(),
			DefaultReactionEmoji = (forumChannelProperties.DefaultReactionEmoji.IsSpecified ? ((Optional<ModifyForumReactionEmojiParams>)new ModifyForumReactionEmojiParams
			{
				EmojiId = ((forumChannelProperties.DefaultReactionEmoji.Value is Emote emote) ? ((Optional<ulong?>)emote.Id) : Optional<ulong?>.Unspecified),
				EmojiName = ((forumChannelProperties.DefaultReactionEmoji.Value is Emoji emoji) ? ((Optional<string>)emoji.Name) : Optional<string>.Unspecified)
			}) : Optional<ModifyForumReactionEmojiParams>.Unspecified),
			ThreadRateLimitPerUser = forumChannelProperties.DefaultSlowModeInterval,
			CategoryId = forumChannelProperties.CategoryId,
			IsNsfw = forumChannelProperties.IsNsfw,
			Topic = forumChannelProperties.Topic,
			DefaultAutoArchiveDuration = forumChannelProperties.AutoArchiveDuration,
			DefaultSortOrder = forumChannelProperties.DefaultSortOrder.GetValueOrDefault(ForumSortOrder.LatestActivity),
			DefaultLayout = forumChannelProperties.DefaultLayout
		};
		return RestForumChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestMediaChannel> CreateMediaChannelAsync(IGuild guild, BaseDiscordClient client, string name, RequestOptions options, Action<ForumChannelProperties> func = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		ForumChannelProperties forumChannelProperties = new ForumChannelProperties();
		func?.Invoke(forumChannelProperties);
		Preconditions.AtMost(forumChannelProperties.Tags.IsSpecified ? forumChannelProperties.Tags.Value.Count() : 0, 20, "Tags", "Media channel can have max 20 tags.");
		CreateGuildChannelParams args = new CreateGuildChannelParams(name, ChannelType.Media)
		{
			Position = forumChannelProperties.Position,
			Overwrites = (forumChannelProperties.PermissionOverwrites.IsSpecified ? ((Optional<Discord.API.Overwrite[]>)forumChannelProperties.PermissionOverwrites.Value.Select((Overwrite overwrite) => new Discord.API.Overwrite
			{
				TargetId = overwrite.TargetId,
				TargetType = overwrite.TargetType,
				Allow = overwrite.Permissions.AllowValue.ToString(),
				Deny = overwrite.Permissions.DenyValue.ToString()
			}).ToArray()) : Optional.Create<Discord.API.Overwrite[]>()),
			SlowModeInterval = forumChannelProperties.ThreadCreationInterval,
			AvailableTags = (from x in forumChannelProperties.Tags.GetValueOrDefault(Array.Empty<ForumTagProperties>())
				select new ModifyForumTagParams
				{
					Id = (((Optional<ulong>?)x.Id) ?? Optional<ulong>.Unspecified),
					Name = x.Name,
					EmojiId = ((x.Emoji is Emote emote2) ? ((Optional<ulong?>)emote2.Id) : Optional<ulong?>.Unspecified),
					EmojiName = ((x.Emoji is Emoji emoji2) ? ((Optional<string>)emoji2.Name) : Optional<string>.Unspecified),
					Moderated = x.IsModerated
				}).ToArray(),
			DefaultReactionEmoji = (forumChannelProperties.DefaultReactionEmoji.IsSpecified ? ((Optional<ModifyForumReactionEmojiParams>)new ModifyForumReactionEmojiParams
			{
				EmojiId = ((forumChannelProperties.DefaultReactionEmoji.Value is Emote emote) ? ((Optional<ulong?>)emote.Id) : Optional<ulong?>.Unspecified),
				EmojiName = ((forumChannelProperties.DefaultReactionEmoji.Value is Emoji emoji) ? ((Optional<string>)emoji.Name) : Optional<string>.Unspecified)
			}) : Optional<ModifyForumReactionEmojiParams>.Unspecified),
			ThreadRateLimitPerUser = forumChannelProperties.DefaultSlowModeInterval,
			CategoryId = forumChannelProperties.CategoryId,
			IsNsfw = forumChannelProperties.IsNsfw,
			Topic = forumChannelProperties.Topic,
			DefaultAutoArchiveDuration = forumChannelProperties.AutoArchiveDuration,
			DefaultSortOrder = forumChannelProperties.DefaultSortOrder.GetValueOrDefault(ForumSortOrder.LatestActivity)
		};
		return RestMediaChannel.Create(client, guild, await client.ApiClient.CreateGuildChannelAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildVoiceRegionsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((VoiceRegion x) => RestVoiceRegion.Create(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestIntegration>> GetIntegrationsAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetIntegrationsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Integration x) => RestIntegration.Create(client, guild, x)).ToImmutableArray();
	}

	public static Task DeleteIntegrationAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		return client.ApiClient.DeleteIntegrationAsync(guild.Id, id, options);
	}

	public static async Task<IReadOnlyCollection<RestGuildCommand>> GetSlashCommandsAsync(IGuild guild, BaseDiscordClient client, bool withLocalizations, string locale, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildApplicationCommandsAsync(guild.Id, withLocalizations, locale, options)).Select((ApplicationCommand x) => RestGuildCommand.Create(client, x, guild.Id)).ToImmutableArray();
	}

	public static async Task<RestGuildCommand> GetSlashCommandAsync(IGuild guild, ulong id, BaseDiscordClient client, RequestOptions options)
	{
		return RestGuildCommand.Create(client, await client.ApiClient.GetGuildApplicationCommandAsync(guild.Id, id, options), guild.Id);
	}

	public static async Task<IReadOnlyCollection<RestInviteMetadata>> GetInvitesAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildInvitesAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((InviteMetadata x) => RestInviteMetadata.Create(client, guild, null, x)).ToImmutableArray();
	}

	public static async Task<RestInviteMetadata> GetVanityInviteAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		InviteVanity vanityModel = await client.ApiClient.GetVanityInviteAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (vanityModel == null)
		{
			throw new InvalidOperationException("This guild does not have a vanity URL.");
		}
		InviteMetadata inviteMetadata = await client.ApiClient.GetInviteAsync(vanityModel.Code, options).ConfigureAwait(continueOnCapturedContext: false);
		inviteMetadata.Uses = vanityModel.Uses;
		return RestInviteMetadata.Create(client, guild, null, inviteMetadata);
	}

	public static async Task<RestRole> CreateRoleAsync(IGuild guild, BaseDiscordClient client, string name, GuildPermissions? permissions, Color? color, bool isHoisted, bool isMentionable, RequestOptions options, Image? icon, Emoji emoji)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (icon.HasValue || emoji != null)
		{
			guild.Features.EnsureFeature(GuildFeature.RoleIcons);
			if (icon.HasValue && emoji != null)
			{
				throw new ArgumentException("Emoji and Icon properties cannot be present on a role at the same time.");
			}
		}
		ModifyGuildRoleParams obj = new ModifyGuildRoleParams
		{
			Color = (((Optional<uint>?)color?.RawValue) ?? Optional.Create<uint>()),
			Hoist = isHoisted,
			Mentionable = isMentionable,
			Name = name
		};
		string text = permissions?.RawValue.ToString();
		obj.Permissions = ((text != null) ? ((Optional<string>)text) : Optional.Create<string>());
		obj.Icon = icon?.ToModel();
		obj.Emoji = emoji?.Name;
		ModifyGuildRoleParams args = obj;
		return RestRole.Create(client, guild, await client.ApiClient.CreateGuildRoleAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestRole> GetRoleAsync(IGuild guild, BaseDiscordClient client, ulong roleId, RequestOptions options)
	{
		Role role = await client.ApiClient.GetRoleAsync(guild.Id, roleId, options).ConfigureAwait(continueOnCapturedContext: false);
		return (role == null) ? null : RestRole.Create(client, guild, role);
	}

	public static async Task<RestGuildUser> AddGuildUserAsync(IGuild guild, BaseDiscordClient client, ulong userId, string accessToken, Action<AddGuildUserProperties> func, RequestOptions options)
	{
		AddGuildUserProperties addGuildUserProperties = new AddGuildUserProperties();
		func?.Invoke(addGuildUserProperties);
		if (addGuildUserProperties.Roles.IsSpecified)
		{
			IEnumerable<ulong> enumerable = addGuildUserProperties.Roles.Value.Select((IRole r) => r.Id);
			if (addGuildUserProperties.RoleIds.IsSpecified)
			{
				addGuildUserProperties.RoleIds = Optional.Create(addGuildUserProperties.RoleIds.Value.Concat(enumerable));
			}
			else
			{
				addGuildUserProperties.RoleIds = Optional.Create(enumerable);
			}
		}
		AddGuildMemberParams args = new AddGuildMemberParams
		{
			AccessToken = accessToken,
			Nickname = addGuildUserProperties.Nickname,
			IsDeafened = addGuildUserProperties.Deaf,
			IsMuted = addGuildUserProperties.Mute,
			RoleIds = (addGuildUserProperties.RoleIds.IsSpecified ? ((Optional<ulong[]>)addGuildUserProperties.RoleIds.Value.Distinct().ToArray()) : Optional.Create<ulong[]>())
		};
		GuildMember guildMember = await client.ApiClient.AddGuildMemberAsync(guild.Id, userId, args, options);
		return (guildMember == null) ? null : RestGuildUser.Create(client, guild, guildMember);
	}

	public static Task AddGuildUserAsync(ulong guildId, BaseDiscordClient client, ulong userId, string accessToken, Action<AddGuildUserProperties> func, RequestOptions options)
	{
		AddGuildUserProperties addGuildUserProperties = new AddGuildUserProperties();
		func?.Invoke(addGuildUserProperties);
		if (addGuildUserProperties.Roles.IsSpecified)
		{
			IEnumerable<ulong> enumerable = addGuildUserProperties.Roles.Value.Select((IRole r) => r.Id);
			if (addGuildUserProperties.RoleIds.IsSpecified)
			{
				addGuildUserProperties.RoleIds.Value.Concat(enumerable);
			}
			else
			{
				addGuildUserProperties.RoleIds = Optional.Create(enumerable);
			}
		}
		AddGuildMemberParams args = new AddGuildMemberParams
		{
			AccessToken = accessToken,
			Nickname = addGuildUserProperties.Nickname,
			IsDeafened = addGuildUserProperties.Deaf,
			IsMuted = addGuildUserProperties.Mute,
			RoleIds = (addGuildUserProperties.RoleIds.IsSpecified ? ((Optional<ulong[]>)addGuildUserProperties.RoleIds.Value.Distinct().ToArray()) : Optional.Create<ulong[]>())
		};
		return client.ApiClient.AddGuildMemberAsync(guildId, userId, args, options);
	}

	public static async Task<RestGuildUser> GetUserAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		GuildMember guildMember = await client.ApiClient.GetGuildMemberAsync(guild.Id, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildMember != null)
		{
			return RestGuildUser.Create(client, guild, guildMember);
		}
		return null;
	}

	public static Task<RestGuildUser> GetCurrentUserAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return GetUserAsync(guild, client, client.CurrentUser.Id, options);
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> GetUsersAsync(IGuild guild, BaseDiscordClient client, ulong? fromUserId, int? limit, RequestOptions options)
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
			return (await client.ApiClient.GetGuildMembersAsync(guild.Id, getGuildMembersParams, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildMember x) => RestGuildUser.Create(client, guild, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestGuildUser> lastPage)
		{
			if (lastPage.Count != 1000)
			{
				return false;
			}
			info.Position = lastPage.Max((RestGuildUser x) => x.Id);
			return true;
		}, fromUserId, limit);
	}

	public static async Task<int> PruneUsersAsync(IGuild guild, BaseDiscordClient client, int days, bool simulate, RequestOptions options, IEnumerable<ulong> includeRoleIds)
	{
		GuildPruneParams args = new GuildPruneParams(days, includeRoleIds?.ToArray());
		GetGuildPruneCountResponse getGuildPruneCountResponse = ((!simulate) ? (await client.ApiClient.BeginGuildPruneAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false)) : (await client.ApiClient.GetGuildPruneCountAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false)));
		return getGuildPruneCountResponse.Pruned;
	}

	public static async Task<IReadOnlyCollection<RestGuildUser>> SearchUsersAsync(IGuild guild, BaseDiscordClient client, string query, int? limit, RequestOptions options)
	{
		SearchGuildMembersParams args = new SearchGuildMembersParams
		{
			Query = query,
			Limit = (((Optional<int>?)limit) ?? Optional.Create<int>())
		};
		return (await client.ApiClient.SearchGuildMembersAsync(guild.Id, args, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildMember x) => RestGuildUser.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<MemberSearchResult> SearchUsersAsyncV2(IGuild guild, BaseDiscordClient client, int limit, MemberSearchPropertiesV2 args, RequestOptions options)
	{
		SearchGuildMembersParamsV2 obj = new SearchGuildMembersParamsV2
		{
			Limit = limit
		};
		Discord.API.Rest.MemberSearchPaginationFilter memberSearchPaginationFilter = args?.After?.ToModel();
		obj.After = ((memberSearchPaginationFilter != null) ? ((Optional<Discord.API.Rest.MemberSearchPaginationFilter>)memberSearchPaginationFilter) : Optional<Discord.API.Rest.MemberSearchPaginationFilter>.Unspecified);
		memberSearchPaginationFilter = args?.Before?.ToModel();
		obj.Before = ((memberSearchPaginationFilter != null) ? ((Optional<Discord.API.Rest.MemberSearchPaginationFilter>)memberSearchPaginationFilter) : Optional<Discord.API.Rest.MemberSearchPaginationFilter>.Unspecified);
		obj.Sort = ((Optional<MemberSearchV2SortType>?)args?.Sort) ?? Optional<MemberSearchV2SortType>.Unspecified;
		Discord.API.Rest.MemberSearchFilter memberSearchFilter = args?.OrQuery?.ToModel();
		obj.OrQuery = ((memberSearchFilter != null) ? ((Optional<Discord.API.Rest.MemberSearchFilter>)memberSearchFilter) : Optional<Discord.API.Rest.MemberSearchFilter>.Unspecified);
		memberSearchFilter = args?.AndQuery?.ToModel();
		obj.AndQuery = ((memberSearchFilter != null) ? ((Optional<Discord.API.Rest.MemberSearchFilter>)memberSearchFilter) : Optional<Discord.API.Rest.MemberSearchFilter>.Unspecified);
		SearchGuildMembersParamsV2 args2 = obj;
		GuildMemberSearchResponse guildMemberSearchResponse = await client.ApiClient.SearchGuildMembersAsyncV2(guild.Id, args2, options);
		return new MemberSearchResult(guildMemberSearchResponse.GuildId, guildMemberSearchResponse.Members.Select((SupplementalGuildUser x) => new MemberSearchData(RestGuildUser.Create(client, guild, x.Member), x.InviteCode, x.JoinSourceType, x.InviterId)).ToImmutableArray(), guildMemberSearchResponse.PageResultCount, guildMemberSearchResponse.TotalResultCount);
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestAuditLogEntry>> GetAuditLogsAsync(IGuild guild, BaseDiscordClient client, ulong? from, int? limit, RequestOptions options, ulong? userId = null, ActionType? actionType = null, ulong? afterId = null)
	{
		return new PagedAsyncEnumerable<RestAuditLogEntry>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetAuditLogsParams getAuditLogsParams = new GetAuditLogsParams
			{
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getAuditLogsParams.BeforeEntryId = info.Position.Value;
			}
			if (userId.HasValue)
			{
				getAuditLogsParams.UserId = userId.Value;
			}
			if (actionType.HasValue)
			{
				getAuditLogsParams.ActionType = (int)actionType.Value;
			}
			if (afterId.HasValue)
			{
				getAuditLogsParams.AfterEntryId = afterId.Value;
			}
			AuditLog model = await client.ApiClient.GetAuditLogsAsync(guild.Id, getAuditLogsParams, options);
			return model.Entries.Select((AuditLogEntry x) => RestAuditLogEntry.Create(client, model, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestAuditLogEntry> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Min((RestAuditLogEntry x) => x.Id);
			return true;
		}, from, limit);
	}

	public static async Task<RestWebhook> GetWebhookAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Webhook webhook = await client.ApiClient.GetWebhookAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (webhook == null)
		{
			return null;
		}
		return RestWebhook.Create(client, guild, webhook);
	}

	public static async Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildWebhooksAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Webhook x) => RestWebhook.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<GuildEmote>> GetEmotesAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildEmotesAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((Discord.API.Emoji x) => x.ToEntity()).ToImmutableArray();
	}

	public static async Task<GuildEmote> GetEmoteAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		return (await client.ApiClient.GetGuildEmoteAsync(guild.Id, id, options).ConfigureAwait(continueOnCapturedContext: false)).ToEntity();
	}

	public static async Task<GuildEmote> CreateEmoteAsync(IGuild guild, BaseDiscordClient client, string name, Image image, Optional<IEnumerable<IRole>> roles, RequestOptions options)
	{
		CreateGuildEmoteParams createGuildEmoteParams = new CreateGuildEmoteParams
		{
			Name = name,
			Image = image.ToModel()
		};
		if (roles.IsSpecified)
		{
			createGuildEmoteParams.RoleIds = roles.Value?.Select((IRole xr) => xr.Id).ToArray();
		}
		return (await client.ApiClient.CreateGuildEmoteAsync(guild.Id, createGuildEmoteParams, options).ConfigureAwait(continueOnCapturedContext: false)).ToEntity();
	}

	public static async Task<GuildEmote> ModifyEmoteAsync(IGuild guild, BaseDiscordClient client, ulong id, Action<EmoteProperties> func, RequestOptions options)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		EmoteProperties emoteProperties = new EmoteProperties();
		func(emoteProperties);
		ModifyGuildEmoteParams modifyGuildEmoteParams = new ModifyGuildEmoteParams
		{
			Name = emoteProperties.Name
		};
		if (emoteProperties.Roles.IsSpecified)
		{
			modifyGuildEmoteParams.RoleIds = emoteProperties.Roles.Value?.Select((IRole xr) => xr.Id).ToArray();
		}
		return (await client.ApiClient.ModifyGuildEmoteAsync(guild.Id, id, modifyGuildEmoteParams, options).ConfigureAwait(continueOnCapturedContext: false)).ToEntity();
	}

	public static Task DeleteEmoteAsync(IGuild guild, BaseDiscordClient client, ulong id, RequestOptions options)
	{
		return client.ApiClient.DeleteGuildEmoteAsync(guild.Id, id, options);
	}

	public static async Task<Discord.API.Sticker> CreateStickerAsync(BaseDiscordClient client, IGuild guild, string name, Image image, IEnumerable<string> tags, string description = null, RequestOptions options = null)
	{
		Preconditions.NotNull(name, "name");
		if (description != null)
		{
			Preconditions.AtLeast(description.Length, 2, "description");
			Preconditions.AtMost(description.Length, 100, "description");
		}
		string text = string.Join(", ", tags);
		Preconditions.AtLeast(text.Length, 1, "tags");
		Preconditions.AtMost(text.Length, 200, "tags");
		Preconditions.AtLeast(name.Length, 2, "name");
		Preconditions.AtMost(name.Length, 30, "name");
		CreateStickerParams args = new CreateStickerParams
		{
			Name = name,
			Description = description,
			File = image.Stream,
			Tags = text
		};
		return await client.ApiClient.CreateGuildStickerAsync(args, guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task<Discord.API.Sticker> CreateStickerAsync(BaseDiscordClient client, IGuild guild, string name, Stream file, string filename, IEnumerable<string> tags, string description = null, RequestOptions options = null)
	{
		Preconditions.NotNull(name, "name");
		Preconditions.NotNull(file, "file");
		Preconditions.NotNull(filename, "filename");
		Preconditions.AtLeast(name.Length, 2, "name");
		Preconditions.AtMost(name.Length, 30, "name");
		if (description != null)
		{
			Preconditions.AtLeast(description.Length, 2, "description");
			Preconditions.AtMost(description.Length, 100, "description");
		}
		string text = string.Join(", ", tags);
		Preconditions.AtLeast(text.Length, 1, "tags");
		Preconditions.AtMost(text.Length, 200, "tags");
		CreateStickerParams args = new CreateStickerParams
		{
			Name = name,
			Description = description,
			File = file,
			Tags = text,
			FileName = filename
		};
		return client.ApiClient.CreateGuildStickerAsync(args, guild.Id, options);
	}

	public static Task<Discord.API.Sticker> ModifyStickerAsync(BaseDiscordClient client, ulong guildId, ISticker sticker, Action<StickerProperties> func, RequestOptions options = null)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		StickerProperties stickerProperties = new StickerProperties();
		func(stickerProperties);
		ModifyStickerParams args = new ModifyStickerParams
		{
			Description = stickerProperties.Description,
			Name = stickerProperties.Name,
			Tags = (stickerProperties.Tags.IsSpecified ? ((Optional<string>)string.Join(", ", stickerProperties.Tags.Value)) : Optional<string>.Unspecified)
		};
		return client.ApiClient.ModifyStickerAsync(args, guildId, sticker.Id, options);
	}

	public static Task DeleteStickerAsync(BaseDiscordClient client, ulong guildId, ISticker sticker, RequestOptions options = null)
	{
		return client.ApiClient.DeleteStickerAsync(guildId, sticker.Id, options);
	}

	public static async Task<IReadOnlyCollection<RestUser>> GetEventUsersAsync(BaseDiscordClient client, IGuildScheduledEvent guildEvent, int limit = 100, RequestOptions options = null)
	{
		return (await client.ApiClient.GetGuildScheduledEventUsersAsync(guildEvent.Id, guildEvent.Guild.Id, limit, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildScheduledEventUser x) => RestUser.Create(client, guildEvent.Guild, x)).ToImmutableArray();
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestUser>> GetEventUsersAsync(BaseDiscordClient client, IGuildScheduledEvent guildEvent, ulong? fromUserId, int? limit, RequestOptions options)
	{
		return new PagedAsyncEnumerable<RestUser>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetEventUsersParams getEventUsersParams = new GetEventUsersParams
			{
				Limit = info.PageSize,
				RelativeDirection = Direction.After
			};
			if (info.Position.HasValue)
			{
				getEventUsersParams.RelativeUserId = info.Position.Value;
			}
			return (await client.ApiClient.GetGuildScheduledEventUsersAsync(guildEvent.Id, guildEvent.Guild.Id, getEventUsersParams, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildScheduledEventUser x) => RestUser.Create(client, guildEvent.Guild, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestUser> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Max((RestUser x) => x.Id);
			return true;
		}, fromUserId, limit);
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestUser>> GetEventUsersAsync(BaseDiscordClient client, IGuildScheduledEvent guildEvent, ulong? fromUserId, Direction dir, int limit, RequestOptions options = null)
	{
		if (dir == Direction.Around && limit > 100)
		{
			int num = limit / 2;
			if (fromUserId.HasValue)
			{
				return GetEventUsersAsync(client, guildEvent, fromUserId.Value + 1, Direction.Before, num + 1, options).Concat(GetEventUsersAsync(client, guildEvent, fromUserId, Direction.After, num, options));
			}
			return GetEventUsersAsync(client, guildEvent, null, Direction.Before, num + 1, options);
		}
		return new PagedAsyncEnumerable<RestUser>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetEventUsersParams getEventUsersParams = new GetEventUsersParams
			{
				RelativeDirection = dir,
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getEventUsersParams.RelativeUserId = info.Position.Value;
			}
			GuildScheduledEventUser[] obj = await client.ApiClient.GetGuildScheduledEventUsersAsync(guildEvent.Id, guildEvent.Guild.Id, getEventUsersParams, options).ConfigureAwait(continueOnCapturedContext: false);
			ImmutableArray<RestUser>.Builder builder = ImmutableArray.CreateBuilder<RestUser>();
			GuildScheduledEventUser[] array = obj;
			foreach (GuildScheduledEventUser model in array)
			{
				builder.Add(RestUser.Create(client, guildEvent.Guild, model));
			}
			return builder.ToImmutable();
		}, delegate(PageInfo info, IReadOnlyCollection<RestUser> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			if (dir == Direction.Before)
			{
				info.Position = lastPage.Min((RestUser x) => x.Id);
			}
			else
			{
				info.Position = lastPage.Max((RestUser x) => x.Id);
			}
			return true;
		}, fromUserId, limit);
	}

	public static Task<GuildScheduledEvent> ModifyGuildEventAsync(BaseDiscordClient client, Action<GuildScheduledEventsProperties> func, IGuildScheduledEvent guildEvent, RequestOptions options = null)
	{
		GuildScheduledEventsProperties guildScheduledEventsProperties = new GuildScheduledEventsProperties();
		func(guildScheduledEventsProperties);
		if (guildScheduledEventsProperties.Status.IsSpecified)
		{
			switch (guildScheduledEventsProperties.Status.Value)
			{
			case GuildScheduledEventStatus.Active:
				if (guildEvent.Status == GuildScheduledEventStatus.Scheduled)
				{
					break;
				}
				goto IL_0061;
			case GuildScheduledEventStatus.Completed:
				if (guildEvent.Status == GuildScheduledEventStatus.Active)
				{
					break;
				}
				goto IL_0061;
			case GuildScheduledEventStatus.Cancelled:
				{
					if (guildEvent.Status == GuildScheduledEventStatus.Scheduled)
					{
						break;
					}
					goto IL_0061;
				}
				IL_0061:
				throw new ArgumentException($"Cannot set event to {guildScheduledEventsProperties.Status.Value} when events status is {guildEvent.Status}");
			}
		}
		if (guildScheduledEventsProperties.Type.IsSpecified && guildScheduledEventsProperties.Type.Value == GuildScheduledEventType.External)
		{
			if (!guildScheduledEventsProperties.Location.IsSpecified)
			{
				throw new ArgumentException("Location must be specified for external events.");
			}
			if (!guildScheduledEventsProperties.EndTime.IsSpecified)
			{
				throw new ArgumentException("End time must be specified for external events.");
			}
			if (!guildScheduledEventsProperties.ChannelId.IsSpecified)
			{
				throw new ArgumentException("Channel id must be set to null!");
			}
			if (guildScheduledEventsProperties.ChannelId.Value.HasValue)
			{
				throw new ArgumentException("Channel id must be set to null!");
			}
		}
		Optional<GuildScheduledEventRecurrenceRuleProperties> recurrenceRule = guildScheduledEventsProperties.RecurrenceRule;
		if (recurrenceRule.IsSpecified && recurrenceRule.Value != null)
		{
			GuildScheduledEventRecurrenceRuleProperties value = guildScheduledEventsProperties.RecurrenceRule.Value;
			bool flag = value.ByWeekday?.Any() ?? false;
			bool flag2 = value.ByNWeekday?.Any() ?? false;
			HashSet<int> byMonthDay = value.ByMonthDay;
			bool flag3 = (byMonthDay != null && byMonthDay.Any()) || (value.ByMonth?.Any() ?? false);
			if ((flag & flag2) || (flag & flag3) || (flag2 & flag3))
			{
				throw new ArgumentException("A recurrence rule can have one of ('ByWeekday', 'ByNWeekday', 'ByMonth' + 'ByMonthDay'), but not a combination of them.");
			}
			if (flag)
			{
				RecurrenceFrequency frequency = value.Frequency;
				if (frequency != RecurrenceFrequency.Daily && frequency != RecurrenceFrequency.Weekly)
				{
					throw new ArgumentException("A ByWeekday rule can only be used with Frequency of 'Daily' or 'Weekly'.");
				}
				if (value.Frequency == RecurrenceFrequency.Weekly)
				{
					if (value.ByWeekday.Count != 1)
					{
						throw new ArgumentException("A 'Weekly' recurrence rule must have a single weekday selected.");
					}
					if (value.Interval == 1)
					{
						throw new ArgumentException("Interval can only be set to a value other than '1' when Frequency is set to 'Weekly'");
					}
				}
			}
			if (flag2)
			{
				if (value.Frequency != RecurrenceFrequency.Monthly)
				{
					throw new ArgumentException(string.Format("A {0} rule must have {1} set to 'Monthly'.", value.ByNWeekday, "Frequency"));
				}
				if (value.ByNWeekday.Count != 1)
				{
					throw new ArgumentException($"A {value.ByNWeekday} must have exactly one day selected.");
				}
			}
			if (flag3)
			{
				if (value.Frequency != RecurrenceFrequency.Yearly)
				{
					throw new ArgumentException(string.Format("A {0} rule must have {1} set to 'Yearly'.", value.ByMonth, "Frequency"));
				}
				int? num = value.ByMonth?.Count;
				if (num.HasValue && num == 1)
				{
					num = value.ByMonthDay?.Count;
					if (num.HasValue && num == 1)
					{
						goto IL_0330;
					}
				}
				throw new ArgumentException($"A {value.ByMonth} rule must have exactly 1 day and 1 month selected.");
			}
		}
		goto IL_0330;
		IL_0330:
		ModifyGuildScheduledEventParams modifyGuildScheduledEventParams = new ModifyGuildScheduledEventParams
		{
			ChannelId = guildScheduledEventsProperties.ChannelId,
			Description = guildScheduledEventsProperties.Description,
			EndTime = guildScheduledEventsProperties.EndTime,
			Name = guildScheduledEventsProperties.Name,
			PrivacyLevel = guildScheduledEventsProperties.PrivacyLevel,
			StartTime = guildScheduledEventsProperties.StartTime,
			Status = guildScheduledEventsProperties.Status,
			Type = guildScheduledEventsProperties.Type,
			Image = (guildScheduledEventsProperties.CoverImage.IsSpecified ? ((Optional<Discord.API.Image?>)(guildScheduledEventsProperties.CoverImage.Value?.ToModel())) : Optional<Discord.API.Image?>.Unspecified),
			RecurrenceRule = (guildScheduledEventsProperties.RecurrenceRule.IsSpecified ? ((Optional<Discord.API.GuildScheduledEventRecurrenceRule>)(guildScheduledEventsProperties.RecurrenceRule.Value?.ToModel())) : Optional<Discord.API.GuildScheduledEventRecurrenceRule>.Unspecified)
		};
		if (guildScheduledEventsProperties.Location.IsSpecified)
		{
			modifyGuildScheduledEventParams.EntityMetadata = new GuildScheduledEventEntityMetadata
			{
				Location = guildScheduledEventsProperties.Location
			};
		}
		return client.ApiClient.ModifyGuildScheduledEventAsync(modifyGuildScheduledEventParams, guildEvent.Id, guildEvent.Guild.Id, options);
	}

	public static async Task<RestGuildEvent> GetGuildEventAsync(BaseDiscordClient client, ulong id, IGuild guild, RequestOptions options = null)
	{
		GuildScheduledEvent guildScheduledEvent = await client.ApiClient.GetGuildScheduledEventAsync(id, guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildScheduledEvent == null)
		{
			return null;
		}
		return RestGuildEvent.Create(client, guild, guildScheduledEvent);
	}

	public static async Task<IReadOnlyCollection<RestGuildEvent>> GetGuildEventsAsync(BaseDiscordClient client, IGuild guild, RequestOptions options = null)
	{
		return (await client.ApiClient.ListGuildScheduledEventsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Select((GuildScheduledEvent x) => RestGuildEvent.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<RestGuildEvent> CreateGuildEventAsync(BaseDiscordClient client, IGuild guild, string name, GuildScheduledEventPrivacyLevel privacyLevel, DateTimeOffset startTime, GuildScheduledEventType type, string description = null, DateTimeOffset? endTime = null, ulong? channelId = null, string location = null, Image? bannerImage = null, RequestOptions options = null, GuildScheduledEventRecurrenceRuleProperties recurrenceRule = null)
	{
		if (location != null)
		{
			Preconditions.AtMost(location.Length, 100, "location");
		}
		switch (type)
		{
		case GuildScheduledEventType.Stage:
		case GuildScheduledEventType.Voice:
			if (!channelId.HasValue)
			{
				throw new ArgumentException(string.Format("{0} must not be null when type is {1}", "channelId", type), "channelId");
			}
			break;
		case GuildScheduledEventType.External:
			if (channelId.HasValue)
			{
				throw new ArgumentException("channelId must be null when using external event type", "channelId");
			}
			if (location == null)
			{
				throw new ArgumentException("location must not be null when using external event type", "location");
			}
			if (!endTime.HasValue)
			{
				throw new ArgumentException("endTime must not be null when using external event type", "endTime");
			}
			break;
		}
		if (startTime <= DateTimeOffset.Now)
		{
			throw new ArgumentOutOfRangeException("startTime", "The start time for an event cannot be in the past");
		}
		if (endTime.HasValue && endTime <= startTime)
		{
			throw new ArgumentOutOfRangeException("endTime", "endTime cannot be before the start time");
		}
		if (recurrenceRule != null)
		{
			bool flag = recurrenceRule.ByWeekday?.Any() ?? false;
			bool flag2 = recurrenceRule.ByNWeekday?.Any() ?? false;
			bool flag3 = (recurrenceRule.ByMonthDay?.Any() ?? false) || (recurrenceRule.ByMonth?.Any() ?? false);
			if ((flag & flag2) || (flag & flag3) || (flag2 & flag3))
			{
				throw new ArgumentException("A recurrence rule can have one of ('ByWeekday', 'ByNWeekday', 'ByMonth' + 'ByMonthDay'), but not a combination of them.");
			}
			if (flag)
			{
				RecurrenceFrequency frequency = recurrenceRule.Frequency;
				if (frequency != RecurrenceFrequency.Daily && frequency != RecurrenceFrequency.Weekly)
				{
					throw new ArgumentException("A ByWeekday rule can only be used with Frequency of 'Daily' or 'Weekly'.");
				}
				if (recurrenceRule.Frequency == RecurrenceFrequency.Weekly)
				{
					if (recurrenceRule.ByWeekday.Count != 1)
					{
						throw new ArgumentException("A 'Weekly' recurrence rule must have a single weekday selected.");
					}
					if (recurrenceRule.Interval == 1)
					{
						throw new ArgumentException("Interval can only be set to a value other than '1' when Frequency is set to 'Weekly'");
					}
				}
			}
			if (flag2)
			{
				if (recurrenceRule.Frequency != RecurrenceFrequency.Monthly)
				{
					throw new ArgumentException(string.Format("A {0} rule must have {1} set to 'Monthly'.", recurrenceRule.ByNWeekday, "Frequency"));
				}
				if (recurrenceRule.ByNWeekday.Count != 1)
				{
					throw new ArgumentException($"A {recurrenceRule.ByNWeekday} must have exactly one day selected.");
				}
			}
			if (flag3)
			{
				if (recurrenceRule.Frequency != RecurrenceFrequency.Yearly)
				{
					throw new ArgumentException(string.Format("A {0} rule must have {1} set to 'Yearly'.", recurrenceRule.ByMonth, "Frequency"));
				}
				int? num = recurrenceRule.ByMonth?.Count;
				if (num.HasValue && num == 1)
				{
					num = recurrenceRule.ByMonthDay?.Count;
					if (num.HasValue && num == 1)
					{
						goto IL_0355;
					}
				}
				throw new ArgumentException($"A {recurrenceRule.ByMonth} rule must have exactly 1 day and 1 month selected.");
			}
		}
		goto IL_0355;
		IL_0355:
		CreateGuildScheduledEventParams obj = new CreateGuildScheduledEventParams
		{
			ChannelId = (((Optional<ulong>?)channelId) ?? Optional<ulong>.Unspecified)
		};
		obj.Description = ((description != null) ? ((Optional<string>)description) : Optional<string>.Unspecified);
		obj.EndTime = ((Optional<DateTimeOffset>?)endTime) ?? Optional<DateTimeOffset>.Unspecified;
		obj.Name = name;
		obj.PrivacyLevel = privacyLevel;
		obj.StartTime = startTime;
		obj.Type = type;
		obj.Image = ((Optional<Discord.API.Image>?)bannerImage?.ToModel()) ?? Optional<Discord.API.Image>.Unspecified;
		Discord.API.GuildScheduledEventRecurrenceRule guildScheduledEventRecurrenceRule = recurrenceRule?.ToModel();
		obj.RecurrenceRule = ((guildScheduledEventRecurrenceRule != null) ? ((Optional<Discord.API.GuildScheduledEventRecurrenceRule>)guildScheduledEventRecurrenceRule) : Optional<Discord.API.GuildScheduledEventRecurrenceRule>.Unspecified);
		CreateGuildScheduledEventParams createGuildScheduledEventParams = obj;
		if (location != null)
		{
			createGuildScheduledEventParams.EntityMetadata = new GuildScheduledEventEntityMetadata
			{
				Location = location
			};
		}
		GuildScheduledEvent model = await client.ApiClient.CreateGuildScheduledEventAsync(createGuildScheduledEventParams, guild.Id, options).ConfigureAwait(continueOnCapturedContext: false);
		return RestGuildEvent.Create(client, guild, client.CurrentUser, model);
	}

	public static Task DeleteEventAsync(BaseDiscordClient client, IGuildScheduledEvent guildEvent, RequestOptions options = null)
	{
		return client.ApiClient.DeleteGuildScheduledEventAsync(guildEvent.Id, guildEvent.Guild.Id, options);
	}

	public static async Task<WelcomeScreen> GetWelcomeScreenAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		Discord.API.WelcomeScreen welcomeScreen = await client.ApiClient.GetGuildWelcomeScreenAsync(guild.Id, options);
		if (welcomeScreen.WelcomeChannels.Length == 0)
		{
			return null;
		}
		return new WelcomeScreen(welcomeScreen.Description.GetValueOrDefault(null), welcomeScreen.WelcomeChannels.Select((Discord.API.WelcomeScreenChannel x) => new WelcomeScreenChannel(x.ChannelId, x.Description, x.EmojiName.GetValueOrDefault(null), x.EmojiId.GetValueOrDefault(0uL))).ToList());
	}

	public static async Task<WelcomeScreen> ModifyWelcomeScreenAsync(bool enabled, string description, WelcomeScreenChannelProperties[] channels, IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		if (!guild.Features.HasFeature(GuildFeature.Community))
		{
			throw new InvalidOperationException("Cannot update welcome screen in a non-community guild.");
		}
		ModifyGuildWelcomeScreenParams args = new ModifyGuildWelcomeScreenParams
		{
			Enabled = enabled,
			Description = description,
			WelcomeChannels = channels?.Select((WelcomeScreenChannelProperties ch) => new Discord.API.WelcomeScreenChannel
			{
				ChannelId = ch.Id,
				Description = ch.Description,
				EmojiName = ((ch.Emoji is Emoji emoji) ? ((Optional<string>)emoji.Name) : Optional<string>.Unspecified),
				EmojiId = ((ch.Emoji is Emote emote) ? ((Optional<ulong?>)emote.Id) : Optional<ulong?>.Unspecified)
			}).ToArray()
		};
		Discord.API.WelcomeScreen welcomeScreen = await client.ApiClient.ModifyGuildWelcomeScreenAsync(args, guild.Id, options);
		if (welcomeScreen.WelcomeChannels.Length == 0)
		{
			return null;
		}
		return new WelcomeScreen(welcomeScreen.Description.GetValueOrDefault(null), welcomeScreen.WelcomeChannels.Select((Discord.API.WelcomeScreenChannel x) => new WelcomeScreenChannel(x.ChannelId, x.Description, x.EmojiName.GetValueOrDefault(null), x.EmojiId.GetValueOrDefault(0uL))).ToList());
	}

	public static Task<AutoModerationRule> CreateAutoModRuleAsync(IGuild guild, Action<AutoModRuleProperties> func, BaseDiscordClient client, RequestOptions options)
	{
		AutoModRuleProperties autoModRuleProperties = new AutoModRuleProperties();
		func(autoModRuleProperties);
		if (!autoModRuleProperties.TriggerType.IsSpecified)
		{
			throw new ArgumentException("AutoMod rule must have a specified type.", "TriggerType");
		}
		if (!autoModRuleProperties.Name.IsSpecified || string.IsNullOrWhiteSpace(autoModRuleProperties.Name.Value))
		{
			throw new ArgumentException("Name of the rule must not be empty", "Name");
		}
		Preconditions.AtLeast(autoModRuleProperties.Actions.GetValueOrDefault(Array.Empty<AutoModRuleActionProperties>()).Length, 1, "Actions", "Auto moderation rule must have at least 1 action");
		if (autoModRuleProperties.RegexPatterns.IsSpecified)
		{
			AutoModTriggerType value = autoModRuleProperties.TriggerType.Value;
			if (value != AutoModTriggerType.Keyword && value != AutoModTriggerType.MemberProfile)
			{
				throw new ArgumentException("Regex patterns can only be used with 'Keyword' or 'MemberProfile' trigger type.", "RegexPatterns");
			}
			Preconditions.AtMost(autoModRuleProperties.RegexPatterns.Value.Length, 10, "RegexPatterns", $"Regex pattern count must be less than or equal to {10}.");
			if (autoModRuleProperties.RegexPatterns.Value.Any((string x) => x.Length > 260))
			{
				throw new ArgumentException($"Regex pattern must be less than or equal to {260}.", "RegexPatterns");
			}
		}
		if (autoModRuleProperties.KeywordFilter.IsSpecified)
		{
			AutoModTriggerType value = autoModRuleProperties.TriggerType.Value;
			if (value != AutoModTriggerType.Keyword && value != AutoModTriggerType.MemberProfile)
			{
				throw new ArgumentException("Keyword filter can only be used with 'Keyword' or 'MemberProfile' trigger type.", "KeywordFilter");
			}
			Preconditions.AtMost(autoModRuleProperties.KeywordFilter.Value.Length, 1000, "KeywordFilter", $"Keyword count must be less than or equal to {1000}");
			if (autoModRuleProperties.KeywordFilter.Value.Any((string x) => x.Length > 60))
			{
				throw new ArgumentException($"Keyword length must be less than or equal to {60}.", "KeywordFilter");
			}
		}
		if (autoModRuleProperties.TriggerType.Value == AutoModTriggerType.Keyword)
		{
			Preconditions.AtLeast(autoModRuleProperties.KeywordFilter.GetValueOrDefault(Array.Empty<string>()).Length + autoModRuleProperties.RegexPatterns.GetValueOrDefault(Array.Empty<string>()).Length, 1, "KeywordFilter & RegexPatterns", "Auto moderation rule must have at least 1 keyword or regex pattern");
		}
		if (autoModRuleProperties.AllowList.IsSpecified)
		{
			AutoModTriggerType value = autoModRuleProperties.TriggerType.Value;
			if (value != AutoModTriggerType.Keyword && value != AutoModTriggerType.KeywordPreset && value != AutoModTriggerType.MemberProfile)
			{
				throw new ArgumentException("Allow list can only be used with 'Keyword', 'KeywordPreset' or 'MemberProfile' trigger type.", "AllowList");
			}
			if (autoModRuleProperties.TriggerType.Value == AutoModTriggerType.Keyword)
			{
				Preconditions.AtMost(autoModRuleProperties.AllowList.Value.Length, 100, "AllowList", $"Allow list entry count must be less than or equal to {100}.");
			}
			if (autoModRuleProperties.TriggerType.Value == AutoModTriggerType.KeywordPreset)
			{
				Preconditions.AtMost(autoModRuleProperties.AllowList.Value.Length, 1000, "AllowList", $"Allow list entry count must be less than or equal to {1000}.");
			}
			if (autoModRuleProperties.AllowList.Value.Any((string x) => x.Length > 60))
			{
				throw new ArgumentException($"Allow list entry length must be less than or equal to {60}.", "AllowList");
			}
		}
		if (autoModRuleProperties.TriggerType.Value != AutoModTriggerType.KeywordPreset && autoModRuleProperties.Presets.IsSpecified)
		{
			throw new ArgumentException("Keyword presets scan only be used with 'KeywordPreset' trigger type.", "Presets");
		}
		if (autoModRuleProperties.MentionLimit.IsSpecified)
		{
			if (autoModRuleProperties.TriggerType.Value != AutoModTriggerType.MentionSpam)
			{
				throw new ArgumentException("MentionLimit can only be used with 'MentionSpam' trigger type.", "MentionLimit");
			}
			Preconditions.AtMost(autoModRuleProperties.MentionLimit.Value, 50, "MentionLimit", $"Mention limit must be less or equal to {50}");
			Preconditions.AtLeast(autoModRuleProperties.MentionLimit.Value, 1, "MentionLimit", "Mention limit must be greater or equal to 1");
		}
		if (autoModRuleProperties.ExemptRoles.IsSpecified)
		{
			Preconditions.AtMost(autoModRuleProperties.ExemptRoles.Value.Length, 20, "ExemptRoles", $"Exempt roles count must be less than or equal to {20}.");
		}
		if (autoModRuleProperties.ExemptChannels.IsSpecified)
		{
			Preconditions.AtMost(autoModRuleProperties.ExemptChannels.Value.Length, 50, "ExemptChannels", $"Exempt channels count must be less than or equal to {50}.");
		}
		if (!autoModRuleProperties.Actions.IsSpecified || autoModRuleProperties.Actions.Value.Length == 0)
		{
			throw new ArgumentException("At least 1 action must be set for an auto moderation rule.", "Actions");
		}
		if (autoModRuleProperties.Actions.Value.Any((AutoModRuleActionProperties x) => x.TimeoutDuration.GetValueOrDefault().TotalSeconds > 2419200.0))
		{
			throw new ArgumentException($"Field count must be less than or equal to {2419200}.", "TimeoutDuration");
		}
		if (autoModRuleProperties.Actions.Value.Any((AutoModRuleActionProperties x) => x.CustomMessage.IsSpecified && x.CustomMessage.Value.Length > 150))
		{
			throw new ArgumentException($"Custom message length must be less than or equal to {150}.", "CustomMessage");
		}
		CreateAutoModRuleParams args = new CreateAutoModRuleParams
		{
			EventType = autoModRuleProperties.EventType.GetValueOrDefault(AutoModEventType.MessageSend),
			Enabled = autoModRuleProperties.Enabled.GetValueOrDefault(defaultValue: true),
			ExemptRoles = autoModRuleProperties.ExemptRoles.GetValueOrDefault(),
			ExemptChannels = autoModRuleProperties.ExemptChannels.GetValueOrDefault(),
			Name = autoModRuleProperties.Name.Value,
			TriggerType = autoModRuleProperties.TriggerType.Value,
			Actions = autoModRuleProperties.Actions.Value.Select((AutoModRuleActionProperties x) => new AutoModAction
			{
				Metadata = new ActionMetadata
				{
					ChannelId = (((Optional<ulong>?)x.ChannelId) ?? Optional<ulong>.Unspecified),
					DurationSeconds = (((Optional<int>?)(int?)x.TimeoutDuration?.TotalSeconds) ?? Optional<int>.Unspecified),
					CustomMessage = x.CustomMessage
				},
				Type = x.Type
			}).ToArray(),
			TriggerMetadata = new TriggerMetadata
			{
				AllowList = autoModRuleProperties.AllowList,
				KeywordFilter = autoModRuleProperties.KeywordFilter,
				MentionLimit = autoModRuleProperties.MentionLimit,
				Presets = autoModRuleProperties.Presets,
				RegexPatterns = autoModRuleProperties.RegexPatterns
			}
		};
		return client.ApiClient.CreateGuildAutoModRuleAsync(guild.Id, args, options);
	}

	public static Task<AutoModerationRule> GetAutoModRuleAsync(ulong ruleId, IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return client.ApiClient.GetGuildAutoModRuleAsync(guild.Id, ruleId, options);
	}

	public static Task<AutoModerationRule[]> GetAutoModRulesAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return client.ApiClient.GetGuildAutoModRulesAsync(guild.Id, options);
	}

	public static Task<AutoModerationRule> ModifyRuleAsync(BaseDiscordClient client, IAutoModRule rule, Action<AutoModRuleProperties> func, RequestOptions options)
	{
		AutoModRuleProperties autoModRuleProperties = new AutoModRuleProperties();
		func(autoModRuleProperties);
		ModifyAutoModRuleParams args = new ModifyAutoModRuleParams
		{
			Actions = (autoModRuleProperties.Actions.IsSpecified ? ((Optional<AutoModAction[]>)autoModRuleProperties.Actions.Value.Select((AutoModRuleActionProperties x) => new AutoModAction
			{
				Type = x.Type,
				Metadata = ((x.ChannelId.HasValue || x.TimeoutDuration.HasValue) ? ((Optional<ActionMetadata>)new ActionMetadata
				{
					ChannelId = (((Optional<ulong>?)x.ChannelId) ?? Optional<ulong>.Unspecified),
					DurationSeconds = (x.TimeoutDuration.HasValue ? ((Optional<int>)(int)Math.Floor(x.TimeoutDuration.Value.TotalSeconds)) : Optional<int>.Unspecified),
					CustomMessage = x.CustomMessage
				}) : Optional<ActionMetadata>.Unspecified)
			}).ToArray()) : Optional<AutoModAction[]>.Unspecified),
			Enabled = autoModRuleProperties.Enabled,
			EventType = autoModRuleProperties.EventType,
			ExemptChannels = autoModRuleProperties.ExemptChannels,
			ExemptRoles = autoModRuleProperties.ExemptRoles,
			Name = autoModRuleProperties.Name,
			TriggerType = autoModRuleProperties.TriggerType,
			TriggerMetadata = ((autoModRuleProperties.KeywordFilter.IsSpecified || autoModRuleProperties.Presets.IsSpecified || autoModRuleProperties.MentionLimit.IsSpecified || autoModRuleProperties.RegexPatterns.IsSpecified || autoModRuleProperties.AllowList.IsSpecified) ? ((Optional<TriggerMetadata>)new TriggerMetadata
			{
				KeywordFilter = (autoModRuleProperties.KeywordFilter.IsSpecified ? autoModRuleProperties.KeywordFilter : ((Optional<string[]>)rule.KeywordFilter.ToArray())),
				RegexPatterns = (autoModRuleProperties.RegexPatterns.IsSpecified ? autoModRuleProperties.RegexPatterns : ((Optional<string[]>)rule.RegexPatterns.ToArray())),
				AllowList = (autoModRuleProperties.AllowList.IsSpecified ? autoModRuleProperties.AllowList : ((Optional<string[]>)rule.AllowList.ToArray())),
				MentionLimit = (autoModRuleProperties.MentionLimit.IsSpecified ? autoModRuleProperties.MentionLimit : (((Optional<int>?)rule.MentionTotalLimit) ?? Optional<int>.Unspecified)),
				Presets = (autoModRuleProperties.Presets.IsSpecified ? autoModRuleProperties.Presets : ((Optional<KeywordPresetTypes[]>)rule.Presets.ToArray())),
				MentionRaidProtectionEnabled = (autoModRuleProperties.MentionRaidProtectionEnabled.IsSpecified ? autoModRuleProperties.MentionRaidProtectionEnabled : (((Optional<bool>?)rule.MentionRaidProtectionEnabled) ?? Optional<bool>.Unspecified))
			}) : Optional<TriggerMetadata>.Unspecified)
		};
		return client.ApiClient.ModifyGuildAutoModRuleAsync(rule.GuildId, rule.Id, args, options);
	}

	public static Task DeleteRuleAsync(BaseDiscordClient client, IAutoModRule rule, RequestOptions options)
	{
		return client.ApiClient.DeleteGuildAutoModRuleAsync(rule.GuildId, rule.Id, options);
	}

	public static Task<GuildOnboarding> GetGuildOnboardingAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return client.ApiClient.GetGuildOnboardingAsync(guild.Id, options);
	}

	public static Task<GuildOnboarding> ModifyGuildOnboardingAsync(IGuild guild, Action<GuildOnboardingProperties> func, BaseDiscordClient client, RequestOptions options)
	{
		GuildOnboardingProperties guildOnboardingProperties = new GuildOnboardingProperties();
		func(guildOnboardingProperties);
		ModifyGuildOnboardingParams args = new ModifyGuildOnboardingParams
		{
			DefaultChannelIds = (guildOnboardingProperties.ChannelIds.IsSpecified ? ((Optional<ulong[]>)guildOnboardingProperties.ChannelIds.Value.ToArray()) : Optional<ulong[]>.Unspecified),
			Enabled = guildOnboardingProperties.IsEnabled,
			Mode = guildOnboardingProperties.Mode,
			Prompts = (guildOnboardingProperties.Prompts.IsSpecified ? ((Optional<GuildOnboardingPromptParams[]>)(guildOnboardingProperties.Prompts.Value?.Select((GuildOnboardingPromptProperties prompt) => new GuildOnboardingPromptParams
			{
				Id = prompt.Id.GetValueOrDefault(),
				Type = prompt.Type,
				IsInOnboarding = prompt.IsInOnboarding,
				IsRequired = prompt.IsRequired,
				IsSingleSelect = prompt.IsSingleSelect,
				Title = prompt.Title,
				Options = prompt.Options?.Select((GuildOnboardingPromptOptionProperties option) => new GuildOnboardingPromptOptionParams
				{
					Title = option.Title,
					ChannelIds = option.ChannelIds?.ToArray(),
					RoleIds = option.RoleIds?.ToArray(),
					Description = option.Description,
					EmojiName = option.Emoji.GetValueOrDefault(null)?.Name,
					EmojiId = ((option.Emoji.GetValueOrDefault(null) is Emote emote) ? new ulong?(emote.Id) : ((ulong?)null)),
					EmojiAnimated = ((option.Emoji.GetValueOrDefault(null) is Emote emote2) ? new bool?(emote2.Animated) : ((bool?)null)),
					Id = (((Optional<ulong>?)option.Id) ?? Optional<ulong>.Unspecified)
				}).ToArray()
			}).ToArray())) : Optional<GuildOnboardingPromptParams[]>.Unspecified)
		};
		return client.ApiClient.ModifyGuildOnboardingAsync(guild.Id, args, options);
	}
}
