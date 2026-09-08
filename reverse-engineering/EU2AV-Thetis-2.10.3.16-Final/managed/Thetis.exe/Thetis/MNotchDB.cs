using System;
using System.Collections.Generic;

namespace Thetis;

internal class MNotchDB
{
	private static List<MNotch> _lstNotches = new List<MNotch>();

	private static object _listLock = new object();

	public static int Count
	{
		get
		{
			lock (_listLock)
			{
				return _lstNotches.Count;
			}
		}
	}

	public static void Clear()
	{
		lock (_listLock)
		{
			_lstNotches.Clear();
		}
	}

	public static int IndexOf(MNotch mNotch)
	{
		lock (_listLock)
		{
			return _lstNotches.IndexOf(mNotch);
		}
	}

	public static void Add(MNotch mNotch)
	{
		lock (_listLock)
		{
			_lstNotches.Add(mNotch);
		}
	}

	public static MNotch NotchFromIndex(int index)
	{
		lock (_listLock)
		{
			return _lstNotches[index];
		}
	}

	public static MNotch GetFirstNotchThatMatches(double freqHz, double fwidth, bool bActive)
	{
		lock (_listLock)
		{
			foreach (MNotch lstNotch in _lstNotches)
			{
				if (lstNotch.FCenter == freqHz && lstNotch.FWidth == fwidth && lstNotch.Active == bActive)
				{
					return lstNotch;
				}
			}
			return null;
		}
	}

	public static bool NotchNearFreq(double freqHz, int deltaHz)
	{
		lock (_listLock)
		{
			foreach (MNotch lstNotch in _lstNotches)
			{
				if (Math.Abs(freqHz - lstNotch.FCenter) < (double)deltaHz)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<MNotch> NotchesInBW(double centreBWFreqHz, int lowHz, int highHz)
	{
		lock (_listLock)
		{
			List<MNotch> list = new List<MNotch>();
			double num = centreBWFreqHz + (double)lowHz;
			double num2 = centreBWFreqHz + (double)highHz;
			foreach (MNotch lstNotch in _lstNotches)
			{
				if (lstNotch.FCenter + lstNotch.FWidth / 2.0 >= num && lstNotch.FCenter - lstNotch.FWidth / 2.0 <= num2)
				{
					list.Add(lstNotch);
				}
			}
			return list;
		}
	}

	public static MNotch NotchThatSurroundsFrequencyInBW(double centreBWFreqHz, int lowHz, int highHz, double freqHz, int nPadWidth = 0)
	{
		List<MNotch> list = NotchesInBW(centreBWFreqHz, lowHz, highHz);
		lock (_listLock)
		{
			if (list.Count > 0)
			{
				foreach (MNotch item in list)
				{
					double num = item.FCenter - item.FWidth / 2.0;
					double num2 = item.FCenter + item.FWidth / 2.0;
					if (item.FWidth < (double)(nPadWidth * 2))
					{
						num -= (double)nPadWidth;
						num2 += (double)nPadWidth;
					}
					if (freqHz >= num && freqHz <= num2)
					{
						return item;
					}
				}
			}
			return null;
		}
	}
}
