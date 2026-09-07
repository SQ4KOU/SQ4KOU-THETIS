using System;
using System.Runtime.InteropServices;

namespace Thetis
{
    internal static class GPUWaterfallNative
    {
        private const string DllName = "ChannelMaster.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_GPUWaterfall_Init(int channel, int fftSize, int ringCapacity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_GPUWaterfall_Configure(int channel, int windowType, float kaiserBeta,
            int magnitudeMode, int autoOverlap, float overlapPercent, int lanczosWindow, int resamplingMode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_GPUWaterfall_Process(int channel, int displayWidth, int sampleRate,
            float displayLowHz, float displayHighHz, [Out] float[] outputDb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CM_GPUWaterfall_Free(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_GPUWaterfall_IsReady(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong CM_WaterfallIQ_DroppedSamples(int channel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CM_WaterfallIQ_ResetDropped(int channel);
    }
}
