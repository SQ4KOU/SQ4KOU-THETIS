using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct GlyphOffset
{
	public float AdvanceOffset;

	public float AscenderOffset;
}
