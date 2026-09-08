using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("9064D822-80A7-465C-A986-DF65F78B8FEB")]
public class TextLayout1 : TextLayout
{
	public TextLayout1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextLayout1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextLayout1(nativePtr);
		}
		return null;
	}

	public unsafe void SetPairKerning(RawBool isPairKerningEnabled, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, RawBool, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)67 * (nint)sizeof(void*))))(_nativePointer, isPairKerningEnabled, textRange)).CheckError();
	}

	public unsafe void GetPairKerning(int currentPosition, out RawBool isPairKerningEnabled, out TextRange textRange)
	{
		isPairKerningEnabled = default(RawBool);
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &isPairKerningEnabled)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)68 * (nint)sizeof(void*))))(_nativePointer, currentPosition, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void SetCharacterSpacing(float leadingSpacing, float trailingSpacing, float minimumAdvanceWidth, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, float, float, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)69 * (nint)sizeof(void*))))(_nativePointer, leadingSpacing, trailingSpacing, minimumAdvanceWidth, textRange)).CheckError();
	}

	public unsafe void GetCharacterSpacing(int currentPosition, out float leadingSpacing, out float trailingSpacing, out float minimumAdvanceWidth, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = &minimumAdvanceWidth)
			{
				void* ptr4 = ptr3;
				fixed (float* ptr5 = &trailingSpacing)
				{
					void* ptr6 = ptr5;
					fixed (float* ptr7 = &leadingSpacing)
					{
						void* ptr8 = ptr7;
						result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)70 * (nint)sizeof(void*))))(_nativePointer, currentPosition, ptr8, ptr6, ptr4, ptr2);
					}
				}
			}
		}
		result.CheckError();
	}
}
