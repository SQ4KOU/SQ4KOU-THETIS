using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

public class Audio
{
	public enum AudioState
	{
		DTTSP,
		CW
	}

	public enum SignalSource
	{
		RADIO,
		SINE,
		SINE_TWO_TONE,
		SINE_LEFT_ONLY,
		SINE_RIGHT_ONLY,
		NOISE,
		TRIANGLE,
		SAWTOOTH,
		PULSE,
		SILENCE
	}

	private static bool rx2_auto_mute_tx = true;

	private static bool rx1_blank_display_tx = false;

	private static bool rx2_blank_display_tx = false;

	private static double source_scale = 1.0;

	private static SignalSource tx_input_signal = SignalSource.RADIO;

	private static SignalSource tx_output_signal = SignalSource.RADIO;

	private static bool vox_enabled = false;

	private static float vox_gain = 1f;

	private static double high_swr_scale = 1.0;

	private static double mic_preamp = 1.0;

	private static double wave_preamp = 1.0;

	private static double wave_preamp_adjust = 0.0;

	private static double monitor_volume = 0.0;

	private static double radio_volume = 0.0;

	private static AudioState current_audio_state1 = AudioState.DTTSP;

	private static bool rx2_enabled;

	private static volatile bool wave_playback = false;

	private static bool wave_record;

	public static Console console;

	public static float[] phase_buf_l;

	public static float[] phase_buf_r;

	public static bool phase;

	public static bool scope;

	public static bool two_tone;

	public static bool high_pwr_am;

	public static bool testing;

	private static bool vac_combine_input = false;

	private static bool vac2_combine_input = false;

	private static bool mox = false;

	private static bool mon;

	private static bool full_duplex = false;

	private static bool vfob_tx = false;

	private static bool antivox_source_VAC = false;

	private static bool vac_enabled = false;

	private static bool vac2_enabled = false;

	private static bool vac1_latency_manual = false;

	private static bool vac1_latency_manual_out = false;

	private static bool vac1_latency_pa_in_manual = false;

	private static bool vac1_latency_pa_out_manual = false;

	private static bool vac2_latency_manual = false;

	private static bool vac2_latency_out_manual = false;

	private static bool vac2_latency_pa_in_manual = false;

	private static bool vac2_latency_pa_out_manual = false;

	private static bool vac_bypass = false;

	private static bool vac_rb_reset = false;

	private static bool vac2_rb_reset = false;

	private static double vac_preamp = 1.0;

	private static double vac2_tx_scale = 1.0;

	private static double vac_rx_scale = 1.0;

	private static double vac2_rx_scale = 1.0;

	private static DSPMode tx_dsp_mode = DSPMode.LSB;

	private static int sample_rate1 = 48000;

	private static int sample_rate_rx2 = 48000;

	private static int sample_rate_tx = 48000;

	private static int sample_rate2 = 48000;

	private static int sample_rate3 = 48000;

	private static int block_size1 = 1024;

	private static int block_size_rx2 = 1024;

	private static int block_size_tx = 1024;

	private static int block_size_vac = 1024;

	private static int block_size_vac2 = 1024;

	private static bool vac_stereo = false;

	private static bool vac2_stereo = false;

	private static bool vac_output_iq = false;

	private static bool vac2_output_iq = false;

	private static bool vac_output_rx2 = false;

	private static bool vac_correct_iq = true;

	private static bool vac2_correct_iq = true;

	private static bool vox_active = false;

	private static int host2 = 0;

	private static int host3 = 0;

	private static int input_dev2 = 0;

	private static int input_dev3 = 0;

	private static int output_dev2 = 0;

	private static int output_dev3 = 0;

	private static int latency2 = 120;

	private static int latency2_out = 120;

	private static int latency_pa_in = 120;

	private static int latency_pa_out = 120;

	private static int vac2_latency_out = 120;

	private static int vac2_latency_pa_in = 120;

	private static int vac2_latency_pa_out = 120;

	private static int latency3 = 120;

	private static double vac1_feedbackgainIn = 4E-06;

	private static double vac1_slewtimeIn = 0.003;

	private static double vac2_feedbackgainIn = 4E-06;

	private static double vac2_slewtimeIn = 0.003;

	private static int vac1_prop_ringminIn = 4096;

	private static int vac1_prop_ringmaxIn = 16384;

	private static int vac2_prop_ringminIn = 4096;

	private static int vac2_prop_ringmaxIn = 16384;

	private static int vac1_ff_ringminIn = 4096;

	private static int vac1_ff_ringmaxIn = 262144;

	private static int vac2_ff_ringminIn = 4096;

	private static int vac2_ff_ringmaxIn = 262144;

	private static double vac1_ff_alphaIn = 0.01;

	private static double vac2_ff_alphaIn = 0.01;

	private static double vac1_oldVarIn = 1.0;

	private static double vac2_oldVarIn = 1.0;

	private static double vac1_feedbackgainOut = 4E-06;

	private static double vac1_slewtimeOut = 0.003;

	private static double vac2_feedbackgainOut = 4E-06;

	private static double vac2_slewtimeOut = 0.003;

	private static int vac1_prop_ringminOut = 4096;

	private static int vac1_prop_ringmaxOut = 16384;

	private static int vac2_prop_ringminOut = 4096;

	private static int vac2_prop_ringmaxOut = 16384;

	private static int vac1_ff_ringminOut = 4096;

	private static int vac1_ff_ringmaxOut = 262144;

	private static int vac2_ff_ringminOut = 4096;

	private static int vac2_ff_ringmaxOut = 262144;

	private static double vac1_ff_alphaOut = 0.01;

	private static double vac2_ff_alphaOut = 0.01;

	private static double vac1_oldVarOut = 1.0;

	private static double vac2_oldVarOut = 1.0;

	private static bool mute_rx1 = false;

	private static bool mute_rx2 = false;

	private static int out_rate = 48000;

	private static int out_rate_rx2 = 48000;

	private static int out_rate_tx = 48000;

	private static int out_count = 1024;

	private static int out_count_rx2 = 1024;

	private static int out_count_tx = 1024;

	private static int _swap_iq_vac1 = 0;

	private static int _swap_iq_vac2 = 0;

	private static int _exclusive_out_vac1 = 0;

	private static int _exclusive_out_vac2 = 0;

	private static int _exclusive_in_vac1 = 0;

	private static int _exclusive_in_vac2 = 0;

	private static RadioProtocol _lastRadioProtocol = RadioProtocol.None;

	private static HPSDRHW _lastRadiohadware = HPSDRHW.Unknown;

	private static int m_nScope_pixel_width = 0;

	private static int m_nScope_time = 10000;

	private static int m_nScope_samples_per_pixel = 10000;

	private static int m_nScope_samples_per_pixel_tx = 10000;

	private static readonly object m_objScope_width_lock = new object();

	private static int scope_array_len = 0;

	private static int scope_sample_index = 0;

	private static int scope_pixel_index = 0;

	private static float scope_pixel_min = float.MaxValue;

