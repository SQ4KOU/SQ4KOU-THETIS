using System;

namespace Thetis;

public class RadioDSPTX
{
	private uint thread;

	private bool update;

	private bool force;

	private int buffer_size_dsp = 64;

	private int buffer_size = 64;

	private int filter_size_dsp = 4096;

	private int filter_size = 4096;

	private DSPFilterType filter_type_dsp = DSPFilterType.Low_Latency;

	private DSPFilterType filter_type = DSPFilterType.Low_Latency;

	private DSPMode current_dsp_mode_dsp = DSPMode.USB;

	private DSPMode current_dsp_mode = DSPMode.USB;

	private int sub_am_mode_dsp;

	private int sub_am_mode;

	private int tx_filter_low_dsp;

	private int tx_filter_low;

	private int tx_filter_high_dsp;

	private int tx_filter_high;

	private double tx_osc_dsp;

	private double tx_osc;

	private int tx_eq_num_bands = 3;

	private int[] tx_eq3_dsp = new int[4];

	private int[] tx_eq3 = new int[4];

	private int[] tx_eq10_dsp = new int[11];

	private int[] tx_eq10 = new int[11];

	private bool tx_eq_on_dsp;

	private bool tx_eq_on;

	private bool notch_160_dsp;

	private bool notch_160;

	private double tx_fm_deviation = 5000.0;

	private double tx_fm_deviation_dsp = 5000.0;

	private double ctcss_freq_hz = 100.0;

	private double ctcss_freq_hz_dsp = 100.0;

	private bool ctcss_flag;

	private bool ctcss_flag_dsp;

	private double tx_am_carrier_level_dsp = 0.4;

	private double tx_am_carrier_level = 0.4;

	private int tx_alc_decay_dsp = 10;

	private int tx_alc_decay = 10;

	private double tx_leveler_max_gain_dsp = 15.0;

	private double tx_leveler_max_gain = 15.0;

	private int tx_leveler_decay_dsp = 100;

	private int tx_leveler_decay = 100;

	private bool tx_leveler_on_dsp = true;

	private bool tx_leveler_on = true;

	private bool tx_compand_on_dsp;

	private bool tx_compand_on;

	private double tx_compand_level_dsp = 0.1;

	private double tx_compand_level = 0.1;

	private bool tx_osctrl_on_dsp;

	private bool tx_osctrl_on;

	private bool tx_fm_emph_on_dsp = true;

	private bool tx_fm_emph_on = true;

	private bool tx_eer_mode_run_dsp;

	private bool tx_eer_mode_run;

	private bool tx_eer_mode_am_iq_dsp = true;

	private bool tx_eer_mode_am_iq = true;

	private double tx_eer_mode_mgain_dsp = 0.5;

	private double tx_eer_mode_mgain = 0.5;

	private double tx_eer_mode_pgain_dsp = 0.5;

	private double tx_eer_mode_pgain = 0.5;

	private bool tx_eer_mode_rundelays_dsp = true;

	private bool tx_eer_mode_rundelays = true;

	private double tx_eer_mode_mdelay_dsp;

	private double tx_eer_mode_mdelay;

	private double tx_eer_mode_pdelay_dsp;

	private double tx_eer_mode_pdelay;

	private int tx_bandpass_window_dsp = 1;

	private int tx_bandpass_window = 1;

	private int tx_pregen_run_dsp;

	private int tx_pregen_run;

	private int tx_pregen_mode_dsp;

	private int tx_pregen_mode;

	private double tx_pregen_tone_mag_dsp;

	private double tx_pregen_tone_mag;

	private double tx_pregen_tone_freq_dsp;

	private double tx_pregen_tone_freq;

	private double tx_pregen_noise_mag_dsp;

	private double tx_pregen_noise_mag;

	private double tx_pregen_sweep_mag_dsp;

	private double tx_pregen_sweep_mag;

	private double tx_pregen_sweep_freq1_dsp;

	private double tx_pregen_sweep_freq1;

	private double tx_pregen_sweep_freq2_dsp;

	private double tx_pregen_sweep_freq2;

	private double tx_pregen_sweep_rate_dsp;

	private double tx_pregen_sweep_rate;

	private double tx_pregen_sawtooth_mag_dsp;

	private double tx_pregen_sawtooth_mag;

	private double tx_pregen_sawtooth_freq_dsp;

	private double tx_pregen_sawtooth_freq;

	private double tx_pregen_triangle_mag_dsp;

	private double tx_pregen_triangle_mag;

	private double tx_pregen_triangle_freq_dsp;

	private double tx_pregen_triangle_freq;

	private double tx_pregen_pulse_mag_dsp;

	private double tx_pregen_pulse_mag;

	private double tx_pregen_pulse_freq_dsp;

	private double tx_pregen_pulse_freq;

	private double tx_pregen_pulse_dutycycle_dsp;

	private double tx_pregen_pulse_dutycycle;

	private double tx_pregen_pulse_tonefreq_dsp;

	private double tx_pregen_pulse_tonefreq;

	private double tx_pregen_pulse_transition_dsp;

	private double tx_pregen_pulse_transition;

	private int tx_postgen_run_dsp;

	private int tx_postgen_run;

	private int tx_postgen_mode_dsp;

	private int tx_postgen_mode;

	private double tx_postgen_tone_mag_dsp;

	private double tx_postgen_tone_mag;

	private double tx_postgen_tone_freq_dsp;

	private double tx_postgen_tone_freq;

	private double tx_postgen_tt_mag1_dsp;

	private double tx_postgen_tt_mag1;

	private double tx_postgen_tt_mag2_dsp;

	private double tx_postgen_tt_mag2;

	private double tx_postgen_tt_freq1_dsp;

	private double tx_postgen_tt_freq1;

	private double tx_postgen_tt_freq2_dsp;

	private double tx_postgen_tt_freq2;

	private double tx_postgen_sweep_mag_dsp;

	private double tx_postgen_sweep_mag;

	private double tx_postgen_sweep_freq1_dsp;

	private double tx_postgen_sweep_freq1;

	private double tx_postgen_sweep_freq2_dsp;

	private double tx_postgen_sweep_freq2;

