from pathlib import Path
import importlib.util
import re

HERE = Path(__file__).resolve().parent
BASE_PATH = HERE / 'fix-ui-deps.py'

spec = importlib.util.spec_from_file_location('sq4kou_rade_fix_ui_deps', BASE_PATH)
base = importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)
mod = base.mod


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def patch_setup_public_properties():
    """Close the exact Setup properties referenced by SV1EIA Console mirrors."""
    names = (
        'RADAE',
        'RADAEReporter',
        'RADAEReporting',
        'RADAERX2',
        'RADAEReportingRX2',
        'RADAEVersionRX1',
        'RADAEVersionRX2',
    )
    base.transplant(
        'Project Files/Source/Console/setup.cs',
        'Setup',
        properties=names,
    )
    text = mod.read_text(mod.CONSOLE / 'setup.cs')
    for name in names:
        require(base.target_has_member(text, mod.PROPERTY_RE, name),
                'Setup property missing after transplant: ' + name)
    print('SV1EIA_SETUP_PUBLIC_PROPERTIES=PASS')


def normalize_reporter_compile_items():
    """Put FreeDVReporter Compile items in the existing primary Compile ItemGroup."""
    path = mod.CONSOLE / 'Thetis.csproj'
    text = mod.read_text(path)

    items = (
        ('FreeDVReporter\\FreeDVReporterClient.cs', False),
        ('FreeDVReporter\\FreeDVReporterForm.cs', True),
        ('FreeDVReporter\\FreeDVReporterManager.cs', False),
        ('FreeDVReporter\\Maidenhead.cs', False),
    )

    for include, _ in items:
        esc = re.escape(include)
        text = re.sub(
            r'(?m)^\s*<Compile\s+Include="' + esc + r'"\s*/>\s*\r?\n?',
            '', text)
        text = re.sub(
            r'(?ms)^\s*<Compile\s+Include="' + esc + r'"\s*>.*?</Compile>\s*\r?\n?',
            '', text)

    text = re.sub(r'(?ms)^\s*<ItemGroup>\s*</ItemGroup>\s*\r?\n?', '', text)

    lines = [
        '    <Compile Include="FreeDVReporter\\FreeDVReporterClient.cs" />',
        '    <Compile Include="FreeDVReporter\\FreeDVReporterForm.cs">',
        '      <SubType>Form</SubType>',
        '    </Compile>',
        '    <Compile Include="FreeDVReporter\\FreeDVReporterManager.cs" />',
        '    <Compile Include="FreeDVReporter\\Maidenhead.cs" />',
    ]
    payload = '\n'.join(lines) + '\n'
    anchor = '    <Compile Include="cmaster.cs" />'
    require(anchor in text, 'Thetis.csproj cmaster.cs primary Compile anchor missing')
    text = text.replace(anchor, payload + anchor, 1)
    mod.write_text(path, text)

    final = mod.read_text(path)
    cmaster_pos = final.find(anchor)
    require(cmaster_pos >= 0, 'cmaster.cs anchor lost')
    for include, _ in items:
        needle = '<Compile Include="' + include + '"'
        require(final.count(needle) == 1,
                f'expected exactly one Compile item for {include}, got {final.count(needle)}')
        require(final.find(needle) < cmaster_pos,
                'Reporter Compile item is not in primary source group: ' + include)
        src = mod.CONSOLE / include.replace('\\', '/')
        require(src.is_file(), 'Reporter source missing: ' + include)
        source = mod.read_text(src)
        require('namespace Thetis.FreeDVReporter' in source,
                'Unexpected reporter namespace: ' + include)

    print('FREEDV_REPORTER_PRIMARY_COMPILE_GROUP=PASS')


def _initialize_component(text):
    found = [(s, e, block) for name, s, e, block
             in mod.members_with_bodies(text, mod.METHOD_RE)
             if name == 'InitializeComponent']
    require(len(found) == 1, f'InitializeComponent expected once, found {len(found)}')
    return found[0]


