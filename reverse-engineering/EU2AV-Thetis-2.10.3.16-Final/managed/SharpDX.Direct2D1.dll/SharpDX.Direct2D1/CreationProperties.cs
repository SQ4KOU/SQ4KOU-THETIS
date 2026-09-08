using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct CreationProperties
{
	public ThreadingMode ThreadingMode;

	public DebugLevel DebugLevel;

	public DeviceContextOptions Options;
}
