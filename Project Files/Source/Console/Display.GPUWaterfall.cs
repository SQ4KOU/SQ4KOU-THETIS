using System;

namespace Thetis
{
    partial class Display
    {
        // SQ4KOU: GPU FFT is enabled by default on this feature branch.
        // Any initialization/runtime failure immediately falls back to the existing CPU waterfall.
        private static bool _waterfallUseGPU = true;
        private static int _gpuWaterfallFFTSize = 32768;
        private static readonly int[] _gpuWaterfallConfiguredFFT = new int[2];
        private static readonly bool[] _gpuWaterfallFailed = new bool[2];

        public static bool WaterfallUseGPU
        {
            get { return _waterfallUseGPU; }
            set
            {
                if (_waterfallUseGPU == value) return;
                _waterfallUseGPU = value;
                if (!value) ResetGPUWaterfallState();
            }
        }

        public static int GPUWaterfallFFTSize
        {
            get { return _gpuWaterfallFFTSize; }
            set
            {
                int v = ClampGPUFFTSize(value);
                if (_gpuWaterfallFFTSize == v) return;
                _gpuWaterfallFFTSize = v;
                ResetGPUWaterfallState();
            }
        }

        public static ulong GPUWaterfallDroppedSamplesRX1
        {
            get { return GetGPUWaterfallDroppedSamples(0); }
        }

        public static ulong GPUWaterfallDroppedSamplesRX2
        {
            get { return GetGPUWaterfallDroppedSamples(1); }
        }

        private static int ClampGPUFFTSize(int value)
        {
            if (value < 1024) value = 1024;
            if (value > 262144) value = 262144;
            int p = 1024;
            while (p < value && p < 262144) p <<= 1;
            return p;
        }

        private static ulong GetGPUWaterfallDroppedSamples(int channel)
        {
            try { return GPUWaterfallNative.CM_WaterfallIQ_DroppedSamples(channel); }
            catch { return 0; }
        }

        public static void ResetGPUWaterfallState()
        {
            for (int ch = 0; ch < 2; ch++)
            {
                try { GPUWaterfallNative.CM_GPUWaterfall_Free(ch); }
                catch { }
                _gpuWaterfallConfiguredFFT[ch] = 0;
                _gpuWaterfallFailed[ch] = false;
            }
        }

        private static bool EnsureGPUWaterfall(int channel)
        {
            if (channel < 0 || channel > 1 || _gpuWaterfallFailed[channel]) return false;
            int fftSize = _gpuWaterfallFFTSize;
            if (_gpuWaterfallConfiguredFFT[channel] == fftSize)
            {
                try { return GPUWaterfallNative.CM_GPUWaterfall_IsReady(channel) != 0; }
                catch
                {
                    _gpuWaterfallFailed[channel] = true;
                    return false;
                }
            }

            try
            {
                int ringCapacity = Math.Max(262144, fftSize * 4);
                if (GPUWaterfallNative.CM_GPUWaterfall_Init(channel, fftSize, ringCapacity) != 0)
                {
                    _gpuWaterfallConfiguredFFT[channel] = fftSize;
                    GPUWaterfallNative.CM_WaterfallIQ_ResetDropped(channel);
                    return true;
                }
            }
            catch { }

            _gpuWaterfallFailed[channel] = true;
            _gpuWaterfallConfiguredFFT[channel] = 0;
            return false;
        }

        // Called immediately before the existing CPU waterfall ready/copy block.
        // On success it writes into the exact same SQ4KOU arrays used by the existing renderer.
        // On failure/no-data it changes nothing, so the current CPU path remains authoritative.
        private static void TryUpdateGPUWaterfallRow(int rx, int width, bool localMox)
        {
            if (!_waterfallUseGPU || localMox || width <= 0 || !console.PowerOn) return;
            int channel = rx == 2 ? 1 : 0;
            if (!EnsureGPUWaterfall(channel)) return;

            float[] target = rx == 2 ? new_waterfall_data_bottom : new_waterfall_data;
            if (target == null || target.Length < width) return;

            int sampleRate = rx == 2 ? SampleRateRX2 : SampleRateRX1;
            float lowHz = rx == 2 ? RX2DisplayLow : RXDisplayLow;
            float highHz = rx == 2 ? RX2DisplayHigh : RXDisplayHigh;

            try
            {
                int rc = GPUWaterfallNative.CM_GPUWaterfall_Process(channel, width, sampleRate, lowHz, highHz, target);
                if (rc > 0)
                {
                    if (rx == 2)
                        waterfall_data_ready_bottom = true;
                    else
                        waterfall_data_ready = true;
                }
                else if (rc < 0)
                {
                    _gpuWaterfallFailed[channel] = true;
                    _gpuWaterfallConfiguredFFT[channel] = 0;
                    try { GPUWaterfallNative.CM_GPUWaterfall_Free(channel); } catch { }
                }
            }
            catch
            {
                _gpuWaterfallFailed[channel] = true;
                _gpuWaterfallConfiguredFFT[channel] = 0;
                try { GPUWaterfallNative.CM_GPUWaterfall_Free(channel); } catch { }
            }
        }
    }
}
