using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("10a72a66-e91c-43f4-993f-ddf4b82b0b4a")]
public class StrokeStyle1 : StrokeStyle
{
	public StrokeTransformType StrokeTransformType => GetStrokeTransformType();

	public StrokeStyle1(Factory1 factory, StrokeStyleProperties1 strokeStyleProperties)
		: base(IntPtr.Zero)
	{
		factory.CreateStrokeStyle(ref strokeStyleProperties, null, 0, this);
	}

	public StrokeStyle1(Factory1 factory, StrokeStyleProperties1 strokeStyleProperties, float[] dashes)
		: base(IntPtr.Zero)
	{
		factory.CreateStrokeStyle(ref strokeStyleProperties, dashes, dashes.Length, this);
	}

	public StrokeStyle1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator StrokeStyle1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new StrokeStyle1(nativePtr);
		}
		return null;
	}

	internal unsafe StrokeTransformType GetStrokeTransformType()
	{
		return ((delegate* unmanaged[Stdcall]<void*, StrokeTransformType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer);
	}
}
