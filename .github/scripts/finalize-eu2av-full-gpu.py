from pathlib import Path
import re

ROOT = Path('Project Files/Source/Console')
DISPLAY = ROOT / 'display.cs'
GPU = ROOT / 'Display.GPUWaterfall.cs'
SETUP = ROOT / 'Setup.GPUWaterfall.cs'
PROJ = ROOT / 'Thetis.csproj'


def read(p):
    return p.read_text(encoding='utf-8-sig')


def write(p, s):
    p.write_text(s, encoding='utf-8')


def replace_once(s, old, new, label):
    n = s.count(old)
    if n != 1:
        raise RuntimeError(f'{label}: expected exactly one match, got {n}')
    return s.replace(old, new, 1)


def replace_method(s, signature, new_method):
    i = s.find(signature)
    if i < 0:
        raise RuntimeError(f'method signature not found: {signature}')
    b = s.find('{', i)
    if b < 0:
        raise RuntimeError(f'opening brace not found: {signature}')
    depth = 0
    j = b
    in_str = False
    in_chr = False
    esc = False
    line_comment = False
    block_comment = False
    while j < len(s):
        c = s[j]
        n = s[j + 1] if j + 1 < len(s) else ''
        if line_comment:
            if c == '\n': line_comment = False
        elif block_comment:
            if c == '*' and n == '/':
                block_comment = False
                j += 1
        elif in_str:
            if esc: esc = False
            elif c == '\\': esc = True
            elif c == '"': in_str = False
        elif in_chr:
            if esc: esc = False
            elif c == '\\': esc = True
            elif c == "'": in_chr = False
        else:
            if c == '/' and n == '/': line_comment = True; j += 1
            elif c == '/' and n == '*': block_comment = True; j += 1
            elif c == '"': in_str = True
            elif c == "'": in_chr = True
            elif c == '{': depth += 1
            elif c == '}':
                depth -= 1
                if depth == 0:
                    return s[:i] + new_method + s[j + 1:]
        j += 1
    raise RuntimeError(f'closing brace not found: {signature}')


# ---------------------------------------------------------------------------
# display.cs: wire the actual D2D detector, effect draw path, complete
# Waterfall-Pro post processing, threshold controls and NF percentile mode.
# ---------------------------------------------------------------------------
display = read(DISPLAY)

if 'DetectGPUCapabilitiesFromD2D();' not in display:
    pat = re.compile(r'(?m)^(\s*)createD2DRenderTarget\(\);\s*$')
    display, n = pat.subn(lambda m: m.group(0) + '\n' + m.group(1) + 'DetectGPUCapabilitiesFromD2D();', display, count=1)
    if n != 1:
        raise RuntimeError(f'D2D detector insertion: expected 1 createD2DRenderTarget, got {n}')

rx1_draw = '_d2dRenderTarget.DrawBitmap(_waterfall_bmp_dx2d, new RectangleF(0, nVerticalShift + 20, _waterfall_bmp_dx2d.Size.Width, _waterfall_bmp_dx2d.Size.Height), m_fRX1WaterfallOpacity, BitmapInterpolationMode.Linear);'
rx2_draw = '_d2dRenderTarget.DrawBitmap(_waterfall_bmp2_dx2d, new RectangleF(0, nVerticalShift + 20, _waterfall_bmp2_dx2d.Size.Width, _waterfall_bmp2_dx2d.Size.Height), m_fRX2WaterfallOpacity, BitmapInterpolationMode.Linear);'
if rx1_draw in display:
    display = replace_once(display, rx1_draw, 'DrawWaterfallToTarget(_waterfall_bmp_dx2d, nVerticalShift, 20, m_fRX1WaterfallOpacity);', 'RX1 GPU effect draw')
if rx2_draw in display:
    display = replace_once(display, rx2_draw, 'DrawWaterfallToTarget(_waterfall_bmp2_dx2d, nVerticalShift, 20, m_fRX2WaterfallOpacity);', 'RX2 GPU effect draw')

threshold_marker = '                    GPUWaterfallPipeline managedGpuPipeline = null;'
threshold_call = '                    ApplyWaterfallProThresholds(rx, local_mox, ref low_threshold, ref high_threshold);\n\n'
if threshold_call.strip() not in display:
    display = replace_once(display, threshold_marker, threshold_call + threshold_marker, 'Waterfall Pro thresholds')

