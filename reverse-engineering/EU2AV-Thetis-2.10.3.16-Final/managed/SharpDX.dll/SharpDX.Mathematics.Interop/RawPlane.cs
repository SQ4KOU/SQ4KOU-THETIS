using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("Normal: {Normal}, D: {D}")]
public struct RawPlane(RawVector3 normal, float d)
{
	public RawVector3 Normal = normal;

	public float D = d;
}
