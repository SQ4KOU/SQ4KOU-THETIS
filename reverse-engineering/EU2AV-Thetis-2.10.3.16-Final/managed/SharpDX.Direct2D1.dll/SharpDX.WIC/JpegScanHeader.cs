using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct JpegScanHeader
{
	public int CComponents;

	public int RestartInterval;

	public int ComponentSelectors;

	public int HuffmanTableIndices;

	public byte StartSpectralSelection;

	public byte EndSpectralSelection;

	public byte SuccessiveApproximationHigh;

	public byte SuccessiveApproximationLow;
}
