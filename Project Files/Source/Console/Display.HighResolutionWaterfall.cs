using System;

namespace Thetis
{
    partial class Display
    {
        private const int NativeHighResolutionWaterfallFftSize = 262144;
        private const int NativeHighResolutionWaterfallOversample = 4;
        private const int NativeHighResolutionWaterfallMaxPixels = 32768;
        private static readonly object _nativeHighResolutionWaterfallLock = new object();
        private static clsSpectrumProcessor _nativeHighResolutionWaterfallProcessor;
        private static readonly bool[] _nativeHighResolutionWaterfallAdded = new bool[2];
        private static readonly int[] _nativeHighResolutionWaterfallWidth = new int[2];
        private static readonly float[][] _nativeHighResolutionWaterfallData = new float[2][];
        private static readonly float[][] _nativeHighResolutionWaterfallDataCopy = new float[2][];
        private static readonly float[][] _nativeHighResolutionWaterfallOversampled = new float[2][];
        private static long _nativeHighResolutionWaterfallRetryAfterTicks;

        private static bool TryGetNativeHighResolutionWaterfallData(
            int rx,
            int width,
            out float[] data,
            out float[] dataCopy)
        {
            data = null;
            dataCopy = null;

            if (console == null || !console.PowerOn || width < 64 || width > BUFFER_SIZE)
                return false;

            long nowTicks = DateTime.UtcNow.Ticks;
            if (nowTicks < _nativeHighResolutionWaterfallRetryAfterTicks)
                return false;

            int receiverId = rx - 1;
            if (receiverId < 0 || receiverId > 1)
                return false;

            lock (_nativeHighResolutionWaterfallLock)
            {
                try
                {
                    if (_nativeHighResolutionWaterfallProcessor == null)
                        _nativeHighResolutionWaterfallProcessor = new clsSpectrumProcessor(console);

                    SpecHPSDR source = console.specRX.GetSpecRX(receiverId);
                    if (source == null)
                        return false;

                    int frameRate = Math.Max(1, Math.Min(120, source.FrameRate));
                    int sampleRate = source.SampleRate > 0 ? source.SampleRate : 192000;

                    // Do not ask WDSP to collapse the whole visible RF span directly
                    // into one spectrum value per screen pixel.  That destroys narrow
                    // carriers (FT8 is the most obvious case) before the waterfall ever
                    // sees them.  Instead request an oversampled row and reduce it to
                    // the screen width with a peak-preserving detector below.
                    int analyzerPixels = Math.Min(
                        NativeHighResolutionWaterfallMaxPixels,
                        Math.Max(width, width * NativeHighResolutionWaterfallOversample));

                    if (!_nativeHighResolutionWaterfallAdded[receiverId])
                    {
                        if (!_nativeHighResolutionWaterfallProcessor.AddReceiver(
                            receiverId,
                            analyzerPixels,
                            frameRate,
                            NativeHighResolutionWaterfallFftSize))
                        {
                            return false;
                        }

                        _nativeHighResolutionWaterfallAdded[receiverId] = true;
                        _nativeHighResolutionWaterfallWidth[receiverId] = analyzerPixels;
                    }
                    else if (_nativeHighResolutionWaterfallWidth[receiverId] != analyzerPixels)
                    {
                        _nativeHighResolutionWaterfallProcessor.SetReceiverPixelResolution(receiverId, analyzerPixels);
                        _nativeHighResolutionWaterfallWidth[receiverId] = analyzerPixels;
                    }

                    // Keep the independent analyzer locked to the exact RX viewport
                    // and waterfall detector/averaging policy while forcing the maximum
                    // native WDSP FFT size supported by the existing ChannelMaster path.
                    _nativeHighResolutionWaterfallProcessor.SetReceiverFrameRate(receiverId, frameRate);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverFFTSize(receiverId, NativeHighResolutionWaterfallFftSize);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverSampleRate(receiverId, sampleRate);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverZoomSlider(receiverId, source.ZoomSlider);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverPanSlider(receiverId, source.PanSlider);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverDetectorType(receiverId, source.DetTypeWF);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverAverageMode(receiverId, source.AverageModeWF);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverAverageTau(
                        receiverId,
                        source.AvTauWF > 0.0 ? source.AvTauWF : 0.12);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverAverageOn(receiverId, source.AverageOn);
                    _nativeHighResolutionWaterfallProcessor.SetReceiverPeakOn(receiverId, source.PeakOn);

                    if (_nativeHighResolutionWaterfallData[receiverId] == null ||
                        _nativeHighResolutionWaterfallData[receiverId].Length != width)
                    {
                        _nativeHighResolutionWaterfallData[receiverId] = new float[width];
                        _nativeHighResolutionWaterfallDataCopy[receiverId] = new float[width];
                    }

                    if (_nativeHighResolutionWaterfallOversampled[receiverId] == null ||
                        _nativeHighResolutionWaterfallOversampled[receiverId].Length != analyzerPixels)
                    {
                        _nativeHighResolutionWaterfallOversampled[receiverId] = new float[analyzerPixels];
                    }

                    int pixelCount;
                    int dataIndex;
                    if (!_nativeHighResolutionWaterfallProcessor.TryCopyReceiverPixels(
                        receiverId,
                        _nativeHighResolutionWaterfallOversampled[receiverId],
                        out pixelCount,
                        out dataIndex) ||
                        pixelCount != analyzerPixels)
                    {
                        return false;
                    }

                    // Peak-preserving horizontal reduction.  Each destination pixel
                    // keeps the strongest dBm sample from its exact source interval
                    // instead of averaging/interpolating it away.  This mirrors the
                    // important visual property of the former GPU waterfall's
                    // PeakHoldPower resampling without restoring that renderer.
                    float[] oversampled = _nativeHighResolutionWaterfallOversampled[receiverId];
                    float[] reduced = _nativeHighResolutionWaterfallData[receiverId];
                    float[] reducedCopy = _nativeHighResolutionWaterfallDataCopy[receiverId];

                    for (int x = 0; x < width; x++)
                    {
                        int start = (int)(((long)x * analyzerPixels) / width);
                        int end = (int)(((long)(x + 1) * analyzerPixels) / width);
                        if (end <= start) end = start + 1;
                        if (end > analyzerPixels) end = analyzerPixels;

                        float peak = oversampled[start];
                        for (int src = start + 1; src < end; src++)
                        {
                            if (oversampled[src] > peak)
                                peak = oversampled[src];
                        }

                        reduced[x] = peak;
                        reducedCopy[x] = peak;
                    }

                    data = reduced;
                    dataCopy = reducedCopy;
                    return true;
                }
                catch (Exception ex)
                {
                    Common.LogString("Native high-resolution waterfall fallback: " + ex.Message);
                    ShutdownNativeHighResolutionWaterfall();
                    _nativeHighResolutionWaterfallRetryAfterTicks =
                        DateTime.UtcNow.AddSeconds(2).Ticks;
                    return false;
                }
            }
        }

        private static void ShutdownNativeHighResolutionWaterfall()
        {
            lock (_nativeHighResolutionWaterfallLock)
            {
                if (_nativeHighResolutionWaterfallProcessor != null)
                {
                    try
                    {
                        _nativeHighResolutionWaterfallProcessor.Dispose();
                    }
                    catch
                    {
                    }

                    _nativeHighResolutionWaterfallProcessor = null;
                }

                for (int i = 0; i < 2; i++)
                {
                    _nativeHighResolutionWaterfallAdded[i] = false;
                    _nativeHighResolutionWaterfallWidth[i] = 0;
                    _nativeHighResolutionWaterfallData[i] = null;
                    _nativeHighResolutionWaterfallDataCopy[i] = null;
                    _nativeHighResolutionWaterfallOversampled[i] = null;
                }
            }
        }
    }
}
