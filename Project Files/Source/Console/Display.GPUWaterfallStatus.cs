using System;

namespace Thetis
{
    partial class Display
    {
        // V4 controls mirrored from the 2.10.3.16 Extended Waterfall Pro page.
        private static int _gpuWaterfallTemporalMode = 0; // 0=Off, 1=Fast, 2=Smooth
        private static int _gpuWaterfallNFMode = 1;       // 0=Off, 1=Percentile
        private static int _gpuWaterfallNFLowPercent = 1;
        private static int _gpuWaterfallNFHighPercent = 99;
        private static bool _gpuWaterfallNFAutoHigh = true;
        private static float _gpuWaterfallNFAutoHighDb = 0.0f;
        private static int _gpuWaterfallNFAGCSmooth = 1;  // 0=Off, 1=Fast, 2=Medium, 3=Slow
        private static int _gpuWaterfallDetectMode = 0;   // 0=Peak, 1=Average
        private static bool _gpuWaterfallZoomAdaptive = false;
        private static float _gpuWaterfallPaletteSharpness = 0.0f; // 0..1
        private static float _gpuWaterfallPaletteContrast = 0.0f;  // 0..1

        private static readonly float[][] _gpuProScratch = new float[2][];
        private static readonly float[][] _gpuProSpatial = new float[2][];
        private static readonly bool[] _gpuProFloorValid = new bool[2];
        private static readonly float[] _gpuProLowDb = new float[2];
        private static readonly float[] _gpuProHighDb = new float[2];

        public static int GPUWaterfallTemporalMode
        {
            get { return _gpuWaterfallTemporalMode; }
            set { _gpuWaterfallTemporalMode = Math.Max(0, Math.Min(2, value)); }
        }

        public static int GPUWaterfallNFMode { get { return _gpuWaterfallNFMode; } set { _gpuWaterfallNFMode = Math.Max(0, Math.Min(1, value)); ResetGPUWaterfallProTracking(); } }
        public static int GPUWaterfallNFLowPercent { get { return _gpuWaterfallNFLowPercent; } set { _gpuWaterfallNFLowPercent = Math.Max(0, Math.Min(49, value)); ResetGPUWaterfallProTracking(); } }
        public static int GPUWaterfallNFHighPercent { get { return _gpuWaterfallNFHighPercent; } set { _gpuWaterfallNFHighPercent = Math.Max(51, Math.Min(100, value)); ResetGPUWaterfallProTracking(); } }
        public static bool GPUWaterfallNFAutoHigh { get { return _gpuWaterfallNFAutoHigh; } set { _gpuWaterfallNFAutoHigh = value; ResetGPUWaterfallProTracking(); } }
        public static float GPUWaterfallNFAutoHighDb { get { return _gpuWaterfallNFAutoHighDb; } set { _gpuWaterfallNFAutoHighDb = Math.Max(-30.0f, Math.Min(30.0f, value)); ResetGPUWaterfallProTracking(); } }
        public static int GPUWaterfallNFAGCSmooth { get { return _gpuWaterfallNFAGCSmooth; } set { _gpuWaterfallNFAGCSmooth = Math.Max(0, Math.Min(3, value)); } }
        public static int GPUWaterfallDetectMode { get { return _gpuWaterfallDetectMode; } set { _gpuWaterfallDetectMode = Math.Max(0, Math.Min(1, value)); ResetGPUWaterfallProTracking(); } }
        public static bool GPUWaterfallZoomAdaptive { get { return _gpuWaterfallZoomAdaptive; } set { _gpuWaterfallZoomAdaptive = value; } }
        public static float GPUWaterfallPaletteSharpness { get { return _gpuWaterfallPaletteSharpness; } set { _gpuWaterfallPaletteSharpness = Math.Max(0.0f, Math.Min(1.0f, value)); } }
        public static float GPUWaterfallPaletteContrast { get { return _gpuWaterfallPaletteContrast; } set { _gpuWaterfallPaletteContrast = Math.Max(0.0f, Math.Min(1.0f, value)); } }

        public static int GPUWaterfallRuntimeLevelRX1 { get { return GPUWaterfallRuntimeLevel(0); } }
        public static int GPUWaterfallRuntimeLevelRX2 { get { return GPUWaterfallRuntimeLevel(1); } }

        private static int GPUWaterfallRuntimeLevel(int channel)
        {
            if (!_waterfallUseGPU) return 0;
            if (channel < 0 || channel > 1) return 0;
            if (_gpuWaterfallFailed[channel]) return 0;
            if (_gpuWaterfallConfiguredFFT[channel] <= 0) return 1;
            try { return GPUWaterfallNative.CM_GPUWaterfall_IsReady(channel) != 0 ? 2 : 1; }
            catch { return 0; }
        }

        public static string GPUWaterfallRuntimeTextRX1 { get { return GPUWaterfallRuntimeText(0); } }
        public static string GPUWaterfallRuntimeTextRX2 { get { return GPUWaterfallRuntimeText(1); } }

        private static string GPUWaterfallRuntimeText(int channel)
        {
            int level = GPUWaterfallRuntimeLevel(channel);
            if (level >= 2) return "Level 2 (GPU FFT ACTIVE)";
            if (level == 1) return "Level 1 (GPU READY / waiting for IQ)";
            return _waterfallUseGPU ? "Level 0 (CPU FALLBACK)" : "Level 0 (CPU)";
        }

        public static string GPUWaterfallAdapterName { get { return GPUDetectorNative.GetAdapterName(); } }
        public static string GPUWaterfallFeatureLevel { get { return GPUDetectorNative.GetFeatureLevelText(); } }
        public static string GPUWaterfallCapabilities { get { return GPUDetectorNative.GetCapabilities(); } }

