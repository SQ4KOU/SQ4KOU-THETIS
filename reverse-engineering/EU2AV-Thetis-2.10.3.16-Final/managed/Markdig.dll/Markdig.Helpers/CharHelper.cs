using System;
using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Markdig.Helpers;

public static class CharHelper
{
	public const int TabSize = 4;

	public const char ReplacementChar = '\ufffd';

	public const string ReplacementCharString = "\ufffd";

	private const string EmailUsernameSpecialChars = ".!#$%&'*+/=?^_`{|}~-+.~";

	private const string AsciiWhitespaceChars = "\t\n\f\r ";

	internal const string WhitespaceChars = "\t\n\f\r \u00a0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u202f\u205f\u3000";

	private const string AsciiPunctuationChars = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

	private const int UnicodePunctuationCategoryMask = 33292288;

	private const int UnicodePunctuationOrSpaceCategoryMask = 33294336;

	private const int CommonMarkPunctuationCategoryMask = 536608768;

	private static readonly SearchValues<char> s_emailUsernameSpecialChar = SearchValues.Create(".!#$%&'*+/=?^_`{|}~-+.~");

	private static readonly SearchValues<char> s_emailUsernameSpecialCharOrDigit = SearchValues.Create(".!#$%&'*+/=?^_`{|}~-+.~0123456789");

	private static readonly SearchValues<char> s_asciiPunctuationChars = SearchValues.Create("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~");

	private static readonly SearchValues<char> s_asciiPunctuationCharsOrZero = SearchValues.Create("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~\0");

	private static readonly SearchValues<char> s_asciiPunctuationOrWhitespaceCharsOrZero = SearchValues.Create("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~\t\n\f\r \0");

	private static readonly SearchValues<char> s_escapableSymbolChars = SearchValues.Create("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~•");

	internal static readonly SearchValues<char> InvalidAutoLinkCharacters = SearchValues.Create("\u0001\u0002\u0003\u0004\u0005\u0006\a\b\t\n\v\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f <>\u007f");

