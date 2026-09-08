using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class GeneratedNameParser
{
	public const char FileTypeNameStartChar = '<';

	private const int sha256LengthBytes = 32;

	private const int sha256LengthHexChars = 64;

	private static readonly string s_regexPatternString;

	private static readonly Regex s_fileTypeOrdinalPattern;

	internal static bool IsSynthesizedLocalName(string name)
	{
		return name.StartsWith("CS$", StringComparison.Ordinal);
	}

	internal static GeneratedNameKind GetKind(string name)
	{
		if (!TryParseGeneratedName(name, out var kind, out var _, out var _))
		{
			return GeneratedNameKind.None;
		}
		return kind;
	}

	internal static bool TryParseGeneratedName(string name, out GeneratedNameKind kind, out int openBracketOffset, out int closeBracketOffset)
	{
		openBracketOffset = -1;
		if (name.StartsWith("CS$<", StringComparison.Ordinal))
		{
			openBracketOffset = 3;
		}
		else if (name.StartsWith("<", StringComparison.Ordinal))
		{
			openBracketOffset = 0;
		}
		if (openBracketOffset >= 0)
		{
			closeBracketOffset = IndexOfBalancedParenthesis(name, openBracketOffset, '>');
			if (closeBracketOffset >= 0 && closeBracketOffset + 1 < name.Length)
			{
				int num = name[closeBracketOffset + 1];
				bool flag;
				switch (num)
				{
				case 49:
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
				case 56:
				case 57:
				case 65:
				case 66:
				case 67:
				case 68:
				case 69:
				case 70:
				case 71:
				case 72:
				case 73:
				case 74:
				case 75:
				case 76:
				case 77:
				case 78:
				case 79:
				case 80:
				case 81:
				case 82:
				case 83:
				case 84:
				case 85:
				case 86:
				case 87:
				case 88:
				case 89:
				case 90:
				case 97:
				case 98:
				case 99:
				case 100:
				case 101:
				case 102:
				case 103:
				case 104:
				case 105:
				case 106:
				case 107:
				case 108:
				case 109:
				case 110:
				case 111:
				case 112:
				case 113:
				case 114:
				case 115:
				case 116:
				case 117:
				case 118:
				case 119:
				case 120:
				case 121:
				case 122:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					kind = (GeneratedNameKind)num;
					return true;
				}
			}
		}
		kind = GeneratedNameKind.None;
		openBracketOffset = -1;
		closeBracketOffset = -1;
		return false;
	}

	private static int IndexOfBalancedParenthesis(string str, int openingOffset, char closing)
	{
		char c = str[openingOffset];
		int num = 1;
		for (int i = openingOffset + 1; i < str.Length; i++)
		{
			char c2 = str[i];
			if (c2 == c)
			{
				num++;
			}
			else if (c2 == closing)
			{
				num--;
				if (num == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	internal static bool TryParseSourceMethodNameFromGeneratedName(string generatedName, GeneratedNameKind requiredKind, [NotNullWhen(true)] out string? methodName)
	{
		if (!TryParseGeneratedName(generatedName, out var kind, out var openBracketOffset, out var closeBracketOffset))
		{
			methodName = null;
			return false;
		}
		if (requiredKind != GeneratedNameKind.None && kind != requiredKind)
		{
			methodName = null;
			return false;
		}
		methodName = generatedName.Substring(openBracketOffset + 1, closeBracketOffset - openBracketOffset - 1);
		if (kind.IsTypeName())
		{
			methodName = methodName.Replace('-', '.');
		}
		return true;
	}

	internal static bool TryParseLocalFunctionName(string generatedName, [NotNullWhen(true)] out string? localFunctionName)
	{
		localFunctionName = null;
		if (!TryParseGeneratedName(generatedName, out var kind, out var _, out var closeBracketOffset) || kind != GeneratedNameKind.LocalFunction)
		{
			return false;
		}
		int num = closeBracketOffset + 2 + "__".Length;
		if (num >= generatedName.Length)
		{
			return false;
		}
		int num2 = generatedName.IndexOf('|', num);
		if (num2 < 0)
		{
			return false;
		}
		localFunctionName = generatedName.Substring(num, num2 - num);
		return true;
	}

	internal static bool TryParseSlotIndex(string fieldName, out int slotIndex)
	{
		int num = fieldName.LastIndexOf('_');
		if (num - 1 < 0 || num == fieldName.Length || fieldName[num - 1] != '_')
		{
			slotIndex = -1;
			return false;
		}
		if (int.TryParse(fieldName.Substring(num + 1), NumberStyles.None, CultureInfo.InvariantCulture, out slotIndex) && slotIndex >= 1)
		{
			slotIndex--;
			return true;
		}
		slotIndex = -1;
		return false;
	}

	internal static bool TryParseAnonymousTypeParameterName(string typeParameterName, [NotNullWhen(true)] out string? propertyName)
	{
		if (typeParameterName.StartsWith("<", StringComparison.Ordinal) && typeParameterName.EndsWith(">j__TPar", StringComparison.Ordinal))
		{
			propertyName = typeParameterName.Substring(1, typeParameterName.Length - 9);
			return true;
		}
		propertyName = null;
		return false;
	}

	internal static bool TryParsePrimaryConstructorParameterFieldName(string fieldName, [NotNullWhen(true)] out string? parameterName)
	{
		if (fieldName.StartsWith("<", StringComparison.Ordinal) && fieldName.EndsWith(">P", StringComparison.Ordinal))
		{
			parameterName = fieldName.Substring(1, fieldName.Length - 3);
			return true;
		}
		parameterName = null;
		return false;
	}

	static GeneratedNameParser()
	{
		s_regexPatternString = $"<([a-zA-Z_0-9]*)>F([0-9A-F]{{{64}}})__";
		s_fileTypeOrdinalPattern = new Regex(s_regexPatternString, RegexOptions.Compiled);
	}

	internal static bool TryParseFileTypeName(string generatedName, [NotNullWhen(true)] out string? displayFileName, [NotNullWhen(true)] out byte[]? checksum, [NotNullWhen(true)] out string? originalTypeName)
	{
		Match match = s_fileTypeOrdinalPattern.Match(generatedName);
		if (match != null && match.Success)
		{
			GroupCollection groups = match.Groups;
			int index = match.Index;
			int length = match.Length;
			displayFileName = groups[1].Value;
			string value = groups[2].Value;
			byte[] array = new byte[32];
			for (int i = 0; i < 32; i++)
			{
				array[i] = (byte)((hexCharToByte(value[i * 2]) << 4) | hexCharToByte(value[i * 2 + 1]));
			}
			checksum = array;
			int startIndex = index + length;
			originalTypeName = generatedName.Substring(startIndex);
			return true;
		}
		checksum = null;
		displayFileName = null;
		originalTypeName = null;
		return false;
		static byte hexCharToByte(char c)
		{
			switch (c)
			{
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				return (byte)(c - 48);
			case 'A':
			case 'B':
			case 'C':
			case 'D':
			case 'E':
			case 'F':
				return (byte)(10 + c - 65);
			default:
				return @throw(c);
			}
		}
		static byte @throw(char c)
		{
			throw ExceptionUtilities.UnexpectedValue(c);
		}
	}
}
