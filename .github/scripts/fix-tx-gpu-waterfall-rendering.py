from pathlib import Path
import re

path = Path(r"Project Files/Source/Console/Display.GPUWaterfall.cs")
text = path.read_text(encoding="utf-8")

start_marker = "        private static bool UpdateManagedGPUWaterfallRenderer(int rx, int width, int height, int horizontalShiftPixels, bool addRow, bool clearExisting,"
end_marker = "        private static bool CanDrawManagedGPUWaterfall(int rx, ColorScheme scheme, int width, int height)"

if start_marker not in text:
    raise SystemExit("UpdateManagedGPUWaterfallRenderer start marker not found")
if end_marker not in text:
    raise SystemExit("CanDrawManagedGPUWaterfall end marker not found")

replacement = r'''        private static bool UpdateManagedGPUWaterfallRenderer(int rx, int width, int height, int horizontalShiftPixels, bool addRow, bool clearExisting,
            ColorScheme scheme, bool localMox, float lowThreshold, float highThreshold, float fOffset,
            GPUWaterfallPipeline pipeline, bool gpuRowReady, float gpuCalOffset)
        {
            int index = rx - 1;
            if (index < 0 || index > 1 || !ManagedGPUFFTRequested || !IsGPUWaterfallPaletteScheme(scheme))
            {
                if (index >= 0 && index < 2) _gpuRendererHasData[index] = false;
                return false;
            }

            // Preserve the native Stop RX Waterfall on TX option.  When it is enabled,
            // MOX freezes the existing GPU history exactly as the CPU waterfall does.
            if (localMox && ((rx == 1 && m_bStopRX1WaterfallOnTX) ||
                             (rx == 2 && m_bStopRX2WaterfallOnTX)))
            {
                return _gpuRendererHasData[index];
            }

            WaterfallGPURenderer renderer = EnsureGPUWaterfallRenderer(rx, width, height);
            if (renderer == null || !renderer.IsInitialized ||
                (!localMox && (pipeline == null || !pipeline.IsInitialized)))
            {
                // A temporary RX FFT failure invalidates the managed RX source, but a MOX
                // transition itself must never invalidate/clear the persistent waterfall texture.
                if (!localMox) _gpuRendererHasData[index] = false;
                return false;
            }

            bool paletteReady = true;
            if (scheme == ColorScheme.Custom)
            {
                System.Drawing.Color[] colours;
                bool ok;
                if (localMox)
                {
                    colours = _tx_waterfall_grad;
                    ok = _tx_waterfall_grad_ok;
                }
                else if (rx == 1)
                {
                    colours = _rx1_waterfall_grad;
                    ok = _rx1_waterfall_grad_ok;
                }
                else
                {
                    colours = _rx2_waterfall_grad;
                    ok = _rx2_waterfall_grad_ok;
                }

                if (!ok) paletteReady = false;
                else UploadCustomGradientToGPU(renderer, colours);
            }
            else
            {
                WaterfallPalette palette = GetGPUWaterfallPalette(scheme);
                if (palette == null) paletteReady = false; else UploadPaletteToGPU(renderer, palette);
            }
            if (!paletteReady)
            {
                // Do not destroy history merely because a TX custom palette is temporarily unavailable.
                if (!localMox) _gpuRendererHasData[index] = false;
                return false;
            }

            // RX uses the native managed GPU FFT SRV.  During MOX the normal Thetis display
            // pipeline already supplies the correct TX spectrum in current_waterfall_data*_copy.
            // Feed that TX spectrum into WaterfallGPURenderer.ProcessRow(float[]) so palette,
            // 8/16-bit conversion and history scrolling remain GPU-rendered while preserving
            // one continuous RX -> TX -> RX waterfall texture.
            int txDecimation = Math.Max(1, m_nDecimation);
            float[] txSpectrum = null;
            int txSpectrumLength = 0;
            if (localMox)
            {
                txSpectrum = rx == 1 ? current_waterfall_data_copy : current_waterfall_data_bottom_copy;
                if (txSpectrum != null)
                {
                    int expectedBins = Math.Max(1, width / txDecimation);
                    txSpectrumLength = Math.Min(txSpectrum.Length, expectedBins);
                }
            }

            bool rxInserted = !localMox && addRow && gpuRowReady &&
                              pipeline != null && pipeline.MagSpectrumView != null &&
                              !pipeline.MagSpectrumView.IsDisposed;
            bool txInserted = localMox && addRow && txSpectrum != null && txSpectrumLength > 0;
            bool inserted = rxInserted || txInserted;

            if (clearExisting || (inserted && !_gpuRendererHasData[index])) renderer.Clear();
            if (inserted)
            {
                float gamma = WaterfallEnhancer.Gamma;
                float invGamma = gamma != 0f ? 1f / gamma : 1f;
                GetEffectiveManagedGPUParams(rx, out int effectiveToneMap, out float effectiveTemporalAlpha);

                if (localMox)
                {
                    // CPU TX pixels are uncalibrated spectrum values.  fOffset is applied by
                    // moving the thresholds into the same value domain, exactly as on RX.
                    renderer.ProcessRow(txSpectrum, txSpectrumLength, txDecimation,
                        lowThreshold - fOffset, highThreshold - fOffset,
                        gamma, invGamma, effectiveToneMap,
                        WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.ContrastBoost,
                        WaterfallEnhancer.DitherEnabled, WaterfallEnhancer.Levels,
                        0f, 0.05f, true, scheme == ColorScheme.Custom,
                        WaterfallEnhancer.PaletteSharpness, WaterfallEnhancer.PaletteContrast);
                }
                else
                {
                    renderer.ProcessRow(pipeline.MagSpectrumView, width, 1,
                        lowThreshold - gpuCalOffset - fOffset, highThreshold - gpuCalOffset - fOffset,
                        gamma, invGamma, effectiveToneMap,
                        WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.ContrastBoost,
                        WaterfallEnhancer.DitherEnabled, WaterfallEnhancer.Levels,
                        0f, 0.05f, true, scheme == ColorScheme.Custom,
                        WaterfallEnhancer.PaletteSharpness, WaterfallEnhancer.PaletteContrast);
                }
            }

            renderer.AdvanceRow(horizontalShiftPixels, inserted);
            if (inserted)
            {
                _gpuRendererHasData[index] = true;
                recordWaterfallAdvance(rx, height);
            }
            return _gpuRendererHasData[index];
        }

        private static bool CanDrawManagedGPUWaterfall(int rx, ColorScheme scheme, int width, int height)'''

pattern = re.compile(
    re.escape(start_marker) + r".*?" + re.escape(end_marker),
    flags=re.S,
)
text2, count = pattern.subn(replacement, text, count=1)
if count != 1:
    raise SystemExit(f"Expected exactly one function replacement, got {count}")

required = [
    "txSpectrum = rx == 1 ? current_waterfall_data_copy : current_waterfall_data_bottom_copy;",
    "renderer.ProcessRow(txSpectrum, txSpectrumLength, txDecimation,",
    "colours = _tx_waterfall_grad;",
    "m_bStopRX1WaterfallOnTX",
    "pipeline.MagSpectrumView",
]
for token in required:
    if token not in text2:
        raise SystemExit(f"Post-patch verification failed: {token}")

# The previous temporary MOX freeze must be gone.
if "if (localMox)\n            {\n                return false;\n            }" in text2:
    raise SystemExit("Old localMox freeze is still present")

path.write_text(text2, encoding="utf-8")
print("TX GPU waterfall renderer patch applied and verified")
