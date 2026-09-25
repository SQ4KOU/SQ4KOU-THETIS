using System;
using System.Diagnostics;
using SharpDX;

namespace Thetis
{
    partial class Display
    {
        private static WaterfallGPURenderer _waterfallGPU1 = null;

        private static WaterfallGPURenderer _waterfallGPU2 = null;

        private static GPUWaterfallPipeline _gpuFFT1 = null;

        private static GPUWaterfallPipeline _gpuFFT2 = null;

        private static float[] _gpuIQbufI = null;

        private static float[] _gpuIQbufQ = null;

        private static float[] _gpuIQaccumI = null;

        private static float[] _gpuIQaccumQ = null;

        private static float[][] _gpuIQringI = new float[2][];

        private static float[][] _gpuIQringQ = new float[2][];

        private static int[] _gpuIQringHead = new int[2];

        private static int[] _gpuIQringCount = new int[2];

        private static int[] _gpuSampleCredit = new int[2];

        private static bool[] _gpuFirstFillDone = new bool[2];

        // 0=RX1, 1=RX2, 2=TX post-DSP ring. Prevent RX/TX sample mixing across MOX transitions.
        private static int[] _gpuLastIQSource = new int[2] { -1, -1 };

        private static int _gpuWaterfallDebugSkip = 0;

        private static int[] _gpuRowLogSkip = new int[2];

        private static bool[] _gpuPipeFailLogged = new bool[2];

        private static long[] _gpuLastDropLogMs = new long[2];

        private static readonly int[] _gpuCalOutlierSkip = new int[2];

        private static float _gpuCalOffsetRX1 = 0f;

        private static float _gpuCalOffsetRX2 = 0f;

        private static double[] _gpuLastEffectiveOverlap = new double[2] { -1.0, -1.0 };

        private static bool _gpuCalInitRX1 = false;

        private static bool _gpuCalInitRX2 = false;

        private static int _gpuCalStartupCountRX1 = 0;

        private static int _gpuCalStartupCountRX2 = 0;

        private const int GPU_CAL_STARTUP_FRAMES = 10;

        private static int _gpuLastFFTSizeRX1 = 0;

        private static int _gpuLastFFTSizeRX2 = 0;

        private static float[] _gpuCalReferenceRowRX1 = null;

        private static float[] _gpuCalReferenceRowRX2 = null;

        private static float[] _gpuMedianBuffer = null;

        private const int GPU_WATERFALL_IQ_CAPACITY = 524288;

        private static bool _gpuWaterfallPipelineEnabled = false;

        private static int _gpuWaterfallFFTSize = 16384;

        private static int _gpuWaterfallOverlapPercent = 85;

        private static GPUWaterfallWindowType _gpuWaterfallWindowType = GPUWaterfallWindowType.Nuttall;

        private static double _gpuWaterfallKaiserBeta = 6.0;

        private static GPUWaterfallMagnitudeMode _gpuWaterfallMagnitudeMode = GPUWaterfallMagnitudeMode.PeakHoldPower;

        private static bool _gpuWaterfallAutoOverlap = false;

        private static int _gpuWaterfallLanczosWindow = 3;

        private static GPUWaterfallResamplingMode _gpuWaterfallResamplingMode = GPUWaterfallResamplingMode.Quality;

        private static bool _gpuWaterfallLinearDraw = true;

        private static float[] _gpuPaletteUpload = new float[4096];

        public static event Action<int, double> GPUWaterfallEffectiveOverlapChanged;

        public static bool GPUWaterfallPipelineEnabled
        {
            get
            {
                return _gpuWaterfallPipelineEnabled;
            }
            set
            {
                if (_gpuWaterfallPipelineEnabled != value)
                {
                    _gpuWaterfallPipelineEnabled = value;
                    try
                    {
                        SetNativeWaterfallIQEnabled(value);
                    }
                    catch
                    {
                    }
                    ResetWaterfallBmp();
                    ResetWaterfallBmp2();
                    ResetGPUWaterfallState(1);
                    ResetGPUWaterfallState(2);
                }
            }
        }

