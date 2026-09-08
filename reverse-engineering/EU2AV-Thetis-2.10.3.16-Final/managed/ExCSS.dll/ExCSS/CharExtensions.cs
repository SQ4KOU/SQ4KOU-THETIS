namespace ExCSS;

internal static class CharExtensions
{
	public static int FromHex(this char c)
	{
		if (!c.IsDigit())
		{
			return c - (c.IsLowercaseAscii() ? 87 : 55);
		}
		return c - 48;
	}

	public static string ToHex(this char character)
	{
		int num = character;
		return num.ToString("x");
	}

	public static bool IsInRange(this char c, int lower, int upper)
	{
		if (c >= lower)
		{
			return c <= upper;
		}
		return false;
	}

	public static bool IsNormalQueryCharacter(this char c)
	{
		if (c.IsInRange(33, 126) && c != '"' && c != '`' && c != '#' && c != '<')
		{
			return c != '>';
		}
		return false;
	}

	public static bool IsNormalPathCharacter(this char c)
	{
		if (c.IsInRange(32, 126) && c != '"' && c != '`' && c != '#' && c != '<' && c != '>' && c != ' ')
		{
			return c != '?';
		}
		return false;
	}

	public static bool IsUppercaseAscii(this char c)
	{
		if (c >= 'A')
		{
			return c <= 'Z';
		}
		return false;
	}

	public static bool IsLowercaseAscii(this char c)
	{
		if (c >= 'a')
		{
			return c <= 'z';
		}
		return false;
	}

	public static bool IsAlphanumericAscii(this char c)
	{
		if (!c.IsDigit() && !c.IsUppercaseAscii())
		{
			return c.IsLowercaseAscii();
		}
		return true;
	}

	public static bool IsHex(this char c)
	{
		if (!c.IsDigit() && (c < 'A' || c > 'F'))
		{
			if (c >= 'a')
			{
				return c <= 'f';
			}
			return false;
		}
		return true;
	}

	public static bool IsNonAscii(this char c)
	{
		if (c != '\uffff')
		{
			return c >= '\u0080';
		}
		return false;
	}

	public static bool IsNonPrintable(this char c)
	{
		if (c >= '\u007f')
		{
			if (c < '\u00a0')
			{
				goto IL_001f;
			}
		}
		else if (c >= '\u000e')
		{
			if (c <= '\u001f')
			{
				goto IL_001f;
			}
		}
		else if (c <= '\b')
		{
			goto IL_001f;
		}
		return false;
		IL_001f:
		return true;
	}

	public static bool IsLetter(this char c)
	{
		if (!c.IsUppercaseAscii())
		{
			return c.IsLowercaseAscii();
		}
		return true;
	}

	public static bool IsName(this char c)
	{
		if (!c.IsNonAscii() && !c.IsLetter() && c != '_' && c != '-')
		{
			return c.IsDigit();
		}
		return true;
	}

	public static bool IsNameStart(this char c)
	{
		if (!c.IsNonAscii() && !c.IsUppercaseAscii() && !c.IsLowercaseAscii())
		{
			return c == '_';
		}
		return true;
	}

	public static bool IsLineBreak(this char c)
	{
		if (c == '\n' || c == '\r')
		{
			return true;
		}
		return false;
	}

	public static bool IsSpaceCharacter(this char c)
	{
		switch (c)
		{
		case '\t':
		case '\n':
		case '\f':
		case '\r':
		case ' ':
			return true;
		default:
			return false;
		}
	}

	public static bool IsDigit(this char c)
	{
		if (c >= '0')
		{
			return c <= '9';
		}
		return false;
	}

	public static bool IsInvalid(this int c)
	{
		if (c != 0 && c <= 1114111)
		{
			if (c > 55296)
			{
				return c < 57343;
			}
			return false;
		}
		return true;
	}

	public static bool IsOneOf(this char c, char a, char b)
	{
		if (a != c)
		{
			return b == c;
		}
		return true;
	}

	public static bool IsOneOf(this char c, char o1, char o2, char o3)
	{
		if (c != o1 && c != o2)
		{
			return c == o3;
		}
		return true;
	}

	public static bool IsOneOf(this char c, char o1, char o2, char o3, char o4)
	{
		if (c != o1 && c != o2 && c != o3)
		{
			return c == o4;
		}
		return true;
	}
}
