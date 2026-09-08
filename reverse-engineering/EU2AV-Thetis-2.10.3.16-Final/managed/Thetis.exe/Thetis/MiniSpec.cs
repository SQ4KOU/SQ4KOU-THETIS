using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Thetis;

public static class MiniSpec
{
	public class FilterCharacteristics
	{
		public double[] segments;

		public int index_low;

		public int index_upper;

		public double corner_freq;

		public int hz_span;

		public int middle_index;

		public int six_db_shift;

		public double min;

		public double max;
	}

	public class Notch
	{
		public int index;

		public double frequency_hz;

		public double width_hz;

		public bool active;
	}

	public class clsMiniSpec
	{
		private int _rx;

		private int _id;

		private int _disp;

		private bool _enabled;

		private int _pixels;

		private int _hwsample_rate;

		private int _frame_rate;

		private double _rx_frequency;

		private double _tx_frequency;

		private float[] _new_display_data;

		private float[] _new_display_data_raw;

		private float[] _current_display_data;

		private float[] _current_display_data_raw;

		private bool _display_running;

		private Thread _display_thread;

		private bool _pause_display;

		private bool _new_data_available;

		private SpecHPSDR _spec;

		private bool _mox;

		private int _data_index;

		private double _min_notch_width;

		private double _centre_freq;

		private int _max_filter_width;

		private bool _avg_on;

		private int _cw_pitch;

		private DSPMode _mode;

		private bool _ctun;

		private bool _sub_receiver;

		private readonly object _data_lock = new object();

		private readonly object _new_data_lock = new object();

		private readonly Stopwatch _pan_stopwatch;

		private Timer _pan_final_timer;

		private readonly object _pan_lock = new object();

		internal bool Enable
		{
			set
			{
				lock (_new_data_lock)
				{
					if (value != _enabled)
					{
						_enabled = value;
						int run = (_enabled ? 1 : 0);
						cmaster.RunAnalyzer(_disp, run);
					}
				}
			}
		}

		public bool SubReceiver => _sub_receiver;

		public DSPMode Mode
		{
			set
			{
				_mode = value;
			}
		}

		public bool Ctun
		{
			set
			{
				_ctun = value;
			}
		}

		public int CWPitchOffset
		{
			get
			{
				int result = 0;
				switch (_mode)
				{
				case DSPMode.CWL:
					result = _cw_pitch;
					break;
				case DSPMode.CWU:
					result = -_cw_pitch;
					break;
				}
				return result;
			}
			set
			{
				_cw_pitch = value;
			}
		}

		public bool AVGOn
		{
			set
			{
				if (value != _avg_on)
				{
					_avg_on = value;
					_spec.AverageOn = _avg_on;
				}
			}
		}

		public int MaxFilterWidth
		{
			set
			{
				if (value != _max_filter_width)
				{
					_max_filter_width = value;
					zoom();
					lock (_pan_lock)
					{
						setPan();
					}
				}
			}
		}

		public int RX => _rx;

		public bool MOX
		{
			set
			{
				if (value != _mox)
				{
					_mox = value;
					lock (_new_data_lock)
					{
						UpdateSpecSettings();
						resetBuffers();
					}
				}
			}
		}

		public double MinNotchWidth
		{
			get
			{
				return _min_notch_width;
			}
			set
			{
				_min_notch_width = value;
			}
		}

		public int HWSampleRate
		{
			get
			{
				return _hwsample_rate;
			}
			set
			{
				UpdateSpecSettings();
			}
		}

		public int SpecLowFreq
		{
			get
			{
				if (_spec == null)
				{
					return 0;
				}
				return _spec.LowFreq;
			}
		}

		public int SpecHighFreq
		{
			get
			{
				if (_spec == null)
				{
					return 0;
				}
				return _spec.HighFreq;
			}
		}

		public int DataIndex
		{
			get
			{
				lock (_data_lock)
				{
					return _data_index;
				}
			}
		}

