using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("53dd9855-a3b0-4d5b-82e1-26e25c5e5797")]
public class LookupTable3D : Resource
{
	public LookupTable3D(DeviceContext2 context2, BufferPrecision precision, int[] extents, byte[] data, int dataCount, int[] strides)
		: this(IntPtr.Zero)
	{
		context2.CreateLookupTable3D(precision, extents, data, dataCount, strides, this);
	}

	public LookupTable3D(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator LookupTable3D(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new LookupTable3D(nativePtr);
		}
		return null;
	}
}
