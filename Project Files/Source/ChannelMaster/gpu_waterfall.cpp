#define NOMINMAX
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
#define GPU_WF_AUTO_ROWS_PER_SEC 30.0

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
    g_output[i] = g_input[ReverseBits(i, g_bits)];
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

static double BesselI0(double x)
{
    double ax = fabs(x);
    if (ax < 3.75)
    {
        double y = x / 3.75;
        y *= y;
        return 1.0 + y * (3.5156229 + y * (3.0899424 + y * (1.2067492 +
            y * (0.2659732 + y * (0.0360768 + y * 0.0045813)))));
    }

    double y = 3.75 / ax;
    return (exp(ax) / sqrt(ax)) * (0.39894228 + y * (0.01328592 +
        y * (0.00225319 + y * (-0.00157565 + y * (0.00916281 +
        y * (-0.02057706 + y * (0.02635537 + y * (-0.01647633 + y * 0.00392377))))))));
}

static double Sinc(double x)
{
    const double pi = 3.14159265358979323846;
    if (fabs(x) < 1.0e-12) return 1.0;
    double px = pi * x;
    return sin(px) / px;
}

struct GPUWaterfallState
{
    int fftSize;
    int hopSize;
    bool primed;
    bool ready;
    HRESULT lastError;

    int windowType;          // 0 Hann, 1 Hamming, 2 Blackman-Harris, 3 Kaiser
    float kaiserBeta;
    int magnitudeMode;       // 0 dBFS, 1 PSD dBFS/Hz
    bool autoOverlap;
    float overlapPercent;
    int lanczosWindow;       // 2..4
    int resamplingMode;      // 0 Linear, 1 Power Average, 2 Peak, 3 Lanczos
    float coherentGain;
    float enbwBins;

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
    std::vector<float> window;

    GPUWaterfallState()
        : fftSize(0), hopSize(0), primed(false), ready(false), lastError(S_OK),
          windowType(0), kaiserBeta(8.6f), magnitudeMode(0), autoOverlap(true),
          overlapPercent(75.0f), lanczosWindow(3), resamplingMode(1),
          coherentGain(0.5f), enbwBins(1.5f),
          device(0), context(0), bitReverseCS(0), stageCS(0), magnitudeCS(0),
          inputBuffer(0), inputSRV(0), fftA(0), fftASRV(0), fftAUAV(0),
          fftB(0), fftBSRV(0), fftBUAV(0), magBuffer(0), magUAV(0), magStaging(0), paramsBuffer(0)
    {
    }

    void ReleaseResources()
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
        window.clear();
        fftSize = 0;
        hopSize = 0;
    }
};

static GPUWaterfallState g_gpuWaterfall[GPU_WF_MAX_CHANNELS];

static void BuildWindow(GPUWaterfallState& s)
{
    if (s.fftSize <= 0) return;
    s.window.resize(s.fftSize);

    const double pi2 = 6.28318530717958647692;
    double sum = 0.0;
    double sumSq = 0.0;
    double denomKaiser = BesselI0((double)s.kaiserBeta);
    if (denomKaiser <= 0.0) denomKaiser = 1.0;

    for (int i = 0; i < s.fftSize; ++i)
    {
        double t = s.fftSize > 1 ? (double)i / (double)(s.fftSize - 1) : 0.0;
        double w;
        switch (s.windowType)
        {
        case 1: // Hamming
            w = 0.54 - 0.46 * cos(pi2 * t);
            break;
        case 2: // Blackman-Harris 4-term
            w = 0.35875 - 0.48829 * cos(pi2 * t) + 0.14128 * cos(2.0 * pi2 * t) - 0.01168 * cos(3.0 * pi2 * t);
            break;
        case 3: // Kaiser
        {
            double x = 2.0 * t - 1.0;
            double a = 1.0 - x * x;
            if (a < 0.0) a = 0.0;
            w = BesselI0((double)s.kaiserBeta * sqrt(a)) / denomKaiser;
            break;
        }
        default: // Hann
            w = 0.5 - 0.5 * cos(pi2 * t);
            break;
        }
        s.window[i] = (float)w;
        sum += w;
        sumSq += w * w;
    }

    if (sum <= 1.0e-20)
    {
        s.coherentGain = 1.0f;
        s.enbwBins = 1.0f;
    }
    else
    {
        s.coherentGain = (float)(sum / (double)s.fftSize);
        s.enbwBins = (float)(((double)s.fftSize * sumSq) / (sum * sum));
        if (s.enbwBins < 1.0f) s.enbwBins = 1.0f;
    }
}

