using System;

namespace Thetis;

public class RadioDSPRX
{
	private uint thread;

	private uint subrx;

	private bool update;

	private bool force;

	private int buffer_size_dsp = 64;

	private int buffer_size = 64;

	private int filter_size_dsp = 2048;

	private int filter_size = 2048;

	private DSPFilterType filter_type_dsp = DSPFilterType.Low_Latency;

	private DSPFilterType filter_type = DSPFilterType.Low_Latency;

	private DSPMode dsp_mode_dsp = DSPMode.USB;

	private DSPMode dsp_mode = DSPMode.USB;

	private int rx_filter_low_dsp;

	private int rx_filter_low;

	private int rx_filter_high_dsp;

	private int rx_filter_high;

	private int noise_reduction_dsp;

	private int noise_reduction;

	private int nr_taps_dsp = 64;

	private int nr_taps = 64;

	private int nr_delay_dsp = 16;

	private int nr_delay = 16;

	private double nr_gain_dsp = 0.0016;

	private double nr_gain = 0.0016;

	private double nr_leak_dsp = 1E-06;

	private double nr_leak = 1E-06;

	private bool auto_notch_filter_dsp;

	private bool auto_notch_filter;

	private int anf_taps_dsp = 64;

	private int anf_taps = 64;

	private int anf_delay_dsp = 16;

	private int anf_delay = 16;

	private double anf_gain_dsp = 0.001;

	private double anf_gain = 0.001;

	private double anf_leak_dsp = 1E-07;

	private double anf_leak = 1E-07;

	private AGCMode rx_agc_mode_dsp = AGCMode.MED;

	private AGCMode rx_agc_mode = AGCMode.MED;

	private int rx_eq_num_bands = 10;

	private bool _legacy_eq = true;

	private int[] rx_eq3_dsp = new int[4];

	private int[] rx_eq3 = new int[4];

	private int[] rx_eq10_dsp = new int[11];

	private int[] rx_eq10 = new int[11];

	private bool rx_eq_on_dsp;

	private bool rx_eq_on;

	private double nb_threshold_dsp = 3.3;

	private double nb_threshold = 3.3;

	private double nb_tau_dsp = 5E-05;

	private double nb_tau = 5E-05;

	private double nb_advtime_dsp = 5E-05;

	private double nb_advtime = 5E-05;

	private double nb_hangtime_dsp = 5E-05;

	private double nb_hangtime = 5E-05;

	private int nb_mode_dsp;

	private int nb_mode;

	private double rx_fixed_agc_dsp = 20.0;

	private double rx_fixed_agc = 20.0;

	private double rx_agc_max_gain_dsp = 90.0;

	private double rx_agc_max_gain = 90.0;

	private int rx_agc_decay_dsp = 250;

	private int rx_agc_decay = 250;

	private int rx_agc_hang_dsp = 250;

	private int rx_agc_hang = 250;

	private double rx_output_gain_dsp = 1.0;

	private double rx_output_gain = 1.0;

	private int rx_agc_slope_dsp;

	private int rx_agc_slope;

	private int rx_agc_hang_threshold_dsp;

	private int rx_agc_hang_threshold;

	private bool bin_on_dsp;

	private bool bin_on;

	private float rx_squelch_threshold_dsp = -150f;

	private float rx_squelch_threshold = -150f;

	private bool _bSSqlOn;

	private bool _bSSqlOn_dsp;

	private float _fSSqlThreshold = 0.16f;

	private float _fSSqlThreshold_dsp = 0.16f;

	private float _fSSqlMuteTimeConstant = 0.1f;

	private float _fSSqlMuteTimeConstant_dsp = 0.1f;

	private float _fSSqlUnMuteTimeConstant = 0.1f;

	private float _fSSqlUnMuteTimeConstant_dsp = 0.1f;

	private float fm_squelch_threshold = 1f;

	private float fm_squelch_threshold_dsp = 1f;

	private bool rx_am_squelch_on_dsp;

	private bool rx_am_squelch_on;

	private bool rx_fm_squelch_on_dsp;

	private bool rx_fm_squelch_on;

	private double rx_am_squelch_max_tail_dsp = 1.5;

	private double rx_am_squelch_max_tail = 1.5;

	private bool spectrum_pre_filter_dsp = true;

	private bool spectrum_pre_filter = true;

	private bool active_dsp;

	private bool active;

	private float pan_dsp = 0.5f;

	private float pan = 0.5f;

	private double rx_osc_dsp;

	private double rx_osc;

	private double rx_fm_deviation = 5000.0;

	private double rx_fm_deviation_dsp = 5000.0;

	private bool[] notch_on = new bool[9];

	private bool[] notch_on_dsp = new bool[9];

	private double[] notch_freq = new double[9];

	private double[] notch_freq_dsp = new double[9];

	private double[] notch_bw = new double[9];

	private double[] notch_bw_dsp = new double[9];

	private bool rx_fm_ctcss_filter_dsp = true;

	private bool rx_fm_ctcss_filter = true;

	private bool rx_fm_detector_limiter_dsp;

	private bool rx_fm_detector_limiter;

	private double rx_fm_limiter_gain_dsp = 10.0;

	private double rx_fm_limiter_gain = 10.0;

	private double rx_fm_lowcut_dsp = 300.0;

	private double rx_fm_lowcut = 300.0;

	private double rx_fm_highcut_dsp = 3000.0;

	private double rx_fm_highcut = 3000.0;

	private int rx_anf_position_dsp = 1;

	private int rx_anf_position = 1;

	private int rx_anr_position_dsp = 1;

	private int rx_anr_position = 1;

	private bool rx_cbl_run_dsp = true;

	private bool rx_cbl_run = true;

	private int rx_cbl_position_dsp = 1;

	private int rx_cbl_position = 1;

	private int rx_amd_fadelevel_dsp = 1;

	private int rx_amd_fadelevel = 1;

	private int rx_amd_sbmode_dsp;

	private int rx_amd_sbmode;

	private int rx_bandpass_window_dsp;

	private int rx_bandpass_window;

	private int rx_pregen_run_dsp;

	private int rx_pregen_run;

	private int rx_pregen_mode_dsp;

	private int rx_pregen_mode;

	private double rx_pregen_tone_mag_dsp;

	private double rx_pregen_tone_mag;

	private double rx_pregen_tone_freq_dsp;

	private double rx_pregen_tone_freq;

	private double rx_pregen_noise_mag_dsp;

	private double rx_pregen_noise_mag;

	private double rx_pregen_sweep_mag_dsp;

	private double rx_pregen_sweep_mag;

	private double rx_pregen_sweep_freq1_dsp;

	private double rx_pregen_sweep_freq1;

	private double rx_pregen_sweep_freq2_dsp;

	private double rx_pregen_sweep_freq2;

	private double rx_pregen_sweep_rate_dsp;

	private double rx_pregen_sweep_rate;

	private bool rx_apf_run_dsp;

	private bool rx_apf_run;

	private double rx_apf_freq_dsp = 600.0;

	private double rx_apf_freq = 600.0;

	private double rx_apf_bw_dsp = 100.0;

	private double rx_apf_bw = 100.0;

	private double rx_apf_gain_dsp = 2.0;

	private double rx_apf_gain = 2.0;

	private int _rx_apf_type_dsp = 3;

	private int _rx_apf_type = 3;

	private bool rx_dolly_run_dsp;

	private bool rx_dolly_run;

	private double rx_dolly_freq0_dsp = 2125.0;

	private double rx_dolly_freq0 = 2125.0;

	private double rx_dolly_freq1_dsp = 2295.0;

	private double rx_dolly_freq1 = 2295.0;

	private int rx_nr2_gain_method = 2;

	private int rx_nr2_gain_method_dsp = 2;

