/*  WsjtAudioBridge.cs
 *
 *  Named-pipe client for the WSJT-X AUDIO channels.
 *
 *  Copyright (C) 2026  SDR-VST3 Thetis integration (WSJT-X sidecar)
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License
 *  as published by the Free Software Foundation; either version 2
 *  of the License, or (at your option) any later version.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Thetis.WSJTX
{
    /// <summary>
    /// Drains the down-sampled RX audio from the native module
    /// (WsjtDrainRx) and pushes it into the WSJT-X AUDIO pipe as
    /// 8 kHz mono float (little-endian) -- the exact wire format the
    /// WSJT-X sidecar's SoundInPipe consumes.
    ///
    /// P1 is decode-only: no reverse direction (decode only): WSJT-X never
    /// writes TX audio back to this channel -- TX audio is carried on a
    /// separate TXAUDIO pipe drained by the parallel TX reader thread below.
    /// </summary>
    internal static class WsjtAudioBridge
    {
        private static NamedPipeClientStream _stream;
        private static volatile bool _running;
        private static int _gen;        // incremented on each Start/Stop cycle
        private static Thread _writerThread;
        private static readonly object _writeLock = new object();
        private static readonly float[] _rxBuf = new float[4096];
        private static readonly byte[] _writeBytes = new byte[4096 * 4];

        private const int DRAIN_BATCH = 2048;   // samples per WsjtDrainRx call

        public static void Start(int pid)
        {
            Stop();                              // tear down any prior session
            if (pid <= 0) return;
            _gen++;
            _running = true;
            var stream = new NamedPipeClientStream(
                ".",
                WsjtManager.AudioPipeName(pid),
                PipeDirection.Out,
                PipeOptions.Asynchronous);

            _stream = stream;

            bool connected = false;
            try
            {
                stream.Connect(3000);
                connected = true;
            }
            catch (TimeoutException)
            {
                Trace.WriteLine("WSJT-AUDIO: pipe connect timed out");
            }
            catch (IOException ex)
            {
                Trace.WriteLine("WSJT-AUDIO: pipe connect error: " + ex.Message);
            }
            if (!connected)
            {
                try { stream.Dispose(); } catch { }
                _stream = null;
                return;
            }
            Trace.WriteLine(string.Format("WSJT-AUDIO: connected (pid {0})", pid));

            _writerThread = new Thread(WriterLoop) { IsBackground = true, Name = "wsjtx-audio-wr" };
            _writerThread.Start();
        }

        private static void WriterLoop()
        {
            int gen = _gen;
            while (_running)
            {
                if (_gen != gen) return;
                var s = _stream;
                if (s == null || !s.IsConnected)
                {
                    Thread.Sleep(20);
                    continue;
                }
                try
                {
                    int n = cmaster.WsjtDrainRx(0, _rxBuf, DRAIN_BATCH);
                    if (n > 0)
                    {
                        Buffer.BlockCopy(_rxBuf, 0, _writeBytes, 0, n * 4);
                        lock (_writeLock)
                        {
                            if (_stream != null && _stream.IsConnected)
                                _stream.Write(_writeBytes, 0, n * 4);
                        }
                    }
                }
                catch (ObjectDisposedException) { return; }
                catch (IOException ex)
                {
                    Trace.WriteLine("WSJT-AUDIO: write exception: " + ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("WSJT-AUDIO: writer error: " + ex.Message);
                }
                Thread.Sleep(4);
            }
        }

        public static void Stop()
        {
            _running = false;
            _gen++;
            var s = _stream;
            _stream = null;
            if (s != null)
            {
                try { s.Close(); } catch { }
                try { s.Dispose(); } catch { }
            }
            var t = _writerThread;
            _writerThread = null;
            if (t != null)
            {
                try { t.Join(200); } catch { }
            }
            Trace.WriteLine("WSJT-AUDIO: stopped");
        }
    }
}