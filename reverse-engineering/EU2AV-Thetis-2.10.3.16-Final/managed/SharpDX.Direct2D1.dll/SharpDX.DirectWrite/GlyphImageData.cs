using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct GlyphImageData
{
	public IntPtr ImageData;

	public int ImageDataSize;

	public int UniqueDataId;

	public int PixelsPerEm;

	public Size2 PixelSize;

	public RawPoint HorizontalLeftOrigin;

	public RawPoint HorizontalRightOrigin;

	public RawPoint VerticalTopOrigin;

	public RawPoint VerticalBottomOrigin;
}
