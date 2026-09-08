using System;
using System.Collections.Generic;
using System.Linq;

namespace Thetis;

public class BandStackFilter
{
	public enum FilterReturnMode
	{
		FIRST,
		LastVisited,
		Specific,
		Current
	}

	public string GUID = Guid.NewGuid().ToString();

	public string FilterName;

	public string FilterDescription;

	public readonly List<BandFrequencyData> FrequenciesToFilterOn = new List<BandFrequencyData>();

	public readonly List<DSPMode> ModesToFilterOn = new List<DSPMode>();

	public readonly List<DSPSubMode> SubModesToFilterOn = new List<DSPSubMode>();

	public readonly List<Band> BandsToFilterOn = new List<Band>();

	public bool FilterOnModes;

	public bool FilterOnSubModes;

	public bool FilterOnFrequencies;

	public bool FilterOnBands;

	public bool UserDefined;

	private FilterReturnMode m_filterReturnMode;

	private string m_sSpecificReturnGUID;

	private int m_nCurrentlySelectedIndex;

	private string m_sCurrentlySelectedGUID;

	private List<BandStackEntry> m_lstFilteredList;

	private BandStackEntry m_lastVisited;

	public string GuidOfCurrentBlind
	{
		get
		{
			return m_sCurrentlySelectedGUID;
		}
		set
		{
			m_sCurrentlySelectedGUID = value;
		}
	}

	public FilterReturnMode ReturnMode
	{
		get
		{
			return m_filterReturnMode;
		}
		set
		{
			m_filterReturnMode = value;
		}
	}

	public string ReturnGUID
	{
		get
		{
			return m_sSpecificReturnGUID;
		}
		set
		{
			m_sSpecificReturnGUID = value;
		}
	}

	public int NumberOfEntries => m_lstFilteredList.Count;

	public List<BandStackEntry> Entries
	{
		get
		{
			List<BandStackEntry> list = new List<BandStackEntry>();
			foreach (BandStackEntry lstFiltered in m_lstFilteredList)
			{
				list.Add(lstFiltered.Copy());
			}
			return list;
		}
	}

	public BandStackEntry LastVisited => m_lastVisited;

	public int IndexOfCurrentBlind
	{
		get
		{
			return m_nCurrentlySelectedIndex;
		}
		set
		{
			m_nCurrentlySelectedIndex = value;
		}
	}

	public int IndexOfCurrent
	{
		get
		{
			return m_nCurrentlySelectedIndex;
		}
		set
		{
			if (m_lstFilteredList.Count == 0)
			{
				m_nCurrentlySelectedIndex = -1;
				return;
			}
			int num = value;
			if (num > m_lstFilteredList.Count - 1)
			{
				num = m_lstFilteredList.Count - 1;
			}
			m_nCurrentlySelectedIndex = num;
		}
	}

	public string GuidOfCurrent
	{
		get
		{
			string result = "";
			if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
			{
				result = m_lstFilteredList[m_nCurrentlySelectedIndex].GUID;
			}
			return result;
		}
	}

	public BandStackFilter()
	{
		m_sCurrentlySelectedGUID = "";
		m_nCurrentlySelectedIndex = -1;
		FilterOnFrequencies = true;
		FilterOnBands = true;
		FilterOnModes = false;
		FilterOnSubModes = false;
		UserDefined = false;
		m_sSpecificReturnGUID = "";
		m_filterReturnMode = FilterReturnMode.Current;
		m_lstFilteredList = new List<BandStackEntry>();
		m_lastVisited = new BandStackEntry();
	}

