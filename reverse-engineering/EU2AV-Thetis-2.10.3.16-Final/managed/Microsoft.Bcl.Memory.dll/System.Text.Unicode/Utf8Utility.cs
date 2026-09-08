using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text.Unicode;

internal static class Utf8Utility
{
	internal const int MaxBytesPerScalar = 4;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int GetIndexOfFirstInvalidUtf8Sequence(ReadOnlySpan<byte> utf8Data, out bool isAscii)
	{
		fixed (byte* reference = &MemoryMarshal.GetReference(utf8Data))
		{
			byte* pointerToFirstInvalidByte = GetPointerToFirstInvalidByte(reference, utf8Data.Length, out var utf16CodeUnitCountAdjustment, out var _);
			int num = (int)(void*)Unsafe.ByteOffset(ref *reference, ref *pointerToFirstInvalidByte);
			isAscii = utf16CodeUnitCountAdjustment == 0;
			if (num >= utf8Data.Length)
			{
				return -1;
			}
			return num;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool AllBytesInUInt32AreAscii(uint value)
	{
		return (value & 0x80808080u) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool AllBytesInUInt64AreAscii(ulong value)
	{
		return (value & 0x8080808080808080uL) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ConvertAllAsciiBytesInUInt32ToLowercase(uint value)
	{
		int num = (int)value + -2139062144 - 1094795585;
		uint num2 = (uint)((int)value + -2139062144 - 1532713819);
		uint num3 = (((uint)num ^ num2) & 0x80808080u) >> 2;
		return value ^ num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ConvertAllAsciiBytesInUInt32ToUppercase(uint value)
	{
		int num = (int)value + -2139062144 - 1633771873;
		uint num2 = (uint)((int)value + -2139062144 - 2071690107);
		uint num3 = (((uint)num ^ num2) & 0x80808080u) >> 2;
		return value ^ num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong ConvertAllAsciiBytesInUInt64ToUppercase(ulong value)
	{
		long num = (long)value + -9187201950435737472L - 7016996765293437281L;
		ulong num2 = (ulong)((long)value + -9187201950435737472L - 8897841259083430779L);
		ulong num3 = (((ulong)num ^ num2) & 0x8080808080808080uL) >> 2;
		return value ^ num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong ConvertAllAsciiBytesInUInt64ToLowercase(ulong value)
	{
		long num = (long)value + -9187201950435737472L - 4702111234474983745L;
		ulong num2 = (ulong)((long)value + -9187201950435737472L - 6582955728264977243L);
		ulong num3 = (((ulong)num ^ num2) & 0x8080808080808080uL) >> 2;
		return value ^ num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool UInt32OrdinalIgnoreCaseAscii(uint valueA, uint valueB)
	{
		uint num = (((valueA + 1061109567) ^ (valueA + 623191333)) & 0x80808080u) >> 2;
		uint num2 = (((valueB + 1061109567) ^ (valueB + 623191333)) & 0x80808080u) >> 2;
		return (valueA | num) == (valueB | num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool UInt64OrdinalIgnoreCaseAscii(ulong valueA, ulong valueB)
	{
		ulong num = (((valueA + 4557430888798830399L) ^ (valueA + 2676586395008836901L)) & 0x8080808080808080uL) >> 2;
		ulong num2 = (((valueB + 4557430888798830399L) ^ (valueB + 2676586395008836901L)) & 0x8080808080808080uL) >> 2;
		return (valueA | num) == (valueB | num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ExtractCharFromFirstThreeByteSequence(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			return ((value & 0x3F0000) >> 16) | ((value & 0x3F00) >> 2) | ((value & 0xF) << 12);
		}
		return ((value & 0xF000000) >> 12) | ((value & 0x3F0000) >> 10) | ((value & 0x3F00) >> 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ExtractCharFromFirstTwoByteSequence(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			uint num = (uint)((byte)value << 6);
			return (byte)(value >> 8) + num - 12288 - 128;
		}
		return (ushort)(((value & 0x1F000000) >> 18) | ((value & 0x3F0000) >> 16));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ExtractCharsFromFourByteSequence(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			return ((uint)((byte)value << 8) | ((value & 0x3F00) >> 6) | ((value & 0x300000) >> 20) | ((value & 0x3F000000) >> 8) | ((value & 0xF0000) << 6)) - 64 - 8192 + 2048 + 3690987520u;
		}
		return ((value & 0xFF000000u) | ((value & 0x3F0000) << 2) | ((value & 0x3000) << 4) | ((value & 0xF00) >> 2) | (value & 0x3F)) - 536870912 - 4194304 + 56320 + 134217728;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ExtractFourUtf8BytesFromSurrogatePair(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			value += 64;
			uint value2 = BinaryPrimitives.ReverseEndianness(value & 0x3F0700);
			value2 = BitOperations.RotateLeft(value2, 16);
			uint num = (value & 0xFC) << 6;
			uint num2 = (value >> 6) & 0xF0000;
			num2 |= num;
			return ((value & 3) << 20) | 0x808080F0u | value2 | num2;
		}
		value -= 3623934976u;
		value += 4194304;
		uint num3 = value & 0x7000000;
		uint num4 = (value >> 2) & 0x3F0000;
		num4 |= num3;
		uint num5 = (value << 2) & 0xF00;
		uint num6 = (value >> 4) & 0x3000;
		num6 |= num5;
		return (uint)((int)(value & 0x3F) + -260013952) | num4 | num6;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ExtractTwoCharsPackedFromTwoAdjacentTwoByteSequences(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			return ((value & 0x3F003F00) >> 8) | ((value & 0x1F001F) << 6);
		}
		return ((value & 0x1F001F00) >> 2) | (value & 0x3F003F);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ExtractTwoUtf8TwoByteSequencesFromTwoPackedUtf16Chars(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			return ((value >> 6) & 0x1F001F) + ((value << 8) & 0x3F003F00) + 2160099520u;
		}
		return ((value << 2) & 0x1F001F00) + (value & 0x3F003F) + 3229663360u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ExtractUtf8TwoByteSequenceFromFirstUtf16Char(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			uint num = (value << 2) & 0x1F00;
			value &= 0x3F;
			return BinaryPrimitives.ReverseEndianness((ushort)(num + value + 49280));
		}
		uint num2 = (value >> 16) & 0x3F;
		value = (value >> 14) & 0x1F00;
		return value + num2 + 49280;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsFirstCharAscii(uint value)
	{
		if (!BitConverter.IsLittleEndian || (value & 0xFF80) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value < 8388608;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsFirstCharAtLeastThreeUtf8Bytes(uint value)
	{
		if (!BitConverter.IsLittleEndian || (value & 0xF800) == 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value >= 134217728;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsFirstCharSurrogate(uint value)
	{
		if (!BitConverter.IsLittleEndian || ((value - 55296) & 0xF800) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (uint)((int)value - -671088640) < 134217728u;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsFirstCharTwoUtf8Bytes(uint value)
	{
		if (!BitConverter.IsLittleEndian || ((value - 128) & 0xFFFF) >= 1920)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return System.Text.UnicodeUtility.IsInRangeInclusive(value, 8388608u, 134217727u);
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsLowByteUtf8ContinuationByte(uint value)
	{
		return (uint)(byte)(value - 128) <= 63u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsSecondCharAscii(uint value)
	{
		if (!BitConverter.IsLittleEndian || value >= 8388608)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (value & 0xFF80) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsSecondCharAtLeastThreeUtf8Bytes(uint value)
	{
		if (!BitConverter.IsLittleEndian || (value & 0xF8000000u) == 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (value & 0xF800) != 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsSecondCharSurrogate(uint value)
	{
		if (!BitConverter.IsLittleEndian || (uint)((int)value - -671088640) >= 134217728u)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return ((value - 55296) & 0xF800) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsSecondCharTwoUtf8Bytes(uint value)
	{
		if (!BitConverter.IsLittleEndian || !System.Text.UnicodeUtility.IsInRangeInclusive(value, 8388608u, 134217727u))
		{
			if (!BitConverter.IsLittleEndian)
			{
				return ((value - 128) & 0xFFFF) < 1920;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsUtf8ContinuationByte(in byte value)
	{
		return (sbyte)value < -64;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsWellFormedUtf16SurrogatePair(uint value)
	{
		if (!BitConverter.IsLittleEndian || (((int)value - -603924480) & -67044352) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (((int)value - -671032320) & -67044352) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ToLittleEndian(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			return value;
		}
		return BinaryPrimitives.ReverseEndianness(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32BeginsWithOverlongUtf8TwoByteSequence(uint value)
	{
		if (!BitConverter.IsLittleEndian || (uint)(byte)value >= 194u)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value < 3254779904u;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32BeginsWithUtf8FourByteMask(uint value)
	{
		if (!BitConverter.IsLittleEndian || (((int)value - -2139062032) & -1061109512) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (((int)value - -260013952) & -121585472) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32BeginsWithUtf8ThreeByteMask(uint value)
	{
		if (!BitConverter.IsLittleEndian || ((value - 8421600) & 0xC0C0F0) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (((int)value - -528449536) & -255803392) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32BeginsWithUtf8TwoByteMask(uint value)
	{
		if (!BitConverter.IsLittleEndian || ((value - 32960) & 0xC0E0) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (((int)value - -1065353216) & -524288000) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32EndsWithOverlongUtf8TwoByteSequence(uint value)
	{
		if (!BitConverter.IsLittleEndian || (value & 0x1E0000) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (value & 0x1E00) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32EndsWithUtf8TwoByteMask(uint value)
	{
		if (!BitConverter.IsLittleEndian || (((int)value - -2134900736) & -1059061760) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return ((value - 49280) & 0xE0C0) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32BeginsWithValidUtf8TwoByteSequenceLittleEndian(uint value)
	{
		if (!BitConverter.IsLittleEndian || !System.Text.UnicodeUtility.IsInRangeInclusive(value & 0xC0FF, 32962u, 32991u))
		{
			if (!BitConverter.IsLittleEndian)
			{
				return false;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32EndsWithValidUtf8TwoByteSequenceLittleEndian(uint value)
	{
		if (!BitConverter.IsLittleEndian || !System.Text.UnicodeUtility.IsInRangeInclusive(value & 0xC0FF0000u, 2160197632u, 2162098176u))
		{
			if (!BitConverter.IsLittleEndian)
			{
				return false;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32FirstByteIsAscii(uint value)
	{
		if (!BitConverter.IsLittleEndian || (value & 0x80) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (int)value >= 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32FourthByteIsAscii(uint value)
	{
		if (!BitConverter.IsLittleEndian || (int)value < 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (value & 0x80) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32SecondByteIsAscii(uint value)
	{
		if (!BitConverter.IsLittleEndian || (value & 0x8000) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (value & 0x800000) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool UInt32ThirdByteIsAscii(uint value)
	{
		if (!BitConverter.IsLittleEndian || (value & 0x800000) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (value & 0x8000) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteTwoUtf16CharsAsTwoUtf8ThreeByteSequences(ref byte outputBuffer, uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			uint num = ((value << 2) & 0x3F00) | ((value & 0x3F) << 16);
			uint num2 = ((value >> 4) & 0xF000000) | ((value >> 12) & 0xF);
			Unsafe.WriteUnaligned(ref outputBuffer, num + num2 + 3766517984u);
			Unsafe.WriteUnaligned(ref Unsafe.Add(ref outputBuffer, 4), (ushort)(((value >> 22) & 0x3F) + ((value >> 8) & 0x3F00) + 32896));
		}
		else
		{
			Unsafe.Add(ref outputBuffer, 5) = (byte)((value & 0x3F) | 0x80);
			Unsafe.Add(ref outputBuffer, 4) = (byte)(((value >>= 6) & 0x3F) | 0x80);
			Unsafe.Add(ref outputBuffer, 3) = (byte)(((value >>= 6) & 0xF) | 0xE0);
			Unsafe.Add(ref outputBuffer, 2) = (byte)(((value >>= 4) & 0x3F) | 0x80);
			Unsafe.Add(ref outputBuffer, 1) = (byte)(((value >>= 6) & 0x3F) | 0x80);
			outputBuffer = (byte)((value >>= 6) | 0xE0);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteFirstUtf16CharAsUtf8ThreeByteSequence(ref byte outputBuffer, uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			uint num = (value << 2) & 0x3F00;
			uint num2 = (uint)(ushort)value >> 12;
			Unsafe.WriteUnaligned(ref outputBuffer, (ushort)(num + num2 + 32992));
			Unsafe.Add(ref outputBuffer, 2) = (byte)((value & 0x3F) | 0xFFFFFF80u);
		}
		else
		{
			Unsafe.Add(ref outputBuffer, 2) = (byte)(((value >>= 16) & 0x3F) | 0x80);
			Unsafe.Add(ref outputBuffer, 1) = (byte)(((value >>= 6) & 0x3F) | 0x80);
			outputBuffer = (byte)((value >>= 6) | 0xE0);
		}
	}

	public unsafe static OperationStatus TranscodeToUtf16(byte* pInputBuffer, int inputLength, char* pOutputBuffer, int outputCharsRemaining, out byte* pInputBufferRemaining, out char* pOutputBufferRemaining)
	{
		nuint num = Ascii.WidenAsciiToUtf16(pInputBuffer, pOutputBuffer, (uint)Math.Min(inputLength, outputCharsRemaining));
		pInputBuffer += num;
		pOutputBuffer += num;
		if ((int)num == inputLength)
		{
			pInputBufferRemaining = pInputBuffer;
			pOutputBufferRemaining = pOutputBuffer;
			return OperationStatus.Done;
		}
		inputLength -= (int)num;
		outputCharsRemaining -= (int)num;
		if (inputLength < 4)
		{
			goto IL_06fa;
		}
		byte* ptr = pInputBuffer + (uint)inputLength - 4;
		while (true)
		{
			IL_004a:
			uint num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
			while (true)
			{
				IL_0051:
				if (!Ascii.AllBytesInUInt32AreAscii(num2))
				{
					goto IL_011c;
				}
				int num4;
				uint num5;
				if (outputCharsRemaining >= 4)
				{
					Ascii.WidenFourAsciiBytesToUtf16AndWriteToBuffer(ref *pOutputBuffer, num2);
					pInputBuffer += 4;
					pOutputBuffer += 4;
					outputCharsRemaining -= 4;
					uint num3 = Math.Min((uint)((int)(void*)Unsafe.ByteOffset(ref *pInputBuffer, ref *ptr) + 4), (uint)outputCharsRemaining) / 8;
					num4 = 0;
					while ((uint)num4 < num3)
					{
						num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
						num5 = Unsafe.ReadUnaligned<uint>(pInputBuffer + 4);
						if (Ascii.AllBytesInUInt32AreAscii(num2 | num5))
						{
							pInputBuffer += 8;
							Ascii.WidenFourAsciiBytesToUtf16AndWriteToBuffer(ref *pOutputBuffer, num2);
							Ascii.WidenFourAsciiBytesToUtf16AndWriteToBuffer(ref pOutputBuffer[4], num5);
							pOutputBuffer += 8;
							num4++;
							continue;
						}
						goto IL_00f0;
					}
					outputCharsRemaining -= 8 * num4;
					goto IL_0518;
				}
				goto IL_051f;
				IL_0518:
				if (pInputBuffer <= ptr)
				{
					goto IL_004a;
				}
				goto IL_051f;
				IL_011c:
				if (UInt32FirstByteIsAscii(num2))
				{
					if (outputCharsRemaining >= 3)
					{
						uint num6 = ToLittleEndian(num2);
						nuint num7 = 1u;
						*pOutputBuffer = (char)(byte)num6;
						if (UInt32SecondByteIsAscii(num2))
						{
							num7++;
							num6 >>= 8;
							pOutputBuffer[1] = (char)(byte)num6;
							if (UInt32ThirdByteIsAscii(num2))
							{
								num7++;
								num6 >>= 8;
								pOutputBuffer[2] = (char)(byte)num6;
							}
						}
						pInputBuffer += num7;
						pOutputBuffer += num7;
						outputCharsRemaining -= (int)num7;
					}
					else
					{
						if (outputCharsRemaining == 0)
						{
							break;
						}
						uint num8 = ToLittleEndian(num2);
						pInputBuffer++;
						*(pOutputBuffer++) = (char)(byte)num8;
						outputCharsRemaining--;
						if (UInt32SecondByteIsAscii(num2))
						{
							if (outputCharsRemaining == 0)
							{
								break;
							}
							pInputBuffer++;
							num8 >>= 8;
							*(pOutputBuffer++) = (char)(byte)num8;
							if (UInt32ThirdByteIsAscii(num2))
							{
								break;
							}
							outputCharsRemaining = 0;
						}
					}
					if (pInputBuffer > ptr)
					{
						goto IL_051f;
					}
					num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
				}
				uint num9;
				while (UInt32BeginsWithUtf8TwoByteMask(num2))
				{
					if (!UInt32BeginsWithOverlongUtf8TwoByteSequence(num2))
					{
						while ((BitConverter.IsLittleEndian && UInt32EndsWithValidUtf8TwoByteSequenceLittleEndian(num2)) || (!BitConverter.IsLittleEndian && UInt32EndsWithUtf8TwoByteMask(num2) && !UInt32EndsWithOverlongUtf8TwoByteSequence(num2)))
						{
							if (outputCharsRemaining >= 2)
							{
								Unsafe.WriteUnaligned(pOutputBuffer, ExtractTwoCharsPackedFromTwoAdjacentTwoByteSequences(num2));
								pInputBuffer += 4;
								pOutputBuffer += 2;
								outputCharsRemaining -= 2;
								if (pInputBuffer <= ptr)
								{
									num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
									if (BitConverter.IsLittleEndian)
									{
										if (!UInt32BeginsWithValidUtf8TwoByteSequenceLittleEndian(num2))
										{
											goto IL_0051;
										}
										continue;
									}
									if (!UInt32BeginsWithUtf8TwoByteMask(num2))
									{
										goto IL_0051;
									}
									if (!UInt32BeginsWithOverlongUtf8TwoByteSequence(num2))
									{
										continue;
									}
									goto IL_070d;
								}
							}
							goto IL_051f;
						}
						num9 = ExtractCharFromFirstTwoByteSequence(num2);
						if (UInt32ThirdByteIsAscii(num2))
						{
							if (UInt32FourthByteIsAscii(num2))
							{
								goto IL_02a8;
							}
							if (outputCharsRemaining >= 2)
							{
								*pOutputBuffer = (char)num9;
								pOutputBuffer[1] = (char)(byte)(num2 >> (BitConverter.IsLittleEndian ? 16 : 8));
								pInputBuffer += 3;
								pOutputBuffer += 2;
								outputCharsRemaining -= 2;
								if (ptr >= pInputBuffer)
								{
									num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
									continue;
								}
							}
						}
						else if (outputCharsRemaining != 0)
						{
							*pOutputBuffer = (char)num9;
							pInputBuffer += 2;
							pOutputBuffer++;
							outputCharsRemaining--;
							if (ptr >= pInputBuffer)
							{
								num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
								break;
							}
						}
						goto IL_051f;
					}
					goto IL_070d;
				}
				if (UInt32BeginsWithUtf8ThreeByteMask(num2))
				{
					while (true)
					{
						if (BitConverter.IsLittleEndian)
						{
							if ((num2 & 0x200F) == 0 || ((num2 - 8205) & 0x200F) == 0)
							{
								break;
							}
						}
						else if ((num2 & 0xF200000) == 0 || ((num2 - 220200960) & 0xF200000) == 0)
						{
							break;
						}
						if (outputCharsRemaining == 0)
						{
							goto end_IL_0051;
						}
						if (BitConverter.IsLittleEndian && (((int)num2 - -536870912) & -268435456) == 0 && outputCharsRemaining > 1 && (nint)(void*)Unsafe.ByteOffset(ref *pInputBuffer, ref *ptr) >= (nint)3)
						{
							uint num10 = Unsafe.ReadUnaligned<uint>(pInputBuffer + 3);
							if (UInt32BeginsWithUtf8ThreeByteMask(num10) && (num10 & 0x200F) != 0 && ((num10 - 8205) & 0x200F) != 0)
							{
								*pOutputBuffer = (char)ExtractCharFromFirstThreeByteSequence(num2);
								pOutputBuffer[1] = (char)ExtractCharFromFirstThreeByteSequence(num10);
								pInputBuffer += 6;
								pOutputBuffer += 2;
								outputCharsRemaining -= 2;
								goto IL_045a;
							}
						}
						*pOutputBuffer = (char)ExtractCharFromFirstThreeByteSequence(num2);
						pInputBuffer += 3;
						pOutputBuffer++;
						outputCharsRemaining--;
						goto IL_045a;
						IL_045a:
						if (UInt32FourthByteIsAscii(num2))
						{
							if (outputCharsRemaining == 0)
							{
								goto end_IL_0051;
							}
							if (BitConverter.IsLittleEndian)
							{
								*pOutputBuffer = (char)(num2 >> 24);
							}
							else
							{
								*pOutputBuffer = (char)(byte)num2;
							}
							pInputBuffer++;
							pOutputBuffer++;
							outputCharsRemaining--;
						}
						if (pInputBuffer <= ptr)
						{
							num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
							if (!UInt32BeginsWithUtf8ThreeByteMask(num2))
							{
								goto IL_0051;
							}
							continue;
						}
						goto IL_051f;
					}
				}
				else if (UInt32BeginsWithUtf8FourByteMask(num2))
				{
					if (BitConverter.IsLittleEndian)
					{
						if (System.Text.UnicodeUtility.IsInRangeInclusive(BitOperations.RotateRight(num2 & 0xFFFF, 8), 4026531984u, 4093640847u))
						{
							goto IL_04f3;
						}
					}
					else if (System.Text.UnicodeUtility.IsInRangeInclusive(num2, 4035969024u, 4103077887u))
					{
						goto IL_04f3;
					}
				}
				goto IL_070d;
				IL_04f3:
				if (outputCharsRemaining < 2)
				{
					break;
				}
				Unsafe.WriteUnaligned(pOutputBuffer, ExtractCharsFromFourByteSequence(num2));
				pInputBuffer += 4;
				pOutputBuffer += 2;
				outputCharsRemaining -= 2;
				goto IL_0518;
				IL_02a8:
				if (outputCharsRemaining >= 3)
				{
					*pOutputBuffer = (char)num9;
					if (BitConverter.IsLittleEndian)
					{
						num2 >>= 16;
						pOutputBuffer[1] = (char)(byte)num2;
						num2 >>= 8;
						pOutputBuffer[2] = (char)num2;
					}
					else
					{
						pOutputBuffer[2] = (char)(byte)num2;
						pOutputBuffer[1] = (char)(byte)(num2 >> 8);
					}
					pInputBuffer += 4;
					pOutputBuffer += 3;
					outputCharsRemaining -= 3;
					goto IL_0518;
				}
				goto IL_051f;
				IL_00f0:
				if (Ascii.AllBytesInUInt32AreAscii(num2))
				{
					Ascii.WidenFourAsciiBytesToUtf16AndWriteToBuffer(ref *pOutputBuffer, num2);
					num2 = num5;
					pInputBuffer += 4;
					pOutputBuffer += 4;
					outputCharsRemaining -= 4;
				}
				outputCharsRemaining -= 8 * num4;
				goto IL_011c;
				IL_051f:
				inputLength = (int)(void*)Unsafe.ByteOffset(ref *pInputBuffer, ref *ptr) + 4;
				goto IL_06fa;
				continue;
				end_IL_0051:
				break;
			}
			break;
		}
		goto IL_0709;
		IL_0709:
		OperationStatus result = OperationStatus.DestinationTooSmall;
		goto IL_070f;
		IL_06fa:
		while (true)
		{
			if (inputLength > 0)
			{
				uint num11 = *pInputBuffer;
				if (num11 <= 127)
				{
					if (outputCharsRemaining != 0)
					{
						*pOutputBuffer = (char)num11;
						pInputBuffer++;
						pOutputBuffer++;
						inputLength--;
						outputCharsRemaining--;
						continue;
					}
					goto IL_0709;
				}
				num11 -= 194;
				if ((uint)(byte)num11 <= 29u)
				{
					if (inputLength < 2)
					{
						goto IL_0705;
					}
					uint num12 = pInputBuffer[1];
					if (IsLowByteUtf8ContinuationByte(num12))
					{
						if (outputCharsRemaining != 0)
						{
							uint num13 = (num11 << 6) + num12 + 128 - 128;
							*pOutputBuffer = (char)num13;
							pInputBuffer += 2;
							pOutputBuffer++;
							inputLength -= 2;
							outputCharsRemaining--;
							continue;
						}
						goto IL_0709;
					}
				}
				else if ((uint)(byte)num11 <= 45u)
				{
					if (inputLength >= 3)
					{
						uint num14 = pInputBuffer[1];
						uint num15 = pInputBuffer[2];
						if (IsLowByteUtf8ContinuationByte(num14) && IsLowByteUtf8ContinuationByte(num15))
						{
							uint num16 = (num11 << 12) + (num14 << 6);
							if (num16 >= 133120)
							{
								num16 -= 186368;
								if (num16 >= 2048)
								{
									if (outputCharsRemaining != 0)
									{
										num16 += num15;
										num16 += 55296;
										num16 -= 128;
										*pOutputBuffer = (char)num16;
										pInputBuffer += 3;
										pOutputBuffer++;
										inputLength -= 3;
										outputCharsRemaining--;
										continue;
									}
									goto IL_0709;
								}
							}
						}
					}
					else
					{
						if (inputLength < 2)
						{
							goto IL_0705;
						}
						uint num17 = pInputBuffer[1];
						if (IsLowByteUtf8ContinuationByte(num17))
						{
							uint num18 = (num11 << 6) + num17;
							if (num18 >= 2080 && !System.Text.UnicodeUtility.IsInRangeInclusive(num18, 2912u, 2943u))
							{
								goto IL_0705;
							}
						}
					}
				}
				else if ((uint)(byte)num11 <= 50u)
				{
					if (inputLength < 2)
					{
						goto IL_0705;
					}
					uint num19 = pInputBuffer[1];
					if (IsLowByteUtf8ContinuationByte(num19) && System.Text.UnicodeUtility.IsInRangeInclusive((num11 << 6) + num19, 3088u, 3343u))
					{
						if (inputLength < 3)
						{
							goto IL_0705;
						}
						if (IsLowByteUtf8ContinuationByte(pInputBuffer[2]))
						{
							if (inputLength < 4)
							{
								goto IL_0705;
							}
							if (IsLowByteUtf8ContinuationByte(pInputBuffer[3]))
							{
								goto IL_0709;
							}
						}
					}
				}
				goto IL_070d;
			}
			result = OperationStatus.Done;
			break;
			IL_0705:
			result = OperationStatus.NeedMoreData;
			break;
		}
		goto IL_070f;
		IL_070d:
		result = OperationStatus.InvalidData;
		goto IL_070f;
		IL_070f:
		pInputBufferRemaining = pInputBuffer;
		pOutputBufferRemaining = pOutputBuffer;
		return result;
	}

	public unsafe static OperationStatus TranscodeToUtf8(char* pInputBuffer, int inputLength, byte* pOutputBuffer, int outputBytesRemaining, out char* pInputBufferRemaining, out byte* pOutputBufferRemaining)
	{
		nuint num = Ascii.NarrowUtf16ToAscii(pInputBuffer, pOutputBuffer, (uint)Math.Min(inputLength, outputBytesRemaining));
		pInputBuffer += num;
		pOutputBuffer += num;
		if ((int)num == inputLength)
		{
			pInputBufferRemaining = pInputBuffer;
			pOutputBufferRemaining = pOutputBuffer;
			return OperationStatus.Done;
		}
		inputLength -= (int)num;
		outputBytesRemaining -= (int)num;
		if (inputLength < 2)
		{
			goto IL_0354;
		}
		char* ptr = pInputBuffer + (uint)inputLength - 2;
		uint num2;
		while (true)
		{
			num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
			int num5;
			while (true)
			{
				IL_005e:
				uint num6;
				if (System.Text.Unicode.Utf16Utility.AllCharsInUInt32AreAscii(num2))
				{
					if (outputBytesRemaining < 2)
					{
						break;
					}
					uint num3 = num2 | (num2 >> 8);
					Unsafe.WriteUnaligned(pOutputBuffer, (ushort)num3);
					pInputBuffer += 2;
					pOutputBuffer += 2;
					outputBytesRemaining -= 2;
					uint num4 = (uint)Math.Min((uint)((int)(ptr - pInputBuffer) + 2), outputBytesRemaining) / 4;
					num5 = 0;
					while ((uint)num5 < num4)
					{
						num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
						num6 = Unsafe.ReadUnaligned<uint>(pInputBuffer + 2);
						if (System.Text.Unicode.Utf16Utility.AllCharsInUInt32AreAscii(num2 | num6))
						{
							Unsafe.WriteUnaligned(pOutputBuffer, (ushort)(num2 | (num2 >> 8)));
							Unsafe.WriteUnaligned(pOutputBuffer + 2, (ushort)(num6 | (num6 >> 8)));
							pInputBuffer += 4;
							pOutputBuffer += 4;
							num5++;
							continue;
						}
						goto IL_010e;
					}
					goto IL_0101;
				}
				goto IL_013f;
				IL_013f:
				while (true)
				{
					if (IsFirstCharAscii(num2))
					{
						if (outputBytesRemaining == 0)
						{
							break;
						}
						if (BitConverter.IsLittleEndian)
						{
							*pOutputBuffer = (byte)num2;
						}
						else
						{
							*pOutputBuffer = (byte)(num2 >> 16);
						}
						pInputBuffer++;
						pOutputBuffer++;
						outputBytesRemaining--;
						if (pInputBuffer > ptr)
						{
							goto IL_0349;
						}
						num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
					}
					if (!IsFirstCharAtLeastThreeUtf8Bytes(num2))
					{
						while (IsSecondCharTwoUtf8Bytes(num2))
						{
							if (outputBytesRemaining < 4)
							{
								goto end_IL_005e;
							}
							Unsafe.WriteUnaligned(pOutputBuffer, ExtractTwoUtf8TwoByteSequencesFromTwoPackedUtf16Chars(num2));
							pInputBuffer += 2;
							pOutputBuffer += 4;
							outputBytesRemaining -= 4;
							if (pInputBuffer <= ptr)
							{
								num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
								if (!IsFirstCharTwoUtf8Bytes(num2))
								{
									goto IL_005e;
								}
								continue;
							}
							goto IL_0349;
						}
						if (outputBytesRemaining < 2)
						{
							break;
						}
						Unsafe.WriteUnaligned(pOutputBuffer, (ushort)ExtractUtf8TwoByteSequenceFromFirstUtf16Char(num2));
						if (IsSecondCharAscii(num2))
						{
							goto IL_01ed;
						}
						pInputBuffer++;
						pOutputBuffer += 2;
						outputBytesRemaining -= 2;
						if (pInputBuffer > ptr)
						{
							goto IL_0349;
						}
						num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
					}
					while (!IsFirstCharSurrogate(num2))
					{
						if (IsSecondCharAtLeastThreeUtf8Bytes(num2) && !IsSecondCharSurrogate(num2) && outputBytesRemaining >= 6)
						{
							WriteTwoUtf16CharsAsTwoUtf8ThreeByteSequences(ref *pOutputBuffer, num2);
							pInputBuffer += 2;
							pOutputBuffer += 6;
							outputBytesRemaining -= 6;
							if (pInputBuffer <= ptr)
							{
								num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
								if (!IsFirstCharAtLeastThreeUtf8Bytes(num2))
								{
									goto IL_005e;
								}
								continue;
							}
						}
						else
						{
							if (outputBytesRemaining < 3)
							{
								goto end_IL_013f;
							}
							WriteFirstUtf16CharAsUtf8ThreeByteSequence(ref *pOutputBuffer, num2);
							pInputBuffer++;
							pOutputBuffer += 3;
							outputBytesRemaining -= 3;
							if (!IsSecondCharAscii(num2))
							{
								goto IL_0302;
							}
							if (outputBytesRemaining == 0)
							{
								goto end_IL_013f;
							}
							if (BitConverter.IsLittleEndian)
							{
								*pOutputBuffer = (byte)(num2 >> 16);
							}
							else
							{
								*pOutputBuffer = (byte)num2;
							}
							pInputBuffer++;
							pOutputBuffer++;
							outputBytesRemaining--;
							if (pInputBuffer <= ptr)
							{
								num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
								if (!IsFirstCharAtLeastThreeUtf8Bytes(num2))
								{
									goto IL_005e;
								}
								continue;
							}
						}
						goto IL_0349;
					}
					goto IL_0312;
					IL_0302:
					if (pInputBuffer <= ptr)
					{
						num2 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
						continue;
					}
					goto IL_0349;
					continue;
					end_IL_013f:
					break;
				}
				goto IL_040f;
				IL_010e:
				outputBytesRemaining -= 4 * num5;
				if (System.Text.Unicode.Utf16Utility.AllCharsInUInt32AreAscii(num2))
				{
					Unsafe.WriteUnaligned(pOutputBuffer, (ushort)(num2 | (num2 >> 8)));
					pInputBuffer += 2;
					pOutputBuffer += 2;
					outputBytesRemaining -= 2;
					num2 = num6;
				}
				goto IL_013f;
				continue;
				end_IL_005e:
				break;
			}
			break;
			IL_0342:
			if (pInputBuffer <= ptr)
			{
				continue;
			}
			goto IL_0349;
			IL_0101:
			outputBytesRemaining -= 4 * num5;
			goto IL_0342;
			IL_01ed:
			if (outputBytesRemaining >= 3)
			{
				if (BitConverter.IsLittleEndian)
				{
					num2 >>= 16;
				}
				pOutputBuffer[2] = (byte)num2;
				pInputBuffer += 2;
				pOutputBuffer += 3;
				outputBytesRemaining -= 3;
				goto IL_0342;
			}
			pInputBuffer++;
			pOutputBuffer += 2;
			goto IL_040f;
			IL_0312:
			if (IsWellFormedUtf16SurrogatePair(num2))
			{
				if (outputBytesRemaining >= 4)
				{
					Unsafe.WriteUnaligned(pOutputBuffer, ExtractFourUtf8BytesFromSurrogatePair(num2));
					pInputBuffer += 2;
					pOutputBuffer += 4;
					outputBytesRemaining -= 4;
					goto IL_0342;
				}
				goto IL_040f;
			}
			goto IL_0413;
		}
		uint num7 = ((!BitConverter.IsLittleEndian) ? (num2 >> 16) : (num2 & 0xFFFF));
		goto IL_0375;
		IL_0415:
		pInputBufferRemaining = pInputBuffer;
		pOutputBufferRemaining = pOutputBuffer;
		OperationStatus result;
		return result;
		IL_0407:
		result = OperationStatus.Done;
		goto IL_0415;
		IL_0375:
		if (num7 <= 127)
		{
			if (outputBytesRemaining != 0)
			{
				*pOutputBuffer = (byte)num7;
				pInputBuffer++;
				pOutputBuffer++;
				goto IL_0403;
			}
		}
		else if (num7 < 2048)
		{
			if (outputBytesRemaining >= 2)
			{
				pOutputBuffer[1] = (byte)((num7 & 0x3F) | 0xFFFFFF80u);
				*pOutputBuffer = (byte)((num7 >> 6) | 0xFFFFFFC0u);
				pInputBuffer++;
				pOutputBuffer += 2;
				goto IL_0403;
			}
		}
		else
		{
			if (System.Text.UnicodeUtility.IsSurrogateCodePoint(num7))
			{
				if (num7 > 56319)
				{
					goto IL_0413;
				}
				result = OperationStatus.NeedMoreData;
				goto IL_0415;
			}
			if (outputBytesRemaining >= 3)
			{
				pOutputBuffer[2] = (byte)((num7 & 0x3F) | 0xFFFFFF80u);
				pOutputBuffer[1] = (byte)(((num7 >> 6) & 0x3F) | 0xFFFFFF80u);
				*pOutputBuffer = (byte)((num7 >> 12) | 0xFFFFFFE0u);
				pInputBuffer++;
				pOutputBuffer += 3;
				goto IL_0403;
			}
		}
		goto IL_040f;
		IL_0413:
		result = OperationStatus.InvalidData;
		goto IL_0415;
		IL_0354:
		if (inputLength != 0)
		{
			num7 = *pInputBuffer;
			goto IL_0375;
		}
		goto IL_0407;
		IL_0403:
		if (inputLength <= 1)
		{
			goto IL_0407;
		}
		goto IL_040f;
		IL_040f:
		result = OperationStatus.DestinationTooSmall;
		goto IL_0415;
		IL_0349:
		inputLength = (int)(ptr - pInputBuffer) + 2;
		goto IL_0354;
	}

	public unsafe static byte* GetPointerToFirstInvalidByte(byte* pInputBuffer, int inputLength, out int utf16CodeUnitCountAdjustment, out int scalarCountAdjustment)
	{
		nuint indexOfFirstNonAsciiByte = Ascii.GetIndexOfFirstNonAsciiByte(pInputBuffer, (uint)inputLength);
		pInputBuffer += indexOfFirstNonAsciiByte;
		inputLength -= (int)indexOfFirstNonAsciiByte;
		if (inputLength == 0)
		{
			utf16CodeUnitCountAdjustment = 0;
			scalarCountAdjustment = 0;
			return pInputBuffer;
		}
		int num = 0;
		int num2 = 0;
		nuint num6;
		if (inputLength >= 4)
		{
			byte* ptr = pInputBuffer + (uint)inputLength - 4;
			while (pInputBuffer <= ptr)
			{
				uint num3 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
				while (true)
				{
					IL_0040:
					if (Ascii.AllBytesInUInt32AreAscii(num3))
					{
						pInputBuffer += 4;
						if ((nint)(void*)Unsafe.ByteOffset(ref *pInputBuffer, ref *ptr) < (nint)16)
						{
							break;
						}
						num3 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
						if (Ascii.AllBytesInUInt32AreAscii(num3))
						{
							pInputBuffer = (byte*)((nuint)(pInputBuffer + 4) & (nuint)(~(nint)3));
							byte* ptr2 = ptr - 12;
							while (true)
							{
								if (Ascii.AllBytesInUInt32AreAscii(*(uint*)pInputBuffer | ((uint*)pInputBuffer)[1]))
								{
									if (Ascii.AllBytesInUInt32AreAscii(((uint*)pInputBuffer)[2] | ((uint*)pInputBuffer)[3]))
									{
										pInputBuffer += 16;
										if (pInputBuffer > ptr2)
										{
											break;
										}
										continue;
									}
									pInputBuffer += 8;
								}
								num3 = *(uint*)pInputBuffer;
								if (Ascii.AllBytesInUInt32AreAscii(num3))
								{
									pInputBuffer += 4;
									num3 = *(uint*)pInputBuffer;
								}
								goto IL_00d4;
							}
							break;
						}
					}
					goto IL_00d4;
					IL_0489:
					pInputBuffer += 4;
					num -= 2;
					num2--;
					break;
					IL_00d4:
					uint num4 = Ascii.CountNumberOfLeadingAsciiBytesFromUInt32WithSomeNonAsciiData(num3);
					pInputBuffer += num4;
					if (ptr < pInputBuffer)
					{
						goto end_IL_0496;
					}
					num3 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
					while (true)
					{
						num3 -= (uint)(BitConverter.IsLittleEndian ? 32960 : (-1065353216));
						if ((num3 & (uint)(BitConverter.IsLittleEndian ? 49376 : (-524288000))) != 0)
						{
							break;
						}
						if ((!BitConverter.IsLittleEndian || (uint)(byte)num3 >= 2u) && (BitConverter.IsLittleEndian || num3 >= 33554432))
						{
							while ((BitConverter.IsLittleEndian && UInt32EndsWithValidUtf8TwoByteSequenceLittleEndian(num3)) || (!BitConverter.IsLittleEndian && UInt32EndsWithUtf8TwoByteMask(num3) && !UInt32EndsWithOverlongUtf8TwoByteSequence(num3)))
							{
								pInputBuffer += 4;
								num -= 2;
								if (pInputBuffer > ptr)
								{
									goto end_IL_0496;
								}
								num3 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
								if (BitConverter.IsLittleEndian)
								{
									if (!UInt32BeginsWithValidUtf8TwoByteSequenceLittleEndian(num3))
									{
										goto IL_0040;
									}
									continue;
								}
								if (!UInt32BeginsWithUtf8TwoByteMask(num3))
								{
									goto IL_0040;
								}
								if (!UInt32BeginsWithOverlongUtf8TwoByteSequence(num3))
								{
									continue;
								}
								goto IL_059a;
							}
							num--;
							if (UInt32ThirdByteIsAscii(num3))
							{
								if (UInt32FourthByteIsAscii(num3))
								{
									pInputBuffer += 4;
									goto end_IL_0040;
								}
								pInputBuffer += 3;
								if (pInputBuffer > ptr)
								{
									goto end_IL_0040;
								}
								num3 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
								continue;
							}
							pInputBuffer += 2;
							goto end_IL_0040;
						}
						goto IL_059a;
					}
					num3 -= (uint)(BitConverter.IsLittleEndian ? 8388640 : 536903680);
					if ((num3 & (uint)(BitConverter.IsLittleEndian ? 12632304 : (-255803392))) == 0)
					{
						while (true)
						{
							if (BitConverter.IsLittleEndian)
							{
								if ((num3 & 0x200F) == 0 || ((num3 - 8205) & 0x200F) == 0)
								{
									break;
								}
							}
							else if ((num3 & 0xF200000) == 0 || ((num3 - 220200960) & 0xF200000) == 0)
							{
								break;
							}
							while (true)
							{
								IL_0278:
								IntPtr intPtr = ((!BitConverter.IsLittleEndian) ? ((nint)(sbyte)num3 >> 7) : ((int)num3 >> 31));
								pInputBuffer += 4;
								pInputBuffer += (nint)intPtr;
								num -= 2;
								ulong num5;
								while (IntPtr.Size >= 8 && BitConverter.IsLittleEndian && (nint)(ptr - pInputBuffer) >= 5)
								{
									num5 = Unsafe.ReadUnaligned<ulong>(pInputBuffer);
									num3 = (uint)num5;
									if ((num5 & 0xC0F0C0C0F0C0C0F0uL) == 9286563722648649952uL && IsUtf8ContinuationByte(in pInputBuffer[8]))
									{
										if (((int)num5 & 0x200F) == 0 || (((int)num5 - 8205) & 0x200F) == 0)
										{
											goto end_IL_022f;
										}
										num5 >>= 24;
										if (((int)num5 & 0x200F) != 0 && (((int)num5 - 8205) & 0x200F) != 0)
										{
											num5 >>= 24;
											if (((int)num5 & 0x200F) != 0 && (((int)num5 - 8205) & 0x200F) != 0)
											{
												pInputBuffer += 9;
												num -= 6;
												continue;
											}
										}
										goto IL_0278;
									}
									goto IL_037c;
								}
								break;
								IL_037c:
								if ((num5 & 0xC0C0F0C0C0F0L) == 141291010687200L)
								{
									if (((int)num5 & 0x200F) == 0 || (((int)num5 - 8205) & 0x200F) == 0)
									{
										goto end_IL_022f;
									}
									num5 >>= 24;
									if (((int)num5 & 0x200F) == 0 || (((int)num5 - 8205) & 0x200F) == 0)
									{
										continue;
									}
									goto IL_03de;
								}
								goto IL_03ec;
							}
							if (pInputBuffer > ptr)
							{
								goto end_IL_0496;
							}
							num3 = Unsafe.ReadUnaligned<uint>(pInputBuffer);
							if (!UInt32BeginsWithUtf8ThreeByteMask(num3))
							{
								goto IL_0040;
							}
							continue;
							IL_03ec:
							if (!UInt32BeginsWithUtf8ThreeByteMask(num3))
							{
								goto IL_0040;
							}
							continue;
							end_IL_022f:
							break;
						}
					}
					else if (BitConverter.IsLittleEndian)
					{
						num3 &= 0xC0C0FFFFu;
						if ((int)num3 <= -2147467265)
						{
							num3 = BitOperations.RotateRight(num3, 8);
							if (System.Text.UnicodeUtility.IsInRangeInclusive(num3, 276824080u, 343932943u))
							{
								goto IL_0489;
							}
						}
					}
					else
					{
						num3 -= 128;
						if ((num3 & 0xC0C0C0) == 0 && System.Text.UnicodeUtility.IsInRangeInclusive(num3, 269484032u, 336592895u))
						{
							goto IL_0489;
						}
					}
					goto IL_059a;
					IL_03de:
					pInputBuffer += 6;
					num -= 4;
					break;
					continue;
					end_IL_0040:
					break;
				}
				continue;
				end_IL_0496:
				break;
			}
			num6 = (nuint)((byte*)(void*)Unsafe.ByteOffset(ref *pInputBuffer, ref *ptr) + 4);
		}
		else
		{
			num6 = (uint)inputLength;
		}
		while (num6 != 0)
		{
			uint num7 = *pInputBuffer;
			if ((uint)(byte)num7 < 128u)
			{
				pInputBuffer++;
				num6--;
				continue;
			}
			if (num6 < 2)
			{
				break;
			}
			uint value = pInputBuffer[1];
			if ((uint)(byte)num7 < 224u)
			{
				if ((uint)(byte)num7 < 194u || !IsLowByteUtf8ContinuationByte(value))
				{
					break;
				}
				pInputBuffer += 2;
				num--;
				num6 -= 2;
				continue;
			}
			if (num6 < 3 || (uint)(byte)num7 >= 240u)
			{
				break;
			}
			if ((byte)num7 == 224)
			{
				if (!System.Text.UnicodeUtility.IsInRangeInclusive(value, 160u, 191u))
				{
					break;
				}
			}
			else if ((byte)num7 == 237)
			{
				if (!System.Text.UnicodeUtility.IsInRangeInclusive(value, 128u, 159u))
				{
					break;
				}
			}
			else if (!IsLowByteUtf8ContinuationByte(value))
			{
				break;
			}
			if (!IsUtf8ContinuationByte(in pInputBuffer[2]))
			{
				break;
			}
			pInputBuffer += 3;
			num -= 2;
			num6 -= 3;
		}
		goto IL_059a;
		IL_059a:
		utf16CodeUnitCountAdjustment = num;
		scalarCountAdjustment = num2;
		return pInputBuffer;
	}
}
