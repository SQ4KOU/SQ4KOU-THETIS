#define NOMINMAX
#include <Windows.h>
#include <d3d11.h>
#include <stdint.h>
#include <math.h>
#include <stdio.h>
#include <vector>
#include <string>
#include <algorithm>
#include "gpu_waterfall_exact.h"

#pragma comment(lib, "d3d11.lib")

#define EXACT_WF_MAX_CHANNELS 8
#define EXACT_WF_THREADS 256

struct Float2 { float x; float y; };

#pragma pack(push, 4)
struct FFTParamsExact
{
    uint32_t N;
    uint32_t Bits;
    uint32_t Stage;
    uint32_t DisplayWidth;
    int32_t FirstBin;
    uint32_t BinCount;
    float SampleRate;
    float WindowPower;
    float CoherentGain;
    int32_t MagnitudeMode;
    float DisplayLowFreq;
    float DisplayHighFreq;
    float BinWidth;
    uint32_t Padding;
    int32_t LanczosWindow;
    int32_t ResamplingMode;
};
#pragma pack(pop)
static_assert(sizeof(FFTParamsExact) == 64, "FFTParamsExact layout must match ff62e7ad shader contract");

template <class T> static void SafeRelease(T*& p) { if (p) { p->Release(); p = 0; } }

static unsigned Log2Pow2(unsigned n)
{
    unsigned bits = 0;
    while ((1u << bits) < n) ++bits;
    return bits;
}

static bool IsPowerOfTwo(int n)
{
    return n >= 1024 && n <= 262144 && (n & (n - 1)) == 0;
}

static double BesselI0(double x)
{
    double ax = fabs(x);
    if (ax < 3.75)
    {
        double y = x / 3.75; y *= y;
        return 1.0 + y * (3.5156229 + y * (3.0899424 + y * (1.2067492 +
            y * (0.2659732 + y * (0.0360768 + y * 0.0045813)))));
    }
    double y = 3.75 / ax;
    return (exp(ax) / sqrt(ax)) * (0.39894228 + y * (0.01328592 +
        y * (0.00225319 + y * (-0.00157565 + y * (0.00916281 +
        y * (-0.02057706 + y * (0.02635537 + y * (-0.01647633 + y * 0.00392377))))))));
}

static bool ReadFileBytes(const wchar_t* name, std::vector<unsigned char>& out)
{
    wchar_t path[MAX_PATH];
    DWORD n = GetModuleFileNameW(0, path, MAX_PATH);
    if (!n || n >= MAX_PATH) return false;
    wchar_t* slash = wcsrchr(path, L'\\');
    if (!slash) return false;
    *(slash + 1) = 0;
    std::wstring full(path);
    full += name;

    FILE* f = 0;
    if (_wfopen_s(&f, full.c_str(), L"rb") != 0 || !f) return false;
    fseek(f, 0, SEEK_END);
    long len = ftell(f);
    fseek(f, 0, SEEK_SET);
    if (len <= 0) { fclose(f); return false; }
    out.resize((size_t)len);
    size_t got = fread(out.data(), 1, out.size(), f);
    fclose(f);
    return got == out.size();
}

struct ExactState
{
    int fftSize = 0;
    int displayWidth = 0;
    unsigned bits = 0;
    int windowType = 4;       // Nuttall
    float kaiserBeta = 6.0f;
    int magnitudeMode = 2;    // PeakHoldPower
    int lanczosWindow = 3;
    int resamplingMode = 1;   // Quality
    float windowPower = 1.0f;
    float coherentGain = 1.0f;
    bool ready = false;

    ID3D11Device* device = 0;
    ID3D11DeviceContext* context = 0;
    ID3D11ComputeShader* bitReverse = 0;
    ID3D11ComputeShader* stageAB = 0;
    ID3D11ComputeShader* stageBA = 0;
    ID3D11ComputeShader* magnitude = 0;

    ID3D11Buffer* input = 0;
    ID3D11ShaderResourceView* inputSRV = 0;
    ID3D11Buffer* window = 0;
    ID3D11ShaderResourceView* windowSRV = 0;
    ID3D11Buffer* fftA = 0;
    ID3D11UnorderedAccessView* fftAUAV = 0;
    ID3D11Buffer* fftB = 0;
    ID3D11UnorderedAccessView* fftBUAV = 0;
    ID3D11Buffer* mag = 0;
    ID3D11UnorderedAccessView* magUAV = 0;
    ID3D11Buffer* magStaging = 0;
    ID3D11Buffer* constants = 0;

    std::vector<Float2> iq;
    std::vector<float> win;

