using System;

namespace Thetis
{
    partial class Display
    {
        private static int _gpuWaterfallTemporalMode = 0; // 0=Off, 1=Fast, 2=Smooth

        public static int GPUWaterfallTemporalMode
        {
            get { return _gpuWaterfallTemporalMode; }
            set { _gpuWaterfallTemporalMode = Math.Max(0, Math.Min(2, value)); }
        }

        public static int GPUWaterfallRuntimeLevelRX1 { get { return GPUWaterfallRuntimeLevel(0); } }
        public static int GPUWaterfallRuntimeLevelRX2 { get { return GPUWaterfallRuntimeLevel(1); } }

        private static int GPUWaterfallRuntimeLevel(int channel)
        {
            if (!_waterfallUseGPU) return 0;
            if (channel < 0 || channel > 1) return 0;
            if (_gpuWaterfallFailed[channel]) return 0;
            if (_gpuWaterfallConfiguredFFT[channel] <= 0) return 1;
            try
            {
                return GPUWaterfallNative.CM_GPUWaterfall_IsReady(channel) != 0 ? 2 : 1;
            }
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
            ResetGPUWaterfallState();
        }
    }
}
