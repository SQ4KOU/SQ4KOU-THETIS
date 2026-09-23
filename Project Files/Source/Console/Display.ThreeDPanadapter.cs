using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using Color = System.Drawing.Color;

namespace Thetis
{
    // SQ4KOU: safe 3D panadapter port.
    // This is intentionally isolated from GPUWaterfallPipeline/WaterfallGPURenderer.
    // It consumes the existing RX panadapter FFT data and renders through the already
    // active SharpDX Direct2D target. No Vortice/.NET 10 dependency is introduced.
    partial class Display
    {
        public const int Max3DHistoryLines = 60;

        private static float[][] _3dHistoryBuffer;
        private static float[][] _3dMedianPrev;
        private static float[][] _3dLerpRows;
        private static float[] _3dRowLift;
        private static int _3dHistoryCount;
        private static int _3dHistoryHead;
        private static int _3dMedianCount;
        private static long _3dLastPushTicks;
        private static long _3dPushIntervalTicks = 400000; // 25 FPS, DateTime ticks

        private static bool _pan3DEnabled;
        private static float _pan3DPerspective = 0.60f;
        private static float _pan3DDepth = 0.58f;
        private static float _pan3DDepthFade = 0.16f;
        private static float _pan3DRidgeHeight = 0.46f;
        private static float _pan3DZCurve = 0.90f;
        private static int _pan3DLineCount = 35;
        private static int _pan3DColorMap;
        private static bool _pan3DSideWalls = true;
        private static bool _pan3DWaterfallSync = true;
        private static Color _pan3DLineColor = Color.Aquamarine;
        private static bool _pan3DFillColorEnabled;
        private static Color _pan3DFillColor = Color.Aquamarine;
        private static float _pan3DFillAlpha = 0.55f;
        private static byte[] _pan3DColorLut;

        public static bool Pan3DEnabled
        {
            get { return _pan3DEnabled; }
            set
            {
                if (_pan3DEnabled == value) return;
                _pan3DEnabled = value;
                if (!value) Reset3DHistory();
                else Ensure3DHistoryBuffers();
            }
        }

        public static float Pan3DPerspective
        {
            get { return _pan3DPerspective; }
            set { _pan3DPerspective = Math.Max(0.10f, Math.Min(1.0f, value)); }
        }

        public static float Pan3DDepth
        {
            get { return _pan3DDepth; }
            set { _pan3DDepth = Math.Max(0.0f, Math.Min(1.0f, value)); }
        }

        public static float Pan3DDepthFade
        {
            get { return _pan3DDepthFade; }
            set { _pan3DDepthFade = Math.Max(0.0f, Math.Min(1.0f, value)); }
        }

        public static float Pan3DRidgeHeight
        {
            get { return _pan3DRidgeHeight; }
            set { _pan3DRidgeHeight = Math.Max(0.10f, Math.Min(1.0f, value)); }
        }

        public static float Pan3DZCurve
        {
            get { return _pan3DZCurve; }
            set { _pan3DZCurve = Math.Max(0.05f, Math.Min(1.0f, value)); }
        }

        public static int Pan3DLineCount
        {
            get { return _pan3DLineCount; }
            set { _pan3DLineCount = Math.Max(2, Math.Min(Max3DHistoryLines, value)); }
        }

        public static int Pan3DSpeed
        {
            get
            {
                long v = _3dPushIntervalTicks <= 0 ? 400000 : _3dPushIntervalTicks;
                return (int)(10000000L / v);
            }
            set { _3dPushIntervalTicks = 10000000L / Math.Max(1, Math.Min(60, value)); }
        }

        public static int Pan3DColorMap
        {
            get { return _pan3DColorMap; }
            set { _pan3DColorMap = Math.Max(0, Math.Min(3, value)); }
        }

        public static bool Pan3DSideWalls
        {
            get { return _pan3DSideWalls; }
            set { _pan3DSideWalls = value; }
        }

        public static bool Pan3DWaterfallSync
        {
            get { return _pan3DWaterfallSync; }
            set { _pan3DWaterfallSync = value; }
        }

        public static Color Pan3DLineColor
        {
            get { return _pan3DLineColor; }
            set { _pan3DLineColor = value; }
        }

        public static bool Pan3DFillColorEnabled
        {
            get { return _pan3DFillColorEnabled; }
            set { _pan3DFillColorEnabled = value; }
        }

