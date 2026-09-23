using System;
using System.Linq;

namespace Thetis
{
    // Compatibility detector for the restored SQ4KOU/EU2AV Waterfall menu.
    // Uses the SDR-VST3/Vortice renderer state directly; it does not depend on
    // the obsolete ChannelMaster GPU-detector exports from the older branch.
    public static class GPUDetector
    {
        public enum CapabilityLevel
        {
            Level0_CPU_Only,
            Level1_Basic_Effects,
            Level2_Custom_Shaders
        }

        private static Display.AdaptorInfo BestHardwareAdapter()
        {
            try
            {
                Display.AdaptorInfo[] a = Display.DX2Adaptors();
                if (a == null || a.Length == 0) return null;
                return a.FirstOrDefault(x => x.IsDefaultHardware && x.IsHardware)
                    ?? a.FirstOrDefault(x => x.IsHardware)
                    ?? a[0];
            }
            catch
            {
                return null;
            }
        }

        public static CapabilityLevel Level
        {
            get
            {
                if (Display.RenderPath == Display.DXRenderPath.Hardware && Display.IsDX2DSetup)
                    return Display.GpuComputeEnabled
                        ? CapabilityLevel.Level2_Custom_Shaders
                        : CapabilityLevel.Level1_Basic_Effects;

                return BestHardwareAdapter() != null
                    ? CapabilityLevel.Level1_Basic_Effects
                    : CapabilityLevel.Level0_CPU_Only;
            }
        }

        public static string GPUName
        {
            get
            {
                Display.AdaptorInfo a = BestHardwareAdapter();
                return a != null && !String.IsNullOrWhiteSpace(a.Description)
                    ? a.Description.Trim()
                    : "unknown";
            }
        }

        public static string FeaturesList
        {
            get
            {
                string render = Display.RenderPathString();
                string compute = Display.GpuComputeEnabled ? "Compute ON" : "Compute OFF";
                string mesh = Display.SQ4KOUWaterfallMeshEnabled ? "WaterfallMesh ON" : "WaterfallMesh OFF";
                return "D3D11 " + render + ", " + compute + ", " + mesh;
            }
        }

        public static bool HasDeviceContext
        {
            get { return Display.IsDX2DSetup && Display.RenderPath == Display.DXRenderPath.Hardware; }
        }

        public static bool HasBuiltInEffects
        {
            get { return BestHardwareAdapter() != null; }
        }

        public static bool HasCustomShaders
        {
            get { return HasDeviceContext && Display.GpuComputeEnabled; }
        }

        public static void Refresh()
        {
            // No cache: the restored Waterfall diagnostics always reflect live state.
        }
    }
}
