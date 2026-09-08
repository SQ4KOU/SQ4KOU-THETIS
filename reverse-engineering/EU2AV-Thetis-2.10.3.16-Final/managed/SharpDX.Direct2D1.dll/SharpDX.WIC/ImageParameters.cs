using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ImageParameters(SharpDX.Direct2D1.PixelFormat pixelFormat, float dpiX, float dpiY, float top, float left, int pixelWidth, int pixelHeight)
{
	public SharpDX.Direct2D1.PixelFormat PixelFormat = pixelFormat;

	public float DpiX = dpiX;

	public float DpiY = dpiY;

	public float Top = top;

	public float Left = left;

	public int PixelWidth = pixelWidth;

	public int PixelHeight = pixelHeight;
}
