#include <Windows.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include "waterfall_iq.h"

#define WF_MAX_STREAMS 32

typedef struct WF_IQ_RING {
    float *data;                 /* interleaved I,Q */
    int capacity;                /* complex samples */
    volatile LONG readIndex;
    volatile LONG writeIndex;
    volatile LONG enabled;
    volatile LONG dropped;
    CRITICAL_SECTION lock;
    volatile LONG lockReady;
} WF_IQ_RING;

static WF_IQ_RING g_wf[WF_MAX_STREAMS];

static int valid_stream(int s) { return s >= 0 && s < WF_MAX_STREAMS; }
static void ensure_lock(WF_IQ_RING* r)
{
    if (InterlockedCompareExchange(&r->lockReady, 1, 0) == 0)
        InitializeCriticalSectionAndSpinCount(&r->lock, 2500);
    else
        while (InterlockedCompareExchange(&r->lockReady, 0, 0) != 1) Sleep(0);
}

__declspec(dllexport) void __cdecl CM_WaterfallIQ_Init(int stream, int capacitySamples)
{
    WF_IQ_RING* r;
    float* p;
    if (!valid_stream(stream) || capacitySamples <= 1) return;
    r = &g_wf[stream]; ensure_lock(r);
    EnterCriticalSection(&r->lock);
    InterlockedExchange(&r->enabled, 0);
    if (r->capacity != capacitySamples || r->data == NULL) {
        p = (float*)malloc((size_t)capacitySamples * 2u * sizeof(float));
        if (p != NULL) {
            memset(p, 0, (size_t)capacitySamples * 2u * sizeof(float));
            if (r->data) free(r->data);
            r->data = p; r->capacity = capacitySamples;
        }
    }
    r->readIndex = r->writeIndex = 0; r->dropped = 0;
    LeaveCriticalSection(&r->lock);
}

__declspec(dllexport) void __cdecl CM_WaterfallIQ_Free(int stream)
{
    WF_IQ_RING* r;
    if (!valid_stream(stream)) return;
    r=&g_wf[stream]; ensure_lock(r); EnterCriticalSection(&r->lock);
    r->enabled=0; r->readIndex=r->writeIndex=0;
    if (r->data) { free(r->data); r->data=NULL; }
    r->capacity=0; LeaveCriticalSection(&r->lock);
}

__declspec(dllexport) void __cdecl CM_WaterfallIQ_SetEnabled(int stream, int enable)
{
    WF_IQ_RING* r;
    if (!valid_stream(stream)) return;
    r=&g_wf[stream]; ensure_lock(r); EnterCriticalSection(&r->lock);
    if (enable) r->readIndex=r->writeIndex=0;
    r->enabled = enable ? 1 : 0;
    LeaveCriticalSection(&r->lock);
}

__declspec(dllexport) int __cdecl CM_WaterfallIQ_Available(int stream)
{
    WF_IQ_RING* r; int rd,wr,n;
    if (!valid_stream(stream)) return 0;
    r=&g_wf[stream]; if (!r->data || r->capacity<=1) return 0;
    ensure_lock(r); EnterCriticalSection(&r->lock);
    rd=r->readIndex; wr=r->writeIndex;
    n = (wr >= rd) ? (wr-rd) : (r->capacity-rd+wr);
    LeaveCriticalSection(&r->lock); return n;
}

__declspec(dllexport) int __cdecl CM_WaterfallIQ_Get(int stream, float* outI, float* outQ, int maxSamples)
{
    WF_IQ_RING* r; int rd,wr,avail,n,k,idx;
    if (!valid_stream(stream) || !outI || !outQ || maxSamples<=0) return 0;
    r=&g_wf[stream]; if (!r->data || r->capacity<=1) return 0;
    ensure_lock(r); EnterCriticalSection(&r->lock);
    rd=r->readIndex; wr=r->writeIndex;
    avail=(wr>=rd)?(wr-rd):(r->capacity-rd+wr); n=maxSamples<avail?maxSamples:avail;
    for(k=0;k<n;k++){ idx=rd+k; if(idx>=r->capacity) idx-=r->capacity; outI[k]=r->data[idx*2]; outQ[k]=r->data[idx*2+1]; }
    rd += n; while(rd>=r->capacity) rd-=r->capacity; r->readIndex=rd;
    LeaveCriticalSection(&r->lock); return n;
}

__declspec(dllexport) int __cdecl CM_WaterfallIQ_DroppedSamples(int stream)
{ return valid_stream(stream) ? (int)InterlockedCompareExchange(&g_wf[stream].dropped,0,0) : 0; }
__declspec(dllexport) void __cdecl CM_WaterfallIQ_ResetDropped(int stream)
{ if(valid_stream(stream)) InterlockedExchange(&g_wf[stream].dropped,0); }

void CM_WaterfallIQ_Push(int stream, int nsamples, const double* data)
{
    WF_IQ_RING* r; int rd,wr,used,freeN,n,k,idx;
    if (!valid_stream(stream) || nsamples<=0 || !data) return;
    r=&g_wf[stream]; if (!r->data || r->capacity<=1 || !r->enabled) return;
    ensure_lock(r); EnterCriticalSection(&r->lock);
    if (!r->enabled) { LeaveCriticalSection(&r->lock); return; }
    rd=r->readIndex; wr=r->writeIndex;
    used=(wr>=rd)?(wr-rd):(r->capacity-rd+wr); freeN=(r->capacity-1)-used; n=nsamples<freeN?nsamples:freeN;
    for(k=0;k<n;k++){ idx=wr+k; while(idx>=r->capacity) idx-=r->capacity; r->data[idx*2]=(float)data[k*2]; r->data[idx*2+1]=(float)data[k*2+1]; }
    wr += n; while(wr>=r->capacity) wr-=r->capacity; r->writeIndex=wr;
    if(n<nsamples) InterlockedExchangeAdd(&r->dropped,nsamples-n);
    LeaveCriticalSection(&r->lock);
}
