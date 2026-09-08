using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Thetis;

public class TCPIPtciSocketListener
{
	private enum TCIOutboundPriority
	{
		Urgent,
		Binary,
		Control
	}

	private sealed class TCIOutboundFrame
	{
		public byte[] Frame;

		public string LogText;
	}

	private sealed class TCIRxAudioResamplerState
	{
		public int InputRate;

		public int OutputRate;

		public IntPtr LeftResampler = IntPtr.Zero;

		public IntPtr RightResampler = IntPtr.Zero;
	}

	public delegate void ClientConnected();

	public delegate void ClientDisconnected();

	public delegate void ClientError(SocketException se);

	public struct VFOData
	{
		public double freqMHz;

		public int offsetHz;

		public double centreMHz;

		public int chan;

		public int duplicate_tochan;

		public bool replace_if_duplicated;

		public bool cen;

		public int rx;

		public bool sendIF;

		public bool SendTXInfo;

		public Band TXfreqBand;

		public bool RX2EnabledForTX;

		public bool TXVFOB;
	}

	private enum EOpcodeType
	{
		Fragment = 0,
		Text = 1,
		Binary = 2,
		ClosedConnection = 8,
		Ping = 9,
		Pong = 10
	}

	private const int SOCKET_READ_TIMEOUT_MS = 250;

	private const int SOCKET_READ_BUFFER_SIZE = 8192;

	public ClientConnected ClientConnectedHandlers;

	public ClientDisconnected ClientDisconnectedHandlers;

	public ClientError ClientErrorHandlers;

	private Console _console;

	private TcpClient m_client;

	private NetworkStream m_stream;

	private bool m_stopClient;

	private bool m_disconnected;

	private int m_disconnectNotified;

	private Thread m_clientListenerThread;

	private bool m_markedForDeletion;

	private bool m_bWebSocket;

	private Thread m_VFODataThread;

	private Thread m_sendThread;

	private TCPIPtciServer m_server;

	private int m_nRateLimit;

	private LinkedList<VFOData> m_vfoDataList = new LinkedList<VFOData>();

	private List<byte> _m_buffer = new List<byte>();

	private Stopwatch m_swVFO = new Stopwatch();

	private System.Threading.Timer m_tmVFOtimer;

	private Stopwatch m_swCentre = new Stopwatch();

	private System.Threading.Timer m_tmCentretimer;

	private Stopwatch m_swTXFrequency = new Stopwatch();

	private System.Threading.Timer m_tmTXFrequency;

	private readonly object m_objStreamLock = new object();

	private readonly object m_objOutboundLock = new object();

	private readonly object m_objTxQueueLock = new object();

	private readonly object m_objRxAudioLock = new object();

	private const int MAX_TX_AUDIO_QUEUE_BLOCKS = 64;

	private const int MAX_TX_AUDIO_QUEUE_COMPLEX_SAMPLES = 96000;

	private readonly AutoResetEvent m_outboundFrameEvent = new AutoResetEvent(initialState: false);

	private readonly HashSet<int> m_iqStreamEnabled = new HashSet<int>();

	private readonly HashSet<int> m_audioStreamEnabled = new HashSet<int>();

	private readonly Queue<TCIQueuedTxAudio> m_txAudioQueue = new Queue<TCIQueuedTxAudio>();

	private readonly Queue<TCIOutboundFrame> m_outboundUrgentFrames = new Queue<TCIOutboundFrame>();

	private readonly Queue<TCIOutboundFrame> m_outboundBinaryFrames = new Queue<TCIOutboundFrame>();

	private readonly Queue<TCIOutboundFrame> m_outboundControlFrames = new Queue<TCIOutboundFrame>();

	private readonly Queue<string> m_outboundCoalescedOrder = new Queue<string>();

	private readonly HashSet<string> m_outboundCoalescedKeys = new HashSet<string>();

	private readonly Dictionary<string, TCIOutboundFrame> m_outboundCoalescedFrames = new Dictionary<string, TCIOutboundFrame>();

	private readonly Dictionary<int, TCIPendingFloatBuffer> m_rxAudioLeftPending = new Dictionary<int, TCIPendingFloatBuffer>();

	private readonly Dictionary<int, TCIPendingFloatBuffer> m_rxAudioRightPending = new Dictionary<int, TCIPendingFloatBuffer>();

	private readonly Dictionary<int, TCIRxAudioResamplerState> m_rxAudioResamplers = new Dictionary<int, TCIRxAudioResamplerState>();

	private int[] m_hwSampleRate = new int[2] { 48000, 48000 };

	private int m_audioSampleRate = 48000;

	private TCISampleType m_audioSampleType = TCISampleType.FLOAT32;

	private int m_audioStreamChannels = 2;

	private int m_audioStreamSamples = 2048;

	private bool m_audioStreamSamplesExplicitlySet;

	private int m_txStreamAudioBufferingMs = 50;

	private bool m_txUsesTCIAudio;

	private bool m_tciPttActive;

	private int m_txQueuedComplexSamples;

	private bool m_seenModernTxAudioNegotiation;

	private readonly clsTCISensorManager m_sensorManager = new clsTCISensorManager();

	private System.Threading.Timer m_tmRxSensors;

	private System.Threading.Timer m_tmTxSensors;

	private object m_objVFODataLock = new object();

	private Console consoleThreadSafe
	{
		get
		{
			if (_console == null)
			{
				return null;
			}
			if (_console.InvokeRequired)
			{
				return (Console)_console.Invoke((Func<Console>)(() => _console.ThreadSafeTCIAccessor));
			}
			return _console.ThreadSafeTCIAccessor;
		}
	}

	public TCPIPtciSocketListener(TcpClient client, Console c, TCPIPtciServer server, int rateLimit)
	{
		_console = c;
		m_nRateLimit = rateLimit;
		m_server = server;
		m_client = client;
		m_client.ReceiveTimeout = 0;
		m_stream = client.GetStream();
		m_stream.ReadTimeout = -1;
		m_audioStreamSamples = getDefaultAudioStreamSamples(m_audioSampleRate);
		for (int i = 0; i < m_hwSampleRate.Length; i++)
		{
			m_hwSampleRate[i] = cmaster.GetInputRate(0, i);
		}
	}

	~TCPIPtciSocketListener()
	{
		StopSocketListener();
	}

	public void ClickedOnSpot(string callsign, long frequency, int rx = -1, int chan = -1)
	{
		if (rx == -1 || chan == -1)
		{
			sendClickedOnSpot(callsign, frequency);
		}
		else
		{
			sendClickedOnSpotRX(rx - 1, chan, callsign, frequency);
		}
	}

	public void ThetisFocusChange(bool focus)
	{
		if (!m_disconnected)
		{
			sendAppFocus(focus);
		}
	}

	public void RX2EnabledChange(bool enabled)
	{
		if (!m_disconnected)
		{
			sendRXEnable(1, enabled);
			sendTXEnable(1, enabled && !consoleThreadSafe.MOX);
		}
	}

	public void HWSampleRateChange(int rx, int oldSampleRate, int newSampleRate)
	{
		if (m_disconnected)
		{
			return;
		}
		int publishedIQSampleRateLocked;
		int num3;
		lock (m_objStreamLock)
		{
			int num = rx - 1;
			if (num >= 0 && num < m_hwSampleRate.Length)
			{
				m_hwSampleRate[num] = newSampleRate;
			}
			publishedIQSampleRateLocked = getPublishedIQSampleRateLocked();
			int num2 = 48000;
			if (m_hwSampleRate != null && m_hwSampleRate.Length != 0 && m_hwSampleRate[0] > 0)
			{
				num2 = m_hwSampleRate[0];
			}
			num3 = num2 / 2;
		}
		sendIQSampleRate(publishedIQSampleRateLocked);
		sendIFLimits(-num3, num3);
	}

	internal bool RequiresRxSensorUpdate(int receiver, int channel)
	{
		return m_sensorManager.RequiresRxChannelUpdate(receiver, channel);
	}

	internal bool SensorRequiresUpdate(int receiver, Reading reading)
	{
		return m_sensorManager.SensorRequiresUpdate(receiver, reading);
	}

	internal bool RequiresTxSensorUpdate()
	{
		return m_sensorManager.RequiresTxUpdate();
	}

	internal void MeterReadingsChanged(int rx, bool tx, ref Dictionary<Reading, float> readings)
	{
		if (readings == null)
		{
			return;
		}
		if (tx)
		{
			if (readings.TryGetValue(Reading.MIC, out var value) && readings.TryGetValue(Reading.PWR, out var value2) && readings.TryGetValue(Reading.SWR, out var value3))
			{
				m_sensorManager.SetTxReadings(value, value2, value2, value3);
			}
			return;
		}
		int num = rx - 1;
		if (num >= 0 && num <= 1 && readings.TryGetValue(Reading.SIGNAL_STRENGTH, out var value4) && readings.TryGetValue(Reading.AVG_SIGNAL_STRENGTH, out var value5) && readings.TryGetValue(Reading.SIGNAL_MAX_BIN, out var value6))
		{
			m_sensorManager.SetRxChannelReading(num, 0, value4, value5, value6);
		}
	}

	internal int MinimumRequiredRxSensorInterval()
	{
		if (!m_sensorManager.RxSensorsEnabled)
		{
			return int.MaxValue;
		}
		return m_sensorManager.RxIntervalMs;
	}

	internal int MinimumRequiredTxSensorInterval()
	{
		if (!m_sensorManager.TxSensorsEnabled)
		{
			return int.MaxValue;
		}
		return m_sensorManager.TxIntervalMs;
	}

	private int getPublishedIQSampleRate()
	{
		lock (m_objStreamLock)
		{
			return getPublishedIQSampleRateLocked();
		}
	}

	private int getPublishedIQSampleRateLocked()
	{
		int num = 48000;
		for (int i = 0; i < m_hwSampleRate.Length; i++)
		{
			if (m_hwSampleRate[i] > num)
			{
				num = m_hwSampleRate[i];
			}
		}
		return Math.Min(num, 384000);
	}

	private unsafe void destroyRxAudioResamplerState(TCIRxAudioResamplerState state)
	{
		if (state != null)
		{
			if (state.LeftResampler != IntPtr.Zero)
			{
				WDSP.destroy_resampleFV(state.LeftResampler.ToPointer());
				state.LeftResampler = IntPtr.Zero;
			}
			if (state.RightResampler != IntPtr.Zero)
			{
				WDSP.destroy_resampleFV(state.RightResampler.ToPointer());
				state.RightResampler = IntPtr.Zero;
			}
			state.InputRate = 0;
			state.OutputRate = 0;
		}
	}

	private void clearRxAudioStateForReceiver(int receiver)
	{
		lock (m_objRxAudioLock)
		{
			m_rxAudioLeftPending.Remove(receiver);
			m_rxAudioRightPending.Remove(receiver);
			if (m_rxAudioResamplers.TryGetValue(receiver, out var value))
			{
				destroyRxAudioResamplerState(value);
				m_rxAudioResamplers.Remove(receiver);
			}
		}
	}

	private void clearRxAudioStreamState()
	{
		lock (m_objRxAudioLock)
		{
			m_rxAudioLeftPending.Clear();
			m_rxAudioRightPending.Clear();
			foreach (TCIRxAudioResamplerState value in m_rxAudioResamplers.Values)
			{
				destroyRxAudioResamplerState(value);
			}
			m_rxAudioResamplers.Clear();
		}
	}

	private unsafe int resampleRxAudioSamples(int receiver, int inputRate, int targetRate, float[] left, float[] right, int samples, out float[] leftOut, out float[] rightOut, out bool resampled)
	{
		leftOut = left;
		if (right != null)
		{
			rightOut = right;
		}
		else
		{
			rightOut = left;
		}
		resampled = false;
		if (left == null || samples <= 0)
		{
			return 0;
		}
		if (inputRate <= 0 || targetRate <= 0 || inputRate == targetRate)
		{
			return Math.Min(samples, left.Length);
		}
		if (!m_rxAudioResamplers.TryGetValue(receiver, out var value))
		{
			value = new TCIRxAudioResamplerState();
			m_rxAudioResamplers[receiver] = value;
		}
		if (value.LeftResampler == IntPtr.Zero || value.RightResampler == IntPtr.Zero || value.InputRate != inputRate || value.OutputRate != targetRate)
		{
			destroyRxAudioResamplerState(value);
			value.LeftResampler = (IntPtr)WDSP.create_resampleFV(inputRate, targetRate);
			value.RightResampler = (IntPtr)WDSP.create_resampleFV(inputRate, targetRate);
			value.InputRate = inputRate;
			value.OutputRate = targetRate;
		}
		if (value.LeftResampler == IntPtr.Zero || value.RightResampler == IntPtr.Zero)
		{
			return Math.Min(samples, left.Length);
		}
		float[] array = right;
		if (array == null || array.Length < samples)
		{
			array = new float[samples];
			int num = 0;
			if (right != null && right.Length != 0)
			{
				num = Math.Min(samples, right.Length);
				Array.Copy(right, 0, array, 0, num);
			}
			if (num < samples)
			{
				Array.Copy(left, num, array, num, samples - num);
			}
		}
		int num2 = Math.Max(samples + 64, (int)Math.Ceiling((double)samples * (double)targetRate / (double)inputRate) + 64);
		float[] array2 = new float[num2];
		float[] array3 = new float[num2];
		int val = 0;
		int val2 = 0;
		fixed (float* input = left)
		{
			fixed (float* input2 = array)
			{
				fixed (float* output = array2)
				{
					fixed (float* output2 = array3)
					{
						WDSP.xresampleFV(input, output, samples, &val, value.LeftResampler.ToPointer());
						WDSP.xresampleFV(input2, output2, samples, &val2, value.RightResampler.ToPointer());
					}
				}
			}
		}
		int num3 = Math.Min(val, val2);
		if (num3 <= 0)
		{
			leftOut = Array.Empty<float>();
			rightOut = Array.Empty<float>();
			return 0;
		}
		if (num3 != array2.Length)
		{
			float[] array4 = new float[num3];
			float[] array5 = new float[num3];
			Array.Copy(array2, array4, num3);
			Array.Copy(array3, array5, num3);
			array2 = array4;
			array3 = array5;
		}
		leftOut = array2;
		rightOut = array3;
		resampled = true;
		return num3;
	}

	public void DrivePowerChange(int rx, int newPower, bool tune)
	{
		if (!m_disconnected)
		{
			if (tune)
			{
				sendTunePower(rx - 1, newPower);
			}
			else
			{
				sendDrivePower(rx - 1, newPower);
			}
		}
	}

	public void TuneChange(int rx, bool oldTune, bool newTune)
	{
		if (!m_disconnected)
		{
			sendTune(rx - 1, newTune);
		}
	}

	public void SplitChange(int rx, bool newSplit)
	{
		if (!m_disconnected)
		{
			bool vFOSplit = consoleThreadSafe.VFOSplit;
			sendSplit(rx - 1, vFOSplit);
		}
	}

	public void MuteChanged(int rx, bool newState)
	{
		if (!m_disconnected)
		{
			sendMute(consoleThreadSafe.MUT || (consoleThreadSafe.RX2Enabled && consoleThreadSafe.MUT2));
			sendMuteRX(rx - 1, newState);
		}
	}

	public void AnfChanged(int rx, bool newState)
	{
		if (!m_disconnected)
		{
			sendAnfEnable(rx - 1, newState);
		}
	}

	public void RxAfGainChanged(int rx, bool is_subrx, int gain)
	{
		if (!m_disconnected)
		{
			int chan = (is_subrx ? 1 : 0);
			double volume = audioGainToDb((float)gain / 100f);
			sendRxVolume(rx - 1, chan, volume);
		}
	}

	public void CTUNChanged(int rx, bool enabled)
	{
		if (!m_disconnected)
		{
			sendCTUN(rx - 1, enabled);
		}
	}

	public void VFOSyncChanged(bool enabled)
	{
		if (!m_disconnected)
		{
			sendVFOSyncEx(enabled);
		}
	}

	public void FMDeviationChanged(int rx, int deviationHz)
	{
		if (!m_disconnected)
		{
			sendFMDeviationEx(rx - 1, deviationHz);
		}
	}

	public void AGCModeChanged(int rx, AGCMode mode)
	{
		if (!m_disconnected)
		{
			sendAgcMode(rx - 1, mode);
		}
	}

	public void AGCAutoChanged(int rx, bool enabled)
	{
		if (!m_disconnected)
		{
			sendAgcAutoEx(rx - 1, enabled);
		}
	}

	public void TXProfileChanged(string profile)
	{
		if (!m_disconnected)
		{
			sendTXProfile(profile);
		}
	}

	public void TXProfilesChanged()
	{
		if (!m_disconnected)
		{
			sendTXProfiles();
		}
	}

	public void CalibrationChanged(int rx)
	{
		if (!m_disconnected)
		{
			float meter = ((rx == 0) ? consoleThreadSafe.RX1MeterCalOffset : consoleThreadSafe.RX2MeterCalOffset);
			float display = ((rx == 0) ? consoleThreadSafe.RX1DisplayCalOffset : consoleThreadSafe.RX2DisplayCalOffset);
			float xvtr = ((rx == 0) ? consoleThreadSafe.RX1XVTRGainOffset : consoleThreadSafe.RX2XVTRGainOffset);
			float six_meter = ((rx == 0) ? consoleThreadSafe.RX6mGainOffset_RX1 : consoleThreadSafe.RX6mGainOffset_RX2);
			float tXDisplayCalOffset = consoleThreadSafe.TXDisplayCalOffset;
			sendCalibration(rx, meter, display, xvtr, six_meter, tXDisplayCalOffset);
		}
	}

	public void MONChanged(bool newState)
	{
		if (!m_disconnected)
		{
			sendMONEnable(newState);
		}
	}

	public void MONVolumeChanged(int newVolume)
	{
		if (!m_disconnected)
		{
			sendMONVolume(linearToDbVolume(newVolume));
		}
	}

	public void VolumeChanged(int newVolume)
	{
		if (!m_disconnected)
		{
			sendVolume(linearToDbVolume(newVolume));
		}
	}

	public void BalanceChanged(int rx, bool is_subrx, int newBalance)
	{
		if (!m_disconnected)
		{
			int chan = (is_subrx ? 1 : 0);
			double balance = 40.0 - (double)newBalance * 0.8;
			sendRxBalance(rx - 1, chan, balance);
		}
	}

	public void RxStepAttChanged(int rx, int attenuation)
	{
		if (!m_disconnected)
		{
			sendRxStepAttEx(rx - 1, attenuation);
		}
	}

	public void RxPreampAttChanged(int rx, PreampMode preamp_mode)
	{
		if (!m_disconnected)
		{
			int num = preampModeToAttenuation(preamp_mode);
			sendRxPreampAttEx(rx - 1, -num);
		}
	}

	public void RxStepAttEnabledChanged(int rx, bool enabled)
	{
		if (!m_disconnected)
		{
			sendRxStepAttEnabledEx(rx - 1, enabled);
		}
	}

	public void AGCGainChanged(int rx, int newGain)
	{
		if (!m_disconnected)
		{
			sendAgcGain(rx - 1, newGain);
		}
	}

	public void RITChanged(bool newState)
	{
		if (!m_disconnected)
		{
			sendRITEnable(0, newState);
			sendRITEnable(1, newState);
		}
	}

	public void XITChanged(bool newState)
	{
		if (!m_disconnected)
		{
			sendXITEnable(0, newState);
			sendXITEnable(1, newState);
		}
	}

	public void RITValueChanged(int newValue)
	{
		if (!m_disconnected)
		{
			sendRITOffset(0, newValue);
			sendRITOffset(1, newValue);
		}
	}

	public void XITValueChanged(int newValue)
	{
		if (!m_disconnected)
		{
			sendXITOffset(0, newValue);
			sendXITOffset(1, newValue);
		}
	}

	public void CwMacrosSpeedChanged(int newSpeed)
	{
		if (!m_disconnected)
		{
			sendCwMacrosSpeed(newSpeed);
		}
	}

	public void CwMacrosDelayChanged(int newDelay)
	{
		if (!m_disconnected)
		{
			sendCwMacrosDelay(newDelay);
		}
	}

	public void CwKeyerSpeedChanged(int newSpeed)
	{
		if (!m_disconnected)
		{
			sendCwKeyerSpeed(newSpeed);
		}
	}

	public void CwMacrosEmpty(int rx)
	{
		if (!m_disconnected)
		{
			sendCwMacrosEmpty(rx);
		}
	}

	public void CwCallsignSent(string callsign)
	{
		if (!m_disconnected)
		{
			sendCallsignSend(callsign);
		}
	}

	public void NBChanged(int rx, int newNB)
	{
		if (!m_disconnected)
		{
			bool enabled = newNB > 0;
			sendNBEnable(rx - 1, enabled, is_extended: false, newNB);
			sendNBEnable(rx - 1, enabled, is_extended: true, newNB);
		}
	}

	public void NRChanged(int rx, int newNR)
	{
		if (!m_disconnected)
		{
			bool enabled = newNR > 0;
			sendNREnable(rx - 1, enabled, is_extended: false, newNR);
			sendNREnable(rx - 1, enabled, is_extended: true, newNR);
		}
	}

