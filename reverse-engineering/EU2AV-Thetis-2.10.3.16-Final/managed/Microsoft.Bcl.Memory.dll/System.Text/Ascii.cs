using System.Numerics;
using System.Runtime.CompilerServices;

namespace System.Text;

internal static class Ascii
{
	private const uint UInt32HighBitsOnlyMask = 2155905152u;

	private const ulong UInt64HighBitsOnlyMask = 9259542123273814144uL;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AllBytesInUInt64AreAscii(ulong value)
	{
		return (value & 0x8080808080808080uL) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AllCharsInUInt32AreAscii(uint value)
	{
		return (value & 0xFF80FF80u) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AllCharsInUInt64AreAscii(ulong value)
	{
		return (value & 0xFF80FF80FF80FF80uL) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AllCharsInUInt64AreAscii<T>(ulong value) where T : unmanaged
	{
		if (!(typeof(T) == typeof(byte)))
		{
			return AllCharsInUInt64AreAscii(value);
		}
		return AllBytesInUInt64AreAscii(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool FirstCharInUInt32IsAscii(uint value)
	{
		if (!BitConverter.IsLittleEndian || (value & 0xFF80) != 0)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return (value & 0xFF800000u) == 0;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static nuint GetIndexOfFirstNonAsciiByte(byte* pBuffer, nuint bufferLength)
	{
		return GetIndexOfFirstNonAsciiByte_Vector(pBuffer, bufferLength);
	}

	private unsafe static nuint GetIndexOfFirstNonAsciiByte_Vector(byte* pBuffer, nuint bufferLength)
	{
		byte* ptr = pBuffer;
		while (true)
		{
			uint num;
			if (bufferLength >= 8)
			{
				num = Unsafe.ReadUnaligned<uint>(pBuffer);
				uint num2 = Unsafe.ReadUnaligned<uint>(pBuffer + 4);
				if (!AllBytesInUInt32AreAscii(num | num2))
				{
					if (AllBytesInUInt32AreAscii(num))
					{
						num = num2;
						pBuffer += 4;
					}
					goto IL_0095;
				}
				pBuffer += 8;
				bufferLength -= 8;
				continue;
			}
			if ((bufferLength & 4) != 0)
			{
				num = Unsafe.ReadUnaligned<uint>(pBuffer);
				if (!AllBytesInUInt32AreAscii(num))
				{
					goto IL_0095;
				}
				pBuffer += 4;
			}
			if ((bufferLength & 2) != 0)
			{
				num = Unsafe.ReadUnaligned<ushort>(pBuffer);
				if (!AllBytesInUInt32AreAscii(num))
				{
					if (!BitConverter.IsLittleEndian)
					{
						num <<= 16;
					}
					goto IL_0095;
				}
				pBuffer += 2;
			}
			if ((bufferLength & 1) != 0 && *pBuffer >= 0)
			{
				pBuffer++;
			}
			break;
			IL_0095:
			pBuffer += CountNumberOfLeadingAsciiBytesFromUInt32WithSomeNonAsciiData(num);
			break;
		}
		return (nuint)(pBuffer - (nuint)ptr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static nuint GetIndexOfFirstNonAsciiChar(char* pBuffer, nuint bufferLength)
	{
		return GetIndexOfFirstNonAsciiChar_Vector(pBuffer, bufferLength);
	}

	private unsafe static nuint GetIndexOfFirstNonAsciiChar_Vector(char* pBuffer, nuint bufferLength)
	{
		char* ptr = pBuffer;
		while (true)
		{
			uint num;
			if (bufferLength >= 4)
			{
				num = Unsafe.ReadUnaligned<uint>(pBuffer);
				uint num2 = Unsafe.ReadUnaligned<uint>(pBuffer + 2);
				if (!AllCharsInUInt32AreAscii(num | num2))
				{
					if (AllCharsInUInt32AreAscii(num))
					{
						num = num2;
						pBuffer += 2;
					}
					goto IL_007d;
				}
				pBuffer += 4;
				bufferLength -= 4;
				continue;
			}
			if ((bufferLength & 2) != 0)
			{
				num = Unsafe.ReadUnaligned<uint>(pBuffer);
				if (!AllCharsInUInt32AreAscii(num))
				{
					goto IL_007d;
				}
				pBuffer += 2;
			}
			if ((bufferLength & 1) != 0 && *pBuffer <= '\u007f')
			{
				pBuffer++;
			}
			break;
			IL_007d:
			if (FirstCharInUInt32IsAscii(num))
			{
				pBuffer++;
			}
			break;
		}
		return (nuint)(pBuffer - ptr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void NarrowFourUtf16CharsToAsciiAndWriteToBuffer(ref byte outputBuffer, ulong value)
	{
		if (BitConverter.IsLittleEndian)
		{
			outputBuffer = (byte)value;
			value >>= 16;
			Unsafe.Add(ref outputBuffer, 1) = (byte)value;
			value >>= 16;
			Unsafe.Add(ref outputBuffer, 2) = (byte)value;
			value >>= 16;
			Unsafe.Add(ref outputBuffer, 3) = (byte)value;
		}
		else
		{
			Unsafe.Add(ref outputBuffer, 3) = (byte)value;
			value >>= 16;
			Unsafe.Add(ref outputBuffer, 2) = (byte)value;
			value >>= 16;
			Unsafe.Add(ref outputBuffer, 1) = (byte)value;
			value >>= 16;
			outputBuffer = (byte)value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref byte outputBuffer, uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			outputBuffer = (byte)value;
			Unsafe.Add(ref outputBuffer, 1) = (byte)(value >> 16);
		}
		else
		{
			Unsafe.Add(ref outputBuffer, 1) = (byte)value;
			outputBuffer = (byte)(value >> 16);
		}
	}

	internal unsafe static nuint NarrowUtf16ToAscii(char* pUtf16Buffer, byte* pAsciiBuffer, nuint elementCount)
	{
		nuint num = 0u;
		uint num2 = 0u;
		uint num3 = 0u;
		ulong num4 = 0uL;
		nuint num5 = elementCount - num;
		if (num5 < 4)
		{
			goto IL_009d;
		}
		nuint num6 = num + num5 - 4;
		while (true)
		{
			if (IntPtr.Size >= 8)
			{
				num4 = Unsafe.ReadUnaligned<ulong>(pUtf16Buffer + num);
				if (!AllCharsInUInt64AreAscii(num4))
				{
					break;
				}
				NarrowFourUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[num], num4);
			}
			else
			{
				num2 = Unsafe.ReadUnaligned<uint>(pUtf16Buffer + num);
				num3 = Unsafe.ReadUnaligned<uint>(pUtf16Buffer + num + 2);
				if (!AllCharsInUInt32AreAscii(num2 | num3))
				{
					break;
				}
				NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[num], num2);
				NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[num + 2], num3);
			}
			num += 4;
			if (num <= num6)
			{
				continue;
			}
			goto IL_009d;
		}
		if (IntPtr.Size >= 8)
		{
			num2 = (uint)((!BitConverter.IsLittleEndian) ? (num4 >> 32) : num4);
			if (AllCharsInUInt32AreAscii(num2))
			{
				NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[num], num2);
				num2 = (uint)((!BitConverter.IsLittleEndian) ? num4 : (num4 >> 32));
				num += 2;
			}
		}
		else if (AllCharsInUInt32AreAscii(num2))
		{
			NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[num], num2);
			num2 = num3;
			num += 2;
		}
		goto IL_0151;
		IL_0151:
		if (FirstCharInUInt32IsAscii(num2))
		{
			if (!BitConverter.IsLittleEndian)
			{
				num2 >>= 16;
			}
			pAsciiBuffer[num] = (byte)num2;
			num++;
		}
		goto IL_00ef;
		IL_00ef:
		return num;
		IL_009d:
		if (((int)num5 & 2) != 0)
		{
			num2 = Unsafe.ReadUnaligned<uint>(pUtf16Buffer + num);
			if (!AllCharsInUInt32AreAscii(num2))
			{
				goto IL_0151;
			}
			NarrowTwoUtf16CharsToAsciiAndWriteToBuffer(ref pAsciiBuffer[num], num2);
			num += 2;
		}
		if (((int)num5 & 1) != 0)
		{
			num2 = pUtf16Buffer[num];
			if (num2 <= 127)
			{
				pAsciiBuffer[num] = (byte)num2;
				num++;
			}
		}
		goto IL_00ef;
	}

	internal unsafe static nuint WidenAsciiToUtf16(byte* pAsciiBuffer, char* pUtf16Buffer, nuint elementCount)
	{
		nuint num = 0u;
		nuint num2 = elementCount - num;
		if (num2 < 4)
		{
			goto IL_0040;
		}
		nuint num3 = num + num2 - 4;
		uint num4;
		while (true)
		{
			num4 = Unsafe.ReadUnaligned<uint>(pAsciiBuffer + num);
			if (!AllBytesInUInt32AreAscii(num4))
			{
				break;
			}
			WidenFourAsciiBytesToUtf16AndWriteToBuffer(ref pUtf16Buffer[num], num4);
			num += 4;
			if (num <= num3)
			{
				continue;
			}
			goto IL_0040;
		}
		goto IL_00d3;
		IL_0040:
		if (((int)num2 & 2) != 0)
		{
			num4 = Unsafe.ReadUnaligned<ushort>(pAsciiBuffer + num);
			if (!AllBytesInUInt32AreAscii(num4))
			{
				if (!BitConverter.IsLittleEndian)
				{
					num4 <<= 16;
				}
				goto IL_00d3;
			}
			if (BitConverter.IsLittleEndian)
			{
				pUtf16Buffer[num] = (char)(byte)num4;
				pUtf16Buffer[num + 1] = (char)(num4 >> 8);
			}
			else
			{
				pUtf16Buffer[num + 1] = (char)(byte)num4;
				pUtf16Buffer[num] = (char)(num4 >> 8);
			}
			num += 2;
		}
		if (((int)num2 & 1) != 0)
		{
			num4 = pAsciiBuffer[num];
			if (((byte)num4 & 0x80) == 0)
			{
				pUtf16Buffer[num] = (char)num4;
				num++;
			}
		}
		goto IL_00d1;
		IL_00d3:
		if (BitConverter.IsLittleEndian)
		{
			while (((byte)num4 & 0x80) == 0)
			{
				pUtf16Buffer[num] = (char)(byte)num4;
				num++;
				num4 >>= 8;
			}
		}
		else
		{
			while ((num4 & 0x80000000u) == 0)
			{
				num4 = BitOperations.RotateLeft(num4, 8);
				pUtf16Buffer[num] = (char)(byte)num4;
				num++;
			}
		}
		goto IL_00d1;
		IL_00d1:
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void WidenFourAsciiBytesToUtf16AndWriteToBuffer(ref char outputBuffer, uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			outputBuffer = (char)(byte)value;
			value >>= 8;
			Unsafe.Add(ref outputBuffer, 1) = (char)(byte)value;
			value >>= 8;
			Unsafe.Add(ref outputBuffer, 2) = (char)(byte)value;
			value >>= 8;
			Unsafe.Add(ref outputBuffer, 3) = (char)value;
		}
		else
		{
			Unsafe.Add(ref outputBuffer, 3) = (char)(byte)value;
			value >>= 8;
			Unsafe.Add(ref outputBuffer, 2) = (char)(byte)value;
			value >>= 8;
			Unsafe.Add(ref outputBuffer, 1) = (char)(byte)value;
			value >>= 8;
			outputBuffer = (char)value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool AllBytesInUInt32AreAscii(uint value)
	{
		return (value & 0x80808080u) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint CountNumberOfLeadingAsciiBytesFromUInt32WithSomeNonAsciiData(uint value)
	{
		if (BitConverter.IsLittleEndian)
		{
			return (uint)BitOperations.TrailingZeroCount(value & 0x80808080u) >> 3;
		}
		value = ~value;
		value = BitOperations.RotateLeft(value, 1);
		uint num = value & 1;
		uint num2 = num;
		value = BitOperations.RotateLeft(value, 8);
		num &= value;
		uint num3 = num2 + num;
		value = BitOperations.RotateLeft(value, 8);
		num &= value;
		return num3 + num;
	}
}
