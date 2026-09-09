using System;

namespace Thetis
{
    partial class Display
    {
        public static bool GPUWaterfallActualActiveRX1 { get { return GPUWaterfallActualActive(1); } }
        public static bool GPUWaterfallActualActiveRX2 { get { return GPUWaterfallActualActive(2); } }
        public static string GPUWaterfallRuntimeStatusRX1 { get { return GPUWaterfallRuntimeStatus(1); } }
        public static string GPUWaterfallRuntimeStatusRX2 { get { return GPUWaterfallRuntimeStatus(2); } }

        private static bool GPUWaterfallActualActive(int rx)
        {
            int index = rx - 1;
            if (index < 0 || index > 1) return false;

            GPUWaterfallPipeline pipeline = rx == 1 ? _gpuFFT1 : _gpuFFT2;
            WaterfallGPURenderer renderer = rx == 1 ? _waterfallGPU1 : _waterfallGPU2;

            return ManagedGPUFFTRequested &&
                   _gpuRendererHasData[index] &&
                   pipeline != null && pipeline.IsInitialized &&
                   renderer != null && renderer.IsInitialized;
        }

        private static string GPUWaterfallRuntimeStatus(int rx)
        {
            int index = rx - 1;
            if (index < 0 || index > 1) return "INVALID RX";

            if (!_gpuWaterfallPipelineEnabled)
                return "CPU FALLBACK: GPU FFT disabled";

            if (_waterfallRenderQuality != WaterfallRenderQuality.High)
                return "CPU FALLBACK: quality is not High";

            if (!_gpuEffectsEnabled)
                return "CPU FALLBACK: GPU target disabled";

            if (!GPUDetector.HasDeviceContext)
                return "CPU FALLBACK: no D3D11 device";

            GPUWaterfallPipeline pipeline = rx == 1 ? _gpuFFT1 : _gpuFFT2;
            if (pipeline == null)
                return "CPU FALLBACK: FFT pipeline not created";
            if (!pipeline.IsInitialized)
                return "CPU FALLBACK: FFT pipeline init failed";

            WaterfallGPURenderer renderer = rx == 1 ? _waterfallGPU1 : _waterfallGPU2;
            if (renderer == null)
                return "GPU FFT READY: renderer not created";
            if (!renderer.IsInitialized)
                return "CPU FALLBACK: renderer init failed";

            if (!_gpuRendererHasData[index])
                return "GPU READY: waiting for GPU row/palette";

            return "GPU ACTIVE: FFT + waterfall renderer";
        }
    }
}