	private static float scope_pixel_max = float.MinValue;

	private static float[] scope_min;

	private static readonly object m_objArrayLock = new object();

	public static float[] scope_max;

	private static int scope2_sample_index = 0;

	private static int scope2_pixel_index = 0;

	private static float scope2_pixel_min = float.MaxValue;

	private static float scope2_pixel_max = float.MinValue;

	public static float[] scope2_max;

	private static float[] scope2_min;

	public static bool RX2AutoMuteTX
	{
		get
		{
			return rx2_auto_mute_tx;
		}
		set
		{
			rx2_auto_mute_tx = value;
		}
	}

	public static bool RX1BlankDisplayTX
	{
		get
		{
			return rx1_blank_display_tx;
		}
		set
		{
			rx1_blank_display_tx = value;
		}
	}

	public static bool RX2BlankDisplayTX
	{
		get
		{
			return rx2_blank_display_tx;
		}
		set
		{
			rx2_blank_display_tx = value;
		}
	}

	public static double SourceScale
	{
		get
		{
			return source_scale;
		}
		set
		{
			source_scale = value;
		}
	}

	public static SignalSource TXInputSignal
	{
		get
		{
			return tx_input_signal;
		}
		set
		{
			tx_input_signal = value;
		}
	}

	public static SignalSource TXOutputSignal
	{
		get
		{
			return tx_output_signal;
		}
		set
		{
			tx_output_signal = value;
		}
	}

	public static bool VOXEnabled
	{
		get
		{
			return vox_enabled;
		}
		set
		{
			vox_enabled = value;
			cmaster.CMSetTXAVoxRun(0);
			if (vox_enabled && vfob_tx)
			{
				ivac.SetIVACvox(0, 0);
				ivac.SetIVACvox(1, 1);
			}
			if (vox_enabled && !vfob_tx)
			{
				ivac.SetIVACvox(0, 1);
				ivac.SetIVACvox(1, 0);
			}
			if (!vox_enabled)
			{
				ivac.SetIVACvox(0, 0);
				ivac.SetIVACvox(1, 0);
			}
		}
	}

	public static float VOXGain
	{
		get
		{
			return vox_gain;
		}
		set
		{
			vox_gain = value;
		}
	}

	public static double HighSWRScale
	{
		get
		{
			return high_swr_scale;
		}
		set
		{
			high_swr_scale = value;
			cmaster.CMSetTXOutputLevel();
		}
	}

	public static double MicPreamp
	{
		get
		{
			return mic_preamp;
		}
		set
		{
			mic_preamp = value;
			cmaster.CMSetTXAPanelGain1(WDSP.id(1u, 0u));
		}
	}

	public static double WavePreamp
	{
		get
		{
			return wave_preamp;
		}
		set
		{
			wave_preamp = value;
			cmaster.CMSetTXAPanelGain1(WDSP.id(1u, 0u));
		}
	}

	public static double WavePreampAdjust
	{
		get
		{
			return wave_preamp_adjust;
		}
		set
		{
			wave_preamp_adjust = value;
			cmaster.CMSetTXAPanelGain1(WDSP.id(1u, 0u));
		}
	}

	public static double MonitorVolume
	{
		get
		{
			return monitor_volume;
		}
		set
		{
			monitor_volume = value;
			cmaster.CMSetAudioVolume(value);
			ivac.SetIVACmonVol(0, monitor_volume);
			cmaster.SetTCIRxAudioMonVol(0, monitor_volume);
			cmaster.SetTCIRxAudioMonVol(1, monitor_volume);
		}
	}

	public static double RadioVolume
	{
		get
		{
			return radio_volume;
		}
		set
		{
			radio_volume = value;
			NetworkIO.SetOutputPower((float)(value * 1.02));
			cmaster.CMSetTXOutputLevel();
		}
	}

	public static AudioState CurrentAudioState1
	{
		get
		{
			return current_audio_state1;
		}
		set
		{
			current_audio_state1 = value;
		}
	}

	public static bool RX2Enabled
	{
		get
		{
			return rx2_enabled;
		}
		set
		{
			rx2_enabled = value;
		}
	}

	public static bool WavePlayback
	{
		get
		{
			return wave_playback;
		}
		set
		{
			wave_playback = value;
			cmaster.CMSetSRXWavePlayRun(0);
			cmaster.CMSetSRXWavePlayRun(1);
			cmaster.CMSetTXAPanelGain1(WDSP.id(1u, 0u));
		}
	}

	public static bool WaveRecord
	{
		get
		{
			return wave_record;
		}
		set
		{
			wave_record = value;
			cmaster.CMSetSRXWaveRecordRun(0);
			cmaster.CMSetSRXWaveRecordRun(1);
		}
	}

	public static bool VACCombineInput
	{
		get
		{
			return vac_combine_input;
		}
		set
		{
			vac_combine_input = value;
			ivac.SetIVACcombine(0, Convert.ToInt32(value));
		}
	}

	public static bool VAC2CombineInput
	{
		get
		{
			return vac2_combine_input;
		}
		set
		{
			vac2_combine_input = value;
			ivac.SetIVACcombine(1, Convert.ToInt32(value));
		}
	}

	public static bool MOX
	{
		get
		{
			return mox;
		}
		set
		{
			mox = value;
			if (mox)
			{
				if (rx2_enabled && vfob_tx)
				{
					ivac.SetIVACmox(0, 0);
					ivac.SetIVACmox(1, 1);
					cmaster.SetTCIRxAudioMox(0, 0);
					cmaster.SetTCIRxAudioMox(1, 1);
				}
				else
				{
					ivac.SetIVACmox(0, 1);
					ivac.SetIVACmox(1, 0);
					cmaster.SetTCIRxAudioMox(0, 1);
					cmaster.SetTCIRxAudioMox(1, 0);
				}
			}
			else
			{
				ivac.SetIVACmox(0, 0);
				ivac.SetIVACmox(1, 0);
				cmaster.SetTCIRxAudioMox(0, 0);
				cmaster.SetTCIRxAudioMox(1, 0);
			}
			WaveThing.UpdateMox();
		}
	}

	public unsafe static bool MON
	{
		get
		{
			return mon;
		}
		set
		{
			mon = value;
			setupIVACforMon();
			cmaster.SetAAudioMixVol(null, 0, WDSP.id(1u, 0u), 0.5);
			cmaster.SetAAudioMixWhat(null, 0, WDSP.id(1u, 0u), value);
		}
	}

	public static bool FullDuplex
	{
		set
		{
			full_duplex = value;
		}
	}

	public static bool VFOBTX
	{
		get
		{
			return vfob_tx;
		}
		set
		{
			vfob_tx = value;
		}
	}

	public static bool AntiVOXSourceVAC
	{
		get
		{
			return antivox_source_VAC;
		}
		set
		{
			antivox_source_VAC = value;
			cmaster.CMSetAntiVoxSourceWhat();
		}
	}

