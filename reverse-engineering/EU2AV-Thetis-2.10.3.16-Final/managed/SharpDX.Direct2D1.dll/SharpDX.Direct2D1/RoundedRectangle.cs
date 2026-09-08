using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RoundedRectangle
{
	public RawRectangleF Rect;

	public float RadiusX;

	public float RadiusY;
}
