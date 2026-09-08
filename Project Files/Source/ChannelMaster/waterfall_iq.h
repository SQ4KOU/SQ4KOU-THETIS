#pragma once
#ifdef __cplusplus
extern "C" {
#endif
__declspec(dllexport) void __cdecl CM_WaterfallIQ_Init(int stream, int capacitySamples);
__declspec(dllexport) void __cdecl CM_WaterfallIQ_Free(int stream);
__declspec(dllexport) void __cdecl CM_WaterfallIQ_SetEnabled(int stream, int enable);
__declspec(dllexport) int  __cdecl CM_WaterfallIQ_Available(int stream);
__declspec(dllexport) int  __cdecl CM_WaterfallIQ_Get(int stream, float* outI, float* outQ, int maxSamples);
__declspec(dllexport) int  __cdecl CM_WaterfallIQ_DroppedSamples(int stream);
__declspec(dllexport) void __cdecl CM_WaterfallIQ_ResetDropped(int stream);
void CM_WaterfallIQ_Push(int stream, int nsamples, const double* data);
#ifdef __cplusplus
}
#endif
