using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("d8b768ff-64bc-4e66-982b-ec8e87f693f7")]
public class FontFace2 : FontFace1
{
	public RawBool IsColorFont => IsColorFont_();

	public int ColorPaletteCount => GetColorPaletteCount();

	public int PaletteEntryCount => GetPaletteEntryCount();

	public FontFace2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFace2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFace2(nativePtr);
		}
		return null;
	}

	internal unsafe RawBool IsColorFont_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetColorPaletteCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetPaletteEntryCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetPaletteEntries(int colorPaletteIndex, int firstEntryIndex, int entryCount, RawColor4[] aletteEntriesRef)
	{
		Result result;
		fixed (RawColor4* ptr = aletteEntriesRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, colorPaletteIndex, firstEntryIndex, entryCount, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetRecommendedRenderingMode(float fontEmSize, float dpiX, float dpiY, RawMatrix3x2? transform, RawBool isSideways, OutlineThreshold outlineThreshold, MeasuringMode measuringMode, RenderingParams renderingParams, out RenderingMode renderingMode, out GridFitMode gridFitMode)
	{
		IntPtr zero = IntPtr.Zero;
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		zero = CppObject.ToCallbackPtr<RenderingParams>(renderingParams);
		Result result;
		fixed (GridFitMode* ptr = &gridFitMode)
		{
			void* ptr2 = ptr;
			fixed (RenderingMode* ptr3 = &renderingMode)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
				result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, void*, RawBool, int, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(nativePointer, fontEmSize, dpiX, dpiY, intPtr, isSideways, (int)outlineThreshold, (int)measuringMode, (void*)zero, ptr4, ptr2);
			}
		}
		result.CheckError();
	}
}