	private static readonly string[] smallNumberStringCache = new string[27]
	{
		"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
		"10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
		"20", "21", "22", "23", "24", "25", "26"
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsPunctuationException(char c)
	{
		switch (c)
		{
		case '-':
		case '†':
		case '‡':
		case '−':
			return true;
		default:
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsPunctuationException(Rune c)
	{
		if (c.IsBmp)
		{
			return IsPunctuationException((char)c.Value);
		}
		return false;
	}

	public static void CheckOpenCloseDelimiter(char pc, char c, bool enableWithinWord, out bool canOpen, out bool canClose)
	{
		pc.CheckUnicodeCategory(out var space, out var punctuation);
		c.CheckUnicodeCategory(out var space2, out var punctuation2);
		CheckOpenCloseDelimiter(space, punctuation, punctuation && IsPunctuationException(pc), space2, punctuation2, punctuation2 && IsPunctuationException(c), enableWithinWord, out canOpen, out canClose);
	}

	internal static void CheckOpenCloseDelimiter(Rune pc, Rune c, bool enableWithinWord, out bool canOpen, out bool canClose)
	{
		pc.CheckUnicodeCategory(out var space, out var punctuation);
		c.CheckUnicodeCategory(out var space2, out var punctuation2);
		CheckOpenCloseDelimiter(space, punctuation, punctuation && IsPunctuationException(pc), space2, punctuation2, punctuation2 && IsPunctuationException(c), enableWithinWord, out canOpen, out canClose);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CheckOpenCloseDelimiter(bool prevIsWhiteSpace, bool prevIsPunctuation, bool prevIsExcepted, bool nextIsWhiteSpace, bool nextIsPunctuation, bool nextIsExcepted, bool enableWithinWord, out bool canOpen, out bool canClose)
	{
		canOpen = !nextIsWhiteSpace && (!nextIsPunctuation | nextIsExcepted | prevIsWhiteSpace | prevIsPunctuation);
		canClose = !prevIsWhiteSpace && (!prevIsPunctuation | prevIsExcepted | nextIsWhiteSpace | nextIsPunctuation);
		if (!enableWithinWord)
		{
			bool flag = canOpen;
			canOpen = canOpen && (!canClose | prevIsPunctuation);
			canClose = canClose && (!flag | nextIsPunctuation);
		}
	}

	internal static void CheckOpenCloseDelimiterCjkFriendly(Rune pc, Rune c, Rune twoPreviousRune, bool enableWithinWord, out bool canOpen, out bool canClose)
	{
		pc.CheckUnicodeCategory(out var space, out var punctuation);
		c.CheckUnicodeCategory(out var space2, out var punctuation2);
		if (space | space2)
		{
			canOpen = !space2;
			canClose = !space;
			return;
		}
		bool flag = false;
		Rune rune = pc;
		if (IsNonEmojiGeneralUseVariantSelector(pc))
		{
			flag = true;
			rune = twoPreviousRune;
			rune.CheckUnicodeCategory(out var _, out punctuation);
		}
		canOpen = punctuation;
		canClose = punctuation2;
		if (enableWithinWord)
		{
			bool num = IsCjk(rune) || (flag ? IsCjkAmbiousPunctuation(rune, pc) : IsIdeographicVariationSelector(rune));
			bool flag2 = IsCjk(c);
			bool flag3 = num | flag2;
			canOpen |= flag3 || !punctuation2;
			canClose |= flag3 || !punctuation;
		}
		static bool IsCjk(Rune r)
		{
			int value = r.Value;
			if (value >= 63744)
			{
				if (value >= 101760)
				{
					if (value >= 119552)
					{
						if (value >= 127536)
						{
							if (value >= 127584)
							{
								if (value >= 131072)
								{
									if (value <= 262141)
									{
										goto IL_0530;
									}
								}
								else if (value <= 127589)
								{
									goto IL_0530;
								}
							}
							else if (value >= 127552)
							{
								if (value <= 127560)
								{
									goto IL_0530;
								}
							}
							else if (value <= 127537 || value == 127543 || value == 127547)
							{
								goto IL_0530;
							}
						}
						else if (value >= 127504)
						{
							if (value >= 127515)
							{
								if (value <= 127534)
								{
									goto IL_0530;
								}
							}
							else if (value <= 127513)
							{
								goto IL_0530;
							}
						}
						else if (value >= 119648)
						{
							if (value <= 119670 || value == 127488 || value == 127490)
							{
								goto IL_0530;
							}
						}
						else if (value <= 119638)
						{
							goto IL_0530;
						}
					}
					else if (value >= 110592)
					{
						if (value >= 110948)
						{
							if (value >= 110960)
							{
								if (value <= 111355)
								{
									goto IL_0530;
								}
							}
							else if (value <= 110951)
							{
								goto IL_0530;
							}
						}
						else if (value >= 110928)
						{
							if (value <= 110930 || value == 110933)
							{
								goto IL_0530;
							}
						}
						else if (value <= 110882 || value == 110898)
						{
							goto IL_0530;
						}
					}
					else if (value >= 110581)
					{
						if (value >= 110589)
						{
							if (value <= 110590)
							{
								goto IL_0530;
							}
						}
						else if (value <= 110587)
						{
							goto IL_0530;
						}
					}
					else if (value >= 110576)
					{
						if (value <= 110579)
						{
							goto IL_0530;
						}
					}
					else if (value <= 101874)
					{
						goto IL_0530;
					}
				}
				else if (value >= 65490)
				{
					if (value >= 94176)
					{
						if (value >= 94208)
						{
							if (value >= 101631)
							{
								if (value <= 101662)
								{
									goto IL_0530;
								}
							}
							else if (value <= 101589)
							{
								goto IL_0530;
							}
						}
						else if (value >= 94192)
						{
							if (value <= 94198)
							{
								goto IL_0530;
							}
						}
						else if (value <= 94180)
						{
							goto IL_0530;
						}
					}
					else if (value >= 65504)
					{
						if (value >= 65512)
						{
							if (value <= 65518)
							{
								goto IL_0530;
							}
						}
						else if (value <= 65510)
						{
							goto IL_0530;
						}
					}
					else if (value >= 65498)
					{
						if (value <= 65500)
						{
							goto IL_0530;
						}
					}
					else if (value <= 65495)
					{
						goto IL_0530;
					}
				}
				else if (value >= 65128)
				{
					if (value >= 65474)
					{
						if (value >= 65482)
						{
							if (value <= 65487)
							{
								goto IL_0530;
							}
						}
						else if (value <= 65479)
						{
							goto IL_0530;
						}
					}
					else if (value >= 65281)
					{
						if (value <= 65470)
						{
							goto IL_0530;
						}
					}
					else if (value <= 65131)
					{
						goto IL_0530;
					}
				}
				else if (value >= 65072)
				{
					if (value >= 65108)
					{
						if (value <= 65126)
						{
							goto IL_0530;
						}
					}
					else if (value <= 65106)
					{
						goto IL_0530;
					}
				}
				else if (value >= 65040)
				{
					if (value <= 65049)
					{
						goto IL_0530;
					}
				}
				else if (value <= 64255)
				{
					goto IL_0530;
				}
			}
			else if (value >= 12783)
			{
				if (value >= 43360)
				{
					if (value >= 55216)
					{
						if (value >= 55243)
						{
							if (value <= 55291)
							{
								goto IL_0530;
							}
						}
						else if (value <= 55238)
						{
							goto IL_0530;
						}
					}
					else if (value >= 44032)
					{
						if (value <= 55203)
						{
							goto IL_0530;
						}
					}
					else if (value <= 43388)
					{
						goto IL_0530;
					}
				}
				else if (value >= 12880)
				{
					if (value >= 42128)
					{
						if (value <= 42182)
						{
							goto IL_0530;
						}
					}
					else if (value <= 42124)
					{
						goto IL_0530;
					}
				}
				else if (value >= 12832)
				{
					if (value <= 12871)
					{
						goto IL_0530;
					}
				}
				else if (value <= 12830)
				{
					goto IL_0530;
				}
			}
			else if (value >= 11931)
			{
				if (value >= 12441)
				{
					if (value >= 12593)
					{
						if (value >= 12688)
						{
							if (value <= 12773)
							{
								goto IL_0530;
							}
						}
						else if (value <= 12686)
						{
							goto IL_0530;
						}
					}
					else if (value >= 12549)
					{
						if (value <= 12591)
						{
							goto IL_0530;
						}
					}
					else if (value <= 12543)
					{
						goto IL_0530;
					}
				}
				else if (value >= 12272)
				{
					if (value >= 12353)
					{
						if (value <= 12438)
						{
							goto IL_0530;
						}
					}
					else if (value <= 12350)
					{
						goto IL_0530;
					}
				}
				else if (value >= 12032)
				{
					if (value <= 12245)
					{
						goto IL_0530;
					}
				}
				else if (value <= 12019)
				{
					goto IL_0530;
				}
			}
			else if (value >= 9866)
			{
				if (value >= 11904)
				{
					if (value <= 11929)
					{
						goto IL_0530;
					}
				}
				else if (value <= 9871)
				{
					goto IL_0530;
				}
			}
			else if (value >= 9001)
			{
				if (value >= 9776)
				{
					if (value <= 9783)
					{
						goto IL_0530;
					}
				}
				else if (value <= 9002)
				{
					goto IL_0530;
				}
			}
			else if (value >= 4352 && (value <= 4607 || value == 8361))
			{
				goto IL_0530;
			}
			return false;
			IL_0530:
			return true;
		}
		static bool IsCjkAmbiousPunctuation(Rune main, Rune vs)
		{
			bool flag4 = vs.Value == 65025;
			if (flag4)
			{
				int value = main.Value;
				bool flag5 = (((uint)(value - 8216) <= 1u || (uint)(value - 8220) <= 1u) ? true : false);
				flag4 = flag5;
			}
			return flag4;
		}
		static bool IsIdeographicVariationSelector(Rune r)
		{
			int value = r.Value;
			if (value >= 917760)
			{
				return value <= 917999;
			}
			return false;
		}
		static bool IsNonEmojiGeneralUseVariantSelector(Rune r)
		{
			int value = r.Value;
			if (value >= 65024)
			{
				return value <= 65038;
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRomanLetterPartial(char c)
	{
		if (!IsRomanLetterLowerPartial(c))
		{
			return IsRomanLetterUpperPartial(c);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRomanLetterLowerPartial(char c)
	{
		if (c != 'i' && c != 'v')
		{
			return c == 'x';
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRomanLetterUpperPartial(char c)
	{
		if (c != 'I' && c != 'V')
		{
			return c == 'X';
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int RomanToArabic(ReadOnlySpan<char> text)
	{
		int num = 0;
		for (int i = 0; i < text.Length; i++)
		{
			int num2 = RomanToArabic(text[i]);
			num = (((uint)(i + 1) >= text.Length || num2 >= RomanToArabic(text[i + 1])) ? (num + num2) : (num - num2));
		}
		return num;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int RomanToArabic(char c)
		{
			return (c | 0x20) switch
			{
				105 => 1, 
				118 => 5, 
				_ => 10, 
			};
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int AddTab(int column)
	{
		return 4 + (column & -4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAcrossTab(int column)
	{
		return (column & 3) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsWhitespace(this char c)
	{
		if (c < '\u00a0')
		{
			long num = 30399299632234496L << (int)c;
			ulong num2 = (ulong)c - 64uL;
			return (long)((ulong)num & num2) < 0L;
		}
		return IsWhitespaceRare(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsWhitespace(this Rune r)
	{
		if (r.IsBmp)
		{
			return ((char)r.Value).IsWhitespace();
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsWhiteSpaceOrZero(this char c)
	{
		if (c < '\u00a0')
		{
			long num = -9192972737222541312L << (int)c;
			ulong num2 = (ulong)c - 64uL;
			return (long)((ulong)num & num2) < 0L;
		}
		return IsWhitespaceRare(c);
	}

	private static bool IsWhitespaceRare(char c)
	{
		if (c < '\u1680')
		{
			return c == '\u00a0';
		}
		if (c <= '\u3000')
		{
			if (c != '\u1680' && !IsInInclusiveRange(c, 8192u, 8202u) && c != '\u202f' && c != '\u205f')
			{
				return c == '\u3000';
			}
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsControl(this char c)
	{
		return char.IsControl(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEscapableSymbol(this char c)
	{
		return s_escapableSymbolChars.Contains(c);
	}

	public static void CheckUnicodeCategory(this char c, out bool space, out bool punctuation)
	{
		if (c.IsWhitespace())
		{
			space = true;
			punctuation = false;
		}
		else if (c <= '\u007f')
		{
			space = c == '\0';
			punctuation = c.IsAsciiPunctuationOrZero();
		}
		else
		{
			space = false;
			punctuation = (0x1FFC0000 & (1 << (int)CharUnicodeInfo.GetUnicodeCategory(c))) != 0;
		}
	}

	internal static void CheckUnicodeCategory(this Rune r, out bool space, out bool punctuation)
	{
		if (r.IsWhitespace())
		{
			space = true;
			punctuation = false;
		}
		else if (r.Value <= 127)
		{
			space = r.Value == 0;
			punctuation = r.IsBmp && ((char)r.Value).IsAsciiPunctuationOrZero();
		}
		else
		{
			space = false;
			punctuation = (0x1FFC0000 & (1 << (int)Rune.GetUnicodeCategory(r))) != 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsSpaceOrPunctuationForGFMAutoLink(char c)
	{
		if (c <= '\u007f')
		{
			return s_asciiPunctuationOrWhitespaceCharsOrZero.Contains(c);
		}
		return NonAscii(c);
		static bool NonAscii(char ch)
		{
			return (0x1FC0800 & (1 << (int)CharUnicodeInfo.GetUnicodeCategory(ch))) != 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNewLineOrLineFeed(this char c)
	{
		if (c != '\n')
		{
			return c == '\r';
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsZero(this char c)
	{
		return c == '\0';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSpace(this char c)
	{
		return c == ' ';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsTab(this char c)
	{
		return c == '\t';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSpaceOrTab(this char c)
	{
		if (!c.IsSpace())
		{
			return c.IsTab();
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static char EscapeInsecure(this char c)
	{
		if (c != 0)
		{
			return c;
		}
		return '\ufffd';
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAlphaUpper(this char c)
	{
		return (uint)(c - 65) <= 25u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAlpha(this char c)
	{
		return (uint)((c - 65) & -33) <= 25u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAlphaNumeric(this char c)
	{
		if (!c.IsAlpha())
		{
			return c.IsDigit();
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDigit(this char c)
	{
		return (uint)(c - 48) <= 9u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsAsciiPunctuationOrZero(this char c)
	{
		return s_asciiPunctuationCharsOrZero.Contains(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAsciiPunctuation(this char c)
	{
		return s_asciiPunctuationChars.Contains(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEmailUsernameSpecialChar(char c)
	{
		return s_emailUsernameSpecialChar.Contains(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsEmailUsernameSpecialCharOrDigit(char c)
	{
		return s_emailUsernameSpecialCharOrDigit.Contains(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsHighSurrogate(char c)
	{
		return char.IsHighSurrogate(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLowSurrogate(char c)
	{
		return char.IsLowSurrogate(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsInInclusiveRange(int value, uint min, uint max)
	{
		return (uint)(value - (int)min) <= max - min;
	}

	public static bool IsRightToLeft(int c)
	{
		if ((c < 1488 || c > 1514) && (c < 1520 || c > 1524) && (c < 1569 || c > 1594) && (c < 1600 || c > 1610) && (c < 1645 || c > 1647) && (c < 1649 || c > 1749) && (c < 1765 || c > 1766) && (c < 1786 || c > 1790) && (c < 1792 || c > 1805) && (c < 1810 || c > 1836) && (c < 1920 || c > 1957) && (c < 64287 || c > 64296) && (c < 64298 || c > 64310) && (c < 64312 || c > 64316) && (c < 64320 || c > 64321) && (c < 64323 || c > 64324) && (c < 64326 || c > 64433) && (c < 64467 || c > 64829) && (c < 64848 || c > 64911) && (c < 64914 || c > 64967) && (c < 65008 || c > 65020) && (c < 65136 || c > 65140) && (c < 65142 || c > 65276) && c != 1470 && c != 1472 && c != 1475 && c != 1563 && c != 1567 && c != 1757 && c != 1808 && c != 1969 && c != 8207 && c != 64285)
		{
			return c == 64318;
		}
		return true;
	}

	public static bool IsLeftToRight(int c)
	{
		if ((c < 65 || c > 90) && (c < 97 || c > 122) && (c < 192 || c > 214) && (c < 216 || c > 246) && (c < 248 || c > 544) && (c < 546 || c > 563) && (c < 592 || c > 685) && (c < 688 || c > 696) && (c < 699 || c > 705) && (c < 720 || c > 721) && (c < 736 || c > 740) && (c < 904 || c > 906) && (c < 910 || c > 929) && (c < 931 || c > 974) && (c < 976 || c > 1013) && (c < 1024 || c > 1154) && (c < 1162 || c > 1230) && (c < 1232 || c > 1269) && (c < 1272 || c > 1273) && (c < 1280 || c > 1295) && (c < 1329 || c > 1366) && (c < 1369 || c > 1375) && (c < 1377 || c > 1415) && (c < 2309 || c > 2361) && (c < 2365 || c > 2368) && (c < 2377 || c > 2380) && (c < 2392 || c > 2401) && (c < 2404 || c > 2416) && (c < 2434 || c > 2435) && (c < 2437 || c > 2444) && (c < 2447 || c > 2448) && (c < 2451 || c > 2472) && (c < 2474 || c > 2480) && (c < 2486 || c > 2489) && (c < 2494 || c > 2496) && (c < 2503 || c > 2504) && (c < 2507 || c > 2508) && (c < 2524 || c > 2525) && (c < 2527 || c > 2529) && (c < 2534 || c > 2545) && (c < 2548 || c > 2554) && (c < 2565 || c > 2570) && (c < 2575 || c > 2576) && (c < 2579 || c > 2600) && (c < 2602 || c > 2608) && (c < 2610 || c > 2611) && (c < 2613 || c > 2614) && (c < 2616 || c > 2617) && (c < 2622 || c > 2624) && (c < 2649 || c > 2652) && (c < 2662 || c > 2671) && (c < 2674 || c > 2676) && (c < 2693 || c > 2699) && (c < 2703 || c > 2705) && (c < 2707 || c > 2728) && (c < 2730 || c > 2736) && (c < 2738 || c > 2739) && (c < 2741 || c > 2745) && (c < 2749 || c > 2752) && (c < 2763 || c > 2764) && (c < 2790 || c > 2799) && (c < 2818 || c > 2819) && (c < 2821 || c > 2828) && (c < 2831 || c > 2832) && (c < 2835 || c > 2856) && (c < 2858 || c > 2864) && (c < 2866 || c > 2867) && (c < 2870 || c > 2873) && (c < 2877 || c > 2878) && (c < 2887 || c > 2888) && (c < 2891 || c > 2892) && (c < 2908 || c > 2909) && (c < 2911 || c > 2913) && (c < 2918 || c > 2928) && (c < 2949 || c > 2954) && (c < 2958 || c > 2960) && (c < 2962 || c > 2965) && (c < 2969 || c > 2970) && (c < 2974 || c > 2975) && (c < 2979 || c > 2980) && (c < 2984 || c > 2986) && (c < 2990 || c > 2997) && (c < 2999 || c > 3001) && (c < 3006 || c > 3007) && (c < 3009 || c > 3010) && (c < 3014 || c > 3016) && (c < 3018 || c > 3020) && (c < 3047 || c > 3058) && (c < 3073 || c > 3075) && (c < 3077 || c > 3084) && (c < 3086 || c > 3088) && (c < 3090 || c > 3112) && (c < 3114 || c > 3123) && (c < 3125 || c > 3129) && (c < 3137 || c > 3140) && (c < 3168 || c > 3169) && (c < 3174 || c > 3183) && (c < 3202 || c > 3203) && (c < 3205 || c > 3212) && (c < 3214 || c > 3216) && (c < 3218 || c > 3240) && (c < 3242 || c > 3251) && (c < 3253 || c > 3257) && (c < 3264 || c > 3268) && (c < 3271 || c > 3272) && (c < 3274 || c > 3275) && (c < 3285 || c > 3286) && (c < 3296 || c > 3297) && (c < 3302 || c > 3311) && (c < 3330 || c > 3331) && (c < 3333 || c > 3340) && (c < 3342 || c > 3344) && (c < 3346 || c > 3368) && (c < 3370 || c > 3385) && (c < 3390 || c > 3392) && (c < 3398 || c > 3400) && (c < 3402 || c > 3404) && (c < 3424 || c > 3425) && (c < 3430 || c > 3439) && (c < 3458 || c > 3459) && (c < 3461 || c > 3478) && (c < 3482 || c > 3505) && (c < 3507 || c > 3515) && (c < 3520 || c > 3526) && (c < 3535 || c > 3537) && (c < 3544 || c > 3551) && (c < 3570 || c > 3572) && (c < 3585 || c > 3632) && (c < 3634 || c > 3635) && (c < 3648 || c > 3654) && (c < 3663 || c > 3675) && (c < 3713 || c > 3714) && (c < 3719 || c > 3720) && (c < 3732 || c > 3735) && (c < 3737 || c > 3743) && (c < 3745 || c > 3747) && (c < 3754 || c > 3755) && (c < 3757 || c > 3760) && (c < 3762 || c > 3763) && (c < 3776 || c > 3780) && (c < 3792 || c > 3801) && (c < 3804 || c > 3805) && (c < 3840 || c > 3863) && (c < 3866 || c > 3892) && (c < 3902 || c > 3911) && (c < 3913 || c > 3946) && (c < 3976 || c > 3979) && (c < 4030 || c > 4037) && (c < 4039 || c > 4044) && (c < 4096 || c > 4129) && (c < 4131 || c > 4135) && (c < 4137 || c > 4138) && (c < 4160 || c > 4183) && (c < 4256 || c > 4293) && (c < 4304 || c > 4344) && (c < 4352 || c > 4441) && (c < 4447 || c > 4514) && (c < 4520 || c > 4601) && (c < 4608 || c > 4614) && (c < 4616 || c > 4678) && (c < 4682 || c > 4685) && (c < 4688 || c > 4694) && (c < 4698 || c > 4701) && (c < 4704 || c > 4742) && (c < 4746 || c > 4749) && (c < 4752 || c > 4782) && (c < 4786 || c > 4789) && (c < 4792 || c > 4798) && (c < 4802 || c > 4805) && (c < 4808 || c > 4814) && (c < 4816 || c > 4822) && (c < 4824 || c > 4846) && (c < 4848 || c > 4878) && (c < 4882 || c > 4885) && (c < 4888 || c > 4894) && (c < 4896 || c > 4934) && (c < 4936 || c > 4954) && (c < 4961 || c > 4988) && (c < 5024 || c > 5108) && (c < 5121 || c > 5750) && (c < 5761 || c > 5786) && (c < 5792 || c > 5872) && (c < 5888 || c > 5900) && (c < 5902 || c > 5905) && (c < 5920 || c > 5937) && (c < 5941 || c > 5942) && (c < 5952 || c > 5969) && (c < 5984 || c > 5996) && (c < 5998 || c > 6000) && (c < 6016 || c > 6070) && (c < 6078 || c > 6085) && (c < 6087 || c > 6088) && (c < 6100 || c > 6106) && (c < 6112 || c > 6121) && (c < 6160 || c > 6169) && (c < 6176 || c > 6263) && (c < 6272 || c > 6312) && (c < 7680 || c > 7835) && (c < 7840 || c > 7929) && (c < 7936 || c > 7957) && (c < 7960 || c > 7965) && (c < 7968 || c > 8005) && (c < 8008 || c > 8013) && (c < 8016 || c > 8023) && (c < 8031 || c > 8061) && (c < 8064 || c > 8116) && (c < 8118 || c > 8124) && (c < 8130 || c > 8132) && (c < 8134 || c > 8140) && (c < 8144 || c > 8147) && (c < 8150 || c > 8155) && (c < 8160 || c > 8172) && (c < 8178 || c > 8180) && (c < 8182 || c > 8188) && (c < 8458 || c > 8467) && (c < 8473 || c > 8477) && (c < 8490 || c > 8493) && (c < 8495 || c > 8497) && (c < 8499 || c > 8505) && (c < 8509 || c > 8511) && (c < 8517 || c > 8521) && (c < 8544 || c > 8579) && (c < 9014 || c > 9082) && (c < 9372 || c > 9449) && (c < 12293 || c > 12295) && (c < 12321 || c > 12329) && (c < 12337 || c > 12341) && (c < 12344 || c > 12348) && (c < 12353 || c > 12438) && (c < 12445 || c > 12447) && (c < 12449 || c > 12538) && (c < 12540 || c > 12543) && (c < 12549 || c > 12588) && (c < 12593 || c > 12686) && (c < 12688 || c > 12727) && (c < 12784 || c > 12828) && (c < 12832 || c > 12867) && (c < 12896 || c > 12923) && (c < 12927 || c > 12976) && (c < 12992 || c > 13003) && (c < 13008 || c > 13054) && (c < 13056 || c > 13174) && (c < 13179 || c > 13277) && (c < 13280 || c > 13310) && (c < 13312 || c > 19893) && (c < 19968 || c > 40869) && (c < 40960 || c > 42124) && (c < 44032 || c > 55203) && (c < 55296 || c > 64045) && (c < 64048 || c > 64106) && (c < 64256 || c > 64262) && (c < 64275 || c > 64279) && (c < 65313 || c > 65338) && (c < 65345 || c > 65370) && (c < 65382 || c > 65470) && (c < 65474 || c > 65479) && (c < 65482 || c > 65487) && (c < 65490 || c > 65495) && (c < 65498 || c > 65500) && (c < 66304 || c > 66334) && (c < 66336 || c > 66339) && (c < 66352 || c > 66378) && (c < 66560 || c > 66597) && (c < 66600 || c > 66637) && (c < 118784 || c > 119029) && (c < 119040 || c > 119078) && (c < 119082 || c > 119142) && (c < 119146 || c > 119154) && (c < 119171 || c > 119172) && (c < 119180 || c > 119209) && (c < 119214 || c > 119261) && (c < 119808 || c > 119892) && (c < 119894 || c > 119964) && (c < 119966 || c > 119967) && (c < 119973 || c > 119974) && (c < 119977 || c > 119980) && (c < 119982 || c > 119993) && (c < 119997 || c > 120000) && (c < 120002 || c > 120003) && (c < 120005 || c > 120069) && (c < 120071 || c > 120074) && (c < 120077 || c > 120084) && (c < 120086 || c > 120092) && (c < 120094 || c > 120121) && (c < 120123 || c > 120126) && (c < 120128 || c > 120132) && (c < 120138 || c > 120144) && (c < 120146 || c > 120483) && (c < 120488 || c > 120777) && (c < 131072 || c > 173782) && (c < 194560 || c > 195101) && (c < 983040 || c > 1048573) && (c < 1048576 || c > 1114109) && c != 170 && c != 181 && c != 186 && c != 750 && c != 890 && c != 902 && c != 908 && c != 1417 && c != 2307 && c != 2384 && c != 2482 && c != 2519 && c != 2654 && c != 2691 && c != 2701 && c != 2761 && c != 2768 && c != 2784 && c != 2880 && c != 2903 && c != 2947 && c != 2972 && c != 3031 && c != 3262 && c != 3294 && c != 3415 && c != 3517 && c != 3716 && c != 3722 && c != 3725 && c != 3749 && c != 3751 && c != 3773 && c != 3782 && c != 3894 && c != 3896 && c != 3967 && c != 3973 && c != 4047 && c != 4140 && c != 4145 && c != 4152 && c != 4347 && c != 4680 && c != 4696 && c != 4744 && c != 4784 && c != 4800 && c != 4880 && c != 6108 && c != 8025 && c != 8027 && c != 8029 && c != 8126 && c != 8206 && c != 8305 && c != 8319 && c != 8450 && c != 8455 && c != 8469 && c != 8484 && c != 8486 && c != 8488 && c != 9109 && c != 119970 && c != 119995)
		{
			return c == 120134;
		}
		return true;
	}

	internal static string SmallNumberToString(int number)
	{
		string[] array = smallNumberStringCache;
		if ((uint)number < (uint)array.Length)
		{
			return array[number];
		}
		return number.ToString(CultureInfo.InvariantCulture);
	}
}
