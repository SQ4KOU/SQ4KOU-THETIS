using System;

namespace Thetis;

internal static class SharedWaterfallState
{
	public struct PerRxSnapshot
	{
		public bool Valid;

		public float LowThreshold;

		public float HighThreshold;

		public float GpuCalOffset;

		public float RXDisplayOffsetDb;

		public ColorScheme Scheme;

		public int PaletteVersion;

		public float Gamma;

		public int EffectiveToneMap;

		public float EffectiveTemporalAlpha;

		public float[] PanadapterRow;

		public int PanadapterWidth;

		public float[] WaterfallRow;

		public int WaterfallRowWidth;

		public long PublishTickMs;

		public float[] NarrowRow;

		public int NarrowRowWidth;

		public float RXDisplayLowHz;

		public float RXDisplayHighHz;
	}

	private static readonly PerRxSnapshot[] _rx = new PerRxSnapshot[2];

	private static int _paletteVersion = 0;

	private static bool _detachedActive = false;

	private static bool _produceForDetachedFresh = false;

	private static readonly float[] _fdSpanLow = new float[2];

	private static readonly float[] _fdSpanHigh = new float[2];

	private static readonly bool[] _fdSpanActive = new bool[2];

	public static PerRxSnapshot RX1 => _rx[0];

	public static PerRxSnapshot RX2 => _rx[1];

	public static ref PerRxSnapshot RX1Ref => ref _rx[0];

	public static ref PerRxSnapshot RX2Ref => ref _rx[1];

	public static bool DetachedActive
	{
		get
		{
			return _detachedActive;
		}
		internal set
		{
			_detachedActive = value;
		}
	}

	public static bool ProduceForDetachedFresh
	{
		get
		{
			return _produceForDetachedFresh;
		}
		internal set
		{
			_produceForDetachedFresh = value;
		}
	}

	public static int PaletteVersion => _paletteVersion;

	public static void BumpPaletteVersion()
	{
		_paletteVersion++;
	}

	public static void SetFilterDisplaySpan(int rx, float low, float high)
	{
		int num = rx - 1;
		if ((uint)num <= 1u)
		{
			_fdSpanLow[num] = low;
			_fdSpanHigh[num] = high;
			_fdSpanActive[num] = high > low;
		}
	}

	public static float FilterDisplaySpanLow(int rx)
	{
		return _fdSpanLow[rx - 1];
	}

	public static float FilterDisplaySpanHigh(int rx)
	{
		return _fdSpanHigh[rx - 1];
	}

	public static bool FilterDisplaySpanActive(int rx)
	{
		return _fdSpanActive[rx - 1];
	}

	public static void PublishRX(int rx, float lowThreshold, float highThreshold, float gpuCalOffset, float rxDisplayOffsetDb, ColorScheme scheme, int paletteVersion, float gamma, int effectiveToneMap, float effectiveTemporalAlpha, float[] panadapterRow, int panadapterWidth, float rxDisplayLowHz, float rxDisplayHighHz, float[] waterfallRow, int waterfallRowWidth, float[] narrowRow, int narrowRowWidth)
	{
		int num = rx - 1;
		if ((uint)num <= 1u)
		{
			_rx[num].Valid = true;
			_rx[num].LowThreshold = lowThreshold;
			_rx[num].HighThreshold = highThreshold;
			_rx[num].GpuCalOffset = gpuCalOffset;
			_rx[num].RXDisplayOffsetDb = rxDisplayOffsetDb;
			_rx[num].Scheme = scheme;
			_rx[num].PaletteVersion = paletteVersion;
			_rx[num].Gamma = gamma;
			_rx[num].EffectiveToneMap = effectiveToneMap;
			_rx[num].EffectiveTemporalAlpha = effectiveTemporalAlpha;
			_rx[num].PanadapterRow = panadapterRow;
			_rx[num].PanadapterWidth = panadapterWidth;
			_rx[num].RXDisplayLowHz = rxDisplayLowHz;
			_rx[num].RXDisplayHighHz = rxDisplayHighHz;
			_rx[num].WaterfallRow = waterfallRow;
			_rx[num].WaterfallRowWidth = waterfallRowWidth;
			_rx[num].NarrowRow = narrowRow;
			_rx[num].NarrowRowWidth = narrowRowWidth;
			_rx[num].PublishTickMs = Environment.TickCount;
		}
	}
}
