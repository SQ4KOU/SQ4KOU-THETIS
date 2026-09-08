using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("18079135-4cf3-4868-bc8e-06067e6d242d")]
public interface CommandSink3 : CommandSink2, CommandSink1, CommandSink, IUnknown, ICallbackable, IDisposable
{
	void DrawSpriteBatch(SpriteBatch spriteBatch, int startIndex, int spriteCount, Bitmap bitmap, BitmapInterpolationMode interpolationMode, SpriteOptions spriteOptions);
}
