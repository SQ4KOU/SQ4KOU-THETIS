using System;
using System.Runtime.InteropServices;

namespace Thetis
{
    partial class Display
    {
        // SQ4KOU synthesis:
        // native DirectCompute FFT / IQ analysis from the proven GPU waterfall,
        // feeding the SDR-VST3 Vortice colour-compute + WaterfallMesh presenter.
        // Legacy Thetis data remains the permanent fallback.
        private const int SQ4KOUHighResFftSize = 16384;

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CM_GPUWaterfall_Init(int channel, int fftSize, int ringCapacity);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CM_GPUWaterfall_Configure(int channel, int windowType, float kaiserBeta,
            int magnitudeMode, int autoOverlap, float overlapPercent, int lanczosWindow, int resamplingMode);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CM_GPUWaterfall_Process(int channel, int displayWidth, int sampleRate,
            float displayLowHz, float displayHighHz, [In, Out] float[] outputDb);

        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CM_GPUWaterfall_IsReady(int channel);

        private static readonly object _sq4kouHighResLock = new object();
        private static readonly bool[] _sq4kouHighResReady = new bool[3];
        private static readonly bool[] _sq4kouHighResLoggedActive = new bool[3];
        private static readonly bool[] _sq4kouHighResLoggedFailure = new bool[3];
        private static readonly float[][] _sq4kouHighResRows = new float[3][];
        private static readonly float[][] _sq4kouHighResCalScratch = new float[3][];
        private static readonly float[] _sq4kouHighResCal = new float[3];
        private static readonly bool[] _sq4kouHighResCalValid = new bool[3];
        private static readonly string[] _sq4kouHighResPaneStatus = new string[2] { "LEGACY", "LEGACY" };

        public static bool SQ4KOUHighResWaterfallEnabled { get; set; } = true;
        public static string SQ4KOUHighResWaterfallStatusRX1 { get { return _sq4kouHighResPaneStatus[0]; } }
        public static string SQ4KOUHighResWaterfallStatusRX2 { get { return _sq4kouHighResPaneStatus[1]; } }

        private static float MedianCalibration(int source, float[] gpuRow, int gpuWidth,
            float[] cpuReference, int cpuCount, int pixelStep)
        {
            if (gpuRow == null || cpuReference == null || cpuCount < 16)
                return _sq4kouHighResCal[source];

            if (_sq4kouHighResCalScratch[source] == null || _sq4kouHighResCalScratch[source].Length < cpuCount)
                _sq4kouHighResCalScratch[source] = new float[cpuCount];

            float[] scratch = _sq4kouHighResCalScratch[source];
            int count = 0;
            int step = Math.Max(1, pixelStep);

            for (int i = 0; i < cpuCount; i++)
            {
                int x = Math.Min(gpuWidth - 1, i * step + step / 2);
                float a = cpuReference[i];
                float b = gpuRow[x];
                if (float.IsNaN(a) || float.IsInfinity(a) || float.IsNaN(b) || float.IsInfinity(b))
                    continue;
                scratch[count++] = a - b;
            }

            if (count < 16)
                return _sq4kouHighResCal[source];

            Array.Sort(scratch, 0, count);
            float candidate = scratch[count / 2];
            if (candidate > 200f) candidate = 200f;
            if (candidate < -200f) candidate = -200f;

            if (!_sq4kouHighResCalValid[source])
            {
                _sq4kouHighResCal[source] = candidate;
                _sq4kouHighResCalValid[source] = true;
            }
            else
            {
                float delta = candidate - _sq4kouHighResCal[source];
                if (delta > 2.0f) delta = 2.0f;
                if (delta < -2.0f) delta = -2.0f;
                _sq4kouHighResCal[source] += 0.25f * delta;
            }

            return _sq4kouHighResCal[source];
        }

