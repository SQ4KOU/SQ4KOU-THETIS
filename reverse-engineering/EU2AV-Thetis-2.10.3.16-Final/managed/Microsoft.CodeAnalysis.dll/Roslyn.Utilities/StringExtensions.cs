using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Roslyn.Utilities;

internal static class StringExtensions
{
	private static readonly Func<char, char> s_toLower = char.ToLower;

	private static readonly Func<char, char> s_toUpper = char.ToUpper;

	private static ImmutableArray<string> s_lazyNumerals;

	private static UTF8Encoding? s_lazyUtf8;

	private const string AttributeSuffix = "Attribute";

	internal static string GetNumeral(int number)
	{
		ImmutableArray<string> value = s_lazyNumerals;
		if (value.IsDefault)
		{
			value = ImmutableArray.Create(new ReadOnlySpan<string>(new string[10] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }));
			ImmutableInterlocked.InterlockedInitialize(ref s_lazyNumerals, value);
		}
		if (number >= value.Length)
		{
			return number.ToString(CultureInfo.InvariantCulture);
		}
		return value[number];
	}

	public static string Join(this IEnumerable<string?> source, string separator)
	{
		return string.Join(separator, source);
	}

	public static bool LooksLikeInterfaceName(this string name)
	{
		if (name.Length >= 3 && name[0] == 'I' && char.IsUpper(name[1]))
		{
			return char.IsLower(name[2]);
		}
		return false;
	}

	public static bool LooksLikeTypeParameterName(this string name)
	{
		if (name.Length >= 3 && name[0] == 'T' && char.IsUpper(name[1]))
		{
			return char.IsLower(name[2]);
		}
		return false;
	}

	[return: NotNullIfNotNull("shortName")]
	public static string? ToPascalCase(this string? shortName, bool trimLeadingTypePrefix = true)
	{
		return shortName.ConvertCase(trimLeadingTypePrefix, s_toUpper);
	}

	[return: NotNullIfNotNull("shortName")]
	public static string? ToCamelCase(this string? shortName, bool trimLeadingTypePrefix = true)
	{
		return shortName.ConvertCase(trimLeadingTypePrefix, s_toLower);
	}

	[return: NotNullIfNotNull("shortName")]
	private static string? ConvertCase(this string? shortName, bool trimLeadingTypePrefix, Func<char, char> convert)
	{
		if (!RoslynString.IsNullOrEmpty(shortName))
		{
			if (trimLeadingTypePrefix && (shortName.LooksLikeInterfaceName() || shortName.LooksLikeTypeParameterName()))
			{
				return convert(shortName[1]) + shortName.Substring(2);
			}
			if (convert(shortName[0]) != shortName[0])
			{
				return convert(shortName[0]) + shortName.Substring(1);
			}
		}
		return shortName;
	}

	internal static bool IsValidClrTypeName([NotNullWhen(true)] this string? name)
	{
		if (!RoslynString.IsNullOrEmpty(name))
		{
			return name.IndexOf('\0') == -1;
		}
		return false;
	}

	internal static bool IsValidClrNamespaceName([NotNullWhen(true)] this string? name)
	{
		if (RoslynString.IsNullOrEmpty(name))
		{
			return false;
		}
		char c = '.';
		foreach (char c2 in name)
		{
			if (c2 == '\0' || (c2 == '.' && c == '.'))
			{
				return false;
			}
			c = c2;
		}
		return c != '.';
	}

	internal static string GetWithSingleAttributeSuffix(this string name, bool isCaseSensitive)
	{
		string text = name;
		while ((text = text.GetWithoutAttributeSuffix(isCaseSensitive)) != null)
		{
			name = text;
		}
		return name + "Attribute";
	}

	internal static bool TryGetWithoutAttributeSuffix(this string name, [NotNullWhen(true)] out string? result)
	{
		return name.TryGetWithoutAttributeSuffix(isCaseSensitive: true, out result);
	}

	internal static string? GetWithoutAttributeSuffix(this string? name, bool isCaseSensitive)
	{
		if (!name.TryGetWithoutAttributeSuffix(isCaseSensitive, out string result))
		{
			return null;
		}
		return result;
	}

	internal static bool TryGetWithoutAttributeSuffix(this string? name, bool isCaseSensitive, [NotNullWhen(true)] out string? result)
	{
		if (name.HasAttributeSuffix(isCaseSensitive))
		{
			int length = "Attribute".Length;
			result = name.Substring(0, name.Length - length);
			return true;
		}
		result = null;
		return false;
	}

	internal static bool HasAttributeSuffix([NotNullWhen(true)] this string? name, bool isCaseSensitive)
	{
		if (name == null)
		{
			return false;
		}
		StringComparison comparisonType = (isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
		if (name.Length > "Attribute".Length)
		{
			return name.EndsWith("Attribute", comparisonType);
		}
		return false;
	}

	internal static bool IsValidUnicodeString(this string str)
	{
		int num = 0;
		while (num < str.Length)
		{
			char c = str[num++];
			if (char.IsHighSurrogate(c))
			{
				if (num >= str.Length || !char.IsLowSurrogate(str[num]))
				{
					return false;
				}
				num++;
			}
			else if (char.IsLowSurrogate(c))
			{
				return false;
			}
		}
		return true;
	}

	internal static string Unquote(this string arg)
	{
		bool quoted;
		return arg.Unquote(out quoted);
	}

	internal static string Unquote(this string arg, out bool quoted)
	{
		if (arg.Length > 1 && arg[0] == '"' && arg[arg.Length - 1] == '"')
		{
			quoted = true;
			return arg.Substring(1, arg.Length - 2);
		}
		quoted = false;
		return arg;
	}

	public static int GetCaseInsensitivePrefixLength(this string string1, string string2)
	{
		int i;
		for (i = 0; i < string1.Length && i < string2.Length && char.ToUpper(string1[i]) == char.ToUpper(string2[i]); i++)
		{
		}
		return i;
	}

	public static int GetCaseSensitivePrefixLength(this string string1, string string2)
	{
		int i;
		for (i = 0; i < string1.Length && i < string2.Length && string1[i] == string2[i]; i++)
		{
		}
		return i;
	}

	internal static bool TryGetUtf8ByteRepresentation(this string s, [NotNullWhen(true)] out byte[]? result, [NotNullWhen(false)] out string? error)
	{
		if (s_lazyUtf8 == null)
		{
			s_lazyUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
		}
		try
		{
			result = s_lazyUtf8.GetBytes(s);
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			result = null;
			error = ex.Message;
			return false;
		}
	}
}
