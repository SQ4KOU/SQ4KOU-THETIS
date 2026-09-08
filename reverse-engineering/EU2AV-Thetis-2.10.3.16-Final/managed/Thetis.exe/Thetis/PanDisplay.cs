#define TRACE
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis;

public class PanDisplay : PictureBox
{
	public const float CLEAR_FLAG = -999.999f;

	public const int BUFFER_SIZE = 4096;

	public string background_image;

	public float[] new_display_data;

	public float[] current_display_data;

	private Point[] points;

	private int waterfall_counter;

	private Bitmap waterfall_bmp;

	private float[] waterfall_data;

	private Task draw_display_task;

	private Bitmap pDisplay_buffer;

	public bool pauseDisplayThread;

	private FRSRegion current_region;

	private float freq_ruler_position = 0.5f;

	private int nreceivers = 2;

	private Console console;

	private bool refresh_panadapter_grid = true;

	private Rectangle agcKnee;

	private Rectangle agcHang;

	private Rectangle filterRect;

	private int filterLeft;

	private int filterRight;

	private int filterTop;

	private int filterBottom;

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

	private float rx_display_cal_offset = -2.1f;

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

	private readonly HiPerfTimer timer_waterfall = new HiPerfTimer();

	private float waterfallPreviousMinValue;

	private Rectangle freqScalePanRect;

	private Rectangle panRect;

	private Rectangle waterfallRect;

	private Rectangle dBmScalePanRect;

	private Rectangle secScaleWaterfallRect;

	private int displayTop;

	private DisplayRegion mouseRegion;

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

	private CancellationTokenSource cancelTokenSource1 = new CancellationTokenSource();

	private Point mousePos;

	private Point mouseDownPos;

	private Point rulerMouseDownPos;

	private int sample_rate = 192000;

	private int data_type = 1;

	private int fft_size = 4096;

	private int window_type = 6;

	private double kaiser_pi = 14.0;

	private int pixels = 2048;

	private bool average_on = true;

	private bool peak_on;

	private double tau = 0.12;

	private int frame_rate = 15;

	private double z_factor = 0.5;

	private double p_slider = 0.5;

	private double freq_offset;

	private int low_freq;

	private int high_freq;

	private int display_id;

	public bool init;

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

	public int SampleRate
	{
		get
		{
			return sample_rate;
		}
		set
		{
			sample_rate = value;
			initAnalyzer();
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
			initAnalyzer();
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
			initAnalyzer();
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
			initAnalyzer();
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
			initAnalyzer();
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
			initAnalyzer();
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
			initAnalyzer();
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
			initAnalyzer();
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
			initAnalyzer();
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
			initAnalyzer();
		}
	}

	public double ZoomFactor
	{
		get
		{
			return z_factor;
		}
		set
		{
			if (value > 1.0)
			{
				value = 1.0;
			}
			if (value < 0.05)
			{
				value = 0.05;
			}
			z_factor = value;
			initAnalyzer();
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
			initAnalyzer();
		}
	}

	public double FreqOffset
	{
		get
		{
			return freq_offset;
		}
		set
		{
			freq_offset = value;
			initAnalyzer();
		}
	}

	public int LowFreq
	{
		get
		{
			return low_freq;
		}
		set
		{
			low_freq = value;
		}
	}

	public int HighFreq
	{
		get
		{
			return high_freq;
		}
		set
		{
			high_freq = value;
		}
	}

	public int DisplayID
	{
		get
		{
			return display_id;
		}
		set
		{
			display_id = value;
		}
	}

	public PanDisplay()
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
		try
		{
			DrawPanadapter(e, rx);
			if (waterfallRect.Height >= 10)
			{
				DrawWaterfall(e, rx);
			}
		}
		catch (Exception)
		{
		}
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

