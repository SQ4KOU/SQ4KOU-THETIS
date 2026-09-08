using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DdsParameters
{
	public int Width;

	public int Height;

	public int Depth;

	public int MipLevels;

	public int ArraySize;

	public Format DxgiFormat;

	public DdsDimension Dimension;

	public DdsAlphaMode AlphaMode;
}
