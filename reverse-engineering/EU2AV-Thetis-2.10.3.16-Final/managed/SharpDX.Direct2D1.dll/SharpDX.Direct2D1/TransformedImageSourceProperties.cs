using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TransformedImageSourceProperties
{
	public Orientation Orientation;

	public float ScaleX;

	public float ScaleY;

	public InterpolationMode InterpolationMode;

	public TransformedImageSourceOptions Options;
}
