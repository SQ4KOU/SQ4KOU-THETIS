"""Read-only gate: native Final palettes must never be replaced by legacy aliases."""
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
C = ROOT / 'Project Files/Source/Console'
REF = ROOT / '.github/recovered/eu2av-2.10.3.16'
BASE = 'a423339defae21e3ba149ee0bda8bc6f3d62cafe'

def read(path):
    return path.read_text(encoding='utf-8-sig')

def compact(text):
    return re.sub(r'\s+', '', text)

def method(text, name):
    match = re.search(r'(?m)^[ \t]*(?:private|internal|public) (?:static )?[^\n=]+\b' + name + r'\([^\n]*\)\s*\{', text)
    assert match, name
    start = text.index('{', match.start())
    depth, end = 1, start + 1
    while depth:
        depth += (text[end] == '{') - (text[end] == '}')
        end += 1
    return text[match.start():end]

def baseline(path):
    return subprocess.check_output(['git', 'show', f'{BASE}:{path}'], cwd=ROOT).decode('utf-8-sig')

palette = read(C / 'WaterfallPalette.cs')
reference = read(REF / 'WaterfallPalette.EU2AV.2.10.3.16.cs.txt')
assert compact(palette) == compact(reference.replace('namespace Thetis;', 'namespace Thetis {') + '}')
setup = read(C / 'setup.cs')
gpu = read(C / 'Display.GPUWaterfall.cs')
display = read(C / 'display.cs')
designer = read(C / 'setup.designer.cs')
for combo, rx in [('comboColorPalette', 'RX1'), ('comboRX2ColorPalette', 'RX2'), ('comboColorPalette_tx', 'TX')]:
    for label, enum in [('Enhanced 256', 'Enhanced256'), ('BlackWhite 256', 'Grayscale256')]:
        assert re.search(re.escape(f'else if ({combo}.Text == "{label}")') + r'\s*\{\s*' + re.escape(f'console.{rx}ColourScheme = ColorScheme.{enum};'), setup)
        # All 12 items exist before getOptions, including when GPU detection is pending.
        items = re.search(r'this\.' + combo + r'\.Items.AddRange\(new object\[\] \{(.*?)\}\);', designer, re.S).group(1)
        assert label in items
        assert len(re.findall(r'"[^"]+"', items)) == 12
for name in ['Enhanced256', 'Grayscale256']:
    assert f'case (ColorScheme.{name}):' in display
    assert f'if (scheme == ColorScheme.{name}) return GetPalette{name}();' in gpu
    assert f'scheme == ColorScheme.{name}' in method(gpu, 'IsGPUWaterfallPaletteScheme')
assert 'UploadLegacyPaletteToGPU' not in gpu
assert 'SQ4KOU_GPU_PALETTE_256_ALIAS' not in setup
assert 'ColorScheme.enhanced' not in gpu and 'ColorScheme.BLACKWHITE' not in gpu
assert '_gpuPaletteUpload[n] = r / 255f;' in gpu
# Appended enum values preserve every existing persisted numeric identity.
enums = read(C / 'enums.cs')
items = re.search(r'enum ColorScheme\s*\{([^}]+)', enums).group(1)
items = re.sub(r'//[^\n]*', '', items)
assert [x.strip().split('=')[0].strip() for x in items.split(',') if x.strip()] == ['original','enhanced','SPECTRAN','BLACKWHITE','LinLog','LinRad','LinAuto','off','Custom','Console','Thermal','DeepBlue','Enhanced256','Grayscale256']
# Exact Final General layout; Waterfall Pro differs only by the protected startup timer.
ui = read(C / 'Setup.GPUWaterfall.cs')
ref_setup = read(REF / 'Setup.EU2AV.2.10.3.16.cs.txt')
for name in ['InitGeneralTabWaterfallControls', 'InitWaterfallTab', 'UpdateOnePaletteCombo', 'UpdateWaterfallPaletteItems']:
    assert compact(method(ui, name)) == compact(method(ref_setup, name)), name
