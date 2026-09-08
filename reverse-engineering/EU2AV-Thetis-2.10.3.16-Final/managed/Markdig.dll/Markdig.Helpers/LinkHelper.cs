using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Markdig.Syntax;

namespace Markdig.Helpers;

public static class LinkHelper
{
	public static bool TryParseAutolink(StringSlice text, [NotNullWhen(true)] out string? link, out bool isEmail)
	{
		return TryParseAutolink(ref text, out link, out isEmail);
	}

	public static string Urilize(string headingText, bool allowOnlyAscii, bool keepOpeningDigits = false)
	{
		return Urilize(headingText.AsSpan(), allowOnlyAscii, keepOpeningDigits);
	}

	public static string Urilize(ReadOnlySpan<char> headingText, bool allowOnlyAscii, bool keepOpeningDigits = false)
	{
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		bool flag = keepOpeningDigits && headingText.Length > 0 && char.IsLetterOrDigit(headingText[0]);
		bool flag2 = false;
		string text = string.Empty;
		if (allowOnlyAscii)
		{
			text = headingText.ToString().Normalize(NormalizationForm.FormD);
		}
		ReadOnlySpan<char> readOnlySpan = (string.IsNullOrEmpty(text) ? headingText : text.AsSpan());
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			if (allowOnlyAscii && CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
			{
				continue;
			}
			ReadOnlySpan<char> readOnlySpan2 = ((!IsSpecialScandinavianOrGermanChar(c)) ? (allowOnlyAscii ? CharNormalizer.ConvertToAscii(c).AsSpan() : ReadOnlySpan<char>.Empty) : NormalizeScandinavianOrGermanChar(c));
			for (int j = 0; j < ((readOnlySpan2.Length < 1) ? 1 : readOnlySpan2.Length); j++)
			{
				if (!readOnlySpan2.IsEmpty)
				{
					c = readOnlySpan2[j];
				}
				if (char.IsLetter(c))
				{
					if (!allowOnlyAscii || (c >= ' ' && c < '\u007f'))
					{
						c = (char.IsUpper(c) ? char.ToLowerInvariant(c) : c);
						valueStringBuilder.Append(c);
						flag = true;
						flag2 = false;
					}
				}
				else
				{
					if (!flag)
					{
						continue;
					}
					if (IsReservedPunctuation(c))
					{
						if (flag2)
						{
							valueStringBuilder.Length--;
						}
						if (valueStringBuilder[valueStringBuilder.Length - 1] != c)
						{
							valueStringBuilder.Append(c);
						}
						flag2 = false;
					}
					else if (c.IsDigit())
					{
						valueStringBuilder.Append(c);
						flag2 = false;
					}
					else if (!flag2 && c.IsWhitespace())
					{
						if (!IsReservedPunctuation(valueStringBuilder[valueStringBuilder.Length - 1]))
						{
							valueStringBuilder.Append('-');
						}
						flag2 = true;
					}
				}
			}
		}
		while (valueStringBuilder.Length > 0 && IsReservedPunctuation(valueStringBuilder[valueStringBuilder.Length - 1]))
		{
			valueStringBuilder.Length--;
		}
		return valueStringBuilder.ToString();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsSpecialScandinavianOrGermanChar(char c)
	{
		if (c != 'ä' && c != 'ö' && c != 'ü' && c != 'Ä' && c != 'Ö' && c != 'Ü' && c != 'ß' && c != 'æ' && c != 'ø' && c != 'å' && c != 'Æ' && c != 'Ø' && c != 'Å' && c != 'þ' && c != 'ð' && c != 'Þ')
		{
			return c == 'Ð';
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ReadOnlySpan<char> NormalizeScandinavianOrGermanChar(char c)
	{
		return c switch
		{
			'ä' => "ae".AsSpan(), 
			'ö' => "oe".AsSpan(), 
			'ü' => "ue".AsSpan(), 
			'Ä' => "Ae".AsSpan(), 
			'Ö' => "Oe".AsSpan(), 
			'Ü' => "Ue".AsSpan(), 
			'ß' => "ss".AsSpan(), 
			'æ' => "ae".AsSpan(), 
			'ø' => "oe".AsSpan(), 
			'å' => "aa".AsSpan(), 
			'Æ' => "Ae".AsSpan(), 
			'Ø' => "Oe".AsSpan(), 
			'Å' => "Aa".AsSpan(), 
			'þ' => "th".AsSpan(), 
			'Þ' => "Th".AsSpan(), 
			'ð' => "d".AsSpan(), 
			'Ð' => "D".AsSpan(), 
			_ => ReadOnlySpan<char>.Empty, 
		};
	}

	public static string UrilizeAsGfm(string headingText)
	{
		return UrilizeAsGfm(headingText.AsSpan());
	}

	public static string UrilizeAsGfm(ReadOnlySpan<char> headingText)
	{
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		for (int i = 0; i < headingText.Length; i++)
		{
			char c = headingText[i];
			if (!char.IsLetterOrDigit(c))
			{
				switch (c)
				{
				case '-':
				case '_':
					break;
				case ' ':
					valueStringBuilder.Append('-');
					continue;
				default:
					continue;
				}
			}
			valueStringBuilder.Append(char.ToLowerInvariant(c));
		}
		return valueStringBuilder.ToString();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsReservedPunctuation(char c)
	{
		if (c != '_' && c != '-')
		{
			return c == '.';
		}
		return true;
	}

	public static bool TryParseAutolink(ref StringSlice text, [NotNullWhen(true)] out string? link, out bool isEmail)
	{
		link = null;
		isEmail = false;
		char currentChar = text.CurrentChar;
		if (currentChar != '<')
		{
			return false;
		}
		currentChar = text.NextChar();
		int num = 0;
		if (!currentChar.IsAlpha())
		{
			if (!CharHelper.IsEmailUsernameSpecialCharOrDigit(currentChar))
			{
				return false;
			}
			num = -1;
		}
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		valueStringBuilder.Append(currentChar);
		while (true)
		{
			currentChar = text.NextChar();
			bool flag = currentChar == '+' || currentChar == '.' || currentChar == '-';
			bool flag2 = currentChar.IsAlphaNumeric() | flag;
			if (num <= 0 && CharHelper.IsEmailUsernameSpecialChar(currentChar))
			{
				flag2 = true;
				if (!flag)
				{
					num = -1;
				}
			}
			if (flag2)
			{
				if (num > 0 && valueStringBuilder.Length >= 32)
				{
					break;
				}
				valueStringBuilder.Append(currentChar);
				continue;
			}
			if (currentChar == ':')
			{
				if (num < 0 || valueStringBuilder.Length <= 2)
				{
					break;
				}
				num = 1;
			}
			else
			{
				if (currentChar != '@' || num > 0)
				{
					break;
				}
				num = -1;
			}
			valueStringBuilder.Append(currentChar);
			if (num < 0)
			{
				isEmail = true;
				bool flag3 = false;
				int num2 = 0;
				char c = '\0';
				while (true)
				{
					currentChar = text.NextChar();
					if (currentChar == '>')
					{
						if ((num2 == 0) | flag3)
						{
							break;
						}
						text.SkipChar();
						link = valueStringBuilder.ToString();
						return true;
					}
					if (currentChar.IsAlphaNumeric() || (num2 > 0 && (flag3 = currentChar == '-')))
					{
						num2++;
						if (num2 > 63)
						{
							break;
						}
					}
					else
					{
						if (currentChar != '.' || c == '.' || c == '-')
						{
							break;
						}
						num2 = 0;
						flag3 = false;
					}
					valueStringBuilder.Append(currentChar);
					c = currentChar;
				}
				break;
			}
			text.SkipChar();
			ReadOnlySpan<char> span = text.AsSpan();
			int num3 = span.IndexOfAny(CharHelper.InvalidAutoLinkCharacters);
			if ((uint)num3 >= (uint)span.Length || span[num3] != '>')
			{
				break;
			}
			valueStringBuilder.Append(span.Slice(0, num3));
			link = valueStringBuilder.ToString();
			text.Start += num3 + 1;
			return true;
		}
		valueStringBuilder.Dispose();
		return false;
	}

	public static bool TryParseInlineLink(StringSlice text, out string? link, out string? title)
	{
		SourceSpan linkSpan;
		SourceSpan titleSpan;
		return TryParseInlineLink(ref text, out link, out title, out linkSpan, out titleSpan);
	}

	public static bool TryParseInlineLink(StringSlice text, out string? link, out string? title, out SourceSpan linkSpan, out SourceSpan titleSpan)
	{
		return TryParseInlineLink(ref text, out link, out title, out linkSpan, out titleSpan);
	}

	public static bool TryParseInlineLink(ref StringSlice text, out string? link, out string? title)
	{
		SourceSpan linkSpan;
		SourceSpan titleSpan;
		return TryParseInlineLink(ref text, out link, out title, out linkSpan, out titleSpan);
	}

	public static bool TryParseInlineLink(ref StringSlice text, out string? link, out string? title, out SourceSpan linkSpan, out SourceSpan titleSpan)
	{
		bool flag = false;
		char currentChar = text.CurrentChar;
		link = null;
		title = null;
		linkSpan = SourceSpan.Empty;
		titleSpan = SourceSpan.Empty;
		if (currentChar == '(')
		{
			text.SkipChar();
			text.TrimStart();
			int start = text.Start;
			if (TryParseUrl(ref text, out link, out var _))
			{
				linkSpan.Start = start;
				linkSpan.End = text.Start - 1;
				if (linkSpan.End < linkSpan.Start)
				{
					linkSpan = SourceSpan.Empty;
				}
				text.TrimStart(out var spaceCount);
				bool flag2 = spaceCount > 0;
				if (text.CurrentChar == ')')
				{
					flag = true;
				}
				else if (flag2)
				{
					char currentChar2 = text.CurrentChar;
					start = text.Start;
					char enclosingCharacter;
					if (currentChar2 == ')')
					{
						flag = true;
					}
					else if (TryParseTitle(ref text, out title, out enclosingCharacter))
					{
						titleSpan.Start = start;
						titleSpan.End = text.Start - 1;
						if (titleSpan.End < titleSpan.Start)
						{
							titleSpan = SourceSpan.Empty;
						}
						text.TrimStart();
						if (text.CurrentChar == ')')
						{
							flag = true;
						}
					}
				}
			}
		}
		if (flag)
		{
			text.SkipChar();
		}
		return flag;
	}

	public static bool TryParseInlineLinkTrivia(ref StringSlice text, [NotNullWhen(true)] out string? link, out SourceSpan unescapedLink, out string? title, out SourceSpan unescapedTitle, out char titleEnclosingCharacter, out SourceSpan linkSpan, out SourceSpan titleSpan, out SourceSpan triviaBeforeLink, out SourceSpan triviaAfterLink, out SourceSpan triviaAfterTitle, out bool urlHasPointyBrackets)
	{
		bool flag = false;
		char currentChar = text.CurrentChar;
		link = null;
		unescapedLink = SourceSpan.Empty;
		title = null;
		unescapedTitle = SourceSpan.Empty;
		linkSpan = SourceSpan.Empty;
		titleSpan = SourceSpan.Empty;
		triviaBeforeLink = SourceSpan.Empty;
		triviaAfterLink = SourceSpan.Empty;
		triviaAfterTitle = SourceSpan.Empty;
		urlHasPointyBrackets = false;
		titleEnclosingCharacter = '\0';
		if (currentChar == '(')
		{
			text.SkipChar();
			int start = text.Start;
			text.TrimStart();
			triviaBeforeLink = new SourceSpan(start, text.Start - 1);
			int start2 = text.Start;
			if (TryParseUrlTrivia(ref text, out link, out urlHasPointyBrackets))
			{
				linkSpan.Start = start2;
				linkSpan.End = text.Start - 1;
				unescapedLink.Start = start2 + (urlHasPointyBrackets ? 1 : 0);
				unescapedLink.End = text.Start - 1 - (urlHasPointyBrackets ? 1 : 0);
				if (linkSpan.End < linkSpan.Start)
				{
					linkSpan = SourceSpan.Empty;
				}
				int start3 = text.Start;
				text.TrimStart(out var spaceCount);
				triviaAfterLink = new SourceSpan(start3, text.Start - 1);
				bool flag2 = spaceCount > 0;
				if (text.CurrentChar == ')')
				{
					flag = true;
				}
				else if (flag2)
				{
					char currentChar2 = text.CurrentChar;
					start2 = text.Start;
					if (currentChar2 == ')')
					{
						flag = true;
					}
					else if (TryParseTitleTrivia(ref text, out title, out titleEnclosingCharacter))
					{
						titleSpan.Start = start2;
						titleSpan.End = text.Start - 1;
						unescapedTitle.Start = start2 + 1;
						unescapedTitle.End = text.Start - 1 - 1;
						if (titleSpan.End < titleSpan.Start)
						{
							titleSpan = SourceSpan.Empty;
						}
						int start4 = text.Start;
						text.TrimStart();
						triviaAfterTitle = new SourceSpan(start4, text.Start - 1);
						if (text.CurrentChar == ')')
						{
							flag = true;
						}
					}
				}
			}
		}
		if (flag)
		{
			text.SkipChar();
			if (title == null)
			{
				title = string.Empty;
			}
		}
		return flag;
	}

	public static bool TryParseTitle<T>(T text, out string? title) where T : ICharIterator
	{
		char enclosingCharacter;
		return TryParseTitle(ref text, out title, out enclosingCharacter);
	}

	public static bool TryParseTitle<T>(ref T text, out string? title, out char enclosingCharacter) where T : ICharIterator
	{
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		enclosingCharacter = '\0';
		char currentChar = text.CurrentChar;
		if (currentChar == '\'' || currentChar == '"' || currentChar == '(')
		{
			enclosingCharacter = currentChar;
			char c = ((currentChar == '(') ? ')' : currentChar);
			bool flag = false;
			bool flag2 = false;
			while ((currentChar = text.NextChar()) != 0)
			{
				if (currentChar == '\r' || currentChar == '\n')
				{
					if (flag2)
					{
						break;
					}
					if (flag)
					{
						flag = false;
						valueStringBuilder.Append('\\');
					}
					valueStringBuilder.Append(currentChar);
					if (currentChar == '\r' && text.PeekChar() == '\n')
					{
						valueStringBuilder.Append('\n');
						text.SkipChar();
					}
					flag2 = true;
					continue;
				}
				if (flag)
				{
					flag = false;
					if (!currentChar.IsAsciiPunctuation())
					{
						valueStringBuilder.Append('\\');
					}
					valueStringBuilder.Append(currentChar);
					continue;
				}
				if (currentChar == c)
				{
					text.SkipChar();
					title = valueStringBuilder.ToString();
					return true;
				}
				if (currentChar == '\\')
				{
					flag = true;
					flag2 = false;
					continue;
				}
				if (flag2 && !currentChar.IsSpaceOrTab())
				{
					flag2 = false;
				}
				valueStringBuilder.Append(currentChar);
			}
		}
		valueStringBuilder.Dispose();
		title = null;
		return false;
	}

	public static bool TryParseTitleTrivia<T>(ref T text, out string? title, out char enclosingCharacter) where T : ICharIterator
	{
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		enclosingCharacter = '\0';
		char currentChar = text.CurrentChar;
		if (currentChar == '\'' || currentChar == '"' || currentChar == '(')
		{
			enclosingCharacter = currentChar;
			char c = ((currentChar == '(') ? ')' : currentChar);
			bool flag = false;
			bool flag2 = false;
			while ((currentChar = text.NextChar()) != 0)
			{
				if (currentChar == '\r' || currentChar == '\n')
				{
					if (flag2)
					{
						break;
					}
					if (flag)
					{
						flag = false;
						valueStringBuilder.Append('\\');
					}
					valueStringBuilder.Append(currentChar);
					if (currentChar == '\r' && text.PeekChar() == '\n')
					{
						valueStringBuilder.Append('\n');
						text.SkipChar();
					}
					flag2 = true;
					continue;
				}
				if (flag)
				{
					flag = false;
					if (!currentChar.IsAsciiPunctuation())
					{
						valueStringBuilder.Append('\\');
					}
					valueStringBuilder.Append(currentChar);
					continue;
				}
				if (currentChar == c)
				{
					text.SkipChar();
					title = valueStringBuilder.ToString();
					return true;
				}
				if (currentChar == '\\')
				{
					flag = true;
					flag2 = false;
					continue;
				}
				if (flag2 && !currentChar.IsSpaceOrTab())
				{
					flag2 = false;
				}
				valueStringBuilder.Append(currentChar);
			}
		}
		valueStringBuilder.Dispose();
		title = null;
		return false;
	}

	public static bool TryParseUrl<T>(T text, [NotNullWhen(true)] out string? link) where T : ICharIterator
	{
		bool hasPointyBrackets;
		return TryParseUrl(ref text, out link, out hasPointyBrackets);
	}

	public static bool TryParseUrl<T>(ref T text, [NotNullWhen(true)] out string? link, out bool hasPointyBrackets, bool isAutoLink = false) where T : ICharIterator
	{
		bool flag = false;
		hasPointyBrackets = false;
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		char c = text.CurrentChar;
		if (c == '<')
		{
			bool flag2 = false;
			do
			{
				c = text.NextChar();
				if (!flag2 && c == '>')
				{
					text.SkipChar();
					hasPointyBrackets = true;
					flag = true;
					break;
				}
				if (!flag2 && c == '<')
				{
					break;
				}
				if (flag2)
				{
					flag2 = false;
					if (!c.IsAsciiPunctuation())
					{
						valueStringBuilder.Append('\\');
					}
				}
				else if (c == '\\')
				{
					flag2 = true;
					continue;
				}
				if (c.IsNewLineOrLineFeed())
				{
					break;
				}
				valueStringBuilder.Append(c);
			}
			while (c != 0);
		}
		else
		{
			bool flag3 = false;
			int num = 0;
			while (true)
			{
				if (c == '(' && !flag3)
				{
					num++;
				}
				if (c == ')' && !flag3)
				{
					num--;
					if (num < 0)
					{
						flag = true;
						break;
					}
				}
				if (!isAutoLink)
				{
					if (flag3)
					{
						flag3 = false;
						if (!c.IsAsciiPunctuation())
						{
							valueStringBuilder.Append('\\');
						}
					}
					else if (c == '\\')
					{
						flag3 = true;
						c = text.NextChar();
						continue;
					}
				}
				if (IsEndOfUri(c, isAutoLink))
				{
					flag = true;
					break;
				}
				if (isAutoLink)
				{
					if (c == '&' && HtmlHelper.ScanEntity(text, out var _, out var _, out var _) > 0)
					{
						flag = true;
						break;
					}
					if (IsTrailingUrlStopCharacter(c) && IsEndOfUri(text.PeekChar(), isAutoLink: true))
					{
						flag = true;
						break;
					}
				}
				valueStringBuilder.Append(c);
				c = text.NextChar();
			}
			if (num > 0)
			{
				flag = false;
			}
		}
		if (flag)
		{
			link = valueStringBuilder.ToString();
		}
		else
		{
			valueStringBuilder.Dispose();
			link = null;
		}
		return flag;
	}

	public static bool TryParseUrlTrivia<T>(ref T text, out string? link, out bool hasPointyBrackets, bool isAutoLink = false) where T : ICharIterator
	{
		bool flag = false;
		hasPointyBrackets = false;
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		char c = text.CurrentChar;
		if (c == '<')
		{
			bool flag2 = false;
			do
			{
				c = text.NextChar();
				if (!flag2 && c == '>')
				{
					text.SkipChar();
					hasPointyBrackets = true;
					flag = true;
					break;
				}
				if (!flag2 && c == '<')
				{
					break;
				}
				if (flag2)
				{
					flag2 = false;
					if (!c.IsAsciiPunctuation())
					{
						valueStringBuilder.Append('\\');
					}
				}
				else if (c == '\\')
				{
					flag2 = true;
					continue;
				}
				if (c.IsNewLineOrLineFeed())
				{
					break;
				}
				valueStringBuilder.Append(c);
			}
			while (c != 0);
		}
		else
		{
			bool flag3 = false;
			int num = 0;
			while (true)
			{
				if (c == '(' && !flag3)
				{
					num++;
				}
				if (c == ')' && !flag3)
				{
					num--;
					if (num < 0)
					{
						flag = true;
						break;
					}
				}
				if (!isAutoLink)
				{
					if (flag3)
					{
						flag3 = false;
						if (!c.IsAsciiPunctuation())
						{
							valueStringBuilder.Append('\\');
						}
					}
					else if (c == '\\')
					{
						flag3 = true;
						c = text.NextChar();
						continue;
					}
				}
				if (IsEndOfUri(c, isAutoLink))
				{
					flag = true;
					break;
				}
				if (isAutoLink)
				{
					if (c == '&' && HtmlHelper.ScanEntity(text, out var _, out var _, out var _) > 0)
					{
						flag = true;
						break;
					}
					if (IsTrailingUrlStopCharacter(c) && IsEndOfUri(text.PeekChar(), isAutoLink: true))
					{
						flag = true;
						break;
					}
				}
				valueStringBuilder.Append(c);
				c = text.NextChar();
			}
			if (num > 0)
			{
				flag = false;
			}
		}
		if (flag)
		{
			link = valueStringBuilder.ToString();
		}
		else
		{
			valueStringBuilder.Dispose();
			link = null;
		}
		return flag;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsTrailingUrlStopCharacter(char c)
	{
		if (c != '?' && c != '!' && c != '.' && c != ',' && c != ':' && c != '*' && c != '*' && c != '_')
		{
			return c == '~';
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsEndOfUri(char c, bool isAutoLink)
	{
		if (c != 0 && !c.IsSpaceOrTab() && !c.IsControl())
		{
			if (isAutoLink)
			{
				return c == '<';
			}
			return false;
		}
		return true;
	}

	public static bool IsValidDomain(string link, int prefixLength, bool allowDomainWithoutPeriod = false)
	{
		int num = 1;
		bool flag = false;
		int num2 = -1;
		for (int i = prefixLength; (uint)i < (uint)link.Length; i++)
		{
			char c = link[i];
			if (!c.IsAlphaNumeric())
			{
				if (c == '.')
				{
					if (!flag)
					{
						return false;
					}
					num++;
					flag = false;
					continue;
				}
				if (c == '/' || c == '?' || c == '#' || c == ':')
				{
					break;
				}
				switch (c)
				{
				case '_':
					num2 = num;
					break;
				default:
					if (CharHelper.IsSpaceOrPunctuationForGFMAutoLink(c))
					{
						return false;
					}
					break;
				case '-':
					break;
				}
			}
			flag = true;
		}
		if (((num != 1) | allowDomainWithoutPeriod) & flag)
		{
			return num - num2 >= 2;
		}
		return false;
	}

	public static bool TryParseLinkReferenceDefinition<T>(ref T text, out string? label, out string? url, out string? title, out SourceSpan labelSpan, out SourceSpan urlSpan, out SourceSpan titleSpan) where T : ICharIterator
	{
		url = null;
		title = null;
		urlSpan = SourceSpan.Empty;
		titleSpan = SourceSpan.Empty;
		if (!TryParseLabel(ref text, out label, out labelSpan))
		{
			return false;
		}
		if (text.CurrentChar != ':')
		{
			label = null;
			return false;
		}
		text.SkipChar();
		text.TrimStart();
		urlSpan.Start = text.Start;
		bool flag = text.CurrentChar == '<';
		if (!TryParseUrl(ref text, out url, out var _) || (!flag && string.IsNullOrEmpty(url)))
		{
			return false;
		}
		urlSpan.End = text.Start - 1;
		T val = text;
		bool flag2 = CharIteratorHelper.TrimStartAndCountNewLines(ref text, out var countNewLines);
		char currentChar = text.CurrentChar;
		if (currentChar == '\'' || currentChar == '"' || currentChar == '(')
		{
			titleSpan.Start = text.Start;
			if (!TryParseTitle(ref text, out title, out var _))
			{
				return false;
			}
			titleSpan.End = text.Start - 1;
			if (!flag2)
			{
				return false;
			}
		}
		else if (text.IsEmpty || countNewLines > 0)
		{
			return true;
		}
		currentChar = text.CurrentChar;
		while (currentChar.IsSpaceOrTab())
		{
			currentChar = text.NextChar();
		}
		if (currentChar != 0 && currentChar != '\n' && currentChar != '\r')
		{
			if (countNewLines > 0 && title != null)
			{
				text = val;
				title = null;
				return true;
			}
			label = null;
			url = null;
			title = null;
			return false;
		}
		if (currentChar == '\r' && text.PeekChar() == '\n')
		{
			text.SkipChar();
		}
		return true;
	}

	public static bool TryParseLinkReferenceDefinitionTrivia<T>(ref T text, out SourceSpan triviaBeforeLabel, out string? label, out SourceSpan labelWithTrivia, out SourceSpan triviaBeforeUrl, out string? url, out SourceSpan unescapedUrl, out bool urlHasPointyBrackets, out SourceSpan triviaBeforeTitle, out string? title, out SourceSpan unescapedTitle, out char titleEnclosingCharacter, out NewLine newLine, out SourceSpan triviaAfterTitle, out SourceSpan labelSpan, out SourceSpan urlSpan, out SourceSpan titleSpan) where T : ICharIterator
	{
		labelWithTrivia = SourceSpan.Empty;
		triviaBeforeUrl = SourceSpan.Empty;
		url = null;
		unescapedUrl = SourceSpan.Empty;
		triviaBeforeTitle = SourceSpan.Empty;
		title = null;
		unescapedTitle = SourceSpan.Empty;
		newLine = NewLine.None;
		urlSpan = SourceSpan.Empty;
		titleSpan = SourceSpan.Empty;
		text.TrimStart();
		triviaBeforeLabel = new SourceSpan(0, text.Start - 1);
		triviaAfterTitle = SourceSpan.Empty;
		urlHasPointyBrackets = false;
		titleEnclosingCharacter = '\0';
		labelWithTrivia.Start = text.Start + 1;
		if (!TryParseLabelTrivia(ref text, out label, out labelSpan))
		{
			return false;
		}
		labelWithTrivia.End = text.Start - 2;
		if (text.CurrentChar != ':')
		{
			label = null;
			return false;
		}
		text.SkipChar();
		int start = text.Start;
		text.TrimStart();
		triviaBeforeUrl = new SourceSpan(start, text.Start - 1);
		urlSpan.Start = text.Start;
		bool flag = text.CurrentChar == '<';
		unescapedUrl.Start = text.Start + (flag ? 1 : 0);
		if (!TryParseUrlTrivia(ref text, out url, out urlHasPointyBrackets) || (!flag && string.IsNullOrEmpty(url)))
		{
			return false;
		}
		urlSpan.End = text.Start - 1;
		unescapedUrl.End = text.Start - 1 - (flag ? 1 : 0);
		int start2 = text.Start;
		T val = text;
		bool flag2 = CharIteratorHelper.TrimStartAndCountNewLines(ref text, out var countNewLines, out newLine);
		int end = text.Start - 1;
		triviaBeforeTitle = new SourceSpan(start2, end);
		char currentChar = text.CurrentChar;
		if (currentChar == '\'' || currentChar == '"' || currentChar == '(')
		{
			titleSpan.Start = text.Start;
			unescapedTitle.Start = text.Start + 1;
			if (!TryParseTitleTrivia(ref text, out title, out titleEnclosingCharacter))
			{
				return false;
			}
			titleSpan.End = text.Start - 1;
			unescapedTitle.End = text.Start - 1 - 1;
			if (!flag2)
			{
				return false;
			}
			newLine = NewLine.None;
		}
		else if (text.IsEmpty || countNewLines > 0)
		{
			triviaBeforeTitle.End -= newLine.Length();
			triviaAfterTitle = new SourceSpan(text.Start, text.Start - 1);
			return true;
		}
		currentChar = text.CurrentChar;
		int start3 = text.Start;
		while (currentChar.IsSpaceOrTab())
		{
			currentChar = text.NextChar();
		}
		if (currentChar != 0 && currentChar != '\n' && currentChar != '\r')
		{
			if (countNewLines > 0 && title != null)
			{
				text = val;
				title = null;
				newLine = NewLine.None;
				unescapedTitle = SourceSpan.Empty;
				triviaAfterTitle = SourceSpan.Empty;
				return true;
			}
			label = null;
			url = null;
			unescapedUrl = SourceSpan.Empty;
			title = null;
			unescapedTitle = SourceSpan.Empty;
			return false;
		}
		triviaAfterTitle = new SourceSpan(start3, text.Start - 1);
		if (currentChar != 0)
		{
			if (currentChar == '\n')
			{
				newLine = NewLine.LineFeed;
			}
			else if (currentChar == '\r' && text.PeekChar() == '\n')
			{
				newLine = NewLine.CarriageReturnLineFeed;
				text.SkipChar();
			}
			else if (currentChar == '\r')
			{
				newLine = NewLine.CarriageReturn;
			}
		}
		return true;
	}

	public static bool TryParseLabel<T>(T lines, [NotNullWhen(true)] out string? label) where T : ICharIterator
	{
		SourceSpan labelSpan;
		return TryParseLabel(ref lines, allowEmpty: false, out label, out labelSpan);
	}

	public static bool TryParseLabel<T>(T lines, [NotNullWhen(true)] out string? label, out SourceSpan labelSpan) where T : ICharIterator
	{
		return TryParseLabel(ref lines, allowEmpty: false, out label, out labelSpan);
	}

	public static bool TryParseLabel<T>(ref T lines, [NotNullWhen(true)] out string? label) where T : ICharIterator
	{
		SourceSpan labelSpan;
		return TryParseLabel(ref lines, allowEmpty: false, out label, out labelSpan);
	}

	public static bool TryParseLabel<T>(ref T lines, [NotNullWhen(true)] out string? label, out SourceSpan labelSpan) where T : ICharIterator
	{
		return TryParseLabel(ref lines, allowEmpty: false, out label, out labelSpan);
	}

	public static bool TryParseLabelTrivia<T>(ref T lines, [NotNullWhen(true)] out string? label, out SourceSpan labelSpan) where T : ICharIterator
	{
		return TryParseLabelTrivia(ref lines, allowEmpty: false, out label, out labelSpan);
	}

	public static bool TryParseLabel<T>(ref T lines, bool allowEmpty, [NotNullWhen(true)] out string? label, out SourceSpan labelSpan) where T : ICharIterator
	{
		label = null;
		char currentChar = lines.CurrentChar;
		labelSpan = SourceSpan.Empty;
		if (currentChar != '[')
		{
			return false;
		}
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		int num = -1;
		int num2 = -1;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = false;
		while (true)
		{
			currentChar = lines.NextChar();
			if (currentChar != 0)
			{
				if (flag)
				{
					if (currentChar == '[' || currentChar == ']' || currentChar == '\\')
					{
						goto IL_0115;
					}
				}
				else if (currentChar != '[')
				{
					if (currentChar != ']')
					{
						goto IL_0115;
					}
					lines.SkipChar();
					if (allowEmpty | flag3)
					{
						int num3 = valueStringBuilder.Length - 1;
						while (num3 >= 0 && valueStringBuilder[num3].IsWhitespace())
						{
							valueStringBuilder.Length = num3;
							num2--;
							num3--;
						}
						if (valueStringBuilder.Length <= 999)
						{
							labelSpan.Start = num;
							labelSpan.End = num2;
							if (labelSpan.Start > labelSpan.End)
							{
								labelSpan = SourceSpan.Empty;
							}
							break;
						}
					}
				}
			}
			valueStringBuilder.Dispose();
			return false;
			IL_0115:
			bool flag4 = currentChar.IsWhitespace();
			if (flag4)
			{
				currentChar = ' ';
			}
			if (!flag && currentChar == '\\')
			{
				if (num < 0)
				{
					num = lines.Start;
				}
				flag = true;
			}
			else
			{
				flag = false;
				if (!flag2 || !flag4)
				{
					if (num < 0)
					{
						num = lines.Start;
					}
					num2 = lines.Start;
					valueStringBuilder.Append(currentChar);
					if (!flag4)
					{
						flag3 = true;
					}
				}
			}
			flag2 = flag4;
		}
		label = valueStringBuilder.ToString();
		return true;
	}

	public static bool TryParseLabelTrivia<T>(ref T lines, bool allowEmpty, out string? label, out SourceSpan labelSpan) where T : ICharIterator
	{
		label = null;
		char currentChar = lines.CurrentChar;
		labelSpan = SourceSpan.Empty;
		if (currentChar != '[')
		{
			return false;
		}
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		int num = -1;
		int num2 = -1;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = false;
		while (true)
		{
			currentChar = lines.NextChar();
			if (currentChar != 0)
			{
				if (flag)
				{
					if (currentChar == '[' || currentChar == ']' || currentChar == '\\')
					{
						goto IL_0118;
					}
				}
				else if (currentChar != '[')
				{
					if (currentChar != ']')
					{
						goto IL_0118;
					}
					lines.SkipChar();
					if (allowEmpty | flag3)
					{
						int num3 = valueStringBuilder.Length - 1;
						while (num3 >= 0 && valueStringBuilder[num3].IsWhitespace())
						{
							valueStringBuilder.Length = num3;
							num2--;
							num3--;
						}
						if (valueStringBuilder.Length <= 999)
						{
							labelSpan.Start = num;
							labelSpan.End = num2;
							if (labelSpan.Start > labelSpan.End)
							{
								labelSpan = SourceSpan.Empty;
							}
							break;
						}
					}
				}
			}
			valueStringBuilder.Dispose();
			return false;
			IL_0118:
			bool flag4 = currentChar.IsWhitespace();
			if (!flag && currentChar == '\\')
			{
				if (num < 0)
				{
					num = lines.Start;
				}
				flag = true;
			}
			else
			{
				flag = false;
				if (!flag2 || !flag4)
				{
					if (num < 0)
					{
						num = lines.Start;
					}
					num2 = lines.Start;
					if (flag4)
					{
						valueStringBuilder.Append(' ');
					}
					else
					{
						valueStringBuilder.Append(currentChar);
					}
					if (!flag4)
					{
						flag3 = true;
					}
				}
			}
			flag2 = flag4;
		}
		label = valueStringBuilder.ToString();
		return true;
	}
}
