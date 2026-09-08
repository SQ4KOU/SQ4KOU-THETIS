using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Size = 4)]
[DebuggerDisplay("R:{R} G:{G} B:{B} A:{A}")]
public struct RawColorBGRA(byte b, byte g, byte r, byte a)
{
	public byte B = b;

	public byte G = g;

	public byte R = r;

	public byte A = a;
}
