using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("D8CD007F-D08F-4191-9BFC-236EA7F0E4B5")]
public class BitmapDecoderInfo : BitmapCodecInfo
{
	public BitmapPattern[] Patterns
	{
		get
		{
			int patternsActualRef = 0;
			int atternCountRef = 0;
			GetPatterns(0, null, out atternCountRef, out patternsActualRef);
			if (patternsActualRef == 0)
			{
				return new BitmapPattern[0];
			}
			atternCountRef = patternsActualRef;
			BitmapPattern[] array = new BitmapPattern[patternsActualRef];
			GetPatterns(atternCountRef, array, out atternCountRef, out patternsActualRef);
			return array;
		}
	}

	public BitmapDecoderInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapDecoderInfo(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapDecoderInfo(nativePtr);
		}
		return null;
	}

	internal unsafe void GetPatterns(int sizePatterns, BitmapPattern[] patternsRef, out int atternCountRef, out int patternsActualRef)
	{
		Result result;
		fixed (int* ptr = &patternsActualRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &atternCountRef)
			{
				void* ptr4 = ptr3;
				fixed (BitmapPattern* ptr5 = patternsRef)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, sizePatterns, ptr6, ptr4, ptr2);
				}
			}
		}
		result.CheckError();
	}

	public unsafe RawBool MatchesPattern(IStream streamRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(streamRef);
		RawBool result = default(RawBool);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &result)).CheckError();
		return result;
	}

	internal unsafe void CreateInstance(BitmapDecoder bitmapDecoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmapDecoderOut.NativePointer = zero;
		result.CheckError();
	}
}
