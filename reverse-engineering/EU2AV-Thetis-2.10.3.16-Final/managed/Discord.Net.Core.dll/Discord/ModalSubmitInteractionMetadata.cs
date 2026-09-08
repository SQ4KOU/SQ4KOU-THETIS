using System;
using System.Collections.Generic;

namespace Discord;

public readonly struct ModalSubmitInteractionMetadata : IMessageInteractionMetadata, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	public ulong Id { get; }

	public InteractionType Type { get; }

	public ulong UserId { get; }

	public IUser User { get; }

	public IReadOnlyDictionary<ApplicationIntegrationType, ulong> IntegrationOwners { get; }

	public ulong? OriginalResponseMessageId { get; }

	public IMessageInteractionMetadata TriggeringInteractionMetadata { get; }

	internal ModalSubmitInteractionMetadata(ulong id, InteractionType type, ulong userId, IReadOnlyDictionary<ApplicationIntegrationType, ulong> integrationOwners, ulong? originalResponseMessageId, IMessageInteractionMetadata triggeringInteractionMetadata, IUser user)
	{
		Id = id;
		Type = type;
		UserId = userId;
		IntegrationOwners = integrationOwners;
		OriginalResponseMessageId = originalResponseMessageId;
		TriggeringInteractionMetadata = triggeringInteractionMetadata;
		User = user;
	}
}
