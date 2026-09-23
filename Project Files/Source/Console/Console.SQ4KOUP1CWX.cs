namespace Thetis
{
    public partial class Console
    {
        // SQ4KOU P1 CWX SOFTWARE-IQ BRIDGE
        // Red Pitaya Protocol 1 does not turn the native Hermes firmware-keyer
        // DOT/DASH/CWX encoding into RF. CWX therefore uses the existing WDSP
        // TX post generator and sends ordinary TX IQ while CWX is active.
        // Scope is deliberately limited to CWX + Protocol 1.
        private bool _sq4kou_p1_software_cwx_active = false;
        private bool _sq4kou_p1_software_cwx_prev_fw_keyer = true;

        public bool SQ4KOUP1SoftwareCWXActive
        {
            get { return _sq4kou_p1_software_cwx_active; }
        }

        public bool SQ4KOUStartP1SoftwareCWX()
        {
            if (NetworkIO.CurrentRadioProtocol != RadioProtocol.USB) return false;

            DSPMode tx_mode = VFOBTX && RX2Enabled ? RX2DSPMode : RX1DSPMode;
            if (tx_mode != DSPMode.CWL && tx_mode != DSPMode.CWU) return false;
            if (_sq4kou_p1_software_cwx_active) return true;

            _sq4kou_p1_software_cwx_prev_fw_keyer = CWFWKeyer;

            // Must be false BEFORE MOX. Protocol 1 then carries normal IQ instead
            // of replacing TX I/Q words with the 3-bit DOT/DASH/CWX value.
            if (CWFWKeyer)
                CWFWKeyer = false;

            radio.GetDSPTX(0).TXPostGenRun = 0;
            radio.GetDSPTX(0).TXPostGenMode = 0; // single tone
            radio.GetDSPTX(0).TXPostGenToneFreq = tx_mode == DSPMode.CWL ? -cw_pitch : +cw_pitch;
            radio.GetDSPTX(0).TXPostGenToneMag = MAX_TONE_MAG;

            _sq4kou_p1_software_cwx_active = true;
            return true;
        }

        public void SQ4KOUArmP1SoftwareCWXTX()
        {
            if (!_sq4kou_p1_software_cwx_active) return;

            // Normal MOX intentionally leaves the WDSP TX channel off in CW mode
            // because the native firmware keyer normally produces RF. Software
            // CWX needs the TX channel running so TXPostGen reaches Protocol 1 IQ.
            WDSP.SetChannelState(WDSP.id(1, 0), 1, 0);
        }

        public void SQ4KOUSetP1SoftwareCWXKey(bool key_down)
        {
            if (!_sq4kou_p1_software_cwx_active) return;
            radio.GetDSPTX(0).TXPostGenRun = key_down ? 1 : 0;
        }

        public void SQ4KOUStopP1SoftwareCWX()
        {
            if (!_sq4kou_p1_software_cwx_active) return;

            radio.GetDSPTX(0).TXPostGenRun = 0;
            _sq4kou_p1_software_cwx_active = false;

            // Restore the exact pre-CWX keyer choice, not an assumed default.
            if (CWFWKeyer != _sq4kou_p1_software_cwx_prev_fw_keyer)
                CWFWKeyer = _sq4kou_p1_software_cwx_prev_fw_keyer;
        }
    }
}
