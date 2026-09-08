using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("a16907d7-bc02-4801-99e8-8cf7f485f774")]
public class GeometryRealization : Resource
{
	public GeometryRealization(DeviceContext1 context, Geometry geometry, float flatteningTolerance)
		: this(IntPtr.Zero)
	{
		context.CreateFilledGeometryRealization(geometry, flatteningTolerance, this);
	}

	public GeometryRealization(DeviceContext1 context, Geometry geometry, float flatteningTolerance, float strokeWidth, StrokeStyle strokeStyle)
		: this(IntPtr.Zero)
	{
		context.CreateStrokedGeometryRealization(geometry, flatteningTolerance, strokeWidth, strokeStyle, this);
	}

	public GeometryRealization(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GeometryRealization(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GeometryRealization(nativePtr);
		}
		return null;
	}
}
