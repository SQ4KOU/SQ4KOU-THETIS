using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Thetis;

internal static class GPUWaterfallLogger
{
    private sealed class WorkItem
    {
        public string Line;
    }

    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OpenHPSDR", "Thetis-x64", "renderer_diagnostics.log");

    private const long MaxSizeBytes = 8L * 1024L * 1024L;
    private const int MaxQueuedLines = 16384;

    private static readonly ConcurrentQueue<WorkItem> _queue = new ConcurrentQueue<WorkItem>();
    private static readonly ConcurrentDictionary<string, long> _rateTicks = new ConcurrentDictionary<string, long>();
    private static readonly object _startLock = new object();
    private static readonly ManualResetEventSlim _wake = new ManualResetEventSlim(false);

    private static int _queuedCount;
    private static int _droppedLines;
    private static Thread _writerThread;
    private static volatile bool _started;

    private static long _frameSeq;
    private static volatile int _frameInProgress;
    private static long _frameStartTicks;
    private static long _lastFrameEndTicks;
    private static volatile int _frameThreadId;
    private static volatile string _frameStage = "idle";
    private static long _lastWatchdogFrameSeq = -1;
    private static long _lastNoFrameReportTicks;
    private static long _lastStatsTicks;
    private static volatile bool _watchdogArmed;

    // Output-side diagnostics.  Internal "Draw/Present OK" does not prove that the
    // operator actually received the expected pixels.  A low-rate worker samples the
    // real client-area image from the desktop after Present, outside the renderer lock.
    private sealed class VisualProbeRequest
    {
        public IntPtr Hwnd;
        public int Width;
        public int Height;
        public long FrameSeq;
        public bool ForceSnapshot;
        public string Reason;
    }

    private sealed class VisualSample
    {
        public ulong Hash;
        public double Mean;
        public double StdDev;
        public double DarkRatio;
        public double BrightRatio;
        public byte[] Luma;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

    private const uint GA_ROOT = 2;
    private const int VisualProbeIntervalMs = 750;
    private const int VisualStatsIntervalMs = 1000;
    private const int VisualSnapshotMinIntervalMs = 1500;

    private static readonly AutoResetEvent _visualWake = new AutoResetEvent(false);
    private static readonly object _visualLayoutLock = new object();
    private static Thread _visualThread;
    private static VisualProbeRequest _pendingVisualProbe;
    private static VisualProbeRequest _lastVisualProbe;
    private static long _lastVisualQueueTicks;
    private static long _lastVisualStatsTicks;
    private static long _lastVisualSnapshotTicks;
    private static int _visualWaterfallY = -1;
    private static int _visualWaterfallHeight = -1;
    private static long _visualLayoutTicks;
    private static VisualSample _prevBandVisual;
    private static VisualSample _prevWaterfallVisual;
    private static long _bandStillSinceTicks;
    private static long _waterfallStillSinceTicks;

    public static string FilePath => LogPath;

    public static void Log(string prefix, string message)
    {
        try
        {
            EnsureStarted();
            Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [T{Environment.CurrentManagedThreadId:D2}] [{prefix}] {message}");
        }
        catch
        {
        }
    }

    public static void LogRateLimited(string prefix, string key, int intervalMs, string message)
    {
        try
        {
            long now = Stopwatch.GetTimestamp();
            long minTicks = (long)(Stopwatch.Frequency * (intervalMs / 1000.0));
            string fullKey = prefix + "|" + key;
            while (true)
            {
                if (!_rateTicks.TryGetValue(fullKey, out long old))
                {
                    if (_rateTicks.TryAdd(fullKey, now))
                    {
                        Log(prefix, message);
                        return;
                    }
                    continue;
                }

                if (now - old < minTicks) return;
                if (_rateTicks.TryUpdate(fullKey, now, old))
                {
                    Log(prefix, message);
                    return;
                }
            }
        }
        catch
        {
        }
    }