    void Release()
    {
        ready = false;
        SafeRelease(constants);
        SafeRelease(magStaging);
        SafeRelease(magUAV);
        SafeRelease(mag);
        SafeRelease(fftBUAV);
        SafeRelease(fftB);
        SafeRelease(fftAUAV);
        SafeRelease(fftA);
        SafeRelease(windowSRV);
        SafeRelease(window);
        SafeRelease(inputSRV);
        SafeRelease(input);
        SafeRelease(magnitude);
        SafeRelease(stageBA);
        SafeRelease(stageAB);
        SafeRelease(bitReverse);
        SafeRelease(context);
        SafeRelease(device);
        fftSize = displayWidth = 0;
        iq.clear();
        win.clear();
    }
};

static ExactState g_exact[EXACT_WF_MAX_CHANNELS];

static HRESULT CreateStructuredBuffer(ID3D11Device* dev, UINT bytes, UINT stride, UINT bind, ID3D11Buffer** out)
{
    D3D11_BUFFER_DESC d = {};
    d.ByteWidth = bytes;
    d.Usage = D3D11_USAGE_DEFAULT;
    d.BindFlags = bind;
    d.CPUAccessFlags = 0;
    d.MiscFlags = D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
    d.StructureByteStride = stride;
    return dev->CreateBuffer(&d, 0, out);
}

static HRESULT CreateBufferSRV(ID3D11Device* dev, ID3D11Buffer* buffer, UINT elements, ID3D11ShaderResourceView** out)
{
    D3D11_SHADER_RESOURCE_VIEW_DESC d = {};
    d.Format = DXGI_FORMAT_UNKNOWN;
    d.ViewDimension = D3D11_SRV_DIMENSION_BUFFER;
    d.Buffer.FirstElement = 0;
    d.Buffer.NumElements = elements;
    return dev->CreateShaderResourceView(buffer, &d, out);
}

static HRESULT CreateBufferUAV(ID3D11Device* dev, ID3D11Buffer* buffer, UINT elements, ID3D11UnorderedAccessView** out)
{
    D3D11_UNORDERED_ACCESS_VIEW_DESC d = {};
    d.Format = DXGI_FORMAT_UNKNOWN;
    d.ViewDimension = D3D11_UAV_DIMENSION_BUFFER;
    d.Buffer.FirstElement = 0;
    d.Buffer.NumElements = elements;
    d.Buffer.Flags = 0;
    return dev->CreateUnorderedAccessView(buffer, &d, out);
}

static bool LoadShaders(ExactState& s)
{
    std::vector<unsigned char> a,b,c,d;
    if (!ReadFileBytes(L"waterfall_fft_bitreverse_cs.bin", a) ||
        !ReadFileBytes(L"waterfall_fft_stage_ab_cs.bin", b) ||
        !ReadFileBytes(L"waterfall_fft_stage_ba_cs.bin", c) ||
        !ReadFileBytes(L"waterfall_fft_magnitude_cs.bin", d))
        return false;

    if (FAILED(s.device->CreateComputeShader(a.data(), a.size(), 0, &s.bitReverse))) return false;
    if (FAILED(s.device->CreateComputeShader(b.data(), b.size(), 0, &s.stageAB))) return false;
    if (FAILED(s.device->CreateComputeShader(c.data(), c.size(), 0, &s.stageBA))) return false;
    if (FAILED(s.device->CreateComputeShader(d.data(), d.size(), 0, &s.magnitude))) return false;
    return true;
}

