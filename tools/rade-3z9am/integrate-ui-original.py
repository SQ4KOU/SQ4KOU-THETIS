from pathlib import Path
import re
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[2]
VENDOR = ROOT / 'Project Files' / 'lib' / 'Thetis-RADE-vendor'
CONSOLE = ROOT / 'Project Files' / 'Source' / 'Console'
PIN = '408f2b5232ff0a2aec9b538a40d4cb1b02627b17'
KEEP_RE = re.compile(r'(?i)(radae|\brade\b|freedv|reporter|qsy)')
EOO_DENY_RE = re.compile(r'(?i)(eoo|endofove?r|mox|ptt|pollptt|un-?key|keying)')


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


def brace_end(text, open_pos):
    depth = 0
    i = open_pos
    in_str = False
    verbatim = False
    quote = ''
    escape = False
    while i < len(text):
        ch = text[i]
        if in_str:
            if verbatim:
                if ch == '"' and i + 1 < len(text) and text[i + 1] == '"':
                    i += 2
                    continue
                if ch == quote:
                    in_str = False
            else:
                if escape:
                    escape = False
                elif ch == '\\':
                    escape = True
                elif ch == quote:
                    in_str = False
            i += 1
            continue
        if ch in ('"', "'"):
            in_str = True
            quote = ch
            verbatim = ch == '"' and i > 0 and text[i - 1] == '@'
            i += 1
            continue
        if ch == '{':
            depth += 1
        elif ch == '}':
            depth -= 1
            if depth == 0:
                return i + 1
        i += 1
    raise RuntimeError('unbalanced C# braces')


METHOD_RE = re.compile(
    r'(?m)^(?P<indent>[ \t]*)(?P<prefix>(?:(?:public|private|protected|internal|static|async|virtual|override|sealed|new)\s+)+)'
    r'(?P<ret>[A-Za-z_][\w<>,\.\[\]\? ]*)\s+(?P<name>[A-Za-z_]\w*)\s*\([^;{}]*\)\s*\{')

PROPERTY_RE = re.compile(
    r'(?m)^(?P<indent>[ \t]*)(?P<prefix>(?:(?:public|private|protected|internal|static|virtual|override|new)\s+)+)'
    r'(?P<type>[A-Za-z_][\w<>,\.\[\]\? ]*)\s+(?P<name>[A-Za-z_]\w*)\s*\{')


def members_with_bodies(text, regex):
    out = []
    seen_starts = set()
    for m in regex.finditer(text):
        if m.start() in seen_starts:
            continue
        open_pos = text.find('{', m.start(), m.end())
        try:
            end = brace_end(text, open_pos)
        except RuntimeError:
            continue
        out.append((m.group('name'), m.start(), end, text[m.start():end]))
        seen_starts.add(m.start())
    return out


def find_named_method(text, name):
    for n, s, e, block in members_with_bodies(text, METHOD_RE):
        if n == name:
            return s, e, block
    return None


def class_insert_pos(text):
    # Setup/cmaster/common/console designer files are namespace + one top-level class.
    m = re.search(r'\n[ \t]{4}\}\s*\n\}\s*$', text)
    if m:
        return m.start() + 1
    pos = text.rfind('\n}')
    require(pos >= 0, 'cannot find class end')
    return pos


def insert_class_members(text, blocks):
    blocks = [b.rstrip() for b in blocks if b and b.strip()]
    if not blocks:
        return text
    pos = class_insert_pos(text)
    payload = '\n\n' + '\n\n'.join(blocks) + '\n'
    return text[:pos] + payload + text[pos:]


def copy_reporter_sources():
    src = VENDOR / 'Project Files' / 'Source' / 'Console' / 'FreeDVReporter'
    dst = CONSOLE / 'FreeDVReporter'
    require(src.is_dir(), 'SV1EIA FreeDVReporter directory missing')
    if dst.exists():
        shutil.rmtree(dst)
    shutil.copytree(src, dst)
    names = sorted(p.name for p in dst.glob('*.cs'))
    require(names == ['FreeDVReporterClient.cs', 'FreeDVReporterForm.cs', 'FreeDVReporterManager.cs', 'Maidenhead.cs'],
            'unexpected FreeDVReporter source set: ' + ','.join(names))
    print('FreeDVReporter exact source copy: ' + ', '.join(names))


