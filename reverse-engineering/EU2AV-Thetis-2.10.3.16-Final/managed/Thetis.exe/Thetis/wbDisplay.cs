#define TRACE
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis;

public class wbDisplay : PictureBox
{
	public const float CLEAR_FLAG = -999.999f;

	public const int BUFFER_SIZE = 4096;

	public string background_image;

	public float[] new_display_data;

	public float[] current_display_data;

	private Point[] points;

	private Task draw_display_task;

	private Bitmap wbDisplay_buffer;

	public bool pauseDisplayThread;

	private string update_rate = "15";

	private string frame_size = "32";

	private FRSRegion current_region;

	private float freq_ruler_position = 1f;

	private int nreceivers = 2;

	private Console console;

	private bool refresh_panadapter_grid = true;

	private Rectangle agcKnee;

	private Rectangle agcHang;

	private Rectangle filterRect;

	private Color notch_on_color = Color.Olive;

	private Color notch_on_color_zoomed = Color.FromArgb(190, 128, 128, 0);

	private Color notch_highlight_color = Color.YellowGreen;

	private Color notch_highlight_color_zoomed = Color.FromArgb(190, 154, 205, 50);

	private Color notch_perm_on_color = Color.DarkGreen;

	private Color notch_perm_highlight_color = Color.Chartreuse;

	private Color notch_off_color = Color.Gray;

	private Color channel_background_on = Color.FromArgb(150, Color.DodgerBlue);

	private Color channel_background_off = Color.FromArgb(100, Color.RoyalBlue);

	private Color channel_foreground = Color.Cyan;

	private ColorScheme color_scheme = ColorScheme.enhanced;

	private bool reverse_waterfall;

	private bool pan_fill = true;

	private Color pan_fill_color = Color.FromArgb(100, 0, 0, 127);

	private bool display_duplex;

	private bool split_display;

	private int rx_filter_low;

	private int rx_filter_high;

	private bool sub_rx1_enabled;

	private bool split_enabled;

	private bool show_freq_offset;

	private double freq;

	private long _vfo_hz = 10000000L;

	private long vfoa_sub_hz;

	private int rit_hz;

	private int freq_diff;

	private int cw_pitch = 600;

	public bool specready;

	private Control target;

	private float alex_preamp_offset;

	private float preamp_offset;

	private float rx_display_cal_offset = -40.1f;

	private float rx_fft_size_offset;

	private HPSDRModel current_model = HPSDRModel.HERMES;

	private int display_cursor_x;

	private int display_cursor_y;

	private bool grid_control = true;

	private bool show_agc = true;

	private bool spectrum_line = true;

	private bool display_agc_hang_line = true;

	private bool rx1_hang_spectrum_line = true;

	private ClickTuneMode current_click_tune_mode;

	private bool high_swr;

	private bool mox;

	private DSPMode rx_dsp_mode = DSPMode.USB;

	private DisplayMode current_display_mode = DisplayMode.PANADAPTER;

	private float max_x;

	private float max_y;

	private bool data_ready;

	private bool waterfall_data_ready;

	public float display_avg_mult_old = 0.8f;

	public float display_avg_mult_new = 0.2f;

	private int display_avg_num_blocks = 5;

	public float waterfall_avg_mult_old = 17f / 18f;

	public float waterfall_avg_mult_new = 1f / 18f;

	private int waterfall_avg_num_blocks = 18;

	private int spectrum_grid_max = -50;

	private int spectrum_grid_min = -170;

	private int spectrum_grid_step = 10;

	private static Color band_edge_color = Color.Red;

	private Pen band_edge_pen = new Pen(band_edge_color);

	private static Color sub_rx_zero_line_color = Color.LightSkyBlue;

	private Pen sub_rx_zero_line_pen = new Pen(sub_rx_zero_line_color);

	private static Color sub_rx_filter_color = Color.Blue;

	private SolidBrush sub_rx_filter_brush = new SolidBrush(sub_rx_filter_color);

	private static Color grid_text_color = Color.Yellow;

	private SolidBrush grid_text_brush = new SolidBrush(grid_text_color);

	private Pen grid_text_pen = new Pen(grid_text_color);

	private static Color grid_zero_color = Color.Red;

	private Pen grid_zero_pen = new Pen(grid_zero_color);

	private static Color grid_color = Color.FromArgb(65, 255, 255, 255);

	private Pen grid_pen = new Pen(grid_color);

	private static Color hgrid_color = Color.White;

	private Pen hgrid_pen = new Pen(hgrid_color);

	private static Color data_line_color = Color.White;

	private Pen data_line_pen = new Pen(new SolidBrush(data_line_color), 1f);

	private Pen data_line_fpen = new Pen(Color.FromArgb(100, data_line_color));

	private static Color grid_pen_dark = Color.FromArgb(65, 255, 255, 255);

	private Pen grid_pen_inb = new Pen(grid_pen_dark);

	private static Color display_filter_color = Color.FromArgb(65, 255, 255, 255);

	private SolidBrush display_filter_brush = new SolidBrush(display_filter_color);

	private Pen cw_zero_pen = new Pen(Color.FromArgb(255, display_filter_color));

	private static Color display_background_color = Color.Black;

	private SolidBrush display_background_brush = new SolidBrush(display_background_color);

	private bool show_cwzero_line;

	private Color waterfall_low_color = Color.Black;

	private Color waterfall_mid_color = Color.Red;

	private Color waterfall_high_color = Color.Yellow;

	private float waterfall_high_threshold = -80f;

	private float waterfall_low_threshold = -130f;

	private float display_line_width = 1f;

	private DisplayLabelAlignment display_label_align;

	private bool click_tune_filter = true;

	private bool show_cth_line;

	private int top_size;

	private int _lin_corr = 2;

	private int _linlog_corr = -14;

	private SolidBrush pana_text_brush = new SolidBrush(Color.Khaki);

	private Font pana_font = new Font("Tahoma", 7f, FontStyle.Regular, GraphicsUnit.Point, 0);

	private Pen dhp = new Pen(Color.FromArgb(0, 255, 0));

	private Pen dhp1 = new Pen(Color.FromArgb(150, 0, 0, 255));

	private Pen dhp2 = new Pen(Color.FromArgb(150, 255, 0, 0));

	private Font font14 = new Font("Arial", 14f, FontStyle.Bold);

	private Font font9 = new Font("Arial", 9f);

	private bool waterfall_agc;

