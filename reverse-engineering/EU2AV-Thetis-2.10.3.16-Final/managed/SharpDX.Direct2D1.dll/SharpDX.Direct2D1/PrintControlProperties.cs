using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PrintControlProperties
{
	public PrintFontSubsetMode FontSubset;

	public float RasterDPI;

	public ColorSpace ColorSpace;
}
