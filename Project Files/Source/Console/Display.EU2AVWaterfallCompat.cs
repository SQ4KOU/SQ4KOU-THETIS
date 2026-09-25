using System;

namespace Thetis
{
    partial class Display
    {
        public enum WaterfallRenderQuality { Low, Medium, High }

        private static WaterfallRenderQuality _waterfallRenderQuality = WaterfallRenderQuality.High;
        private static NoiseFloorPro.DetectionMode _nfMode = NoiseFloorPro.DetectionMode.Average;
        private static float _nfLowPct = 10f;
        private static float _nfHighPct = 99f;
        private static float _wfAgcSmoothing = 0.4f;
        private static float _autoHighMarginDb = 6f;
        private static float _temporalAlpha = 0f;
        private static float _autoThresholdFineOffset = -3f;
        private static bool _autoHighEnabledRX1;
        private static bool _autoHighEnabledRX2;
        private static bool _temporalEnabled;
        private static bool _autoThresholdEnabled;
        private static bool _zoomAdaptiveEnabled = true;
        private static bool _autoEnableGPU = true;
        private static bool _gpuEffectsEnabled;

        public static WaterfallRenderQuality WaterfallQuality
        {
            get { return _waterfallRenderQuality; }
            set
            {
                if (_waterfallRenderQuality == value) return;
                _waterfallRenderQuality = value;
                _gpuWaterfallLinearDraw = value != WaterfallRenderQuality.Low;
                LogGPU("WaterfallQuality changed to " + value + ", linearDraw=" + _gpuWaterfallLinearDraw);
            }
        }

        public static NoiseFloorPro.DetectionMode NFMode { get { return _nfMode; } set { _nfMode = value; } }
        public static float NFLowPct { get { return _nfLowPct; } set { _nfLowPct = value < 1f ? 1f : (value > 49f ? 49f : value); } }
        public static float NFHighPct { get { return _nfHighPct; } set { _nfHighPct = value < 50f ? 50f : (value > 99.9f ? 99.9f : value); } }
        public static float WaterfallAgcSmoothing { get { return _wfAgcSmoothing; } set { _wfAgcSmoothing = value < 0.05f ? 0.05f : (value > 0.9f ? 0.9f : value); } }
        public static bool AutoHighEnabledRX1 { get { return _autoHighEnabledRX1; } set { _autoHighEnabledRX1 = value; } }
        public static bool AutoHighEnabledRX2 { get { return _autoHighEnabledRX2; } set { _autoHighEnabledRX2 = value; } }
        public static float AutoHighMarginDb { get { return _autoHighMarginDb; } set { _autoHighMarginDb = value < 0f ? 0f : (value > 30f ? 30f : value); } }
        public static bool TemporalEnabled
        {
            get { return _temporalEnabled; }
            set
            {
                _temporalEnabled = value;
                if (!value) ResetTemporalWaterfallState();
            }
        }
        public static float TemporalStrength { get { return _temporalAlpha; } set { _temporalAlpha = value < 0f ? 0f : (value > 0.5f ? 0.5f : value); } }
        public static bool AutoThresholdEnabled { get { return _autoThresholdEnabled; } set { _autoThresholdEnabled = value; } }
        public static float AutoThresholdFineOffset { get { return _autoThresholdFineOffset; } set { _autoThresholdFineOffset = value < -20f ? -20f : (value > 20f ? 20f : value); } }
        public static bool ZoomAdaptiveEnabled { get { return _zoomAdaptiveEnabled; } set { _zoomAdaptiveEnabled = value; } }
        public static bool AutoEnableGPU { get { return _autoEnableGPU; } set { _autoEnableGPU = value; } }

        public static bool GPUEffectsEnabled
        {
            get { return _gpuEffectsEnabled; }
            set
            {
                if (_gpuEffectsEnabled == value) return;
                _gpuEffectsEnabled = value;
                if (!value)
                {
                    _gpuRendererHasData[0] = false;
                    _gpuRendererHasData[1] = false;
                    WaterfallEffect.Reset();
                }
                ResetGPUWaterfallState(1, false);
                ResetGPUWaterfallState(2, false);
            }
        }

        public static bool GPUEffectsAvailable { get { return WaterfallEffect.IsAvailable; } }
        public static string GPUName { get { return _gpu ?? "unknown"; } }
        public static int GPUDetectionLevel { get { return (int)GPUDetector.Level; } }
    }
}
