using System;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("8c427831-3d90-4476-b647-c4fae349e4db")]
public class DeviceContext4 : DeviceContext3
{
	public DeviceContext4(Device4 device, DeviceContextOptions options)
		: base(IntPtr.Zero)
	{
		device.CreateDeviceContext(options, this);
	}

	public DeviceContext4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext4(nativePtr);
		}
		return null;
	}

	public unsafe void CreateSvgGlyphStyle(out SvgGlyphStyle svgGlyphStyle)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)108 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			svgGlyphStyle = new SvgGlyphStyle(zero);
		}
		else
		{
			svgGlyphStyle = null;
		}
		result.CheckError();
	}

	public unsafe void DrawText(string text, int stringLength, TextFormat textFormat, RawRectangleF layoutRect, Brush defaultFillBrush, SvgGlyphStyle svgGlyphStyle, int colorPaletteIndex, DrawTextOptions options, MeasuringMode measuringMode)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextFormat>(textFormat);
		zero2 = CppObject.ToCallbackPtr<Brush>(defaultFillBrush);
		zero3 = CppObject.ToCallbackPtr<SvgGlyphStyle>(svgGlyphStyle);
		fixed (char* ptr = text)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, void*, int, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)109 * (nint)sizeof(void*))))(_nativePointer, ptr, stringLength, (void*)zero, &layoutRect, (void*)zero2, (void*)zero3, colorPaletteIndex, (int)options, (int)measuringMode);
		}
	}

	public unsafe void DrawTextLayout(RawVector2 origin, TextLayout textLayout, Brush defaultFillBrush, SvgGlyphStyle svgGlyphStyle, int colorPaletteIndex, DrawTextOptions options)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextLayout>(textLayout);
		zero2 = CppObject.ToCallbackPtr<Brush>(defaultFillBrush);
		zero3 = CppObject.ToCallbackPtr<SvgGlyphStyle>(svgGlyphStyle);
		((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)110 * (nint)sizeof(void*))))(_nativePointer, origin, (void*)zero, (void*)zero2, (void*)zero3, colorPaletteIndex, (int)options);
	}

	public unsafe void DrawColorBitmapGlyphRun(GlyphImageFormatS glyphImageFormat, RawVector2 baselineOrigin, GlyphRun glyphRun, MeasuringMode measuringMode, ColorBitmapGlyphSnapOption bitmapSnapOption)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		glyphRun.__MarshalTo(ref @ref);
		((delegate* unmanaged[Stdcall]<void*, int, RawVector2, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)111 * (nint)sizeof(void*))))(_nativePointer, (int)glyphImageFormat, baselineOrigin, &@ref, (int)measuringMode, (int)bitmapSnapOption);
		glyphRun.__MarshalFree(ref @ref);
	}

	public unsafe void DrawSvgGlyphRun(RawVector2 baselineOrigin, GlyphRun glyphRun, Brush defaultFillBrush, SvgGlyphStyle svgGlyphStyle, int colorPaletteIndex, MeasuringMode measuringMode)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		glyphRun.__MarshalTo(ref @ref);
		zero = CppObject.ToCallbackPtr<Brush>(defaultFillBrush);
		zero2 = CppObject.ToCallbackPtr<SvgGlyphStyle>(svgGlyphStyle);
		((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)112 * (nint)sizeof(void*))))(_nativePointer, baselineOrigin, &@ref, (void*)zero, (void*)zero2, colorPaletteIndex, (int)measuringMode);
		glyphRun.__MarshalFree(ref @ref);
	}

	public unsafe void GetColorBitmapGlyphImage(GlyphImageFormatS glyphImageFormat, RawVector2 glyphOrigin, FontFace fontFace, float fontEmSize, short glyphIndex, RawBool isSideways, RawMatrix3x2? worldTransform, float dpiX, float dpiY, out RawMatrix3x2 glyphTransform, out Image glyphImage)
	{
		IntPtr zero = IntPtr.Zero;
		glyphTransform = default(RawMatrix3x2);
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		Result result;
		fixed (RawMatrix3x2* ptr = &glyphTransform)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			void* intPtr = (void*)zero;
			RawMatrix3x2* intPtr2 = ((!worldTransform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, int, RawVector2, void*, float, short, RawBool, void*, float, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)113 * (nint)sizeof(void*))))(nativePointer, (int)glyphImageFormat, glyphOrigin, intPtr, fontEmSize, glyphIndex, isSideways, intPtr2, dpiX, dpiY, ptr2, &zero2);
		}
		if (zero2 != IntPtr.Zero)
		{
			glyphImage = new Image(zero2);
		}
		else
		{
			glyphImage = null;
		}
		result.CheckError();
	}

	public unsafe void GetSvgGlyphImage(RawVector2 glyphOrigin, FontFace fontFace, float fontEmSize, short glyphIndex, RawBool isSideways, RawMatrix3x2? worldTransform, Brush defaultFillBrush, SvgGlyphStyle svgGlyphStyle, int colorPaletteIndex, out RawMatrix3x2 glyphTransform, out CommandList glyphImage)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		glyphTransform = default(RawMatrix3x2);
		IntPtr zero4 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		zero2 = CppObject.ToCallbackPtr<Brush>(defaultFillBrush);
		zero3 = CppObject.ToCallbackPtr<SvgGlyphStyle>(svgGlyphStyle);
		Result result;
		fixed (RawMatrix3x2* ptr = &glyphTransform)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			void* intPtr = (void*)zero;
			RawMatrix3x2* intPtr2 = ((!worldTransform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, float, short, RawBool, void*, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)114 * (nint)sizeof(void*))))(nativePointer, glyphOrigin, intPtr, fontEmSize, glyphIndex, isSideways, intPtr2, (void*)zero2, (void*)zero3, colorPaletteIndex, ptr2, &zero4);
		}
		if (zero4 != IntPtr.Zero)
		{
			glyphImage = new CommandList(zero4);
		}
		else
		{
			glyphImage = null;
		}
		result.CheckError();
	}
}