        public static int GPUWaterfallFFTSize
        {
            get
            {
                return _gpuWaterfallFFTSize;
            }
            set
            {
                int num = value;
                if (num < 1024)
                {
                    num = 1024;
                }
                if (num > 262144)
                {
                    num = 262144;
                }
                num = PowerOfTwo(num);
                if (_gpuWaterfallFFTSize != num)
                {
                    LogGPU($"GPUWaterfallFFTSize changed from {_gpuWaterfallFFTSize} to {num}. Stack trace:\n{new StackTrace()}");
                    _gpuWaterfallFFTSize = num;
                    if (_gpuWaterfallPipelineEnabled)
                    {
                        ResetGPUWaterfallState(1);
                        ResetGPUWaterfallState(2);
                    }
                }
            }
        }

        public static bool GPUWaterfallLinearDraw
        {
            get
            {
                return _gpuWaterfallLinearDraw;
            }
            set
            {
                _gpuWaterfallLinearDraw = value;
            }
        }

        public static GPUWaterfallWindowType GPUWaterfallWindowType
        {
            get
            {
                return _gpuWaterfallWindowType;
            }
            set
            {
                if (_gpuWaterfallWindowType != value)
                {
                    _gpuWaterfallWindowType = value;
                    if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
                    {
                        _gpuFFT1.WindowType = value;
                    }
                    if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
                    {
                        _gpuFFT2.WindowType = value;
                    }
                }
            }
        }

        public static double GPUWaterfallKaiserBeta
        {
            get
            {
                return _gpuWaterfallKaiserBeta;
            }
            set
            {
                double num = value;
                if (num < 0.0)
                {
                    num = 0.0;
                }
                if (num > 20.0)
                {
                    num = 20.0;
                }
                if (!(Math.Abs(_gpuWaterfallKaiserBeta - num) < 0.01))
                {
                    _gpuWaterfallKaiserBeta = num;
                    if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
                    {
                        _gpuFFT1.KaiserBeta = num;
                    }
                    if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
                    {
                        _gpuFFT2.KaiserBeta = num;
                    }
                }
            }
        }

        public static GPUWaterfallMagnitudeMode GPUWaterfallMagnitudeMode
        {
            get
            {
                return _gpuWaterfallMagnitudeMode;
            }
            set
            {
                if (_gpuWaterfallMagnitudeMode != value)
                {
                    _gpuWaterfallMagnitudeMode = value;
                    if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
                    {
                        _gpuFFT1.MagnitudeMode = value;
                    }
                    if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
                    {
                        _gpuFFT2.MagnitudeMode = value;
                    }
                }
            }
        }

        public static int GPUWaterfallOverlapPercent
        {
            get
            {
                return _gpuWaterfallOverlapPercent;
            }
            set
            {
                int num = value;
                if (num < 0)
                {
                    num = 0;
                }
                if (num > 95)
                {
                    num = 95;
                }
                if (_gpuWaterfallOverlapPercent != num)
                {
                    _gpuWaterfallOverlapPercent = num;
                    ResetGPUWaterfallState(1, resetCalibration: false);
                    ResetGPUWaterfallState(2, resetCalibration: false);
                }
            }
        }

        public static bool GPUWaterfallAutoOverlap
        {
            get
            {
                return _gpuWaterfallAutoOverlap;
            }
            set
            {
                if (_gpuWaterfallAutoOverlap != value)
                {
                    _gpuWaterfallAutoOverlap = value;
                    ResetGPUWaterfallState(1, resetCalibration: false);
                    ResetGPUWaterfallState(2, resetCalibration: false);
                }
            }
        }

        public static int GPUWaterfallLanczosWindow
        {
            get
            {
                return _gpuWaterfallLanczosWindow;
            }
            set
            {
                int num = value;
                if (num < 0)
                {
                    num = 0;
                }
                if (num > 4)
                {
                    num = 4;
                }
                if (num == 1)
                {
                    num = 2;
                }
                if (_gpuWaterfallLanczosWindow != num)
                {
                    _gpuWaterfallLanczosWindow = num;
                    if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
                    {
                        _gpuFFT1.LanczosWindow = num;
                    }
                    if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
                    {
                        _gpuFFT2.LanczosWindow = num;
                    }
                }
            }
        }

        public static GPUWaterfallResamplingMode GPUWaterfallResamplingMode
        {
            get
            {
                return _gpuWaterfallResamplingMode;
            }
            set
            {
                if (_gpuWaterfallResamplingMode != value)
                {
                    _gpuWaterfallResamplingMode = value;
                    if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
                    {
                        _gpuFFT1.ResamplingMode = value;
                    }
                    if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
                    {
                        _gpuFFT2.ResamplingMode = value;
                    }
                }
            }
        }

