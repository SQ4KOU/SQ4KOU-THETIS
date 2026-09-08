using System;
using System.Runtime.CompilerServices;

namespace Markdig.Helpers;

internal static class HexConverter
{
	public enum Casing : uint
	{
		Upper = 0u,
		Lower = 8224u
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ToCharsBuffer(byte value, Span<char> buffer, int startingIndex = 0, Casing casing = Casing.Upper)
	{
		uint num = (uint)(((value & 0xF0) << 4) + (value & 0xF) - 35209);
		uint num2 = ((((0 - num) & 0x7070) >> 4) + num + 47545) | (uint)casing;
		buffer[startingIndex + 1] = (char)(num2 & 0xFF);
		buffer[startingIndex] = (char)(num2 >> 8);
	}
}
