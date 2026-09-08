using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}")]
public struct RawVector3(float x, float y, float z)
{
	public float X = x;

	public float Y = y;

	public float Z = z;
}
