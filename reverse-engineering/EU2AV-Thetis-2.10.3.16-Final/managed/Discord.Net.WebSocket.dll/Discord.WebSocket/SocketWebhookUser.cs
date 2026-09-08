using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SocketWebhookUser : SocketUser, IWebhookUser, IGuildUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
	public SocketGuild Guild { get; }

	public ulong WebhookId { get; }

	public override string Username { get; internal set; }

	public override ushort DiscriminatorValue { get; internal set; }

	public override string AvatarId { get; internal set; }

	public override string GlobalName { get; internal set; }

	public override string AvatarDecorationHash { get; internal set; }

	public override ulong? AvatarDecorationSkuId { get; internal set; }

	public override PrimaryGuild? PrimaryGuild { get; internal set; }

	public override bool IsBot { get; internal set; }

	public override bool IsWebhook => true;

	internal override SocketPresence Presence
	{
		get
		{
			return new SocketPresence(UserStatus.Offline, null, null);
		}
		set
		{
		}
	}

	internal override SocketGlobalUser GlobalUser
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	private string DebuggerDisplay
	{
		get
		{
			if (DiscriminatorValue == 0)
			{
				return string.Format("{0} ({1}{2}, Webhook)", Username, base.Id, IsBot ? ", Bot" : "");
			}
			return string.Format("{0}#{1} ({2}{3}, Webhook)", Username, base.Discriminator, base.Id, IsBot ? ", Bot" : "");
		}
	}

	IGuild IGuildUser.Guild => Guild;

	ulong IGuildUser.GuildId => Guild.Id;

	IReadOnlyCollection<ulong> IGuildUser.RoleIds => ImmutableArray.Create<ulong>();

	DateTimeOffset? IGuildUser.JoinedAt => null;

	string IGuildUser.DisplayName => null;

	string IGuildUser.Nickname => null;

	string IGuildUser.DisplayAvatarId => null;

	string IGuildUser.GuildAvatarId => null;

	string IGuildUser.GuildBannerHash => null;

	DateTimeOffset? IGuildUser.PremiumSince => null;

	DateTimeOffset? IGuildUser.TimedOutUntil => null;

	bool? IGuildUser.IsPending => null;

	int IGuildUser.Hierarchy => 0;

	GuildPermissions IGuildUser.GuildPermissions => GuildPermissions.Webhook;

	GuildUserFlags IGuildUser.Flags => GuildUserFlags.None;

	bool IVoiceState.IsDeafened => false;

	bool IVoiceState.IsMuted => false;

	bool IVoiceState.IsSelfDeafened => false;

	bool IVoiceState.IsSelfMuted => false;

	bool IVoiceState.IsSuppressed => false;

	IVoiceChannel IVoiceState.VoiceChannel => null;

	string IVoiceState.VoiceSessionId => null;

	bool IVoiceState.IsStreaming => false;

	bool IVoiceState.IsVideoing => false;

	DateTimeOffset? IVoiceState.RequestToSpeakTimestamp => null;

	internal SocketWebhookUser(SocketGuild guild, ulong id, ulong webhookId)
		: base(guild.Discord, id)
	{
		Guild = guild;
		WebhookId = webhookId;
	}

	internal static SocketWebhookUser Create(SocketGuild guild, ClientState state, User model, ulong webhookId)
	{
		SocketWebhookUser socketWebhookUser = new SocketWebhookUser(guild, model.Id, webhookId);
		socketWebhookUser.Update(state, model);
		return socketWebhookUser;
	}

	internal new SocketWebhookUser Clone()
	{
		return MemberwiseClone() as SocketWebhookUser;
	}

	string IGuildUser.GetGuildAvatarUrl(ImageFormat format, ushort size)
	{
		return null;
	}

	string IGuildUser.GetGuildBannerUrl(ImageFormat format, ushort size)
	{
		return null;
	}

	ChannelPermissions IGuildUser.GetPermissions(IGuildChannel channel)
	{
		return Permissions.ToChannelPerms(channel, GuildPermissions.Webhook.RawValue);
	}

	Task IGuildUser.KickAsync(string reason, RequestOptions options)
	{
		throw new NotSupportedException("Webhook users cannot be kicked.");
	}

	Task IGuildUser.ModifyAsync(Action<GuildUserProperties> func, RequestOptions options)
	{
		throw new NotSupportedException("Webhook users cannot be modified.");
	}

	Task IGuildUser.AddRoleAsync(ulong roleId, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.AddRoleAsync(IRole role, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.AddRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.RemoveRoleAsync(ulong roleId, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.RemoveRoleAsync(IRole role, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.RemoveRolesAsync(IEnumerable<ulong> roles, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options)
	{
		throw new NotSupportedException("Roles are not supported on webhook users.");
	}

	Task IGuildUser.SetTimeOutAsync(TimeSpan span, RequestOptions options)
	{
		throw new NotSupportedException("Timeouts are not supported on webhook users.");
	}

	Task IGuildUser.RemoveTimeOutAsync(RequestOptions options)
	{
		throw new NotSupportedException("Timeouts are not supported on webhook users.");
	}
}
