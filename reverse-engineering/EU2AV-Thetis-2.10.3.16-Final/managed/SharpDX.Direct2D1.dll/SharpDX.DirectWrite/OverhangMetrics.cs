using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct OverhangMetrics
{
	public float Left;

	public float Top;

	public float Right;

	public float Bottom;
}
