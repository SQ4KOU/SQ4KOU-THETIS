using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

public class CATCommands
{
	private readonly Console console;

	private readonly CATParser parser;

	private readonly string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

	private Band[] BandList;

	private int LastBandIndex;

	private readonly ASCIIEncoding AE = new ASCIIEncoding();

	private readonly int NCATInit = 500;

	private int NCATs;

	private readonly int NCATtime = 50;

	public bool firstTimeCAT = true;

	public bool isMidi;

	public bool isMidi2;

	public bool Verbose { get; set; }

	public CATCommands()
	{
	}

	public CATCommands(Console c, CATParser p)
	{
		console = c;
		parser = p;
		MakeBandList();
	}

	public string AG(string s)
	{
		if (s.Length == parser.nSet)
		{
			int aF = (int)Math.Round((double)Convert.ToInt32(s.Substring(1)) / 2.55, 0);
			console.AF = aF;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int n = (int)Math.Round((double)console.AF / 0.392, 0);
			return AddLeadingZeros(n);
		}
		return parser.Error1;
	}

	public string AI(string s)
	{
		return ZZAI(s);
	}

	public string BD()
	{
		return ZZBD();
	}

	public string BU()
	{
		return ZZBU();
	}

	public string CN(string s)
	{
		return ZZTB(s);
	}

	public string CT(string s)
	{
		return ZZTA(s);
	}

	public string DN()
	{
		return ZZSA();
	}

	public string FA(string s)
	{
		return ZZFA(s);
	}

	public string FB(string s)
	{
		return ZZFB(s);
	}

	public string FR(string s)
	{
		if (s.Length == parser.nSet)
		{
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return "0";
		}
		return parser.Error1;
	}

	public string FT(string s, bool bFromCatDirect = false)
	{
		return ZZSP(s, bFromCatDirect);
	}

	public string FW(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.RX1Filter = String2Filter(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return Filter2String(console.RX1Filter);
		}
		return parser.Error1;
	}

	public string GT(string s)
	{
		if (ZZGT(s).Length > 0)
		{
			return ZZGT(s).PadLeft(3, '0');
		}
		return "";
	}

	public string ID()
	{
		return console.CATRigType switch
		{
			900 => "900", 
			13 => "013", 
			19 => "019", 
			20 => "020", 
			_ => "019", 
		};
	}

	public string IF()
	{
		if (NCATs < NCATInit)
		{
			Thread.Sleep(NCATtime);
			NCATs++;
		}
		string text = "";
		string text2 = "0";
		string text3 = "0";
		string text4 = "0";
		string text5 = "";
		int num = 0;
		if (console.RITOn)
		{
			text2 = "1";
		}
		else if (console.XITOn)
		{
			text3 = "1";
		}
		if (text2 == "1")
		{
			num = console.RITValue;
		}
		else if (text3 == "1")
		{
			num = console.XITValue;
		}
		string text6 = ((num >= 0) ? ("+" + Convert.ToString(Math.Abs(num)).PadLeft(5, '0')) : ("-" + Convert.ToString(Math.Abs(num)).PadLeft(5, '0')));
		if (console.MOX)
		{
			text4 = "1";
		}
		int tuneStepIndex = console.TuneStepIndex;
		string text7 = Step2String(tuneStepIndex);
		string text8 = "0";
		if (console.VFOSplit)
		{
			text8 = "1";
		}
		string text9 = ZZFA("");
		if (text9.Length > 11)
		{
			text9 = text9.Substring(text9.Length - 11, 11);
		}
		text += text9;
		text += text7;
		text += text6;
		text += text2;
		text += text3;
		text += "000";
		text += text4;
		text5 = Mode2KString(console.RX1DSPMode);
		text = ((!(text5 == "?;")) ? (text + text5) : (text + "2"));
		text += "0";
		text += "0";
		text += text8;
		return text + "0000";
	}

	public string KS(string s)
	{
		return ZZKS(s);
	}

	public string KY(string s)
	{
		switch (console.RX1DSPMode)
		{
		case DSPMode.LSB:
		case DSPMode.USB:
		case DSPMode.DSB:
		case DSPMode.FM:
		case DSPMode.AM:
		case DSPMode.SPEC:
		case DSPMode.SAM:
		case DSPMode.DRM:
			if (console.RX1Band >= Band.B160M && console.RX1Band <= Band.B40M)
			{
				console.RX1DSPMode = DSPMode.CWL;
			}
			else
			{
				console.RX1DSPMode = DSPMode.CWU;
			}
			break;
		default:
			console.RX1DSPMode = DSPMode.CWU;
			break;
		case DSPMode.CWL:
		case DSPMode.CWU:
			break;
		}
		if (s.Length == parser.nSet)
		{
			string text = "";
			text = ((s.Trim().Length != 0) ? s.TrimEnd() : " ");
			if (text.Length > 1)
			{
				byte[] bytes = AE.GetBytes(text);
				return console.CWXForm.RemoteMessage(bytes);
			}
			char msg = Convert.ToChar(text);
			return console.CWXForm.RemoteMessage(msg);
		}
		if (s.Length == parser.nGet)
		{
			if (console.CWXForm.Characters2Send < 72)
			{
				return "0";
			}
			return "1";
		}
		return parser.Error1;
	}