def patch_setup_designer_runtime_dependencies():
    """Fix the runtime-only dependency missed by the selective designer port.

    The RADE/Reporter lines add two controls to tpGeneralLog.  The generic
    owner name did not match the RADE selector, so the field existed but the
    TabPage was never constructed.  C# therefore compiled but crashed in
    Setup.InitializeComponent().  Construct the owner before any dereference,
    attach it to tcGeneral, and gate every RADE/Reporter designer control for
    assignment-before-use.
    """
    path = mod.CONSOLE / 'setup.designer.cs'
    text = mod.read_text(path)
    start, end, block = _initialize_component(text)

    # Normalize tpGeneralLog construction to the first executable section of
    # InitializeComponent.  This guarantees it exists before Controls.Add.
    block = re.sub(
        r'(?m)^\s*this\.tpGeneralLog\s*=\s*new\s+System\.Windows\.Forms\.TabPage\(\);\s*\r?\n?',
        '', block)
    open_pos = block.find('{')
    require(open_pos >= 0, 'InitializeComponent opening brace missing')
    nl = block.find('\n', open_pos)
    require(nl >= 0, 'InitializeComponent opening newline missing')
    init_line = '            this.tpGeneralLog = new System.Windows.Forms.TabPage();\n'
    block = block[:nl + 1] + init_line + block[nl + 1:]

    # The vendor exposes this as General -> Log.  Attach once, after the
    # existing General tab additions, without importing unrelated log widgets.
    add_line = '            this.tcGeneral.Controls.Add(this.tpGeneralLog);'
    if add_line not in block:
        adds = list(re.finditer(
            r'(?m)^\s*this\.tcGeneral\.Controls\.Add\(this\.[A-Za-z_]\w*\);\s*$', block))
        require(adds, 'tcGeneral Controls.Add anchor missing')
        p = adds[-1].end()
        block = block[:p] + '\n' + add_line + block[p:]

    # Give the transplanted tab the exact vendor identity/layout essentials.
    # Existing RADE/Reporter child Controls.Add calls remain untouched.
    if 'this.tpGeneralLog.Name = "tpGeneralLog";' not in block:
        props = (
            '            this.tpGeneralLog.BackColor = System.Drawing.SystemColors.Control;\n'
            '            this.tpGeneralLog.Location = new System.Drawing.Point(4, 44);\n'
            '            this.tpGeneralLog.Name = "tpGeneralLog";\n'
            '            this.tpGeneralLog.Padding = new System.Windows.Forms.Padding(3);\n'
            '            this.tpGeneralLog.Size = new System.Drawing.Size(596, 296);\n'
            '            this.tpGeneralLog.TabIndex = 11;\n'
            '            this.tpGeneralLog.Text = "Log";\n'
            '            this.toolTip1.SetToolTip(this.tpGeneralLog, "Network Diagnostics Log");\n'
        )
        first_child = re.search(
            r'(?m)^\s*this\.tpGeneralLog\.Controls\.Add\(', block)
        require(first_child is not None, 'tpGeneralLog RADE/Reporter child anchor missing')
        p = first_child.start()
        block = block[:p] + props + block[p:]

    text = text[:start] + block + text[end:]
    mod.write_text(path, text)

    # Static runtime-order gate: every RADE/Reporter control dereferenced in
    # InitializeComponent must have a preceding `new`, plus the generic owner.
    final = mod.read_text(path)
    _, _, init = _initialize_component(final)
    names = {'tpGeneralLog'}
    names.update(re.findall(
        r'this\.([A-Za-z_]\w*(?:rade|reporter)[A-Za-z0-9_]*)\.',
        init, flags=re.I))
    require('chkReporterLogEnable' in names and 'chkRadaeLogEnable' in names,
            'Reporter/RADAE log controls missing from runtime gate')

    for name in sorted(names):
        use_m = re.search(r'this\.' + re.escape(name) + r'\.', init)
        if use_m is None:
            continue
        new_m = re.search(r'this\.' + re.escape(name) + r'\s*=\s*new\s+', init)
        require(new_m is not None,
                'RUNTIME_NULL_RISK: control used without construction: ' + name)
        require(new_m.start() < use_m.start(),
                'RUNTIME_NULL_RISK: control constructed after first use: ' + name)

    require(add_line in init, 'tpGeneralLog is not attached to tcGeneral')
    print('TPGENERALLOG_RUNTIME_DEP=PASS')
    print('SV1EIA_SETUP_RUNTIME_CONTROL_ORDER=PASS')


def validate_protected_arbiter():
    ri = mod.read_text(mod.CONSOLE / 'RadeIntegration.cs')
    for token in (
        'SetRadaeTxSilenceHold(1)',
        'RadaeNotifyEndOfOver()',
        'GetRadaeEooFlushed()',
    ):
        require(token in ri, 'SQ4KOU EOO arbiter damaged: ' + token)
    print('SQ4KOU_REDPITAYA_PTT_EOO_STILL=PASS')


def main():
    patch_setup_public_properties()
    normalize_reporter_compile_items()
    patch_setup_designer_runtime_dependencies()
    validate_protected_arbiter()
    print('FINAL_SV1EIA_RADE_COMPILE_CLOSURE=PASS')


if __name__ == '__main__':
    main()