original_ui = baseline('Project Files/Source/Console/Setup.GPUWaterfall.cs')
for name in ['InitNoiseFloorProControls', 'ApplyGPUSelection', 'SyncGPUWaterfallPipelineEnabled', 'UpdateWaterfallRenderQualityItems']:
    assert method(ui, name) == method(original_ui, name), f'Protected startup changed: {name}'
original_setup = baseline('Project Files/Source/Console/setup.cs')
assert compact(method(setup, 'comboColorDepth_SelectedIndexChanged')) == compact(method(ref_setup, 'comboColorDepth_SelectedIndexChanged'))
assert compact(method(setup, 'DoColorDepthRebuild')) == compact(method(ref_setup, 'DoColorDepthRebuild'))
original_display = baseline('Project Files/Source/Console/display.cs')
# Every Display change is confined to the two palette fields/getters and switch cases.
# Materialize the exact previously tested 93783094 runtime patch; do not drop it
# merely because its changes were previously generated only on the Windows runner.
protected = ROOT / '.github/recovered/sq4kou-noghost-93783094'
ps = read(protected / 'fix-gpu-shader-embedded-resources.ps1')
replacements = re.findall(r"\$(?:replacement|rebuildReplacement) = @'\n(.*?)\n'@", ps, re.S)
assert len(replacements) == 3
assert compact(method(display, 'RebuildForColorDepth')) == compact(replacements[2])
resize = method(original_display, 'resizeDX2D').replace('resizeDX2D(out string error)', 'resizeDX2DForFormat(Format newFormat, out string error)').replace('_swapChain.Description.ModeDescription.Format', 'newFormat')
assert compact(method(display, 'resizeDX2DForFormat')) == compact(resize)
new_display = display.replace(method(display, 'resizeDX2DForFormat'), '')
new_display = new_display.replace(method(new_display, 'RebuildForColorDepth'), method(original_display, 'RebuildForColorDepth'))
for name in ['Enhanced256', 'Grayscale256']:
    new_display = new_display.replace(method(new_display, 'GetPalette' + name), '')
    new_display = new_display.replace(f'private static WaterfallPalette _palette{name};', '')
    new_display = re.sub(r'case \(ColorScheme\.' + name + r'\):.*?break;', '', new_display, flags=re.S)
assert compact(new_display) == compact(original_display), 'Non-palette display/ghost/depth change'
# Renderer, shaders, format conversion and FFT implementation remain byte-identical.
for name in ['GPUDetector.cs','Display.EU2AVWaterfallCompat.cs','WaterfallEnhancer.cs','WaterfallPixelWriter.cs','SharedWaterfallState.cs']:
    path = f'Project Files/Source/Console/{name}'
    assert (C / name).read_bytes() == subprocess.check_output(['git','show',f'{BASE}:{path}'],cwd=ROOT), name
for filename, name, expected in [('GPUWaterfallPipeline.cs', 'LoadBytecode', replacements[0]), ('WaterfallGPURenderer.cs', 'LoadShaderBytecode', replacements[1])]:
    text = read(C / filename)
    before = baseline('Project Files/Source/Console/' + filename)
    assert compact(method(text, name)) == compact(expected)
    text = text.replace(method(text, name), method(before, name))
    assert compact(text) == compact(before), filename
assert 'if (clearExisting || (inserted && !_gpuRendererHasData[index])) renderer.Clear();' in gpu
assert '0f, 0.05f, true, scheme == ColorScheme.Custom,' in gpu
assert 'effectiveTemporalAlpha, 0.05f' not in gpu
project = read(C / 'Thetis.csproj')
for shader in C.glob('waterfall_*.bin'):
    assert f'<EmbeddedResource Include="{shader.name}"><LogicalName>Thetis.{shader.name}</LogicalName></EmbeddedResource>' in project