	private double tx_postgen_sweep_rate_dsp;

	private double tx_postgen_sweep_rate;

	private double tx_postgen_pulse_mag_dsp;

	private double tx_postgen_pulse_mag;

	private double tx_postgen_pulse_tonefreq_dsp;

	private double tx_postgen_pulse_tonefreq;

	private double tx_postgen_pulse_freq_dsp;

	private double tx_postgen_pulse_freq;

	private double tx_postgen_pulse_dutycycle_dsp;

	private double tx_postgen_pulse_dutycycle;

	private double tx_postgen_pulse_transition_dsp;

	private double tx_postgen_pulse_transition;

	private bool tx_postgen_pulse_iqout_dsp = true;

	private bool tx_postgen_pulse_iqout = true;

	private double tx_postgen_tt_pulse_mag1_dsp;

	private double tx_postgen_tt_pulse_mag1;

	private double tx_postgen_tt_pulse_mag2_dsp;

	private double tx_postgen_tt_pulse_mag2;

	private double tx_postgen_tt_pulse_tone_freq1_dsp;

	private double tx_postgen_tt_pulse_tone_freq1;

	private double tx_postgen_tt_pulse_tone_freq2_dsp;

	private double tx_postgen_tt_pulse_tone_freq2;

	private double tx_postgen_tt_pulse_freq_dsp;

	private double tx_postgen_tt_pulse_freq;

	private double tx_postgen_tt_pulse_dutycycle_dsp;

	private double tx_postgen_tt_pulse_dutycycle;

	private double tx_postgen_tt_pulse_transition_dsp;

	private double tx_postgen_tt_pulse_transition;

	private bool tx_postgen_tt_pulse_iqout_dsp = true;

	private bool tx_postgen_tt_pulse_iqout = true;

	private bool ps_run_cal_dsp;

	private bool ps_run_cal;

	private double mic_gain_dsp = 0.5;

	private double mic_gain = 0.5;

	private double tx_fm_lowcut_dsp = 300.0;

	private double tx_fm_lowcut = 300.0;

	private double tx_fm_highcut_dsp = 3000.0;

	private double tx_fm_highcut = 3000.0;

	public bool Update
	{
		get
		{
			return update;
		}
		set
		{
			update = value;
			if (value)
			{
				SyncAll();
			}
		}
	}

	public bool Force
	{
		get
		{
			return force;
		}
		set
		{
			force = value;
		}
	}

	public int BufferSize
	{
		get
		{
			return buffer_size;
		}
		set
		{
			buffer_size = value;
			if (update && (value != buffer_size_dsp || force))
			{
				WDSP.SetDSPBuffsize(WDSP.id(thread, 0u), value);
				Audio.console.specRX.GetSpecRX(cmaster.inid(1, 0)).BlockSize = value;
				Audio.console.specRX.GetSpecRX(cmaster.inid(1, 0)).SampleRate = 96000;
				buffer_size_dsp = value;
			}
		}
	}

	public int FilterSize
	{
		get
		{
			return filter_size;
		}
		set
		{
			filter_size = value;
			if (update && (value != filter_size_dsp || force))
			{
				WDSP.TXASetNC(WDSP.id(thread, 0u), value);
				filter_size_dsp = value;
			}
		}
	}

	public DSPFilterType FilterType
	{
		get
		{
			return filter_type;
		}
		set
		{
			filter_type = value;
			if (update && (value != filter_type_dsp || force))
			{
				WDSP.TXASetMP(WDSP.id(thread, 0u), Convert.ToBoolean(value));
				filter_type_dsp = value;
			}
		}
	}

	public DSPMode CurrentDSPMode
	{
		get
		{
			return current_dsp_mode;
		}
		set
		{
			current_dsp_mode = value;
			if (!update || (value == current_dsp_mode_dsp && !force))
			{
				return;
			}
			if (current_dsp_mode == DSPMode.AM || current_dsp_mode == DSPMode.SAM)
			{
				switch (sub_am_mode)
				{
				case 0:
					WDSP.SetTXAMode(WDSP.id(thread, 0u), DSPMode.AM);
					break;
				case 1:
					WDSP.SetTXAMode(WDSP.id(thread, 0u), DSPMode.AM_LSB);
					break;
				case 2:
					WDSP.SetTXAMode(WDSP.id(thread, 0u), DSPMode.AM_USB);
					break;
				}
			}
			else
			{
				WDSP.SetTXAMode(WDSP.id(thread, 0u), value);
			}
			current_dsp_mode_dsp = value;
		}
	}

	public int SubAMMode
	{
		get
		{
			return sub_am_mode;
		}
		set
		{
			sub_am_mode = value;
			if (!update || (value == sub_am_mode_dsp && !force))
			{
				return;
			}
			if (current_dsp_mode == DSPMode.AM || current_dsp_mode == DSPMode.SAM)
			{
				switch (sub_am_mode)
				{
				case 0:
					WDSP.SetTXAMode(WDSP.id(thread, 0u), DSPMode.AM);
					break;
				case 1:
					WDSP.SetTXAMode(WDSP.id(thread, 0u), DSPMode.AM_LSB);
					break;
				case 2:
					WDSP.SetTXAMode(WDSP.id(thread, 0u), DSPMode.AM_USB);
					break;
				}
			}
			sub_am_mode_dsp = value;
		}
	}

	public int TXFilterLow
	{
		get
		{
			return tx_filter_low;
		}
		set
		{
			tx_filter_low = value;
			if (update && (value != tx_filter_low_dsp || force))
			{
				WDSP.SetTXABandpassFreqs(WDSP.id(thread, 0u), value, tx_filter_high);
				tx_filter_low_dsp = value;
			}
		}
	}

	public int TXFilterHigh
	{
		get
		{
			return tx_filter_high;
		}
		set
		{
			tx_filter_high = value;
			if (update && (value != tx_filter_high_dsp || force))
			{
				WDSP.SetTXABandpassFreqs(WDSP.id(thread, 0u), tx_filter_low, value);
				tx_filter_high_dsp = value;
			}
		}
	}

