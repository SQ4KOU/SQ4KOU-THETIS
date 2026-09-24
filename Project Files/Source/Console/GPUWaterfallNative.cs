using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Thetis
{
    internal static class GPUWaterfallNative
    {
        private const string DllName = "ChannelMaster.dll";

        // Native IQ producer ring only. The FFT and waterfall rendering themselves
        // are the recovered managed D3D11/SharpDX pipeline from EU2AV 2.10.3.16.
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_WaterfallIQ_Init(int channel, int requestedSamples);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CM_WaterfallIQ_SetEnabled(int channel, int enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_WaterfallIQ_Available(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_WaterfallIQ_Get(
            int channel,
            int requestedSamples,
            [Out] float[] iOut,
            [Out] float[] qOut);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CM_WaterfallIQ_Free(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong CM_WaterfallIQ_DroppedSamples(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CM_WaterfallIQ_ResetDropped(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int CM_GPUDetector_GetAdapterName(StringBuilder buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_GPUDetector_GetFeatureLevel();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int CM_GPUDetector_GetCapabilities(StringBuilder buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_GPUDetector_Test();
    }
}
