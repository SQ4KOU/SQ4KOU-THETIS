using System.Runtime.InteropServices;

namespace Thetis;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct FFTParams
{
    public uint N;
    public uint Bits;
    public uint Stage;
    public uint DisplayWidth;
    public int FirstBin;
    public uint BinCount;
    public float SampleRate;
    public float WindowPower;
    public float CoherentGain;
    public int MagnitudeMode;
    public float DisplayLowFreq;
    public float DisplayHighFreq;
    public float BinWidth;
    public uint Padding;
    public int LanczosWindow;
    public int ResamplingMode;
}