	private int rx_nr2_npe_method;

	private int rx_nr2_npe_method_dsp;

	private int rx_nr2_ae_run = 1;

	private int rx_nr2_ae_run_dsp = 1;

	private int rx_nr2_ae_post2_run;

	private int rx_nr2_ae_post2_run_dsp;

	private double rx_nr2_ae_post2_nlevel = 15.0;

	private double rx_nr2_ae_post2_nlevel_dsp = 15.0;

	private double rx_nr2_ae_post2_factor = 15.0;

	private double rx_nr2_ae_post2_factor_dsp = 15.0;

	private double rx_nr2_ae_post2_rate = 5.0;

	private double rx_nr2_ae_post2_rate_dsp = 5.0;

	private int rx_nr2_ae_post2_taper = 12;

	private int rx_nr2_ae_post2_taper_dsp = 12;

	private int rx_nr2_run;

	private int rx_nr2_run_dsp;

	private int rx_nr2_position = 1;

	private int rx_nr2_position_dsp = 1;

	private int rx_nr3_run;

	private int rx_nr3_run_dsp;

	private int rx_nr3_position = 1;

	private int rx_nr3_position_dsp = 1;

	private int rx_nr3_fixed_gain = 1;

	private int rx_nr3_fixed_gain_dsp = 1;

	private int rx_nr4_run;

	private int rx_nr4_run_dsp;

	private int rx_nr4_position = 1;

	private int rx_nr4_position_dsp = 1;

	private float rx_nr4_reductionAmount = 10f;

	private float rx_nr4_reductionAmount_dsp = 10f;

	private float rx_nr4_smoothingFactor;

	private float rx_nr4_smoothingFactor_dsp;

	private float rx_nr4_whiteningFactor;

	private float rx_nr4_whiteningFactor_dsp;

	private float rx_nr4_noiseRescale = 2f;

	private float rx_nr4_noiseRescale_dsp = 2f;

	private float rx_nr4_postFilterThreshold;

	private float rx_nr4_postFilterThreshold_dsp;

	private int rx_nr4_noiseScalingType;

