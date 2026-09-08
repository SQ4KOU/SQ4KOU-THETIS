using System;

namespace Discord;

public struct SKU : ISnowflakeEntity, IEntity<ulong>
{
	public ulong Id { get; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	public SKUType Type { get; }

	public ulong ApplicationId { get; }

	public string Name { get; }

	public string Slug { get; }

	public SKUFlags Flags { get; }

	internal SKU(ulong id, SKUType type, ulong applicationId, string name, string slug, SKUFlags flags)
	{
		Id = id;
		Type = type;
		ApplicationId = applicationId;
		Name = name;
		Slug = slug;
		Flags = flags;
	}
}
