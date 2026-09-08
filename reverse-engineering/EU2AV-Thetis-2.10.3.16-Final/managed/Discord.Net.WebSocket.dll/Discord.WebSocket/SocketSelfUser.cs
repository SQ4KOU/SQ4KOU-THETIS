using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SocketSelfUser : SocketUser, ISelfUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence
{
	public string Email { get; private set; }

	public bool IsVerified { get; private set; }

	public bool IsMfaEnabled { get; private set; }

	internal override SocketGlobalUser GlobalUser { get; set; }

	public string BannerId { get; internal set; }

	public Color? BannerColor { get; internal set; }

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

	internal override SocketPresence Presence
	{
		get
		{
			return GlobalUser.Presence;
		}
		set
		{
			GlobalUser.Presence = value;
		}
	}

	public UserProperties Flags { get; internal set; }

	public PremiumType PremiumType { get; internal set; }

	public string Locale { get; internal set; }

	public override bool IsWebhook => false;

	private string DebuggerDisplay
	{
		get
		{
			if (DiscriminatorValue == 0)
			{
				return string.Format("{0} ({1}{2}, Self)", Username, base.Id, IsBot ? ", Bot" : "");
			}
			return string.Format("{0}#{1} ({2}{3}, Self)", Username, base.Discriminator, base.Id, IsBot ? ", Bot" : "");
		}
	}

	internal SocketSelfUser(DiscordSocketClient discord, SocketGlobalUser globalUser)
		: base(discord, globalUser.Id)
	{
		GlobalUser = globalUser;
	}

	internal static SocketSelfUser Create(DiscordSocketClient discord, ClientState state, User model)
	{
		SocketSelfUser socketSelfUser = new SocketSelfUser(discord, discord.GetOrCreateSelfUser(state, model));
		socketSelfUser.Update(state, model);
		return socketSelfUser;
	}

	internal override bool Update(ClientState state, User model)
	{
		bool result = base.Update(state, model);
		if (model.Email.IsSpecified)
		{
			Email = model.Email.Value;
			result = true;
		}
		if (model.Verified.IsSpecified)
		{
			IsVerified = model.Verified.Value;
			result = true;
		}
		if (model.MfaEnabled.IsSpecified)
		{
			IsMfaEnabled = model.MfaEnabled.Value;
			result = true;
		}
		if (model.Flags.IsSpecified && model.Flags.Value != Flags)
		{
			Flags = model.Flags.Value;
			result = true;
		}
		if (model.PremiumType.IsSpecified && model.PremiumType.Value != PremiumType)
		{
			PremiumType = model.PremiumType.Value;
			result = true;
		}
		if (model.Locale.IsSpecified && model.Locale.Value != Locale)
		{
			Locale = model.Locale.Value;
			result = true;
		}
		if (model.BannerColor.IsSpecified && model.BannerColor.Value != BannerColor)
		{
			BannerColor = model.BannerColor.Value;
			result = true;
		}
		if (model.Banner.IsSpecified && model.Banner.Value != BannerId)
		{
			BannerId = model.Banner.Value;
			result = true;
		}
		return result;
	}

	public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions options = null)
	{
		return UserHelper.ModifyAsync(this, base.Discord, func, options);
	}

	internal new SocketSelfUser Clone()
	{
		return MemberwiseClone() as SocketSelfUser;
	}
}