	public double TXOsc
	{
		get
		{
			return tx_osc;
		}
		set
		{
			tx_osc = value;
			if (update && (value != tx_osc_dsp || force))
			{
				tx_osc_dsp = value;
			}
		}
	}

	public int TXEQNumBands
	{
		get
		{
			return tx_eq_num_bands;
		}
		set
		{
			tx_eq_num_bands = value;
		}
	}

	public int[] TXEQ3
	{
		get
		{
			return tx_eq3;
		}
		set
		{
			for (int i = 0; i < tx_eq3.Length && i < value.Length; i++)
			{
				tx_eq3[i] = value[i];
			}
			if (update)
			{
				for (int j = 0; j < tx_eq3_dsp.Length && j < value.Length; j++)
				{
					tx_eq3_dsp[j] = value[j];
				}
			}
		}
	}

	public int[] TXEQ10
	{
		get
		{
			return tx_eq10;
		}
		set
		{
			for (int i = 0; i < tx_eq10.Length && i < value.Length; i++)
			{
				tx_eq10[i] = value[i];
			}
			if (update)
			{
				for (int j = 0; j < tx_eq10_dsp.Length && j < value.Length; j++)
				{
					tx_eq10_dsp[j] = value[j];
				}
			}
		}
	}

	public bool TXEQOn
	{
		get
		{
			return tx_eq_on;
		}
		set
		{
			tx_eq_on = value;
			if (update && (value != tx_eq_on_dsp || force))
			{
				WDSP.SetTXAEQRun(WDSP.id(thread, 0u), value);
				tx_eq_on_dsp = value;
			}
		}
	}

	public bool Notch160
	{
		get
		{
			return notch_160;
		}
		set
		{
			notch_160 = value;
			if (update && (value != notch_160_dsp || force))
			{
				notch_160_dsp = value;
			}
		}
	}

	public double TXFMDeviation
	{
		get
		{
			return tx_fm_deviation;
		}
		set
		{
			tx_fm_deviation = value;
			if (update && (value != tx_fm_deviation_dsp || force))
			{
				WDSP.SetTXAFMDeviation(WDSP.id(thread, 0u), value);
				tx_fm_deviation_dsp = value;
			}
		}
	}

	public double CTCSSFreqHz
	{
		get
		{
			return ctcss_freq_hz;
		}
		set
		{
			ctcss_freq_hz = value;
			if (update && (value != ctcss_freq_hz_dsp || force))
			{
				WDSP.SetTXACTCSSFreq(WDSP.id(thread, 0u), value);
				WDSP.SetRXACTCSSFreq(WDSP.id(0u, 0u), value);
				WDSP.SetRXACTCSSFreq(WDSP.id(0u, 1u), value);
				WDSP.SetRXACTCSSFreq(WDSP.id(2u, 0u), value);
				ctcss_freq_hz_dsp = value;
			}
		}
	}

	public bool CTCSSFlag
	{
		get
		{
			return ctcss_flag;
		}
		set
		{
			ctcss_flag = value;
			if (update && (value != ctcss_flag_dsp || force))
			{
				WDSP.SetTXACTCSSRun(WDSP.id(thread, 0u), value);
				ctcss_flag_dsp = value;
			}
		}
	}

	public double TXAMCarrierLevel
	{
		get
		{
			return tx_am_carrier_level;
		}
		set
		{
			tx_am_carrier_level = value;
			if (update && (value != tx_am_carrier_level_dsp || force))
			{
				WDSP.SetTXAAMCarrierLevel(WDSP.id(thread, 0u), value);
				tx_am_carrier_level_dsp = value;
			}
		}
	}

	public int TXALCDecay
	{
		get
		{
			return tx_alc_decay;
		}
		set
		{
			tx_alc_decay = value;
			if (update && (value != tx_alc_decay_dsp || force))
			{
				WDSP.SetTXAALCDecay(WDSP.id(thread, 0u), value);
				tx_alc_decay_dsp = value;
			}
		}
	}

	public double TXLevelerMaxGain
	{
		get
		{
			return tx_leveler_max_gain;
		}
		set
		{
			tx_leveler_max_gain = value;
			if (update && (value != tx_leveler_max_gain_dsp || force))
			{
				WDSP.SetTXALevelerTop(WDSP.id(thread, 0u), value);
				tx_leveler_max_gain_dsp = value;
			}
		}
	}

	public int TXLevelerDecay
	{
		get
		{
			return tx_leveler_decay;
		}
		set
		{
			tx_leveler_decay = value;
			if (update && (value != tx_leveler_decay_dsp || force))
			{
				WDSP.SetTXALevelerDecay(WDSP.id(thread, 0u), value);
				tx_leveler_decay_dsp = value;
			}
		}
	}

	public bool TXLevelerOn
	{
		get
		{
			return tx_leveler_on;
		}
		set
		{
			tx_leveler_on = value;
			if (update && (value != tx_leveler_on_dsp || force))
			{
				WDSP.SetTXALevelerSt(WDSP.id(thread, 0u), value);
				tx_leveler_on_dsp = value;
			}
		}
	}

	public bool TXCompandOn
	{
		get
		{
			return tx_compand_on;
		}
		set
		{
			tx_compand_on = value;
			if (update && (value != tx_compand_on_dsp || force))
			{
				WDSP.SetTXACompressorRun(WDSP.id(thread, 0u), value);
				tx_compand_on_dsp = value;
			}
		}
	}

	public double TXCompandLevel
	{
		get
		{
			return tx_compand_level;
		}
		set
		{
			tx_compand_level = value;
			if (update && (value != tx_compand_level_dsp || force))
			{
				WDSP.SetTXACompressorGain(WDSP.id(thread, 0u), value);
				tx_compand_level_dsp = value;
			}
		}
	}

	public bool TXOsctrlOn
	{
		get
		{
			return tx_osctrl_on;
		}
		set
		{
			tx_osctrl_on = value;
			if (update && (value != tx_osctrl_on_dsp || force))
			{
				WDSP.SetTXAosctrlRun(WDSP.id(thread, 0u), value);
				tx_osctrl_on_dsp = value;
			}
		}
	}

