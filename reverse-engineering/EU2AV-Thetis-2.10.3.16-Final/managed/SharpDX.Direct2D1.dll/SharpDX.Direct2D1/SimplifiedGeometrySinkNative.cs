using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2cd9069e-12e2-11dc-9fed-001143a055f9")]
internal class SimplifiedGeometrySinkNative : ComObject, SimplifiedGeometrySink, IUnknown, ICallbackable, IDisposable
{
	public FillMode FillMode_
	{
		set
		{
			SetFillMode_(value);
		}
	}

	public PathSegment SegmentFlags_
	{
		set
		{
			SetSegmentFlags_(value);
		}
	}

	public void AddBeziers(BezierSegment[] beziers)
	{
		AddBeziers_(beziers, beziers.Length);
	}

	public void AddLines(RawVector2[] points)
	{
		AddLines_(points, points.Length);
	}

	public void BeginFigure(RawVector2 startPoint, FigureBegin figureBegin)
	{
		BeginFigure_(startPoint, figureBegin);
	}

	public void Close()
	{
		Close_();
	}

	public void EndFigure(FigureEnd figureEnd)
	{
		EndFigure_(figureEnd);
	}

	public void SetFillMode(FillMode fillMode)
	{
		SetFillMode_(fillMode);
	}

	public void SetSegmentFlags(PathSegment vertexFlags)
	{
		SetSegmentFlags_(vertexFlags);
	}

	public SimplifiedGeometrySinkNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SimplifiedGeometrySinkNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SimplifiedGeometrySinkNative(nativePtr);
		}
		return null;
	}

	internal unsafe void SetFillMode_(FillMode fillMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (int)fillMode);
	}

	internal unsafe void SetSegmentFlags_(PathSegment vertexFlags)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)vertexFlags);
	}

	internal unsafe void BeginFigure_(RawVector2 startPoint, FigureBegin figureBegin)
	{
		((delegate* unmanaged[Stdcall]<void*, RawVector2, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, startPoint, (int)figureBegin);
	}

	internal unsafe void AddLines_(RawVector2[] ointsRef, int pointsCount)
	{
		fixed (RawVector2* ptr = ointsRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr2, pointsCount);
		}
	}

	internal unsafe void AddBeziers_(BezierSegment[] beziers, int beziersCount)
	{
		fixed (BezierSegment* ptr = beziers)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2, beziersCount);
		}
	}

	internal unsafe void EndFigure_(FigureEnd figureEnd)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)figureEnd);
	}

	internal unsafe void Close_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