        public static bool TestGPUWaterfallHardware()
        {
            try { return GPUDetectorNative.CM_GPUDetector_Test() != 0; }
            catch { return false; }
        }

        private static void ResetGPUWaterfallProTracking()
        {
            for (int ch = 0; ch < 2; ch++) _gpuProFloorValid[ch] = false;
        }

        public static void ApplyGPUWaterfallEU2AVDefaults()
        {
            _gpuWaterfallFFTSize = 16384;
            _gpuWaterfallWindowType = 1;          // Hamming
            _gpuWaterfallKaiserBeta = 8.6f;
            _gpuWaterfallMagnitudeMode = 0;       // Peak Amp / dBFS
            _gpuWaterfallAutoOverlap = false;
            _gpuWaterfallOverlapPercent = 0.0f;
            _gpuWaterfallLanczosWindow = 3;
            _gpuWaterfallResamplingMode = 0;      // Linear
            _gpuWaterfallTemporalMode = 0;        // Off
            _gpuWaterfallNFMode = 1;
            _gpuWaterfallNFLowPercent = 1;
            _gpuWaterfallNFHighPercent = 99;
            _gpuWaterfallNFAutoHigh = true;
            _gpuWaterfallNFAutoHighDb = 0.0f;
            _gpuWaterfallNFAGCSmooth = 1;         // Fast
            _gpuWaterfallDetectMode = 0;          // Peak
            _gpuWaterfallZoomAdaptive = false;
            _gpuWaterfallPaletteSharpness = 0.0f;
            _gpuWaterfallPaletteContrast = 0.0f;
            ResetGPUWaterfallProTracking();
            ResetGPUWaterfallState();
        }

        // Percentile clipping mirrors the Extended "Noise Floor Pro" concept while preserving
        // absolute dB calibration inside the accepted percentile range. It only suppresses row
        // outliers, so it cannot invent gain or change the central spectral calibration.
        private static void ApplyGPUWaterfallPro(int channel, float[] row, int width)
        {
            if (row == null || width <= 2) return;

            if (_gpuWaterfallNFMode != 0)
            {
                if (_gpuProScratch[channel] == null || _gpuProScratch[channel].Length < width)
                    _gpuProScratch[channel] = new float[width];
                float[] scratch = _gpuProScratch[channel];
                int count = 0;
                for (int i = 0; i < width; i++)
                {
                    float v = row[i];
                    if (!float.IsNaN(v) && !float.IsInfinity(v)) scratch[count++] = v;
                }

                if (count >= 16)
                {
                    Array.Sort(scratch, 0, count);
                    int lowIndex = (int)Math.Round((count - 1) * (_gpuWaterfallNFLowPercent / 100.0));
                    int highIndex = (int)Math.Round((count - 1) * (_gpuWaterfallNFHighPercent / 100.0));
                    lowIndex = Math.Max(0, Math.Min(count - 1, lowIndex));
                    highIndex = Math.Max(lowIndex + 1, Math.Min(count - 1, highIndex));

                    float low = scratch[lowIndex];
                    float high = _gpuWaterfallDetectMode == 0 ? scratch[count - 1] : scratch[highIndex];
                    if (_gpuWaterfallNFAutoHigh) high += _gpuWaterfallNFAutoHighDb;
                    if (high < low + 3.0f) high = low + 3.0f;

                    float a;
                    switch (_gpuWaterfallNFAGCSmooth)
                    {
                        case 0: a = 1.0f; break;
                        case 2: a = 0.12f; break;
                        case 3: a = 0.04f; break;
                        default: a = 0.35f; break;
                    }

                    if (!_gpuProFloorValid[channel])
                    {
                        _gpuProLowDb[channel] = low;
                        _gpuProHighDb[channel] = high;
                        _gpuProFloorValid[channel] = true;
                    }
                    else
                    {
                        _gpuProLowDb[channel] += a * (low - _gpuProLowDb[channel]);
                        _gpuProHighDb[channel] += a * (high - _gpuProHighDb[channel]);
                    }

                    low = _gpuProLowDb[channel];
                    high = _gpuProHighDb[channel];
                    for (int i = 0; i < width; i++)
                    {
                        if (row[i] < low) row[i] = low;
                        else if (row[i] > high) row[i] = high;
                    }
                }
            }

            // Palette contrast acts in the dB domain around the row midpoint.
            if (_gpuWaterfallPaletteContrast > 0.0001f)
            {
                float min = row[0], max = row[0];
                for (int i = 1; i < width; i++)
                {
                    if (row[i] < min) min = row[i];
                    if (row[i] > max) max = row[i];
                }
                float mid = 0.5f * (min + max);
                float gain = 1.0f + 0.75f * _gpuWaterfallPaletteContrast;
                for (int i = 0; i < width; i++) row[i] = mid + (row[i] - mid) * gain;
            }

            // Palette sharpness is a conservative 1-D unsharp mask across frequency.
            if (_gpuWaterfallPaletteSharpness > 0.0001f)
            {
                if (_gpuProSpatial[channel] == null || _gpuProSpatial[channel].Length < width)
                    _gpuProSpatial[channel] = new float[width];
                float[] src = _gpuProSpatial[channel];
                Array.Copy(row, src, width);
                float amount = 0.35f * _gpuWaterfallPaletteSharpness;
                for (int i = 1; i < width - 1; i++)
                {
                    float neighbours = 0.5f * (src[i - 1] + src[i + 1]);
                    row[i] = src[i] + amount * (src[i] - neighbours);
                }
            }
        }
    }
}
