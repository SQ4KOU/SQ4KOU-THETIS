using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class RestUserGuild : RestEntity<ulong>, IUserGuild, IDeletable, ISnowflakeEntity, IEntity<ulong>
{
	private string _iconId;

	public string Name { get; private set; }

	public bool IsOwner { get; private set; }

	public GuildPermissions Permissions { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public string IconUrl => CDN.GetGuildIconUrl(base.Id, _iconId, 2048);

	public GuildFeatures Features { get; private set; }

	public int? ApproximateMemberCount { get; private set; }

	public int? ApproximatePresenceCount { get; private set; }

	private string DebuggerDisplay => string.Format("{0} ({1}{2})", Name, base.Id, IsOwner ? ", Owned" : "");

	internal RestUserGuild(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestUserGuild Create(BaseDiscordClient discord, UserGuild model)
	{
		RestUserGuild restUserGuild = new RestUserGuild(discord, model.Id);
		restUserGuild.Update(model);
		return restUserGuild;
	}

	internal void Update(UserGuild model)
	{
		_iconId = model.Icon;
		IsOwner = model.Owner;
		Name = model.Name;
		Permissions = new GuildPermissions(model.Permissions);
		Features = model.Features;
		ApproximateMemberCount = (model.ApproximateMemberCount.IsSpecified ? new int?(model.ApproximateMemberCount.Value) : ((int?)null));
		ApproximatePresenceCount = (model.ApproximatePresenceCount.IsSpecified ? new int?(model.ApproximatePresenceCount.Value) : ((int?)null));
	}

	public Task LeaveAsync(RequestOptions options = null)
	{
		return base.Discord.ApiClient.LeaveGuildAsync(base.Id, options);
	}

	public async Task<RestGuildUser> GetCurrentUserGuildMemberAsync(RequestOptions options = null)
	{
		GuildMember model = await base.Discord.ApiClient.GetCurrentUserGuildMember(base.Id, options);
		return RestGuildUser.Create(base.Discord, null, model, base.Id);
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return base.Discord.ApiClient.DeleteGuildAsync(base.Id, options);
	}

	public override string ToString()
	{
		return Name;
	}
}