	public static bool VACEnabled
	{
		get
		{
			return vac_enabled;
		}
		set
		{
			vac_enabled = value;
			cmaster.CMSetTXAPanelGain1(WDSP.id(1u, 0u));
			cmaster.CMSetAntiVoxSourceWhat();
			if (console.PowerOn)
			{
				EnableVAC1(value);
			}
		}
	}

	public static bool VAC2Enabled
	{
		get
		{
			return vac2_enabled;
		}
		set
		{
			vac2_enabled = value;
			cmaster.CMSetAntiVoxSourceWhat();
			if (console.PowerOn)
			{
				EnableVAC2(value);
			}
		}
	}

	public static bool VAC1LatencyManual
	{
		get
		{
			return vac1_latency_manual;
		}
		set
		{
			vac1_latency_manual = value;
		}
	}

	public static bool VAC1LatencyManualOut
	{
		get
		{
			return vac1_latency_manual_out;
		}
		set
		{
			vac1_latency_manual_out = value;
		}
	}

	public static bool VAC1LatencyPAInManual
	{
		get
		{
			return vac1_latency_pa_in_manual;
		}
		set
		{
			vac1_latency_pa_in_manual = value;
		}
	}

	public static bool VAC1LatencyPAOutManual
	{
		get
		{
			return vac1_latency_pa_out_manual;
		}
		set
		{
			vac1_latency_pa_out_manual = value;
		}
	}

	public static bool VAC2LatencyManual
	{
		get
		{
			return vac2_latency_manual;
		}
		set
		{
			vac2_latency_manual = value;
		}
	}

	public static bool VAC2LatencyOutManual
	{
		get
		{
			return vac2_latency_out_manual;
		}
		set
		{
			vac2_latency_out_manual = value;
		}
	}

	public static bool VAC2LatencyPAInManual
	{
		get
		{
			return vac2_latency_pa_in_manual;
		}
		set
		{
			vac2_latency_pa_in_manual = value;
		}
	}

	public static bool VAC2LatencyPAOutManual
	{
		get
		{
			return vac2_latency_pa_out_manual;
		}
		set
		{
			vac2_latency_pa_out_manual = value;
		}
	}

	public static bool VACBypass
	{
		get
		{
			return vac_bypass;
		}
		set
		{
			vac_bypass = value;
			cmaster.CMSetTXAPanelGain1(WDSP.id(1u, 0u));
			ivac.SetIVACbypass(0, Convert.ToInt32(value));
			ivac.SetIVACbypass(1, Convert.ToInt32(value));
		}
	}

	public static bool VACRBReset
	{
		get
		{
			return vac_rb_reset;
		}
		set
		{
			vac_rb_reset = value;
			ivac.SetIVACRBReset(0, Convert.ToInt32(value));
		}
	}

	public static bool VAC2RBReset
	{
		get
		{
			return vac2_rb_reset;
		}
		set
		{
			vac2_rb_reset = value;
			ivac.SetIVACRBReset(1, Convert.ToInt32(value));
		}
	}

	public static double VACPreamp
	{
		get
		{
			return vac_preamp;
		}
		set
		{
			vac_preamp = value;
			cmaster.CMSetTXAPanelGain1(WDSP.id(1u, 0u));
			ivac.SetIVACpreamp(0, value);
		}
	}

	public static double VAC2TXScale
	{
		get
		{
			return vac2_tx_scale;
		}
		set
		{
			vac2_tx_scale = value;
			ivac.SetIVACpreamp(1, value);
		}
	}

	public static double VACRXScale
	{
		get
		{
			return vac_rx_scale;
		}
		set
		{
			vac_rx_scale = value;
			ivac.SetIVACrxscale(0, value);
		}
	}

	public static double VAC2RXScale
	{
		get
		{
			return vac2_rx_scale;
		}
		set
		{
			vac2_rx_scale = value;
			ivac.SetIVACrxscale(1, value);
		}
	}

	public static DSPMode TXDSPMode
	{
		get
		{
			return tx_dsp_mode;
		}
		set
		{
			tx_dsp_mode = value;
			cmaster.CMSetTXAVoxRun(0);
			cmaster.CMSetTXAPanelGain1(WDSP.id(1u, 0u));
		}
	}

	public static int SampleRate1
	{
		get
		{
			return sample_rate1;
		}
		set
		{
			sample_rate1 = value;
			SetOutCount();
			cmaster.SetXcmInrate(0, value);
		}
	}

	public static int SampleRateRX2
	{
		get
		{
			return sample_rate_rx2;
		}
		set
		{
			sample_rate_rx2 = value;
			cmaster.SetXcmInrate(1, value);
			SetOutCountRX2();
		}
	}

	public static int SampleRateTX
	{
		get
		{
			return sample_rate_tx;
		}
		set
		{
			sample_rate_tx = value;
			SetOutCountTX();
		}
	}

	public static int SampleRate2
	{
		get
		{
			return sample_rate2;
		}
		set
		{
			sample_rate2 = value;
			ivac.SetIVACvacRate(0, value);
		}
	}

	public static int SampleRate3
	{
		get
		{
			return sample_rate3;
		}
		set
		{
			sample_rate3 = value;
			ivac.SetIVACvacRate(1, value);
		}
	}

	public static int BlockSize
	{
		get
		{
			return block_size1;
		}
		set
		{
			block_size1 = value;
			SetOutCount();
		}
	}

	public static int BlockSizeRX2
	{
		get
		{
			return block_size_rx2;
		}
		set
		{
			block_size_rx2 = value;
			SetOutCountRX2();
		}
	}

	public static int BlockSizeTX
	{
		get
		{
			return block_size_tx;
		}
		set
		{
			block_size_tx = value;
			SetOutCountTX();
		}
	}

	public static int BlockSizeVAC
	{
		get
		{
			return block_size_vac;
		}
		set
		{
			block_size_vac = value;
			ivac.SetIVACvacSize(0, value);
		}
	}

	public static int BlockSizeVAC2
	{
		get
		{
			return block_size_vac2;
		}
		set
		{
			block_size_vac2 = value;
			ivac.SetIVACvacSize(1, value);
		}
	}

	public static bool VACStereo
	{
		get
		{
			return vac_stereo;
		}
		set
		{
			vac_stereo = value;
			ivac.SetIVACstereo(0, Convert.ToInt32(value));
		}
	}

	public static bool VAC2Stereo
	{
		set
		{
			vac2_stereo = value;
			ivac.SetIVACstereo(1, Convert.ToInt32(value));
		}
	}

	public static bool VACOutputIQ
	{
		get
		{
			return vac_output_iq;
		}
		set
		{
			vac_output_iq = value;
			ivac.SetIVACiqType(0, Convert.ToInt32(value));
		}
	}

	public static bool VAC2OutputIQ
	{
		set
		{
			vac2_output_iq = value;
			ivac.SetIVACiqType(1, Convert.ToInt32(value));
		}
	}

	public static bool VACOutputRX2
	{
		set
		{
			vac_output_rx2 = value;
		}
	}