	private int rx_nr4_noiseScalingType_dsp;

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
				WDSP.SetDSPBuffsize(WDSP.id(thread, subrx), value);
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
				WDSP.RXASetNC(WDSP.id(thread, subrx), value);
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
				WDSP.RXASetMP(WDSP.id(thread, subrx), Convert.ToBoolean(value));
				filter_type_dsp = value;
			}
		}
	}

	public DSPMode DSPMode
	{
		get
		{
			return dsp_mode;
		}
		set
		{
			dsp_mode = value;
			if (update && (value != dsp_mode_dsp || force))
			{
				WDSP.SetRXAMode(WDSP.id(thread, subrx), value);
				dsp_mode_dsp = value;
			}
		}
	}

	public int RXFilterLow
	{
		get
		{
			return rx_filter_low;
		}
		set
		{
			rx_filter_low = value;
			if (update && (value != rx_filter_low_dsp || force))
			{
				WDSP.RXANBPSetFreqs(WDSP.id(thread, subrx), value, rx_filter_high);
				WDSP.SetRXABandpassFreqs(WDSP.id(thread, subrx), value, rx_filter_high);
				WDSP.SetRXASNBAOutputBandwidth(WDSP.id(thread, subrx), value, rx_filter_high);
				rx_filter_low_dsp = value;
			}
		}
	}

	public int RXFilterHigh
	{
		get
		{
			return rx_filter_high;
		}
		set
		{
			rx_filter_high = value;
			if (update && (value != rx_filter_high_dsp || force))
			{
				WDSP.RXANBPSetFreqs(WDSP.id(thread, subrx), rx_filter_low, value);
				WDSP.SetRXABandpassFreqs(WDSP.id(thread, subrx), rx_filter_low, value);
				WDSP.SetRXASNBAOutputBandwidth(WDSP.id(thread, subrx), rx_filter_low, value);
				rx_filter_high_dsp = value;
			}
		}
	}

	public int RXANR1Run
	{
		get
		{
			return noise_reduction;
		}
		set
		{
			noise_reduction = value;
			if (update && (value != noise_reduction_dsp || force))
			{
				WDSP.SetRXAANRRun(WDSP.id(thread, subrx), value);
				noise_reduction_dsp = value;
			}
		}
	}

	public bool AutoNotchFilter
	{
		get
		{
			return auto_notch_filter;
		}
		set
		{
			auto_notch_filter = value;
			if (update && (value != auto_notch_filter_dsp || force))
			{
				WDSP.SetRXAANFRun(WDSP.id(thread, subrx), value);
				auto_notch_filter_dsp = value;
			}
		}
	}

	public AGCMode RXAGCMode
	{
		get
		{
			return rx_agc_mode;
		}
		set
		{
			rx_agc_mode = value;
			if (update && (value != rx_agc_mode_dsp || force))
			{
				WDSP.SetRXAAGCMode(WDSP.id(thread, subrx), value);
				rx_agc_mode_dsp = value;
			}
		}
	}

	public int RXEQNumBands
	{
		get
		{
			return rx_eq_num_bands;
		}
		set
		{
			rx_eq_num_bands = value;
		}
	}

	public bool LegacyEQ
	{
		get
		{
			return _legacy_eq;
		}
		set
		{
			_legacy_eq = value;
		}
	}

	public unsafe int[] RXEQ3
	{
		get
		{
			return rx_eq3;
		}
		set
		{
			for (int i = 0; i < rx_eq3.Length && i < value.Length; i++)
			{
				rx_eq3[i] = value[i];
			}
			if (update)
			{
				fixed (int* ptr = &rx_eq3[0])
				{
					WDSP.SetRXAGrphEQ(WDSP.id(thread, subrx), ptr);
				}
				for (int j = 0; j < rx_eq3_dsp.Length && j < value.Length; j++)
				{
					rx_eq3_dsp[j] = value[j];
				}
			}
		}
	}

	public unsafe int[] RXEQ10
	{
		get
		{
			return rx_eq10;
		}
		set
		{
			for (int i = 0; i < rx_eq10.Length && i < value.Length; i++)
			{
				rx_eq10[i] = value[i];
			}
			if (update)
			{
				fixed (int* ptr = &rx_eq10[0])
				{
					WDSP.SetRXAGrphEQ10(WDSP.id(thread, subrx), ptr);
				}
				for (int j = 0; j < rx_eq10_dsp.Length && j < value.Length; j++)
				{
					rx_eq10_dsp[j] = value[j];
				}
			}
		}
	}

	public bool RXEQOn
	{
		get
		{
			return rx_eq_on;
		}
		set
		{
			rx_eq_on = value;
			if (update && (value != rx_eq_on_dsp || force))
			{
				WDSP.SetRXAEQRun(WDSP.id(thread, subrx), value);
				rx_eq_on_dsp = value;
			}
		}
	}

	public double NBThreshold
	{
		get
		{
			return nb_threshold;
		}
		set
		{
			nb_threshold = value;
			if (update && (value != nb_threshold_dsp || force))
			{
				if (thread == 0 && subrx == 0)
				{
					cmaster.SetRCVRANBThreshold(0, 0, value);
					cmaster.SetRCVRANBThreshold(2, 0, value);
					cmaster.SetRCVRANBThreshold(2, 1, value);
					cmaster.SetRCVRNOBThreshold(0, 0, value);
					cmaster.SetRCVRNOBThreshold(2, 0, value);
					cmaster.SetRCVRNOBThreshold(2, 1, value);
				}
				else if (thread == 2 && subrx == 0)
				{
					cmaster.SetRCVRANBThreshold(0, 1, value);
					cmaster.SetRCVRNOBThreshold(0, 1, value);
				}
				nb_threshold_dsp = value;
			}
		}
	}

	public double NBTau
	{
		get
		{
			return nb_tau;
		}
		set
		{
			nb_tau = value;
			if (update && (value != nb_tau_dsp || force))
			{
				if (thread == 0 && subrx == 0)
				{
					cmaster.SetRCVRANBTau(0, 0, value);
					cmaster.SetRCVRANBTau(2, 0, value);
					cmaster.SetRCVRANBTau(2, 1, value);
					cmaster.SetRCVRNOBTau(0, 0, value);
					cmaster.SetRCVRNOBTau(2, 0, value);
					cmaster.SetRCVRNOBTau(2, 1, value);
				}
				else if (thread == 2 && subrx == 0)
				{
					cmaster.SetRCVRANBTau(0, 1, value);
					cmaster.SetRCVRNOBTau(0, 1, value);
				}
				nb_tau_dsp = value;
			}
		}
	}

	public double NBAdvTime
	{
		get
		{
			return nb_advtime;
		}
		set
		{
			nb_advtime = value;
			if (update && (value != nb_advtime_dsp || force))
			{
				if (thread == 0 && subrx == 0)
				{
					cmaster.SetRCVRANBAdvtime(0, 0, value);
					cmaster.SetRCVRANBAdvtime(2, 0, value);
					cmaster.SetRCVRANBAdvtime(2, 1, value);
					cmaster.SetRCVRNOBAdvtime(0, 0, value);
					cmaster.SetRCVRNOBAdvtime(2, 0, value);
					cmaster.SetRCVRNOBAdvtime(2, 1, value);
				}
				else if (thread == 2 && subrx == 0)
				{
					cmaster.SetRCVRANBAdvtime(0, 1, value);
					cmaster.SetRCVRNOBAdvtime(0, 1, value);
				}
				nb_advtime_dsp = value;
			}
		}
	}

	public double NBHangTime
	{
		get
		{
			return nb_hangtime;
		}
		set
		{
			nb_hangtime = value;
			if (update && (value != nb_hangtime_dsp || force))
			{
				if (thread == 0 && subrx == 0)
				{
					cmaster.SetRCVRANBHangtime(0, 0, value);
					cmaster.SetRCVRANBHangtime(2, 0, value);
					cmaster.SetRCVRANBHangtime(2, 1, value);
					cmaster.SetRCVRNOBHangtime(0, 0, value);
					cmaster.SetRCVRNOBHangtime(2, 0, value);
					cmaster.SetRCVRNOBHangtime(2, 1, value);
				}
				else if (thread == 2 && subrx == 0)
				{
					cmaster.SetRCVRANBHangtime(0, 1, value);
					cmaster.SetRCVRNOBHangtime(0, 1, value);
				}
				nb_hangtime_dsp = value;
			}
		}
	}

	public int NBMode
	{
		get
		{
			return nb_mode;
		}
		set
		{
			nb_mode = value;
			if (update && (value != nb_mode_dsp || force))
			{
				if (thread == 0 && subrx == 0)
				{
					cmaster.SetRCVRNOBMode(0, 0, value);
					cmaster.SetRCVRNOBMode(2, 0, value);
					cmaster.SetRCVRNOBMode(2, 1, value);
				}
				else if (thread == 2 && subrx == 0)
				{
					cmaster.SetRCVRNOBMode(0, 1, value);
				}
				nb_mode_dsp = value;
			}
		}
	}

	public double RXFixedAGC
	{
		get
		{
			return rx_fixed_agc;
		}
		set
		{
			rx_fixed_agc = value;
			if (update && (value != rx_fixed_agc_dsp || force))
			{
				WDSP.SetRXAAGCFixed(WDSP.id(thread, subrx), value);
				rx_fixed_agc_dsp = value;
			}
		}
	}

	public double RXAGCMaxGain
	{
		get
		{
			return rx_agc_max_gain;
		}
		set
		{
			rx_agc_max_gain = value;
			if (update && (value != rx_agc_max_gain_dsp || force))
			{
				WDSP.SetRXAAGCTop(WDSP.id(thread, subrx), value);
				rx_agc_max_gain_dsp = value;
			}
		}
	}

	public int RXAGCDecay
	{
		get
		{
			return rx_agc_decay;
		}
		set
		{
			rx_agc_decay = value;
			if (update && (value != rx_agc_decay_dsp || force))
			{
				WDSP.SetRXAAGCDecay(WDSP.id(thread, subrx), value);
				rx_agc_decay_dsp = value;
			}
		}
	}

	public int RXAGCHang
	{
		get
		{
			return rx_agc_hang;
		}
		set
		{
			rx_agc_hang = value;
			if (update && (value != rx_agc_hang_dsp || force))
			{
				WDSP.SetRXAAGCHang(WDSP.id(thread, subrx), value);
				rx_agc_hang_dsp = value;
			}
		}
	}

	public double RXOutputGain
	{
		get
		{
			return rx_output_gain;
		}
		set
		{
			rx_output_gain = value;
			if (!update || (value == rx_output_gain_dsp && !force))
			{
				return;
			}
			WDSP.SetRXAPanelGain1(WDSP.id(thread, subrx), value);
			rx_output_gain_dsp = value;
			switch (WDSP.id(thread, subrx))
			{
			case 0:
				if (WaveThing.wave_file_writer[0] != null)
				{
					WaveThing.wave_file_writer[0].RecordGain = (float)value;
				}
				break;
			case 2:
				if (WaveThing.wave_file_writer[1] != null)
				{
					WaveThing.wave_file_writer[1].RecordGain = (float)value;
				}
				break;
			}
		}
	}

	public int RXAGCSlope
	{
		get
		{
			return rx_agc_slope;
		}
		set
		{
			rx_agc_slope = value;
			if (update && (value != rx_agc_slope_dsp || force))
			{
				WDSP.SetRXAAGCSlope(WDSP.id(thread, subrx), value);
				rx_agc_slope_dsp = value;
			}
		}
	}

	public int RXAGCHangThreshold
	{
		get
		{
			return rx_agc_hang_threshold;
		}
		set
		{
			rx_agc_hang_threshold = value;
			if (update && (value != rx_agc_hang_threshold_dsp || force))
			{
				WDSP.SetRXAAGCHangThreshold(WDSP.id(thread, subrx), value);
				rx_agc_hang_threshold_dsp = value;
			}
		}
	}

	public bool BinOn
	{
		get
		{
			return bin_on;
		}
		set
		{
			bin_on = value;
			if (update && (value != bin_on_dsp || force))
			{
				WDSP.SetRXAPanelBinaural(WDSP.id(thread, subrx), value);
				bin_on_dsp = value;
			}
		}
	}

	public float RXSquelchThreshold
	{
		get
		{
			return rx_squelch_threshold;
		}
		set
		{
			rx_squelch_threshold = value;
			if (update && (value != rx_squelch_threshold_dsp || force))
			{
				WDSP.SetRXAAMSQThreshold(WDSP.id(thread, subrx), value);
				rx_squelch_threshold_dsp = value;
			}
		}
	}

	public bool SSqlOn
	{
		get
		{
			return _bSSqlOn;
		}
		set
		{
			_bSSqlOn = value;
			if (update && (value != _bSSqlOn_dsp || force))
			{
				WDSP.SetRXASSQLRun(WDSP.id(thread, subrx), value);
				_bSSqlOn_dsp = value;
			}
		}
	}

	public float SSqlThreshold
	{
		get
		{
			return _fSSqlThreshold;
		}
		set
		{
			_fSSqlThreshold = value;
			if (_fSSqlThreshold < 0f)
			{
				_fSSqlThreshold = 0f;
			}
			if (_fSSqlThreshold > 1f)
			{
				_fSSqlThreshold = 1f;
			}
			if (update && (_fSSqlThreshold != _fSSqlThreshold_dsp || force))
			{
				WDSP.SetRXASSQLThreshold(WDSP.id(thread, subrx), _fSSqlThreshold);
				_fSSqlThreshold_dsp = _fSSqlThreshold;
			}
		}
	}

	public float SqlMuteTimeConstant
	{
		get
		{
			return _fSSqlMuteTimeConstant;
		}
		set
		{
			_fSSqlMuteTimeConstant = value;
			if (_fSSqlMuteTimeConstant < 0.1f)
			{
				_fSSqlMuteTimeConstant = 0.1f;
			}
			if (_fSSqlMuteTimeConstant > 2f)
			{
				_fSSqlMuteTimeConstant = 2f;
			}
			if (update && (_fSSqlMuteTimeConstant != _fSSqlMuteTimeConstant_dsp || force))
			{
				WDSP.SetRXASSQLTauMute(WDSP.id(thread, subrx), _fSSqlMuteTimeConstant);
				_fSSqlMuteTimeConstant_dsp = _fSSqlMuteTimeConstant;
			}
		}
	}

	public float SqlUnMuteTimeConstant
	{
		get
		{
			return _fSSqlUnMuteTimeConstant;
		}
		set
		{
			_fSSqlUnMuteTimeConstant = value;
			if (_fSSqlUnMuteTimeConstant < 0.1f)
			{
				_fSSqlUnMuteTimeConstant = 0.1f;
			}
			if (_fSSqlUnMuteTimeConstant > 1f)
			{
				_fSSqlUnMuteTimeConstant = 1f;
			}
			if (update && (_fSSqlUnMuteTimeConstant != _fSSqlUnMuteTimeConstant_dsp || force))
			{
				WDSP.SetRXASSQLTauUnMute(WDSP.id(thread, subrx), _fSSqlUnMuteTimeConstant);
				_fSSqlUnMuteTimeConstant_dsp = _fSSqlUnMuteTimeConstant;
			}
		}
	}

	public float FMSquelchThreshold
	{
		get
		{
			return fm_squelch_threshold;
		}
		set
		{
			fm_squelch_threshold = value;
			if (update && (value != fm_squelch_threshold_dsp || force))
			{
				WDSP.SetRXAFMSQThreshold(WDSP.id(thread, subrx), value);
				fm_squelch_threshold_dsp = value;
			}
		}
	}

	public bool RXAMSquelchOn
	{
		get
		{
			return rx_am_squelch_on;
		}
		set
		{
			rx_am_squelch_on = value;
			if (update && (value != rx_am_squelch_on_dsp || force))
			{
				WDSP.SetRXAAMSQRun(WDSP.id(thread, subrx), value);
				rx_am_squelch_on_dsp = value;
			}
		}
	}

	public bool RXFMSquelchOn
	{
		get
		{
			return rx_fm_squelch_on;
		}
		set
		{
			rx_fm_squelch_on = value;
			if (update && (value != rx_fm_squelch_on_dsp || force))
			{
				WDSP.SetRXAFMSQRun(WDSP.id(thread, subrx), value);
				rx_fm_squelch_on_dsp = value;
			}
		}
	}

	public double RXAMSquelchMaxTail
	{
		get
		{
			return rx_am_squelch_max_tail;
		}
		set
		{
			rx_am_squelch_max_tail = value;
			if (update && (value != rx_am_squelch_max_tail_dsp || force))
			{
				WDSP.SetRXAAMSQMaxTail(WDSP.id(thread, subrx), value);
				rx_am_squelch_max_tail_dsp = value;
			}
		}
	}

	public bool SpectrumPreFilter
	{
		get
		{
			return spectrum_pre_filter;
		}
		set
		{
			spectrum_pre_filter = value;
			if (update && (value != spectrum_pre_filter_dsp || force))
			{
				spectrum_pre_filter_dsp = value;
			}
		}
	}

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			active = value;
			if (update && (value != active_dsp || force))
			{
				active_dsp = value;
			}
		}
	}

	public float Pan
	{
		get
		{
			return pan;
		}
		set
		{
			pan = value;
			if (update && (value != pan_dsp || force))
			{
				WDSP.SetRXAPanelPan(WDSP.id(thread, subrx), value);
				pan_dsp = value;
			}
		}
	}

	public double RXOsc
	{
		get
		{
			return rx_osc;
		}
		set
		{
			rx_osc = value;
			if (update && (value != rx_osc_dsp || force))
			{
				WDSP.SetRXAShiftFreq(WDSP.id(thread, subrx), 0.0 - value);
				WDSP.RXANBPSetShiftFrequency(WDSP.id(thread, subrx), 0.0 - value);
				rx_osc_dsp = value;
			}
		}
	}

	public double RXFMDeviation
	{
		get
		{
			return rx_fm_deviation;
		}
		set
		{
			rx_fm_deviation = value;
			if (update && (value != rx_fm_deviation_dsp || force))
			{
				WDSP.SetRXAFMDeviation(WDSP.id(thread, subrx), value);
				rx_fm_deviation_dsp = value;
			}
		}
	}

	public bool RXFMCTCSSFilter
	{
		get
		{
			return rx_fm_ctcss_filter;
		}
		set
		{
			rx_fm_ctcss_filter = value;
			if (update && (value != rx_fm_ctcss_filter_dsp || force))
			{
				WDSP.SetRXACTCSSRun(WDSP.id(thread, subrx), value);
				rx_fm_ctcss_filter_dsp = value;
			}
		}
	}

	public bool RXFMDETLIMRUN
	{
		get
		{
			return rx_fm_detector_limiter;
		}
		set
		{
			rx_fm_detector_limiter = value;
			if (update && (value != rx_fm_detector_limiter_dsp || force))
			{
				WDSP.SetRXAFMLimRun(WDSP.id(thread, subrx), value);
				rx_fm_detector_limiter_dsp = value;
			}
		}
	}

	public double RXFMDETLIMGAIN
	{
		get
		{
			return rx_fm_limiter_gain;
		}
		set
		{
			rx_fm_limiter_gain = value;
			if (update && (value != rx_fm_limiter_gain_dsp || force))
			{
				WDSP.SetRXAFMLimGain(WDSP.id(thread, subrx), value);
				rx_fm_limiter_gain_dsp = value;
			}
		}
	}

	public double RXFMLowCut
	{
		get
		{
			return rx_fm_lowcut;
		}
		set
		{
			rx_fm_lowcut = value;
			if (update && (value != rx_fm_lowcut_dsp || force))
			{
				WDSP.SetRXAFMAFFilter(WDSP.id(thread, subrx), value, rx_fm_highcut_dsp);
				rx_fm_lowcut_dsp = value;
			}
		}
	}

	public double RXFMHighCut
	{
		get
		{
			return rx_fm_highcut;
		}
		set
		{
			rx_fm_highcut = value;
			if (update && (value != rx_fm_highcut_dsp || force))
			{
				WDSP.SetRXAFMAFFilter(WDSP.id(thread, subrx), rx_fm_lowcut_dsp, value);
				rx_fm_highcut_dsp = value;
			}
		}
	}

	public int RXANFPosition
	{
		get
		{
			return rx_anf_position;
		}
		set
		{
			rx_anf_position = value;
			if (update && (value != rx_anf_position_dsp || force))
			{
				WDSP.SetRXAANFPosition(WDSP.id(thread, subrx), value);
				rx_anf_position_dsp = value;
			}
		}
	}

	public int RXANRPosition
	{
		get
		{
			return rx_anr_position;
		}
		set
		{
			rx_anr_position = value;
			if (update && (value != rx_anr_position_dsp || force))
			{
				WDSP.SetRXAANRPosition(WDSP.id(thread, subrx), value);
				rx_anr_position_dsp = value;
			}
		}
	}

	public bool RXCBLRun
	{
		get
		{
			return rx_cbl_run;
		}
		set
		{
			rx_cbl_run = value;
			if (update && (value != rx_cbl_run_dsp || force))
			{
				WDSP.SetRXACBLRun(WDSP.id(thread, subrx), value);
				rx_cbl_run_dsp = value;
			}
		}
	}

	public int RXCBLPosition
	{
		get
		{
			return rx_cbl_position;
		}
		set
		{
			rx_cbl_position = value;
			if (update && (value != rx_cbl_position_dsp || force))
			{
				WDSP.SetRXACBLPosition(WDSP.id(thread, subrx), value);
				rx_cbl_position_dsp = value;
			}
		}
	}

	public int RXAMDFadeLevel
	{
		get
		{
			return rx_amd_fadelevel;
		}
		set
		{
			rx_amd_fadelevel = value;
			if (update && (value != rx_amd_fadelevel_dsp || force))
			{
				WDSP.SetRXAAMDFadeLevel(WDSP.id(thread, subrx), value);
				rx_amd_fadelevel_dsp = value;
			}
		}
	}

	public int RXAMDSBMode
	{
		get
		{
			return rx_amd_sbmode;
		}
		set
		{
			rx_amd_sbmode = value;
			if (update && (value != rx_amd_sbmode_dsp || force))
			{
				WDSP.SetRXAAMDSBMode(WDSP.id(thread, subrx), value);
				rx_amd_sbmode_dsp = value;
			}
		}
	}

	public int RXBandpassWindow
	{
		get
		{
			return rx_bandpass_window;
		}
		set
		{
			rx_bandpass_window = value;
			if (update && (value != rx_bandpass_window_dsp || force))
			{
				WDSP.SetRXABandpassWindow(WDSP.id(thread, subrx), value);
				WDSP.RXANBPSetWindow(WDSP.id(thread, subrx), value);
				rx_bandpass_window_dsp = value;
			}
		}
	}

	public int RXPreGenRun
	{
		get
		{
			return rx_pregen_run;
		}
		set
		{
			rx_pregen_run = value;
			if (update && (value != rx_pregen_run_dsp || force))
			{
				WDSP.SetRXAPreGenRun(WDSP.id(thread, subrx), value);
				rx_pregen_run_dsp = value;
			}
		}
	}

	public int RXPreGenMode
	{
		get
		{
			return rx_pregen_mode;
		}
		set
		{
			rx_pregen_mode = value;
			if (update && (value != rx_pregen_mode_dsp || force))
			{
				WDSP.SetRXAPreGenMode(WDSP.id(thread, subrx), value);
				rx_pregen_mode_dsp = value;
			}
		}
	}

	public double RXPreGenToneMag
	{
		get
		{
			return rx_pregen_tone_mag;
		}
		set
		{
			rx_pregen_tone_mag = value;
			if (update && (value != rx_pregen_tone_mag_dsp || force))
			{
				WDSP.SetRXAPreGenToneMag(WDSP.id(thread, subrx), value);
				rx_pregen_tone_mag_dsp = value;
			}
		}
	}

	public double RXPreGenToneFreq
	{
		get
		{
			return rx_pregen_tone_freq;
		}
		set
		{
			rx_pregen_tone_freq = value;
			if (update && (value != rx_pregen_tone_freq_dsp || force))
			{
				WDSP.SetRXAPreGenToneFreq(WDSP.id(thread, subrx), value);
				rx_pregen_tone_freq_dsp = value;
			}
		}
	}

	public double RXPreGenNoiseMag
	{
		get
		{
			return rx_pregen_noise_mag;
		}
		set
		{
			rx_pregen_noise_mag = value;
			if (update && (value != rx_pregen_noise_mag_dsp || force))
			{
				WDSP.SetRXAPreGenNoiseMag(WDSP.id(thread, subrx), value);
				rx_pregen_noise_mag_dsp = value;
			}
		}
	}

	public double RXPreGenSweepMag
	{
		get
		{
			return rx_pregen_sweep_mag;
		}
		set
		{
			rx_pregen_sweep_mag = value;
			if (update && (value != rx_pregen_sweep_mag_dsp || force))
			{
				WDSP.SetRXAPreGenSweepMag(WDSP.id(thread, subrx), value);
				rx_pregen_sweep_mag_dsp = value;
			}
		}
	}

	public double RXPreGenSweepFreq1
	{
		get
		{
			return rx_pregen_sweep_freq1;
		}
		set
		{
			rx_pregen_sweep_freq1 = value;
			if (update && (value != rx_pregen_sweep_freq1_dsp || force))
			{
				rx_pregen_sweep_freq1_dsp = value;
				WDSP.SetRXAPreGenSweepFreq(WDSP.id(thread, subrx), rx_pregen_sweep_freq1_dsp, rx_pregen_sweep_freq2_dsp);
			}
		}
	}

	public double RXPreGenSweepFreq2
	{
		get
		{
			return rx_pregen_sweep_freq2;
		}
		set
		{
			rx_pregen_sweep_freq2 = value;
			if (update && (value != rx_pregen_sweep_freq2_dsp || force))
			{
				rx_pregen_sweep_freq2_dsp = value;
				WDSP.SetRXAPreGenSweepFreq(WDSP.id(thread, subrx), rx_pregen_sweep_freq1_dsp, rx_pregen_sweep_freq2_dsp);
			}
		}
	}

	public double RXPreGenSweepRate
	{
		get
		{
			return rx_pregen_sweep_rate;
		}
		set
		{
			rx_pregen_sweep_rate = value;
			if (update && (value != rx_pregen_sweep_rate_dsp || force))
			{
				WDSP.SetRXAPreGenSweepRate(WDSP.id(thread, subrx), value);
				rx_pregen_sweep_rate_dsp = value;
			}
		}
	}

	public bool RXAPFRun
	{
		get
		{
			return rx_apf_run;
		}
		set
		{
			rx_apf_run = value;
			if (update && (value != rx_apf_run_dsp || force))
			{
				WDSP.SetRXASPCWRun(WDSP.id(thread, subrx), value);
				rx_apf_run_dsp = value;
			}
		}
	}

	public double RXAPFFreq
	{
		get
		{
			return rx_apf_freq;
		}
		set
		{
			rx_apf_freq = value;
			if (update && (value != rx_apf_freq_dsp || force))
			{
				WDSP.SetRXASPCWFreq(WDSP.id(thread, subrx), value);
				rx_apf_freq_dsp = value;
			}
		}
	}

	public double RXAPFBw
	{
		get
		{
			return rx_apf_bw;
		}
		set
		{
			rx_apf_bw = value;
			if (update && (value != rx_apf_bw_dsp || force))
			{
				WDSP.SetRXASPCWBandwidth(WDSP.id(thread, subrx), value);
				rx_apf_bw_dsp = value;
			}
		}
	}

	public double RXAPFGain
	{
		get
		{
			return rx_apf_gain;
		}
		set
		{
			rx_apf_gain = value;
			if (update && (value != rx_apf_gain_dsp || force))
			{
				WDSP.SetRXASPCWGain(WDSP.id(thread, subrx), value);
				rx_apf_gain_dsp = value;
			}
		}
	}

	public int RXAPFType
	{
		get
		{
			return _rx_apf_type;
		}
		set
		{
			_rx_apf_type = value;
			if (update && (value != _rx_apf_type_dsp || force))
			{
				WDSP.SetRXASPCWSelection(WDSP.id(thread, subrx), value);
				_rx_apf_type_dsp = value;
			}
		}
	}

	public bool RXADollyRun
	{
		get
		{
			return rx_dolly_run;
		}
		set
		{
			rx_dolly_run = value;
			if (update && (value != rx_dolly_run_dsp || force))
			{
				WDSP.SetRXAmpeakRun(WDSP.id(thread, subrx), value);
				rx_dolly_run_dsp = value;
			}
		}
	}

	public double RXADollyFreq0
	{
		get
		{
			return rx_dolly_freq0;
		}
		set
		{
			rx_dolly_freq0 = value;
			if (update && (value != rx_dolly_freq0_dsp || force))
			{
				WDSP.SetRXAmpeakFilFreq(WDSP.id(thread, subrx), 0, value);
				rx_dolly_freq0_dsp = value;
			}
		}
	}

	public double RXADollyFreq1
	{
		get
		{
			return rx_dolly_freq1;
		}
		set
		{
			rx_dolly_freq1 = value;
			if (update && (value != rx_dolly_freq1_dsp || force))
			{
				WDSP.SetRXAmpeakFilFreq(WDSP.id(thread, subrx), 1, value);
				rx_dolly_freq1_dsp = value;
			}
		}
	}

	public int RXANR2GainMethod
	{
		get
		{
			return rx_nr2_gain_method;
		}
		set
		{
			rx_nr2_gain_method = value;
			if (update && (value != rx_nr2_gain_method_dsp || force))
			{
				WDSP.SetRXAEMNRgainMethod(WDSP.id(thread, subrx), value);
				rx_nr2_gain_method_dsp = value;
			}
		}
	}

	public int RXANR2NPEMethod
	{
		get
		{
			return rx_nr2_npe_method;
		}
		set
		{
			rx_nr2_npe_method = value;
			if (update && (value != rx_nr2_npe_method_dsp || force))
			{
				WDSP.SetRXAEMNRnpeMethod(WDSP.id(thread, subrx), value);
				rx_nr2_npe_method_dsp = value;
			}
		}
	}

	public int RXANR2AERun
	{
		get
		{
			return rx_nr2_ae_run;
		}
		set
		{
			rx_nr2_ae_run = value;
			if (update && (value != rx_nr2_ae_run_dsp || force))
			{
				WDSP.SetRXAEMNRaeRun(WDSP.id(thread, subrx), value);
				rx_nr2_ae_run_dsp = value;
			}
		}
	}

	public int RXAEMNRpost2Run
	{
		get
		{
			return rx_nr2_ae_post2_run;
		}
		set
		{
			rx_nr2_ae_post2_run = value;
			if (update && (value != rx_nr2_ae_post2_run_dsp || force))
			{
				WDSP.SetRXAEMNRpost2Run(WDSP.id(thread, subrx), value);
				rx_nr2_ae_post2_run_dsp = value;
			}
		}
	}

	public double RXAEMNRpost2Nlevel
	{
		get
		{
			return rx_nr2_ae_post2_nlevel;
		}
		set
		{
			rx_nr2_ae_post2_nlevel = value;
			if (update && (value != rx_nr2_ae_post2_nlevel_dsp || force))
			{
				WDSP.SetRXAEMNRpost2Nlevel(WDSP.id(thread, subrx), value);
				rx_nr2_ae_post2_nlevel_dsp = value;
			}
		}
	}

	public double RXAEMNRpost2Factor
	{
		get
		{
			return rx_nr2_ae_post2_factor;
		}
		set
		{
			rx_nr2_ae_post2_factor = value;
			if (update && (value != rx_nr2_ae_post2_factor_dsp || force))
			{
				WDSP.SetRXAEMNRpost2Factor(WDSP.id(thread, subrx), value);
				rx_nr2_ae_post2_factor_dsp = value;
			}
		}
	}

	public double RXAEMNRpost2Rate
	{
		get
		{
			return rx_nr2_ae_post2_rate;
		}
		set
		{
			rx_nr2_ae_post2_rate = value;
			if (update && (value != rx_nr2_ae_post2_rate_dsp || force))
			{
				WDSP.SetRXAEMNRpost2Rate(WDSP.id(thread, subrx), value);
				rx_nr2_ae_post2_rate_dsp = value;
			}
		}
	}

	public int RXAEMNRpost2Taper
	{
		get
		{
			return rx_nr2_ae_post2_taper;
		}
		set
		{
			rx_nr2_ae_post2_taper = value;
			if (update && (value != rx_nr2_ae_post2_taper_dsp || force))
			{
				WDSP.SetRXAEMNRpost2Taper(WDSP.id(thread, subrx), value);
				rx_nr2_ae_post2_taper_dsp = value;
			}
		}
	}

	public int RXANR2Run
	{
		get
		{
			return rx_nr2_run;
		}
		set
		{
			rx_nr2_run = value;
			if (update && (value != rx_nr2_run_dsp || force))
			{
				WDSP.SetRXAEMNRRun(WDSP.id(thread, subrx), value);
				rx_nr2_run_dsp = value;
			}
		}
	}

	public int RXANR2Position
	{
		get
		{
			return rx_nr2_position;
		}
		set
		{
			rx_nr2_position = value;
			if (update && (value != rx_nr2_position_dsp || force))
			{
				WDSP.SetRXAEMNRPosition(WDSP.id(thread, subrx), value);
				rx_nr2_position_dsp = value;
			}
		}
	}

	public int RXANR3Run
	{
		get
		{
			return rx_nr3_run;
		}
		set
		{
			rx_nr3_run = value;
			if (update && (value != rx_nr3_run_dsp || force))
			{
				WDSP.SetRXARNNRRun(WDSP.id(thread, subrx), value);
				rx_nr3_run_dsp = value;
			}
		}
	}

	public int RXANR3Position
	{
		get
		{
			return rx_nr3_position;
		}
		set
		{
			rx_nr3_position = value;
			if (update && (value != rx_nr3_position_dsp || force))
			{
				WDSP.SetRXARNNRPosition(WDSP.id(thread, subrx), value);
				rx_nr3_position_dsp = value;
			}
		}
	}

	public int RXANR3FixedGain
	{
		get
		{
			return rx_nr3_fixed_gain;
		}
		set
		{
			rx_nr3_fixed_gain = value;
			if (update && (value != rx_nr3_fixed_gain_dsp || force))
			{
				WDSP.SetRXARNNRUseDefaultGain(WDSP.id(thread, subrx), value);
				rx_nr3_fixed_gain_dsp = value;
			}
		}
	}

	public int RXANR4Run
	{
		get
		{
			return rx_nr4_run;
		}
		set
		{
			rx_nr4_run = value;
			if (update && (value != rx_nr4_run_dsp || force))
			{
				WDSP.SetRXASBNRRun(WDSP.id(thread, subrx), value);
				rx_nr4_run_dsp = value;
			}
		}
	}

	public int RXANR4Position
	{
		get
		{
			return rx_nr4_position;
		}
		set
		{
			rx_nr4_position = value;
			if (update && (value != rx_nr4_position_dsp || force))
			{
				WDSP.SetRXASBNRPosition(WDSP.id(thread, subrx), value);
				rx_nr4_position_dsp = value;
			}
		}
	}

	public float RXASBNRreductionAmount
	{
		get
		{
			return rx_nr4_reductionAmount;
		}
		set
		{
			rx_nr4_reductionAmount = value;
			if (update && (value != rx_nr4_reductionAmount_dsp || force))
			{
				WDSP.SetRXASBNRreductionAmount(WDSP.id(thread, subrx), value);
				rx_nr4_reductionAmount_dsp = value;
			}
		}
	}

	public float RXASBNRsmoothingFactor
	{
		get
		{
			return rx_nr4_smoothingFactor;
		}
		set
		{
			rx_nr4_smoothingFactor = value;
			if (update && (value != rx_nr4_smoothingFactor_dsp || force))
			{
				WDSP.SetRXASBNRsmoothingFactor(WDSP.id(thread, subrx), value);
				rx_nr4_smoothingFactor_dsp = value;
			}
		}
	}

	public float RXASBNRwhiteningFactor
	{
		get
		{
			return rx_nr4_whiteningFactor;
		}
		set
		{
			rx_nr4_whiteningFactor = value;
			if (update && (value != rx_nr4_whiteningFactor_dsp || force))
			{
				WDSP.SetRXASBNRwhiteningFactor(WDSP.id(thread, subrx), value);
				rx_nr4_whiteningFactor_dsp = value;
			}
		}
	}

	public float RXASBNRnoiseRescale
	{
		get
		{
			return rx_nr4_noiseRescale;
		}
		set
		{
			rx_nr4_noiseRescale = value;
			if (update && (value != rx_nr4_noiseRescale_dsp || force))
			{
				WDSP.SetRXASBNRnoiseRescale(WDSP.id(thread, subrx), value);
				rx_nr4_noiseRescale_dsp = value;
			}
		}
	}

	public float RXASBNRpostFilterThreshold
	{
		get
		{
			return rx_nr4_postFilterThreshold;
		}
		set
		{
			rx_nr4_postFilterThreshold = value;
			if (update && (value != rx_nr4_postFilterThreshold_dsp || force))
			{
				WDSP.SetRXASBNRpostFilterThreshold(WDSP.id(thread, subrx), value);
				rx_nr4_postFilterThreshold_dsp = value;
			}
		}
	}

	public int RXASBNRnoiseScalingType
	{
		get
		{
			return rx_nr4_noiseScalingType;
		}
		set
		{
			rx_nr4_noiseScalingType = value;
			if (update && (value != rx_nr4_noiseScalingType_dsp || force))
			{
				WDSP.SetRXASBNRnoiseScalingType(WDSP.id(thread, subrx), value);
				rx_nr4_noiseScalingType_dsp = value;
			}
		}
	}

	public RadioDSPRX(uint t, uint rx)
	{
		thread = t;
		subrx = rx;
	}

	public void Copy(RadioDSPRX rx)
	{
		DSPMode = rx.dsp_mode;
		FilterSize = rx.filter_size;
		FilterType = rx.filter_type;
		SetRXFilter(rx.rx_filter_low, rx.rx_filter_high);
		RXANR1Run = rx.noise_reduction;
		SetNRVals(rx.nr_taps, rx.nr_delay, rx.nr_gain, rx.nr_leak);
		AutoNotchFilter = rx.auto_notch_filter;
		SetANFVals(rx.anf_taps, rx.anf_delay, rx.anf_gain, rx.anf_leak);
		RXAGCMode = rx.rx_agc_mode;
		RXEQNumBands = rx_eq_num_bands;
		if (LegacyEQ)
		{
			if (rx_eq_num_bands == 3)
			{
				RXEQ10 = rx.rx_eq10;
				RXEQ3 = rx.rx_eq3;
			}
			else
			{
				RXEQ3 = rx.rx_eq3;
				RXEQ10 = rx.rx_eq10;
			}
		}
		RXEQOn = rx.rx_eq_on;
		NBThreshold = rx.nb_threshold;
		NBTau = rx.nb_tau;
		NBAdvTime = rx.nb_advtime;
		NBHangTime = rx.nb_hangtime;
		NBMode = rx.nb_mode;
		RXFixedAGC = rx.rx_fixed_agc;
		RXAGCMaxGain = rx.rx_agc_max_gain;
		RXAGCDecay = rx.rx_agc_decay;
		RXAGCHang = rx.rx_agc_hang;
		RXOutputGain = rx.rx_output_gain;
		RXAGCSlope = rx.rx_agc_slope;
		RXAGCHangThreshold = rx.rx_agc_hang_threshold;
		BinOn = rx.bin_on;
		RXSquelchThreshold = rx.rx_squelch_threshold;
		FMSquelchThreshold = rx.fm_squelch_threshold;
		RXAMSquelchMaxTail = rx.rx_am_squelch_max_tail;
		RXAMSquelchOn = rx.rx_am_squelch_on;
		SpectrumPreFilter = rx.spectrum_pre_filter;
		Active = rx.active;
		Pan = rx.pan;
		RXOsc = rx.rx_osc;
		RXFMSquelchOn = rx.rx_fm_squelch_on;
		RXFMDeviation = rx.rx_fm_deviation;
		RXFMCTCSSFilter = rx.rx_fm_ctcss_filter;
		RXFMDETLIMRUN = rx.rx_fm_detector_limiter;
		RXFMDETLIMGAIN = rx.rx_fm_limiter_gain;
		RXANFPosition = rx.rx_anf_position;
		RXANRPosition = rx.rx_anr_position;
		RXCBLRun = rx.rx_cbl_run;
		RXCBLPosition = rx.rx_cbl_position;
		RXAMDFadeLevel = rx.rx_amd_fadelevel;
		RXAMDSBMode = rx.rx_amd_sbmode;
		RXBandpassWindow = rx.rx_bandpass_window;
		RXPreGenRun = rx.rx_pregen_run;
		RXPreGenMode = rx.rx_pregen_mode;
		RXPreGenToneMag = rx.rx_pregen_tone_mag;
		RXPreGenToneFreq = rx.rx_pregen_tone_freq;
		RXPreGenNoiseMag = rx.rx_pregen_noise_mag;
		RXPreGenSweepMag = rx.rx_pregen_sweep_mag;
		RXPreGenSweepFreq1 = rx.rx_pregen_sweep_freq1;
		RXPreGenSweepFreq2 = rx.rx_pregen_sweep_freq2;
		RXPreGenSweepRate = rx.rx_pregen_sweep_rate;
		RXAPFRun = rx.rx_apf_run;
		RXAPFFreq = rx.rx_apf_freq;
		RXAPFBw = rx.rx_apf_bw;
		RXAPFGain = rx.rx_apf_gain;
		RXAPFType = rx._rx_apf_type;
		RXADollyRun = rx.rx_dolly_run;
		RXADollyFreq0 = rx.rx_dolly_freq0;
		RXADollyFreq1 = rx.rx_dolly_freq1;
		RXANR2GainMethod = rx.rx_nr2_gain_method;
		RXANR2NPEMethod = rx.rx_nr2_npe_method;
		RXANR2AERun = rx.rx_nr2_ae_run;
		RXAEMNRpost2Run = rx.rx_nr2_ae_post2_run;
		RXAEMNRpost2Nlevel = rx.rx_nr2_ae_post2_nlevel;
		RXAEMNRpost2Factor = rx.rx_nr2_ae_post2_factor;
		RXAEMNRpost2Rate = rx.rx_nr2_ae_post2_rate;
		RXAEMNRpost2Taper = rx.rx_nr2_ae_post2_taper;
		RXANR2Run = rx.rx_nr2_run;
		RXANR2Position = rx.rx_nr2_position;
		RXANR3Run = rx.rx_nr3_run;
		RXANR3Position = rx.rx_nr3_position;
		RXANR3FixedGain = rx.rx_nr3_fixed_gain;
		RXANR4Run = rx.rx_nr4_run;
		RXANR4Position = rx.rx_nr4_position;
		RXASBNRreductionAmount = rx.rx_nr4_reductionAmount;
		RXASBNRsmoothingFactor = rx.rx_nr4_smoothingFactor;
		RXASBNRwhiteningFactor = rx.rx_nr4_whiteningFactor;
		RXASBNRnoiseRescale = rx.rx_nr4_noiseRescale;
		RXASBNRpostFilterThreshold = rx.rx_nr4_postFilterThreshold;
		RXASBNRnoiseScalingType = rx.rx_nr4_noiseScalingType;
		RXFilterLow = rx.rx_filter_low;
		RXFilterHigh = rx.rx_filter_high;
	}

	private void SyncAll()
	{
		DSPMode = dsp_mode;
		FilterSize = filter_size;
		FilterType = filter_type;
		SetRXFilter(rx_filter_low, rx_filter_high);
		RXANR1Run = noise_reduction;
		SetNRVals(nr_taps, nr_delay, nr_gain, nr_leak);
		AutoNotchFilter = auto_notch_filter;
		SetANFVals(anf_taps, anf_delay, anf_gain, anf_leak);
		RXAGCMode = rx_agc_mode;
		if (LegacyEQ)
		{
			if (rx_eq_num_bands == 3)
			{
				RXEQ10 = rx_eq10;
				RXEQ3 = rx_eq3;
			}
			else
			{
				RXEQ3 = rx_eq3;
				RXEQ10 = rx_eq10;
			}
		}
		RXEQOn = rx_eq_on;
		NBThreshold = nb_threshold;
		NBTau = nb_tau;
		NBAdvTime = nb_advtime;
		NBHangTime = nb_hangtime;
		NBMode = nb_mode;
		RXFixedAGC = rx_fixed_agc;
		RXAGCMaxGain = rx_agc_max_gain;
		RXAGCDecay = rx_agc_decay;
		RXAGCHang = rx_agc_hang;
		RXOutputGain = rx_output_gain;
		RXAGCSlope = rx_agc_slope;
		RXAGCHangThreshold = rx_agc_hang_threshold;
		BinOn = bin_on;
		RXSquelchThreshold = rx_squelch_threshold;
		RXAMSquelchMaxTail = rx_am_squelch_max_tail;
		RXAMSquelchOn = rx_am_squelch_on;
		FMSquelchThreshold = fm_squelch_threshold;
		SpectrumPreFilter = spectrum_pre_filter;
		Active = active;
		Pan = pan;
		RXOsc = rx_osc;
		RXFMDeviation = rx_fm_deviation;
		RXFMDETLIMRUN = rx_fm_detector_limiter;
		RXFMDETLIMGAIN = rx_fm_limiter_gain;
		RXFMCTCSSFilter = rx_fm_ctcss_filter;
		RXANFPosition = rx_anf_position;
		RXANRPosition = rx_anr_position;
		RXCBLRun = rx_cbl_run;
		RXCBLPosition = rx_cbl_position;
		RXAMDFadeLevel = rx_amd_fadelevel;
		RXAMDSBMode = rx_amd_sbmode;
		RXBandpassWindow = rx_bandpass_window;
		RXPreGenRun = rx_pregen_run;
		RXPreGenMode = rx_pregen_mode;
		RXPreGenToneMag = rx_pregen_tone_mag;
		RXPreGenToneFreq = rx_pregen_tone_freq;
		RXPreGenNoiseMag = rx_pregen_noise_mag;
		RXPreGenSweepMag = rx_pregen_sweep_mag;
		RXPreGenSweepFreq1 = rx_pregen_sweep_freq1;
		RXPreGenSweepFreq2 = rx_pregen_sweep_freq2;
		RXPreGenSweepRate = rx_pregen_sweep_rate;
		RXAPFRun = rx_apf_run;
		RXAPFFreq = rx_apf_freq;
		RXAPFBw = rx_apf_bw;
		RXAPFGain = rx_apf_gain;
		RXAPFType = _rx_apf_type;
		RXADollyRun = rx_dolly_run;
		RXADollyFreq0 = rx_dolly_freq0;
		RXADollyFreq1 = rx_dolly_freq1;
		RXANR2GainMethod = rx_nr2_gain_method;
		RXANR2NPEMethod = rx_nr2_npe_method;
		RXANR2AERun = rx_nr2_ae_run;
		RXAEMNRpost2Run = rx_nr2_ae_post2_run;
		RXAEMNRpost2Nlevel = rx_nr2_ae_post2_nlevel;
		RXAEMNRpost2Factor = rx_nr2_ae_post2_factor;
		RXAEMNRpost2Rate = rx_nr2_ae_post2_rate;
		RXAEMNRpost2Taper = rx_nr2_ae_post2_taper;
		RXANR2Run = rx_nr2_run;
		RXANR2Position = rx_nr2_position;
		RXANR3Run = rx_nr3_run;
		RXANR3Position = rx_nr3_position;
		RXANR3FixedGain = rx_nr3_fixed_gain;
		RXANR4Run = rx_nr4_run;
		RXANR4Position = rx_nr4_position;
		RXASBNRreductionAmount = rx_nr4_reductionAmount;
		RXASBNRsmoothingFactor = rx_nr4_smoothingFactor;
		RXASBNRwhiteningFactor = rx_nr4_whiteningFactor;
		RXASBNRnoiseRescale = rx_nr4_noiseRescale;
		RXASBNRpostFilterThreshold = rx_nr4_postFilterThreshold;
		RXASBNRnoiseScalingType = rx_nr4_noiseScalingType;
		RXFMLowCut = rx_fm_lowcut;
		RXFMHighCut = rx_fm_highcut;
	}

	public void SetRXFilter(int low, int high)
	{
		rx_filter_low = low;
		rx_filter_high = high;
		if (update && (low != rx_filter_low_dsp || high != rx_filter_high_dsp || force))
		{
			WDSP.RXANBPSetFreqs(WDSP.id(thread, subrx), low, high);
			WDSP.SetRXABandpassFreqs(WDSP.id(thread, subrx), low, high);
			WDSP.SetRXASNBAOutputBandwidth(WDSP.id(thread, subrx), low, high);
			rx_filter_low_dsp = low;
			rx_filter_high_dsp = high;
		}
	}

	public void SetNRVals(int taps, int delay, double gain, double leak)
	{
		nr_taps = taps;
		nr_delay = delay;
		nr_gain = gain;
		nr_leak = leak;
		if (update && (taps != nr_taps_dsp || delay != nr_delay_dsp || gain != nr_gain_dsp || leak != nr_leak_dsp || force))
		{
			WDSP.SetRXAANRVals(WDSP.id(thread, subrx), taps, delay, gain, leak);
			nr_taps_dsp = taps;
			nr_delay_dsp = delay;
			nr_gain_dsp = gain;
			nr_leak_dsp = leak;
		}
	}

	public void SetANFVals(int taps, int delay, double gain, double leak)
	{
		anf_taps = taps;
		anf_delay = delay;
		anf_gain = gain;
		anf_leak = leak;
		if (update && (taps != anf_taps_dsp || delay != anf_delay_dsp || gain != anf_gain_dsp || leak != anf_leak_dsp || force))
		{
			WDSP.SetRXAANFVals(WDSP.id(thread, subrx), taps, delay, gain, leak);
			anf_taps_dsp = taps;
			anf_delay_dsp = delay;
			anf_gain_dsp = gain;
			anf_leak_dsp = leak;
		}
	}

	public bool GetNotchOn(int index)
	{
		return notch_on[index];
	}

	public void SetNotchOn(uint index, bool b)
	{
		notch_on[index] = b;
		if (update && (b != notch_on_dsp[index] || force))
		{
			notch_on_dsp[index] = b;
		}
	}

	public double GetNotchFreq(uint index)
	{
		return notch_freq[index];
	}

	public void SetNotchFreq(uint index, double freq)
	{
		notch_freq[index] = freq;
		if (update && (freq != notch_freq_dsp[index] || force))
		{
			notch_freq_dsp[index] = freq;
		}
	}

	public double GetNotchBW(uint index)
	{
		return notch_bw[index];
	}

	public void SetNotchBW(uint index, double bw)
	{
		notch_bw[index] = bw;
		if (update && (bw != notch_bw_dsp[index] || force))
		{
			notch_bw_dsp[index] = bw;
		}
	}
}