for path in C.glob('waterfall_*.bin'):
    assert path.read_bytes() == subprocess.check_output(['git','show',f'{BASE}:Project Files/Source/Console/{path.name}'],cwd=ROOT), path.name
# Compare static Display tab inventory and concrete UI properties against Final.
roots = {'tpDisplayGeneral', 'tpDisplayTop', 'tpDisplayBottom', 'tpDisplayTransmit'}
def controls(text):
    names = set(roots)
    edges = re.findall(r'this\.(\w+)\.Controls.Add\(this\.(\w+)\)', text)
    while True:
        expanded = names | {child for parent, child in edges if parent in names}
        if expanded == names:
            return names
        names = expanded
actual_controls, final_controls = controls(designer), controls(ref_setup)
assert final_controls - actual_controls == {'chkDetachPanafall'}
assert not actual_controls - final_controls
names = actual_controls & final_controls
def properties(text):
    return {(name, prop): re.sub(r'(?:System\.Windows\.Forms\.|System\.Drawing\.|\s+)', '', value)
            for name, prop, value in re.findall(r'this\.(\w+)\.(\w+) = ([^;\n]+);', text)
            if name in names and prop in ['Text','Location','Size','Minimum','Maximum','Checked','SelectedIndex','Visible','Enabled']}
actual_props, final_props = properties(designer), properties(ref_setup)
for key in actual_props.keys() & final_props.keys():
    assert actual_props[key] == final_props[key], key
print('DISPLAY_STATIC_UI_PARITY=PASS (known missing feature: Detach Panafall)')
print('FINAL_PALETTE_SOURCE_PARITY=PASS')
print('SETUP_GENERAL_AND_PALETTE_LISTS=PASS')
print('GPU_START_RETRY_PRESERVED=PASS')
print('CONFIRMED_NOGHOST_EMBEDDED_SHADERS_AND_8_16_BIT_PATCH=PASS')

# All GPU menu controls and their event connections, including quality and General.
init_names = ['InitGeneralTabWaterfallControls', 'InitNoiseFloorProControls', 'InitWaterfallQualityControls']
actual_init = '\n'.join(method(ui if name != 'InitWaterfallQualityControls' else setup, name) for name in init_names)
final_init = '\n'.join(method(ref_setup, name) for name in init_names)
def event_map(text):
    return set(re.findall(r'(\w+)\.(\w+) \+= (\w+);', text))
actual_events = event_map(actual_init)
final_events = event_map(final_init)
assert actual_events == final_events, (actual_events - final_events, final_events - actual_events)
for control, event, handler in sorted(final_events):
    if handler == 'OnGPUWaterfallEffectiveOverlapChanged':
        source = ui
    else:
        source = ui if re.search(r'private void ' + handler + r'\(', ui) else setup
    assert compact(method(source, handler)) == compact(method(ref_setup, handler)), handler
    print(f'EVENT_PARITY=PASS {control}.{event} -> {handler}')
# Exact control construction, except the already-proven retry timer callback and
# location of the init calls (retained constructor ordering).
def without_timer(text):
    a = text.index('if (_gpuStatusTimer == null)')
    b = text.index('num3 += 28;', a)
    return text[:a] + text[b:]
assert compact(without_timer(method(ui, 'InitNoiseFloorProControls'))) == compact(without_timer(method(ref_setup, 'InitNoiseFloorProControls')))
a = method(setup, 'InitWaterfallQualityControls')
b = method(ref_setup, 'InitWaterfallQualityControls')
a = re.sub(r'//[^\n]*', '', a)
b = b.replace('InitGeneralTabWaterfallControls();', '').replace('InitNoiseFloorProControls(groupBox, num4);', '')
assert compact(a) == compact(b)
assert compact(method(setup, 'SyncWaterfallEnhancerFromControls')) == compact(method(ref_setup, 'SyncWaterfallEnhancerFromControls'))
print(f'GPU_MENU_ALL_CONTROLS_EVENTS_AND_RESTORE=PASS ({len(final_events)} event connections)')