	public bool TXFMEmphOn
	{
		get
		{
			return tx_fm_emph_on;
		}
		set
		{
			tx_fm_emph_on = value;
			if (update && (value != tx_fm_emph_on_dsp || force))
			{
				WDSP.SetTXAFMEmphPosition(WDSP.id(thread, 0u), value);
				tx_fm_emph_on_dsp = value;
			}
		}
	}

	public bool TXEERModeRun
	{
		get
		{
			return tx_eer_mode_run;
		}
		set
		{
			tx_eer_mode_run = value;
			if (update && (value != tx_eer_mode_run_dsp || force))
			{
				cmaster.CMSetEERRun(0);
				tx_eer_mode_run_dsp = value;
			}
		}
	}

	public bool TXEERModeAMIQ
	{
		get
		{
			return tx_eer_mode_am_iq;
		}
		set
		{
			tx_eer_mode_am_iq = value;
			if (update && (value != tx_eer_mode_am_iq_dsp || force))
			{
				cmaster.SetEERAMIQ(0, value);
				tx_eer_mode_am_iq_dsp = value;
			}
		}
	}

	public double TXEERModeMgain
	{
		get
		{
			return tx_eer_mode_mgain;
		}
		set
		{
			tx_eer_mode_mgain = value;
			if (update && (value != tx_eer_mode_mgain_dsp || force))
			{
				cmaster.SetEERMgain(0, value);
				tx_eer_mode_mgain_dsp = value;
			}
		}
	}

	public double TXEERModePgain
	{
		get
		{
			return tx_eer_mode_pgain;
		}
		set
		{
			tx_eer_mode_pgain = value;
			if (update && (value != tx_eer_mode_pgain_dsp || force))
			{
				cmaster.SetEERPgain(0, value);
				tx_eer_mode_pgain_dsp = value;
			}
		}
	}

	public bool TXEERModeRunDelays
	{
		get
		{
			return tx_eer_mode_rundelays;
		}
		set
		{
			tx_eer_mode_rundelays = value;
			if (update && (value != tx_eer_mode_rundelays_dsp || force))
			{
				cmaster.SetEERRunDelays(0, value);
				tx_eer_mode_rundelays_dsp = value;
			}
		}
	}

	public double TXEERModeMdelay
	{
		get
		{
			return tx_eer_mode_mdelay;
		}
		set
		{
			tx_eer_mode_mdelay = value;
			if (update && (value != tx_eer_mode_mdelay_dsp || force))
			{
				cmaster.SetEERMdelay(0, value);
				tx_eer_mode_mdelay_dsp = value;
			}
		}
	}

	public double TXEERModePdelay
	{
		get
		{
			return tx_eer_mode_pdelay;
		}
		set
		{
			tx_eer_mode_pdelay = value;
			if (update && (value != tx_eer_mode_pdelay_dsp || force))
			{
				cmaster.SetEERPdelay(0, value);
				tx_eer_mode_pdelay_dsp = value;
			}
		}
	}

	public int TXBandpassWindow
	{
		get
		{
			return tx_bandpass_window;
		}
		set
		{
			tx_bandpass_window = value;
			if (update && (value != tx_bandpass_window_dsp || force))
			{
				WDSP.SetTXABandpassWindow(WDSP.id(thread, 0u), value);
				tx_bandpass_window_dsp = value;
			}
		}
	}

	public int TXPreGenRun
	{
		get
		{
			return tx_pregen_run;
		}
		set
		{
			tx_pregen_run = value;
			if (update && (value != tx_pregen_run_dsp || force))
			{
				WDSP.SetTXAPreGenRun(WDSP.id(thread, 0u), value);
				tx_pregen_run_dsp = value;
			}
		}
	}

	public int TXPreGenMode
	{
		get
		{
			return tx_pregen_mode;
		}
		set
		{
			tx_pregen_mode = value;
			if (update && (value != tx_pregen_mode_dsp || force))
			{
				WDSP.SetTXAPreGenMode(WDSP.id(thread, 0u), value);
				tx_pregen_mode_dsp = value;
			}
		}
	}

	public double TXPreGenToneMag
	{
		get
		{
			return tx_pregen_tone_mag;
		}
		set
		{
			tx_pregen_tone_mag = value;
			if (update && (value != tx_pregen_tone_mag_dsp || force))
			{
				WDSP.SetTXAPreGenToneMag(WDSP.id(thread, 0u), value);
				tx_pregen_tone_mag_dsp = value;
			}
		}
	}

	public double TXPreGenToneFreq
	{
		get
		{
			return tx_pregen_tone_freq;
		}
		set
		{
			tx_pregen_tone_freq = value;
			if (update && (value != tx_pregen_tone_freq_dsp || force))
			{
				WDSP.SetTXAPreGenToneFreq(WDSP.id(thread, 0u), value);
				tx_pregen_tone_freq_dsp = value;
			}
		}
	}

	public double TXPreGenNoiseMag
	{
		get
		{
			return tx_pregen_noise_mag;
		}
		set
		{
			tx_pregen_noise_mag = value;
			if (update && (value != tx_pregen_noise_mag_dsp || force))
			{
				WDSP.SetTXAPreGenNoiseMag(WDSP.id(thread, 0u), value);
				tx_pregen_noise_mag_dsp = value;
			}
		}
	}

	public double TXPreGenSweepMag
	{
		get
		{
			return tx_pregen_sweep_mag;
		}
		set
		{
			tx_pregen_sweep_mag = value;
			if (update && (value != tx_pregen_sweep_mag_dsp || force))
			{
				WDSP.SetTXAPreGenSweepMag(WDSP.id(thread, 0u), value);
				tx_pregen_sweep_mag_dsp = value;
			}
		}
	}

	public double TXPreGenSweepFreq1
	{
		get
		{
			return tx_pregen_sweep_freq1;
		}
		set
		{
			tx_pregen_sweep_freq1 = value;
			if (update && (value != tx_pregen_sweep_freq1_dsp || force))
			{
				tx_pregen_sweep_freq1_dsp = value;
				WDSP.SetTXAPreGenSweepFreq(WDSP.id(thread, 0u), tx_pregen_sweep_freq1_dsp, tx_pregen_sweep_freq2_dsp);
			}
		}
	}