	public void BinChanged(int rx, bool newState)
	{
		if (!m_disconnected)
		{
			sendRxBinEnable(rx - 1, newState);
		}
	}

	public void LockChanged(int rx, bool newState)
	{
		if (!m_disconnected)
		{
			sendLock(rx - 1, newState);
		}
	}

	public void VFOLocksChanged()
	{
		if (!m_disconnected)
		{
			sendAllVFOLocks();
		}
	}

	public void SqlChanged(int rx, SquelchState newState)
	{
		if (!m_disconnected)
		{
			sendSqlEnable(rx - 1, newState != SquelchState.OFF);
		}
	}

	public void SqlLevelChanged(int rx, int newValue)
	{
		if (!m_disconnected)
		{
			sendSqlLevel(rx - 1, newValue);
		}
	}

	public void ApfChanged(int rx, bool newState)
	{
		if (!m_disconnected)
		{
			sendRxApfEnable(rx - 1, newState);
		}
	}

	public void NfChanged(bool newState)
	{
		if (!m_disconnected)
		{
			sendRxNfEnable(0, newState);
			sendRxNfEnable(1, newState);
		}
	}

	public void DiglOffsetChanged(int newValue)
	{
		if (!m_disconnected)
		{
			sendDiglOffset(newValue);
		}
	}

	public void DiguOffsetChanged(int newValue)
	{
		if (!m_disconnected)
		{
			sendDiguOffset(newValue);
		}
	}

	public void TXFrequencyChanged(long new_frequency, Band new_band, bool rx2_enabled, bool tx_vfob)
	{
		if (!m_disconnected)
		{
			sendTXFrequencyChanged(new_frequency, new_band, rx2_enabled, tx_vfob);
		}
	}

	private void limitList()
	{
		lock (m_objVFODataLock)
		{
			while (m_vfoDataList.Count > 10)
			{
				m_vfoDataList.RemoveFirst();
			}
		}
	}

	private async void VFOdata()
	{
		while (!m_stopClient)
		{
			int count;
			lock (m_objVFODataLock)
			{
				count = m_vfoDataList.Count;
			}
			if (count > 0)
			{
				VFOData value;
				lock (m_objVFODataLock)
				{
					value = m_vfoDataList.First.Value;
					m_vfoDataList.RemoveFirst();
				}
				if (value.SendTXInfo)
				{
					sendTXFrequencyChanged((long)(value.freqMHz * 1000000.0), value.TXfreqBand, value.RX2EnabledForTX, value.TXVFOB);
				}
				else if (value.cen)
				{
					sendDDS(value.rx, (long)(value.centreMHz * 1000000.0));
					if (value.sendIF)
					{
						sendIF(value.rx, value.chan, value.offsetHz);
					}
				}
				else if (value.duplicate_tochan != -1)
				{
					if (!value.replace_if_duplicated)
					{
						if (value.sendIF)
						{
							sendIF(value.rx, value.chan, value.offsetHz);
						}
						sendVFO(value.rx, value.chan, (long)(value.freqMHz * 1000000.0));
					}
					if (value.sendIF)
					{
						sendIF(value.rx, value.duplicate_tochan, value.offsetHz);
					}
					sendVFO(value.rx, value.duplicate_tochan, (long)(value.freqMHz * 1000000.0));
				}
				else
				{
					if (value.sendIF)
					{
						sendIF(value.rx, value.chan, value.offsetHz);
					}
					sendVFO(value.rx, value.chan, (long)(value.freqMHz * 1000000.0));
				}
			}
			lock (m_objVFODataLock)
			{
				count = m_vfoDataList.Count;
			}
			if (!m_stopClient && count == 0)
			{
				await Task.Delay(1);
			}
		}
	}

	private void vfoFrequencyChange(VFOData vfod)
	{
		if (m_disconnected)
		{
			return;
		}
		lock (m_objVFODataLock)
		{
			limitList();
			m_vfoDataList.AddLast(vfod);
		}
	}

	private void centreFrequencyChange(VFOData vfod)
	{
		if (m_disconnected)
		{
			return;
		}
		lock (m_objVFODataLock)
		{
			limitList();
			m_vfoDataList.AddLast(vfod);
		}
	}

	private void txFrequencyChange(VFOData vfod)
	{
		if (m_disconnected)
		{
			return;
		}
		lock (m_objVFODataLock)
		{
			limitList();
			m_vfoDataList.AddLast(vfod);
		}
	}

	public void MoxChange(int rx, bool oldMox, bool newMox)
	{
		if (m_disconnected)
		{
			return;
		}
		if (newMox)
		{
			if (rx == 1)
			{
				if (consoleThreadSafe.RX2Enabled)
				{
					sendTXEnable(1, bEnable: false);
				}
			}
			else
			{
				sendTXEnable(0, bEnable: false);
			}
		}
		else if (rx == 1)
		{
			if (consoleThreadSafe.RX2Enabled)
			{
				sendTXEnable(1, bEnable: true);
			}
		}
		else
		{
			sendTXEnable(0, bEnable: true);
		}
		sendMOX(rx - 1, newMox);
	}

	public void ModeChange(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
	{
		if (!m_disconnected)
		{
			sendMode(rx - 1, newMode);
		}
	}

	public void BandChange(int rx, Band oldBand, Band newBand)
	{
		if (!m_disconnected)
		{
			sendTXEnable(rx - 1, rx == 1 || consoleThreadSafe.RX2Enabled);
		}
	}

	public void FilterChange(int rx, Filter oldFilter, Filter newFilter, Band band, int low, int high)
	{
		if (!m_disconnected)
		{
			sendFilterBand(rx - 1, low, high);
		}
	}

	public void FilterEdgesChange(int rx, Filter filter, Band band, int low, int high)
	{
		if (!m_disconnected)
		{
			sendFilterBand(rx - 1, low, high);
		}
	}

	public void TXFilterBandChanged(int low, int high)
	{
		if (!m_disconnected)
		{
			sendTXFilterBandEx(low, high);
		}
	}

	public void PowerChange(bool oldPower, bool newPower)
	{
		if (!m_disconnected)
		{
			sendStartStop(newPower);
		}
	}

	public void StartSocketListener()
	{
		if (m_client != null)
		{
			m_sendThread = new Thread(SendThreadProc);
			m_sendThread.Name = "TCI client sender Thread";
			m_sendThread.Priority = ThreadPriority.AboveNormal;
			m_sendThread.Start();
			m_VFODataThread = new Thread(VFOdata);
			m_VFODataThread.Priority = ThreadPriority.Normal;
			m_VFODataThread.Start();
			m_clientListenerThread = new Thread(SocketListenerThreadStart);
			m_clientListenerThread.Name = "TCI client listener Thread";
			m_clientListenerThread.Start();
		}
	}

	private static byte[] getFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(Message ?? string.Empty);
		byte[] array = new byte[10];
		long num = -1L;
		long num2 = bytes.Length;
		array[0] = (byte)(128 + Opcode);
		if (num2 <= 125)
		{
			array[1] = (byte)num2;
			num = 2L;
		}
		else if (num2 >= 126 && num2 <= 65535)
		{
			array[1] = 126;
			array[2] = (byte)((num2 >> 8) & 0xFF);
			array[3] = (byte)(num2 & 0xFF);
			num = 4L;
		}
		else
		{
			array[1] = 127;
			array[2] = (byte)((num2 >> 56) & 0xFF);
			array[3] = (byte)((num2 >> 48) & 0xFF);
			array[4] = (byte)((num2 >> 40) & 0xFF);
			array[5] = (byte)((num2 >> 32) & 0xFF);
			array[6] = (byte)((num2 >> 24) & 0xFF);
			array[7] = (byte)((num2 >> 16) & 0xFF);
			array[8] = (byte)((num2 >> 8) & 0xFF);
			array[9] = (byte)(num2 & 0xFF);
			num = 10L;
		}
		byte[] array2 = new byte[num + num2];
		long num3 = 0L;
		for (long num4 = 0L; num4 < num; num4++)
		{
			array2[num3] = array[num4];
			num3++;
		}
		for (long num4 = 0L; num4 < num2; num4++)
		{
			array2[num3] = bytes[num4];
			num3++;
		}
		return array2;
	}

	private static byte[] GetFrameFromBytes(byte[] payload, EOpcodeType opcode = EOpcodeType.Binary)
	{
		if (payload == null)
		{
			payload = Array.Empty<byte>();
		}
		byte[] array = new byte[10];
		long num = -1L;
		long num2 = payload.LongLength;
		array[0] = (byte)(128 + opcode);
		if (num2 <= 125)
		{
			array[1] = (byte)num2;
			num = 2L;
		}
		else if (num2 <= 65535)
		{
			array[1] = 126;
			array[2] = (byte)((num2 >> 8) & 0xFF);
			array[3] = (byte)(num2 & 0xFF);
			num = 4L;
		}
		else
		{
			array[1] = 127;
			array[2] = (byte)((num2 >> 56) & 0xFF);
			array[3] = (byte)((num2 >> 48) & 0xFF);
			array[4] = (byte)((num2 >> 40) & 0xFF);
			array[5] = (byte)((num2 >> 32) & 0xFF);
			array[6] = (byte)((num2 >> 24) & 0xFF);
			array[7] = (byte)((num2 >> 16) & 0xFF);
			array[8] = (byte)((num2 >> 8) & 0xFF);
			array[9] = (byte)(num2 & 0xFF);
			num = 10L;
		}
		byte[] array2 = new byte[num + num2];
		Buffer.BlockCopy(array, 0, array2, 0, (int)num);
		if (num2 > 0)
		{
			Buffer.BlockCopy(payload, 0, array2, (int)num, (int)num2);
		}
		return array2;
	}

