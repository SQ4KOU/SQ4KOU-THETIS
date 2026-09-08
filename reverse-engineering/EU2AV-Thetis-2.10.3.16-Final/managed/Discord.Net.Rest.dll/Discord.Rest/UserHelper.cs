using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class UserHelper
{
	public static Task<User> ModifyAsync(ISelfUser user, BaseDiscordClient client, Action<SelfUserProperties> func, RequestOptions options)
	{
		SelfUserProperties selfUserProperties = new SelfUserProperties();
		func(selfUserProperties);
		ModifyCurrentUserParams modifyCurrentUserParams = new ModifyCurrentUserParams
		{
			Avatar = (selfUserProperties.Avatar.IsSpecified ? ((Optional<Discord.API.Image?>)(selfUserProperties.Avatar.Value?.ToModel())) : Optional.Create<Discord.API.Image?>()),
			Username = selfUserProperties.Username,
			Banner = (selfUserProperties.Banner.IsSpecified ? ((Optional<Discord.API.Image?>)(selfUserProperties.Banner.Value?.ToModel())) : Optional.Create<Discord.API.Image?>())
		};
		if (!modifyCurrentUserParams.Avatar.IsSpecified && user.AvatarId != null)
		{
			modifyCurrentUserParams.Avatar = new Discord.API.Image(user.AvatarId);
		}
		return client.ApiClient.ModifySelfAsync(modifyCurrentUserParams, options);
	}

	public static async Task<GuildUserProperties> ModifyAsync(IGuildUser user, BaseDiscordClient client, Action<GuildUserProperties> func, RequestOptions options)
	{
		GuildUserProperties args = new GuildUserProperties();
		func(args);
		if (args.TimedOutUntil.IsSpecified && args.TimedOutUntil.Value.Value.Offset > new TimeSpan(28, 0, 0, 0))
		{
			throw new ArgumentOutOfRangeException("TimedOutUntil", "Offset cannot be more than 28 days from the current date.");
		}
		ModifyGuildMemberParams modifyGuildMemberParams = new ModifyGuildMemberParams
		{
			Deaf = args.Deaf,
			Mute = args.Mute,
			Nickname = args.Nickname,
			TimedOutUntil = args.TimedOutUntil,
			Flags = args.Flags
		};
		if (args.Channel.IsSpecified)
		{
			modifyGuildMemberParams.ChannelId = args.Channel.Value?.Id;
		}
		else if (args.ChannelId.IsSpecified)
		{
			modifyGuildMemberParams.ChannelId = args.ChannelId.Value;
		}
		if (args.Roles.IsSpecified)
		{
			modifyGuildMemberParams.RoleIds = args.Roles.Value.Select((IRole x) => x.Id).Distinct().ToArray();
		}
		else if (args.RoleIds.IsSpecified)
		{
			modifyGuildMemberParams.RoleIds = args.RoleIds.Value.Distinct().ToArray();
		}
		if (modifyGuildMemberParams.Nickname.IsSpecified && modifyGuildMemberParams.Nickname.Value == null)
		{
			modifyGuildMemberParams.Nickname = new Optional<string>(string.Empty);
		}
		await client.ApiClient.ModifyGuildMemberAsync(user.GuildId, user.Id, modifyGuildMemberParams, options).ConfigureAwait(continueOnCapturedContext: false);
		return args;
	}

	public static Task KickAsync(IGuildUser user, BaseDiscordClient client, string reason, RequestOptions options)
	{
		return client.ApiClient.RemoveGuildMemberAsync(user.GuildId, user.Id, reason, options);
	}

	public static async Task<RestDMChannel> CreateDMChannelAsync(IUser user, BaseDiscordClient client, RequestOptions options)
	{
		CreateDMChannelParams args = new CreateDMChannelParams(user.Id);
		return RestDMChannel.Create(client, await client.ApiClient.CreateDMChannelAsync(args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task AddRolesAsync(IGuildUser user, BaseDiscordClient client, IEnumerable<ulong> roleIds, RequestOptions options)
	{
		foreach (ulong roleId in roleIds)
		{
			await client.ApiClient.AddRoleAsync(user.Guild.Id, user.Id, roleId, options).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static async Task RemoveRolesAsync(IGuildUser user, BaseDiscordClient client, IEnumerable<ulong> roleIds, RequestOptions options)
	{
		foreach (ulong roleId in roleIds)
		{
			await client.ApiClient.RemoveRoleAsync(user.Guild.Id, user.Id, roleId, options).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static Task SetTimeoutAsync(IGuildUser user, BaseDiscordClient client, TimeSpan span, RequestOptions options)
	{
		if (span.TotalDays > 28.0)
		{
			throw new ArgumentOutOfRangeException("span", "Offset cannot be more than 28 days from the current date.");
		}
		if (span.Ticks <= 0)
		{
			throw new ArgumentOutOfRangeException("span", "Offset cannot hold no value or have a negative value.");
		}
		ModifyGuildMemberParams args = new ModifyGuildMemberParams
		{
			TimedOutUntil = DateTimeOffset.UtcNow.Add(span)
		};
		return client.ApiClient.ModifyGuildMemberAsync(user.Guild.Id, user.Id, args, options);
	}

	public static Task RemoveTimeOutAsync(IGuildUser user, BaseDiscordClient client, RequestOptions options)
	{
		ModifyGuildMemberParams args = new ModifyGuildMemberParams
		{
			TimedOutUntil = null
		};
		return client.ApiClient.ModifyGuildMemberAsync(user.Guild.Id, user.Id, args, options);
	}
}
