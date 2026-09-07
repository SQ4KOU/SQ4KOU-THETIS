#include <Windows.h>
#include <malloc.h>
#include <stdint.h>
#include <string.h>
#include "waterfall_iq.h"

#define WF_MAX_CHANNELS 8
#define WF_MIN_CAPACITY 1024

typedef struct _WF_IQ_RING
{
    float* data;                    /* interleaved I,Q */
    LONG capacity;                  /* complex samples; power of two */
    LONG mask;
    volatile LONG readIndex;
    volatile LONG writeIndex;
    volatile LONG enabled;
    volatile LONG64 dropped;
} WF_IQ_RING;

static WF_IQ_RING g_wfIq[WF_MAX_CHANNELS];

static int wf_valid_channel(int channel)
{
    return channel >= 0 && channel < WF_MAX_CHANNELS;
}

static LONG wf_round_pow2(LONG value)
{
    LONG n = WF_MIN_CAPACITY;
    if (value < WF_MIN_CAPACITY) value = WF_MIN_CAPACITY;
    while (n < value && n < (1 << 28)) n <<= 1;
    return n;
}

__declspec(dllexport) int __cdecl CM_WaterfallIQ_Init(int channel, int requestedSamples)
{
    WF_IQ_RING* r;
    LONG capacity;
    float* newData;

    if (!wf_valid_channel(channel)) return 0;

    r = &g_wfIq[channel];
    capacity = wf_round_pow2((LONG)requestedSamples);

    /* Init is called while capture is disabled. */
    InterlockedExchange(&r->enabled, 0);

    if (r->data != 0 && r->capacity == capacity)
    {
        InterlockedExchange(&r->readIndex, 0);
        InterlockedExchange(&r->writeIndex, 0);
        InterlockedExchange64(&r->dropped, 0);
        return capacity;
    }

    newData = (float*)_aligned_malloc((size_t)capacity * 2u * sizeof(float), 64);
    if (newData == 0) return 0;
    memset(newData, 0, (size_t)capacity * 2u * sizeof(float));

    if (r->data != 0)
    {
        _aligned_free(r->data);
    }

    r->data = newData;
    r->capacity = capacity;
    r->mask = capacity - 1;
    InterlockedExchange(&r->readIndex, 0);
    InterlockedExchange(&r->writeIndex, 0);
    InterlockedExchange64(&r->dropped, 0);
    return capacity;
}

__declspec(dllexport) void __cdecl CM_WaterfallIQ_SetEnabled(int channel, int enabled)
{
    WF_IQ_RING* r;
    if (!wf_valid_channel(channel)) return;
    r = &g_wfIq[channel];
    if (r->data == 0) return;

    if (enabled)
    {
        InterlockedExchange(&r->readIndex, 0);
        InterlockedExchange(&r->writeIndex, 0);
        InterlockedExchange(&r->enabled, 1);
    }
    else
    {
        InterlockedExchange(&r->enabled, 0);
    }
}

__declspec(dllexport) int __cdecl CM_WaterfallIQ_Available(int channel)
{
    WF_IQ_RING* r;
    LONG rd, wr;
    if (!wf_valid_channel(channel)) return 0;
    r = &g_wfIq[channel];
    if (r->data == 0 || r->capacity == 0) return 0;

    rd = InterlockedCompareExchange(&r->readIndex, 0, 0);
    wr = InterlockedCompareExchange(&r->writeIndex, 0, 0);
    return (int)((wr - rd) & r->mask);
}

__declspec(dllexport) int __cdecl CM_WaterfallIQ_Get(int channel, int requestedSamples, float* iOut, float* qOut)
{
    WF_IQ_RING* r;
    LONG rd, wr, available, count, i;
    if (!wf_valid_channel(channel) || requestedSamples <= 0 || iOut == 0 || qOut == 0) return 0;
    r = &g_wfIq[channel];
    if (r->data == 0 || r->capacity == 0) return 0;

    rd = InterlockedCompareExchange(&r->readIndex, 0, 0);
    wr = InterlockedCompareExchange(&r->writeIndex, 0, 0);
    available = (wr - rd) & r->mask;
    count = requestedSamples < available ? requestedSamples : available;

    for (i = 0; i < count; i++)
    {
        LONG idx = (rd + i) & r->mask;
        iOut[i] = r->data[(size_t)idx * 2u + 0u];
        qOut[i] = r->data[(size_t)idx * 2u + 1u];
    }

    if (count > 0)
        InterlockedExchange(&r->readIndex, (rd + count) & r->mask);
    return (int)count;
}

__declspec(dllexport) void __cdecl CM_WaterfallIQ_Free(int channel)
{
    WF_IQ_RING* r;
    if (!wf_valid_channel(channel)) return;
    r = &g_wfIq[channel];

    InterlockedExchange(&r->enabled, 0);
    MemoryBarrier();
    if (r->data != 0)
    {
        _aligned_free(r->data);
        r->data = 0;
    }
    r->capacity = 0;
    r->mask = 0;
    InterlockedExchange(&r->readIndex, 0);
    InterlockedExchange(&r->writeIndex, 0);
}

__declspec(dllexport) unsigned __int64 __cdecl CM_WaterfallIQ_DroppedSamples(int channel)
{
    if (!wf_valid_channel(channel)) return 0;
    return (unsigned __int64)InterlockedCompareExchange64(&g_wfIq[channel].dropped, 0, 0);
}

__declspec(dllexport) void __cdecl CM_WaterfallIQ_ResetDropped(int channel)
{
    if (!wf_valid_channel(channel)) return;
    InterlockedExchange64(&g_wfIq[channel].dropped, 0);
}

void CM_WaterfallIQ_Push(int channel, int nsamples, const double* data)
{
    WF_IQ_RING* r;
    LONG rd, wr, used, freeCount, count, i;

    if (!wf_valid_channel(channel) || nsamples <= 0 || data == 0) return;
    r = &g_wfIq[channel];
    if (InterlockedCompareExchange(&r->enabled, 0, 0) == 0 || r->data == 0 || r->capacity == 0) return;

    rd = InterlockedCompareExchange(&r->readIndex, 0, 0);
    wr = InterlockedCompareExchange(&r->writeIndex, 0, 0);
    used = (wr - rd) & r->mask;
    freeCount = (r->capacity - 1) - used;
    count = nsamples < freeCount ? nsamples : freeCount;

    for (i = 0; i < count; i++)
    {
        LONG idx = (wr + i) & r->mask;
        r->data[(size_t)idx * 2u + 0u] = (float)data[(size_t)i * 2u + 0u];
        r->data[(size_t)idx * 2u + 1u] = (float)data[(size_t)i * 2u + 1u];
    }

    if (count > 0)
        InterlockedExchange(&r->writeIndex, (wr + count) & r->mask);

    if (count < nsamples)
        InterlockedExchangeAdd64(&r->dropped, (LONG64)(nsamples - count));
}