	private static string getCoalescedTextFrameKey(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			return null;
		}
		string text = message.Trim();
		int num = text.IndexOf(':');
		if (num <= 0)
		{
			return null;
		}
		string text2 = text.Substring(0, num).ToLowerInvariant();
		string[] array = text.Substring(num + 1).TrimEnd(';').Split(',');
		switch (text2)
		{
		case "vfo":
		case "if":
			if (array.Length >= 2)
			{
				return text2 + ":" + array[0] + "," + array[1];
			}
			break;
		case "dds":
		case "rx_filter_band":
		case "rx_step_att_ex":
		case "rx_balance":
		case "tune_drive":
		case "rx_preamp_att_ex":
		case "agc_gain":
		case "drive":
		case "tune":
			if (array.Length >= 1)
			{
				return text2 + ":" + array[0];
			}
			break;
		case "tx_filter_band_ex":
		case "tx_frequency":
		case "tx_frequency_ex":
		case "volume":
			return text2;
		}
		return null;
	}

	private bool hasPendingOutboundFramesLocked()
	{
		if (m_outboundUrgentFrames.Count <= 0 && m_outboundBinaryFrames.Count <= 0 && m_outboundControlFrames.Count <= 0)
		{
			return m_outboundCoalescedOrder.Count > 0;
		}
		return true;
	}

	private bool tryDequeueNextOutboundFrameLocked(out TCIOutboundFrame frame)
	{
		frame = null;
		if (m_outboundUrgentFrames.Count > 0)
		{
			frame = m_outboundUrgentFrames.Dequeue();
			return true;
		}
		if (m_outboundBinaryFrames.Count > 0)
		{
			frame = m_outboundBinaryFrames.Dequeue();
			return true;
		}
		if (m_outboundControlFrames.Count > 0)
		{
			frame = m_outboundControlFrames.Dequeue();
			return true;
		}
		while (m_outboundCoalescedOrder.Count > 0)
		{
			string text = m_outboundCoalescedOrder.Dequeue();
			m_outboundCoalescedKeys.Remove(text);
			if (m_outboundCoalescedFrames.TryGetValue(text, out frame))
			{
				m_outboundCoalescedFrames.Remove(text);
				return true;
			}
		}
		return false;
	}

	private void clearOutboundFrames()
	{
		lock (m_objOutboundLock)
		{
			m_outboundUrgentFrames.Clear();
			m_outboundBinaryFrames.Clear();
			m_outboundControlFrames.Clear();
			m_outboundCoalescedOrder.Clear();
			m_outboundCoalescedKeys.Clear();
			m_outboundCoalescedFrames.Clear();
		}
	}

	private void flushOutboundFrames(int timeoutMs)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (stopwatch.ElapsedMilliseconds < timeoutMs)
		{
			lock (m_objOutboundLock)
			{
				if (!hasPendingOutboundFramesLocked())
				{
					break;
				}
			}
			Thread.Sleep(5);
		}
	}

	private void enqueueOutboundFrame(byte[] frameBytes, string logText, TCIOutboundPriority priority, string coalescedKey = null)
	{
		if (frameBytes == null || frameBytes.Length == 0)
		{
			return;
		}
		lock (m_objOutboundLock)
		{
			TCIOutboundFrame tCIOutboundFrame = new TCIOutboundFrame
			{
				Frame = frameBytes,
				LogText = logText
			};
			if (!string.IsNullOrEmpty(coalescedKey) && priority == TCIOutboundPriority.Control)
			{
				m_outboundCoalescedFrames[coalescedKey] = tCIOutboundFrame;
				if (m_outboundCoalescedKeys.Add(coalescedKey))
				{
					m_outboundCoalescedOrder.Enqueue(coalescedKey);
				}
			}
			else
			{
				switch (priority)
				{
				case TCIOutboundPriority.Urgent:
					m_outboundUrgentFrames.Enqueue(tCIOutboundFrame);
					break;
				case TCIOutboundPriority.Binary:
					m_outboundBinaryFrames.Enqueue(tCIOutboundFrame);
					break;
				default:
					m_outboundControlFrames.Enqueue(tCIOutboundFrame);
					break;
				}
			}
		}
		m_outboundFrameEvent.Set();
	}

	private void SendThreadProc()
	{
		while (true)
		{
			TCIOutboundFrame frame = null;
			lock (m_objOutboundLock)
			{
				tryDequeueNextOutboundFrameLocked(out frame);
			}
			if (frame == null)
			{
				if (m_stopClient)
				{
					lock (m_objOutboundLock)
					{
						if (!hasPendingOutboundFramesLocked())
						{
							break;
						}
					}
				}
				m_outboundFrameEvent.WaitOne(20);
				continue;
			}
			try
			{
				if (m_bWebSocket && m_client != null && m_stream != null && m_client.Connected)
				{
					m_stream.Write(frame.Frame, 0, frame.Frame.Length);
					if (!string.IsNullOrEmpty(frame.LogText) && m_server != null && m_server.LogForm != null)
					{
						m_server.LogForm.Log(bIn: false, frame.LogText);
					}
				}
			}
			catch (Exception)
			{
				abortSocketTransport();
			}
		}
	}

	private void abortSocketTransport()
	{
		m_stopClient = true;
		m_outboundFrameEvent.Set();
		lock (m_objStreamLock)
		{
			try
			{
				if (m_stream != null)
				{
					m_stream.Close();
					m_stream = null;
				}
			}
			catch
			{
			}
			try
			{
				m_client?.Close();
			}
			catch
			{
			}
		}
	}

	private static bool isSocketReadTimeout(IOException ex)
	{
		if (ex?.InnerException is SocketException ex2)
		{
			if (ex2.SocketErrorCode != SocketError.TimedOut)
			{
				return ex2.SocketErrorCode == SocketError.WouldBlock;
			}
			return true;
		}
		return false;
	}

	private bool upgradeToWebSocket(string msg)
	{
		try
		{
			string s = Regex.Match(msg, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
			string text = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(s)));
			byte[] bytes = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: " + text + "\r\n\r\n");
			m_stream.Write(bytes, 0, bytes.Length);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void sendStart()
	{
		sendTextFrame("start;");
	}

	private void sendStop()
	{
		sendTextFrame("stop;");
	}

	private void sendSplit(int rx, bool bSplit)
	{
		string sMsg = "split_enable:" + rx + "," + bSplit.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendRITEnable(int rx, bool enabled)
	{
		string sMsg = "rit_enable:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendXITEnable(int rx, bool enabled)
	{
		string sMsg = "xit_enable:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendRITOffset(int rx, int offset)
	{
		string sMsg = "rit_offset:" + rx + "," + offset + ";";
		sendTextFrame(sMsg);
	}

	private void sendXITOffset(int rx, int offset)
	{
		string sMsg = "xit_offset:" + rx + "," + offset + ";";
		sendTextFrame(sMsg);
	}

	private void sendRxBinEnable(int rx, bool enabled)
	{
		string sMsg = "rx_bin_enable:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendRxApfEnable(int rx, bool enabled)
	{
		string sMsg = "rx_apf_enable:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendRxNfEnable(int rx, bool enabled)
	{
		string sMsg = "rx_nf_enable:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendLock(int rx, bool enabled)
	{
		string sMsg = "lock:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendVFOLock(int rx, int chan, bool enabled)
	{
		string sMsg = "vfo_lock:" + rx + "," + chan + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendSqlEnable(int rx, bool enabled)
	{
		string sMsg = "sql_enable:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendSqlLevel(int rx, int level)
	{
		string sMsg = "sql_level:" + rx + "," + level + ";";
		sendTextFrame(sMsg);
	}

	private void sendCwMacrosSpeed(int speed)
	{
		sendTextFrame("cw_macros_speed:" + speed + ";");
	}

	private void sendCwMacrosDelay(int delayMs)
	{
		sendTextFrame("cw_macros_delay:" + delayMs + ";");
	}

	private void sendCwKeyerSpeed(int speed)
	{
		sendTextFrame("cw_keyer_speed:" + speed + ";");
	}

	private void sendCwMacrosEmpty(int rx)
	{
		sendTextFrame("cw_macros_empty:" + rx + ";");
	}

	private void sendCallsignSend(string callsign)
	{
		sendTextFrame("callsign_send:" + callsign + ";");
	}

	private bool tryGetVFOLockState(int rx, int chan, out bool enabled)
	{
		enabled = false;
		if (rx < 0 || rx > 1 || chan < 0 || chan > 1)
		{
			return false;
		}
		if (consoleThreadSafe == null || !consoleThreadSafe.RX2Enabled)
		{
			if (rx != 0)
			{
				return false;
			}
			enabled = ((chan == 0) ? consoleThreadSafe.VFOALock : consoleThreadSafe.VFOBLock);
			return true;
		}
		switch (rx)
		{
		case 0:
			if (chan != 0)
			{
				return false;
			}
			enabled = consoleThreadSafe.VFOALock;
			return true;
		case 1:
			enabled = consoleThreadSafe.VFOBLock;
			return true;
		default:
			return false;
		}
	}

	private bool trySetVFOLockState(int rx, int chan, bool enabled)
	{
		if (rx < 0 || rx > 1 || chan < 0 || chan > 1)
		{
			return false;
		}
		if (consoleThreadSafe == null || !consoleThreadSafe.RX2Enabled)
		{
			if (rx != 0)
			{
				return false;
			}
			if (chan == 0)
			{
				consoleThreadSafe.VFOALock = enabled;
			}
			else
			{
				consoleThreadSafe.VFOBLock = enabled;
			}
			return true;
		}
		switch (rx)
		{
		case 0:
			if (chan != 0)
			{
				return false;
			}
			consoleThreadSafe.VFOALock = enabled;
			return true;
		case 1:
			consoleThreadSafe.VFOBLock = enabled;
			return true;
		default:
			return false;
		}
	}

	private void sendAllVFOLocks()
	{
		if (consoleThreadSafe.RX2Enabled)
		{
			if (tryGetVFOLockState(0, 0, out var enabled))
			{
				sendVFOLock(0, 0, enabled);
			}
			if (tryGetVFOLockState(1, 0, out var enabled2))
			{
				sendVFOLock(1, 0, enabled2);
			}
			if (tryGetVFOLockState(1, 1, out var enabled3))
			{
				sendVFOLock(1, 1, enabled3);
			}
		}
		else
		{
			if (tryGetVFOLockState(0, 0, out var enabled4))
			{
				sendVFOLock(0, 0, enabled4);
			}
			if (tryGetVFOLockState(0, 1, out var enabled5))
			{
				sendVFOLock(0, 1, enabled5);
			}
		}
	}

	private void sendDiglOffset(int offset)
	{
		string sMsg = "digl_offset:" + offset + ";";
		sendTextFrame(sMsg);
	}

	private void sendDiguOffset(int offset)
	{
		string sMsg = "digu_offset:" + offset + ";";
		sendTextFrame(sMsg);
	}

	private void sendVFO(int rx, int chan, long vfo = -1L)
	{
		bool flag = m_server != null && consoleThreadSafe != null && consoleThreadSafe.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
		if (vfo == -1)
		{
			switch (rx)
			{
			case 0:
				switch (chan)
				{
				case 0:
					vfo = (long)(consoleThreadSafe.VFOAFreq * 1000000.0);
					break;
				case 1:
					vfo = (long)(consoleThreadSafe.VFOBFreq * 1000000.0);
					break;
				}
				break;
			case 1:
				switch (chan)
				{
				case 0:
					vfo = ((!flag) ? ((long)(consoleThreadSafe.VFOBFreq * 1000000.0)) : ((long)(consoleThreadSafe.VFOAFreq * 1000000.0)));
					break;
				case 1:
					vfo = (long)(consoleThreadSafe.VFOBFreq * 1000000.0);
					break;
				}
				break;
			}
		}
		string sMsg = "vfo:" + rx + "," + chan + "," + vfo + ";";
		sendTextFrame(sMsg);
	}

	private void sendIF(int rx, int chan, int offset = -999999999)
	{
		if (offset == -999999999)
		{
			switch (rx)
			{
			case 0:
				offset = chan switch
				{
					0 => (int)consoleThreadSafe.radio.GetDSPRX(0, 0).RXOsc, 
					1 => (int)consoleThreadSafe.radio.GetDSPRX(0, 1).RXOsc, 
					_ => 0, 
				};
				break;
			case 1:
				offset = (int)consoleThreadSafe.radio.GetDSPRX(1, 0).RXOsc;
				break;
			}
		}
		offset += -consoleThreadSafe.GetDSPcwPitchShiftToZero(rx + 1);
		string sMsg = "if:" + rx + "," + chan + "," + offset + ";";
		sendTextFrame(sMsg);
	}

	private void sendMOX(int rx, bool mox, bool signalTCI = false)
	{
		string text = ((!signalTCI) ? "" : ",tci");
		string sMsg = "trx:" + rx + "," + mox.ToString().ToLower() + text + ";";
		sendTextFrame(sMsg);
	}

	private void sendAudioStartStop(int receiver, bool enable)
	{
		sendTextFrame((enable ? "audio_start:" : "audio_stop:") + receiver + ";");
	}

	private void sendMode(int rx, DSPMode mode = DSPMode.FIRST)
	{
		if (mode == DSPMode.FIRST)
		{
			switch (rx)
			{
			case 0:
				mode = consoleThreadSafe.RX1DSPMode;
				break;
			case 1:
				mode = consoleThreadSafe.RX2DSPMode;
				break;
			}
		}
		if (mode != DSPMode.FIRST && mode != DSPMode.LAST)
		{
			string text = ((m_server == null || !m_server.CWLUbecomesCW || (mode != DSPMode.CWL && mode != DSPMode.CWU)) ? mode.ToString().ToLower() : "cw");
			string sMsg = "modulation:" + rx + "," + text.ToUpper() + ";";
			sendTextFrame(sMsg);
		}
	}

	private void sendMute(bool mute)
	{
		string sMsg = "mute:" + mute.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendMuteRX(int rx, bool mute)
	{
		string sMsg = "rx_mute:" + rx + "," + mute.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendMONEnable(bool enable)
	{
		string sMsg = "mon_enable:" + enable.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendVolume(double volume)
	{
		if (!(volume < -60.0) && !(volume > 0.0))
		{
			string sMsg = "volume:" + volume.ToString("F1", CultureInfo.InvariantCulture).ToLower() + ";";
			sendTextFrame(sMsg);
		}
	}

	private void sendMONVolume(double volume)
	{
		if (!(volume < -60.0) && !(volume > 0.0))
		{
			string sMsg = "mon_volume:" + volume.ToString("F1", CultureInfo.InvariantCulture).ToLower() + ";";
			sendTextFrame(sMsg);
		}
	}

	private void sendRxBalance(int rx, int chan, double balance)
	{
		string sMsg = "rx_balance:" + rx + "," + chan + "," + balance.ToString("F2", CultureInfo.InvariantCulture) + ";";
		sendTextFrame(sMsg);
	}

	private void sendRxStepAttEx(int rx, int attenuation)
	{
		string sMsg = "rx_step_att_ex:" + rx + "," + Math.Abs(attenuation).ToString(CultureInfo.InvariantCulture) + ";";
		sendTextFrame(sMsg);
	}

	private void sendRxPreampAttEx(int rx, int attenuation)
	{
		string sMsg = "rx_preamp_att_ex:" + rx + "," + attenuation.ToString(CultureInfo.InvariantCulture) + ";";
		sendTextFrame(sMsg);
	}

	private void sendRxStepAttEnabledEx(int rx, bool enabled)
	{
		string sMsg = "rx_step_att_enabled_ex:" + rx + "," + enabled.ToString().ToLowerInvariant() + ";";
		sendTextFrame(sMsg);
	}

	private void sendVFOSyncEx(bool enabled)
	{
		string sMsg = "vfo_sync_ex:" + enabled.ToString().ToLowerInvariant() + ";";
		sendTextFrame(sMsg);
	}

	private void sendFMDeviationEx(int rx, int deviationHz)
	{
		string sMsg = "fm_deviation_ex:" + rx + "," + deviationHz.ToString(CultureInfo.InvariantCulture) + ";";
		sendTextFrame(sMsg);
	}

	private void sendAgcAutoEx(int rx, bool enabled)
	{
		string sMsg = "agc_auto_ex:" + rx + "," + enabled.ToString().ToLowerInvariant() + ";";
		sendTextFrame(sMsg);
	}

	private string agcModeToTciMode(AGCMode mode)
	{
		return mode switch
		{
			AGCMode.FIXD => "off", 
			AGCMode.LONG => "long", 
			AGCMode.SLOW => "slow", 
			AGCMode.FAST => "fast", 
			AGCMode.CUSTOM => "custom", 
			AGCMode.MED => "normal", 
			_ => "normal", 
		};
	}

	private AGCMode tciModeToAgcMode(string mode)
	{
		switch (mode.Trim().ToLowerInvariant())
		{
		case "off":
		case "fixd":
		case "fixed":
			return AGCMode.FIXD;
		case "long":
			return AGCMode.LONG;
		case "slow":
			return AGCMode.SLOW;
		case "fast":
			return AGCMode.FAST;
		case "custom":
			return AGCMode.CUSTOM;
		case "med":
		case "medium":
		case "normal":
			return AGCMode.MED;
		default:
			return AGCMode.MED;
		}
	}

	private void sendAgcMode(int rx, AGCMode mode)
	{
		string sMsg = "agc_mode:" + rx + "," + agcModeToTciMode(mode) + ";";
		sendTextFrame(sMsg);
	}

	private void sendAgcGain(int rx, int gain)
	{
		string sMsg = "agc_gain:" + rx + "," + gain + ";";
		sendTextFrame(sMsg);
	}

	private void sendTXFrequencyChanged(long new_frequency, Band new_band, bool rx2_enabled, bool tx_vfob)
	{
		string text = $"tx_frequency:{new_frequency};";
		sendTextFrame(text.ToLower());
		text = $"tx_frequency_ex:{new_frequency},{new_band.ToString()},{rx2_enabled.ToString()},{tx_vfob.ToString()};";
		sendTextFrame(text.ToLower());
	}

	private void sendTunePower(int rx, int drive)
	{
		if (drive >= 0 && drive <= 100)
		{
			string sMsg = "tune_drive:" + rx + "," + drive.ToString().ToLower() + ";";
			sendTextFrame(sMsg);
		}
	}

	private void sendDrivePower(int rx, int drive)
	{
		if (drive >= 0 && drive <= 100)
		{
			string sMsg = "drive:" + rx + "," + drive.ToString().ToLower() + ";";
			sendTextFrame(sMsg);
		}
	}

	private void sendTune(int rx, bool tune)
	{
		string sMsg = "tune:" + rx + "," + tune.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendRXEnable(int rx, bool enable)
	{
		string sMsg = "rx_enable:" + rx + "," + enable.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendTXEnable(int rx, bool bEnable)
	{
		string sMsg = "tx_enable:" + rx + "," + bEnable.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendVFOLimits(int low, int high)
	{
		string sMsg = "vfo_limits:" + low + "," + high + ";";
		sendTextFrame(sMsg);
	}

	private void sendAppFocus(bool focus)
	{
		string sMsg = "app_focus:" + focus.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void sendIFLimits(int low, int high)
	{
		string sMsg = "if_limits:" + low + "," + high + ";";
		sendTextFrame(sMsg);
	}

	private void sendClickedOnSpot(string callsign, long frequency)
	{
		string sMsg = "clicked_on_spot:" + callsign.Trim() + "," + frequency + ";";
		sendTextFrame(sMsg);
	}

	private void sendClickedOnSpotRX(int rx, int chan, string callsign, long frequency)
	{
		string sMsg = "rx_clicked_on_spot:" + rx + "," + chan + "," + callsign.Trim() + "," + frequency + ";";
		sendTextFrame(sMsg);
	}

	private void sendRxSensors(int rx, double levelDbm)
	{
		sendTextFrame("rx_sensors:" + rx + "," + levelDbm.ToString("F1", CultureInfo.InvariantCulture) + ";");
	}

	private void sendRxChannelSensors(int rx, int channel, double levelDbm, double avgLevelDbm, double peakBinDbm)
	{
		sendTextFrame("rx_channel_sensors:" + rx + "," + channel + "," + levelDbm.ToString("F1", CultureInfo.InvariantCulture) + ";");
		sendTextFrame("rx_channel_sensors_ex:" + rx + "," + channel + "," + levelDbm.ToString("F1", CultureInfo.InvariantCulture) + "," + avgLevelDbm.ToString("F1", CultureInfo.InvariantCulture) + "," + peakBinDbm.ToString("F1", CultureInfo.InvariantCulture) + ";");
	}

	private void sendTxSensors(int rx, double micLevelDbm, double rmsPowerWatts, double peakPowerWatts, double swr)
	{
		string sMsg = $"tx_sensors:{rx},{micLevelDbm:F1},{rmsPowerWatts:F1},{peakPowerWatts:F1},{swr:F1};";
		sendTextFrame(sMsg);
	}

	private void sendDDS(int rx, long ddsFreq = -1L)
	{
		if (ddsFreq == -1)
		{
			switch (rx)
			{
			case 0:
				ddsFreq = (long)(consoleThreadSafe.CentreFrequency * 1000000.0);
				break;
			case 1:
				ddsFreq = (long)(consoleThreadSafe.CentreRX2Frequency * 1000000.0);
				break;
			}
		}
		ddsFreq += consoleThreadSafe.GetDSPcwPitchShiftToZero(rx + 1);
		string sMsg = "dds:" + rx + "," + ddsFreq + ";";
		sendTextFrame(sMsg);
	}

	private void sendFilterBand(int rx, int low, int high)
	{
		string sMsg = "rx_filter_band:" + rx + "," + low + "," + high + ";";
		sendTextFrame(sMsg);
	}

	private void normalizeTXFilterBandForSet(ref int low, ref int high)
	{
		low = Math.Max(0, low);
		high = Math.Max(0, high);
		if (low > high)
		{
			int num = low;
			low = high;
			high = num;
		}
		if (high < low + 100)
		{
			high = low + 100;
		}
	}

	private void normalizeTXFilterBandForSend(ref int low, ref int high)
	{
		low = Math.Abs(low);
		high = Math.Abs(high);
		normalizeTXFilterBandForSet(ref low, ref high);
	}

	private void sendTXFilterBandEx(int low, int high)
	{
		normalizeTXFilterBandForSend(ref low, ref high);
		string sMsg = "tx_filter_band_ex:" + low + "," + high + ";";
		sendTextFrame(sMsg);
	}

	private void sendStartStop(bool bPower)
	{
		if (bPower)
		{
			sendStart();
		}
		else
		{
			sendStop();
		}
	}

	private int preampModeToAttenuation(PreampMode mode)
	{
		return mode switch
		{
			PreampMode.HPSDR_OFF => 20, 
			PreampMode.HPSDR_ON => 0, 
			PreampMode.HPSDR_MINUS10 => 10, 
			PreampMode.HPSDR_MINUS20 => 20, 
			PreampMode.HPSDR_MINUS30 => 30, 
			PreampMode.HPSDR_MINUS40 => 40, 
			PreampMode.HPSDR_MINUS50 => 50, 
			PreampMode.SA_MINUS10 => 10, 
			PreampMode.SA_MINUS20 => -20, 
			PreampMode.SA_MINUS30 => 30, 
			_ => 0, 
		};
	}

	private void sendInitialRadioState()
	{
		bool num = m_server == null || m_server.SendInitialFrequencyStateOnConnect;
		bool rX2Enabled = consoleThreadSafe.RX2Enabled;
		if (num)
		{
			sendDDS(0, -1L);
			sendDDS(1, -1L);
			sendIF(0, 0);
			sendIF(0, 1);
			sendIF(1, 1);
			sendIF(1, 1);
			sendVFO(0, 0, -1L);
			sendVFO(0, 1, -1L);
			sendVFO(1, 0, -1L);
			sendVFO(1, 1, -1L);
			sendTXFrequencyChanged((long)(consoleThreadSafe.TXFreq * 1000000.0), consoleThreadSafe.TXBand, consoleThreadSafe.RX2Enabled, consoleThreadSafe.VFOBTX);
		}
		sendMode(0);
		sendMode(1);
		sendFilterBand(0, consoleThreadSafe.RX1FilterLow, consoleThreadSafe.RX1FilterHigh);
		sendFilterBand(1, consoleThreadSafe.RX2FilterLow, consoleThreadSafe.RX2FilterHigh);
		sendTXFilterBandEx(consoleThreadSafe.TXFilterLow, consoleThreadSafe.TXFilterHigh);
		sendRXEnable(0, !consoleThreadSafe.MOX);
		sendRXEnable(1, rX2Enabled && !consoleThreadSafe.MOX);
		for (int i = 1; i <= 2; i++)
		{
			int selectedNR = consoleThreadSafe.GetSelectedNR(i);
			int rx = i - 1;
			sendNREnable(rx, selectedNR > 0, is_extended: false, selectedNR);
			sendNREnable(rx, selectedNR > 0, is_extended: true, selectedNR);
		}
		for (int j = 1; j <= 2; j++)
		{
			int selectedNB = consoleThreadSafe.GetSelectedNB(j);
			int rx2 = j - 1;
			sendNBEnable(rx2, selectedNB > 0, is_extended: false, selectedNB);
			sendNBEnable(rx2, selectedNB > 0, is_extended: true, selectedNB);
		}
		sendRxBinEnable(0, consoleThreadSafe.GetBin(1));
		sendRxBinEnable(1, consoleThreadSafe.GetBin(2));
		sendAnfEnable(0, consoleThreadSafe.GetANF(1));
		sendAnfEnable(1, consoleThreadSafe.GetANF(2));
		if (!consoleThreadSafe.IsSetupFormNull)
		{
			sendRxApfEnable(0, consoleThreadSafe.SetupForm.RX1APFEnable);
			sendRxApfEnable(1, consoleThreadSafe.SetupForm.RX2APFEnable);
		}
		sendRxNfEnable(0, consoleThreadSafe.GetMNF(1));
		sendRxNfEnable(1, consoleThreadSafe.GetMNF(2));
		double volume = audioGainToDb((float)consoleThreadSafe.RX0Gain / 100f);
		double volume2 = audioGainToDb((float)consoleThreadSafe.RX1Gain / 100f);
		double volume3 = audioGainToDb((float)consoleThreadSafe.RX2Gain / 100f);
		sendRxVolume(0, 0, volume);
		sendRxVolume(0, 1, volume2);
		sendRxVolume(1, 0, volume3);
		sendRxVolume(1, 1, volume3);
		sendRxBalance(0, 0, 40.0 - (double)consoleThreadSafe.GetBal(1) * 0.8);
		sendRxBalance(0, 1, 40.0 - (double)consoleThreadSafe.GetBal(1, subrx: true) * 0.8);
		sendRxBalance(1, 0, 40.0 - (double)consoleThreadSafe.GetBal(2) * 0.8);
		sendRxBalance(1, 1, 40.0 - (double)consoleThreadSafe.GetBal(2, subrx: true) * 0.8);
		sendVFOSyncEx(consoleThreadSafe.VFOSync);
		sendRxStepAttEnabledEx(0, consoleThreadSafe.RX1StepAttEnabled);
		sendRxStepAttEnabledEx(1, consoleThreadSafe.RX2StepAttEnabled);
		sendRxStepAttEx(0, consoleThreadSafe.RX1AttenuatorData);
		sendRxStepAttEx(1, consoleThreadSafe.RX2AttenuatorData);
		sendRxPreampAttEx(0, -preampModeToAttenuation(consoleThreadSafe.RX1PreampMode));
		sendRxPreampAttEx(1, -preampModeToAttenuation(consoleThreadSafe.RX2PreampMode));
		sendAgcMode(0, consoleThreadSafe.GetAGCMode(1));
		sendAgcMode(1, consoleThreadSafe.GetAGCMode(2));
		sendAgcGain(0, consoleThreadSafe.GetAgcT(1));
		sendAgcGain(1, consoleThreadSafe.GetAgcT(2));
		sendAgcAutoEx(0, consoleThreadSafe.GetAGCAuto(1));
		sendAgcAutoEx(1, consoleThreadSafe.GetAGCAuto(2));
		sendFMDeviationEx(0, consoleThreadSafe.FMDeviation_Hz);
		sendFMDeviationEx(1, consoleThreadSafe.FMDeviation_Hz);
		sendCTUN(0, consoleThreadSafe.GetCTUN(1));
		sendCTUN(1, consoleThreadSafe.GetCTUN(2));
		sendTXProfiles();
		sendTXProfile(consoleThreadSafe.TXProfile);
		CalibrationChanged(0);
		CalibrationChanged(1);
		sendRITEnable(0, consoleThreadSafe.RITOn);
		sendRITEnable(1, consoleThreadSafe.RITOn);
		sendXITEnable(0, consoleThreadSafe.XITOn);
		sendXITEnable(1, consoleThreadSafe.XITOn);
		sendRITOffset(0, consoleThreadSafe.RITValue);
		sendRITOffset(1, consoleThreadSafe.RITValue);
		sendXITOffset(0, consoleThreadSafe.XITValue);
		sendXITOffset(1, consoleThreadSafe.XITValue);
		sendLock(0, consoleThreadSafe.VFOALock);
		if (rX2Enabled)
		{
			sendLock(1, consoleThreadSafe.VFOBLock);
		}
		sendAllVFOLocks();
		sendSqlEnable(0, consoleThreadSafe.GetSqlMode(1) != SquelchState.OFF);
		sendSqlEnable(1, consoleThreadSafe.GetSqlMode(2) != SquelchState.OFF);
		sendSqlLevel(0, consoleThreadSafe.GetSql(1));
		sendSqlLevel(1, consoleThreadSafe.GetSql(2));
		sendDiglOffset(consoleThreadSafe.DIGLClickTuneOffset);
		sendDiguOffset(consoleThreadSafe.DIGUClickTuneOffset);
		if (m_server != null)
		{
			sendCwMacrosSpeed(m_server.GetCwMacrosSpeed());
			sendCwMacrosDelay(m_server.GetCwMacrosDelay());
			sendCwKeyerSpeed(m_server.GetCwKeyerSpeed());
		}
		sendSplit(0, consoleThreadSafe.VFOSplit);
		sendSplit(1, rX2Enabled && consoleThreadSafe.VFOSplit);
		sendTXEnable(0, !consoleThreadSafe.MOX);
		sendTXEnable(1, rX2Enabled && !consoleThreadSafe.MOX);
		sendRxChannelEnable(0, 0, enabled: true);
		sendRxChannelEnable(0, 1, consoleThreadSafe.GetSubRX(1));
		sendRxChannelEnable(1, 0, rX2Enabled);
		sendRxChannelEnable(1, 1, enabled: false);
		handleDrive(new string[1] { "0" });
		handleDrive(new string[1] { "1" });
		handleTuneDrive(new string[1] { "0" });
		handleTuneDrive(new string[1] { "1" });
		sendMOX(0, consoleThreadSafe.MOX && !(consoleThreadSafe.VFOBTX & rX2Enabled));
		sendMOX(1, consoleThreadSafe.MOX && (consoleThreadSafe.VFOBTX & rX2Enabled));
		sendTune(0, consoleThreadSafe.TUN && !(consoleThreadSafe.VFOBTX & rX2Enabled));
		sendTune(1, consoleThreadSafe.TUN && (consoleThreadSafe.VFOBTX & rX2Enabled));
		sendIQStartStop(0, enable: false);
		sendIQStartStop(1, enable: false);
		sendIQSampleRate(getPublishedIQSampleRate());
		sendAudioSampleRate(m_audioSampleRate);
		sendAudioStreamSampleType(m_audioSampleType);
		sendAudioStreamChannels(m_audioStreamChannels);
		sendAudioStreamSamples(m_audioStreamSamples);
		sendTxStreamAudioBuffering(m_txStreamAudioBufferingMs);
		sendMute(consoleThreadSafe.MUT || (consoleThreadSafe.MUT2 & rX2Enabled));
		sendMuteRX(0, consoleThreadSafe.MUT);
		sendMuteRX(1, consoleThreadSafe.MUT2);
		sendVolume(linearToDbVolume(consoleThreadSafe.AF));
		sendMONEnable(consoleThreadSafe.MON);
		sendMONVolume(linearToDbVolume(consoleThreadSafe.TXAF));
		sendStartStop(consoleThreadSafe.PowerOn);
	}

	private void sendInitialisationData()
	{
		string text = ((m_server == null || !m_server.EmulateExpertSDR3Protocol) ? "Thetis" : "ExpertSDR3");
		sendTextFrame("protocol:" + text + ",2.0;");
		string text2 = ((m_server == null || !m_server.EmulateSunSDR2Pro) ? HardwareSpecific.Model.ToString() : "SunSDR2PRO");
		sendTextFrame("device:" + text2 + ";");
		sendTextFrame("receive_only:false;");
		sendTextFrame("trx_count:2;");
		sendTextFrame("channels_count:2;");
		sendVFOLimits(0, (int)(consoleThreadSafe.MaxFreq * 1000000.0));
		int num = consoleThreadSafe.SampleRateRX1 / 2;
		sendIFLimits(-num, num);
		string text3 = ((m_server == null) ? "cwl,cwu" : (m_server.CWLUbecomesCW ? "cwl,cwu,cw" : "cwl,cwu"));
		sendTextFrame("modulations_list:" + ("am,sam,dsb,lsb,usb,nfm,fm,digl,digu," + text3).ToUpper() + ";");
		sendInitialRadioState();
		sendTextFrame("ready;");
		m_server?.RefreshStreamRunState();
	}

	private void setRxSensorsEnabled(bool enabled, int intervalMs, bool fireImmediately)
	{
		m_sensorManager.ConfigureRxSensors(enabled, intervalMs);
		if (m_tmRxSensors != null)
		{
			m_tmRxSensors.Change(-1, -1);
			m_tmRxSensors.Dispose();
			m_tmRxSensors = null;
		}
		if (enabled)
		{
			m_tmRxSensors = new System.Threading.Timer(RxSensorsTimerCallback, null, (!fireImmediately) ? m_sensorManager.RxIntervalMs : 0, m_sensorManager.RxIntervalMs);
		}
	}

	private void setTxSensorsEnabled(bool enabled, int intervalMs, bool fireImmediately)
	{
		m_sensorManager.ConfigureTxSensors(enabled, intervalMs);
		if (m_tmTxSensors != null)
		{
			m_tmTxSensors.Change(-1, -1);
			m_tmTxSensors.Dispose();
			m_tmTxSensors = null;
		}
		if (enabled)
		{
			m_tmTxSensors = new System.Threading.Timer(TxSensorsTimerCallback, null, (!fireImmediately) ? m_sensorManager.TxIntervalMs : 0, m_sensorManager.TxIntervalMs);
		}
	}

	private void RxSensorsTimerCallback(object state)
	{
		if (!m_stopClient && !m_disconnected)
		{
			bool flag = false;
			bool flag2 = false;
			try
			{
				flag = consoleThreadSafe.RX2Enabled;
				flag2 = !flag && consoleThreadSafe.GetSubRX(1);
			}
			catch
			{
			}
			if (m_sensorManager.TryGetRxChannelReadingForSend(0, 0, out var signal, out var avg_signal, out var peak_bin_signal))
			{
				sendRxSensors(0, signal);
				sendRxChannelSensors(0, 0, signal, avg_signal, peak_bin_signal);
				m_sensorManager.ConsumeRxChannelReading(0, 0);
			}
			if (flag2 && m_sensorManager.TryGetRxChannelReadingForSend(0, 1, out var signal2, out var avg_signal2, out var peak_bin_signal2))
			{
				sendRxChannelSensors(0, 1, signal2, avg_signal2, peak_bin_signal2);
				m_sensorManager.ConsumeRxChannelReading(0, 1);
			}
			if (flag && m_sensorManager.TryGetRxChannelReadingForSend(1, 0, out var signal3, out var avg_signal3, out var peak_bin_signal3))
			{
				sendRxSensors(1, signal3);
				sendRxChannelSensors(1, 0, signal3, avg_signal3, peak_bin_signal3);
				m_sensorManager.ConsumeRxChannelReading(1, 0);
			}
		}
	}

	private void TxSensorsTimerCallback(object state)
	{
		if (!m_stopClient && !m_disconnected && m_sensorManager.TryGetTxReadingsForSend(out var micLevelDbm, out var powerWatts, out var peakPowerWatts, out var swr))
		{
			sendTxSensors(0, micLevelDbm, powerWatts, peakPowerWatts, swr);
			sendTxSensors(1, micLevelDbm, powerWatts, peakPowerWatts, swr);
			m_sensorManager.ConsumeTxReadings();
		}
	}

	private int findEndOfHeader(byte[] bytes)
	{
		int result = 0;
		for (int i = 0; i <= bytes.Length - 4; i++)
		{
			if (bytes[i] == 13 && bytes[i + 1] == 10 && bytes[i + 2] == 13 && bytes[i + 3] == 10)
			{
				result = i + 4;
				break;
			}
		}
		return result;
	}

	private void SocketListenerThreadStart()
	{
		System.Threading.Timer timer = new System.Threading.Timer(PingFrameTimer, null, 20000, 20000);
		byte[] array = new byte[8192];
		ClientConnectedHandlers?.Invoke();
		while (!m_stopClient)
		{
			try
			{
				if (m_stream == null || m_client == null)
				{
					m_stopClient = true;
					continue;
				}
				Socket client = m_client.Client;
				if (client == null)
				{
					m_stopClient = true;
				}
				else
				{
					if (!client.Poll(250000, SelectMode.SelectRead))
					{
						continue;
					}
					if (client.Available < 1)
					{
						m_stopClient = true;
						continue;
					}
					int num = m_stream.Read(array, 0, array.Length);
					if (num < 1)
					{
						m_stopClient = true;
						continue;
					}
					_m_buffer.AddRange(array.Take(num));
					if (!m_bWebSocket)
					{
						byte[] bytes = _m_buffer.ToArray();
						int num2 = findEndOfHeader(bytes);
						if (num2 > 0)
						{
							string text = Encoding.UTF8.GetString(bytes, 0, num2);
							if (Regex.IsMatch(text, "^GET", RegexOptions.IgnoreCase))
							{
								if (upgradeToWebSocket(text))
								{
									m_bWebSocket = true;
									_m_buffer.RemoveRange(0, num2);
									sendInitialisationData();
								}
								else
								{
									m_stopClient = true;
								}
							}
						}
					}
					if (m_bWebSocket)
					{
						byte[] array2 = _m_buffer.ToArray();
						int frameLength = GetFrameLength(array2);
						while (!m_stopClient && frameLength > -1 && array2.Length >= frameLength)
						{
							_m_buffer.RemoveRange(0, frameLength);
							ParseReceiveBuffer(array2);
							array2 = _m_buffer.ToArray();
							frameLength = GetFrameLength(array2);
						}
					}
					continue;
				}
			}
			catch (IOException ex) when (isSocketReadTimeout(ex))
			{
			}
			catch (SocketException se)
			{
				m_stopClient = true;
				ClientErrorHandlers?.Invoke(se);
			}
			catch
			{
				m_stopClient = true;
			}
		}
		m_markedForDeletion = true;
		timer.Change(-1, -1);
		timer = null;
		m_disconnected = true;
		notifyServerDisconnected();
		ClientDisconnectedHandlers?.Invoke();
	}

	private void notifyServerDisconnected(TCPIPtciServer server = null)
	{
		if (Interlocked.Exchange(ref m_disconnectNotified, 1) != 0)
		{
			return;
		}
		try
		{
			((server != null) ? server : m_server)?.OnSocketListenerDisconnected(this);
		}
		catch
		{
		}
	}

	private void sendPingFrame(string sMsg)
	{
		try
		{
			if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null && m_client.Connected)
			{
				enqueueOutboundFrame(getFrameFromString(sMsg, EOpcodeType.Ping), null, TCIOutboundPriority.Urgent);
			}
		}
		catch
		{
			m_stopClient = true;
		}
	}

	private void sendPongFrame(string sMsg)
	{
		try
		{
			if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null && m_client.Connected)
			{
				enqueueOutboundFrame(getFrameFromString(sMsg, EOpcodeType.Pong), null, TCIOutboundPriority.Urgent);
			}
		}
		catch
		{
			m_stopClient = true;
		}
	}

	private void sendTextFrame(string sMsg)
	{
		try
		{
			if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null && m_client.Connected)
			{
				enqueueOutboundFrame(getFrameFromString(sMsg), sMsg, TCIOutboundPriority.Control, getCoalescedTextFrameKey(sMsg));
			}
		}
		catch (Exception)
		{
			m_stopClient = true;
		}
	}

	private void sendBinaryFrame(byte[] payload)
	{
		try
		{
			if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null && m_client.Connected)
			{
				enqueueOutboundFrame(GetFrameFromBytes(payload), null, TCIOutboundPriority.Binary);
			}
		}
		catch
		{
			m_stopClient = true;
		}
	}

	private void sendCloseFrame()
	{
		try
		{
			if (!m_stopClient && m_bWebSocket && m_client != null && m_stream != null && m_client.Connected)
			{
				enqueueOutboundFrame(getFrameFromString("", EOpcodeType.ClosedConnection), null, TCIOutboundPriority.Urgent);
			}
		}
		catch
		{
		}
	}

	public void StopSocketListener()
	{
		TCPIPtciServer server = m_server;
		notifyServerDisconnected(server);
		lock (m_objStreamLock)
		{
			m_txUsesTCIAudio = false;
			m_tciPttActive = false;
		}
		clearQueuedTxAudio();
		clearRxAudioStreamState();
		server?.ReleaseActiveTxAudioListener(this);
		server?.RefreshTxAudioSourceState();
		m_server?.RefreshStreamRunState();
		if (m_client == null)
		{
			return;
		}
		if (m_tmVFOtimer != null)
		{
			m_tmVFOtimer.Change(-1, -1);
			m_tmVFOtimer = null;
		}
		if (m_tmCentretimer != null)
		{
			m_tmCentretimer.Change(-1, -1);
			m_tmCentretimer = null;
		}
		if (m_tmTXFrequency != null)
		{
			m_tmTXFrequency.Change(-1, -1);
			m_tmTXFrequency = null;
		}
		if (m_tmRxSensors != null)
		{
			m_tmRxSensors.Change(-1, -1);
			m_tmRxSensors.Dispose();
			m_tmRxSensors = null;
		}
		if (m_tmTxSensors != null)
		{
			m_tmTxSensors.Change(-1, -1);
			m_tmTxSensors.Dispose();
			m_tmTxSensors = null;
		}
		if (m_stream != null)
		{
			sendStop();
			sendCloseFrame();
			flushOutboundFrames(100);
			m_stream.Close();
			m_stream = null;
		}
		m_stopClient = true;
		m_outboundFrameEvent.Set();
		m_client.Close();
		if (m_sendThread != null)
		{
			m_sendThread.Join(100);
			if (m_sendThread.IsAlive)
			{
				m_sendThread.Abort();
			}
			m_sendThread = null;
		}
		if (m_VFODataThread != null)
		{
			m_VFODataThread.Join(50);
			if (m_VFODataThread.IsAlive)
			{
				m_VFODataThread.Abort();
			}
			m_VFODataThread = null;
		}
		if (m_clientListenerThread != null)
		{
			m_clientListenerThread.Join(50);
			if (m_clientListenerThread.IsAlive)
			{
				m_clientListenerThread.Abort();
				m_disconnected = true;
				notifyServerDisconnected(server);
				ClientDisconnectedHandlers?.Invoke();
			}
			m_clientListenerThread = null;
		}
		m_server = null;
		m_client = null;
		clearOutboundFrames();
		m_markedForDeletion = true;
	}

	public bool IsMarkedForDeletion()
	{
		return m_markedForDeletion;
	}

	public bool IsDisconnected()
	{
		return m_disconnected;
	}

	private int GetFrameLength(byte[] bytes)
	{
		if (bytes.Length < 2)
		{
			return -1;
		}
		bool flag = (bytes[1] & 0x80) != 0;
		int num = bytes[1] & 0x7F;
		if (num <= 125)
		{
			if (!flag)
			{
				return num + 2;
			}
			return num + 6;
		}
		switch (num)
		{
		case 126:
		{
			if (bytes.Length < 4)
			{
				return -1;
			}
			int num3 = BitConverter.ToInt16(new byte[2]
			{
				bytes[3],
				bytes[2]
			}, 0);
			if (!flag)
			{
				return num3 + 4;
			}
			return num3 + 8;
		}
		case 127:
		{
			if (bytes.Length < 10)
			{
				return -1;
			}
			int num2 = (int)BitConverter.ToInt64(new byte[8]
			{
				bytes[9],
				bytes[8],
				bytes[7],
				bytes[6],
				bytes[5],
				bytes[4],
				bytes[3],
				bytes[2]
			}, 0);
			if (!flag)
			{
				return num2 + 10;
			}
			return num2 + 14;
		}
		default:
			return -1;
		}
	}

	private void ParseReceiveBuffer(byte[] bytes)
	{
		if (bytes.Length == 0)
		{
			return;
		}
		bool flag = (bytes[1] & 0x80) != 0;
		EOpcodeType eOpcodeType = (EOpcodeType)(bytes[0] & 0xF);
		if (eOpcodeType == EOpcodeType.ClosedConnection)
		{
			sendCloseFrame();
			m_stopClient = true;
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = bytes[1] & 0x7F;
		if (num3 <= 125)
		{
			num2 = 2;
			num = num3;
		}
		else
		{
			switch (num3)
			{
			case 126:
				num2 = 4;
				num = BitConverter.ToInt16(new byte[2]
				{
					bytes[3],
					bytes[2]
				}, 0);
				break;
			case 127:
				num2 = 10;
				num = (int)BitConverter.ToInt64(new byte[8]
				{
					bytes[9],
					bytes[8],
					bytes[7],
					bytes[6],
					bytes[5],
					bytes[4],
					bytes[3],
					bytes[2]
				}, 0);
				break;
			}
		}
		int num4 = num2;
		if (flag)
		{
			byte[] array = new byte[4]
			{
				bytes[num2],
				bytes[num2 + 1],
				bytes[num2 + 2],
				bytes[num2 + 3]
			};
			num4 += 4;
			for (int i = 0; i < num; i++)
			{
				bytes[num4 + i] = (byte)(bytes[num4 + i] ^ array[i % 4]);
			}
		}
		switch (eOpcodeType)
		{
		case EOpcodeType.Text:
			parseTextFrame(Encoding.UTF8.GetString(bytes, num4, num));
			break;
		case EOpcodeType.Ping:
			sendPongFrame("Thetis");
			break;
		case EOpcodeType.Binary:
		{
			byte[] array2 = new byte[num];
			Buffer.BlockCopy(bytes, num4, array2, 0, num);
			handleBinaryFrame(array2);
			break;
		}
		}
	}

	private void handleSetInFocus()
	{
		consoleThreadSafe.Focus();
	}

	private void handleStart()
	{
		if (!consoleThreadSafe.PowerOn)
		{
			consoleThreadSafe.PowerOn = true;
		}
	}

	private void handleStop()
	{
		if (consoleThreadSafe.PowerOn)
		{
			consoleThreadSafe.PowerOn = false;
		}
	}

	private void handleSpotClear()
	{
		SpotManager2.ClearAllSpots(non_swl: true, swl: false);
	}

	private void handleSplitEnableMessage(string[] args)
	{
		int result = 0;
		bool result2 = false;
		bool flag = int.TryParse(args[0], out result);
		if (args.Length == 2)
		{
			if (flag)
			{
				flag = bool.TryParse(args[1], out result2);
			}
			if (flag)
			{
				if (!consoleThreadSafe.IsSetupFormNull && consoleThreadSafe.SetupForm.SplitFromCATorTCIcancelsQSPLIT && consoleThreadSafe.SetupForm.QuickSplitEnabled)
				{
					consoleThreadSafe.SetupForm.QuickSplitEnabled = false;
				}
				if ((result == 0 || result == 1) && consoleThreadSafe.VFOSplit != result2)
				{
					consoleThreadSafe.VFOSplit = result2;
				}
			}
		}
		else if (args.Length == 1 && flag)
		{
			bool vFOSplit = consoleThreadSafe.VFOSplit;
			sendSplit(result, vFOSplit);
		}
	}

	private void handleRITEnableMessage(string[] args)
	{
		if (args.Length < 1 || args.Length > 2 || !int.TryParse(args[0], out var result))
		{
			return;
		}
		if (args.Length == 2)
		{
			if (bool.TryParse(args[1], out var result2) && (result == 0 || result == 1))
			{
				consoleThreadSafe.RITOn = result2;
			}
		}
		else
		{
			sendRITEnable(result, consoleThreadSafe.RITOn);
		}
	}

	private void handleXITEnableMessage(string[] args)
	{
		if (args.Length < 1 || args.Length > 2 || !int.TryParse(args[0], out var result))
		{
			return;
		}
		if (args.Length == 2)
		{
			if (bool.TryParse(args[1], out var result2) && (result == 0 || result == 1))
			{
				consoleThreadSafe.XITOn = result2;
			}
		}
		else
		{
			sendXITEnable(result, consoleThreadSafe.XITOn);
		}
	}

	private void handleRITOffsetMessage(string[] args)
	{
		if (args.Length < 1 || args.Length > 2 || !int.TryParse(args[0], out var result))
		{
			return;
		}
		if (args.Length == 2)
		{
			if (int.TryParse(args[1], out var result2) && (result == 0 || result == 1))
			{
				consoleThreadSafe.RITValue = result2;
			}
		}
		else
		{
			sendRITOffset(result, consoleThreadSafe.RITValue);
		}
	}

	private void handleXITOffsetMessage(string[] args)
	{
		if (args.Length < 1 || args.Length > 2 || !int.TryParse(args[0], out var result))
		{
			return;
		}
		if (args.Length == 2)
		{
			if (int.TryParse(args[1], out var result2) && (result == 0 || result == 1))
			{
				consoleThreadSafe.XITValue = result2;
			}
		}
		else
		{
			sendXITOffset(result, consoleThreadSafe.XITValue);
		}
	}

	private void handleRxBinEnable(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			bool result2;
			if (args.Length == 1)
			{
				sendRxBinEnable(result, consoleThreadSafe.GetBin(result + 1));
			}
			else if (bool.TryParse(args[1], out result2))
			{
				consoleThreadSafe.SetBin(result + 1, result2);
			}
		}
	}

	private void handleRxApfEnable(string[] args)
	{
		if (args == null || args.Length < 1 || args.Length > 2 || !int.TryParse(args[0], out var result) || result < 0 || result > 1 || consoleThreadSafe.IsSetupFormNull)
		{
			return;
		}
		bool result2;
		if (args.Length == 1)
		{
			bool enabled = ((result == 0) ? consoleThreadSafe.SetupForm.RX1APFEnable : consoleThreadSafe.SetupForm.RX2APFEnable);
			sendRxApfEnable(result, enabled);
		}
		else if (bool.TryParse(args[1], out result2))
		{
			if (result == 0)
			{
				consoleThreadSafe.SetupForm.RX1APFEnable = result2;
			}
			else
			{
				consoleThreadSafe.SetupForm.RX2APFEnable = result2;
			}
		}
	}

	private void handleRxNfEnable(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			bool result2;
			if (args.Length == 1)
			{
				sendRxNfEnable(result, consoleThreadSafe.GetMNF(result + 1));
			}
			else if (bool.TryParse(args[1], out result2))
			{
				consoleThreadSafe.TNFActive = result2;
			}
		}
	}

	private void handleLock(string[] args)
	{
		if (args == null || args.Length < 1 || args.Length > 2 || !int.TryParse(args[0], out var result) || result < 0 || result > 1)
		{
			return;
		}
		bool result2;
		if (args.Length == 1)
		{
			sendLock(result, (result == 0) ? consoleThreadSafe.VFOALock : consoleThreadSafe.VFOBLock);
		}
		else if (bool.TryParse(args[1], out result2))
		{
			if (result == 0)
			{
				consoleThreadSafe.VFOALock = result2;
			}
			else
			{
				consoleThreadSafe.VFOBLock = result2;
			}
		}
	}

	private void handleVFOLock(string[] args)
	{
		if (args == null || args.Length < 2 || args.Length > 3 || !int.TryParse(args[0], out var result) || !int.TryParse(args[1], out var result2))
		{
			return;
		}
		bool result3;
		if (args.Length == 2)
		{
			if (tryGetVFOLockState(result, result2, out var enabled))
			{
				sendVFOLock(result, result2, enabled);
			}
		}
		else if (bool.TryParse(args[2], out result3))
		{
			trySetVFOLockState(result, result2, result3);
		}
	}

	private void handleSqlEnable(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			bool result2;
			if (args.Length == 1)
			{
				sendSqlEnable(result, consoleThreadSafe.GetSqlMode(result + 1) != SquelchState.OFF);
			}
			else if (bool.TryParse(args[1], out result2))
			{
				consoleThreadSafe.SetSqlMode(result + 1, result2 ? SquelchState.SQL : SquelchState.OFF);
			}
		}
	}

	private void handleSqlLevel(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			int result2;
			if (args.Length == 1)
			{
				sendSqlLevel(result, consoleThreadSafe.GetSql(result + 1));
			}
			else if (int.TryParse(args[1], out result2))
			{
				result2 = Math.Max(-140, Math.Min(0, result2));
				consoleThreadSafe.SetSql(result + 1, result2);
			}
		}
	}

	private void handleDiglOffset(string[] args, bool hasArgs = true)
	{
		int result;
		if (!hasArgs || args == null || args.Length == 0)
		{
			sendDiglOffset(consoleThreadSafe.DIGLClickTuneOffset);
		}
		else if (int.TryParse(args[0], out result))
		{
			result = Math.Max(0, Math.Min(4000, result));
			consoleThreadSafe.DIGLClickTuneOffset = result;
		}
	}

	private void handleDiguOffset(string[] args, bool hasArgs = true)
	{
		int result;
		if (!hasArgs || args == null || args.Length == 0)
		{
			sendDiguOffset(consoleThreadSafe.DIGUClickTuneOffset);
		}
		else if (int.TryParse(args[0], out result))
		{
			result = Math.Max(0, Math.Min(4000, result));
			consoleThreadSafe.DIGUClickTuneOffset = result;
		}
	}

	private void handleCwMacrosSpeed(string[] args, bool hasArgs = true)
	{
		int result;
		if (!hasArgs || args == null || args.Length < 1)
		{
			if (m_server != null)
			{
				sendCwMacrosSpeed(m_server.GetCwMacrosSpeed());
			}
		}
		else if (int.TryParse(args[0], out result))
		{
			m_server?.SetCwMacrosSpeed(result);
		}
	}

	private void handleCwMacrosDelay(string[] args, bool hasArgs = true)
	{
		int result;
		if (!hasArgs || args == null || args.Length < 1)
		{
			if (m_server != null)
			{
				sendCwMacrosDelay(m_server.GetCwMacrosDelay());
			}
		}
		else if (int.TryParse(args[0], out result))
		{
			m_server?.SetCwMacrosDelay(result);
		}
	}

	private void handleCwKeyerSpeed(string[] args, bool hasArgs = true)
	{
		int result;
		if (!hasArgs || args == null || args.Length < 1)
		{
			if (m_server != null)
			{
				sendCwKeyerSpeed(m_server.GetCwKeyerSpeed());
			}
		}
		else if (int.TryParse(args[0], out result))
		{
			m_server?.SetCwKeyerSpeed(result);
		}
	}

	private void handleCwMacrosSpeedUp(string[] args)
	{
		if (args != null && args.Length == 1 && int.TryParse(args[0], out var result))
		{
			m_server?.IncreaseCwMacrosSpeed(result);
		}
	}

	private void handleCwMacrosSpeedDown(string[] args)
	{
		if (args != null && args.Length == 1 && int.TryParse(args[0], out var result))
		{
			m_server?.DecreaseCwMacrosSpeed(result);
		}
	}

	private void handleCwMacros(string[] args)
	{
		if (args != null && args.Length >= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			string text = string.Join(",", args.Skip(1).ToArray());
			m_server?.SendCwMacro(this, result, text);
		}
	}

	private void handleCwTerminal(string[] args)
	{
		if (args != null && args.Length == 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1 && bool.TryParse(args[1], out var result2))
		{
			m_server?.SetCwTerminalEnabled(this, result, result2);
		}
	}

	private void handleCwMsg(string[] args)
	{
		if (args != null && args.Length >= 1)
		{
			int result;
			if (args.Length == 1)
			{
				m_server?.UpdateCwMessageCallsign(this, args[0]);
			}
			else if (args.Length >= 4 && int.TryParse(args[0], out result) && result >= 0 && result <= 1)
			{
				string prefix = args[1];
				string callsign = args[2];
				string suffix = string.Join(",", args.Skip(3).ToArray());
				m_server?.SendCwMessage(this, result, prefix, callsign, suffix);
			}
		}
	}

	private void handleCwMacrosStop()
	{
		m_server?.StopCwMacros(this);
	}

	private void handleKeyer(string[] args)
	{
		if (args != null && args.Length >= 2 && args.Length <= 3 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1 && bool.TryParse(args[1], out var result2))
		{
			int result3 = 0;
			if (args.Length <= 2 || int.TryParse(args[2], out result3))
			{
				m_server?.HandleCwKeyer(this, result, result2, Math.Max(0, result3));
			}
		}
	}

	private void handleTrxMessage(string[] args)
	{
		int result = 0;
		bool result2 = false;
		bool flag = int.TryParse(args[0], out result);
		if (flag && args.Length > 1)
		{
			flag = bool.TryParse(args[1], out result2);
		}
		if (args.Length > 1)
		{
			if (flag && shouldIgnoreTrxForCurrentCwBreakIn())
			{
				bool tciPttActive;
				lock (m_objStreamLock)
				{
					tciPttActive = m_tciPttActive;
					m_txUsesTCIAudio = false;
					m_tciPttActive = false;
				}
				clearQueuedTxAudio();
				if (tciPttActive)
				{
					m_server?.ReleaseActiveTxAudioListener(this);
				}
				m_server?.RefreshTxAudioSourceState();
				m_server?.RefreshStreamRunState();
				return;
			}
			bool flag2 = args.Length > 2 && args[2].ToLower() == "tci";
			bool mOX = consoleThreadSafe.MOX;
			bool tciPttActive2;
			lock (m_objStreamLock)
			{
				tciPttActive2 = m_tciPttActive;
			}
			bool flag3 = (flag2 & flag & result2) && (!mOX | tciPttActive2);
			bool flag4 = false;
			if (flag3)
			{
				flag4 = m_server == null || m_server.TryAcquireActiveTxAudioListener(this);
			}
			else if (m_server != null)
			{
				m_server.ReleaseActiveTxAudioListener(this);
			}
			lock (m_objStreamLock)
			{
				m_txUsesTCIAudio = flag2;
				m_tciPttActive = flag3 & flag4;
			}
			if (!m_tciPttActive)
			{
				clearQueuedTxAudio();
			}
			if (flag)
			{
				if (result2 & mOX)
				{
					m_server?.RefreshTxAudioSourceState();
					m_server?.RefreshStreamRunState();
					return;
				}
				switch (result)
				{
				case 0:
					if (consoleThreadSafe.RX2Enabled && consoleThreadSafe.VFOBTX)
					{
						consoleThreadSafe.VFOATX = true;
					}
					if (consoleThreadSafe.MOX != result2)
					{
						consoleThreadSafe.TCIPTT = result2;
					}
					break;
				case 1:
					if (consoleThreadSafe.RX2Enabled)
					{
						if (!consoleThreadSafe.VFOBTX)
						{
							consoleThreadSafe.VFOBTX = true;
						}
						if (consoleThreadSafe.MOX != result2)
						{
							consoleThreadSafe.TCIPTT = result2;
						}
					}
					break;
				}
				if (!result2)
				{
					m_server?.NotifyCwTciPttReleased(this);
				}
			}
			m_server?.RefreshTxAudioSourceState();
			m_server?.RefreshStreamRunState();
		}
		else if (flag && args.Length == 1)
		{
			sendMOX(result, consoleThreadSafe.MOX, m_txUsesTCIAudio);
		}
	}

	private bool shouldIgnoreTrxForCurrentCwBreakIn()
	{
		if (consoleThreadSafe == null)
		{
			return false;
		}
		DSPMode dSPMode = ((consoleThreadSafe.RX2Enabled && consoleThreadSafe.VFOBTX) ? consoleThreadSafe.RX2DSPMode : consoleThreadSafe.RX1DSPMode);
		if (dSPMode != DSPMode.CWL && dSPMode != DSPMode.CWU)
		{
			return false;
		}
		BreakIn currentBreakInMode = consoleThreadSafe.CurrentBreakInMode;
		if (currentBreakInMode != BreakIn.QSK)
		{
			return currentBreakInMode == BreakIn.Manual;
		}
		return true;
	}

	private void handleIF(string[] args)
	{
		int result = 0;
		int result2 = 0;
		long result3 = 0L;
		bool flag = int.TryParse(args[0], out result);
		if (flag)
		{
			flag = int.TryParse(args[1], out result2);
		}
		if (args.Length == 3)
		{
			if (flag)
			{
				flag = long.TryParse(args[2], out result3);
			}
			if (!flag)
			{
				return;
			}
			double num = (double)result3 / 1000000.0;
			switch (result)
			{
			case 0:
			{
				double value = consoleThreadSafe.CentreFrequency + num;
				value = Math.Round(value, 6);
				switch (result2)
				{
				case 0:
					if (consoleThreadSafe.VFOAFreq != value)
					{
						consoleThreadSafe.VFOAFreq = value;
					}
					break;
				case 1:
					if (consoleThreadSafe.VFOBFreq != value)
					{
						consoleThreadSafe.VFOBFreq = value;
					}
					break;
				}
				break;
			}
			case 1:
			{
				if (!consoleThreadSafe.RX2Enabled)
				{
					break;
				}
				double value = consoleThreadSafe.CentreRX2Frequency + num;
				value = Math.Round(value, 6);
				switch (result2)
				{
				case 0:
					if (consoleThreadSafe.VFOBFreq != value)
					{
						consoleThreadSafe.VFOBFreq = value;
					}
					break;
				case 1:
					if (consoleThreadSafe.VFOBFreq != value)
					{
						consoleThreadSafe.VFOBFreq = value;
					}
					break;
				}
				break;
			}
			}
		}
		else
		{
			if (args.Length != 2)
			{
				return;
			}
			bool flag2 = m_server != null && consoleThreadSafe != null && consoleThreadSafe.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
			if (!flag)
			{
				return;
			}
			double num2 = 0.0;
			switch (result)
			{
			case 0:
				switch (result2)
				{
				case 0:
					num2 = consoleThreadSafe.VFOAFreq - consoleThreadSafe.CentreFrequency;
					break;
				case 1:
					num2 = consoleThreadSafe.VFOBFreq - consoleThreadSafe.CentreFrequency;
					break;
				}
				break;
			case 1:
				num2 = ((result2 != 0) ? (consoleThreadSafe.VFOBFreq - consoleThreadSafe.CentreRX2Frequency) : ((!flag2) ? (consoleThreadSafe.VFOBFreq - consoleThreadSafe.CentreRX2Frequency) : (consoleThreadSafe.VFOAFreq - consoleThreadSafe.CentreFrequency)));
				break;
			}
			num2 *= 1000000.0;
			sendIF(result, result2, (int)num2);
		}
	}

	private void handleDDS(string[] args)
	{
		int result = 0;
		long result2 = 0L;
		bool flag = int.TryParse(args[0], out result);
		if (args.Length == 2)
		{
			if (flag)
			{
				flag = long.TryParse(args[1], out result2);
			}
			if (flag)
			{
				double num = (double)result2 / 1000000.0;
				switch (result)
				{
				case 0:
				{
					double value2 = num - consoleThreadSafe.CentreFrequency;
					value2 = Math.Round(value2, 6);
					consoleThreadSafe.CentreFrequency = num;
					consoleThreadSafe.VFOAFreq += value2;
					break;
				}
				case 1:
				{
					double value = num - consoleThreadSafe.CentreRX2Frequency;
					value = Math.Round(value, 6);
					consoleThreadSafe.CentreRX2Frequency = num;
					consoleThreadSafe.VFOBFreq += value;
					break;
				}
				}
			}
		}
		else if (flag && args.Length == 1 && flag)
		{
			double num2 = 0.0;
			switch (result)
			{
			case 0:
				num2 = consoleThreadSafe.CentreFrequency;
				break;
			case 1:
				num2 = consoleThreadSafe.CentreRX2Frequency;
				break;
			}
			sendDDS(result, (long)(num2 * 1000000.0));
		}
	}

	private void handleVFOMessage(string[] args)
	{
		int result = 0;
		int result2 = 0;
		long result3 = 0L;
		bool flag = m_server != null && consoleThreadSafe != null && consoleThreadSafe.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
		bool flag2 = int.TryParse(args[0], out result);
		if (flag2)
		{
			flag2 = int.TryParse(args[1], out result2);
		}
		if (args.Length == 3)
		{
			if (flag2)
			{
				flag2 = long.TryParse(args[2], out result3);
			}
			if (!flag2)
			{
				return;
			}
			double value = (double)result3 / 1000000.0;
			value = Math.Round(value, 6);
			switch (result)
			{
			case 0:
				switch (result2)
				{
				case 0:
					if (consoleThreadSafe.VFOAFreq != value)
					{
						consoleThreadSafe.VFOAFreq = value;
					}
					break;
				case 1:
					if (consoleThreadSafe.VFOBFreq != value)
					{
						consoleThreadSafe.VFOBFreq = value;
					}
					break;
				}
				break;
			case 1:
				if (!consoleThreadSafe.RX2Enabled)
				{
					break;
				}
				switch (result2)
				{
				case 0:
					if (flag)
					{
						if (consoleThreadSafe.VFOAFreq != value)
						{
							consoleThreadSafe.VFOAFreq = value;
						}
					}
					else if (consoleThreadSafe.VFOBFreq != value)
					{
						consoleThreadSafe.VFOBFreq = value;
					}
					break;
				case 1:
					if (consoleThreadSafe.VFOBFreq != value)
					{
						consoleThreadSafe.VFOBFreq = value;
					}
					break;
				}
				break;
			}
		}
		else
		{
			if (args.Length != 2 || !flag2)
			{
				return;
			}
			double freqMHz = 0.0;
			switch (result)
			{
			case 0:
				switch (result2)
				{
				case 0:
					freqMHz = consoleThreadSafe.VFOAFreq;
					break;
				case 1:
					freqMHz = consoleThreadSafe.VFOBFreq;
					break;
				}
				break;
			case 1:
				freqMHz = ((result2 != 0) ? consoleThreadSafe.VFOBFreq : ((!flag) ? consoleThreadSafe.VFOBFreq : consoleThreadSafe.VFOAFreq));
				break;
			}
			VFOData vfod = new VFOData
			{
				cen = false,
				centreMHz = -1.0,
				rx = (flag ? 1 : result),
				freqMHz = freqMHz,
				offsetHz = -1,
				chan = result2,
				duplicate_tochan = -1,
				replace_if_duplicated = false,
				sendIF = false
			};
			VFOChange(vfod);
		}
	}

	private void handleModulationMessage(string[] args)
	{
		bool flag = int.TryParse(args[0], out var result);
		if (args.Length == 2)
		{
			if (!flag)
			{
				return;
			}
			DSPMode dSPMode;
			switch (args[1].ToLower())
			{
			case "lsb":
				dSPMode = DSPMode.LSB;
				break;
			case "usb":
				dSPMode = DSPMode.USB;
				break;
			case "dsb":
				dSPMode = DSPMode.DSB;
				break;
			case "am":
				dSPMode = DSPMode.AM;
				break;
			case "sam":
				dSPMode = DSPMode.SAM;
				break;
			case "nfm":
			case "fm":
				dSPMode = DSPMode.FM;
				break;
			case "cw":
			{
				bool flag2 = false;
				if (m_server != null && consoleThreadSafe != null && m_server.CWbecomesCWUabove10mhz)
				{
					bool flag3 = consoleThreadSafe.VFOAFreq >= 10.0;
					bool flag4 = consoleThreadSafe.VFOBFreq >= 10.0;
					switch (result)
					{
					case 0:
						flag2 = ((!consoleThreadSafe.VFOATX) ? flag4 : flag3);
						break;
					case 1:
						flag2 = ((!consoleThreadSafe.VFOBTX) ? flag3 : flag4);
						break;
					}
				}
				dSPMode = (flag2 ? DSPMode.CWU : DSPMode.CWL);
				break;
			}
			case "cwl":
				dSPMode = DSPMode.CWL;
				break;
			case "cwu":
				dSPMode = DSPMode.CWU;
				break;
			case "digl":
				dSPMode = DSPMode.DIGL;
				break;
			case "digu":
				dSPMode = DSPMode.DIGU;
				break;
			default:
				dSPMode = DSPMode.FIRST;
				break;
			}
			if (dSPMode == DSPMode.FIRST)
			{
				return;
			}
			switch (result)
			{
			case 0:
				if (consoleThreadSafe.RX1DSPMode != dSPMode)
				{
					consoleThreadSafe.RX1DSPMode = dSPMode;
				}
				break;
			case 1:
				if (consoleThreadSafe.RX2DSPMode != dSPMode)
				{
					consoleThreadSafe.RX2DSPMode = dSPMode;
				}
				break;
			}
		}
		else if (flag && args.Length == 1)
		{
			switch (result)
			{
			case 0:
				sendMode(result, consoleThreadSafe.RX1DSPMode);
				break;
			case 1:
				sendMode(result, consoleThreadSafe.RX2DSPMode);
				break;
			}
		}
	}

	private void handleDeleteSpot(string[] args)
	{
		if (args.Length == 1)
		{
			SpotManager2.DeleteSpot(args[0]);
		}
	}

	private void lineOutEnable(int vac_number, bool enable)
	{
		switch (vac_number)
		{
		case 0:
			consoleThreadSafe.Invoke((MethodInvoker)delegate
			{
				if (consoleThreadSafe.SetupForm.VACEnable != enable)
				{
					consoleThreadSafe.SetupForm.VACEnable = enable;
				}
			});
			break;
		case 1:
			consoleThreadSafe.Invoke((MethodInvoker)delegate
			{
				if (consoleThreadSafe.SetupForm.VAC2Enable != enable)
				{
					consoleThreadSafe.SetupForm.VAC2Enable = enable;
				}
			});
			break;
		}
	}

	private void handleLineOutStart(string[] args)
	{
		if (args.Length == 1 && int.TryParse(args[0], out var result) && !consoleThreadSafe.IsSetupFormNull)
		{
			lineOutEnable(result, enable: true);
		}
	}

	private void handleLineOutStop(string[] args)
	{
		if (args.Length == 1 && int.TryParse(args[0], out var result) && !consoleThreadSafe.IsSetupFormNull)
		{
			lineOutEnable(result, enable: false);
		}
	}

	private void handleDrive(string[] args)
	{
		if (args.Length < 1)
		{
			return;
		}
		bool flag = int.TryParse(args[0], out var result);
		if (flag && args.Length == 2)
		{
			if (int.TryParse(args[1], out var result2))
			{
				consoleThreadSafe.PWR = result2;
			}
		}
		else if (flag && args.Length == 1)
		{
			sendDrivePower(drive: (!consoleThreadSafe.SendLimitedPowerLevels) ? consoleThreadSafe.PWR : consoleThreadSafe.PWRConstrained, rx: result);
		}
	}

	private void handleTuneDrive(string[] args)
	{
		if (args.Length < 1)
		{
			return;
		}
		bool flag = int.TryParse(args[0], out var result);
		if (flag && args.Length == 2)
		{
			if (int.TryParse(args[1], out var result2))
			{
				switch (consoleThreadSafe.TuneDrivePowerOrigin)
				{
				case DrivePowerSource.DRIVE_SLIDER:
					consoleThreadSafe.PWR = result2;
					break;
				case DrivePowerSource.TUNE_SLIDER:
					consoleThreadSafe.TunePWR = result2;
					break;
				}
			}
		}
		else if (flag && args.Length == 1)
		{
			int drive = 0;
			switch (consoleThreadSafe.TuneDrivePowerOrigin)
			{
			case DrivePowerSource.DRIVE_SLIDER:
				drive = ((!consoleThreadSafe.SendLimitedPowerLevels) ? consoleThreadSafe.PWR : consoleThreadSafe.PWRConstrained);
				break;
			case DrivePowerSource.TUNE_SLIDER:
				drive = ((!consoleThreadSafe.SendLimitedPowerLevels) ? consoleThreadSafe.TunePWR : consoleThreadSafe.TunePWRConstrained);
				break;
			case DrivePowerSource.FIXED:
				drive = consoleThreadSafe.TunePower;
				break;
			}
			sendTunePower(result, drive);
		}
	}

	private void handleMute(string[] args, bool hasArgs = true)
	{
		if (hasArgs && args.Length == 1)
		{
			if (bool.TryParse(args[0], out var result))
			{
				consoleThreadSafe.MUT = result;
				consoleThreadSafe.MUT2 = result;
			}
		}
		else if (!hasArgs)
		{
			sendMute(consoleThreadSafe.MUT || consoleThreadSafe.MUT2);
		}
	}

	private void handleMuteRX(string[] args)
	{
		if (args.Length < 1)
		{
			return;
		}
		bool flag = int.TryParse(args[0], out var result);
		if (flag && args.Length == 2)
		{
			if (bool.TryParse(args[1], out var result2))
			{
				switch (result)
				{
				case 0:
					consoleThreadSafe.MUT = result2;
					break;
				case 1:
					consoleThreadSafe.MUT2 = result2;
					break;
				}
			}
		}
		else if (flag && args.Length == 1)
		{
			sendMuteRX(result, (result == 0) ? consoleThreadSafe.MUT : consoleThreadSafe.MUT2);
		}
	}

	private void handleMONEnable(string[] args, bool hasArgs = true)
	{
		if (hasArgs && args.Length == 1)
		{
			if (bool.TryParse(args[0], out var result))
			{
				consoleThreadSafe.MON = result;
			}
		}
		else if (!hasArgs)
		{
			sendMONEnable(consoleThreadSafe.MON);
		}
	}

	private double linearToDbVolume(int volume)
	{
		double num = -60.0;
		double num2 = 0.0;
		double num3 = 100.0;
		double num4 = 0.0;
		double val = ((double)volume - num4) / (num3 - num4) * (num2 - num) + num;
		return Math.Max(num, Math.Min(num2, val));
	}

	private int dbToLinearVolume(double dBLevel)
	{
		double num = -60.0;
		double num2 = 0.0;
		double num3 = 100.0;
		double num4 = 0.0;
		double val = (dBLevel - num) / (num2 - num) * (num3 - num4) + num4;
		val = Math.Max(num4, Math.Min(num3, val));
		return (int)val;
	}

	private void handleMONVolume(string[] args, bool hasArgs = true)
	{
		if (hasArgs && args.Length == 1)
		{
			if (double.TryParse(args[0], out var result))
			{
				consoleThreadSafe.TXAF = dbToLinearVolume(result);
			}
		}
		else if (!hasArgs)
		{
			sendMONVolume(linearToDbVolume(consoleThreadSafe.TXAF));
		}
	}

	private void handleVolume(string[] args, bool hasArgs = true)
	{
		if (hasArgs && args.Length == 1)
		{
			if (double.TryParse(args[0], out var result))
			{
				consoleThreadSafe.AF = dbToLinearVolume(result);
			}
		}
		else if (!hasArgs)
		{
			sendVolume(linearToDbVolume(consoleThreadSafe.AF));
		}
	}

	private void handleSpotSimulateClick(string[] args)
	{
		if (m_server != null && args.Length == 2)
		{
			string callsign = args[0];
			if (long.TryParse(args[1], out var result))
			{
				m_server.SendSpotSimulationClickToAll(callsign, result);
			}
		}
	}

	private void handleSpot(string[] args, bool is_json, string msg)
	{
		if (args.Length < 4)
		{
			return;
		}
		long result = 0L;
		uint result2 = 0u;
		DSPMode result3 = DSPMode.FIRST;
		bool flag = false;
		string text = "";
		if (!is_json)
		{
			for (int i = 4; i < args.Length; i++)
			{
				text = text + args[i] + ",";
			}
			if (text.EndsWith(","))
			{
				text = text.Substring(0, text.Length - 1);
			}
		}
		else
		{
			int num = msg.ToLower().IndexOf("[json]{");
			if (num <= -1)
			{
				return;
			}
			text = msg.Substring(num + 6);
			flag = true;
		}
		bool flag2 = long.TryParse(args[2], out result);
		if (flag2)
		{
			flag2 = uint.TryParse(args[3], out result2);
		}
		if (flag2)
		{
			flag2 = Enum.TryParse<DSPMode>(args[1].ToUpper(), out result3);
			if (!flag2)
			{
				bool flag3 = result >= 10000000 || (result >= 5300000 && result < 5410000);
				switch (SpotManager2.FilterForRawMode(args[1].ToLower()))
				{
				case "lsb":
					result3 = DSPMode.LSB;
					break;
				case "usb":
					result3 = DSPMode.USB;
					break;
				case "am":
					result3 = DSPMode.AM;
					break;
				case "nfm":
				case "fm":
					result3 = DSPMode.FM;
					break;
				case "dsb":
					result3 = DSPMode.DSB;
					break;
				case "drm":
					result3 = DSPMode.DRM;
					break;
				case "spec":
					result3 = DSPMode.SPEC;
					break;
				case "sam":
					result3 = DSPMode.SAM;
					break;
				case "cwl":
					result3 = DSPMode.CWL;
					break;
				case "cwu":
					result3 = DSPMode.CWU;
					break;
				case "digu":
					result3 = DSPMode.DIGU;
					break;
				case "digl":
					result3 = DSPMode.DIGL;
					break;
				case "ssb":
					result3 = (flag3 ? DSPMode.USB : DSPMode.LSB);
					break;
				case "cw":
					result3 = m_server.CWSpotForce switch
					{
						TCICWSpotForce.CWU => DSPMode.CWU, 
						TCICWSpotForce.CWL => DSPMode.CWL, 
						_ => (!flag3) ? DSPMode.CWL : DSPMode.CWU, 
					};
					break;
				case "psk":
				case "jt9":
				case "fsk":
				case "rtty":
				case "jt65":
				case "mt63":
				case "domi":
				case "sstv":
				case "contesa":
				case "packtor":
				case "olivia":
					result3 = ((!flag3) ? DSPMode.DIGL : DSPMode.DIGU);
					break;
				case "ft8":
					result3 = DSPMode.DIGU;
					break;
				default:
					result3 = DSPMode.FIRST;
					break;
				}
				flag2 = true;
			}
		}
		if (!flag2)
		{
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		byte[] array = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);
		string text2 = Encoding.Unicode.GetString(array, 0, array.Length);
		if (text2.ToLower() == "nil")
		{
			text2 = "";
		}
		SpotManager2.JsonSpotData jsonSpotData = null;
		if (flag)
		{
			try
			{
				jsonSpotData = JsonConvert.DeserializeObject<SpotManager2.JsonSpotData>(text2);
			}
			catch
			{
				return;
			}
		}
		SpotManager2.AddSpot(args[0], result3, result, Color.FromArgb((int)result2), text2, jsonSpotData);
	}

	private void handleTune(string[] args)
	{
		int result = 0;
		bool result2 = false;
		if (args.Length == 2)
		{
			bool flag = int.TryParse(args[0], out result);
			if (flag)
			{
				flag = bool.TryParse(args[1], out result2);
			}
			if (flag && consoleThreadSafe.TUN != result2)
			{
				consoleThreadSafe.TUN = result2;
			}
		}
		else if (args.Length == 1)
		{
			sendTune(result, consoleThreadSafe.TUN);
		}
	}

	private void handleRxFilterBand(string[] args)
	{
		if (m_server == null)
		{
			return;
		}
		int result = 0;
		int result2 = 0;
		int result3 = 0;
		if (args.Length == 1)
		{
			int.TryParse(args[0], out result);
			switch (result)
			{
			case 0:
				result2 = consoleThreadSafe.RX1FilterLow;
				result3 = consoleThreadSafe.RX1FilterHigh;
				break;
			case 1:
				result2 = consoleThreadSafe.RX2FilterLow;
				result3 = consoleThreadSafe.RX2FilterHigh;
				break;
			}
			sendFilterBand(result, result2, result3);
		}
		else
		{
			if (args.Length != 3)
			{
				return;
			}
			bool flag = int.TryParse(args[0], out result);
			if (flag)
			{
				flag = int.TryParse(args[1], out result2);
			}
			if (flag)
			{
				flag = int.TryParse(args[2], out result3);
			}
			if (flag)
			{
				switch (result)
				{
				case 0:
					consoleThreadSafe.UpdateRX1Filters(result2, result3);
					break;
				case 1:
					consoleThreadSafe.UpdateRX2Filters(result2, result3);
					break;
				}
			}
		}
	}

	private void handleTXFilterBandEx(string[] args)
	{
		if (m_server == null || (args != null && args.Length != 0 && args.Length != 2))
		{
			return;
		}
		int result;
		int result2;
		if (args == null || args.Length == 0)
		{
			sendTXFilterBandEx(consoleThreadSafe.TXFilterLow, consoleThreadSafe.TXFilterHigh);
		}
		else if (int.TryParse(args[0], out result) && int.TryParse(args[1], out result2))
		{
			normalizeTXFilterBandForSet(ref result, ref result2);
			if (consoleThreadSafe.TXFilterLow != result)
			{
				consoleThreadSafe.TXFilterLow = result;
			}
			if (consoleThreadSafe.TXFilterHigh != result2)
			{
				consoleThreadSafe.TXFilterHigh = result2;
			}
		}
	}

	private void handleRXEnable(string[] args)
	{
		int result = 0;
		bool result2 = false;
		bool flag = int.TryParse(args[0], out result);
		if (result < 0 || result > 1)
		{
			return;
		}
		if (args.Length == 2)
		{
			if (flag)
			{
				flag = bool.TryParse(args[1], out result2);
			}
			if (flag && result == 1 && consoleThreadSafe.RX2Enabled != result2)
			{
				consoleThreadSafe.RX2Enabled = result2;
			}
		}
		else if (flag && args.Length == 1)
		{
			switch (result)
			{
			case 0:
				sendRXEnable(result, !consoleThreadSafe.MOX);
				break;
			case 1:
				sendRXEnable(result, consoleThreadSafe.RX2Enabled && !consoleThreadSafe.MOX);
				break;
			}
		}
	}

	private void handleRxSensorsEnable(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && bool.TryParse(args[0], out var result))
		{
			int result2 = m_sensorManager.RxIntervalMs;
			if (args.Length != 2 || int.TryParse(args[1], out result2))
			{
				setRxSensorsEnabled(result, result2, result);
			}
		}
	}

	private void handleTxSensorsEnable(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && bool.TryParse(args[0], out var result))
		{
			int result2 = m_sensorManager.TxIntervalMs;
			if (args.Length != 2 || int.TryParse(args[1], out result2))
			{
				setTxSensorsEnabled(result, result2, result);
			}
		}
	}

	private void sendNREnable(int rx, bool enabled, bool is_extended, int nr)
	{
		string sMsg = ((!is_extended) ? ("rx_nr_enable:" + rx + "," + enabled.ToString().ToLower() + ";") : ("rx_nr_enable_ex:" + rx + "," + enabled.ToString().ToLower() + "," + nr + ";"));
		sendTextFrame(sMsg);
	}

	private void sendNBEnable(int rx, bool enabled, bool is_extended, int nb)
	{
		string sMsg = ((!is_extended) ? ("rx_nb_enable:" + rx + "," + enabled.ToString().ToLower() + ";") : ("rx_nb_enable_ex:" + rx + "," + enabled.ToString().ToLower() + "," + nb + ";"));
		sendTextFrame(sMsg);
	}

	private void handleNREnable(string[] args, bool is_extended)
	{
		if (args == null || args.Length < 1 || (is_extended && args.Length < 3) || !int.TryParse(args[0], out var result) || result < 0 || result > 1)
		{
			return;
		}
		int result2 = 1;
		bool result3 = false;
		if (args.Length == 1)
		{
			result2 = consoleThreadSafe.GetSelectedNR(result + 1);
			result3 = result2 > 0;
			sendNREnable(result, result3, is_extended: false, result2);
			sendNREnable(result, result3, is_extended: true, result2);
		}
		else if (bool.TryParse(args[1], out result3) && (!is_extended || int.TryParse(args[2], out result2)) && result2 >= 0 && result2 <= 4)
		{
			if (result3)
			{
				consoleThreadSafe.SelectNR(result + 1, incude_sub: false, (!is_extended) ? 1 : result2);
			}
			else
			{
				consoleThreadSafe.SelectNR(result + 1, incude_sub: false, 0);
			}
		}
	}

	private void handleRxNBEnable(string[] args, bool is_extended)
	{
		if (args == null || args.Length < 1 || (is_extended && args.Length < 3) || !int.TryParse(args[0], out var result) || result < 0 || result > 1)
		{
			return;
		}
		int result2 = 1;
		bool result3 = false;
		if (args.Length == 1)
		{
			result2 = consoleThreadSafe.GetSelectedNB(result + 1);
			result3 = result2 > 0;
			sendNBEnable(result, result3, is_extended: false, result2);
			sendNBEnable(result, result3, is_extended: true, result2);
		}
		else if (bool.TryParse(args[1], out result3) && (!is_extended || int.TryParse(args[2], out result2)) && result2 >= 0 && result2 <= 2)
		{
			if (result3)
			{
				consoleThreadSafe.SetSelectedNB(result + 1, (!is_extended) ? 1 : result2);
			}
			else
			{
				consoleThreadSafe.SetSelectedNB(result + 1, 0);
			}
		}
	}

	private void sendAnfEnable(int rx, bool enabled)
	{
		string sMsg = "rx_anf_enable:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void handleAnfEnable(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			bool result2 = false;
			if (args.Length == 1)
			{
				result2 = consoleThreadSafe.GetANF(result + 1);
				sendAnfEnable(result, result2);
			}
			else if (bool.TryParse(args[1], out result2))
			{
				consoleThreadSafe.SetANF(result + 1, result2);
			}
		}
	}

	private double dbToAudioGain(double db)
	{
		if (db <= -60.0)
		{
			return 0.0;
		}
		if (db >= 0.0)
		{
			return 1.0;
		}
		return Math.Pow(10.0, db / 20.0);
	}

	private double audioGainToDb(double gain)
	{
		if (gain <= 0.0)
		{
			return -60.0;
		}
		if (gain >= 1.0)
		{
			return 0.0;
		}
		return 20.0 * Math.Log10(gain);
	}

	private void sendRxVolume(int rx, int chan, double volume)
	{
		string sMsg = "rx_volume:" + rx + "," + chan + "," + volume.ToString("F2", CultureInfo.InvariantCulture) + ";";
		sendTextFrame(sMsg);
	}

	private void handleRxVolume(string[] args)
	{
		if (args == null || args.Length < 2 || args.Length > 3 || !int.TryParse(args[0], out var result) || result < 0 || result > 1 || !int.TryParse(args[1], out var result2) || result2 < 0 || result2 > 1)
		{
			return;
		}
		int num = 0;
		if (args.Length == 2)
		{
			switch (result + 1)
			{
			case 1:
				switch (result2)
				{
				case 0:
					num = consoleThreadSafe.RX0Gain;
					break;
				case 1:
					num = consoleThreadSafe.RX1Gain;
					break;
				default:
					return;
				}
				break;
			case 2:
				num = consoleThreadSafe.RX2Gain;
				break;
			}
			double gain = (float)num / 100f;
			double volume = audioGainToDb(gain);
			sendRxVolume(result, result2, volume);
		}
		else
		{
			if (!double.TryParse(args[2], out var result3))
			{
				return;
			}
			double num2 = dbToAudioGain(result3) * 100.0;
			switch (result + 1)
			{
			case 1:
				switch (result2)
				{
				case 0:
					consoleThreadSafe.RX0Gain = (int)num2;
					break;
				case 1:
					consoleThreadSafe.RX1Gain = (int)num2;
					break;
				}
				break;
			case 2:
				consoleThreadSafe.RX2Gain = (int)num2;
				break;
			}
		}
	}

	private void handleRxBalance(string[] args)
	{
		if (args != null && args.Length >= 2 && args.Length <= 3 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1 && int.TryParse(args[1], out var result2) && result2 >= 0 && result2 <= 1)
		{
			bool subrx = result2 == 1;
			double result3;
			if (args.Length == 2)
			{
				int bal = consoleThreadSafe.GetBal(result + 1, subrx);
				double balance = 40.0 - (double)bal * 0.8;
				sendRxBalance(result, result2, balance);
			}
			else if (double.TryParse(args[2], out result3))
			{
				result3 = Math.Max(-40.0, Math.Min(40.0, result3));
				int val = (int)Math.Round((40.0 - result3) / 0.8, MidpointRounding.AwayFromZero);
				val = Math.Max(0, Math.Min(100, val));
				consoleThreadSafe.SetBal(result + 1, val, subrx);
			}
		}
	}

	private void handleRxStepAttEnabledEx(string[] args)
	{
		if (args == null || args.Length < 1 || args.Length > 2 || !int.TryParse(args[0], out var result) || result < 0 || result > 1)
		{
			return;
		}
		bool result2;
		if (args.Length == 1)
		{
			bool enabled = ((result == 0) ? consoleThreadSafe.RX1StepAttEnabled : consoleThreadSafe.RX2StepAttEnabled);
			sendRxStepAttEnabledEx(result, enabled);
		}
		else if (bool.TryParse(args[1], out result2) && consoleThreadSafe != null && !consoleThreadSafe.IsSetupFormNull)
		{
			if (result == 0)
			{
				consoleThreadSafe.SetupForm.RX1EnableAtt = result2;
			}
			else
			{
				consoleThreadSafe.SetupForm.RX2EnableAtt = result2;
			}
		}
	}

	private void handleRxStepAttEx(string[] args)
	{
		if (args == null || args.Length < 1 || args.Length > 2 || !int.TryParse(args[0], out var result) || result < 0 || result > 1)
		{
			return;
		}
		int result2;
		if (args.Length == 1)
		{
			int attenuation = ((result == 0) ? consoleThreadSafe.RX1AttenuatorData : consoleThreadSafe.RX2AttenuatorData);
			sendRxStepAttEx(result, attenuation);
		}
		else if (int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out result2) && result2 >= 0)
		{
			if (result == 0)
			{
				consoleThreadSafe.RX1AttenuatorData = result2;
			}
			else
			{
				consoleThreadSafe.RX2AttenuatorData = result2;
			}
		}
	}

	private void handleRxPreampAttEx(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			int result2;
			if (args.Length == 1)
			{
				PreampMode mode = ((result == 0) ? consoleThreadSafe.RX1PreampMode : consoleThreadSafe.RX2PreampMode);
				sendRxPreampAttEx(result, -preampModeToAttenuation(mode));
			}
			else if (int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out result2) && result2 <= 0)
			{
				consoleThreadSafe.SetATT(result + 1, Math.Abs(result2), Console.SetAttMode.PREAMP_MODE);
			}
		}
	}

	private void handleVfoSyncEx(string[] args)
	{
		bool result;
		if (args == null || args.Length == 0)
		{
			sendVFOSyncEx(consoleThreadSafe.VFOSync);
		}
		else if (args.Length == 1 && bool.TryParse(args[0], out result))
		{
			consoleThreadSafe.VFOSync = result;
		}
	}

	private void handleVfoSwapEx()
	{
		consoleThreadSafe.VFOSwap();
	}

	private void handleFMDeviationEx(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			int result2;
			if (args.Length == 1)
			{
				sendFMDeviationEx(result, consoleThreadSafe.FMDeviation_Hz);
			}
			else if (int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out result2) && (result2 == 2500 || result2 == 5000))
			{
				consoleThreadSafe.FMDeviation_Hz = result2;
			}
		}
	}

	private void handleAgcAutoEx(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			bool result2;
			if (args.Length == 1)
			{
				sendAgcAutoEx(result, consoleThreadSafe.GetAGCAuto(result + 1));
			}
			else if (bool.TryParse(args[1], out result2))
			{
				consoleThreadSafe.SetAGCAuto(result + 1, result2);
			}
		}
	}

	private void handleAgcMode(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			if (args.Length == 1)
			{
				sendAgcMode(result, consoleThreadSafe.GetAGCMode(result + 1));
			}
			else
			{
				consoleThreadSafe.SetAGCMode(result + 1, tciModeToAgcMode(args[1]));
			}
		}
	}

	private void handleAgcGain(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result) && result >= 0 && result <= 1)
		{
			int result2;
			if (args.Length == 1)
			{
				sendAgcGain(result, consoleThreadSafe.GetAgcT(result + 1));
			}
			else if (int.TryParse(args[1], out result2))
			{
				result2 = Math.Max(-20, Math.Min(120, result2));
				consoleThreadSafe.SetAgcT(result + 1, result2);
			}
		}
	}

	private void sendCTUN(int rx, bool enabled)
	{
		string sMsg = "rx_ctun_ex:" + rx + "," + enabled.ToString().ToLower() + ";";
		sendTextFrame(sMsg);
	}

	private void handleCTUN(string[] args)
	{
		if (args != null && args.Length >= 1 && args.Length <= 2 && int.TryParse(args[0], out var result))
		{
			bool result2 = false;
			if (args.Length == 1)
			{
				result2 = consoleThreadSafe.GetCTUN(result + 1);
				sendCTUN(result, result2);
			}
			else if (bool.TryParse(args[1], out result2))
			{
				consoleThreadSafe.SetCTUN(result + 1, result2);
			}
		}
	}

	private void sendTXProfile(string prof)
	{
		string sMsg = "tx_profile_ex:" + prof + ";";
		sendTextFrame(sMsg);
	}

	private void sendTXProfiles()
	{
		if (consoleThreadSafe != null && !consoleThreadSafe.IsSetupFormNull)
		{
			string[] tXProfileStrings = consoleThreadSafe.SetupForm.GetTXProfileStrings();
			string text = string.Join(",", tXProfileStrings);
			string sMsg = "tx_profiles_ex:" + text + ";";
			sendTextFrame(sMsg);
		}
	}

	private void handleTXProfile(string[] args)
	{
		if (args != null && args.Length <= 1)
		{
			if (args.Length == 0)
			{
				string tXProfile = consoleThreadSafe.TXProfile;
				sendTXProfile(tXProfile);
			}
			else
			{
				consoleThreadSafe.SafeTXProfileSet(args[0]);
			}
		}
	}

	private void handleTXProfiles()
	{
		sendTXProfiles();
	}

	private void handleShutdown()
	{
		if (consoleThreadSafe.InvokeRequired)
		{
			consoleThreadSafe.BeginInvoke((MethodInvoker)delegate
			{
				consoleThreadSafe.Close();
			});
		}
		else
		{
			consoleThreadSafe.Close();
		}
	}

	private void sendCalibration(int rx, float meter, float display, float xvtr, float six_meter, float tx_display_offset)
	{
		if (rx >= 0 && rx <= 1)
		{
			string sMsg = string.Format("calibration_ex:{0},{1},", rx, meter.ToString("F6", CultureInfo.InvariantCulture)) + display.ToString("F6", CultureInfo.InvariantCulture) + "," + xvtr.ToString("F6", CultureInfo.InvariantCulture) + "," + six_meter.ToString("F6", CultureInfo.InvariantCulture) + "," + tx_display_offset.ToString("F6", CultureInfo.InvariantCulture) + ";";
			sendTextFrame(sMsg);
		}
	}

	private void handleCalibration(string[] args)
	{
		if (args.Length == 1 && int.TryParse(args[0], out var result))
		{
			CalibrationChanged(result);
		}
	}

	private void handleRunCatCommand(string msg)
	{
		int num = msg.IndexOf(':');
		if (num == -1 || num == msg.Length - 1)
		{
			return;
		}
		string text = msg.Substring(num + 1);
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		try
		{
			text += ";";
			string text2 = consoleThreadSafe.ThreadSafeCatParse(text);
			if (!string.IsNullOrEmpty(text2))
			{
				text2.Replace(";", "");
				string text3 = "run_cat_ex:" + text + "," + text2;
				if (text3.Right(1) != ";")
				{
					text3 += ";";
				}
				sendTextFrame(text3);
			}
		}
		catch
		{
			sendTextFrame("run_cat_ex:" + text + ",?;");
		}
	}

	private List<string> splitTextCommands(string msg)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrWhiteSpace(msg))
		{
			return list;
		}
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int num2 = 0;
		for (int i = 0; i < msg.Length; i++)
		{
			char c = msg[i];
			if (!flag && c == ':')
			{
				flag = msg.Substring(num, i - num).Trim().Equals("spot", StringComparison.OrdinalIgnoreCase);
			}
			if (flag && !flag2 && c == '[' && i + 6 < msg.Length && string.Compare(msg, i, "[json]{", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
			{
				flag2 = true;
				num2 = 1;
				flag3 = false;
				flag4 = false;
				i += 6;
			}
			else if (flag2)
			{
				if (flag4)
				{
					flag4 = false;
					continue;
				}
				switch (c)
				{
				case '\\':
					flag4 = true;
					continue;
				case '"':
					flag3 = !flag3;
					continue;
				}
				if (flag3)
				{
					continue;
				}
				switch (c)
				{
				case '{':
					num2++;
					break;
				case '}':
					num2--;
					if (num2 <= 0)
					{
						flag2 = false;
						num2 = 0;
					}
					break;
				}
			}
			else if (c == ';')
			{
				string text = msg.Substring(num, i - num).Trim();
				if (text.Length > 0)
				{
					list.Add(text);
				}
				num = i + 1;
				flag = false;
				flag2 = false;
				flag3 = false;
				flag4 = false;
				num2 = 0;
			}
		}
		if (num < msg.Length)
		{
			string text2 = msg.Substring(num).Trim();
			if (text2.Length > 0)
			{
				list.Add(text2);
			}
		}
		return list;
	}

	private void parseTextFrame(string msg)
	{
		if (string.IsNullOrWhiteSpace(msg))
		{
			return;
		}
		List<string> list = splitTextCommands(msg);
		if (list.Count > 1)
		{
			for (int i = 0; i < list.Count; i++)
			{
				parseTextFrame(list[i]);
			}
			return;
		}
		if (list.Count == 1)
		{
			msg = list[0];
		}
		if (m_server != null && m_server.LogForm != null)
		{
			m_server.LogForm.Log(bIn: true, msg);
		}
		if (msg.EndsWith(";"))
		{
			msg = msg.Substring(0, msg.Length - 1).Trim();
		}
		string[] array = msg.Split(new char[1] { ':' }, 2);
		bool flag = array.Length >= 2 && array[0].ToLower().Trim() == "spot" && msg.ToLower().IndexOf("[json]{") >= 0;
		if ((array.Length == 2) | flag)
		{
			string text = array[0].ToLower().Trim();
			string[] args = array[1].Split(',');
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 10:
				switch (text[4])
				{
				case 'l':
					if (text == "modulation")
					{
						handleModulationMessage(args);
					}
					break;
				case 'e':
					switch (text)
					{
					case "rit_enable":
						handleRITEnableMessage(args);
						break;
					case "xit_enable":
						handleXITEnableMessage(args);
						break;
					case "sql_enable":
						handleSqlEnable(args);
						break;
					case "mon_enable":
						handleMONEnable(args);
						break;
					}
					break;
				case 'o':
					switch (text)
					{
					case "rit_offset":
						handleRITOffsetMessage(args);
						break;
					case "xit_offset":
						handleXITOffsetMessage(args);
						break;
					case "audio_stop":
						handleAudioStart(args, enable: false);
						break;
					}
					break;
				case '_':
					if (text == "tune_drive")
					{
						handleTuneDrive(args);
					}
					break;
				case 'v':
					if (text == "mon_volume")
					{
						handleMONVolume(args);
					}
					break;
				case 'a':
					if (text == "rx_balance")
					{
						handleRxBalance(args);
					}
					break;
				case 't':
					if (text == "rx_ctun_ex")
					{
						handleCTUN(args);
					}
					break;
				case 'c':
					if (text == "run_cat_ex")
					{
						handleRunCatCommand(msg);
					}
					break;
				}
				break;
			case 3:
				switch (text[0])
				{
				case 'v':
					if (text == "vfo")
					{
						handleVFOMessage(args);
					}
					break;
				case 't':
					if (text == "trx")
					{
						handleTrxMessage(args);
					}
					break;
				case 'd':
					if (text == "dds")
					{
						handleDDS(args);
					}
					break;
				}
				break;
			case 12:
				switch (text[4])
				{
				case 't':
					if (text == "split_enable")
					{
						handleSplitEnableMessage(args);
					}
					break;
				case 'f':
					if (text == "rx_nf_enable")
					{
						handleRxNfEnable(args);
					}
					break;
				case 'r':
					if (text == "rx_nr_enable")
					{
						handleNREnable(args, is_extended: false);
					}
					break;
				case 'b':
					if (text == "rx_nb_enable")
					{
						handleRxNBEnable(args, is_extended: false);
					}
					break;
				}
				break;
			case 13:
				switch (text[4])
				{
				case 'i':
					if (text == "rx_bin_enable")
					{
						handleRxBinEnable(args);
					}
					break;
				case 'p':
					if (text == "rx_apf_enable")
					{
						handleRxApfEnable(args);
					}
					break;
				case 'a':
					if (text == "iq_samplerate")
					{
						handleIQSampleRate(args);
					}
					break;
				case '_':
					if (text == "line_out_stop")
					{
						handleLineOutStop(args);
					}
					break;
				case 'n':
					if (text == "rx_anf_enable")
					{
						handleAnfEnable(args);
					}
					break;
				case 'r':
					if (text == "tx_profile_ex")
					{
						handleTXProfile(args);
					}
					break;
				}
				break;
			case 4:
				switch (text[0])
				{
				case 'l':
					if (text == "lock")
					{
						handleLock(args);
					}
					break;
				case 't':
					if (text == "tune")
					{
						handleTune(args);
					}
					break;
				case 's':
					if (text == "spot")
					{
						handleSpot(args, flag, msg);
					}
					break;
				case 'm':
					if (text == "mute")
					{
						handleMute(args);
					}
					break;
				}
				break;
			case 8:
				switch (text[4])
				{
				case 'l':
					if (text == "vfo_lock")
					{
						handleVFOLock(args);
					}
					break;
				case 't':
					if (text == "iq_start")
					{
						handleIQStart(args, enable: true);
					}
					break;
				case 'm':
					if (text == "agc_mode")
					{
						handleAgcMode(args);
					}
					break;
				case 'g':
					if (text == "agc_gain")
					{
						handleAgcGain(args);
					}
					break;
				}
				break;
			case 9:
				switch (text[3])
				{
				case '_':
					if (text == "sql_level")
					{
						handleSqlLevel(args);
					}
					break;
				case 'm':
					if (text == "cw_macros")
					{
						handleCwMacros(args);
					}
					break;
				case 'e':
					if (text == "rx_enable")
					{
						handleRXEnable(args);
					}
					break;
				case 'v':
					if (text == "rx_volume")
					{
						handleRxVolume(args);
					}
					break;
				}
				break;
			case 11:
				switch (text[1])
				{
				case 'i':
					if (!(text == "digl_offset"))
					{
						if (text == "digu_offset")
						{
							handleDiguOffset(args);
						}
					}
					else
					{
						handleDiglOffset(args);
					}
					break;
				case 'w':
					if (text == "cw_terminal")
					{
						handleCwTerminal(args);
					}
					break;
				case 'u':
					if (text == "audio_start")
					{
						handleAudioStart(args, enable: true);
					}
					break;
				case 'p':
					if (text == "spot_delete")
					{
						handleDeleteSpot(args);
					}
					break;
				case 'f':
					if (text == "vfo_sync_ex")
					{
						handleVfoSyncEx(args);
					}
					break;
				case 'g':
					if (text == "agc_auto_ex")
					{
						handleAgcAutoEx(args);
					}
					break;
				}
				break;
			case 15:
				switch (text[4])
				{
				case 'a':
					if (!(text == "cw_macros_speed"))
					{
						if (text == "cw_macros_delay")
						{
							handleCwMacrosDelay(args);
						}
					}
					else
					{
						handleCwMacrosSpeed(args);
					}
					break;
				case 'r':
					if (text == "rx_nr_enable_ex")
					{
						handleNREnable(args, is_extended: true);
					}
					break;
				case 'b':
					if (text == "rx_nb_enable_ex")
					{
						handleRxNBEnable(args, is_extended: true);
					}
					break;
				case 'e':
					if (text == "fm_deviation_ex")
					{
						handleFMDeviationEx(args);
					}
					break;
				}
				break;
			case 14:
				switch (text[3])
				{
				case 'k':
					if (text == "cw_keyer_speed")
					{
						handleCwKeyerSpeed(args);
					}
					break;
				case 'e':
					if (text == "line_out_start")
					{
						handleLineOutStart(args);
					}
					break;
				case 'f':
					if (text == "rx_filter_band")
					{
						handleRxFilterBand(args);
					}
					break;
				case 's':
					if (text == "rx_step_att_ex")
					{
						handleRxStepAttEx(args);
					}
					break;
				case 'i':
					if (text == "calibration_ex")
					{
						handleCalibration(args);
					}
					break;
				}
				break;
			case 20:
				switch (text[0])
				{
				case 'c':
					if (text == "cw_macros_speed_down")
					{
						handleCwMacrosSpeedDown(args);
					}
					break;
				case 'a':
					if (text == "audio_stream_samples")
					{
						handleAudioStreamSamples(args);
					}
					break;
				}
				break;
			case 6:
				switch (text[0])
				{
				case 'c':
					if (text == "cw_msg")
					{
						handleCwMsg(args);
					}
					break;
				case 'v':
					if (text == "volume")
					{
						handleVolume(args);
					}
					break;
				}
				break;
			case 5:
				switch (text[0])
				{
				case 'k':
					if (text == "keyer")
					{
						handleKeyer(args);
					}
					break;
				case 'd':
					if (text == "drive")
					{
						handleDrive(args);
					}
					break;
				}
				break;
			case 16:
				switch (text[0])
				{
				case 'a':
					if (text == "audio_samplerate")
					{
						handleAudioSampleRate(args);
					}
					break;
				case 'r':
					if (text == "rx_preamp_att_ex")
					{
						handleRxPreampAttEx(args);
					}
					break;
				}
				break;
			case 7:
				switch (text[0])
				{
				case 'i':
					if (text == "iq_stop")
					{
						handleIQStart(args, enable: false);
					}
					break;
				case 'r':
					if (text == "rx_mute")
					{
						handleMuteRX(args);
					}
					break;
				}
				break;
			case 17:
				switch (text[3])
				{
				case 'f':
					if (text == "tx_filter_band_ex")
					{
						handleTXFilterBandEx(args);
					}
					break;
				case 'c':
					if (text == "rx_channel_enable")
					{
						handleRxChannelEnable(args);
					}
					break;
				case 's':
					if (!(text == "rx_sensors_enable"))
					{
						if (text == "tx_sensors_enable")
						{
							handleTxSensorsEnable(args);
						}
					}
					else
					{
						handleRxSensorsEnable(args);
					}
					break;
				}
				break;
			case 18:
				if (text == "cw_macros_speed_up")
				{
					handleCwMacrosSpeedUp(args);
				}
				break;
			case 24:
				if (text == "audio_stream_sample_type")
				{
					handleAudioStreamSampleType(args);
				}
				break;
			case 21:
				if (text == "audio_stream_channels")
				{
					handleAudioStreamChannels(args);
				}
				break;
			case 25:
				if (text == "tx_stream_audio_buffering")
				{
					handleTxStreamAudioBuffering(args);
				}
				break;
			case 2:
				if (text == "if")
				{
					handleIF(args);
				}
				break;
			case 19:
				if (text == "spot_simulate_click")
				{
					handleSpotSimulateClick(args);
				}
				break;
			case 22:
				if (text == "rx_step_att_enabled_ex")
				{
					handleRxStepAttEnabledEx(args);
				}
				break;
			case 23:
				break;
			}
		}
		else
		{
			if (array.Length != 1)
			{
				return;
			}
			string text2 = array[0].ToLower().Trim();
			if (text2 == null)
			{
				return;
			}
			switch (text2.Length)
			{
			case 4:
				switch (text2[0])
				{
				case 's':
					if (text2 == "stop")
					{
						handleStop();
					}
					break;
				case 'm':
					if (text2 == "mute")
					{
						handleMute(null, hasArgs: false);
					}
					break;
				}
				break;
			case 11:
				switch (text2[3])
				{
				case '_':
					if (!(text2 == "vfo_sync_ex"))
					{
						if (text2 == "vfo_swap_ex")
						{
							handleVfoSwapEx();
						}
					}
					else
					{
						handleVfoSyncEx(null);
					}
					break;
				case 'l':
					if (text2 == "digl_offset")
					{
						handleDiglOffset(null, hasArgs: false);
					}
					break;
				case 'u':
					if (text2 == "digu_offset")
					{
						handleDiguOffset(null, hasArgs: false);
					}
					break;
				case 't':
					if (text2 == "shutdown_ex")
					{
						handleShutdown();
					}
					break;
				}
				break;
			case 10:
				switch (text2[4])
				{
				case 'e':
					if (text2 == "mon_enable")
					{
						handleMONEnable(null, hasArgs: false);
					}
					break;
				case 'v':
					if (text2 == "mon_volume")
					{
						handleMONVolume(null, hasArgs: false);
					}
					break;
				case '_':
					if (text2 == "spot_clear")
					{
						handleSpotClear();
					}
					break;
				}
				break;
			case 15:
				switch (text2[10])
				{
				case 's':
					if (text2 == "cw_macros_speed")
					{
						handleCwMacrosSpeed(null, hasArgs: false);
					}
					break;
				case 'd':
					if (text2 == "cw_macros_delay")
					{
						handleCwMacrosDelay(null, hasArgs: false);
					}
					break;
				}
				break;
			case 14:
				switch (text2[3])
				{
				case 'k':
					if (text2 == "cw_keyer_speed")
					{
						handleCwKeyerSpeed(null, hasArgs: false);
					}
					break;
				case 'm':
					if (text2 == "cw_macros_stop")
					{
						handleCwMacrosStop();
					}
					break;
				case 'p':
					if (text2 == "tx_profiles_ex")
					{
						handleTXProfiles();
					}
					break;
				}
				break;
			case 13:
				switch (text2[0])
				{
				case 'i':
					if (text2 == "iq_samplerate")
					{
						sendIQSampleRate(getPublishedIQSampleRate());
					}
					break;
				case 't':
					if (text2 == "tx_profile_ex")
					{
						string[] args2 = new string[0];
						handleTXProfile(args2);
					}
					break;
				}
				break;
			case 5:
				if (text2 == "start")
				{
					handleStart();
				}
				break;
			case 17:
				if (text2 == "tx_filter_band_ex")
				{
					handleTXFilterBandEx(null);
				}
				break;
			case 12:
				if (text2 == "set_in_focus")
				{
					handleSetInFocus();
				}
				break;
			case 6:
				if (text2 == "volume")
				{
					handleVolume(null, hasArgs: false);
				}
				break;
			case 16:
				if (text2 == "audio_samplerate")
				{
					sendAudioSampleRate(m_audioSampleRate);
				}
				break;
			case 25:
				if (text2 == "tx_stream_audio_buffering")
				{
					sendTxStreamAudioBuffering(m_txStreamAudioBufferingMs);
				}
				break;
			case 7:
			case 8:
			case 9:
			case 18:
			case 19:
			case 20:
			case 21:
			case 22:
			case 23:
			case 24:
				break;
			}
		}
	}

	private static int getDefaultAudioStreamSamples(int sampleRate)
	{
		return sampleRate switch
		{
			8000 => 256, 
			12000 => 512, 
			24000 => 1024, 
			_ => 2048, 
		};
	}

	private static int getBytesPerSample(TCISampleType sampleType)
	{
		return sampleType switch
		{
			TCISampleType.INT16 => 2, 
			TCISampleType.INT24 => 3, 
			_ => 4, 
		};
	}

	private static void writeUInt32(byte[] buffer, int offset, uint value)
	{
		buffer[offset] = (byte)(value & 0xFF);
		buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
		buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
		buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
	}

	private byte[] buildStreamPayload(int receiver, int sampleRate, TCISampleType sampleType, int length, TCIStreamType streamType, int channels, byte[] samplePayload)
	{
		int num = ((samplePayload != null) ? samplePayload.Length : 0);
		byte[] array = new byte[64 + num];
		int num2 = 0;
		writeUInt32(array, num2, (uint)receiver);
		num2 += 4;
		writeUInt32(array, num2, (uint)sampleRate);
		num2 += 4;
		writeUInt32(array, num2, (uint)sampleType);
		num2 += 4;
		writeUInt32(array, num2, 0u);
		num2 += 4;
		writeUInt32(array, num2, 0u);
		num2 += 4;
		writeUInt32(array, num2, (uint)length);
		num2 += 4;
		writeUInt32(array, num2, (uint)streamType);
		num2 += 4;
		writeUInt32(array, num2, (uint)channels);
		num2 += 4;
		int num3 = 0;
		while (num3 < 8)
		{
			writeUInt32(array, num2, 0u);
			num3++;
			num2 += 4;
		}
		if (num > 0)
		{
			Buffer.BlockCopy(samplePayload, 0, array, 64, num);
		}
		return array;
	}

	private byte[] encodeSamples(float[] samples, TCISampleType sampleType)
	{
		if (samples == null || samples.Length == 0)
		{
			return Array.Empty<byte>();
		}
		int bytesPerSample = getBytesPerSample(sampleType);
		byte[] array = new byte[samples.Length * bytesPerSample];
		if (sampleType == TCISampleType.FLOAT32)
		{
			Buffer.BlockCopy(samples, 0, array, 0, array.Length);
			return array;
		}
		int num = 0;
		for (int i = 0; i < samples.Length; i++)
		{
			float num2 = Math.Max(-1f, Math.Min(1f, samples[i]));
			switch (sampleType)
			{
			case TCISampleType.INT16:
			{
				short num5 = (short)Math.Round(num2 * 32767f);
				array[num++] = (byte)(num5 & 0xFF);
				array[num++] = (byte)((num5 >> 8) & 0xFF);
				break;
			}
			case TCISampleType.INT24:
			{
				int num4 = (int)Math.Round(num2 * 8388607f);
				array[num++] = (byte)(num4 & 0xFF);
				array[num++] = (byte)((num4 >> 8) & 0xFF);
				array[num++] = (byte)((num4 >> 16) & 0xFF);
				break;
			}
			case TCISampleType.INT32:
			{
				int num3 = (int)Math.Round(num2 * 2.1474836E+09f);
				array[num++] = (byte)(num3 & 0xFF);
				array[num++] = (byte)((num3 >> 8) & 0xFF);
				array[num++] = (byte)((num3 >> 16) & 0xFF);
				array[num++] = (byte)((num3 >> 24) & 0xFF);
				break;
			}
			}
		}
		return array;
	}

	private static float[] decodeSamples(byte[] payload, int dataOffset, int length, TCISampleType sampleType)
	{
		float[] array = new float[length];
		int num = dataOffset;
		for (int i = 0; i < length; i++)
		{
			switch (sampleType)
			{
			case TCISampleType.INT16:
				array[i] = (float)BitConverter.ToInt16(payload, num) / 32768f;
				num += 2;
				break;
			case TCISampleType.INT24:
			{
				int num2 = payload[num] | (payload[num + 1] << 8) | (payload[num + 2] << 16);
				if ((num2 & 0x800000) != 0)
				{
					num2 |= -16777216;
				}
				array[i] = (float)num2 / 8388608f;
				num += 3;
				break;
			}
			case TCISampleType.INT32:
				array[i] = (float)BitConverter.ToInt32(payload, num) / 2.1474836E+09f;
				num += 4;
				break;
			default:
				array[i] = BitConverter.ToSingle(payload, num);
				num += 4;
				break;
			}
		}
		return array;
	}

	private static double[] convertStreamSamplesToComplex(float[] samples, int channels)
	{
		if (channels < 1)
		{
			channels = 1;
		}
		int num = ((channels <= 1) ? samples.Length : (samples.Length / channels));
		double[] array = new double[num * 2];
		if (channels == 1)
		{
			for (int i = 0; i < num; i++)
			{
				array[2 * i + 1] = (array[2 * i] = samples[i]);
			}
		}
		else
		{
			int num2 = 0;
			int num3 = 0;
			while (num2 < num)
			{
				array[2 * num2] = samples[num3];
				array[2 * num2 + 1] = samples[num3 + 1];
				num2++;
				num3 += channels;
			}
		}
		return array;
	}

	private void sendIQSampleRate(int sampleRate)
	{
		if (sampleRate < 48000)
		{
			sampleRate = 48000;
		}
		if (sampleRate > 384000)
		{
			sampleRate = 384000;
		}
		sendTextFrame("iq_samplerate:" + sampleRate + ";");
	}

	private void sendAudioSampleRate(int sampleRate)
	{
		sendTextFrame("audio_samplerate:" + sampleRate + ";");
	}

	private void sendAudioStreamSampleType(TCISampleType sampleType)
	{
		sendTextFrame("audio_stream_sample_type:" + sampleType.ToString().ToLower() + ";");
	}

	private void sendAudioStreamChannels(int channels)
	{
		sendTextFrame("audio_stream_channels:" + channels + ";");
	}

	private void sendAudioStreamSamples(int samples)
	{
		sendTextFrame("audio_stream_samples:" + samples + ";");
	}

	private void sendTxStreamAudioBuffering(int milliseconds)
	{
		sendTextFrame("tx_stream_audio_buffering:" + milliseconds + ";");
	}

	private bool wantsIQStream(int receiver)
	{
		lock (m_objStreamLock)
		{
			if (m_server != null && m_server.AlwaysStreamIQ)
			{
				return true;
			}
			return m_iqStreamEnabled.Contains(receiver);
		}
	}

	private bool wantsAudioStream(int receiver)
	{
		lock (m_objStreamLock)
		{
			return m_audioStreamEnabled.Contains(receiver);
		}
	}

	internal bool IsReadyForStreaming()
	{
		if (m_bWebSocket)
		{
			return !m_disconnected;
		}
		return false;
	}

	internal bool WantsAnyRxStream()
	{
		lock (m_objStreamLock)
		{
			return m_iqStreamEnabled.Count > 0 || m_audioStreamEnabled.Count > 0;
		}
	}

	internal void PublishIQSamples(int receiver, int sampleRate, float[] iqSamples, int complexSamples = -1)
	{
		if (iqSamples != null && wantsIQStream(receiver))
		{
			if (complexSamples < 0)
			{
				complexSamples = iqSamples.Length / 2;
			}
			byte[] samplePayload = encodeSamples(iqSamples, TCISampleType.FLOAT32);
			sendBinaryFrame(buildStreamPayload(receiver, sampleRate, TCISampleType.FLOAT32, complexSamples * 2, TCIStreamType.IQ_STREAM, 2, samplePayload));
		}
	}

	internal void PublishRxAudioSamples(int receiver, int sampleRate, float[] left, float[] right, int samples = -1)
	{
		if (!wantsAudioStream(receiver) || left == null)
		{
			return;
		}
		if (samples < 0)
		{
			samples = left.Length;
		}
		if (samples <= 0)
		{
			return;
		}
		if (samples > left.Length)
		{
			samples = left.Length;
		}
		TCISampleType audioSampleType;
		int audioStreamChannels;
		int audioStreamSamples;
		int audioSampleRate;
		lock (m_objStreamLock)
		{
			audioSampleType = m_audioSampleType;
			audioStreamChannels = m_audioStreamChannels;
			audioStreamSamples = m_audioStreamSamples;
			audioSampleRate = m_audioSampleRate;
		}
		lock (m_objRxAudioLock)
		{
			if (sampleRate != audioSampleRate)
			{
				samples = resampleRxAudioSamples(receiver, sampleRate, audioSampleRate, left, right, samples, out left, out right, out var resampled);
				if (resampled)
				{
					sampleRate = audioSampleRate;
				}
				if (samples <= 0)
				{
					return;
				}
			}
			if (!m_rxAudioLeftPending.TryGetValue(receiver, out var value))
			{
				value = new TCIPendingFloatBuffer(samples * 2);
				m_rxAudioLeftPending[receiver] = value;
			}
			if (!m_rxAudioRightPending.TryGetValue(receiver, out var value2))
			{
				value2 = new TCIPendingFloatBuffer(samples * 2);
				m_rxAudioRightPending[receiver] = value2;
			}
			value.Enqueue(left, 0, samples);
			int num = ((right != null) ? Math.Min(samples, right.Length) : 0);
			if (num > 0)
			{
				value2.Enqueue(right, 0, num);
			}
			if (num < samples)
			{
				value2.Enqueue(left, num, samples - num);
			}
			while (audioStreamSamples > 0 && value.Count >= audioStreamSamples && value2.Count >= audioStreamSamples)
			{
				float[] array = ((audioStreamChannels <= 1) ? new float[audioStreamSamples] : new float[audioStreamSamples * 2]);
				if (audioStreamChannels <= 1)
				{
					value.CopyTo(array, 0, audioStreamSamples);
				}
				else
				{
					for (int i = 0; i < audioStreamSamples; i++)
					{
						array[2 * i] = value.Peek(i);
						array[2 * i + 1] = value2.Peek(i);
					}
				}
				value.Advance(audioStreamSamples);
				value2.Advance(audioStreamSamples);
				byte[] samplePayload = encodeSamples(array, audioSampleType);
				sendBinaryFrame(buildStreamPayload(receiver, sampleRate, audioSampleType, array.Length, TCIStreamType.RX_AUDIO_STREAM, audioStreamChannels, samplePayload));
			}
		}
	}

	internal void SendTxChrono(int receiver)
	{
		int audioSampleRate;
		int audioStreamSamples;
		int audioStreamChannels;
		TCISampleType audioSampleType;
		bool seenModernTxAudioNegotiation;
		lock (m_objStreamLock)
		{
			audioSampleRate = m_audioSampleRate;
			audioStreamSamples = m_audioStreamSamples;
			audioStreamChannels = m_audioStreamChannels;
			audioSampleType = m_audioSampleType;
			seenModernTxAudioNegotiation = m_seenModernTxAudioNegotiation;
		}
		int length = (seenModernTxAudioNegotiation ? (audioStreamSamples * Math.Max(1, audioStreamChannels)) : audioStreamSamples);
		sendBinaryFrame(buildStreamPayload(receiver, audioSampleRate, audioSampleType, length, TCIStreamType.TX_CHRONO, audioStreamChannels, Array.Empty<byte>()));
	}

	internal bool UsesTCITxAudio()
	{
		lock (m_objStreamLock)
		{
			return m_txUsesTCIAudio;
		}
	}

	internal bool UsesActiveTCITxAudio()
	{
		lock (m_objStreamLock)
		{
			return m_txUsesTCIAudio && m_tciPttActive;
		}
	}

	internal bool TryGetTxAudioRequestSettings(out int sampleRate, out int samples, out int bufferingMs)
	{
		lock (m_objStreamLock)
		{
			sampleRate = m_audioSampleRate;
			samples = m_audioStreamSamples;
			bufferingMs = m_txStreamAudioBufferingMs;
			return m_tciPttActive;
		}
	}

	internal void SyncTciPttToMox(bool expectedMox)
	{
		bool flag = false;
		lock (m_objStreamLock)
		{
			if (!expectedMox)
			{
				flag = m_tciPttActive;
				m_tciPttActive = false;
			}
		}
		if (flag)
		{
			clearQueuedTxAudio();
			m_server?.ReleaseActiveTxAudioListener(this);
		}
	}

	private void clearQueuedTxAudio()
	{
		lock (m_objTxQueueLock)
		{
			m_txAudioQueue.Clear();
			m_txQueuedComplexSamples = 0;
		}
	}

	internal bool TryDequeueTxAudio(out TCIQueuedTxAudio queuedAudio)
	{
		lock (m_objTxQueueLock)
		{
			if (m_txAudioQueue.Count > 0)
			{
				queuedAudio = m_txAudioQueue.Dequeue();
				if (queuedAudio != null)
				{
					m_txQueuedComplexSamples = Math.Max(0, m_txQueuedComplexSamples - Math.Max(0, queuedAudio.ComplexSamples));
				}
				return true;
			}
		}
		queuedAudio = null;
		return false;
	}

	private void handleBinaryFrame(byte[] payload)
	{
		if (payload == null || payload.Length < 64)
		{
			return;
		}
		int receiver = BitConverter.ToInt32(payload, 0);
		int sampleRate = BitConverter.ToInt32(payload, 4);
		TCISampleType sampleType = (TCISampleType)BitConverter.ToUInt32(payload, 8);
		int num = BitConverter.ToInt32(payload, 20);
		uint num2 = BitConverter.ToUInt32(payload, 24);
		int num3 = BitConverter.ToInt32(payload, 28);
		if (num2 != 2 || num <= 0)
		{
			return;
		}
		int bytesPerSample = getBytesPerSample(sampleType);
		int num4 = 64;
		int num5 = payload.Length - num4;
		if (num5 < bytesPerSample)
		{
			return;
		}
		int num6 = num5 / bytesPerSample;
		int num7;
		int num8;
		if (num3 == 1 || num3 == 2)
		{
			num7 = num3;
			num8 = Math.Min(num, num6);
			if (num7 > 1)
			{
				num8 -= num8 % num7;
			}
		}
		else
		{
			num7 = ((num6 < num * 2) ? 1 : 2);
			num8 = Math.Min(num, num6);
			if (num7 > 1)
			{
				num8 -= num8 % num7;
			}
		}
		if (num8 <= 0)
		{
			return;
		}
		float[] array = decodeSamples(payload, num4, num8, sampleType);
		for (int i = 0; i < array.Length; i++)
		{
			float num9 = array[i];
			if (float.IsNaN(num9) || float.IsInfinity(num9))
			{
				array[i] = 0f;
			}
			else if (num9 > 4f)
			{
				array[i] = 4f;
			}
			else if (num9 < -4f)
			{
				array[i] = -4f;
			}
		}
		int complexSamples = ((num7 <= 1) ? array.Length : (array.Length / num7));
		TCIQueuedTxAudio tCIQueuedTxAudio = new TCIQueuedTxAudio
		{
			Receiver = receiver,
			SampleRate = sampleRate,
			SampleType = sampleType,
			Channels = num7,
			ComplexSamples = complexSamples,
			Samples = convertStreamSamplesToComplex(array, num7)
		};
		lock (m_objTxQueueLock)
		{
			while ((m_txAudioQueue.Count >= 64 || m_txQueuedComplexSamples + tCIQueuedTxAudio.ComplexSamples > 96000) && m_txAudioQueue.Count != 0)
			{
				TCIQueuedTxAudio tCIQueuedTxAudio2 = m_txAudioQueue.Dequeue();
				if (tCIQueuedTxAudio2 != null)
				{
					m_txQueuedComplexSamples = Math.Max(0, m_txQueuedComplexSamples - Math.Max(0, tCIQueuedTxAudio2.ComplexSamples));
				}
			}
			m_txAudioQueue.Enqueue(tCIQueuedTxAudio);
			m_txQueuedComplexSamples += Math.Max(0, tCIQueuedTxAudio.ComplexSamples);
		}
	}

	private void handleIQSampleRate(string[] args)
	{
		if (args.Length == 1 && int.TryParse(args[0], out var result))
		{
			sendIQSampleRate(result);
		}
	}

	private int getCurrentMaxHWSampleRate()
	{
		int num = 48000;
		try
		{
			for (int i = 0; i < cmaster.CMrcvr; i++)
			{
				int inputRate = cmaster.GetInputRate(0, i);
				if (inputRate > num)
				{
					num = inputRate;
				}
			}
		}
		catch
		{
		}
		return num;
	}

	private void handleAudioSampleRate(string[] args)
	{
		if (args.Length != 1)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		int audioSampleRate;
		int audioStreamSamples;
		if (int.TryParse(args[0], out var result))
		{
			if (result == 8000 || result == 12000 || result == 24000 || result == 48000)
			{
				lock (m_objStreamLock)
				{
					flag2 = m_audioSampleRate != result;
					m_audioSampleRate = result;
					if (!m_audioStreamSamplesExplicitlySet)
					{
						int defaultAudioStreamSamples = getDefaultAudioStreamSamples(result);
						if (m_audioStreamSamples != defaultAudioStreamSamples)
						{
							m_audioStreamSamples = defaultAudioStreamSamples;
							flag = true;
						}
					}
					audioSampleRate = m_audioSampleRate;
					audioStreamSamples = m_audioStreamSamples;
				}
			}
			else
			{
				lock (m_objStreamLock)
				{
					audioSampleRate = m_audioSampleRate;
					audioStreamSamples = m_audioStreamSamples;
				}
			}
		}
		else
		{
			lock (m_objStreamLock)
			{
				audioSampleRate = m_audioSampleRate;
				audioStreamSamples = m_audioStreamSamples;
			}
		}
		if (flag2)
		{
			clearRxAudioStreamState();
		}
		sendAudioSampleRate(audioSampleRate);
		if (flag)
		{
			sendAudioStreamSamples(audioStreamSamples);
		}
	}

	private void handleIQStart(string[] args, bool enable)
	{
		if (args.Length != 1 || !int.TryParse(args[0], out var result))
		{
			return;
		}
		lock (m_objStreamLock)
		{
			if (enable)
			{
				m_iqStreamEnabled.Add(result);
			}
			else
			{
				m_iqStreamEnabled.Remove(result);
			}
		}
		sendIQStartStop(result, enable);
		m_server?.RefreshStreamRunState();
	}

	private void sendIQStartStop(int receiver, bool enable)
	{
		sendTextFrame((enable ? "iq_start:" : "iq_stop:") + receiver + ";");
	}

	private void applyIQSampleRateToReceiver(int receiver, int sampleRate)
	{
		Console localConsole = consoleThreadSafe;
		if (localConsole == null || sampleRate <= 0)
		{
			return;
		}
		MethodInvoker methodInvoker = delegate
		{
			if (!localConsole.IsSetupFormNull)
			{
				if (receiver == 0)
				{
					if (localConsole.SetupForm.GetHWSampleRate(1) != sampleRate)
					{
						localConsole.SetupForm.SetHWSampleRate(1, sampleRate);
					}
				}
				else if (receiver == 1 && localConsole.RX2Enabled && localConsole.SetupForm.GetHWSampleRate(2) != sampleRate)
				{
					localConsole.SetupForm.SetHWSampleRate(2, sampleRate);
				}
			}
		};
		if (localConsole.InvokeRequired)
		{
			localConsole.Invoke(methodInvoker);
		}
		else
		{
			methodInvoker();
		}
	}

	private void handleRxChannelEnable(string[] args)
	{
		if (args.Length < 2 || args.Length > 3 || !int.TryParse(args[0], out var result))
		{
			return;
		}
		int result3;
		bool result4;
		if (args.Length == 2)
		{
			if (int.TryParse(args[1], out var result2))
			{
				bool enabled = false;
				switch (result)
				{
				case 0:
					enabled = result2 == 0 || consoleThreadSafe.GetSubRX(1);
					break;
				case 1:
					enabled = result2 == 0 && consoleThreadSafe.RX2Enabled;
					break;
				}
				sendRxChannelEnable(result, result2, enabled);
			}
		}
		else if (int.TryParse(args[1], out result3) && bool.TryParse(args[2], out result4))
		{
			if (result == 0 && result3 == 1)
			{
				consoleThreadSafe.SetSubRX(1, result4);
			}
			else if (result == 1)
			{
				consoleThreadSafe.RX2Enabled = result4;
			}
			sendRxChannelEnable(result, result3, result4);
		}
	}

	private void sendRxChannelEnable(int rx, int channel, bool enabled)
	{
		sendTextFrame("rx_channel_enable:" + rx + "," + channel + "," + enabled.ToString().ToLower() + ";");
	}

	private void handleAudioStart(string[] args, bool enable)
	{
		if (args.Length != 1 || !int.TryParse(args[0], out var result))
		{
			return;
		}
		lock (m_objStreamLock)
		{
			if (enable)
			{
				m_audioStreamEnabled.Add(result);
			}
			else
			{
				m_audioStreamEnabled.Remove(result);
			}
		}
		if (!enable)
		{
			clearRxAudioStateForReceiver(result);
		}
		sendAudioStartStop(result, enable);
		m_server?.RefreshStreamRunState();
	}

	private void handleAudioStreamSampleType(string[] args)
	{
		if (args.Length != 1)
		{
			return;
		}
		lock (m_objStreamLock)
		{
			switch (args[0].Trim().ToLower())
			{
			case "int16":
				m_audioSampleType = TCISampleType.INT16;
				break;
			case "int24":
				m_audioSampleType = TCISampleType.INT24;
				break;
			case "int32":
				m_audioSampleType = TCISampleType.INT32;
				break;
			default:
				m_audioSampleType = TCISampleType.FLOAT32;
				break;
			}
			m_seenModernTxAudioNegotiation = true;
		}
		sendAudioStreamSampleType(m_audioSampleType);
	}

	private void handleAudioStreamChannels(string[] args)
	{
		if (args.Length != 1)
		{
			return;
		}
		lock (m_objStreamLock)
		{
			if (int.TryParse(args[0], out var result) && (result == 1 || result == 2))
			{
				m_audioStreamChannels = result;
			}
			m_seenModernTxAudioNegotiation = true;
		}
		sendAudioStreamChannels(m_audioStreamChannels);
	}

	private void handleAudioStreamSamples(string[] args)
	{
		if (args.Length != 1)
		{
			return;
		}
		int audioStreamSamples;
		if (int.TryParse(args[0], out var result))
		{
			if (result >= 100 && result <= 2048)
			{
				lock (m_objStreamLock)
				{
					m_audioStreamSamples = result;
					m_audioStreamSamplesExplicitlySet = true;
					m_seenModernTxAudioNegotiation = true;
					audioStreamSamples = m_audioStreamSamples;
				}
			}
			else
			{
				lock (m_objStreamLock)
				{
					audioStreamSamples = m_audioStreamSamples;
				}
			}
		}
		else
		{
			lock (m_objStreamLock)
			{
				audioStreamSamples = m_audioStreamSamples;
			}
		}
		sendAudioStreamSamples(audioStreamSamples);
	}

	private void handleTxStreamAudioBuffering(string[] args)
	{
		if (args.Length != 1)
		{
			return;
		}
		lock (m_objStreamLock)
		{
			if (int.TryParse(args[0], out var result) && result >= 50 && result <= 500)
			{
				m_txStreamAudioBufferingMs = result;
			}
			m_seenModernTxAudioNegotiation = true;
		}
		sendTxStreamAudioBuffering(m_txStreamAudioBufferingMs);
	}

	private void PingFrameTimer(object o)
	{
		sendPingFrame("Thetis");
	}

	private void VFOcallback(object o)
	{
		VFOData vfod = (VFOData)o;
		vfoFrequencyChange(vfod);
	}

	private void Centrecallback(object o)
	{
		VFOData vfod = (VFOData)o;
		centreFrequencyChange(vfod);
	}

	public void VFOChange(VFOData vfod)
	{
		if (m_tmVFOtimer != null)
		{
			m_tmVFOtimer.Change(-1, -1);
			m_tmVFOtimer = null;
		}
		if (!m_swVFO.IsRunning || (m_swVFO.IsRunning && m_swVFO.ElapsedMilliseconds > m_nRateLimit))
		{
			vfoFrequencyChange(vfod);
			if (m_nRateLimit > 0)
			{
				m_swVFO.Restart();
			}
		}
		else
		{
			m_tmVFOtimer = new System.Threading.Timer(VFOcallback, vfod, m_nRateLimit, -1);
		}
	}

	public void CentreChange(VFOData vfod)
	{
		if (m_tmCentretimer != null)
		{
			m_tmCentretimer.Change(-1, -1);
			m_tmCentretimer = null;
		}
		if (!m_swCentre.IsRunning || (m_swCentre.IsRunning && m_swCentre.ElapsedMilliseconds > m_nRateLimit))
		{
			centreFrequencyChange(vfod);
			if (m_nRateLimit > 0)
			{
				m_swCentre.Restart();
			}
		}
		else
		{
			m_tmCentretimer = new System.Threading.Timer(Centrecallback, vfod, m_nRateLimit, -1);
		}
	}

	public void TXFrequencyChange(VFOData vfod)
	{
		if (m_tmTXFrequency != null)
		{
			m_tmTXFrequency.Change(-1, -1);
			m_tmTXFrequency = null;
		}
		if (!m_swTXFrequency.IsRunning || (m_swTXFrequency.IsRunning && m_swTXFrequency.ElapsedMilliseconds > m_nRateLimit))
		{
			txFrequencyChange(vfod);
			if (m_nRateLimit > 0)
			{
				m_swTXFrequency.Restart();
			}
		}
		else
		{
			m_tmTXFrequency = new System.Threading.Timer(Centrecallback, vfod, m_nRateLimit, -1);
		}
	}
}
