using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Thetis;

internal class NetworkIO
{
	private static float _swr_protect = 1f;

	private static double[][] _lastVFOfreq = new double[2][]
	{
		new double[4],
		new double[1]
	};

	private static double _freq_correction_factor = 1.0;

	private static double _low_freq_offset;

	private static double _high_freq_offset;

	public static HPSDRHW BoardID { get; set; } = HPSDRHW.Hermes;

	public static RadioProtocol CurrentRadioProtocol { get; set; } = RadioProtocol.ETH;

	public static RadioProtocol SelectedRadioProtocol { get; set; } = RadioProtocol.ETH;

	public static byte BetaVersion { get; set; } = 0;

	public static byte FWCodeVersion { get; set; } = 0;

	public static byte HwRev { get; set; } = 0;

	public static bool Supports24BitAudio => (HwRev & 0x10) != 0;

	public static byte Protocol2VersionSupported { get; set; } = 0;

	public static bool FWVersionsChecked { get; set; } = false;

	public static string GetFWVersionErrorMsg { get; set; } = "";

	public static string BoardMismatch { get; set; } = "";

	public static float SWRProtect
	{
		get
		{
			return _swr_protect;
		}
		set
		{
			_swr_protect = value;
		}
	}

	public static double FreqCorrectionFactor
	{
		get
		{
			return _freq_correction_factor;
		}
		set
		{
			_freq_correction_factor = value;
			FreqCorrectionChanged();
		}
	}

	public static double LowFreqOffset
	{
		get
		{
			return _low_freq_offset;
		}
		set
		{
			_low_freq_offset = value;
		}
	}

