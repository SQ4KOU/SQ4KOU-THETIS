using System;
using SharpDX.Direct2D1;

namespace Thetis
{
    partial class Display
    {
        private static float[] _nfScratch;
        private static float _autoHighRX1 = -40f;
        private static float _autoHighRX2 = -40f;
        private static readonly float[][] _temporalPrevRows = new float[2][];
        private static readonly bool[] _temporalPrevValidRows = new bool[2];

        internal static void DetectGPUCapabilitiesFromD2D()
        {
            try
            {
                SharpDX.Direct2D1.DeviceContext dc = _d2dRenderTarget as SharpDX.Direct2D1.DeviceContext;
                if (dc == null || dc.IsDisposed || _d2dFactory == null) return;
                GPUDetector.Detect(dc, _d2dFactory);
                if (_autoEnableGPU && !_gpuEffectsEnabled && GPUDetector.HasBuiltInEffects)
                {
                    _gpuEffectsEnabled = true;
                    LogTool.AddLogEntry("GPU post-processing enabled (Auto): Level " + (int)GPUDetector.Level, "D2D");
                }
            }
            catch (Exception ex)
            {
                LogTool.AddLogEntry("GPUDetector: " + ex.Message, "D2D");
            }
        }

        internal static void ResetTemporalWaterfallState()
        {
            _temporalPrevValidRows[0] = false;
            _temporalPrevValidRows[1] = false;
        }

        private static void GetEffectiveWaterfallProParams(int rx, out int toneMapMode, out float temporalAlpha)
        {
            if (_zoomAdaptiveEnabled)
            {
                int sampleRate = rx == 1 ? SampleRateRX1 : SampleRateRX2;
                ZoomAdaptive.ComputeParams(getWaterfallSpanHz(rx), sampleRate, (int)WaterfallEnhancer.ToneMap, out toneMapMode, out temporalAlpha);
            }
            else
            {
                toneMapMode = (int)WaterfallEnhancer.ToneMap;
                temporalAlpha = _temporalEnabled ? _temporalAlpha : 0f;
            }
        }

        internal static void GetEffectiveManagedGPUParams(int rx, out int toneMapMode, out float temporalAlpha)
        {
            GetEffectiveWaterfallProParams(rx, out toneMapMode, out temporalAlpha);
        }

        private static void ApplyWaterfallProPostProcessing(float[] rowF, int width, int rx)
        {
            if (rowF == null || width <= 0) return;
            int count = width * 4;
            if (rowF.Length < count) return;

            GetEffectiveWaterfallProParams(rx, out int effectiveToneMap, out float effectiveTemporalAlpha);
            int ti = rx == 2 ? 1 : 0;
            if (effectiveTemporalAlpha > 0f)
            {
                if (_temporalPrevRows[ti] == null || _temporalPrevRows[ti].Length < count)
                {
                    _temporalPrevRows[ti] = new float[count];
                    _temporalPrevValidRows[ti] = false;
                }
                float[] prev = _temporalPrevRows[ti];
                if (_temporalPrevValidRows[ti])
                {
                    for (int i = 0; i < count; i += 4)
                    {
                        float dr = rowF[i] - prev[i];
                        float dg = rowF[i + 1] - prev[i + 1];
                        float db = rowF[i + 2] - prev[i + 2];
                        float lumaDelta = 0.299f * dr + 0.587f * dg + 0.114f * db;
                        float motion2 = lumaDelta * lumaDelta;
                        float alpha = motion2 < 225f ? effectiveTemporalAlpha : effectiveTemporalAlpha * (15f / (float)Math.Sqrt(motion2));
                        float current = 1f - alpha;
                        rowF[i] = rowF[i] * current + prev[i] * alpha;
                        rowF[i + 1] = rowF[i + 1] * current + prev[i + 1] * alpha;
                        rowF[i + 2] = rowF[i + 2] * current + prev[i + 2] * alpha;
                        rowF[i + 3] = rowF[i + 3] * current + prev[i + 3] * alpha;
                    }
                }
                Array.Copy(rowF, prev, count);
                _temporalPrevValidRows[ti] = true;
            }
            else
            {
                _temporalPrevValidRows[ti] = false;
            }

            bool needSat = WaterfallEnhancer.SaturationBoost > 0f || WaterfallEnhancer.ContrastBoost > 0f;
            bool needToneMap = effectiveToneMap != 0;
            bool gpu8 = _gpuEffectsEnabled && WaterfallEnhancer.Depth == WaterfallEnhancer.ColorDepth.Bit8;
            bool customShader = gpu8 && GPUDetector.HasCustomShaders;
            bool builtInGpu = (gpu8 && GPUDetector.HasBuiltInEffects && !GPUDetector.HasCustomShaders) || customShader;
            bool cpuSat = needSat && !builtInGpu;
            bool cpuGamma = WaterfallEnhancer.Gamma != 1f && !builtInGpu;
            bool cpuToneMap = needToneMap && !customShader;
            bool cpuDither = WaterfallEnhancer.DitherEnabled && !customShader;

            if (cpuSat || cpuGamma || cpuToneMap || cpuDither)
            {
                int y = _ditherFrameY & 7;
                for (int x = 0; x < width; x++)
                {
                    int p = x * 4;
                    if (cpuSat) WaterfallEnhancer.ApplySaturationContrast(rowF, p);
                    if (cpuToneMap)
                    {
                        rowF[p] = WaterfallEnhancer.ApplyToneMapMode(rowF[p], effectiveToneMap);
                        rowF[p + 1] = WaterfallEnhancer.ApplyToneMapMode(rowF[p + 1], effectiveToneMap);
                        rowF[p + 2] = WaterfallEnhancer.ApplyToneMapMode(rowF[p + 2], effectiveToneMap);
                    }
                    if (cpuGamma)
                    {
                        rowF[p] = WaterfallEnhancer.ApplyGammaFloat(rowF[p]);
                        rowF[p + 1] = WaterfallEnhancer.ApplyGammaFloat(rowF[p + 1]);
                        rowF[p + 2] = WaterfallEnhancer.ApplyGammaFloat(rowF[p + 2]);
                    }
                    if (cpuDither)
                    {
                        rowF[p] = WaterfallEnhancer.ApplyDitherFloat(rowF[p], x, y);
                        rowF[p + 1] = WaterfallEnhancer.ApplyDitherFloat(rowF[p + 1], x, y);
                        rowF[p + 2] = WaterfallEnhancer.ApplyDitherFloat(rowF[p + 2], x, y);
                    }
                }
            }
        }

