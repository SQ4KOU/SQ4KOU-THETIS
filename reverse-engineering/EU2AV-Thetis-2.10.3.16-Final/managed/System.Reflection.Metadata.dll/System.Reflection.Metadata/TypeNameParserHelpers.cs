using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace System.Reflection.Metadata;

internal static class TypeNameParserHelpers
{
	internal const int SZArray = -1;

	internal const int Pointer = -2;

	internal const int ByRef = -3;

	private const char EscapeCharacter = '\\';

	internal static int GetFullTypeNameLength(ReadOnlySpan<char> input, out bool isNestedType)
	{
		isNestedType = false;
		int num = GetUnescapedOffset(input, 0);
		isNestedType = num > 0 && num < input.Length && input[num] == '+';
		return num;
		static int GetUnescapedOffset(ReadOnlySpan<char> readOnlySpan, int startOffset)
		{
			int i;
			for (i = startOffset; i < readOnlySpan.Length; i++)
			{
				char c = readOnlySpan[i];
				if (c == '\\')
				{
					i++;
					if (i == readOnlySpan.Length || !NeedsEscaping(readOnlySpan[i]))
					{
						return -1;
					}
				}
				else if (NeedsEscaping(c))
				{
					break;
				}
			}
			return i;
		}
		static bool NeedsEscaping(char c)
		{
			switch (c)
			{
			case '&':
			case '*':
			case '+':
			case ',':
			case '[':
			case '\\':
			case ']':
				return true;
			default:
				return false;
			}
		}
	}

	internal static int IndexOfNamespaceDelimiter(ReadOnlySpan<char> fullName)
	{
		int num = fullName.LastIndexOf('.');
		if (num > 0 && fullName[num - 1] == '.')
		{
			num--;
		}
		return num;
	}

	internal static string Unescape(string input)
	{
		int num = input.IndexOf('\\');
		if (num < 0)
		{
			return input;
		}
		return UnescapeToBuilder(input, num);
		static string UnescapeToBuilder(string name, int indexOfEscapeCharacter)
		{
			Span<char> initialBuffer = stackalloc char[64];
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			valueStringBuilder.EnsureCapacity(name.Length);
			valueStringBuilder.Append(name.AsSpan(0, indexOfEscapeCharacter));
			int num2 = indexOfEscapeCharacter;
			while (num2 < name.Length)
			{
				char c = name[num2++];
				if (c != '\\' || num2 == name.Length)
				{
					valueStringBuilder.Append(c);
				}
				else if (name[num2] == '\\')
				{
					valueStringBuilder.Append(c);
					num2++;
				}
			}
			return valueStringBuilder.ToString();
		}
	}

	internal static ReadOnlySpan<char> GetAssemblyNameCandidate(ReadOnlySpan<char> input)
	{
		int num = input.IndexOf(']');
		if (num > 0 && input[num - 1] == '\\')
		{
			num = GetUnescapedOffset(input, num);
		}
		if (num >= 0)
		{
			return input.Slice(0, num);
		}
		return input;
		static int GetUnescapedOffset(ReadOnlySpan<char> readOnlySpan, int startIndex)
		{
			int i;
			for (i = startIndex; i < readOnlySpan.Length && (readOnlySpan[i] != ']' || readOnlySpan[i - 1] == '\\'); i++)
			{
			}
			return i;
		}
	}

	internal static void AppendRankOrModifierStringRepresentation(int rankOrModifier, ref ValueStringBuilder builder)
	{
		switch (rankOrModifier)
		{
		case -3:
			builder.Append('&');
			return;
		case -2:
			builder.Append('*');
			return;
		case -1:
			builder.Append("[]");
			return;
		case 1:
			builder.Append("[*]");
			return;
		}
		builder.Append('[');
		builder.Append(',', rankOrModifier - 1);
		builder.Append(']');
	}

	internal static bool IsBeginningOfGenericArgs(ref ReadOnlySpan<char> span, out bool doubleBrackets)
	{
		doubleBrackets = false;
		if (!span.IsEmpty && span[0] == '[')
		{
			ReadOnlySpan<char> readOnlySpan = span.Slice(1).TrimStart();
			if (!readOnlySpan.IsEmpty)
			{
				if (readOnlySpan[0] == '[')
				{
					doubleBrackets = true;
					span = readOnlySpan.Slice(1).TrimStart();
					return true;
				}
				char c = readOnlySpan[0];
				if ((c != '*' && c != ',' && c != ']') || 1 == 0)
				{
					span = readOnlySpan;
					return true;
				}
			}
		}
		return false;
	}