	public double TXPreGenSweepFreq2
	{
		get
		{
			return tx_pregen_sweep_freq2;
		}
		set
		{
			tx_pregen_sweep_freq2 = value;
			if (update && (value != tx_pregen_sweep_freq2_dsp || force))
			{
				tx_pregen_sweep_freq2_dsp = value;
				WDSP.SetTXAPreGenSweepFreq(WDSP.id(thread, 0u), tx_pregen_sweep_freq1_dsp, tx_pregen_sweep_freq2_dsp);
			}
		}
	}

	public double TXPreGenSweepRate
	{
		get
		{
			return tx_pregen_sweep_rate;
		}
		set
		{
			tx_pregen_sweep_rate = value;
			if (update && (value != tx_pregen_sweep_rate_dsp || force))
			{
				WDSP.SetTXAPreGenSweepRate(WDSP.id(thread, 0u), value);
				tx_pregen_sweep_rate_dsp = value;
			}
		}
	}

	public double TXPreGenSawtoothMag
	{
		get
		{
			return tx_pregen_sawtooth_mag;
		}
		set
		{
			tx_pregen_sawtooth_mag = value;
			if (update && (value != tx_pregen_sawtooth_mag_dsp || force))
			{
				WDSP.SetTXAPreGenSawtoothMag(WDSP.id(thread, 0u), value);
				tx_pregen_sawtooth_mag_dsp = value;
			}
		}
	}

	public double TXPreGenSawtoothFreq
	{
		get
		{
			return tx_pregen_sawtooth_freq;
		}
		set
		{
			tx_pregen_sawtooth_freq = value;
			if (update && (value != tx_pregen_sawtooth_freq_dsp || force))
			{
				WDSP.SetTXAPreGenSawtoothFreq(WDSP.id(thread, 0u), value);
				tx_pregen_sawtooth_freq_dsp = value;
			}
		}
	}

	public double TXPreGenTriangleMag
	{
		get
		{
			return tx_pregen_triangle_mag;
		}
		set
		{
			tx_pregen_triangle_mag = value;
			if (update && (value != tx_pregen_triangle_mag_dsp || force))
			{
				WDSP.SetTXAPreGenTriangleMag(WDSP.id(thread, 0u), value);
				tx_pregen_triangle_mag_dsp = value;
			}
		}
	}

	public double TXPreGenTriangleFreq
	{
		get
		{
			return tx_pregen_triangle_freq;
		}
		set
		{
			tx_pregen_triangle_freq = value;
			if (update && (value != tx_pregen_triangle_freq_dsp || force))
			{
				WDSP.SetTXAPreGenTriangleFreq(WDSP.id(thread, 0u), value);
				tx_pregen_triangle_freq_dsp = value;
			}
		}
	}

	public double TXPreGenPulseMag
	{
		get
		{
			return tx_pregen_pulse_mag;
		}
		set
		{
			tx_pregen_pulse_mag = value;
			if (update && (value != tx_pregen_pulse_mag_dsp || force))
			{
				WDSP.SetTXAPreGenPulseMag(WDSP.id(thread, 0u), value);
				tx_pregen_pulse_mag_dsp = value;
			}
		}
	}

	public double TXPreGenPulseFreq
	{
		get
		{
			return tx_pregen_pulse_freq;
		}
		set
		{
			tx_pregen_pulse_freq = value;
			if (update && (value != tx_pregen_pulse_freq_dsp || force))
			{
				WDSP.SetTXAPreGenPulseFreq(WDSP.id(thread, 0u), value);
				tx_pregen_pulse_freq_dsp = value;
			}
		}
	}

	public double TXPreGenPulseDutyCycle
	{
		get
		{
			return tx_pregen_pulse_dutycycle;
		}
		set
		{
			tx_pregen_pulse_dutycycle = value;
			if (update && (value != tx_pregen_pulse_dutycycle_dsp || force))
			{
				WDSP.SetTXAPreGenPulseDutyCycle(WDSP.id(thread, 0u), value);
				tx_pregen_pulse_dutycycle_dsp = value;
			}
		}
	}

	public double TXPreGenPulseToneFreq
	{
		get
		{
			return tx_pregen_pulse_tonefreq;
		}
		set
		{
			tx_pregen_pulse_tonefreq = value;
			if (update && (value != tx_pregen_pulse_tonefreq_dsp || force))
			{
				WDSP.SetTXAPreGenPulseToneFreq(WDSP.id(thread, 0u), value);
				tx_pregen_pulse_tonefreq_dsp = value;
			}
		}
	}

	public double TXPreGenPulseTransition
	{
		get
		{
			return tx_pregen_pulse_transition;
		}
		set
		{
			tx_pregen_pulse_transition = value;
			if (update && (value != tx_pregen_pulse_transition_dsp || force))
			{
				WDSP.SetTXAPreGenPulseTransition(WDSP.id(thread, 0u), value);
				tx_pregen_pulse_transition_dsp = value;
			}
		}
	}

	public int TXPostGenRun
	{
		get
		{
			return tx_postgen_run;
		}
		set
		{
			tx_postgen_run = value;
			if (update && (value != tx_postgen_run_dsp || force))
			{
				WDSP.SetTXAPostGenRun(WDSP.id(thread, 0u), value);
				tx_postgen_run_dsp = value;
			}
		}
	}

	public int TXPostGenMode
	{
		get
		{
			return tx_postgen_mode;
		}
		set
		{
			tx_postgen_mode = value;
			if (update && (value != tx_postgen_mode_dsp || force))
			{
				WDSP.SetTXAPostGenMode(WDSP.id(thread, 0u), value);
				tx_postgen_mode_dsp = value;
			}
		}
	}

	public double TXPostGenToneMag
	{
		get
		{
			return tx_postgen_tone_mag;
		}
		set
		{
			tx_postgen_tone_mag = value;
			if (update && (value != tx_postgen_tone_mag_dsp || force))
			{
				WDSP.SetTXAPostGenToneMag(WDSP.id(thread, 0u), value);
				tx_postgen_tone_mag_dsp = value;
			}
		}
	}