	public static bool VACCorrectIQ
	{
		set
		{
			vac_correct_iq = value;
		}
	}

	public static bool VAC2CorrectIQ
	{
		set
		{
			vac2_correct_iq = value;
		}
	}

	public static bool VOXActive
	{
		get
		{
			return vox_active;
		}
		set
		{
			vox_active = value;
		}
	}

	public static int Host2
	{
		get
		{
			return host2;
		}
		set
		{
			host2 = value;
		}
	}

	public static int Host3
	{
		get
		{
			return host3;
		}
		set
		{
			host3 = value;
		}
	}

	public static int Input2
	{
		get
		{
			return input_dev2;
		}
		set
		{
			input_dev2 = value;
		}
	}

	public static int Input3
	{
		get
		{
			return input_dev3;
		}
		set
		{
			input_dev3 = value;
		}
	}

	public static int Output2
	{
		get
		{
			return output_dev2;
		}
		set
		{
			output_dev2 = value;
		}
	}

	public static int Output3
	{
		get
		{
			return output_dev3;
		}
		set
		{
			output_dev3 = value;
		}
	}

	public static int Latency2
	{
		set
		{
			latency2 = value;
		}
	}

	public static int Latency2_Out
	{
		set
		{
			latency2_out = value;
		}
	}

	public static int LatencyPAIn
	{
		set
		{
			latency_pa_in = value;
		}
	}

	public static int LatencyPAOut
	{
		set
		{
			latency_pa_out = value;
		}
	}

	public static int VAC2LatencyOut
	{
		set
		{
			vac2_latency_out = value;
		}
	}

	public static int VAC2LatencyPAIn
	{
		set
		{
			vac2_latency_pa_in = value;
		}
	}

	public static int VAC2LatencyPAOut
	{
		set
		{
			vac2_latency_pa_out = value;
		}
	}

	public static int Latency3
	{
		set
		{
			latency3 = value;
		}
	}

	public static double VAC1FeedbackGainIn
	{
		set
		{
			vac1_feedbackgainIn = value;
			ivac.SetIVACFeedbackGain(0, 1, vac1_feedbackgainIn);
		}
	}

	public static double VAC1SlewTimeIn
	{
		set
		{
			vac1_slewtimeIn = value;
			ivac.SetIVACSlewTime(0, 1, vac1_slewtimeIn);
		}
	}

	public static int VAC1PropRingMinIn
	{
		get
		{
			return vac1_prop_ringminIn;
		}
		set
		{
			vac1_prop_ringminIn = value;
			ivac.SetIVACPropRingMin(0, 1, vac1_prop_ringminIn);
		}
	}

	public static int VAC1PropRingMaxIn
	{
		get
		{
			return vac1_prop_ringmaxIn;
		}
		set
		{
			if (isPowerOfTwo(value))
			{
				vac1_prop_ringmaxIn = value;
				ivac.SetIVACPropRingMax(0, 1, vac1_prop_ringmaxIn);
			}
		}
	}

	public static int VAC1FFRingMinIn
	{
		get
		{
			return vac1_ff_ringminIn;
		}
		set
		{
			vac1_ff_ringminIn = value;
			ivac.SetIVACFFRingMin(0, 1, vac1_ff_ringminIn);
		}
	}

	public static int VAC1FFRingMaxIn
	{
		get
		{
			return vac1_ff_ringmaxIn;
		}
		set
		{
			if (isPowerOfTwo(value))
			{
				vac1_ff_ringmaxIn = value;
				ivac.SetIVACFFRingMax(0, 1, vac1_ff_ringmaxIn);
			}
		}
	}

	public static double VAC1FFAlphaIn
	{
		set
		{
			vac1_ff_alphaIn = value;
			ivac.SetIVACFFAlpha(0, 1, vac1_ff_alphaIn);
		}
	}

	public static double VAC1OldVarIn
	{
		set
		{
			vac1_oldVarIn = value;
		}
	}

	public unsafe static bool VAC1ControlFlagIn
	{
		get
		{
			int num = default(int);
			ivac.GetIVACControlFlag(0, 1, &num);
			return num == 1;
		}
	}

	public static double VAC1FeedbackGainOut
	{
		set
		{
			vac1_feedbackgainOut = value;
			ivac.SetIVACFeedbackGain(0, 0, vac1_feedbackgainOut);
		}
	}

	public static double VAC1SlewTimeOut
	{
		set
		{
			vac1_slewtimeOut = value;
			ivac.SetIVACSlewTime(0, 0, vac1_slewtimeOut);
		}
	}

	public static int VAC1PropRingMinOut
	{
		get
		{
			return vac1_prop_ringminOut;
		}
		set
		{
			vac1_prop_ringminOut = value;
			ivac.SetIVACPropRingMin(0, 0, vac1_prop_ringminOut);
		}
	}

	public static int VAC1PropRingMaxOut
	{
		get
		{
			return vac1_prop_ringmaxOut;
		}
		set
		{
			if (isPowerOfTwo(value))
			{
				vac1_prop_ringmaxOut = value;
				ivac.SetIVACPropRingMax(0, 0, vac1_prop_ringmaxOut);
			}
		}
	}

	public static int VAC1FFRingMinOut
	{
		get
		{
			return vac1_ff_ringminOut;
		}
		set
		{
			vac1_ff_ringminOut = value;
			ivac.SetIVACFFRingMin(0, 0, vac1_ff_ringminOut);
		}
	}

	public static int VAC1FFRingMaxOut
	{
		get
		{
			return vac1_ff_ringmaxOut;
		}
		set
		{
			if (isPowerOfTwo(value))
			{
				vac1_ff_ringmaxOut = value;
				ivac.SetIVACFFRingMax(0, 0, vac1_ff_ringmaxOut);
			}
		}
	}

	public static double VAC1FFAlphaOut
	{
		set
		{
			vac1_ff_alphaOut = value;
			ivac.SetIVACFFAlpha(0, 0, vac1_ff_alphaOut);
		}
	}

	public static double VAC1OldVarOut
	{
		set
		{
			vac1_oldVarOut = value;
		}
	}

	public unsafe static bool VAC1ControlFlagOut
	{
		get
		{
			int num = default(int);
			ivac.GetIVACControlFlag(0, 0, &num);
			return num == 1;
		}
	}

	public static double VAC2FeedbackGainIn
	{
		set
		{
			vac2_feedbackgainIn = value;
			ivac.SetIVACFeedbackGain(1, 1, vac2_feedbackgainIn);
		}
	}

	public static double VAC2SlewTimeIn
	{
		set
		{
			vac2_slewtimeIn = value;
			ivac.SetIVACSlewTime(1, 1, vac2_slewtimeIn);
		}
	}

	public static int VAC2PropRingMinIn
	{
		set
		{
			vac2_prop_ringminIn = value;
			ivac.SetIVACPropRingMin(1, 1, vac2_prop_ringminIn);
		}
	}

