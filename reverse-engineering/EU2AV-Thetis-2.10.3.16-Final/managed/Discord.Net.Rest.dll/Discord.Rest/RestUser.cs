using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class RestUser : RestEntity<ulong>, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence, IUpdateable
{
	public bool IsBot { get; private set; }

	public string Username { get; private set; }

	public ushort DiscriminatorValue { get; private set; }

	public string AvatarId { get; private set; }

	public string BannerId { get; private set; }

	public Color? BannerColor { get; private set; }

	public Color? AccentColor { get; private set; }

	public UserProperties? PublicFlags { get; private set; }

	public string GlobalName { get; internal set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public string Discriminator => DiscriminatorValue.ToString("D4");

	public string Mention => MentionUtils.MentionUser(base.Id);

	public virtual IActivity Activity => null;

	public virtual UserStatus Status => UserStatus.Offline;

	public virtual IReadOnlyCollection<ClientType> ActiveClients => ImmutableHashSet<ClientType>.Empty;

	public virtual IReadOnlyCollection<IActivity> Activities => ImmutableList<IActivity>.Empty;

	public virtual bool IsWebhook => false;

	public string AvatarDecorationHash { get; private set; }

	public ulong? AvatarDecorationSkuId { get; private set; }

	public PrimaryGuild? PrimaryGuild { get; private set; }

	private string DebuggerDisplay => string.Format("{0} ({1}{2})", Format.UsernameAndDiscriminator(this, base.Discord.FormatUsersInBidirectionalUnicode), base.Id, IsBot ? ", Bot" : "");

	internal RestUser(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestUser Create(BaseDiscordClient discord, User model)
	{
		return Create(discord, null, model, null);
	}

	internal static RestUser Create(BaseDiscordClient discord, IGuild guild, User model, ulong? webhookId)
	{
		RestUser restUser = ((!webhookId.HasValue) ? new RestUser(discord, model.Id) : new RestWebhookUser(discord, guild, model.Id, webhookId.Value));
		restUser.Update(model);
		return restUser;
	}

	internal static RestUser Create(BaseDiscordClient discord, IGuild guild, GuildScheduledEventUser model)
	{
		if (model.Member.IsSpecified)
		{
			GuildMember value = model.Member.Value;
			value.User = model.User;
			return RestGuildUser.Create(discord, guild, value);
		}
		return Create(discord, model.User);
	}

	internal virtual void Update(User model)
	{
		if (model.Avatar.IsSpecified)
		{
			AvatarId = model.Avatar.Value;
		}
		if (model.Banner.IsSpecified)
		{
			BannerId = model.Banner.Value;
		}
		if (model.BannerColor.IsSpecified)
		{
			BannerColor = model.BannerColor.Value;
		}
		if (model.AccentColor.IsSpecified)
		{
			AccentColor = model.AccentColor.Value;
		}
		if (model.Discriminator.IsSpecified)
		{
			DiscriminatorValue = ushort.Parse(model.Discriminator.GetValueOrDefault(null) ?? "0", NumberStyles.None, CultureInfo.InvariantCulture);
		}
		if (model.Bot.IsSpecified)
		{
			IsBot = model.Bot.Value;
		}
		if (model.Username.IsSpecified)
		{
			Username = model.Username.Value;
		}
		if (model.PublicFlags.IsSpecified)
		{
			PublicFlags = model.PublicFlags.Value;
		}
		if (model.GlobalName.IsSpecified)
		{
			GlobalName = model.GlobalName.Value;
		}
		Optional<AvatarDecorationData> avatarDecoration = model.AvatarDecoration;
		if (avatarDecoration.IsSpecified && avatarDecoration.Value != null)
		{
			AvatarDecorationHash = model.AvatarDecoration.Value?.Asset;
			AvatarDecorationSkuId = model.AvatarDecoration.Value?.SkuId;
		}
		if (model.PrimaryGuild.IsSpecified)
		{
			if (model.PrimaryGuild.Value == null)
			{
				PrimaryGuild = null;
			}
			else
			{
				PrimaryGuild = new PrimaryGuild(model.PrimaryGuild.Value.GuildId, model.PrimaryGuild.Value.IdentityEnabled, model.PrimaryGuild.Value.Tag, model.PrimaryGuild.Value.BadgeHash);
			}
		}
	}

	public virtual async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetUserAsync(base.Id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task<RestDMChannel> CreateDMChannelAsync(RequestOptions options = null)
	{
		return UserHelper.CreateDMChannelAsync(this, base.Discord, options);
	}

	public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
	{
		return CDN.GetUserAvatarUrl(base.Id, AvatarId, size, format);
	}

	public string GetBannerUrl(ImageFormat format = ImageFormat.Auto, ushort size = 256)
	{
		return CDN.GetUserBannerUrl(base.Id, BannerId, size, format);
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

	async Task<IDMChannel> IUser.CreateDMChannelAsync(RequestOptions options)
	{
		return await CreateDMChannelAsync(options).ConfigureAwait(continueOnCapturedContext: false);
	}
}
