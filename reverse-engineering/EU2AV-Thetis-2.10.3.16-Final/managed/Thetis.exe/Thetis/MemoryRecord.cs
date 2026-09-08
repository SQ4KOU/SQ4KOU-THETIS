using System;
using System.ComponentModel;

namespace Thetis;

public class MemoryRecord : IComparable, INotifyPropertyChanged
{
	private string group = "";

	private double rx_freq = 10.0;

	private string name = "";

	private DSPMode dsp_mode;

	private DateTime startdate = DateTime.Now;

	private int duration = 25;

	private bool recording;

	private bool repeating;

	private bool repeatingm;

	private string comments = "";

	private bool scan = true;

	private string tune_step = "10Hz";

	private FMTXMode repeater_mode;

	private double rptr_offset = 0.1;

	private bool ctcss_on;

	private double ctcss_freq;

	private int deviation = 5000;

	private int power;

	private bool split;

	private double tx_freq = 10.0;

	private Filter rx_filter;

	private int rx_filter_low;

	private int rx_filter_high;

	private AGCMode agc_mode = AGCMode.MED;

	private int agct = 80;

	private bool scheduleon;

	private int extra;

	public string Group
	{
		get
		{
			return group;
		}
		set
		{
			group = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Group"));
		}
	}

	public double RXFreq
	{
		get
		{
			return rx_freq;
		}
		set
		{
			rx_freq = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("RXFreq"));
		}
	}

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Name"));
		}
	}

	public DSPMode DSPMode
	{
		get
		{
			return dsp_mode;
		}
		set
		{
			dsp_mode = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("DSPMode"));
		}
	}

	public DateTime StartDate
	{
		get
		{
			return startdate;
		}
		set
		{
			startdate = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("StartDate"));
		}
	}

	public int Duration
	{
		get
		{
			return duration;
		}
		set
		{
			duration = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Duration"));
		}
	}

	public bool Recording
	{
		get
		{
			return recording;
		}
		set
		{
			recording = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Recording"));
		}
	}

	public bool Repeating
	{
		get
		{
			return repeating;
		}
		set
		{
			repeating = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Repeating"));
		}
	}

	public bool Repeatingm
	{
		get
		{
			return repeatingm;
		}
		set
		{
			repeatingm = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Repeatingm"));
		}
	}

	public string Comments
	{
		get
		{
			return comments;
		}
		set
		{
			comments = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Comments"));
		}
	}

	public bool Scan
	{
		get
		{
			return scan;
		}
		set
		{
			scan = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Scan"));
		}
	}

	public string TuneStep
	{
		get
		{
			return tune_step;
		}
		set
		{
			tune_step = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("TuneStep"));
		}
	}

	public FMTXMode RPTR
	{
		get
		{
			return repeater_mode;
		}
		set
		{
			repeater_mode = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("RPT"));
		}
	}

	public double RPTROffset
	{
		get
		{
			return rptr_offset;
		}
		set
		{
			rptr_offset = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("RPTOffset"));
		}
	}

	public bool CTCSSOn
	{
		get
		{
			return ctcss_on;
		}
		set
		{
			ctcss_on = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("CTCSSOn"));
		}
	}

	public double CTCSSFreq
	{
		get
		{
			return ctcss_freq;
		}
		set
		{
			ctcss_freq = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("CTCSSFreq"));
		}
	}

	public int Deviation
	{
		get
		{
			return deviation;
		}
		set
		{
			deviation = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Deviation"));
		}
	}

	public int Power
	{
		get
		{
			return power;
		}
		set
		{
			power = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Power"));
		}
	}

	public bool Split
	{
		get
		{
			return split;
		}
		set
		{
			split = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Split"));
		}
	}

	public double TXFreq
	{
		get
		{
			return tx_freq;
		}
		set
		{
			tx_freq = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("TXFreq"));
		}
	}

	public Filter RXFilter
	{
		get
		{
			return rx_filter;
		}
		set
		{
			rx_filter = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Filter"));
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
			OnPropertyChanged(this, new PropertyChangedEventArgs("FilterLow"));
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
			OnPropertyChanged(this, new PropertyChangedEventArgs("FilterHigh"));
		}
	}

	public AGCMode AGCMode
	{
		get
		{
			return agc_mode;
		}
		set
		{
			agc_mode = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("AGCMode"));
		}
	}

	public int AGCT
	{
		get
		{
			return agct;
		}
		set
		{
			agct = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("AGCT"));
		}
	}

	public bool ScheduleOn
	{
		get
		{
			return scheduleon;
		}
		set
		{
			scheduleon = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("ScheduleOn"));
		}
	}

	public int Extra
	{
		get
		{
			return extra;
		}
		set
		{
			extra = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("Extra"));
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public MemoryRecord()
	{
	}

	public MemoryRecord(string _group, double _rxfreq, string _name, DSPMode _dsp_mode, bool _scan, string _tune_step, FMTXMode _repeater_mode, double _fm_tx_offset_mhz, bool _ctcss_on, double _ctcss_freq, int _power, int _deviation, bool _split, double _txfreq, Filter _filter, int _filterlow, int _filterhigh, string _comments, AGCMode _agc_mode, int _agc_thresh, DateTime _StartDate, bool _ScheduleOn, int _Duration, bool _Repeating, bool _Recording, bool _Repeatingm, int _Extra)
	{
		group = _group;
		rx_freq = _rxfreq;
		name = _name;
		dsp_mode = _dsp_mode;
		scan = _scan;
		tune_step = _tune_step;
		repeater_mode = _repeater_mode;
		rptr_offset = _fm_tx_offset_mhz;
		ctcss_on = _ctcss_on;
		ctcss_freq = _ctcss_freq;
		power = _power;
		deviation = _deviation;
		split = _split;
		tx_freq = _txfreq;
		rx_filter = _filter;
		rx_filter_low = _filterlow;
		rx_filter_high = _filterhigh;
		comments = _comments;
		agc_mode = _agc_mode;
		agct = _agc_thresh;
		startdate = _StartDate;
		scheduleon = _ScheduleOn;
		duration = _Duration;
		repeating = _Repeating;
		recording = _Recording;
		repeatingm = _Repeatingm;
		extra = _Extra;
	}

	public MemoryRecord(MemoryRecord rec)
	{
		group = rec.group;
		rx_freq = rec.rx_freq;
		name = rec.name;
		dsp_mode = rec.dsp_mode;
		scan = rec.scan;
		tune_step = rec.tune_step;
		repeater_mode = rec.repeater_mode;
		rptr_offset = rec.rptr_offset;
		ctcss_on = rec.ctcss_on;
		ctcss_freq = rec.ctcss_freq;
		power = rec.power;
		deviation = rec.deviation;
		split = rec.split;
		tx_freq = rec.tx_freq;
		rx_filter = rec.rx_filter;
		rx_filter_low = rec.rx_filter_low;
		rx_filter_high = rec.rx_filter_high;
		comments = rec.comments;
		agc_mode = rec.agc_mode;
		agct = rec.agct;
		startdate = rec.startdate;
		scheduleon = rec.scheduleon;
		duration = rec.duration;
		repeating = rec.repeating;
		recording = rec.recording;
		repeatingm = rec.repeatingm;
		extra = rec.extra;
	}

	private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (PropertyChanged != null)
		{
			PropertyChanged(sender, e);
		}
	}

	public int CompareTo(object obj)
	{
		MemoryRecord memoryRecord = (MemoryRecord)obj;
		if (Group != memoryRecord.Group)
		{
			return Group.CompareTo(memoryRecord.Group);
		}
		if (RXFreq != memoryRecord.RXFreq)
		{
			return RXFreq.CompareTo(memoryRecord.RXFreq);
		}
		return Name.CompareTo(memoryRecord.Name);
	}
}
