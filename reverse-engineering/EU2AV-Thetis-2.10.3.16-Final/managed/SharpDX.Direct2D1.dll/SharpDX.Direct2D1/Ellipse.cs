using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct Ellipse(RawVector2 center, float radiusX, float radiusY)
{
	public RawVector2 Point = center;

	public float RadiusX = radiusX;

	public float RadiusY = radiusY;
}