    public static void FrameEnter(string stage)
    {
        try
        {
            EnsureStarted();
            long seq = Interlocked.Increment(ref _frameSeq);
            _watchdogArmed = true;
            _frameThreadId = Environment.CurrentManagedThreadId;
            _frameStage = stage ?? "enter";
            Interlocked.Exchange(ref _frameStartTicks, Stopwatch.GetTimestamp());
            Volatile.Write(ref _frameInProgress, 1);
            if (seq == 1)
                Log("SESSION", "Renderer diagnostics active. File=" + LogPath);
        }
        catch
        {
        }
    }

    public static void FrameStage(string stage)
    {
        try
        {
            _frameStage = stage ?? "?";
        }
        catch
        {
        }
    }

    public static void FrameEnd(string reason)
    {
        try
        {
            long now = Stopwatch.GetTimestamp();
            long start = Interlocked.Read(ref _frameStartTicks);
            double ms = start > 0 ? (now - start) * 1000.0 / Stopwatch.Frequency : 0.0;
            _frameStage = "idle";
            Volatile.Write(ref _frameInProgress, 0);
            Interlocked.Exchange(ref _lastFrameEndTicks, now);
            if (ms >= 250.0)
                Log("SLOW-FRAME", $"seq={Interlocked.Read(ref _frameSeq)} elapsed={ms:F1}ms reason={reason}");
        }
        catch
        {
        }
    }

    public static void FrameStats(string message)
    {
        try
        {
            long now = Stopwatch.GetTimestamp();
            long old = Interlocked.Read(ref _lastStatsTicks);
            if (old != 0 && (now - old) < Stopwatch.Frequency) return;
            if (Interlocked.CompareExchange(ref _lastStatsTicks, now, old) == old)
                Log("FRAME", message);
        }
        catch
        {
        }
    }

    public static void SetVisualWaterfallLayout(int rx, int y, int height)
    {
        if (rx != 1) return;
        try
        {
            lock (_visualLayoutLock)
            {
                _visualWaterfallY = y;
                _visualWaterfallHeight = height;
                _visualLayoutTicks = Stopwatch.GetTimestamp();
            }
        }
        catch
        {
        }
    }

    public static void QueueVisualProbe(IntPtr hwnd, int width, int height)
    {
        try
        {
            if (hwnd == IntPtr.Zero || width <= 0 || height <= 0) return;
            EnsureStarted();

            long now = Stopwatch.GetTimestamp();
            long old = Interlocked.Read(ref _lastVisualQueueTicks);
            long minTicks = (long)(Stopwatch.Frequency * (VisualProbeIntervalMs / 1000.0));
            if (old != 0 && now - old < minTicks) return;
            if (Interlocked.CompareExchange(ref _lastVisualQueueTicks, now, old) != old) return;

            var request = new VisualProbeRequest
            {
                Hwnd = hwnd,
                Width = width,
                Height = height,
                FrameSeq = Interlocked.Read(ref _frameSeq),
                ForceSnapshot = false,
                Reason = null
            };
            Interlocked.Exchange(ref _lastVisualProbe, request);
            Interlocked.Exchange(ref _pendingVisualProbe, request);
            _visualWake.Set();
        }
        catch
        {
        }
    }

    private static void QueueUrgentVisualProbe(string reason)
    {
        try
        {
            VisualProbeRequest last = Interlocked.CompareExchange(ref _lastVisualProbe, null, null);
            if (last == null || last.Hwnd == IntPtr.Zero) return;

            Interlocked.Exchange(ref _pendingVisualProbe, new VisualProbeRequest
            {
                Hwnd = last.Hwnd,
                Width = last.Width,
                Height = last.Height,
                FrameSeq = Interlocked.Read(ref _frameSeq),
                ForceSnapshot = true,
                Reason = reason
            });
            _visualWake.Set();
        }
        catch
        {
        }
    }

