using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Thetis
{
    internal static class RadeNative
    {
        private const string DLL = "ChannelMaster.dll";

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeRxEnabled(int rx, int enable);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeRxEnabled(int rx);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeTxEnabled(int enable);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeTxEnabled();
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeProtocolV2(int rx, int on);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeProtocolV2(int rx);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeTxRx(int rx);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeMoxState(int mox);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void RadaeNotifyBeginOver();
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void RadaeNotifyEndOfOver();
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeEooFlushed();
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeTxSilenceHold(int on);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)] internal static extern void SetRadaeEooCallsign(string callsign);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeSync(int rx);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeSnrDb(int rx);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeRxLevelDb(int rx);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeClip(int rx);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeTxMicLevelDb();
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeTxMicClip();
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern int GetRadaeRemoteCallsign(int rx, StringBuilder dst, int max);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeMicScale(double scale);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeRxDialScale(int rx, double scale);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeRxAFGain(int rx, double gain);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeMicRNNoiseEnabled(int enable);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeMicAGCEnabled(int enable);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeMicAGCTargetLufs(double target);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeMicEQEnabled(int enable);
        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] internal static extern void SetRadaeLoopbackEnabled(int rx, int enable);

        internal static double DbToLinear(decimal db)
        {
            return Math.Pow(10.0, (double)db / 20.0);
        }
    }
}
