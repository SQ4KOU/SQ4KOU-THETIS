using System;
using System.Globalization;
using System.Resources;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public sealed class LocalizableResourceString : LocalizableString
{
	private readonly string _nameOfLocalizableResource;

	private readonly ResourceManager _resourceManager;

	private readonly Type _resourceSource;

	private readonly string[] _formatArguments;

	public LocalizableResourceString(string nameOfLocalizableResource, ResourceManager resourceManager, Type resourceSource)
		: this(nameOfLocalizableResource, resourceManager, resourceSource, Array.Empty<string>())
	{
	}

	public LocalizableResourceString(string nameOfLocalizableResource, ResourceManager resourceManager, Type resourceSource, params string[] formatArguments)
	{
		if (nameOfLocalizableResource == null)
		{
			throw new ArgumentNullException("nameOfLocalizableResource");
		}
		if (resourceManager == null)
		{
			throw new ArgumentNullException("resourceManager");
		}
		if (resourceSource == null)
		{
			throw new ArgumentNullException("resourceSource");
		}
		if (formatArguments == null)
		{
			throw new ArgumentNullException("formatArguments");
		}
		_resourceManager = resourceManager;
		_nameOfLocalizableResource = nameOfLocalizableResource;
		_resourceSource = resourceSource;
		_formatArguments = formatArguments;
	}

	protected override string GetText(IFormatProvider? formatProvider)
	{
		CultureInfo culture = (formatProvider as CultureInfo) ?? CultureInfo.CurrentUICulture;
		string text = _resourceManager.GetString(_nameOfLocalizableResource, culture);
		if (text == null)
		{
			return string.Empty;
		}
		if (_formatArguments.Length == 0)
		{
			return text;
		}
		object[] formatArguments = _formatArguments;
		return string.Format(text, formatArguments);
	}

	protected override bool AreEqual(object? other)
	{
		if (other is LocalizableResourceString localizableResourceString && _nameOfLocalizableResource == localizableResourceString._nameOfLocalizableResource && _resourceManager == localizableResourceString._resourceManager && _resourceSource == localizableResourceString._resourceSource)
		{
			return _formatArguments.SequenceEqual(localizableResourceString._formatArguments, (string a, string b) => a == b);
		}
		return false;
	}

	protected override int GetHash()
	{
		return Hash.Combine(_nameOfLocalizableResource.GetHashCode(), Hash.Combine(_resourceManager.GetHashCode(), Hash.Combine(_resourceSource.GetHashCode(), Hash.CombineValues(_formatArguments))));
	}
}
