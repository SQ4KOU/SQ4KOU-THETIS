using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RenderingControls
{
	public BufferPrecision BufferPrecision;

	public Size2 TileSize;
}
