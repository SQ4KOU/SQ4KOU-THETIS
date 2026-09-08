using System;
using System.Collections.Generic;

namespace Discord;

public readonly struct MessageComponentInteractionMetadata : IMessageInteractionMetadata, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	public ulong Id { get; }

	public InteractionType Type { get; }

	public ulong UserId { get; }

	public IUser User { get; }

	public IReadOnlyDictionary<ApplicationIntegrationType, ulong> IntegrationOwners { get; }

	public ulong? OriginalResponseMessageId { get; }

	public ulong InteractedMessageId { get; }

	internal MessageComponentInteractionMetadata(ulong id, InteractionType type, ulong userId, IReadOnlyDictionary<ApplicationIntegrationType, ulong> integrationOwners, ulong? originalResponseMessageId, ulong interactedMessageId, IUser user)
	{
		Id = id;
		Type = type;
		UserId = userId;
		IntegrationOwners = integrationOwners;
		OriginalResponseMessageId = originalResponseMessageId;
		InteractedMessageId = interactedMessageId;
		User = user;
	}
}