	public static int VAC2PropRingMaxIn
	{
		get
		{
			return vac2_prop_ringmaxIn;
		}
		set
		{
			if (isPowerOfTwo(value))
			{
				vac2_prop_ringmaxIn = value;
				ivac.SetIVACPropRingMax(1, 1, vac2_prop_ringmaxIn);
			}
		}
	}

	public static int VAC2FFRingMinIn
	{
		set
		{
			vac2_ff_ringminIn = value;
			ivac.SetIVACFFRingMin(1, 1, vac2_ff_ringminIn);
		}
	}

	public static int VAC2FFRingMaxIn
	{
		get
		{
			return vac2_ff_ringmaxIn;
		}
		set
		{
			if (isPowerOfTwo(value))
			{
				vac2_ff_ringmaxIn = value;
				ivac.SetIVACFFRingMax(1, 1, vac2_ff_ringmaxIn);
			}
		}
	}

	public static double VAC2FFAlphaIn
	{
		set
		{
			vac2_ff_alphaIn = value;
			ivac.SetIVACFFAlpha(1, 1, vac2_ff_alphaIn);
		}
	}

	public static double VAC2OldVarIn
	{
		set
		{
			vac2_oldVarIn = value;
		}
	}

	public unsafe static bool VAC2ControlFlagIn
	{
		get
		{
			int num = default(int);
			ivac.GetIVACControlFlag(1, 1, &num);
			return num == 1;
		}
	}

	public static double VAC2FeedbackGainOut
	{
		set
		{
			vac2_feedbackgainOut = value;
			ivac.SetIVACFeedbackGain(1, 0, vac2_feedbackgainOut);
		}
	}

	public static double VAC2SlewTimeOut
	{
		set
		{
			vac2_slewtimeOut = value;
			ivac.SetIVACSlewTime(1, 0, vac2_slewtimeOut);
		}
	}

	public static int VAC2PropRingMinOut
	{
		set
		{
			vac2_prop_ringminOut = value;
			ivac.SetIVACPropRingMin(1, 0, vac2_prop_ringminOut);
		}
	}

	public static int VAC2PropRingMaxOut
	{
		get
		{
			return vac2_prop_ringmaxOut;
		}
		set
		{
			if (isPowerOfTwo(value))
			{
				vac2_prop_ringmaxOut = value;
				ivac.SetIVACPropRingMax(1, 0, vac2_prop_ringmaxOut);
			}
		}
	}

	public static int VAC2FFRingMinOut
	{
		set
		{
			vac2_ff_ringminOut = value;
			ivac.SetIVACFFRingMin(1, 0, vac2_ff_ringminOut);
		}
	}

	public static int VAC2FFRingMaxOut
	{
		get
		{
			return vac2_ff_ringmaxOut;
		}
		set
		{
			if (isPowerOfTwo(value))
			{
				vac2_ff_ringmaxOut = value;
				ivac.SetIVACFFRingMax(1, 0, vac2_ff_ringmaxOut);
			}
		}
	}

	public static double VAC2FFAlphaOut
	{
		set
		{
			vac2_ff_alphaOut = value;
			ivac.SetIVACFFAlpha(1, 0, vac2_ff_alphaOut);
		}
	}

	public static double VAC2OldVarOut
	{
		set
		{
			vac2_oldVarOut = value;
		}
	}

	public unsafe static bool VAC2ControlFlagOut
	{
		get
		{
			int num = default(int);
			ivac.GetIVACControlFlag(1, 0, &num);
			return num == 1;
		}
	}

	public unsafe static bool MuteRX1
	{
		get
		{
			return mute_rx1;
		}
		set
		{
			mute_rx1 = value;
			setupIVACforMon();
			cmaster.SetAAudioMixWhat(null, 0, 0, !mute_rx1);
			cmaster.SetAAudioMixWhat(null, 0, 1, !mute_rx1);
		}
	}

	public unsafe static bool MuteRX2
	{
		get
		{
			return mute_rx2;
		}
		set
		{
			mute_rx2 = value;
			setupIVACforMon();
			cmaster.SetAAudioMixWhat(null, 0, 2, !mute_rx2);
		}
	}

	public static int OutRate
	{
		get
		{
			return out_rate;
		}
		set
		{
			out_rate = value;
			SetOutCount();
		}
	}

	public static int OutRateRX2
	{
		get
		{
			return out_rate_rx2;
		}
		set
		{
			out_rate_rx2 = value;
			SetOutCountRX2();
		}
	}

	public static int OutRateTX
	{
		get
		{
			return out_rate_tx;
		}
		set
		{
			out_rate_tx = value;
			SetOutCountTX();
		}
	}

	public static int OutCount
	{
		get
		{
			return out_count;
		}
		set
		{
			out_count = value;
		}
	}

	public static int OutCountRX2
	{
		get
		{
			return out_count_rx2;
		}
		set
		{
			out_count_rx2 = value;
		}
	}

	public static int OutCountTX
	{
		get
		{
			return out_count_tx;
		}
		set
		{
			out_count_tx = value;
		}
	}

	public static int VAC1SwapIQ
	{
		get
		{
			return _swap_iq_vac1;
		}
		set
		{
			_swap_iq_vac1 = value;
			ivac.SetIVACswapIQout(0, _swap_iq_vac1);
		}
	}

	public static int VAC2SwapIQ
	{
		get
		{
			return _swap_iq_vac2;
		}
		set
		{
			_swap_iq_vac2 = value;
			ivac.SetIVACswapIQout(1, _swap_iq_vac2);
		}
	}

	public static int VAC1ExclusiveOut
	{
		get
		{
			return _exclusive_out_vac1;
		}
		set
		{
			_exclusive_out_vac1 = value;
			ivac.SetIVACExclusiveOut(0, _exclusive_out_vac1);
		}
	}

	public static int VAC2ExclusiveOut
	{
		get
		{
			return _exclusive_out_vac2;
		}
		set
		{
			_exclusive_out_vac2 = value;
			ivac.SetIVACExclusiveOut(1, _exclusive_out_vac2);
		}
	}

	public static int VAC1ExclusiveIn
	{
		get
		{
			return _exclusive_in_vac1;
		}
		set
		{
			_exclusive_in_vac1 = value;
			ivac.SetIVACExclusiveIn(0, _exclusive_in_vac1);
		}
	}

	public static int VAC2ExclusiveIn
	{
		get
		{
			return _exclusive_in_vac2;
		}
		set
		{
			_exclusive_in_vac2 = value;
			ivac.SetIVACExclusiveIn(1, _exclusive_in_vac2);
		}
	}

	public static RadioProtocol LastRadioProtocol
	{
		get
		{
			return _lastRadioProtocol;
		}
		set
		{
			_lastRadioProtocol = value;
		}
	}

	public static HPSDRHW LastRadioHardware
	{
		get
		{
			return _lastRadiohadware;
		}
		set
		{
			_lastRadiohadware = value;
		}
	}

	public static int ScopePixelWidth
	{
		set
		{
			bool flag = false;
			lock (m_objScope_width_lock)
			{
				flag = value != m_nScope_pixel_width;
				m_nScope_pixel_width = value;
			}
			if (flag)
			{
				ScopeTime = m_nScope_time;
			}
		}
	}

