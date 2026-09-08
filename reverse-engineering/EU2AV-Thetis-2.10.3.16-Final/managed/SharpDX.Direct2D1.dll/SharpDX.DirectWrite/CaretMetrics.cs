using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct CaretMetrics
{
	public short SlopeRise;

	public short SlopeRun;

	public short Offset;
}
