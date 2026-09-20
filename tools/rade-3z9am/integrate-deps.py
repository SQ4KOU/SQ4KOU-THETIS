from pathlib import Path
import re
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[2]
VENDOR = ROOT / 'Project Files' / 'lib' / 'Thetis-RADE-vendor'
CONSOLE = ROOT / 'Project Files' / 'Source' / 'Console'
PIN = '408f2b5232ff0a2aec9b538a40d4cb1b02627b17'

# Red-Pitaya/PTT/EOO policy: dependency closure is allowed to ADD members that
# are absent from the SQ4KOU base, but it never replaces an existing member.
# Keying/PTT state-machine members stay owned by SQ4KOU.  Two SV1EIA helper
# names containing MOX/EOO are UI/reporter metadata helpers, not keying loops,
# and are explicitly allowed because the imported Setup handlers call them.
DENY_RE = re.compile(r'(?i)(pollptt|pttstatemachine|endofove?r|notifyend|eoo(flush|timeout|margin)|un-?key)')
ALLOW_DENIED = {'SetMoxEnabled', 'RadaeEooCallsign'}


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def read_text(path):
    return Path(path).read_text(encoding='utf-8-sig')


def write_text(path, text):
    Path(path).write_text(text, encoding='utf-8')


def vendor_text(rel):
    p = VENDOR / rel
    require(p.exists(), 'vendor file missing: ' + rel)
    return read_text(p)


def code_mask(text):
    mask = bytearray(len(text))
    i = 0
    state = 'code'
    while i < len(text):
        ch = text[i]
        nxt = text[i + 1] if i + 1 < len(text) else ''
        if state == 'code':
            if ch == '/' and nxt == '/':
                state = 'line_comment'; i += 2; continue
            if ch == '/' and nxt == '*':
                state = 'block_comment'; i += 2; continue
            if ch == '@' and nxt == '"':
                state = 'verbatim'; i += 2; continue
            if ch == '$' and nxt == '@' and i + 2 < len(text) and text[i + 2] == '"':
                state = 'verbatim'; i += 3; continue
            if ch == '@' and nxt == '$' and i + 2 < len(text) and text[i + 2] == '"':
                state = 'verbatim'; i += 3; continue
            if ch == '$' and nxt == '"':
                state = 'string'; i += 2; continue
            if ch == '"':
                state = 'string'; i += 1; continue
            if ch == "'":
                state = 'char'; i += 1; continue
            mask[i] = 1; i += 1; continue
        if state == 'line_comment':
            if ch == '\n': state = 'code'
            i += 1; continue
        if state == 'block_comment':
            if ch == '*' and nxt == '/': state = 'code'; i += 2
            else: i += 1
            continue
        if state == 'string':
            if ch == '\\': i += 2
            elif ch == '"': state = 'code'; i += 1
            else: i += 1
            continue
        if state == 'verbatim':
            if ch == '"' and nxt == '"': i += 2
            elif ch == '"': state = 'code'; i += 1
            else: i += 1
            continue
        if state == 'char':
            if ch == '\\': i += 2
            elif ch == "'": state = 'code'; i += 1
            else: i += 1
    return mask


def brace_end(text, open_pos, mask=None):
    if mask is None:
        mask = code_mask(text)
    require(0 <= open_pos < len(text) and text[open_pos] == '{' and mask[open_pos], 'invalid opening brace')
    depth = 0
    for i in range(open_pos, len(text)):
        if not mask[i]:
            continue
        if text[i] == '{': depth += 1
        elif text[i] == '}':
            depth -= 1
            if depth == 0: return i + 1
    raise RuntimeError('unbalanced braces')


METHOD_RE = re.compile(
    r'(?m)^[ \t]*(?P<prefix>(?:(?:public|private|protected|internal|static|async|virtual|override|sealed|new|unsafe|extern)\s+)+)'
    r'(?P<ret>[A-Za-z_][\w<>,\.\[\]\? ]*)\s+(?P<name>[A-Za-z_]\w*)\s*\([^;{}]*\)\s*\{')
PROPERTY_RE = re.compile(
    r'(?m)^[ \t]*(?P<prefix>(?:(?:public|private|protected|internal|static|virtual|override|new|unsafe)\s+)+)'
    r'(?P<type>[A-Za-z_][\w<>,\.\[\]\? ]*)\s+(?P<name>[A-Za-z_]\w*)\s*\{')
