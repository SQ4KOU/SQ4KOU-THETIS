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

        // EOO-safe RADE keying arbiter.  On a RADE TX release the normal MOX
        // falling edge is intercepted, the radio stays keyed, the native modem
        // emits/drains EOO, and only then is the real MOX falling edge allowed.
        private enum RadePttState { Idle, Transmitting, EmitEOO, Flushing, Releasing }
        private volatile RadePttState _radePttState = RadePttState.Idle;
        private volatile bool _radePttRequest;
        private volatile bool _radeOverWasRade;
        private volatile bool _radeArbiterActuating;
        private readonly System.Diagnostics.Stopwatch _radeSequenceClock = new System.Diagnostics.Stopwatch();
        private long _radeFlushAckMs = -1;
        private const int RadeEooMarginMs = 300;
        private const int RadeEooTimeoutMs = 3000;

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

                // Runtime timer is intentionally telemetry/routing only.  MOX/EOO
                // sequencing is owned by the synchronous MOX interception plus
                // PollPTT arbiter below; a timer edge must never generate a second EOO.
                _radeRuntimeTimer = new Timer();
                _radeRuntimeTimer.Interval = 100;
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
                int txRx = (RX2Enabled && chkVFOBTX.Checked) ? 1 : 0;
                RadeNative.SetRadaeTxRx(txRx);
            }
            catch { }
        }

        private bool RadeOverActiveForCurrentTx()
        {
            try
            {
                bool rx2Over = RX2Enabled && chkVFOBTX.Checked;
                bool rx1 = RadeNative.GetRadaeRxEnabled(0) != 0;
                bool rx2 = RadeNative.GetRadaeRxEnabled(1) != 0;
                return (rx1 && !rx2Over) || (rx2 && rx2Over);
            }
            catch { return false; }
        }

        // Called at the very start of chkMOX_CheckedChanged2.  Return true when
        // the attempted falling edge was consumed and normal un-key must stop.
        private bool RadeInterceptMoxChange(bool requestedTx)
        {
            if (_radeArbiterActuating) return false;

            if (requestedTx)
            {
                if (RadeOverActiveForCurrentTx() && !chkTUN.Checked && !chk2TONE.Checked)
                    _radePttRequest = true;
                return false; // key-up is never delayed
            }

            if (_radePttState == RadePttState.Transmitting && _radeOverWasRade)
            {
                _radePttRequest = false;

                // Restore the visual checkbox without firing the normal MOX
                // handler a second time.  _mox/hardware are still TX because the
                // current falling-edge handler exits before touching them.
                chkMOX.CheckedChanged -= chkMOX_CheckedChanged2;
                try { chkMOX.Checked = true; }
                finally { chkMOX.CheckedChanged += chkMOX_CheckedChanged2; }
                System.Diagnostics.Debug.WriteLine("[RADE-PTT] release intercepted; holding TX for EOO");
                return true;
            }

            return false;
        }

        // Called after the normal MOX transition has completed.  This is where
        // the native modem is coupled to the real TX edge, not a GUI timer.
        private void RadeAfterMoxChanged(bool tx)
        {
            try
            {
                if (tx && _radePttRequest && _radePttState == RadePttState.Idle)
                {
                    _radeOverWasRade = RadeOverActiveForCurrentTx() && !chkTUN.Checked && !chk2TONE.Checked;
                    if (!_radeOverWasRade)
                    {
                        _radePttRequest = false;
                        return;
                    }

                    RadeNative.SetRadaeTxRx((RX2Enabled && chkVFOBTX.Checked) ? 1 : 0);
                    RadeNative.SetRadaeTxSilenceHold(0);
                    RadeNative.SetRadaeMoxState(1);
                    RadeNative.RadaeNotifyBeginOver();
                    _radePttState = RadePttState.Transmitting;
                    System.Diagnostics.Debug.WriteLine("[RADE-PTT] Idle->Transmitting");
                }
                else if (!tx && _radePttState == RadePttState.Releasing)
                {
                    // The real hardware falling edge completed.  Hold is released
                    // by the state machine immediately after this callback returns.
                    System.Diagnostics.Debug.WriteLine("[RADE-PTT] hardware un-key complete");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[RADE-PTT] edge error: " + ex.Message);
            }
        }

        // Called from the existing ~1 ms PollPTT loop.  Returns true while normal
        // PTT polling must be suspended so nothing can fight the deferred un-key.
        private bool RadePttStateMachine()
        {
            try
            {
                switch (_radePttState)
                {
                    case RadePttState.Idle:
                    case RadePttState.Transmitting:
                        if (_radePttState == RadePttState.Transmitting && !_radePttRequest)
                        {
                            // Keep ChannelMaster routing in TX, but stop ingesting
                            // live voice and emit EOO into the still-keyed TX path.
                            RadeNative.SetRadaeTxSilenceHold(1);
                            RadeNative.RadaeNotifyEndOfOver();
                            RadeNative.SetRadaeMoxState(0);
                            _radeFlushAckMs = -1;
                            _radeSequenceClock.Restart();
                            _radePttState = RadePttState.EmitEOO;
                            System.Diagnostics.Debug.WriteLine("[RADE-PTT] Transmitting->EmitEOO");
                            return true;
                        }
                        return false;

                    case RadePttState.EmitEOO:
                        _radePttState = RadePttState.Flushing;
                        System.Diagnostics.Debug.WriteLine("[RADE-PTT] EmitEOO->Flushing");
                        return true;

                    case RadePttState.Flushing:
                        if (_radeFlushAckMs < 0 && RadeNative.GetRadaeEooFlushed() != 0)
                        {
                            _radeFlushAckMs = _radeSequenceClock.ElapsedMilliseconds;
                            System.Diagnostics.Debug.WriteLine("[RADE-PTT] EOO flushed @ " + _radeFlushAckMs + "ms");
                        }

                        bool marginDone = _radeFlushAckMs >= 0 &&
                            (_radeSequenceClock.ElapsedMilliseconds - _radeFlushAckMs) >= RadeEooMarginMs;
                        bool timedOut = _radeSequenceClock.ElapsedMilliseconds >= RadeEooTimeoutMs;
                        if (marginDone || timedOut)
                        {
                            if (timedOut)
                                System.Diagnostics.Debug.WriteLine("[RADE-PTT] EOO flush timeout; forcing safe RX");
                            _radePttState = RadePttState.Releasing;
                        }
                        return true;

                    case RadePttState.Releasing:
                        _radeArbiterActuating = true;
                        try
                        {
                            if (chkMOX.Checked) chkMOX.Checked = false; // real Thetis/RedPitaya un-key path
                        }
                        finally { _radeArbiterActuating = false; }

                        // Only after HdwMOXChanged/AudioMOXChanged/cmaster.Mox have
                        // completed do we allow the modem hold to drop.
                        RadeNative.SetRadaeTxSilenceHold(0);
                        _radePttRequest = false;
                        _radeOverWasRade = false;
                        _radeSequenceClock.Stop();
                        _radePttState = RadePttState.Idle;
                        System.Diagnostics.Debug.WriteLine("[RADE-PTT] Releasing->Idle");
                        return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[RADE-PTT] arbiter error: " + ex.Message);
                try { RadeNative.SetRadaeTxSilenceHold(0); } catch { }
                _radePttRequest = false;
                _radeOverWasRade = false;
                _radePttState = RadePttState.Idle;
            }
            return false;
        }

        private bool RadePttPostReleaseBusy
        {
            get
            {
                return _radePttState == RadePttState.EmitEOO ||
                       _radePttState == RadePttState.Flushing ||
                       _radePttState == RadePttState.Releasing;
            }
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
