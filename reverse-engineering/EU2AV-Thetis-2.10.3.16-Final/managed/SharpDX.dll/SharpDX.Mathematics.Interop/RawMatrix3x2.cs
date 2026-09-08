using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct RawMatrix3x2(float m11, float m12, float m21, float m22, float m31, float m32)
{
	public float M11 = m11;

	public float M12 = m12;

	public float M21 = m21;

	public float M22 = m22;

	public float M31 = m31;

	public float M32 = m32;
}
