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


def field_decl_line(text, name):
    pat = re.compile(r'\b' + re.escape(name) + r'\b')
    for line in text.splitlines():
        if pat.search(line) and FIELD_RE.match(line):
            return line
    raise RuntimeError('field/declaration not found: ' + name)


def target_has_field(text, name):
    # A usage is NOT a declaration.  The previous implementation used a raw
    # token search and therefore skipped fields referenced by transplanted code.
    pat = re.compile(r'\b' + re.escape(name) + r'\b')
    return any(pat.search(line) and FIELD_RE.match(line) for line in text.splitlines())


def target_has_member(text, regex, name):
    return any(n == name for n, _, _, _ in mod.members_with_bodies(text, regex))


def transplant(rel, class_name, methods=(), properties=(), fields=(), overload_methods=()):
    ref = mod.vendor_text(rel)
    path = mod.ROOT / rel
    target = mod.read_text(path)
    blocks = []

    for name in fields:
        if not target_has_field(target, name):
            blocks.append(field_decl_line(ref, name))

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


def declaration_once(rel, class_name, token, exact_decl_fragment):
    ref = mod.vendor_text(rel)
    path = mod.ROOT / rel
    target = mod.read_text(path)
    if exact_decl_fragment in target:
        return
    line = next((ln for ln in ref.splitlines() if exact_decl_fragment in ln), None)
    require(line is not None, f'declaration not found in vendor {rel}: {token}')
    target = insert_into_class(target, class_name, [line])
    mod.write_text(path, target)
    print(f'DEP_DECL {rel}: {token}')


def _method_record(text, name):
    found = [(n, s, e, b) for n, s, e, b in mod.members_with_bodies(text, mod.METHOD_RE) if n == name]
    require(len(found) == 1, f'method {name} expected once, found {len(found)}')
    return found[0]


def _designer_statements(method_block, controls):
    lines = method_block.splitlines()
    out = []
    i = 0
    tokens = tuple('this.' + c for c in controls)
    while i < len(lines):
        line = lines[i]
        stripped = line.strip()
        if stripped.startswith('//') or not any(t in line for t in tokens):
            i += 1
            continue
        stmt = [line]
        # Designer statements may span multiple lines (notably Items.AddRange).
        while ';' not in stmt[-1] and i + 1 < len(lines):
            i += 1
            stmt.append(lines[i])
        block = '\n'.join(stmt)
        if block.strip() not in [x.strip() for x in out]:
            out.append(block)
        i += 1
    return out


def patch_console_designer_controls():
    rel = 'Project Files/Source/Console/console.Designer.cs'
    ref = mod.vendor_text(rel)
    path = mod.ROOT / rel
    target = mod.read_text(path)
    controls = (
        'chkRADE', 'chkREPR', 'chkVIS', 'cmbRadeVersionRX1',
        'chkRADERX2', 'chkVISRX2', 'cmbRadeVersionRX2',
    )

    field_blocks = []
    for name in controls:
        if not target_has_field(target, name):
            field_blocks.append(field_decl_line(ref, name))
    if field_blocks:
        target = insert_into_class(target, 'Console', field_blocks)

    _, _, _, vb = _method_record(ref, 'InitializeComponent')
    _, ts, te, tb = _method_record(target, 'InitializeComponent')
    stmts = _designer_statements(vb, controls)
    inst = [s for s in stmts if ' = new ' in s]
    config = [s for s in stmts if ' = new ' not in s]

    def missing(block, seq):
        return [s for s in seq if s.strip() not in block]

    inst = missing(tb, inst)
    config = missing(tb, config)
    new_tb = tb
    if inst:
        brace = new_tb.find('{')
        require(brace >= 0, 'InitializeComponent opening brace missing')
        nl = new_tb.find('\n', brace)
        require(nl >= 0, 'InitializeComponent newline missing')
        new_tb = new_tb[:nl + 1] + '\n'.join(inst) + '\n' + new_tb[nl + 1:]
    if config:
        pos = new_tb.rfind('            this.ResumeLayout(false);')
        if pos < 0:
            pos = new_tb.rfind('}')
        require(pos >= 0, 'InitializeComponent closing insertion point missing')
        new_tb = new_tb[:pos] + '\n'.join(config) + '\n' + new_tb[pos:]

    if new_tb != tb:
        target = target[:ts] + new_tb + target[te:]
    mod.write_text(path, target)

    final = mod.read_text(path)
    _, _, _, init = _method_record(final, 'InitializeComponent')
    for name in controls:
        require(target_has_field(final, name), 'console designer field missing: ' + name)
        require(('this.' + name + ' = new ') in init, 'console designer init missing: ' + name)
    print('SV1EIA_RADE_CONSOLE_CONTROLS=PASS')


