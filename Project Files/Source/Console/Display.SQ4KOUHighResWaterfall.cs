using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace Thetis
{
    partial class Display
    {
        private static readonly object _sq4kouHighResLock = new object();
        private static SharpDX.Direct3D11.Device _sq4kouGpuDevice;
        private static readonly GPUWaterfallPipeline[] _sq4kouGpuPipe = new GPUWaterfallPipeline[2];
        private static readonly float[][] _sq4kouRingI = new float[2][];
        private static readonly float[][] _sq4kouRingQ = new float[2][];
        private static readonly int[] _sq4kouRingHead = new int[2];
        private static readonly int[] _sq4kouRingCount = new int[2];
        private static readonly int[] _sq4kouSampleCredit = new int[2];
        private static readonly bool[] _sq4kouFirstFill = new bool[2];
        private static readonly int[] _sq4kouLastSource = new int[2] { -1, -1 };
        private static readonly float[][] _sq4kouAccumI = new float[2][];
        private static readonly float[][] _sq4kouAccumQ = new float[2][];
        private static readonly float[][] _sq4kouFftI = new float[2][];
        private static readonly float[][] _sq4kouFftQ = new float[2][];
        private static readonly float[][] _sq4kouLastRow = new float[2][];
        private static readonly bool[] _sq4kouHasRow = new bool[2];
        private static readonly float[][] _sq4kouCalScratch = new float[2][];
        private static readonly float[] _sq4kouCalOffset = new float[2];
        private static readonly bool[] _sq4kouCalValid = new bool[2];
        private static readonly string[] _sq4kouHighResPaneStatus = new string[2] { "LEGACY", "LEGACY" };
        private static readonly double[] _sq4kouLastEffectiveOverlap = new double[2] { -1.0, -1.0 };

        // State expected by the final GPU Waterfall UI.
        private static bool _gpuWaterfallLinearDraw = true;
        private static readonly bool[] _gpuRendererHasData = new bool[2];

        private static bool _gpuWaterfallPipelineEnabled = false;
        private static int _gpuWaterfallFFTSize = 16384;
        private static int _gpuWaterfallOverlapPercent = 85;
        private static GPUWaterfallWindowType _gpuWaterfallWindowType = GPUWaterfallWindowType.Nuttall;
        private static double _gpuWaterfallKaiserBeta = 6.0;
        private static GPUWaterfallMagnitudeMode _gpuWaterfallMagnitudeMode = GPUWaterfallMagnitudeMode.PeakHoldPower;
        private static bool _gpuWaterfallAutoOverlap = false;
        private static int _gpuWaterfallLanczosWindow = 3;
        private static GPUWaterfallResamplingMode _gpuWaterfallResamplingMode = GPUWaterfallResamplingMode.Quality;

        public static bool SQ4KOUHighResWaterfallEnabled
        {
            get { return _gpuWaterfallPipelineEnabled; }
            set { GPUWaterfallPipelineEnabled = value; }
        }

        public static int SQ4KOUHighResFftSize { get { return _gpuWaterfallFFTSize; } }
        public static int SQ4KOUHighResWindowType { get { return (int)_gpuWaterfallWindowType; } }
        public static int SQ4KOUHighResResamplingMode { get { return (int)_gpuWaterfallResamplingMode; } }
        public static float SQ4KOUHighResOverlapPercent { get { return _gpuWaterfallOverlapPercent; } }
        public static bool SQ4KOUHighResAutoOverlap { get { return _gpuWaterfallAutoOverlap; } }
        public static string SQ4KOUHighResWaterfallStatusRX1 { get { return _sq4kouHighResPaneStatus[0]; } }
        public static string SQ4KOUHighResWaterfallStatusRX2 { get { return _sq4kouHighResPaneStatus[1]; } }

        public static event Action<int, double> GPUWaterfallEffectiveOverlapChanged;

        public static bool GPUWaterfallPipelineEnabled
        {
            get { return _gpuWaterfallPipelineEnabled; }
            set
            {
                if (_gpuWaterfallPipelineEnabled == value) return;
                _gpuWaterfallPipelineEnabled = value;
                SetNativeWaterfallIQEnabled(value);
                ResetGPUWaterfallState(1);
                ResetGPUWaterfallState(2);
            }
        }

        public static int GPUWaterfallFFTSize
        {
            get { return _gpuWaterfallFFTSize; }
            set
            {
                int v = PowerOfTwo(Math.Max(1024, Math.Min(262144, value)));
                if (_gpuWaterfallFFTSize == v) return;
                _gpuWaterfallFFTSize = v;
                ResetGPUWaterfallState(1);
                ResetGPUWaterfallState(2);
            }
        }

        public static GPUWaterfallWindowType GPUWaterfallWindowType
        {
            get { return _gpuWaterfallWindowType; }
            set
            {
                if (_gpuWaterfallWindowType == value) return;
                _gpuWaterfallWindowType = value;
                ReconfigurePipelines();
            }
        }

        public static double GPUWaterfallKaiserBeta
        {
            get { return _gpuWaterfallKaiserBeta; }
            set
            {
                double v = Math.Max(0.0, Math.Min(20.0, value));
                if (Math.Abs(_gpuWaterfallKaiserBeta - v) < 0.01) return;
                _gpuWaterfallKaiserBeta = v;
                ReconfigurePipelines();
            }
        }

        public static GPUWaterfallMagnitudeMode GPUWaterfallMagnitudeMode
        {
            get { return _gpuWaterfallMagnitudeMode; }
            set
            {
                if (_gpuWaterfallMagnitudeMode == value) return;
                _gpuWaterfallMagnitudeMode = value;
                ReconfigurePipelines();
            }
        }

        public static int GPUWaterfallOverlapPercent
        {
            get { return _gpuWaterfallOverlapPercent; }
            set { _gpuWaterfallOverlapPercent = Math.Max(0, Math.Min(95, value)); }
        }

        public static bool GPUWaterfallAutoOverlap
        {
            get { return _gpuWaterfallAutoOverlap; }
            set { _gpuWaterfallAutoOverlap = value; }
        }

        public static int GPUWaterfallLanczosWindow
        {
            get { return _gpuWaterfallLanczosWindow; }
            set
            {
                int v = value <= 0 ? 2 : Math.Max(2, Math.Min(4, value));
                if (_gpuWaterfallLanczosWindow == v) return;
                _gpuWaterfallLanczosWindow = v;
                ReconfigurePipelines();
            }
        }

        public static GPUWaterfallResamplingMode GPUWaterfallResamplingMode
        {
            get { return _gpuWaterfallResamplingMode; }
            set
            {
                if (_gpuWaterfallResamplingMode == value) return;
                _gpuWaterfallResamplingMode = value;
                ReconfigurePipelines();
            }
        }

        public static void ConfigureSQ4KOUHighResWaterfall(int fftSize, int windowType,
            int resamplingMode, bool autoOverlap, float overlapPercent)
        {
            GPUWaterfallFFTSize = fftSize;
            GPUWaterfallWindowType = (GPUWaterfallWindowType)Math.Max(0, Math.Min(5, windowType));
            GPUWaterfallResamplingMode = (GPUWaterfallResamplingMode)Math.Max(0, Math.Min(1, resamplingMode));
            GPUWaterfallAutoOverlap = autoOverlap;
            GPUWaterfallOverlapPercent = (int)Math.Round(Math.Max(0.0f, Math.Min(95.0f, overlapPercent)));
        }

        private static void ReconfigurePipelines()
        {
            lock (_sq4kouHighResLock)
            {
                for (int i = 0; i < 2; i++)
                {
                    GPUWaterfallPipeline p = _sq4kouGpuPipe[i];
                    if (p != null && p.IsInitialized)
                    {
                        p.WindowType = _gpuWaterfallWindowType;
                        p.KaiserBeta = _gpuWaterfallKaiserBeta;
                        p.MagnitudeMode = _gpuWaterfallMagnitudeMode;
                        p.LanczosWindow = _gpuWaterfallLanczosWindow;
                        p.ResamplingMode = _gpuWaterfallResamplingMode;
                    }
                }
            }
        }

        private static bool EnsureSQ4KOUDevice()
        {
            if (_sq4kouGpuDevice != null) return true;
            try
            {
                _sq4kouGpuDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.None);
                SetNativeWaterfallIQEnabled(true);
                LogGPU("Managed GPU waterfall device created");
                return true;
            }
            catch (Exception ex)
            {
                LogGPU("Managed GPU waterfall device creation failed: " + ex.Message);
                _sq4kouGpuDevice = null;
                return false;
            }
        }

        private static void EnsurePipeline(int pane, int width, int sampleRate)
        {
            if (!EnsureSQ4KOUDevice()) return;
            GPUWaterfallPipeline p = _sq4kouGpuPipe[pane];
            if (p == null)
            {
                // Reset ring/credit state before constructing the new pipeline.
                // Calling Reset after assignment would dispose the pipeline we just created.
                ResetGPUWaterfallState(pane + 1, false);
                p = new GPUWaterfallPipeline(_sq4kouGpuDevice, _gpuWaterfallFFTSize, width, sampleRate);
                _sq4kouGpuPipe[pane] = p;
            }
            else
            {
                p.Resize(_gpuWaterfallFFTSize, width, sampleRate);
            }

            if (p.IsInitialized)
            {
                p.WindowType = _gpuWaterfallWindowType;
                p.KaiserBeta = _gpuWaterfallKaiserBeta;
                p.MagnitudeMode = _gpuWaterfallMagnitudeMode;
                p.LanczosWindow = _gpuWaterfallLanczosWindow;
                p.ResamplingMode = _gpuWaterfallResamplingMode;
            }
        }

        private static void SetNativeWaterfallIQEnabled(bool enabled)
        {
            for (int ch = 0; ch < 3; ch++)
            {
                try
                {
                    if (enabled) GPUWaterfallNative.CM_WaterfallIQ_Init(ch, 524288);
                    GPUWaterfallNative.CM_WaterfallIQ_SetEnabled(ch, enabled ? 1 : 0);
                    if (!enabled) GPUWaterfallNative.CM_WaterfallIQ_ResetDropped(ch);
                }
                catch (Exception ex)
                {
                    LogGPU("Native IQ control failed ch" + ch + ": " + ex.Message);
                }
            }
        }

        private static int ReadWaterfallIQ(int stream, float[] outI, float[] outQ, int maxSamples)
        {
            if (outI == null || outQ == null || outI.Length < maxSamples || outQ.Length < maxSamples) return 0;
            int n;
            try { n = GPUWaterfallNative.CM_WaterfallIQ_Get(stream, maxSamples, outI, outQ); }
            catch { return 0; }

            // Exact final branch convention. Without this swap the spectrum is mirrored.
            for (int i = 0; i < n; i++)
            {
                float t = outI[i];
                outI[i] = outQ[i];
                outQ[i] = t;
            }
            return n;
        }

        private static void ResetGPUWaterfallState(int rx, bool resetCalibration = true)
        {
            int pane = rx == 2 ? 1 : 0;
            lock (_sq4kouHighResLock)
            {
                _sq4kouRingHead[pane] = 0;
                _sq4kouRingCount[pane] = 0;
                _sq4kouSampleCredit[pane] = 0;
                _sq4kouFirstFill[pane] = false;
                _sq4kouLastSource[pane] = -1;
                _sq4kouHasRow[pane] = false;
                _gpuRendererHasData[pane] = false;
                if (_sq4kouRingI[pane] != null) Array.Clear(_sq4kouRingI[pane], 0, _sq4kouRingI[pane].Length);
                if (_sq4kouRingQ[pane] != null) Array.Clear(_sq4kouRingQ[pane], 0, _sq4kouRingQ[pane].Length);
                if (resetCalibration)
                {
                    _sq4kouCalValid[pane] = false;
                    _sq4kouCalOffset[pane] = 0f;
                }
                try
                {
                    GPUWaterfallPipeline p = _sq4kouGpuPipe[pane];
                    if (p != null)
                    {
                        p.Dispose();
                        _sq4kouGpuPipe[pane] = null;
                    }
                }
                catch { }
            }
        }

        private static int PowerOfTwo(int value)
        {
            int p = 1;
            while (p < value && p < 262144) p <<= 1;
            return p > 262144 ? 262144 : p;
        }

        private static float Median(float[] data, int count, int pane)
        {
            if (data == null || count <= 0) return -200f;
            if (_sq4kouCalScratch[pane] == null || _sq4kouCalScratch[pane].Length < count)
                _sq4kouCalScratch[pane] = new float[count];
            float[] tmp = _sq4kouCalScratch[pane];
            Array.Copy(data, tmp, count);
            Array.Sort(tmp, 0, count);
            int mid = count / 2;
            return (count & 1) != 0 ? tmp[mid] : 0.5f * (tmp[mid - 1] + tmp[mid]);
        }

        private static void UpdateCalibration(int pane, float[] gpuRow, int width,
            float[] cpuReference, int cpuCount)
        {
            if (gpuRow == null || cpuReference == null || width <= 0 || cpuCount < 16) return;
            float[] refRow = new float[width];
            for (int x = 0; x < width; x++)
            {
                int src = (int)((long)x * cpuCount / width);
                if (src >= cpuCount) src = cpuCount - 1;
                refRow[x] = cpuReference[src];
            }

            float diff = Median(refRow, width, pane) - Median(gpuRow, width, pane);
            if (float.IsNaN(diff) || float.IsInfinity(diff) || diff < -12f || diff > 7f) return;

            if (!_sq4kouCalValid[pane])
            {
                _sq4kouCalOffset[pane] = diff;
                _sq4kouCalValid[pane] = true;
            }
            else
            {
                float old = _sq4kouCalOffset[pane];
                float next = old * 0.9f + diff * 0.1f;
                if (next > old + 1f) next = old + 1f;
                else if (next < old - 1f) next = old - 1f;
                _sq4kouCalOffset[pane] = next;
            }
        }

        private static bool TrySQ4KOUHighResWaterfall(int rx, int width,
            float[] cpuReference, int cpuCount, int pixelStep,
            bool localMox, bool displayDuplex, out float[] highResRow)
        {
            highResRow = null;
            int pane = rx == 2 ? 1 : 0;
            if (!_gpuWaterfallPipelineEnabled || width < 64)
            {
                _sq4kouHighResPaneStatus[pane] = "LEGACY (GPU FFT OFF)";
                return false;
            }

            int source = localMox && !displayDuplex ? 2 : pane;
            int sampleRate = source == 2 ? cmaster.GetChannelOutputRate(1, 0) : cmaster.GetInputRate(0, pane);
            if (sampleRate <= 0 && source == 2) sampleRate = cmaster.GetInputRate(1, 0);
            if (sampleRate <= 0) sampleRate = source == 2 ? 192000 : (rx == 1 ? SampleRateRX1 : SampleRateRX2);

            lock (_sq4kouHighResLock)
            {
                EnsurePipeline(pane, width, sampleRate);
                GPUWaterfallPipeline pipe = _sq4kouGpuPipe[pane];
                if (pipe == null || !pipe.IsInitialized)
                {
                    _sq4kouHighResPaneStatus[pane] = "GPU FFT INIT FAILED -> LEGACY";
                    return false;
                }

                if (_sq4kouLastSource[pane] != source)
                {
                    _sq4kouRingHead[pane] = 0;
                    _sq4kouRingCount[pane] = 0;
                    _sq4kouSampleCredit[pane] = 0;
                    _sq4kouFirstFill[pane] = false;
                    _sq4kouHasRow[pane] = false;
                    _sq4kouCalValid[pane] = false;
                    _sq4kouLastSource[pane] = source;
                }

                int fftSize = pipe.FFTSize;
                if (_sq4kouRingI[pane] == null || _sq4kouRingI[pane].Length < 524288)
                {
                    _sq4kouRingI[pane] = new float[524288];
                    _sq4kouRingQ[pane] = new float[524288];
                }
                if (_sq4kouAccumI[pane] == null || _sq4kouAccumI[pane].Length < 524288)
                {
                    _sq4kouAccumI[pane] = new float[524288];
                    _sq4kouAccumQ[pane] = new float[524288];
                    _sq4kouFftI[pane] = new float[524288];
                    _sq4kouFftQ[pane] = new float[524288];
                }

                int available = GPUWaterfallNative.CM_WaterfallIQ_Available(source);
                if (available > 0)
                {
                    int want = Math.Min(available, 2 * fftSize);
                    int got = ReadWaterfallIQ(source, _sq4kouAccumI[pane], _sq4kouAccumQ[pane], want);
                    if (got > 0)
                    {
                        int head = _sq4kouRingHead[pane];
                        for (int i = 0; i < got; i++)
                        {
                            _sq4kouRingI[pane][head] = _sq4kouAccumI[pane][i];
                            _sq4kouRingQ[pane][head] = _sq4kouAccumQ[pane][i];
                            head = (head + 1) % fftSize;
                        }
                        _sq4kouRingHead[pane] = head;
                        _sq4kouRingCount[pane] = Math.Min(_sq4kouRingCount[pane] + got, fftSize);
                        _sq4kouSampleCredit[pane] += got;
                    }
                }

                if (_sq4kouRingCount[pane] < fftSize)
                {
                    _sq4kouHighResPaneStatus[pane] = "GPU FFT PRIMING";
                    return false;
                }

                double overlap = _gpuWaterfallOverlapPercent / 100.0;
                int hop = Math.Max(1, Math.Min(fftSize, (int)Math.Round(fftSize * (1.0 - overlap))));
                if (_gpuWaterfallAutoOverlap)
                {
                    double fps = Math.Max(1.0, m_nFps);
                    int period = Math.Max(1, rx == 1 ? waterfall_update_period : rx2_waterfall_update_period);
                    double targetRows = Math.Min(fps / period, 30.0);
                    double maxRows = sampleRate / (0.05 * fftSize);
                    if (targetRows > maxRows) targetRows = maxRows;
                    hop = Math.Max(1, Math.Min(fftSize, (int)Math.Round(sampleRate / targetRows)));
                }

                double effectiveOverlap = 1.0 - (double)hop / fftSize;
                if (Math.Abs(effectiveOverlap - _sq4kouLastEffectiveOverlap[pane]) > 0.005)
                {
                    _sq4kouLastEffectiveOverlap[pane] = effectiveOverlap;
                    GPUWaterfallEffectiveOverlapChanged?.Invoke(rx, effectiveOverlap);
                }

                if (!_sq4kouFirstFill[pane])
                {
                    _sq4kouFirstFill[pane] = true;
                    _sq4kouSampleCredit[pane] = 0;
                }
                else
                {
                    if (_sq4kouSampleCredit[pane] < hop)
                    {
                        if (_sq4kouHasRow[pane])
                        {
                            highResRow = _sq4kouLastRow[pane];
                            _sq4kouHighResPaneStatus[pane] = "GPU FFT " + fftSize + " ACTIVE (HOLD)";
                            return true;
                        }
                        return false;
                    }
                    _sq4kouSampleCredit[pane] -= hop;
                }
                if (_sq4kouSampleCredit[pane] > hop * 2) _sq4kouSampleCredit[pane] = hop * 2;

                int headNow = _sq4kouRingHead[pane];
                for (int i = 0; i < fftSize; i++)
                {
                    int src = (headNow + i) % fftSize;
                    _sq4kouFftI[pane][i] = _sq4kouRingI[pane][src];
                    _sq4kouFftQ[pane][i] = _sq4kouRingQ[pane][src];
                }

                float lowHz = localMox && !displayDuplex ? tx_display_low : (rx == 1 ? RXDisplayLow : RX2DisplayLow);
                float highHz = localMox && !displayDuplex ? tx_display_high : (rx == 1 ? RXDisplayHigh : RX2DisplayHigh);
                pipe.SetFrequencySpan(lowHz, highHz);

                float[] row = pipe.Process(_sq4kouFftI[pane], _sq4kouFftQ[pane], fftSize);
                if (row == null)
                {
                    if (_sq4kouHasRow[pane])
                    {
                        highResRow = _sq4kouLastRow[pane];
                        _sq4kouHighResPaneStatus[pane] = "GPU FFT " + fftSize + " ACTIVE (READBACK HOLD)";
                        return true;
                    }
                    _sq4kouHighResPaneStatus[pane] = "GPU FFT READBACK";
                    return false;
                }

                if (_sq4kouLastRow[pane] == null || _sq4kouLastRow[pane].Length != width)
                    _sq4kouLastRow[pane] = new float[width];

                UpdateCalibration(pane, row, width, cpuReference, cpuCount);
                float cal = _sq4kouCalValid[pane] ? _sq4kouCalOffset[pane] : 0f;
                for (int i = 0; i < width; i++) _sq4kouLastRow[pane][i] = row[i] + cal;

                _sq4kouHasRow[pane] = true;
                highResRow = _sq4kouLastRow[pane];
                _sq4kouHighResPaneStatus[pane] = "GPU FFT " + fftSize + " ACTIVE";
                return true;
            }
        }

        public static bool ProbeSQ4KOUHighResPipeline(out string status)
        {
            status = "not tested";
            SharpDX.Direct3D11.Device testDevice = null;
            GPUWaterfallPipeline testPipe = null;
            try
            {
                const int fft = 4096;
                const int width = 1024;
                const int sampleRate = 192000;

                testDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.None);
                testPipe = new GPUWaterfallPipeline(testDevice, fft, width, sampleRate);
                if (!testPipe.IsInitialized)
                {
                    status = "FAIL: pipeline init";
                    return false;
                }

                float[] i = new float[fft];
                float[] q = new float[fft];
                double phase = 0.0;
                double step = 2.0 * Math.PI * 12000.0 / sampleRate;
                for (int n = 0; n < fft; n++)
                {
                    i[n] = (float)Math.Cos(phase);
                    q[n] = (float)Math.Sin(phase);
                    phase += step;
                }

                testPipe.SetFrequencySpan(-96000f, 96000f);
                float[] row = null;
                for (int pass = 0; pass < 8 && row == null; pass++)
                    row = testPipe.Process(i, q, fft);

                if (row == null || row.Length < width)
                {
                    status = "FAIL: no FFT readback";
                    return false;
                }

                float min = float.MaxValue;
                float max = float.MinValue;
                int finite = 0;
                for (int x = 0; x < width; x++)
                {
                    float v = row[x];
                    if (float.IsNaN(v) || float.IsInfinity(v)) continue;
                    finite++;
                    if (v < min) min = v;
                    if (v > max) max = v;
                }

                if (finite < width * 9 / 10 || max - min < 6f)
                {
                    status = "FAIL: invalid FFT output";
                    return false;
                }

                status = $"PASS: D3D11 compute + shaders + readback ({width}px, span {max - min:F1} dB)";
                return true;
            }
            catch (Exception ex)
            {
                status = "FAIL: " + ex.GetType().Name + ": " + ex.Message;
                return false;
            }
            finally
            {
                try { testPipe?.Dispose(); } catch { }
                try { testDevice?.Dispose(); } catch { }
            }
        }

        private static void LogGPU(string message)
        {
            GPUWaterfallLogger.Log("GPU-DISP", message);
        }
    }
}
