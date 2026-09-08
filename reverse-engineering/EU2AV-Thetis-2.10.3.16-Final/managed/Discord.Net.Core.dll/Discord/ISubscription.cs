using System;
using System.Collections.Generic;

namespace Discord;

public interface ISubscription : ISnowflakeEntity, IEntity<ulong>
{
	ulong UserId { get; }

	IReadOnlyCollection<ulong> SKUIds { get; }

	IReadOnlyCollection<ulong> EntitlementIds { get; }

	IReadOnlyCollection<ulong> RenewalSKUIds { get; }

	DateTimeOffset CurrentPeriodStart { get; }

	DateTimeOffset CurrentPeriodEnd { get; }

	SubscriptionStatus Status { get; }

	DateTimeOffset? CanceledAt { get; }

	string Country { get; }
}