def patch_console_dependencies():
    rel = 'Project Files/Source/Console/console.cs'
    transplant(
        rel, 'Console',
        methods=(
            'SetMoxEnabled',
            'SetRx1RadeControlVisible',
            'SetRx2RadeControlVisible',
            'chkRADE_CheckedChanged',
            'chkREPR_CheckedChanged',
            'chkVIS_CheckedChanged',
            'chkRADERX2_CheckedChanged',
            'chkVISRX2_CheckedChanged',
            'cmbRadeVersionRX1_SelectedIndexChanged',
            'cmbRadeVersionRX2_SelectedIndexChanged',
            'NotifyRadaeEnabledChanged',
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
            '_radae_eoo_callsign', 'RadaeEnabledChangedHandlers',
        ),
    )
    declaration_once(
        rel, 'Console', 'RadaeEnabledChanged',
        'public delegate void RadaeEnabledChanged(int rx, bool enabled)')


def patch_setup_general_log_designer():
    """Restore the real SV1EIA tpGeneralLog lifecycle, not just its field.

    The RADE surface adds children to tpGeneralLog.  A field declaration alone
    compiles but leaves the TabPage null at runtime, causing Setup.InitializeComponent
    to throw before Thetis starts.  Copy all vendor designer statements that touch
    tpGeneralLog and force construction before the first use.
    """
    rel = 'Project Files/Source/Console/setup.designer.cs'
    ref = mod.vendor_text(rel)
    path = mod.ROOT / rel
    target = mod.read_text(path)

    if not target_has_field(target, 'tpGeneralLog'):
        target = insert_into_class(target, 'Setup', [field_decl_line(ref, 'tpGeneralLog')])

    _, _, _, vendor_init = _method_record(ref, 'InitializeComponent')
    _, ts, te, target_init = _method_record(target, 'InitializeComponent')
    statements = _designer_statements(vendor_init, ('tpGeneralLog',))
    ctor = [s for s in statements if re.search(r'this\.tpGeneralLog\s*=\s*new\s+', s)]
    require(len(ctor) == 1, f'expected one SV1EIA tpGeneralLog constructor statement, found {len(ctor)}')

    new_init = target_init
    ctor_stmt = ctor[0]
    if ctor_stmt.strip() not in new_init:
        brace = new_init.find('{')
        require(brace >= 0, 'Setup.InitializeComponent opening brace missing')
        nl = new_init.find('\n', brace)
        require(nl >= 0, 'Setup.InitializeComponent newline missing')
        new_init = new_init[:nl + 1] + ctor_stmt + '\n' + new_init[nl + 1:]

    missing = [s for s in statements if s.strip() != ctor_stmt.strip() and s.strip() not in new_init]
    if missing:
        pos = new_init.rfind('            this.ResumeLayout(false);')
        if pos < 0:
            pos = new_init.rfind('}')
        require(pos >= 0, 'Setup.InitializeComponent closing insertion point missing')
        new_init = new_init[:pos] + '\n'.join(missing) + '\n' + new_init[pos:]

    if new_init != target_init:
        target = target[:ts] + new_init + target[te:]
    mod.write_text(path, target)

    final = mod.read_text(path)
    _, _, _, init = _method_record(final, 'InitializeComponent')
    ctor_pos = init.find('this.tpGeneralLog = new ')
    use_positions = [m.start() for m in re.finditer(r'this\.tpGeneralLog\.', init)]
    require(ctor_pos >= 0, 'tpGeneralLog is never constructed')
    require(use_positions, 'tpGeneralLog has no designer use')
    require(ctor_pos < min(use_positions), 'tpGeneralLog is used before construction')
    parent = re.search(
        r'this\.(?!tpGeneralLog\b)[A-Za-z_]\w*\.(?:Controls|TabPages)\.(?:Add|AddRange)\([^;]*this\.tpGeneralLog',
        init, flags=re.S)
    require(parent is not None, 'tpGeneralLog is not attached to a parent control')
    print('SV1EIA_TPGENERALLOG_INIT_ORDER=PASS')


