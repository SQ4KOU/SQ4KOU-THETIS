using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2cd906a8-12e2-11dc-9fed-001143a055f9")]
public class Brush : Resource
{
	public float Opacity
	{
		get
		{
			return GetOpacity();
		}
		set
		{
			SetOpacity(value);
		}
	}

	public RawMatrix3x2 Transform
	{
		get
		{
			GetTransform(out var transform);
			return transform;
		}
		set
		{
			SetTransform(ref value);
		}
	}

	public Brush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Brush(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Brush(nativePtr);
		}
		return null;
	}

	internal unsafe void SetOpacity(float opacity)
	{
		((delegate* unmanaged[Stdcall]<void*, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, opacity);
	}

	internal unsafe void SetTransform(ref RawMatrix3x2 transform)
	{
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe float GetOpacity()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetTransform(out RawMatrix3x2 transform)
	{
		transform = default(RawMatrix3x2);
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
