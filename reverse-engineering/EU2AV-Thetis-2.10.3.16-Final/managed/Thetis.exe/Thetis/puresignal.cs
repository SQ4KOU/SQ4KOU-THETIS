using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Thetis;

internal static class puresignal
{
	public enum EngineState
	{
		LRESET,
		LWAIT,
		LMOXDELAY,
		LSETUP,
		LCOLLECT,
		MOXCHECK,
		LCALC,
		LDELAY,
		LSTAYON,
		LTURNON
	}

	private static bool _psHardResetAvailable;

	private static int[] _info;

	private static int[] _oldInfo;

	private static bool _bInvertRedBlue;

	private static int _targetFeedbackLevel;

	public static int TargetFeedbackLevel
	{
		get
		{
			return _targetFeedbackLevel;
		}
		set
		{
			if (value < 1)
			{
				value = 1;
			}
			if (value > 256)
			{
				value = 256;
			}
			_targetFeedbackLevel = value;
		}
	}

	public static int FeedbackLevelGreenLow => (int)((double)_targetFeedbackLevel * 0.7);

	public static int FeedbackLevelGreenHigh => (int)((double)_targetFeedbackLevel * 1.3);

	public static int[] Info => _info;

	public static bool HasInfoChanged
	{
		get
		{
			for (int i = 0; i < 16; i++)
			{
				if (_info[i] != _oldInfo[i])
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool CalibrationAttemptsChanged => _info[5] != _oldInfo[5];

	public static bool CorrectionsBeingApplied => _info[14] == 1;

	public static int CalibrationCount => _info[5];

	public static bool Correcting => FeedbackLevel > 90;

	public static bool IsFeedbackLevelOK => FeedbackLevel <= 256;

	public static bool IsFeedbackLevelOKRange
	{
		get
		{
			if (FeedbackLevel > 128)
			{
				return FeedbackLevel <= 181;
			}
			return false;
		}
	}

	public static int FeedbackLevel => _info[4];

	public static Color FeedbackColourLevel
	{
		get
		{
			if (FeedbackLevel > 181)
			{
				if (_bInvertRedBlue)
				{
					return Color.Red;
				}
				return Color.DodgerBlue;
			}
			if (FeedbackLevel > 128)
			{
				return Color.Lime;
			}
			if (FeedbackLevel > 90)
			{
				return Color.Yellow;
			}
			if (_bInvertRedBlue)
			{
				return Color.DodgerBlue;
			}
			return Color.Red;
		}
	}

	public static EngineState State => (EngineState)_info[15];

	public static bool InvertRedBlue
	{
		get
		{
			return _bInvertRedBlue;
		}
		set
		{
			_bInvertRedBlue = value;
		}
	}

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSRunCal(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSMox(int channel, bool mox);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetPSInfo(int channel, int* info);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSReset(int channel, int reset);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSMancal(int channel, int mancal);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSAutomode(int channel, int automode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSTurnon(int channel, int turnon);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSControl(int channel, int reset, int mancal, int automode, int turnon);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSContext(int channel, int band, double freq_mhz, int att);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int PSGetCachedATT(int channel, int band, double freq_mhz);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int PSCacheSaveFileATT(int channel, [MarshalAs(UnmanagedType.LPStr)] string path, int att);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int PSCacheLoadFile(int channel, [MarshalAs(UnmanagedType.LPStr)] string path);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSDCBHw24(int channel, int enable);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void PSHardReset(int channel);

	public static bool TryPSHardReset(int channel)
	{
		if (!_psHardResetAvailable)
		{
			return false;
		}
		try
		{
			PSHardReset(channel);
			return true;
		}
		catch (EntryPointNotFoundException)
		{
			_psHardResetAvailable = false;
			return false;
		}
		catch
		{
			return false;
		}
	}

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSLoopDelay(int channel, double delay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSMoxDelay(int channel, double delay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double SetPSTXDelay(int channel, double delay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void PSSaveCorr(int channel, string filename);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void PSRestoreCorr(int channel, string filename);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSHWPeak(int channel, double peak);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetPSHWPeak(int channel, double* peak);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetPSMaxTX(int channel, double* maxtx);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void GetPSDisp(int channel, IntPtr x, IntPtr ym, IntPtr yc, IntPtr ys, IntPtr xm_cor, IntPtr ym_cor, IntPtr xa_cor, IntPtr ya_cor, IntPtr nsamps_out, IntPtr cpts_out, IntPtr phs_ref_deg_out);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void GetPSCurveVals(int channel, int bin, out double m, out double c, out double s);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double GetPSXYLag(int channel);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetPSCollectProgress(int channel);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSFeedbackRate(int channel, int rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSStabilize(int channel, int stbl);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSEMAAlpha(int channel, double alpha);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSPinAlpha(int channel, double alpha);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSPinMode(int channel, int pin);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSIntsAndSpi(int channel, int ints, int spi);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSDCBEnable(int channel, int enable);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSDCBCap(int channel, double cap);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSEQEnable(int channel, int enable);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSOutlierSigma(int channel, double sigma);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void ResetPSAdvancedParams(int channel);

	static puresignal()
	{
		_psHardResetAvailable = true;
		_info = new int[16];
		_oldInfo = new int[16];
		_bInvertRedBlue = false;
		_targetFeedbackLevel = 152;
		for (int i = 0; i < 16; i++)
		{
			_info[i] = 0;
			_oldInfo[i] = _info[i];
		}
	}

	public unsafe static void GetInfo(int txachannel)
	{
		fixed (int* ptr = &_oldInfo[0])
		{
			void* destptr = ptr;
			fixed (int* ptr2 = &_info[0])
			{
				void* srcptr = ptr2;
				Win32.memcpy(destptr, srcptr, 64);
			}
		}
		fixed (int* info = &_info[0])
		{
			GetPSInfo(txachannel, info);
		}
	}

	public static bool NeedToRecalibrate(int nCurrentATTonTX)
	{
		int num = _targetFeedbackLevel * 181 / 152;
		int num2 = _targetFeedbackLevel * 128 / 152;
		if (FeedbackLevel <= num)
		{
			if (FeedbackLevel <= num2)
			{
				return nCurrentATTonTX > 0;
			}
			return false;
		}
		return true;
	}
}