DELEGATE_RE = re.compile(
    r'(?m)^[ \t]*(?:(?:public|private|protected|internal|static|new|unsafe)\s+)*delegate\s+[^;{}]+\s+(?P<name>[A-Za-z_]\w*)\s*\([^;{}]*\)\s*;')
FIELD_RE = re.compile(
    r'(?m)^[ \t]*(?:(?:public|private|protected|internal)\s+)(?:(?:static|readonly|volatile|const|new|unsafe)\s+)*'
    r'[^\r\n;{}()]+?\s+(?P<name>[A-Za-z_]\w*)\s*(?:=[^;\r\n]*)?;[ \t]*(?://[^\r\n]*)?$')
EXPR_RE = re.compile(
    r'(?m)^[ \t]*(?:(?:public|private|protected|internal|static|async|virtual|override|sealed|new|unsafe)\s+)+'
    r'[^\r\n;{}]+?\s+(?P<name>[A-Za-z_]\w*)\s*(?:\([^;{}]*\))?\s*=>[^;\r\n]+;[ \t]*$')


def member_map(text):
    mask = code_mask(text)
    out = {}
    for rx in (METHOD_RE, PROPERTY_RE):
        for m in rx.finditer(text):
            if not mask[m.start()]:
                continue
            op = next((i for i in range(m.start(), m.end()) if text[i] == '{' and mask[i]), -1)
            if op < 0:
                continue
            try:
                end = brace_end(text, op, mask)
            except RuntimeError:
                continue
            out.setdefault(m.group('name'), text[m.start():end])
    for rx in (DELEGATE_RE, FIELD_RE, EXPR_RE):
        for m in rx.finditer(text):
            if mask[m.start()]:
                out.setdefault(m.group('name'), m.group(0))
    return out


def class_insert_pos(text, class_name):
    mask = code_mask(text)
    rx = re.compile(r'(?m)^[ \t]*(?:(?:public|private|protected|internal|sealed|abstract|static|partial|new|unsafe)\s+)*class\s+' + re.escape(class_name) + r'\b')
    for m in rx.finditer(text):
        if not mask[m.start()]:
            continue
        op = -1
        for i in range(m.end(), len(text)):
            if not mask[i]:
                continue
            if text[i] == '{': op = i; break
            if text[i] in ';}': break
        if op >= 0:
            return brace_end(text, op, mask) - 1
    raise RuntimeError('class not found: ' + class_name)


def denied(name):
    return name not in ALLOW_DENIED and DENY_RE.search(name) is not None


def transplant(rel, class_name, seeds):
    ref = vendor_text('Project Files/Source/Console/' + rel)
    path = CONSOLE / rel
    require(path.exists(), 'target file missing: ' + rel)
    target = read_text(path)
    rmap = member_map(ref)
    tmap = member_map(target)
    queue = list(seeds)
    added = []
    seen = set()

    while queue:
        name = queue.pop(0)
        if name in seen:
            continue
        seen.add(name)
        if name in tmap:
            continue
        require(name in rmap, f'{rel}: vendor member not found: {name}')
        require(not denied(name), f'{rel}: protected keying dependency rejected: {name}')
        block = rmap[name]
        added.append((name, block))
        tmap[name] = block

        # Dependency closure: if the exact transplanted member references another
        # member declared by the same SV1EIA class and absent from SQ4KOU, port it
        # too. Existing SQ4KOU members always win and are never replaced.
        for ident in sorted(set(re.findall(r'\b[A-Za-z_]\w*\b', block))):
            if ident in rmap and ident not in tmap and ident not in seen and not denied(ident):
                queue.append(ident)

    if added:
        pos = class_insert_pos(target, class_name)
        payload = '\n\n        // SQ4KOU: exact SV1EIA RADE dependency closure (pinned 408f2b52)\n' + \
                  '\n\n'.join(block.rstrip() for _, block in added) + '\n'
        target = target[:pos] + payload + target[pos:]
        write_text(path, target)

    final = member_map(read_text(path))
    for name in seeds:
        require(name in final, f'{rel}: dependency transplant failed: {name}')
    print(rel + ' dependency closure: ' + (', '.join(name for name, _ in added) if added else 'already satisfied'))


