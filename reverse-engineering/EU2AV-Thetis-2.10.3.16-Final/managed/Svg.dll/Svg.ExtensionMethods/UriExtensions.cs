using System;

namespace Svg.ExtensionMethods;

public static class UriExtensions
{
	public static Uri ReplaceWithNullIfNone(this Uri uri)
	{
		if (!string.Equals(uri?.ToString().Trim(), "none", StringComparison.OrdinalIgnoreCase))
		{
			return uri;
		}
		return null;
	}
}