		public unsafe float[] Data
		{
			get
			{
				lock (_new_data_lock)
				{
					if (_new_data_available)
					{
						lock (_data_lock)
						{
							fixed (float* ptr = &_new_display_data[0])
							{
								void* srcptr = ptr;
								fixed (float* ptr2 = &_current_display_data[0])
								{
									void* destptr = ptr2;
									Win32.memcpy(destptr, srcptr, _current_display_data.Length * 4);
								}
							}
							fixed (float* ptr = &_new_display_data_raw[0])
							{
								void* srcptr2 = ptr;
								fixed (float* ptr2 = &_current_display_data_raw[0])
								{
									void* destptr2 = ptr2;
									Win32.memcpy(destptr2, srcptr2, _current_display_data.Length * 4);
								}
							}
						}
						_new_data_available = false;
					}
				}
				lock (_data_lock)
				{
					return _current_display_data;
				}
			}
		}

		public float[] DataRaw
		{
			get
			{
				lock (_data_lock)
				{
					return _current_display_data_raw;
				}
			}
		}

		public double RXFrequency
		{
			get
			{
				return _rx_frequency;
			}
			set
			{
				double num = Math.Round(value, 6);
				if (_rx_frequency != num)
				{
					_rx_frequency = num;
					if (!_mox)
					{
						rateLimitSetPan();
					}
				}
			}
		}

		public double TXFrequency
		{
			get
			{
				return _tx_frequency;
			}
			set
			{
				double num = Math.Round(value, 6);
				if (_tx_frequency != num)
				{
					_tx_frequency = num;
					if (_mox)
					{
						rateLimitSetPan();
					}
				}
			}
		}

		public int Pixels
		{
			get
			{
				return _pixels;
			}
			set
			{
				_pause_display = true;
				_new_data_available = false;
				_pixels = value;
				_new_display_data = new float[_pixels];
				_new_display_data_raw = new float[_pixels];
				_current_display_data = new float[_pixels];
				_current_display_data_raw = new float[_pixels];
				for (int i = 0; i < _pixels; i++)
				{
					_new_display_data[i] = -200f;
					_new_display_data_raw[i] = -200f;
					_current_display_data[i] = -200f;
					_current_display_data_raw[i] = -200f;
				}
				_spec.Pixels = _pixels;
				_pause_display = false;
			}
		}

		public double CentreFreq
		{
			set
			{
				double num = Math.Round(value, 6);
				if (num != _centre_freq)
				{
					_centre_freq = num;
					rateLimitSetPan();
				}
			}
		}

		public clsMiniSpec(int rx, int id, bool sub_receiver, Console console)
		{
			_pan_stopwatch = Stopwatch.StartNew();
			_enabled = UsingAFilter(id, sub_receiver);
			_hwsample_rate = 0;
			_rx = rx;
			_sub_receiver = sub_receiver;
			_mox = console.MOX;
			_max_filter_width = _console.MaxFilterWidth;
			_mode = ((_rx == 1) ? _console.RX1DSPMode : _console.RX2DSPMode);
			_cw_pitch = _console.CWPitch;
			_ctun = ((_rx == 1) ? _console.ClickTuneDisplay : _console.ClickTuneRX2Display);
			_centre_freq = ((_rx == 1) ? _console.CentreFrequency : _console.CentreRX2Frequency);
			_rx_frequency = ((_rx == 1) ? _console.VFOAFreq : _console.VFOBFreq);
			_tx_frequency = _console.TXFreq;
			_id = id;
			_disp = cmaster.AllocAnalyzer(0, _id, 262144);
			if (!_enabled)
			{
				cmaster.RunAnalyzer(_disp, 0);
			}
			_new_data_available = false;
			_data_index = 0;
			_min_notch_width = console.GetMinimumRXNotchWidth(_rx);
			_pixels = 1024;
			_frame_rate = 30;
			_new_display_data = new float[_pixels];
			_new_display_data_raw = new float[_pixels];
			_current_display_data = new float[_pixels];
			_current_display_data_raw = new float[_pixels];
			for (int i = 0; i < _pixels; i++)
			{
				_new_display_data[i] = -200f;
				_new_display_data_raw[i] = -200f;
				_current_display_data[i] = -200f;
				_current_display_data_raw[i] = -200f;
			}
			_spec = new SpecHPSDR(_disp);
			_spec.Update = false;
			_spec.FrameRate = _frame_rate;
			_spec.PixelOut = 1;
			_spec.IgnoreFrequencyOffset = true;
			_spec.Pixels = _pixels;
			UpdateSpecSettings();
			_avg_on = _spec.AverageOn;
			_pause_display = false;
			_display_running = true;
			_display_thread = new Thread(runDisplay)
			{
				Name = "MiniRX Display Thread",
				Priority = ThreadPriority.Lowest,
				IsBackground = true
			};
			_display_thread.Start();
		}