	public double TXPostGenToneFreq
	{
		get
		{
			return tx_postgen_tone_freq;
		}
		set
		{
			tx_postgen_tone_freq = value;
			if (update && (value != tx_postgen_tone_freq_dsp || force))
			{
				WDSP.SetTXAPostGenToneFreq(WDSP.id(thread, 0u), value);
				tx_postgen_tone_freq_dsp = value;
			}
		}
	}

	public double TXPostGenTTMag1
	{
		get
		{
			return tx_postgen_tt_mag1;
		}
		set
		{
			tx_postgen_tt_mag1 = value;
			if (update && (value != tx_postgen_tt_mag1_dsp || force))
			{
				tx_postgen_tt_mag1_dsp = value;
				WDSP.SetTXAPostGenTTMag(WDSP.id(thread, 0u), tx_postgen_tt_mag1_dsp, tx_postgen_tt_mag2_dsp);
			}
		}
	}

	public double TXPostGenTTMag2
	{
		get
		{
			return tx_postgen_tt_mag2;
		}
		set
		{
			tx_postgen_tt_mag2 = value;
			if (update && (value != tx_postgen_tt_mag2_dsp || force))
			{
				tx_postgen_tt_mag2_dsp = value;
				WDSP.SetTXAPostGenTTMag(WDSP.id(thread, 0u), tx_postgen_tt_mag1_dsp, tx_postgen_tt_mag2_dsp);
			}
		}
	}

	public double TXPostGenTTFreq1
	{
		get
		{
			return tx_postgen_tt_freq1;
		}
		set
		{
			tx_postgen_tt_freq1 = value;
			if (update && (value != tx_postgen_tt_freq1_dsp || force))
			{
				tx_postgen_tt_freq1_dsp = value;
				WDSP.SetTXAPostGenTTFreq(WDSP.id(thread, 0u), tx_postgen_tt_freq1_dsp, tx_postgen_tt_freq2_dsp);
			}
		}
	}

	public double TXPostGenTTFreq2
	{
		get
		{
			return tx_postgen_tt_freq2;
		}
		set
		{
			tx_postgen_tt_freq2 = value;
			if (update && (value != tx_postgen_tt_freq2_dsp || force))
			{
				tx_postgen_tt_freq2_dsp = value;
				WDSP.SetTXAPostGenTTFreq(WDSP.id(thread, 0u), tx_postgen_tt_freq1_dsp, tx_postgen_tt_freq2_dsp);
			}
		}
	}

	public double TXPostGenSweepMag
	{
		get
		{
			return tx_postgen_sweep_mag;
		}
		set
		{
			tx_postgen_sweep_mag = value;
			if (update && (value != tx_postgen_sweep_mag_dsp || force))
			{
				WDSP.SetTXAPostGenSweepMag(WDSP.id(thread, 0u), value);
				tx_postgen_sweep_mag_dsp = value;
			}
		}
	}

	public double TXPostGenSweepFreq1
	{
		get
		{
			return tx_postgen_sweep_freq1;
		}
		set
		{
			tx_postgen_sweep_freq1 = value;
			if (update && (value != tx_postgen_sweep_freq1_dsp || force))
			{
				tx_postgen_sweep_freq1_dsp = value;
				WDSP.SetTXAPostGenSweepFreq(WDSP.id(thread, 0u), tx_postgen_sweep_freq1_dsp, tx_postgen_sweep_freq2_dsp);
			}
		}
	}

	public double TXPostGenSweepFreq2
	{
		get
		{
			return tx_postgen_sweep_freq2;
		}
		set
		{
			tx_postgen_sweep_freq2 = value;
			if (update && (value != tx_postgen_sweep_freq2_dsp || force))
			{
				tx_postgen_sweep_freq2_dsp = value;
				WDSP.SetTXAPostGenSweepFreq(WDSP.id(thread, 0u), tx_postgen_sweep_freq1_dsp, tx_postgen_sweep_freq2_dsp);
			}
		}
	}

	public double TXPostGenSweepRate
	{
		get
		{
			return tx_postgen_sweep_rate;
		}
		set
		{
			tx_postgen_sweep_rate = value;
			if (update && (value != tx_postgen_sweep_rate_dsp || force))
			{
				WDSP.SetTXAPostGenSweepRate(WDSP.id(thread, 0u), value);
				tx_postgen_sweep_rate_dsp = value;
			}
		}
	}

	public double TXPostGenPulseMag
	{
		get
		{
			return tx_postgen_pulse_mag;
		}
		set
		{
			tx_postgen_pulse_mag = value;
			if (update && (value != tx_postgen_pulse_mag_dsp || force))
			{
				WDSP.SetTXAPostGenPulseMag(WDSP.id(thread, 0u), value);
				tx_postgen_pulse_mag_dsp = value;
			}
		}
	}

	public double TXPostGenPulseToneFreq
	{
		get
		{
			return tx_postgen_pulse_tonefreq;
		}
		set
		{
			tx_postgen_pulse_tonefreq = value;
			if (update && (value != tx_postgen_pulse_tonefreq_dsp || force))
			{
				WDSP.SetTXAPostGenPulseToneFreq(WDSP.id(thread, 0u), value);
				tx_postgen_pulse_tonefreq_dsp = value;
			}
		}
	}

	public double TXPostGenPulseFreq
	{
		get
		{
			return tx_postgen_pulse_freq;
		}
		set
		{
			tx_postgen_pulse_freq = value;
			if (update && (value != tx_postgen_pulse_freq_dsp || force))
			{
				WDSP.SetTXAPostGenPulseFreq(WDSP.id(thread, 0u), value);
				tx_postgen_pulse_freq_dsp = value;
			}
		}
	}

	public double TXPostGenPulseDutyCycle
	{
		get
		{
			return tx_postgen_pulse_dutycycle;
		}
		set
		{
			tx_postgen_pulse_dutycycle = value;
			if (update && (value != tx_postgen_pulse_dutycycle_dsp || force))
			{
				WDSP.SetTXAPostGenPulseDutyCycle(WDSP.id(thread, 0u), value);
				tx_postgen_pulse_dutycycle_dsp = value;
			}
		}
	}

