using System;

namespace Discord;

public interface IEntitlement : ISnowflakeEntity, IEntity<ulong>
{
	ulong SkuId { get; }

	ulong? UserId { get; }

	ulong? GuildId { get; }

	ulong ApplicationId { get; }

	EntitlementType Type { get; }

	bool IsConsumed { get; }

	DateTimeOffset? StartsAt { get; }

	DateTimeOffset? EndsAt { get; }
}
