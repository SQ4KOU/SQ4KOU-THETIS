using System;
using System.Runtime.InteropServices;

namespace Thetis;

public class SpecHPSDR
{
	private int disp;

	private bool update;

	private int spur_eliminationtion_ffts = 1;

	private int data_type = 1;

	private int fft_size = 4096;

	private int blocksize;

	private int window_type = 4;

	private double kaiser_pi = 14.0;

	private int overlap = 30000;

	private int clip;

	private double span_clip_l;

	private double span_clip_h;

	private int pixels = 2048;

	private int stitches = 1;

	private int calibration_data_set;

	private double span_min_freq;

	private double span_max_freq;

	private bool average_on;

	private bool peak_on;

	private int det_type_pan;

	private int det_type_wf;

	private bool norm_oneHz_pan;

	private int frame_rate = 15;

	private const int MAX_AV_FRAMES = 60;

	private double tau;

	private double tau_wf;

	private int av_mode;

	private int av_mode_wf;

	private double z_slider;

	private double pan_slider;

	private int sample_rate;

	private bool nb_on;

	private bool nb2_on;

	private int _pixel_out = 2;

	private bool _ignore_frequency_offset;

	private const double KEEP_TIME = 0.1;

	private int max_w;

	private int _low_freq;

	private int _high_freq;

	public bool Update
	{
		get
		{
			return update;
		}
		set
		{
			update = value;
		}
	}

	public int SpurEliminationFFTS
	{
		get
		{
			return spur_eliminationtion_ffts;
		}
		set
		{
			spur_eliminationtion_ffts = value;
		}
	}

	public int DataType
	{
		get
		{
			return data_type;
		}
		set
		{
			data_type = value;
		}
	}

	public int FFTSize
	{
		get
		{
			return fft_size;
		}
		set
		{
			fft_size = value;
			if (update)
			{
				initAnalyzer();
			}
		}
	}

	public int BlockSize
	{
		get
		{
			return blocksize;
		}
		set
		{
			blocksize = value;
			if (update)
			{
				initAnalyzer();
			}
			_ = disp;
			_ = disp;
			_ = 1;
		}
	}

	public int WindowType
	{
		get
		{
			return window_type;
		}
		set
		{
			window_type = value;
			if (update)
			{
				initAnalyzer();
			}
		}
	}

	public double KaiserPi
	{
		get
		{
			return kaiser_pi;
		}
		set
		{
			kaiser_pi = value;
		}
	}

	public int Overlap
	{
		get
		{
			return overlap;
		}
		set
		{
			overlap = value;
		}
	}

	public int Clip
	{
		get
		{
			return clip;
		}
		set
		{
			clip = value;
		}
	}

	public double SpanClipL
	{
		get
		{
			return span_clip_l;
		}
		set
		{
			span_clip_l = value;
		}
	}

	public double SpanClipH
	{
		get
		{
			return span_clip_h;
		}
		set
		{
			span_clip_h = value;
		}
	}

	public int Pixels
	{
		get
		{
			return pixels;
		}
		set
		{
			pixels = value;
			if (update)
			{
				initAnalyzer();
			}
		}
	}

	public int Stitches
	{
		get
		{
			return stitches;
		}
		set
		{
			stitches = value;
		}
	}

	public int CalibrationDataSet
	{
		get
		{
			return calibration_data_set;
		}
		set
		{
			calibration_data_set = value;
		}
	}

	public double SpanMinFreq
	{
		get
		{
			return span_min_freq;
		}
		set
		{
			span_min_freq = value;
		}
	}

	public double SpanMaxFreq
	{
		get
		{
			return span_max_freq;
		}
		set
		{
			span_max_freq = value;
		}
	}

	public bool AverageOn
	{
		get
		{
			return average_on;
		}
		set
		{
			average_on = value;
			int mode;
			int mode2;
			if (peak_on)
			{
				mode = (mode2 = -1);
			}
			else if (average_on)
			{
				mode = av_mode;
				mode2 = av_mode_wf;
			}
			else
			{
				mode = (mode2 = 0);
			}
			SpecHPSDRDLL.SetDisplayAverageMode(disp, 0, mode);
			SpecHPSDRDLL.SetDisplayAverageMode(disp, 1, mode2);
		}
	}

	public bool PeakOn
	{
		get
		{
			return peak_on;
		}
		set
		{
			peak_on = value;
			int mode;
			int mode2;
			if (peak_on)
			{
				mode = (mode2 = -1);
			}
			else if (average_on)
			{
				mode = av_mode;
				mode2 = av_mode_wf;
			}
			else
			{
				mode = (mode2 = 0);
			}
			SpecHPSDRDLL.SetDisplayAverageMode(disp, 0, mode);
			SpecHPSDRDLL.SetDisplayAverageMode(disp, 1, mode2);
		}
	}

	public double DisplayENB => SpecHPSDRDLL.GetDisplayENB(disp);

