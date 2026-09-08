using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct InkBezierSegment
{
	public InkPoint Point1;

	public InkPoint Point2;

	public InkPoint Point3;
}