def patch_designer():
    rel = 'Project Files/Source/Console/setup.designer.cs'
    ref = vendor_text(rel)
    path = CONSOLE / 'setup.designer.cs'
    target = read_text(path)

    if 'this.tpDSPRADE = new' in target:
        print('SV1EIA RADE designer already present; skip reinjection')
        handler_names = set(re.findall(r'this\.([A-Za-z_]\w*(?:Radae|RADAE|RADE|FreeDV|Reporter)\w*)\s*\)', target))
        return handler_names

    ref_init_info = find_named_method(ref, 'InitializeComponent')
    tgt_init_info = find_named_method(target, 'InitializeComponent')
    require(ref_init_info and tgt_init_info, 'InitializeComponent not found')
    _, _, ref_init = ref_init_info
    ts, te, tgt_init = tgt_init_info

    ids = set(re.findall(r'this\.([A-Za-z_]\w*(?:Radae|RADAE|RADE|FreeDV|Reporter)\w*)', ref_init, flags=re.I))
    require('tpDSPRADE' in ids, 'SV1EIA tpDSPRADE not found')

    # Include all children of the RADE tab/groups, even when WinForms gave a generic name.
    changed = True
    adds = re.findall(r'this\.([A-Za-z_]\w*)\.Controls\.Add\(this\.([A-Za-z_]\w*)\)', ref_init)
    while changed:
        changed = False
        for owner, child in adds:
            if owner in ids and child not in ids:
                ids.add(child)
                changed = True

    selected_lines = []
    for line in ref_init.splitlines():
        if any(('this.' + cid) in line for cid in ids):
            selected_lines.append(line)
    require(len(selected_lines) > 40, f'too few RADE designer lines: {len(selected_lines)}')

    # Event handlers referenced by the exact SV1EIA designer must be ported even if
    # their historical WinForms name is generic.
    handlers = set()
    for line in selected_lines:
        if '+=' in line:
            handlers.update(re.findall(r'this\.([A-Za-z_]\w*)\s*\)', line))
            handlers.update(re.findall(r'\+=\s*this\.([A-Za-z_]\w*)', line))

    # Inject RADE initialization just before the form's final ResumeLayout.
    anchor = tgt_init.rfind('this.ResumeLayout(false);')
    if anchor < 0:
        anchor = tgt_init.rfind('}')
    payload = '\n            // SV1EIA RADE Setup -> DSP -> RADE (pinned 408f2b52)\n' + '\n'.join(selected_lines) + '\n'
    tgt_init2 = tgt_init[:anchor] + payload + tgt_init[anchor:]
    target = target[:ts] + tgt_init2 + target[te:]

    # Port the exact field declarations for the transitive RADE control set.
    decls = []
    for line in ref.splitlines():
        if not re.match(r'^\s*(?:private|public|internal|protected)\s+[^;]+;\s*$', line):
            continue
        if any(re.search(r'\b' + re.escape(cid) + r'\b', line) for cid in ids):
            name_match = re.search(r'([A-Za-z_]\w*)\s*;\s*$', line)
            if name_match and not re.search(r'\b' + re.escape(name_match.group(1)) + r'\s*;', target):
                decls.append(line)
    require(len(decls) >= 10, f'too few RADE designer declarations: {len(decls)}')
    target = insert_class_members(target, decls)
    write_text(path, target)
    print(f'SV1EIA designer port: controls={len(ids)} lines={len(selected_lines)} handlers={len(handlers)}')
    return handlers


