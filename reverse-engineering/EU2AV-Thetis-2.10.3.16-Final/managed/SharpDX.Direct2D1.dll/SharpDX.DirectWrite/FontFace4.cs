using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

[Guid("27F2A904-4EB8-441D-9678-0563F53E3E2F")]
public class FontFace4 : FontFace3
{
	public FontFace4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFace4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFace4(nativePtr);
		}
		return null;
	}

	public unsafe void GetGlyphImageFormats(short glyphId, int pixelsPerEmFirst, int pixelsPerEmLast, out GlyphImageFormatS glyphImageFormats)
	{
		Result result;
		fixed (GlyphImageFormatS* ptr = &glyphImageFormats)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, short, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)49 * (nint)sizeof(void*))))(_nativePointer, glyphId, pixelsPerEmFirst, pixelsPerEmLast, ptr2);
		}
		result.CheckError();
	}

	public unsafe GlyphImageFormatS GetGlyphImageFormats()
	{
		return ((delegate* unmanaged[Stdcall]<void*, GlyphImageFormatS>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)50 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetGlyphImageData(short glyphId, int pixelsPerEm, GlyphImageFormatS glyphImageFormat, out GlyphImageData glyphData, out IntPtr glyphDataContext)
	{
		glyphData = default(GlyphImageData);
		Result result;
		fixed (IntPtr* ptr = &glyphDataContext)
		{
			void* ptr2 = ptr;
			fixed (GlyphImageData* ptr3 = &glyphData)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, short, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)51 * (nint)sizeof(void*))))(_nativePointer, glyphId, pixelsPerEm, (int)glyphImageFormat, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void ReleaseGlyphImageData(IntPtr glyphDataContext)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)52 * (nint)sizeof(void*))))(_nativePointer, (void*)glyphDataContext);
	}
}