        private static float BlendWaterfallProAGC(float previous, float target)
        {
            return previous * (1f - _wfAgcSmoothing) + target * _wfAgcSmoothing;
        }

        private static void ApplyWaterfallProThresholds(int rx, bool localMox, ref float lowThreshold, ref float highThreshold)
        {
            if (localMox) return;
            if (rx == 2)
            {
                if (_autoThresholdEnabled && m_bNoiseFloorGoodRX2)
                {
                    float nf = m_fLerpAverageRX2 + _fNFshiftDBM;
                    float userOffset = rx2_waterfall_low_threshold - nf;
                    if (userOffset < 0f) userOffset = 0f;
                    if (userOffset > 10f) userOffset = 10f;
                    float range = rx2_waterfall_high_threshold - rx2_waterfall_low_threshold;
                    if (range < 10f) range = 10f;
                    lowThreshold = nf + userOffset + _autoThresholdFineOffset;
                    highThreshold = lowThreshold + range;
                }
                else if (_autoHighEnabledRX2)
                {
                    if (_autoHighRX2 <= -39f) _autoHighRX2 = rx2_waterfall_high_threshold;
                    float candidate = _autoHighRX2 + _autoHighMarginDb;
                    if (candidate < rx2_waterfall_high_threshold) candidate = rx2_waterfall_high_threshold;
                    highThreshold = candidate;
                }
                return;
            }

            if (_autoThresholdEnabled && m_bNoiseFloorGoodRX1)
            {
                float nf = m_fLerpAverageRX1 + _fNFshiftDBM;
                float userOffset = waterfall_low_threshold - nf;
                if (userOffset < 0f) userOffset = 0f;
                if (userOffset > 10f) userOffset = 10f;
                float range = waterfall_high_threshold - waterfall_low_threshold;
                if (range < 10f) range = 10f;
                lowThreshold = nf + userOffset + _autoThresholdFineOffset;
                highThreshold = lowThreshold + range;
            }
            else if (_autoHighEnabledRX1)
            {
                if (_autoHighRX1 <= -39f) _autoHighRX1 = waterfall_high_threshold;
                float candidate = _autoHighRX1 + _autoHighMarginDb;
                if (candidate < waterfall_high_threshold) candidate = waterfall_high_threshold;
                highThreshold = candidate;
            }
        }

        private static void DrawWaterfallToTarget(SharpDX.Direct2D1.Bitmap bmp, int nVerticalShift, int topMargin, float opacity)
        {
            SharpDX.Direct2D1.DeviceContext dc = _d2dRenderTarget as SharpDX.Direct2D1.DeviceContext;
            if (_gpuEffectsEnabled && dc != null && GPUDetector.HasBuiltInEffects && WaterfallEnhancer.Depth == WaterfallEnhancer.ColorDepth.Bit8)
            {
                int rx = object.ReferenceEquals(bmp, _waterfall_bmp2_dx2d) ? 2 : 1;
                GetEffectiveWaterfallProParams(rx, out int toneMapMode, out float temporalAlphaUnused);
                if (WaterfallEffect.Draw(dc, bmp, 0f, nVerticalShift + topMargin, opacity, 1f + WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.Gamma, toneMapMode, WaterfallEnhancer.DitherEnabled)) return;
                _gpuEffectsEnabled = false;
                LogTool.AddLogEntry("GPU draw failed, switched to CPU", "D2D");
            }
            _d2dRenderTarget.DrawBitmap(bmp, new SharpDX.RectangleF(0f, nVerticalShift + topMargin, bmp.Size.Width, bmp.Size.Height), opacity, _gpuWaterfallLinearDraw ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor);
        }
        private static void OnD2DDeviceContextRecreated(SharpDX.Direct2D1.DeviceContext dc)
        {
            if (dc == null || dc.IsDisposed) return;
            try { if (_waterfallGPU1 != null) _waterfallGPU1.UpdateDeviceContext(dc); } catch { }
            try { if (_waterfallGPU2 != null) _waterfallGPU2.UpdateDeviceContext(dc); } catch { }
            WaterfallEffect.Reset();
            DetectGPUCapabilitiesFromD2D();
        }

        private static void ShutdownManagedGPUWaterfallResources()
        {
            try { if (_waterfallGPU1 != null) _waterfallGPU1.Dispose(); } catch { }
            _waterfallGPU1 = null;
            try { if (_waterfallGPU2 != null) _waterfallGPU2.Dispose(); } catch { }
            _waterfallGPU2 = null;
            try { if (_gpuFFT1 != null) _gpuFFT1.Dispose(); } catch { }
            _gpuFFT1 = null;
            try { if (_gpuFFT2 != null) _gpuFFT2.Dispose(); } catch { }
            _gpuFFT2 = null;
            _gpuEffectsEnabled = false;
            ResetTemporalWaterfallState();
            WaterfallEffect.Reset();
        }

    }
}
