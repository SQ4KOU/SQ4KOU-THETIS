using System;
using Discord.API;

namespace Discord.Rest;

public class RestEntitlement : RestEntity<ulong>, IEntitlement, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public ulong SkuId { get; private set; }

	public ulong? UserId { get; private set; }

	public ulong? GuildId { get; private set; }

	public ulong ApplicationId { get; private set; }

	public EntitlementType Type { get; private set; }

	public bool IsConsumed { get; private set; }

	public DateTimeOffset? StartsAt { get; private set; }

	public DateTimeOffset? EndsAt { get; private set; }

	internal RestEntitlement(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestEntitlement Create(BaseDiscordClient discord, Entitlement model)
	{
		RestEntitlement restEntitlement = new RestEntitlement(discord, model.Id);
		restEntitlement.Update(model);
		return restEntitlement;
	}

	internal void Update(Entitlement model)
	{
		SkuId = model.SkuId;
		UserId = (model.UserId.IsSpecified ? new ulong?(model.UserId.Value) : ((ulong?)null));
		GuildId = (model.GuildId.IsSpecified ? new ulong?(model.GuildId.Value) : ((ulong?)null));
		ApplicationId = model.ApplicationId;
		Type = model.Type;
		IsConsumed = model.IsConsumed.GetValueOrDefault(defaultValue: false);
		StartsAt = model.StartsAt;
		EndsAt = (model.EndsAt.IsSpecified ? model.EndsAt.Value : ((DateTimeOffset?)null));
	}
}