def patch_setup_dependencies():
    rel = 'Project Files/Source/Console/setup.cs'
    transplant(
        rel, 'Setup',
        methods=('UpdateTxMeasureEnabled',),
        fields=('_forcingAllEvents', 'm_radaeCallUpdating', 'm_radaeGridUpdating'),
    )
    patch_setup_general_log_designer()


def patch_common_dependencies():
    rel = 'Project Files/Source/Console/common.cs'
    transplant(
        rel, 'Common',
        methods=('LogNetError',),
        fields=('m_oNetLogLock', 'LogEnabled', 'LogMaxLines', 'OnLogOverflow'),
    )


def patch_meter_visibility_dependencies():
    rel = 'Project Files/Source/Console/MeterManager.cs'
    transplant(
        rel, 'MeterManager',
        methods=('containerShouldHide', 'applyContainerVisibilityGates', 'OnRadaeEnabledChanged'),
        overload_methods=('ContainerHidesWhenRADENotEnabled',),
    )
    path = mod.ROOT / rel
    text = mod.read_text(path)
    wiring = (
        ('_console.RX2EnabledPreChangedHandlers += OnRX2EnabledPreChanged;',
         '_console.RadaeEnabledChangedHandlers += OnRadaeEnabledChanged;'),
        ('_console.RX2EnabledPreChangedHandlers -= OnRX2EnabledPreChanged;',
         '_console.RadaeEnabledChangedHandlers -= OnRadaeEnabledChanged;'),
    )
    for anchor, line in wiring:
        if line not in text:
            require(anchor in text, 'MeterManager event-wiring anchor missing: ' + anchor)
            text = text.replace(anchor, anchor + '\n            ' + line, 1)
    mod.write_text(path, text)

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
    missing = [p for p in includes if f'<Compile Include="{p}"' not in text]
    if missing:
        lines = []
        for p in missing:
            if p.endswith('FreeDVReporterForm.cs'):
                lines += [f'    <Compile Include="{p}">', '      <SubType>Form</SubType>', '    </Compile>']
            else:
                lines.append(f'    <Compile Include="{p}" />')
        insert = '\n'.join(lines) + '\n'
        anchor = '    <Compile Include="cmaster.cs"'
        pos = text.find(anchor)
        require(pos >= 0, 'Thetis.csproj cmaster.cs Compile anchor missing')
        text = text[:pos] + insert + text[pos:]
        mod.write_text(path, text)

    for p in includes:
        require((mod.CONSOLE / p.replace('\\', '/')).is_file(), 'Reporter source missing on disk: ' + p)
    final = mod.read_text(path)
    for p in includes:
        require(f'<Compile Include="{p}"' in final, 'Reporter Compile item missing: ' + p)
    print('FREEDV_REPORTER_PROJECT_ITEMS=PASS')


