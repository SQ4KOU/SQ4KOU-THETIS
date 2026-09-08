using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RasterizerStateDescription1
{
	public FillMode FillMode;

	public CullMode CullMode;

	public RawBool IsFrontCounterClockwise;

	public int DepthBias;

	public float DepthBiasClamp;

	public float SlopeScaledDepthBias;

	public RawBool IsDepthClipEnabled;

	public RawBool IsScissorEnabled;

	public RawBool IsMultisampleEnabled;

	public RawBool IsAntialiasedLineEnabled;

	public int ForcedSampleCount;

	public static RasterizerStateDescription1 Default()
	{
		return new RasterizerStateDescription1
		{
			FillMode = FillMode.Solid,
			CullMode = CullMode.Back,
			IsFrontCounterClockwise = false,
			DepthBias = 0,
			SlopeScaledDepthBias = 0f,
			DepthBiasClamp = 0f,
			IsDepthClipEnabled = true,
			IsScissorEnabled = false,
			IsMultisampleEnabled = false,
			IsAntialiasedLineEnabled = false,
			ForcedSampleCount = 0
		};
	}
}
