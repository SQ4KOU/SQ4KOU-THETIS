using System;

namespace Svg;

public static class ExternalTypeExtensions
{
	public static bool AllowsResolving(this ExternalType externalType, Uri uri)
	{
		if (uri.IsAbsoluteUri)
		{
			if (!externalType.HasFlag(ExternalType.Local) || !uri.IsFile)
			{
				if (externalType.HasFlag(ExternalType.Remote))
				{
					return !uri.IsFile;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
