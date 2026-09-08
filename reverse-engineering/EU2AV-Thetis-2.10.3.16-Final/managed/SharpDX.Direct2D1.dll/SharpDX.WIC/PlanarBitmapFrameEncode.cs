using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("F928B7B8-2221-40C1-B72E-7E82F1974D1A")]
public class PlanarBitmapFrameEncode : ComObject
{
	public PlanarBitmapFrameEncode(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PlanarBitmapFrameEncode(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PlanarBitmapFrameEncode(nativePtr);
		}
		return null;
	}

	public unsafe void WritePixels(int lineCount, BitmapPlane[] planesRef, int planes)
	{
		Result result;
		fixed (BitmapPlane* ptr = planesRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, lineCount, ptr2, planes);
		}
		result.CheckError();
	}

	public unsafe void WriteSource(BitmapSource[] planesOut, int planes, RawBox? rcSourceRef)
	{
		IntPtr* ptr = null;
		if (planesOut != null)
		{
			ptr = stackalloc IntPtr[planesOut.Length];
		}
		if (planesOut != null)
		{
			for (int i = 0; i < planesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<BitmapSource>(planesOut[i]);
			}
		}
		RawBox value = default(RawBox);
		if (rcSourceRef.HasValue)
		{
			value = rcSourceRef.Value;
		}
		void* nativePointer = _nativePointer;
		IntPtr* intPtr = ptr;
		RawBox* intPtr2 = ((!rcSourceRef.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, intPtr, planes, intPtr2)).CheckError();
	}

	public unsafe void WriteSource(ComArray<BitmapSource> planesOut, int planes, RawBox? rcSourceRef)
	{
		RawBox value = default(RawBox);
		if (rcSourceRef.HasValue)
		{
			value = rcSourceRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(planesOut?.NativePointer ?? IntPtr.Zero);
		RawBox* intPtr2 = ((!rcSourceRef.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, intPtr, planes, intPtr2)).CheckError();
	}

	private unsafe void WriteSource(IntPtr planesOut, int planes, IntPtr rcSourceRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)planesOut, planes, (void*)rcSourceRef)).CheckError();
	}
}
