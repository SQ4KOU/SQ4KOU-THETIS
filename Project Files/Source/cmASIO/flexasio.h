#ifndef _flexasio_h
#define _flexasio_h

#ifdef __cplusplus
extern "C" {
#endif

    typedef void (__cdecl *FlexASIOCallback)(
        void* in0, void* in1, void* in2, void* in3,
        void* in4, void* in5, void* in6, void* in7,
        void* out0, void* out1, void* out2, void* out3,
        void* out4, void* out5, void* out6, void* out7,
        int frames);

    __declspec(dllexport) int prepareFlexASIO(int samplerate, const char* asioDriverName, FlexASIOCallback callback);
    __declspec(dllexport) long flexAsioStart();
    __declspec(dllexport) long flexAsioStop();
    __declspec(dllexport) void unloadFlexASIO();
    __declspec(dllexport) int getFlexASIOBlockSize();
    __declspec(dllexport) int getFlexASIOError();

#ifdef __cplusplus
}
#endif

#endif
