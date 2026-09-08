using System;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketEntitlement : SocketEntity<ulong>, IEntitlement, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt { get; private set; }

	public ulong SkuId { get; private set; }

	public Cacheable<SocketUser, RestUser, IUser, ulong>? User { get; private set; }

	public Cacheable<SocketGuild, RestGuild, IGuild, ulong>? Guild { get; private set; }

	public ulong ApplicationId { get; private set; }

	public EntitlementType Type { get; private set; }

	public bool IsConsumed { get; private set; }

	public DateTimeOffset? StartsAt { get; private set; }

	public DateTimeOffset? EndsAt { get; private set; }

	ulong? IEntitlement.GuildId => Guild?.Id;

	ulong? IEntitlement.UserId => User?.Id;

	internal SocketEntitlement(DiscordSocketClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static SocketEntitlement Create(DiscordSocketClient discord, Entitlement model)
	{
		SocketEntitlement socketEntitlement = new SocketEntitlement(discord, model.Id);
		socketEntitlement.Update(model);
		return socketEntitlement;
	}

	internal void Update(Entitlement model)
	{
		SkuId = model.SkuId;
		if (model.UserId.IsSpecified)
		{
			SocketUser user = base.Discord.GetUser(model.UserId.Value);
			User = new Cacheable<SocketUser, RestUser, IUser, ulong>(user, model.UserId.Value, user != null, async () => await base.Discord.Rest.GetUserAsync(model.UserId.Value));
		}
		if (model.GuildId.IsSpecified)
		{
			SocketGuild guild = base.Discord.GetGuild(model.GuildId.Value);
			Guild = new Cacheable<SocketGuild, RestGuild, IGuild, ulong>(guild, model.GuildId.Value, guild != null, async () => await base.Discord.Rest.GetGuildAsync(model.GuildId.Value));
		}
		ApplicationId = model.ApplicationId;
		Type = model.Type;
		IsConsumed = model.IsConsumed.GetValueOrDefault(defaultValue: false);
		StartsAt = model.StartsAt;
		EndsAt = (model.EndsAt.IsSpecified ? model.EndsAt.Value : ((DateTimeOffset?)null));
	}

	internal SocketEntitlement Clone()
	{
		return MemberwiseClone() as SocketEntitlement;
	}
}
