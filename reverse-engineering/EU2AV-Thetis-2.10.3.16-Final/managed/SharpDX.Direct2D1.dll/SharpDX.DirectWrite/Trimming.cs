using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct Trimming
{
	public TrimmingGranularity Granularity;

	public int Delimiter;

	public int DelimiterCount;
}
