namespace System;

internal static class IndexOfHelpers
{
	public static bool ContainsAnyExcept(this ReadOnlySpan<char> span, char value0, char value1, char value2)
	{
		for (int i = 0; i < span.Length; i++)
		{
			char c = span[i];
			if (c != value0 && c != value1 && c != value2)
			{
				return true;
			}
		}
		return false;
	}

	public static int IndexOfAny(this ReadOnlySpan<char> span, string values)
	{
		for (int i = 0; i < span.Length; i++)
		{
			char c = span[i];
			foreach (char c2 in values)
			{
				if (c == c2)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public static bool Contains<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
	{
		return span.IndexOf<T>(value) >= 0;
	}
}
