using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Timers;
using Thetis.Properties;

namespace Thetis;

internal static class SpotManager2
{
	public class JsonSpotData
	{
		public string Spotter { get; set; } = "";

		public string Comment { get; set; } = "";

		public int Heading { get; set; } = -1;

		public string Continent { get; set; } = "";

		public string Country { get; set; } = "";

		public string UtcTime { get; set; } = "";

		public string TextColor { get; set; } = "";

		public string Flag { get; set; } = "";

		public string FlagSpotter { get; set; } = "";

		public long Distance { get; set; } = -1L;

		public bool IsSWL { get; set; }

		public long SWLSecondsToLive { get; set; }
	}

	public class smSpot
	{
		public string callsign;

		public DSPMode mode;

		public long frequencyHZ;

		public Color colour;

		public DateTime timeAdded;

		public string additionalText;

		public string spotter;

		public int heading;

		public string continent;

		public string country;

		public DateTime utc_spot_time;

		public bool IsSWL;

		public long SWLSecondsToLive;

		public long distance;

		public bool previously_highlighted;

		public bool flashing;

		public DateTime flash_start_time;

		public bool[] Visible;

		public SizeF Size;

		public Rectangle[] BoundingBoxInPixels;

		public bool[] Highlight;

		public Color text_colour;

		public bool use_text_colour;

		public string cached_display_text;

		public int colour_luminance;

		public bool spot_flag_in_use;

		public bool spotter_flag_in_use;

		public Image flag;

		public Image flag_spotter;

		public smSpot()
		{
			DateTime utcNow = DateTime.UtcNow;
			callsign = "";
			mode = DSPMode.FIRST;
			frequencyHZ = 0L;
			colour = Color.White;
			timeAdded = utcNow;
			additionalText = "";
			spotter = "";
			heading = -1;
			continent = "";
			country = "";
			flag = null;
			flag_spotter = null;
			utc_spot_time = utcNow;
			IsSWL = false;
			SWLSecondsToLive = 0L;
			previously_highlighted = false;
			flashing = false;
			flash_start_time = utcNow;
			text_colour = Color.Empty;
			use_text_colour = false;
			cached_display_text = null;
			colour_luminance = Common.GetLuminance(colour);
		}

		public void BrowseQRZ()
		{
			Common.OpenUri("https://www.qrz.com/db/" + callsign.ToUpper().Trim());
		}

		public void BrowseHamQTH()
		{
			Common.OpenUri("https://www.hamqth.com/" + callsign.ToUpper().Trim());
		}
	}

	private const int MAX_RX = 2;

	private static readonly List<smSpot> _spots;

	private static readonly object _objLock;

	private static smSpot[] _sortedSpotsCache;

	private static bool _sortedSpotsDirty;

	private static readonly smSpot[] _highlightedSpots;

	private static int _lifeTime;

	private static int _maxNumber;

	private static Timer _tickTimer;

	private static ConcurrentDictionary<string, Image> _flag_images;

	private static bool _replaceOwnCallAppearance;

	private static string _replaceCall;

	private static Color _replaceBackgroundColour;

	public static int LifeTime
	{
		get
		{
			return _lifeTime;
		}
		set
		{
			_lifeTime = value;
		}
	}

	public static int MaxNumber
	{
		get
		{
			return _maxNumber;
		}
		set
		{
			_maxNumber = value;
		}
	}

	public static bool HasSpots
	{
		get
		{
			lock (_objLock)
			{
				return _spots.Count > 0;
			}
		}
	}

	static SpotManager2()
	{
		_spots = new List<smSpot>();
		_objLock = new object();
		_sortedSpotsCache = Array.Empty<smSpot>();
		_sortedSpotsDirty = true;
		_highlightedSpots = new smSpot[2];
		_lifeTime = 60;
		_maxNumber = 100;
		_replaceOwnCallAppearance = false;
		_replaceCall = "";
		_replaceBackgroundColour = Color.DarkGray;
		_flag_images = new ConcurrentDictionary<string, Image>();
		FlagAtlas.Init(Resources.flagatlas_image, Resources.flagatlas_json);
		_tickTimer = new Timer(1000.0);
		_tickTimer.Elapsed += onTick;
		_tickTimer.AutoReset = true;
		_tickTimer.Enabled = true;
	}

