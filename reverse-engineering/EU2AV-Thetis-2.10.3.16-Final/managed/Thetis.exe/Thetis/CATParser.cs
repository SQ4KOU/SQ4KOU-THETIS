using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Thetis;

public class CATParser
{
	private string current_cat;

	private string prefix;

	private string suffix;

	private string extension;

	private readonly char[] term = new char[1] { ';' };

	public int nSet;

	public int nGet;

	public int nAns;

	public bool IsActive;

	private XmlDocument doc;

	private readonly CATCommands cmdlist;

	private readonly Console console;

	public string Error1 = "?;";

	public string Error2 = "E;";

	public string Error3 = "O;";

	private bool IsExtended;

	private readonly ASCIIEncoding AE = new ASCIIEncoding();

	private bool verbose;

	private int verbose_error_code;

	public bool Verbose
	{
		get
		{
			return verbose;
		}
		set
		{
			verbose = value;
			cmdlist.Verbose = value;
		}
	}

	public int Verbose_Error_Code
	{
		get
		{
			return verbose_error_code;
		}
		set
		{
			verbose_error_code = value;
		}
	}

	public CATParser(Console c)
	{
		console = c;
		cmdlist = new CATCommands(console, this);
		GetCATData();
	}

	private void GetCATData()
	{
		string text = "CATStructs.xml";
		doc = new XmlDocument();
		try
		{
			doc.Load(Application.StartupPath + "\\" + text);
		}
		catch (FileNotFoundException ex)
		{
			throw ex;
		}
	}

	public string Get(byte[] pCmdString)
	{
		return Get(AE.GetString(pCmdString));
	}

	public string Get(string pCmdString)
	{
		current_cat = pCmdString;
		string text = "";
		prefix = "";
		suffix = "";
		extension = "";
		verbose_error_code = 0;
		if (current_cat.Length < 3)
		{
			if (!Verbose)
			{
				return Error1;
			}
			verbose_error_code = 1;
		}
		if (CheckFormat())
		{
			switch (prefix)
			{
			case "AG":
				text = cmdlist.AG(suffix);
				break;
			case "AI":
				text = cmdlist.AI(suffix);
				break;
			case "BD":
				text = cmdlist.BD();
				break;
			case "BU":
				text = cmdlist.BU();
				break;
			case "CN":
				text = cmdlist.CN(suffix);
				break;
			case "CT":
				text = cmdlist.CT(suffix);
				break;
			case "DN":
				text = cmdlist.DN();
				break;
			case "FA":
				text = cmdlist.FA(suffix);
				break;
			case "FB":
				text = cmdlist.FB(suffix);
				break;
			case "FR":
				text = cmdlist.FR(suffix);
				break;
			case "FT":
				text = cmdlist.FT(suffix, bFromCatDirect: true);
				break;
			case "FW":
				text = cmdlist.FW(suffix);
				break;
			case "GT":
				text = cmdlist.GT(suffix);
				break;
			case "ID":
				text = cmdlist.ID();
				break;
			case "IF":
				text = cmdlist.IF();
				break;
			case "KS":
				text = cmdlist.KS(suffix);
				break;
			case "KY":
				text = cmdlist.KY(suffix);
				break;
			case "MD":
				text = cmdlist.MD(suffix);
				break;
			case "MG":
				text = cmdlist.MG(suffix);
				break;
			case "MO":
				text = cmdlist.MO(suffix);
				break;
			case "NB":
				text = cmdlist.NB(suffix);
				break;
			case "NT":
				text = cmdlist.NT(suffix);
				break;
			case "OF":
				text = cmdlist.OF(suffix);
				break;
			case "OS":
				text = cmdlist.OS(suffix);
				break;
			case "PC":
				text = cmdlist.PC(suffix);
				break;
			case "PR":
				text = cmdlist.PR(suffix);
				break;
			case "PS":
				text = cmdlist.PS(suffix);
				break;
			case "QI":
				text = cmdlist.QI();
				break;
			case "RC":
				text = cmdlist.RC();
				break;
			case "RD":
				text = cmdlist.RD(suffix);
				break;
			case "RT":
				text = cmdlist.RT(suffix);
				break;
			case "RU":
				text = cmdlist.RU(suffix);
				break;
			case "RX":
				text = cmdlist.RX(suffix);
				break;
			case "SH":
				text = cmdlist.SH(suffix);
				break;
			case "SL":
				text = cmdlist.SL(suffix);
				break;
			case "SM":
				text = cmdlist.SM(suffix);
				break;
			case "SQ":
				text = cmdlist.SQ(suffix);
				break;
			case "TX":
				text = cmdlist.TX(suffix);
				break;
			case "UP":
				text = cmdlist.UP();
				break;
			case "XT":
				text = cmdlist.XT(suffix);
				break;
			case "ZZ":
				text = ParseExtended();
				break;
			}
			if (prefix != "ZZ" && !text.Contains(Error1))
			{
				text = ((text.Length == nAns && nAns > 0) ? (prefix + text + ";") : ((nAns != -1 && !(text == "")) ? Error3 : ""));
			}
		}
		else
		{
			text = ProcessError(Error1);
		}
		return text;
	}

