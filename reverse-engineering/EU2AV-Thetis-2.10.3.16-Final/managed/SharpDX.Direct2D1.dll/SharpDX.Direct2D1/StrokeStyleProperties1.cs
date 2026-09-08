using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct StrokeStyleProperties1
{
	public CapStyle StartCap;

	public CapStyle EndCap;

	public CapStyle DashCap;

	public LineJoin LineJoin;

	public float MiterLimit;

	public DashStyle DashStyle;

	public float DashOffset;

	public StrokeTransformType TransformType;
}
