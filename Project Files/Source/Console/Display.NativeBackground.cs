// Native D3D11 background compositor for Pan3D / native waterfall.
// Keeps the SDR-VST3 skin visible without opening a second Direct2D frame.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using MapMode = Vortice.Direct3D11.MapMode;

namespace Thetis
{
    partial class Display
    {
        private static ID3D11Texture2D _nativeBgTexture;
        private static ID3D11ShaderResourceView _nativeBgSRV;
        private static ID3D11VertexShader _nativeBgVS;
        private static ID3D11PixelShader _nativeBgPS;
        private static ID3D11Buffer _nativeBgCB;
        private static ID3D11SamplerState _nativeBgSampler;
        private static ID3D11BlendState _nativeBgBlend;
        private static ID3D11RasterizerState _nativeBgRS;
        private static bool _nativeBgPipelineBuilt;
        private static int _nativeBgWidth;
        private static int _nativeBgHeight;

        private const string NATIVE_BG_HLSL = @"
            cbuffer BgCB : register(b0) { float4 Dest; };
            Texture2D BgTex : register(t0);
            SamplerState BgSamp : register(s0);

            struct BGO { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            BGO vs_bg(uint vid : SV_VertexID)
            {
                BGO o;
                static const float2 q[4] =
                {
                    float2(0.0, 0.0), float2(1.0, 0.0),
                    float2(0.0, 1.0), float2(1.0, 1.0)
                };
                float2 uv = q[vid & 3u];
                float2 p = lerp(Dest.xy, Dest.zw, uv);
                o.pos = float4(p.x * 2.0 - 1.0, 1.0 - p.y * 2.0, 0.0, 1.0);
                o.uv = uv;
                return o;
            }

            float4 ps_bg(BGO i) : SV_Target
            {
                return BgTex.SampleLevel(BgSamp, i.uv, 0);
            }
            ";

        private static void ReleaseNativeBackgroundTexture()
        {
            _nativeBgSRV?.Dispose(); _nativeBgSRV = null;
            _nativeBgTexture?.Dispose(); _nativeBgTexture = null;
            _nativeBgWidth = 0;
            _nativeBgHeight = 0;
        }

        private static void ReleaseNativeBackgroundResources()
        {
            ReleaseNativeBackgroundTexture();
            _nativeBgVS?.Dispose(); _nativeBgVS = null;
            _nativeBgPS?.Dispose(); _nativeBgPS = null;
            _nativeBgCB?.Dispose(); _nativeBgCB = null;
            _nativeBgSampler?.Dispose(); _nativeBgSampler = null;
            _nativeBgBlend?.Dispose(); _nativeBgBlend = null;
            _nativeBgRS?.Dispose(); _nativeBgRS = null;
            _nativeBgPipelineBuilt = false;
        }

