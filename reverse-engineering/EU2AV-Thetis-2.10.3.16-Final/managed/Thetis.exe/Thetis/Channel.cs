using System;

namespace Thetis;

public class Channel : IComparable
{
	private double freq;

	private int bw;

	public double Freq
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

	public int BW
	{
		get
		{
			return bw;
		}
		set
		{
			bw = value;
		}
	}

	public double Low
	{
		get
		{
			return freq - (double)bw * 1E-06 / 2.0;
		}
		set
		{
		}
	}

	public double High
	{
		get
		{
			return freq + (double)bw * 1E-06 / 2.0;
		}
		set
		{
		}
	}

	public Channel(double f, int bandwidth)
	{
		freq = f;
		bw = bandwidth;
	}

	public Channel(double f, int bandwidth, bool perm, int dep)
	{
		freq = f;
		bw = bandwidth;
	}

	public override string ToString()
	{
		return freq.ToString("R") + " MHz| " + bw + " Hz";
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		return freq.CompareTo(((Channel)obj).freq);
	}

	public Channel Copy()
	{
		return new Channel(freq, bw);
	}

	public bool InBW(double low, double high)
	{
		double num = Freq - (double)BW / 2.0 * 1E-06;
		double num2 = Freq + (double)BW / 2.0 * 1E-06;
		if ((!(low > num) || !(low < num2)) && (!(high > num) || !(high < num2)))
		{
			if (num > low)
			{
				return num2 < high;
			}
			return false;
		}
		return true;
	}
}
