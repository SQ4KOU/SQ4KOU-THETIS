using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

public class RoleConnectionMetadata
{
	public RoleConnectionMetadataType Type { get; }

	public string Key { get; }

	public string Name { get; }

	public string Description { get; }

	public IReadOnlyDictionary<string, string> NameLocalizations { get; }

	public IReadOnlyDictionary<string, string> DescriptionLocalizations { get; }

	internal RoleConnectionMetadata(RoleConnectionMetadataType type, string key, string name, string description, IDictionary<string, string> nameLocalizations = null, IDictionary<string, string> descriptionLocalizations = null)
	{
		Type = type;
		Key = key;
		Name = name;
		Description = description;
		NameLocalizations = nameLocalizations?.ToImmutableDictionary();
		DescriptionLocalizations = descriptionLocalizations?.ToImmutableDictionary();
	}

	public RoleConnectionMetadataProperties ToRoleConnectionMetadataProperties()
	{
		return new RoleConnectionMetadataProperties
		{
			Name = Name,
			Description = Description,
			Type = Type,
			Key = Key,
			NameLocalizations = NameLocalizations,
			DescriptionLocalizations = DescriptionLocalizations
		};
	}
}
