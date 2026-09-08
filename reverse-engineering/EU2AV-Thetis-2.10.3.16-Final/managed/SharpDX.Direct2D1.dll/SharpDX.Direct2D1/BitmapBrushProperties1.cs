using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct BitmapBrushProperties1
{
	public ExtendMode ExtendModeX;

	public ExtendMode ExtendModeY;

	public InterpolationMode InterpolationMode;
}
