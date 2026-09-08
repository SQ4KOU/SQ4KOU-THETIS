using System.Runtime.CompilerServices;

namespace System;

internal static class SpanExtensions
{
	public static bool StartsWith(this ReadOnlySpan<char> span, string prefix, StringComparison comparisonType)
	{
		if (span.Length >= prefix.Length)
		{
			return span.Slice(0, prefix.Length).Equals(prefix.AsSpan(), comparisonType);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool StartsWith(this ReadOnlySpan<char> span, char c)
	{
		if (span.Length > 0)
		{
			return span[0] == c;
		}
		return false;
	}
}
