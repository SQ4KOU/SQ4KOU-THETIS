using System;
using System.Runtime.CompilerServices;
using SharpDX.DXGI;

namespace Thetis;

public static class WaterfallPixelWriter
{
	public static int PixelSize { get; private set; } = 4;

	public static Format DxgiFormat { get; private set; } = Format.B8G8R8A8_UNorm;

	public static WaterfallEnhancer.ColorDepth Depth { get; private set; } = WaterfallEnhancer.ColorDepth.Bit8;

	public static void UpdateFormat()
	{
		Depth = WaterfallEnhancer.Depth;
		WaterfallEnhancer.ColorDepth depth = Depth;
		if (depth == WaterfallEnhancer.ColorDepth.Bit8 || depth != WaterfallEnhancer.ColorDepth.Bit16)
		{
			PixelSize = 4;
			DxgiFormat = Format.B8G8R8A8_UNorm;
		}
		else
		{
			PixelSize = 8;
			DxgiFormat = Format.R16G16B16A16_Float;
		}
	}

	public static void EncodeRow(float[] rowF, byte[] row, int W)
	{
		switch (Depth)
		{
		case WaterfallEnhancer.ColorDepth.Bit8:
			EncodeRow8(rowF, row, W);
			break;
		case WaterfallEnhancer.ColorDepth.Bit10:
			EncodeRow10(rowF, row, W);
			break;
		case WaterfallEnhancer.ColorDepth.Bit16:
			EncodeRow16(rowF, row, W);
			break;
		}
	}

	public static void EncodeRow8Forced(float[] rowF, byte[] row, int W)
	{
		EncodeRow8(rowF, row, W);
	}

	private static void EncodeRow8(float[] rowF, byte[] row, int W)
	{
		for (int i = 0; i < W; i++)
		{
			int num = i * 4;
			int num2 = i * 4;
			float v = ClampByte(rowF[num]);
			float v2 = ClampByte(rowF[num + 1]);
			float v3 = ClampByte(rowF[num + 2]);
			float v4 = ClampByte(rowF[num + 3]);
			row[num2] = FastRoundToByte(v3);
			row[num2 + 1] = FastRoundToByte(v2);
			row[num2 + 2] = FastRoundToByte(v);
			row[num2 + 3] = FastRoundToByte(v4);
		}
	}

	private static void EncodeRow10(float[] rowF, byte[] row, int W)
	{
		for (int i = 0; i < W; i++)
		{
			int num = i * 4;
			int num2 = i * 4;
			float num3 = ClampByte(rowF[num]);
			float num4 = ClampByte(rowF[num + 1]);
			float num5 = ClampByte(rowF[num + 2]);
			float num6 = ClampByte(rowF[num + 3]);
			int num7 = RoundToInt(num3 * 4.0117645f);
			int num8 = RoundToInt(num4 * 4.0117645f);
			int num9 = RoundToInt(num5 * 4.0117645f);
			int num10 = RoundToInt(num6 * (1f / 85f));
			if (num7 < 0)
			{
				num7 = 0;
			}
			else if (num7 > 1023)
			{
				num7 = 1023;
			}
			if (num8 < 0)
			{
				num8 = 0;
			}
			else if (num8 > 1023)
			{
				num8 = 1023;
			}
			if (num9 < 0)
			{
				num9 = 0;
			}
			else if (num9 > 1023)
			{
				num9 = 1023;
			}
			if (num10 < 0)
			{
				num10 = 0;
			}
			else if (num10 > 3)
			{
				num10 = 3;
			}
			uint num11 = (uint)(num7 | (num8 << 10) | (num9 << 20) | (num10 << 30));
			row[num2] = (byte)(num11 & 0xFF);
			row[num2 + 1] = (byte)((num11 >> 8) & 0xFF);
			row[num2 + 2] = (byte)((num11 >> 16) & 0xFF);
			row[num2 + 3] = (byte)((num11 >> 24) & 0xFF);
		}
	}

