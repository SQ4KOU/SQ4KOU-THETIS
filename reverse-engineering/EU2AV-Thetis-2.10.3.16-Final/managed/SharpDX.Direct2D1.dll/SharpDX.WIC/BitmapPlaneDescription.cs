using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct BitmapPlaneDescription
{
	public Guid Format;

	public int Width;

	public int Height;
}
