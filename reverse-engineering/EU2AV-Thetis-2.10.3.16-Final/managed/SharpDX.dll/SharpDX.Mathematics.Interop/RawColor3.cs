using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("R: {R}, G: {G}, B: {B}")]
public struct RawColor3(float r, float g, float b)
{
	public float R = r;

	public float G = g;

	public float B = b;
}
