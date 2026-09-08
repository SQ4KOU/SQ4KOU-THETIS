using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Thetis.Properties;

namespace Thetis;

internal static class CountryData
{
	[Serializable]
	public class PrefixData
	{
		public string Country { get; set; }

		public string CountryCode { get; set; }

		public string AssetCode { get; set; }

		public List<string> Prefixes { get; set; } = new List<string>();

		public int ADIF { get; set; }

		public int CQZone { get; set; }

		public int ITUZone { get; set; }

		public string Continent { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public double GMTOffset { get; set; }

		public bool ExactCallsign { get; set; }
	}

	private static List<PrefixData> _prefixDataList;

	private static readonly Dictionary<string, string> _countryCodeAliasMap;

	private static readonly Dictionary<string, string> _regionCountryCodeMap;

	private static readonly Dictionary<int, string> _adifCountryCodeMap;

	private static readonly Dictionary<string, string> _assetCodeAliasMap;

	private static readonly Dictionary<int, string> _adifAssetCodeMap;

	static CountryData()
	{
		_countryCodeAliasMap = createCountryCodeAliasMap();
		_regionCountryCodeMap = createRegionCountryCodeMap();
		_adifCountryCodeMap = createAdifCountryCodeMap();
		_assetCodeAliasMap = createAssetCodeAliasMap();
		_adifAssetCodeMap = createAdifAssetCodeMap();
		try
		{
			_prefixDataList = Common.DeserializeFromBase64<List<PrefixData>>(Resources.cty);
			if (_prefixDataList == null)
			{
				return;
			}
			foreach (PrefixData prefixData in _prefixDataList)
			{
				prefixData.CountryCode = getCountryCode(prefixData.Country, prefixData.ADIF);
				prefixData.AssetCode = getAssetCode(prefixData.Country, prefixData.ADIF, prefixData.CountryCode);
			}
		}
		catch
		{
			_prefixDataList = null;
		}
	}

	public static PrefixData GetCallsignData(string callsign)
	{
		if (_prefixDataList == null)
		{
			return null;
		}
		if (string.IsNullOrWhiteSpace(callsign))
		{
			return null;
		}
		callsign = callsign.Trim();
		PrefixData result = null;
		int num = 0;
		foreach (PrefixData prefixData in _prefixDataList)
		{
			foreach (string item in prefixData.Prefixes.OrderByDescending((string p) => p.Length))
			{
				if (callsign.StartsWith(item, StringComparison.OrdinalIgnoreCase))
				{
					if (item.Length > num)
					{
						num = item.Length;
						result = prefixData;
					}
					break;
				}
			}
		}
		return result;
	}