        private static bool TrySQ4KOUHighResWaterfall(int rx, int width,
            float[] cpuReference, int cpuCount, int pixelStep,
            bool localMox, bool displayDuplex, out float[] highResRow)
        {
            highResRow = null;
            int pane = rx == 2 ? 1 : 0;

            // Full-resolution source is intentionally tied to the hardware GPU compute path.
            // If Vortice compute is unavailable, retain the complete legacy/D2D fallback.
            if (!SQ4KOUHighResWaterfallEnabled || !ComputeArmed || width < 64)
            {
                _sq4kouHighResPaneStatus[pane] = "LEGACY";
                return false;
            }

            int source;
            int sampleRate;
            float lowHz;
            float highHz;

            if (localMox && !displayDuplex)
            {
                source = 2; // post-DSP TX IQ
                sampleRate = cmaster.GetChannelOutputRate(1, 0);
                if (sampleRate <= 0) sampleRate = cmaster.GetInputRate(1, 0);
                lowHz = tx_display_low;
                highHz = tx_display_high;
            }
            else
            {
                source = rx == 2 ? 1 : 0;
                sampleRate = cmaster.GetInputRate(0, source);
                lowHz = rx == 2 ? RX2DisplayLow : RXDisplayLow;
                highHz = rx == 2 ? RX2DisplayHigh : RXDisplayHigh;
            }

            if (sampleRate <= 0 || highHz <= lowHz)
            {
                _sq4kouHighResPaneStatus[pane] = "LEGACY";
                return false;
            }

            lock (_sq4kouHighResLock)
            {
                try
                {
                    if (!_sq4kouHighResReady[source] || CM_GPUWaterfall_IsReady(source) == 0)
                    {
                        if (CM_GPUWaterfall_Init(source, SQ4KOUHighResFftSize, SQ4KOUHighResFftSize * 4) == 0)
                        {
                            _sq4kouHighResReady[source] = false;
                            _sq4kouHighResPaneStatus[pane] = "GPU FFT INIT FAILED -> LEGACY";
                            if (!_sq4kouHighResLoggedFailure[source])
                            {
                                _sq4kouHighResLoggedFailure[source] = true;
                                Common.MeshDiagLog("SQ4KOU high-res waterfall: native GPU FFT init failed; legacy fallback active");
                            }
                            return false;
                        }

                        // Recovered EU2AV/SQ4KOU quality baseline:
                        // FFT 16384, Hamming, dBFS, no forced overlap, linear bin-to-pixel mapping.
                        CM_GPUWaterfall_Configure(source, 1, 8.6f, 0, 0, 0.0f, 3, 0);
                        _sq4kouHighResReady[source] = true;
                        _sq4kouHighResCalValid[source] = false;
                    }

                    if (_sq4kouHighResRows[source] == null || _sq4kouHighResRows[source].Length != width)
                        _sq4kouHighResRows[source] = new float[width];

                    float[] row = _sq4kouHighResRows[source];
                    int rc = CM_GPUWaterfall_Process(source, width, sampleRate, lowHz, highHz, row);
                    if (rc == 0)
                    {
                        _sq4kouHighResPaneStatus[pane] = "GPU FFT WAITING FOR IQ";
                        return false;
                    }
                    if (rc < 0)
                    {
                        _sq4kouHighResReady[source] = false;
                        _sq4kouHighResPaneStatus[pane] = "GPU FFT ERROR -> LEGACY";
                        Common.MeshDiagLog("SQ4KOU high-res waterfall: native GPU FFT process error " + rc + "; legacy fallback active");
                        return false;
                    }

                    float cal = MedianCalibration(source, row, width, cpuReference, cpuCount, pixelStep);
                    for (int i = 0; i < width; i++)
                        row[i] += cal;

                    highResRow = row;
                    _sq4kouHighResPaneStatus[pane] = "GPU FFT 16384 ACTIVE";

                    if (!_sq4kouHighResLoggedActive[source])
                    {
                        _sq4kouHighResLoggedActive[source] = true;
                        Common.MeshDiagLog("SQ4KOU high-res waterfall ACTIVE: native DirectCompute FFT=16384 -> Vortice colour compute -> WaterfallMesh/D2D presenter");
                    }
                    return true;
                }
                catch (Exception e)
                {
                    _sq4kouHighResReady[source] = false;
                    _sq4kouHighResPaneStatus[pane] = "GPU FFT EXCEPTION -> LEGACY";
                    Common.MeshDiagLog("SQ4KOU high-res waterfall exception; legacy fallback active: " + e.Message);
                    return false;
                }
            }
        }
    }
}
