using System;

namespace Thetis
{
    partial class Display
    {
        // Compatibility surface for the original SQ4KOU / EU2AV
        // Setup -> Display -> Waterfall menu. The UI contract is retained,
        // while the implementation is mapped onto the SDR-VST3 Vortice backend.

        public enum WaterfallRenderQuality { Low, Medium, High }

        private static WaterfallRenderQuality _waterfallRenderQuality = WaterfallRenderQuality.High;
        private static NoiseFloorPro.DetectionMode _nfMode = NoiseFloorPro.DetectionMode.Average;
        private static float _nfLowPct = 10f;
        private static float _nfHighPct = 99f;
        private static float _wfAgcSmoothing = 0.4f;
        private static float _autoHighMarginDb = 6f;
        private static float _temporalAlpha = 0f;
        private static bool _autoHighEnabledRX1;
        private static bool _autoHighEnabledRX2;
        private static bool _temporalEnabled;
        private static bool _zoomAdaptiveEnabled = true;
        private static bool _autoEnableGPU = true;
        private static bool _gpuEffectsEnabled = true;

        private static readonly float[][] _sq4kouProScratch = new float[2][];
        private static readonly float[][] _sq4kouProSpatial = new float[2][];
        private static readonly float[][] _sq4kouTemporalPrevious = new float[2][];
        private static readonly bool[] _sq4kouProFloorValid = new bool[2];
        private static readonly float[] _sq4kouProLowDb = new float[2];
        private static readonly float[] _sq4kouProHighDb = new float[2];

        public static event Action<int, double> GPUWaterfallEffectiveOverlapChanged;

        public static WaterfallRenderQuality WaterfallQuality
        {
            get { return _waterfallRenderQuality; }
            set { _waterfallRenderQuality = value; }
        }

        public static NoiseFloorPro.DetectionMode NFMode
        {
            get { return _nfMode; }
            set { _nfMode = value; ResetSQ4KOUProTracking(); }
        }

        public static float NFLowPct
        {
            get { return _nfLowPct; }
            set { _nfLowPct = Math.Max(1f, Math.Min(49f, value)); ResetSQ4KOUProTracking(); }
        }

        public static float NFHighPct
        {
            get { return _nfHighPct; }
            set { _nfHighPct = Math.Max(50f, Math.Min(99.9f, value)); ResetSQ4KOUProTracking(); }
        }

        public static float WaterfallAgcSmoothing
        {
            get { return _wfAgcSmoothing; }
            set { _wfAgcSmoothing = Math.Max(0.05f, Math.Min(0.9f, value)); }
        }

        public static bool AutoHighEnabledRX1
        {
            get { return _autoHighEnabledRX1; }
            set { _autoHighEnabledRX1 = value; ResetSQ4KOUProTracking(); }
        }

        public static bool AutoHighEnabledRX2
        {
            get { return _autoHighEnabledRX2; }
            set { _autoHighEnabledRX2 = value; ResetSQ4KOUProTracking(); }
        }

        public static float AutoHighMarginDb
        {
            get { return _autoHighMarginDb; }
            set { _autoHighMarginDb = Math.Max(0f, Math.Min(30f, value)); ResetSQ4KOUProTracking(); }
        }

        public static bool TemporalEnabled
        {
            get { return _temporalEnabled; }
            set { _temporalEnabled = value; ResetSQ4KOUTemporal(); }
        }

        public static float TemporalStrength
        {
            get { return _temporalAlpha; }
            set { _temporalAlpha = Math.Max(0f, Math.Min(0.5f, value)); ResetSQ4KOUTemporal(); }
        }

        public static bool ZoomAdaptiveEnabled
        {
            get { return _zoomAdaptiveEnabled; }
            set { _zoomAdaptiveEnabled = value; }
        }

        public static bool AutoEnableGPU
        {
            get { return _autoEnableGPU; }
            set { _autoEnableGPU = value; }
        }

        public static bool GPUEffectsEnabled
        {
            get { return _gpuEffectsEnabled && GpuComputeEnabled; }
            set
            {
                _gpuEffectsEnabled = value;
                GpuComputeEnabled = value;
                if (!value)
                    SQ4KOUHighResWaterfallEnabled = false;
            }
        }

