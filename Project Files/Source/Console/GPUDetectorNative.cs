using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Thetis
{
    internal static class GPUDetectorNative
    {
        private const string DllName = "ChannelMaster.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int CM_GPUDetector_GetAdapterName(StringBuilder buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_GPUDetector_GetFeatureLevel();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CM_GPUDetector_Test();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int CM_GPUDetector_GetCapabilities(StringBuilder buffer, int bufferSize);

        internal static string GetAdapterName()
        {
            try
            {
                StringBuilder sb = new StringBuilder(256);
                return CM_GPUDetector_GetAdapterName(sb, sb.Capacity) > 0 ? sb.ToString() : "D3D11 hardware GPU not detected";
            }
            catch { return "GPU detector unavailable"; }
        }

        internal static string GetCapabilities()
        {
            try
            {
                StringBuilder sb = new StringBuilder(512);
                CM_GPUDetector_GetCapabilities(sb, sb.Capacity);
                return sb.ToString();
            }
            catch { return "Capabilities unavailable"; }
        }

        internal static string GetFeatureLevelText()
        {
            try
            {
                int fl = CM_GPUDetector_GetFeatureLevel();
                if (fl == 111) return "11.1";
                if (fl == 110) return "11.0";
                if (fl == 101) return "10.1";
                if (fl == 100) return "10.0";
                return fl == 0 ? "n/a" : fl.ToString();
            }
            catch { return "n/a"; }
        }
    }
}
