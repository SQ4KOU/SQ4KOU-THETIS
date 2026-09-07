#include <Windows.h>
#include <d3d11.h>
#include <d3dcompiler.h>
#include <stdint.h>
#include <math.h>
#include <vector>
#include <algorithm>
#include <cstring>
#include "waterfall_iq.h"
#include "gpu_waterfall.h"

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "d3dcompiler.lib")

#define GPU_WF_MAX_CHANNELS 8
#define GPU_WF_THREADS 256

struct Float2
{
    float x;
    float y;
};

struct GPUParams
{
    UINT N;
    UINT bits;
    UINT stage;
    UINT pad0;
    float coherentGain;
    float pad1;
    float pad2;
    float pad3;
};

static const char* kGPUWaterfallHlsl = R"HLSL(
cbuffer Params : register(b0)
{
    uint g_N;
    uint g_bits;
    uint g_stage;
    uint g_pad0;
    float g_coherentGain;
    float g_pad1;
    float g_pad2;
    float g_pad3;
};

StructuredBuffer<float2> g_input : register(t0);
RWStructuredBuffer<float2> g_output : register(u0);

uint ReverseBits(uint v, uint bits)
{
    uint r = 0;
    [loop]
    for (uint i = 0; i < bits; ++i)
    {
        r = (r << 1) | (v & 1);
        v >>= 1;
    }
    return r;
}

[numthreads(256,1,1)]
void BitReverseCS(uint3 tid : SV_DispatchThreadID)
{
    uint i = tid.x;
    if (i >= g_N) return;
    uint src = ReverseBits(i, g_bits);
    float2 z = g_input[src];
    float w = (g_N > 1) ? (0.5 - 0.5 * cos(6.2831853071795864769 * (float)src / (float)(g_N - 1))) : 1.0;
    g_output[i] = z * w;
}

StructuredBuffer<float2> g_stageInput : register(t0);
RWStructuredBuffer<float2> g_stageOutput : register(u0);

[numthreads(256,1,1)]
void StageCS(uint3 tid : SV_DispatchThreadID)
{
    uint k = tid.x;
    uint halfN = g_N >> 1;
    if (k >= halfN) return;

    uint m = 1u << g_stage;
    uint half = m >> 1;
    uint group = k / half;
    uint j = k - group * half;
    uint i0 = group * m + j;
    uint i1 = i0 + half;

    float angle = -6.2831853071795864769 * (float)j / (float)m;
    float s, c;
    sincos(angle, s, c);
    float2 a = g_stageInput[i0];
    float2 b = g_stageInput[i1];
    float2 t = float2(c * b.x - s * b.y, s * b.x + c * b.y);
    g_stageOutput[i0] = a + t;
    g_stageOutput[i1] = a - t;
}

StructuredBuffer<float2> g_fft : register(t0);
RWStructuredBuffer<float> g_mag : register(u0);

[numthreads(256,1,1)]
void MagnitudeCS(uint3 tid : SV_DispatchThreadID)
{
    uint dst = tid.x;
    if (dst >= g_N) return;
    uint src = (dst + (g_N >> 1)) & (g_N - 1);
    float2 z = g_fft[src];
    float mag = sqrt(z.x * z.x + z.y * z.y);
    float denom = max(1.0e-20, (float)g_N * max(g_coherentGain, 1.0e-12));
    float norm = max(mag / denom, 1.0e-20);
    g_mag[dst] = 20.0 * log10(norm);
}
)HLSL";

template <class T>
static void SafeRelease(T*& p)
{
    if (p)
    {
        p->Release();
        p = 0;
    }
}

static bool IsPowerOfTwo(int n)
{
    return n >= 1024 && n <= 262144 && (n & (n - 1)) == 0;
}

static UINT Log2Pow2(UINT n)
{
    UINT bits = 0;
    while ((1u << bits) < n) ++bits;
    return bits;
}