        private static bool EnsureNativeBackgroundPipeline()
        {
            if (_nativeBgPipelineBuilt) return true;
            if (_device == null) return false;

            try
            {
                byte[] vsBytes = Vortice.D3DCompiler.Compiler.Compile(
                    NATIVE_BG_HLSL, "vs_bg", "native_background.hlsl", "vs_5_0",
                    Vortice.D3DCompiler.ShaderFlags.None,
                    Vortice.D3DCompiler.EffectFlags.None).ToArray();
                byte[] psBytes = Vortice.D3DCompiler.Compiler.Compile(
                    NATIVE_BG_HLSL, "ps_bg", "native_background.hlsl", "ps_5_0",
                    Vortice.D3DCompiler.ShaderFlags.None,
                    Vortice.D3DCompiler.EffectFlags.None).ToArray();

                _nativeBgVS = _device.CreateVertexShader(vsBytes, null);
                _nativeBgPS = _device.CreatePixelShader(psBytes);
                _nativeBgCB = _device.CreateBuffer(new BufferDescription(
                    16, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
                _nativeBgSampler = _device.CreateSamplerState(new SamplerDescription(
                    Vortice.Direct3D11.Filter.MinMagMipLinear,
                    TextureAddressMode.Clamp, TextureAddressMode.Clamp, TextureAddressMode.Clamp));
                _nativeBgBlend = _device.CreateBlendState(Vortice.Direct3D11.BlendDescription.AlphaBlend);
                _nativeBgRS = _device.CreateRasterizerState(new Vortice.Direct3D11.RasterizerDescription(
                    CullMode.None, Vortice.Direct3D11.FillMode.Solid));

                _nativeBgPipelineBuilt = true;
                GPUWaterfallLogger.Log("BG-NATIVE", "D3D11 skin pipeline built");
                return true;
            }
            catch (Exception ex)
            {
                GPUWaterfallLogger.Log("BG-NATIVE-FAIL",
                    "pipeline " + ex.GetType().FullName + ": " + ex.Message);
                ReleaseNativeBackgroundResources();
                return false;
            }
        }

        private static void SetNativeBackgroundImage(System.Drawing.Image image)
        {
            ReleaseNativeBackgroundTexture();

            if (image == null || _device == null || image.Width <= 0 || image.Height <= 0)
                return;

            try
            {
                using (Bitmap bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(System.Drawing.Color.Transparent);
                        g.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    }

                    _nativeBgTexture = _device.CreateTexture2D(new Texture2DDescription
                    {
                        Width = (uint)bmp.Width,
                        Height = (uint)bmp.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = Format.B8G8R8A8_UNorm,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Dynamic,
                        BindFlags = BindFlags.ShaderResource,
                        CPUAccessFlags = CpuAccessFlags.Write
                    });

                    ID3D11DeviceContext dc = _device.ImmediateContext;
                    MappedSubresource mapped = dc.Map(
                        _nativeBgTexture, 0, MapMode.WriteDiscard,
                        Vortice.Direct3D11.MapFlags.None);

                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
                    try
                    {
                        int srcPitchAbs = Math.Abs(bd.Stride);
                        int rowBytes = bmp.Width * 4;
                        byte[] row = new byte[rowBytes];

                        for (int y = 0; y < bmp.Height; y++)
                        {
                            int srcOffset = bd.Stride >= 0
                                ? y * bd.Stride
                                : (bmp.Height - 1 - y) * srcPitchAbs;
                            Marshal.Copy(IntPtr.Add(bd.Scan0, srcOffset), row, 0, rowBytes);
                            Marshal.Copy(row, 0,
                                IntPtr.Add(mapped.DataPointer, y * (int)mapped.RowPitch), rowBytes);
                        }
                    }
                    finally
                    {
                        bmp.UnlockBits(bd);
                        dc.Unmap(_nativeBgTexture, 0);
                    }

                    _nativeBgSRV = _device.CreateShaderResourceView(_nativeBgTexture);
                    _nativeBgWidth = bmp.Width;
                    _nativeBgHeight = bmp.Height;
                }

                GPUWaterfallLogger.Log("BG-NATIVE",
                    "skin uploaded " + _nativeBgWidth + "x" + _nativeBgHeight);
            }
            catch (Exception ex)
            {
                ReleaseNativeBackgroundTexture();
                GPUWaterfallLogger.Log("BG-NATIVE-FAIL",
                    "upload " + ex.GetType().FullName + ": " + ex.Message);
            }
        }

        private static bool RenderNativeBackground()
        {
            if (_device == null || !_bDX2Setup || _swapChain1 == null) return false;
            if (!EnsureMeshRTV(_device)) return false;

            try
            {
                ID3D11DeviceContext dc = _device.ImmediateContext;

                dc.ClearRenderTargetView(_meshRTV, new Color4(
                    m_cDX2_display_background_clear_colour.R,
                    m_cDX2_display_background_clear_colour.G,
                    m_cDX2_display_background_clear_colour.B,
                    1f));

                if (_nativeBgSRV == null || _nativeBgWidth <= 0 || _nativeBgHeight <= 0)
                    return true;
                if (!EnsureNativeBackgroundPipeline()) return false;

                float x0 = 0f, y0 = 0f, x1 = 1f, y1 = 1f;
                if (_maintain_background_aspectratio && displayTargetWidth > 0 && displayTargetHeight > 0)
                {
                    float imageAspect = _nativeBgWidth / (float)_nativeBgHeight;
                    float targetAspect = displayTargetWidth / (float)displayTargetHeight;
                    if (imageAspect > targetAspect)
                    {
                        float scaledH = displayTargetWidth / imageAspect;
                        float marginY = (displayTargetHeight - scaledH) * 0.5f;
                        y0 = marginY / displayTargetHeight;
                        y1 = (marginY + scaledH) / displayTargetHeight;
                    }
                    else
                    {
                        float scaledW = displayTargetHeight * imageAspect;
                        float marginX = (displayTargetWidth - scaledW) * 0.5f;
                        x0 = marginX / displayTargetWidth;
                        x1 = (marginX + scaledW) / displayTargetWidth;
                    }
                }

                MappedSubresource cbMap = dc.Map(
                    _nativeBgCB, 0, MapMode.WriteDiscard,
                    Vortice.Direct3D11.MapFlags.None);
                Marshal.Copy(new float[] { x0, y0, x1, y1 }, 0, cbMap.DataPointer, 4);
                dc.Unmap(_nativeBgCB, 0);

                dc.OMSetRenderTargets(new[] { _meshRTV }, null);
                dc.OMSetBlendState(_nativeBgBlend);
                dc.RSSetState(_nativeBgRS);
                dc.RSSetViewport(new Viewport(
                    0f, 0f, (float)displayTargetWidth, (float)displayTargetHeight));
                dc.RSSetScissorRects(new[] { new Vortice.RawRect(
                    0, 0, (int)displayTargetWidth, (int)displayTargetHeight) });
                dc.IASetPrimitiveTopology(Vortice.Direct3D.PrimitiveTopology.TriangleStrip);
                dc.VSSetShader(_nativeBgVS);
                dc.VSSetConstantBuffer(0, _nativeBgCB);
                dc.PSSetShader(_nativeBgPS);
                dc.PSSetShaderResource(0, _nativeBgSRV);
                dc.PSSetSamplers(0, new[] { _nativeBgSampler });
                dc.Draw(4u, 0u);

                GPUWaterfallLogger.LogRateLimited(
                    "BG-NATIVE", "draw", 5000,
                    "skin drawn via D3D11; no D2D prepass");
                return true;
            }
            catch (Exception ex)
            {
                GPUWaterfallLogger.Log("BG-NATIVE-FAIL",
                    "render " + ex.GetType().FullName + ": " + ex.Message);
                return false;
            }
        }
    }
}