	private bool CheckFormat()
	{
		if (current_cat.StartsWith(";"))
		{
			current_cat = current_cat.TrimStart(term);
		}
		if (current_cat.IndexOfAny(term) < 2)
		{
			verbose_error_code = 1;
			return false;
		}
		if (current_cat.Substring(0, 2).ToUpper() == "ZZ" && current_cat.Length > 3)
		{
			IsExtended = true;
		}
		else
		{
			IsExtended = false;
		}
		if (!FindPrefix())
		{
			if (verbose_error_code == 0)
			{
				verbose_error_code = 4;
			}
			return false;
		}
		if (!FindSuffix())
		{
			if (verbose_error_code == 0)
			{
				verbose_error_code = 3;
			}
			return false;
		}
		return true;
	}

	private bool FindPrefix()
	{
		string text = "";
		text = ((!IsExtended) ? current_cat.Substring(0, 2).ToUpper() : current_cat.Substring(0, 4).ToUpper());
		try
		{
			XmlElement documentElement = doc.DocumentElement;
			string xpath = "descendant::catstruct[@code='" + text + "']";
			XmlNode xmlNode = documentElement.SelectSingleNode(xpath);
			if (xmlNode != null)
			{
				foreach (XmlNode item in xmlNode)
				{
					switch (item.Name)
					{
					case "active":
						IsActive = Convert.ToBoolean(item.InnerXml);
						break;
					case "nsetparms":
						nSet = Convert.ToInt16(item.InnerXml);
						break;
					case "ngetparms":
						nGet = Convert.ToInt16(item.InnerXml);
						break;
					case "nansparms":
						nAns = Convert.ToInt16(item.InnerXml);
						break;
					}
				}
				if (IsActive)
				{
					if (IsExtended)
					{
						prefix = text.Substring(0, 2);
						extension = text.Substring(2, 2);
					}
					else
					{
						prefix = text;
						extension = "";
					}
					return true;
				}
				verbose_error_code = 2;
				return false;
			}
			verbose_error_code = 3;
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return false;
	}

	private bool FindSuffix()
	{
		int num = 3;
		int startIndex = 2;
		int num2 = 2;
		if (IsExtended)
		{
			num = 5;
			startIndex = 4;
			num2 = 4;
		}
		string text;
		if (current_cat.Length > num)
		{
			text = current_cat.Substring(startIndex, current_cat.IndexOf(";") - num2);
			if (prefix != "KY" && prefix + extension != "ZZKY" && prefix + extension != "ZZMY" && prefix + extension != "ZZEA" && prefix + extension != "ZZEB" && prefix + extension != "ZZFX" && prefix + extension != "ZZFY" && prefix + extension != "ZZFV" && prefix + extension != "ZZFW" && prefix + extension != "ZZGA" && prefix + extension != "ZZGR" && prefix + extension != "ZZJP" && prefix + extension != "ZZJQ" && prefix + extension != "ZZJR" && prefix + extension != "ZZJS" && !new Regex("^[+-]?[Vv0-9]*$").IsMatch(text))
			{
				verbose_error_code = 5;
				return false;
			}
		}
		else
		{
			text = "";
		}
		suffix = text;
		if (text.Length == nSet || text.Length == nGet)
		{
			return true;
		}
		verbose_error_code = 6;
		return false;
	}

	private string ParseExtended()
	{
		string text = Error1;
		switch (prefix + extension)
		{
		case "ZZAA":
			text = cmdlist.ZZAA(suffix);
			break;
		case "ZZAB":
			text = cmdlist.ZZAB(suffix);
			break;
		case "ZZAC":
			text = cmdlist.ZZAC(suffix);
			break;
		case "ZZAD":
			text = cmdlist.ZZAD(suffix);
			break;
		case "ZZAE":
			text = cmdlist.ZZAE(suffix);
			break;
		case "ZZAF":
			text = cmdlist.ZZAF(suffix);
			break;
		case "ZZAG":
			text = cmdlist.ZZAG(suffix);
			break;
		case "ZZAI":
			text = cmdlist.ZZAI(suffix);
			break;
		case "ZZAP":
			text = cmdlist.ZZAP(suffix);
			break;
		case "ZZAR":
			text = cmdlist.ZZAR(suffix);
			break;
		case "ZZAS":
			text = cmdlist.ZZAS(suffix);
			break;
		case "ZZAT":
			text = cmdlist.ZZAT(suffix);
			break;
		case "ZZAU":
			text = cmdlist.ZZAU(suffix);
			break;
		case "ZZAY":
			text = cmdlist.ZZAY(suffix);
			break;
		case "ZZBA":
			text = cmdlist.ZZBA();
			break;
		case "ZZBB":
			text = cmdlist.ZZBB();
			break;
		case "ZZBD":
			text = cmdlist.ZZBD();
			break;
		case "ZZBE":
			text = cmdlist.ZZBE(suffix);
			break;
		case "ZZBF":
			text = cmdlist.ZZBF(suffix);
			break;
		case "ZZBI":
			text = cmdlist.ZZBI(suffix);
			break;
		case "ZZBG":
			text = cmdlist.ZZBG(suffix);
			break;
		case "ZZBM":
			text = cmdlist.ZZBM(suffix);
			break;
		case "ZZBP":
			text = cmdlist.ZZBP(suffix);
			break;
		case "ZZBR":
			text = cmdlist.ZZBR(suffix);
			break;
		case "ZZBS":
			text = cmdlist.ZZBS(suffix);
			break;
		case "ZZBT":
			text = cmdlist.ZZBT(suffix);
			break;
		case "ZZBU":
			text = cmdlist.ZZBU();
			break;
		case "ZZBY":
			text = cmdlist.ZZBY();
			break;
		case "ZZCB":
			text = cmdlist.ZZCB(suffix);
			break;
		case "ZZCD":
			text = cmdlist.ZZCD(suffix);
			break;
		case "ZZCF":
			text = cmdlist.ZZCF(suffix);
			break;
		case "ZZCI":
			text = cmdlist.ZZCI(suffix);
			break;
		case "ZZCL":
			text = cmdlist.ZZCL(suffix);
			break;
		case "ZZCM":
			text = cmdlist.ZZCM(suffix);
			break;
		case "ZZCN":
			text = cmdlist.ZZCN(suffix);
			break;
		case "ZZCO":
			text = cmdlist.ZZCO(suffix);
			break;
		case "ZZCP":
			text = cmdlist.ZZCP(suffix);
			break;
		case "ZZCS":
			text = cmdlist.ZZCS(suffix);
			break;
		case "ZZCT":
			text = cmdlist.ZZCT(suffix);
			break;
		case "ZZCU":
			text = cmdlist.ZZCU();
			break;
		case "ZZDA":
			text = cmdlist.ZZDA(suffix);
			break;
		case "ZZDB":
			text = cmdlist.ZZDB(suffix);
			break;
		case "ZZDC":
			text = cmdlist.ZZDC(suffix);
			break;
		case "ZZDD":
			text = cmdlist.ZZDD(suffix);
			break;
		case "ZZDE":
			text = cmdlist.ZZDE(suffix);
			break;
		case "ZZDF":
			text = cmdlist.ZZDF(suffix);
			break;
		case "ZZDG":
			text = cmdlist.ZZDG(suffix);
			break;
		case "ZZDH":
			text = cmdlist.ZZDH(suffix);
			break;
		case "ZZDM":
			text = cmdlist.ZZDM(suffix);
			break;
		case "ZZDN":
			text = cmdlist.ZZDN(suffix);
			break;
		case "ZZDO":
			text = cmdlist.ZZDO(suffix);
			break;
		case "ZZDP":
			text = cmdlist.ZZDP(suffix);
			break;
		case "ZZDQ":
			text = cmdlist.ZZDQ(suffix);
			break;
		case "ZZDR":
			text = cmdlist.ZZDR(suffix);
			break;
		case "ZZDU":
			text = cmdlist.ZZDU();
			break;
		case "ZZDX":
			text = cmdlist.ZZDX(suffix);
			break;
		case "ZZER":
			text = cmdlist.ZZER(suffix);
			break;
		case "ZZEA":
			text = cmdlist.ZZEA(suffix);
			break;
		case "ZZEB":
			text = cmdlist.ZZEB(suffix);
			break;
		case "ZZEM":
			text = cmdlist.ZZEM(suffix);
			break;
		case "ZZET":
			text = cmdlist.ZZET(suffix);
			break;
		case "ZZFA":
			text = cmdlist.ZZFA(suffix);
			break;
		case "ZZFB":
			text = cmdlist.ZZFB(suffix);
			break;
		case "ZZFD":
			text = cmdlist.ZZFD(suffix);
			break;
		case "ZZFR":
			text = cmdlist.ZZFR(suffix);
			break;
		case "ZZFS":
			text = cmdlist.ZZFS(suffix);
			break;
		case "ZZFT":
			text = cmdlist.ZZFT(suffix);
			break;
		case "ZZFI":
			text = cmdlist.ZZFI(suffix);
			break;
		case "ZZFJ":
			text = cmdlist.ZZFJ(suffix);
			break;
		case "ZZFL":
			text = cmdlist.ZZFL(suffix);
			break;
		case "ZZFH":
			text = cmdlist.ZZFH(suffix);
			break;
		case "ZZFM":
			text = cmdlist.ZZFM();
			break;
		case "ZZFV":
			text = cmdlist.ZZFV(suffix);
			break;
		case "ZZFW":
			text = cmdlist.ZZFW(suffix);
			break;
		case "ZZFX":
			text = cmdlist.ZZFX(suffix);
			break;
		case "ZZFY":
			text = cmdlist.ZZFY(suffix);
			break;
		case "ZZGA":
			text = cmdlist.ZZGA(suffix);
			break;
		case "ZZGE":
			text = cmdlist.ZZGE(suffix);
			break;
		case "ZZGL":
			text = cmdlist.ZZGL(suffix);
			break;
		case "ZZGT":
			text = cmdlist.ZZGT(suffix);
			break;
		case "ZZGU":
			text = cmdlist.ZZGU(suffix);
			break;
		case "ZZGR":
			text = cmdlist.ZZGR(suffix);
			break;
		case "ZZHA":
			text = cmdlist.ZZHA(suffix);
			break;
		case "ZZHR":
			text = cmdlist.ZZHR(suffix);
			break;
		case "ZZHT":
			text = cmdlist.ZZHT(suffix);
			break;
		case "ZZHU":
			text = cmdlist.ZZHU(suffix);
			break;
		case "ZZHV":
			text = cmdlist.ZZHV(suffix);
			break;
		case "ZZHW":
			text = cmdlist.ZZHW(suffix);
			break;
		case "ZZHX":
			text = cmdlist.ZZHX(suffix);
			break;
		case "ZZID":
			text = cmdlist.ZZID();
			break;
		case "ZZIF":
			text = cmdlist.ZZIF(suffix);
			break;
		case "ZZIO":
			text = cmdlist.ZZIO();
			break;
		case "ZZIS":
			text = cmdlist.ZZIS(suffix);
			break;
		case "ZZIT":
			text = cmdlist.ZZIT(suffix);
			break;
		case "ZZIU":
			text = cmdlist.ZZIU();
			break;
		case "ZZKO":
			text = cmdlist.ZZKO(suffix);
			break;
		case "ZZKM":
			text = cmdlist.ZZKM(suffix);
			break;
		case "ZZKS":
			text = cmdlist.ZZKS(suffix);
			break;
		case "ZZKY":
			text = cmdlist.ZZKY(suffix);
			break;
		case "ZZLA":
			text = cmdlist.ZZLA(suffix);
			break;
		case "ZZLB":
			text = cmdlist.ZZLB(suffix);
			break;
		case "ZZLC":
			text = cmdlist.ZZLC(suffix);
			break;
		case "ZZLD":
			text = cmdlist.ZZLD(suffix);
			break;
		case "ZZLE":
			text = cmdlist.ZZLE(suffix);
			break;
		case "ZZLF":
			text = cmdlist.ZZLF(suffix);
			break;
		case "ZZLG":
			text = cmdlist.ZZLG(suffix);
			break;
		case "ZZLH":
			text = cmdlist.ZZLH(suffix);
			break;
		case "ZZLI":
			text = cmdlist.ZZLI(suffix);
			break;
		case "ZZMA":
			text = cmdlist.ZZMA(suffix);
			break;
		case "ZZMB":
			text = cmdlist.ZZMB(suffix);
			break;
		case "ZZMD":
			text = cmdlist.ZZMD(suffix);
			break;
		case "ZZME":
			text = cmdlist.ZZME(suffix);
			break;
		case "ZZMF":
			text = cmdlist.ZZMF(suffix);
			break;
		case "ZZMG":
			text = cmdlist.ZZMG(suffix);
			break;
		case "ZZML":
			text = cmdlist.ZZML();
			break;
		case "ZZMN":
			text = cmdlist.ZZMN(suffix);
			break;
		case "ZZMO":
			text = cmdlist.ZZMO(suffix);
			break;
		case "ZZMR":
			text = cmdlist.ZZMR(suffix);
			break;
		case "ZZMS":
			text = cmdlist.ZZMS(suffix);
			break;
		case "ZZMT":
			text = cmdlist.ZZMT(suffix);
			break;
		case "ZZMU":
			text = cmdlist.ZZMU(suffix);
			break;
		case "ZZMV":
			text = cmdlist.ZZMV();
			break;
		case "ZZMW":
			text = cmdlist.ZZMW(suffix);
			break;
		case "ZZMX":
			text = cmdlist.ZZMX(suffix);
			break;
		case "ZZMY":
			text = cmdlist.ZZMY();
			break;
		case "ZZMZ":
			text = cmdlist.ZZMZ(suffix);
			break;
		case "ZZJP":
			text = cmdlist.ZZJP(suffix);
			break;
		case "ZZJQ":
			text = cmdlist.ZZJQ(suffix);
			break;
		case "ZZJR":
			text = cmdlist.ZZJR(suffix);
			break;
		case "ZZJS":
			text = cmdlist.ZZJS();
			break;
		case "ZZNA":
			text = cmdlist.ZZNA(suffix);
			break;
		case "ZZNB":
			text = cmdlist.ZZNB(suffix);
			break;
		case "ZZNC":
			text = cmdlist.ZZNC(suffix);
			break;
		case "ZZND":
			text = cmdlist.ZZND(suffix);
			break;
		case "ZZNE":
			text = cmdlist.ZZNE(suffix);
			break;
		case "ZZNF":
			text = cmdlist.ZZNF(suffix);
			break;
		case "ZZNG":
			text = cmdlist.ZZNG(suffix);
			break;
		case "ZZNH":
			text = cmdlist.ZZNH(suffix);
			break;
		case "ZZNL":
			text = cmdlist.ZZNL(suffix);
			break;
		case "ZZNM":
			text = cmdlist.ZZNM(suffix);
			break;
		case "ZZNN":
			text = cmdlist.ZZNN(suffix);
			break;
		case "ZZNO":
			text = cmdlist.ZZNO(suffix);
			break;
		case "ZZNR":
			text = cmdlist.ZZNR(suffix);
			break;
		case "ZZNS":
			text = cmdlist.ZZNS(suffix);
			break;
		case "ZZNT":
			text = cmdlist.ZZNT(suffix);
			break;
		case "ZZNU":
			text = cmdlist.ZZNU(suffix);
			break;
		case "ZZNV":
			text = cmdlist.ZZNV(suffix);
			break;
		case "ZZNW":
			text = cmdlist.ZZNW(suffix);
			break;
		case "ZZOA":
			text = cmdlist.ZZOA(suffix);
			break;
		case "ZZOB":
			text = cmdlist.ZZOB(suffix);
			break;
		case "ZZOC":
			text = cmdlist.ZZOC(suffix);
			break;
		case "ZZOD":
			text = cmdlist.ZZOD(suffix);
			break;
		case "ZZOE":
			text = cmdlist.ZZOE(suffix);
			break;
		case "ZZOF":
			text = cmdlist.ZZOF(suffix);
			break;
		case "ZZOG":
			text = cmdlist.ZZOG(suffix);
			break;
		case "ZZOH":
			text = cmdlist.ZZOH(suffix);
			break;
		case "ZZOJ":
			text = cmdlist.ZZOJ(suffix);
			break;
		case "ZZOL":
			text = cmdlist.ZZOL(suffix);
			break;
		case "ZZOS":
			text = cmdlist.ZZOS(suffix);
			break;
		case "ZZOT":
			text = cmdlist.ZZOT(suffix);
			break;
		case "ZZOU":
			text = cmdlist.ZZOU(suffix);
			break;
		case "ZZOV":
			text = cmdlist.ZZOV(suffix);
			break;
		case "ZZOW":
			text = cmdlist.ZZOW(suffix);
			break;
		case "ZZOX":
			text = cmdlist.ZZOX(suffix);
			break;
		case "ZZOZ":
			text = cmdlist.ZZOZ(suffix);
			break;
		case "ZZPA":
			text = cmdlist.ZZPA(suffix);
			break;
		case "ZZPB":
			text = cmdlist.ZZPB(suffix);
			break;
		case "ZZPC":
			text = cmdlist.ZZPC(suffix);
			break;
		case "ZZPD":
			text = cmdlist.ZZPD();
			break;
		case "ZZPE":
			text = cmdlist.ZZPE(suffix);
			break;
		case "ZZPO":
			text = cmdlist.ZZPO(suffix);
			break;
		case "ZZPS":
			text = cmdlist.ZZPS(suffix);
			break;
		case "ZZPY":
			text = cmdlist.ZZPY(suffix);
			break;
		case "ZZPZ":
			text = cmdlist.ZZPZ(suffix);
			break;
		case "ZZQK":
			text = cmdlist.ZZQK(suffix);
			break;
		case "ZZQM":
			text = cmdlist.ZZQM();
			break;
		case "ZZQR":
			text = cmdlist.ZZQR();
			break;
		case "ZZQS":
			text = cmdlist.ZZQS();
			break;
		case "ZZRA":
			text = cmdlist.ZZRA(suffix);
			break;
		case "ZZRB":
			text = cmdlist.ZZRB(suffix);
			break;
		case "ZZRC":
			text = cmdlist.ZZRC();
			break;
		case "ZZRD":
			text = cmdlist.ZZRD(suffix);
			break;
		case "ZZRF":
			text = cmdlist.ZZRF(suffix);
			break;
		case "ZZRH":
			text = cmdlist.ZZRH(suffix);
			break;
		case "ZZRL":
			text = cmdlist.ZZRL(suffix);
			break;
		case "ZZRM":
			text = cmdlist.ZZRM(suffix);
			break;
		case "ZZRS":
			text = cmdlist.ZZRS(suffix);
			break;
		case "ZZRT":
			text = cmdlist.ZZRT(suffix);
			break;
		case "ZZRU":
			text = cmdlist.ZZRU(suffix);
			break;
		case "ZZRV":
			text = cmdlist.ZZRV();
			break;
		case "ZZRX":
			text = cmdlist.ZZRX(suffix);
			break;
		case "ZZRY":
			text = cmdlist.ZZRY(suffix);
			break;
		case "ZZSA":
			text = cmdlist.ZZSA();
			break;
		case "ZZSB":
			text = cmdlist.ZZSB();
			break;
		case "ZZSD":
			text = cmdlist.ZZSD();
			break;
		case "ZZSF":
			text = cmdlist.ZZSF(suffix);
			break;
		case "ZZSG":
			text = cmdlist.ZZSG();
			break;
		case "ZZSH":
			text = cmdlist.ZZSH();
			break;
		case "ZZSM":
			text = cmdlist.ZZSM(suffix);
			break;
		case "ZZSN":
			text = cmdlist.ZZSN();
			break;
		case "ZZSO":
			text = cmdlist.ZZSO(suffix);
			break;
		case "ZZSP":
			text = cmdlist.ZZSP(suffix, bFromCatDirect: true);
			break;
		case "ZZSQ":
			text = cmdlist.ZZSQ(suffix);
			break;
		case "ZZSR":
			text = cmdlist.ZZSR(suffix);
			break;
		case "ZZSS":
			text = cmdlist.ZZSS();
			break;
		case "ZZST":
			text = cmdlist.ZZST();
			break;
		case "ZZSU":
			text = cmdlist.ZZSU();
			break;
		case "ZZSV":
			text = cmdlist.ZZSV(suffix);
			break;
		case "ZZSW":
			text = cmdlist.ZZSW(suffix);
			break;
		case "ZZSX":
			text = cmdlist.ZZSX(suffix);
			break;
		case "ZZSY":
			text = cmdlist.ZZSY(suffix);
			break;
		case "ZZSZ":
			text = cmdlist.ZZSZ(suffix);
			break;
		case "ZZTA":
			text = cmdlist.ZZTA(suffix);
			break;
		case "ZZTB":
			text = cmdlist.ZZTB(suffix);
			break;
		case "ZZTF":
			text = cmdlist.ZZTF(suffix);
			break;
		case "ZZTH":
			text = cmdlist.ZZTH(suffix);
			break;
		case "ZZTI":
			text = cmdlist.ZZTI(suffix);
			break;
		case "ZZTP":
			text = cmdlist.ZZTP(suffix);
			break;
		case "ZZTL":
			text = cmdlist.ZZTL(suffix);
			break;
		case "ZZTM":
			text = cmdlist.ZZTM(suffix);
			break;
		case "ZZTO":
			text = cmdlist.ZZTO(suffix);
			break;
		case "ZZTS":
			text = cmdlist.ZZTS();
			break;
		case "ZZTU":
			text = cmdlist.ZZTU(suffix);
			break;
		case "ZZTV":
			text = cmdlist.ZZTV(suffix);
			break;
		case "ZZTX":
			text = cmdlist.ZZTX(suffix);
			break;
		case "ZZUA":
			text = cmdlist.ZZUA();
			break;
		case "ZZUP":
			text = cmdlist.ZZUP(suffix);
			break;
		case "ZZUS":
			text = cmdlist.ZZUS();
			break;
		case "ZZUT":
			text = cmdlist.ZZUT(suffix);
			break;
		case "ZZUX":
			text = cmdlist.ZZUX(suffix);
			break;
		case "ZZUY":
			text = cmdlist.ZZUY(suffix);
			break;
		case "ZZVA":
			text = cmdlist.ZZVA(suffix);
			break;
		case "ZZVB":
			text = cmdlist.ZZVB(suffix);
			break;
		case "ZZVC":
			text = cmdlist.ZZVC(suffix);
			break;
		case "ZZVD":
			text = cmdlist.ZZVD(suffix);
			break;
		case "ZZVE":
			text = cmdlist.ZZVE(suffix);
			break;
		case "ZZVF":
			text = cmdlist.ZZVF(suffix);
			break;
		case "ZZVG":
			text = cmdlist.ZZVG(suffix);
			break;
		case "ZZVH":
			text = cmdlist.ZZVH(suffix);
			break;
		case "ZZVI":
			text = cmdlist.ZZVI(suffix);
			break;
		case "ZZVJ":
			text = cmdlist.ZZVJ(suffix);
			break;
		case "ZZVK":
			text = cmdlist.ZZVK(suffix);
			break;
		case "ZZVL":
			text = cmdlist.ZZVL(suffix);
			break;
		case "ZZVM":
			text = cmdlist.ZZVM(suffix);
			break;
		case "ZZVN":
			text = cmdlist.ZZVN();
			break;
		case "ZZVO":
			text = cmdlist.ZZVO(suffix);
			break;
		case "ZZVP":
			text = cmdlist.ZZVP(suffix);
			break;
		case "ZZVQ":
			text = cmdlist.ZZVQ(suffix);
			break;
		case "ZZVR":
			text = cmdlist.ZZVR(suffix);
			break;
		case "ZZVS":
			text = cmdlist.ZZVS(suffix);
			break;
		case "ZZVT":
			text = cmdlist.ZZVT(suffix);
			break;
		case "ZZVU":
			text = cmdlist.ZZVU(suffix);
			break;
		case "ZZVV":
			text = cmdlist.ZZVV(suffix);
			break;
		case "ZZVW":
			text = cmdlist.ZZVW(suffix);
			break;
		case "ZZVX":
			text = cmdlist.ZZVX(suffix);
			break;
		case "ZZVY":
			text = cmdlist.ZZVY(suffix);
			break;
		case "ZZVZ":
			text = cmdlist.ZZVZ(suffix);
			break;
		case "ZZWA":
			text = cmdlist.ZZWA(suffix);
			break;
		case "ZZWB":
			text = cmdlist.ZZWB(suffix);
			break;
		case "ZZWC":
			text = cmdlist.ZZWC(suffix);
			break;
		case "ZZWD":
			text = cmdlist.ZZWD(suffix);
			break;
		case "ZZWE":
			text = cmdlist.ZZWE(suffix);
			break;
		case "ZZWF":
			text = cmdlist.ZZWF(suffix);
			break;
		case "ZZWG":
			text = cmdlist.ZZWG(suffix);
			break;
		case "ZZWH":
			text = cmdlist.ZZWH(suffix);
			break;
		case "ZZWJ":
			text = cmdlist.ZZWJ(suffix);
			break;
		case "ZZWK":
			text = cmdlist.ZZWK(suffix);
			break;
		case "ZZWL":
			text = cmdlist.ZZWL(suffix);
			break;
		case "ZZWM":
			text = cmdlist.ZZWM(suffix);
			break;
		case "ZZWN":
			text = cmdlist.ZZWN(suffix);
			break;
		case "ZZWO":
			text = cmdlist.ZZWO(suffix);
			break;
		case "ZZWP":
			text = cmdlist.ZZWP(suffix);
			break;
		case "ZZWQ":
			text = cmdlist.ZZWQ(suffix);
			break;
		case "ZZWR":
			text = cmdlist.ZZWR(suffix);
			break;
		case "ZZWS":
			text = cmdlist.ZZWS(suffix);
			break;
		case "ZZWT":
			text = cmdlist.ZZWT(suffix);
			break;
		case "ZZWU":
			text = cmdlist.ZZWU(suffix);
			break;
		case "ZZWV":
			text = cmdlist.ZZWV(suffix);
			break;
		case "ZZWW":
			text = cmdlist.ZZWW(suffix);
			break;
		case "ZZXC":
			text = cmdlist.ZZXC();
			break;
		case "ZZXD":
			text = cmdlist.ZZXD(suffix);
			break;
		case "ZZXF":
			text = cmdlist.ZZXF(suffix);
			break;
		case "ZZXH":
			text = cmdlist.ZZXH(suffix);
			break;
		case "ZZXN":
			text = cmdlist.ZZXN(suffix);
			break;
		case "ZZXO":
			text = cmdlist.ZZXO(suffix);
			break;
		case "ZZXS":
			text = cmdlist.ZZXS(suffix);
			break;
		case "ZZXT":
			text = cmdlist.ZZXT(suffix);
			break;
		case "ZZXU":
			text = cmdlist.ZZXU(suffix);
			break;
		case "ZZXV":
			text = cmdlist.ZZXV(suffix);
			break;
		case "ZZYA":
			text = cmdlist.ZZYA(suffix);
			break;
		case "ZZYB":
			text = cmdlist.ZZYB(suffix);
			break;
		case "ZZYC":
			text = cmdlist.ZZYC(suffix);
			break;
		case "ZZYR":
			text = cmdlist.ZZYR(suffix);
			break;
		case "ZZZA":
			text = cmdlist.ZZZA(suffix);
			break;
		case "ZZZB":
			text = cmdlist.ZZZB();
			break;
		case "ZZZD":
			text = cmdlist.ZZZD(suffix);
			break;
		case "ZZZE":
			text = cmdlist.ZZZE(suffix);
			break;
		case "ZZZM":
			text = cmdlist.ZZZM(suffix);
			break;
		case "ZZZP":
			text = cmdlist.ZZZP(suffix);
			break;
		case "ZZZQ":
			text = cmdlist.ZZZQ(suffix);
			break;
		case "ZZZR":
			text = cmdlist.ZZZR(suffix);
			break;
		case "ZZZS":
			text = cmdlist.ZZZS(suffix);
			break;
		case "ZZZT":
			text = cmdlist.ZZZT(suffix);
			break;
		case "ZZZU":
			text = cmdlist.ZZZU(suffix);
			break;
		case "ZZZV":
			text = cmdlist.ZZZV(suffix);
			break;
		case "ZZZW":
			text = cmdlist.ZZZW(suffix);
			break;
		case "ZZZZ":
			text = cmdlist.ZZZZ();
			break;
		case "ZZZN":
			text = cmdlist.ZZZN(suffix);
			break;
		case "ZZZO":
			text = cmdlist.ZZZO(suffix);
			break;
		case "ZZXA":
			text = cmdlist.ZZXA(suffix);
			break;
		}
		if (!text.Contains(Error1))
		{
			if (text.Length == nAns && nAns > 0)
			{
				if (text.StartsWith(" ") && extension != "ML" && extension != "MN")
				{
					text = text.Trim();
				}
				text = prefix + extension + suffix + text + ";";
			}
		}
		else
		{
			text = ProcessError(text);
		}
		return text;
	}

	private string ProcessError(string error)
	{
		string text = "";
		if (Verbose)
		{
			string text2 = ":";
			string text3 = "";
			text3 = ((prefix.Length != 0 || suffix.Length != 0) ? (prefix + extension) : current_cat);
			text = "ZZEM" + text2 + text3 + suffix + text2;
			return verbose_error_code switch
			{
				1 => text + "Bad Command Name;", 
				2 => text + "Inactive Command;", 
				3 => text + "Unknown Command;", 
				4 => text + "Undefined Command Error;", 
				5 => text + "Suffix Format Error;", 
				6 => text + "Suffix Length Error;", 
				7 => text + "Feature Not Available;", 
				8 => text + "Form Must Be Open;", 
				9 => text + "Value out of bounds;", 
				10 => text + "Power must be on;", 
				_ => text + "Undefined Error;", 
			};
		}
		return error;
	}
}
