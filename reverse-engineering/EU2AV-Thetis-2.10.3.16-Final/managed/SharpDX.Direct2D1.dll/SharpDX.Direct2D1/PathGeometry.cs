using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a5-12e2-11dc-9fed-001143a055f9")]
public class PathGeometry : Geometry
{
	public int SegmentCount
	{
		get
		{
			GetSegmentCount(out var count);
			return count;
		}
	}

	public int FigureCount
	{
		get
		{
			GetFigureCount(out var count);
			return count;
		}
	}

	public PathGeometry(Factory factory)
		: base(IntPtr.Zero)
	{
		factory.CreatePathGeometry(this);
	}

	public PathGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PathGeometry(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PathGeometry(nativePtr);
		}
		return null;
	}

	public unsafe GeometrySink Open()
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, &zero);
		GeometrySink result2 = ((!(zero != IntPtr.Zero)) ? null : new GeometrySinkNative(zero));
		result.CheckError();
		return result2;
	}

	public unsafe void Stream(GeometrySink geometrySink)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<GeometrySink>(geometrySink);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetSegmentCount(out int count)
	{
		Result result;
		fixed (int* ptr = &count)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetFigureCount(out int count)
	{
		Result result;
		fixed (int* ptr = &count)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