def validate():
    console = mod.read_text(mod.CONSOLE / 'console.cs')
    cdesigner = mod.read_text(mod.CONSOLE / 'console.Designer.cs')
    setup = mod.read_text(mod.CONSOLE / 'setup.cs')
    common = mod.read_text(mod.CONSOLE / 'common.cs')
    designer = mod.read_text(mod.CONSOLE / 'setup.designer.cs')
    meter = mod.read_text(mod.CONSOLE / 'MeterManager.cs')

    for token in (
        'chkRADEMirror', 'chkREPRMirror', 'chkVISMirror', 'chkRADERX2Mirror',
        'chkVISRX2Mirror', 'cmbRadeVersionRX1Mirror', 'cmbRadeVersionRX2Mirror',
        'SetMoxEnabled', 'SetRx1RadeControlVisible', 'SetRx2RadeControlVisible',
        'RadeMeasureRx1', 'RadeMeasureRx2', 'RadeMeasureTx', 'RadaeEooCallsign',
        'NotifyRadaeEnabledChanged', 'RadaeEnabledChangedHandlers',
    ):
        require(token in console, 'console dependency missing: ' + token)
    for token in ('chkRADE', 'chkREPR', 'chkVIS', 'cmbRadeVersionRX1', 'chkRADERX2', 'chkVISRX2', 'cmbRadeVersionRX2'):
        require(target_has_field(cdesigner, token), 'console designer field missing: ' + token)
    for token in ('_forcingAllEvents', 'm_radaeCallUpdating', 'm_radaeGridUpdating', 'UpdateTxMeasureEnabled'):
        require(target_has_field(setup, token) or token == 'UpdateTxMeasureEnabled', 'setup dependency missing: ' + token)
    for token in ('LogEnabled', 'm_oNetLogLock'):
        require(target_has_field(common, token), 'common field missing: ' + token)
    require('LogNetError' in common, 'common dependency missing: LogNetError')
    require(target_has_field(designer, 'tpGeneralLog'), 'designer dependency missing: tpGeneralLog')

    _, _, _, setup_init = _method_record(designer, 'InitializeComponent')
    ctor_pos = setup_init.find('this.tpGeneralLog = new ')
    use_positions = [m.start() for m in re.finditer(r'this\.tpGeneralLog\.', setup_init)]
    require(ctor_pos >= 0 and use_positions and ctor_pos < min(use_positions),
            'tpGeneralLog runtime initialization order gate failed')
    require(re.search(
        r'this\.(?!tpGeneralLog\b)[A-Za-z_]\w*\.(?:Controls|TabPages)\.(?:Add|AddRange)\([^;]*this\.tpGeneralLog',
        setup_init, flags=re.S) is not None,
        'tpGeneralLog parent attachment gate failed')

    require('ContainerHidesWhenRADENotEnabled' in meter, 'MeterManager RADE gate missing')
    require('_console.RadaeEnabledChangedHandlers += OnRadaeEnabledChanged;' in meter, 'MeterManager RADE event subscribe missing')
    require('_console.RadaeEnabledChangedHandlers -= OnRadaeEnabledChanged;' in meter, 'MeterManager RADE event unsubscribe missing')

    ri = mod.read_text(mod.CONSOLE / 'RadeIntegration.cs')
    for token in ('SetRadaeTxSilenceHold(1)', 'RadaeNotifyEndOfOver()', 'GetRadaeEooFlushed()'):
        require(token in ri, 'SQ4KOU EOO arbiter damaged: ' + token)
    print('SV1EIA_RADE_UI_DEPENDENCY_CLOSURE=PASS')
    print('SV1EIA_SETUP_STARTUP_NULL_GATE=PASS')


def main():
    patch_console_designer_controls()
    patch_console_dependencies()
    patch_setup_dependencies()
    patch_common_dependencies()
    patch_meter_visibility_dependencies()
    patch_reporter_project_items()
    validate()


if __name__ == '__main__':
    main()