static void BuildWindow(ExactState& s)
{
    s.win.assign((size_t)s.fftSize, 0.0f);
    double denom = (double)s.fftSize - 1.0;
    double sum = 0.0, sumSq = 0.0;

    for (int i = 0; i < s.fftSize; ++i)
    {
        double a = 6.28318530717958647692 * (double)i / denom;
        double w;
        switch (s.windowType)
        {
        case 0: w = 0.5 - 0.5 * cos(a); break;
        case 1: w = 0.54 - 0.46 * cos(a); break;
        case 2: w = 0.42 - 0.5 * cos(a) + 0.08 * cos(2.0 * a); break;
        case 3: w = 0.4243801 - 0.4973406 * cos(a) + 0.0782793 * cos(2.0 * a); break;
        case 5:
        {
            double half = denom * 0.5;
            double x = ((double)i - half) / half;
            w = BesselI0((double)s.kaiserBeta * sqrt(std::max(0.0, 1.0 - x*x))) / BesselI0((double)s.kaiserBeta);
            break;
        }
        default:
            // Exact Nuttall coefficients used by ff62e7ad GPUWaterfallPipeline.cs.
            w = 0.3635819 - 0.4891775 * cos(a) + 0.1365995 * cos(2.0 * a) - 0.0106411 * cos(3.0 * a);
            break;
        }
        s.win[(size_t)i] = (float)w;
        sum += w;
        sumSq += w * w;
    }
    s.windowPower = (float)(sumSq / (double)s.fftSize);
    s.coherentGain = (float)(sum / (double)s.fftSize);
    s.context->UpdateSubresource(s.window, 0, 0, s.win.data(), 0, 0);
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfallExact_Init(int channel, int fftSize, int displayWidth)
{
    if (channel < 0 || channel >= EXACT_WF_MAX_CHANNELS || !IsPowerOfTwo(fftSize) ||
        displayWidth <= 0 || displayWidth > 8192) return 0;

    ExactState& s = g_exact[channel];
    if (s.ready && s.fftSize == fftSize && s.displayWidth == displayWidth) return 1;
    s.Release();

    D3D_FEATURE_LEVEL levels[] = { D3D_FEATURE_LEVEL_11_1, D3D_FEATURE_LEVEL_11_0, D3D_FEATURE_LEVEL_10_1, D3D_FEATURE_LEVEL_10_0 };
    D3D_FEATURE_LEVEL actual;
    HRESULT hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0, levels, ARRAYSIZE(levels),
        D3D11_SDK_VERSION, &s.device, &actual, &s.context);
    if (FAILED(hr))
    {
        D3D_FEATURE_LEVEL levels2[] = { D3D_FEATURE_LEVEL_11_0, D3D_FEATURE_LEVEL_10_1, D3D_FEATURE_LEVEL_10_0 };
        hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0, levels2, ARRAYSIZE(levels2),
            D3D11_SDK_VERSION, &s.device, &actual, &s.context);
    }
    if (FAILED(hr)) { s.Release(); return 0; }

    s.fftSize = fftSize;
    s.displayWidth = displayWidth;
    s.bits = Log2Pow2((unsigned)fftSize);

    if (!LoadShaders(s)) { s.Release(); return 0; }

    if (FAILED(CreateStructuredBuffer(s.device, (UINT)fftSize * 8u, 8u, D3D11_BIND_SHADER_RESOURCE, &s.input)) ||
        FAILED(CreateBufferSRV(s.device, s.input, (UINT)fftSize, &s.inputSRV)) ||
        FAILED(CreateStructuredBuffer(s.device, (UINT)fftSize * 4u, 4u, D3D11_BIND_SHADER_RESOURCE, &s.window)) ||
        FAILED(CreateBufferSRV(s.device, s.window, (UINT)fftSize, &s.windowSRV)) ||
        FAILED(CreateStructuredBuffer(s.device, (UINT)fftSize * 8u, 8u, D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS, &s.fftA)) ||
        FAILED(CreateBufferUAV(s.device, s.fftA, (UINT)fftSize, &s.fftAUAV)) ||
        FAILED(CreateStructuredBuffer(s.device, (UINT)fftSize * 8u, 8u, D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS, &s.fftB)) ||
        FAILED(CreateBufferUAV(s.device, s.fftB, (UINT)fftSize, &s.fftBUAV)) ||
        FAILED(CreateStructuredBuffer(s.device, (UINT)displayWidth * 4u, 4u, D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS, &s.mag)) ||
        FAILED(CreateBufferUAV(s.device, s.mag, (UINT)displayWidth, &s.magUAV)))
    {
        s.Release(); return 0;
    }

    D3D11_BUFFER_DESC rd = {};
    rd.ByteWidth = (UINT)displayWidth * 4u;
    rd.Usage = D3D11_USAGE_STAGING;
    rd.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    if (FAILED(s.device->CreateBuffer(&rd, 0, &s.magStaging))) { s.Release(); return 0; }

    D3D11_BUFFER_DESC cb = {};
    cb.ByteWidth = 64;
    cb.Usage = D3D11_USAGE_DEFAULT;
    cb.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    if (FAILED(s.device->CreateBuffer(&cb, 0, &s.constants))) { s.Release(); return 0; }

    s.iq.resize((size_t)fftSize);
    BuildWindow(s);
    s.ready = true;
    return 1;
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfallExact_Configure(
    int channel, int windowType, float kaiserBeta, int magnitudeMode,
    int lanczosWindow, int resamplingMode)
{
    if (channel < 0 || channel >= EXACT_WF_MAX_CHANNELS) return 0;
    ExactState& s = g_exact[channel];
    if (!s.ready) return 0;
    s.windowType = windowType;
    s.kaiserBeta = kaiserBeta;
    s.magnitudeMode = magnitudeMode;
    s.lanczosWindow = lanczosWindow;
    s.resamplingMode = resamplingMode;
    BuildWindow(s);
    return 1;
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUWaterfallExact_Process(
    int channel, int sampleRate, float displayLowHz, float displayHighHz,
    const float* iData, const float* qData, int count, float* outputDb)
{
    if (channel < 0 || channel >= EXACT_WF_MAX_CHANNELS || !iData || !qData || !outputDb) return -1;
    ExactState& s = g_exact[channel];
    if (!s.ready || count < s.fftSize || sampleRate <= 0) return -2;

    for (int i = 0; i < s.fftSize; ++i)
    {
        s.iq[(size_t)i].x = iData[i];
        s.iq[(size_t)i].y = qData[i];
    }
    s.context->UpdateSubresource(s.input, 0, 0, s.iq.data(), 0, 0);

    FFTParamsExact p = {};
    p.N = (uint32_t)s.fftSize;
    p.Bits = s.bits;
    p.Stage = 0;
    p.DisplayWidth = (uint32_t)s.displayWidth;
    p.FirstBin = 0;
    p.BinCount = (uint32_t)s.fftSize;
    p.SampleRate = (float)sampleRate;
    p.WindowPower = s.windowPower;
    p.CoherentGain = s.coherentGain;
    p.MagnitudeMode = s.magnitudeMode;
    p.DisplayLowFreq = displayLowHz;
    p.DisplayHighFreq = displayHighHz;
    p.BinWidth = (float)sampleRate / (float)s.fftSize;
    p.Padding = 0;
    p.LanczosWindow = s.lanczosWindow;
    p.ResamplingMode = s.resamplingMode;

    ID3D11UnorderedAccessView* nullUAV = 0;
    ID3D11ShaderResourceView* nullSRV = 0;

    s.context->CSSetShader(s.bitReverse, 0, 0);
    s.context->CSSetConstantBuffers(0, 1, &s.constants);
    ID3D11ShaderResourceView* srvs[2] = { s.inputSRV, s.windowSRV };
    s.context->CSSetShaderResources(0, 2, srvs);
    s.context->CSSetUnorderedAccessViews(0, 1, &s.fftAUAV, 0);
    s.context->CSSetUnorderedAccessViews(1, 1, &nullUAV, 0);
    s.context->CSSetUnorderedAccessViews(2, 1, &nullUAV, 0);
    s.context->UpdateSubresource(s.constants, 0, 0, &p, 0, 0);
    s.context->Dispatch(((UINT)s.fftSize + 255u) / 256u, 1, 1);

    for (uint32_t stage = 1; stage <= s.bits; ++stage)
    {
        p.Stage = stage;
        s.context->UpdateSubresource(s.constants, 0, 0, &p, 0, 0);
        s.context->CSSetShader((stage & 1u) ? s.stageAB : s.stageBA, 0, 0);
        ID3D11UnorderedAccessView* uavs[2] = { s.fftAUAV, s.fftBUAV };
        s.context->CSSetUnorderedAccessViews(0, 2, uavs, 0);
        s.context->Dispatch((((UINT)s.fftSize >> 1) + 255u) / 256u, 1, 1);
    }

    if ((s.bits & 1u) != 0)
    {
        ID3D11UnorderedAccessView* nu[2] = { 0, 0 };
        s.context->CSSetUnorderedAccessViews(0, 2, nu, 0);
        s.context->CopyResource(s.fftA, s.fftB);
    }

    s.context->CSSetShader(s.magnitude, 0, 0);
    ID3D11UnorderedAccessView* mu[3] = { s.fftAUAV, 0, s.magUAV };
    s.context->CSSetUnorderedAccessViews(0, 3, mu, 0);
    p.Stage = s.bits;
    s.context->UpdateSubresource(s.constants, 0, 0, &p, 0, 0);
    s.context->Dispatch(((UINT)s.displayWidth + 255u) / 256u, 1, 1);

    ID3D11UnorderedAccessView* clearU[3] = { 0,0,0 };
    s.context->CSSetUnorderedAccessViews(0, 3, clearU, 0);
    ID3D11ShaderResourceView* clearS[2] = { 0,0 };
    s.context->CSSetShaderResources(0, 2, clearS);
    s.context->CSSetShader(0,0,0);

    s.context->CopyResource(s.magStaging, s.mag);
    D3D11_MAPPED_SUBRESOURCE mapped = {};
    HRESULT hr = s.context->Map(s.magStaging, 0, D3D11_MAP_READ, 0, &mapped);
    if (FAILED(hr) || !mapped.pData) return -3;
    memcpy(outputDb, mapped.pData, (size_t)s.displayWidth * sizeof(float));
    s.context->Unmap(s.magStaging, 0);
    return 1;
}

extern "C" __declspec(dllexport) void __cdecl CM_GPUWaterfallExact_Free(int channel)
{
    if (channel < 0 || channel >= EXACT_WF_MAX_CHANNELS) return;
    g_exact[channel].Release();
}
