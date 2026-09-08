using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2cd906bb-12e2-11dc-9fed-001143a055f9")]
public class TransformedGeometry : Geometry
{
	public Geometry SourceGeometry
	{
		get
		{
			GetSourceGeometry(out var sourceGeometry);
			return sourceGeometry;
		}
	}

	public RawMatrix3x2 Transform
	{
		get
		{
			GetTransform(out var transform);
			return transform;
		}
	}

	public TransformedGeometry(Factory factory, Geometry geometrySource, RawMatrix3x2 matrix3X2)
		: base(IntPtr.Zero)
	{
		factory.CreateTransformedGeometry(geometrySource, ref matrix3X2, this);
	}

	public TransformedGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TransformedGeometry(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TransformedGeometry(nativePtr);
		}
		return null;
	}

	internal unsafe void GetSourceGeometry(out Geometry sourceGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			sourceGeometry = new Geometry(zero);
		}
		else
		{
			sourceGeometry = null;
		}
	}

	internal unsafe void GetTransform(out RawMatrix3x2 transform)
	{
		transform = default(RawMatrix3x2);
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
