using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct BitmapBrushProperties
{
	public ExtendMode ExtendModeX;

	public ExtendMode ExtendModeY;

	public BitmapInterpolationMode InterpolationMode;
}