        public static bool GPUEffectsAvailable { get { return GPUDetector.HasBuiltInEffects; } }
        public static string GPUName { get { return GPUDetector.GPUName ?? "unknown"; } }
        public static int GPUDetectionLevel { get { return (int)GPUDetector.Level; } }

        public static bool GPUWaterfallPipelineEnabled
        {
            get { return SQ4KOUHighResWaterfallEnabled; }
            set
            {
                SQ4KOUHighResWaterfallEnabled = value;
                ResetSQ4KOUHighResBackend();
            }
        }

        public static int GPUWaterfallFFTSize
        {
            get { return _sq4kouHighResFftSize; }
            set
            {
                if (value < 1024) value = 1024;
                if (value > 262144) value = 262144;
                if ((value & (value - 1)) != 0) value = 16384;
                _sq4kouHighResFftSize = value;
                ResetSQ4KOUHighResBackend();
            }
        }

        public static GPUWaterfallWindowType GPUWaterfallWindowType
        {
            get { return (GPUWaterfallWindowType)_sq4kouHighResWindowType; }
            set
            {
                _sq4kouHighResWindowType = Math.Max(0, Math.Min(5, (int)value));
                ResetSQ4KOUHighResBackend();
            }
        }

        public static double GPUWaterfallKaiserBeta
        {
            get { return _sq4kouHighResKaiserBeta; }
            set
            {
                _sq4kouHighResKaiserBeta = (float)Math.Max(0.0, Math.Min(20.0, value));
                ResetSQ4KOUHighResBackend();
            }
        }

        public static GPUWaterfallMagnitudeMode GPUWaterfallMagnitudeMode
        {
            get { return (GPUWaterfallMagnitudeMode)_sq4kouHighResMagnitudeMode; }
            set
            {
                _sq4kouHighResMagnitudeMode = Math.Max(0, Math.Min(2, (int)value));
                ResetSQ4KOUHighResBackend();
            }
        }

        public static int GPUWaterfallOverlapPercent
        {
            get { return (int)Math.Round(_sq4kouHighResOverlapPercent); }
            set
            {
                _sq4kouHighResOverlapPercent = Math.Max(0f, Math.Min(95f, value));
                ResetSQ4KOUHighResBackend();
                GPUWaterfallEffectiveOverlapChanged?.Invoke(1, _sq4kouHighResOverlapPercent / 100.0);
            }
        }

        public static bool GPUWaterfallAutoOverlap
        {
            get { return _sq4kouHighResAutoOverlap; }
            set
            {
                _sq4kouHighResAutoOverlap = value;
                ResetSQ4KOUHighResBackend();
            }
        }

        public static int GPUWaterfallLanczosWindow
        {
            get { return _sq4kouHighResLanczosWindow; }
            set
            {
                _sq4kouHighResLanczosWindow = value <= 0 ? 2 : Math.Max(2, Math.Min(4, value));
                ResetSQ4KOUHighResBackend();
            }
        }

        public static GPUWaterfallResamplingMode GPUWaterfallResamplingMode
        {
            get { return (GPUWaterfallResamplingMode)Math.Max(0, Math.Min(1, _sq4kouHighResResamplingMode)); }
            set
            {
                // Original menu: Fast = linear, Quality = power-average.
                _sq4kouHighResResamplingMode = ((int)value == 0) ? 0 : 1;
                ResetSQ4KOUHighResBackend();
            }
        }

        private static void ResetSQ4KOUHighResBackend()
        {
            lock (_sq4kouHighResLock)
            {
                for (int i = 0; i < 3; i++)
                {
                    _sq4kouHighResReady[i] = false;
                    _sq4kouHighResCalValid[i] = false;
                    _sq4kouHighResLoggedActive[i] = false;
                    _sq4kouHighResLoggedFailure[i] = false;
                }
            }
            ResetSQ4KOUProTracking();
            ResetSQ4KOUTemporal();
        }

        private static void ResetSQ4KOUProTracking()
        {
            _sq4kouProFloorValid[0] = false;
            _sq4kouProFloorValid[1] = false;
        }

        private static void ResetSQ4KOUTemporal()
        {
            _sq4kouTemporalPrevious[0] = null;
            _sq4kouTemporalPrevious[1] = null;
        }

