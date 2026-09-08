using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ScriptAnalysis
{
	public short Script;

	public ScriptShapes Shapes;
}