static HRESULT CompileCompute(const char* entry, ID3DBlob** blob)
{
    ID3DBlob* errors = 0;
    UINT flags = D3DCOMPILE_ENABLE_STRICTNESS | D3DCOMPILE_OPTIMIZATION_LEVEL3;
    HRESULT hr = D3DCompile(kGPUWaterfallHlsl, strlen(kGPUWaterfallHlsl), "SQ4KOU_GPUWaterfall", 0, 0,
        entry, "cs_5_0", flags, 0, blob, &errors);
    if (errors) errors->Release();
    return hr;
}

struct GPUWaterfallState
{
    int fftSize;
    int hopSize;
    bool primed;
    bool ready;
    HRESULT lastError;

    ID3D11Device* device;
    ID3D11DeviceContext* context;
    ID3D11ComputeShader* bitReverseCS;
    ID3D11ComputeShader* stageCS;
    ID3D11ComputeShader* magnitudeCS;
    ID3D11Buffer* inputBuffer;
    ID3D11ShaderResourceView* inputSRV;
    ID3D11Buffer* fftA;
    ID3D11ShaderResourceView* fftASRV;
    ID3D11UnorderedAccessView* fftAUAV;
    ID3D11Buffer* fftB;
    ID3D11ShaderResourceView* fftBSRV;
    ID3D11UnorderedAccessView* fftBUAV;
    ID3D11Buffer* magBuffer;
    ID3D11UnorderedAccessView* magUAV;
    ID3D11Buffer* magStaging;
    ID3D11Buffer* paramsBuffer;

    std::vector<Float2> rolling;
    std::vector<float> tempI;
    std::vector<float> tempQ;
    std::vector<float> magnitude;

    GPUWaterfallState()
        : fftSize(0), hopSize(0), primed(false), ready(false), lastError(S_OK),
          device(0), context(0), bitReverseCS(0), stageCS(0), magnitudeCS(0),
          inputBuffer(0), inputSRV(0), fftA(0), fftASRV(0), fftAUAV(0),
          fftB(0), fftBSRV(0), fftBUAV(0), magBuffer(0), magUAV(0), magStaging(0), paramsBuffer(0)
    {
    }

    void Release()
    {
        ready = false;
        primed = false;
        SafeRelease(paramsBuffer);
        SafeRelease(magStaging);
        SafeRelease(magUAV);
        SafeRelease(magBuffer);
        SafeRelease(fftBUAV);
        SafeRelease(fftBSRV);
        SafeRelease(fftB);
        SafeRelease(fftAUAV);
        SafeRelease(fftASRV);
        SafeRelease(fftA);
        SafeRelease(inputSRV);
        SafeRelease(inputBuffer);
        SafeRelease(magnitudeCS);
        SafeRelease(stageCS);
        SafeRelease(bitReverseCS);
        SafeRelease(context);
        SafeRelease(device);
        rolling.clear();
        tempI.clear();
        tempQ.clear();
        magnitude.clear();
        fftSize = 0;
        hopSize = 0;
    }
};

static GPUWaterfallState g_gpuWaterfall[GPU_WF_MAX_CHANNELS];

static HRESULT CreateStructuredBuffer(ID3D11Device* dev, UINT count, UINT stride, UINT bindFlags,
    D3D11_USAGE usage, UINT cpuAccess, ID3D11Buffer** buffer)
{
    D3D11_BUFFER_DESC d;
    ZeroMemory(&d, sizeof(d));
    d.ByteWidth = count * stride;
    d.Usage = usage;
    d.BindFlags = bindFlags;
    d.CPUAccessFlags = cpuAccess;
    d.MiscFlags = D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
    d.StructureByteStride = stride;
    return dev->CreateBuffer(&d, 0, buffer);
}

static HRESULT CreateSRV(ID3D11Device* dev, ID3D11Buffer* buffer, UINT count, ID3D11ShaderResourceView** srv)
{
    D3D11_SHADER_RESOURCE_VIEW_DESC d;
    ZeroMemory(&d, sizeof(d));
    d.Format = DXGI_FORMAT_UNKNOWN;
    d.ViewDimension = D3D11_SRV_DIMENSION_BUFFER;
    d.Buffer.FirstElement = 0;
    d.Buffer.NumElements = count;
    return dev->CreateShaderResourceView(buffer, &d, srv);
}