def patch_setup(handler_names):
    rel = 'Project Files/Source/Console/setup.cs'
    ref = vendor_text(rel)
    path = CONSOLE / 'setup.cs'
    target = read_text(path)

    wanted = []
    for name, _, _, block in members_with_bodies(ref, METHOD_RE):
        if KEEP_RE.search(name) or name in handler_names:
            if EOO_DENY_RE.search(name):
                continue
            if not find_named_method(target, name):
                wanted.append(block)
    require(len(wanted) >= 8, f'too few SV1EIA RADE Setup methods selected: {len(wanted)}')
    target = insert_class_members(target, wanted)

    # ForceAllEvents is a shared Thetis method. Preserve SQ4KOU's version and add
    # only the RADE rehydrate/event lines from SV1EIA.
    ref_force = find_named_method(ref, 'ForceAllEvents')
    tgt_force = find_named_method(target, 'ForceAllEvents')
    if ref_force and tgt_force:
        rs, re_, rblock = ref_force
        ts, te, tblock = tgt_force
        extra = [ln for ln in rblock.splitlines() if KEEP_RE.search(ln)]
        missing = [ln for ln in extra if ln.strip() and ln.strip() not in tblock]
        if missing:
            at = tblock.rfind('}')
            tblock = tblock[:at] + '\n' + '\n'.join(missing) + '\n' + tblock[at:]
            target = target[:ts] + tblock + target[te:]

    write_text(path, target)
    print('SV1EIA setup methods ported: ' + ', '.join(sorted(find_method_name(b) for b in wanted)))


def find_method_name(block):
    m = METHOD_RE.search(block)
    return m.group('name') if m else '?'


def patch_cmaster():
    rel = 'Project Files/Source/Console/cmaster.cs'
    ref = vendor_text(rel)
    path = CONSOLE / 'cmaster.cs'
    target = read_text(path)
    blocks = []

    extern_re = re.compile(
        r'(?ms)^[ \t]*(?:(?:\[[^\r\n]+\])\s*\r?\n[ \t]*)+'
        r'(?:public|internal|private)\s+static\s+extern\s+[^;]+;')
    for m in extern_re.finditer(ref):
        block = m.group(0)
        if not KEEP_RE.search(block):
            continue
        name_m = re.search(r'extern\s+[^\(]+\s+([A-Za-z_]\w*)\s*\(', block)
        if name_m and not re.search(r'\b' + re.escape(name_m.group(1)) + r'\s*\(', target):
            blocks.append(block)

    for name, _, _, block in members_with_bodies(ref, METHOD_RE):
        if KEEP_RE.search(name) and not EOO_DENY_RE.search(name) and not find_named_method(target, name):
            blocks.append(block)

    require(len(blocks) >= 10, f'too few cmaster RADE members: {len(blocks)}')
    target = insert_class_members(target, blocks)
    write_text(path, target)
    print(f'SV1EIA cmaster RADE interop ported: {len(blocks)} members')


def patch_common():
    rel = 'Project Files/Source/Console/common.cs'
    ref = vendor_text(rel)
    path = CONSOLE / 'common.cs'
    target = read_text(path)
    blocks = []
    for name, _, _, block in members_with_bodies(ref, METHOD_RE):
        if KEEP_RE.search(name) and not find_named_method(target, name):
            blocks.append(block)
    for line in ref.splitlines():
        if KEEP_RE.search(line) and re.match(r'^\s*(?:public|private|internal|protected)\s+static\s+[^;]+;\s*$', line):
            name_m = re.search(r'([A-Za-z_]\w*)\s*(?:=[^;]*)?;\s*$', line)
            if name_m and not re.search(r'\b' + re.escape(name_m.group(1)) + r'\b', target):
                blocks.append(line)
    if blocks:
        target = insert_class_members(target, blocks)
        write_text(path, target)
    print(f'SV1EIA Common Reporter support ported: {len(blocks)} members')