# Recovered configurable waterfall AGC smoothing instead of the fixed 0.4 factor.
agc_replacements = {
    '_RX1waterfallPreviousMinValue = (_RX1waterfallPreviousMinValue * 0.6f) + (noiseFloorCompensationTarget * 0.4f);': '_RX1waterfallPreviousMinValue = BlendWaterfallProAGC(_RX1waterfallPreviousMinValue, noiseFloorCompensationTarget);',
    '_RX1waterfallPreviousMinValue = (_RX1waterfallPreviousMinValue * 0.6f) + (waterfall_minimum * 0.4f);': '_RX1waterfallPreviousMinValue = BlendWaterfallProAGC(_RX1waterfallPreviousMinValue, waterfall_minimum);',
    '_RX2waterfallPreviousMinValue = (_RX2waterfallPreviousMinValue * 0.6f) + (noiseFloorCompensationTarget * 0.4f);': '_RX2waterfallPreviousMinValue = BlendWaterfallProAGC(_RX2waterfallPreviousMinValue, noiseFloorCompensationTarget);',
    '_RX2waterfallPreviousMinValue = (_RX2waterfallPreviousMinValue * 0.6f) + (waterfall_minimum * 0.4f);': '_RX2waterfallPreviousMinValue = BlendWaterfallProAGC(_RX2waterfallPreviousMinValue, waterfall_minimum);',
}
for old, new in agc_replacements.items():
    if old in display:
        display = replace_once(display, old, new, 'AGC smoothing')

old_wf_nf = 'processNoiseFloor(rx, averageCount, averageSum, nDecimatedWidth, true);'
new_wf_nf = 'processNoiseFloor(rx, averageCount, averageSum, nDecimatedWidth, true, waterfall_data, nDecimatedWidth);'
if old_wf_nf in display:
    display = replace_once(display, old_wf_nf, new_wf_nf, 'waterfall NF percentile input')

new_process_nf = r'''private static void processNoiseFloor(int rx, int averageCount, float averageSum, int width, bool waterfall, float[] fullData = null, int dataCount = 0)
        {
            if (rx != 1 && rx != 2) return;
            ref bool bAlreadyCalculated = ref (rx == 1 ? ref _bNoiseFloorAlreadyCalculatedRX1 : ref _bNoiseFloorAlreadyCalculatedRX2);
            if (bAlreadyCalculated) return;

            int fps = waterfall ? m_nFps / Math.Max(1, rx == 2 ? rx2_waterfall_update_period : waterfall_update_period) : m_nFps;
            if (fps < 1) fps = 1;
            int minSamples = (int)((float)width * ((float)_NFsensitivity / 20f));
            ref bool fastAttack = ref (rx == 1 ? ref m_bFastAttackNoiseFloorRX1 : ref m_bFastAttackNoiseFloorRX2);
            ref float fftBinAverage = ref (rx == 1 ? ref m_fFFTBinAverageRX1 : ref m_fFFTBinAverageRX2);
            ref float lerpAverage = ref (rx == 1 ? ref m_fLerpAverageRX1 : ref m_fLerpAverageRX2);
            ref float attackMs = ref (rx == 1 ? ref m_fAttackTimeInMSForRX1 : ref m_fAttackTimeInMSForRX2);
            ref double lastFastAttack = ref (rx == 1 ? ref _fLastFastAttackEnabledTimeRX1 : ref _fLastFastAttackEnabledTimeRX2);
            ref float fftFill = ref (rx == 1 ? ref _fft_fill_timeRX1 : ref _fft_fill_timeRX2);

            if (_nfMode == NoiseFloorPro.DetectionMode.Percentile && fullData != null && dataCount > 0)
            {
                float[] scratch = _nfScratch;
                if (scratch == null || scratch.Length < dataCount) scratch = (_nfScratch = new float[dataCount]);
                Array.Copy(fullData, 0, scratch, 0, dataCount);
                NoiseFloorPro.ComputeLowHigh(scratch, dataCount, _nfLowPct, _nfHighPct, out var lowDbm, out var highDbm);
                float oldLinear = fastPow10Raw(fftBinAverage);
                float mixed = (fastPow10Raw(lowDbm) + oldLinear) * 0.5f;
                fftBinAverage = 10f * (float)Math.Log10((double)mixed + 1E-60);
                if (rx == 1) _autoHighRX1 = _autoHighRX1 * 0.85f + highDbm * 0.15f;
                else _autoHighRX2 = _autoHighRX2 * 0.85f + highDbm * 0.15f;
            }
            else if (averageCount >= minSamples)
            {
                float average = averageSum / (float)averageCount;
                float oldLinear = fastPow10Raw(fftBinAverage);
                float mixed = (average + oldLinear) * 0.5f;
                fftBinAverage = 10f * (float)Math.Log10((double)mixed + 1E-60);
                float highEstimate = fftBinAverage + 15f;
                if (rx == 1) _autoHighRX1 = _autoHighRX1 * 0.85f + highEstimate * 0.15f;
                else _autoHighRX2 = _autoHighRX2 * 0.85f + highEstimate * 0.15f;
            }
            else
            {
                fftBinAverage += fastAttack ? 3f : 1f;
            }

            fftBinAverage = fftBinAverage < -200f ? -200f : (fftBinAverage > 200f ? 200f : fftBinAverage);
            int lerpCount = !fastAttack ? (int)((double)((float)fps / 1000f) * (double)attackMs) : 0;
            lerpCount++;
            float delta = lerpAverage - fftBinAverage;
            lerpAverage -= delta / (float)lerpCount;
            if (fastAttack)
            {
                float minimumFastAttack = Math.Max(1000f, fftFill + (_wdsp_mox_transition_buffer_clear ? fftFill : 0f));
                if (_high_perf_timer.ElapsedMsec - lastFastAttack > (double)minimumFastAttack) fastAttack = false;
            }
            bAlreadyCalculated = true;
        }'''

