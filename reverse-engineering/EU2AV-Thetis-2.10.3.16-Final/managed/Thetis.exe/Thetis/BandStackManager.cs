using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Thetis;

internal static class BandStackManager
{
	private static List<BandStackEntry> m_lstEntries;

	private static Dictionary<string, BandStackFilter> m_dictFilters;

	private static FRSRegion m_Region;

	private static bool m_bExtended;

	private static bool m_bReady;

	private static FRSRegion m_oldRegion;

	private static bool m_oldExtended;

	private static List<BandFrequencyData> m_frequencyData;

	private static bool m_bListsNeedRebuild;

	public static List<BandStackEntry> BandstackEntries
	{
		get
		{
			if (m_bListsNeedRebuild)
			{
				initLists();
			}
			return m_lstEntries;
		}
	}

	public static bool Ready => m_bReady;

	public static FRSRegion Region
	{
		get
		{
			return m_Region;
		}
		set
		{
			if (value != m_Region)
			{
				m_Region = value;
				m_bListsNeedRebuild = true;
				initLists();
			}
		}
	}

	public static bool Extended
	{
		get
		{
			return m_bExtended;
		}
		set
		{
			bool flag = value;
			if (flag != m_bExtended)
			{
				m_bExtended = flag;
				m_bListsNeedRebuild = true;
				if (m_Region != FRSRegion.FIRST)
				{
					initLists();
				}
			}
		}
	}

	static BandStackManager()
	{
		m_oldRegion = FRSRegion.FIRST;
		m_oldExtended = false;
		m_bListsNeedRebuild = true;
		m_bReady = false;
		m_Region = FRSRegion.FIRST;
		m_bExtended = false;
		m_lstEntries = new List<BandStackEntry>();
		m_dictFilters = new Dictionary<string, BandStackFilter>();
	}

	public static void SaveToDB()
	{
		DB.AddBandStack2Entry(m_lstEntries);
		foreach (KeyValuePair<string, BandStackFilter> dictFilter in m_dictFilters)
		{
			DB.SaveBandStack2Filter(dictFilter.Value);
		}
	}

	public static void RegionReset()
	{
		DB.RemoveAllBandStack2Entries();
		m_bListsNeedRebuild = true;
		initLists();
	}

	private static void initLists()
	{
		m_bListsNeedRebuild = false;
		m_lstEntries.Clear();
		m_dictFilters.Clear();
		foreach (BandStackEntry bandStack2Entry in DB.GetBandStack2Entries())
		{
			m_lstEntries.Add(bandStack2Entry.Copy());
		}
		if (m_lstEntries.Count == 0)
		{
			addStandardFrequencies();
		}
		addStandardFilters();
		foreach (KeyValuePair<string, BandStackFilter> bandStack2Filter in DB.GetBandStack2Filters())
		{
			BandStackFilter value = bandStack2Filter.Value;
			string key = bandStack2Filter.Key;
			if (m_dictFilters.ContainsKey(key))
			{
				m_dictFilters.Remove(key);
			}
			internalAddFilter(value.Copy(), bInitalising: true);
		}
		m_bReady = true;
	}