		private void setupSpecDetails()
		{
			SpecHPSDR specRX = _console.specRX.GetSpecRX(_id);
			_spec.Update = false;
			_spec.DetTypePan = specRX.DetTypePan;
			_spec.AvTau = specRX.AvTau;
			_spec.FFTSize = specRX.FFTSize;
			_spec.BlockSize = specRX.BlockSize;
			_spec.SampleRate = specRX.SampleRate;
			_spec.WindowType = specRX.WindowType;
			_spec.AverageMode = specRX.AverageMode;
			_spec.AverageOn = specRX.AverageOn;
			_spec.NormOneHzPan = specRX.NormOneHzPan;
			_spec.Update = true;
			_spec.initAnalyzer();
			_hwsample_rate = _spec.SampleRate;
		}

		private void resetBuffers()
		{
			lock (_new_data_lock)
			{
				_spec.resetPixelBuffers();
				for (int i = 0; i < _pixels; i++)
				{
					_new_display_data[i] = -200f;
					_new_display_data_raw[i] = -200f;
				}
			}
		}

		public void ClearData()
		{
			lock (_data_lock)
			{
				for (int i = 0; i < _current_display_data.Length; i++)
				{
					_current_display_data[i] = -200f;
					_current_display_data_raw[i] = -200f;
				}
			}
		}

		private unsafe void runDisplay()
		{
			while (_display_running)
			{
				lock (_new_data_lock)
				{
					if (_enabled && !_pause_display && !_new_data_available)
					{
						int flag = 0;
						fixed (float* pix = &_new_display_data[0])
						{
							SpecHPSDRDLL.GetPixels(_disp, 0, pix, ref flag);
						}
						fixed (float* ptr = &_new_display_data[0])
						{
							void* srcptr = ptr;
							fixed (float* ptr2 = &_new_display_data_raw[0])
							{
								void* destptr = ptr2;
								Win32.memcpy(destptr, srcptr, _new_display_data.Length * 4);
							}
						}
						if (flag == 1)
						{
							float num = 0f;
							switch (_rx)
							{
							case 1:
								num = Display.RX1OffsetWithDup;
								break;
							case 2:
								num = Display.RX2OffsetWithDup;
								break;
							}
							for (int i = 0; i < _new_display_data.Length; i++)
							{
								_new_display_data[i] += num;
								_new_display_data_raw[i] += num;
							}
							if (!_mox)
							{
								lock (_notch_locker)
								{
									if (_visual_notch_display)
									{
										int num2 = _spec.HighFreq - _spec.LowFreq;
										float num3 = (float)_pixels / (float)num2;
										foreach (Notch notch in GetNotches(_rx_frequency * 1000000.0, _hwsample_rate / 2))
										{
											if (_tnf && notch.active)
											{
												double num4 = notch.frequency_hz - _rx_frequency * 1000000.0 - (double)CWPitchOffset;
												int num5 = (int)((double)(_pixels / 2) + num4 * (double)num3);
												if (num5 >= 0 && num5 < _new_display_data.Length)
												{
													attenuateData(num5, 200f, (int)(Math.Max(_min_notch_width, notch.width_hz) * (double)num3));
												}
											}
										}
									}
								}
							}
							_new_data_available = true;
							_data_index++;
							if (_data_index > 1800)
							{
								_data_index = 0;
							}
						}
					}
				}
				if (_enabled)
				{
					Thread.Sleep(1000 / _frame_rate);
				}
				else
				{
					Thread.Sleep(200);
				}
			}
		}

		private void attenuateData(int center_index, float attenuation, int span_in_pixels)
		{
			span_in_pixels = Math.Max(2, span_in_pixels);
			int num = span_in_pixels / 2;
			for (int i = -num; i <= num; i++)
			{
				int num2 = center_index + i;
				if (num2 >= 0 && num2 < _new_display_data.Length)
				{
					int val = span_in_pixels * (num - Math.Abs(i)) / num;
					float num3 = 1f / (float)Math.Pow((double)span_in_pixels / (double)Math.Max(1, val), 1.0);
					_new_display_data[num2] -= attenuation * num3;
				}
			}
		}

		public void UpdateSpecSettings()
		{
			setupSpecDetails();
			zoom();
			lock (_pan_lock)
			{
				setPan();
			}
		}

