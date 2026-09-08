using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("c9cdb0dd-f8c9-4e70-b7c2-301c80292c5e")]
public class SvgAttribute : Resource
{
	public SvgElement Element
	{
		get
		{
			GetElement(out var element);
			return element;
		}
	}

	public SvgAttribute Clone()
	{
		Clone(out var attribute);
		return attribute;
	}

	public SvgAttribute(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SvgAttribute(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SvgAttribute(nativePtr);
		}
		return null;
	}

	internal unsafe void GetElement(out SvgElement element)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			element = new SvgElement(zero);
		}
		else
		{
			element = null;
		}
	}

	internal unsafe void Clone(out SvgAttribute attribute)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			attribute = new SvgAttribute(zero);
		}
		else
		{
			attribute = null;
		}
		result.CheckError();
	}
}
