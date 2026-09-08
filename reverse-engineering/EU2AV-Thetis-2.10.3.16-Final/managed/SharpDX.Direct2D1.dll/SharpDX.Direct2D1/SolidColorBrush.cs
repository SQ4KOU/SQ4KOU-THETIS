using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2cd906a9-12e2-11dc-9fed-001143a055f9")]
public class SolidColorBrush : Brush
{
	public RawColor4 Color
	{
		get
		{
			return GetColor();
		}
		set
		{
			SetColor(value);
		}
	}

	public SolidColorBrush(RenderTarget renderTarget, RawColor4 color)
		: this(renderTarget, color, null)
	{
	}

	public SolidColorBrush(RenderTarget renderTarget, RawColor4 color, BrushProperties? brushProperties)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateSolidColorBrush(color, brushProperties, this);
	}

	public SolidColorBrush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SolidColorBrush(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SolidColorBrush(nativePtr);
		}
		return null;
	}

	internal unsafe void SetColor(RawColor4 color)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &color);
	}

	internal unsafe RawColor4 GetColor()
	{
		RawColor4 result = default(RawColor4);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}
}
