using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("07DDCD52-020E-4DE8-AC33-6C953D83F92D")]
public class TextLayout3 : TextLayout2
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

	public TextLayout3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextLayout3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextLayout3(nativePtr);
		}
		return null;
	}

	public unsafe void InvalidateLayout()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)80 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void SetLineSpacing(ref LineSpacing lineSpacingOptions)
	{
		Result result;
		fixed (LineSpacing* ptr = &lineSpacingOptions)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)81 * (nint)sizeof(void*))))(_nativePointer, ptr2);
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
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)82 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetLineMetrics(LineMetrics1[] lineMetrics, int maxLineCount, out int actualLineCount)
	{
		Result result;
		fixed (int* ptr = &actualLineCount)
		{
			void* ptr2 = ptr;
			fixed (LineMetrics1* ptr3 = lineMetrics)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)83 * (nint)sizeof(void*))))(_nativePointer, ptr4, maxLineCount, ptr2);
			}
		}
		result.CheckError();
	}
}
