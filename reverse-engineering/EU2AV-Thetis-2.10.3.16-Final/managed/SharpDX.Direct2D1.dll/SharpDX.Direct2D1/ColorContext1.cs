using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("1ab42875-c57f-4be9-bd85-9cd78d6f55ee")]
public class ColorContext1 : ColorContext
{
	public ColorContextType ColorContextType => GetColorContextType();

	public ColorSpaceType DXGIColorSpace => GetDXGIColorSpace();

	public SimpleColorProfile SimpleColorProfile
	{
		get
		{
			GetSimpleColorProfile(out var simpleProfile);
			return simpleProfile;
		}
	}

	public ColorContext1(DeviceContext5 context, ColorSpaceType colorSpace)
		: base(IntPtr.Zero)
	{
		context.CreateColorContextFromDxgiColorSpace(colorSpace, this);
	}

	public ColorContext1(DeviceContext5 context, ref SimpleColorProfile simpleProfile)
		: base(IntPtr.Zero)
	{
		context.CreateColorContextFromSimpleColorProfile(ref simpleProfile, this);
	}

	public ColorContext1(EffectContext2 context, ColorSpaceType colorSpace)
		: base(IntPtr.Zero)
	{
		context.CreateColorContextFromDxgiColorSpace(colorSpace, this);
	}

	public ColorContext1(EffectContext2 context, ref SimpleColorProfile simpleProfile)
		: base(IntPtr.Zero)
	{
		context.CreateColorContextFromSimpleColorProfile(ref simpleProfile, this);
	}

	public ColorContext1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ColorContext1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ColorContext1(nativePtr);
		}
		return null;
	}

	internal unsafe ColorContextType GetColorContextType()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ColorContextType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ColorSpaceType GetDXGIColorSpace()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ColorSpaceType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetSimpleColorProfile(out SimpleColorProfile simpleProfile)
	{
		simpleProfile = default(SimpleColorProfile);
		Result result;
		fixed (SimpleColorProfile* ptr = &simpleProfile)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
