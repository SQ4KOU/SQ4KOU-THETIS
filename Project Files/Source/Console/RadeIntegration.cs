using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Console
    {
        private ToolStripMenuItem _radeMenuItem;
        private Timer _radeRuntimeTimer;
        private RadeControlForm _radeControlForm;
        private bool _radeLastMox;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                if (MainMenuStrip != null && _radeMenuItem == null)
                {
                    _radeMenuItem = new ToolStripMenuItem("RADE");
                    _radeMenuItem.ToolTipText = "FreeDV RADE V1/V2 controls";
                    _radeMenuItem.Click += delegate { ShowRadeControl(); };
                    MainMenuStrip.Items.Add(_radeMenuItem);
                }

                _radeLastMox = Audio.MOX;
                _radeRuntimeTimer = new Timer();
                _radeRuntimeTimer.Interval = 10;
                _radeRuntimeTimer.Tick += RadeRuntimeTick;
                _radeRuntimeTimer.Start();
            }
            catch { }
        }

        private void ShowRadeControl()
        {
            if (_radeControlForm == null || _radeControlForm.IsDisposed)
                _radeControlForm = new RadeControlForm();
            if (!_radeControlForm.Visible) _radeControlForm.Show(this);
            else _radeControlForm.BringToFront();
        }

        private void RadeRuntimeTick(object sender, EventArgs e)
        {
            try
            {
                bool mox = Audio.MOX;
                int txRx = (Audio.RX2Enabled && Audio.VFOBTX) ? 1 : 0;
                RadeNative.SetRadaeTxRx(txRx);

                if (mox != _radeLastMox)
                {
                    if (mox)
                    {
                        RadeNative.SetRadaeMoxState(1);
                        RadeNative.RadaeNotifyBeginOver();
                    }
                    else
                    {
                        RadeNative.RadaeNotifyEndOfOver();
                        RadeNative.SetRadaeMoxState(0);
                    }
                    _radeLastMox = mox;
                }
            }
            catch { }
        }
    }

    internal sealed class RadeControlForm : Form
    {
        private readonly CheckBox _rx1 = new CheckBox();
        private readonly CheckBox _rx2 = new CheckBox();
        private readonly ComboBox _v1v2Rx1 = new ComboBox();
        private readonly ComboBox _v1v2Rx2 = new ComboBox();
        private readonly NumericUpDown _mic = new NumericUpDown();
        private readonly NumericUpDown _rx1Level = new NumericUpDown();
        private readonly NumericUpDown _rx2Level = new NumericUpDown();
        private readonly CheckBox _rnnoise = new CheckBox();
        private readonly CheckBox _agc = new CheckBox();
        private readonly NumericUpDown _agcTarget = new NumericUpDown();
        private readonly CheckBox _loopback = new CheckBox();
        private readonly TextBox _call = new TextBox();
        private readonly Label _rx1Stat = new Label();
        private readonly Label _rx2Stat = new Label();
        private readonly Label _txStat = new Label();
        private readonly Timer _meterTimer = new Timer();

        internal RadeControlForm()
        {
            Text = "Thetis-RedPitaya - FreeDV RADE V1/V2";
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(440, 355);
            MaximizeBox = false;
            MinimizeBox = false;

            int y = 16;
            _rx1.Text = "RX1 RADE"; _rx1.SetBounds(16, y, 105, 24);
            _v1v2Rx1.DropDownStyle = ComboBoxStyle.DropDownList; _v1v2Rx1.Items.AddRange(new object[] { "V1", "V2" }); _v1v2Rx1.SelectedIndex = 0; _v1v2Rx1.SetBounds(130, y, 72, 24);
            _rx1Stat.SetBounds(215, y + 3, 210, 22);
            Controls.AddRange(new Control[] { _rx1, _v1v2Rx1, _rx1Stat });

            y += 34;
            _rx2.Text = "RX2 RADE"; _rx2.SetBounds(16, y, 105, 24);
            _v1v2Rx2.DropDownStyle = ComboBoxStyle.DropDownList; _v1v2Rx2.Items.AddRange(new object[] { "V1", "V2" }); _v1v2Rx2.SelectedIndex = 0; _v1v2Rx2.SetBounds(130, y, 72, 24);
            _rx2Stat.SetBounds(215, y + 3, 210, 22);
            Controls.AddRange(new Control[] { _rx2, _v1v2Rx2, _rx2Stat });

            y += 42;
            AddLabel("Mic level (dB)", 16, y); SetupDb(_mic, 150, y, -40, 40, 0);
            y += 31;
            AddLabel("RX1 level (dB)", 16, y); SetupDb(_rx1Level, 150, y, -40, 40, 0);
            y += 31;
            AddLabel("RX2 level (dB)", 16, y); SetupDb(_rx2Level, 150, y, -40, 40, 0);

            y += 38;
            _rnnoise.Text = "RNNoise"; _rnnoise.SetBounds(16, y, 90, 24);
            _agc.Text = "Mic AGC"; _agc.SetBounds(115, y, 90, 24);
            AddLabel("AGC LUFS", 215, y + 3);
            _agcTarget.Minimum = -40; _agcTarget.Maximum = -8; _agcTarget.Value = -20; _agcTarget.SetBounds(305, y, 70, 24);
            Controls.AddRange(new Control[] { _rnnoise, _agc, _agcTarget });

            y += 36;
            _loopback.Text = "RX1 loopback"; _loopback.SetBounds(16, y, 115, 24);
            AddLabel("EOO callsign", 150, y + 3);
            _call.CharacterCasing = CharacterCasing.Upper; _call.MaxLength = 15; _call.SetBounds(245, y, 120, 24);
            Controls.AddRange(new Control[] { _loopback, _call });

            y += 42;
            _txStat.SetBounds(16, y, 400, 22);
            Controls.Add(_txStat);

            var note = new Label();
            note.Text = "Use DIGL/DIGU. RADE is inserted directly in ChannelMaster; VAC is not required.";
            note.SetBounds(16, y + 28, 408, 34); note.AutoSize = false;
            Controls.Add(note);

            _rx1.CheckedChanged += delegate { ApplyEnable(); };
            _rx2.CheckedChanged += delegate { ApplyEnable(); };
            _v1v2Rx1.SelectedIndexChanged += delegate { Safe(delegate { RadeNative.SetRadaeProtocolV2(0, _v1v2Rx1.SelectedIndex == 1 ? 1 : 0); }); };
            _v1v2Rx2.SelectedIndexChanged += delegate { Safe(delegate { RadeNative.SetRadaeProtocolV2(1, _v1v2Rx2.SelectedIndex == 1 ? 1 : 0); }); };
            _mic.ValueChanged += delegate { Safe(delegate { RadeNative.SetRadaeMicScale(RadeNative.DbToLinear(_mic.Value)); }); };
            _rx1Level.ValueChanged += delegate { Safe(delegate { RadeNative.SetRadaeRxDialScale(0, RadeNative.DbToLinear(_rx1Level.Value)); }); };
            _rx2Level.ValueChanged += delegate { Safe(delegate { RadeNative.SetRadaeRxDialScale(1, RadeNative.DbToLinear(_rx2Level.Value)); }); };
            _rnnoise.CheckedChanged += delegate { Safe(delegate { RadeNative.SetRadaeMicRNNoiseEnabled(_rnnoise.Checked ? 1 : 0); }); };
            _agc.CheckedChanged += delegate { Safe(delegate { RadeNative.SetRadaeMicAGCEnabled(_agc.Checked ? 1 : 0); }); };
            _agcTarget.ValueChanged += delegate { Safe(delegate { RadeNative.SetRadaeMicAGCTargetLufs((double)_agcTarget.Value); }); };
            _loopback.CheckedChanged += delegate { Safe(delegate { RadeNative.SetRadaeLoopbackEnabled(0, _loopback.Checked ? 1 : 0); }); };
            _call.TextChanged += delegate { Safe(delegate { RadeNative.SetRadaeEooCallsign(_call.Text.Trim()); }); };

            _meterTimer.Interval = 250;
            _meterTimer.Tick += delegate { UpdateMeters(); };
            _meterTimer.Start();
        }

        private void AddLabel(string text, int x, int y)
        {
            var l = new Label(); l.Text = text; l.SetBounds(x, y, 128, 22); Controls.Add(l);
        }

        private static void SetupDb(NumericUpDown n, int x, int y, int min, int max, int value)
        {
            n.Minimum = min; n.Maximum = max; n.Value = value; n.DecimalPlaces = 0; n.Increment = 1; n.SetBounds(x, y, 70, 24);
        }

        private void ApplyEnable()
        {
            Safe(delegate
            {
                RadeNative.SetRadaeRxEnabled(0, _rx1.Checked ? 1 : 0);
                RadeNative.SetRadaeRxEnabled(1, _rx2.Checked ? 1 : 0);
                RadeNative.SetRadaeTxEnabled((_rx1.Checked || _rx2.Checked) ? 1 : 0);
            });
        }

        private void UpdateMeters()
        {
            Safe(delegate
            {
                _rx1Stat.Text = FormatRx(0);
                _rx2Stat.Text = FormatRx(1);
                _txStat.Text = "TX mic: " + RadeNative.GetRadaeTxMicLevelDb() + " dBFS" + (RadeNative.GetRadaeTxMicClip() != 0 ? "  CLIP" : "");
            });
        }

        private static string FormatRx(int rx)
        {
            var sb = new StringBuilder(16);
            RadeNative.GetRadaeRemoteCallsign(rx, sb, sb.Capacity);
            string call = sb.ToString();
            return (RadeNative.GetRadaeSync(rx) != 0 ? "SYNC " : "---- ") + RadeNative.GetRadaeSnrDb(rx) + " dB  " + RadeNative.GetRadaeRxLevelDb(rx) + " dBFS" + (String.IsNullOrEmpty(call) ? "" : "  " + call);
        }

        private static void Safe(Action a)
        {
            try { a(); } catch (DllNotFoundException) { } catch (EntryPointNotFoundException) { } catch { }
        }
    }
}
