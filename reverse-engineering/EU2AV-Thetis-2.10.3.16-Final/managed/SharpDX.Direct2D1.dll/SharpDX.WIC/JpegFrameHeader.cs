using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct JpegFrameHeader
{
	public int Width;

	public int Height;

	public JpegTransferMatrix TransferMatrix;

	public JpegScanType ScanType;

	public int CComponents;

	public int ComponentIdentifiers;

	public int SampleFactors;

	public int QuantizationTableIndices;
}
