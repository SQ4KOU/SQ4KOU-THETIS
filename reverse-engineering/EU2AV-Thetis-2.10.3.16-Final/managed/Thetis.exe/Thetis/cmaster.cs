using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

internal class cmaster
{
	private sealed class TCIIQBlock
	{
		public int Receiver;

		public int SampleRate;

		public int ComplexSamples;

		public float[] Samples;
	}

	private sealed class TCIAudioBlock
	{
		public int Receiver;

		public int SampleRate;

		public int SamplesPerChannel;

		public float[] Left;

		public float[] Right;
	}

	public unsafe delegate void TCIStreamSamples(int id, int nsamples, double* data);

	public unsafe delegate void TCITxInput(int nsamples, double* data);

	public delegate void PushVox(int channel, int active);

	private static bool mox = false;

	private static bool _MONmixState = false;

	private static int cmRCVR = 5;

	private static int cmSubRCVR = 2;

	private static int ps_rate = 192000;

	private static readonly object m_objTCIStreamQueueLock = new object();

	private static readonly object m_objTCIStreamPoolLock = new object();

	private static readonly object m_objTCIIQResamplerLock = new object();

	private static readonly int[] m_tciIQResamplerInputRates = new int[cmRCVR];

	private static readonly int[] m_tciIQResamplerOutputRates = new int[cmRCVR];

	private unsafe static void*[] m_tciIQResamplerI = new void*[cmRCVR];

	private unsafe static void*[] m_tciIQResamplerQ = new void*[cmRCVR];

	private static readonly Dictionary<int, Stack<float[]>> m_tciFloatBufferPool = new Dictionary<int, Stack<float[]>>();

	private static readonly Stack<TCIIQBlock> m_tciIQBlockPool = new Stack<TCIIQBlock>();

	private static readonly Stack<TCIAudioBlock> m_tciAudioBlockPool = new Stack<TCIAudioBlock>();

	private static readonly Queue<TCIIQBlock> m_tciIQQueue = new Queue<TCIIQBlock>();

	private static readonly Queue<TCIAudioBlock> m_tciAudioQueue = new Queue<TCIAudioBlock>();

	private static Thread m_tciRxThread;

	private static readonly AutoResetEvent m_tciRxStreamEvent = new AutoResetEvent(initialState: false);

	private static readonly object m_objTCITxStateLock = new object();

	private static readonly object m_objTCITxResamplerLock = new object();

	private static readonly Queue<float[]> m_tciTxSampleQueue = new Queue<float[]>();

	private static Thread m_tciTxThread;

	private static readonly AutoResetEvent m_tciTxStreamEvent = new AutoResetEvent(initialState: false);

	private static int m_tciTxSampleQueueOffset = 0;

	private static int m_tciTxQueuedSamples = 0;

	private static int m_tciTxChronoOutstanding = 0;

	private static int m_tciTxChronoReceiver = 0;

	private static long m_tciTxLastChronoTick = 0L;

	private static int m_tciTxInputRate = 0;

	private static int m_tciTxResamplerInputRate = 0;

	private static int m_tciTxResamplerOutputRate = 0;

	private unsafe static void* m_tciTxResampler = null;

	private const int TCI_TX_MAX_OUTSTANDING = 64;

	private const int TCI_TX_EXTRA_BUFFER_MS = 50;

	private const int TCI_MAX_IQ_STREAM_RATE = 384000;

	private const int TCI_MAX_POOLED_BLOCKS = 64;

	private const int TCI_MAX_POOLED_BUFFERS_PER_SIZE = 32;

	private static volatile bool m_runTCIStreamThreads = false;

	private static bool ps_loopback = false;

	private unsafe static TCIStreamSamples TCIRxIQOutDel = OnTCIRxIQOutSamples;

	private unsafe static TCIStreamSamples TCIRxAudioOutDel = OnTCIRxAudioOutSamples;

	private unsafe static TCITxInput TCITxAudioInDel = OnTCITxAudioInSamples;

	private static int m_cachedTxInputRate = 0;

	private static volatile TCPIPtciServer _tciServer = null;

	private static PushVox PushVoxDel = VOX.PushVox;

	private static bool EXPOSEwb = true;

	private static wideband[] wideband = new wideband[3];

	public unsafe static bool Mox
	{
		get
		{
			return mox;
		}
		set
		{
			mox = value;
			m_cachedTxInputRate = 0;
			if (mox)
			{
				LoadRouterControlBit(null, 0, 2, 1);
				WaveThing.wplayer[0].Condx = 1;
				WaveThing.wplayer[1].Condx = 1;
				WaveThing.wrecorder[0].Condx = 1;
				WaveThing.wrecorder[1].Condx = 1;
				Scope.dscope[0].Condx = 1;
			}
			else
			{
				LoadRouterControlBit(null, 0, 2, 0);
				WaveThing.wplayer[0].Condx = 0;
				WaveThing.wplayer[1].Condx = 0;
				WaveThing.wrecorder[0].Condx = 0;
				WaveThing.wrecorder[1].Condx = 0;
				Scope.dscope[0].Condx = 0;
			}
			CMSetEERRun(0);
		}
	}

	public static bool MONMixState
	{
		get
		{
			return _MONmixState;
		}
		set
		{
			_MONmixState = value;
		}
	}

	public static int CMrcvr => cmRCVR;

	public static int CMsubrcvr => cmSubRCVR;

	public static int PSrate
	{
		get
		{
			return ps_rate;
		}
		set
		{
			ps_rate = value;
			puresignal.SetPSFeedbackRate(chid(inid(1, 0), 0), ps_rate);
		}
	}

	public static RadioProtocol CurrentRadioProtocol { get; set; }

	public static bool PSLoopback
	{
		get
		{
			return ps_loopback;
		}
		set
		{
			ps_loopback = value;
			if (Audio.console != null)
			{
				CMLoadRouterAll(HardwareSpecific.Model);
			}
		}
	}

