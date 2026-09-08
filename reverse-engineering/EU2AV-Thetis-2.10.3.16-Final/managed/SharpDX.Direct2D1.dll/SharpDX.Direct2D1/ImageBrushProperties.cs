using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ImageBrushProperties
{
	public RawRectangleF SourceRectangle;

	public ExtendMode ExtendModeX;

	public ExtendMode ExtendModeY;

	public InterpolationMode InterpolationMode;
}
