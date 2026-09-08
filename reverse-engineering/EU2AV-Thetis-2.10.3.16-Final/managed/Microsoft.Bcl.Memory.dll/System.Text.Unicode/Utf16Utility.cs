using System.Runtime.CompilerServices;

namespace System.Text.Unicode;

internal static class Utf16Utility
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool AllCharsInUInt32AreAscii(uint value)
	{
		return (value & 0xFF80FF80u) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool AllCharsInUInt64AreAscii(ulong value)
	{
		return (value & 0xFF80FF80FF80FF80uL) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ConvertAllAsciiCharsInUInt32ToLowercase(uint value)
	{
		uint num = value + 8388736 - 4259905;
		uint num2 = value + 8388736 - 5963867;
		uint num3 = ((num ^ num2) & 0x800080) >> 2;
		return value ^ num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ConvertAllAsciiCharsInUInt32ToUppercase(uint value)
	{
		uint num = value + 8388736 - 6357089;
		uint num2 = value + 8388736 - 8061051;
		uint num3 = ((num ^ num2) & 0x800080) >> 2;
		return value ^ num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong ConvertAllAsciiCharsInUInt64ToUppercase(ulong value)
	{
		ulong num = value + 36029346783166592L - 27303489359118433L;
		ulong num2 = value + 36029346783166592L - 34621950424449147L;
		ulong num3 = ((num ^ num2) & 0x80008000800080L) >> 2;
		return value ^ num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong ConvertAllAsciiCharsInUInt64ToLowercase(ulong value)
	{
		ulong num = value + 36029346783166592L - 18296152663326785L;
		ulong num2 = value + 36029346783166592L - 25614613728657499L;
		ulong num3 = ((num ^ num2) & 0x80008000800080L) >> 2;
		return value ^ num3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool UInt32ContainsAnyLowercaseAsciiChar(uint value)
	{
		uint num = value + 8388736 - 6357089;
		uint num2 = value + 8388736 - 8061051;
		return ((num ^ num2) & 0x800080) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool UInt32ContainsAnyUppercaseAsciiChar(uint value)
	{
		uint num = value + 8388736 - 4259905;
		uint num2 = value + 8388736 - 5963867;
		return ((num ^ num2) & 0x800080) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool UInt32OrdinalIgnoreCaseAscii(uint valueA, uint valueB)
	{
		uint num = (valueA ^ valueB) << 2;
		uint num2 = valueA + 327685;
		num2 |= 0xA000A0;
		num2 += 1703962;
		num2 |= 0xFF7FFF7Fu;
		return (num & num2) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool UInt64OrdinalIgnoreCaseAscii(ulong valueA, ulong valueB)
	{
		ulong num = (valueA ^ valueB) << 2;
		ulong num2 = valueA + 1407396358717445L;
		num2 |= 0xA000A000A000A0L;
		num2 += 7318461065330714L;
		num2 |= 0xFF7FFF7FFF7FFF7FuL;
		return (num & num2) == 0;
	}
}