    private static VisualSample SampleRegion(Bitmap bitmap, Rectangle region)
    {
        if (bitmap == null || region.Width <= 8 || region.Height <= 8) return null;

        Rectangle bounds = Rectangle.Intersect(new Rectangle(0, 0, bitmap.Width, bitmap.Height), region);
        if (bounds.Width <= 8 || bounds.Height <= 8) return null;

        BitmapData data = null;
        try
        {
            data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = Math.Abs(data.Stride);
            byte[] pixels = new byte[stride * bitmap.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            int stepX = Math.Max(4, bounds.Width / 160);
            int stepY = Math.Max(3, bounds.Height / 60);
            int cap = ((bounds.Width + stepX - 1) / stepX) * ((bounds.Height + stepY - 1) / stepY);
            byte[] luma = new byte[Math.Max(1, cap)];

            long sum = 0;
            long sumSq = 0;
            int dark = 0;
            int bright = 0;
            int count = 0;
            ulong hash = 1469598103934665603UL;

            for (int y = bounds.Top; y < bounds.Bottom; y += stepY)
            {
                int row = y * stride;
                for (int x = bounds.Left; x < bounds.Right; x += stepX)
                {
                    int p = row + x * 3;
                    if (p + 2 >= pixels.Length) continue;
                    int b = pixels[p + 0];
                    int g = pixels[p + 1];
                    int r = pixels[p + 2];
                    byte lum = (byte)((54 * r + 183 * g + 19 * b) >> 8);
                    luma[count++] = lum;
                    sum += lum;
                    sumSq += (long)lum * lum;
                    if (lum <= 12) dark++;
                    if (lum >= 245) bright++;
                    hash ^= lum;
                    hash *= 1099511628211UL;
                }
            }

            if (count == 0) return null;
            if (count != luma.Length) Array.Resize(ref luma, count);
            double mean = (double)sum / count;
            double variance = Math.Max(0.0, (double)sumSq / count - mean * mean);
            return new VisualSample
            {
                Hash = hash,
                Mean = mean,
                StdDev = Math.Sqrt(variance),
                DarkRatio = (double)dark / count,
                BrightRatio = (double)bright / count,
                Luma = luma
            };
        }
        finally
        {
            if (data != null) bitmap.UnlockBits(data);
        }
    }

    private static double VisualChangedRatio(VisualSample current, VisualSample previous)
    {
        if (current == null || previous == null || current.Luma == null || previous.Luma == null ||
            current.Luma.Length != previous.Luma.Length)
            return 1.0;

        int changed = 0;
        for (int i = 0; i < current.Luma.Length; i++)
            if (Math.Abs(current.Luma[i] - previous.Luma[i]) >= 12) changed++;
        return current.Luma.Length == 0 ? 0.0 : (double)changed / current.Luma.Length;
    }

    private static bool LargeVisualJump(VisualSample current, VisualSample previous)
    {
        if (current == null || previous == null) return false;
        return Math.Abs(current.Mean - previous.Mean) >= 18.0 ||
               Math.Abs(current.StdDev - previous.StdDev) >= 14.0 ||
               Math.Abs(current.DarkRatio - previous.DarkRatio) >= 0.22 ||
               Math.Abs(current.BrightRatio - previous.BrightRatio) >= 0.22;
    }

    private static bool LooksBlank(VisualSample sample)
    {
        if (sample == null) return true;
        return (sample.DarkRatio >= 0.965 && sample.StdDev <= 4.0) ||
               (sample.BrightRatio >= 0.965 && sample.StdDev <= 4.0);
    }

    private static void SaveVisualSnapshot(Bitmap bitmap, long frameSeq, string reason)
    {
        try
        {
            long now = Stopwatch.GetTimestamp();
            long old = Interlocked.Read(ref _lastVisualSnapshotTicks);
            long minTicks = (long)(Stopwatch.Frequency * (VisualSnapshotMinIntervalMs / 1000.0));
            if (old != 0 && now - old < minTicks) return;
            if (Interlocked.CompareExchange(ref _lastVisualSnapshotTicks, now, old) != old) return;

            string dir = Path.Combine(Path.GetDirectoryName(LogPath), "renderer_visual");
            Directory.CreateDirectory(dir);
            string safeReason = string.IsNullOrWhiteSpace(reason) ? "change" : reason.Replace(' ', '_');
            string file = Path.Combine(dir,
                $"visual_{DateTime.Now:yyyyMMdd_HHmmss_fff}_f{frameSeq}_{safeReason}.png");
            bitmap.Save(file, ImageFormat.Png);
            Log("VISUAL-SNAPSHOT", file);
        }
        catch (Exception ex)
        {
            LogRateLimited("VISUAL-PROBE", "snapshot-error", 5000, "snapshot failed: " + ex.Message);
        }
    }

    private static void CaptureVisual(VisualProbeRequest request)
    {
        if (request == null || request.Hwnd == IntPtr.Zero) return;

        int wfY;
        int wfH;
        long layoutTicks;
        lock (_visualLayoutLock)
        {
            wfY = _visualWaterfallY;
            wfH = _visualWaterfallHeight;
            layoutTicks = _visualLayoutTicks;
        }

        long now = Stopwatch.GetTimestamp();
        if (layoutTicks == 0 || now - layoutTicks > 2 * Stopwatch.Frequency) return;
        if (wfY <= 0 || wfY >= request.Height || wfH <= 8) return;

        IntPtr root = GetAncestor(request.Hwnd, GA_ROOT);
        bool foreground = root != IntPtr.Zero && root == GetForegroundWindow();
        if (!foreground)
        {
            // CopyFromScreen forces compositor/GDI synchronization.  Sampling an
            // obscured window is both misleading and unnecessary load, so the probe
            // is dormant unless the actual renderer window is foreground.
            return;
        }

        POINT pt = new POINT();
        if (!ClientToScreen(request.Hwnd, ref pt)) return;

        using Bitmap bitmap = new Bitmap(request.Width, request.Height, PixelFormat.Format24bppRgb);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(pt.X, pt.Y, 0, 0,
                new Size(request.Width, request.Height), CopyPixelOperation.SourceCopy);
        }

        int xMargin = Math.Max(8, request.Width / 20);
        int bandTop = Math.Max(4, request.Height / 80);
        int bandBottom = Math.Max(bandTop + 8, Math.Min(request.Height, wfY - 8));
        int wfTop = Math.Max(0, Math.Min(request.Height - 1, wfY));
        int wfBottom = Math.Max(wfTop + 8, Math.Min(request.Height, wfY + wfH));

        VisualSample band = SampleRegion(bitmap,
            new Rectangle(xMargin, bandTop, Math.Max(8, request.Width - 2 * xMargin), bandBottom - bandTop));
        VisualSample waterfall = SampleRegion(bitmap,
            new Rectangle(xMargin, wfTop, Math.Max(8, request.Width - 2 * xMargin), wfBottom - wfTop));

        double bandChanged = VisualChangedRatio(band, _prevBandVisual);
        double wfChanged = VisualChangedRatio(waterfall, _prevWaterfallVisual);

        bool bandBlank = LooksBlank(band);
        bool wfBlank = LooksBlank(waterfall);
        bool bandJump = LargeVisualJump(band, _prevBandVisual);
        bool wfJump = LargeVisualJump(waterfall, _prevWaterfallVisual);

        if (foreground)
        {
            if (!bandBlank && bandChanged < 0.003)
            {
                if (_bandStillSinceTicks == 0) _bandStillSinceTicks = now;
            }
            else _bandStillSinceTicks = 0;

            if (!wfBlank && wfChanged < 0.003)
            {
                if (_waterfallStillSinceTicks == 0) _waterfallStillSinceTicks = now;
            }
            else _waterfallStillSinceTicks = 0;
        }
        else
        {
            _bandStillSinceTicks = 0;
            _waterfallStillSinceTicks = 0;
        }

        bool bandFrozen = _bandStillSinceTicks != 0 && now - _bandStillSinceTicks >= 2 * Stopwatch.Frequency;
        bool wfFrozen = _waterfallStillSinceTicks != 0 && now - _waterfallStillSinceTicks >= 2 * Stopwatch.Frequency;

        long oldStats = Interlocked.Read(ref _lastVisualStatsTicks);
        long statsTicks = (long)(Stopwatch.Frequency * (VisualStatsIntervalMs / 1000.0));
        if (oldStats == 0 || now - oldStats >= statsTicks)
        {
            if (Interlocked.CompareExchange(ref _lastVisualStatsTicks, now, oldStats) == oldStats)
            {
                Log("VISUAL",
                    $"frame={request.FrameSeq} foreground={foreground} " +
                    $"band mean={band?.Mean:F1} sd={band?.StdDev:F1} dark={band?.DarkRatio:P1} change={bandChanged:P1} blank={bandBlank} frozen={bandFrozen} hash={band?.Hash:X16} " +
                    $"wf mean={waterfall?.Mean:F1} sd={waterfall?.StdDev:F1} dark={waterfall?.DarkRatio:P1} change={wfChanged:P1} blank={wfBlank} frozen={wfFrozen} hash={waterfall?.Hash:X16}");
            }
        }

        if (bandBlank || wfBlank || bandFrozen || wfFrozen || bandJump || wfJump)
        {
            string reason = bandBlank ? "band_blank" :
                            wfBlank ? "waterfall_blank" :
                            bandFrozen ? "band_frozen" :
                            wfFrozen ? "waterfall_frozen" :
                            bandJump ? "band_jump" : "waterfall_jump";
            Log("VISUAL-CHANGE",
                $"frame={request.FrameSeq} reason={reason} " +
                $"bandChange={bandChanged:P1} wfChange={wfChanged:P1} " +
                $"bandMean={band?.Mean:F1} wfMean={waterfall?.Mean:F1}");
            SaveVisualSnapshot(bitmap, request.FrameSeq, reason);
        }
        else if (request.ForceSnapshot)
        {
            string reason = string.IsNullOrWhiteSpace(request.Reason) ? "watchdog" : request.Reason;
            Log("VISUAL-CHANGE",
                $"frame={request.FrameSeq} reason={reason} forced=True " +
                $"bandChange={bandChanged:P1} wfChange={wfChanged:P1}");
            SaveVisualSnapshot(bitmap, request.FrameSeq, reason);
        }

        _prevBandVisual = band;
        _prevWaterfallVisual = waterfall;
    }