static void UpdateHopSize(GPUWaterfallState& s, int sampleRate)
{
    if (s.fftSize <= 0) return;
    int hop;
    if (s.autoOverlap && sampleRate > 0)
    {
        hop = (int)floor((double)sampleRate / GPU_WF_AUTO_ROWS_PER_SEC + 0.5);
        int minHop = std::max(1, (int)floor((double)s.fftSize * 0.05 + 0.5)); // 95% max overlap
        if (hop < minHop) hop = minHop;
        if (hop > s.fftSize) hop = s.fftSize;
    }
    else
    {
        double overlap = (double)s.overlapPercent;
        if (overlap < 0.0) overlap = 0.0;
        if (overlap > 95.0) overlap = 95.0;
        hop = (int)floor((double)s.fftSize * (1.0 - overlap / 100.0) + 0.5);
        if (hop < 1) hop = 1;
        if (hop > s.fftSize) hop = s.fftSize;
    }
    s.hopSize = hop;
}

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

    s.ReleaseResources();
    s.fftSize = fftSize;
    UpdateHopSize(s, 0);

    hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0, requested, ARRAYSIZE(requested),
        D3D11_SDK_VERSION, &s.device, &actual, &s.context);
    if (hr == E_INVALIDARG)
    {
        hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0, &requested[1], 1,
            D3D11_SDK_VERSION, &s.device, &actual, &s.context);
    }
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    ID3DBlob* blob = 0;
    hr = CompileCompute("BitReverseCS", &blob);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = s.device->CreateComputeShader(blob->GetBufferPointer(), blob->GetBufferSize(), 0, &s.bitReverseCS);
    blob->Release(); blob = 0;
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    hr = CompileCompute("StageCS", &blob);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = s.device->CreateComputeShader(blob->GetBufferPointer(), blob->GetBufferSize(), 0, &s.stageCS);
    blob->Release(); blob = 0;
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    hr = CompileCompute("MagnitudeCS", &blob);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = s.device->CreateComputeShader(blob->GetBufferPointer(), blob->GetBufferSize(), 0, &s.magnitudeCS);
    blob->Release(); blob = 0;
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    hr = CreateStructuredBuffer(s.device, fftSize, sizeof(Float2), D3D11_BIND_SHADER_RESOURCE,
        D3D11_USAGE_DYNAMIC, D3D11_CPU_ACCESS_WRITE, &s.inputBuffer);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = CreateSRV(s.device, s.inputBuffer, fftSize, &s.inputSRV);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    hr = CreateStructuredBuffer(s.device, fftSize, sizeof(Float2), D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS,
        D3D11_USAGE_DEFAULT, 0, &s.fftA);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = CreateSRV(s.device, s.fftA, fftSize, &s.fftASRV);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = CreateUAV(s.device, s.fftA, fftSize, &s.fftAUAV);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    hr = CreateStructuredBuffer(s.device, fftSize, sizeof(Float2), D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS,
        D3D11_USAGE_DEFAULT, 0, &s.fftB);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = CreateSRV(s.device, s.fftB, fftSize, &s.fftBSRV);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = CreateUAV(s.device, s.fftB, fftSize, &s.fftBUAV);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    hr = CreateStructuredBuffer(s.device, fftSize, sizeof(float), D3D11_BIND_UNORDERED_ACCESS,
        D3D11_USAGE_DEFAULT, 0, &s.magBuffer);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }
    hr = CreateUAV(s.device, s.magBuffer, fftSize, &s.magUAV);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    D3D11_BUFFER_DESC stagingDesc;
    ZeroMemory(&stagingDesc, sizeof(stagingDesc));
    stagingDesc.ByteWidth = fftSize * sizeof(float);
    stagingDesc.Usage = D3D11_USAGE_STAGING;
    stagingDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    hr = s.device->CreateBuffer(&stagingDesc, 0, &s.magStaging);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    D3D11_BUFFER_DESC cb;
    ZeroMemory(&cb, sizeof(cb));
    cb.ByteWidth = sizeof(GPUParams);
    cb.Usage = D3D11_USAGE_DEFAULT;
    cb.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    hr = s.device->CreateBuffer(&cb, 0, &s.paramsBuffer);
    if (FAILED(hr)) { s.lastError = hr; s.ReleaseResources(); return hr; }

    s.rolling.assign(fftSize, Float2{ 0.0f, 0.0f });
    s.tempI.resize(fftSize);
    s.tempQ.resize(fftSize);
    s.magnitude.resize(fftSize);
    BuildWindow(s);
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

    Float2* dst = (Float2*)mapped.pData;
    for (int i = 0; i < s.fftSize; ++i)
    {
        float w = s.window.empty() ? 1.0f : s.window[i];
        dst[i].x = s.rolling[i].x * w;
        dst[i].y = s.rolling[i].y * w;
    }
    s.context->Unmap(s.inputBuffer, 0);

    GPUParams p;
    ZeroMemory(&p, sizeof(p));
    p.N = (UINT)s.fftSize;
    p.bits = Log2Pow2((UINT)s.fftSize);
    p.coherentGain = s.coherentGain;
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
        ID3D11UnorderedAccessView* dstUAV = inputIsA ? s.fftBUAV : s.fftAUAV;
        s.context->CSSetShader(s.stageCS, 0, 0);
        s.context->CSSetShaderResources(0, 1, &src);
        s.context->CSSetUnorderedAccessViews(0, 1, &dstUAV, 0);
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

