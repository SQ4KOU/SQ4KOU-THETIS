using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("acd16696-8c14-4f5d-877e-fe3fc1d32738")]
public class Font1 : Font
{
	public new FontMetrics1 Metrics
	{
		get
		{
			GetMetrics(out var fontMetrics);
			return fontMetrics;
		}
	}

	public Panose Panose
	{
		get
		{
			GetPanose(out var anoseRef);
			return anoseRef;
		}
	}

	public RawBool IsMonospacedFont => IsMonospacedFont_();

	public Font1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Font1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Font1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetMetrics(out FontMetrics1 fontMetrics)
	{
		fontMetrics = default(FontMetrics1);
		fixed (FontMetrics1* ptr = &fontMetrics)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void GetPanose(out Panose anoseRef)
	{
		Panose.__Native @ref = default(Panose.__Native);
		anoseRef = default(Panose);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		anoseRef.__MarshalFrom(ref @ref);
	}

	public unsafe void GetUnicodeRanges(int maxRangeCount, UnicodeRange[] unicodeRanges, out int actualRangeCount)
	{
		Result result;
		fixed (int* ptr = &actualRangeCount)
		{
			void* ptr2 = ptr;
			fixed (UnicodeRange* ptr3 = unicodeRanges)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, maxRangeCount, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe RawBool IsMonospacedFont_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer);
	}
}
