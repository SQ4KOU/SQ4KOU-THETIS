#!/usr/bin/env python3
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
CON = ROOT / "Project Files/Source/Console"
REF = "origin/reference/eu2av-2.10.3.16-final-decompiled"
REF_BASE = "reverse-engineering/EU2AV-Thetis-2.10.3.16-Final/managed/Thetis.exe"


def fail(msg):
    raise SystemExit("GPU_DECOMPILED_TEST_FAIL: " + msg)


def git_show(path):
    p = subprocess.run(["git", "show", f"{REF}:{path}"], cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if p.returncode:
        fail(f"git show failed for {path}: {p.stderr.decode(errors='replace')}")
    return p.stdout


def write_ref(src, dst):
    data = git_show(src)
    dst.write_bytes(data)
    print(f"RECOVERED {dst.relative_to(ROOT)} {len(data)} bytes")


def write_text(path, text):
    path.write_text(text, encoding="utf-8-sig")
    print(f"WROTE {path.relative_to(ROOT)}")


# Only the recovered Final GPU FFT engine and its exact enums are imported.
# No archive is used: every byte below comes from the locked decompiled Git branch.
for name in [
    "GPUWaterfallPipeline.cs",
    "GPUWaterfallMagnitudeMode.cs",
    "GPUWaterfallResamplingMode.cs",
    "GPUWaterfallWindowType.cs",
]:
    write_ref(f"{REF_BASE}/Thetis/{name}", CON / name)

# The recovered pipeline loads these files by their plain file names from the EXE directory.
for name in [
    "waterfall_fft_bitreverse_cs.bin",
    "waterfall_fft_magnitude_cs.bin",
    "waterfall_fft_stage_ab_cs.bin",
    "waterfall_fft_stage_ba_cs.bin",
]:
    write_ref(f"{REF_BASE}/Thetis.{name}", CON / name)

native = r'''using System;
using System.Runtime.InteropServices;

namespace Thetis
{
    // Managed ABI for the raw-IQ ring already present in SQ4KOU ChannelMaster.
    // FFT execution itself is NOT native here: it is GPUWaterfallPipeline recovered from Final.
    internal static class GPUWaterfallNative
    {
        private const string DllName = "ChannelMaster.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_WaterfallIQ_Init(int channel, int requestedSamples);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CM_WaterfallIQ_SetEnabled(int channel, int enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_WaterfallIQ_Available(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_WaterfallIQ_Get(int channel, int requestedSamples,
            [Out] float[] iOut, [Out] float[] qOut);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CM_WaterfallIQ_Free(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong CM_WaterfallIQ_DroppedSamples(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CM_WaterfallIQ_ResetDropped(int channel);
    }
}
'''
write_text(CON / "GPUWaterfallNative.cs", native)

display = r'''using System;

namespace Thetis
{
    partial class Display
    {
        // SQ4KOU TEST: runtime bridge to GPUWaterfallPipeline recovered from
        // EU2AV Thetis 2.10.3.16 Extended Final. CPU/WDSP remains the safe fallback.
        private static bool _waterfallUseGPU = true;
        private static int _gpuWaterfallFFTSize = 32768;
        private static int _gpuWaterfallWindowType = (int)GPUWaterfallWindowType.Nuttall;
        private static float _gpuWaterfallKaiserBeta = 6.0f;
        private static int _gpuWaterfallMagnitudeMode = (int)GPUWaterfallMagnitudeMode.PeakHoldPower;
        private static bool _gpuWaterfallAutoOverlap = true;
        private static float _gpuWaterfallOverlapPercent = 75.0f;
        private static int _gpuWaterfallLanczosWindow = 3;
        private static int _gpuWaterfallResamplingMode = (int)GPUWaterfallResamplingMode.Quality;

        private static readonly GPUWaterfallPipeline[] _gpuRecoveredPipeline = new GPUWaterfallPipeline[2];
        private static readonly bool[] _gpuRecoveredIQEnabled = new bool[2];
        private static readonly bool[] _gpuRecoveredFailed = new bool[2];
        private static readonly int[] _gpuRecoveredFill = new int[2];
        private static readonly float[][] _gpuRecoveredI = new float[2][];
        private static readonly float[][] _gpuRecoveredQ = new float[2][];
        private static readonly float[][] _gpuRecoveredTmpI = new float[2][];
        private static readonly float[][] _gpuRecoveredTmpQ = new float[2][];
        private static readonly string[] _gpuRecoveredStatus = new string[] { "CPU fallback", "CPU fallback" };

        public static bool WaterfallUseGPU
        {
            get { return _waterfallUseGPU; }
            set { if (_waterfallUseGPU != value) { _waterfallUseGPU = value; ResetGPUWaterfallState(); } }
        }

        public static int GPUWaterfallFFTSize
        {
            get { return _gpuWaterfallFFTSize; }
            set { int v = ClampGPUFFTSize(value); if (_gpuWaterfallFFTSize != v) { _gpuWaterfallFFTSize = v; ResetGPUWaterfallState(); } }
        }

        public static int GPUWaterfallWindowType
        {
            get { return _gpuWaterfallWindowType; }
            set { int v = Math.Max(0, Math.Min(5, value)); if (_gpuWaterfallWindowType != v) { _gpuWaterfallWindowType = v; ResetGPUWaterfallState(); } }
        }

        public static float GPUWaterfallKaiserBeta
        {
            get { return _gpuWaterfallKaiserBeta; }
            set { float v = Math.Max(0.0f, Math.Min(30.0f, value)); if (Math.Abs(_gpuWaterfallKaiserBeta - v) > 0.0001f) { _gpuWaterfallKaiserBeta = v; ResetGPUWaterfallState(); } }
        }

        public static int GPUWaterfallMagnitudeMode
        {
            get { return _gpuWaterfallMagnitudeMode; }
            set { int v = Math.Max(0, Math.Min(2, value)); if (_gpuWaterfallMagnitudeMode != v) { _gpuWaterfallMagnitudeMode = v; ResetGPUWaterfallState(); } }
        }

        public static bool GPUWaterfallAutoOverlap
        {
            get { return _gpuWaterfallAutoOverlap; }
            set { if (_gpuWaterfallAutoOverlap != value) { _gpuWaterfallAutoOverlap = value; ResetGPUWaterfallState(); } }
        }

        public static float GPUWaterfallOverlapPercent
        {
            get { return _gpuWaterfallOverlapPercent; }
            set { float v = Math.Max(0.0f, Math.Min(95.0f, value)); if (Math.Abs(_gpuWaterfallOverlapPercent - v) > 0.01f) { _gpuWaterfallOverlapPercent = v; ResetGPUWaterfallState(); } }
        }

        public static int GPUWaterfallLanczosWindow
        {
            get { return _gpuWaterfallLanczosWindow; }
            set { int v = Math.Max(2, Math.Min(4, value)); if (_gpuWaterfallLanczosWindow != v) { _gpuWaterfallLanczosWindow = v; ResetGPUWaterfallState(); } }
        }

        public static int GPUWaterfallResamplingMode
        {
            get { return _gpuWaterfallResamplingMode; }
            set { int v = Math.Max(0, Math.Min(1, value)); if (_gpuWaterfallResamplingMode != v) { _gpuWaterfallResamplingMode = v; ResetGPUWaterfallState(); } }
        }

        public static ulong GPUWaterfallDroppedSamplesRX1 { get { return GetDropped(0); } }
        public static ulong GPUWaterfallDroppedSamplesRX2 { get { return GetDropped(1); } }
        public static string GPUWaterfallRuntimeTextRX1 { get { return _gpuRecoveredStatus[0]; } }
        public static string GPUWaterfallRuntimeTextRX2 { get { return _gpuRecoveredStatus[1]; } }

        public static void ApplyGPUWaterfallEU2AVDefaults()
        {
            _waterfallUseGPU = true;
            _gpuWaterfallFFTSize = 32768;
            _gpuWaterfallWindowType = (int)GPUWaterfallWindowType.Nuttall;
            _gpuWaterfallKaiserBeta = 6.0f;
            _gpuWaterfallMagnitudeMode = (int)GPUWaterfallMagnitudeMode.PeakHoldPower;
            _gpuWaterfallAutoOverlap = true;
            _gpuWaterfallOverlapPercent = 75.0f;
            _gpuWaterfallLanczosWindow = 3;
            _gpuWaterfallResamplingMode = (int)GPUWaterfallResamplingMode.Quality;
            ResetGPUWaterfallState();
        }

        public static void ResetGPUWaterfallCalibration() { ResetGPUWaterfallState(); }

        private static ulong GetDropped(int channel)
        {
            try { return GPUWaterfallNative.CM_WaterfallIQ_DroppedSamples(channel); }
            catch { return 0; }
        }

        private static int ClampGPUFFTSize(int value)
        {
            if (value < 1024) value = 1024;
            if (value > GPUWaterfallPipeline.MaxFftSize) value = GPUWaterfallPipeline.MaxFftSize;
            int p = 1024;
            while (p < value && p < GPUWaterfallPipeline.MaxFftSize) p <<= 1;
            return p;
        }

        private static void EnsureBuffer(ref float[] buffer, int length)
        {
            if (buffer == null || buffer.Length != length) buffer = new float[length];
        }

        private static void StopRecoveredChannel(int channel)
        {
            try { GPUWaterfallNative.CM_WaterfallIQ_SetEnabled(channel, 0); } catch { }
            try { GPUWaterfallNative.CM_WaterfallIQ_Free(channel); } catch { }
            _gpuRecoveredIQEnabled[channel] = false;
            _gpuRecoveredFill[channel] = 0;
            if (_gpuRecoveredPipeline[channel] != null)
            {
                try { _gpuRecoveredPipeline[channel].Dispose(); } catch { }
                _gpuRecoveredPipeline[channel] = null;
            }
            _gpuRecoveredI[channel] = null;
            _gpuRecoveredQ[channel] = null;
            _gpuRecoveredTmpI[channel] = null;
            _gpuRecoveredTmpQ[channel] = null;
        }

        public static void ResetGPUWaterfallState()
        {
            for (int ch = 0; ch < 2; ch++)
            {
                StopRecoveredChannel(ch);
                _gpuRecoveredFailed[ch] = false;
                _gpuRecoveredStatus[ch] = _waterfallUseGPU ? "GPU waiting" : "CPU fallback";
            }
        }

        private static bool EnsureRecoveredPipeline(int channel, int width, int sampleRate, float lowHz, float highHz)
        {
            if (_device == null || width <= 0 || sampleRate <= 0 || _gpuRecoveredFailed[channel]) return false;

            try
            {
                if (!_gpuRecoveredIQEnabled[channel])
                {
                    int requested = Math.Min(1 << 20, Math.Max(_gpuWaterfallFFTSize * 4, 131072));
                    if (GPUWaterfallNative.CM_WaterfallIQ_Init(channel, requested) <= 0)
                        throw new InvalidOperationException("CM_WaterfallIQ_Init failed");
                    GPUWaterfallNative.CM_WaterfallIQ_ResetDropped(channel);
                    GPUWaterfallNative.CM_WaterfallIQ_SetEnabled(channel, 1);
                    _gpuRecoveredIQEnabled[channel] = true;
                }

                GPUWaterfallPipeline p = _gpuRecoveredPipeline[channel];
                if (p == null || !p.IsInitialized || p.FFTSize != _gpuWaterfallFFTSize || p.DisplayWidth != width || Math.Abs(p.SampleRate - sampleRate) > 0.5f)
                {
                    if (p != null) try { p.Dispose(); } catch { }
                    p = new GPUWaterfallPipeline(_device, _gpuWaterfallFFTSize, width, sampleRate);
                    if (!p.IsInitialized) throw new InvalidOperationException("GPUWaterfallPipeline initialization failed");
                    _gpuRecoveredPipeline[channel] = p;
                    _gpuRecoveredFill[channel] = 0;
                    EnsureBuffer(ref _gpuRecoveredI[channel], _gpuWaterfallFFTSize);
                    EnsureBuffer(ref _gpuRecoveredQ[channel], _gpuWaterfallFFTSize);
                    EnsureBuffer(ref _gpuRecoveredTmpI[channel], _gpuWaterfallFFTSize);
                    EnsureBuffer(ref _gpuRecoveredTmpQ[channel], _gpuWaterfallFFTSize);
                }

                p.WindowType = (GPUWaterfallWindowType)_gpuWaterfallWindowType;
                p.KaiserBeta = _gpuWaterfallKaiserBeta;
                p.MagnitudeMode = (GPUWaterfallMagnitudeMode)_gpuWaterfallMagnitudeMode;
                p.LanczosWindow = _gpuWaterfallLanczosWindow;
                p.ResamplingMode = (GPUWaterfallResamplingMode)_gpuWaterfallResamplingMode;
                p.SetFrequencySpan(lowHz, highHz);
                return true;
            }
            catch (Exception ex)
            {
                _gpuRecoveredStatus[channel] = "CPU fallback: " + ex.GetType().Name;
                _gpuRecoveredFailed[channel] = true;
                StopRecoveredChannel(channel);
                return false;
            }
        }

        private static bool FillRecoveredFrame(int channel)
        {
            int fft = _gpuWaterfallFFTSize;
            EnsureBuffer(ref _gpuRecoveredI[channel], fft);
            EnsureBuffer(ref _gpuRecoveredQ[channel], fft);
            EnsureBuffer(ref _gpuRecoveredTmpI[channel], fft);
            EnsureBuffer(ref _gpuRecoveredTmpQ[channel], fft);

            int fill = _gpuRecoveredFill[channel];
            int need = fft - fill;
            if (need <= 0) return true;

            int available;
            try { available = GPUWaterfallNative.CM_WaterfallIQ_Available(channel); }
            catch { return false; }
            if (available <= 0) return false;

            int take = Math.Min(need, available);
            int got;
            try { got = GPUWaterfallNative.CM_WaterfallIQ_Get(channel, take, _gpuRecoveredTmpI[channel], _gpuRecoveredTmpQ[channel]); }
            catch { return false; }
            if (got <= 0) return false;

            Array.Copy(_gpuRecoveredTmpI[channel], 0, _gpuRecoveredI[channel], fill, got);
            Array.Copy(_gpuRecoveredTmpQ[channel], 0, _gpuRecoveredQ[channel], fill, got);
            _gpuRecoveredFill[channel] = fill + got;
            return _gpuRecoveredFill[channel] >= fft;
        }

        private static void AdvanceRecoveredFrame(int channel)
        {
            int fft = _gpuWaterfallFFTSize;
            float percent = _gpuWaterfallAutoOverlap ? 75.0f : _gpuWaterfallOverlapPercent;
            int overlap = (int)Math.Round(fft * percent / 100.0f);
            if (overlap < 0) overlap = 0;
            if (overlap >= fft) overlap = fft - 1;
            int hop = fft - overlap;
            if (overlap > 0)
            {
                Array.Copy(_gpuRecoveredI[channel], hop, _gpuRecoveredI[channel], 0, overlap);
                Array.Copy(_gpuRecoveredQ[channel], hop, _gpuRecoveredQ[channel], 0, overlap);
            }
            _gpuRecoveredFill[channel] = overlap;
        }

        // Called immediately before the unchanged SQ4KOU CPU waterfall ready/copy block.
        // Returns silently when the GPU frame is not ready, so the original CPU path stays live.
        private static void TryUpdateGPUWaterfallRow(int rx, int width, bool localMox)
        {
            if (!_waterfallUseGPU || localMox || width <= 0 || !console.PowerOn) return;
            int channel = rx == 2 ? 1 : 0;
            int sampleRate = rx == 2 ? SampleRateRX2 : SampleRateRX1;
            float lowHz = rx == 2 ? RX2DisplayLow : RXDisplayLow;
            float highHz = rx == 2 ? RX2DisplayHigh : RXDisplayHigh;

            if (!EnsureRecoveredPipeline(channel, width, sampleRate, lowHz, highHz)) return;
            if (!FillRecoveredFrame(channel))
            {
                _gpuRecoveredStatus[channel] = "GPU Final: filling IQ";
                return;
            }

            try
            {
                float[] row = _gpuRecoveredPipeline[channel].Process(_gpuRecoveredI[channel], _gpuRecoveredQ[channel], _gpuWaterfallFFTSize);
                AdvanceRecoveredFrame(channel);
                if (row == null || row.Length < width)
                {
                    _gpuRecoveredStatus[channel] = "GPU Final: readback pending";
                    return;
                }

                float[] target = rx == 2 ? new_waterfall_data_bottom : new_waterfall_data;
                if (target == null || target.Length < width) return;
                Array.Copy(row, 0, target, 0, width);
                if (rx == 2) waterfall_data_ready_bottom = true;
                else waterfall_data_ready = true;
                _gpuRecoveredStatus[channel] = "GPU Final ACTIVE";
            }
            catch (Exception ex)
            {
                _gpuRecoveredStatus[channel] = "CPU fallback: " + ex.GetType().Name;
                _gpuRecoveredFailed[channel] = true;
                StopRecoveredChannel(channel);
            }
        }
    }
}
'''
write_text(CON / "Display.GPUWaterfall.cs", display)

setup = r'''using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Setup
    {
        // Test-only SETUP page exposing exactly the controls consumed by the recovered Final pipeline.
        private TabPage tpGPUFinalTest;
        private CheckBoxTS chkGPUFinalEnable;
        private ComboBoxTS comboGPUFinalFFT;
        private CheckBoxTS chkGPUFinalAutoOverlap;
        private NumericUpDownTS udGPUFinalOverlap;
        private ComboBoxTS comboGPUFinalWindow;
        private NumericUpDownTS udGPUFinalKaiser;
        private ComboBoxTS comboGPUFinalMagnitude;
        private ComboBoxTS comboGPUFinalResampling;
        private ComboBoxTS comboGPUFinalLanczos;
        private LabelTS lblGPUFinalStatus;
        private System.Windows.Forms.Timer gpuFinalStatusTimer;

        private static LabelTS GPUFinalLabel(string text, int x, int y, int w)
        {
            LabelTS l = new LabelTS(); l.Text = text; l.Location = new Point(x, y + 3); l.Size = new Size(w, 20); return l;
        }

        private static ComboBoxTS GPUFinalCombo(string name, int x, int y, int w, params object[] items)
        {
            ComboBoxTS c = new ComboBoxTS(); c.Name = name; c.DropDownStyle = ComboBoxStyle.DropDownList;
            c.Location = new Point(x, y); c.Size = new Size(w, 22); c.Items.AddRange(items); return c;
        }

        private void InitGPUWaterfallSetupUI()
        {
            if (tpGPUFinalTest != null || tcDisplay == null) return;

            tpGPUFinalTest = new TabPage();
            tpGPUFinalTest.Name = "tpGPUFinalTest";
            tpGPUFinalTest.Text = "GPU Waterfall TEST";
            tpGPUFinalTest.BackColor = SystemColors.Control;
            tpGPUFinalTest.AutoScroll = true;

            GroupBoxTS g = new GroupBoxTS();
            g.Text = "EU2AV 2.10.3.16 Final - recovered GPU FFT";
            g.Location = new Point(10, 8); g.Size = new Size(650, 300);

            chkGPUFinalEnable = new CheckBoxTS();
            chkGPUFinalEnable.Text = "GPU FFT enable"; chkGPUFinalEnable.Location = new Point(18, 28); chkGPUFinalEnable.Size = new Size(125, 22);
            chkGPUFinalEnable.Checked = Display.WaterfallUseGPU;
            chkGPUFinalEnable.CheckedChanged += delegate { Display.WaterfallUseGPU = chkGPUFinalEnable.Checked; };
            g.Controls.Add(chkGPUFinalEnable);

            g.Controls.Add(GPUFinalLabel("FFT size:", 18, 62, 90));
            comboGPUFinalFFT = GPUFinalCombo("comboGPUFinalFFT", 112, 60, 120, "4096", "8192", "16384", "32768", "65536", "131072", "262144");
            string fftText = Display.GPUWaterfallFFTSize.ToString();
            int fftIndex = comboGPUFinalFFT.Items.IndexOf(fftText); comboGPUFinalFFT.SelectedIndex = fftIndex >= 0 ? fftIndex : 3;
            comboGPUFinalFFT.SelectedIndexChanged += delegate { int v; if (int.TryParse(comboGPUFinalFFT.Text, out v)) Display.GPUWaterfallFFTSize = v; };
            g.Controls.Add(comboGPUFinalFFT);

            chkGPUFinalAutoOverlap = new CheckBoxTS(); chkGPUFinalAutoOverlap.Text = "Auto overlap (75%)";
            chkGPUFinalAutoOverlap.Location = new Point(260, 60); chkGPUFinalAutoOverlap.Size = new Size(140, 22); chkGPUFinalAutoOverlap.Checked = Display.GPUWaterfallAutoOverlap;
            chkGPUFinalAutoOverlap.CheckedChanged += delegate { Display.GPUWaterfallAutoOverlap = chkGPUFinalAutoOverlap.Checked; udGPUFinalOverlap.Enabled = !chkGPUFinalAutoOverlap.Checked; };
            g.Controls.Add(chkGPUFinalAutoOverlap);
            udGPUFinalOverlap = new NumericUpDownTS(); udGPUFinalOverlap.Minimum = 0; udGPUFinalOverlap.Maximum = 95; udGPUFinalOverlap.DecimalPlaces = 1; udGPUFinalOverlap.Increment = 1;
            udGPUFinalOverlap.Value = (decimal)Display.GPUWaterfallOverlapPercent; udGPUFinalOverlap.Location = new Point(410, 60); udGPUFinalOverlap.Size = new Size(65, 22); udGPUFinalOverlap.Enabled = !chkGPUFinalAutoOverlap.Checked;
            udGPUFinalOverlap.ValueChanged += delegate { Display.GPUWaterfallOverlapPercent = (float)udGPUFinalOverlap.Value; };
            g.Controls.Add(udGPUFinalOverlap); g.Controls.Add(GPUFinalLabel("%", 480, 62, 25));

            g.Controls.Add(GPUFinalLabel("Window:", 18, 98, 90));
            comboGPUFinalWindow = GPUFinalCombo("comboGPUFinalWindow", 112, 96, 150, "Hann", "Hamming", "Blackman", "Blackman-Harris", "Nuttall", "Kaiser");
            comboGPUFinalWindow.SelectedIndex = Math.Max(0, Math.Min(5, Display.GPUWaterfallWindowType));
            comboGPUFinalWindow.SelectedIndexChanged += delegate { Display.GPUWaterfallWindowType = comboGPUFinalWindow.SelectedIndex; };
            g.Controls.Add(comboGPUFinalWindow);
            g.Controls.Add(GPUFinalLabel("Kaiser beta:", 285, 98, 90));
            udGPUFinalKaiser = new NumericUpDownTS(); udGPUFinalKaiser.Minimum = 0; udGPUFinalKaiser.Maximum = 30; udGPUFinalKaiser.DecimalPlaces = 1; udGPUFinalKaiser.Increment = 0.1M;
            udGPUFinalKaiser.Value = (decimal)Display.GPUWaterfallKaiserBeta; udGPUFinalKaiser.Location = new Point(380, 96); udGPUFinalKaiser.Size = new Size(70, 22);
            udGPUFinalKaiser.ValueChanged += delegate { Display.GPUWaterfallKaiserBeta = (float)udGPUFinalKaiser.Value; };
            g.Controls.Add(udGPUFinalKaiser);

            g.Controls.Add(GPUFinalLabel("Magnitude:", 18, 134, 90));
            comboGPUFinalMagnitude = GPUFinalCombo("comboGPUFinalMagnitude", 112, 132, 160, "PeakHoldAmplitude", "AveragePower", "PeakHoldPower");
            comboGPUFinalMagnitude.SelectedIndex = Math.Max(0, Math.Min(2, Display.GPUWaterfallMagnitudeMode));
            comboGPUFinalMagnitude.SelectedIndexChanged += delegate { Display.GPUWaterfallMagnitudeMode = comboGPUFinalMagnitude.SelectedIndex; };
            g.Controls.Add(comboGPUFinalMagnitude);

            g.Controls.Add(GPUFinalLabel("Resampling:", 300, 134, 90));
            comboGPUFinalResampling = GPUFinalCombo("comboGPUFinalResampling", 395, 132, 100, "Fast", "Quality");
            comboGPUFinalResampling.SelectedIndex = Math.Max(0, Math.Min(1, Display.GPUWaterfallResamplingMode));
            comboGPUFinalResampling.SelectedIndexChanged += delegate { Display.GPUWaterfallResamplingMode = comboGPUFinalResampling.SelectedIndex; };
            g.Controls.Add(comboGPUFinalResampling);

            g.Controls.Add(GPUFinalLabel("Lanczos:", 18, 170, 90));
            comboGPUFinalLanczos = GPUFinalCombo("comboGPUFinalLanczos", 112, 168, 100, "2", "3", "4");
            comboGPUFinalLanczos.SelectedIndex = Math.Max(0, Math.Min(2, Display.GPUWaterfallLanczosWindow - 2));
            comboGPUFinalLanczos.SelectedIndexChanged += delegate { Display.GPUWaterfallLanczosWindow = comboGPUFinalLanczos.SelectedIndex + 2; };
            g.Controls.Add(comboGPUFinalLanczos);

            ButtonTS defaults = new ButtonTS(); defaults.Text = "Final defaults"; defaults.Location = new Point(300, 168); defaults.Size = new Size(100, 25);
            defaults.Click += delegate { Display.ApplyGPUWaterfallEU2AVDefaults(); SyncGPUFinalControls(); };
            g.Controls.Add(defaults);

            lblGPUFinalStatus = GPUFinalLabel("RX1: waiting | RX2: waiting", 18, 218, 600); lblGPUFinalStatus.Name = "lblGPUFinalStatus";
            g.Controls.Add(lblGPUFinalStatus);
            LabelTS note = GPUFinalLabel("CPU/WDSP fallback remains active if GPU initialization/readback fails.", 18, 250, 600); g.Controls.Add(note);

            tpGPUFinalTest.Controls.Add(g); tcDisplay.TabPages.Add(tpGPUFinalTest);
            gpuFinalStatusTimer = new System.Windows.Forms.Timer(); gpuFinalStatusTimer.Interval = 750;
            gpuFinalStatusTimer.Tick += delegate { UpdateGPUFinalStatus(); }; gpuFinalStatusTimer.Start();
            UpdateGPUFinalStatus();
        }

        private void SyncGPUFinalControls()
        {
            if (chkGPUFinalEnable == null) return;
            chkGPUFinalEnable.Checked = Display.WaterfallUseGPU;
            int i = comboGPUFinalFFT.Items.IndexOf(Display.GPUWaterfallFFTSize.ToString()); if (i >= 0) comboGPUFinalFFT.SelectedIndex = i;
            chkGPUFinalAutoOverlap.Checked = Display.GPUWaterfallAutoOverlap;
            udGPUFinalOverlap.Value = (decimal)Display.GPUWaterfallOverlapPercent;
            comboGPUFinalWindow.SelectedIndex = Display.GPUWaterfallWindowType;
            udGPUFinalKaiser.Value = (decimal)Display.GPUWaterfallKaiserBeta;
            comboGPUFinalMagnitude.SelectedIndex = Display.GPUWaterfallMagnitudeMode;
            comboGPUFinalResampling.SelectedIndex = Display.GPUWaterfallResamplingMode;
            comboGPUFinalLanczos.SelectedIndex = Display.GPUWaterfallLanczosWindow - 2;
        }

        private void UpdateGPUFinalStatus()
        {
            if (lblGPUFinalStatus == null) return;
            lblGPUFinalStatus.Text = "RX1: " + Display.GPUWaterfallRuntimeTextRX1 + "  drop=" + Display.GPUWaterfallDroppedSamplesRX1 +
                " | RX2: " + Display.GPUWaterfallRuntimeTextRX2 + "  drop=" + Display.GPUWaterfallDroppedSamplesRX2;
        }
    }
}
'''
write_text(CON / "Setup.GPUWaterfall.cs", setup)

# Add the recovered sources and exact shader payloads to the normal Thetis source build.
csproj = CON / "Thetis.csproj"
text = csproj.read_text(encoding="utf-8-sig")
anchor = '    <Compile Include="Display.GPUWaterfall.cs" />'
if anchor not in text:
    fail("Thetis.csproj GPU Display anchor not found")
compile_lines = [
    '    <Compile Include="GPUWaterfallPipeline.cs" />',
    '    <Compile Include="GPUWaterfallMagnitudeMode.cs" />',
    '    <Compile Include="GPUWaterfallResamplingMode.cs" />',
    '    <Compile Include="GPUWaterfallWindowType.cs" />',
]
insert = "\n".join(x for x in compile_lines if x not in text)
if insert:
    text = text.replace(anchor, anchor + "\n" + insert, 1)

if 'waterfall_fft_bitreverse_cs.bin' not in text:
    block = '''\n  <ItemGroup>\n    <Content Include="waterfall_fft_bitreverse_cs.bin"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>\n    <Content Include="waterfall_fft_magnitude_cs.bin"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>\n    <Content Include="waterfall_fft_stage_ab_cs.bin"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>\n    <Content Include="waterfall_fft_stage_ba_cs.bin"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>\n  </ItemGroup>\n'''
    if '</Project>' not in text: fail("Thetis.csproj closing Project not found")
    text = text.replace('</Project>', block + '</Project>', 1)
csproj.write_text(text, encoding="utf-8-sig")
print("PATCHED Thetis.csproj")

# Verify the inherited SQ4KOU integration seams are present. We do not touch the large files here.
display_main = (CON / "display.cs").read_text(encoding="utf-8-sig")
if "TryUpdateGPUWaterfallRow" not in display_main:
    fail("display.cs runtime call seam missing")
setup_main = (CON / "setup.cs").read_text(encoding="utf-8-sig")
if "InitGPUWaterfallSetupUI" not in setup_main:
    fail("setup.cs SETUP call seam missing")

print("GPU_DECOMPILED_TEST_INTEGRATION_OK")
