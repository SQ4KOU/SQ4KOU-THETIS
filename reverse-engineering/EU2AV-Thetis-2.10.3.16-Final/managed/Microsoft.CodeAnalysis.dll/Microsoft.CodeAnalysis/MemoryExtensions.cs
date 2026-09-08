using System;

namespace Microsoft.CodeAnalysis;

internal static class MemoryExtensions
{
	public static int IndexOf(this ReadOnlySpan<char> span, char target, int startIndex)
	{
		for (int i = startIndex; i < span.Length; i++)
		{
			if (span[i] == target)
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOfAny(this ReadOnlySpan<char> span, char[] characters)
	{
		for (int i = 0; i < span.Length; i++)
		{
			char c = span[i];
			foreach (char c2 in characters)
			{
				if (c == c2)
				{
					return i;
				}
			}
		}
		return -1;
	}

	internal static ReadOnlyMemory<char> TrimStart(this ReadOnlyMemory<char> memory)
	{
		ReadOnlySpan<char> span = memory.Span;
		int i;
		for (i = 0; i < span.Length && char.IsWhiteSpace(span[i]); i++)
		{
		}
		return memory.Slice(i, span.Length - i);
	}

	internal static ReadOnlyMemory<char> TrimEnd(this ReadOnlyMemory<char> memory)
	{
		ReadOnlySpan<char> span = memory.Span;
		int num = span.Length;
		while (num - 1 >= 0 && char.IsWhiteSpace(span[num - 1]))
		{
			num--;
		}
		return memory.Slice(0, num);
	}

	internal static ReadOnlyMemory<char> Trim(this ReadOnlyMemory<char> memory)
	{
		return memory.TrimStart().TrimEnd();
	}

	internal static bool IsNullOrEmpty(this ReadOnlyMemory<char>? memory)
	{
		return !memory.HasValue || memory.GetValueOrDefault().Length <= 0;
	}

	internal static bool IsNullOrWhiteSpace(this ReadOnlyMemory<char>? memory)
	{
		if (memory.HasValue)
		{
			ReadOnlyMemory<char> valueOrDefault = memory.GetValueOrDefault();
			return valueOrDefault.IsWhiteSpace();
		}
		return true;
	}

	internal static bool IsWhiteSpace(this ReadOnlyMemory<char> memory)
	{
		ReadOnlySpan<char> span = memory.Span;
		for (int i = 0; i < span.Length; i++)
		{
			if (!char.IsWhiteSpace(span[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal static bool StartsWith(this ReadOnlyMemory<char> memory, char c)
	{
		if (memory.Length > 0)
		{
			return memory.Span[0] == c;
		}
		return false;
	}

	internal static ReadOnlyMemory<char> Unquote(this ReadOnlyMemory<char> memory)
	{
		ReadOnlySpan<char> span = memory.Span;
		if (span.Length > 1 && span[0] == '"' && span[span.Length - 1] == '"')
		{
			return memory.Slice(1, memory.Length - 2);
		}
		return memory;
	}
}