    private static void VisualLoop()
    {
        while (true)
        {
            try
            {
                _visualWake.WaitOne(500);
                VisualProbeRequest request = Interlocked.Exchange(ref _pendingVisualProbe, null);
                if (request == null) continue;

                // Give DWM a short opportunity to publish the just-presented frame.
                Thread.Sleep(20);
                CaptureVisual(request);
            }
            catch (Exception ex)
            {
                LogRateLimited("VISUAL-PROBE", "capture-error", 5000, ex.GetType().Name + ": " + ex.Message);
            }
        }
    }

    private static void Enqueue(string line)
    {
        if (Interlocked.Increment(ref _queuedCount) > MaxQueuedLines)
        {
            Interlocked.Decrement(ref _queuedCount);
            Interlocked.Increment(ref _droppedLines);
            return;
        }

        _queue.Enqueue(new WorkItem { Line = line });
        _wake.Set();
    }

    private static void EnsureStarted()
    {
        if (_started) return;

        lock (_startLock)
        {
            if (_started) return;
            _started = true;
            _lastFrameEndTicks = Stopwatch.GetTimestamp();

            _writerThread = new Thread(WriterLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "RendererDiagnostics"
            };
            _writerThread.Start();

            _visualThread = new Thread(VisualLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "RendererVisualDiagnostics"
            };
            _visualThread.Start();

            try
            {
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                    Log("UNHANDLED", e.ExceptionObject == null ? "<null>" : e.ExceptionObject.ToString());
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
                    Log("TASK-EX", e.Exception == null ? "<null>" : e.Exception.ToString());
                System.Windows.Forms.Application.ThreadException += (s, e) =>
                    Log("UI-EX", e.Exception == null ? "<null>" : e.Exception.ToString());
                AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    Log("SESSION", "ProcessExit");
                    Flush(1000);
                };
            }
            catch
            {
            }
        }
    }

