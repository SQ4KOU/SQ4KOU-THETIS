using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Thetis;

public sealed class ucParametricEq : UserControl
{
	public sealed class EqPoint
	{
		private readonly int _band_id;

		private Color _band_color;

		private double _frequency_hz;

		private double _gain_db;

		private double _q;

		public int BandId => _band_id;

		public Color BandColor
		{
			get
			{
				return _band_color;
			}
			set
			{
				_band_color = value;
			}
		}

		public double FrequencyHz
		{
			get
			{
				return _frequency_hz;
			}
			set
			{
				_frequency_hz = value;
			}
		}

		public double GainDb
		{
			get
			{
				return _gain_db;
			}
			set
			{
				_gain_db = value;
			}
		}

		public double Q
		{
			get
			{
				return _q;
			}
			set
			{
				_q = value;
			}
		}

		public EqPoint(double frequency_hz, double gain_db, double q)
			: this(0, Color.Empty, frequency_hz, gain_db, q)
		{
		}

		public EqPoint(int band_id, Color band_color, double frequency_hz, double gain_db, double q)
		{
			_band_id = band_id;
			_band_color = band_color;
			_frequency_hz = frequency_hz;
			_gain_db = gain_db;
			_q = q;
		}
	}

	public sealed class EqDraggingEventArgs : EventArgs
	{
		private readonly bool _is_dragging;

		public bool IsDragging => _is_dragging;

		public EqDraggingEventArgs(bool is_dragging)
		{
			_is_dragging = is_dragging;
		}
	}

	public sealed class EqPointDataChangedEventArgs : EventArgs
	{
		private readonly int _index;

		private readonly int _band_id;

		private readonly double _frequency_hz;

		private readonly double _gain_db;

		private readonly double _q;

		private readonly bool _is_dragging;

		public int Index => _index;

		public int BandId => _band_id;

		public double FrequencyHz => _frequency_hz;

		public double GainDb => _gain_db;

		public double Q => _q;

		public bool IsDragging => _is_dragging;

		public EqPointDataChangedEventArgs(int index, int band_id, double frequency_hz, double gain_db, double q)
			: this(index, band_id, frequency_hz, gain_db, q, is_dragging: false)
		{
		}

		public EqPointDataChangedEventArgs(int index, int band_id, double frequency_hz, double gain_db, double q, bool is_dragging)
		{
			_index = index;
			_band_id = band_id;
			_frequency_hz = frequency_hz;
			_gain_db = gain_db;
			_q = q;
			_is_dragging = is_dragging;
		}
	}

	public sealed class EqPointSelectionChangedEventArgs : EventArgs
	{
		private readonly int _index;

		private readonly int _band_id;

		private readonly double _frequency_hz;

		private readonly double _gain_db;

		private readonly double _q;

		public int Index => _index;

		public int BandId => _band_id;

		public double FrequencyHz => _frequency_hz;

		public double GainDb => _gain_db;

		public double Q => _q;

		public EqPointSelectionChangedEventArgs(int index, int band_id, double frequency_hz, double gain_db, double q)
		{
			_index = index;
			_band_id = band_id;
			_frequency_hz = frequency_hz;
			_gain_db = gain_db;
			_q = q;
		}
	}

	private sealed class EqJsonState
	{
		[JsonProperty("band_count")]
		public int BandCount { get; set; }

		[JsonProperty("parametric_eq")]
		public bool ParametricEQ { get; set; }

		[JsonProperty("global_gain_db")]
		public double GlobalGainDb { get; set; }

		[JsonProperty("frequency_min_hz")]
		public double FrequencyMinHz { get; set; }

		[JsonProperty("frequency_max_hz")]
		public double FrequencyMaxHz { get; set; }

		[JsonProperty("points")]
		public List<EqJsonPoint> Points { get; set; }
	}

	private sealed class EqJsonPoint
	{
		[JsonProperty("frequency_hz")]
		public double FrequencyHz { get; set; }

		[JsonProperty("gain_db")]
		public double GainDb { get; set; }

		[JsonProperty("q")]
		public double Q { get; set; }
	}

	private static readonly Color[] _default_band_palette = new Color[18]
	{
		Color.FromArgb(0, 190, 255),
		Color.FromArgb(0, 220, 130),
		Color.FromArgb(255, 210, 0),
		Color.FromArgb(255, 140, 0),
		Color.FromArgb(255, 80, 80),
		Color.FromArgb(255, 0, 180),
		Color.FromArgb(170, 90, 255),
		Color.FromArgb(70, 120, 255),
		Color.FromArgb(0, 200, 200),
		Color.FromArgb(180, 255, 90),
		Color.FromArgb(255, 105, 180),
		Color.FromArgb(255, 215, 120),
		Color.FromArgb(120, 255, 255),
		Color.FromArgb(140, 200, 255),
		Color.FromArgb(220, 160, 255),
		Color.FromArgb(255, 120, 40),
		Color.FromArgb(120, 255, 160),
		Color.FromArgb(255, 60, 120)
	};

	private readonly List<EqPoint> _points;

	private readonly ReadOnlyCollection<EqPoint> _points_readonly;

	private int _band_count;

	private double _frequency_min_hz;

	private double _frequency_max_hz;

	private double _db_min;

	private double _db_max;

	private double _global_gain_db;

	private bool _global_gain_is_horiz_line;

	private int _selected_index;

	private int _drag_index;

	private bool _dragging_global_gain;

	private bool _dragging_point;

	private EqPoint _drag_point_ref;

	private bool _drag_dirty_point;

	private bool _drag_dirty_global_gain;

	private bool _drag_dirty_selected_index;

	private int _plot_margin_left;

	private int _plot_margin_right;

	private int _plot_margin_top;

	private int _plot_margin_bottom;

	private double _y_axis_step_db;

	private int _point_radius;

	private int _hit_radius;

	private double _q_min;

	private double _q_max;

	private double _min_point_spacing_hz;

	private bool _allow_point_reorder;

	private bool _parametric_eq;

	private bool _show_readout;

	private bool _show_dot_readings;

	private bool _show_dot_readings_as_comp;

	private int _global_handle_x_offset;

	private int _global_handle_size;

	private int _global_hit_extra;

	private bool _show_band_shading;

	private bool _use_per_band_colours;

	private Color _band_shade_color;

	private int _band_shade_alpha;

	private double _band_shade_weight_cutoff;

	private bool _show_axis_scales;

	private Color _axis_text_color;

	private Color _axis_tick_color;

	private int _axis_tick_length;

	private bool _log_scale;

	private double[] _bar_chart_data;

	private double[] _bar_chart_peak_data;

	private long[] _bar_chart_peak_hold_until_ticks;

	private DateTime _bar_chart_peak_last_update_utc;

	private readonly Timer _bar_chart_peak_timer;

	private Color _bar_chart_fill_color;

	private int _bar_chart_fill_alpha;

	private Color _bar_chart_peak_color;

	private int _bar_chart_peak_alpha;

	private bool _bar_chart_peak_hold_enabled;

	private int _bar_chart_peak_hold_ms;

	private double _bar_chart_peak_decay_db_per_second;

	[Category("Bar Chart")]
	[DefaultValue(true)]
	public bool BarChartPeakHoldEnabled
	{
		get
		{
			return _bar_chart_peak_hold_enabled;
		}
		set
		{
			bool flag = value;
			if (flag != _bar_chart_peak_hold_enabled)
			{
				_bar_chart_peak_hold_enabled = flag;
				_bar_chart_peak_last_update_utc = DateTime.UtcNow;
				if (!_bar_chart_peak_hold_enabled && _bar_chart_data != null)
				{
					syncBarChartPeaksToData();
				}
				updateBarChartPeakTimerState();
				Invalidate();
			}
		}
	}

	[Category("Bar Chart")]
	[DefaultValue(1000)]
	public int BarChartPeakHoldMs
	{
		get
		{
			return _bar_chart_peak_hold_ms;
		}
		set
		{
			int num = value;
			if (num < 0)
			{
				num = 0;
			}
			if (num != _bar_chart_peak_hold_ms)
			{
				_bar_chart_peak_hold_ms = num;
			}
		}
	}

	[Category("Bar Chart")]
	[DefaultValue(20.0)]
	public double BarChartPeakDecayDbPerSecond
	{
		get
		{
			return _bar_chart_peak_decay_db_per_second;
		}
		set
		{
			double num = value;
			if (!double.IsNaN(num) && !double.IsInfinity(num))
			{
				if (num < 0.0)
				{
					num = 0.0;
				}
				if (!(Math.Abs(num - _bar_chart_peak_decay_db_per_second) < 1E-06))
				{
					_bar_chart_peak_decay_db_per_second = num;
				}
			}
		}
	}

	[Category("Bar Chart")]
	public Color BarChartFillColor
	{
		get
		{
			return _bar_chart_fill_color;
		}
		set
		{
			_bar_chart_fill_color = value;
			Invalidate();
		}
	}

	[Category("Bar Chart")]
	[DefaultValue(120)]
	public int BarChartFillAlpha
	{
		get
		{
			return _bar_chart_fill_alpha;
		}
		set
		{
			int num = value;
			if (num < 0)
			{
				num = 0;
			}
			if (num > 255)
			{
				num = 255;
			}
			if (num != _bar_chart_fill_alpha)
			{
				_bar_chart_fill_alpha = num;
				Invalidate();
			}
		}
	}

	[Category("Bar Chart")]
	public Color BarChartPeakColor
	{
		get
		{
			return _bar_chart_peak_color;
		}
		set
		{
			_bar_chart_peak_color = value;
			Invalidate();
		}
	}

