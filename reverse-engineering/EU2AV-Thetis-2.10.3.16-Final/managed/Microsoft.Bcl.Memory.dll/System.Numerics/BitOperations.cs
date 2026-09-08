using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Numerics;

internal static class BitOperations
{
	private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
	{
		0, 1, 28, 2, 29, 14, 24, 3, 30, 22,
		20, 15, 25, 17, 4, 8, 31, 27, 13, 23,
		21, 19, 16, 7, 26, 12, 18, 6, 11, 5,
		10, 9
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int TrailingZeroCount(uint value)
	{
		if (value == 0)
		{
			return 32;
		}
		return Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn), (IntPtr)(int)((value & (0 - value)) * 125613361 >> 27));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint RotateLeft(uint value, int offset)
	{
		return (value << offset) | (value >> 32 - offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint RotateRight(uint value, int offset)
	{
		return (value >> offset) | (value << 32 - offset);
	}
}