	internal static bool TryGetTypeNameInfo(TypeNameParseOptions options, ref ReadOnlySpan<char> input, ref List<int>? nestedNameLengths, ref int recursiveDepth, out int totalLength)
	{
		totalLength = 0;
		bool isNestedType;
		do
		{
			int fullTypeNameLength = GetFullTypeNameLength(input.Slice(totalLength), out isNestedType);
			if (fullTypeNameLength <= 0)
			{
				return false;
			}
			if (isNestedType)
			{
				if (!TryDive(options, ref recursiveDepth))
				{
					return false;
				}
				(nestedNameLengths ?? (nestedNameLengths = new List<int>())).Add(fullTypeNameLength);
				totalLength++;
			}
			totalLength += fullTypeNameLength;
		}
		while (isNestedType);
		return true;
	}

	internal static bool TryParseNextDecorator(ref ReadOnlySpan<char> input, out int rankOrModifier)
	{
		ReadOnlySpan<char> readOnlySpan = input;
		if (TryStripFirstCharAndTrailingSpaces(ref input, '*'))
		{
			rankOrModifier = -2;
			return true;
		}
		if (TryStripFirstCharAndTrailingSpaces(ref input, '&'))
		{
			rankOrModifier = -3;
			return true;
		}
		if (TryStripFirstCharAndTrailingSpaces(ref input, '['))
		{
			int num = 1;
			bool flag = false;
			while (true)
			{
				if (TryStripFirstCharAndTrailingSpaces(ref input, ']'))
				{
					rankOrModifier = ((num == 1 && !flag) ? (-1) : num);
					return true;
				}
				if (flag)
				{
					break;
				}
				if (num == 1 && TryStripFirstCharAndTrailingSpaces(ref input, '*'))
				{
					flag = true;
					continue;
				}
				if (!TryStripFirstCharAndTrailingSpaces(ref input, ','))
				{
					break;
				}
				num = checked(num + 1);
			}
		}
		input = readOnlySpan;
		rankOrModifier = 0;
		return false;
	}

	internal static bool TryStripFirstCharAndTrailingSpaces(ref ReadOnlySpan<char> span, char value)
	{
		if (!span.IsEmpty && span[0] == value)
		{
			span = span.Slice(1).TrimStart();
			return true;
		}
		return false;
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowArgumentNullException(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowArgumentException_InvalidTypeName(int errorIndex)
	{
		throw new ArgumentException(System.SR.Argument_InvalidTypeName, $"typeName@{errorIndex}");
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowInvalidOperation_MaxNodesExceeded(int limit)
	{
		throw new InvalidOperationException(System.SR.Format(System.SR.InvalidOperation_MaxNodesExceeded, limit));
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowInvalidOperation_NotGenericType()
	{
		throw new InvalidOperationException(System.SR.InvalidOperation_NotGenericType);
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowInvalidOperation_NotNestedType()
	{
		throw new InvalidOperationException(System.SR.InvalidOperation_NotNestedType);
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowInvalidOperation_NoElement()
	{
		throw new InvalidOperationException(System.SR.InvalidOperation_NoElement);
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowInvalidOperation_HasToBeArrayClass()
	{
		throw new InvalidOperationException(System.SR.Argument_HasToBeArrayClass);
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowInvalidOperation_NestedTypeNamespace()
	{
		throw new InvalidOperationException(System.SR.InvalidOperation_NestedTypeNamespace);
	}

	internal static bool IsMaxDepthExceeded(TypeNameParseOptions options, int depth)
	{
		return depth > options.MaxNodes;
	}

	internal static bool TryDive(TypeNameParseOptions options, ref int depth)
	{
		depth++;
		return !IsMaxDepthExceeded(options, depth);
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	internal static void ThrowInvalidOperation_NotSimpleName(string fullName)
	{
		throw new InvalidOperationException(System.SR.Format(System.SR.Arg_NotSimpleTypeName, fullName));
	}
}
