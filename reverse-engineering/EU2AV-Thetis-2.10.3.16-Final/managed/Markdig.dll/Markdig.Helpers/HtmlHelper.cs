using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Markdig.Helpers;

public static class HtmlHelper
{
	private static readonly char[] SearchBackAndAmp;

	private static readonly char[] SearchAmp;

	private static readonly string[] EscapeUrlsForAscii;

	static HtmlHelper()
	{
		SearchBackAndAmp = new char[2] { '\\', '&' };
		SearchAmp = new char[1] { '&' };
		EscapeUrlsForAscii = new string[128];
		for (int i = 0; i < EscapeUrlsForAscii.Length; i++)
		{
			if (i <= 32 || "\"'<>[\\]^`{|}~".IndexOf((char)i) >= 0 || i == 127)
			{
				EscapeUrlsForAscii[i] = $"%{i:X2}";
			}
			else if ((ushort)i == 38)
			{
				EscapeUrlsForAscii[i] = "&amp;";
			}
		}
	}

	public static string? EscapeUrlCharacter(char c)
	{
		if (c >= '\u0080')
		{
			return null;
		}
		return EscapeUrlsForAscii[(uint)c];
	}

	public static bool TryParseHtmlTag(ref StringSlice text, [NotNullWhen(true)] out string? htmlTag)
	{
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
		if (TryParseHtmlTag(ref text, ref builder))
		{
			htmlTag = builder.ToString();
			return true;
		}
		builder.Dispose();
		htmlTag = null;
		return false;
	}

	private static bool TryParseHtmlTag(ref StringSlice text, ref ValueStringBuilder builder)
	{
		char currentChar = text.CurrentChar;
		if (currentChar != '<')
		{
			return false;
		}
		currentChar = text.NextChar();
		builder.Append('<');
		switch (currentChar)
		{
		case '/':
			return TryParseHtmlCloseTag(ref text, ref builder);
		case '?':
			return TryParseHtmlTagProcessingInstruction(ref text, ref builder);
		case '!':
			builder.Append(currentChar);
			return text.NextChar() switch
			{
				'-' => TryParseHtmlTagHtmlComment(ref text, ref builder), 
				'[' => TryParseHtmlTagCData(ref text, ref builder), 
				_ => TryParseHtmlTagDeclaration(ref text, ref builder), 
			};
		default:
			return TryParseHtmlTagOpenTag(ref text, ref builder);
		}
	}