def patch_console_named_members():
    rel = 'Project Files/Source/Console/console.cs'
    ref = vendor_text(rel)
    path = CONSOLE / 'console.cs'
    target = read_text(path)
    blocks = []

    existing_methods = {n for n, _, _, _ in members_with_bodies(target, METHOD_RE)}
    existing_props = {n for n, _, _, _ in members_with_bodies(target, PROPERTY_RE)}

    for name, _, _, block in members_with_bodies(ref, METHOD_RE):
        if KEEP_RE.search(name) and not EOO_DENY_RE.search(name) and name not in existing_methods:
            blocks.append(block)
    for name, _, _, block in members_with_bodies(ref, PROPERTY_RE):
        if KEEP_RE.search(name) and not EOO_DENY_RE.search(name) and name not in existing_props:
            blocks.append(block)

    # Only explicitly named RADE/Reporter fields. Never import generic state or PTT/EOO fields.
    for line in ref.splitlines():
        if not KEEP_RE.search(line) or EOO_DENY_RE.search(line):
            continue
        if re.match(r'^\s*(?:public|private|internal|protected)\s+(?:static\s+)?(?:volatile\s+)?[^;{}]+;\s*$', line):
            name_m = re.search(r'([A-Za-z_]\w*)\s*(?:=[^;]*)?;\s*$', line)
            if name_m and KEEP_RE.search(name_m.group(1)) and not re.search(r'\b' + re.escape(name_m.group(1)) + r'\b', target):
                blocks.append(line)

    if blocks:
        target = insert_class_members(target, blocks)
        write_text(path, target)
    print(f'SV1EIA Console named RADE/Reporter support ported: {len(blocks)} members')


def patch_csproj():
    path = CONSOLE / 'Thetis.csproj'
    s = read_text(path)
    if 'FreeDVReporter\\FreeDVReporterClient.cs' in s:
        print('FreeDVReporter already in Thetis.csproj')
        return
    item = '''  <ItemGroup>\n    <Compile Include="FreeDVReporter\\FreeDVReporterClient.cs" />\n    <Compile Include="FreeDVReporter\\FreeDVReporterForm.cs">\n      <SubType>Form</SubType>\n    </Compile>\n    <Compile Include="FreeDVReporter\\FreeDVReporterManager.cs" />\n    <Compile Include="FreeDVReporter\\Maidenhead.cs" />\n  </ItemGroup>\n'''
    require('</Project>' in s, 'Thetis.csproj end missing')
    s = s.replace('</Project>', item + '</Project>', 1)
    write_text(path, s)
    print('FreeDVReporter project includes=PASS')


def validate():
    setup = read_text(CONSOLE / 'setup.cs')
    designer = read_text(CONSOLE / 'setup.designer.cs')
    cmaster = read_text(CONSOLE / 'cmaster.cs')
    project = read_text(CONSOLE / 'Thetis.csproj')

    for token in ['tpDSPRADE', 'chkRX1RadeControl', 'chkRX2RadeControl',
                  'chkRADAEReporter', 'chkRADAEReporting', 'chkRadaeMicRNNoise',
                  'chkRadaeMicAGC', 'chkRadaeMicEQ']:
        require(token in designer, 'missing SV1EIA RADE Setup control: ' + token)

    require('SetRadaeProtocolV2' in setup, 'missing V1/V2 Setup event logic')
    require('chkRX1RadeControl' in setup and 'chkRX2RadeControl' in setup,
            'missing RX1/RX2 Setup event logic')
    for token in ['SetRadaeRxEnabled', 'SetRadaeProtocolV2', 'SetRadaeRxDialScale']:
        require(token in cmaster, 'missing cmaster RADE interop: ' + token)
    require('FreeDVReporter\\FreeDVReporterManager.cs' in project, 'Reporter project include missing')

    ri = read_text(CONSOLE / 'RadeIntegration.cs')
    for token in ['SetRadaeTxSilenceHold(1)', 'RadaeNotifyEndOfOver()', 'GetRadaeEooFlushed()',
                  'RadeEooMarginMs = 300', 'RadeEooTimeoutMs = 3000']:
        require(token in ri, 'Red Pitaya PTT/EOO arbiter damaged: ' + token)

    print('SV1EIA_RADE_SETUP_NAMED_MEMBER_GATE=PASS')
    print('3Z9AM_EOO_PRESERVED=PASS')


def main():
    require(VENDOR.exists(), 'RADE vendor submodule missing')
    vsha = subprocess.check_output(['git', '-C', str(VENDOR), 'rev-parse', 'HEAD'], text=True).strip()
    require(vsha == PIN, 'wrong RADE vendor pin: ' + vsha)
    copy_reporter_sources()
    handlers = patch_designer()
    patch_setup(handlers)
    patch_cmaster()
    patch_common()
    patch_console_named_members()
    patch_csproj()
    validate()
    print('SV1EIA_RADE_SETUP_1TO1_SURFACE=PASS')


if __name__ == '__main__':
    main()
