using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("b499923b-7029-478f-a8b3-432c7c5f5312")]
public class Ink : Resource
{
	public InkPoint StartPoint
	{
		get
		{
			return GetStartPoint();
		}
		set
		{
			SetStartPoint(value);
		}
	}

	public InkBezierSegment SegmentAtEnd
	{
		set
		{
			SetSegmentAtEnd(ref value);
		}
	}

	public int SegmentCount => GetSegmentCount();

	public Ink(DeviceContext2 context2, InkPoint startPoint)
		: this(IntPtr.Zero)
	{
		context2.CreateInk(startPoint, this);
	}

	public Ink(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Ink(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Ink(nativePtr);
		}
		return null;
	}

	internal unsafe void SetStartPoint(InkPoint startPoint)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &startPoint);
	}

	internal unsafe InkPoint GetStartPoint()
	{
		InkPoint result = default(InkPoint);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	public unsafe void AddSegments(InkBezierSegment[] segments, int segmentsCount)
	{
		Result result;
		fixed (InkBezierSegment* ptr = segments)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr2, segmentsCount);
		}
		result.CheckError();
	}

	public unsafe void RemoveSegmentsAtEnd(int segmentsCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, segmentsCount)).CheckError();
	}

	public unsafe void SetSegments(int startSegment, InkBezierSegment[] segments, int segmentsCount)
	{
		Result result;
		fixed (InkBezierSegment* ptr = segments)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, startSegment, ptr2, segmentsCount);
		}
		result.CheckError();
	}

	internal unsafe void SetSegmentAtEnd(ref InkBezierSegment segment)
	{
		Result result;
		fixed (InkBezierSegment* ptr = &segment)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe int GetSegmentCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetSegments(int startSegment, InkBezierSegment[] segments, int segmentsCount)
	{
		Result result;
		fixed (InkBezierSegment* ptr = segments)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, startSegment, ptr2, segmentsCount);
		}
		result.CheckError();
	}

	public unsafe void StreamAsGeometry(InkStyle inkStyle, RawMatrix3x2? worldTransform, float flatteningTolerance, SimplifiedGeometrySink geometrySink)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<InkStyle>(inkStyle);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		zero2 = CppObject.ToCallbackPtr<SimplifiedGeometrySink>(geometrySink);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawMatrix3x2* intPtr2 = ((!worldTransform.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, flatteningTolerance, (void*)zero2)).CheckError();
	}

	public unsafe void GetBounds(InkStyle inkStyle, RawMatrix3x2? worldTransform, out RawRectangleF bounds)
	{
		IntPtr zero = IntPtr.Zero;
		bounds = default(RawRectangleF);
		zero = CppObject.ToCallbackPtr<InkStyle>(inkStyle);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		Result result;
		fixed (RawRectangleF* ptr = &bounds)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			void* intPtr = (void*)zero;
			RawMatrix3x2* intPtr2 = ((!worldTransform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, ptr2);
		}
		result.CheckError();
	}
}
