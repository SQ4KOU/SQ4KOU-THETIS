using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DepthStencilStateDescription
{
	public RawBool IsDepthEnabled;

	public DepthWriteMask DepthWriteMask;

	public Comparison DepthComparison;

	public RawBool IsStencilEnabled;

	public byte StencilReadMask;

	public byte StencilWriteMask;

	public DepthStencilOperationDescription FrontFace;

	public DepthStencilOperationDescription BackFace;

	public static DepthStencilStateDescription Default()
	{
		return new DepthStencilStateDescription
		{
			IsDepthEnabled = true,
			DepthWriteMask = DepthWriteMask.All,
			DepthComparison = Comparison.Less,
			IsStencilEnabled = false,
			StencilReadMask = byte.MaxValue,
			StencilWriteMask = byte.MaxValue,
			FrontFace = 
			{
				Comparison = Comparison.Always,
				DepthFailOperation = StencilOperation.Keep,
				FailOperation = StencilOperation.Keep,
				PassOperation = StencilOperation.Keep
			},
			BackFace = 
			{
				Comparison = Comparison.Always,
				DepthFailOperation = StencilOperation.Keep,
				FailOperation = StencilOperation.Keep,
				PassOperation = StencilOperation.Keep
			}
		};
	}
}