		public void Shutdown()
		{
			_display_running = false;
			if (_display_thread != null && _display_thread.IsAlive)
			{
				_display_thread.Join(1000 / _frame_rate + 100);
			}
			cmaster.FreeAnalyzer(_disp);
		}

		public void rateLimitSetPan()
		{
			lock (_pan_lock)
			{
				if (_pan_stopwatch.ElapsedMilliseconds >= 333)
				{
					updatePan(null);
				}
				else if (_pan_final_timer == null)
				{
					long num = 333 - _pan_stopwatch.ElapsedMilliseconds;
					if (num <= 0)
					{
						updatePan(null);
					}
					else
					{
						_pan_final_timer = new Timer(updatePan, null, num, -1L);
					}
				}
			}
		}

		private void updatePan(object _)
		{
			lock (_pan_lock)
			{
				Timer timer = null;
				try
				{
					_pan_stopwatch.Restart();
					setPan();
					timer = _pan_final_timer;
					_pan_final_timer = null;
				}
				catch
				{
				}
				finally
				{
					timer?.Dispose();
				}
			}
		}

		private void setPan()
		{
			if (!_mox && !_ctun)
			{
				_spec.PanSlider = 0.5;
				return;
			}
			int num = (int)(_centre_freq * 1000000.0);
			int num2 = ((!_mox) ? ((int)(_rx_frequency * 1000000.0)) : ((int)(_tx_frequency * 1000000.0)));
			(int, int) frequencyExtents = _spec.GetFrequencyExtents(0.0, 0.5);
			int item = frequencyExtents.Item1;
			int num3 = frequencyExtents.Item2 - item;
			int num4 = num - num3 / 2;
			int num5 = num + num3 / 2;
			if (num2 >= num4 && num2 <= num5)
			{
				num4 += (_mox ? 20000 : _max_filter_width);
				num5 -= (_mox ? 20000 : _max_filter_width);
				num3 = num5 - num4;
				double panSlider = (double)(num2 - num4) / (double)num3;
				_spec.PanSlider = panSlider;
			}
		}

		private void zoom()
		{
			double num = (_mox ? 20000 : _max_filter_width);
			_spec.ZoomToBandwidth(num * 2.0);
		}
	}

	public const int TX_BANDWIDTH = 20000;

	public const int PIXELS = 1024;

	public const int FRAME_RATE = 30;

	public const int PAN_RATE_LIMIT_MS = 333;

	public const int SUB_RX_OFFSET = 1024;

	private static Dictionary<int, clsMiniSpec> _mini_spec;

	private static Console _console;

	private static List<Notch> _notches;

	private static readonly object _notch_locker;

	private static bool _visual_notch_display;

	private static bool _tnf;

	private static bool _mox;

	private static Dictionary<int, int> _using_filter_count;

	private static readonly object _filter_characteristics_lock;

	private static FilterCharacteristics[] _rx_filter_characteristics;

	private static FilterCharacteristics _tx_filter_characteristics;

	public static object FilterCharacteristicsLocker => _filter_characteristics_lock;

	public static bool ShowVisualNotch
	{
		get
		{
			lock (_notch_locker)
			{
				return _visual_notch_display;
			}
		}
		set
		{
			lock (_notch_locker)
			{
				_visual_notch_display = value;
			}
		}
	}

	public static bool TNFActive
	{
		get
		{
			lock (_notch_locker)
			{
				return _tnf;
			}
		}
		set
		{
			lock (_notch_locker)
			{
				_tnf = value;
			}
		}
	}

