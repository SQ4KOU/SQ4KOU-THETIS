using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DdsFormatInfo
{
	public Format DxgiFormat;

	public int BytesPerBlock;

	public int BlockWidth;

	public int BlockHeight;
}
