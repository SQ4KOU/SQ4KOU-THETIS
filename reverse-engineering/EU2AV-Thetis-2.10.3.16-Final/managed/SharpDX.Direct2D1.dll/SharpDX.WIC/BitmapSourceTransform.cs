using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("3B16811B-6A43-4ec9-B713-3D5A0C13B940")]
public class BitmapSourceTransform : ComObject
{
	public void CopyPixels(int width, int height, int stride, DataStream output)
	{
		CopyPixels(IntPtr.Zero, width, height, null, BitmapTransformOptions.Rotate0, stride, (int)(output.Length - output.Position), output.PositionPointer);
	}

	public void CopyPixels(int width, int height, BitmapTransformOptions dstTransform, int stride, DataStream output)
	{
		CopyPixels(IntPtr.Zero, width, height, null, dstTransform, stride, (int)(output.Length - output.Position), output.PositionPointer);
	}

	public void CopyPixels(int width, int height, Guid guidDstFormat, BitmapTransformOptions dstTransform, int stride, DataStream output)
	{
		CopyPixels(IntPtr.Zero, width, height, guidDstFormat, dstTransform, stride, (int)(output.Length - output.Position), output.PositionPointer);
	}

	public unsafe void CopyPixels(RawBox rectangle, int width, int height, Guid guidDstFormat, BitmapTransformOptions dstTransform, int stride, DataStream output)
	{
		CopyPixels(new IntPtr(&rectangle), width, height, guidDstFormat, dstTransform, stride, (int)(output.Length - output.Position), output.PositionPointer);
	}

	public void GetClosestSize(ref Size2 size)
	{
		int widthRef = size.Width;
		int heightRef = size.Height;
		GetClosestSize(ref widthRef, ref heightRef);
		size.Width = widthRef;
		size.Height = heightRef;
	}

	public BitmapSourceTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapSourceTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapSourceTransform(nativePtr);
		}
		return null;
	}

	internal unsafe void CopyPixels(IntPtr rectangleRef, int width, int height, Guid? guidDstFormatRef, BitmapTransformOptions dstTransform, int nStride, int bufferSize, IntPtr bufferRef)
	{
		Guid value = default(Guid);
		if (guidDstFormatRef.HasValue)
		{
			value = guidDstFormatRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)rectangleRef;
		Guid* intPtr2 = ((!guidDstFormatRef.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(nativePointer, intPtr, width, height, intPtr2, (int)dstTransform, nStride, bufferSize, (void*)bufferRef)).CheckError();
	}

	internal unsafe void GetClosestSize(ref int widthRef, ref int heightRef)
	{
		Result result;
		fixed (int* ptr = &heightRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &widthRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetClosestPixelFormat(ref Guid guidDstFormatRef)
	{
		Result result;
		fixed (Guid* ptr = &guidDstFormatRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void IsSupportingTransform(BitmapTransformOptions dstTransform, out RawBool fIsSupportedRef)
	{
		fIsSupportedRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fIsSupportedRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (int)dstTransform, ptr2);
		}
		result.CheckError();
	}
}