        public static Color Pan3DFillColor
        {
            get { return _pan3DFillColor; }
            set { _pan3DFillColor = value; }
        }

        public static float Pan3DFillAlpha
        {
            get { return _pan3DFillAlpha; }
            set { _pan3DFillAlpha = Math.Max(0.0f, Math.Min(1.0f, value)); }
        }

        private static void Ensure3DHistoryBuffers()
        {
            if (_3dHistoryBuffer == null || _3dHistoryBuffer.Length != Max3DHistoryLines)
            {
                _3dHistoryBuffer = new float[Max3DHistoryLines][];
                for (int i = 0; i < Max3DHistoryLines; i++) _3dHistoryBuffer[i] = new float[1];
            }

            if (_3dMedianPrev == null || _3dMedianPrev.Length != 2)
            {
                _3dMedianPrev = new float[2][];
                _3dMedianPrev[0] = new float[1];
                _3dMedianPrev[1] = new float[1];
            }

            if (_3dLerpRows == null || _3dLerpRows.Length != Max3DHistoryLines)
                _3dLerpRows = new float[Max3DHistoryLines][];
        }

        private static void Reset3DHistory()
        {
            _3dHistoryCount = 0;
            _3dHistoryHead = 0;
            _3dMedianCount = 0;
            _3dLastPushTicks = 0;
        }

        internal static void Capture3DHistoryFrame(float[] source, int count, bool localMox)
        {
            if (!_pan3DEnabled || localMox || source == null || count < 2) return;

            Ensure3DHistoryBuffers();

            long now = DateTime.UtcNow.Ticks;
            long interval = Math.Max(10000L, _3dPushIntervalTicks);
            if (_3dLastPushTicks != 0 && now - _3dLastPushTicks < interval) return;

            try
            {
                int head = _3dHistoryHead;
                if (_3dHistoryBuffer[head] == null || _3dHistoryBuffer[head].Length < count)
                    _3dHistoryBuffer[head] = new float[count];

                if (_3dMedianPrev[0] == null || _3dMedianPrev[0].Length < count)
                    _3dMedianPrev[0] = new float[count];
                if (_3dMedianPrev[1] == null || _3dMedianPrev[1].Length < count)
                    _3dMedianPrev[1] = new float[count];

                if (_3dMedianCount >= 2)
                {
                    float[] dst = _3dHistoryBuffer[head];
                    float[] p0 = _3dMedianPrev[0];
                    float[] p1 = _3dMedianPrev[1];
                    for (int i = 0; i < count; i++)
                    {
                        float a = source[i];
                        float b = p0[i];
                        float c = p1[i];
                        dst[i] = Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
                    }
                }
                else
                {
                    Array.Copy(source, _3dHistoryBuffer[head], count);
                }

                Array.Copy(_3dMedianPrev[0], _3dMedianPrev[1], count);
                Array.Copy(source, _3dMedianPrev[0], count);
                if (_3dMedianCount < int.MaxValue) _3dMedianCount++;

                _3dHistoryHead = (head + 1) % Max3DHistoryLines;
                if (_3dHistoryCount < Max3DHistoryLines) _3dHistoryCount++;
                _3dLastPushTicks = now;
            }
            catch (Exception ex)
            {
                Common.LogString("SQ4KOU 3D history capture skipped: " + ex.Message);
            }
        }

