using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis;

internal readonly struct CodeAnalysisResourcesLocalizableErrorArgument : IFormattable
{
	private readonly string _targetResourceId;

	internal CodeAnalysisResourcesLocalizableErrorArgument(string targetResourceId)
	{
		_targetResourceId = targetResourceId;
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		if (_targetResourceId != null)
		{
			return CodeAnalysisResources.ResourceManager.GetString(_targetResourceId, formatProvider as CultureInfo) ?? string.Empty;
		}
		return string.Empty;
	}
}
