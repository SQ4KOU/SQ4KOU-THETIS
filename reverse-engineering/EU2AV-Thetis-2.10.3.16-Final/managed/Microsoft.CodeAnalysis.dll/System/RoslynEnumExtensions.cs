namespace System;

internal static class RoslynEnumExtensions
{
	extension(Enum)
	{
		public static TEnum[] GetValues<TEnum>() where TEnum : struct, Enum
		{
			return (TEnum[])Enum.GetValues(typeof(TEnum));
		}

		public static string[] GetNames<TEnum>() where TEnum : struct, Enum
		{
			return Enum.GetNames(typeof(TEnum));
		}
	}
}
