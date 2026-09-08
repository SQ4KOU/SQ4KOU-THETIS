using System;
using System.Net.NetworkInformation;
using System.Threading;

namespace Thetis;

internal static class TimeOutTimerManager
{
	public delegate void ToTOccured(string msg);

	private static ToTOccured _callback;

	private static string _hostAddress;

	private static int _pingTimeOutSeconds;

	private static bool _pingTimeoutEnabled;

	private static int _moxTimeOutSeconds;

	private static bool _moxTimeOutEnabled;

	private static bool _tickerRunning;

	private static DateTime _lastPing;

	private static DateTime _lastMox;

	private static bool _mox;

	private static readonly object _locker = new object();

	private static Console _console;

	private static Ping _ping;

	private static bool _init = false;

	public static void Initialise(Console c)
	{
		if (!_init)
		{
			_ping = new Ping();
			_tickerRunning = false;
			_lastMox = DateTime.UtcNow;
			_lastPing = _lastMox;
			_moxTimeOutEnabled = false;
			_pingTimeoutEnabled = false;
			_console = c;
			_mox = _console.MOX;
			Console console = _console;
			console.MoxChangeHandlers = (Console.MoxChanged)Delegate.Combine(console.MoxChangeHandlers, new Console.MoxChanged(onMox));
			startSecondTicker();
			_init = true;
		}
	}

	public static void Shutdown()
	{
		if (_init)
		{
			Console console = _console;
			console.MoxChangeHandlers = (Console.MoxChanged)Delegate.Remove(console.MoxChangeHandlers, new Console.MoxChanged(onMox));
			_tickerRunning = false;
			_console = null;
			_init = false;
		}
	}

	public static void SetCallback(ToTOccured cb)
	{
		_callback = (ToTOccured)Delegate.Combine(_callback, cb);
	}

	public static void RemoveCallback(ToTOccured cb)
	{
		_callback = (ToTOccured)Delegate.Remove(_callback, cb);
	}

	public static void PingTimeOut(string hostAddress, int timeOutSeconds, bool enabled)
	{
		lock (_locker)
		{
			_hostAddress = hostAddress;
			_pingTimeOutSeconds = timeOutSeconds;
			_pingTimeoutEnabled = enabled;
		}
	}

	public static void MoxTimeOut(int timeOutSeconds, bool enabled)
	{
		lock (_locker)
		{
			_moxTimeOutSeconds = timeOutSeconds;
			_moxTimeOutEnabled = enabled;
		}
	}

	private static void startSecondTicker()
	{
		Thread thread = new Thread(tickLoop);
		thread.Name = "ToT Tick";
		thread.Priority = ThreadPriority.BelowNormal;
		thread.IsBackground = true;
		thread.Start();
	}

	public static void onMox(int rx, bool oldMox, bool newMox)
	{
		lock (_locker)
		{
			_mox = newMox;
			if (newMox && !oldMox)
			{
				_lastMox = DateTime.UtcNow;
				_lastPing = _lastMox;
			}
		}
	}

	private static void tickLoop()
	{
		_tickerRunning = true;
		while (_tickerRunning)
		{
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			lock (_locker)
			{
				if (_mox && (_moxTimeOutEnabled || _pingTimeoutEnabled))
				{
					DateTime utcNow = DateTime.UtcNow;
					if (_moxTimeOutEnabled)
					{
						flag = (utcNow - _lastMox).TotalSeconds >= (double)_moxTimeOutSeconds;
					}
					if (!flag && _pingTimeoutEnabled)
					{
						try
						{
							PingReply pingReply = _ping.Send(_hostAddress, 900);
							if (pingReply.Status == IPStatus.Success)
							{
								_lastPing = utcNow;
							}
							num = (int)pingReply.RoundtripTime;
						}
						catch
						{
						}
						flag2 = (utcNow - _lastPing).TotalSeconds >= (double)_pingTimeOutSeconds;
					}
				}
			}
			if (flag | flag2)
			{
				string msg = (flag ? "MOX" : "PING");
				_callback?.Invoke(msg);
			}
			Thread.Sleep(1000 - num);
		}
	}
}
