using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("62baa2d2-ab54-41b7-b872-787e0106a421")]
public class PathGeometry1 : PathGeometry
{
	public PathGeometry1(Factory1 factory)
		: base(IntPtr.Zero)
	{
		factory.CreatePathGeometry(this);
	}

	public PathGeometry1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PathGeometry1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PathGeometry1(nativePtr);
		}
		return null;
	}

	public unsafe void ComputePointAndSegmentAtLength(float length, int startSegment, RawMatrix3x2? worldTransform, float flatteningTolerance, out PointDescription ointDescriptionRef)
	{
		ointDescriptionRef = default(PointDescription);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		Result result;
		fixed (PointDescription* ptr = &ointDescriptionRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, float, int, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(nativePointer, length, startSegment, intPtr, flatteningTolerance, ptr2);
		}
		result.CheckError();
	}
}
