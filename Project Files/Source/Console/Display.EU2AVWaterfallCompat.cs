using System;
namespace Thetis
{
    partial class Display
    {
        public enum WaterfallRenderQuality { Low, Medium, High }
        private static WaterfallRenderQuality _waterfallRenderQuality = WaterfallRenderQuality.High;
        private static NoiseFloorPro.DetectionMode _nfMode = NoiseFloorPro.DetectionMode.Average;
        private static float _nfLowPct = 10f, _nfHighPct = 99f, _wfAgcSmoothing = 0.4f, _autoHighMarginDb = 6f, _temporalAlpha, _autoThresholdFineOffset = -3f;
        private static bool _autoHighEnabledRX1, _autoHighEnabledRX2, _temporalEnabled, _autoThresholdEnabled, _zoomAdaptiveEnabled = true, _autoEnableGPU = true, _gpuEffectsEnabled;
        private static readonly double[] _lastReportedEffectiveOverlap = new double[] { -1.0, -1.0 };
        public static event Action<int, double> GPUWaterfallEffectiveOverlapChanged;
        public static bool GPUWaterfallPipelineEnabled { get { return WaterfallUseGPU; } set { WaterfallUseGPU = value; } }
        public static WaterfallRenderQuality WaterfallQuality { get { return _waterfallRenderQuality; } set { _waterfallRenderQuality = value; } }
        public static NoiseFloorPro.DetectionMode NFMode { get { return _nfMode; } set { _nfMode = value; } }
        public static float NFLowPct { get { return _nfLowPct; } set { _nfLowPct = value < 1f ? 1f : (value > 49f ? 49f : value); } }
        public static float NFHighPct { get { return _nfHighPct; } set { _nfHighPct = value < 50f ? 50f : (value > 99.9f ? 99.9f : value); } }
        public static float WaterfallAgcSmoothing { get { return _wfAgcSmoothing; } set { _wfAgcSmoothing = value < 0.05f ? 0.05f : (value > 0.9f ? 0.9f : value); } }
        public static bool AutoHighEnabledRX1 { get { return _autoHighEnabledRX1; } set { _autoHighEnabledRX1 = value; } }
        public static bool AutoHighEnabledRX2 { get { return _autoHighEnabledRX2; } set { _autoHighEnabledRX2 = value; } }
        public static float AutoHighMarginDb { get { return _autoHighMarginDb; } set { _autoHighMarginDb = value < 0f ? 0f : (value > 30f ? 30f : value); } }
        public static bool TemporalEnabled { get { return _temporalEnabled; } set { _temporalEnabled = value; if (!value) { _gpuPrevValid[0] = false; _gpuPrevValid[1] = false; } } }
        public static float TemporalStrength { get { return _temporalAlpha; } set { _temporalAlpha = value < 0f ? 0f : (value > 0.5f ? 0.5f : value); } }
        public static bool AutoThresholdEnabled { get { return _autoThresholdEnabled; } set { _autoThresholdEnabled = value; } }
        public static float AutoThresholdFineOffset { get { return _autoThresholdFineOffset; } set { _autoThresholdFineOffset = value < -20f ? -20f : (value > 20f ? 20f : value); } }
        public static bool ZoomAdaptiveEnabled { get { return _zoomAdaptiveEnabled; } set { _zoomAdaptiveEnabled = value; } }
        public static bool AutoEnableGPU { get { return _autoEnableGPU; } set { _autoEnableGPU = value; } }
        public static bool GPUEffectsEnabled { get { return _gpuEffectsEnabled; } set { _gpuEffectsEnabled = value; } }
        public static bool GPUEffectsAvailable { get { return GPUDetector.HasBuiltInEffects; } }
        public static string GPUName { get { return GPUDetector.GPUName ?? "unknown"; } }
        public static int GPUDetectionLevel { get { return (int)GPUDetector.Level; } }
        internal static void ReportGPUWaterfallEffectiveOverlap(int channel, int sampleRate)
        {
            if (channel < 0 || channel > 1 || _gpuWaterfallFFTSize <= 0) return;
            double overlap;
            if (_gpuWaterfallAutoOverlap && sampleRate > 0)
            {
                int hop = (int)Math.Floor((double)sampleRate / 30.0 + 0.5);
                int minHop = Math.Max(1, (int)Math.Floor((double)_gpuWaterfallFFTSize * 0.05 + 0.5));
                if (hop < minHop) hop = minHop; if (hop > _gpuWaterfallFFTSize) hop = _gpuWaterfallFFTSize;
                overlap = 1.0 - (double)hop / (double)_gpuWaterfallFFTSize;
            }
            else overlap = (double)_gpuWaterfallOverlapPercent / 100.0;
            if (overlap < 0.0) overlap = 0.0; if (overlap > 0.95) overlap = 0.95;
            if (Math.Abs(_lastReportedEffectiveOverlap[channel] - overlap) < 0.002) return;
            _lastReportedEffectiveOverlap[channel] = overlap;
            var handler = GPUWaterfallEffectiveOverlapChanged; if (handler != null) handler(channel + 1, overlap);
        }
    }
}
