using System;
using System.Runtime.InteropServices;

namespace Thetis
{
    partial class Display
    {
        private const int SharpWaterfallFftSize = 16384;
        private const int SharpWaterfallRingCapacity = 524288;
        private static readonly object _sharpWaterfallLock = new object();
        private static readonly bool[] _sharpWaterfallInitialized = new bool[2];
        private static readonly float[][] _sharpWaterfallData = new float[2][];
        private static readonly float[][] _sharpWaterfallCopy = new float[2][];
        private static readonly float[][] _sharpWaterfallMedianScratch = new float[2][];
        private static readonly float[] _sharpWaterfallCalOffset = new float[2];
        private static readonly bool[] _sharpWaterfallCalReady = new bool[2];
        private static long _sharpWaterfallRetryAfterTicks;

        private static class SharpWaterfallNative
        {
            private const string DllName = "ChannelMaster.dll";

            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_GPUWaterfall_Init(int channel, int fftSize, int ringCapacity);

            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_GPUWaterfall_Configure(
                int channel, int windowType, float kaiserBeta,
                int magnitudeMode, int autoOverlap, float overlapPercent,
                int lanczosWindow, int resamplingMode);

            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_GPUWaterfall_Process(
                int channel, int displayWidth, int sampleRate,
                float displayLowHz, float displayHighHz,
                [Out] float[] outputDb);

            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_GPUWaterfall_IsReady(int channel);
        }

        private static bool TryGetSharpWaterfallData(
            int rx,
            int width,
            float[] reference,
            int referenceCount,
            out float[] data,
            out float[] dataCopy)
        {
            data = null;
            dataCopy = null;

            if (console == null || !console.PowerOn || width < 64 || width > BUFFER_SIZE)
                return false;

            int slot = rx - 1;
            if (slot < 0 || slot > 1)
                return false;

            long now = DateTime.UtcNow.Ticks;
            if (now < _sharpWaterfallRetryAfterTicks)
                return false;

            lock (_sharpWaterfallLock)
            {
                try
                {
                    if (!_sharpWaterfallInitialized[slot] ||
                        SharpWaterfallNative.CM_GPUWaterfall_IsReady(slot) == 0)
                    {
                        if (SharpWaterfallNative.CM_GPUWaterfall_Init(
                            slot, SharpWaterfallFftSize, SharpWaterfallRingCapacity) == 0)
                            return false;

                        // 4=Nuttall, magnitudeMode=2 (PeakHoldPower contract),
                        // fixed 85% overlap, Lanczos-3, quality/peak resampling=3.
                        if (SharpWaterfallNative.CM_GPUWaterfall_Configure(
                            slot, 4, 6.0f, 2, 0, 85.0f, 3, 3) == 0)
                            return false;

                        _sharpWaterfallInitialized[slot] = true;
                        _sharpWaterfallCalReady[slot] = false;
                        _sharpWaterfallCalOffset[slot] = 0.0f;
                    }

                    if (_sharpWaterfallData[slot] == null || _sharpWaterfallData[slot].Length != width)
                    {
                        _sharpWaterfallData[slot] = new float[width];
                        _sharpWaterfallCopy[slot] = new float[width];
                        _sharpWaterfallMedianScratch[slot] = new float[Math.Max(width, referenceCount)];
                        _sharpWaterfallCalReady[slot] = false;
                    }

                    int sampleRate = cmaster.GetInputRate(0, slot);
                    if (sampleRate <= 0)
                        sampleRate = rx == 1 ? SampleRateRX1 : SampleRateRX2;
                    if (sampleRate <= 0)
                        sampleRate = 192000;

                    float lowHz = rx == 1 ? RXDisplayLow : RX2DisplayLow;
                    float highHz = rx == 1 ? RXDisplayHigh : RX2DisplayHigh;

                    int result = SharpWaterfallNative.CM_GPUWaterfall_Process(
                        slot, width, sampleRate, lowHz, highHz, _sharpWaterfallData[slot]);

                    if (result != 1)
                        return false;

                    // Match the recovered GPU row to the existing WDSP dB scale.
                    // Median calibration only adjusts a constant offset; it does not
                    // smooth or reshape the GPU spectrum.
                    if (reference != null && referenceCount > 8)
                    {
                        float cpuMedian = Median(reference, referenceCount, slot);
                        float gpuMedian = Median(_sharpWaterfallData[slot], width, slot);
                        float targetOffset = cpuMedian - gpuMedian;

                        if (!float.IsNaN(targetOffset) && !float.IsInfinity(targetOffset))
                        {
                            if (targetOffset > 120f) targetOffset = 120f;
                            if (targetOffset < -120f) targetOffset = -120f;

                            if (!_sharpWaterfallCalReady[slot])
                            {
                                _sharpWaterfallCalOffset[slot] = targetOffset;
                                _sharpWaterfallCalReady[slot] = true;
                            }
                            else
                            {
                                float old = _sharpWaterfallCalOffset[slot];
                                float next = old * 0.90f + targetOffset * 0.10f;
                                if (next > old + 0.5f) next = old + 0.5f;
                                if (next < old - 0.5f) next = old - 0.5f;
                                _sharpWaterfallCalOffset[slot] = next;
                            }
                        }
                    }

                    float offset = _sharpWaterfallCalOffset[slot];
                    for (int i = 0; i < width; i++)
                    {
                        float v = _sharpWaterfallData[slot][i] + offset;
                        _sharpWaterfallData[slot][i] = v;
                        _sharpWaterfallCopy[slot][i] = v;
                    }

                    data = _sharpWaterfallData[slot];
                    dataCopy = _sharpWaterfallCopy[slot];
                    return true;
                }
                catch (Exception ex)
                {
                    Common.LogString("Sharp waterfall fallback: " + ex.Message);
                    _sharpWaterfallInitialized[slot] = false;
                    _sharpWaterfallCalReady[slot] = false;
                    _sharpWaterfallRetryAfterTicks = DateTime.UtcNow.AddSeconds(2).Ticks;
                    return false;
                }
            }
        }

        private static float Median(float[] source, int count, int slot)
        {
            if (source == null || count <= 0)
                return -200.0f;

            int n = Math.Min(count, source.Length);
            float[] scratch = _sharpWaterfallMedianScratch[slot];
            if (scratch == null || scratch.Length < n)
            {
                scratch = new float[n];
                _sharpWaterfallMedianScratch[slot] = scratch;
            }

            Array.Copy(source, scratch, n);
            Array.Sort(scratch, 0, n);
            int mid = n >> 1;
            return (n & 1) != 0
                ? scratch[mid]
                : (scratch[mid - 1] + scratch[mid]) * 0.5f;
        }
    }
}
