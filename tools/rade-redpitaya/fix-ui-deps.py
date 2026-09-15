from pathlib import Path
import importlib.util
import re

HERE = Path(__file__).resolve().parent
UI_PATH = HERE / 'integrate-ui.py'

spec = importlib.util.spec_from_file_location('sq4kou_rade_integrate_ui_wrapper', UI_PATH)
ui = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ui)
mod = ui.mod

# Keep the lexical-safe parser and true class-boundary logic from integrate-ui.py.
mod.brace_end = ui.safe_brace_end
mod.members_with_bodies = ui.safe_members_with_bodies
mod.class_insert_pos = ui.safe_class_insert_pos

FIELD_RE = re.compile(
    r'^\s*(?:public|private|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:volatile\s+)?[^;{}]+;\s*$')


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def class_insert_pos_for(text, class_name):
    mask = ui._code_mask(text)
    matches = []
    for m in ui._CLASS_RE.finditer(text):
        if m.group('name') != class_name:
            continue
        if m.start('kw') >= len(mask) or not mask[m.start('kw')]:
            continue
        i = m.end()
        open_pos = -1
        while i < len(text):
            if not mask[i]:
                i += 1
                continue
            if text[i] == '{':
                open_pos = i
                break
            if text[i] in ';}':
                break
            i += 1
        if open_pos >= 0:
            end = ui._brace_end_with_mask(text, open_pos, mask)
            matches.append(end - 1)
    require(len(matches) == 1, f'expected exactly one class {class_name}, found {len(matches)}')
    return matches[0]


def insert_into_class(text, class_name, blocks):
    blocks = [b.rstrip() for b in blocks if b and b.strip()]
    if not blocks:
        return text
    pos = class_insert_pos_for(text, class_name)
    return text[:pos] + '\n\n' + '\n\n'.join(blocks) + '\n' + text[pos:]


def named_body_blocks(text, regex, names):
    names = set(names)
    return [(name, block) for name, _, _, block in mod.members_with_bodies(text, regex) if name in names]


def field_block(text, name):
    pat = re.compile(r'\b' + re.escape(name) + r'\b')
    for line in text.splitlines():
        if pat.search(line) and FIELD_RE.match(line):
            return line
    raise RuntimeError('field declaration not found: ' + name)


def target_has_field(text, name):
    return re.search(r'\b' + re.escape(name) + r'\b', text) is not None


def target_has_member(text, regex, name):
    return any(n == name for n, _, _, _ in mod.members_with_bodies(text, regex))


def transplant(rel, class_name, methods=(), properties=(), fields=(), overload_methods=()):
    ref = mod.vendor_text(rel)
    path = mod.ROOT / rel
    target = mod.read_text(path)
    blocks = []

    for name in fields:
        if not target_has_field(target, name):
            blocks.append(field_block(ref, name))

    for name in properties:
        if not target_has_member(target, mod.PROPERTY_RE, name):
            found = [b for n, b in named_body_blocks(ref, mod.PROPERTY_RE, [name])]
            require(len(found) == 1, f'property {name} expected once in {rel}, found {len(found)}')
            blocks.append(found[0])

    for name in methods:
        if not target_has_member(target, mod.METHOD_RE, name):
            found = [b for n, b in named_body_blocks(ref, mod.METHOD_RE, [name])]
            require(len(found) == 1, f'method {name} expected once in {rel}, found {len(found)}')
            blocks.append(found[0])

    for name in overload_methods:
        if not target_has_member(target, mod.METHOD_RE, name):
            found = [b for n, b in named_body_blocks(ref, mod.METHOD_RE, [name])]
            require(len(found) >= 1, f'overloaded method {name} missing in {rel}')
            blocks.extend(found)

    if blocks:
        target = insert_into_class(target, class_name, blocks)
        mod.write_text(path, target)
    print(f'DEP_CLOSURE {rel}: added={len(blocks)}')


def patch_console_dependencies():
    rel = 'Project Files/Source/Console/console.cs'
    transplant(
        rel, 'Console',
        methods=(
            'SetMoxEnabled',
            'SetRx1RadeControlVisible',
            'SetRx2RadeControlVisible',
        ),
        properties=(
            'chkRADEMirror', 'chkREPRMirror', 'chkVISMirror',
            'chkRADERX2Mirror', 'chkVISRX2Mirror',
            'cmbRadeVersionRX1Mirror', 'cmbRadeVersionRX2Mirror',
            'RadeMeasureRx1', 'RadeMeasureRx2', 'RadeMeasureTx',
            'RadaeEooCallsign',
        ),
        fields=(
            '_rade_measure_rx1', '_rade_measure_rx2', '_rade_measure_tx',
            '_radae_eoo_callsign',
        ),
    )


def patch_setup_dependencies():
    rel = 'Project Files/Source/Console/setup.cs'
    transplant(
        rel, 'Setup',
        methods=('UpdateTxMeasureEnabled',),
        fields=('_forcingAllEvents', 'm_radaeCallUpdating', 'm_radaeGridUpdating'),
    )

    # The exact SV1EIA RADE subtree contains two references to this existing
    # vendor tab page.  The original transitive designer selector brought the
    # initialization lines but not the declaration because its name is generic.
    transplant(
        'Project Files/Source/Console/setup.designer.cs', 'Setup',
        fields=('tpGeneralLog',),
    )