    public static void RendererStopped()
    {
        try
        {
            _watchdogArmed = false;
            _frameStage = "stopped";
            Volatile.Write(ref _frameInProgress, 0);
            Interlocked.Exchange(ref _lastNoFrameReportTicks, 0);
        }
        catch
        {
        }
    }

    private static void CheckWatchdog()
    {
        try
        {
            if (!_watchdogArmed) return;

            long now = Stopwatch.GetTimestamp();
            long seq = Interlocked.Read(ref _frameSeq);

            if (Volatile.Read(ref _frameInProgress) != 0)
            {
                long start = Interlocked.Read(ref _frameStartTicks);
                double ms = start > 0 ? (now - start) * 1000.0 / Stopwatch.Frequency : 0.0;
                if (ms >= 1200.0 && seq != Interlocked.Read(ref _lastWatchdogFrameSeq))
                {
                    Interlocked.Exchange(ref _lastWatchdogFrameSeq, seq);
                    WriteBatch($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [WATCHDOG] STALL frame={seq} elapsed={ms:F0}ms thread={_frameThreadId} stage={_frameStage}{Environment.NewLine}");
                    QueueUrgentVisualProbe("watchdog_" + _frameStage);
                }
            }
            else
            {
                long lastEnd = Interlocked.Read(ref _lastFrameEndTicks);
                if (lastEnd > 0)
                {
                    double idleMs = (now - lastEnd) * 1000.0 / Stopwatch.Frequency;
                    long lastReport = Interlocked.Read(ref _lastNoFrameReportTicks);
                    if (idleMs >= 2500.0 &&
                        (lastReport == 0 || (now - lastReport) >= 2 * Stopwatch.Frequency))
                    {
                        Interlocked.Exchange(ref _lastNoFrameReportTicks, now);
                        WriteBatch($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [WATCHDOG] NO-FRAME for {idleMs:F0}ms lastSeq={seq}{Environment.NewLine}");
                    }
                }
            }
        }
        catch
        {
        }
    }

    private static void WriterLoop()
    {
        StringBuilder sb = new StringBuilder(65536);
        while (true)
        {
            try
            {
                _wake.Wait(250);
                _wake.Reset();

                while (_queue.TryDequeue(out WorkItem item))
                {
                    sb.AppendLine(item.Line);
                    Interlocked.Decrement(ref _queuedCount);
                }

                if (sb.Length > 0)
                {
                    WriteBatch(sb.ToString());
                    sb.Clear();
                }

                int dropped = Interlocked.Exchange(ref _droppedLines, 0);
                if (dropped > 0)
                    WriteBatch($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [LOGGER] dropped={dropped}{Environment.NewLine}");

                CheckWatchdog();
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
            string dir = Path.GetDirectoryName(LogPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(LogPath) && new FileInfo(LogPath).Length > MaxSizeBytes)
            {
                string bak = LogPath + ".bak";
                try
                {
                    if (File.Exists(bak)) File.Delete(bak);
                    File.Move(LogPath, bak);
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
            Stopwatch sw = Stopwatch.StartNew();
            while (Volatile.Read(ref _queuedCount) > 0 && sw.ElapsedMilliseconds < timeoutMs)
            {
                _wake.Set();
                Thread.Sleep(20);
            }
        }
        catch
        {
        }
    }
}
