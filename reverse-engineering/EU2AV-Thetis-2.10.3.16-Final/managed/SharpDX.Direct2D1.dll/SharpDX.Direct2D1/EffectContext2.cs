using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("577ad2a0-9fc7-4dda-8b18-dab810140052")]
public class EffectContext2 : EffectContext1
{
	public EffectContext2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator EffectContext2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new EffectContext2(nativePtr);
		}
		return null;
	}

	public unsafe void CreateColorContextFromDxgiColorSpace(ColorSpaceType colorSpace, ColorContext1 colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, (int)colorSpace, &zero);
		colorContext.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void CreateColorContextFromSimpleColorProfile(ref SimpleColorProfile simpleProfile, ColorContext1 colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (SimpleColorProfile* ptr = &simpleProfile)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		colorContext.NativePointer = zero;
		result.CheckError();
	}
}