	private int waterfall_update_period = 100;

	private Rectangle freqScalePanRect;

	private Rectangle panRect;

	private Rectangle waterfallRect;

	private Rectangle dBmScalePanRect;

	private Rectangle secScaleWaterfallRect;

	private int displayTop;

	public DisplayRegion mouseRegion;

	private bool rx1_sub_drag;

	private bool rx1_spectrum_drag;

	private int spectrum_drag_last_x;

	private Point grid_minmax_drag_start_point = new Point(0, 0);

	private decimal grid_minmax_max_y;

	private decimal grid_minmax_min_y;

	private bool moveX;

	private bool moveY;

	private bool gridmaxadjust;

	private bool gridminmaxadjust;

	private CancellationTokenSource cancelTokenSource;

	private readonly object m_objBufferLock = new object();

	private static readonly object wbMonitor = new object();

	private Point mousePos;

	private Point mouseDownPos;

	private Point rulerMouseDownPos;

	private int adc;

	private int sample_rate = 122880000;

	private int fft_size = 16384;

	private int window_type = 6;

	private double kaiser_pi = 14.0;

	private int avm;

	private bool average_on = true;

	private bool peak_on;

	private double tau = 0.12;

	private int frame_rate = 15;

	private int pixels = 2048;

	private double z_slider;

	private double p_slider = 0.5;

	private int low_freq;

	private int high_freq;

	public bool init;

	public string UpdateRate
	{
		get
		{
			return update_rate;
		}
		set
		{
			update_rate = value;
			NetworkIO.SetWBUpdateRate(1000 / int.Parse(value));
		}
	}

	public string FrameSize
	{
		get
		{
			return frame_size;
		}
		set
		{
			frame_size = value;
			NetworkIO.SetWBPacketsPerFrame(int.Parse(value));
		}
	}

	public float FrameDelta { get; private set; }

	public FRSRegion CurrentRegion
	{
		get
		{
			return current_region;
		}
		set
		{
			current_region = value;
		}
	}

	public float FreqRulerPosition
	{
		get
		{
			return freq_ruler_position;
		}
		set
		{
			freq_ruler_position = value;
			CreateDisplayRegions();
		}
	}

	public int NReceivers
	{
		get
		{
			return nreceivers;
		}
		set
		{
			nreceivers = value;
		}
	}

	public Console cOnsole
	{
		get
		{
			return console;
		}
		set
		{
			console = value;
		}
	}

	public bool RefreshPanadapterGrid
	{
		set
		{
			refresh_panadapter_grid = value;
		}
	}

	public Rectangle AGCKnee
	{
		get
		{
			return agcKnee;
		}
		set
		{
			agcKnee = value;
		}
	}

	public Rectangle AGCHang
	{
		get
		{
			return agcHang;
		}
		set
		{
			agcHang = value;
		}
	}

	public Rectangle FilterRect
	{
		get
		{
			return filterRect;
		}
		set
		{
			filterRect = value;
		}
	}

	public ColorScheme ColorScheme
	{
		get
		{
			return color_scheme;
		}
		set
		{
			color_scheme = value;
		}
	}

	public bool ReverseWaterfall
	{
		get
		{
			return reverse_waterfall;
		}
		set
		{
			reverse_waterfall = value;
		}
	}

	public bool PanFill
	{
		get
		{
			return pan_fill;
		}
		set
		{
			pan_fill = value;
		}
	}

	public Color PanFillColor
	{
		get
		{
			return pan_fill_color;
		}
		set
		{
			pan_fill_color = value;
		}
	}

	public bool DisplayDuplex
	{
		get
		{
			return display_duplex;
		}
		set
		{
			display_duplex = value;
		}
	}

	public bool SplitDisplay
	{
		get
		{
			return split_display;
		}
		set
		{
			split_display = value;
			refresh_panadapter_grid = true;
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
		}
	}

	public bool SubRX1Enabled
	{
		get
		{
			return sub_rx1_enabled;
		}
		set
		{
			sub_rx1_enabled = value;
			if (current_display_mode == DisplayMode.PANADAPTER)
			{
				refresh_panadapter_grid = true;
			}
		}
	}

	public bool SplitEnabled
	{
		get
		{
			return split_enabled;
		}
		set
		{
			split_enabled = value;
		}
	}

	public bool ShowFreqOffset
	{
		get
		{
			return show_freq_offset;
		}
		set
		{
			show_freq_offset = value;
			if (current_display_mode == DisplayMode.PANADAPTER)
			{
				refresh_panadapter_grid = true;
			}
		}
	}

	public double FREQ
	{
		get
		{
			return freq;
		}
		set
		{
			freq = value;
		}
	}

	public long VFOHz
	{
		get
		{
			return _vfo_hz;
		}
		set
		{
			_vfo_hz = value;
			if (current_display_mode == DisplayMode.PANADAPTER)
			{
				refresh_panadapter_grid = true;
			}
		}
	}

	public long VFOASub
	{
		get
		{
			return vfoa_sub_hz;
		}
		set
		{
			vfoa_sub_hz = value;
			if (current_display_mode == DisplayMode.PANADAPTER)
			{
				refresh_panadapter_grid = true;
			}
		}
	}

	public int RIT
	{
		get
		{
			return rit_hz;
		}
		set
		{
			rit_hz = value;
			if (current_display_mode == DisplayMode.PANADAPTER)
			{
				refresh_panadapter_grid = true;
			}
		}
	}

	public int FreqDiff
	{
		get
		{
			return freq_diff;
		}
		set
		{
			freq_diff = value;
		}
	}

	public int CWPitch
	{
		get
		{
			return cw_pitch;
		}
		set
		{
			cw_pitch = value;
		}
	}

	public Control Target
	{
		get
		{
			return target;
		}
		set
		{
			target = value;
		}
	}

	public float AlexPreampOffset
	{
		get
		{
			return alex_preamp_offset;
		}
		set
		{
			alex_preamp_offset = value;
		}
	}

	public float PreampOffset
	{
		get
		{
			return preamp_offset;
		}
		set
		{
			preamp_offset = value;
		}
	}

	public float RXDisplayCalOffset
	{
		get
		{
			return rx_display_cal_offset;
		}
		set
		{
			rx_display_cal_offset = value;
		}
	}

	public float RXFFTSizeOffset
	{
		get
		{
			return rx_fft_size_offset;
		}
		set
		{
			rx_fft_size_offset = value;
		}
	}

