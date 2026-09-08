using System;
using System.Diagnostics;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SocketUnknownUser : SocketUser
{
	public override string Username { get; internal set; }

	public override ushort DiscriminatorValue { get; internal set; }

	public override string AvatarId { get; internal set; }

	public override string GlobalName { get; internal set; }

	public override string AvatarDecorationHash { get; internal set; }

	public override ulong? AvatarDecorationSkuId { get; internal set; }

	public override PrimaryGuild? PrimaryGuild { get; internal set; }

	public override bool IsBot { get; internal set; }

	public override bool IsWebhook => false;

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
				return string.Format("{0} ({1}{2}, Unknown)", Username, base.Id, IsBot ? ", Bot" : "");
			}
			return string.Format("{0}#{1} ({2}{3}, Unknown)", Username, base.Discriminator, base.Id, IsBot ? ", Bot" : "");
		}
	}

	internal SocketUnknownUser(DiscordSocketClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static SocketUnknownUser Create(DiscordSocketClient discord, ClientState state, User model)
	{
		SocketUnknownUser socketUnknownUser = new SocketUnknownUser(discord, model.Id);
		socketUnknownUser.Update(state, model);
		return socketUnknownUser;
	}

	internal new SocketUnknownUser Clone()
	{
		return MemberwiseClone() as SocketUnknownUser;
	}
}
