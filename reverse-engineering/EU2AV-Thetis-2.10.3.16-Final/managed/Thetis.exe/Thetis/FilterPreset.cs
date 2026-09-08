namespace Thetis;

public class FilterPreset
{
	private int[] low;

	private int[] high;

	private string[] name;

	private Filter last_filter;

	public Filter LastFilter
	{
		get
		{
			return last_filter;
		}
		set
		{
			last_filter = value;
		}
	}

	public FilterPreset()
	{
		low = new int[13];
		high = new int[13];
		name = new string[13];
	}

	public void SetLow(Filter f, int val)
	{
		low[(int)f] = val;
	}

	public void SetHigh(Filter f, int val)
	{
		high[(int)f] = val;
	}

	public void SetName(Filter f, string n)
	{
		name[(int)f] = n;
	}

	public void SetFilter(Filter f, int l, int h, string n)
	{
		low[(int)f] = l;
		high[(int)f] = h;
		name[(int)f] = n;
	}

	public int GetLow(Filter f)
	{
		return low[(int)f];
	}

	public int GetHigh(Filter f)
	{
		return high[(int)f];
	}

	public int GetBW(Filter f)
	{
		return high[(int)f] - low[(int)f];
	}

	public string GetName(Filter f)
	{
		return name[(int)f];
	}

	public string ToString(Filter f)
	{
		return name[(int)f] + ": " + low[(int)f] + "| " + high[(int)f];
	}
}
