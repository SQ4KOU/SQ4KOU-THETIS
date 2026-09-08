using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ExCSS;

internal static class StringExtensions
{
	public static bool Has(this string value, char chr, int index = 0)
	{
		if (value != null && value.Length > index)
		{
			return value[index] == chr;
		}
		return false;
	}

	public static bool Contains(this string[] list, string element, StringComparison comparison = StringComparison.Ordinal)
	{
		return list.Any((string t) => t.Equals(element, comparison));
	}

	public static bool Is(this string current, string other)
	{
		return string.Equals(current, other, StringComparison.Ordinal);
	}

	public static bool Isi(this string current, string other)
	{
		return string.Equals(current, other, StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsOneOf(this string element, string item1, string item2)
	{
		if (!element.Is(item1))
		{
			return element.Is(item2);
		}
		return true;
	}

	public static string StylesheetString(this string value)
	{
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		stringBuilder.Append('"');
		if (!string.IsNullOrEmpty(value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				switch (c)
				{
				case '\0':
					throw new ParseException("Unable to parse null symbol");
				case '"':
				case '\\':
					stringBuilder.Append('\\').Append(c);
					continue;
				}
				if (c.IsInRange(1, 31) || c == '{')
				{
					stringBuilder.Append('\\').Append(c.ToHex()).Append((i + 1 != value.Length) ? " " : "");
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
		}
		stringBuilder.Append('"');
		return stringBuilder.ToPool();
	}

	public static string StylesheetFunction(this string value, string argument)
	{
		return value + "(" + argument + ")";
	}

	public static string StylesheetUrl(this string value)
	{
		string argument = value.StylesheetString();
		return FunctionNames.Url.StylesheetFunction(argument);
	}

	public static string StylesheetUnit(this string value, out float result)
	{
		if (!string.IsNullOrEmpty(value))
		{
			int num = value.Length;
			while (!value[num - 1].IsDigit() && --num > 0)
			{
			}
			bool flag = float.TryParse(value.Substring(0, num), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			if ((num > 0) & flag)
			{
				return value.Substring(num);
			}
		}
		result = 0f;
		return null;
	}
}