	public int DetTypePan
	{
		get
		{
			return det_type_pan;
		}
		set
		{
			det_type_pan = value;
			SpecHPSDRDLL.SetDisplayDetectorMode(disp, 0, value);
			updateNormalizePan();
		}
	}

	public int DetTypeWF
	{
		get
		{
			return det_type_wf;
		}
		set
		{
			det_type_wf = value;
			SpecHPSDRDLL.SetDisplayDetectorMode(disp, 1, value);
		}
	}

	public bool NormOneHzPan
	{
		get
		{
			return norm_oneHz_pan;
		}
		set
		{
			norm_oneHz_pan = value;
			updateNormalizePan();
		}
	}

	public int FrameRate
	{
		get
		{
			return frame_rate;
		}
		set
		{
			frame_rate = value;
			if (update)
			{
				initAnalyzer();
			}
		}
	}

	public double AvTau
	{
		get
		{
			return tau;
		}
		set
		{
			tau = value;
			double mult = Math.Exp(-1.0 / ((double)frame_rate * tau));
			int num = Math.Max(2, (int)Math.Min(60.0, (double)frame_rate * tau));
			SpecHPSDRDLL.SetDisplayAvBackmult(disp, 0, mult);
			SpecHPSDRDLL.SetDisplayNumAverage(disp, 0, num);
		}
	}

	public double AvTauWF
	{
		get
		{
			return tau_wf;
		}
		set
		{
			tau_wf = value;
			double mult = Math.Exp(-1.0 / ((double)frame_rate * tau_wf));
			int num = Math.Max(2, (int)Math.Min(60.0, (double)frame_rate * tau_wf));
			SpecHPSDRDLL.SetDisplayAvBackmult(disp, 1, mult);
			SpecHPSDRDLL.SetDisplayNumAverage(disp, 1, num);
		}
	}

	public int AverageMode
	{
		get
		{
			return av_mode;
		}
		set
		{
			av_mode = value;
			int mode = (peak_on ? (-1) : (average_on ? av_mode : 0));
			if (update)
			{
				SpecHPSDRDLL.SetDisplayAverageMode(disp, 0, mode);
			}
		}
	}

	public int AverageModeWF
	{
		get
		{
			return av_mode_wf;
		}
		set
		{
			av_mode_wf = value;
			int mode = (peak_on ? (-1) : (average_on ? av_mode_wf : 0));
			if (update)
			{
				SpecHPSDRDLL.SetDisplayAverageMode(disp, 1, mode);
			}
		}
	}

	public double ZoomSlider
	{
		get
		{
			return z_slider;
		}
		set
		{
			z_slider = value;
			if (update)
			{
				initAnalyzer();
			}
		}
	}

	public double PanSlider
	{
		get
		{
			return pan_slider;
		}
		set
		{
			pan_slider = value;
			if (update)
			{
				initAnalyzer();
			}
		}
	}

	public int SampleRate
	{
		get
		{
			return sample_rate;
		}
		set
		{
			sample_rate = value;
			if (update)
			{
				initAnalyzer();
			}
			SpecHPSDRDLL.SetDisplaySampleRate(disp, sample_rate);
		}
	}

	public bool NBOn
	{
		get
		{
			return nb_on;
		}
		set
		{
			nb_on = value;
		}
	}

	public bool NB2On
	{
		get
		{
			return nb2_on;
		}
		set
		{
			nb2_on = value;
		}
	}

	public int PixelOut
	{
		get
		{
			return _pixel_out;
		}
		set
		{
			_pixel_out = value;
			if (update)
			{
				initAnalyzer();
			}
		}
	}

	public bool IgnoreFrequencyOffset
	{
		get
		{
			return _ignore_frequency_offset;
		}
		set
		{
			_ignore_frequency_offset = value;
		}
	}

	public int LowFreq => _low_freq;

	public int HighFreq => _high_freq;

	public SpecHPSDR(int d)
	{
		disp = d;
	}

	private void updateNormalizePan()
	{
		if (norm_oneHz_pan && (det_type_pan == 2 || det_type_pan == 3 || det_type_pan == 4))
		{
			SpecHPSDRDLL.SetDisplayNormOneHz(disp, 0, norm: true);
		}
		else
		{
			SpecHPSDRDLL.SetDisplayNormOneHz(disp, 0, norm: false);
		}
	}

	public void resetPixelBuffers()
	{
		SpecHPSDRDLL.ResetPixelBuffers(disp);
	}

