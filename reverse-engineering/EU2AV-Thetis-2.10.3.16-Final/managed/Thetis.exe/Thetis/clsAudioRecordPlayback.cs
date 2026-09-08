using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.MediaFoundation;
using NAudio.Wave;
using Newtonsoft.Json;

namespace Thetis;

public sealed class clsAudioRecordPlayback : IDisposable
{
	public sealed class RecordingJsonModel
	{
		public string utc_time { get; set; } = "";

		public string frequency { get; set; } = "";

		public string mode { get; set; } = "";

		public string band { get; set; } = "";

		public string wav_file { get; set; } = "";

		public long wav_file_size_bytes { get; set; }

		public string wav_file_last_write_utc { get; set; } = "";

		public double play_duration_seconds { get; set; }

		public int sample_rate { get; set; }

		public short bit_depth { get; set; }

		public short channels { get; set; }

		public short format_tag { get; set; }

		public string tag_description { get; set; } = "";

		public string mp3_file { get; set; } = "";

		public long mp3_file_size_bytes { get; set; }
	}

	public const bool PlaybackCosineFadeEnabled = true;

	public const int PlaybackCosineFadeMs = 50;

	private const int PcAudioWasapiInputIdBase = 100000;

	private const int PcAudioWasapiOutputIdBase = 200000;

	private const int RecordSpaceCheckMs = 2000;

	private const int WaveThingStopWaitMs = 2000;

	private readonly object _sync = new object();

	private Timer _record_space_timer;

	private int _record_space_timer_busy;

	private string _audio_folder;

	private int _free_space_perc;

	private bool _is_recording;

	private bool _is_playing;

	private IWaveIn _pc_wave_in;

	private NAudio.Wave.WaveFileWriter _pc_wave_writer;

	private string _active_record_id;

	private string _active_record_filename;

	private string _active_record_json_filename;

	private string _active_record_mp3_filename;

	private RecordingDetails _active_record_details;

	private int _active_record_sample_rate;

	private bool _active_record_failed;

	private string _active_record_failure_message;

	private string _active_play_id;

	private string _active_play_filename;

	private IWavePlayer _pc_wave_out;

	private AudioFileReader _pc_audio_reader;

	private int _active_record_wfw_id;

	private int _active_playback_wfw_id;

	private bool _disposed;

	private bool _is_wdsp_playing;

	private float _pc_input_gain;

	private float _pc_playback_gain;

	private byte[] _pc_record_gain_buffer;

	private Console _console;

	private Dictionary<string, bool> _playbackSetting;

	private Dictionary<string, bool> _prePlaybackSetting;

	private readonly SynchronizationContext _sync_context;

	private static readonly object _mf_sync = new object();

	private static bool _mf_started;

	public AudioRecordRxSource RxSource { get; set; }

	public AudioRecordTxSource TxSource { get; set; }

	public int SampleRate { get; set; }

	public AudioBitDepthMode BitDepthMode { get; set; }

	public bool DitherEnabled { get; set; }

	public float DitherAmount { get; set; }

	public bool MoxOnPlayback { get; set; } = true;

	public double MonoToStereoGainDb { get; set; } = 6.0;

	public bool GenerateMP3File { get; set; }

	public bool GenerateJSON { get; set; }

	public int InputPCDeviceID { get; set; } = -1;

	public int OutputPCDeviceID { get; set; } = -1;

	public PCInputSource PCInputSource { get; set; }