# IMPORTANT: match the active method including its line indentation. There is an
# older commented-out processNoiseFloor above it; matching the bare signature
# would modify the commented reference block and can consume unrelated display code.
active_nf_signature = '\n        private static void processNoiseFloor(int rx, int averageCount, float averageSum, int width, bool waterfall)'
display = replace_method(display, active_nf_signature, '\n        ' + new_process_nf)

# Replace the old always-CPU colour post-processing with the recovered policy:
# temporal first, then only the operations not handled by the active D2D effect.
start_marker = '                    // Yurij-eu2av - 2026-07-04: unified float post-processing (all schemes).'
end_marker = '                    // Yurij-eu2av - 2026-07-04: quantise the float row to the active'
si = display.find(start_marker)
ei = display.find(end_marker, si + 1) if si >= 0 else -1
if si >= 0 and ei > si:
    replacement = '                    // EU2AV 2.10.3.16 Waterfall-Pro processing: temporal + CPU/GPU split.\n                    ApplyWaterfallProPostProcessing(rowF, W, rx);\n\n'
    display = display[:si] + replacement + display[ei:]
elif 'ApplyWaterfallProPostProcessing(rowF, W, rx);' not in display:
    raise RuntimeError('Waterfall post-processing block not found')

write(DISPLAY, display)

# ---------------------------------------------------------------------------
# Display.GPUWaterfall.cs: Zoom Adaptive must feed the actual managed renderer.
# ---------------------------------------------------------------------------
gpu = read(GPU)
if 'GetEffectiveManagedGPUParams(rx, out int effectiveToneMap' not in gpu:
    marker = '                float invGamma = gamma != 0f ? 1f / gamma : 1f;\n'
    insert = marker + '                GetEffectiveManagedGPUParams(rx, out int effectiveToneMap, out float effectiveTemporalAlpha);\n'
    gpu = replace_once(gpu, marker, insert, 'managed GPU effective params')
    gpu = replace_once(gpu, '                    gamma, invGamma, (int)WaterfallEnhancer.ToneMap,', '                    gamma, invGamma, effectiveToneMap,', 'managed GPU tone-map')
    gpu = replace_once(gpu, '                    _temporalEnabled ? _temporalAlpha : 0f, 0.05f, true, scheme == ColorScheme.Custom,', '                    effectiveTemporalAlpha, 0.05f, true, scheme == ColorScheme.Custom,', 'managed GPU temporal')
write(GPU, gpu)

# ---------------------------------------------------------------------------
# Setup: force a real D2D capability refresh before applying Auto/Level modes.
# ---------------------------------------------------------------------------
setup = read(SETUP)
apply_sig = '    private void ApplyGPUSelection(int selectedIndex)\n    {\n'
if apply_sig not in setup:
    apply_sig = '\tprivate void ApplyGPUSelection(int selectedIndex)\n\t{\n'
if 'DetectGPUCapabilitiesFromD2D();' not in setup:
    if apply_sig not in setup:
        raise RuntimeError('ApplyGPUSelection signature not found')
    indent = '\t\t' if apply_sig.startswith('\t') else '        '
    setup = setup.replace(apply_sig, apply_sig + indent + 'Display.DetectGPUCapabilitiesFromD2D();\n', 1)
write(SETUP, setup)

# ---------------------------------------------------------------------------
# Project: compile all recovered GPU/post-processing pieces.
# ---------------------------------------------------------------------------
proj = read(PROJ)
compile_items = [
    'Display.GPUPostProcessing.cs',
    'WaterfallEffect.cs',
    'WaterfallEffectImpl.cs',
    'WaterfallEffectParams.cs',
    'ZoomAdaptive.cs',
]
marker = '    <Compile Include="NoiseFloorPro.cs" />'
if marker not in proj:
    raise RuntimeError('csproj compile marker not found')
for f in compile_items:
    item = f'    <Compile Include="{f}" />'
    if item not in proj:
        proj = proj.replace(marker, item + '\n' + marker, 1)
write(PROJ, proj)

print('FINALIZE_EU2AV_FULL_GPU=PASS')