        // Applies the original Waterfall Pro controls to the high-resolution
        // dBm row before colour conversion. This is intentionally bypassed
        // on the legacy analyser path so A/B remains meaningful.
        private static void ApplySQ4KOUWaterfallPro(int pane, float[] row, int width)
        {
            if (row == null || width < 3) return;
            pane = pane == 1 ? 1 : 0;

            if (_nfMode == NoiseFloorPro.DetectionMode.Percentile)
            {
                if (_sq4kouProScratch[pane] == null || _sq4kouProScratch[pane].Length < width)
                    _sq4kouProScratch[pane] = new float[width];

                float[] scratch = _sq4kouProScratch[pane];
                int count = 0;
                for (int i = 0; i < width; i++)
                {
                    float v = row[i];
                    if (!float.IsNaN(v) && !float.IsInfinity(v))
                        scratch[count++] = v;
                }

                if (count >= 16)
                {
                    Array.Sort(scratch, 0, count);
                    int lowIndex = Math.Max(0, Math.Min(count - 1,
                        (int)Math.Round((count - 1) * (_nfLowPct / 100.0))));
                    int highIndex = Math.Max(lowIndex + 1, Math.Min(count - 1,
                        (int)Math.Round((count - 1) * (_nfHighPct / 100.0))));

                    float low = scratch[lowIndex];
                    float high = scratch[highIndex];
                    bool autoHigh = pane == 0 ? _autoHighEnabledRX1 : _autoHighEnabledRX2;
                    if (autoHigh) high += _autoHighMarginDb;
                    if (high < low + 3f) high = low + 3f;

                    float a = _wfAgcSmoothing;
                    if (!_sq4kouProFloorValid[pane])
                    {
                        _sq4kouProLowDb[pane] = low;
                        _sq4kouProHighDb[pane] = high;
                        _sq4kouProFloorValid[pane] = true;
                    }
                    else
                    {
                        _sq4kouProLowDb[pane] += a * (low - _sq4kouProLowDb[pane]);
                        _sq4kouProHighDb[pane] += a * (high - _sq4kouProHighDb[pane]);
                    }

                    low = _sq4kouProLowDb[pane];
                    high = _sq4kouProHighDb[pane];
                    for (int i = 0; i < width; i++)
                    {
                        if (row[i] < low) row[i] = low;
                        else if (row[i] > high) row[i] = high;
                    }
                }
            }

            float contrast = WaterfallEnhancer.PaletteContrast;
            if (contrast > 0.0001f)
            {
                float min = row[0], max = row[0];
                for (int i = 1; i < width; i++)
                {
                    if (row[i] < min) min = row[i];
                    if (row[i] > max) max = row[i];
                }
                float mid = 0.5f * (min + max);
                float gain = 1.0f + 0.75f * contrast;
                for (int i = 0; i < width; i++)
                    row[i] = mid + (row[i] - mid) * gain;
            }

            float sharpness = WaterfallEnhancer.PaletteSharpness;
            if (sharpness > 0.0001f)
            {
                if (_sq4kouProSpatial[pane] == null || _sq4kouProSpatial[pane].Length < width)
                    _sq4kouProSpatial[pane] = new float[width];
                float[] src = _sq4kouProSpatial[pane];
                Array.Copy(row, src, width);
                float amount = 0.35f * Math.Min(1.5f, sharpness);
                for (int i = 1; i < width - 1; i++)
                {
                    float neighbours = 0.5f * (src[i - 1] + src[i + 1]);
                    row[i] = src[i] + amount * (src[i] - neighbours);
                }
            }

            if (_temporalEnabled && _temporalAlpha > 0.0001f)
            {
                if (_sq4kouTemporalPrevious[pane] == null || _sq4kouTemporalPrevious[pane].Length != width)
                {
                    _sq4kouTemporalPrevious[pane] = new float[width];
                    Array.Copy(row, _sq4kouTemporalPrevious[pane], width);
                }
                else
                {
                    float[] prev = _sq4kouTemporalPrevious[pane];
                    float a = _temporalAlpha;
                    for (int i = 0; i < width; i++)
                    {
                        float v = row[i] * (1f - a) + prev[i] * a;
                        prev[i] = v;
                        row[i] = v;
                    }
                }
            }
        }
    }
}