	public void initAnalyzer()
	{
		IntPtr flp = GCHandle.Alloc(new int[1], GCHandleType.Pinned).AddrOfPinnedObject();
		_low_freq = 0;
		_high_freq = 0;
		double num = 0.0;
		int num2 = data_type;
		if (num2 != 0 && num2 == 1)
		{
			overlap = (int)Math.Max(0.0, Math.Ceiling((double)fft_size - (double)sample_rate / (double)frame_rate));
			clip = (int)Math.Floor(0.04 * (double)fft_size);
			double num3 = (double)sample_rate / (double)fft_size;
			int num4 = fft_size - 1 - 2 * clip;
			num = (double)num4 * num3;
			double num5 = stitches * num4 - 1;
			double num6 = Math.Log10(9.0 * z_slider + 1.0);
			double num7 = num5 * (1.0 - 0.99 * num6);
			span_clip_l = pan_slider * (num5 - num7);
			span_clip_h = num5 - num7 - span_clip_l;
			_low_freq = -(int)((num5 / 2.0 - span_clip_l) * num3);
			_high_freq = (int)((num5 / 2.0 - span_clip_h) * num3);
			max_w = fft_size + (int)Math.Min(0.1 * (double)sample_rate, 0.1 * (double)fft_size * (double)frame_rate);
		}
		switch (disp)
		{
		case 0:
			Display.RXDisplayLow = _low_freq;
			Display.RXDisplayHigh = _high_freq;
			break;
		case 1:
			Display.RX2DisplayLow = _low_freq;
			Display.RX2DisplayHigh = _high_freq;
			break;
		case 2:
		case 3:
		case 4:
		case 5:
			Display.TXDisplayLow = _low_freq;
			Display.TXDisplayHigh = _high_freq;
			break;
		}
		if (!_ignore_frequency_offset)
		{
			NetworkIO.LowFreqOffset = num;
			NetworkIO.HighFreqOffset = num;
		}
		if (disp != 0 || Display.CurrentDisplayMode == DisplayMode.PANADAPTER || Display.CurrentDisplayMode == DisplayMode.WATERFALL || Display.CurrentDisplayMode == DisplayMode.PANAFALL || Display.CurrentDisplayMode == DisplayMode.PANASCOPE)
		{
			SpecHPSDRDLL.SetAnalyzer(disp, _pixel_out, spur_eliminationtion_ffts, data_type, flp, fft_size, blocksize, window_type, kaiser_pi, overlap, clip, span_clip_l, span_clip_h, pixels, stitches, calibration_data_set, span_min_freq, span_max_freq, max_w);
		}
	}

	public void ZoomToBandwidth(double target_bandwidth_hz)
	{
		double num = (double)sample_rate / (double)fft_size;
		int num2 = fft_size - 1 - 2 * (int)Math.Floor(0.04 * (double)fft_size);
		double num3 = stitches * num2 - 1;
		double num4 = target_bandwidth_hz / num;
		double num5 = 1.0 - num4 / num3;
		double val = (Math.Pow(10.0, num5 / 0.99) - 1.0) / 9.0;
		double num6 = Math.Max(0.0, Math.Min(1.0, val));
		bool flag = Update;
		if (num6 != ZoomSlider)
		{
			Update = false;
			ZoomSlider = num6;
		}
		Update = flag;
		PanSlider = 0.5;
	}

	public (int, int) GetFrequencyExtents(double zslider, double panslider)
	{
		int item = 0;
		int item2 = 0;
		int num = data_type;
		if (num != 0 && num == 1)
		{
			int num2 = (int)Math.Floor(0.04 * (double)fft_size);
			double num3 = (double)sample_rate / (double)fft_size;
			int num4 = fft_size - 1 - 2 * num2;
			double num5 = stitches * num4 - 1;
			double num6 = Math.Log10(9.0 * zslider + 1.0);
			double num7 = num5 * (1.0 - 0.99 * num6);
			double num8 = panslider * (num5 - num7);
			double num9 = num5 - num7 - num8;
			item = -(int)((num5 / 2.0 - num8) * num3);
			item2 = (int)((num5 / 2.0 - num9) * num3);
		}
		return (item, item2);
	}

	public void CalcSpectrum(int filter_low, int filter_high, int spec_blocksize, int sample_rate)
	{
		IntPtr flp = GCHandle.Alloc(new int[1], GCHandleType.Pinned).AddrOfPinnedObject();
		double num = 0.5 * (double)sample_rate - (double)filter_high;
		double num2 = 0.5 * (double)sample_rate + (double)filter_low;
		double num3 = (double)sample_rate / (double)fft_size;
		int num4 = (int)Math.Floor(num / num3);
		int num5 = (int)Math.Ceiling(num2 / num3);
		int clp = 0;
		int n_stch = 1;
		max_w = fft_size + (int)Math.Min(0.1 * (double)sample_rate, 0.1 * (double)fft_size * (double)frame_rate);
		overlap = (int)Math.Max(0.0, Math.Ceiling((double)fft_size - (double)sample_rate / (double)frame_rate));
		SpecHPSDRDLL.SetAnalyzer(disp, _pixel_out, spur_eliminationtion_ffts, data_type, flp, fft_size, spec_blocksize, window_type, kaiser_pi, overlap, clp, num5, num4, pixels, n_stch, calibration_data_set, span_min_freq, span_max_freq, max_w);
	}
}
