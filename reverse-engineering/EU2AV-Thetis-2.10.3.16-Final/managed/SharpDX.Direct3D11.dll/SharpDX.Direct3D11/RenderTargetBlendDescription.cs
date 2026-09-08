using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RenderTargetBlendDescription(bool isBlendEnabled, BlendOption sourceBlend, BlendOption destinationBlend, BlendOperation blendOperation, BlendOption sourceAlphaBlend, BlendOption destinationAlphaBlend, BlendOperation alphaBlendOperation, ColorWriteMaskFlags renderTargetWriteMask)
{
	public RawBool IsBlendEnabled = isBlendEnabled;

	public BlendOption SourceBlend = sourceBlend;

	public BlendOption DestinationBlend = destinationBlend;

	public BlendOperation BlendOperation = blendOperation;

	public BlendOption SourceAlphaBlend = sourceAlphaBlend;

	public BlendOption DestinationAlphaBlend = destinationAlphaBlend;

	public BlendOperation AlphaBlendOperation = alphaBlendOperation;

	public ColorWriteMaskFlags RenderTargetWriteMask = renderTargetWriteMask;

	public override string ToString()
	{
		return $"IsBlendEnabled: {IsBlendEnabled}, SourceBlend: {SourceBlend}, DestinationBlend: {DestinationBlend}, BlendOperation: {BlendOperation}, SourceAlphaBlend: {SourceAlphaBlend}, DestinationAlphaBlend: {DestinationAlphaBlend}, AlphaBlendOperation: {AlphaBlendOperation}, RenderTargetWriteMask: {RenderTargetWriteMask}";
	}
}
