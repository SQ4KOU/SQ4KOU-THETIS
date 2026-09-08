using System;
using System.Runtime.InteropServices;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("1c4820bb-5771-4518-a581-2fe4dd0ec657")]
public class ColorContext : Resource
{
	public byte[] ProfileData
	{
		get
		{
			byte[] array = new byte[ProfileSize];
			GetProfile(array, array.Length);
			return array;
		}
	}

	public ColorSpace ColorSpace => GetColorSpace();

	internal int ProfileSize => GetProfileSize();

	public ColorContext(EffectContext context, ColorSpace space, byte[] profileRef)
		: base(IntPtr.Zero)
	{
		context.CreateColorContext(space, profileRef, profileRef.Length, this);
	}

	public ColorContext(EffectContext context, string filename)
		: base(IntPtr.Zero)
	{
		context.CreateColorContextFromFilename(filename, this);
	}

	public ColorContext(EffectContext context, SharpDX.WIC.ColorContext wicColorContext)
		: base(IntPtr.Zero)
	{
		context.CreateColorContextFromWicColorContext(wicColorContext, this);
	}

	public ColorContext(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ColorContext(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ColorContext(nativePtr);
		}
		return null;
	}

	internal unsafe ColorSpace GetColorSpace()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ColorSpace>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetProfileSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetProfile(byte[] rofileRef, int profileSize)
	{
		Result result;
		fixed (byte* ptr = rofileRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr2, profileSize);
		}
		result.CheckError();
	}
}
