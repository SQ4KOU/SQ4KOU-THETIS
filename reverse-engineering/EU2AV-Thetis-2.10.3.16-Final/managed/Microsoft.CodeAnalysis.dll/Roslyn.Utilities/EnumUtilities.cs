using System;

namespace Roslyn.Utilities;

internal static class EnumUtilities
{
	internal static T[] GetValues<T>() where T : struct
	{
		return (T[])Enum.GetValues(typeof(T));
	}
}
