using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}, W: {W}")]
public struct RawVector4(float x, float y, float z, float w)
{
	public float X = x;

	public float Y = y;

	public float Z = z;

	public float W = w;
}
