using System.Collections.Generic;

namespace Discord;

public interface IMessageInteractionMetadata : ISnowflakeEntity, IEntity<ulong>
{
	InteractionType Type { get; }

	ulong UserId { get; }

	IUser User { get; }

	IReadOnlyDictionary<ApplicationIntegrationType, ulong> IntegrationOwners { get; }

	ulong? OriginalResponseMessageId { get; }
}
