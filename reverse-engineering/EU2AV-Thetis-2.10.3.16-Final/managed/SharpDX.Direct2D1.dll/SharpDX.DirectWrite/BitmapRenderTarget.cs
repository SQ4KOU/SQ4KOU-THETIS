using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("5e5a32a3-8dff-4773-9ff6-0696eab77267")]
public class BitmapRenderTarget : ComObject
{
	public IntPtr MemoryDC => GetMemoryDC();

	public float PixelsPerDip
	{
		get
		{
			return GetPixelsPerDip();
		}
		set
		{
			SetPixelsPerDip(value);
		}
	}

	public RawMatrix3x2 CurrentTransform
	{
		get
		{
			GetCurrentTransform(out var transform);
			return transform;
		}
		set
		{
			SetCurrentTransform(value);
		}
	}

	public Size2 Size
	{
		get
		{
			GetSize(out var size);
			return size;
		}
	}

	public void DrawGlyphRun(float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, RenderingParams renderingParams, RawColorBGRA textColor, out RawRectangle blackBoxRect)
	{
		int textColor2 = textColor.R | (textColor.G << 8) | (textColor.B << 16);
		DrawGlyphRun(baselineOriginX, baselineOriginY, measuringMode, glyphRun, renderingParams, textColor2, out blackBoxRect);
	}

	public void DrawGlyphRun(float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, RenderingParams renderingParams, RawColorBGRA textColor)
	{
		DrawGlyphRun(baselineOriginX, baselineOriginY, measuringMode, glyphRun, renderingParams, textColor, out var _);
	}

	public BitmapRenderTarget(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapRenderTarget(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapRenderTarget(nativePtr);
		}
		return null;
	}

	private unsafe void DrawGlyphRun(float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, RenderingParams renderingParams, int textColor, out RawRectangle blackBoxRect)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		IntPtr zero = IntPtr.Zero;
		blackBoxRect = default(RawRectangle);
		glyphRun.__MarshalTo(ref @ref);
		zero = CppObject.ToCallbackPtr<RenderingParams>(renderingParams);
		Result result;
		fixed (RawRectangle* ptr = &blackBoxRect)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, float, float, int, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, baselineOriginX, baselineOriginY, (int)measuringMode, &@ref, (void*)zero, textColor, ptr2);
		}
		glyphRun.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe IntPtr GetMemoryDC()
	{
		return ((delegate* unmanaged[Stdcall]<void*, IntPtr>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetPixelsPerDip()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetPixelsPerDip(float pixelsPerDip)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, pixelsPerDip)).CheckError();
	}

	internal unsafe void GetCurrentTransform(out RawMatrix3x2 transform)
	{
		transform = default(RawMatrix3x2);
		Result result;
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void SetCurrentTransform(RawMatrix3x2? transform)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr)).CheckError();
	}

	internal unsafe void GetSize(out Size2 size)
	{
		size = default(Size2);
		Result result;
		fixed (Size2* ptr = &size)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void Resize(int width, int height)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, width, height)).CheckError();
	}
}
