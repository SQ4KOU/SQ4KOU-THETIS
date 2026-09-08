using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("9dbe4c0d-3572-4dd9-9825-5530813bb712")]
public class SvgPointCollection : SvgAttribute
{
	public int PointsCount => GetPointsCount();

	public SvgPointCollection(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SvgPointCollection(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SvgPointCollection(nativePtr);
		}
		return null;
	}

	public unsafe void RemovePointsAtEnd(int pointsCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, pointsCount)).CheckError();
	}

	public unsafe void UpdatePoints(RawVector2[] ointsRef, int pointsCount, int startIndex)
	{
		Result result;
		fixed (RawVector2* ptr = ointsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2, pointsCount, startIndex);
		}
		result.CheckError();
	}

	public unsafe void GetPoints(RawVector2[] ointsRef, int pointsCount, int startIndex)
	{
		Result result;
		fixed (RawVector2* ptr = ointsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2, pointsCount, startIndex);
		}
		result.CheckError();
	}

	internal unsafe int GetPointsCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}
}
