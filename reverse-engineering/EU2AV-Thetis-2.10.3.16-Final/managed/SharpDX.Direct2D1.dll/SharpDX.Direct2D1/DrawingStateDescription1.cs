using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DrawingStateDescription1
{
	public AntialiasMode AntialiasMode;

	public TextAntialiasMode TextAntialiasMode;

	public long Tag1;

	public long Tag2;

	public RawMatrix3x2 Transform;

	public PrimitiveBlend PrimitiveBlend;

	public UnitMode UnitMode;
}