	public static int ScopeTime
	{
		get
		{
			return m_nScope_time;
		}
		set
		{
			m_nScope_time = value;
			double num = 0.0;
			lock (m_objScope_width_lock)
			{
				num = m_nScope_pixel_width;
			}
			m_nScope_samples_per_pixel = (int)((double)OutRate / 1000.0 * (double)m_nScope_time / num);
			m_nScope_samples_per_pixel_tx = (int)((double)OutRateTX / 1000.0 * (double)m_nScope_time / num);
		}
	}

	public static float[] ScopeMin
	{
		set
		{
			lock (m_objArrayLock)
			{
				scope_min = value;
				scope_pixel_index = 0;
				int val = ((scope_min != null) ? scope_min.Length : 0);
				int val2 = ((scope_max != null) ? scope_max.Length : 0);
				scope_array_len = Math.Min(val, val2);
			}
		}
	}

	public static float[] ScopeMax
	{
		set
		{
			lock (m_objArrayLock)
			{
				scope_max = value;
				scope_pixel_index = 0;
				int val = ((scope_min != null) ? scope_min.Length : 0);
				int val2 = ((scope_max != null) ? scope_max.Length : 0);
				scope_array_len = Math.Min(val, val2);
			}
		}
	}

	public static float[] Scope2Max
	{
		set
		{
			lock (m_objArrayLock)
			{
				scope2_max = value;
				scope2_pixel_index = 0;
				int val = ((scope2_min != null) ? scope2_min.Length : 0);
				int val2 = ((scope2_max != null) ? scope2_max.Length : 0);
				scope_array_len = Math.Min(val, val2);
			}
		}
	}

	public static float[] Scope2Min
	{
		set
		{
			lock (m_objArrayLock)
			{
				scope2_min = value;
				scope2_pixel_index = 0;
				int val = ((scope2_min != null) ? scope2_min.Length : 0);
				int val2 = ((scope2_max != null) ? scope2_max.Length : 0);
				scope_array_len = Math.Min(val, val2);
			}
		}
	}

	private static void setupIVACforMon()
	{
		if (mon)
		{
			ivac.SetIVACmon(0, 1);
			ivac.SetIVACmon(1, 0);
			ivac.SetIVACmonVol(0, monitor_volume);
			cmaster.SetTCIRxAudioMon(0, 1);
			cmaster.SetTCIRxAudioMon(1, 1);
			cmaster.SetTCIRxAudioMonVol(0, monitor_volume);
			cmaster.SetTCIRxAudioMonVol(1, monitor_volume);
		}
		else
		{
			ivac.SetIVACmon(0, 0);
			ivac.SetIVACmon(1, 0);
			cmaster.SetTCIRxAudioMon(0, 0);
			cmaster.SetTCIRxAudioMon(1, 0);
		}
	}

	private static bool isPowerOfTwo(int x)
	{
		if (x != 0)
		{
			return (x & (x - 1)) == 0;
		}
		return false;
	}

	private static void SetOutCount()
	{
		if (out_rate >= sample_rate1)
		{
			OutCount = block_size1 * (out_rate / sample_rate1);
		}
		else
		{
			OutCount = block_size1 / (sample_rate1 / out_rate);
		}
	}

	private static void SetOutCountRX2()
	{
		if (out_rate_rx2 >= sample_rate_rx2)
		{
			OutCountRX2 = block_size_rx2 * (out_rate_rx2 / sample_rate_rx2);
		}
		else
		{
			OutCountRX2 = block_size_rx2 / (sample_rate_rx2 / out_rate_rx2);
		}
	}

	private static void SetOutCountTX()
	{
		if (out_rate_tx >= sample_rate_tx)
		{
			OutCountTX = block_size_tx * (out_rate_tx / sample_rate_tx);
		}
		else
		{
			OutCountTX = block_size_tx / (sample_rate_tx / out_rate_tx);
		}
	}

