using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Thetis;

internal static class GPUWaterfallLogger
{
	private sealed class WorkItem
	{
		public string Line;
	}

	private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenHPSDR", "Thetis-x64", "gpu_waterfall.log");

	private const long MaxSizeBytes = 2097152L;

	private const int RepeatWindowMs = 500;

	private const int MaxQueuedLines = 8192;

	private static readonly ConcurrentQueue<WorkItem> _queue = new ConcurrentQueue<WorkItem>();

	private static int _queuedCount;

	private static int _droppedLines;

	private static readonly object _throttleLock = new object();

	private static string _lastLine;

	private static long _lastLineTicks;

	private static int _repeatCount;

	private static Thread _writerThread;

	private static readonly ManualResetEventSlim _wake = new ManualResetEventSlim(initialState: false);

	private static volatile bool _started;

	public static void Log(string prefix, string message)
	{
		try
		{
			string text = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{prefix}] {message}";
			lock (_throttleLock)
			{
				long ticks = DateTime.UtcNow.Ticks;
				if (text == _lastLine && ticks - _lastLineTicks < 5000000)
				{
					_repeatCount++;
					return;
				}
				if (_repeatCount > 0)
				{
					Enqueue(_lastLine + $" (repeated x{_repeatCount + 1})");
					_repeatCount = 0;
				}
				_lastLine = text;
				_lastLineTicks = ticks;
				Enqueue(text);
			}
			EnsureStarted();
		}
		catch
		{
		}
	}

	private static void Enqueue(string line)
	{
		if (Interlocked.Increment(ref _queuedCount) > 8192)
		{
			Interlocked.Decrement(ref _queuedCount);
			Interlocked.Increment(ref _droppedLines);
		}
		else
		{
			_queue.Enqueue(new WorkItem
			{
				Line = line
			});
		}
	}

	private static void EnsureStarted()
	{
		if (_started)
		{
			return;
		}
		lock (_throttleLock)
		{
			if (_started)
			{
				return;
			}
			_started = true;
			_writerThread = new Thread(WriterLoop)
			{
				IsBackground = true,
				Priority = ThreadPriority.BelowNormal,
				Name = "GPUWaterfallLogger"
			};
			_writerThread.Start();
			try
			{
				AppDomain.CurrentDomain.ProcessExit += delegate
				{
					Flush(500);
				};
			}
			catch
			{
			}
		}
	}

	private static void WriterLoop()
	{
		StringBuilder stringBuilder = new StringBuilder(65536);
		while (true)
		{
			try
			{
				_wake.Wait(250);
				_wake.Reset();
				WorkItem result;
				while (_queue.TryDequeue(out result))
				{
					stringBuilder.AppendLine(result.Line);
					Interlocked.Decrement(ref _queuedCount);
				}
				if (stringBuilder.Length > 0)
				{
					WriteBatch(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
				int num = Interlocked.Exchange(ref _droppedLines, 0);
				if (num > 0)
				{
					WriteBatch($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [GPU-LOG] {num} log line(s) dropped (queue overflow)");
				}
			}
			catch
			{
			}
		}
	}

	private static void WriteBatch(string text)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(LogPath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (File.Exists(LogPath))
			{
				try
				{
					if (new FileInfo(LogPath).Length > 2097152)
					{
						string text2 = LogPath + ".bak";
						if (File.Exists(text2))
						{
							File.Delete(text2);
						}
						File.Move(LogPath, text2);
					}
				}
				catch
				{
				}
			}
			File.AppendAllText(LogPath, text);
		}
		catch
		{
		}
	}

	public static void Flush(int timeoutMs)
	{
		try
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (Volatile.Read(ref _queuedCount) > 0 && stopwatch.ElapsedMilliseconds < timeoutMs)
			{
				_wake.Set();
				Thread.Sleep(25);
			}
		}
		catch
		{
		}
	}
}
