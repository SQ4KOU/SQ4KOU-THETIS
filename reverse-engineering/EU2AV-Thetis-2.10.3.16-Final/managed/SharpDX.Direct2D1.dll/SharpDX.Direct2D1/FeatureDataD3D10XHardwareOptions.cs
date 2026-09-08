using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FeatureDataD3D10XHardwareOptions
{
	public RawBool ComputeShadersPlusRawAndStructuredBuffersViaShader4X;
}
