using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Discord;

public class MessageCommandBuilder
{
	public const int MaxNameLength = 32;

	private string _name;

	private Dictionary<string, string> _nameLocalizations;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			Preconditions.NotNullOrEmpty(value, "Name");
			Preconditions.AtLeast(value.Length, 1, "Name");
			Preconditions.AtMost(value.Length, 32, "Name");
			_name = value;
		}
	}

	public bool IsDefaultPermission { get; set; } = true;

	public IReadOnlyDictionary<string, string> NameLocalizations => _nameLocalizations;

	[Obsolete("This property will be deprecated soon. Configure with ContextTypes instead.")]
	public bool IsDMEnabled { get; set; } = true;

	public bool IsNsfw { get; set; }

	public GuildPermission? DefaultMemberPermissions { get; set; }

	public HashSet<ApplicationIntegrationType> IntegrationTypes { get; set; }

	public HashSet<InteractionContextType> ContextTypes { get; set; }

	public MessageCommandProperties Build()
	{
		return new MessageCommandProperties
		{
			Name = Name,
			IsDefaultPermission = IsDefaultPermission,
			IsDMEnabled = IsDMEnabled,
			DefaultMemberPermissions = (((Optional<GuildPermission>?)DefaultMemberPermissions) ?? Optional<GuildPermission>.Unspecified),
			NameLocalizations = NameLocalizations,
			IsNsfw = IsNsfw,
			IntegrationTypes = IntegrationTypes,
			ContextTypes = ContextTypes
		};
	}

	public MessageCommandBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public MessageCommandBuilder WithDefaultPermission(bool isDefaultPermission)
	{
		IsDefaultPermission = isDefaultPermission;
		return this;
	}

	public MessageCommandBuilder WithNameLocalizations(IDictionary<string, string> nameLocalizations)
	{
		if (nameLocalizations == null)
		{
			throw new ArgumentNullException("nameLocalizations");
		}
		foreach (var (text3, name) in nameLocalizations)
		{
			if (!Regex.IsMatch(text3, "^\\w{2}(?:-\\w{2})?$"))
			{
				throw new ArgumentException("Invalid locale: " + text3, "locale");
			}
			EnsureValidCommandName(name);
		}
		_nameLocalizations = new Dictionary<string, string>(nameLocalizations);
		return this;
	}

	[Obsolete("This method will be deprecated soon. Configure with WithContextTypes instead.")]
	public MessageCommandBuilder WithDMPermission(bool permission)
	{
		IsDMEnabled = permission;
		return this;
	}

	public MessageCommandBuilder WithNsfw(bool permission)
	{
		IsNsfw = permission;
		return this;
	}

	public MessageCommandBuilder AddNameLocalization(string locale, string name)
	{
		if (!Regex.IsMatch(locale, "^\\w{2}(?:-\\w{2})?$"))
		{
			throw new ArgumentException("Invalid locale: " + locale, "locale");
		}
		EnsureValidCommandName(name);
		if (_nameLocalizations == null)
		{
			_nameLocalizations = new Dictionary<string, string>();
		}
		_nameLocalizations.Add(locale, name);
		return this;
	}

	private static void EnsureValidCommandName(string name)
	{
		Preconditions.NotNullOrEmpty(name, "name");
		Preconditions.AtLeast(name.Length, 1, "name");
		Preconditions.AtMost(name.Length, 32, "name");
	}

	public MessageCommandBuilder WithDefaultMemberPermissions(GuildPermission? permissions)
	{
		DefaultMemberPermissions = permissions;
		return this;
	}

	public MessageCommandBuilder WithIntegrationTypes(params ApplicationIntegrationType[] integrationTypes)
	{
		IntegrationTypes = ((integrationTypes != null) ? new HashSet<ApplicationIntegrationType>(integrationTypes) : null);
		return this;
	}

	public MessageCommandBuilder WithContextTypes(params InteractionContextType[] contextTypes)
	{
		ContextTypes = ((contextTypes != null) ? new HashSet<InteractionContextType>(contextTypes) : null);
		return this;
	}
}
