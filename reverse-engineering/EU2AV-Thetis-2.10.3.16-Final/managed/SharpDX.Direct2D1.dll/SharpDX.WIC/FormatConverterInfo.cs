using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("9F34FB65-13F4-4f15-BC57-3726B5E53D9F")]
public class FormatConverterInfo : ComponentInfo
{
	public Guid[] PixelFormats
	{
		get
		{
			int actualRef = 0;
			GetPixelFormats(actualRef, null, out actualRef);
			if (actualRef == 0)
			{
				return new Guid[0];
			}
			Guid[] array = new Guid[actualRef];
			GetPixelFormats(actualRef, array, out actualRef);
			return array;
		}
	}

	public FormatConverterInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FormatConverterInfo(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FormatConverterInfo(nativePtr);
		}
		return null;
	}

	internal unsafe void GetPixelFormats(int formats, Guid[] pixelFormatGUIDsRef, out int actualRef)
	{
		Result result;
		fixed (int* ptr = &actualRef)
		{
			void* ptr2 = ptr;
			fixed (Guid* ptr3 = pixelFormatGUIDsRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, formats, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void CreateInstance(FormatConverter converterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &zero);
		converterOut.NativePointer = zero;
		result.CheckError();
	}
}
