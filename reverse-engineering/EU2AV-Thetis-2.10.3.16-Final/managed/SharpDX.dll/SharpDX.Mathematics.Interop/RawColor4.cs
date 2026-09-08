using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("R:{R} G:{G} B:{B} A:{A}")]
public struct RawColor4(float r, float g, float b, float a)
{
	public float R = r;

	public float G = g;

	public float B = b;

	public float A = a;
}