	public HPSDRModel CurrentModel
	{
		get
		{
			return current_model;
		}
		set
		{
			current_model = value;
		}
	}

	public int DisplayCursorX
	{
		get
		{
			return display_cursor_x;
		}
		set
		{
			display_cursor_x = value;
		}
	}

	public int DisplayCursorY
	{
		get
		{
			return display_cursor_y;
		}
		set
		{
			display_cursor_y = value;
		}
	}

	public bool GridControl
	{
		get
		{
			return grid_control;
		}
		set
		{
			grid_control = value;
		}
	}

	public bool ShowAGC
	{
		get
		{
			return show_agc;
		}
		set
		{
			show_agc = value;
		}
	}

	public bool SpectrumLine
	{
		get
		{
			return spectrum_line;
		}
		set
		{
			spectrum_line = value;
		}
	}

	public bool DisplayAGCHangLine
	{
		get
		{
			return display_agc_hang_line;
		}
		set
		{
			display_agc_hang_line = value;
		}
	}

	public bool RX1HangSpectrumLine
	{
		get
		{
			return rx1_hang_spectrum_line;
		}
		set
		{
			rx1_hang_spectrum_line = value;
		}
	}

	public ClickTuneMode CurrentClickTuneMode
	{
		get
		{
			return current_click_tune_mode;
		}
		set
		{
			current_click_tune_mode = value;
		}
	}

	public bool HighSWR
	{
		get
		{
			return high_swr;
		}
		set
		{
			high_swr = value;
		}
	}

	public bool MOX
	{
		get
		{
			return mox;
		}
		set
		{
			mox = value;
		}
	}

	public DSPMode RXDSPMode
	{
		get
		{
			return rx_dsp_mode;
		}
		set
		{
			rx_dsp_mode = value;
		}
	}

	public DisplayMode CurrentDisplayMode
	{
		get
		{
			return current_display_mode;
		}
		set
		{
			current_display_mode = value;
			refresh_panadapter_grid = true;
		}
	}

	public float MaxX
	{
		get
		{
			return max_x;
		}
		set
		{
			max_x = value;
		}
	}

	public float MaxY
	{
		get
		{
			return max_y;
		}
		set
		{
			max_y = value;
		}
	}

	public bool DataReady
	{
		get
		{
			return data_ready;
		}
		set
		{
			data_ready = value;
		}
	}

	public bool WaterfallDataReady
	{
		get
		{
			return waterfall_data_ready;
		}
		set
		{
			waterfall_data_ready = value;
		}
	}

	public int DisplayAvgBlocks
	{
		get
		{
			return display_avg_num_blocks;
		}
		set
		{
			display_avg_num_blocks = value;
			display_avg_mult_old = 1f - 1f / (float)display_avg_num_blocks;
			display_avg_mult_new = 1f / (float)display_avg_num_blocks;
		}
	}

	public int WaterfallAvgBlocks
	{
		get
		{
			return waterfall_avg_num_blocks;
		}
		set
		{
			waterfall_avg_num_blocks = value;
			waterfall_avg_mult_old = 1f - 1f / (float)waterfall_avg_num_blocks;
			waterfall_avg_mult_new = 1f / (float)waterfall_avg_num_blocks;
		}
	}

	public int SpectrumGridMax
	{
		get
		{
			return spectrum_grid_max;
		}
		set
		{
			spectrum_grid_max = value;
			refresh_panadapter_grid = true;
		}
	}

	public int SpectrumGridMin
	{
		get
		{
			return spectrum_grid_min;
		}
		set
		{
			spectrum_grid_min = value;
			refresh_panadapter_grid = true;
		}
	}

	public int SpectrumGridStep
	{
		get
		{
			return spectrum_grid_step;
		}
		set
		{
			spectrum_grid_step = value;
			refresh_panadapter_grid = true;
		}
	}

	public Color BandEdgeColor
	{
		get
		{
			return band_edge_color;
		}
		set
		{
			band_edge_color = value;
			band_edge_pen.Color = band_edge_color;
			if (current_display_mode == DisplayMode.PANADAPTER)
			{
				refresh_panadapter_grid = true;
			}
		}
	}

	public Color SubRXZeroLine
	{
		get
		{
			return sub_rx_zero_line_color;
		}
		set
		{
			sub_rx_zero_line_color = value;
			sub_rx_zero_line_pen.Color = sub_rx_zero_line_color;
			if (current_display_mode == DisplayMode.PANADAPTER && sub_rx1_enabled)
			{
				refresh_panadapter_grid = true;
			}
		}
	}

	public Color SubRXFilterColor
	{
		get
		{
			return sub_rx_filter_color;
		}
		set
		{
			sub_rx_filter_color = value;
			sub_rx_filter_brush.Color = sub_rx_filter_color;
			if (current_display_mode == DisplayMode.PANADAPTER && sub_rx1_enabled)
			{
				refresh_panadapter_grid = true;
			}
		}
	}

	public Color GridTextColor
	{
		get
		{
			return grid_text_color;
		}
		set
		{
			grid_text_color = value;
			grid_text_brush.Color = grid_text_color;
			grid_text_pen.Color = grid_text_color;
			refresh_panadapter_grid = true;
		}
	}

	public Color GridZeroColor
	{
		get
		{
			return grid_zero_color;
		}
		set
		{
			grid_zero_color = value;
			grid_zero_pen.Color = grid_zero_color;
			refresh_panadapter_grid = true;
		}
	}

	public Color GridColor
	{
		get
		{
			return grid_color;
		}
		set
		{
			grid_color = value;
			grid_pen.Color = grid_color;
			refresh_panadapter_grid = true;
		}
	}

	public Color HGridColor
	{
		get
		{
			return hgrid_color;
		}
		set
		{
			hgrid_color = value;
			hgrid_pen = new Pen(hgrid_color);
			refresh_panadapter_grid = true;
		}
	}

	public Color DataLineColor
	{
		get
		{
			return data_line_color;
		}
		set
		{
			data_line_color = value;
			data_line_pen.Color = data_line_color;
			data_line_fpen.Color = Color.FromArgb(100, data_line_color);
			refresh_panadapter_grid = true;
		}
	}

	public Color GridPenDark
	{
		get
		{
			return grid_pen_dark;
		}
		set
		{
			grid_pen_dark = value;
			grid_pen_inb.Color = grid_pen_dark;
			refresh_panadapter_grid = true;
		}
	}

