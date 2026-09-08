using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class SocketUser : SocketEntity<ulong>, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence
{
	public abstract bool IsBot { get; internal set; }

	public abstract string Username { get; internal set; }

	public abstract ushort DiscriminatorValue { get; internal set; }

	public abstract string AvatarId { get; internal set; }

	public abstract bool IsWebhook { get; }

	public UserProperties? PublicFlags { get; private set; }

	internal abstract SocketGlobalUser GlobalUser { get; set; }

	internal abstract SocketPresence Presence { get; set; }

	public abstract string GlobalName { get; internal set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public string Discriminator => DiscriminatorValue.ToString("D4");

	public string Mention => MentionUtils.MentionUser(base.Id);

	public UserStatus Status => Presence.Status;

	public IReadOnlyCollection<ClientType> ActiveClients => Presence.ActiveClients ?? ImmutableHashSet<ClientType>.Empty;

	public IReadOnlyCollection<IActivity> Activities => Presence.Activities ?? ImmutableList<IActivity>.Empty;

	public abstract string AvatarDecorationHash { get; internal set; }

	public abstract ulong? AvatarDecorationSkuId { get; internal set; }

	public abstract PrimaryGuild? PrimaryGuild { get; internal set; }

	public IReadOnlyCollection<SocketGuild> MutualGuilds => base.Discord.Guilds.Where((SocketGuild g) => g.GetUser(base.Id) != null).ToImmutableArray();

	private string DebuggerDisplay => string.Format("{0} ({1}{2})", Format.UsernameAndDiscriminator(this, base.Discord.FormatUsersInBidirectionalUnicode), base.Id, IsBot ? ", Bot" : "");

	internal SocketUser(DiscordSocketClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal virtual bool Update(ClientState state, User model)
	{
		if (Presence == null)
		{
			SocketPresence socketPresence = (Presence = new SocketPresence());
		}
		bool result = false;
		if (model.Avatar.IsSpecified && model.Avatar.Value != AvatarId)
		{
			AvatarId = model.Avatar.Value;
			result = true;
		}
		if (model.Discriminator.IsSpecified && ushort.Parse(model.Discriminator.GetValueOrDefault(null) ?? "0", NumberStyles.None, CultureInfo.InvariantCulture) != DiscriminatorValue)
		{
			DiscriminatorValue = ushort.Parse(model.Discriminator.GetValueOrDefault(null) ?? "0", NumberStyles.None, CultureInfo.InvariantCulture);
			result = true;
		}
		if (model.Bot.IsSpecified && model.Bot.Value != IsBot)
		{
			IsBot = model.Bot.Value;
			result = true;
		}
		if (model.Username.IsSpecified && model.Username.Value != Username)
		{
			Username = model.Username.Value;
			result = true;
		}
		if (model.PublicFlags.IsSpecified && model.PublicFlags.Value != PublicFlags)
		{
			PublicFlags = model.PublicFlags.Value;
			result = true;
		}
		if (model.GlobalName.IsSpecified && model.GlobalName.Value != GlobalName)
		{
			GlobalName = model.GlobalName.Value;
			result = true;
		}
		Optional<AvatarDecorationData> avatarDecoration = model.AvatarDecoration;
		if (avatarDecoration.IsSpecified && avatarDecoration.Value != null && (model.AvatarDecoration.Value.Asset != AvatarDecorationHash || model.AvatarDecoration.Value.SkuId != AvatarDecorationSkuId))
		{
			AvatarDecorationHash = model.AvatarDecoration.Value?.Asset;
			AvatarDecorationSkuId = model.AvatarDecoration.Value?.SkuId;
			result = true;
		}
		if (model.PrimaryGuild.IsSpecified)
		{
			if (model.PrimaryGuild.Value == null)
			{
				if (PrimaryGuild.HasValue)
				{
					result = true;
				}
				PrimaryGuild = null;
			}
			else if (PrimaryGuild?.GuildId != model.PrimaryGuild.Value.GuildId || PrimaryGuild?.BadgeHash != model.PrimaryGuild.Value.BadgeHash || PrimaryGuild?.Tag != model.PrimaryGuild.Value.Tag || PrimaryGuild?.IdentityEnabled != model.PrimaryGuild.Value.IdentityEnabled)
			{
				PrimaryGuild = new PrimaryGuild(model.PrimaryGuild.Value.GuildId, model.PrimaryGuild.Value.IdentityEnabled, model.PrimaryGuild.Value.Tag, model.PrimaryGuild.Value.BadgeHash);
				result = true;
			}
		}
		return result;
	}

	internal virtual void Update(Presence model)
	{
		if (Presence == null)
		{
			SocketPresence socketPresence = (Presence = new SocketPresence());
		}
		Presence.Update(model);
	}

	public async Task<IDMChannel> CreateDMChannelAsync(RequestOptions options = null)
	{
		return await UserHelper.CreateDMChannelAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return CDN.GetUserAvatarUrl(base.Id, AvatarId, size, format);
	}

	public string GetDefaultAvatarUrl()
	{
		if (DiscriminatorValue == 0)
		{
			return CDN.GetDefaultUserAvatarUrl(base.Id);
		}
		return CDN.GetDefaultUserAvatarUrl(DiscriminatorValue);
	}

	public virtual string GetDisplayAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return GetAvatarUrl(format, size) ?? GetDefaultAvatarUrl();
	}

	public string GetAvatarDecorationUrl()
	{
		if (AvatarDecorationHash == null)
		{
			return null;
		}
		return CDN.GetAvatarDecorationUrl(AvatarDecorationHash);
	}

	public override string ToString()
	{
		return Format.UsernameAndDiscriminator(this, base.Discord.FormatUsersInBidirectionalUnicode);
	}

	internal SocketUser Clone()
	{
		return MemberwiseClone() as SocketUser;
	}
}
