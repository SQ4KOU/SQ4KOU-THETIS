using System;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Symbols;

internal static class CommonGeneratedNames
{
	public const char GenerationSeparator = '#';

	public const string FixedBufferFieldSuffix = ">e__FixedBuffer";

	public static bool TryParseDebugIds(ReadOnlySpan<char> metadataNameSuffix, char idSeparator, bool isMethodIdOptional, out DebugId methodId, out DebugId entityId)
	{
		methodId = (entityId = default(DebugId));
		int num = -1;
		long num2 = 1L;
		long num3 = 0L;
		DebugId? debugId = null;
		for (int num4 = metadataNameSuffix.Length - 1; num4 >= -1; num4--)
		{
			char c = ((num4 >= 0) ? metadataNameSuffix[num4] : '\0');
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
				num3 += (c - 48) * num2;
				if (num3 > int.MaxValue)
				{
					return false;
				}
				num2 *= 10;
				break;
			case '#':
				if (num >= 0 || num2 == 1)
				{
					return false;
				}
				num = (int)num3;
				num3 = 0L;
				num2 = 1L;
				break;
			default:
			{
				if (num2 == 1)
				{
					return false;
				}
				DebugId debugId2 = new DebugId((int)num3, (num >= 0) ? num : 0);
				num = -1;
				num3 = 0L;
				num2 = 1L;
				if (!debugId.HasValue)
				{
					if (c == idSeparator)
					{
						debugId = debugId2;
						break;
					}
					if (isMethodIdOptional)
					{
						entityId = debugId2;
					}
					else
					{
						methodId = debugId2;
					}
					return true;
				}
				if (c == idSeparator)
				{
					return false;
				}
				methodId = debugId2;
				entityId = debugId.Value;
				return true;
			}
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Symbols/CommonGeneratedNameParser.cs", 106);
	}
}
