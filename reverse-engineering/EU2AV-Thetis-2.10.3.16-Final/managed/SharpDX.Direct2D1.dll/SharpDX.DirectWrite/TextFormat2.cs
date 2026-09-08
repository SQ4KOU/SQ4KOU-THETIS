using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("F67E0EDD-9E3D-4ECC-8C32-4183253DFE70")]
public class TextFormat2 : TextFormat1
{
	public LineSpacing LineSpacing
	{
		get
		{
			GetLineSpacing(out var lineSpacingOptions);
			return lineSpacingOptions;
		}
		set
		{
			SetLineSpacing(ref value);
		}
	}

	public TextFormat2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextFormat2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextFormat2(nativePtr);
		}
		return null;
	}

	internal unsafe void SetLineSpacing(ref LineSpacing lineSpacingOptions)
	{
		Result result;
		fixed (LineSpacing* ptr = &lineSpacingOptions)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetLineSpacing(out LineSpacing lineSpacingOptions)
	{
		lineSpacingOptions = default(LineSpacing);
		Result result;
		fixed (LineSpacing* ptr = &lineSpacingOptions)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
