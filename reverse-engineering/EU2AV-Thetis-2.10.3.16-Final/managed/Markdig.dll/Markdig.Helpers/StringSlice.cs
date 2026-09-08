using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Markdig.Helpers;

public struct StringSlice : ICharIterator
{
	public static readonly StringSlice Empty = new StringSlice(string.Empty);

	public readonly string Text;

	public NewLine NewLine;

	public int Start { get; set; }

	public int End { get; set; }

	public readonly int Length => End - Start + 1;

	public readonly char CurrentChar
	{
		get
		{
			int start = Start;
			if (start > End)
			{
				return '\0';
			}
			return Text[start];
		}
	}

	internal readonly Rune CurrentRune
	{
		get
		{
			int start = Start;
			if (start > End)
			{
				return default(Rune);
			}
			char c = Text[start];
			if (!Rune.TryCreate(c, out var result) && start + 1 <= End)
			{
				Rune.TryCreate(c, Text[start + 1], out result);
			}
			return result;
		}
	}

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Start > End;
		}
	}

	public readonly char this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Text[index];
		}
	}

	public StringSlice(string text)
	{
		Text = text;
		Start = 0;
		End = (Text?.Length ?? 0) - 1;
		NewLine = NewLine.None;
	}

	public StringSlice(string text, NewLine newLine)
	{
		Text = text;
		Start = 0;
		End = (Text?.Length ?? 0) - 1;
		NewLine = newLine;
	}

	public StringSlice(string text, int start, int end)
	{
		if (text == null)
		{
			ThrowHelper.ArgumentNullException_text();
		}
		Text = text;
		Start = start;
		End = end;
		NewLine = NewLine.None;
	}

	public StringSlice(string text, int start, int end, NewLine newLine)
	{
		if (text == null)
		{
			ThrowHelper.ArgumentNullException_text();
		}
		Text = text;
		Start = start;
		End = end;
		NewLine = newLine;
	}

	internal StringSlice(string text, int start, int end, NewLine newLine, bool dummy)
	{
		Text = text;
		Start = start;
		End = end;
		NewLine = newLine;
	}

	internal readonly Rune RuneAt(int index)
	{
		string text = Text;
		char c = text[index];
		if (!Rune.TryCreate(c, out var result) && (uint)(index + 1) < (uint)text.Length)
		{
			Rune.TryCreate(c, text[index + 1], out result);
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public char NextChar()
	{
		int start = Start;
		if (start >= End)
		{
			Start = End + 1;
			return '\0';
		}
		start = (Start = start + 1);
		return Text[start];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Rune NextRune()
	{
		int num = Start;
		if (num >= End)
		{
			Start = End + 1;
			return default(Rune);
		}
		if (char.IsHighSurrogate(Text[num++]) && num <= End && char.IsLowSurrogate(Text[num]))
		{
			num++;
		}
		Start = num;
		char c = Text[num];
		if (!Rune.TryCreate(c, out var result) && num + 1 <= End)
		{
			Rune.TryCreate(c, Text[num + 1], out result);
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SkipChar()
	{
		int start = Start;
		if (start <= End)
		{
			Start = start + 1;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int CountAndSkipChar(char matchChar)
	{
		string text = Text;
		int end = End;
		int i;
		for (i = Start; i <= end && (uint)i < (uint)text.Length && text[i] == matchChar; i++)
		{
		}
		int result = i - Start;
		Start = i;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly char PeekChar()
	{
		int num = Start + 1;
		if (num > End)
		{
			return '\0';
		}
		return Text[num];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly char PeekChar(int offset)
	{
		int num = Start + offset;
		if (num < Start || num > End)
		{
			return '\0';
		}
		return Text[num];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly char PeekCharAbsolute(int index)
	{
		string text = Text;
		if ((uint)index >= (uint)text.Length)
		{
			return '\0';
		}
		return text[index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly char PeekCharExtra(int offset)
	{
		int num = Start + offset;
		string text = Text;
		if ((uint)num >= (uint)text.Length)
		{
			return '\0';
		}
		return text[num];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal readonly Rune PeekRuneExtra(int offset)
	{
		int num = Start + offset;
		string text = Text;
		if ((uint)num >= (uint)text.Length)
		{
			return default(Rune);
		}
		char c = text[num];
		if (Rune.TryCreate(c, out var result))
		{
			return result;
		}
		if (offset < 0)
		{
			if ((uint)(num - 1) < (uint)text.Length)
			{
				Rune.TryCreate(text[num - 1], c, out result);
			}
		}
		else if ((uint)(num + 1) < (uint)text.Length)
		{
			Rune.TryCreate(c, text[num + 1], out result);
		}
		return result;
	}

	public readonly bool Match(string text, int offset = 0)
	{
		return Match(text, End, offset);
	}

	public readonly bool Match(string text, int end, int offset)
	{
		int num = Start + offset;
		if (end - num + 1 < text.Length)
		{
			return false;
		}
		string text2 = Text;
		int num2 = 0;
		while (num2 < text.Length)
		{
			if (text[num2] != text2[num])
			{
				return false;
			}
			num2++;
			num++;
		}
		return true;
	}

	public bool SkipSpacesToEndOfLineOrEndOfDocument()
	{
		for (int i = Start; i <= End; i++)
		{
			char c = Text[i];
			if (c.IsWhitespace())
			{
				if (c == '\n' || (c == '\r' && i + 1 <= End && Text[i + 1] != '\n'))
				{
					return true;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	public readonly bool MatchLowercase(string text, int offset = 0)
	{
		return MatchLowercase(text, End, offset);
	}

	public readonly bool MatchLowercase(string text, int end, int offset)
	{
		int num = Start + offset;
		if (end - num + 1 < text.Length)
		{
			return false;
		}
		string text2 = Text;
		int num2 = 0;
		while (num2 < text.Length)
		{
			if (text[num2] != char.ToLowerInvariant(text2[num]))
			{
				return false;
			}
			num2++;
			num++;
		}
		return true;
	}

	public readonly int IndexOf(string text, int offset = 0, bool ignoreCase = false)
	{
		offset += Start;
		int num = End - offset + 1;
		if (num <= 0)
		{
			return -1;
		}
		return Text.IndexOf(text, offset, num, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	public readonly int IndexOf(char c)
	{
		int start = Start;
		int num = End - start + 1;
		if (num <= 0)
		{
			return -1;
		}
		return Text.IndexOf(c, start, num);
	}

	public bool TrimStart()
	{
		string text = Text;
		int end = End;
		int i;
		for (i = Start; i <= end && (uint)i < (uint)text.Length && text[i].IsWhitespace(); i++)
		{
		}
		Start = i;
		return i > end;
	}

	public void TrimStart(out int spaceCount)
	{
		string text = Text;
		int end = End;
		int i;
		for (i = Start; i <= end && (uint)i < (uint)text.Length && text[i].IsWhitespace(); i++)
		{
		}
		spaceCount = i - Start;
		Start = i;
	}

	public bool TrimEnd()
	{
		string text = Text;
		int start = Start;
		int num = End;
		while (start <= num && (uint)num < (uint)text.Length && text[num].IsWhitespace())
		{
			num--;
		}
		End = num;
		return start > num;
	}

	public void Trim()
	{
		string text = Text;
		int i = Start;
		int num;
		for (num = End; i <= num && (uint)i < (uint)text.Length && text[i].IsWhitespace(); i++)
		{
		}
		while (i <= num && (uint)num < (uint)text.Length && text[num].IsWhitespace())
		{
			num--;
		}
		Start = i;
		End = num;
	}

	public override readonly string ToString()
	{
		string text = Text;
		int start = Start;
		int num = End - start + 1;
		if (text == null || num <= 0)
		{
			return string.Empty;
		}
		return text.Substring(start, num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<char> AsSpan()
	{
		string text = Text;
		int start = Start;
		int num = End - start + 1;
		if (text == null || (ulong)((long)(uint)start + (long)(uint)num) > (ulong)(uint)text.Length)
		{
			return default(ReadOnlySpan<char>);
		}
		return text.AsSpan(start, num);
	}

	public readonly bool IsEmptyOrWhitespace()
	{
		string text = Text;
		int end = End;
		for (int i = Start; i <= end && (uint)i < (uint)text.Length; i++)
		{
			if (!text[i].IsWhitespace())
			{
				return false;
			}
		}
		return true;
	}

	public bool Overlaps(StringSlice other)
	{
		if (IsEmpty || other.IsEmpty)
		{
			return false;
		}
		if (Start <= other.End)
		{
			return End >= other.Start;
		}
		return false;
	}
}
