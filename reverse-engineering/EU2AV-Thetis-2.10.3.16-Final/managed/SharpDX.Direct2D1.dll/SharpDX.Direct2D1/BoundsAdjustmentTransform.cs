using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("90f732e2-5092-4606-a819-8651970baccd")]
public class BoundsAdjustmentTransform : TransformNodeNative
{
	public RawRectangle OutputBounds
	{
		get
		{
			GetOutputBounds(out var outputBounds);
			return outputBounds;
		}
		set
		{
			SetOutputBounds(value);
		}
	}

	public BoundsAdjustmentTransform(EffectContext context, RawRectangle outputRectangle)
		: base(IntPtr.Zero)
	{
		context.CreateBoundsAdjustmentTransform(outputRectangle, this);
	}

	public BoundsAdjustmentTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BoundsAdjustmentTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BoundsAdjustmentTransform(nativePtr);
		}
		return null;
	}

	internal unsafe void SetOutputBounds(RawRectangle outputBounds)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &outputBounds);
	}

	internal unsafe void GetOutputBounds(out RawRectangle outputBounds)
	{
		outputBounds = default(RawRectangle);
		fixed (RawRectangle* ptr = &outputBounds)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
