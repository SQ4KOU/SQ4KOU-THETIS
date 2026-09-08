using System;
namespace Thetis
{
    public static class GPUDetector
    {
        public enum CapabilityLevel { Level0_CPU_Only, Level1_Basic_Effects, Level2_Custom_Shaders }
        private static bool _detected;
        private static CapabilityLevel _level = CapabilityLevel.Level0_CPU_Only;
        private static string _gpuName = "unknown";
        private static string _features = "";
        private static bool _hasDeviceContext, _hasBuiltInEffects, _hasCustomShaders;
        public static CapabilityLevel Level { get { EnsureDetected(); return _level; } }
        public static string GPUName { get { EnsureDetected(); return _gpuName; } }
        public static string FeaturesList { get { EnsureDetected(); return _features; } }
        public static bool HasDeviceContext { get { EnsureDetected(); return _hasDeviceContext; } }
        public static bool HasBuiltInEffects { get { EnsureDetected(); return _hasBuiltInEffects; } }
        public static bool HasCustomShaders { get { EnsureDetected(); return _hasCustomShaders; } }
        public static void Refresh() { _detected = false; EnsureDetected(); }
        private static void EnsureDetected()
        {
            if (_detected) return; _detected = true;
            _level = CapabilityLevel.Level0_CPU_Only; _gpuName = "unknown"; _features = "";
            _hasDeviceContext = _hasBuiltInEffects = _hasCustomShaders = false;
            try
            {
                _gpuName = GPUDetectorNative.GetAdapterName();
                int fl = GPUDetectorNative.CM_GPUDetector_GetFeatureLevel();
                _hasDeviceContext = fl > 0;
                if (_hasDeviceContext) { _level = CapabilityLevel.Level1_Basic_Effects; _hasBuiltInEffects = true; }
                if (GPUDetectorNative.CM_GPUDetector_Test() != 0) { _level = CapabilityLevel.Level2_Custom_Shaders; _hasCustomShaders = true; }
                _features = GPUDetectorNative.GetCapabilities();
                if (String.IsNullOrEmpty(_features)) _features = _hasDeviceContext ? "D3D11 Device/Context" : "";
            }
            catch { _level = CapabilityLevel.Level0_CPU_Only; _features = ""; }
        }
    }
}