static double DbToPower(float db)
{
    return pow(10.0, (double)db / 10.0);
}

static float PowerToDb(double power)
{
    if (power < 1.0e-30) power = 1.0e-30;
    return (float)(10.0 * log10(power));
}

static float SampleLinear(const GPUWaterfallState& s, double pos)
{
    if (pos < 0.0) pos = 0.0;
    if (pos > (double)(s.fftSize - 1)) pos = (double)(s.fftSize - 1);
    int i0 = (int)floor(pos);
    int i1 = i0 + 1;
    if (i1 >= s.fftSize) i1 = s.fftSize - 1;
    double frac = pos - (double)i0;
    double p0 = DbToPower(s.magnitude[i0]);
    double p1 = DbToPower(s.magnitude[i1]);
    return PowerToDb(p0 + (p1 - p0) * frac);
}

static float SampleLanczos(const GPUWaterfallState& s, double pos)
{
    int a = s.lanczosWindow;
    if (a < 2) a = 2;
    if (a > 4) a = 4;
    int center = (int)floor(pos);
    double sum = 0.0;
    double weightSum = 0.0;

    for (int i = center - a + 1; i <= center + a; ++i)
    {
        if (i < 0 || i >= s.fftSize) continue;
        double x = pos - (double)i;
        if (fabs(x) >= (double)a) continue;
        double w = Sinc(x) * Sinc(x / (double)a);
        sum += DbToPower(s.magnitude[i]) * w;
        weightSum += w;
    }

    if (fabs(weightSum) < 1.0e-12) return SampleLinear(s, pos);
    double p = sum / weightSum;
    if (p <= 0.0) return SampleLinear(s, pos);
    return PowerToDb(p);
}

