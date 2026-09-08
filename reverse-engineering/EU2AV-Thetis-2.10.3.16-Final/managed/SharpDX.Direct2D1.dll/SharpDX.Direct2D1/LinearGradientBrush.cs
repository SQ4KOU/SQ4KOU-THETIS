using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2cd906ab-12e2-11dc-9fed-001143a055f9")]
public class LinearGradientBrush : Brush
{
	public RawVector2 StartPoint
	{
		get
		{
			return GetStartPoint();
		}
		set
		{
			SetStartPoint(value);
		}
	}

	public RawVector2 EndPoint
	{
		get
		{
			return GetEndPoint();
		}
		set
		{
			SetEndPoint(value);
		}
	}

	public GradientStopCollection GradientStopCollection
	{
		get
		{
			GetGradientStopCollection(out var gradientStopCollection);
			return gradientStopCollection;
		}
	}

	public LinearGradientBrush(RenderTarget renderTarget, LinearGradientBrushProperties linearGradientBrushProperties, GradientStopCollection gradientStopCollection)
		: this(renderTarget, linearGradientBrushProperties, null, gradientStopCollection)
	{
	}

	public LinearGradientBrush(RenderTarget renderTarget, LinearGradientBrushProperties linearGradientBrushProperties, BrushProperties? brushProperties, GradientStopCollection gradientStopCollection)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateLinearGradientBrush(linearGradientBrushProperties, brushProperties, gradientStopCollection, this);
	}

	public LinearGradientBrush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator LinearGradientBrush(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new LinearGradientBrush(nativePtr);
		}
		return null;
	}

	internal unsafe void SetStartPoint(RawVector2 startPoint)
	{
		((delegate* unmanaged[Stdcall]<void*, RawVector2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, startPoint);
	}

	internal unsafe void SetEndPoint(RawVector2 endPoint)
	{
		((delegate* unmanaged[Stdcall]<void*, RawVector2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, endPoint);
	}

	internal unsafe RawVector2 GetStartPoint()
	{
		RawVector2 result = default(RawVector2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe RawVector2 GetEndPoint()
	{
		RawVector2 result = default(RawVector2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe void GetGradientStopCollection(out GradientStopCollection gradientStopCollection)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			gradientStopCollection = new GradientStopCollection(zero);
		}
		else
		{
			gradientStopCollection = null;
		}
	}
}
