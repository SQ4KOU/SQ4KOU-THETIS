using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SamplerStateDescription
{
	public Filter Filter;

	public TextureAddressMode AddressU;

	public TextureAddressMode AddressV;

	public TextureAddressMode AddressW;

	public float MipLodBias;

	public int MaximumAnisotropy;

	public Comparison ComparisonFunction;

	public RawColor4 BorderColor;

	public float MinimumLod;

	public float MaximumLod;

	public static SamplerStateDescription Default()
	{
		return new SamplerStateDescription
		{
			Filter = Filter.MinMagMipLinear,
			AddressU = TextureAddressMode.Clamp,
			AddressV = TextureAddressMode.Clamp,
			AddressW = TextureAddressMode.Clamp,
			MinimumLod = float.MinValue,
			MaximumLod = float.MaxValue,
			MipLodBias = 0f,
			MaximumAnisotropy = 16,
			ComparisonFunction = Comparison.Never,
			BorderColor = default(RawColor4)
		};
	}
}