	private static void EncodeRow16(float[] rowF, byte[] row, int W)
	{
		for (int i = 0; i < W; i++)
		{
			int num = i * 4;
			int num2 = i * 8;
			float value = ClampByte(rowF[num]) * 0.003921569f;
			float value2 = ClampByte(rowF[num + 1]) * 0.003921569f;
			float value3 = ClampByte(rowF[num + 2]) * 0.003921569f;
			float value4 = ClampByte(rowF[num + 3]) * 0.003921569f;
			WriteHalf(row, num2, value);
			WriteHalf(row, num2 + 2, value2);
			WriteHalf(row, num2 + 4, value3);
			WriteHalf(row, num2 + 6, value4);
		}
	}

	private static void WriteHalf(byte[] row, int offset, float value)
	{
		ushort num = FloatToHalfBits(value);
		row[offset] = (byte)(num & 0xFF);
		row[offset + 1] = (byte)((num >> 8) & 0xFF);
	}

	public static ushort FloatToHalfBitsPublic(float value)
	{
		return FloatToHalfBits(value);
	}

	public static float SrgbToLinear(float v)
	{
		if (v <= 0.04045f)
		{
			return v / 12.92f;
		}
		return (float)Math.Pow(((double)v + 0.055) / 1.055, 2.4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static ushort FloatToHalfBits(float value)
	{
		int num = *(int*)(&value);
		uint num2 = (uint)((num >>> 16) & 0x8000);
		uint num3 = (uint)(num & 0x7FFFFF);
		int num4 = ((num >>> 23) & 0xFF) - 127 + 15;
		if (num4 <= 0)
		{
			if (num4 < -10)
			{
				return (ushort)num2;
			}
			num3 |= 0x800000;
			uint num5 = (uint)(14 - num4);
			uint num6 = num3 >> (int)num5;
			return (ushort)(num2 | num6);
		}
		if (num4 == 143)
		{
			if (num3 != 0)
			{
				return (ushort)(num2 | 0x7E00);
			}
			return (ushort)(num2 | 0x7C00);
		}
		if (num4 > 30)
		{
			return (ushort)(num2 | 0x7C00);
		}
		return (ushort)(num2 | (uint)(num4 << 10) | (num3 >> 13));
	}

	public static void FillClearBuffer(byte[] buf, int byteCount)
	{
		switch (Depth)
		{
		case WaterfallEnhancer.ColorDepth.Bit8:
		{
			for (int j = 0; j + 3 < byteCount; j += 4)
			{
				buf[j] = 0;
				buf[j + 1] = 0;
				buf[j + 2] = 0;
				buf[j + 3] = byte.MaxValue;
			}
			break;
		}
		case WaterfallEnhancer.ColorDepth.Bit10:
		{
			for (int k = 0; k + 3 < byteCount; k += 4)
			{
				buf[k] = 0;
				buf[k + 1] = 0;
				buf[k + 2] = 0;
				buf[k + 3] = 192;
			}
			break;
		}
		case WaterfallEnhancer.ColorDepth.Bit16:
		{
			for (int i = 0; i + 7 < byteCount; i += 8)
			{
				buf[i] = 0;
				buf[i + 1] = 0;
				buf[i + 2] = 0;
				buf[i + 3] = 0;
				buf[i + 4] = 0;
				buf[i + 5] = 0;
				buf[i + 6] = 0;
				buf[i + 7] = 60;
			}
			break;
		}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float ClampByte(float v)
	{
		if (v < 0f)
		{
			return 0f;
		}
		if (v > 255f)
		{
			return 255f;
		}
		return v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte FastRoundToByte(float v)
	{
		int num = (int)(v + 0.5f);
		if (num < 0)
		{
			num = 0;
		}
		else if (num > 255)
		{
			num = 255;
		}
		return (byte)num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int RoundToInt(float v)
	{
		return (int)(v + 0.5f);
	}
}
