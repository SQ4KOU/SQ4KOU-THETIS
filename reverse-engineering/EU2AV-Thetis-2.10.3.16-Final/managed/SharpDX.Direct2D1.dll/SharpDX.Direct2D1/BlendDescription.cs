using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct BlendDescription
{
	public Blend SourceBlend;

	public Blend DestinationBlend;

	public BlendOperation BlendOperation;

	public Blend SourceBlendAlpha;

	public Blend DestinationBlendAlpha;

	public BlendOperation BlendOperationAlpha;

	public RawColor4 BlendFactor;
}