	public static int IndexFromGUID(string sGUID)
	{
		int result = -1;
		for (int i = 0; i < m_lstEntries.Count; i++)
		{
			if (m_lstEntries[i].GUID == sGUID)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private static void addStandardFilters()
	{
		foreach (Band value in Enum.GetValues(typeof(Band)))
		{
			if (value != Band.FIRST && value != Band.LAST)
			{
				List<BandFrequencyData> frequencyRangesForBand = GetFrequencyRangesForBand(value, m_bExtended, m_Region);
				BandStackFilter bandStackFilter = new BandStackFilter();
				bandStackFilter.FilterName = value.ToString();
				bandStackFilter.FilterDescription = "";
				bandStackFilter.FilterOnBands = true;
				bandStackFilter.FilterOnFrequencies = false;
				bandStackFilter.FilterOnModes = false;
				bandStackFilter.UserDefined = false;
				bandStackFilter.BandsToFilterOn.Add(value);
				bandStackFilter.FrequenciesToFilterOn.AddRange(frequencyRangesForBand);
				internalAddFilter(bandStackFilter);
			}
		}
	}

	public static BandStackFilter GetFilter(string sFilterName, bool bIncludeUserDefined = true)
	{
		if (m_dictFilters.ContainsKey(sFilterName) && ((bIncludeUserDefined && m_dictFilters[sFilterName].UserDefined) || !m_dictFilters[sFilterName].UserDefined))
		{
			return m_dictFilters[sFilterName];
		}
		return null;
	}

	public static BandStackFilter GetFilter(Band b, bool bIncludeUserDefined = true)
	{
		return GetFilter(b.ToString(), bIncludeUserDefined);
	}

	public static List<BandStackFilter> GetFilters(Band b, bool onlyFirst = false, bool bIncludeUserDefined = true)
	{
		List<BandStackFilter> list = new List<BandStackFilter>();
		foreach (KeyValuePair<string, BandStackFilter> dictFilter in m_dictFilters)
		{
			BandStackFilter value = dictFilter.Value;
			if ((!bIncludeUserDefined || !value.UserDefined) && value.UserDefined)
			{
				continue;
			}
			foreach (Band item in value.BandsToFilterOn)
			{
				if (b == item)
				{
					list.Add(value);
					if (onlyFirst)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	public static bool DoesFilterNameExist(string sFilterName)
	{
		return m_dictFilters.ContainsKey(sFilterName);
	}

	private static bool internalAddFilter(BandStackFilter bsf, bool bInitalising = false)
	{
		if (m_dictFilters.ContainsKey(bsf.FilterName))
		{
			return false;
		}
		bsf.GenerateFilteredList(bMaintainSelected: true, bInitalising);
		bsf.SelectInitial();
		m_dictFilters.Add(bsf.FilterName, bsf);
		return true;
	}

	public static bool AddFilter(BandStackFilter bsf)
	{
		return internalAddFilter(bsf);
	}

	public static void AddEntry(BandStackEntry bse)
	{
		bse.CentreFrequency = Math.Round(bse.CentreFrequency, 6);
		bse.Frequency = Math.Round(bse.Frequency, 6);
		m_lstEntries.Add(bse);
		DB.AddBandStack2Entry(bse);
	}

	public static bool DeleteEntry(BandStackEntry bse)
	{
		bool result = false;
		int num = IndexFromGUID(bse.GUID);
		if (num != -1)
		{
			m_lstEntries.RemoveAt(num);
			result = true;
			DB.RemoveBandStack2Entry(bse);
		}
		return result;
	}

	public static bool DeleteEntry(string sGUID)
	{
		bool result = false;
		int num = IndexFromGUID(sGUID);
		if (num != -1)
		{
			m_lstEntries.RemoveAt(num);
			result = true;
		}
		return result;
	}

	public static List<BandFrequencyData> GetBandFrequencyDataForFrequency(double frequency, bool extended, FRSRegion region, Band band = Band.LAST)
	{
		if (extended)
		{
			region = FRSRegion.Extended;
		}
		List<BandFrequencyData> source = frequencyData(region);
		IEnumerable<BandFrequencyData> source2 = ((band != Band.LAST) ? source.Where((BandFrequencyData item) => ((item.lowOnly && frequency == item.low) || (frequency >= item.low && frequency < item.high && !item.lowOnly)) && item.region == region && item.band == band) : source.Where((BandFrequencyData item) => ((item.lowOnly && frequency == item.low) || (frequency >= item.low && frequency < item.high && !item.lowOnly)) && item.region == region));
		List<BandFrequencyData> list = source2.ToList();
		if (list.Count == 0)
		{
			if (band == Band.LAST)
			{
				list.Add(new BandFrequencyData(0.0, 0.0, Band.GEN, BandType.GEN, lowOnly: true, region));
			}
			else
			{
				list.Add(new BandFrequencyData(0.0, 0.0, band, bandToBandType(band), lowOnly: true, region));
			}
		}
		return list;
	}

	public static List<BandFrequencyData> GetFrequencyRangesForBand(Band band, bool extended, FRSRegion region)
	{
		if (extended)
		{
			region = FRSRegion.Extended;
		}
		return (from item in frequencyData(region)
			where item.band == band && item.region == region
			select item).ToList();
	}

	public static bool IsFrequencyInBandType(double frequency, BandType bandType, bool extended, FRSRegion region)
	{
		if (extended)
		{
			region = FRSRegion.Extended;
		}
		List<BandFrequencyData> bandFrequencyDataForFrequency = GetBandFrequencyDataForFrequency(frequency, extended, region);
		bool result = false;
		foreach (BandFrequencyData item in bandFrequencyDataForFrequency)
		{
			if (item.bandType == bandType)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool IsOKToTX(double frequency, bool extended, FRSRegion region)
	{
		bool result = false;
		if (extended)
		{
			region = FRSRegion.Extended;
		}
		List<BandFrequencyData> bandFrequencyDataForFrequency = GetBandFrequencyDataForFrequency(frequency, extended, region);
		if (bandFrequencyDataForFrequency.Count > 0)
		{
			foreach (BandFrequencyData item in bandFrequencyDataForFrequency)
			{
				if (item.bandType == BandType.HF && item.band != Band.WWV && item.band != Band.BLMF)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private static void addStandardFrequencies()
	{
		switch (m_Region)
		{
		case FRSRegion.Australia:
			AddRegion2BandStack();
			break;
		case FRSRegion.US:
			AddRegion2BandStack(bIgnore60m: true);
			AddUS_PlusBandStack();
			break;
		case FRSRegion.Japan:
			AddRegion3BandStack();
			break;
		case FRSRegion.India:
			AddRegion1BandStack();
			break;
		case FRSRegion.Spain:
		case FRSRegion.Slovakia:
			AddRegion1BandStack();
			break;
		case FRSRegion.Europe:
		case FRSRegion.Italy_Plus:
		case FRSRegion.Germany:
			AddRegion1BandStack();
			break;
		case FRSRegion.Israel:
			AddRegion1BandStack();
			break;
		case FRSRegion.UK:
			AddUK_PlusBandStack();
			break;
		case FRSRegion.Norway:
		case FRSRegion.Denmark:
			AddRegion1BandStack();
			break;
		case FRSRegion.Latvia:
			AddRegion1BandStack();
			break;
		case FRSRegion.Bulgaria:
			AddRegion1BandStack();
			break;
		case FRSRegion.Greece:
			AddRegion1BandStack();
			break;
		case FRSRegion.Hungary:
			AddRegion1BandStack();
			break;
		case FRSRegion.Netherlands:
			AddRegion1BandStack();
			break;
		case FRSRegion.France:
			AddRegion1BandStack();
			break;
		case FRSRegion.Russia:
			AddRegion1BandStack();
			break;
		case FRSRegion.Sweden:
			AddSwedenBandStack();
			break;
		case FRSRegion.Region1:
			AddRegion1BandStack();
			break;
		case FRSRegion.Region2:
			AddRegion2BandStack();
			break;
		case FRSRegion.Region3:
			AddRegion3BandStack();
			break;
		}
		AddBandStackSWL();
	}

	private static BandType bandToBandType(Band b)
	{
		switch (b)
		{
		case Band.B160M:
		case Band.B80M:
		case Band.B60M:
		case Band.B40M:
		case Band.B30M:
		case Band.B20M:
		case Band.B17M:
		case Band.B15M:
		case Band.B12M:
		case Band.B10M:
		case Band.B6M:
			return BandType.HF;
		case Band.B120M:
		case Band.B90M:
		case Band.B61M:
		case Band.B49M:
		case Band.B41M:
		case Band.B31M:
		case Band.B25M:
		case Band.B22M:
		case Band.B19M:
		case Band.B16M:
		case Band.B14M:
		case Band.B13M:
		case Band.B11M:
			return BandType.GEN;
		case Band.WWV:
			return BandType.GEN;
		case Band.VHF0:
		case Band.VHF1:
		case Band.VHF2:
		case Band.VHF3:
		case Band.VHF4:
		case Band.VHF5:
		case Band.VHF6:
		case Band.VHF7:
		case Band.VHF8:
		case Band.VHF9:
		case Band.VHF10:
		case Band.VHF11:
		case Band.VHF12:
		case Band.VHF13:
			return BandType.VHF;
		default:
			return BandType.GEN;
		}
	}

	public static Band GetNearestBandForFrequency(double freq, bool ignoreGen, bool ignoreWWV)
	{
		Band result = Band.FIRST;
		double num = double.MaxValue;
		foreach (BandFrequencyData frequencyDatum in m_frequencyData)
		{
			if ((ignoreGen && frequencyDatum.bandType == BandType.GEN) || (ignoreWWV && frequencyDatum.band == Band.WWV))
			{
				continue;
			}
			double low = frequencyDatum.low;
			double high = frequencyDatum.high;
			if (frequencyDatum.lowOnly)
			{
				double num2 = Math.Abs(freq - low);
				if (num2 < num)
				{
					num = num2;
					result = frequencyDatum.band;
				}
				continue;
			}
			if (freq >= low && freq <= high)
			{
				return frequencyDatum.band;
			}
			double val = Math.Abs(freq - low);
			double val2 = Math.Abs(freq - high);
			double num3 = Math.Min(val, val2);
			if (num3 < num)
			{
				num = num3;
				result = frequencyDatum.band;
			}
		}
		return result;
	}

	public static BandType GetBandTypeForFrequency(double frequency)
	{
		BandType result = BandType.GEN;
		if (m_frequencyData == null || m_frequencyData.Count == 0)
		{
			return result;
		}
		List<BandFrequencyData> list = m_frequencyData.Where((BandFrequencyData bfd) => (bfd.lowOnly && bfd.low == frequency) || (!bfd.lowOnly && frequency >= bfd.low && frequency < bfd.high)).ToList();
		if (list.Count > 0)
		{
			return list.First().bandType;
		}
		return result;
	}

	private static List<BandFrequencyData> frequencyData(FRSRegion region)
	{
		if (m_bExtended)
		{
			region = FRSRegion.Extended;
		}
		if (region != m_oldRegion || m_bExtended != m_oldExtended)
		{
			if (m_frequencyData != null)
			{
				m_frequencyData.Clear();
			}
			else
			{
				m_frequencyData = new List<BandFrequencyData>();
			}
			m_oldRegion = region;
			m_oldExtended = m_bExtended;
			List<BandFrequencyData> list = m_frequencyData;
			switch (region)
			{
			case FRSRegion.Extended:
				list.Add(new BandFrequencyData(0.2, 1.8, Band.BLMF, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(2.5, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(5.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(10.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(15.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(20.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(25.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(3.33, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(7.85, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(14.67, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(2.3, 3.0, Band.B120M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.0, 3.5, Band.B90M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(4.0, 5.1, Band.B61M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.5, 7.0, Band.B49M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.3, 9.0, Band.B41M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(9.0, 10.1, Band.B31M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.16, 13.57, Band.B25M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(13.57, 14.0, Band.B22M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(15.1, 17.0, Band.B19M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(17.0, 18.068, Band.B16M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.168, 21.0, Band.B14M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.45, 24.89, Band.B13M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.99, 28.0, Band.B11M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(1.8, 2.75, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(2.75, 5.25, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.25, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 8.7, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(8.7, 12.075, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(12.075, 16.209, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(16.209, 19.584, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(19.584, 23.17, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(23.17, 26.495, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(26.495, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.US:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 4.0, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.1, 5.5, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.3, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				list.Add(new BandFrequencyData(2.5, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(5.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(10.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(15.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(20.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(25.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(3.33, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(7.85, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(14.67, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(0.2, 1.8, Band.BLMF, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(1.8, 3.0, Band.B120M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.0, 4.1, Band.B90M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(4.1, 5.06, Band.B61M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.06, 7.2, Band.B49M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.2, 9.0, Band.B41M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(9.0, 11.6, Band.B31M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(11.6, 13.57, Band.B25M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(13.57, 13.87, Band.B22M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(13.87, 17.0, Band.B19M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(17.0, 18.0, Band.B16M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.0, 21.0, Band.B14M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 25.0, Band.B13M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(25.0, 28.0, Band.B11M, BandType.GEN, lowOnly: false, region));
				break;
			case FRSRegion.India:
				list.Add(new BandFrequencyData(1.81, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.9, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Spain:
				list.Add(new BandFrequencyData(1.81, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.1, 5.5, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Europe:
				list.Add(new BandFrequencyData(1.81, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.1, 5.5, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Israel:
				list.Add(new BandFrequencyData(1.81, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 5.5, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.UK:
				list.Add(new BandFrequencyData(1.81, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.25, 5.41, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.03, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Italy_Plus:
				list.Add(new BandFrequencyData(1.83, 1.85, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 6.975, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(6.975, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.08, 51.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Japan:
				list.Add(new BandFrequencyData(1.83, 1.9125, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.805, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(4.629995, 4.630005, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(6.975, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Australia:
				list.Add(new BandFrequencyData(1.81, 1.875, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.3, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Norway:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.25, 5.45, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Denmark:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.25, 5.45, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Latvia:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Slovakia:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Bulgaria:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Greece:
				list.Add(new BandFrequencyData(1.8, 1.85, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Hungary:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.1, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Netherlands:
				list.Add(new BandFrequencyData(1.8, 1.88, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.1, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.France:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Russia:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 25.14, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Sweden:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.31, 5.93, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Germany:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.3515, 5.3665, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 51.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Region1:
				list.Add(new BandFrequencyData(1.81, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.3515, 5.3665, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Region2:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 4.0, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.3515, 5.3665, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.3, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			case FRSRegion.Region3:
				list.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.5, 3.9, Band.B80M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.3515, 5.3665, Band.B60M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, lowOnly: false, region));
				list.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, lowOnly: false, region));
				break;
			}
			if (region != FRSRegion.US && region != FRSRegion.Extended)
			{
				list.Add(new BandFrequencyData(2.5, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(5.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(10.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(15.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(20.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(25.0, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(3.33, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(7.85, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(14.67, 0.0, Band.WWV, BandType.HF, lowOnly: true, region));
				list.Add(new BandFrequencyData(0.2, 1.8, Band.BLMF, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(2.3, 3.0, Band.B120M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(3.0, 3.5, Band.B90M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(4.1, 5.06, Band.B61M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(5.06, 7.2, Band.B49M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(7.2, 9.0, Band.B41M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(9.0, 9.99, Band.B31M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(11.6, 13.57, Band.B25M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(13.57, 13.87, Band.B22M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(15.1, 17.0, Band.B19M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(17.0, 18.0, Band.B16M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(18.0, 21.0, Band.B14M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(21.0, 25.0, Band.B13M, BandType.GEN, lowOnly: false, region));
				list.Add(new BandFrequencyData(25.0, 28.0, Band.B11M, BandType.GEN, lowOnly: false, region));
			}
			return list;
		}
		return m_frequencyData;
	}

	public static string ModeToString(DSPMode mode)
	{
		return mode.ToString();
	}

	public static DSPMode StringToMode(string mode)
	{
		if (Enum.TryParse<DSPMode>(mode, out var result))
		{
			return result;
		}
		return DSPMode.FIRST;
	}

	public static string FilterToString(Filter filter)
	{
		return filter.ToString();
	}

	public static Filter StringToFilter(string filter)
	{
		if (Enum.TryParse<Filter>(filter, out var result))
		{
			return result;
		}
		return Filter.NONE;
	}

	public static Color BandToColour(Band b)
	{
		switch (b)
		{
		case Band.GEN:
			return Color.White;
		case Band.B160M:
		case Band.B80M:
		case Band.B60M:
		case Band.B40M:
		case Band.B30M:
		case Band.B20M:
		case Band.B17M:
		case Band.B15M:
		case Band.B12M:
		case Band.B10M:
		case Band.B6M:
		case Band.B2M:
			return Color.White;
		case Band.WWV:
			return Color.Green;
		case Band.BLMF:
			return Color.White;
		case Band.B120M:
		case Band.B90M:
		case Band.B61M:
		case Band.B49M:
		case Band.B41M:
		case Band.B31M:
		case Band.B25M:
		case Band.B22M:
		case Band.B19M:
		case Band.B16M:
		case Band.B14M:
		case Band.B13M:
		case Band.B11M:
			return Color.Coral;
		case Band.VHF0:
		case Band.VHF1:
		case Band.VHF2:
		case Band.VHF3:
		case Band.VHF4:
		case Band.VHF5:
		case Band.VHF6:
		case Band.VHF7:
		case Band.VHF8:
		case Band.VHF9:
		case Band.VHF10:
		case Band.VHF11:
		case Band.VHF12:
		case Band.VHF13:
			return Color.Gold;
		default:
			return Color.White;
		}
	}

	public static string BandToString(Band b)
	{
		return (b switch
		{
			Band.GEN => "GEN", 
			Band.B160M => "160M", 
			Band.B80M => "80M", 
			Band.B60M => "60M", 
			Band.B40M => "40M", 
			Band.B30M => "30M", 
			Band.B20M => "20M", 
			Band.B17M => "17M", 
			Band.B15M => "15M", 
			Band.B12M => "12M", 
			Band.B10M => "10M", 
			Band.B6M => "6M", 
			Band.B2M => "2M", 
			Band.WWV => "WWV", 
			Band.BLMF => "LMF", 
			Band.B120M => "120M", 
			Band.B90M => "90M", 
			Band.B61M => "61M", 
			Band.B49M => "49M", 
			Band.B41M => "41M", 
			Band.B31M => "31M", 
			Band.B25M => "25M", 
			Band.B22M => "22M", 
			Band.B19M => "19M", 
			Band.B16M => "16M", 
			Band.B14M => "14M", 
			Band.B13M => "13M", 
			Band.B11M => "11M", 
			Band.VHF0 => "VHF0", 
			Band.VHF1 => "VHF1", 
			Band.VHF2 => "VHF2", 
			Band.VHF3 => "VHF3", 
			Band.VHF4 => "VHF4", 
			Band.VHF5 => "VHF5", 
			Band.VHF6 => "VHF6", 
			Band.VHF7 => "VHF7", 
			Band.VHF8 => "VHF8", 
			Band.VHF9 => "VHF9", 
			Band.VHF10 => "VHF10", 
			Band.VHF11 => "VHF11", 
			Band.VHF12 => "VHF12", 
			Band.VHF13 => "VHF13", 
			_ => "GEN", 
		}).ToUpper();
	}

	public static Band StringToBand(string s)
	{
		if (s.StartsWith("B") || s.StartsWith("b"))
		{
			s = s.Substring(1);
		}
		return s.ToUpper() switch
		{
			"GEN" => Band.GEN, 
			"160M" => Band.B160M, 
			"80M" => Band.B80M, 
			"60M" => Band.B60M, 
			"40M" => Band.B40M, 
			"30M" => Band.B30M, 
			"20M" => Band.B20M, 
			"17M" => Band.B17M, 
			"15M" => Band.B15M, 
			"12M" => Band.B12M, 
			"10M" => Band.B10M, 
			"6M" => Band.B6M, 
			"2M" => Band.B2M, 
			"WWV" => Band.WWV, 
			"LMF" => Band.BLMF, 
			"120M" => Band.B120M, 
			"90M" => Band.B90M, 
			"61M" => Band.B61M, 
			"49M" => Band.B49M, 
			"41M" => Band.B41M, 
			"31M" => Band.B31M, 
			"25M" => Band.B25M, 
			"22M" => Band.B22M, 
			"19M" => Band.B19M, 
			"16M" => Band.B16M, 
			"14M" => Band.B14M, 
			"13M" => Band.B13M, 
			"11M" => Band.B11M, 
			"VHF0" => Band.VHF0, 
			"VHF1" => Band.VHF1, 
			"VHF2" => Band.VHF2, 
			"VHF3" => Band.VHF3, 
			"VHF4" => Band.VHF4, 
			"VHF5" => Band.VHF5, 
			"VHF6" => Band.VHF6, 
			"VHF7" => Band.VHF7, 
			"VHF8" => Band.VHF8, 
			"VHF9" => Band.VHF9, 
			"VHF10" => Band.VHF10, 
			"VHF11" => Band.VHF11, 
			"VHF12" => Band.VHF12, 
			"VHF13" => Band.VHF13, 
			_ => Band.GEN, 
		};
	}

	private static void addBSObjectToEntries(object[] o, bool bIgnore60m = false)
	{
		for (int i = 0; i < o.Length / 7; i++)
		{
			BandStackEntry bandStackEntry = new BandStackEntry
			{
				Band = StringToBand((string)o[i * 7]),
				Description = (string)o[i * 7],
				Mode = StringToMode((string)o[i * 7 + 1]),
				Filter = StringToFilter((string)o[i * 7 + 2]),
				Frequency = (double)o[i * 7 + 3],
				CTUNEnabled = (bool)o[i * 7 + 4],
				ZoomFactor = 1.0,
				ZoomSlider = (int)o[i * 7 + 5],
				CentreFrequency = (double)o[i * 7 + 6],
				Locked = false
			};
			if (!bIgnore60m || bandStackEntry.Band != Band.B60M)
			{
				if (m_Region == FRSRegion.US && bandStackEntry.Band == Band.B60M)
				{
					bandStackEntry.Locked = true;
				}
				if (bandStackEntry.Band == Band.WWV)
				{
					bandStackEntry.Locked = true;
				}
				while (isGuidInList(bandStackEntry.GUID))
				{
					bandStackEntry.GUID = Guid.NewGuid().ToString();
				}
				m_lstEntries.Add(bandStackEntry);
			}
		}
	}

	private static bool isGuidInList(string sGUID)
	{
		return m_lstEntries.Where((BandStackEntry item) => item.GUID == sGUID).Count() > 0;
	}

	private static void AddRegion1BandStack()
	{
		addBSObjectToEntries(new object[455]
		{
			"160M", "CWL", "F1", 1.81, false, 150, 0.0, "160M", "DIGU", "F1",
			1.838, false, 150, 0.0, "160M", "LSB", "F6", 1.843, false, 150,
			0.0, "160M", "LSB", "F6", 1.85, false, 150, 0.0, "80M", "CWL",
			"F1", 3.505, false, 150, 0.0, "80M", "CWL", "F1", 3.51, false,
			150, 0.0, "80M", "DIGU", "F1", 3.59, false, 150, 0.0, "80M",
			"LSB", "F6", 3.75, false, 150, 0.0, "60M", "USB", "F6", 5.354,
			false, 150, 0.0, "40M", "CWL", "F1", 7.005, false, 150, 0.0,
			"40M", "CWL", "F1", 7.01, false, 150, 0.0, "40M", "DIGU", "F1",
			7.045, false, 150, 0.0, "40M", "LSB", "F6", 7.09, false, 150,
			0.0, "40M", "LSB", "F6", 7.1, false, 150, 0.0, "30M", "CWU",
			"F1", 10.107, false, 150, 0.0, "30M", "CWU", "F1", 10.11, false,
			150, 0.0, "30M", "CWU", "F1", 10.115, false, 150, 0.0, "30M",
			"CWU", "F1", 10.12, false, 150, 0.0, "30M", "DIGU", "F1", 10.14,
			false, 150, 0.0, "20M", "CWU", "F1", 14.005, false, 150, 0.0,
			"20M", "CWU", "F1", 14.01, false, 150, 0.0, "20M", "DIGU", "F1",
			14.085, false, 150, 0.0, "20M", "USB", "F6", 14.155, false, 150,
			0.0, "20M", "USB", "F6", 14.225, false, 150, 0.0, "17M", "CWU",
			"F1", 18.07, false, 150, 0.0, "17M", "CWU", "F1", 18.078, false,
			150, 0.0, "17M", "DIGU", "F1", 18.1, false, 150, 0.0, "17M",
			"USB", "F6", 18.12, false, 150, 0.0, "17M", "USB", "F6", 18.14,
			false, 150, 0.0, "15M", "CWU", "F1", 21.005, false, 150, 0.0,
			"15M", "CWU", "F1", 21.01, false, 150, 0.0, "15M", "DIGU", "F1",
			21.09, false, 150, 0.0, "15M", "USB", "F6", 21.21, false, 150,
			0.0, "15M", "USB", "F6", 21.29, false, 150, 0.0, "12M", "CWU",
			"F1", 24.9, false, 150, 0.0, "12M", "CWU", "F1", 24.905, false,
			150, 0.0, "12M", "DIGU", "F1", 24.92, false, 150, 0.0, "12M",
			"USB", "F6", 24.94, false, 150, 0.0, "12M", "USB", "F6", 24.95,
			false, 150, 0.0, "10M", "CWU", "F1", 28.005, false, 150, 0.0,
			"10M", "CWU", "F1", 28.01, false, 150, 0.0, "10M", "DIGU", "F1",
			28.12, false, 150, 0.0, "10M", "USB", "F6", 28.4, false, 150,
			0.0, "10M", "USB", "F6", 28.45, false, 150, 0.0, "6M", "CWU",
			"F1", 50.08, false, 150, 0.0, "6M", "CWU", "F1", 50.09, false,
			150, 0.0, "6M", "USB", "F6", 50.15, false, 150, 0.0, "6M",
			"USB", "F6", 50.165, false, 150, 0.0, "6M", "DIGU", "F1", 50.25,
			false, 150, 0.0, "2M", "CWU", "F1", 144.04, false, 150, 0.0,
			"2M", "CWU", "F1", 144.05, false, 150, 0.0, "2M", "DIGU", "F1",
			144.138, false, 150, 0.0, "2M", "USB", "F6", 144.3, false, 150,
			0.0, "2M", "USB", "F6", 144.31, false, 150, 0.0, "WWV", "SAM",
			"F7", 2.5, false, 150, 0.0, "WWV", "SAM", "F7", 5.0, false,
			150, 0.0, "WWV", "SAM", "F7", 10.0, false, 150, 0.0, "WWV",
			"SAM", "F7", 15.0, false, 150, 0.0, "WWV", "SAM", "F7", 20.0,
			false, 150, 0.0, "WWV", "SAM", "F5", 25.0, false, 150, 0.0,
			"GEN", "SAM", "F6", 13.845, false, 150, 0.0, "GEN", "SAM", "F7",
			5.975, false, 150, 0.0, "GEN", "SAM", "F7", 9.55, false, 150,
			0.0, "GEN", "SAM", "F7", 3.85, false, 150, 0.0, "GEN", "SAM",
			"F8", 0.59, false, 150, 0.0
		});
	}

	private static void AddRegion2BandStack(bool bIgnore60m = false)
	{
		addBSObjectToEntries(new object[462]
		{
			"160M", "CWL", "F5", 1.81, false, 150, 0.0, "160M", "CWU", "F1",
			1.835, false, 150, 0.0, "160M", "LSB", "F6", 1.84, false, 150,
			0.0, "160M", "LSB", "F6", 1.845, false, 150, 0.0, "160M", "LSB",
			"F6", 1.845, false, 150, 0.0, "80M", "CWU", "F1", 3.501, false,
			150, 0.0, "80M", "CWU", "F1", 3.52, false, 150, 0.0, "80M",
			"LSB", "F6", 3.65, false, 150, 0.0, "80M", "LSB", "F6", 3.715,
			false, 150, 0.0, "80M", "SAM", "F5", 3.875, false, 150, 0.0,
			"60M", "USB", "F6", 5.354, false, 150, 0.0, "40M", "CWU", "F1",
			7.001, false, 150, 0.0, "40M", "CWU", "F3", 7.021, false, 150,
			0.0, "40M", "LSB", "F6", 7.152, false, 150, 0.0, "40M", "LSB",
			"F6", 7.18, false, 150, 0.0, "40M", "LSB", "F6", 7.255, false,
			150, 0.0, "30M", "CWU", "F1", 10.107, false, 150, 0.0, "30M",
			"CWU", "F1", 10.11, false, 150, 0.0, "30M", "CWU", "F1", 10.12,
			false, 150, 0.0, "30M", "CWU", "F1", 10.13, false, 150, 0.0,
			"30M", "CWU", "F5", 10.14, false, 150, 0.0, "20M", "CWU", "F1",
			14.01, false, 150, 0.0, "20M", "CWU", "F1", 14.02, false, 150,
			0.0, "20M", "USB", "F6", 14.155, false, 150, 0.0, "20M", "USB",
			"F6", 14.23, false, 150, 0.0, "20M", "USB", "F6", 14.334, false,
			150, 0.0, "17M", "CWU", "F1", 18.07, false, 150, 0.0, "17M",
			"CWU", "F1", 18.09, false, 150, 0.0, "17M", "USB", "F6", 18.125,
			false, 150, 0.0, "17M", "USB", "F6", 18.135, false, 150, 0.0,
			"17M", "USB", "F6", 18.14, false, 150, 0.0, "15M", "CWU", "F1",
			21.001, false, 150, 0.0, "15M", "CWU", "F1", 21.021, false, 150,
			0.0, "15M", "USB", "F6", 21.205, false, 150, 0.0, "15M", "USB",
			"F6", 21.255, false, 150, 0.0, "15M", "USB", "F6", 21.3, false,
			150, 0.0, "12M", "CWU", "F1", 24.895, false, 150, 0.0, "12M",
			"CWU", "F1", 24.898, false, 150, 0.0, "12M", "USB", "F6", 24.931,
			false, 150, 0.0, "12M", "USB", "F6", 24.94, false, 150, 0.0,
			"12M", "USB", "F6", 24.95, false, 150, 0.0, "10M", "CWU", "F1",
			28.01, false, 150, 0.0, "10M", "CWU", "F1", 28.02, false, 150,
			0.0, "10M", "USB", "F6", 28.305, false, 150, 0.0, "10M", "USB",
			"F6", 28.35, false, 150, 0.0, "10M", "USB", "F6", 28.45, false,
			150, 0.0, "6M", "CWU", "F1", 50.01, false, 150, 0.0, "6M",
			"CWU", "F1", 50.015, false, 150, 0.0, "6M", "USB", "F6", 50.125,
			false, 150, 0.0, "6M", "USB", "F6", 50.13, false, 150, 0.0,
			"6M", "USB", "F6", 50.2, false, 150, 0.0, "2M", "CWU", "F1",
			144.01, false, 150, 0.0, "2M", "CWU", "F1", 144.015, false, 150,
			0.0, "2M", "USB", "F6", 144.2, false, 150, 0.0, "2M", "USB",
			"F6", 144.22, false, 150, 0.0, "2M", "USB", "F6", 144.21, false,
			150, 0.0, "WWV", "SAM", "F7", 2.5, false, 150, 0.0, "WWV",
			"SAM", "F7", 5.0, false, 150, 0.0, "WWV", "SAM", "F7", 10.0,
			false, 150, 0.0, "WWV", "SAM", "F7", 15.0, false, 150, 0.0,
			"WWV", "SAM", "F7", 20.0, false, 150, 0.0, "GEN", "SAM", "F5",
			13.845, false, 150, 0.0, "GEN", "SAM", "F5", 9.55, false, 150,
			0.0, "GEN", "SAM", "F5", 5.975, false, 150, 0.0, "GEN", "SAM",
			"F5", 3.25, false, 150, 0.0, "GEN", "SAM", "F4", 0.59, false,
			150, 0.0
		}, bIgnore60m);
	}

	private static void AddRegion3BandStack()
	{
		addBSObjectToEntries(new object[336]
		{
			"160M", "CWL", "F1", 1.82, false, 150, 0.0, "160M", "DIGU", "F1",
			1.832, false, 150, 0.0, "160M", "LSB", "F6", 1.843, false, 150,
			0.0, "80M", "CWL", "F1", 3.51, false, 150, 0.0, "80M", "DIGU",
			"F1", 3.58, false, 150, 0.0, "80M", "LSB", "F6", 3.75, false,
			150, 0.0, "60M", "USB", "F6", 5.354, false, 150, 0.0, "40M",
			"CWL", "F1", 7.01, false, 150, 0.0, "40M", "DIGU", "F1", 7.035,
			false, 150, 0.0, "40M", "LSB", "F6", 7.12, false, 150, 0.0,
			"30M", "CWU", "F1", 10.11, false, 150, 0.0, "30M", "CWU", "F1",
			10.12, false, 150, 0.0, "30M", "DIGU", "F1", 10.14, false, 150,
			0.0, "20M", "CWU", "F1", 14.01, false, 150, 0.0, "20M", "DIGU",
			"F1", 14.085, false, 150, 0.0, "20M", "USB", "F6", 14.225, false,
			150, 0.0, "17M", "CWU", "F1", 18.078, false, 150, 0.0, "17M",
			"DIGU", "F1", 18.1, false, 150, 0.0, "17M", "USB", "F6", 18.14,
			false, 150, 0.0, "15M", "CWU", "F1", 21.01, false, 150, 0.0,
			"15M", "DIGU", "F1", 21.09, false, 150, 0.0, "15M", "USB", "F6",
			21.3, false, 150, 0.0, "12M", "CWU", "F1", 24.9, false, 150,
			0.0, "12M", "DIGU", "F1", 24.92, false, 150, 0.0, "12M", "USB",
			"F6", 24.94, false, 150, 0.0, "10M", "CWU", "F1", 28.01, false,
			150, 0.0, "10M", "DIGU", "F1", 28.12, false, 150, 0.0, "10M",
			"USB", "F6", 28.4, false, 150, 0.0, "6M", "CWU", "F1", 50.09,
			false, 150, 0.0, "6M", "USB", "F6", 50.15, false, 150, 0.0,
			"6M", "DIGU", "F1", 50.25, false, 150, 0.0, "2M", "CWU", "F1",
			144.05, false, 150, 0.0, "2M", "DIGU", "F1", 144.138, false, 150,
			0.0, "2M", "USB", "F6", 144.2, false, 150, 0.0, "WWV", "SAM",
			"F5", 2.5, false, 150, 0.0, "WWV", "SAM", "F5", 5.0, false,
			150, 0.0, "WWV", "SAM", "F5", 10.0, false, 150, 0.0, "WWV",
			"SAM", "F5", 15.0, false, 150, 0.0, "WWV", "SAM", "F5", 20.0,
			false, 150, 0.0, "WWV", "SAM", "F5", 25.0, false, 150, 0.0,
			"WWV", "USB", "F6", 3.33, false, 150, 0.0, "WWV", "USB", "F6",
			7.85, false, 150, 0.0, "WWV", "USB", "F6", 14.67, false, 150,
			0.0, "GEN", "SAM", "F6", 13.845, false, 150, 0.0, "GEN", "SAM",
			"F7", 5.975, false, 150, 0.0, "GEN", "SAM", "F7", 9.55, false,
			150, 0.0, "GEN", "SAM", "F7", 3.85, false, 150, 0.0, "GEN",
			"SAM", "F8", 0.59, false, 150, 0.0
		});
	}

	private static void AddBandStackSWL()
	{
		addBSObjectToEntries(new object[371]
		{
			"LMF", "SAM", "F4", 0.56, false, 150, 0.0, "LMF", "SAM", "F4",
			0.72, false, 150, 0.0, "LMF", "SAM", "F4", 0.78, false, 150,
			0.0, "LMF", "SAM", "F4", 1.0, false, 150, 0.0, "LMF", "SAM",
			"F4", 1.7, false, 150, 0.0, "120M", "SAM", "F4", 2.4, false,
			150, 0.0, "120M", "SAM", "F4", 2.41, false, 150, 0.0, "120M",
			"SAM", "F4", 2.42, false, 150, 0.0, "90M", "SAM", "F4", 3.3,
			false, 150, 0.0, "90M", "SAM", "F4", 3.31, false, 150, 0.0,
			"90M", "SAM", "F4", 3.32, false, 150, 0.0, "61M", "SAM", "F4",
			4.7, false, 150, 0.0, "61M", "SAM", "F4", 4.8, false, 150,
			0.0, "61M", "SAM", "F4", 4.82, false, 150, 0.0, "49M", "SAM",
			"F4", 5.6, false, 150, 0.0, "49M", "SAM", "F4", 5.7, false,
			150, 0.0, "49M", "SAM", "F4", 5.8, false, 150, 0.0, "49M",
			"SAM", "F4", 5.9, false, 150, 0.0, "49M", "SAM", "F4", 6.0,
			false, 150, 0.0, "49M", "SAM", "F4", 6.2, false, 150, 0.0,
			"41M", "SAM", "F4", 7.31, false, 150, 0.0, "41M", "SAM", "F4",
			7.4, false, 150, 0.0, "41M", "SAM", "F4", 7.5, false, 150,
			0.0, "31M", "SAM", "F4", 9.1, false, 150, 0.0, "31M", "SAM",
			"F4", 9.2, false, 150, 0.0, "31M", "SAM", "F4", 9.3, false,
			150, 0.0, "31M", "SAM", "F4", 9.4, false, 150, 0.0, "31M",
			"SAM", "F4", 9.5, false, 150, 0.0, "31M", "SAM", "F4", 9.6,
			false, 150, 0.0, "25M", "SAM", "F4", 11.7, false, 150, 0.0,
			"25M", "SAM", "F4", 11.8, false, 150, 0.0, "25M", "SAM", "F4",
			11.9, false, 150, 0.0, "22M", "SAM", "F4", 13.6, false, 150,
			0.0, "22M", "SAM", "F4", 13.7, false, 150, 0.0, "22M", "SAM",
			"F4", 13.8, false, 150, 0.0, "19M", "SAM", "F4", 15.2, false,
			150, 0.0, "19M", "SAM", "F4", 15.3, false, 150, 0.0, "19M",
			"SAM", "F4", 15.4, false, 150, 0.0, "16M", "SAM", "F4", 17.5,
			false, 150, 0.0, "16M", "SAM", "F4", 17.6, false, 150, 0.0,
			"16M", "SAM", "F4", 17.7, false, 150, 0.0, "14M", "SAM", "F4",
			18.9, false, 150, 0.0, "14M", "SAM", "F4", 19.0, false, 150,
			0.0, "14M", "SAM", "F4", 19.1, false, 150, 0.0, "13M", "SAM",
			"F4", 21.5, false, 150, 0.0, "13M", "SAM", "F4", 21.6, false,
			150, 0.0, "13M", "SAM", "F4", 21.7, false, 150, 0.0, "11M",
			"SAM", "F4", 25.7, false, 150, 0.0, "11M", "SAM", "F4", 26.0,
			false, 150, 0.0, "11M", "SAM", "F4", 26.5, false, 150, 0.0,
			"11M", "SAM", "F4", 27.0, false, 150, 0.0, "11M", "SAM", "F4",
			27.5, false, 150, 0.0, "11M", "SAM", "F4", 27.8, false, 150,
			0.0
		});
	}

	private static void AddUK_PlusBandStack()
	{
		addBSObjectToEntries(new object[539]
		{
			"160M", "CWL", "F1", 1.81, false, 150, 0.0, "160M", "CWU", "F1",
			1.82, false, 150, 0.0, "160M", "DIGU", "F1", 1.838, false, 150,
			0.0, "160M", "USB", "F6", 1.84, false, 150, 0.0, "160M", "USB",
			"F6", 1.845, false, 150, 0.0, "80M", "CWL", "F1", 3.505, false,
			150, 0.0, "80M", "CWU", "F1", 3.51, false, 150, 0.0, "80M",
			"DIGU", "F1", 3.59, false, 150, 0.0, "80M", "LSB", "F6", 3.75,
			false, 150, 0.0, "80M", "LSB", "F6", 3.77, false, 150, 0.0,
			"60M", "USB", "F6", 5.2585, false, 150, 0.0, "60M", "USB", "F6",
			5.276, false, 150, 0.0, "60M", "USB", "F6", 5.2885, false, 150,
			0.0, "60M", "USB", "F6", 5.298, false, 150, 0.0, "60M", "USB",
			"F6", 5.313, false, 150, 0.0, "60M", "USB", "F6", 5.333, false,
			150, 0.0, "60M", "USB", "F6", 5.354, false, 150, 0.0, "60M",
			"USB", "F6", 5.362, false, 150, 0.0, "60M", "USB", "F6", 5.378,
			false, 150, 0.0, "60M", "USB", "F6", 5.395, false, 150, 0.0,
			"60M", "USB", "F6", 5.4035, false, 150, 0.0, "40M", "CWL", "F1",
			7.005, false, 150, 0.0, "40M", "CWU", "F1", 7.01, false, 150,
			0.0, "40M", "DIGU", "F1", 7.045, false, 150, 0.0, "40M", "LSB",
			"F6", 7.09, false, 150, 0.0, "40M", "LSB", "F6", 7.1, false,
			150, 0.0, "30M", "CWU", "F1", 10.105, false, 150, 0.0, "30M",
			"CWU", "F1", 10.11, false, 150, 0.0, "30M", "CWU", "F1", 10.12,
			false, 150, 0.0, "30M", "CWU", "F1", 10.13, false, 150, 0.0,
			"30M", "DIGU", "F1", 10.14, false, 150, 0.0, "20M", "CWU", "F1",
			14.005, false, 150, 0.0, "20M", "CWU", "F1", 14.01, false, 150,
			0.0, "20M", "DIGU", "F1", 14.085, false, 150, 0.0, "20M", "USB",
			"F6", 14.145, false, 150, 0.0, "20M", "USB", "F6", 14.225, false,
			150, 0.0, "17M", "CWU", "F1", 18.07, false, 150, 0.0, "17M",
			"CWU", "F1", 18.078, false, 150, 0.0, "17M", "DIGU", "F1", 18.1,
			false, 150, 0.0, "17M", "USB", "F6", 18.14, false, 150, 0.0,
			"17M", "USB", "F6", 18.15, false, 150, 0.0, "15M", "CWU", "F1",
			21.005, false, 150, 0.0, "15M", "CWU", "F1", 21.01, false, 150,
			0.0, "15M", "DIGU", "F1", 21.09, false, 150, 0.0, "15M", "USB",
			"F6", 21.25, false, 150, 0.0, "15M", "USB", "F6", 21.3, false,
			150, 0.0, "12M", "CWU", "F1", 24.9, false, 150, 0.0, "12M",
			"CWU", "F1", 24.91, false, 150, 0.0, "12M", "DIGU", "F1", 24.92,
			false, 150, 0.0, "12M", "USB", "F6", 24.94, false, 150, 0.0,
			"12M", "USB", "F6", 24.95, false, 150, 0.0, "10M", "CWU", "F1",
			28.005, false, 150, 0.0, "10M", "CWU", "F1", 28.01, false, 150,
			0.0, "10M", "DIGU", "F1", 28.12, false, 150, 0.0, "10M", "USB",
			"F6", 28.4, false, 150, 0.0, "10M", "USB", "F6", 28.45, false,
			150, 0.0, "6M", "CWU", "F1", 50.09, false, 150, 0.0, "6M",
			"CWU", "F1", 50.095, false, 150, 0.0, "6M", "USB", "F6", 50.15,
			false, 150, 0.0, "6M", "USB", "F6", 50.16, false, 150, 0.0,
			"6M", "DIGU", "F1", 50.25, false, 150, 0.0, "2M", "CWU", "F1",
			144.03, false, 150, 0.0, "2M", "CWU", "F1", 144.05, false, 150,
			0.0, "2M", "DIGU", "F1", 144.138, false, 150, 0.0, "2M", "USB",
			"F6", 144.3, false, 150, 0.0, "2M", "USB", "F6", 144.31, false,
			150, 0.0, "WWV", "SAM", "F7", 2.5, false, 150, 0.0, "WWV",
			"SAM", "F7", 5.0, false, 150, 0.0, "WWV", "SAM", "F7", 10.0,
			false, 150, 0.0, "WWV", "SAM", "F7", 15.0, false, 150, 0.0,
			"WWV", "SAM", "F7", 20.0, false, 150, 0.0, "WWV", "SAM", "F5",
			25.0, false, 150, 0.0, "GEN", "SAM", "F6", 13.845, false, 150,
			0.0, "GEN", "SAM", "F7", 5.975, false, 150, 0.0, "GEN", "SAM",
			"F7", 9.55, false, 150, 0.0, "GEN", "SAM", "F7", 3.85, false,
			150, 0.0, "GEN", "SAM", "F8", 0.59, false, 150, 0.0
		});
	}

	private static void AddUS_PlusBandStack()
	{
		addBSObjectToEntries(new object[35]
		{
			"60M", "USB", "F6", 5.3306, false, 150, 0.0, "60M", "USB", "F6",
			5.3466, false, 150, 0.0, "60M", "USB", "F6", 5.3571, false, 150,
			0.0, "60M", "USB", "F6", 5.3716, false, 150, 0.0, "60M", "USB",
			"F6", 5.4036, false, 150, 0.0
		});
	}

	private static void AddSwedenBandStack()
	{
		addBSObjectToEntries(new object[511]
		{
			"160M", "CWL", "F1", 1.805, false, 150, 0.0, "160M", "CWL", "F1",
			1.81, false, 150, 0.0, "160M", "DIGU", "F1", 1.838, false, 150,
			0.0, "160M", "LSB", "F6", 1.843, false, 150, 0.0, "160M", "LSB",
			"F6", 1.85, false, 150, 0.0, "80M", "CWL", "F1", 3.505, false,
			150, 0.0, "80M", "CWL", "F1", 3.51, false, 150, 0.0, "80M",
			"DIGU", "F1", 3.59, false, 150, 0.0, "80M", "LSB", "F6", 3.75,
			false, 150, 0.0, "80M", "LSB", "F6", 3.9, false, 150, 0.0,
			"60M", "USB", "F6", 5.31, false, 150, 0.0, "60M", "USB", "F6",
			5.32, false, 150, 0.0, "60M", "USB", "F6", 5.38, false, 150,
			0.0, "60M", "USB", "F6", 5.39, false, 150, 0.0, "40M", "CWL",
			"F1", 7.005, false, 150, 0.0, "40M", "CWL", "F1", 7.01, false,
			150, 0.0, "40M", "DIGU", "F1", 7.045, false, 150, 0.0, "40M",
			"LSB", "F6", 7.09, false, 150, 0.0, "40M", "LSB", "F6", 7.1,
			false, 150, 0.0, "30M", "CWU", "F1", 10.107, false, 150, 0.0,
			"30M", "CWU", "F1", 10.11, false, 150, 0.0, "30M", "CWU", "F1",
			10.115, false, 150, 0.0, "30M", "CWU", "F1", 10.12, false, 150,
			0.0, "30M", "DIGU", "F1", 10.14, false, 150, 0.0, "20M", "CWU",
			"F1", 14.005, false, 150, 0.0, "20M", "CWU", "F1", 14.01, false,
			150, 0.0, "20M", "DIGU", "F1", 14.085, false, 150, 0.0, "20M",
			"USB", "F6", 14.155, false, 150, 0.0, "20M", "USB", "F6", 14.225,
			false, 150, 0.0, "17M", "CWU", "F1", 18.07, false, 150, 0.0,
			"17M", "CWU", "F1", 18.078, false, 150, 0.0, "17M", "DIGU", "F1",
			18.1, false, 150, 0.0, "17M", "USB", "F6", 18.12, false, 150,
			0.0, "17M", "USB", "F6", 18.14, false, 150, 0.0, "15M", "CWU",
			"F1", 21.005, false, 150, 0.0, "15M", "CWU", "F1", 21.01, false,
			150, 0.0, "15M", "DIGU", "F1", 21.09, false, 150, 0.0, "15M",
			"USB", "F6", 21.21, false, 150, 0.0, "15M", "USB", "F6", 21.29,
			false, 150, 0.0, "12M", "CWU", "F1", 24.9, false, 150, 0.0,
			"12M", "CWU", "F1", 24.905, false, 150, 0.0, "12M", "DIGU", "F1",
			24.92, false, 150, 0.0, "12M", "USB", "F6", 24.94, false, 150,
			0.0, "12M", "USB", "F6", 24.95, false, 150, 0.0, "10M", "CWU",
			"F1", 28.005, false, 150, 0.0, "10M", "CWU", "F1", 28.01, false,
			150, 0.0, "10M", "DIGU", "F1", 28.12, false, 150, 0.0, "10M",
			"USB", "F6", 28.4, false, 150, 0.0, "10M", "USB", "F6", 28.45,
			false, 150, 0.0, "6M", "CWU", "F1", 50.08, false, 150, 0.0,
			"6M", "CWU", "F1", 50.09, false, 150, 0.0, "6M", "USB", "F6",
			50.15, false, 150, 0.0, "6M", "USB", "F6", 50.165, false, 150,
			0.0, "6M", "DIGU", "F1", 50.25, false, 150, 0.0, "2M", "CWU",
			"F1", 144.04, false, 150, 0.0, "2M", "CWU", "F1", 144.05, false,
			150, 0.0, "2M", "DIGU", "F1", 144.138, false, 150, 0.0, "2M",
			"USB", "F6", 144.3, false, 150, 0.0, "2M", "USB", "F6", 144.31,
			false, 150, 0.0, "WWV", "SAM", "F5", 2.5, false, 150, 0.0,
			"WWV", "SAM", "F5", 5.0, false, 150, 0.0, "WWV", "SAM", "F5",
			10.0, false, 150, 0.0, "WWV", "SAM", "F5", 15.0, false, 150,
			0.0, "WWV", "SAM", "F5", 20.0, false, 150, 0.0, "WWV", "SAM",
			"F5", 25.0, false, 150, 0.0, "WWV", "USB", "F6", 3.33, false,
			150, 0.0, "WWV", "USB", "F6", 7.85, false, 150, 0.0, "WWV",
			"USB", "F6", 14.67, false, 150, 0.0, "GEN", "SAM", "F6", 13.845,
			false, 150, 0.0, "GEN", "SAM", "F7", 5.975, false, 150, 0.0,
			"GEN", "SAM", "F7", 9.55, false, 150, 0.0, "GEN", "SAM", "F7",
			3.85, false, 150, 0.0, "GEN", "SAM", "F8", 0.59, false, 150,
			0.0
		});
	}

	private static void AddRegionJapanBandStack()
	{
		addBSObjectToEntries(new object[490]
		{
			"160M", "CWL", "F5", 1.81, false, 150, 0.0, "160M", "CWU", "F1",
			1.825, false, 150, 0.0, "160M", "CWU", "F1", 1.835, false, 150,
			0.0, "160M", "USB", "F6", 1.84, false, 150, 0.0, "160M", "USB",
			"F6", 1.845, false, 150, 0.0, "80M", "CWL", "F1", 3.501, false,
			150, 0.0, "80M", "CWU", "F1", 3.51, false, 150, 0.0, "80M",
			"LSB", "F6", 3.751, false, 150, 0.0, "80M", "LSB", "F6", 3.755,
			false, 150, 0.0, "80M", "LSB", "F6", 3.85, false, 150, 0.0,
			"60M", "USB", "F6", 5.3305, false, 150, 0.0, "60M", "USB", "F6",
			5.3465, false, 150, 0.0, "60M", "USB", "F6", 5.357, false, 150,
			0.0, "60M", "USB", "F6", 5.3715, false, 150, 0.0, "60M", "USB",
			"F6", 5.4035, false, 150, 0.0, "40M", "CWL", "F1", 7.001, false,
			150, 0.0, "40M", "CWU", "F1", 7.005, false, 150, 0.0, "40M",
			"LSB", "F6", 7.09, false, 150, 0.0, "40M", "LSB", "F6", 7.105,
			false, 150, 0.0, "40M", "LSB", "F6", 7.19, false, 150, 0.0,
			"30M", "CWU", "F1", 10.105, false, 150, 0.0, "30M", "CWU", "F1",
			10.11, false, 150, 0.0, "30M", "CWU", "F1", 10.12, false, 150,
			0.0, "30M", "CWU", "F1", 10.13, false, 150, 0.0, "30M", "CWU",
			"F5", 10.14, false, 150, 0.0, "20M", "CWU", "F1", 14.005, false,
			150, 0.0, "20M", "CWU", "F1", 14.01, false, 150, 0.0, "20M",
			"USB", "F6", 14.136, false, 150, 0.0, "20M", "USB", "F6", 14.23,
			false, 150, 0.0, "20M", "USB", "F6", 14.336, false, 150, 0.0,
			"17M", "CWU", "F1", 18.09, false, 150, 0.0, "17M", "CWU", "F1",
			18.095, false, 150, 0.0, "17M", "USB", "F6", 18.125, false, 150,
			0.0, "17M", "USB", "F6", 18.14, false, 150, 0.0, "17M", "USB",
			"F6", 18.145, false, 150, 0.0, "15M", "CWU", "F1", 21.001, false,
			150, 0.0, "15M", "CWU", "F1", 21.005, false, 150, 0.0, "15M",
			"USB", "F6", 21.255, false, 150, 0.0, "15M", "USB", "F6", 21.27,
			false, 150, 0.0, "15M", "USB", "F6", 21.3, false, 150, 0.0,
			"12M", "CWU", "F1", 24.895, false, 150, 0.0, "12M", "CWU", "F1",
			24.897, false, 150, 0.0, "12M", "USB", "F6", 24.9, false, 150,
			0.0, "12M", "USB", "F6", 24.935, false, 150, 0.0, "12M", "USB",
			"F6", 24.945, false, 150, 0.0, "10M", "CWU", "F1", 28.005, false,
			150, 0.0, "10M", "CWU", "F1", 28.01, false, 150, 0.0, "10M",
			"USB", "F6", 28.3, false, 150, 0.0, "10M", "USB", "F6", 28.4,
			false, 150, 0.0, "10M", "USB", "F6", 28.45, false, 150, 0.0,
			"6M", "CWU", "F1", 50.01, false, 150, 0.0, "6M", "CWU", "F1",
			50.015, false, 150, 0.0, "6M", "USB", "F6", 50.125, false, 150,
			0.0, "6M", "USB", "F6", 50.18, false, 150, 0.0, "6M", "USB",
			"F6", 50.19, false, 150, 0.0, "2M", "CWU", "F1", 144.01, false,
			150, 0.0, "2M", "CWU", "F1", 144.015, false, 150, 0.0, "2M",
			"USB", "F6", 144.2, false, 150, 0.0, "2M", "USB", "F6", 144.21,
			false, 150, 0.0, "2M", "USB", "F6", 144.215, false, 150, 0.0,
			"WWV", "SAM", "F7", 2.5, false, 150, 0.0, "WWV", "SAM", "F7",
			5.0, false, 150, 0.0, "WWV", "SAM", "F7", 10.0, false, 150,
			0.0, "WWV", "SAM", "F7", 15.0, false, 150, 0.0, "WWV", "SAM",
			"F7", 20.0, false, 150, 0.0, "GEN", "SAM", "F5", 13.845, false,
			150, 0.0, "GEN", "SAM", "F5", 9.55, false, 150, 0.0, "GEN",
			"SAM", "F5", 5.975, false, 150, 0.0, "GEN", "SAM", "F5", 3.25,
			false, 150, 0.0, "GEN", "SAM", "F4", 0.59, false, 150, 0.0
		});
	}
}
