using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.API;

namespace Discord.WebSocket;

public class SocketSubscription : SocketEntity<ulong>, ISubscription, ISnowflakeEntity, IEntity<ulong>
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

	internal SocketSubscription(DiscordSocketClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static SocketSubscription Create(DiscordSocketClient discord, Subscription model)
	{
		SocketSubscription socketSubscription = new SocketSubscription(discord, model.Id);
		socketSubscription.Update(model);
		return socketSubscription;
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

	internal SocketSubscription Clone()
	{
		return MemberwiseClone() as SocketSubscription;
	}
}