	public Color DisplayFilterColor
	{
		get
		{
			return display_filter_color;
		}
		set
		{
			display_filter_color = value;
			display_filter_brush.Color = display_filter_color;
			cw_zero_pen.Color = Color.FromArgb(255, display_filter_color);
			refresh_panadapter_grid = true;
		}
	}

	public Color DisplayBackgroundColor
	{
		get
		{
			return display_background_color;
		}
		set
		{
			display_background_color = value;
			display_background_brush.Color = display_background_color;
			refresh_panadapter_grid = true;
		}
	}

	public bool ShowCWZeroLine
	{
		get
		{
			return show_cwzero_line;
		}
		set
		{
			show_cwzero_line = value;
		}
	}

	public Color WaterfallLowColor
	{
		get
		{
			return waterfall_low_color;
		}
		set
		{
			waterfall_low_color = value;
		}
	}

	public Color WaterfallMidColor
	{
		get
		{
			return waterfall_mid_color;
		}
		set
		{
			waterfall_mid_color = value;
		}
	}

	public Color WaterfallHighColor
	{
		get
		{
			return waterfall_high_color;
		}
		set
		{
			waterfall_high_color = value;
		}
	}

	public float WaterfallHighThreshold
	{
		get
		{
			return waterfall_high_threshold;
		}
		set
		{
			waterfall_high_threshold = value;
		}
	}

	public float WaterfallLowThreshold
	{
		get
		{
			return waterfall_low_threshold;
		}
		set
		{
			waterfall_low_threshold = value;
		}
	}

	public float DisplayLineWidth
	{
		get
		{
			return display_line_width;
		}
		set
		{
			display_line_width = value;
			data_line_pen.Width = display_line_width;
		}
	}

	public DisplayLabelAlignment DisplayLabelAlign
	{
		get
		{
			return display_label_align;
		}
		set
		{
			display_label_align = value;
			refresh_panadapter_grid = true;
		}
	}

	public bool ClickTuneFilter
	{
		get
		{
			return click_tune_filter;
		}
		set
		{
			click_tune_filter = value;
		}
	}

	public bool ShowCTHLine
	{
		get
		{
			return show_cth_line;
		}
		set
		{
			show_cth_line = value;
		}
	}

	public int TopSize
	{
		get
		{
			return top_size;
		}
		set
		{
			top_size = value;
		}
	}

	public int LinCor
	{
		get
		{
			return _lin_corr;
		}
		set
		{
			_lin_corr = value;
		}
	}

	public int LinLogCor
	{
		get
		{
			return _linlog_corr;
		}
		set
		{
			_linlog_corr = value;
		}
	}

	public bool WaterfallAGC
	{
		get
		{
			return waterfall_agc;
		}
		set
		{
			waterfall_agc = value;
		}
	}

	public int WaterfallUpdatePeriod
	{
		get
		{
			return waterfall_update_period;
		}
		set
		{
			waterfall_update_period = value;
		}
	}

	public Rectangle FreqScalePanRect
	{
		get
		{
			return freqScalePanRect;
		}
		set
		{
			freqScalePanRect = value;
		}
	}

	public Rectangle PanRect
	{
		get
		{
			return panRect;
		}
		set
		{
			panRect = value;
		}
	}

	public Rectangle WaterfallRect
	{
		get
		{
			return waterfallRect;
		}
		set
		{
			waterfallRect = value;
		}
	}

	public Rectangle DBMScalePanRect
	{
		get
		{
			return dBmScalePanRect;
		}
		set
		{
			dBmScalePanRect = value;
		}
	}

	public Rectangle SecScalePanRect
	{
		get
		{
			return secScaleWaterfallRect;
		}
		set
		{
			secScaleWaterfallRect = value;
		}
	}

	public int ADC
	{
		get
		{
			return adc;
		}
		set
		{
			adc = value;
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
			initWideband();
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
			initWideband();
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
			initWideband();
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
			initWideband();
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
			if (peak_on)
			{
				avm = -1;
			}
			else if (average_on)
			{
				avm = 3;
			}
			else
			{
				avm = 0;
			}
			initWideband();
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
			if (peak_on)
			{
				avm = -1;
			}
			else if (average_on)
			{
				avm = 3;
			}
			else
			{
				avm = 0;
			}
			initWideband();
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
			initWideband();
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
			initWideband();
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
			initWideband();
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
			initWideband();
		}
	}

	public double PanSlider
	{
		get
		{
			return p_slider;
		}
		set
		{
			p_slider = value;
			initWideband();
		}
	}

	public int LowFreq => low_freq;

	public int HighFreq => high_freq;

	public wbDisplay()
	{
		base.MouseEnter += PanDisplay_MouseEnter;
		base.MouseWheel += PanDisplay_MouseWheel;
		base.MouseMove += PanDisplay_MouseMove;
		base.MouseDown += PanDisplay_MouseDown;
		base.MouseUp += PanDisplay_MouseUp;
	}

	public void Init()
	{
		int num = base.Width;
		_ = base.Height;
		CreateDisplayRegions();
		new_display_data = new float[4096];
		current_display_data = new float[4096];
		for (int i = 0; i < 4096; i++)
		{
			new_display_data[i] = -200f;
			current_display_data[i] = -200f;
		}
		Pixels = num;
	}

	public void DrawBackground()
	{
		Invalidate();
	}

	private void drawChannelBar(Graphics g, Channel chan, int left, int right, int top, int height, Color c, Color h)
	{
		int num = right - left;
		Pen pen = new Pen(h, 1f);
		g.FillRectangle(new SolidBrush(c), left, top, num, height);
		if (num > 2)
		{
			g.DrawLine(pen, left, top, left, top + height - 1);
			g.DrawLine(pen, right, top, right, top + height - 1);
		}
	}

	public void RenderGDIPlus(int rx, Graphics e)
	{
		DrawWideBand(e, rx);
	}

	private void UpdateDisplayPeak(float[] buffer, float[] new_data)
	{
		if (buffer[0] == -999.999f)
		{
			for (int i = 0; i < 4096; i++)
			{
				buffer[i] = new_data[i];
			}
			return;
		}
		for (int j = 0; j < 4096; j++)
		{
			if (new_data[j] > buffer[j])
			{
				buffer[j] = new_data[j];
			}
			new_data[j] = buffer[j];
		}
	}

