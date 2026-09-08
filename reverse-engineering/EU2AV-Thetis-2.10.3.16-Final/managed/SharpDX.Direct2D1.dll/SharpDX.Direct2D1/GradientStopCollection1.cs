using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("ae1572f4-5dd0-4777-998b-9279472ae63b")]
public class GradientStopCollection1 : GradientStopCollection
{
	public ColorSpace PreInterpolationSpace => GetPreInterpolationSpace();

	public ColorSpace PostInterpolationSpace => GetPostInterpolationSpace();

	public BufferPrecision BufferPrecision => GetBufferPrecision();

	public ColorInterpolationMode ColorInterpolationMode => GetColorInterpolationMode();

	public GradientStopCollection1(DeviceContext context, GradientStop[] straightAlphaGradientStops, ColorSpace preInterpolationSpace, ColorSpace postInterpolationSpace, BufferPrecision bufferPrecision, ExtendMode extendMode, ColorInterpolationMode colorInterpolationMode)
		: base(IntPtr.Zero)
	{
		context.CreateGradientStopCollection(straightAlphaGradientStops, straightAlphaGradientStops.Length, preInterpolationSpace, postInterpolationSpace, bufferPrecision, extendMode, colorInterpolationMode, this);
	}

	public GradientStopCollection1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GradientStopCollection1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GradientStopCollection1(nativePtr);
		}
		return null;
	}

	public unsafe void GetGradientStops1(GradientStop[] gradientStops, int gradientStopsCount)
	{
		fixed (GradientStop* ptr = gradientStops)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2, gradientStopsCount);
		}
	}

	internal unsafe ColorSpace GetPreInterpolationSpace()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ColorSpace>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ColorSpace GetPostInterpolationSpace()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ColorSpace>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe BufferPrecision GetBufferPrecision()
	{
		return ((delegate* unmanaged[Stdcall]<void*, BufferPrecision>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ColorInterpolationMode GetColorInterpolationMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ColorInterpolationMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer);
	}
}