def patch_common_dependencies():
    rel = 'Project Files/Source/Console/common.cs'
    transplant(
        rel, 'Common',
        methods=('LogNetError',),
        fields=('m_oNetLogLock', 'LogEnabled', 'LogMaxLines', 'OnLogOverflow'),
    )


def patch_meter_visibility_dependencies():
    # Setup -> DSP -> RADE exposes the exact SV1EIA meter-container gate.
    # Port only that gate and its two small helpers, not unrelated meter code.
    transplant(
        'Project Files/Source/Console/MeterManager.cs', 'MeterManager',
        methods=('containerShouldHide', 'applyContainerVisibilityGates'),
        overload_methods=('ContainerHidesWhenRADENotEnabled',),
    )
    transplant(
        'Project Files/Source/Console/ucMeter.cs', 'ucMeter',
        properties=('ContainerHidesWhenRADENotEnabled',),
        fields=('_container_hides_when_rade_not_enabled',),
    )
    transplant(
        'Project Files/Source/Console/frmMeterDisplay.cs', 'frmMeterDisplay',
        properties=('ContainerHidesWhenRADENotEnabled',),
        fields=('_container_hides_when_rade_not_enabled',),
    )


def patch_reporter_project_items():
    path = mod.CONSOLE / 'Thetis.csproj'
    text = mod.read_text(path)
    includes = [
        'FreeDVReporter\\FreeDVReporterClient.cs',
        'FreeDVReporter\\FreeDVReporterForm.cs',
        'FreeDVReporter\\FreeDVReporterManager.cs',
        'FreeDVReporter\\Maidenhead.cs',
    ]
    missing = [p for p in includes if f'Include="{p}"' not in text]
    if missing:
        lines = ['  <ItemGroup>']
        for p in missing:
            if p.endswith('FreeDVReporterForm.cs'):
                lines += [f'    <Compile Include="{p}">', '      <SubType>Form</SubType>', '    </Compile>']
            else:
                lines.append(f'    <Compile Include="{p}" />')
        lines.append('  </ItemGroup>')
        item = '\n'.join(lines) + '\n'
        require('</Project>' in text, 'Thetis.csproj closing tag missing')
        text = text.replace('</Project>', item + '</Project>', 1)
        mod.write_text(path, text)

    for p in includes:
        require((mod.CONSOLE / p.replace('\\', '/')).is_file(), 'Reporter source missing on disk: ' + p)
    final = mod.read_text(path)
    for p in includes:
        require(f'Include="{p}"' in final, 'Reporter Compile item missing: ' + p)
    print('FREEDV_REPORTER_PROJECT_ITEMS=PASS')


def validate():
    console = mod.read_text(mod.CONSOLE / 'console.cs')
    setup = mod.read_text(mod.CONSOLE / 'setup.cs')
    common = mod.read_text(mod.CONSOLE / 'common.cs')
    designer = mod.read_text(mod.CONSOLE / 'setup.designer.cs')
    meter = mod.read_text(mod.CONSOLE / 'MeterManager.cs')

    for token in (
        'chkRADEMirror', 'chkREPRMirror', 'chkVISMirror', 'chkRADERX2Mirror',
        'chkVISRX2Mirror', 'cmbRadeVersionRX1Mirror', 'cmbRadeVersionRX2Mirror',
        'SetMoxEnabled', 'SetRx1RadeControlVisible', 'SetRx2RadeControlVisible',
        'RadeMeasureRx1', 'RadeMeasureRx2', 'RadeMeasureTx', 'RadaeEooCallsign',
    ):
        require(token in console, 'console dependency missing: ' + token)
    for token in ('_forcingAllEvents', 'm_radaeCallUpdating', 'm_radaeGridUpdating', 'UpdateTxMeasureEnabled'):
        require(token in setup, 'setup dependency missing: ' + token)
    for token in ('LogEnabled', 'LogNetError'):
        require(token in common, 'common dependency missing: ' + token)
    require('tpGeneralLog' in designer, 'designer dependency missing: tpGeneralLog')
    require('ContainerHidesWhenRADENotEnabled' in meter, 'MeterManager RADE gate missing')

    # Never replace the SQ4KOU EOO/PTT arbiter.  The protected workflow gate
    # still checks the same hooks after this closure step.
    ri = mod.read_text(mod.CONSOLE / 'RadeIntegration.cs')
    for token in ('SetRadaeTxSilenceHold(1)', 'RadaeNotifyEndOfOver()', 'GetRadaeEooFlushed()'):
        require(token in ri, 'SQ4KOU EOO arbiter damaged: ' + token)
    print('SV1EIA_RADE_UI_DEPENDENCY_CLOSURE=PASS')


def main():
    patch_console_dependencies()
    patch_setup_dependencies()
    patch_common_dependencies()
    patch_meter_visibility_dependencies()
    patch_reporter_project_items()
    validate()


if __name__ == '__main__':
    main()