	public static ArrayList GetPAHosts()
	{
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < PA19.PA_GetHostApiCount(); i++)
		{
			arrayList.Add(PA19.PA_GetHostApiInfo(i).name);
		}
		return arrayList;
	}

	public static ArrayList GetPAInputDevices(int hostIndex)
	{
		ArrayList arrayList = new ArrayList();
		if (hostIndex >= PA19.PA_GetHostApiCount())
		{
			return arrayList;
		}
		PA19.PaHostApiInfo paHostApiInfo = PA19.PA_GetHostApiInfo(hostIndex);
		for (int i = 0; i < paHostApiInfo.deviceCount; i++)
		{
			PA19.PaDeviceInfo paDeviceInfo = PA19.PA_GetDeviceInfo(PA19.PA_HostApiDeviceIndexToDeviceIndex(hostIndex, i));
			if (paDeviceInfo.maxInputChannels <= 0)
			{
				continue;
			}
			string text = paDeviceInfo.name;
			int num = text.IndexOf("- ");
			if (num > 0)
			{
				char c = text[num - 1];
				if (c >= '0' && c <= '9')
				{
					int num2 = text.IndexOf("(");
					text = paDeviceInfo.name.Substring(0, num2 + 1);
					text += paDeviceInfo.name.Substring(num + 2, paDeviceInfo.name.Length - num - 2);
				}
			}
			arrayList.Add(new PADeviceInfo(text, i));
		}
		return arrayList;
	}

	public static bool CheckPAInputDevices(int hostIndex, string name)
	{
		PA19.PaHostApiInfo paHostApiInfo = PA19.PA_GetHostApiInfo(hostIndex);
		for (int i = 0; i < paHostApiInfo.deviceCount; i++)
		{
			PA19.PaDeviceInfo paDeviceInfo = PA19.PA_GetDeviceInfo(PA19.PA_HostApiDeviceIndexToDeviceIndex(hostIndex, i));
			if (paDeviceInfo.maxInputChannels > 0 && paDeviceInfo.name.Contains(name))
			{
				return true;
			}
		}
		return false;
	}

	public static ArrayList GetPAOutputDevices(int hostIndex)
	{
		ArrayList arrayList = new ArrayList();
		if (hostIndex >= PA19.PA_GetHostApiCount())
		{
			return arrayList;
		}
		PA19.PaHostApiInfo paHostApiInfo = PA19.PA_GetHostApiInfo(hostIndex);
		for (int i = 0; i < paHostApiInfo.deviceCount; i++)
		{
			PA19.PaDeviceInfo paDeviceInfo = PA19.PA_GetDeviceInfo(PA19.PA_HostApiDeviceIndexToDeviceIndex(hostIndex, i));
			if (paDeviceInfo.maxOutputChannels <= 0)
			{
				continue;
			}
			string text = paDeviceInfo.name;
			int num = text.IndexOf("- ");
			if (num > 0)
			{
				char c = text[num - 1];
				if (c >= '0' && c <= '9')
				{
					int num2 = text.IndexOf("(");
					text = paDeviceInfo.name.Substring(0, num2 + 1);
					text += paDeviceInfo.name.Substring(num + 2, paDeviceInfo.name.Length - num - 2);
				}
			}
			arrayList.Add(new PADeviceInfo(text, i));
		}
		return arrayList;
	}

	public static bool CheckPAOutputDevices(int hostIndex, string name)
	{
		PA19.PaHostApiInfo paHostApiInfo = PA19.PA_GetHostApiInfo(hostIndex);
		for (int i = 0; i < paHostApiInfo.deviceCount; i++)
		{
			PA19.PaDeviceInfo paDeviceInfo = PA19.PA_GetDeviceInfo(PA19.PA_HostApiDeviceIndexToDeviceIndex(hostIndex, i));
			if (paDeviceInfo.maxOutputChannels > 0 && paDeviceInfo.name.Contains(name))
			{
				return true;
			}
		}
		return false;
	}

	public static void EnableVAC1(bool enable)
	{
		if (enable)
		{
			int n = 1;
			_ = sample_rate2;
			_ = block_size_vac;
			double lat = (vac1_latency_manual ? ((double)latency2 / 1000.0) : PA19.PA_GetDeviceInfo(input_dev2).defaultLowInputLatency);
			double lat2 = (vac1_latency_manual_out ? ((double)latency2_out / 1000.0) : PA19.PA_GetDeviceInfo(output_dev2).defaultLowOutputLatency);
			double lat3 = (vac1_latency_pa_in_manual ? ((double)latency_pa_in / 1000.0) : PA19.PA_GetDeviceInfo(input_dev2).defaultLowInputLatency);
			double lat4 = (vac1_latency_pa_out_manual ? ((double)latency_pa_out / 1000.0) : PA19.PA_GetDeviceInfo(output_dev2).defaultLowOutputLatency);
			if (vac_output_iq)
			{
				n = 2;
				_ = sample_rate1;
				_ = block_size1;
			}
			else if (vac_stereo)
			{
				n = 2;
			}
			VACRBReset = true;
			ivac.SetIVAChostAPIindex(0, host2);
			ivac.SetIVACinputDEVindex(0, input_dev2);
			ivac.SetIVACoutputDEVindex(0, output_dev2);
			ivac.SetIVACnumChannels(0, n);
			ivac.SetIVACInLatency(0, lat, 0);
			ivac.SetIVACOutLatency(0, lat2, 0);
			ivac.SetIVACPAInLatency(0, lat3, 0);
			ivac.SetIVACPAOutLatency(0, lat4, 1);
			ivac.SetIVACFeedbackGain(0, 0, vac1_feedbackgainOut);
			ivac.SetIVACFeedbackGain(0, 1, vac1_feedbackgainIn);
			ivac.SetIVACSlewTime(0, 0, vac1_slewtimeOut);
			ivac.SetIVACSlewTime(0, 1, vac1_slewtimeIn);
			ivac.SetIVACPropRingMin(0, 0, vac1_prop_ringminOut);
			ivac.SetIVACPropRingMin(0, 1, vac1_prop_ringminIn);
			ivac.SetIVACPropRingMax(0, 0, vac1_prop_ringmaxOut);
			ivac.SetIVACPropRingMax(0, 1, vac1_prop_ringmaxIn);
			ivac.SetIVACFFRingMin(0, 0, vac1_ff_ringminOut);
			ivac.SetIVACFFRingMin(0, 1, vac1_ff_ringminIn);
			ivac.SetIVACFFRingMax(0, 0, vac1_ff_ringmaxOut);
			ivac.SetIVACFFRingMax(0, 1, vac1_ff_ringmaxIn);
			ivac.SetIVACFFAlpha(0, 0, vac1_ff_alphaOut);
			ivac.SetIVACFFAlpha(0, 1, vac1_ff_alphaIn);
			ivac.SetIVACswapIQout(0, _swap_iq_vac1);
			ivac.SetIVACinitialVars(0, vac1_oldVarIn, vac1_oldVarOut);
			try
			{
				if (ivac.StartAudioIVAC(0) == 1 && console.PowerOn)
				{
					ivac.SetIVACrun(0, 1);
				}
			}
			catch (Exception)
			{
				MessageBox.Show("The program is having trouble starting the VAC audio streams.\nPlease examine the VAC related settings on the Setup Form -> Audio Tab and try again.", "VAC Audio Stream Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		else
		{
			ivac.SetIVACrun(0, 0);
			ivac.StopAudioIVAC(0);
		}
		Thread.Sleep(10);
	}

	public static void EnableVAC2(bool enable)
	{
		if (enable)
		{
			int n = 1;
			_ = sample_rate3;
			_ = block_size_vac2;
			double lat = (vac2_latency_manual ? ((double)latency3 / 1000.0) : PA19.PA_GetDeviceInfo(input_dev3).defaultLowInputLatency);
			double lat2 = (vac2_latency_out_manual ? ((double)vac2_latency_out / 1000.0) : PA19.PA_GetDeviceInfo(output_dev3).defaultLowOutputLatency);
			double lat3 = (vac2_latency_pa_in_manual ? ((double)vac2_latency_pa_in / 1000.0) : PA19.PA_GetDeviceInfo(input_dev3).defaultLowInputLatency);
			double lat4 = (vac2_latency_pa_out_manual ? ((double)vac2_latency_pa_out / 1000.0) : PA19.PA_GetDeviceInfo(output_dev3).defaultLowOutputLatency);
			if (vac2_output_iq)
			{
				n = 2;
				_ = sample_rate_rx2;
				_ = block_size_rx2;
			}
			else if (vac2_stereo)
			{
				n = 2;
			}
			VAC2RBReset = true;
			ivac.SetIVAChostAPIindex(1, host3);
			ivac.SetIVACinputDEVindex(1, input_dev3);
			ivac.SetIVACoutputDEVindex(1, output_dev3);
			ivac.SetIVACnumChannels(1, n);
			ivac.SetIVACInLatency(1, lat, 0);
			ivac.SetIVACOutLatency(1, lat2, 0);
			ivac.SetIVACPAInLatency(1, lat3, 0);
			ivac.SetIVACPAOutLatency(1, lat4, 1);
			ivac.SetIVACFeedbackGain(1, 0, vac2_feedbackgainOut);
			ivac.SetIVACFeedbackGain(1, 1, vac2_feedbackgainIn);
			ivac.SetIVACSlewTime(1, 0, vac2_slewtimeOut);
			ivac.SetIVACSlewTime(1, 1, vac2_slewtimeIn);
			ivac.SetIVACPropRingMin(1, 0, vac2_prop_ringminOut);
			ivac.SetIVACPropRingMin(1, 1, vac2_prop_ringminIn);
			ivac.SetIVACPropRingMax(1, 0, vac2_prop_ringmaxOut);
			ivac.SetIVACPropRingMax(1, 1, vac2_prop_ringmaxIn);
			ivac.SetIVACFFRingMin(1, 0, vac2_ff_ringminOut);
			ivac.SetIVACFFRingMin(1, 1, vac2_ff_ringminIn);
			ivac.SetIVACFFRingMax(1, 0, vac2_ff_ringmaxOut);
			ivac.SetIVACFFRingMax(1, 1, vac2_ff_ringmaxIn);
			ivac.SetIVACFFAlpha(1, 0, vac2_ff_alphaOut);
			ivac.SetIVACFFAlpha(1, 1, vac2_ff_alphaIn);
			ivac.SetIVACswapIQout(1, _swap_iq_vac2);
			ivac.SetIVACinitialVars(1, vac2_oldVarIn, vac2_oldVarOut);
			try
			{
				if (ivac.StartAudioIVAC(1) == 1 && console.PowerOn)
				{
					ivac.SetIVACrun(1, 1);
				}
			}
			catch (Exception)
			{
				MessageBox.Show("The program is having trouble starting the VAC audio streams.\nPlease examine the VAC related settings on the Setup Form -> Audio Tab and try again.", "VAC2 Audio Stream Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		else
		{
			ivac.SetIVACrun(1, 0);
			ivac.StopAudioIVAC(1);
		}
		Thread.Sleep(10);
	}

	public static bool Start()
	{
		bool flag = false;
		phase_buf_l = new float[2048];
		phase_buf_r = new float[2048];
		Console console = Console.getConsole();
		Cursor current = Cursor.Current;
		Cursor.Current = Cursors.WaitCursor;
		int num = NetworkIO.InitRadio();
		Cursor.Current = current;
		switch (num)
		{
		case -101:
			MessageBox.Show(NetworkIO.GetFWVersionErrorMsg, "Firmware Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return false;
		default:
			MessageBox.Show("Error starting SDR hardware, is it connected and powered?", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return false;
		case 0:
		{
			if (NetworkIO.CurrentRadioProtocol == RadioProtocol.USB)
			{
				Audio.console.SampleRateTX = 48000;
				WDSP.SetTXACFIRRun(cmaster.chid(cmaster.inid(1, 0), 0), run: false);
			}
			else
			{
				Audio.console.SampleRateTX = 192000;
				WDSP.SetTXACFIRRun(cmaster.chid(cmaster.inid(1, 0), 0), run: true);
			}
			bool flag2 = false;
			if (LastRadioHardware == HPSDRHW.Unknown || (LastRadioHardware != HPSDRHW.Unknown && LastRadioHardware != NetworkIO.BoardID))
			{
				flag2 = true;
				LastRadioHardware = NetworkIO.BoardID;
			}
			if (LastRadioProtocol == RadioProtocol.None || (LastRadioProtocol != RadioProtocol.None && LastRadioProtocol != NetworkIO.CurrentRadioProtocol))
			{
				flag2 = true;
				LastRadioProtocol = NetworkIO.CurrentRadioProtocol;
			}
			if (flag2)
			{
				Audio.console.psform.SetDefaultPeaks();
			}
			console.SetupForm.InitAudioTab();
			console.SetupForm.ForceAudioReset();
			cmaster.PSLoopback = cmaster.PSLoopback;
			if (NetworkIO.StartAudioNative() == 0)
			{
				flag = true;
			}
			if (!console.IsSetupFormNull && console.SetupForm.SelectedRadioList != null)
			{
				if (flag)
				{
					console.SetupForm.SelectedRadioList.RadioConnected();
				}
				else
				{
					console.SetupForm.SelectedRadioList.RadioDisconnected();
				}
				console.SetupForm.SelectedRadioList.PLLLocked(NetworkIO.CurrentRadioProtocol == RadioProtocol.ETH && NetworkIO.getHaveSync() == 1 && NetworkIO.GetPLLLock());
			}
			return flag;
		}
		}
	}

	public static void Stop()
	{
		Console console = Console.getConsole();
		NetworkIO.StopAudio();
		if (!console.IsSetupFormNull && console.SetupForm.SelectedRadioList != null)
		{
			console.SetupForm.SelectedRadioList.DisconnectAll();
		}
	}

	public unsafe static void DoScope(float* buf, int frameCount)
	{
		lock (m_objArrayLock)
		{
			if (scope_min == null || scope_max == null)
			{
				return;
			}
			int num = ((!mox) ? m_nScope_samples_per_pixel : m_nScope_samples_per_pixel_tx);
			for (int i = 0; i < frameCount; i++)
			{
				if (Display.CurrentDisplayMode == DisplayMode.SCOPE || Display.CurrentDisplayMode == DisplayMode.SCOPE2 || Display.CurrentDisplayMode == DisplayMode.PANASCOPE || Display.CurrentDisplayMode == DisplayMode.SPECTRASCOPE)
				{
					if (buf[i] < scope_pixel_min)
					{
						scope_pixel_min = buf[i];
					}
					if (buf[i] > scope_pixel_max)
					{
						scope_pixel_max = buf[i];
					}
				}
				else
				{
					scope_pixel_min = buf[i];
					scope_pixel_max = buf[i];
				}
				scope_sample_index++;
				if (scope_sample_index >= num)
				{
					scope_sample_index = 0;
					scope_min[scope_pixel_index] = scope_pixel_min;
					scope_max[scope_pixel_index] = scope_pixel_max;
					scope_pixel_min = float.MaxValue;
					scope_pixel_max = float.MinValue;
					scope_pixel_index++;
					if (scope_pixel_index >= scope_array_len)
					{
						scope_pixel_index = 0;
					}
				}
			}
		}
	}

	public unsafe static void DoScope2(float* buf, int frameCount)
	{
		lock (m_objArrayLock)
		{
			if (scope2_min == null || scope2_max == null)
			{
				return;
			}
			int num = ((!mox) ? m_nScope_samples_per_pixel : m_nScope_samples_per_pixel_tx);
			for (int i = 0; i < frameCount; i++)
			{
				if (Display.CurrentDisplayMode == DisplayMode.SCOPE || Display.CurrentDisplayMode == DisplayMode.SCOPE2 || Display.CurrentDisplayMode == DisplayMode.PANASCOPE || Display.CurrentDisplayMode == DisplayMode.SPECTRASCOPE)
				{
					if (buf[i] < scope2_pixel_min)
					{
						scope2_pixel_min = buf[i];
					}
					if (buf[i] > scope2_pixel_max)
					{
						scope2_pixel_max = buf[i];
					}
				}
				else
				{
					scope2_pixel_min = buf[i];
					scope2_pixel_max = buf[i];
				}
				scope2_sample_index++;
				if (scope2_sample_index >= num)
				{
					scope2_sample_index = 0;
					scope2_min[scope2_pixel_index] = scope2_pixel_min;
					scope2_max[scope2_pixel_index] = scope2_pixel_max;
					scope2_pixel_min = float.MaxValue;
					scope2_pixel_max = float.MinValue;
					scope2_pixel_index++;
					if (scope2_pixel_index >= scope_array_len)
					{
						scope2_pixel_index = 0;
					}
				}
			}
		}
	}
}
