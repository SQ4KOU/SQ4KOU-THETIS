using System;
using Vortice.Direct2D1;
using Vortice.DXGI;

namespace Thetis
{
    partial class Display
    {
        private static readonly object _waterfallQualityLock = new object();
        private static readonly float[][] _waterfallQualityFloatRows = new float[2][];
        private static readonly byte[][] _waterfallQualityEncodedRows = new byte[2][];
        private static readonly int[] _waterfallQualityDitherRow = new int[2];

        private static AlphaMode WaterfallBitmapAlphaMode
        {
            get { return WaterfallEnhancer.Depth == WaterfallEnhancer.ColorDepth.Bit16 ? AlphaMode.Ignore : ALPHA_MODE; }
        }

        private static Format WaterfallBitmapFormat
        {
            get { return WaterfallPixelWriter.DxgiFormat; }
        }

        private static int WaterfallBitmapPixelSize
        {
            get { return WaterfallPixelWriter.PixelSize; }
        }

        private static byte[] PrepareWaterfallDisplayRow(int rx, byte[] bgra, int width)
        {
            if (bgra == null || width <= 0 || bgra.Length < width * 4)
                return bgra;

            bool effects =
                WaterfallEnhancer.Quality != WaterfallEnhancer.QualityLevel.Classic ||
                WaterfallEnhancer.ToneMap != WaterfallEnhancer.ToneMapMode.None ||
                Math.Abs(WaterfallEnhancer.Gamma - 1.0f) > 0.0001f ||
                WaterfallEnhancer.DitherEnabled;
            bool formatConversion = WaterfallPixelWriter.PixelSize != 4;

            if (!effects && !formatConversion)
                return bgra;

            int slot = rx == 2 ? 1 : 0;
            int floatCount = width * 4;
            int byteCount = width * WaterfallPixelWriter.PixelSize;

            lock (_waterfallQualityLock)
            {
                if (_waterfallQualityFloatRows[slot] == null || _waterfallQualityFloatRows[slot].Length < floatCount)
                    _waterfallQualityFloatRows[slot] = new float[floatCount];
                if (_waterfallQualityEncodedRows[slot] == null || _waterfallQualityEncodedRows[slot].Length != byteCount)
                    _waterfallQualityEncodedRows[slot] = new byte[byteCount];

                float[] rowF = _waterfallQualityFloatRows[slot];
                byte[] encoded = _waterfallQualityEncodedRows[slot];
                int ditherY = _waterfallQualityDitherRow[slot]++ & 7;

                for (int x = 0; x < width; x++)
                {
                    int bi = x * 4;
                    int fi = bi;
                    rowF[fi + 0] = bgra[bi + 2];
                    rowF[fi + 1] = bgra[bi + 1];
                    rowF[fi + 2] = bgra[bi + 0];
                    rowF[fi + 3] = bgra[bi + 3];
                }

                for (int x = 0; x < width; x++)
                {
                    int fi = x * 4;

                    WaterfallEnhancer.ApplySaturationContrast(rowF, fi);

                    if (WaterfallEnhancer.ToneMap != WaterfallEnhancer.ToneMapMode.None)
                    {
                        rowF[fi + 0] = WaterfallEnhancer.ApplyToneMap(rowF[fi + 0]);
                        rowF[fi + 1] = WaterfallEnhancer.ApplyToneMap(rowF[fi + 1]);
                        rowF[fi + 2] = WaterfallEnhancer.ApplyToneMap(rowF[fi + 2]);
                    }

                    if (Math.Abs(WaterfallEnhancer.Gamma - 1.0f) > 0.0001f)
                    {
                        rowF[fi + 0] = WaterfallEnhancer.ApplyGammaFloat(rowF[fi + 0]);
                        rowF[fi + 1] = WaterfallEnhancer.ApplyGammaFloat(rowF[fi + 1]);
                        rowF[fi + 2] = WaterfallEnhancer.ApplyGammaFloat(rowF[fi + 2]);
                    }

                    if (WaterfallEnhancer.DitherEnabled)
                    {
                        rowF[fi + 0] = WaterfallEnhancer.ApplyDitherFloat(rowF[fi + 0], x, ditherY);
                        rowF[fi + 1] = WaterfallEnhancer.ApplyDitherFloat(rowF[fi + 1], x, ditherY);
                        rowF[fi + 2] = WaterfallEnhancer.ApplyDitherFloat(rowF[fi + 2], x, ditherY);
                    }
                }

                WaterfallPixelWriter.EncodeRow(rowF, encoded, width);
                return encoded;
            }
        }

        private static void ResetWaterfallQualityBuffers()
        {
            lock (_waterfallQualityLock)
            {
                _waterfallQualityFloatRows[0] = null;
                _waterfallQualityFloatRows[1] = null;
                _waterfallQualityEncodedRows[0] = null;
                _waterfallQualityEncodedRows[1] = null;
                _waterfallQualityDitherRow[0] = 0;
                _waterfallQualityDitherRow[1] = 0;
            }
        }

        public static bool RebuildWaterfallForColorDepth()
        {
            WaterfallEnhancer.ColorDepth requested = WaterfallEnhancer.Depth;
            WaterfallPixelWriter.UpdateFormat();
            ResetWaterfallQualityBuffers();

            try
            {
                ResetWaterfallBmp();
                if (RX2Enabled) ResetWaterfallBmp2();

                bool ok1 = _waterfall_bmp_dx2d == null || _waterfall_bmp_dx2d.PixelFormat.Format == WaterfallPixelWriter.DxgiFormat;
                bool ok2 = !RX2Enabled || _waterfall_bmp2_dx2d == null || _waterfall_bmp2_dx2d.PixelFormat.Format == WaterfallPixelWriter.DxgiFormat;
                if (ok1 && ok2) return true;
            }
            catch (Exception ex)
            {
                LogTool.AddLogEntry("Waterfall color-depth rebuild failed: " + ex.Message, "DX2D");
            }

            if (requested != WaterfallEnhancer.ColorDepth.Bit8)
            {
                WaterfallEnhancer.SetColorDepth(WaterfallEnhancer.ColorDepth.Bit8);
                WaterfallPixelWriter.UpdateFormat();
                ResetWaterfallQualityBuffers();
                try
                {
                    ResetWaterfallBmp();
                    if (RX2Enabled) ResetWaterfallBmp2();
                }
                catch (Exception ex)
                {
                    LogTool.AddLogEntry("Waterfall 8-bit fallback rebuild failed: " + ex.Message, "DX2D");
                }
            }
            return false;
        }
    }
}
