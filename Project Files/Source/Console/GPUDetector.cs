using System;
using System.Linq;

namespace Thetis
{
    public static class GPUDetector
    {
        // Compatibility only. The rebuilt Waterfall UI does not use synthetic levels.
        public enum CapabilityLevel { Level0_CPU_Only, Level1_Basic_Effects, Level2_Custom_Shaders }

        private static Display.AdaptorInfo BestHardwareAdapter()
        {
            try
            {
                Display.AdaptorInfo[] a = Display.DX2Adaptors();
                if (a == null || a.Length == 0) return null;
                return a.FirstOrDefault(x => x.IsDefaultHardware && x.IsHardware)
                    ?? a.FirstOrDefault(x => x.IsHardware);
            }
            catch { return null; }
        }

        public static CapabilityLevel Level
        {
            get
            {
                if (!HasDeviceContext) return CapabilityLevel.Level0_CPU_Only;
                return Display.GpuComputeAvailable ? CapabilityLevel.Level2_Custom_Shaders
                                                   : CapabilityLevel.Level1_Basic_Effects;
            }
        }

        public static string GPUName
        {
            get
            {
                try
                {
                    string active = Display.ActiveGPUName;
                    if (!String.IsNullOrWhiteSpace(active) &&
                        active.IndexOf("unknown", StringComparison.OrdinalIgnoreCase) < 0 &&
                        active.IndexOf("unkown", StringComparison.OrdinalIgnoreCase) < 0)
                        return active.Trim();
                }
                catch { }

                Display.AdaptorInfo a = BestHardwareAdapter();
                return a != null && !String.IsNullOrWhiteSpace(a.Description)
                    ? a.Description.Trim() : "unknown";
            }
        }

        public static string FeaturesList
        {
            get
            {
                return "D3D11 " + Display.RenderPathString() +
                    " | HLSL " + (Display.GpuComputeEnabled ? "ON" : "OFF") +
                    " (" + (Display.GpuComputeAvailable ? "available" : "unavailable") + ")" +
                    " | GPU FFT " + (Display.GPUWaterfallPipelineEnabled ? "ON" : "OFF") +
                    " | WaterfallMesh " + (Display.SQ4KOUWaterfallMeshEnabled ? "ON" : "OFF");
            }
        }

        public static bool HasDeviceContext =>
            Display.IsDX2DSetup && Display.RenderPath == Display.DXRenderPath.Hardware;
        public static bool HasBuiltInEffects => HasDeviceContext;
        public static bool HasCustomShaders => Display.GpuComputeAvailable;
        public static void Refresh() { }
    }
}
