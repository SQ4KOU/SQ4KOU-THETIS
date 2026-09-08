using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.API;

namespace Discord.Rest;

public class RestSubscription : RestEntity<ulong>, ISubscription, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public ulong UserId { get; private set; }

	public IReadOnlyCollection<ulong> SKUIds { get; private set; }

	public IReadOnlyCollection<ulong> EntitlementIds { get; private set; }

	public IReadOnlyCollection<ulong> RenewalSKUIds { get; private set; }

	public DateTimeOffset CurrentPeriodStart { get; private set; }

	public DateTimeOffset CurrentPeriodEnd { get; private set; }

	public SubscriptionStatus Status { get; private set; }

	public DateTimeOffset? CanceledAt { get; private set; }

	public string Country { get; private set; }

	internal RestSubscription(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestSubscription Create(BaseDiscordClient discord, Subscription model)
	{
		RestSubscription restSubscription = new RestSubscription(discord, model.Id);
		restSubscription.Update(model);
		return restSubscription;
	}

	internal void Update(Subscription model)
	{
		UserId = model.UserId;
		SKUIds = ((IEnumerable<ulong>)model.SKUIds).ToImmutableArray();
		EntitlementIds = ((IEnumerable<ulong>)model.EntitlementIds).ToImmutableArray();
		RenewalSKUIds = model.RenewalSKUIds?.ToImmutableArray() ?? ImmutableArray<ulong>.Empty;
		CurrentPeriodStart = model.CurrentPeriodStart;
		CurrentPeriodEnd = model.CurrentPeriodEnd;
		Status = model.Status;
		CanceledAt = model.CanceledAt;
		Country = model.Country;
	}
}
