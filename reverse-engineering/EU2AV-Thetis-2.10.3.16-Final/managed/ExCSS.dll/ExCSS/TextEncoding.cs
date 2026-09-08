using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExCSS;

public static class TextEncoding
{
	public static HashSet<string> AvailableEncodings = new HashSet<string>(from encoding in Encoding.GetEncodings()
		select encoding.Name);

	public static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	public static readonly Encoding Utf16Be = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);

	public static readonly Encoding Utf16Le = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);

	public static readonly Encoding Utf32Le = GetEncoding("UTF-32LE");

	public static readonly Encoding Utf32Be = GetEncoding("UTF-32BE");

	public static readonly Encoding Gb18030 = GetEncoding("GB18030");

	public static readonly Encoding Big5 = GetEncoding("big5");

	public static readonly Encoding Windows874 = GetEncoding("windows-874");

	public static readonly Encoding Windows1250 = GetEncoding("windows-1250");

	public static readonly Encoding Windows1251 = GetEncoding("windows-1251");

	public static readonly Encoding Windows1252 = GetEncoding("windows-1252");

	public static readonly Encoding Windows1253 = GetEncoding("windows-1253");

	public static readonly Encoding Windows1254 = GetEncoding("windows-1254");

	public static readonly Encoding Windows1255 = GetEncoding("windows-1255");

	public static readonly Encoding Windows1256 = GetEncoding("windows-1256");

	public static readonly Encoding Windows1257 = GetEncoding("windows-1257");

	public static readonly Encoding Windows1258 = GetEncoding("windows-1258");

	public static readonly Encoding Latin2 = GetEncoding("iso-8859-2");

	public static readonly Encoding Latin3 = GetEncoding("iso-8859-3");

	public static readonly Encoding Latin4 = GetEncoding("iso-8859-4");

	public static readonly Encoding Latin5 = GetEncoding("iso-8859-5");

	public static readonly Encoding Latin13 = GetEncoding("iso-8859-13");

	public static readonly Encoding UsAscii = GetEncoding("us-ascii");

	public static readonly Encoding Korean = GetEncoding("ks_c_5601-1987");

	private static readonly Dictionary<string, Encoding> Encodings = CreateEncodings();

	public static bool IsUnicode(this Encoding encoding)
	{
		if (encoding != Utf16Be)
		{
			return encoding == Utf16Le;
		}
		return true;
	}

	public static bool IsSupported(string charset)
	{
		return Encodings.ContainsKey(charset);
	}

	public static Encoding Resolve(string charset)
	{
		if (charset != null && Encodings.TryGetValue(charset, out var value))
		{
			return value;
		}
		return Utf8;
	}

	private static Encoding GetEncoding(string name)
	{
		try
		{
			if (AvailableEncodings.Contains(name))
			{
				return Encoding.GetEncoding(name);
			}
		}
		catch
		{
		}
		return Utf8;
	}

	private static Dictionary<string, Encoding> CreateEncodings()
	{
		Dictionary<string, Encoding> obj = new Dictionary<string, Encoding>(StringComparer.OrdinalIgnoreCase)
		{
			{ "unicode-1-1-utf-8", Utf8 },
			{ "utf-8", Utf8 },
			{ "utf8", Utf8 },
			{ "utf-16be", Utf16Be },
			{ "utf-16", Utf16Le },
			{ "utf-16le", Utf16Le },
			{ "dos-874", Windows874 },
			{ "iso-8859-11", Windows874 },
			{ "iso8859-11", Windows874 },
			{ "iso885911", Windows874 },
			{ "tis-620", Windows874 },
			{ "windows-874", Windows874 },
			{ "cp1250", Windows1250 },
			{ "windows-1250", Windows1250 },
			{ "x-cp1250", Windows1250 },
			{ "cp1251", Windows1251 },
			{ "windows-1251", Windows1251 },
			{ "x-cp1251", Windows1251 },
			{ "x-user-defined", Windows1252 },
			{ "ansi_x3.4-1968", Windows1252 },
			{ "ascii", Windows1252 },
			{ "cp1252", Windows1252 },
			{ "cp819", Windows1252 },
			{ "csisolatin1", Windows1252 },
			{ "ibm819", Windows1252 },
			{ "iso-8859-1", Windows1252 },
			{ "iso-ir-100", Windows1252 },
			{ "iso8859-1", Windows1252 },
			{ "iso88591", Windows1252 },
			{ "iso_8859-1", Windows1252 },
			{ "iso_8859-1:1987", Windows1252 },
			{ "l1", Windows1252 },
			{ "latin1", Windows1252 },
			{ "us-ascii", Windows1252 },
			{ "windows-1252", Windows1252 },
			{ "x-cp1252", Windows1252 },
			{ "cp1253", Windows1253 },
			{ "windows-1253", Windows1253 },
			{ "x-cp1253", Windows1253 },
			{ "cp1254", Windows1254 },
			{ "csisolatin5", Windows1254 },
			{ "iso-8859-9", Windows1254 },
			{ "iso-ir-148", Windows1254 },
			{ "iso8859-9", Windows1254 },
			{ "iso88599", Windows1254 },
			{ "iso_8859-9", Windows1254 },
			{ "iso_8859-9:1989", Windows1254 },
			{ "l5", Windows1254 },
			{ "latin5", Windows1254 },
			{ "windows-1254", Windows1254 },
			{ "x-cp1254", Windows1254 },
			{ "cp1255", Windows1255 },
			{ "windows-1255", Windows1255 },
			{ "x-cp1255", Windows1255 },
			{ "cp1256", Windows1256 },
			{ "windows-1256", Windows1256 },
			{ "x-cp1256", Windows1256 },
			{ "cp1257", Windows1257 },
			{ "windows-1257", Windows1257 },
			{ "x-cp1257", Windows1257 },
			{ "cp1258", Windows1258 },
			{ "windows-1258", Windows1258 },
			{ "x-cp1258", Windows1258 }
		};
		Encoding encoding = GetEncoding("macintosh");
		obj.Add("csmacintosh", encoding);
		obj.Add("mac", encoding);
		obj.Add("macintosh", encoding);
		obj.Add("x-mac-roman", encoding);
		Encoding encoding2 = GetEncoding("x-mac-cyrillic");
		obj.Add("x-mac-cyrillic", encoding2);
		obj.Add("x-mac-ukrainian", encoding2);
		Encoding encoding3 = GetEncoding("cp866");
		obj.Add("866", encoding3);
		obj.Add("cp866", encoding3);
		obj.Add("csibm866", encoding3);
		obj.Add("ibm866", encoding3);
		obj.Add("csisolatin2", Latin2);
		obj.Add("iso-8859-2", Latin2);
		obj.Add("iso-ir-101", Latin2);
		obj.Add("iso8859-2", Latin2);
		obj.Add("iso88592", Latin2);
		obj.Add("iso_8859-2", Latin2);
		obj.Add("iso_8859-2:1987", Latin2);
		obj.Add("l2", Latin2);
		obj.Add("latin2", Latin2);
		obj.Add("csisolatin3", Latin3);
		obj.Add("iso-8859-3", Latin3);
		obj.Add("iso-ir-109", Latin3);
		obj.Add("iso8859-3", Latin3);
		obj.Add("iso88593", Latin3);
		obj.Add("iso_8859-3", Latin3);
		obj.Add("iso_8859-3:1988", Latin3);
		obj.Add("l3", Latin3);
		obj.Add("latin3", Latin3);
		obj.Add("csisolatin4", Latin4);
		obj.Add("iso-8859-4", Latin4);
		obj.Add("iso-ir-110", Latin4);
		obj.Add("iso8859-4", Latin4);
		obj.Add("iso88594", Latin4);
		obj.Add("iso_8859-4", Latin4);
		obj.Add("iso_8859-4:1988", Latin4);
		obj.Add("l4", Latin4);
		obj.Add("latin4", Latin4);
		obj.Add("csisolatincyrillic", Latin5);
		obj.Add("cyrillic", Latin5);
		obj.Add("iso-8859-5", Latin5);
		obj.Add("iso-ir-144", Latin5);
		obj.Add("iso8859-5", Latin5);
		obj.Add("iso88595", Latin5);
		obj.Add("iso_8859-5", Latin5);
		obj.Add("iso_8859-5:1988", Latin5);
		Encoding encoding4 = GetEncoding("iso-8859-6");
		obj.Add("arabic", encoding4);
		obj.Add("asmo-708", encoding4);
		obj.Add("csiso88596e", encoding4);
		obj.Add("csiso88596i", encoding4);
		obj.Add("csisolatinarabic", encoding4);
		obj.Add("ecma-114", encoding4);
		obj.Add("iso-8859-6", encoding4);
		obj.Add("iso-8859-6-e", encoding4);
		obj.Add("iso-8859-6-i", encoding4);
		obj.Add("iso-ir-127", encoding4);
		obj.Add("iso8859-6", encoding4);
		obj.Add("iso88596", encoding4);
		obj.Add("iso_8859-6", encoding4);
		obj.Add("iso_8859-6:1987", encoding4);
		Encoding encoding5 = GetEncoding("iso-8859-7");
		obj.Add("csisolatingreek", encoding5);
		obj.Add("ecma-118", encoding5);
		obj.Add("elot_928", encoding5);
		obj.Add("greek", encoding5);
		obj.Add("greek8", encoding5);
		obj.Add("iso-8859-7", encoding5);
		obj.Add("iso-ir-126", encoding5);
		obj.Add("iso8859-7", encoding5);
		obj.Add("iso88597", encoding5);
		obj.Add("iso_8859-7", encoding5);
		obj.Add("iso_8859-7:1987", encoding5);
		obj.Add("sun_eu_greek", encoding5);
		Encoding encoding6 = GetEncoding("iso-8859-8");
		obj.Add("csiso88598e", encoding6);
		obj.Add("csisolatinhebrew", encoding6);
		obj.Add("hebrew", encoding6);
		obj.Add("iso-8859-8", encoding6);
		obj.Add("iso-8859-8-e", encoding6);
		obj.Add("iso-ir-138", encoding6);
		obj.Add("iso8859-8", encoding6);
		obj.Add("iso88598", encoding6);
		obj.Add("iso_8859-8", encoding6);
		obj.Add("iso_8859-8:1988", encoding6);
		obj.Add("visual", encoding6);
		Encoding encoding7 = GetEncoding("iso-8859-8-i");
		obj.Add("csiso88598i", encoding7);
		obj.Add("iso-8859-8-i", encoding7);
		obj.Add("logical", encoding7);
		Encoding encoding8 = GetEncoding("iso-8859-13");
		obj.Add("iso-8859-13", encoding8);
		obj.Add("iso8859-13", encoding8);
		obj.Add("iso885913", encoding8);
		Encoding encoding9 = GetEncoding("iso-8859-15");
		obj.Add("csisolatin9", encoding9);
		obj.Add("iso-8859-15", encoding9);
		obj.Add("iso8859-15", encoding9);
		obj.Add("iso885915", encoding9);
		obj.Add("iso_8859-15", encoding9);
		obj.Add("l9", encoding9);
		Encoding encoding10 = GetEncoding("koi8-r");
		obj.Add("cskoi8r", encoding10);
		obj.Add("koi", encoding10);
		obj.Add("koi8", encoding10);
		obj.Add("koi8-r", encoding10);
		obj.Add("koi8_r", encoding10);
		obj.Add("koi8-u", GetEncoding("koi8-u"));
		Encoding encoding11 = GetEncoding("x-cp20936");
		obj.Add("chinese", encoding11);
		obj.Add("csgb2312", encoding11);
		obj.Add("csiso58gb231280", encoding11);
		obj.Add("gb2312", encoding11);
		obj.Add("gb_2312", encoding11);
		obj.Add("gb_2312-80", encoding11);
		obj.Add("gbk", encoding11);
		obj.Add("iso-ir-58", encoding11);
		obj.Add("x-gbk", encoding11);
		obj.Add("hz-gb-2312", GetEncoding("hz-gb-2312"));
		obj.Add("gb18030", Gb18030);
		obj.Add("big5", Big5);
		obj.Add("big5-hkscs", Big5);
		obj.Add("cn-big5", Big5);
		obj.Add("csbig5", Big5);
		obj.Add("x-x-big5", Big5);
		Encoding encoding12 = GetEncoding("iso-2022-jp");
		obj.Add("csiso2022jp", encoding12);
		obj.Add("iso-2022-jp", encoding12);
		Encoding encoding13 = GetEncoding("iso-2022-kr");
		obj.Add("csiso2022kr", encoding13);
		obj.Add("iso-2022-kr", encoding13);
		Encoding encoding14 = GetEncoding("iso-2022-cn");
		obj.Add("iso-2022-cn", encoding14);
		obj.Add("iso-2022-cn-ext", encoding14);
		obj.Add("shift_jis", GetEncoding("shift_jis"));
		Encoding encoding15 = GetEncoding("euc-jp");
		obj.Add("euc-jp", encoding15);
		return obj;
	}
}
