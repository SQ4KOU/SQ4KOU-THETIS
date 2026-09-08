using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Thetis;

public static class StringExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Truncate(this string source, int maxLength)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source.Length <= maxLength)
		{
			return source;
		}
		return source.Substring(0, maxLength);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Contains(this string source, string toCheck, StringComparison comp)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (toCheck == null)
		{
			throw new ArgumentNullException("toCheck");
		}
		if (source == null)
		{
			return false;
		}
		return source.IndexOf(toCheck, comp) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Left(this string source, int length)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length cannot be negative.");
		}
		if (source.Length <= length)
		{
			return source;
		}
		return source.Substring(0, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Right(this string source, int length)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length cannot be negative.");
		}
		if (length < source.Length)
		{
			return source.Substring(source.Length - length);
		}
		return source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ReplaceIgnoreTokenCase(this string source, string token, string replacement)
	{
		if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(token))
		{
			return source;
		}
		int num = 0;
		int num2 = source.IndexOf(token, num, StringComparison.OrdinalIgnoreCase);
		if (num2 < 0)
		{
			return source;
		}
		StringBuilder stringBuilder = new StringBuilder(source.Length + Math.Max(0, replacement.Length - token.Length) * 4);
		while (num2 >= 0)
		{
			stringBuilder.Append(source, num, num2 - num);
			stringBuilder.Append(replacement);
			num = num2 + token.Length;
			num2 = source.IndexOf(token, num, StringComparison.OrdinalIgnoreCase);
		}
		stringBuilder.Append(source, num, source.Length - num);
		return stringBuilder.ToString();
	}
}
