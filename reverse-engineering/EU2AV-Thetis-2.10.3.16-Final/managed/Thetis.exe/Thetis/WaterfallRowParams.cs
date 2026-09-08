using System.Runtime.InteropServices;

namespace Thetis;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct WaterfallRowParams
{
	public int Width;

	public int Decimation;

	public int PaletteSize;

	public int RowYForDither;

	public int ToneMapMode;

	public int DitherEnabled;

	public int DitherLevels;

	public int Pad0;

	public int IsPaletteScheme;

	public int ApplyGammaToPercent;

	public int IsLinearOutput;

	public int Pad2;

	public float LowThreshold;

	public float HighThreshold;

	public float Dt;

	public float Tau;

	public float Gamma;

	public float InvGamma;

	public float MotionThreshold;

	public float QualityContrast;

	public float SaturationBoost;

	public float ContrastBoost;

	public float PaletteSharpness;

	public float PaletteContrast;
}
