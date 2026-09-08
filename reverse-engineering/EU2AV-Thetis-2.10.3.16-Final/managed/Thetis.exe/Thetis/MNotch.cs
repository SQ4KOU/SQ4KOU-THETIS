using System;

namespace Thetis;

public class MNotch : IComparable
{
	private double fcenter;

	private double fwidth;

	private bool active;

	public double FCenter
	{
		get
		{
			return fcenter;
		}
		set
		{
			fcenter = value;
		}
	}

	public double FWidth
	{
		get
		{
			return fwidth;
		}
		set
		{
			fwidth = value;
		}
	}

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			active = value;
		}
	}

	public MNotch(double freq, double width, bool act)
	{
		fcenter = freq;
		fwidth = width;
		active = act;
	}

	public static MNotch Parse(string s)
	{
		int num = s.IndexOf("MHz");
		double freq = double.Parse(s.Substring(0, num));
		int num2 = s.LastIndexOf("Hz");
		double width = double.Parse(s.Substring(num + 5, num2 - (num + 5)));
		num = s.IndexOf("active:");
		bool act = bool.Parse(s.Substring(num + 7));
		return new MNotch(freq, width, act);
	}

	public override string ToString()
	{
		return fcenter.ToString("R") + " MHz| " + fwidth + " Hz| active: " + active;
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		return fcenter.CompareTo(((MNotch)obj).fcenter);
	}
}
