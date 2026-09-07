#pragma once

#ifdef __cplusplus
extern "C" {
#endif

__declspec(dllexport) int __cdecl CM_GPUWaterfall_Init(int channel, int fftSize, int ringCapacity);
__declspec(dllexport) int __cdecl CM_GPUWaterfall_Configure(int channel, int windowType, float kaiserBeta,
    int magnitudeMode, int autoOverlap, float overlapPercent, int lanczosWindow, int resamplingMode);
__declspec(dllexport) int __cdecl CM_GPUWaterfall_Process(int channel, int displayWidth, int sampleRate,
    float displayLowHz, float displayHighHz, float* outputDb);
__declspec(dllexport) void __cdecl CM_GPUWaterfall_Free(int channel);
__declspec(dllexport) int __cdecl CM_GPUWaterfall_IsReady(int channel);

#ifdef __cplusplus
}

// SQ4KOU GPU detector compatibility surface used by the recovered EU2AV Waterfall setup.
// gpu_waterfall.h is included only by gpu_waterfall.cpp, after Windows/D3D11/D3DCompiler headers.
namespace sq4kou_gpu_detector
{
    struct DetectorState
    {
        int featureLevel;
        int customShaderOK;
        HRESULT lastError;
        char adapterName[256];
        char capabilities[512];

        DetectorState()
            : featureLevel(0), customShaderOK(0), lastError(E_FAIL)
        {
            adapterName[0] = 0;
            capabilities[0] = 0;
        }
    };

    static INIT_ONCE g_detectorOnce = INIT_ONCE_STATIC_INIT;
    static DetectorState g_detector;

    static void CopyAnsi(char* dst, int dstSize, const char* src)
    {
        if (!dst || dstSize <= 0) return;
        if (!src) src = "";
        strncpy_s(dst, (size_t)dstSize, src, _TRUNCATE);
    }

    static BOOL CALLBACK DetectOnce(PINIT_ONCE, PVOID, PVOID*)
    {
        ID3D11Device* device = 0;
        ID3D11DeviceContext* context = 0;
        D3D_FEATURE_LEVEL actual = D3D_FEATURE_LEVEL_9_1;
        D3D_FEATURE_LEVEL requested[] =
        {
            D3D_FEATURE_LEVEL_11_1,
            D3D_FEATURE_LEVEL_11_0,
            D3D_FEATURE_LEVEL_10_1,
            D3D_FEATURE_LEVEL_10_0
        };

        HRESULT hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0,
            requested, ARRAYSIZE(requested), D3D11_SDK_VERSION,
            &device, &actual, &context);
        if (hr == E_INVALIDARG)
        {
            hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0,
                &requested[1], ARRAYSIZE(requested) - 1, D3D11_SDK_VERSION,
                &device, &actual, &context);
        }

        g_detector.lastError = hr;
        if (FAILED(hr) || !device || !context)
        {
            CopyAnsi(g_detector.adapterName, 256, "D3D11 hardware GPU not detected");
            CopyAnsi(g_detector.capabilities, 512, "CPU fallback");
            if (context) context->Release();
            if (device) device->Release();
            return TRUE;
        }

        switch (actual)
        {
        case D3D_FEATURE_LEVEL_11_1: g_detector.featureLevel = 111; break;
        case D3D_FEATURE_LEVEL_11_0: g_detector.featureLevel = 110; break;
        case D3D_FEATURE_LEVEL_10_1: g_detector.featureLevel = 101; break;
        case D3D_FEATURE_LEVEL_10_0: g_detector.featureLevel = 100; break;
        default: g_detector.featureLevel = (int)actual; break;
        }

        IDXGIDevice* dxgiDevice = 0;
        IDXGIAdapter* adapter = 0;
        if (SUCCEEDED(device->QueryInterface(__uuidof(IDXGIDevice), (void**)&dxgiDevice)) && dxgiDevice)
        {
            if (SUCCEEDED(dxgiDevice->GetAdapter(&adapter)) && adapter)
            {
                DXGI_ADAPTER_DESC desc;
                ZeroMemory(&desc, sizeof(desc));
                if (SUCCEEDED(adapter->GetDesc(&desc)))
                {
                    WideCharToMultiByte(CP_ACP, 0, desc.Description, -1,
                        g_detector.adapterName, 256, 0, 0);
                }
            }
        }
        if (g_detector.adapterName[0] == 0)
            CopyAnsi(g_detector.adapterName, 256, "D3D11 hardware adapter");

        // Verify the capability that matters to the FFT backend: compile and create
        // a Shader Model 5 compute shader on the selected hardware device.
        static const char testShader[] =
            "RWStructuredBuffer<float> OutBuf : register(u0);"
            "[numthreads(1,1,1)] void CSMain(uint3 id : SV_DispatchThreadID) { if(id.x==0){} }";

        ID3DBlob* shaderBlob = 0;
        ID3DBlob* errors = 0;
        hr = D3DCompile(testShader, sizeof(testShader) - 1, "SQ4KOU_GPUDetector",
            0, 0, "CSMain", "cs_5_0",
            D3DCOMPILE_ENABLE_STRICTNESS | D3DCOMPILE_OPTIMIZATION_LEVEL3,
            0, &shaderBlob, &errors);
        if (errors) errors->Release();

        if (SUCCEEDED(hr) && shaderBlob)
        {
            ID3D11ComputeShader* cs = 0;
            hr = device->CreateComputeShader(shaderBlob->GetBufferPointer(),
                shaderBlob->GetBufferSize(), 0, &cs);
            if (SUCCEEDED(hr) && cs)
            {
                g_detector.customShaderOK = 1;
                cs->Release();
            }
            shaderBlob->Release();
        }
        g_detector.lastError = hr;

        if (g_detector.customShaderOK)
            CopyAnsi(g_detector.capabilities, 512, "Device/Context, DirectCompute, CustomHLSL");
        else
            CopyAnsi(g_detector.capabilities, 512, "Device/Context; DirectCompute custom shader test FAILED");

        if (adapter) adapter->Release();
        if (dxgiDevice) dxgiDevice->Release();
        context->Release();
        device->Release();
        return TRUE;
    }

    static DetectorState& GetState()
    {
        InitOnceExecuteOnce(&g_detectorOnce, DetectOnce, 0, 0);
        return g_detector;
    }
}

extern "C" {

__declspec(dllexport) int __cdecl CM_GPUDetector_GetAdapterName(char* buffer, int bufferSize)
{
    sq4kou_gpu_detector::DetectorState& s = sq4kou_gpu_detector::GetState();
    if (!buffer || bufferSize <= 0) return 0;
    sq4kou_gpu_detector::CopyAnsi(buffer, bufferSize, s.adapterName);
    return (int)strlen(buffer);
}

__declspec(dllexport) int __cdecl CM_GPUDetector_GetFeatureLevel(void)
{
    return sq4kou_gpu_detector::GetState().featureLevel;
}

__declspec(dllexport) int __cdecl CM_GPUDetector_GetCapabilities(char* buffer, int bufferSize)
{
    sq4kou_gpu_detector::DetectorState& s = sq4kou_gpu_detector::GetState();
    if (!buffer || bufferSize <= 0) return 0;
    sq4kou_gpu_detector::CopyAnsi(buffer, bufferSize, s.capabilities);
    return (int)strlen(buffer);
}

__declspec(dllexport) int __cdecl CM_GPUDetector_Test(void)
{
    return sq4kou_gpu_detector::GetState().customShaderOK;
}

}
#endif