	private void DrawPanadapterGrid(Graphics g, int rx)
	{
		int num = panRect.Width;
		int num2 = panRect.Height;
		g.FillRectangle(Brushes.Black, freqScalePanRect);
		g.DrawRectangle(new Pen(Color.AntiqueWhite, 2f), freqScalePanRect);
		bool flag = false;
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
		int num12 = 0;
		long vfo_hz = _vfo_hz;
		num3 = low_freq;
		num4 = high_freq;
		num9 = spectrum_grid_max;
		num10 = spectrum_grid_min;
		num11 = spectrum_grid_step;
		g.FillRectangle(display_background_brush, 0, 0, num, num2);
		num12 = freq_diff;
		int num13 = num9 - num10;
		if (split_display)
		{
			num11 *= 2;
		}
		int num14 = rx_filter_low;
		int num15 = rx_filter_high;
		if (rx_dsp_mode == DSPMode.DRM)
		{
			num14 = -5000;
			num15 = 5000;
		}
		int num16 = num4 - num3;
		while (num16 / num7 > 10)
		{
			num7 = array[num6] * (int)Math.Pow(10.0, num5);
			num6 = (num6 + 1) % 4;
			if (num6 == 0)
			{
				num5++;
			}
		}
		_ = (double)num * (double)num7 / (double)num16;
		_ = num16 / num7;
		int num17 = (num9 - num10) / num11;
		_ = (double)num2 / (double)num17;
		int num18 = 0;
		if (!flag && sub_rx1_enabled && rx == 1)
		{
			int num19 = (int)((float)(num14 - num3 + vfoa_sub_hz - vfo_hz - rit_hz) / (float)num16 * (float)num);
			int num20 = (int)((float)(num15 - num3 + vfoa_sub_hz - vfo_hz - rit_hz) / (float)num16 * (float)num);
			if (num19 == num20)
			{
				num20 = num19 + 1;
			}
			g.FillRectangle(sub_rx_filter_brush, num19, num18, num20 - num19, num2 - num18);
			int num21 = (int)((float)(vfoa_sub_hz - vfo_hz - num3) / (float)num16 * (float)num);
			g.DrawLine(sub_rx_zero_line_pen, num21, num18, num21, num2);
			g.DrawLine(sub_rx_zero_line_pen, num21 - 1, num18, num21 - 1, num2);
		}
		if (!flag)
		{
			filterLeft = (int)((float)(num14 - num3 - num12) / (float)num16 * (float)num);
			filterRight = (int)((float)(num15 - num3 - num12) / (float)num16 * (float)num);
			filterTop = panRect.Top + 1;
			filterBottom = panRect.Top + panRect.Height - 1;
			if (filterLeft == filterRight)
			{
				filterRight = filterLeft + 1;
			}
			filterRect = new Rectangle(filterLeft, filterTop, filterRight - filterLeft, filterBottom - filterTop);
			if ((filterLeft >= panRect.Left && filterLeft <= panRect.Right) || (filterRight >= panRect.Left && filterRight <= panRect.Right) || (filterLeft < panRect.Left && filterRight > panRect.Right))
			{
				g.FillRectangle(display_filter_brush, filterLeft, num18, filterRight - filterLeft, num2 - num18);
			}
		}
		if (current_region == FRSRegion.US || current_region == FRSRegion.UK)
		{
			foreach (Channel item in Console.Channels60m)
			{
				long num22 = vfo_hz;
				int num23 = rit_hz;
				if (item.InBW((double)(num22 + num3) * 1E-06, (double)(num22 + num4) * 1E-06))
				{
					bool flag2 = console.RX1IsIn60mChannel(item);
					if (rx == 2)
					{
						flag2 = console.RX2IsIn60mChannel(item);
					}
					switch (rx_dsp_mode)
					{
					default:
						flag2 = false;
						break;
					case DSPMode.USB:
					case DSPMode.CWL:
					case DSPMode.CWU:
					case DSPMode.AM:
					case DSPMode.DIGU:
					case DSPMode.SAM:
						break;
					}
					switch (rx_dsp_mode)
					{
					case DSPMode.CWL:
						num22 += cw_pitch;
						break;
					case DSPMode.CWU:
						num22 -= cw_pitch;
						break;
					}
					int num24 = (int)((float)(item.Freq * 1000000.0 - (double)num22 - (double)(item.BW / 2) - (double)num3 - (double)num23) / (float)num16 * (float)num);
					int num25 = (int)((float)(item.Freq * 1000000.0 - (double)num22 + (double)(item.BW / 2) - (double)num3 - (double)num23) / (float)num16 * (float)num);
					if (num25 == num24)
					{
						num25 = num24 + 1;
					}
					Color c = channel_background_off;
					Color h = channel_foreground;
					if (flag2)
					{
						c = channel_background_on;
					}
					drawChannelBar(g, item, num24, num25, num18, num2 - num18, c, h);
				}
			}
		}
		if (!flag && show_cwzero_line && (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU))
		{
			int num26 = cw_pitch;
			if (rx_dsp_mode == DSPMode.CWL)
			{
				num26 = -cw_pitch;
			}
			int num27 = (split_enabled ? ((int)((float)(num26 - num3 + (vfoa_sub_hz - vfo_hz)) / (float)num16 * (float)num)) : ((int)((float)(num26 - num3 - num12) / (float)num16 * (float)num)));
			g.DrawLine(cw_zero_pen, num27, num18, num27, num2);
			g.DrawLine(cw_zero_pen, num27 + 1, num18, num27 + 1, num2);
		}
		int num28 = (int)((float)(-num12 - num3) / (float)num16 * (float)num);
		if (num28 >= 0 && num28 <= num)
		{
			g.DrawLine(grid_zero_pen, num28, num18, num28, num2);
			g.DrawLine(grid_zero_pen, num28 + 1, num18, num28 + 1, num2);
		}
		if (show_freq_offset)
		{
			g.DrawString("0", font9, grid_zero_pen.Brush, num28 - 5, (float)Math.Floor((double)num2 * 0.01));
		}
		double num29 = vfo_hz + rit_hz;
		switch (rx_dsp_mode)
		{
		case DSPMode.CWL:
			num29 += (double)cw_pitch;
			break;
		case DSPMode.CWU:
			num29 -= (double)cw_pitch;
			break;
		}
		long num30 = (long)(num29 / (double)num7) * num7;
		long num31 = (long)(num29 - (double)num30);
		int num32 = num16 / num7 + 1;
		for (int i = 0; i < num32 + 1; i++)
		{
			int num33 = i * num7 + num3 / num7 * num7;
			double num34 = (double)(num30 + num33) / 1000000.0;
			int num35 = (int)((double)(num33 - num31 - num3) / (double)num16 * (double)num);
			num34.ToString();
			if (!show_freq_offset)
			{
				string text;
				switch (current_region)
				{
				case FRSRegion.LAST:
					if (!show_freq_offset)
					{
						g.DrawLine(band_edge_pen, num35, num18, num35, num2);
						text = num34.ToString("f3");
						g.DrawString(x: num35 - ((num34 < 10.0) ? ((int)((double)(text.Length + 1) * 4.1) - 14) : ((!(num34 < 100.0)) ? ((int)((double)(text.Length + 1) * 4.1) - 8) : ((int)((double)(text.Length + 1) * 4.1) - 11))), s: text, font: font9, brush: band_edge_pen.Brush, y: freqScalePanRect.Top + 3);
						float num36 = (float)((int)((float)((i + 1) * num7 + num3 / num7 * num7 - num31 - num3) / (float)num16 * (float)num) - num35) / (float)num8;
						for (int j = 1; j < num8; j++)
						{
							float num37 = (float)num35 + (float)j * num36;
							g.DrawLine(grid_pen_inb, num37, num18, num37, num2);
						}
					}
					continue;
				case FRSRegion.US:
				case FRSRegion.Extended:
					if (num34 != 0.1357 && num34 != 0.1378 && num34 != 0.472 && num34 != 0.479 && num34 != 1.8 && num34 != 2.0 && num34 != 3.5 && num34 != 4.0 && num34 != 7.0 && num34 != 7.3 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.0 && num34 != 54.0 && num34 != 144.0 && num34 != 148.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Spain:
					if (num34 != 1.81 && num34 != 2.0 && num34 != 3.5 && num34 != 3.8 && num34 != 7.0 && num34 != 7.2 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.0 && num34 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.India:
					if (num34 != 1.81 && num34 != 1.86 && num34 != 3.5 && num34 != 3.9 && num34 != 7.0 && num34 != 7.2 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.0 && num34 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Europe:
					if (num34 != 1.81 && num34 != 2.0 && num34 != 3.5 && num34 != 3.8 && num34 != 7.0 && num34 != 7.2 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.08 && num34 != 51.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.UK:
					if (num34 != 0.472 && num34 != 0.479 && num34 != 1.81 && num34 != 2.0 && num34 != 3.5 && num34 != 3.8 && num34 != 5.2585 && num34 != 5.4065 && num34 != 7.0 && num34 != 7.2 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.0 && num34 != 52.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Italy_Plus:
					if (num34 != 1.81 && num34 != 2.0 && num34 != 3.5 && num34 != 3.8 && num34 != 6.975 && num34 != 7.2 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.08 && num34 != 51.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Japan:
					if (num34 != 0.1357 && num34 != 0.1378 && num34 != 1.81 && num34 != 1.825 && num34 != 1.9075 && num34 != 1.9125 && num34 != 3.5 && num34 != 3.575 && num34 != 3.599 && num34 != 3.612 && num34 != 3.68 && num34 != 3.687 && num34 != 3.702 && num34 != 3.716 && num34 != 3.745 && num34 != 3.77 && num34 != 3.791 && num34 != 3.805 && num34 != 7.0 && num34 != 7.2 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.0 && num34 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Australia:
					if (num34 != 0.1357 && num34 != 0.1378 && num34 != 0.472 && num34 != 0.479 && num34 != 1.8 && num34 != 1.875 && num34 != 3.5 && num34 != 3.7 && num34 != 3.776 && num34 != 3.8 && num34 != 7.0 && num34 != 7.3 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.0 && num34 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				case FRSRegion.Norway:
					if (num34 != 1.8 && num34 != 1.875 && num34 != 3.5 && num34 != 3.7 && num34 != 3.776 && num34 != 3.8 && num34 != 5.26 && num34 != 5.41 && num34 != 7.0 && num34 != 7.3 && num34 != 10.1 && num34 != 10.15 && num34 != 14.0 && num34 != 14.35 && num34 != 18.068 && num34 != 18.168 && num34 != 21.0 && num34 != 21.45 && num34 != 24.89 && num34 != 24.99 && num34 != 28.0 && num34 != 29.7 && num34 != 50.0 && num34 != 54.0)
					{
						break;
					}
					goto case FRSRegion.LAST;
				}
				if (grid_control)
				{
					g.DrawLine(grid_pen, num35, num18, num35, num2);
					float num38 = (float)((int)((float)((i + 1) * num7 + num3 / num7 * num7 - num31 - num3) / (float)num16 * (float)num) - num35) / (float)num8;
					for (int k = 1; k < num8; k++)
					{
						float num39 = (float)num35 + (float)k * num38;
						g.DrawLine(grid_pen_inb, num39, num18, num39, num2);
					}
				}
				int num40;
				if ((double)(int)(num34 * 1000.0) == num34 * 1000.0)
				{
					text = num34.ToString("f3");
					num40 = ((num34 < 10.0) ? ((int)((double)(text.Length + 1) * 4.1) - 14) : ((!(num34 < 100.0)) ? ((int)((double)(text.Length + 1) * 4.1) - 8) : ((int)((double)(text.Length + 1) * 4.1) - 11)));
				}
				else
				{
					text = num34.ToString("f4");
					int startIndex = text.IndexOf('.') + 4;
					text = text.Insert(startIndex, " ");
					num40 = ((num34 < 10.0) ? ((int)((double)text.Length * 4.1) - 14) : ((!(num34 < 100.0)) ? ((int)((double)text.Length * 4.1) - 8) : ((int)((double)text.Length * 4.1) - 11)));
				}
				g.DrawString(text, font9, grid_text_brush, num35 - num40, freqScalePanRect.Top + 3);
			}
			else
			{
				num35 = Convert.ToInt32((double)(-(num33 - num3)) / (double)(num3 - num4) * (double)num);
				g.DrawLine(grid_pen, num35, num18, num35, num2);
				string text = num33.ToString();
				int num40 = (int)((double)(text.Length + 1) * 4.1);
				int num41 = (int)((double)text.Length * 4.1);
				if (num35 - num40 >= 0 && num35 + num41 < num && num33 != 0)
				{
					g.DrawString(text, font9, grid_text_brush, num35 - num40, freqScalePanRect.Top + 3);
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
			FRSRegion.US => new int[26]
			{
				135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000, 7000000, 7300000,
				10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000,
				28000000, 29700000, 50000000, 54000000, 144000000, 148000000
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
			double num42 = (double)array2[l] - num29;
			if (num42 >= (double)num3 && num42 <= (double)num4)
			{
				int num43 = (int)((num42 - (double)num3) / (double)num16 * (double)num);
				g.DrawLine(band_edge_pen, num43, num18, num43, num2);
			}
		}
		for (int m = 1; m < num17; m++)
		{
			int num44 = 0;
			int num45 = num9 - m * num11;
			int num46 = (int)((double)(num9 - num45) * (double)num2 / (double)num13);
			g.DrawLine(hgrid_pen, 0, num46, num, num46);
			if (m != 0)
			{
				string text2 = (num9 - m * num11).ToString();
				if (text2.Length == 3)
				{
					num44 = (int)g.MeasureString("-", font9).Width - 2;
				}
				SizeF sizeF = g.MeasureString(text2, font9);
				int num47 = 0;
				switch (display_label_align)
				{
				case DisplayLabelAlignment.LEFT:
					num47 = num44 + 3;
					break;
				case DisplayLabelAlignment.CENTER:
					num47 = num28 + num44;
					break;
				case DisplayLabelAlignment.RIGHT:
					num47 = (int)((float)num - sizeF.Width - 3f);
					break;
				case DisplayLabelAlignment.AUTO:
					num47 = num44 + 3;
					break;
				case DisplayLabelAlignment.OFF:
					num47 = num;
					break;
				}
				num46 -= 8;
				if (num46 + 9 < num2)
				{
					g.DrawString(text2, font9, grid_text_brush, num47, num46);
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

	private unsafe bool DrawPanadapter(Graphics g, int rx)
	{
		int num = panRect.Width;
		int num2 = panRect.Height;
		DrawPanadapterGrid(g, rx);
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
		float num3 = 0f;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		float num7 = float.MinValue;
		bool flag = false;
		bool flag2 = false;
		int num8 = 0;
		int num9 = 0;
		if ((CurrentDisplayMode == DisplayMode.PANAFALL && nreceivers <= 2 && display_duplex) || (CurrentDisplayMode == DisplayMode.PANAFALL && nreceivers > 2) || (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex))
		{
			flag2 = true;
		}
		num6 = high_freq;
		num8 = spectrum_grid_max;
		num9 = spectrum_grid_min;
		if (rx_dsp_mode == DSPMode.DRM)
		{
			num5 += 12000;
			num6 += 12000;
		}
		int num10 = num8 - num9;
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
			for (int i = 0; i < num; i++)
			{
				float num11 = float.MinValue;
				float num12 = (float)i * num3 + (float)num4;
				int num13 = (int)Math.Floor(num12);
				int num14 = (int)Math.Floor(num12 + num3);
				if (flag && !flag2)
				{
					if ((double)num3 <= 1.0 || num13 == num14)
					{
						num11 = current_display_data[num13 % 4096] * ((float)num13 - num12 + 1f) + current_display_data[(num13 + 1) % 4096] * (num12 - (float)num13);
					}
					else
					{
						for (int j = num13; j < num14; j++)
						{
							if (current_display_data[j % 4096] > num11)
							{
								num11 = current_display_data[j % 4096];
							}
						}
					}
				}
				else
				{
					num11 = current_display_data[i];
				}
				num11 += rx_display_cal_offset;
				if (!flag || (flag & flag2))
				{
					num11 += preamp_offset;
				}
				if (num11 > num7)
				{
					num7 = num11;
					max_x = i;
				}
				points[i].X = i;
				points[i].Y = (int)Math.Floor(((float)num8 - num11) * (float)num2 / (float)num10);
				points[i].Y = Math.Min(points[i].Y, num2);
			}
		}
		catch (Exception value)
		{
			Trace.WriteLine(value);
		}
		max_y = num7;
		try
		{
			if (pan_fill)
			{
				points[num].X = num;
				points[num].Y = num2;
				points[num + 1].X = 0;
				points[num + 1].Y = num2;
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
				if (display_cursor_y <= num2)
				{
					g.DrawLine(pen, display_cursor_x, 0, display_cursor_x, num2);
					g.DrawLine(pen, 0, display_cursor_y, num, display_cursor_y);
				}
			}
		}
		catch (Exception)
		{
		}
		return true;
	}

	private unsafe bool DrawWaterfall(Graphics g, int rx)
	{
		int num = waterfallRect.Width;
		int num2 = waterfallRect.Height;
		if (waterfall_data == null || waterfall_data.Length < num)
		{
			waterfall_data = new float[num];
		}
		float num3 = 0f;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		float num8 = float.MinValue;
		bool flag = false;
		float num9 = float.MaxValue;
		float num10 = float.MaxValue;
		float num11 = float.MinValue;
		float num12 = float.MaxValue;
		int num13 = 0;
		int num14 = 0;
		int num15 = 0;
		bool flag2 = false;
		float num16 = 0f;
		float num17 = 0f;
		float num18 = 0f;
		float num19 = 0f;
		ColorScheme colorScheme = ColorScheme.enhanced;
		Color black = Color.Black;
		Color red = Color.Red;
		Color blue = Color.Blue;
		if ((CurrentDisplayMode == DisplayMode.PANAFALL && nreceivers <= 2 && display_duplex) || (CurrentDisplayMode == DisplayMode.PANAFALL && nreceivers > 2) || (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex))
		{
			flag2 = true;
		}
		colorScheme = color_scheme;
		black = waterfall_low_color;
		red = waterfall_mid_color;
		blue = waterfall_high_color;
		num6 = low_freq;
		num7 = high_freq;
		_ = spectrum_grid_max;
		_ = spectrum_grid_min;
		num17 = waterfall_high_threshold;
		if (waterfall_agc)
		{
			num18 = num19;
			num16 = waterfallPreviousMinValue;
		}
		else
		{
			num16 = waterfall_low_threshold;
		}
		if (console.PowerOn)
		{
			if (rx_dsp_mode == DSPMode.DRM)
			{
				num6 += 12000;
				num7 += 12000;
			}
			if (data_ready)
			{
				if ((!flag2 & flag) && (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU))
				{
					for (int i = 0; i < current_display_data.Length; i++)
					{
						current_display_data[i] = -200f;
					}
				}
				else
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
				}
				data_ready = false;
			}
			int num20 = 0;
			timer_waterfall.Stop();
			num20 = (int)timer_waterfall.DurationMsec;
			if (num20 > waterfall_update_period || num20 < 0)
			{
				timer_waterfall.Start();
				num4 = num7 - num6;
				num5 = 2048 + num6 * 4096 / sample_rate;
				num4 = (num7 - num6) * 4096 / sample_rate;
				if (num5 < 0)
				{
					num5 += 4096;
				}
				if (num4 - num5 > 4097)
				{
					num4 = 4096 - num5;
				}
				num3 = (float)num4 / (float)num;
				for (int j = 0; j < num; j++)
				{
					float num21 = float.MinValue;
					float num22 = (float)j * num3 + (float)num5;
					int num23 = (int)Math.Floor(num22);
					int num24 = (int)Math.Floor(num22 + num3);
					if (flag && !flag2)
					{
						if ((double)num3 <= 1.0 || num23 == num24)
						{
							num21 = current_display_data[num23 % 4096] * ((float)num23 - num22 + 1f) + current_display_data[(num23 + 1) % 4096] * (num22 - (float)num23);
						}
						else
						{
							for (int k = num23; k < num24; k++)
							{
								if (current_display_data[k % 4096] > num21)
								{
									num21 = current_display_data[k % 4096];
								}
							}
						}
					}
					else
					{
						num21 = current_display_data[j];
					}
					num21 += rx_display_cal_offset;
					num21 += preamp_offset - alex_preamp_offset;
					if (num21 > num8)
					{
						num8 = num21;
						max_x = j;
					}
					if (num21 < num9)
					{
						num9 = num21;
					}
					waterfall_data[j] = num21;
				}
				max_y = num8;
				num12 = num9;
				BitmapData bitmapData = waterfall_bmp.LockBits(new Rectangle(0, 0, waterfall_bmp.Width, waterfall_bmp.Height), ImageLockMode.ReadWrite, waterfall_bmp.PixelFormat);
				int num25 = 3;
				byte* ptr3 = null;
				int num26 = bitmapData.Stride * bitmapData.Height;
				Win32.memcpy(new IntPtr((int)bitmapData.Scan0 + bitmapData.Stride).ToPointer(), bitmapData.Scan0.ToPointer(), num26 - bitmapData.Stride);
				ptr3 = (byte*)(void*)bitmapData.Scan0;
				switch (colorScheme)
				{
				case ColorScheme.original:
				{
					for (int num48 = 0; num48 < num; num48++)
					{
						if (waterfall_data[num48] <= num16)
						{
							num13 = black.R;
							num14 = black.G;
							num15 = black.B;
						}
						else if (waterfall_data[num48] >= num17)
						{
							num13 = blue.R;
							num14 = blue.G;
							num15 = blue.B;
						}
						else
						{
							float num49 = (waterfall_data[num48] - num16) / (num17 - num16);
							if ((double)num49 <= 0.5)
							{
								num49 *= 2f;
								num13 = (int)((1f - num49) * (float)(int)black.R + num49 * (float)(int)red.R);
								num14 = (int)((1f - num49) * (float)(int)black.G + num49 * (float)(int)red.G);
								num15 = (int)((1f - num49) * (float)(int)black.B + num49 * (float)(int)red.B);
							}
							else
							{
								num49 = (float)((double)num49 - 0.5) * 2f;
								num13 = (int)((1f - num49) * (float)(int)red.R + num49 * (float)(int)blue.R);
								num14 = (int)((1f - num49) * (float)(int)red.G + num49 * (float)(int)blue.G);
								num15 = (int)((1f - num49) * (float)(int)red.B + num49 * (float)(int)blue.B);
							}
						}
						ptr3[num48 * num25] = (byte)num15;
						ptr3[num48 * num25 + 1] = (byte)num14;
						ptr3[num48 * num25 + 2] = (byte)num13;
					}
					break;
				}
				case ColorScheme.enhanced:
				{
					for (int m = 0; m < num; m++)
					{
						if (waterfall_data[m] <= num16)
						{
							num13 = black.R;
							num14 = black.G;
							num15 = black.B;
						}
						else if (waterfall_data[m] >= num17)
						{
							num13 = 192;
							num14 = 124;
							num15 = 255;
						}
						else
						{
							float num29 = num17 - num16;
							float num30 = (waterfall_data[m] - num16) / num29;
							if (num30 < 2f / 9f)
							{
								float num31 = num30 / (2f / 9f);
								num13 = (int)((1.0 - (double)num31) * (double)(int)black.R);
								num14 = (int)((1.0 - (double)num31) * (double)(int)black.G);
								num15 = (int)((float)(int)black.B + num31 * (float)(255 - black.B));
							}
							else if (num30 < 1f / 3f)
							{
								float num32 = (num30 - 2f / 9f) / (1f / 9f);
								num13 = 0;
								num14 = (int)(num32 * 255f);
								num15 = 255;
							}
							else if (num30 < 4f / 9f)
							{
								float num33 = (num30 - 1f / 3f) / (1f / 9f);
								num13 = 0;
								num14 = 255;
								num15 = (int)((1.0 - (double)num33) * 255.0);
							}
							else if (num30 < 5f / 9f)
							{
								num13 = (int)((num30 - 4f / 9f) / (1f / 9f) * 255f);
								num14 = 255;
								num15 = 0;
							}
							else if (num30 < 7f / 9f)
							{
								float num34 = (num30 - 5f / 9f) / (2f / 9f);
								num13 = 255;
								num14 = (int)((1.0 - (double)num34) * 255.0);
								num15 = 0;
							}
							else if (num30 < 8f / 9f)
							{
								float num35 = (num30 - 7f / 9f) / (1f / 9f);
								num13 = 255;
								num14 = 0;
								num15 = (int)(num35 * 255f);
							}
							else
							{
								float num36 = (num30 - 8f / 9f) / (1f / 9f);
								num13 = (int)((0.75 + 0.25 * (1.0 - (double)num36)) * 255.0);
								num14 = (int)((double)(num36 * 255f) * 0.5);
								num15 = 255;
							}
						}
						if (num18 > waterfall_data[m])
						{
							num18 = waterfall_data[m];
						}
						ptr3[m * num25] = (byte)num15;
						ptr3[m * num25 + 1] = (byte)num14;
						ptr3[m * num25 + 2] = (byte)num13;
					}
					break;
				}
				case ColorScheme.SPECTRAN:
				{
					for (int num39 = 0; num39 < num; num39++)
					{
						if (waterfall_data[num39] <= num16)
						{
							num13 = 0;
							num14 = 0;
							num15 = 0;
						}
						else if (waterfall_data[num39] >= WaterfallHighThreshold)
						{
							num13 = 240;
							num14 = 240;
							num15 = 240;
						}
						else
						{
							float num40 = WaterfallHighThreshold - num16;
							float num41 = waterfall_data[num39] - num16;
							float num42 = 100f * num41 / num40;
							if (num42 < 5f)
							{
								num13 = (num14 = 0);
								num15 = (int)num42 * 5;
							}
							else if (num42 < 11f)
							{
								num13 = (num14 = 0);
								num15 = (int)num42 * 5;
							}
							else if (num42 < 22f)
							{
								num13 = (num14 = 0);
								num15 = (int)num42 * 5;
							}
							else if (num42 < 44f)
							{
								num13 = (num14 = 0);
								num15 = (int)num42 * 5;
							}
							else if (num42 < 51f)
							{
								num13 = (num14 = 0);
								num15 = (int)num42 * 5;
							}
							else if (num42 < 66f)
							{
								num13 = (num14 = (int)(num42 - 50f) * 2);
								num15 = 255;
							}
							else if (num42 < 77f)
							{
								num13 = (num14 = (int)(num42 - 50f) * 3);
								num15 = 255;
							}
							else if (num42 < 88f)
							{
								num13 = (num14 = (int)(num42 - 50f) * 4);
								num15 = 255;
							}
							else if (num42 < 99f)
							{
								num13 = (num14 = (int)(num42 - 50f) * 5);
								num15 = 255;
							}
						}
						if (num18 > waterfall_data[num39])
						{
							num18 = waterfall_data[num39];
						}
						ptr3[num39 * num25] = (byte)num15;
						ptr3[num39 * num25 + 1] = (byte)num14;
						ptr3[num39 * num25 + 2] = (byte)num13;
					}
					break;
				}
				case ColorScheme.BLACKWHITE:
				{
					for (int num50 = 0; num50 < num; num50++)
					{
						if (waterfall_data[num50] <= num16)
						{
							num13 = 0;
							num14 = 0;
							num15 = 0;
						}
						else if (waterfall_data[num50] >= WaterfallHighThreshold)
						{
							num13 = 255;
							num14 = 255;
							num15 = 255;
						}
						else
						{
							float num51 = WaterfallHighThreshold - num16;
							float num52 = waterfall_data[num50] - num16;
							num13 = (int)(100f * num52 / num51 / 100f * 255f);
							num14 = num13;
							num15 = num13;
						}
						if (num18 > waterfall_data[num50])
						{
							num18 = waterfall_data[num50];
						}
						ptr3[num50 * num25] = (byte)num15;
						ptr3[num50 * num25 + 1] = (byte)num14;
						ptr3[num50 * num25 + 2] = (byte)num13;
					}
					break;
				}
				case ColorScheme.LinLog:
				{
					for (int num43 = 0; num43 < num; num43++)
					{
						if (waterfall_data[num43] <= num16)
						{
							num13 = 0;
							num14 = 0;
							num15 = 0;
						}
						else if (waterfall_data[num43] >= num17)
						{
							num13 = 252;
							num14 = 252;
							num15 = 252;
						}
						else
						{
							float num44 = num17 - num16;
							float num45 = waterfall_data[num43] - num16 + (float)LinLogCor;
							float num46 = 1024f * num45 / num44;
							float num47 = (float)Math.Log10(1024.0);
							if (num46 == 0f)
							{
								num46 = 0.001f;
							}
							num46 = (float)Math.Log10(num46);
							if (num46 < num47 / 23f)
							{
								num13 = 0;
								num14 = 0;
								num15 = 0;
							}
							else if (num46 < 2f * num47 / 23f)
							{
								num13 = 32;
								num14 = 0;
								num15 = 0;
							}
							else if (num46 < 3f * num47 / 23f)
							{
								num13 = 64;
								num14 = 0;
								num15 = 0;
							}
							else if (num46 < 4f * num47 / 23f)
							{
								num13 = 96;
								num14 = 0;
								num15 = 0;
							}
							else if (num46 < 5f * num47 / 23f)
							{
								num13 = 104;
								num14 = 40;
								num15 = 0;
							}
							else if (num46 < 6f * num47 / 23f)
							{
								num13 = 112;
								num14 = 60;
								num15 = 0;
							}
							else if (num46 < 7f * num47 / 23f)
							{
								num13 = 116;
								num14 = 88;
								num15 = 0;
							}
							else if (num46 < 8f * num47 / 23f)
							{
								num13 = 92;
								num14 = 112;
								num15 = 0;
							}
							else if (num46 < 9f * num47 / 23f)
							{
								num13 = 80;
								num14 = 132;
								num15 = 0;
							}
							else if (num46 < 10f * num47 / 23f)
							{
								num13 = 20;
								num14 = 140;
								num15 = 0;
							}
							else if (num46 < 11f * num47 / 23f)
							{
								num13 = 0;
								num14 = 160;
								num15 = 40;
							}
							else if (num46 < 12f * num47 / 23f)
							{
								num13 = 0;
								num14 = 160;
								num15 = 120;
							}
							else if (num46 < 13f * num47 / 23f)
							{
								num13 = 0;
								num14 = 140;
								num15 = 148;
							}
							else if (num46 < 14f * num47 / 23f)
							{
								num13 = 0;
								num14 = 132;
								num15 = 192;
							}
							else if (num46 < 15f * num47 / 23f)
							{
								num13 = 0;
								num14 = 112;
								num15 = 200;
							}
							else if (num46 < 16f * num47 / 23f)
							{
								num13 = 0;
								num14 = 88;
								num15 = 208;
							}
							else if (num46 < 17f * num47 / 23f)
							{
								num13 = 0;
								num14 = 60;
								num15 = 232;
							}
							else if (num46 < 18f * num47 / 23f)
							{
								num13 = 0;
								num14 = 40;
								num15 = 252;
							}
							else if (num46 < 19f * num47 / 23f)
							{
								num13 = 80;
								num14 = 80;
								num15 = 252;
							}
							else if (num46 < 20f * num47 / 23f)
							{
								num13 = 124;
								num14 = 124;
								num15 = 252;
							}
							else if (num46 < 21f * num47 / 23f)
							{
								num13 = 172;
								num14 = 172;
								num15 = 252;
							}
							else if (num46 >= 21f * num47 / 23f)
							{
								num13 = 252;
								num14 = 252;
								num15 = 252;
							}
							else
							{
								num13 = 0;
								num14 = 0;
								num15 = 0;
							}
						}
						if (num18 > waterfall_data[num43])
						{
							num18 = waterfall_data[num43];
						}
						ptr3[num43 * num25] = (byte)num13;
						ptr3[num43 * num25 + 1] = (byte)num14;
						ptr3[num43 * num25 + 2] = (byte)num15;
					}
					break;
				}
				case ColorScheme.LinRad:
				{
					for (int n = 0; n < num; n++)
					{
						if (waterfall_data[n] <= num16)
						{
							num13 = 0;
							num14 = 0;
							num15 = 0;
						}
						else if (waterfall_data[n] >= num17)
						{
							num13 = 252;
							num14 = 252;
							num15 = 252;
						}
						else
						{
							float num37 = num17 - num16;
							float num38 = (waterfall_data[n] - num16 + (float)LinCor) / num37;
							if (num38 < 1f / 23f)
							{
								num13 = 0;
								num14 = 0;
								num15 = 0;
							}
							else if (num38 < 0.08695652f)
							{
								num13 = 32;
								num14 = 0;
								num15 = 0;
							}
							else if (num38 < 0.13043478f)
							{
								num13 = 64;
								num14 = 0;
								num15 = 0;
							}
							else if (num38 < 0.17391305f)
							{
								num13 = 96;
								num14 = 0;
								num15 = 0;
							}
							else if (num38 < 0.2173913f)
							{
								num13 = 104;
								num14 = 40;
								num15 = 0;
							}
							else if (num38 < 0.26086956f)
							{
								num13 = 112;
								num14 = 60;
								num15 = 0;
							}
							else if (num38 < 0.3043478f)
							{
								num13 = 116;
								num14 = 88;
								num15 = 0;
							}
							else if (num38 < 0.3478261f)
							{
								num13 = 92;
								num14 = 112;
								num15 = 0;
							}
							else if (num38 < 0.39130434f)
							{
								num13 = 80;
								num14 = 132;
								num15 = 0;
							}
							else if (num38 < 0.4347826f)
							{
								num13 = 20;
								num14 = 140;
								num15 = 0;
							}
							else if (num38 < 0.47826087f)
							{
								num13 = 0;
								num14 = 160;
								num15 = 40;
							}
							else if (num38 < 0.5217391f)
							{
								num13 = 0;
								num14 = 160;
								num15 = 120;
							}
							else if (num38 < 0.5652174f)
							{
								num13 = 0;
								num14 = 140;
								num15 = 148;
							}
							else if (num38 < 0.6086956f)
							{
								num13 = 0;
								num14 = 132;
								num15 = 192;
							}
							else if (num38 < 0.65217394f)
							{
								num13 = 0;
								num14 = 112;
								num15 = 200;
							}
							else if (num38 < 0.6956522f)
							{
								num13 = 0;
								num14 = 88;
								num15 = 208;
							}
							else if (num38 < 0.73913044f)
							{
								num13 = 0;
								num14 = 60;
								num15 = 232;
							}
							else if (num38 < 0.7826087f)
							{
								num13 = 0;
								num14 = 40;
								num15 = 252;
							}
							else if (num38 < 0.82608694f)
							{
								num13 = 80;
								num14 = 80;
								num15 = 252;
							}
							else if (num38 < 0.8695652f)
							{
								num13 = 124;
								num14 = 124;
								num15 = 252;
							}
							else if (num38 < 0.9130435f)
							{
								num13 = 172;
								num14 = 172;
								num15 = 252;
							}
							else if (num38 >= 0.9130435f)
							{
								num13 = 252;
								num14 = 252;
								num15 = 252;
							}
							else
							{
								num13 = 0;
								num14 = 0;
								num15 = 0;
							}
						}
						if (num18 > waterfall_data[n])
						{
							num18 = waterfall_data[n];
						}
						ptr3[n * num25] = (byte)num13;
						ptr3[n * num25 + 1] = (byte)num14;
						ptr3[n * num25 + 2] = (byte)num15;
					}
					break;
				}
				case ColorScheme.LinAuto:
				{
					for (int l = 0; l < num; l++)
					{
						num10 = num12 - 5f;
						num11 = max_y;
						if (waterfall_data[l] <= num10)
						{
							num13 = 0;
							num14 = 0;
							num15 = 0;
						}
						else if (waterfall_data[l] >= num11)
						{
							num13 = 252;
							num14 = 252;
							num15 = 252;
						}
						else
						{
							float num27 = num11 - num10;
							float num28 = (waterfall_data[l] - num10) / num27;
							if (num28 < 1f / 23f)
							{
								num13 = 0;
								num14 = 0;
								num15 = 0;
							}
							else if (num28 < 0.08695652f)
							{
								num13 = 32;
								num14 = 0;
								num15 = 0;
							}
							else if (num28 < 0.13043478f)
							{
								num13 = 64;
								num14 = 0;
								num15 = 0;
							}
							else if (num28 < 0.17391305f)
							{
								num13 = 96;
								num14 = 0;
								num15 = 0;
							}
							else if (num28 < 0.2173913f)
							{
								num13 = 104;
								num14 = 40;
								num15 = 0;
							}
							else if (num28 < 0.26086956f)
							{
								num13 = 112;
								num14 = 60;
								num15 = 0;
							}
							else if (num28 < 0.3043478f)
							{
								num13 = 116;
								num14 = 88;
								num15 = 0;
							}
							else if (num28 < 0.3478261f)
							{
								num13 = 92;
								num14 = 112;
								num15 = 0;
							}
							else if (num28 < 0.39130434f)
							{
								num13 = 80;
								num14 = 132;
								num15 = 0;
							}
							else if (num28 < 0.4347826f)
							{
								num13 = 20;
								num14 = 140;
								num15 = 0;
							}
							else if (num28 < 0.47826087f)
							{
								num13 = 0;
								num14 = 160;
								num15 = 40;
							}
							else if (num28 < 0.5217391f)
							{
								num13 = 0;
								num14 = 160;
								num15 = 120;
							}
							else if (num28 < 0.5652174f)
							{
								num13 = 0;
								num14 = 140;
								num15 = 148;
							}
							else if (num28 < 0.6086956f)
							{
								num13 = 0;
								num14 = 132;
								num15 = 192;
							}
							else if (num28 < 0.65217394f)
							{
								num13 = 0;
								num14 = 112;
								num15 = 200;
							}
							else if (num28 < 0.6956522f)
							{
								num13 = 0;
								num14 = 88;
								num15 = 208;
							}
							else if (num28 < 0.73913044f)
							{
								num13 = 0;
								num14 = 60;
								num15 = 232;
							}
							else if (num28 < 0.7826087f)
							{
								num13 = 0;
								num14 = 40;
								num15 = 252;
							}
							else if (num28 < 0.82608694f)
							{
								num13 = 80;
								num14 = 80;
								num15 = 252;
							}
							else if (num28 < 0.8695652f)
							{
								num13 = 124;
								num14 = 124;
								num15 = 252;
							}
							else if (num28 < 0.9130435f)
							{
								num13 = 172;
								num14 = 172;
								num15 = 252;
							}
							else if (num28 >= 0.9130435f)
							{
								num13 = 252;
								num14 = 252;
								num15 = 252;
							}
							else
							{
								num13 = 0;
								num14 = 0;
								num15 = 0;
							}
						}
						ptr3[l * num25] = (byte)num13;
						ptr3[l * num25 + 1] = (byte)num14;
						ptr3[l * num25 + 2] = (byte)num15;
					}
					break;
				}
				}
				waterfall_bmp.UnlockBits(bitmapData);
				waterfallPreviousMinValue = (waterfallPreviousMinValue * 8f + num18 * 2f) / 10f + 1f;
			}
			g.DrawImageUnscaled(waterfall_bmp, 0, waterfallRect.Top);
		}
		waterfall_counter++;
		if (current_click_tune_mode != ClickTuneMode.Off)
		{
			Pen pen = ((current_click_tune_mode == ClickTuneMode.VFOA) ? grid_text_pen : Pens.Red);
			if (display_cursor_y <= num2)
			{
				g.DrawLine(pen, display_cursor_x, 0, display_cursor_x, num2);
				if (ShowCTHLine)
				{
					g.DrawLine(pen, 0, display_cursor_y, num, display_cursor_y);
				}
			}
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
		waterfallRect = new Rectangle(freqScalePanRect.Left, freqScalePanRect.Top + freqScalePanRect.Height, freqScalePanRect.Width, num2 - freqScalePanRect.Top - freqScalePanRect.Height);
		dBmScalePanRect = new Rectangle(panRect.Left, displayTop, 35, panRect.Height);
		secScaleWaterfallRect = new Rectangle(waterfallRect.Left, freqScalePanRect.Top + freqScalePanRect.Height, 45, waterfallRect.Height);
		if (waterfallRect.Height == 0)
		{
			waterfallRect.Height = 1;
		}
		waterfall_bmp = new Bitmap(waterfallRect.Width, waterfallRect.Height, PixelFormat.Format24bppRgb);
	}

	private void getRegion(Point p)
	{
		if (FreqScalePanRect.Contains(p))
		{
			mouseRegion = DisplayRegion.freqScalePanadapterRegion;
		}
		else if (DBMScalePanRect.Contains(p))
		{
			mouseRegion = DisplayRegion.dBmScalePanadapterRegion;
		}
		else if (Math.Abs(p.X - FilterRect.Left) < 3 && PanRect.Contains(p))
		{
			mouseRegion = DisplayRegion.filterRegionLow;
		}
		else if (Math.Abs(p.X - FilterRect.Right) < 3 && PanRect.Contains(p))
		{
			mouseRegion = DisplayRegion.filterRegionHigh;
		}
		else if (FilterRect.Contains(p))
		{
			mouseRegion = DisplayRegion.filterRegion;
		}
		else if (PanRect.Contains(p))
		{
			mouseRegion = DisplayRegion.panadapterRegion;
		}
		else if (WaterfallRect.Contains(p))
		{
			mouseRegion = DisplayRegion.waterfallRegion;
		}
		else
		{
			mouseRegion = DisplayRegion.elsewhere;
		}
	}

	public void UpdateGraphicsBuffer()
	{
		if (pDisplay_buffer != null)
		{
			pDisplay_buffer.Dispose();
			base.Image.Dispose();
		}
		pDisplay_buffer = new Bitmap(base.Width, base.Height);
		base.Image = pDisplay_buffer;
	}

	public bool Cancel_Display1()
	{
		cancelTokenSource1.Cancel();
		return true;
	}

	public void StartDisplay(int rx)
	{
		UpdateGraphicsBuffer();
		draw_display_task = Task.Factory.StartNew(delegate
		{
			while (!cancelTokenSource1.IsCancellationRequested)
			{
				if (!pauseDisplayThread)
				{
					RunDisplay(rx);
					using (Graphics graphics = Graphics.FromImage(pDisplay_buffer))
					{
						graphics.Clear(Color.Transparent);
						RenderGDIPlus(rx, graphics);
					}
					Invoke((Action)delegate
					{
						base.Image = pDisplay_buffer;
						Refresh();
					});
				}
				Thread.Sleep(60);
			}
		}, cancelTokenSource1.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
	}

	private unsafe void RunDisplay(int rx)
	{
		if ((!DataReady || !WaterfallDataReady) && (!DataReady || !WaterfallDataReady))
		{
			int flag = 0;
			fixed (float* pix = &new_display_data[0])
			{
				SpecHPSDRDLL.GetPixels(rx - 2, 0, pix, ref flag);
			}
			DataReady = true;
		}
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
					mouseDownPos = mousePos;
				}
			}
			if (e.Button == MouseButtons.Right)
			{
				Point point2 = Point.Subtract(mouseDownPos, size);
				if (point2.X > 0)
				{
					ZoomFactor += 0.01;
				}
				else if (point2.X < 0)
				{
					ZoomFactor -= 0.01;
				}
				mouseDownPos = mousePos;
			}
			break;
		case DisplayRegion.dBmScalePanadapterRegion:
			if (gridminmaxadjust)
			{
				double num = (double)(e.Y - grid_minmax_drag_start_point.Y) / 10.0 * 5.0;
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
				double num4 = (double)(e.Y - grid_minmax_drag_start_point.Y) / 10.0 * 5.0;
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

	public void initAnalyzer()
	{
		if (!init)
		{
			return;
		}
		IntPtr flp = GCHandle.Alloc(new int[1], GCHandleType.Pinned).AddrOfPinnedObject();
		Math.Exp(-1.0 / ((double)frame_rate * tau));
		int ovrlp = 0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int max_w = 0;
		int num4 = data_type;
		if (num4 != 0 && num4 == 1)
		{
			ovrlp = (int)Math.Max(0.0, Math.Ceiling((double)fft_size - (double)sample_rate / (double)frame_rate));
			num = (int)Math.Floor(0.017 * (double)fft_size);
			double num5 = (double)sample_rate / (double)fft_size;
			int num6 = fft_size - 2 * num;
			double num7 = (double)num6 * num5;
			double num8 = Math.Log10(9.0 * z_factor + 1.0);
			int num9 = (int)((double)num6 * (1.0 - 0.99 * num8));
			num2 = (int)Math.Floor(p_slider * (double)(num6 - num9));
			num3 = num6 - num9 - num2;
			int num10 = (int)(freq_offset / num5);
			if ((num3 -= num10) < 0)
			{
				num3 = 0;
			}
			num2 = num6 - num9 - num3;
			low_freq = -(int)(0.5 * num7 - (double)num2 * num5 + num5 / 2.0);
			high_freq = (int)(0.5 * num7 - (double)num3 * num5 - num5 / 2.0);
			max_w = fft_size + (int)Math.Min(0.1 * (double)sample_rate, 0.1 * (double)fft_size * (double)frame_rate);
		}
		SpecHPSDRDLL.SetAnalyzer(display_id, 1, 1, data_type, flp, fft_size, cmaster.GetBuffSize(sample_rate), window_type, kaiser_pi, ovrlp, num, num2, num3, pixels, 1, 0, 0.0, 0.0, max_w);
	}
}