	public static TCPIPtciServer TCIServer
	{
		get
		{
			return _tciServer;
		}
		set
		{
			_tciServer = value;
		}
	}

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetRadioStructure(int cmSTREAM, int cmRCVR, int cmXMTR, int cmSubRCVR, int cmNspc, int* cmSPC, int* cmMAXInbound, int cmMAXInRate, int cmMAXAudioRate, int cmMAXTxOutRate);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void CreateRadio();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void DestroyRadio();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "set_cmdefault_rates")]
	public unsafe static extern void SetCMDefaultRates(int* xcm_inrates, int aud_outrate, int* rcvr_ch_outrates, int* xmtr_ch_outrates);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetXcmInrate(int in_id, int rate);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetXmtrChannelOutrate(int xmtr_id, int rate, bool state);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getbuffsize")]
	public static extern int GetBuffSize(int rate);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int inid(int stype, int id);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void Inbound(int id, int nsamples, double* data);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int chid(int stream, int subrx);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getInputRate")]
	public static extern int GetInputRate(int stype, int id);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getChannelOutputRate")]
	public static extern int GetChannelOutputRate(int stype, int id);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getCMAstate")]
	public static extern int GetCMAstate();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void CM_WaterfallIQ_Init(int stream, int capacitySamples);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void CM_WaterfallIQ_Free(int stream);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void CM_WaterfallIQ_SetEnabled(int stream, int enable);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int CM_WaterfallIQ_Available(int stream);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern int CM_WaterfallIQ_Get(int stream, float* outI, float* outQ, int maxSamples);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int CM_WaterfallIQ_DroppedSamples(int stream);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void CM_WaterfallIQ_ResetDropped(int stream);

	public static void InitWaterfallIQ(int capacitySamples)
	{
		for (int i = 0; i < cmRCVR; i++)
		{
			CM_WaterfallIQ_Init(inid(0, i), capacitySamples);
		}
	}

	public static void FreeWaterfallIQ()
	{
		for (int i = 0; i < cmRCVR; i++)
		{
			CM_WaterfallIQ_Free(inid(0, i));
		}
	}

	public static void SetWaterfallIQEnabled(bool enabled)
	{
		for (int i = 0; i < cmRCVR; i++)
		{
			CM_WaterfallIQ_SetEnabled(inid(0, i), enabled ? 1 : 0);
		}
	}

	public static int GetWaterfallIQDroppedSamples(int stream)
	{
		return CM_WaterfallIQ_DroppedSamples(stream);
	}

	public static void ResetWaterfallIQDropped(int stream)
	{
		CM_WaterfallIQ_ResetDropped(stream);
	}

	public unsafe static int ReadWaterfallIQ(int stream, float[] outI, float[] outQ, int maxSamples)
	{
		if (outI == null || outQ == null || outI.Length < maxSamples || outQ.Length < maxSamples)
		{
			return 0;
		}
		int num;
		fixed (float* outI2 = outI)
		{
			fixed (float* outQ2 = outQ)
			{
				num = CM_WaterfallIQ_Get(stream, outI2, outQ2, maxSamples);
			}
		}
		for (int i = 0; i < num; i++)
		{
			float num2 = outI[i];
			outI[i] = outQ[i];
			outQ[i] = num2;
		}
		return num;
	}

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendpOutboundTCIRxIQ(TCIStreamSamples del);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendpOutboundTCIRxAudio(TCIStreamSamples del);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendpInboundTCITxAudio(TCITxInput del);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXTCIRun(int active);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXTCIAudioRun(int txid, int active);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTCIRxAudioMox(int id, int mox);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTCIRxAudioMon(int id, int mon);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTCIRxAudioMonVol(int id, double vol);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void LoadRouterAll(void* ptr, int id, int sources, int calls, int varvals, int* nstreams, int* function, int* callid);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void LoadRouterControlBit(void* ptr, int id, int var_number, int bit);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTopPan3Run(bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRunPanadapter(int id, bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSTxIdx(int id, int idx);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPSRxIdx(int id, int idx);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "alloc_analyzer")]
	public static extern int AllocAnalyzer(int stype, int id, int max_fft_size);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "free_analyzer")]
	public static extern int FreeAnalyzer(int disp);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "run_analyzer")]
	public static extern int RunAnalyzer(int disp, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetDEXPAttackThreshold")]
	public static extern void SetTXAVoxThresh(int id, double thresh);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetDEXPPeakSignal(int id, double* peak);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPRun(int id, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPDetectorTau(int id, double tau);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPAttackTime(int id, double time);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPReleaseTime(int id, double time);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPHoldTime(int id, double time);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPExpansionRatio(int id, double ratio);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPHysteresisRatio(int id, double ratio);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPAttackThreshold(int id, double thresh);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPLowCut(int id, double lowcut);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPHighCut(int id, double highcut);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPRunSideChannelFilter(int id, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPRunVox(int id, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPRunAudioDelay(int id, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDEXPAudioDelay(int id, double delay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAntiVOXRun(int id, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAntiVOXGain(int id, double gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAntiVOXDetectorTau(int id, double tau);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAntiVOXSourceStates(int txid, int streams, int states);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAntiVOXSourceWhat(int txid, int stream, int state);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetSiphonInsize(int id, int size);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetaSipF1EXT(int id, float* buff, int size);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetAAudioMixWhat(void* ptr, int id, int stream, bool state);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetAAudioMixState(void* ptr, int id, int stream, bool state);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetAAudioMixStates(void* ptr, int id, int streams, int states);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetAAudioMixVolume(void* ptr, int id, double volume);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetAAudioMixVol(void* ptr, int id, int stream, double vol);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXVAC(int txid, int txvac);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXFixedGainRun(int id, bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXFixedGain(int id, double Igain, double Qgain);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERRun(int id, bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERAMIQ(int id, bool amiq);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERMgain(int id, double gain);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERPgain(int id, double gain);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERRunDelays(int id, bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERMdelay(int id, double delay);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERPdelay(int id, double delay);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRANBRun(int stype, int id, bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRANBTau(int stype, int id, double tau);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRANBHangtime(int stype, int id, double time);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRANBAdvtime(int stype, int id, double time);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRANBBacktau(int stype, int id, double tau);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRANBThreshold(int stype, int id, double thresh);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRNOBRun(int stype, int id, bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRNOBMode(int stype, int id, int mode);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRNOBTau(int stype, int id, double tau);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRNOBHangtime(int stype, int id, double time);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRNOBAdvtime(int stype, int id, double time);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRNOBBacktau(int stype, int id, double tau);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRCVRNOBThreshold(int stype, int id, double thresh);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetAndResetAmpProtect(int txid);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAmpProtectRun(int txid, int run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetADCSupply(int txid, int v);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getLEDs();

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetCMVersion();

	[DllImport("cmASIO.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetCMasioVersion();

	public unsafe static void CMCreateCMaster()
	{
		int[] array = new int[1] { 2 };
		int[] obj = new int[8] { 240, 240, 240, 240, 240, 720, 240, 240 };
		fixed (int* cmSPC = array)
		{
			fixed (int* cmMAXInbound = obj)
			{
				SetRadioStructure(8, cmRCVR, 1, cmSubRCVR, 1, cmSPC, cmMAXInbound, 1536000, 48000, 384000);
			}
		}
		SendCallbacks();
		int[] array2 = new int[8] { 192000, 192000, 192000, 192000, 192000, 48000, 192000, 192000 };
		int aud_outrate = 48000;
		int[] array3 = new int[5] { 48000, 48000, 48000, 48000, 48000 };
		int[] obj2 = new int[1] { 192000 };
		fixed (int* xcm_inrates = array2)
		{
			fixed (int* rcvr_ch_outrates = array3)
			{
				fixed (int* xmtr_ch_outrates = obj2)
				{
					SetCMDefaultRates(xcm_inrates, aud_outrate, rcvr_ch_outrates, xmtr_ch_outrates);
				}
			}
		}
		CreateRadio();
		InitWaterfallIQ(524288);
		int num = inid(1, 0);
		int channel = chid(num, 0);
		SetXcmInrate(num, 48000);
		if (NetworkIO.CurrentRadioProtocol == RadioProtocol.USB)
		{
			WDSP.SetTXACFIRRun(channel, run: false);
		}
		else
		{
			WDSP.SetTXACFIRRun(channel, run: true);
		}
		SetPSRxIdx(0, 0);
		SetPSTxIdx(0, 1);
		puresignal.SetPSFeedbackRate(channel, ps_rate);
		puresignal.SetPSHWPeak(channel, 0.2899);
		WDSP.TXASetSipMode(channel, 1);
		WDSP.TXASetSipDisplay(channel, num);
		NetworkIO.CreateRNet();
	}

	public unsafe static void CMLoadRouterAll(HPSDRModel model)
	{
		switch (NetworkIO.CurrentRadioProtocol)
		{
		case RadioProtocol.USB:
			if (ps_loopback)
			{
				switch (model)
				{
				case HPSDRModel.ANAN10E:
				case HPSDRModel.ANAN100B:
				{
					int[] array13 = new int[16]
					{
						2, 2, 2, 2, 2, 2, 2, 2, 0, 0,
						0, 0, 0, 2, 0, 2
					};
					int[] array14 = new int[16]
					{
						2, 2, 2, 2, 2, 1, 2, 1, 0, 0,
						0, 0, 0, 2, 0, 2
					};
					fixed (int* nstreams7 = &(new int[1] { 2 })[0])
					{
						fixed (int* function7 = &array13[0])
						{
							fixed (int* callid7 = &array14[0])
							{
								LoadRouterAll(null, 0, 1, 2, 8, nstreams7, function7, callid7);
							}
						}
					}
					break;
				}
				case HPSDRModel.HERMES:
				case HPSDRModel.ANAN10:
				case HPSDRModel.ANAN100:
				case HPSDRModel.ANAN_G2E:
				{
					int[] array11 = new int[48]
					{
						1, 1, 1, 1, 1, 0, 1, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 2, 0, 2, 0, 0, 0, 0, 0, 2,
						0, 2, 1, 1, 1, 1, 1, 0, 1, 0,
						0, 0, 0, 0, 0, 0, 0, 0
					};
					int[] array12 = new int[48]
					{
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 1, 0, 1, 0, 0, 0, 0, 0, 2,
						0, 2, 1, 1, 1, 1, 1, 0, 1, 0,
						0, 0, 0, 0, 0, 0, 0, 0
					};
					fixed (int* nstreams6 = &(new int[3] { 1, 2, 1 })[0])
					{
						fixed (int* function6 = &array11[0])
						{
							fixed (int* callid6 = &array12[0])
							{
								LoadRouterAll(null, 0, 3, 2, 8, nstreams6, function6, callid6);
							}
						}
					}
					break;
				}
				case HPSDRModel.ANAN100D:
				case HPSDRModel.ANAN200D:
				case HPSDRModel.ORIONMKII:
				case HPSDRModel.ANAN7000D:
				case HPSDRModel.ANAN8000D:
				case HPSDRModel.ANVELINAPRO3:
				case HPSDRModel.REDPITAYA:
				{
					int[] array9 = new int[48]
					{
						2, 2, 2, 2, 2, 0, 2, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 2, 0, 2, 0, 0, 0, 0, 0, 2,
						0, 2, 1, 1, 1, 1, 1, 0, 1, 0,
						0, 0, 0, 0, 0, 0, 0, 0
					};
					int[] array10 = new int[48]
					{
						3, 3, 0, 0, 3, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 1, 0, 1, 0, 0, 0, 0, 0, 2,
						0, 2, 1, 1, 1, 1, 1, 0, 1, 0,
						0, 0, 0, 0, 0, 0, 0, 0
					};
					fixed (int* nstreams5 = &(new int[3] { 2, 2, 1 })[0])
					{
						fixed (int* function5 = &array9[0])
						{
							fixed (int* callid5 = &array10[0])
							{
								LoadRouterAll(null, 0, 3, 2, 8, nstreams5, function5, callid5);
							}
						}
					}
					break;
				}
				case HPSDRModel.ANAN_G2:
				case HPSDRModel.ANAN_G2_1K:
				case HPSDRModel.HERMESLITE:
					break;
				}
				break;
			}
			switch (model)
			{
			case HPSDRModel.ANAN10E:
			case HPSDRModel.ANAN100B:
			{
				int[] array19 = new int[16]
				{
					2, 2, 2, 2, 2, 2, 2, 2, 0, 0,
					0, 0, 0, 2, 0, 2
				};
				int[] array20 = new int[16]
				{
					2, 2, 2, 2, 2, 1, 2, 1, 0, 0,
					0, 0, 0, 2, 0, 2
				};
				fixed (int* nstreams10 = &(new int[1] { 2 })[0])
				{
					fixed (int* function10 = &array19[0])
					{
						fixed (int* callid10 = &array20[0])
						{
							LoadRouterAll(null, 0, 1, 2, 8, nstreams10, function10, callid10);
						}
					}
				}
				break;
			}
			case HPSDRModel.HERMES:
			case HPSDRModel.ANAN10:
			case HPSDRModel.ANAN100:
			case HPSDRModel.ANAN_G2E:
			{
				int[] array17 = new int[24]
				{
					1, 1, 1, 1, 1, 1, 1, 1, 0, 0,
					0, 0, 0, 2, 0, 2, 1, 1, 1, 1,
					1, 1, 1, 1
				};
				int[] array18 = new int[24]
				{
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 1, 0, 1, 1, 1, 1, 1,
					1, 1, 1, 1
				};
				fixed (int* nstreams9 = &(new int[3] { 1, 2, 1 })[0])
				{
					fixed (int* function9 = &array17[0])
					{
						fixed (int* callid9 = &array18[0])
						{
							LoadRouterAll(null, 0, 3, 1, 8, nstreams9, function9, callid9);
						}
					}
				}
				break;
			}
			case HPSDRModel.ANAN100D:
			case HPSDRModel.ANAN200D:
			case HPSDRModel.ORIONMKII:
			case HPSDRModel.ANAN7000D:
			case HPSDRModel.ANAN8000D:
			case HPSDRModel.ANVELINAPRO3:
			case HPSDRModel.REDPITAYA:
			{
				int[] array15 = new int[24]
				{
					2, 2, 2, 2, 2, 2, 2, 2, 0, 0,
					0, 0, 0, 2, 0, 2, 1, 1, 1, 1,
					1, 1, 1, 1
				};
				int[] array16 = new int[24]
				{
					3, 3, 0, 0, 3, 3, 0, 0, 0, 0,
					0, 0, 0, 1, 0, 1, 1, 1, 1, 1,
					1, 1, 1, 1
				};
				fixed (int* nstreams8 = &(new int[3] { 2, 2, 1 })[0])
				{
					fixed (int* function8 = &array15[0])
					{
						fixed (int* callid8 = &array16[0])
						{
							LoadRouterAll(null, 0, 3, 1, 8, nstreams8, function8, callid8);
						}
					}
				}
				break;
			}
			case HPSDRModel.ANAN_G2:
			case HPSDRModel.ANAN_G2_1K:
			case HPSDRModel.HERMESLITE:
				break;
			}
			break;
		case RadioProtocol.ETH:
			if (ps_loopback)
			{
				switch (model)
				{
				case HPSDRModel.ANAN100D:
				case HPSDRModel.ANAN200D:
				case HPSDRModel.ORIONMKII:
				case HPSDRModel.ANAN7000D:
				case HPSDRModel.ANAN8000D:
				case HPSDRModel.ANAN_G2:
				case HPSDRModel.ANAN_G2_1K:
				case HPSDRModel.ANVELINAPRO3:
				case HPSDRModel.REDPITAYA:
				{
					int[] array3 = new int[112]
					{
						0, 0, 2, 2, 0, 2, 2, 2, 0, 0,
						0, 0, 0, 2, 0, 2, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 1, 1, 0, 0, 1, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
						1, 1, 1, 0, 1, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
						1, 1, 0, 0, 0, 0, 0, 0, 0, 0,
						1, 1, 1, 1, 1, 1, 1, 1, 0, 0,
						0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
						1, 1, 1, 1, 0, 0, 0, 0, 0, 0,
						0, 0
					};
					int[] array4 = new int[112]
					{
						0, 0, 0, 0, 0, 1, 0, 1, 0, 0,
						0, 0, 0, 2, 0, 2, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 2, 2, 2, 2, 2, 2,
						2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
						3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
						3, 3, 3, 3, 3, 3, 4, 4, 4, 4,
						4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
						4, 4
					};
					fixed (int* nstreams2 = &(new int[7] { 2, 1, 1, 1, 1, 1, 1 })[0])
					{
						fixed (int* function2 = &array3[0])
						{
							fixed (int* callid2 = &array4[0])
							{
								LoadRouterAll(null, 0, 7, 2, 8, nstreams2, function2, callid2);
							}
						}
					}
					break;
				}
				case HPSDRModel.HERMES:
				case HPSDRModel.ANAN10:
				case HPSDRModel.ANAN10E:
				case HPSDRModel.ANAN100:
				case HPSDRModel.ANAN100B:
				case HPSDRModel.ANAN_G2E:
				{
					int[] array = new int[32]
					{
						1, 1, 2, 2, 1, 2, 2, 2, 0, 0,
						0, 0, 0, 2, 0, 2, 1, 1, 0, 0,
						1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0
					};
					int[] array2 = new int[32]
					{
						0, 0, 0, 0, 0, 1, 0, 1, 0, 0,
						0, 0, 0, 2, 0, 2, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1
					};
					fixed (int* nstreams = &(new int[2] { 2, 1 })[0])
					{
						fixed (int* function = &array[0])
						{
							fixed (int* callid = &array2[0])
							{
								LoadRouterAll(null, 0, 2, 2, 8, nstreams, function, callid);
							}
						}
					}
					break;
				}
				case HPSDRModel.HERMESLITE:
					break;
				}
				break;
			}
			switch (model)
			{
			case HPSDRModel.ANAN100D:
			case HPSDRModel.ANAN200D:
			case HPSDRModel.ORIONMKII:
			case HPSDRModel.ANAN7000D:
			case HPSDRModel.ANAN8000D:
			case HPSDRModel.ANAN_G2:
			case HPSDRModel.ANAN_G2_1K:
			case HPSDRModel.ANVELINAPRO3:
			case HPSDRModel.REDPITAYA:
			{
				int[] array7 = new int[56]
				{
					0, 0, 2, 2, 0, 2, 2, 2, 0, 0,
					0, 0, 0, 0, 0, 0, 1, 1, 0, 0,
					1, 1, 0, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1
				};
				int[] array8 = new int[56]
				{
					0, 0, 0, 0, 0, 1, 0, 1, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
					1, 1, 2, 2, 2, 2, 2, 2, 2, 2,
					3, 3, 3, 3, 3, 3, 3, 3, 4, 4,
					4, 4, 4, 4, 4, 4
				};
				fixed (int* nstreams4 = &(new int[7] { 2, 1, 1, 1, 1, 1, 1 })[0])
				{
					fixed (int* function4 = &array7[0])
					{
						fixed (int* callid4 = &array8[0])
						{
							LoadRouterAll(null, 0, 7, 1, 8, nstreams4, function4, callid4);
						}
					}
				}
				break;
			}
			case HPSDRModel.HERMES:
			case HPSDRModel.ANAN10:
			case HPSDRModel.ANAN10E:
			case HPSDRModel.ANAN100:
			case HPSDRModel.ANAN100B:
			case HPSDRModel.ANAN_G2E:
			{
				int[] array5 = new int[32]
				{
					1, 1, 2, 2, 1, 2, 2, 2, 0, 0,
					0, 0, 0, 2, 0, 2, 1, 1, 0, 0,
					1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0
				};
				int[] array6 = new int[32]
				{
					0, 0, 0, 0, 0, 1, 0, 1, 0, 0,
					0, 0, 0, 3, 0, 3, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1
				};
				fixed (int* nstreams3 = &(new int[2] { 2, 1 })[0])
				{
					fixed (int* function3 = &array5[0])
					{
						fixed (int* callid3 = &array6[0])
						{
							LoadRouterAll(null, 0, 2, 2, 8, nstreams3, function3, callid3);
						}
					}
				}
				break;
			}
			case HPSDRModel.HPSDR:
			case HPSDRModel.HERMESLITE:
				break;
			}
			break;
		}
	}

	public static void CMSetAntiVoxSourceWhat()
	{
		bool vACEnabled = Audio.console.VACEnabled;
		bool vAC2Enabled = Audio.console.VAC2Enabled;
		bool antiVOXSourceVAC = Audio.AntiVOXSourceVAC;
		int stream = WDSP.id(0u, 0u);
		int stream2 = WDSP.id(0u, 1u);
		int stream3 = WDSP.id(2u, 0u);
		if (antiVOXSourceVAC)
		{
			if (vACEnabled)
			{
				SetAntiVOXSourceWhat(0, stream, 1);
				SetAntiVOXSourceWhat(0, stream2, 1);
			}
			else
			{
				SetAntiVOXSourceWhat(0, stream, 0);
				SetAntiVOXSourceWhat(0, stream2, 0);
			}
			if (vAC2Enabled)
			{
				SetAntiVOXSourceWhat(0, stream3, 1);
			}
			else
			{
				SetAntiVOXSourceWhat(0, stream3, 0);
			}
		}
		else
		{
			SetAntiVOXSourceWhat(0, stream, 1);
			SetAntiVOXSourceWhat(0, stream2, 1);
			SetAntiVOXSourceWhat(0, stream3, 1);
		}
	}

	public unsafe static void CMSetAudioVolume(double volume)
	{
		SetAAudioMixVolume(null, 0, volume);
	}

	public static void CMSetFRXNBRun(int id)
	{
		bool flag = false;
		switch (id)
		{
		case 0:
			flag = Audio.console.specRX.GetSpecRX(0).NBOn;
			SetRCVRANBRun(0, 0, flag);
			break;
		case 1:
			flag = Audio.console.specRX.GetSpecRX(1).NBOn && Audio.console.RX2Enabled;
			SetRCVRANBRun(0, 1, flag);
			break;
		}
	}

	public static void CMSetFRXNB2Run(int id)
	{
		bool flag = false;
		switch (id)
		{
		case 0:
			flag = Audio.console.specRX.GetSpecRX(0).NB2On;
			SetRCVRNOBRun(0, 0, flag);
			break;
		case 1:
			flag = Audio.console.specRX.GetSpecRX(1).NB2On && Audio.console.RX2Enabled;
			SetRCVRNOBRun(0, 1, flag);
			break;
		}
	}

	public static void CMSetSRXWavePlayRun(int id)
	{
		bool flag = false;
		switch (id)
		{
		case 0:
			flag = Audio.WavePlayback && WaveThing.wave_file_reader[0] != null;
			break;
		case 1:
			flag = Audio.WavePlayback && WaveThing.wave_file_reader[1] != null && Audio.console.RX2Enabled;
			break;
		}
		WaveThing.SetWavePlayerRun(id, flag ? 1 : 0);
		WaveThing.wplayer[id].Run = flag;
	}

	public static void CMSetSRXWaveRecordRun(int id)
	{
		bool flag = false;
		switch (id)
		{
		case 0:
			flag = Audio.WaveRecord && WaveThing.wave_file_writer[0] != null;
			if (flag)
			{
				WaveThing.wave_file_writer[0].RecordGain = (float)Audio.console.radio.GetDSPRX(0, 0).RXOutputGain;
			}
			break;
		case 1:
			flag = Audio.WaveRecord && WaveThing.wave_file_writer[1] != null && Audio.console.RX2Enabled;
			if (flag)
			{
				WaveThing.wave_file_writer[1].RecordGain = (float)Audio.console.radio.GetDSPRX(1, 0).RXOutputGain;
			}
			break;
		}
		WaveThing.SetWaveRecorderRun(id, flag ? 1 : 0);
		WaveThing.wrecorder[id].Run = flag;
	}

	public static void CMSetEERRun(int id)
	{
		if (Audio.console.radio.GetDSPTX(0).TXEERModeRun && mox)
		{
			SetEERRun(id, run: true);
			NetworkIO.EnableEClassModulation(1);
		}
		else
		{
			SetEERRun(id, run: false);
			NetworkIO.EnableEClassModulation(0);
		}
	}

	public static void CMSetTXAVoxRun(int id)
	{
		DSPMode tXDSPMode = Audio.TXDSPMode;
		bool run = Audio.VOXEnabled && (tXDSPMode == DSPMode.LSB || tXDSPMode == DSPMode.USB || tXDSPMode == DSPMode.DSB || tXDSPMode == DSPMode.AM || tXDSPMode == DSPMode.SAM || tXDSPMode == DSPMode.FM || tXDSPMode == DSPMode.DIGL || tXDSPMode == DSPMode.DIGU);
		SetDEXPRunVox(id, run);
	}

	public static void CMSetTXAVoxThresh(int id, double thresh)
	{
		if (Audio.console.MicBoost)
		{
			thresh *= (double)Audio.VOXGain;
		}
		SetDEXPAttackThreshold(id, thresh);
	}

	public static void CMSetTXAPanelGain1(int channel)
	{
		double micGain = 1.0;
		DSPMode tXDSPMode = Audio.TXDSPMode;
		if ((!Audio.VACEnabled && (tXDSPMode == DSPMode.LSB || tXDSPMode == DSPMode.USB || tXDSPMode == DSPMode.DSB || tXDSPMode == DSPMode.AM || tXDSPMode == DSPMode.SAM || tXDSPMode == DSPMode.FM || tXDSPMode == DSPMode.DIGL || tXDSPMode == DSPMode.DIGU)) || (Audio.VACEnabled && Audio.VACBypass && (tXDSPMode == DSPMode.DIGL || tXDSPMode == DSPMode.DIGU || tXDSPMode == DSPMode.LSB || tXDSPMode == DSPMode.USB || tXDSPMode == DSPMode.DSB || tXDSPMode == DSPMode.AM || tXDSPMode == DSPMode.SAM || tXDSPMode == DSPMode.FM)))
		{
			if (!Audio.WavePlayback)
			{
				micGain = ((Audio.VACEnabled || (tXDSPMode != DSPMode.DIGL && tXDSPMode != DSPMode.DIGU)) ? Audio.MicPreamp : Audio.VACPreamp);
			}
			else
			{
				double num = 20.0 * Math.Log10(Audio.WavePreamp);
				num += 20.0 * Math.Log10(Audio.WavePreampAdjust);
				if (num < -70.0)
				{
					num = -70.0;
				}
				if (num > 70.0)
				{
					num = 70.0;
				}
				micGain = Math.Pow(10.0, num / 20.0);
			}
		}
		Audio.console.radio.GetDSPTX(0).MicGain = micGain;
	}

	public static void CMSetScopeRun(int id, bool run)
	{
		Scope.SetScopeRun(id, run ? 1 : 0);
		Scope.dscope[id].Run = run;
	}

	public static void CMSetTXOutputLevelRun()
	{
		bool run = false;
		SetTXFixedGainRun(0, run);
	}

	public static void CMSetTXOutputLevel()
	{
		double num = Audio.RadioVolume * Audio.HighSWRScale;
		SetTXFixedGain(0, num, num);
	}

	public static void SendCallbacks()
	{
		SendCBPushVox(0, PushVoxDel);
		SendpOutboundTCIRxIQ(TCIRxIQOutDel);
		SendpOutboundTCIRxAudio(TCIRxAudioOutDel);
		SendpInboundTCITxAudio(TCITxAudioInDel);
		ensureTCIStreamThreads();
		WaveThing.initWaves();
		Scope.initScope();
	}

	private static void ensureTCIStreamThreads()
	{
		m_runTCIStreamThreads = true;
		if (m_tciRxThread == null)
		{
			m_tciRxThread = new Thread(TCIRxThreadProc);
			m_tciRxThread.IsBackground = true;
			m_tciRxThread.Name = "TCI RX Stream";
			m_tciRxThread.Priority = ThreadPriority.AboveNormal;
			m_tciRxThread.Start();
		}
		if (m_tciTxThread == null)
		{
			m_tciTxThread = new Thread(TCITxThreadProc);
			m_tciTxThread.IsBackground = true;
			m_tciTxThread.Name = "TCI TX Stream";
			m_tciTxThread.Priority = ThreadPriority.AboveNormal;
			m_tciTxThread.Start();
		}
	}

	public static void StopTCIStreamThreads()
	{
		m_runTCIStreamThreads = false;
		m_tciRxStreamEvent.Set();
		m_tciTxStreamEvent.Set();
		m_tciRxThread?.Join(500);
		m_tciTxThread?.Join(500);
		m_tciRxThread = null;
		m_tciTxThread = null;
	}

	private static void TCIRxThreadProc()
	{
		while (m_runTCIStreamThreads)
		{
			try
			{
				if (TCIServer != null)
				{
					serviceTCIRxStreams();
				}
			}
			catch
			{
			}
			if (m_runTCIStreamThreads)
			{
				m_tciRxStreamEvent.WaitOne(1000);
				continue;
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void serviceTCIRxStreams()
	{
		TCPIPtciServer tCIServer = TCIServer;
		if (tCIServer == null)
		{
			return;
		}
		TCIIQBlock block;
		while (tryDequeueTCIIQ(out block))
		{
			try
			{
				tCIServer.PublishIQSamples(block.Receiver, block.SampleRate, block.Samples, block.ComplexSamples);
			}
			finally
			{
				returnTCIIQBlock(block);
			}
		}
		TCIAudioBlock block2;
		while (tryDequeueTCIAudio(out block2))
		{
			try
			{
				tCIServer.PublishRxAudioSamples(block2.Receiver, block2.SampleRate, block2.Left, block2.Right, block2.SamplesPerChannel);
			}
			finally
			{
				returnTCIAudioBlock(block2);
			}
		}
	}

	private static void TCITxThreadProc()
	{
		while (m_runTCIStreamThreads)
		{
			try
			{
				serviceTCITxProtocol();
			}
			catch
			{
			}
			if (m_runTCIStreamThreads)
			{
				m_tciTxStreamEvent.WaitOne(1000);
				continue;
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void serviceTCITxProtocol()
	{
		TCPIPtciServer tCIServer = TCIServer;
		if (tCIServer == null || !tCIServer.UsesActiveTCITxAudio() || !Audio.MOX)
		{
			resetTCITxState();
			return;
		}
		int num = ((Audio.RX2Enabled && Audio.VFOBTX) ? 1 : 0);
		if (m_cachedTxInputRate <= 0)
		{
			m_cachedTxInputRate = GetInputRate(1, 0);
		}
		int num2 = ((m_cachedTxInputRate > 0) ? m_cachedTxInputRate : 48000);
		bool flag = false;
		lock (m_objTCITxStateLock)
		{
			if (m_tciTxInputRate != 0 && m_tciTxInputRate != num2)
			{
				flag = true;
			}
			else if (m_tciTxChronoReceiver != num)
			{
				flag = true;
			}
		}
		if (flag)
		{
			resetTCITxState();
		}
		lock (m_objTCITxStateLock)
		{
			m_tciTxChronoReceiver = num;
			m_tciTxInputRate = num2;
		}
		if (!tCIServer.TryGetTxAudioRequestSettings(out var sampleRate, out var samples, out var bufferingMs))
		{
			resetTCITxState();
			return;
		}
		if (sampleRate <= 0)
		{
			sampleRate = 48000;
		}
		if (samples <= 0)
		{
			samples = 480;
		}
		if (bufferingMs < 50)
		{
			bufferingMs = 50;
		}
		TCIQueuedTxAudio queuedAudio;
		while (tCIServer.TryDequeueTxAudio(out queuedAudio))
		{
			lock (m_objTCITxStateLock)
			{
				if (m_tciTxChronoOutstanding > 0)
				{
					m_tciTxChronoOutstanding--;
				}
			}
			if (queuedAudio != null && queuedAudio.Receiver == num)
			{
				queueTCITxAudio(queuedAudio, num2, tCIServer.TXStereoInputMode);
			}
		}
		int num3 = GetBuffSize(num2);
		if (num3 <= 0)
		{
			num3 = 720;
		}
		int num4 = Math.Max(num3, (int)Math.Ceiling((double)samples * (double)num2 / (double)Math.Max(1, sampleRate)));
		int num5 = Math.Max(num3 * 4, (int)Math.Ceiling((double)((bufferingMs + 50) * num2) / 1000.0));
		long num6 = DateTime.UtcNow.Ticks / 10000;
		int tciTxQueuedSamples;
		int tciTxChronoOutstanding;
		lock (m_objTCITxStateLock)
		{
			if (m_tciTxChronoOutstanding > 0 && num6 - m_tciTxLastChronoTick > Math.Max(250, bufferingMs * 4))
			{
				m_tciTxChronoOutstanding = 0;
			}
			tciTxQueuedSamples = m_tciTxQueuedSamples;
			tciTxChronoOutstanding = m_tciTxChronoOutstanding;
		}
		int num7 = tciTxQueuedSamples + tciTxChronoOutstanding * num4;
		int num8 = ((num7 < num5) ? ((int)Math.Ceiling((double)(num5 - num7) / (double)num4)) : 0);
		while (num8 > 0)
		{
			bool flag2;
			lock (m_objTCITxStateLock)
			{
				flag2 = m_tciTxChronoOutstanding < 64;
				if (flag2)
				{
					m_tciTxChronoOutstanding++;
					m_tciTxLastChronoTick = num6;
				}
			}
			if (flag2)
			{
				tCIServer.SendTxChrono(num);
				num8--;
				continue;
			}
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void resetTCITxState()
	{
		lock (m_objTCITxResamplerLock)
		{
			m_tciTxResamplerInputRate = 0;
			m_tciTxResamplerOutputRate = 0;
			if (m_tciTxResampler != null)
			{
				WDSP.destroy_resampleFV(m_tciTxResampler);
				m_tciTxResampler = null;
			}
		}
		lock (m_objTCITxStateLock)
		{
			m_tciTxSampleQueue.Clear();
			m_tciTxSampleQueueOffset = 0;
			m_tciTxQueuedSamples = 0;
			m_tciTxChronoOutstanding = 0;
			m_tciTxChronoReceiver = 0;
			m_tciTxLastChronoTick = 0L;
			m_tciTxInputRate = 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void queueTCITxAudio(TCIQueuedTxAudio queuedAudio, int targetRate, TCITxStereoInputMode stereoInputMode)
	{
		if (queuedAudio == null || queuedAudio.Samples == null || queuedAudio.ComplexSamples <= 0)
		{
			return;
		}
		int num = Math.Min(queuedAudio.ComplexSamples, queuedAudio.Samples.Length / 2);
		if (num <= 0)
		{
			return;
		}
		float[] array = new float[num];
		if (queuedAudio.Channels <= 1)
		{
			for (int i = 0; i < num; i++)
			{
				array[i] = (float)queuedAudio.Samples[2 * i];
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				double num2 = queuedAudio.Samples[2 * j];
				double num3 = queuedAudio.Samples[2 * j + 1];
				switch (stereoInputMode)
				{
				case TCITxStereoInputMode.Left:
					array[j] = (float)num2;
					break;
				case TCITxStereoInputMode.Right:
					array[j] = (float)num3;
					break;
				default:
					array[j] = (float)((num2 + num3) * 0.5);
					break;
				}
			}
		}
		float[] array2 = resampleTCITxSamples(array, queuedAudio.SampleRate, targetRate);
		if (array2 == null || array2.Length == 0)
		{
			return;
		}
		lock (m_objTCITxStateLock)
		{
			m_tciTxSampleQueue.Enqueue(array2);
			m_tciTxQueuedSamples += array2.Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static float[] resampleTCITxSamples(float[] input, int inputRate, int targetRate)
	{
		if (input == null || input.Length == 0)
		{
			return Array.Empty<float>();
		}
		if (inputRate <= 0 || targetRate <= 0 || inputRate == targetRate)
		{
			return input;
		}
		lock (m_objTCITxResamplerLock)
		{
			if (m_tciTxResampler == null || m_tciTxResamplerInputRate != inputRate || m_tciTxResamplerOutputRate != targetRate)
			{
				if (m_tciTxResampler != null)
				{
					WDSP.destroy_resampleFV(m_tciTxResampler);
				}
				void* ptr = WDSP.create_resampleFV(inputRate, targetRate);
				if (ptr == null)
				{
					return Array.Empty<float>();
				}
				m_tciTxResampler = ptr;
				m_tciTxResamplerInputRate = inputRate;
				m_tciTxResamplerOutputRate = targetRate;
			}
			float[] array = new float[Math.Max(input.Length + 64, (int)Math.Ceiling((double)input.Length * (double)targetRate / (double)inputRate) + 64)];
			int num = 0;
			fixed (float* input2 = input)
			{
				fixed (float* output = array)
				{
					WDSP.xresampleFV(input2, output, input.Length, &num, m_tciTxResampler);
				}
			}
			if (num <= 0)
			{
				return Array.Empty<float>();
			}
			if (num == array.Length)
			{
				return array;
			}
			float[] array2 = new float[num];
			Array.Copy(array, array2, num);
			return array2;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void destroyTCIIQResampler(int receiver)
	{
		if (receiver >= 0 && receiver < cmRCVR)
		{
			if (m_tciIQResamplerI[receiver] != null)
			{
				WDSP.destroy_resampleFV(m_tciIQResamplerI[receiver]);
				m_tciIQResamplerI[receiver] = null;
			}
			if (m_tciIQResamplerQ[receiver] != null)
			{
				WDSP.destroy_resampleFV(m_tciIQResamplerQ[receiver]);
				m_tciIQResamplerQ[receiver] = null;
			}
			m_tciIQResamplerInputRates[receiver] = 0;
			m_tciIQResamplerOutputRates[receiver] = 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static float[] resampleTCIIQSamples(int receiver, float[] input, int inputRate, int targetRate)
	{
		if (input == null || input.Length == 0)
		{
			return Array.Empty<float>();
		}
		if (receiver < 0 || receiver >= cmRCVR || inputRate <= 0 || targetRate <= 0)
		{
			return input;
		}
		if (inputRate <= targetRate)
		{
			lock (m_objTCIIQResamplerLock)
			{
				destroyTCIIQResampler(receiver);
				return input;
			}
		}
		int num = input.Length / 2;
		if (num <= 0)
		{
			return Array.Empty<float>();
		}
		float[] array = new float[num];
		float[] array2 = new float[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = input[2 * i];
			array2[i] = input[2 * i + 1];
		}
		lock (m_objTCIIQResamplerLock)
		{
			if (m_tciIQResamplerI[receiver] == null || m_tciIQResamplerQ[receiver] == null || m_tciIQResamplerInputRates[receiver] != inputRate || m_tciIQResamplerOutputRates[receiver] != targetRate)
			{
				destroyTCIIQResampler(receiver);
				void* ptr = WDSP.create_resampleFV(inputRate, targetRate);
				void* ptr2 = WDSP.create_resampleFV(inputRate, targetRate);
				if (ptr == null || ptr2 == null)
				{
					if (ptr != null)
					{
						WDSP.destroy_resampleFV(ptr);
					}
					if (ptr2 != null)
					{
						WDSP.destroy_resampleFV(ptr2);
					}
					return Array.Empty<float>();
				}
				m_tciIQResamplerI[receiver] = ptr;
				m_tciIQResamplerQ[receiver] = ptr2;
				m_tciIQResamplerInputRates[receiver] = inputRate;
				m_tciIQResamplerOutputRates[receiver] = targetRate;
			}
			int num2 = Math.Max(num + 64, (int)Math.Ceiling((double)num * (double)targetRate / (double)inputRate) + 64);
			float[] array3 = new float[num2];
			float[] array4 = new float[num2];
			int val = 0;
			int val2 = 0;
			fixed (float* input2 = array)
			{
				fixed (float* input3 = array2)
				{
					fixed (float* output = array3)
					{
						fixed (float* output2 = array4)
						{
							WDSP.xresampleFV(input2, output, num, &val, m_tciIQResamplerI[receiver]);
							WDSP.xresampleFV(input3, output2, num, &val2, m_tciIQResamplerQ[receiver]);
						}
					}
				}
			}
			int num3 = Math.Min(val, val2);
			if (num3 <= 0)
			{
				return Array.Empty<float>();
			}
			float[] array5 = new float[num3 * 2];
			for (int j = 0; j < num3; j++)
			{
				array5[2 * j] = array3[j];
				array5[2 * j + 1] = array4[j];
			}
			return array5;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float[] rentTCIFloatBuffer(int length)
	{
		if (length <= 0)
		{
			return Array.Empty<float>();
		}
		lock (m_objTCIStreamPoolLock)
		{
			if (m_tciFloatBufferPool.TryGetValue(length, out var value) && value.Count > 0)
			{
				return value.Pop();
			}
		}
		return new float[length];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void returnTCIFloatBuffer(float[] buffer)
	{
		if (buffer == null || buffer.Length == 0)
		{
			return;
		}
		lock (m_objTCIStreamPoolLock)
		{
			if (!m_tciFloatBufferPool.TryGetValue(buffer.Length, out var value))
			{
				value = new Stack<float[]>();
				m_tciFloatBufferPool[buffer.Length] = value;
			}
			if (value.Count < 32)
			{
				value.Push(buffer);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static TCIIQBlock rentTCIIQBlock()
	{
		lock (m_objTCIStreamPoolLock)
		{
			if (m_tciIQBlockPool.Count > 0)
			{
				return m_tciIQBlockPool.Pop();
			}
		}
		return new TCIIQBlock();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void returnTCIIQBlock(TCIIQBlock block)
	{
		if (block == null)
		{
			return;
		}
		returnTCIFloatBuffer(block.Samples);
		block.Receiver = 0;
		block.SampleRate = 0;
		block.ComplexSamples = 0;
		block.Samples = null;
		lock (m_objTCIStreamPoolLock)
		{
			if (m_tciIQBlockPool.Count < 64)
			{
				m_tciIQBlockPool.Push(block);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static TCIAudioBlock rentTCIAudioBlock()
	{
		lock (m_objTCIStreamPoolLock)
		{
			if (m_tciAudioBlockPool.Count > 0)
			{
				return m_tciAudioBlockPool.Pop();
			}
		}
		return new TCIAudioBlock();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void returnTCIAudioBlock(TCIAudioBlock block)
	{
		if (block == null)
		{
			return;
		}
		returnTCIFloatBuffer(block.Left);
		returnTCIFloatBuffer(block.Right);
		block.Receiver = 0;
		block.SampleRate = 0;
		block.SamplesPerChannel = 0;
		block.Left = null;
		block.Right = null;
		lock (m_objTCIStreamPoolLock)
		{
			if (m_tciAudioBlockPool.Count < 64)
			{
				m_tciAudioBlockPool.Push(block);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void enqueueTCIIQ(TCIIQBlock block)
	{
		TCIIQBlock tCIIQBlock = null;
		lock (m_objTCIStreamQueueLock)
		{
			if (m_tciIQQueue.Count >= 32)
			{
				tCIIQBlock = m_tciIQQueue.Dequeue();
			}
			m_tciIQQueue.Enqueue(block);
		}
		if (tCIIQBlock != null)
		{
			returnTCIIQBlock(tCIIQBlock);
		}
		m_tciRxStreamEvent.Set();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void enqueueTCIAudio(TCIAudioBlock block)
	{
		TCIAudioBlock tCIAudioBlock = null;
		lock (m_objTCIStreamQueueLock)
		{
			if (m_tciAudioQueue.Count >= 128)
			{
				tCIAudioBlock = m_tciAudioQueue.Dequeue();
			}
			m_tciAudioQueue.Enqueue(block);
		}
		if (tCIAudioBlock != null)
		{
			returnTCIAudioBlock(tCIAudioBlock);
		}
		m_tciRxStreamEvent.Set();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool tryDequeueTCIIQ(out TCIIQBlock block)
	{
		lock (m_objTCIStreamQueueLock)
		{
			if (m_tciIQQueue.Count > 0)
			{
				block = m_tciIQQueue.Dequeue();
				return true;
			}
		}
		block = null;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool tryDequeueTCIAudio(out TCIAudioBlock block)
	{
		lock (m_objTCIStreamQueueLock)
		{
			if (m_tciAudioQueue.Count > 0)
			{
				block = m_tciAudioQueue.Dequeue();
				return true;
			}
		}
		block = null;
		return false;
	}

	private unsafe static void OnTCIRxIQOutSamples(int id, int nsamples, double* data)
	{
		TCPIPtciServer tCIServer = TCIServer;
		if (tCIServer == null || data == null || nsamples <= 0)
		{
			return;
		}
		int inputRate = GetInputRate(0, id);
		int num = ((inputRate > 384000) ? 384000 : inputRate);
		bool iQSwap = tCIServer.IQSwap;
		float[] array = rentTCIFloatBuffer(nsamples * 2);
		for (int i = 0; i < nsamples; i++)
		{
			array[2 * i] = (float)data[2 * i];
			array[2 * i + 1] = (iQSwap ? ((float)(0.0 - data[2 * i + 1])) : ((float)data[2 * i + 1]));
		}
		if (inputRate > 384000)
		{
			float[] array2 = resampleTCIIQSamples(id, array, inputRate, num);
			if (array2 != array)
			{
				returnTCIFloatBuffer(array);
			}
			array = array2;
		}
		int num2 = ((array != null) ? (array.Length / 2) : 0);
		if (num2 <= 0)
		{
			returnTCIFloatBuffer(array);
			return;
		}
		TCIIQBlock tCIIQBlock = rentTCIIQBlock();
		tCIIQBlock.Receiver = id;
		tCIIQBlock.SampleRate = num;
		tCIIQBlock.ComplexSamples = num2;
		tCIIQBlock.Samples = array;
		enqueueTCIIQ(tCIIQBlock);
	}

	private unsafe static void OnTCIRxAudioOutSamples(int id, int nsamples, double* data)
	{
		if (TCIServer != null && data != null && nsamples > 0)
		{
			float[] array = rentTCIFloatBuffer(nsamples);
			float[] array2 = rentTCIFloatBuffer(nsamples);
			for (int i = 0; i < nsamples; i++)
			{
				array[i] = (float)data[2 * i];
				array2[i] = (float)data[2 * i + 1];
			}
			TCIAudioBlock tCIAudioBlock = rentTCIAudioBlock();
			tCIAudioBlock.Receiver = id;
			tCIAudioBlock.SampleRate = GetChannelOutputRate(0, id);
			tCIAudioBlock.SamplesPerChannel = nsamples;
			tCIAudioBlock.Left = array;
			tCIAudioBlock.Right = array2;
			enqueueTCIAudio(tCIAudioBlock);
		}
	}

	private unsafe static void OnTCITxAudioInSamples(int nsamples, double* data)
	{
		if (data == null || nsamples <= 0)
		{
			return;
		}
		lock (m_objTCITxStateLock)
		{
			int num = 0;
			while (num < nsamples && m_tciTxSampleQueue.Count > 0)
			{
				float[] array = m_tciTxSampleQueue.Peek();
				int val = array.Length - m_tciTxSampleQueueOffset;
				int num2 = Math.Min(nsamples - num, val);
				for (int i = 0; i < num2; i++)
				{
					data[2 * (num + i) + 1] = (data[2 * (num + i)] = array[m_tciTxSampleQueueOffset + i]);
				}
				num += num2;
				m_tciTxSampleQueueOffset += num2;
				m_tciTxQueuedSamples = Math.Max(0, m_tciTxQueuedSamples - num2);
				if (m_tciTxSampleQueueOffset >= array.Length)
				{
					m_tciTxSampleQueue.Dequeue();
					m_tciTxSampleQueueOffset = 0;
				}
			}
			for (int j = num; j < nsamples; j++)
			{
				data[2 * j] = 0.0;
				data[2 * j + 1] = 0.0;
			}
		}
		m_tciTxStreamEvent.Set();
	}

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendCBPushVox(int id, PushVox Del);

	private static void create_wb(int adc)
	{
		if (EXPOSEwb)
		{
			wideband[adc] = new wideband(adc);
		}
	}

	public static wideband Getwb(int adc)
	{
		if (wideband[adc] == null || wideband[adc].IsDisposed)
		{
			create_wb(adc);
		}
		wideband[adc].Show();
		if (wideband[adc].WindowState == FormWindowState.Minimized)
		{
			wideband[adc].WindowState = FormWindowState.Normal;
		}
		return wideband[adc];
	}

	public static void Hidewb(int adc)
	{
		if (wideband[adc] != null)
		{
			wideband[adc].Hide();
		}
	}

	public static void Closewb(int adc)
	{
		if (wideband[adc] != null)
		{
			wideband[adc].Close();
		}
	}

	public static void Savewb(int adc)
	{
		if (wideband[adc] != null)
		{
			wideband[adc].SaveWideBand();
		}
	}
}
