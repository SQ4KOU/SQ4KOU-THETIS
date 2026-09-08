using System;

namespace Thetis;

public class BandStackEntry : IComparable
{
	private double frequency;

	private double centreFrequency;

	private Band band;

	private bool cTUNEnabled;

	private DSPMode mode;

	private DSPSubMode subMode;

	private Filter filter;

	private double zoomFactor;

	private int zoomSlider;

	private bool locked;

	private string description;

	private int lowFilter;

	private int highFilter;

	public string GUID;

	public double Frequency
	{
		get
		{
			return frequency;
		}
		set
		{
			frequency = value;
		}
	}

	public double CentreFrequency
	{
		get
		{
			return centreFrequency;
		}
		set
		{
			centreFrequency = value;
		}
	}

	public Band Band
	{
		get
		{
			return band;
		}
		set
		{
			band = value;
		}
	}

	public bool CTUNEnabled
	{
		get
		{
			return cTUNEnabled;
		}
		set
		{
			cTUNEnabled = value;
		}
	}

	public DSPMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
		}
	}

	public DSPSubMode SubMode
	{
		get
		{
			return subMode;
		}
		set
		{
			subMode = value;
		}
	}

	public Filter Filter
	{
		get
		{
			return filter;
		}
		set
		{
			filter = value;
		}
	}

	public double ZoomFactor
	{
		get
		{
			return zoomFactor;
		}
		set
		{
			zoomFactor = value;
		}
	}

	public int ZoomSlider
	{
		get
		{
			return zoomSlider;
		}
		set
		{
			zoomSlider = value;
		}
	}

	public bool Locked
	{
		get
		{
			return locked;
		}
		set
		{
			locked = value;
		}
	}

	public string Description
	{
		get
		{
			return description;
		}
		set
		{
			description = value;
		}
	}

	public int LowFilter
	{
		get
		{
			return lowFilter;
		}
		set
		{
			lowFilter = value;
		}
	}

	public int HighFilter
	{
		get
		{
			return highFilter;
		}
		set
		{
			highFilter = value;
		}
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (obj.GetType() != typeof(BandStackEntry))
		{
			return 1;
		}
		int result = 1;
		BandStackEntry bandStackEntry = obj as BandStackEntry;
		if (Frequency == bandStackEntry.Frequency && Band == bandStackEntry.Band)
		{
			result = 0;
		}
		return result;
	}

	public BandStackEntry()
	{
		GUID = Guid.NewGuid().ToString();
	}

	public BandStackEntry Copy(bool bNewGUID = false)
	{
		BandStackEntry bandStackEntry = new BandStackEntry
		{
			Frequency = Frequency,
			CentreFrequency = CentreFrequency,
			Band = Band,
			CTUNEnabled = CTUNEnabled,
			Mode = Mode,
			SubMode = SubMode,
			Filter = Filter,
			ZoomFactor = ZoomFactor,
			ZoomSlider = ZoomSlider,
			Locked = Locked,
			Description = Description,
			GUID = GUID
		};
		if (bNewGUID)
		{
			bandStackEntry.GUID = Guid.NewGuid().ToString();
		}
		return bandStackEntry;
	}
}