	public static double HighFreqOffset
	{
		get
		{
			return _high_freq_offset;
		}
		set
		{
			_high_freq_offset = value;
		}
	}

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetOutputPowerFactor(int i);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void DeInitMetisSockets();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern int nativeInitMetis(string netaddr, int port, string localaddr, int localport, int protocol, int model_id);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int SetXVTREnable(int enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetWBPacketsPerFrame(int pps);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetWBUpdateRate(int ur);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetWBEnable(int adc, int enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendHighPriority(int enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDDCRate(int id, int rate);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void CmdRx();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getOOO();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getSeqInDelta(bool bInit, int rx, int[] deltas, StringBuilder dateTimeStamp, out uint received_seqnum, out uint last_seqnum);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void clearSnapshots();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "create_rnet")]
	public static extern void CreateRNet();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "destroy_rnet")]
	public static extern void DestroyRNet();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetMetisIPAddr();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void GetMACAddr(byte[] addr_bytes);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void GetCodeVersion(byte[] addr_bytes);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void GetBoardID(byte[] addr_bytes);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int StartAudioNative();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int StopAudio();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAlexHPFBits(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAlexLPFBits(int bits, bool isTX, bool isMox);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void DisablePA(int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTRXrelay(int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAlex2HPFBits(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetBPF2Gnd(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAlex2LPFBits(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableApolloFilter(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SelectApolloFilter(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableApolloTuner(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableApolloAutoTune(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableEClassModulation(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERPWMmin(int min);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERPWMmax(int max);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAudioAmpEnable(bool enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getUserADC0();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getUserADC1();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getUserADC2();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getUserADC3();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetUserOut0(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetUserOut1(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetUserOut2(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetUserOut3(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI01();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI02();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI03();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI04();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI04_p2();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI05_p2();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI06_p2();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI08_p2();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool getUserI02_p2();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPureSignal(int enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableRx(int id, int enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableRxs(int Rxs);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableRxSync(int id, int sync);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void Protocol1DDCConfig(int ddcconfig, int en_diversity, int rxcount, int nddc);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int nativeGetDotDashPTT();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPttOut(int xmitbit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetVFOfreq(int id, int freq, int tx);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetWatchdogTimer(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMicXlr(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAudio24(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void NotifyRxReconfig();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMicBoost(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetLineIn(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetLineBoost(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAlexAtten(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADCDither(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADCRandom(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTxAttenData(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRX1Preamp(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRX2Preamp(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADC1StepAttenData(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADC2StepAttenData(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADC3StepAttenData(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMicTipRing(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMicBias(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMicPTT(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getAndResetADC_Overload();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern ushort getADCmaxMagnitude(int adc);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern ushort getAndResetADCmaxMagnitudeAtOverload(int adc);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getHaveSync();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getExciterPower();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern float getRevPower();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern float getFwdPower();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getHermesDCVoltage();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableCWKeyer(int enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetSidetoneRun(int id, int enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetSidetoneVolume(int id, double volume);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWSidetoneVolume(int vol);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWPTTDelay(int delay);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWHangTime(int hang);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWSidetoneFreq(int freq);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWKeyerSpeed(int speed);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWKeyerMode(int mode);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWKeyerWeight(int weight);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWEdgeLength(int edge_length);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void EnableCWKeyerSpacing(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void ReversePaddles(int bits);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWDash(int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWDot(int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWX(int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWIambic(int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWBreakIn(int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCWSidetone(int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetOCBits(int b);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetOCExtraBits(int b);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAntBits(int rx_ant, int trx_ant, int tx_ant, int rx_out, bool tx);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetMKIIBPF(int bpf);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRxADC(int n);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADC_cntrl1(int g);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetADC_cntrl1();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADC_cntrl2(int g);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetADC_cntrl2();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADC_cntrl_P1(int g);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetADC_cntrl_P1();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool GetPLLLock();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void ATU_Tune(int tune);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int SendStartToMetis();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int SendStopToMetis();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void LRAudioSwap(int swap);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCATPort(int port);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double GetInboundBps();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double GetOutboundBps();

	public static int InitRadio()
	{
		int num = -1;
		Console console = Console.getConsole();
		if (console.IsSetupFormNull)
		{
			return -1;
		}
		if (console.SetupForm.SelectedRadioList == null)
		{
			return -1;
		}
		ucRadioList selectedRadioList = console.SetupForm.SelectedRadioList;
		if (selectedRadioList == null)
		{
			return -1;
		}
		bool flag = true;
		if (selectedRadioList.IsFirstRadioFoundSelected)
		{
			console.SetupForm.ScanForFirstFoundRadio();
			flag = false;
		}
		NicRadioScanResult selectedNICDetails = selectedRadioList.SelectedNICDetails;
		RadioInfo radioInfo = selectedRadioList.SelectedRadioDetails;
		if (selectedNICDetails == null)
		{
			return -105;
		}
		if (radioInfo == null)
		{
			return -106;
		}
		string text = radioInfo.IpAddress.ToString();
		int discoveryPortBase = radioInfo.DiscoveryPortBase;
		string text2 = selectedNICDetails.LocalIPv4.ToString();
		int listenToRadioOnUDPPort = console.SetupForm.ListenToRadioOnUDPPort;
		int model = (int)HardwareSpecific.Model;
		int num2 = ((radioInfo.Protocol != RadioDiscoveryRadioProtocol.P1) ? 1 : 0);
		if (!radioInfo.IsCustom & flag)
		{
			RadioDiscoveryOptions radioDiscoveryOptions = new RadioDiscoveryOptions();
			radioDiscoveryOptions.IgnoreSubnetCheck = true;
			radioDiscoveryOptions.IncludeEthernet = true;
			radioDiscoveryOptions.IncludeWireless = true;
			radioDiscoveryOptions.IncludeOtherInterfaceTypes = !console.SetupForm.LimitInterfacesToEthernetWifi;
			radioDiscoveryOptions.AllowLoopback = false;
			radioDiscoveryOptions.AllowAPIPA = true;
			radioDiscoveryOptions.IncludeGeneralBroadcast = true;
			radioDiscoveryOptions.DiscoveryPortBase = discoveryPortBase;
			radioDiscoveryOptions.BindLocalPort = listenToRadioOnUDPPort;
			radioDiscoveryOptions.ScanPerformance = console.SetupForm.SelectedDiscoveryProfile;
			if (!IPAddress.TryParse(text, out var address))
			{
				return -103;
			}
			radioDiscoveryOptions.FixedTargetIp = address;
			if (!IPAddress.TryParse(text2, out address))
			{
				return -104;
			}
			radioDiscoveryOptions.FixedLocalIp = address;
			if (console.SetupForm.NetworkProtocolMustMatch)
			{
				switch (num2)
				{
				case 0:
					radioDiscoveryOptions.ProtocolMode = RadioDiscoveryProtocolMode.P1Only;
					break;
				case 1:
					radioDiscoveryOptions.ProtocolMode = RadioDiscoveryProtocolMode.P2Only;
					break;
				default:
					return -102;
				}
			}
			else
			{
				radioDiscoveryOptions.ProtocolMode = RadioDiscoveryProtocolMode.Auto;
			}
			NicRadioScanResult nicRadioScanResult = new RadioDiscoveryService().DiscoverUsingSingleNic(radioDiscoveryOptions, radioDiscoveryOptions.FixedLocalIp);
			if (nicRadioScanResult == null || nicRadioScanResult.Radios == null || nicRadioScanResult.Radios.Count != 1)
			{
				return -1;
			}
			radioInfo = nicRadioScanResult.Radios[0];
			num2 = ((radioInfo.Protocol != RadioDiscoveryRadioProtocol.P1) ? 1 : 0);
			if (radioInfo.DeviceType == HPSDRHW.HermesII && radioInfo.CodeVersion < 103)
			{
				GetFWVersionErrorMsg = "Invalid Firmware!\nRequires 10.3 or greater. ";
				return -101;
			}
			selectedRadioList.UpdateSelectedDetails(nicRadioScanResult, nicRadioScanResult.Radios[0]);
		}
		num = nativeInitMetis(text, discoveryPortBase, text2, listenToRadioOnUDPPort, num2, model);
		if (num == 0)
		{
			BoardID = radioInfo.DeviceType;
			CurrentRadioProtocol = (RadioProtocol)num2;
			FWCodeVersion = radioInfo.CodeVersion;
			BetaVersion = radioInfo.BetaVersion;
			Protocol2VersionSupported = radioInfo.Protocol2Supported;
			HwRev = radioInfo.HwRev;
			SetAudio24(Supports24BitAudio ? 1 : 0);
			bool flag2;
			switch (HardwareSpecific.Model)
			{
			case HPSDRModel.ANVELINAPRO3:
				flag2 = BoardID == HPSDRHW.OrionMKII;
				break;
			case HPSDRModel.REDPITAYA:
				flag2 = BoardID == HPSDRHW.Hermes || BoardID == HPSDRHW.OrionMKII;
				break;
			case HPSDRModel.ANAN10:
			case HPSDRModel.ANAN10E:
			case HPSDRModel.ANAN100:
			case HPSDRModel.ANAN100B:
				flag2 = BoardID == HPSDRHW.Hermes || BoardID == HPSDRHW.HermesII;
				break;
			default:
				flag2 = BoardID == HardwareSpecific.Hardware;
				break;
			}
			if (!flag2)
			{
				BoardMismatch = "The board returned from network query was: " + BoardID.ToString() + "\nExpected board for model selected is: " + HardwareSpecific.Hardware.ToString() + "\n\nIf this is expected you can ignore this warning";
			}
			else
			{
				BoardMismatch = "";
			}
		}
		return num;
	}

	public static void SetOutputPower(float f)
	{
		if ((double)f < 0.0)
		{
			f = 0f;
		}
		if ((double)f >= 1.0)
		{
			f = 1f;
		}
		SetOutputPowerFactor((int)(255f * f * _swr_protect));
	}

	public static void VFOfreq(int id, double f, int tx)
	{
		_lastVFOfreq[tx][id] = f;
		int num = (int)(f * 1000000.0 * _freq_correction_factor);
		if (num >= 0)
		{
			if (CurrentRadioProtocol == RadioProtocol.USB)
			{
				SetVFOfreq(id, num, tx);
			}
			else
			{
				SetVFOfreq(id, Freq2PhaseWord(num), tx);
			}
		}
	}

	public static void FreqCorrectionChanged()
	{
		if (!Console.FreqCalibrationRunning)
		{
			VFOfreq(0, _lastVFOfreq[0][0], 0);
			VFOfreq(1, _lastVFOfreq[0][1], 0);
			VFOfreq(2, _lastVFOfreq[0][2], 0);
			VFOfreq(3, _lastVFOfreq[0][3], 0);
			VFOfreq(0, _lastVFOfreq[1][0], 1);
		}
	}

	public static int Freq2PhaseWord(int freq)
	{
		return (int)((long)Math.Pow(2.0, 32.0) * freq / 122880000);
	}
}
