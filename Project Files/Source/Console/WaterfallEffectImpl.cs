using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Thetis;

[CustomEffect("Waterfall HDR post-processing (saturation, tone-map, gamma, dither)", "Waterfall", "Yurij-eu2av", DisplayName = "WaterfallPostProc")]
[Guid("7A2C3F4E-1B5D-4A6E-9C7F-8E0F1A2B3C4D")]
public class WaterfallEffectImpl : CustomEffectBase, DrawTransform, Transform, TransformNode, IUnknown, ICallbackable, IDisposable
{
    internal static readonly Guid CustomEffectGuid = new Guid("7A2C3F4E-1B5D-4A6E-9C7F-8E0F1A2B3C4D");
    private static readonly Guid ShaderGuid = new Guid("B3D4E5F6-2C6E-4A7F-8D9E-0F1A2B3C4D5E");
    private static byte[] _shaderBytecode;
    private EffectContext _context;

    [PropertyBinding(0, "0.0", "4.0", "1.0", DisplayName = "Saturation")]
    public float Saturation { get; set; } = 1f;

    [PropertyBinding(1, "0.1", "4.0", "1.0", DisplayName = "Gamma")]
    public float Gamma { get; set; } = 1f;

    [PropertyBinding(2, "0", "2", "0", DisplayName = "ToneMapMode")]
    public int ToneMapMode { get; set; }

    [PropertyBinding(3, "0.0", "0.1", "0.0", DisplayName = "DitherAmount")]
    public float DitherAmount { get; set; }

    public int InputCount => 1;

    private static byte[] GetShaderBytecode()
    {
        if (_shaderBytecode != null) return _shaderBytecode;
        try
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string text = null;
            string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
            foreach (string text2 in manifestResourceNames)
            {
                if (text2.EndsWith("waterfall_postproc.bin", StringComparison.OrdinalIgnoreCase))
                {
                    text = text2;
                    break;
                }
            }
            if (text != null)
            {
                using Stream stream = executingAssembly.GetManifestResourceStream(text);
                _shaderBytecode = new byte[stream.Length];
                stream.Read(_shaderBytecode, 0, _shaderBytecode.Length);
                return _shaderBytecode;
            }
            string path = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "waterfall_postproc.bin");
            if (File.Exists(path)) _shaderBytecode = File.ReadAllBytes(path);
        }
        catch
        {
        }
        return _shaderBytecode;
    }

    public override void Initialize(EffectContext context, TransformGraph graph)
    {
        _context = context;
        byte[] shaderBytecode = GetShaderBytecode();
        if (shaderBytecode == null || shaderBytecode.Length == 0) throw new InvalidOperationException("waterfall_postproc.bin not found");
        context.LoadPixelShader(ShaderGuid, shaderBytecode);
        graph.SetOutputNode(this);
    }

    public override void PrepareForRender(ChangeType changeType)
    {
    }

    public void SetDrawInformation(DrawInformation drawInfo)
    {
        drawInfo.SetPixelShader(ShaderGuid, PixelOptions.None);
        WaterfallEffectParams structure = new WaterfallEffectParams
        {
            Saturation = Saturation,
            Gamma = Gamma,
            ToneMapMode = ToneMapMode,
            DitherAmount = DitherAmount
        };
        using DataStream dataStream = new DataStream(Marshal.SizeOf(typeof(WaterfallEffectParams)), canRead: true, canWrite: true);
        Marshal.StructureToPtr(structure, dataStream.DataPointer, fDeleteOld: false);
        dataStream.Position = 0L;
        drawInfo.SetPixelConstantBuffer(dataStream);
    }

    public void MapOutputRectangleToInputRectangles(RawRectangle outputRect, RawRectangle[] inputRects)
    {
        inputRects[0] = outputRect;
    }

    public RawRectangle MapInputRectanglesToOutputRectangle(RawRectangle[] inputRects, RawRectangle[] inputOpaqueRects, out RawRectangle outputRect)
    {
        outputRect = inputRects[0];
        return outputRect;
    }

    public RawRectangle MapInvalidRect(int inputIndex, RawRectangle invalidInputRect)
    {
        return invalidInputRect;
    }
}
