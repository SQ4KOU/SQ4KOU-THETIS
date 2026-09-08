using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SampleDescription(int count, int quality)
{
	public int Count = count;

	public int Quality = quality;

	public override string ToString()
	{
		return $"{{{Count}, {Quality}}}";
	}
}