def ensure_reporter():
    src = VENDOR / 'Project Files' / 'Source' / 'Console' / 'FreeDVReporter'
    dst = CONSOLE / 'FreeDVReporter'
    require(src.is_dir(), 'SV1EIA FreeDVReporter directory missing')
    if dst.exists():
        shutil.rmtree(dst)
    shutil.copytree(src, dst)
    expected = ['FreeDVReporterClient.cs', 'FreeDVReporterForm.cs', 'FreeDVReporterManager.cs', 'Maidenhead.cs']
    names = sorted(p.name for p in dst.glob('*.cs'))
    require(names == sorted(expected), 'unexpected FreeDVReporter source set: ' + ','.join(names))

    p = CONSOLE / 'Thetis.csproj'
    s = read_text(p)
    missing = [n for n in expected if ('FreeDVReporter\\' + n) not in s]
    if missing:
        lines = []
        for n in missing:
            if n == 'FreeDVReporterForm.cs':
                lines.append('    <Compile Include="FreeDVReporter\\FreeDVReporterForm.cs"><SubType>Form</SubType></Compile>')
            else:
                lines.append(f'    <Compile Include="FreeDVReporter\\{n}" />')
        item = '  <ItemGroup>\n' + '\n'.join(lines) + '\n  </ItemGroup>\n'
        require('</Project>' in s, 'Thetis.csproj end missing')
        s = s.replace('</Project>', item + '</Project>', 1)
        write_text(p, s)
    s = read_text(p)
    for n in expected:
        require(('FreeDVReporter\\' + n) in s, 'project include missing: ' + n)
    print('FreeDVReporter source/project closure=PASS')


def clean_orphan_designer_layout():
    p = CONSOLE / 'setup.designer.cs'
    s = read_text(p)
    # The 3Z9AM base does not contain SV1EIA's General->Log page. The RADE
    # tab import can pull its final ResumeLayout/PerformLayout calls because the
    # vendor InitializeComponent shares the same tail. They are not RADE UI and
    # must not create a foreign tab in the SQ4KOU build.
    decl = re.search(r'(?m)^\s*(?:private|public|internal|protected)\s+[^;\r\n]+\btpGeneralLog\s*;', s)
    if not decl and 'this.tpGeneralLog.' in s:
        before = s.count('this.tpGeneralLog.')
        s = '\n'.join(line for line in s.split('\n') if 'this.tpGeneralLog.' not in line)
        write_text(p, s)
        print(f'tpGeneralLog orphan layout refs removed={before}')


def main():
    require(VENDOR.exists(), 'RADE vendor submodule missing')
    vsha = subprocess.check_output(['git', '-C', str(VENDOR), 'rev-parse', 'HEAD'], text=True).strip()
    require(vsha == PIN, 'wrong vendor pin: ' + vsha)

    ensure_reporter()
    clean_orphan_designer_layout()

    transplant('common.cs', 'Common', ['LogEnabled', 'LogNetError'])
    transplant('setup.cs', 'Setup', [
        '_forcingAllEvents', 'UpdateTxMeasureEnabled', 'm_radaeCallUpdating', 'm_radaeGridUpdating'
    ])
    transplant('console.cs', 'Console', [
        'chkRADEMirror', 'cmbRadeVersionRX1Mirror', 'cmbRadeVersionRX2Mirror',
        'chkREPRMirror', 'chkVISMirror', 'chkVISRX2Mirror', 'chkRADERX2Mirror',
        'SetMoxEnabled', 'SetRx1RadeControlVisible', 'SetRx2RadeControlVisible',
        'RadeMeasureRx1', 'RadeMeasureRx2', 'RadeMeasureTx', 'RadaeEooCallsign'
    ])
    transplant('MeterManager.cs', 'MeterManager', ['ContainerHidesWhenRADENotEnabled'])
    transplant('ucMeter.cs', 'ucMeter', ['ContainerHidesWhenRADENotEnabled'])
    transplant('frmMeterDisplay.cs', 'frmMeterDisplay', ['ContainerHidesWhenRADENotEnabled'])

    # Protected 3Z9AM/PTT/EOO ownership is verified by the workflow gate.
    # Do not guess source paths here; this integrator only appends absent RADE
    # dependencies and never replaces existing SQ4KOU members.
    print('SQ4KOU protected-path ownership delegated to workflow gate')
    print('SV1EIA_RADE_DEPENDENCY_CLOSURE=PASS')


if __name__ == '__main__':
    main()
