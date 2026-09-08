using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2cd9069f-12e2-11dc-9fed-001143a055f9")]
internal class GeometrySinkNative : SimplifiedGeometrySinkNative, GeometrySink, SimplifiedGeometrySink, IUnknown, ICallbackable, IDisposable
{
	public void AddLine(RawVector2 point)
	{
		AddLine_(point);
	}

	public void AddBezier(BezierSegment bezier)
	{
		AddBezier_(ref bezier);
	}

	public void AddQuadraticBezier(QuadraticBezierSegment bezier)
	{
		AddQuadraticBezier_(bezier);
	}

	public void AddQuadraticBeziers(QuadraticBezierSegment[] beziers)
	{
		AddQuadraticBeziers_(beziers, beziers.Length);
	}

	public void AddArc(ArcSegment arc)
	{
		AddArc_(ref arc);
	}

	public GeometrySinkNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GeometrySinkNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GeometrySinkNative(nativePtr);
		}
		return null;
	}

	internal unsafe void AddLine_(RawVector2 point)
	{
		((delegate* unmanaged[Stdcall]<void*, RawVector2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, point);
	}

	internal unsafe void AddBezier_(ref BezierSegment bezier)
	{
		fixed (BezierSegment* ptr = &bezier)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void AddQuadraticBezier_(QuadraticBezierSegment bezier)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &bezier);
	}

	internal unsafe void AddQuadraticBeziers_(QuadraticBezierSegment[] beziers, int beziersCount)
	{
		fixed (QuadraticBezierSegment* ptr = beziers)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, ptr2, beziersCount);
		}
	}

	internal unsafe void AddArc_(ref ArcSegment arc)
	{
		fixed (ArcSegment* ptr = &arc)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
