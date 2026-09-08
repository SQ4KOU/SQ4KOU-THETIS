using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PixelFormat(Format format, AlphaMode alphaMode)
{
	public Format Format = format;

	public AlphaMode AlphaMode = alphaMode;
}