	public static int MaxPixels
	{
		set
		{
			foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec)
			{
				item.Value.Pixels = value;
			}
		}
	}

	public static object NotchLocker => _notch_locker;

	static MiniSpec()
	{
		_notch_locker = new object();
		_filter_characteristics_lock = new object();
		_mini_spec = new Dictionary<int, clsMiniSpec>();
		_notches = new List<Notch>();
		_tnf = false;
		_mox = false;
		_visual_notch_display = false;
		_using_filter_count = new Dictionary<int, int>();
		_rx_filter_characteristics = new FilterCharacteristics[2];
		for (int i = 0; i < _rx_filter_characteristics.Length; i++)
		{
			_rx_filter_characteristics[i] = new FilterCharacteristics();
		}
		_tx_filter_characteristics = new FilterCharacteristics();
	}

	public static void Init(Console console)
	{
		_console = console;
		if (_console == null)
		{
			return;
		}
		_mox = _console.MOX;
		_tnf = _console.TNFActive;
		_visual_notch_display = Display.ShowVisualNotch;
		Console console2 = _console;
		console2.MoxPreChangeHandlers = (Console.MoxPreChanged)Delegate.Combine(console2.MoxPreChangeHandlers, new Console.MoxPreChanged(OnMox));
		Console console3 = _console;
		console3.MoxChangeHandlers = (Console.MoxChanged)Delegate.Combine(console3.MoxChangeHandlers, new Console.MoxChanged(OnMox));
		Console console4 = _console;
		console4.NotchChangedHandlers = (Console.NotchChanged)Delegate.Combine(console4.NotchChangedHandlers, new Console.NotchChanged(OnNotchChanged));
		Console console5 = _console;
		console5.TNFChangedHandlers = (Console.TNFChanged)Delegate.Combine(console5.TNFChangedHandlers, new Console.TNFChanged(OnTNFChanged));
		Console console6 = _console;
		console6.MinimumRXNotchWidthChangedHandlers = (Console.MinimumRXNotchWidthChanged)Delegate.Combine(console6.MinimumRXNotchWidthChangedHandlers, new Console.MinimumRXNotchWidthChanged(OnMinNotchWidth));
		Console console7 = _console;
		console7.CentreFrequencyHandlers = (Console.CentreFrequencyChanged)Delegate.Combine(console7.CentreFrequencyHandlers, new Console.CentreFrequencyChanged(OnCentreFrequency));
		Console console8 = _console;
		console8.FilterEdgesChangedHandlers = (Console.FilterEdgesChanged)Delegate.Combine(console8.FilterEdgesChangedHandlers, new Console.FilterEdgesChanged(OnFilterEdgesChanged));
		Console console9 = _console;
		console9.HWSampleRateChangedHandlers = (Console.HWSampleRateChanged)Delegate.Combine(console9.HWSampleRateChangedHandlers, new Console.HWSampleRateChanged(OnHWSampleRateChanged));
		Console console10 = _console;
		console10.SpectrumSettingsChangedHandlers = (Console.SpectrumSettingsChanged)Delegate.Combine(console10.SpectrumSettingsChangedHandlers, new Console.SpectrumSettingsChanged(OnSpectrumSettingsChanged));
		Console console11 = _console;
		console11.AVGChangedHandlers = (Console.AVGOnChanged)Delegate.Combine(console11.AVGChangedHandlers, new Console.AVGOnChanged(OnAVGChanged));
		Console console12 = _console;
		console12.NotifiySpectrumDetailsChangedHandlers = (Console.NotifiySpectrumDetailsChanged)Delegate.Combine(console12.NotifiySpectrumDetailsChangedHandlers, new Console.NotifiySpectrumDetailsChanged(OnSpectrumDetailsChanged));
		Console console13 = _console;
		console13.CWPitchChangedHandlers = (Console.CWPitchChanged)Delegate.Combine(console13.CWPitchChangedHandlers, new Console.CWPitchChanged(OnCWPitchChanged));
		Console console14 = _console;
		console14.ModeChangeHandlers = (Console.ModeChanged)Delegate.Combine(console14.ModeChangeHandlers, new Console.ModeChanged(OnModeChanged));
		Console console15 = _console;
		console15.PowerChangeHanders = (Console.PowerChanged)Delegate.Combine(console15.PowerChangeHanders, new Console.PowerChanged(OnPowerChanged));
		Console console16 = _console;
		console16.CTUNChangedHandlers = (Console.CTUNChanged)Delegate.Combine(console16.CTUNChangedHandlers, new Console.CTUNChanged(OnCTUNChanged));
		lock (_notch_locker)
		{
			_notches.Clear();
			double num = _console.MaxFreq * 1000000.0 / 2.0;
			int num2 = 0;
			foreach (MNotch item2 in MNotchDB.NotchesInBW(num, (int)(0.0 - num), (int)num))
			{
				Notch item = new Notch
				{
					index = num2,
					active = item2.Active,
					frequency_hz = item2.FCenter,
					width_hz = item2.FWidth
				};
				_notches.Add(item);
				num2++;
			}
		}
	}

	public static void UpdateRXFilterCharacteristics(DSPMode mode, double[] segments, int index_low, int index_upper, double corner_freq)
	{
		lock (_filter_characteristics_lock)
		{
			int num = ((mode == DSPMode.FM) ? 1 : 0);
			_rx_filter_characteristics[num].segments = (double[])segments.Clone();
			_rx_filter_characteristics[num].index_low = index_low;
			_rx_filter_characteristics[num].index_upper = index_upper;
			_rx_filter_characteristics[num].corner_freq = corner_freq;
			_rx_filter_characteristics[num].hz_span = index_upper - index_low;
			_rx_filter_characteristics[num].middle_index = index_low + (index_upper - index_low) / 2;
			_rx_filter_characteristics[num].six_db_shift = _rx_filter_characteristics[num].middle_index - index_low;
			_rx_filter_characteristics[num].min = double.MaxValue;
			_rx_filter_characteristics[num].max = double.MinValue;
			for (int i = 1; i < _rx_filter_characteristics[num].segments.Length - 1; i++)
			{
				if (_rx_filter_characteristics[num].segments[i] < _rx_filter_characteristics[num].min)
				{
					_rx_filter_characteristics[num].min = _rx_filter_characteristics[num].segments[i];
				}
				if (_rx_filter_characteristics[num].segments[i] > _rx_filter_characteristics[num].max)
				{
					_rx_filter_characteristics[num].max = _rx_filter_characteristics[num].segments[i];
				}
			}
		}
	}

	public static void UpdateTXFilterCharacteristics(double[] segments, int index_low, int index_upper, double corner_freq)
	{
		lock (_filter_characteristics_lock)
		{
			_tx_filter_characteristics.segments = (double[])segments.Clone();
			_tx_filter_characteristics.index_low = index_low;
			_tx_filter_characteristics.index_upper = index_upper;
			_tx_filter_characteristics.corner_freq = corner_freq;
			_tx_filter_characteristics.hz_span = index_upper - index_low;
			_tx_filter_characteristics.middle_index = index_low + (index_upper - index_low) / 2;
			_tx_filter_characteristics.six_db_shift = _tx_filter_characteristics.middle_index - index_low;
			_tx_filter_characteristics.min = double.MaxValue;
			_tx_filter_characteristics.max = double.MinValue;
			for (int i = 1; i < _tx_filter_characteristics.segments.Length - 1; i++)
			{
				if (_tx_filter_characteristics.segments[i] < _tx_filter_characteristics.min)
				{
					_tx_filter_characteristics.min = _tx_filter_characteristics.segments[i];
				}
				if (_tx_filter_characteristics.segments[i] > _tx_filter_characteristics.max)
				{
					_tx_filter_characteristics.max = _tx_filter_characteristics.segments[i];
				}
			}
		}
	}

	public static FilterCharacteristics GetRXCharacteristic(DSPMode mode)
	{
		lock (_filter_characteristics_lock)
		{
			int num = ((mode == DSPMode.FM) ? 1 : 0);
			return _rx_filter_characteristics[num];
		}
	}

	public static FilterCharacteristics GetTXCharacteristic()
	{
		lock (_filter_characteristics_lock)
		{
			return _tx_filter_characteristics;
		}
	}

	public static bool UsingAFilter(int id, bool sub_receiver = false)
	{
		int key = (sub_receiver ? (id + 1024) : id);
		if (!_using_filter_count.ContainsKey(key))
		{
			return false;
		}
		return _using_filter_count[key] > 0;
	}

	public static void UsingFilter(int id, bool sub_receiver = false)
	{
		int key = (sub_receiver ? (id + 1024) : id);
		if (_using_filter_count.ContainsKey(key))
		{
			int num = _using_filter_count[key];
			num++;
			_using_filter_count[key] = num;
		}
		else
		{
			_using_filter_count.Add(key, 1);
		}
		if (_using_filter_count[key] <= 0)
		{
			return;
		}
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Key == key))
		{
			item.Value.Enable = true;
		}
	}

	public static void StopUsingFilter(int id, bool sub_receiver = false)
	{
		int key = (sub_receiver ? (id + 1024) : id);
		if (!_using_filter_count.ContainsKey(key))
		{
			return;
		}
		int num = _using_filter_count[key];
		num--;
		_using_filter_count[key] = num;
		if (_using_filter_count[key] > 0)
		{
			return;
		}
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Key == key))
		{
			item.Value.Enable = false;
		}
	}

	private static void OnSpectrumDetailsChanged(int rx)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.UpdateSpecSettings();
		}
	}

	private static void OnAVGChanged(int rx, bool old_state, bool new_state)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.AVGOn = new_state;
		}
	}

	private static void OnFilterEdgesChanged(int rx, Filter filter, Band band, int low, int high, string sName, int max_width, int max_shift)
	{
		if (max_width == -1)
		{
			return;
		}
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.MaxFilterWidth = max_width;
		}
	}

	private static void OnMinNotchWidth(int rx, double width)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.MinNotchWidth = width;
		}
	}

	private static void OnTNFChanged(bool old_tnf, bool new_tnf)
	{
		lock (_notch_locker)
		{
			_tnf = new_tnf;
		}
	}

	private static void OnMox(int rx, bool oldMox, bool newMox)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.MOX = newMox;
		}
	}

	public static void Add(int rx, int id, bool sub_receiver = false)
	{
		int key = (sub_receiver ? (id + 1024) : id);
		if (_mini_spec.ContainsKey(key))
		{
			_mini_spec[key].Shutdown();
			_mini_spec.Remove(key);
		}
		clsMiniSpec value = new clsMiniSpec(rx, id, sub_receiver, _console);
		_mini_spec.Add(key, value);
	}

	public static clsMiniSpec GetMiniRX(int id, bool sub_receiver = false)
	{
		int key = (sub_receiver ? (id + 1024) : id);
		if (!_mini_spec.ContainsKey(key))
		{
			return null;
		}
		if (sub_receiver)
		{
			if (_mini_spec[key].SubReceiver)
			{
				return _mini_spec[key];
			}
			return null;
		}
		return _mini_spec[key];
	}

	private static void shutdownRX(int key)
	{
		if (_mini_spec.ContainsKey(key))
		{
			_mini_spec[key].Shutdown();
			_mini_spec.Remove(key);
		}
	}

	public static void ShutdownAllRX()
	{
		if (_console != null)
		{
			Console console = _console;
			console.MoxPreChangeHandlers = (Console.MoxPreChanged)Delegate.Remove(console.MoxPreChangeHandlers, new Console.MoxPreChanged(OnMox));
			Console console2 = _console;
			console2.MoxChangeHandlers = (Console.MoxChanged)Delegate.Remove(console2.MoxChangeHandlers, new Console.MoxChanged(OnMox));
			Console console3 = _console;
			console3.NotchChangedHandlers = (Console.NotchChanged)Delegate.Remove(console3.NotchChangedHandlers, new Console.NotchChanged(OnNotchChanged));
			Console console4 = _console;
			console4.TNFChangedHandlers = (Console.TNFChanged)Delegate.Remove(console4.TNFChangedHandlers, new Console.TNFChanged(OnTNFChanged));
			Console console5 = _console;
			console5.MinimumRXNotchWidthChangedHandlers = (Console.MinimumRXNotchWidthChanged)Delegate.Remove(console5.MinimumRXNotchWidthChangedHandlers, new Console.MinimumRXNotchWidthChanged(OnMinNotchWidth));
			Console console6 = _console;
			console6.CentreFrequencyHandlers = (Console.CentreFrequencyChanged)Delegate.Remove(console6.CentreFrequencyHandlers, new Console.CentreFrequencyChanged(OnCentreFrequency));
			Console console7 = _console;
			console7.FilterEdgesChangedHandlers = (Console.FilterEdgesChanged)Delegate.Remove(console7.FilterEdgesChangedHandlers, new Console.FilterEdgesChanged(OnFilterEdgesChanged));
			Console console8 = _console;
			console8.HWSampleRateChangedHandlers = (Console.HWSampleRateChanged)Delegate.Remove(console8.HWSampleRateChangedHandlers, new Console.HWSampleRateChanged(OnHWSampleRateChanged));
			Console console9 = _console;
			console9.SpectrumSettingsChangedHandlers = (Console.SpectrumSettingsChanged)Delegate.Remove(console9.SpectrumSettingsChangedHandlers, new Console.SpectrumSettingsChanged(OnSpectrumSettingsChanged));
			Console console10 = _console;
			console10.AVGChangedHandlers = (Console.AVGOnChanged)Delegate.Remove(console10.AVGChangedHandlers, new Console.AVGOnChanged(OnAVGChanged));
			Console console11 = _console;
			console11.NotifiySpectrumDetailsChangedHandlers = (Console.NotifiySpectrumDetailsChanged)Delegate.Remove(console11.NotifiySpectrumDetailsChangedHandlers, new Console.NotifiySpectrumDetailsChanged(OnSpectrumDetailsChanged));
			Console console12 = _console;
			console12.CWPitchChangedHandlers = (Console.CWPitchChanged)Delegate.Remove(console12.CWPitchChangedHandlers, new Console.CWPitchChanged(OnCWPitchChanged));
			Console console13 = _console;
			console13.ModeChangeHandlers = (Console.ModeChanged)Delegate.Remove(console13.ModeChangeHandlers, new Console.ModeChanged(OnModeChanged));
			Console console14 = _console;
			console14.PowerChangeHanders = (Console.PowerChanged)Delegate.Remove(console14.PowerChangeHanders, new Console.PowerChanged(OnPowerChanged));
			Console console15 = _console;
			console15.CTUNChangedHandlers = (Console.CTUNChanged)Delegate.Remove(console15.CTUNChangedHandlers, new Console.CTUNChanged(OnCTUNChanged));
		}
		foreach (int item in _mini_spec.Keys.ToList())
		{
			shutdownRX(item);
		}
	}

	private static void OnPowerChanged(bool old_state, bool new_state)
	{
		if (!old_state || new_state)
		{
			return;
		}
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec)
		{
			item.Value.ClearData();
		}
	}

	private static void OnCTUNChanged(int rx, bool oldCTUN, bool newCTUN, Band band)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.Ctun = newCTUN;
		}
	}

	private static void OnModeChanged(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.Mode = newMode;
		}
	}

	private static void OnCWPitchChanged(int old_pitch, int new_pitch, bool show_cwzero)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec)
		{
			item.Value.CWPitchOffset = new_pitch;
		}
	}

	private static void OnSpectrumSettingsChanged(int rx)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.UpdateSpecSettings();
		}
	}

	private static void OnHWSampleRateChanged(int rx, int old_rate, int new_rate)
	{
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.HWSampleRate = new_rate;
		}
	}

	private static void OnCentreFrequency(int rx, double oldFreq, double newFreq, Band band, double offset)
	{
		if (oldFreq == newFreq)
		{
			return;
		}
		foreach (KeyValuePair<int, clsMiniSpec> item in _mini_spec.Where((KeyValuePair<int, clsMiniSpec> minispec) => minispec.Value.RX == rx))
		{
			item.Value.CentreFreq = newFreq;
		}
	}

	private static void OnNotchChanged(int notch_index, double old_bw, double new_bw, bool active, double old_centre_freq, double new_centre_freq, bool added, bool removed)
	{
		lock (_notch_locker)
		{
			if (added)
			{
				if (notch_index < _notches.Count)
				{
					_notches.RemoveAt(notch_index);
				}
				int count = _notches.Count;
				Notch item = new Notch
				{
					index = count,
					active = active,
					frequency_hz = new_centre_freq,
					width_hz = new_bw
				};
				_notches.Add(item);
				return;
			}
			if (removed)
			{
				if (notch_index < _notches.Count)
				{
					_notches.RemoveAt(notch_index);
				}
				int num = 0;
				{
					foreach (Notch notch in _notches)
					{
						notch.index = num;
						num++;
					}
					return;
				}
			}
			if (notch_index < _notches.Count)
			{
				if (new_bw != -1.0)
				{
					_notches[notch_index].width_hz = new_bw;
				}
				if (new_centre_freq != -1.0)
				{
					_notches[notch_index].frequency_hz = new_centre_freq;
				}
				_notches[notch_index].active = active;
			}
		}
	}

	public static List<Notch> GetNotches(double centre_hz, int half_bandwidth)
	{
		lock (_notch_locker)
		{
			double lower_bound = centre_hz - (double)half_bandwidth;
			double upper_bound = centre_hz + (double)half_bandwidth;
			return _notches.Where((Notch notch) => notch.frequency_hz >= lower_bound && notch.frequency_hz <= upper_bound).ToList();
		}
	}

	public static Notch GetNotch(int notch_index)
	{
		lock (_notch_locker)
		{
			if (notch_index >= _notches.Count)
			{
				return null;
			}
			return _notches[notch_index];
		}
	}
}