        public static ulong GPUWaterfallDroppedSamplesRX1 => GPUWaterfallNative.CM_WaterfallIQ_DroppedSamples(0);
        public static ulong GPUWaterfallDroppedSamplesRX2 => GPUWaterfallNative.CM_WaterfallIQ_DroppedSamples(1);
        public static float GPUWaterfallCalibrationOffsetRX1 => _gpuCalOffsetRX1;
        public static float GPUWaterfallCalibrationOffsetRX2 => _gpuCalOffsetRX2;

        private static void EnsureGPUWaterfallPipeline(int rx, int width, int height)
        {
            if (!_gpuWaterfallPipelineEnabled || !_gpuEffectsEnabled)
            {
                if (rx == 1)
                {
                    Utilities.Dispose(ref _gpuFFT1);
                }
                else
                {
                    Utilities.Dispose(ref _gpuFFT2);
                }
                return;
            }
            int num = cmaster.GetInputRate(0, rx - 1);
            if (num <= 0)
            {
                num = ((rx == 1) ? SampleRateRX1 : SampleRateRX2);
            }
            GPUWaterfallPipeline gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
            bool num2 = gPUWaterfallPipeline == null;
            if (num2)
            {
                gPUWaterfallPipeline = new GPUWaterfallPipeline(_device, _gpuWaterfallFFTSize, width, num);
                if (rx == 1)
                {
                    _gpuFFT1 = gPUWaterfallPipeline;
                }
                else
                {
                    _gpuFFT2 = gPUWaterfallPipeline;
                }
            }
            else
            {
                gPUWaterfallPipeline.Resize(_gpuWaterfallFFTSize, width, num);
            }
            if (num2)
            {
                ResetGPUWaterfallState(rx);
            }
            if (gPUWaterfallPipeline.IsInitialized)
            {
                gPUWaterfallPipeline.WindowType = _gpuWaterfallWindowType;
                gPUWaterfallPipeline.KaiserBeta = _gpuWaterfallKaiserBeta;
                gPUWaterfallPipeline.MagnitudeMode = _gpuWaterfallMagnitudeMode;
                gPUWaterfallPipeline.LanczosWindow = _gpuWaterfallLanczosWindow;
                gPUWaterfallPipeline.ResamplingMode = _gpuWaterfallResamplingMode;
            }
            LogGPU($"InitOrResizeGPUWaterfall RX{rx}: FFT={_gpuWaterfallFFTSize}, width={width}, SR={num}, window={_gpuWaterfallWindowType}, kaiser={_gpuWaterfallKaiserBeta:F1}, magnitude={_gpuWaterfallMagnitudeMode}, resampling={_gpuWaterfallResamplingMode}, overlap={_gpuWaterfallOverlapPercent}%, initialized={gPUWaterfallPipeline.IsInitialized}");
        }

        private static void ResetGPUWaterfallState(int rx, bool resetCalibration = true)
        {
            lock (_objDX2Lock)
            {
                int num = rx - 1;
                _gpuSampleCredit[num] = 0;
                _gpuIQringHead[num] = 0;
                _gpuIQringCount[num] = 0;
                _gpuFirstFillDone[num] = false;
                _gpuRendererHasData[num] = false;
                if (_gpuIQringI[num] != null)
                {
                    Array.Clear(_gpuIQringI[num], 0, _gpuIQringI[num].Length);
                }
                if (_gpuIQringQ[num] != null)
                {
                    Array.Clear(_gpuIQringQ[num], 0, _gpuIQringQ[num].Length);
                }
                if (resetCalibration)
                {
                    if (rx == 1)
                    {
                        _gpuCalInitRX1 = false;
                        _gpuCalStartupCountRX1 = 0;
                        _gpuLastFFTSizeRX1 = 0;
                    }
                    else
                    {
                        _gpuCalInitRX2 = false;
                        _gpuCalStartupCountRX2 = 0;
                        _gpuLastFFTSizeRX2 = 0;
                    }
                }
            }
        }