static HRESULT CreateUAV(ID3D11Device* dev, ID3D11Buffer* buffer, UINT count, ID3D11UnorderedAccessView** uav)
{
    D3D11_UNORDERED_ACCESS_VIEW_DESC d;
    ZeroMemory(&d, sizeof(d));
    d.Format = DXGI_FORMAT_UNKNOWN;
    d.ViewDimension = D3D11_UAV_DIMENSION_BUFFER;
    d.Buffer.FirstElement = 0;
    d.Buffer.NumElements = count;
    d.Buffer.Flags = 0;
    return dev->CreateUnorderedAccessView(buffer, &d, uav);
}

static HRESULT BuildGPUState(GPUWaterfallState& s, int fftSize)
{
    HRESULT hr;
    D3D_FEATURE_LEVEL requested[] = { D3D_FEATURE_LEVEL_11_1, D3D_FEATURE_LEVEL_11_0 };
    D3D_FEATURE_LEVEL actual = D3D_FEATURE_LEVEL_11_0;

    s.Release();
    s.fftSize = fftSize;
    s.hopSize = std::max(256, fftSize / 4); /* 75% overlap */

    hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0, requested, ARRAYSIZE(requested),
        D3D11_SDK_VERSION, &s.device, &actual, &s.context);
    if (hr == E_INVALIDARG)
    {
        hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0, &requested[1], 1,
            D3D11_SDK_VERSION, &s.device, &actual, &s.context);
    }
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    ID3DBlob* blob = 0;
    hr = CompileCompute("BitReverseCS", &blob);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = s.device->CreateComputeShader(blob->GetBufferPointer(), blob->GetBufferSize(), 0, &s.bitReverseCS);
    blob->Release(); blob = 0;
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    hr = CompileCompute("StageCS", &blob);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = s.device->CreateComputeShader(blob->GetBufferPointer(), blob->GetBufferSize(), 0, &s.stageCS);
    blob->Release(); blob = 0;
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    hr = CompileCompute("MagnitudeCS", &blob);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = s.device->CreateComputeShader(blob->GetBufferPointer(), blob->GetBufferSize(), 0, &s.magnitudeCS);
    blob->Release(); blob = 0;
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    hr = CreateStructuredBuffer(s.device, fftSize, sizeof(Float2), D3D11_BIND_SHADER_RESOURCE,
        D3D11_USAGE_DYNAMIC, D3D11_CPU_ACCESS_WRITE, &s.inputBuffer);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = CreateSRV(s.device, s.inputBuffer, fftSize, &s.inputSRV);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    hr = CreateStructuredBuffer(s.device, fftSize, sizeof(Float2), D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS,
        D3D11_USAGE_DEFAULT, 0, &s.fftA);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = CreateSRV(s.device, s.fftA, fftSize, &s.fftASRV);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = CreateUAV(s.device, s.fftA, fftSize, &s.fftAUAV);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    hr = CreateStructuredBuffer(s.device, fftSize, sizeof(Float2), D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS,
        D3D11_USAGE_DEFAULT, 0, &s.fftB);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = CreateSRV(s.device, s.fftB, fftSize, &s.fftBSRV);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = CreateUAV(s.device, s.fftB, fftSize, &s.fftBUAV);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    hr = CreateStructuredBuffer(s.device, fftSize, sizeof(float), D3D11_BIND_UNORDERED_ACCESS,
        D3D11_USAGE_DEFAULT, 0, &s.magBuffer);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }
    hr = CreateUAV(s.device, s.magBuffer, fftSize, &s.magUAV);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    D3D11_BUFFER_DESC stagingDesc;
    ZeroMemory(&stagingDesc, sizeof(stagingDesc));
    stagingDesc.ByteWidth = fftSize * sizeof(float);
    stagingDesc.Usage = D3D11_USAGE_STAGING;
    stagingDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    hr = s.device->CreateBuffer(&stagingDesc, 0, &s.magStaging);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    D3D11_BUFFER_DESC cb;
    ZeroMemory(&cb, sizeof(cb));
    cb.ByteWidth = sizeof(GPUParams);
    cb.Usage = D3D11_USAGE_DEFAULT;
    cb.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    hr = s.device->CreateBuffer(&cb, 0, &s.paramsBuffer);
    if (FAILED(hr)) { s.lastError = hr; s.Release(); return hr; }

    s.rolling.assign(fftSize, Float2{ 0.0f, 0.0f });
    s.tempI.resize(fftSize);
    s.tempQ.resize(fftSize);
    s.magnitude.resize(fftSize);
    s.ready = true;
    s.lastError = S_OK;
    return S_OK;
}