	internal static bool TryParseHtmlTagOpenTag(ref StringSlice text, ref ValueStringBuilder builder)
	{
		char currentChar = text.CurrentChar;
		if (!currentChar.IsAlpha())
		{
			return false;
		}
		builder.Append(currentChar);
		while (true)
		{
			currentChar = text.NextChar();
			if (!currentChar.IsAlphaNumeric() && currentChar != '-')
			{
				break;
			}
			builder.Append(currentChar);
		}
		bool flag = false;
		while (true)
		{
			bool flag2 = false;
			while (currentChar.IsWhitespace())
			{
				builder.Append(currentChar);
				currentChar = text.NextChar();
				flag2 = true;
			}
			switch (currentChar)
			{
			case '\0':
				return false;
			case '>':
				text.SkipChar();
				builder.Append(currentChar);
				return true;
			case '/':
				builder.Append('/');
				currentChar = text.NextChar();
				if (currentChar != '>')
				{
					return false;
				}
				text.SkipChar();
				builder.Append('>');
				return true;
			case '=':
				if (!flag)
				{
					return false;
				}
				builder.Append('=');
				currentChar = text.NextChar();
				while (currentChar.IsWhitespace())
				{
					builder.Append(currentChar);
					currentChar = text.NextChar();
				}
				if (currentChar == '\'' || currentChar == '"')
				{
					builder.Append(currentChar);
					char c = currentChar;
					while (true)
					{
						currentChar = text.NextChar();
						if (currentChar == '\0')
						{
							return false;
						}
						if (currentChar == c)
						{
							break;
						}
						builder.Append(currentChar);
					}
					builder.Append(currentChar);
					currentChar = text.NextChar();
				}
				else
				{
					int num = 0;
					while (true)
					{
						if (currentChar == '\0')
						{
							return false;
						}
						if (IsSpaceOrSpecialHtmlChar(currentChar))
						{
							break;
						}
						num++;
						builder.Append(currentChar);
						currentChar = text.NextChar();
					}
					if (num == 0)
					{
						return false;
					}
				}
				flag = false;
				continue;
			}
			if (!flag2)
			{
				return false;
			}
			if (!currentChar.IsAlpha() && currentChar != '_' && currentChar != ':')
			{
				return false;
			}
			builder.Append(currentChar);
			while (true)
			{
				currentChar = text.NextChar();
				if (!currentChar.IsAlphaNumeric() && !IsCharToAppend(currentChar))
				{
					break;
				}
				builder.Append(currentChar);
			}
			flag = true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool IsCharToAppend(char c2)
		{
			if ((uint)(c2 - 45) > 50u)
			{
				return false;
			}
			return (0x400600080000000L & (1L << (int)c2)) != 0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool IsSpaceOrSpecialHtmlChar(char c2)
		{
			if (c2 > '>')
			{
				return c2 == '`';
			}
			return (0x7000008500000400L & (1L << (int)c2)) != 0;
		}
	}

	private static bool TryParseHtmlTagDeclaration(ref StringSlice text, ref ValueStringBuilder builder)
	{
		char c = text.CurrentChar;
		bool flag = false;
		while (c.IsAlphaUpper())
		{
			builder.Append(c);
			c = text.NextChar();
			flag = true;
		}
		if (!flag || !c.IsWhitespace())
		{
			return false;
		}
		while (true)
		{
			builder.Append(c);
			c = text.NextChar();
			switch (c)
			{
			case '\0':
				return false;
			case '>':
				text.SkipChar();
				builder.Append('>');
				return true;
			}
		}
	}

	private static bool TryParseHtmlTagCData(ref StringSlice text, ref ValueStringBuilder builder)
	{
		if (text.Match("[CDATA["))
		{
			builder.Append("[CDATA[");
			text.Start += 6;
			char c = '\0';
			char c2;
			do
			{
				c2 = c;
				c = text.NextChar();
				if (c == '\0')
				{
					return false;
				}
				builder.Append(c);
			}
			while (c != ']' || c2 != ']' || text.PeekChar() != '>');
			text.SkipChar();
			text.SkipChar();
			builder.Append('>');
			return true;
		}
		return false;
	}

	internal static bool TryParseHtmlCloseTag(ref StringSlice text, ref ValueStringBuilder builder)
	{
		builder.Append('/');
		char c = text.NextChar();
		if (!c.IsAlpha())
		{
			return false;
		}
		builder.Append(c);
		bool flag = false;
		while (true)
		{
			c = text.NextChar();
			if (c == '>')
			{
				text.SkipChar();
				builder.Append('>');
				return true;
			}
			if (flag)
			{
				if (c != ' ')
				{
					break;
				}
			}
			else if (c == ' ')
			{
				flag = true;
			}
			else if (!c.IsAlphaNumeric() && c != '-')
			{
				break;
			}
			builder.Append(c);
		}
		return false;
	}

	private static bool TryParseHtmlTagHtmlComment(ref StringSlice text, ref ValueStringBuilder builder)
	{
		char c = text.NextChar();
		if (c != '-')
		{
			return false;
		}
		switch (text.NextChar())
		{
		case '>':
			builder.Append("-->");
			text.SkipChar();
			return true;
		case '-':
			if (text.PeekChar() == '>')
			{
				builder.Append("--->");
				text.SkipChar();
				text.SkipChar();
				return true;
			}
			break;
		}
		ReadOnlySpan<char> span = text.AsSpan();
		int num = span.IndexOf("-->".AsSpan(), StringComparison.Ordinal);
		if (num < 0)
		{
			return false;
		}
		builder.Append("--");
		builder.Append(span.Slice(0, num + "-->".Length));
		text.Start += num + "-->".Length;
		return true;
	}

	private static bool TryParseHtmlTagProcessingInstruction(ref StringSlice text, ref ValueStringBuilder builder)
	{
		builder.Append('?');
		char c = '\0';
		while (true)
		{
			char c2 = text.NextChar();
			switch (c2)
			{
			case '\0':
				return false;
			case '>':
				if (c == '?')
				{
					builder.Append('>');
					text.SkipChar();
					return true;
				}
				break;
			}
			c = c2;
			builder.Append(c2);
		}
	}

	public static string Unescape(string? text, bool removeBackSlash = true)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		int num = 0;
		int num2 = 0;
		char c = '\0';
		char[] anyOf = (removeBackSlash ? SearchBackAndAmp : SearchAmp);
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder sb = new ValueStringBuilder(initialBuffer);
		while ((num = text.IndexOfAny(anyOf, num)) != -1)
		{
			c = text[num];
			if (removeBackSlash && c == '\\')
			{
				num++;
				if (text.Length == num)
				{
					break;
				}
				c = text[num];
				if (c.IsEscapableSymbol())
				{
					sb.Append(text.AsSpan(num2, num - num2 - 1));
					num2 = num;
				}
			}
			else
			{
				if (c != '&')
				{
					continue;
				}
				int num3 = ScanEntity(new StringSlice(text, num, text.Length - 1), out var numericEntity, out var namedEntityStart, out var namedEntityLength);
				if (num3 == 0)
				{
					num++;
					continue;
				}
				num += num3;
				if (namedEntityLength > 0)
				{
					string text2 = EntityHelper.DecodeEntity(text.AsSpan(namedEntityStart, namedEntityLength));
					if (text2 != null)
					{
						sb.Append(text.AsSpan(num2, num - num3 - num2));
						sb.Append(text2);
						num2 = num;
					}
				}
				else if (numericEntity >= 0)
				{
					sb.Append(text.AsSpan(num2, num - num3 - num2));
					EntityHelper.DecodeEntity(numericEntity, ref sb);
					num2 = num;
				}
			}
		}
		if (c == '\0')
		{
			sb.Dispose();
			return text;
		}
		sb.Append(text.AsSpan(num2, text.Length - num2));
		return sb.ToString();
	}

	public static int ScanEntity<T>(T slice, out int numericEntity, out int namedEntityStart, out int namedEntityLength) where T : ICharIterator
	{
		numericEntity = 0;
		namedEntityStart = 0;
		namedEntityLength = 0;
		if (slice.CurrentChar != '&' || slice.PeekChar(3) == '\0')
		{
			return 0;
		}
		int start = slice.Start;
		char c = slice.NextChar();
		int num = 0;
		if (c == '#')
		{
			c = slice.PeekChar();
			if ((c | 0x20) == 120)
			{
				c = slice.NextChar();
				while (c != 0)
				{
					c = slice.NextChar();
					if (c.IsDigit())
					{
						if (++num == 7)
						{
							return 0;
						}
						numericEntity = numericEntity * 16 + (c - 48);
						continue;
					}
					if ((uint)((c - 65) & -33) <= 5u)
					{
						if (++num == 7)
						{
							return 0;
						}
						numericEntity = numericEntity * 16 + ((c | 0x20) - 97 + 10);
						continue;
					}
					if (c == ';')
					{
						if (num != 0)
						{
							return slice.Start - start + 1;
						}
						return 0;
					}
					return 0;
				}
			}
			else
			{
				while (c != 0)
				{
					c = slice.NextChar();
					if (c.IsDigit())
					{
						if (++num == 8)
						{
							return 0;
						}
						numericEntity = numericEntity * 10 + (c - 48);
						continue;
					}
					if (c == ';')
					{
						if (num != 0)
						{
							return slice.Start - start + 1;
						}
						return 0;
					}
					return 0;
				}
			}
		}
		else
		{
			if (!c.IsAlpha())
			{
				return 0;
			}
			namedEntityStart = slice.Start;
			namedEntityLength++;
			while (c != 0)
			{
				c = slice.NextChar();
				if (c.IsAlphaNumeric())
				{
					if (++num == 32)
					{
						return 0;
					}
					namedEntityLength++;
					continue;
				}
				if (c == ';')
				{
					if (num != 0)
					{
						return slice.Start - start + 1;
					}
					return 0;
				}
				return 0;
			}
		}
		return 0;
	}
}
