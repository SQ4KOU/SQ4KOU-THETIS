using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Thetis;

public class HiPerfTimer
{
	private long startTime;

	private long stopTime;

	private long elapsedTime;

	private long freq;

	public double Duration => (double)(stopTime - startTime) / (double)freq;

	public double DurationMsec => 1000.0 * (double)(stopTime - startTime) / (double)freq;

	public double Elapsed
	{
		get
		{
			QueryPerformanceCounter(out elapsedTime);
			return (double)(elapsedTime - startTime) / (double)freq;
		}
	}

	public double ElapsedMsec
	{
		get
		{
			QueryPerformanceCounter(out elapsedTime);
			return 1000.0 * (double)(elapsedTime - startTime) / (double)freq;
		}
	}

	[DllImport("Kernel32.dll")]
	private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

	[DllImport("Kernel32.dll")]
	private static extern bool QueryPerformanceFrequency(out long lpFrequency);

	public HiPerfTimer()
	{
		startTime = 0L;
		stopTime = 0L;
		elapsedTime = 0L;
		if (!QueryPerformanceFrequency(out freq))
		{
			throw new Exception();
		}
	}

	public void Start()
	{
		Thread.Sleep(0);
		QueryPerformanceCounter(out startTime);
	}

	public void Stop()
	{
		QueryPerformanceCounter(out stopTime);
	}

	public void Reset()
	{
		Start();
	}

	public long GetFreq()
	{
		long lpFrequency = 0L;
		QueryPerformanceFrequency(out lpFrequency);
		return lpFrequency;
	}
}