	public string AudioFolder
	{
		get
		{
			return _audio_folder;
		}
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				_audio_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Thetis");
			}
			else
			{
				_audio_folder = value.Trim();
			}
			ensureFolderExists(_audio_folder);
		}
	}

	public int StopRecordingFreeSpacePerc
	{
		get
		{
			return _free_space_perc;
		}
		set
		{
			_free_space_perc = value;
		}
	}

	public double WDSPTXAudioGainInDb
	{
		get
		{
			if (Audio.WavePreamp <= 0.0)
			{
				return -70.0;
			}
			double num = 20.0 * Math.Log10(Audio.WavePreamp);
			if (num < -70.0)
			{
				return -70.0;
			}
			if (num > 70.0)
			{
				return 70.0;
			}
			return num;
		}
		set
		{
			double num = value;
			if (num < -70.0)
			{
				num = -70.0;
			}
			if (num > 70.0)
			{
				num = 70.0;
			}
			Audio.WavePreamp = Math.Pow(10.0, num / 20.0);
		}
	}

	public float PCInputGain
	{
		get
		{
			return _pc_input_gain;
		}
		set
		{
			float num = value;
			if (float.IsNaN(num) || float.IsInfinity(num))
			{
				num = 1f;
			}
			if (num < 0f)
			{
				num = 0f;
			}
			if (num > 8f)
			{
				num = 8f;
			}
			lock (_sync)
			{
				_pc_input_gain = num;
			}
		}
	}

	public float PCOutputGain
	{
		get
		{
			return _pc_playback_gain;
		}
		set
		{
			float num = value;
			if (float.IsNaN(num) || float.IsInfinity(num))
			{
				num = 1f;
			}
			if (num < 0f)
			{
				num = 0f;
			}
			if (num > 8f)
			{
				num = 8f;
			}
			lock (_sync)
			{
				_pc_playback_gain = num;
				if (_pc_audio_reader != null)
				{
					_pc_audio_reader.Volume = _pc_playback_gain;
				}
			}
		}
	}

	public bool IsPlaying
	{
		get
		{
			lock (_sync)
			{
				return _is_playing;
			}
		}
	}

	public bool IsRecording
	{
		get
		{
			lock (_sync)
			{
				return _is_recording;
			}
		}
	}

	public bool IsWDSPBusy
	{
		get
		{
			lock (_sync)
			{
				return (_active_playback_wfw_id > -1 && _is_playing) || (_active_record_wfw_id > -1 && _is_recording);
			}
		}
	}

	public bool IsBusy
	{
		get
		{
			lock (_sync)
			{
				return _is_recording || _is_playing;
			}
		}
	}

	public event Action<bool, string, string> RecordingChanged;

	public event Action<bool, string, string, bool> PlayingChanged;

	public event Action<string, RecordingJsonModel> RecordingJsonWritten;

	public event Action<string, string, string> RecordError;

	public event Action<string, string, string> PlaybackError;

	public clsAudioRecordPlayback(Console c)
	{
		_sync_context = SynchronizationContext.Current;
		_console = c;
		Console console = _console;
		console.MoxPreChangeHandlers = (Console.MoxPreChanged)Delegate.Combine(console.MoxPreChangeHandlers, new Console.MoxPreChanged(OnPreMox));
		_playbackSetting = new Dictionary<string, bool>();
		_prePlaybackSetting = new Dictionary<string, bool>();
		_is_wdsp_playing = false;
		_active_record_wfw_id = -1;
		_active_playback_wfw_id = -1;
		_pc_input_gain = 1f;
		_pc_playback_gain = 1f;
		_audio_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Thetis");
		_free_space_perc = 10;
		RxSource = AudioRecordRxSource.ReceiverOutputAudio;
		TxSource = AudioRecordTxSource.MicAudio;
		SampleRate = 48000;
		BitDepthMode = AudioBitDepthMode.IeeeFloat32;
		DitherEnabled = false;
		DitherAmount = 0.8f;
		ensureFolderExists(_audio_folder);
		_record_space_timer = new Timer(onRecordSpaceTimer, null, -1, -1);
	}

	private void initPlaybackSettings()
	{
		if (_playbackSetting.Count < 1)
		{
			_playbackSetting["TXEQ"] = _console.TXEQ;
			_playbackSetting["COMP"] = _console.CPDR;
			_playbackSetting["CFC"] = _console.CFCEnabled;
			_playbackSetting["PHASE"] = _console.PhaseRotEnabled;
			_playbackSetting["LEVELER"] = _console.LevelerEnabled;
			_playbackSetting["MON"] = _console.MON;
			_playbackSetting["BYPASS_VAC"] = _console.BypassVACWhenPlayingWAV;
			_playbackSetting["MOX"] = _console.MOX;
		}
	}

	private static double? tryGetWavDurationSeconds(string wavPath)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(wavPath))
			{
				return null;
			}
			if (!File.Exists(wavPath))
			{
				return null;
			}
			using BinaryReader reader = new BinaryReader(File.Open(wavPath, FileMode.Open, FileAccess.Read, FileShare.Read));
			if (!tryParseWaveHeader(reader, out var formatTag, out var sampleRate, out var channels, out var bitsPerSample, out var _, out var dataLengthBytes, out var _))
			{
				return null;
			}
			if (channels < 1 || channels > 2)
			{
				return null;
			}
			if (sampleRate <= 0)
			{
				return null;
			}
			int num;
			if (formatTag == 3)
			{
				num = 4;
			}
			else
			{
				num = bitsPerSample / 8;
				if (num < 1)
				{
					num = 1;
				}
			}
			int num2 = channels * num;
			if (num2 < 1)
			{
				num2 = 1;
			}
			if (dataLengthBytes <= 0)
			{
				return 0.0;
			}
			double num3 = (double)dataLengthBytes / (double)num2 / (double)sampleRate;
			if (num3 < 0.0)
			{
				num3 = 0.0;
			}
			return Math.Round(num3, 3);
		}
		catch
		{
			return null;
		}
	}

	private static bool tryParseUtcStamp(string s, out DateTime utc)
	{
		utc = default(DateTime);
		if (string.IsNullOrWhiteSpace(s))
		{
			return false;
		}
		return DateTime.TryParseExact(s.Trim(), "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out utc);
	}

	private void refreshExistingJsonFromWavIfNeeded(string unique_id, string wavPath, int formatTag, int sampleRate, int channels, int bitsPerSample)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(wavPath) || !File.Exists(wavPath))
			{
				return;
			}
			string text = Path.ChangeExtension(wavPath, ".json");
			DateTime dateTime = DateTime.SpecifyKind(File.GetLastWriteTimeUtc(wavPath), DateTimeKind.Utc);
			string b = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
			RecordingJsonModel recordingJsonModel = null;
			if (File.Exists(text))
			{
				try
				{
					recordingJsonModel = JsonConvert.DeserializeObject<RecordingJsonModel>(File.ReadAllText(text, Encoding.UTF8));
				}
				catch
				{
					recordingJsonModel = null;
				}
			}
			string text2 = recordingJsonModel?.wav_file_last_write_utc;
			if (recordingJsonModel != null && string.Equals((text2 ?? string.Empty).Trim(), b, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			RecordingDetails recordingDetails = new RecordingDetails();
			DateTime utcTime = dateTime;
			if (recordingJsonModel != null)
			{
				if (tryParseUtcStamp(recordingJsonModel.utc_time, out var utc))
				{
					utcTime = utc;
				}
				recordingDetails.Frequency = recordingJsonModel.frequency;
				recordingDetails.Mode = recordingJsonModel.mode;
				recordingDetails.Band = recordingJsonModel.band;
				recordingDetails.Mp3File = recordingJsonModel.mp3_file;
				recordingDetails.Mp3FileSizeBytes = recordingJsonModel.mp3_file_size_bytes;
			}
			else
			{
				recordingDetails.Frequency = "";
				recordingDetails.Mode = "";
				recordingDetails.Band = "";
				string text3 = Path.ChangeExtension(wavPath, ".mp3");
				if (File.Exists(text3))
				{
					recordingDetails.Mp3File = Path.GetFileName(text3) ?? "";
					try
					{
						FileInfo fileInfo = new FileInfo(text3);
						recordingDetails.Mp3FileSizeBytes = fileInfo.Length;
					}
					catch
					{
						recordingDetails.Mp3FileSizeBytes = null;
					}
				}
				else
				{
					recordingDetails.Mp3File = "";
					recordingDetails.Mp3FileSizeBytes = null;
				}
			}
			recordingDetails.UtcTime = utcTime;
			recordingDetails.WavFile = Path.GetFileName(wavPath) ?? "";
			try
			{
				FileInfo fileInfo2 = new FileInfo(wavPath);
				recordingDetails.WavFileSizeBytes = fileInfo2.Length;
				recordingDetails.WavFileLastWriteUtc = DateTime.SpecifyKind(fileInfo2.LastWriteTimeUtc, DateTimeKind.Utc);
				recordingDetails.PlayDurationSeconds = tryGetWavDurationSeconds(wavPath);
			}
			catch
			{
				recordingDetails.WavFileSizeBytes = null;
				recordingDetails.WavFileLastWriteUtc = null;
				recordingDetails.PlayDurationSeconds = null;
			}
			recordingDetails.SampleRate = sampleRate;
			recordingDetails.BitDepth = (short)bitsPerSample;
			recordingDetails.Channels = (short)channels;
			recordingDetails.FormatTag = (short)formatTag;
			if (!string.IsNullOrWhiteSpace(recordingDetails.Mp3File))
			{
				try
				{
					string text4 = Path.Combine(Path.GetDirectoryName(wavPath) ?? "", recordingDetails.Mp3File);
					if (File.Exists(text4))
					{
						FileInfo fileInfo3 = new FileInfo(text4);
						recordingDetails.Mp3FileSizeBytes = fileInfo3.Length;
					}
					else
					{
						recordingDetails.Mp3FileSizeBytes = null;
					}
				}
				catch
				{
					recordingDetails.Mp3FileSizeBytes = null;
				}
			}
			else
			{
				recordingDetails.Mp3FileSizeBytes = null;
			}
			writeRecordingJson(unique_id, text, recordingDetails);
		}
		catch
		{
		}
	}

	public void SetPlaybackSetting(string setting, bool value)
	{
		initPlaybackSettings();
		setting = (setting ?? string.Empty).ToUpper();
		_playbackSetting[setting] = value;
	}

	public bool GetPlaybackSetting(string setting)
	{
		initPlaybackSettings();
		setting = (setting ?? string.Empty).ToUpper();
		if (!_playbackSetting.ContainsKey(setting))
		{
			return false;
		}
		return _playbackSetting[setting];
	}

	private void storeRestoreSettings(bool store, bool playback)
	{
		if (store)
		{
			if (playback)
			{
				_prePlaybackSetting["TXEQ"] = _console.TXEQ;
				_prePlaybackSetting["COMP"] = _console.CPDR;
				_prePlaybackSetting["CFC"] = _console.CFCEnabled;
				_prePlaybackSetting["PHASE"] = _console.PhaseRotEnabled;
				_prePlaybackSetting["LEVELER"] = _console.LevelerEnabled;
				_prePlaybackSetting["MON"] = _console.MON;
				_prePlaybackSetting["BYPASS_VAC"] = Audio.VACBypass;
				_prePlaybackSetting["MOX"] = _console.MOX;
			}
			else
			{
				_prePlaybackSetting["RXEQ"] = _console.RXEQ;
			}
		}
		else if (playback)
		{
			if (_prePlaybackSetting.ContainsKey("TXEQ") && _console.TXEQ != _prePlaybackSetting["TXEQ"])
			{
				_console.TXEQ = _prePlaybackSetting["TXEQ"];
			}
			if (_prePlaybackSetting.ContainsKey("COMP") && _console.CPDR != _prePlaybackSetting["COMP"])
			{
				_console.CPDR = _prePlaybackSetting["COMP"];
			}
			if (_prePlaybackSetting.ContainsKey("CFC") && _console.CFCEnabled != _prePlaybackSetting["CFC"])
			{
				_console.CFCEnabled = _prePlaybackSetting["CFC"];
			}
			if (_prePlaybackSetting.ContainsKey("PHASE") && _console.PhaseRotEnabled != _prePlaybackSetting["PHASE"])
			{
				_console.PhaseRotEnabled = _prePlaybackSetting["PHASE"];
			}
			if (_prePlaybackSetting.ContainsKey("LEVELER") && _console.LevelerEnabled != _prePlaybackSetting["LEVELER"])
			{
				_console.LevelerEnabled = _prePlaybackSetting["LEVELER"];
			}
			if (_prePlaybackSetting.ContainsKey("MON") && _console.MON != _prePlaybackSetting["MON"])
			{
				_console.MON = _prePlaybackSetting["MON"];
			}
			if (_prePlaybackSetting.ContainsKey("BYPASS_VAC") && Audio.VACBypass != _prePlaybackSetting["BYPASS_VAC"])
			{
				Audio.VACBypass = _prePlaybackSetting["BYPASS_VAC"];
			}
			if (_prePlaybackSetting.ContainsKey("MOX") && _console.MOX != _prePlaybackSetting["MOX"])
			{
				_console.MOX = _prePlaybackSetting["MOX"];
			}
		}
		else if (_prePlaybackSetting.ContainsKey("RXEQ") && _console.RXEQ != _prePlaybackSetting["RXEQ"])
		{
			_console.RXEQ = _prePlaybackSetting["RXEQ"];
		}
	}

	private void activatePlaybackRecordSettings(bool playback, bool ignore_temp_changes = false)
	{
		if (playback)
		{
			if (!ignore_temp_changes)
			{
				if (GetPlaybackSetting("TXEQ") && _console.TXEQ)
				{
					_console.TXEQ = false;
				}
				if (GetPlaybackSetting("COMP") && _console.CPDR)
				{
					_console.CPDR = false;
				}
				if (GetPlaybackSetting("CFC") && _console.CFCEnabled)
				{
					_console.CFCEnabled = false;
				}
				if (GetPlaybackSetting("PHASE") && _console.PhaseRotEnabled)
				{
					_console.PhaseRotEnabled = false;
				}
				if (GetPlaybackSetting("LEVELER") && _console.LevelerEnabled)
				{
					_console.LevelerEnabled = false;
				}
			}
			if (GetPlaybackSetting("MON") && !_console.MON)
			{
				_console.MON = true;
			}
			Audio.VACBypass = _console.BypassVACWhenPlayingWAV;
			if (!_console.MOX && MoxOnPlayback)
			{
				_console.MOX = true;
			}
		}
		else if (!ignore_temp_changes && GetPlaybackSetting("RXEQ") && _console.RXEQ)
		{
			_console.RXEQ = false;
		}
	}

	private void OnPreMox(int rx, bool oldMox, bool newMox)
	{
		if (oldMox != newMox)
		{
			string error;
			try
			{
				StopRecord(out error);
			}
			catch
			{
			}
			try
			{
				StopPlayback(out error);
			}
			catch
			{
			}
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		string error;
		try
		{
			StopRecord(out error);
		}
		catch
		{
		}
		try
		{
			StopPlayback(out error);
		}
		catch
		{
		}
		lock (_sync)
		{
			if (_pc_wave_in != null)
			{
				try
				{
					_pc_wave_in.DataAvailable -= onPcDataAvailable;
				}
				catch
				{
				}
				try
				{
					_pc_wave_in.RecordingStopped -= onPcRecordingStopped;
				}
				catch
				{
				}
				try
				{
					_pc_wave_in.Dispose();
				}
				catch
				{
				}
				_pc_wave_in = null;
			}
			if (_pc_wave_writer != null)
			{
				try
				{
					_pc_wave_writer.Dispose();
				}
				catch
				{
				}
				_pc_wave_writer = null;
			}
		}
		stopRecordSpaceTimer();
		if (_record_space_timer != null)
		{
			try
			{
				_record_space_timer.Dispose();
			}
			catch
			{
			}
			_record_space_timer = null;
		}
		Console console = _console;
		console.MoxPreChangeHandlers = (Console.MoxPreChanged)Delegate.Remove(console.MoxPreChangeHandlers, new Console.MoxPreChanged(OnPreMox));
	}

	public bool OkToRecord(string filepath, bool allowUnknown = true)
	{
		if (Common.TryGetDriveTotalAndFreeBytes(filepath, out var totalBytes, out var freeBytes) && totalBytes != 0)
		{
			ulong num = freeBytes * 100 / totalBytes;
			if (num > 100)
			{
				num = 100uL;
			}
			return num >= (ulong)_free_space_perc;
		}
		return allowUnknown;
	}

	private void startRecordSpaceTimer()
	{
		_record_space_timer?.Change(2000, 2000);
	}

	private void stopRecordSpaceTimer()
	{
		Timer record_space_timer = _record_space_timer;
		if (record_space_timer != null)
		{
			try
			{
				record_space_timer.Change(-1, -1);
			}
			catch
			{
			}
			Interlocked.Exchange(ref _record_space_timer_busy, 0);
		}
	}

	private void onRecordSpaceTimer(object state)
	{
		if (Interlocked.Exchange(ref _record_space_timer_busy, 1) != 0)
		{
			return;
		}
		try
		{
			bool is_recording;
			string active_record_filename;
			lock (_sync)
			{
				is_recording = _is_recording;
				active_record_filename = _active_record_filename;
			}
			if (is_recording && !string.IsNullOrWhiteSpace(active_record_filename) && !OkToRecord(active_record_filename, allowUnknown: false))
			{
				StopRecord(out var _);
			}
		}
		catch
		{
		}
		finally
		{
			Interlocked.Exchange(ref _record_space_timer_busy, 0);
		}
	}

	public List<AudioDeviceInfo> GetPcInputDevices()
	{
		List<AudioDeviceInfo> list = new List<AudioDeviceInfo>();
		list.Add(new AudioDeviceInfo(-1, "Default Windows Sound Mapper - Input", AudioDeviceDriver.MME));
		try
		{
			MMDeviceCollection mMDeviceCollection = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
			for (int i = 0; i < mMDeviceCollection.Count; i++)
			{
				string text = mMDeviceCollection[i]?.FriendlyName;
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "Input " + i.ToString(CultureInfo.InvariantCulture);
				}
				list.Add(new AudioDeviceInfo(100000 + i, text, AudioDeviceDriver.WASAPI));
			}
		}
		catch
		{
		}
		try
		{
			int deviceCount = WaveIn.DeviceCount;
			for (int j = 0; j < deviceCount; j++)
			{
				string text2 = WaveIn.GetCapabilities(j).ProductName;
				if (string.IsNullOrWhiteSpace(text2))
				{
					text2 = "Input " + j.ToString(CultureInfo.InvariantCulture);
				}
				list.Add(new AudioDeviceInfo(j, text2, AudioDeviceDriver.MME));
			}
		}
		catch
		{
		}
		return list;
	}

	public List<AudioDeviceInfo> GetPcOutputDevices()
	{
		List<AudioDeviceInfo> list = new List<AudioDeviceInfo>();
		list.Add(new AudioDeviceInfo(-1, "Default Windows Sound Mapper - Output", AudioDeviceDriver.MME));
		try
		{
			MMDeviceCollection mMDeviceCollection = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
			for (int i = 0; i < mMDeviceCollection.Count; i++)
			{
				string text = mMDeviceCollection[i]?.FriendlyName;
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "Output " + i.ToString(CultureInfo.InvariantCulture);
				}
				list.Add(new AudioDeviceInfo(200000 + i, text, AudioDeviceDriver.WASAPI));
			}
		}
		catch
		{
		}
		try
		{
			int deviceCount = WaveOut.DeviceCount;
			for (int j = 0; j < deviceCount; j++)
			{
				string text2 = WaveOut.GetCapabilities(j).ProductName;
				if (string.IsNullOrWhiteSpace(text2))
				{
					text2 = "Output " + j.ToString(CultureInfo.InvariantCulture);
				}
				list.Add(new AudioDeviceInfo(j, text2, AudioDeviceDriver.MME));
			}
		}
		catch
		{
		}
		return list;
	}

	public bool DeleteRecording(string full_path, out string error, bool delete_containing_folder_if_empty = false)
	{
		error = null;
		try
		{
			string text = resolvePlayPath(full_path, out error);
			if (text == null)
			{
				return false;
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			bool num = Directory.Exists(text);
			string text2 = null;
			if (num)
			{
				text2 = text;
				string[] files = Directory.GetFiles(text, "*.wav", SearchOption.TopDirectoryOnly);
				foreach (string text3 in files)
				{
					hashSet.Add(text3);
					hashSet.Add(Path.ChangeExtension(text3, ".json"));
					hashSet.Add(Path.ChangeExtension(text3, ".mp3"));
				}
			}
			else
			{
				string path = text;
				if (hasRealExtension(text))
				{
					string text4 = (Path.GetExtension(text) ?? string.Empty).ToLowerInvariant();
					if (text4 != ".wav" && text4 != ".json" && text4 != ".mp3")
					{
						error = "Unsupported file type.";
						return false;
					}
					string directoryName = Path.GetDirectoryName(text);
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					if (string.IsNullOrWhiteSpace(directoryName) || string.IsNullOrWhiteSpace(fileNameWithoutExtension))
					{
						return true;
					}
					path = Path.Combine(directoryName, fileNameWithoutExtension);
				}
				string text5 = Path.ChangeExtension(path, ".wav");
				string text6 = Path.ChangeExtension(path, ".json");
				string text7 = Path.ChangeExtension(path, ".mp3");
				hashSet.Add(text5);
				hashSet.Add(text6);
				hashSet.Add(text7);
				bool flag = false;
				if (File.Exists(text5))
				{
					flag = true;
				}
				else if (File.Exists(text6))
				{
					flag = true;
				}
				else if (File.Exists(text7))
				{
					flag = true;
				}
				if (!flag)
				{
					return true;
				}
				text2 = Path.GetDirectoryName(text5);
			}
			string text8 = null;
			string text9 = null;
			bool flag2 = false;
			bool flag3 = false;
			lock (_sync)
			{
				text8 = _active_record_filename;
				text9 = _active_play_filename;
				flag2 = _is_recording;
				flag3 = _is_playing;
			}
			if (flag2 && !string.IsNullOrWhiteSpace(text8) && hashSet.Contains(text8))
			{
				error = "Cannot delete an active recording.";
				return false;
			}
			if (flag3 && !string.IsNullOrWhiteSpace(text9) && hashSet.Contains(text9))
			{
				error = "Cannot delete an active playback file.";
				return false;
			}
			foreach (string item in hashSet)
			{
				if (!string.IsNullOrWhiteSpace(item) && File.Exists(item))
				{
					try
					{
						File.Delete(item);
					}
					catch
					{
					}
				}
			}
			if (delete_containing_folder_if_empty && !string.IsNullOrWhiteSpace(text2) && Directory.Exists(text2))
			{
				bool flag4 = false;
				bool flag5 = false;
				try
				{
					flag4 = isUnderBaseFolder(_audio_folder, text2);
				}
				catch
				{
					flag4 = false;
				}
				try
				{
					string a = Path.GetFullPath(_audio_folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					string b = Path.GetFullPath(text2).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					flag5 = string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
				}
				catch
				{
					flag5 = false;
				}
				if (flag4 && !flag5)
				{
					try
					{
						Directory.Delete(text2, recursive: false);
					}
					catch
					{
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			return false;
		}
	}

	private static bool hasTrailingSeparator(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		char c = path[path.Length - 1];
		if (c != Path.DirectorySeparatorChar)
		{
			return c == Path.AltDirectorySeparatorChar;
		}
		return true;
	}

	private static bool containsDirectorySeparator(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		if (path.IndexOf(Path.DirectorySeparatorChar) < 0)
		{
			return path.IndexOf(Path.AltDirectorySeparatorChar) >= 0;
		}
		return true;
	}

	private static bool hasRealExtension(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}
		string extension = Path.GetExtension(path.Trim());
		if (string.IsNullOrEmpty(extension))
		{
			return false;
		}
		if (extension == ".")
		{
			return false;
		}
		return true;
	}

	private static string ensureTrailingSeparator(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		if (hasTrailingSeparator(path))
		{
			return path;
		}
		return path + Path.DirectorySeparatorChar;
	}

	private static bool isUnderBaseFolder(string baseFolder, string candidatePath)
	{
		if (string.IsNullOrWhiteSpace(baseFolder) || string.IsNullOrWhiteSpace(candidatePath))
		{
			return false;
		}
		string fullPath = Path.GetFullPath(baseFolder);
		string fullPath2 = Path.GetFullPath(candidatePath);
		fullPath = ensureTrailingSeparator(fullPath);
		return fullPath2.StartsWith(fullPath, StringComparison.OrdinalIgnoreCase);
	}

	private string makeUniquePathInFolder(string folder, string prefix, out string filename)
	{
		ensureFolderExists(folder);
		int num = 0;
		string text;
		while (true)
		{
			filename = generateFilename(prefix, num, "wav");
			text = Path.Combine(folder, filename);
			if (!File.Exists(text))
			{
				break;
			}
			num++;
		}
		return text;
	}

	private string resolveRecordPath(string record_id, string input_path, string prefix, out string filename, out string error)
	{
		filename = null;
		error = null;
		string text = null;
		if (!string.IsNullOrWhiteSpace(input_path))
		{
			text = input_path.Trim();
		}
		if (string.IsNullOrEmpty(text))
		{
			return makeUniquePathInFolder(_audio_folder, prefix, out filename);
		}
		bool num = Path.IsPathRooted(text);
		bool flag = containsDirectorySeparator(text);
		bool flag2 = !hasTrailingSeparator(text) && hasRealExtension(text);
		if (num)
		{
			if (flag2)
			{
				string directoryName = Path.GetDirectoryName(text);
				if (string.IsNullOrWhiteSpace(directoryName))
				{
					error = "Invalid file path.";
					return null;
				}
				ensureFolderExists(directoryName);
				filename = Path.GetFileName(text);
				return text;
			}
			ensureFolderExists(text);
			return makeUniquePathInFolder(text, prefix, out filename);
		}
		if (!flag)
		{
			if (flag2)
			{
				ensureFolderExists(_audio_folder);
				filename = text;
				return Path.Combine(_audio_folder, text);
			}
			string text2 = Path.Combine(_audio_folder, text);
			if (!isUnderBaseFolder(_audio_folder, text2))
			{
				error = "Invalid path.";
				return null;
			}
			return makeUniquePathInFolder(text2, prefix, out filename);
		}
		if (flag2)
		{
			string text3 = Path.Combine(_audio_folder, text);
			if (!isUnderBaseFolder(_audio_folder, text3))
			{
				error = "Invalid path.";
				return null;
			}
			string directoryName2 = Path.GetDirectoryName(text3);
			if (string.IsNullOrWhiteSpace(directoryName2))
			{
				error = "Invalid file path.";
				return null;
			}
			ensureFolderExists(directoryName2);
			filename = Path.GetFileName(text3);
			return text3;
		}
		string text4 = Path.Combine(_audio_folder, text);
		if (!isUnderBaseFolder(_audio_folder, text4))
		{
			error = "Invalid path.";
			return null;
		}
		ensureFolderExists(text4);
		return makeUniquePathInFolder(text4, prefix, out filename);
	}

	private string resolvePlayPath(string input_path, out string error)
	{
		error = null;
		string text = null;
		if (!string.IsNullOrWhiteSpace(input_path))
		{
			text = input_path.Trim();
		}
		if (string.IsNullOrEmpty(text))
		{
			error = "Path is empty.";
			return null;
		}
		if (Path.IsPathRooted(text))
		{
			return text;
		}
		string text2 = Path.Combine(_audio_folder, text);
		if (!isUnderBaseFolder(_audio_folder, text2))
		{
			error = "Invalid path.";
			return null;
		}
		return text2;
	}

	private static DateTime ensureUtc(DateTime dt)
	{
		if (dt == default(DateTime))
		{
			return DateTime.UtcNow;
		}
		if (dt.Kind == DateTimeKind.Utc)
		{
			return dt;
		}
		if (dt.Kind == DateTimeKind.Local)
		{
			return dt.ToUniversalTime();
		}
		return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
	}

	private static RecordingDetails ensureDetails(RecordingDetails details, bool needed)
	{
		if (!needed)
		{
			return details;
		}
		if (details == null)
		{
			return new RecordingDetails
			{
				UtcTime = DateTime.UtcNow
			};
		}
		if (details.UtcTime == default(DateTime))
		{
			details.UtcTime = DateTime.UtcNow;
		}
		return details;
	}

	public bool GetJSONDetailsFromFile(string full_path_file, out RecordingJsonModel json_data)
	{
		json_data = null;
		try
		{
			if (string.IsNullOrWhiteSpace(full_path_file))
			{
				return false;
			}
			string path = full_path_file;
			if ((Path.GetExtension(path) ?? string.Empty).ToLowerInvariant() != ".wav")
			{
				path = Path.ChangeExtension(path, ".wav");
			}
			if (!File.Exists(path))
			{
				json_data = null;
				return false;
			}
			string text = Path.ChangeExtension(path, ".json");
			if (string.IsNullOrWhiteSpace(text) || !File.Exists(text))
			{
				json_data = null;
				return true;
			}
			string value = File.ReadAllText(text, Encoding.UTF8);
			if (string.IsNullOrWhiteSpace(value))
			{
				json_data = null;
				return true;
			}
			RecordingJsonModel recordingJsonModel = null;
			try
			{
				recordingJsonModel = JsonConvert.DeserializeObject<RecordingJsonModel>(value);
			}
			catch
			{
				recordingJsonModel = null;
			}
			json_data = recordingJsonModel;
			return true;
		}
		catch
		{
			json_data = null;
			return false;
		}
	}

	public string RecordToFileFromWDSP(string record_id, string full_path, int wfw_id, out string error, bool remove_if_file_exists = false, RecordingDetails details = null, bool ignore_temp_changes = false)
	{
		error = null;
		string text = null;
		lock (_sync)
		{
			if (wfw_id < 0 || wfw_id >= WaveThing.wave_file_writer.Length)
			{
				error = "Invalid WDSP ID.";
				return null;
			}
			if (_is_recording)
			{
				error = "Already recording.";
				return null;
			}
			if (SampleRate < 6000)
			{
				error = "SampleRate is invalid.";
				return null;
			}
			try
			{
				text = resolveRecordPath(record_id, full_path, "wdsp", out var _, out error);
				if (text == null)
				{
					return null;
				}
				if (remove_if_file_exists && File.Exists(text))
				{
					try
					{
						File.Delete(text);
					}
					catch
					{
					}
				}
				if (!OkToRecord(text))
				{
					error = "Not enough free space to start recording.";
					return null;
				}
				if (!Common.CanCreateFile(text))
				{
					error = "Unable to create file\n" + text + "\n\nThis may be due to controlled folder access or some other reason.";
					return null;
				}
				short bitDepth = 32;
				short formatTag = 3;
				switch (BitDepthMode)
				{
				case AudioBitDepthMode.IeeeFloat32:
					bitDepth = 32;
					formatTag = 3;
					break;
				case AudioBitDepthMode.Pcm32:
					bitDepth = 32;
					formatTag = 1;
					break;
				case AudioBitDepthMode.Pcm24:
					bitDepth = 24;
					formatTag = 1;
					break;
				case AudioBitDepthMode.Pcm16:
					bitDepth = 16;
					formatTag = 1;
					break;
				case AudioBitDepthMode.Pcm8:
					bitDepth = 8;
					formatTag = 1;
					break;
				}
				bool needed = GenerateJSON || GenerateMP3File;
				RecordingDetails recordingDetails = ensureDetails(details, needed);
				if (recordingDetails != null)
				{
					recordingDetails.UtcTime = ensureUtc(recordingDetails.UtcTime);
					recordingDetails.SampleRate = SampleRate;
					recordingDetails.BitDepth = bitDepth;
					recordingDetails.FormatTag = formatTag;
					recordingDetails.Channels = 2;
					recordingDetails.WavFile = Path.GetFileName(text) ?? "";
					recordingDetails.WavFileSizeBytes = null;
					recordingDetails.WavFileLastWriteUtc = null;
					recordingDetails.PlayDurationSeconds = null;
					recordingDetails.Mp3File = "";
					recordingDetails.Mp3FileSizeBytes = null;
				}
				_active_record_wfw_id = wfw_id;
				_active_record_id = record_id ?? string.Empty;
				_active_record_filename = text;
				_active_record_details = recordingDetails;
				_active_record_sample_rate = SampleRate;
				_active_record_json_filename = null;
				_active_record_mp3_filename = null;
				if (GenerateJSON && _active_record_details != null)
				{
					_active_record_json_filename = Path.ChangeExtension(text, ".json");
				}
				if (GenerateMP3File && _active_record_details != null)
				{
					_active_record_mp3_filename = Path.ChangeExtension(text, ".mp3");
				}
				bool recordRxPreProcessed = RxSource == AudioRecordRxSource.ReceiverInputIQ;
				bool recordTxPreProcessed = TxSource == AudioRecordTxSource.MicAudio;
				storeRestoreSettings(store: true, playback: false);
				activatePlaybackRecordSettings(playback: false, ignore_temp_changes);
				Thread.Sleep(50);
				WaveThing.wave_file_writer[_active_record_wfw_id] = new WaveFileWriter(_active_record_wfw_id, 2, SampleRate, text, recordRxPreProcessed, recordTxPreProcessed, formatTag, bitDepth, onWdspRecordFinished);
				WaveThing.wave_file_writer[_active_record_wfw_id].DitherEnabled = DitherEnabled;
				WaveThing.wave_file_writer[_active_record_wfw_id].DitherAmount = DitherAmount;
				Audio.WaveRecord = true;
				setRecordingState(recording: true);
				startRecordSpaceTimer();
				return text;
			}
			catch (Exception ex)
			{
				error = ex.Message;
				raiseRecordError(record_id, text, error);
				try
				{
					Audio.WaveRecord = false;
				}
				catch
				{
				}
				try
				{
					if (!string.IsNullOrWhiteSpace(_active_record_filename) && File.Exists(_active_record_filename))
					{
						try
						{
							File.Delete(_active_record_filename);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				try
				{
					if (!string.IsNullOrWhiteSpace(_active_record_json_filename) && File.Exists(_active_record_json_filename))
					{
						try
						{
							File.Delete(_active_record_json_filename);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				try
				{
					if (!string.IsNullOrWhiteSpace(_active_record_mp3_filename) && File.Exists(_active_record_mp3_filename))
					{
						try
						{
							File.Delete(_active_record_mp3_filename);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				stopRecordSpaceTimer();
				setRecordingState(recording: false);
				clearActiveRecordLocked();
				return null;
			}
		}
	}

	public string RecordToFileFromPCAudio(string record_id, string full_path, int pcAudioDeviceInputId, out string error, bool remove_if_file_exists = false, RecordingDetails details = null)
	{
		error = null;
		string text = null;
		lock (_sync)
		{
			if (_is_recording)
			{
				error = "Already recording.";
				return null;
			}
			if (SampleRate < 6000)
			{
				error = "SampleRate is invalid.";
				return null;
			}
			try
			{
				text = resolveRecordPath(record_id, full_path, "pc", out var _, out error);
				if (text == null)
				{
					return null;
				}
				if (remove_if_file_exists && File.Exists(text))
				{
					try
					{
						File.Delete(text);
					}
					catch
					{
					}
				}
				if (!OkToRecord(text))
				{
					error = "Not enough free space to start recording.";
					return null;
				}
				if (!Common.CanCreateFile(text))
				{
					error = "Unable to create file.";
					return null;
				}
				int num = 2;
				int num2 = 16;
				WaveFormat waveFormat;
				switch (BitDepthMode)
				{
				case AudioBitDepthMode.IeeeFloat32:
					num2 = 32;
					waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, num);
					break;
				case AudioBitDepthMode.Pcm32:
					num2 = 32;
					waveFormat = new WaveFormat(SampleRate, num2, num);
					break;
				case AudioBitDepthMode.Pcm24:
					num2 = 24;
					waveFormat = new WaveFormat(SampleRate, num2, num);
					break;
				case AudioBitDepthMode.Pcm16:
					num2 = 16;
					waveFormat = new WaveFormat(SampleRate, num2, num);
					break;
				case AudioBitDepthMode.Pcm8:
					num2 = 8;
					waveFormat = new WaveFormat(SampleRate, num2, num);
					break;
				default:
					num2 = 16;
					waveFormat = new WaveFormat(SampleRate, num2, num);
					break;
				}
				bool needed = GenerateJSON || GenerateMP3File;
				RecordingDetails recordingDetails = ensureDetails(details, needed);
				if (recordingDetails != null)
				{
					recordingDetails.UtcTime = ensureUtc(recordingDetails.UtcTime);
					recordingDetails.SampleRate = SampleRate;
					recordingDetails.BitDepth = (short)num2;
					recordingDetails.FormatTag = (short)((BitDepthMode != AudioBitDepthMode.IeeeFloat32) ? 1 : 3);
					recordingDetails.Channels = (short)num;
					recordingDetails.WavFile = Path.GetFileName(text) ?? "";
					recordingDetails.WavFileSizeBytes = null;
					recordingDetails.WavFileLastWriteUtc = null;
					recordingDetails.PlayDurationSeconds = null;
					recordingDetails.Mp3File = "";
					recordingDetails.Mp3FileSizeBytes = null;
				}
				_active_record_wfw_id = -1;
				_active_record_id = record_id ?? string.Empty;
				_active_record_filename = text;
				_active_record_details = recordingDetails;
				_active_record_sample_rate = SampleRate;
				_active_record_json_filename = null;
				_active_record_mp3_filename = null;
				if (GenerateJSON && _active_record_details != null)
				{
					_active_record_json_filename = Path.ChangeExtension(text, ".json");
				}
				if (GenerateMP3File && _active_record_details != null)
				{
					_active_record_mp3_filename = Path.ChangeExtension(text, ".mp3");
				}
				IWaveIn pc_wave_in;
				if (pcAudioDeviceInputId >= 100000)
				{
					int num3 = pcAudioDeviceInputId - 100000;
					MMDeviceCollection mMDeviceCollection = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
					if (num3 < 0 || num3 >= mMDeviceCollection.Count)
					{
						throw new IndexOutOfRangeException("WASAPI input device index is out of range.");
					}
					pc_wave_in = new WasapiCapture(mMDeviceCollection[num3])
					{
						WaveFormat = waveFormat
					};
				}
				else
				{
					pc_wave_in = new WaveInEvent
					{
						DeviceNumber = pcAudioDeviceInputId,
						WaveFormat = waveFormat,
						BufferMilliseconds = 50,
						NumberOfBuffers = 3
					};
				}
				_pc_wave_in = pc_wave_in;
				_pc_wave_in.DataAvailable += onPcDataAvailable;
				_pc_wave_in.RecordingStopped += onPcRecordingStopped;
				_pc_wave_writer = new NAudio.Wave.WaveFileWriter(text, waveFormat);
				Thread.Sleep(50);
				_pc_wave_in.StartRecording();
				setRecordingState(recording: true);
				startRecordSpaceTimer();
				return text;
			}
			catch (Exception ex)
			{
				error = ex.Message;
				raiseRecordError(record_id, text, error);
				try
				{
					cleanupPcRecording();
				}
				catch
				{
				}
				try
				{
					if (!string.IsNullOrWhiteSpace(_active_record_filename) && File.Exists(_active_record_filename))
					{
						try
						{
							File.Delete(_active_record_filename);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				try
				{
					if (!string.IsNullOrWhiteSpace(_active_record_json_filename) && File.Exists(_active_record_json_filename))
					{
						try
						{
							File.Delete(_active_record_json_filename);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				try
				{
					if (!string.IsNullOrWhiteSpace(_active_record_mp3_filename) && File.Exists(_active_record_mp3_filename))
					{
						try
						{
							File.Delete(_active_record_mp3_filename);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				stopRecordSpaceTimer();
				setRecordingState(recording: false);
				clearActiveRecordLocked();
				return null;
			}
		}
	}

	public bool StopRecord(out string error)
	{
		error = null;
		stopRecordSpaceTimer();
		string unique_id = null;
		string text = null;
		WaveFileWriter waveFileWriter = null;
		lock (_sync)
		{
			if (!_is_recording)
			{
				return true;
			}
			unique_id = _active_record_id;
			text = _active_record_filename;
			try
			{
				Audio.WaveRecord = false;
				if (_active_record_wfw_id >= 0)
				{
					waveFileWriter = WaveThing.wave_file_writer[_active_record_wfw_id];
					waveFileWriter?.Stop();
				}
				if (_pc_wave_in != null)
				{
					try
					{
						_pc_wave_in.StopRecording();
					}
					catch
					{
					}
				}
			}
			catch (Exception ex)
			{
				error = ex.Message;
				try
				{
					if (!string.IsNullOrWhiteSpace(_active_record_json_filename) && File.Exists(_active_record_json_filename))
					{
						try
						{
							File.Delete(_active_record_json_filename);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				setRecordingState(recording: false);
				clearActiveRecordLocked();
			}
		}
		if (waveFileWriter != null)
		{
			try
			{
				if (waveFileWriter.WaitForStop(2000))
				{
					completeRecordStateIfCurrent(unique_id, text);
				}
			}
			catch
			{
			}
		}
		storeRestoreSettings(store: false, playback: false);
		if (!string.IsNullOrWhiteSpace(error))
		{
			raiseRecordError(unique_id, text, error);
		}
		return error == null;
	}

	private void applyPcInputSourceStereoRemap(byte[] buffer, int bytesRecorded)
	{
		if (buffer == null || bytesRecorded < 1)
		{
			return;
		}
		PCInputSource pCInputSource = PCInputSource;
		if (pCInputSource == PCInputSource.Both)
		{
			return;
		}
		int num = ((BitDepthMode == AudioBitDepthMode.Pcm8) ? 1 : ((BitDepthMode == AudioBitDepthMode.Pcm16) ? 2 : ((BitDepthMode != AudioBitDepthMode.Pcm24) ? 4 : 3)));
		int num2 = num * 2;
		int i = 0;
		if (pCInputSource == PCInputSource.Left)
		{
			for (; i + num2 - 1 < bytesRecorded; i += num2)
			{
				for (int j = 0; j < num; j++)
				{
					buffer[i + num + j] = buffer[i + j];
				}
			}
		}
		else
		{
			if (pCInputSource != PCInputSource.Right)
			{
				return;
			}
			for (; i + num2 - 1 < bytesRecorded; i += num2)
			{
				for (int k = 0; k < num; k++)
				{
					buffer[i + k] = buffer[i + num + k];
				}
			}
		}
	}

	public bool CanBePlayed(string filepath)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(filepath))
			{
				return false;
			}
			string text = resolvePlayPath(filepath, out var _);
			if (text == null)
			{
				return false;
			}
			if (!File.Exists(text))
			{
				return false;
			}
			if ((Path.GetExtension(text) ?? string.Empty).ToLowerInvariant() == ".wav")
			{
				using (BinaryReader reader = new BinaryReader(File.Open(text, FileMode.Open, FileAccess.Read, FileShare.Read)))
				{
					if (!tryParseWaveHeader(reader, out var _, out var sampleRate, out var channels, out var _, out var _, out var dataLengthBytes, out var _))
					{
						return false;
					}
					if (channels < 1 || channels > 2)
					{
						return false;
					}
					if (sampleRate < 6000)
					{
						return false;
					}
					if (dataLengthBytes <= 0)
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public bool PlayFileViaWDSP(string play_id, string full_path, int wfw_id, out string error, double adjustGain_dB = 0.0, bool ignore_temp_changes = false)
	{
		bool result = false;
		bool flag = false;
		error = null;
		string text = null;
		lock (_sync)
		{
			if (wfw_id < 0 || wfw_id >= WaveThing.wave_file_reader.Length)
			{
				error = "Invalid WDSP ID.";
				return false;
			}
			if (_is_playing)
			{
				error = "Already playing.";
				return false;
			}
			try
			{
				text = resolvePlayPath(full_path, out error);
				if (text == null)
				{
					return false;
				}
				if (!File.Exists(text))
				{
					error = "File does not exist.";
					return false;
				}
				BinaryReader binaryReader = null;
				try
				{
					binaryReader = new BinaryReader(File.Open(text, FileMode.Open, FileAccess.Read, FileShare.Read));
				}
				catch (Exception ex)
				{
					error = ex.Message;
					raisePlaybackError(play_id, text, error);
					return false;
				}
				if (!tryParseWaveHeader(binaryReader, out var formatTag, out var sampleRate, out var channels, out var bitsPerSample, out var _, out var dataLengthBytes, out var error2))
				{
					try
					{
						binaryReader.Close();
					}
					catch
					{
					}
					error = error2;
					return false;
				}
				try
				{
					refreshExistingJsonFromWavIfNeeded(play_id, text, formatTag, sampleRate, channels, bitsPerSample);
				}
				catch
				{
				}
				storeRestoreSettings(store: true, playback: true);
				double micPreamp = Audio.MicPreamp;
				double wavePreamp = Audio.WavePreamp;
				_ = Audio.WavePreampAdjust;
				Audio.MicPreamp = 0.0;
				Audio.WavePreamp = 0.0;
				Audio.WavePreampAdjust = 0.0;
				activatePlaybackRecordSettings(playback: true, ignore_temp_changes);
				_active_playback_wfw_id = wfw_id;
				_active_play_id = play_id ?? string.Empty;
				_active_play_filename = text;
				_is_wdsp_playing = true;
				WaveThing.wave_file_reader[_active_playback_wfw_id] = new WaveFileReader1(_active_playback_wfw_id, formatTag, sampleRate, channels, bitsPerSample, dataLengthBytes, fade_enabled: true, 50, MonoToStereoGainDb, binaryReader, onWdspPlaybackFinished);
				_console.SetWavePlayback(wfw_id, enabled: true);
				Audio.WavePlayback = true;
				setPlayingState(playing: true);
				Audio.MicPreamp = micPreamp;
				Audio.WavePreamp = wavePreamp;
				Audio.WavePreampAdjust = Math.Pow(10.0, adjustGain_dB / 20.0);
				result = true;
			}
			catch (Exception ex2)
			{
				error = ex2.Message;
				raisePlaybackError(play_id, text, error);
				try
				{
					_console.SetWavePlayback(wfw_id, enabled: false);
					Audio.WavePlayback = false;
				}
				catch
				{
				}
				setPlayingState(playing: false);
				flag = true;
			}
		}
		if (flag)
		{
			storeRestoreSettings(store: false, playback: true);
		}
		return result;
	}

	private double adjustWavePreampByDB(double nonDBfloor, double db_adjust)
	{
		double num = 20.0 * Math.Log10(nonDBfloor) + db_adjust;
		if (num < -70.0)
		{
			num = -70.0;
		}
		if (num > 70.0)
		{
			num = 70.0;
		}
		return Math.Pow(10.0, num / 20.0);
	}

	public bool PlayFileViaPCAudio(string play_id, string full_path, int pcAudioDeviceOutputId, out string error)
	{
		bool result = false;
		error = null;
		string text = null;
		lock (_sync)
		{
			if (_is_playing)
			{
				error = "Already playing.";
				return false;
			}
			try
			{
				text = resolvePlayPath(full_path, out error);
				if (text == null)
				{
					return false;
				}
				if (!File.Exists(text))
				{
					error = "File does not exist.";
					return false;
				}
				if (string.Equals(Path.GetExtension(text) ?? string.Empty, ".wav", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						using BinaryReader reader = new BinaryReader(File.Open(text, FileMode.Open, FileAccess.Read, FileShare.Read));
						if (tryParseWaveHeader(reader, out var formatTag, out var sampleRate, out var channels, out var bitsPerSample, out var _, out var _, out var _))
						{
							if (channels < 1 || channels > 2)
							{
								error = "Unsupported channel count.";
								return false;
							}
							refreshExistingJsonFromWavIfNeeded(play_id, text, formatTag, sampleRate, channels, bitsPerSample);
						}
					}
					catch
					{
					}
				}
				_active_play_id = play_id ?? string.Empty;
				_active_play_filename = text;
				_is_wdsp_playing = false;
				_pc_audio_reader = new AudioFileReader(text);
				_pc_audio_reader.Volume = _pc_playback_gain;
				IWavePlayer pc_wave_out;
				if (pcAudioDeviceOutputId >= 200000)
				{
					int num = pcAudioDeviceOutputId - 200000;
					MMDeviceCollection mMDeviceCollection = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
					if (num < 0 || num >= mMDeviceCollection.Count)
					{
						throw new IndexOutOfRangeException("WASAPI output device index is out of range.");
					}
					pc_wave_out = new WasapiOut(mMDeviceCollection[num], AudioClientShareMode.Shared, useEventSync: false, 200);
				}
				else
				{
					pc_wave_out = new WaveOutEvent
					{
						DeviceNumber = pcAudioDeviceOutputId
					};
				}
				_pc_wave_out = pc_wave_out;
				_pc_wave_out.PlaybackStopped += pcWaveOut_PlaybackStopped;
				_pc_wave_out.Init(_pc_audio_reader);
				_pc_wave_out.Play();
				setPlayingState(playing: true);
				result = true;
			}
			catch (Exception ex)
			{
				error = ex.Message;
				raisePlaybackError(play_id, text, error);
				try
				{
					cleanupPcPlayback();
				}
				catch
				{
				}
				setPlayingState(playing: false);
			}
		}
		return result;
	}

	private void pcWaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
	{
		string active_play_id;
		string active_play_filename;
		lock (_sync)
		{
			active_play_id = _active_play_id;
			active_play_filename = _active_play_filename;
		}
		StopPlayback(out var error);
		string text = null;
		if (e != null && e.Exception != null)
		{
			text = getMediaFailureMessage(e.Exception);
		}
		if (!string.IsNullOrWhiteSpace(error))
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				text = error;
			}
			else if (text.IndexOf(error, StringComparison.Ordinal) < 0)
			{
				text = text + Environment.NewLine + error;
			}
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			raisePlaybackError(active_play_id, active_play_filename, text);
		}
	}

	private void cleanupPcPlayback()
	{
		if (_pc_wave_out != null)
		{
			try
			{
				_pc_wave_out.PlaybackStopped -= pcWaveOut_PlaybackStopped;
			}
			catch
			{
			}
			try
			{
				_pc_wave_out.Stop();
			}
			catch
			{
			}
			try
			{
				_pc_wave_out.Dispose();
			}
			catch
			{
			}
			_pc_wave_out = null;
		}
		if (_pc_audio_reader != null)
		{
			try
			{
				_pc_audio_reader.Dispose();
			}
			catch
			{
			}
			_pc_audio_reader = null;
		}
	}

	public bool StopPlayback(out string error)
	{
		error = null;
		bool flag = false;
		string unique_id = null;
		string filename = null;
		WaveFileReader1 waveFileReader = null;
		lock (_sync)
		{
			if (!_is_playing)
			{
				return true;
			}
			unique_id = _active_play_id;
			filename = _active_play_filename;
			try
			{
				if (_active_playback_wfw_id >= 0)
				{
					_console.SetWavePlayback(_active_playback_wfw_id, enabled: false);
					Audio.WavePlayback = false;
					waveFileReader = WaveThing.wave_file_reader[_active_playback_wfw_id];
					if (waveFileReader != null)
					{
						waveFileReader.Stop();
						WaveThing.wave_file_reader[_active_playback_wfw_id] = null;
					}
					_active_playback_wfw_id = -1;
					flag = true;
				}
				cleanupPcPlayback();
			}
			catch (Exception ex)
			{
				error = ex.Message;
				try
				{
					cleanupPcPlayback();
				}
				catch
				{
				}
			}
		}
		if (waveFileReader != null)
		{
			try
			{
				waveFileReader.WaitForStop(2000);
			}
			catch
			{
			}
		}
		if (flag)
		{
			storeRestoreSettings(store: false, playback: true);
		}
		setPlayingState(playing: false);
		if (!string.IsNullOrWhiteSpace(error))
		{
			raisePlaybackError(unique_id, filename, error);
		}
		return error == null;
	}

	private static string getMediaFailureMessage(Exception ex)
	{
		if (ex == null)
		{
			return null;
		}
		string text = ex.Message;
		if (string.IsNullOrWhiteSpace(text))
		{
			text = ex.GetType().Name;
		}
		return text;
	}

	private void markActiveRecordFailureLocked(Exception ex)
	{
		_active_record_failed = true;
		string mediaFailureMessage = getMediaFailureMessage(ex);
		if (!string.IsNullOrWhiteSpace(mediaFailureMessage))
		{
			if (string.IsNullOrWhiteSpace(_active_record_failure_message))
			{
				_active_record_failure_message = mediaFailureMessage;
			}
			else if (_active_record_failure_message.IndexOf(mediaFailureMessage, StringComparison.Ordinal) < 0)
			{
				_active_record_failure_message = _active_record_failure_message + Environment.NewLine + mediaFailureMessage;
			}
		}
	}

	private void stopFaultedPcRecording(Exception ex)
	{
		IWaveIn waveIn = null;
		string wav = null;
		string json = null;
		string mp = null;
		RecordingDetails details = null;
		string unique_id = null;
		string failureMessage = null;
		bool flag = false;
		lock (_sync)
		{
			if (!_is_recording)
			{
				return;
			}
			if (ex != null)
			{
				markActiveRecordFailureLocked(ex);
			}
			if (!_active_record_failed)
			{
				return;
			}
			waveIn = _pc_wave_in;
			if (waveIn == null)
			{
				wav = _active_record_filename;
				json = _active_record_json_filename;
				mp = _active_record_mp3_filename;
				details = _active_record_details;
				unique_id = _active_record_id;
				failureMessage = _active_record_failure_message;
				cleanupPcRecording();
				flag = true;
			}
		}
		if (waveIn != null)
		{
			try
			{
				waveIn.StopRecording();
				return;
			}
			catch (Exception ex2)
			{
				lock (_sync)
				{
					if (!_is_recording)
					{
						return;
					}
					markActiveRecordFailureLocked(ex2);
					wav = _active_record_filename;
					json = _active_record_json_filename;
					mp = _active_record_mp3_filename;
					details = _active_record_details;
					unique_id = _active_record_id;
					failureMessage = _active_record_failure_message;
					cleanupPcRecording();
					flag = true;
				}
			}
		}
		if (flag)
		{
			recordCompleted(unique_id, wav, json, mp, details, recordSucceeded: false, failureMessage);
		}
	}

	private void onPcDataAvailable(object sender, WaveInEventArgs e)
	{
		Exception ex = null;
		lock (_sync)
		{
			if (_pc_wave_writer == null || _active_record_failed)
			{
				return;
			}
			try
			{
				float pc_input_gain = _pc_input_gain;
				if (pc_input_gain <= 0f)
				{
					if (_pc_record_gain_buffer == null || _pc_record_gain_buffer.Length < e.BytesRecorded)
					{
						_pc_record_gain_buffer = new byte[e.BytesRecorded];
					}
					if (BitDepthMode == AudioBitDepthMode.Pcm8)
					{
						for (int i = 0; i < e.BytesRecorded; i++)
						{
							_pc_record_gain_buffer[i] = 128;
						}
					}
					else
					{
						Array.Clear(_pc_record_gain_buffer, 0, e.BytesRecorded);
					}
					_pc_wave_writer.Write(_pc_record_gain_buffer, 0, e.BytesRecorded);
					_pc_wave_writer.Flush();
					return;
				}
				PCInputSource pCInputSource = PCInputSource;
				if (pc_input_gain == 1f && pCInputSource == PCInputSource.Both)
				{
					_pc_wave_writer.Write(e.Buffer, 0, e.BytesRecorded);
					_pc_wave_writer.Flush();
					return;
				}
				if (_pc_record_gain_buffer == null || _pc_record_gain_buffer.Length < e.BytesRecorded)
				{
					_pc_record_gain_buffer = new byte[e.BytesRecorded];
				}
				Buffer.BlockCopy(e.Buffer, 0, _pc_record_gain_buffer, 0, e.BytesRecorded);
				if (pCInputSource != PCInputSource.Both)
				{
					applyPcInputSourceStereoRemap(_pc_record_gain_buffer, e.BytesRecorded);
				}
				if (pc_input_gain == 1f)
				{
					_pc_wave_writer.Write(_pc_record_gain_buffer, 0, e.BytesRecorded);
					_pc_wave_writer.Flush();
					return;
				}
				int bytesRecorded = e.BytesRecorded;
				if (BitDepthMode == AudioBitDepthMode.IeeeFloat32)
				{
					for (int j = 0; j + 3 < bytesRecorded; j += 4)
					{
						float num = BitConverter.ToSingle(_pc_record_gain_buffer, j) * pc_input_gain;
						if (num > 1f)
						{
							num = 1f;
						}
						if (num < -1f)
						{
							num = -1f;
						}
						byte[] bytes = BitConverter.GetBytes(num);
						_pc_record_gain_buffer[j] = bytes[0];
						_pc_record_gain_buffer[j + 1] = bytes[1];
						_pc_record_gain_buffer[j + 2] = bytes[2];
						_pc_record_gain_buffer[j + 3] = bytes[3];
					}
				}
				else if (BitDepthMode == AudioBitDepthMode.Pcm32)
				{
					for (int k = 0; k + 3 < bytesRecorded; k += 4)
					{
						double num2 = (double)BitConverter.ToInt32(_pc_record_gain_buffer, k) * (double)pc_input_gain;
						if (num2 > 2147483647.0)
						{
							num2 = 2147483647.0;
						}
						if (num2 < -2147483648.0)
						{
							num2 = -2147483648.0;
						}
						int num3 = (int)num2;
						_pc_record_gain_buffer[k] = (byte)(num3 & 0xFF);
						_pc_record_gain_buffer[k + 1] = (byte)((num3 >> 8) & 0xFF);
						_pc_record_gain_buffer[k + 2] = (byte)((num3 >> 16) & 0xFF);
						_pc_record_gain_buffer[k + 3] = (byte)((num3 >> 24) & 0xFF);
					}
				}
				else if (BitDepthMode == AudioBitDepthMode.Pcm24)
				{
					for (int l = 0; l + 2 < bytesRecorded; l += 3)
					{
						int num4 = _pc_record_gain_buffer[l] | (_pc_record_gain_buffer[l + 1] << 8) | (_pc_record_gain_buffer[l + 2] << 16);
						if ((num4 & 0x800000) != 0)
						{
							num4 |= -16777216;
						}
						double num5 = (double)num4 * (double)pc_input_gain;
						if (num5 > 8388607.0)
						{
							num5 = 8388607.0;
						}
						if (num5 < -8388608.0)
						{
							num5 = -8388608.0;
						}
						int num6 = (int)num5;
						_pc_record_gain_buffer[l] = (byte)(num6 & 0xFF);
						_pc_record_gain_buffer[l + 1] = (byte)((num6 >> 8) & 0xFF);
						_pc_record_gain_buffer[l + 2] = (byte)((num6 >> 16) & 0xFF);
					}
				}
				else if (BitDepthMode == AudioBitDepthMode.Pcm8)
				{
					for (int m = 0; m < bytesRecorded; m++)
					{
						double num7 = (double)(_pc_record_gain_buffer[m] - 128) * (double)pc_input_gain;
						if (num7 > 127.0)
						{
							num7 = 127.0;
						}
						if (num7 < -128.0)
						{
							num7 = -128.0;
						}
						int num8 = (int)num7 + 128;
						_pc_record_gain_buffer[m] = (byte)num8;
					}
				}
				else
				{
					for (int n = 0; n + 1 < bytesRecorded; n += 2)
					{
						float num9 = (float)(short)(_pc_record_gain_buffer[n] | (_pc_record_gain_buffer[n + 1] << 8)) * pc_input_gain;
						if (num9 > 32767f)
						{
							num9 = 32767f;
						}
						if (num9 < -32768f)
						{
							num9 = -32768f;
						}
						short num10 = (short)num9;
						_pc_record_gain_buffer[n] = (byte)(num10 & 0xFF);
						_pc_record_gain_buffer[n + 1] = (byte)((num10 >> 8) & 0xFF);
					}
				}
				_pc_wave_writer.Write(_pc_record_gain_buffer, 0, e.BytesRecorded);
				_pc_wave_writer.Flush();
			}
			catch (Exception ex2)
			{
				markActiveRecordFailureLocked(ex2);
				ex = ex2;
			}
		}
		if (ex != null)
		{
			stopFaultedPcRecording(ex);
		}
	}

	private void onPcRecordingStopped(object sender, StoppedEventArgs e)
	{
		string active_record_filename;
		string active_record_json_filename;
		string active_record_mp3_filename;
		RecordingDetails active_record_details;
		string active_record_id;
		bool recordSucceeded;
		string active_record_failure_message;
		lock (_sync)
		{
			if (e != null && e.Exception != null)
			{
				markActiveRecordFailureLocked(e.Exception);
			}
			active_record_filename = _active_record_filename;
			active_record_json_filename = _active_record_json_filename;
			active_record_mp3_filename = _active_record_mp3_filename;
			active_record_details = _active_record_details;
			active_record_id = _active_record_id;
			recordSucceeded = !_active_record_failed;
			active_record_failure_message = _active_record_failure_message;
			cleanupPcRecording();
		}
		recordCompleted(active_record_id, active_record_filename, active_record_json_filename, active_record_mp3_filename, active_record_details, recordSucceeded, active_record_failure_message);
	}

	private void cleanupPcRecording()
	{
		if (_pc_wave_in != null)
		{
			try
			{
				_pc_wave_in.DataAvailable -= onPcDataAvailable;
			}
			catch
			{
			}
			try
			{
				_pc_wave_in.RecordingStopped -= onPcRecordingStopped;
			}
			catch
			{
			}
			try
			{
				_pc_wave_in.Dispose();
			}
			catch
			{
			}
			_pc_wave_in = null;
		}
		if (_pc_wave_writer != null)
		{
			try
			{
				_pc_wave_writer.Dispose();
			}
			catch
			{
			}
			_pc_wave_writer = null;
		}
	}

	private void onWdspPlaybackFinished(Exception ex)
	{
		string active_play_id;
		string active_play_filename;
		lock (_sync)
		{
			active_play_id = _active_play_id;
			active_play_filename = _active_play_filename;
		}
		StopPlayback(out var error);
		string text = getMediaFailureMessage(ex);
		if (!string.IsNullOrWhiteSpace(error))
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				text = error;
			}
			else if (text.IndexOf(error, StringComparison.Ordinal) < 0)
			{
				text = text + Environment.NewLine + error;
			}
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			raisePlaybackError(active_play_id, active_play_filename, text);
		}
	}

	private void onWdspRecordFinished(string wavPath, Exception ex)
	{
		if (ex != null)
		{
			try
			{
				Audio.WaveRecord = false;
			}
			catch
			{
			}
		}
		string active_record_filename;
		string active_record_json_filename;
		string active_record_mp3_filename;
		RecordingDetails active_record_details;
		string active_record_id;
		bool recordSucceeded;
		string active_record_failure_message;
		lock (_sync)
		{
			if (string.IsNullOrWhiteSpace(_active_record_filename) || !string.Equals(_active_record_filename, wavPath, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			if (_active_record_wfw_id >= 0 && _active_record_wfw_id < WaveThing.wave_file_writer.Length)
			{
				WaveThing.wave_file_writer[_active_record_wfw_id] = null;
			}
			if (ex != null)
			{
				markActiveRecordFailureLocked(ex);
			}
			active_record_filename = _active_record_filename;
			active_record_json_filename = _active_record_json_filename;
			active_record_mp3_filename = _active_record_mp3_filename;
			active_record_details = _active_record_details;
			active_record_id = _active_record_id;
			recordSucceeded = !_active_record_failed;
			active_record_failure_message = _active_record_failure_message;
		}
		recordCompleted(active_record_id, active_record_filename, active_record_json_filename, active_record_mp3_filename, active_record_details, recordSucceeded, active_record_failure_message);
	}

	private void recordCompleted(string unique_id, string wav, string json, string mp3, RecordingDetails details, bool recordSucceeded, string failureMessage)
	{
		Action a = delegate
		{
			if (recordSucceeded && details != null)
			{
				details.UtcTime = ensureUtc(details.UtcTime);
				if (!string.IsNullOrWhiteSpace(wav))
				{
					details.WavFile = Path.GetFileName(wav) ?? "";
					try
					{
						if (File.Exists(wav))
						{
							FileInfo fileInfo = new FileInfo(wav);
							details.WavFileSizeBytes = fileInfo.Length;
							details.WavFileLastWriteUtc = DateTime.SpecifyKind(fileInfo.LastWriteTimeUtc, DateTimeKind.Utc);
							details.PlayDurationSeconds = tryGetWavDurationSeconds(wav);
						}
						else
						{
							details.WavFileSizeBytes = null;
							details.WavFileLastWriteUtc = null;
						}
					}
					catch
					{
						details.WavFileSizeBytes = null;
					}
				}
			}
			string text = null;
			if (recordSucceeded && GenerateMP3File && details != null && !string.IsNullOrWhiteSpace(wav) && File.Exists(wav))
			{
				text = Path.ChangeExtension(wav, ".mp3");
				try
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
				}
				catch
				{
				}
				if (!generateMp3FromWav(wav, text))
				{
					try
					{
						if (File.Exists(text))
						{
							File.Delete(text);
						}
					}
					catch
					{
					}
					text = null;
				}
				else
				{
					details.Mp3File = Path.GetFileName(text) ?? "";
					try
					{
						if (File.Exists(text))
						{
							FileInfo fileInfo2 = new FileInfo(text);
							details.Mp3FileSizeBytes = fileInfo2.Length;
						}
						else
						{
							details.Mp3FileSizeBytes = null;
						}
					}
					catch
					{
						details.Mp3FileSizeBytes = null;
					}
				}
			}
			if (recordSucceeded && GenerateJSON && details != null && !string.IsNullOrWhiteSpace(json))
			{
				try
				{
					if (File.Exists(json))
					{
						File.Delete(json);
					}
				}
				catch
				{
				}
				if (!writeRecordingJson(unique_id, json, details))
				{
					try
					{
						if (File.Exists(json))
						{
							File.Delete(json);
						}
					}
					catch
					{
					}
				}
			}
			else if (!recordSucceeded && !string.IsNullOrWhiteSpace(failureMessage))
			{
				raiseRecordError(unique_id, wav, failureMessage);
			}
			completeRecordStateIfCurrent(unique_id, wav);
		};
		if (_sync_context != null)
		{
			_sync_context.Post(delegate
			{
				a();
			}, null);
		}
		else
		{
			a();
		}
	}

	private bool writeRecordingJson(string unique_id, string jsonPath, RecordingDetails details)
	{
		try
		{
			DateTime dateTime = ensureUtc(details.UtcTime);
			RecordingJsonModel recordingJsonModel = new RecordingJsonModel();
			recordingJsonModel.utc_time = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
			if (details.Frequency != null)
			{
				recordingJsonModel.frequency = details.Frequency;
			}
			if (details.Mode != null)
			{
				recordingJsonModel.mode = details.Mode;
			}
			if (details.Band != null)
			{
				recordingJsonModel.band = details.Band;
			}
			if (details.WavFile != null)
			{
				recordingJsonModel.wav_file = details.WavFile;
			}
			if (details.WavFileSizeBytes.HasValue)
			{
				recordingJsonModel.wav_file_size_bytes = details.WavFileSizeBytes.Value;
			}
			DateTime? dateTime2 = details.WavFileLastWriteUtc;
			if (!dateTime2.HasValue)
			{
				try
				{
					string path = Path.ChangeExtension(jsonPath, ".wav");
					if (File.Exists(path))
					{
						dateTime2 = DateTime.SpecifyKind(File.GetLastWriteTimeUtc(path), DateTimeKind.Utc);
					}
				}
				catch
				{
					dateTime2 = null;
				}
			}
			recordingJsonModel.wav_file_last_write_utc = (dateTime2.HasValue ? ensureUtc(dateTime2.Value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : "");
			recordingJsonModel.sample_rate = details.SampleRate;
			recordingJsonModel.bit_depth = details.BitDepth;
			recordingJsonModel.channels = details.Channels;
			recordingJsonModel.format_tag = details.FormatTag;
			string text = ((details.FormatTag == 3) ? "IEEE_FLOAT" : ((details.FormatTag == 1) ? "PCM" : details.FormatTag.ToString()));
			recordingJsonModel.tag_description = text + " " + details.BitDepth + "-bit";
			if (details.Mp3File != null)
			{
				recordingJsonModel.mp3_file = details.Mp3File;
			}
			if (details.Mp3FileSizeBytes.HasValue)
			{
				recordingJsonModel.mp3_file_size_bytes = details.Mp3FileSizeBytes.Value;
			}
			double? num = details.PlayDurationSeconds;
			if (!num.HasValue)
			{
				try
				{
					num = tryGetWavDurationSeconds(Path.ChangeExtension(jsonPath, ".wav"));
				}
				catch
				{
					num = null;
				}
			}
			if (num.HasValue)
			{
				recordingJsonModel.play_duration_seconds = num.Value;
			}
			string contents = JsonConvert.SerializeObject(recordingJsonModel, Formatting.Indented);
			string directoryName = Path.GetDirectoryName(jsonPath);
			if (!string.IsNullOrWhiteSpace(directoryName))
			{
				ensureFolderExists(directoryName);
			}
			File.WriteAllText(jsonPath, contents, Encoding.UTF8);
			raiseRecordingJsonWritten(unique_id, recordingJsonModel);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void raiseRecordingJsonWritten(string unique_id, RecordingJsonModel json_data)
	{
		Action<string, RecordingJsonModel> action = RecordingJsonWritten;
		if (action == null)
		{
			return;
		}
		string arg = unique_id ?? string.Empty;
		Delegate[] invocationList = action.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			try
			{
				((Action<string, RecordingJsonModel>)invocationList[i])(arg, json_data);
			}
			catch
			{
			}
		}
	}

	private void raiseMediaError(Action<string, string, string> handler, string unique_id, string filename, string errorMessage)
	{
		if (handler == null || string.IsNullOrWhiteSpace(errorMessage))
		{
			return;
		}
		string uid = unique_id ?? string.Empty;
		string file = filename ?? string.Empty;
		string msg = errorMessage.Trim();
		Action a = delegate
		{
			Delegate[] invocationList = handler.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				try
				{
					((Action<string, string, string>)invocationList[i])(uid, file, msg);
				}
				catch
				{
				}
			}
		};
		if (_sync_context != null)
		{
			_sync_context.Post(delegate
			{
				a();
			}, null);
		}
		else
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				a();
			});
		}
	}

	private void raiseRecordError(string unique_id, string filename, string errorMessage)
	{
		raiseMediaError(RecordError, unique_id, filename, errorMessage);
	}

	private void raisePlaybackError(string unique_id, string filename, string errorMessage)
	{
		raiseMediaError(PlaybackError, unique_id, filename, errorMessage);
	}

	private static void ensureMediaFoundation()
	{
		lock (_mf_sync)
		{
			if (_mf_started)
			{
				return;
			}
			try
			{
				MediaFoundationApi.Startup();
				_mf_started = true;
			}
			catch
			{
				_mf_started = false;
			}
		}
	}

	private bool generateMp3FromWav(string wavPath, string mp3Path)
	{
		try
		{
			ensureMediaFoundation();
			if (!_mf_started)
			{
				return false;
			}
			using (AudioFileReader inputProvider = new AudioFileReader(wavPath))
			{
				MediaFoundationEncoder.EncodeToMp3(inputProvider, mp3Path);
			}
			return File.Exists(mp3Path);
		}
		catch
		{
			return false;
		}
	}

	private void clearActiveRecordLocked()
	{
		_active_record_id = null;
		_active_record_filename = null;
		_active_record_json_filename = null;
		_active_record_mp3_filename = null;
		_active_record_details = null;
		_active_record_wfw_id = -1;
		_active_record_sample_rate = 0;
		_active_record_failed = false;
		_active_record_failure_message = null;
	}

	private bool isActiveRecordMatchLocked(string unique_id, string wav)
	{
		if (string.Equals(_active_record_id ?? string.Empty, unique_id ?? string.Empty, StringComparison.Ordinal))
		{
			return string.Equals(_active_record_filename, wav, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private void completeRecordStateIfCurrent(string unique_id, string wav)
	{
		bool flag = false;
		string arg = null;
		string arg2 = null;
		lock (_sync)
		{
			if (!isActiveRecordMatchLocked(unique_id, wav))
			{
				return;
			}
			flag = _is_recording;
			arg = _active_record_id;
			arg2 = _active_record_filename;
			_is_recording = false;
			clearActiveRecordLocked();
		}
		stopRecordSpaceTimer();
		storeRestoreSettings(store: false, playback: false);
		if (!flag)
		{
			return;
		}
		Action<bool, string, string> action = RecordingChanged;
		if (action == null)
		{
			return;
		}
		Delegate[] invocationList = action.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			try
			{
				((Action<bool, string, string>)invocationList[i])(arg1: false, arg, arg2);
			}
			catch
			{
			}
		}
	}

	private void setRecordingState(bool recording)
	{
		bool flag = false;
		string arg = null;
		string arg2 = null;
		lock (_sync)
		{
			if (_is_recording != recording)
			{
				_is_recording = recording;
				arg = _active_record_id;
				arg2 = _active_record_filename;
				flag = true;
				if (!recording)
				{
					_active_record_wfw_id = -1;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		Action<bool, string, string> action = RecordingChanged;
		if (action == null)
		{
			return;
		}
		Delegate[] invocationList = action.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			try
			{
				((Action<bool, string, string>)invocationList[i])(recording, arg, arg2);
			}
			catch
			{
			}
		}
	}

	private void setPlayingState(bool playing)
	{
		bool flag = false;
		string arg = null;
		string arg2 = null;
		bool arg3 = false;
		lock (_sync)
		{
			if (_is_playing != playing)
			{
				_is_playing = playing;
				arg = _active_play_id;
				arg2 = _active_play_filename;
				arg3 = _is_wdsp_playing;
				flag = true;
				if (!playing)
				{
					_active_play_id = null;
					_active_play_filename = null;
					_is_wdsp_playing = false;
					_active_playback_wfw_id = -1;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		Action<bool, string, string, bool> action = PlayingChanged;
		if (action == null)
		{
			return;
		}
		Delegate[] invocationList = action.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			try
			{
				((Action<bool, string, string, bool>)invocationList[i])(playing, arg, arg2, arg3);
			}
			catch
			{
			}
		}
	}

	private static string generateFilename(string prefix, int suffixNumber, string ext)
	{
		string text = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		return prefix + "_" + text + "_" + suffixNumber + "." + ext;
	}

	private static void ensureFolderExists(string folder)
	{
		if (string.IsNullOrWhiteSpace(folder))
		{
			return;
		}
		try
		{
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
		}
		catch
		{
		}
	}

	private static bool tryParseWaveHeader(BinaryReader reader, out int formatTag, out int sampleRate, out int channels, out int bitsPerSample, out long dataStart, out long dataLengthBytes, out string error)
	{
		formatTag = 0;
		sampleRate = 0;
		channels = 0;
		bitsPerSample = 0;
		dataStart = 0L;
		dataLengthBytes = 0L;
		error = null;
		if (reader == null)
		{
			error = "Reader is null.";
			return false;
		}
		try
		{
			reader.BaseStream.Position = 0L;
			RIFFChunk rIFFChunk = null;
			fmtChunk fmtChunk2 = null;
			dataChunk dataChunk2 = null;
			long num = 0L;
			bool flag = true;
			if (reader.BaseStream.Length < 16)
			{
				error = "File is too small.";
				return false;
			}
			if (reader.PeekChar() != 82)
			{
				error = "Not a RIFF file.";
				return false;
			}
			while (flag && (dataChunk2 == null || rIFFChunk == null || fmtChunk2 == null) && reader.BaseStream.Position < reader.BaseStream.Length)
			{
				try
				{
					Chunk chunk = Chunk.ReadChunk(ref reader);
					if (chunk is RIFFChunk)
					{
						rIFFChunk = (RIFFChunk)chunk;
						continue;
					}
					if (chunk is fmtChunk)
					{
						fmtChunk2 = (fmtChunk)chunk;
						continue;
					}
					if (chunk is dataChunk)
					{
						dataChunk2 = (dataChunk)chunk;
						num = reader.BaseStream.Position;
						continue;
					}
					int num2 = 0;
					if (reader.BaseStream.Position + 4 <= reader.BaseStream.Length)
					{
						num2 = reader.ReadInt32();
						if (num2 < 0)
						{
							num2 = 0;
						}
						long num3 = reader.BaseStream.Position + num2;
						if (num3 > reader.BaseStream.Length)
						{
							num3 = reader.BaseStream.Length;
						}
						reader.BaseStream.Position = num3;
					}
					else
					{
						flag = false;
					}
				}
				catch
				{
					flag = false;
				}
			}
			if (!flag || rIFFChunk == null || fmtChunk2 == null)
			{
				error = "Unable to read wave header.";
				return false;
			}
			if (rIFFChunk.riff_type != 1163280727)
			{
				error = "Not a WAVE file.";
				return false;
			}
			formatTag = fmtChunk2.format;
			sampleRate = fmtChunk2.sample_rate;
			channels = fmtChunk2.channels;
			bitsPerSample = fmtChunk2.bits_per_sample;
			if (channels < 1 || channels > 2)
			{
				error = "Unsupported channel count.";
				return false;
			}
			if (formatTag == 3 && bitsPerSample != 32)
			{
				error = "IEEE float must be 32-bit.";
				return false;
			}
			if (formatTag == 1 && bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 24 && bitsPerSample != 32)
			{
				error = "Unsupported PCM bit depth.";
				return false;
			}
			if (dataChunk2 != null)
			{
				dataLengthBytes = dataChunk2.chunk_size;
				if (dataLengthBytes < 0)
				{
					dataLengthBytes = 0L;
				}
			}
			dataStart = num;
			if (dataStart < 0)
			{
				dataStart = 0L;
			}
			if (dataStart > reader.BaseStream.Length)
			{
				dataStart = reader.BaseStream.Length;
			}
			reader.BaseStream.Position = dataStart;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			return false;
		}
	}
}
