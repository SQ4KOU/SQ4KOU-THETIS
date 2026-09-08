using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("791e8298-3ef3-4230-9880-c9bdecc42064")]
public class BitmapRenderTarget1 : BitmapRenderTarget
{
	public TextAntialiasMode TextAntialiasMode
	{
		get
		{
			return GetTextAntialiasMode();
		}
		set
		{
			SetTextAntialiasMode(value);
		}
	}

	public BitmapRenderTarget1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapRenderTarget1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapRenderTarget1(nativePtr);
		}
		return null;
	}

	internal unsafe TextAntialiasMode GetTextAntialiasMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, TextAntialiasMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetTextAntialiasMode(TextAntialiasMode antialiasMode)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (int)antialiasMode)).CheckError();
	}
}