	[Category("Bar Chart")]
	[DefaultValue(230)]
	public int BarChartPeakAlpha
	{
		get
		{
			return _bar_chart_peak_alpha;
		}
		set
		{
			int num = value;
			if (num < 0)
			{
				num = 0;
			}
			if (num > 255)
			{
				num = 255;
			}
			if (num != _bar_chart_peak_alpha)
			{
				_bar_chart_peak_alpha = num;
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	[DefaultValue(0.0)]
	public double YAxisStepDb
	{
		get
		{
			return _y_axis_step_db;
		}
		set
		{
			double num = value;
			if (!double.IsNaN(num) && !double.IsInfinity(num))
			{
				if (num < 0.0)
				{
					num = 0.0;
				}
				if (num > 0.0 && num < 0.1)
				{
					num = 0.1;
				}
				if (!(Math.Abs(num - _y_axis_step_db) < 1E-06))
				{
					_y_axis_step_db = num;
					Invalidate();
				}
			}
		}
	}

	[Category("EQ")]
	[DefaultValue(10)]
	public int BandCount
	{
		get
		{
			return _band_count;
		}
		set
		{
			int num = value;
			if (num < 2)
			{
				num = 2;
			}
			if (num > 256)
			{
				num = 256;
			}
			if (num != _band_count)
			{
				_band_count = num;
				bool num2 = _selected_index != -1;
				resetPointsDefault();
				if (num2)
				{
					_selected_index = -1;
					raiseSelectedIndexChanged(is_dragging: false);
				}
				raisePointsChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public double FrequencyMinHz
	{
		get
		{
			return _frequency_min_hz;
		}
		set
		{
			if (!double.IsNaN(value) && !double.IsInfinity(value) && !(value >= _frequency_max_hz))
			{
				double frequency_min_hz = _frequency_min_hz;
				double frequency_max_hz = _frequency_max_hz;
				_frequency_min_hz = value;
				rescaleFrequencies(frequency_min_hz, frequency_max_hz, _frequency_min_hz, _frequency_max_hz);
				enforceOrdering(enforce_spacing_all: true);
				raisePointsChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public double FrequencyMaxHz
	{
		get
		{
			return _frequency_max_hz;
		}
		set
		{
			if (!double.IsNaN(value) && !double.IsInfinity(value) && !(value <= _frequency_min_hz))
			{
				double frequency_min_hz = _frequency_min_hz;
				double frequency_max_hz = _frequency_max_hz;
				_frequency_max_hz = value;
				rescaleFrequencies(frequency_min_hz, frequency_max_hz, _frequency_min_hz, _frequency_max_hz);
				enforceOrdering(enforce_spacing_all: true);
				raisePointsChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	[DefaultValue(false)]
	public bool LogScale
	{
		get
		{
			return _log_scale;
		}
		set
		{
			bool flag = value;
			if (flag != _log_scale)
			{
				_log_scale = flag;
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public double DbMin
	{
		get
		{
			return _db_min;
		}
		set
		{
			if (!double.IsNaN(value) && !double.IsInfinity(value) && !(value >= _db_max))
			{
				_db_min = value;
				clampAllGains();
				if (_bar_chart_data != null)
				{
					syncBarChartPeaksToData();
				}
				raisePointsChanged(is_dragging: false);
				raiseGlobalGainChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public double DbMax
	{
		get
		{
			return _db_max;
		}
		set
		{
			if (!double.IsNaN(value) && !double.IsInfinity(value) && !(value <= _db_min))
			{
				_db_max = value;
				clampAllGains();
				if (_bar_chart_data != null)
				{
					syncBarChartPeaksToData();
				}
				raisePointsChanged(is_dragging: false);
				raiseGlobalGainChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public double GlobalGainDb
	{
		get
		{
			return _global_gain_db;
		}
		set
		{
			double num = clamp(value, _db_min, _db_max);
			if (!(Math.Abs(num - _global_gain_db) < 1E-06))
			{
				_global_gain_db = num;
				if (_dragging_global_gain)
				{
					_drag_dirty_global_gain = true;
				}
				raiseGlobalGainChanged(isDraggingNow());
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	[DefaultValue(false)]
	public bool GlobalGainIsHorizLine
	{
		get
		{
			return _global_gain_is_horiz_line;
		}
		set
		{
			bool flag = value;
			if (flag != _global_gain_is_horiz_line)
			{
				_global_gain_is_horiz_line = flag;
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public bool ShowReadout
	{
		get
		{
			return _show_readout;
		}
		set
		{
			_show_readout = value;
			Invalidate();
		}
	}

	[Category("EQ")]
	[DefaultValue(false)]
	public bool ShowDotReadings
	{
		get
		{
			return _show_dot_readings;
		}
		set
		{
			bool flag = value;
			if (flag != _show_dot_readings)
			{
				_show_dot_readings = flag;
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	[DefaultValue(false)]
	public bool ShowDotReadingsAsComp
	{
		get
		{
			return _show_dot_readings_as_comp;
		}
		set
		{
			bool flag = value;
			if (flag != _show_dot_readings_as_comp)
			{
				_show_dot_readings_as_comp = flag;
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public double MinPointSpacingHz
	{
		get
		{
			return _min_point_spacing_hz;
		}
		set
		{
			double num = value;
			if (!double.IsNaN(num) && !double.IsInfinity(num))
			{
				if (num < 0.0)
				{
					num = 0.0;
				}
				_min_point_spacing_hz = num;
				enforceOrdering(enforce_spacing_all: true);
				raisePointsChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public bool AllowPointReorder
	{
		get
		{
			return _allow_point_reorder;
		}
		set
		{
			bool flag = value;
			if (flag != _allow_point_reorder)
			{
				_allow_point_reorder = flag;
				enforceOrdering(enforce_spacing_all: true);
				raisePointsChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public bool ParametricEQ
	{
		get
		{
			return _parametric_eq;
		}
		set
		{
			bool flag = value;
			if (flag != _parametric_eq)
			{
				_parametric_eq = flag;
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public double QMin
	{
		get
		{
			return _q_min;
		}
		set
		{
			if (!double.IsNaN(value) && !double.IsInfinity(value) && !(value <= 0.0))
			{
				_q_min = value;
				if (_q_max < _q_min)
				{
					_q_max = _q_min;
				}
				clampAllQ();
				raisePointsChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public double QMax
	{
		get
		{
			return _q_max;
		}
		set
		{
			if (!double.IsNaN(value) && !double.IsInfinity(value) && !(value <= 0.0))
			{
				_q_max = value;
				if (_q_min > _q_max)
				{
					_q_min = _q_max;
				}
				clampAllQ();
				raisePointsChanged(is_dragging: false);
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public bool ShowBandShading
	{
		get
		{
			return _show_band_shading;
		}
		set
		{
			_show_band_shading = value;
			Invalidate();
		}
	}

	[Category("EQ")]
	public bool UsePerBandColours
	{
		get
		{
			return _use_per_band_colours;
		}
		set
		{
			_use_per_band_colours = value;
			Invalidate();
		}
	}

	[Category("EQ")]
	public Color BandShadeColor
	{
		get
		{
			return _band_shade_color;
		}
		set
		{
			_band_shade_color = value;
			Invalidate();
		}
	}

	[Category("EQ")]
	public int BandShadeAlpha
	{
		get
		{
			return _band_shade_alpha;
		}
		set
		{
			int num = value;
			if (num < 0)
			{
				num = 0;
			}
			if (num > 255)
			{
				num = 255;
			}
			_band_shade_alpha = num;
			Invalidate();
		}
	}

	[Category("EQ")]
	public double BandShadeWeightCutoff
	{
		get
		{
			return _band_shade_weight_cutoff;
		}
		set
		{
			double num = value;
			if (!double.IsNaN(num) && !double.IsInfinity(num))
			{
				if (num < 0.0)
				{
					num = 0.0;
				}
				_band_shade_weight_cutoff = num;
				Invalidate();
			}
		}
	}

	[Category("EQ")]
	public bool ShowAxisScales
	{
		get
		{
			return _show_axis_scales;
		}
		set
		{
			_show_axis_scales = value;
			Invalidate();
		}
	}

	[Category("EQ")]
	public int AxisTickLength
	{
		get
		{
			return _axis_tick_length;
		}
		set
		{
			int num = value;
			if (num < 2)
			{
				num = 2;
			}
			if (num > 20)
			{
				num = 20;
			}
			_axis_tick_length = num;
			Invalidate();
		}
	}

	[Category("EQ")]
	public Color AxisTextColor
	{
		get
		{
			return _axis_text_color;
		}
		set
		{
			_axis_text_color = value;
			Invalidate();
		}
	}

	[Category("EQ")]
	public Color AxisTickColor
	{
		get
		{
			return _axis_tick_color;
		}
		set
		{
			_axis_tick_color = value;
			Invalidate();
		}
	}

	[Browsable(false)]
	public IReadOnlyList<EqPoint> Points => _points_readonly;

	[Browsable(false)]
	public int SelectedIndex
	{
		get
		{
			return _selected_index;
		}
		set
		{
			int num = value;
			if (num < -1)
			{
				num = -1;
			}
			if (num >= _points.Count)
			{
				num = _points.Count - 1;
			}
			if (num != _selected_index)
			{
				EqPoint eqPoint = null;
				int selected_index = _selected_index;
				if (selected_index >= 0 && selected_index < _points.Count)
				{
					eqPoint = _points[selected_index];
				}
				_selected_index = num;
				if (eqPoint != null)
				{
					raisePointUnselected(selected_index, eqPoint);
				}
				if (_selected_index >= 0 && _selected_index < _points.Count)
				{
					EqPoint p = _points[_selected_index];
					raisePointSelected(_selected_index, p);
				}
				raiseSelectedIndexChanged(isDraggingNow());
				Invalidate();
			}
		}
	}

	public event EventHandler<EqDraggingEventArgs> PointsChanged;

	public event EventHandler<EqDraggingEventArgs> GlobalGainChanged;

	public event EventHandler<EqDraggingEventArgs> SelectedIndexChanged;

	public event EventHandler<EqPointDataChangedEventArgs> PointDataChanged;

	public event EventHandler<EqPointSelectionChangedEventArgs> PointSelected;

	public event EventHandler<EqPointSelectionChangedEventArgs> PointUnselected;

	public ucParametricEq()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		_band_count = 10;
		_frequency_min_hz = 0.0;
		_frequency_max_hz = 4000.0;
		_db_min = -24.0;
		_db_max = 24.0;
		_global_gain_db = 0.0;
		_global_gain_is_horiz_line = false;
		_plot_margin_left = 30;
		_plot_margin_right = 28;
		_plot_margin_top = 14;
		_plot_margin_bottom = 62;
		_y_axis_step_db = 0.0;
		_point_radius = 5;
		_hit_radius = 11;
		_q_min = 0.2;
		_q_max = 30.0;
		_min_point_spacing_hz = 5.0;
		_allow_point_reorder = true;
		_parametric_eq = true;
		_show_readout = true;
		_show_dot_readings = false;
		_show_dot_readings_as_comp = false;
		_global_handle_x_offset = 6;
		_global_handle_size = 10;
		_global_hit_extra = 6;
		_show_band_shading = true;
		_use_per_band_colours = true;
		_band_shade_color = Color.FromArgb(200, 200, 200);
		_band_shade_alpha = 70;
		_band_shade_weight_cutoff = 0.002;
		_show_axis_scales = true;
		_axis_text_color = Color.FromArgb(170, 170, 170);
		_axis_tick_color = Color.FromArgb(80, 80, 80);
		_axis_tick_length = 6;
		_log_scale = false;
		_bar_chart_data = null;
		_bar_chart_peak_data = null;
		_bar_chart_peak_hold_until_ticks = null;
		_bar_chart_peak_last_update_utc = DateTime.UtcNow;
		_bar_chart_fill_color = Color.FromArgb(0, 120, 255);
		_bar_chart_fill_alpha = 120;
		_bar_chart_peak_color = Color.FromArgb(160, 210, 255);
		_bar_chart_peak_alpha = 230;
		_bar_chart_peak_hold_enabled = true;
		_bar_chart_peak_hold_ms = 1000;
		_bar_chart_peak_decay_db_per_second = 20.0;
		_bar_chart_peak_timer = new Timer();
		_bar_chart_peak_timer.Interval = 33;
		_bar_chart_peak_timer.Tick += barChartPeakTimer_Tick;
		_points = new List<EqPoint>();
		_points_readonly = new ReadOnlyCollection<EqPoint>(_points);
		_selected_index = -1;
		_drag_index = -1;
		_drag_point_ref = null;
		_drag_dirty_point = false;
		_drag_dirty_global_gain = false;
		_drag_dirty_selected_index = false;
		BackColor = Color.FromArgb(25, 25, 25);
		ForeColor = Color.Gainsboro;
		resetPointsDefault();
	}

	private double getYAxisStepDb()
	{
		if (_y_axis_step_db > 0.0)
		{
			return _y_axis_step_db;
		}
		return chooseDbStep(_db_max - _db_min);
	}

	private double chooseDbStep(double span)
	{
		if (span <= 3.0)
		{
			return 0.5;
		}
		if (span <= 6.0)
		{
			return 1.0;
		}
		if (span <= 12.0)
		{
			return 2.0;
		}
		if (span <= 24.0)
		{
			return 3.0;
		}
		if (span <= 48.0)
		{
			return 6.0;
		}
		if (span <= 96.0)
		{
			return 12.0;
		}
		return 24.0;
	}

	private void raisePointSelected(int index, EqPoint p)
	{
		if (p != null)
		{
			PointSelected?.Invoke(this, new EqPointSelectionChangedEventArgs(index, p.BandId, p.FrequencyHz, p.GainDb, p.Q));
		}
	}

	private void raisePointUnselected(int index, EqPoint p)
	{
		if (p != null)
		{
			PointUnselected?.Invoke(this, new EqPointSelectionChangedEventArgs(index, p.BandId, p.FrequencyHz, p.GainDb, p.Q));
		}
	}

	public void ResetPoints()
	{
		resetPointsDefault();
		raisePointsChanged(is_dragging: false);
		Invalidate();
	}

	public void DrawBarChart(double[] data)
	{
		if (data == null || data.Length == 0)
		{
			_bar_chart_data = null;
			_bar_chart_peak_data = null;
			_bar_chart_peak_hold_until_ticks = null;
			updateBarChartPeakTimerState();
			Invalidate();
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		applyBarChartPeakDecay(utcNow);
		bool flag = _bar_chart_data == null || _bar_chart_peak_data == null || _bar_chart_peak_hold_until_ticks == null || _bar_chart_data.Length != data.Length || _bar_chart_peak_data.Length != data.Length || _bar_chart_peak_hold_until_ticks.Length != data.Length;
		_bar_chart_data = new double[data.Length];
		Array.Copy(data, _bar_chart_data, data.Length);
		if (flag)
		{
			_bar_chart_peak_data = new double[data.Length];
			_bar_chart_peak_hold_until_ticks = new long[data.Length];
		}
		long ticks = utcNow.AddMilliseconds(_bar_chart_peak_hold_ms).Ticks;
		for (int i = 0; i < _bar_chart_data.Length; i++)
		{
			double num = clamp(_bar_chart_data[i], _db_min, _db_max);
			_bar_chart_data[i] = num;
			if (flag || !_bar_chart_peak_hold_enabled)
			{
				_bar_chart_peak_data[i] = num;
				_bar_chart_peak_hold_until_ticks[i] = ticks;
			}
			else if (num >= _bar_chart_peak_data[i])
			{
				_bar_chart_peak_data[i] = num;
				_bar_chart_peak_hold_until_ticks[i] = ticks;
			}
			else if (_bar_chart_peak_data[i] < num)
			{
				_bar_chart_peak_data[i] = num;
			}
		}
		_bar_chart_peak_last_update_utc = utcNow;
		updateBarChartPeakTimerState();
		Invalidate();
	}

	public void GetDefaults(out double[] F, out double[] G, out double[] Q, out double global_preamp_db, out double min_hz, out double max_hz, out bool parametric_eq, out int band_count, int default_band_count = 10, double default_min_hz = 0.0, double default_max_hz = 4000.0)
	{
		band_count = default_band_count;
		global_preamp_db = 0.0;
		min_hz = default_min_hz;
		max_hz = default_max_hz;
		parametric_eq = true;
		int num = band_count;
		if (num < 2)
		{
			num = 2;
		}
		F = new double[num];
		G = new double[num];
		Q = new double[num];
		double num2 = max_hz - min_hz;
		if (num2 <= 0.0)
		{
			num2 = 1.0;
		}
		for (int i = 0; i < num; i++)
		{
			double num3 = (double)i / (double)(num - 1);
			F[i] = min_hz + num3 * num2;
			G[i] = 0.0;
			Q[i] = 4.0;
		}
	}

	public bool SetPointHz(int band_id, double frequency_hz, bool is_dragging = false)
	{
		EqPoint eqPoint = findPointByBandId(band_id);
		if (eqPoint == null)
		{
			return false;
		}
		return setPointHzInternal(eqPoint, frequency_hz, is_dragging);
	}

	public int GetIndexFromBandId(int band_id)
	{
		for (int i = 0; i < _points.Count; i++)
		{
			if (_points[i].BandId == band_id)
			{
				return i;
			}
		}
		return -1;
	}

	private EqPoint findPointByBandId(int band_id)
	{
		for (int i = 0; i < _points.Count; i++)
		{
			if (_points[i].BandId == band_id)
			{
				return _points[i];
			}
		}
		return null;
	}

	private bool setPointHzInternal(EqPoint p, double frequency_hz, bool is_dragging)
	{
		if (p == null)
		{
			return false;
		}
		int num = _points.IndexOf(p);
		if (num < 0)
		{
			return false;
		}
		double frequencyHz = p.FrequencyHz;
		double gainDb = p.GainDb;
		double q = p.Q;
		double num2;
		if (isFrequencyLockedIndex(num))
		{
			num2 = getLockedFrequencyForIndex(num);
		}
		else
		{
			num2 = clamp(frequency_hz, _frequency_min_hz, _frequency_max_hz);
			if (!_allow_point_reorder)
			{
				double num3;
				double num4;
				if (num == 0)
				{
					num3 = _frequency_min_hz;
					num4 = _points[1].FrequencyHz - _min_point_spacing_hz;
				}
				else if (num == _points.Count - 1)
				{
					num3 = _points[_points.Count - 2].FrequencyHz + _min_point_spacing_hz;
					num4 = _frequency_max_hz;
				}
				else
				{
					num3 = _points[num - 1].FrequencyHz + _min_point_spacing_hz;
					num4 = _points[num + 1].FrequencyHz - _min_point_spacing_hz;
				}
				if (num4 < num3)
				{
					num4 = num3;
				}
				num2 = clamp(num2, num3, num4);
			}
		}
		if (Math.Abs(p.FrequencyHz - num2) <= 1E-06)
		{
			return true;
		}
		p.FrequencyHz = num2;
		if (_allow_point_reorder && !isFrequencyLockedIndex(num))
		{
			enforceOrdering(enforce_spacing_all: false);
			num = _points.IndexOf(p);
			double num5 = _frequency_min_hz;
			double num6 = _frequency_max_hz;
			if (_points.Count > 1)
			{
				if (num > 0)
				{
					num5 = _points[num - 1].FrequencyHz + _min_point_spacing_hz;
				}
				if (num < _points.Count - 1)
				{
					num6 = _points[num + 1].FrequencyHz - _min_point_spacing_hz;
				}
				if (num6 < num5)
				{
					num6 = num5;
				}
			}
			double num7 = clamp(p.FrequencyHz, num5, num6);
			if (Math.Abs(num7 - p.FrequencyHz) > 1E-06)
			{
				p.FrequencyHz = num7;
			}
			enforceOrdering(enforce_spacing_all: false);
		}
		raisePointsChanged(is_dragging);
		if (Math.Abs(p.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(p.GainDb - gainDb) > 1E-06 || Math.Abs(p.Q - q) > 1E-06)
		{
			raisePointDataChangedForPoint(p, is_dragging);
		}
		Invalidate();
		return true;
	}

	public void GetPointData(int index, out double frequency_hz, out double gain_db, out double q)
	{
		frequency_hz = 0.0;
		gain_db = 0.0;
		q = 0.0;
		if (index >= 0 && index < _points.Count)
		{
			EqPoint eqPoint = _points[index];
			frequency_hz = Math.Round(eqPoint.FrequencyHz, 3);
			gain_db = Math.Round(eqPoint.GainDb, 1);
			q = (_parametric_eq ? Math.Round(eqPoint.Q, 2) : 0.0);
		}
	}

	public bool SetPointData(int index, double frequency_hz, double gain_db, double q)
	{
		if (index < 0 || index >= _points.Count)
		{
			return false;
		}
		EqPoint eqPoint = _points[index];
		double frequencyHz = eqPoint.FrequencyHz;
		double gainDb = eqPoint.GainDb;
		double q2 = eqPoint.Q;
		if (isFrequencyLockedIndex(index))
		{
			eqPoint.FrequencyHz = getLockedFrequencyForIndex(index);
		}
		else
		{
			eqPoint.FrequencyHz = clamp(frequency_hz, _frequency_min_hz, _frequency_max_hz);
		}
		eqPoint.GainDb = clamp(gain_db, _db_min, _db_max);
		eqPoint.Q = clamp(q, _q_min, _q_max);
		enforceOrdering(enforce_spacing_all: true);
		if (Math.Abs(eqPoint.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(eqPoint.GainDb - gainDb) > 1E-06 || Math.Abs(eqPoint.Q - q2) > 1E-06)
		{
			raisePointsChanged(is_dragging: false);
			raisePointDataChangedForPoint(eqPoint, is_dragging: false);
			Invalidate();
		}
		return true;
	}

	public void GetPointsData(out double[] frequency_hz, out double[] gain_db, out double[] q)
	{
		int count = _points.Count;
		double[] array = new double[count];
		double[] array2 = new double[count];
		double[] array3 = new double[count];
		for (int i = 0; i < count; i++)
		{
			EqPoint eqPoint = _points[i];
			array[i] = eqPoint.FrequencyHz;
			array2[i] = eqPoint.GainDb;
			array3[i] = eqPoint.Q;
		}
		frequency_hz = array;
		gain_db = array2;
		q = array3;
	}

	public bool SetPointsData(double[] frequency_hz, double[] gain_db, double[] q)
	{
		if (frequency_hz == null || gain_db == null || q == null)
		{
			return false;
		}
		if (frequency_hz.Length != _points.Count)
		{
			return false;
		}
		if (gain_db.Length != _points.Count)
		{
			return false;
		}
		if (q.Length != _points.Count)
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < _points.Count; i++)
		{
			EqPoint eqPoint = _points[i];
			double frequencyHz = eqPoint.FrequencyHz;
			double gainDb = eqPoint.GainDb;
			double q2 = eqPoint.Q;
			if (isFrequencyLockedIndex(i))
			{
				eqPoint.FrequencyHz = getLockedFrequencyForIndex(i);
			}
			else
			{
				eqPoint.FrequencyHz = clamp(frequency_hz[i], _frequency_min_hz, _frequency_max_hz);
			}
			eqPoint.GainDb = clamp(gain_db[i], _db_min, _db_max);
			eqPoint.Q = clamp(q[i], _q_min, _q_max);
			if (Math.Abs(eqPoint.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(eqPoint.GainDb - gainDb) > 1E-06 || Math.Abs(eqPoint.Q - q2) > 1E-06)
			{
				flag = true;
			}
		}
		enforceOrdering(enforce_spacing_all: true);
		if (flag)
		{
			raisePointsChanged(is_dragging: false);
			Invalidate();
		}
		return true;
	}

	public string SaveToJsonFromPoints(double[] F, double[] G, double[] Q, double global_gain_db, double frequency_min_hz, double frequency_max_hz, bool parametric_eq)
	{
		if (F == null || G == null || Q == null)
		{
			return null;
		}
		if (F.Length < 2)
		{
			return null;
		}
		if (G.Length != F.Length)
		{
			return null;
		}
		if (Q.Length != F.Length)
		{
			return null;
		}
		if (double.IsNaN(frequency_min_hz) || double.IsInfinity(frequency_min_hz))
		{
			return null;
		}
		if (double.IsNaN(frequency_max_hz) || double.IsInfinity(frequency_max_hz))
		{
			return null;
		}
		if (frequency_max_hz <= frequency_min_hz)
		{
			return null;
		}
		EqJsonState eqJsonState = new EqJsonState();
		eqJsonState.ParametricEQ = parametric_eq;
		eqJsonState.GlobalGainDb = Math.Round(clamp(global_gain_db, _db_min, _db_max), 1);
		eqJsonState.FrequencyMinHz = Math.Round(frequency_min_hz, 3);
		eqJsonState.FrequencyMaxHz = Math.Round(frequency_max_hz, 3);
		eqJsonState.BandCount = F.Length;
		List<EqJsonPoint> list = new List<EqJsonPoint>(F.Length);
		for (int i = 0; i < F.Length; i++)
		{
			EqJsonPoint eqJsonPoint = new EqJsonPoint();
			eqJsonPoint.FrequencyHz = Math.Round(clamp(F[i], frequency_min_hz, frequency_max_hz), 3);
			eqJsonPoint.GainDb = Math.Round(clamp(G[i], _db_min, _db_max), 1);
			eqJsonPoint.Q = Math.Round(clamp(Q[i], _q_min, _q_max), 2);
			list.Add(eqJsonPoint);
		}
		list[0].FrequencyHz = Math.Round(frequency_min_hz, 3);
		list[list.Count - 1].FrequencyHz = Math.Round(frequency_max_hz, 3);
		eqJsonState.Points = list;
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.Formatting = Formatting.Indented;
		return JsonConvert.SerializeObject(eqJsonState, jsonSerializerSettings);
	}

	public bool PointsFromJson(string json, out double[] F, out double[] G, out double[] Q, out double global_gain_db, out double frequency_min_hz, out double frequency_max_hz, out bool parametric_eq, out int band_count)
	{
		F = null;
		G = null;
		Q = null;
		global_gain_db = 0.0;
		frequency_min_hz = 0.0;
		frequency_max_hz = 0.0;
		parametric_eq = false;
		band_count = 0;
		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}
		EqJsonState eqJsonState;
		try
		{
			eqJsonState = JsonConvert.DeserializeObject<EqJsonState>(json);
		}
		catch
		{
			return false;
		}
		if (eqJsonState == null)
		{
			return false;
		}
		if (eqJsonState.Points == null)
		{
			return false;
		}
		if (eqJsonState.Points.Count < 2)
		{
			return false;
		}
		if (eqJsonState.BandCount < 2)
		{
			eqJsonState.BandCount = eqJsonState.Points.Count;
		}
		if (eqJsonState.BandCount < 2)
		{
			return false;
		}
		if (eqJsonState.BandCount > 256)
		{
			return false;
		}
		if (eqJsonState.BandCount != eqJsonState.Points.Count)
		{
			return false;
		}
		if (double.IsNaN(eqJsonState.FrequencyMinHz) || double.IsInfinity(eqJsonState.FrequencyMinHz))
		{
			return false;
		}
		if (double.IsNaN(eqJsonState.FrequencyMaxHz) || double.IsInfinity(eqJsonState.FrequencyMaxHz))
		{
			return false;
		}
		if (eqJsonState.FrequencyMaxHz <= eqJsonState.FrequencyMinHz)
		{
			return false;
		}
		int count = eqJsonState.Points.Count;
		F = new double[count];
		G = new double[count];
		Q = new double[count];
		parametric_eq = eqJsonState.ParametricEQ;
		global_gain_db = Math.Round(clamp(eqJsonState.GlobalGainDb, _db_min, _db_max), 1);
		frequency_min_hz = Math.Round(eqJsonState.FrequencyMinHz, 3);
		frequency_max_hz = Math.Round(eqJsonState.FrequencyMaxHz, 3);
		band_count = eqJsonState.BandCount;
		for (int i = 0; i < count; i++)
		{
			EqJsonPoint eqJsonPoint = eqJsonState.Points[i];
			double value = clamp(eqJsonPoint.FrequencyHz, eqJsonState.FrequencyMinHz, eqJsonState.FrequencyMaxHz);
			double value2 = clamp(eqJsonPoint.GainDb, _db_min, _db_max);
			double value3 = clamp(eqJsonPoint.Q, _q_min, _q_max);
			if (i == 0)
			{
				value = eqJsonState.FrequencyMinHz;
			}
			if (i == count - 1)
			{
				value = eqJsonState.FrequencyMaxHz;
			}
			F[i] = Math.Round(value, 3);
			G[i] = Math.Round(value2, 1);
			Q[i] = Math.Round(value3, 2);
		}
		return true;
	}

	public string SaveToJson()
	{
		EqJsonState eqJsonState = new EqJsonState();
		eqJsonState.BandCount = _points.Count;
		eqJsonState.ParametricEQ = _parametric_eq;
		eqJsonState.GlobalGainDb = Math.Round(_global_gain_db, 1);
		eqJsonState.FrequencyMinHz = Math.Round(_frequency_min_hz, 3);
		eqJsonState.FrequencyMaxHz = Math.Round(_frequency_max_hz, 3);
		List<EqJsonPoint> list = new List<EqJsonPoint>(_points.Count);
		for (int i = 0; i < _points.Count; i++)
		{
			EqPoint eqPoint = _points[i];
			EqJsonPoint eqJsonPoint = new EqJsonPoint();
			eqJsonPoint.FrequencyHz = Math.Round(eqPoint.FrequencyHz, 3);
			eqJsonPoint.GainDb = Math.Round(eqPoint.GainDb, 1);
			eqJsonPoint.Q = Math.Round(eqPoint.Q, 2);
			list.Add(eqJsonPoint);
		}
		eqJsonState.Points = list;
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.Formatting = Formatting.Indented;
		return JsonConvert.SerializeObject(eqJsonState, jsonSerializerSettings);
	}

	public bool LoadFromJson(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}
		EqJsonState eqJsonState;
		try
		{
			eqJsonState = JsonConvert.DeserializeObject<EqJsonState>(json);
		}
		catch
		{
			return false;
		}
		if (eqJsonState == null)
		{
			return false;
		}
		if (eqJsonState.Points == null)
		{
			return false;
		}
		if (eqJsonState.Points.Count < 2)
		{
			return false;
		}
		if (eqJsonState.BandCount < 2)
		{
			eqJsonState.BandCount = eqJsonState.Points.Count;
		}
		if (eqJsonState.BandCount < 2)
		{
			return false;
		}
		if (eqJsonState.BandCount > 256)
		{
			return false;
		}
		if (eqJsonState.BandCount != eqJsonState.Points.Count)
		{
			return false;
		}
		if (double.IsNaN(eqJsonState.FrequencyMinHz) || double.IsInfinity(eqJsonState.FrequencyMinHz))
		{
			return false;
		}
		if (double.IsNaN(eqJsonState.FrequencyMaxHz) || double.IsInfinity(eqJsonState.FrequencyMaxHz))
		{
			return false;
		}
		if (eqJsonState.FrequencyMaxHz <= eqJsonState.FrequencyMinHz)
		{
			return false;
		}
		bool flag = false;
		if (eqJsonState.BandCount != _points.Count)
		{
			flag = true;
			_band_count = eqJsonState.BandCount;
			resetPointsDefault();
		}
		bool parametric_eq = _parametric_eq;
		double global_gain_db = _global_gain_db;
		double frequency_min_hz = _frequency_min_hz;
		double frequency_max_hz = _frequency_max_hz;
		_parametric_eq = eqJsonState.ParametricEQ;
		_global_gain_db = clamp(eqJsonState.GlobalGainDb, _db_min, _db_max);
		_frequency_min_hz = eqJsonState.FrequencyMinHz;
		_frequency_max_hz = eqJsonState.FrequencyMaxHz;
		if (parametric_eq != _parametric_eq)
		{
			flag = true;
		}
		if (Math.Abs(global_gain_db - _global_gain_db) > 1E-06)
		{
			flag = true;
		}
		if (Math.Abs(frequency_min_hz - _frequency_min_hz) > 1E-06)
		{
			flag = true;
		}
		if (Math.Abs(frequency_max_hz - _frequency_max_hz) > 1E-06)
		{
			flag = true;
		}
		for (int i = 0; i < _points.Count; i++)
		{
			EqPoint eqPoint = _points[i];
			EqJsonPoint eqJsonPoint = eqJsonState.Points[i];
			double frequencyHz = eqPoint.FrequencyHz;
			double gainDb = eqPoint.GainDb;
			double q = eqPoint.Q;
			if (isFrequencyLockedIndex(i))
			{
				eqPoint.FrequencyHz = getLockedFrequencyForIndex(i);
			}
			else
			{
				eqPoint.FrequencyHz = clamp(eqJsonPoint.FrequencyHz, _frequency_min_hz, _frequency_max_hz);
			}
			eqPoint.GainDb = clamp(eqJsonPoint.GainDb, _db_min, _db_max);
			eqPoint.Q = clamp(eqJsonPoint.Q, _q_min, _q_max);
			if (Math.Abs(eqPoint.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(eqPoint.GainDb - gainDb) > 1E-06 || Math.Abs(eqPoint.Q - q) > 1E-06)
			{
				flag = true;
			}
		}
		enforceOrdering(enforce_spacing_all: true);
		if (flag)
		{
			raisePointsChanged(is_dragging: false);
			raiseGlobalGainChanged(is_dragging: false);
			Invalidate();
		}
		return true;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width < 2 || clientRectangle.Height < 2)
		{
			return;
		}
		using (SolidBrush brush = new SolidBrush(BackColor))
		{
			graphics.FillRectangle(brush, clientRectangle);
		}
		Rectangle plotRect = getPlotRect();
		if (plotRect.Width >= 2 && plotRect.Height >= 2)
		{
			drawGrid(graphics, plotRect);
			Region clip = graphics.Clip;
			graphics.SetClip(plotRect);
			if (_show_band_shading)
			{
				drawBandShading(graphics, plotRect);
			}
			drawCurve(graphics, plotRect);
			drawPoints(graphics, plotRect);
			graphics.Clip = clip;
			if (_show_axis_scales)
			{
				drawAxisScales(graphics, plotRect);
			}
			drawGlobalGainHandle(graphics, plotRect);
			drawBorder(graphics, plotRect);
			if (_show_readout)
			{
				drawReadout(graphics, plotRect);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && _bar_chart_peak_timer != null)
		{
			_bar_chart_peak_timer.Stop();
			_bar_chart_peak_timer.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();
		Rectangle plotRect = getPlotRect();
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		if (hitTestGlobalGainHandle(plotRect, e.Location))
		{
			_dragging_global_gain = true;
			_dragging_point = false;
			_drag_index = -1;
			_drag_point_ref = null;
			_drag_dirty_point = false;
			_drag_dirty_global_gain = false;
			_drag_dirty_selected_index = false;
			base.Capture = true;
			Invalidate();
			return;
		}
		if (plotRect.Contains(e.Location))
		{
			int num = hitTestPoint(plotRect, e.Location);
			if (num >= 0)
			{
				SelectedIndex = num;
				_dragging_point = true;
				_dragging_global_gain = false;
				_drag_index = num;
				_drag_point_ref = _points[num];
				_drag_dirty_point = false;
				_drag_dirty_global_gain = false;
				_drag_dirty_selected_index = false;
				base.Capture = true;
				Invalidate();
				return;
			}
		}
		SelectedIndex = -1;
		Invalidate();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		Rectangle plotRect = getPlotRect();
		if (_dragging_global_gain)
		{
			if (base.Capture)
			{
				double globalGainDb = dbFromY(plotRect, e.Location.Y);
				GlobalGainDb = globalGainDb;
			}
		}
		else if (_dragging_point)
		{
			if (!base.Capture || _drag_index < 0 || _drag_index >= _points.Count)
			{
				return;
			}
			EqPoint eqPoint = _points[_drag_index];
			double frequencyHz = eqPoint.FrequencyHz;
			double gainDb = eqPoint.GainDb;
			double q = eqPoint.Q;
			double num = eqPoint.FrequencyHz;
			double v = dbFromY(plotRect, e.Location.Y);
			v = clamp(v, _db_min, _db_max);
			if (!isFrequencyLockedIndex(_drag_index))
			{
				num = freqFromX(plotRect, e.Location.X);
				num = clamp(num, _frequency_min_hz, _frequency_max_hz);
				if (!_allow_point_reorder)
				{
					double num2;
					double num3;
					if (_drag_index == 0)
					{
						num2 = _frequency_min_hz;
						num3 = _points[1].FrequencyHz - _min_point_spacing_hz;
					}
					else if (_drag_index == _points.Count - 1)
					{
						num2 = _points[_points.Count - 2].FrequencyHz + _min_point_spacing_hz;
						num3 = _frequency_max_hz;
					}
					else
					{
						num2 = _points[_drag_index - 1].FrequencyHz + _min_point_spacing_hz;
						num3 = _points[_drag_index + 1].FrequencyHz - _min_point_spacing_hz;
					}
					if (num3 < num2)
					{
						num3 = num2;
					}
					num = clamp(num, num2, num3);
				}
			}
			bool flag = false;
			if (Math.Abs(eqPoint.FrequencyHz - num) > 1E-06)
			{
				eqPoint.FrequencyHz = num;
				flag = true;
			}
			if (Math.Abs(eqPoint.GainDb - v) > 1E-06)
			{
				eqPoint.GainDb = v;
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			if (_allow_point_reorder && !isFrequencyLockedIndex(_drag_index))
			{
				enforceOrdering(enforce_spacing_all: false);
				int drag_index = _drag_index;
				double num4 = _frequency_min_hz;
				double num5 = _frequency_max_hz;
				if (_points.Count > 1)
				{
					if (drag_index > 0)
					{
						num4 = _points[drag_index - 1].FrequencyHz + _min_point_spacing_hz;
					}
					if (drag_index < _points.Count - 1)
					{
						num5 = _points[drag_index + 1].FrequencyHz - _min_point_spacing_hz;
					}
					if (num5 < num4)
					{
						num5 = num4;
					}
				}
				double num6 = clamp(eqPoint.FrequencyHz, num4, num5);
				if (Math.Abs(num6 - eqPoint.FrequencyHz) > 1E-06)
				{
					eqPoint.FrequencyHz = num6;
				}
				enforceOrdering(enforce_spacing_all: false);
			}
			_drag_dirty_point = true;
			raisePointsChanged(is_dragging: true);
			if (Math.Abs(eqPoint.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(eqPoint.GainDb - gainDb) > 1E-06 || Math.Abs(eqPoint.Q - q) > 1E-06)
			{
				raisePointDataChangedForPoint(eqPoint, is_dragging: true);
			}
			Invalidate();
		}
		else
		{
			bool flag2 = false;
			if (hitTestGlobalGainHandle(plotRect, e.Location))
			{
				flag2 = true;
			}
			else if (plotRect.Contains(e.Location) && hitTestPoint(plotRect, e.Location) >= 0)
			{
				flag2 = true;
			}
			Cursor = (flag2 ? Cursors.Hand : Cursors.Default);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		bool dragging_point = _dragging_point;
		bool dragging_global_gain = _dragging_global_gain;
		EqPoint drag_point_ref = _drag_point_ref;
		bool drag_dirty_point = _drag_dirty_point;
		bool drag_dirty_global_gain = _drag_dirty_global_gain;
		bool drag_dirty_selected_index = _drag_dirty_selected_index;
		if (base.Capture)
		{
			base.Capture = false;
		}
		_dragging_global_gain = false;
		_dragging_point = false;
		_drag_index = -1;
		_drag_point_ref = null;
		_drag_dirty_point = false;
		_drag_dirty_global_gain = false;
		_drag_dirty_selected_index = false;
		if (dragging_point & drag_dirty_point)
		{
			raisePointsChanged(is_dragging: false);
			if (drag_point_ref != null)
			{
				raisePointDataChangedForPoint(drag_point_ref, is_dragging: false);
			}
		}
		if (dragging_global_gain & drag_dirty_global_gain)
		{
			raiseGlobalGainChanged(is_dragging: false);
		}
		if (drag_dirty_selected_index)
		{
			raiseSelectedIndexChanged(is_dragging: false);
		}
		Invalidate();
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		bool is_dragging = isDraggingNow();
		Rectangle plotRect = getPlotRect();
		double num = (double)e.Delta / 120.0;
		if (num == 0.0)
		{
			return;
		}
		if (_selected_index < 0 || _selected_index >= _points.Count)
		{
			if (hitTestGlobalGainHandle(plotRect, e.Location))
			{
				GlobalGainDb = clamp(_global_gain_db + num * 0.5, _db_min, _db_max);
			}
		}
		else
		{
			if (!plotRect.Contains(e.Location))
			{
				return;
			}
			EqPoint eqPoint = _points[_selected_index];
			double frequencyHz = eqPoint.FrequencyHz;
			double gainDb = eqPoint.GainDb;
			double q = eqPoint.Q;
			bool num2 = (Control.ModifierKeys & Keys.Control) == Keys.Control;
			bool flag = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
			if (num2)
			{
				if (isFrequencyLockedIndex(_selected_index))
				{
					return;
				}
				double num3 = chooseFrequencyStep(_frequency_max_hz - _frequency_min_hz) / 5.0;
				if (num3 < 1.0)
				{
					num3 = 1.0;
				}
				double v = eqPoint.FrequencyHz + num * num3;
				v = clamp(v, _frequency_min_hz, _frequency_max_hz);
				if (!_allow_point_reorder)
				{
					double num4;
					double num5;
					if (_selected_index == 0)
					{
						num4 = _frequency_min_hz;
						num5 = _points[1].FrequencyHz - _min_point_spacing_hz;
					}
					else if (_selected_index == _points.Count - 1)
					{
						num4 = _points[_points.Count - 2].FrequencyHz + _min_point_spacing_hz;
						num5 = _frequency_max_hz;
					}
					else
					{
						num4 = _points[_selected_index - 1].FrequencyHz + _min_point_spacing_hz;
						num5 = _points[_selected_index + 1].FrequencyHz - _min_point_spacing_hz;
					}
					if (num5 < num4)
					{
						num5 = num4;
					}
					v = clamp(v, num4, num5);
				}
				if (!(Math.Abs(v - eqPoint.FrequencyHz) > 1E-06))
				{
					return;
				}
				eqPoint.FrequencyHz = v;
				if (_allow_point_reorder)
				{
					enforceOrdering(enforce_spacing_all: false);
					int selected_index = _selected_index;
					double num6 = _frequency_min_hz;
					double num7 = _frequency_max_hz;
					if (_points.Count > 1)
					{
						if (selected_index > 0)
						{
							num6 = _points[selected_index - 1].FrequencyHz + _min_point_spacing_hz;
						}
						if (selected_index < _points.Count - 1)
						{
							num7 = _points[selected_index + 1].FrequencyHz - _min_point_spacing_hz;
						}
						if (num7 < num6)
						{
							num7 = num6;
						}
					}
					double num8 = clamp(eqPoint.FrequencyHz, num6, num7);
					if (Math.Abs(num8 - eqPoint.FrequencyHz) > 1E-06)
					{
						eqPoint.FrequencyHz = num8;
					}
					enforceOrdering(enforce_spacing_all: false);
				}
				raisePointsChanged(is_dragging: false);
				if (Math.Abs(eqPoint.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(eqPoint.GainDb - gainDb) > 1E-06 || Math.Abs(eqPoint.Q - q) > 1E-06)
				{
					raisePointDataChangedForPoint(eqPoint, is_dragging);
				}
				Invalidate();
				return;
			}
			if (!_parametric_eq)
			{
				double num9 = clamp(eqPoint.GainDb + num * 0.5, _db_min, _db_max);
				if (Math.Abs(num9 - eqPoint.GainDb) > 1E-06)
				{
					eqPoint.GainDb = num9;
					raisePointsChanged(is_dragging: false);
					if (Math.Abs(eqPoint.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(eqPoint.GainDb - gainDb) > 1E-06 || Math.Abs(eqPoint.Q - q) > 1E-06)
					{
						raisePointDataChangedForPoint(eqPoint, is_dragging);
					}
					Invalidate();
				}
				return;
			}
			if (flag)
			{
				double num10 = clamp(eqPoint.GainDb + num * 0.5, _db_min, _db_max);
				if (Math.Abs(num10 - eqPoint.GainDb) > 1E-06)
				{
					eqPoint.GainDb = num10;
					raisePointsChanged(is_dragging: false);
					if (Math.Abs(eqPoint.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(eqPoint.GainDb - gainDb) > 1E-06 || Math.Abs(eqPoint.Q - q) > 1E-06)
					{
						raisePointDataChangedForPoint(eqPoint, is_dragging);
					}
					Invalidate();
				}
				return;
			}
			double num11 = Math.Pow(1.12, num);
			double num12 = clamp(eqPoint.Q * num11, _q_min, _q_max);
			if (Math.Abs(num12 - eqPoint.Q) > 1E-06)
			{
				eqPoint.Q = num12;
				raisePointsChanged(is_dragging);
				if (Math.Abs(eqPoint.FrequencyHz - frequencyHz) > 1E-06 || Math.Abs(eqPoint.GainDb - gainDb) > 1E-06 || Math.Abs(eqPoint.Q - q) > 1E-06)
				{
					raisePointDataChangedForPoint(eqPoint, is_dragging);
				}
				Invalidate();
			}
		}
	}

	private bool isDraggingNow()
	{
		if (!_dragging_global_gain)
		{
			return _dragging_point;
		}
		return true;
	}

	private int getAxisLabelMaxWidth()
	{
		double yAxisStepDb = getYAxisStepDb();
		string obj = formatDbTick(_db_min, yAxisStepDb);
		string text = formatDbTick(_db_max, yAxisStepDb);
		Size size = TextRenderer.MeasureText(obj, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
		Size size2 = TextRenderer.MeasureText(text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
		return Math.Max(size.Width, size2.Width);
	}

	private int getComputedPlotMarginLeft()
	{
		int num = _plot_margin_left;
		if (_show_axis_scales)
		{
			int axisLabelMaxWidth = getAxisLabelMaxWidth();
			int num2 = _axis_tick_length + 4 + axisLabelMaxWidth + 8;
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (num < 10)
		{
			num = 10;
		}
		return num;
	}

	private int getComputedPlotMarginRight()
	{
		int num = _plot_margin_right;
		int num2 = _global_handle_x_offset + _global_handle_size * 2 + _global_hit_extra + 6;
		if (num2 > num)
		{
			num = num2;
		}
		if (num < 10)
		{
			num = 10;
		}
		return num;
	}

	private Rectangle getPlotRect()
	{
		Rectangle clientRectangle = base.ClientRectangle;
		int computedPlotMarginLeft = getComputedPlotMarginLeft();
		int computedPlotMarginRight = getComputedPlotMarginRight();
		int computedPlotMarginBottom = getComputedPlotMarginBottom();
		int num = clientRectangle.X + computedPlotMarginLeft;
		int num2 = clientRectangle.Y + _plot_margin_top;
		int num3 = clientRectangle.Width - computedPlotMarginLeft - computedPlotMarginRight;
		int num4 = clientRectangle.Height - _plot_margin_top - computedPlotMarginBottom;
		if (num3 < 1)
		{
			num3 = 1;
		}
		if (num4 < 1)
		{
			num4 = 1;
		}
		return new Rectangle(num, num2, num3, num4);
	}

	private void drawGrid(Graphics g, Rectangle plot)
	{
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(18, 18, 18)))
		{
			g.FillRectangle(brush, plot);
		}
		drawBarChart(g, plot);
		using (Pen pen = new Pen(Color.FromArgb(45, 45, 45), 1f))
		{
			if (_log_scale)
			{
				List<double> logFrequencyTicks = getLogFrequencyTicks(plot);
				for (int i = 0; i < logFrequencyTicks.Count; i++)
				{
					float num = xFromFreq(plot, logFrequencyTicks[i]);
					g.DrawLine(pen, num, plot.Top, num, plot.Bottom);
				}
			}
			else
			{
				int num2 = 10;
				for (int j = 0; j <= num2; j++)
				{
					float num3 = (float)j / (float)num2;
					int num4 = plot.Left + (int)Math.Round(num3 * (float)plot.Width);
					g.DrawLine(pen, num4, plot.Top, num4, plot.Bottom);
				}
			}
			double yAxisStepDb = getYAxisStepDb();
			for (double num5 = Math.Ceiling(_db_min / yAxisStepDb) * yAxisStepDb; num5 <= _db_max + 1E-06; num5 += yAxisStepDb)
			{
				float num6 = yFromDb(plot, num5);
				g.DrawLine(pen, plot.Left, num6, plot.Right, num6);
			}
		}
		using (Pen pen2 = new Pen(Color.FromArgb(75, 75, 75), 1.5f))
		{
			float num7 = yFromDb(plot, 0.0);
			g.DrawLine(pen2, plot.Left, num7, plot.Right, num7);
		}
		if (_global_gain_is_horiz_line)
		{
			using (Pen pen3 = new Pen(Color.FromArgb(180, Color.White), 1.5f))
			{
				pen3.DashStyle = DashStyle.Dash;
				float num8 = yFromDb(plot, _global_gain_db);
				g.DrawLine(pen3, plot.Left, num8, plot.Right, num8);
			}
		}
	}

	private void drawBarChart(Graphics g, Rectangle plot)
	{
		if (_bar_chart_data == null || _bar_chart_data.Length == 0)
		{
			return;
		}
		applyBarChartPeakDecay(DateTime.UtcNow);
		using SolidBrush brush = new SolidBrush(Color.FromArgb(_bar_chart_fill_alpha, _bar_chart_fill_color.R, _bar_chart_fill_color.G, _bar_chart_fill_color.B));
		using Pen pen = new Pen(Color.FromArgb(_bar_chart_peak_alpha, _bar_chart_peak_color.R, _bar_chart_peak_color.G, _bar_chart_peak_color.B), 1f);
		int num = _bar_chart_data.Length;
		for (int i = 0; i < num; i++)
		{
			int num2;
			int num3;
			if (_log_scale)
			{
				double frequency_hz = frequencyFromNormalizedPosition((double)i / (double)num);
				double frequency_hz2 = frequencyFromNormalizedPosition((double)(i + 1) / (double)num);
				num2 = (int)Math.Round(xFromFreq(plot, frequency_hz));
				num3 = (int)Math.Round(xFromFreq(plot, frequency_hz2));
			}
			else
			{
				num2 = plot.Left + (int)Math.Round((double)i * (double)plot.Width / (double)num);
				num3 = plot.Left + (int)Math.Round((double)(i + 1) * (double)plot.Width / (double)num);
			}
			int num4 = num3 - num2;
			if (num4 <= 0)
			{
				continue;
			}
			int num5 = ((num4 > 1) ? (num4 - 1) : num4);
			if (num5 < 1)
			{
				num5 = 1;
			}
			double db = clamp(_bar_chart_data[i], _db_min, _db_max);
			int num6 = (int)Math.Round(yFromDb(plot, db));
			int num7 = plot.Bottom - num6;
			if (num7 < 1)
			{
				num7 = 1;
			}
			if (num6 < plot.Top)
			{
				num7 -= plot.Top - num6;
				num6 = plot.Top;
			}
			if (num6 + num7 > plot.Bottom)
			{
				num7 = plot.Bottom - num6;
			}
			if (num7 > 0)
			{
				g.FillRectangle(brush, num2, num6, num5, num7);
			}
			if (_bar_chart_peak_hold_enabled && _bar_chart_peak_data != null && i < _bar_chart_peak_data.Length)
			{
				double db2 = clamp(_bar_chart_peak_data[i], _db_min, _db_max);
				int num8 = (int)Math.Round(yFromDb(plot, db2));
				if (num8 < plot.Top)
				{
					num8 = plot.Top;
				}
				if (num8 > plot.Bottom - 1)
				{
					num8 = plot.Bottom - 1;
				}
				int num9 = num2 + num5 - 1;
				if (num9 < num2)
				{
					num9 = num2;
				}
				g.DrawLine(pen, num2, num8, num9, num8);
			}
		}
	}

	private void drawBandShading(Graphics g, Rectangle plot)
	{
		if (_points.Count == 0)
		{
			return;
		}
		if (_parametric_eq)
		{
			int num = plot.Width;
			if (num < 64)
			{
				num = 64;
			}
			double num2 = _frequency_max_hz - _frequency_min_hz;
			if (num2 <= 0.0)
			{
				num2 = 1.0;
			}
			float num3 = yFromDb(plot, 0.0);
			for (int i = 0; i < _points.Count; i++)
			{
				EqPoint eqPoint = _points[i];
				if (Math.Abs(eqPoint.GainDb) < 1E-06)
				{
					continue;
				}
				double num4 = clamp(eqPoint.Q, _q_min, _q_max);
				double num5 = num2 / (num4 * 3.0);
				double num6 = num2 / 6000.0;
				if (num5 < num6)
				{
					num5 = num6;
				}
				double num7 = num5 / 2.3548200450309493;
				Color color = eqPoint.BandColor;
				if (color == Color.Empty)
				{
					color = getBandBaseColor(i);
				}
				Color color2 = Color.FromArgb(_band_shade_alpha, color.R, color.G, color.B);
				if (!_use_per_band_colours)
				{
					color2 = Color.FromArgb(_band_shade_alpha, _band_shade_color.R, _band_shade_color.G, _band_shade_color.B);
				}
				PointF[] array = new PointF[num + 2];
				array[0] = new PointF(plot.Left, num3);
				for (int j = 0; j < num; j++)
				{
					double num8 = (double)j / (double)(num - 1);
					double num9 = _frequency_min_hz + num8 * num2;
					double num10 = (num9 - eqPoint.FrequencyHz) / num7;
					double num11 = Math.Exp(-0.5 * num10 * num10);
					double db = 0.0;
					if (num11 >= _band_shade_weight_cutoff)
					{
						db = eqPoint.GainDb * num11;
					}
					float num12 = (_log_scale ? xFromFreq(plot, num9) : ((float)plot.Left + (float)(num8 * (double)plot.Width)));
					float num13 = yFromDb(plot, db);
					array[j + 1] = new PointF(num12, num13);
				}
				array[num + 1] = new PointF(plot.Right, num3);
				using SolidBrush brush = new SolidBrush(color2);
				g.FillPolygon(brush, array, FillMode.Winding);
			}
			return;
		}
		float num14 = yFromDb(plot, 0.0);
		double frequency_hz = _frequency_min_hz;
		double db2 = 0.0;
		Color empty = Color.Empty;
		if (_points.Count > 0)
		{
			empty = _points[0].BandColor;
			if (empty == Color.Empty)
			{
				empty = getBandBaseColor(0);
			}
		}
		else
		{
			empty = _band_shade_color;
		}
		for (int k = 0; k <= _points.Count; k++)
		{
			double num15;
			double num16;
			Color color3;
			if (k < _points.Count)
			{
				EqPoint eqPoint2 = _points[k];
				num15 = eqPoint2.FrequencyHz;
				num16 = eqPoint2.GainDb;
				color3 = eqPoint2.BandColor;
				if (color3 == Color.Empty)
				{
					color3 = getBandBaseColor(k);
				}
			}
			else
			{
				num15 = _frequency_max_hz;
				num16 = 0.0;
				if (_points.Count > 0)
				{
					color3 = _points[_points.Count - 1].BandColor;
					if (color3 == Color.Empty)
					{
						color3 = getBandBaseColor(_points.Count - 1);
					}
				}
				else
				{
					color3 = _band_shade_color;
				}
			}
			if (num15 < _frequency_min_hz)
			{
				num15 = _frequency_min_hz;
			}
			if (num15 > _frequency_max_hz)
			{
				num15 = _frequency_max_hz;
			}
			float num17 = xFromFreq(plot, frequency_hz);
			float num18 = xFromFreq(plot, num15);
			if (Math.Abs(num18 - num17) < 0.5f)
			{
				frequency_hz = num15;
				db2 = num16;
				empty = color3;
				continue;
			}
			float num19 = yFromDb(plot, db2);
			float num20 = yFromDb(plot, num16);
			PointF[] points = new PointF[4]
			{
				new PointF(num17, num14),
				new PointF(num17, num19),
				new PointF(num18, num20),
				new PointF(num18, num14)
			};
			Color color4 = empty;
			Color color5 = color3;
			if (!_use_per_band_colours)
			{
				color4 = _band_shade_color;
				color5 = _band_shade_color;
			}
			Color color6 = Color.FromArgb(_band_shade_alpha, color4.R, color4.G, color4.B);
			Color color7 = Color.FromArgb(_band_shade_alpha, color5.R, color5.G, color5.B);
			using (LinearGradientBrush brush2 = new LinearGradientBrush(new PointF(num17, 0f), new PointF(num18, 0f), color6, color7))
			{
				g.FillPolygon(brush2, points, FillMode.Winding);
			}
			frequency_hz = num15;
			db2 = num16;
			empty = color3;
		}
	}

	private void drawCurve(Graphics g, Rectangle plot)
	{
		if (_points.Count == 0)
		{
			return;
		}
		int num = plot.Width;
		if (num < 64)
		{
			num = 64;
		}
		PointF[] array = new PointF[num];
		for (int i = 0; i < num; i++)
		{
			double num2 = (double)i / (double)(num - 1);
			double frequency_hz = (_log_scale ? frequencyFromNormalizedPosition(num2) : (_frequency_min_hz + num2 * (_frequency_max_hz - _frequency_min_hz)));
			double num3 = responseDbAtFrequency(frequency_hz);
			if (!_global_gain_is_horiz_line)
			{
				num3 += _global_gain_db;
			}
			float num4 = (_log_scale ? xFromFreq(plot, frequency_hz) : ((float)plot.Left + (float)(num2 * (double)plot.Width)));
			float num5 = yFromDb(plot, num3);
			array[i] = new PointF(num4, num5);
		}
		using Pen pen = new Pen(Color.White, 2f);
		g.DrawLines(pen, array);
	}

	private void drawGlobalGainHandle(Graphics g, Rectangle plot)
	{
		float num = yFromDb(plot, _global_gain_db);
		int num2 = plot.Right + _global_handle_x_offset;
		int global_handle_size = _global_handle_size;
		Point[] points = new Point[3]
		{
			new Point(num2, (int)Math.Round(num)),
			new Point(num2 + global_handle_size, (int)Math.Round(num) - global_handle_size),
			new Point(num2 + global_handle_size, (int)Math.Round(num) + global_handle_size)
		};
		using (SolidBrush brush = new SolidBrush(Color.White))
		{
			g.FillPolygon(brush, points);
		}
		using Pen pen = new Pen(Color.FromArgb(40, 40, 40), 1f);
		g.DrawPolygon(pen, points);
	}

	private void drawPoints(Graphics g, Rectangle plot)
	{
		for (int i = 0; i < _points.Count; i++)
		{
			EqPoint eqPoint = _points[i];
			float num = xFromFreq(plot, eqPoint.FrequencyHz);
			float num2 = yFromDb(plot, eqPoint.GainDb);
			bool num3 = i == _selected_index;
			Color pointDisplayColor = getPointDisplayColor(i);
			float num4 = _point_radius;
			if (num3)
			{
				num4++;
			}
			using (SolidBrush brush = new SolidBrush(pointDisplayColor))
			{
				g.FillEllipse(brush, num - num4, num2 - num4, num4 * 2f, num4 * 2f);
			}
			using (Pen pen = new Pen(Color.FromArgb(35, 35, 35), 1f))
			{
				g.DrawEllipse(pen, num - num4, num2 - num4, num4 * 2f, num4 * 2f);
			}
			if (_show_dot_readings && _dragging_point && i == _drag_index)
			{
				drawDotReading(g, plot, eqPoint, num, num2, num4);
			}
		}
	}

	private void drawDotReading(Graphics g, Rectangle plot, EqPoint p, float dot_x, float dot_y, float dot_radius)
	{
		string text = "F " + formatDotReadingHz(p.FrequencyHz) + "   " + (_show_dot_readings_as_comp ? "C" : "G") + " " + formatDotReadingDb(p.GainDb);
		if (_parametric_eq)
		{
			text = text + "   Q " + p.Q.ToString("0.00");
		}
		Size size = TextRenderer.MeasureText(text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
		int num = 8;
		int num2 = 5;
		int num3 = 8;
		float num4 = 8f;
		float num5 = 14f;
		float num6 = 2f;
		float num7 = (float)plot.Left + (float)plot.Width * 0.5f;
		bool flag = dot_x >= num7;
		RectangleF rect = new RectangleF(0f, 0f, size.Width + num * 2, size.Height + num2 * 2);
		rect.X = dot_x - rect.Width * 0.5f;
		rect.Y = dot_y - dot_radius - num4 - rect.Height;
		if (rect.Y < (float)plot.Top + num6)
		{
			rect.Y = dot_y + dot_radius + num4;
			rect.X = (flag ? (dot_x - num5 - rect.Width) : (dot_x + num5));
		}
		if (rect.Bottom > (float)plot.Bottom - num6)
		{
			rect.Y = dot_y - dot_radius - num4 - rect.Height;
			rect.X = dot_x - rect.Width * 0.5f;
		}
		if (rect.X < (float)plot.Left + num6)
		{
			rect.X = (float)plot.Left + num6;
		}
		if (rect.Right > (float)plot.Right - num6)
		{
			rect.X = (float)plot.Right - num6 - rect.Width;
		}
		if (rect.Y < (float)plot.Top + num6)
		{
			rect.Y = (float)plot.Top + num6;
		}
		if (rect.Bottom > (float)plot.Bottom - num6)
		{
			rect.Y = (float)plot.Bottom - num6 - rect.Height;
		}
		using GraphicsPath path = createRoundedRectPath(rect, num3);
		using SolidBrush brush = new SolidBrush(Color.FromArgb(150, 18, 18, 18));
		using Pen pen = new Pen(Color.FromArgb(90, 255, 220, 120), 1f);
		using SolidBrush brush2 = new SolidBrush(Color.FromArgb(255, 235, 90));
		g.FillPath(brush, path);
		g.DrawPath(pen, path);
		g.DrawString(text, Font, brush2, rect.X + (float)num, rect.Y + (float)num2);
	}

	private GraphicsPath createRoundedRectPath(RectangleF rect, float radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = radius * 2f;
		if (num > rect.Width)
		{
			num = rect.Width;
		}
		if (num > rect.Height)
		{
			num = rect.Height;
		}
		if (num < 2f)
		{
			graphicsPath.AddRectangle(rect);
			return graphicsPath;
		}
		RectangleF rect2 = new RectangleF(rect.X, rect.Y, num, num);
		graphicsPath.AddArc(rect2, 180f, 90f);
		rect2.X = rect.Right - num;
		graphicsPath.AddArc(rect2, 270f, 90f);
		rect2.Y = rect.Bottom - num;
		graphicsPath.AddArc(rect2, 0f, 90f);
		rect2.X = rect.X;
		graphicsPath.AddArc(rect2, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	private void drawAxisScales(Graphics g, Rectangle plot)
	{
		using Pen pen = new Pen(_axis_tick_color, 1f);
		using SolidBrush brush = new SolidBrush(_axis_text_color);
		double yAxisStepDb = getYAxisStepDb();
		double num = Math.Ceiling(_db_min / yAxisStepDb) * yAxisStepDb;
		bool flag = false;
		bool flag2 = false;
		for (double num2 = num; num2 <= _db_max + 1E-06; num2 += yAxisStepDb)
		{
			if (Math.Abs(num2 - _db_min) < 1E-06)
			{
				flag = true;
			}
			if (Math.Abs(num2 - _db_max) < 1E-06)
			{
				flag2 = true;
			}
			float num3 = yFromDb(plot, num2);
			g.DrawLine(pen, plot.Left - _axis_tick_length, num3, plot.Left, num3);
			string s = formatDbTick(num2, yAxisStepDb);
			SizeF sizeF = g.MeasureString(s, Font);
			float num4 = (float)(plot.Left - _axis_tick_length) - 4f - sizeF.Width;
			float num5 = num3 - sizeF.Height * 0.5f;
			g.DrawString(s, Font, brush, num4, num5);
		}
		if (!flag)
		{
			double db_min = _db_min;
			float num6 = yFromDb(plot, db_min);
			g.DrawLine(pen, plot.Left - _axis_tick_length, num6, plot.Left, num6);
			string s2 = formatDbTick(db_min, yAxisStepDb);
			SizeF sizeF2 = g.MeasureString(s2, Font);
			float num7 = (float)(plot.Left - _axis_tick_length) - 4f - sizeF2.Width;
			float num8 = num6 - sizeF2.Height * 0.5f;
			g.DrawString(s2, Font, brush, num7, num8);
		}
		if (!flag2)
		{
			double db_max = _db_max;
			float num9 = yFromDb(plot, db_max);
			g.DrawLine(pen, plot.Left - _axis_tick_length, num9, plot.Left, num9);
			string s3 = formatDbTick(db_max, yAxisStepDb);
			SizeF sizeF3 = g.MeasureString(s3, Font);
			float num10 = (float)(plot.Left - _axis_tick_length) - 4f - sizeF3.Width;
			float num11 = num9 - sizeF3.Height * 0.5f;
			g.DrawString(s3, Font, brush, num10, num11);
		}
		double num12 = _frequency_max_hz - _frequency_min_hz;
		if (num12 <= 0.0)
		{
			num12 = 1.0;
		}
		float y = plot.Bottom;
		float y2 = plot.Bottom + _axis_tick_length;
		float num13 = (float)(plot.Bottom + _axis_tick_length) + 2f;
		if (_log_scale)
		{
			List<double> logFrequencyTicks = getLogFrequencyTicks(plot);
			for (int i = 0; i < logFrequencyTicks.Count; i++)
			{
				double num14 = logFrequencyTicks[i];
				float num15 = xFromFreq(plot, num14);
				g.DrawLine(pen, num15, y, num15, y2);
				string s4 = formatHzTick(num14);
				float num16 = num15 - g.MeasureString(s4, Font).Width * 0.5f;
				float num17 = num13;
				g.DrawString(s4, Font, brush, num16, num17);
			}
		}
		else
		{
			double num18 = chooseFrequencyStep(num12);
			for (double num19 = Math.Ceiling(_frequency_min_hz / num18) * num18; num19 <= _frequency_max_hz + 1E-06; num19 += num18)
			{
				float num20 = xFromFreq(plot, num19);
				g.DrawLine(pen, num20, y, num20, y2);
				string s5 = formatHzTick(num19);
				float num21 = num20 - g.MeasureString(s5, Font).Width * 0.5f;
				float num22 = num13;
				g.DrawString(s5, Font, brush, num21, num22);
			}
		}
	}

	private string formatDbTick(double db, double step_db)
	{
		string text = "0";
		double num = Math.Abs(step_db);
		if (num < 1.0)
		{
			text = "0.##";
		}
		else if (Math.Abs(num - Math.Round(num)) > 1E-06)
		{
			text = "0.#";
		}
		if (Math.Abs(db) < 1E-06)
		{
			return "0";
		}
		if (db > 0.0)
		{
			return "+" + db.ToString(text);
		}
		return db.ToString(text);
	}

	private string formatHzTick(double hz)
	{
		double num = Math.Abs(hz);
		if (num >= 1000.0)
		{
			double value = hz / 1000.0;
			if (Math.Abs(value) >= 10.0)
			{
				return value.ToString("0.#") + "k";
			}
			return value.ToString("0.##") + "k";
		}
		if (num >= 100.0)
		{
			return hz.ToString("0");
		}
		if (num >= 10.0)
		{
			return hz.ToString("0.#");
		}
		return hz.ToString("0.##");
	}

	private double chooseFrequencyStep(double span)
	{
		if (span <= 300.0)
		{
			return 25.0;
		}
		if (span <= 600.0)
		{
			return 50.0;
		}
		if (span <= 1200.0)
		{
			return 100.0;
		}
		if (span <= 2500.0)
		{
			return 250.0;
		}
		if (span <= 6000.0)
		{
			return 500.0;
		}
		if (span <= 12000.0)
		{
			return 1000.0;
		}
		if (span <= 24000.0)
		{
			return 2000.0;
		}
		return 5000.0;
	}

	private void drawBorder(Graphics g, Rectangle plot)
	{
		using Pen pen = new Pen(Color.FromArgb(70, 70, 70), 1f);
		g.DrawRectangle(pen, plot);
	}

	private void drawReadout(Graphics g, Rectangle plot)
	{
		int num = base.ClientRectangle.Bottom - (Font.Height + 4);
		int left = plot.Left;
		string text;
		if (_selected_index >= 0 && _selected_index < _points.Count)
		{
			EqPoint eqPoint = _points[_selected_index];
			int bandId = eqPoint.BandId;
			if (_parametric_eq)
			{
				text = "P" + bandId + "  F " + formatHz(eqPoint.FrequencyHz) + "  G " + formatDb(eqPoint.GainDb) + "  Q " + eqPoint.Q.ToString("0.00");
				text = text + "     Global " + formatDb(_global_gain_db);
			}
			else
			{
				text = "P" + bandId + "  F " + formatHz(eqPoint.FrequencyHz) + "  G " + formatDb(eqPoint.GainDb);
				text = text + "     Global " + formatDb(_global_gain_db);
			}
		}
		else
		{
			text = "Global " + formatDb(_global_gain_db);
		}
		using SolidBrush brush = new SolidBrush(ForeColor);
		g.DrawString(text, Font, brush, left, num);
	}

	private double responseDbAtFrequency(double frequency_hz)
	{
		if (!_parametric_eq)
		{
			if (_points.Count == 0)
			{
				return 0.0;
			}
			if (frequency_hz <= _points[0].FrequencyHz)
			{
				return _points[0].GainDb;
			}
			if (frequency_hz >= _points[_points.Count - 1].FrequencyHz)
			{
				return _points[_points.Count - 1].GainDb;
			}
			for (int i = 1; i < _points.Count; i++)
			{
				EqPoint eqPoint = _points[i - 1];
				EqPoint eqPoint2 = _points[i];
				if (frequency_hz <= eqPoint2.FrequencyHz)
				{
					double num = eqPoint2.FrequencyHz - eqPoint.FrequencyHz;
					if (num <= 1E-07)
					{
						return eqPoint2.GainDb;
					}
					double num2 = (frequency_hz - eqPoint.FrequencyHz) / num;
					if (num2 < 0.0)
					{
						num2 = 0.0;
					}
					if (num2 > 1.0)
					{
						num2 = 1.0;
					}
					return eqPoint.GainDb + (eqPoint2.GainDb - eqPoint.GainDb) * num2;
				}
			}
			return _points[_points.Count - 1].GainDb;
		}
		double num3 = _frequency_max_hz - _frequency_min_hz;
		if (num3 <= 0.0)
		{
			num3 = 1.0;
		}
		double num4 = 0.0;
		for (int j = 0; j < _points.Count; j++)
		{
			EqPoint eqPoint3 = _points[j];
			double num5 = clamp(eqPoint3.Q, _q_min, _q_max);
			double num6 = num3 / (num5 * 3.0);
			double num7 = num3 / 6000.0;
			if (num6 < num7)
			{
				num6 = num7;
			}
			double num8 = num6 / 2.3548200450309493;
			double num9 = (frequency_hz - eqPoint3.FrequencyHz) / num8;
			double num10 = Math.Exp(-0.5 * num9 * num9);
			num4 += eqPoint3.GainDb * num10;
		}
		return num4;
	}

	private void barChartPeakTimer_Tick(object sender, EventArgs e)
	{
		if (_bar_chart_data == null || _bar_chart_data.Length == 0)
		{
			updateBarChartPeakTimerState();
			return;
		}
		if (!_bar_chart_peak_hold_enabled)
		{
			updateBarChartPeakTimerState();
			return;
		}
		applyBarChartPeakDecay(DateTime.UtcNow);
		Invalidate();
	}

	private void applyBarChartPeakDecay(DateTime now_utc)
	{
		if (_bar_chart_data == null || _bar_chart_peak_data == null || _bar_chart_peak_hold_until_ticks == null)
		{
			_bar_chart_peak_last_update_utc = now_utc;
			return;
		}
		if (_bar_chart_peak_data.Length != _bar_chart_data.Length || _bar_chart_peak_hold_until_ticks.Length != _bar_chart_data.Length)
		{
			syncBarChartPeaksToData();
			_bar_chart_peak_last_update_utc = now_utc;
			return;
		}
		double num = (now_utc - _bar_chart_peak_last_update_utc).TotalSeconds;
		if (num < 0.0)
		{
			num = 0.0;
		}
		_bar_chart_peak_last_update_utc = now_utc;
		if (!_bar_chart_peak_hold_enabled)
		{
			syncBarChartPeaksToData();
		}
		else
		{
			if (num <= 0.0)
			{
				return;
			}
			double num2 = _bar_chart_peak_decay_db_per_second * num;
			long ticks = now_utc.Ticks;
			for (int i = 0; i < _bar_chart_data.Length; i++)
			{
				double num3 = clamp(_bar_chart_data[i], _db_min, _db_max);
				if (_bar_chart_peak_data[i] < num3)
				{
					_bar_chart_peak_data[i] = num3;
				}
				else if (ticks > _bar_chart_peak_hold_until_ticks[i])
				{
					double num4 = _bar_chart_peak_data[i] - num2;
					if (num4 < num3)
					{
						num4 = num3;
					}
					_bar_chart_peak_data[i] = num4;
				}
			}
		}
	}

	private void syncBarChartPeaksToData()
	{
		if (_bar_chart_data == null)
		{
			_bar_chart_peak_data = null;
			_bar_chart_peak_hold_until_ticks = null;
			return;
		}
		if (_bar_chart_peak_data == null || _bar_chart_peak_data.Length != _bar_chart_data.Length)
		{
			_bar_chart_peak_data = new double[_bar_chart_data.Length];
		}
		if (_bar_chart_peak_hold_until_ticks == null || _bar_chart_peak_hold_until_ticks.Length != _bar_chart_data.Length)
		{
			_bar_chart_peak_hold_until_ticks = new long[_bar_chart_data.Length];
		}
		long ticks = DateTime.UtcNow.AddMilliseconds(_bar_chart_peak_hold_ms).Ticks;
		for (int i = 0; i < _bar_chart_data.Length; i++)
		{
			_bar_chart_peak_data[i] = clamp(_bar_chart_data[i], _db_min, _db_max);
			_bar_chart_peak_hold_until_ticks[i] = ticks;
		}
	}

	private void updateBarChartPeakTimerState()
	{
		if (_bar_chart_data != null && _bar_chart_data.Length != 0 && _bar_chart_peak_hold_enabled)
		{
			if (!_bar_chart_peak_timer.Enabled)
			{
				_bar_chart_peak_last_update_utc = DateTime.UtcNow;
				_bar_chart_peak_timer.Start();
			}
		}
		else if (_bar_chart_peak_timer.Enabled)
		{
			_bar_chart_peak_timer.Stop();
		}
	}

	private Color getBandBaseColor(int index)
	{
		int num = _default_band_palette.Length;
		if (num <= 0)
		{
			return Color.FromArgb(200, 200, 200);
		}
		int num2 = index % num;
		if (num2 < 0)
		{
			num2 = 0;
		}
		return _default_band_palette[num2];
	}

	private Color getPointDisplayColor(int index)
	{
		EqPoint eqPoint = _points[index];
		Color color = eqPoint.BandColor;
		if (color == Color.Empty)
		{
			color = getBandBaseColor(eqPoint.BandId - 1);
		}
		Color result = (_use_per_band_colours ? color : Color.FromArgb(90, 200, 255));
		if (index == _selected_index)
		{
			result = Color.FromArgb(255, 200, 80);
		}
		return result;
	}

	private string formatHz(double hz)
	{
		if (hz >= 1000.0)
		{
			return (hz / 1000.0).ToString("0.###") + " kHz";
		}
		return hz.ToString("0") + " Hz";
	}

	private string formatDotReadingHz(double hz)
	{
		return hz.ToString("0") + " Hz";
	}

	private string formatDb(double db)
	{
		return ((db >= 0.0) ? "+" : "") + db.ToString("0.0") + " dB";
	}

	private string formatDotReadingDb(double db)
	{
		return ((db >= 0.0) ? "+" : "") + db.ToString("0.0") + " dB";
	}

	private int hitTestPoint(Rectangle plot, Point pt)
	{
		int result = -1;
		double num = double.MaxValue;
		for (int i = 0; i < _points.Count; i++)
		{
			EqPoint eqPoint = _points[i];
			float num2 = xFromFreq(plot, eqPoint.FrequencyHz);
			float num3 = yFromDb(plot, eqPoint.GainDb);
			double num4 = (double)pt.X - (double)num2;
			double num5 = (double)pt.Y - (double)num3;
			double num6 = num4 * num4 + num5 * num5;
			double num7 = _hit_radius;
			if (num6 <= num7 * num7 && num6 < num)
			{
				num = num6;
				result = i;
			}
		}
		return result;
	}

	private bool hitTestGlobalGainHandle(Rectangle plot, Point pt)
	{
		float num = yFromDb(plot, _global_gain_db);
		int num2 = plot.Right + _global_handle_x_offset;
		int global_handle_size = _global_handle_size;
		return new Rectangle(num2 - _global_hit_extra, (int)Math.Round(num) - (global_handle_size + _global_hit_extra), (global_handle_size + _global_hit_extra) * 2, (global_handle_size + _global_hit_extra) * 2).Contains(pt);
	}

	private float xFromFreq(Rectangle plot, double frequency_hz)
	{
		double normalizedFrequencyPosition = getNormalizedFrequencyPosition(frequency_hz);
		return (float)plot.Left + (float)(normalizedFrequencyPosition * (double)plot.Width);
	}

	private double freqFromX(Rectangle plot, int x)
	{
		double num = ((double)x - (double)plot.Left) / (double)plot.Width;
		if (num < 0.0)
		{
			num = 0.0;
		}
		if (num > 1.0)
		{
			num = 1.0;
		}
		return frequencyFromNormalizedPosition(num);
	}

	private float yFromDb(Rectangle plot, double db)
	{
		double num = _db_max - _db_min;
		if (num <= 0.0)
		{
			num = 1.0;
		}
		double num2 = (db - _db_min) / num;
		return (float)plot.Bottom - (float)(num2 * (double)plot.Height);
	}

	private double dbFromY(Rectangle plot, int y)
	{
		double num = _db_max - _db_min;
		if (num <= 0.0)
		{
			num = 1.0;
		}
		double num2 = ((double)plot.Bottom - (double)y) / (double)plot.Height;
		if (num2 < 0.0)
		{
			num2 = 0.0;
		}
		if (num2 > 1.0)
		{
			num2 = 1.0;
		}
		return _db_min + num2 * num;
	}

	private double clamp(double v, double min, double max)
	{
		if (v < min)
		{
			return min;
		}
		if (v > max)
		{
			return max;
		}
		return v;
	}

	private double getLogFrequencyCentreHz()
	{
		return getLogFrequencyCentreHz(_frequency_min_hz, _frequency_max_hz);
	}

	private double getLogFrequencyCentreHz(double min_hz, double max_hz)
	{
		double num = max_hz - min_hz;
		if (num <= 0.0)
		{
			return min_hz;
		}
		return min_hz + num * 0.125;
	}

	private double getNormalizedFrequencyPosition(double frequency_hz)
	{
		return getNormalizedFrequencyPosition(frequency_hz, _frequency_min_hz, _frequency_max_hz, _log_scale);
	}

	private double getNormalizedFrequencyPosition(double frequency_hz, double min_hz, double max_hz)
	{
		return getNormalizedFrequencyPosition(frequency_hz, min_hz, max_hz, _log_scale);
	}

	private double getNormalizedFrequencyPosition(double frequency_hz, double min_hz, double max_hz, bool use_log_scale)
	{
		double num = max_hz - min_hz;
		if (num <= 0.0)
		{
			return 0.0;
		}
		double num2 = (clamp(frequency_hz, min_hz, max_hz) - min_hz) / num;
		if (!use_log_scale)
		{
			return num2;
		}
		double centre_ratio = (getLogFrequencyCentreHz(min_hz, max_hz) - min_hz) / num;
		double logFrequencyShape = getLogFrequencyShape(centre_ratio);
		if (logFrequencyShape <= 0.0)
		{
			return num2;
		}
		return Math.Log(1.0 + logFrequencyShape * num2) / Math.Log(1.0 + logFrequencyShape);
	}

	private double frequencyFromNormalizedPosition(double t)
	{
		return frequencyFromNormalizedPosition(t, _frequency_min_hz, _frequency_max_hz, _log_scale);
	}

	private double frequencyFromNormalizedPosition(double t, double min_hz, double max_hz)
	{
		return frequencyFromNormalizedPosition(t, min_hz, max_hz, _log_scale);
	}

	private double frequencyFromNormalizedPosition(double t, double min_hz, double max_hz, bool use_log_scale)
	{
		if (t < 0.0)
		{
			t = 0.0;
		}
		if (t > 1.0)
		{
			t = 1.0;
		}
		double num = max_hz - min_hz;
		if (num <= 0.0)
		{
			return min_hz;
		}
		if (!use_log_scale)
		{
			return min_hz + t * num;
		}
		double centre_ratio = (getLogFrequencyCentreHz(min_hz, max_hz) - min_hz) / num;
		double logFrequencyShape = getLogFrequencyShape(centre_ratio);
		if (logFrequencyShape <= 0.0)
		{
			return min_hz + t * num;
		}
		double num2 = (Math.Exp(t * Math.Log(1.0 + logFrequencyShape)) - 1.0) / logFrequencyShape;
		return min_hz + num2 * num;
	}

	private double getLogFrequencyShape(double centre_ratio)
	{
		if (centre_ratio <= 0.0 || centre_ratio >= 1.0)
		{
			return 0.0;
		}
		if (Math.Abs(centre_ratio - 0.5) < 1E-07)
		{
			return 0.0;
		}
		double num = (1.0 - 2.0 * centre_ratio) / (centre_ratio * centre_ratio);
		if (num < 0.0)
		{
			return 0.0;
		}
		return num;
	}

	private List<double> getLogFrequencyTicks(Rectangle plot)
	{
		List<double> list = new List<double>();
		addLogFrequencyTickCandidate(list, _frequency_min_hz);
		addLogFrequencyTickCandidate(list, getLogFrequencyCentreHz());
		addLogFrequencyTickCandidate(list, _frequency_max_hz);
		if (_frequency_min_hz <= 0.0 && _frequency_max_hz >= 0.0)
		{
			addLogFrequencyTickCandidate(list, 0.0);
		}
		double d = ((_frequency_min_hz > 0.0) ? _frequency_min_hz : 1.0);
		double frequency_max_hz = _frequency_max_hz;
		if (frequency_max_hz > 0.0)
		{
			int num = (int)Math.Floor(Math.Log10(d));
			int num2 = (int)Math.Ceiling(Math.Log10(frequency_max_hz));
			double[] array = new double[3] { 1.0, 2.0, 5.0 };
			for (int i = num; i <= num2; i++)
			{
				double num3 = Math.Pow(10.0, i);
				for (int j = 0; j < array.Length; j++)
				{
					addLogFrequencyTickCandidate(list, array[j] * num3);
				}
			}
		}
		list.Sort();
		List<double> list2 = new List<double>();
		for (int k = 0; k < list.Count; k++)
		{
			double num4 = list[k];
			if (list2.Count <= 0 || !(Math.Abs(list2[list2.Count - 1] - num4) < 1E-06))
			{
				list2.Add(num4);
			}
		}
		if (list2.Count <= 2)
		{
			return list2;
		}
		List<double> list3 = new List<double>();
		double num5 = 28.0;
		for (int l = 0; l < list2.Count; l++)
		{
			double num6 = list2[l];
			bool flag = l == 0 || l == list2.Count - 1;
			if (!flag)
			{
				float num7 = xFromFreq(plot, num6);
				bool flag2 = true;
				for (int m = 0; m < list3.Count; m++)
				{
					float num8 = xFromFreq(plot, list3[m]);
					if ((double)Math.Abs(num7 - num8) < num5)
					{
						flag2 = false;
						break;
					}
				}
				flag = flag2;
			}
			if (flag)
			{
				list3.Add(num6);
			}
		}
		return list3;
	}

	private void addLogFrequencyTickCandidate(List<double> ticks, double frequency_hz)
	{
		if (!double.IsNaN(frequency_hz) && !double.IsInfinity(frequency_hz) && !(frequency_hz < _frequency_min_hz) && !(frequency_hz > _frequency_max_hz))
		{
			ticks.Add(frequency_hz);
		}
	}

	private void resetPointsDefault()
	{
		_points.Clear();
		int num = _band_count;
		if (num < 2)
		{
			num = 2;
		}
		_selected_index = -1;
		_drag_index = -1;
		_dragging_point = false;
		_dragging_global_gain = false;
		_drag_point_ref = null;
		_drag_dirty_point = false;
		_drag_dirty_global_gain = false;
		_drag_dirty_selected_index = false;
		double num2 = _frequency_max_hz - _frequency_min_hz;
		if (num2 <= 0.0)
		{
			num2 = 1.0;
		}
		for (int i = 0; i < num; i++)
		{
			double num3 = (double)i / (double)(num - 1);
			double frequency_hz = _frequency_min_hz + num3 * num2;
			double gain_db = 0.0;
			double q = 4.0;
			int band_id = i + 1;
			Color bandBaseColor = getBandBaseColor(i);
			_points.Add(new EqPoint(band_id, bandBaseColor, frequency_hz, gain_db, q));
		}
		enforceOrdering(enforce_spacing_all: true);
		clampAllGains();
		clampAllQ();
	}

	private void rescaleFrequencies(double old_min, double old_max, double new_min, double new_max)
	{
		double num = old_max - old_min;
		double num2 = new_max - new_min;
		if (num <= 0.0)
		{
			num = 1.0;
		}
		if (num2 <= 0.0)
		{
			num2 = 1.0;
		}
		for (int i = 0; i < _points.Count; i++)
		{
			EqPoint eqPoint = _points[i];
			double num3 = (eqPoint.FrequencyHz - old_min) / num;
			if (num3 < 0.0)
			{
				num3 = 0.0;
			}
			if (num3 > 1.0)
			{
				num3 = 1.0;
			}
			double frequencyHz = new_min + num3 * num2;
			eqPoint.FrequencyHz = frequencyHz;
		}
		if (_points.Count > 0)
		{
			_points[0].FrequencyHz = _frequency_min_hz;
		}
		if (_points.Count > 1)
		{
			_points[_points.Count - 1].FrequencyHz = _frequency_max_hz;
		}
	}

	private void enforceOrdering(bool enforce_spacing_all)
	{
		if (_points.Count == 0)
		{
			return;
		}
		EqPoint eqPoint = null;
		if (_selected_index >= 0 && _selected_index < _points.Count)
		{
			eqPoint = _points[_selected_index];
		}
		EqPoint eqPoint2 = null;
		if (_drag_index >= 0 && _drag_index < _points.Count)
		{
			eqPoint2 = _points[_drag_index];
		}
		if (_allow_point_reorder && _points.Count > 1)
		{
			_points.Sort(delegate(EqPoint a, EqPoint b)
			{
				int num12 = a.FrequencyHz.CompareTo(b.FrequencyHz);
				return (num12 != 0) ? num12 : a.BandId.CompareTo(b.BandId);
			});
		}
		if (eqPoint != null)
		{
			int num = _points.IndexOf(eqPoint);
			if (num != _selected_index)
			{
				_selected_index = num;
				raiseSelectedIndexChanged(isDraggingNow());
			}
		}
		else if (_selected_index != -1)
		{
			_selected_index = -1;
			raiseSelectedIndexChanged(isDraggingNow());
		}
		if (eqPoint2 != null)
		{
			_drag_index = _points.IndexOf(eqPoint2);
		}
		else
		{
			_drag_index = -1;
		}
		for (int num2 = 0; num2 < _points.Count; num2++)
		{
			EqPoint eqPoint3 = _points[num2];
			eqPoint3.FrequencyHz = clamp(eqPoint3.FrequencyHz, _frequency_min_hz, _frequency_max_hz);
		}
		if (_points.Count > 0)
		{
			_points[0].FrequencyHz = _frequency_min_hz;
		}
		if (_points.Count > 1)
		{
			_points[_points.Count - 1].FrequencyHz = _frequency_max_hz;
		}
		if (!enforce_spacing_all || _points.Count < 3)
		{
			return;
		}
		double num3 = _min_point_spacing_hz;
		double num4 = (_frequency_max_hz - _frequency_min_hz) / (double)(_points.Count - 1);
		if (num3 > num4)
		{
			num3 = num4;
		}
		if (num3 < 0.0)
		{
			num3 = 0.0;
		}
		for (int num5 = 1; num5 < _points.Count - 1; num5++)
		{
			double num6 = _frequency_min_hz + num3 * (double)num5;
			double num7 = _frequency_max_hz - num3 * (double)(_points.Count - 1 - num5);
			if (num7 < num6)
			{
				num7 = num6;
			}
			EqPoint eqPoint4 = _points[num5];
			eqPoint4.FrequencyHz = clamp(eqPoint4.FrequencyHz, num6, num7);
		}
		for (int num8 = 1; num8 < _points.Count - 1; num8++)
		{
			double num9 = _points[num8 - 1].FrequencyHz + num3;
			if (_points[num8].FrequencyHz < num9)
			{
				_points[num8].FrequencyHz = num9;
			}
		}
		for (int num10 = _points.Count - 2; num10 >= 1; num10--)
		{
			double num11 = _points[num10 + 1].FrequencyHz - num3;
			if (_points[num10].FrequencyHz > num11)
			{
				_points[num10].FrequencyHz = num11;
			}
		}
		_points[0].FrequencyHz = _frequency_min_hz;
		_points[_points.Count - 1].FrequencyHz = _frequency_max_hz;
	}

	private void clampAllGains()
	{
		for (int i = 0; i < _points.Count; i++)
		{
			EqPoint eqPoint = _points[i];
			eqPoint.GainDb = clamp(eqPoint.GainDb, _db_min, _db_max);
		}
		_global_gain_db = clamp(_global_gain_db, _db_min, _db_max);
	}

	private void clampAllQ()
	{
		for (int i = 0; i < _points.Count; i++)
		{
			EqPoint eqPoint = _points[i];
			eqPoint.Q = clamp(eqPoint.Q, _q_min, _q_max);
		}
	}

	private void raisePointsChanged(bool is_dragging)
	{
		PointsChanged?.Invoke(this, new EqDraggingEventArgs(is_dragging));
	}

	private void raiseGlobalGainChanged(bool is_dragging)
	{
		GlobalGainChanged?.Invoke(this, new EqDraggingEventArgs(is_dragging));
	}

	private void raiseSelectedIndexChanged(bool is_dragging)
	{
		if (is_dragging)
		{
			_drag_dirty_selected_index = true;
		}
		SelectedIndexChanged?.Invoke(this, new EqDraggingEventArgs(is_dragging));
	}

	private void raisePointDataChangedForPoint(EqPoint p, bool is_dragging)
	{
		if (p != null)
		{
			int num = _points.IndexOf(p);
			if (num >= 0)
			{
				PointDataChanged?.Invoke(this, new EqPointDataChangedEventArgs(num, p.BandId, p.FrequencyHz, p.GainDb, p.Q, is_dragging));
			}
		}
	}

	private int getComputedPlotMarginBottom()
	{
		if (_show_readout)
		{
			return _plot_margin_bottom;
		}
		int num = 8;
		num = ((!_show_axis_scales) ? (num + 8) : (num + (_axis_tick_length + 2 + Font.Height + 4)));
		if (num < 10)
		{
			num = 10;
		}
		return num;
	}

	private bool isFrequencyLockedIndex(int index)
	{
		if (_points.Count > 0)
		{
			if (index != 0)
			{
				return index == _points.Count - 1;
			}
			return true;
		}
		return false;
	}

	private double getLockedFrequencyForIndex(int index)
	{
		if (index <= 0)
		{
			return _frequency_min_hz;
		}
		if (index >= _points.Count - 1)
		{
			return _frequency_max_hz;
		}
		return _points[index].FrequencyHz;
	}
}
