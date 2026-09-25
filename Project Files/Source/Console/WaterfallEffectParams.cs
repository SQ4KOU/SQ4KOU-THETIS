using System.Runtime.InteropServices;

namespace Thetis;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct WaterfallEffectParams
{
    public float Saturation;
    public float Gamma;
    public int ToneMapMode;
    public float DitherAmount;
}
