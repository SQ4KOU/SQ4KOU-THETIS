using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NAudio.Wave;

public class WaveOut : IWavePlayer, IDisposable, IWavePosition
{
	private IntPtr hWaveOut;

	private WaveOutBuffer[] buffers;

	private IWaveProvider waveStream;

	private volatile PlaybackState playbackState;

	private readonly WaveInterop.WaveCallback callback;

	private readonly WaveCallbackInfo callbackInfo;

	private readonly object waveOutLock;

	private int queuedBuffers;

	private readonly SynchronizationContext syncContext;

	public static int DeviceCount => WaveInterop.waveOutGetNumDevs();

	public int DesiredLatency { get; set; }

	public int NumberOfBuffers { get; set; }

	public int DeviceNumber { get; set; } = -1;

	public WaveFormat OutputWaveFormat => waveStream.WaveFormat;

	public PlaybackState PlaybackState => playbackState;

	public float Volume
	{
		get
		{
			return WaveOutUtils.GetWaveOutVolume(hWaveOut, waveOutLock);
		}
		set
		{
			WaveOutUtils.SetWaveOutVolume(value, hWaveOut, waveOutLock);
		}
	}

	public event EventHandler<StoppedEventArgs> PlaybackStopped;

	public static WaveOutCapabilities GetCapabilities(int devNumber)
	{
		WaveOutCapabilities waveOutCaps = default(WaveOutCapabilities);
		int waveOutCapsSize = Marshal.SizeOf(waveOutCaps);
		MmException.Try(WaveInterop.waveOutGetDevCaps((IntPtr)devNumber, out waveOutCaps, waveOutCapsSize), "waveOutGetDevCaps");
		return waveOutCaps;
	}

	public WaveOut()
		: this((SynchronizationContext.Current == null) ? WaveCallbackInfo.FunctionCallback() : WaveCallbackInfo.NewWindow())
	{
	}

	public WaveOut(IntPtr windowHandle)
		: this(WaveCallbackInfo.ExistingWindow(windowHandle))
	{
	}

	public WaveOut(WaveCallbackInfo callbackInfo)
	{
		syncContext = SynchronizationContext.Current;
		DesiredLatency = 300;
		NumberOfBuffers = 2;
		callback = Callback;
		waveOutLock = new object();
		this.callbackInfo = callbackInfo;
		callbackInfo.Connect(callback);
	}

	public void Init(IWaveProvider waveProvider)
	{
		waveStream = waveProvider;
		int bufferSize = waveProvider.WaveFormat.ConvertLatencyToByteSize((DesiredLatency + NumberOfBuffers - 1) / NumberOfBuffers);
		MmResult result;
		lock (waveOutLock)
		{
			result = callbackInfo.WaveOutOpen(out hWaveOut, DeviceNumber, waveStream.WaveFormat, callback);
		}
		MmException.Try(result, "waveOutOpen");
		buffers = new WaveOutBuffer[NumberOfBuffers];
		playbackState = PlaybackState.Stopped;
		for (int i = 0; i < NumberOfBuffers; i++)
		{
			buffers[i] = new WaveOutBuffer(hWaveOut, bufferSize, waveStream, waveOutLock);
		}
	}

	public void Play()
	{
		if (playbackState == PlaybackState.Stopped)
		{
			playbackState = PlaybackState.Playing;
			EnqueueBuffers();
		}
		else if (playbackState == PlaybackState.Paused)
		{
			EnqueueBuffers();
			Resume();
			playbackState = PlaybackState.Playing;
		}
	}

	private void EnqueueBuffers()
	{
		for (int i = 0; i < NumberOfBuffers; i++)
		{
			if (!buffers[i].InQueue)
			{
				if (!buffers[i].OnDone())
				{
					playbackState = PlaybackState.Stopped;
					break;
				}
				Interlocked.Increment(ref queuedBuffers);
			}
		}
	}

	public void Pause()
	{
		if (playbackState == PlaybackState.Playing)
		{
			playbackState = PlaybackState.Paused;
			MmResult mmResult;
			lock (waveOutLock)
			{
				mmResult = WaveInterop.waveOutPause(hWaveOut);
			}
			if (mmResult != MmResult.NoError)
			{
				throw new MmException(mmResult, "waveOutPause");
			}
		}
	}

	public void Resume()
	{
		if (playbackState == PlaybackState.Paused)
		{
			MmResult mmResult;
			lock (waveOutLock)
			{
				mmResult = WaveInterop.waveOutRestart(hWaveOut);
			}
			if (mmResult != MmResult.NoError)
			{
				throw new MmException(mmResult, "waveOutRestart");
			}
			playbackState = PlaybackState.Playing;
		}
	}

	public void Stop()
	{
		if (playbackState != PlaybackState.Stopped)
		{
			playbackState = PlaybackState.Stopped;
			MmResult mmResult;
			lock (waveOutLock)
			{
				mmResult = WaveInterop.waveOutReset(hWaveOut);
			}
			if (mmResult != MmResult.NoError)
			{
				throw new MmException(mmResult, "waveOutReset");
			}
			if (callbackInfo.Strategy == WaveCallbackStrategy.FunctionCallback)
			{
				RaisePlaybackStoppedEvent(null);
			}
		}
	}

	public long GetPosition()
	{
		return WaveOutUtils.GetPositionBytes(hWaveOut, waveOutLock);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true);
	}

	protected void Dispose(bool disposing)
	{
		Stop();
		if (disposing && buffers != null)
		{
			for (int i = 0; i < buffers.Length; i++)
			{
				if (buffers[i] != null)
				{
					buffers[i].Dispose();
				}
			}
			buffers = null;
		}
		lock (waveOutLock)
		{
			WaveInterop.waveOutClose(hWaveOut);
		}
		if (disposing)
		{
			callbackInfo.Disconnect();
		}
	}

	~WaveOut()
	{
		Dispose(disposing: false);
	}

	private void Callback(IntPtr hWaveOut, WaveInterop.WaveMessage uMsg, IntPtr dwInstance, WaveHeader wavhdr, IntPtr dwReserved)
	{
		if (uMsg != WaveInterop.WaveMessage.WaveOutDone)
		{
			return;
		}
		WaveOutBuffer waveOutBuffer = (WaveOutBuffer)((GCHandle)wavhdr.userData).Target;
		Interlocked.Decrement(ref queuedBuffers);
		Exception e = null;
		if (PlaybackState == PlaybackState.Playing)
		{
			lock (waveOutLock)
			{
				try
				{
					if (waveOutBuffer.OnDone())
					{
						Interlocked.Increment(ref queuedBuffers);
					}
				}
				catch (Exception ex)
				{
					e = ex;
				}
			}
		}
		if (queuedBuffers == 0 && (callbackInfo.Strategy != WaveCallbackStrategy.FunctionCallback || playbackState != PlaybackState.Stopped))
		{
			playbackState = PlaybackState.Stopped;
			RaisePlaybackStoppedEvent(e);
		}
	}

	private void RaisePlaybackStoppedEvent(Exception e)
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
}
