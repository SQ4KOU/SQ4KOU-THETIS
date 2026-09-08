using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace Thetis;

internal class Display
{
	private sealed class BandEdgeRegionCacheDX2D
	{
		private static readonly int[] s_usBandEdges = new int[28]
		{
			135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000, 5330500, 5406400,
			7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
			24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000
		};

		private static readonly int[] s_germanyBandEdges = new int[28]
		{
			135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000, 5351500, 5366500,
			7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
			24890000, 24990000, 28000000, 29700000, 50000000, 51000000, 144000000, 146000000
		};

		private static readonly int[] s_region1BandEdges = new int[28]
		{
			135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000, 5351500, 5366500,
			7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
			24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 146000000
		};

		private static readonly int[] s_region2BandEdges = new int[28]
		{
			135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000, 5351500, 5366500,
			7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
			24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000
		};

		private static readonly int[] s_region3BandEdges = new int[26]
		{
			135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 3900000, 7000000, 7300000,
			10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000,
			28000000, 29700000, 50000000, 54000000, 144000000, 148000000
		};

		private static readonly int[] s_spainBandEdges = new int[26]
		{
			135700, 137800, 472000, 479000, 1810000, 1850000, 3500000, 3800000, 7000000, 7200000,
			10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000,
			28000000, 29700000, 50000000, 52000000, 144000000, 148000000
		};

		private static readonly int[] s_australiaBandEdges = new int[26]
		{
			135700, 137800, 472000, 479000, 1800000, 1875000, 3500000, 3800000, 7000000, 7300000,
			10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000,
			28000000, 29700000, 50000000, 54000000, 144000000, 148000000
		};

		private static readonly int[] s_ukBandEdges = new int[28]
		{
			135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000, 5258500, 5406500,
			7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
			24890000, 24990000, 28000000, 29700000, 50000000, 52000000, 144000000, 148000000
		};

		private static readonly int[] s_indiaBandEdges = new int[22]
		{
			1810000, 1860000, 3500000, 3900000, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000,
			18068000, 18168000, 21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 54000000,
			144000000, 148000000
		};

		private static readonly int[] s_norwayBandEdges = new int[24]
		{
			1800000, 2000000, 3500000, 4000000, 5260000, 5410000, 7000000, 7300000, 10100000, 10150000,
			14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000, 28000000, 29700000,
			50000000, 54000000, 144000000, 148000000
		};

		private static readonly int[] s_japanBandEdges = new int[38]
		{
			135700, 137800, 472000, 479000, 1800000, 1875000, 1907500, 1912500, 3500000, 3575000,
			3599000, 3612000, 3680000, 3687000, 3702000, 3716000, 3745000, 3770000, 3791000, 3805000,
			7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
			24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 146000000
		};

		private static readonly int[] s_defaultBandEdges = new int[26]
		{
			135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000, 7000000, 7300000,
			10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000, 24890000, 24990000,
			28000000, 29700000, 50000000, 54000000, 144000000, 148000000
		};

		private bool m_bInitialised;

		private FRSRegion m_currentRegion = FRSRegion.FIRST;

		private int[] m_edges = new int[0];

		private HashSet<int> m_edgeSet = new HashSet<int>();

		public int[] Edges => m_edges;

		public void Update(FRSRegion region)
		{
			if (!m_bInitialised || m_currentRegion != region)
			{
				switch (region)
				{
				case FRSRegion.US:
					m_edges = s_usBandEdges;
					break;
				case FRSRegion.Germany:
					m_edges = s_germanyBandEdges;
					break;
				case FRSRegion.Region1:
					m_edges = s_region1BandEdges;
					break;
				case FRSRegion.Region2:
					m_edges = s_region2BandEdges;
					break;
				case FRSRegion.Region3:
					m_edges = s_region3BandEdges;
					break;
				case FRSRegion.Spain:
					m_edges = s_spainBandEdges;
					break;
				case FRSRegion.Australia:
					m_edges = s_australiaBandEdges;
					break;
				case FRSRegion.UK:
					m_edges = s_ukBandEdges;
					break;
				case FRSRegion.India:
					m_edges = s_indiaBandEdges;
					break;
				case FRSRegion.Norway:
					m_edges = s_norwayBandEdges;
					break;
				case FRSRegion.Japan:
					m_edges = s_japanBandEdges;
					break;
				default:
					m_edges = s_defaultBandEdges;
					break;
				}
				m_edgeSet = new HashSet<int>(m_edges);
				m_currentRegion = region;
				m_bInitialised = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(int frequencyHz)
		{
			return m_edgeSet.Contains(frequencyHz);
		}
	}

	public enum WaterfallRenderQuality
	{
		Low,
		Medium,
		High
	}

	private struct WaterfallAgcCacheKey(bool isTx, int bandValue) : IEquatable<WaterfallAgcCacheKey>
	{
		public readonly bool IsTx = isTx;

		public readonly int BandValue = bandValue;

		public bool Equals(WaterfallAgcCacheKey other)
		{
			if (IsTx == other.IsTx)
			{
				return BandValue == other.BandValue;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is WaterfallAgcCacheKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (IsTx ? 397 : 211) ^ BandValue;
		}
	}

	private struct WaterfallAgcCacheEntry
	{
		public float PreviousMin;
	}

	public class AdaptorInfo
	{
		public string Description { get; set; }

		public bool IsHardware { get; set; }

		public bool IsDefaultHardware { get; set; }

		public bool IsDisplayAttached { get; set; }

		public int VendorId { get; set; }

		public int DeviceId { get; set; }

		public long DedicatedVideoMemory { get; set; }

		public long DedicatedSystemMemory { get; set; }

		public long SharedSystemMemory { get; set; }

		public long AdapterLuid { get; set; }
	}

	private struct Maximums
	{
		public float max_dBm;

		public int X;

		public int MaxY_pixel;

		public bool Enabled;

		public double Time;
	}

	private class clsNotchCoords
	{
		public int _c_x;

		public int _left_x;

		public int _right_x;

		public bool _Use;

		public int _widthHz;

		public clsNotchCoords(int nC_x, int nLeft_X, int nRight_X, bool bUse, int nWidthHz)
		{
			_c_x = nC_x;
			_left_x = nLeft_X;
			_right_x = nRight_X;
			_Use = bUse;
			_widthHz = nWidthHz;
		}
	}

	private class SnowFlake
	{
		public float X { get; set; }

		public float Y { get; set; }

		public float FallSpeed { get; set; }

		public float Alpha { get; set; }

		public float XShift { get; set; }

		public bool Settled { get; set; }

		public float Size { get; set; }

		public bool Finished { get; set; }

		public SnowFlake(int width)
		{
			X = _rnd.Next(width);
			Y = 0f;
			FallSpeed = _rnd.NextFloat(0.1f, 1.5f);
			Alpha = _rnd.Next(64, 256);
			XShift = _rnd.NextFloat(-0.5f, 0.5f);
			Settled = false;
			Size = _rnd.NextFloat(1f, 2f);
			Finished = false;
		}

		public void Update()
		{
			if (!Settled)
			{
				Y += FallSpeed;
				X += XShift;
				int num = _rnd.Next(0, 10);
				if (num == 9 && XShift < 0.5f)
				{
					XShift += 0.1f;
				}
				else if (num == 0 && XShift > -0.5f)
				{
					XShift -= 0.1f;
				}
				if (Y >= (float)(Target.Height - 1))
				{
					Y = Target.Height - 1;
					Settled = true;
				}
				if (X > (float)(Target.Width - 1))
				{
					X = 0f;
				}
				else if (X < 0f)
				{
					X = Target.Width - 1;
				}
			}
			else
			{
				Alpha -= 0.5f;
				if (Alpha <= 0f)
				{
					Alpha = 0f;
					Finished = true;
				}
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct DEVMODE
	{
		private const int CCHDEVICENAME = 32;

		private const int CCHFORMNAME = 32;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmDeviceName;

		public ushort dmSpecVersion;

		public ushort dmDriverVersion;

		public ushort dmSize;

		public ushort dmDriverExtra;

		public uint dmFields;

		public int dmPositionX;

		public int dmPositionY;

		public uint dmDisplayOrientation;

		public uint dmDisplayFixedOutput;

		public short dmColor;

		public short dmDuplex;

		public short dmYResolution;

		public short dmTTOption;

		public short dmCollate;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmFormName;

		public ushort dmLogPixels;

		public uint dmBitsPerPel;

		public uint dmPelsWidth;

		public uint dmPelsHeight;

		public uint dmDisplayFlags;

		public uint dmDisplayFrequency;
	}

	private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
	{
		internal static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

		public bool Equals(T x, T y)
		{
			return x == y;
		}

		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private static readonly BandEdgeRegionCacheDX2D m_bandEdgeRegionCacheDX2D = new BandEdgeRegionCacheDX2D();

	private const SharpDX.Direct2D1.AlphaMode ALPHA_MODE = SharpDX.Direct2D1.AlphaMode.Premultiplied;

	public const float CLEAR_FLAG = -999.999f;

	public const int BUFFER_SIZE = 16384;

	public static Console console;

	public static string background_image = null;

	private static int[] histogram_data = null;

	private static int[] histogram_history;

	public static float[] new_display_data;

	public static float[] current_display_data;

	public static float[] new_display_data_bottom;

	public static float[] current_display_data_bottom;

	public static float[] current_display_data_copy;

	public static float[] current_display_data_bottom_copy;

	public static float[] new_waterfall_data;

	public static float[] current_waterfall_data;

	public static float[] new_waterfall_data_bottom;

	public static float[] current_waterfall_data_bottom;

	private static readonly double[] _pendingWaterfallPixelRef = new double[2]
	{
		double.NaN,
		double.NaN
	};

	private static readonly double[] _currentWaterfallPixelRef = new double[2]
	{
		double.NaN,
		double.NaN
	};

	private static readonly double[] _waterfallBitmapCenterMHz = new double[2]
	{
		double.NaN,
		double.NaN
	};

	private static readonly double[] _waterfallBitmapSpanHz = new double[2]
	{
		double.NaN,
		double.NaN
	};

	private static readonly double[] _waterfallBitmapShiftRemainderPixels = new double[2];

	private static readonly int[] _waterfallBitmapWidths = new int[2];

	private static bool _allowWaterfallSmear = false;

	private static float[] waterfall_data;

	public static float[] current_waterfall_data_copy;

	public static float[] current_waterfall_data_bottom_copy;

	private static SharpDX.Direct2D1.Bitmap _waterfall_bmp_dx2d = null;

	private static SharpDX.Direct2D1.Bitmap _waterfall_bmp2_dx2d = null;

	private static WaterfallGPURenderer _waterfallGPU1 = null;

	private static WaterfallGPURenderer _waterfallGPU2 = null;

	private static GPUWaterfallPipeline _gpuFFT1 = null;

	private static GPUWaterfallPipeline _gpuFFT2 = null;

	private static float[] _gpuIQbufI = null;

	private static float[] _gpuIQbufQ = null;

	private static float[] _gpuIQaccumI = null;

	private static float[] _gpuIQaccumQ = null;

	private static float[][] _gpuIQringI = new float[2][];

	private static float[][] _gpuIQringQ = new float[2][];

	private static int[] _gpuIQringHead = new int[2];

	private static int[] _gpuIQringCount = new int[2];

	private static int[] _gpuSampleCredit = new int[2];

	private static bool[] _gpuFirstFillDone = new bool[2];

	private static int _gpuWaterfallDebugSkip = 0;

	private static int[] _gpuRowLogSkip = new int[2];

	private static bool[] _gpuPipeFailLogged = new bool[2];

	private static long[] _gpuLastDropLogMs = new long[2];

	private static readonly int[] _gpuCalOutlierSkip = new int[2];

	private static float _gpuCalOffsetRX1 = 0f;

	private static float _gpuCalOffsetRX2 = 0f;

	private static double[] _gpuLastEffectiveOverlap = new double[2] { -1.0, -1.0 };

	private static bool _gpuCalInitRX1 = false;

	private static bool _gpuCalInitRX2 = false;

	private static int _gpuCalStartupCountRX1 = 0;

	private static int _gpuCalStartupCountRX2 = 0;

	private const int GPU_CAL_STARTUP_FRAMES = 10;

	private static int _gpuLastFFTSizeRX1 = 0;

	private static int _gpuLastFFTSizeRX2 = 0;

	private static float[] _gpuCalReferenceRowRX1 = null;

	private static float[] _gpuCalReferenceRowRX2 = null;

	private static float[] _gpuMedianBuffer = null;

	private const int GPU_WATERFALL_IQ_CAPACITY = 524288;

	private static bool _gpuWaterfallPipelineEnabled = false;

	private static int _gpuWaterfallFFTSize = 16384;

	private static int _gpuWaterfallOverlapPercent = 85;

	private static GPUWaterfallWindowType _gpuWaterfallWindowType = GPUWaterfallWindowType.Nuttall;

	private static double _gpuWaterfallKaiserBeta = 6.0;

	private static GPUWaterfallMagnitudeMode _gpuWaterfallMagnitudeMode = GPUWaterfallMagnitudeMode.PeakHoldPower;

	private static bool _gpuWaterfallAutoOverlap = false;

	private static int _gpuWaterfallLanczosWindow = 3;

	private static GPUWaterfallResamplingMode _gpuWaterfallResamplingMode = GPUWaterfallResamplingMode.Quality;

	private static bool _gpuWaterfallLinearDraw = true;

	private static System.Drawing.Image _backgroundImageSource = null;

	private static bool _detachPanafallEnabled = false;

	private static DisplayMode _displayModeBeforeDetach = DisplayMode.PANAFALL;

	private static bool _wfDrawnRX1ThisFrame = false;

	private static bool _wfDrawnRX2ThisFrame = false;

	private static DetachedPanafallRenderer _detachedRenderer = null;

	private static volatile DetachedPanafallRenderer _pendingDetachRendererForTeardown = null;

	private static volatile bool _detachedResizePending = false;

	private static int _detachedResizeW = 0;

	private static int _detachedResizeH = 0;

	private static long _detachedResizeLastApplyMs = 0L;

	private const int DETACHED_RESIZE_MIN_INTERVAL_MS = 100;

	private static WaterfallRenderQuality _waterfallRenderQuality = WaterfallRenderQuality.High;

	private static float[] _gpuPaletteUpload = new float[4096];

	private static readonly long[][] _waterfallRowUtcTicks = new long[2][]
	{
		new long[0],
		new long[0]
	};

	private static readonly long[][] _waterfallRowLabelUtcTicks = new long[2][]
	{
		new long[0],
		new long[0]
	};

	private static readonly long[][] _waterfallRowLabelIntervalMs = new long[2][]
	{
		new long[0],
		new long[0]
	};

	private static readonly int[] _waterfallRowTimeCounts = new int[2];

	private static readonly int[] _waterfallRowsSinceLastLabel = new int[2];

	private static readonly double[] _waterfallLineIntervalMs = new double[2];

	private static readonly double[] _waterfallLastAdvanceFrameStart = new double[2]
	{
		double.NaN,
		double.NaN
	};

	private const double WATERFALL_TIME_LABEL_TARGET_ROWS = 56.0;

	private const double WATERFALL_TIME_LABEL_MIN_ROWS_FOR_FIVE_SECONDS = 18.0;

	private const double WATERFALL_TIME_LABEL_MAX_ROWS_FOR_FIVE_SECONDS = 96.0;

	private const double WATERFALL_TIME_LABEL_MIN_ROWS_FOR_ONE_SECOND = 26.0;

	private static readonly long[] _waterfallTimeLabelIntervalsMs = new long[16]
	{
		1000L, 5000L, 10000L, 15000L, 30000L, 60000L, 120000L, 300000L, 600000L, 900000L,
		1800000L, 3600000L, 7200000L, 10800000L, 21600000L, 43200000L
	};

	private static bool _testing_imd = false;

	private static bool _show_imd_measurements = false;

	private static bool _tnf_active = true;

	private static bool m_bFrameRateIssue = false;

	private static bool _bGetPixelsIssueRX1 = false;

	private static bool _bGetPixelsIssueRX2 = false;

	private static bool m_bShowFrameRateIssue = true;

	private static bool m_bShowGetPixelsIssue = false;

	private static float m_fRX1WaterfallOpacity = 1f;

	private static float m_fRX2WaterfallOpacity = 1f;

	public static System.Drawing.Rectangle AGCKnee = default(System.Drawing.Rectangle);

	public static System.Drawing.Rectangle AGCHang = default(System.Drawing.Rectangle);

	public static System.Drawing.Rectangle AGCRX2Knee = default(System.Drawing.Rectangle);

	public static System.Drawing.Rectangle AGCRX2Hang = default(System.Drawing.Rectangle);

	private static System.Drawing.Color notch_callout_active_color = System.Drawing.Color.Chartreuse;

	private static System.Drawing.Color notch_callout_inactive_color = System.Drawing.Color.OrangeRed;

	private static System.Drawing.Color notch_highlight_color = System.Drawing.Color.Chartreuse;

	private static System.Drawing.Color notch_tnf_off_colour = System.Drawing.Color.Olive;

	private static System.Drawing.Color notch_active_colour = System.Drawing.Color.Yellow;

	private static System.Drawing.Color notch_inactive_colour = System.Drawing.Color.Gray;

	private static System.Drawing.Color notch_bw_colour = System.Drawing.Color.Yellow;

	private static System.Drawing.Color notch_bw_colour_inactive = System.Drawing.Color.Gray;

	private static System.Drawing.Color channel_background_on = System.Drawing.Color.FromArgb(150, System.Drawing.Color.DodgerBlue);

	private static System.Drawing.Color channel_background_off = System.Drawing.Color.FromArgb(100, System.Drawing.Color.RoyalBlue);

	private static System.Drawing.Color channel_foreground = System.Drawing.Color.Cyan;

	private static Pen m_pTNFInactive = new Pen(notch_tnf_off_colour, 1f);

	private static System.Drawing.Brush m_bTNFInactive = new SolidBrush(changeAlpha(notch_tnf_off_colour, 92));

	private static Pen m_pNotchActive = new Pen(notch_active_colour, 1f);

	private static Pen m_pNotchInactive = new Pen(notch_inactive_colour, 1f);

	private static Pen m_pHighlighted = new Pen(notch_highlight_color, 1f);

	private static System.Drawing.Brush m_bBWFillColour = new SolidBrush(changeAlpha(notch_bw_colour, 92));

	private static System.Drawing.Brush m_bBWFillColourInactive = new SolidBrush(changeAlpha(notch_bw_colour_inactive, 92));

	private static System.Drawing.Brush m_bBWHighlighedFillColour = new SolidBrush(changeAlpha(notch_highlight_color, 92));

	private static System.Drawing.Brush m_bTextCallOutActive = new SolidBrush(notch_callout_active_color);

	private static System.Drawing.Brush m_bTextCallOutInactive = new SolidBrush(notch_callout_inactive_color);

	private static ColorScheme _rx1_color_scheme = ColorScheme.enhanced;

	private static ColorScheme _rx2_color_scheme = ColorScheme.enhanced;

	private static ColorScheme _tx_color_scheme = ColorScheme.enhanced;

	private static bool reverse_waterfall = false;

	private static bool pan_fill = false;

	private static bool m_bSpectralPeakHoldRX1 = false;

	private static bool m_bSpectralPeakHoldRX2 = false;

	private static bool tx_pan_fill = false;

	private static System.Drawing.Color pan_fill_color = System.Drawing.Color.FromArgb(100, 0, 0, 127);

	private static bool _tx_on_vfob = false;

	private static bool display_duplex = false;

	private static readonly object m_objSplitDisplayLock = new object();

	private static bool split_display = false;

	private static DisplayMode current_display_mode_bottom = DisplayMode.PANADAPTER;

	private static int rx1_filter_low;

	private static int rx1_filter_high;

	private static int rx2_filter_low;

	private static int rx2_filter_high;

	private static int tx_filter_low;

	private static int tx_filter_high;

	private static bool sub_rx1_enabled = false;

	private static bool split_enabled = false;

	private static bool show_freq_offset = false;

	private static bool show_zero_line = false;

	private static double _mouseFrequency;

	private static long vfoa_hz;

	private static long vfoa_sub_hz;

	private static long vfob_hz;

	private static long vfob_sub_hz;

	private static int rx_display_bw;

	private static int rit_hz;

	private static int xit_hz;

	private static int freq_diff = 0;

	private static int rx2_freq_diff = 0;

	private static double m_dSpecralPeakHoldDelayRX1 = 100.0;

	private static double m_dSpecralPeakHoldDelayRX2 = 100.0;

	private static bool m_bAutoAGCRX1 = false;

	private static bool m_bAutoAGCRX2 = false;

	private static bool _rx1ClickDisplayCTUN = false;

	private static bool _rx2ClickDisplayCTUN = false;

	private static bool m_bDelayRX1Blobs = false;

	private static bool m_bDelayRX2Blobs = false;

	private static bool m_bDelayRX1SpectrumPeaks = false;

	private static bool m_bDelayRX2SpectrumPeaks = false;

	private static double m_dPeakDelay = 0.0;

	private static bool m_bFastAttackNoiseFloorRX1 = false;

	private static bool m_bFastAttackNoiseFloorRX2 = false;

	private static double m_dCentreFreqRX1 = 0.0;

	private static double m_dCentreFreqRX2 = 0.0;

	private static int m_nHighlightedBandStackEntryIndex = -1;

	private static bool m_bShowBandStackOverlays = false;

	private static BandStackEntry[] m_bandStackOverlays;

	private static int m_nHightlightFilterEdgeRX1 = 0;

	private static int m_nHightlightFilterEdgeRX2 = 0;

	private static int m_nHightlightFilterEdgeTX = 0;

	private static int cw_pitch = 600;

	private static int m_nPhasePointSize = 1;

	private static bool m_bShowFPS = false;

	private static double _fps_profile_start = double.MinValue;

	private static bool _runningFPSProfile = false;

	private static bool m_bShowVisualNotch = false;

	public static bool specready = false;

	private static int displayTargetHeight = 0;

	private static int displayTargetWidth = 0;

	private static bool _hiDpiPhysicalRender = false;

	private static float _renderScale = 1f;

	private static Control displayTarget = null;

	private static double[] _mnfMinSizeRX = new double[2] { 100.0, 100.0 };

	private static double _mnfMinSizeTX = 100.0;

	private static string _cpu;

	private static string _gpu;

	private static string _ram;

	private static string _installed_ram;

	private static AdaptorInfo _display_adaptor = null;

	private static int m_nDecimation = 1;

	private static int rx_display_low = -4000;

	private static int rx_display_high = 4000;

	private static int rx2_display_low = -4000;

	private static int rx2_display_high = 4000;

	private static int tx_display_low = -4000;

	private static int tx_display_high = 4000;

	private static int rx_spectrum_display_low = -4000;

	private static int rx_spectrum_display_high = 4000;

	private static int rx2_spectrum_display_low = -4000;

	private static int rx2_spectrum_display_high = 4000;

	private static int tx_spectrum_display_low = -4000;

	private static int tx_spectrum_display_high = 4000;

	private static float rx1_preamp_offset = 0f;

	private static bool _ignore_attenuator_offset = false;

	private static float alex_preamp_offset = 0f;

	private static float rx2_preamp_offset = 0f;

	private static float tx_attenuator_offset = 0f;

	private static bool tx_display_cal_control = false;

	private static float rx1_display_cal_offset;

	private static float rx2_display_cal_offset;

	private static float rx1_fft_size_offset;

	private static float rx2_fft_size_offset;

	private static float tx_display_cal_offset = 0f;

	private static int display_cursor_x;

	private static int display_cursor_y;

	private static bool _grid_control_major = false;

	private static bool _grid_control_minor = false;

	private static bool _show_frequency_numbers = true;

	private static bool show_agc = false;

	private static bool spectrum_line = false;

	private static bool display_agc_hang_line = false;

	private static bool rx1_hang_spectrum_line = false;

	private static bool display_rx2_gain_line = false;

	private static bool rx2_gain_spectrum_line = false;

	private static bool display_rx2_hang_line = false;

	private static bool rx2_hang_spectrum_line = false;

	private static bool tx_grid_control = false;

	private static ClickTuneMode current_click_tune_mode = ClickTuneMode.Off;

	private static int scope_time = 50;

	private static int sample_rate_rx1 = 384000;

	private static int sample_rate_rx2 = 384000;

	private static int sample_rate_tx = 192000;

	private static bool high_swr = false;

	private static bool _power_folded_back = false;

	private static bool _old_mox = false;

	private static bool _mox = false;

	private static bool m_bShowRX1NoiseFloor = false;

	private static bool m_bShowRX2NoiseFloor = false;

	private static bool blank_bottom_display = false;

	private static DSPMode rx1_dsp_mode = DSPMode.USB;

	private static DSPMode rx2_dsp_mode = DSPMode.USB;

	private static MNotch m_objHightlightedNotch;

	private static DisplayMode current_display_mode = DisplayMode.PANAFALL;

	private static float max_x;

	private static float max_y;

	private static bool _rx2_enabled = false;

	private static bool _bRebuildRXLinearGradBrush = true;

	private static bool _bRebuildTXLinearGradBrush = true;

	private static bool data_ready;

	private static bool data_ready_bottom;

	private static bool waterfall_data_ready_bottom;

	private static bool waterfall_data_ready;

	private static int spectrum_grid_max = -40;

	private static int spectrum_grid_min = -140;

	private static int spectrum_grid_step = 5;

	private static int rx2_spectrum_grid_max = -40;

	private static int rx2_spectrum_grid_min = -140;

	private static int rx2_spectrum_grid_step = 5;

	private static int tx_spectrum_grid_max = 20;

	private static int tx_spectrum_grid_min = -80;

	private static int tx_spectrum_grid_step = 5;

	private static int tx_wf_amp_max = 30;

	private static int tx_wf_amp_min = -70;

	private static System.Drawing.Color band_edge_color = System.Drawing.Color.Red;

	private static Pen band_edge_pen = new Pen(band_edge_color);

	private static System.Drawing.Color tx_band_edge_color = System.Drawing.Color.Red;

	private static Pen tx_band_edge_pen = new Pen(tx_band_edge_color);

	private static System.Drawing.Color sub_rx_zero_line_color = System.Drawing.Color.LightSkyBlue;

	private static Pen sub_rx_zero_line_pen = new Pen(sub_rx_zero_line_color, 2f);

	private static System.Drawing.Color sub_rx_filter_color = System.Drawing.Color.Blue;

	private static SolidBrush sub_rx_filter_brush = new SolidBrush(sub_rx_filter_color);

	private static System.Drawing.Color grid_text_color = System.Drawing.Color.Yellow;

	private static SolidBrush grid_text_brush = new SolidBrush(grid_text_color);

	private static Pen grid_text_pen = new Pen(grid_text_color);

	private static System.Drawing.Color grid_tx_text_color = System.Drawing.Color.FromArgb(255, System.Drawing.Color.Yellow);

	private static SolidBrush grid_tx_text_brush = new SolidBrush(System.Drawing.Color.FromArgb(255, grid_tx_text_color));

	private static System.Drawing.Color grid_zero_color = System.Drawing.Color.Red;

	private static Pen grid_zero_pen = new Pen(grid_zero_color, 2f);

	private static System.Drawing.Color tx_grid_zero_color = System.Drawing.Color.FromArgb(255, System.Drawing.Color.Red);

	private static Pen tx_grid_zero_pen = new Pen(System.Drawing.Color.FromArgb(255, tx_grid_zero_color), 2f);

	private static System.Drawing.Color grid_color = System.Drawing.Color.FromArgb(65, 255, 255, 255);

	private static Pen grid_pen = new Pen(grid_color);

	private static System.Drawing.Color tx_vgrid_color = System.Drawing.Color.FromArgb(65, 255, 255, 255);

	private static Pen tx_vgrid_pen = new Pen(tx_vgrid_color);

	private static System.Drawing.Color hgrid_color = System.Drawing.Color.White;

	private static Pen hgrid_pen = new Pen(hgrid_color);

	private static System.Drawing.Color tx_hgrid_color = System.Drawing.Color.White;

	private static Pen tx_hgrid_pen = new Pen(tx_hgrid_color);

	private static Pen p1 = new Pen(System.Drawing.Color.YellowGreen, 2f);

	private static Pen peak_blob_pen = new Pen(System.Drawing.Color.OrangeRed);

	private static Pen peak_blob_text_pen = new Pen(System.Drawing.Color.YellowGreen);

	private static System.Drawing.Color data_fill_color = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Blue);

	private static System.Drawing.Color data_fill_color_tx = System.Drawing.Color.FromArgb(128, System.Drawing.Color.DarkRed);

	private static System.Drawing.Color dataPeaks_fill_color = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Gray);

	private static Pen data_fill_fpen = new Pen(data_fill_color);

	private static Pen data_fill_fpen_tx = new Pen(data_fill_color_tx);

	private static Pen dataPeaks_fill_fpen = new Pen(dataPeaks_fill_color);

	private static System.Drawing.Color data_line_color = System.Drawing.Color.White;

	private static Pen data_line_pen = new Pen(new SolidBrush(data_line_color), 1f);

	private static System.Drawing.Color tx_data_line_color = System.Drawing.Color.White;

	private static Pen tx_data_line_pen = new Pen(new SolidBrush(tx_data_line_color), 1f);

	private static Pen tx_data_line_fpen = new Pen(System.Drawing.Color.FromArgb(100, tx_data_line_color));

	private static System.Drawing.Color grid_pen_dark = System.Drawing.Color.FromArgb(65, 255, 255, 255);

	private static Pen grid_pen_inb = new Pen(grid_pen_dark);

	private static System.Drawing.Color tx_vgrid_pen_fine = System.Drawing.Color.FromArgb(65, 255, 255, 255);

	private static Pen tx_vgrid_pen_inb = new Pen(tx_vgrid_pen_fine);

	private static System.Drawing.Color bandstack_overlay_color = System.Drawing.Color.FromArgb(192, 192, 64, 0);

	private static SolidBrush bandstack_overlay_brush = new SolidBrush(bandstack_overlay_color);

	private static SolidBrush bandstack_overlay_brush_lines = new SolidBrush(System.Drawing.Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64, 255), bandstack_overlay_color));

	private static SolidBrush bandstack_overlay_brush_highlight = new SolidBrush(System.Drawing.Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64, 255), bandstack_overlay_color));

	private static System.Drawing.Color display_filter_color = System.Drawing.Color.FromArgb(65, 255, 255, 255);

	private static SolidBrush display_filter_brush = new SolidBrush(display_filter_color);

	private static Pen cw_zero_pen = new Pen(System.Drawing.Color.FromArgb(255, display_filter_color), 2f);

	private static System.Drawing.Color tx_filter_color = System.Drawing.Color.FromArgb(65, 255, 255, 255);

	private static SolidBrush tx_filter_brush = new SolidBrush(tx_filter_color);

	private static bool m_bShowNoiseFloorDBM = true;

	private static float m_fNoiseFloorLineWidth = 1f;

	private static System.Drawing.Color noisefloor_color = System.Drawing.Color.Red;

	private static System.Drawing.Color noisefloor_color_text = System.Drawing.Color.Yellow;

	private static float m_fWaterfallAGCOffsetRX1 = 0f;

	private static float m_fWaterfallAGCOffsetRX2 = 0f;

	private static bool m_bWaterfallUseNFForACGRX1 = false;

	private static bool m_bWaterfallUseNFForACGRX2 = false;

	private static System.Drawing.Color display_filter_tx_color = System.Drawing.Color.Yellow;

	private static Pen tx_filter_pen = new Pen(display_filter_tx_color, 2f);

	private static System.Drawing.Color display_background_color = System.Drawing.Color.Black;

	private static SolidBrush display_background_brush = new SolidBrush(display_background_color);

	private static System.Drawing.Color tx_display_background_color = System.Drawing.Color.Black;

	private static SolidBrush tx_display_background_brush = new SolidBrush(tx_display_background_color);

	private static bool m_bShowTXFilterOnWaterfall = false;

	private static bool m_bShowRXFilterOnWaterfall = false;

	private static bool m_bShowTXZeroLineOnWaterfall = false;

	private static bool m_bShowRXZeroLineOnWaterfall = false;

	private static bool m_bShowTXFilterOnRXWaterfall = false;

	private static WaterfallTimePosition m_eShowWaterfallTime = WaterfallTimePosition.LEFT;

	private static WaterfallTimeMode m_eWaterfallTime = WaterfallTimeMode.UTC;

	private static System.Drawing.Color m_cWaterfallTimeColour = System.Drawing.Color.White;

	private static bool draw_tx_filter = false;

	private static bool show_cwzero_line = false;

	private static bool draw_tx_cw_freq = false;

	private static System.Drawing.Color waterfall_low_color = System.Drawing.Color.Black;

	private static System.Drawing.Color waterfall_low_color_tx = System.Drawing.Color.Black;

	private static float waterfall_high_threshold = -80f;

	private static float waterfall_low_threshold = -130f;

	private static System.Drawing.Color rx2_waterfall_low_color = System.Drawing.Color.Black;

	private static float rx2_waterfall_high_threshold = -80f;

	private static float rx2_waterfall_low_threshold = -130f;

	private static float _display_line_width = 1f;

	private static float _tx_display_line_width = 1f;

	private static DisplayLabelAlignment display_label_align = DisplayLabelAlignment.LEFT;

	private static DisplayLabelAlignment tx_display_label_align = DisplayLabelAlignment.CENTER;

	private static int phase_num_pts = 100;

	private static bool click_tune_filter = false;

	private static bool show_cth_line = false;

	private static double f_center = vfoa_hz;

	private static int top_size = 0;

	private static int _lin_corr = 2;

	private static int _linlog_corr = -14;

	private static SolidBrush pana_text_brush = new SolidBrush(System.Drawing.Color.Khaki);

	private static System.Drawing.Font pana_font = new System.Drawing.Font("Tahoma", 7f, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 0);

	private static System.Drawing.Font m_fntCallOutFont = new System.Drawing.Font("Trebuchet MS", 9f, System.Drawing.FontStyle.Regular);

	private static Pen dhp = new Pen(System.Drawing.Color.FromArgb(0, 255, 0));

	private static Pen dhp1 = new Pen(System.Drawing.Color.FromArgb(150, 0, 0, 255));

	private static Pen dhp2 = new Pen(System.Drawing.Color.FromArgb(150, 255, 0, 0));

	private static System.Drawing.Font font1r = new System.Drawing.Font("Microsft Sans Serif", 9f, System.Drawing.FontStyle.Regular);

	private static System.Drawing.Font font10 = new System.Drawing.Font("Arial", 10f);

	private static System.Drawing.Font font12 = new System.Drawing.Font("Arial", 12f);

	private static System.Drawing.Font font14b = new System.Drawing.Font("Arial", 14f, System.Drawing.FontStyle.Bold);

	private static System.Drawing.Font font9 = new System.Drawing.Font("Arial", 9f);

	private static System.Drawing.Font font9b = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Bold);

	private static System.Drawing.Font font95 = new System.Drawing.Font("Arial", 9.5f);

	private static System.Drawing.Font font32b = new System.Drawing.Font("Arial", 32f, System.Drawing.FontStyle.Bold);

	private static ArrayPool<float> m_objFloatPool = ArrayPool<float>.Shared;

	private static ArrayPool<int> m_objIntPool = ArrayPool<int>.Shared;

	private const float WATERFALL_AGC_RESTART_FLOOR_DBM = -150f;

	private static readonly int[] _currentWaterfallBandByRx = new int[2] { -2147483648, -2147483648 };

	private static readonly Dictionary<WaterfallAgcCacheKey, WaterfallAgcCacheEntry> _waterfallAgcCache = new Dictionary<WaterfallAgcCacheKey, WaterfallAgcCacheEntry>();

	private static bool _ignore_waterfall_rx1_agc = false;

	private static bool _ignore_waterfall_rx2_agc = false;

	private static double _rx1_no_agc_duration = 0.0;

	private static double _rx2_no_agc_duration = 0.0;

	private static float[] scope_min;

	private static float[] scope_max;

	private static System.Drawing.Point[] points;

	private static System.Drawing.Point[] pointsStore1;

	private static System.Drawing.Point[] pointsStore2;

	private static int lastResize = -999;

	private static float[] scope2_min = new float[displayTargetWidth];

	private static float[] scope2_max = new float[displayTargetWidth];

	private static bool m_bRX1_spectrum_thresholds = false;

	private static bool m_bRX2_spectrum_thresholds = false;

	private static bool rx1_waterfall_agc = false;

	private static bool rx2_waterfall_agc = false;

	private static int waterfall_update_period = 2;

	private static int rx2_waterfall_update_period = 2;

	private static int _targetFps = 60;

	private static HiPerfTimer _wfRowTimerRX1;

	private static HiPerfTimer _wfRowTimerRX2;

	private static double _wfRowIntervalMsRX1;

	private static double _wfRowIntervalMsRX2;

	private static double _wfLastRowTimeRX1;

	private static double _wfLastRowTimeRX2;

	private static bool m_bStopRX1WaterfallOnTX = false;

	private static bool m_bStopRX2WaterfallOnTX = false;

	private static float _RX1waterfallPreviousMinValue = -150f;

	private static float _RX2waterfallPreviousMinValue = -150f;

	private static bool _bDX2Setup = false;

	private static bool _rebuildingColorDepth = false;

	private static bool _renderingFrame = false;

	private static readonly AutoResetEvent _frameCompleteEvent = new AutoResetEvent(initialState: false);

	private static Surface _surface;

	private static SwapChain _swapChain;

	private static SwapChain1 _swapChain1;

	private static IntPtr _swapChainHwnd = IntPtr.Zero;

	private static RenderTarget _d2dRenderTarget;

	private static SharpDX.Direct2D1.Factory1 _d2dFactory;

	private static SharpDX.Direct2D1.Device _d2dDevice;

	private static Bitmap1 _d2dTargetBitmap;

	private static SharpDX.Direct3D11.Device _device;

	private static SharpDX.DXGI.Factory1 _factory1;

	private static readonly object _objDX2Lock = new object();

	private static Vector2 m_pixelShift = new Vector2(0.5f, 0.5f);

	private static int _nOldHeightRX1 = -1;

	private static int _nOldHeightRX2 = -1;

	private static bool _bNoiseFloorAlreadyCalculatedRX1 = false;

	private static bool _bNoiseFloorAlreadyCalculatedRX2 = false;

	private static PresentFlags _NoVSYNCpresentFlag = PresentFlags.None;

	private static bool _allowTearing = false;

	private static SwapChainFlags _swapChainFlags = SwapChainFlags.None;

	private static int _nBufferCount = 1;

	private static bool m_bHighlightNumberScaleRX1 = false;

	private static bool m_bHighlightNumberScaleRX2 = false;

	private static long _lastSrgbWatchdogMs = 0L;

	private static float m_fPanafallSplitPerc = 0.5f;

	private static bool m_bAntiAlias = false;

	private static int m_nRX1DisplayHeight = 0;

	private static int m_nRX2DisplayHeight = 0;

	private static string m_sDebugText = "";

	private static bool _maintain_background_aspectratio = false;

	private static SharpDX.Direct2D1.Bitmap _pause_bitmap = null;

	private static bool _paused_display = false;

	private static bool _old_paused_display = false;

	private static bool _pa_issue = false;

	private static string _pa_state_details = "";

	private static bool _valid_fps_profile = false;

	private static double _last_valid_check = double.MinValue;

	private static int _dx_fail_retry = 0;

	private static readonly List<int> _fps_profile_data = new List<int>();

	private static int m_nVBlanks = 0;

	private static int m_nFps = 0;

	private static int m_nFrameCount = 0;

	private static HiPerfTimer _high_perf_timer = new HiPerfTimer();

	private static double m_fLastTime = _high_perf_timer.ElapsedMsec;

	private static double m_dElapsedFrameStart = _high_perf_timer.ElapsedMsec;

	private static bool m_bPeakBlobMaximums = true;

	private static bool m_bInsideFilterOnly = false;

	private static int m_nNumberOfMaximums = 3;

	private static Maximums[] m_nRX1Maximums = new Maximums[20];

	private static Maximums[] m_nRX2Maximums = new Maximums[20];

	private static Maximums[] m_rx1_spectrumPeaks;

	private static Maximums[] m_rx2_spectrumPeaks;

	private static bool m_bBlobPeakHold = false;

	private static double m_fBlobPeakHoldMS = 500.0;

	private static bool m_bBlobPeakHoldDrop = false;

	private static Ellipse m_objEllipse = new Ellipse(Vector2.Zero, 5f, 5f);

	private static float m_fNoiseFloorRX1 = -200f;

	private static float m_fNoiseFloorRX2 = -200f;

	private static bool m_bNoiseFloorGoodRX1 = false;

	private static bool m_bNoiseFloorGoodRX2 = false;

	private static float m_fFFTBinAverageRX1 = -200f;

	private static float m_fFFTBinAverageRX2 = -200f;

	private static float m_fLerpAverageRX1 = -200f;

	private static float m_fLerpAverageRX2 = -200f;

	private static float m_fAttackTimeInMSForRX1 = 2000f;

	private static float m_fAttackTimeInMSForRX2 = 2000f;

	private static NoiseFloorPro.DetectionMode _nfMode = NoiseFloorPro.DetectionMode.Average;

	private static float _nfLowPct = 10f;

	private static float _nfHighPct = 99f;

	private static float _wfAgcSmoothing = 0.4f;

	private static bool _autoHighEnabledRX1 = false;

	private static bool _autoHighEnabledRX2 = false;

	private static float _autoHighMarginDb = 6f;

	private static float _autoHighRX1 = -40f;

	private static float _autoHighRX2 = -40f;

	[ThreadStatic]
	private static float[] _nfScratch;

	private static bool _temporalEnabled = false;

	private static float _temporalAlpha = 0f;

	[ThreadStatic]
	private static float[] _temporalPrevRowF;

	[ThreadStatic]
	private static bool _temporalPrevValid;

	private static bool _zoomAdaptiveEnabled = true;

	[ThreadStatic]
	private static int _effectiveToneMap;

	[ThreadStatic]
	private static float _effectiveTemporalAlpha;

	private static bool _autoThresholdEnabled = false;

	private const float AUTO_THRESHOLD_MAX_OFFSET_DB = 10f;

	private static float _autoThresholdFineOffset = -3f;

	private static bool _gpuEffectsEnabled = false;

	private static bool _autoEnableGPU = true;

	private static Effect _wfEffect;

	private static double _fLastFastAttackEnabledTimeRX1 = 0.0;

	private static double _fLastFastAttackEnabledTimeRX2 = 0.0;

	private static float m_dBmPerSecondSpectralPeakFallRX1 = 6f;

	private static float m_dBmPerSecondSpectralPeakFallRX2 = 6f;

	private static float m_dBmPerSecondPeakBlobFall = 6f;

	private static bool m_bActivePeakFillRX1 = false;

	private static bool m_bActivePeakFillRX2 = false;

	private static readonly object _rx1_offset_locker = new object();

	private static readonly object _rx2_offset_locker = new object();

	private static bool _activePeakInTxRX1 = false;

	private static bool _activePeakInTxRX2 = false;

	private static float _ema_dbc = -999f;

	private static int _two_tone_readings_X_offset = 50;

	private static float _ema_f0l;

	private static float _ema_f0u;

	private static float _ema_imd3l;

	private static float _ema_imd3u;

	private static float _ema_imd5l;

	private static float _ema_imd5u;

	private static float _ema_f0l_freq;

	private static float _ema_f0h_freq;

	private static float _ema_imd3l_freq;

	private static float _ema_imd3h_freq;

	private static float _ema_imd5l_freq;

	private static float _ema_imd5h_freq;

	private static float _ema_imd3dBc;

	private static float _ema_imd5dBc;

	private static float _ema_oip3;

	private static float _ema_oip5;

	private static float _fNFshiftDBM = 0f;

	private static int _NFsensitivity = 3;

	private static int _ditherFrameY = 0;

	private static System.Drawing.Color[] _rx1_waterfall_grad = new System.Drawing.Color[WaterfallEnhancer.LUT_SIZE];

	private static System.Drawing.Color[] _rx2_waterfall_grad = new System.Drawing.Color[WaterfallEnhancer.LUT_SIZE];

	private static bool _rx1_waterfall_grad_ok = false;

	private static bool _rx2_waterfall_grad_ok = false;

	private static System.Drawing.Color[] _tx_waterfall_grad = new System.Drawing.Color[WaterfallEnhancer.LUT_SIZE];

	private static WaterfallPalette _paletteConsole;

	private static WaterfallPalette _paletteThermal;

	private static WaterfallPalette _paletteDeepBlue;

	private static WaterfallPalette _paletteEnhanced256;

	private static WaterfallPalette _paletteGrayscale256;

	private static bool _tx_waterfall_grad_ok = false;

	private static bool _old_power = false;

	private static bool _stopRx1Waterfall = false;

	private static bool _stopRx2Waterfall = false;

	private static DateTime _rx1_centrefreq_change_time = DateTime.UtcNow;

	private static DateTime _rx2_centrefreq_change_time = DateTime.UtcNow;

	private static int _detachedFrameCounter = 0;

	private static bool _detachedSkipRender = false;

	private static SharpDX.Direct2D1.Bitmap _bitmapBackground;

	private static SharpDX.Direct2D1.Brush m_bDX2_dataPeaks_fill_fpen_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_data_fill_fpen_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_data_fill_fpen_brush_tx;

	private static SharpDX.Direct2D1.Brush m_bDX2_data_line_pen_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_data_line_pen_brush_tx;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_data_line_fpen_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_data_line_pen_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_sub_rx_filter_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_sub_rx_zero_line_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_filter_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_cw_zero_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_pNotchActive;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWFillColour;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_pNotchInactive;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWFillColourInactive;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_pTNFInactive;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_bTNFInactive;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_grid_zero_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_grid_zero_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_vgrid_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_grid_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_hgrid_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_hgrid_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_grid_text_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_bandstack_overlay_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_bandstack_overlay_brush_lines;

	private static SharpDX.Direct2D1.Brush m_bDX2_bandstack_overlay_brush_highlight;

	private static SharpDX.Direct2D1.Brush m_bDX2_display_filter_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_filter_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_bTextCallOutActive;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_bTextCallOutInactive;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_pHighlighted;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWHighlighedFillColour;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_band_edge_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_tx_vgrid_pen_inb;

	private static SharpDX.Direct2D1.Brush m_bDX2_band_edge_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_grid_pen_inb;

	private static SharpDX.Direct2D1.Brush m_bDX2_Red;

	private static SharpDX.Direct2D1.Brush m_bDX2_Yellow;

	private static SharpDX.Direct2D1.Brush m_bDX2_YellowGreen;

	private static SharpDX.Direct2D1.Brush m_bDX2_Gray;

	private static SharpDX.Direct2D1.Brush m_bDX2_PeakBlob;

	private static SharpDX.Direct2D1.Brush m_bDX2_PeakBlobText;

	private static SharpDX.Direct2D1.Brush m_bDX2_grid_tx_text_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_grid_text_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_pana_text_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_p1;

	private static SharpDX.Direct2D1.Brush m_bDX2_display_background_brush;

	private static Color4 m_cDX2_display_background_colour;

	private static Color4 m_cDX2_display_background_clear_colour;

	private static SharpDX.Direct2D1.Brush m_bDX2_y1_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_y2_brush;

	private static SharpDX.Direct2D1.Brush m_bDX2_waveform_line_pen;

	private static SharpDX.Direct2D1.Brush m_bDX2_dhp;

	private static SharpDX.Direct2D1.Brush m_bDX2_dhp1;

	private static SharpDX.Direct2D1.Brush m_bDX2_dhp2;

	private static StrokeStyle m_styleDots;

	private static SharpDX.Direct2D1.Brush m_bDX2_noisefloor;

	private static SharpDX.Direct2D1.Brush m_bDX2_noisefloor_text;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_bHightlightNumberScale;

	private static SharpDX.Direct2D1.Brush m_bDX2_m_bHightlightNumbers;

	private static LinearGradientBrush m_brushLGDataFillRX1 = null;

	private static LinearGradientBrush m_brushLGDataLineRX1 = null;

	private static LinearGradientBrush m_brushLGDataFillRX2 = null;

	private static LinearGradientBrush m_brushLGDataLineRX2 = null;

	private static LinearGradientBrush m_brushLGDataFillTX_RX1 = null;

	private static LinearGradientBrush m_brushLGDataLineTX_RX1 = null;

	private static LinearGradientBrush m_brushLGDataFillTX_RX2 = null;

	private static LinearGradientBrush m_brushLGDataLineTX_RX2 = null;

	private static bool m_bUseLinearGradient = false;

	private static bool m_bUseLinearGradientForDataLine = false;

	private static bool m_bUseLinearGradientTX = false;

	private static bool m_bUseLinearGradientForDataLineTX = false;

	private static SharpDX.DirectWrite.Factory fontFactory;

	private static TextFormat fontDX2d_callout;

	private static TextFormat fontDX2d_font9;

	private static TextFormat fontDX2d_font9b;

	private static TextFormat fontDX2d_font9c;

	private static TextFormat fontDX2d_panafont;

	private static TextFormat fontDX2d_font10;

	private static TextFormat fontDX2d_font12;

	private static TextFormat fontDX2d_font14;

	private static TextFormat fontDX2d_font32;

	private static TextFormat fontDX2d_font1;

	private static TextFormat fontDX2d_fps_profile;

	private const int MAX_STRING_CACHE_ENTRIES = 500;

	private static readonly Dictionary<(string Text, string FontFamily, float Size), SizeF> m_stringSizeCache = new Dictionary<(string, string, float), SizeF>(501);

	private static readonly Queue<(string Text, string FontFamily, float Size)> _stringMeasureKeys = new Queue<(string, string, float)>(501);

	private static bool m_bAlwaysShowCursorInfo = false;

	private static string m_sMHzCursorDisplay = "";

	private static string m_sOtherData1CursorDisplay = "";

	private static string m_sOtherData2CursorDisplay = "";

	private static bool _joinBandEdges = false;

	private const float Scale10 = 27866352f;

	private const float Scale = 8754473f / (float)Math.PI;

	private const int Bias = 1065353216;

	private static bool m_bShowPhaseAngularMean = false;

	private static double m_dLastAngleLerp = 0.0;

	private static PointF m_dOldCM = new PointF(0f, 0f);

	private static bool _NFDecimal = false;

	private static int _new_spot_fade = 255;

	private static double _pulsePhase;

	private static double _lastRenderTime;

	private static List<int> _spotLayerRightRX1 = new List<int>();

	private static List<int> _spotLayerRightRX2 = new List<int>();

	private static readonly Dictionary<int, SharpDX.Direct2D1.Brush> _DX2Brushes = new Dictionary<int, SharpDX.Direct2D1.Brush>(256);

	private static readonly Dictionary<System.Drawing.Image, SharpDX.Direct2D1.Bitmap> _spotFlagBitmapCache = new Dictionary<System.Drawing.Image, SharpDX.Direct2D1.Bitmap>(ReferenceEqualityComparer<System.Drawing.Image>.Instance);

	private static bool _bShiftKeyDown = false;

	private static bool _spot_highlighted = false;

	private static bool _showTCISpots = false;

	private static bool _flashNewTCISpots = false;

	private static bool _override_spot_flash_colour = false;

	private static System.Drawing.Color _spot_flash_colour = System.Drawing.Color.Yellow;

	private static bool _bUseLegacyBuffers = false;

	private static float _fft_fill_timeRX1 = 0f;

	private static float _fft_fill_timeRX2 = 0f;

	private static bool _wdsp_mox_transition_buffer_clear = false;

	private static bool _snowFall = false;

	private static readonly object _snowLock = new object();

	private static Random _rnd = new Random();

	private static List<SnowFlake> _snow = new List<SnowFlake>();

	private static double _oldSnowFrame = 0.0;

	private static double _oldSantaFrame = 0.0;

	private static double _oldSantaXFrame = 0.0;

	private static SharpDX.Direct2D1.Bitmap[] _santaFrames;

	private static int _santaFrameIndex;

	private static float _santaX;

	private static bool _showSanta;

	private static DateTime _whenToShowSanta;

	private static bool _spot_flags = true;

	private static bool _spot_cache_dirty = true;

	private static int _spot_backpanel_alpha = 255;

	public static bool AllowWaterfallSmear
	{
		get
		{
			return _allowWaterfallSmear;
		}
		set
		{
			_allowWaterfallSmear = value;
		}
	}

	internal static SharpDX.Direct3D11.Device SharedD3DDevice => _device;

	internal static SharpDX.Direct2D1.Device SharedD2DDevice => _d2dDevice;

	internal static SharpDX.DXGI.Factory1 SharedDXGIFactory => _factory1;

	internal static SharpDX.Direct2D1.Factory1 SharedD2DFactory => _d2dFactory;

	internal static GPUWaterfallPipeline SharedGPUFFTRX1 => _gpuFFT1;

	internal static GPUWaterfallPipeline SharedGPUFFTRX2 => _gpuFFT2;

	public static object DX2RenderLock => _objDX2Lock;

	internal static System.Drawing.Image SharedBackgroundImageSource => _backgroundImageSource;

	internal static int SharedDisplayTargetWidth => displayTargetWidth;

	internal static float[] SharedCurrentWaterfallDataRX1 => current_waterfall_data;

	public static bool DetachPanafallEnabled => _detachPanafallEnabled;

	internal static DetachedPanafallRenderer DetachedRenderer => _detachedRenderer;

	internal static bool DetachedResizePending => _detachedResizePending;

	internal static bool HasPendingDetachedTeardown => _pendingDetachRendererForTeardown != null;

	public static bool TestingIMD
	{
		get
		{
			return _testing_imd;
		}
		set
		{
			_testing_imd = value;
		}
	}

	public static bool ShowIMDMeasurments
	{
		get
		{
			return _show_imd_measurements;
		}
		set
		{
			if (value)
			{
				FastAttackNoiseFloorRX1 = true;
			}
			_show_imd_measurements = value;
		}
	}

	public static bool TNFActive
	{
		get
		{
			return _tnf_active;
		}
		set
		{
			_tnf_active = value;
		}
	}

	public static bool FrameRateIssue
	{
		get
		{
			return m_bFrameRateIssue;
		}
		set
		{
			m_bFrameRateIssue = value;
		}
	}

	public static bool GetPixelsIssueRX1
	{
		get
		{
			return _bGetPixelsIssueRX1;
		}
		set
		{
			_bGetPixelsIssueRX1 = value;
		}
	}

	public static bool GetPixelsIssueRX2
	{
		get
		{
			return _bGetPixelsIssueRX2;
		}
		set
		{
			_bGetPixelsIssueRX2 = value;
		}
	}

	public static bool ShowFrameRateIssue
	{
		get
		{
			return m_bShowFrameRateIssue;
		}
		set
		{
			m_bShowFrameRateIssue = value;
		}
	}

	public static bool ShowGetPixelsIssue
	{
		get
		{
			return m_bShowGetPixelsIssue;
		}
		set
		{
			m_bShowGetPixelsIssue = value;
		}
	}

	public static float RX1WaterfallOpacity
	{
		get
		{
			return m_fRX1WaterfallOpacity;
		}
		set
		{
			m_fRX1WaterfallOpacity = value;
		}
	}

	public static float RX2WaterfallOpacity
	{
		get
		{
			return m_fRX2WaterfallOpacity;
		}
		set
		{
			m_fRX2WaterfallOpacity = value;
		}
	}

	public static ColorScheme RX1ColorScheme
	{
		get
		{
			return _rx1_color_scheme;
		}
		set
		{
			if (_rx1_color_scheme != value)
			{
				_rx1_color_scheme = value;
				SharedWaterfallState.BumpPaletteVersion();
			}
		}
	}

	public static ColorScheme RX2ColorScheme
	{
		get
		{
			return _rx2_color_scheme;
		}
		set
		{
			if (_rx2_color_scheme != value)
			{
				_rx2_color_scheme = value;
				SharedWaterfallState.BumpPaletteVersion();
			}
		}
	}

	public static ColorScheme TXColorScheme
	{
		get
		{
			return _tx_color_scheme;
		}
		set
		{
			if (_tx_color_scheme != value)
			{
				_tx_color_scheme = value;
				SharedWaterfallState.BumpPaletteVersion();
			}
		}
	}

	public static bool ReverseWaterfall
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

	public static bool PanFill
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

	public static bool SpectralPeakHoldRX1
	{
		get
		{
			return m_bSpectralPeakHoldRX1;
		}
		set
		{
			m_bSpectralPeakHoldRX1 = value;
			if (m_bSpectralPeakHoldRX1)
			{
				ResetSpectrumPeaks(1);
			}
		}
	}

	public static bool SpectralPeakHoldRX2
	{
		get
		{
			return m_bSpectralPeakHoldRX2;
		}
		set
		{
			m_bSpectralPeakHoldRX2 = value;
			if (m_bSpectralPeakHoldRX2)
			{
				ResetSpectrumPeaks(2);
			}
		}
	}

	public static bool TXPanFill
	{
		get
		{
			return tx_pan_fill;
		}
		set
		{
			tx_pan_fill = value;
		}
	}

	public static System.Drawing.Color PanFillColor
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

	public static bool TXOnVFOB
	{
		get
		{
			return _tx_on_vfob;
		}
		set
		{
			_tx_on_vfob = value;
		}
	}

	public static bool DisplayDuplex
	{
		get
		{
			return display_duplex;
		}
		set
		{
			if (_mox && value != display_duplex)
			{
				ResetBlobMaximums(1, bClear: true);
				ResetBlobMaximums(2, bClear: true);
				ResetSpectrumPeaks(1);
				ResetSpectrumPeaks(2);
			}
			display_duplex = value;
		}
	}

	public static bool SplitDisplay
	{
		get
		{
			return split_display;
		}
		set
		{
			lock (m_objSplitDisplayLock)
			{
				split_display = value;
			}
		}
	}

	public static DisplayMode CurrentDisplayModeBottom
	{
		get
		{
			return current_display_mode_bottom;
		}
		set
		{
			bool num = current_display_mode_bottom != value;
			current_display_mode_bottom = value;
			if (num)
			{
				lock (_objDX2Lock)
				{
					clearBuffers(displayTargetWidth, 2);
				}
				if (value == DisplayMode.PANAFALL || value == DisplayMode.WATERFALL)
				{
					ResetWaterfallBmp2();
				}
			}
		}
	}

	public static int RX1FilterLow
	{
		get
		{
			return rx1_filter_low;
		}
		set
		{
			rx1_filter_low = value;
		}
	}

	public static int RX1FilterHigh
	{
		get
		{
			return rx1_filter_high;
		}
		set
		{
			rx1_filter_high = value;
		}
	}

	public static int RX2FilterLow
	{
		get
		{
			return rx2_filter_low;
		}
		set
		{
			rx2_filter_low = value;
		}
	}

	public static int RX2FilterHigh
	{
		get
		{
			return rx2_filter_high;
		}
		set
		{
			rx2_filter_high = value;
		}
	}

	public static int TXFilterLow
	{
		get
		{
			return tx_filter_low;
		}
		set
		{
			tx_filter_low = value;
		}
	}

	public static int TXFilterHigh
	{
		get
		{
			return tx_filter_high;
		}
		set
		{
			tx_filter_high = value;
		}
	}

	public static bool SubRX1Enabled
	{
		get
		{
			return sub_rx1_enabled;
		}
		set
		{
			sub_rx1_enabled = value;
		}
	}

	public static bool SplitEnabled
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

	public static bool ShowFreqOffset
	{
		get
		{
			return show_freq_offset;
		}
		set
		{
			show_freq_offset = value;
		}
	}

	public static bool ShowZeroLine
	{
		get
		{
			return show_zero_line;
		}
		set
		{
			show_zero_line = value;
		}
	}

	public static double MouseFrequency
	{
		get
		{
			return _mouseFrequency;
		}
		set
		{
			_mouseFrequency = value;
		}
	}

	public static long VFOA
	{
		get
		{
			return vfoa_hz;
		}
		set
		{
			vfoa_hz = value;
		}
	}

	public static long VFOASub
	{
		get
		{
			return vfoa_sub_hz;
		}
		set
		{
			vfoa_sub_hz = value;
		}
	}

	public static long VFOB
	{
		get
		{
			return vfob_hz;
		}
		set
		{
			vfob_hz = value;
		}
	}

	public static long VFOBSub
	{
		get
		{
			return vfob_sub_hz;
		}
		set
		{
			vfob_sub_hz = value;
		}
	}

	public static int RXDisplayBW
	{
		get
		{
			return rx_display_bw;
		}
		set
		{
			rx_display_bw = value;
		}
	}

	public static int RIT
	{
		get
		{
			return rit_hz;
		}
		set
		{
			rit_hz = value;
		}
	}

	public static int XIT
	{
		get
		{
			return xit_hz;
		}
		set
		{
			xit_hz = value;
		}
	}

	public static int FreqDiff
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

	public static int RX2FreqDiff
	{
		get
		{
			return rx2_freq_diff;
		}
		set
		{
			rx2_freq_diff = value;
		}
	}

	public static double SpectralPeakHoldDelayRX1
	{
		get
		{
			return m_dSpecralPeakHoldDelayRX1;
		}
		set
		{
			m_dSpecralPeakHoldDelayRX1 = value;
		}
	}

	public static double SpectralPeakHoldDelayRX2
	{
		get
		{
			return m_dSpecralPeakHoldDelayRX2;
		}
		set
		{
			m_dSpecralPeakHoldDelayRX2 = value;
		}
	}

	public static bool AutoAGCRX1
	{
		set
		{
			m_bAutoAGCRX1 = value;
		}
	}

	public static bool AutoAGCRX2
	{
		set
		{
			m_bAutoAGCRX2 = value;
		}
	}

	public static bool FastAttackNoiseFloorRX1
	{
		get
		{
			return m_bFastAttackNoiseFloorRX1;
		}
		set
		{
			m_bNoiseFloorGoodRX1 = false;
			if (value)
			{
				_fLastFastAttackEnabledTimeRX1 = _high_perf_timer.ElapsedMsec;
			}
			m_bFastAttackNoiseFloorRX1 = value;
		}
	}

	public static bool FastAttackNoiseFloorRX2
	{
		get
		{
			return m_bFastAttackNoiseFloorRX2;
		}
		set
		{
			m_bNoiseFloorGoodRX2 = false;
			if (value)
			{
				_fLastFastAttackEnabledTimeRX2 = _high_perf_timer.ElapsedMsec;
			}
			m_bFastAttackNoiseFloorRX2 = value;
		}
	}

	public static double CentreFreqRX1
	{
		get
		{
			return m_dCentreFreqRX1;
		}
		set
		{
			double num = Math.Round(m_dCentreFreqRX1, 6);
			double num2 = Math.Round(value, 6);
			if (num != num2)
			{
				SpecHPSDRDLL.SetPixelRef(cmaster.inid(0, 0), value);
				_rx1_centrefreq_change_time = DateTime.UtcNow;
				_stopRx1Waterfall = true;
				m_dCentreFreqRX1 = value;
				N1MM.Resize(1);
			}
		}
	}

	public static double CentreFreqRX2
	{
		get
		{
			return m_dCentreFreqRX2;
		}
		set
		{
			double num = Math.Round(m_dCentreFreqRX2, 6);
			double num2 = Math.Round(value, 6);
			if (num != num2)
			{
				SpecHPSDRDLL.SetPixelRef(cmaster.inid(0, 1), value);
				_rx2_centrefreq_change_time = DateTime.UtcNow;
				_stopRx2Waterfall = true;
				m_dCentreFreqRX2 = value;
				N1MM.Resize(2);
			}
		}
	}

	public static int HighlightedBandStackEntryIndex
	{
		get
		{
			return m_nHighlightedBandStackEntryIndex;
		}
		set
		{
			m_nHighlightedBandStackEntryIndex = value;
		}
	}

	public static bool ShowBandStackOverlays
	{
		get
		{
			return m_bShowBandStackOverlays;
		}
		set
		{
			m_bShowBandStackOverlays = value;
		}
	}

	public static BandStackEntry[] BandStackOverlays
	{
		get
		{
			return m_bandStackOverlays;
		}
		set
		{
			m_bandStackOverlays = value;
		}
	}

	public static int HightlightFilterEdgeRX1
	{
		get
		{
			return m_nHightlightFilterEdgeRX1;
		}
		set
		{
			m_nHightlightFilterEdgeRX1 = value;
		}
	}

	public static int HightlightFilterEdgeRX2
	{
		get
		{
			return m_nHightlightFilterEdgeRX2;
		}
		set
		{
			m_nHightlightFilterEdgeRX2 = value;
		}
	}

	public static int HightlightFilterEdgeTX
	{
		get
		{
			return m_nHightlightFilterEdgeTX;
		}
		set
		{
			m_nHightlightFilterEdgeTX = value;
		}
	}

	public static int CWPitch
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

	public static int PhasePointSize
	{
		get
		{
			return m_nPhasePointSize;
		}
		set
		{
			m_nPhasePointSize = value;
		}
	}

	public static bool ShowFPS
	{
		get
		{
			return m_bShowFPS;
		}
		set
		{
			m_bShowFPS = value;
		}
	}

	public static bool RunningFPSProfile
	{
		get
		{
			return _runningFPSProfile;
		}
		set
		{
			if (value)
			{
				_runningFPSProfile = false;
				_fps_profile_data.Clear();
				_fps_profile_start = m_dElapsedFrameStart;
				_runningFPSProfile = value;
			}
			else
			{
				_runningFPSProfile = false;
				_fps_profile_start = double.MinValue;
				_fps_profile_data.Clear();
			}
		}
	}

	public static bool ShowVisualNotch
	{
		get
		{
			return m_bShowVisualNotch;
		}
		set
		{
			m_bShowVisualNotch = value;
		}
	}

	public static bool HiDpiPhysicalRender
	{
		get
		{
			return _hiDpiPhysicalRender;
		}
		set
		{
			_hiDpiPhysicalRender = value;
		}
	}

	public static float RenderScale
	{
		get
		{
			return _renderScale;
		}
		internal set
		{
			_renderScale = ((value < 1f) ? 1f : value);
		}
	}

	public static AdaptorInfo DisplayAdaptor
	{
		get
		{
			return _display_adaptor;
		}
		set
		{
			_display_adaptor = value;
		}
	}

	public static Control Target
	{
		get
		{
			return displayTarget;
		}
		set
		{
			lock (_objDX2Lock)
			{
				_cpu = Common.GetCpuName();
				_ram = Common.GetTotalRam();
				_installed_ram = Common.GetInstalledRam();
				displayTarget = value;
				if (_hiDpiPhysicalRender)
				{
					Size physicalClientSize = HiDpiSurfaces.GetPhysicalClientSize(displayTarget.Handle);
					if (physicalClientSize.Width >= 64 && physicalClientSize.Height >= 64)
					{
						displayTargetHeight = physicalClientSize.Height;
						displayTargetWidth = Math.Min(physicalClientSize.Width, 16384);
						_renderScale = Math.Max(1f, (float)physicalClientSize.Width / (float)Math.Max(1, displayTarget.Width));
					}
					else
					{
						displayTargetHeight = displayTarget.Height;
						displayTargetWidth = Math.Min(displayTarget.Width, 16384);
						_renderScale = 1f;
					}
				}
				else
				{
					displayTargetHeight = displayTarget.Height;
					displayTargetWidth = Math.Min(displayTarget.Width, 16384);
					_renderScale = 1f;
				}
				initDisplayArrays(displayTargetWidth, displayTargetHeight);
				_mnfMinSizeRX[0] = console.GetMinimumRXNotchWidth(1);
				_mnfMinSizeRX[1] = console.GetMinimumRXNotchWidth(2);
				_mnfMinSizeTX = console.GetMinimumTXNotchWidth();
				if (!_bDX2Setup)
				{
					initDX2D(DriverType.Hardware, _display_adaptor);
				}
				else if (_swapChainHwnd != IntPtr.Zero && _swapChainHwnd != displayTarget.Handle)
				{
					if (!recreateSwapChainForHwnd(out var error))
					{
						ShutdownDX2D();
						MessageBox.Show("Unable to resize DirectX render target (Target). DirectX has been shut down.\n\n" + error, "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
						return;
					}
					ResetWaterfallBmp();
					ResetWaterfallBmp2();
				}
				else
				{
					if (!resizeDX2D(out var error2))
					{
						ShutdownDX2D();
						MessageBox.Show("Unable to resize DirectX render target (Target). DirectX has been shut down.\n\n" + error2, "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
						return;
					}
					ResetWaterfallBmp();
					ResetWaterfallBmp2();
				}
				Audio.ScopePixelWidth = displayTargetWidth;
				if (specready)
				{
					console.specRX.GetSpecRX(0).Pixels = displayTargetWidth / m_nDecimation;
					console.specRX.GetSpecRX(1).Pixels = displayTargetWidth / m_nDecimation;
					console.specRX.GetSpecRX(cmaster.inid(1, 0)).Pixels = displayTargetWidth / m_nDecimation;
				}
				N1MM.Resize();
				if (_snowFall)
				{
					lock (_snowLock)
					{
						_snow.Clear();
						_showSanta = false;
						return;
					}
				}
			}
		}
	}

	public static int DisplayWidth => displayTargetWidth;

	public static Size TargetSize
	{
		get
		{
			if (displayTarget == null)
			{
				return Size.Empty;
			}
			return new Size(displayTargetWidth, displayTargetHeight);
		}
	}

	public static int Decimation
	{
		get
		{
			return m_nDecimation;
		}
		set
		{
			int nDecimation = m_nDecimation;
			lock (_objDX2Lock)
			{
				m_nDecimation = value;
			}
			if (nDecimation != m_nDecimation)
			{
				N1MM.Resize();
			}
		}
	}

	public static int RXDisplayLow
	{
		get
		{
			return rx_display_low;
		}
		set
		{
			if (value != rx_display_low)
			{
				ResetBlobMaximums(1, bClear: true);
				ResetSpectrumPeaks(1);
				rx_display_low = value;
				N1MM.Resize(1);
			}
		}
	}

	public static int RXDisplayHigh
	{
		get
		{
			return rx_display_high;
		}
		set
		{
			if (value != rx_display_high)
			{
				ResetBlobMaximums(1, bClear: true);
				ResetSpectrumPeaks(1);
				rx_display_high = value;
				N1MM.Resize(1);
			}
		}
	}

	public static int RX2DisplayLow
	{
		get
		{
			return rx2_display_low;
		}
		set
		{
			if (value != rx2_display_low)
			{
				ResetBlobMaximums(2, bClear: true);
				ResetSpectrumPeaks(2);
				rx2_display_low = value;
				N1MM.Resize(2);
			}
		}
	}

	public static int RX2DisplayHigh
	{
		get
		{
			return rx2_display_high;
		}
		set
		{
			if (value != rx2_display_high)
			{
				ResetBlobMaximums(2, bClear: true);
				ResetSpectrumPeaks(2);
				rx2_display_high = value;
				N1MM.Resize(2);
			}
		}
	}

	public static int TXDisplayLow
	{
		get
		{
			return tx_display_low;
		}
		set
		{
			tx_display_low = value;
		}
	}

	public static int TXDisplayHigh
	{
		get
		{
			return tx_display_high;
		}
		set
		{
			tx_display_high = value;
		}
	}

	public static int RXSpectrumDisplayLow
	{
		get
		{
			return rx_spectrum_display_low;
		}
		set
		{
			rx_spectrum_display_low = value;
		}
	}

	public static int RXSpectrumDisplayHigh
	{
		get
		{
			return rx_spectrum_display_high;
		}
		set
		{
			rx_spectrum_display_high = value;
		}
	}

	public static int RX2SpectrumDisplayLow
	{
		get
		{
			return rx2_spectrum_display_low;
		}
		set
		{
			rx2_spectrum_display_low = value;
		}
	}

	public static int RX2SpectrumDisplayHigh
	{
		get
		{
			return rx2_spectrum_display_high;
		}
		set
		{
			rx2_spectrum_display_high = value;
		}
	}

	public static int TXSpectrumDisplayLow
	{
		get
		{
			return tx_spectrum_display_low;
		}
		set
		{
			tx_spectrum_display_low = value;
		}
	}

	public static int TXSpectrumDisplayHigh
	{
		get
		{
			return tx_spectrum_display_high;
		}
		set
		{
			tx_spectrum_display_high = value;
		}
	}

	public static float RX1PreampOffset
	{
		get
		{
			return rx1_preamp_offset;
		}
		set
		{
			rx1_preamp_offset = value;
		}
	}

	public static bool IgnoreAttenuatorOffset
	{
		get
		{
			return _ignore_attenuator_offset;
		}
		set
		{
			_ignore_attenuator_offset = value;
		}
	}

	public static float AlexPreampOffset
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

	public static float RX2PreampOffset
	{
		get
		{
			return rx2_preamp_offset;
		}
		set
		{
			rx2_preamp_offset = value;
		}
	}

	public static float TXAttenuatorOffset
	{
		get
		{
			return tx_attenuator_offset;
		}
		set
		{
			tx_attenuator_offset = value;
		}
	}

	public static bool TXDisplayCalControl
	{
		get
		{
			return tx_display_cal_control;
		}
		set
		{
			tx_display_cal_control = value;
		}
	}

	public static float RX1DisplayCalOffset
	{
		get
		{
			return rx1_display_cal_offset;
		}
		set
		{
			rx1_display_cal_offset = value;
		}
	}

	public static float RX2DisplayCalOffset
	{
		get
		{
			return rx2_display_cal_offset;
		}
		set
		{
			rx2_display_cal_offset = value;
		}
	}

	public static float RX1FFTSizeOffset
	{
		get
		{
			return rx1_fft_size_offset;
		}
		set
		{
			rx1_fft_size_offset = value;
		}
	}

	public static float RX2FFTSizeOffset
	{
		get
		{
			return rx2_fft_size_offset;
		}
		set
		{
			rx2_fft_size_offset = value;
		}
	}

	public static float TXDisplayCalOffset
	{
		get
		{
			return tx_display_cal_offset;
		}
		set
		{
			tx_display_cal_offset = value;
		}
	}

	public static int DisplayCursorX
	{
		get
		{
			return display_cursor_x;
		}
		set
		{
			display_cursor_x = ((value < 0) ? (-1) : ((int)((float)value * _renderScale)));
		}
	}

	public static int DisplayCursorY
	{
		get
		{
			return display_cursor_y;
		}
		set
		{
			display_cursor_y = ((value < 0) ? (-1) : ((int)((float)value * _renderScale)));
		}
	}

	public static bool GridControlMajor
	{
		get
		{
			return _grid_control_major;
		}
		set
		{
			_grid_control_major = value;
		}
	}

	public static bool GridControlMinor
	{
		get
		{
			return _grid_control_minor;
		}
		set
		{
			_grid_control_minor = value;
		}
	}

	public static bool ShowFrequencyNumbers
	{
		get
		{
			return _show_frequency_numbers;
		}
		set
		{
			_show_frequency_numbers = value;
		}
	}

	public static bool ShowAGC
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

	public static bool SpectrumLine
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

	public static bool DisplayAGCHangLine
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

	public static bool RX1HangSpectrumLine
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

	public static bool DisplayRX2GainLine
	{
		get
		{
			return display_rx2_gain_line;
		}
		set
		{
			display_rx2_gain_line = value;
		}
	}

	public static bool RX2GainSpectrumLine
	{
		get
		{
			return rx2_gain_spectrum_line;
		}
		set
		{
			rx2_gain_spectrum_line = value;
		}
	}

	public static bool DisplayRX2HangLine
	{
		get
		{
			return display_rx2_hang_line;
		}
		set
		{
			display_rx2_hang_line = value;
		}
	}

	public static bool RX2HangSpectrumLine
	{
		get
		{
			return rx2_hang_spectrum_line;
		}
		set
		{
			rx2_hang_spectrum_line = value;
		}
	}

	public static bool TXGridControl
	{
		get
		{
			return tx_grid_control;
		}
		set
		{
			tx_grid_control = value;
		}
	}

	public static ClickTuneMode CurrentClickTuneMode
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

	public static int ScopeTime
	{
		get
		{
			return scope_time;
		}
		set
		{
			scope_time = value;
		}
	}

	public static int SampleRateRX1
	{
		get
		{
			return sample_rate_rx1;
		}
		set
		{
			sample_rate_rx1 = value;
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
		}
	}

	public static bool GPUWaterfallPipelineEnabled
	{
		get
		{
			return _gpuWaterfallPipelineEnabled;
		}
		set
		{
			if (_gpuWaterfallPipelineEnabled != value)
			{
				_gpuWaterfallPipelineEnabled = value;
				try
				{
					cmaster.SetWaterfallIQEnabled(value);
				}
				catch
				{
				}
				ResetWaterfallBmp();
				ResetWaterfallBmp2();
				ResetGPUWaterfallState(1);
				ResetGPUWaterfallState(2);
			}
		}
	}

	public static int GPUWaterfallFFTSize
	{
		get
		{
			return _gpuWaterfallFFTSize;
		}
		set
		{
			int num = value;
			if (num < 1024)
			{
				num = 1024;
			}
			if (num > 262144)
			{
				num = 262144;
			}
			num = PowerOfTwo(num);
			if (_gpuWaterfallFFTSize != num)
			{
				LogGPU($"GPUWaterfallFFTSize changed from {_gpuWaterfallFFTSize} to {num}. Stack trace:\n{new StackTrace()}");
				_gpuWaterfallFFTSize = num;
				if (_gpuWaterfallPipelineEnabled)
				{
					ResetGPUWaterfallState(1);
					ResetGPUWaterfallState(2);
				}
			}
		}
	}

	public static bool GPUWaterfallLinearDraw
	{
		get
		{
			return _gpuWaterfallLinearDraw;
		}
		set
		{
			_gpuWaterfallLinearDraw = value;
		}
	}

	public static WaterfallRenderQuality WaterfallQuality
	{
		get
		{
			return _waterfallRenderQuality;
		}
		set
		{
			if (_waterfallRenderQuality != value)
			{
				_waterfallRenderQuality = value;
				_gpuWaterfallLinearDraw = value != WaterfallRenderQuality.Low;
				LogGPU($"WaterfallQuality changed to {value}, linearDraw={_gpuWaterfallLinearDraw}");
			}
		}
	}

	public static GPUWaterfallWindowType GPUWaterfallWindowType
	{
		get
		{
			return _gpuWaterfallWindowType;
		}
		set
		{
			if (_gpuWaterfallWindowType != value)
			{
				_gpuWaterfallWindowType = value;
				if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
				{
					_gpuFFT1.WindowType = value;
				}
				if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
				{
					_gpuFFT2.WindowType = value;
				}
			}
		}
	}

	public static double GPUWaterfallKaiserBeta
	{
		get
		{
			return _gpuWaterfallKaiserBeta;
		}
		set
		{
			double num = value;
			if (num < 0.0)
			{
				num = 0.0;
			}
			if (num > 20.0)
			{
				num = 20.0;
			}
			if (!(Math.Abs(_gpuWaterfallKaiserBeta - num) < 0.01))
			{
				_gpuWaterfallKaiserBeta = num;
				if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
				{
					_gpuFFT1.KaiserBeta = num;
				}
				if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
				{
					_gpuFFT2.KaiserBeta = num;
				}
			}
		}
	}

	public static GPUWaterfallMagnitudeMode GPUWaterfallMagnitudeMode
	{
		get
		{
			return _gpuWaterfallMagnitudeMode;
		}
		set
		{
			if (_gpuWaterfallMagnitudeMode != value)
			{
				_gpuWaterfallMagnitudeMode = value;
				if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
				{
					_gpuFFT1.MagnitudeMode = value;
				}
				if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
				{
					_gpuFFT2.MagnitudeMode = value;
				}
			}
		}
	}

	public static int GPUWaterfallOverlapPercent
	{
		get
		{
			return _gpuWaterfallOverlapPercent;
		}
		set
		{
			int num = value;
			if (num < 0)
			{
				num = 0;
			}
			if (num > 95)
			{
				num = 95;
			}
			if (_gpuWaterfallOverlapPercent != num)
			{
				_gpuWaterfallOverlapPercent = num;
				ResetGPUWaterfallState(1, resetCalibration: false);
				ResetGPUWaterfallState(2, resetCalibration: false);
			}
		}
	}

	public static bool GPUWaterfallAutoOverlap
	{
		get
		{
			return _gpuWaterfallAutoOverlap;
		}
		set
		{
			if (_gpuWaterfallAutoOverlap != value)
			{
				_gpuWaterfallAutoOverlap = value;
				ResetGPUWaterfallState(1, resetCalibration: false);
				ResetGPUWaterfallState(2, resetCalibration: false);
			}
		}
	}

	public static int GPUWaterfallLanczosWindow
	{
		get
		{
			return _gpuWaterfallLanczosWindow;
		}
		set
		{
			int num = value;
			if (num < 0)
			{
				num = 0;
			}
			if (num > 4)
			{
				num = 4;
			}
			if (num == 1)
			{
				num = 2;
			}
			if (_gpuWaterfallLanczosWindow != num)
			{
				_gpuWaterfallLanczosWindow = num;
				if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
				{
					_gpuFFT1.LanczosWindow = num;
				}
				if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
				{
					_gpuFFT2.LanczosWindow = num;
				}
			}
		}
	}

	public static GPUWaterfallResamplingMode GPUWaterfallResamplingMode
	{
		get
		{
			return _gpuWaterfallResamplingMode;
		}
		set
		{
			if (_gpuWaterfallResamplingMode != value)
			{
				_gpuWaterfallResamplingMode = value;
				if (_gpuFFT1 != null && _gpuFFT1.IsInitialized)
				{
					_gpuFFT1.ResamplingMode = value;
				}
				if (_gpuFFT2 != null && _gpuFFT2.IsInitialized)
				{
					_gpuFFT2.ResamplingMode = value;
				}
			}
		}
	}

	public static bool HighSWR
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

	public static bool PowerFoldedBack
	{
		get
		{
			return _power_folded_back;
		}
		set
		{
			_power_folded_back = value;
		}
	}

	public static bool MOX
	{
		get
		{
			return _mox;
		}
		set
		{
			lock (_objDX2Lock)
			{
				if (value != _old_mox)
				{
					bool isTxContext = value && (!_tx_on_vfob || (_tx_on_vfob && !_rx2_enabled));
					_RX1waterfallPreviousMinValue = getWaterfallCachedPreviousMinOrFloor(1, isTxContext);
					if (_rx2_enabled)
					{
						bool isTxContext2 = value && _tx_on_vfob && _rx2_enabled;
						_RX2waterfallPreviousMinValue = getWaterfallCachedPreviousMinOrFloor(2, isTxContext2);
					}
					PurgeBuffers();
					_old_mox = value;
				}
				_mox = value;
			}
		}
	}

	public static bool ShowRX1NoiseFloor
	{
		get
		{
			return m_bShowRX1NoiseFloor;
		}
		set
		{
			m_bShowRX1NoiseFloor = value;
		}
	}

	public static bool ShowRX2NoiseFloor
	{
		get
		{
			return m_bShowRX2NoiseFloor;
		}
		set
		{
			m_bShowRX2NoiseFloor = value;
		}
	}

	public static bool BlankBottomDisplay
	{
		get
		{
			return blank_bottom_display;
		}
		set
		{
			blank_bottom_display = value;
		}
	}

	public static DSPMode RX1DSPMode
	{
		get
		{
			return rx1_dsp_mode;
		}
		set
		{
			rx1_dsp_mode = value;
		}
	}

	public static DSPMode RX2DSPMode
	{
		get
		{
			return rx2_dsp_mode;
		}
		set
		{
			rx2_dsp_mode = value;
		}
	}

	public static MNotch HighlightNotch
	{
		get
		{
			return m_objHightlightedNotch;
		}
		set
		{
			m_objHightlightedNotch = value;
		}
	}

	public static DisplayMode CurrentDisplayMode
	{
		get
		{
			return current_display_mode;
		}
		set
		{
			bool num = current_display_mode != value;
			current_display_mode = value;
			if (console.PowerOn)
			{
				console._pause_DisplayThread = true;
			}
			if (num)
			{
				lock (_objDX2Lock)
				{
					clearBuffers(displayTargetWidth, 1);
				}
				if (value == DisplayMode.PANAFALL || value == DisplayMode.WATERFALL)
				{
					ResetWaterfallBmp();
				}
			}
			switch (current_display_mode)
			{
			case DisplayMode.PHASE2:
				Audio.phase = true;
				break;
			case DisplayMode.SCOPE:
			case DisplayMode.SCOPE2:
			case DisplayMode.PANASCOPE:
			case DisplayMode.SPECTRASCOPE:
				cmaster.CMSetScopeRun(0, run: true);
				break;
			default:
				Audio.phase = false;
				cmaster.CMSetScopeRun(0, run: false);
				break;
			}
			console._pause_DisplayThread = false;
		}
	}

	public static float MaxX
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

	public static float MaxY
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

	public static bool RX2Enabled
	{
		get
		{
			return _rx2_enabled;
		}
		set
		{
			_rx2_enabled = value;
		}
	}

	public static bool RebuildLinearGradientBrushRX
	{
		get
		{
			return _bRebuildRXLinearGradBrush;
		}
		set
		{
			_bRebuildRXLinearGradBrush = value;
		}
	}

	public static bool RebuildLinearGradientBrushTX
	{
		get
		{
			return _bRebuildTXLinearGradBrush;
		}
		set
		{
			_bRebuildTXLinearGradBrush = value;
		}
	}

	public static bool DataReady
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

	public static bool DataReadyBottom
	{
		get
		{
			return data_ready_bottom;
		}
		set
		{
			data_ready_bottom = value;
		}
	}

	public static bool WaterfallDataReadyBottom
	{
		get
		{
			return waterfall_data_ready_bottom;
		}
		set
		{
			waterfall_data_ready_bottom = value;
		}
	}

	public static bool WaterfallDataReady
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

	public static int SpectrumGridMax
	{
		get
		{
			return spectrum_grid_max;
		}
		set
		{
			if (value != spectrum_grid_max)
			{
				_bRebuildRXLinearGradBrush = true;
			}
			spectrum_grid_max = value;
		}
	}

	public static int SpectrumGridMin
	{
		get
		{
			return spectrum_grid_min;
		}
		set
		{
			if (value != spectrum_grid_min)
			{
				_bRebuildRXLinearGradBrush = true;
			}
			spectrum_grid_min = value;
		}
	}

	public static int SpectrumGridStep
	{
		get
		{
			return spectrum_grid_step;
		}
		set
		{
			if (value != spectrum_grid_step)
			{
				_bRebuildRXLinearGradBrush = true;
			}
			spectrum_grid_step = value;
		}
	}

	public static int SpectrumGridMaxMoxModified
	{
		get
		{
			if (localMox(1))
			{
				return tx_spectrum_grid_max;
			}
			return spectrum_grid_max;
		}
	}

	public static int SpectrumGridMinMoxModified
	{
		get
		{
			if (localMox(1))
			{
				return tx_spectrum_grid_min;
			}
			return spectrum_grid_min;
		}
	}

	public static int SpectrumGridStepMoxModified
	{
		get
		{
			if (localMox(1))
			{
				return tx_spectrum_grid_step;
			}
			return spectrum_grid_step;
		}
	}

	public static int RX2SpectrumGridMaxMoxModified
	{
		get
		{
			if (localMox(2))
			{
				return tx_spectrum_grid_max;
			}
			return rx2_spectrum_grid_max;
		}
	}

	public static int RX2SpectrumGridMinMoxModified
	{
		get
		{
			if (localMox(2))
			{
				return tx_spectrum_grid_min;
			}
			return rx2_spectrum_grid_min;
		}
	}

	public static int RX2SpectrumGridStepMoxModified
	{
		get
		{
			if (localMox(2))
			{
				return tx_spectrum_grid_step;
			}
			return rx2_spectrum_grid_step;
		}
	}

	public static int RX2SpectrumGridMax
	{
		get
		{
			return rx2_spectrum_grid_max;
		}
		set
		{
			if (value != rx2_spectrum_grid_max)
			{
				_bRebuildRXLinearGradBrush = true;
			}
			rx2_spectrum_grid_max = value;
		}
	}

	public static int RX2SpectrumGridMin
	{
		get
		{
			return rx2_spectrum_grid_min;
		}
		set
		{
			if (value != rx2_spectrum_grid_min)
			{
				_bRebuildRXLinearGradBrush = true;
			}
			rx2_spectrum_grid_min = value;
		}
	}

	public static int RX2SpectrumGridStep
	{
		get
		{
			return rx2_spectrum_grid_step;
		}
		set
		{
			if (value != rx2_spectrum_grid_step)
			{
				_bRebuildRXLinearGradBrush = true;
			}
			rx2_spectrum_grid_step = value;
		}
	}

	public static int TXSpectrumGridMax
	{
		get
		{
			return tx_spectrum_grid_max;
		}
		set
		{
			tx_spectrum_grid_max = value;
		}
	}

	public static int TXSpectrumGridMin
	{
		get
		{
			return tx_spectrum_grid_min;
		}
		set
		{
			tx_spectrum_grid_min = value;
		}
	}

	public static int TXSpectrumGridStep
	{
		get
		{
			return tx_spectrum_grid_step;
		}
		set
		{
			tx_spectrum_grid_step = value;
		}
	}

	public static int TXWFAmpMax
	{
		get
		{
			return tx_wf_amp_max;
		}
		set
		{
			tx_wf_amp_max = value;
			if (console != null)
			{
				console.CheckForMinMaxWaterfallUpdatesTX();
			}
		}
	}

	public static int TXWFAmpMin
	{
		get
		{
			return tx_wf_amp_min;
		}
		set
		{
			tx_wf_amp_min = value;
			if (console != null)
			{
				console.CheckForMinMaxWaterfallUpdatesTX();
			}
		}
	}

	public static System.Drawing.Color BandEdgeColor
	{
		get
		{
			return band_edge_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				band_edge_color = value;
				band_edge_pen.Color = band_edge_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color TXBandEdgeColor
	{
		get
		{
			return tx_band_edge_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				tx_band_edge_color = value;
				tx_band_edge_pen.Color = tx_band_edge_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color SubRXZeroLine
	{
		get
		{
			return sub_rx_zero_line_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				sub_rx_zero_line_color = value;
				sub_rx_zero_line_pen.Color = sub_rx_zero_line_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color SubRXFilterColor
	{
		get
		{
			return sub_rx_filter_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				sub_rx_filter_color = value;
				sub_rx_filter_brush.Color = sub_rx_filter_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color GridTextColor
	{
		get
		{
			return grid_text_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				grid_text_color = value;
				grid_text_brush.Color = grid_text_color;
				grid_text_pen.Color = grid_text_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color GridTXTextColor
	{
		get
		{
			return grid_tx_text_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				grid_tx_text_color = value;
				grid_tx_text_brush.Color = grid_tx_text_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color GridZeroColor
	{
		get
		{
			return grid_zero_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				grid_zero_color = value;
				grid_zero_pen.Color = grid_zero_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color TXGridZeroColor
	{
		get
		{
			return tx_grid_zero_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				tx_grid_zero_color = value;
				tx_grid_zero_pen.Color = tx_grid_zero_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color GridColor
	{
		get
		{
			return grid_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				grid_color = value;
				grid_pen.Color = grid_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color TXVGridColor
	{
		get
		{
			return tx_vgrid_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				tx_vgrid_color = value;
				tx_vgrid_pen.Color = tx_vgrid_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color HGridColor
	{
		get
		{
			return hgrid_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				hgrid_color = value;
				hgrid_pen.Color = hgrid_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color TXHGridColor
	{
		get
		{
			return tx_hgrid_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				tx_hgrid_color = value;
				tx_hgrid_pen.Color = tx_hgrid_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color DataFillColor
	{
		get
		{
			return data_fill_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				data_fill_color = value;
				data_fill_fpen.Color = data_fill_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color DataFillColorTX
	{
		get
		{
			return data_fill_color_tx;
		}
		set
		{
			lock (_objDX2Lock)
			{
				data_fill_color_tx = value;
				data_fill_fpen_tx.Color = data_fill_color_tx;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color DataPeaksFillColor
	{
		get
		{
			return dataPeaks_fill_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				dataPeaks_fill_color = value;
				dataPeaks_fill_fpen.Color = dataPeaks_fill_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color DataLineColor
	{
		get
		{
			return data_line_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				data_line_color = value;
				data_line_pen.Color = data_line_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color TXDataLineColor
	{
		get
		{
			return tx_data_line_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				tx_data_line_color = value;
				tx_data_line_pen.Color = tx_data_line_color;
				tx_data_line_fpen.Color = System.Drawing.Color.FromArgb(100, tx_data_line_color);
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color GridPenDark
	{
		get
		{
			return grid_pen_dark;
		}
		set
		{
			lock (_objDX2Lock)
			{
				grid_pen_dark = value;
				grid_pen_inb.Color = grid_pen_dark;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color TXVGridPenFine
	{
		get
		{
			return tx_vgrid_pen_fine;
		}
		set
		{
			lock (_objDX2Lock)
			{
				tx_vgrid_pen_fine = value;
				tx_vgrid_pen_inb.Color = tx_vgrid_pen_fine;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color BandstackOverlayColor
	{
		get
		{
			return bandstack_overlay_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				bandstack_overlay_color = value;
				bandstack_overlay_brush.Color = bandstack_overlay_color;
				bandstack_overlay_brush_lines.Color = System.Drawing.Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64, 255), bandstack_overlay_color);
				bandstack_overlay_brush_highlight.Color = System.Drawing.Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64, 255), bandstack_overlay_color);
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color DisplayFilterColor
	{
		get
		{
			return display_filter_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				display_filter_color = value;
				display_filter_brush.Color = display_filter_color;
				cw_zero_pen.Color = System.Drawing.Color.FromArgb(255, display_filter_color);
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color TXFilterColor
	{
		get
		{
			return tx_filter_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				tx_filter_color = value;
				tx_filter_brush.Color = tx_filter_color;
				buildDX2Resources();
			}
		}
	}

	public static bool ShowNoiseFloorDBM
	{
		get
		{
			return m_bShowNoiseFloorDBM;
		}
		set
		{
			m_bShowNoiseFloorDBM = value;
		}
	}

	public static float NoiseFloorLineWidth
	{
		get
		{
			return m_fNoiseFloorLineWidth;
		}
		set
		{
			m_fNoiseFloorLineWidth = value;
		}
	}

	public static System.Drawing.Color NoiseFloorColor
	{
		get
		{
			return noisefloor_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				noisefloor_color = value;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color NoiseFloorColorText
	{
		get
		{
			return noisefloor_color_text;
		}
		set
		{
			lock (_objDX2Lock)
			{
				noisefloor_color_text = value;
				buildDX2Resources();
			}
		}
	}

	public static float WaterfallAGCOffsetRX1
	{
		get
		{
			return m_fWaterfallAGCOffsetRX1;
		}
		set
		{
			m_fWaterfallAGCOffsetRX1 = value;
		}
	}

	public static float WaterfallAGCOffsetRX2
	{
		get
		{
			return m_fWaterfallAGCOffsetRX2;
		}
		set
		{
			m_fWaterfallAGCOffsetRX2 = value;
		}
	}

	public static bool WaterfallUseNFForACGRX1
	{
		get
		{
			return m_bWaterfallUseNFForACGRX1;
		}
		set
		{
			m_bWaterfallUseNFForACGRX1 = value;
		}
	}

	public static bool WaterfallUseNFForACGRX2
	{
		get
		{
			return m_bWaterfallUseNFForACGRX2;
		}
		set
		{
			m_bWaterfallUseNFForACGRX2 = value;
		}
	}

	public static System.Drawing.Color DisplayFilterTXColor
	{
		get
		{
			return display_filter_tx_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				display_filter_tx_color = value;
				tx_filter_pen.Color = display_filter_tx_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color DisplayBackgroundColor
	{
		get
		{
			return display_background_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				display_background_color = value;
				display_background_brush.Color = display_background_color;
				buildDX2Resources();
			}
		}
	}

	public static System.Drawing.Color TXDisplayBackgroundColor
	{
		get
		{
			return tx_display_background_color;
		}
		set
		{
			lock (_objDX2Lock)
			{
				tx_display_background_color = value;
				tx_display_background_brush.Color = tx_display_background_color;
				buildDX2Resources();
			}
		}
	}

	public static bool ShowTXFilterOnWaterfall
	{
		get
		{
			return m_bShowTXFilterOnWaterfall;
		}
		set
		{
			m_bShowTXFilterOnWaterfall = value;
		}
	}

	public static bool ShowRXFilterOnWaterfall
	{
		get
		{
			return m_bShowRXFilterOnWaterfall;
		}
		set
		{
			m_bShowRXFilterOnWaterfall = value;
		}
	}

	public static bool ShowTXZeroLineOnWaterfall
	{
		get
		{
			return m_bShowTXZeroLineOnWaterfall;
		}
		set
		{
			m_bShowTXZeroLineOnWaterfall = value;
		}
	}

	public static bool ShowRXZeroLineOnWaterfall
	{
		get
		{
			return m_bShowRXZeroLineOnWaterfall;
		}
		set
		{
			m_bShowRXZeroLineOnWaterfall = value;
		}
	}

	public static bool ShowTXFilterOnRXWaterfall
	{
		get
		{
			return m_bShowTXFilterOnRXWaterfall;
		}
		set
		{
			m_bShowTXFilterOnRXWaterfall = value;
		}
	}

	public static WaterfallTimePosition ShowWaterfallTime
	{
		get
		{
			return m_eShowWaterfallTime;
		}
		set
		{
			m_eShowWaterfallTime = value;
		}
	}

	public static WaterfallTimeMode WaterfallTime
	{
		get
		{
			return m_eWaterfallTime;
		}
		set
		{
			m_eWaterfallTime = value;
		}
	}

	public static System.Drawing.Color WaterfallTimeColour
	{
		get
		{
			return m_cWaterfallTimeColour;
		}
		set
		{
			m_cWaterfallTimeColour = value;
		}
	}

	public static bool DrawTXFilter
	{
		get
		{
			return draw_tx_filter;
		}
		set
		{
			draw_tx_filter = value;
		}
	}

	public static bool ShowCWZeroLine
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

	public static bool DrawTXCWFreq
	{
		get
		{
			return draw_tx_cw_freq;
		}
		set
		{
			draw_tx_cw_freq = value;
		}
	}

	public static System.Drawing.Color WaterfallLowColor
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

	public static System.Drawing.Color WaterfallLowColorTX
	{
		get
		{
			return waterfall_low_color_tx;
		}
		set
		{
			waterfall_low_color_tx = value;
		}
	}

	public static float WaterfallHighThreshold
	{
		get
		{
			return waterfall_high_threshold;
		}
		set
		{
			waterfall_high_threshold = value;
			if (console != null)
			{
				console.CheckForMinMaxWaterfallUpdatesRX(1);
			}
		}
	}

	public static float WaterfallLowThreshold
	{
		get
		{
			return waterfall_low_threshold;
		}
		set
		{
			waterfall_low_threshold = value;
			if (console != null)
			{
				console.CheckForMinMaxWaterfallUpdatesRX(1);
			}
		}
	}

	public static System.Drawing.Color RX2WaterfallLowColor
	{
		get
		{
			return rx2_waterfall_low_color;
		}
		set
		{
			rx2_waterfall_low_color = value;
		}
	}

	public static float RX2WaterfallHighThreshold
	{
		get
		{
			return rx2_waterfall_high_threshold;
		}
		set
		{
			rx2_waterfall_high_threshold = value;
			if (console != null)
			{
				console.CheckForMinMaxWaterfallUpdatesRX(2);
			}
		}
	}

	public static float RX2WaterfallLowThreshold
	{
		get
		{
			return rx2_waterfall_low_threshold;
		}
		set
		{
			rx2_waterfall_low_threshold = value;
			if (console != null)
			{
				console.CheckForMinMaxWaterfallUpdatesRX(2);
			}
		}
	}

	public static float DisplayLineWidth
	{
		get
		{
			return _display_line_width;
		}
		set
		{
			lock (_objDX2Lock)
			{
				_display_line_width = value;
				data_line_pen.Width = _display_line_width;
			}
		}
	}

	public static float TXDisplayLineWidth
	{
		get
		{
			return _tx_display_line_width;
		}
		set
		{
			lock (_objDX2Lock)
			{
				_tx_display_line_width = value;
				tx_data_line_pen.Width = _tx_display_line_width;
			}
		}
	}

	public static DisplayLabelAlignment DisplayLabelAlign
	{
		get
		{
			return display_label_align;
		}
		set
		{
			display_label_align = value;
		}
	}

	public static DisplayLabelAlignment TXDisplayLabelAlign
	{
		get
		{
			return tx_display_label_align;
		}
		set
		{
			tx_display_label_align = value;
		}
	}

	public static int PhaseNumPts
	{
		get
		{
			return phase_num_pts;
		}
		set
		{
			lock (_objDX2Lock)
			{
				phase_num_pts = value;
			}
		}
	}

	public static bool ClickTuneFilter
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

	public static bool ShowCTHLine
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

	public static double F_Center
	{
		get
		{
			return f_center;
		}
		set
		{
			f_center = value;
		}
	}

	public static int TopSize
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

	public static int LinCor
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

	public static int LinLogCor
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

	public static float[] Scope2Min
	{
		get
		{
			return scope2_min;
		}
		set
		{
			scope2_min = value;
		}
	}

	public static float[] Scope2Max
	{
		get
		{
			return scope2_max;
		}
		set
		{
			scope2_max = value;
		}
	}

	public static bool SpectrumBasedThresholdsRX1
	{
		get
		{
			return m_bRX1_spectrum_thresholds;
		}
		set
		{
			m_bRX1_spectrum_thresholds = value;
		}
	}

	public static bool SpectrumBasedThresholdsRX2
	{
		get
		{
			return m_bRX2_spectrum_thresholds;
		}
		set
		{
			m_bRX2_spectrum_thresholds = value;
		}
	}

	public static bool RX1WaterfallAGC
	{
		get
		{
			return rx1_waterfall_agc;
		}
		set
		{
			rx1_waterfall_agc = value;
		}
	}

	public static bool RX2WaterfallAGC
	{
		get
		{
			return rx2_waterfall_agc;
		}
		set
		{
			rx2_waterfall_agc = value;
		}
	}

	public static int WaterfallUpdatePeriod
	{
		get
		{
			return waterfall_update_period;
		}
		set
		{
			int num = value;
			if (num < 1)
			{
				num = 1;
			}
			if (num > 9999)
			{
				num = 9999;
			}
			waterfall_update_period = num;
			RecalculateWaterfallRowIntervals();
		}
	}

	public static int RX2WaterfallUpdatePeriod
	{
		get
		{
			return rx2_waterfall_update_period;
		}
		set
		{
			int num = value;
			if (num < 1)
			{
				num = 1;
			}
			if (num > 9999)
			{
				num = 9999;
			}
			rx2_waterfall_update_period = num;
			RecalculateWaterfallRowIntervals();
		}
	}

	public static int TargetFPS
	{
		get
		{
			return _targetFps;
		}
		set
		{
			_targetFps = Math.Max(1, value);
			RecalculateWaterfallRowIntervals();
		}
	}

	public static bool StopRX1WaterfallOnTx
	{
		get
		{
			return m_bStopRX1WaterfallOnTX;
		}
		set
		{
			m_bStopRX1WaterfallOnTX = value;
		}
	}

	public static bool StopRX2WaterfallOnTx
	{
		get
		{
			return m_bStopRX2WaterfallOnTX;
		}
		set
		{
			m_bStopRX2WaterfallOnTX = value;
		}
	}

	public static bool HighlightNumberScaleRX1
	{
		get
		{
			return m_bHighlightNumberScaleRX1;
		}
		set
		{
			m_bHighlightNumberScaleRX1 = value;
		}
	}

	public static bool HighlightNumberScaleRX2
	{
		get
		{
			return m_bHighlightNumberScaleRX2;
		}
		set
		{
			m_bHighlightNumberScaleRX2 = value;
		}
	}

	public static bool IsDX2DSetup => _bDX2Setup;

	public static int PanafallSplitBarPos => (int)((float)displayTargetHeight * m_fPanafallSplitPerc);

	public static int PanafallSplitBarPosLogical => (int)((float)((displayTarget != null) ? displayTarget.Height : 0) * m_fPanafallSplitPerc);

	public static float PanafallSplitBarPerc
	{
		get
		{
			return m_fPanafallSplitPerc;
		}
		set
		{
			bool flag = false;
			lock (_objDX2Lock)
			{
				flag = value != m_fPanafallSplitPerc;
				m_fPanafallSplitPerc = value;
			}
			if (flag)
			{
				ResetWaterfallBmp();
			}
		}
	}

	public static bool AntiAlias
	{
		get
		{
			return m_bAntiAlias;
		}
		set
		{
			m_bAntiAlias = value;
			setupAliasing();
		}
	}

	public static int RX1DisplayHeight
	{
		get
		{
			return m_nRX1DisplayHeight;
		}
		set
		{
		}
	}

	public static int RX2DisplayHeight
	{
		get
		{
			return m_nRX2DisplayHeight;
		}
		set
		{
		}
	}

	public static string DebugText
	{
		get
		{
			return m_sDebugText;
		}
		set
		{
			m_sDebugText = value;
		}
	}

	public static bool MaintainBackgroundAspectRatio
	{
		get
		{
			return _maintain_background_aspectratio;
		}
		set
		{
			_maintain_background_aspectratio = value;
		}
	}

	public static bool PausedDisplay
	{
		get
		{
			return _paused_display;
		}
		set
		{
			lock (_objDX2Lock)
			{
				_old_paused_display = _paused_display;
				_paused_display = value;
				console.SetupInfoBarButton(ucInfoBar.ActionTypes.DisplayPause, _paused_display);
				pauseDisplay();
				if (_old_paused_display && !_paused_display)
				{
					FastAttackNoiseFloorRX1 = true;
					FastAttackNoiseFloorRX2 = true;
				}
			}
			if (console != null)
			{
				console.SetGeneralSetting(0, OtherButtonId.PAUSE, _paused_display);
			}
		}
	}

	public static PAstatusIndicatorState PAStatus
	{
		set
		{
			_pa_state_details = Console.GetPAStatusText(value);
			_pa_issue = value != PAstatusIndicatorState.NotUsed && value != PAstatusIndicatorState.OK;
		}
	}

	public static int VerticalBlanks
	{
		get
		{
			return m_nVBlanks;
		}
		set
		{
			int num = value;
			if (num < 0)
			{
				num = 0;
			}
			if (num > 4)
			{
				num = 4;
			}
			m_nVBlanks = num;
		}
	}

	public static int CurrentFPS
	{
		get
		{
			return m_nFps;
		}
		set
		{
			m_nFps = value;
		}
	}

	public static bool ShowPeakBlobs
	{
		get
		{
			return m_bPeakBlobMaximums;
		}
		set
		{
			m_bPeakBlobMaximums = value;
		}
	}

	public static bool ShowPeakBlobsInsideFilterOnly
	{
		get
		{
			return m_bInsideFilterOnly;
		}
		set
		{
			m_bInsideFilterOnly = value;
		}
	}

	public static int NumberOfPeakBlobs
	{
		get
		{
			return m_nNumberOfMaximums;
		}
		set
		{
			int num = value;
			if (num < 1)
			{
				num = 1;
			}
			if (num > m_nRX1Maximums.Length)
			{
				num = m_nRX1Maximums.Length;
			}
			m_nNumberOfMaximums = num;
		}
	}

	public static bool BlobPeakHold
	{
		get
		{
			return m_bBlobPeakHold;
		}
		set
		{
			m_bBlobPeakHold = value;
		}
	}

	public static double BlobPeakHoldMS
	{
		get
		{
			return m_fBlobPeakHoldMS;
		}
		set
		{
			m_fBlobPeakHoldMS = value;
		}
	}

	public static bool BlobPeakHoldDrop
	{
		get
		{
			return m_bBlobPeakHoldDrop;
		}
		set
		{
			m_bBlobPeakHoldDrop = value;
		}
	}

	public static bool ZoomAdaptiveEnabled
	{
		get
		{
			return _zoomAdaptiveEnabled;
		}
		set
		{
			_zoomAdaptiveEnabled = value;
		}
	}

	public static NoiseFloorPro.DetectionMode NFMode
	{
		get
		{
			return _nfMode;
		}
		set
		{
			_nfMode = value;
		}
	}

	public static float NFLowPct
	{
		get
		{
			return _nfLowPct;
		}
		set
		{
			_nfLowPct = ((value < 1f) ? 1f : ((value > 49f) ? 49f : value));
		}
	}

	public static float NFHighPct
	{
		get
		{
			return _nfHighPct;
		}
		set
		{
			_nfHighPct = ((value < 50f) ? 50f : ((value > 99.9f) ? 99.9f : value));
		}
	}

	public static float WaterfallAgcSmoothing
	{
		get
		{
			return _wfAgcSmoothing;
		}
		set
		{
			_wfAgcSmoothing = ((value < 0.05f) ? 0.05f : ((value > 0.9f) ? 0.9f : value));
		}
	}

	public static bool AutoHighEnabledRX1
	{
		get
		{
			return _autoHighEnabledRX1;
		}
		set
		{
			_autoHighEnabledRX1 = value;
		}
	}

	public static bool AutoHighEnabledRX2
	{
		get
		{
			return _autoHighEnabledRX2;
		}
		set
		{
			_autoHighEnabledRX2 = value;
		}
	}

	public static float AutoHighMarginDb
	{
		get
		{
			return _autoHighMarginDb;
		}
		set
		{
			_autoHighMarginDb = ((value < 0f) ? 0f : ((value > 30f) ? 30f : value));
		}
	}

	public static bool TemporalEnabled
	{
		get
		{
			return _temporalEnabled;
		}
		set
		{
			_temporalEnabled = value;
			if (!value)
			{
				_temporalPrevValid = false;
			}
		}
	}

	public static float TemporalStrength
	{
		get
		{
			return _temporalAlpha;
		}
		set
		{
			_temporalAlpha = ((value < 0f) ? 0f : ((value > 0.5f) ? 0.5f : value));
		}
	}

	public static bool AutoThresholdEnabled
	{
		get
		{
			return _autoThresholdEnabled;
		}
		set
		{
			_autoThresholdEnabled = value;
		}
	}

	public static float AutoThresholdFineOffset
	{
		get
		{
			return _autoThresholdFineOffset;
		}
		set
		{
			_autoThresholdFineOffset = ((value < -20f) ? (-20f) : ((value > 20f) ? 20f : value));
		}
	}

	public static bool AutoEnableGPU
	{
		get
		{
			return _autoEnableGPU;
		}
		set
		{
			_autoEnableGPU = value;
		}
	}

	public static bool GPUEffectsEnabled
	{
		get
		{
			return _gpuEffectsEnabled;
		}
		set
		{
			_gpuEffectsEnabled = value;
			if (!value)
			{
				Utilities.Dispose(ref _wfEffect);
			}
		}
	}

	public static bool GPUEffectsAvailable => WaterfallEffect.IsAvailable;

	public static string GPUName => _gpu ?? "unknown";

	public static int GPUDetectionLevel => (int)GPUDetector.Level;

	public static float AttackTimeInMSForRX1
	{
		get
		{
			return m_fAttackTimeInMSForRX1;
		}
		set
		{
			m_fAttackTimeInMSForRX1 = value;
		}
	}

	public static float AttackTimeInMSForRX2
	{
		get
		{
			return m_fAttackTimeInMSForRX2;
		}
		set
		{
			m_fAttackTimeInMSForRX2 = value;
		}
	}

	public static bool IsNoiseFloorGoodRX1
	{
		get
		{
			return m_bNoiseFloorGoodRX1;
		}
		set
		{
		}
	}

	public static bool IsNoiseFloorGoodRX2
	{
		get
		{
			return m_bNoiseFloorGoodRX2;
		}
		set
		{
		}
	}

	public static float NoiseFloorRX1
	{
		get
		{
			m_bNoiseFloorGoodRX1 = false;
			return m_fNoiseFloorRX1;
		}
	}

	public static float NoiseFloorRX2
	{
		get
		{
			m_bNoiseFloorGoodRX2 = false;
			return m_fNoiseFloorRX2;
		}
	}

	public static float ActiveNoiseFloorRX1 => m_fLerpAverageRX1 + _fNFshiftDBM;

	public static float ActiveNoiseFloorRX2 => m_fLerpAverageRX2 + _fNFshiftDBM;

	public static float SpectralPeakFallRX1
	{
		get
		{
			return m_dBmPerSecondSpectralPeakFallRX1;
		}
		set
		{
			m_dBmPerSecondSpectralPeakFallRX1 = value;
		}
	}

	public static float SpectralPeakFallRX2
	{
		get
		{
			return m_dBmPerSecondSpectralPeakFallRX2;
		}
		set
		{
			m_dBmPerSecondSpectralPeakFallRX2 = value;
		}
	}

	public static float PeakBlobFall
	{
		get
		{
			return m_dBmPerSecondPeakBlobFall;
		}
		set
		{
			m_dBmPerSecondPeakBlobFall = value;
		}
	}

	public static bool ActivePeakFillRX1
	{
		get
		{
			return m_bActivePeakFillRX1;
		}
		set
		{
			m_bActivePeakFillRX1 = value;
		}
	}

	public static bool ActivePeakFillRX2
	{
		get
		{
			return m_bActivePeakFillRX2;
		}
		set
		{
			m_bActivePeakFillRX2 = value;
		}
	}

	public static float RX1Offset
	{
		get
		{
			lock (_rx1_offset_locker)
			{
				bool num = localMox(1);
				bool flag = isRxDuplex(1);
				float num2;
				if (!num)
				{
					num2 = ((!_mox || !_tx_on_vfob || flag) ? rx1_display_cal_offset : ((!console.RX2Enabled) ? tx_display_cal_offset : rx1_display_cal_offset));
				}
				else
				{
					num2 = tx_display_cal_offset;
					if (flag)
					{
						num2 += rx1_display_cal_offset;
						num2 += tx_attenuator_offset;
					}
				}
				if (!num)
				{
					num2 += rx1_preamp_offset;
				}
				return num2;
			}
		}
	}

	public static float RX1OffsetWithDup
	{
		get
		{
			lock (_rx1_offset_locker)
			{
				bool num = localMox(1);
				bool flag = true;
				float num2;
				if (!num)
				{
					num2 = ((!_mox || !_tx_on_vfob || flag) ? rx1_display_cal_offset : ((!console.RX2Enabled) ? tx_display_cal_offset : rx1_display_cal_offset));
				}
				else
				{
					num2 = tx_display_cal_offset;
					if (flag)
					{
						num2 += rx1_display_cal_offset;
						num2 += tx_attenuator_offset;
					}
				}
				if (!num)
				{
					num2 += rx1_preamp_offset;
				}
				return num2;
			}
		}
	}

	public static float RX2Offset
	{
		get
		{
			lock (_rx2_offset_locker)
			{
				bool num = localMox(2);
				bool flag = isRxDuplex(2);
				float num2;
				if (num)
				{
					num2 = tx_display_cal_offset;
					if (flag)
					{
						num2 += rx2_display_cal_offset;
						num2 += tx_attenuator_offset;
					}
				}
				else
				{
					num2 = rx2_display_cal_offset;
				}
				if (!num)
				{
					num2 += rx2_preamp_offset;
				}
				return num2;
			}
		}
	}

	public static float RX2OffsetWithDup
	{
		get
		{
			lock (_rx2_offset_locker)
			{
				bool num = localMox(2);
				bool flag = true;
				float num2;
				if (num)
				{
					num2 = tx_display_cal_offset;
					if (flag)
					{
						num2 += rx2_display_cal_offset;
						num2 += tx_attenuator_offset;
					}
				}
				else
				{
					num2 = rx2_display_cal_offset;
				}
				if (!num)
				{
					num2 += rx2_preamp_offset;
				}
				return num2;
			}
		}
	}

	public static bool ActivePeakInTxRX1
	{
		get
		{
			return _activePeakInTxRX1;
		}
		set
		{
			_activePeakInTxRX1 = value;
		}
	}

	public static bool ActivePeakInTxRX2
	{
		get
		{
			return _activePeakInTxRX2;
		}
		set
		{
			_activePeakInTxRX2 = value;
		}
	}

	public static float NFshiftDBM
	{
		get
		{
			return NFshiftDBM;
		}
		set
		{
			float num = value;
			if (num < -12f)
			{
				num = 12f;
			}
			if (num > 12f)
			{
				num = 12f;
			}
			_fNFshiftDBM = num;
		}
	}

	public static int NFsensitivity
	{
		get
		{
			return _NFsensitivity;
		}
		set
		{
			int num = value;
			if (num < 1)
			{
				num = 1;
			}
			if (num > 19)
			{
				num = 19;
			}
			_NFsensitivity = num;
		}
	}

	internal static System.Drawing.Color[] RX1WaterfallGradient => _rx1_waterfall_grad;

	public static bool UseLinearGradient
	{
		get
		{
			return m_bUseLinearGradient;
		}
		set
		{
			m_bUseLinearGradient = value;
			if (m_bUseLinearGradient)
			{
				_bRebuildRXLinearGradBrush = true;
			}
		}
	}

	public static bool UseLinearGradientForDataLine
	{
		get
		{
			return m_bUseLinearGradientForDataLine;
		}
		set
		{
			m_bUseLinearGradientForDataLine = value;
		}
	}

	public static bool UseLinearGradientTX
	{
		get
		{
			return m_bUseLinearGradientTX;
		}
		set
		{
			m_bUseLinearGradientTX = value;
			if (m_bUseLinearGradientTX)
			{
				_bRebuildTXLinearGradBrush = true;
			}
		}
	}

	public static bool UseLinearGradientForDataLineTX
	{
		get
		{
			return m_bUseLinearGradientForDataLineTX;
		}
		set
		{
			m_bUseLinearGradientForDataLineTX = value;
		}
	}

	public static int CachedMeasureStringsCount => m_stringSizeCache.Count;

	public static bool AlwaysShowCursorInfo
	{
		get
		{
			return m_bAlwaysShowCursorInfo;
		}
		set
		{
			m_bAlwaysShowCursorInfo = value;
		}
	}

	public static string MHzCursorDisplay
	{
		internal get
		{
			return m_sMHzCursorDisplay;
		}
		set
		{
			m_sMHzCursorDisplay = value;
		}
	}

	public static string OtherData1CursorDisplay
	{
		internal get
		{
			return m_sOtherData1CursorDisplay;
		}
		set
		{
			m_sOtherData1CursorDisplay = value;
		}
	}

	public static string OtherData2CursorDisplay
	{
		internal get
		{
			return m_sOtherData2CursorDisplay;
		}
		set
		{
			m_sOtherData2CursorDisplay = value;
		}
	}

	public static bool JoinBandEdges
	{
		get
		{
			return _joinBandEdges;
		}
		set
		{
			_joinBandEdges = value;
		}
	}

	public static bool ShowPhaseAngularMean
	{
		get
		{
			return m_bShowPhaseAngularMean;
		}
		set
		{
			m_bShowPhaseAngularMean = value;
		}
	}

	public static bool NoiseFloorDecimal
	{
		get
		{
			return _NFDecimal;
		}
		set
		{
			_NFDecimal = value;
		}
	}

	public static bool DisplayShiftKeyDown
	{
		get
		{
			return _bShiftKeyDown;
		}
		set
		{
			_bShiftKeyDown = value;
		}
	}

	public static int CachedDXBrushes
	{
		get
		{
			if (_DX2Brushes != null)
			{
				return _DX2Brushes.Count;
			}
			return 0;
		}
	}

	public static bool ShowTCISpots
	{
		get
		{
			return _showTCISpots;
		}
		set
		{
			_showTCISpots = value;
		}
	}

	public static bool FlashNewTCISpots
	{
		get
		{
			return _flashNewTCISpots;
		}
		set
		{
			_flashNewTCISpots = value;
		}
	}

	public static bool OverrideSpotFlashColour
	{
		get
		{
			return _override_spot_flash_colour;
		}
		set
		{
			_override_spot_flash_colour = value;
		}
	}

	public static System.Drawing.Color SpotFlashColour
	{
		get
		{
			return _spot_flash_colour;
		}
		set
		{
			_spot_flash_colour = value;
		}
	}

	public static bool UseLegacyBuffers
	{
		get
		{
			return _bUseLegacyBuffers;
		}
		set
		{
			_bUseLegacyBuffers = value;
		}
	}

	public static float RX1FFTFillTime
	{
		get
		{
			return _fft_fill_timeRX1;
		}
		set
		{
			_fft_fill_timeRX1 = value;
		}
	}

	public static float RX2FFTFillTime
	{
		get
		{
			return _fft_fill_timeRX2;
		}
		set
		{
			_fft_fill_timeRX2 = value;
		}
	}

	public static bool WDSPMOXTransitionBufferClear
	{
		get
		{
			return _wdsp_mox_transition_buffer_clear;
		}
		set
		{
			_wdsp_mox_transition_buffer_clear = value;
		}
	}

	public static bool SnowFall
	{
		get
		{
			return _snowFall;
		}
		set
		{
			_snowFall = value;
			if (!_snowFall)
			{
				lock (_snowLock)
				{
					_snow.Clear();
					_showSanta = false;
				}
			}
		}
	}

	public static bool ShowSpotFlags
	{
		get
		{
			return _spot_flags;
		}
		set
		{
			_spot_flags = value;
			_spot_cache_dirty = true;
		}
	}

	public static int TCIBackPanelAlpha
	{
		get
		{
			return _spot_backpanel_alpha;
		}
		set
		{
			_spot_backpanel_alpha = value;
			_spot_cache_dirty = true;
		}
	}

	public static event Action<int, double> GPUWaterfallEffectiveOverlapChanged;

	private static SharpDX.Direct2D1.AlphaMode BitmapAlphaModeForFormat(Format fmt)
	{
		if (fmt != Format.R16G16B16A16_Float)
		{
			return SharpDX.Direct2D1.AlphaMode.Premultiplied;
		}
		return SharpDX.Direct2D1.AlphaMode.Ignore;
	}

	internal static void RequestDetachedResize(int width, int height)
	{
		_detachedResizeW = Math.Max(64, width);
		_detachedResizeH = Math.Max(64, height);
		_detachedResizePending = true;
	}

	internal static void PerformDetachedResize()
	{
		if (!_detachedResizePending)
		{
			return;
		}
		long num = Environment.TickCount;
		if (num - _detachedResizeLastApplyMs < 100)
		{
			return;
		}
		_detachedResizeLastApplyMs = num;
		_detachedResizePending = false;
		int detachedResizeW = _detachedResizeW;
		int detachedResizeH = _detachedResizeH;
		DetachedPanafallRenderer detachedRenderer = _detachedRenderer;
		GPUWaterfallLogger.Log("DetachedResize", "performing " + detachedResizeW + "x" + detachedResizeH + " renderer=" + (detachedRenderer != null));
		if (detachedRenderer == null)
		{
			return;
		}
		try
		{
			lock (_objDX2Lock)
			{
				if (!_bDX2Setup)
				{
					GPUWaterfallLogger.Log("DetachedResize", "DX not setup, abort");
					return;
				}
				detachedRenderer.Resize(detachedResizeW, detachedResizeH);
				GPUWaterfallLogger.Log("DetachedResize", "done, renderer now " + detachedRenderer.DebugSize);
			}
		}
		catch (Exception ex)
		{
			GPUWaterfallLogger.Log("DetachedResize", "exception: " + ex);
		}
	}

	public static void SetupDelegates()
	{
		_rx1ClickDisplayCTUN = console.ClickTuneDisplay;
		_rx2ClickDisplayCTUN = console.ClickTuneRX2Display;
		Console obj = console;
		obj.PowerChangeHanders = (Console.PowerChanged)Delegate.Combine(obj.PowerChangeHanders, new Console.PowerChanged(OnPowerChangeHander));
		Console obj2 = console;
		obj2.BandChangeHandlers = (Console.BandChanged)Delegate.Combine(obj2.BandChangeHandlers, new Console.BandChanged(OnBandChangeHandler));
		Console obj3 = console;
		obj3.AttenuatorDataChangedHandlers = (Console.AttenuatorDataChanged)Delegate.Combine(obj3.AttenuatorDataChangedHandlers, new Console.AttenuatorDataChanged(OnAttenuatorDataChanged));
		Console obj4 = console;
		obj4.PreampModeChangedHandlers = (Console.PreampModeChanged)Delegate.Combine(obj4.PreampModeChangedHandlers, new Console.PreampModeChanged(OnPreampModeChanged));
		Console obj5 = console;
		obj5.CentreFrequencyHandlers = (Console.CentreFrequencyChanged)Delegate.Combine(obj5.CentreFrequencyHandlers, new Console.CentreFrequencyChanged(OnCentreFrequencyChanged));
		Console obj6 = console;
		obj6.CTUNChangedHandlers = (Console.CTUNChanged)Delegate.Combine(obj6.CTUNChangedHandlers, new Console.CTUNChanged(OnCTUNChanged));
		Console obj7 = console;
		obj7.MinimumRXNotchWidthChangedHandlers = (Console.MinimumRXNotchWidthChanged)Delegate.Combine(obj7.MinimumRXNotchWidthChangedHandlers, new Console.MinimumRXNotchWidthChanged(OnMinRXNotchWidthChanged));
		Console obj8 = console;
		obj8.MinimumTXNotchWidthChangedHandlers = (Console.MinimumTXNotchWidthChanged)Delegate.Combine(obj8.MinimumTXNotchWidthChangedHandlers, new Console.MinimumTXNotchWidthChanged(OnMinTXNotchWidthChanged));
		Console obj9 = console;
		obj9.WaterfallRXGradientChangedHandlers = (Console.WaterfallRXGradientChanged)Delegate.Combine(obj9.WaterfallRXGradientChangedHandlers, new Console.WaterfallRXGradientChanged(OnWaterfallRXGradientChanged));
		Console obj10 = console;
		obj10.WaterfallTXGradientChangedHandlers = (Console.WaterfallTXGradientChanged)Delegate.Combine(obj10.WaterfallTXGradientChangedHandlers, new Console.WaterfallTXGradientChanged(OnWaterfallTXGradientChanged));
	}

	public static void RemoveDelegates()
	{
		Console obj = console;
		obj.PowerChangeHanders = (Console.PowerChanged)Delegate.Remove(obj.PowerChangeHanders, new Console.PowerChanged(OnPowerChangeHander));
		Console obj2 = console;
		obj2.BandChangeHandlers = (Console.BandChanged)Delegate.Remove(obj2.BandChangeHandlers, new Console.BandChanged(OnBandChangeHandler));
		Console obj3 = console;
		obj3.AttenuatorDataChangedHandlers = (Console.AttenuatorDataChanged)Delegate.Remove(obj3.AttenuatorDataChangedHandlers, new Console.AttenuatorDataChanged(OnAttenuatorDataChanged));
		Console obj4 = console;
		obj4.PreampModeChangedHandlers = (Console.PreampModeChanged)Delegate.Remove(obj4.PreampModeChangedHandlers, new Console.PreampModeChanged(OnPreampModeChanged));
		Console obj5 = console;
		obj5.CentreFrequencyHandlers = (Console.CentreFrequencyChanged)Delegate.Remove(obj5.CentreFrequencyHandlers, new Console.CentreFrequencyChanged(OnCentreFrequencyChanged));
		Console obj6 = console;
		obj6.CTUNChangedHandlers = (Console.CTUNChanged)Delegate.Remove(obj6.CTUNChangedHandlers, new Console.CTUNChanged(OnCTUNChanged));
		Console obj7 = console;
		obj7.MinimumRXNotchWidthChangedHandlers = (Console.MinimumRXNotchWidthChanged)Delegate.Remove(obj7.MinimumRXNotchWidthChangedHandlers, new Console.MinimumRXNotchWidthChanged(OnMinRXNotchWidthChanged));
		Console obj8 = console;
		obj8.MinimumTXNotchWidthChangedHandlers = (Console.MinimumTXNotchWidthChanged)Delegate.Remove(obj8.MinimumTXNotchWidthChangedHandlers, new Console.MinimumTXNotchWidthChanged(OnMinTXNotchWidthChanged));
		Console obj9 = console;
		obj9.WaterfallRXGradientChangedHandlers = (Console.WaterfallRXGradientChanged)Delegate.Remove(obj9.WaterfallRXGradientChangedHandlers, new Console.WaterfallRXGradientChanged(OnWaterfallRXGradientChanged));
		Console obj10 = console;
		obj10.WaterfallTXGradientChangedHandlers = (Console.WaterfallTXGradientChanged)Delegate.Remove(obj10.WaterfallTXGradientChangedHandlers, new Console.WaterfallTXGradientChanged(OnWaterfallTXGradientChanged));
	}

	private static void OnMinRXNotchWidthChanged(int rx, double width)
	{
		if (rx >= 1 && rx <= 2)
		{
			_mnfMinSizeRX[rx - 1] = width;
		}
	}

	private static void OnMinTXNotchWidthChanged(double width)
	{
		_mnfMinSizeTX = width;
	}

	private static void OnCTUNChanged(int rx, bool oldCTUN, bool newCTUN, Band band)
	{
		switch (rx)
		{
		case 1:
			_rx1ClickDisplayCTUN = newCTUN;
			break;
		case 2:
			_rx2ClickDisplayCTUN = newCTUN;
			break;
		}
	}

	private static void OnPowerChangeHander(bool oldPower, bool newPower)
	{
		if (newPower)
		{
			PurgeBuffers();
		}
	}

	private static void OnBandChangeHandler(int rx, Band oldBand, Band newBand)
	{
		setCurrentWaterfallBand(rx, newBand);
		if (rx == 1)
		{
			FastAttackNoiseFloorRX1 = true;
			_RX1waterfallPreviousMinValue = getWaterfallCachedPreviousMinOrFloor(1, isTxContext: false);
		}
		else
		{
			FastAttackNoiseFloorRX2 = true;
			_RX2waterfallPreviousMinValue = getWaterfallCachedPreviousMinOrFloor(2, isTxContext: false);
		}
	}

	private static void processBlobsActivePeakDisplayDelay()
	{
		if ((m_bDelayRX1Blobs || m_bDelayRX2Blobs || m_bDelayRX1SpectrumPeaks || m_bDelayRX2SpectrumPeaks) && m_dElapsedFrameStart > m_dPeakDelay)
		{
			if (m_bDelayRX1Blobs)
			{
				m_bDelayRX1Blobs = false;
			}
			if (m_bDelayRX2Blobs)
			{
				m_bDelayRX2Blobs = false;
			}
			if (m_bDelayRX1SpectrumPeaks)
			{
				m_bDelayRX1SpectrumPeaks = false;
			}
			if (m_bDelayRX2SpectrumPeaks)
			{
				m_bDelayRX2SpectrumPeaks = false;
			}
		}
	}

	private static void delayBlobsActivePeakDisplay(int rx, bool blobs)
	{
		if (rx == 1)
		{
			if (blobs)
			{
				m_bDelayRX1Blobs = true;
			}
			else
			{
				m_bDelayRX1SpectrumPeaks = true;
			}
		}
		else if (blobs)
		{
			m_bDelayRX2Blobs = true;
		}
		else
		{
			m_bDelayRX2SpectrumPeaks = true;
		}
		m_dPeakDelay = m_dElapsedFrameStart + 500.0;
	}

	private static void resetPeaksAndNoise(int rx)
	{
		ResetBlobMaximums(rx, bClear: true);
		ResetSpectrumPeaks(rx);
		switch (rx)
		{
		case 1:
			FastAttackNoiseFloorRX1 = true;
			break;
		case 2:
			FastAttackNoiseFloorRX2 = true;
			break;
		}
	}

	private static void OnAttenuatorDataChanged(int rx, int oldAtt, int newAtt)
	{
		if (rx == 1)
		{
			FastAttackNoiseFloorRX1 = true;
		}
		else
		{
			FastAttackNoiseFloorRX2 = true;
		}
	}

	private static void OnPreampModeChanged(int rx, PreampMode oldMode, PreampMode newMode)
	{
		if (rx == 1)
		{
			FastAttackNoiseFloorRX1 = true;
		}
		else
		{
			FastAttackNoiseFloorRX2 = true;
		}
	}

	private static void OnCentreFrequencyChanged(int rx, double oldFreq, double newFreq, Band band, double offset)
	{
		if (rx == 1)
		{
			if (Math.Abs(oldFreq - newFreq) > 0.5)
			{
				FastAttackNoiseFloorRX1 = true;
			}
			ResetBlobMaximums(1, bClear: true);
			ResetSpectrumPeaks(1);
		}
		else
		{
			if (Math.Abs(oldFreq - newFreq) > 0.5)
			{
				FastAttackNoiseFloorRX2 = true;
			}
			ResetBlobMaximums(2, bClear: true);
			ResetSpectrumPeaks(2);
		}
	}

	private static int PowerOfTwo(int v)
	{
		int[] array = new int[9] { 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144 };
		int num = array[0];
		int num2 = Math.Abs(v - num);
		for (int i = 1; i < array.Length; i++)
		{
			int num3 = Math.Abs(v - array[i]);
			if (num3 < num2)
			{
				num2 = num3;
				num = array[i];
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool localMox(int rx)
	{
		switch (rx)
		{
		case 1:
			if (_mox)
			{
				if (_tx_on_vfob)
				{
					if (_tx_on_vfob)
					{
						return !_rx2_enabled;
					}
					return false;
				}
				return true;
			}
			return false;
		case 2:
			if (_mox)
			{
				if (_tx_on_vfob)
				{
					return _rx2_enabled;
				}
				return false;
			}
			return false;
		default:
			return false;
		}
	}

	private static int getWaterfallRxIndex(int rx)
	{
		return rx - 1;
	}

	private static void setCurrentWaterfallBand(int rx, Band band)
	{
		int waterfallRxIndex = getWaterfallRxIndex(rx);
		if (waterfallRxIndex >= 0 && waterfallRxIndex < _currentWaterfallBandByRx.Length)
		{
			_currentWaterfallBandByRx[waterfallRxIndex] = (int)band;
		}
	}

	private static int getWaterfallContextBandValue(int rx, bool isTxContext)
	{
		if (!isTxContext)
		{
			int waterfallRxIndex = getWaterfallRxIndex(rx);
			if (waterfallRxIndex < 0 || waterfallRxIndex >= _currentWaterfallBandByRx.Length)
			{
				return int.MinValue;
			}
			return _currentWaterfallBandByRx[waterfallRxIndex];
		}
		int num = ((_tx_on_vfob && _rx2_enabled) ? 1 : 0);
		if (num < 0 || num >= _currentWaterfallBandByRx.Length)
		{
			return int.MinValue;
		}
		return _currentWaterfallBandByRx[num];
	}

	private static float getWaterfallCachedPreviousMinOrFloor(int rx, bool isTxContext)
	{
		int waterfallContextBandValue = getWaterfallContextBandValue(rx, isTxContext);
		if (waterfallContextBandValue != int.MinValue && _waterfallAgcCache.TryGetValue(new WaterfallAgcCacheKey(isTxContext, waterfallContextBandValue), out var value))
		{
			return value.PreviousMin;
		}
		return -150f;
	}

	private static void updateWaterfallAgcCache(int rx, bool isTxContext, float previousMin)
	{
		int waterfallContextBandValue = getWaterfallContextBandValue(rx, isTxContext);
		if (waterfallContextBandValue != int.MinValue)
		{
			_waterfallAgcCache[new WaterfallAgcCacheKey(isTxContext, waterfallContextBandValue)] = new WaterfallAgcCacheEntry
			{
				PreviousMin = previousMin
			};
		}
	}

	private static void clearBuffers(int W, int rx)
	{
		resetPeaksAndNoise(rx);
		if (rx == 1)
		{
			Parallel.For(0, W, delegate(int i)
			{
				histogram_data[i] = int.MaxValue;
				histogram_history[i] = 0;
			});
			Parallel.For(0, W, delegate(int i)
			{
				new_display_data[i] = -200f;
				current_display_data[i] = -200f;
				new_waterfall_data[i] = -200f;
				current_waterfall_data[i] = -200f;
				current_display_data_copy[i] = -200f;
				current_display_data_bottom_copy[i] = -200f;
			});
			_rx1_no_agc_duration = _high_perf_timer.ElapsedMsec + (double)_fft_fill_timeRX1 + (double)((float)m_nFps / 1000f * 2f);
			_ignore_waterfall_rx1_agc = true;
		}
		else
		{
			Parallel.For(0, W, delegate(int i)
			{
				new_display_data_bottom[i] = -200f;
				current_display_data_bottom[i] = -200f;
				new_waterfall_data_bottom[i] = -200f;
				current_waterfall_data_bottom[i] = -200f;
				current_waterfall_data_copy[i] = -200f;
				current_waterfall_data_bottom_copy[i] = -200f;
			});
			_rx2_no_agc_duration = _high_perf_timer.ElapsedMsec + (double)_fft_fill_timeRX2 + (double)((float)m_nFps / 1000f * 2f);
			_ignore_waterfall_rx2_agc = true;
		}
	}

	private static void initDisplayArrays(int W, int H)
	{
		lock (_objDX2Lock)
		{
			if (histogram_data != null)
			{
				m_objIntPool.Return(histogram_data);
			}
			if (histogram_history != null)
			{
				m_objIntPool.Return(histogram_history);
			}
			histogram_data = m_objIntPool.Rent(W);
			histogram_history = m_objIntPool.Rent(W);
			if (new_display_data != null)
			{
				m_objFloatPool.Return(new_display_data);
			}
			if (current_display_data != null)
			{
				m_objFloatPool.Return(current_display_data);
			}
			if (new_display_data_bottom != null)
			{
				m_objFloatPool.Return(new_display_data_bottom);
			}
			if (current_display_data_bottom != null)
			{
				m_objFloatPool.Return(current_display_data_bottom);
			}
			if (new_waterfall_data != null)
			{
				m_objFloatPool.Return(new_waterfall_data);
			}
			if (current_waterfall_data != null)
			{
				m_objFloatPool.Return(current_waterfall_data);
			}
			if (new_waterfall_data_bottom != null)
			{
				m_objFloatPool.Return(new_waterfall_data_bottom);
			}
			if (current_waterfall_data_bottom != null)
			{
				m_objFloatPool.Return(current_waterfall_data_bottom);
			}
			if (current_display_data_copy != null)
			{
				m_objFloatPool.Return(current_display_data_copy);
			}
			if (current_waterfall_data_copy != null)
			{
				m_objFloatPool.Return(current_waterfall_data_copy);
			}
			if (current_display_data_bottom_copy != null)
			{
				m_objFloatPool.Return(current_display_data_bottom_copy);
			}
			if (current_waterfall_data_bottom_copy != null)
			{
				m_objFloatPool.Return(current_waterfall_data_bottom_copy);
			}
			new_display_data = m_objFloatPool.Rent(16384);
			current_display_data = m_objFloatPool.Rent(16384);
			new_display_data_bottom = m_objFloatPool.Rent(16384);
			current_display_data_bottom = m_objFloatPool.Rent(16384);
			current_display_data_copy = m_objFloatPool.Rent(16384);
			current_display_data_bottom_copy = m_objFloatPool.Rent(16384);
			new_waterfall_data = m_objFloatPool.Rent(W);
			current_waterfall_data = m_objFloatPool.Rent(W);
			new_waterfall_data_bottom = m_objFloatPool.Rent(W);
			current_waterfall_data_bottom = m_objFloatPool.Rent(W);
			current_waterfall_data_copy = m_objFloatPool.Rent(W);
			current_waterfall_data_bottom_copy = m_objFloatPool.Rent(W);
			m_rx1_spectrumPeaks = new Maximums[W];
			m_rx2_spectrumPeaks = new Maximums[W];
			clearBuffers(W, 1);
			clearBuffers(W, 2);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static System.Drawing.Color changeAlpha(System.Drawing.Color c, int A)
	{
		return System.Drawing.Color.FromArgb(A, c.R, c.G, c.B);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float dBToPixel(float dB, int H, bool tx = false)
	{
		if (!tx)
		{
			return ((float)spectrum_grid_max - dB) * (float)H / (float)(spectrum_grid_max - spectrum_grid_min);
		}
		return ((float)tx_spectrum_grid_max - dB) * (float)H / (float)(tx_spectrum_grid_max - tx_spectrum_grid_min);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float dBToRX2Pixel(float dB, int H, bool tx = false)
	{
		if (!tx)
		{
			return ((float)rx2_spectrum_grid_max - dB) * (float)H / (float)(rx2_spectrum_grid_max - rx2_spectrum_grid_min);
		}
		return ((float)tx_spectrum_grid_max - dB) * (float)H / (float)(tx_spectrum_grid_max - tx_spectrum_grid_min);
	}

	private static void updateSharePointsArray(int nW)
	{
		if (nW == lastResize)
		{
			points = pointsStore1;
			return;
		}
		if (nW == lastResize + 2)
		{
			points = pointsStore2;
			return;
		}
		pointsStore1 = new System.Drawing.Point[nW];
		pointsStore2 = new System.Drawing.Point[nW + 2];
		points = pointsStore1;
		lastResize = nW;
	}

	public static void RecalculateWaterfallRowIntervals()
	{
		int num = Math.Max(1, _targetFps);
		int num2 = Math.Max(1, Math.Min(waterfall_update_period, 9999));
		int num3 = Math.Max(1, Math.Min(rx2_waterfall_update_period, 9999));
		_wfRowIntervalMsRX1 = (double)num2 * 1000.0 / (double)num;
		_wfRowIntervalMsRX2 = (double)num3 * 1000.0 / (double)num;
		if (_wfRowTimerRX1 != null)
		{
			_wfLastRowTimeRX1 = _wfRowTimerRX1.ElapsedMsec;
		}
		if (_wfRowTimerRX2 != null)
		{
			_wfLastRowTimeRX2 = _wfRowTimerRX2.ElapsedMsec;
		}
	}

	private static void ResetWaterfallBmp()
	{
		int num = displayTargetHeight;
		if (current_display_mode == DisplayMode.PANAFALL)
		{
			num /= 2;
		}
		if (_rx2_enabled)
		{
			num /= 2;
		}
		int num2 = 0;
		if (!_rx2_enabled && current_display_mode == DisplayMode.PANAFALL)
		{
			num = displayTargetHeight - PanafallSplitBarPos;
		}
		lock (_objDX2Lock)
		{
			if (_bDX2Setup)
			{
				SharpDX.Direct2D1.Bitmap comObject = null;
				if (_waterfall_bmp_dx2d != null && !_waterfall_bmp_dx2d.IsDisposed && (float)displayTargetWidth == _waterfall_bmp_dx2d.Size.Width)
				{
					bool flag = false;
					try
					{
						flag = _waterfall_bmp_dx2d.PixelFormat.Format == Format.B8G8R8A8_UNorm;
					}
					catch
					{
					}
					if (flag)
					{
						int num3 = Math.Min(num - 20, (int)_waterfall_bmp_dx2d.Size.Height);
						num2 = num3;
						comObject = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp_dx2d.Size.Width, num3), new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, BitmapAlphaModeForFormat(Format.B8G8R8A8_UNorm))));
						try
						{
							comObject.CopyFromBitmap(_waterfall_bmp_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)comObject.Size.Width, (int)comObject.Size.Height));
						}
						catch (Exception ex)
						{
							LogTool.AddLogEntry("ResetWaterfallBmp: unable to preserve waterfall content: " + ex.Message, "DX2D");
							Utilities.Dispose(ref comObject);
							comObject = null;
							num2 = 0;
						}
					}
				}
				if (_waterfall_bmp_dx2d != null)
				{
					Utilities.Dispose(ref _waterfall_bmp_dx2d);
					_waterfall_bmp_dx2d = null;
				}
				_waterfall_bmp_dx2d = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2(displayTargetWidth, num - 20), new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, BitmapAlphaModeForFormat(Format.B8G8R8A8_UNorm))));
				clearWaterfallBitmapRegion(_waterfall_bmp_dx2d, 0, 0, displayTargetWidth, num - 20);
				if (_d2dRenderTarget is SharpDX.Direct2D1.DeviceContext deviceContext)
				{
					Format dxgiFormat = WaterfallPixelWriter.DxgiFormat;
					if (_waterfallGPU1 == null)
					{
						_waterfallGPU1 = new WaterfallGPURenderer(_device, deviceContext, displayTargetWidth, num - 20, dxgiFormat);
					}
					else
					{
						_waterfallGPU1.Resize(deviceContext, displayTargetWidth, num - 20, dxgiFormat);
					}
				}
				EnsureGPUWaterfallPipeline(1, displayTargetWidth, num - 20);
				if (comObject != null)
				{
					_waterfall_bmp_dx2d.CopyFromBitmap(comObject, new SharpDX.Point(0, 0));
					Utilities.Dispose(ref comObject);
					comObject = null;
				}
			}
		}
		if (num2 > 0)
		{
			resizeWaterfallTimeOverlay(1, num - 20, num2);
			return;
		}
		resetWaterfallTimeOverlay(1);
		resetWaterfallBitmapAlignment(1);
	}

	private static void ResetWaterfallBmp2()
	{
		int num = displayTargetHeight;
		if (current_display_mode_bottom == DisplayMode.PANAFALL)
		{
			num /= 2;
		}
		num /= 2;
		int num2 = 0;
		lock (_objDX2Lock)
		{
			if (_bDX2Setup)
			{
				SharpDX.Direct2D1.Bitmap comObject = null;
				if (_waterfall_bmp2_dx2d != null && !_waterfall_bmp2_dx2d.IsDisposed && (float)displayTargetWidth == _waterfall_bmp2_dx2d.Size.Width)
				{
					bool flag = false;
					try
					{
						flag = _waterfall_bmp2_dx2d.PixelFormat.Format == Format.B8G8R8A8_UNorm;
					}
					catch
					{
					}
					if (flag)
					{
						int num3 = Math.Min(num - 20, (int)_waterfall_bmp2_dx2d.Size.Height);
						num2 = num3;
						comObject = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp2_dx2d.Size.Width, num3), new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, BitmapAlphaModeForFormat(Format.B8G8R8A8_UNorm))));
						try
						{
							comObject.CopyFromBitmap(_waterfall_bmp2_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)comObject.Size.Width, (int)comObject.Size.Height));
						}
						catch (Exception ex)
						{
							LogTool.AddLogEntry("ResetWaterfallBmp2: unable to preserve waterfall content: " + ex.Message, "DX2D");
							Utilities.Dispose(ref comObject);
							comObject = null;
							num2 = 0;
						}
					}
				}
				if (_waterfall_bmp2_dx2d != null)
				{
					Utilities.Dispose(ref _waterfall_bmp2_dx2d);
					_waterfall_bmp2_dx2d = null;
				}
				_waterfall_bmp2_dx2d = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2(displayTargetWidth, num - 20), new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, BitmapAlphaModeForFormat(Format.B8G8R8A8_UNorm))));
				clearWaterfallBitmapRegion(_waterfall_bmp2_dx2d, 0, 0, displayTargetWidth, num - 20);
				if (_d2dRenderTarget is SharpDX.Direct2D1.DeviceContext deviceContext)
				{
					Format dxgiFormat = WaterfallPixelWriter.DxgiFormat;
					if (_waterfallGPU2 == null)
					{
						_waterfallGPU2 = new WaterfallGPURenderer(_device, deviceContext, displayTargetWidth, num - 20, dxgiFormat);
					}
					else
					{
						_waterfallGPU2.Resize(deviceContext, displayTargetWidth, num - 20, dxgiFormat);
					}
				}
				EnsureGPUWaterfallPipeline(2, displayTargetWidth, num - 20);
				if (comObject != null)
				{
					_waterfall_bmp2_dx2d.CopyFromBitmap(comObject, new SharpDX.Point(0, 0));
					Utilities.Dispose(ref comObject);
					comObject = null;
				}
			}
		}
		if (num2 > 0)
		{
			resizeWaterfallTimeOverlay(2, num - 20, num2);
			return;
		}
		resetWaterfallTimeOverlay(2);
		resetWaterfallBitmapAlignment(2);
	}

	private static void EnsureGPUWaterfallPipeline(int rx, int width, int height)
	{
		if (!_gpuWaterfallPipelineEnabled || !_gpuEffectsEnabled)
		{
			if (rx == 1)
			{
				Utilities.Dispose(ref _gpuFFT1);
			}
			else
			{
				Utilities.Dispose(ref _gpuFFT2);
			}
			return;
		}
		int num = cmaster.GetInputRate(0, rx - 1);
		if (num <= 0)
		{
			num = ((rx == 1) ? SampleRateRX1 : SampleRateRX2);
		}
		GPUWaterfallPipeline gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
		bool num2 = gPUWaterfallPipeline == null;
		if (num2)
		{
			gPUWaterfallPipeline = new GPUWaterfallPipeline(_device, _gpuWaterfallFFTSize, width, num);
			if (rx == 1)
			{
				_gpuFFT1 = gPUWaterfallPipeline;
			}
			else
			{
				_gpuFFT2 = gPUWaterfallPipeline;
			}
		}
		else
		{
			gPUWaterfallPipeline.Resize(_gpuWaterfallFFTSize, width, num);
		}
		if (num2)
		{
			ResetGPUWaterfallState(rx);
		}
		if (gPUWaterfallPipeline.IsInitialized)
		{
			gPUWaterfallPipeline.WindowType = _gpuWaterfallWindowType;
			gPUWaterfallPipeline.KaiserBeta = _gpuWaterfallKaiserBeta;
			gPUWaterfallPipeline.MagnitudeMode = _gpuWaterfallMagnitudeMode;
			gPUWaterfallPipeline.LanczosWindow = _gpuWaterfallLanczosWindow;
			gPUWaterfallPipeline.ResamplingMode = _gpuWaterfallResamplingMode;
		}
		LogGPU($"InitOrResizeGPUWaterfall RX{rx}: FFT={_gpuWaterfallFFTSize}, width={width}, SR={num}, window={_gpuWaterfallWindowType}, kaiser={_gpuWaterfallKaiserBeta:F1}, magnitude={_gpuWaterfallMagnitudeMode}, resampling={_gpuWaterfallResamplingMode}, overlap={_gpuWaterfallOverlapPercent}%, initialized={gPUWaterfallPipeline.IsInitialized}");
	}

	private static void ResetGPUWaterfallState(int rx, bool resetCalibration = true)
	{
		lock (_objDX2Lock)
		{
			int num = rx - 1;
			_gpuSampleCredit[num] = 0;
			_gpuIQringHead[num] = 0;
			_gpuIQringCount[num] = 0;
			_gpuFirstFillDone[num] = false;
			if (_gpuIQringI[num] != null)
			{
				Array.Clear(_gpuIQringI[num], 0, _gpuIQringI[num].Length);
			}
			if (_gpuIQringQ[num] != null)
			{
				Array.Clear(_gpuIQringQ[num], 0, _gpuIQringQ[num].Length);
			}
			if (resetCalibration)
			{
				if (rx == 1)
				{
					_gpuCalInitRX1 = false;
					_gpuCalStartupCountRX1 = 0;
					_gpuLastFFTSizeRX1 = 0;
				}
				else
				{
					_gpuCalInitRX2 = false;
					_gpuCalStartupCountRX2 = 0;
					_gpuLastFFTSizeRX2 = 0;
				}
			}
			HiPerfTimer hiPerfTimer = ((rx == 1) ? _wfRowTimerRX1 : _wfRowTimerRX2);
			if (hiPerfTimer != null)
			{
				if (rx == 1)
				{
					_wfLastRowTimeRX1 = hiPerfTimer.ElapsedMsec;
				}
				else
				{
					_wfLastRowTimeRX2 = hiPerfTimer.ElapsedMsec;
				}
			}
		}
	}

	private static float[] ProcessGPUWaterfall(int rx, int width)
	{
		if (!_gpuWaterfallPipelineEnabled)
		{
			return null;
		}
		GPUWaterfallPipeline gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
		if (gPUWaterfallPipeline == null || !gPUWaterfallPipeline.IsInitialized)
		{
			EnsureGPUWaterfallPipeline(rx, width, 1);
			gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
		}
		if (gPUWaterfallPipeline != null && gPUWaterfallPipeline.IsInitialized && gPUWaterfallPipeline.FFTSize != _gpuWaterfallFFTSize)
		{
			EnsureGPUWaterfallPipeline(rx, width, 1);
			gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
		}
		int inputRate = cmaster.GetInputRate(0, rx - 1);
		if (gPUWaterfallPipeline != null && inputRate > 0 && Math.Abs(gPUWaterfallPipeline.SampleRate - (float)inputRate) > 1f)
		{
			EnsureGPUWaterfallPipeline(rx, width, 1);
			gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
			ResetGPUWaterfallState(rx);
		}
		if (gPUWaterfallPipeline == null || !gPUWaterfallPipeline.IsInitialized)
		{
			if (!_gpuPipeFailLogged[rx - 1])
			{
				_gpuPipeFailLogged[rx - 1] = true;
				LogGPU(string.Format("ProcessGPUWaterfall RX{0}: pipe {1}", rx, (gPUWaterfallPipeline == null) ? "null" : "not initialized"));
			}
			return null;
		}
		_gpuPipeFailLogged[rx - 1] = false;
		int fFTSize = gPUWaterfallPipeline.FFTSize;
		int num = ((rx == 1) ? _gpuLastFFTSizeRX1 : _gpuLastFFTSizeRX2);
		if (fFTSize != num)
		{
			if (rx == 1)
			{
				_gpuCalInitRX1 = false;
				_gpuLastFFTSizeRX1 = fFTSize;
			}
			else
			{
				_gpuCalInitRX2 = false;
				_gpuLastFFTSizeRX2 = fFTSize;
			}
		}
		int num2 = rx - 1;
		if (fFTSize != num)
		{
			_gpuSampleCredit[num2] = 0;
		}
		if (_gpuIQringI[num2] == null || _gpuIQringI[num2].Length < 524288)
		{
			_gpuIQringI[num2] = new float[524288];
			_gpuIQringQ[num2] = new float[524288];
			_gpuIQringHead[num2] = 0;
			_gpuIQringCount[num2] = 0;
			_gpuFirstFillDone[num2] = false;
		}
		if (_gpuIQbufI == null || _gpuIQbufI.Length < 524288)
		{
			_gpuIQbufI = new float[524288];
			_gpuIQbufQ = new float[524288];
		}
		int stream = ((rx != 1) ? 1 : 0);
		int num3 = 0;
		int waterfallIQDroppedSamples = cmaster.GetWaterfallIQDroppedSamples(stream);
		if (waterfallIQDroppedSamples > 0)
		{
			long num4 = Environment.TickCount;
			if (num4 - _gpuLastDropLogMs[num2] >= 1000)
			{
				_gpuLastDropLogMs[num2] = num4;
				LogGPU($"RX{rx} I/Q ring dropped {waterfallIQDroppedSamples} samples (reader lagged behind writer)");
			}
			cmaster.ResetWaterfallIQDropped(stream);
		}
		int num5 = _gpuIQringCount[num2];
		int num6 = _gpuIQringHead[num2];
		int num7 = cmaster.CM_WaterfallIQ_Available(stream);
		if (num7 > 0)
		{
			int maxSamples = Math.Min(num7, 2 * fFTSize);
			if (_gpuIQaccumI == null || _gpuIQaccumI.Length < 524288)
			{
				_gpuIQaccumI = new float[524288];
				_gpuIQaccumQ = new float[524288];
			}
			num3 = cmaster.ReadWaterfallIQ(stream, _gpuIQaccumI, _gpuIQaccumQ, maxSamples);
			if (num3 > 0)
			{
				float[] array = _gpuIQringI[num2];
				float[] array2 = _gpuIQringQ[num2];
				for (int i = 0; i < num3; i++)
				{
					array[num6] = _gpuIQaccumI[i];
					array2[num6] = _gpuIQaccumQ[i];
					num6 = (num6 + 1) % fFTSize;
				}
				num5 = Math.Min(num5 + num3, fFTSize);
				_gpuIQringHead[num2] = num6;
				_gpuIQringCount[num2] = num5;
				_gpuSampleCredit[num2] += num3;
			}
		}
		if (num5 < fFTSize)
		{
			return null;
		}
		double num8 = (double)_gpuWaterfallOverlapPercent / 100.0;
		int num9 = Math.Max(1, Math.Min(fFTSize, (int)Math.Round((double)fFTSize * (1.0 - num8))));
		if (_gpuWaterfallAutoOverlap)
		{
			double num10 = Math.Max(1.0, _targetFps);
			int num11 = Math.Max(1, (rx == 1) ? waterfall_update_period : rx2_waterfall_update_period);
			double num12 = ((gPUWaterfallPipeline.SampleRate > 1f) ? gPUWaterfallPipeline.SampleRate : 192000f);
			double num13 = Math.Min(num10 / (double)num11, 30.0);
			double num14 = num12 / (0.05 * (double)fFTSize);
			if (num13 > num14)
			{
				num13 = num14;
			}
			num9 = Math.Max(1, Math.Min(fFTSize, (int)Math.Round(num12 / num13)));
		}
		double num15 = 1.0 - (double)num9 / (double)fFTSize;
		if (Math.Abs(num15 - _gpuLastEffectiveOverlap[num2]) > 0.005)
		{
			_gpuLastEffectiveOverlap[num2] = num15;
			GPUWaterfallEffectiveOverlapChanged?.Invoke(rx, num15);
		}
		if (!_gpuFirstFillDone[num2])
		{
			_gpuFirstFillDone[num2] = true;
			_gpuSampleCredit[num2] = 0;
		}
		else
		{
			if (_gpuSampleCredit[num2] < num9)
			{
				return null;
			}
			_gpuSampleCredit[num2] -= num9;
		}
		int num16 = num9 * 2;
		if (_gpuSampleCredit[num2] > num16)
		{
			_gpuSampleCredit[num2] = num16;
		}
		double num17 = 0.0;
		double num18 = 0.0;
		float num19 = 0f;
		float[] array3 = _gpuIQringI[num2];
		float[] array4 = _gpuIQringQ[num2];
		for (int j = 0; j < fFTSize; j++)
		{
			int num20 = (_gpuIQringHead[num2] + j) % fFTSize;
			float num21 = array3[num20];
			float num22 = array4[num20];
			_gpuIQbufI[j] = num21;
			_gpuIQbufQ[j] = num22;
			num17 += (double)(num21 * num21);
			num18 += (double)(num22 * num22);
			float num23 = Math.Abs(num21);
			if (Math.Abs(num22) > num23)
			{
				num23 = Math.Abs(num22);
			}
			if (num23 > num19)
			{
				num19 = num23;
			}
		}
		Math.Sqrt(num17 / (double)fFTSize);
		Math.Sqrt(num18 / (double)fFTSize);
		int num24;
		int num25;
		if (localMox(rx))
		{
			num24 = ((!DisplayDuplex) ? 1 : 0);
			if (num24 != 0)
			{
				num25 = tx_display_low;
				goto IL_05a5;
			}
		}
		else
		{
			num24 = 0;
		}
		num25 = ((rx == 1) ? RXDisplayLow : RX2DisplayLow);
		goto IL_05a5;
		IL_05a5:
		float num26 = num25;
		float num27 = ((num24 != 0) ? tx_display_high : ((rx == 1) ? RXDisplayHigh : RX2DisplayHigh));
		bool flag = ++_gpuRowLogSkip[num2] >= 30;
		if (flag)
		{
			_gpuRowLogSkip[num2] = 0;
		}
		if (flag)
		{
			LogGPU($"ProcessGPUWaterfall RX{rx}: span set lowFreq={num26:F0}, highFreq={num27:F0}, width={width}, fftSize={fFTSize}");
		}
		gPUWaterfallPipeline.SetFrequencySpan(num26, num27);
		gPUWaterfallPipeline.SetNarrowSpan(SharedWaterfallState.FilterDisplaySpanActive(rx), SharedWaterfallState.FilterDisplaySpanLow(rx), SharedWaterfallState.FilterDisplaySpanHigh(rx));
		float[] array5 = gPUWaterfallPipeline.Process(_gpuIQbufI, _gpuIQbufQ, fFTSize);
		if (flag)
		{
			LogGPU(string.Format("ProcessGPUWaterfall RX{0}: available={1}, got={2}, credit={3}, row={4}, width={5}", rx, num7, num3, _gpuSampleCredit[num2], (array5 != null) ? ("len=" + array5.Length) : "null", width));
		}
		if (array5 != null && array5.Length < width)
		{
			return null;
		}
		if (array5 != null)
		{
			float[] array6 = ((rx == 1) ? _gpuCalReferenceRowRX1 : _gpuCalReferenceRowRX2);
			int num28 = Math.Min(width, (array6 != null) ? array6.Length : 0);
			float num29 = ((rx == 1) ? _gpuCalOffsetRX1 : _gpuCalOffsetRX2);
			int num30 = 0;
			try
			{
				num30 = console.specRX.GetSpecRX((rx != 1) ? 1 : 0).FFTSize;
			}
			catch
			{
			}
			if (num28 > 0)
			{
				float num31 = CalculateMedian(array6, num28);
				if (num31 > -180f)
				{
					float num32 = CalculateMedian(array5, num28);
					float num33 = num31 - num32;
					if (float.IsNaN(num33))
					{
						num33 = 0f;
					}
					if (num33 > 200f)
					{
						num33 = 200f;
					}
					if (num33 < -200f)
					{
						num33 = -200f;
					}
					if (num33 < -12f || num33 > 7f)
					{
						if (++_gpuCalOutlierSkip[num2] >= 30)
						{
							_gpuCalOutlierSkip[num2] = 0;
							LogGPU($"AB RX{rx}: calibration outlier diff={num33:F1} dB ignored (GPU fft={fFTSize}, WDSP fft={num30}) — keeping calOffset={num29:F2}");
						}
					}
					else
					{
						bool num34 = ((rx == 1) ? _gpuCalInitRX1 : _gpuCalInitRX2);
						int num35 = ((rx == 1) ? _gpuCalStartupCountRX1 : _gpuCalStartupCountRX2);
						float num36 = ((!num34 || num35 < 10) ? 0.3f : 0.1f);
						num29 = num29 * (1f - num36) + num33 * num36;
						float num37 = ((rx == 1) ? _gpuCalOffsetRX1 : _gpuCalOffsetRX2);
						if (!num34)
						{
							num29 = num33;
						}
						else if (num29 > num37 + 1f)
						{
							num29 = num37 + 1f;
						}
						else if (num29 < num37 - 1f)
						{
							num29 = num37 - 1f;
						}
						if (!num34)
						{
							if (rx == 1)
							{
								_gpuCalInitRX1 = true;
								_gpuCalStartupCountRX1 = 1;
							}
							else
							{
								_gpuCalInitRX2 = true;
								_gpuCalStartupCountRX2 = 1;
							}
						}
						else if (rx == 1 && num35 < 10)
						{
							_gpuCalStartupCountRX1++;
						}
						else if (rx == 2 && num35 < 10)
						{
							_gpuCalStartupCountRX2++;
						}
						if (rx == 1)
						{
							_gpuCalOffsetRX1 = num29;
						}
						else
						{
							_gpuCalOffsetRX2 = num29;
						}
					}
				}
				for (int k = 0; k < width; k++)
				{
					array5[k] += num29;
				}
			}
			if (++_gpuWaterfallDebugSkip >= 30)
			{
				_gpuWaterfallDebugSkip = 0;
				float num38 = array5[0];
				float num39 = array5[0];
				int num40 = 0;
				for (int l = 1; l < width; l++)
				{
					if (array5[l] < num38)
					{
						num38 = array5[l];
					}
					if (array5[l] > num39)
					{
						num39 = array5[l];
						num40 = l;
					}
				}
				float num41 = ((num28 > 0) ? (CalculateMedian(array5, num28) - num29) : (-200f));
				float num42 = ((num28 > 0) ? CalculateMedian(array6, num28) : (-200f));
				float num43 = ((num28 > 0) ? array6[0] : (-200f));
				if (num28 > 0)
				{
					for (int m = 1; m < num28; m++)
					{
						if (array6[m] > num43)
						{
							num43 = array6[m];
						}
					}
				}
				float num44 = ((rx == 1) ? RXDisplayLow : RX2DisplayLow);
				float num45 = ((rx == 1) ? RXDisplayHigh : RX2DisplayHigh);
				float num46 = num45 - num44;
				if (width > 1)
				{
					_ = (float)num40 * num46 / (float)(width - 1);
				}
				float num47 = gPUWaterfallPipeline.SampleRate / (float)fFTSize;
				_ = num44 / num47;
				_ = num45 / num47;
				LogGPU($"AB RX{rx}: fft={fFTSize}/w{num30}, step={num9}, overlap={num15 * 100.0:F0}%, medians CPU={num42:F1}/GPUraw={num41:F1} dB (diff={num42 - num41:F1}), max CPU={num43:F1}/GPU={num39 - num29:F1} dB, calOffset={num29:F2}");
			}
		}
		return array5;
	}

	private static float CalculateMedian(float[] data, int length)
	{
		if (data == null || length <= 0)
		{
			return -200f;
		}
		if (_gpuMedianBuffer == null || _gpuMedianBuffer.Length < length)
		{
			_gpuMedianBuffer = new float[length];
		}
		Array.Copy(data, _gpuMedianBuffer, length);
		int num = length / 2;
		int num2 = 0;
		int num3 = length - 1;
		while (num2 < num3)
		{
			int num4 = PartitionMedian(_gpuMedianBuffer, num2, num3);
			if (num4 < num)
			{
				num2 = num4 + 1;
				continue;
			}
			if (num4 <= num)
			{
				break;
			}
			num3 = num4 - 1;
		}
		if ((length & 1) == 1)
		{
			return _gpuMedianBuffer[num];
		}
		float num5 = _gpuMedianBuffer[num - 1];
		float num6 = _gpuMedianBuffer[num];
		if (num5 > num6)
		{
			float num7 = num5;
			num5 = num6;
			num6 = num7;
		}
		return (num5 + num6) * 0.5f;
	}

	private static int PartitionMedian(float[] arr, int lo, int hi)
	{
		float num = arr[hi];
		int num2 = lo;
		for (int i = lo; i < hi; i++)
		{
			if (arr[i] <= num)
			{
				float num3 = arr[num2];
				arr[num2] = arr[i];
				arr[i] = num3;
				num2++;
			}
		}
		float num4 = arr[num2];
		arr[num2] = arr[hi];
		arr[hi] = num4;
		return num2;
	}

	private static void LogGPU(string message)
	{
		GPUWaterfallLogger.Log("GPU-DISP", message);
	}

	public static void ShutdownDX2D()
	{
		lock (_objDX2Lock)
		{
			if (!_bDX2Setup)
			{
				return;
			}
			try
			{
				if (_device != null && _device.ImmediateContext != null)
				{
					_device.ImmediateContext.ClearState();
					_device.ImmediateContext.Flush();
				}
				releaseFonts();
				releaseDX2Resources();
				if (_bitmapBackground != null)
				{
					Utilities.Dispose(ref _bitmapBackground);
				}
				santaCleanUp();
				Utilities.Dispose(ref _waterfallGPU1);
				Utilities.Dispose(ref _waterfallGPU2);
				Utilities.Dispose(ref _gpuFFT1);
				Utilities.Dispose(ref _gpuFFT2);
				_gpuIQbufI = null;
				_gpuIQbufQ = null;
				_gpuIQaccumI = null;
				_gpuIQaccumQ = null;
				_gpuIQringI[0] = null;
				_gpuIQringI[1] = null;
				_gpuIQringQ[0] = null;
				_gpuIQringQ[1] = null;
				_gpuCalReferenceRowRX1 = null;
				_gpuCalReferenceRowRX2 = null;
				_gpuFirstFillDone[0] = false;
				_gpuFirstFillDone[1] = false;
				_gpuSampleCredit[0] = 0;
				_gpuSampleCredit[1] = 0;
				Utilities.Dispose(ref _waterfall_bmp_dx2d);
				Utilities.Dispose(ref _waterfall_bmp2_dx2d);
				if (_pause_bitmap != null)
				{
					Utilities.Dispose(ref _pause_bitmap);
					_pause_bitmap = null;
				}
				if (_d2dRenderTarget is SharpDX.Direct2D1.DeviceContext deviceContext)
				{
					deviceContext.Target = null;
				}
				Utilities.Dispose(ref _d2dTargetBitmap);
				_gpuEffectsEnabled = false;
				WaterfallEffect.Reset();
				Utilities.Dispose(ref _d2dRenderTarget);
				Utilities.Dispose(ref _d2dDevice);
				Utilities.Dispose(ref _swapChain1);
				Utilities.Dispose(ref _swapChain);
				Utilities.Dispose(ref _surface);
				Utilities.Dispose(ref _d2dFactory);
				Utilities.Dispose(ref _factory1);
				_bitmapBackground = null;
				_waterfall_bmp_dx2d = null;
				_waterfall_bmp2_dx2d = null;
				_d2dTargetBitmap = null;
				_d2dRenderTarget = null;
				_d2dDevice = null;
				_swapChain1 = null;
				_swapChain = null;
				_surface = null;
				_d2dFactory = null;
				_factory1 = null;
				if (_device != null && _device.ImmediateContext != null)
				{
					SharpDX.Direct3D11.DeviceContext comObject = _device.ImmediateContext;
					Utilities.Dispose(ref comObject);
					comObject = null;
				}
				DeviceDebug comObject2 = null;
				if (_device != null && !string.IsNullOrEmpty(_device.DebugName))
				{
					comObject2 = new DeviceDebug(_device);
					comObject2.ReportLiveDeviceObjects(ReportingLevel.Detail);
				}
				if (comObject2 != null)
				{
					Utilities.Dispose(ref comObject2);
					comObject2 = null;
				}
				Utilities.Dispose(ref _device);
				_device = null;
				_bDX2Setup = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem Shutting Down DirectX !" + Environment.NewLine + Environment.NewLine + "[" + ex.ToString() + "]", "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
		}
	}

	private static void WaitForGPUIdle()
	{
		try
		{
			if (_device == null || _device.ImmediateContext == null)
			{
				return;
			}
			using Query query = new Query(_device, new QueryDescription
			{
				Type = QueryType.Event,
				Flags = QueryFlags.None
			});
			_device.ImmediateContext.End(query);
			bool result = false;
			for (int i = 0; i < 1000; i++)
			{
				if (_device.ImmediateContext.GetData((Asynchronous)query, out result) & result)
				{
					break;
				}
				Thread.Sleep(1);
			}
		}
		catch
		{
		}
	}

	public static bool RebuildForColorDepth()
	{
		if (displayTarget == null)
		{
			return false;
		}
		WaterfallEnhancer.ColorDepth depth = WaterfallEnhancer.Depth;
		_rebuildingColorDepth = true;
		bool detachedActive = SharedWaterfallState.DetachedActive;
		if (detachedActive && console != null)
		{
			try
			{
				console.BeginInvoke((MethodInvoker)delegate
				{
					console.DetachPanafall(enable: false);
				}).AsyncWaitHandle.WaitOne(2000, exitContext: false);
			}
			catch
			{
			}
			Thread.Sleep(200);
		}
		try
		{
			if (console != null)
			{
				console._pause_DisplayThread = true;
				int num = 0;
				while (_renderingFrame && num < 500 && !_frameCompleteEvent.WaitOne(10, exitContext: false))
				{
					num += 10;
				}
			}
			WaitForGPUIdle();
			WaterfallPixelWriter.UpdateFormat();
			bool flag = false;
			if (_bDX2Setup)
			{
				if (ResizeDX2DForFormat(WaterfallPixelWriter.DxgiFormat, out var error))
				{
					ResetWaterfallBmp();
					ResetWaterfallBmp2();
					buildDX2Resources();
					buildFontsDX2D();
					SetDX2BackgoundImage(console.PnlDisplayBackgroundImage);
					if (detachedActive && console != null)
					{
						try
						{
							console.BeginInvoke((MethodInvoker)delegate
							{
								if (!SharedWaterfallState.DetachedActive)
								{
									console.DetachPanafall(enable: true);
								}
							});
						}
						catch
						{
						}
					}
					flag = true;
				}
				else
				{
					LogTool.AddLogEntry("ResizeDX2DForFormat failed, falling back to full rebuild: " + error, "DX2D");
				}
			}
			if (!flag)
			{
				ShutdownDX2D();
				try
				{
					initDX2D(DriverType.Hardware, _display_adaptor);
				}
				catch (Exception ex)
				{
					LogTool.AddLogEntry("RebuildForColorDepth init failed: " + ex.Message, "DX2D");
				}
				if (!_bDX2Setup)
				{
					if (depth != WaterfallEnhancer.ColorDepth.Bit8)
					{
						WaterfallEnhancer.SetColorDepth(WaterfallEnhancer.ColorDepth.Bit8);
						WaterfallPixelWriter.UpdateFormat();
						try
						{
							initDX2D(DriverType.Hardware, _display_adaptor);
						}
						catch (Exception ex2)
						{
							LogTool.AddLogEntry("RebuildForColorDepth 8-bit fallback failed: " + ex2.Message, "DX2D");
						}
					}
					return _bDX2Setup;
				}
				if (detachedActive && _bDX2Setup && console != null)
				{
					try
					{
						console.BeginInvoke((MethodInvoker)delegate
						{
							if (!SharedWaterfallState.DetachedActive)
							{
								console.DetachPanafall(enable: true);
							}
						});
					}
					catch
					{
					}
				}
			}
			return true;
		}
		finally
		{
			if (console != null)
			{
				console._pause_DisplayThread = false;
			}
			_rebuildingColorDepth = false;
		}
	}

	public static AdaptorInfo[] DX2Adaptors()
	{
		SharpDX.DXGI.Factory1 comObject = new SharpDX.DXGI.Factory1();
		int adapterCount = comObject.GetAdapterCount();
		List<AdaptorInfo> list = new List<AdaptorInfo>(adapterCount);
		for (int i = 0; i < adapterCount; i++)
		{
			Adapter comObject2 = comObject.GetAdapter(i);
			Adapter1 comObject3 = comObject2.QueryInterface<Adapter1>();
			AdapterDescription1 description = comObject3.Description1;
			bool isHardware = (description.Flags & AdapterFlags.Software) == 0;
			bool isDisplayAttached = comObject3.GetOutputCount() > 0;
			AdaptorInfo item = new AdaptorInfo
			{
				Description = description.Description,
				IsHardware = isHardware,
				IsDefaultHardware = false,
				IsDisplayAttached = isDisplayAttached,
				VendorId = description.VendorId,
				DeviceId = description.DeviceId,
				DedicatedVideoMemory = description.DedicatedVideoMemory,
				DedicatedSystemMemory = description.DedicatedSystemMemory,
				SharedSystemMemory = description.SharedSystemMemory,
				AdapterLuid = description.Luid
			};
			list.Add(item);
			Utilities.Dispose(ref comObject3);
			Utilities.Dispose(ref comObject2);
		}
		Utilities.Dispose(ref comObject);
		List<AdaptorInfo> list2 = new List<AdaptorInfo>(list.Count);
		HashSet<long> hashSet = new HashSet<long>();
		for (int j = 0; j < list.Count; j++)
		{
			AdaptorInfo adaptorInfo = list[j];
			if (!hashSet.Contains(adaptorInfo.AdapterLuid))
			{
				list2.Add(adaptorInfo);
				hashSet.Add(adaptorInfo.AdapterLuid);
			}
		}
		for (int k = 0; k < list2.Count; k++)
		{
			if (list2[k].IsHardware)
			{
				list2[k].IsDefaultHardware = true;
				break;
			}
		}
		return list2.ToArray();
	}

	private static string getGPUNameInUse()
	{
		lock (_objDX2Lock)
		{
			if (_bDX2Setup)
			{
				SharpDX.DXGI.Device comObject = _device.QueryInterface<SharpDX.DXGI.Device>();
				Adapter comObject2 = comObject.Adapter;
				string description = comObject2.Description.Description;
				Utilities.Dispose(ref comObject2);
				Utilities.Dispose(ref comObject);
				return description;
			}
			return "Unkown GPU";
		}
	}

	private static void createD2DRenderTarget()
	{
		if (_d2dRenderTarget is SharpDX.Direct2D1.DeviceContext deviceContext)
		{
			deviceContext.Target = null;
		}
		Utilities.Dispose(ref _d2dTargetBitmap);
		Utilities.Dispose(ref _d2dRenderTarget);
		_d2dTargetBitmap = null;
		_d2dRenderTarget = null;
		Format format = Format.Unknown;
		try
		{
			format = _surface.Description.Format;
		}
		catch
		{
		}
		try
		{
			if (_d2dDevice == null)
			{
				SharpDX.DXGI.Device comObject = _device.QueryInterface<SharpDX.DXGI.Device>();
				try
				{
					_d2dDevice = new SharpDX.Direct2D1.Device(_d2dFactory, comObject);
				}
				finally
				{
					Utilities.Dispose(ref comObject);
				}
			}
			SharpDX.Direct2D1.DeviceContext deviceContext2 = (SharpDX.Direct2D1.DeviceContext)(_d2dRenderTarget = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, DeviceContextOptions.None));
			BitmapProperties1 bitmapProperties = new BitmapProperties1(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore), 96f, 96f, BitmapOptions.Target | BitmapOptions.CannotDraw);
			_d2dTargetBitmap = new Bitmap1(deviceContext2, _surface, bitmapProperties);
			deviceContext2.Target = _d2dTargetBitmap;
			deviceContext2.DotsPerInch = new Size2F(96f, 96f);
			_waterfallGPU1?.UpdateDeviceContext(deviceContext2);
			_waterfallGPU2?.UpdateDeviceContext(deviceContext2);
			WaterfallEffect.Reset();
		}
		catch (Exception ex)
		{
			LogTool.AddLogEntry("DeviceContext failed, falling back to RenderTarget: " + ex.Message, "DX2D");
			LogGPU($"createD2DRenderTarget fallback: Depth={WaterfallEnhancer.Depth}, PixelSize={WaterfallPixelWriter.PixelSize}, Format={format}, backBufferFmt={_surface?.Description.Format}");
			WaterfallEnhancer.SetColorDepth(WaterfallEnhancer.ColorDepth.Bit8);
			WaterfallPixelWriter.UpdateFormat();
			Utilities.Dispose(ref _d2dTargetBitmap);
			Utilities.Dispose(ref _d2dRenderTarget);
			_d2dTargetBitmap = null;
			_d2dRenderTarget = null;
			Format format2 = format;
			if (format2 == Format.Unknown)
			{
				format2 = Format.B8G8R8A8_UNorm;
			}
			_d2dRenderTarget = new RenderTarget(properties: new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(format2, BitmapAlphaModeForFormat(format2))), factory: _d2dFactory, dxgiSurface: _surface);
		}
	}

	private static void initDX2D(DriverType driverType = DriverType.Hardware, AdaptorInfo adaptorInfo = null)
	{
		lock (_objDX2Lock)
		{
			if (_bDX2Setup || displayTarget == null)
			{
				return;
			}
			try
			{
				DeviceCreationFlags deviceCreationFlags = DeviceCreationFlags.None;
				int major = Environment.OSVersion.Version.Major;
				int minor = Environment.OSVersion.Version.Minor;
				SharpDX.Direct3D.FeatureLevel[] featureLevels;
				if (major >= 10)
				{
					featureLevels = new SharpDX.Direct3D.FeatureLevel[9]
					{
						SharpDX.Direct3D.FeatureLevel.Level_12_1,
						SharpDX.Direct3D.FeatureLevel.Level_12_0,
						SharpDX.Direct3D.FeatureLevel.Level_11_1,
						SharpDX.Direct3D.FeatureLevel.Level_11_0,
						SharpDX.Direct3D.FeatureLevel.Level_10_1,
						SharpDX.Direct3D.FeatureLevel.Level_10_0,
						SharpDX.Direct3D.FeatureLevel.Level_9_3,
						SharpDX.Direct3D.FeatureLevel.Level_9_2,
						SharpDX.Direct3D.FeatureLevel.Level_9_1
					};
					_NoVSYNCpresentFlag = PresentFlags.DoNotWait;
				}
				else if (major == 6 && minor >= 2)
				{
					featureLevels = new SharpDX.Direct3D.FeatureLevel[7]
					{
						SharpDX.Direct3D.FeatureLevel.Level_11_1,
						SharpDX.Direct3D.FeatureLevel.Level_11_0,
						SharpDX.Direct3D.FeatureLevel.Level_10_1,
						SharpDX.Direct3D.FeatureLevel.Level_10_0,
						SharpDX.Direct3D.FeatureLevel.Level_9_3,
						SharpDX.Direct3D.FeatureLevel.Level_9_2,
						SharpDX.Direct3D.FeatureLevel.Level_9_1
					};
					_NoVSYNCpresentFlag = PresentFlags.DoNotWait;
				}
				else if (major == 6 && minor < 2)
				{
					featureLevels = new SharpDX.Direct3D.FeatureLevel[6]
					{
						SharpDX.Direct3D.FeatureLevel.Level_11_0,
						SharpDX.Direct3D.FeatureLevel.Level_10_1,
						SharpDX.Direct3D.FeatureLevel.Level_10_0,
						SharpDX.Direct3D.FeatureLevel.Level_9_3,
						SharpDX.Direct3D.FeatureLevel.Level_9_2,
						SharpDX.Direct3D.FeatureLevel.Level_9_1
					};
					_NoVSYNCpresentFlag = PresentFlags.None;
				}
				else
				{
					featureLevels = new SharpDX.Direct3D.FeatureLevel[1] { SharpDX.Direct3D.FeatureLevel.Level_9_1 };
					_NoVSYNCpresentFlag = PresentFlags.None;
				}
				_factory1 = new SharpDX.DXGI.Factory1();
				Adapter comObject = null;
				if (adaptorInfo != null)
				{
					int adapterCount = _factory1.GetAdapterCount();
					for (int i = 0; i < adapterCount; i++)
					{
						Adapter comObject2 = _factory1.GetAdapter(i);
						Adapter1 comObject3 = comObject2.QueryInterface<Adapter1>();
						AdapterDescription1 description = comObject3.Description1;
						if (description.VendorId == adaptorInfo.VendorId && description.DeviceId == adaptorInfo.DeviceId)
						{
							comObject = comObject2;
							Utilities.Dispose(ref comObject3);
							break;
						}
						Utilities.Dispose(ref comObject3);
						Utilities.Dispose(ref comObject2);
					}
				}
				if (comObject != null)
				{
					_device = new SharpDX.Direct3D11.Device(comObject, deviceCreationFlags | DeviceCreationFlags.PreventAlteringLayerSettingsFromRegistry | DeviceCreationFlags.BgraSupport, featureLevels);
					Utilities.Dispose(ref comObject);
				}
				else
				{
					_device = new SharpDX.Direct3D11.Device(driverType, deviceCreationFlags | DeviceCreationFlags.PreventAlteringLayerSettingsFromRegistry | DeviceCreationFlags.BgraSupport, featureLevels);
				}
				SharpDX.DXGI.Device1 comObject4 = _device.QueryInterfaceOrNull<SharpDX.DXGI.Device1>();
				if (comObject4 != null)
				{
					comObject4.MaximumFrameLatency = 1;
					Utilities.Dispose(ref comObject4);
					comObject4 = null;
				}
				if (WaterfallPixelWriter.DxgiFormat == Format.R16G16B16A16_Float)
				{
					try
					{
						if ((_device.CheckFormatSupport(Format.R16G16B16A16_Float) & FormatSupport.Display) == 0)
						{
							LogTool.AddLogEntry("R16G16B16A16_Float not supported as display format; will fall back to 8-bit", "DX2D");
							throw new SharpDXException(Result.Fail, "R16G16B16A16_Float not supported as display format");
						}
					}
					catch (Exception ex)
					{
						LogTool.AddLogEntry("CheckFormatSupport(R16G16B16A16_Float) failed: " + ex.Message, "DX2D");
						throw;
					}
				}
				WaterfallPixelWriter.UpdateFormat();
				LogGPU($"initDX2D UpdateFormat: Depth={WaterfallEnhancer.Depth}, PixelSize={WaterfallPixelWriter.PixelSize}, DxgiFormat={WaterfallPixelWriter.DxgiFormat}");
				_allowTearing = false;
				_swapChainFlags = SwapChainFlags.None;
				SharpDX.DXGI.Factory5 comObject5 = _factory1.QueryInterfaceOrNull<SharpDX.DXGI.Factory5>();
				if (comObject5 != null)
				{
					int num = Marshal.SizeOf(typeof(bool));
					IntPtr intPtr = Marshal.AllocHGlobal(num);
					try
					{
						comObject5.CheckFeatureSupport(SharpDX.DXGI.Feature.PresentAllowTearing, intPtr, num);
						_allowTearing = Marshal.ReadInt32(intPtr) == 1;
					}
					finally
					{
						Marshal.FreeHGlobal(intPtr);
						Utilities.Dispose(ref comObject5);
					}
				}
				_swapChainFlags = (_allowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None);
				SharpDX.DXGI.Factory4 comObject6 = _factory1.QueryInterfaceOrNull<SharpDX.DXGI.Factory4>();
				bool flag = false;
				if (comObject6 != null)
				{
					if (!_bUseLegacyBuffers)
					{
						flag = true;
					}
					Utilities.Dispose(ref comObject6);
					comObject6 = null;
				}
				SwapEffect swapEffect = (flag ? SwapEffect.FlipDiscard : SwapEffect.Discard);
				_nBufferCount = ((!flag) ? 1 : 2);
				Format format = ((WaterfallPixelWriter.DxgiFormat == Format.R16G16B16A16_Float) ? Format.B8G8R8A8_UNorm : WaterfallPixelWriter.DxgiFormat);
				ModeDescription modeDescription = new ModeDescription(displayTargetWidth, displayTargetHeight, new Rational(console.DisplayFPS, 1), format);
				modeDescription.ScanlineOrdering = DisplayModeScanlineOrder.Progressive;
				modeDescription.Scaling = DisplayModeScaling.Stretched;
				SwapChainDescription description2 = new SwapChainDescription
				{
					BufferCount = _nBufferCount,
					ModeDescription = modeDescription,
					IsWindowed = true,
					OutputHandle = displayTarget.Handle,
					SampleDescription = new SampleDescription(1, 0),
					SwapEffect = swapEffect,
					Usage = Usage.RenderTargetOutput,
					Flags = _swapChainFlags
				};
				_factory1.MakeWindowAssociation(displayTarget.Handle, WindowAssociationFlags.IgnoreAll);
				try
				{
					_swapChain = new SwapChain(_factory1, _device, description2);
				}
				catch (Exception ex2)
				{
					if (WaterfallPixelWriter.DxgiFormat != Format.R16G16B16A16_Float)
					{
						throw;
					}
					LogTool.AddLogEntry("16-bit flip-model swap chain failed (" + ex2.Message + "); retrying with Discard", "DX2D");
					_allowTearing = false;
					_swapChainFlags = SwapChainFlags.None;
					_nBufferCount = 1;
					description2.BufferCount = 1;
					description2.SwapEffect = SwapEffect.Discard;
					description2.Flags = SwapChainFlags.None;
					_swapChain = new SwapChain(_factory1, _device, description2);
				}
				_swapChain1 = _swapChain.QueryInterface<SwapChain1>();
				_swapChainHwnd = displayTarget.Handle;
				ApplySRGBColorSpaceForFloatSwapChain();
				if (format != WaterfallPixelWriter.DxgiFormat)
				{
					_swapChain1.ResizeBuffers(_nBufferCount, displayTargetWidth, displayTargetHeight, WaterfallPixelWriter.DxgiFormat, _swapChainFlags);
					LogGPU($"Cold start: swap chain resized from {format} to {WaterfallPixelWriter.DxgiFormat} (sRGB color space preserved)");
				}
				ApplySRGBColorSpaceForFloatSwapChain();
				_d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded, DebugLevel.None);
				_surface = _swapChain1.GetBackBuffer<Surface>(0);
				createD2DRenderTarget();
				try
				{
					if (!GPUDetector.HasDeviceContext && _d2dRenderTarget is SharpDX.Direct2D1.DeviceContext dc)
					{
						GPUDetector.Detect(dc, _d2dFactory);
					}
					if (_autoEnableGPU && !_gpuEffectsEnabled && GPUDetector.HasBuiltInEffects)
					{
						_gpuEffectsEnabled = true;
						LogTool.AddLogEntry("GPU post-processing enabled (Auto): Level " + (int)GPUDetector.Level, "D2D");
					}
				}
				catch (Exception ex3)
				{
					LogTool.AddLogEntry("GPUDetector: " + ex3.Message, "D2D");
				}
				if (deviceCreationFlags == DeviceCreationFlags.Debug)
				{
					_device.DebugName = "DeviceDB";
					_swapChain.DebugName = "SwapChainDB";
					_swapChain1.DebugName = "SwapChain1DB";
					_surface.DebugName = "SurfaceDB";
				}
				else
				{
					_device.DebugName = "";
				}
				_bDX2Setup = true;
				_gpu = getGPUNameInUse();
				setupAliasing();
				ResetWaterfallBmp();
				ResetWaterfallBmp2();
				buildDX2Resources();
				buildFontsDX2D();
				SetDX2BackgoundImage(console.PnlDisplayBackgroundImage);
			}
			catch (Exception ex4)
			{
				ShutdownDX2D();
				if (!_rebuildingColorDepth)
				{
					MessageBox.Show("Problem initialising DirectX !" + Environment.NewLine + Environment.NewLine + "[" + ex4.ToString() + "]", "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				}
				else
				{
					LogTool.AddLogEntry("initDX2D failed during color-depth rebuild: " + ex4.Message, "DX2D");
				}
			}
		}
	}

	public static int DXVersion()
	{
		lock (_objDX2Lock)
		{
			if (!_bDX2Setup)
			{
				return -1;
			}
			try
			{
				switch (_device.FeatureLevel)
				{
				case SharpDX.Direct3D.FeatureLevel.Level_9_1:
					return 91;
				case SharpDX.Direct3D.FeatureLevel.Level_9_2:
					return 92;
				case SharpDX.Direct3D.FeatureLevel.Level_9_3:
					return 93;
				case SharpDX.Direct3D.FeatureLevel.Level_10_0:
					return 100;
				case SharpDX.Direct3D.FeatureLevel.Level_10_1:
					return 101;
				case SharpDX.Direct3D.FeatureLevel.Level_11_0:
					return 110;
				case SharpDX.Direct3D.FeatureLevel.Level_11_1:
					return 111;
				case SharpDX.Direct3D.FeatureLevel.Level_12_0:
					return 120;
				case SharpDX.Direct3D.FeatureLevel.Level_12_1:
					return 121;
				}
			}
			catch
			{
			}
			return -1;
		}
	}

	public static void ResetDX2DModeDescription()
	{
		try
		{
			lock (_objDX2Lock)
			{
				if (_bDX2Setup)
				{
					ModeDescription newTargetParametersRef = new ModeDescription(displayTargetWidth, displayTargetHeight, new Rational(console.DisplayFPS, 1), WaterfallPixelWriter.DxgiFormat);
					_swapChain1.ResizeTarget(ref newTargetParametersRef);
					if (!resizeDX2D(out var error))
					{
						ShutdownDX2D();
						MessageBox.Show("Unable to resize DirectX render target (ResetDX2DModeDescription). DirectX has been shut down.\n\n" + error, "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
					}
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static bool resizeDX2D(out string error)
	{
		try
		{
			lock (_objDX2Lock)
			{
				if (!_bDX2Setup)
				{
					error = "DirectX not setup";
					return false;
				}
				if (_d2dRenderTarget is SharpDX.Direct2D1.DeviceContext deviceContext)
				{
					deviceContext.Target = null;
				}
				Utilities.Dispose(ref _d2dTargetBitmap);
				_d2dTargetBitmap = null;
				Utilities.Dispose(ref _d2dRenderTarget);
				Utilities.Dispose(ref _surface);
				_d2dRenderTarget = null;
				_surface = null;
				_device.ImmediateContext.ClearState();
				_device.ImmediateContext.Flush();
				_swapChain1.ResizeBuffers(_nBufferCount, displayTargetWidth, displayTargetHeight, _swapChain.Description.ModeDescription.Format, _swapChainFlags);
				_surface = _swapChain1.GetBackBuffer<Surface>(0);
				createD2DRenderTarget();
				setupAliasing();
				FastAttackNoiseFloorRX1 = true;
				if (RX2Enabled)
				{
					FastAttackNoiseFloorRX2 = true;
				}
				m_stringSizeCache.Clear();
				_stringMeasureKeys.Clear();
				error = "";
				return true;
			}
		}
		catch (Exception ex)
		{
			error = ex.Message;
			error = error + "\n\nDeviceRemovedReason : " + _device.DeviceRemovedReason.ToString();
			return false;
		}
	}

	private static bool recreateSwapChainForHwnd(out string error)
	{
		try
		{
			lock (_objDX2Lock)
			{
				if (!_bDX2Setup)
				{
					error = "DirectX not setup";
					return false;
				}
				if (_d2dRenderTarget is SharpDX.Direct2D1.DeviceContext deviceContext)
				{
					deviceContext.Target = null;
				}
				Utilities.Dispose(ref _d2dTargetBitmap);
				_d2dTargetBitmap = null;
				Utilities.Dispose(ref _d2dRenderTarget);
				Utilities.Dispose(ref _surface);
				_d2dRenderTarget = null;
				_surface = null;
				_device.ImmediateContext.ClearState();
				_device.ImmediateContext.Flush();
				Utilities.Dispose(ref _swapChain1);
				Utilities.Dispose(ref _swapChain);
				Format format = ((WaterfallPixelWriter.DxgiFormat == Format.R16G16B16A16_Float) ? Format.B8G8R8A8_UNorm : WaterfallPixelWriter.DxgiFormat);
				ModeDescription modeDescription = new ModeDescription(displayTargetWidth, displayTargetHeight, new Rational(console.DisplayFPS, 1), format);
				modeDescription.ScanlineOrdering = DisplayModeScanlineOrder.Progressive;
				modeDescription.Scaling = DisplayModeScaling.Stretched;
				SwapChainDescription description = new SwapChainDescription
				{
					BufferCount = _nBufferCount,
					ModeDescription = modeDescription,
					IsWindowed = true,
					OutputHandle = displayTarget.Handle,
					SampleDescription = new SampleDescription(1, 0),
					SwapEffect = ((!_bUseLegacyBuffers) ? SwapEffect.FlipDiscard : SwapEffect.Discard),
					Usage = Usage.RenderTargetOutput,
					Flags = _swapChainFlags
				};
				_factory1.MakeWindowAssociation(displayTarget.Handle, WindowAssociationFlags.IgnoreAll);
				try
				{
					_swapChain = new SwapChain(_factory1, _device, description);
				}
				catch (Exception ex)
				{
					LogTool.AddLogEntry("Swap chain recreate (HWND change) failed (" + ex.Message + "); retrying with Discard", "DX2D");
					_allowTearing = false;
					_swapChainFlags = SwapChainFlags.None;
					_nBufferCount = 1;
					description.BufferCount = 1;
					description.SwapEffect = SwapEffect.Discard;
					description.Flags = SwapChainFlags.None;
					_swapChain = new SwapChain(_factory1, _device, description);
				}
				_swapChain1 = _swapChain.QueryInterface<SwapChain1>();
				_swapChainHwnd = displayTarget.Handle;
				ApplySRGBColorSpaceForFloatSwapChain();
				if (format != WaterfallPixelWriter.DxgiFormat)
				{
					_swapChain1.ResizeBuffers(_nBufferCount, displayTargetWidth, displayTargetHeight, WaterfallPixelWriter.DxgiFormat, _swapChainFlags);
				}
				ApplySRGBColorSpaceForFloatSwapChain();
				_surface = _swapChain1.GetBackBuffer<Surface>(0);
				createD2DRenderTarget();
				setupAliasing();
				FastAttackNoiseFloorRX1 = true;
				if (RX2Enabled)
				{
					FastAttackNoiseFloorRX2 = true;
				}
				m_stringSizeCache.Clear();
				_stringMeasureKeys.Clear();
				LogGPU($"Swap chain recreated for new HWND 0x{_swapChainHwnd.ToInt64():X} ({displayTargetWidth}x{displayTargetHeight})");
				error = "";
				return true;
			}
		}
		catch (Exception ex2)
		{
			error = ex2.Message;
			error = error + "\n\nDeviceRemovedReason : " + _device.DeviceRemovedReason.ToString();
			return false;
		}
	}

	private static void ApplySRGBColorSpaceForFloatSwapChain(bool quiet = false)
	{
		if (WaterfallPixelWriter.DxgiFormat != Format.R16G16B16A16_Float || _swapChain1 == null)
		{
			return;
		}
		SwapChain3 comObject = _swapChain1.QueryInterfaceOrNull<SwapChain3>();
		if (comObject == null)
		{
			if (!quiet)
			{
				LogGPU("SwapChain3 not available; float swap chain keeps default color space");
			}
			return;
		}
		try
		{
			SwapChainColorSpaceSupportFlags swapChainColorSpaceSupportFlags = comObject.CheckColorSpaceSupport(ColorSpaceType.RgbFullG22NoneP709);
			if ((swapChainColorSpaceSupportFlags & SwapChainColorSpaceSupportFlags.Present) != SwapChainColorSpaceSupportFlags.None)
			{
				comObject.ColorSpace1 = ColorSpaceType.RgbFullG22NoneP709;
				if (!quiet)
				{
					LogGPU($"Float swap chain color space set to RGB_FULL_G22_NONE_P709 (sRGB), support flags={swapChainColorSpaceSupportFlags}");
				}
			}
			else if (!quiet)
			{
				LogGPU($"sRGB color space not supported on float swap chain (support flags={swapChainColorSpaceSupportFlags}); background may look washed out");
			}
		}
		catch (Exception ex)
		{
			if (!quiet)
			{
				LogGPU("SetColorSpace1 failed: " + ex.Message);
			}
		}
		finally
		{
			Utilities.Dispose(ref comObject);
		}
	}

	private static bool ResizeDX2DForFormat(Format newFormat, out string error)
	{
		try
		{
			lock (_objDX2Lock)
			{
				if (!_bDX2Setup)
				{
					error = "DirectX not setup";
					return false;
				}
				Utilities.Dispose(ref _waterfallGPU1);
				Utilities.Dispose(ref _waterfallGPU2);
				Utilities.Dispose(ref _waterfall_bmp_dx2d);
				Utilities.Dispose(ref _waterfall_bmp2_dx2d);
				_waterfall_bmp_dx2d = null;
				_waterfall_bmp2_dx2d = null;
				if (_bitmapBackground != null)
				{
					Utilities.Dispose(ref _bitmapBackground);
				}
				_bitmapBackground = null;
				if (_pause_bitmap != null)
				{
					Utilities.Dispose(ref _pause_bitmap);
				}
				_pause_bitmap = null;
				if (_d2dRenderTarget is SharpDX.Direct2D1.DeviceContext deviceContext)
				{
					deviceContext.Target = null;
				}
				Utilities.Dispose(ref _d2dTargetBitmap);
				_d2dTargetBitmap = null;
				Utilities.Dispose(ref _d2dRenderTarget);
				_d2dRenderTarget = null;
				Utilities.Dispose(ref _d2dDevice);
				_d2dDevice = null;
				Utilities.Dispose(ref _d2dFactory);
				_d2dFactory = null;
				Utilities.Dispose(ref _surface);
				_surface = null;
				releaseDX2Resources();
				releaseFonts();
				_device.ImmediateContext.ClearState();
				_device.ImmediateContext.Flush();
				_swapChain1.ResizeBuffers(_nBufferCount, displayTargetWidth, displayTargetHeight, newFormat, _swapChainFlags);
				ApplySRGBColorSpaceForFloatSwapChain();
				_surface = _swapChain1.GetBackBuffer<Surface>(0);
				_d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded, DebugLevel.None);
				createD2DRenderTarget();
				setupAliasing();
				FastAttackNoiseFloorRX1 = true;
				if (RX2Enabled)
				{
					FastAttackNoiseFloorRX2 = true;
				}
				m_stringSizeCache.Clear();
				_stringMeasureKeys.Clear();
				error = "";
				return true;
			}
		}
		catch (Exception ex)
		{
			error = ex.Message;
			if (_device != null)
			{
				error = error + "\n\nDeviceRemovedReason : " + _device.DeviceRemovedReason.ToString();
			}
			return false;
		}
	}

	private static void setupAliasing()
	{
		lock (_objDX2Lock)
		{
			if (_bDX2Setup)
			{
				if (m_bAntiAlias)
				{
					_d2dRenderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
				}
				else
				{
					_d2dRenderTarget.AntialiasMode = AntialiasMode.Aliased;
				}
				_d2dRenderTarget.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Default;
			}
		}
	}

	private static void pauseDisplay()
	{
		if (_pause_bitmap != null)
		{
			Utilities.Dispose(ref _pause_bitmap);
			_pause_bitmap = null;
		}
		if (_paused_display)
		{
			Texture2D comObject = _swapChain1.GetBackBuffer<Texture2D>(0);
			Texture2DDescription description = comObject.Description;
			description.CpuAccessFlags = CpuAccessFlags.Read;
			description.Usage = ResourceUsage.Default;
			description.BindFlags = BindFlags.ShaderResource;
			description.CpuAccessFlags = CpuAccessFlags.None;
			Texture2D comObject2 = new Texture2D(_device, description);
			_device.ImmediateContext.CopyResource(comObject, comObject2);
			Surface comObject3 = comObject2.QueryInterface<Surface>();
			Size2F dotsPerInch = _d2dRenderTarget.DotsPerInch;
			SharpDX.Direct2D1.PixelFormat pixelFormat = new SharpDX.Direct2D1.PixelFormat(WaterfallPixelWriter.DxgiFormat, BitmapAlphaModeForFormat(WaterfallPixelWriter.DxgiFormat));
			_pause_bitmap = new SharpDX.Direct2D1.Bitmap(bitmapProperties: new BitmapProperties(pixelFormat, dotsPerInch.Width, dotsPerInch.Height), renderTarget: _d2dRenderTarget, surface: comObject3);
			Utilities.Dispose(ref comObject3);
			Utilities.Dispose(ref comObject2);
			Utilities.Dispose(ref comObject);
		}
	}

	public static void RenderDX2D()
	{
		try
		{
			lock (_objDX2Lock)
			{
				if (!_bDX2Setup)
				{
					return;
				}
				_renderingFrame = true;
				try
				{
					m_dElapsedFrameStart = _high_perf_timer.ElapsedMsec;
					calcFps();
					_bNoiseFloorAlreadyCalculatedRX1 = false;
					_bNoiseFloorAlreadyCalculatedRX2 = false;
					_wfDrawnRX1ThisFrame = false;
					_wfDrawnRX2ThisFrame = false;
					_d2dRenderTarget.BeginDraw();
					if (_paused_display && _pause_bitmap != null)
					{
						SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(0f, 0f, displayTargetWidth, displayTargetHeight);
						_d2dRenderTarget.DrawBitmap(_pause_bitmap, rectangleF, 1f, BitmapInterpolationMode.Linear);
					}
					else
					{
						Matrix3x2 matrix3x = _d2dRenderTarget.Transform;
						matrix3x.TranslationVector = m_pixelShift;
						_d2dRenderTarget.Transform = matrix3x;
						_d2dRenderTarget.Clear(m_cDX2_display_background_clear_colour);
						SharpDX.RectangleF rectangleF2;
						if (_bitmapBackground != null)
						{
							if (_maintain_background_aspectratio && _bitmapBackground != null)
							{
								float num = _bitmapBackground.PixelSize.Width;
								float num2 = _bitmapBackground.PixelSize.Height;
								float num3 = num / num2;
								float num4 = displayTargetWidth / displayTargetHeight;
								if (num3 > num4)
								{
									float num5 = (float)displayTargetWidth / num3;
									rectangleF2 = new SharpDX.RectangleF(0f, ((float)displayTargetHeight - num5) / 2f, displayTargetWidth, num5);
								}
								else
								{
									float num6 = (float)displayTargetHeight * num3;
									rectangleF2 = new SharpDX.RectangleF(((float)displayTargetWidth - num6) / 2f, 0f, num6, displayTargetHeight);
								}
							}
							else
							{
								rectangleF2 = new SharpDX.RectangleF(0f, 0f, displayTargetWidth, displayTargetHeight);
							}
							_d2dRenderTarget.DrawBitmap(_bitmapBackground, rectangleF2, 1f, BitmapInterpolationMode.Linear);
						}
						else
						{
							rectangleF2 = new SharpDX.RectangleF(0f, 0f, displayTargetWidth, displayTargetHeight);
						}
						_d2dRenderTarget.FillRectangle(rectangleF2, m_bDX2_display_background_brush);
						if (_bRebuildRXLinearGradBrush || _bRebuildTXLinearGradBrush)
						{
							int num7 = displayTargetHeight;
							int num8 = displayTargetHeight;
							if (!split_display)
							{
								switch (current_display_mode)
								{
								case DisplayMode.PANAFALL:
									num7 = (int)((float)num7 * m_fPanafallSplitPerc);
									break;
								case DisplayMode.PANASCOPE:
								case DisplayMode.SPECTRASCOPE:
									num7 /= 2;
									break;
								}
							}
							else
							{
								num7 /= 2;
								num8 /= 2;
								DisplayMode displayMode = current_display_mode;
								if ((uint)(displayMode - 8) <= 2u)
								{
									num7 /= 2;
								}
								displayMode = current_display_mode_bottom;
								if ((uint)(displayMode - 8) <= 2u)
								{
									num8 /= 2;
								}
							}
							if (_bRebuildRXLinearGradBrush)
							{
								buildLinearGradientBrush(0, num7, 1);
								int num9 = 0;
								if (split_display)
								{
									switch (current_display_mode_bottom)
									{
									case DisplayMode.PANADAPTER:
									case DisplayMode.WATERFALL:
										num9 = num8;
										break;
									case DisplayMode.PANAFALL:
										num9 = num8 * 2;
										break;
									}
								}
								buildLinearGradientBrush(num9, num8 + num9, 2);
								_bRebuildRXLinearGradBrush = false;
							}
							if (_bRebuildTXLinearGradBrush)
							{
								buildLinearGradientBrushTX(0, num7, 1);
								int num10 = 0;
								if (split_display)
								{
									switch (current_display_mode_bottom)
									{
									case DisplayMode.PANADAPTER:
									case DisplayMode.WATERFALL:
										num10 = num8;
										break;
									case DisplayMode.PANAFALL:
										num10 = num8 * 2;
										break;
									}
								}
								buildLinearGradientBrushTX(num10, num8 + num10, 2);
								_bRebuildTXLinearGradBrush = false;
							}
						}
						if (!split_display)
						{
							m_nRX1DisplayHeight = displayTargetHeight;
							switch (current_display_mode)
							{
							case DisplayMode.SPECTRUM:
								DrawSpectrumDX2D(1, displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.PANADAPTER:
								DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, bottom: false);
								if (_showTCISpots)
								{
									drawSpots(1, 0, displayTargetWidth, bottom: false);
								}
								break;
							case DisplayMode.SCOPE:
								DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.SCOPE2:
								DrawScope2DX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.PHASE:
								DrawPhaseDX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.PHASE2:
								DrawPhase2DX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.WATERFALL:
								DrawWaterfallDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, bottom: false);
								if (_showTCISpots)
								{
									drawSpots(1, 0, displayTargetWidth, bottom: false);
								}
								break;
							case DisplayMode.HISTOGRAM:
								DrawHistogramDX2D(1, displayTargetWidth, m_nRX1DisplayHeight);
								break;
							case DisplayMode.PANAFALL:
								lock (m_objSplitDisplayLock)
								{
									m_nRX1DisplayHeight = (int)((float)displayTargetHeight * m_fPanafallSplitPerc);
									split_display = PanafallSplitBarPos <= displayTargetHeight / 2;
									DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, bottom: false);
									DrawWaterfallDX2D(PanafallSplitBarPos, displayTargetWidth, displayTargetHeight - m_nRX1DisplayHeight, 1, bottom: true);
									if (_showTCISpots)
									{
										drawSpots(1, 0, displayTargetWidth, bottom: false);
									}
									split_display = false;
								}
								break;
							case DisplayMode.PANASCOPE:
								lock (m_objSplitDisplayLock)
								{
									m_nRX1DisplayHeight = displayTargetHeight / 2;
									split_display = true;
									DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, bottom: false);
									DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: true);
									if (_showTCISpots)
									{
										drawSpots(1, 0, displayTargetWidth, bottom: false);
									}
									split_display = false;
								}
								break;
							case DisplayMode.SPECTRASCOPE:
								lock (m_objSplitDisplayLock)
								{
									m_nRX1DisplayHeight = displayTargetHeight / 2;
									split_display = true;
									DrawSpectrumDX2D(1, displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
									DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: true);
									split_display = false;
								}
								break;
							}
						}
						else
						{
							m_nRX1DisplayHeight = displayTargetHeight / 2;
							switch (current_display_mode)
							{
							case DisplayMode.SPECTRUM:
								DrawSpectrumDX2D(1, displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.SCOPE:
								DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.SCOPE2:
								DrawScope2DX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.PHASE:
								DrawPhaseDX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.PHASE2:
								DrawPhase2DX2D(displayTargetWidth, m_nRX1DisplayHeight, bottom: false);
								break;
							case DisplayMode.PANADAPTER:
								DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, bottom: false);
								if (_showTCISpots)
								{
									drawSpots(1, 0, displayTargetWidth, bottom: false);
								}
								break;
							case DisplayMode.WATERFALL:
								DrawWaterfallDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, bottom: false);
								if (_showTCISpots)
								{
									drawSpots(1, 0, displayTargetWidth, bottom: false);
								}
								break;
							case DisplayMode.HISTOGRAM:
								DrawHistogramDX2D(1, displayTargetWidth, m_nRX1DisplayHeight);
								break;
							case DisplayMode.PANAFALL:
								m_nRX1DisplayHeight = displayTargetHeight / 4;
								DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, bottom: false);
								DrawWaterfallDX2D(m_nRX1DisplayHeight, displayTargetWidth, m_nRX1DisplayHeight, 1, bottom: true);
								if (_showTCISpots)
								{
									drawSpots(1, 0, displayTargetWidth, bottom: false);
								}
								break;
							}
							m_nRX2DisplayHeight = displayTargetHeight / 2;
							switch (current_display_mode_bottom)
							{
							case DisplayMode.PANADAPTER:
								DrawPanadapterDX2D(m_nRX2DisplayHeight, displayTargetWidth, m_nRX2DisplayHeight, 2, bottom: true);
								if (_showTCISpots)
								{
									drawSpots(2, m_nRX2DisplayHeight, displayTargetWidth, bottom: false);
								}
								break;
							case DisplayMode.WATERFALL:
								DrawWaterfallDX2D(m_nRX2DisplayHeight, displayTargetWidth, m_nRX2DisplayHeight, 2, bottom: true);
								if (_showTCISpots)
								{
									drawSpots(2, m_nRX2DisplayHeight, displayTargetWidth, bottom: false);
								}
								break;
							case DisplayMode.PANAFALL:
								m_nRX2DisplayHeight = displayTargetHeight / 4;
								DrawPanadapterDX2D(m_nRX2DisplayHeight * 2, displayTargetWidth, m_nRX2DisplayHeight, 2, bottom: false);
								DrawWaterfallDX2D(m_nRX2DisplayHeight * 3, displayTargetWidth, m_nRX2DisplayHeight, 2, bottom: true);
								if (_showTCISpots)
								{
									drawSpots(2, m_nRX2DisplayHeight * 2, displayTargetWidth, bottom: false);
								}
								break;
							}
						}
						try
						{
							ProduceForDetached();
						}
						catch (Exception ex)
						{
							GPUWaterfallLogger.Log("DetachedProduce", "exception: " + ex.Message);
						}
						if (m_nRX1DisplayHeight != _nOldHeightRX1)
						{
							_nOldHeightRX1 = m_nRX1DisplayHeight;
							_bRebuildRXLinearGradBrush = true;
							_bRebuildTXLinearGradBrush = true;
						}
						if (m_nRX2DisplayHeight != _nOldHeightRX2)
						{
							_nOldHeightRX2 = m_nRX2DisplayHeight;
							_bRebuildRXLinearGradBrush = true;
							_bRebuildTXLinearGradBrush = true;
						}
						if (high_swr || _power_folded_back || _pa_issue)
						{
							if (high_swr || _power_folded_back)
							{
								if (_power_folded_back)
								{
									drawStringDX2D("HIGH SWR\n\nPOWER FOLD BACK", fontDX2d_font14, m_bDX2_Red, 245f, 20f);
								}
								else
								{
									drawStringDX2D("HIGH SWR", fontDX2d_font14, m_bDX2_Red, 245f, 20f);
								}
							}
							if (_pa_issue)
							{
								drawStringDX2D(_pa_state_details, fontDX2d_font14, m_bDX2_Red, 120f, (high_swr || _power_folded_back) ? 40 : 20);
							}
							_d2dRenderTarget.DrawRectangle(new SharpDX.RectangleF(3f, 3f, displayTargetWidth - 6, displayTargetHeight - 6), m_bDX2_Red, 6f);
						}
						if (m_bShowFrameRateIssue && m_bFrameRateIssue)
						{
							_d2dRenderTarget.FillRectangle(new SharpDX.RectangleF(0f, 0f, 8f, 8f), m_bDX2_Red);
						}
						if (m_bShowGetPixelsIssue && (_bGetPixelsIssueRX1 || _bGetPixelsIssueRX2))
						{
							_d2dRenderTarget.FillRectangle(new SharpDX.RectangleF(0f, 8f, 8f, 8f), m_bDX2_Yellow);
						}
						if (m_bShowFPS)
						{
							if (_runningFPSProfile)
							{
								showFPSProfile();
							}
							_d2dRenderTarget.DrawText(m_nFps.ToString(), fontDX2d_callout, new SharpDX.RectangleF(10f, 0f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_m_bTextCallOutActive, DrawTextOptions.None);
						}
						processBlobsActivePeakDisplayDelay();
						if (!string.IsNullOrEmpty(m_sDebugText))
						{
							string[] array = m_sDebugText.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
							int num11 = 32;
							for (int i = 0; i < array.Length; i++)
							{
								_d2dRenderTarget.DrawText(array[i].ToString(), fontDX2d_callout, new SharpDX.RectangleF(64f, num11, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_m_bTextCallOutActive, DrawTextOptions.None);
								num11 += 12;
							}
						}
						DrawCursorInfo(displayTargetWidth);
						if (_snowFall)
						{
							letItSnow();
						}
						_d2dRenderTarget.Transform = Matrix3x2.Identity;
					}
					string error;
					try
					{
						_d2dRenderTarget.EndDraw();
					}
					catch (SharpDXException ex2) when (ex2.ResultCode == SharpDX.Direct2D1.ResultCode.RecreateTarget)
					{
						if (_dx_fail_retry < 10)
						{
							resizeDX2D(out error);
							_dx_fail_retry++;
							Thread.Sleep(50);
							return;
						}
					}
					if (WaterfallPixelWriter.DxgiFormat == Format.R16G16B16A16_Float)
					{
						long num12 = Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency;
						if (num12 - _lastSrgbWatchdogMs > 2000)
						{
							_lastSrgbWatchdogMs = num12;
							ApplySRGBColorSpaceForFloatSwapChain(quiet: true);
						}
					}
					PresentFlags flags = ((m_nVBlanks == 0) ? (_allowTearing ? PresentFlags.AllowTearing : _NoVSYNCpresentFlag) : PresentFlags.None);
					Result result = _swapChain1.TryPresent(m_nVBlanks, flags);
					if (result != Result.Ok && !(result == SharpDX.DXGI.ResultCode.WasStillDrawing) && !(result == 142213121))
					{
						if ((result == SharpDX.DXGI.ResultCode.DeviceRemoved || result == SharpDX.DXGI.ResultCode.DeviceReset) && _dx_fail_retry < 10)
						{
							resizeDX2D(out error);
							_dx_fail_retry++;
							Thread.Sleep(50);
							return;
						}
						string text = "";
						if (result == SharpDX.DXGI.ResultCode.InvalidCall)
						{
							text = "Present Device Invalid Call" + Environment.NewLine + Environment.NewLine + "[ " + result.ToString() + " ]";
						}
						if (result == SharpDX.DXGI.ResultCode.DeviceReset)
						{
							text = "Present Device Reset" + Environment.NewLine + Environment.NewLine + "[ " + result.ToString() + " ]";
						}
						if (result == SharpDX.DXGI.ResultCode.DeviceRemoved)
						{
							text = "Present Device Removed" + Environment.NewLine + Environment.NewLine + "[ " + result.ToString() + " ]";
						}
						if (_dx_fail_retry > 0)
						{
							text = text + "\n\nDirectX failures during present have occurred " + _dx_fail_retry + " times before this.";
						}
						if (!string.IsNullOrEmpty(text))
						{
							throw new Exception(text);
						}
					}
					_dx_fail_retry = 0;
				}
				finally
				{
					_renderingFrame = false;
					_frameCompleteEvent.Set();
				}
			}
		}
		catch (Exception ex3)
		{
			ShutdownDX2D();
			MessageBox.Show("Problem in DirectX Renderer !" + Environment.NewLine + Environment.NewLine + "[ " + ex3.ToString() + " ]", "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
	}

	public static void RenderDetachedDX2D()
	{
		try
		{
			lock (_objDX2Lock)
			{
				if (!_bDX2Setup)
				{
					return;
				}
				DetachedPanafallRenderer pendingDetachRendererForTeardown = _pendingDetachRendererForTeardown;
				if (pendingDetachRendererForTeardown != null)
				{
					_pendingDetachRendererForTeardown = null;
					_detachedRenderer = null;
					try
					{
						pendingDetachRendererForTeardown.Dispose();
					}
					catch (Exception ex)
					{
						GPUWaterfallLogger.Log("DetachedTeardown", "dispose exception: " + ex);
					}
				}
				if (SharedWaterfallState.DetachedActive)
				{
					_detachedRenderer?.Render();
				}
			}
		}
		catch (Exception ex2)
		{
			GPUWaterfallLogger.Log("DetachedRender", "exception: " + ex2);
		}
	}

	public static void SetDetachPanafall(bool enable)
	{
		lock (_objDX2Lock)
		{
			if (enable == _detachPanafallEnabled)
			{
				return;
			}
			if (enable)
			{
				_displayModeBeforeDetach = current_display_mode;
				_detachPanafallEnabled = true;
				SharedWaterfallState.DetachedActive = true;
				try
				{
					ResetGPUWaterfallState(1);
					ResetGPUWaterfallState(2);
				}
				catch
				{
				}
				if (current_display_mode != DisplayMode.PANADAPTER)
				{
					current_display_mode = DisplayMode.PANADAPTER;
				}
			}
			else
			{
				_detachPanafallEnabled = false;
				SharedWaterfallState.DetachedActive = false;
				SharedWaterfallState.ProduceForDetachedFresh = false;
				if (current_display_mode == DisplayMode.PANADAPTER && _displayModeBeforeDetach != DisplayMode.PANADAPTER)
				{
					CurrentDisplayMode = _displayModeBeforeDetach;
				}
			}
		}
	}

	public static void SetDetachedRenderer(DetachedPanafallRenderer renderer)
	{
		if (renderer != null)
		{
			lock (_objDX2Lock)
			{
				_detachedRenderer = renderer;
				_pendingDetachRendererForTeardown = null;
				try
				{
					ResetGPUWaterfallState(1);
					ResetGPUWaterfallState(2);
					return;
				}
				catch
				{
					return;
				}
			}
		}
		_pendingDetachRendererForTeardown = _detachedRenderer;
		_detachedRenderer = null;
	}

	internal static void ShutdownDetachedRenderer()
	{
		GPUWaterfallLogger.Log("DetachedShutdown", "ShutdownDetachedRenderer entry: detached=" + (_detachedRenderer != null) + " pending=" + (_pendingDetachRendererForTeardown != null));
		DetachedPanafallRenderer detachedPanafallRenderer = ((_detachedRenderer != null) ? _detachedRenderer : _pendingDetachRendererForTeardown);
		_detachedRenderer = null;
		_pendingDetachRendererForTeardown = null;
		if (detachedPanafallRenderer == null)
		{
			return;
		}
		try
		{
			lock (_objDX2Lock)
			{
				detachedPanafallRenderer.Dispose();
			}
			GPUWaterfallLogger.Log("DetachedShutdown", "renderer disposed synchronously");
		}
		catch (Exception ex)
		{
			GPUWaterfallLogger.Log("DetachedShutdown", "sync dispose exception: " + ex);
		}
	}

	private static void showFPSProfile()
	{
		if (_high_perf_timer.ElapsedMsec - _last_valid_check >= 5000.0)
		{
			_valid_fps_profile = !console.IsSetupFormNull && console.SetupForm.ValidFpsProfile();
			_last_valid_check = _high_perf_timer.ElapsedMsec;
		}
		RoundedRectangle roundedRect = default(RoundedRectangle);
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(20f, 20f, 300f, _valid_fps_profile ? 170 : 184);
		roundedRect.Rect = rectangleF;
		roundedRect.RadiusX = 14f;
		roundedRect.RadiusY = 14f;
		_d2dRenderTarget.FillRoundedRectangle(roundedRect, m_bDX2_m_bHightlightNumberScale);
		_d2dRenderTarget.DrawRoundedRectangle(roundedRect, m_bDX2_Yellow);
		rectangleF.Inflate(-6f, -6f);
		SharpDX.RectangleF rectangleF2 = new SharpDX.RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
		_d2dRenderTarget.PushAxisAlignedClip(rectangleF2, AntialiasMode.Aliased);
		if (_valid_fps_profile)
		{
			_d2dRenderTarget.DrawText($"{m_nFps}", fontDX2d_fps_profile, new SharpDX.RectangleF(50f, 20f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
		}
		else
		{
			_d2dRenderTarget.DrawText($"{m_nFps}*", fontDX2d_fps_profile, new SharpDX.RectangleF(50f, 20f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
		}
		_d2dRenderTarget.DrawText(_cpu ?? "", fontDX2d_callout, new SharpDX.RectangleF(30f, 104f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
		_d2dRenderTarget.DrawText(_gpu ?? "", fontDX2d_callout, new SharpDX.RectangleF(30f, 118f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
		_d2dRenderTarget.DrawText($"Render Target Dimensions : {displayTargetWidth} x {displayTargetHeight}", fontDX2d_callout, new SharpDX.RectangleF(30f, 132f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
		_d2dRenderTarget.DrawText("Available Physical Ram : " + _ram, fontDX2d_callout, new SharpDX.RectangleF(30f, 146f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
		_d2dRenderTarget.DrawText("Installed Ram : " + _installed_ram, fontDX2d_callout, new SharpDX.RectangleF(30f, 160f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
		if (!_valid_fps_profile)
		{
			_d2dRenderTarget.DrawText("* Settings have deviated from expected !", fontDX2d_callout, new SharpDX.RectangleF(30f, 174f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Red, DrawTextOptions.None);
		}
		if (_fps_profile_data.Count > 0)
		{
			bool flag = m_dElapsedFrameStart - _fps_profile_start >= 10000.0;
			_d2dRenderTarget.DrawText("10 seconds", fontDX2d_callout, new SharpDX.RectangleF(220f, 40f, float.PositiveInfinity, float.PositiveInfinity), flag ? m_bDX2_Yellow : m_bDX2_Gray, DrawTextOptions.None);
			_d2dRenderTarget.DrawText($"Min : {_fps_profile_data.Min()}", fontDX2d_callout, new SharpDX.RectangleF(220f, 58f, float.PositiveInfinity, float.PositiveInfinity), flag ? m_bDX2_Yellow : m_bDX2_Gray, DrawTextOptions.None);
			_d2dRenderTarget.DrawText($"Max : {_fps_profile_data.Max()}", fontDX2d_callout, new SharpDX.RectangleF(220f, 74f, float.PositiveInfinity, float.PositiveInfinity), flag ? m_bDX2_Yellow : m_bDX2_Gray, DrawTextOptions.None);
		}
		_d2dRenderTarget.PopAxisAlignedClip();
	}

	private static void calcFps()
	{
		m_nFrameCount++;
		if (!(m_dElapsedFrameStart >= m_fLastTime + 1000.0))
		{
			return;
		}
		double num = m_dElapsedFrameStart - (m_fLastTime + 1000.0);
		if (num > 2000.0 || num < 0.0)
		{
			num = 0.0;
		}
		m_nFrameCount -= (m_nFps = (int)((double)m_nFrameCount / (1000.0 + num) * 1000.0));
		m_fLastTime = m_dElapsedFrameStart - num;
		if (_runningFPSProfile)
		{
			_fps_profile_data.Add(m_nFps);
			if (_fps_profile_data.Count > 10)
			{
				_fps_profile_data.RemoveAt(0);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int isOccupied(int rx, int nX)
	{
		Maximums[] array = ((rx == 1) ? m_nRX1Maximums : m_nRX2Maximums);
		int result = -1;
		for (int i = 0; i < m_nNumberOfMaximums; i++)
		{
			ref Maximums reference = ref array[i];
			int num = Math.Abs(nX - reference.X);
			if (reference.Enabled && num < 10)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void processMaximums(int rx, float dbm, int nX, int nY)
	{
		Maximums[] array = ((rx == 1) ? m_nRX1Maximums : m_nRX2Maximums);
		int num = isOccupied(rx, nX);
		if (num >= 0)
		{
			ref Maximums reference = ref array[num];
			if (dbm >= reference.max_dBm)
			{
				reference.Enabled = true;
				reference.max_dBm = dbm;
				reference.X = nX;
				reference.MaxY_pixel = nY;
				reference.Time = m_dElapsedFrameStart;
				int num2 = num;
				while (num2 > 0 && array[num2].max_dBm > array[num2 - 1].max_dBm)
				{
					Maximums maximums = array[num2 - 1];
					array[num2 - 1] = array[num2];
					array[num2] = maximums;
					num2--;
				}
			}
			return;
		}
		for (int i = 0; i < m_nNumberOfMaximums; i++)
		{
			ref Maximums reference2 = ref array[i];
			if (dbm > array[i].max_dBm)
			{
				for (int num3 = m_nNumberOfMaximums - 1; num3 > i; num3--)
				{
					ref Maximums reference3 = ref array[num3];
					ref Maximums reference4 = ref array[num3 - 1];
					reference3.Enabled = reference4.Enabled;
					reference3.max_dBm = reference4.max_dBm;
					reference3.X = reference4.X;
					reference3.MaxY_pixel = reference4.MaxY_pixel;
					reference3.Time = reference4.Time;
				}
				reference2.Enabled = true;
				reference2.max_dBm = dbm;
				reference2.X = nX;
				reference2.MaxY_pixel = nY;
				reference2.Time = m_dElapsedFrameStart;
				break;
			}
		}
	}

	public static void ResetSpectrumPeaks(int rx)
	{
		delayBlobsActivePeakDisplay(rx, blobs: false);
		Maximums[] maximums;
		if (rx == 1)
		{
			maximums = m_rx1_spectrumPeaks;
		}
		else
		{
			maximums = m_rx2_spectrumPeaks;
		}
		Parallel.For(0, maximums.Length, delegate(int i)
		{
			maximums[i].max_dBm = float.MinValue;
		});
	}

	public static void ResetBlobMaximums(int rx, bool bClear = false)
	{
		if (bClear)
		{
			delayBlobsActivePeakDisplay(rx, blobs: true);
		}
		Maximums[] array = ((rx != 1) ? m_nRX2Maximums : m_nRX1Maximums);
		int num = (bClear ? array.Length : m_nNumberOfMaximums);
		for (int i = 0; i < num; i++)
		{
			if (bClear || !m_bBlobPeakHold || (m_bBlobPeakHold && !m_bBlobPeakHoldDrop && m_dElapsedFrameStart >= array[i].Time + m_fBlobPeakHoldMS))
			{
				array[i].Enabled = false;
				array[i].max_dBm = float.MinValue;
			}
		}
	}

	private static void getFilterXPositions(int rx, int W, bool local_mox, bool displayduplex, out int filter_left_x, out int filter_right_x)
	{
		int num;
		int num2;
		int num3;
		int num4;
		int num5;
		if (rx == 1)
		{
			num = rx_display_low;
			num2 = rx_display_high;
			num3 = freq_diff;
			num4 = rx1_filter_low;
			num5 = rx1_filter_high;
		}
		else
		{
			num = rx2_display_low;
			num2 = rx2_display_high;
			num3 = rx2_freq_diff;
			num4 = rx2_filter_low;
			num5 = rx2_filter_high;
		}
		if (local_mox)
		{
			if (!displayduplex)
			{
				num = tx_display_low;
				num2 = tx_display_high;
			}
			num4 = tx_filter_low;
			num5 = tx_filter_high;
		}
		int num6 = num2 - num;
		filter_left_x = (int)((float)(num4 - num - num3) / (float)num6 * (float)W);
		filter_right_x = (int)((float)(num5 - num - num3) / (float)num6 * (float)W);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool isRxDuplex(int rx)
	{
		if (rx == 1)
		{
			return display_duplex;
		}
		return false;
	}

	private static void modifyDataForNotches(ref float[] data, int rx, bool bottom, bool local_mox, bool displayduplex, int W)
	{
		int num;
		int num2;
		if (rx == 1)
		{
			if (local_mox)
			{
				if (displayduplex)
				{
					num = rx_display_low;
					num2 = rx_display_high;
				}
				else
				{
					num = tx_display_low;
					num2 = tx_display_high;
				}
			}
			else
			{
				num = rx_display_low;
				num2 = rx_display_high;
			}
		}
		else if (local_mox)
		{
			if (displayduplex)
			{
				num = tx_display_low;
				num2 = tx_display_high;
			}
			else
			{
				num = tx_display_low;
				num2 = tx_display_high;
			}
		}
		else
		{
			num = rx2_display_low;
			num2 = rx2_display_high;
		}
		float num3 = 100f;
		int width = num2 - num;
		List<clsNotchCoords> list = handleNotches(rx, bottom, getCWSideToneShift(rx), num, num2, 0, 0, width, W, 0, bDraw: false);
		int num4 = W / m_nDecimation;
		foreach (clsNotchCoords item in list)
		{
			if (!item._Use)
			{
				continue;
			}
			int val = item._c_x - item._left_x;
			val = Math.Max(1, val);
			for (int num5 = item._c_x; num5 > item._c_x - val; num5--)
			{
				int num6 = num5 / m_nDecimation;
				if (num6 >= 0 && num6 <= num4 - 1)
				{
					int num7 = item._c_x - num5;
					float num8 = 1f / (float)Math.Pow((double)val / (double)(val - num7), 1.5);
					data[num6] -= num3 * num8;
				}
			}
			int val2 = item._right_x - item._c_x;
			val2 = Math.Max(1, val2);
			for (int i = item._c_x; i < item._c_x + val2; i++)
			{
				int num9 = i / m_nDecimation;
				if (num9 >= 0 && num9 <= num4 - 1)
				{
					int num10 = i - item._c_x;
					float num11 = 1f / (float)Math.Pow((double)val2 / (double)(val2 - num10), 1.5);
					data[num9] -= num3 * num11;
				}
			}
		}
	}

	private unsafe static bool DrawPanadapterDX2D(int nVerticalShift, int W, int H, int rx, bool bottom)
	{
		drawPanadapterAndWaterfallGridDX2D(nVerticalShift, W, H, rx, bottom, out var left_edge, out var right_edge);
		float num = float.MinValue;
		float num2 = float.MinValue;
		bool flag = isRxDuplex(rx);
		bool flag2 = localMox(rx);
		int num3 = 0;
		int num4 = 0;
		Maximums[] array = null;
		int num5 = W / m_nDecimation;
		bool flag3 = false;
		bool flag4;
		double num6;
		bool flag5;
		bool flag6;
		bool flag7;
		int num8;
		float[] data;
		float[] array2;
		float num7;
		if (rx == 1)
		{
			flag4 = (!flag2 || _activePeakInTxRX1) && m_bSpectralPeakHoldRX1 && !m_bDelayRX1SpectrumPeaks;
			num6 = m_dSpecralPeakHoldDelayRX1;
			flag5 = m_bPeakBlobMaximums && !m_bDelayRX1Blobs;
			flag6 = (flag2 && _testing_imd && _show_imd_measurements) & flag;
			num7 = m_dBmPerSecondSpectralPeakFallRX1;
			flag7 = m_bActivePeakFillRX1;
			if (flag2)
			{
				num3 = tx_spectrum_grid_max;
				num4 = tx_spectrum_grid_min;
			}
			else
			{
				num3 = spectrum_grid_max;
				num4 = spectrum_grid_min;
			}
			num8 = num3 - num4;
			if (data_ready)
			{
				flag3 = true;
				if (!flag && (flag2 || (_mox && _tx_on_vfob)) && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
				{
					for (int i = 0; i < num5; i++)
					{
						current_display_data[i] = (float)num4 - rx1_display_cal_offset;
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
							Win32.memcpy(destptr, srcptr, num5 * 4);
						}
					}
				}
				fixed (float* ptr = &current_display_data[0])
				{
					void* srcptr2 = ptr;
					fixed (float* ptr2 = &current_display_data_copy[0])
					{
						void* destptr2 = ptr2;
						Win32.memcpy(destptr2, srcptr2, num5 * 4);
					}
				}
				data_ready = false;
			}
			data = current_display_data;
			array2 = current_display_data_copy;
		}
		else
		{
			flag4 = (!flag2 || _activePeakInTxRX2) && m_bSpectralPeakHoldRX2 && !m_bDelayRX2SpectrumPeaks;
			num6 = m_dSpecralPeakHoldDelayRX2;
			flag5 = m_bPeakBlobMaximums && !m_bDelayRX2Blobs;
			flag6 = false;
			num7 = m_dBmPerSecondSpectralPeakFallRX2;
			flag7 = m_bActivePeakFillRX2;
			if (flag2)
			{
				num3 = tx_spectrum_grid_max;
				num4 = tx_spectrum_grid_min;
			}
			else
			{
				num3 = rx2_spectrum_grid_max;
				num4 = rx2_spectrum_grid_min;
			}
			num8 = num3 - num4;
			if (data_ready_bottom)
			{
				flag3 = true;
				if (blank_bottom_display || (flag2 && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU)))
				{
					for (int j = 0; j < num5; j++)
					{
						current_display_data_bottom[j] = (float)num4 - rx2_display_cal_offset;
					}
				}
				else
				{
					fixed (float* ptr = &new_display_data_bottom[0])
					{
						void* srcptr3 = ptr;
						fixed (float* ptr2 = &current_display_data_bottom[0])
						{
							void* destptr3 = ptr2;
							Win32.memcpy(destptr3, srcptr3, num5 * 4);
						}
					}
				}
				fixed (float* ptr = &current_display_data_bottom[0])
				{
					void* srcptr4 = ptr;
					fixed (float* ptr2 = &current_display_data_bottom_copy[0])
					{
						void* destptr4 = ptr2;
						Win32.memcpy(destptr4, srcptr4, num5 * 4);
					}
				}
				data_ready_bottom = false;
			}
			data = current_display_data_bottom;
			array2 = current_display_data_bottom_copy;
		}
		num7 /= (float)m_nFps;
		float num9 = ((rx == 1) ? RX1Offset : RX2Offset);
		SharpDX.Direct2D1.Brush brush;
		SharpDX.Direct2D1.Brush brush2;
		SharpDX.Direct2D1.Brush bDX2_dataPeaks_fill_fpen_brush;
		float width;
		if (flag2)
		{
			if (rx == 1)
			{
				brush = ((m_bUseLinearGradientForDataLineTX && m_bUseLinearGradientTX) ? m_brushLGDataLineTX_RX1 : m_bDX2_data_line_pen_brush_tx);
				brush2 = (m_bUseLinearGradientTX ? m_brushLGDataFillTX_RX1 : m_bDX2_data_fill_fpen_brush_tx);
			}
			else
			{
				brush = ((m_bUseLinearGradientForDataLineTX && m_bUseLinearGradientTX) ? m_brushLGDataLineTX_RX2 : m_bDX2_data_line_pen_brush_tx);
				brush2 = (m_bUseLinearGradientTX ? m_brushLGDataFillTX_RX2 : m_bDX2_data_fill_fpen_brush_tx);
			}
			bDX2_dataPeaks_fill_fpen_brush = m_bDX2_dataPeaks_fill_fpen_brush;
			width = _tx_display_line_width;
		}
		else
		{
			if (rx == 1)
			{
				brush = ((m_bUseLinearGradientForDataLine && m_bUseLinearGradient) ? m_brushLGDataLineRX1 : m_bDX2_data_line_pen_brush);
				brush2 = (m_bUseLinearGradient ? m_brushLGDataFillRX1 : m_bDX2_data_fill_fpen_brush);
			}
			else
			{
				brush = ((m_bUseLinearGradientForDataLine && m_bUseLinearGradient) ? m_brushLGDataLineRX2 : m_bDX2_data_line_pen_brush);
				brush2 = (m_bUseLinearGradient ? m_brushLGDataFillRX2 : m_bDX2_data_fill_fpen_brush);
			}
			bDX2_dataPeaks_fill_fpen_brush = m_bDX2_dataPeaks_fill_fpen_brush;
			width = _display_line_width;
		}
		float num10 = (float)H / (float)num8;
		float num11 = data[0] + num9;
		int num12 = (int)(((float)num3 - num11) * num10 - 0.5f) + nVerticalShift;
		bool flag8 = false;
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		Vector2 vector3 = default(Vector2);
		Vector2 vector4 = new Vector2(0f, nVerticalShift + H);
		Vector2 vector5 = new Vector2(0f, num12);
		int filter_left_x = 0;
		int filter_right_x = 0;
		if (flag5)
		{
			ResetBlobMaximums(rx);
			if (!flag6 && m_bInsideFilterOnly)
			{
				getFilterXPositions(rx, W, flag2, flag, out filter_left_x, out filter_right_x);
			}
		}
		Vector2 vector6 = default(Vector2);
		if (flag4)
		{
			if (rx == 1)
			{
				if (W > m_rx1_spectrumPeaks.Length)
				{
					m_rx1_spectrumPeaks = new Maximums[W];
					ResetSpectrumPeaks(1);
				}
				array = m_rx1_spectrumPeaks;
			}
			else
			{
				if (W > m_rx2_spectrumPeaks.Length)
				{
					m_rx2_spectrumPeaks = new Maximums[W];
					ResetSpectrumPeaks(2);
				}
				array = m_rx2_spectrumPeaks;
			}
			vector6.X = 0f;
			vector6.Y = (int)(((float)num3 - array[0].max_dBm) * num10 - 0.5f);
			if (vector6.Y >= (float)H)
			{
				vector6.Y = H;
			}
			vector6.Y += nVerticalShift;
		}
		float num13 = float.PositiveInfinity;
		float num14 = float.NegativeInfinity;
		int num15 = 0;
		int num16 = 0;
		bool flag9 = true;
		float num17 = 10f;
		System.Drawing.Rectangle r = new System.Drawing.Rectangle(40, 0, 8, 8);
		List<Maximums> list = new List<Maximums>();
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(0f, nVerticalShift, W, H);
		_d2dRenderTarget.PushAxisAlignedClip(rectangleF, AntialiasMode.Aliased);
		if (flag3 && m_bShowVisualNotch && !flag2)
		{
			modifyDataForNotches(ref data, rx, bottom, flag2, flag, W);
		}
		float num18 = 0f;
		int num19 = 1;
		float num20 = ((rx == 1) ? (m_fFFTBinAverageRX1 + 2f) : (m_fFFTBinAverageRX2 + 2f));
		bool flag10 = flag5 | flag6;
		int nDecimation = m_nDecimation;
		double dElapsedFrameStart = m_dElapsedFrameStart;
		for (int k = 0; k < num5; k++)
		{
			vector.X = k * nDecimation;
			num11 = data[k] + num9;
			float num21 = array2[k] + num9;
			if (!flag2 && num21 < num20)
			{
				num18 += fastPow10Raw(num21);
				num19++;
			}
			num12 = (int)(((float)num3 - num11) * num10 - 0.5f) + nVerticalShift;
			vector.Y = num12;
			if (num11 > num)
			{
				num = num11;
				num2 = vector.X;
			}
			if (flag10 && ((!m_bInsideFilterOnly || (vector.X >= (float)filter_left_x && vector.X <= (float)filter_right_x)) | flag6))
			{
				if (num11 > num14)
				{
					num14 = num11;
					num15 = num12;
					num16 = k;
				}
				if (num11 < num13)
				{
					num13 = num11;
				}
				if (flag9)
				{
					if (num11 < num14 - num17)
					{
						if (flag6)
						{
							list.Add(new Maximums
							{
								max_dBm = num14,
								X = num16,
								Enabled = true,
								MaxY_pixel = num15,
								Time = dElapsedFrameStart
							});
						}
						else
						{
							processMaximums(rx, num14, num16, num15);
						}
						num13 = num11;
						flag9 = false;
					}
				}
				else if (num11 > num13 + num17)
				{
					num14 = num11;
					num15 = num12;
					num16 = k;
					flag9 = true;
				}
			}
			if (pan_fill)
			{
				vector4.X = vector.X;
				_d2dRenderTarget.DrawLine(vector4, vector, brush2, sw(nDecimation));
			}
			if (flag4)
			{
				ref Maximums reference = ref array[k];
				if (num11 >= reference.max_dBm)
				{
					reference.max_dBm = num11;
					reference.Time = dElapsedFrameStart;
				}
				if (reference.max_dBm >= num11)
				{
					vector2.X = vector.X;
					vector2.Y = (int)(((float)num3 - reference.max_dBm) * num10 - 0.5f) + nVerticalShift;
					if (flag7)
					{
						_d2dRenderTarget.DrawLine(vector, vector2, bDX2_dataPeaks_fill_fpen_brush, sw(nDecimation));
					}
					else
					{
						_d2dRenderTarget.DrawLine(vector6, vector2, bDX2_dataPeaks_fill_fpen_brush, sw(width));
						vector6 = vector2;
					}
					if (dElapsedFrameStart - reference.Time > num6)
					{
						reference.max_dBm -= num7;
					}
				}
			}
			if (k > 0 && k < num5 - 1 && vector.Y == vector5.Y)
			{
				vector3 = vector;
				flag8 = true;
				continue;
			}
			if (flag8)
			{
				_d2dRenderTarget.DrawLine(vector5, vector3, brush, sw(width));
				vector5 = vector3;
				flag8 = false;
			}
			_d2dRenderTarget.DrawLine(vector5, vector, brush, sw(width));
			vector5 = vector;
		}
		if (!flag2)
		{
			bool bNoiseFloorAlreadyCalculatedRX = _bNoiseFloorAlreadyCalculatedRX1;
			bool bNoiseFloorAlreadyCalculatedRX2 = _bNoiseFloorAlreadyCalculatedRX2;
			processNoiseFloor(rx, num19, num18, num5, waterfall: false);
			float dB;
			bool flag11;
			int num22;
			int num23;
			if (rx == 1)
			{
				if (!m_bFastAttackNoiseFloorRX1 && !bNoiseFloorAlreadyCalculatedRX)
				{
					m_fNoiseFloorRX1 = m_fLerpAverageRX1 + _fNFshiftDBM;
					m_bNoiseFloorGoodRX1 = true;
				}
				dB = m_fLerpAverageRX1 + _fNFshiftDBM;
				num22 = (int)dBToPixel(dB, H);
				num23 = (int)dBToPixel(m_fFFTBinAverageRX1 + _fNFshiftDBM, H);
				flag11 = m_bShowRX1NoiseFloor;
			}
			else
			{
				if (!m_bFastAttackNoiseFloorRX2 && !bNoiseFloorAlreadyCalculatedRX2)
				{
					m_fNoiseFloorRX2 = m_fLerpAverageRX2 + _fNFshiftDBM;
					m_bNoiseFloorGoodRX2 = true;
				}
				dB = m_fLerpAverageRX2 + _fNFshiftDBM;
				num22 = (int)dBToRX2Pixel(dB, H);
				num23 = (int)dBToRX2Pixel(m_fFFTBinAverageRX2 + _fNFshiftDBM, H);
				flag11 = m_bShowRX2NoiseFloor;
			}
			if (flag11)
			{
				num22 += nVerticalShift;
				bool num24 = ((rx == 1) ? m_bFastAttackNoiseFloorRX1 : m_bFastAttackNoiseFloorRX2);
				num23 += nVerticalShift;
				SharpDX.Direct2D1.Brush b = (num24 ? m_bDX2_Gray : m_bDX2_noisefloor);
				SharpDX.Direct2D1.Brush b2 = (num24 ? m_bDX2_Gray : m_bDX2_noisefloor_text);
				int num25 = num22;
				r.Y = num25 - 8;
				drawFillRectangleDX2D(b, r);
				drawLineDX2D(b, 40f, num25, W - 40, num25, m_styleDots, m_fNoiseFloorLineWidth);
				if (m_bShowNoiseFloorDBM)
				{
					drawLineDX2D(b, r.X - 3, num23, r.X - 3, num25, 2f);
					drawStringDX2D(dB.ToString(_NFDecimal ? "F1" : "F0"), fontDX2d_font9b, b2, r.X + r.Width, r.Y - 6);
				}
				else
				{
					drawStringDX2D("-NF", fontDX2d_panafont, b2, r.X + r.Width, r.Y - 4);
				}
			}
		}
		if (flag5 | flag6)
		{
			Maximums[] array3 = ((!flag6) ? ((rx == 1) ? m_nRX1Maximums : m_nRX2Maximums) : list.OrderByDescending((Maximums m) => m.max_dBm).Take(20).ToArray());
			int num26 = (flag6 ? array3.Length : m_nNumberOfMaximums);
			bool flag12 = m_bBlobPeakHold && m_bBlobPeakHoldDrop;
			for (int num27 = 0; num27 < num26; num27++)
			{
				ref Maximums reference2 = ref array3[num27];
				if (!reference2.Enabled)
				{
					continue;
				}
				if (flag12)
				{
					double num28 = dElapsedFrameStart - reference2.Time;
					if ((double)reference2.max_dBm > -200.0 && num28 > m_fBlobPeakHoldMS)
					{
						reference2.max_dBm -= m_dBmPerSecondPeakBlobFall / (float)m_nFps;
						reference2.MaxY_pixel = (int)(((float)num3 - reference2.max_dBm) * num10 - 0.5f);
					}
					else if ((double)reference2.max_dBm <= -200.0)
					{
						reference2.Enabled = false;
						reference2.max_dBm = float.MinValue;
					}
				}
				m_objEllipse.Point.X = reference2.X * nDecimation;
				m_objEllipse.Point.Y = reference2.MaxY_pixel;
				string text = ((rx != 1) ? ((m_bShowRX2NoiseFloor && !flag2) ? (" (" + (reference2.max_dBm - m_fLerpAverageRX2).ToString("f1") + ")") : "") : ((m_bShowRX1NoiseFloor && !flag2) ? (" (" + (reference2.max_dBm - m_fLerpAverageRX1).ToString("f1") + ")") : ""));
				_d2dRenderTarget.DrawEllipse(m_objEllipse, m_bDX2_PeakBlob);
				_d2dRenderTarget.DrawText(reference2.max_dBm.ToString("f1") + text, fontDX2d_callout, new SharpDX.RectangleF(m_objEllipse.Point.X + 6f, m_objEllipse.Point.Y - 8f, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_PeakBlobText, DrawTextOptions.None);
			}
			if (flag6)
			{
				Maximums[] array4 = list.OrderByDescending((Maximums item) => item.max_dBm).ToArray();
				if (array4.Length >= 2)
				{
					RoundedRectangle roundedRect = new RoundedRectangle
					{
						Rect = new SharpDX.RectangleF(_two_tone_readings_X_offset, 50f, 260f, 180f),
						RadiusX = 14f,
						RadiusY = 14f
					};
					_d2dRenderTarget.FillRoundedRectangle(roundedRect, m_bDX2_m_bHightlightNumberScale);
					_d2dRenderTarget.DrawRoundedRectangle(roundedRect, m_bDX2_m_bHightlightNumbers);
					int num29 = Math.Abs(array4[0].X - array4[1].X);
					if (num29 > 10)
					{
						int num30 = ((array4[0].X < array4[1].X) ? array4[0].X : array4[1].X);
						int offset = ((array4[0].X > array4[1].X) ? array4[0].X : array4[1].X);
						int mid_x = num30 + num29 / 2;
						Maximums[] array5 = (from m in list
							orderby m.X descending
							where m.X < mid_x
							select m).ToArray();
						Maximums[] array6 = (from m in array4
							orderby m.X
							where m.X > mid_x
							select m).ToArray();
						int num31 = findImd(array5, 1, num29, num30, low: true, out var X);
						int num32 = findImd(array6, 1, num29, offset, low: false, out var X2);
						int num33 = findImd(array5, 3, num29, num30, low: true, out var X3);
						int num34 = findImd(array6, 3, num29, offset, low: false, out var X4);
						int num35 = findImd(array5, 5, num29, num30, low: true, out var X5);
						int num36 = findImd(array6, 5, num29, offset, low: false, out var X6);
						if (num31 != -1 && num32 != -1 && num33 != -1 && num34 != -1 && num35 != -1 && num36 != -1)
						{
							float[] array7 = new float[2]
							{
								array5[num31].max_dBm,
								array6[num32].max_dBm
							};
							float[] array8 = new float[2]
							{
								array5[num33].max_dBm,
								array6[num34].max_dBm
							};
							float[] array9 = new float[2]
							{
								array5[num35].max_dBm,
								array6[num36].max_dBm
							};
							long num37 = (long)(m_dCentreFreqRX1 * 1000000.0) + left_edge;
							double num38 = (double)((long)(m_dCentreFreqRX1 * 1000000.0) + right_edge - num37) / (double)W;
							long num39 = num37 + (long)((double)X * num38);
							long num40 = num37 + (long)((double)X2 * num38);
							long num41 = num37 + (long)((double)X3 * num38);
							long num42 = num37 + (long)((double)X4 * num38);
							long num43 = num37 + (long)((double)X5 * num38);
							long num44 = num37 + (long)((double)X6 * num38);
							float num45 = Math.Max(array7[0], array7[1]);
							float num46 = Math.Min(array7[0], array7[1]);
							float num47 = Math.Max(array8[0], array8[1]);
							float num48 = Math.Max(array9[0], array9[1]);
							float num49 = num46 - num47;
							float num50 = num46 - num48;
							float num51 = num46 + num49 / 2f;
							float num52 = num46 + num50 / 2f;
							_two_tone_readings_X_offset = X5 - (int)(roundedRect.Rect.Right - roundedRect.Rect.Left) - num29;
							if (_two_tone_readings_X_offset < 50)
							{
								_two_tone_readings_X_offset = 50;
							}
							if (_ema_dbc == -999f)
							{
								_ema_dbc = num45;
								_ema_f0l = array7[0];
								_ema_f0u = array7[1];
								_ema_imd3l = array8[0];
								_ema_imd3u = array8[1];
								_ema_imd5l = array9[0];
								_ema_imd5u = array9[1];
								_ema_f0l_freq = num39;
								_ema_f0h_freq = num40;
								_ema_imd3l_freq = num41;
								_ema_imd3h_freq = num42;
								_ema_imd5l_freq = num43;
								_ema_imd5h_freq = num44;
								_ema_imd3dBc = num49;
								_ema_imd5dBc = num50;
								_ema_oip3 = num51;
								_ema_oip5 = num52;
							}
							else
							{
								float num53 = 0.1f;
								_ema_dbc = num53 * num45 + (1f - num53) * _ema_dbc;
								_ema_f0l = num53 * array7[0] + (1f - num53) * _ema_f0l;
								_ema_f0u = num53 * array7[1] + (1f - num53) * _ema_f0u;
								_ema_imd3l = num53 * array8[0] + (1f - num53) * _ema_imd3l;
								_ema_imd3u = num53 * array8[1] + (1f - num53) * _ema_imd3u;
								_ema_imd5l = num53 * array9[0] + (1f - num53) * _ema_imd5l;
								_ema_imd5u = num53 * array9[1] + (1f - num53) * _ema_imd5u;
								_ema_f0l_freq = num53 * (float)num39 + (1f - num53) * _ema_f0l_freq;
								_ema_f0h_freq = num53 * (float)num40 + (1f - num53) * _ema_f0h_freq;
								_ema_imd3l_freq = num53 * (float)num41 + (1f - num53) * _ema_imd3l_freq;
								_ema_imd3h_freq = num53 * (float)num42 + (1f - num53) * _ema_imd3h_freq;
								_ema_imd5l_freq = num53 * (float)num43 + (1f - num53) * _ema_imd5l_freq;
								_ema_imd5h_freq = num53 * (float)num44 + (1f - num53) * _ema_imd5h_freq;
								_ema_imd3dBc = num53 * num49 + (1f - num53) * _ema_imd3dBc;
								_ema_imd5dBc = num53 * num50 + (1f - num53) * _ema_imd5dBc;
								_ema_oip3 = num53 * num51 + (1f - num53) * _ema_oip3;
								_ema_oip5 = num53 * num52 + (1f - num53) * _ema_oip5;
							}
							float num54 = 0f - (_ema_dbc - _ema_f0l);
							float num55 = 0f - (_ema_dbc - _ema_f0u);
							float num56 = 0f - (_ema_dbc - _ema_imd3l);
							float num57 = 0f - (_ema_dbc - _ema_imd3u);
							float num58 = 0f - (_ema_dbc - _ema_imd5l);
							float num59 = 0f - (_ema_dbc - _ema_imd5u);
							float num60 = (float)(num40 - num39) / 1000f;
							float num61 = 0f - _ema_imd3dBc;
							float num62 = 0f - _ema_imd5dBc;
							string text2 = "    f0 L\n    f0 U\nIMD3 L\nIMD3 U\nIMD5 L\nIMD5 U\n\n        IMD3\n        IMD5\n        OIP3\n        OIP5";
							string text3 = _ema_f0l.ToString("f2") + "\n" + _ema_f0u.ToString("f2") + "\n" + _ema_imd3l.ToString("f2") + "\n" + _ema_imd3u.ToString("f2") + "\n" + _ema_imd5l.ToString("f2") + "\n" + _ema_imd5u.ToString("f2") + "\n\n    " + num61.ToString("f2") + " dBc\n    " + num62.ToString("f2") + " dBc\n    " + _ema_oip3.ToString("f2") + " dB\n    " + _ema_oip5.ToString("f2") + " dB";
							string text4 = num54.ToString("f2") + "\n" + num55.ToString("f2") + "\n" + num56.ToString("f2") + "\n" + num57.ToString("f2") + "\n" + num58.ToString("f2") + "\n" + num59.ToString("f2");
							string text5 = ((double)_ema_f0l_freq * 1E-06).ToString("f6") + " MHz\n" + ((double)_ema_f0h_freq * 1E-06).ToString("f6") + " MHz\n" + ((double)_ema_imd3l_freq * 1E-06).ToString("f6") + " MHz\n" + ((double)_ema_imd3h_freq * 1E-06).ToString("f6") + " MHz\n" + ((double)_ema_imd5l_freq * 1E-06).ToString("f6") + " MHz\n" + ((double)_ema_imd5h_freq * 1E-06).ToString("f6") + " MHz\n\n\n  " + num60.ToString("F3") + " kHz";
							_d2dRenderTarget.DrawText("dBm        dBc           frequency", fontDX2d_callout, new SharpDX.RectangleF(_two_tone_readings_X_offset + 70, 54f, 200f, 120f), m_bDX2_PeakBlobText, DrawTextOptions.None);
							_d2dRenderTarget.DrawText(text2, fontDX2d_callout, new SharpDX.RectangleF(_two_tone_readings_X_offset + 10, 70f, 200f, 120f), m_bDX2_PeakBlobText, DrawTextOptions.None);
							_d2dRenderTarget.DrawText(text3, fontDX2d_callout, new SharpDX.RectangleF(_two_tone_readings_X_offset + 64, 70f, 200f, 120f), m_bDX2_PeakBlobText, DrawTextOptions.None);
							_d2dRenderTarget.DrawText(text4, fontDX2d_callout, new SharpDX.RectangleF(_two_tone_readings_X_offset + 114, 70f, 200f, 120f), m_bDX2_PeakBlobText, DrawTextOptions.None);
							_d2dRenderTarget.DrawText(text5, fontDX2d_callout, new SharpDX.RectangleF(_two_tone_readings_X_offset + 170, 70f, 200f, 120f), m_bDX2_PeakBlobText, DrawTextOptions.None);
							_d2dRenderTarget.DrawText("f0 diff", fontDX2d_callout, new SharpDX.RectangleF(_two_tone_readings_X_offset + 190, 166f, 200f, 120f), m_bDX2_PeakBlobText, DrawTextOptions.None);
						}
						else
						{
							_two_tone_readings_X_offset = 50;
							_d2dRenderTarget.DrawText("Peaks not found !\n\nEnsure that IMD3 lower/upper and\nIMD5 lower/upper are in the display.\n\nTry changing FFT windowing method.", fontDX2d_callout, new SharpDX.RectangleF(_two_tone_readings_X_offset + 10, 54f, 200f, 120f), m_bDX2_PeakBlobText, DrawTextOptions.None);
						}
					}
					else
					{
						_two_tone_readings_X_offset = 50;
						_d2dRenderTarget.DrawText("Peaks not found !\n\nTry increasing zoom and/or\nchanging sample rate.\n\nFundamental peak separation needs to be increased.\n\nTry changing FFT windowing method.", fontDX2d_callout, new SharpDX.RectangleF(_two_tone_readings_X_offset + 10, 54f, 200f, 120f), m_bDX2_PeakBlobText, DrawTextOptions.None);
					}
				}
			}
			else if (_ema_dbc != -999f)
			{
				_ema_dbc = -999f;
			}
		}
		_d2dRenderTarget.PopAxisAlignedClip();
		if (!bottom)
		{
			max_y = num;
			max_x = num2;
		}
		return true;
	}

	private static int findImd(Maximums[] sorted, int imd, int pixel_jump, int offset, bool low, out int X)
	{
		int num = (imd - 1) / 2;
		int num2 = ((!low) ? (offset + num * pixel_jump) : (offset - num * pixel_jump));
		int num3 = pixel_jump / 4;
		X = -1;
		int result = -1;
		float num4 = float.MinValue;
		int num5 = int.MaxValue;
		for (int i = 0; i < sorted.Length; i++)
		{
			int num6 = Math.Abs(sorted[i].X - num2);
			if (num6 <= num3 && (sorted[i].max_dBm > num4 || (sorted[i].max_dBm == num4 && num6 < num5)))
			{
				num4 = sorted[i].max_dBm;
				X = sorted[i].X;
				num5 = num6;
				result = i;
			}
		}
		return result;
	}

	private static void processNoiseFloor(int rx, int averageCount, float averageSum, int width, bool waterfall, float[] fullData = null, int dataCount = 0)
	{
		if (rx != 1 && rx != 2)
		{
			return;
		}
		ref bool reference = ref rx == 1 ? ref _bNoiseFloorAlreadyCalculatedRX1 : ref _bNoiseFloorAlreadyCalculatedRX2;
		if (reference)
		{
			return;
		}
		int num2;
		if (waterfall)
		{
			double num = ((rx == 2) ? _wfRowIntervalMsRX2 : _wfRowIntervalMsRX1);
			num2 = ((!(num > 0.0)) ? (m_nFps / ((rx == 2) ? rx2_waterfall_update_period : waterfall_update_period)) : ((int)(1000.0 / num)));
		}
		else
		{
			num2 = m_nFps;
		}
		int num3 = (int)((float)width * ((float)_NFsensitivity / 20f));
		ref bool reference2 = ref rx == 1 ? ref m_bFastAttackNoiseFloorRX1 : ref m_bFastAttackNoiseFloorRX2;
		ref float reference3 = ref rx == 1 ? ref m_fFFTBinAverageRX1 : ref m_fFFTBinAverageRX2;
		ref float reference4 = ref rx == 1 ? ref m_fLerpAverageRX1 : ref m_fLerpAverageRX2;
		ref float reference5 = ref rx == 1 ? ref m_fAttackTimeInMSForRX1 : ref m_fAttackTimeInMSForRX2;
		ref double reference6 = ref rx == 1 ? ref _fLastFastAttackEnabledTimeRX1 : ref _fLastFastAttackEnabledTimeRX2;
		ref float reference7 = ref rx == 1 ? ref _fft_fill_timeRX1 : ref _fft_fill_timeRX2;
		if (_nfMode == NoiseFloorPro.DetectionMode.Percentile && fullData != null && dataCount > 0)
		{
			float[] array = _nfScratch;
			if (array == null || array.Length < dataCount)
			{
				array = (_nfScratch = new float[dataCount]);
			}
			Array.Copy(fullData, 0, array, 0, dataCount);
			NoiseFloorPro.ComputeLowHigh(array, dataCount, _nfLowPct, _nfHighPct, out var lowDbm, out var highDbm);
			float num4 = fastPow10Raw(reference3);
			float num5 = (fastPow10Raw(lowDbm) + num4) * 0.5f;
			reference3 = 10f * (float)Math.Log10((double)num5 + 1E-60);
			if (rx == 1)
			{
				_autoHighRX1 = _autoHighRX1 * 0.85f + highDbm * 0.15f;
			}
			else
			{
				_autoHighRX2 = _autoHighRX2 * 0.85f + highDbm * 0.15f;
			}
		}
		else if (averageCount >= num3)
		{
			float num6 = averageSum / (float)averageCount;
			float num7 = fastPow10Raw(reference3);
			float num8 = (num6 + num7) * 0.5f;
			reference3 = 10f * (float)Math.Log10((double)num8 + 1E-60);
			float num9 = reference3 + 15f;
			if (rx == 1)
			{
				_autoHighRX1 = _autoHighRX1 * 0.85f + num9 * 0.15f;
			}
			else
			{
				_autoHighRX2 = _autoHighRX2 * 0.85f + num9 * 0.15f;
			}
		}
		else
		{
			reference3 += (reference2 ? 3f : 1f);
		}
		reference3 = ((reference3 < -200f) ? (-200f) : ((reference3 > 200f) ? 200f : reference3));
		int num10 = ((!reference2) ? ((int)((double)((float)num2 / 1000f) * (double)reference5)) : 0);
		num10++;
		float num11 = reference4 - reference3;
		reference4 -= num11 / (float)num10;
		if (reference2)
		{
			float num12 = Math.Max(1000f, reference7 + (_wdsp_mox_transition_buffer_clear ? reference7 : 0f));
			if (_high_perf_timer.ElapsedMsec - reference6 > (double)num12)
			{
				reference2 = false;
			}
		}
		reference = true;
	}

	private static void resetWaterfallTimeOverlay(int rx)
	{
		int num = rx - 1;
		if (num >= 0 && num < _waterfallRowUtcTicks.Length)
		{
			_waterfallRowUtcTicks[num] = new long[_waterfallRowUtcTicks[num].Length];
			_waterfallRowLabelUtcTicks[num] = new long[_waterfallRowLabelUtcTicks[num].Length];
			_waterfallRowLabelIntervalMs[num] = new long[_waterfallRowLabelIntervalMs[num].Length];
			_waterfallRowTimeCounts[num] = 0;
			_waterfallRowsSinceLastLabel[num] = int.MaxValue;
			_waterfallLineIntervalMs[num] = 0.0;
			_waterfallLastAdvanceFrameStart[num] = double.NaN;
		}
	}

	private static void resizeWaterfallTimeOverlay(int rx, int waterHeight, int preservedRows)
	{
		int num = rx - 1;
		if (num < 0 || num >= _waterfallRowUtcTicks.Length)
		{
			return;
		}
		if (waterHeight <= 0 || preservedRows <= 0)
		{
			resetWaterfallTimeOverlay(rx);
			return;
		}
		int num2 = Math.Min(Math.Min(preservedRows, waterHeight), Math.Min(_waterfallRowTimeCounts[num], _waterfallRowUtcTicks[num].Length));
		long[] array = new long[waterHeight];
		long[] array2 = new long[waterHeight];
		long[] array3 = new long[waterHeight];
		if (num2 > 0)
		{
			Array.Copy(_waterfallRowUtcTicks[num], array, num2);
			Array.Copy(_waterfallRowLabelUtcTicks[num], array2, num2);
			Array.Copy(_waterfallRowLabelIntervalMs[num], array3, num2);
		}
		_waterfallRowUtcTicks[num] = array;
		_waterfallRowLabelUtcTicks[num] = array2;
		_waterfallRowLabelIntervalMs[num] = array3;
		_waterfallRowTimeCounts[num] = num2;
		_waterfallRowsSinceLastLabel[num] = int.MaxValue;
		for (int i = 0; i < num2; i++)
		{
			if (array2[i] != 0L)
			{
				_waterfallRowsSinceLastLabel[num] = i;
				break;
			}
		}
	}

	private static double getWaterfallLineIntervalMs(int rx)
	{
		int num = rx - 1;
		if (num < 0 || num >= _waterfallLineIntervalMs.Length)
		{
			return 1000.0;
		}
		double num2 = ((rx == 2) ? _wfRowIntervalMsRX2 : _wfRowIntervalMsRX1);
		if (num2 <= 0.0)
		{
			int num3 = Math.Max(1, m_nFps);
			int num4 = Math.Max(1, (rx == 2) ? rx2_waterfall_update_period : waterfall_update_period);
			num2 = 1000.0 * (double)num4 / (double)num3;
		}
		double num5 = _waterfallLineIntervalMs[num];
		if (!(num5 > 0.0) || !(Math.Abs(num5 - num2) <= num2 * 0.35))
		{
			return num2;
		}
		return num5;
	}

	private static void recordWaterfallAdvance(int rx, int waterHeight)
	{
		int num = rx - 1;
		if (num < 0 || num >= _waterfallRowUtcTicks.Length || waterHeight <= 0)
		{
			return;
		}
		double dElapsedFrameStart = m_dElapsedFrameStart;
		double num2 = _waterfallLastAdvanceFrameStart[num];
		if (!double.IsNaN(num2))
		{
			double num3 = dElapsedFrameStart - num2;
			if (num3 > 0.0 && num3 < 60000.0)
			{
				_waterfallLineIntervalMs[num] = ((_waterfallLineIntervalMs[num] <= 0.0) ? num3 : (_waterfallLineIntervalMs[num] * 0.85 + num3 * 0.15));
			}
		}
		_waterfallLastAdvanceFrameStart[num] = dElapsedFrameStart;
		if (_waterfallRowUtcTicks[num].Length != waterHeight)
		{
			_waterfallRowUtcTicks[num] = new long[waterHeight];
			_waterfallRowLabelUtcTicks[num] = new long[waterHeight];
			_waterfallRowLabelIntervalMs[num] = new long[waterHeight];
			_waterfallRowTimeCounts[num] = 0;
			_waterfallRowsSinceLastLabel[num] = int.MaxValue;
		}
		long[] array = _waterfallRowUtcTicks[num];
		long[] array2 = _waterfallRowLabelUtcTicks[num];
		long[] array3 = _waterfallRowLabelIntervalMs[num];
		int num4 = Math.Min(_waterfallRowTimeCounts[num], waterHeight);
		int num5 = Math.Min(num4, waterHeight - 1);
		if (num5 > 0)
		{
			Array.Copy(array, 0, array, 1, num5);
			Array.Copy(array2, 0, array2, 1, num5);
			Array.Copy(array3, 0, array3, 1, num5);
		}
		long num6 = (array[0] = DateTime.UtcNow.Ticks);
		array2[0] = 0L;
		array3[0] = 0L;
		_waterfallRowTimeCounts[num] = Math.Min(waterHeight, num4 + 1);
		if (_waterfallRowsSinceLastLabel[num] < int.MaxValue)
		{
			_waterfallRowsSinceLastLabel[num]++;
		}
		if (_waterfallRowTimeCounts[num] > 1)
		{
			float num7 = (measureStringDX2D("00:00:00", fontDX2d_font9, cacheStringLength: true).Height + 4f) * 2f;
			long num8 = chooseWaterfallLabelIntervalMs(getWaterfallLineIntervalMs(rx), num7);
			long num9 = num8 * 10000;
			long num10 = toDisplayTicks(array[1]);
			long num11 = toDisplayTicks(num6);
			long num12 = num11 - num11 % num9;
			if (num11 > num10 && num12 > num10 && _waterfallRowsSinceLastLabel[num] >= (int)Math.Ceiling(num7))
			{
				array2[0] = num6 - (num11 - num12);
				array3[0] = num8;
				_waterfallRowsSinceLastLabel[num] = 0;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static long toDisplayTicks(long utcTicks)
	{
		if (m_eWaterfallTime != WaterfallTimeMode.LOCAL)
		{
			return utcTicks;
		}
		return new DateTime(utcTicks, DateTimeKind.Utc).ToLocalTime().Ticks;
	}

	private static long chooseWaterfallLabelIntervalMs(double msPerLine, double minLabelSpacing)
	{
		double num = Math.Max(1.0, msPerLine);
		double num2 = 5000.0 / num;
		double num3 = 1000.0 / num;
		double num4 = Math.Max(26.0, minLabelSpacing * 1.5);
		if (num2 > 96.0 && num3 >= num4)
		{
			return _waterfallTimeLabelIntervalsMs[0];
		}
		if (num2 >= 18.0)
		{
			return 5000L;
		}
		long result = _waterfallTimeLabelIntervalsMs[1];
		double num5 = double.MaxValue;
		for (int i = 1; i < _waterfallTimeLabelIntervalsMs.Length; i++)
		{
			double num6 = Math.Abs((double)_waterfallTimeLabelIntervalsMs[i] / num - 56.0);
			if (num6 < num5)
			{
				num5 = num6;
				result = _waterfallTimeLabelIntervalsMs[i];
			}
		}
		return result;
	}

	private static void drawWaterfallTimeOverlay(int nVerticalShift, int W, int H, int rx)
	{
		if (m_eShowWaterfallTime == WaterfallTimePosition.NONE)
		{
			return;
		}
		int num = H - 20;
		if (num <= 8)
		{
			return;
		}
		int num2 = rx - 1;
		if (num2 < 0 || num2 >= _waterfallRowLabelUtcTicks.Length)
		{
			return;
		}
		int num3 = Math.Min(_waterfallRowTimeCounts[num2], num);
		if (num3 == 0)
		{
			return;
		}
		float num4 = (float)nVerticalShift + 20f;
		float num5 = num4 + (float)num;
		SharpDX.Direct2D1.Brush dXBrushForColour = getDXBrushForColour(m_cWaterfallTimeColour, 255);
		SharpDX.Direct2D1.Brush dXBrushForColour2 = getDXBrushForColour(System.Drawing.Color.Black, 144);
		SharpDX.Direct2D1.Brush dXBrushForColour3 = getDXBrushForColour(m_cWaterfallTimeColour, 96);
		long[] array = _waterfallRowLabelUtcTicks[num2];
		long[] array2 = _waterfallRowLabelIntervalMs[num2];
		for (int i = 0; i < num3; i++)
		{
			long num6 = array[i];
			if (num6 == 0L)
			{
				continue;
			}
			long num7 = ((array2[i] > 0) ? array2[i] : 5000);
			float num8 = num4 + (float)i;
			DateTime dateTime = new DateTime(toDisplayTicks(num6), DateTimeKind.Unspecified);
			string s = ((num7 >= 60000) ? dateTime.ToString("HH:mm") : dateTime.ToString("HH:mm:ss"));
			SizeF sizeF = measureStringDX2D(s, fontDX2d_font9, num7 < 60000);
			float num9 = sizeF.Width + 8f;
			float num10 = sizeF.Height + 4f;
			float num11 = num8 - num10 / 2f;
			if (!(num11 < num4) && !(num11 + num10 > num5))
			{
				float num12;
				float num13;
				float val;
				if (m_eShowWaterfallTime == WaterfallTimePosition.LEFT)
				{
					num12 = 2f;
					num13 = 12f;
					val = num13 + 4f;
				}
				else
				{
					num12 = (float)W - 2f - 1f;
					num13 = num12 - 10f;
					val = num13 - 4f - num9;
				}
				val = Math.Max(2f, Math.Min(val, (float)W - 2f - num9));
				float x = val + 4f;
				drawLineDX2D(dXBrushForColour, num12, num8, num13, num8, 1.5f);
				RoundedRectangle roundedRect = new RoundedRectangle
				{
					RadiusX = 4f,
					RadiusY = 4f,
					Rect = new SharpDX.RectangleF(val, num11, num9, num10)
				};
				_d2dRenderTarget.FillRoundedRectangle(roundedRect, dXBrushForColour2);
				_d2dRenderTarget.DrawRoundedRectangle(roundedRect, dXBrushForColour3);
				drawStringDX2D(s, fontDX2d_font9, dXBrushForColour, x, num11 + 2f);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetPendingWaterfallPixelRef(int rx, double pixel_ref)
	{
		int num = ((rx == 2) ? 1 : 0);
		_pendingWaterfallPixelRef[num] = pixel_ref;
	}

	private static void resetWaterfallBitmapAlignment(int rx)
	{
		int num = ((rx == 2) ? 1 : 0);
		_pendingWaterfallPixelRef[num] = double.NaN;
		_currentWaterfallPixelRef[num] = double.NaN;
		_waterfallBitmapCenterMHz[num] = double.NaN;
		_waterfallBitmapSpanHz[num] = double.NaN;
		_waterfallBitmapShiftRemainderPixels[num] = 0.0;
		_waterfallBitmapWidths[num] = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double getWaterfallSpanHz(int rx)
	{
		return (rx == 2) ? (RX2DisplayHigh - RX2DisplayLow) : (RXDisplayHigh - RXDisplayLow);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool isWaterfallNoiseFloorCompensationEnabled(int rx)
	{
		return rx switch
		{
			2 => m_bWaterfallUseNFForACGRX2, 
			1 => m_bWaterfallUseNFForACGRX1, 
			_ => false, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool useWaterfallNoiseFloorCompensation(int rx)
	{
		if (!isWaterfallNoiseFloorCompensationEnabled(rx))
		{
			return false;
		}
		switch (rx)
		{
		default:
			return false;
		case 2:
			if (!m_bFastAttackNoiseFloorRX2)
			{
				return m_bNoiseFloorGoodRX2;
			}
			return false;
		case 1:
			if (!m_bFastAttackNoiseFloorRX1)
			{
				return m_bNoiseFloorGoodRX1;
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float getWaterfallNoiseFloorCompensationTarget(int rx)
	{
		return rx switch
		{
			2 => m_fLerpAverageRX2 - m_fWaterfallAGCOffsetRX2, 
			1 => m_fLerpAverageRX1 - m_fWaterfallAGCOffsetRX1, 
			_ => -150f, 
		};
	}

	private static int prepareWaterfallBitmapShift(int rx, int width, double centerMHz, out bool clearBitmap)
	{
		int num = ((rx == 2) ? 1 : 0);
		double waterfallSpanHz = getWaterfallSpanHz(rx);
		bool flag = waterfallSpanHz > 0.0 && !double.IsNaN(_waterfallBitmapSpanHz[num]) && Math.Abs(_waterfallBitmapSpanHz[num] - waterfallSpanHz) > 0.5;
		clearBitmap = _waterfallBitmapWidths[num] > 0 && _waterfallBitmapWidths[num] != width;
		if (clearBitmap | flag)
		{
			_waterfallBitmapShiftRemainderPixels[num] = 0.0;
		}
		_waterfallBitmapWidths[num] = width;
		_waterfallBitmapSpanHz[num] = waterfallSpanHz;
		if (width <= 0 || waterfallSpanHz <= 0.0 || double.IsNaN(centerMHz) || centerMHz <= 0.0)
		{
			if (clearBitmap)
			{
				_waterfallBitmapCenterMHz[num] = double.NaN;
			}
			return 0;
		}
		if (clearBitmap || double.IsNaN(_waterfallBitmapCenterMHz[num]) || _waterfallBitmapCenterMHz[num] <= 0.0)
		{
			_waterfallBitmapCenterMHz[num] = centerMHz;
			_waterfallBitmapShiftRemainderPixels[num] = 0.0;
			return 0;
		}
		if (flag)
		{
			_waterfallBitmapCenterMHz[num] = centerMHz;
			return 0;
		}
		double num2 = waterfallSpanHz / (double)width;
		if (num2 <= 0.0)
		{
			_waterfallBitmapCenterMHz[num] = centerMHz;
			_waterfallBitmapShiftRemainderPixels[num] = 0.0;
			return 0;
		}
		double num3 = _waterfallBitmapShiftRemainderPixels[num] - (centerMHz - _waterfallBitmapCenterMHz[num]) * 1000000.0 / num2;
		int num4 = ((num3 >= 0.0) ? ((int)Math.Floor(num3)) : ((int)Math.Ceiling(num3)));
		_waterfallBitmapCenterMHz[num] = centerMHz;
		_waterfallBitmapShiftRemainderPixels[num] = num3 - (double)num4;
		return num4;
	}

	private static void clearWaterfallBitmapRegion(SharpDX.Direct2D1.Bitmap bitmap, int x, int y, int width, int height)
	{
		if (bitmap == null || bitmap.IsDisposed || width <= 0 || height <= 0)
		{
			return;
		}
		int num = ((bitmap.PixelFormat.Format == Format.R16G16B16A16_Float) ? 8 : 4);
		int num2 = width * num;
		int num3 = num2 * height;
		byte[] array = ArrayPool<byte>.Shared.Rent(num3);
		try
		{
			if (num == 8)
			{
				WaterfallPixelWriter.FillClearBuffer(array, num3);
			}
			else
			{
				for (int i = 0; i < num3; i += 4)
				{
					array[i] = 0;
					array[i + 1] = 0;
					array[i + 2] = 0;
					array[i + 3] = byte.MaxValue;
				}
			}
			bitmap.CopyFromMemory(array, num2, new SharpDX.Rectangle(x, y, width, height));
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	private static void DrawPaletteScheme(WaterfallPalette palette, float[] waterfall_data, float[] dataCopy, float fOffset, ref float waterfall_minimum, float low_threshold, float high_threshold, int nDecimatedWidth, int m_nDecimation, float[] rowF, float alphaF)
	{
		float num = high_threshold - low_threshold;
		if (num <= 0f)
		{
			num = 1f;
		}
		float paletteSharpness = WaterfallEnhancer.PaletteSharpness;
		float paletteContrast = WaterfallEnhancer.PaletteContrast;
		int num2 = ((paletteSharpness > 0f) ? ((int)(256f - paletteSharpness * 170f)) : 256);
		if (num2 < 3)
		{
			num2 = 3;
		}
		float num3 = ((paletteContrast > 1f) ? 1f : paletteContrast);
		float num4 = 1f - 0.8f * (paletteContrast / 1.5f);
		for (int i = 0; i < nDecimatedWidth; i++)
		{
			float num5 = waterfall_data[i];
			float percent = ((num5 <= low_threshold) ? 0f : ((!(num5 >= high_threshold)) ? ((num5 - low_threshold) / num) : 1f));
			percent = WaterfallEnhancer.ApplyQualityPercent(percent);
			if (num3 > 0f)
			{
				float num6 = ((percent >= 0.5f) ? 1f : (-1f));
				float num7 = 0.5f + num6 * 0.5f * (float)Math.Pow(Math.Abs(2f * percent - 1f), num4);
				percent += (num7 - percent) * num3;
			}
			if (num2 < 256)
			{
				percent = (float)Math.Floor(percent * (float)num2) / (float)num2;
				if (percent > 1f)
				{
					percent = 1f;
				}
			}
			palette.Sample(percent, out var r, out var g, out var b);
			if (waterfall_minimum > dataCopy[i] + fOffset)
			{
				waterfall_minimum = dataCopy[i] + fOffset;
			}
			int num8 = i * m_nDecimation * 4;
			rowF[num8] = r;
			rowF[num8 + 1] = g;
			rowF[num8 + 2] = b;
			rowF[num8 + 3] = alphaF;
		}
	}

	internal static void UploadPaletteToGPU(WaterfallGPURenderer renderer, WaterfallPalette palette)
	{
		if (palette != null && renderer != null)
		{
			for (int i = 0; i < 256; i++)
			{
				palette.Sample((float)i / 255f, out var r, out var g, out var b);
				int num = i * 4;
				_gpuPaletteUpload[num] = r / 255f;
				_gpuPaletteUpload[num + 1] = g / 255f;
				_gpuPaletteUpload[num + 2] = b / 255f;
				_gpuPaletteUpload[num + 3] = 1f;
			}
			renderer.SetPalette(_gpuPaletteUpload, 256);
		}
	}

	internal static void UploadCustomGradientToGPU(WaterfallGPURenderer renderer, System.Drawing.Color[] colours)
	{
		if (renderer != null && colours != null && colours.Length >= 2)
		{
			int num = colours.Length;
			if (num > 1024)
			{
				num = 1024;
			}
			for (int i = 0; i < num; i++)
			{
				System.Drawing.Color color = colours[i];
				int num2 = i * 4;
				_gpuPaletteUpload[num2] = (float)(int)color.R / 255f;
				_gpuPaletteUpload[num2 + 1] = (float)(int)color.G / 255f;
				_gpuPaletteUpload[num2 + 2] = (float)(int)color.B / 255f;
				_gpuPaletteUpload[num2 + 3] = 1f;
			}
			renderer.SetPalette(_gpuPaletteUpload, num);
		}
	}

	private static void DrawWaterfallToTarget(SharpDX.Direct2D1.Bitmap bmp, int nVerticalShift, int topMargin, float opacity)
	{
		SharpDX.Direct2D1.DeviceContext deviceContext = _d2dRenderTarget as SharpDX.Direct2D1.DeviceContext;
		if (_gpuEffectsEnabled && deviceContext != null && GPUDetector.HasBuiltInEffects && WaterfallEnhancer.Depth == WaterfallEnhancer.ColorDepth.Bit8)
		{
			if (WaterfallEffect.Draw(deviceContext, bmp, 0f, nVerticalShift + topMargin, opacity, 1f + WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.Gamma, _effectiveToneMap, WaterfallEnhancer.DitherEnabled))
			{
				return;
			}
			_gpuEffectsEnabled = false;
			LogTool.AddLogEntry("GPU draw failed, switched to CPU", "D2D");
		}
		_d2dRenderTarget.DrawBitmap(bmp, new SharpDX.RectangleF(0f, nVerticalShift + topMargin, bmp.Size.Width, bmp.Size.Height), opacity, _gpuWaterfallLinearDraw ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor);
	}

	public static void ResetWaterfallTimers()
	{
		if (_wfRowTimerRX1 != null)
		{
			_wfLastRowTimeRX1 = _wfRowTimerRX1.ElapsedMsec;
		}
		if (_wfRowTimerRX2 != null)
		{
			_wfLastRowTimeRX2 = _wfRowTimerRX2.ElapsedMsec;
		}
		resetWaterfallTimeOverlay(1);
		resetWaterfallTimeOverlay(2);
	}

	internal static WaterfallPalette GetPaletteConsole()
	{
		if (_paletteConsole == null)
		{
			_paletteConsole = new WaterfallPalette();
			_paletteConsole.Build(WaterfallPalette.ConsoleStops);
		}
		return _paletteConsole;
	}

	internal static WaterfallPalette GetPaletteThermal()
	{
		if (_paletteThermal == null)
		{
			_paletteThermal = new WaterfallPalette();
			_paletteThermal.Build(WaterfallPalette.ThermalStops);
		}
		return _paletteThermal;
	}

	internal static WaterfallPalette GetPaletteDeepBlue()
	{
		if (_paletteDeepBlue == null)
		{
			_paletteDeepBlue = new WaterfallPalette();
			_paletteDeepBlue.Build(WaterfallPalette.DeepBlueStops);
		}
		return _paletteDeepBlue;
	}

	internal static WaterfallPalette GetPaletteEnhanced256()
	{
		if (_paletteEnhanced256 == null)
		{
			_paletteEnhanced256 = new WaterfallPalette();
			_paletteEnhanced256.Build(WaterfallPalette.EnhancedStops);
		}
		return _paletteEnhanced256;
	}

	internal static WaterfallPalette GetPaletteGrayscale256()
	{
		if (_paletteGrayscale256 == null)
		{
			_paletteGrayscale256 = new WaterfallPalette();
			_paletteGrayscale256.Build(WaterfallPalette.GrayscaleStops);
		}
		return _paletteGrayscale256;
	}

	internal static WaterfallPalette GetPaletteForScheme(ColorScheme scheme)
	{
		return scheme switch
		{
			ColorScheme.Console => GetPaletteConsole(), 
			ColorScheme.Thermal => GetPaletteThermal(), 
			ColorScheme.DeepBlue => GetPaletteDeepBlue(), 
			ColorScheme.Enhanced256 => GetPaletteEnhanced256(), 
			ColorScheme.Grayscale256 => GetPaletteGrayscale256(), 
			_ => null, 
		};
	}

	private static void OnWaterfallRXGradientChanged(int rx, System.Drawing.Color[] colours)
	{
		if (colours.Length < 2)
		{
			return;
		}
		int num = colours.Length;
		System.Drawing.Color[] array;
		switch (rx)
		{
		case 1:
			_rx1_waterfall_grad_ok = false;
			if (_rx1_waterfall_grad == null || _rx1_waterfall_grad.Length != num)
			{
				_rx1_waterfall_grad = new System.Drawing.Color[num];
			}
			array = _rx1_waterfall_grad;
			break;
		case 2:
			_rx2_waterfall_grad_ok = false;
			if (_rx2_waterfall_grad == null || _rx2_waterfall_grad.Length != num)
			{
				_rx2_waterfall_grad = new System.Drawing.Color[num];
			}
			array = _rx2_waterfall_grad;
			break;
		default:
			return;
		}
		for (int i = 0; i < num; i++)
		{
			array[i] = System.Drawing.Color.FromArgb(255, colours[i]);
		}
		switch (rx)
		{
		case 1:
			_rx1_waterfall_grad_ok = true;
			break;
		case 2:
			_rx2_waterfall_grad_ok = true;
			break;
		}
	}

	private static void OnWaterfallTXGradientChanged(System.Drawing.Color[] colours)
	{
		if (colours.Length >= 2)
		{
			int num = colours.Length;
			_tx_waterfall_grad_ok = false;
			if (_tx_waterfall_grad == null || _tx_waterfall_grad.Length != num)
			{
				_tx_waterfall_grad = new System.Drawing.Color[num];
			}
			System.Drawing.Color[] tx_waterfall_grad = _tx_waterfall_grad;
			for (int i = 0; i < num; i++)
			{
				tx_waterfall_grad[i] = System.Drawing.Color.FromArgb(255, colours[i]);
			}
			_tx_waterfall_grad_ok = true;
		}
	}

	private static void ComputeWaterfallThresholds(int rx, bool local_mox, out float low_threshold, out float high_threshold, out ColorScheme cScheme, out System.Drawing.Color low_color, out bool useNoiseFloorCompensation, out bool useSettledNoiseFloorCompensation, out float noiseFloorCompensationTarget)
	{
		low_threshold = 0f;
		high_threshold = 0f;
		cScheme = ColorScheme.enhanced;
		low_color = System.Drawing.Color.Black;
		useNoiseFloorCompensation = !local_mox && isWaterfallNoiseFloorCompensationEnabled(rx);
		useSettledNoiseFloorCompensation = !local_mox && useWaterfallNoiseFloorCompensation(rx);
		noiseFloorCompensationTarget = (useNoiseFloorCompensation ? getWaterfallNoiseFloorCompensationTarget(rx) : (-150f));
		if (rx == 2)
		{
			if (local_mox)
			{
				low_threshold = TXWFAmpMin;
				high_threshold = TXWFAmpMax;
				cScheme = _tx_color_scheme;
				low_color = waterfall_low_color_tx;
				return;
			}
			if (_autoThresholdEnabled && m_bNoiseFloorGoodRX2)
			{
				float num = m_fLerpAverageRX2 + _fNFshiftDBM;
				float num2 = rx2_waterfall_low_threshold - num;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
				if (num2 > 10f)
				{
					num2 = 10f;
				}
				float num3 = rx2_waterfall_high_threshold - rx2_waterfall_low_threshold;
				if (num3 < 10f)
				{
					num3 = 10f;
				}
				low_threshold = num + num2 + _autoThresholdFineOffset;
				high_threshold = low_threshold + num3;
			}
			else
			{
				high_threshold = rx2_waterfall_high_threshold;
				if (_autoHighEnabledRX2)
				{
					if (_autoHighRX2 <= -39f)
					{
						_autoHighRX2 = rx2_waterfall_high_threshold;
					}
					float num4 = _autoHighRX2 + _autoHighMarginDb;
					if (num4 < rx2_waterfall_high_threshold)
					{
						num4 = rx2_waterfall_high_threshold;
					}
					high_threshold = num4;
				}
				if (rx2_waterfall_agc && !m_bRX2_spectrum_thresholds)
				{
					if (useNoiseFloorCompensation)
					{
						low_threshold = (useSettledNoiseFloorCompensation ? noiseFloorCompensationTarget : _RX2waterfallPreviousMinValue);
					}
					else
					{
						low_threshold = _RX2waterfallPreviousMinValue;
						low_threshold -= m_fWaterfallAGCOffsetRX2;
					}
				}
				else
				{
					low_threshold = rx2_waterfall_low_threshold;
				}
			}
			cScheme = _rx2_color_scheme;
			low_color = rx2_waterfall_low_color;
			return;
		}
		if (local_mox)
		{
			low_threshold = TXWFAmpMin;
			high_threshold = TXWFAmpMax;
			cScheme = _tx_color_scheme;
			low_color = waterfall_low_color_tx;
			return;
		}
		if (_autoThresholdEnabled && m_bNoiseFloorGoodRX1)
		{
			float num5 = m_fLerpAverageRX1 + _fNFshiftDBM;
			float num6 = waterfall_low_threshold - num5;
			if (num6 < 0f)
			{
				num6 = 0f;
			}
			if (num6 > 10f)
			{
				num6 = 10f;
			}
			float num7 = waterfall_high_threshold - waterfall_low_threshold;
			if (num7 < 10f)
			{
				num7 = 10f;
			}
			low_threshold = num5 + num6 + _autoThresholdFineOffset;
			high_threshold = low_threshold + num7;
		}
		else
		{
			high_threshold = waterfall_high_threshold;
			if (_autoHighEnabledRX1)
			{
				if (_autoHighRX1 <= -39f)
				{
					_autoHighRX1 = waterfall_high_threshold;
				}
				float num8 = _autoHighRX1 + _autoHighMarginDb;
				if (num8 < waterfall_high_threshold)
				{
					num8 = waterfall_high_threshold;
				}
				high_threshold = num8;
			}
			if (rx1_waterfall_agc && !m_bRX1_spectrum_thresholds)
			{
				if (useNoiseFloorCompensation)
				{
					low_threshold = (useSettledNoiseFloorCompensation ? noiseFloorCompensationTarget : _RX1waterfallPreviousMinValue);
				}
				else
				{
					low_threshold = _RX1waterfallPreviousMinValue;
					low_threshold -= m_fWaterfallAGCOffsetRX1;
				}
			}
			else
			{
				low_threshold = waterfall_low_threshold;
			}
		}
		cScheme = _rx1_color_scheme;
		low_color = waterfall_low_color;
	}

	private static float[] ExpandDecimatedRow(float[] src, int W)
	{
		int nDecimation = m_nDecimation;
		if (src == null || nDecimation <= 1 || W <= 0)
		{
			return src;
		}
		int num = W / nDecimation;
		if (num <= 0 || src.Length < num)
		{
			return src;
		}
		float[] array = new float[W];
		for (int i = 0; i < W; i++)
		{
			int num2 = i / nDecimation;
			if (num2 >= num)
			{
				num2 = num - 1;
			}
			array[i] = src[num2];
		}
		return array;
	}

	private unsafe static void ProduceForDetached()
	{
		bool num = SharedWaterfallState.DetachedActive && _detachedRenderer != null;
		bool flag = SharedWaterfallState.FilterDisplaySpanActive(1);
		if ((!num && !flag) || _wfDrawnRX1ThisFrame)
		{
			return;
		}
		_detachedFrameCounter++;
		int num2 = Math.Max(1, waterfall_update_period);
		if (_detachedFrameCounter % num2 != 0)
		{
			SharedWaterfallState.ProduceForDetachedFresh = false;
			return;
		}
		int num3 = displayTargetWidth;
		double waterfallSpanHz = getWaterfallSpanHz(1);
		if (_zoomAdaptiveEnabled)
		{
			int sampleRateRX = SampleRateRX1;
			ZoomAdaptive.ComputeParams(waterfallSpanHz, sampleRateRX, (int)WaterfallEnhancer.ToneMap, out var toneMapMode, out var temporalAlpha);
			_effectiveToneMap = toneMapMode;
			_effectiveTemporalAlpha = temporalAlpha;
		}
		else
		{
			_effectiveToneMap = (int)WaterfallEnhancer.ToneMap;
			_effectiveTemporalAlpha = (_temporalEnabled ? _temporalAlpha : 0f);
		}
		bool local_mox = localMox(1);
		ComputeWaterfallThresholds(1, local_mox, out var low_threshold, out var high_threshold, out var cScheme, out var low_color, out var useNoiseFloorCompensation, out var useSettledNoiseFloorCompensation, out var noiseFloorCompensationTarget);
		try
		{
			GPUWaterfallPipeline gpuFFT = _gpuFFT1;
			if (_waterfallRenderQuality == WaterfallRenderQuality.High && _gpuWaterfallPipelineEnabled && _gpuEffectsEnabled && gpuFFT != null && gpuFFT.IsInitialized && gpuFFT.MagSpectrumView != null && !gpuFFT.MagSpectrumView.IsDisposed)
			{
				try
				{
					int flag2 = 0;
					double pixel_ref = 0.0;
					fixed (float* pix = &new_waterfall_data[0])
					{
						SpecHPSDRDLL.GetPixels(0, 1, pix, ref flag2, out pixel_ref);
					}
					if (flag2 == 1)
					{
						fixed (float* ptr = &new_waterfall_data[0])
						{
							void* srcptr = ptr;
							fixed (float* ptr2 = &current_waterfall_data[0])
							{
								void* destptr = ptr2;
								Win32.memcpy(destptr, srcptr, num3 * 4);
							}
						}
						fixed (float* ptr = &current_waterfall_data[0])
						{
							void* srcptr2 = ptr;
							fixed (float* ptr2 = &current_waterfall_data_copy[0])
							{
								void* destptr2 = ptr2;
								Win32.memcpy(destptr2, srcptr2, num3 * 4);
							}
						}
						float[] array = _gpuCalReferenceRowRX1;
						if (array == null || array.Length != num3)
						{
							array = new float[num3];
						}
						Array.Copy(current_waterfall_data, array, num3);
						_gpuCalReferenceRowRX1 = array;
					}
				}
				catch
				{
				}
				SharedWaterfallState.ProduceForDetachedFresh = ProcessGPUWaterfall(1, num3) != null;
			}
			else
			{
				bool produceForDetachedFresh = false;
				if (waterfall_data_ready)
				{
					fixed (float* ptr = &new_waterfall_data[0])
					{
						void* srcptr3 = ptr;
						fixed (float* ptr2 = &current_waterfall_data[0])
						{
							void* destptr3 = ptr2;
							Win32.memcpy(destptr3, srcptr3, num3 * 4);
						}
					}
					fixed (float* ptr = &current_waterfall_data[0])
					{
						void* srcptr4 = ptr;
						fixed (float* ptr2 = &current_waterfall_data_copy[0])
						{
							void* destptr4 = ptr2;
							Win32.memcpy(destptr4, srcptr4, num3 * 4);
						}
					}
					waterfall_data_ready = false;
					produceForDetachedFresh = true;
				}
				SharedWaterfallState.ProduceForDetachedFresh = produceForDetachedFresh;
			}
		}
		catch (Exception ex)
		{
			SharedWaterfallState.ProduceForDetachedFresh = false;
			GPUWaterfallLogger.Log("DetachedProduce", "row exception: " + ex.Message);
		}
		ComputeWaterfallThresholds(1, local_mox, out low_threshold, out high_threshold, out cScheme, out low_color, out useNoiseFloorCompensation, out useSettledNoiseFloorCompensation, out noiseFloorCompensationTarget);
		float gpuCalOffsetRX = _gpuCalOffsetRX1;
		float rX1Offset = RX1Offset;
		float[] panadapterRow = ExpandDecimatedRow(current_display_data, num3);
		int panadapterWidth = num3;
		int num4;
		int rXDisplayLow;
		if (localMox(1))
		{
			num4 = ((!DisplayDuplex) ? 1 : 0);
			if (num4 != 0)
			{
				rXDisplayLow = tx_display_low;
				goto IL_0337;
			}
		}
		else
		{
			num4 = 0;
		}
		rXDisplayLow = RXDisplayLow;
		goto IL_0337;
		IL_0337:
		float rxDisplayLowHz = rXDisplayLow;
		float rxDisplayHighHz = ((num4 != 0) ? tx_display_high : RXDisplayHigh);
		float[] waterfallRow = ExpandDecimatedRow(current_waterfall_data, num3);
		GPUWaterfallPipeline gpuFFT2 = _gpuFFT1;
		float[] array2 = ((gpuFFT2 != null && gpuFFT2.IsInitialized) ? gpuFFT2.NarrowRow : null);
		int narrowRowWidth = ((array2 != null) ? 1024 : 0);
		SharedWaterfallState.PublishRX(1, low_threshold, high_threshold, gpuCalOffsetRX, rX1Offset, cScheme, SharedWaterfallState.PaletteVersion, WaterfallEnhancer.Gamma, _effectiveToneMap, _effectiveTemporalAlpha, panadapterRow, panadapterWidth, rxDisplayLowHz, rxDisplayHighHz, waterfallRow, num3, array2, narrowRowWidth);
	}

	private static void PublishDetachedSnapshotIfActive(int rx, int W, float low_threshold, float high_threshold, ColorScheme cScheme, float fOffset)
	{
		float gpuCalOffset = ((rx == 1) ? _gpuCalOffsetRX1 : _gpuCalOffsetRX2);
		float[] panadapterRow = ExpandDecimatedRow((rx == 1) ? current_display_data : current_display_data_bottom, W);
		int num;
		int num2;
		if (localMox(rx))
		{
			num = ((!DisplayDuplex) ? 1 : 0);
			if (num != 0)
			{
				num2 = tx_display_low;
				goto IL_0055;
			}
		}
		else
		{
			num = 0;
		}
		num2 = ((rx == 1) ? RXDisplayLow : RX2DisplayLow);
		goto IL_0055;
		IL_0055:
		float rxDisplayLowHz = num2;
		float rxDisplayHighHz = ((num != 0) ? tx_display_high : ((rx == 1) ? RXDisplayHigh : RX2DisplayHigh));
		float[] waterfallRow = ExpandDecimatedRow((rx == 1) ? current_waterfall_data : current_waterfall_data_bottom, W);
		GPUWaterfallPipeline gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
		SharedWaterfallState.ProduceForDetachedFresh = gPUWaterfallPipeline != null && gPUWaterfallPipeline.IsInitialized && gPUWaterfallPipeline.MagSpectrumView != null && !gPUWaterfallPipeline.MagSpectrumView.IsDisposed;
		float[] array = ((gPUWaterfallPipeline != null && gPUWaterfallPipeline.IsInitialized) ? gPUWaterfallPipeline.NarrowRow : null);
		int narrowRowWidth = ((array != null) ? 1024 : 0);
		SharedWaterfallState.PublishRX(rx, low_threshold, high_threshold, gpuCalOffset, fOffset, cScheme, SharedWaterfallState.PaletteVersion, WaterfallEnhancer.Gamma, _effectiveToneMap, _effectiveTemporalAlpha, panadapterRow, W, rxDisplayLowHz, rxDisplayHighHz, waterfallRow, W, array, narrowRowWidth);
	}

	private unsafe static bool DrawWaterfallDX2D(int nVerticalShift, int W, int H, int rx, bool bottom)
	{
		switch (rx)
		{
		case 1:
			_wfDrawnRX1ThisFrame = true;
			break;
		case 2:
			_wfDrawnRX2ThisFrame = true;
			break;
		}
		bool flag;
		if (!_allowWaterfallSmear && ((rx == 1) ? _stopRx1Waterfall : _stopRx2Waterfall))
		{
			DateTime dateTime = ((rx == 1) ? _rx1_centrefreq_change_time : _rx2_centrefreq_change_time);
			float num = ((rx == 1) ? _fft_fill_timeRX1 : _fft_fill_timeRX2);
			num += (float)m_nFps / 1000f * 2f;
			num *= 1.05f;
			if (num < 100f)
			{
				num = 200f;
			}
			flag = (DateTime.UtcNow - dateTime).TotalMilliseconds > (double)num;
			if (flag)
			{
				switch (rx)
				{
				case 1:
					_stopRx1Waterfall = false;
					break;
				case 2:
					_stopRx2Waterfall = false;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		Matrix3x2 matrix3x = _d2dRenderTarget.Transform;
		_d2dRenderTarget.Transform = Matrix3x2.Identity;
		if (waterfall_data == null || waterfall_data.Length < W)
		{
			waterfall_data = new float[W];
		}
		float num2 = float.MinValue;
		bool flag2 = localMox(rx);
		float num3 = float.MaxValue;
		float num4 = float.MaxValue;
		float num5 = float.MinValue;
		float num6 = float.MaxValue;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		bool flag3 = isRxDuplex(rx);
		float low_threshold = 0f;
		float high_threshold = 0f;
		float waterfall_minimum = 200f;
		ComputeWaterfallThresholds(rx, flag2, out low_threshold, out high_threshold, out var cScheme, out var low_color, out var useNoiseFloorCompensation, out var _, out var noiseFloorCompensationTarget);
		bool flag4 = false;
		int num10 = W / m_nDecimation;
		double waterfallSpanHz = getWaterfallSpanHz(rx);
		if (_zoomAdaptiveEnabled)
		{
			int sampleRateHz = ((rx == 1) ? SampleRateRX1 : SampleRateRX2);
			ZoomAdaptive.ComputeParams(waterfallSpanHz, sampleRateHz, (int)WaterfallEnhancer.ToneMap, out var toneMapMode, out var temporalAlpha);
			_effectiveToneMap = toneMapMode;
			_effectiveTemporalAlpha = temporalAlpha;
		}
		else
		{
			_effectiveToneMap = (int)WaterfallEnhancer.ToneMap;
			_effectiveTemporalAlpha = (_temporalEnabled ? _temporalAlpha : 0f);
		}
		if (console.PowerOn != _old_power)
		{
			_old_power = console.PowerOn;
			_RX1waterfallPreviousMinValue = -150f;
			_RX2waterfallPreviousMinValue = -150f;
		}
		if (console.PowerOn)
		{
			if (rx == 1 && waterfall_data_ready)
			{
				flag4 = true;
				if ((!flag3 & flag2) && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
				{
					for (int i = 0; i < num10; i++)
					{
						current_waterfall_data[i] = -200f;
					}
				}
				else
				{
					fixed (float* ptr = &new_waterfall_data[0])
					{
						void* srcptr = ptr;
						fixed (float* ptr2 = &current_waterfall_data[0])
						{
							void* destptr = ptr2;
							Win32.memcpy(destptr, srcptr, num10 * 4);
						}
					}
				}
				fixed (float* ptr = &current_waterfall_data[0])
				{
					void* srcptr2 = ptr;
					fixed (float* ptr2 = &current_waterfall_data_copy[0])
					{
						void* destptr2 = ptr2;
						Win32.memcpy(destptr2, srcptr2, num10 * 4);
					}
				}
				_currentWaterfallPixelRef[0] = _pendingWaterfallPixelRef[0];
				waterfall_data_ready = false;
			}
			else if (rx == 2 && waterfall_data_ready_bottom)
			{
				flag4 = true;
				if (flag2 && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
				{
					for (int j = 0; j < num10; j++)
					{
						current_waterfall_data_bottom[j] = -200f;
					}
				}
				else
				{
					fixed (float* ptr = &new_waterfall_data_bottom[0])
					{
						void* srcptr3 = ptr;
						fixed (float* ptr2 = &current_waterfall_data_bottom[0])
						{
							void* destptr3 = ptr2;
							Win32.memcpy(destptr3, srcptr3, num10 * 4);
						}
					}
				}
				fixed (float* ptr = &current_waterfall_data_bottom[0])
				{
					void* srcptr4 = ptr;
					fixed (float* ptr2 = &current_waterfall_data_bottom_copy[0])
					{
						void* destptr4 = ptr2;
						Win32.memcpy(destptr4, srcptr4, num10 * 4);
					}
				}
				_currentWaterfallPixelRef[1] = _pendingWaterfallPixelRef[1];
				waterfall_data_ready_bottom = false;
			}
			WaterfallGPURenderer waterfallGPURenderer = null;
			bool flag5 = false;
			bool gpuEffectsEnabled = _gpuEffectsEnabled;
			if (gpuEffectsEnabled && _gpuEffectsEnabled && (WaterfallEnhancer.Depth == WaterfallEnhancer.ColorDepth.Bit8 || WaterfallEnhancer.Depth == WaterfallEnhancer.ColorDepth.Bit16) && _d2dRenderTarget is SharpDX.Direct2D1.DeviceContext)
			{
				waterfallGPURenderer = ((rx == 1) ? _waterfallGPU1 : _waterfallGPU2);
				bool flag6 = cScheme == ColorScheme.Console || cScheme == ColorScheme.Thermal || cScheme == ColorScheme.DeepBlue || cScheme == ColorScheme.Enhanced256 || cScheme == ColorScheme.Grayscale256 || cScheme == ColorScheme.Custom;
				if (((waterfallGPURenderer?.IsInitialized ?? false) & flag6) && waterfallGPURenderer.Width == W && waterfallGPURenderer.Height == H - 20)
				{
					bool flag7 = true;
					if (cScheme == ColorScheme.Custom)
					{
						if (flag2)
						{
							if (!_tx_waterfall_grad_ok)
							{
								flag7 = false;
							}
						}
						else if (rx == 1)
						{
							if (!_rx1_waterfall_grad_ok)
							{
								flag7 = false;
							}
						}
						else if (!_rx2_waterfall_grad_ok)
						{
							flag7 = false;
						}
					}
					flag5 = flag7;
				}
			}
			bool flag8 = false;
			bool flag9 = false;
			if (rx == 1)
			{
				if (_wfRowTimerRX1 == null)
				{
					_wfRowTimerRX1 = new HiPerfTimer();
					_wfRowTimerRX1.Start();
					_wfLastRowTimeRX1 = 0.0;
				}
				double elapsedMsec = _wfRowTimerRX1.ElapsedMsec;
				if (elapsedMsec - _wfLastRowTimeRX1 >= _wfRowIntervalMsRX1)
				{
					flag8 = true;
					_wfLastRowTimeRX1 += _wfRowIntervalMsRX1;
					if (elapsedMsec - _wfLastRowTimeRX1 > _wfRowIntervalMsRX1 * 5.0)
					{
						_wfLastRowTimeRX1 = elapsedMsec;
					}
				}
			}
			else
			{
				if (_wfRowTimerRX2 == null)
				{
					_wfRowTimerRX2 = new HiPerfTimer();
					_wfRowTimerRX2.Start();
					_wfLastRowTimeRX2 = 0.0;
				}
				double elapsedMsec2 = _wfRowTimerRX2.ElapsedMsec;
				if (elapsedMsec2 - _wfLastRowTimeRX2 >= _wfRowIntervalMsRX2)
				{
					flag8 = true;
					_wfLastRowTimeRX2 += _wfRowIntervalMsRX2;
					if (elapsedMsec2 - _wfLastRowTimeRX2 > _wfRowIntervalMsRX2 * 5.0)
					{
						_wfLastRowTimeRX2 = elapsedMsec2;
					}
				}
			}
			bool flag10 = false;
			float num11 = 0f;
			GPUWaterfallPipeline gPUWaterfallPipeline = null;
			bool flag11 = _waterfallRenderQuality == WaterfallRenderQuality.High && _gpuWaterfallPipelineEnabled && _gpuEffectsEnabled;
			if (flag11 & flag8)
			{
				float[] array = ((rx == 1) ? current_waterfall_data_copy : current_waterfall_data_bottom_copy);
				if (array != null)
				{
					float[] array2 = ((rx == 1) ? _gpuCalReferenceRowRX1 : _gpuCalReferenceRowRX2);
					if (array2 == null || array2.Length != array.Length)
					{
						array2 = new float[array.Length];
					}
					Array.Copy(array, array2, array.Length);
					if (rx == 1)
					{
						_gpuCalReferenceRowRX1 = array2;
					}
					else
					{
						_gpuCalReferenceRowRX2 = array2;
					}
				}
				gPUWaterfallPipeline = ((rx == 1) ? _gpuFFT1 : _gpuFFT2);
				float[] array3 = ProcessGPUWaterfall(rx, W);
				if (array3 != null)
				{
					float[] destinationArray = ((rx == 1) ? current_waterfall_data : current_waterfall_data_bottom);
					float[] destinationArray2 = ((rx == 1) ? current_waterfall_data_copy : current_waterfall_data_bottom_copy);
					Array.Copy(array3, destinationArray, W);
					Array.Copy(array3, destinationArray2, W);
					num10 = W;
					flag10 = true;
					num11 = ((rx == 1) ? _gpuCalOffsetRX1 : _gpuCalOffsetRX2);
				}
			}
			bool flag12 = gPUWaterfallPipeline?.IsInitialized ?? false;
			if (flag8 && ((!flag11 || !flag12) | flag10))
			{
				float[] data;
				float[] array4;
				if (rx == 1)
				{
					data = current_waterfall_data;
					array4 = current_waterfall_data_copy;
				}
				else
				{
					data = current_waterfall_data_bottom;
					array4 = current_waterfall_data_bottom_copy;
				}
				if (!flag2 && flag4 && m_bShowVisualNotch)
				{
					modifyDataForNotches(ref data, rx, bottom, flag2, flag3, W);
				}
				float num12 = ((rx != 1) ? RX2Offset : RX1Offset);
				PublishDetachedSnapshotIfActive(rx, W, low_threshold, high_threshold, cScheme, num12);
				float num13 = 0f;
				int num14 = 0;
				float num15 = ((rx == 1) ? (m_fFFTBinAverageRX1 + 2f) : (m_fFFTBinAverageRX2 + 2f));
				for (int k = 0; k < num10; k++)
				{
					float num16 = data[k] + num12;
					float num17 = array4[k] + num12;
					float num18 = num17;
					if (flag10)
					{
						float[] array5 = ((rx == 1) ? _gpuCalReferenceRowRX1 : _gpuCalReferenceRowRX2);
						if (array5 != null && k < array5.Length)
						{
							num18 = array5[k] + num12;
						}
					}
					if (!flag2 && num18 < num15)
					{
						num13 += fastPow10Raw(num18);
						num14++;
					}
					if (num17 > num2)
					{
						num2 = num17;
						max_x = k * (flag10 ? 1 : m_nDecimation);
					}
					if (num17 < num3)
					{
						num3 = num17;
					}
					waterfall_data[k] = num16;
				}
				if (!flag2)
				{
					bool bNoiseFloorAlreadyCalculatedRX = _bNoiseFloorAlreadyCalculatedRX1;
					bool bNoiseFloorAlreadyCalculatedRX2 = _bNoiseFloorAlreadyCalculatedRX2;
					float[] fullData = waterfall_data;
					if (flag10)
					{
						float[] array6 = ((rx == 1) ? _gpuCalReferenceRowRX1 : _gpuCalReferenceRowRX2);
						if (array6 != null && array6.Length >= num10)
						{
							fullData = array6;
						}
					}
					processNoiseFloor(rx, num14, num13, num10, waterfall: true, fullData, num10);
					if (rx == 1)
					{
						if (!m_bFastAttackNoiseFloorRX1 && !bNoiseFloorAlreadyCalculatedRX)
						{
							m_fNoiseFloorRX1 = m_fLerpAverageRX1 + _fNFshiftDBM;
							m_bNoiseFloorGoodRX1 = true;
						}
					}
					else if (!m_bFastAttackNoiseFloorRX2 && !bNoiseFloorAlreadyCalculatedRX2)
					{
						m_fNoiseFloorRX2 = m_fLerpAverageRX2 + _fNFshiftDBM;
						m_bNoiseFloorGoodRX2 = true;
					}
				}
				max_y = num2;
				num6 = num3;
				if (_detachedSkipRender)
				{
					PublishDetachedSnapshotIfActive(rx, W, low_threshold, high_threshold, cScheme, num12);
					return true;
				}
				double centerMHz = ((rx == 1) ? _currentWaterfallPixelRef[0] : _currentWaterfallPixelRef[1]);
				bool clearBitmap = false;
				int num19 = ((!_allowWaterfallSmear) ? prepareWaterfallBitmapShift(rx, W, centerMHz, out clearBitmap) : 0);
				if (num19 > W)
				{
					num19 = W;
				}
				else if (num19 < -W)
				{
					num19 = -W;
				}
				SharpDX.Direct2D1.Bitmap bitmap = ((rx != 1) ? _waterfall_bmp2_dx2d : _waterfall_bmp_dx2d);
				if (clearBitmap)
				{
					clearWaterfallBitmapRegion(bitmap, 0, 0, W, (int)bitmap.Size.Height);
				}
				int height = (int)bitmap.Size.Height - (flag ? 1 : 0);
				SharpDX.Direct2D1.Bitmap comObject = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)bitmap.Size.Width, height), new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(bitmap.PixelFormat.Format, BitmapAlphaModeForFormat(bitmap.PixelFormat.Format))));
				comObject.CopyFromBitmap(bitmap, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)comObject.Size.Width, height));
				bool flag13 = ((rx == 1 && m_bStopRX1WaterfallOnTX) & flag2) || ((rx == 2 && m_bStopRX2WaterfallOnTX) & flag2);
				if (flag5)
				{
					if (cScheme == ColorScheme.Custom)
					{
						System.Drawing.Color[] colours = (flag2 ? _tx_waterfall_grad : ((rx == 1) ? _rx1_waterfall_grad : _rx2_waterfall_grad));
						UploadCustomGradientToGPU(waterfallGPURenderer, colours);
					}
					else
					{
						UploadPaletteToGPU(waterfallGPURenderer, GetPaletteForScheme(cScheme));
					}
					if (clearBitmap)
					{
						waterfallGPURenderer.Clear();
					}
					for (int l = 0; l < num10; l++)
					{
						if (waterfall_minimum > array4[l] + num12)
						{
							waterfall_minimum = array4[l] + num12;
						}
					}
					if (!flag13)
					{
						bool flag14 = flag && (flag10 || !flag11);
						if (flag14)
						{
							float invGamma = ((WaterfallEnhancer.Gamma != 0f) ? (1f / WaterfallEnhancer.Gamma) : 1f);
							if (flag10 && gPUWaterfallPipeline != null && gPUWaterfallPipeline.MagSpectrumView != null)
							{
								waterfallGPURenderer.ProcessRow(gPUWaterfallPipeline.MagSpectrumView, W, 1, low_threshold - num11 - num12, high_threshold - num11 - num12, WaterfallEnhancer.Gamma, invGamma, _effectiveToneMap, WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.ContrastBoost, WaterfallEnhancer.DitherEnabled, WaterfallEnhancer.Levels, _effectiveTemporalAlpha, 0.05f, cScheme != ColorScheme.Custom, cScheme == ColorScheme.Custom, WaterfallEnhancer.PaletteSharpness, WaterfallEnhancer.PaletteContrast);
							}
							else
							{
								waterfallGPURenderer.ProcessRow(waterfall_data, num10, flag10 ? 1 : m_nDecimation, low_threshold, high_threshold, WaterfallEnhancer.Gamma, invGamma, _effectiveToneMap, WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.ContrastBoost, WaterfallEnhancer.DitherEnabled, WaterfallEnhancer.Levels, _effectiveTemporalAlpha, 0.05f, cScheme != ColorScheme.Custom, cScheme == ColorScheme.Custom, WaterfallEnhancer.PaletteSharpness, WaterfallEnhancer.PaletteContrast);
							}
						}
						waterfallGPURenderer.AdvanceRow(num19, flag14);
						if (flag14)
						{
							recordWaterfallAdvance(rx, H - 20);
						}
					}
					flag9 = true;
				}
				int nDecimation = m_nDecimation;
				bool flag15 = false;
				if (flag10 && !flag5)
				{
					nDecimation = m_nDecimation;
					m_nDecimation = 1;
					flag15 = true;
				}
				try
				{
					if (!flag9)
					{
						float num20 = 255f;
						int pixelSize = WaterfallPixelWriter.PixelSize;
						float[] array7 = new float[W * 4];
						byte[] array8 = new byte[W * pixelSize];
						bool flag16 = false;
						switch (cScheme)
						{
						case ColorScheme.Custom:
						{
							System.Drawing.Color[] array9;
							if (flag2)
							{
								if (!_tx_waterfall_grad_ok)
								{
									flag16 = true;
									break;
								}
								array9 = _tx_waterfall_grad;
							}
							else if (rx == 1)
							{
								if (!_rx1_waterfall_grad_ok)
								{
									flag16 = true;
									break;
								}
								array9 = _rx1_waterfall_grad;
							}
							else
							{
								if (!_rx2_waterfall_grad_ok)
								{
									flag16 = true;
									break;
								}
								array9 = _rx2_waterfall_grad;
							}
							int num47 = array9.Length;
							for (int num48 = 0; num48 < num10; num48++)
							{
								if (waterfall_data[num48] <= low_threshold)
								{
									num7 = (int)array9[0].R;
									num8 = (int)array9[0].G;
									num9 = (int)array9[0].B;
								}
								else if (waterfall_data[num48] >= high_threshold)
								{
									num7 = (int)array9[num47 - 1].R;
									num8 = (int)array9[num47 - 1].G;
									num9 = (int)array9[num47 - 1].B;
								}
								else
								{
									float num49 = high_threshold - low_threshold;
									int num50 = (int)(WaterfallEnhancer.ApplyGamma(WaterfallEnhancer.ApplyQualityPercent((waterfall_data[num48] - low_threshold) / num49)) * (float)(num47 - 1));
									if (num50 < 0)
									{
										num50 = 0;
									}
									else if (num50 >= num47)
									{
										num50 = num47 - 1;
									}
									num7 = (int)array9[num50].R;
									num8 = (int)array9[num50].G;
									num9 = (int)array9[num50].B;
								}
								if (waterfall_minimum > array4[num48] + num12)
								{
									waterfall_minimum = array4[num48] + num12;
								}
								int num51 = num48 * m_nDecimation * 4;
								array7[num51] = num7;
								array7[num51 + 1] = num8;
								array7[num51 + 2] = num9;
								array7[num51 + 3] = num20;
							}
							break;
						}
						case ColorScheme.enhanced:
						{
							for (int n = 0; n < num10; n++)
							{
								if (waterfall_data[n] <= low_threshold)
								{
									num7 = (int)low_color.R;
									num8 = (int)low_color.G;
									num9 = (int)low_color.B;
								}
								else if (waterfall_data[n] >= high_threshold)
								{
									num7 = 192f;
									num8 = 124f;
									num9 = 255f;
								}
								else
								{
									float num24 = high_threshold - low_threshold;
									float percent = (waterfall_data[n] - low_threshold) / num24;
									percent = WaterfallEnhancer.ApplyQualityPercent(percent);
									if (percent < 2f / 9f)
									{
										float num25 = percent / (2f / 9f);
										num7 = (float)((1.0 - (double)num25) * (double)(int)low_color.R);
										num8 = (float)((1.0 - (double)num25) * (double)(int)low_color.G);
										num9 = (float)(int)low_color.B + num25 * (float)(255 - low_color.B);
									}
									else if (percent < 1f / 3f)
									{
										float num26 = (percent - 2f / 9f) / (1f / 9f);
										num7 = 0f;
										num8 = num26 * 255f;
										num9 = 255f;
									}
									else if (percent < 4f / 9f)
									{
										float num27 = (percent - 1f / 3f) / (1f / 9f);
										num7 = 0f;
										num8 = 255f;
										num9 = (float)((1.0 - (double)num27) * 255.0);
									}
									else if (percent < 5f / 9f)
									{
										num7 = (percent - 4f / 9f) / (1f / 9f) * 255f;
										num8 = 255f;
										num9 = 0f;
									}
									else if (percent < 7f / 9f)
									{
										float num28 = (percent - 5f / 9f) / (2f / 9f);
										num7 = 255f;
										num8 = (float)((1.0 - (double)num28) * 255.0);
										num9 = 0f;
									}
									else if (percent < 8f / 9f)
									{
										float num29 = (percent - 7f / 9f) / (1f / 9f);
										num7 = 255f;
										num8 = 0f;
										num9 = num29 * 255f;
									}
									else
									{
										float num30 = (percent - 8f / 9f) / (1f / 9f);
										num7 = (float)((0.75 + 0.25 * (1.0 - (double)num30)) * 255.0);
										num8 = num30 * 255f * 0.5f;
										num9 = 255f;
									}
								}
								if (waterfall_minimum > array4[n] + num12)
								{
									waterfall_minimum = array4[n] + num12;
								}
								int num31 = n * m_nDecimation * 4;
								array7[num31] = num7;
								array7[num31 + 1] = num8;
								array7[num31 + 2] = num9;
								array7[num31 + 3] = num20;
							}
							break;
						}
						case ColorScheme.SPECTRAN:
						{
							for (int num36 = 0; num36 < num10; num36++)
							{
								if (waterfall_data[num36] <= low_threshold)
								{
									num7 = 0f;
									num8 = 0f;
									num9 = 0f;
								}
								else if (waterfall_data[num36] >= high_threshold)
								{
									num7 = 240f;
									num8 = 240f;
									num9 = 240f;
								}
								else
								{
									float num37 = high_threshold - low_threshold;
									float num38 = waterfall_data[num36] - low_threshold;
									float num39 = 100f * num38 / num37;
									num39 = WaterfallEnhancer.ApplyQualityPercent(num39 / 100f) * 100f;
									if (num39 < 5f)
									{
										num7 = (num8 = 0f);
										num9 = num39 * 5f;
									}
									else if (num39 < 11f)
									{
										num7 = (num8 = 0f);
										num9 = num39 * 5f;
									}
									else if (num39 < 22f)
									{
										num7 = (num8 = 0f);
										num9 = num39 * 5f;
									}
									else if (num39 < 44f)
									{
										num7 = (num8 = 0f);
										num9 = num39 * 5f;
									}
									else if (num39 < 51f)
									{
										num7 = (num8 = 0f);
										num9 = num39 * 5f;
									}
									else if (num39 < 66f)
									{
										num7 = (num8 = (num39 - 50f) * 2f);
										num9 = 255f;
									}
									else if (num39 < 77f)
									{
										num7 = (num8 = (num39 - 50f) * 3f);
										num9 = 255f;
									}
									else if (num39 < 88f)
									{
										num7 = (num8 = (num39 - 50f) * 4f);
										num9 = 255f;
									}
									else if (num39 < 99f)
									{
										num7 = (num8 = (num39 - 50f) * 5f);
										num9 = 255f;
									}
								}
								if (waterfall_minimum > array4[num36] + num12)
								{
									waterfall_minimum = array4[num36] + num12;
								}
								int num40 = num36 * m_nDecimation * 4;
								array7[num40] = num7;
								array7[num40 + 1] = num8;
								array7[num40 + 2] = num9;
								array7[num40 + 3] = num20;
							}
							break;
						}
						case ColorScheme.BLACKWHITE:
						{
							for (int num52 = 0; num52 < num10; num52++)
							{
								if (waterfall_data[num52] <= low_threshold)
								{
									num7 = 0f;
									num8 = 0f;
									num9 = 0f;
								}
								else if (waterfall_data[num52] >= high_threshold)
								{
									num7 = 255f;
									num8 = 255f;
									num9 = 255f;
								}
								else
								{
									float num53 = high_threshold - low_threshold;
									float num54 = waterfall_data[num52] - low_threshold;
									num7 = WaterfallEnhancer.ApplyQualityPercent(100f * num54 / num53 / 100f) * 100f / 100f * 255f;
									num8 = num7;
									num9 = num7;
								}
								if (waterfall_minimum > array4[num52] + num12)
								{
									waterfall_minimum = array4[num52] + num12;
								}
								int num55 = num52 * m_nDecimation * 4;
								array7[num55] = num7;
								array7[num55 + 1] = num8;
								array7[num55 + 2] = num9;
								array7[num55 + 3] = num20;
							}
							break;
						}
						case ColorScheme.LinLog:
						{
							for (int num41 = 0; num41 < num10; num41++)
							{
								if (waterfall_data[num41] <= low_threshold)
								{
									num7 = 0f;
									num8 = 0f;
									num9 = 0f;
								}
								else if (waterfall_data[num41] >= high_threshold)
								{
									num7 = 252f;
									num8 = 252f;
									num9 = 252f;
								}
								else
								{
									float num42 = high_threshold - low_threshold;
									float num43 = waterfall_data[num41] - low_threshold + (float)LinLogCor;
									float num44 = 1024f * num43 / num42;
									float num45 = (float)Math.Log10(1024.0);
									if (num44 == 0f)
									{
										num44 = 0.001f;
									}
									num44 = (float)Math.Log10(num44);
									if (num44 < num45 / 23f)
									{
										num7 = 0f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num44 < 2f * num45 / 23f)
									{
										num7 = 32f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num44 < 3f * num45 / 23f)
									{
										num7 = 64f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num44 < 4f * num45 / 23f)
									{
										num7 = 96f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num44 < 5f * num45 / 23f)
									{
										num7 = 104f;
										num8 = 40f;
										num9 = 0f;
									}
									else if (num44 < 6f * num45 / 23f)
									{
										num7 = 112f;
										num8 = 60f;
										num9 = 0f;
									}
									else if (num44 < 7f * num45 / 23f)
									{
										num7 = 116f;
										num8 = 88f;
										num9 = 0f;
									}
									else if (num44 < 8f * num45 / 23f)
									{
										num7 = 92f;
										num8 = 112f;
										num9 = 0f;
									}
									else if (num44 < 9f * num45 / 23f)
									{
										num7 = 80f;
										num8 = 132f;
										num9 = 0f;
									}
									else if (num44 < 10f * num45 / 23f)
									{
										num7 = 20f;
										num8 = 140f;
										num9 = 0f;
									}
									else if (num44 < 11f * num45 / 23f)
									{
										num7 = 0f;
										num8 = 160f;
										num9 = 40f;
									}
									else if (num44 < 12f * num45 / 23f)
									{
										num7 = 0f;
										num8 = 160f;
										num9 = 120f;
									}
									else if (num44 < 13f * num45 / 23f)
									{
										num7 = 0f;
										num8 = 140f;
										num9 = 148f;
									}
									else if (num44 < 14f * num45 / 23f)
									{
										num7 = 0f;
										num8 = 132f;
										num9 = 192f;
									}
									else if (num44 < 15f * num45 / 23f)
									{
										num7 = 0f;
										num8 = 112f;
										num9 = 200f;
									}
									else if (num44 < 16f * num45 / 23f)
									{
										num7 = 0f;
										num8 = 88f;
										num9 = 208f;
									}
									else if (num44 < 17f * num45 / 23f)
									{
										num7 = 0f;
										num8 = 60f;
										num9 = 232f;
									}
									else if (num44 < 18f * num45 / 23f)
									{
										num7 = 0f;
										num8 = 40f;
										num9 = 252f;
									}
									else if (num44 < 19f * num45 / 23f)
									{
										num7 = 80f;
										num8 = 80f;
										num9 = 252f;
									}
									else if (num44 < 20f * num45 / 23f)
									{
										num7 = 124f;
										num8 = 124f;
										num9 = 252f;
									}
									else if (num44 < 21f * num45 / 23f)
									{
										num7 = 172f;
										num8 = 172f;
										num9 = 252f;
									}
									else if (num44 >= 21f * num45 / 23f)
									{
										num7 = 252f;
										num8 = 252f;
										num9 = 252f;
									}
									else
									{
										num7 = 0f;
										num8 = 0f;
										num9 = 0f;
									}
								}
								if (waterfall_minimum > array4[num41] + num12)
								{
									waterfall_minimum = array4[num41] + num12;
								}
								int num46 = num41 * m_nDecimation * 4;
								array7[num46] = num7;
								array7[num46 + 1] = num8;
								array7[num46 + 2] = num9;
								array7[num46 + 3] = num20;
							}
							break;
						}
						case ColorScheme.LinRad:
						{
							for (int num32 = 0; num32 < num10; num32++)
							{
								if (waterfall_data[num32] <= low_threshold)
								{
									num7 = 0f;
									num8 = 0f;
									num9 = 0f;
								}
								else if (waterfall_data[num32] >= high_threshold)
								{
									num7 = 252f;
									num8 = 252f;
									num9 = 252f;
								}
								else
								{
									float num33 = high_threshold - low_threshold;
									float num34 = (waterfall_data[num32] - low_threshold + (float)LinCor) / num33;
									if (num34 < 1f / 23f)
									{
										num7 = 0f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num34 < 0.08695652f)
									{
										num7 = 32f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num34 < 0.13043478f)
									{
										num7 = 64f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num34 < 0.17391305f)
									{
										num7 = 96f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num34 < 0.2173913f)
									{
										num7 = 104f;
										num8 = 40f;
										num9 = 0f;
									}
									else if (num34 < 0.26086956f)
									{
										num7 = 112f;
										num8 = 60f;
										num9 = 0f;
									}
									else if (num34 < 0.3043478f)
									{
										num7 = 116f;
										num8 = 88f;
										num9 = 0f;
									}
									else if (num34 < 0.3478261f)
									{
										num7 = 92f;
										num8 = 112f;
										num9 = 0f;
									}
									else if (num34 < 0.39130434f)
									{
										num7 = 80f;
										num8 = 132f;
										num9 = 0f;
									}
									else if (num34 < 0.4347826f)
									{
										num7 = 20f;
										num8 = 140f;
										num9 = 0f;
									}
									else if (num34 < 0.47826087f)
									{
										num7 = 0f;
										num8 = 160f;
										num9 = 40f;
									}
									else if (num34 < 0.5217391f)
									{
										num7 = 0f;
										num8 = 160f;
										num9 = 120f;
									}
									else if (num34 < 0.5652174f)
									{
										num7 = 0f;
										num8 = 140f;
										num9 = 148f;
									}
									else if (num34 < 0.6086956f)
									{
										num7 = 0f;
										num8 = 132f;
										num9 = 192f;
									}
									else if (num34 < 0.65217394f)
									{
										num7 = 0f;
										num8 = 112f;
										num9 = 200f;
									}
									else if (num34 < 0.6956522f)
									{
										num7 = 0f;
										num8 = 88f;
										num9 = 208f;
									}
									else if (num34 < 0.73913044f)
									{
										num7 = 0f;
										num8 = 60f;
										num9 = 232f;
									}
									else if (num34 < 0.7826087f)
									{
										num7 = 0f;
										num8 = 40f;
										num9 = 252f;
									}
									else if (num34 < 0.82608694f)
									{
										num7 = 80f;
										num8 = 80f;
										num9 = 252f;
									}
									else if (num34 < 0.8695652f)
									{
										num7 = 124f;
										num8 = 124f;
										num9 = 252f;
									}
									else if (num34 < 0.9130435f)
									{
										num7 = 172f;
										num8 = 172f;
										num9 = 252f;
									}
									else if (num34 >= 0.9130435f)
									{
										num7 = 252f;
										num8 = 252f;
										num9 = 252f;
									}
									else
									{
										num7 = 0f;
										num8 = 0f;
										num9 = 0f;
									}
								}
								if (waterfall_minimum > array4[num32] + num12)
								{
									waterfall_minimum = array4[num32] + num12;
								}
								int num35 = num32 * m_nDecimation * 4;
								array7[num35] = num7;
								array7[num35 + 1] = num8;
								array7[num35 + 2] = num9;
								array7[num35 + 3] = num20;
							}
							break;
						}
						case ColorScheme.LinAuto:
						{
							for (int m = 0; m < num10; m++)
							{
								num4 = num6 - 5f;
								num5 = max_y;
								if (waterfall_data[m] <= num4)
								{
									num7 = 0f;
									num8 = 0f;
									num9 = 0f;
								}
								else if (waterfall_data[m] >= num5)
								{
									num7 = 252f;
									num8 = 252f;
									num9 = 252f;
								}
								else
								{
									float num21 = num5 - num4;
									float num22 = (waterfall_data[m] - num4) / num21;
									if (num22 < 1f / 23f)
									{
										num7 = 0f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num22 < 0.08695652f)
									{
										num7 = 32f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num22 < 0.13043478f)
									{
										num7 = 64f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num22 < 0.17391305f)
									{
										num7 = 96f;
										num8 = 0f;
										num9 = 0f;
									}
									else if (num22 < 0.2173913f)
									{
										num7 = 104f;
										num8 = 40f;
										num9 = 0f;
									}
									else if (num22 < 0.26086956f)
									{
										num7 = 112f;
										num8 = 60f;
										num9 = 0f;
									}
									else if (num22 < 0.3043478f)
									{
										num7 = 116f;
										num8 = 88f;
										num9 = 0f;
									}
									else if (num22 < 0.3478261f)
									{
										num7 = 92f;
										num8 = 112f;
										num9 = 0f;
									}
									else if (num22 < 0.39130434f)
									{
										num7 = 80f;
										num8 = 132f;
										num9 = 0f;
									}
									else if (num22 < 0.4347826f)
									{
										num7 = 20f;
										num8 = 140f;
										num9 = 0f;
									}
									else if (num22 < 0.47826087f)
									{
										num7 = 0f;
										num8 = 160f;
										num9 = 40f;
									}
									else if (num22 < 0.5217391f)
									{
										num7 = 0f;
										num8 = 160f;
										num9 = 120f;
									}
									else if (num22 < 0.5652174f)
									{
										num7 = 0f;
										num8 = 140f;
										num9 = 148f;
									}
									else if (num22 < 0.6086956f)
									{
										num7 = 0f;
										num8 = 132f;
										num9 = 192f;
									}
									else if (num22 < 0.65217394f)
									{
										num7 = 0f;
										num8 = 112f;
										num9 = 200f;
									}
									else if (num22 < 0.6956522f)
									{
										num7 = 0f;
										num8 = 88f;
										num9 = 208f;
									}
									else if (num22 < 0.73913044f)
									{
										num7 = 0f;
										num8 = 60f;
										num9 = 232f;
									}
									else if (num22 < 0.7826087f)
									{
										num7 = 0f;
										num8 = 40f;
										num9 = 252f;
									}
									else if (num22 < 0.82608694f)
									{
										num7 = 80f;
										num8 = 80f;
										num9 = 252f;
									}
									else if (num22 < 0.8695652f)
									{
										num7 = 124f;
										num8 = 124f;
										num9 = 252f;
									}
									else if (num22 < 0.9130435f)
									{
										num7 = 172f;
										num8 = 172f;
										num9 = 252f;
									}
									else if (num22 >= 0.9130435f)
									{
										num7 = 252f;
										num8 = 252f;
										num9 = 252f;
									}
									else
									{
										num7 = 0f;
										num8 = 0f;
										num9 = 0f;
									}
								}
								int num23 = m * m_nDecimation * 4;
								array7[num23] = num7;
								array7[num23 + 1] = num8;
								array7[num23 + 2] = num9;
								array7[num23 + 3] = num20;
							}
							break;
						}
						case ColorScheme.Console:
						case ColorScheme.Thermal:
						case ColorScheme.DeepBlue:
						case ColorScheme.Enhanced256:
						case ColorScheme.Grayscale256:
							DrawPaletteScheme(GetPaletteForScheme(cScheme), waterfall_data, array4, num12, ref waterfall_minimum, low_threshold, high_threshold, num10, m_nDecimation, array7, num20);
							break;
						}
						if (!flag16)
						{
							for (int num56 = 0; num56 < num10; num56++)
							{
								int num57 = num56 * m_nDecimation * 4;
								for (int num58 = 1; num58 < m_nDecimation; num58++)
								{
									int num59 = (num56 * m_nDecimation + num58) * 4;
									array7[num59] = array7[num57];
									array7[num59 + 1] = array7[num57 + 1];
									array7[num59 + 2] = array7[num57 + 2];
									array7[num59 + 3] = array7[num57 + 3];
								}
							}
							if (_effectiveTemporalAlpha > 0f)
							{
								int num60 = W * 4;
								if (_temporalPrevRowF == null || _temporalPrevRowF.Length < num60)
								{
									_temporalPrevRowF = new float[num60];
									_temporalPrevValid = false;
								}
								if (_temporalPrevValid)
								{
									float effectiveTemporalAlpha = _effectiveTemporalAlpha;
									for (int num61 = 0; num61 < num60; num61 += 4)
									{
										float num62 = array7[num61] - _temporalPrevRowF[num61];
										float num63 = array7[num61 + 1] - _temporalPrevRowF[num61 + 1];
										float num64 = array7[num61 + 2] - _temporalPrevRowF[num61 + 2];
										float num65 = 0.299f * num62 + 0.587f * num63 + 0.114f * num64;
										float num66 = num65 * num65;
										float num67;
										if (num66 < 225f)
										{
											num67 = effectiveTemporalAlpha;
										}
										else
										{
											float num68 = (float)Math.Sqrt(num66);
											num67 = effectiveTemporalAlpha * (15f / num68);
										}
										float num69 = 1f - num67;
										array7[num61] = array7[num61] * num69 + _temporalPrevRowF[num61] * num67;
										array7[num61 + 1] = array7[num61 + 1] * num69 + _temporalPrevRowF[num61 + 1] * num67;
										array7[num61 + 2] = array7[num61 + 2] * num69 + _temporalPrevRowF[num61 + 2] * num67;
										array7[num61 + 3] = array7[num61 + 3] * num69 + _temporalPrevRowF[num61 + 3] * num67;
									}
								}
								Array.Copy(array7, _temporalPrevRowF, num60);
								_temporalPrevValid = true;
							}
							bool num70 = WaterfallEnhancer.SaturationBoost > 0f || WaterfallEnhancer.ContrastBoost > 0f;
							bool flag17 = _effectiveToneMap != 0;
							bool flag18 = _gpuEffectsEnabled && WaterfallEnhancer.Depth == WaterfallEnhancer.ColorDepth.Bit8;
							bool flag19 = flag18 && GPUDetector.HasCustomShaders;
							bool flag20 = (flag18 && GPUDetector.HasBuiltInEffects && !GPUDetector.HasCustomShaders) | flag19;
							bool flag21 = num70 && !flag20;
							bool flag22 = WaterfallEnhancer.Gamma != 1f && !flag20;
							bool flag23 = flag17 && !flag19;
							bool flag24 = WaterfallEnhancer.DitherEnabled && !flag19;
							if ((!flag18 | flag23 | flag21 | flag22 | flag24) && (flag21 | flag23 | flag24 | flag22))
							{
								int y = _ditherFrameY & 7;
								for (int num71 = 0; num71 < W; num71++)
								{
									int num72 = num71 * 4;
									if (flag21)
									{
										WaterfallEnhancer.ApplySaturationContrast(array7, num72);
									}
									if (flag23)
									{
										array7[num72] = WaterfallEnhancer.ApplyToneMapMode(array7[num72], _effectiveToneMap);
										array7[num72 + 1] = WaterfallEnhancer.ApplyToneMapMode(array7[num72 + 1], _effectiveToneMap);
										array7[num72 + 2] = WaterfallEnhancer.ApplyToneMapMode(array7[num72 + 2], _effectiveToneMap);
									}
									if (flag22)
									{
										array7[num72] = WaterfallEnhancer.ApplyGammaFloat(array7[num72]);
										array7[num72 + 1] = WaterfallEnhancer.ApplyGammaFloat(array7[num72 + 1]);
										array7[num72 + 2] = WaterfallEnhancer.ApplyGammaFloat(array7[num72 + 2]);
									}
									if (flag24)
									{
										array7[num72] = WaterfallEnhancer.ApplyDitherFloat(array7[num72], num71, y);
										array7[num72 + 1] = WaterfallEnhancer.ApplyDitherFloat(array7[num72 + 1], num71, y);
										array7[num72 + 2] = WaterfallEnhancer.ApplyDitherFloat(array7[num72 + 2], num71, y);
									}
								}
							}
							WaterfallPixelWriter.EncodeRow8Forced(array7, array8, W);
							_ditherFrameY++;
						}
						if (!flag13)
						{
							if (flag)
							{
								bitmap.CopyFromMemory(array8, W * pixelSize, new SharpDX.Rectangle(0, 0, W, 1));
							}
							int num73 = W - Math.Abs(num19);
							int y2 = (flag ? 1 : 0);
							if (num73 > 0)
							{
								int x = ((num19 < 0) ? (-num19) : 0);
								int x2 = ((num19 > 0) ? num19 : 0);
								bitmap.CopyFromBitmap(comObject, new SharpDX.Point(x2, y2), new SharpDX.Rectangle(x, 0, num73, height));
							}
							if (num19 > 0)
							{
								clearWaterfallBitmapRegion(bitmap, 0, y2, num19, height);
							}
							else if (num19 < 0)
							{
								clearWaterfallBitmapRegion(bitmap, W + num19, y2, -num19, height);
							}
							else if (num73 <= 0)
							{
								clearWaterfallBitmapRegion(bitmap, 0, y2, W, height);
							}
							if (flag)
							{
								recordWaterfallAdvance(rx, H - 20);
							}
						}
					}
				}
				finally
				{
					if (flag15)
					{
						m_nDecimation = nDecimation;
					}
				}
				Utilities.Dispose(ref comObject);
				comObject = null;
				bool flag25 = (rx == 1 && _ignore_waterfall_rx1_agc && _high_perf_timer.ElapsedMsec < _rx1_no_agc_duration) || (rx == 2 && _ignore_waterfall_rx2_agc && _high_perf_timer.ElapsedMsec < _rx2_no_agc_duration);
				if (!flag25)
				{
					if (rx == 1)
					{
						_ignore_waterfall_rx1_agc = false;
					}
					else
					{
						_ignore_waterfall_rx2_agc = false;
					}
				}
				if (!flag2 && !flag25)
				{
					if (rx == 1)
					{
						float wfAgcSmoothing = _wfAgcSmoothing;
						if ((rx1_waterfall_agc && !m_bRX1_spectrum_thresholds) & useNoiseFloorCompensation)
						{
							_RX1waterfallPreviousMinValue = _RX1waterfallPreviousMinValue * (1f - wfAgcSmoothing) + noiseFloorCompensationTarget * wfAgcSmoothing;
						}
						else
						{
							_RX1waterfallPreviousMinValue = _RX1waterfallPreviousMinValue * (1f - wfAgcSmoothing) + waterfall_minimum * wfAgcSmoothing;
						}
					}
					else
					{
						float wfAgcSmoothing2 = _wfAgcSmoothing;
						if ((rx2_waterfall_agc && !m_bRX2_spectrum_thresholds) & useNoiseFloorCompensation)
						{
							_RX2waterfallPreviousMinValue = _RX2waterfallPreviousMinValue * (1f - wfAgcSmoothing2) + noiseFloorCompensationTarget * wfAgcSmoothing2;
						}
						else
						{
							_RX2waterfallPreviousMinValue = _RX2waterfallPreviousMinValue * (1f - wfAgcSmoothing2) + waterfall_minimum * wfAgcSmoothing2;
						}
					}
				}
				if (!flag2)
				{
					updateWaterfallAgcCache(rx, isTxContext: false, (rx == 1) ? _RX1waterfallPreviousMinValue : _RX2waterfallPreviousMinValue);
				}
				else
				{
					updateWaterfallAgcCache(rx, isTxContext: true, (rx == 1) ? _RX1waterfallPreviousMinValue : _RX2waterfallPreviousMinValue);
				}
			}
			if (flag5 && waterfallGPURenderer != null)
			{
				waterfallGPURenderer.Draw(0, nVerticalShift + 20, (rx == 1) ? m_fRX1WaterfallOpacity : m_fRX2WaterfallOpacity, null, _gpuWaterfallLinearDraw);
			}
			else if (rx == 1)
			{
				DrawWaterfallToTarget(_waterfall_bmp_dx2d, nVerticalShift, 20, m_fRX1WaterfallOpacity);
			}
			else
			{
				DrawWaterfallToTarget(_waterfall_bmp2_dx2d, nVerticalShift, 20, m_fRX2WaterfallOpacity);
			}
		}
		_d2dRenderTarget.Transform = matrix3x;
		drawPanadapterAndWaterfallGridDX2D(nVerticalShift, W, H, rx, bottom, out var _, out var _, bIsWaterfall: true);
		if (console.PowerOn)
		{
			drawWaterfallTimeOverlay(nVerticalShift, W, H, rx);
		}
		return true;
	}

	private static Color4 convertColour(System.Drawing.Color c)
	{
		float red = (float)(int)c.R / 255f;
		float green = (float)(int)c.G / 255f;
		float blue = (float)(int)c.B / 255f;
		float alpha = (float)(int)c.A / 255f;
		return new Color4(red, green, blue, alpha);
	}

	private static SolidColorBrush convertBrush(SolidBrush b)
	{
		return new SolidColorBrush(_d2dRenderTarget, convertColour(b.Color));
	}

	public static void SetDX2BackgoundImage(System.Drawing.Image image)
	{
		lock (_objDX2Lock)
		{
			if (!_bDX2Setup)
			{
				return;
			}
			if (_bitmapBackground != null)
			{
				Utilities.Dispose(ref _bitmapBackground);
				_bitmapBackground = null;
			}
			_backgroundImageSource = image;
			if (image != null)
			{
				try
				{
					if (image.Width > 0 && image.Height > 0)
					{
						using System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image);
						if (bitmap.Width > 0 && bitmap.Height > 0)
						{
							_bitmapBackground = SDXBitmapFromSysBitmap(_d2dRenderTarget, bitmap);
						}
					}
				}
				catch
				{
				}
			}
			if (_detachedRenderer == null)
			{
				return;
			}
			try
			{
				_detachedRenderer.OnBackgroundChanged();
			}
			catch
			{
			}
		}
	}

	private static SharpDX.Direct2D1.Bitmap SDXBitmapFromSysBitmap(RenderTarget rt, System.Drawing.Bitmap bitmap)
	{
		return SDXBitmapFromSysBitmapCore(rt, bitmap);
	}

	internal static SharpDX.Direct2D1.Bitmap SDXBitmapFromSysBitmapPublic(RenderTarget rt, System.Drawing.Bitmap bitmap)
	{
		return SDXBitmapFromSysBitmapCore(rt, bitmap);
	}

	private static SharpDX.Direct2D1.Bitmap SDXBitmapFromSysBitmapCore(RenderTarget rt, System.Drawing.Bitmap bitmap)
	{
		System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
		Size2 size = new Size2(bitmap.Width, bitmap.Height);
		BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
		_ = bitmap.Width;
		_ = bitmap.Height;
		SharpDX.Direct2D1.Bitmap result;
		if (WaterfallPixelWriter.PixelSize == 8)
		{
			int num = bitmap.Width * 8;
			DataStream comObject = new DataStream(bitmap.Height * num, canRead: true, canWrite: true);
			for (int i = 0; i < bitmap.Height; i++)
			{
				IntPtr ptr = IntPtr.Add(bitmapData.Scan0, bitmapData.Stride * i);
				int num2 = 0;
				for (int j = 0; j < bitmap.Width; j++)
				{
					byte b = Marshal.ReadByte(ptr, num2++);
					byte b2 = Marshal.ReadByte(ptr, num2++);
					byte b3 = Marshal.ReadByte(ptr, num2++);
					byte num3 = Marshal.ReadByte(ptr, num2++);
					float value = (float)(int)b3 / 255f;
					float value2 = (float)(int)b2 / 255f;
					float value3 = (float)(int)b / 255f;
					float value4 = (float)(int)num3 / 255f;
					WriteHalf(comObject, value);
					WriteHalf(comObject, value2);
					WriteHalf(comObject, value3);
					WriteHalf(comObject, value4);
				}
			}
			bitmap.UnlockBits(bitmapData);
			comObject.Position = 0L;
			result = new SharpDX.Direct2D1.Bitmap(bitmapProperties: new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(WaterfallPixelWriter.DxgiFormat, BitmapAlphaModeForFormat(WaterfallPixelWriter.DxgiFormat))), renderTarget: rt, size: size, dataPointer: comObject, pitch: num);
			Utilities.Dispose(ref comObject);
		}
		else
		{
			int num4 = bitmap.Width * 4;
			DataStream comObject2 = new DataStream(bitmap.Height * num4, canRead: true, canWrite: true);
			for (int k = 0; k < bitmap.Height; k++)
			{
				IntPtr ptr2 = IntPtr.Add(bitmapData.Scan0, bitmapData.Stride * k);
				int num5 = 0;
				for (int l = 0; l < bitmap.Width; l++)
				{
					byte b4 = Marshal.ReadByte(ptr2, num5++);
					byte b5 = Marshal.ReadByte(ptr2, num5++);
					byte b6 = Marshal.ReadByte(ptr2, num5++);
					byte b7 = Marshal.ReadByte(ptr2, num5++);
					int value5 = b4 | (b5 << 8) | (b6 << 16) | (b7 << 24);
					comObject2.Write(value5);
				}
			}
			bitmap.UnlockBits(bitmapData);
			comObject2.Position = 0L;
			result = new SharpDX.Direct2D1.Bitmap(bitmapProperties: new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(WaterfallPixelWriter.DxgiFormat, BitmapAlphaModeForFormat(WaterfallPixelWriter.DxgiFormat))), renderTarget: rt, size: size, dataPointer: comObject2, pitch: num4);
			Utilities.Dispose(ref comObject2);
		}
		return result;
	}

	private static void WriteHalf(DataStream s, float value)
	{
		ushort num = WaterfallPixelWriter.FloatToHalfBitsPublic(value);
		s.Write((byte)(num & 0xFF));
		s.Write((byte)((num >> 8) & 0xFF));
	}

	private static void buildLinearGradientBrush(int top, int bottom, int rx)
	{
		int num;
		int num2;
		if (rx == 1)
		{
			num = spectrum_grid_min;
			num2 = spectrum_grid_max;
		}
		else
		{
			num = rx2_spectrum_grid_min;
			num2 = rx2_spectrum_grid_max;
		}
		List<ucLGPicker.ColourGradientData> colourGradientDataForDBMRange = console.SetupForm.RX1GradPicker.GetColourGradientDataForDBMRange(num, num2);
		GradientStop[] array = new GradientStop[colourGradientDataForDBMRange.Count];
		GradientStop[] array2 = new GradientStop[colourGradientDataForDBMRange.Count];
		for (int i = 0; i < colourGradientDataForDBMRange.Count; i++)
		{
			System.Drawing.Color c = System.Drawing.Color.FromArgb(data_fill_color.A, colourGradientDataForDBMRange[i].color.R, colourGradientDataForDBMRange[i].color.G, colourGradientDataForDBMRange[i].color.B);
			System.Drawing.Color c2 = System.Drawing.Color.FromArgb(data_line_color.A, colourGradientDataForDBMRange[i].color.R, colourGradientDataForDBMRange[i].color.G, colourGradientDataForDBMRange[i].color.B);
			array[i] = new GradientStop
			{
				Color = convertColour(c),
				Position = colourGradientDataForDBMRange[i].percent
			};
			array2[i] = new GradientStop
			{
				Color = convertColour(c2),
				Position = colourGradientDataForDBMRange[i].percent
			};
		}
		GradientStopCollection comObject = new GradientStopCollection(_d2dRenderTarget, array);
		GradientStopCollection comObject2 = new GradientStopCollection(_d2dRenderTarget, array2);
		if (rx == 1)
		{
			if (m_brushLGDataFillRX1 != null)
			{
				Utilities.Dispose(ref m_brushLGDataFillRX1);
				m_brushLGDataFillRX1 = null;
			}
			m_brushLGDataFillRX1 = new LinearGradientBrush(_d2dRenderTarget, new LinearGradientBrushProperties
			{
				StartPoint = new Vector2(0f, bottom),
				EndPoint = new Vector2(0f, top)
			}, comObject);
			if (m_brushLGDataLineRX1 != null)
			{
				Utilities.Dispose(ref m_brushLGDataLineRX1);
				m_brushLGDataLineRX1 = null;
			}
			m_brushLGDataLineRX1 = new LinearGradientBrush(_d2dRenderTarget, new LinearGradientBrushProperties
			{
				StartPoint = new Vector2(0f, bottom),
				EndPoint = new Vector2(0f, top)
			}, comObject2);
		}
		else
		{
			if (m_brushLGDataFillRX2 != null)
			{
				Utilities.Dispose(ref m_brushLGDataFillRX2);
				m_brushLGDataFillRX2 = null;
			}
			m_brushLGDataFillRX2 = new LinearGradientBrush(_d2dRenderTarget, new LinearGradientBrushProperties
			{
				StartPoint = new Vector2(0f, bottom),
				EndPoint = new Vector2(0f, top)
			}, comObject);
			if (m_brushLGDataLineRX2 != null)
			{
				Utilities.Dispose(ref m_brushLGDataLineRX2);
				m_brushLGDataLineRX2 = null;
			}
			m_brushLGDataLineRX2 = new LinearGradientBrush(_d2dRenderTarget, new LinearGradientBrushProperties
			{
				StartPoint = new Vector2(0f, bottom),
				EndPoint = new Vector2(0f, top)
			}, comObject2);
		}
		Utilities.Dispose(ref comObject);
		Utilities.Dispose(ref comObject2);
		comObject = null;
		comObject2 = null;
	}

	private static void buildLinearGradientBrushTX(int top, int bottom, int rx)
	{
		int num = tx_spectrum_grid_min;
		int num2 = tx_spectrum_grid_max;
		List<ucLGPicker.ColourGradientData> colourGradientDataForDBMRange = console.SetupForm.TXGradPicker.GetColourGradientDataForDBMRange(num, num2);
		GradientStop[] array = new GradientStop[colourGradientDataForDBMRange.Count];
		GradientStop[] array2 = new GradientStop[colourGradientDataForDBMRange.Count];
		for (int i = 0; i < colourGradientDataForDBMRange.Count; i++)
		{
			System.Drawing.Color c = System.Drawing.Color.FromArgb(data_fill_color_tx.A, colourGradientDataForDBMRange[i].color.R, colourGradientDataForDBMRange[i].color.G, colourGradientDataForDBMRange[i].color.B);
			System.Drawing.Color c2 = System.Drawing.Color.FromArgb(tx_data_line_color.A, colourGradientDataForDBMRange[i].color.R, colourGradientDataForDBMRange[i].color.G, colourGradientDataForDBMRange[i].color.B);
			array[i] = new GradientStop
			{
				Color = convertColour(c),
				Position = colourGradientDataForDBMRange[i].percent
			};
			array2[i] = new GradientStop
			{
				Color = convertColour(c2),
				Position = colourGradientDataForDBMRange[i].percent
			};
		}
		GradientStopCollection comObject = new GradientStopCollection(_d2dRenderTarget, array);
		GradientStopCollection comObject2 = new GradientStopCollection(_d2dRenderTarget, array2);
		if (rx == 1)
		{
			if (m_brushLGDataFillTX_RX1 != null)
			{
				Utilities.Dispose(ref m_brushLGDataFillTX_RX1);
				m_brushLGDataFillTX_RX1 = null;
			}
			m_brushLGDataFillTX_RX1 = new LinearGradientBrush(_d2dRenderTarget, new LinearGradientBrushProperties
			{
				StartPoint = new Vector2(0f, bottom),
				EndPoint = new Vector2(0f, top)
			}, comObject);
			if (m_brushLGDataLineTX_RX1 != null)
			{
				Utilities.Dispose(ref m_brushLGDataLineTX_RX1);
				m_brushLGDataLineTX_RX1 = null;
			}
			m_brushLGDataLineTX_RX1 = new LinearGradientBrush(_d2dRenderTarget, new LinearGradientBrushProperties
			{
				StartPoint = new Vector2(0f, bottom),
				EndPoint = new Vector2(0f, top)
			}, comObject2);
		}
		else
		{
			if (m_brushLGDataFillTX_RX2 != null)
			{
				Utilities.Dispose(ref m_brushLGDataFillTX_RX2);
				m_brushLGDataFillTX_RX2 = null;
			}
			m_brushLGDataFillTX_RX2 = new LinearGradientBrush(_d2dRenderTarget, new LinearGradientBrushProperties
			{
				StartPoint = new Vector2(0f, bottom),
				EndPoint = new Vector2(0f, top)
			}, comObject);
			if (m_brushLGDataLineTX_RX2 != null)
			{
				Utilities.Dispose(ref m_brushLGDataLineTX_RX2);
				m_brushLGDataLineTX_RX2 = null;
			}
			m_brushLGDataLineTX_RX2 = new LinearGradientBrush(_d2dRenderTarget, new LinearGradientBrushProperties
			{
				StartPoint = new Vector2(0f, bottom),
				EndPoint = new Vector2(0f, top)
			}, comObject2);
		}
		Utilities.Dispose(ref comObject);
		Utilities.Dispose(ref comObject2);
		comObject = null;
		comObject2 = null;
	}

	private static void releaseDX2Resources()
	{
		clearAllDynamicBrushes();
		clearSpotFlagBitmapCache();
		if (m_brushLGDataFillRX1 != null)
		{
			Utilities.Dispose(ref m_brushLGDataFillRX1);
		}
		if (m_brushLGDataFillRX2 != null)
		{
			Utilities.Dispose(ref m_brushLGDataFillRX2);
		}
		if (m_brushLGDataLineRX1 != null)
		{
			Utilities.Dispose(ref m_brushLGDataLineRX1);
		}
		if (m_brushLGDataLineRX2 != null)
		{
			Utilities.Dispose(ref m_brushLGDataLineRX2);
		}
		_bRebuildRXLinearGradBrush = false;
		if (m_brushLGDataFillTX_RX1 != null)
		{
			Utilities.Dispose(ref m_brushLGDataFillTX_RX1);
		}
		if (m_brushLGDataLineTX_RX1 != null)
		{
			Utilities.Dispose(ref m_brushLGDataLineTX_RX1);
		}
		if (m_brushLGDataFillTX_RX2 != null)
		{
			Utilities.Dispose(ref m_brushLGDataFillTX_RX2);
		}
		if (m_brushLGDataLineTX_RX2 != null)
		{
			Utilities.Dispose(ref m_brushLGDataLineTX_RX2);
		}
		_bRebuildTXLinearGradBrush = false;
		if (m_bDX2_dataPeaks_fill_fpen_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_dataPeaks_fill_fpen_brush);
		}
		if (m_bDX2_data_fill_fpen_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_data_fill_fpen_brush);
		}
		if (m_bDX2_data_fill_fpen_brush_tx != null)
		{
			Utilities.Dispose(ref m_bDX2_data_fill_fpen_brush_tx);
		}
		if (m_bDX2_data_line_pen_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_data_line_pen_brush);
		}
		if (m_bDX2_data_line_pen_brush_tx != null)
		{
			Utilities.Dispose(ref m_bDX2_data_line_pen_brush_tx);
		}
		if (m_bDX2_tx_data_line_fpen_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_data_line_fpen_brush);
		}
		if (m_bDX2_tx_data_line_pen_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_data_line_pen_brush);
		}
		if (m_bDX2_p1 != null)
		{
			Utilities.Dispose(ref m_bDX2_p1);
		}
		if (m_bDX2_display_background_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_display_background_brush);
		}
		if (m_bDX2_grid_tx_text_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_grid_tx_text_brush);
		}
		if (m_bDX2_grid_text_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_grid_text_brush);
		}
		if (m_bDX2_pana_text_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_pana_text_brush);
		}
		if (m_bDX2_bandstack_overlay_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_bandstack_overlay_brush);
		}
		if (m_bDX2_bandstack_overlay_brush_lines != null)
		{
			Utilities.Dispose(ref m_bDX2_bandstack_overlay_brush_lines);
		}
		if (m_bDX2_bandstack_overlay_brush_highlight != null)
		{
			Utilities.Dispose(ref m_bDX2_bandstack_overlay_brush_highlight);
		}
		if (m_bDX2_display_filter_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_display_filter_brush);
		}
		if (m_bDX2_tx_filter_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_filter_brush);
		}
		if (m_bDX2_m_bTextCallOutActive != null)
		{
			Utilities.Dispose(ref m_bDX2_m_bTextCallOutActive);
		}
		if (m_bDX2_m_bTextCallOutInactive != null)
		{
			Utilities.Dispose(ref m_bDX2_m_bTextCallOutInactive);
		}
		if (m_bDX2_m_pHighlighted != null)
		{
			Utilities.Dispose(ref m_bDX2_m_pHighlighted);
		}
		if (m_bDX2_m_bBWHighlighedFillColour != null)
		{
			Utilities.Dispose(ref m_bDX2_m_bBWHighlighedFillColour);
		}
		if (m_bDX2_tx_band_edge_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_band_edge_pen);
		}
		if (m_bDX2_tx_vgrid_pen_inb != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_vgrid_pen_inb);
		}
		if (m_bDX2_band_edge_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_band_edge_pen);
		}
		if (m_bDX2_grid_pen_inb != null)
		{
			Utilities.Dispose(ref m_bDX2_grid_pen_inb);
		}
		if (m_bDX2_sub_rx_filter_brush != null)
		{
			Utilities.Dispose(ref m_bDX2_sub_rx_filter_brush);
		}
		if (m_bDX2_sub_rx_zero_line_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_sub_rx_zero_line_pen);
		}
		if (m_bDX2_tx_filter_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_filter_pen);
		}
		if (m_bDX2_cw_zero_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_cw_zero_pen);
		}
		if (m_bDX2_m_pNotchActive != null)
		{
			Utilities.Dispose(ref m_bDX2_m_pNotchActive);
		}
		if (m_bDX2_m_bBWFillColour != null)
		{
			Utilities.Dispose(ref m_bDX2_m_bBWFillColour);
		}
		if (m_bDX2_m_pNotchInactive != null)
		{
			Utilities.Dispose(ref m_bDX2_m_pNotchInactive);
		}
		if (m_bDX2_m_bBWFillColourInactive != null)
		{
			Utilities.Dispose(ref m_bDX2_m_bBWFillColourInactive);
		}
		if (m_bDX2_m_pTNFInactive != null)
		{
			Utilities.Dispose(ref m_bDX2_m_pTNFInactive);
		}
		if (m_bDX2_m_bTNFInactive != null)
		{
			Utilities.Dispose(ref m_bDX2_m_bTNFInactive);
		}
		if (m_bDX2_tx_grid_zero_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_grid_zero_pen);
		}
		if (m_bDX2_grid_zero_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_grid_zero_pen);
		}
		if (m_bDX2_tx_vgrid_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_vgrid_pen);
		}
		if (m_bDX2_grid_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_grid_pen);
		}
		if (m_bDX2_tx_hgrid_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_tx_hgrid_pen);
		}
		if (m_bDX2_hgrid_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_hgrid_pen);
		}
		if (m_bDX2_grid_text_pen != null)
		{
			Utilities.Dispose(ref m_bDX2_grid_text_pen);
		}
		if (m_styleDots != null)
		{
			Utilities.Dispose(ref m_styleDots);
		}
		if (m_bDX2_noisefloor != null)
		{
			Utilities.Dispose(ref m_bDX2_noisefloor);
		}
		if (m_bDX2_noisefloor_text != null)
		{
			Utilities.Dispose(ref m_bDX2_noisefloor_text);
		}
		m_brushLGDataFillRX1 = null;
		m_brushLGDataFillRX2 = null;
		m_brushLGDataLineRX1 = null;
		m_brushLGDataLineRX2 = null;
		m_brushLGDataFillTX_RX1 = null;
		m_brushLGDataLineTX_RX1 = null;
		m_brushLGDataFillTX_RX2 = null;
		m_brushLGDataLineTX_RX2 = null;
		m_bDX2_dataPeaks_fill_fpen_brush = null;
		m_bDX2_data_fill_fpen_brush = null;
		m_bDX2_data_fill_fpen_brush_tx = null;
		m_bDX2_data_line_pen_brush = null;
		m_bDX2_data_line_pen_brush_tx = null;
		m_bDX2_tx_data_line_fpen_brush = null;
		m_bDX2_tx_data_line_pen_brush = null;
		m_bDX2_p1 = null;
		m_bDX2_display_background_brush = null;
		m_bDX2_m_bHightlightNumbers = null;
		m_bDX2_m_bHightlightNumberScale = null;
		m_bDX2_grid_tx_text_brush = null;
		m_bDX2_grid_text_brush = null;
		m_bDX2_pana_text_brush = null;
		m_bDX2_bandstack_overlay_brush = null;
		m_bDX2_bandstack_overlay_brush_lines = null;
		m_bDX2_bandstack_overlay_brush_highlight = null;
		m_bDX2_display_filter_brush = null;
		m_bDX2_tx_filter_brush = null;
		m_bDX2_m_bTextCallOutActive = null;
		m_bDX2_m_bTextCallOutInactive = null;
		m_bDX2_m_pHighlighted = null;
		m_bDX2_m_bBWHighlighedFillColour = null;
		m_bDX2_tx_band_edge_pen = null;
		m_bDX2_tx_vgrid_pen_inb = null;
		m_bDX2_band_edge_pen = null;
		m_bDX2_grid_pen_inb = null;
		m_bDX2_Red = null;
		m_bDX2_Yellow = null;
		m_bDX2_YellowGreen = null;
		m_bDX2_Gray = null;
		m_bDX2_PeakBlob = null;
		m_bDX2_PeakBlobText = null;
		m_bDX2_y1_brush = null;
		m_bDX2_y2_brush = null;
		m_bDX2_waveform_line_pen = null;
		m_bDX2_dhp = null;
		m_bDX2_dhp1 = null;
		m_bDX2_dhp2 = null;
		m_bDX2_sub_rx_filter_brush = null;
		m_bDX2_sub_rx_zero_line_pen = null;
		m_bDX2_tx_filter_pen = null;
		m_bDX2_cw_zero_pen = null;
		m_bDX2_m_pNotchActive = null;
		m_bDX2_m_bBWFillColour = null;
		m_bDX2_m_pNotchInactive = null;
		m_bDX2_m_bBWFillColourInactive = null;
		m_bDX2_m_pTNFInactive = null;
		m_bDX2_m_bTNFInactive = null;
		m_bDX2_tx_grid_zero_pen = null;
		m_bDX2_grid_zero_pen = null;
		m_bDX2_tx_vgrid_pen = null;
		m_bDX2_grid_pen = null;
		m_bDX2_tx_hgrid_pen = null;
		m_bDX2_hgrid_pen = null;
		m_bDX2_grid_text_pen = null;
		m_styleDots = null;
		m_bDX2_noisefloor = null;
		m_bDX2_noisefloor_text = null;
	}

	private static void buildDX2Resources()
	{
		lock (_objDX2Lock)
		{
			if (_bDX2Setup)
			{
				LogGPU($"buildDX2Resources: Depth={WaterfallEnhancer.Depth}, PixelSize={WaterfallPixelWriter.PixelSize}, DxgiFormat={WaterfallPixelWriter.DxgiFormat}, RT type={_d2dRenderTarget?.GetType().Name}");
				releaseDX2Resources();
				_bRebuildRXLinearGradBrush = true;
				_bRebuildTXLinearGradBrush = true;
				m_bDX2_dataPeaks_fill_fpen_brush = convertBrush((SolidBrush)dataPeaks_fill_fpen.Brush);
				m_bDX2_data_fill_fpen_brush = convertBrush((SolidBrush)data_fill_fpen.Brush);
				m_bDX2_data_fill_fpen_brush_tx = convertBrush((SolidBrush)data_fill_fpen_tx.Brush);
				m_bDX2_data_line_pen_brush = convertBrush((SolidBrush)data_line_pen.Brush);
				m_bDX2_data_line_pen_brush_tx = convertBrush((SolidBrush)tx_data_line_pen.Brush);
				m_bDX2_tx_data_line_fpen_brush = convertBrush((SolidBrush)tx_data_line_fpen.Brush);
				m_bDX2_tx_data_line_pen_brush = convertBrush((SolidBrush)tx_data_line_pen.Brush);
				m_bDX2_p1 = convertBrush((SolidBrush)p1.Brush);
				m_bDX2_display_background_brush = convertBrush(display_background_brush);
				m_cDX2_display_background_colour = convertColour(display_background_brush.Color);
				m_cDX2_display_background_clear_colour = convertColour(System.Drawing.Color.FromArgb(255, System.Drawing.Color.Black));
				m_bDX2_m_bHightlightNumbers = getDXBrushForColour(System.Drawing.Color.FromArgb(255, 255, 255));
				m_bDX2_m_bHightlightNumberScale = getDXBrushForColour(System.Drawing.Color.FromArgb(192, 64, 64, 64));
				m_bDX2_grid_tx_text_brush = convertBrush(grid_tx_text_brush);
				m_bDX2_grid_text_brush = convertBrush(grid_text_brush);
				m_bDX2_pana_text_brush = convertBrush(pana_text_brush);
				m_bDX2_bandstack_overlay_brush = convertBrush(bandstack_overlay_brush);
				m_bDX2_bandstack_overlay_brush_lines = convertBrush(bandstack_overlay_brush_lines);
				m_bDX2_bandstack_overlay_brush_highlight = convertBrush(bandstack_overlay_brush_highlight);
				m_bDX2_display_filter_brush = convertBrush(display_filter_brush);
				m_bDX2_tx_filter_brush = convertBrush(tx_filter_brush);
				m_bDX2_m_bTextCallOutActive = convertBrush((SolidBrush)m_bTextCallOutActive);
				m_bDX2_m_bTextCallOutInactive = convertBrush((SolidBrush)m_bTextCallOutInactive);
				m_bDX2_m_pHighlighted = convertBrush((SolidBrush)m_pHighlighted.Brush);
				m_bDX2_m_bBWHighlighedFillColour = convertBrush((SolidBrush)m_bBWHighlighedFillColour);
				m_bDX2_tx_band_edge_pen = convertBrush((SolidBrush)tx_band_edge_pen.Brush);
				m_bDX2_tx_vgrid_pen_inb = convertBrush((SolidBrush)tx_vgrid_pen_inb.Brush);
				m_bDX2_band_edge_pen = convertBrush((SolidBrush)band_edge_pen.Brush);
				m_bDX2_grid_pen_inb = convertBrush((SolidBrush)grid_pen_inb.Brush);
				m_bDX2_Red = getDXBrushForColour(System.Drawing.Color.Red);
				m_bDX2_Yellow = getDXBrushForColour(System.Drawing.Color.Yellow);
				m_bDX2_YellowGreen = getDXBrushForColour(System.Drawing.Color.YellowGreen);
				m_bDX2_Gray = getDXBrushForColour(System.Drawing.Color.Gray);
				m_bDX2_PeakBlob = getDXBrushForColour(System.Drawing.Color.OrangeRed);
				m_bDX2_PeakBlobText = getDXBrushForColour(System.Drawing.Color.Chartreuse);
				m_bDX2_y1_brush = getDXBrushForColour(System.Drawing.Color.FromArgb(64, 64, 64));
				m_bDX2_y2_brush = getDXBrushForColour(System.Drawing.Color.FromArgb(48, 48, 48));
				m_bDX2_waveform_line_pen = getDXBrushForColour(System.Drawing.Color.LightGreen);
				m_bDX2_dhp = getDXBrushForColour(System.Drawing.Color.FromArgb(0, 255, 0));
				m_bDX2_dhp1 = getDXBrushForColour(System.Drawing.Color.FromArgb(150, 0, 0, 255));
				m_bDX2_dhp2 = getDXBrushForColour(System.Drawing.Color.FromArgb(150, 255, 0, 0));
				m_bDX2_sub_rx_filter_brush = convertBrush(sub_rx_filter_brush);
				m_bDX2_sub_rx_zero_line_pen = convertBrush((SolidBrush)sub_rx_zero_line_pen.Brush);
				m_bDX2_tx_filter_pen = convertBrush((SolidBrush)tx_filter_pen.Brush);
				m_bDX2_cw_zero_pen = convertBrush((SolidBrush)cw_zero_pen.Brush);
				m_bDX2_m_pNotchActive = convertBrush((SolidBrush)m_pNotchActive.Brush);
				m_bDX2_m_bBWFillColour = convertBrush((SolidBrush)m_bBWFillColour);
				m_bDX2_m_pNotchInactive = convertBrush((SolidBrush)m_pNotchInactive.Brush);
				m_bDX2_m_bBWFillColourInactive = convertBrush((SolidBrush)m_bBWFillColourInactive);
				m_bDX2_m_pTNFInactive = convertBrush((SolidBrush)m_pTNFInactive.Brush);
				m_bDX2_m_bTNFInactive = convertBrush((SolidBrush)m_bTNFInactive);
				m_bDX2_tx_grid_zero_pen = convertBrush((SolidBrush)tx_grid_zero_pen.Brush);
				m_bDX2_grid_zero_pen = convertBrush((SolidBrush)grid_zero_pen.Brush);
				m_bDX2_tx_vgrid_pen = convertBrush((SolidBrush)tx_vgrid_pen.Brush);
				m_bDX2_grid_pen = convertBrush((SolidBrush)grid_pen.Brush);
				m_bDX2_tx_hgrid_pen = convertBrush((SolidBrush)tx_hgrid_pen.Brush);
				m_bDX2_hgrid_pen = convertBrush((SolidBrush)hgrid_pen.Brush);
				m_bDX2_grid_text_pen = convertBrush((SolidBrush)grid_text_pen.Brush);
				StrokeStyleProperties properties = new StrokeStyleProperties
				{
					DashOffset = 2f,
					DashStyle = DashStyle.Dash
				};
				m_styleDots = new StrokeStyle(_d2dFactory, properties);
				m_bDX2_noisefloor = convertBrush(new SolidBrush(noisefloor_color));
				m_bDX2_noisefloor_text = convertBrush(new SolidBrush(noisefloor_color_text));
			}
		}
	}

	private static void releaseFonts()
	{
		if (fontDX2d_callout != null)
		{
			Utilities.Dispose(ref fontDX2d_callout);
		}
		if (fontDX2d_font9 != null)
		{
			Utilities.Dispose(ref fontDX2d_font9);
		}
		if (fontDX2d_font9b != null)
		{
			Utilities.Dispose(ref fontDX2d_font9b);
		}
		if (fontDX2d_font9c != null)
		{
			Utilities.Dispose(ref fontDX2d_font9c);
		}
		if (fontDX2d_panafont != null)
		{
			Utilities.Dispose(ref fontDX2d_panafont);
		}
		if (fontDX2d_font10 != null)
		{
			Utilities.Dispose(ref fontDX2d_font10);
		}
		if (fontDX2d_font12 != null)
		{
			Utilities.Dispose(ref fontDX2d_font12);
		}
		if (fontDX2d_font14 != null)
		{
			Utilities.Dispose(ref fontDX2d_font14);
		}
		if (fontDX2d_font32 != null)
		{
			Utilities.Dispose(ref fontDX2d_font32);
		}
		if (fontDX2d_font1 != null)
		{
			Utilities.Dispose(ref fontDX2d_font1);
		}
		if (fontDX2d_fps_profile != null)
		{
			Utilities.Dispose(ref fontDX2d_fps_profile);
		}
		if (fontFactory != null)
		{
			Utilities.Dispose(ref fontFactory);
		}
		fontDX2d_callout = null;
		fontDX2d_font9 = null;
		fontDX2d_font9b = null;
		fontDX2d_font9c = null;
		fontDX2d_panafont = null;
		fontDX2d_font10 = null;
		fontDX2d_font12 = null;
		fontDX2d_font14 = null;
		fontDX2d_font32 = null;
		fontDX2d_font1 = null;
		fontDX2d_fps_profile = null;
		fontFactory = null;
	}

	private static void buildFontsDX2D()
	{
		lock (_objDX2Lock)
		{
			if (_bDX2Setup)
			{
				releaseFonts();
				fontFactory = new SharpDX.DirectWrite.Factory();
				fontDX2d_callout = new TextFormat(fontFactory, m_fntCallOutFont.FontFamily.Name, m_fntCallOutFont.Size / 72f * 96f * _renderScale);
				fontDX2d_font9 = new TextFormat(fontFactory, font9.FontFamily.Name, font9.Size / 72f * 96f * _renderScale);
				fontDX2d_font9b = new TextFormat(fontFactory, font9b.FontFamily.Name, FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, font9b.Size / 72f * 96f * _renderScale);
				fontDX2d_font9c = new TextFormat(fontFactory, font95.FontFamily.Name, font95.Size / 72f * 96f * _renderScale);
				fontDX2d_panafont = new TextFormat(fontFactory, pana_font.FontFamily.Name, pana_font.Size / 72f * 96f * _renderScale);
				fontDX2d_font10 = new TextFormat(fontFactory, font10.FontFamily.Name, font10.Size / 72f * 96f * _renderScale);
				fontDX2d_font12 = new TextFormat(fontFactory, font12.FontFamily.Name, font12.Size / 72f * 96f * _renderScale);
				fontDX2d_font14 = new TextFormat(fontFactory, font14b.FontFamily.Name, font14b.Size / 72f * 96f * _renderScale);
				fontDX2d_font32 = new TextFormat(fontFactory, font32b.FontFamily.Name, font32b.Size / 72f * 96f * _renderScale);
				fontDX2d_font1 = new TextFormat(fontFactory, font1r.FontFamily.Name, font1r.Size / 72f * 96f * _renderScale);
				fontDX2d_fps_profile = new TextFormat(fontFactory, m_fntCallOutFont.FontFamily.Name, 85.333336f * _renderScale);
			}
		}
	}

	private static void clearBackgroundDX2D(int rx, int W, int H, bool bottom)
	{
		switch (rx)
		{
		case 1:
			if (bottom)
			{
				_d2dRenderTarget.FillRectangle(new SharpDX.RectangleF(0f, H, W, H), m_bDX2_display_background_brush);
			}
			else
			{
				_d2dRenderTarget.FillRectangle(new SharpDX.RectangleF(0f, 0f, W, H), m_bDX2_display_background_brush);
			}
			break;
		case 2:
			if (bottom)
			{
				if (current_display_mode_bottom == DisplayMode.PANAFALL)
				{
					_d2dRenderTarget.FillRectangle(new SharpDX.RectangleF(0f, H * 3, W, H), m_bDX2_display_background_brush);
				}
				else
				{
					_d2dRenderTarget.FillRectangle(new SharpDX.RectangleF(0f, H, W, H), m_bDX2_display_background_brush);
				}
			}
			else
			{
				_d2dRenderTarget.FillRectangle(new SharpDX.RectangleF(0f, H * 2, W, H), m_bDX2_display_background_brush);
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void drawLineDX2D(SharpDX.Direct2D1.Brush b, float x1, float y1, float x2, float y2, float strokeWidth = 1f)
	{
		if (WaterfallPixelWriter.PixelSize == 8)
		{
			strokeWidth *= 1f;
		}
		_d2dRenderTarget.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), b, strokeWidth);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void drawLineDX2D(SharpDX.Direct2D1.Brush b, float x1, float y1, float x2, float y2, StrokeStyle strokeStyle, float strokeWidth = 1f)
	{
		if (WaterfallPixelWriter.PixelSize == 8)
		{
			strokeWidth *= 1f;
		}
		_d2dRenderTarget.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), b, strokeWidth, strokeStyle);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float sw(float width)
	{
		if (WaterfallPixelWriter.PixelSize != 8)
		{
			return width;
		}
		return width * 1f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void drawFillRectangleDX2D(SharpDX.Direct2D1.Brush b, float x, float y, float w, float h)
	{
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(x, y, w, h);
		_d2dRenderTarget.FillRectangle(rectangleF, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void drawRectangleDX2D(SharpDX.Direct2D1.Brush b, float x, float y, float w, float h)
	{
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(x, y, w, h);
		_d2dRenderTarget.DrawRectangle(rectangleF, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void drawElipseDX2D(SharpDX.Direct2D1.Brush b, float xMiddle, float yMiddle, float w, float h)
	{
		Ellipse ellipse = new Ellipse(new Vector2(xMiddle, yMiddle), w / 2f, h / 2f);
		_d2dRenderTarget.DrawEllipse(ellipse, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void drawFillElipseDX2D(SharpDX.Direct2D1.Brush b, float xMiddle, float yMiddle, float w, float h)
	{
		Ellipse ellipse = new Ellipse(new Vector2(xMiddle, yMiddle), w / 2f, h / 2f);
		_d2dRenderTarget.FillEllipse(ellipse, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void drawRectangleDX2D(SharpDX.Direct2D1.Brush b, System.Drawing.Rectangle r, float lineWidth = 1f)
	{
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(r.X, r.Y, r.Width, r.Height);
		_d2dRenderTarget.DrawRectangle(rectangleF, b, lineWidth);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void drawFillRectangleDX2D(SharpDX.Direct2D1.Brush b, System.Drawing.Rectangle r)
	{
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(r.X, r.Y, r.Width, r.Height);
		_d2dRenderTarget.FillRectangle(rectangleF, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GridLabelOffset(string label, float legacyOffset)
	{
		float num = (float)label.Length * 4.1f;
		if (num <= 0.01f)
		{
			return (int)legacyOffset;
		}
		return (int)(measureStringDX2D(label, fontDX2d_font9, cacheStringLength: true).Width * (legacyOffset / num));
	}

	private static void drawStringDX2D(string s, TextFormat tf, SharpDX.Direct2D1.Brush b, float x, float y)
	{
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(x, y, float.PositiveInfinity, float.PositiveInfinity);
		_d2dRenderTarget.DrawText(s, tf, rectangleF, b, DrawTextOptions.None);
	}

	private static void drawFilterOverlayDX2D(SharpDX.Direct2D1.Brush brush, int filter_left_x, int filter_right_x, int W, int H, int rx, int top, bool bottom, int nVerticalShfit)
	{
		if (filter_left_x == filter_right_x)
		{
			filter_right_x = filter_left_x + 1;
		}
		int num = filter_right_x - filter_left_x;
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(filter_left_x, nVerticalShfit + top, num, H - top);
		_d2dRenderTarget.FillRectangle(rectangleF, brush);
	}

	private static void drawChannelBarDX2D(Channel chan, int left, int right, int top, int height, System.Drawing.Color c, System.Drawing.Color h)
	{
		int num = right - left;
		drawFillRectangleDX2D(convertBrush(new SolidBrush(c)), left, top, num, height);
		if (num > 2)
		{
			using (Pen pen = new Pen(h, 1f))
			{
				drawLineDX2D(convertBrush((SolidBrush)pen.Brush), left, top, left, top + height - 1, pen.Width);
				drawLineDX2D(convertBrush((SolidBrush)pen.Brush), right, top, right, top + height - 1, pen.Width);
			}
		}
	}

	private static SizeF measureStringDX2D(string s, TextFormat tf, bool cacheStringLength = false)
	{
		(string, string, float) tuple = ((!cacheStringLength) ? (s, tf.FontFamilyName, tf.FontSize) : (s.Length.ToString(), tf.FontFamilyName, tf.FontSize));
		if (m_stringSizeCache.TryGetValue(tuple, out var value))
		{
			return value;
		}
		TextLayout comObject = new TextLayout(fontFactory, s, tf, float.PositiveInfinity, float.PositiveInfinity);
		SizeF sizeF = new SizeF(comObject.Metrics.Width, comObject.Metrics.Height);
		Utilities.Dispose(ref comObject);
		comObject = null;
		m_stringSizeCache.Add(tuple, sizeF);
		_stringMeasureKeys.Enqueue(tuple);
		if (m_stringSizeCache.Count > 500)
		{
			(string, string, float) key = _stringMeasureKeys.Dequeue();
			m_stringSizeCache.Remove(key);
		}
		return sizeF;
	}

	internal static int getCWSideToneShift(int rx, DSPMode forceMode = DSPMode.FIRST)
	{
		int result = 0;
		switch ((forceMode == DSPMode.FIRST) ? ((rx == 1) ? rx1_dsp_mode : rx2_dsp_mode) : forceMode)
		{
		case DSPMode.CWL:
			result = cw_pitch;
			break;
		case DSPMode.CWU:
			result = -cw_pitch;
			break;
		}
		return result;
	}

	private static List<clsNotchCoords> handleNotches(int rx, bool bottom, int cwSideToneShift, int Low, int High, int nVerticalShift, int top, int width, int W, int H, bool bDraw)
	{
		long num = ((rx == 1) ? vfoa_hz : vfob_hz);
		int num2 = ((rx == 1) ? ((!_rx1ClickDisplayCTUN) ? rit_hz : 0) : 0);
		num += cwSideToneShift;
		List<MNotch> list = MNotchDB.NotchesInBW(num, Low - console.MaxFilterWidth, High + console.MaxFilterWidth);
		List<clsNotchCoords> list2 = new List<clsNotchCoords>();
		double num3 = (localMox(rx) ? _mnfMinSizeTX : _mnfMinSizeRX[rx - 1]);
		foreach (MNotch item2 in list)
		{
			int num4;
			int num5;
			int num6;
			if (bDraw)
			{
				num4 = (int)((float)(item2.FCenter - (double)num - (double)Low - (double)num2) / (float)width * (float)W);
				num5 = (int)((float)(item2.FCenter - (double)num - item2.FWidth / 2.0 - (double)Low - (double)num2) / (float)width * (float)W);
				num6 = (int)((float)(item2.FCenter - (double)num + item2.FWidth / 2.0 - (double)Low - (double)num2) / (float)width * (float)W);
			}
			else
			{
				double num7 = ((item2.FWidth < num3) ? num3 : item2.FWidth);
				num7 += 20.0;
				num4 = (int)((float)(item2.FCenter - (double)num - (double)Low - (double)num2) / (float)width * (float)W);
				num5 = (int)((float)(item2.FCenter - (double)num - num7 / 2.0 - (double)Low - (double)num2) / (float)width * (float)W);
				num6 = (int)((float)(item2.FCenter - (double)num + num7 / 2.0 - (double)Low - (double)num2) / (float)width * (float)W);
			}
			clsNotchCoords item = new clsNotchCoords(num4, num5, num6, _tnf_active && item2.Active, (int)item2.FWidth);
			list2.Add(item);
			if (!bDraw)
			{
				continue;
			}
			SharpDX.Direct2D1.Brush b;
			SharpDX.Direct2D1.Brush b2;
			if (_tnf_active)
			{
				if (item2.Active)
				{
					b = m_bDX2_m_pNotchActive;
					b2 = m_bDX2_m_bBWFillColour;
				}
				else
				{
					b = m_bDX2_m_pNotchInactive;
					b2 = m_bDX2_m_bBWFillColourInactive;
				}
			}
			else
			{
				b = m_bDX2_m_pTNFInactive;
				b2 = m_bDX2_m_bTNFInactive;
			}
			if (item2 == m_objHightlightedNotch)
			{
				SharpDX.Direct2D1.Brush b3 = ((!item2.Active) ? m_bDX2_m_bTextCallOutInactive : m_bDX2_m_bTextCallOutActive);
				b = m_bDX2_m_pHighlighted;
				b2 = m_bDX2_m_bBWHighlighedFillColour;
				string text = (item2.FCenter / 1000000.0).ToString("f6") + "MHz";
				int startIndex = text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) + 4;
				drawStringDX2D("F: " + text.Insert(startIndex, " "), fontDX2d_callout, b3, num6 + 4, nVerticalShift + top + H / 4);
				drawStringDX2D("W: " + item2.FWidth.ToString("f0") + "Hz", fontDX2d_callout, b3, num6 + 4, nVerticalShift + top + H / 4 + 12);
			}
			drawLineDX2D(b, num4, nVerticalShift + top, num4, nVerticalShift + H);
			if (num5 != num6)
			{
				drawFillRectangleDX2D(b2, num5, nVerticalShift + top, num6 - num5, H - top);
			}
		}
		return list2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static float fastPow10Shifted(float dBdiv10)
	{
		if (dBdiv10 <= -20f || dBdiv10 >= 20f)
		{
			return (float)Math.Pow(10.0, dBdiv10);
		}
		int num = (int)(dBdiv10 * 27866352f) + 1065353216;
		return *(float*)(&num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static float fastPow10Raw(float dB)
	{
		if (dB <= -200f || dB >= 200f)
		{
			return (float)Math.Pow(10.0, (double)dB / 10.0);
		}
		int num = (int)(dB * (8754473f / (float)Math.PI)) + 1065353216;
		return *(float*)(&num);
	}

	private unsafe static int drawPanadapterAndWaterfallGridDX2D(int nVerticalShift, int W, int H, int rx, bool bottom, out long left_edge, out long right_edge, bool bIsWaterfall = false)
	{
		DisplayLabelAlignment displayLabelAlignment = display_label_align;
		bool flag = isRxDuplex(rx);
		bool flag2 = localMox(rx);
		int num = 0;
		int num2 = 0;
		_ = W / 2;
		int[] array = new int[4] { 10, 20, 25, 50 };
		int num3 = 1;
		int num4 = 0;
		int num5 = 50;
		int num6 = 5;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int cWSideToneShift = getCWSideToneShift(rx);
		int num11 = -cWSideToneShift;
		int num12;
		if (rx == 1)
		{
			if (flag2)
			{
				if (flag)
				{
					num = rx_display_low;
					num2 = rx_display_high;
					num12 = sample_rate_rx1;
				}
				else
				{
					num = tx_display_low;
					num2 = tx_display_high;
					num12 = sample_rate_tx;
				}
				num7 = tx_spectrum_grid_max;
				num8 = tx_spectrum_grid_min;
				num9 = tx_spectrum_grid_step;
				displayLabelAlignment = tx_display_label_align;
			}
			else
			{
				num = rx_display_low;
				num2 = rx_display_high;
				num7 = spectrum_grid_max;
				num8 = spectrum_grid_min;
				num9 = spectrum_grid_step;
				num12 = sample_rate_rx1;
			}
			num10 = freq_diff;
		}
		else
		{
			if (flag2)
			{
				if (flag)
				{
					num = tx_display_low;
					num2 = tx_display_high;
					num12 = sample_rate_tx;
				}
				else
				{
					num = tx_display_low;
					num2 = tx_display_high;
					num12 = sample_rate_tx;
				}
				num7 = tx_spectrum_grid_max;
				num8 = tx_spectrum_grid_min;
				num9 = tx_spectrum_grid_step;
				displayLabelAlignment = tx_display_label_align;
			}
			else
			{
				num = rx2_display_low;
				num2 = rx2_display_high;
				num7 = rx2_spectrum_grid_max;
				num8 = rx2_spectrum_grid_min;
				num9 = rx2_spectrum_grid_step;
				num12 = sample_rate_rx2;
			}
			num10 = rx2_freq_diff;
		}
		int num13 = num7 - num8;
		if (split_display)
		{
			num9 *= 2;
		}
		int num14;
		int num15;
		if (rx == 1)
		{
			if (flag2)
			{
				num14 = tx_filter_low;
				num15 = tx_filter_high;
			}
			else
			{
				num14 = rx1_filter_low;
				num15 = rx1_filter_high;
			}
		}
		else if (flag2)
		{
			num14 = tx_filter_low;
			num15 = tx_filter_high;
		}
		else
		{
			num14 = rx2_filter_low;
			num15 = rx2_filter_high;
		}
		if ((rx1_dsp_mode == DSPMode.DRM && rx == 1) || (rx2_dsp_mode == DSPMode.DRM && rx == 2))
		{
			num14 = -6000;
			num15 = 6000;
		}
		int num16 = num2 - num;
		while (num16 / num5 > 10)
		{
			num5 = array[num4] * (int)Math.Pow(10.0, num3);
			num4 = (num4 + 1) % 4;
			if (num4 == 0)
			{
				num3++;
			}
		}
		int num17 = (num7 - num8) / num9;
		int num18 = ((!bIsWaterfall) ? ((int)((double)num9 * (double)H / (double)num13)) : 20);
		long num19 = ((!flag2) ? (vfoa_sub_hz - vfoa_hz) : ((!flag) ? 0 : (vfoa_sub_hz - vfoa_hz)));
		int num20 = ((!flag2) ? ((rx == 1) ? ((!console.CTuneDisplay) ? rit_hz : 0) : 0) : 0);
		SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(0f, nVerticalShift, W, H);
		_d2dRenderTarget.PushAxisAlignedClip(rectangleF, AntialiasMode.Aliased);
		if (!flag2 && sub_rx1_enabled && rx == 1)
		{
			int num21 = (_rx1ClickDisplayCTUN ? rit_hz : 0);
			if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
			{
				int filter_left_x = (int)((float)(num14 - num + num19 + num21) / (float)num16 * (float)W);
				int filter_right_x = (int)((float)(num15 - num + num19 + num21) / (float)num16 * (float)W);
				drawFilterOverlayDX2D(m_bDX2_sub_rx_filter_brush, filter_left_x, filter_right_x, W, H, rx, num18, bottom, nVerticalShift);
			}
			if ((bIsWaterfall && m_bShowRXZeroLineOnWaterfall) || !bIsWaterfall)
			{
				int num22 = (int)((float)(num19 - num + num21) / (float)num16 * (float)W);
				drawLineDX2D(m_bDX2_sub_rx_zero_line_pen, num22, nVerticalShift + num18, num22, nVerticalShift + H, 2f);
			}
		}
		if (((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall) && !flag2)
		{
			int num23 = (int)((float)(num14 - num - num10) / (float)num16 * (float)W);
			int num24 = (int)((float)(num15 - num - num10) / (float)num16 * (float)W);
			drawFilterOverlayDX2D(m_bDX2_display_filter_brush, num23, num24, W, H, rx, num18, bottom, nVerticalShift);
			if (!bIsWaterfall)
			{
				int num25 = 0;
				switch (rx)
				{
				case 1:
					num25 = m_nHightlightFilterEdgeRX1;
					break;
				case 2:
					num25 = m_nHightlightFilterEdgeRX2;
					break;
				}
				switch (num25)
				{
				case -1:
					drawLineDX2D(m_bDX2_cw_zero_pen, num23, nVerticalShift + num18, num23, nVerticalShift + H, 2f);
					break;
				case 1:
					drawLineDX2D(m_bDX2_cw_zero_pen, num24, nVerticalShift + num18, num24, nVerticalShift + H, 2f);
					break;
				}
			}
		}
		if (((rx == 1 && rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU) || (rx == 2 && rx2_dsp_mode != DSPMode.CWL && rx2_dsp_mode != DSPMode.CWU)) && ((bIsWaterfall && m_bShowTXFilterOnRXWaterfall) || !bIsWaterfall))
		{
			int num26;
			int num27;
			if (flag2)
			{
				num26 = num14;
				num27 = num15;
			}
			else
			{
				num26 = tx_filter_low;
				num27 = tx_filter_high;
			}
			int num28 = 0;
			int num29 = ((!flag2) ? xit_hz : (flag ? xit_hz : 0));
			int num30;
			int num31;
			if (!split_enabled)
			{
				if (!flag2)
				{
					num28 = ((rx == 1) ? rit_hz : 0);
				}
				num30 = (int)((float)(num26 - num - num10 + num29 - num28) / (float)num16 * (float)W);
				num31 = (int)((float)(num27 - num - num10 + num29 - num28) / (float)num16 * (float)W);
			}
			else
			{
				if (!flag2)
				{
					num28 = ((rx != 1 || !_rx1ClickDisplayCTUN) ? ((rx != 2) ? rit_hz : 0) : 0);
				}
				num30 = (int)((float)(num26 - num + num29 + num19 - num28) / (float)num16 * (float)W);
				num31 = (int)((float)(num27 - num + num29 + num19 - num28) / (float)num16 * (float)W);
			}
			if (flag2)
			{
				drawFilterOverlayDX2D(m_bDX2_tx_filter_brush, num30, num31, W, H, rx, num18, bottom, nVerticalShift);
			}
			else if (draw_tx_filter && ((rx == 2 && _tx_on_vfob) || (rx == 1 && (!_tx_on_vfob || !_rx2_enabled))))
			{
				drawLineDX2D(m_bDX2_tx_filter_pen, num30, nVerticalShift + num18, num30, nVerticalShift + H, tx_filter_pen.Width);
				drawLineDX2D(m_bDX2_tx_filter_pen, num31, nVerticalShift + num18, num31, nVerticalShift + H, tx_filter_pen.Width);
			}
		}
		if (!bIsWaterfall && (console.CurrentRegion == FRSRegion.US || console.CurrentRegion == FRSRegion.UK))
		{
			foreach (Channel item in Console.Channels60m)
			{
				long num32 = vfoa_hz;
				int num33 = ((!_rx1ClickDisplayCTUN) ? rit_hz : 0);
				if (flag2)
				{
					num33 = 0;
				}
				if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
				{
					num32 = vfob_hz;
				}
				if (item.InBW((double)(num32 + num) * 1E-06, (double)(num32 + num2) * 1E-06))
				{
					bool flag3 = console.RX1IsIn60mChannel(item);
					DSPMode dSPMode = rx1_dsp_mode;
					if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
					{
						flag3 = console.RX2IsIn60mChannel(item);
						dSPMode = rx2_dsp_mode;
					}
					switch (dSPMode)
					{
					default:
						flag3 = false;
						break;
					case DSPMode.USB:
					case DSPMode.CWL:
					case DSPMode.CWU:
					case DSPMode.AM:
					case DSPMode.DIGU:
					case DSPMode.SAM:
						break;
					}
					num32 += cWSideToneShift;
					int num34 = (int)((float)(item.Freq * 1000000.0 - (double)num32 - (double)(item.BW / 2) - (double)num - (double)num33) / (float)num16 * (float)W);
					int num35 = (int)((float)(item.Freq * 1000000.0 - (double)num32 + (double)(item.BW / 2) - (double)num - (double)num33) / (float)num16 * (float)W);
					if (num35 == num34)
					{
						num35 = num34 + 1;
					}
					System.Drawing.Color c = (flag3 ? channel_background_on : channel_background_off);
					System.Drawing.Color h = channel_foreground;
					drawChannelBarDX2D(item, num34, num35, nVerticalShift + num18, H - num18, c, h);
				}
			}
		}
		if (m_bShowBandStackOverlays && m_bandStackOverlays != null && rx == 1 && !flag2 && !bIsWaterfall)
		{
			long num36 = vfoa_hz;
			int num37 = ((!_rx1ClickDisplayCTUN) ? rit_hz : 0);
			for (int i = 0; i < m_bandStackOverlays.Length; i++)
			{
				int num38 = (int)((float)(m_bandStackOverlays[i].Frequency * 1000000.0 - (double)num36 + (double)m_bandStackOverlays[i].LowFilter - (double)num - (double)num37) / (float)num16 * (float)W);
				int num39 = (int)((float)(m_bandStackOverlays[i].Frequency * 1000000.0 - (double)num36 + (double)m_bandStackOverlays[i].HighFilter - (double)num - (double)num37) / (float)num16 * (float)W);
				drawFilterOverlayDX2D((i == m_nHighlightedBandStackEntryIndex) ? m_bDX2_bandstack_overlay_brush_highlight : m_bDX2_bandstack_overlay_brush, num38, num39, W, H, rx, num18, bottom, nVerticalShift);
				drawLineDX2D(m_bDX2_bandstack_overlay_brush_lines, num38, nVerticalShift + num18, num38, nVerticalShift + H, 2f);
				drawLineDX2D(m_bDX2_bandstack_overlay_brush_lines, num39, nVerticalShift + num18, num39, nVerticalShift + H, 2f);
			}
		}
		if (!flag2 && !bIsWaterfall)
		{
			handleNotches(rx, bottom, cWSideToneShift, num, num2, nVerticalShift, num18, num16, W, H, bDraw: true);
		}
		if (!bIsWaterfall)
		{
			if (show_cwzero_line)
			{
				if (rx == 1 && !flag2 && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
				{
					int num40 = (int)((float)(num11 - num - num10) / (float)num16 * (float)W);
					drawLineDX2D(m_bDX2_cw_zero_pen, num40, nVerticalShift + num18, num40, nVerticalShift + H, cw_zero_pen.Width);
				}
				if (rx == 2 && !flag2 && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
				{
					int num41 = (int)((float)(num11 - num - num10) / (float)num16 * (float)W);
					drawLineDX2D(m_bDX2_cw_zero_pen, num41, nVerticalShift + num18, num41, nVerticalShift + H, cw_zero_pen.Width);
				}
			}
			if (draw_tx_cw_freq)
			{
				if (rx == 1 && !flag2 && (!_rx2_enabled || !_tx_on_vfob) && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
				{
					int num43;
					if (!split_enabled)
					{
						int num42 = rit_hz;
						num43 = (int)((float)(num11 - num - num10 + xit_hz - num42) / (float)num16 * (float)W);
					}
					else
					{
						int num42 = ((!_rx1ClickDisplayCTUN) ? rit_hz : 0);
						num43 = (int)((float)(num11 - num + xit_hz - num42 + num19) / (float)num16 * (float)W);
					}
					drawLineDX2D(m_bDX2_tx_filter_pen, num43, nVerticalShift + num18, num43, nVerticalShift + H, tx_filter_pen.Width);
				}
				if (rx == 2 && !flag2 && _rx2_enabled && _tx_on_vfob && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
				{
					int num44 = (split_enabled ? ((int)((float)(num11 - num + xit_hz + num19) / (float)num16 * (float)W)) : ((int)((float)(num11 - num - num10 + xit_hz) / (float)num16 * (float)W)));
					drawLineDX2D(m_bDX2_tx_filter_pen, num44, nVerticalShift + num18, num44, nVerticalShift + H, tx_filter_pen.Width);
				}
			}
		}
		int num46;
		if (flag2)
		{
			int num45 = (flag ? xit_hz : 0);
			num46 = (split_enabled ? ((int)((float)(-num + num45 + num19) / (float)num16 * (float)W)) : ((int)((float)(-num10 - num + num45) / (float)num16 * (float)W)));
		}
		else
		{
			num46 = (int)((float)(-num10 - num) / (float)num16 * (float)W);
		}
		if (((!bIsWaterfall && show_zero_line) | (bIsWaterfall && ((m_bShowRXZeroLineOnWaterfall & !flag2) || (m_bShowTXZeroLineOnWaterfall & flag2)))) && num46 >= 0 && num46 <= W)
		{
			float strokeWidth = (flag2 ? tx_grid_zero_pen.Width : grid_zero_pen.Width);
			drawLineDX2D(flag2 ? m_bDX2_tx_grid_zero_pen : m_bDX2_grid_zero_pen, num46, nVerticalShift + num18, num46, nVerticalShift + H, strokeWidth);
		}
		if (show_freq_offset)
		{
			SharpDX.Direct2D1.Brush b = (flag2 ? m_bDX2_tx_grid_zero_pen : m_bDX2_grid_zero_pen);
			drawStringDX2D("0", fontDX2d_font9, b, num46 - 5, nVerticalShift + 4);
		}
		m_bandEdgeRegionCacheDX2D.Update(console.CurrentRegion);
		int[] edges = m_bandEdgeRegionCacheDX2D.Edges;
		double num47;
		if (rx != 1)
		{
			num47 = (flag2 ? ((double)(vfob_hz + xit_hz)) : ((!console.VFOSync) ? ((double)vfob_hz) : ((double)(vfob_hz + num20))));
		}
		else if (!flag2)
		{
			num47 = ((!flag2 || !_tx_on_vfob) ? ((double)(vfoa_hz + num20)) : ((!console.RX2Enabled) ? ((double)vfoa_sub_hz) : ((double)(vfoa_hz + num20))));
		}
		else
		{
			num47 = ((!split_enabled) ? ((double)vfoa_hz) : ((double)(vfoa_sub_hz - num19)));
			num47 += (double)((!flag) ? xit_hz : 0);
		}
		num47 += (double)cWSideToneShift;
		long num48 = (long)(num47 / (double)num5) * num5;
		long num49 = (long)(num47 - (double)num48);
		int num50 = num16 / num5 + 1;
		for (int j = -1; j < num50 + 1; j++)
		{
			int num51 = j * num5 + num / num5 * num5;
			double num52 = (double)(num48 + num51) / 1000000.0;
			int num53 = (int)((double)(num51 - num49 - num) / (double)num16 * (double)W);
			if (!show_freq_offset)
			{
				bool flag4 = false;
				for (int k = 0; k < edges.Length; k++)
				{
					if (num52 == (double)edges[k] / 1000000.0)
					{
						flag4 = true;
						break;
					}
				}
				SharpDX.Direct2D1.Brush b2;
				SharpDX.Direct2D1.Brush b3;
				SharpDX.Direct2D1.Brush b4;
				if (flag4)
				{
					if (flag2)
					{
						b2 = m_bDX2_tx_band_edge_pen;
						b3 = m_bDX2_tx_vgrid_pen_inb;
						b4 = m_bDX2_tx_band_edge_pen;
					}
					else
					{
						b2 = m_bDX2_band_edge_pen;
						b3 = m_bDX2_grid_pen_inb;
						b4 = m_bDX2_band_edge_pen;
					}
				}
				else if (flag2)
				{
					b2 = m_bDX2_tx_vgrid_pen;
					b3 = m_bDX2_tx_vgrid_pen_inb;
					b4 = m_bDX2_grid_tx_text_brush;
				}
				else
				{
					b2 = m_bDX2_grid_pen;
					b3 = m_bDX2_grid_pen_inb;
					b4 = m_bDX2_grid_text_brush;
				}
				if (_grid_control_major && !bIsWaterfall)
				{
					drawLineDX2D(b2, num53, nVerticalShift + num18, num53, nVerticalShift + H);
					if (_grid_control_minor)
					{
						float num54 = (float)((int)((float)((j + 1) * num5 + num / num5 * num5 - num49 - num) / (float)num16 * (float)W) - num53) / (float)num6;
						for (int l = 1; l < num6; l++)
						{
							float num55 = (float)num53 + (float)l * num54;
							drawLineDX2D(b3, num55, nVerticalShift + num18, num55, nVerticalShift + H);
						}
					}
				}
				if (_show_frequency_numbers)
				{
					int num56;
					string text;
					if ((double)(int)(num52 * 1000.0) == num52 * 1000.0)
					{
						text = num52.ToString("f3");
						num56 = ((num52 < 10.0) ? GridLabelOffset(text, (float)(text.Length + 1) * 4.1f - 14f) : ((!(num52 < 100.0)) ? GridLabelOffset(text, (float)(text.Length + 1) * 4.1f - 8f) : GridLabelOffset(text, (float)(text.Length + 1) * 4.1f - 11f)));
					}
					else
					{
						text = num52.ToString("f4");
						int startIndex = text.IndexOf('.') + 4;
						text = text.Insert(startIndex, " ");
						num56 = ((num52 < 10.0) ? GridLabelOffset(text, (float)text.Length * 4.1f - 14f) : ((!(num52 < 100.0)) ? GridLabelOffset(text, (float)text.Length * 4.1f - 8f) : GridLabelOffset(text, (float)text.Length * 4.1f - 11f)));
					}
					drawStringDX2D(text, fontDX2d_font9, b4, num53 - num56, nVerticalShift + 4);
				}
			}
			else
			{
				num53 = Convert.ToInt32((double)(-(num51 - num)) / (double)(num - num2) * (double)W);
				if (!bIsWaterfall)
				{
					drawLineDX2D(flag2 ? m_bDX2_tx_vgrid_pen : m_bDX2_grid_pen, num53, nVerticalShift + num18, num53, nVerticalShift + H);
				}
				string text = num51.ToString();
				int num56 = GridLabelOffset(text, (float)(text.Length + 1) * 4.1f);
				int num57 = GridLabelOffset(text, (float)text.Length * 4.1f);
				if (num53 - num56 >= 0 && num53 + num57 < W && num51 != 0)
				{
					SharpDX.Direct2D1.Brush b5 = (flag2 ? m_bDX2_grid_tx_text_brush : m_bDX2_grid_text_brush);
					drawStringDX2D(text, fontDX2d_font9, b5, num53 - num56, nVerticalShift + 4);
				}
			}
		}
		if (!bIsWaterfall && _joinBandEdges)
		{
			int num58 = ((rx == 1) ? ((!_rx1ClickDisplayCTUN) ? rit_hz : 0) : 0);
			long num59 = ((rx == 1) ? vfoa_hz : vfob_hz);
			SharpDX.Direct2D1.Brush b6 = (flag2 ? m_bDX2_tx_band_edge_pen : m_bDX2_band_edge_pen);
			for (int m = 0; m < edges.Length; m += 2)
			{
				int num60 = edges[m];
				int num61 = edges[m + 1];
				int num62 = (int)((float)(num60 - cWSideToneShift - num59 - num - num58) / (float)num16 * (float)W);
				int num63 = (int)((float)(num61 - cWSideToneShift - num59 - num - num58) / (float)num16 * (float)W);
				bool num64 = num62 >= 0 && num62 <= W;
				bool flag5 = num63 >= 0 && num63 <= W;
				if (num64)
				{
					if (flag5)
					{
						drawLineDX2D(b6, num62, nVerticalShift + 2, num63, nVerticalShift + 2);
						drawLineDX2D(b6, num62, nVerticalShift + 2, num62, nVerticalShift + 8);
						drawLineDX2D(b6, num63, nVerticalShift + 2, num63, nVerticalShift + 8);
					}
					else
					{
						drawLineDX2D(b6, num62, nVerticalShift + 2, W, nVerticalShift + 2);
						drawLineDX2D(b6, num62, nVerticalShift + 2, num62, nVerticalShift + 8);
					}
				}
				else if (flag5)
				{
					drawLineDX2D(b6, 0f, nVerticalShift + 2, num63, nVerticalShift + 2);
					drawLineDX2D(b6, num63, nVerticalShift + 2, num63, nVerticalShift + 8);
				}
				else if (num62 < 0 && num63 > W)
				{
					drawLineDX2D(b6, 0f, nVerticalShift + 2, W, nVerticalShift + 2);
				}
			}
		}
		if (!bIsWaterfall)
		{
			SharpDX.Direct2D1.Brush b2 = ((!flag2) ? m_bDX2_band_edge_pen : m_bDX2_tx_band_edge_pen);
			for (int n = 0; n < edges.Length; n++)
			{
				double num65 = (double)edges[n] - num47;
				if (num65 >= (double)num && num65 <= (double)num2)
				{
					int num66 = (int)((num65 - (double)num) / (double)num16 * (double)W);
					drawLineDX2D(b2, num66, nVerticalShift + num18, num66, nVerticalShift + H);
				}
			}
		}
		if (!bIsWaterfall)
		{
			SharpDX.Direct2D1.Brush b7;
			if ((!m_bHighlightNumberScaleRX1 || rx != 1) && (!m_bHighlightNumberScaleRX2 || rx != 2))
			{
				b7 = ((!flag2) ? m_bDX2_grid_text_brush : m_bDX2_grid_tx_text_brush);
			}
			else
			{
				if (rx == 1)
				{
					drawFillRectangleDX2D(m_bDX2_m_bHightlightNumberScale, console.RX1DisplayGridX, nVerticalShift + num18, console.RX1DisplayGridW - console.RX1DisplayGridX, H - num18);
				}
				else
				{
					drawFillRectangleDX2D(m_bDX2_m_bHightlightNumberScale, console.RX2DisplayGridX, nVerticalShift + num18, console.RX2DisplayGridW - console.RX2DisplayGridX, H - num18);
				}
				b7 = m_bDX2_m_bHightlightNumbers;
			}
			int num67 = 0;
			int num68 = 0;
			int num69 = (int)measureStringDX2D("-999", fontDX2d_font9).Width + 12;
			switch (displayLabelAlignment)
			{
			case DisplayLabelAlignment.LEFT:
				num67 = 0;
				num68 = num69;
				break;
			case DisplayLabelAlignment.CENTER:
				if (rx == 1 && (rx1_dsp_mode == DSPMode.USB || rx1_dsp_mode == DSPMode.DIGU || rx1_dsp_mode == DSPMode.CWU))
				{
					num67 = num46 - num69;
					num68 = num67 + num69;
				}
				else if (rx == 2 && (rx2_dsp_mode == DSPMode.USB || rx2_dsp_mode == DSPMode.DIGU || rx2_dsp_mode == DSPMode.CWU))
				{
					num67 = num46 - num69;
					num68 = num67 + num69;
				}
				else
				{
					num67 = num46;
					num68 = num67 + num69;
				}
				break;
			case DisplayLabelAlignment.RIGHT:
				num67 = W - num69;
				num68 = W;
				break;
			case DisplayLabelAlignment.AUTO:
				num67 = 0;
				num68 = num69;
				break;
			case DisplayLabelAlignment.OFF:
				num67 = W;
				num68 = W + num69;
				break;
			}
			for (int num70 = 1; num70 < num17; num70++)
			{
				int num71 = 0;
				int num72 = num7 - num70 * num9;
				int num73 = (int)((double)(num7 - num72) * (double)H / (double)num13);
				if (_grid_control_minor)
				{
					drawLineDX2D(flag2 ? m_bDX2_tx_hgrid_pen : m_bDX2_hgrid_pen, 0f, nVerticalShift + num73, W, nVerticalShift + num73);
				}
				if (num70 != 1)
				{
					string text2 = (num7 - num70 * num9).ToString();
					if (text2.Length == 3)
					{
						num71 = (int)measureStringDX2D("0", fontDX2d_font9).Width;
					}
					SizeF sizeF = measureStringDX2D(text2, fontDX2d_font9);
					int num74 = 0;
					switch (displayLabelAlignment)
					{
					case DisplayLabelAlignment.LEFT:
						num74 = num71 + 3;
						break;
					case DisplayLabelAlignment.CENTER:
						num74 = ((rx == 1 && (rx1_dsp_mode == DSPMode.USB || rx1_dsp_mode == DSPMode.DIGU || rx1_dsp_mode == DSPMode.CWU)) ? (num46 - num71 - (int)sizeF.Width) : ((rx != 2 || (rx2_dsp_mode != DSPMode.USB && rx2_dsp_mode != DSPMode.DIGU && rx2_dsp_mode != DSPMode.CWU)) ? (num46 + num71) : (num46 - num71 - (int)sizeF.Width)));
						break;
					case DisplayLabelAlignment.RIGHT:
						num74 = (int)((float)W - sizeF.Width - 3f);
						break;
					case DisplayLabelAlignment.AUTO:
						num74 = num71 + 3;
						break;
					case DisplayLabelAlignment.OFF:
						num74 = W;
						break;
					}
					num73 -= 8;
					if (num73 + 9 < H)
					{
						drawStringDX2D(text2, fontDX2d_font9, b7, num74, nVerticalShift + num73);
					}
				}
			}
			if (rx == 1)
			{
				console.RX1DisplayGridX = num67;
				console.RX1DisplayGridW = num68;
			}
			else
			{
				console.RX2DisplayGridX = num67;
				console.RX2DisplayGridW = num68;
			}
		}
		if (current_click_tune_mode != ClickTuneMode.Off)
		{
			SharpDX.Direct2D1.Brush b8 = ((current_click_tune_mode == ClickTuneMode.VFOA) ? m_bDX2_grid_text_pen : m_bDX2_Red);
			int num75 = nVerticalShift + num18;
			if ((rx != 1) ? ((current_display_mode_bottom == DisplayMode.PANAFALL) ? (display_cursor_y > 2 * H) : (display_cursor_y > H)) : (!_rx2_enabled || ((current_display_mode == DisplayMode.PANAFALL) ? (display_cursor_y <= 2 * H) : (display_cursor_y <= H))))
			{
				double num76 = _mouseFrequency + (double)num14;
				double num77 = _mouseFrequency + (double)num15;
				int num78 = (int)((num76 - (double)num) / (double)num16 * (double)W);
				int num79 = (int)((num77 - (double)num) / (double)num16 * (double)W);
				if (ClickTuneFilter)
				{
					if (((rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU) && rx == 1) || ((rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU) && rx == 2))
					{
						drawFillRectangleDX2D(m_bDX2_display_filter_brush, display_cursor_x - (num79 - num78) / 2, num75, num79 - num78, H - num18);
					}
					else
					{
						drawFillRectangleDX2D(m_bDX2_display_filter_brush, num78, num75, num79 - num78, H - num18);
					}
				}
				drawLineDX2D(b8, display_cursor_x, num75 - num18, display_cursor_x, num75 - num18 + H);
				if (ShowCTHLine)
				{
					drawLineDX2D(b8, 0f, display_cursor_y, W, display_cursor_y);
				}
			}
		}
		if (!bIsWaterfall && !flag2 && console.PowerOn && (((current_display_mode == DisplayMode.PANADAPTER || current_display_mode == DisplayMode.PANAFALL || current_display_mode == DisplayMode.PANASCOPE) && rx == 1) || ((current_display_mode_bottom == DisplayMode.PANADAPTER || current_display_mode_bottom == DisplayMode.PANAFALL || current_display_mode_bottom == DisplayMode.PANASCOPE) && rx == 2)))
		{
			int num80 = (int)((float)(num14 - num) / (float)num16 * (float)W);
			int num81 = (int)((float)(num15 - num) / (float)num16 * (float)W);
			if (num80 == num81)
			{
				num81 = num80 + 1;
			}
			int num82 = 0;
			int num83 = 0;
			int num84 = 0;
			int num85 = 0;
			int num86 = 0;
			int num87 = 0;
			int num88 = 0;
			int num89 = 0;
			if (rx == 1)
			{
				if (spectrum_line)
				{
					num82 = 40;
					num83 = W - 40;
				}
				else
				{
					num82 = num80;
					num83 = num81;
				}
				if (rx1_hang_spectrum_line)
				{
					num84 = W - 40;
					num85 = 50;
				}
				else
				{
					num84 = num81;
					num85 = num80;
				}
			}
			else
			{
				if (rx2_gain_spectrum_line)
				{
					num86 = 40;
					num87 = W - 40;
				}
				else
				{
					num86 = num80;
					num87 = num81;
				}
				if (rx2_hang_spectrum_line)
				{
					num88 = W - 40;
					num89 = 50;
				}
				else
				{
					num88 = num81;
					num89 = num80;
				}
			}
			if (rx == 1)
			{
				float num90 = 0f;
				num90 = ((console.RX1AGCMode != AGCMode.FIXD) ? (2f + (rx1_display_cal_offset + (rx1_preamp_offset - alex_preamp_offset) - rx1_fft_size_offset)) : (-18f));
				double num91 = 0.0;
				float num92 = 0f;
				double num93 = 0.0;
				float num94 = 0f;
				double size = console.specRX.GetSpecRX(0).FFTSize;
				WDSP.GetRXAAGCThresh(WDSP.id(0u, 0u), &num91, size, num12);
				WDSP.GetRXAAGCHangLevel(WDSP.id(0u, 0u), &num93);
				num91 = Math.Round(num91);
				int aGCFixedGain = console.SetupForm.AGCFixedGain;
				string text3 = "";
				if (console.RX1AGCMode == AGCMode.FIXD)
				{
					num92 = dBToPixel(0f - (float)aGCFixedGain + num90, H);
					text3 = (m_bAutoAGCRX1 ? "-Fa" : "-F");
				}
				else
				{
					num92 = dBToPixel((float)num91 + num90, H);
					num94 = dBToPixel((float)num93 + num90, H);
					num94 += (float)nVerticalShift;
					if (display_agc_hang_line && console.RX1AGCMode != AGCMode.MED && console.RX1AGCMode != AGCMode.FAST)
					{
						AGCHang.Height = 8;
						AGCHang.Width = 8;
						AGCHang.X = 40;
						AGCHang.Y = (int)num94 - AGCHang.Height;
						drawFillRectangleDX2D(m_bDX2_Yellow, AGCHang);
						drawLineDX2D(m_bDX2_Yellow, num85, num94, num84, num94, m_styleDots);
						drawStringDX2D("-H", fontDX2d_panafont, m_bDX2_pana_text_brush, AGCHang.X + AGCHang.Width, AGCHang.Y - AGCHang.Height / 2);
					}
					text3 = (m_bAutoAGCRX1 ? "-Ga" : "-G");
				}
				num92 += (float)nVerticalShift;
				if (show_agc)
				{
					AGCKnee.Height = 8;
					AGCKnee.Width = 8;
					AGCKnee.X = 40;
					AGCKnee.Y = (int)num92 - AGCKnee.Height;
					drawFillRectangleDX2D(m_bDX2_YellowGreen, AGCKnee);
					drawLineDX2D(m_bDX2_YellowGreen, num82, num92, num83, num92, m_styleDots);
					drawStringDX2D(text3, fontDX2d_panafont, m_bDX2_pana_text_brush, AGCKnee.X + AGCKnee.Width, AGCKnee.Y - AGCKnee.Height / 2);
				}
			}
			else
			{
				float num95 = 0f;
				double num96 = 0.0;
				float num97 = 0f;
				double num98 = 0.0;
				float num99 = 0f;
				string text4 = "";
				int num100 = 0;
				num95 = ((console.RX2AGCMode != AGCMode.FIXD) ? (2f + (rx2_display_cal_offset + rx2_preamp_offset) - rx2_fft_size_offset) : (-18f));
				double size2 = console.specRX.GetSpecRX(1).FFTSize;
				WDSP.GetRXAAGCThresh(WDSP.id(2u, 0u), &num96, size2, num12);
				num96 = Math.Round(num96);
				WDSP.GetRXAAGCHangLevel(WDSP.id(2u, 0u), &num98);
				num100 = console.SetupForm.AGCRX2FixedGain;
				if (console.RX2AGCMode == AGCMode.FIXD)
				{
					num97 = dBToRX2Pixel(0f - (float)num100 + num95, H);
					text4 = (m_bAutoAGCRX2 ? "-Fa" : "-F");
				}
				else
				{
					num97 = dBToRX2Pixel((float)num96 + num95, H);
					num99 = dBToRX2Pixel((float)num98 + num95, H);
					num99 += (float)nVerticalShift;
					if (display_rx2_hang_line && console.RX2AGCMode != AGCMode.MED && console.RX2AGCMode != AGCMode.FAST)
					{
						AGCRX2Hang.Height = 8;
						AGCRX2Hang.Width = 8;
						AGCRX2Hang.X = 40;
						AGCRX2Hang.Y = (int)num99 - AGCRX2Hang.Height;
						drawFillRectangleDX2D(m_bDX2_Yellow, AGCRX2Hang);
						drawLineDX2D(m_bDX2_Yellow, num89, num99, num88, num99, m_styleDots);
						drawStringDX2D("-H", fontDX2d_panafont, m_bDX2_pana_text_brush, AGCRX2Hang.X + AGCRX2Hang.Width, AGCRX2Hang.Y - AGCRX2Hang.Height / 2);
					}
					text4 = (m_bAutoAGCRX2 ? "-Ga" : "-G");
				}
				if (display_rx2_gain_line)
				{
					num97 += (float)nVerticalShift;
					AGCRX2Knee.Height = 8;
					AGCRX2Knee.Width = 8;
					AGCRX2Knee.X = 40;
					AGCRX2Knee.Y = (int)num97 - AGCRX2Knee.Height;
					drawFillRectangleDX2D(m_bDX2_YellowGreen, AGCRX2Knee);
					drawLineDX2D(m_bDX2_YellowGreen, num86, num97, num87, num97, m_styleDots);
					drawStringDX2D(text4, fontDX2d_panafont, m_bDX2_pana_text_brush, AGCRX2Knee.X + AGCRX2Knee.Width, AGCRX2Knee.Y - AGCRX2Knee.Height / 2);
				}
			}
		}
		_d2dRenderTarget.PopAxisAlignedClip();
		left_edge = num;
		right_edge = num2;
		return num46;
	}

	private static void AppendCursorInfoLines(List<string> lines, string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		string[] array = text.Replace("\r\n", "\n").Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]))
			{
				lines.Add(array[i]);
			}
		}
	}

	private static void DrawCursorInfoPanel(List<string> lines, int W, float yPos)
	{
		if (lines == null || lines.Count == 0)
		{
			return;
		}
		float height = measureStringDX2D("00:00:00", fontDX2d_callout).Height;
		float num = 0f;
		for (int i = 0; i < lines.Count; i++)
		{
			float width = measureStringDX2D(lines[i], fontDX2d_callout).Width;
			if (width > num)
			{
				num = width;
			}
		}
		float num2 = num + 10f;
		float height2 = (float)lines.Count * height + (float)(lines.Count - 1) * 2f + 6f;
		int num3 = display_cursor_x + 12;
		float num4 = (float)num3 - 5f;
		float y = yPos - 3f;
		if (num4 + num2 > (float)W)
		{
			num3 -= (int)num2 + 24;
			num4 = (float)num3 - 5f;
		}
		RoundedRectangle roundedRect = new RoundedRectangle
		{
			RadiusX = 4f,
			RadiusY = 4f,
			Rect = new SharpDX.RectangleF(num4, y, num2, height2)
		};
		_d2dRenderTarget.FillRoundedRectangle(roundedRect, getDXBrushForColour(System.Drawing.Color.Black, 144));
		_d2dRenderTarget.DrawRoundedRectangle(roundedRect, getDXBrushForColour(System.Drawing.Color.White, 96));
		float num5 = yPos;
		for (int j = 0; j < lines.Count; j++)
		{
			drawStringDX2D(lines[j], fontDX2d_callout, m_bDX2_m_bTextCallOutActive, num3, num5);
			num5 += height + 2f;
		}
	}

	private static void DrawCursorInfo(int W)
	{
		if (!_spot_highlighted && (m_bAlwaysShowCursorInfo || Common.ShiftKeyDown) && display_cursor_x != -1)
		{
			List<string> lines = new List<string>(4);
			AppendCursorInfoLines(lines, m_sMHzCursorDisplay);
			AppendCursorInfoLines(lines, m_sOtherData1CursorDisplay);
			AppendCursorInfoLines(lines, m_sOtherData2CursorDisplay);
			DrawCursorInfoPanel(lines, W, display_cursor_y - 18);
		}
	}

	private unsafe static bool DrawSpectrumDX2D(int rx, int W, int H, bool bottom)
	{
		DrawSpectrumGridDX2D(W, H, bottom);
		float num = float.MinValue;
		int num2 = 0;
		int num3 = 0;
		if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
		{
			_ = rx_spectrum_display_low;
			_ = rx_spectrum_display_high;
			num2 = spectrum_grid_max;
			num3 = spectrum_grid_min;
		}
		else
		{
			_ = tx_spectrum_display_low;
			_ = tx_spectrum_display_high;
			num2 = tx_spectrum_grid_max;
			num3 = tx_spectrum_grid_min;
		}
		_ = rx1_dsp_mode;
		_ = 11;
		int num4 = num2 - num3;
		int num5 = W / m_nDecimation;
		if (!bottom && data_ready)
		{
			if (_mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
			{
				for (int i = 0; i < num5; i++)
				{
					current_display_data[i] = (float)num3 - rx1_display_cal_offset;
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
						Win32.memcpy(destptr, srcptr, num5 * 4);
					}
				}
			}
			data_ready = false;
		}
		else if (bottom && data_ready_bottom)
		{
			fixed (float* ptr = &new_display_data_bottom[0])
			{
				void* srcptr2 = ptr;
				fixed (float* ptr2 = &current_display_data_bottom[0])
				{
					void* destptr2 = ptr2;
					Win32.memcpy(destptr2, srcptr2, num5 * 4);
				}
			}
			data_ready_bottom = false;
		}
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		float num6 = (bottom ? current_display_data_bottom[0] : current_display_data[0]);
		num6 += ((rx == 1) ? RX1Offset : RX2Offset);
		if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
		{
			num6 = ((rx != 1) ? (num6 + rx2_preamp_offset) : (num6 + (rx1_preamp_offset - alex_preamp_offset)));
		}
		int num7 = (int)Math.Min(Math.Floor(((float)num2 - num6) * (float)H / (float)num4), H);
		vector2.X = 0f;
		vector2.Y = num7;
		float num8 = ((rx == 1) ? RX1Offset : RX2Offset);
		for (int j = 0; j < num5; j++)
		{
			num6 = (bottom ? current_display_data_bottom[j] : current_display_data[j]);
			num6 += num8;
			if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
			{
				num6 = ((rx != 1) ? (num6 + rx2_preamp_offset) : (num6 + (rx1_preamp_offset - alex_preamp_offset)));
			}
			if (num6 > num)
			{
				num = num6;
				max_x = j * m_nDecimation;
			}
			num7 = (int)Math.Min(Math.Floor(((float)num2 - num6) * (float)H / (float)num4), H);
			vector.X = j * m_nDecimation;
			vector.Y = num7;
			_d2dRenderTarget.DrawLine(vector2, vector, m_bDX2_data_line_pen_brush, sw(data_line_pen.Width));
			vector2.X = vector.X;
			vector2.Y = vector.Y;
		}
		max_y = num;
		if (current_click_tune_mode != ClickTuneMode.Off)
		{
			SharpDX.Direct2D1.Brush b = ((current_click_tune_mode == ClickTuneMode.VFOA) ? m_bDX2_grid_text_brush : m_bDX2_Red);
			if (bottom)
			{
				drawLineDX2D(b, display_cursor_x, H, display_cursor_x, H + H, grid_text_pen.Width);
				drawLineDX2D(b, 0f, display_cursor_y, W, display_cursor_y, grid_text_pen.Width);
			}
			else
			{
				drawLineDX2D(b, display_cursor_x, 0f, display_cursor_x, H, grid_text_pen.Width);
				drawLineDX2D(b, 0f, display_cursor_y, W, display_cursor_y, grid_text_pen.Width);
			}
		}
		return true;
	}

	private static void DrawSpectrumGridDX2D(int W, int H, bool bottom)
	{
		drawFillRectangleDX2D(m_bDX2_display_background_brush, 0f, bottom ? H : 0, W, H);
		int num = 0;
		int num2 = 0;
		if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
		{
			num = rx_spectrum_display_low;
			num2 = rx_spectrum_display_high;
		}
		else if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
		{
			num = rx_spectrum_display_low;
			num2 = rx_spectrum_display_high;
		}
		else
		{
			num = tx_spectrum_display_low;
			num2 = tx_spectrum_display_high;
		}
		int num3 = (int)((0.0 - (double)num) / (double)(num2 - num) * (double)W);
		int num4 = W / 2;
		int[] array = new int[4] { 10, 20, 25, 50 };
		int num5 = 1;
		int num6 = 0;
		int num7 = 50;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
		{
			num8 = spectrum_grid_max;
			num9 = spectrum_grid_min;
			num10 = spectrum_grid_step;
		}
		else if (_mox)
		{
			num8 = tx_spectrum_grid_max;
			num9 = tx_spectrum_grid_min;
			num10 = tx_spectrum_grid_step;
		}
		int num11 = num8 - num9;
		if (split_display)
		{
			num10 *= 2;
		}
		if (num2 == 0)
		{
			int num12 = -num;
			while (num12 / num7 > 7)
			{
				num7 = array[num6] * (int)Math.Pow(10.0, num5);
				num6 = (num6 + 1) % 4;
				if (num6 == 0)
				{
					num5++;
				}
			}
			float num13 = W * num7 / num12;
			int num14 = num12 / num7;
			for (int i = 1; i <= num14; i++)
			{
				int num15 = W - (int)Math.Floor((float)i * num13);
				if (bottom)
				{
					drawLineDX2D(m_bDX2_grid_pen, num15, H, num15, H + H);
				}
				else
				{
					drawLineDX2D(m_bDX2_grid_pen, num15, 0f, num15, H);
				}
				string text = (i * num7).ToString();
				int num16 = GridLabelOffset(text, (float)(text.Length + 1) * 4.1f);
				if (num15 - num16 >= 0)
				{
					if (bottom)
					{
						drawStringDX2D("-" + text, fontDX2d_font9, m_bDX2_grid_text_brush, num15 - num16, (float)H + (float)Math.Floor((double)H * 0.01));
					}
					else
					{
						drawStringDX2D("-" + text, fontDX2d_font9, m_bDX2_grid_text_brush, num15 - num16, (float)Math.Floor((double)H * 0.01));
					}
				}
			}
			num14 = (num8 - num9) / num10;
			num13 = H / num14;
			for (int j = 1; j < num14; j++)
			{
				int num17 = 0;
				int num18 = num8 - j * num10;
				int num19 = (int)Math.Floor((double)(num8 - num18) * (double)H / (double)num11);
				if (bottom)
				{
					drawLineDX2D(m_bDX2_hgrid_pen, 0f, H + num19, W, H + num19);
				}
				else
				{
					drawLineDX2D(m_bDX2_hgrid_pen, 0f, num19, W, num19);
				}
				if (j == 1)
				{
					continue;
				}
				string text2 = num18.ToString();
				if (text2.Length == 3)
				{
					num17 = (int)measureStringDX2D("0", fontDX2d_font9).Width;
				}
				SizeF sizeF = measureStringDX2D(text2, fontDX2d_font9);
				int num20 = 0;
				switch (display_label_align)
				{
				case DisplayLabelAlignment.LEFT:
					num20 = num17 + 3;
					break;
				case DisplayLabelAlignment.CENTER:
					num20 = num3 + num17;
					break;
				case DisplayLabelAlignment.RIGHT:
					num20 = (int)((float)W - sizeF.Width - 3f);
					break;
				case DisplayLabelAlignment.AUTO:
					num20 = num17 + 3;
					break;
				case DisplayLabelAlignment.OFF:
					num20 = W;
					break;
				}
				console.RX1DisplayGridX = num20;
				console.RX1DisplayGridW = (int)((float)num20 + sizeF.Width);
				num19 -= 8;
				if (num19 + 9 < H)
				{
					if (bottom)
					{
						drawStringDX2D(text2, fontDX2d_font9, m_bDX2_grid_text_brush, num20, H + num19);
					}
					else
					{
						drawStringDX2D(text2, fontDX2d_font9, m_bDX2_grid_text_brush, num20, num19);
					}
				}
			}
			if (rx1_dsp_mode == DSPMode.AM || rx1_dsp_mode == DSPMode.SAM || rx1_dsp_mode == DSPMode.FM || rx1_dsp_mode == DSPMode.DSB || rx1_dsp_mode == DSPMode.SPEC)
			{
				if (bottom)
				{
					drawLineDX2D(m_bDX2_grid_zero_pen, W - 1, H, W - 1, H + H);
					drawLineDX2D(m_bDX2_grid_zero_pen, W - 2, H, W - 2, H + H);
				}
				else
				{
					drawLineDX2D(m_bDX2_grid_zero_pen, W - 1, 0f, W - 1, H);
					drawLineDX2D(m_bDX2_grid_zero_pen, W - 2, 0f, W - 2, H);
				}
			}
		}
		else if (num == 0)
		{
			int num21 = num2;
			while (num21 / num7 > 7)
			{
				num7 = array[num6] * (int)Math.Pow(10.0, num5);
				num6 = (num6 + 1) % 4;
				if (num6 == 0)
				{
					num5++;
				}
			}
			float num22 = W * num7 / num21;
			int num23 = num21 / num7;
			for (int k = 1; k <= num23; k++)
			{
				int num24 = (int)Math.Floor((float)k * num22);
				if (bottom)
				{
					drawLineDX2D(m_bDX2_grid_pen, num24, H, num24, H + H);
				}
				else
				{
					drawLineDX2D(m_bDX2_grid_pen, num24, 0f, num24, H);
				}
				string text3 = (k * num7).ToString();
				int num25 = GridLabelOffset(text3, (float)text3.Length * 4.1f);
				if (num24 - num25 + text3.Length * 7 < W)
				{
					if (bottom)
					{
						drawStringDX2D(text3, fontDX2d_font9, m_bDX2_grid_text_brush, num24 - num25, (float)H + (float)Math.Floor((double)H * 0.01));
					}
					else
					{
						drawStringDX2D(text3, fontDX2d_font9, m_bDX2_grid_text_brush, num24 - num25, (float)Math.Floor((double)H * 0.01));
					}
				}
			}
			int num26 = (num8 - num9) / num10;
			num22 = H / num26;
			for (int l = 1; l < num26; l++)
			{
				int num27 = 0;
				int num28 = num8 - l * num10;
				int num29 = (int)Math.Floor((double)(num8 - num28) * (double)H / (double)num11);
				if (bottom)
				{
					drawLineDX2D(m_bDX2_hgrid_pen, 0f, H + num29, W, H + num29);
				}
				else
				{
					drawLineDX2D(m_bDX2_hgrid_pen, 0f, num29, W, num29);
				}
				if (l == 1)
				{
					continue;
				}
				string text4 = num28.ToString();
				if (text4.Length == 3)
				{
					num27 = (int)measureStringDX2D("-", fontDX2d_font9).Width - 2;
				}
				GridLabelOffset(text4, (float)text4.Length * 4.1f);
				SizeF sizeF2 = measureStringDX2D(text4, fontDX2d_font9);
				int num30 = 0;
				switch (display_label_align)
				{
				case DisplayLabelAlignment.LEFT:
					num30 = num27 + 3;
					break;
				case DisplayLabelAlignment.CENTER:
					num30 = num3 + num27;
					break;
				case DisplayLabelAlignment.RIGHT:
					num30 = (int)((float)W - sizeF2.Width - 3f);
					break;
				case DisplayLabelAlignment.AUTO:
					num30 = num27 + 3;
					break;
				case DisplayLabelAlignment.OFF:
					num30 = W;
					break;
				}
				console.RX1DisplayGridX = num30;
				console.RX1DisplayGridW = (int)((float)num30 + sizeF2.Width);
				num29 -= 8;
				if (num29 + 9 < H)
				{
					if (bottom)
					{
						drawStringDX2D(text4, fontDX2d_font9, m_bDX2_grid_text_brush, num30, H + num29);
					}
					drawStringDX2D(text4, fontDX2d_font9, m_bDX2_grid_text_brush, num30, num29);
				}
			}
			if (rx1_dsp_mode == DSPMode.AM || rx1_dsp_mode == DSPMode.SAM || rx1_dsp_mode == DSPMode.FM || rx1_dsp_mode == DSPMode.DSB || rx1_dsp_mode == DSPMode.SPEC)
			{
				if (bottom)
				{
					drawLineDX2D(m_bDX2_grid_pen, 0f, H, 0f, H + H);
					drawLineDX2D(m_bDX2_grid_pen, 1f, H, 1f, H + H);
				}
				else
				{
					drawLineDX2D(m_bDX2_grid_pen, 0f, 0f, 0f, H);
					drawLineDX2D(m_bDX2_grid_pen, 1f, 0f, 1f, H);
				}
			}
		}
		else
		{
			if (num >= 0 || num2 <= 0)
			{
				return;
			}
			int num31 = num2;
			while (num31 / num7 > 4)
			{
				num7 = array[num6] * (int)Math.Pow(10.0, num5);
				num6 = (num6 + 1) % 4;
				if (num6 == 0)
				{
					num5++;
				}
			}
			int num32 = W / 2 * num7 / num31;
			int num33 = num31 / num7;
			for (int m = 1; m <= num33; m++)
			{
				int num34 = num4 - m * num32;
				int num35 = num4 + m * num32;
				if (bottom)
				{
					drawLineDX2D(m_bDX2_grid_pen, num34, H, num34, H + H);
					drawLineDX2D(m_bDX2_grid_pen, num35, H, num35, H + H);
				}
				else
				{
					drawLineDX2D(m_bDX2_grid_pen, num34, 0f, num34, H);
					drawLineDX2D(m_bDX2_grid_pen, num35, 0f, num35, H);
				}
				string text5 = (m * num7).ToString();
				int num36 = GridLabelOffset(text5, (float)(text5.Length + 1) * 4.1f);
				int num37 = GridLabelOffset(text5, (float)text5.Length * 4.1f);
				if (num34 - num36 >= 0)
				{
					if (bottom)
					{
						drawStringDX2D("-" + text5, fontDX2d_font9, m_bDX2_grid_text_brush, num34 - num36, (float)H + (float)Math.Floor((double)H * 0.01));
						drawStringDX2D(text5, fontDX2d_font9, m_bDX2_grid_text_brush, num35 - num37, (float)H + (float)Math.Floor((double)H * 0.01));
					}
					else
					{
						drawStringDX2D("-" + text5, fontDX2d_font9, m_bDX2_grid_text_brush, num34 - num36, (float)Math.Floor((double)H * 0.01));
						drawStringDX2D(text5, fontDX2d_font9, m_bDX2_grid_text_brush, num35 - num37, (float)Math.Floor((double)H * 0.01));
					}
				}
			}
			int num38 = (num8 - num9) / num10;
			num32 = H / num38;
			for (int n = 1; n < num38; n++)
			{
				int num39 = 0;
				int num40 = num8 - n * num10;
				int num41 = (int)Math.Floor((double)(num8 - num40) * (double)H / (double)num11);
				if (bottom)
				{
					drawLineDX2D(m_bDX2_grid_pen, 0f, H + num41, W, H + num41);
				}
				else
				{
					drawLineDX2D(m_bDX2_grid_pen, 0f, num41, W, num41);
				}
				if (n == 1)
				{
					continue;
				}
				string text6 = num40.ToString();
				if (text6.Length == 3)
				{
					num39 = (int)measureStringDX2D("-", fontDX2d_font9).Width - 2;
				}
				GridLabelOffset(text6, (float)text6.Length * 4.1f);
				SizeF sizeF3 = measureStringDX2D(text6, fontDX2d_font9);
				int num42 = 0;
				switch (display_label_align)
				{
				case DisplayLabelAlignment.LEFT:
					num42 = num39 + 3;
					break;
				case DisplayLabelAlignment.CENTER:
					num42 = num3 + num39;
					break;
				case DisplayLabelAlignment.RIGHT:
					num42 = (int)((float)W - sizeF3.Width - 3f);
					break;
				case DisplayLabelAlignment.AUTO:
					num42 = num39 + 3;
					break;
				case DisplayLabelAlignment.OFF:
					num42 = W;
					break;
				}
				console.RX1DisplayGridX = num42;
				console.RX1DisplayGridW = (int)((float)num42 + sizeF3.Width);
				num41 -= 8;
				if (num41 + 9 < H)
				{
					if (bottom)
					{
						drawStringDX2D(text6, fontDX2d_font9, m_bDX2_grid_text_brush, num42, H + num41);
					}
					drawStringDX2D(text6, fontDX2d_font9, m_bDX2_grid_text_brush, num42, num41);
				}
			}
			if (rx1_dsp_mode == DSPMode.AM || rx1_dsp_mode == DSPMode.SAM || rx1_dsp_mode == DSPMode.FM || rx1_dsp_mode == DSPMode.DSB || rx1_dsp_mode == DSPMode.SPEC)
			{
				if (bottom)
				{
					drawLineDX2D(m_bDX2_grid_zero_pen, num4, H, num4, H + H);
					drawLineDX2D(m_bDX2_grid_zero_pen, num4 - 1, H, num4 - 1, H + H);
				}
				else
				{
					drawLineDX2D(m_bDX2_grid_zero_pen, num4, 0f, num4, H);
					drawLineDX2D(m_bDX2_grid_zero_pen, num4 - 1, 0f, num4 - 1, H);
				}
			}
		}
	}

	private static bool DrawScopeDX2D(int W, int H, bool bottom)
	{
		int num = W / m_nDecimation;
		if (scope_min == null || scope_min.Length != num)
		{
			scope_min = new float[num];
			Audio.ScopeMin = scope_min;
			return false;
		}
		if (scope_max == null || scope_max.Length != num)
		{
			scope_max = new float[num];
			Audio.ScopeMax = scope_max;
			return false;
		}
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		int num2 = (int)((float)H * 0.5f);
		Vector2 vector3 = default(Vector2);
		Vector2 vector4 = default(Vector2);
		vector3.X = (vector4.X = 0f);
		int num3 = (int)((float)(H / 2) * scope_max[0]);
		vector3.Y = H / 2 - num3;
		num3 = (int)((float)(H / 2) * scope_min[0]);
		vector4.Y = H / 2 - num3;
		if (bottom)
		{
			vector3.Y += H;
			vector4.Y += H;
			num2 += H;
		}
		drawLineDX2D(m_bDX2_y2_brush, 0f, num2, W, num2);
		for (int i = 1; i < num; i++)
		{
			vector2.X = i * m_nDecimation;
			vector.X = vector2.X;
			num3 = (int)((float)(H / 2) * scope_max[i]);
			vector2.Y = H / 2 - num3;
			num3 = (int)((float)(H / 2) * scope_min[i]);
			vector.Y = H / 2 - num3;
			vector2.Y -= 0.5f;
			vector.Y -= 0.5f;
			if (bottom)
			{
				vector2.Y += H;
				vector.Y += H;
			}
			if (vector3.Y > vector.Y)
			{
				_d2dRenderTarget.DrawLine(vector3, vector, m_bDX2_waveform_line_pen, sw(1f));
			}
			if (vector4.Y < vector2.Y)
			{
				_d2dRenderTarget.DrawLine(vector4, vector2, m_bDX2_waveform_line_pen, sw(1f));
			}
			if (vector == vector2)
			{
				_d2dRenderTarget.FillRectangle(new SharpDX.RectangleF(vector.X, vector.Y, 1f, 1f), m_bDX2_waveform_line_pen);
			}
			else
			{
				_d2dRenderTarget.DrawLine(vector, vector2, m_bDX2_waveform_line_pen, sw(1f));
			}
			vector3.X = i * m_nDecimation;
			vector4.X = vector3.X;
			vector3.Y = vector2.Y;
			vector4.Y = vector.Y;
		}
		if (current_click_tune_mode != ClickTuneMode.Off)
		{
			SharpDX.Direct2D1.Brush b = ((current_click_tune_mode == ClickTuneMode.VFOA) ? m_bDX2_grid_text_pen : m_bDX2_Red);
			if (bottom)
			{
				drawLineDX2D(b, display_cursor_x, 0f, display_cursor_x, H + H);
			}
			else
			{
				drawLineDX2D(b, display_cursor_x, 0f, display_cursor_x, H);
			}
			drawLineDX2D(b, 0f, display_cursor_y, W, display_cursor_y);
		}
		return true;
	}

	private static bool DrawScope2DX2D(int W, int H, bool bottom)
	{
		int num = W / m_nDecimation;
		if (scope_min == null || scope_min.Length != num)
		{
			scope_min = new float[num];
			Audio.ScopeMin = scope_min;
		}
		if (scope_max == null || scope_max.Length != num)
		{
			scope_max = new float[num];
			Audio.ScopeMax = scope_max;
		}
		if (scope2_min == null || scope2_min.Length != num)
		{
			scope2_min = new float[num];
			Audio.Scope2Min = scope2_min;
		}
		if (scope2_max == null || scope2_max.Length != num)
		{
			scope2_max = new float[num];
			Audio.Scope2Max = scope2_max;
		}
		int num2 = (int)((float)H * 0.25f);
		int num3 = (int)((float)H * 0.5f);
		int num4 = (int)((float)H * 0.75f);
		drawLineDX2D(m_bDX2_y1_brush, 0f, num2, W, num2);
		drawLineDX2D(m_bDX2_y2_brush, 0f, num3, W, num3);
		drawLineDX2D(m_bDX2_y1_brush, 0f, num4, W, num4);
		float num5 = (float)H / 4f;
		Vector2 vector = default(Vector2);
		Vector2 vector2 = new Vector2
		{
			X = 0f,
			Y = (int)((float)num2 - scope2_max[0] * num5)
		};
		for (int i = 0; i < num; i++)
		{
			int num6 = i;
			int num7 = (int)((float)num2 - scope2_max[num6] * num5);
			vector.X = i * m_nDecimation;
			vector.Y = num7;
			_d2dRenderTarget.DrawLine(vector2, vector, m_bDX2_waveform_line_pen, sw(1f));
			vector2.X = vector.X;
			vector2.Y = vector.Y;
		}
		vector2.X = 0f;
		vector2.Y = (int)((float)num4 - scope_max[0] * num5);
		for (int j = 0; j < num; j++)
		{
			int num8 = j;
			int num9 = (int)((float)num4 - scope_max[num8] * num5);
			vector.X = j * m_nDecimation;
			vector.Y = num9;
			_d2dRenderTarget.DrawLine(vector2, vector, m_bDX2_waveform_line_pen, sw(1f));
			vector2.X = vector.X;
			vector2.Y = vector.Y;
		}
		return true;
	}

	private static float lerp(float first, float second, float by)
	{
		return first * (1f - by) + second * by;
	}

	private static PointF lerpPointF(PointF first, PointF second, float by)
	{
		return new PointF(lerp(first.X, second.X, by), lerp(first.Y, second.Y, by));
	}

	private unsafe static bool DrawPhaseDX2D(int W, int H, bool bottom)
	{
		DrawPhaseGridDX2D(W, H, bottom);
		int num = phase_num_pts;
		if (!bottom && data_ready)
		{
			fixed (float* ptr = &new_display_data[0])
			{
				void* srcptr = ptr;
				fixed (float* ptr2 = &current_display_data[0])
				{
					void* destptr = ptr2;
					Win32.memcpy(destptr, srcptr, num * 2 * 4);
				}
			}
			data_ready = false;
		}
		else if (bottom && data_ready_bottom)
		{
			fixed (float* ptr = &new_display_data_bottom[0])
			{
				void* srcptr2 = ptr;
				fixed (float* ptr2 = &current_display_data_bottom[0])
				{
					void* destptr2 = ptr2;
					Win32.memcpy(destptr2, srcptr2, num * 2 * 4);
				}
			}
			data_ready_bottom = false;
		}
		int num2 = m_nPhasePointSize / 2;
		Vector2 vector = default(Vector2);
		double num3 = 0.0;
		double num4 = 0.0;
		for (int i = 0; i < num; i++)
		{
			int num5 = 0;
			int num6 = 0;
			if (bottom)
			{
				num3 += (double)current_display_data_bottom[i * 2];
				num4 += (double)current_display_data_bottom[i * 2 + 1];
				num5 = (int)(current_display_data_bottom[i * 2] * (float)H / 2f);
				num6 = (int)(current_display_data_bottom[i * 2 + 1] * (float)H / 2f);
			}
			else
			{
				num3 += (double)current_display_data[i * 2];
				num4 += (double)current_display_data[i * 2 + 1];
				num5 = (int)(current_display_data[i * 2] * (float)H / 2f);
				num6 = (int)(current_display_data[i * 2 + 1] * (float)H / 2f);
			}
			vector.X = W / 2 + num5;
			vector.Y = H / 2 + num6;
			if (bottom)
			{
				vector.Y += H;
			}
			drawFillRectangleDX2D(m_bDX2_data_line_pen_brush, vector.X - (float)num2, vector.Y - (float)num2, m_nPhasePointSize, m_nPhasePointSize);
		}
		double num7 = Math.Atan2(num3, num4);
		float x = (float)Math.Sin(num7);
		float y = (float)Math.Cos(num7);
		PointF pointF = lerpPointF(m_dOldCM, new PointF(x, y), (float)((m_dElapsedFrameStart - m_dLastAngleLerp) / 50.0));
		m_dOldCM.X = pointF.X;
		m_dOldCM.Y = pointF.Y;
		m_dLastAngleLerp = m_dElapsedFrameStart;
		if (m_bShowPhaseAngularMean)
		{
			double num8 = Math.Atan2(pointF.X, pointF.Y);
			x = (float)Math.Sin(num8) * (float)H / 2f;
			y = (float)Math.Cos(num8) * (float)H / 2f;
			x += (float)(W / 2);
			y += (float)(H / 2);
			if (bottom)
			{
				y += (float)H;
			}
			Vector2 vector2 = new Vector2(W / 2, H / 2);
			Vector2 vector3 = new Vector2(x, y);
			_d2dRenderTarget.DrawLine(vector2, vector3, m_bDX2_Red, sw(3f));
		}
		if (current_click_tune_mode != ClickTuneMode.Off)
		{
			SharpDX.Direct2D1.Brush b = ((current_click_tune_mode == ClickTuneMode.VFOA) ? m_bDX2_grid_text_pen : m_bDX2_Red);
			if (bottom)
			{
				drawLineDX2D(b, display_cursor_x, 0f, display_cursor_x, H + H);
			}
			else
			{
				drawLineDX2D(b, display_cursor_x, 0f, display_cursor_x, H);
			}
			drawLineDX2D(b, 0f, display_cursor_y, W, display_cursor_y);
		}
		return true;
	}

	private static void DrawPhaseGridDX2D(int W, int H, bool bottom)
	{
		drawFillRectangleDX2D(m_bDX2_display_background_brush, 0f, bottom ? H : 0, W, H);
		for (double num = 0.5; num < 3.0; num += 0.5)
		{
			if (bottom)
			{
				drawElipseDX2D(m_bDX2_grid_pen, W / 2, H + H / 2, (int)((double)H * num), (int)((double)H * num));
			}
			else
			{
				drawElipseDX2D(m_bDX2_grid_pen, W / 2, H / 2, (int)((double)H * num), (int)((double)H * num));
			}
		}
	}

	private unsafe static void DrawPhase2DX2D(int W, int H, bool bottom)
	{
		DrawPhaseGridDX2D(W, H, bottom);
		int num = phase_num_pts;
		if (!bottom && data_ready)
		{
			fixed (float* ptr = &new_display_data[0])
			{
				void* srcptr = ptr;
				fixed (float* ptr2 = &current_display_data[0])
				{
					void* destptr = ptr2;
					Win32.memcpy(destptr, srcptr, num * 2 * 4);
				}
			}
			data_ready = false;
		}
		else if (bottom && data_ready_bottom)
		{
			fixed (float* ptr = &new_display_data_bottom[0])
			{
				void* srcptr2 = ptr;
				fixed (float* ptr2 = &current_display_data_bottom[0])
				{
					void* destptr2 = ptr2;
					Win32.memcpy(destptr2, srcptr2, num * 2 * 4);
				}
			}
			data_ready_bottom = false;
		}
		int num2 = m_nPhasePointSize / 2;
		Vector2 vector = default(Vector2);
		for (int i = 0; i < num; i++)
		{
			int num3 = 0;
			int num4 = 0;
			if (bottom)
			{
				num3 = (int)((double)(current_display_data_bottom[i * 2] * (float)H) * 0.5 * 500.0);
				num4 = (int)((double)(current_display_data_bottom[i * 2 + 1] * (float)H) * 0.5 * 500.0);
			}
			else
			{
				num3 = (int)((double)(current_display_data[i * 2] * (float)H) * 0.5 * 500.0);
				num4 = (int)((double)(current_display_data[i * 2 + 1] * (float)H) * 0.5 * 500.0);
			}
			vector.X = (int)((double)W * 0.5 + (double)num3);
			vector.Y = (int)((double)H * 0.5 + (double)num4);
			if (bottom)
			{
				vector.Y += H;
			}
			drawFillRectangleDX2D(m_bDX2_data_line_pen_brush, vector.X - (float)num2, vector.Y - (float)num2, m_nPhasePointSize, m_nPhasePointSize);
		}
		if (current_click_tune_mode != ClickTuneMode.Off)
		{
			SharpDX.Direct2D1.Brush b = ((current_click_tune_mode == ClickTuneMode.VFOA) ? m_bDX2_grid_text_pen : m_bDX2_Red);
			if (bottom)
			{
				drawLineDX2D(b, display_cursor_x, 0f, display_cursor_x, H + H);
			}
			else
			{
				drawLineDX2D(b, display_cursor_x, 0f, display_cursor_x, H);
			}
			drawLineDX2D(b, 0f, display_cursor_y, W, display_cursor_y);
		}
	}

	private unsafe static bool DrawHistogramDX2D(int rx, int W, int H)
	{
		DrawSpectrumGridDX2D(W, H, bottom: false);
		updateSharePointsArray(W);
		float num = -2.1474836E+09f;
		int num2 = 0;
		int num3 = 0;
		if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
		{
			_ = rx_spectrum_display_low;
			_ = rx_spectrum_display_high;
			num2 = spectrum_grid_max;
			num3 = spectrum_grid_min;
		}
		else
		{
			_ = tx_spectrum_display_low;
			_ = tx_spectrum_display_high;
			num2 = tx_spectrum_grid_max;
			num3 = tx_spectrum_grid_min;
		}
		_ = rx1_dsp_mode;
		_ = 11;
		int num4 = W / m_nDecimation;
		int num5 = num2 - num3;
		if (data_ready)
		{
			fixed (float* ptr = &new_display_data[0])
			{
				void* srcptr = ptr;
				fixed (float* ptr2 = &current_display_data[0])
				{
					void* destptr = ptr2;
					Win32.memcpy(destptr, srcptr, num4 * 4);
				}
			}
			data_ready = false;
		}
		int num6 = 0;
		float num7 = ((rx == 1) ? RX1Offset : RX2Offset);
		for (int i = 0; i < num4; i++)
		{
			float num8 = (num8 = current_display_data[i]);
			num8 += num7;
			if (!_mox)
			{
				num8 += rx1_preamp_offset - alex_preamp_offset;
			}
			if (rx1_dsp_mode == DSPMode.SPEC)
			{
				num8 += 6f;
			}
			if (num8 > num)
			{
				num = num8;
				max_x = i * m_nDecimation;
			}
			points[i].X = i * m_nDecimation;
			points[i].Y = (int)Math.Min(Math.Floor(((float)num2 - num8) * (float)H / (float)num5), H);
			num6 += points[i].Y;
		}
		max_y = num;
		float num9 = 0f;
		num9 = (float)((double)((float)num6 / (float)num4) / 1.12);
		for (int j = 0; j < num4; j++)
		{
			if (points[j].Y < histogram_data[j])
			{
				histogram_history[j] = 0;
				histogram_data[j] = points[j].Y;
			}
			else
			{
				histogram_history[j]++;
				if (histogram_history[j] > 51)
				{
					histogram_history[j] = 0;
					histogram_data[j] = points[j].Y;
				}
				int num10 = Math.Max(255 - histogram_history[j] * 5, 0);
				int num11 = points[j].Y - histogram_data[j];
				m_bDX2_dhp.Opacity = (float)num10 / 255f;
				drawFillRectangleDX2D(m_bDX2_dhp, j * m_nDecimation, histogram_data[j], m_nDecimation, num11);
			}
			if ((float)points[j].Y >= num9)
			{
				drawFillRectangleDX2D(m_bDX2_dhp1, points[j].X, points[j].Y, m_nDecimation, H - points[j].Y);
				continue;
			}
			drawFillRectangleDX2D(m_bDX2_dhp1, points[j].X, (int)Math.Floor(num9), m_nDecimation, H - (int)Math.Floor(num9));
			drawFillRectangleDX2D(m_bDX2_dhp2, points[j].X, points[j].Y, m_nDecimation, (int)Math.Floor(num9) - points[j].Y);
		}
		if (current_click_tune_mode != ClickTuneMode.Off)
		{
			SharpDX.Direct2D1.Brush b = ((current_click_tune_mode == ClickTuneMode.VFOA) ? m_bDX2_grid_text_pen : m_bDX2_Red);
			drawLineDX2D(b, display_cursor_x, 0f, display_cursor_x, H);
			drawLineDX2D(b, 0f, display_cursor_y, W, display_cursor_y);
		}
		return true;
	}

	private static int getSpotLayer(int rx, int leftX)
	{
		if (rx == 1)
		{
			if (_spotLayerRightRX1 == null)
			{
				_spotLayerRightRX1 = new List<int>();
			}
			for (int i = 0; i < _spotLayerRightRX1.Count; i++)
			{
				int num = _spotLayerRightRX1[i];
				if (leftX > num)
				{
					return i;
				}
			}
			_spotLayerRightRX1.Add(int.MinValue);
			return _spotLayerRightRX1.Count - 1;
		}
		if (_spotLayerRightRX2 == null)
		{
			_spotLayerRightRX2 = new List<int>();
		}
		for (int j = 0; j < _spotLayerRightRX2.Count; j++)
		{
			int num2 = _spotLayerRightRX2[j];
			if (leftX > num2)
			{
				return j;
			}
		}
		_spotLayerRightRX2.Add(int.MinValue);
		return _spotLayerRightRX2.Count - 1;
	}

	private static void updateLayer(int rx, int layer, int rightX)
	{
		if (layer < 0)
		{
			return;
		}
		if (rx == 1)
		{
			if (layer < _spotLayerRightRX1.Count)
			{
				_spotLayerRightRX1[layer] = rightX;
			}
		}
		else if (layer < _spotLayerRightRX2.Count)
		{
			_spotLayerRightRX2[layer] = rightX;
		}
	}

	private static string getCallsignString(SpotManager2.smSpot spot)
	{
		if (_bShiftKeyDown)
		{
			if (!string.IsNullOrEmpty(spot.spotter))
			{
				return spot.spotter;
			}
			return spot.callsign;
		}
		return spot.callsign;
	}

	public static void drawSpots(int rx, int nVerticalShift, int W, bool bottom)
	{
		if (bottom)
		{
			return;
		}
		SpotManager2.smSpot[] frequencySortedSpots = SpotManager2.GetFrequencySortedSpots();
		if (frequencySortedSpots.Length == 0)
		{
			_spot_highlighted = false;
			return;
		}
		int num = rx - 1;
		bool flag = localMox(rx);
		bool flag2 = isRxDuplex(rx);
		long num2;
		int num3;
		int num4;
		int num5;
		if (rx == 1)
		{
			num2 = vfoa_hz;
			if (flag)
			{
				if (flag2)
				{
					num3 = RXDisplayLow;
					num4 = RXDisplayHigh;
				}
				else
				{
					num3 = TXDisplayLow;
					num4 = TXDisplayHigh;
				}
			}
			else
			{
				num3 = RXDisplayLow;
				num4 = RXDisplayHigh;
			}
			_spotLayerRightRX1.Clear();
			num5 = ((!_rx1ClickDisplayCTUN) ? rit_hz : 0);
		}
		else
		{
			num2 = vfob_hz;
			if (flag)
			{
				if (flag2)
				{
					num3 = RX2DisplayLow;
					num4 = RX2DisplayHigh;
				}
				else
				{
					num3 = TXDisplayLow;
					num4 = TXDisplayHigh;
				}
			}
			else
			{
				num3 = RX2DisplayLow;
				num4 = RX2DisplayHigh;
			}
			_spotLayerRightRX2.Clear();
			num5 = 0;
		}
		int num6 = nVerticalShift + 20;
		long num7 = num2 + num3;
		long num8 = num2 + num4;
		long num9 = num8 - num7;
		float num10 = (float)W / (float)num9;
		int cWSideToneShift = getCWSideToneShift(rx);
		foreach (SpotManager2.smSpot smSpot in frequencySortedSpots)
		{
			if (smSpot.frequencyHZ < num7 || smSpot.frequencyHZ > num8)
			{
				smSpot.Visible[num] = false;
				continue;
			}
			if (_spot_cache_dirty || (smSpot.spot_flag_in_use && smSpot.flag == null) || (smSpot.spotter_flag_in_use && smSpot.flag_spotter == null))
			{
				smSpot.cached_display_text = "";
				smSpot.Size.Width = 0f;
				smSpot.Size.Height = 0f;
				smSpot.spot_flag_in_use = false;
				smSpot.spotter_flag_in_use = false;
			}
			smSpot.spot_flag_in_use = smSpot.flag != null;
			smSpot.spotter_flag_in_use = smSpot.flag_spotter != null;
			string callsignString = getCallsignString(smSpot);
			if (!string.Equals(smSpot.cached_display_text, callsignString, StringComparison.Ordinal))
			{
				smSpot.cached_display_text = callsignString;
				smSpot.Size = getSpotTagSize(callsignString, smSpot.flag);
			}
			else if (smSpot.Size.Width <= 0f || smSpot.Size.Height <= 0f)
			{
				smSpot.Size = getSpotTagSize(callsignString, smSpot.flag);
			}
			int num11 = (int)smSpot.Size.Width;
			int num12 = (int)smSpot.Size.Height;
			int num13 = num11 / 2;
			float num14 = (float)(smSpot.frequencyHZ - num7 - cWSideToneShift - num5) * num10;
			int num15;
			int rightX;
			switch (smSpot.mode)
			{
			case DSPMode.LSB:
			case DSPMode.CWL:
			case DSPMode.DIGL:
			case DSPMode.AM_LSB:
				num15 = (int)(num14 - (float)num11);
				rightX = (int)num14 + 4;
				break;
			case DSPMode.USB:
			case DSPMode.CWU:
			case DSPMode.DIGU:
			case DSPMode.AM_USB:
				num15 = (int)num14;
				rightX = (int)(num14 + (float)num11 + 4f);
				break;
			default:
				num15 = (int)(num14 - (float)num13 - 2f);
				rightX = (int)(num14 + (float)num13 + 2f);
				break;
			}
			int spotLayer = getSpotLayer(rx, num15);
			if (spotLayer > -1)
			{
				updateLayer(rx, spotLayer, rightX);
				int num16 = num6 + 20 + spotLayer * num12;
				smSpot.BoundingBoxInPixels[num].X = num15 - 1;
				smSpot.BoundingBoxInPixels[num].Y = num16 - 1;
				smSpot.BoundingBoxInPixels[num].Width = num11 + 4;
				smSpot.BoundingBoxInPixels[num].Height = num12;
				if (smSpot.Highlight[num])
				{
					SharpDX.Direct2D1.Brush dXBrushForColour = getDXBrushForColour(smSpot.colour, 255);
					drawLineDX2D(dXBrushForColour, num14, num6, num14, num16, 3f);
					drawFillElipseDX2D(dXBrushForColour, num14, num6, 6f, 6f);
				}
				else
				{
					SharpDX.Direct2D1.Brush dXBrushForColour = getDXBrushForColour(smSpot.colour, 192);
					drawLineDX2D(dXBrushForColour, num14, num6, num14, num16);
					drawFillElipseDX2D(dXBrushForColour, num14, num6, 4f, 4f);
				}
				smSpot.Visible[num] = true;
			}
			else
			{
				smSpot.Visible[num] = false;
			}
		}
		_spot_cache_dirty = false;
		SharpDX.Direct2D1.Brush dXBrushForColour2 = getDXBrushForColour(System.Drawing.Color.White, 255);
		SharpDX.Direct2D1.Brush dXBrushForColour3 = getDXBrushForColour(System.Drawing.Color.Black, 255);
		SharpDX.Direct2D1.Brush dXBrushForColour4 = getDXBrushForColour(System.Drawing.Color.Yellow, 255);
		double dElapsedFrameStart = m_dElapsedFrameStart;
		double num17 = dElapsedFrameStart - _lastRenderTime;
		_lastRenderTime = dElapsedFrameStart;
		_pulsePhase = (_pulsePhase + Math.PI * 2.0 * num17 / 1000.0) % (Math.PI * 2.0);
		_new_spot_fade = (int)((Math.Cos(_pulsePhase) + 1.0) * 0.5 * 255.0);
		SpotManager2.smSpot smSpot2 = null;
		foreach (SpotManager2.smSpot smSpot3 in frequencySortedSpots)
		{
			if (!smSpot3.Visible[num])
			{
				continue;
			}
			string callsignString = smSpot3.cached_display_text ?? getCallsignString(smSpot3);
			SharpDX.Direct2D1.Brush dXBrushForColour = getDXBrushForColour(smSpot3.colour, _spot_backpanel_alpha);
			SharpDX.Direct2D1.Brush textBrush = (smSpot3.use_text_colour ? getDXBrushForColour(smSpot3.text_colour, 255) : ((smSpot3.colour_luminance <= 128) ? dXBrushForColour2 : dXBrushForColour3));
			if (smSpot3.Highlight[num])
			{
				smSpot2 = smSpot3;
				smSpot3.previously_highlighted = true;
				smSpot3.flashing = false;
				continue;
			}
			drawFillRectangleDX2D(dXBrushForColour, smSpot3.BoundingBoxInPixels[num]);
			drawSpotTagContent(smSpot3, callsignString, textBrush, smSpot3.BoundingBoxInPixels[num]);
			if (_flashNewTCISpots && smSpot3.flashing && !smSpot3.IsSWL && !smSpot3.previously_highlighted)
			{
				SharpDX.Direct2D1.Brush b = (_override_spot_flash_colour ? getDXBrushForColour(_spot_flash_colour, _new_spot_fade) : getDXBrushForColour(smSpot3.colour, _new_spot_fade));
				System.Drawing.Rectangle r = new System.Drawing.Rectangle(smSpot3.BoundingBoxInPixels[num].X - 2, smSpot3.BoundingBoxInPixels[num].Y - 2, smSpot3.BoundingBoxInPixels[num].Width + 4, smSpot3.BoundingBoxInPixels[num].Height + 4);
				drawRectangleDX2D(b, r, 4f);
				if ((DateTime.UtcNow - smSpot3.flash_start_time).TotalSeconds > 120.0 && _new_spot_fade < 20)
				{
					smSpot3.flashing = false;
				}
			}
		}
		if (smSpot2 != null)
		{
			_spot_highlighted = true;
			string callsignString = smSpot2.cached_display_text ?? getCallsignString(smSpot2);
			SharpDX.Direct2D1.Brush dXBrushForColour = getDXBrushForColour(smSpot2.colour, 255);
			SharpDX.Direct2D1.Brush textBrush = ((smSpot2.colour_luminance <= 128) ? dXBrushForColour2 : dXBrushForColour3);
			System.Drawing.Rectangle r2 = new System.Drawing.Rectangle(smSpot2.BoundingBoxInPixels[num].X - 2, smSpot2.BoundingBoxInPixels[num].Y - 2, smSpot2.BoundingBoxInPixels[num].Width + 4, smSpot2.BoundingBoxInPixels[num].Height + 4);
			drawFillRectangleDX2D(dXBrushForColour, r2);
			drawRectangleDX2D(dXBrushForColour4, r2, 2f);
			drawSpotTagContent(smSpot2, callsignString, textBrush, smSpot2.BoundingBoxInPixels[num]);
			string text = smSpot2.additionalText;
			if (!string.IsNullOrEmpty(smSpot2.spotter))
			{
				text = text + "\nSpotter: " + smSpot2.spotter;
			}
			if (smSpot2.heading >= 0)
			{
				text = text + "\nHeading: " + smSpot2.heading;
			}
			if (smSpot2.distance >= 0)
			{
				text = text + "\nDistance: " + smSpot2.distance;
			}
			if (!string.IsNullOrEmpty(smSpot2.continent))
			{
				text = text + "\nContinent: " + smSpot2.continent;
			}
			if (!string.IsNullOrEmpty(smSpot2.country))
			{
				text = text + "\nCountry: " + smSpot2.country;
			}
			TimeSpan timeSpan = DateTime.UtcNow - smSpot2.utc_spot_time;
			string text2;
			if (timeSpan.TotalDays > 2.0)
			{
				text2 = "Old spot (>2 days)";
			}
			else if (timeSpan.TotalDays >= 1.0)
			{
				int num18 = (int)timeSpan.TotalDays;
				text2 = num18 + " day" + ((num18 == 1) ? "" : "s");
			}
			else if (timeSpan.TotalHours >= 1.0)
			{
				int num19 = (int)timeSpan.TotalHours;
				text2 = num19 + " hour" + ((num19 == 1) ? "" : "s");
			}
			else if (timeSpan.TotalMinutes >= 1.0)
			{
				int num20 = (int)timeSpan.TotalMinutes;
				text2 = num20 + " minute" + ((num20 == 1) ? "" : "s");
			}
			else
			{
				int num21 = (int)timeSpan.TotalSeconds;
				text2 = num21 + " second" + ((num21 == 1) ? "" : "s");
			}
			text = text + "\nAge: " + text2;
			text = text.Trim();
			SizeF sizeF = measureStringDX2D(text, fontDX2d_font12);
			int num22 = r2.X + r2.Width / 2 - (int)(sizeF.Width / 2f);
			int num23 = r2.Y + r2.Height + 6;
			int num24 = (int)(sizeF.Width + 8f);
			int height = (int)(sizeF.Height + 8f);
			int num25 = num22 - 4;
			if (num25 < 0)
			{
				num25 = 0;
			}
			else if (num25 + num24 > W)
			{
				num25 = W - num24;
			}
			System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(num25, num23 - 4, num24, height);
			RoundedRectangle roundedRect = new RoundedRectangle
			{
				Rect = new SharpDX.RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height),
				RadiusX = 8f,
				RadiusY = 8f
			};
			_d2dRenderTarget.FillRoundedRectangle(roundedRect, getDXBrushForColour(System.Drawing.Color.LightGray));
			_d2dRenderTarget.DrawRoundedRectangle(roundedRect, getDXBrushForColour(System.Drawing.Color.White), 2f);
			drawStringDX2D(text, fontDX2d_font12, getDXBrushForColour(System.Drawing.Color.Black), rectangle.X + 2, rectangle.Y + 2);
		}
		else
		{
			_spot_highlighted = false;
		}
	}

	private static void clearAllDynamicBrushes()
	{
		if (!_bDX2Setup || _DX2Brushes == null)
		{
			return;
		}
		foreach (KeyValuePair<int, SharpDX.Direct2D1.Brush> dX2Brush in _DX2Brushes)
		{
			SharpDX.Direct2D1.Brush comObject = dX2Brush.Value;
			Utilities.Dispose(ref comObject);
			comObject = null;
		}
		_DX2Brushes.Clear();
	}

	private static SharpDX.Direct2D1.Brush getDXBrushForColour(System.Drawing.Color c, int replaceAlpha = -1)
	{
		if (!_bDX2Setup)
		{
			return null;
		}
		int num = ((replaceAlpha >= 0 && replaceAlpha <= 255) ? replaceAlpha : c.A);
		int key = (num << 24) | (c.R << 16) | (c.G << 8) | c.B;
		if (_DX2Brushes.TryGetValue(key, out var value))
		{
			return value;
		}
		SolidColorBrush solidColorBrush = new SolidColorBrush(color: new RawColor4((float)(int)c.R / 255f, (float)(int)c.G / 255f, (float)(int)c.B / 255f, (float)num / 255f), renderTarget: _d2dRenderTarget);
		_DX2Brushes.Add(key, solidColorBrush);
		return solidColorBrush;
	}

	public static void PurgeBuffers()
	{
		lock (_objDX2Lock)
		{
			clearBuffers(displayTargetWidth, 1);
			if (_rx2_enabled)
			{
				clearBuffers(displayTargetWidth, 2);
			}
			resetWaterfallBitmapAlignment(1);
			resetWaterfallBitmapAlignment(2);
		}
	}

	private static void letItSnow()
	{
		bool flag = false;
		if (m_dElapsedFrameStart >= _oldSnowFrame + 16.0)
		{
			_oldSnowFrame = m_dElapsedFrameStart;
			flag = true;
		}
		lock (_snowLock)
		{
			if (flag)
			{
				if (_snow.Count < 500)
				{
					SnowFlake item = new SnowFlake(Target.Width);
					_snow.Add(item);
				}
				foreach (SnowFlake item2 in _snow)
				{
					item2.Update();
				}
				_snow.RemoveAll((SnowFlake s) => s.Finished);
			}
			Ellipse ellipse = new Ellipse(new Vector2(0f, 0f), 1f, 1f);
			foreach (SnowFlake item3 in _snow)
			{
				ellipse.Point.X = item3.X;
				ellipse.Point.Y = item3.Y;
				ellipse.RadiusX = item3.Size;
				ellipse.RadiusY = item3.Size;
				_d2dRenderTarget.FillEllipse(ellipse, getDXBrushForColour(System.Drawing.Color.White, (int)item3.Alpha));
			}
		}
		plotSanta();
	}

	private static void plotSanta()
	{
		if (!_showSanta)
		{
			DateTime now = DateTime.Now;
			if (now > _whenToShowSanta)
			{
				_showSanta = now >= new DateTime(now.Year, 12, 1) && now.Date <= new DateTime(now.Year, 12, 25);
				_whenToShowSanta = now.AddSeconds(_rnd.Next(120, 600));
			}
		}
		else
		{
			if (_santaFrames.Length == 0)
			{
				return;
			}
			bool flag = false;
			if (m_dElapsedFrameStart >= _oldSantaFrame + 250.0)
			{
				_oldSantaFrame = m_dElapsedFrameStart;
				flag = true;
			}
			SharpDX.Direct2D1.Bitmap bitmap = _santaFrames[_santaFrameIndex];
			float y = (float)displayTargetHeight - bitmap.Size.Height / 2f;
			SharpDX.RectangleF rectDest = new SharpDX.RectangleF(_santaX, y, bitmap.Size.Width / 2f, bitmap.Size.Height / 2f);
			_d2dRenderTarget.DrawBitmap(bitmap, rectDest, 1f, BitmapInterpolationMode.Linear);
			if (m_dElapsedFrameStart >= _oldSantaXFrame + 16.0)
			{
				if (_santaFrameIndex <= 2)
				{
					lock (_snowLock)
					{
						float x = rectDest.X + rectDest.Width / 2f;
						_snow.Where((SnowFlake snowflake) => snowflake.Settled && snowflake.X >= x && snowflake.X <= rectDest.X + rectDest.Width).ToList().ForEach(delegate(SnowFlake snowflake)
						{
							snowflake.Alpha = 0f;
						});
					}
				}
				_oldSantaXFrame = m_dElapsedFrameStart;
				_santaX += 0.5f;
				if (_santaX > (float)displayTargetWidth)
				{
					_santaX = -50f;
					_showSanta = false;
				}
			}
			if (flag)
			{
				_santaFrameIndex++;
				if (_santaFrameIndex >= _santaFrames.Length)
				{
					_santaFrameIndex = 0;
				}
			}
		}
	}

	public static void SetSantaGif(System.Drawing.Image image)
	{
		lock (_objDX2Lock)
		{
			if (!_bDX2Setup || image == null || image.RawFormat == null || image.RawFormat.Guid != ImageFormat.Gif.Guid || image.FrameDimensionsList == null || image.FrameDimensionsList.Length == 0)
			{
				return;
			}
			_santaFrameIndex = 0;
			_santaX = -50f;
			_showSanta = false;
			_whenToShowSanta = DateTime.Now.AddSeconds(_rnd.Next(120, 600));
			FrameDimension dimension = new FrameDimension(image.FrameDimensionsList[0]);
			int frameCount = image.GetFrameCount(dimension);
			santaCleanUp();
			_santaFrames = new SharpDX.Direct2D1.Bitmap[frameCount];
			for (int i = 0; i < frameCount; i++)
			{
				image.SelectActiveFrame(dimension, i);
				using System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image);
				_santaFrames[i] = SDXBitmapFromSysBitmap(_d2dRenderTarget, bitmap);
			}
		}
	}

	private static void santaCleanUp()
	{
		if (_santaFrames == null)
		{
			return;
		}
		for (int i = 0; i < _santaFrames.Length; i++)
		{
			if (_santaFrames[i] != null)
			{
				Utilities.Dispose(ref _santaFrames[i]);
				_santaFrames[i] = null;
			}
		}
		_santaFrames = null;
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

	[DllImport("user32.dll")]
	private static extern uint GetDpiForWindowLocal(IntPtr hWnd);

	public static int GetCurrentMonitorRefreshRate(Form form)
	{
		Screen screen = Screen.FromControl(form);
		DEVMODE devMode = new DEVMODE
		{
			dmDeviceName = new string(new char[32])
		};
		if (EnumDisplaySettings(screen.DeviceName, -1, ref devMode))
		{
			return (int)devMode.dmDisplayFrequency;
		}
		return 60;
	}

	private static SizeF getSpotTagSize(string displayText, System.Drawing.Image flagImage)
	{
		SizeF sizeF = measureStringDX2D(displayText, fontDX2d_font9);
		getSpotFlagRenderSize(flagImage, (int)sizeF.Height, out var width, out var height);
		int num = (int)sizeF.Width + 4;
		if (width > 0)
		{
			num += width + 3;
		}
		int num2 = Math.Max((int)sizeF.Height, height) + 2;
		return new SizeF(num, num2);
	}

	private static void drawSpotTagContent(SpotManager2.smSpot spot, string displayText, SharpDX.Direct2D1.Brush textBrush, System.Drawing.Rectangle bounds)
	{
		int num = bounds.X + 1;
		if (_spot_flags)
		{
			System.Drawing.Image flagImage = (_bShiftKeyDown ? spot.flag_spotter : spot.flag);
			getSpotFlagRenderSize(flagImage, bounds.Height - 2, out var width, out var height);
			if (width > 0)
			{
				SharpDX.Direct2D1.Bitmap spotFlagBitmap = getSpotFlagBitmap(flagImage);
				if (spotFlagBitmap != null)
				{
					SharpDX.RectangleF rectangleF = new SharpDX.RectangleF(bounds.X + 1, (float)bounds.Y + (float)(bounds.Height - height) / 2f, width, height);
					_d2dRenderTarget.DrawBitmap(spotFlagBitmap, rectangleF, 1f, BitmapInterpolationMode.Linear);
				}
				num += width + 3;
			}
		}
		drawStringDX2D(displayText, fontDX2d_font9, textBrush, num, bounds.Y + 1);
	}

	private static SharpDX.Direct2D1.Bitmap getSpotFlagBitmap(System.Drawing.Image flagImage)
	{
		if (flagImage == null || !_bDX2Setup || _d2dRenderTarget == null)
		{
			return null;
		}
		lock (_spotFlagBitmapCache)
		{
			if (_spotFlagBitmapCache.TryGetValue(flagImage, out var value))
			{
				return value;
			}
		}
		try
		{
			using System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(flagImage);
			SharpDX.Direct2D1.Bitmap comObject = SDXBitmapFromSysBitmap(_d2dRenderTarget, bitmap);
			if (comObject == null)
			{
				return null;
			}
			lock (_spotFlagBitmapCache)
			{
				if (_spotFlagBitmapCache.TryGetValue(flagImage, out var value2))
				{
					Utilities.Dispose(ref comObject);
					return value2;
				}
				_spotFlagBitmapCache[flagImage] = comObject;
				return comObject;
			}
		}
		catch
		{
			return null;
		}
	}

	private static void getSpotFlagRenderSize(System.Drawing.Image flagImage, int targetHeight, out int width, out int height)
	{
		width = 0;
		height = 0;
		if (_spot_flags && flagImage != null && flagImage.Width > 0 && flagImage.Height > 0 && targetHeight > 0)
		{
			height = Math.Max(1, targetHeight);
			width = Math.Max(1, (int)Math.Round((double)flagImage.Width * ((double)height / (double)flagImage.Height)));
		}
	}

	private static void clearSpotFlagBitmapCache()
	{
		lock (_spotFlagBitmapCache)
		{
			foreach (SharpDX.Direct2D1.Bitmap value in _spotFlagBitmapCache.Values)
			{
				SharpDX.Direct2D1.Bitmap comObject = value;
				Utilities.Dispose(ref comObject);
			}
			_spotFlagBitmapCache.Clear();
		}
	}
}
