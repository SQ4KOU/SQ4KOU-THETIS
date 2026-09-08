using System;
using System.ComponentModel;

namespace Thetis;

public class DXMemRecord : IComparable, INotifyPropertyChanged
{
	private string dxurl = "k1rfi.com:7300";

	public string DXURL
	{
		get
		{
			return dxurl;
		}
		set
		{
			dxurl = value;
			OnPropertyChanged(this, new PropertyChangedEventArgs("DXURL"));
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public DXMemRecord()
	{
	}

	public DXMemRecord(string _dxurl)
	{
		dxurl = _dxurl;
	}

	public DXMemRecord(DXMemRecord rec)
	{
		dxurl = rec.dxurl;
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
		DXMemRecord dXMemRecord = (DXMemRecord)obj;
		_ = DXURL != dXMemRecord.DXURL;
		return DXURL.CompareTo(dXMemRecord.DXURL);
	}
}
