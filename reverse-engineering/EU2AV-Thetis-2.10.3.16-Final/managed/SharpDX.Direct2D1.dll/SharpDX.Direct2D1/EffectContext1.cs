using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("84ab595a-fc81-4546-bacd-e8ef4d8abe7a")]
public class EffectContext1 : EffectContext
{
	public EffectContext1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator EffectContext1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new EffectContext1(nativePtr);
		}
		return null;
	}

	public unsafe void CreateLookupTable3D(BufferPrecision precision, int[] extents, byte[] data, int dataCount, int[] strides, out LookupTable3D lookupTable)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (int* ptr = strides)
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = data)
			{
				void* ptr4 = ptr3;
				fixed (int* ptr5 = extents)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (int)precision, ptr6, ptr4, dataCount, ptr2, &zero);
				}
			}
		}
		if (zero != IntPtr.Zero)
		{
			lookupTable = new LookupTable3D(zero);
		}
		else
		{
			lookupTable = null;
		}
		result.CheckError();
	}
}
