using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("bae8b344-23fc-4071-8cb5-d05d6f073848")]
public class InkStyle : Resource
{
	public RawMatrix3x2 NibTransform
	{
		get
		{
			GetNibTransform(out var transform);
			return transform;
		}
		set
		{
			SetNibTransform(ref value);
		}
	}

	public InkNibShape NibShape
	{
		get
		{
			return GetNibShape();
		}
		set
		{
			SetNibShape(value);
		}
	}

	public InkStyle(DeviceContext2 context2)
		: this(IntPtr.Zero)
	{
		context2.CreateInkStyle(null, this);
	}

	public InkStyle(DeviceContext2 context2, InkStyleProperties inkStyleProperties)
		: this(IntPtr.Zero)
	{
		context2.CreateInkStyle(inkStyleProperties, this);
	}

	public InkStyle(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator InkStyle(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new InkStyle(nativePtr);
		}
		return null;
	}

	internal unsafe void SetNibTransform(ref RawMatrix3x2 transform)
	{
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void GetNibTransform(out RawMatrix3x2 transform)
	{
		transform = default(RawMatrix3x2);
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void SetNibShape(InkNibShape nibShape)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (int)nibShape);
	}

	internal unsafe InkNibShape GetNibShape()
	{
		return ((delegate* unmanaged[Stdcall]<void*, InkNibShape>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}
}
