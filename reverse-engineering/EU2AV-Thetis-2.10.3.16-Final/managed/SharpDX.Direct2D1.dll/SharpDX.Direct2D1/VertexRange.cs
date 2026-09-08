using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VertexRange(int startVertex, int vertexCount)
{
	public int StartVertex = startVertex;

	public int VertexCount = vertexCount;
}