        private static float[] ProcessGPUWaterfall(int rx, int width)
        {
            if (!_gpuWaterfallPipelineEnabled)
            {
                return null;
            }
            GPUWaterfallPipeline gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
            if (gPUWaterfallPipeline == null || !gPUWaterfallPipeline.IsInitialized)
            {
                EnsureGPUWaterfallPipeline(rx, width, 1);
                gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
            }
            if (gPUWaterfallPipeline != null && gPUWaterfallPipeline.IsInitialized && gPUWaterfallPipeline.FFTSize != _gpuWaterfallFFTSize)
            {
                EnsureGPUWaterfallPipeline(rx, width, 1);
                gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
            }
            bool gpuTxSource = localMox(rx) && !DisplayDuplex;
            int gpuSourceStream = gpuTxSource ? 2 : ((rx != 1) ? 1 : 0);
            int inputRate = gpuTxSource ? cmaster.GetChannelOutputRate(1, 0) : cmaster.GetInputRate(0, rx - 1);
            if (inputRate <= 0 && gpuTxSource)
            {
                inputRate = cmaster.GetInputRate(1, 0);
            }
            if (inputRate <= 0)
            {
                inputRate = gpuTxSource ? 192000 : ((rx == 1) ? SampleRateRX1 : SampleRateRX2);
            }
            if (gPUWaterfallPipeline != null && inputRate > 0 && Math.Abs(gPUWaterfallPipeline.SampleRate - (float)inputRate) > 1f)
            {
                // Keep the same recovered GPU pipeline, but bind it to the active IQ source rate.
                gPUWaterfallPipeline.Resize(_gpuWaterfallFFTSize, width, inputRate);
                if (gPUWaterfallPipeline.IsInitialized)
                {
                    gPUWaterfallPipeline.WindowType = _gpuWaterfallWindowType;
                    gPUWaterfallPipeline.KaiserBeta = _gpuWaterfallKaiserBeta;
                    gPUWaterfallPipeline.MagnitudeMode = _gpuWaterfallMagnitudeMode;
                    gPUWaterfallPipeline.LanczosWindow = _gpuWaterfallLanczosWindow;
                    gPUWaterfallPipeline.ResamplingMode = _gpuWaterfallResamplingMode;
                }
                ResetGPUWaterfallState(rx);
            }
            if (gPUWaterfallPipeline == null || !gPUWaterfallPipeline.IsInitialized)
            {
                if (!_gpuPipeFailLogged[rx - 1])
                {
                    _gpuPipeFailLogged[rx - 1] = true;
                    LogGPU(string.Format("ProcessGPUWaterfall RX{0}: pipe {1}", rx, (gPUWaterfallPipeline == null) ? "null" : "not initialized"));
                }
                return null;
            }
            _gpuPipeFailLogged[rx - 1] = false;
            int fFTSize = gPUWaterfallPipeline.FFTSize;
            int num = ((rx == 1) ? _gpuLastFFTSizeRX1 : _gpuLastFFTSizeRX2);
            if (fFTSize != num)
            {
                if (rx == 1)
                {
                    _gpuCalInitRX1 = false;
                    _gpuLastFFTSizeRX1 = fFTSize;
                }
                else
                {
                    _gpuCalInitRX2 = false;
                    _gpuLastFFTSizeRX2 = fFTSize;
                }
            }
            int num2 = rx - 1;
            if (_gpuLastIQSource[num2] != gpuSourceStream)
            {
                // RX<->TX transition: discard accumulated samples so one FFT can never mix sources.
                _gpuIQringHead[num2] = 0;
                _gpuIQringCount[num2] = 0;
                _gpuSampleCredit[num2] = 0;
                _gpuFirstFillDone[num2] = false;
                _gpuRendererHasData[num2] = false;
                _gpuLastIQSource[num2] = gpuSourceStream;
                if (rx == 1)
                {
                    _gpuCalInitRX1 = false;
                    _gpuCalStartupCountRX1 = 0;
                }
                else
                {
                    _gpuCalInitRX2 = false;
                    _gpuCalStartupCountRX2 = 0;
                }
            }
            if (fFTSize != num)
            {
                _gpuSampleCredit[num2] = 0;
            }
            if (_gpuIQringI[num2] == null || _gpuIQringI[num2].Length < 524288)
            {
                _gpuIQringI[num2] = new float[524288];
                _gpuIQringQ[num2] = new float[524288];
                _gpuIQringHead[num2] = 0;
                _gpuIQringCount[num2] = 0;
                _gpuFirstFillDone[num2] = false;
            }
            if (_gpuIQbufI == null || _gpuIQbufI.Length < 524288)
            {
                _gpuIQbufI = new float[524288];
                _gpuIQbufQ = new float[524288];
            }
            int stream = gpuSourceStream;
            int num3 = 0;
            int waterfallIQDroppedSamples = (int)Math.Min((ulong)int.MaxValue, GPUWaterfallNative.CM_WaterfallIQ_DroppedSamples(stream));
            if (waterfallIQDroppedSamples > 0)
            {
                long num4 = Environment.TickCount;
                if (num4 - _gpuLastDropLogMs[num2] >= 1000)
                {
                    _gpuLastDropLogMs[num2] = num4;
                    LogGPU($"RX{rx} I/Q ring dropped {waterfallIQDroppedSamples} samples (reader lagged behind writer)");
                }
                GPUWaterfallNative.CM_WaterfallIQ_ResetDropped(stream);
            }
            int num5 = _gpuIQringCount[num2];
            int num6 = _gpuIQringHead[num2];
            int num7 = GPUWaterfallNative.CM_WaterfallIQ_Available(stream);
            if (num7 > 0)
            {
                int maxSamples = Math.Min(num7, 2 * fFTSize);
                if (_gpuIQaccumI == null || _gpuIQaccumI.Length < 524288)
                {
                    _gpuIQaccumI = new float[524288];
                    _gpuIQaccumQ = new float[524288];
                }
                num3 = ReadWaterfallIQ(stream, _gpuIQaccumI, _gpuIQaccumQ, maxSamples);
                if (num3 > 0)
                {
                    float[] array = _gpuIQringI[num2];
                    float[] array2 = _gpuIQringQ[num2];
                    for (int i = 0; i < num3; i++)
                    {
                        array[num6] = _gpuIQaccumI[i];
                        array2[num6] = _gpuIQaccumQ[i];
                        num6 = (num6 + 1) % fFTSize;
                    }
                    num5 = Math.Min(num5 + num3, fFTSize);
                    _gpuIQringHead[num2] = num6;
                    _gpuIQringCount[num2] = num5;
                    _gpuSampleCredit[num2] += num3;
                }
            }
            if (num5 < fFTSize)
            {
                return null;
            }
            double num8 = (double)_gpuWaterfallOverlapPercent / 100.0;
            int num9 = Math.Max(1, Math.Min(fFTSize, (int)Math.Round((double)fFTSize * (1.0 - num8))));
            if (_gpuWaterfallAutoOverlap)
            {
                double num10 = Math.Max(1.0, m_nFps);
                int num11 = Math.Max(1, (rx == 1) ? waterfall_update_period : rx2_waterfall_update_period);
                double num12 = ((gPUWaterfallPipeline.SampleRate > 1f) ? gPUWaterfallPipeline.SampleRate : 192000f);
                double num13 = Math.Min(num10 / (double)num11, 30.0);
                double num14 = num12 / (0.05 * (double)fFTSize);
                if (num13 > num14)
                {
                    num13 = num14;
                }
                num9 = Math.Max(1, Math.Min(fFTSize, (int)Math.Round(num12 / num13)));
            }
            double num15 = 1.0 - (double)num9 / (double)fFTSize;
            if (Math.Abs(num15 - _gpuLastEffectiveOverlap[num2]) > 0.005)
            {
                _gpuLastEffectiveOverlap[num2] = num15;
                GPUWaterfallEffectiveOverlapChanged?.Invoke(rx, num15);
            }
            if (!_gpuFirstFillDone[num2])
            {
                _gpuFirstFillDone[num2] = true;
                _gpuSampleCredit[num2] = 0;
            }
            else
            {
                if (_gpuSampleCredit[num2] < num9)
                {
                    return null;
                }
                _gpuSampleCredit[num2] -= num9;
            }
            int num16 = num9 * 2;
            if (_gpuSampleCredit[num2] > num16)
            {
                _gpuSampleCredit[num2] = num16;
            }
            float[] array3 = _gpuIQringI[num2];
            float[] array4 = _gpuIQringQ[num2];
            for (int j = 0; j < fFTSize; j++)
            {
                int num20 = (_gpuIQringHead[num2] + j) % fFTSize;
                _gpuIQbufI[j] = array3[num20];
                _gpuIQbufQ[j] = array4[num20];
            }
            int num24;
            int num25;
            if (localMox(rx))
            {
                num24 = ((!DisplayDuplex) ? 1 : 0);
                if (num24 != 0)
                {
                    num25 = tx_display_low;
                    goto IL_05a5;
                }
            }
            else
            {
                num24 = 0;
            }
            num25 = ((rx == 1) ? RXDisplayLow : RX2DisplayLow);
            IL_05a5:
            float num26 = num25;
            float num27 = ((num24 != 0) ? tx_display_high : ((rx == 1) ? RXDisplayHigh : RX2DisplayHigh));
            bool flag = ++_gpuRowLogSkip[num2] >= 30;
            if (flag)
            {
                _gpuRowLogSkip[num2] = 0;
            }
            if (flag)
            {
                LogGPU($"ProcessGPUWaterfall RX{rx}: span set lowFreq={num26:F0}, highFreq={num27:F0}, width={width}, fftSize={fFTSize}");
            }
            gPUWaterfallPipeline.SetFrequencySpan(num26, num27);
            float[] array5 = gPUWaterfallPipeline.Process(_gpuIQbufI, _gpuIQbufQ, fFTSize);
            if (flag)
            {
                LogGPU(string.Format("ProcessGPUWaterfall RX{0}: available={1}, got={2}, credit={3}, row={4}, width={5}", rx, num7, num3, _gpuSampleCredit[num2], (array5 != null) ? ("len=" + array5.Length) : "null", width));
            }
            if (array5 != null && array5.Length < width)
            {
                return null;
            }
            if (array5 != null)
            {
                float[] array6 = ((rx == 1) ? _gpuCalReferenceRowRX1 : _gpuCalReferenceRowRX2);
                int num28 = Math.Min(width, (array6 != null) ? array6.Length : 0);
                float num29 = ((rx == 1) ? _gpuCalOffsetRX1 : _gpuCalOffsetRX2);
                if (num28 > 0)
                {
                    float num31 = CalculateMedian(array6, num28);
                    if (num31 > -180f)
                    {
                        float num32 = CalculateMedian(array5, num28);
                        float num33 = num31 - num32;
                        if (float.IsNaN(num33)) num33 = 0f;
                        if (num33 > 200f) num33 = 200f;
                        if (num33 < -200f) num33 = -200f;
                        if (num33 >= -12f && num33 <= 7f)
                        {
                            bool num34 = ((rx == 1) ? _gpuCalInitRX1 : _gpuCalInitRX2);
                            int num35 = ((rx == 1) ? _gpuCalStartupCountRX1 : _gpuCalStartupCountRX2);
                            float num36 = ((!num34 || num35 < 10) ? 0.3f : 0.1f);
                            num29 = num29 * (1f - num36) + num33 * num36;
                            float num37 = ((rx == 1) ? _gpuCalOffsetRX1 : _gpuCalOffsetRX2);
                            if (!num34) num29 = num33;
                            else if (num29 > num37 + 1f) num29 = num37 + 1f;
                            else if (num29 < num37 - 1f) num29 = num37 - 1f;
                            if (!num34)
                            {
                                if (rx == 1) { _gpuCalInitRX1 = true; _gpuCalStartupCountRX1 = 1; }
                                else { _gpuCalInitRX2 = true; _gpuCalStartupCountRX2 = 1; }
                            }
                            else if (rx == 1 && num35 < 10) _gpuCalStartupCountRX1++;
                            else if (rx == 2 && num35 < 10) _gpuCalStartupCountRX2++;
                            if (rx == 1) _gpuCalOffsetRX1 = num29;
                            else _gpuCalOffsetRX2 = num29;
                        }
                    }
                }
                for (int k = 0; k < width; k++) array5[k] += num29;
            }
            return array5;
        }