	public double TXPostGenPulseTransition
	{
		get
		{
			return tx_postgen_pulse_transition;
		}
		set
		{
			tx_postgen_pulse_transition = value;
			if (update && (value != tx_postgen_pulse_transition_dsp || force))
			{
				WDSP.SetTXAPostGenPulseTransition(WDSP.id(thread, 0u), value);
				tx_postgen_pulse_transition_dsp = value;
			}
		}
	}

	public bool TXPostGenPulseIQOut
	{
		get
		{
			return tx_postgen_pulse_iqout;
		}
		set
		{
			tx_postgen_pulse_iqout = value;
			if (update && (value != tx_postgen_pulse_iqout_dsp || force))
			{
				WDSP.SetTXAPostGenPulseIQout(WDSP.id(thread, 0u), value ? 1 : 0);
				tx_postgen_pulse_iqout_dsp = value;
			}
		}
	}

	public double TXPostGenTTPulseMag1
	{
		get
		{
			return tx_postgen_tt_pulse_mag1;
		}
		set
		{
			tx_postgen_tt_pulse_mag1 = value;
			if (update && (value != tx_postgen_tt_pulse_mag1_dsp || force))
			{
				WDSP.SetTXAPostGenTTPulseMag(WDSP.id(thread, 0u), value, tx_postgen_tt_pulse_mag2_dsp);
				tx_postgen_tt_pulse_mag1_dsp = value;
			}
		}
	}

	public double TXPostGenTTPulseMag2
	{
		get
		{
			return tx_postgen_tt_pulse_mag2;
		}
		set
		{
			tx_postgen_tt_pulse_mag2 = value;
			if (update && (value != tx_postgen_tt_pulse_mag2_dsp || force))
			{
				WDSP.SetTXAPostGenTTPulseMag(WDSP.id(thread, 0u), tx_postgen_tt_pulse_mag1_dsp, value);
				tx_postgen_tt_pulse_mag2_dsp = value;
			}
		}
	}

	public double TXPostGenTTPulseToneFreq1
	{
		get
		{
			return tx_postgen_tt_pulse_tone_freq1;
		}
		set
		{
			tx_postgen_tt_pulse_tone_freq1 = value;
			if (update && (value != tx_postgen_tt_pulse_tone_freq1_dsp || force))
			{
				WDSP.SetTXAPostGenTTPulseToneFreq(WDSP.id(thread, 0u), value, tx_postgen_tt_pulse_tone_freq2_dsp);
				tx_postgen_tt_pulse_tone_freq1_dsp = value;
			}
		}
	}

	public double TXPostGenTTPulseToneFreq2
	{
		get
		{
			return tx_postgen_tt_pulse_tone_freq2;
		}
		set
		{
			tx_postgen_tt_pulse_tone_freq2 = value;
			if (update && (value != tx_postgen_tt_pulse_tone_freq2_dsp || force))
			{
				WDSP.SetTXAPostGenTTPulseToneFreq(WDSP.id(thread, 0u), tx_postgen_tt_pulse_tone_freq1_dsp, value);
				tx_postgen_tt_pulse_tone_freq2_dsp = value;
			}
		}
	}

	public double TXPostGenTTPulseFreq
	{
		get
		{
			return tx_postgen_tt_pulse_freq;
		}
		set
		{
			tx_postgen_tt_pulse_freq = value;
			if (update && (value != tx_postgen_tt_pulse_freq_dsp || force))
			{
				WDSP.SetTXAPostGenTTPulseFreq(WDSP.id(thread, 0u), value);
				tx_postgen_tt_pulse_freq_dsp = value;
			}
		}
	}

	public double TXPostGenTTPulseDutyCycle
	{
		get
		{
			return tx_postgen_tt_pulse_dutycycle;
		}
		set
		{
			tx_postgen_tt_pulse_dutycycle = value;
			if (update && (value != tx_postgen_tt_pulse_dutycycle_dsp || force))
			{
				WDSP.SetTXAPostGenTTPulseDutyCycle(WDSP.id(thread, 0u), value);
				tx_postgen_tt_pulse_dutycycle_dsp = value;
			}
		}
	}

	public double TXPostGenTTPulseTransition
	{
		get
		{
			return tx_postgen_tt_pulse_transition;
		}
		set
		{
			tx_postgen_tt_pulse_transition = value;
			if (update && (value != tx_postgen_tt_pulse_transition_dsp || force))
			{
				WDSP.SetTXAPostGenTTPulseTransition(WDSP.id(thread, 0u), value);
				tx_postgen_tt_pulse_transition_dsp = value;
			}
		}
	}

	public bool TXPostGenTTPulseIQOut
	{
		get
		{
			return tx_postgen_tt_pulse_iqout;
		}
		set
		{
			tx_postgen_tt_pulse_iqout = value;
			if (update && (value != tx_postgen_tt_pulse_iqout_dsp || force))
			{
				WDSP.SetTXAPostGenTTPulseIQout(WDSP.id(thread, 0u), value ? 1 : 0);
				tx_postgen_tt_pulse_iqout_dsp = value;
			}
		}
	}

	public bool PSRunCal
	{
		get
		{
			return ps_run_cal;
		}
		set
		{
			ps_run_cal = value;
			if (update && (value != ps_run_cal_dsp || force))
			{
				puresignal.SetPSRunCal(WDSP.id(thread, 0u), value);
				ps_run_cal_dsp = value;
			}
		}
	}

	public double MicGain
	{
		get
		{
			return mic_gain;
		}
		set
		{
			mic_gain = value;
			if (update && (value != mic_gain_dsp || force))
			{
				WDSP.SetTXAPanelGain1(WDSP.id(thread, 0u), value);
				mic_gain_dsp = value;
			}
		}
	}

	public double TXFMLowCut
	{
		get
		{
			return tx_fm_lowcut;
		}
		set
		{
			tx_fm_lowcut = value;
			if (update && (value != tx_fm_lowcut_dsp || force))
			{
				WDSP.SetTXAFMAFFilter(WDSP.id(thread, 0u), value, tx_fm_highcut_dsp);
				tx_fm_lowcut_dsp = value;
			}
		}
	}

