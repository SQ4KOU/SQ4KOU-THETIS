using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct GradientMeshPatch
{
	public RawVector2 Point00;

	public RawVector2 Point01;

	public RawVector2 Point02;

	public RawVector2 Point03;

	public RawVector2 Point10;

	public RawVector2 Point11;

	public RawVector2 Point12;

	public RawVector2 Point13;

	public RawVector2 Point20;

	public RawVector2 Point21;

	public RawVector2 Point22;

	public RawVector2 Point23;

	public RawVector2 Point30;

	public RawVector2 Point31;

	public RawVector2 Point32;

	public RawVector2 Point33;

	public RawColor4 Color00;

	public RawColor4 Color03;

	public RawColor4 Color30;

	public RawColor4 Color33;

	public PatchEdgeMode TopEdgeMode;

	public PatchEdgeMode LeftEdgeMode;

	public PatchEdgeMode BottomEdgeMode;

	public PatchEdgeMode RightEdgeMode;
}
