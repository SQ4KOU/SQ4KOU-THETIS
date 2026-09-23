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

        private static int _sq4kouHighResFftSize = 16384;
        private static int _sq4kouHighResWindowType = 1;       // Hamming
        private static float _sq4kouHighResKaiserBeta = 8.6f;
        private static int _sq4kouHighResMagnitudeMode = 0;    // dBFS
        private static bool _sq4kouHighResAutoOverlap = false;
        private static float _sq4kouHighResOverlapPercent = 0.0f;
        private static int _sq4kouHighResLanczosWindow = 3;
        private static int _sq4kouHighResResamplingMode = 0;   // Linear

        public static bool SQ4KOUHighResWaterfallEnabled { get; set; } = true;
        public static int SQ4KOUHighResFftSize { get { return _sq4kouHighResFftSize; } }
        public static int SQ4KOUHighResWindowType { get { return _sq4kouHighResWindowType; } }
        public static int SQ4KOUHighResResamplingMode { get { return _sq4kouHighResResamplingMode; } }
        public static float SQ4KOUHighResOverlapPercent { get { return _sq4kouHighResOverlapPercent; } }
        public static bool SQ4KOUHighResAutoOverlap { get { return _sq4kouHighResAutoOverlap; } }
        public static string SQ4KOUHighResWaterfallStatusRX1 { get { return _sq4kouHighResPaneStatus[0]; } }
        public static string SQ4KOUHighResWaterfallStatusRX2 { get { return _sq4kouHighResPaneStatus[1]; } }

        public static void ConfigureSQ4KOUHighResWaterfall(int fftSize, int windowType,
            int resamplingMode, bool autoOverlap, float overlapPercent)
        {
            int[] allowed = new int[] { 2048, 4096, 8192, 16384, 32768, 65536 };
            bool validFft = false;
            for (int i = 0; i < allowed.Length; i++)
                if (allowed[i] == fftSize) { validFft = true; break; }
            if (!validFft) fftSize = 16384;

            windowType = Math.Max(0, Math.Min(5, windowType));
            resamplingMode = Math.Max(0, Math.Min(3, resamplingMode));
            overlapPercent = Math.Max(0.0f, Math.Min(95.0f, overlapPercent));

            lock (_sq4kouHighResLock)
            {
                _sq4kouHighResFftSize = fftSize;
                _sq4kouHighResWindowType = windowType;
                _sq4kouHighResResamplingMode = resamplingMode;
                _sq4kouHighResAutoOverlap = autoOverlap;
                _sq4kouHighResOverlapPercent = overlapPercent;

                for (int i = 0; i < 3; i++)
                {
                    _sq4kouHighResReady[i] = false;
                    _sq4kouHighResCalValid[i] = false;
                    _sq4kouHighResLoggedActive[i] = false;
                    _sq4kouHighResLoggedFailure[i] = false;
                }
            }
        }

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

            // High-res data path uses the Vortice colour-compute stage, so keep it
            // tied to ComputeArmed. Legacy Thetis remains an immediate fallback.
            if (!SQ4KOUHighResWaterfallEnabled || !ComputeArmed || width < 64)
            {
                _sq4kouHighResPaneStatus[pane] = SQ4KOUHighResWaterfallEnabled
                    ? "LEGACY (GPU COLOR COMPUTE UNAVAILABLE)"
                    : "LEGACY (A/B)";
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
                _sq4kouHighResPaneStatus[pane] = "LEGACY (INVALID RATE/SPAN)";
                return false;
            }

            lock (_sq4kouHighResLock)
            {
                try
                {
                    if (!_sq4kouHighResReady[source] || CM_GPUWaterfall_IsReady(source) == 0)
                    {
                        if (CM_GPUWaterfall_Init(source, _sq4kouHighResFftSize, _sq4kouHighResFftSize * 4) == 0)
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

                        CM_GPUWaterfall_Configure(source,
                            _sq4kouHighResWindowType,
                            _sq4kouHighResKaiserBeta,
                            _sq4kouHighResMagnitudeMode,
                            _sq4kouHighResAutoOverlap ? 1 : 0,
                            _sq4kouHighResOverlapPercent,
                            _sq4kouHighResLanczosWindow,
                            _sq4kouHighResResamplingMode);

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

                    ApplySQ4KOUWaterfallPro(pane, row, width);

                    highResRow = row;
                    _sq4kouHighResPaneStatus[pane] = "GPU FFT " + _sq4kouHighResFftSize + " ACTIVE";

                    if (!_sq4kouHighResLoggedActive[source])
                    {
                        _sq4kouHighResLoggedActive[source] = true;
                        Common.MeshDiagLog("SQ4KOU high-res waterfall ACTIVE: native DirectCompute FFT=" +
                            _sq4kouHighResFftSize + " -> Vortice colour compute -> WaterfallMesh/D2D presenter");
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
