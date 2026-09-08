using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SvgViewBox
{
	public float X;

	public float Y;

	public float Width;

	public float Height;
}
