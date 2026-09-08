using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("3fe6adea-7643-4f53-bd14-a0ce63f24042")]
public class OffsetTransform : TransformNodeNative
{
	public RawPoint Offset
	{
		get
		{
			return GetOffset();
		}
		set
		{
			SetOffset(value);
		}
	}

	public OffsetTransform(EffectContext context, RawPoint offset)
		: base(IntPtr.Zero)
	{
		context.CreateOffsetTransform(offset, this);
	}

	public OffsetTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator OffsetTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new OffsetTransform(nativePtr);
		}
		return null;
	}

	internal unsafe void SetOffset(RawPoint offset)
	{
		((delegate* unmanaged[Stdcall]<void*, RawPoint, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, offset);
	}

	internal unsafe RawPoint GetOffset()
	{
		RawPoint result = default(RawPoint);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}
}
