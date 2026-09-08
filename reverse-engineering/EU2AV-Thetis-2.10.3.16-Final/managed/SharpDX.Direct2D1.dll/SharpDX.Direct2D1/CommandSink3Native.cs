using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("18079135-4cf3-4868-bc8e-06067e6d242d")]
internal class CommandSink3Native : CommandSink2Native, CommandSink3, CommandSink2, CommandSink1, CommandSink, IUnknown, ICallbackable, IDisposable
{
	public void DrawSpriteBatch(SpriteBatch spriteBatch, int startIndex, int spriteCount, Bitmap bitmap, BitmapInterpolationMode interpolationMode, SpriteOptions spriteOptions)
	{
		DrawSpriteBatch_(spriteBatch, startIndex, spriteCount, bitmap, interpolationMode, spriteOptions);
	}

	public CommandSink3Native(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CommandSink3Native(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CommandSink3Native(nativePtr);
		}
		return null;
	}

	internal unsafe void DrawSpriteBatch_(SpriteBatch spriteBatch, int startIndex, int spriteCount, Bitmap bitmap, BitmapInterpolationMode interpolationMode, SpriteOptions spriteOptions)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SpriteBatch>(spriteBatch);
		zero2 = CppObject.ToCallbackPtr<Bitmap>(bitmap);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, startIndex, spriteCount, (void*)zero2, (int)interpolationMode, (int)spriteOptions)).CheckError();
	}
}