	private static void LoadPrefixes(string filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(filePath);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("//dict");
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (!(childNode.Name == "key"))
			{
				continue;
			}
			string innerText = childNode.InnerText;
			XmlNode nextSibling = childNode.NextSibling;
			if (nextSibling == null || !(nextSibling.Name == "dict"))
			{
				continue;
			}
			string countryName = null;
			int num = 0;
			int cQZone = 0;
			int iTUZone = 0;
			string continent = null;
			double latitude = 0.0;
			double longitude = 0.0;
			double gMTOffset = 0.0;
			bool exactCallsign = false;
			List<string> list = new List<string> { innerText };
			foreach (XmlNode childNode2 in nextSibling.ChildNodes)
			{
				if (childNode2.Name == "key" && childNode2.InnerText == "Country")
				{
					countryName = ((childNode2.NextSibling == null) ? null : childNode2.NextSibling.InnerText);
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "Prefix")
				{
					string text = ((childNode2.NextSibling == null) ? null : childNode2.NextSibling.InnerText);
					if (text != null)
					{
						list.Add(text);
					}
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "ADIF")
				{
					num = int.Parse((childNode2.NextSibling == null) ? "0" : childNode2.NextSibling.InnerText);
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "CQZone")
				{
					cQZone = int.Parse((childNode2.NextSibling == null) ? "0" : childNode2.NextSibling.InnerText);
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "ITUZone")
				{
					iTUZone = int.Parse((childNode2.NextSibling == null) ? "0" : childNode2.NextSibling.InnerText);
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "Continent")
				{
					continent = ((childNode2.NextSibling == null) ? null : childNode2.NextSibling.InnerText);
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "Latitude")
				{
					latitude = double.Parse((childNode2.NextSibling == null) ? "0" : childNode2.NextSibling.InnerText, CultureInfo.InvariantCulture);
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "Longitude")
				{
					longitude = double.Parse((childNode2.NextSibling == null) ? "0" : childNode2.NextSibling.InnerText, CultureInfo.InvariantCulture);
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "GMTOffset")
				{
					gMTOffset = double.Parse((childNode2.NextSibling == null) ? "0" : childNode2.NextSibling.InnerText, CultureInfo.InvariantCulture);
				}
				else if (childNode2.Name == "key" && childNode2.InnerText == "ExactCallsign")
				{
					exactCallsign = childNode2.NextSibling != null && childNode2.NextSibling.InnerText == "true";
				}
			}
			if (countryName == null)
			{
				continue;
			}
			PrefixData prefixData = _prefixDataList.FirstOrDefault((PrefixData pd) => pd.Country == countryName);
			if (prefixData == null)
			{
				string countryCode = getCountryCode(countryName, num);
				PrefixData item = new PrefixData
				{
					Country = countryName,
					CountryCode = countryCode,
					AssetCode = getAssetCode(countryName, num, countryCode),
					Prefixes = list,
					ADIF = num,
					CQZone = cQZone,
					ITUZone = iTUZone,
					Continent = continent,
					Latitude = latitude,
					Longitude = longitude,
					GMTOffset = gMTOffset,
					ExactCallsign = exactCallsign
				};
				_prefixDataList.Add(item);
			}
			else
			{
				prefixData.Prefixes.AddRange(list);
				if (string.IsNullOrWhiteSpace(prefixData.CountryCode))
				{
					prefixData.CountryCode = getCountryCode(countryName, num);
				}
				if (string.IsNullOrWhiteSpace(prefixData.AssetCode))
				{
					prefixData.AssetCode = getAssetCode(countryName, num, prefixData.CountryCode);
				}
			}
		}
		foreach (PrefixData prefixData2 in _prefixDataList)
		{
			prefixData2.Prefixes = prefixData2.Prefixes.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy((string p) => p, StringComparer.OrdinalIgnoreCase).ToList();
		}
	}

	private static string getCountryCode(string country, int adif)
	{
		string key = normalizeCountryName(country);
		if (_countryCodeAliasMap.TryGetValue(key, out var value))
		{
			return value;
		}
		if (_regionCountryCodeMap.TryGetValue(key, out value))
		{
			return value;
		}
		if (_adifCountryCodeMap.TryGetValue(adif, out value))
		{
			return value;
		}
		return string.Empty;
	}

	private static string getAssetCode(string country, int adif, string countryCode)
	{
		string key = normalizeCountryName(country);
		if (_assetCodeAliasMap.TryGetValue(key, out var value))
		{
			return value;
		}
		if (_adifAssetCodeMap.TryGetValue(adif, out value))
		{
			return value;
		}
		return countryCode ?? string.Empty;
	}

