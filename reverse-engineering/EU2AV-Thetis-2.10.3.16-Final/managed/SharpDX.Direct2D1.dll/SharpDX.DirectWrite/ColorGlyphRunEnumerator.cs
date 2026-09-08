using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("d31fbe17-f157-41a2-8d24-cb779e0560e8")]
public class ColorGlyphRunEnumerator : ComObject
{
	public unsafe ColorGlyphRun CurrentRun
	{
		get
		{
			GetCurrentRun(out var colorGlyphRun);
			ColorGlyphRun result = default(ColorGlyphRun);
			result.__MarshalFrom(ref *(ColorGlyphRun.__Native*)(void*)colorGlyphRun);
			return result;
		}
	}

	public ColorGlyphRunEnumerator(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ColorGlyphRunEnumerator(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ColorGlyphRunEnumerator(nativePtr);
		}
		return null;
	}

	public unsafe void MoveNext(out RawBool hasRun)
	{
		hasRun = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &hasRun)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetCurrentRun(out IntPtr colorGlyphRun)
	{
		Result result;
		fixed (IntPtr* ptr = &colorGlyphRun)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
