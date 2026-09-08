#!/usr/bin/env python3
from pathlib import Path
import re, subprocess, sys

ROOT = Path(__file__).resolve().parents[1]
REF = "origin/reference/eu2av-2.10.3.16-final-decompiled"
REF_BASE = "reverse-engineering/EU2AV-Thetis-2.10.3.16-Final/managed/Thetis.exe"
CON = ROOT / "Project Files/Source/Console"
CM = ROOT / "Project Files/Source/ChannelMaster"


def die(msg):
    raise SystemExit("GPU16_INTEGRATOR_FAIL: " + msg)

def git_show(path):
    p = subprocess.run(["git", "show", f"{REF}:{path}"], cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if p.returncode:
        die(f"git show failed for {path}: {p.stderr.decode(errors='replace')}")
    return p.stdout

def write_bytes(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)

def write_text(path, text):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8-sig")

def replace_once(text, old, new, label):
    n = text.count(old)
    if n != 1:
        die(f"{label}: expected 1 anchor, got {n}")
    return text.replace(old, new, 1)

def add_before(text, anchor, insertion, label):
    if insertion.strip() in text:
        return text
    return replace_once(text, anchor, insertion + anchor, label)

# ---------------------------------------------------------------------------
# 1. Exact recovered managed GPU classes from 2.10.3.16 Final decompilation.
# ---------------------------------------------------------------------------
managed = [
    "GPUWaterfallLogger.cs",
    "GPUWaterfallMagnitudeMode.cs",
    "GPUWaterfallPipeline.cs",
    "GPUWaterfallResamplingMode.cs",
    "GPUWaterfallWindowType.cs",
    "FFTParams.cs",
    "WaterfallGPURenderer.cs",
    "WaterfallRowParams.cs",
    "GPUDetector.cs",
    "WaterfallEffect.cs",
    "WaterfallEffectImpl.cs",
    "WaterfallEffectParams.cs",
]
for name in managed:
    data = git_show(f"{REF_BASE}/Thetis/{name}")
    write_bytes(CON / name, data)

# Make shader loading deterministic in a normal source build: embedded resource
# first, exact Final disk-name fallback second. This does not alter shader bytes.
loader = r'''using System;
using System.IO;
using System.Reflection;

namespace Thetis
{
    internal static class GPUWaterfallShaderLoader
    {
        internal static byte[] Load(string filename)
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                foreach (string resource in asm.GetManifestResourceNames())
                {
                    if (resource.EndsWith(filename, StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream s = asm.GetManifestResourceStream(resource))
                        {
                            if (s == null) continue;
                            byte[] bytes = new byte[s.Length];
                            int off = 0;
                            while (off < bytes.Length)
                            {
                                int n = s.Read(bytes, off, bytes.Length - off);
                                if (n <= 0) break;
                                off += n;
                            }
                            if (off == bytes.Length) return bytes;
                        }
                    }
                }
                string disk = Path.Combine(Path.GetDirectoryName(asm.Location), filename);
                return File.Exists(disk) ? File.ReadAllBytes(disk) : null;
            }
            catch { return null; }
        }
    }
}
'''
write_text(CON / "GPUWaterfallShaderLoader.cs", loader)

# Patch exact recovered classes only at their loader chokepoints.
p = (CON / "GPUWaterfallPipeline.cs").read_text(encoding="utf-8-sig")
pat = re.compile(r'private static byte\[\] LoadBytecode\(string filename\)\s*\{.*?\n\t\}', re.S)
m = pat.search(p)
if not m: die("GPUWaterfallPipeline.LoadBytecode not found")
p = p[:m.start()] + '''private static byte[] LoadBytecode(string filename)\n\t{\n\t\tbyte[] b = GPUWaterfallShaderLoader.Load(filename);\n\t\tif (b == null || b.Length == 0) LogGPU("Shader binary not found: " + filename);\n\t\treturn b;\n\t}''' + p[m.end():]
(CON / "GPUWaterfallPipeline.cs").write_text(p, encoding="utf-8-sig")

p = (CON / "WaterfallGPURenderer.cs").read_text(encoding="utf-8-sig")
pat = re.compile(r'private static byte\[\] LoadShaderBytecode\(\)\s*\{.*?\n\t\}', re.S)
m = pat.search(p)
if not m: die("WaterfallGPURenderer.LoadShaderBytecode not found")
p = p[:m.start()] + '''private static byte[] LoadShaderBytecode()\n\t{\n\t\tbyte[] b = GPUWaterfallShaderLoader.Load("waterfall_row_cs.bin");\n\t\tif (b == null || b.Length == 0) LogGPU("Shader binary not found: waterfall_row_cs.bin");\n\t\treturn b;\n\t}''' + p[m.end():]
(CON / "WaterfallGPURenderer.cs").write_text(p, encoding="utf-8-sig")

# Exact recovered DXBC assets. Reference physical names include Thetis. prefix.
shaders = [
    "waterfall_fft_bitreverse_cs.bin", "waterfall_fft_magnitude_cs.bin",
    "waterfall_fft_stage_ab_cs.bin", "waterfall_fft_stage_ba_cs.bin",
    "waterfall_postproc.bin", "waterfall_resolve_cs.bin", "waterfall_row_cs.bin"
]
for name in shaders:
    write_bytes(CON / name, git_show(f"{REF_BASE}/Thetis.{name}"))

# ---------------------------------------------------------------------------
# 2. Native ChannelMaster role recovered from Final: raw IQ ring only.
#    API/signature is kept byte-for-byte compatible at the ABI level with the
#    decompiled cmaster.cs declarations: Get(stream, outI, outQ, maxSamples).
# ---------------------------------------------------------------------------
waterfall_h = r'''#pragma once
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
'''
write_text(CM / "waterfall_iq.h", waterfall_h)

waterfall_c = r'''#include <Windows.h>
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
'''
write_text(CM / "waterfall_iq.c", waterfall_c)

# Hook raw receiver IQ BEFORE xpipe, as recovered native ABI is a tap, not DSP.
cmaster = (CM / "cmaster.c").read_text(encoding="utf-8-sig")
if '#include "waterfall_iq.h"' not in cmaster:
    cmaster = replace_once(cmaster, '#include "cmcomm.h"', '#include "cmcomm.h"\n#include "waterfall_iq.h"', "cmaster include")
anchor = 'case 0:  // standard receiver\n\t\trx = rxid (stream);'
if 'CM_WaterfallIQ_Push(stream' not in cmaster:
    cmaster = replace_once(cmaster, anchor, anchor + '\n\t\tCM_WaterfallIQ_Push(stream, pcm->xcm_insize[stream], pcm->in[stream]);', "raw IQ hook")
(CM / "cmaster.c").write_text(cmaster, encoding="utf-8-sig")

# ---------------------------------------------------------------------------
# 3. Exact C# P/Invoke facade recovered from Final.
# ---------------------------------------------------------------------------
cmcs = CON / "cmaster.cs"
t = cmcs.read_text(encoding="utf-8-sig")
interop = r'''
        // Recovered 2.10.3.16 Final GPU waterfall raw-IQ bridge.
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CM_WaterfallIQ_Init(int stream, int capacitySamples);
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CM_WaterfallIQ_Free(int stream);
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CM_WaterfallIQ_SetEnabled(int stream, int enable);
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CM_WaterfallIQ_Available(int stream);
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int CM_WaterfallIQ_Get(int stream, float* outI, float* outQ, int maxSamples);
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CM_WaterfallIQ_DroppedSamples(int stream);
        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CM_WaterfallIQ_ResetDropped(int stream);

        public static void InitWaterfallIQ(int capacitySamples)
        {
            for (int i = 0; i < cmRCVR; i++) CM_WaterfallIQ_Init(inid(0, i), capacitySamples);
        }
        public static void FreeWaterfallIQ()
        {
            for (int i = 0; i < cmRCVR; i++) CM_WaterfallIQ_Free(inid(0, i));
        }
        public static void SetWaterfallIQEnabled(bool enabled)
        {
            for (int i = 0; i < cmRCVR; i++) CM_WaterfallIQ_SetEnabled(inid(0, i), enabled ? 1 : 0);
        }
        public static int GetWaterfallIQDroppedSamples(int stream) { return CM_WaterfallIQ_DroppedSamples(stream); }
        public static void ResetWaterfallIQDropped(int stream) { CM_WaterfallIQ_ResetDropped(stream); }
        public unsafe static int ReadWaterfallIQ(int stream, float[] outI, float[] outQ, int maxSamples)
        {
            if (outI == null || outQ == null || outI.Length < maxSamples || outQ.Length < maxSamples) return 0;
            int n;
            fixed (float* pI = outI) fixed (float* pQ = outQ) n = CM_WaterfallIQ_Get(stream, pI, pQ, maxSamples);
            // Exact recovered managed convention: swap I/Q after the native read.
            for (int i = 0; i < n; i++) { float x = outI[i]; outI[i] = outQ[i]; outQ[i] = x; }
            return n;
        }

'''
if 'CM_WaterfallIQ_Init' not in t:
    marker = '        [DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]\n        public static extern void SendpOutboundTCIRxIQ'
    t = add_before(t, marker, interop, "cmaster.cs interop")
cmcs.write_text(t, encoding="utf-8-sig")

# ---------------------------------------------------------------------------
# 4. Project wiring. Exact recovered binaries are embedded; also copied to the
#    output directory for the original Final disk-loader behavior.
# ---------------------------------------------------------------------------
proj = CON / "Thetis.csproj"
t = proj.read_text(encoding="utf-8-sig")
if '<LangVersion>latest</LangVersion>' not in t:
    t = replace_once(t, '<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>', '<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>\n    <LangVersion>latest</LangVersion>', "LangVersion")

compile_items = managed + ["GPUWaterfallShaderLoader.cs"]
# Add compile items before first existing Console compile item.
first_compile = re.search(r'\s*<Compile Include="', t)
if not first_compile: die("Thetis.csproj Compile anchor missing")
pos = first_compile.start()
add = ''.join(f'    <Compile Include="{n}" />\n' for n in compile_items if f'<Compile Include="{n}"' not in t)
t = t[:pos] + '\n' + add + t[pos:]
# Embedded resources in their own ItemGroup before project end.
if 'LogicalName>Thetis.waterfall_row_cs.bin' not in t:
    res = '  <ItemGroup>\n' + ''.join(
        f'    <EmbeddedResource Include="{n}">\n      <LogicalName>Thetis.{n}</LogicalName>\n      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\n    </EmbeddedResource>\n' for n in shaders
    ) + '  </ItemGroup>\n'
    t = t.replace('</Project>', res + '</Project>')
proj.write_text(t, encoding="utf-8-sig")

vcx = CM / "ChannelMaster.vcxproj"
t = vcx.read_text(encoding="utf-8-sig")
if 'waterfall_iq.c' not in t:
    m = re.search(r'(\s*<ClCompile Include="cmaster\.c"\s*/>)', t)
    if not m: die("ChannelMaster.vcxproj cmaster.c anchor missing")
    t = t[:m.end()] + '\n    <ClCompile Include="waterfall_iq.c" />' + t[m.end():]
if 'waterfall_iq.h' not in t:
    # Any ClInclude is a safe ItemGroup anchor; add after first.
    m = re.search(r'(\s*<ClInclude Include="[^"]+"\s*/>)', t)
    if not m: die("ChannelMaster.vcxproj header anchor missing")
    t = t[:m.end()] + '\n    <ClInclude Include="waterfall_iq.h" />' + t[m.end():]
vcx.write_text(t, encoding="utf-8-sig")

# ---------------------------------------------------------------------------
# 5. Static invariants — fail before MSBuild if architecture regresses.
# ---------------------------------------------------------------------------
checks = {
    "no native FFT module": not (CM / "gpu_waterfall.cpp").exists(),
    "correct IQ Get ABI": "CM_WaterfallIQ_Get(int stream, float* outI, float* outQ, int maxSamples)" in waterfall_h,
    "raw IQ pre-xpipe hook": cmaster.index("CM_WaterfallIQ_Push(stream") < cmaster.index("xpipe (stream, 0, pcm->in)", cmaster.index("case 0:  // standard receiver")),
    "GPU FFT managed": (CON / "GPUWaterfallPipeline.cs").exists(),
    "GPU row managed": (CON / "WaterfallGPURenderer.cs").exists(),
    "shader count": sum((CON / n).exists() for n in shaders) == 7,
}
for k,v in checks.items():
    print(("PASS " if v else "FAIL ") + k)
if not all(checks.values()): die("static invariant failure")
print("GPU16_STAGE1_INTEGRATION=PASS")
