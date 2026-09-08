using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SvgPreserveAspectRatio
{
	public RawBool Defer;

	public SvgAspectAlign Align;

	public SvgAspectScaling MeetOrSlice;
}
