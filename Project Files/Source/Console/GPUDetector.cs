using System;
using System.Collections.Generic;
using System.IO;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;

namespace Thetis;

public static class GPUDetector
{
    public enum CapabilityLevel
    {
        Level0_CPU_Only,
        Level1_Basic_Effects,
        Level2_Custom_Shaders
    }

    public static CapabilityLevel Level { get; private set; } = CapabilityLevel.Level0_CPU_Only;

    public static string GPUName { get; private set; } = "unknown";

    public static string FeaturesList { get; private set; } = "";

    public static bool HasDeviceContext { get; private set; }

    public static bool HasBuiltInEffects { get; private set; }

    public static bool HasCustomShaders { get; private set; }

    public static void Detect(DeviceContext dc, Factory1 factory)
    {
        HasDeviceContext = false;
        HasBuiltInEffects = false;
        HasCustomShaders = false;
        Level = CapabilityLevel.Level0_CPU_Only;
        List<string> list = new List<string>();
        string text = "";
        try
        {
            try
            {
                GPUName = Display.GPUName;
            }
            catch
            {
                GPUName = "unknown";
            }
            if (dc != null && !dc.IsDisposed)
            {
                HasDeviceContext = true;
                list.Add("DeviceContext");
                text += "DeviceContext: OK\n";
                try
                {
                    using (Saturation saturation = new Saturation(dc))
                    {
                        saturation.Value = 0.5f;
                    }
                    HasBuiltInEffects = true;
                    Level = CapabilityLevel.Level1_Basic_Effects;
                    list.Add("Saturation");
                    text += "Built-in Saturation Effect: OK\n";
                    try
                    {
                        using (GammaTransfer gammaTransfer = new GammaTransfer(dc))
                        {
                            gammaTransfer.RedExponent = 1f;
                        }
                        list.Add("GammaTransfer");
                        text += "Built-in GammaTransfer Effect: OK\n";
                    }
                    catch (Exception ex)
                    {
                        text = text + "GammaTransfer: FAIL (" + ex.Message + ")\n";
                    }
                    try
                    {
                        using (ColorMatrix colorMatrix = new ColorMatrix(dc))
                        {
                            colorMatrix.ClampOutput = true;
                        }
                        list.Add("ColorMatrix");
                        text += "Built-in ColorMatrix Effect: OK\n";
                    }
                    catch (Exception ex2)
                    {
                        text = text + "ColorMatrix: FAIL (" + ex2.Message + ")\n";
                    }
                }
                catch (Exception ex3)
                {
                    text = text + "Built-in Effects: FAIL (" + ex3.Message + ")\n";
                }
                if (HasBuiltInEffects)
                {
                    try
                    {
                        try
                        {
                            factory.UnRegisterEffect<WaterfallEffectImpl>();
                        }
                        catch
                        {
                        }
                        factory.RegisterEffect<WaterfallEffectImpl>(WaterfallEffectImpl.CustomEffectGuid);
                        HasCustomShaders = true;
                        Level = CapabilityLevel.Level2_Custom_Shaders;
                        list.Add("CustomHLSL");
                        text += "RegisterEffect<WaterfallEffectImpl>: OK — Level 2\n";
                    }
                    catch (Exception ex4)
                    {
                        text = text + "RegisterEffect<T>(guid): FAIL (" + ex4.GetType().Name + ": " + ex4.Message + ")\n";
                        if (ex4.InnerException != null)
                        {
                            text = text + "  inner: " + ex4.InnerException.Message + "\n";
                        }
                        text = text + "  stack: " + ex4.StackTrace + "\n";
                    }
                }
            }
            else
            {
                text += "DeviceContext: null/disposed\n";
            }
        }
        catch (Exception ex5)
        {
            text = text + "Detection exception: " + ex5.Message + "\n";
        }
        FeaturesList = string.Join(", ", list);
        try
        {
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gpu_detection.log"), "Yurij-eu2av GPU Detection " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\nGPU: " + GPUName + "\nLevel: " + Level.ToString() + "\nFeatures: " + FeaturesList + "\nHasDeviceContext: " + HasDeviceContext + "\nHasBuiltInEffects: " + HasBuiltInEffects + "\nHasCustomShaders: " + HasCustomShaders + "\n\n" + text);
        }
        catch
        {
        }
    }
}