static void UnbindCompute(ID3D11DeviceContext* c)
{
    ID3D11ShaderResourceView* nullSRV[1] = { 0 };
    ID3D11UnorderedAccessView* nullUAV[1] = { 0 };
    c->CSSetShaderResources(0, 1, nullSRV);
    c->CSSetUnorderedAccessViews(0, 1, nullUAV, 0);
}

static HRESULT RunFFT(GPUWaterfallState& s)
{
    HRESULT hr;
    D3D11_MAPPED_SUBRESOURCE mapped;
    ZeroMemory(&mapped, sizeof(mapped));
    hr = s.context->Map(s.inputBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped);
    if (FAILED(hr)) return hr;
    memcpy(mapped.pData, &s.rolling[0], (size_t)s.fftSize * sizeof(Float2));
    s.context->Unmap(s.inputBuffer, 0);

    GPUParams p;
    ZeroMemory(&p, sizeof(p));
    p.N = (UINT)s.fftSize;
    p.bits = Log2Pow2((UINT)s.fftSize);
    p.coherentGain = 0.5f;
    s.context->UpdateSubresource(s.paramsBuffer, 0, 0, &p, 0, 0);
    s.context->CSSetConstantBuffers(0, 1, &s.paramsBuffer);

    s.context->CSSetShader(s.bitReverseCS, 0, 0);
    s.context->CSSetShaderResources(0, 1, &s.inputSRV);
    s.context->CSSetUnorderedAccessViews(0, 1, &s.fftAUAV, 0);
    s.context->Dispatch(((UINT)s.fftSize + GPU_WF_THREADS - 1) / GPU_WF_THREADS, 1, 1);
    UnbindCompute(s.context);

    bool inputIsA = true;
    for (UINT stage = 1; stage <= p.bits; ++stage)
    {
        p.stage = stage;
        s.context->UpdateSubresource(s.paramsBuffer, 0, 0, &p, 0, 0);
        ID3D11ShaderResourceView* src = inputIsA ? s.fftASRV : s.fftBSRV;
        ID3D11UnorderedAccessView* dst = inputIsA ? s.fftBUAV : s.fftAUAV;
        s.context->CSSetShader(s.stageCS, 0, 0);
        s.context->CSSetShaderResources(0, 1, &src);
        s.context->CSSetUnorderedAccessViews(0, 1, &dst, 0);
        s.context->Dispatch((((UINT)s.fftSize >> 1) + GPU_WF_THREADS - 1) / GPU_WF_THREADS, 1, 1);
        UnbindCompute(s.context);
        inputIsA = !inputIsA;
    }

    ID3D11ShaderResourceView* finalSRV = inputIsA ? s.fftASRV : s.fftBSRV;
    s.context->CSSetShader(s.magnitudeCS, 0, 0);
    s.context->CSSetShaderResources(0, 1, &finalSRV);
    s.context->CSSetUnorderedAccessViews(0, 1, &s.magUAV, 0);
    s.context->Dispatch(((UINT)s.fftSize + GPU_WF_THREADS - 1) / GPU_WF_THREADS, 1, 1);
    UnbindCompute(s.context);

    s.context->CopyResource(s.magStaging, s.magBuffer);
    ZeroMemory(&mapped, sizeof(mapped));
    hr = s.context->Map(s.magStaging, 0, D3D11_MAP_READ, 0, &mapped);
    if (FAILED(hr)) return hr;
    memcpy(&s.magnitude[0], mapped.pData, (size_t)s.fftSize * sizeof(float));
    s.context->Unmap(s.magStaging, 0);
    return S_OK;
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfall_Init(int channel, int fftSize, int ringCapacity)
{
    if (channel < 0 || channel >= GPU_WF_MAX_CHANNELS || !IsPowerOfTwo(fftSize)) return 0;
    GPUWaterfallState& s = g_gpuWaterfall[channel];
    if (s.ready && s.fftSize == fftSize)
    {
        CM_WaterfallIQ_SetEnabled(channel, 1);
        return 1;
    }

    if (FAILED(BuildGPUState(s, fftSize))) return 0;
    if (ringCapacity < fftSize * 4) ringCapacity = fftSize * 4;
    if (CM_WaterfallIQ_Init(channel, ringCapacity) <= 0)
    {
        s.Release();
        return 0;
    }
    CM_WaterfallIQ_ResetDropped(channel);
    CM_WaterfallIQ_SetEnabled(channel, 1);
    return 1;
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfall_Process(int channel, int displayWidth, int sampleRate,
    float displayLowHz, float displayHighHz, float* outputDb)
{
    if (channel < 0 || channel >= GPU_WF_MAX_CHANNELS || displayWidth <= 0 || sampleRate <= 0 || outputDb == 0) return -1;
    GPUWaterfallState& s = g_gpuWaterfall[channel];
    if (!s.ready || s.fftSize <= 0) return -2;

    int need = s.primed ? s.hopSize : s.fftSize;
    if (CM_WaterfallIQ_Available(channel) < need) return 0;

    if (!s.primed)
    {
        int got = CM_WaterfallIQ_Get(channel, s.fftSize, &s.tempI[0], &s.tempQ[0]);
        if (got != s.fftSize) return 0;
        for (int i = 0; i < s.fftSize; ++i)
        {
            s.rolling[i].x = s.tempI[i];
            s.rolling[i].y = s.tempQ[i];
        }
        s.primed = true;
    }
    else
    {
        int keep = s.fftSize - s.hopSize;
        memmove(&s.rolling[0], &s.rolling[s.hopSize], (size_t)keep * sizeof(Float2));
        int got = CM_WaterfallIQ_Get(channel, s.hopSize, &s.tempI[0], &s.tempQ[0]);
        if (got != s.hopSize) return 0;
        for (int i = 0; i < s.hopSize; ++i)
        {
            int dst = keep + i;
            s.rolling[dst].x = s.tempI[i];
            s.rolling[dst].y = s.tempQ[i];
        }
    }

    HRESULT hr = RunFFT(s);
    if (FAILED(hr))
    {
        s.lastError = hr;
        s.ready = false;
        CM_WaterfallIQ_SetEnabled(channel, 0);
        return -3;
    }

    float span = displayHighHz - displayLowHz;
    float nyquist = 0.5f * (float)sampleRate;
    for (int x = 0; x < displayWidth; ++x)
    {
        float t = displayWidth > 1 ? (float)x / (float)(displayWidth - 1) : 0.5f;
        float f = displayLowHz + span * t;
        float pos = ((f + nyquist) / (float)sampleRate) * (float)s.fftSize;
        if (pos < 0.0f) pos = 0.0f;
        if (pos > (float)(s.fftSize - 1)) pos = (float)(s.fftSize - 1);
        int i0 = (int)floorf(pos);
        int i1 = std::min(i0 + 1, s.fftSize - 1);
        float frac = pos - (float)i0;
        outputDb[x] = s.magnitude[i0] + (s.magnitude[i1] - s.magnitude[i0]) * frac;
    }
    return 1;
}

extern "C" __declspec(dllexport) void __cdecl CM_GPUWaterfall_Free(int channel)
{
    if (channel < 0 || channel >= GPU_WF_MAX_CHANNELS) return;
    CM_WaterfallIQ_SetEnabled(channel, 0);
    CM_WaterfallIQ_Free(channel);
    g_gpuWaterfall[channel].Release();
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfall_IsReady(int channel)
{
    if (channel < 0 || channel >= GPU_WF_MAX_CHANNELS) return 0;
    return g_gpuWaterfall[channel].ready ? 1 : 0;
}
