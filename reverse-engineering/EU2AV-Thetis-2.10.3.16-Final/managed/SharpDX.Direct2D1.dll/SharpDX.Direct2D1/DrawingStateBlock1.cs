using System;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;

namespace SharpDX.Direct2D1;

[Guid("689f1f85-c72e-4e33-8f19-85754efd5ace")]
public class DrawingStateBlock1 : DrawingStateBlock
{
	public new DrawingStateDescription1 Description
	{
		get
		{
			GetDescription(out var stateDescription);
			return stateDescription;
		}
		set
		{
			SetDescription(ref value);
		}
	}

	public DrawingStateBlock1(Factory1 factory)
		: base(IntPtr.Zero)
	{
		factory.CreateDrawingStateBlock(null, null, this);
	}

	public DrawingStateBlock1(Factory1 factory, DrawingStateDescription1 drawingStateDescription)
		: this(factory, drawingStateDescription, null)
	{
	}

	public DrawingStateBlock1(Factory1 factory, RenderingParams textRenderingParams)
		: base(IntPtr.Zero)
	{
		factory.CreateDrawingStateBlock(null, textRenderingParams, this);
	}

	public DrawingStateBlock1(Factory1 factory, DrawingStateDescription1 drawingStateDescription, RenderingParams textRenderingParams)
		: base(IntPtr.Zero)
	{
		factory.CreateDrawingStateBlock(drawingStateDescription, textRenderingParams, this);
	}

	public DrawingStateBlock1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DrawingStateBlock1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DrawingStateBlock1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out DrawingStateDescription1 stateDescription)
	{
		stateDescription = default(DrawingStateDescription1);
		fixed (DrawingStateDescription1* ptr = &stateDescription)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void SetDescription(ref DrawingStateDescription1 stateDescription)
	{
		fixed (DrawingStateDescription1* ptr = &stateDescription)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
