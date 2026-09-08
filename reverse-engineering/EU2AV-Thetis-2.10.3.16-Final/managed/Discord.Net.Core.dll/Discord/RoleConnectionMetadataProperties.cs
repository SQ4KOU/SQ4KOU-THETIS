using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

public class RoleConnectionMetadataProperties
{
	private const int MaxKeyLength = 50;

	private const int MaxNameLength = 100;

	private const int MaxDescriptionLength = 200;

	private string _key;

	private string _name;

	private string _description;

	private IReadOnlyDictionary<string, string> _nameLocalizations;

	private IReadOnlyDictionary<string, string> _descriptionLocalizations;

	public RoleConnectionMetadataType Type { get; set; }

	public string Key
	{
		get
		{
			return _key;
		}
		set
		{
			Preconditions.AtMost(value.Length, 50, "Key", $"Key length must be less than or equal to {50}");
			_key = value;
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			Preconditions.AtMost(value.Length, 100, "Name", $"Name length must be less than or equal to {100}");
			_name = value;
		}
	}

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			Preconditions.AtMost(value.Length, 200, "Description", $"Description length must be less than or equal to {200}");
			_description = value;
		}
	}

	public IReadOnlyDictionary<string, string> NameLocalizations
	{
		get
		{
			return _nameLocalizations;
		}
		set
		{
			if (value != null)
			{
				foreach (KeyValuePair<string, string> item in value)
				{
					if (item.Value.Length > 100)
					{
						throw new ArgumentException($"Name localization length must be less than or equal to {100}. Locale '{item}'");
					}
				}
			}
			_nameLocalizations = value;
		}
	}

	public IReadOnlyDictionary<string, string> DescriptionLocalizations
	{
		get
		{
			return _descriptionLocalizations;
		}
		set
		{
			if (value != null)
			{
				foreach (KeyValuePair<string, string> item in value)
				{
					if (item.Value.Length > 200)
					{
						throw new ArgumentException($"Description localization length must be less than or equal to {200}. Locale '{item}'");
					}
				}
			}
			_descriptionLocalizations = value;
		}
	}

	public RoleConnectionMetadataProperties(RoleConnectionMetadataType type, string key, string name, string description, IDictionary<string, string> nameLocalizations = null, IDictionary<string, string> descriptionLocalizations = null)
	{
		Type = type;
		Key = key;
		Name = name;
		Description = description;
		NameLocalizations = nameLocalizations?.ToImmutableDictionary();
		DescriptionLocalizations = descriptionLocalizations?.ToImmutableDictionary();
	}

	public RoleConnectionMetadataProperties()
	{
	}

	public static RoleConnectionMetadataProperties FromRoleConnectionMetadata(RoleConnectionMetadata metadata)
	{
		return new RoleConnectionMetadataProperties
		{
			Name = metadata.Name,
			Description = metadata.Description,
			Type = metadata.Type,
			Key = metadata.Key,
			NameLocalizations = metadata.NameLocalizations,
			DescriptionLocalizations = metadata.DescriptionLocalizations
		};
	}
}
