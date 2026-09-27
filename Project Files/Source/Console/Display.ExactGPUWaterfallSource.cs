using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Thetis
{
    partial class Display
    {
        private const int ExactGpuFftSize = 16384;
        private const int ExactGpuIqCapacity = 524288;
        private const int ExactGpuWorkerStaleMs = 750;
        private const int ExactGpuWorkerHungMs = 2000;

        // IMPORTANT ARCHITECTURE RULE:
        // Native D3D11 FFT work never runs on the render thread.  The native path
        // performs staging-resource Map(READ), which is a synchronous GPU wait.
        // Doing that inside RenderDX2D / D2D BeginDraw-EndDraw can stall the shared
        // renderer even though the FFT uses a second D3D11 device.  The worker below
        // owns every native GPU FFT call; RenderDX2D only consumes completed CPU rows.
        private sealed class ExactWorkerRequest
        {
            public int Rx;
            public int Width;
            public int FftSize;
            public int SampleRate;
            public int Window;
            public float Kaiser;
            public int Magnitude;
            public int Lanczos;
            public int Resampling;
            public int OverlapPercent;
            public bool AutoOverlap;
            public double Fps;
            public int UpdatePeriod;
            public float LowHz;
            public float HighHz;
            public long RequestSeq;
        }

        private sealed class ExactWorkerResult
        {
            public float[] Row;
            public int Width;
            public int FftSize;
            public double EffectiveOverlap;
            public long ResultSeq;
            public long ProducedTicks;
        }

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
        private static readonly float[][] _exactNativeRow = new float[2][];

        // Render-thread-only calibration/result buffers.
        private static readonly float[][] _exactRowCopy = new float[2][];
        private static readonly float[][] _exactMedianScratch = new float[2][];
        private static readonly float[] _exactCalOffset = new float[2];
        private static readonly bool[] _exactCalInit = new bool[2];

        private static readonly int[] _exactConfiguredWindow = new int[2] { -1, -1 };
        private static readonly float[] _exactConfiguredKaiser = new float[2] { float.NaN, float.NaN };
        private static readonly int[] _exactConfiguredMagnitude = new int[2] { -1, -1 };
        private static readonly int[] _exactConfiguredLanczos = new int[2] { -1, -1 };
        private static readonly int[] _exactConfiguredResampling = new int[2] { -1, -1 };

        private static readonly ExactWorkerRequest[] _exactWorkerRequest = new ExactWorkerRequest[2];
        private static readonly ExactWorkerResult[] _exactWorkerResult = new ExactWorkerResult[2];
        private static readonly long[] _exactRequestSeq = new long[2];
        private static readonly long[] _exactResultSeq = new long[2];
        private static readonly long[] _exactConsumedResultSeq = new long[2];
        private static readonly int[] _exactDesiredEnabled = new int[2];
        private static readonly int[] _exactResetEpoch = new int[2];
        private static readonly int[] _exactWorkerResetSeen = new int[2] { -1, -1 };

        private static readonly AutoResetEvent _exactWorkerWake = new AutoResetEvent(false);
        private static Thread _exactWorkerThread;
        private static int _exactWorkerStarted;
        private static int _exactWorkerBusy;
        private static long _exactWorkerBusySinceTicks;
        private static long _exactWorkerLastProgressTicks;
        private static int _exactWorkerDeclaredHung;

        // SDR-VST3 stability path: GPU FFT runs on a native dedicated D3D11 device,
        // while visible waterfall history remains on the normal Vortice/D2D bitmap.
        private static bool ExactNativeGPURequested =>
            _gpuWaterfallPipelineEnabled &&
            _waterfallRenderQuality == WaterfallRenderQuality.High &&
            !m_bForceCPURendering && m_eRenderPath == DXRenderPath.Hardware;

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
            internal static extern void CM_GPUWaterfallExact_Free(int channel);
        }

        private static void EnsureExactGPUWorker()
        {
            if (Interlocked.CompareExchange(ref _exactWorkerStarted, 1, 0) != 0)
                return;

            Interlocked.Exchange(ref _exactWorkerLastProgressTicks, Stopwatch.GetTimestamp());
            _exactWorkerThread = new Thread(ExactGPUWorkerLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "ExactGPUWaterfall"
            };
            _exactWorkerThread.Start();
            GPUWaterfallLogger.Log("WF-WORKER", "native GPU waterfall worker started");
        }

        private static void ResetExactGPUWaterfallSourceForModeChange(bool enableIQ)
        {
            for (int slot = 0; slot < 2; slot++)
            {
                Interlocked.Exchange(ref _exactDesiredEnabled[slot], enableIQ ? 1 : 0);
                Interlocked.Increment(ref _exactResetEpoch[slot]);
                Interlocked.Exchange(ref _exactWorkerResult[slot], null);
                Interlocked.Exchange(ref _exactConsumedResultSeq[slot], 0);

                _exactCalInit[slot] = false;
                _exactCalOffset[slot] = 0f;
                _gpuLastEffectiveOverlap[slot] = -1.0;
            }

            EnsureExactGPUWorker();
            _exactWorkerWake.Set();

            GPUWaterfallLogger.Log("WF-SOURCE",
                "exact source reset requested enableIQ=" + enableIQ +
                " forceCPU=" + m_bForceCPURendering +
                " pipeline=" + _gpuWaterfallPipelineEnabled);
        }

        private static void ResetExactWorkerSlot(int slot, bool enable)
        {
            _exactRingHead[slot] = 0;
            _exactRingCount[slot] = 0;
            _exactSampleCredit[slot] = 0;
            _exactFirstFill[slot] = false;

            if (_exactGpuIqInit[slot])
            {
                try { ExactGpuNative.CM_WaterfallIQ_SetEnabled(slot, enable ? 1 : 0); }
                catch { }
            }

            Interlocked.Exchange(ref _exactWorkerResult[slot], null);
        }

        private static int CalculateExactGPUWaterfallHopWorker(ExactWorkerRequest request, out double effectiveOverlap)
        {
            int overlap = request.OverlapPercent;
            if (overlap < 0) overlap = 0;
            if (overlap > 95) overlap = 95;

            int hop = Math.Max(1, Math.Min(request.FftSize,
                (int)Math.Round(request.FftSize * (1.0 - overlap / 100.0))));

            if (request.AutoOverlap)
            {
                double fps = Math.Max(1.0, request.Fps);
                int updatePeriod = Math.Max(1, request.UpdatePeriod);
                double rowsPerSecond = Math.Min(fps / updatePeriod, 30.0);

                double maxRowsAt95Percent = request.SampleRate / (0.05 * request.FftSize);
                if (rowsPerSecond > maxRowsAt95Percent)
                    rowsPerSecond = maxRowsAt95Percent;
                if (rowsPerSecond < 1.0)
                    rowsPerSecond = 1.0;

                hop = Math.Max(1, Math.Min(request.FftSize,
                    (int)Math.Round(request.SampleRate / rowsPerSecond)));
            }

            effectiveOverlap = 1.0 - (double)hop / request.FftSize;
            return hop;
        }

        private static void ExactGPUWorkerLoop()
        {
            while (true)
            {
                try
                {
                    _exactWorkerWake.WaitOne(5);
                    Interlocked.Exchange(ref _exactWorkerLastProgressTicks, Stopwatch.GetTimestamp());

                    for (int slot = 0; slot < 2; slot++)
                    {
                        int epoch = Volatile.Read(ref _exactResetEpoch[slot]);
                        bool enable = Volatile.Read(ref _exactDesiredEnabled[slot]) != 0;

                        if (_exactWorkerResetSeen[slot] != epoch)
                        {
                            _exactWorkerResetSeen[slot] = epoch;
                            ResetExactWorkerSlot(slot, enable);
                        }

                        if (!enable)
                            continue;

                        ExactWorkerRequest request = Interlocked.CompareExchange(ref _exactWorkerRequest[slot], null, null);
                        if (request == null)
                            continue;

                        ProcessExactGPUWorkerSlot(slot, request);
                        Interlocked.Exchange(ref _exactWorkerLastProgressTicks, Stopwatch.GetTimestamp());
                    }
                }
                catch (Exception ex)
                {
                    GPUWaterfallLogger.LogRateLimited("WF-WORKER", "loop-ex", 2000, ex.ToString());
                    Thread.Sleep(10);
                }
            }
        }

        private static void ProcessExactGPUWorkerSlot(int slot, ExactWorkerRequest request)
        {
            try
            {
                if (!_exactGpuIqInit[slot])
                {
                    if (ExactGpuNative.CM_WaterfallIQ_Init(slot, ExactGpuIqCapacity) <= 0)
                    {
                        GPUWaterfallLogger.LogRateLimited("WF-WORKER", "iq-init-" + slot, 2000,
                            "RX" + (slot + 1) + " IQ init failed");
                        return;
                    }
                    ExactGpuNative.CM_WaterfallIQ_SetEnabled(slot, 1);
                    _exactGpuIqInit[slot] = true;
                }

                bool sizeChanged = !_exactGpuInit[slot] ||
                    _exactNativeRow[slot] == null || _exactNativeRow[slot].Length != request.Width ||
                    _exactFrameI[slot] == null || _exactFrameI[slot].Length != request.FftSize;

                if (sizeChanged)
                {
                    Interlocked.Exchange(ref _exactWorkerBusy, 1);
                    Interlocked.Exchange(ref _exactWorkerBusySinceTicks, Stopwatch.GetTimestamp());
                    int initResult = ExactGpuNative.CM_GPUWaterfallExact_Init(slot, request.FftSize, request.Width);
                    Interlocked.Exchange(ref _exactWorkerBusy, 0);

                    if (initResult == 0)
                    {
                        GPUWaterfallLogger.LogRateLimited("WF-WORKER", "init-" + slot, 2000,
                            "RX" + (slot + 1) + " exact init FAILED fft=" + request.FftSize + " width=" + request.Width);
                        return;
                    }

                    _exactGpuInit[slot] = true;
                    _exactRingI[slot] = new float[request.FftSize];
                    _exactRingQ[slot] = new float[request.FftSize];
                    _exactReadI[slot] = new float[request.FftSize * 2];
                    _exactReadQ[slot] = new float[request.FftSize * 2];
                    _exactFrameI[slot] = new float[request.FftSize];
                    _exactFrameQ[slot] = new float[request.FftSize];
                    _exactNativeRow[slot] = new float[request.Width];
                    _exactRingHead[slot] = 0;
                    _exactRingCount[slot] = 0;
                    _exactSampleCredit[slot] = 0;
                    _exactFirstFill[slot] = false;
                    _exactConfiguredWindow[slot] = -1;

                    GPUWaterfallLogger.Log("WF-WORKER",
                        "RX" + (slot + 1) + " exact init fft=" + request.FftSize + " width=" + request.Width);
                }

                if (_exactConfiguredWindow[slot] != request.Window ||
                    _exactConfiguredKaiser[slot] != request.Kaiser ||
                    _exactConfiguredMagnitude[slot] != request.Magnitude ||
                    _exactConfiguredLanczos[slot] != request.Lanczos ||
                    _exactConfiguredResampling[slot] != request.Resampling)
                {
                    Interlocked.Exchange(ref _exactWorkerBusy, 1);
                    Interlocked.Exchange(ref _exactWorkerBusySinceTicks, Stopwatch.GetTimestamp());
                    int configureResult = ExactGpuNative.CM_GPUWaterfallExact_Configure(
                        slot, request.Window, request.Kaiser, request.Magnitude, request.Lanczos, request.Resampling);
                    Interlocked.Exchange(ref _exactWorkerBusy, 0);

                    if (configureResult == 0)
                    {
                        GPUWaterfallLogger.LogRateLimited("WF-WORKER", "cfg-" + slot, 2000,
                            "RX" + (slot + 1) + " configure FAILED");
                        return;
                    }

                    _exactConfiguredWindow[slot] = request.Window;
                    _exactConfiguredKaiser[slot] = request.Kaiser;
                    _exactConfiguredMagnitude[slot] = request.Magnitude;
                    _exactConfiguredLanczos[slot] = request.Lanczos;
                    _exactConfiguredResampling[slot] = request.Resampling;
                }

                int available = ExactGpuNative.CM_WaterfallIQ_Available(slot);
                if (available > 0)
                {
                    int requestSamples = Math.Min(available, request.FftSize * 2);
                    int got = ExactGpuNative.CM_WaterfallIQ_Get(
                        slot, requestSamples, _exactReadI[slot], _exactReadQ[slot]);
                    if (got > 0)
                    {
                        int head = _exactRingHead[slot];
                        int count = _exactRingCount[slot];
                        for (int i = 0; i < got; i++)
                        {
                            // ff62e7ad convention: swap I/Q before FFT.
                            _exactRingI[slot][head] = _exactReadQ[slot][i];
                            _exactRingQ[slot][head] = _exactReadI[slot][i];
                            head = (head + 1) % request.FftSize;
                        }
                        count = Math.Min(count + got, request.FftSize);
                        _exactRingHead[slot] = head;
                        _exactRingCount[slot] = count;
                        _exactSampleCredit[slot] += got;
                    }
                }

                if (_exactRingCount[slot] < request.FftSize)
                {
                    GPUWaterfallLogger.LogRateLimited("WF-WORKER", "fill-" + slot, 1000,
                        "RX" + (slot + 1) + " filling ring count=" + _exactRingCount[slot] + "/" + request.FftSize);
                    return;
                }

                int hop = CalculateExactGPUWaterfallHopWorker(request, out double effectiveOverlap);
                if (!_exactFirstFill[slot])
                {
                    _exactFirstFill[slot] = true;
                    _exactSampleCredit[slot] = 0;
                }
                else
                {
                    if (_exactSampleCredit[slot] < hop)
                        return;
                    _exactSampleCredit[slot] -= hop;
                }

                int maxCredit = hop * 2;
                if (_exactSampleCredit[slot] > maxCredit)
                    _exactSampleCredit[slot] = maxCredit;

                int ringHead = _exactRingHead[slot];
                for (int i = 0; i < request.FftSize; i++)
                {
                    int src = (ringHead + i) % request.FftSize;
                    _exactFrameI[slot][i] = _exactRingI[slot][src];
                    _exactFrameQ[slot][i] = _exactRingQ[slot][src];
                }

                Interlocked.Exchange(ref _exactWorkerBusy, 1);
                Interlocked.Exchange(ref _exactWorkerBusySinceTicks, Stopwatch.GetTimestamp());
                int result = ExactGpuNative.CM_GPUWaterfallExact_Process(
                    slot, request.SampleRate, request.LowHz, request.HighHz,
                    _exactFrameI[slot], _exactFrameQ[slot], request.FftSize, _exactNativeRow[slot]);
                Interlocked.Exchange(ref _exactWorkerBusy, 0);
                Interlocked.Exchange(ref _exactWorkerLastProgressTicks, Stopwatch.GetTimestamp());

                if (result != 1)
                {
                    GPUWaterfallLogger.LogRateLimited("WF-WORKER", "process-" + slot, 1000,
                        "RX" + (slot + 1) + " process result=" + result +
                        " fft=" + request.FftSize + " width=" + request.Width);
                    return;
                }

                float[] published = new float[request.Width];
                Array.Copy(_exactNativeRow[slot], published, request.Width);
                long resultSeq = Interlocked.Increment(ref _exactResultSeq[slot]);
                var workerResult = new ExactWorkerResult
                {
                    Row = published,
                    Width = request.Width,
                    FftSize = request.FftSize,
                    EffectiveOverlap = effectiveOverlap,
                    ResultSeq = resultSeq,
                    ProducedTicks = Stopwatch.GetTimestamp()
                };
                Interlocked.Exchange(ref _exactWorkerResult[slot], workerResult);

                GPUWaterfallLogger.LogRateLimited("WF-WORKER", "ready-" + slot, 1000,
                    "RX" + (slot + 1) + " READY fft=" + request.FftSize +
                    " width=" + request.Width + " overlap=" + (effectiveOverlap * 100.0).ToString("F0") + "%");
            }
            catch (Exception ex)
            {
                Interlocked.Exchange(ref _exactWorkerBusy, 0);
                GPUWaterfallLogger.LogRateLimited("WF-WORKER", "slot-ex-" + slot, 2000, ex.ToString());
            }
        }

        // Render-thread API.  It only publishes a request and consumes a completed
        // immutable row.  It never calls D3D11, never maps a staging resource and
        // therefore cannot block D2D EndDraw or hold _objDX2Lock on a GPU wait.
        // 1 = new GPU row consumed; 0 = worker healthy but no new row; -1 = CPU fallback.
        private static int TryGetExactGpuWaterfallDataRow(
            int rx, int width, float[] reference, int referenceCount,
            out float[] dataRow)
        {
            dataRow = null;
            if (!ExactNativeGPURequested || console == null || !console.PowerOn ||
                width <= 0 || width > 8192)
                return -1;

            int slot = rx - 1;
            if (slot < 0 || slot > 1)
                return -1;

            EnsureExactGPUWorker();

            if (Volatile.Read(ref _exactWorkerBusy) != 0)
            {
                long busySince = Interlocked.Read(ref _exactWorkerBusySinceTicks);
                if (busySince > 0)
                {
                    double busyMs = (Stopwatch.GetTimestamp() - busySince) * 1000.0 / Stopwatch.Frequency;
                    if (busyMs >= ExactGpuWorkerHungMs)
                    {
                        if (Interlocked.Exchange(ref _exactWorkerDeclaredHung, 1) == 0)
                            GPUWaterfallLogger.Log("WF-WORKER-HANG",
                                "native GPU worker blocked for " + busyMs.ToString("F0") +
                                "ms; render thread remains on CPU waterfall fallback");
                        return -1;
                    }
                }
            }

            int fftSize = _gpuWaterfallFFTSize;
            if (fftSize < 1024) fftSize = ExactGpuFftSize;
            if (fftSize > 262144) fftSize = 262144;

            int sampleRate = cmaster.GetInputRate(0, slot);
            if (sampleRate <= 0) sampleRate = rx == 1 ? SampleRateRX1 : SampleRateRX2;
            if (sampleRate <= 0) sampleRate = 192000;

            var request = new ExactWorkerRequest
            {
                Rx = rx,
                Width = width,
                FftSize = fftSize,
                SampleRate = sampleRate,
                Window = (int)_gpuWaterfallWindowType,
                Kaiser = (float)_gpuWaterfallKaiserBeta,
                Magnitude = (int)_gpuWaterfallMagnitudeMode,
                Lanczos = _gpuWaterfallLanczosWindow,
                Resampling = (int)_gpuWaterfallResamplingMode,
                OverlapPercent = _gpuWaterfallOverlapPercent,
                AutoOverlap = _gpuWaterfallAutoOverlap,
                Fps = Math.Max(1.0, m_nFps),
                UpdatePeriod = Math.Max(1, rx == 1 ? waterfall_update_period : rx2_waterfall_update_period),
                LowHz = rx == 1 ? RXDisplayLow : RX2DisplayLow,
                HighHz = rx == 1 ? RXDisplayHigh : RX2DisplayHigh,
                RequestSeq = Interlocked.Increment(ref _exactRequestSeq[slot])
            };

            Interlocked.Exchange(ref _exactDesiredEnabled[slot], 1);
            Interlocked.Exchange(ref _exactWorkerRequest[slot], request);
            _exactWorkerWake.Set();

            ExactWorkerResult workerResult =
                Interlocked.CompareExchange(ref _exactWorkerResult[slot], null, null);
            if (workerResult == null || workerResult.Row == null ||
                workerResult.Width != width || workerResult.FftSize != fftSize)
                return 0;

            double ageMs = (Stopwatch.GetTimestamp() - workerResult.ProducedTicks) * 1000.0 / Stopwatch.Frequency;
            if (ageMs > ExactGpuWorkerStaleMs)
            {
                GPUWaterfallLogger.LogRateLimited("WF-WORKER-STALE", "rx" + rx, 1000,
                    "RX" + rx + " latest GPU row age=" + ageMs.ToString("F0") + "ms; using CPU row");
                return -1;
            }

            long consumed = Interlocked.Read(ref _exactConsumedResultSeq[slot]);
            if (workerResult.ResultSeq <= consumed)
                return 0;
            Interlocked.Exchange(ref _exactConsumedResultSeq[slot], workerResult.ResultSeq);

            if (Math.Abs(workerResult.EffectiveOverlap - _gpuLastEffectiveOverlap[slot]) > 0.005)
            {
                _gpuLastEffectiveOverlap[slot] = workerResult.EffectiveOverlap;
                GPUWaterfallEffectiveOverlapChanged?.Invoke(rx, workerResult.EffectiveOverlap);
            }

            int refCount = Math.Min(referenceCount, reference == null ? 0 : reference.Length);
            if (refCount > 8)
            {
                float cpuMedian = ExactMedian(reference, refCount, slot);
                float gpuMedian = ExactMedian(workerResult.Row, width, slot);
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

            if (_exactRowCopy[slot] == null || _exactRowCopy[slot].Length != width)
                _exactRowCopy[slot] = new float[width];

            float offset = _exactCalOffset[slot];
            float rowMin = float.PositiveInfinity;
            float rowMax = float.NegativeInfinity;
            int finiteCount = 0;
            for (int i = 0; i < width; i++)
            {
                float v = workerResult.Row[i] + offset;
                _exactRowCopy[slot][i] = v;
                if (!float.IsNaN(v) && !float.IsInfinity(v))
                {
                    finiteCount++;
                    if (v < rowMin) rowMin = v;
                    if (v > rowMax) rowMax = v;
                }
            }

            GPUWaterfallLogger.LogRateLimited("WF-SOURCE", "ready-rx" + rx, 1000,
                "RX" + rx + " ASYNC READY sr=" + sampleRate + " fft=" + fftSize +
                " width=" + width + " age=" + ageMs.ToString("F0") + "ms");
            GPUWaterfallLogger.LogRateLimited("WF-DATA", "rx" + rx, 1000,
                "RX" + rx +
                " finite=" + finiteCount + "/" + width +
                " min=" + (finiteCount > 0 ? rowMin.ToString("F1") : "NaN") +
                " max=" + (finiteCount > 0 ? rowMax.ToString("F1") : "NaN") +
                " span=" + (finiteCount > 0 ? (rowMax - rowMin).ToString("F1") : "NaN") +
                " cal=" + offset.ToString("F2"));

            dataRow = _exactRowCopy[slot];
            return 1;
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