	private static Dictionary<int, string> createAdifCountryCodeMap()
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		addCountryCode(dictionary, 6, "US");
		addCountryCode(dictionary, 10, "TF");
		addCountryCode(dictionary, 13, "AQ");
		addCountryCode(dictionary, 15, "RU");
		addCountryCode(dictionary, 16, "NZ");
		addCountryCode(dictionary, 17, "VE");
		addCountryCode(dictionary, 20, "UM");
		addCountryCode(dictionary, 21, "ES");
		addCountryCode(dictionary, 24, "BV");
		addCountryCode(dictionary, 31, "KI");
		addCountryCode(dictionary, 33, "IO");
		addCountryCode(dictionary, 34, "NZ");
		addCountryCode(dictionary, 36, "FR");
		addCountryCode(dictionary, 37, "CR");
		addCountryCode(dictionary, 41, "TF");
		addCountryCode(dictionary, 46, "MY");
		addCountryCode(dictionary, 47, "CL");
		addCountryCode(dictionary, 48, "KI");
		addCountryCode(dictionary, 54, "RU");
		addCountryCode(dictionary, 67, "US");
		addCountryCode(dictionary, 71, "EC");
		addCountryCode(dictionary, 98, "VC");
		addCountryCode(dictionary, 99, "TF");
		addCountryCode(dictionary, 110, "US");
		addCountryCode(dictionary, 111, "HM");
		addCountryCode(dictionary, 117, "UN");
		addCountryCode(dictionary, 123, "UM");
		addCountryCode(dictionary, 124, "TF");
		addCountryCode(dictionary, 126, "RU");
		addCountryCode(dictionary, 131, "TF");
		addCountryCode(dictionary, 133, "NZ");
		addCountryCode(dictionary, 137, "KR");
		addCountryCode(dictionary, 142, "IN");
		addCountryCode(dictionary, 149, "PT");
		addCountryCode(dictionary, 152, "MO");
		addCountryCode(dictionary, 153, "AU");
		addCountryCode(dictionary, 161, "CO");
		addCountryCode(dictionary, 165, "MU");
		addCountryCode(dictionary, 166, "MP");
		addCountryCode(dictionary, 172, "PN");
		addCountryCode(dictionary, 174, "UM");
		addCountryCode(dictionary, 191, "CK");
		addCountryCode(dictionary, 195, "GQ");
		addCountryCode(dictionary, 199, "BV");
		addCountryCode(dictionary, 201, "ZA");
		addCountryCode(dictionary, 204, "MX");
		addCountryCode(dictionary, 205, "SH");
		addCountryCode(dictionary, 206, "UN");
		addCountryCode(dictionary, 207, "MU");
		addCountryCode(dictionary, 215, "CY");
		addCountryCode(dictionary, 217, "CL");
		addCountryCode(dictionary, 230, "DE");
		addCountryCode(dictionary, 234, "CK");
		addCountryCode(dictionary, 235, "GS");
		addCountryCode(dictionary, 238, "AQ");
		addCountryCode(dictionary, 240, "GS");
		addCountryCode(dictionary, 241, "AQ");
		addCountryCode(dictionary, 246, "MT");
		addCountryCode(dictionary, 247, string.Empty);
		addCountryCode(dictionary, 248, "IT");
		addCountryCode(dictionary, 250, "SH");
		addCountryCode(dictionary, 253, "BR");
		addCountryCode(dictionary, 256, "PT");
		addCountryCode(dictionary, 270, "TK");
		addCountryCode(dictionary, 272, "PT");
		addCountryCode(dictionary, 273, "BR");
		addCountryCode(dictionary, 274, "SH");
		addCountryCode(dictionary, 276, "TF");
		addCountryCode(dictionary, 283, "GB");
		addCountryCode(dictionary, 285, "VI");
		addCountryCode(dictionary, 289, "UN");
		addCountryCode(dictionary, 299, "MY");
		addCountryCode(dictionary, 301, "KI");
		addCountryCode(dictionary, 302, "EH");
		addCountryCode(dictionary, 318, "CN");
		addCountryCode(dictionary, 321, "HK");
		addCountryCode(dictionary, 339, "JP");
		addCountryCode(dictionary, 344, "KP");
		addCountryCode(dictionary, 345, "BN");
		addCountryCode(dictionary, 386, "TW");
		addCountryCode(dictionary, 414, "CD");
		addCountryCode(dictionary, 453, "RE");
		addCountryCode(dictionary, 468, "SZ");
		addCountryCode(dictionary, 489, "FJ");
		addCountryCode(dictionary, 490, "KI");
		addCountryCode(dictionary, 504, "SK");
		addCountryCode(dictionary, 507, "SB");
		addCountryCode(dictionary, 508, "PF");
		addCountryCode(dictionary, 509, "PF");
		addCountryCode(dictionary, 512, "NC");
		addCountryCode(dictionary, 513, "PN");
		addCountryCode(dictionary, 515, "AS");
		addCountryCode(dictionary, 519, "BQ");
		addCountryCode(dictionary, 520, "BQ");
		addCountryCode(dictionary, 521, "SS");
		addCountryCode(dictionary, 522, "XK");
		return dictionary;
	}

	private static Dictionary<int, string> createAdifAssetCodeMap()
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		addCountryCode(dictionary, 205, "SH-AC");
		addCountryCode(dictionary, 250, "SH-HL");
		addCountryCode(dictionary, 274, "SH-TA");
		return dictionary;
	}

	private static Dictionary<string, string> createCountryCodeAliasMap()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		addAlias(dictionary, "Aland Islands", "AX");
		addAlias(dictionary, "Agalega and Saint Brandon", "MU");
		addAlias(dictionary, "Agaléga and Saint Brandon", "MU");
		addAlias(dictionary, "Alaska", "US");
		addAlias(dictionary, "Andaman and Nicobar Islands", "IN");
		addAlias(dictionary, "Annobon", "GQ");
		addAlias(dictionary, "Annobon Island", "GQ");
		addAlias(dictionary, "Annobón", "GQ");
		addAlias(dictionary, "Asiatic Russia", "RU");
		addAlias(dictionary, "Asiatic Turkey", "TR");
		addAlias(dictionary, "Aves Island", "VE");
		addAlias(dictionary, "Azores", "PT");
		addAlias(dictionary, "Balearic Islands", "ES");
		addAlias(dictionary, "Bear Island", "SJ");
		addAlias(dictionary, "Bouvet", "BV");
		addAlias(dictionary, "Bouvet Island", "BV");
		addAlias(dictionary, "Canary Islands", "ES");
		addAlias(dictionary, "Ceuta and Melilla", "ES");
		addAlias(dictionary, "Ceuta & Melilla", "ES");
		addAlias(dictionary, "Corsica", "FR");
		addAlias(dictionary, "Crete", "GR");
		addAlias(dictionary, "Culebra and Vieques", "PR");
		addAlias(dictionary, "Culebra & Vieques", "PR");
		addAlias(dictionary, "Desecheo Island", "PR");
		addAlias(dictionary, "Dodecanese", "GR");
		addAlias(dictionary, "East Malaysia", "MY");
		addAlias(dictionary, "Easter Island", "CL");
		addAlias(dictionary, "England", "GB");
		addAlias(dictionary, "European Russia", "RU");
		addAlias(dictionary, "European Turkey", "TR");
		addAlias(dictionary, "Fernando de Noronha", "BR");
		addAlias(dictionary, "Franz Josef Land", "RU");
		addAlias(dictionary, "Guantanamo Bay", "CU");
		addAlias(dictionary, "Hawaii", "US");
		addAlias(dictionary, "Howland and Baker Islands", "UM");
		addAlias(dictionary, "Howland & Baker Islands", "UM");
		addAlias(dictionary, "International Telecommunication Union Headquarters", "UN");
		addAlias(dictionary, "Isla de Aves", "VE");
		addAlias(dictionary, "Jan Mayen", "SJ");
		addAlias(dictionary, "Juan Fernandez Islands", "CL");
		addAlias(dictionary, "Juan Fernández Islands", "CL");
		addAlias(dictionary, "Kaliningrad", "RU");
		addAlias(dictionary, "Kure Island", "UM");
		addAlias(dictionary, "Lakshadweep Islands", "IN");
		addAlias(dictionary, "Lord Howe Island", "AU");
		addAlias(dictionary, "Madeira", "PT");
		addAlias(dictionary, "Malyj Vysotskij Island", "RU");
		addAlias(dictionary, "Market Reef", "AX");
		addAlias(dictionary, "Mellish Reef", "AU");
		addAlias(dictionary, "Minami Torishima", "JP");
		addAlias(dictionary, "Mona Island", "PR");
		addAlias(dictionary, "Mount Athos", "GR");
		addAlias(dictionary, "Navassa Island", "UM");
		addAlias(dictionary, "New Zealand Subantarctic Islands", "NZ");
		addAlias(dictionary, "North Cook Islands", "CK");
		addAlias(dictionary, "Northern Ireland", "GB");
		addAlias(dictionary, "Ogasawara", "JP");
		addAlias(dictionary, "Ogasawara Islands", "JP");
		addAlias(dictionary, "Okinawa", "JP");
		addAlias(dictionary, "Palmyra and Jarvis Islands", "UM");
		addAlias(dictionary, "Palmyra & Jarvis Islands", "UM");
		addAlias(dictionary, "Peter I Island", "BV");
		addAlias(dictionary, "Pratas Island", "TW");
		addAlias(dictionary, "Rodrigues Island", "MU");
		addAlias(dictionary, "Rodriguez Island", "MU");
		addAlias(dictionary, "Rotuma Island", "FJ");
		addAlias(dictionary, "Sable Island", "CA");
		addAlias(dictionary, "San Andres and Providencia", "CO");
		addAlias(dictionary, "San Andres & Providencia", "CO");
		addAlias(dictionary, "Sardinia", "IT");
		addAlias(dictionary, "Scarborough Reef", "CN");
		addAlias(dictionary, "Scarborough Shoal", "CN");
		addAlias(dictionary, "Scotland", "GB");
		addAlias(dictionary, "Shetland Islands", "GB");
		addAlias(dictionary, "Sicily", "IT");
		addAlias(dictionary, "South Cook Islands", "CK");
		addAlias(dictionary, "Spratly Islands", string.Empty);
		addAlias(dictionary, "St. Paul Island", "CA");
		addAlias(dictionary, "St Paul Island", "CA");
		addAlias(dictionary, "Svalbard", "SJ");
		addAlias(dictionary, "United Nations Headquarters", "UN");
		addAlias(dictionary, "Wake Island", "UM");
		addAlias(dictionary, "Wales", "GB");
		addAlias(dictionary, "West Malaysia", "MY");
		addAlias(dictionary, "Willis Island", "AU");
		return dictionary;
	}

	private static Dictionary<string, string> createAssetCodeAliasMap()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		addAlias(dictionary, "Arab League", "ARAB");
		addAlias(dictionary, "League of Arab States", "ARAB");
		addAlias(dictionary, "Association of Southeast Asian Nations", "ASEAN");
		addAlias(dictionary, "ASEAN", "ASEAN");
		addAlias(dictionary, "Central European Free Trade Agreement", "CEFTA");
		addAlias(dictionary, "CEFTA", "CEFTA");
		addAlias(dictionary, "East African Community", "EAC");
		addAlias(dictionary, "EAC", "EAC");
		addAlias(dictionary, "England", "GB-ENG");
		addAlias(dictionary, "Northern Ireland", "GB-NIR");
		addAlias(dictionary, "Scotland", "GB-SCT");
		addAlias(dictionary, "Wales", "GB-WLS");
		addAlias(dictionary, "Catalonia", "ES-CT");
		addAlias(dictionary, "Catalunya", "ES-CT");
		addAlias(dictionary, "Galicia", "ES-GA");
		addAlias(dictionary, "Basque Country", "ES-PV");
		addAlias(dictionary, "Pais Vasco", "ES-PV");
		addAlias(dictionary, "País Vasco", "ES-PV");
		addAlias(dictionary, "Euskadi", "ES-PV");
		addAlias(dictionary, "Ascension", "SH-AC");
		addAlias(dictionary, "Ascension Island", "SH-AC");
		addAlias(dictionary, "Saint Helena", "SH-HL");
		addAlias(dictionary, "St Helena", "SH-HL");
		addAlias(dictionary, "St. Helena", "SH-HL");
		addAlias(dictionary, "Tristan da Cunha", "SH-TA");
		return dictionary;
	}

	private static Dictionary<string, string> createRegionCountryCodeMap()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
		foreach (CultureInfo cultureInfo in cultures)
		{
			try
			{
				RegionInfo regionInfo = new RegionInfo(cultureInfo.Name);
				addAlias(dictionary, regionInfo.EnglishName, regionInfo.TwoLetterISORegionName);
				addAlias(dictionary, regionInfo.NativeName, regionInfo.TwoLetterISORegionName);
			}
			catch
			{
			}
		}
		addAlias(dictionary, "The Gambia", "GM");
		addAlias(dictionary, "Czech Republic", "CZ");
		addAlias(dictionary, "Viet Nam", "VN");
		addAlias(dictionary, "Bosnia-Herzegovina", "BA");
		addAlias(dictionary, "Brunei", "BN");
		addAlias(dictionary, "Cape Verde", "CV");
		addAlias(dictionary, "Curacao", "CW");
		addAlias(dictionary, "Curaçao", "CW");
		addAlias(dictionary, "Democratic Republic of the Congo", "CD");
		addAlias(dictionary, "East Timor", "TL");
		addAlias(dictionary, "Eswatini", "SZ");
		addAlias(dictionary, "Iran", "IR");
		addAlias(dictionary, "Laos", "LA");
		addAlias(dictionary, "Micronesia", "FM");
		addAlias(dictionary, "Moldova", "MD");
		addAlias(dictionary, "North Macedonia", "MK");
		addAlias(dictionary, "Palestine", "PS");
		addAlias(dictionary, "Republic of the Congo", "CG");
		addAlias(dictionary, "Russia", "RU");
		addAlias(dictionary, "South Korea", "KR");
		addAlias(dictionary, "North Korea", "KP");
		addAlias(dictionary, "Syria", "SY");
		addAlias(dictionary, "Taiwan", "TW");
		addAlias(dictionary, "Tanzania", "TZ");
		addAlias(dictionary, "United States", "US");
		addAlias(dictionary, "Venezuela", "VE");
		addAlias(dictionary, "Antarctica", "AQ");
		addAlias(dictionary, "Bonaire", "BQ");
		addAlias(dictionary, "Brunei Darussalam", "BN");
		addAlias(dictionary, "Hong Kong", "HK");
		addAlias(dictionary, "Macao", "MO");
		addAlias(dictionary, "Republic of Kosovo", "XK");
		addAlias(dictionary, "Republic of Korea", "KR");
		addAlias(dictionary, "Republic of South Sudan", "SS");
		addAlias(dictionary, "Reunion Island", "RE");
		addAlias(dictionary, "Slovak Republic", "SK");
		addAlias(dictionary, "Vienna Intl Ctr", "UN");
		addAlias(dictionary, "Wallis & Futuna Islands", "WF");
		addAlias(dictionary, "Western Sahara", "EH");
		return dictionary;
	}

	private static void addCountryCode(Dictionary<int, string> map, int adif, string code)
	{
		if (!map.ContainsKey(adif))
		{
			map.Add(adif, code);
		}
		else
		{
			map[adif] = code;
		}
	}

	private static void addAlias(Dictionary<string, string> map, string name, string code)
	{
		string text = normalizeCountryName(name);
		if (text.Length != 0)
		{
			if (!map.ContainsKey(text))
			{
				map.Add(text, code);
			}
			else
			{
				map[text] = code;
			}
		}
	}

	private static string normalizeCountryName(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}
		string text = value.Trim().Normalize(NormalizationForm.FormD);
		StringBuilder stringBuilder = new StringBuilder(text.Length + 8);
		foreach (char c in text)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
			{
				if (c == '&')
				{
					stringBuilder.Append(" AND ");
				}
				else if (char.IsLetterOrDigit(c))
				{
					stringBuilder.Append(char.ToUpperInvariant(c));
				}
				else
				{
					stringBuilder.Append(' ');
				}
			}
		}
		string text2 = stringBuilder.ToString();
		while (text2.Contains("  "))
		{
			text2 = text2.Replace("  ", " ");
		}
		text2 = " " + text2.Trim() + " ";
		text2 = text2.Replace(" N Z ", " NEW ZEALAND ");
		text2 = text2.Replace(" ST ", " SAINT ");
		text2 = text2.Replace(" IS ", " ISLANDS ");
		text2 = text2.Replace(" INTL ", " INTERNATIONAL ");
		text2 = text2.Replace(" CTR ", " CENTRE ");
		while (text2.Contains("  "))
		{
			text2 = text2.Replace("  ", " ");
		}
		return text2.Trim();
	}
}
