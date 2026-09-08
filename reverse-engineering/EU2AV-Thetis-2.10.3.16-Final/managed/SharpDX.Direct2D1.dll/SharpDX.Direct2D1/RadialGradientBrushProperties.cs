using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RadialGradientBrushProperties
{
	public RawVector2 Center;

	public RawVector2 GradientOriginOffset;

	public float RadiusX;

	public float RadiusY;
}