        private static float CalculateMedian(float[] data, int length)
        {
            if (data == null || length <= 0) return -200f;
            if (_gpuMedianBuffer == null || _gpuMedianBuffer.Length < length) _gpuMedianBuffer = new float[length];
            Array.Copy(data, _gpuMedianBuffer, length);
            Array.Sort(_gpuMedianBuffer, 0, length);
            int mid = length / 2;
            return (length & 1) == 1 ? _gpuMedianBuffer[mid] : (_gpuMedianBuffer[mid - 1] + _gpuMedianBuffer[mid]) * 0.5f;
        }

        private static void LogGPU(string message)
        {
            GPUWaterfallLogger.Log("GPU-DISP", message);
        }

        private static readonly bool[] _gpuRendererHasData = new bool[2];

        private static void SetNativeWaterfallIQEnabled(bool enabled)
        {
            for (int ch = 0; ch < 3; ch++)
            {
                try
                {
                    if (enabled) GPUWaterfallNative.CM_WaterfallIQ_Init(ch, GPU_WATERFALL_IQ_CAPACITY);
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
            for (int i = 0; i < n; i++)
            {
                float t = outI[i]; outI[i] = outQ[i]; outQ[i] = t;
            }
            return n;
        }

        private static int PowerOfTwo(int value)
        {
            int p = 1;
            while (p < value && p < 262144) p <<= 1;
            return p > 262144 ? 262144 : p;
        }

        private static void CaptureGPUCalibrationReference(int rx, float[] cpuRow, int cpuWidth, int gpuWidth)
        {
            if (cpuRow == null || cpuWidth <= 0 || gpuWidth <= 0) return;
            float[] dst = rx == 1 ? _gpuCalReferenceRowRX1 : _gpuCalReferenceRowRX2;
            if (dst == null || dst.Length != gpuWidth) dst = new float[gpuWidth];
            if (cpuWidth == gpuWidth) Array.Copy(cpuRow, dst, gpuWidth);
            else
            {
                for (int x = 0; x < gpuWidth; x++)
                {
                    int src = (int)((long)x * cpuWidth / gpuWidth);
                    if (src >= cpuWidth) src = cpuWidth - 1;
                    dst[x] = cpuRow[src];
                }
            }
            if (rx == 1) _gpuCalReferenceRowRX1 = dst; else _gpuCalReferenceRowRX2 = dst;
        }

        private static GPUWaterfallPipeline GetGPUWaterfallPipeline(int rx) => rx == 1 ? _gpuFFT1 : _gpuFFT2;
        private static float GetGPUWaterfallCalibrationOffset(int rx) => rx == 1 ? _gpuCalOffsetRX1 : _gpuCalOffsetRX2;
        private static bool ManagedGPUFFTRequested => _gpuWaterfallPipelineEnabled && _gpuEffectsEnabled && _waterfallRenderQuality == WaterfallRenderQuality.High;

        private static WaterfallGPURenderer EnsureGPUWaterfallRenderer(int rx, int width, int height)
        {
            if (!_gpuEffectsEnabled || width <= 0 || height <= 0) return null;
            if (!(_d2dRenderTarget is SharpDX.Direct2D1.DeviceContext dc)) return null;
            SharpDX.DXGI.Format format = WaterfallPixelWriter.DxgiFormat;
            if (format != SharpDX.DXGI.Format.B8G8R8A8_UNorm && format != SharpDX.DXGI.Format.R16G16B16A16_Float) return null;
            WaterfallGPURenderer renderer = rx == 1 ? _waterfallGPU1 : _waterfallGPU2;
            if (renderer == null)
            {
                renderer = new WaterfallGPURenderer(_device, dc, width, height, format);
                if (rx == 1) _waterfallGPU1 = renderer; else _waterfallGPU2 = renderer;
            }
            else renderer.Resize(dc, width, height, format);
            return renderer;
        }

        private static bool IsGPUWaterfallPaletteScheme(ColorScheme scheme)
        {
            return scheme == ColorScheme.Console || scheme == ColorScheme.Thermal || scheme == ColorScheme.DeepBlue || scheme == ColorScheme.Custom;
        }

        private static void UploadPaletteToGPU(WaterfallGPURenderer renderer, WaterfallPalette palette)
        {
            if (renderer == null || palette == null) return;
            for (int i = 0; i < 256; i++)
            {
                palette.Sample((float)i / 255f, out float r, out float g, out float b);
                int n = i * 4;
                _gpuPaletteUpload[n] = r; _gpuPaletteUpload[n + 1] = g; _gpuPaletteUpload[n + 2] = b; _gpuPaletteUpload[n + 3] = 1f;
            }
            renderer.SetPalette(_gpuPaletteUpload, 256);
        }

        private static void UploadCustomGradientToGPU(WaterfallGPURenderer renderer, System.Drawing.Color[] colours)
        {
            if (renderer == null || colours == null || colours.Length < 2) return;
            int count = Math.Min(1024, colours.Length);
            for (int i = 0; i < count; i++)
            {
                int n = i * 4;
                _gpuPaletteUpload[n] = colours[i].R / 255f;
                _gpuPaletteUpload[n + 1] = colours[i].G / 255f;
                _gpuPaletteUpload[n + 2] = colours[i].B / 255f;
                _gpuPaletteUpload[n + 3] = 1f;
            }
            renderer.SetPalette(_gpuPaletteUpload, count);
        }

        private static WaterfallPalette GetGPUWaterfallPalette(ColorScheme scheme)
        {
            if (scheme == ColorScheme.Console) return GetPaletteConsole();
            if (scheme == ColorScheme.Thermal) return GetPaletteThermal();
            if (scheme == ColorScheme.DeepBlue) return GetPaletteDeepBlue();
            return null;
        }

        private static bool UpdateManagedGPUWaterfallRenderer(int rx, int width, int height, int horizontalShiftPixels, bool addRow, bool clearExisting,
            ColorScheme scheme, bool localMox, float lowThreshold, float highThreshold, float fOffset,
            GPUWaterfallPipeline pipeline, bool gpuRowReady, float gpuCalOffset)
        {
            int index = rx - 1;
            if (index < 0 || index > 1 || localMox || !ManagedGPUFFTRequested || !IsGPUWaterfallPaletteScheme(scheme))
            {
                if (index >= 0 && index < 2) _gpuRendererHasData[index] = false;
                return false;
            }
            WaterfallGPURenderer renderer = EnsureGPUWaterfallRenderer(rx, width, height);
            if (renderer == null || !renderer.IsInitialized || pipeline == null || !pipeline.IsInitialized)
            {
                _gpuRendererHasData[index] = false;
                return false;
            }
            bool paletteReady = true;
            if (scheme == ColorScheme.Custom)
            {
                System.Drawing.Color[] colours = rx == 1 ? _rx1_waterfall_grad : _rx2_waterfall_grad;
                bool ok = rx == 1 ? _rx1_waterfall_grad_ok : _rx2_waterfall_grad_ok;
                if (!ok) paletteReady = false; else UploadCustomGradientToGPU(renderer, colours);
            }
            else
            {
                WaterfallPalette palette = GetGPUWaterfallPalette(scheme);
                if (palette == null) paletteReady = false; else UploadPaletteToGPU(renderer, palette);
            }
            if (!paletteReady) { _gpuRendererHasData[index] = false; return false; }
            if (clearExisting) renderer.Clear();
            bool inserted = addRow && gpuRowReady && pipeline.MagSpectrumView != null && !pipeline.MagSpectrumView.IsDisposed;
            if (inserted)
            {
                float gamma = WaterfallEnhancer.Gamma;
                float invGamma = gamma != 0f ? 1f / gamma : 1f;
                renderer.ProcessRow(pipeline.MagSpectrumView, width, 1,
                    lowThreshold - gpuCalOffset - fOffset, highThreshold - gpuCalOffset - fOffset,
                    gamma, invGamma, (int)WaterfallEnhancer.ToneMap,
                    WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.ContrastBoost,
                    WaterfallEnhancer.DitherEnabled, WaterfallEnhancer.Levels,
                    _temporalEnabled ? _temporalAlpha : 0f, 0.05f, true, scheme == ColorScheme.Custom,
                    WaterfallEnhancer.PaletteSharpness, WaterfallEnhancer.PaletteContrast);
            }
            renderer.AdvanceRow(horizontalShiftPixels, inserted);
            if (inserted) { _gpuRendererHasData[index] = true; recordWaterfallAdvance(rx, height); }
            return _gpuRendererHasData[index];
        }

        private static bool CanDrawManagedGPUWaterfall(int rx, ColorScheme scheme, int width, int height)
        {
            int index = rx - 1;
            if (index < 0 || index > 1 || !_gpuRendererHasData[index] || !ManagedGPUFFTRequested || !IsGPUWaterfallPaletteScheme(scheme)) return false;
            WaterfallGPURenderer renderer = rx == 1 ? _waterfallGPU1 : _waterfallGPU2;
            return renderer != null && renderer.IsInitialized && renderer.Width == width && renderer.Height == height;
        }

        private static void DrawManagedGPUWaterfall(int rx, int nVerticalShift, float opacity)
        {
            WaterfallGPURenderer renderer = rx == 1 ? _waterfallGPU1 : _waterfallGPU2;
            if (renderer != null && renderer.IsInitialized) renderer.Draw(0, nVerticalShift + 20, opacity, null, _gpuWaterfallLinearDraw);
        }
    }
}
