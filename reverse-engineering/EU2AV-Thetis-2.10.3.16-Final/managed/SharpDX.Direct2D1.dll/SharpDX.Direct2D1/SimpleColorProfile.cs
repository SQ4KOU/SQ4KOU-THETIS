using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SimpleColorProfile
{
	public RawVector2 RedPrimary;

	public RawVector2 GreenPrimary;

	public RawVector2 BluePrimary;

	public RawVector2 WhitePointXZ;

	public Gamma1 Gamma;
}
