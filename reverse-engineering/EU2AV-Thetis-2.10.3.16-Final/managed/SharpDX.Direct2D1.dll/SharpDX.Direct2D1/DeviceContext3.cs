using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("235a7496-8351-414c-bcd4-6672ab2d8e00")]
public class DeviceContext3 : DeviceContext2
{
	public DeviceContext3(Device3 device, DeviceContextOptions options)
		: base(IntPtr.Zero)
	{
		device.CreateDeviceContext(options, this);
	}

	public DeviceContext3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext3(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateSpriteBatch(SpriteBatch spriteBatch)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)106 * (nint)sizeof(void*))))(_nativePointer, &zero);
		spriteBatch.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void DrawSpriteBatch(SpriteBatch spriteBatch, int startIndex, int spriteCount, Bitmap bitmap, BitmapInterpolationMode interpolationMode, SpriteOptions spriteOptions)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SpriteBatch>(spriteBatch);
		zero2 = CppObject.ToCallbackPtr<Bitmap>(bitmap);
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)107 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, startIndex, spriteCount, (void*)zero2, (int)interpolationMode, (int)spriteOptions);
	}
}