	public BandStackFilter Copy()
	{
		BandStackFilter bandStackFilter = new BandStackFilter
		{
			GUID = GUID,
			FilterName = FilterName,
			FilterDescription = FilterDescription,
			FilterOnModes = FilterOnModes,
			FilterOnSubModes = FilterOnSubModes,
			FilterOnFrequencies = FilterOnFrequencies,
			FilterOnBands = FilterOnBands,
			UserDefined = UserDefined,
			m_filterReturnMode = m_filterReturnMode,
			m_sSpecificReturnGUID = m_sSpecificReturnGUID,
			m_nCurrentlySelectedIndex = m_nCurrentlySelectedIndex
		};
		bandStackFilter.m_lastVisited = LastVisited.Copy();
		foreach (BandFrequencyData item in FrequenciesToFilterOn)
		{
			bandStackFilter.FrequenciesToFilterOn.Add(item.Copy());
		}
		foreach (DSPMode item2 in ModesToFilterOn)
		{
			bandStackFilter.ModesToFilterOn.Add(item2);
		}
		foreach (DSPSubMode item3 in SubModesToFilterOn)
		{
			bandStackFilter.SubModesToFilterOn.Add(item3);
		}
		foreach (Band item4 in BandsToFilterOn)
		{
			bandStackFilter.BandsToFilterOn.Add(item4);
		}
		return bandStackFilter;
	}

