using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct InkPoint
{
	public float X;

	public float Y;

	public float Radius;
}
