using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class RestInvite : RestEntity<string>, IInvite, IEntity<string>, IDeletable, IUpdateable
{
	public ChannelType ChannelType { get; private set; }

	public string ChannelName { get; private set; }

	public string GuildName { get; private set; }

	public int? PresenceCount { get; private set; }

	public int? MemberCount { get; private set; }

	public ulong ChannelId { get; private set; }

	public ulong? GuildId { get; private set; }

	public IUser Inviter { get; private set; }

	public IUser TargetUser { get; private set; }

	public TargetUserType TargetUserType { get; private set; }

	public PartialGuild PartialGuild { get; private set; }

	public RestApplication Application { get; private set; }

	public DateTimeOffset? ExpiresAt { get; private set; }

	public RestGuildEvent ScheduledEvent { get; private set; }

	internal IChannel Channel { get; }

	internal IGuild Guild { get; }

	public string Code => base.Id;

	public string Url => "https://discord.gg/" + Code;

	private string DebuggerDisplay => Url + " (" + GuildName + " / " + ChannelName + ")";

	IGuild IInvite.Guild
	{
		get
		{
			if (Guild != null)
			{
				return Guild;
			}
			if (Channel is IGuildChannel guildChannel)
			{
				return guildChannel.Guild;
			}
			throw new InvalidOperationException("Unable to return this entity's parent unless it was fetched through that object.");
		}
	}

	IChannel IInvite.Channel
	{
		get
		{
			if (Channel != null)
			{
				return Channel;
			}
			throw new InvalidOperationException("Unable to return this entity's parent unless it was fetched through that object.");
		}
	}

	IApplication IInvite.Application => Application;

	internal RestInvite(BaseDiscordClient discord, IGuild guild, IChannel channel, string id)
		: base(discord, id)
	{
		Guild = guild;
		Channel = channel;
	}

	internal static RestInvite Create(BaseDiscordClient discord, IGuild guild, IChannel channel, Invite model)
	{
		RestInvite restInvite = new RestInvite(discord, guild, channel, model.Code);
		restInvite.Update(model);
		return restInvite;
	}

	internal void Update(Invite model)
	{
		GuildId = (model.Guild.IsSpecified ? new ulong?(model.Guild.Value.Id) : ((ulong?)null));
		ChannelId = model.Channel.Id;
		GuildName = (model.Guild.IsSpecified ? model.Guild.Value.Name : null);
		ChannelName = model.Channel.Name;
		MemberCount = (model.MemberCount.IsSpecified ? model.MemberCount.Value : ((int?)null));
		PresenceCount = (model.PresenceCount.IsSpecified ? model.PresenceCount.Value : ((int?)null));
		ChannelType = (ChannelType)model.Channel.Type;
		Inviter = (model.Inviter.IsSpecified ? RestUser.Create(base.Discord, model.Inviter.Value) : null);
		TargetUser = (model.TargetUser.IsSpecified ? RestUser.Create(base.Discord, model.TargetUser.Value) : null);
		TargetUserType = (model.TargetUserType.IsSpecified ? model.TargetUserType.Value : TargetUserType.Undefined);
		if (model.Guild.IsSpecified)
		{
			PartialGuild = PartialGuildExtensions.Create(model.Guild.Value);
		}
		if (model.Application.IsSpecified)
		{
			Application = RestApplication.Create(base.Discord, model.Application.Value);
		}
		ExpiresAt = (model.ExpiresAt.IsSpecified ? model.ExpiresAt.Value : ((DateTimeOffset?)null));
		if (model.ScheduledEvent.IsSpecified)
		{
			ScheduledEvent = RestGuildEvent.Create(base.Discord, Guild, model.ScheduledEvent.Value);
		}
	}

	public async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetInviteAsync(Code, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return InviteHelper.DeleteAsync(this, base.Discord, options);
	}

	public override string ToString()
	{
		return Url;
	}
}