        public static void RestorePan3DPersisted()
        {
            if (DB.ds == null) return;
            try
            {
                List<string> vars = DB.GetVars("3DPanadapter").OfType<string>().ToList();
                foreach (string entry in vars)
                {
                    if (String.IsNullOrEmpty(entry)) continue;
                    int sep = entry.IndexOf('/');
                    if (sep <= 0) continue;

                    string name = entry.Substring(0, sep);
                    string value = entry.Substring(sep + 1);

                    bool bv;
                    int iv;
                    float fv;
                    switch (name)
                    {
                        case "chk3DEnabled":
                            if (Boolean.TryParse(value, out bv)) Pan3DEnabled = bv;
                            break;
                        case "chk3DWaterfallSync":
                            if (Boolean.TryParse(value, out bv)) Pan3DWaterfallSync = bv;
                            break;
                        case "chk3DSideWalls":
                            if (Boolean.TryParse(value, out bv)) Pan3DSideWalls = bv;
                            break;
                        case "ud3DXOffset":
                            if (TryParse3DFloat(value, out fv)) Pan3DPerspective = fv;
                            break;
                        case "ud3DYOffset":
                            if (TryParse3DFloat(value, out fv)) Pan3DDepth = fv;
                            break;
                        case "ud3DRidgeHeight":
                            if (TryParse3DFloat(value, out fv)) Pan3DRidgeHeight = fv;
                            break;
                        case "ud3DHaze":
                            if (TryParse3DFloat(value, out fv)) Pan3DDepthFade = fv;
                            break;
                        case "ud3DLineCount":
                            if (Int32.TryParse(value, out iv)) Pan3DLineCount = iv;
                            break;
                        case "ud3DSpeed":
                            if (Int32.TryParse(value, out iv)) Pan3DSpeed = iv;
                            break;
                        case "ud3DZCurve":
                            if (TryParse3DFloat(value, out fv)) Pan3DZCurve = fv;
                            break;
                        case "combo3DColorMap":
                            if (String.Equals(value, "Turbo", StringComparison.OrdinalIgnoreCase)) Pan3DColorMap = 1;
                            else if (String.Equals(value, "Viridis", StringComparison.OrdinalIgnoreCase)) Pan3DColorMap = 2;
                            else if (String.Equals(value, "Inferno", StringComparison.OrdinalIgnoreCase)) Pan3DColorMap = 3;
                            else Pan3DColorMap = 0;
                            break;
                        case "chk3DFillColorEnable":
                            if (Boolean.TryParse(value, out bv)) Pan3DFillColorEnabled = bv;
                            break;
                        case "tb3DFillOpacity":
                            if (Int32.TryParse(value, out iv)) Pan3DFillAlpha = iv / 100.0f;
                            break;
                        case "clrbtn3DLineColor":
                            Pan3DLineColor = Parse3DColor(value, Pan3DLineColor);
                            break;
                        case "clrbtn3DFillColor":
                            Pan3DFillColor = Parse3DColor(value, Pan3DFillColor);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
        }

        private static bool TryParse3DFloat(string value, out float result)
        {
            return Single.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ||
                   Single.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
        }

        private static Color Parse3DColor(string value, Color fallback)
        {
            try
            {
                string[] p = value.Split('.');
                if (p.Length != 4) return fallback;
                int r, g, b, a;
                if (!Int32.TryParse(p[0], out r) || !Int32.TryParse(p[1], out g) ||
                    !Int32.TryParse(p[2], out b) || !Int32.TryParse(p[3], out a)) return fallback;
                return Color.FromArgb(a, r, g, b);
            }
            catch { return fallback; }
        }

        private static float Lift3D(float strength)
        {
            float s = Math.Max(0f, Math.Min(1f, strength));
            return (float)Math.Pow(s, Math.Max(0.05f, _pan3DZCurve));
        }

        private static bool Get3DWaterfallThresholds(int rx, out float low, out float high)
        {
            if (rx == 2)
            {
                high = rx2_waterfall_high_threshold;
                low = rx2_waterfall_low_threshold;
                if (rx2_waterfall_agc && !m_bRX2_spectrum_thresholds)
                    low = _RX2waterfallPreviousMinValue - m_fWaterfallAGCOffsetRX2;
            }
            else
            {
                high = waterfall_high_threshold;
                low = waterfall_low_threshold;
                if (rx1_waterfall_agc && !m_bRX1_spectrum_thresholds)
                    low = _RX1waterfallPreviousMinValue - m_fWaterfallAGCOffsetRX1;
            }
            return high > low;
        }

        private static void Get3DWaterfallColor(int rx, float dBm, float low, float high, out int r, out int g, out int b)
        {
            ColorScheme scheme = rx == 2 ? _rx2_color_scheme : _rx1_color_scheme;
            Color lowColor = rx == 2 ? rx2_waterfall_low_color : waterfall_low_color;
            Color[] grad = rx == 2 ? _rx2_waterfall_grad : _rx1_waterfall_grad;
            bool gradOk = rx == 2 ? _rx2_waterfall_grad_ok : _rx1_waterfall_grad_ok;

            float pct = dBm <= low ? 0f : dBm >= high ? 1f : (dBm - low) / (high - low);

            if (scheme == ColorScheme.Custom && gradOk && grad != null && grad.Length > 1)
            {
                int idx = (int)(pct * (grad.Length - 1));
                idx = Math.Max(0, Math.Min(grad.Length - 1, idx));
                r = grad[idx].R; g = grad[idx].G; b = grad[idx].B;
                return;
            }

            WaterfallPalette palette = null;
            if (scheme == ColorScheme.Console) palette = GetPaletteConsole();
            else if (scheme == ColorScheme.Thermal) palette = GetPaletteThermal();
            else if (scheme == ColorScheme.DeepBlue) palette = GetPaletteDeepBlue();
            else if (scheme == ColorScheme.Enhanced256) palette = GetPaletteEnhanced256();
            else if (scheme == ColorScheme.Grayscale256) palette = GetPaletteGrayscale256();

            if (palette != null)
            {
                float rf, gf, bf;
                palette.Sample(pct, out rf, out gf, out bf);
                r = (int)Math.Max(0, Math.Min(255, rf));
                g = (int)Math.Max(0, Math.Min(255, gf));
                b = (int)Math.Max(0, Math.Min(255, bf));
                return;
            }

            if (scheme == ColorScheme.enhanced)
            {
                if (dBm <= low)
                {
                    r = lowColor.R; g = lowColor.G; b = lowColor.B; return;
                }
                if (dBm >= high)
                {
                    r = 192; g = 124; b = 255; return;
                }

                if (pct < 2f / 9f)
                {
                    float q = pct / (2f / 9f);
                    r = (int)((1f - q) * lowColor.R);
                    g = (int)((1f - q) * lowColor.G);
                    b = (int)(lowColor.B + q * (255 - lowColor.B));
                }
                else if (pct < 3f / 9f)
                {
                    float q = (pct - 2f / 9f) / (1f / 9f);
                    r = 0; g = (int)(q * 255); b = 255;
                }
                else if (pct < 4f / 9f)
                {
                    float q = (pct - 3f / 9f) / (1f / 9f);
                    r = 0; g = 255; b = (int)((1f - q) * 255);
                }
                else if (pct < 5f / 9f)
                {
                    float q = (pct - 4f / 9f) / (1f / 9f);
                    r = (int)(q * 255); g = 255; b = 0;
                }
                else if (pct < 7f / 9f)
                {
                    float q = (pct - 5f / 9f) / (2f / 9f);
                    r = 255; g = (int)((1f - q) * 255); b = 0;
                }
                else if (pct < 8f / 9f)
                {
                    float q = (pct - 7f / 9f) / (1f / 9f);
                    r = 255; g = 0; b = (int)(q * 255);
                }
                else
                {
                    float q = (pct - 8f / 9f) / (1f / 9f);
                    r = (int)((0.75f + 0.25f * (1f - q)) * 255);
                    g = (int)(q * 127.5f);
                    b = 255;
                }
                return;
            }

            if (scheme == ColorScheme.SPECTRAN)
            {
                float lp = pct * 100f;
                if (lp < 22f) { r = 0; g = 0; b = (int)lp * 5; }
                else if (lp < 51f) { r = 0; g = 0; b = Math.Min(255, (int)lp * 5); }
                else if (lp < 66f) { r = 0; g = (int)((lp - 51f) * 6.93f); b = 255; }
                else if (lp < 78f) { r = (int)((lp - 66f) * 8.5f); g = 255; b = 255 - (int)((lp - 66f) * 8.5f); }
                else { r = 255; g = 255 - (int)((lp - 78f) * 11.36f); b = 0; }
                return;
            }

            int v = (int)(pct * 255f);
            r = v; g = v; b = v;
        }

        private static void Build3DColorLut()
        {
            byte[] lut = new byte[3 * 256 * 3];
            for (int map = 0; map < 3; map++)
            {
                for (int i = 0; i < 256; i++)
                {
                    float s = i / 255f;
                    float r, g, b;
                    if (map == 0) Turbo3D(s, out r, out g, out b);
                    else if (map == 1) Viridis3D(s, out r, out g, out b);
                    else Inferno3D(s, out r, out g, out b);

                    int o = (map * 256 + i) * 3;
                    lut[o] = (byte)(Math.Max(0f, Math.Min(1f, r)) * 255f + 0.5f);
                    lut[o + 1] = (byte)(Math.Max(0f, Math.Min(1f, g)) * 255f + 0.5f);
                    lut[o + 2] = (byte)(Math.Max(0f, Math.Min(1f, b)) * 255f + 0.5f);
                }
            }
            _pan3DColorLut = lut;
        }

        private static void Turbo3D(float s, out float r, out float g, out float b)
        {
            float s2 = s * s, s3 = s2 * s, s4 = s3 * s, s5 = s4 * s;
            r = 0.13572138f + 4.61539260f * s - 42.66032258f * s2 + 132.13108234f * s3 - 152.94239396f * s4 + 59.28637943f * s5;
            g = 0.09140261f + 2.19418839f * s + 4.84296658f * s2 - 14.18503333f * s3 + 4.27729857f * s4 + 2.82956604f * s5;
            b = 0.10667330f + 12.64194608f * s - 60.58204836f * s2 + 110.36276771f * s3 - 89.90310912f * s4 + 27.34824973f * s5;
        }

        private static void Viridis3D(float s, out float r, out float g, out float b)
        {
            float s2 = s * s, s3 = s2 * s, s4 = s3 * s, s5 = s4 * s;
            r = 0.28115018f - 0.38551194f * s + 0.47364613f * s2 + 1.41630094f * s3 - 2.13771225f * s4 + 1.35253483f * s5;
            g = 0.01335544f + 1.23019548f * s - 0.99407731f * s2 + 0.42095064f * s3 + 0.32516897f * s4 - 0.11355376f * s5;
            b = 0.31013484f + 1.65444245f * s - 2.79180835f * s2 + 3.40206332f * s3 - 2.77442300f * s4 + 1.19994366f * s5;
        }

        private static readonly float[] _inferno3DStops =
        {
            0.0f, 0f, 0f, 4f, 0.1f, 22f, 11f, 57f, 0.2f, 66f, 10f, 104f,
            0.3f, 106f, 23f, 110f, 0.4f, 147f, 38f, 103f, 0.5f, 188f, 55f, 84f,
            0.6f, 221f, 81f, 58f, 0.7f, 243f, 120f, 25f, 0.8f, 252f, 165f, 10f,
            0.9f, 246f, 215f, 70f, 1.0f, 252f, 255f, 164f
        };

        private static void Inferno3D(float s, out float r, out float g, out float b)
        {
            if (s <= 0f) { r = 0f; g = 0f; b = 4f / 255f; return; }
            if (s >= 1f) { r = 252f / 255f; g = 1f; b = 164f / 255f; return; }

            float fs = s * 10f;
            int i0 = (int)fs;
            if (i0 > 9) i0 = 9;
            float t = fs - i0;
            int o0 = i0 * 4;
            int o1 = o0 + 4;
            r = (_inferno3DStops[o0 + 1] + (_inferno3DStops[o1 + 1] - _inferno3DStops[o0 + 1]) * t) / 255f;
            g = (_inferno3DStops[o0 + 2] + (_inferno3DStops[o1 + 2] - _inferno3DStops[o0 + 2]) * t) / 255f;
            b = (_inferno3DStops[o0 + 3] + (_inferno3DStops[o1 + 3] - _inferno3DStops[o0 + 3]) * t) / 255f;
        }

        private static void Select3DSurfaceColor(int rx, float dBm, float strength, bool waterfallSync,
            float wfLow, float wfHigh, Color[] gradient, out int r, out int g, out int b)
        {
            if (waterfallSync)
            {
                Get3DWaterfallColor(rx, dBm, wfLow, wfHigh, out r, out g, out b);
                return;
            }

            if (_pan3DColorMap > 0)
            {
                if (_pan3DColorLut == null) Build3DColorLut();
                int ci = Math.Max(0, Math.Min(255, (int)(strength * 255f)));
                int o = ((_pan3DColorMap - 1) * 256 + ci) * 3;
                r = _pan3DColorLut[o];
                g = _pan3DColorLut[o + 1];
                b = _pan3DColorLut[o + 2];
                return;
            }

            if (gradient != null)
            {
                int idx = Math.Max(0, Math.Min(gradient.Length - 1, (int)(strength * (gradient.Length - 1))));
                r = gradient[idx].R;
                g = gradient[idx].G;
                b = gradient[idx].B;
                return;
            }

            float bright = 0.25f + 0.75f * strength;
            r = (int)(_pan3DLineColor.R * bright);
            g = (int)(_pan3DLineColor.G * bright);
            b = (int)(_pan3DLineColor.B * bright);
        }

        internal static void DrawPanadapter3DHistoryDX2D(int nVerticalShift, int W, int H, int rx,
            int gridMax, int nDecimatedWidth, int localDecimation)
        {
            if (!_pan3DEnabled || rx != 1 || _3dHistoryBuffer == null || _3dHistoryCount < 2) return;
            if (_d2dRenderTarget == null || nDecimatedWidth < 2 || H < 20 || W < 20) return;

            int linesToDraw = Math.Min(_3dHistoryCount, _pan3DLineCount);
            if (linesToDraw < 2) return;

            int gridMin = spectrum_grid_min;
            int yRange = gridMax - gridMin;
            if (yRange <= 0) return;

            int bgR = (int)(m_cDX2_display_background_clear_colour.Red * 255f);
            int bgG = (int)(m_cDX2_display_background_clear_colour.Green * 255f);
            int bgB = (int)(m_cDX2_display_background_clear_colour.Blue * 255f);

            bool wfSync = _pan3DWaterfallSync && Get3DWaterfallThresholds(rx, out float wfLow, out float wfHigh);

            Color[] gradient = null;
            if (!wfSync && _pan3DColorMap == 0 && m_bUseLinearGradient && console.SetupForm != null && console.SetupForm.RX1GradPicker != null)
            {
                try
                {
                    gradient = new Color[64];
                    for (int i = 0; i < gradient.Length; i++)
                    {
                        float dbm = gridMin + (i / 63f) * yRange;
                        gradient[i] = console.SetupForm.RX1GradPicker.GetColourForDBM(dbm);
                    }
                }
                catch { gradient = null; }
            }

            float bottomY = nVerticalShift + H;
            float depthSpan = H * _pan3DDepth;
            float frontRidge = H * _pan3DRidgeHeight;
            float backWidth = _pan3DPerspective;
            float phase = 0f;
            if (_3dLastPushTicks > 0)
            {
                long interval = Math.Max(10000L, _3dPushIntervalTicks);
                phase = (DateTime.UtcNow.Ticks - _3dLastPushTicks) / (float)interval;
                phase = Math.Max(0f, Math.Min(1f, phase));
            }

            if (_3dLerpRows == null || _3dLerpRows.Length < Max3DHistoryLines)
                _3dLerpRows = new float[Max3DHistoryLines][];

            int rowCount = linesToDraw - 1;
            float[] rowDepth = new float[rowCount];
            float[] rowInset = new float[rowCount];
            float[] rowBaseline = new float[rowCount];
            float[] rowRidge = new float[rowCount];
            float[] edgeLeft = new float[rowCount];
            float[] edgeRight = new float[rowCount];
            float[][] rowSource = new float[linesToDraw][];

            for (int i = 0; i < rowCount; i++)
            {
                int line = i + 1;
                float d = line / (float)(linesToDraw - 1);
                rowDepth[i] = d;
                float widthFrac = 1f - d * (1f - backWidth);
                rowInset[i] = W * (1f - widthFrac) * 0.5f;
                rowBaseline[i] = bottomY - d * depthSpan;
                rowRidge[i] = frontRidge * widthFrac;

                float fi = line - phase;
                int i0 = Math.Max(0, (int)fi);
                float w = fi - i0;
                int i1 = i0 + 1;
                if (i1 > linesToDraw - 1) { i1 = i0; w = 0f; }

                int idx0 = (_3dHistoryHead - 1 - i0 + Max3DHistoryLines * 2) % Max3DHistoryLines;
                int idx1 = (_3dHistoryHead - 1 - i1 + Max3DHistoryLines * 2) % Max3DHistoryLines;
                float[] f0 = _3dHistoryBuffer[idx0];
                float[] f1 = _3dHistoryBuffer[idx1];
                if (f0 == null || f0.Length < nDecimatedWidth) continue;

                if (w > 0.0001f && f1 != null && f1.Length >= nDecimatedWidth)
                {
                    float[] dst = _3dLerpRows[line];
                    if (dst == null || dst.Length < nDecimatedWidth)
                        dst = _3dLerpRows[line] = new float[nDecimatedWidth];
                    for (int c = 0; c < nDecimatedWidth; c++) dst[c] = f0[c] + (f1[c] - f0[c]) * w;
                    rowSource[line] = dst;
                }
                else rowSource[line] = f0;

                float sl = (rowSource[line][0] - gridMin) / yRange;
                float sr = (rowSource[line][nDecimatedWidth - 1] - gridMin) / yRange;
                edgeLeft[i] = Math.Max(nVerticalShift, Math.Min(bottomY, rowBaseline[i] - Lift3D(sl) * rowRidge[i]));
                edgeRight[i] = Math.Max(nVerticalShift, Math.Min(bottomY, rowBaseline[i] - Lift3D(sr) * rowRidge[i]));
            }

            var brushes = new Dictionary<int, SolidColorBrush>();
            RectangleF clipRect = new RectangleF(0, nVerticalShift, W, H);
            _d2dRenderTarget.PushAxisAlignedClip(clipRect, AntialiasMode.Aliased);

            try
            {
                // Perspective floor grid and side rails.
                for (int g = 0; g <= 5; g++)
                {
                    float d = g / 5f;
                    float widthFrac = 1f - d * (1f - backWidth);
                    float inset = W * (1f - widthFrac) * 0.5f;
                    float y = bottomY - d * depthSpan;
                    SolidColorBrush gb = Get3DBrush(brushes, _pan3DLineColor.R, _pan3DLineColor.G, _pan3DLineColor.B, 0.03f + 0.10f * (1f - d));
                    _d2dRenderTarget.DrawLine(new Vector2(inset, y), new Vector2(W - inset, y), gb, 1f);
                }

                float bi = W * (1f - backWidth) * 0.5f;
                SolidColorBrush rail = Get3DBrush(brushes, _pan3DLineColor.R, _pan3DLineColor.G, _pan3DLineColor.B, 0.12f);
                _d2dRenderTarget.DrawLine(new Vector2(0, bottomY), new Vector2(bi, bottomY - depthSpan), rail, 1f);
                _d2dRenderTarget.DrawLine(new Vector2(W, bottomY), new Vector2(W - bi, bottomY - depthSpan), rail, 1f);

                // Solid side walls.
                if (_pan3DSideWalls && rowCount > 1 && rowSource[1] != null)
                {
                    int wr, wg, wb;
                    float edgeStrength = Math.Max(0f, Math.Min(1f, (rowSource[1][0] - gridMin) / yRange));
                    Select3DSurfaceColor(rx, rowSource[1][0], edgeStrength, wfSync, wfLow, wfHigh, gradient, out wr, out wg, out wb);
                    wr = (int)(wr * 0.32f); wg = (int)(wg * 0.32f); wb = (int)(wb * 0.32f);
                    SolidColorBrush wall = Get3DBrush(brushes, wr, wg, wb, 0.92f);
                    Fill3DSideWall(true, rowCount, rowInset, edgeLeft, bottomY, W, wall);
                    Fill3DSideWall(false, rowCount, rowInset, edgeRight, bottomY, W, wall);
                }

                for (int ri = rowCount - 1; ri >= 0; ri--)
                {
                    int line = ri + 1;
                    float[] frame = rowSource[line];
                    if (frame == null || frame.Length < nDecimatedWidth) continue;

                    if (_3dRowLift == null || _3dRowLift.Length < nDecimatedWidth)
                        _3dRowLift = new float[nDecimatedWidth];

                    for (int c = 0; c < nDecimatedWidth; c++)
                    {
                        float s = (frame[c] - gridMin) / yRange;
                        _3dRowLift[c] = Lift3D(s);
                    }

                    float d = rowDepth[ri];
                    float inset = rowInset[ri];
                    float rowW = W - 2f * inset;
                    float baseY = rowBaseline[ri];
                    float ridge = rowRidge[ri];
                    float dim = 0.72f + 0.28f * (1f - d);
                    float haze = d * _pan3DDepthFade;
                    float alpha = 1f - d * 0.15f;

                    for (int c = 0; c < nDecimatedWidth; c++)
                    {
                        float x = inset + (c / (float)(nDecimatedWidth - 1)) * rowW;
                        float dbm = frame[c];
                        float strength = Math.Max(0f, Math.Min(1f, (dbm - gridMin) / yRange));
                        float y = Math.Max(nVerticalShift, Math.Min(bottomY, baseY - _3dRowLift[c] * ridge));

                        int r, g, b;
                        Select3DSurfaceColor(rx, dbm, strength, wfSync, wfLow, wfHigh, gradient, out r, out g, out b);
                        r = (int)(r * dim); g = (int)(g * dim); b = (int)(b * dim);

                        int cl = c > 0 ? c - 1 : c;
                        int cr = c < nDecimatedWidth - 1 ? c + 1 : c;
                        float shade = 1f + 0.55f * (_3dRowLift[cl] - _3dRowLift[cr]);
                        shade = Math.Max(0.68f, Math.Min(1.32f, shade));
                        r = (int)(r * shade); g = (int)(g * shade); b = (int)(b * shade);

                        r = Clamp3D((int)(r * (1f - haze) + bgR * haze));
                        g = Clamp3D((int)(g * (1f - haze) + bgG * haze));
                        b = Clamp3D((int)(b * (1f - haze) + bgB * haze));

                        SolidColorBrush fill = Get3DBrush(brushes, r, g, b, alpha);
                        _d2dRenderTarget.DrawLine(new Vector2(x, bottomY), new Vector2(x, y), fill, Math.Max(1f, localDecimation));
                    }

                    Vector2 prev = new Vector2();
                    bool havePrev = false;
                    for (int c = 0; c < nDecimatedWidth; c++)
                    {
                        float x = inset + (c / (float)(nDecimatedWidth - 1)) * rowW;
                        float dbm = frame[c];
                        float strength = Math.Max(0f, Math.Min(1f, (dbm - gridMin) / yRange));
                        float y = Math.Max(nVerticalShift, Math.Min(bottomY, baseY - _3dRowLift[c] * ridge));

                        int r, g, b;
                        Select3DSurfaceColor(rx, dbm, strength, wfSync, wfLow, wfHigh, gradient, out r, out g, out b);
                        float bright = 0.35f + 0.65f * strength;
                        r = Clamp3D((int)(r * bright * dim * 1.35f));
                        g = Clamp3D((int)(g * bright * dim * 1.35f));
                        b = Clamp3D((int)(b * bright * dim * 1.35f));
                        float oa = Math.Max(0.10f, (1f - d * 0.4f) * 0.85f);

                        Vector2 pt = new Vector2(x, y);
                        if (havePrev)
                        {
                            SolidColorBrush outline = Get3DBrush(brushes, r, g, b, oa);
                            _d2dRenderTarget.DrawLine(prev, pt, outline, 1f);
                        }
                        prev = pt;
                        havePrev = true;
                    }
                }
            }
            finally
            {
                try { _d2dRenderTarget.PopAxisAlignedClip(); } catch { }
                foreach (SolidColorBrush brush in brushes.Values)
                {
                    try { brush.Dispose(); } catch { }
                }
            }
        }

        private static int Clamp3D(int v)
        {
            return v < 0 ? 0 : v > 255 ? 255 : v;
        }

        private static SolidColorBrush Get3DBrush(Dictionary<int, SolidColorBrush> cache, int r, int g, int b, float alpha)
        {
            r = Clamp3D(r); g = Clamp3D(g); b = Clamp3D(b);
            alpha = Math.Max(0f, Math.Min(1f, alpha));
            int a = Clamp3D((int)(alpha * 255f));
            int key = (a << 24) | (r << 16) | (g << 8) | b;
            SolidColorBrush brush;
            if (!cache.TryGetValue(key, out brush))
            {
                brush = new SolidColorBrush(_d2dRenderTarget, new Color4(r / 255f, g / 255f, b / 255f, alpha));
                cache[key] = brush;
            }
            return brush;
        }

        private static void Fill3DSideWall(bool left, int rowCount, float[] inset, float[] edge, float bottomY, int width, SolidColorBrush brush)
        {
            using (PathGeometry geo = new PathGeometry(_d2dFactory))
            using (GeometrySink sink = geo.Open())
            {
                float x0 = left ? inset[0] : width - inset[0];
                sink.BeginFigure(new Vector2(x0, edge[0]), FigureBegin.Filled);
                for (int i = 1; i < rowCount; i++)
                {
                    float x = left ? inset[i] : width - inset[i];
                    sink.AddLine(new Vector2(x, edge[i]));
                }
                float xb = left ? inset[rowCount - 1] : width - inset[rowCount - 1];
                sink.AddLine(new Vector2(xb, bottomY));
                sink.AddLine(new Vector2(x0, bottomY));
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();
                _d2dRenderTarget.FillGeometry(geo, brush);
            }
        }
    }
}