	private void DrawWideBandGrid(Graphics g, int rx)
	{
		int num = panRect.Width;
		int num2 = panRect.Height;
		g.FillRectangle(Brushes.Black, freqScalePanRect);
		g.DrawRectangle(new Pen(Color.AntiqueWhite, 2f), freqScalePanRect);
		int num3 = 0;
		int num4 = 0;
		_ = num / 2;
		int[] array = new int[4] { 10, 20, 25, 50 };
		int num5 = 1;
		int num6 = 0;
		int num7 = 50;
		int num8 = 5;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		_ = _vfo_hz;
		num3 = low_freq;
		num4 = high_freq;
		num9 = spectrum_grid_max;
		num10 = spectrum_grid_min;
		num11 = spectrum_grid_step;
		g.FillRectangle(display_background_brush, 0, 0, num, num2);
		_ = freq_diff;
		int num12 = num9 - num10;
		if (split_display)
		{
			num11 *= 2;
		}
		int num13 = num4 - num3;
		while (num13 / num7 > 10)
		{
			num7 = array[num6] * (int)Math.Pow(10.0, num5);
			num6 = (num6 + 1) % 4;
			if (num6 == 0)
			{
				num5++;
			}
		}
		_ = (double)num * (double)num7 / (double)num13;
		_ = num13 / num7;
		int num14 = (num9 - num10) / num11;
		_ = (double)num2 / (double)num14;
		int num15 = 0;
		double num16 = num3;
		long num17 = (long)(num16 / (double)num7) * num7;
		long num18 = (long)(num16 - (double)num17);
		int num19 = num13 / num7 + 1;
		for (int i = 0; i < num19 + 1; i++)
		{
			int num20 = i * num7 + num3 / num7 * num7;
			double num21 = (double)(num17 + num20) / 1000000.0;
			int num22 = (int)((double)(num20 - num18 - num3) / (double)num13 * (double)num);
			num21.ToString();
			if (!show_freq_offset)
			{
				int num23;
				string text;
				switch (current_region)
				{
				case FRSRegion.LAST:
					if (!show_freq_offset)
					{
						g.DrawLine(band_edge_pen, num22, num15, num22, num2);
						text = num21.ToString("f1");
						if (num21 < 10.0)
						{
							num23 = (int)((double)(text.Length + 1) * 4.1) - 14;
						}
						else if (num21 < 100.0)
						{
							num23 = (int)((double)(text.Length + 1) * 4.1) - 11;
						}
						else
						{
							num23 = (int)((double)(text.Length + 1) * 4.1) - 8;
						}
						float num24 = (float)((int)((float)((i + 1) * num7 + num3 / num7 * num7 - num18 - num3) / (float)num13 * (float)num) - num22) / (float)num8;
						for (int j = 1; j < num8; j++)
						{
							float num25 = (float)num22 + (float)j * num24;
							g.DrawLine(grid_pen_inb, num25, num15, num25, num2);
						}
					}
					continue;
				case FRSRegion.Spain:
					if (num21 != 1.81 && num21 != 2.0 && num21 != 3.5 && num21 != 3.8 && num21 != 7.0 && num21 != 7.2 && num21 != 10.1 && num21 != 10.15 && num21 != 14.0 && num21 != 14.35 && num21 != 18.068 && num21 != 18.168 && num21 != 21.0 && num21 != 21.45 && num21 != 24.89 && num21 != 24.99 && num21 != 28.0 && num21 != 29.7 && num21 != 50.0 && num21 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.India:
					if (num21 != 1.81 && num21 != 1.86 && num21 != 3.5 && num21 != 3.9 && num21 != 7.0 && num21 != 7.2 && num21 != 10.1 && num21 != 10.15 && num21 != 14.0 && num21 != 14.35 && num21 != 18.068 && num21 != 18.168 && num21 != 21.0 && num21 != 21.45 && num21 != 24.89 && num21 != 24.99 && num21 != 28.0 && num21 != 29.7 && num21 != 50.0 && num21 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Europe:
					if (num21 != 1.81 && num21 != 2.0 && num21 != 3.5 && num21 != 3.8 && num21 != 7.0 && num21 != 7.2 && num21 != 10.1 && num21 != 10.15 && num21 != 14.0 && num21 != 14.35 && num21 != 18.068 && num21 != 18.168 && num21 != 21.0 && num21 != 21.45 && num21 != 24.89 && num21 != 24.99 && num21 != 28.0 && num21 != 29.7 && num21 != 50.08 && num21 != 51.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.UK:
					if (num21 != 0.472 && num21 != 0.479 && num21 != 1.81 && num21 != 2.0 && num21 != 3.5 && num21 != 3.8 && num21 != 5.2585 && num21 != 5.4065 && num21 != 7.0 && num21 != 7.2 && num21 != 10.1 && num21 != 10.15 && num21 != 14.0 && num21 != 14.35 && num21 != 18.068 && num21 != 18.168 && num21 != 21.0 && num21 != 21.45 && num21 != 24.89 && num21 != 24.99 && num21 != 28.0 && num21 != 29.7 && num21 != 50.0 && num21 != 52.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Italy_Plus:
					if (num21 != 1.81 && num21 != 2.0 && num21 != 3.5 && num21 != 3.8 && num21 != 6.975 && num21 != 7.2 && num21 != 10.1 && num21 != 10.15 && num21 != 14.0 && num21 != 14.35 && num21 != 18.068 && num21 != 18.168 && num21 != 21.0 && num21 != 21.45 && num21 != 24.89 && num21 != 24.99 && num21 != 28.0 && num21 != 29.7 && num21 != 50.08 && num21 != 51.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Japan:
					if (num21 != 0.1357 && num21 != 0.1378 && num21 != 1.81 && num21 != 1.825 && num21 != 1.9075 && num21 != 1.9125 && num21 != 3.5 && num21 != 3.575 && num21 != 3.599 && num21 != 3.612 && num21 != 3.68 && num21 != 3.687 && num21 != 3.702 && num21 != 3.716 && num21 != 3.745 && num21 != 3.77 && num21 != 3.791 && num21 != 3.805 && num21 != 7.0 && num21 != 7.2 && num21 != 10.1 && num21 != 10.15 && num21 != 14.0 && num21 != 14.35 && num21 != 18.068 && num21 != 18.168 && num21 != 21.0 && num21 != 21.45 && num21 != 24.89 && num21 != 24.99 && num21 != 28.0 && num21 != 29.7 && num21 != 50.0 && num21 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Australia:
					if (num21 != 0.1357 && num21 != 0.1378 && num21 != 0.472 && num21 != 0.479 && num21 != 1.8 && num21 != 1.875 && num21 != 3.5 && num21 != 3.7 && num21 != 3.776 && num21 != 3.8 && num21 != 7.0 && num21 != 7.3 && num21 != 10.1 && num21 != 10.15 && num21 != 14.0 && num21 != 14.35 && num21 != 18.068 && num21 != 18.168 && num21 != 21.0 && num21 != 21.45 && num21 != 24.89 && num21 != 24.99 && num21 != 28.0 && num21 != 29.7 && num21 != 50.0 && num21 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Norway:
					if (num21 != 1.8 && num21 != 1.875 && num21 != 3.5 && num21 != 3.7 && num21 != 3.776 && num21 != 3.8 && num21 != 5.26 && num21 != 5.41 && num21 != 7.0 && num21 != 7.3 && num21 != 10.1 && num21 != 10.15 && num21 != 14.0 && num21 != 14.35 && num21 != 18.068 && num21 != 18.168 && num21 != 21.0 && num21 != 21.45 && num21 != 24.89 && num21 != 24.99 && num21 != 28.0 && num21 != 29.7 && num21 != 50.0 && num21 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				}
				if (grid_control)
				{
					g.DrawLine(grid_pen, num22, num15, num22, num2);
					float num26 = (float)((int)((float)((i + 1) * num7 + num3 / num7 * num7 - num18 - num3) / (float)num13 * (float)num) - num22) / (float)num8;
					for (int k = 1; k < num8; k++)
					{
						float num27 = (float)num22 + (float)k * num26;
						g.DrawLine(grid_pen_inb, num27, num15, num27, num2);
					}
				}
				if ((double)(int)(num21 * 1000.0) == num21 * 1000.0)
				{
					text = num21.ToString("f1");
					num23 = ((num21 < 10.0) ? ((int)((double)(text.Length + 1) * 4.1) - 14) : ((!(num21 < 100.0)) ? ((int)((double)(text.Length + 1) * 4.1) - 8) : ((int)((double)(text.Length + 1) * 4.1) - 11)));
				}
				else
				{
					text = num21.ToString("f4");
					int startIndex = text.IndexOf('.') + 4;
					text = text.Insert(startIndex, " ");
					num23 = ((num21 < 10.0) ? ((int)((double)text.Length * 4.1) - 14) : ((!(num21 < 100.0)) ? ((int)((double)text.Length * 4.1) - 8) : ((int)((double)text.Length * 4.1) - 11)));
				}
				g.DrawString(text, font9, grid_text_brush, num22 - num23, freqScalePanRect.Top + 3);
			}
			else
			{
				num22 = Convert.ToInt32((double)(-(num20 - num3)) / (double)(num3 - num4) * (double)num);
				g.DrawLine(grid_pen, num22, num15, num22, num2);
				string text = num20.ToString();
				int num23 = (int)((double)(text.Length + 1) * 4.1);
				int num28 = (int)((double)text.Length * 4.1);
				if (num22 - num23 >= 0 && num22 + num28 < num && num20 != 0)
				{
					g.DrawString(text, font9, grid_text_brush, num22 - num23, freqScalePanRect.Top + 3);
				}
			}
		}
		int[] array2 = current_region switch
		{
			FRSRegion.Australia => new int[24]
			{
				135700, 137800, 472000, 479000, 1800000, 1875000, 3500000, 3800000, 7000000, 7300000,
				10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 28000000, 29700000,
				50000000, 54000000, 144000000, 148000000
			}, 
			FRSRegion.UK => new int[26]
			{
				472000, 479000, 1810000, 2000000, 3500000, 3800000, 5258500, 5406500, 7000000, 7200000,
				10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000,
				28000000, 29700000, 50000000, 52000000, 144000000, 148000000
			}, 
			FRSRegion.India => new int[22]
			{
				1810000, 1860000, 3500000, 3900000, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000,
				18068000, 18168000, 21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 54000000,
				144000000, 148000000
			}, 
			FRSRegion.Norway => new int[24]
			{
				1800000, 2000000, 3500000, 4000000, 5260000, 5410000, 7000000, 7300000, 10100000, 10150000,
				14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000, 28000000, 29700000,
				50000000, 54000000, 144000000, 148000000
			}, 
			FRSRegion.US => new int[24]
			{
				135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000, 7000000, 7300000,
				10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000,
				28000000, 29700000, 50000000, 54000000
			}, 
			FRSRegion.Japan => new int[37]
			{
				135700, 137800, 472000, 479000, 1810000, 1810000, 1907500, 1912500, 3500000, 3575000,
				3599000, 3612000, 3687000, 3702000, 3716000, 3745000, 3770000, 3791000, 3805000, 7000000,
				7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000,
				24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000
			}, 
			_ => new int[22]
			{
				1800000, 2000000, 3500000, 4000000, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000,
				18068000, 18168000, 21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 54000000,
				144000000, 148000000
			}, 
		};
		for (int l = 0; l < array2.Length; l++)
		{
			double num29 = (double)array2[l] - num16;
			if (num29 >= (double)num3 && num29 <= (double)num4)
			{
				int num30 = (int)((num29 - (double)num3) / (double)num13 * (double)num);
				g.DrawLine(band_edge_pen, num30, num15, num30, num2);
			}
		}
		for (int m = 1; m < num14; m++)
		{
			int num31 = num9 - m * num11;
			int num32 = (int)((double)(num9 - num31) * (double)num2 / (double)num12);
			g.DrawLine(hgrid_pen, 0, num32, num, num32);
			if (m != 0)
			{
				string text2 = (num9 - m * num11).ToString();
				if (text2.Length == 3)
				{
					_ = g.MeasureString("-", font9).Width;
				}
				int num33 = (int)((float)num - g.MeasureString(text2, font9).Width - 3f);
				num32 -= 8;
				if (num32 + 9 < num2)
				{
					g.DrawString(text2, font9, grid_text_brush, num33, num32);
				}
			}
		}
	}

	private float dBToPixel(float dB)
	{
		return ((float)spectrum_grid_max - dB) * (float)panRect.Height / (float)(spectrum_grid_max - spectrum_grid_min);
	}

	private void DrawOffBackground(Graphics g, int W, int H, bool bottom)
	{
		g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);
		if (high_swr && !bottom)
		{
			g.DrawString("High SWR", font14, Brushes.Red, 245f, 20f);
		}
	}

	private unsafe bool DrawWideBand(Graphics g, int rx)
	{
		int num = panRect.Width;
		int H = panRect.Height;
		DrawWideBandGrid(g, rx);
		if (pan_fill)
		{
			if (points == null || points.Length < num + 2)
			{
				points = new Point[num + 2];
			}
		}
		else if (points == null || points.Length < num)
		{
			points = new Point[num];
		}
		int num2 = 0;
		int num3 = 0;
		float local_max_y = float.MinValue;
		int grid_max = 0;
		int num4 = 0;
		num3 = high_freq;
		grid_max = spectrum_grid_max;
		num4 = spectrum_grid_min;
		if (rx_dsp_mode == DSPMode.DRM)
		{
			num2 += 12000;
			num3 += 12000;
		}
		int yRange = grid_max - num4;
		if (data_ready)
		{
			fixed (float* ptr = &new_display_data[0])
			{
				void* srcptr = ptr;
				fixed (float* ptr2 = &current_display_data[0])
				{
					void* destptr = ptr2;
					Win32.memcpy(destptr, srcptr, 16384);
				}
			}
			data_ready = false;
		}
		try
		{
			Parallel.For(0, num, delegate(int i)
			{
				float num5 = float.MinValue;
				num5 = current_display_data[i];
				num5 += rx_display_cal_offset;
				num5 += preamp_offset;
				if (num5 > local_max_y)
				{
					local_max_y = num5;
					max_x = i;
				}
				points[i].X = i;
				points[i].Y = (int)Math.Floor(((float)grid_max - num5) * (float)H / (float)yRange);
				points[i].Y = Math.Min(points[i].Y, H);
			});
		}
		catch (Exception value)
		{
			Trace.WriteLine(value);
		}
		max_y = local_max_y;
		try
		{
			if (pan_fill)
			{
				points[num].X = num;
				points[num].Y = H;
				points[num + 1].X = 0;
				points[num + 1].Y = H;
				g.FillPolygon(data_line_fpen.Brush, points);
				points[num] = points[num - 1];
				points[num + 1] = points[num - 1];
				data_line_pen.Color = data_line_color;
				g.DrawLines(data_line_pen, points);
			}
			else
			{
				g.DrawLines(data_line_pen, points);
			}
		}
		catch (Exception)
		{
		}
		points = null;
		try
		{
			if (current_click_tune_mode != ClickTuneMode.Off)
			{
				Pen pen = ((current_click_tune_mode != ClickTuneMode.VFOA) ? new Pen(Color.Red) : new Pen(grid_text_color));
				if (display_cursor_y <= H)
				{
					g.DrawLine(pen, display_cursor_x, 0, display_cursor_x, H);
					g.DrawLine(pen, 0, display_cursor_y, num, display_cursor_y);
				}
			}
		}
		catch (Exception)
		{
		}
		return true;
	}

	public void CreateDisplayRegions()
	{
		int num = base.Width;
		int num2 = base.Height;
		displayTop = 0;
		int num3 = 20;
		freqScalePanRect = new Rectangle(0, displayTop + (int)Math.Round((float)(num2 - displayTop - num3) * freq_ruler_position), num, num3);
		panRect = new Rectangle(0, displayTop, num, freqScalePanRect.Top - displayTop);
		dBmScalePanRect = new Rectangle(panRect.Right - 35, displayTop, 35, panRect.Height);
	}

	private void getRegion(Point p)
	{
		if (DBMScalePanRect.Contains(p))
		{
			mouseRegion = DisplayRegion.dBmScalePanadapterRegion;
		}
		else
		{
			mouseRegion = DisplayRegion.elsewhere;
		}
	}

	public void UpdateGraphicsBuffer()
	{
		int num = Math.Max(1, base.Width);
		int num2 = Math.Max(1, base.Height);
		wbDisplay_buffer = new Bitmap(num, num2);
	}

	public void Cancel_Display()
	{
		if (cancelTokenSource != null)
		{
			cancelTokenSource.Cancel();
			cancelTokenSource.Dispose();
			cancelTokenSource = null;
		}
	}

	public void StartDisplay(int rx)
	{
		if (cancelTokenSource != null)
		{
			Cancel_Display();
		}
		cancelTokenSource = new CancellationTokenSource();
		UpdateGraphicsBuffer();
		draw_display_task = Task.Factory.StartNew(delegate
		{
			while (cancelTokenSource != null && !cancelTokenSource.IsCancellationRequested)
			{
				RunDisplay(rx);
				if (!pauseDisplayThread)
				{
					using Graphics graphics = Graphics.FromImage(wbDisplay_buffer);
					graphics.Clear(Color.Transparent);
					if (DrawWideBand(graphics, rx))
					{
						Invoke((Action)delegate
						{
							base.Image = wbDisplay_buffer;
							Refresh();
						});
					}
				}
			}
		}, cancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
	}

	private unsafe void RunDisplay(int rx)
	{
		if (!DataReady)
		{
			int flag = 0;
			fixed (float* pix = &new_display_data[0])
			{
				SpecHPSDRDLL.GetPixels(rx, 0, pix, ref flag);
			}
			DataReady = true;
		}
		Thread.Sleep(16);
	}

	private void PanDisplay_MouseMove(object sender, MouseEventArgs e)
	{
		Size size = new Size(-1, -1);
		size = new Size(e.X, e.Y);
		mousePos = new Point(size);
		if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right && e.Button != MouseButtons.Middle)
		{
			getRegion(mousePos);
		}
		switch (mouseRegion)
		{
		case DisplayRegion.freqScalePanadapterRegion:
			if (e.Button == MouseButtons.Left)
			{
				Point point = Point.Subtract(mouseDownPos, size);
				if (point.Y != 0 && !moveY && !moveX)
				{
					moveY = true;
					moveX = false;
				}
				if (moveY)
				{
					int num6 = base.Height - FreqScalePanRect.Height;
					int num7 = rulerMouseDownPos.Y - point.Y;
					if (num7 < PanRect.Top)
					{
						num7 = PanRect.Top;
					}
					if (num7 > num6)
					{
						num7 = num6;
					}
					FreqRulerPosition = (float)(num7 - PanRect.Top) / (float)(num6 - PanRect.Top);
				}
				if (point.X != 0 && !moveY && !moveX)
				{
					moveX = true;
					moveY = false;
				}
				if (moveX)
				{
					_ = (double)(HighFreq - LowFreq) / (double)FreqScalePanRect.Width;
					_ = point.X;
					if (point.X > 0)
					{
						PanSlider += 0.001;
					}
					else if (point.X < 0)
					{
						PanSlider -= 0.001;
					}
					mouseDownPos = mousePos;
				}
			}
			if (e.Button == MouseButtons.Right)
			{
				Point point2 = Point.Subtract(mouseDownPos, size);
				if (point2.X > 0)
				{
					ZoomSlider += 0.001;
				}
				else if (point2.X < 0)
				{
					ZoomSlider -= 0.001;
				}
				mouseDownPos = mousePos;
			}
			break;
		case DisplayRegion.dBmScalePanadapterRegion:
			if (gridminmaxadjust)
			{
				double num = (e.Y - grid_minmax_drag_start_point.Y) / 10 * 5;
				decimal num2 = grid_minmax_max_y;
				num2 += (decimal)num;
				decimal num3 = grid_minmax_min_y;
				num3 += (decimal)num;
				if (num2 > 200m)
				{
					num2 = 200m;
				}
				if (num3 < -200m)
				{
					num3 = -200m;
				}
				SpectrumGridMax = (int)num2;
				SpectrumGridMin = (int)num3;
			}
			if (gridmaxadjust)
			{
				double num4 = (e.Y - grid_minmax_drag_start_point.Y) / 10 * 5;
				decimal num5 = grid_minmax_max_y;
				num5 += (decimal)num4;
				if (num5 > 200m)
				{
					num5 = 200m;
				}
				SpectrumGridMax = (int)num5;
			}
			break;
		case DisplayRegion.filterRegionLow:
			_ = e.Button;
			_ = 1048576;
			break;
		case DisplayRegion.filterRegionHigh:
			_ = e.Button;
			_ = 1048576;
			break;
		case DisplayRegion.filterRegion:
			_ = e.Button;
			_ = 1048576;
			break;
		case DisplayRegion.panadapterRegion:
		case DisplayRegion.waterfallRegion:
			break;
		}
	}

	private void PanDisplay_MouseUp(object sender, MouseEventArgs e)
	{
		Point point = new Point(-1, -1);
		point = new Point(e.X, e.Y);
		mousePos = point;
		mouseDownPos = mousePos;
		getRegion(mousePos);
		if (e.Button == MouseButtons.Left)
		{
			gridminmaxadjust = false;
			moveX = false;
			moveY = false;
			if (rx1_sub_drag)
			{
				rx1_sub_drag = false;
			}
			if (rx1_spectrum_drag)
			{
				rx1_spectrum_drag = false;
			}
		}
		if (e.Button == MouseButtons.Right)
		{
			gridminmaxadjust = false;
			gridmaxadjust = false;
		}
	}

	private void PanDisplay_MouseEnter(object sender, EventArgs e)
	{
		if (!Focused)
		{
			Focus();
		}
	}

	private void PanDisplay_MouseWheel(object sender, MouseEventArgs e)
	{
		_ = e.Delta;
	}

	private void PanDisplay_MouseDown(object sender, MouseEventArgs e)
	{
		Point point = new Point(-1, -1);
		point = (mousePos = new Point(e.X, e.Y));
		mouseDownPos = mousePos;
		getRegion(mousePos);
		switch (e.Button)
		{
		case MouseButtons.Left:
			if (mouseRegion != DisplayRegion.filterRegion)
			{
				if (mouseRegion == DisplayRegion.freqScalePanadapterRegion)
				{
					rulerMouseDownPos = new Point(FreqScalePanRect.Left, FreqScalePanRect.Top);
					spectrum_drag_last_x = e.X;
				}
				else if (mouseRegion == DisplayRegion.dBmScalePanadapterRegion)
				{
					rulerMouseDownPos = new Point(DBMScalePanRect.Left, DBMScalePanRect.Top);
					grid_minmax_drag_start_point = point;
					gridminmaxadjust = true;
					grid_minmax_max_y = SpectrumGridMax;
					grid_minmax_min_y = SpectrumGridMin;
				}
			}
			if (mouseRegion == DisplayRegion.panadapterRegion || mouseRegion == DisplayRegion.waterfallRegion)
			{
				spectrum_drag_last_x = e.X;
			}
			break;
		case MouseButtons.Right:
			if (mouseRegion == DisplayRegion.dBmScalePanadapterRegion)
			{
				grid_minmax_drag_start_point = new Point(e.X, e.Y);
				gridmaxadjust = true;
				grid_minmax_max_y = SpectrumGridMax;
			}
			else
			{
				_ = mouseRegion;
			}
			break;
		}
	}

	public void create_wideband(int adc)
	{
		int success = 0;
		SpecHPSDRDLL.XCreateAnalyzer(32 + adc, ref success, 16384, 1, 1, "");
	}

	public void initWideband()
	{
		if (init)
		{
			int disp = adc + 32;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			IntPtr flp = GCHandle.Alloc(new int[1], GCHandleType.Pinned).AddrOfPinnedObject();
			double mult = Math.Exp(-1.0 / ((double)frame_rate * tau));
			num = (int)Math.Floor(0.0 * (double)fft_size);
			double num4 = (double)sample_rate / (double)fft_size;
			int num5 = fft_size / 2 - 2 * num;
			double num6 = (double)num5 * num4;
			double num7 = Math.Log10(9.0 * z_slider + 1.0);
			int num8 = (int)((double)num5 * (1.0 - 0.99 * num7));
			num2 = (int)Math.Floor(p_slider * (double)(num5 - num8));
			num3 = num5 - num8 - num2;
			low_freq = sample_rate / 4 - (int)(0.5 * num6 - (double)num2 * num4);
			high_freq = sample_rate / 4 + (int)(0.5 * num6 - (double)num3 * num4);
			SpecHPSDRDLL.SetAnalyzer(disp, 2, 1, 0, flp, fft_size, 512, window_type, kaiser_pi, 0, num, num2, num3, pixels, 1, 0, 0.0, 0.0, 2 * fft_size);
			SpecHPSDRDLL.SetDisplayAverageMode(disp, 0, avm);
			SpecHPSDRDLL.SetDisplayAvBackmult(disp, 0, mult);
		}
	}
}