	public static void FreeUpFlags()
	{
		if (_flag_images != null)
		{
			while (_flag_images.Count > 0)
			{
				_flag_images.TryRemove(_flag_images.First().Key, out var value);
				value?.Dispose();
			}
		}
	}

	private static int compareByFrequency(smSpot left, smSpot right)
	{
		return left.frequencyHZ.CompareTo(right.frequencyHZ);
	}

	private static void markSortedSpotsDirty()
	{
		_sortedSpotsDirty = true;
	}

	private static void RebuildSortedSpotsCache()
	{
		if (_sortedSpotsDirty)
		{
			if (_spots.Count == 0)
			{
				_sortedSpotsCache = Array.Empty<smSpot>();
				_sortedSpotsDirty = false;
				return;
			}
			smSpot[] array = _spots.ToArray();
			Array.Sort(array, compareByFrequency);
			_sortedSpotsCache = array;
			_sortedSpotsDirty = false;
		}
	}

	private static void clearHighlightedReference(smSpot spot)
	{
		if (spot == null)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (_highlightedSpots[i] == spot)
			{
				spot.Highlight[i] = false;
				_highlightedSpots[i] = null;
			}
		}
	}

	private static void pruneHighlightedReferences()
	{
		for (int i = 0; i < 2; i++)
		{
			smSpot smSpot2 = _highlightedSpots[i];
			if (smSpot2 != null && !_spots.Contains(smSpot2))
			{
				smSpot2.Highlight[i] = false;
				_highlightedSpots[i] = null;
			}
		}
	}

	private static void onTick(object source, ElapsedEventArgs e)
	{
		lock (_objLock)
		{
			DateTime utcNow = DateTime.UtcNow;
			if (_spots.RemoveAll((smSpot o) => !o.IsSWL && (utcNow - o.timeAdded).TotalMinutes > (double)_lifeTime) + _spots.RemoveAll((smSpot o) => o.IsSWL && o.SWLSecondsToLive != 0L && utcNow > o.timeAdded + TimeSpan.FromSeconds(o.SWLSecondsToLive)) > 0)
			{
				markSortedSpotsDirty();
				pruneHighlightedReferences();
			}
		}
	}

	public static smSpot HighlightSpot(int x, int y)
	{
		lock (_objLock)
		{
			smSpot smSpot2 = null;
			int num = -1;
			for (int i = 0; i < 2; i++)
			{
				if (smSpot2 != null)
				{
					break;
				}
				for (int j = 0; j < _spots.Count; j++)
				{
					smSpot smSpot3 = _spots[j];
					if (smSpot3.Visible[i] && smSpot3.BoundingBoxInPixels[i].Contains(x, y))
					{
						smSpot2 = smSpot3;
						num = i;
						break;
					}
				}
			}
			for (int k = 0; k < 2; k++)
			{
				smSpot smSpot4 = _highlightedSpots[k];
				if (smSpot4 != null && (smSpot4 != smSpot2 || k != num))
				{
					smSpot4.Highlight[k] = false;
					_highlightedSpots[k] = null;
				}
			}
			if (smSpot2 != null)
			{
				smSpot2.Highlight[num] = true;
				_highlightedSpots[num] = smSpot2;
			}
			return smSpot2;
		}
	}

	public static DSPMode SpotModeNumberToDSPMode(int number, double freq = -1.0)
	{
		DSPMode result = DSPMode.FIRST;
		bool flag = false;
		if (freq > -1.0)
		{
			flag = freq >= 10000000.0 || (freq >= 5300000.0 && freq < 5410000.0);
		}
		switch (number)
		{
		case 0:
			result = (flag ? DSPMode.USB : DSPMode.LSB);
			break;
		case 1:
			result = ((!flag) ? DSPMode.CWL : DSPMode.CWU);
			break;
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
			result = ((!flag) ? DSPMode.DIGL : DSPMode.DIGU);
			break;
		case 11:
			result = DSPMode.FM;
			break;
		case 12:
			result = DSPMode.DRM;
			break;
		case 13:
			result = ((!flag) ? DSPMode.DIGL : DSPMode.DIGU);
			break;
		case 14:
			result = ((!flag) ? DSPMode.AM_LSB : DSPMode.AM_USB);
			break;
		}
		return result;
	}

	public static string FilterForRawMode(string raw_mode)
	{
		if (string.IsNullOrEmpty(raw_mode))
		{
			return "";
		}
		int num = raw_mode.IndexOf("-swl[", StringComparison.OrdinalIgnoreCase);
		if (num == -1)
		{
			num = raw_mode.IndexOf("swl[", StringComparison.OrdinalIgnoreCase);
		}
		if (num != -1)
		{
			return raw_mode.Substring(0, num);
		}
		return raw_mode;
	}

	private static Color getSpotTextColour(string text_colour)
	{
		if (string.IsNullOrEmpty(text_colour))
		{
			return Color.Empty;
		}
		string text = text_colour.Trim();
		if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return Color.FromArgb(result);
		}
		if (text.Length > 0 && text[0] == '#')
		{
			text = text.Substring(1);
		}
		if (text.Length == 6)
		{
			if (int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result2))
			{
				int red = (result2 >> 16) & 0xFF;
				int green = (result2 >> 8) & 0xFF;
				int blue = result2 & 0xFF;
				return Color.FromArgb(255, red, green, blue);
			}
			return Color.Empty;
		}
		if (text.Length == 8)
		{
			if (int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result3))
			{
				int alpha = (result3 >> 24) & 0xFF;
				int red2 = (result3 >> 16) & 0xFF;
				int green2 = (result3 >> 8) & 0xFF;
				int blue2 = result3 & 0xFF;
				return Color.FromArgb(alpha, red2, green2, blue2);
			}
			return Color.Empty;
		}
		return Color.Empty;
	}

	private static Image getFlagImage(string flag)
	{
		if (string.IsNullOrWhiteSpace(flag))
		{
			return null;
		}
		flag = flag.Trim();
		if (flag.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
		{
			flag = flag.Substring(0, flag.Length - 4);
		}
		if (string.IsNullOrWhiteSpace(flag))
		{
			return null;
		}
		flag = flag.ToLowerInvariant();
		if (_flag_images.TryGetValue(flag, out var value))
		{
			return value;
		}
		try
		{
			value = FlagAtlas.GetFlag(flag);
			_flag_images[flag] = value;
			return value;
		}
		catch
		{
			return null;
		}
	}

	private static Image getFlagImageFromCallsign(string callsign, out string country)
	{
		country = null;
		if (string.IsNullOrWhiteSpace(callsign))
		{
			return null;
		}
		CountryData.PrefixData callsignData = CountryData.GetCallsignData(callsign);
		if (callsignData == null)
		{
			return null;
		}
		country = callsignData.Country;
		string assetCode = callsignData.AssetCode;
		if (string.IsNullOrWhiteSpace(assetCode))
		{
			return null;
		}
		return getFlagImage(assetCode);
	}

	public static void AddSpot(string callsign, DSPMode mode, long frequencyHz, Color colour, string additionalText, JsonSpotData jsonSpotData = null)
	{
		callsign = callsign.ToUpper().Trim();
		additionalText = additionalText.Trim();
		string text = "";
		string text2 = "";
		int heading = -1;
		string text3 = "";
		string continent = "";
		Color color = Color.Empty;
		bool use_text_colour = false;
		bool isSWL = false;
		long num = 0L;
		Image image = null;
		Image image2 = null;
		long distance = -1L;
		if (jsonSpotData != null)
		{
			text = jsonSpotData.UtcTime.Trim();
			text2 = jsonSpotData.Spotter.Trim();
			heading = jsonSpotData.Heading;
			text3 = jsonSpotData.Country.Trim();
			color = getSpotTextColour(jsonSpotData.TextColor);
			use_text_colour = color != Color.Empty;
			additionalText = jsonSpotData.Comment.Trim();
			image = getFlagImage(jsonSpotData.Flag);
			image2 = getFlagImage(jsonSpotData.FlagSpotter);
			isSWL = jsonSpotData.IsSWL;
			num = jsonSpotData.SWLSecondsToLive;
			if (num < 0)
			{
				num = 0L;
			}
			distance = jsonSpotData.Distance;
		}
		if (!string.IsNullOrEmpty(callsign) && !callsign.StartsWith("CQ_", StringComparison.OrdinalIgnoreCase))
		{
			string country = null;
			if (!string.IsNullOrEmpty(callsign) && image == null)
			{
				image = getFlagImageFromCallsign(callsign, out country);
			}
			if (string.IsNullOrEmpty(text3) && !string.IsNullOrEmpty(country))
			{
				text3 = country;
			}
			string text4 = (string.IsNullOrEmpty(text2) ? callsign : text2);
			if (!string.IsNullOrEmpty(text4) && image2 == null)
			{
				image2 = getFlagImageFromCallsign(text4, out var _);
			}
		}
		DateTime dateTime = DateTime.UtcNow;
		if (!string.IsNullOrEmpty(text) && DateTime.TryParseExact(text, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var result))
		{
			dateTime = result;
		}
		smSpot smSpot2 = new smSpot
		{
			callsign = callsign,
			mode = mode,
			frequencyHZ = frequencyHz,
			colour = colour,
			additionalText = additionalText,
			spotter = text2,
			heading = heading,
			continent = continent,
			country = text3,
			timeAdded = DateTime.UtcNow,
			utc_spot_time = dateTime,
			flag = image,
			flag_spotter = image2,
			IsSWL = isSWL,
			SWLSecondsToLive = num,
			previously_highlighted = false,
			flashing = ((DateTime.UtcNow - dateTime).TotalSeconds <= 120.0),
			text_colour = color,
			use_text_colour = use_text_colour,
			distance = distance
		};
		if (_replaceOwnCallAppearance && smSpot2.callsign == _replaceCall)
		{
			smSpot2.colour = _replaceBackgroundColour;
		}
		smSpot2.colour_luminance = Common.GetLuminance(smSpot2.colour);
		if (smSpot2.callsign.Length > 20)
		{
			smSpot2.callsign = smSpot2.callsign.Substring(0, 20);
		}
		if (smSpot2.spotter.Length > 20)
		{
			smSpot2.spotter = smSpot2.spotter.Substring(0, 20);
		}
		if (smSpot2.additionalText.Length > 30)
		{
			smSpot2.additionalText = smSpot2.additionalText.Substring(0, 30);
		}
		if (smSpot2.continent.Length > 30)
		{
			smSpot2.continent = smSpot2.continent.Substring(0, 30);
		}
		if (smSpot2.country.Length > 30)
		{
			smSpot2.country = smSpot2.country.Substring(0, 30);
		}
		if (smSpot2.heading < 0 || smSpot2.heading > 360)
		{
			smSpot2.heading = -1;
		}
		smSpot2.Highlight = new bool[2];
		smSpot2.BoundingBoxInPixels = new Rectangle[2];
		smSpot2.Visible = new bool[2];
		for (int i = 0; i < 2; i++)
		{
			smSpot2.Highlight[i] = false;
			smSpot2.BoundingBoxInPixels[i] = new Rectangle(-1, -1, 0, 0);
			smSpot2.Visible[i] = false;
		}
		lock (_objLock)
		{
			smSpot smSpot3 = null;
			for (int j = 0; j < _spots.Count; j++)
			{
				smSpot smSpot4 = _spots[j];
				if (string.Equals(smSpot4.callsign?.Trim(), smSpot2.callsign?.Trim(), StringComparison.OrdinalIgnoreCase) && Math.Abs(smSpot4.frequencyHZ - frequencyHz) <= 5000)
				{
					smSpot3 = smSpot4;
					break;
				}
			}
			if (smSpot3 != null)
			{
				smSpot2.flash_start_time = smSpot3.flash_start_time;
				smSpot2.flashing = smSpot3.flashing;
				if (smSpot2.mode == smSpot3.mode && Math.Abs(smSpot2.frequencyHZ - smSpot3.frequencyHZ) <= 5000 && smSpot2.colour == smSpot3.colour && smSpot2.heading == smSpot3.heading && string.Equals(smSpot2.additionalText?.Trim(), smSpot3.additionalText?.Trim(), StringComparison.OrdinalIgnoreCase) && string.Equals(smSpot2.spotter?.Trim(), smSpot3.spotter?.Trim(), StringComparison.OrdinalIgnoreCase) && string.Equals(smSpot2.continent?.Trim(), smSpot3.continent?.Trim(), StringComparison.OrdinalIgnoreCase) && string.Equals(smSpot2.country?.Trim(), smSpot3.country?.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					smSpot2.utc_spot_time = smSpot3.utc_spot_time;
				}
				clearHighlightedReference(smSpot3);
				_spots.Remove(smSpot3);
			}
			int num2 = 0;
			for (int k = 0; k < _spots.Count; k++)
			{
				if (!_spots[k].IsSWL)
				{
					num2++;
				}
			}
			if (!smSpot2.IsSWL && num2 >= _maxNumber)
			{
				for (int num3 = num2 - _maxNumber + 1; num3 > 0; num3--)
				{
					int num4 = -1;
					DateTime dateTime2 = DateTime.MaxValue;
					for (int l = 0; l < _spots.Count; l++)
					{
						smSpot smSpot5 = _spots[l];
						if (!smSpot5.IsSWL && !(smSpot5.timeAdded >= dateTime2))
						{
							dateTime2 = smSpot5.timeAdded;
							num4 = l;
						}
					}
					if (num4 < 0)
					{
						break;
					}
					clearHighlightedReference(_spots[num4]);
					_spots.RemoveAt(num4);
				}
			}
			_spots.Add(smSpot2);
			markSortedSpotsDirty();
		}
	}

	public static smSpot[] GetFrequencySortedSpots()
	{
		lock (_objLock)
		{
			RebuildSortedSpotsCache();
			return _sortedSpotsCache;
		}
	}

	public static void ClearAllSpots(bool non_swl, bool swl)
	{
		lock (_objLock)
		{
			bool flag = false;
			for (int num = _spots.Count - 1; num >= 0; num--)
			{
				smSpot smSpot2 = _spots[num];
				if ((non_swl && !smSpot2.IsSWL) || (swl && smSpot2.IsSWL))
				{
					clearHighlightedReference(smSpot2);
					_spots.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				markSortedSpotsDirty();
			}
		}
	}

	public static void DeleteSpot(string callsign)
	{
		lock (_objLock)
		{
			string text = callsign.ToUpper().Trim();
			bool flag = false;
			for (int num = _spots.Count - 1; num >= 0; num--)
			{
				smSpot smSpot2 = _spots[num];
				if (!(smSpot2.callsign != text))
				{
					clearHighlightedReference(smSpot2);
					_spots.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				markSortedSpotsDirty();
			}
		}
	}

	public static void OwnCallApearance(bool bEnabled, string sCall, Color replacementColorBackground)
	{
		_replaceOwnCallAppearance = bEnabled;
		_replaceCall = sCall.ToUpper().Trim();
		_replaceBackgroundColour = replacementColorBackground;
	}
}
