namespace Thetis;

public class Channel60m
{
	private double freq;

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

	public Channel60m(double f)
	{
		freq = f;
	}
}
