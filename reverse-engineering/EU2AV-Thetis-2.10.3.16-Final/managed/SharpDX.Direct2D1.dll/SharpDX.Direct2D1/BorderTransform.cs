using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("4998735c-3a19-473c-9781-656847e3a347")]
public class BorderTransform : ConcreteTransform
{
	public ExtendMode ExtendModeX
	{
		get
		{
			return GetExtendModeX();
		}
		set
		{
			SetExtendModeX(value);
		}
	}

	public ExtendMode ExtendModeY
	{
		get
		{
			return GetExtendModeY();
		}
		set
		{
			SetExtendModeY(value);
		}
	}

	public BorderTransform(EffectContext context, ExtendMode extendModeX, ExtendMode extendModeY)
		: base(IntPtr.Zero)
	{
		context.CreateBorderTransform(extendModeX, extendModeY, this);
	}

	public BorderTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BorderTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BorderTransform(nativePtr);
		}
		return null;
	}

	internal unsafe void SetExtendModeX(ExtendMode extendMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (int)extendMode);
	}

	internal unsafe void SetExtendModeY(ExtendMode extendMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (int)extendMode);
	}

	internal unsafe ExtendMode GetExtendModeX()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ExtendMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ExtendMode GetExtendModeY()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ExtendMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}
}
