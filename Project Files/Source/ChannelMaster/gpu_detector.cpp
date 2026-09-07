#define NOMINMAX
#include <Windows.h>
#include <d3d11.h>
#include <d3dcompiler.h>
#include <dxgi.h>
#include <cstring>
#include "gpu_detector.h"

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "d3dcompiler.lib")
#pragma comment(lib, "dxgi.lib")

template <class T>
static void SafeRelease(T*& p)
{
    if (p) { p->Release(); p = 0; }
}

static HRESULT CreateHardwareDevice(ID3D11Device** device, ID3D11DeviceContext** context, D3D_FEATURE_LEVEL* level)
{
    D3D_FEATURE_LEVEL requested[] = { D3D_FEATURE_LEVEL_11_1, D3D_FEATURE_LEVEL_11_0 };
    HRESULT hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0,
        requested, ARRAYSIZE(requested), D3D11_SDK_VERSION, device, level, context);
    if (hr == E_INVALIDARG)
    {
        hr = D3D11CreateDevice(0, D3D_DRIVER_TYPE_HARDWARE, 0, 0,
            &requested[1], 1, D3D11_SDK_VERSION, device, level, context);
    }
    return hr;
}

static int CopyAnsi(const char* src, char* buffer, int bufferSize)
{
    if (!buffer || bufferSize <= 0) return 0;
    if (!src) src = "";
    size_t n = strlen(src);
    if ((int)n >= bufferSize) n = (size_t)(bufferSize - 1);
    memcpy(buffer, src, n);
    buffer[n] = 0;
    return (int)n;
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUDetector_GetAdapterName(char* buffer, int bufferSize)
{
    if (!buffer || bufferSize <= 0) return 0;
    buffer[0] = 0;

    ID3D11Device* device = 0;
    ID3D11DeviceContext* context = 0;
    D3D_FEATURE_LEVEL level = D3D_FEATURE_LEVEL_11_0;
    HRESULT hr = CreateHardwareDevice(&device, &context, &level);
    if (FAILED(hr)) return 0;

    IDXGIDevice* dxgiDevice = 0;
    IDXGIAdapter* adapter = 0;
    DXGI_ADAPTER_DESC desc;
    ZeroMemory(&desc, sizeof(desc));

    hr = device->QueryInterface(__uuidof(IDXGIDevice), (void**)&dxgiDevice);
    if (SUCCEEDED(hr)) hr = dxgiDevice->GetAdapter(&adapter);
    if (SUCCEEDED(hr)) hr = adapter->GetDesc(&desc);

    int result = 0;
    if (SUCCEEDED(hr))
    {
        char name[256];
        int written = WideCharToMultiByte(CP_ACP, 0, desc.Description, -1, name, (int)sizeof(name), 0, 0);
        if (written > 0) result = CopyAnsi(name, buffer, bufferSize);
    }

    SafeRelease(adapter);
    SafeRelease(dxgiDevice);
    SafeRelease(context);
    SafeRelease(device);
    return result;
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUDetector_GetFeatureLevel(void)
{
    ID3D11Device* device = 0;
    ID3D11DeviceContext* context = 0;
    D3D_FEATURE_LEVEL level = D3D_FEATURE_LEVEL_11_0;
    HRESULT hr = CreateHardwareDevice(&device, &context, &level);
    SafeRelease(context);
    SafeRelease(device);
    if (FAILED(hr)) return 0;

    switch (level)
    {
        case D3D_FEATURE_LEVEL_11_1: return 111;
        case D3D_FEATURE_LEVEL_11_0: return 110;
        case D3D_FEATURE_LEVEL_10_1: return 101;
        case D3D_FEATURE_LEVEL_10_0: return 100;
        default: return (int)level;
    }
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUDetector_GetCapabilities(char* buffer, int bufferSize)
{
    int fl = CM_GPUDetector_GetFeatureLevel();
    if (fl >= 110)
        return CopyAnsi("Device/Context, Compute Shader 5.0, DirectCompute FFT, D3D11 hardware", buffer, bufferSize);
    if (fl > 0)
        return CopyAnsi("D3D11 device detected, Compute Shader 5.0 unavailable", buffer, bufferSize);
    return CopyAnsi("No D3D11 hardware device", buffer, bufferSize);
}

extern "C" __declspec(dllexport) int __cdecl CM_GPUDetector_Test(void)
{
    static const char* hlsl =
        "RWStructuredBuffer<float> g_out : register(u0);"
        "[numthreads(1,1,1)] void main(uint3 id:SV_DispatchThreadID){ g_out[0]=123.25f; }";

    ID3D11Device* device = 0;
    ID3D11DeviceContext* context = 0;
    D3D_FEATURE_LEVEL level = D3D_FEATURE_LEVEL_11_0;
    ID3DBlob* shaderBlob = 0;
    ID3DBlob* errors = 0;
    ID3D11ComputeShader* shader = 0;
    ID3D11Buffer* gpuBuffer = 0;
    ID3D11UnorderedAccessView* uav = 0;
    ID3D11Buffer* staging = 0;

    HRESULT hr = CreateHardwareDevice(&device, &context, &level);
    if (FAILED(hr) || level < D3D_FEATURE_LEVEL_11_0) goto fail;

    hr = D3DCompile(hlsl, strlen(hlsl), "SQ4KOU_GPU_TEST", 0, 0, "main", "cs_5_0",
        D3DCOMPILE_ENABLE_STRICTNESS | D3DCOMPILE_OPTIMIZATION_LEVEL3, 0, &shaderBlob, &errors);
    if (FAILED(hr)) goto fail;

    hr = device->CreateComputeShader(shaderBlob->GetBufferPointer(), shaderBlob->GetBufferSize(), 0, &shader);
    if (FAILED(hr)) goto fail;

    D3D11_BUFFER_DESC bd;
    ZeroMemory(&bd, sizeof(bd));
    bd.ByteWidth = sizeof(float);
    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.BindFlags = D3D11_BIND_UNORDERED_ACCESS;
    bd.MiscFlags = D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
    bd.StructureByteStride = sizeof(float);
    hr = device->CreateBuffer(&bd, 0, &gpuBuffer);
    if (FAILED(hr)) goto fail;

    D3D11_UNORDERED_ACCESS_VIEW_DESC ud;
    ZeroMemory(&ud, sizeof(ud));
    ud.Format = DXGI_FORMAT_UNKNOWN;
    ud.ViewDimension = D3D11_UAV_DIMENSION_BUFFER;
    ud.Buffer.NumElements = 1;
    hr = device->CreateUnorderedAccessView(gpuBuffer, &ud, &uav);
    if (FAILED(hr)) goto fail;

    D3D11_BUFFER_DESC sd;
    ZeroMemory(&sd, sizeof(sd));
    sd.ByteWidth = sizeof(float);
    sd.Usage = D3D11_USAGE_STAGING;
    sd.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    hr = device->CreateBuffer(&sd, 0, &staging);
    if (FAILED(hr)) goto fail;

    context->CSSetShader(shader, 0, 0);
    context->CSSetUnorderedAccessViews(0, 1, &uav, 0);
    context->Dispatch(1, 1, 1);
    {
        ID3D11UnorderedAccessView* nullUav = 0;
        context->CSSetUnorderedAccessViews(0, 1, &nullUav, 0);
    }
    context->CopyResource(staging, gpuBuffer);

    D3D11_MAPPED_SUBRESOURCE mapped;
    ZeroMemory(&mapped, sizeof(mapped));
    hr = context->Map(staging, 0, D3D11_MAP_READ, 0, &mapped);
    if (FAILED(hr)) goto fail;
    {
        float value = *(const float*)mapped.pData;
        context->Unmap(staging, 0);
        if (value < 123.24f || value > 123.26f) goto fail;
    }

    SafeRelease(staging);
    SafeRelease(uav);
    SafeRelease(gpuBuffer);
    SafeRelease(shader);
    SafeRelease(errors);
    SafeRelease(shaderBlob);
    SafeRelease(context);
    SafeRelease(device);
    return 1;

fail:
    SafeRelease(staging);
    SafeRelease(uav);
    SafeRelease(gpuBuffer);
    SafeRelease(shader);
    SafeRelease(errors);
    SafeRelease(shaderBlob);
    SafeRelease(context);
    SafeRelease(device);
    return 0;
}
