using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Utils;

namespace NAudio.Wave;

public class WasapiOut : IWavePlayer, IDisposable, IWavePosition
{
	private AudioClient audioClient;

	private readonly MMDevice mmDevice;

	private readonly AudioClientShareMode shareMode;

	private AudioRenderClient renderClient;

	private IWaveProvider sourceProvider;

	private int latencyMilliseconds;

	private int bufferFrameCount;

	private int bytesPerFrame;

	private readonly bool isUsingEventSync;

	private EventWaitHandle frameEventWaitHandle;

	private byte[] readBuffer;

	private volatile PlaybackState playbackState;

	private Thread playThread;

	private readonly SynchronizationContext syncContext;

	private bool dmoResamplerNeeded;

	public WaveFormat OutputWaveFormat { get; private set; }

	public PlaybackState PlaybackState => playbackState;

	public float Volume
	{
		get
		{
			return mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
			}
			if (value > 1f)
			{
				throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
			}
			mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar = value;
		}
	}

	public AudioStreamVolume AudioStreamVolume
	{
		get
		{
			if (shareMode == AudioClientShareMode.Exclusive)
			{
				throw new InvalidOperationException("AudioStreamVolume is ONLY supported for shared audio streams.");
			}
			return audioClient.AudioStreamVolume;
		}
	}

	public event EventHandler<StoppedEventArgs> PlaybackStopped;

	public WasapiOut()
		: this(GetDefaultAudioEndpoint(), AudioClientShareMode.Shared, useEventSync: true, 200)
	{
	}

	public WasapiOut(AudioClientShareMode shareMode, int latency)
		: this(GetDefaultAudioEndpoint(), shareMode, useEventSync: true, latency)
	{
	}

	public WasapiOut(AudioClientShareMode shareMode, bool useEventSync, int latency)
		: this(GetDefaultAudioEndpoint(), shareMode, useEventSync, latency)
	{
	}

	public WasapiOut(MMDevice device, AudioClientShareMode shareMode, bool useEventSync, int latency)
	{
		audioClient = device.AudioClient;
		mmDevice = device;
		this.shareMode = shareMode;
		isUsingEventSync = useEventSync;
		latencyMilliseconds = latency;
		syncContext = SynchronizationContext.Current;
		OutputWaveFormat = audioClient.MixFormat;
	}

	private static MMDevice GetDefaultAudioEndpoint()
	{
		if (Environment.OSVersion.Version.Major < 6)
		{
			throw new NotSupportedException("WASAPI supported only on Windows Vista and above");
		}
		return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
	}

	private void PlayThread()
	{
		ResamplerDmoStream resamplerDmoStream = null;
		IWaveProvider playbackProvider = sourceProvider;
		Exception e = null;
		try
		{
			if (dmoResamplerNeeded)
			{
				resamplerDmoStream = new ResamplerDmoStream(sourceProvider, OutputWaveFormat);
				playbackProvider = resamplerDmoStream;
			}
			bufferFrameCount = audioClient.BufferSize;
			bytesPerFrame = OutputWaveFormat.Channels * OutputWaveFormat.BitsPerSample / 8;
			readBuffer = BufferHelpers.Ensure(readBuffer, bufferFrameCount * bytesPerFrame);
			if (FillBuffer(playbackProvider, bufferFrameCount))
			{
				return;
			}
			WaitHandle[] waitHandles = new WaitHandle[1] { frameEventWaitHandle };
			audioClient.Start();
			while (playbackState != PlaybackState.Stopped)
			{
				if (isUsingEventSync)
				{
					WaitHandle.WaitAny(waitHandles, 3 * latencyMilliseconds, exitContext: false);
				}
				else
				{
					Thread.Sleep(latencyMilliseconds / 2);
				}
				if (playbackState == PlaybackState.Playing)
				{
					int num = ((!isUsingEventSync) ? audioClient.CurrentPadding : ((shareMode == AudioClientShareMode.Shared) ? audioClient.CurrentPadding : 0));
					int num2 = bufferFrameCount - num;
					if (num2 > 10 && FillBuffer(playbackProvider, num2))
					{
						break;
					}
				}
			}
			if (playbackState == PlaybackState.Playing)
			{
				Thread.Sleep(isUsingEventSync ? latencyMilliseconds : (latencyMilliseconds / 2));
			}
			audioClient.Stop();
			playbackState = PlaybackState.Stopped;
			audioClient.Reset();
		}
		catch (Exception ex)
		{
			e = ex;
		}
		finally
		{
			resamplerDmoStream?.Dispose();
			RaisePlaybackStopped(e);
		}
	}

	private void RaisePlaybackStopped(Exception e)
	{
		EventHandler<StoppedEventArgs> handler = PlaybackStopped;
		if (handler == null)
		{
			return;
		}
		if (syncContext == null)
		{
			handler(this, new StoppedEventArgs(e));
			return;
		}
		syncContext.Post(delegate
		{
			handler(this, new StoppedEventArgs(e));
		}, null);
	}

	private unsafe bool FillBuffer(IWaveProvider playbackProvider, int frameCount)
	{
		int num = frameCount * bytesPerFrame;
		int num2 = playbackProvider.Read(readBuffer, 0, num);
		if (num2 == 0)
		{
			return true;
		}
		IntPtr buffer = renderClient.GetBuffer(frameCount);
		Marshal.Copy(readBuffer, 0, buffer, num2);
		if (isUsingEventSync && shareMode == AudioClientShareMode.Exclusive)
		{
			if (num2 < num)
			{
				byte* ptr = (byte*)(void*)buffer;
				while (num2 < num)
				{
					ptr[num2++] = 0;
				}
			}
			renderClient.ReleaseBuffer(frameCount, AudioClientBufferFlags.None);
		}
		else
		{
			int numFramesWritten = num2 / bytesPerFrame;
			renderClient.ReleaseBuffer(numFramesWritten, AudioClientBufferFlags.None);
		}
		return false;
	}

	private WaveFormat GetFallbackFormat()
	{
		int sampleRate = audioClient.MixFormat.SampleRate;
		int channels = audioClient.MixFormat.Channels;
		List<int> list = new List<int> { OutputWaveFormat.SampleRate };
		if (!list.Contains(sampleRate))
		{
			list.Add(sampleRate);
		}
		if (!list.Contains(44100))
		{
			list.Add(44100);
		}
		if (!list.Contains(48000))
		{
			list.Add(48000);
		}
		List<int> list2 = new List<int> { OutputWaveFormat.Channels };
		if (!list2.Contains(channels))
		{
			list2.Add(channels);
		}
		if (!list2.Contains(2))
		{
			list2.Add(2);
		}
		List<int> list3 = new List<int> { OutputWaveFormat.BitsPerSample };
		if (!list3.Contains(32))
		{
			list3.Add(32);
		}
		if (!list3.Contains(24))
		{
			list3.Add(24);
		}
		if (!list3.Contains(16))
		{
			list3.Add(16);
		}
		List<int> list4 = new List<int> { 0 };
		if (list2.Contains(1))
		{
			list4.Add(4);
		}
		if (list2.Contains(2))
		{
			list4.Add(12);
		}
		if (list2.Contains(3))
		{
			list4.Add(11);
		}
		if (list2.Contains(4))
		{
			list4.Add(51);
			list4.Add(263);
		}
		if (list2.Contains(5))
		{
			list4.Add(1543);
		}
		if (list2.Contains(6))
		{
			list4.Add(1551);
		}
		if (list2.Contains(7))
		{
			list4.Add(1591);
		}
		if (list2.Contains(8))
		{
			list4.Add(1599);
		}
		foreach (int item in list)
		{
			foreach (int item2 in list2)
			{
				foreach (int item3 in list3)
				{
					foreach (int item4 in list4)
					{
						WaveFormatExtensible waveFormatExtensible = new WaveFormatExtensible(item, item3, item2, item4);
						if (audioClient.IsFormatSupported(shareMode, waveFormatExtensible))
						{
							return waveFormatExtensible;
						}
					}
				}
			}
		}
		throw new NotSupportedException("Can't find a supported format to use");
	}

	public long GetPosition()
	{
		ulong position;
		switch (playbackState)
		{
		case PlaybackState.Stopped:
			return 0L;
		case PlaybackState.Playing:
			position = audioClient.AudioClockClient.AdjustedPosition;
			break;
		default:
		{
			audioClient.AudioClockClient.GetPosition(out position, out var _);
			break;
		}
		}
		return (long)position * (long)OutputWaveFormat.AverageBytesPerSecond / (long)audioClient.AudioClockClient.Frequency;
	}

	public void Play()
	{
		if (playbackState != PlaybackState.Playing)
		{
			if (playbackState == PlaybackState.Stopped)
			{
				playThread = new Thread(PlayThread)
				{
					IsBackground = true
				};
				playbackState = PlaybackState.Playing;
				playThread.Start();
			}
			else
			{
				playbackState = PlaybackState.Playing;
			}
		}
	}

	public void Stop()
	{
		if (playbackState != PlaybackState.Stopped)
		{
			playbackState = PlaybackState.Stopped;
			playThread.Join();
			playThread = null;
		}
	}

	public void Pause()
	{
		if (playbackState == PlaybackState.Playing)
		{
			playbackState = PlaybackState.Paused;
		}
	}

	public void Init(IWaveProvider waveProvider)
	{
		long num = (long)latencyMilliseconds * 10000L;
		OutputWaveFormat = waveProvider.WaveFormat;
		AudioClientStreamFlags audioClientStreamFlags = AudioClientStreamFlags.SrcDefaultQuality | AudioClientStreamFlags.AutoConvertPcm;
		sourceProvider = waveProvider;
		if (shareMode == AudioClientShareMode.Exclusive)
		{
			audioClientStreamFlags = AudioClientStreamFlags.None;
			if (!audioClient.IsFormatSupported(shareMode, OutputWaveFormat, out var closestMatchFormat))
			{
				if (closestMatchFormat == null)
				{
					OutputWaveFormat = GetFallbackFormat();
				}
				else
				{
					OutputWaveFormat = closestMatchFormat;
				}
				try
				{
					using (new ResamplerDmoStream(waveProvider, OutputWaveFormat))
					{
					}
				}
				catch (Exception)
				{
					OutputWaveFormat = GetFallbackFormat();
					using (new ResamplerDmoStream(waveProvider, OutputWaveFormat))
					{
					}
				}
				dmoResamplerNeeded = true;
			}
			else
			{
				dmoResamplerNeeded = false;
			}
		}
		if (isUsingEventSync)
		{
			if (shareMode == AudioClientShareMode.Shared)
			{
				audioClient.Initialize(shareMode, AudioClientStreamFlags.EventCallback | audioClientStreamFlags, num, 0L, OutputWaveFormat, Guid.Empty);
				long streamLatency = audioClient.StreamLatency;
				if (streamLatency != 0L)
				{
					latencyMilliseconds = (int)(streamLatency / 10000);
				}
			}
			else
			{
				try
				{
					audioClient.Initialize(shareMode, AudioClientStreamFlags.EventCallback | audioClientStreamFlags, num, num, OutputWaveFormat, Guid.Empty);
				}
				catch (COMException ex2)
				{
					if (ex2.ErrorCode != -2004287463)
					{
						throw;
					}
					long num2 = (long)(10000000.0 / (double)OutputWaveFormat.SampleRate * (double)audioClient.BufferSize + 0.5);
					audioClient.Dispose();
					audioClient = mmDevice.AudioClient;
					audioClient.Initialize(shareMode, AudioClientStreamFlags.EventCallback | audioClientStreamFlags, num2, num2, OutputWaveFormat, Guid.Empty);
				}
			}
			frameEventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);
			audioClient.SetEventHandle(frameEventWaitHandle.SafeWaitHandle.DangerousGetHandle());
		}
		else
		{
			audioClient.Initialize(shareMode, audioClientStreamFlags, num, 0L, OutputWaveFormat, Guid.Empty);
		}
		renderClient = audioClient.AudioRenderClient;
	}

	public void Dispose()
	{
		if (audioClient != null)
		{
			Stop();
			audioClient.Dispose();
			audioClient = null;
			renderClient = null;
		}
	}
}
