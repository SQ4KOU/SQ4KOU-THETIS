using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TextRange(int startPosition, int length)
{
	public int StartPosition = startPosition;

	public int Length = length;
}
