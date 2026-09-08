using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("1edd9491-9853-4299-898f-6432983b6f3a")]
public class GdiInterop : ComObject
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class LogFont
	{
		public int lfHeight;

		public int lfWidth;

		public int lfEscapement;

		public int lfOrientation;

		public int lfWeight;

		public byte lfItalic;

		public byte lfUnderline;

		public byte lfStrikeOut;

		public byte lfCharSet;

		public byte lfOutPrecision;

		public byte lfClipPrecision;

		public byte lfQuality;

		public byte lfPitchAndFamily;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string lfFaceName;
	}

	public struct FontSignature
	{
		public int fsUsb1;

		public int fsUsb2;

		public int fsUsb3;

		public int fsUsb4;

		public int fsCsb1;

		public int fsCsb2;
	}

	public unsafe Font FromLogFont(object logFont)
	{
		int num = Marshal.SizeOf(logFont);
		byte* value = stackalloc byte[(int)(uint)num];
		Marshal.StructureToPtr(logFont, new IntPtr(value), fDeleteOld: false);
		CreateFontFromLOGFONT(new IntPtr(value), out var font);
		return font;
	}

	public unsafe bool ToLogFont(Font font, object logFont)
	{
		int num = Marshal.SizeOf(logFont);
		byte* value = stackalloc byte[(int)(uint)num];
		ConvertFontToLOGFONT(font, new IntPtr(value), out var isSystemFont);
		Marshal.PtrToStructure(new IntPtr(value), logFont);
		return isSystemFont;
	}

	public GdiInterop(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GdiInterop(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GdiInterop(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateFontFromLOGFONT(IntPtr logFont, out Font font)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)logFont, &zero);
		if (zero != IntPtr.Zero)
		{
			font = new Font(zero);
		}
		else
		{
			font = null;
		}
		result.CheckError();
	}

	internal unsafe void ConvertFontToLOGFONT(Font font, IntPtr logFont, out RawBool isSystemFont)
	{
		IntPtr zero = IntPtr.Zero;
		isSystemFont = default(RawBool);
		zero = CppObject.ToCallbackPtr<Font>(font);
		Result result;
		fixed (RawBool* ptr = &isSystemFont)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)logFont, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void ConvertFontFaceToLOGFONT(FontFace font, IntPtr logFont)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(font);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)logFont)).CheckError();
	}

	public unsafe FontFace CreateFontFaceFromHdc(IntPtr hdc)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)hdc, &zero);
		FontFace result2 = ((!(zero != IntPtr.Zero)) ? null : new FontFace(zero));
		result.CheckError();
		return result2;
	}

	public unsafe BitmapRenderTarget CreateBitmapRenderTarget(IntPtr hdc, int width, int height)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)hdc, width, height, &zero);
		BitmapRenderTarget result2 = ((!(zero != IntPtr.Zero)) ? null : new BitmapRenderTarget(zero));
		result.CheckError();
		return result2;
	}
}