static float ResolvePixel(const GPUWaterfallState& s, double p0, double p1)
{
    if (p1 < p0)
    {
        double t = p0;
        p0 = p1;
        p1 = t;
    }
    if (p0 < 0.0) p0 = 0.0;
    if (p1 < 0.0) p1 = 0.0;
    if (p0 > (double)s.fftSize) p0 = (double)s.fftSize;
    if (p1 > (double)s.fftSize) p1 = (double)s.fftSize;

    double span = p1 - p0;
    double center = 0.5 * (p0 + p1);

    if (s.resamplingMode == 0)
        return SampleLinear(s, center);
    if (s.resamplingMode == 3 && span <= 1.5)
        return SampleLanczos(s, center);
    if (span <= 1.0)
        return SampleLinear(s, center);

    int firstBin = (int)floor(p0);
    int lastBin = (int)ceil(p1);
    double sumPower = 0.0;
    double sumWeight = 0.0;
    double peakPower = 0.0;

    for (int b = firstBin; b < lastBin; ++b)
    {
        if (b < 0 || b >= s.fftSize) continue;
        double left = p0 > (double)b ? p0 : (double)b;
        double right = p1 < (double)(b + 1) ? p1 : (double)(b + 1);
        double weight = right - left;
        if (weight <= 0.0) continue;

        double power = DbToPower(s.magnitude[b]);
        sumPower += power * weight;
        sumWeight += weight;
        if (power > peakPower) peakPower = power;
    }

    double meanPower = sumWeight > 0.0 ? sumPower / sumWeight : 1.0e-30;
    if (meanPower < 1.0e-30) meanPower = 1.0e-30;
    if (peakPower < meanPower) peakPower = meanPower;

    if (s.resamplingMode == 2)
        return PowerToDb(peakPower);

    // Power-average default. A small peak component preserves narrow carriers.
    return PowerToDb(0.88 * meanPower + 0.12 * peakPower);
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
        s.ReleaseResources();
        return 0;
    }
    CM_WaterfallIQ_ResetDropped(channel);
    CM_WaterfallIQ_SetEnabled(channel, 1);
    return 1;
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfall_Configure(int channel, int windowType, float kaiserBeta,
    int magnitudeMode, int autoOverlap, float overlapPercent, int lanczosWindow, int resamplingMode)
{
    if (channel < 0 || channel >= GPU_WF_MAX_CHANNELS) return 0;
    GPUWaterfallState& s = g_gpuWaterfall[channel];

    if (windowType < 0) windowType = 0;
    if (windowType > 3) windowType = 3;
    if (kaiserBeta < 0.0f) kaiserBeta = 0.0f;
    if (kaiserBeta > 20.0f) kaiserBeta = 20.0f;
    if (magnitudeMode < 0) magnitudeMode = 0;
    if (magnitudeMode > 1) magnitudeMode = 1;
    if (overlapPercent < 0.0f) overlapPercent = 0.0f;
    if (overlapPercent > 95.0f) overlapPercent = 95.0f;
    if (lanczosWindow < 2) lanczosWindow = 2;
    if (lanczosWindow > 4) lanczosWindow = 4;
    if (resamplingMode < 0) resamplingMode = 0;
    if (resamplingMode > 3) resamplingMode = 3;

    bool windowChanged = s.windowType != windowType || fabs((double)s.kaiserBeta - (double)kaiserBeta) > 0.0001;
    s.windowType = windowType;
    s.kaiserBeta = kaiserBeta;
    s.magnitudeMode = magnitudeMode;
    s.autoOverlap = autoOverlap != 0;
    s.overlapPercent = overlapPercent;
    s.lanczosWindow = lanczosWindow;
    s.resamplingMode = resamplingMode;

    if (s.ready && windowChanged)
        BuildWindow(s);
    if (s.ready)
        s.primed = false;
    return 1;
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfall_Process(int channel, int displayWidth, int sampleRate,
    float displayLowHz, float displayHighHz, float* outputDb)
{
    if (channel < 0 || channel >= GPU_WF_MAX_CHANNELS || displayWidth <= 0 || sampleRate <= 0 || outputDb == 0) return -1;
    GPUWaterfallState& s = g_gpuWaterfall[channel];
    if (!s.ready || s.fftSize <= 0) return -2;

    UpdateHopSize(s, sampleRate);
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
        if (keep > 0)
            memmove(&s.rolling[0], &s.rolling[s.hopSize], (size_t)keep * sizeof(Float2));
        int got = CM_WaterfallIQ_Get(channel, s.hopSize, &s.tempI[0], &s.tempQ[0]);
        if (got != s.hopSize) return 0;
        for (int i = 0; i < s.hopSize; ++i)
        {
            int dstIndex = keep + i;
            s.rolling[dstIndex].x = s.tempI[i];
            s.rolling[dstIndex].y = s.tempQ[i];
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

    float spanHz = displayHighHz - displayLowHz;
    float nyquist = 0.5f * (float)sampleRate;
    float psdCorrection = 0.0f;
    if (s.magnitudeMode == 1)
    {
        double binWidth = (double)sampleRate / (double)s.fftSize;
        double noiseBandwidth = binWidth * (double)s.enbwBins;
        if (noiseBandwidth > 1.0e-20)
            psdCorrection = (float)(10.0 * log10(noiseBandwidth));
    }

    for (int x = 0; x < displayWidth; ++x)
    {
        float fx0 = displayLowHz + spanHz * ((float)x / (float)displayWidth);
        float fx1 = displayLowHz + spanHz * ((float)(x + 1) / (float)displayWidth);
        double p0 = ((double)fx0 + (double)nyquist) / (double)sampleRate * (double)s.fftSize;
        double p1 = ((double)fx1 + (double)nyquist) / (double)sampleRate * (double)s.fftSize;
        outputDb[x] = ResolvePixel(s, p0, p1) - psdCorrection;
    }
    return 1;
}

extern "C" __declspec(dllexport) void __cdecl CM_GPUWaterfall_Free(int channel)
{
    if (channel < 0 || channel >= GPU_WF_MAX_CHANNELS) return;
    CM_WaterfallIQ_SetEnabled(channel, 0);
    CM_WaterfallIQ_Free(channel);
    g_gpuWaterfall[channel].ReleaseResources();
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfall_IsReady(int channel)
{
    if (channel < 0 || channel >= GPU_WF_MAX_CHANNELS) return 0;
    return g_gpuWaterfall[channel].ready ? 1 : 0;
}
