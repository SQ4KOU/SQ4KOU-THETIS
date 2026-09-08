using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FileFragment
{
	public long FileOffset;

	public long FragmentSize;
}
