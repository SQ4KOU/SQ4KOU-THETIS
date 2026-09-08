using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct HwndRenderTargetProperties
{
	public IntPtr Hwnd;

	public Size2 PixelSize;

	public PresentOptions PresentOptions;
}