	public double TXFMHighCut
	{
		get
		{
			return tx_fm_highcut;
		}
		set
		{
			tx_fm_highcut = value;
			if (update && (value != tx_fm_highcut_dsp || force))
			{
				WDSP.SetTXAFMAFFilter(WDSP.id(thread, 0u), tx_fm_lowcut_dsp, value);
				tx_fm_highcut_dsp = value;
			}
		}
	}

	public RadioDSPTX(uint t)
	{
		thread = t;
	}

	private void SyncAll()
	{
		CurrentDSPMode = current_dsp_mode;
		SubAMMode = sub_am_mode;
		SetTXFilter(tx_filter_low, tx_filter_high);
		FilterSize = filter_size;
		FilterType = filter_type;
		TXOsc = tx_osc;
		if (tx_eq_num_bands == 3)
		{
			TXEQ10 = tx_eq10;
			TXEQ3 = tx_eq3;
		}
		else
		{
			TXEQ3 = tx_eq3;
			TXEQ10 = tx_eq10;
		}
		TXEQOn = tx_eq_on;
		Notch160 = notch_160;
		TXAMCarrierLevel = tx_am_carrier_level;
		TXALCDecay = tx_alc_decay;
		TXLevelerMaxGain = tx_leveler_max_gain;
		TXLevelerDecay = tx_leveler_decay;
		TXLevelerOn = tx_leveler_on;
		TXCompandOn = tx_compand_on;
		TXCompandLevel = tx_compand_level;
		TXOsctrlOn = tx_osctrl_on;
		CTCSSFreqHz = ctcss_freq_hz;
		TXFMDeviation = tx_fm_deviation;
		CTCSSFlag = ctcss_flag;
		TXFMEmphOn = tx_fm_emph_on;
		TXEERModeRun = tx_eer_mode_run;
		TXEERModeAMIQ = tx_eer_mode_am_iq;
		TXEERModeMgain = tx_eer_mode_mgain;
		TXEERModePgain = tx_eer_mode_pgain;
		TXEERModeRunDelays = tx_eer_mode_rundelays;
		TXEERModeMdelay = tx_eer_mode_mdelay;
		TXEERModePdelay = tx_eer_mode_pdelay;
		TXBandpassWindow = tx_bandpass_window;
		TXPreGenRun = tx_pregen_run;
		TXPreGenMode = tx_pregen_mode;
		TXPreGenToneMag = tx_pregen_tone_mag;
		TXPreGenToneFreq = tx_pregen_tone_freq;
		TXPreGenNoiseMag = tx_pregen_noise_mag;
		TXPreGenSweepMag = tx_pregen_sweep_mag;
		TXPreGenSweepFreq1 = tx_pregen_sweep_freq1;
		TXPreGenSweepFreq2 = tx_pregen_sweep_freq2;
		TXPreGenSweepRate = tx_pregen_sweep_rate;
		TXPreGenSawtoothMag = tx_pregen_sawtooth_mag;
		TXPreGenSawtoothFreq = tx_pregen_sawtooth_freq;
		TXPreGenTriangleMag = tx_pregen_triangle_mag;
		TXPreGenTriangleFreq = tx_pregen_triangle_freq;
		TXPreGenPulseMag = tx_pregen_pulse_mag;
		TXPreGenPulseFreq = tx_pregen_pulse_freq;
		TXPreGenPulseDutyCycle = tx_pregen_pulse_dutycycle;
		TXPreGenPulseToneFreq = tx_pregen_pulse_tonefreq;
		TXPreGenPulseTransition = tx_pregen_pulse_transition;
		TXPostGenRun = tx_postgen_run;
		TXPostGenMode = tx_postgen_mode;
		TXPostGenToneMag = tx_postgen_tone_mag;
		TXPostGenToneFreq = tx_postgen_tone_freq;
		TXPostGenTTMag1 = tx_postgen_tt_mag1;
		TXPostGenTTMag2 = tx_postgen_tt_mag2;
		TXPostGenTTFreq1 = tx_postgen_tt_freq1;
		TXPostGenTTFreq2 = tx_postgen_tt_freq2;
		TXPostGenSweepMag = tx_postgen_sweep_mag;
		TXPostGenSweepFreq1 = tx_postgen_sweep_freq1;
		TXPostGenSweepFreq2 = tx_postgen_sweep_freq2;
		TXPostGenSweepRate = tx_postgen_sweep_rate;
		PSRunCal = ps_run_cal;
		MicGain = mic_gain;
		TXFilterLow = tx_filter_low;
		TXFilterHigh = tx_filter_high;
		TXFMLowCut = tx_fm_lowcut;
		TXFMHighCut = tx_fm_highcut;
		TXPostGenPulseIQOut = tx_postgen_pulse_iqout;
		TXPostGenPulseToneFreq = tx_postgen_pulse_tonefreq;
		TXPostGenPulseMag = tx_postgen_pulse_mag;
		TXPostGenPulseFreq = tx_postgen_pulse_freq;
		TXPostGenPulseDutyCycle = tx_postgen_pulse_dutycycle;
		TXPostGenPulseTransition = tx_postgen_pulse_transition;
		TXPostGenTTPulseIQOut = tx_postgen_tt_pulse_iqout;
		TXPostGenTTPulseToneFreq1 = tx_postgen_tt_pulse_tone_freq1;
		TXPostGenTTPulseToneFreq2 = tx_postgen_tt_pulse_tone_freq2;
		TXPostGenTTPulseMag1 = tx_postgen_tt_pulse_mag1;
		TXPostGenTTPulseMag2 = tx_postgen_tt_pulse_mag2;
		TXPostGenTTPulseFreq = tx_postgen_tt_pulse_freq;
		TXPostGenTTPulseDutyCycle = tx_postgen_tt_pulse_dutycycle;
		TXPostGenTTPulseTransition = tx_postgen_tt_pulse_transition;
	}

	public void SetTXFilter(int low, int high)
	{
		tx_filter_low = low;
		tx_filter_high = high;
		if (update && (low != tx_filter_low_dsp || high != tx_filter_high_dsp || force))
		{
			WDSP.SetTXABandpassFreqs(WDSP.id(thread, 0u), low, high);
			tx_filter_low_dsp = low;
			tx_filter_high_dsp = high;
		}
	}
}
