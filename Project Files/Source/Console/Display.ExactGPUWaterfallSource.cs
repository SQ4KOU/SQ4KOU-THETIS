using System;
using System.Runtime.InteropServices;

namespace Thetis
{
    partial class Display
    {
        private const int ExactGpuFftSize = 16384;
        private const int ExactGpuIqCapacity = 524288;
        private const int ExactGpuOverlapPercent = 85;

        private static readonly object _exactGpuLock = new object();
        private static readonly bool[] _exactGpuInit = new bool[2];
        private static readonly bool[] _exactGpuIqInit = new bool[2];
        private static readonly float[][] _exactRingI = new float[2][];
        private static readonly float[][] _exactRingQ = new float[2][];
        private static readonly int[] _exactRingHead = new int[2];
        private static readonly int[] _exactRingCount = new int[2];
        private static readonly int[] _exactSampleCredit = new int[2];
        private static readonly bool[] _exactFirstFill = new bool[2];
        private static readonly float[][] _exactReadI = new float[2][];
        private static readonly float[][] _exactReadQ = new float[2][];
        private static readonly float[][] _exactFrameI = new float[2][];
        private static readonly float[][] _exactFrameQ = new float[2][];
        private static readonly float[][] _exactRow = new float[2][];
        private static readonly float[][] _exactRowCopy = new float[2][];
        private static readonly byte[][] _exactColorRow = new byte[2][];
        private static readonly float[][] _exactMedianScratch = new float[2][];
        private static readonly float[] _exactCalOffset = new float[2];
        private static readonly bool[] _exactCalInit = new bool[2];
        private static bool _exactGpuUnavailable;

        private static class ExactGpuNative
        {
            private const string DllName = "ChannelMaster.dll";

            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_WaterfallIQ_Init(int channel, int requestedSamples);
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void CM_WaterfallIQ_SetEnabled(int channel, int enabled);
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_WaterfallIQ_Available(int channel);
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_WaterfallIQ_Get(int channel, int requestedSamples, [Out] float[] iOut, [Out] float[] qOut);

            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_GPUWaterfallExact_Init(int channel, int fftSize, int displayWidth);
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_GPUWaterfallExact_Configure(
                int channel, int windowType, float kaiserBeta, int magnitudeMode, int lanczosWindow, int resamplingMode);
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_GPUWaterfallExact_Process(
                int channel, int sampleRate, float displayLowHz, float displayHighHz,
                float[] iData, float[] qData, int count, [Out] float[] outputDb);

            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int CM_GPUWaterfallExact_RenderRow(
                int channel, float lowThreshold, float highThreshold,
                [Out] byte[] outputBGRA, int outputBytes);
        }