	public int IndexFromGUID(string sGUID)
	{
		int result = -1;
		for (int i = 0; i < m_lstFilteredList.Count; i++)
		{
			if (m_lstFilteredList[i].GUID == sGUID)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public bool UpdateEntry(BandStackEntry bse)
	{
		bool result = false;
		int num = IndexFromGUID(bse.GUID);
		if (num != -1)
		{
			if (!m_lstFilteredList[num].Locked)
			{
				m_lstFilteredList[num] = bse.Copy();
			}
			m_lstFilteredList[num].Locked = bse.Locked;
			result = true;
			int num2 = BandStackManager.IndexFromGUID(bse.GUID);
			if (num2 != -1)
			{
				BandStackManager.BandstackEntries.RemoveAt(num2);
				BandStackManager.BandstackEntries.Insert(num2, m_lstFilteredList[num].Copy());
			}
		}
		return result;
	}

	public bool UpdateCurrentWithLastVisitedData(bool bCheckForFreqDupe = false)
	{
		bool result = false;
		if (m_nCurrentlySelectedIndex < 0 || m_nCurrentlySelectedIndex > m_lstFilteredList.Count - 1)
		{
			return result;
		}
		if (m_lstFilteredList[m_nCurrentlySelectedIndex].Locked)
		{
			return result;
		}
		if (bCheckForFreqDupe)
		{
			List<BandStackEntry> list = FindEntriesForFrequency(m_lastVisited.Frequency);
			if (list.Count > 0)
			{
				foreach (BandStackEntry item in list)
				{
					int num = IndexFromGUID(item.GUID);
					if (num != -1 && num != m_nCurrentlySelectedIndex)
					{
						return false;
					}
				}
			}
		}
		BandStackEntry bandStackEntry = m_lstFilteredList[m_nCurrentlySelectedIndex];
		bandStackEntry.Band = m_lastVisited.Band;
		bandStackEntry.Filter = m_lastVisited.Filter;
		bandStackEntry.Frequency = m_lastVisited.Frequency;
		bandStackEntry.CentreFrequency = m_lastVisited.CentreFrequency;
		bandStackEntry.CTUNEnabled = m_lastVisited.CTUNEnabled;
		bandStackEntry.Filter = m_lastVisited.Filter;
		bandStackEntry.Mode = m_lastVisited.Mode;
		bandStackEntry.SubMode = m_lastVisited.SubMode;
		bandStackEntry.ZoomFactor = m_lastVisited.ZoomFactor;
		bandStackEntry.ZoomSlider = m_lastVisited.ZoomSlider;
		int num2 = BandStackManager.IndexFromGUID(bandStackEntry.GUID);
		if (num2 != -1)
		{
			BandStackManager.BandstackEntries.RemoveAt(num2);
			BandStackManager.BandstackEntries.Insert(num2, bandStackEntry.Copy());
			result = true;
		}
		return result;
	}

	public void Remove(int index)
	{
		if (index >= 0 && index <= m_lstFilteredList.Count - 1)
		{
			m_lstFilteredList.RemoveAt(index);
			m_nCurrentlySelectedIndex--;
		}
	}

	public void RemoveCurrent()
	{
		if (m_lstFilteredList.Count != 0 && m_nCurrentlySelectedIndex != -1)
		{
			m_lstFilteredList.RemoveAt(m_nCurrentlySelectedIndex);
			m_nCurrentlySelectedIndex--;
		}
	}

	public BandStackEntry EntryByIndex(int index)
	{
		if (m_lstFilteredList.Count == 0)
		{
			return null;
		}
		if (index > m_lstFilteredList.Count - 1)
		{
			return null;
		}
		return m_lstFilteredList[index].Copy();
	}

	public BandStackEntry SelectInitial()
	{
		BandStackEntry bandStackEntry = null;
		if (m_nCurrentlySelectedIndex == -1 && m_lstFilteredList.Count > 0)
		{
			m_nCurrentlySelectedIndex = 0;
		}
		switch (m_filterReturnMode)
		{
		case FilterReturnMode.Current:
			if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
			{
				bandStackEntry = m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
			}
			break;
		case FilterReturnMode.Specific:
		{
			int num = IndexFromGUID(ReturnGUID);
			if (num < 0)
			{
				m_filterReturnMode = FilterReturnMode.Current;
			}
			else
			{
				m_nCurrentlySelectedIndex = num;
			}
			if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
			{
				bandStackEntry = m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
			}
			break;
		}
		case FilterReturnMode.LastVisited:
			m_nCurrentlySelectedIndex = -1;
			bandStackEntry = m_lastVisited.Copy();
			break;
		}
		if (bandStackEntry != null)
		{
			BandStackEntry bandStackEntry2 = bandStackEntry.Copy();
			bandStackEntry2.Locked = false;
			m_lastVisited = bandStackEntry2;
		}
		return bandStackEntry;
	}

	public BandStackEntry First()
	{
		BandStackEntry result = null;
		if (m_lstFilteredList.Count > 0)
		{
			result = m_lstFilteredList[0].Copy();
		}
		return result;
	}

	public BandStackEntry Current()
	{
		BandStackEntry result = null;
		if (m_filterReturnMode == FilterReturnMode.LastVisited && m_nCurrentlySelectedIndex == -1)
		{
			result = m_lastVisited.Copy();
		}
		else
		{
			if (m_nCurrentlySelectedIndex == -1 && m_lstFilteredList.Count > 0)
			{
				m_nCurrentlySelectedIndex = 0;
			}
			if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
			{
				result = m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
			}
		}
		return result;
	}

	public BandStackEntry Next()
	{
		if (m_nCurrentlySelectedIndex == -1 && m_lstFilteredList.Count > 0)
		{
			m_nCurrentlySelectedIndex = 0;
		}
		else
		{
			m_nCurrentlySelectedIndex++;
			if (m_nCurrentlySelectedIndex > m_lstFilteredList.Count - 1)
			{
				m_nCurrentlySelectedIndex = 0;
			}
		}
		if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
		{
			return m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
		}
		return null;
	}

	public BandStackEntry Previous()
	{
		if (m_nCurrentlySelectedIndex == -1 && m_lstFilteredList.Count > 0)
		{
			m_nCurrentlySelectedIndex = 0;
		}
		else
		{
			m_nCurrentlySelectedIndex--;
			if (m_nCurrentlySelectedIndex < 0)
			{
				m_nCurrentlySelectedIndex = m_lstFilteredList.Count - 1;
			}
		}
		if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
		{
			return m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
		}
		return null;
	}

	public List<BandStackEntry> FindEntriesForFrequency(double frequency)
	{
		IEnumerable<BandStackEntry> enumerable = m_lstFilteredList.Where((BandStackEntry item) => item.Frequency == frequency);
		List<BandStackEntry> list = new List<BandStackEntry>();
		foreach (BandStackEntry item in enumerable)
		{
			list.Add(item.Copy());
		}
		return list;
	}

	public List<BandStackEntry> FindForFrequencyRange(double frequencyLow, double frequencyHigh)
	{
		return m_lstFilteredList.Where((BandStackEntry item) => item.Frequency >= frequencyLow && item.Frequency <= frequencyHigh).ToList();
	}

	public void GenerateFilteredList(bool bMaintainSelected, bool bInitalising = false)
	{
		int nCurrentlySelectedIndex = m_nCurrentlySelectedIndex;
		string sGUID = m_sCurrentlySelectedGUID;
		if (!bInitalising)
		{
			sGUID = "";
			if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
			{
				sGUID = m_lstFilteredList[m_nCurrentlySelectedIndex].GUID;
			}
		}
		m_lstFilteredList.Clear();
		m_nCurrentlySelectedIndex = -1;
		List<BandStackEntry> list = new List<BandStackEntry>();
		if (FilterOnBands)
		{
			foreach (Band b in BandsToFilterOn)
			{
				IEnumerable<BandStackEntry> collection = from item in BandStackManager.BandstackEntries
					where item.Band == b
					orderby item.Frequency
					select item;
				list.AddRange(collection);
			}
		}
		if (FilterOnFrequencies)
		{
			list = (FilterOnBands ? list : BandStackManager.BandstackEntries);
			List<BandStackEntry> list2 = new List<BandStackEntry>();
			foreach (BandFrequencyData bfd in FrequenciesToFilterOn)
			{
				IEnumerable<BandStackEntry> collection = from item in list
					where (bfd.lowOnly && bfd.low == item.Frequency) || (!bfd.lowOnly && item.Frequency >= bfd.low && item.Frequency < bfd.high)
					orderby item.Frequency
					select item;
				list2.AddRange(collection);
			}
			list.Clear();
			list.AddRange(list2);
		}
		if (FilterOnModes)
		{
			list = ((FilterOnBands || FilterOnFrequencies) ? list : BandStackManager.BandstackEntries);
			List<BandStackEntry> list3 = new List<BandStackEntry>();
			foreach (DSPMode mode in ModesToFilterOn)
			{
				IEnumerable<BandStackEntry> collection = from item in list
					where item.Mode == mode
					orderby item.Frequency
					select item;
				list3.AddRange(collection);
			}
			list.Clear();
			list.AddRange(list3);
		}
		if (FilterOnSubModes)
		{
			list = ((FilterOnBands || FilterOnFrequencies || FilterOnSubModes) ? list : BandStackManager.BandstackEntries);
			List<BandStackEntry> list4 = new List<BandStackEntry>();
			foreach (DSPSubMode subMode in SubModesToFilterOn)
			{
				IEnumerable<BandStackEntry> collection = from item in list
					where item.SubMode == subMode
					orderby item.Frequency
					select item;
				list4.AddRange(collection);
			}
			list.Clear();
			list.AddRange(list4);
		}
		List<BandStackEntry> collection2 = ((!FilterOnBands && !FilterOnFrequencies && !FilterOnModes && !FilterOnSubModes) ? BandStackManager.BandstackEntries.OrderBy((BandStackEntry i) => i.Frequency).ToList() : list.OrderBy((BandStackEntry i) => i.Frequency).ToList());
		m_lstFilteredList.AddRange(collection2);
		if (bMaintainSelected)
		{
			int num = IndexFromGUID(sGUID);
			if (num == -1)
			{
				num = nCurrentlySelectedIndex;
			}
			if (m_lstFilteredList.Count == 0)
			{
				num = -1;
			}
			if (num > m_lstFilteredList.Count - 1)
			{
				num = m_lstFilteredList.Count - 1;
			}
			m_nCurrentlySelectedIndex = num;
		}
		if (m_lstFilteredList.Count > 0 && m_nCurrentlySelectedIndex == -1)
		{
			m_nCurrentlySelectedIndex = 0;
		}
	}
}
