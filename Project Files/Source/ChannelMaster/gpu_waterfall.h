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
#endif
