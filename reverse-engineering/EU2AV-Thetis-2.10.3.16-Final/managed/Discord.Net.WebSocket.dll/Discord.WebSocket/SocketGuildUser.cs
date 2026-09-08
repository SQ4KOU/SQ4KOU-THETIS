using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Audio;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SocketGuildUser : SocketUser, IGuildUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IVoiceState
{
	private long? _premiumSinceTicks;

	private long? _timedOutTicks;

	private long? _joinedAtTicks;

	private ImmutableArray<ulong> _roleIds;

	private bool? _isDeafened;

	private bool? _isMuted;

	internal override SocketGlobalUser GlobalUser { get; set; }

	public SocketGuild Guild { get; }

	public string DisplayName => Nickname ?? GlobalName ?? Username;

	public string Nickname { get; private set; }

	public string DisplayAvatarId => GuildAvatarId ?? AvatarId;

	public string GuildAvatarId { get; private set; }

	public override bool IsBot
	{
		get
		{
			return GlobalUser.IsBot;
		}
		internal set
		{
			GlobalUser.IsBot = value;
		}
	}

	public override string Username
	{
		get
		{
			return GlobalUser.Username;
		}
		internal set
		{
			GlobalUser.Username = value;
		}
	}

	public override ushort DiscriminatorValue
	{
		get
		{
			return GlobalUser.DiscriminatorValue;
		}
		internal set
		{
			GlobalUser.DiscriminatorValue = value;
		}
	}

	public override string AvatarId
	{
		get
		{
			return GlobalUser.AvatarId;
		}
		internal set
		{
			GlobalUser.AvatarId = value;
		}
	}

	public override string GlobalName
	{
		get
		{
			return GlobalUser.GlobalName;
		}
		internal set
		{
			GlobalUser.GlobalName = value;
		}
	}

	public override string AvatarDecorationHash
	{
		get
		{
			return GlobalUser.AvatarDecorationHash;
		}
		internal set
		{
			GlobalUser.AvatarDecorationHash = value;
		}
	}

	public override ulong? AvatarDecorationSkuId
	{
		get
		{
			return GlobalUser.AvatarDecorationSkuId;
		}
		internal set
		{
			GlobalUser.AvatarDecorationSkuId = value;
		}
	}

	public override PrimaryGuild? PrimaryGuild
	{
		get
		{
			return GlobalUser.PrimaryGuild;
		}
		internal set
		{
			GlobalUser.PrimaryGuild = value;
		}
	}

	public string GuildBannerHash { get; private set; }

	public GuildPermissions GuildPermissions => new GuildPermissions(Permissions.ResolveGuild(Guild, this));

	internal override SocketPresence Presence { get; set; }

	public override bool IsWebhook => false;

	public bool IsSelfDeafened => VoiceState?.IsSelfDeafened ?? false;

	public bool IsSelfMuted => VoiceState?.IsSelfMuted ?? false;

	public bool IsSuppressed => VoiceState?.IsSuppressed ?? false;

	public bool IsDeafened => VoiceState?.IsDeafened ?? (_isDeafened == true);

	public bool IsMuted => VoiceState?.IsMuted ?? (_isMuted == true);

	public bool IsStreaming => VoiceState?.IsStreaming ?? false;

	public bool IsVideoing => VoiceState?.IsVideoing ?? false;

	public DateTimeOffset? RequestToSpeakTimestamp => VoiceState?.RequestToSpeakTimestamp ?? ((DateTimeOffset?)null);

	public bool? IsPending { get; private set; }

	public GuildUserFlags Flags { get; private set; }

	public DateTimeOffset? JoinedAt => DateTimeUtils.FromTicks(_joinedAtTicks);

	public IReadOnlyCollection<SocketRole> Roles => (from id in _roleIds
		select Guild.GetRole(id) into x
		where x != null
		select x).ToReadOnlyCollection(() => _roleIds.Length);

	public SocketVoiceChannel VoiceChannel => VoiceState?.VoiceChannel;

	public string VoiceSessionId => VoiceState?.VoiceSessionId ?? "";

	public SocketVoiceState? VoiceState => Guild.GetVoiceState(base.Id);

	public AudioInStream AudioStream => Guild.GetAudioStream(base.Id);

	public DateTimeOffset? PremiumSince => DateTimeUtils.FromTicks(_premiumSinceTicks);

	public DateTimeOffset? TimedOutUntil
	{
		get
		{
			if (!_timedOutTicks.HasValue || _timedOutTicks.Value < 0)
			{
				return null;
			}
			return DateTimeUtils.FromTicks(_timedOutTicks);
		}
	}

	public int Hierarchy
	{
		get
		{
			if (Guild.OwnerId == base.Id)
			{
				return int.MaxValue;
			}
			int num = 0;
			for (int i = 0; i < _roleIds.Length; i++)
			{
				SocketRole role = Guild.GetRole(_roleIds[i]);
				if (role != null && role.Position > num)
				{
					num = role.Position;
				}
			}
			return num;
		}
	}

	private string DebuggerDisplay
	{
		get
		{
			if (DiscriminatorValue == 0)
			{
				return string.Format("{0} ({1}{2}, Guild)", Username, base.Id, IsBot ? ", Bot" : "");
			}
			return string.Format("{0}#{1} ({2}{3}, Guild)", Username, base.Discriminator, base.Id, IsBot ? ", Bot" : "");
		}
	}

	IGuild IGuildUser.Guild => Guild;

	ulong IGuildUser.GuildId => Guild.Id;

	IReadOnlyCollection<ulong> IGuildUser.RoleIds => _roleIds;

	IVoiceChannel IVoiceState.VoiceChannel => VoiceChannel;

	internal SocketGuildUser(SocketGuild guild, SocketGlobalUser globalUser)
		: base(guild.Discord, globalUser.Id)
	{
		Guild = guild;
		GlobalUser = globalUser;
	}

	internal static SocketGuildUser Create(SocketGuild guild, ClientState state, User model)
	{
		SocketGuildUser socketGuildUser = new SocketGuildUser(guild, guild.Discord.GetOrCreateUser(state, model));
		socketGuildUser.Update(state, model);
		socketGuildUser.UpdateRoles(Array.Empty<ulong>());
		return socketGuildUser;
	}

	internal static SocketGuildUser Create(SocketGuild guild, ClientState state, GuildMember model)
	{
		SocketGuildUser socketGuildUser = new SocketGuildUser(guild, guild.Discord.GetOrCreateUser(state, model.User));
		socketGuildUser.Update(state, model);
		if (!model.Roles.IsSpecified)
		{
			socketGuildUser.UpdateRoles(Array.Empty<ulong>());
		}
		return socketGuildUser;
	}

	internal static SocketGuildUser Create(SocketGuild guild, ClientState state, Presence model)
	{
		SocketGuildUser socketGuildUser = new SocketGuildUser(guild, guild.Discord.GetOrCreateUser(state, model.User));
		socketGuildUser.Update(state, model, updatePresence: false);
		if (!model.Roles.IsSpecified)
		{
			socketGuildUser.UpdateRoles(Array.Empty<ulong>());
		}
		return socketGuildUser;
	}

	internal void Update(ClientState state, GuildMember model)
	{
		base.Update(state, model.User);
		if (model.JoinedAt.IsSpecified)
		{
			_joinedAtTicks = model.JoinedAt.Value.GetValueOrDefault(DateTimeOffset.UtcNow).UtcTicks;
		}
		if (model.Nick.IsSpecified)
		{
			Nickname = model.Nick.Value;
		}
		if (model.Avatar.IsSpecified)
		{
			GuildAvatarId = model.Avatar.Value;
		}
		if (model.Roles.IsSpecified)
		{
			UpdateRoles(model.Roles.Value);
		}
		if (model.PremiumSince.IsSpecified)
		{
			_premiumSinceTicks = model.PremiumSince.Value?.UtcTicks;
		}
		if (model.TimedOutUntil.IsSpecified)
		{
			_timedOutTicks = model.TimedOutUntil.Value?.UtcTicks;
		}
		if (model.Pending.IsSpecified)
		{
			IsPending = model.Pending.Value;
		}
		if (model.Banner.IsSpecified)
		{
			GuildBannerHash = model.Banner.Value;
		}
		if (model.Mute.IsSpecified)
		{
			_isMuted = model.Mute.Value;
		}
		if (model.Deaf.IsSpecified)
		{
			_isDeafened = model.Deaf.Value;
		}
		Flags = model.Flags;
	}

	internal void Update(ClientState state, Presence model, bool updatePresence)
	{
		if (updatePresence)
		{
			Update(model);
		}
		if (model.Nick.IsSpecified)
		{
			Nickname = model.Nick.Value;
		}
		if (model.Roles.IsSpecified)
		{
			UpdateRoles(model.Roles.Value);
		}
		if (model.PremiumSince.IsSpecified)
		{
			_premiumSinceTicks = model.PremiumSince.Value?.UtcTicks;
		}
	}

	internal override void Update(Presence model)
	{
		if (Presence == null)
		{
			SocketPresence socketPresence = (Presence = new SocketPresence());
		}
		Presence.Update(model);
		GlobalUser.Update(model);
	}

	private void UpdateRoles(ulong[] roleIds)
	{
		ImmutableArray<ulong>.Builder builder = ImmutableArray.CreateBuilder<ulong>(roleIds.Length + 1);
		builder.Add(Guild.Id);
		for (int i = 0; i < roleIds.Length; i++)
		{
			builder.Add(roleIds[i]);
		}
		_roleIds = builder.ToImmutable();
	}

	public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
	{
		return UserHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public Task KickAsync(string reason = null, RequestOptions options = null)
	{
		return UserHelper.KickAsync(this, base.Discord, reason, options);
	}

	public Task AddRoleAsync(ulong roleId, RequestOptions options = null)
	{
		return AddRolesAsync(new ulong[1] { roleId }, options);
	}

	public Task AddRoleAsync(IRole role, RequestOptions options = null)
	{
		return AddRoleAsync(role.Id, options);
	}

	public Task AddRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options = null)
	{
		return UserHelper.AddRolesAsync(this, base.Discord, roleIds, options);
	}

	public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
	{
		return AddRolesAsync(roles.Select((IRole x) => x.Id), options);
	}

	public Task RemoveRoleAsync(ulong roleId, RequestOptions options = null)
	{
		return RemoveRolesAsync(new ulong[1] { roleId }, options);
	}

	public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
	{
		return RemoveRoleAsync(role.Id, options);
	}

	public Task RemoveRolesAsync(IEnumerable<ulong> roleIds, RequestOptions options = null)
	{
		return UserHelper.RemoveRolesAsync(this, base.Discord, roleIds, options);
	}

	public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
	{
		return RemoveRolesAsync(roles.Select((IRole x) => x.Id));
	}

	public Task SetTimeOutAsync(TimeSpan span, RequestOptions options = null)
	{
		return UserHelper.SetTimeoutAsync(this, base.Discord, span, options);
	}

	public Task RemoveTimeOutAsync(RequestOptions options = null)
	{
		return UserHelper.RemoveTimeOutAsync(this, base.Discord, options);
	}

	public ChannelPermissions GetPermissions(IGuildChannel channel)
	{
		return new ChannelPermissions(Permissions.ResolveChannel(Guild, this, channel, GuildPermissions.RawValue));
	}

	public string GetGuildAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return CDN.GetGuildUserAvatarUrl(base.Id, Guild.Id, GuildAvatarId, size, format);
	}

	public string GetGuildBannerUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return CDN.GetGuildUserBannerUrl(base.Id, Guild.Id, GuildBannerHash, size, format);
	}

	public override string GetDisplayAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return GetGuildAvatarUrl(format, size) ?? base.GetDisplayAvatarUrl(format, size);
	}

	internal new SocketGuildUser Clone()
	{
		SocketGuildUser obj = MemberwiseClone() as SocketGuildUser;
		obj.GlobalUser = GlobalUser.Clone();
		return obj;
	}
}
