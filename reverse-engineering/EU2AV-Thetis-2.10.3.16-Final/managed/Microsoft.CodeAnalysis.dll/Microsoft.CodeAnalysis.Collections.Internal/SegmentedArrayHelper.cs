using System;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.Collections.Internal;

internal static class SegmentedArrayHelper
{
	internal static class TestAccessor
	{
		public static int CalculateSegmentSize(int elementSize)
		{
			return SegmentedArrayHelper.CalculateSegmentSize(elementSize);
		}

		public static int CalculateSegmentShift(int segmentSize)
		{
			return SegmentedArrayHelper.CalculateSegmentShift(segmentSize);
		}

		public static int CalculateOffsetMask(int segmentSize)
		{
			return SegmentedArrayHelper.CalculateOffsetMask(segmentSize);
		}
	}

	private static class FallbackSegmentHelper<T>
	{
		public static readonly int SegmentSize = CalculateSegmentSize(Unsafe.SizeOf<T>());

		public static readonly int SegmentShift = CalculateSegmentShift(SegmentSize);

		public static readonly int OffsetMask = CalculateOffsetMask(SegmentSize);
	}

	internal const int IntrosortSizeThreshold = 16;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetSegmentSize<T>()
	{
		return Unsafe.SizeOf<T>() switch
		{
			1 => 65536, 
			2 => 32768, 
			4 => 16384, 
			8 => 8192, 
			12 => 4096, 
			16 => 4096, 
			24 => 2048, 
			28 => 2048, 
			32 => 2048, 
			40 => 2048, 
			64 => 1024, 
			_ => FallbackSegmentHelper<T>.SegmentSize, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetSegmentShift<T>()
	{
		return Unsafe.SizeOf<T>() switch
		{
			1 => 16, 
			2 => 15, 
			4 => 14, 
			8 => 13, 
			12 => 12, 
			16 => 12, 
			24 => 11, 
			28 => 11, 
			32 => 11, 
			40 => 11, 
			64 => 10, 
			_ => FallbackSegmentHelper<T>.SegmentShift, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetOffsetMask<T>()
	{
		return Unsafe.SizeOf<T>() switch
		{
			1 => 65535, 
			2 => 32767, 
			4 => 16383, 
			8 => 8191, 
			12 => 4095, 
			16 => 4095, 
			24 => 2047, 
			28 => 2047, 
			32 => 2047, 
			40 => 2047, 
			64 => 1023, 
			_ => FallbackSegmentHelper<T>.OffsetMask, 
		};
	}

	private static int CalculateSegmentSize(int elementSize)
	{
		int num = 2;
		while (ArraySize(elementSize, num << 1) < 85000)
		{
			num <<= 1;
		}
		return num;
		static int ArraySize(int num2, int segmentSize)
		{
			return 2 * IntPtr.Size + 8 + num2 * segmentSize;
		}
	}

	private static int CalculateSegmentShift(int segmentSize)
	{
		int num = 0;
		while ((segmentSize >>= 1) != 0)
		{
			num++;
		}
		return num;
	}

	private static int CalculateOffsetMask(int segmentSize)
	{
		return segmentSize - 1;
	}
}
