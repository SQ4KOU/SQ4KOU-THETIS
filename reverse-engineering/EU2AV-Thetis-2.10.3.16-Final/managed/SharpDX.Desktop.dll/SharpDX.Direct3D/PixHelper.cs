using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D;

public static class PixHelper
{
	public static bool IsCurrentlyProfiled => D3DPERF_GetStatus() != 0;

	public static int BeginEvent(RawColorBGRA color, string name)
	{
		return D3DPERF_BeginEvent(color, name);
	}

	public static int BeginEvent(RawColorBGRA color, string name, params object[] parameters)
	{
		return D3DPERF_BeginEvent(color, string.Format(name, parameters));
	}

	public static int EndEvent()
	{
		return D3DPERF_EndEvent();
	}

	public static void SetMarker(RawColorBGRA color, string name)
	{
		D3DPERF_SetMarker(color, name);
	}

	public static void SetMarker(RawColorBGRA color, string name, params object[] parameters)
	{
		D3DPERF_SetMarker(color, string.Format(name, parameters));
	}

	public static void AllowProfiling(bool enableFlag)
	{
		D3DPERF_SetOptions((!enableFlag) ? 1 : 0);
	}

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern int D3DPERF_BeginEvent(RawColorBGRA color, string name);

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern int D3DPERF_EndEvent();

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern void D3DPERF_SetMarker(RawColorBGRA color, string wszName);

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern void D3DPERF_SetOptions(int options);

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern int D3DPERF_GetStatus();
}