	public string MD(string s)
	{
		if (NCATs < NCATInit)
		{
			Thread.Sleep(NCATtime);
			NCATs++;
		}
		if (s.Length == parser.nSet)
		{
			if (Convert.ToInt32(s) > 0 && Convert.ToInt32(s) <= 9)
			{
				KString2Mode(s);
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			return Mode2KString(console.RX1DSPMode);
		}
		return parser.Error1;
	}

	public string MG(string s)
	{
		if (s.Length == parser.nSet)
		{
			int val = Convert.ToInt32(s);
			val = Math.Max(0, val);
			val = Math.Min(100, val);
			int n = (int)Math.Round((double)val / 1.43, 0);
			s = AddLeadingZeros(n);
			return ZZMG(s);
		}
		if (s.Length == parser.nGet)
		{
			s = ZZMG("");
			int val = Convert.ToInt32(s);
			int n2 = (int)Math.Round((double)val / 0.7, 0);
			s = AddLeadingZeros(n2);
			return s;
		}
		return parser.Error1;
	}

	public string MO(string s)
	{
		return ZZMO(s);
	}

	public string NB(string s)
	{
		return ZZNA(s);
	}

	public string NT(string s)
	{
		return ZZNT(s);
	}

	public string OF(string s)
	{
		return ZZOT(s);
	}

	public string OS(string s)
	{
		return ZZOS(s);
	}

	public string PC(string s)
	{
		return ZZPC(s);
	}

	public string PR(string s)
	{
		if (s.Length == parser.nGet)
		{
			return "0";
		}
		return parser.Error1;
	}

	public string PS(string s)
	{
		return ZZPS(s);
	}

	public string QI()
	{
		return ZZQS();
	}

	public string RC()
	{
		return ZZRC();
	}

	public string RD(string s)
	{
		return ZZRD(s);
	}

	public string RT(string s)
	{
		return ZZRT(s);
	}

	public string RU(string s)
	{
		return ZZRU(s);
	}

	public string RX(string s)
	{
		console.CATPTT = false;
		return "";
	}

	public string SH(string s)
	{
		if (s.Length == parser.nSet)
		{
			SetFilter(s, "SH");
			return "";
		}
		if (s.Length == parser.nGet)
		{
			switch (console.RX1DSPMode)
			{
			case DSPMode.USB:
			case DSPMode.DSB:
			case DSPMode.CWU:
			case DSPMode.FM:
			case DSPMode.AM:
			case DSPMode.SAM:
			case DSPMode.DRM:
				return Frequency2Code(console.RX1FilterHigh, "SH");
			case DSPMode.LSB:
			case DSPMode.CWL:
				return Frequency2Code(console.RX1FilterLow, "SH");
			default:
				return Frequency2Code(console.RX1FilterHigh, "SH");
			}
		}
		return parser.Error1;
	}

	public string SL(string s)
	{
		if (s.Length == parser.nSet)
		{
			SetFilter(s, "SL");
			return "";
		}
		if (s.Length == parser.nGet)
		{
			switch (console.RX1DSPMode)
			{
			case DSPMode.USB:
			case DSPMode.DSB:
			case DSPMode.CWU:
			case DSPMode.FM:
			case DSPMode.AM:
			case DSPMode.SAM:
			case DSPMode.DRM:
				return Frequency2Code(console.RX1FilterLow, "SL");
			case DSPMode.LSB:
			case DSPMode.CWL:
				return Frequency2Code(console.RX1FilterHigh, "SL");
			default:
				return Frequency2Code(console.RX1FilterLow, "SL");
			}
		}
		return parser.Error1;
	}

	public string SM(string s)
	{
		int num = 0;
		double num2 = 0.0;
		if (s == "0" || s == "2")
		{
			float num3 = 0f;
			if (console.PowerOn)
			{
				num3 = WDSP.CalculateRXMeter(0u, 0u, WDSP.MeterType.SIGNAL_STRENGTH);
			}
			num3 = num3 + console.RX1MeterCalOffset + console.PreampOffset;
			num3 = Math.Max(-140f, num3);
			num3 = Math.Min(-10f, num3);
			num2 = (num3 + 127f) / 6f;
			if (num2 < 0.0)
			{
				num2 = 0.0;
			}
			if (num2 <= 9.0)
			{
				num = Math.Abs((int)(num2 * 1.6667));
			}
			else
			{
				double num4 = num3 + 73f;
				num = 15 + (int)num4;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (num > 30)
			{
				num = 30;
			}
			return num.ToString().PadLeft(5, '0');
		}
		return parser.Error1;
	}

	public string SQ(string s)
	{
		string text = s.Substring(0, 1);
		double num = 0.0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToDouble(s.Substring(1));
			num = Math.Max(0.0, num);
			num = Math.Min(255.0, num);
			num = -160.0 + num * 0.62745;
			console.Squelch = Convert.ToInt32(Math.Round(num, 0));
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int n = Convert.ToInt32((1.0 - Math.Abs((double)console.Squelch / 160.0)) * 255.0);
			return text + AddLeadingZeros(n).Substring(1);
		}
		return parser.Error1;
	}

	public string TX(string s)
	{
		console.CATPTT = true;
		return "";
	}

	public string UP()
	{
		return ZZSB();
	}

	public string XT(string s)
	{
		return ZZXS(s);
	}

	public string ZZAA(string s)
	{
		int aPFGain = 0;
		int num = 0;
		if (s != "")
		{
			aPFGain = Convert.ToInt32(s);
			aPFGain = Math.Max(0, aPFGain);
			aPFGain = Math.Min(100, aPFGain);
		}
		if (s.Length == parser.nSet)
		{
			console.APFGain = aPFGain;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.APFGain;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZAB(string s)
	{
		int aPFBandwidth = 0;
		int num = 0;
		if (s != "")
		{
			aPFBandwidth = Convert.ToInt32(s);
			aPFBandwidth = Math.Max(10, aPFBandwidth);
			aPFBandwidth = Math.Min(150, aPFBandwidth);
		}
		if (s.Length == parser.nSet)
		{
			console.APFBandwidth = aPFBandwidth;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.APFBandwidth;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZAC(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Convert.ToInt32(s);
			if (num >= 0 || num <= 25)
			{
				console.TuneStepIndex = num;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			int num = console.TuneStepIndex;
			return AddLeadingZeros(num);
		}
		return parser.Error1;
	}

	public string ZZAD(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Convert.ToInt32(s);
			if (num >= 0 || num <= 25)
			{
				console.VFOAFreq = console.CATVFOA - Step2Freq(num);
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZAE(string s)
	{
		int tuneStepIndex = console.TuneStepIndex;
		int num = 0;
		double num2 = (double)console.TuneStepList[tuneStepIndex].StepHz * 1E-06;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			if (num >= 0 && tuneStepIndex <= 99)
			{
				console.VFOAFreq = console.CATVFOA - num2 * (double)num;
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZAF(string s)
	{
		int tuneStepIndex = console.TuneStepIndex;
		int num = 0;
		double num2 = (double)console.TuneStepList[tuneStepIndex].StepHz * 1E-06;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			if (num >= 0 && tuneStepIndex <= 99)
			{
				console.VFOAFreq = console.CATVFOA + num2 * (double)num;
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZAG(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(0, num);
			num = Math.Min(100, num);
			console.AF = num;
			console.TitleBarEncoderString = "master AF Gain = " + num + "%";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.AF);
		}
		return parser.Error1;
	}

	public string ZZAI(string s)
	{
		if (console.SetupForm.AllowFreqBroadcast)
		{
			if (s.Length == parser.nSet)
			{
				if (s == "0")
				{
					console.KWAutoInformation = false;
				}
				else
				{
					console.KWAutoInformation = true;
				}
				return "";
			}
			if (s.Length == parser.nGet)
			{
				if (console.KWAutoInformation)
				{
					return "1";
				}
				return "0";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZAP(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.CATAPF = 0;
			}
			else if (s == "1")
			{
				console.CATAPF = 1;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATAPF == 1)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZAR(string s)
	{
		int num = 0;
		int num2 = 0;
		if (s != "")
		{
			num = Convert.ToInt32(s);
			num = Math.Max(-20, num);
			num = Math.Min(120, num);
		}
		if (s.Length == parser.nSet)
		{
			if (console.RF != num && console.AutoAGCRX1)
			{
				console.AutoAGCRX1 = false;
			}
			console.RF = num;
			console.TitleBarEncoderString = "RX1 AGC Threshold = " + num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num2 = console.RF;
			string text = ((num2 < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num2)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZAS(string s)
	{
		int num = 0;
		int num2 = 0;
		if (s != "")
		{
			num = Convert.ToInt32(s);
			num = Math.Max(-20, num);
			num = Math.Min(120, num);
		}
		if (s.Length == parser.nSet)
		{
			if (console.RX2RF != num && console.AutoAGCRX2)
			{
				console.AutoAGCRX2 = false;
			}
			console.RX2RF = num;
			console.TitleBarEncoderString = "RX2 AGC Threshold = " + num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num2 = console.RX2RF;
			string text = ((num2 < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num2)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZAT(string s)
	{
		int aPFFreq = 0;
		int num = 0;
		if (s != "")
		{
			aPFFreq = Convert.ToInt32(s);
			aPFFreq = Math.Max(-250, aPFFreq);
			aPFFreq = Math.Min(250, aPFFreq);
		}
		if (s.Length >= parser.nSet)
		{
			console.APFFreq = aPFFreq;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.APFFreq;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZAU(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			if (num >= 0 || num <= 14)
			{
				console.VFOAFreq = console.CATVFOA + Step2Freq(num);
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZAY(string s)
	{
		int aPFType = 0;
		int num = 0;
		if (s != "")
		{
			aPFType = Convert.ToInt32(s);
			aPFType = Math.Max(0, aPFType);
			aPFType = Math.Min(3, aPFType);
		}
		if (s.Length == parser.nSet)
		{
			console.APFType = aPFType;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.APFType.ToString();
		}
		return parser.Error1;
	}

	public string ZZBA()
	{
		console.CATRX2BandUpDown(-1);
		return "";
	}

	public string ZZBB()
	{
		console.CATRX2BandUpDown(1);
		return "";
	}

	public string ZZBD()
	{
		BandDown();
		return "";
	}

	public string ZZBE(string s)
	{
		int tuneStepIndex = console.TuneStepIndex;
		double num = (double)console.TuneStepList[tuneStepIndex].StepHz * 1E-06;
		if (s.Length == parser.nSet)
		{
			int num2 = Convert.ToInt32(s);
			if (num2 >= 0 && tuneStepIndex <= 99)
			{
				console.VFOBFreq = console.CATVFOB - num * (double)num2;
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZBF(string s)
	{
		int tuneStepIndex = console.TuneStepIndex;
		double num = (double)console.TuneStepList[tuneStepIndex].StepHz * 1E-06;
		if (s.Length == parser.nSet)
		{
			int num2 = Convert.ToInt32(s);
			if (num2 >= 0 && tuneStepIndex <= 99)
			{
				console.VFOBFreq = console.CATVFOB + num * (double)num2;
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZBG(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATBandGroup = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATBandGroup.ToString();
		}
		return parser.Error1;
	}

	public string ZZBI(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATBIN = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATBIN.ToString();
		}
		return parser.Error1;
	}

	public string ZZBM(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Convert.ToInt32(s);
			if (num >= 0 || num <= 14)
			{
				console.VFOBFreq = console.CATVFOB - Step2Freq(num);
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZBP(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Convert.ToInt32(s);
			if (num >= 0 || num <= 14)
			{
				console.VFOBFreq = console.CATVFOB + Step2Freq(num);
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZBR(string s)
	{
		if (HardwareSpecific.Model == HPSDRModel.HPSDR)
		{
			int cATBCIReject = 0;
			if (s != "")
			{
				cATBCIReject = Convert.ToInt32(s);
			}
			if (s.Length == parser.nSet && (s == "0" || s == "1"))
			{
				console.CATBCIReject = cATBCIReject;
				return "";
			}
			if (s.Length == parser.nGet)
			{
				return console.CATBCIReject.ToString();
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZBS(string s)
	{
		return GetBand(s);
	}

	public string ZZBT(string s)
	{
		if (s.Length == parser.nGet)
		{
			return Band2String(console.RX2Band);
		}
		if (s.Length == parser.nSet)
		{
			Band band = String2Band(s);
			console.RX2Band = band;
			console.SetupRX2Band(band);
			return "";
		}
		return parser.Error1;
	}

	public string ZZBU()
	{
		BandUp();
		return "";
	}

	public string ZZBY()
	{
		if (console.InvokeRequired)
		{
			console.BeginInvoke((MethodInvoker)delegate
			{
				console.Close();
			});
		}
		else
		{
			console.Close();
		}
		return "";
	}

	public string ZZCB(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATBreakIn = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATBreakIn.ToString();
		}
		return parser.Error1;
	}

	public string ZZCD(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(150, val);
		val = Math.Min(5000, val);
		if (s.Length == parser.nSet)
		{
			console.SetupForm.BreakInDelay = val;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.BreakInDelay);
		}
		return parser.Error1;
	}

	public string ZZCF(string s)
	{
		DSPMode rX1DSPMode = console.RX1DSPMode;
		if ((uint)(rX1DSPMode - 3) <= 1u)
		{
			if (s.Length == parser.nSet && (s == "0" || s == "1"))
			{
				if (s == "1")
				{
					console.ShowCWTXFreq = true;
				}
				else
				{
					console.ShowCWTXFreq = false;
				}
				return "";
			}
			if (s.Length == parser.nGet)
			{
				if (console.ShowCWTXFreq)
				{
					return "1";
				}
				return "0";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZCI(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.CWIambic = true;
			}
			else
			{
				console.CWIambic = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.CWIambic)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZCL(string s)
	{
		int cATCWPitch = 0;
		if (s != "")
		{
			cATCWPitch = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.CATCWPitch = cATCWPitch;
			console.TitleBarEncoderString = "CW Sidetone Freq = " + cATCWPitch + "Hz";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.CATCWPitch);
		}
		return parser.Error1;
	}

	public string ZZCM(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.CWHWSidetone = false;
			}
			else
			{
				console.CWHWSidetone = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.CWHWSidetone)
			{
				return "0";
			}
			return "1";
		}
		return parser.Error1;
	}

	public string ZZCN(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "1")
			{
				console.CTuneDisplay = true;
				return "";
			}
			if (s == "0")
			{
				console.CTuneDisplay = false;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (console.CTuneDisplay)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZCO(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "1")
			{
				console.CTuneRX2Display = true;
				return "";
			}
			if (s == "0")
			{
				console.CTuneRX2Display = false;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (console.CTuneRX2Display)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZCP(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.CATCmpd = 0;
			}
			else if (s == "1")
			{
				console.CATCmpd = 1;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATCmpd.ToString();
		}
		return parser.Error1;
	}

	public string ZZCS(string s)
	{
		int cATCWSpeed = 1;
		if (s != "")
		{
			cATCWSpeed = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			console.CATCWSpeed = cATCWSpeed;
			console.TitleBarEncoderString = "CW Speed = " + cATCWSpeed + "WPM";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.CATCWSpeed);
		}
		return parser.Error1;
	}

	public string ZZCT(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(console.CPDRMin, val);
		val = Math.Min(console.CPDRMax, val);
		if (s.Length == parser.nSet)
		{
			console.CPDRLevel = val;
			console.TitleBarEncoderString = "Comp Threshold = " + val + "dB";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.CPDRLevel);
		}
		return parser.Error1;
	}

	public string ZZCU()
	{
		if (console.initializing)
		{
			return "";
		}
		return $"{console.CPUPercSmoothed:000.00}";
	}

	public string ZZDA(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATDisplayAvg = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATDisplayAvg.ToString();
		}
		return parser.Error1;
	}

	public string ZZDB(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "1")
			{
				console.CATDiversityRXRefSource = true;
				return "";
			}
			if (s == "0")
			{
				console.CATDiversityRXRefSource = false;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATDiversityRXRefSource)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZDC(string s)
	{
		int val = 0;
		if (s.Length == parser.nSet)
		{
			if (s != null && s != "")
			{
				val = Convert.ToInt32(s);
			}
			val = Math.Max(0, val);
			val = Math.Min(5000, val);
			decimal cATDiversityRX2Gain = (decimal)val / 1000.0m;
			console.CATDiversityRX2Gain = cATDiversityRX2Gain;
			console.TitleBarEncoderString = "Diversity RX2 Gain = " + cATDiversityRX2Gain;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			decimal cATDiversityRX2Gain = console.CATDiversityRX2Gain;
			val = (int)(cATDiversityRX2Gain * 1000.0m);
			return AddLeadingZeros(val);
		}
		return parser.Error1;
	}

	public string ZZDD(string s)
	{
		int val = 0;
		if (s.Length == parser.nSet)
		{
			if (s != null && s != "")
			{
				val = Convert.ToInt32(s);
			}
			val = Math.Min(18000, val);
			val = Math.Max(-18000, val);
			decimal cATDiversityPhase = (decimal)val / 100.0m;
			console.TitleBarEncoderString = "Diversity Phase = " + cATDiversityPhase + "degrees";
			console.CATDiversityPhase = cATDiversityPhase;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			decimal cATDiversityPhase = console.CATDiversityPhase;
			val = (int)(cATDiversityPhase * 100.0m);
			string text = ((val >= 0) ? "+" : "-");
			return text + AddLeadingZeros(Math.Abs(val)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZDE(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "1")
			{
				console.CATDiversityEnable = true;
				return "";
			}
			if (s == "0")
			{
				console.CATDiversityEnable = false;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATDiversityEnable)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZDF(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "1")
			{
				console.CATDiversityForm = true;
				return "";
			}
			if (s == "0")
			{
				console.CATDiversityForm = false;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATDiversityForm)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZDG(string s)
	{
		int val = 0;
		if (s.Length == parser.nSet)
		{
			if (s != null && s != "")
			{
				val = Convert.ToInt32(s);
			}
			val = Math.Max(0, val);
			val = Math.Min(1000, val);
			decimal cATDiversityGain = (decimal)val / 1000.0m;
			console.CATDiversityGain = cATDiversityGain;
			console.TitleBarEncoderString = "Diversity RX Gain = " + cATDiversityGain;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			decimal cATDiversityGain = console.CATDiversityGain;
			val = (int)(cATDiversityGain * 1000.0m);
			return AddLeadingZeros(val);
		}
		return parser.Error1;
	}

	public string ZZDH(string s)
	{
		if (s.Length == parser.nSet)
		{
			switch (s)
			{
			case "2":
				console.CATDiversityRXSource = 2;
				return "";
			case "1":
				console.CATDiversityRXSource = 1;
				return "";
			case "0":
				console.CATDiversityRXSource = 0;
				return "";
			default:
				return parser.Error1;
			}
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATDiversityRXSource == 2)
			{
				return "2";
			}
			if (console.CATDiversityRXSource == 1)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZDM(string s)
	{
		int num = -1;
		if (s.Length == parser.nSet)
		{
			switch (Convert.ToInt32(s))
			{
			case 0:
				console.DisplayModeText = "Spectrum";
				break;
			case 1:
				console.DisplayModeText = "Panadapter";
				break;
			case 2:
				console.DisplayModeText = "Scope";
				break;
			case 3:
				console.DisplayModeText = "Phase";
				break;
			case 4:
				console.DisplayModeText = "Phase2";
				break;
			case 5:
				console.DisplayModeText = "Waterfall";
				break;
			case 6:
				console.DisplayModeText = "Histogram";
				break;
			case 7:
				console.DisplayModeText = "Panafall";
				break;
			case 8:
				console.DisplayModeText = "Panascope";
				break;
			case 9:
				console.DisplayModeText = "Off";
				break;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.GetDisplayMode(1) switch
			{
				DisplayMode.SPECTRUM => "0", 
				DisplayMode.PANADAPTER => "1", 
				DisplayMode.SCOPE => "2", 
				DisplayMode.PHASE => "3", 
				DisplayMode.PHASE2 => "4", 
				DisplayMode.WATERFALL => "5", 
				DisplayMode.HISTOGRAM => "6", 
				DisplayMode.PANAFALL => "7", 
				DisplayMode.PANASCOPE => "8", 
				DisplayMode.OFF => "9", 
				_ => parser.Error1, 
			};
		}
		return parser.Error1;
	}

	public string ZZDN(string s)
	{
		int cATWFLo = 0;
		int num = 0;
		if (s != "")
		{
			cATWFLo = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.CATWFLo = cATWFLo;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.SetupForm.CATWFLo;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZDO(string s)
	{
		int cATWFHi = 0;
		int num = 0;
		if (s != "")
		{
			cATWFHi = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.CATWFHi = cATWFHi;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.SetupForm.CATWFHi;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZDP(string s)
	{
		int cATSGMax = 0;
		int num = 0;
		if (s != "")
		{
			cATSGMax = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.CATSGMax = cATSGMax;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.SetupForm.CATSGMax;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZDQ(string s)
	{
		int cATSGMin = 0;
		int num = 0;
		if (s != "")
		{
			cATSGMin = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.CATSGMin = cATSGMin;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.SetupForm.CATSGMin;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZDR(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.CATSGStep = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.CATSGStep);
		}
		return parser.Error1;
	}

	public string ZZDU()
	{
		int nAns = parser.nAns;
		string text = ":";
		parser.nAns = 1;
		string text2 = string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + ZZSW("") + text, ZZSP(""), text), ZZTU(""), text), ZZTX(""), text), "0:0:0:"), ZZRS(""), text), ZZRT(""), text), ZZDM(""), text), ZZGT(""), text), ZZMU(""), text), ZZXS(""), text);
		parser.nAns = 2;
		string text3 = string.Concat(string.Concat(string.Concat(string.Concat(text2 + ZZAC("") + text, ZZMD(""), text), ZZME(""), text), ZZFJ(""), text), ZZFI(""), text);
		parser.nAns = 3;
		string text4 = string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(text3 + "000:", ZZBT(""), text), ZZPC(""), text), ZZBS(""), text), ZZAG(""), text), ZZKS(""), text), ZZTO(""), text);
		parser.nAns = 4;
		string text5 = string.Concat(text4 + "0000:", ZZSM("0"), text);
		parser.nAns = 5;
		string text6 = string.Concat(string.Concat(text5 + ZZRF("") + text, "00000:"), ZZXF(""), text);
		parser.nAns = 6;
		string text7 = text6 + ZZCU() + text;
		parser.nAns = 11;
		string result = string.Concat(text7 + ZZFA("") + text, ZZFB(""));
		parser.nAns = nAns;
		return result;
	}

	public string ZZDX(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.CATPhoneDX = s;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATPhoneDX;
		}
		return parser.Error1;
	}

	public string ZZEA(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = int.Parse(s.Substring(0, 3));
			int[] array = new int[num + 1];
			s = s.Remove(0, 3);
			for (int i = 0; i <= num; i++)
			{
				array[i] = int.Parse(s.Substring(0, 3));
				s = s.Remove(0, 3);
			}
			console.EQForm.RXEQ = array;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int[] rXEQ = console.EQForm.RXEQ;
			int numBands = console.EQForm.NumBands;
			string text = numBands.ToString().PadLeft(3, '0');
			for (int j = 0; j <= numBands; j++)
			{
				text = ((rXEQ[j] >= 0) ? (text + rXEQ[j].ToString().PadLeft(3, '0')) : (text + "-" + Math.Abs(rXEQ[j]).ToString().PadLeft(2, '0')));
			}
			return text.PadRight(36, '0');
		}
		return parser.Error1;
	}

	public string ZZEB(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = int.Parse(s.Substring(0, 3));
			int[] array = new int[num + 1];
			s = s.Remove(0, 3);
			for (int i = 0; i <= num; i++)
			{
				array[i] = int.Parse(s.Substring(0, 3));
				s = s.Remove(0, 3);
			}
			console.EQForm.TXEQ = array;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int[] tXEQ = console.EQForm.TXEQ;
			int numBands = console.EQForm.NumBands;
			string text = numBands.ToString().PadLeft(3, '0');
			for (int j = 0; j <= numBands; j++)
			{
				text = ((tXEQ[j] >= 0) ? (text + tXEQ[j].ToString().PadLeft(3, '0')) : (text + "-" + Math.Abs(tXEQ[j]).ToString().PadLeft(2, '0')));
			}
			return text.PadRight(36, '0');
		}
		return parser.Error1;
	}

	public string ZZEM(string s)
	{
		if (s.Length == parser.nSet && (s == "1" || s == "0"))
		{
			if (s == "1")
			{
				parser.Verbose = true;
			}
			else
			{
				parser.Verbose = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (parser.Verbose)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZER(string s)
	{
		if (s.Length == parser.nSet && (s == "1" || s == "0"))
		{
			if (s == "1")
			{
				console.CATRXEQ = "1";
			}
			else if (s == "0")
			{
				console.CATRXEQ = "0";
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATRXEQ;
		}
		return parser.Error1;
	}

	public string ZZET(string s)
	{
		if (s.Length == parser.nSet && (s == "1" || s == "0"))
		{
			if (s == "1")
			{
				console.CATTXEQ = "1";
			}
			else if (s == "0")
			{
				console.CATTXEQ = "0";
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATTXEQ;
		}
		return parser.Error1;
	}

	public string ZZFA(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (console.SetupForm.RttyOffsetEnabledA && (console.RX1DSPMode == DSPMode.DIGU || console.RX1DSPMode == DSPMode.DIGL))
			{
				int num = int.Parse(s);
				if (console.RX1DSPMode == DSPMode.DIGU)
				{
					num -= Convert.ToInt32(console.SetupForm.RttyOffsetHigh);
				}
				else if (console.RX1DSPMode == DSPMode.DIGL)
				{
					num += Convert.ToInt32(console.SetupForm.RttyOffsetLow);
				}
				s = AddLeadingZeros(num, 11);
				s = s.Insert(5, separator);
			}
			else
			{
				s = s.Insert(5, separator);
			}
			if (!isMidi && console.CATChangesCenterFreq)
			{
				console.UpdateCenterFreq = true;
			}
			console.VFOAFreq = double.Parse(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.RttyOffsetEnabledA && (console.RX1DSPMode == DSPMode.DIGU || console.RX1DSPMode == DSPMode.DIGL))
			{
				int num2 = Convert.ToInt32(Math.Round(console.CATVFOA, 6) * 1000000.0);
				if (console.RX1DSPMode == DSPMode.DIGU)
				{
					num2 += Convert.ToInt32(console.SetupForm.RttyOffsetHigh);
				}
				else if (console.RX1DSPMode == DSPMode.DIGL)
				{
					num2 -= Convert.ToInt32(console.SetupForm.RttyOffsetLow);
				}
				return AddLeadingZeros(num2);
			}
			return StrVFOFreq("A");
		}
		return parser.Error1;
	}

	public string ZZFB(string s)
	{
		if (s.Length == parser.nSet)
		{
			DSPMode dSPMode = (console.RX2Enabled ? console.RX2DSPMode : console.RX1DSPMode);
			if (console.SetupForm.RttyOffsetEnabledB && (dSPMode == DSPMode.DIGU || dSPMode == DSPMode.DIGL))
			{
				int num = int.Parse(s);
				switch (dSPMode)
				{
				case DSPMode.DIGU:
					num -= Convert.ToInt32(console.SetupForm.RttyOffsetHigh);
					break;
				case DSPMode.DIGL:
					num += Convert.ToInt32(console.SetupForm.RttyOffsetLow);
					break;
				}
				s = AddLeadingZeros(num, 11);
				s = s.Insert(5, separator);
			}
			else
			{
				s = s.Insert(5, separator);
			}
			if (!isMidi2 && console.CATChangesCenterFreq)
			{
				console.UpdateRX2CenterFreq = true;
			}
			console.VFOBFreq = double.Parse(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			DSPMode dSPMode2 = (console.RX2Enabled ? console.RX2DSPMode : console.RX1DSPMode);
			if (console.SetupForm.RttyOffsetEnabledB && (dSPMode2 == DSPMode.DIGU || dSPMode2 == DSPMode.DIGL))
			{
				int num2 = Convert.ToInt32(Math.Round(console.CATVFOB, 6) * 1000000.0);
				switch (dSPMode2)
				{
				case DSPMode.DIGU:
					num2 += Convert.ToInt32(console.SetupForm.RttyOffsetHigh);
					break;
				case DSPMode.DIGL:
					num2 -= Convert.ToInt32(console.SetupForm.RttyOffsetLow);
					break;
				}
				return AddLeadingZeros(num2);
			}
			return StrVFOFreq("B");
		}
		return parser.Error1;
	}

	public string ZZFT(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.TXFreq = double.Parse(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int n = Convert.ToInt32(Math.Round(console.TXFreq, 6) * 1000000.0);
			return AddLeadingZeros(n);
		}
		return parser.Error1;
	}

	public string ZZFD(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "1")
			{
				console.FMDeviation_Hz = 5000;
			}
			else
			{
				console.FMDeviation_Hz = 2500;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.FMDeviation_Hz == 5000)
			{
				return "1";
			}
			if (console.FMDeviation_Hz == 2500)
			{
				return "0";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZFI(string s)
	{
		int num = 0;
		if (s != "")
		{
			num = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			if (num < 13)
			{
				console.RX1Filter = (Filter)num;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros((int)console.RX1Filter);
		}
		return parser.Error1;
	}

	public string ZZFJ(string s)
	{
		int num = 0;
		if (s != "")
		{
			num = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			if (num < 13)
			{
				console.RX2Filter = (Filter)num;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros((int)console.RX2Filter);
		}
		return parser.Error1;
	}

	public string ZZFL(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SelectRX1VarFilter();
			int val = Convert.ToInt32(s);
			val = Math.Min(10000, val);
			val = Math.Max(-10000, val);
			console.RX1FilterLow = val;
			console.TitleBarEncoderString = "VFO A Filter low cut = " + val + "Hz";
			console.UpdateRX1Filters(val, console.RX1FilterHigh, force: true);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int val = console.RX1FilterLow;
			string text = ((val >= 0) ? "+" : "-");
			return text + AddLeadingZeros(Math.Abs(val)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZFH(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SelectRX1VarFilter();
			int val = Convert.ToInt32(s);
			val = Math.Min(10000, val);
			val = Math.Max(-10000, val);
			console.RX1FilterHigh = val;
			console.TitleBarEncoderString = "VFO A Filter high cut = " + val + "Hz";
			console.UpdateRX1Filters(console.RX1FilterLow, val, force: true);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int val = console.RX1FilterHigh;
			string text = ((val >= 0) ? "+" : "-");
			return text + AddLeadingZeros(Math.Abs(val)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZFM()
	{
		string text = HardwareSpecific.Model.ToString();
		bool alexPresent = console.AlexPresent;
		switch (text)
		{
		case "HPSDR":
		case "HERMES":
			if (alexPresent)
			{
				return "1";
			}
			return "0";
		case "ANAN10":
		case "ANAN10E":
			return "0";
		case "ANAN100":
		case "ANAN100B":
		case "ANAN100D":
		case "ANAN200D":
		case "ANAN7000D":
		case "ANAN8000D":
		case "ANVELINAPRO3":
		case "ANAN_G2":
		case "ANAN_G2_1K":
			return "1";
		default:
			return parser.Error1;
		}
	}

	public string ZZFR(string s)
	{
		if (s.Length == parser.nSet)
		{
			int val = Convert.ToInt32(s);
			val = Math.Min(10000, val);
			val = Math.Max(-10000, val);
			console.RX2FilterHigh = val;
			console.TitleBarEncoderString = "VFO B Filter high cut = " + val + "Hz";
			console.UpdateRX2Filters(console.RX2FilterLow, val, force: true);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int val = console.RX2FilterHigh;
			string text = ((val >= 0) ? "+" : "-");
			return text + AddLeadingZeros(Math.Abs(val)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZFS(string s)
	{
		if (s.Length == parser.nSet)
		{
			int val = Convert.ToInt32(s);
			val = Math.Min(10000, val);
			val = Math.Max(-10000, val);
			console.RX2FilterLow = val;
			console.TitleBarEncoderString = "VFO B Filter low cut =" + val + "Hz";
			console.UpdateRX2Filters(val, console.RX2FilterHigh, force: true);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int val = console.RX2FilterLow;
			string text = ((val >= 0) ? "+" : "-");
			return text + AddLeadingZeros(Math.Abs(val)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZFV(string s)
	{
		if (s.Length == parser.nGet)
		{
			if (!new Regex("^[a-fA-F0-9][a-fA-F0-9]$").IsMatch(s))
			{
				return parser.Error1;
			}
			byte.Parse(s, NumberStyles.HexNumber);
			uint num = 0u;
			return $"{(byte)num:X2}";
		}
		return parser.Error1;
	}

	public string ZZFW(string s)
	{
		if (s.Length == parser.nGet)
		{
			if (!new Regex("^[a-fA-F0-9][a-fA-F0-9]$").IsMatch(s))
			{
				return parser.Error1;
			}
			byte.Parse(s, NumberStyles.HexNumber);
			uint num = 0u;
			string text = $"{num >> 8:X2}";
			string text2 = $"{(byte)num:X2}";
			return text + text2;
		}
		return parser.Error1;
	}

	public string ZZFX(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (!new Regex("^[a-fA-F0-9][a-fA-F0-9][a-fA-F0-9][a-fA-F0-9]$").IsMatch(s))
			{
				return parser.Error1;
			}
			byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
			byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
			return "";
		}
		return parser.Error1;
	}

	public string ZZFY(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (!new Regex("^[a-fA-F0-9][a-fA-F0-9][a-fA-F0-9][a-fA-F0-9][a-fA-F0-9][a-fA-F0-9]$").IsMatch(s))
			{
				return parser.Error1;
			}
			byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
			byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
			byte.Parse(s.Substring(4, 2), NumberStyles.HexNumber);
			return "";
		}
		return parser.Error1;
	}

	public string ZZGA(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (Guid.TryParse(s, out var result))
			{
				return "ZZGA" + result.ToString().ToLower();
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZGR(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (Guid.TryParse(s, out var result))
			{
				return "ZZGR" + result.ToString().ToLower();
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZGE(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.NoiseGateEnabled = true;
			}
			else
			{
				console.NoiseGateEnabled = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.NoiseGateEnabled)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZGL(string s)
	{
		int noiseGate = 0;
		int num = 0;
		if (s != "")
		{
			noiseGate = Convert.ToInt32(s);
			noiseGate = Math.Max(-160, noiseGate);
			noiseGate = Math.Min(0, noiseGate);
		}
		if (s.Length == parser.nSet)
		{
			console.NoiseGate = noiseGate;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.NoiseGate;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZGT(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (Convert.ToInt32(s) > -1 && Convert.ToInt32(s) < 6)
			{
				console.RX1AGCMode = (AGCMode)Convert.ToInt32(s);
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			return ((int)console.RX1AGCMode).ToString();
		}
		return parser.Error1;
	}

	public string ZZGU(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (Convert.ToInt32(s) > -1 && Convert.ToInt32(s) < 6)
			{
				console.RX2AGCMode = (AGCMode)Convert.ToInt32(s);
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			return ((int)console.RX2AGCMode).ToString();
		}
		return parser.Error1;
	}

	public string ZZHA(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.AudioBufferSize = Index2Width(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return Width2Index(console.SetupForm.AudioBufferSize);
		}
		return parser.Error1;
	}

	public string ZZHR(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Index2Width(s);
			console.DSPBufPhoneRX = num;
			console.SetupForm.DSPPhoneRXBuffer = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return Width2Index(console.DSPBufPhoneRX);
		}
		return parser.Error1;
	}

	public string ZZHT(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Index2Width(s);
			console.DSPBufPhoneTX = num;
			console.SetupForm.DSPPhoneTXBuffer = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return Width2Index(console.DSPBufPhoneTX);
		}
		return parser.Error1;
	}

	public string ZZHU(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Index2Width(s);
			console.DSPBufCWRX = num;
			console.SetupForm.DSPCWRXBuffer = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return Width2Index(console.DSPBufCWRX);
		}
		return parser.Error1;
	}

	public string ZZHV(string s)
	{
		if (s.Length == parser.nSet)
		{
			Index2Width(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return Width2Index(console.DSPBufCWRX);
		}
		return parser.Error1;
	}

	public string ZZHW(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Index2Width(s);
			console.DSPBufDigRX = num;
			console.SetupForm.DSPDigRXBuffer = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return Width2Index(console.DSPBufDigRX);
		}
		return parser.Error1;
	}

	public string ZZHX(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Index2Width(s);
			console.DSPBufDigTX = num;
			console.SetupForm.DSPDigTXBuffer = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return Width2Index(console.DSPBufDigTX);
		}
		return parser.Error1;
	}

	public string ZZID()
	{
		console.SetupForm.CATSetRig("PowerSDR");
		return "";
	}

	public string ZZIF(string s)
	{
		string text = "0";
		string text2 = "0";
		string text3 = "0";
		int num = 0;
		if (console.RITOn)
		{
			text = "1";
		}
		else if (console.XITOn)
		{
			text2 = "1";
		}
		if (text == "1")
		{
			num = console.RITValue;
		}
		else if (text2 == "1")
		{
			num = console.XITValue;
		}
		string text4 = ((num >= 0) ? ("+" + Convert.ToString(Math.Abs(num)).PadLeft(5, '0')) : ("-" + Convert.ToString(Math.Abs(num)).PadLeft(5, '0')));
		if (console.MOX)
		{
			text3 = "1";
		}
		int tuneStepIndex = console.TuneStepIndex;
		string text5 = Step2String(tuneStepIndex);
		string text6 = "0";
		if (console.VFOSplit)
		{
			text6 = "1";
		}
		string text7 = ZZFA("");
		if (text7.Length > 11)
		{
			text7 = text7.Substring(text7.Length - 11, 11);
		}
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + text7, text5), text4), text), text2), "000"), text3), Mode2String(console.RX1DSPMode)), "0"), "0"), text6), "0000");
	}

	public string ZZIO()
	{
		return "000";
	}

	public string ZZIS(string s)
	{
		int cATFilterWidth = 0;
		if (s != "")
		{
			cATFilterWidth = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			console.CATFilterWidth = cATFilterWidth;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.CATFilterWidth);
		}
		return parser.Error1;
	}

	public string ZZIT(string s)
	{
		int cATFilterShift = 0;
		string text = "-";
		if (s != "")
		{
			cATFilterShift = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			console.CATFilterShift = cATFilterShift;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			cATFilterShift = console.CATFilterShift;
			if (cATFilterShift >= 0)
			{
				text = "+";
			}
			return text + AddLeadingZeros(Math.Abs(cATFilterShift)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZIU()
	{
		console.CATFilterShiftReset = 1;
		return "";
	}

	public string ZZKO(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "1")
			{
				console.CATCWXForm = true;
				return "";
			}
			if (s == "0")
			{
				console.CATCWXForm = false;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATCWXForm)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZKM(string s)
	{
		int num = 0;
		if (s != "0" && s.Length > 0)
		{
			num = Convert.ToInt32(s);
			if (num > 0 || num < 10)
			{
				console.CWXForm.StartQueue = num;
				return "";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZKS(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			console.CWXForm.WPM = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.CWXForm.WPM);
		}
		return parser.Error1;
	}

	public string ZZKY(string s)
	{
		if (s.Length == parser.nSet)
		{
			switch (console.RX1DSPMode)
			{
			case DSPMode.LSB:
			case DSPMode.USB:
			case DSPMode.DSB:
			case DSPMode.FM:
			case DSPMode.AM:
			case DSPMode.SPEC:
			case DSPMode.SAM:
			case DSPMode.DRM:
				if (console.RX1Band >= Band.B160M && console.RX1Band <= Band.B40M)
				{
					console.RX1DSPMode = DSPMode.CWL;
				}
				else
				{
					console.RX1DSPMode = DSPMode.CWU;
				}
				break;
			default:
				console.RX1DSPMode = DSPMode.CWU;
				break;
			case DSPMode.CWL:
			case DSPMode.CWU:
				break;
			}
			string text = "";
			text = ((s.Trim().Length != 0) ? s.TrimEnd() : " ");
			if (text.Length > 1)
			{
				byte[] bytes = AE.GetBytes(text);
				return console.CWXForm.RemoteMessage(bytes);
			}
			char msg = Convert.ToChar(text);
			return console.CWXForm.RemoteMessage(msg);
		}
		if (s.Length == parser.nGet)
		{
			int characters2Send = console.CWXForm.Characters2Send;
			if (characters2Send > 0 && characters2Send < 72)
			{
				return "0";
			}
			if (characters2Send >= 72)
			{
				return "1";
			}
			if (characters2Send == 0)
			{
				return "2";
			}
			return parser.Error1;
		}
		return parser.Error1;
	}

	public string ZZLA(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(100, val);
		if (s.Length == parser.nSet)
		{
			console.RX0Gain = val;
			console.TitleBarEncoderString = "RX1 AF Gain = " + val + "%";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.RX0Gain);
		}
		return parser.Error1;
	}

	public string ZZLB(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(100, val);
		if (s.Length == parser.nSet)
		{
			console.PanMainRX = val;
			if (val == 50)
			{
				console.TitleBarEncoderString = "RX1 Stereo Balance = MID";
			}
			else if (val < 50)
			{
				console.TitleBarEncoderString = "RX1 Stereo Balance = left " + 2 * (50 - val) + "%";
			}
			else
			{
				console.TitleBarEncoderString = "RX1 Stereo Balance = right " + 2 * (val - 50) + "%";
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.PanMainRX);
		}
		return parser.Error1;
	}

	public string ZZLC(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(100, val);
		if (s.Length == parser.nSet)
		{
			console.RX1Gain = val;
			console.TitleBarEncoderString = "sub RX Gain = " + val + "%";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.RX1Gain);
		}
		return parser.Error1;
	}

	public string ZZLD(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(100, val);
		if (s.Length == parser.nSet)
		{
			console.PanSubRX = val;
			if (val == 50)
			{
				console.TitleBarEncoderString = "sub-RX Stereo Balance = MID";
			}
			else if (val < 50)
			{
				console.TitleBarEncoderString = "sub-RX Stereo Balance = left " + 2 * (50 - val) + "%";
			}
			else
			{
				console.TitleBarEncoderString = "sub-RX Stereo Balance = right " + 2 * (val - 50) + "%";
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.PanSubRX);
		}
		return parser.Error1;
	}

	public string ZZLE(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(100, val);
		if (s.Length == parser.nSet)
		{
			console.RX2Gain = val;
			console.TitleBarEncoderString = "RX2 AF Gain =" + val + "%";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.RX2Gain);
		}
		return parser.Error1;
	}

	public string ZZLF(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(100, val);
		if (s.Length == parser.nSet)
		{
			console.RX2Pan = val;
			if (val == 50)
			{
				console.TitleBarEncoderString = "RX2 Stereo Balance = MID";
			}
			else if (val < 50)
			{
				console.TitleBarEncoderString = "RX2 Stereo Balance = left " + 2 * (50 - val) + "%";
			}
			else
			{
				console.TitleBarEncoderString = "RX2 Stereo Balance = right " + 2 * (val - 50) + "%";
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.RX2Pan);
		}
		return parser.Error1;
	}

	public string ZZLG(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.SetupForm.AutoMuteRX1onVFOBTX = false;
			}
			else
			{
				console.SetupForm.AutoMuteRX1onVFOBTX = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.AutoMuteRX1onVFOBTX)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZLH(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.SetupForm.AutoMuteRX2onVFOATX = false;
			}
			else
			{
				console.SetupForm.AutoMuteRX2onVFOATX = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.AutoMuteRX2onVFOATX)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZLI(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.PSA = false;
			}
			else if (s == "1")
			{
				console.PSA = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.PSA)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZMA(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.MUT = false;
			}
			else if (s == "1")
			{
				console.MUT = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.MUT)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZMB(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.MUT2 = false;
			}
			else if (s == "1")
			{
				console.MUT2 = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.MUT2)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZMD(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (Convert.ToInt32(s) >= 0 && Convert.ToInt32(s) <= 11)
			{
				String2Mode(s);
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			return Mode2String(console.RX1DSPMode);
		}
		return parser.Error1;
	}

	public string ZZME(string s)
	{
		if (s.Length == parser.nGet)
		{
			return Mode2String(console.RX2DSPMode);
		}
		if (s.Length == parser.nSet && s != "08")
		{
			switch (s)
			{
			case "00":
				console.RX2DSPMode = DSPMode.LSB;
				break;
			case "01":
				console.RX2DSPMode = DSPMode.USB;
				break;
			case "02":
				console.RX2DSPMode = DSPMode.DSB;
				break;
			case "03":
				console.RX2DSPMode = DSPMode.CWL;
				break;
			case "04":
				console.RX2DSPMode = DSPMode.CWU;
				break;
			case "05":
				console.RX2DSPMode = DSPMode.FM;
				break;
			case "06":
				console.RX2DSPMode = DSPMode.AM;
				break;
			case "07":
				console.RX2DSPMode = DSPMode.DIGU;
				break;
			case "09":
				console.RX2DSPMode = DSPMode.DIGL;
				break;
			case "10":
				console.RX2DSPMode = DSPMode.SAM;
				break;
			case "11":
				console.RX2DSPMode = DSPMode.DRM;
				break;
			}
			return "";
		}
		return parser.Error1;
	}

	public string ZZMF(string s)
	{
		if (s.Length == parser.nSet)
		{
			string text = "";
			for (int i = 0; i < parser.nSet / 2; i++)
			{
				int num = Convert.ToInt32(s.Substring(2 * i, 2), 10);
				text += char.ConvertFromUtf32(num + 32);
			}
			console.TitleBarMultifunctionString = "   multifunction encoder = " + text;
			return "";
		}
		return parser.Error1;
	}

	public string ZZZD(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Convert.ToInt32(s);
			console.HandleFrontPanelVFOEncoderStep(-num);
			return "";
		}
		return parser.Error1;
	}

	public string ZZZU(string s)
	{
		if (s.Length == parser.nSet)
		{
			int steps = Convert.ToInt32(s);
			console.HandleFrontPanelVFOEncoderStep(steps);
			return "";
		}
		return parser.Error1;
	}

	public string ZZZS(string s)
	{
		if (s.Length == parser.nSet)
		{
			int handleAttachedHardwareID = Convert.ToInt32(s);
			console.HandleAttachedHardwareID = handleAttachedHardwareID;
			return "";
		}
		return parser.Error1;
	}

	public string ZZZE(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Convert.ToInt32(s);
			int num2 = num % 10;
			num /= 10;
			if (num >= 1 && num <= 20)
			{
				console.HandleFrontPanelEncoderStep(num - 1, num2);
			}
			else if (num >= 51 && num <= 70)
			{
				console.HandleFrontPanelEncoderStep(num - 51, -num2);
			}
			return "";
		}
		return parser.Error1;
	}

	public string ZZZP(string s)
	{
		if (s.Length == parser.nSet)
		{
			int num = Convert.ToInt32(s);
			bool state = false;
			bool longPress = false;
			if (num % 10 == 1)
			{
				state = true;
			}
			else if (num % 10 == 2)
			{
				longPress = true;
			}
			num /= 10;
			console.HandleFrontPanelButtonPress(num - 1, state, longPress);
			return "";
		}
		return parser.Error1;
	}

	public string ZZZA(string s)
	{
		if (s.Length == parser.nSet)
		{
			int tripState = Convert.ToInt32(s);
			console.CATHandleAmplifierTripMessage(tripState);
			return "";
		}
		return parser.Error1;
	}

	public string ZZOX(string s)
	{
		if (s.Length == parser.nSet)
		{
			bool tuneState = false;
			if (Convert.ToInt32(s) == 1)
			{
				tuneState = true;
			}
			console.CATHandleAriesTuneMessage(tuneState);
			return "";
		}
		return parser.Error1;
	}

	public string ZZOZ(string s)
	{
		if (s.Length == parser.nSet)
		{
			bool eraseState = false;
			if (Convert.ToInt32(s) == 1)
			{
				eraseState = true;
			}
			console.CATHandleAriesEraseMessage(eraseState);
			return "";
		}
		return parser.Error1;
	}

	public string ZZMG(string s)
	{
		int cATMIC = 0;
		if (s != "")
		{
			cATMIC = Convert.ToInt32(s);
			cATMIC = Math.Min(70, cATMIC);
			cATMIC = Math.Max(-50, cATMIC);
		}
		if (s.Length == parser.nSet || s.Length == parser.nSet + 1)
		{
			console.CATMIC = cATMIC;
			console.TitleBarEncoderString = "Mic Gain = " + console.CATMIC + "dB";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			string text = ((console.CATMIC < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(console.CATMIC)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZML()
	{
		string text = "";
		string text2 = "";
		try
		{
			DSPMode[] array = (DSPMode[])Enum.GetValues(typeof(DSPMode));
			int num = 0;
			for (num = 0; array[num] != DSPMode.LAST; num++)
			{
				if (array[num] != DSPMode.FIRST && array[num] != DSPMode.LAST)
				{
					if (array[num] <= DSPMode.DIGL)
					{
						string text3 = array[num].ToString();
						int num2 = (int)array[num];
						text2 = text3 + "0" + num2 + ":";
					}
					else
					{
						string text4 = array[num].ToString();
						int num2 = (int)array[num];
						text2 = text4 + num2 + ":";
					}
				}
				text += text2.PadLeft(7, ' ');
			}
			return text.Remove(text.Length - 1);
		}
		catch
		{
			parser.Verbose_Error_Code = 4;
			return parser.Error1;
		}
	}

	public string ZZMN(string s)
	{
		if (s.Length == parser.nGet)
		{
			return console.GetFilterPresets(int.Parse(s));
		}
		return parser.Error1;
	}

	public string ZZMO(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.MON = false;
			}
			else if (s == "1")
			{
				console.MON = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.MON)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZMR(string s)
	{
		int num = -1;
		if (s != "")
		{
			num = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet && num > -1 && num < 7)
		{
			String2RXMeter(num);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return RXMeter2String();
		}
		return parser.Error1;
	}

	public string ZZMS(string s)
	{
		if (s.Length == parser.nSet && (s == "1" || s == "0"))
		{
			if (s == "1")
			{
				console.CATPanSwap = "1";
			}
			else if (s == "0")
			{
				console.CATPanSwap = "0";
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATPanSwap;
		}
		return parser.Error1;
	}

	public string ZZMT(string s)
	{
		int num = -1;
		if (s != "")
		{
			num = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet && num > -1 && num < 15)
		{
			String2TXMeter(num);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return TXMeter2String().PadLeft(2, '0');
		}
		return parser.Error1;
	}

	public string ZZMU(string s)
	{
		if (s.Length == parser.nSet && (s == "1" || s == "0"))
		{
			if (s == "1")
			{
				console.CATMultRX = "1";
			}
			else if (s == "0")
			{
				console.CATMultRX = "0";
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATMultRX;
		}
		return parser.Error1;
	}

	public string ZZMV()
	{
		try
		{
			return AddLeadingZeros(console.MemoryList.List.Count);
		}
		catch
		{
			parser.Verbose_Error_Code = 4;
			return parser.Error1;
		}
	}

	public string ZZMW(string s)
	{
		try
		{
			MemoryRecord channelRecord = GetChannelRecord(s);
			if (channelRecord.Comments.StartsWith(s))
			{
				console.MemoryList.List.Remove(channelRecord);
			}
			return "";
		}
		catch
		{
			parser.Verbose_Error_Code = 4;
			return parser.Error1;
		}
	}

	public string ZZMX(string s)
	{
		try
		{
			int index = GetIndex(s);
			if (index >= 0)
			{
				console.changeComboFMMemory(index);
				return "";
			}
			parser.Verbose_Error_Code = 9;
			return parser.Error1;
		}
		catch
		{
			parser.Verbose_Error_Code = 4;
			return parser.Error1;
		}
	}

	public string ZZMY()
	{
		try
		{
			int nextChannelNumber = GetNextChannelNumber();
			int nAns = parser.nAns;
			parser.nAns = 3;
			string text = AddLeadingZeros(nextChannelNumber);
			console.MemoryList.List.Add(new MemoryRecord("", console.VFOAFreq, "", console.RX1DSPMode, _scan: true, console.TuneStepList[console.TuneStepIndex].Name, console.CurrentFMTXMode, console.FMTXOffsetMHz, console.radio.GetDSPTX(0).CTCSSFlag, console.radio.GetDSPTX(0).CTCSSFreqHz, console.PWR, (int)console.radio.GetDSPTX(0).TXFMDeviation, console.VFOSplit, console.TXFreq, console.RX1Filter, console.RX1FilterLow, console.RX1FilterHigh, text + ":", console.radio.GetDSPRX(0, 0).RXAGCMode, console.RF, DateTime.Now, _ScheduleOn: false, 0, _Repeating: false, _Recording: false, _Repeatingm: false, 0));
			parser.nAns = nAns;
			return "";
		}
		catch
		{
			parser.Verbose_Error_Code = 4;
			return parser.Error1;
		}
	}

	public string ZZMZ(string s)
	{
		if (GetIndex(s) >= 0)
		{
			try
			{
				MemoryRecord channelRecord = GetChannelRecord(s);
				MemoryRecord item = new MemoryRecord
				{
					Group = channelRecord.Group,
					RXFreq = console.VFOAFreq,
					Name = channelRecord.Name,
					DSPMode = console.RX1DSPMode,
					Scan = channelRecord.Scan,
					TuneStep = console.TuneStepList[console.TuneStepIndex].Name,
					RPTR = console.CurrentFMTXMode,
					RPTROffset = console.FMTXOffsetMHz,
					CTCSSOn = console.radio.GetDSPTX(0).CTCSSFlag,
					CTCSSFreq = console.radio.GetDSPTX(0).CTCSSFreqHz,
					Power = console.PWR,
					Deviation = (int)console.radio.GetDSPTX(0).TXFMDeviation,
					Split = console.VFOSplit,
					TXFreq = console.TXFreq,
					RXFilter = console.RX1Filter,
					RXFilterLow = console.RX1FilterLow,
					RXFilterHigh = console.RX1FilterHigh,
					Comments = channelRecord.Comments,
					AGCMode = console.radio.GetDSPRX(0, 0).RXAGCMode,
					AGCT = console.RF
				};
				console.MemoryList.List.Remove(channelRecord);
				console.MemoryList.List.Add(item);
				return "";
			}
			catch
			{
				parser.Verbose_Error_Code = 4;
				return parser.Error1;
			}
		}
		parser.Verbose_Error_Code = 9;
		return parser.Error1;
	}

	public string ZZJS()
	{
		if (console == null)
		{
			return parser.Error1;
		}
		string error;
		try
		{
			console.ARP.StopRecord(out error);
		}
		catch
		{
		}
		try
		{
			console.ARP.StopPlayback(out error);
		}
		catch
		{
		}
		return "";
	}

	public string ZZJR(string s)
	{
		if (console == null)
		{
			return parser.Error1;
		}
		if (s.Length == parser.nSet)
		{
			string error;
			try
			{
				console.ARP.StopRecord(out error);
			}
			catch
			{
			}
			try
			{
				console.ARP.StopPlayback(out error);
			}
			catch
			{
			}
			int result = 0;
			int result2 = 0;
			int result3 = 0;
			int result4 = 0;
			bool flag = !string.IsNullOrWhiteSpace(s);
			if (flag)
			{
				flag = s.Length == 6;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(0, 1), out result);
			}
			if (flag)
			{
				flag = result >= 1 && result <= 2;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(1, 1), out result3);
			}
			if (flag)
			{
				flag = result3 >= 0 && result3 <= 1;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(2, 3), out result2);
			}
			if (flag)
			{
				flag = result2 >= 1 && result2 <= 128;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(5, 1), out result4);
			}
			if (flag)
			{
				flag = result4 >= 0 && result4 <= 1;
			}
			if (flag)
			{
				int wfw_id;
				Band b;
				double num;
				DSPMode dSPMode;
				switch (result)
				{
				case 1:
					wfw_id = 0;
					b = console.RX1Band;
					num = console.VFOAFreq;
					dSPMode = console.RX1DSPMode;
					break;
				case 2:
					wfw_id = 1;
					b = console.RX2Band;
					num = console.VFOBFreq;
					dSPMode = console.RX2DSPMode;
					break;
				default:
					wfw_id = 0;
					b = console.RX1Band;
					num = console.VFOAFreq;
					dSPMode = console.RX1DSPMode;
					break;
				}
				RecordingDetails details = new RecordingDetails
				{
					Band = BandStackManager.BandToString(b),
					Frequency = num.ToString("F6", CultureInfo.InvariantCulture),
					Mode = dSPMode.ToString(),
					UtcTime = DateTime.UtcNow
				};
				string full_path = "cat\\slot_" + result2 + ".wav";
				string value = ((result3 != 0) ? console.ARP.RecordToFileFromPCAudio("cat", full_path, console.ARP.InputPCDeviceID, out var error2, remove_if_file_exists: true, details) : console.ARP.RecordToFileFromWDSP("cat", full_path, wfw_id, out error2, remove_if_file_exists: true, details, result4 == 1));
				if (!string.IsNullOrEmpty(value) && error2 == null)
				{
					return "";
				}
				return parser.Error1;
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (!console.ARP.IsRecording)
			{
				return "0";
			}
			return "1";
		}
		return "";
	}

	public string ZZJP(string s)
	{
		if (console == null)
		{
			return parser.Error1;
		}
		if (s.Length == parser.nSet)
		{
			string error;
			try
			{
				console.ARP.StopRecord(out error);
			}
			catch
			{
			}
			try
			{
				console.ARP.StopPlayback(out error);
			}
			catch
			{
			}
			int result = 0;
			int result2 = 0;
			int result3 = 0;
			int result4 = 0;
			int result5 = 0;
			string text = null;
			bool flag = !string.IsNullOrWhiteSpace(s);
			if (flag)
			{
				flag = s.Length == 9;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(0, 1), out result);
			}
			if (flag)
			{
				flag = result >= 1 && result <= 2;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(1, 1), out result3);
			}
			if (flag)
			{
				flag = result3 >= 0 && result3 <= 1;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(2, 3), out result2);
			}
			if (flag)
			{
				flag = result2 >= 1 && result2 <= 128;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(5, 1), out result4);
			}
			if (flag)
			{
				flag = result4 >= 0 && result4 <= 1;
			}
			if (result3 == 0)
			{
				if (flag)
				{
					text = s.Substring(6, 3);
				}
				if (flag)
				{
					flag = text[0] == '+' || text[0] == '-';
				}
				if (flag)
				{
					flag = char.IsDigit(text[1]) && char.IsDigit(text[2]);
				}
				if (flag)
				{
					flag = int.TryParse(text, out result5);
				}
				if (flag)
				{
					flag = result5 >= -70 && result5 <= 70;
				}
			}
			if (flag)
			{
				int wfw_id = result switch
				{
					1 => 0, 
					2 => 1, 
					_ => 0, 
				};
				string full_path = "cat\\slot_" + result2 + ".wav";
				string error2 = null;
				try
				{
					flag = ((result3 != 0) ? console.ARP.PlayFileViaPCAudio("cat", full_path, console.ARP.OutputPCDeviceID, out error2) : console.ARP.PlayFileViaWDSP("cat", full_path, wfw_id, out error2, result5, result4 == 1));
				}
				catch
				{
					flag = false;
				}
				if (flag && error2 == null)
				{
					return "";
				}
				return parser.Error1;
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (!console.ARP.IsPlaying)
			{
				return "0";
			}
			return "1";
		}
		return "";
	}

	public string ZZJQ(string s)
	{
		if (console == null)
		{
			return parser.Error1;
		}
		if (s.Length == parser.nSet)
		{
			string error;
			try
			{
				console.ARP.StopRecord(out error);
			}
			catch
			{
			}
			try
			{
				console.ARP.StopPlayback(out error);
			}
			catch
			{
			}
			int result = 0;
			int result2 = 0;
			bool flag = !string.IsNullOrWhiteSpace(s);
			MeterManager.clsVoiceRecordPlay clsVoiceRecordPlay = null;
			if (flag)
			{
				flag = s.Length == 8;
			}
			if (flag)
			{
				clsVoiceRecordPlay = MeterManager.GetVoiceRecordPlayFrom4Char(s.Substring(0, 4));
				flag = clsVoiceRecordPlay != null;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(4, 1), out result2);
			}
			if (flag)
			{
				flag = result2 >= 0 && result2 <= 1;
			}
			if (flag)
			{
				flag = int.TryParse(s.Substring(5, 3), out result);
			}
			if (flag)
			{
				flag = result >= 1 && result <= clsVoiceRecordPlay.Slots;
			}
			if (flag)
			{
				try
				{
					if (result2 == 1)
					{
						clsVoiceRecordPlay.RecordToSlot(result - 1);
					}
					else
					{
						clsVoiceRecordPlay.PlayFromSlot(result - 1);
					}
					return "";
				}
				catch
				{
					return parser.Error1;
				}
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (!console.ARP.IsBusy)
			{
				return "0";
			}
			return "1";
		}
		return "";
	}

	public string ZZNA(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATNB1 = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATNB1.ToString();
		}
		return parser.Error1;
	}

	public string ZZNB(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATNB2 = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATNB2.ToString();
		}
		return parser.Error1;
	}

	public string ZZNC(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATRX2NB1 = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATRX2NB1.ToString();
		}
		return parser.Error1;
	}

	public string ZZND(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATRX2NB2 = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATRX2NB2.ToString();
		}
		return parser.Error1;
	}

	public string ZZNL(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.CATNB1Threshold = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.CATNB1Threshold);
		}
		return parser.Error1;
	}

	public string ZZNM(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.CATNB2Threshold = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.CATNB2Threshold);
		}
		return parser.Error1;
	}

	public string ZZNN(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATSNB = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATSNB.ToString();
		}
		return parser.Error1;
	}

	public string ZZNO(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATRX2SNB = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATRX2SNB.ToString();
		}
		return parser.Error1;
	}

	public string ZZNR(string s)
	{
		int cATNR = 0;
		if (s != "")
		{
			cATNR = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATNR = cATNR;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATNR.ToString();
		}
		return parser.Error1;
	}

	public string ZZNS(string s)
	{
		int cATNR = 0;
		if (s != "")
		{
			cATNR = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATNR2 = cATNR;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATNR2.ToString();
		}
		return parser.Error1;
	}

	public string ZZNE(string s)
	{
		int nr = 0;
		if (s != "")
		{
			nr = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			switch (s)
			{
			case "0":
			case "1":
			case "2":
			case "3":
			case "4":
				console.SelectNR(1, incude_sub: true, nr);
				return "";
			}
		}
		if (s.Length == parser.nGet)
		{
			return console.GetSelectedNR(1).ToString();
		}
		return parser.Error1;
	}

	public string ZZNF(string s)
	{
		int nr = 0;
		if (s != "")
		{
			nr = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			switch (s)
			{
			case "0":
			case "1":
			case "2":
			case "3":
			case "4":
				console.SelectNR(2, incude_sub: true, nr);
				return "";
			}
		}
		if (s.Length == parser.nGet)
		{
			return console.GetSelectedNR(2).ToString();
		}
		return parser.Error1;
	}

	public string ZZNG(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			if (!console.IsSetupFormNull)
			{
				num = Convert.ToInt32(s);
				if (num < 0)
				{
					num = 0;
				}
				if (num > 100)
				{
					num = 100;
				}
				float dB = 0.2f * (float)num;
				if (console.SetupForm.InvokeRequired)
				{
					console.SetupForm.Invoke((MethodInvoker)delegate
					{
						console.SetupForm.NR4RedcutionAmmountRX1 = dB;
					});
				}
				else
				{
					console.SetupForm.NR4RedcutionAmmountRX1 = dB;
				}
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (!console.IsSetupFormNull)
			{
				float dB2 = 0f;
				if (console.SetupForm.InvokeRequired)
				{
					console.SetupForm.Invoke((MethodInvoker)delegate
					{
						dB2 = console.SetupForm.NR4RedcutionAmmountRX1;
					});
				}
				else
				{
					dB2 = console.SetupForm.NR4RedcutionAmmountRX1;
				}
				int n = (int)(dB2 / 20f * 100f);
				return AddLeadingZeros(n);
			}
			return AddLeadingZeros(0);
		}
		return parser.Error1;
	}

	public string ZZNH(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			if (!console.IsSetupFormNull)
			{
				num = Convert.ToInt32(s);
				if (num < 0)
				{
					num = 0;
				}
				if (num > 100)
				{
					num = 100;
				}
				float dB = 0.2f * (float)num;
				if (console.SetupForm.InvokeRequired)
				{
					console.SetupForm.Invoke((MethodInvoker)delegate
					{
						console.SetupForm.NR4RedcutionAmmountRX2 = dB;
					});
				}
				else
				{
					console.SetupForm.NR4RedcutionAmmountRX2 = dB;
				}
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (!console.IsSetupFormNull)
			{
				float dB2 = 0f;
				if (console.SetupForm.InvokeRequired)
				{
					console.SetupForm.Invoke((MethodInvoker)delegate
					{
						dB2 = console.SetupForm.NR4RedcutionAmmountRX2;
					});
				}
				else
				{
					dB2 = console.SetupForm.NR4RedcutionAmmountRX2;
				}
				int n = (int)(dB2 / 20f * 100f);
				return AddLeadingZeros(n);
			}
			return AddLeadingZeros(0);
		}
		return parser.Error1;
	}

	public string ZZNT(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATANF = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATANF.ToString();
		}
		return parser.Error1;
	}

	public string ZZNU(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATRX2ANF = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATRX2ANF.ToString();
		}
		return parser.Error1;
	}

	public string ZZNV(string s)
	{
		int cATRX2NR = 0;
		if (s != "")
		{
			cATRX2NR = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATRX2NR = cATRX2NR;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATRX2NR.ToString();
		}
		return parser.Error1;
	}

	public string ZZNW(string s)
	{
		int cATRX2NR = 0;
		if (s != "")
		{
			cATRX2NR = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATRX2NR2 = cATRX2NR;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATRX2NR2.ToString();
		}
		return parser.Error1;
	}

	public string ZZOA(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(1, val);
		val = Math.Min(3, val);
		if (s.Length == parser.nSet)
		{
			if (console.SetupForm != null)
			{
				console.SetupForm.SetRXAntenna(val, console.RX1Band);
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm != null)
			{
				return console.SetupForm.GetRXAntenna(console.RX1Band).ToString();
			}
			return "";
		}
		return parser.Error1;
	}

	public string ZZOB(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZOC(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(1, val);
		val = Math.Min(3, val);
		if (s.Length == parser.nSet)
		{
			if (console.SetupForm != null)
			{
				console.SetupForm.SetTXAntenna(val, console.TXBand);
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm != null)
			{
				return console.SetupForm.GetTXAntenna(console.TXBand).ToString();
			}
			return "";
		}
		return parser.Error1;
	}

	public string ZZOD(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZOE(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZOF(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZOG(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZOH(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZOJ(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZOL(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(10000, val);
		if (s.Length == parser.nSet)
		{
			console.SetupForm.DigL_CT_Offset = val;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.DigL_CT_Offset);
		}
		return parser.Error1;
	}

	public string ZZOS(string s)
	{
		if (s.Length == parser.nSet)
		{
			String2OffsetDirection(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return OffsetDirection2String();
		}
		return parser.Error1;
	}

	public string ZZOT(string s)
	{
		if (s.Length == parser.nSet)
		{
			s = s.Insert(3, ".");
			double fMTXOffsetMHz = double.Parse(s);
			console.FMTXOffsetMHz = fMTXOffsetMHz;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int n = Convert.ToInt32(Math.Abs(console.FMTXOffsetMHz * 1000000.0));
			return AddLeadingZeros(n);
		}
		return parser.Error1;
	}

	public string ZZOU(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(10000, val);
		if (s.Length == parser.nSet)
		{
			console.SetupForm.DigU_CT_Offset = val;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.DigU_CT_Offset);
		}
		return parser.Error1;
	}

	public string ZZOV(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZOW(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZPA(string s)
	{
		int num = 0;
		if (s != "")
		{
			num = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			if (num > -1 && num < 10)
			{
				switch (s)
				{
				case "0":
					console.CATPreamp = PreampMode.HPSDR_OFF;
					break;
				case "1":
					console.CATPreamp = PreampMode.HPSDR_ON;
					break;
				case "2":
					console.CATPreamp = PreampMode.HPSDR_MINUS10;
					break;
				case "3":
					console.CATPreamp = PreampMode.HPSDR_MINUS20;
					break;
				case "4":
					console.CATPreamp = PreampMode.HPSDR_MINUS30;
					break;
				case "5":
					console.CATPreamp = PreampMode.HPSDR_MINUS40;
					break;
				case "6":
					console.CATPreamp = PreampMode.HPSDR_MINUS50;
					break;
				}
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			return ((int)console.CATPreamp).ToString();
		}
		return parser.Error1;
	}

	public string ZZPB(string s)
	{
		int num = 0;
		if (s != "")
		{
			num = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet)
		{
			if (num > -1 && num < 10)
			{
				switch (s)
				{
				case "0":
					console.RX2PreampMode = PreampMode.HPSDR_OFF;
					break;
				case "1":
					console.RX2PreampMode = PreampMode.HPSDR_ON;
					break;
				case "2":
					console.RX2PreampMode = PreampMode.HPSDR_MINUS10;
					break;
				case "3":
					console.RX2PreampMode = PreampMode.HPSDR_MINUS20;
					break;
				case "4":
					console.RX2PreampMode = PreampMode.HPSDR_MINUS30;
					break;
				}
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			return ((int)console.RX2PreampMode).ToString();
		}
		return parser.Error1;
	}

	public string ZZPC(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			console.PWR = num;
			console.TitleBarEncoderString = "Drive = " + num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SendLimitedPowerLevels)
			{
				return AddLeadingZeros(console.PWRConstrained);
			}
			return AddLeadingZeros(console.PWR);
		}
		return parser.Error1;
	}

	public string ZZPD()
	{
		console.CATDispCenter = "1";
		return "";
	}

	public string ZZPE(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(0, num);
			num = Math.Min(1000, num);
			console.Pan = num;
			console.TitleBarEncoderString = "Display pan =" + num / 10 + "%";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.Pan);
		}
		return parser.Error1;
	}

	public string ZZPO(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.CATDispPeak = s;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATDispPeak;
		}
		return parser.Error1;
	}

	public string ZZPS(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.PowerOn = false;
			}
			else if (s == "1")
			{
				console.PowerOn = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.PowerOn)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZPY(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(10, num);
			num = Math.Min(240, num);
			console.Zoom = num;
			console.TitleBarEncoderString = "Display zoom =" + Convert.ToInt32((double)num / 2.4) + "%";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.Zoom);
		}
		return parser.Error1;
	}

	public string ZZPZ(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.CATDispZoom = s;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATDispZoom;
		}
		return parser.Error1;
	}

	public string ZZQK(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATQSKBreakIn = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATQSKBreakIn.ToString();
		}
		return parser.Error1;
	}

	public string ZZQM()
	{
		return StrVFOFreq("C");
	}

	public string ZZQR()
	{
		console.CATMemoryQR();
		return "";
	}

	public string ZZQS()
	{
		console.CATMemoryQS();
		return "";
	}

	public string ZZRA(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.SetupForm.RttyOffsetEnabledA = false;
			}
			else if (s == "1")
			{
				console.SetupForm.RttyOffsetEnabledA = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.RttyOffsetEnabledA)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZRB(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.SetupForm.RttyOffsetEnabledB = false;
			}
			else if (s == "1")
			{
				console.SetupForm.RttyOffsetEnabledB = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.RttyOffsetEnabledB)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZRC()
	{
		console.RITValue = 0;
		return "";
	}

	public string ZZRD(string s)
	{
		if (s.Length == parser.nSet)
		{
			return ZZRF(s);
		}
		if (s.Length == parser.nGet)
		{
			console.RITValue -= 10;
			return "";
		}
		return parser.Error1;
	}

	public string ZZRF(string s)
	{
		int rITValue = 0;
		int num = 0;
		if (s != "")
		{
			rITValue = Convert.ToInt32(s);
			rITValue = Math.Max(-99999, rITValue);
			rITValue = Math.Min(99999, rITValue);
		}
		if (s.Length == parser.nSet)
		{
			console.RITValue = rITValue;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.RITValue;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZRH(string s)
	{
		int rttyOffsetHigh = 0;
		int num = 0;
		if (s != "")
		{
			rttyOffsetHigh = Convert.ToInt32(s);
			rttyOffsetHigh = Math.Max(-3000, rttyOffsetHigh);
			rttyOffsetHigh = Math.Min(3000, rttyOffsetHigh);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.RttyOffsetHigh = rttyOffsetHigh;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.SetupForm.RttyOffsetHigh;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZRL(string s)
	{
		int rttyOffsetLow = 0;
		int num = 0;
		if (s != "")
		{
			rttyOffsetLow = Convert.ToInt32(s);
			rttyOffsetLow = Math.Max(-3000, rttyOffsetLow);
			rttyOffsetLow = Math.Min(3000, rttyOffsetLow);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.RttyOffsetLow = rttyOffsetLow;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.SetupForm.RttyOffsetLow;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZRM(string s)
	{
		string result = parser.Error1;
		if (!console.MOX)
		{
			switch (s)
			{
			case "0":
				result = console.CATReadSigStrength().PadLeft(20);
				break;
			case "1":
				result = console.CATReadAvgStrength().PadLeft(20);
				break;
			case "2":
				result = console.CATReadADC_L().PadLeft(20);
				break;
			case "3":
				result = console.CATReadADC_R().PadLeft(20);
				break;
			}
		}
		else
		{
			switch (s)
			{
			case "4":
				result = console.CATReadALC().PadLeft(20);
				break;
			case "5":
				result = console.CATReadFwdPwr().PadLeft(20);
				break;
			case "6":
				result = parser.Error1;
				break;
			case "7":
				result = console.CATReadRevPwr().PadLeft(20);
				break;
			case "8":
				result = console.CATReadSWR().PadLeft(20);
				break;
			}
		}
		return result;
	}

	public string ZZRS(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.RX2Enabled = false;
			}
			else if (s == "1")
			{
				console.RX2Enabled = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.RX2Enabled)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZRT(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.RITOn = false;
			}
			else if (s == "1")
			{
				console.RITOn = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.RITOn)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZRU(string s)
	{
		if (s.Length == parser.nSet)
		{
			return ZZRF(s);
		}
		if (s.Length == parser.nGet)
		{
			console.RITValue += 10;
			return "";
		}
		return parser.Error1;
	}

	public string ZZRV()
	{
		if (HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 || HardwareSpecific.Model == HPSDRModel.ANAN_G2_1K)
		{
			return $"{console.MKIIPAVolts:00.0}";
		}
		if (HardwareSpecific.Model != HPSDRModel.HPSDR)
		{
			return "00.0";
		}
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZRX(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(0, num);
			num = Math.Min(31, num);
			console.SetupForm.ATTOnRX1 = num;
			console.TitleBarEncoderString = "RX1 Step Atten = " + num + "dB";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.ATTOnRX1);
		}
		return parser.Error1;
	}

	public string ZZRY(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(0, num);
			num = Math.Min(31, num);
			console.SetupForm.ATTOnRX2 = num;
			console.TitleBarEncoderString = "RX2 Step Atten = " + num + "dB";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.ATTOnRX2);
		}
		return parser.Error1;
	}

	public string ZZSA()
	{
		try
		{
			int tuneStepIndex = console.TuneStepIndex;
			List<TuneStep> tuneStepList = console.TuneStepList;
			console.VFOAFreq = console.CATVFOA - (double)tuneStepList[tuneStepIndex].StepHz * 1E-06;
			return "";
		}
		catch (Exception)
		{
			return parser.Error1;
		}
	}

	public string ZZSB()
	{
		try
		{
			int tuneStepIndex = console.TuneStepIndex;
			List<TuneStep> tuneStepList = console.TuneStepList;
			console.VFOAFreq = console.CATVFOA + (double)tuneStepList[tuneStepIndex].StepHz * 1E-06;
			return "";
		}
		catch (Exception)
		{
			return parser.Error1;
		}
	}

	public string ZZSD()
	{
		console.CATTuneStepDown();
		return "";
	}

	public string ZZSF(string s)
	{
		int center = Convert.ToInt32(s.Substring(0, 4), 10);
		int width = Convert.ToInt32(s.Substring(4), 10);
		SetFilterCenterAndWidth(center, width);
		return "";
	}

	public string ZZSG()
	{
		try
		{
			int tuneStepIndex = console.TuneStepIndex;
			List<TuneStep> tuneStepList = console.TuneStepList;
			console.VFOBFreq = console.CATVFOB - (double)tuneStepList[tuneStepIndex].StepHz * 1E-06;
			return "";
		}
		catch (Exception)
		{
			return parser.Error1;
		}
	}

	public string ZZSH()
	{
		try
		{
			int tuneStepIndex = console.TuneStepIndex;
			List<TuneStep> tuneStepList = console.TuneStepList;
			console.VFOBFreq = console.CATVFOB + (double)tuneStepList[tuneStepIndex].StepHz * 1E-06;
			return "";
		}
		catch (Exception)
		{
			return parser.Error1;
		}
	}

	public string ZZSM(string s)
	{
		int num = 0;
		if (s == "0" || s == "1")
		{
			float num2 = 0f;
			if (console.PowerOn)
			{
				num2 = ((!(s == "0")) ? WDSP.CalculateRXMeter(2u, 0u, WDSP.MeterType.SIGNAL_STRENGTH) : WDSP.CalculateRXMeter(0u, 0u, WDSP.MeterType.SIGNAL_STRENGTH));
			}
			if (HardwareSpecific.Model == HPSDRModel.HPSDR)
			{
				num2 = num2 + console.RX1MeterCalOffset + Display.RX1PreampOffset + console.RX1XVTRGainOffset;
			}
			else if (s == "0")
			{
				num2 = num2 + console.RX1MeterCalOffset + Display.RX1PreampOffset + console.RX1XVTRGainOffset;
			}
			else if (s == "1")
			{
				num2 = num2 + console.RX2MeterCalOffset + Display.RX2PreampOffset + console.RX2XVTRGainOffset;
			}
			num2 = Math.Max(-140f, num2);
			num2 = Math.Min(-10f, num2);
			return (((int)num2 + 140) * 2).ToString().PadLeft(3, '0');
		}
		return parser.Error1;
	}

	public string ZZSN()
	{
		return console.SetupForm.SerialNumber;
	}

	public string ZZSP(string s, bool bFromCatDirect = false)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (bFromCatDirect && !console.IsSetupFormNull && console.SetupForm.SplitFromCATorTCIcancelsQSPLIT && console.SetupForm.QuickSplitEnabled)
			{
				console.SetupForm.QuickSplitEnabled = false;
			}
			if (s == "0")
			{
				console.VFOSplit = false;
			}
			else
			{
				console.VFOSplit = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (!console.VFOSplit)
			{
				return "0";
			}
			return "1";
		}
		return parser.Error1;
	}

	public string ZZSO(string s)
	{
		if (s.Length == parser.nSet)
		{
			switch (s)
			{
			case "0":
			case "1":
			case "2":
				console.CATSquelch = Convert.ToInt32(s);
				return "";
			}
		}
		if (s.Length == parser.nGet)
		{
			return console.CATSquelch.ToString();
		}
		return parser.Error1;
	}

	public string ZZSQ(string s)
	{
		int num = 0;
		int nSet = 3;
		int nSet2 = 1;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			if (console.RX1DSPMode == DSPMode.FM)
			{
				num = Math.Max(0, num);
				num = Math.Min(100, num);
				num *= -1;
			}
			else
			{
				num = Math.Max(0, num);
				num = Math.Min(160, num);
			}
			string text = ZZSO("");
			if (text == "0")
			{
				parser.nSet = nSet2;
				ZZSO("1");
				parser.nSet = nSet;
			}
			console.Squelch = num * -1;
			console.TitleBarEncoderString = "RX1 Squelch = -" + num;
			parser.nSet = nSet2;
			ZZSO(text);
			parser.nSet = nSet;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(Math.Abs(console.Squelch));
		}
		return parser.Error1;
	}

	public string ZZSR(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return "0";
		}
		return parser.Error1;
	}

	public string ZZSS()
	{
		console.CWXForm.CWXStop();
		return "";
	}

	public string ZZST()
	{
		int tuneStepIndex = console.TuneStepIndex;
		return Step2String(tuneStepIndex);
	}

	public string ZZSU()
	{
		console.CATTuneStepUp();
		return "";
	}

	public string ZZSV(string s)
	{
		if (s.Length == parser.nSet)
		{
			switch (s)
			{
			case "0":
			case "1":
			case "2":
				console.CATSquelch2 = Convert.ToInt32(s);
				return "";
			}
		}
		if (s.Length == parser.nGet)
		{
			return console.CATSquelch2.ToString();
		}
		return parser.Error1;
	}

	public string ZZSW(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.SwapVFOA_BTX = false;
			}
			else if (s == "1")
			{
				console.SwapVFOA_BTX = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SwapVFOA_BTX)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZSX(string s)
	{
		int num = 0;
		int nSet = 3;
		int nSet2 = 1;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			if (console.RX2DSPMode == DSPMode.FM)
			{
				num = Math.Max(0, num);
				num = Math.Min(100, num);
				num *= -1;
			}
			else
			{
				num = Math.Max(0, num);
				num = Math.Min(160, num);
			}
			string text = ZZSV("");
			if (text == "0")
			{
				parser.nSet = nSet2;
				ZZSV("1");
				parser.nSet = nSet;
			}
			console.Squelch2 = num * -1;
			console.TitleBarEncoderString = "RX2 Squelch = -" + num;
			parser.nSet = nSet2;
			ZZSV(text);
			parser.nSet = nSet;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(Math.Abs(console.Squelch2));
		}
		return parser.Error1;
	}

	public string ZZSY(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.VFOSync = true;
			}
			else
			{
				console.VFOSync = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.VFOSync)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZSZ(string s)
	{
		int currentTuneStepHz = console.CurrentTuneStepHz;
		double freq_mhz = ((!(s == "0")) ? console.VFOBFreq : console.VFOAFreq);
		double num = console.SnapTune(freq_mhz, currentTuneStepHz, 1);
		if (s == "0")
		{
			console.VFOAFreq = num;
		}
		else
		{
			console.VFOBFreq = num;
		}
		return "";
	}

	public string ZZTA(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "1")
			{
				console.CTCSSOn = true;
				return "";
			}
			if (s == "0")
			{
				console.CTCSSOn = false;
				return "";
			}
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			if (console.CTCSSOn)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZTB(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (int.Parse(s) > 0 && int.Parse(s) <= 49)
			{
				console.CTCSSFreq = String2CTCSSFreq(s);
				return "";
			}
			parser.Verbose_Error_Code = 9;
			return parser.Error1;
		}
		if (s.Length == parser.nGet)
		{
			int freq = Convert.ToInt32(console.CTCSSFreq * 10.0);
			return CTCSSFreq2String(freq);
		}
		return parser.Error1;
	}

	public string ZZTF(string s)
	{
		DSPMode rX1DSPMode = console.RX1DSPMode;
		if ((uint)(rX1DSPMode - 3) <= 1u)
		{
			return parser.Error1;
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.ShowTXFilter = true;
			}
			else
			{
				console.ShowTXFilter = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.ShowTXFilter)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZTH(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(500, num);
			num = Math.Min(20000, num);
			console.SetupForm.TXFilterHigh = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.TXFilterHigh);
		}
		return parser.Error1;
	}

	public string ZZTI(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.RXOnly = false;
			}
			else if (s == "1")
			{
				console.RXOnly = true;
				console.MOX = false;
			}
			return "";
		}
		return parser.Error1;
	}

	public string ZZTL(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(0, num);
			num = Math.Min(2000, num);
			console.SetupForm.TXFilterLow = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.TXFilterLow);
		}
		return parser.Error1;
	}

	public string ZZTM(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(0, num);
			num = Math.Min(100, num);
			console.TXAF = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.TXAF);
		}
		return parser.Error1;
	}

	public string ZZTO(string s)
	{
		int num = 0;
		if (s.Length == parser.nSet)
		{
			num = Convert.ToInt32(s);
			num = Math.Max(0, num);
			num = Math.Min(100, num);
			switch (console.TuneDrivePowerOrigin)
			{
			case DrivePowerSource.DRIVE_SLIDER:
				console.PWR = num;
				break;
			case DrivePowerSource.TUNE_SLIDER:
				console.TunePWR = num;
				break;
			case DrivePowerSource.FIXED:
				if (!console.IsSetupFormNull)
				{
					console.SetupForm.FixedTunePower = num;
				}
				break;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			int n = 0;
			switch (console.TuneDrivePowerOrigin)
			{
			case DrivePowerSource.DRIVE_SLIDER:
				n = ((!console.SendLimitedPowerLevels) ? console.PWR : console.PWRConstrained);
				break;
			case DrivePowerSource.TUNE_SLIDER:
				n = ((!console.SendLimitedPowerLevels) ? console.TunePWR : console.TunePWRConstrained);
				break;
			case DrivePowerSource.FIXED:
				n = console.SetupForm.FixedTunePower;
				break;
			}
			return AddLeadingZeros(n);
		}
		return parser.Error1;
	}

	public string ZZTP(string s)
	{
		int cATTXProfileCount = console.CATTXProfileCount;
		int num = 0;
		if (s != "")
		{
			num = Convert.ToInt32(s);
		}
		if (s.Length == parser.nSet && num < cATTXProfileCount)
		{
			console.CATTXProfile = num;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.CATTXProfile);
		}
		return parser.Error1;
	}

	public string ZZTS()
	{
		if (HardwareSpecific.Model == HPSDRModel.HERMES)
		{
			float num = 0f;
			double num2 = 0.0;
			num = 0f / 4096f * 2.5f;
			double num3 = 301.0 - (double)(num * 1000f) / 2.2;
			return ((!(num3 >= 100.0)) ? Math.Round(num3, 2) : Math.Round(num3, 1)).ToString().PadLeft(5, '0');
		}
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZTU(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.TUN = false;
			}
			else if (s == "1")
			{
				console.TUN = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.TUN)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZTV(string s)
	{
		if (ZZRS("") == "1" && (ZZSP("") == "1" || ZZMU("") == "1"))
		{
			if (s.Length == parser.nSet)
			{
				console.VFOASubFreq = double.Parse(s) / 1000000.0;
				return "";
			}
			if (s.Length == parser.nGet)
			{
				int n = Convert.ToInt32(Math.Round(console.VFOASubFreq, 6) * 1000000.0);
				return AddLeadingZeros(n);
			}
			return parser.Error1;
		}
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZTX(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.CATPTT = false;
			}
			else if (s == "1")
			{
				console.CATPTT = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATPTT || console.MOX)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZUA()
	{
		return console.CATGetXVTRBandNames();
	}

	public string ZZVA(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.SetupForm.VACEnable = true;
			}
			else
			{
				console.SetupForm.VACEnable = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.VACEnable)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZUP(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (!(s == "0"))
			{
				if (s == "1")
				{
					console.CATxPA = true;
				}
			}
			else
			{
				console.CATxPA = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATxPA)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZUS()
	{
		console.CATSingleCal();
		return "";
	}

	public string ZZUT(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (!(s == "0"))
			{
				if (s == "1")
				{
					console.CATTTTest = 1;
				}
			}
			else
			{
				console.CATTTTest = 0;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATTTTest == 1)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVB(string s)
	{
		int vACRXGain = 0;
		int num = 0;
		if (s != "")
		{
			vACRXGain = Convert.ToInt32(s);
			vACRXGain = Math.Max(-40, vACRXGain);
			vACRXGain = Math.Min(40, vACRXGain);
		}
		if (s.Length == parser.nSet)
		{
			console.VACRXGain = vACRXGain;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.VACRXGain;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZVC(string s)
	{
		int vACTXGain = 0;
		int num = 0;
		if (s != "")
		{
			vACTXGain = Convert.ToInt32(s);
			vACTXGain = Math.Max(-40, vACTXGain);
			vACTXGain = Math.Min(40, vACTXGain);
		}
		if (s.Length == parser.nSet)
		{
			console.VACTXGain = vACTXGain;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.VACTXGain;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZVD(string s)
	{
		int num = -1;
		if (s.Length == parser.nSet)
		{
			switch (Convert.ToInt32(s))
			{
			case 0:
				console.VACSampleRate = "6000";
				break;
			case 1:
				console.VACSampleRate = "8000";
				break;
			case 2:
				console.VACSampleRate = "11025";
				break;
			case 3:
				console.VACSampleRate = "12000";
				break;
			case 4:
				console.VACSampleRate = "24000";
				break;
			case 5:
				console.VACSampleRate = "22050";
				break;
			case 6:
				console.VACSampleRate = "44100";
				break;
			case 7:
				console.VACSampleRate = "48000";
				break;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			string vACSampleRate = console.VACSampleRate;
			string text = "";
			return vACSampleRate switch
			{
				"6000" => "0", 
				"8000" => "1", 
				"11025" => "2", 
				"12000" => "3", 
				"24000" => "4", 
				"22050" => "5", 
				"44100" => "6", 
				"48000" => "7", 
				"96000" => "8", 
				"192000" => "9", 
				_ => parser.Error1, 
			};
		}
		return parser.Error1;
	}

	public string ZZVE(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.VOXEnable = true;
			}
			else
			{
				console.VOXEnable = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.VOXEnable)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZQA(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.QuickPlay = true;
			}
			else
			{
				console.QuickPlay = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.QuickPlay)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZQB(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.QuickRec = true;
			}
			else
			{
				console.QuickRec = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.QuickRec)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVF(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.VACStereo = true;
			}
			else
			{
				console.VACStereo = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.VACStereo)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVG(string s)
	{
		int val = 0;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		num = console.VOXSensExtent;
		num2 = console.VOXSensMin;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(1000, val);
		if (s.Length == parser.nSet)
		{
			num3 = (double)val / 1000.0;
			console.VOXSens = (int)(num2 + num * num3);
			console.TitleBarEncoderString = "Vox Gain = " + console.VOXSens + "dB";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			val = (int)(((double)console.VOXSens - num2) * 1000.0 / num);
			return AddLeadingZeros(val);
		}
		return parser.Error1;
	}

	public string ZZVH(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.SetupForm.IQOutToVAC = false;
			}
			else if (s == "1")
			{
				console.SetupForm.IQOutToVAC = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.IQOutToVAC)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVI(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.VACInputCable = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.VACInputCable);
		}
		return parser.Error1;
	}

	public string ZZVJ(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1") && console.SetupForm.IQOutToVAC)
		{
			if (s == "0")
			{
				console.SetupForm.VACUseRX2 = false;
			}
			else if (s == "1")
			{
				console.SetupForm.VACUseRX2 = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.VACUseRX2)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVK(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.SetupForm.VAC2Enable = true;
			}
			else
			{
				console.SetupForm.VAC2Enable = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.VAC2Enable)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVL(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			switch (console.VFOLock)
			{
			case CheckState.Unchecked:
				console.CATVFOLock = true;
				console.CATVFOBLock = false;
				console.VFOLock = CheckState.Checked;
				break;
			case CheckState.Checked:
				console.CATVFOLock = true;
				console.CATVFOBLock = true;
				console.VFOLock = CheckState.Indeterminate;
				break;
			case CheckState.Indeterminate:
				console.CATVFOLock = false;
				console.CATVFOBLock = false;
				console.VFOLock = CheckState.Unchecked;
				break;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.CATVFOLock || console.CATVFOBLock)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZUX(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (!(s == "0"))
			{
				if (s == "1")
				{
					console.VFOALock = true;
					console.CATVFOLock = true;
				}
			}
			else
			{
				console.VFOALock = false;
				console.CATVFOLock = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.VFOLock == CheckState.Checked || console.VFOALock || console.CATVFOLock)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZUY(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (!(s == "0"))
			{
				if (s == "1")
				{
					console.VFOBLock = true;
					console.CATVFOBLock = true;
				}
			}
			else
			{
				console.VFOBLock = false;
				console.CATVFOBLock = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.VFOBLock || console.VFOLock == CheckState.Indeterminate || console.CATVFOBLock)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVM(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.VACDriver = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.VACDriver);
		}
		return parser.Error1;
	}

	public string ZZVN()
	{
		return Common.GetFileVersion().PadLeft(12, '0');
	}

	public string ZZVO(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.VACOutputCable = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.VACOutputCable);
		}
		return parser.Error1;
	}

	public string ZZVP(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.SetupForm.VAC1Calibrate = false;
			}
			else if (s == "1")
			{
				console.SetupForm.VAC1Calibrate = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.VAC1Calibrate)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVQ(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.VAC2Driver = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.VAC2Driver);
		}
		return parser.Error1;
	}

	public string ZZVR(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.VAC2InputCable = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.VAC2InputCable);
		}
		return parser.Error1;
	}

	public string ZZVS(string s)
	{
		if ((s.Length == parser.nSet) & (Convert.ToInt32(s) <= 3))
		{
			console.CATVFOSwap(s);
			return "";
		}
		return parser.Error1;
	}

	public string ZZVT(string s)
	{
		if (s.Length == parser.nSet)
		{
			console.SetupForm.VAC2OutputCable = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.SetupForm.VAC2OutputCable);
		}
		return parser.Error1;
	}

	public string ZZVU(string s)
	{
		int num = -1;
		if (s.Length == parser.nSet)
		{
			switch (Convert.ToInt32(s))
			{
			case 0:
				console.SetupForm.VAC2SampleRate = "6000";
				break;
			case 1:
				console.SetupForm.VAC2SampleRate = "8000";
				break;
			case 2:
				console.SetupForm.VAC2SampleRate = "11025";
				break;
			case 3:
				console.SetupForm.VAC2SampleRate = "12000";
				break;
			case 4:
				console.SetupForm.VAC2SampleRate = "24000";
				break;
			case 5:
				console.SetupForm.VAC2SampleRate = "22050";
				break;
			case 6:
				console.SetupForm.VAC2SampleRate = "44100";
				break;
			case 7:
				console.SetupForm.VAC2SampleRate = "48000";
				break;
			case 8:
				console.SetupForm.VAC2SampleRate = "96000";
				break;
			case 9:
				console.SetupForm.VAC2SampleRate = "192000";
				break;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			string vAC2SampleRate = console.SetupForm.VAC2SampleRate;
			string text = "";
			return vAC2SampleRate switch
			{
				"6000" => "0", 
				"8000" => "1", 
				"11025" => "2", 
				"12000" => "3", 
				"24000" => "4", 
				"22050" => "5", 
				"44100" => "6", 
				"48000" => "7", 
				"96000" => "8", 
				"192000" => "9", 
				_ => parser.Error1, 
			};
		}
		return parser.Error1;
	}

	public string ZZVV(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "1")
			{
				console.SetupForm.VAC2Stereo = true;
			}
			else
			{
				console.SetupForm.VAC2Stereo = false;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.VAC2Stereo)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZVW(string s)
	{
		int vAC2RXGain = 0;
		int num = 0;
		if (s != "")
		{
			vAC2RXGain = Convert.ToInt32(s);
			vAC2RXGain = Math.Max(-40, vAC2RXGain);
			vAC2RXGain = Math.Min(40, vAC2RXGain);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.VAC2RXGain = vAC2RXGain;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.SetupForm.VAC2RXGain;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZVX(string s)
	{
		int vAC2TXGain = 0;
		int num = 0;
		if (s != "")
		{
			vAC2TXGain = Convert.ToInt32(s);
			vAC2TXGain = Math.Max(-40, vAC2TXGain);
			vAC2TXGain = Math.Min(40, vAC2TXGain);
		}
		if (s.Length == parser.nSet)
		{
			console.SetupForm.VAC2TXGain = vAC2TXGain;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.SetupForm.VAC2TXGain;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZVY(string s)
	{
		int num = -1;
		if (s.Length == parser.nSet)
		{
			switch (Convert.ToInt32(s))
			{
			case 0:
				console.SetupForm.VAC1BufferSize = "512";
				break;
			case 1:
				console.SetupForm.VAC1BufferSize = "1024";
				break;
			case 2:
				console.SetupForm.VAC1BufferSize = "2048";
				break;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			string vAC1BufferSize = console.SetupForm.VAC1BufferSize;
			string text = "";
			return vAC1BufferSize switch
			{
				"512" => "0", 
				"1024" => "1", 
				"2048" => "2", 
				_ => parser.Error1, 
			};
		}
		return parser.Error1;
	}

	public string ZZVZ(string s)
	{
		int num = -1;
		if (s.Length == parser.nSet)
		{
			switch (Convert.ToInt32(s))
			{
			case 0:
				console.SetupForm.VAC2BufferSize = "512";
				break;
			case 1:
				console.SetupForm.VAC2BufferSize = "1024";
				break;
			case 2:
				console.SetupForm.VAC2BufferSize = "2048";
				break;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			string vAC2BufferSize = console.SetupForm.VAC2BufferSize;
			string text = "";
			return vAC2BufferSize switch
			{
				"512" => "0", 
				"1024" => "1", 
				"2048" => "2", 
				_ => parser.Error1, 
			};
		}
		return parser.Error1;
	}

	public string ZZWA(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWB(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWC(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWD(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWE(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWF(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWG(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWH(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWJ(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWK(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWL(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWM(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWN(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWO(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWP(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWQ(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWR(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWS(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWT(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWU(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWV(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZWW(string s)
	{
		parser.Verbose_Error_Code = 7;
		return parser.Error1;
	}

	public string ZZXC()
	{
		console.XITValue = 0;
		return "";
	}

	public string ZZXD(string s)
	{
		if (s.Length == parser.nSet)
		{
			return ZZXF(s);
		}
		if (s.Length == parser.nGet)
		{
			console.XITValue -= 10;
			return "";
		}
		return parser.Error1;
	}

	public string ZZXF(string s)
	{
		int xITValue = 0;
		int num = 0;
		if (s != "")
		{
			xITValue = Convert.ToInt32(s);
			xITValue = Math.Max(-99999, xITValue);
			xITValue = Math.Min(99999, xITValue);
		}
		if (s.Length == parser.nSet)
		{
			console.XITValue = xITValue;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			num = console.XITValue;
			string text = ((num < 0) ? "-" : "+");
			return text + AddLeadingZeros(Math.Abs(num)).Substring(1);
		}
		return parser.Error1;
	}

	public string ZZXH(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(4000, val);
		double num = val;
		if (s.Length == parser.nSet)
		{
			console.VOXHangTime = num;
			console.TitleBarEncoderString = "Vox Delay = " + num / 1000.0 + "s";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			val = (int)console.VOXHangTime;
			return AddLeadingZeros(val);
		}
		return parser.Error1;
	}

	public string ZZXN(string s)
	{
		int num = 0;
		int num2 = 0;
		if (s.Length == parser.nGet)
		{
			AGCMode num3 = console.RX1AGCMode & (AGCMode)7;
			num2 = (int)console.CATPreamp;
			num2 = (num2 & 7) << 3;
			num = (int)(num3 + num2);
			if (console.CATSquelch != 0)
			{
				num += 64;
			}
			if (console.CATNB1 != 0)
			{
				num += 128;
			}
			if (console.CATNB2 != 0)
			{
				num += 256;
			}
			if (console.CATNR != 0)
			{
				num += 512;
			}
			if (console.CATNR2 != 0)
			{
				num += 1024;
			}
			if (console.CATSNB != 0)
			{
				num += 2048;
			}
			if (console.CATANF != 0)
			{
				num += 4096;
			}
			return AddLeadingZeros(num);
		}
		return parser.Error1;
	}

	public string ZZXO(string s)
	{
		int num = 0;
		int num2 = 0;
		if (s.Length == parser.nGet)
		{
			AGCMode num3 = console.RX2AGCMode & (AGCMode)7;
			num2 = (int)console.RX2PreampMode;
			num2 = (num2 & 7) << 3;
			num = (int)(num3 + num2);
			if (console.CATSquelch2 != 0)
			{
				num += 64;
			}
			if (console.CATRX2NB1 != 0)
			{
				num += 128;
			}
			if (console.CATRX2NB2 != 0)
			{
				num += 256;
			}
			if (console.CATRX2NR != 0)
			{
				num += 512;
			}
			if (console.CATRX2NR2 != 0)
			{
				num += 1024;
			}
			if (console.CATRX2SNB != 0)
			{
				num += 2048;
			}
			if (console.CATRX2ANF != 0)
			{
				num += 4096;
			}
			return AddLeadingZeros(num);
		}
		return parser.Error1;
	}

	public string ZZXS(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.XITOn = false;
			}
			else if (s == "1")
			{
				console.XITOn = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.XITOn)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZXT(string s)
	{
		if (s.Length == parser.nSet)
		{
			if (s == "0")
			{
				console.X2TR = false;
			}
			else if (s == "1")
			{
				console.X2TR = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.X2TR)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZXU(string s)
	{
		if (s.Length == parser.nSet)
		{
			return ZZXF(s);
		}
		if (s.Length == parser.nGet)
		{
			console.XITValue += 10;
			return "";
		}
		return parser.Error1;
	}

	public string ZZXV(string s)
	{
		int num = 0;
		if (s.Length == parser.nGet)
		{
			if (console.RITOn)
			{
				num++;
			}
			if (console.VFOLock == CheckState.Checked || console.VFOALock || console.CATVFOLock)
			{
				num += 2;
			}
			if (console.VFOBLock || console.VFOLock == CheckState.Indeterminate || console.CATVFOBLock)
			{
				num += 4;
			}
			if (console.VFOSplit)
			{
				num += 8;
			}
			if (console.CTuneDisplay)
			{
				num += 16;
			}
			if (console.CTuneRX2Display)
			{
				num += 32;
			}
			if (console.CATPTT || console.MOX)
			{
				num += 64;
			}
			if (console.TUN)
			{
				num += 128;
			}
			if (console.XITOn)
			{
				num += 256;
			}
			if (console.VFOSync)
			{
				num += 512;
			}
			return AddLeadingZeros(num);
		}
		return parser.Error1;
	}

	public string ZZYA(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.SetupForm.VAC2DirectIQ = false;
			}
			else if (s == "1")
			{
				console.SetupForm.VAC2DirectIQ = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.VAC2DirectIQ)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZYB(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.SetupForm.VAC2Calibrate = false;
			}
			else if (s == "1")
			{
				console.SetupForm.VAC2Calibrate = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.SetupForm.VAC2Calibrate)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZYC(string s)
	{
		int val = 0;
		if (s != null && s != "")
		{
			val = Convert.ToInt32(s);
		}
		val = Math.Max(0, val);
		val = Math.Min(70, val);
		if (s.Length == parser.nSet)
		{
			console.FMMic = val;
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return AddLeadingZeros(console.FMMic);
		}
		return parser.Error1;
	}

	public string ZZYR(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.CATRX1RX2RadioButton = Convert.ToInt32(s);
			return "";
		}
		if (s.Length == parser.nGet)
		{
			return console.CATRX1RX2RadioButton.ToString();
		}
		return parser.Error1;
	}

	public string ZZZB()
	{
		if (console.CATDisplayAvg == 1)
		{
			console.CATZB = "1";
		}
		return "";
	}

	public string ZZZZ()
	{
		console.Siolisten.SIO.Close();
		return "";
	}

	public string ZZZT(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.ZoomToBand(bStore: false);
			}
			else if (s == "1")
			{
				console.ZoomToBand(bStore: true);
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.ZTBisRecallStore)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZZQ(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.AutoAGCRX1 = false;
			}
			else if (s == "1")
			{
				console.AutoAGCRX1 = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.AutoAGCRX1)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZZR(string s)
	{
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (s == "0")
			{
				console.AutoAGCRX2 = false;
			}
			else if (s == "1")
			{
				console.AutoAGCRX2 = true;
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (console.AutoAGCRX2)
			{
				return "1";
			}
			return "0";
		}
		return parser.Error1;
	}

	public string ZZZM(string s)
	{
		if (s.Length == parser.nGet)
		{
			return "ZZZM" + HardwareSpecific.Model.ToString() + ";";
		}
		return parser.Error1;
	}

	public string ZZZV(string s)
	{
		if (s.Length == parser.nGet)
		{
			return "ZZZV" + console.VersionWithoutFW.Replace(";", "") + ";";
		}
		return parser.Error1;
	}

	public string ZZZW(string s)
	{
		if (console == null || console.Midi2Cat == null)
		{
			return parser.Error1;
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.Midi2Cat.SwapVFOWheelsProperty = s == "1";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (!console.Midi2Cat.SwapVFOWheelsProperty)
			{
				return "0";
			}
			return "1";
		}
		return parser.Error1;
	}

	public string ZZZN(string s)
	{
		if (console == null || console.Midi2Cat == null)
		{
			return parser.Error1;
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.QuickSplitEnabled = s == "1";
			}
			return "";
		}
		if (s.Length == parser.nGet)
		{
			bool flag = false;
			if (!console.IsSetupFormNull)
			{
				flag = console.SetupForm.QuickSplitEnabled;
			}
			if (!flag)
			{
				return "0";
			}
			return "1";
		}
		return parser.Error1;
	}

	public string ZZZO(string s)
	{
		if (console == null || console.Midi2Cat == null)
		{
			return parser.Error1;
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.QuickSplitEnabled = s == "1";
			}
			console.VFOSplit = s == "1";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			bool flag = console.VFOSplit;
			if (!console.IsSetupFormNull)
			{
				flag &= console.SetupForm.QuickSplitEnabled;
			}
			if (!flag)
			{
				return "0";
			}
			return "1";
		}
		return parser.Error1;
	}

	public string ZZXA(string s)
	{
		if (console == null)
		{
			return parser.Error1;
		}
		if (s.Length == parser.nSet && (s == "0" || s == "1"))
		{
			console.EnableAudioAmplifier = s == "1";
			return "";
		}
		if (s.Length == parser.nGet)
		{
			if (!console.EnableAudioAmplifier)
			{
				return "0";
			}
			return "1";
		}
		return parser.Error1;
	}

	private string AddLeadingZeros(int n, int pad_len = -1)
	{
		if (pad_len < 0)
		{
			return n.ToString().PadLeft(parser.nAns, '0');
		}
		return n.ToString().PadLeft(pad_len, '0');
	}

	private string JustSuffix(string s)
	{
		string text = "";
		text = s.Substring(4);
		return text.Substring(0, text.Length - 1);
	}

	private string OffsetDirection2String()
	{
		string text = "";
		return console.CurrentFMTXMode switch
		{
			FMTXMode.Simplex => "0", 
			FMTXMode.High => "1", 
			FMTXMode.Low => "2", 
			_ => "0", 
		};
	}

	private void String2OffsetDirection(string s)
	{
		switch (s)
		{
		case "0":
			console.CurrentFMTXMode = FMTXMode.Simplex;
			break;
		case "1":
			console.CurrentFMTXMode = FMTXMode.High;
			break;
		case "2":
			console.CurrentFMTXMode = FMTXMode.Low;
			break;
		default:
			console.CurrentFMTXMode = FMTXMode.Simplex;
			break;
		}
	}

	private double String2CTCSSFreq(string s)
	{
		double num = 0.0;
		return s switch
		{
			"01" => 67.0, 
			"02" => 69.3, 
			"03" => 71.9, 
			"04" => 74.4, 
			"05" => 77.0, 
			"06" => 79.7, 
			"07" => 82.5, 
			"08" => 85.4, 
			"09" => 88.5, 
			"10" => 91.5, 
			"11" => 94.8, 
			"12" => 97.4, 
			"13" => 100.0, 
			"14" => 103.5, 
			"15" => 107.2, 
			"16" => 110.9, 
			"17" => 114.8, 
			"18" => 118.8, 
			"19" => 123.0, 
			"20" => 127.3, 
			"21" => 131.8, 
			"22" => 136.5, 
			"23" => 141.3, 
			"24" => 146.2, 
			"25" => 151.4, 
			"26" => 156.7, 
			"27" => 159.8, 
			"28" => 162.2, 
			"29" => 165.5, 
			"30" => 167.9, 
			"31" => 171.3, 
			"32" => 173.8, 
			"33" => 177.3, 
			"34" => 179.9, 
			"35" => 183.5, 
			"36" => 186.2, 
			"37" => 189.9, 
			"38" => 192.8, 
			"39" => 199.5, 
			"40" => 203.5, 
			"41" => 206.5, 
			"42" => 210.7, 
			"43" => 218.1, 
			"44" => 225.7, 
			"45" => 229.1, 
			"46" => 233.6, 
			"47" => 241.8, 
			"48" => 250.3, 
			"49" => 254.1, 
			_ => 67.0, 
		};
	}

	private string CTCSSFreq2String(int freq)
	{
		string text = "";
		return freq switch
		{
			670 => "01", 
			693 => "02", 
			719 => "03", 
			744 => "04", 
			770 => "05", 
			797 => "06", 
			825 => "07", 
			854 => "08", 
			885 => "09", 
			915 => "10", 
			948 => "11", 
			974 => "12", 
			1000 => "13", 
			1035 => "14", 
			1072 => "15", 
			1109 => "16", 
			1148 => "17", 
			1188 => "18", 
			1230 => "19", 
			1273 => "20", 
			1318 => "21", 
			1365 => "22", 
			1413 => "23", 
			1462 => "24", 
			1514 => "25", 
			1567 => "26", 
			1598 => "27", 
			1622 => "28", 
			1655 => "29", 
			1679 => "30", 
			1713 => "31", 
			1738 => "32", 
			1773 => "33", 
			1799 => "34", 
			1835 => "35", 
			1862 => "36", 
			1899 => "37", 
			1928 => "38", 
			1995 => "39", 
			2035 => "40", 
			2065 => "41", 
			2107 => "42", 
			2181 => "43", 
			2257 => "44", 
			2291 => "45", 
			2336 => "46", 
			2418 => "47", 
			2503 => "48", 
			2541 => "49", 
			_ => "01", 
		};
	}

	private SortableBindingList<MemoryRecord> GetMemoryList()
	{
		try
		{
			return console.MemoryList.List;
		}
		catch
		{
			SortableBindingList<MemoryRecord> result = new SortableBindingList<MemoryRecord>();
			parser.Verbose_Error_Code = 4;
			return result;
		}
	}

	private MemoryRecord GetChannelRecord(string channel)
	{
		MemoryRecord result = new MemoryRecord();
		try
		{
			SortableBindingList<MemoryRecord> memoryList = GetMemoryList();
			int num = 0;
			for (num = 0; num < memoryList.Count; num++)
			{
				if (memoryList[num].Comments.Substring(0, 4) == channel + ":")
				{
					result = memoryList[num];
				}
			}
		}
		catch
		{
		}
		return result;
	}

	private int GetIndex(string channel)
	{
		int result = -1;
		try
		{
			SortableBindingList<MemoryRecord> memoryList = GetMemoryList();
			int num = 0;
			for (num = 0; num < memoryList.Count; num++)
			{
				if (memoryList[num].Comments.Substring(0, 4) == channel + ":")
				{
					result = num;
				}
			}
		}
		catch
		{
		}
		return result;
	}

	private int GetNextChannelNumber()
	{
		SortableBindingList<MemoryRecord> sortableBindingList = new SortableBindingList<MemoryRecord>();
		sortableBindingList = console.MemoryList.List;
		int num = 0;
		int num2 = 0;
		for (num = 0; num < sortableBindingList.Count; num++)
		{
			if (sortableBindingList[num].Comments.Contains(":"))
			{
				int num3 = int.Parse(sortableBindingList[num].Comments.Substring(0, sortableBindingList[num].Comments.IndexOf(":")));
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
		}
		return num2 + 1;
	}

	private string StrVFOFreq(string vfo)
	{
		double num = 0.0;
		string text = "";
		switch (vfo)
		{
		case "A":
			num = Math.Round(console.CATVFOA, 6);
			break;
		case "B":
			num = Math.Round(console.CATVFOB, 6);
			break;
		case "C":
			num = Convert.ToDouble(console.CATQMSValue);
			break;
		}
		text = (((int)num < 10) ? (text + "0000" + num) : (((int)num < 100) ? (text + "000" + num) : (((int)num < 1000) ? (text + "00" + num) : (((int)num >= 10000) ? num.ToString() : (text + "0" + num)))));
		if (text.IndexOf(separator) > 0)
		{
			text = text.Remove(text.IndexOf(separator), 1);
		}
		return text.PadRight(11, '0');
	}

	public string Filter2String(Filter f)
	{
		string text = f.ToString();
		string result = "";
		int num = 0;
		switch (text)
		{
		case "F6000":
			result = "6000";
			break;
		case "F4000":
			result = "4000";
			break;
		case "F2600":
			result = "2600";
			break;
		case "F2100":
			result = "2100";
			break;
		case "F1000":
			result = "1000";
			break;
		case "F500":
			result = "0500";
			break;
		case "F250":
			result = "0250";
			break;
		case "F100":
			result = "0100";
			break;
		case "F50":
			result = "0050";
			break;
		case "F25":
			result = "0025";
			break;
		case "VAR1":
			num = Math.Abs(console.RX1FilterHigh - console.RX1FilterLow);
			result = AddLeadingZeros(num);
			break;
		case "VAR2":
			num = Math.Abs(console.RX1FilterHigh - console.RX1FilterLow);
			result = AddLeadingZeros(num);
			break;
		}
		return result;
	}

	public Filter String2Filter(string f)
	{
		Filter result = Filter.FIRST;
		switch (f)
		{
		case "6000":
			result = Filter.F1;
			break;
		case "4000":
			result = Filter.F2;
			break;
		case "2600":
			result = Filter.F3;
			break;
		case "2100":
			result = Filter.F4;
			break;
		case "1000":
			result = Filter.F5;
			break;
		case "0500":
			result = Filter.F6;
			break;
		case "0250":
			result = Filter.F7;
			break;
		case "0100":
			result = Filter.F8;
			break;
		case "0050":
			result = Filter.F9;
			break;
		case "0025":
			result = Filter.F10;
			break;
		case "VAR1":
			result = Filter.VAR1;
			break;
		case "VAR2":
			result = Filter.VAR2;
			break;
		}
		return result;
	}

	private void SetFilterCenterAndWidth(int center, int width)
	{
		if (center != 0 && width != 0)
		{
			int num = center - width / 2;
			int num2 = center + width / 2;
			if (num < 0)
			{
				num = 0;
			}
			switch (console.RX1DSPMode)
			{
			case DSPMode.LSB:
			{
				int num3 = num2;
				num2 = -num;
				num = -num3;
				break;
			}
			case DSPMode.AM:
			case DSPMode.SAM:
				num = -num2;
				break;
			}
			console.SelectRX1VarFilter();
			console.UpdateRX1Filters(num, num2, force: true);
		}
	}

	private string Frequency2Code(int f, string n)
	{
		f = Math.Abs(f);
		string result = "";
		switch (console.RX1DSPMode)
		{
		case DSPMode.LSB:
		case DSPMode.USB:
		case DSPMode.CWL:
		case DSPMode.CWU:
			if (!(n == "SH"))
			{
				if (n == "SL")
				{
					if (f >= 0 && f <= 25)
					{
						result = "00";
					}
					else if (f > 25 && f <= 75)
					{
						result = "01";
					}
					else if (f > 75 && f <= 150)
					{
						result = "02";
					}
					else if (f > 150 && f <= 250)
					{
						result = "03";
					}
					else if (f > 250 && f <= 350)
					{
						result = "04";
					}
					else if (f > 350 && f <= 450)
					{
						result = "05";
					}
					else if (f > 450 && f <= 550)
					{
						result = "06";
					}
					else if (f > 550 && f <= 650)
					{
						result = "07";
					}
					else if (f > 650 && f <= 750)
					{
						result = "08";
					}
					else if (f > 750 && f <= 850)
					{
						result = "09";
					}
					else if (f > 850 && f <= 950)
					{
						result = "10";
					}
					else if (f > 950)
					{
						result = "11";
					}
				}
			}
			else if (f >= 0 && f <= 1500)
			{
				result = "00";
			}
			else if (f > 1500 && f <= 1700)
			{
				result = "01";
			}
			else if (f > 1700 && f <= 1900)
			{
				result = "02";
			}
			else if (f > 1900 && f <= 2100)
			{
				result = "03";
			}
			else if (f > 2100 && f <= 2300)
			{
				result = "04";
			}
			else if (f > 2300 && f <= 2500)
			{
				result = "05";
			}
			else if (f > 2500 && f <= 2700)
			{
				result = "06";
			}
			else if (f > 2700 && f <= 2900)
			{
				result = "07";
			}
			else if (f > 2900 && f <= 3200)
			{
				result = "08";
			}
			else if (f > 3200 && f <= 3700)
			{
				result = "09";
			}
			else if (f > 3700 && f <= 4500)
			{
				result = "10";
			}
			else if (f > 4500)
			{
				result = "11";
			}
			break;
		case DSPMode.DSB:
		case DSPMode.FM:
		case DSPMode.AM:
		case DSPMode.SAM:
		case DSPMode.DRM:
			if (!(n == "SH"))
			{
				if (n == "SL")
				{
					if (f >= 0 && f <= 50)
					{
						result = "00";
					}
					else if (f > 50 && f <= 150)
					{
						result = "01";
					}
					else if (f > 150 && f <= 350)
					{
						result = "02";
					}
					else if (f > 350)
					{
						result = "03";
					}
				}
			}
			else if (f >= 0 && f <= 2750)
			{
				result = "00";
			}
			else if (f > 2750 && f <= 3500)
			{
				result = "01";
			}
			else if (f > 3500 && f <= 4500)
			{
				result = "02";
			}
			else if (f > 4500)
			{
				result = "03";
			}
			break;
		}
		return result;
	}

	private int Code2Frequency(string c, string n)
	{
		int result = 0;
		string text = "SSB";
		int num = 0;
		switch (console.RX1DSPMode)
		{
		case DSPMode.DSB:
		case DSPMode.FM:
		case DSPMode.AM:
		case DSPMode.SAM:
		case DSPMode.DRM:
			text = "DSB";
			break;
		}
		if (n == "SL")
		{
			num = ((text == "SSB") ? 1 : 3);
		}
		else if (n == "SH")
		{
			num = ((!(text == "SSB")) ? 4 : 2);
		}
		switch (num)
		{
		case 1:
			switch (c)
			{
			case "00":
				result = 0;
				break;
			case "01":
				result = 50;
				break;
			case "02":
				result = 100;
				break;
			case "03":
				result = 200;
				break;
			case "04":
				result = 300;
				break;
			case "05":
				result = 400;
				break;
			case "06":
				result = 500;
				break;
			case "07":
				result = 600;
				break;
			case "08":
				result = 700;
				break;
			case "09":
				result = 800;
				break;
			case "10":
				result = 900;
				break;
			case "11":
				result = 1000;
				break;
			}
			break;
		case 2:
			switch (c)
			{
			case "00":
				result = 1400;
				break;
			case "01":
				result = 1600;
				break;
			case "02":
				result = 1800;
				break;
			case "03":
				result = 2000;
				break;
			case "04":
				result = 2200;
				break;
			case "05":
				result = 2400;
				break;
			case "06":
				result = 2600;
				break;
			case "07":
				result = 2800;
				break;
			case "08":
				result = 3000;
				break;
			case "09":
				result = 3400;
				break;
			case "10":
				result = 4000;
				break;
			case "11":
				result = 5000;
				break;
			}
			break;
		case 3:
			switch (c)
			{
			case "00":
				result = 0;
				break;
			case "01":
				result = 100;
				break;
			case "02":
				result = 200;
				break;
			case "03":
				result = 500;
				break;
			}
			break;
		case 4:
			switch (c)
			{
			case "00":
				result = 2500;
				break;
			case "01":
				result = 3000;
				break;
			case "02":
				result = 4000;
				break;
			case "03":
				result = 5000;
				break;
			}
			break;
		}
		return result;
	}

	private void SetFilter(string c, string n)
	{
		console.RX1Filter = Filter.VAR1;
		int num = 0;
		int num2 = 0;
		switch (console.RX1DSPMode)
		{
		case DSPMode.USB:
		case DSPMode.CWU:
			num = Code2Frequency(c, n);
			if (n == "SH")
			{
				console.RX1FilterHigh = num;
			}
			else
			{
				console.RX1FilterLow = num;
			}
			break;
		case DSPMode.LSB:
		case DSPMode.CWL:
			if (n == "SH")
			{
				num = Code2Frequency(c, "SH");
				console.RX1FilterLow = -num;
			}
			else
			{
				num = Code2Frequency(c, "SL");
				console.RX1FilterHigh = -num;
			}
			break;
		case DSPMode.DSB:
		case DSPMode.FM:
		case DSPMode.AM:
		case DSPMode.SAM:
		case DSPMode.DRM:
		{
			if (n == "SH")
			{
				num = Code2Frequency(c, "SH");
				console.RX1FilterHigh = num / 2;
				console.RX1FilterLow = -num / 2;
				break;
			}
			num = console.RX1FilterHigh * 2;
			string c2 = Frequency2Code(num, "SH");
			num = Code2Frequency(c2, "SH");
			console.RX1FilterHigh = num / 2;
			console.RX1FilterLow = -num / 2;
			num2 = Code2Frequency(c, "SL");
			console.RX1FilterLow += num2;
			break;
		}
		case DSPMode.DIGU:
		case DSPMode.SPEC:
		case DSPMode.DIGL:
			break;
		}
	}

	public void String2Mode(string pIndex)
	{
		if (pIndex == null)
		{
			return;
		}
		int length = pIndex.Length;
		if (length != 2)
		{
			return;
		}
		switch (pIndex[1])
		{
		case '0':
			if (!(pIndex == "00"))
			{
				if (pIndex == "10")
				{
					console.RX1DSPMode = DSPMode.SAM;
				}
			}
			else
			{
				console.RX1DSPMode = DSPMode.LSB;
			}
			break;
		case '1':
			if (!(pIndex == "01"))
			{
				if (pIndex == "11")
				{
					console.RX1DSPMode = DSPMode.DRM;
				}
			}
			else
			{
				console.RX1DSPMode = DSPMode.USB;
			}
			break;
		case '2':
			if (pIndex == "02")
			{
				console.RX1DSPMode = DSPMode.DSB;
			}
			break;
		case '3':
			if (pIndex == "03")
			{
				console.RX1DSPMode = DSPMode.CWL;
			}
			break;
		case '4':
			if (pIndex == "04")
			{
				console.RX1DSPMode = DSPMode.CWU;
			}
			break;
		case '5':
			if (pIndex == "05")
			{
				console.RX1DSPMode = DSPMode.FM;
			}
			break;
		case '6':
			if (pIndex == "06")
			{
				console.RX1DSPMode = DSPMode.AM;
			}
			break;
		case '7':
			if (pIndex == "07")
			{
				console.RX1DSPMode = DSPMode.DIGU;
			}
			break;
		case '8':
			if (pIndex == "08")
			{
				console.RX1DSPMode = DSPMode.SPEC;
			}
			break;
		case '9':
			if (pIndex == "09")
			{
				console.RX1DSPMode = DSPMode.DIGL;
			}
			break;
		}
	}

	public string Mode2String(DSPMode pMode)
	{
		string text = "";
		return pMode switch
		{
			DSPMode.LSB => "00", 
			DSPMode.USB => "01", 
			DSPMode.DSB => "02", 
			DSPMode.CWL => "03", 
			DSPMode.CWU => "04", 
			DSPMode.FM => "05", 
			DSPMode.AM => "06", 
			DSPMode.DIGU => "07", 
			DSPMode.SPEC => "08", 
			DSPMode.DIGL => "09", 
			DSPMode.SAM => "10", 
			DSPMode.DRM => "11", 
			_ => parser.Error1, 
		};
	}

	public void KString2Mode(string pIndex)
	{
		switch (pIndex)
		{
		case "1":
			if (console.SetupForm.DigUIsUSB)
			{
				console.RX1DSPMode = DSPMode.DIGL;
			}
			else
			{
				console.RX1DSPMode = DSPMode.LSB;
			}
			break;
		case "2":
			if (console.SetupForm.DigUIsUSB)
			{
				console.RX1DSPMode = DSPMode.DIGU;
			}
			else
			{
				console.RX1DSPMode = DSPMode.USB;
			}
			break;
		case "3":
			console.RX1DSPMode = DSPMode.CWU;
			break;
		case "4":
			console.RX1DSPMode = DSPMode.FM;
			break;
		case "5":
			console.RX1DSPMode = DSPMode.AM;
			break;
		case "6":
			console.RX1DSPMode = DSPMode.DIGL;
			break;
		case "7":
			console.RX1DSPMode = DSPMode.CWL;
			break;
		case "9":
			console.RX1DSPMode = DSPMode.DIGU;
			break;
		default:
			console.RX1DSPMode = DSPMode.USB;
			break;
		}
	}

	public string Mode2KString(DSPMode pMode)
	{
		string text = "";
		switch (pMode)
		{
		case DSPMode.LSB:
			return "1";
		case DSPMode.USB:
			return "2";
		case DSPMode.CWU:
			return "3";
		case DSPMode.FM:
			return "4";
		case DSPMode.AM:
			return "5";
		case DSPMode.DIGL:
			if (console.SetupForm.DigUIsUSB)
			{
				return "1";
			}
			return "6";
		case DSPMode.CWL:
			return "7";
		case DSPMode.DIGU:
			if (console.SetupForm.DigUIsUSB)
			{
				return "2";
			}
			return "9";
		default:
			return parser.Error1;
		}
	}

	private void MakeBandList()
	{
		int num = 0;
		BandList = new Band[44];
		foreach (Band value in Enum.GetValues(typeof(Band)))
		{
			BandList.SetValue(value, num);
			num++;
		}
		if (console.XVTRPresent)
		{
			LastBandIndex = Array.IndexOf(BandList, Band.B2M);
		}
		else
		{
			LastBandIndex = Array.IndexOf(BandList, Band.B6M);
		}
	}

	private void SetBandGroup(int band)
	{
		int nSet = parser.nSet;
		parser.nSet = 1;
		if (band == 0)
		{
			ZZBG("0");
		}
		else
		{
			ZZBG("1");
		}
		parser.nSet = nSet;
	}

	private string GetBand(string b)
	{
		if (b.Length == parser.nSet)
		{
			if (b.StartsWith("V") || b.StartsWith("v"))
			{
				SetBandGroup(1);
			}
			else
			{
				SetBandGroup(0);
			}
		}
		if (b.Length == parser.nSet)
		{
			console.SetCATBand(String2Band(b));
			return "";
		}
		if (b.Length == parser.nGet)
		{
			return Band2String(console.RX1Band);
		}
		return parser.Error1;
	}

	private void BandUp()
	{
		Band rX1Band = console.RX1Band;
		int num = Array.IndexOf(BandList, rX1Band);
		Band cATBand = ((num != LastBandIndex) ? BandList[num + 1] : BandList[0]);
		console.SetCATBand(cATBand);
	}

	private void BandDown()
	{
		Band rX1Band = console.RX1Band;
		int num = Array.IndexOf(BandList, rX1Band);
		Band cATBand = ((num <= 0) ? BandList[LastBandIndex] : BandList[num - 1]);
		console.SetCATBand(cATBand);
	}

	private string Band2String(Band pBand)
	{
		return pBand switch
		{
			Band.GEN => "888", 
			Band.B160M => "160", 
			Band.B60M => "060", 
			Band.B80M => "080", 
			Band.B40M => "040", 
			Band.B30M => "030", 
			Band.B20M => "020", 
			Band.B17M => "017", 
			Band.B15M => "015", 
			Band.B12M => "012", 
			Band.B10M => "010", 
			Band.B6M => "006", 
			Band.B2M => "002", 
			Band.WWV => "999", 
			Band.VHF0 => "V00", 
			Band.VHF1 => "V01", 
			Band.VHF2 => "V02", 
			Band.VHF3 => "V03", 
			Band.VHF4 => "V04", 
			Band.VHF5 => "V05", 
			Band.VHF6 => "V06", 
			Band.VHF7 => "V07", 
			Band.VHF8 => "V08", 
			Band.VHF9 => "V09", 
			Band.VHF10 => "V10", 
			Band.VHF11 => "V11", 
			Band.VHF12 => "V12", 
			Band.VHF13 => "V13", 
			_ => "888", 
		};
	}

	private Band String2Band(string pBand)
	{
		return pBand.ToUpper() switch
		{
			"888" => Band.GEN, 
			"160" => Band.B160M, 
			"060" => Band.B60M, 
			"080" => Band.B80M, 
			"040" => Band.B40M, 
			"030" => Band.B30M, 
			"020" => Band.B20M, 
			"017" => Band.B17M, 
			"015" => Band.B15M, 
			"012" => Band.B12M, 
			"010" => Band.B10M, 
			"006" => Band.B6M, 
			"002" => Band.B2M, 
			"999" => Band.WWV, 
			"V00" => Band.VHF0, 
			"V01" => Band.VHF1, 
			"V02" => Band.VHF2, 
			"V03" => Band.VHF3, 
			"V04" => Band.VHF4, 
			"V05" => Band.VHF5, 
			"V06" => Band.VHF6, 
			"V07" => Band.VHF7, 
			"V08" => Band.VHF8, 
			"V09" => Band.VHF9, 
			"V10" => Band.VHF10, 
			"V11" => Band.VHF11, 
			"V12" => Band.VHF12, 
			"V13" => Band.VHF13, 
			_ => Band.GEN, 
		};
	}

	private double Step2Freq(int step)
	{
		double result = 0.0;
		switch (step)
		{
		case 0:
			result = 1E-06;
			break;
		case 1:
			result = 1E-05;
			break;
		case 2:
			result = 2.5E-05;
			break;
		case 3:
			result = 5E-05;
			break;
		case 4:
			result = 0.0001;
			break;
		case 5:
			result = 0.00025;
			break;
		case 6:
			result = 0.0005;
			break;
		case 7:
			result = 0.001;
			break;
		case 8:
			result = 0.005;
			break;
		case 9:
			result = 0.009;
			break;
		case 10:
			result = 0.01;
			break;
		case 11:
			result = 0.1;
			break;
		case 12:
			result = 0.25;
			break;
		case 13:
			result = 0.5;
			break;
		case 14:
			result = 1.0;
			break;
		case 15:
			result = 10.0;
			break;
		}
		return result;
	}

	private string Step2String(int pSize)
	{
		List<string> list = new List<string>
		{
			"0000", "0000", "0001", "0001", "1000", "0010", "1001", "1010", "0011", "0011",
			"0011", "1011", "0011", "1100", "0100", "0100", "0100", "0100", "0100", "0100",
			"0100", "0101", "1101", "1110", "0110", "0111"
		};
		if (pSize < 0 || pSize >= list.Count)
		{
			return "0000";
		}
		return list[pSize];
	}

	private void String2RXMeter(int m)
	{
		console.CurrentMeterRXMode = (MeterRXMode)m;
	}

	private string RXMeter2String()
	{
		return ((int)console.CurrentMeterRXMode).ToString();
	}

	private void String2TXMeter(int m)
	{
		console.CurrentMeterTXMode = (MeterTXMode)m;
	}

	private string TXMeter2String()
	{
		return ((int)console.CurrentMeterTXMode).ToString();
	}

	private string CAT2RigType()
	{
		return "";
	}

	private string RigType2CAT()
	{
		return "";
	}

	private string Width2Index(int txt)
	{
		string text = "";
		return txt switch
		{
			256 => "0", 
			512 => "1", 
			1024 => "2", 
			2048 => "3", 
			4096 => "4", 
			8192 => "5", 
			16384 => "6", 
			_ => "0", 
		};
	}

	private int Index2Width(string ndx)
	{
		return ndx switch
		{
			"0" => 256, 
			"1" => 512, 
			"2" => 1024, 
			"3" => 2048, 
			"4" => 4096, 
			"5" => 8192, 
			"6" => 16384, 
			_ => 256, 
		};
	}
}
