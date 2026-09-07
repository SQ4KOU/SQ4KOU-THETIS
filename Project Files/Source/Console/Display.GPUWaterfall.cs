using System;

namespace Thetis
{
    partial class Display
    {
        // SQ4KOU GPU Waterfall V2.
        // GPU FFT remains optional and any failure falls back to the existing CPU/WDSP path.
        private static bool _waterfallUseGPU = true;
        private static int _gpuWaterfallFFTSize = 32768;
        private static readonly int[] _gpuWaterfallConfiguredFFT = new int[2];
        private static readonly bool[] _gpuWaterfallFailed = new bool[2];

        // V2 post-processing state.
        private static readonly float[][] _gpuRawRow = new float[2][];
        private static readonly float[][] _gpuPrevRow = new float[2][];
        private static readonly float[][] _gpuCpuReference = new float[2][];
        private static readonly float[][] _gpuCalibrationScratch = new float[2][];
        private static readonly bool[] _gpuPrevValid = new bool[2];
        private static readonly bool[] _gpuCalibrationValid = new bool[2];
        private static readonly float[] _gpuCalibrationOffset = new float[2];
        private static readonly int[] _gpuCalibrationFrame = new int[2];

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

        public static float GPUWaterfallCalibrationOffsetRX1
        {
            get { return _gpuCalibrationOffset[0]; }
        }

