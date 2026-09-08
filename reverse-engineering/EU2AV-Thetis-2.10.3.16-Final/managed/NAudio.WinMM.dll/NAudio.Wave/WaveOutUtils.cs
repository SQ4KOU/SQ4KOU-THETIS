using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave;

public static class WaveOutUtils
{
	public static float GetWaveOutVolume(IntPtr hWaveOut, object lockObject)
	{
		MmResult result;
		int dwVolume;
		lock (lockObject)
		{
			result = WaveInterop.waveOutGetVolume(hWaveOut, out dwVolume);
		}
		MmException.Try(result, "waveOutGetVolume");
		return (float)(dwVolume & 0xFFFF) / 65535f;
	}

	public static void SetWaveOutVolume(float value, IntPtr hWaveOut, object lockObject)
	{
		if (value < 0f)
		{
			throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
		}
		if (value > 1f)
		{
			throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
		}
		int dwVolume = (int)(value * 65535f) + ((int)(value * 65535f) << 16);
		MmResult result;
		lock (lockObject)
		{
			result = WaveInterop.waveOutSetVolume(hWaveOut, dwVolume);
		}
		MmException.Try(result, "waveOutSetVolume");
	}

	public static long GetPositionBytes(IntPtr hWaveOut, object lockObject)
	{
		lock (lockObject)
		{
			MmTime mmTime = new MmTime
			{
				wType = 4u
			};
			MmException.Try(WaveInterop.waveOutGetPosition(hWaveOut, ref mmTime, Marshal.SizeOf(mmTime)), "waveOutGetPosition");
			if (mmTime.wType != 4)
			{
				throw new Exception($"waveOutGetPosition: wType -> Expected {4}, Received {mmTime.wType}");
			}
			return mmTime.cb;
		}
	}
}
