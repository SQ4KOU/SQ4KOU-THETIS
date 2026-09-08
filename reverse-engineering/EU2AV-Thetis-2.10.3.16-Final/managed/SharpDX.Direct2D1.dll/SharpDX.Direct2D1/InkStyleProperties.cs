using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct InkStyleProperties
{
	public InkNibShape NibShape;

	public RawMatrix3x2 NibTransform;
}