        // 1 = exact GPU row ready; 0 = healthy source waiting for overlap hop; -1 = source unavailable.
        private static int TryGetExactGpuWaterfallRow(
            int rx, int width, float[] reference, int referenceCount,
            float lowThreshold, float highThreshold, float fOffset,
            out byte[] colorRow)
        {
            colorRow = null;
            if (_exactGpuUnavailable || console == null || !console.PowerOn || width <= 0 || width > 8192)
                return -1;

            int slot = rx - 1;
            if (slot < 0 || slot > 1) return -1;

            lock (_exactGpuLock)
            {
                try
                {
                    if (!_exactGpuIqInit[slot])
                    {
                        if (ExactGpuNative.CM_WaterfallIQ_Init(slot, ExactGpuIqCapacity) <= 0)
                            return -1;
                        ExactGpuNative.CM_WaterfallIQ_SetEnabled(slot, 1);
                        _exactGpuIqInit[slot] = true;
                    }

                    if (!_exactGpuInit[slot] || _exactRow[slot] == null || _exactRow[slot].Length != width)
                    {
                        if (ExactGpuNative.CM_GPUWaterfallExact_Init(slot, ExactGpuFftSize, width) == 0)
                        {
                            _exactGpuUnavailable = true;
                            return -1;
                        }

                        // ff62e7ad defaults: Nuttall, beta 6, PeakHoldPower, Lanczos 3, Quality.
                        if (ExactGpuNative.CM_GPUWaterfallExact_Configure(slot, 4, 6.0f, 2, 3, 1) == 0)
                        {
                            _exactGpuUnavailable = true;
                            return -1;
                        }

                        _exactGpuInit[slot] = true;
                        _exactRingI[slot] = new float[ExactGpuFftSize];
                        _exactRingQ[slot] = new float[ExactGpuFftSize];
                        _exactReadI[slot] = new float[ExactGpuFftSize * 2];
                        _exactReadQ[slot] = new float[ExactGpuFftSize * 2];
                        _exactFrameI[slot] = new float[ExactGpuFftSize];
                        _exactFrameQ[slot] = new float[ExactGpuFftSize];
                        _exactRow[slot] = new float[width];
                        _exactRowCopy[slot] = new float[width];
                        _exactColorRow[slot] = new byte[width * 4];
                        _exactMedianScratch[slot] = new float[Math.Max(width, referenceCount)];
                        _exactRingHead[slot] = 0;
                        _exactRingCount[slot] = 0;
                        _exactSampleCredit[slot] = 0;
                        _exactFirstFill[slot] = false;
                        _exactCalInit[slot] = false;
                        _exactCalOffset[slot] = 0f;
                    }

                    int available = ExactGpuNative.CM_WaterfallIQ_Available(slot);
                    if (available > 0)
                    {
                        int request = Math.Min(available, ExactGpuFftSize * 2);
                        int got = ExactGpuNative.CM_WaterfallIQ_Get(slot, request, _exactReadI[slot], _exactReadQ[slot]);
                        if (got > 0)
                        {
                            int head = _exactRingHead[slot];
                            int count = _exactRingCount[slot];

                            // Exact ff62e7ad ReadWaterfallIQ convention: swap I and Q before FFT.
                            for (int i = 0; i < got; i++)
                            {
                                _exactRingI[slot][head] = _exactReadQ[slot][i];
                                _exactRingQ[slot][head] = _exactReadI[slot][i];
                                head = (head + 1) % ExactGpuFftSize;
                            }
                            count = Math.Min(count + got, ExactGpuFftSize);
                            _exactRingHead[slot] = head;
                            _exactRingCount[slot] = count;
                            _exactSampleCredit[slot] += got;
                        }
                    }

                    if (_exactRingCount[slot] < ExactGpuFftSize)
                        return 0;

                    int hop = Math.Max(1, Math.Min(ExactGpuFftSize,
                        (int)Math.Round(ExactGpuFftSize * (1.0 - ExactGpuOverlapPercent / 100.0))));

                    if (!_exactFirstFill[slot])
                    {
                        _exactFirstFill[slot] = true;
                        _exactSampleCredit[slot] = 0;
                    }
                    else
                    {
                        if (_exactSampleCredit[slot] < hop)
                            return 0;
                        _exactSampleCredit[slot] -= hop;
                    }

                    int maxCredit = hop * 2;
                    if (_exactSampleCredit[slot] > maxCredit)
                        _exactSampleCredit[slot] = maxCredit;

                    int ringHead = _exactRingHead[slot];
                    for (int i = 0; i < ExactGpuFftSize; i++)
                    {
                        int src = (ringHead + i) % ExactGpuFftSize;
                        _exactFrameI[slot][i] = _exactRingI[slot][src];
                        _exactFrameQ[slot][i] = _exactRingQ[slot][src];
                    }

                    int sampleRate = cmaster.GetInputRate(0, slot);
                    if (sampleRate <= 0)
                        sampleRate = rx == 1 ? SampleRateRX1 : SampleRateRX2;
                    if (sampleRate <= 0) sampleRate = 192000;

                    float lowHz = rx == 1 ? RXDisplayLow : RX2DisplayLow;
                    float highHz = rx == 1 ? RXDisplayHigh : RX2DisplayHigh;

                    int result = ExactGpuNative.CM_GPUWaterfallExact_Process(
                        slot, sampleRate, lowHz, highHz,
                        _exactFrameI[slot], _exactFrameQ[slot], ExactGpuFftSize, _exactRow[slot]);

                    if (result != 1)
                    {
                        _exactGpuUnavailable = true;
                        return -1;
                    }

                    // Same calibration concept as ff62e7ad: only a constant dB offset.
                    int refCount = Math.Min(referenceCount, reference == null ? 0 : reference.Length);
                    if (refCount > 8)
                    {
                        float cpuMedian = ExactMedian(reference, refCount, slot);
                        float gpuMedian = ExactMedian(_exactRow[slot], width, slot);
                        float target = cpuMedian - gpuMedian;
                        if (!float.IsNaN(target) && !float.IsInfinity(target) && target >= -12f && target <= 7f)
                        {
                            if (!_exactCalInit[slot])
                            {
                                _exactCalOffset[slot] = target;
                                _exactCalInit[slot] = true;
                            }
                            else
                            {
                                float old = _exactCalOffset[slot];
                                float next = old * 0.9f + target * 0.1f;
                                if (next > old + 1f) next = old + 1f;
                                if (next < old - 1f) next = old - 1f;
                                _exactCalOffset[slot] = next;
                            }
                        }
                    }

                    float offset = _exactCalOffset[slot];
                    for (int i = 0; i < width; i++)
                    {
                        float v = _exactRow[slot][i] + offset;
                        _exactRow[slot][i] = v;
                        _exactRowCopy[slot][i] = v;
                    }

                    int colorResult = ExactGpuNative.CM_GPUWaterfallExact_RenderRow(
                        slot,
                        lowThreshold - offset - fOffset,
                        highThreshold - offset - fOffset,
                        _exactColorRow[slot],
                        _exactColorRow[slot].Length);

                    if (colorResult != 1)
                    {
                        _exactGpuUnavailable = true;
                        return -1;
                    }

                    colorRow = _exactColorRow[slot];
                    return 1;
                }
                catch (Exception ex)
                {
                    Common.LogString("Exact GPU waterfall disabled: " + ex.Message);
                    _exactGpuUnavailable = true;
                    return -1;
                }
            }
        }

        private static float ExactMedian(float[] source, int count, int slot)
        {
            if (source == null || count <= 0) return -200f;
            int n = Math.Min(count, source.Length);
            float[] scratch = _exactMedianScratch[slot];
            if (scratch == null || scratch.Length < n)
            {
                scratch = new float[n];
                _exactMedianScratch[slot] = scratch;
            }
            Array.Copy(source, scratch, n);
            Array.Sort(scratch, 0, n);
            int mid = n / 2;
            return (n & 1) != 0 ? scratch[mid] : (scratch[mid - 1] + scratch[mid]) * 0.5f;
        }
    }
}