        public static float GPUWaterfallCalibrationOffsetRX2
        {
            get { return _gpuCalibrationOffset[1]; }
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

        private static void EnsureRowBuffer(ref float[] buffer, int width)
        {
            if (buffer == null || buffer.Length < width)
                buffer = new float[width];
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static void ResetGPUWaterfallState()
        {
            for (int ch = 0; ch < 2; ch++)
            {
                try { GPUWaterfallNative.CM_GPUWaterfall_Free(ch); }
                catch { }

                _gpuWaterfallConfiguredFFT[ch] = 0;
                _gpuWaterfallFailed[ch] = false;
                _gpuPrevValid[ch] = false;
                _gpuCalibrationValid[ch] = false;
                _gpuCalibrationOffset[ch] = 0.0f;
                _gpuCalibrationFrame[ch] = 0;
                _gpuRawRow[ch] = null;
                _gpuPrevRow[ch] = null;
                _gpuCpuReference[ch] = null;
                _gpuCalibrationScratch[ch] = null;
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
                int ringCapacity = Math.Max(1048576, fftSize * 4);
                if (GPUWaterfallNative.CM_GPUWaterfall_Init(channel, fftSize, ringCapacity) != 0)
                {
                    _gpuWaterfallConfiguredFFT[channel] = fftSize;
                    _gpuPrevValid[channel] = false;
                    _gpuCalibrationValid[channel] = false;
                    _gpuCalibrationOffset[channel] = 0.0f;
                    _gpuCalibrationFrame[channel] = 0;
                    GPUWaterfallNative.CM_WaterfallIQ_ResetDropped(channel);
                    return true;
                }
            }
            catch { }

            _gpuWaterfallFailed[channel] = true;
            _gpuWaterfallConfiguredFFT[channel] = 0;
            return false;
        }

        // Robust A/B level calibration. We compare the GPU row with the currently available
        // CPU/WDSP row and use the median delta, then a slow EMA. Median makes the calibration
        // insensitive to strong stations and local spectral peaks.
        private static void UpdateGPUCalibration(int channel, float[] cpu, float[] gpu, int width)
        {
            _gpuCalibrationFrame[channel]++;
            if ((_gpuCalibrationFrame[channel] & 7) != 0) return; // every 8th GPU row

            EnsureRowBuffer(ref _gpuCalibrationScratch[channel], width);
            float[] scratch = _gpuCalibrationScratch[channel];
            int count = 0;

            for (int i = 0; i < width; i++)
            {
                float c = cpu[i];
                float g = gpu[i];
                if (!IsFinite(c) || !IsFinite(g)) continue;
                if (c < -220.0f || c > 60.0f || g < -260.0f || g > 80.0f) continue;

                float d = c - g;
                if (d < -80.0f || d > 80.0f) continue;
                scratch[count++] = d;
            }

            if (count < Math.Max(32, width / 8)) return;

            Array.Sort(scratch, 0, count);
            float median;
            if ((count & 1) != 0)
                median = scratch[count / 2];
            else
                median = 0.5f * (scratch[count / 2 - 1] + scratch[count / 2]);

            if (median < -60.0f) median = -60.0f;
            if (median > 60.0f) median = 60.0f;

            if (!_gpuCalibrationValid[channel])
            {
                _gpuCalibrationOffset[channel] = median;
                _gpuCalibrationValid[channel] = true;
            }
            else
            {
                // Slow enough not to pump with signals, fast enough to settle after a rate/span change.
                _gpuCalibrationOffset[channel] += 0.08f * (median - _gpuCalibrationOffset[channel]);
            }
        }

        // Adaptive temporal filter. Quiet noise is strongly stabilised, while a real signal
        // appearing/disappearing is followed quickly so CW/SSB edges are not smeared.
        private static void PostProcessGPUWaterfallRow(int channel, float[] raw, float[] target, int width)
        {
            EnsureRowBuffer(ref _gpuPrevRow[channel], width);
            float[] prev = _gpuPrevRow[channel];
            float offset = _gpuCalibrationValid[channel] ? _gpuCalibrationOffset[channel] : 0.0f;

            if (!_gpuPrevValid[channel])
            {
                for (int i = 0; i < width; i++)
                {
                    float v = raw[i] + offset;
                    target[i] = v;
                    prev[i] = v;
                }
                _gpuPrevValid[channel] = true;
                return;
            }

            for (int i = 0; i < width; i++)
            {
                float current = raw[i] + offset;
                float old = prev[i];
                float delta = Math.Abs(current - old);

                float alpha;
                if (delta >= 12.0f)
                    alpha = 0.90f;
                else if (delta >= 6.0f)
                    alpha = 0.70f;
                else if (delta >= 2.5f)
                    alpha = 0.45f;
                else
                    alpha = 0.28f;

                float filtered = old + alpha * (current - old);
                prev[i] = filtered;
                target[i] = filtered;
            }
        }

        // Called immediately before the unchanged SQ4KOU CPU waterfall ready/copy block.
        // V2 keeps a separate GPU row, optionally calibrates it against the CPU/WDSP row,
        // post-processes it, then publishes it through the existing SQ4KOU waterfall arrays.
        private static void TryUpdateGPUWaterfallRow(int rx, int width, bool localMox)
        {
            if (!_waterfallUseGPU || localMox || width <= 0 || !console.PowerOn) return;

            int channel = rx == 2 ? 1 : 0;
            if (!EnsureGPUWaterfall(channel)) return;

            float[] target = rx == 2 ? new_waterfall_data_bottom : new_waterfall_data;
            if (target == null || target.Length < width) return;

            bool cpuReady = rx == 2 ? waterfall_data_ready_bottom : waterfall_data_ready;
            if (cpuReady)
            {
                EnsureRowBuffer(ref _gpuCpuReference[channel], width);
                Array.Copy(target, _gpuCpuReference[channel], width);
            }

            EnsureRowBuffer(ref _gpuRawRow[channel], width);
            float[] raw = _gpuRawRow[channel];

            int sampleRate = rx == 2 ? SampleRateRX2 : SampleRateRX1;
            float lowHz = rx == 2 ? RX2DisplayLow : RXDisplayLow;
            float highHz = rx == 2 ? RX2DisplayHigh : RXDisplayHigh;

            try
            {
                int rc = GPUWaterfallNative.CM_GPUWaterfall_Process(channel, width, sampleRate, lowHz, highHz, raw);
                if (rc > 0)
                {
                    if (cpuReady)
                        UpdateGPUCalibration(channel, _gpuCpuReference[channel], raw, width);

                    PostProcessGPUWaterfallRow(channel, raw, target, width);

                    if (rx == 2)
                        waterfall_data_ready_bottom = true;
                    else
                        waterfall_data_ready = true;
                }
                else if (rc < 0)
                {
                    _gpuWaterfallFailed[channel] = true;
                    _gpuWaterfallConfiguredFFT[channel] = 0;
                    _gpuPrevValid[channel] = false;
                    _gpuCalibrationValid[channel] = false;
                    try { GPUWaterfallNative.CM_GPUWaterfall_Free(channel); } catch { }
                }
            }
            catch
            {
                _gpuWaterfallFailed[channel] = true;
                _gpuWaterfallConfiguredFFT[channel] = 0;
                _gpuPrevValid[channel] = false;
                _gpuCalibrationValid[channel] = false;
                try { GPUWaterfallNative.CM_GPUWaterfall_Free(channel); } catch { }
            }
        }
    }
}
