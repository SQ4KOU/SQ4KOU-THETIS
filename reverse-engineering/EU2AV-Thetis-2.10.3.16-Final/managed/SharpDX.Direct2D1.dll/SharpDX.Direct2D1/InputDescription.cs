using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct InputDescription(Filter filter, int levelOfDetail)
{
	public Filter Filter = filter;

	public int LevelOfDetailCount = levelOfDetail;
}
