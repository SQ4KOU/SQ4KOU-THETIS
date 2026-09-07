#pragma once

#ifdef __cplusplus
extern "C" {
#endif

__declspec(dllexport) int __cdecl CM_GPUDetector_GetAdapterName(char* buffer, int bufferSize);
__declspec(dllexport) int __cdecl CM_GPUDetector_GetFeatureLevel(void);
__declspec(dllexport) int __cdecl CM_GPUDetector_Test(void);
__declspec(dllexport) int __cdecl CM_GPUDetector_GetCapabilities(char* buffer, int bufferSize);

#ifdef __cplusplus
}
#endif
