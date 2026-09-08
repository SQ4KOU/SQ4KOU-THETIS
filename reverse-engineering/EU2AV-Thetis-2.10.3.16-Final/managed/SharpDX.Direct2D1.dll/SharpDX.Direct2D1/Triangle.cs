using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct Triangle
{
	public RawVector2 Point1;

	public RawVector2 Point2;

	public RawVector2 Point3;
}
