using System;
using System.Collections.Generic;

namespace Discord;

public readonly struct ApplicationCommandInteractionMetadata : IMessageInteractionMetadata, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	public ulong Id { get; }

	public InteractionType Type { get; }

	public ulong UserId { get; }

	public IUser User { get; }

	public IReadOnlyDictionary<ApplicationIntegrationType, ulong> IntegrationOwners { get; }

	public ulong? OriginalResponseMessageId { get; }

	public string Name { get; }

	internal ApplicationCommandInteractionMetadata(ulong id, InteractionType type, ulong userId, IReadOnlyDictionary<ApplicationIntegrationType, ulong> integrationOwners, ulong? originalResponseMessageId, string name, IUser user)
	{
		Id = id;
		Type = type;
		UserId = userId;
		IntegrationOwners = integrationOwners;
		OriginalResponseMessageId = originalResponseMessageId;
		Name = name;
		User = user;
	}
}
